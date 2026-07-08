# Phase 4: Implementation Tickets — EPIC-W7-103

**Epic:** EPIC-W7-103
**Method:** ProcessFleetSlot
**Source:** src/V12_002.SIMA.Fleet.cs
**Original CYC:** 13
**Wave:** 7 | **Phase:** 4 — Ticket Generation

---

## ticket_count: 3

---

## Ticket 1

- **ticket_id:** 1
- **helper_name:** ExecuteDispatchCore
- **concern:** Happy-path dispatch sequence: validate timestamp with early-exit guard, initialize follower bracket FSM, submit and register fleet orders
- **lines_to_move:** The try-body of ProcessFleetSlot — ValidateDispatchTimestamp early-exit guard, InitializeFollowerBracketFSM call, SubmitAndRegisterFleetOrders call; takes ref bool syncCleared to expose state mutation at call boundary
- **cyc_reduction:** 4
- **projected_helper_cyc:** 2

---

## Ticket 2

- **ticket_id:** 2
- **helper_name:** HandleDispatchFailure
- **concern:** Catch-path compensation: log exception, conditionally clear dispatch sync pending, conditionally reverse reserved position delta, rollback fleet dispatch state
- **lines_to_move:** The catch(Exception ex) body — diagnostic log, conditional ClearDispatchSyncPending, conditional AddExpectedPositionDeltaLocked reversal, RollbackFleetDispatchState
- **cyc_reduction:** 3
- **projected_helper_cyc:** 3

---

## Ticket 3

- **ticket_id:** 3
- **helper_name:** TryRepumpIfQueued
- **concern:** Check whether photon dispatch ring or pending fleet dispatch queue is non-empty; if so re-trigger PumpFleetDispatch via TriggerCustomEvent with defensive try/catch and diagnostic logging
- **lines_to_move:** Re-pump logic in finally block — compound queue-check condition, TriggerCustomEvent call, defensive try/catch, diagnostic log on exception
- **cyc_reduction:** 4
- **projected_helper_cyc:** 5

---

## projected_parent_cyc_after_all: 5

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Epic** | EPIC-W7-103 |
| **Wave** | 7 |
| **Phase** | 4 — Ticket Generation |
| **Bobcoins Used** | 1.8 |
| **Execution Time** | 2026-06-29T01:30:00Z |
| **jcodemunch tools called** | resolve_repo, get_symbol_complexity, get_extraction_candidates |
| **sequential-thinking calls** | 4 (1 probe + 3 ticket thoughts) |
| **ticket_count** | 3 |
| **projected_parent_cyc_after_all** | 5 |
| **Original CYC** | 13 |
