# EPIC-W7-033 — Phase 4: Implementation Tickets

**Agent:** v12-phase4-tickets
**Wave:** 7 | **Phase:** 4 — Ticket Generation
**Method:** `FlattenSinglePosition` | **Source:** `src/V12_002.Orders.Management.Flatten.cs`
**Baseline CYC:** 27 | **Target CYC:** ≤ 8
**ticket_count:** 5

---

## Ticket Summary

| Ticket | Helper | CYC Removed | Projected Helper CYC |
|--------|--------|-------------|----------------------|
| T1 | `ClearPendingStopOrders` | 2 | 2 |
| T2 | `CancelAllTargetOrders` | 5 | 5 |
| T3 | `IsOrderCancellable` | 3 | 4 |
| T4 | `ResolveFlattenQuantity` | 4 | 5 |
| T5 | `SubmitFlattenMarketOrder` | 4 | 4 |

**projected_parent_cyc_after_all: 1**

---

## Ticket T1

- **ticket_id:** T1
- **helper_name:** `ClearPendingStopOrders`
- **concern:** Stop-state cleanup — `RequestStopCancelLifecycleSafe()` call and `pendingStopReplacements.TryRemove()` if-branch
- **lines_to_move:** `RequestStopCancelLifecycleSafe()` invocation + `pendingStopReplacements.TryRemove()` if-branch + associated `Print()` statements
- **cyc_reduction:** 2
- **projected_helper_cyc:** 2

## Ticket T2

- **ticket_id:** T2
- **helper_name:** `CancelAllTargetOrders`
- **concern:** T1-T5 target teardown — for loop over target orders dict, null checks, cancellable predicate call, cancel invocation
- **lines_to_move:** `for(tNum=1..5)` loop + `GetTargetOrdersDictionary()` + null checks + `IsOrderCancellable()` predicate call + `CancelOrderSafe()`
- **cyc_reduction:** 5
- **projected_helper_cyc:** 5

## Ticket T3

- **ticket_id:** T3
- **helper_name:** `IsOrderCancellable`
- **concern:** Order state validity predicate — compound `OrderState == Working || Accepted || Submitted` check
- **lines_to_move:** Compound `OrderState` OR-chain branches extracted from T1-T5 loop condition
- **cyc_reduction:** 3
- **projected_helper_cyc:** 4

## Ticket T4

- **ticket_id:** T4
- **helper_name:** `ResolveFlattenQuantity`
- **concern:** Safe flatten quantity resolution — try/catch for Position.Quantity read, null guards, livePositionQty fallback
- **lines_to_move:** `try/catch` for `Position.Quantity` read + `Position != null` + `MarketPosition != Flat` guards + `livePositionQty > 0` fallback logic
- **cyc_reduction:** 4
- **projected_helper_cyc:** 5

## Ticket T5

- **ticket_id:** T5
- **helper_name:** `SubmitFlattenMarketOrder`
- **concern:** Single submission path — flattenQty guard, direction ternary for Sell/BuyToCover, SubmitOrderUnmanaged call, null guard result
- **lines_to_move:** `flattenQty > 0` guard + `Direction == Long` ternary + `SubmitOrderUnmanaged()` + `flattenOrder == null` null guard + result `Print()`
- **cyc_reduction:** 4
- **projected_helper_cyc:** 4

---

## projected_parent_cyc_after_all: 1

Parent becomes thin orchestrator: `Print()` header + sequential calls to 4 helpers above. Zero decision branches remaining.

---

## Agent Tracking

| Field | Value |
|-------|-------|
| Agent Name | v12-phase4-tickets |
| Bobcoins Used | 0.8 |
| Execution Time | 2026-06-29T23:00:00Z |
| Wave | 7 |
| Epic | EPIC-W7-033 |
