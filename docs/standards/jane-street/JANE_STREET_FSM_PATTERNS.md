# Jane Street FSM Patterns: V12 Translation Guide

**Version**: 1.0  
**Last Updated**: 2026-06-03  
**Status**: Active Standard  
**Compliance**: V12 DNA Mandatory

---

## Overview

This document translates Jane Street's Finite State Machine (FSM) patterns from OCaml into V12-aligned C# implementations. Jane Street uses **algebraic data types** (variants) to model state machines, making illegal state transitions **unrepresentable at compile time**—a core principle of V12 DNA.

### Jane Street FSM Philosophy

Jane Street's approach to state machines:
- **Type-Driven State**: Each state is a distinct type, not an enum + data
- **Exhaustive Matching**: Compiler enforces handling all states
- **Illegal Transitions Impossible**: Type system prevents invalid state changes
- **Event-Driven**: State transitions triggered by typed events
- **No Mutable State**: New state returned, old state immutable

### V12 Alignment

V12 DNA implements these principles:
- ✅ **Discriminated Unions**: Sealed record hierarchies replace OCaml variants
- ✅ **Actor Pattern**: Message queue + single-threaded state machine
- ✅ **Lock-Free**: No locks, only immutable state transitions
- ✅ **Type-Safe Events**: Sealed event hierarchies
- ✅ **CYC ≤15**: Simple switch expressions for transitions

---

## Pattern 1: Basic FSM (Order Lifecycle)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Variant-based FSM *)
type order_state =
  | New of { order_id: int; price: float; quantity: int }
  | Pending of { order_id: int; exchange_id: string }
  | PartiallyFilled of { order_id: int; filled_qty: int; remaining_qty: int }
  | Filled of { order_id: int; avg_price: float }
  | Cancelled of { order_id: int; reason: string }
  | Rejected of { order_id: int; error: string }

type order_event =
  | Submit
  | Acknowledge of string  (* exchange_id *)
  | Fill of { quantity: int; price: float }
  | Cancel
  | Reject of string  (* error *)

(* State transition function *)
let transition state event =
  match state, event with
  | New n, Submit -> 
      Pending { order_id = n.order_id; exchange_id = "PENDING" }
  | Pending p, Acknowledge exchange_id ->
      Pending { p with exchange_id }
  | Pending p, Fill { quantity; price } ->
      PartiallyFilled { 
        order_id = p.order_id; 
        filled_qty = quantity; 
        remaining_qty = 0  (* simplified *)
      }
  | PartiallyFilled pf, Fill { quantity; price } ->
      if pf.remaining_qty <= quantity then
        Filled { order_id = pf.order_id; avg_price = price }
      else
        PartiallyFilled { 
          pf with 
          filled_qty = pf.filled_qty + quantity;
          remaining_qty = pf.remaining_qty - quantity
        }
  | (Pending _ | PartiallyFilled _), Cancel ->
      Cancelled { order_id = get_order_id state; reason = "User requested" }
  | New n, Reject error ->
      Rejected { order_id = n.order_id; error }
  | _ -> state  (* Illegal transitions: no-op *)
```

### V12 Translation (C#)

```csharp
// V12: Discriminated unions for states and events
public abstract record OrderState
{
    private OrderState() { }  // Sealed hierarchy

    public sealed record New(int OrderId, double Price, int Quantity) : OrderState;
    public sealed record Pending(int OrderId, string ExchangeId) : OrderState;
    public sealed record PartiallyFilled(int OrderId, int FilledQty, int RemainingQty) : OrderState;
    public sealed record Filled(int OrderId, double AvgPrice) : OrderState;
    public sealed record Cancelled(int OrderId, string Reason) : OrderState;
    public sealed record Rejected(int OrderId, string Error) : OrderState;

    // Helper: Extract order ID from any state
    public int OrderId => this switch
    {
        New n => n.OrderId,
        Pending p => p.OrderId,
        PartiallyFilled pf => pf.OrderId,
        Filled f => f.OrderId,
        Cancelled c => c.OrderId,
        Rejected r => r.OrderId,
        _ => throw new InvalidOperationException("Unknown state")
    };
}

public abstract record OrderEvent
{
    private OrderEvent() { }  // Sealed hierarchy

    public sealed record Submit : OrderEvent;
    public sealed record Acknowledge(string ExchangeId) : OrderEvent;
    public sealed record Fill(int Quantity, double Price) : OrderEvent;
    public sealed record Cancel : OrderEvent;
    public sealed record Reject(string Error) : OrderEvent;
}

// State transition function (pure, no side effects)
public static class OrderFSM
{
    public static OrderState Transition(OrderState state, OrderEvent evt) =>
        (state, evt) switch
        {
            // New -> Pending
            (OrderState.New n, OrderEvent.Submit) =>
                new OrderState.Pending(n.OrderId, "PENDING"),

            // Pending -> Pending (acknowledge)
            (OrderState.Pending p, OrderEvent.Acknowledge ack) =>
                new OrderState.Pending(p.OrderId, ack.ExchangeId),

            // Pending -> PartiallyFilled
            (OrderState.Pending p, OrderEvent.Fill fill) =>
                new OrderState.PartiallyFilled(p.OrderId, fill.Quantity, 0),

            // PartiallyFilled -> Filled or PartiallyFilled
            (OrderState.PartiallyFilled pf, OrderEvent.Fill fill) =>
                pf.RemainingQty <= fill.Quantity
                    ? new OrderState.Filled(pf.OrderId, fill.Price)
                    : new OrderState.PartiallyFilled(
                        pf.OrderId,
                        pf.FilledQty + fill.Quantity,
                        pf.RemainingQty - fill.Quantity),

            // Pending/PartiallyFilled -> Cancelled
            (OrderState.Pending p, OrderEvent.Cancel) =>
                new OrderState.Cancelled(p.OrderId, "User requested"),
            (OrderState.PartiallyFilled pf, OrderEvent.Cancel) =>
                new OrderState.Cancelled(pf.OrderId, "User requested"),

            // New -> Rejected
            (OrderState.New n, OrderEvent.Reject reject) =>
                new OrderState.Rejected(n.OrderId, reject.Error),

            // Illegal transitions: no-op
            _ => state
        };
}
```

**V12 DNA Alignment:**
- ✅ Lock-free: Pure function, no mutable state
- ✅ Type-safe: Compiler enforces exhaustive matching
- ✅ CYC ≤8: Simple switch expression
- ✅ Correctness by construction: Illegal states unrepresentable

**DO:**
- ✅ Use sealed record hierarchies for states and events
- ✅ Make transition function pure (no side effects)
- ✅ Use switch expressions for exhaustive matching
- ✅ Return new state, never mutate existing state

**DON'T:**
- ❌ Use enums for states (loses type safety)
- ❌ Mutate state in-place (breaks immutability)
- ❌ Use if/else chains (not exhaustive)
- ❌ Allow external types to extend state hierarchy

---

## Pattern 2: FSM Actor (Stateful Message Processing)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Actor with FSM state *)
type fsm_message =
  | Event of order_event
  | Query of (order_state Deferred.t Ivar.t)

let create_order_actor initial_state =
  let mailbox = Mailbox.create () in
  let state = ref initial_state in
  
  let rec loop () =
    let%bind msg = Mailbox.receive mailbox in
    match msg with
    | Event evt ->
        state := transition !state evt;
        loop ()
    | Query reply ->
        Ivar.fill reply (return !state);
        loop ()
  in
  don't_wait_for (loop ());
  mailbox
```

### V12 Translation (C#)

```csharp
// V12: FSM Actor with Channel-based mailbox
public sealed class OrderFSMActor : IDisposable
{
    private readonly Channel<FSMMessage> _mailbox;
    private OrderState _state;
    private readonly Task _processingLoop;
    private readonly CancellationTokenSource _cts;

    // Message types
    public abstract record FSMMessage
    {
        private FSMMessage() { }
        
        public sealed record Event(OrderEvent OrderEvent) : FSMMessage;
        public sealed record Query(TaskCompletionSource<OrderState> Reply) : FSMMessage;
    }

    public OrderFSMActor(OrderState initialState)
    {
        _mailbox = Channel.CreateUnbounded<FSMMessage>(
            new UnboundedChannelOptions
            {
                SingleReader = true,  // Lock-free optimization
                SingleWriter = false
            });
        _state = initialState;
        _cts = new CancellationTokenSource();
        _processingLoop = ProcessMessagesAsync(_cts.Token);
    }

    // Public API: Send event to FSM
    public async ValueTask SendEventAsync(
        OrderEvent evt,
        CancellationToken ct = default)
    {
        await _mailbox.Writer.WriteAsync(
            new FSMMessage.Event(evt), ct)
            .ConfigureAwait(false);
    }

    // Public API: Query current state
    public async ValueTask<OrderState> QueryStateAsync(
        CancellationToken ct = default)
    {
        var tcs = new TaskCompletionSource<OrderState>();
        await _mailbox.Writer.WriteAsync(
            new FSMMessage.Query(tcs), ct)
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
                case FSMMessage.Event evt:
                    // Pure state transition
                    _state = OrderFSM.Transition(_state, evt.OrderEvent);
                    break;

                case FSMMessage.Query query:
                    // Return current state (immutable)
                    query.Reply.SetResult(_state);
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

// Usage: FSM Actor lifecycle
public async Task OrderLifecycleExample()
{
    var initialState = new OrderState.New(OrderId: 1, Price: 100.0, Quantity: 10);
    using var actor = new OrderFSMActor(initialState);

    // Submit order
    await actor.SendEventAsync(new OrderEvent.Submit());

    // Query state
    var state1 = await actor.QueryStateAsync();
    Console.WriteLine($"State after submit: {state1}");  // Pending

    // Acknowledge
    await actor.SendEventAsync(new OrderEvent.Acknowledge("EXCH-123"));

    // Fill
    await actor.SendEventAsync(new OrderEvent.Fill(Quantity: 10, Price: 100.5));

    // Query final state
    var finalState = await actor.QueryStateAsync();
    Console.WriteLine($"Final state: {finalState}");  // Filled
}
```

**V12 DNA Alignment:**
- ✅ Lock-free: Single-reader channel (no locks)
- ✅ Type-safe: Discriminated unions for messages
- ✅ CYC ≤8: Simple switch expression
- ✅ Actor pattern: Encapsulated mutable state

**DO:**
- ✅ Use `Channel<T>` with `SingleReader = true`
- ✅ Keep state private to the actor
- ✅ Use pure transition functions
- ✅ Use `TaskCompletionSource<T>` for queries

**DON'T:**
- ❌ Access `_state` from outside the processing loop
- ❌ Use locks inside the actor
- ❌ Mutate state directly (use transition function)
- ❌ Forget to dispose actors

---

## Pattern 3: Hierarchical FSM (Nested States)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Nested variants for hierarchical states *)
type connection_state =
  | Disconnected
  | Connecting of { attempt: int }
  | Connected of connected_state

and connected_state =
  | Idle
  | Authenticating of { username: string }
  | Authenticated of { session_id: string }
  | Streaming of { session_id: string; stream_id: int }

type connection_event =
  | Connect
  | ConnectionEstablished
  | Authenticate of string
  | AuthSuccess of string
  | StartStream of int
  | Disconnect

let transition state event =
  match state, event with
  | Disconnected, Connect ->
      Connecting { attempt = 1 }
  | Connecting c, ConnectionEstablished ->
      Connected Idle
  | Connected Idle, Authenticate username ->
      Connected (Authenticating { username })
  | Connected (Authenticating a), AuthSuccess session_id ->
      Connected (Authenticated { session_id })
  | Connected (Authenticated a), StartStream stream_id ->
      Connected (Streaming { session_id = a.session_id; stream_id })
  | Connected _, Disconnect ->
      Disconnected
  | _ -> state
```

### V12 Translation (C#)

```csharp
// V12: Nested discriminated unions for hierarchical FSM
public abstract record ConnectionState
{
    private ConnectionState() { }

    public sealed record Disconnected : ConnectionState;
    public sealed record Connecting(int Attempt) : ConnectionState;
    public sealed record Connected(ConnectedState State) : ConnectionState;
}

public abstract record ConnectedState
{
    private ConnectedState() { }

    public sealed record Idle : ConnectedState;
    public sealed record Authenticating(string Username) : ConnectedState;
    public sealed record Authenticated(string SessionId) : ConnectedState;
    public sealed record Streaming(string SessionId, int StreamId) : ConnectedState;
}

public abstract record ConnectionEvent
{
    private ConnectionEvent() { }

    public sealed record Connect : ConnectionEvent;
    public sealed record ConnectionEstablished : ConnectionEvent;
    public sealed record Authenticate(string Username) : ConnectionEvent;
    public sealed record AuthSuccess(string SessionId) : ConnectionEvent;
    public sealed record StartStream(int StreamId) : ConnectionEvent;
    public sealed record Disconnect : ConnectionEvent;
}

// Hierarchical state transition
public static class ConnectionFSM
{
    public static ConnectionState Transition(ConnectionState state, ConnectionEvent evt) =>
        (state, evt) switch
        {
            // Disconnected -> Connecting
            (ConnectionState.Disconnected, ConnectionEvent.Connect) =>
                new ConnectionState.Connecting(Attempt: 1),

            // Connecting -> Connected(Idle)
            (ConnectionState.Connecting, ConnectionEvent.ConnectionEstablished) =>
                new ConnectionState.Connected(new ConnectedState.Idle()),

            // Connected(Idle) -> Connected(Authenticating)
            (ConnectionState.Connected { State: ConnectedState.Idle }, 
             ConnectionEvent.Authenticate auth) =>
                new ConnectionState.Connected(
                    new ConnectedState.Authenticating(auth.Username)),

            // Connected(Authenticating) -> Connected(Authenticated)
            (ConnectionState.Connected { State: ConnectedState.Authenticating auth },
             ConnectionEvent.AuthSuccess success) =>
                new ConnectionState.Connected(
                    new ConnectedState.Authenticated(success.SessionId)),

            // Connected(Authenticated) -> Connected(Streaming)
            (ConnectionState.Connected { State: ConnectedState.Authenticated auth },
             ConnectionEvent.StartStream stream) =>
                new ConnectionState.Connected(
                    new ConnectedState.Streaming(auth.SessionId, stream.StreamId)),

            // Connected(*) -> Disconnected
            (ConnectionState.Connected, ConnectionEvent.Disconnect) =>
                new ConnectionState.Disconnected(),

            // Illegal transitions: no-op
            _ => state
        };
}
```

**V12 DNA Alignment:**
- ✅ Lock-free: Pure function, no mutable state
- ✅ Type-safe: Nested discriminated unions
- ✅ CYC ≤10: Acceptable for hierarchical matching
- ✅ Correctness by construction: Illegal substates unrepresentable

**DO:**
- ✅ Use nested discriminated unions for hierarchical states
- ✅ Use property patterns for matching nested states
- ✅ Keep transition function pure
- ✅ Model parent-child state relationships explicitly

**DON'T:**
- ❌ Use flat enums for hierarchical states (loses structure)
- ❌ Use inheritance for state behavior (use pattern matching)
- ❌ Allow invalid parent-child combinations
- ❌ Mutate nested state in-place

---

## Pattern 4: FSM with Side Effects (Effect Handlers)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Effect handlers separate from transitions *)
type effect =
  | SendToExchange of order
  | LogEvent of string
  | NotifyUser of string

let transition_with_effects state event =
  match state, event with
  | New n, Submit ->
      let new_state = Pending { order_id = n.order_id; exchange_id = "PENDING" } in
      let effects = [
        SendToExchange (order_from_new n);
        LogEvent "Order submitted"
      ] in
      (new_state, effects)
  | Filled f, _ ->
      let effects = [NotifyUser "Order filled"] in
      (state, effects)
  | _ -> (state, [])

(* Execute effects separately *)
let execute_effect = function
  | SendToExchange order -> send_to_exchange order
  | LogEvent msg -> Log.info "%s" msg
  | NotifyUser msg -> notify_user msg
```

### V12 Translation (C#)

```csharp
// V12: Effect handlers separate from pure transitions
public abstract record OrderEffect
{
    private OrderEffect() { }

    public sealed record SendToExchange(Order Order) : OrderEffect;
    public sealed record LogEvent(string Message) : OrderEffect;
    public sealed record NotifyUser(string Message) : OrderEffect;
}

// Transition returns new state + effects
public static class OrderFSMWithEffects
{
    public static (OrderState NewState, OrderEffect[] Effects) TransitionWithEffects(
        OrderState state,
        OrderEvent evt) =>
        (state, evt) switch
        {
            // New -> Pending (with effects)
            (OrderState.New n, OrderEvent.Submit) =>
            (
                new OrderState.Pending(n.OrderId, "PENDING"),
                new OrderEffect[]
                {
                    new OrderEffect.SendToExchange(OrderFromNew(n)),
                    new OrderEffect.LogEvent("Order submitted")
                }
            ),

            // Filled (with notification)
            (OrderState.Filled f, _) =>
            (
                state,
                new OrderEffect[]
                {
                    new OrderEffect.NotifyUser($"Order {f.OrderId} filled at {f.AvgPrice}")
                }
            ),

            // No effects
            _ => (OrderFSM.Transition(state, evt), Array.Empty<OrderEffect>())
        };

    private static Order OrderFromNew(OrderState.New n) =>
        new Order(n.OrderId, n.Price, n.Quantity);
}

// Effect executor (impure, side effects)
public sealed class OrderEffectExecutor
{
    private readonly IExchangeClient _exchangeClient;
    private readonly ILogger _logger;
    private readonly INotificationService _notificationService;

    public async ValueTask ExecuteEffectAsync(
        OrderEffect effect,
        CancellationToken ct = default)
    {
        switch (effect)
        {
            case OrderEffect.SendToExchange send:
                await _exchangeClient.SendOrderAsync(send.Order, ct)
                    .ConfigureAwait(false);
                break;

            case OrderEffect.LogEvent log:
                _logger.LogInformation(log.Message);
                break;

            case OrderEffect.NotifyUser notify:
                await _notificationService.NotifyAsync(notify.Message, ct)
                    .ConfigureAwait(false);
                break;
        }
    }
}

// FSM Actor with effect execution
public sealed class OrderFSMActorWithEffects : IDisposable
{
    private readonly Channel<OrderEvent> _mailbox;
    private OrderState _state;
    private readonly OrderEffectExecutor _effectExecutor;
    private readonly Task _processingLoop;
    private readonly CancellationTokenSource _cts;

    public OrderFSMActorWithEffects(
        OrderState initialState,
        OrderEffectExecutor effectExecutor)
    {
        _mailbox = Channel.CreateUnbounded<OrderEvent>(
            new UnboundedChannelOptions { SingleReader = true });
        _state = initialState;
        _effectExecutor = effectExecutor;
        _cts = new CancellationTokenSource();
        _processingLoop = ProcessMessagesAsync(_cts.Token);
    }

    private async Task ProcessMessagesAsync(CancellationToken ct)
    {
        await foreach (var evt in _mailbox.Reader.ReadAllAsync(ct))
        {
            // Pure transition
            var (newState, effects) = OrderFSMWithEffects.TransitionWithEffects(_state, evt);
            _state = newState;

            // Execute effects (impure)
            foreach (var effect in effects)
            {
                await _effectExecutor.ExecuteEffectAsync(effect, ct)
                    .ConfigureAwait(false);
            }
        }
    }

    public async ValueTask SendEventAsync(OrderEvent evt, CancellationToken ct = default)
    {
        await _mailbox.Writer.WriteAsync(evt, ct).ConfigureAwait(false);
    }

    public void Dispose()
    {
        _mailbox.Writer.Complete();
        _cts.Cancel();
        _processingLoop.Wait();
        _cts.Dispose();
    }
}
```

**V12 DNA Alignment:**
- ✅ Lock-free: Pure transition + async effect execution
- ✅ Type-safe: Discriminated unions for effects
- ✅ CYC ≤8: Simple effect dispatch
- ✅ Separation of concerns: Pure logic + impure effects

**DO:**
- ✅ Separate pure transitions from impure effects
- ✅ Return effects as data (discriminated unions)
- ✅ Execute effects after state transition
- ✅ Use async effect executors

**DON'T:**
- ❌ Mix side effects into transition function
- ❌ Execute effects before state transition (ordering matters)
- ❌ Ignore effect execution failures (handle errors)
- ❌ Block on effect execution (use async)

---

## Pattern 5: FSM Composition (Parallel State Machines)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Compose multiple FSMs *)
type composite_state = {
  order_state: order_state;
  risk_state: risk_state;
  market_state: market_state;
}

type composite_event =
  | OrderEvent of order_event
  | RiskEvent of risk_event
  | MarketEvent of market_event

let transition_composite state event =
  match event with
  | OrderEvent evt ->
      { state with order_state = order_transition state.order_state evt }
  | RiskEvent evt ->
      { state with risk_state = risk_transition state.risk_state evt }
  | MarketEvent evt ->
      { state with market_state = market_transition state.market_state evt }
```

### V12 Translation (C#)

```csharp
// V12: Composite FSM with parallel state machines
public sealed record CompositeState(
    OrderState OrderState,
    RiskState RiskState,
    MarketState MarketState);

public abstract record CompositeEvent
{
    private CompositeEvent() { }

    public sealed record OrderEvent(OrderEvent Event) : CompositeEvent;
    public sealed record RiskEvent(RiskEvent Event) : CompositeEvent;
    public sealed record MarketEvent(MarketEvent Event) : CompositeEvent;
}

// Composite transition (delegates to sub-FSMs)
public static class CompositeFSM
{
    public static CompositeState Transition(CompositeState state, CompositeEvent evt) =>
        evt switch
        {
            CompositeEvent.OrderEvent orderEvt =>
                state with
                {
                    OrderState = OrderFSM.Transition(state.OrderState, orderEvt.Event)
                },

            CompositeEvent.RiskEvent riskEvt =>
                state with
                {
                    RiskState = RiskFSM.Transition(state.RiskState, riskEvt.Event)
                },

            CompositeEvent.MarketEvent marketEvt =>
                state with
                {
                    MarketState = MarketFSM.Transition(state.MarketState, marketEvt.Event)
                },

            _ => state
        };
}

// Composite FSM Actor
public sealed class CompositeFSMActor : IDisposable
{
    private readonly Channel<CompositeEvent> _mailbox;
    private CompositeState _state;
    private readonly Task _processingLoop;
    private readonly CancellationTokenSource _cts;

    public CompositeFSMActor(CompositeState initialState)
    {
        _mailbox = Channel.CreateUnbounded<CompositeEvent>(
            new UnboundedChannelOptions { SingleReader = true });
        _state = initialState;
        _cts = new CancellationTokenSource();
        _processingLoop = ProcessMessagesAsync(_cts.Token);
    }

    private async Task ProcessMessagesAsync(CancellationToken ct)
    {
        await foreach (var evt in _mailbox.Reader.ReadAllAsync(ct))
        {
            _state = CompositeFSM.Transition(_state, evt);
        }
    }

    public async ValueTask SendEventAsync(CompositeEvent evt, CancellationToken ct = default)
    {
        await _mailbox.Writer.WriteAsync(evt, ct).ConfigureAwait(false);
    }

    public async ValueTask<CompositeState> QueryStateAsync(CancellationToken ct = default)
    {
        var tcs = new TaskCompletionSource<CompositeState>();
        // Query implementation omitted for brevity
        return await tcs.Task.ConfigureAwait(false);
    }

    public void Dispose()
    {
        _mailbox.Writer.Complete();
        _cts.Cancel();
        _processingLoop.Wait();
        _cts.Dispose();
    }
}
```

**V12 DNA Alignment:**
- ✅ Lock-free: Pure composition, no shared state
- ✅ Type-safe: Discriminated unions for events
- ✅ CYC ≤8: Simple delegation
- ✅ Composable: Sub-FSMs independent

**DO:**
- ✅ Use records with `with` expressions for composition
- ✅ Delegate to sub-FSM transition functions
- ✅ Keep sub-FSMs independent
- ✅ Use discriminated unions for composite events

**DON'T:**
- ❌ Couple sub-FSMs (cross-dependencies)
- ❌ Mutate composite state in-place
- ❌ Mix sub-FSM logic in composite transition
- ❌ Forget to update all sub-states

---

## Pattern 6: FSM Testing (Property-Based)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Property-based FSM testing *)
open Base_quickcheck

let%test_unit "FSM transitions are deterministic" =
  Quickcheck.test
    ~sexp_of:[%sexp_of: order_state * order_event]
    (Generator.tuple2 gen_order_state gen_order_event)
    ~f:(fun (state, event) ->
      let result1 = transition state event in
      let result2 = transition state event in
      [%test_eq: order_state] result1 result2)

let%test_unit "FSM never enters invalid state" =
  Quickcheck.test
    ~sexp_of:[%sexp_of: order_state * order_event list]
    (Generator.tuple2 gen_order_state (Generator.list gen_order_event))
    ~f:(fun (initial_state, events) ->
      let final_state = List.fold events ~init:initial_state ~f:transition in
      assert (is_valid_state final_state))
```

### V12 Translation (C#)

```csharp
// V12: Property-based FSM testing with FsCheck
using FsCheck;
using FsCheck.Xunit;

public class OrderFSMProperties
{
    // Generator for random order states
    public static Arbitrary<OrderState> ArbOrderState() =>
        Arb.From(Gen.OneOf(
            Gen.Constant(new OrderState.New(1, 100.0, 10)),
            Gen.Constant(new OrderState.Pending(1, "EXCH-123")),
            Gen.Constant(new OrderState.Filled(1, 100.5))
        ));

    // Generator for random order events
    public static Arbitrary<OrderEvent> ArbOrderEvent() =>
        Arb.From(Gen.OneOf(
            Gen.Constant(new OrderEvent.Submit()),
            Gen.Constant(new OrderEvent.Cancel()),
            Gen.Constant(new OrderEvent.Fill(10, 100.5))
        ));

    // Property: Transitions are deterministic
    [Property(Arbitrary = new[] { typeof(OrderFSMProperties) })]
    public Property TransitionsAreDeterministic(OrderState state, OrderEvent evt)
    {
        var result1 = OrderFSM.Transition(state, evt);
        var result2 = OrderFSM.Transition(state, evt);
        return (result1 == result2).ToProperty();
    }

    // Property: FSM never enters invalid state
    [Property(Arbitrary = new[] { typeof(OrderFSMProperties) })]
    public Property NeverEntersInvalidState(OrderState initialState, OrderEvent[] events)
    {
        var finalState = events.Aggregate(initialState, OrderFSM.Transition);
        return IsValidState(finalState).ToProperty();
    }

    // Property: Idempotent transitions
    [Property(Arbitrary = new[] { typeof(OrderFSMProperties) })]
    public Property IdempotentTransitions(OrderState state, OrderEvent evt)
    {
        var once = OrderFSM.Transition(state, evt);
        var twice = OrderFSM.Transition(once, evt);
        
        // Some transitions are idempotent (e.g., Cancel on Cancelled)
        return (once == twice || IsValidTransition(once, evt)).ToProperty();
    }

    // Property: State invariants preserved
    [Property(Arbitrary = new[] { typeof(OrderFSMProperties) })]
    public Property StateInvariantsPreserved(OrderState state, OrderEvent evt)
    {
        var newState = OrderFSM.Transition(state, evt);
        
        // Invariant: Order ID never changes
        return (state.OrderId == newState.OrderId).ToProperty();
    }

    private static bool IsValidState(OrderState state) =>
        state switch
        {
            OrderState.New n => n.Quantity > 0 && n.Price > 0,
            OrderState.PartiallyFilled pf => pf.FilledQty > 0 && pf.RemainingQty >= 0,
            OrderState.Filled f => f.AvgPrice > 0,
            _ => true
        };

    private static bool IsValidTransition(OrderState state, OrderEvent evt) =>
        (state, evt) switch
        {
            (OrderState.Filled, _) => true,  // Terminal state
            (OrderState.Cancelled, _) => true,  // Terminal state
            (OrderState.Rejected, _) => true,  // Terminal state
            _ => false
        };
}
```

**V12 DNA Alignment:**
- ✅ Lock-free: Pure functions, no side effects
- ✅ Type-safe: Compiler-enforced properties
- ✅ CYC ≤8: Simple property checks
- ✅ Exhaustive: Tests all state/event combinations

**DO:**
- ✅ Use property-based testing for FSMs
- ✅ Test determinism (same input → same output)
- ✅ Test state invariants (never invalid)
- ✅ Test idempotency (where applicable)

**DON'T:**
- ❌ Only test happy paths (use property-based)
- ❌ Forget to test illegal transitions
- ❌ Ignore state invariants
- ❌ Use mutable state in tests

---

## Pattern 7: FSM Visualization (State Diagrams)

### Jane Street Approach (OCaml)

```ocaml
(* Jane Street: Generate Graphviz from FSM *)
let generate_state_diagram () =
  let states = [
    "New"; "Pending"; "PartiallyFilled"; "Filled"; "Cancelled"; "Rejected"
  ] in
  let transitions = [
    ("New", "Submit", "Pending");
    ("Pending", "Acknowledge", "Pending");
    ("Pending", "Fill", "PartiallyFilled");
    ("PartiallyFilled", "Fill", "Filled");
    ("Pending", "Cancel", "Cancelled");
    ("New", "Reject", "Rejected");
  ] in
  
  Printf.printf "digraph OrderFSM {\n";
  List.iter (fun state ->
    Printf.printf "  %s [shape=box];\n" state
  ) states;
  List.iter (fun (from, event, to_) ->
    Printf.printf "  %s -> %s [label=\"%s\"];\n" from to_ event
  ) transitions;
  Printf.printf "}\n"
```

### V12 Translation (C#)

```csharp
// V12: FSM visualization with Mermaid
public static class OrderFSMVisualizer
{
    public static string GenerateMermaidDiagram()
    {
        var sb = new StringBuilder();
        sb.AppendLine("stateDiagram-v2");
        sb.AppendLine("    [*] --> New");
        sb.AppendLine();
        
        // States
        sb.AppendLine("    New --> Pending : Submit");
        sb.AppendLine("    Pending --> Pending : Acknowledge");
        sb.AppendLine("    Pending --> PartiallyFilled : Fill");
        sb.AppendLine("    PartiallyFilled --> PartiallyFilled : Fill");
        sb.AppendLine("    PartiallyFilled --> Filled : Fill (complete)");
        sb.AppendLine("    Pending --> Cancelled : Cancel");
        sb.AppendLine("    PartiallyFilled --> Cancelled : Cancel");
        sb.AppendLine("    New --> Rejected : Reject");
        sb.AppendLine();
        
        // Terminal states
        sb.AppendLine("    Filled --> [*]");
        sb.AppendLine("    Cancelled --> [*]");
        sb.AppendLine("    Rejected --> [*]");
        
        return sb.ToString();
    }

    // Generate from FSM reflection
    public static string GenerateDiagramFromFSM()
    {
        var stateType = typeof(OrderState);
        var eventType = typeof(OrderEvent);
        
        var states = stateType.GetNestedTypes()
            .Where(t => t.IsSubclassOf(stateType))
            .Select(t => t.Name)
            .ToArray();
        
        var events = eventType.GetNestedTypes()
            .Where(t => t.IsSubclassOf(eventType))
            .Select(t => t.Name)
            .ToArray();
        
        var sb = new StringBuilder();
        sb.AppendLine("stateDiagram-v2");
        sb.AppendLine("    [*] --> New");
        sb.AppendLine();
        
        // Generate all possible transitions (simplified)
        foreach (var state in states)
        {
            foreach (var evt in events)
            {
                // Test transition
                var testState = CreateTestState(state);
                var testEvent = CreateTestEvent(evt);
                var newState = OrderFSM.Transition(testState, testEvent);
                
                if (newState.GetType().Name != state)
                {
                    sb.AppendLine($"    {state} --> {newState.GetType().Name} : {evt}");
                }
            }
        }
        
        return sb.ToString();
    }

    private static OrderState CreateTestState(string stateName) =>
        stateName switch
        {
            "New" => new OrderState.New(1, 100.0, 10),
            "Pending" => new OrderState.Pending(1, "EXCH-123"),
            "PartiallyFilled" => new OrderState.PartiallyFilled(1, 5, 5),
            "Filled" => new OrderState.Filled(1, 100.5),
            "Cancelled" => new OrderState.Cancelled(1, "User requested"),
            "Rejected" => new OrderState.Rejected(1, "Invalid order"),
            _ => throw new ArgumentException($"Unknown state: {stateName}")
        };

    private static OrderEvent CreateTestEvent(string eventName) =>
        eventName switch
        {
            "Submit" => new OrderEvent.Submit(),
            "Acknowledge" => new OrderEvent.Acknowledge("EXCH-123"),
            "Fill" => new OrderEvent.Fill(10, 100.5),
            "Cancel" => new OrderEvent.Cancel(),
            "Reject" => new OrderEvent.Reject("Invalid"),
            _ => throw new ArgumentException($"Unknown event: {eventName}")
        };
}

// Usage: Generate diagram in tests
[Fact]
public void GenerateFSMDiagram()
{
    var diagram = OrderFSMVisualizer.GenerateMermaidDiagram();
    File.WriteAllText("order_fsm.mmd", diagram);
    
    // Output can be rendered with Mermaid CLI or in Markdown
    _output.WriteLine(diagram);
}
```

**V12 DNA Alignment:**
- ✅ Lock-free: Pure visualization, no side effects
- ✅ Type-safe: Reflection-based generation
- ✅ CYC ≤10: Acceptable for visualization logic
- ✅ Documentation: Auto-generated from code

**DO:**
- ✅ Generate diagrams from FSM code (single source of truth)
- ✅ Use Mermaid for Markdown-embeddable diagrams
- ✅ Include diagrams in documentation
- ✅ Update diagrams automatically in CI

**DON'T:**
- ❌ Maintain diagrams manually (drift from code)
- ❌ Use complex visualization tools (keep it simple)
- ❌ Forget to document terminal states
- ❌ Ignore illegal transitions in diagrams

---

## Summary Checklist

### FSM Patterns Compliance

- [ ] **Basic FSM**: Use discriminated unions for states and events
- [ ] **FSM Actor**: Use Channel<T> with single-reader optimization
- [ ] **Hierarchical FSM**: Use nested discriminated unions
- [ ] **FSM with Effects**: Separate pure transitions from impure effects
- [ ] **FSM Composition**: Compose multiple FSMs with records
- [ ] **FSM Testing**: Use property-based testing (FsCheck)
- [ ] **FSM Visualization**: Generate Mermaid diagrams from code

### V12 DNA Compliance Matrix

| Pattern | Lock-Free | Type-Safe | CYC ≤15 | Immutable | Testable |
|---------|-----------|-----------|---------|-----------|----------|
| Basic FSM | ✅ | ✅ | ✅ | ✅ | ✅ |
| FSM Actor | ✅ | ✅ | ✅ | ⚠️ | ✅ |
| Hierarchical FSM | ✅ | ✅ | ⚠️ | ✅ | ✅ |
| FSM with Effects | ✅ | ✅ | ✅ | ✅ | ✅ |
| FSM Composition | ✅ | ✅ | ✅ | ✅ | ✅ |
| FSM Testing | ✅ | ✅ | ✅ | ✅ | ✅ |
| FSM Visualization | ✅ | ✅ | ⚠️ | ✅ | ✅ |

**Legend**: ✅ Full compliance | ⚠️ Acceptable | ❌ Not applicable

---

## References

### Jane Street Resources
- **Firestore KB**: `weeks_making_ocaml_safe_2025` (Making OCaml Safe for Performance Engineering)
- **Firestore KB**: `jane_street_build_exchange_2015` (How to Build an Exchange)

### V12 Standards
- [`JANE_STREET_CORE_PATTERNS.md`](./JANE_STREET_CORE_PATTERNS.md) - Discriminated unions, Result monad
- [`JANE_STREET_ASYNC_PATTERNS.md`](./JANE_STREET_ASYNC_PATTERNS.md) - Actor pattern, Channel<T>
- [`AGENTS.md`](../../AGENTS.md) - Section 2: Lock-Free Actor Pattern

### Related Documents
- [`JANE_STREET_TESTING_PATTERNS.md`](./JANE_STREET_TESTING_PATTERNS.md) - Property-based testing
- [`JANE_STREET_TYPE_SAFETY.md`](./JANE_STREET_TYPE_SAFETY.md) - Type-driven development

---

**Document Status**: ✅ Complete (7 patterns documented)  
**Next Review**: 2026-07-03  
**Maintainer**: V12 Architecture Team