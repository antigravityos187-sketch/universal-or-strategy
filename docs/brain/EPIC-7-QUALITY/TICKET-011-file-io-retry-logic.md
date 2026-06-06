# [EPIC-7-QUALITY-011] File I/O Security: Retry Logic

## Priority: P2 MEDIUM

## Labels
`security`, `file-io`, `P2`, `epic-7-quality-phase2`, `resilience`

## Summary
Implement exponential backoff retry logic for transient file I/O failures to improve resilience against temporary filesystem issues.

## Affected Files
- [`src/V12_002.StickyState.cs`](../../src/V12_002.StickyState.cs) - State persistence
- [`src/V12_002.UI.Compliance.cs`](../../src/V12_002.UI.Compliance.cs) - Compliance logging

## Security Impact
- **Severity:** MEDIUM
- **Risk:** Transient file I/O failures can cause:
  - **State loss** - Failed state writes not retried
  - **Compliance gaps** - Missing trade logs for audit trail
  - **False negatives** - Errors attributed to bugs rather than transient failures

## V12 DNA Violation
Violates **Jane Street Resilience**: "Systems must gracefully handle transient failures" - no retry logic for temporary issues.

## Root Cause Analysis
File I/O operations fail immediately on transient errors:
1. **Disk busy** - Antivirus scanning file
2. **Network timeout** - MyDocuments on network share
3. **Lock contention** - Another process has file open
4. **Filesystem lag** - Slow disk I/O

These are **temporary** failures that would succeed on retry, but current code treats them as permanent.

## Remediation Approach

### Phase 1: Retry Helper (Foundation)
Extend `FileSystemHelpers.cs` with retry logic:

```csharp
// V12.EPIC-7-QUALITY-011: Retry logic for transient file I/O failures
public static class FileSystemHelpers
{
    private const int MaxRetries = 3;
    private const int BaseDelayMs = 100;

    /// <summary>
    /// Executes a file operation with exponential backoff retry.
    /// </summary>
    public static T ExecuteWithRetry<T>(Func<T> operation, string operationName)
    {
        int attempt = 0;
        Exception lastException = null;

        while (attempt < MaxRetries)
        {
            try
            {
                return operation();
            }
            catch (IOException ex) when (IsTransientError(ex))
            {
                lastException = ex;
                attempt++;

                if (attempt < MaxRetries)
                {
                    int delayMs = BaseDelayMs * (int)Math.Pow(2, attempt - 1);
                    Print($"[FILE-IO] WARNING: {operationName} failed (attempt {attempt}/{MaxRetries}), retrying in {delayMs}ms: {ex.Message}");
                    Thread.Sleep(delayMs);
                }
            }
        }

        // All retries exhausted
        Print($"[FILE-IO] ERROR: {operationName} failed after {MaxRetries} attempts: {lastException?.Message}");
        throw new IOException($"{operationName} failed after {MaxRetries} retries", lastException);
    }

    /// <summary>
    /// Determines if an IOException is transient (worth retrying).
    /// </summary>
    private static bool IsTransientError(IOException ex)
    {
        // Common transient error codes
        int hResult = ex.HResult;
        return hResult == unchecked((int)0x80070020) || // ERROR_SHARING_VIOLATION
               hResult == unchecked((int)0x80070021) || // ERROR_LOCK_VIOLATION
               hResult == unchecked((int)0x80070050) || // ERROR_FILE_EXISTS (race condition)
               hResult == unchecked((int)0x80070070);   // ERROR_DISK_FULL (may be temporary)
    }

    /// <summary>
    /// Atomically writes content to a file with retry logic.
    /// </summary>
    public static void WriteAllTextAtomicWithRetry(string path, string content)
    {
        ExecuteWithRetry(() =>
        {
            WriteAllTextAtomic(path, content);
            return true; // Success
        }, $"Write to {path}");
    }
}
```

### Phase 2: Apply Retry Logic

**StickyState.cs:**
```csharp
// BEFORE:
File.WriteAllText(tempPath, jsonWithChecksum, Encoding.UTF8);

// AFTER:
FileSystemHelpers.WriteAllTextAtomicWithRetry(_stickyStatePath, jsonWithChecksum);
```

**Compliance.cs:**
```csharp
// BEFORE:
System.IO.File.WriteAllText(_csvPath, _csvHeader + Environment.NewLine);

// AFTER:
FileSystemHelpers.WriteAllTextAtomicWithRetry(_csvPath, _csvHeader + Environment.NewLine);
```

### Phase 3: Retry Metrics
Add metrics to track retry effectiveness:
```csharp
private static int _totalRetries = 0;
private static int _successfulRetries = 0;

public static void LogRetryMetrics()
{
    if (_totalRetries > 0)
    {
        double successRate = (_successfulRetries / (double)_totalRetries) * 100;
        Print($"[FILE-IO] Retry metrics: {_successfulRetries}/{_totalRetries} successful ({successRate:F1}%)");
    }
}
```

## Acceptance Criteria
- [ ] Retry logic implemented in `FileSystemHelpers.cs`
- [ ] Exponential backoff (100ms, 200ms, 400ms)
- [ ] Transient error detection (sharing violation, lock violation)
- [ ] All state persistence operations use retry logic
- [ ] All compliance logging operations use retry logic
- [ ] Retry metrics tracked and logged
- [ ] Build passes with 0 warnings
- [ ] Retry stress test passes (inject transient failures, verify recovery)

## Testing Strategy
1. **Unit Tests**:
   - Mock transient errors (sharing violation, lock violation)
   - Verify exponential backoff timing
   - Verify retry exhaustion after 3 attempts
2. **Integration Tests**:
   - Lock file externally, verify retry succeeds when lock released
   - Simulate disk full, verify retry after space freed
3. **Stress Tests**:
   - 1000 concurrent writes with random transient failures
   - Verify all writes eventually succeed

## Effort Estimate
**4-6 hours**
- 2h: Implement retry logic in `FileSystemHelpers.cs`
- 1h: Apply retry logic to all file I/O operations
- 1h: Add retry metrics and logging
- 1-2h: Testing and verification

## Dependencies
- **TICKET-010** (P1) - Path validation must complete first (retry logic uses validated paths)
- Enhances **TICKET-007** (P1) - State persistence error handling

## Blockers
- None

## References
- Microsoft: [Transient Fault Handling](https://docs.microsoft.com/en-us/azure/architecture/best-practices/transient-faults)
- Polly: [Retry Policies](https://github.com/App-vNext/Polly#retry)
- V12 DNA: [AGENTS.md](../../AGENTS.md) Lines 51-52 (Jane Street Alignment)

## Implementation Notes
- Use `Thread.Sleep` for delays (avoid async in NinjaTrader strategy)
- Consider using `Polly` library for advanced retry policies (if allowed)
- Add circuit breaker pattern if retry rate exceeds threshold
- Log retry attempts at WARNING level, exhaustion at ERROR level

## Jane Street Alignment
This ticket enforces Jane Street's **resilience** principles:
1. **Graceful degradation** - Retry transient failures before failing
2. **Explicit error handling** - Log all retry attempts for forensics
3. **Fail-fast on permanent errors** - Don't retry non-transient errors
4. **Observability** - Track retry metrics for capacity planning

## Status
🔴 **OPEN** - Not Started

## Assigned To
_Unassigned_ (Recommend: Advanced mode)

## Created
2026-05-26T17:09:00Z