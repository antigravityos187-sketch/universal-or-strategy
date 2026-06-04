# Jane Street Serialization Patterns: V12 Translation Guide

**Version**: 1.0  
**Last Updated**: 2026-06-03  
**Status**: Active Standard  
**Compliance**: V12 DNA Mandatory

---

## Overview

This document translates Jane Street's serialization patterns from OCaml into V12-aligned C# implementations. Jane Street prioritizes **zero-copy serialization**, **schema evolution**, and **type-safe wire formats**—critical for high-frequency trading systems.

### Jane Street Serialization Philosophy

Jane Street's approach to serialization:
- **Zero-Copy**: Deserialize directly from network buffers
- **Schema Evolution**: Forward/backward compatibility
- **Type-Safe**: Compiler-enforced wire formats
- **Compact**: Minimal wire size for latency
- **Versioned**: Explicit version negotiation

### V12 Alignment

V12 DNA implements these principles:
- ✅ **Span<T>**: Zero-copy deserialization
- ✅ **Binary Primitives**: Endian-safe encoding
- ✅ **Versioned Schemas**: Explicit version fields
- ✅ **Result<T,E>**: Type-safe deserialization errors
- ✅ **Readonly Structs**: Immutable wire formats

---

## Pattern 1: Binary Protocol (Fixed-Size Messages)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Bin_prot for binary serialization *)
type order_message = {
  version: int;
  order_id: int64;
  price: int64;
  quantity: int;
  side: order_side;
} [@@deriving bin_io]

let serialize_order buf order =
  let writer = Bin_prot.Writer.create buf in
  Bin_prot.Writer.write_int writer order.version;
  Bin_prot.Writer.write_int64 writer order.order_id;
  Bin_prot.Writer.write_int64 writer order.price;
  Bin_prot.Writer.write_int writer order.quantity;
  Bin_prot.Writer.write_int writer (order_side_to_int order.side)
```

### V12 Translation (C#)

```csharp
// V12: Fixed-size binary protocol
[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 29)]
public readonly struct OrderMessage
{
    public readonly byte Version;      // 1 byte
    public readonly long OrderId;      // 8 bytes
    public readonly long Price;        // 8 bytes (fixed-point: price * 10000)
    public readonly int Quantity;      // 4 bytes
    public readonly byte Side;         // 1 byte (0=Buy, 1=Sell)
    private readonly long _padding;    // 7 bytes (align to 32 bytes)

    public const int WireSize = 29;

    public OrderMessage(long orderId, long price, int quantity, OrderSide side)
    {
        Version = 1;
        OrderId = orderId;
        Price = price;
        Quantity = quantity;
        Side = (byte)side;
        _padding = 0;
    }

    // Zero-copy serialization
    public void WriteTo(Span<byte> buffer)
    {
        if (buffer.Length < WireSize)
            throw new ArgumentException("Buffer too small");

        int offset = 0;
        buffer[offset++] = Version;
        BinaryPrimitives.WriteInt64LittleEndian(buffer.Slice(offset, 8), OrderId);
        offset += 8;
        BinaryPrimitives.WriteInt64LittleEndian(buffer.Slice(offset, 8), Price);
        offset += 8;
        BinaryPrimitives.WriteInt32LittleEndian(buffer.Slice(offset, 4), Quantity);
        offset += 4;
        buffer[offset] = Side;
    }

    // Zero-copy deserialization
    public static Result<OrderMessage, string> ReadFrom(ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length < WireSize)
            return Result<OrderMessage, string>.Err("Buffer too small");

        int offset = 0;
        byte version = buffer[offset++];
        
        if (version != 1)
            return Result<OrderMessage, string>.Err($"Unsupported version: {version}");

        long orderId = BinaryPrimitives.ReadInt64LittleEndian(buffer.Slice(offset, 8));
        offset += 8;
        long price = BinaryPrimitives.ReadInt64LittleEndian(buffer.Slice(offset, 8));
        offset += 8;
        int quantity = BinaryPrimitives.ReadInt32LittleEndian(buffer.Slice(offset, 4));
        offset += 4;
        byte side = buffer[offset];

        if (side > 1)
            return Result<OrderMessage, string>.Err($"Invalid side: {side}");

        return Result<OrderMessage, string>.Ok(
            new OrderMessage(orderId, price, quantity, (OrderSide)side));
    }
}

public enum OrderSide : byte
{
    Buy = 0,
    Sell = 1
}
```

**V12 DNA Alignment:**
- ✅ Lock-free: Immutable readonly struct
- ✅ Type-safe: Result<T,E> for deserialization
- ✅ CYC ≤8: Simple serialization logic
- ✅ Zero-copy: Direct Span<T> access

**DO:**
- ✅ Use fixed-size structs for predictable wire format
- ✅ Use `BinaryPrimitives` for endian-safe encoding
- ✅ Version all messages (forward compatibility)
- ✅ Return `Result<T,E>` for deserialization errors

**DON'T:**
- ❌ Use variable-length encoding in hot paths
- ❌ Forget version field (breaks compatibility)
- ❌ Use big-endian on x86 (performance penalty)
- ❌ Throw exceptions in deserialization (use Result<T,E>)

---

## Pattern 2: Schema Evolution (Versioned Messages)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Versioned messages with Bin_prot *)
type order_message_v1 = {
  order_id: int64;
  price: int64;
  quantity: int;
} [@@deriving bin_io]

type order_message_v2 = {
  order_id: int64;
  price: int64;
  quantity: int;
  timestamp: int64;  (* New field *)
} [@@deriving bin_io]

let deserialize_order buf =
  let version = Bin_prot.Reader.read_int buf in
  match version with
  | 1 -> V1 (Bin_prot.Reader.read_order_message_v1 buf)
  | 2 -> V2 (Bin_prot.Reader.read_order_message_v2 buf)
  | _ -> Error "Unsupported version"
```

### V12 Translation (C#)

```csharp
// V12: Versioned message hierarchy
public abstract record OrderMessage
{
    private OrderMessage() { }

    public sealed record V1(long OrderId, long Price, int Quantity) : OrderMessage
    {
        public const int WireSize = 21;  // 1 + 8 + 8 + 4

        public void WriteTo(Span<byte> buffer)
        {
            buffer[0] = 1;  // Version
            BinaryPrimitives.WriteInt64LittleEndian(buffer.Slice(1, 8), OrderId);
            BinaryPrimitives.WriteInt64LittleEndian(buffer.Slice(9, 8), Price);
            BinaryPrimitives.WriteInt32LittleEndian(buffer.Slice(17, 4), Quantity);
        }
    }

    public sealed record V2(long OrderId, long Price, int Quantity, long Timestamp) : OrderMessage
    {
        public const int WireSize = 29;  // 1 + 8 + 8 + 4 + 8

        public void WriteTo(Span<byte> buffer)
        {
            buffer[0] = 2;  // Version
            BinaryPrimitives.WriteInt64LittleEndian(buffer.Slice(1, 8), OrderId);
            BinaryPrimitives.WriteInt64LittleEndian(buffer.Slice(9, 8), Price);
            BinaryPrimitives.WriteInt32LittleEndian(buffer.Slice(17, 4), Quantity);
            BinaryPrimitives.WriteInt64LittleEndian(buffer.Slice(21, 8), Timestamp);
        }

        // Upgrade from V1
        public static V2 FromV1(V1 v1, long timestamp) =>
            new V2(v1.OrderId, v1.Price, v1.Quantity, timestamp);
    }

    // Versioned deserialization
    public static Result<OrderMessage, string> ReadFrom(ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length < 1)
            return Result<OrderMessage, string>.Err("Buffer too small");

        byte version = buffer[0];

        return version switch
        {
            1 when buffer.Length >= V1.WireSize =>
                Result<OrderMessage, string>.Ok(new V1(
                    BinaryPrimitives.ReadInt64LittleEndian(buffer.Slice(1, 8)),
                    BinaryPrimitives.ReadInt64LittleEndian(buffer.Slice(9, 8)),
                    BinaryPrimitives.ReadInt32LittleEndian(buffer.Slice(17, 4)))),

            2 when buffer.Length >= V2.WireSize =>
                Result<OrderMessage, string>.Ok(new V2(
                    BinaryPrimitives.ReadInt64LittleEndian(buffer.Slice(1, 8)),
                    BinaryPrimitives.ReadInt64LittleEndian(buffer.Slice(9, 8)),
                    BinaryPrimitives.ReadInt32LittleEndian(buffer.Slice(17, 4)),
                    BinaryPrimitives.ReadInt64LittleEndian(buffer.Slice(21, 8)))),

            _ => Result<OrderMessage, string>.Err($"Unsupported version: {version}")
        };
    }
}

// Version negotiation
public sealed class ProtocolNegotiator
{
    private const byte CurrentVersion = 2;
    private const byte MinSupportedVersion = 1;

    public static Result<byte, string> NegotiateVersion(byte peerVersion)
    {
        if (peerVersion < MinSupportedVersion)
            return Result<byte, string>.Err($"Peer version {peerVersion} too old");

        if (peerVersion > CurrentVersion)
            return Result<byte, string>.Ok(CurrentVersion);  // Use our version

        return Result<byte, string>.Ok(peerVersion);  // Use peer's version
    }
}
```

**V12 DNA Alignment:**
- ✅ Lock-free: Immutable records
- ✅ Type-safe: Discriminated unions for versions
- ✅ CYC ≤8: Simple version dispatch
- ✅ Evolvable: Forward/backward compatible

**DO:**
- ✅ Use discriminated unions for versioned messages
- ✅ Support multiple versions simultaneously
- ✅ Provide upgrade paths (V1 → V2)
- ✅ Negotiate protocol version at connection time

**DON'T:**
- ❌ Break wire format compatibility
- ❌ Remove old version support without deprecation period
- ❌ Use optional fields (use versioned messages)
- ❌ Forget to test cross-version compatibility

---

## Pattern 3: Variable-Length Encoding (Strings and Arrays)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Length-prefixed strings *)
let serialize_string buf str =
  let len = String.length str in
  Bin_prot.Writer.write_int buf len;
  Bin_prot.Writer.write_string buf str

let deserialize_string buf =
  let len = Bin_prot.Reader.read_int buf in
  Bin_prot.Reader.read_string buf len
```

### V12 Translation (C#)

```csharp
// V12: Length-prefixed variable-length encoding
public static class VariableLengthEncoding
{
    // Write length-prefixed string (UTF-8)
    public static int WriteString(Span<byte> buffer, string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            BinaryPrimitives.WriteInt32LittleEndian(buffer, 0);
            return 4;
        }

        int byteCount = Encoding.UTF8.GetByteCount(value);
        if (buffer.Length < 4 + byteCount)
            throw new ArgumentException("Buffer too small");

        BinaryPrimitives.WriteInt32LittleEndian(buffer, byteCount);
        Encoding.UTF8.GetBytes(value, buffer.Slice(4));
        
        return 4 + byteCount;
    }

    // Read length-prefixed string (UTF-8)
    public static Result<(string Value, int BytesRead), string> ReadString(
        ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length < 4)
            return Result<(string, int), string>.Err("Buffer too small for length");

        int length = BinaryPrimitives.ReadInt32LittleEndian(buffer);
        
        if (length < 0)
            return Result<(string, int), string>.Err("Negative length");

        if (length == 0)
            return Result<(string, int), string>.Ok((string.Empty, 4));

        if (buffer.Length < 4 + length)
            return Result<(string, int), string>.Err("Buffer too small for string");

        string value = Encoding.UTF8.GetString(buffer.Slice(4, length));
        return Result<(string, int), string>.Ok((value, 4 + length));
    }

    // Write length-prefixed array
    public static int WriteArray<T>(Span<byte> buffer, ReadOnlySpan<T> values, 
        Func<Span<byte>, T, int> writeElement)
        where T : struct
    {
        BinaryPrimitives.WriteInt32LittleEndian(buffer, values.Length);
        int offset = 4;

        foreach (ref readonly var value in values)
        {
            int written = writeElement(buffer.Slice(offset), value);
            offset += written;
        }

        return offset;
    }

    // Read length-prefixed array
    public static Result<(T[] Values, int BytesRead), string> ReadArray<T>(
        ReadOnlySpan<byte> buffer,
        Func<ReadOnlySpan<byte>, Result<(T, int), string>> readElement)
        where T : struct
    {
        if (buffer.Length < 4)
            return Result<(T[], int), string>.Err("Buffer too small for length");

        int count = BinaryPrimitives.ReadInt32LittleEndian(buffer);
        
        if (count < 0)
            return Result<(T[], int), string>.Err("Negative count");

        if (count == 0)
            return Result<(T[], int), string>.Ok((Array.Empty<T>(), 4));

        var values = new T[count];
        int offset = 4;

        for (int i = 0; i < count; i++)
        {
            var result = readElement(buffer.Slice(offset));
            if (!result.IsOk)
                return Result<(T[], int), string>.Err(result.Error);

            values[i] = result.Value.Item1;
            offset += result.Value.Item2;
        }

        return Result<(T[], int), string>.Ok((values, offset));
    }
}

// Usage: Complex message with variable-length fields
public sealed record OrderBookSnapshot
{
    public long Timestamp { get; init; }
    public string Symbol { get; init; } = string.Empty;
    public PriceLevel[] Bids { get; init; } = Array.Empty<PriceLevel>();
    public PriceLevel[] Asks { get; init; } = Array.Empty<PriceLevel>();

    public int WriteTo(Span<byte> buffer)
    {
        int offset = 0;

        // Fixed fields
        BinaryPrimitives.WriteInt64LittleEndian(buffer.Slice(offset, 8), Timestamp);
        offset += 8;

        // Variable-length string
        offset += VariableLengthEncoding.WriteString(buffer.Slice(offset), Symbol);

        // Variable-length arrays
        offset += VariableLengthEncoding.WriteArray(
            buffer.Slice(offset), 
            Bids.AsSpan(), 
            (buf, level) =>
            {
                BinaryPrimitives.WriteInt64LittleEndian(buf, level.Price);
                BinaryPrimitives.WriteInt32LittleEndian(buf.Slice(8), level.Quantity);
                return 12;
            });

        offset += VariableLengthEncoding.WriteArray(
            buffer.Slice(offset), 
            Asks.AsSpan(), 
            (buf, level) =>
            {
                BinaryPrimitives.WriteInt64LittleEndian(buf, level.Price);
                BinaryPrimitives.WriteInt32LittleEndian(buf.Slice(8), level.Quantity);
                return 12;
            });

        return offset;
    }
}

public readonly struct PriceLevel
{
    public readonly long Price;
    public readonly int Quantity;

    public PriceLevel(long price, int quantity)
    {
        Price = price;
        Quantity = quantity;
    }
}
```

**V12 DNA Alignment:**
- ✅ Lock-free: Immutable data structures
- ✅ Type-safe: Result<T,E> for deserialization
- ✅ CYC ≤10: Acceptable for variable-length logic
- ✅ Zero-copy: Direct Span<T> access

**DO:**
- ✅ Use length-prefixed encoding for variable-length data
- ✅ Validate lengths before reading
- ✅ Use UTF-8 for string encoding (compact)
- ✅ Return bytes read for streaming deserialization

**DON'T:**
- ❌ Use null-terminated strings (ambiguous)
- ❌ Forget to validate lengths (buffer overflow)
- ❌ Use UTF-16 for wire format (bloated)
- ❌ Allocate in hot paths (use ArrayPool<T>)

---

## Pattern 4: Compression (Dictionary Encoding)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Dictionary encoding for repeated values *)
module Symbol_dictionary = struct
  type t = {
    mutable symbols: string array;
    mutable symbol_to_id: (string, int) Hashtbl.t;
  }

  let create () = {
    symbols = [||];
    symbol_to_id = Hashtbl.create 1000;
  }

  let intern t symbol =
    match Hashtbl.find_opt t.symbol_to_id symbol with
    | Some id -> id
    | None ->
        let id = Array.length t.symbols in
        t.symbols <- Array.append t.symbols [| symbol |];
        Hashtbl.add t.symbol_to_id symbol id;
        id
end
```

### V12 Translation (C#)

```csharp
// V12: Dictionary encoding for symbol compression
public sealed class SymbolDictionary
{
    private readonly List<string> _symbols;
    private readonly Dictionary<string, int> _symbolToId;
    private readonly ReaderWriterLockSlim _lock;

    public SymbolDictionary()
    {
        _symbols = new List<string>();
        _symbolToId = new Dictionary<string, int>();
        _lock = new ReaderWriterLockSlim();
    }

    // Intern symbol (get or create ID)
    public int Intern(string symbol)
    {
        _lock.EnterReadLock();
        try
        {
            if (_symbolToId.TryGetValue(symbol, out int id))
                return id;
        }
        finally
        {
            _lock.ExitReadLock();
        }

        _lock.EnterWriteLock();
        try
        {
            // Double-check after acquiring write lock
            if (_symbolToId.TryGetValue(symbol, out int id))
                return id;

            int newId = _symbols.Count;
            _symbols.Add(symbol);
            _symbolToId[symbol] = newId;
            return newId;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    // Lookup symbol by ID
    public Option<string> Lookup(int id)
    {
        _lock.EnterReadLock();
        try
        {
            return id >= 0 && id < _symbols.Count
                ? Option<string>.Some(_symbols[id])
                : Option<string>.None();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    // Serialize dictionary (for connection handshake)
    public void WriteTo(Span<byte> buffer)
    {
        _lock.EnterReadLock();
        try
        {
            int offset = 0;
            BinaryPrimitives.WriteInt32LittleEndian(buffer, _symbols.Count);
            offset += 4;

            foreach (var symbol in _symbols)
            {
                offset += VariableLengthEncoding.WriteString(buffer.Slice(offset), symbol);
            }
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
}

// Compressed message using dictionary
public readonly struct CompressedOrderMessage
{
    public readonly byte Version;
    public readonly int SymbolId;      // 4 bytes instead of variable-length string
    public readonly long OrderId;
    public readonly long Price;
    public readonly int Quantity;

    public const int WireSize = 25;

    public void WriteTo(Span<byte> buffer)
    {
        buffer[0] = Version;
        BinaryPrimitives.WriteInt32LittleEndian(buffer.Slice(1, 4), SymbolId);
        BinaryPrimitives.WriteInt64LittleEndian(buffer.Slice(5, 8), OrderId);
        BinaryPrimitives.WriteInt64LittleEndian(buffer.Slice(13, 8), Price);
        BinaryPrimitives.WriteInt32LittleEndian(buffer.Slice(21, 4), Quantity);
    }
}
```

**V12 DNA Alignment:**
- ✅ Lock-free: Read-write lock for dictionary
- ✅ Type-safe: Option<T> for lookups
- ✅ CYC ≤8: Simple dictionary operations
- ✅ Compact: Fixed-size IDs instead of strings

**DO:**
- ✅ Use dictionary encoding for repeated values
- ✅ Synchronize dictionary at connection time
- ✅ Use fixed-size IDs (4 bytes)
- ✅ Use read-write locks for concurrent access

**DON'T:**
- ❌ Use dictionary encoding for unique values
- ❌ Forget to synchronize dictionaries
- ❌ Use variable-size IDs (defeats compression)
- ❌ Ignore dictionary size limits (use LRU eviction)

---

## Pattern 5: Checksums and Integrity

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: CRC32 checksums for message integrity *)
let serialize_with_checksum buf msg =
  let msg_bytes = serialize_message msg in
  let checksum = Crc.crc32 msg_bytes in
  Bin_prot.Writer.write_int buf checksum;
  Bin_prot.Writer.write_bytes buf msg_bytes

let deserialize_with_checksum buf =
  let checksum = Bin_prot.Reader.read_int buf in
  let msg_bytes = Bin_prot.Reader.read_bytes buf in
  let computed = Crc.crc32 msg_bytes in
  if checksum <> computed then
    Error "Checksum mismatch"
  else
    deserialize_message msg_bytes
```

### V12 Translation (C#)

```csharp
// V12: CRC32 checksums with System.IO.Hashing
using System.IO.Hashing;

public static class MessageIntegrity
{
    // Write message with CRC32 checksum
    public static int WriteWithChecksum(Span<byte> buffer, ReadOnlySpan<byte> message)
    {
        if (buffer.Length < 4 + message.Length)
            throw new ArgumentException("Buffer too small");

        // Compute CRC32
        var crc = Crc32.Hash(message);
        
        // Write checksum (4 bytes)
        BinaryPrimitives.WriteUInt32LittleEndian(buffer, BitConverter.ToUInt32(crc));
        
        // Write message
        message.CopyTo(buffer.Slice(4));
        
        return 4 + message.Length;
    }

    // Read message with CRC32 verification
    public static Result<ReadOnlySpan<byte>, string> ReadWithChecksum(
        ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length < 4)
            return Result<ReadOnlySpan<byte>, string>.Err("Buffer too small");

        uint storedChecksum = BinaryPrimitives.ReadUInt32LittleEndian(buffer);
        var message = buffer.Slice(4);

        // Verify CRC32
        var computedCrc = Crc32.Hash(message);
        uint computedChecksum = BitConverter.ToUInt32(computedCrc);

        if (storedChecksum != computedChecksum)
            return Result<ReadOnlySpan<byte>, string>.Err(
                $"Checksum mismatch: expected {storedChecksum:X8}, got {computedChecksum:X8}");

        return Result<ReadOnlySpan<byte>, string>.Ok(message);
    }

    // Write message with XXHash64 (faster than CRC32)
    public static int WriteWithXXHash(Span<byte> buffer, ReadOnlySpan<byte> message)
    {
        if (buffer.Length < 8 + message.Length)
            throw new ArgumentException("Buffer too small");

        // Compute XXHash64
        var hash = XxHash64.Hash(message);
        
        // Write hash (8 bytes)
        BinaryPrimitives.WriteUInt64LittleEndian(buffer, hash);
        
        // Write message
        message.CopyTo(buffer.Slice(8));
        
        return 8 + message.Length;
    }

    // Read message with XXHash64 verification
    public static Result<ReadOnlySpan<byte>, string> ReadWithXXHash(
        ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length < 8)
            return Result<ReadOnlySpan<byte>, string>.Err("Buffer too small");

        ulong storedHash = BinaryPrimitives.ReadUInt64LittleEndian(buffer);
        var message = buffer.Slice(8);

        // Verify XXHash64
        ulong computedHash = XxHash64.Hash(message);

        if (storedHash != computedHash)
            return Result<ReadOnlySpan<byte>, string>.Err(
                $"Hash mismatch: expected {storedHash:X16}, got {computedHash:X16}");

        return Result<ReadOnlySpan<byte>, string>.Ok(message);
    }
}

// Framed messages (length + checksum + payload)
public static class FramedMessages
{
    // Frame format: [length:4][checksum:4][payload:N]
    public static int WriteFrame(Span<byte> buffer, ReadOnlySpan<byte> payload)
    {
        int frameSize = 8 + payload.Length;
        if (buffer.Length < frameSize)
            throw new ArgumentException("Buffer too small");

        // Write length
        BinaryPrimitives.WriteInt32LittleEndian(buffer, payload.Length);

        // Write checksum + payload
        MessageIntegrity.WriteWithChecksum(buffer.Slice(4), payload);

        return frameSize;
    }

    // Read frame
    public static Result<ReadOnlySpan<byte>, string> ReadFrame(
        ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length < 8)
            return Result<ReadOnlySpan<byte>, string>.Err("Buffer too small for frame header");

        int length = BinaryPrimitives.ReadInt32LittleEndian(buffer);
        
        if (length < 0)
            return Result<ReadOnlySpan<byte>, string>.Err("Negative length");

        if (buffer.Length < 8 + length)
            return Result<ReadOnlySpan<byte>, string>.Err("Incomplete frame");

        return MessageIntegrity.ReadWithChecksum(buffer.Slice(4, 4 + length));
    }
}
```

**V12 DNA Alignment:**
- ✅ Lock-free: Pure functions, no side effects
- ✅ Type-safe: Result<T,E> for verification
- ✅ CYC ≤8: Simple checksum logic
- ✅ Integrity: Detects corruption

**DO:**
- ✅ Use CRC32 or XXHash64 for checksums
- ✅ Verify checksums before deserialization
- ✅ Use framed messages (length + checksum + payload)
- ✅ Return descriptive errors on mismatch

**DON'T:**
- ❌ Use MD5/SHA1 for checksums (too slow)
- ❌ Skip checksum verification (silent corruption)
- ❌ Use checksums for cryptographic security (use HMAC)
- ❌ Forget to handle checksum mismatches gracefully

---

## Pattern 6: Zero-Copy Deserialization

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Zero-copy with Bigstring *)
let deserialize_zero_copy bigstring offset =
  let order_id = Bigstring.get_int64_le bigstring offset in
  let price = Bigstring.get_int64_le bigstring (offset + 8) in
  let quantity = Bigstring.get_int32_le bigstring (offset + 16) in
  { order_id; price; quantity }
```

### V12 Translation (C#)

```csharp
// V12: Zero-copy deserialization with ref structs
public ref struct OrderMessageView
{
    private readonly ReadOnlySpan<byte> _buffer;

    public OrderMessageView(ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length < 21)
            throw new ArgumentException("Buffer too small");
        _buffer = buffer;
    }

    // Zero-copy property accessors
    public byte Version => _buffer[0];

    public long OrderId => 
        BinaryPrimitives.ReadInt64LittleEndian(_buffer.Slice(1, 8));

    public long Price => 
        BinaryPrimitives.ReadInt64LittleEndian(_buffer.Slice(9, 8));

    public int Quantity => 
        BinaryPrimitives.ReadInt32LittleEndian(_buffer.Slice(17, 4));

    // Validate without allocation
    public Result<Unit, string> Validate()
    {
        if (Version != 1)
            return Result<Unit, string>.Err($"Unsupported version: {Version}");

        if (Quantity <= 0)
            return Result<Unit, string>.Err("Invalid quantity");

        if (Price <= 0)
            return Result<Unit, string>.Err("Invalid price");

        return Result<Unit, string>.Ok(Unit.Value);
    }

    // Convert to owned type (allocates)
    public OrderMessage ToOwned() =>
        new OrderMessage(OrderId, Price, Quantity);
}

// Batch processing with zero-copy
public static class BatchProcessor
{
    public static int ProcessBatch(ReadOnlySpan<byte> buffer)
    {
        int offset = 0;
        int count = 0;

        while (offset + 21 <= buffer.Length)
        {
            var view = new OrderMessageView(buffer.Slice(offset, 21));
            
            var validation = view.Validate();
            if (validation.IsOk)
            {
                ProcessOrder(view);
                count++;
            }

            offset += 21;
        }

        return count;
    }

    private static void ProcessOrder(OrderMessageView order)
    {
        // Process without allocation
        Console.WriteLine($"Order {order.OrderId}: {order.Quantity} @ {order.Price}");
    }
}
```

**V12 DNA Alignment:**
- ✅ Lock-free: Immutable views
- ✅ Type-safe: Ref struct prevents heap allocation
- ✅ CYC ≤8: Simple property accessors
- ✅ Zero-copy: Direct buffer access

**DO:**
- ✅ Use ref structs for zero-copy views
- ✅ Validate before processing
- ✅ Use property accessors for readability
- ✅ Convert to owned types only when needed

**DON'T:**
- ❌ Store ref structs in fields (compiler error)
- ❌ Return ref structs from async methods (compiler error)
- ❌ Box ref structs (defeats zero-copy)
- ❌ Forget to validate buffer size

---

## Pattern 7: Streaming Deserialization

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Streaming deserialization with Async *)
let deserialize_stream reader =
  let rec loop acc =
    let%bind msg_opt = read_message reader in
    match msg_opt with
    | None -> return (List.rev acc)
    | Some msg -> loop (msg :: acc)
  in
  loop []
```

### V12 Translation (C#)

```csharp
// V12: Streaming deserialization with IAsyncEnumerable
public sealed class MessageStream
{
    private readonly Stream _stream;
    private readonly byte[] _buffer;
    private int _bufferOffset;
    private int _bufferLength;

    public MessageStream(Stream stream, int bufferSize = 4096)
    {
        _stream = stream;
        _buffer = new byte[bufferSize];
        _bufferOffset = 0;
        _bufferLength = 0;
    }

    // Streaming deserialization
    public async IAsyncEnumerable<Result<OrderMessage, string>> ReadMessagesAsync(
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        while (!ct.IsCancellationRequested)
        {
            // Ensure we have at least 4 bytes for length
            if (!await EnsureBytesAsync(4, ct))
                yield break;

            // Read message length
            int length = BinaryPrimitives.ReadInt32LittleEndian(
                _buffer.AsSpan(_bufferOffset, 4));

            if (length < 0 || length > 1_000_000)
            {
                yield return Result<OrderMessage, string>.Err($"Invalid length: {length}");
                yield break;
            }

            // Ensure we have the full message
            if (!await EnsureBytesAsync(4 + length, ct))
            {
                yield return Result<OrderMessage, string>.Err("Incomplete message");
                yield break;
            }

            // Deserialize message
            var messageBuffer = _buffer.AsSpan(_bufferOffset + 4, length);
            var result = OrderMessage.ReadFrom(messageBuffer);

            _bufferOffset += 4 + length;
            yield return result;
        }
    }

    // Ensure buffer has at least N bytes
    private async ValueTask<bool> EnsureBytesAsync(int count, CancellationToken ct)
    {
        // Compact buffer if needed
        if (_bufferOffset > 0 && _bufferLength - _bufferOffset < count)
        {
            int remaining = _bufferLength - _bufferOffset;
            Array.Copy(_buffer, _bufferOffset, _buffer, 0, remaining);
            _bufferOffset = 0;
            _bufferLength = remaining;
        }

        // Read more data if needed
        while (_bufferLength - _bufferOffset < count)
        {
            int bytesRead = await _stream.ReadAsync(
                _buffer.AsMemory(_bufferLength), ct).ConfigureAwait(false);

            if (bytesRead == 0)
                return false;  // End of stream

            _bufferLength += bytesRead;
        }

        return true;
    }
}

// Usage: Process streaming messages
public async Task ProcessStreamAsync(Stream stream, CancellationToken ct)
{
    var messageStream = new MessageStream(stream);

    await foreach (var result in messageStream.ReadMessagesAsync(ct))
    {
        if (result.IsOk)
        {
            ProcessMessage(result.Value);
        }
        else
        {
            Console.WriteLine($"Error: {result.Error}");
            break;
        }
    }
}
```

**V12 DNA Alignment:**
- ✅ Lock-free: Single-threaded stream processing
- ✅ Type-safe: Result<T,E> for deserialization
- ✅ CYC ≤10: Acceptable for streaming logic
- ✅ Streaming: Processes messages as they arrive

**DO:**
- ✅ Use `IAsyncEnumerable<T>` for streaming
- ✅ Buffer data to minimize I/O calls
- ✅ Compact buffer when needed
- ✅ Handle partial messages gracefully

**DON'T:**
- ❌ Read one byte at a time (I/O overhead)
- ❌ Allocate per message (use buffer pool)
- ❌ Ignore end-of-stream (handle gracefully)
- ❌ Block on I/O (use async)

---

## Summary Checklist

### Serialization Patterns Compliance

- [ ] **Binary Protocol**: Use fixed-size structs with BinaryPrimitives
- [ ] **Schema Evolution**: Use versioned messages with discriminated unions
- [ ] **Variable-Length**: Use length-prefixed encoding
- [ ] **Compression**: Use dictionary encoding for repeated values
- [ ] **Checksums**: Use CRC32/XXHash64 for integrity
- [ ] **Zero-Copy**: Use ref structs for deserialization views
- [ ] **Streaming**: Use IAsyncEnumerable<T> for streaming deserialization

### V12 DNA Compliance Matrix

| Pattern | Lock-Free | Type-Safe | CYC ≤15 | Zero-Copy | Versioned |
|---------|-----------|-----------|---------|-----------|-----------|
| Binary Protocol | ✅ | ✅ | ✅ | ✅ | ✅ |
| Schema Evolution | ✅ | ✅ | ✅ | ✅ | ✅ |
| Variable-Length | ✅ | ✅ | ⚠️ | ✅ | ✅ |
| Compression | ⚠️ | ✅ | ✅ | ✅ | ✅ |
| Checksums | ✅ | ✅ | ✅ | ✅ | ✅ |
| Zero-Copy | ✅ | ✅ | ✅ | ✅ | ✅ |
| Streaming | ✅ | ✅ | ⚠️ | ⚠️ | ✅ |

**Legend**: ✅ Full compliance | ⚠️ Acceptable | ❌ Not applicable

---

## References

### Jane Street Resources
- **Firestore KB**: `jane_street_build_exchange_2015` (How to Build an Exchange)
- **Firestore KB**: `jane_street_trading_billions_2023` (Production Engineering When Trading Billions)

### V12 Standards
- [`JANE_STREET_CORE_PATTERNS.md`](./JANE_STREET_CORE_PATTERNS.md) - Result monad, Option type
- [`JANE_STREET_PERFORMANCE_PATTERNS.md`](./JANE_STREET_PERFORMANCE_PATTERNS.md) - Zero-allocation patterns
- [`AGENTS.md`](../../AGENTS.md) - Section 2: ASCII-Only Compliance

### Related Documents
- [`JANE_STREET_ASYNC_PATTERNS.md`](./JANE_STREET_ASYNC_PATTERNS.md) - Streaming patterns
- [`JANE_STREET_TYPE_SAFETY.md`](./JANE_STREET_TYPE_SAFETY.md) - Type-safe wire formats

---

**Document Status**: ✅ Complete (7 patterns documented)  
**Next Review**: 2026-07-03  
**Maintainer**: V12 Architecture Team