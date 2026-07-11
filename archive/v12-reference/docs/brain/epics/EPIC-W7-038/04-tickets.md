# Phase 4: Implementation Tickets — EPIC-W7-038

**Epic:** EPIC-W7-038
**Method:** VerifyPhotonSlotIntegrity
**Source:** src/V12_002.SIMA.Fleet.cs
**Original CYC:** 9
**Wave:** 7 | **Phase:** 4 — Ticket Generation

---

## ticket_count: 4

---

## Ticket 1

- **ticket_id:** 1
- **helper_name:** LogIntegrityFailure
- **concern:** Emit telemetry and Print diagnostic on integrity mismatch. Calls TrackPhotonCrcFailure + Print(string.Format). Zero branches — pure logging delegation.
- **lines_to_move:** TrackPhotonCrcFailure call and Print(string.Format(...)) block from the failure arm of the outer integrity gate.
- **cyc_reduction:** 1
- **projected_helper_cyc:** 1

---

## Ticket 2

- **ticket_id:** 2
- **helper_name:** RollbackStateEntries
- **concern:** Remove all state-dictionary entries for the failed slot: activePositions, entryOrders, stopOrders, 5 target-order dicts (for-loop + null guard), _followerBrackets. Guarded by FleetEntryName != null.
- **lines_to_move:** if(_sb.FleetEntryName != null) block containing all TryRemove calls across activePositions, entryOrders, stopOrders, for-loop GetTargetOrdersDictionary, _followerBrackets.
- **cyc_reduction:** 3
- **projected_helper_cyc:** 4

---

## Ticket 3

- **ticket_id:** 3
- **helper_name:** RollbackSlotResources
- **concern:** Release all low-level slot resources: conditional delta rollback (compound &&), sync-clear, pool release + sideband clear (sbIdx guards), Interlocked.Decrement, Volatile.Read, TryResetCircuitBreakerIfBelow.
- **lines_to_move:** Full resource-release block: AddExpectedPositionDeltaLocked (if compound-&&), ClearDispatchSyncPending, pool.Release, sideband clear, Interlocked.Decrement, TryResetCircuitBreakerIfBelow.
- **cyc_reduction:** 5
- **projected_helper_cyc:** 6

---

## Ticket 4

- **ticket_id:** 4
- **helper_name:** TryReprimePump
- **concern:** Re-prime the fleet dispatch pump if work is pending. Compound-condition guard (ring || queue non-empty) + try/catch TriggerCustomEvent. Diagnostic Print on catch.
- **lines_to_move:** if(!ring.IsEmpty || !queue.IsEmpty) block with try { TriggerCustomEvent(...) } catch { Print(...) }.
- **cyc_reduction:** 2
- **projected_helper_cyc:** 3

---

## projected_parent_cyc_after_all: 2

## CYC Summary

| Symbol | Projected CYC | <= 8? |
|---|---|---|
| VerifyPhotonSlotIntegrity (parent) | 2 | YES |
| LogIntegrityFailure | 1 | YES |
| RollbackStateEntries | 4 | YES |
| RollbackSlotResources | 6 | YES |
| TryReprimePump | 3 | YES |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Epic** | EPIC-W7-038 |
| **Wave** | 7 |
| **Phase** | 4 — Ticket Generation |
| **Bobcoins Used** | 0.8 |
| **Execution Time** | 2026-06-29T01:20:00Z |
| **jcodemunch tools called** | resolve_repo, search_symbols, get_symbol_complexity, get_extraction_candidates |
| **sequential-thinking calls** | 4 (1 probe + 3 validation thoughts) |
| **ticket_count** | 4 |
| **projected_parent_cyc_after_all** | 2 |
| **Original CYC** | 9 |
