# Phase 4: Implementation Tickets — EPIC-W7-105

**Epic:** EPIC-W7-105
**Method:** DrainAllDispatchQueuesOnAbort
**Source:** src/V12_002.SIMA.Fleet.cs
**Original CYC:** 12
**Wave:** 7 | **Phase:** 4 — Ticket Generation

---

## ticket_count: 3

---

## Ticket 1

- **ticket_id:** 1
- **helper_name:** DrainPhotonSlotOnAbort
- **concern:** Per-slot Photon ring sideband rollback, pool release, and counter decrement — full rollback of one dequeued FleetDispatchSlot
- **lines_to_move:** The per-slot body inside the while(_photonDispatchRing.TryDequeue) loop: TrackPhotonDequeue, TryGetSidebandKey call, conditional AddExpectedPositionDeltaLocked, conditional ClearDispatchSyncPending, conditional pool release, conditional sideband reset, Interlocked.Decrement
- **cyc_reduction:** 5
- **projected_helper_cyc:** 6

---

## Ticket 2

- **ticket_id:** 2
- **helper_name:** DrainLegacySlotOnAbort
- **concern:** Per-item legacy queue delta rollback and counter decrement — full rollback of one dequeued FleetDispatchRequest
- **lines_to_move:** The per-item body inside the while(_pendingFleetDispatches.TryDequeue) loop: conditional AddExpectedPositionDeltaLocked for ReservedDelta, unconditional ClearDispatchSyncPending, Interlocked.Decrement
- **cyc_reduction:** 1
- **projected_helper_cyc:** 2

---

## Ticket 3

- **ticket_id:** 3
- **helper_name:** TryGetSidebandKey
- **concern:** Bounds-safe sideband key read from _photonSideband[]: returns true and sets key when sbIdx is in range; returns false and sets key=null otherwise — called by DrainPhotonSlotOnAbort
- **lines_to_move:** The compound bounds-check sbIdx >= 0 && sbIdx < _photonSideband.Length plus the key assignment from _photonSideband[sbIdx].ExpectedKey
- **cyc_reduction:** 1
- **projected_helper_cyc:** 2

---

## projected_parent_cyc_after_all: 3

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Epic** | EPIC-W7-105 |
| **Wave** | 7 |
| **Phase** | 4 — Ticket Generation |
| **Bobcoins Used** | ~10 |
| **Execution Time** | 2026-06-29T01:25:00Z |
| **jcodemunch tools called** | resolve_repo, get_symbol_complexity, get_extraction_candidates |
| **sequential-thinking calls** | 4 (1 probe + 3 ticket thoughts) |
| **ticket_count** | 3 |
| **projected_parent_cyc_after_all** | 3 |
| **Original CYC** | 12 |
