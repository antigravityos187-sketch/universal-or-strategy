# EPIC-W7-119 — Phase 4: Implementation Tickets

**Agent:** v12-phase4-tickets
**Wave:** 7 | **Phase:** 4 — Ticket Generation
**Method:** `Dispatch_ProcessFleetLoop` | **Source:** `src/V12_002.SIMA.Dispatch.cs`
**Baseline CYC:** 14 | **Target CYC:** ≤ 8
**ticket_count:** 3

---

## Ticket Summary

| Ticket | Helper | CYC Removed | Projected Helper CYC |
|--------|--------|-------------|----------------------|
| T1 | `ShouldSkipFleetIteration` | 2 | 2 |
| T2 | `Dispatch_RollbackFleetSlot` | 3 | 3 |
| T3 | `Dispatch_HandleFleetSlotException` | 5 | 5 |

**projected_parent_cyc_after_all: 7**

---

## Ticket T1

- **ticket_id:** T1
- **helper_name:** `ShouldSkipFleetIteration`
- **concern:** Circuit-breaker guard — evaluates `_reaperCircuitBreakerTripped` Volatile.Read, appends to dispatchLog on skip. AggressiveInlining hot-path per-iteration predicate, zero-alloc.
- **lines_to_move:** CB tripped guard check from loop body: `Volatile.Read(ref _reaperCircuitBreakerTripped) == 1` branch + log append
- **cyc_reduction:** 2
- **projected_helper_cyc:** 2

## Ticket T2

- **ticket_id:** T2
- **helper_name:** `Dispatch_RollbackFleetSlot`
- **concern:** 5-target rollback — for-loop rollback of activePositions, entryOrders, stopOrders and 2 other target order dicts + null-guard inside catch body. Cold error-recovery path, NoInlining.
- **lines_to_move:** For-loop rollback inside catch body: 5x ConcurrentDictionary TryRemove calls
- **cyc_reduction:** 3
- **projected_helper_cyc:** 3

## Ticket T3

- **ticket_id:** T3
- **helper_name:** `Dispatch_HandleFleetSlotException`
- **concern:** Full catch handler — syncPending rollback, reservedDelta rollback, registeredForCleanup cleanup (delegates to Dispatch_RollbackFleetSlot), FSM cleanup, log append. Removes 4 if-guards from parent catch. Cold error path, NoInlining.
- **lines_to_move:** Entire catch body from Dispatch_ProcessFleetLoop: syncPending rollback + reservedDelta rollback + cleanup + FSM cleanup + log
- **cyc_reduction:** 5
- **projected_helper_cyc:** 5

---

## projected_parent_cyc_after_all: 7

Parent `Dispatch_ProcessFleetLoop` retains: base + for loop + ShouldSkipFleetIteration guard + alloc + register + main dispatch + catch(delegates to T3). CYC = 7.

---

## Agent Tracking

| Field | Value |
|-------|-------|
| Agent Name | v12-phase4-tickets |
| Bobcoins Used | 0.6 |
| Execution Time | 2026-06-29T23:00:00Z |
| Wave | 7 |
| Epic | EPIC-W7-119 |
