# Jane Street Testing Patterns: V12 Translation Guide

**Version**: 1.0  
**Last Updated**: 2026-06-03  
**Status**: Active Standard  
**Compliance**: V12 DNA Mandatory

---

## Overview

This document translates Jane Street's testing patterns from OCaml into V12-aligned C# implementations. Jane Street's approach emphasizes **property-based testing**, **expect tests**, and **inline tests**—strategies that catch bugs traditional unit tests miss.

### Jane Street Testing Philosophy

Jane Street's testing strategy prioritizes:
- **Property-Based Testing**: Generate thousands of random inputs, verify invariants
- **Expect Tests**: Snapshot testing for complex outputs
- **Inline Tests**: Tests live next to code they test
- **Deterministic Randomness**: Reproducible test failures
- **Fast Feedback**: Tests run in <1 second for tight TDD loops

### V12 Alignment

V12 DNA adapts these principles:
- ✅ **FsCheck**: Property-based testing for C#
- ✅ **Verify**: Snapshot testing (expect tests)
- ✅ **xUnit Theory**: Data-driven tests
- ✅ **Deterministic Seeds**: Reproducible random tests
- ✅ **Parallel Execution**: Fast test suites

---

## Pattern 1: Property-Based Testing (Invariants)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Property-based testing with Base_quickcheck *)
open Base_quickcheck

let%test_unit "List.rev is involutive" =
  Quickcheck.test
    ~sexp_of:[%sexp_of: int list]
    (Generator.list Generator.int)
    ~f:(fun xs ->
      [%test_eq: int list] xs (List.rev (List.rev xs)))

let%test_unit "Order book maintains price-time priority" =
  Quickcheck.test
    ~sexp_of:[%sexp_of: order list]
    gen_order_sequence
    ~f:(fun orders ->
      let book = List.fold orders ~init:OrderBook.empty ~f:OrderBook.add in
      assert (OrderBook.is_price_time_sorted book))

(* Custom generators *)
let gen_valid_order =
  Generator.map2
    Generator.int
    Generator.float
    ~f:(fun quantity price ->
      if quantity > 0 && price > 0.0 then
        Some { quantity; price }
      else
        None)
  |> Generator.filter_map ~f:Fn.id
```

### V12 Translation (C#)

```csharp
// V12: Property-based testing with FsCheck
using FsCheck;
using FsCheck.Xunit;

public class OrderBookProperties
{
    // Property: List reverse is involutive
    [Property]
    public Property ReverseIsInvolutive(int[] xs)
    {
        var reversed = xs.Reverse().ToArray();
        var doubleReversed = reversed.Reverse().ToArray();
        return xs.SequenceEqual(doubleReversed).ToProperty();
    }

    // Property: Order book maintains price-time priority
    [Property(Arbitrary = new[] { typeof(OrderGenerators) })]
    public Property OrderBookMaintainsPriceTimePriority(Order[] orders)
    {
        var book = orders.Aggregate(
            OrderBook.Empty,
            (acc, order) => acc.Add(order));

        return book.IsPriceTimeSorted().ToProperty();
    }

    // Property: Adding and removing order is identity
    [Property(Arbitrary = new[] { typeof(OrderGenerators) })]
    public Property AddRemoveIsIdentity(OrderBook book, Order order)
    {
        var withOrder = book.Add(order);
        var withoutOrder = withOrder.Remove(order.OrderId);
        
        return (book == withoutOrder).ToProperty();
    }

    // Property: Order book never has negative quantities
    [Property(Arbitrary = new[] { typeof(OrderGenerators) })]
    public Property NeverNegativeQuantities(Order[] orders)
    {
        var book = orders.Aggregate(OrderBook.Empty, (acc, o) => acc.Add(o));
        
        return book.AllOrders.All(o => o.Quantity > 0).ToProperty();
    }
}

// Custom generators
public static class OrderGenerators
{
    public static Arbitrary<Order> ArbOrder() =>
        Arb.From(
            from quantity in Gen.Choose(1, 1000)
            from price in Gen.Choose(1, 10000).Select(p => p / 100.0)
            from orderId in Gen.Choose(1, int.MaxValue)
            select new Order(orderId, price, quantity));

    public static Arbitrary<OrderBook> ArbOrderBook() =>
        Arb.From(
            from orders in Arb.Generate<Order[]>()
            select orders.Aggregate(OrderBook.Empty, (acc, o) => acc.Add(o)));

    // Generator for valid order sequences (no duplicates)
    public static Arbitrary<Order[]> ArbOrderSequence() =>
        Arb.From(
            from count in Gen.Choose(0, 100)
            from orders in Gen.ArrayOf(count, ArbOrder().Generator)
            let uniqueOrders = orders.DistinctBy(o => o.OrderId).ToArray()
            select uniqueOrders);
}
```

**V12 DNA Alignment:**
- ✅ Lock-free: Pure functions, no side effects
- ✅ Type-safe: Compiler-enforced properties
- ✅ CYC ≤8: Simple property checks
- ✅ Exhaustive: Tests thousands of random inputs

**DO:**
- ✅ Use FsCheck for property-based testing
- ✅ Test invariants, not specific values
- ✅ Create custom generators for domain types
- ✅ Use `[Property]` attribute for xUnit integration

**DON'T:**
- ❌ Only test happy paths (use property-based)
- ❌ Hardcode test data (use generators)
- ❌ Ignore shrinking (FsCheck finds minimal failing case)
- ❌ Test implementation details (test properties)

---

## Pattern 2: Expect Tests (Snapshot Testing)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Expect tests with ppx_expect *)
let%expect_test "order book pretty print" =
  let book = OrderBook.empty
    |> OrderBook.add { order_id = 1; price = 100.0; quantity = 10 }
    |> OrderBook.add { order_id = 2; price = 100.5; quantity = 5 }
  in
  print_s [%sexp (book : OrderBook.t)];
  [%expect {|
    ((bids ((100.0 ((order_id 1) (quantity 10)))))
     (asks ((100.5 ((order_id 2) (quantity 5))))))
  |}]

let%expect_test "FSM state transitions" =
  let state = OrderState.New { order_id = 1; price = 100.0; quantity = 10 } in
  let state = transition state Submit in
  print_s [%sexp (state : OrderState.t)];
  [%expect {| (Pending ((order_id 1) (exchange_id PENDING))) |}];
  
  let state = transition state (Fill { quantity = 10; price = 100.5 }) in
  print_s [%sexp (state : OrderState.t)];
  [%expect {| (Filled ((order_id 1) (avg_price 100.5))) |}]
```

### V12 Translation (C#)

```csharp
// V12: Snapshot testing with Verify
using VerifyXunit;

[UsesVerify]
public class OrderBookSnapshotTests
{
    // Snapshot test: Order book pretty print
    [Fact]
    public Task OrderBookPrettyPrint()
    {
        var book = OrderBook.Empty
            .Add(new Order(1, 100.0, 10))
            .Add(new Order(2, 100.5, 5));

        return Verify(book);
    }

    // Snapshot test: FSM state transitions
    [Fact]
    public Task FSMStateTransitions()
    {
        var state = new OrderState.New(OrderId: 1, Price: 100.0, Quantity: 10);
        
        var transitions = new List<(string Event, OrderState State)>
        {
            ("Submit", OrderFSM.Transition(state, new OrderEvent.Submit())),
        };

        var pending = transitions.Last().State;
        transitions.Add((
            "Fill",
            OrderFSM.Transition(pending, new OrderEvent.Fill(10, 100.5))
        ));

        return Verify(transitions);
    }

    // Snapshot test: Complex calculation result
    [Fact]
    public Task RiskCalculationSnapshot()
    {
        var portfolio = new Portfolio
        {
            Positions = new[]
            {
                new Position("AAPL", 100, 150.0),
                new Position("GOOGL", 50, 2800.0)
            }
        };

        var risk = RiskEngine.Calculate(portfolio);

        return Verify(risk);
    }

    // Snapshot test with scrubbing (remove timestamps)
    [Fact]
    public Task OrderEventLogWithScrubbing()
    {
        var events = new[]
        {
            new OrderEvent { OrderId = 1, Type = "Submit", Timestamp = DateTime.UtcNow },
            new OrderEvent { OrderId = 1, Type = "Fill", Timestamp = DateTime.UtcNow }
        };

        var settings = new VerifySettings();
        settings.ScrubMembers("Timestamp");  // Remove non-deterministic fields

        return Verify(events, settings);
    }
}

// Verify snapshot files stored in: OrderBookSnapshotTests.OrderBookPrettyPrint.verified.txt
/*
{
  Bids: [
    {
      Price: 100.0,
      Orders: [
        {
          OrderId: 1,
          Quantity: 10
        }
      ]
    }
  ],
  Asks: [
    {
      Price: 100.5,
      Orders: [
        {
          OrderId: 2,
          Quantity: 5
        }
      ]
    }
  ]
}
*/
```

**V12 DNA Alignment:**
- ✅ Lock-free: Pure functions, deterministic output
- ✅ Type-safe: Compiler-enforced serialization
- ✅ CYC ≤8: Simple snapshot assertions
- ✅ Regression detection: Catches unexpected changes

**DO:**
- ✅ Use Verify for complex output testing
- ✅ Scrub non-deterministic fields (timestamps, GUIDs)
- ✅ Commit `.verified.txt` files to git
- ✅ Review snapshot diffs in PRs

**DON'T:**
- ❌ Use snapshots for simple assertions (use `Assert.Equal`)
- ❌ Ignore snapshot diffs (review carefully)
- ❌ Include non-deterministic data (scrub it)
- ❌ Use snapshots for binary data (use hash comparison)

---

## Pattern 3: Inline Tests (Co-located Tests)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Inline tests with ppx_inline_test *)
module OrderBook = struct
  type t = { bids: order list; asks: order list }

  let add book order =
    (* implementation *)
    ...

  let%test "adding order increases size" =
    let book = empty in
    let book' = add book { order_id = 1; price = 100.0; quantity = 10 } in
    size book' = size book + 1

  let%test_unit "price-time priority maintained" =
    let book = empty
      |> add { order_id = 1; price = 100.0; quantity = 10 }
      |> add { order_id = 2; price = 100.0; quantity = 5 }
    in
    match best_bid book with
    | Some order -> [%test_eq: int] order.order_id 1
    | None -> failwith "Expected bid"
end
```

### V12 Translation (C#)

```csharp
// V12: Inline tests with xUnit (separate file, but co-located)
// File: OrderBook.cs
public sealed class OrderBook
{
    public ImmutableList<Order> Bids { get; init; }
    public ImmutableList<Order> Asks { get; init; }

    public static readonly OrderBook Empty = new()
    {
        Bids = ImmutableList<Order>.Empty,
        Asks = ImmutableList<Order>.Empty
    };

    public OrderBook Add(Order order)
    {
        // Implementation
        return order.Side == OrderSide.Buy
            ? this with { Bids = Bids.Add(order).Sort() }
            : this with { Asks = Asks.Add(order).Sort() };
    }

    public int Size => Bids.Count + Asks.Count;

    public Option<Order> BestBid() =>
        Bids.Any()
            ? Option<Order>.Some(Bids.First())
            : Option<Order>.None();
}

// File: OrderBook.Tests.cs (co-located in same directory)
public class OrderBookInlineTests
{
    [Fact]
    public void AddingOrderIncreasesSize()
    {
        var book = OrderBook.Empty;
        var book2 = book.Add(new Order(1, 100.0, 10, OrderSide.Buy));
        
        Assert.Equal(book.Size + 1, book2.Size);
    }

    [Fact]
    public void PriceTimePriorityMaintained()
    {
        var book = OrderBook.Empty
            .Add(new Order(1, 100.0, 10, OrderSide.Buy))
            .Add(new Order(2, 100.0, 5, OrderSide.Buy));

        var bestBid = book.BestBid();
        
        Assert.True(bestBid.IsSome);
        Assert.Equal(1, bestBid.Value.OrderId);
    }

    [Theory]
    [InlineData(100.0, 10, OrderSide.Buy)]
    [InlineData(100.5, 5, OrderSide.Sell)]
    public void AddOrderWithDifferentSides(double price, int quantity, OrderSide side)
    {
        var book = OrderBook.Empty.Add(new Order(1, price, quantity, side));
        
        Assert.Equal(1, book.Size);
        if (side == OrderSide.Buy)
            Assert.Single(book.Bids);
        else
            Assert.Single(book.Asks);
    }
}
```

**V12 DNA Alignment:**
- ✅ Lock-free: Pure functions, no side effects
- ✅ Type-safe: Compiler-enforced test signatures
- ✅ CYC ≤8: Simple test logic
- ✅ Co-located: Tests near code they test

**DO:**
- ✅ Place test files next to implementation files
- ✅ Use `[Theory]` for data-driven tests
- ✅ Test one behavior per test method
- ✅ Use descriptive test names

**DON'T:**
- ❌ Put all tests in separate `Tests/` directory (loses co-location)
- ❌ Test multiple behaviors in one test
- ❌ Use magic numbers (use named constants)
- ❌ Ignore test failures (fix immediately)

---

## Pattern 4: Deterministic Randomness (Reproducible Tests)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Deterministic random testing *)
let%test_unit "order matching is deterministic" =
  let seed = 42 in
  let rng = Random.State.make [| seed |] in
  
  let orders1 = generate_random_orders rng 100 in
  let result1 = match_orders orders1 in
  
  (* Reset RNG with same seed *)
  let rng = Random.State.make [| seed |] in
  let orders2 = generate_random_orders rng 100 in
  let result2 = match_orders orders2 in
  
  [%test_eq: match_result] result1 result2
```

### V12 Translation (C#)

```csharp
// V12: Deterministic random testing with seeded Random
public class DeterministicRandomTests
{
    private const int Seed = 42;

    [Fact]
    public void OrderMatchingIsDeterministic()
    {
        // First run
        var rng1 = new Random(Seed);
        var orders1 = GenerateRandomOrders(rng1, 100);
        var result1 = MatchOrders(orders1);

        // Second run with same seed
        var rng2 = new Random(Seed);
        var orders2 = GenerateRandomOrders(rng2, 100);
        var result2 = MatchOrders(orders2);

        Assert.Equal(result1, result2);
    }

    [Property]
    public Property FsCheckUsesReproducibleSeed()
    {
        // FsCheck automatically uses deterministic seeds
        // Failed tests print seed for reproduction
        return Prop.ForAll(
            Arb.From<int[]>(),
            xs =>
            {
                var sorted = xs.OrderBy(x => x).ToArray();
                return sorted.SequenceEqual(sorted.OrderBy(x => x));
            });
    }

    [Fact]
    public void ReproduceFailedPropertyTest()
    {
        // When FsCheck test fails, it prints:
        // "Falsifiable, after 42 tests (seed: 1234567890)"
        
        var config = Configuration.QuickThrowOnFailure;
        config.Replay = Random.StdGen.NewStdGen(1234567890, 296);  // Use printed seed

        Prop.ForAll(
            Arb.From<int[]>(),
            xs => xs.Length < 1000  // Reproduce specific failure
        ).Check(config);
    }

    private static Order[] GenerateRandomOrders(Random rng, int count)
    {
        return Enumerable.Range(0, count)
            .Select(i => new Order(
                OrderId: i,
                Price: rng.Next(90, 110) + rng.NextDouble(),
                Quantity: rng.Next(1, 100),
                Side: rng.Next(2) == 0 ? OrderSide.Buy : OrderSide.Sell))
            .ToArray();
    }

    private static MatchResult MatchOrders(Order[] orders)
    {
        // Deterministic matching logic
        return new MatchResult(/* ... */);
    }
}
```

**V12 DNA Alignment:**
- ✅ Lock-free: Pure functions, deterministic
- ✅ Type-safe: Compiler-enforced seeds
- ✅ CYC ≤8: Simple seed management
- ✅ Reproducible: Same seed → same test

**DO:**
- ✅ Use seeded `Random` for deterministic tests
- ✅ Print seed on test failure (for reproduction)
- ✅ Use FsCheck's built-in seed management
- ✅ Document how to reproduce failures

**DON'T:**
- ❌ Use `Random.Shared` in tests (non-deterministic)
- ❌ Ignore seed values (can't reproduce failures)
- ❌ Use `DateTime.Now` or `Guid.NewGuid()` (non-deterministic)
- ❌ Forget to reset RNG state between tests

---

## Pattern 5: Fast Feedback Loop (Parallel Tests)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Parallel test execution with inline_test_runner *)
let%test_module "OrderBook" = (module struct
  let%test "add order" = (* fast test *)
  let%test "remove order" = (* fast test *)
  let%test "match orders" = (* fast test *)
end)

(* Tests run in parallel by default *)
(* dune runtest runs all tests in <1 second *)
```

### V12 Translation (C#)

```csharp
// V12: Parallel test execution with xUnit
[Collection("OrderBook")]  // Tests in same collection run sequentially
public class OrderBookFastTests
{
    [Fact]
    public void AddOrder()
    {
        // Fast test (<10ms)
        var book = OrderBook.Empty.Add(new Order(1, 100.0, 10, OrderSide.Buy));
        Assert.Equal(1, book.Size);
    }

    [Fact]
    public void RemoveOrder()
    {
        // Fast test (<10ms)
        var book = OrderBook.Empty
            .Add(new Order(1, 100.0, 10, OrderSide.Buy))
            .Remove(1);
        Assert.Equal(0, book.Size);
    }

    [Fact]
    public void MatchOrders()
    {
        // Fast test (<10ms)
        var book = OrderBook.Empty
            .Add(new Order(1, 100.0, 10, OrderSide.Buy))
            .Add(new Order(2, 100.0, 10, OrderSide.Sell));
        
        var matches = book.Match();
        Assert.Single(matches);
    }
}

// Parallel execution configuration (xunit.runner.json)
/*
{
  "parallelizeAssembly": true,
  "parallelizeTestCollections": true,
  "maxParallelThreads": -1  // Use all available cores
}
*/

// Benchmark: Ensure tests are fast
public class TestPerformanceBenchmark
{
    [Fact]
    public void AllTestsRunInUnder1Second()
    {
        var sw = Stopwatch.StartNew();
        
        // Run all OrderBook tests
        var testClass = new OrderBookFastTests();
        testClass.AddOrder();
        testClass.RemoveOrder();
        testClass.MatchOrders();
        
        sw.Stop();
        Assert.True(sw.ElapsedMilliseconds < 1000, 
            $"Tests took {sw.ElapsedMilliseconds}ms (should be <1000ms)");
    }
}

// Slow test isolation
[Collection("SlowTests")]  // Run separately
public class OrderBookSlowTests
{
    [Fact]
    public async Task StressTestOrderMatching()
    {
        // Slow test (>1 second)
        var orders = Enumerable.Range(0, 10000)
            .Select(i => new Order(i, 100.0, 10, OrderSide.Buy))
            .ToArray();

        var book = orders.Aggregate(OrderBook.Empty, (acc, o) => acc.Add(o));
        
        Assert.Equal(10000, book.Size);
    }
}
```

**V12 DNA Alignment:**
- ✅ Lock-free: Parallel test execution safe
- ✅ Type-safe: Compiler-enforced test isolation
- ✅ CYC ≤8: Simple test logic
- ✅ Fast: <1 second for full suite

**DO:**
- ✅ Enable parallel test execution
- ✅ Keep tests fast (<10ms each)
- ✅ Isolate slow tests in separate collections
- ✅ Use `[Collection]` for test ordering

**DON'T:**
- ❌ Use shared mutable state (breaks parallelism)
- ❌ Write slow tests (>100ms)
- ❌ Disable parallel execution (slow feedback)
- ❌ Ignore test performance (profile regularly)

---

## Pattern 6: Test Data Builders (Readable Tests)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Test data builders *)
module Order_builder = struct
  type t = {
    order_id: int;
    price: float;
    quantity: int;
    side: order_side;
  }

  let default = {
    order_id = 1;
    price = 100.0;
    quantity = 10;
    side = Buy;
  }

  let with_price price t = { t with price }
  let with_quantity quantity t = { t with quantity }
  let with_side side t = { t with side }
  let build t = t
end

let%test "order matching" =
  let buy_order = Order_builder.(default |> with_side Buy |> build) in
  let sell_order = Order_builder.(default |> with_side Sell |> build) in
  (* test logic *)
```

### V12 Translation (C#)

```csharp
// V12: Test data builders with fluent API
public sealed class OrderBuilder
{
    private int _orderId = 1;
    private double _price = 100.0;
    private int _quantity = 10;
    private OrderSide _side = OrderSide.Buy;

    public static OrderBuilder Default() => new();

    public OrderBuilder WithOrderId(int orderId)
    {
        _orderId = orderId;
        return this;
    }

    public OrderBuilder WithPrice(double price)
    {
        _price = price;
        return this;
    }

    public OrderBuilder WithQuantity(int quantity)
    {
        _quantity = quantity;
        return this;
    }

    public OrderBuilder WithSide(OrderSide side)
    {
        _side = side;
        return this;
    }

    public Order Build() => new(_orderId, _price, _quantity, _side);

    // Convenience methods
    public static Order BuyOrder(double price = 100.0, int quantity = 10) =>
        Default().WithPrice(price).WithQuantity(quantity).WithSide(OrderSide.Buy).Build();

    public static Order SellOrder(double price = 100.0, int quantity = 10) =>
        Default().WithPrice(price).WithQuantity(quantity).WithSide(OrderSide.Sell).Build();
}

// Usage: Readable tests
public class OrderMatchingTests
{
    [Fact]
    public void MatchingBuyAndSellOrders()
    {
        var buyOrder = OrderBuilder.BuyOrder(price: 100.0, quantity: 10);
        var sellOrder = OrderBuilder.SellOrder(price: 100.0, quantity: 10);

        var book = OrderBook.Empty
            .Add(buyOrder)
            .Add(sellOrder);

        var matches = book.Match();

        Assert.Single(matches);
        Assert.Equal(10, matches[0].Quantity);
    }

    [Fact]
    public void PartialFillScenario()
    {
        var largeBuy = OrderBuilder.Default()
            .WithSide(OrderSide.Buy)
            .WithQuantity(100)
            .Build();

        var smallSell = OrderBuilder.Default()
            .WithSide(OrderSide.Sell)
            .WithQuantity(30)
            .Build();

        var book = OrderBook.Empty.Add(largeBuy).Add(smallSell);
        var matches = book.Match();

        Assert.Single(matches);
        Assert.Equal(30, matches[0].Quantity);
    }
}
```

**V12 DNA Alignment:**
- ✅ Lock-free: Immutable builders
- ✅ Type-safe: Compiler-enforced builder methods
- ✅ CYC ≤8: Simple builder logic
- ✅ Readable: Fluent API for test data

**DO:**
- ✅ Use builders for complex test data
- ✅ Provide sensible defaults
- ✅ Use fluent API for readability
- ✅ Create convenience methods for common scenarios

**DON'T:**
- ❌ Use builders for simple data (use constructors)
- ❌ Make builders mutable (use `with` expressions)
- ❌ Forget to provide defaults
- ❌ Create builders for every type (only complex ones)

---

## Pattern 7: Contract Testing (Interface Compliance)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Module signature testing *)
module type EXCHANGE = sig
  type t
  val submit_order : t -> order -> order_result Deferred.t
  val cancel_order : t -> order_id -> unit Deferred.t
  val query_order : t -> order_id -> order_state option Deferred.t
end

(* Test that implementations satisfy contract *)
module Test_exchange (E : EXCHANGE) = struct
  let%test_unit "submit order returns order ID" =
    let%bind result = E.submit_order exchange test_order in
    match result with
    | Ok order_id -> assert (order_id > 0)
    | Error _ -> failwith "Expected success"

  let%test_unit "cancel order is idempotent" =
    let%bind () = E.cancel_order exchange 1 in
    let%bind () = E.cancel_order exchange 1 in
    (* Should not fail *)
    return ()
end

(* Apply tests to all implementations *)
module Test_mock_exchange = Test_exchange(Mock_exchange)
module Test_real_exchange = Test_exchange(Real_exchange)
```

### V12 Translation (C#)

```csharp
// V12: Contract testing with abstract base class
public interface IExchange
{
    ValueTask<Result<OrderId, OrderError>> SubmitOrderAsync(
        Order order, CancellationToken ct = default);
    
    ValueTask<Result<Unit, string>> CancelOrderAsync(
        int orderId, CancellationToken ct = default);
    
    ValueTask<Option<OrderState>> QueryOrderAsync(
        int orderId, CancellationToken ct = default);
}

// Abstract contract tests
public abstract class ExchangeContractTests
{
    protected abstract IExchange CreateExchange();

    [Fact]
    public async Task SubmitOrderReturnsOrderId()
    {
        var exchange = CreateExchange();
        var order = OrderBuilder.BuyOrder();

        var result = await exchange.SubmitOrderAsync(order);

        Assert.True(result.IsOk);
        Assert.True(result.Value.Id > 0);
    }

    [Fact]
    public async Task CancelOrderIsIdempotent()
    {
        var exchange = CreateExchange();
        var order = OrderBuilder.BuyOrder();
        var submitResult = await exchange.SubmitOrderAsync(order);
        var orderId = submitResult.Value.Id;

        // Cancel twice
        var cancel1 = await exchange.CancelOrderAsync(orderId);
        var cancel2 = await exchange.CancelOrderAsync(orderId);

        // Both should succeed (idempotent)
        Assert.True(cancel1.IsOk);
        Assert.True(cancel2.IsOk);
    }

    [Fact]
    public async Task QueryNonExistentOrderReturnsNone()
    {
        var exchange = CreateExchange();

        var result = await exchange.QueryOrderAsync(999999);

        Assert.True(result.IsNone);
    }
}

// Concrete implementations
public class MockExchangeContractTests : ExchangeContractTests
{
    protected override IExchange CreateExchange() => new MockExchange();
}

public class RealExchangeContractTests : ExchangeContractTests
{
    protected override IExchange CreateExchange() => new RealExchange();
}

// All implementations must pass the same contract tests
```

**V12 DNA Alignment:**
- ✅ Lock-free: Pure interface contracts
- ✅ Type-safe: Compiler-enforced interface compliance
- ✅ CYC ≤8: Simple contract tests
- ✅ Reusable: Same tests for all implementations

**DO:**
- ✅ Use abstract base classes for contract tests
- ✅ Test interface behavior, not implementation
- ✅ Apply same tests to all implementations
- ✅ Test edge cases (null, empty, invalid)

**DON'T:**
- ❌ Test implementation details (test interface)
- ❌ Duplicate tests across implementations
- ❌ Forget to test error cases
- ❌ Ignore performance contracts (add benchmarks)

---

## Summary Checklist

### Testing Patterns Compliance

- [ ] **Property-Based Testing**: Use FsCheck for invariant testing
- [ ] **Expect Tests**: Use Verify for snapshot testing
- [ ] **Inline Tests**: Co-locate tests with implementation
- [ ] **Deterministic Randomness**: Use seeded Random for reproducibility
- [ ] **Fast Feedback**: Enable parallel execution, keep tests <10ms
- [ ] **Test Data Builders**: Use fluent API for readable test data
- [ ] **Contract Testing**: Use abstract base classes for interface compliance

### V12 DNA Compliance Matrix

| Pattern | Lock-Free | Type-Safe | CYC ≤15 | Fast | Reproducible |
|---------|-----------|-----------|---------|------|--------------|
| Property-Based | ✅ | ✅ | ✅ | ✅ | ✅ |
| Expect Tests | ✅ | ✅ | ✅ | ✅ | ✅ |
| Inline Tests | ✅ | ✅ | ✅ | ✅ | ✅ |
| Deterministic Random | ✅ | ✅ | ✅ | ✅ | ✅ |
| Fast Feedback | ✅ | ✅ | ✅ | ✅ | ✅ |
| Test Builders | ✅ | ✅ | ✅ | ✅ | ✅ |
| Contract Testing | ✅ | ✅ | ✅ | ✅ | ✅ |

---

## References

### Jane Street Resources
- **Firestore KB**: `will_wilson_why_testing_hard_2026` (Why Testing Is Hard and How to Fix It)
- **Firestore KB**: `jane_street_trading_billions_2023` (Production Engineering When Trading Billions)

### V12 Standards
- [`JANE_STREET_CORE_PATTERNS.md`](./JANE_STREET_CORE_PATTERNS.md) - Result monad, Option type
- [`JANE_STREET_FSM_PATTERNS.md`](./JANE_STREET_FSM_PATTERNS.md) - FSM testing patterns
- [`AGENTS.md`](../../AGENTS.md) - Section 10: Code Quality Toolchain

### Related Documents
- [`JANE_STREET_PERFORMANCE_PATTERNS.md`](./JANE_STREET_PERFORMANCE_PATTERNS.md) - Performance testing
- [`JANE_STREET_TYPE_SAFETY.md`](./JANE_STREET_TYPE_SAFETY.md) - Type-driven testing

### External Resources
- **FsCheck**: https://fscheck.github.io/FsCheck/
- **Verify**: https://github.com/VerifyTests/Verify
- **xUnit**: https://xunit.net/

---

**Document Status**: ✅ Complete (7 patterns documented)  
**Next Review**: 2026-07-03  
**Maintainer**: V12 Architecture Team