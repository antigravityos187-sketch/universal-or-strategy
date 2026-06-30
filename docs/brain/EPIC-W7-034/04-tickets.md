# EPIC-W7-034 — Phase 4: Implementation Tickets

**Agent:** v12-phase4-tickets
**Wave:** 7 | **Phase:** 4 — Ticket Generation
**Method:** `ManageCIT` | **Source:** `src/V12_002.Orders.Management.Flatten.cs`
**Baseline CYC:** 11 | **Target CYC:** ≤ 8
**ticket_count:** 1

---

## Ticket Summary

| Ticket | Helper | CYC Removed | Projected Helper CYC |
|--------|--------|-------------|----------------------|
| T1 | `ProcessCitOrder` | 7 | 8 |

**projected_parent_cyc_after_all: 4**

---

## Ticket T1

- **ticket_id:** T1
- **helper_name:** `ProcessCitOrder`
- **concern:** Per-order dispatch — follower/local routing resolution, nudge price calculation, ExecuteFollowerNudge or ExecuteLocalNudge dispatch, one-shot nudge guard, per-order exception handling
- **lines_to_move:** Branches 4–10 of ManageCIT: `pos != null && pos.IsFollower && pos.ExecutingAccount != null` compound, `isFollower` routing, `ExecuteFollowerNudge` + budget check, `ExecuteLocalNudge`, `_citNudgedKeys.TryAdd`, `catch (InvalidOperationException when ...)`, `catch (Exception)`
- **cyc_reduction:** 7
- **projected_helper_cyc:** 8

---

## projected_parent_cyc_after_all: 4

Parent retains: `ValidateCitConfiguration` guard + foreach loop + `ShouldChaseOrder` guard + `ProcessCitOrder` call with budget check. CYC = 4.

---

## Agent Tracking

| Field | Value |
|-------|-------|
| Agent Name | v12-phase4-tickets |
| Bobcoins Used | 0.5 |
| Execution Time | 2026-06-29T23:00:00Z |
| Wave | 7 |
| Epic | EPIC-W7-034 |
