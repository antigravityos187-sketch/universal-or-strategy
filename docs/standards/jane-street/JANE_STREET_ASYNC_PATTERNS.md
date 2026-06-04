# Jane Street Async Patterns: V12 Translation Guide

**Version**: 1.0  
**Last Updated**: 2026-06-03  
**Status**: Active Standard  
**Compliance**: V12 DNA Mandatory

---

## Overview

This document translates Jane Street's asynchronous programming patterns from OCaml's Async library into V12-aligned C# implementations. Jane Street's approach emphasizes **structured concurrency**, **explicit error propagation**, and **zero-allocation hot paths**—critical for microsecond-latency trading systems.

### Jane Street Async Philosophy

Jane Street's Async library prioritizes:
- **Cooperative Scheduling**: No OS threads in hot paths, lightweight fibers instead
- **Explicit Cancellation**: Cancellation tokens propagate through call chains
- **Structured Concurrency**: Parent tasks own child lifetimes
- **Zero-Allocation**: Async operations reuse pre-allocated state machines
- **Deterministic Execution**: No thread pool randomness, predictable scheduling

### V12 Alignment

V12 DNA adapts these principles:
- ✅ **Actor/FSM Pattern**: Replaces Async's Deferred.t with message queues
- ✅ **ValueTask<T>**: Zero-allocation async for hot paths
- ✅ **CancellationToken**: Explicit cancellation propagation
- ✅ **ConfigureAwait(false)**: No SynchronizationContext overhead
- ✅ **Lock-Free Queues**: Replaces Async's scheduler with SPSC rings

---

## Pattern 1: Deferred Values (Async Primitives)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Deferred.t for async operations *)
open Async

let fetch_market_data symbol =
  let%bind response = Http.get (market_data_url symbol) in
  let%bind parsed = parse_json response in
  return parsed

(* Combinators for parallel execution *)
let fetch_multiple_symbols symbols =
  Deferred.all (List.map ~f:fetch_market_data symbols)

(* Timeout with cancellation *)
let fetch_with_timeout symbol timeout =
  Deferred.any [
    fetch_market_data symbol;
    (Clock.after timeout >>| fun () -> Error "Timeout")
  ]
```

### V12 Translation (C#)

```csharp
// V12: ValueTask<T> for zero-allocation async
public readonly struct MarketDataService
{
    private readonly HttpClient _httpClient;

    // Zero-allocation async method
    public async ValueTask<Result<MarketData, string>> FetchMarketDataAsync(
        string symbol,
        CancellationToken ct = default)
    {
        try
        {
            // ConfigureAwait(false): no SynchronizationContext overhead
            var response = await _httpClient
                .GetStringAsync(MarketDataUrl(symbol), ct)
                .ConfigureAwait(false);

            var parsed = ParseJson(response);
            return Result<MarketData, string>.Ok(parsed);
        }
        catch (HttpRequestException ex)
        {
            return Result<MarketData, string>.Err(ex.Message);
        }
    }

    // Parallel execution with WhenAll
    public async ValueTask<Result<MarketData[], string>> FetchMultipleSymbolsAsync(
        string[] symbols,
        CancellationToken ct = default)
    {
        var tasks = symbols
            .Select(s => FetchMarketDataAsync(s, ct).AsTask())
            .ToArray();

        var results = await Task.WhenAll(tasks).ConfigureAwait(false);

        // Aggregate errors
        var errors = results.Where(r => !r.IsOk).Select(r => r.Error).ToArray();
        if (errors.Length > 0)
            return Result<MarketData[], string>.Err(string.Join("; ", errors));

        return Result<MarketData[], string>.Ok(
            results.Select(r => r.Value).ToArray());
    }

    // Timeout with cancellation
    public async ValueTask<Result<MarketData, string>> FetchWithTimeoutAsync(
        string symbol,
        TimeSpan timeout,
        CancellationToken ct = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(timeout);

        try
        {
            return await FetchMarketDataAsync(symbol, cts.Token)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return Result<MarketData, string>.Err("Timeout");
        }
    }
}
```

**V12 DNA Alignment:**
- ✅ Lock-free: No shared mutable state
- ✅ Type-safe: Result<T,E> for error handling
- ✅ CYC ≤8: Simple async/await flow
- ✅ Zero-allocation: ValueTask<T> reuses state machines

**DO:**
- ✅ Use `ValueTask<T>` for hot-path async methods
- ✅ Always use `ConfigureAwait(false)` in library code
- ✅ Propagate `CancellationToken` through all async calls
- ✅ Return `Result<T,E>` instead of throwing exceptions

**DON'T:**
- ❌ Use `Task<T>` in hot paths (allocates on heap)
- ❌ Forget `ConfigureAwait(false)` (SynchronizationContext overhead)
- ❌ Ignore `CancellationToken` (can't cancel operations)
- ❌ Use `async void` (swallows exceptions)

---

## Pattern 2: Actor Model (Message-Passing Concurrency)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Mailbox-based actors *)
type order_message =
  | Submit of order * (order_result Deferred.t Ivar.t)
  | Cancel of order_id * (unit Deferred.t Ivar.t)
  | Query of order_id * (order_state option Deferred.t Ivar.t)

let order_actor () =
  let mailbox = Mailbox.create () in
  let state = ref OrderMap.empty in
  
  let rec loop () =
    let%bind msg = Mailbox.receive mailbox in
    match msg with
    | Submit (order, reply) ->
        let result = process_order !state order in
        state := update_state !state order result;
        Ivar.fill reply (return result);
        loop ()
    | Cancel (order_id, reply) ->
        state := cancel_order !state order_id;
        Ivar.fill reply (return ());
        loop ()
    | Query (order_id, reply) ->
        let order_state = OrderMap.find_opt order_id !state in
        Ivar.fill reply (return order_state);
        loop ()
  in
  don't_wait_for (loop ());
  mailbox
```

### V12 Translation (C#)

```csharp
// V12: Actor with lock-free SPSC queue
public sealed class OrderActor : IDisposable
{
    private readonly Channel<OrderMessage> _mailbox;
    private readonly Dictionary<int, OrderState> _state;
    private readonly Task _processingLoop;
    private readonly CancellationTokenSource _cts;

    public OrderActor()
    {
        _mailbox = Channel.CreateUnbounded<OrderMessage>(
            new UnboundedChannelOptions
            {
                SingleReader = true,  // Lock-free optimization
                SingleWriter = false
            });
        _state = new Dictionary<int, OrderState>();
        _cts = new CancellationTokenSource();
        _processingLoop = ProcessMessagesAsync(_cts.Token);
    }

    // Message types (discriminated union)
    public abstract record OrderMessage
    {
        private OrderMessage() { }
        
        public sealed record Submit(
            Order Order, 
            TaskCompletionSource<Result<OrderId, OrderError>> Reply) 
            : OrderMessage;
        
        public sealed record Cancel(
            int OrderId, 
            TaskCompletionSource<Result<Unit, string>> Reply) 
            : OrderMessage;
        
        public sealed record Query(
            int OrderId, 
            TaskCompletionSource<Option<OrderState>> Reply) 
            : OrderMessage;
    }

    // Public API: Send messages to actor
    public async ValueTask<Result<OrderId, OrderError>> SubmitOrderAsync(
        Order order,
        CancellationToken ct = default)
    {
        var tcs = new TaskCompletionSource<Result<OrderId, OrderError>>();
        await _mailbox.Writer.WriteAsync(
            new OrderMessage.Submit(order, tcs), ct)
            .ConfigureAwait(false);
        return await tcs.Task.ConfigureAwait(false);
    }

    public async ValueTask<Result<Unit, string>> CancelOrderAsync(
        int orderId,
        CancellationToken ct = default)
    {
        var tcs = new TaskCompletionSource<Result<Unit, string>>();
        await _mailbox.Writer.WriteAsync(
            new OrderMessage.Cancel(orderId, tcs), ct)
            .ConfigureAwait(false);
        return await tcs.Task.ConfigureAwait(false);
    }

    public async ValueTask<Option<OrderState>> QueryOrderAsync(
        int orderId,
        CancellationToken ct = default)
    {
        var tcs = new TaskCompletionSource<Option<OrderState>>();
        await _mailbox.Writer.WriteAsync(
            new OrderMessage.Query(orderId, tcs), ct)
            .ConfigureAwait(false);
        return await tcs.Task.ConfigureAwait(false);
    }

    // Private: Single-threaded message processing loop
    private async Task ProcessMessagesAsync(CancellationToken ct)
    {
        await foreach (var msg in _mailbox.Reader.ReadAllAsync(ct))
        {
            switch (msg)
            {
                case OrderMessage.Submit(var order, var reply):
                    var result = ProcessOrder(order);
                    _state[result.Value.Id] = OrderState.Pending(order);
                    reply.SetResult(result);
                    break;

                case OrderMessage.Cancel(var orderId, var reply):
                    if (_state.Remove(orderId))
                        reply.SetResult(Result<Unit, string>.Ok(Unit.Value));
                    else
                        reply.SetResult(Result<Unit, string>.Err("Order not found"));
                    break;

                case OrderMessage.Query(var orderId, var reply):
                    var state = _state.TryGetValue(orderId, out var s)
                        ? Option<OrderState>.Some(s)
                        : Option<OrderState>.None();
                    reply.SetResult(state);
                    break;
            }
        }
    }

    public void Dispose()
    {
        _mailbox.Writer.Complete();
        _cts.Cancel();
        _processingLoop.Wait();
        _cts.Dispose();
    }
}

// Unit type for void results
public readonly struct Unit
{
    public static readonly Unit Value = default;
}
```

**V12 DNA Alignment:**
- ✅ Lock-free: Single-reader channel (no locks)
- ✅ Type-safe: Discriminated unions for messages
- ✅ CYC ≤8: Simple switch expression
- ✅ Actor pattern: No shared mutable state

**DO:**
- ✅ Use `Channel<T>` with `SingleReader = true` for lock-free queues
- ✅ Use discriminated unions for message types
- ✅ Keep actor state private (encapsulation)
- ✅ Use `TaskCompletionSource<T>` for request-reply

**DON'T:**
- ❌ Access actor state from outside the processing loop
- ❌ Use locks inside the actor (defeats the purpose)
- ❌ Block the processing loop (use async all the way)
- ❌ Forget to dispose actors (memory leak)

---

## Pattern 3: Structured Concurrency (Scoped Parallelism)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Structured concurrency with Monitor.t *)
let process_batch orders =
  Monitor.protect
    ~finally:(fun () -> cleanup_resources ())
    (fun () ->
      let%bind results = 
        Deferred.all (List.map ~f:process_order orders)
      in
      return results)

(* Parallel with early cancellation *)
let process_with_circuit_breaker orders max_failures =
  let failures = ref 0 in
  let%bind results =
    Deferred.List.map orders ~how:`Parallel ~f:(fun order ->
      let%bind result = process_order order in
      match result with
      | Error _ ->
          failures := !failures + 1;
          if !failures >= max_failures then
            raise Circuit_breaker_tripped
          else
            return result
      | Ok _ -> return result)
  in
  return results
```

### V12 Translation (C#)

```csharp
// V12: Structured concurrency with TaskGroup pattern
public sealed class OrderBatchProcessor
{
    // Scoped parallelism with automatic cleanup
    public async ValueTask<Result<OrderResult[], string>> ProcessBatchAsync(
        Order[] orders,
        CancellationToken ct = default)
    {
        // Resource scope: automatically cleaned up
        await using var resources = await AcquireResourcesAsync(ct)
            .ConfigureAwait(false);

        try
        {
            var tasks = orders
                .Select(o => ProcessOrderAsync(o, ct).AsTask())
                .ToArray();

            var results = await Task.WhenAll(tasks).ConfigureAwait(false);

            return Result<OrderResult[], string>.Ok(results);
        }
        catch (Exception ex)
        {
            return Result<OrderResult[], string>.Err(ex.Message);
        }
        // resources.DisposeAsync() called automatically
    }

    // Circuit breaker pattern with early cancellation
    public async ValueTask<Result<OrderResult[], string>> ProcessWithCircuitBreakerAsync(
        Order[] orders,
        int maxFailures,
        CancellationToken ct = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var failures = 0;
        var results = new List<OrderResult>();
        var semaphore = new SemaphoreSlim(1, 1);  // Protect failure counter

        var tasks = orders.Select(async order =>
        {
            try
            {
                var result = await ProcessOrderAsync(order, cts.Token)
                    .ConfigureAwait(false);

                if (!result.IsOk)
                {
                    await semaphore.WaitAsync(cts.Token).ConfigureAwait(false);
                    try
                    {
                        failures++;
                        if (failures >= maxFailures)
                        {
                            cts.Cancel();  // Trip circuit breaker
                            return Result<OrderResult, string>.Err("Circuit breaker tripped");
                        }
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }

                return result;
            }
            catch (OperationCanceledException)
            {
                return Result<OrderResult, string>.Err("Cancelled");
            }
        }).ToArray();

        var allResults = await Task.WhenAll(tasks).ConfigureAwait(false);

        var errors = allResults.Where(r => !r.IsOk).Select(r => r.Error).ToArray();
        if (errors.Length > 0)
            return Result<OrderResult[], string>.Err(string.Join("; ", errors));

        return Result<OrderResult[], string>.Ok(
            allResults.Select(r => r.Value).ToArray());
    }
}

// Resource scope pattern
public sealed class OrderResources : IAsyncDisposable
{
    private readonly HttpClient _httpClient;
    private readonly DatabaseConnection _dbConnection;

    public async ValueTask DisposeAsync()
    {
        await _dbConnection.CloseAsync().ConfigureAwait(false);
        _httpClient.Dispose();
    }
}
```

**V12 DNA Alignment:**
- ✅ Lock-free: Minimal synchronization (SemaphoreSlim for counter only)
- ✅ Type-safe: Result<T,E> for error aggregation
- ✅ CYC ≤10: Acceptable for circuit breaker logic
- ✅ Structured: Resources cleaned up automatically

**DO:**
- ✅ Use `IAsyncDisposable` for resource scopes
- ✅ Use `CancellationTokenSource.CreateLinkedTokenSource` for child cancellation
- ✅ Aggregate errors with `Result<T,E>`
- ✅ Use `Task.WhenAll` for parallel execution

**DON'T:**
- ❌ Forget to link cancellation tokens (orphaned tasks)
- ❌ Use `Task.WaitAll` (blocks thread pool)
- ❌ Ignore exceptions in parallel tasks (silent failures)
- ❌ Leak resources (always use `using` or `await using`)

---

## Pattern 4: Backpressure (Flow Control)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Pipe.t for backpressure *)
let process_stream pipe =
  Pipe.iter pipe ~f:(fun item ->
    let%bind result = process_item item in
    (* Pipe automatically applies backpressure *)
    return ())

(* Bounded queue with backpressure *)
let create_bounded_processor capacity =
  let pipe = Pipe.create ~capacity () in
  let (reader, writer) = pipe in
  
  (* Producer blocks when queue is full *)
  let produce item =
    Pipe.write writer item
  in
  
  (* Consumer processes at its own pace *)
  let consume () =
    Pipe.iter reader ~f:process_item
  in
  
  (produce, consume)
```

### V12 Translation (C#)

```csharp
// V12: Channel<T> with bounded capacity for backpressure
public sealed class BoundedStreamProcessor<T>
{
    private readonly Channel<T> _channel;
    private readonly Func<T, CancellationToken, ValueTask<Result<Unit, string>>> _processor;

    public BoundedStreamProcessor(
        int capacity,
        Func<T, CancellationToken, ValueTask<Result<Unit, string>>> processor)
    {
        _channel = Channel.CreateBounded<T>(
            new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait,  // Backpressure
                SingleReader = true,
                SingleWriter = false
            });
        _processor = processor;
    }

    // Producer: blocks when channel is full (backpressure)
    public async ValueTask<Result<Unit, string>> ProduceAsync(
        T item,
        CancellationToken ct = default)
    {
        try
        {
            await _channel.Writer.WriteAsync(item, ct).ConfigureAwait(false);
            return Result<Unit, string>.Ok(Unit.Value);
        }
        catch (ChannelClosedException)
        {
            return Result<Unit, string>.Err("Channel closed");
        }
    }

    // Consumer: processes at its own pace
    public async Task ConsumeAsync(CancellationToken ct = default)
    {
        await foreach (var item in _channel.Reader.ReadAllAsync(ct))
        {
            var result = await _processor(item, ct).ConfigureAwait(false);
            if (!result.IsOk)
            {
                // Log error but continue processing
                Console.WriteLine($"Processing error: {result.Error}");
            }
        }
    }

    // Signal completion
    public void Complete() => _channel.Writer.Complete();
}

// Usage: Market data stream with backpressure
public sealed class MarketDataStream
{
    private readonly BoundedStreamProcessor<MarketTick> _processor;

    public MarketDataStream(int bufferSize = 1000)
    {
        _processor = new BoundedStreamProcessor<MarketTick>(
            bufferSize,
            ProcessTickAsync);
    }

    private async ValueTask<Result<Unit, string>> ProcessTickAsync(
        MarketTick tick,
        CancellationToken ct)
    {
        // Simulate processing
        await Task.Delay(1, ct).ConfigureAwait(false);
        
        // Update internal state
        UpdateMarketData(tick);
        
        return Result<Unit, string>.Ok(Unit.Value);
    }

    // Producer: blocks if consumer is slow (backpressure)
    public async ValueTask<Result<Unit, string>> PublishTickAsync(
        MarketTick tick,
        CancellationToken ct = default)
    {
        return await _processor.ProduceAsync(tick, ct).ConfigureAwait(false);
    }

    // Consumer: start processing loop
    public Task StartProcessingAsync(CancellationToken ct = default)
    {
        return _processor.ConsumeAsync(ct);
    }
}
```

**V12 DNA Alignment:**
- ✅ Lock-free: Single-reader channel optimization
- ✅ Type-safe: Result<T,E> for error handling
- ✅ CYC ≤8: Simple producer-consumer loop
- ✅ Backpressure: Automatic flow control

**DO:**
- ✅ Use `BoundedChannelFullMode.Wait` for backpressure
- ✅ Set `SingleReader = true` for lock-free optimization
- ✅ Handle `ChannelClosedException` gracefully
- ✅ Use `ReadAllAsync` for consuming channels

**DON'T:**
- ❌ Use unbounded channels for high-throughput streams (memory leak)
- ❌ Use `BoundedChannelFullMode.DropOldest` (data loss)
- ❌ Block the consumer (defeats backpressure)
- ❌ Forget to call `Complete()` (consumer hangs)

---

## Pattern 5: Async Sequences (Lazy Streams)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Pipe.t for lazy async sequences *)
let generate_ticks symbol =
  Pipe.create_reader ~close_on_exception:true (fun writer ->
    let rec loop () =
      let%bind tick = fetch_next_tick symbol in
      let%bind () = Pipe.write writer tick in
      loop ()
    in
    loop ())

(* Transform stream lazily *)
let filter_and_map pipe predicate f =
  Pipe.filter_map pipe ~f:(fun item ->
    if predicate item then
      Some (f item)
    else
      None)
```

### V12 Translation (C#)

```csharp
// V12: IAsyncEnumerable<T> for lazy async sequences
public sealed class TickGenerator
{
    private readonly string _symbol;
    private readonly HttpClient _httpClient;

    // Lazy async sequence: generates ticks on demand
    public async IAsyncEnumerable<MarketTick> GenerateTicksAsync(
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        while (!ct.IsCancellationRequested)
        {
            var tick = await FetchNextTickAsync(_symbol, ct)
                .ConfigureAwait(false);
            
            if (tick.IsOk)
                yield return tick.Value;
            else
                break;  // Stop on error
        }
    }

    // Transform stream lazily (LINQ-style)
    public static async IAsyncEnumerable<U> FilterMapAsync<T, U>(
        IAsyncEnumerable<T> source,
        Func<T, bool> predicate,
        Func<T, U> mapper,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        await foreach (var item in source.WithCancellation(ct))
        {
            if (predicate(item))
                yield return mapper(item);
        }
    }

    // Usage: Lazy pipeline
    public async IAsyncEnumerable<double> GetFilteredPricesAsync(
        string symbol,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var ticks = GenerateTicksAsync(ct);
        
        var filtered = FilterMapAsync(
            ticks,
            tick => tick.Volume > 1000,  // Filter
            tick => tick.Price,          // Map
            ct);

        await foreach (var price in filtered)
        {
            yield return price;
        }
    }
}

// Buffering for batch processing
public static class AsyncEnumerableExtensions
{
    public static async IAsyncEnumerable<T[]> BufferAsync<T>(
        this IAsyncEnumerable<T> source,
        int bufferSize,
        TimeSpan timeout,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var buffer = new List<T>(bufferSize);
        var timer = new PeriodicTimer(timeout);

        await using var enumerator = source.GetAsyncEnumerator(ct);
        
        while (true)
        {
            var hasNext = await enumerator.MoveNextAsync().ConfigureAwait(false);
            
            if (hasNext)
            {
                buffer.Add(enumerator.Current);
                
                if (buffer.Count >= bufferSize)
                {
                    yield return buffer.ToArray();
                    buffer.Clear();
                }
            }
            else if (buffer.Count > 0)
            {
                yield return buffer.ToArray();
                break;
            }
            else
            {
                break;
            }

            // Timeout: flush partial buffer
            if (await timer.WaitForNextTickAsync(ct).ConfigureAwait(false))
            {
                if (buffer.Count > 0)
                {
                    yield return buffer.ToArray();
                    buffer.Clear();
                }
            }
        }
    }
}
```

**V12 DNA Alignment:**
- ✅ Lock-free: No shared mutable state
- ✅ Type-safe: Compiler-enforced async iteration
- ✅ CYC ≤8: Simple yield-based logic
- ✅ Lazy: Items generated on demand

**DO:**
- ✅ Use `IAsyncEnumerable<T>` for lazy async sequences
- ✅ Use `[EnumeratorCancellation]` attribute for cancellation
- ✅ Use `ConfigureAwait(false)` in async iterators
- ✅ Use LINQ-style operators for transformations

**DON'T:**
- ❌ Materialize entire sequence with `ToArrayAsync` (defeats laziness)
- ❌ Forget `WithCancellation(ct)` when consuming
- ❌ Use `yield return` in hot loops (allocation overhead)
- ❌ Block in async iterators (use `await` instead)

---

## Pattern 6: Timeout and Retry (Resilience)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Timeout with Clock.after *)
let with_timeout timeout f =
  Deferred.any [
    f ();
    (Clock.after timeout >>| fun () -> Error "Timeout")
  ]

(* Exponential backoff retry *)
let rec retry_with_backoff ~max_attempts ~base_delay f =
  let rec loop attempt =
    if attempt >= max_attempts then
      return (Error "Max retries exceeded")
    else
      match%bind f () with
      | Ok result -> return (Ok result)
      | Error _ ->
          let delay = Time.Span.scale base_delay (Float.of_int (1 lsl attempt)) in
          let%bind () = Clock.after delay in
          loop (attempt + 1)
  in
  loop 0
```

### V12 Translation (C#)

```csharp
// V12: Timeout and retry with Polly-style patterns
public static class ResilienceExtensions
{
    // Timeout wrapper
    public static async ValueTask<Result<T, string>> WithTimeoutAsync<T>(
        this ValueTask<Result<T, string>> task,
        TimeSpan timeout,
        CancellationToken ct = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(timeout);

        try
        {
            return await task.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return Result<T, string>.Err("Timeout");
        }
    }

    // Exponential backoff retry
    public static async ValueTask<Result<T, string>> RetryWithBackoffAsync<T>(
        Func<CancellationToken, ValueTask<Result<T, string>>> operation,
        int maxAttempts,
        TimeSpan baseDelay,
        CancellationToken ct = default)
    {
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            var result = await operation(ct).ConfigureAwait(false);
            
            if (result.IsOk)
                return result;

            if (attempt < maxAttempts - 1)
            {
                var delay = TimeSpan.FromMilliseconds(
                    baseDelay.TotalMilliseconds * (1 << attempt));
                await Task.Delay(delay, ct).ConfigureAwait(false);
            }
        }

        return Result<T, string>.Err("Max retries exceeded");
    }

    // Combined: retry with timeout per attempt
    public static async ValueTask<Result<T, string>> RetryWithTimeoutAsync<T>(
        Func<CancellationToken, ValueTask<Result<T, string>>> operation,
        int maxAttempts,
        TimeSpan baseDelay,
        TimeSpan timeout,
        CancellationToken ct = default)
    {
        return await RetryWithBackoffAsync(
            async (attemptCt) =>
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(attemptCt);
                cts.CancelAfter(timeout);
                return await operation(cts.Token).ConfigureAwait(false);
            },
            maxAttempts,
            baseDelay,
            ct).ConfigureAwait(false);
    }
}

// Usage: Resilient market data fetch
public sealed class ResilientMarketDataService
{
    private readonly HttpClient _httpClient;

    public async ValueTask<Result<MarketData, string>> FetchWithResilienceAsync(
        string symbol,
        CancellationToken ct = default)
    {
        return await ResilienceExtensions.RetryWithTimeoutAsync(
            async (attemptCt) =>
            {
                var response = await _httpClient
                    .GetStringAsync(MarketDataUrl(symbol), attemptCt)
                    .ConfigureAwait(false);
                
                var parsed = ParseJson(response);
                return Result<MarketData, string>.Ok(parsed);
            },
            maxAttempts: 3,
            baseDelay: TimeSpan.FromMilliseconds(100),
            timeout: TimeSpan.FromSeconds(5),
            ct).ConfigureAwait(false);
    }
}
```

**V12 DNA Alignment:**
- ✅ Lock-free: No shared mutable state
- ✅ Type-safe: Result<T,E> for error handling
- ✅ CYC ≤10: Acceptable for retry logic
- ✅ Resilient: Automatic retry with backoff

**DO:**
- ✅ Use exponential backoff for retries
- ✅ Set per-attempt timeouts (not just global)
- ✅ Return `Result<T,E>` for retry exhaustion
- ✅ Use `CancellationTokenSource.CreateLinkedTokenSource` for nested cancellation

**DON'T:**
- ❌ Use fixed delays (causes thundering herd)
- ❌ Retry indefinitely (resource exhaustion)
- ❌ Ignore cancellation tokens (can't stop retries)
- ❌ Throw exceptions for retry exhaustion (use Result<T,E>)

---

## Pattern 7: Async Coordination (Synchronization Primitives)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Ivar.t for one-time coordination *)
let create_coordinator () =
  let ready = Ivar.create () in
  let wait_for_ready () = Ivar.read ready in
  let signal_ready () = Ivar.fill ready () in
  (wait_for_ready, signal_ready)

(* Throttle: limit concurrent operations *)
let create_throttle max_concurrent =
  let semaphore = Throttle.create ~max_concurrent_jobs:max_concurrent in
  fun f -> Throttle.enqueue semaphore f
```

### V12 Translation (C#)

```csharp
// V12: TaskCompletionSource for one-time coordination
public sealed class AsyncCoordinator
{
    private readonly TaskCompletionSource<bool> _readySignal;

    public AsyncCoordinator()
    {
        _readySignal = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);
    }

    // Wait for signal
    public Task WaitForReadyAsync(CancellationToken ct = default)
    {
        return _readySignal.Task.WaitAsync(ct);
    }

    // Signal once
    public void SignalReady()
    {
        _readySignal.TrySetResult(true);
    }
}

// Throttle: limit concurrent operations
public sealed class AsyncThrottle
{
    private readonly SemaphoreSlim _semaphore;

    public AsyncThrottle(int maxConcurrent)
    {
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    // Execute with throttling
    public async ValueTask<T> ExecuteAsync<T>(
        Func<CancellationToken, ValueTask<T>> operation,
        CancellationToken ct = default)
    {
        await _semaphore.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            return await operation(ct).ConfigureAwait(false);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}

// Usage: Coordinated startup
public sealed class TradingSystem
{
    private readonly AsyncCoordinator _marketDataReady;
    private readonly AsyncCoordinator _riskEngineReady;
    private readonly AsyncThrottle _orderThrottle;

    public TradingSystem()
    {
        _marketDataReady = new AsyncCoordinator();
        _riskEngineReady = new AsyncCoordinator();
        _orderThrottle = new AsyncThrottle(maxConcurrent: 10);
    }

    // Wait for all subsystems
    public async Task WaitForStartupAsync(CancellationToken ct = default)
    {
        await Task.WhenAll(
            _marketDataReady.WaitForReadyAsync(ct),
            _riskEngineReady.WaitForReadyAsync(ct)
        ).ConfigureAwait(false);
    }

    // Submit order with throttling
    public async ValueTask<Result<OrderId, OrderError>> SubmitOrderAsync(
        Order order,
        CancellationToken ct = default)
    {
        return await _orderThrottle.ExecuteAsync(
            async (throttleCt) =>
            {
                // Actual order submission
                return await SendToExchangeAsync(order, throttleCt)
                    .ConfigureAwait(false);
            },
            ct).ConfigureAwait(false);
    }
}
```

**V12 DNA Alignment:**
- ✅ Lock-free: SemaphoreSlim is lock-free for uncontended case
- ✅ Type-safe: Result<T,E> for error handling
- ✅ CYC ≤8: Simple coordination logic
- ✅ Async: No thread blocking

**DO:**
- ✅ Use `TaskCompletionSource` with `RunContinuationsAsynchronously`
- ✅ Use `SemaphoreSlim` for throttling (not `Semaphore`)
- ✅ Always release semaphores in `finally` blocks
- ✅ Use `WaitAsync` with cancellation tokens

**DON'T:**
- ❌ Use `ManualResetEvent` (blocks threads)
- ❌ Use `lock` in async methods (deadlock risk)
- ❌ Forget to release semaphores (resource leak)
- ❌ Use `Task.Wait()` or `.Result` (deadlock risk)

---

## Summary Checklist

### Async Patterns Compliance

- [ ] **Deferred Values**: Use `ValueTask<T>` with `ConfigureAwait(false)`
- [ ] **Actor Model**: Use `Channel<T>` with single-reader optimization
- [ ] **Structured Concurrency**: Use `IAsyncDisposable` for resource scopes
- [ ] **Backpressure**: Use bounded channels with `BoundedChannelFullMode.Wait`
- [ ] **Async Sequences**: Use `IAsyncEnumerable<T>` for lazy streams
- [ ] **Timeout and Retry**: Use exponential backoff with per-attempt timeouts
- [ ] **Async Coordination**: Use `TaskCompletionSource` and `SemaphoreSlim`

### V12 DNA Compliance Matrix

| Pattern | Lock-Free | Type-Safe | CYC ≤15 | Zero-Alloc | Cancellable |
|---------|-----------|-----------|---------|------------|-------------|
| Deferred Values | ✅ | ✅ | ✅ | ✅ | ✅ |
| Actor Model | ✅ | ✅ | ✅ | ⚠️ | ✅ |
| Structured Concurrency | ✅ | ✅ | ⚠️ | ❌ | ✅ |
| Backpressure | ✅ | ✅ | ✅ | ⚠️ | ✅ |
| Async Sequences | ✅ | ✅ | ✅ | ⚠️ | ✅ |
| Timeout/Retry | ✅ | ✅ | ⚠️ | ❌ | ✅ |
| Coordination | ⚠️ | ✅ | ✅ | ❌ | ✅ |

**Legend**: ✅ Full compliance | ⚠️ Acceptable | ❌ Not applicable

---

## References

### Jane Street Resources
- **Firestore KB**: `gjengset_concurrency_coordination_2020` (The Cost of Concurrency Coordination)
- **Firestore KB**: `jane_street_trading_billions_2023` (Production Engineering When Trading Billions)

### V12 Standards
- [`JANE_STREET_CORE_PATTERNS.md`](./JANE_STREET_CORE_PATTERNS.md) - Result monad, Option type
- [`JANE_STREET_FSM_PATTERNS.md`](./JANE_STREET_FSM_PATTERNS.md) - State machine design
- [`AGENTS.md`](../../AGENTS.md) - Section 2: Lock-Free Actor Pattern

### Related Documents
- [`JANE_STREET_PERFORMANCE_PATTERNS.md`](./JANE_STREET_PERFORMANCE_PATTERNS.md) - Zero-allocation techniques
- [`JANE_STREET_TESTING_PATTERNS.md`](./JANE_STREET_TESTING_PATTERNS.md) - Async testing strategies

---

**Document Status**: ✅ Complete (7 patterns documented)  
**Next Review**: 2026-07-03  
**Maintainer**: V12 Architecture Team