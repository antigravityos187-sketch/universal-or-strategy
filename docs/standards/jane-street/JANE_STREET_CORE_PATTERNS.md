# Jane Street Core Patterns: V12 Translation Guide

**Version**: 1.0  
**Last Updated**: 2026-06-03  
**Status**: Active Standard  
**Compliance**: V12 DNA Mandatory

---

## Overview

This document translates Jane Street's core OCaml patterns into V12-aligned C# implementations. Jane Street's approach emphasizes **type safety**, **immutability**, and **making illegal states unrepresentable**—principles that align perfectly with V12 DNA.

### Jane Street Philosophy

Jane Street's OCaml codebase prioritizes:
- **Correctness by Construction**: Type system prevents invalid states at compile time
- **Explicit Error Handling**: No exceptions in hot paths, Result types everywhere
- **Zero-Allocation Hot Paths**: Microsecond-latency constraints demand it
- **Immutability by Default**: Mutable state is explicit and localized
- **Algebraic Data Types**: Sum types (variants) model state machines naturally

### V12 Alignment

V12 DNA shares these principles:
- ✅ **Lock-Free Actor Pattern**: Replaces OCaml's immutable message passing
- ✅ **Result<T,E> Monad**: Replaces OCaml's `Result.t`
- ✅ **Readonly Structs**: Replaces OCaml's immutable records
- ✅ **Discriminated Unions**: Replaces OCaml's variants (via sealed class hierarchies)
- ✅ **CYC ≤15**: Matches Jane Street's cognitive simplicity mandate

---

## Pattern 1: Result Monad (Explicit Error Handling)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Result.t for explicit error handling *)
type order_result = 
  | Ok of order_id
  | Error of order_error

let submit_order order =
  match validate_order order with
  | Error e -> Error e
  | Ok validated ->
      match send_to_exchange validated with
      | Error e -> Error e
      | Ok order_id -> Ok order_id

(* Bind operator for chaining *)
let (>>=) result f =
  match result with
  | Ok x -> f x
  | Error e -> Error e

let submit_order_monadic order =
  validate_order order >>= fun validated ->
  send_to_exchange validated
```

### V12 Translation (C#)

```csharp
// V12: Result<T,E> with explicit error types
public readonly struct Result<T, E>
{
    public readonly bool IsOk;
    public readonly T Value;
    public readonly E Error;

    private Result(bool isOk, T value, E error)
    {
        IsOk = isOk;
        Value = value;
        Error = error;
    }

    public static Result<T, E> Ok(T value) => 
        new Result<T, E>(true, value, default!);
    
    public static Result<T, E> Err(E error) => 
        new Result<T, E>(false, default!, error);

    // Bind operator (monadic composition)
    public Result<U, E> Bind<U>(Func<T, Result<U, E>> f) =>
        IsOk ? f(Value) : Result<U, E>.Err(Error);
}

// Usage: Zero-allocation error handling
public Result<OrderId, OrderError> SubmitOrder(Order order)
{
    return ValidateOrder(order)
        .Bind(validated => SendToExchange(validated));
}

// Pattern matching via switch expression
public string HandleResult(Result<OrderId, OrderError> result) =>
    result.IsOk 
        ? $"Order submitted: {result.Value}"
        : $"Order failed: {result.Error}";
```

**V12 DNA Alignment:**
- ✅ Lock-free: No shared mutable state
- ✅ Type-safe: Compiler enforces error handling
- ✅ CYC ≤8: Simple branching logic
- ✅ Zero-allocation: Readonly struct on stack

**DO:**
- ✅ Use `Result<T,E>` for all fallible operations
- ✅ Chain operations with `Bind` to avoid nested if/else
- ✅ Make error types explicit (no `Exception` in hot path)
- ✅ Use readonly structs to avoid heap allocation

**DON'T:**
- ❌ Throw exceptions in hot paths (microsecond latency killer)
- ❌ Use nullable reference types (`T?`) for error handling
- ❌ Return `bool` with out parameters (loses type safety)
- ❌ Use `try/catch` for control flow

---

## Pattern 2: Discriminated Unions (State Machines)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Variants for state machines *)
type order_state =
  | Pending of { order_id: int; timestamp: float }
  | Filled of { order_id: int; fill_price: float; quantity: int }
  | Cancelled of { order_id: int; reason: string }
  | Rejected of { order_id: int; error: string }

let handle_order_event state event =
  match state, event with
  | Pending p, FillEvent fill -> 
      Filled { order_id = p.order_id; fill_price = fill.price; quantity = fill.qty }
  | Pending p, CancelEvent -> 
      Cancelled { order_id = p.order_id; reason = "User requested" }
  | _ -> state  (* Illegal transitions ignored *)
```

### V12 Translation (C#)

```csharp
// V12: Sealed class hierarchy for discriminated unions
public abstract record OrderState
{
    private OrderState() { }  // Sealed: only nested types allowed

    public sealed record Pending(int OrderId, double Timestamp) : OrderState;
    public sealed record Filled(int OrderId, double FillPrice, int Quantity) : OrderState;
    public sealed record Cancelled(int OrderId, string Reason) : OrderState;
    public sealed record Rejected(int OrderId, string Error) : OrderState;
}

// Pattern matching with switch expression
public OrderState HandleOrderEvent(OrderState state, OrderEvent evt) =>
    (state, evt) switch
    {
        (OrderState.Pending p, FillEvent fill) => 
            new OrderState.Filled(p.OrderId, fill.Price, fill.Quantity),
        
        (OrderState.Pending p, CancelEvent) => 
            new OrderState.Cancelled(p.OrderId, "User requested"),
        
        _ => state  // Illegal transitions: no-op
    };

// Exhaustive pattern matching (compiler-enforced)
public string DescribeState(OrderState state) =>
    state switch
    {
        OrderState.Pending p => $"Pending order {p.OrderId}",
        OrderState.Filled f => $"Filled {f.Quantity} @ {f.FillPrice}",
        OrderState.Cancelled c => $"Cancelled: {c.Reason}",
        OrderState.Rejected r => $"Rejected: {r.Error}",
        _ => throw new InvalidOperationException("Unreachable")
    };
```

**V12 DNA Alignment:**
- ✅ Lock-free: Immutable records, no shared state
- ✅ Type-safe: Compiler enforces exhaustive matching
- ✅ CYC ≤8: Simple switch expressions
- ✅ Correctness by construction: Illegal states unrepresentable

**DO:**
- ✅ Use sealed record hierarchies for sum types
- ✅ Make base class private constructor (sealed hierarchy)
- ✅ Use switch expressions for exhaustive matching
- ✅ Model FSM states as discriminated unions

**DON'T:**
- ❌ Use enums + separate data classes (loses type safety)
- ❌ Use inheritance for behavior (use pattern matching)
- ❌ Allow external types to extend the hierarchy
- ❌ Use nullable fields to represent "no value" states

---

## Pattern 3: Immutable Data Structures

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Immutable records with functional updates *)
type market_data = {
  symbol: string;
  bid: float;
  ask: float;
  last_update: float;
}

let update_bid data new_bid =
  { data with bid = new_bid; last_update = Unix.gettimeofday () }

(* Persistent data structures: sharing unchanged parts *)
let update_multiple_symbols data_map updates =
  List.fold_left 
    (fun acc (symbol, new_bid) ->
      Map.update symbol 
        (Option.map (fun data -> update_bid data new_bid))
        acc)
    data_map
    updates
```

### V12 Translation (C#)

```csharp
// V12: Readonly struct with 'with' expressions
public readonly struct MarketData
{
    public readonly string Symbol { get; init; }
    public readonly double Bid { get; init; }
    public readonly double Ask { get; init; }
    public readonly double LastUpdate { get; init; }

    // Functional update (creates new instance)
    public MarketData WithBid(double newBid) =>
        this with 
        { 
            Bid = newBid, 
            LastUpdate = DateTimeOffset.UtcNow.ToUnixTimeSeconds() 
        };
}

// Persistent collections: ImmutableDictionary
public ImmutableDictionary<string, MarketData> UpdateMultipleSymbols(
    ImmutableDictionary<string, MarketData> dataMap,
    IEnumerable<(string Symbol, double NewBid)> updates)
{
    return updates.Aggregate(
        dataMap,
        (acc, update) => acc.SetItem(
            update.Symbol,
            acc[update.Symbol].WithBid(update.NewBid)
        )
    );
}
```

**V12 DNA Alignment:**
- ✅ Lock-free: No mutable shared state
- ✅ Type-safe: Readonly enforced at compile time
- ✅ CYC ≤8: Simple functional updates
- ✅ Zero-allocation: Struct on stack (for small data)

**DO:**
- ✅ Use readonly structs for small immutable data (<16 bytes)
- ✅ Use `with` expressions for functional updates
- ✅ Use `ImmutableDictionary` for persistent collections
- ✅ Make all fields `readonly` and `init`-only

**DON'T:**
- ❌ Use mutable properties in data structures
- ❌ Use `ref` or `out` parameters to "update" data
- ❌ Copy large structs (use records for >16 bytes)
- ❌ Mutate collections in-place (use persistent collections)

---

## Pattern 4: Option Type (Explicit Absence)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Option.t for nullable values *)
type price_quote = {
  bid: float option;
  ask: float option;
  mid: float option;
}

let calculate_mid quote =
  match quote.bid, quote.ask with
  | Some b, Some a -> Some ((b +. a) /. 2.0)
  | _ -> None

(* Monadic operations *)
let get_spread quote =
  Option.bind quote.ask (fun a ->
    Option.bind quote.bid (fun b ->
      Some (a -. b)))
```

### V12 Translation (C#)

```csharp
// V12: Option<T> struct (no null references)
public readonly struct Option<T>
{
    private readonly bool _hasValue;
    private readonly T _value;

    private Option(bool hasValue, T value)
    {
        _hasValue = hasValue;
        _value = value;
    }

    public static Option<T> Some(T value) => 
        new Option<T>(true, value);
    
    public static Option<T> None() => 
        new Option<T>(false, default!);

    public bool IsSome => _hasValue;
    public bool IsNone => !_hasValue;

    public T ValueOr(T defaultValue) => 
        _hasValue ? _value : defaultValue;

    public Option<U> Bind<U>(Func<T, Option<U>> f) =>
        _hasValue ? f(_value) : Option<U>.None();
}

// Usage: Explicit absence handling
public readonly struct PriceQuote
{
    public readonly Option<double> Bid { get; init; }
    public readonly Option<double> Ask { get; init; }
    public readonly Option<double> Mid { get; init; }
}

public Option<double> CalculateMid(PriceQuote quote) =>
    quote.Bid.Bind(bid =>
        quote.Ask.Bind(ask =>
            Option<double>.Some((bid + ask) / 2.0)));

public Option<double> GetSpread(PriceQuote quote) =>
    quote.Ask.Bind(ask =>
        quote.Bid.Bind(bid =>
            Option<double>.Some(ask - bid)));
```

**V12 DNA Alignment:**
- ✅ Lock-free: No shared mutable state
- ✅ Type-safe: No null reference exceptions
- ✅ CYC ≤8: Simple monadic composition
- ✅ Zero-allocation: Struct on stack

**DO:**
- ✅ Use `Option<T>` instead of nullable reference types
- ✅ Use `Bind` for monadic composition
- ✅ Use `ValueOr` for default values
- ✅ Make absence explicit in type signatures

**DON'T:**
- ❌ Use `null` for "no value" (loses type safety)
- ❌ Use `Nullable<T>` for reference types
- ❌ Throw exceptions for missing values
- ❌ Use sentinel values (e.g., `-1` for "no price")

---

## Pattern 5: Pipeline Operator (Functional Composition)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Pipeline operator for readability *)
let process_order order =
  order
  |> validate_order
  |> enrich_with_market_data
  |> calculate_risk
  |> submit_to_exchange

(* Composition with error handling *)
let process_order_safe order =
  order
  |> validate_order
  >>= enrich_with_market_data
  >>= calculate_risk
  >>= submit_to_exchange
```

### V12 Translation (C#)

```csharp
// V12: Extension methods for pipeline style
public static class PipelineExtensions
{
    // Pipe operator (forward composition)
    public static U Pipe<T, U>(this T value, Func<T, U> f) => f(value);

    // Bind operator for Result<T,E>
    public static Result<U, E> Bind<T, U, E>(
        this Result<T, E> result, 
        Func<T, Result<U, E>> f) =>
        result.IsOk ? f(result.Value) : Result<U, E>.Err(result.Error);
}

// Usage: Readable pipeline
public Result<OrderId, OrderError> ProcessOrder(Order order) =>
    order
        .Pipe(ValidateOrder)
        .Bind(EnrichWithMarketData)
        .Bind(CalculateRisk)
        .Bind(SubmitToExchange);

// Alternative: LINQ query syntax
public Result<OrderId, OrderError> ProcessOrderLinq(Order order) =>
    from validated in ValidateOrder(order)
    from enriched in EnrichWithMarketData(validated)
    from risk in CalculateRisk(enriched)
    from orderId in SubmitToExchange(risk)
    select orderId;
```

**V12 DNA Alignment:**
- ✅ Lock-free: Pure functions, no side effects
- ✅ Type-safe: Compiler enforces type flow
- ✅ CYC ≤8: Linear composition, no branching
- ✅ Readable: Top-to-bottom data flow

**DO:**
- ✅ Use extension methods for pipeline operators
- ✅ Chain operations with `Bind` for error handling
- ✅ Use LINQ query syntax for complex pipelines
- ✅ Keep pipeline stages pure (no side effects)

**DON'T:**
- ❌ Mix side effects in pipeline stages
- ❌ Use pipelines for imperative code (use statements)
- ❌ Nest pipelines deeply (extract to named functions)
- ❌ Ignore errors in pipeline (use `Result<T,E>`)

---

## Pattern 6: Type-Driven Development

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Types as specifications *)
type order_side = Buy | Sell

type order_type = 
  | Market
  | Limit of { price: float }
  | Stop of { trigger_price: float }

type order = {
  side: order_side;
  order_type: order_type;
  quantity: int;
  symbol: string;
}

(* Impossible to create invalid orders *)
let create_limit_order side symbol quantity price =
  if price <= 0.0 then
    Error "Price must be positive"
  else if quantity <= 0 then
    Error "Quantity must be positive"
  else
    Ok {
      side;
      order_type = Limit { price };
      quantity;
      symbol;
    }
```

### V12 Translation (C#)

```csharp
// V12: Smart constructors enforce invariants
public enum OrderSide { Buy, Sell }

public abstract record OrderType
{
    private OrderType() { }
    public sealed record Market : OrderType;
    public sealed record Limit(double Price) : OrderType;
    public sealed record Stop(double TriggerPrice) : OrderType;
}

public sealed class Order
{
    public OrderSide Side { get; }
    public OrderType Type { get; }
    public int Quantity { get; }
    public string Symbol { get; }

    // Private constructor: only factory methods can create
    private Order(OrderSide side, OrderType type, int quantity, string symbol)
    {
        Side = side;
        Type = type;
        Quantity = quantity;
        Symbol = symbol;
    }

    // Smart constructor: enforces invariants
    public static Result<Order, string> CreateLimitOrder(
        OrderSide side, 
        string symbol, 
        int quantity, 
        double price)
    {
        if (price <= 0.0)
            return Result<Order, string>.Err("Price must be positive");
        
        if (quantity <= 0)
            return Result<Order, string>.Err("Quantity must be positive");
        
        if (string.IsNullOrWhiteSpace(symbol))
            return Result<Order, string>.Err("Symbol required");

        return Result<Order, string>.Ok(
            new Order(side, new OrderType.Limit(price), quantity, symbol));
    }
}
```

**V12 DNA Alignment:**
- ✅ Lock-free: Immutable after construction
- ✅ Type-safe: Invalid states unrepresentable
- ✅ CYC ≤8: Simple validation logic
- ✅ Correctness by construction: Smart constructors

**DO:**
- ✅ Use private constructors + factory methods
- ✅ Validate invariants in factory methods
- ✅ Return `Result<T,E>` from factories
- ✅ Make all fields readonly after construction

**DON'T:**
- ❌ Use public constructors for complex types
- ❌ Allow invalid objects to be created
- ❌ Validate in multiple places (centralize in factory)
- ❌ Use setters to "fix" invalid objects

---

## Pattern 7: Zero-Allocation Hot Paths

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

  let write_int64 t value =
    Bytes.set_int64_le t.buf t.pos value;
    t.pos <- t.pos + 8;
    t.len <- t.len + 8
end

(* Reuse buffers across calls *)
let process_messages iobuf messages =
  List.iter (fun msg ->
    Iobuf.write_int64 iobuf msg.timestamp;
    Iobuf.write_int64 iobuf msg.order_id;
    send_to_network iobuf
  ) messages
```

### V12 Translation (C#)

```csharp
// V12: Span<T> and ArrayPool for zero-allocation
public sealed class MessageProcessor
{
    private readonly ArrayPool<byte> _bufferPool;
    private readonly int _bufferSize;

    public MessageProcessor(int bufferSize = 4096)
    {
        _bufferPool = ArrayPool<byte>.Shared;
        _bufferSize = bufferSize;
    }

    // Zero-allocation message processing
    public void ProcessMessages(ReadOnlySpan<Message> messages)
    {
        byte[] buffer = _bufferPool.Rent(_bufferSize);
        try
        {
            Span<byte> span = buffer.AsSpan(0, _bufferSize);
            int pos = 0;

            foreach (ref readonly Message msg in messages)
            {
                // Write directly to span (no allocation)
                BinaryPrimitives.WriteInt64LittleEndian(
                    span.Slice(pos, 8), msg.Timestamp);
                pos += 8;

                BinaryPrimitives.WriteInt64LittleEndian(
                    span.Slice(pos, 8), msg.OrderId);
                pos += 8;

                SendToNetwork(span.Slice(0, pos));
                pos = 0;  // Reset for next message
            }
        }
        finally
        {
            _bufferPool.Return(buffer);
        }
    }
}

// Readonly struct for zero-copy message
public readonly ref struct Message
{
    public readonly long Timestamp { get; init; }
    public readonly long OrderId { get; init; }
}
```

**V12 DNA Alignment:**
- ✅ Lock-free: No shared mutable state
- ✅ Type-safe: Span<T> bounds-checked
- ✅ CYC ≤8: Simple loop logic
- ✅ Zero-allocation: ArrayPool + Span<T>

**DO:**
- ✅ Use `Span<T>` for stack-allocated buffers
- ✅ Use `ArrayPool<T>` for reusable buffers
- ✅ Use `ref readonly` for zero-copy iteration
- ✅ Profile allocations with dotMemory/PerfView

**DON'T:**
- ❌ Allocate in hot paths (>1000 calls/sec)
- ❌ Use LINQ in hot paths (allocates enumerators)
- ❌ Box value types (use generics with constraints)
- ❌ Forget to return pooled buffers (memory leak)

---

## Summary Checklist

### Core Patterns Compliance

- [ ] **Result Monad**: All fallible operations return `Result<T,E>`
- [ ] **Discriminated Unions**: State machines use sealed record hierarchies
- [ ] **Immutable Data**: All data structures are readonly structs/records
- [ ] **Option Type**: No null references, use `Option<T>` for absence
- [ ] **Pipeline Operators**: Functional composition via extension methods
- [ ] **Type-Driven Development**: Smart constructors enforce invariants
- [ ] **Zero-Allocation**: Hot paths use `Span<T>` and `ArrayPool<T>`

### V12 DNA Compliance Matrix

| Pattern | Lock-Free | Type-Safe | CYC ≤15 | ASCII-Only | Zero-Alloc |
|---------|-----------|-----------|---------|------------|------------|
| Result Monad | ✅ | ✅ | ✅ | ✅ | ✅ |
| Discriminated Unions | ✅ | ✅ | ✅ | ✅ | ✅ |
| Immutable Data | ✅ | ✅ | ✅ | ✅ | ✅ |
| Option Type | ✅ | ✅ | ✅ | ✅ | ✅ |
| Pipeline Operators | ✅ | ✅ | ✅ | ✅ | ✅ |
| Type-Driven Dev | ✅ | ✅ | ✅ | ✅ | ✅ |
| Zero-Allocation | ✅ | ✅ | ✅ | ✅ | ✅ |

---

## References

### Jane Street Resources
- **Firestore KB**: `weeks_making_ocaml_safe_2025` (Making OCaml Safe for Performance Engineering)
- **Firestore KB**: `carl_cook_microsecond_2017` (When a Microsecond Is an Eternity)
- **Firestore KB**: `jane_street_trading_billions_2023` (Production Engineering When Trading Billions)

### V12 Standards
- [`REFACTORING_ANTIPATTERNS.md`](../REFACTORING_ANTIPATTERNS.md)
- [`JANE_STREET_DEVIATIONS.md`](../JANE_STREET_DEVIATIONS.md)
- [`AGENTS.md`](../../AGENTS.md) - Section 2: Architectural Mandates

### Related Documents
- [`JANE_STREET_ASYNC_PATTERNS.md`](./JANE_STREET_ASYNC_PATTERNS.md) - Async/await and concurrency
- [`JANE_STREET_FSM_PATTERNS.md`](./JANE_STREET_FSM_PATTERNS.md) - State machine design
- [`JANE_STREET_TESTING_PATTERNS.md`](./JANE_STREET_TESTING_PATTERNS.md) - Property-based testing
- [`JANE_STREET_PERFORMANCE_PATTERNS.md`](./JANE_STREET_PERFORMANCE_PATTERNS.md) - Microsecond optimization

---

**Document Status**: ✅ Complete (7 patterns documented)  
**Next Review**: 2026-07-03  
**Maintainer**: V12 Architecture Team