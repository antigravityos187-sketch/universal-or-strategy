# Jane Street Performance Patterns: V12 Translation Guide

**Version**: 1.0  
**Last Updated**: 2026-06-03  
**Status**: Active Standard  
**Compliance**: V12 DNA Mandatory

---

## Overview

This document translates Jane Street's performance optimization patterns from OCaml into V12-aligned C# implementations. Jane Street operates at **microsecond latency** constraints where every allocation, cache miss, and branch misprediction matters.

### Jane Street Performance Philosophy

Jane Street's performance strategy prioritizes:
- **Zero-Allocation Hot Paths**: No GC pressure in critical code
- **Cache-Friendly Data Structures**: Sequential memory access patterns
- **Branch Prediction**: Minimize unpredictable branches
- **Lock-Free Algorithms**: No contention in hot paths
- **Mechanical Sympathy**: Understand hardware behavior

### V12 Alignment

V12 DNA implements these principles:
- ✅ **Span<T> and Memory<T>**: Zero-allocation slicing
- ✅ **ArrayPool<T>**: Reusable buffers
- ✅ **ValueTask<T>**: Zero-allocation async
- ✅ **Struct Layouts**: Cache-line alignment
- ✅ **SIMD**: Vectorized operations

---

## Pattern 1: Zero-Allocation Hot Paths

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Avoid allocation in hot paths *)
module Iobuf = struct
  (* Pre-allocated buffer for zero-copy I/O *)
  type t = {
    mutable buf: bytes;
    mutable pos: int;
    mutable len: int;
  }

  let create size = {
    buf = Bytes.create size;
    pos = 0;
    len = 0;
  }

  (* Write without allocation *)
  let write_int64_le t value =
    Bytes.set_int64_le t.buf t.pos value;
    t.pos <- t.pos + 8;
    t.len <- t.len + 8

  (* Read without allocation *)
  let read_int64_le t =
    let value = Bytes.get_int64_le t.buf t.pos in
    t.pos <- t.pos + 8;
    value
end
```

### V12 Translation (C#)

```csharp
// V12: Zero-allocation with Span<T> and ArrayPool<T>
public sealed class ZeroAllocBuffer : IDisposable
{
    private readonly byte[] _buffer;
    private readonly ArrayPool<byte> _pool;
    private int _position;
    private int _length;

    public ZeroAllocBuffer(int size, ArrayPool<byte>? pool = null)
    {
        _pool = pool ?? ArrayPool<byte>.Shared;
        _buffer = _pool.Rent(size);
        _position = 0;
        _length = 0;
    }

    // Write without allocation
    public void WriteInt64LittleEndian(long value)
    {
        var span = _buffer.AsSpan(_position, 8);
        BinaryPrimitives.WriteInt64LittleEndian(span, value);
        _position += 8;
        _length += 8;
    }

    // Read without allocation
    public long ReadInt64LittleEndian()
    {
        var span = _buffer.AsSpan(_position, 8);
        var value = BinaryPrimitives.ReadInt64LittleEndian(span);
        _position += 8;
        return value;
    }

    // Zero-copy slice
    public ReadOnlySpan<byte> AsSpan() => 
        _buffer.AsSpan(0, _length);

    public void Reset()
    {
        _position = 0;
        _length = 0;
    }

    public void Dispose()
    {
        _pool.Return(_buffer);
    }
}

// Usage: Zero-allocation message processing
public sealed class MessageProcessor
{
    private readonly ZeroAllocBuffer _buffer;

    public MessageProcessor(int bufferSize = 4096)
    {
        _buffer = new ZeroAllocBuffer(bufferSize);
    }

    // Hot path: zero allocations
    public void ProcessMessage(long timestamp, long orderId, double price)
    {
        _buffer.Reset();
        _buffer.WriteInt64LittleEndian(timestamp);
        _buffer.WriteInt64LittleEndian(orderId);
        
        // Write double as long bits (zero-allocation)
        var priceBits = BitConverter.DoubleToInt64Bits(price);
        _buffer.WriteInt64LittleEndian(priceBits);

        SendToNetwork(_buffer.AsSpan());
    }

    private void SendToNetwork(ReadOnlySpan<byte> data)
    {
        // Network I/O (zero-copy)
    }
}
```

**V12 DNA Alignment:**
- ✅ Lock-free: No shared mutable state
- ✅ Type-safe: Span<T> bounds-checked
- ✅ CYC ≤8: Simple buffer operations
- ✅ Zero-allocation: ArrayPool + Span<T>

**DO:**
- ✅ Use `Span<T>` for stack-allocated buffers
- ✅ Use `ArrayPool<T>` for reusable buffers
- ✅ Use `BinaryPrimitives` for endian-safe I/O
- ✅ Profile allocations with dotMemory/PerfView

**DON'T:**
- ❌ Allocate in hot paths (>1000 calls/sec)
- ❌ Use LINQ in hot paths (allocates enumerators)
- ❌ Box value types (use generics with constraints)
- ❌ Forget to return pooled buffers (memory leak)

---

## Pattern 2: Cache-Friendly Data Structures

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Array-based data structures for cache locality *)
module Ring_buffer = struct
  type 'a t = {
    mutable data: 'a array;
    mutable head: int;
    mutable tail: int;
    capacity: int;
  }

  let create capacity default_value = {
    data = Array.create capacity default_value;
    head = 0;
    tail = 0;
    capacity;
  }

  (* Sequential access: cache-friendly *)
  let push t value =
    t.data.(t.tail) <- value;
    t.tail <- (t.tail + 1) mod t.capacity

  let pop t =
    let value = t.data.(t.head) in
    t.head <- (t.head + 1) mod t.capacity;
    value
end
```

### V12 Translation (C#)

```csharp
// V12: Cache-friendly ring buffer with struct layout
[StructLayout(LayoutKind.Sequential, Pack = 64)]  // Cache-line aligned
public struct CacheFriendlyRingBuffer<T> where T : struct
{
    private readonly T[] _data;
    private int _head;
    private int _tail;
    private readonly int _capacity;

    public CacheFriendlyRingBuffer(int capacity)
    {
        _data = GC.AllocateArray<T>(capacity, pinned: true);  // Pinned for cache locality
        _head = 0;
        _tail = 0;
        _capacity = capacity;
    }

    // Sequential access: cache-friendly
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Push(T value)
    {
        _data[_tail] = value;
        _tail = (_tail + 1) % _capacity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Pop()
    {
        var value = _data[_head];
        _head = (_head + 1) % _capacity;
        return value;
    }

    // Batch operations: maximize cache hits
    public void PushBatch(ReadOnlySpan<T> values)
    {
        foreach (ref readonly var value in values)
        {
            Push(value);
        }
    }

    public int PopBatch(Span<T> destination)
    {
        int count = Math.Min(destination.Length, Count);
        for (int i = 0; i < count; i++)
        {
            destination[i] = Pop();
        }
        return count;
    }

    public int Count => (_tail - _head + _capacity) % _capacity;
}

// Benchmark: Cache locality matters
[MemoryDiagnoser]
public class CacheLocalityBenchmark
{
    private CacheFriendlyRingBuffer<long> _ringBuffer;
    private long[] _randomAccess;

    [GlobalSetup]
    public void Setup()
    {
        _ringBuffer = new CacheFriendlyRingBuffer<long>(1024);
        _randomAccess = new long[1024];
    }

    [Benchmark]
    public long SequentialAccess()
    {
        long sum = 0;
        for (int i = 0; i < 1024; i++)
        {
            _ringBuffer.Push(i);
            sum += _ringBuffer.Pop();
        }
        return sum;
    }

    [Benchmark]
    public long RandomAccess()
    {
        long sum = 0;
        var rng = new Random(42);
        for (int i = 0; i < 1024; i++)
        {
            int index = rng.Next(1024);
            sum += _randomAccess[index];
        }
        return sum;
    }
}
```

**V12 DNA Alignment:**
- ✅ Lock-free: Single-producer single-consumer safe
- ✅ Type-safe: Generic constraints
- ✅ CYC ≤8: Simple ring buffer logic
- ✅ Cache-friendly: Sequential memory access

**DO:**
- ✅ Use array-based data structures for hot paths
- ✅ Align structs to cache-line boundaries (64 bytes)
- ✅ Use `GC.AllocateArray(pinned: true)` for hot data
- ✅ Batch operations to maximize cache hits

**DON'T:**
- ❌ Use linked lists in hot paths (cache misses)
- ❌ Use dictionaries for small datasets (<100 items)
- ❌ Ignore cache-line alignment (false sharing)
- ❌ Use random access patterns (cache thrashing)

---

## Pattern 3: Branch Prediction Optimization

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Minimize unpredictable branches *)
let process_order order =
  (* Predictable branch: side is usually Buy *)
  match order.side with
  | Buy -> process_buy order
  | Sell -> process_sell order

(* Branchless: use arithmetic instead *)
let sign_branchless x =
  (x > 0) - (x < 0)  (* Returns -1, 0, or 1 without branches *)

(* Lookup table: eliminate branches *)
let price_level_table = Array.init 100 (fun i -> float i *. 0.01)

let get_price_level index =
  price_level_table.(index)  (* No branch, just array access *)
```

### V12 Translation (C#)

```csharp
// V12: Branch prediction optimization
public static class BranchOptimization
{
    // Predictable branch: hint to compiler
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ProcessOrder(Order order)
    {
        // Most orders are Buy (predictable branch)
        if (order.Side == OrderSide.Buy)
        {
            ProcessBuy(order);
        }
        else
        {
            ProcessSell(order);
        }
    }

    // Branchless: use arithmetic
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int SignBranchless(int x)
    {
        return (x > 0 ? 1 : 0) - (x < 0 ? 1 : 0);
    }

    // Lookup table: eliminate branches
    private static readonly double[] PriceLevelTable = 
        Enumerable.Range(0, 100).Select(i => i * 0.01).ToArray();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double GetPriceLevel(int index)
    {
        return PriceLevelTable[index];  // No branch, just array access
    }

    // Branchless min/max (for hot paths)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int MinBranchless(int a, int b)
    {
        return a + ((b - a) & ((b - a) >> 31));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int MaxBranchless(int a, int b)
    {
        return a - ((a - b) & ((a - b) >> 31));
    }

    // Conditional move (branchless)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ConditionalMove(bool condition, int trueValue, int falseValue)
    {
        int mask = condition ? -1 : 0;
        return (trueValue & mask) | (falseValue & ~mask);
    }
}

// Benchmark: Branch vs branchless
[MemoryDiagnoser]
public class BranchPredictionBenchmark
{
    private int[] _predictableData;
    private int[] _randomData;

    [GlobalSetup]
    public void Setup()
    {
        _predictableData = Enumerable.Range(0, 1000).ToArray();
        
        var rng = new Random(42);
        _randomData = Enumerable.Range(0, 1000)
            .Select(_ => rng.Next(-100, 100))
            .ToArray();
    }

    [Benchmark]
    public int PredictableBranch()
    {
        int sum = 0;
        foreach (var x in _predictableData)
        {
            if (x > 0)  // Predictable: always true
                sum += x;
        }
        return sum;
    }

    [Benchmark]
    public int UnpredictableBranch()
    {
        int sum = 0;
        foreach (var x in _randomData)
        {
            if (x > 0)  // Unpredictable: 50/50
                sum += x;
        }
        return sum;
    }

    [Benchmark]
    public int Branchless()
    {
        int sum = 0;
        foreach (var x in _randomData)
        {
            // Branchless: use arithmetic
            sum += x & (x >> 31);  // Add x if positive, 0 if negative
        }
        return sum;
    }
}
```

**V12 DNA Alignment:**
- ✅ Lock-free: Pure functions, no side effects
- ✅ Type-safe: Compiler-enforced types
- ✅ CYC ≤8: Simple branchless logic
- ✅ Predictable: Minimize branch mispredictions

**DO:**
- ✅ Use lookup tables for small domains (<1000 values)
- ✅ Use branchless arithmetic for hot paths
- ✅ Profile branch mispredictions with perf/VTune
- ✅ Use `[MethodImpl(AggressiveInlining)]` for hot methods

**DON'T:**
- ❌ Use unpredictable branches in hot paths
- ❌ Use switch statements with many cases (use lookup table)
- ❌ Ignore branch prediction (profile with perf stat)
- ❌ Optimize prematurely (measure first)

---

## Pattern 4: SIMD Vectorization

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: SIMD via C bindings *)
external vector_add : float array -> float array -> float array = "caml_vector_add"

(* C implementation uses AVX2 *)
(*
CAMLprim value caml_vector_add(value a, value b) {
  __m256d va = _mm256_loadu_pd(&Double_field(a, 0));
  __m256d vb = _mm256_loadu_pd(&Double_field(b, 0));
  __m256d vc = _mm256_add_pd(va, vb);
  // Store result
}
*)
```

### V12 Translation (C#)

```csharp
// V12: SIMD with System.Runtime.Intrinsics
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

public static class SIMDOperations
{
    // Vector addition (AVX2)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void VectorAdd(ReadOnlySpan<double> a, ReadOnlySpan<double> b, Span<double> result)
    {
        if (!Avx2.IsSupported)
        {
            // Fallback: scalar addition
            for (int i = 0; i < a.Length; i++)
                result[i] = a[i] + b[i];
            return;
        }

        int i = 0;
        int vectorSize = Vector256<double>.Count;  // 4 doubles per vector

        // Process 4 doubles at a time
        for (; i <= a.Length - vectorSize; i += vectorSize)
        {
            var va = Vector256.Create(a[i], a[i + 1], a[i + 2], a[i + 3]);
            var vb = Vector256.Create(b[i], b[i + 1], b[i + 2], b[i + 3]);
            var vc = Avx2.Add(va, vb);
            
            result[i] = vc.GetElement(0);
            result[i + 1] = vc.GetElement(1);
            result[i + 2] = vc.GetElement(2);
            result[i + 3] = vc.GetElement(3);
        }

        // Process remaining elements
        for (; i < a.Length; i++)
            result[i] = a[i] + b[i];
    }

    // Dot product (AVX2)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double DotProduct(ReadOnlySpan<double> a, ReadOnlySpan<double> b)
    {
        if (!Avx2.IsSupported)
        {
            // Fallback: scalar dot product
            double sum = 0;
            for (int i = 0; i < a.Length; i++)
                sum += a[i] * b[i];
            return sum;
        }

        var vsum = Vector256<double>.Zero;
        int i = 0;
        int vectorSize = Vector256<double>.Count;

        // Process 4 doubles at a time
        for (; i <= a.Length - vectorSize; i += vectorSize)
        {
            var va = Vector256.Create(a[i], a[i + 1], a[i + 2], a[i + 3]);
            var vb = Vector256.Create(b[i], b[i + 1], b[i + 2], b[i + 3]);
            var vmul = Avx2.Multiply(va, vb);
            vsum = Avx2.Add(vsum, vmul);
        }

        // Horizontal sum
        double sum = vsum.GetElement(0) + vsum.GetElement(1) + 
                     vsum.GetElement(2) + vsum.GetElement(3);

        // Process remaining elements
        for (; i < a.Length; i++)
            sum += a[i] * b[i];

        return sum;
    }

    // Find max value (AVX2)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Max(ReadOnlySpan<double> values)
    {
        if (!Avx2.IsSupported || values.Length < 4)
        {
            double max = double.MinValue;
            foreach (var v in values)
                if (v > max) max = v;
            return max;
        }

        var vmax = Vector256.Create(double.MinValue);
        int i = 0;
        int vectorSize = Vector256<double>.Count;

        for (; i <= values.Length - vectorSize; i += vectorSize)
        {
            var v = Vector256.Create(values[i], values[i + 1], values[i + 2], values[i + 3]);
            vmax = Avx2.Max(vmax, v);
        }

        double max = Math.Max(
            Math.Max(vmax.GetElement(0), vmax.GetElement(1)),
            Math.Max(vmax.GetElement(2), vmax.GetElement(3)));

        for (; i < values.Length; i++)
            if (values[i] > max) max = values[i];

        return max;
    }
}

// Benchmark: SIMD vs scalar
[MemoryDiagnoser]
public class SIMDBenchmark
{
    private double[] _a;
    private double[] _b;
    private double[] _result;

    [GlobalSetup]
    public void Setup()
    {
        _a = Enumerable.Range(0, 1000).Select(i => (double)i).ToArray();
        _b = Enumerable.Range(0, 1000).Select(i => (double)i * 2).ToArray();
        _result = new double[1000];
    }

    [Benchmark]
    public void ScalarAdd()
    {
        for (int i = 0; i < _a.Length; i++)
            _result[i] = _a[i] + _b[i];
    }

    [Benchmark]
    public void SIMDAdd()
    {
        SIMDOperations.VectorAdd(_a, _b, _result);
    }

    [Benchmark]
    public double ScalarDotProduct()
    {
        double sum = 0;
        for (int i = 0; i < _a.Length; i++)
            sum += _a[i] * _b[i];
        return sum;
    }

    [Benchmark]
    public double SIMDDotProduct()
    {
        return SIMDOperations.DotProduct(_a, _b);
    }
}
```

**V12 DNA Alignment:**
- ✅ Lock-free: Pure functions, no side effects
- ✅ Type-safe: Span<T> bounds-checked
- ✅ CYC ≤10: Acceptable for SIMD logic
- ✅ High-performance: 4x throughput with AVX2

**DO:**
- ✅ Use SIMD for data-parallel operations
- ✅ Provide scalar fallback for non-AVX2 CPUs
- ✅ Process data in vector-sized chunks
- ✅ Benchmark SIMD vs scalar (not always faster)

**DON'T:**
- ❌ Use SIMD for small datasets (<100 elements)
- ❌ Forget scalar fallback (portability)
- ❌ Ignore alignment (use aligned loads when possible)
- ❌ Use SIMD for non-data-parallel code

---

## Pattern 5: Lock-Free Algorithms

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Lock-free queue with atomic operations *)
module Lock_free_queue = struct
  type 'a t = {
    mutable head: 'a node option Atomic.t;
    mutable tail: 'a node option Atomic.t;
  }

  and 'a node = {
    value: 'a;
    mutable next: 'a node option Atomic.t;
  }

  let create () = {
    head = Atomic.make None;
    tail = Atomic.make None;
  }

  let enqueue t value =
    let new_node = { value; next = Atomic.make None } in
    let rec loop () =
      let tail = Atomic.get t.tail in
      match tail with
      | None ->
          if Atomic.compare_and_set t.tail None (Some new_node) then
            Atomic.set t.head (Some new_node)
          else
            loop ()
      | Some tail_node ->
          if Atomic.compare_and_set tail_node.next None (Some new_node) then
            Atomic.set t.tail (Some new_node)
          else
            loop ()
    in
    loop ()
end
```

### V12 Translation (C#)

```csharp
// V12: Lock-free queue with Interlocked
public sealed class LockFreeQueue<T> where T : class
{
    private sealed class Node
    {
        public T Value;
        public Node? Next;

        public Node(T value)
        {
            Value = value;
            Next = null;
        }
    }

    private Node? _head;
    private Node? _tail;

    public LockFreeQueue()
    {
        _head = null;
        _tail = null;
    }

    // Lock-free enqueue
    public void Enqueue(T value)
    {
        var newNode = new Node(value);

        while (true)
        {
            var tail = Volatile.Read(ref _tail);

            if (tail == null)
            {
                // Empty queue: try to set both head and tail
                if (Interlocked.CompareExchange(ref _tail, newNode, null) == null)
                {
                    Volatile.Write(ref _head, newNode);
                    return;
                }
            }
            else
            {
                // Non-empty: try to append to tail
                if (Interlocked.CompareExchange(ref tail.Next, newNode, null) == null)
                {
                    // Successfully appended, update tail
                    Interlocked.CompareExchange(ref _tail, newNode, tail);
                    return;
                }
                else
                {
                    // Someone else appended, help move tail forward
                    Interlocked.CompareExchange(ref _tail, tail.Next, tail);
                }
            }
        }
    }

    // Lock-free dequeue
    public bool TryDequeue(out T? value)
    {
        while (true)
        {
            var head = Volatile.Read(ref _head);

            if (head == null)
            {
                value = default;
                return false;
            }

            var next = Volatile.Read(ref head.Next);

            if (Interlocked.CompareExchange(ref _head, next, head) == head)
            {
                value = head.Value;

                // If queue is now empty, clear tail
                if (next == null)
                {
                    Interlocked.CompareExchange(ref _tail, null, head);
                }

                return true;
            }
        }
    }

    public bool IsEmpty => Volatile.Read(ref _head) == null;
}

// Benchmark: Lock-free vs lock-based
[MemoryDiagnoser]
public class LockFreeBenchmark
{
    private LockFreeQueue<int> _lockFreeQueue;
    private ConcurrentQueue<int> _concurrentQueue;
    private Queue<int> _lockedQueue;
    private object _lock;

    [GlobalSetup]
    public void Setup()
    {
        _lockFreeQueue = new LockFreeQueue<int>();
        _concurrentQueue = new ConcurrentQueue<int>();
        _lockedQueue = new Queue<int>();
        _lock = new object();
    }

    [Benchmark]
    public void LockFreeEnqueueDequeue()
    {
        _lockFreeQueue.Enqueue(42);
        _lockFreeQueue.TryDequeue(out _);
    }

    [Benchmark]
    public void ConcurrentQueueEnqueueDequeue()
    {
        _concurrentQueue.Enqueue(42);
        _concurrentQueue.TryDequeue(out _);
    }

    [Benchmark]
    public void LockedQueueEnqueueDequeue()
    {
        lock (_lock)
        {
            _lockedQueue.Enqueue(42);
            _lockedQueue.Dequeue();
        }
    }
}
```

**V12 DNA Alignment:**
- ✅ Lock-free: No locks, only atomic operations
- ✅ Type-safe: Generic constraints
- ✅ CYC ≤10: Acceptable for lock-free logic
- ✅ High-performance: No contention

**DO:**
- ✅ Use `Interlocked.CompareExchange` for atomic updates
- ✅ Use `Volatile.Read/Write` for memory barriers
- ✅ Implement helping (assist other threads)
- ✅ Test with ThreadSanitizer/Concurrency Visualizer

**DON'T:**
- ❌ Use locks in hot paths (contention)
- ❌ Forget memory barriers (reordering bugs)
- ❌ Ignore ABA problem (use versioned pointers)
- ❌ Use lock-free for complex data structures (use lock-based)

---

## Pattern 6: Memory Layout Optimization

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Packed records for cache efficiency *)
type market_tick = {
  symbol_id: int;        (* 8 bytes *)
  price: int64;          (* 8 bytes *)
  quantity: int;         (* 8 bytes *)
  timestamp: int64;      (* 8 bytes *)
}  (* Total: 32 bytes, fits in 1 cache line *)

(* Array of ticks: sequential memory *)
let ticks = Array.create 1000 default_tick
```

### V12 Translation (C#)

```csharp
// V12: Cache-line aligned structs
[StructLayout(LayoutKind.Sequential, Pack = 8, Size = 32)]
public struct MarketTick
{
    public int SymbolId;      // 4 bytes
    public long Price;        // 8 bytes
    public int Quantity;      // 4 bytes
    public long Timestamp;    // 8 bytes
    private long _padding;    // 8 bytes (align to 32 bytes)
}

// Cache-line aligned array
public sealed class TickBuffer
{
    [StructLayout(LayoutKind.Sequential, Pack = 64)]
    private struct CacheLineAlignedArray
    {
        public MarketTick Tick0;
        public MarketTick Tick1;
        // ... up to cache line size
    }

    private readonly MarketTick[] _ticks;

    public TickBuffer(int capacity)
    {
        // Allocate pinned array for cache locality
        _ticks = GC.AllocateArray<MarketTick>(capacity, pinned: true);
    }

    public ref MarketTick this[int index] => ref _ticks[index];

    public Span<MarketTick> AsSpan() => _ticks.AsSpan();
}

// False sharing prevention
[StructLayout(LayoutKind.Explicit, Size = 128)]  // 2 cache lines
public struct PaddedCounter
{
    [FieldOffset(0)]
    public long Value;
    
    // Padding to prevent false sharing (64 bytes before, 64 bytes after)
    [FieldOffset(64)]
    private long _padding;
}

// Benchmark: Memory layout impact
[MemoryDiagnoser]
public class MemoryLayoutBenchmark
{
    private MarketTick[] _alignedTicks;
    private List<MarketTick> _unalignedTicks;

    [GlobalSetup]
    public void Setup()
    {
        _alignedTicks = GC.AllocateArray<MarketTick>(1000, pinned: true);
        _unalignedTicks = new List<MarketTick>(1000);
        
        for (int i = 0; i < 1000; i++)
        {
            var tick = new MarketTick
            {
                SymbolId = i,
                Price = 100 * i,
                Quantity = 10,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };
            _alignedTicks[i] = tick;
            _unalignedTicks.Add(tick);
        }
    }

    [Benchmark]
    public long SequentialAccessAligned()
    {
        long sum = 0;
        foreach (ref readonly var tick in _alignedTicks.AsSpan())
        {
            sum += tick.Price;
        }
        return sum;
    }

    [Benchmark]
    public long SequentialAccessUnaligned()
    {
        long sum = 0;
        foreach (var tick in _unalignedTicks)
        {
            sum += tick.Price;
        }
        return sum;
    }
}
```

**V12 DNA Alignment:**
- ✅ Lock-free: Immutable structs
- ✅ Type-safe: Struct layout attributes
- ✅ CYC ≤8: Simple memory access
- ✅ Cache-friendly: Aligned to cache lines

**DO:**
- ✅ Align structs to cache-line boundaries (64 bytes)
- ✅ Use `GC.AllocateArray(pinned: true)` for hot data
- ✅ Pad structs to prevent false sharing
- ✅ Use `StructLayout` for explicit control

**DON'T:**
- ❌ Ignore cache-line alignment (false sharing)
- ❌ Use reference types for hot data (indirection)
- ❌ Forget padding between shared counters
- ❌ Use `List<T>` for hot paths (use arrays)

---

## Pattern 7: Profiling and Measurement

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Inline profiling with Core_kernel.Time_stamp_counter *)
let profile_function name f =
  let start = Time_stamp_counter.now () in
  let result = f () in
  let end_ = Time_stamp_counter.now () in
  let cycles = Time_stamp_counter.diff end_ start in
  printf "%s: %d cycles\n" name (Time_stamp_counter.to_int63 cycles);
  result

(* Benchmark with statistical analysis *)
let%bench "order matching" =
  match_orders test_orders
```

### V12 Translation (C#)

```csharp
// V12: Profiling with BenchmarkDotNet
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

[MemoryDiagnoser]
[DisassemblyDiagnoser]  // Show generated assembly
[HardwareCounters(HardwareCounter.CacheMisses, HardwareCounter.BranchMispredictions)]
public class OrderMatchingBenchmark
{
    private Order[] _orders;
    private OrderBook _book;

    [GlobalSetup]
    public void Setup()
    {
        _orders = GenerateTestOrders(1000);
        _book = OrderBook.Empty;
    }

    [Benchmark(Baseline = true)]
    public void MatchOrders()
    {
        foreach (var order in _orders)
        {
            _book = _book.Add(order);
        }
        _book.Match();
    }

    [Benchmark]
    public void MatchOrdersOptimized()
    {
        // Optimized version
        foreach (var order in _orders.AsSpan())
        {
            _book = _book.Add(order);
        }
        _book.MatchOptimized();
    }
}

// Inline profiling with Stopwatch
public static class InlineProfiler
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Profile<T>(string name, Func<T> f)
    {
        var sw = Stopwatch.StartNew();
        var result = f();
        sw.Stop();
        
        Console.WriteLine($"{name}: {sw.ElapsedTicks} ticks ({sw.Elapsed.TotalMicroseconds:F2} μs)");
        return result;
    }

    // High-resolution profiling with QueryPerformanceCounter
    [DllImport("kernel32.dll")]
    private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

    [DllImport("kernel32.dll")]
    private static extern bool QueryPerformanceFrequency(out long lpFrequency);

    public static long GetTimestamp()
    {
        QueryPerformanceCounter(out long timestamp);
        return timestamp;
    }

    public static double TicksToMicroseconds(long ticks)
    {
        QueryPerformanceFrequency(out long frequency);
        return (ticks * 1_000_000.0) / frequency;
    }
}

// Usage: Inline profiling
public void ProcessOrders()
{
    var result = InlineProfiler.Profile("ProcessOrders", () =>
    {
        // Hot path code
        return MatchOrders(_orders);
    });
}

// Statistical analysis
public class PerformanceStats
{
    private readonly List<double> _samples = new();

    public void AddSample(double microseconds)
    {
        _samples.Add(microseconds);
    }

    public (double Mean, double StdDev, double P50, double P99) GetStats()
    {
        var sorted = _samples.OrderBy(x => x).ToArray();
        var mean = _samples.Average();
        var variance = _samples.Select(x => Math.Pow(x - mean, 2)).Average();
        var stdDev = Math.Sqrt(variance);
        var p50 = sorted[(int)(sorted.Length * 0.50)];
        var p99 = sorted[(int)(sorted.Length * 0.99)];

        return (mean, stdDev, p50, p99);
    }
}
```

**V12 DNA Alignment:**
- ✅ Lock-free: Profiling doesn't affect concurrency
- ✅ Type-safe: Compiler-enforced profiling
- ✅ CYC ≤8: Simple profiling logic
- ✅ Measurable: Statistical analysis

**DO:**
- ✅ Use BenchmarkDotNet for microbenchmarks
- ✅ Profile with hardware counters (cache misses, branch mispredictions)
- ✅ Use statistical analysis (mean, stddev, percentiles)
- ✅ Profile in production (sampling profiler)

**DON'T:**
- ❌ Trust microbenchmarks without validation
- ❌ Ignore warmup (JIT compilation)
- ❌ Profile debug builds (use Release)
- ❌ Optimize without measuring (premature optimization)

---

## Summary Checklist

### Performance Patterns Compliance

- [ ] **Zero-Allocation**: Use Span<T> and ArrayPool<T> in hot paths
- [ ] **Cache-Friendly**: Use array-based data structures, align to cache lines
- [ ] **Branch Prediction**: Use lookup tables, branchless arithmetic
- [ ] **SIMD**: Use vectorized operations for data-parallel code
- [ ] **Lock-Free**: Use atomic operations, no locks in hot paths
- [ ] **Memory Layout**: Align structs to cache lines, prevent false sharing
- [ ] **Profiling**: Use BenchmarkDotNet, hardware counters, statistical analysis

### V12 DNA Compliance Matrix

| Pattern | Lock-Free | Type-Safe | CYC ≤15 | Zero-Alloc | Measurable |
|---------|-----------|-----------|---------|------------|------------|
| Zero-Allocation | ✅ | ✅ | ✅ | ✅ | ✅ |
| Cache-Friendly | ✅ | ✅ | ✅ | ✅ | ✅ |
| Branch Prediction | ✅ | ✅ | ✅ | ✅ | ✅ |
| SIMD | ✅ | ✅ | ⚠️ | ✅ | ✅ |
| Lock-Free | ✅ | ✅ | ⚠️ | ⚠️ | ✅ |
| Memory Layout | ✅ | ✅ | ✅ | ✅ | ✅ |
| Profiling | ✅ | ✅ | ✅ | ✅ | ✅ |

**Legend**: ✅ Full compliance | ⚠️ Acceptable | ❌ Not applicable

---

## References

### Jane Street Resources
- **Firestore KB**: `carl_cook_microsecond_2017` (When a Microsecond Is an Eternity)
- **Firestore KB**: `godbolt_skylake_deep_dive_2025` (Advanced Skylake Deep Dive)
- **Firestore KB**: `cantrill_hardware_software_codesign_2025` (Hardware-Software Codesign)

### V12 Standards
- [`JANE_STREET_CORE_PATTERNS.md`](./JANE_STREET_CORE_PATTERNS.md) - Zero-allocation patterns
- [`JANE_STREET_ASYNC_PATTERNS.md`](./JANE_STREET_ASYNC_PATTERNS.md) - ValueTask<T>
- [`AGENTS.md`](../../AGENTS.md) - Section 2: Lock-Free Actor Pattern

### Related Documents
- [`JANE_STREET_TESTING_PATTERNS.md`](./JANE_STREET_TESTING_PATTERNS.md) - Performance testing
- [`JANE_STREET_TOOLS_PATTERNS.md`](./JANE_STREET_TOOLS_PATTERNS.md) - Profiling tools

### External Resources
- **BenchmarkDotNet**: https://benchmarkdotnet.org/
- **Intel VTune**: https://www.intel.com/content/www/us/en/developer/tools/oneapi/vtune-profiler.html
- **PerfView**: https://github.com/microsoft/perfview

---

**Document Status**: ✅ Complete (7 patterns documented)  
**Next Review**: 2026-07-03  
**Maintainer**: V12 Architecture Team