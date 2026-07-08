# EPIC-W7-089 — Phase 0: Hotspot Analysis
## Method: `CancelWatchdogWorkingOrders`
**Source:** `src/V12_002.Safety.Watchdog.cs` | Lines 138–165
**Wave:** 7 | **Phase:** 0 | **CYC Confirmed:** 10

---

## 1. Method Signature

```csharp
private void CancelWatchdogWorkingOrders(Account masterAccount, string instrumentName)
```

Called exclusively from [`ExecuteWatchdogLeadAccountFlatten()`](../../src/V12_002.Safety.Watchdog.cs:229)
inside an `EnterFlattenScope` / `ExitFlattenScope` guard.

---

## 2. Cyclomatic Complexity Breakdown (CYC = 10)

| # | Construct | Line(s) | Decision point |
|---|-----------|---------|----------------|
| 1 | Base path | — | method entry |
| 2 | `foreach` over `masterAccount.Orders.ToArray()` | 142 | loop iteration |
| 3 | `if (order == null \|\| order.Instrument == null)` | 144–145 | null guard (continue) |
| 4 | `if (order.Instrument.FullName != instrumentName)` | 146–147 | instrument filter (continue) |
| 5 | `order.OrderState == OrderState.Working` | 149 | OR-chain branch 1 |
| 6 | `\|\| order.OrderState == OrderState.Submitted` | 150 | OR-chain branch 2 |
| 7 | `\|\| order.OrderState == OrderState.Accepted` | 151 | OR-chain branch 3 |
| 8 | `\|\| order.OrderState == OrderState.ChangePending` | 152 | OR-chain branch 4 |
| 9 | `\|\| order.OrderState == OrderState.ChangeSubmitted` | 153 | OR-chain branch 5 |
| 10 | `foreach` over `ordersToCancel` (second loop) | 160 | loop iteration |

> The 5-way `OrderState` OR-chain (decisions 5–9) is the primary complexity driver.
> Each OR operand is an independent predicate counted by McCabe's standard rule.

---

## 3. Call Graph

```
OnWatchdogTimer()                              [timer thread]
  └─ Enqueue(ctx => ctx.ExecuteWatchdogLeadAccountFlatten())
       └─ ExecuteWatchdogLeadAccountFlatten()  [actor/strategy thread]
            ├─ EnterFlattenScope()
            ├─ CancelWatchdogWorkingOrders()   ◄── TARGET
            │    └─ CancelOrderOnAccount()      [V12_002.Orders.CancelGateway.cs:46]
            │         ├─ IsOrderTerminal()      [V12_002.Orders.Management.Flatten.cs:698]
            │         └─ executingAccount.Cancel() / CancelOrder()
            ├─ FlattenWatchdogPositions()
            ├─ SetExpectedPositionLocked()
            ├─ PublishUiSnapshot()
            └─ ExitFlattenScope()
```

**Escalation path** (if `ExecuteWatchdogLeadAccountFlatten` was already enqueued but stage is still 1):
```
OnWatchdogTimer() [stage==1]
  └─ ExecuteWatchdogDirectFallback()
       └─ CancelDirectFallbackOrders()   [parallel implementation, no CancelOrderOnAccount]
```

---

## 4. Blast Radius

| Symbol | File | Relationship |
|--------|------|--------------|
| `ExecuteWatchdogLeadAccountFlatten` | `Safety.Watchdog.cs:211` | **sole caller** |
| `CancelOrderOnAccount` | `Orders.CancelGateway.cs:46` | terminal cancel dispatch |
| `IsOrderTerminal` | `Orders.Management.Flatten.cs:698` | terminal-state guard inside CancelOrderOnAccount |
| `EnterFlattenScope` / `ExitFlattenScope` | `V12_002.cs:695,701` | flatten scope wrapping caller |
| `SetExpectedPositionLocked` | `V12_002.SIMA.cs:124` | resets expected position after cancels |
| `PublishUiSnapshot` | `V12_002.UI.Snapshot.cs:211` | UI refresh after flatten |
| `_watchdogStage` (int, volatile) | `V12_002.cs:655` | CAS state machine controlling re-entry |
| `_strategyHeartbeatTicks` (long) | `V12_002.cs:188` | heartbeat liveness signal |

**No direct fleet/follower impact** — `CancelOrderOnAccount` routes through `executingAccount.Cancel()` only when `executingAccount != Account` (i.e., fleet followers). This method passes `masterAccount` which equals `Account`; therefore it falls through to NinjaScript's `CancelOrder()`.

---

## 5. Risk & Hotspot Assessment

### 5.1 Thread-Safety Concern (HIGH)
`CancelWatchdogWorkingOrders` is executed on the **strategy/actor thread** (via `Enqueue`).
`masterAccount.Orders` is a NinjaTrader broker collection that can be mutated on background threads.
The `.ToArray()` snapshot (line 142) mitigates concurrent modification during iteration, but
the second `foreach` (line 160) over `ordersToCancel` calls `CancelOrderOnAccount` which may
observe already-terminal orders — this is handled by the `IsOrderTerminal` guard inside
`CancelOrderOnAccount`, but only if the order object is updated synchronously (broker-dependent).

### 5.2 Duplicate-State Enumeration (MEDIUM)
`OrderState.Working`, `Submitted`, `Accepted`, `ChangePending`, `ChangeSubmitted` are checked
individually rather than via a helper like `IsOrderNonTerminal()`. This is the mirror image of
`IsOrderTerminal()` but lacks a shared definition, creating a maintenance risk: if NinjaTrader
adds a new non-terminal state it must be updated in both this method **and**
`CancelDirectFallbackOrders` (lines 279–284), which contains an exact duplicate of the same
5-way OR-chain.

### 5.3 No Logging of Zero-Cancel Case (LOW)
When `ordersToCancel.Count == 0` the method exits silently. The caller
`ExecuteWatchdogLeadAccountFlatten` has already confirmed `HasWatchdogLeadAccountWorkingOrder()`
returned `true`, so this is logically reachable only via a TOCTOU race. Silent exit makes
post-mortem diagnosis harder.

### 5.4 Structural Duplication (MEDIUM)
`CancelWatchdogWorkingOrders` (strategy-thread path) and `CancelDirectFallbackOrders`
(direct fallback path) share identical order-enumeration and state-filtering logic but differ
only in their cancel dispatch (`CancelOrderOnAccount` vs `masterAccount.Cancel(array)`).
Divergence risk is non-trivial given the safety-critical context.

---

## 6. Recommended Refactor Targets (Phase 1+)

1. **Extract `IsOrderCancellable(OrderState)`** — inverse of `IsOrderTerminal`; shared between
   `CancelWatchdogWorkingOrders`, `CancelDirectFallbackOrders`, and any future cancel paths.
2. **Merge enumeration logic** — a private `CollectCancellableOrders(Account, string)` helper
   returning `List<Order>` would remove the duplication between the two fallback paths.
3. **Add trace log** for the zero-cancel path to aid deadlock post-mortems.

---

## 7. Source Coordinates

| Item | Value |
|------|-------|
| File | `src/V12_002.Safety.Watchdog.cs` |
| Method start | Line 138 |
| Method end | Line 165 |
| Build tag | Build 1108.004 |
| Namespace | `NinjaTrader.NinjaScript.Strategies` |
| Class | `V12_002 : Strategy` (partial) |

---

*Generated: Phase 0 — Hotspot Analysis | EPIC-W7-089*
