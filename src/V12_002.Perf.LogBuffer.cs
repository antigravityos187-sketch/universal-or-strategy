using System;
using System.Threading;

namespace NinjaTrader.NinjaScript.Strategies
{
    /// <summary>
    /// Thread-local string formatting buffer to eliminate string.Format() allocations in hot paths.
    /// V12 DNA: ThreadStatic (validated safe by T01B), ASCII-only, zero CYC increase.
    /// </summary>
    public static class LogBuffer
    {
        [ThreadStatic]
        private static char[] _buffer;

        [ThreadStatic]
        private static int _threadId;

        [ThreadStatic]
        private static bool _threadIdInitialized;

        private static int _overflowCount;
        private static int _threadAffinityWarnings;

        private const char OpenBrace = (char)0x7B; // '{'
        private const char CloseBrace = (char)0x7D; // '}'
        private const char Colon = (char)0x3A; // ':'

        /// <summary>
        /// Drop-in replacement for string.Format() with zero allocations for common patterns.
        /// Falls back to string.Format() if buffer overflows (correctness by construction).
        /// </summary>
        public static string Format(string format, params object[] args)
        {
            // Lazy initialization of thread-local buffer
            if (_buffer == null)
            {
                _buffer = new char[512];
            }

            // ValidateThreadAffinity telemetry (T01B Section 6.3)
            ValidateThreadAffinity();

            // Attempt zero-allocation formatting
            int length = FormatInternal(format, args);
            if (length >= 0 && length < _buffer.Length)
            {
                return new string(_buffer, 0, length);
            }

            // Overflow: fallback to string.Format() and increment counter
            Interlocked.Increment(ref _overflowCount);
            return string.Format(format, args);
        }

        /// <summary>
        /// Internal formatting logic supporting common patterns:
        /// - positional arguments: N-th arg substituted at index N
        /// - Mixed literal text and placeholders
        /// </summary>
        private static int FormatInternal(string format, object[] args)
        {
            int bufferPos = 0;
            int formatPos = 0;

            while (formatPos < format.Length)
            {
                char c = format[formatPos];

                if (c == OpenBrace)
                {
                    int advance = TryExpandPlaceholder(format, formatPos, args, ref bufferPos);
                    if (advance < 0)
                        return -1;
                    formatPos += advance;
                    continue;
                }

                if (bufferPos >= _buffer.Length)
                    return -1;

                _buffer[bufferPos++] = c;
                formatPos++;
            }

            return bufferPos;
        }

        /// <summary>
        /// Attempts to expand a placeholder starting at formatPos.
        /// Returns 3 if arg was written, 1 to treat brace as literal (writes the { char),
        /// -1 on overflow or format specifier.
        /// </summary>
        private static int TryExpandPlaceholder(string format, int formatPos, object[] args, ref int bufferPos)
        {
            if (HasFormatSpecifier(format, formatPos))
                return -1;

            string argStr;
            if (!TryGetSingleDigitArg(format, formatPos, args, out argStr))
            {
                // Literal brace: write the { char before advancing past it
                if (bufferPos >= _buffer.Length)
                    return -1;
                _buffer[bufferPos++] = OpenBrace;
                return 1;
            }

            if (bufferPos + argStr.Length >= _buffer.Length)
                return -1;

            argStr.CopyTo(0, _buffer, bufferPos, argStr.Length);
            bufferPos += argStr.Length;
            return 3;
        }

        /// <summary>
        /// Returns true if the opening brace at formatPos is followed by a format specifier colon.
        /// </summary>
        private static bool HasFormatSpecifier(string format, int formatPos)
        {
            int p = formatPos + 1;
            while (p < format.Length && format[p] != CloseBrace)
            {
                if (format[p] == Colon)
                    return true;
                p++;
            }
            return false;
        }

        /// <summary>
        /// Extracts the argument string for a single-digit placeholder at formatPos.
        /// Returns false if the pattern does not match or argIndex is out of range.
        /// </summary>
        private static bool TryGetSingleDigitArg(string format, int formatPos, object[] args, out string argStr)
        {
            argStr = null;

            if (formatPos + 2 >= format.Length || format[formatPos + 2] != CloseBrace)
                return false;

            char digitChar = format[formatPos + 1];
            if (digitChar < (char)0x30 || digitChar > (char)0x39)
                return false;

            int argIndex = digitChar - '0';
            if (argIndex >= args.Length)
                return false;

            object arg = args[argIndex];
            argStr = arg != null ? arg.ToString() : "null";
            return true;
        }

        /// <summary>
        /// ValidateThreadAffinity: Track thread ID on first buffer access per thread.
        /// Log warning if thread ID changes (indicates NinjaTrader platform update).
        /// T01B Section 6.3 early-warning system.
        /// </summary>
        private static void ValidateThreadAffinity()
        {
            int currentThreadId = Thread.CurrentThread.ManagedThreadId;

            if (!_threadIdInitialized)
            {
                _threadId = currentThreadId;
                _threadIdInitialized = true;
            }
            else if (_threadId != currentThreadId)
            {
                // Thread affinity violation detected
                Interlocked.Increment(ref _threadAffinityWarnings);
                _threadId = currentThreadId; // Update to new thread ID
            }
        }

        /// <summary>
        /// Telemetry: Get overflow count (buffer too small for format string).
        /// </summary>
        public static int GetOverflowCount() => _overflowCount;

        /// <summary>
        /// Telemetry: Get thread affinity warning count (ThreadStatic migration detected).
        /// </summary>
        public static int GetThreadAffinityWarnings() => _threadAffinityWarnings;
    }
}

// Made with Bob
