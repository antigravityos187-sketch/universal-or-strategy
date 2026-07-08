# Phase 4: Implementation Tickets — EPIC-W7-104

**Epic:** EPIC-W7-104
**Method:** SubmitAndRegisterFleetOrders
**Source:** src/V12_002.SIMA.Fleet.cs
**Original CYC:** 12
**Wave:** 7 | **Phase:** 4 — Ticket Generation

---

## ticket_count: 3

---

## Ticket 1

- **ticket_id:** 1
- **helper_name:** BuildSubmitSlice
- **concern:** Guard-clause array-slice builder — returns full orders array unchanged when slice not needed; allocates trimmed slice only when orderCount is a valid sub-range
- **lines_to_move:** The compound &&-guard block (orders != null && orderCount > 0 && orderCount < orders.Length) lines 184-188; guard-clause inversion returning orders or trimmed copy
- **cyc_reduction:** 3
- **projected_helper_cyc:** 4

---

## Ticket 2

- **ticket_id:** 2
- **helper_name:** TransitionFsmToSubmitted
- **concern:** Single FSM state transition: guards FSM entry exists, is non-null, and is in PendingSubmit state before transitioning to Submitted and stamping LastUpdateUtc
- **lines_to_move:** The triple-&& FSM PendingSubmit->Submitted state transition block (lines 195-203); converted to guard-clause early-return pattern
- **cyc_reduction:** 3
- **projected_helper_cyc:** 4

---

## Ticket 3

- **ticket_id:** 3
- **helper_name:** RegisterOrderIdsInFsmIndex
- **concern:** Order-ID index maintenance: validates FSM entry existence then loops all submitted orders, registering each valid OrderId->fleetEntryName mapping in _orderIdToFsmKey
- **lines_to_move:** Nested if + for + compound null/empty guard registering order-IDs into _orderIdToFsmKey (lines 206-214); TryGetValue FSM guard + for-loop + continue guard
- **cyc_reduction:** 4
- **projected_helper_cyc:** 5

---

## projected_parent_cyc_after_all: 1

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Epic** | EPIC-W7-104 |
| **Wave** | 7 |
| **Phase** | 4 — Ticket Generation |
| **Bobcoins Used** | 5 |
| **Execution Time** | 2026-06-29T01:20:00Z |
| **jcodemunch tools called** | resolve_repo, get_symbol_complexity, get_extraction_candidates |
| **sequential-thinking calls** | 4 (1 probe + 3 ticket thoughts) |
| **ticket_count** | 3 |
| **projected_parent_cyc_after_all** | 1 |
| **Original CYC** | 12 |
