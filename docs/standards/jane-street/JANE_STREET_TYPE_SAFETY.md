# Jane Street Type Safety: V12 Translation Guide

**Version**: 1.0  
**Last Updated**: 2026-06-03  
**Status**: Active Standard  
**Compliance**: V12 DNA Mandatory

---

## Overview

This document translates Jane Street's type safety patterns from OCaml into V12-aligned C# implementations. Jane Street's philosophy: **"Make illegal states unrepresentable"**—use the type system to prevent bugs at compile time, not runtime.

### Jane Street Type Safety Philosophy

Jane Street's approach to type safety:
- **Phantom Types**: Encode invariants in type parameters
- **Smart Constructors**: Validate at construction time
- **Newtype Pattern**: Prevent mixing incompatible values
- **Exhaustive Matching**: Compiler enforces all cases
- **No Null**: Option<T> instead of null references

### V12 Alignment

V12 DNA implements these principles:
- ✅ **Sealed Hierarchies**: Discriminated unions
- ✅ **Private Constructors**: Smart constructors
- ✅ **Readonly Structs**: Immutability by default
- ✅ **Result<T,E>**: Explicit error handling
- ✅ **Option<T>**: No null references

---

## Pattern 1: Phantom Types (Compile-Time Invariants)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Phantom types for units *)
type 'a price = Price of float

type usd
type eur

let usd_price : usd price = Price 100.0
let eur_price : eur price = Price 85.0

(* Compile error: cannot mix USD and EUR *)
(* let total = add_prices usd_price eur_price *)
```

### V12 Translation (C#)

```csharp
// V12: Phantom types with generic markers
public readonly struct Price<TCurrency>
    where TCurrency : struct, ICurrency
{
    private readonly double _value;

    private Price(double value)
    {
        _value = value;
    }

    public static Result<Price<TCurrency>, string> Create(double value)
    {
        if (value < 0)
            return Result<Price<TCurrency>, string>.Err("Price cannot be negative");
        
        return Result<Price<TCurrency>, string>.Ok(new Price<TCurrency>(value));
    }

    public double Value => _value;

    // Type-safe addition: only same currency
    public static Price<TCurrency> operator +(Price<TCurrency> a, Price<TCurrency> b) =>
        new Price<TCurrency>(a._value + b._value);

    // Compile error: cannot add different currencies
    // public static Price<TCurrency> operator +(Price<TCurrency> a, Price<TOther> b) => ...
}

// Currency markers (zero-size types)
public interface ICurrency { }
public struct USD : ICurrency { }
public struct EUR : ICurrency { }
public struct GBP : ICurrency { }

// Usage: Type-safe currency operations
public class TradingExample
{
    public void Execute()
    {
        var usdPrice = Price<USD>.Create(100.0).Value;
        var eurPrice = Price<EUR>.Create(85.0).Value;

        // OK: Same currency
        var totalUsd = usdPrice + usdPrice;

        // Compile error: Cannot mix currencies
        // var mixed = usdPrice + eurPrice;  // Type mismatch!

        // Explicit conversion required
        var convertedEur = ConvertToUSD(eurPrice);
        var total = usdPrice + convertedEur;
    }

    private Price<USD> ConvertToUSD(Price<EUR> eurPrice)
    {
        const double exchangeRate = 1.18;
        return Price<USD>.Create(eurPrice.Value * exchangeRate).Value;
    }
}
```

**V12 DNA Alignment:**
- ✅ Lock-free: Immutable readonly struct
- ✅ Type-safe: Compiler prevents mixing currencies
- ✅ CYC ≤8: Simple phantom type logic
- ✅ Zero-runtime cost: Phantom types erased

**DO:**
- ✅ Use phantom types for units (currency, distance, time)
- ✅ Use zero-size marker types
- ✅ Prevent mixing incompatible values at compile time
- ✅ Use smart constructors for validation

**DON'T:**
- ❌ Use runtime checks for unit mismatches
- ❌ Use strings for units ("USD", "EUR")
- ❌ Allow implicit conversions
- ❌ Forget to validate in smart constructors

---

## Pattern 2: Newtype Pattern (Semantic Types)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Newtype for semantic distinction *)
module Order_id : sig
  type t
  val of_int : int -> t
  val to_int : t -> int
end = struct
  type t = int
  let of_int x = x
  let to_int x = x
end

module Symbol : sig
  type t
  val of_string : string -> t
  val to_string : t -> string
end = struct
  type t = string
  let of_string x = x
  let to_string x = x
end

(* Compile error: cannot mix order_id and symbol *)
(* let lookup order_id symbol = ... *)
```

### V12 Translation (C#)

```csharp
// V12: Newtype pattern with readonly structs
public readonly struct OrderId : IEquatable<OrderId>
{
    private readonly int _value;

    private OrderId(int value)
    {
        _value = value;
    }

    public static Result<OrderId, string> Create(int value)
    {
        if (value <= 0)
            return Result<OrderId, string>.Err("Order ID must be positive");
        
        return Result<OrderId, string>.Ok(new OrderId(value));
    }

    public int Value => _value;

    public bool Equals(OrderId other) => _value == other._value;
    public override bool Equals(object? obj) => obj is OrderId other && Equals(other);
    public override int GetHashCode() => _value.GetHashCode();
    public override string ToString() => $"OrderId({_value})";

    public static bool operator ==(OrderId left, OrderId right) => left.Equals(right);
    public static bool operator !=(OrderId left, OrderId right) => !left.Equals(right);
}

public readonly struct Symbol : IEquatable<Symbol>
{
    private readonly string _value;

    private Symbol(string value)
    {
        _value = value;
    }

    public static Result<Symbol, string> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<Symbol, string>.Err("Symbol cannot be empty");
        
        if (value.Length > 10)
            return Result<Symbol, string>.Err("Symbol too long");
        
        if (!value.All(char.IsLetterOrDigit))
            return Result<Symbol, string>.Err("Symbol must be alphanumeric");
        
        return Result<Symbol, string>.Ok(new Symbol(value.ToUpperInvariant()));
    }

    public string Value => _value;

    public bool Equals(Symbol other) => _value == other._value;
    public override bool Equals(object? obj) => obj is Symbol other && Equals(other);
    public override int GetHashCode() => _value.GetHashCode();
    public override string ToString() => _value;

    public static bool operator ==(Symbol left, Symbol right) => left.Equals(right);
    public static bool operator !=(Symbol left, Symbol right) => !left.Equals(right);
}

// Usage: Type-safe API
public class OrderBook
{
    private readonly Dictionary<Symbol, List<Order>> _orders = new();

    public void AddOrder(OrderId orderId, Symbol symbol, double price, int quantity)
    {
        // Compile error: cannot pass int where OrderId expected
        // AddOrder(123, symbol, price, quantity);  // Type mismatch!

        // Compile error: cannot pass string where Symbol expected
        // AddOrder(orderId, "AAPL", price, quantity);  // Type mismatch!

        if (!_orders.ContainsKey(symbol))
            _orders[symbol] = new List<Order>();

        _orders[symbol].Add(new Order(orderId, price, quantity));
    }
}
```

**V12 DNA Alignment:**
- ✅ Lock-free: Immutable readonly structs
- ✅ Type-safe: Compiler prevents mixing types
- ✅ CYC ≤8: Simple newtype wrappers
- ✅ Validated: Smart constructors enforce invariants

**DO:**
- ✅ Use newtypes for semantic distinction
- ✅ Use private constructors + smart constructors
- ✅ Validate in smart constructors
- ✅ Implement IEquatable<T> for value semantics

**DON'T:**
- ❌ Use primitive types for domain concepts
- ❌ Allow public constructors (bypasses validation)
- ❌ Use inheritance for newtypes (use composition)
- ❌ Forget to override Equals/GetHashCode

---

## Pattern 3: Exhaustive Matching (Compiler-Enforced)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Exhaustive pattern matching *)
type order_status =
  | Pending
  | Filled
  | Cancelled
  | Rejected

let describe_status status =
  match status with
  | Pending -> "Order is pending"
  | Filled -> "Order is filled"
  | Cancelled -> "Order is cancelled"
  | Rejected -> "Order is rejected"
  (* Compiler error if any case is missing *)
```

### V12 Translation (C#)

```csharp
// V12: Exhaustive matching with sealed hierarchies
public abstract record OrderStatus
{
    private OrderStatus() { }  // Sealed hierarchy

    public sealed record Pending : OrderStatus;
    public sealed record Filled(double AvgPrice) : OrderStatus;
    public sealed record Cancelled(string Reason) : OrderStatus;
    public sealed record Rejected(string Error) : OrderStatus;
}

// Exhaustive matching with switch expression
public static class OrderStatusExtensions
{
    public static string Describe(this OrderStatus status) =>
        status switch
        {
            OrderStatus.Pending => "Order is pending",
            OrderStatus.Filled f => $"Order filled at {f.AvgPrice}",
            OrderStatus.Cancelled c => $"Order cancelled: {c.Reason}",
            OrderStatus.Rejected r => $"Order rejected: {r.Error}",
            _ => throw new InvalidOperationException("Unreachable")
        };

    // Compiler warning if case is missing (with nullable reference types)
    public static bool IsTerminal(this OrderStatus status) =>
        status switch
        {
            OrderStatus.Pending => false,
            OrderStatus.Filled => true,
            OrderStatus.Cancelled => true,
            OrderStatus.Rejected => true,
            // Compiler warning: CS8509 if case is missing
        };

    // Type-safe visitor pattern
    public static T Match<T>(
        this OrderStatus status,
        Func<T> onPending,
        Func<OrderStatus.Filled, T> onFilled,
        Func<OrderStatus.Cancelled, T> onCancelled,
        Func<OrderStatus.Rejected, T> onRejected) =>
        status switch
        {
            OrderStatus.Pending => onPending(),
            OrderStatus.Filled f => onFilled(f),
            OrderStatus.Cancelled c => onCancelled(c),
            OrderStatus.Rejected r => onRejected(r),
            _ => throw new InvalidOperationException("Unreachable")
        };
}

// Usage: Exhaustive handling
public void ProcessStatus(OrderStatus status)
{
    var message = status.Match(
        onPending: () => "Waiting for execution",
        onFilled: f => $"Executed at {f.AvgPrice}",
        onCancelled: c => $"Cancelled: {c.Reason}",
        onRejected: r => $"Rejected: {r.Error}");

    Console.WriteLine(message);
}
```

**V12 DNA Alignment:**
- ✅ Lock-free: Immutable records
- ✅ Type-safe: Compiler enforces exhaustiveness
- ✅ CYC ≤8: Simple switch expressions
- ✅ Maintainable: Adding new case triggers compiler warnings

**DO:**
- ✅ Use sealed record hierarchies for sum types
- ✅ Use switch expressions for exhaustive matching
- ✅ Enable nullable reference types (CS8509 warnings)
- ✅ Use visitor pattern for complex matching

**DON'T:**
- ❌ Use enums + separate data classes (loses type safety)
- ❌ Use default case in switch (hides missing cases)
- ❌ Allow external types to extend hierarchy
- ❌ Ignore compiler warnings (CS8509)

---

## Pattern 4: No Null (Option Type)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Option.t instead of null *)
let find_order order_id orders =
  List.find_opt (fun o -> o.order_id = order_id) orders

let process_order order_id orders =
  match find_order order_id orders with
  | None -> Error "Order not found"
  | Some order -> Ok (execute_order order)
```

### V12 Translation (C#)

```csharp
// V12: Option<T> instead of null
public readonly struct Option<T>
{
    private readonly bool _hasValue;
    private readonly T _value;

    private Option(bool hasValue, T value)
    {
        _hasValue = hasValue;
        _value = value;
    }

    public static Option<T> Some(T value) => new(true, value);
    public static Option<T> None() => new(false, default!);

    public bool IsSome => _hasValue;
    public bool IsNone => !_hasValue;

    public T Value => _hasValue ? _value : throw new InvalidOperationException("No value");
    public T ValueOr(T defaultValue) => _hasValue ? _value : defaultValue;

    public Option<U> Map<U>(Func<T, U> f) =>
        _hasValue ? Option<U>.Some(f(_value)) : Option<U>.None();

    public Option<U> Bind<U>(Func<T, Option<U>> f) =>
        _hasValue ? f(_value) : Option<U>.None();

    public T Match<T>(Func<T, T> onSome, Func<T> onNone) =>
        _hasValue ? onSome(_value) : onNone();
}

// Usage: No null references
public class OrderRepository
{
    private readonly Dictionary<OrderId, Order> _orders = new();

    public Option<Order> FindOrder(OrderId orderId)
    {
        return _orders.TryGetValue(orderId, out var order)
            ? Option<Order>.Some(order)
            : Option<Order>.None();
    }

    public Result<ExecutionResult, string> ProcessOrder(OrderId orderId)
    {
        return FindOrder(orderId).Match(
            onSome: order => ExecuteOrder(order),
            onNone: () => Result<ExecutionResult, string>.Err("Order not found"));
    }

    private Result<ExecutionResult, string> ExecuteOrder(Order order)
    {
        // Execution logic
        return Result<ExecutionResult, string>.Ok(new ExecutionResult());
    }
}

// Nullable reference types integration
#nullable enable

public class NullableExample
{
    // Compiler warning: CS8600 if null assigned
    public string GetSymbol(Option<Order> orderOpt)
    {
        return orderOpt.Match(
            onSome: order => order.Symbol.Value,
            onNone: () => "UNKNOWN");
    }

    // No null checks needed with Option<T>
    public double GetPrice(Option<Order> orderOpt)
    {
        return orderOpt.Map(o => o.Price).ValueOr(0.0);
    }
}
```

**V12 DNA Alignment:**
- ✅ Lock-free: Immutable readonly struct
- ✅ Type-safe: No null reference exceptions
- ✅ CYC ≤8: Simple option operations
- ✅ Explicit: Absence is explicit in type signature

**DO:**
- ✅ Use Option<T> instead of null
- ✅ Use Map/Bind for monadic composition
- ✅ Use Match for exhaustive handling
- ✅ Enable nullable reference types

**DON'T:**
- ❌ Use null for "no value" (loses type safety)
- ❌ Use Nullable<T> for reference types
- ❌ Throw exceptions for missing values
- ❌ Use sentinel values (e.g., -1 for "no ID")

---

## Pattern 5: Validated Types (Parse, Don't Validate)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Parse, don't validate *)
module Email : sig
  type t
  val of_string : string -> (t, string) result
  val to_string : t -> string
end = struct
  type t = string

  let of_string s =
    if String.contains s '@' then
      Ok s
    else
      Error "Invalid email"

  let to_string t = t
end

(* Once parsed, email is guaranteed valid *)
let send_email (email : Email.t) body =
  (* No validation needed here *)
  ...
```

### V12 Translation (C#)

```csharp
// V12: Parse, don't validate
public readonly struct Email : IEquatable<Email>
{
    private readonly string _value;

    private Email(string value)
    {
        _value = value;
    }

    public static Result<Email, string> Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<Email, string>.Err("Email cannot be empty");

        if (!value.Contains('@'))
            return Result<Email, string>.Err("Email must contain @");

        var parts = value.Split('@');
        if (parts.Length != 2)
            return Result<Email, string>.Err("Email must have exactly one @");

        if (string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
            return Result<Email, string>.Err("Email parts cannot be empty");

        return Result<Email, string>.Ok(new Email(value.ToLowerInvariant()));
    }

    public string Value => _value;

    public bool Equals(Email other) => _value == other._value;
    public override bool Equals(object? obj) => obj is Email other && Equals(other);
    public override int GetHashCode() => _value.GetHashCode();
    public override string ToString() => _value;

    public static bool operator ==(Email left, Email right) => left.Equals(right);
    public static bool operator !=(Email left, Email right) => !left.Equals(right);
}

// Usage: Parse once, use everywhere
public class EmailService
{
    // Email is guaranteed valid (parsed at boundary)
    public void SendEmail(Email email, string body)
    {
        // No validation needed here!
        Console.WriteLine($"Sending email to {email.Value}");
    }

    // Parse at system boundary
    public Result<Unit, string> SendEmailFromString(string emailStr, string body)
    {
        return Email.Parse(emailStr).Bind(email =>
        {
            SendEmail(email, body);
            return Result<Unit, string>.Ok(Unit.Value);
        });
    }
}

// Validated collection types
public readonly struct NonEmptyList<T>
{
    private readonly T[] _items;

    private NonEmptyList(T[] items)
    {
        _items = items;
    }

    public static Result<NonEmptyList<T>, string> Create(T[] items)
    {
        if (items == null || items.Length == 0)
            return Result<NonEmptyList<T>, string>.Err("List cannot be empty");

        return Result<NonEmptyList<T>, string>.Ok(new NonEmptyList<T>(items));
    }

    public T Head => _items[0];
    public ReadOnlySpan<T> Tail => _items.AsSpan(1);
    public ReadOnlySpan<T> AsSpan() => _items.AsSpan();
    public int Count => _items.Length;
}
```

**V12 DNA Alignment:**
- ✅ Lock-free: Immutable validated types
- ✅ Type-safe: Validation at construction
- ✅ CYC ≤8: Simple validation logic
- ✅ Parse once: No repeated validation

**DO:**
- ✅ Parse at system boundaries
- ✅ Use validated types internally
- ✅ Return Result<T,E> from parsers
- ✅ Make validation explicit in type

**DON'T:**
- ❌ Validate repeatedly (parse once)
- ❌ Use strings for validated data
- ❌ Allow invalid construction
- ❌ Throw exceptions from parsers (use Result<T,E>)

---

## Pattern 6: Type-Level State Machines

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Type-level state machines *)
type 'a connection_state =
  | Disconnected : unit connection_state
  | Connected : socket connection_state
  | Authenticated : (socket * session) connection_state

let connect () : socket connection_state = ...
let authenticate (conn : socket connection_state) : (socket * session) connection_state = ...
let send_message (conn : (socket * session) connection_state) msg = ...

(* Compile error: cannot send on unauthenticated connection *)
(* let conn = connect () in send_message conn "hello" *)
```

### V12 Translation (C#)

```csharp
// V12: Type-level state machines with phantom types
public interface IConnectionState { }
public struct Disconnected : IConnectionState { }
public struct Connected : IConnectionState { }
public struct Authenticated : IConnectionState { }

public sealed class Connection<TState>
    where TState : struct, IConnectionState
{
    private readonly Socket? _socket;
    private readonly Session? _session;

    private Connection(Socket? socket, Session? session)
    {
        _socket = socket;
        _session = session;
    }

    // Factory: Create disconnected connection
    public static Connection<Disconnected> Create() =>
        new Connection<Disconnected>(null, null);

    // Transition: Disconnected → Connected
    public async ValueTask<Result<Connection<Connected>, string>> ConnectAsync(
        this Connection<Disconnected> conn,
        string host,
        int port)
    {
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        
        try
        {
            await socket.ConnectAsync(host, port);
            return Result<Connection<Connected>, string>.Ok(
                new Connection<Connected>(socket, null));
        }
        catch (Exception ex)
        {
            return Result<Connection<Connected>, string>.Err(ex.Message);
        }
    }

    // Transition: Connected → Authenticated
    public async ValueTask<Result<Connection<Authenticated>, string>> AuthenticateAsync(
        this Connection<Connected> conn,
        string username,
        string password)
    {
        var session = await PerformAuthenticationAsync(conn._socket!, username, password);
        
        if (session == null)
            return Result<Connection<Authenticated>, string>.Err("Authentication failed");

        return Result<Connection<Authenticated>, string>.Ok(
            new Connection<Authenticated>(conn._socket, session));
    }

    // Only authenticated connections can send messages
    public async ValueTask<Result<Unit, string>> SendMessageAsync(
        this Connection<Authenticated> conn,
        string message)
    {
        // Compile-time guarantee: connection is authenticated
        await conn._socket!.SendAsync(Encoding.UTF8.GetBytes(message));
        return Result<Unit, string>.Ok(Unit.Value);
    }

    private static async Task<Session?> PerformAuthenticationAsync(
        Socket socket, string username, string password)
    {
        // Authentication logic
        return new Session();
    }
}

// Usage: Type-safe state transitions
public async Task ExampleAsync()
{
    var conn = Connection<Disconnected>.Create();

    // Connect
    var connectedResult = await conn.ConnectAsync("localhost", 8080);
    if (!connectedResult.IsOk)
        return;

    var connected = connectedResult.Value;

    // Authenticate
    var authenticatedResult = await connected.AuthenticateAsync("user", "pass");
    if (!authenticatedResult.IsOk)
        return;

    var authenticated = authenticatedResult.Value;

    // Send message (only possible on authenticated connection)
    await authenticated.SendMessageAsync("Hello, world!");

    // Compile error: cannot send on unauthenticated connection
    // await connected.SendMessageAsync("Hello");  // Type mismatch!
}

public class Socket { }
public class Session { }
```

**V12 DNA Alignment:**
- ✅ Lock-free: Immutable state transitions
- ✅ Type-safe: Compiler enforces state machine
- ✅ CYC ≤8: Simple state transition logic
- ✅ Compile-time: Invalid transitions impossible

**DO:**
- ✅ Use phantom types for state machines
- ✅ Make invalid transitions unrepresentable
- ✅ Use extension methods for transitions
- ✅ Return new state from transitions

**DON'T:**
- ❌ Use runtime state checks (use types)
- ❌ Allow invalid state transitions
- ❌ Use enums for states (loses type safety)
- ❌ Mutate state in-place (return new state)

---

## Pattern 7: Dependent Types (Compile-Time Constraints)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: GADTs for dependent types *)
type _ expr =
  | Int : int -> int expr
  | Bool : bool -> bool expr
  | Add : int expr * int expr -> int expr
  | If : bool expr * 'a expr * 'a expr -> 'a expr

let rec eval : type a. a expr -> a = function
  | Int n -> n
  | Bool b -> b
  | Add (e1, e2) -> eval e1 + eval e2
  | If (cond, then_, else_) ->
      if eval cond then eval then_ else eval else_

(* Compile error: type mismatch *)
(* let bad = Add (Int 1, Bool true) *)
```

### V12 Translation (C#)

```csharp
// V12: Dependent types with generic constraints
public interface IExpr<T> { }

public sealed record IntExpr(int Value) : IExpr<int>;
public sealed record BoolExpr(bool Value) : IExpr<bool>;
public sealed record AddExpr(IExpr<int> Left, IExpr<int> Right) : IExpr<int>;
public sealed record IfExpr<T>(IExpr<bool> Condition, IExpr<T> Then, IExpr<T> Else) : IExpr<T>;

public static class ExprEvaluator
{
    public static T Eval<T>(IExpr<T> expr) =>
        expr switch
        {
            IntExpr i => (T)(object)i.Value,
            BoolExpr b => (T)(object)b.Value,
            AddExpr add => (T)(object)(Eval(add.Left) + Eval(add.Right)),
            IfExpr<T> ifExpr => Eval(ifExpr.Condition)
                ? Eval(ifExpr.Then)
                : Eval(ifExpr.Else),
            _ => throw new InvalidOperationException("Unknown expression")
        };

    // Compile error: type mismatch
    // var bad = new AddExpr(new IntExpr(1), new BoolExpr(true));  // Type error!
}

// Usage: Type-safe expression evaluation
public void Example()
{
    // OK: Type-safe expressions
    IExpr<int> expr1 = new IntExpr(42);
    IExpr<int> expr2 = new AddExpr(new IntExpr(1), new IntExpr(2));
    IExpr<int> expr3 = new IfExpr<int>(
        new BoolExpr(true),
        new IntExpr(10),
        new IntExpr(20));

    var result1 = ExprEvaluator.Eval(expr1);  // 42
    var result2 = ExprEvaluator.Eval(expr2);  // 3
    var result3 = ExprEvaluator.Eval(expr3);  // 10

    // Compile error: cannot add int and bool
    // var bad = new AddExpr(new IntExpr(1), new BoolExpr(true));
}
```

**V12 DNA Alignment:**
- ✅ Lock-free: Immutable expression trees
- ✅ Type-safe: Compiler enforces type constraints
- ✅ CYC ≤8: Simple evaluation logic
- ✅ Compile-time: Type errors caught early

**DO:**
- ✅ Use generic constraints for dependent types
- ✅ Use sealed record hierarchies
- ✅ Make type errors compile-time
- ✅ Use pattern matching for evaluation

**DON'T:**
- ❌ Use runtime type checks (use generics)
- ❌ Allow type mismatches at runtime
- ❌ Use object for type erasure (loses safety)
- ❌ Ignore compiler warnings

---

## Summary Checklist

### Type Safety Patterns Compliance

- [ ] **Phantom Types**: Use generic markers for compile-time invariants
- [ ] **Newtype Pattern**: Use readonly structs for semantic types
- [ ] **Exhaustive Matching**: Use sealed hierarchies with switch expressions
- [ ] **No Null**: Use Option<T> instead of null references
- [ ] **Validated Types**: Parse at boundaries, use validated types internally
- [ ] **Type-Level State Machines**: Use phantom types for state transitions
- [ ] **Dependent Types**: Use generic constraints for compile-time type checking

### V12 DNA Compliance Matrix

| Pattern | Lock-Free | Type-Safe | CYC ≤15 | Compile-Time | Validated |
|---------|-----------|-----------|---------|--------------|-----------|
| Phantom Types | ✅ | ✅ | ✅ | ✅ | ✅ |
| Newtype Pattern | ✅ | ✅ | ✅ | ✅ | ✅ |
| Exhaustive Matching | ✅ | ✅ | ✅ | ✅ | ✅ |
| No Null | ✅ | ✅ | ✅ | ✅ | ✅ |
| Validated Types | ✅ | ✅ | ✅ | ⚠️ | ✅ |
| Type-Level FSM | ✅ | ✅ | ✅ | ✅ | ✅ |
| Dependent Types | ✅ | ✅ | ✅ | ✅ | ✅ |

**Legend**: ✅ Full compliance | ⚠️ Acceptable | ❌ Not applicable

---

## References

### Jane Street Resources
- **Firestore KB**: `weeks_making_ocaml_safe_2025` (Making OCaml Safe for Performance Engineering)

### V12 Standards
- [`JANE_STREET_CORE_PATTERNS.md`](./JANE_STREET_CORE_PATTERNS.md) - Result monad, Option type
- [`JANE_STREET_FSM_PATTERNS.md`](./JANE_STREET_FSM_PATTERNS.md) - State machine design
- [`AGENTS.md`](../../AGENTS.md) - Section 2: Correctness by Construction

### Related Documents
- [`JANE_STREET_TESTING_PATTERNS.md`](./JANE_STREET_TESTING_PATTERNS.md) - Type-driven testing
- [`JANE_STREET_CODE_REVIEW.md`](./JANE_STREET_CODE_REVIEW.md) - Type safety review

---

**Document Status**: ✅ Complete (7 patterns documented)  
**Next Review**: 2026-07-03  
**Maintainer**: V12 Architecture Team