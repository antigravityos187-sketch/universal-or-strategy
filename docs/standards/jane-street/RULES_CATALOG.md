# Jane Street Rules Catalog

**Version**: 1.0  
**Last Updated**: 2026-06-03  
**Status**: Active Standard  
**Compliance**: V12 DNA Mandatory

---

## Overview

This catalog contains all DO/DON'T rules extracted from the 10 Jane Street standards documents. Rules are categorized by topic, assigned severity levels (P0/P1/P2), and include automation patterns for the rule checker.

**Rule Format**:
- **Rule ID**: Unique identifier (JS-XXX)
- **Category**: Type Safety, Performance, Concurrency, Testing, etc.
- **Severity**: P0 (CRITICAL - Blocking), P1 (HIGH - Warning), P2 (MEDIUM - Info)
- **Source**: Originating standards document
- **Pattern**: Regex or AST pattern for automation
- **DO/DON'T**: Code examples
- **Fix Suggestion**: Actionable remediation

---

## Category: Type Safety (20 rules)

### Rule JS-001: Use Result<T,E> Instead of Exceptions

**Category**: Type Safety  
**Severity**: P0 (CRITICAL)  
**Source**: JANE_STREET_CORE_PATTERNS.md, Pattern 1

**Description**: Never throw exceptions in hot paths. Use Result<T,E> for explicit error handling.

**Pattern** (for automation):
```regex
throw\s+new\s+\w+Exception\((?!.*\/\/\s*Result<T,E>)
```

**DO**:
```csharp
public Result<OrderId, OrderError> SubmitOrder(Order order)
{
    return ValidateOrder(order)
        .Bind(validated => SendToExchange(validated));
}
```

**DON'T**:
```csharp
public OrderId SubmitOrder(Order order)
{
    if (!IsValid(order))
        throw new InvalidOrderException(); // ❌ Exception in hot path
    return SendToExchange(order);
}
```

**Fix Suggestion**: Replace exception throwing with `Result<T,E>.Err(error)` and update return type.

**Related Rules**: JS-002 (Option<T>), JS-015 (Validated Types)

---

### Rule JS-002: Use Option<T> Instead of Null

**Category**: Type Safety  
**Severity**: P0 (CRITICAL)  
**Source**: JANE_STREET_CORE_PATTERNS.md, Pattern 4

**Description**: Never return null for missing values. Use Option<T> or nullable reference types.

**Pattern** (for automation):
```regex
return\s+null\s*;(?!\s*//\s*Option<T>)
```

**DO**:
```csharp
public Option<User> FindUser(int id) => 
    _users.TryGetValue(id, out var user) 
        ? Option<User>.Some(user) 
        : Option<User>.None();
```

**DON'T**:
```csharp
public User FindUser(int id) => 
    _users.ContainsKey(id) ? _users[id] : null; // ❌ Implicit null
```

**Fix Suggestion**: Change return type to `Option<T>` and use `Some`/`None` constructors.

**Related Rules**: JS-001 (Result<T,E>), JS-015 (Validated Types)

---

### Rule JS-003: Use Sealed Record Hierarchies for Sum Types

**Category**: Type Safety  
**Severity**: P0 (CRITICAL)  
**Source**: JANE_STREET_CORE_PATTERNS.md, Pattern 2

**Description**: Model discriminated unions with sealed record hierarchies, not enums + separate data classes.

**Pattern** (for automation):
```regex
public\s+enum\s+\w+State.*\n.*public\s+class\s+\w+Data
```

**DO**:
```csharp
public abstract record OrderState
{
    private OrderState() { }
    public sealed record Pending(int OrderId) : OrderState;
    public sealed record Filled(int OrderId, double Price) : OrderState;
}
```

**DON'T**:
```csharp
public enum OrderState { Pending, Filled }
public class OrderData { public int OrderId; public double? Price; } // ❌ Separate data
```

**Fix Suggestion**: Convert enum to sealed record hierarchy with data embedded in each variant.

**Related Rules**: JS-004 (Exhaustive Matching), JS-010 (Private Constructors)

---

### Rule JS-004: Use Switch Expressions for Exhaustive Matching

**Category**: Type Safety  
**Severity**: P1 (HIGH)  
**Source**: JANE_STREET_TYPE_SAFETY.md, Pattern 3

**Description**: Use switch expressions with sealed hierarchies to ensure exhaustive matching.

**Pattern** (for automation):
```regex
switch\s*\(.*\)\s*\{.*default\s*:.*\}
```

**DO**:
```csharp
public string Describe(OrderState state) =>
    state switch
    {
        OrderState.Pending p => $"Pending {p.OrderId}",
        OrderState.Filled f => $"Filled at {f.Price}",
        _ => throw new InvalidOperationException("Unreachable")
    };
```

**DON'T**:
```csharp
public string Describe(OrderState state)
{
    switch (state)
    {
        case OrderState.Pending: return "Pending";
        default: return "Unknown"; // ❌ Hides missing cases
    }
}
```

**Fix Suggestion**: Use switch expression and remove default case to expose missing patterns.

**Related Rules**: JS-003 (Sealed Hierarchies), JS-005 (Enable Nullable)

---

### Rule JS-005: Enable Nullable Reference Types

**Category**: Type Safety  
**Severity**: P1 (HIGH)  
**Source**: JANE_STREET_TYPE_SAFETY.md, Pattern 4

**Description**: Enable nullable reference types to catch null reference errors at compile time.

**Pattern** (for automation):
```regex
^(?!.*#nullable\s+enable)
```

**DO**:
```csharp
#nullable enable

public string GetSymbol(Option<Order> orderOpt) =>
    orderOpt.Match(
        onSome: order => order.Symbol.Value,
        onNone: () => "UNKNOWN");
```

**DON'T**:
```csharp
// ❌ No #nullable enable
public string GetSymbol(Order order) => order?.Symbol ?? "UNKNOWN";
```

**Fix Suggestion**: Add `#nullable enable` at top of file and fix all warnings.

**Related Rules**: JS-002 (Option<T>), JS-004 (Exhaustive Matching)

---

### Rule JS-006: Use Phantom Types for Units

**Category**: Type Safety  
**Severity**: P1 (HIGH)  
**Source**: JANE_STREET_TYPE_SAFETY.md, Pattern 1

**Description**: Use phantom types to prevent mixing incompatible units (currency, distance, time).

**Pattern** (for automation):
```regex
public\s+\w+\s+\w+(USD|EUR|GBP)\s*\+\s*\w+(USD|EUR|GBP)(?!\1)
```

**DO**:
```csharp
public readonly struct Price<TCurrency> where TCurrency : struct, ICurrency
{
    public static Price<TCurrency> operator +(Price<TCurrency> a, Price<TCurrency> b) => ...
}
```

**DON'T**:
```csharp
public double UsdPrice + double EurPrice; // ❌ Can mix currencies
```

**Fix Suggestion**: Wrap primitives in generic structs with currency marker types.

**Related Rules**: JS-007 (Newtype Pattern), JS-010 (Private Constructors)

---

### Rule JS-007: Use Newtype Pattern for Semantic Types

**Category**: Type Safety  
**Severity**: P1 (HIGH)  
**Source**: JANE_STREET_TYPE_SAFETY.md, Pattern 2

**Description**: Wrap primitives in readonly structs to prevent mixing semantically different values.

**Pattern** (for automation):
```regex
public\s+\w+\s+\w+\(int\s+orderId,\s+int\s+userId\)
```

**DO**:
```csharp
public readonly struct OrderId { private readonly int _value; }
public readonly struct UserId { private readonly int _value; }
public void Process(OrderId orderId, UserId userId) { }
```

**DON'T**:
```csharp
public void Process(int orderId, int userId) { } // ❌ Can swap arguments
```

**Fix Suggestion**: Create readonly struct wrappers with private constructors and smart constructors.

**Related Rules**: JS-006 (Phantom Types), JS-010 (Private Constructors)

---

### Rule JS-008: Use Readonly Structs for Immutable Data

**Category**: Type Safety  
**Severity**: P1 (HIGH)  
**Source**: JANE_STREET_CORE_PATTERNS.md, Pattern 3

**Description**: Use readonly structs for small immutable data (<16 bytes).

**Pattern** (for automation):
```regex
public\s+struct\s+\w+(?!\s+:\s+readonly)
```

**DO**:
```csharp
public readonly struct MarketData
{
    public readonly double Bid { get; init; }
    public readonly double Ask { get; init; }
}
```

**DON'T**:
```csharp
public struct MarketData
{
    public double Bid { get; set; } // ❌ Mutable
}
```

**Fix Suggestion**: Add `readonly` modifier to struct and make all fields `readonly` or `init`-only.

**Related Rules**: JS-009 (Immutable Collections), JS-011 (With Expressions)

---

### Rule JS-009: Use ImmutableDictionary for Persistent Collections

**Category**: Type Safety  
**Severity**: P2 (MEDIUM)  
**Source**: JANE_STREET_CORE_PATTERNS.md, Pattern 3

**Description**: Use ImmutableDictionary instead of Dictionary for persistent collections.

**Pattern** (for automation):
```regex
Dictionary<\w+,\s*\w+>\s+\w+\s*=\s*new\(\)
```

**DO**:
```csharp
public ImmutableDictionary<string, MarketData> UpdateSymbols(
    ImmutableDictionary<string, MarketData> dataMap,
    IEnumerable<(string, double)> updates) => ...
```

**DON'T**:
```csharp
public Dictionary<string, MarketData> dataMap = new(); // ❌ Mutable
```

**Fix Suggestion**: Replace `Dictionary<K,V>` with `ImmutableDictionary<K,V>`.

**Related Rules**: JS-008 (Readonly Structs), JS-011 (With Expressions)

---

### Rule JS-010: Use Private Constructors for Smart Constructors

**Category**: Type Safety  
**Severity**: P0 (CRITICAL)  
**Source**: JANE_STREET_CORE_PATTERNS.md, Pattern 6

**Description**: Use private constructors + factory methods to enforce invariants.

**Pattern** (for automation):
```regex
public\s+\w+\(.*\)\s*\{(?!.*private\s+\w+\()
```

**DO**:
```csharp
public sealed class Order
{
    private Order(...) { }
    public static Result<Order, string> CreateLimitOrder(...) { }
}
```

**DON'T**:
```csharp
public class Order
{
    public Order(double price) { } // ❌ Public constructor, no validation
}
```

**Fix Suggestion**: Make constructor private, add static factory method with validation.

**Related Rules**: JS-001 (Result<T,E>), JS-015 (Validated Types)

---

### Rule JS-011: Use With Expressions for Functional Updates

**Category**: Type Safety  
**Severity**: P2 (MEDIUM)  
**Source**: JANE_STREET_CORE_PATTERNS.md, Pattern 3

**Description**: Use `with` expressions for functional updates of immutable data.

**Pattern** (for automation):
```regex
\w+\.\w+\s*=\s*\w+;(?!.*with)
```

**DO**:
```csharp
public MarketData WithBid(double newBid) =>
    this with { Bid = newBid, LastUpdate = DateTimeOffset.UtcNow };
```

**DON'T**:
```csharp
public void UpdateBid(double newBid)
{
    this.Bid = newBid; // ❌ Mutation
}
```

**Fix Suggestion**: Replace mutation with `with` expression returning new instance.

**Related Rules**: JS-008 (Readonly Structs), JS-009 (Immutable Collections)

---

### Rule JS-012: Use Bind for Monadic Composition

**Category**: Type Safety  
**Severity**: P2 (MEDIUM)  
**Source**: JANE_STREET_CORE_PATTERNS.md, Pattern 5

**Description**: Use Bind operator to chain Result<T,E> and Option<T> operations.

**Pattern** (for automation):
```regex
if\s*\(.*\.IsOk\).*if\s*\(.*\.IsOk\)
```

**DO**:
```csharp
public Result<OrderId, OrderError> ProcessOrder(Order order) =>
    ValidateOrder(order)
        .Bind(EnrichWithMarketData)
        .Bind(SubmitToExchange);
```

**DON'T**:
```csharp
var result1 = ValidateOrder(order);
if (!result1.IsOk) return result1.Error;
var result2 = EnrichWithMarketData(result1.Value);
if (!result2.IsOk) return result2.Error; // ❌ Nested if/else
```

**Fix Suggestion**: Replace nested if/else with `.Bind()` chain.

**Related Rules**: JS-001 (Result<T,E>), JS-002 (Option<T>)

---

### Rule JS-013: Use Extension Methods for Pipeline Operators

**Category**: Type Safety  
**Severity**: P2 (MEDIUM)  
**Source**: JANE_STREET_CORE_PATTERNS.md, Pattern 5

**Description**: Use extension methods to create pipeline-style APIs.

**Pattern** (for automation):
```regex
public\s+static\s+\w+\s+Pipe<.*>\(this\s+\w+\s+value
```

**DO**:
```csharp
public static U Pipe<T, U>(this T value, Func<T, U> f) => f(value);

var result = order
    .Pipe(ValidateOrder)
    .Bind(EnrichWithMarketData);
```

**DON'T**:
```csharp
var validated = ValidateOrder(order);
var enriched = EnrichWithMarketData(validated); // ❌ Imperative style
```

**Fix Suggestion**: Create extension methods for common operations.

**Related Rules**: JS-012 (Bind), JS-014 (LINQ Query Syntax)

---

### Rule JS-014: Use LINQ Query Syntax for Complex Pipelines

**Category**: Type Safety  
**Severity**: P2 (MEDIUM)  
**Source**: JANE_STREET_CORE_PATTERNS.md, Pattern 5

**Description**: Use LINQ query syntax for complex monadic pipelines.

**Pattern** (for automation):
```regex
\.Bind\(.*\.Bind\(.*\.Bind\(
```

**DO**:
```csharp
public Result<OrderId, OrderError> ProcessOrder(Order order) =>
    from validated in ValidateOrder(order)
    from enriched in EnrichWithMarketData(validated)
    from orderId in SubmitToExchange(enriched)
    select orderId;
```

**DON'T**:
```csharp
ValidateOrder(order)
    .Bind(v => EnrichWithMarketData(v)
        .Bind(e => SubmitToExchange(e))); // ❌ Deeply nested
```

**Fix Suggestion**: Convert nested Bind to LINQ query syntax.

**Related Rules**: JS-012 (Bind), JS-013 (Pipeline Operators)

---

### Rule JS-015: Parse at Boundaries, Use Validated Types Internally

**Category**: Type Safety  
**Severity**: P0 (CRITICAL)  
**Source**: JANE_STREET_TYPE_SAFETY.md, Pattern 5

**Description**: Parse and validate at system boundaries, use validated types internally.

**Pattern** (for automation):
```regex
public\s+\w+\s+\w+\(string\s+email\)(?!.*Email\.Parse)
```

**DO**:
```csharp
public readonly struct Email
{
    private Email(string value) { }
    public static Result<Email, string> Parse(string value) { }
}

public void SendEmail(Email email, string body) { } // No validation needed
```

**DON'T**:
```csharp
public void SendEmail(string email, string body)
{
    if (!email.Contains('@')) throw new Exception(); // ❌ Repeated validation
}
```

**Fix Suggestion**: Create validated type with Parse method, use internally.

**Related Rules**: JS-010 (Private Constructors), JS-001 (Result<T,E>)

---

### Rule JS-016: Use Type-Level State Machines

**Category**: Type Safety  
**Severity**: P1 (HIGH)  
**Source**: JANE_STREET_TYPE_SAFETY.md, Pattern 6

**Description**: Use phantom types to encode state machine transitions at compile time.

**Pattern** (for automation):
```regex
public\s+enum\s+\w+State.*\n.*public\s+void\s+\w+\(.*State\s+state\)
```

**DO**:
```csharp
public sealed class Connection<TState> where TState : struct, IConnectionState
{
    public async ValueTask<Connection<Authenticated>> AuthenticateAsync(
        this Connection<Connected> conn) { }
}
```

**DON'T**:
```csharp
public enum ConnectionState { Disconnected, Connected, Authenticated }
public void SendMessage(ConnectionState state) { } // ❌ Runtime check
```

**Fix Suggestion**: Use phantom types to make invalid state transitions unrepresentable.

**Related Rules**: JS-006 (Phantom Types), JS-003 (Sealed Hierarchies)

---

### Rule JS-017: Use Generic Constraints for Dependent Types

**Category**: Type Safety  
**Severity**: P1 (HIGH)  
**Source**: JANE_STREET_TYPE_SAFETY.md, Pattern 7

**Description**: Use generic constraints to enforce type relationships at compile time.

**Pattern** (for automation):
```regex
public\s+interface\s+IExpr\s*\{(?!.*<T>)
```

**DO**:
```csharp
public interface IExpr<T> { }
public sealed record AddExpr(IExpr<int> Left, IExpr<int> Right) : IExpr<int>;
```

**DON'T**:
```csharp
public interface IExpr { }
public class AddExpr { public IExpr Left; public IExpr Right; } // ❌ No type safety
```

**Fix Suggestion**: Add generic type parameter to interface and constrain implementations.

**Related Rules**: JS-006 (Phantom Types), JS-016 (Type-Level FSM)

---

### Rule JS-018: Implement IEquatable<T> for Value Types

**Category**: Type Safety  
**Severity**: P2 (MEDIUM)  
**Source**: JANE_STREET_TYPE_SAFETY.md, Pattern 2

**Description**: Implement IEquatable<T> for readonly structs to avoid boxing.

**Pattern** (for automation):
```regex
public\s+readonly\s+struct\s+\w+(?!.*:\s*IEquatable)
```

**DO**:
```csharp
public readonly struct OrderId : IEquatable<OrderId>
{
    public bool Equals(OrderId other) => _value == other._value;
    public override int GetHashCode() => _value.GetHashCode();
}
```

**DON'T**:
```csharp
public readonly struct OrderId
{
    // ❌ No IEquatable, causes boxing
}
```

**Fix Suggestion**: Implement IEquatable<T>, override Equals/GetHashCode, add == and != operators.

**Related Rules**: JS-007 (Newtype Pattern), JS-008 (Readonly Structs)

---

### Rule JS-019: Override ToString for Debugging

**Category**: Type Safety  
**Severity**: P2 (MEDIUM)  
**Source**: JANE_STREET_TYPE_SAFETY.md, Pattern 2

**Description**: Override ToString for custom types to aid debugging.

**Pattern** (for automation):
```regex
public\s+readonly\s+struct\s+\w+(?!.*override\s+string\s+ToString)
```

**DO**:
```csharp
public readonly struct OrderId
{
    public override string ToString() => $"OrderId({_value})";
}
```

**DON'T**:
```csharp
public readonly struct OrderId
{
    // ❌ No ToString, prints type name only
}
```

**Fix Suggestion**: Add `override string ToString()` with meaningful representation.

**Related Rules**: JS-007 (Newtype Pattern), JS-018 (IEquatable)

---

### Rule JS-020: Use Records for Data Transfer Objects

**Category**: Type Safety  
**Severity**: P2 (MEDIUM)  
**Source**: JANE_STREET_CORE_PATTERNS.md, Pattern 2

**Description**: Use records for DTOs to get value semantics and immutability.

**Pattern** (for automation):
```regex
public\s+class\s+\w+DTO\s*\{(?!.*record)
```

**DO**:
```csharp
public record OrderDTO(int OrderId, double Price, int Quantity);
```

**DON'T**:
```csharp
public class OrderDTO
{
    public int OrderId { get; set; } // ❌ Mutable class
}
```

**Fix Suggestion**: Convert class to record with positional parameters.

**Related Rules**: JS-008 (Readonly Structs), JS-011 (With Expressions)

---

## Category: Concurrency (15 rules)

### Rule JS-021: No Lock() Usage

**Category**: Concurrency  
**Severity**: P0 (CRITICAL)  
**Source**: AGENTS.md, Section 2

**Description**: Lock usage is strictly banned. Use Actor/FSM pattern or atomic primitives.

**Pattern** (for automation):
```regex
lock\s*\(
```

**DO**:
```csharp
private readonly Channel<Message> _channel = Channel.CreateUnbounded<Message>();

public async ValueTask EnqueueAsync(Message msg) =>
    await _channel.Writer.WriteAsync(msg);
```

**DON'T**:
```csharp
private readonly object _lock = new();

public void UpdateState()
{
    lock (_lock) { } // ❌ BANNED
}
```

**Fix Suggestion**: Replace lock with Channel-based Actor pattern or Interlocked operations.

**Related Rules**: JS-022 (Actor Pattern), JS-023 (Atomic Primitives)

---

### Rule JS-022: Use Actor Pattern for Stateful Concurrency

**Category**: Concurrency  
**Severity**: P0 (CRITICAL)  
**Source**: JANE_STREET_ASYNC_PATTERNS.md, Pattern 2

**Description**: Use Channel-based Actor pattern for stateful message processing.

**Pattern** (for automation):
```regex
private\s+\w+\s+_state;.*lock\s*\(
```

**DO**:
```csharp
public sealed class OrderActor
{
    private readonly Channel<Message> _mailbox = Channel.CreateUnbounded<Message>();
    
    public async ValueTask EnqueueAsync(Message msg) =>
        await _mailbox.Writer.WriteAsync(msg);
        
    private async Task ProcessMessagesAsync()
    {
        await foreach (var msg in _mailbox.Reader.ReadAllAsync())
        {
            _state = HandleMessage(_state, msg);
        }
    }
}
```

**DON'T**:
```csharp
private OrderState _state;
private readonly object _lock = new();

public void UpdateState(Message msg)
{
    lock (_lock) { _state = HandleMessage(_state, msg); } // ❌ Lock
}
```

**Fix Suggestion**: Convert to Actor pattern with Channel-based mailbox.

**Related Rules**: JS-021 (No Lock), JS-024 (Structured Concurrency)

---

### Rule JS-023: Use Atomic Primitives for Simple State

**Category**: Concurrency  
**Severity**: P1 (HIGH)  
**Source**: JANE_STREET_PERFORMANCE_PATTERNS.md, Pattern 5

**Description**: Use Interlocked operations for simple atomic state updates.

**Pattern** (for automation):
```regex
lock\s*\(\w+\)\s*\{\s*\w+\s*[+\-*/]=
```

**DO**:
```csharp
private long _counter;

public void Increment() =>
    Interlocked.Increment(ref _counter);
```

**DON'T**:
```csharp
private long _counter;
private readonly object _lock = new();

public void Increment()
{
    lock (_lock) { _counter++; } // ❌ Lock for simple increment
}
```

**Fix Suggestion**: Replace lock with Interlocked.Increment/Add/CompareExchange.

**Related Rules**: JS-021 (No Lock), JS-025 (Lock-Free Data Structures)

---

### Rule JS-024: Use Structured Concurrency

**Category**: Concurrency  
**Severity**: P1 (HIGH)  
**Source**: JANE_STREET_ASYNC_PATTERNS.md, Pattern 3

**Description**: Use Task.WhenAll for scoped parallelism, ensure all tasks complete.

**Pattern** (for automation):
```regex
Task\.Run\(.*\);(?!.*await)
```

**DO**:
```csharp
public async Task ProcessOrdersAsync(Order[] orders)
{
    var tasks = orders.Select(ProcessOrderAsync).ToArray();
    await Task.WhenAll(tasks); // Structured: all tasks complete
}
```

**DON'T**:
```csharp
public void ProcessOrders(Order[] orders)
{
    foreach (var order in orders)
        Task.Run(() => ProcessOrder(order)); // ❌ Fire-and-forget
}
```

**Fix Suggestion**: Use Task.WhenAll and await all tasks before returning.

**Related Rules**: JS-026 (Backpressure), JS-027 (Timeout)

---

### Rule JS-025: Use Lock-Free Data Structures

**Category**: Concurrency  
**Severity**: P1 (HIGH)  
**Source**: JANE_STREET_PERFORMANCE_PATTERNS.md, Pattern 5

**Description**: Use ConcurrentQueue/ConcurrentDictionary instead of lock-protected collections.

**Pattern** (for automation):
```regex
lock\s*\(\w+\)\s*\{\s*\w+\.Add\(
```

**DO**:
```csharp
private readonly ConcurrentQueue<Order> _orders = new();

public void AddOrder(Order order) =>
    _orders.Enqueue(order);
```

**DON'T**:
```csharp
private readonly List<Order> _orders = new();
private readonly object _lock = new();

public void AddOrder(Order order)
{
    lock (_lock) { _orders.Add(order); } // ❌ Lock-protected collection
}
```

**Fix Suggestion**: Replace lock-protected collection with concurrent collection.

**Related Rules**: JS-021 (No Lock), JS-023 (Atomic Primitives)

---

### Rule JS-026: Implement Backpressure

**Category**: Concurrency  
**Severity**: P1 (HIGH)  
**Source**: JANE_STREET_ASYNC_PATTERNS.md, Pattern 4

**Description**: Use bounded channels to implement backpressure and prevent memory exhaustion.

**Pattern** (for automation):
```regex
Channel\.CreateUnbounded<\w+>\(\)
```

**DO**:
```csharp
private readonly Channel<Message> _channel = 
    Channel.CreateBounded<Message>(new BoundedChannelOptions(1000)
    {
        FullMode = BoundedChannelFullMode.Wait
    });
```

**DON'T**:
```csharp
private readonly Channel<Message> _channel = 
    Channel.CreateUnbounded<Message>(); // ❌ No backpressure
```

**Fix Suggestion**: Use CreateBounded with appropriate capacity and FullMode.

**Related Rules**: JS-022 (Actor Pattern), JS-024 (Structured Concurrency)

---

### Rule JS-027: Use Timeout for All Async Operations

**Category**: Concurrency  
**Severity**: P1 (HIGH)  
**Source**: JANE_STREET_ASYNC_PATTERNS.md, Pattern 6

**Description**: Always use CancellationToken with timeout for async operations.

**Pattern** (for automation):
```regex
async\s+\w+\s+\w+Async\([^)]*\)(?!.*CancellationToken)
```

**DO**:
```csharp
public async ValueTask<Result<Order, string>> FetchOrderAsync(
    int orderId,
    CancellationToken ct = default)
{
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
    cts.CancelAfter(TimeSpan.FromSeconds(5));
    return await _client.GetOrderAsync(orderId, cts.Token);
}
```

**DON'T**:
```csharp
public async Task<Order> FetchOrderAsync(int orderId)
{
    return await _client.GetOrderAsync(orderId); // ❌ No timeout
}
```

**Fix Suggestion**: Add CancellationToken parameter and use CancelAfter.

**Related Rules**: JS-028 (Retry with Backoff), JS-024 (Structured Concurrency)

---

### Rule JS-028: Implement Retry with Exponential Backoff

**Category**: Concurrency  
**Severity**: P2 (MEDIUM)  
**Source**: JANE_STREET_ASYNC_PATTERNS.md, Pattern 6

**Description**: Use exponential backoff for retry logic to avoid thundering herd.

**Pattern** (for automation):
```regex
for\s*\(.*retries.*\)\s*\{.*await.*\}(?!.*Task\.Delay)
```

**DO**:
```csharp
public async ValueTask<Result<T, string>> RetryAsync<T>(
    Func<CancellationToken, ValueTask<Result<T, string>>> operation,
    int maxRetries = 3)
{
    for (int i = 0; i < maxRetries; i++)
    {
        var result = await operation(ct);
        if (result.IsOk) return result;
        
        var delay = TimeSpan.FromMilliseconds(100 * Math.Pow(2, i));
        await Task.Delay(delay, ct);
    }
}
```

**DON'T**:
```csharp
for (int i = 0; i < 3; i++)
{
    var result = await operation();
    if (result.IsOk) return result;
    // ❌ No delay, thundering herd
}
```

**Fix Suggestion**: Add exponential backoff delay between retries.

**Related Rules**: JS-027 (Timeout), JS-029 (Circuit Breaker)

---

### Rule JS-029: Use Circuit Breaker for External Services

**Category**: Concurrency  
**Severity**: P2 (MEDIUM)  
**Source**: JANE_STREET_ASYNC_PATTERNS.md, Pattern 6

**Description**: Implement circuit breaker pattern for external service calls.

**Pattern** (for automation):
```regex
await\s+_httpClient\.GetAsync\((?!.*CircuitBreaker)
```

**DO**:
```csharp
private readonly CircuitBreaker _breaker = new(
    failureThreshold: 5,
    timeout: TimeSpan.FromSeconds(30));

public async ValueTask<Result<T, string>> CallServiceAsync<T>()
{
    if (_breaker.IsOpen)
        return Result<T, string>.Err("Circuit breaker open");
        
    try
    {
        var result = await _httpClient.GetAsync(url);
        _breaker.RecordSuccess();
        return result;
    }
    catch
    {
        _breaker.RecordFailure();
        throw;
    }
}
```

**DON'T**:
```csharp
public async Task<T> CallServiceAsync<T>()
{
    return await _httpClient.GetAsync(url); // ❌ No circuit breaker
}
```

**Fix Suggestion**: Wrap external calls in circuit breaker pattern.

**Related Rules**: JS-027 (Timeout), JS-028 (Retry)

---

### Rule JS-030: Use AsyncLocal for Context Propagation

**Category**: Concurrency  
**Severity**: P2 (MEDIUM)  
**Source**: JANE_STREET_ASYNC_PATTERNS.md, Pattern 7

**Description**: Use AsyncLocal<T> for async context propagation, not ThreadLocal<T>.

**Pattern** (for automation):
```regex
private\s+static\s+readonly\s+ThreadLocal<\w+>
```

**DO**:
```csharp
private static readonly AsyncLocal<RequestContext> _context = new();

public static RequestContext Current
{
    get => _context.Value;
    set => _context.Value = value;
}
```

**DON'T**:
```csharp
private static readonly ThreadLocal<RequestContext> _context = new(); // ❌ ThreadLocal
```

**Fix Suggestion**: Replace ThreadLocal<T> with AsyncLocal<T>.

**Related Rules**: JS-024 (Structured Concurrency), JS-031 (ConfigureAwait)

---

### Rule JS-031: Use ConfigureAwait(false) in Libraries

**Category**: Concurrency  
**Severity**: P2 (MEDIUM)  
**Source**: JANE_STREET_ASYNC_PATTERNS.md, Pattern 1

**Description**: Use ConfigureAwait(false) in library code to avoid capturing SynchronizationContext.

**Pattern** (for automation):
```regex
await\s+\w+\.(?!.*ConfigureAwait)
```

**DO**:
```csharp
public async ValueTask<Result<T, string>> FetchAsync<T>()
{
    var response = await _httpClient.GetAsync(url).ConfigureAwait(false);
    return await ProcessResponseAsync(response).ConfigureAwait(false);
}
```

**DON'T**:
```csharp
public async Task<T> FetchAsync<T>()
{
    var response = await _httpClient.GetAsync(url); // ❌ Captures context
}
```

**Fix Suggestion**: Add `.ConfigureAwait(false)` to all awaits in library code.

**Related Rules**: JS-027 (Timeout), JS-030 (AsyncLocal)

---

### Rule JS-032: Use ValueTask for Hot Paths

**Category**: Concurrency  
**Severity**: P1 (HIGH)  
**Source**: JANE_STREET_ASYNC_PATTERNS.md, Pattern 1

**Description**: Use ValueTask<T> instead of Task<T> in hot paths to avoid allocation.

**Pattern** (for automation):
```regex
public\s+async\s+Task<\w+>\s+\w+Async\((?!.*ValueTask)
```

**DO**:
```csharp
public async ValueTask<Result<Order, string>> GetOrderAsync(int id)
{
    if (_cache.TryGetValue(id, out var order))
        return Result<Order, string>.Ok(order); // No allocation
        
    return await FetchFromDatabaseAsync(id);
}
```

**DON'T**:
```csharp
public async Task<Order> GetOrderAsync(int id)
{
    if (_cache.TryGetValue(id, out var order))
        return order; // ❌ Allocates Task
}
```

**Fix Suggestion**: Change return type from Task<T> to ValueTask<T>.

**Related Rules**: JS-033 (Zero-Allocation), JS-034 (Span<T>)

---

### Rule JS-033: Avoid Async Void

**Category**: Concurrency  
**Severity**: P0 (CRITICAL)  
**Source**: JANE_STREET_ASYNC_PATTERNS.md, Pattern 1

**Description**: Never use async void except for event handlers.

**Pattern** (for automation):
```regex
async\s+void\s+\w+\((?!.*EventHandler)
```

**DO**:
```csharp
public async ValueTask ProcessOrderAsync(Order order)
{
    await _channel.Writer.WriteAsync(order);
}
```

**DON'T**:
```csharp
public async void ProcessOrder(Order order) // ❌ async void
{
    await _channel.Writer.WriteAsync(order);
}
```

**Fix Suggestion**: Change return type to ValueTask or Task.

**Related Rules**: JS-032 (ValueTask), JS-024 (Structured Concurrency)

---

### Rule JS-034: Use Async Sequences for Lazy Streams

**Category**: Concurrency  
**Severity**: P2 (MEDIUM)  
**Source**: JANE_STREET_ASYNC_PATTERNS.md, Pattern 5

**Description**: Use IAsyncEnumerable<T> for lazy async streams.

**Pattern** (for automation):
```regex
public\s+async\s+Task<List<\w+>>\s+\w+Async\(
```

**DO**:
```csharp
public async IAsyncEnumerable<Order> StreamOrdersAsync(
    [EnumeratorCancellation] CancellationToken ct = default)
{
    await foreach (var order in _channel.Reader.ReadAllAsync(ct))
    {
        yield return order;
    }
}
```

**DON'T**:
```csharp
public async Task<List<Order>> GetAllOrdersAsync()
{
    var orders = new List<Order>();
    await foreach (var order in _channel.Reader.ReadAllAsync())
        orders.Add(order); // ❌ Loads all into memory
    return orders;
}
```

**Fix Suggestion**: Change return type to IAsyncEnumerable<T> and use yield return.

**Related Rules**: JS-026 (Backpressure), JS-032 (ValueTask)

---

### Rule JS-035: Use SemaphoreSlim for Async Coordination

**Category**: Concurrency  
**Severity**: P2 (MEDIUM)  
**Source**: JANE_STREET_ASYNC_PATTERNS.md, Pattern 7

**Description**: Use SemaphoreSlim (not Semaphore) for async coordination.

**Pattern** (for automation):
```regex
private\s+readonly\s+Semaphore\s+\w+
```

**DO**:
```csharp
private readonly SemaphoreSlim _semaphore = new(1, 1);

public async ValueTask<T> ExecuteAsync<T>(Func<ValueTask<T>> operation)
{
    await _semaphore.WaitAsync();
    try
    {
        return await operation();
    }
    finally
    {
        _semaphore.Release();
    }
}
```

**DON'T**:
```csharp
private readonly Semaphore _semaphore = new(1, 1); // ❌ Blocks thread

public T Execute<T>(Func<T> operation)
{
    _semaphore.WaitOne();
    try { return operation(); }
    finally { _semaphore.Release(); }
}
```

**Fix Suggestion**: Replace Semaphore with SemaphoreSlim and use WaitAsync.

**Related Rules**: JS-021 (No Lock), JS-024 (Structured Concurrency)

---

## Category: Performance (15 rules)

### Rule JS-036: Use Span<T> for Zero-Allocation

**Category**: Performance  
**Severity**: P0 (CRITICAL)  
**Source**: JANE_STREET_CORE_PATTERNS.md, Pattern 7

**Description**: Use Span<T> for stack-allocated buffers in hot paths.

**Pattern** (for automation):
```regex
byte\[\]\s+buffer\s*=\s*new\s+byte\[
```

**DO**:
```csharp
public void ProcessMessages(ReadOnlySpan<Message> messages)
{
    Span<byte> buffer = stackalloc byte[4096];
    foreach (ref readonly Message msg in messages)
    {
        SerializeToSpan(msg, buffer);
    }
}
```

**DON'T**:
```csharp
public void ProcessMessages(Message[] messages)
{
    byte[] buffer = new byte[4096]; // ❌ Heap allocation
}
```

**Fix Suggestion**: Replace byte[] with Span<byte> and use stackalloc.

**Related Rules**: JS-037 (ArrayPool), JS-038 (Ref Readonly)

---

### Rule JS-037: Use ArrayPool for Reusable Buffers

**Category**: Performance  
**Severity**: P0 (CRITICAL)  
**Source**: JANE_STREET_CORE_PATTERNS.md, Pattern 7

**Description**: Use ArrayPool<T> for reusable buffers to avoid allocation.

**Pattern** (for automation):
```regex
new\s+byte\[\d+\](?!.*ArrayPool)
```

**DO**:
```csharp
private readonly ArrayPool<byte> _pool = ArrayPool<byte>.Shared;

public void ProcessMessage(Message msg)
{
    byte[] buffer = _pool.Rent(4096);
    try
    {
        SerializeToBuffer(msg, buffer);
    }
    finally
    {
        _pool.Return(buffer);
    }
}
```

**DON'T**:
```csharp
public void ProcessMessage(Message msg)
{
    byte[] buffer = new byte[4096]; // ❌ Allocates every call
}
```

**Fix Suggestion**: Use ArrayPool.Rent/Return instead of new[].

**Related Rules**: JS-036 (Span<T>), JS-039 (Memory<T>)

---

### Rule JS-038: Use Ref Readonly for Zero-Copy Iteration

**Category**: Performance  
**Severity**: P1 (HIGH)  
**Source**: JANE_STREET_CORE_PATTERNS.md, Pattern 7

**Description**: Use `ref readonly` for zero-copy iteration over structs.

**Pattern** (for automation):
```regex
foreach\s*\(\s*var\s+\w+\s+in\s+\w+\)(?!.*ref\s+readonly)
```

**DO**:
```csharp
public void ProcessOrders(ReadOnlySpan<Order> orders)
{
    foreach (ref readonly Order order in orders)
    {
        ProcessOrder(order); // No copy
    }
}
```

**DON'T**:
```csharp
public void ProcessOrders(Order[] orders)
{
    foreach (var order in orders) // ❌ Copies struct
    {
        ProcessOrder(order);
    }
}
```

**Fix Suggestion**: Use `ref readonly` in foreach for struct iteration.

**Related Rules**: JS-036 (Span<T>), JS-040 (Readonly Struct)

---

### Rule JS-039: Use Memory<T> for Async Buffer Operations

**Category**: Performance  
**Severity**: P1 (HIGH)  
**Source**: JANE_STREET_PERFORMANCE_PATTERNS.md, Pattern 1

**Description**: Use Memory<T> instead of Span<T> for async operations.

**Pattern** (for automation):
```regex
async\s+\w+\s+\w+Async\(.*Span<\w+>
```

**DO**:
```csharp
public async ValueTask WriteAsync(Memory<byte> buffer, CancellationToken ct)
{
    await _stream.WriteAsync(buffer, ct);
}
```

**DON'T**:
```csharp
public async Task WriteAsync(Span<byte> buffer) // ❌ Span in async
{
    await _stream.WriteAsync(buffer.ToArray()); // Allocates
}
```

**Fix Suggestion**: Replace Span<T> with Memory<T> in async methods.

**Related Rules**: JS-036 (Span<T>), JS-032 (ValueTask)

---

### Rule JS-040: Use Readonly Struct for Small Value Types

**Category**: Performance  
**Severity**: P1 (HIGH)  
**Source**: JANE_STREET_PERFORMANCE_PATTERNS.md, Pattern 1

**Description**: Use readonly struct for small value types (<16 bytes) to avoid defensive copies.

**Pattern** (for automation):
```regex
public\s+struct\s+\w+(?!.*readonly).*\{.*\}
```

**DO**:
```csharp
public readonly struct Price
{
    private readonly double _value;
    public Price(double value) => _value = value;
}
```

**DON'T**:
```csharp
public struct Price // ❌ Not readonly, defensive copies
{
    public double Value { get; set; }
}
```

**Fix Suggestion**: Add `readonly` modifier to struct and make all fields readonly.

**Related Rules**: JS-008 (Readonly Structs), JS-038 (Ref Readonly)

---

### Rule JS-041: Use StructLayout for Cache-Friendly Data

**Category**: Performance  
**Severity**: P1 (HIGH)  
**Source**: JANE_STREET_PERFORMANCE_PATTERNS.md, Pattern 2

**Description**: Use StructLayout(LayoutKind.Sequential) for cache-friendly data structures.

**Pattern** (for automation):
```regex
public\s+struct\s+\w+(?!.*\[StructLayout)
```

**DO**:
```csharp
[StructLayout(LayoutKind.Sequential, Pack = 8)]
public readonly struct MarketData
{
    public readonly long Timestamp;
    public readonly double Bid;
    public readonly double Ask;
}
```

**DON'T**:
```csharp
public struct MarketData // ❌ Auto layout, cache misses
{
    public double Bid;
    public long Timestamp;
    public double Ask;
}
```

**Fix Suggestion**: Add [StructLayout(LayoutKind.Sequential)] and order fields by access pattern.

