# EPIC-W7-150 — Phase 4: Implementation Tickets

**Agent:** v12-phase4-tickets
**Wave:** 7 | **Phase:** 4 — Ticket Generation
**Method:** `ProcessQueuedExecution_HandleFleetBrackets` | **Source:** `src/V12_002.UI.Compliance.cs`
**Baseline CYC:** 10 | **Target CYC:** ≤ 8
**ticket_count:** 2

---

## Ticket Summary

| Ticket | Helper | CYC Removed | Projected Helper CYC |
|--------|--------|-------------|----------------------|
| T1 | `TryGetEligibleFollowerPosition` | 2 | 3 |
| T2 | `LogFleetBracketError` | 1 | 1 |

**projected_parent_cyc_after_all: 8**

---

## Ticket T1

- **ticket_id:** T1
- **helper_name:** `TryGetEligibleFollowerPosition`
- **concern:** Follower eligibility guard — evaluates compound `TryGetValue && pos.IsFollower && !pos.EntryFilled`, returns bool with out PositionInfo. Removes 2 `&&` operators from parent. AggressiveInlining hot path.
- **lines_to_move:** `if (activePositions.TryGetValue(fleetKey, out var pos) && pos.IsFollower && !pos.EntryFilled)` compound condition
- **cyc_reduction:** 2
- **projected_helper_cyc:** 3

## Ticket T2

- **ticket_id:** T2
- **helper_name:** `LogFleetBracketError`
- **concern:** Cold error logging — wraps `Print(string.Format("[SIMA V12.7] Error...", ex.Message))`. Cold path, NoInlining.
- **lines_to_move:** `catch` block body: Print error log statement
- **cyc_reduction:** 1
- **projected_helper_cyc:** 1

---

## projected_parent_cyc_after_all: 8

Parent `ProcessQueuedExecution_HandleFleetBrackets` after extractions: filledOrder compound guard + foreach + kvp match + eligible-follower call + bracket submission + catch delegation. CYC = 8.

---

## Agent Tracking

| Field | Value |
|-------|-------|
| Agent Name | v12-phase4-tickets |
| Bobcoins Used | 0.5 |
| Execution Time | 2026-06-29T23:00:00Z |
| Wave | 7 |
| Epic | EPIC-W7-150 |
