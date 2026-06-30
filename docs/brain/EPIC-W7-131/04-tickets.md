# EPIC-W7-131 — Phase 4: Implementation Tickets

**Agent:** v12-phase4-tickets
**Wave:** 7 | **Phase:** 4 — Ticket Generation
**Method:** `SymmetryGuardPruneDispatches` | **Source:** `src/V12_002.Symmetry.Replace.cs`
**Baseline CYC:** 9 | **Target CYC:** ≤ 8
**ticket_count:** 3

---

## Ticket Summary

| Ticket | Helper | CYC Removed | Projected Helper CYC |
|--------|--------|-------------|----------------------|
| T1 | `HasActiveFollowers` | 2 | 3 |
| T2 | `ShouldPruneDispatch` | 4 | 4 |
| T3 | `TryPruneDispatchEntry` | 3 | 3 |

**projected_parent_cyc_after_all: 2**

---

## Ticket T1

- **ticket_id:** T1
- **helper_name:** `HasActiveFollowers`
- **concern:** Active follower check — pure read: iterate `ctx.Followers` snapshot, check `activePositions.ContainsKey` per follower, return bool
- **lines_to_move:** inner `foreach` over `ctx.Followers` + `activePositions.ContainsKey` check from foreach body
- **cyc_reduction:** 2
- **projected_helper_cyc:** 3

## Ticket T2

- **ticket_id:** T2
- **helper_name:** `ShouldPruneDispatch`
- **concern:** Eviction policy — boolean: TTL check OR (anchor resolved AND no active followers via HasActiveFollowers). Single named predicate for dispatch pruning logic.
- **lines_to_move:** TTL check + anchor-resolved + !HasActiveFollowers compound condition
- **cyc_reduction:** 4
- **projected_helper_cyc:** 4

## Ticket T3

- **ticket_id:** T3
- **helper_name:** `TryPruneDispatchEntry`
- **concern:** Per-entry prune action — null-guard ctx + call ShouldPruneDispatch + call `symmetryDispatchById.TryRemove`
- **lines_to_move:** Null guard on ctx + ShouldPruneDispatch call + TryRemove from per-entry iteration body
- **cyc_reduction:** 3
- **projected_helper_cyc:** 3

---

## projected_parent_cyc_after_all: 2

Parent `SymmetryGuardPruneDispatches` retains: snapshot `symmetryDispatchById.ToArray()` + foreach + delegates to `TryPruneDispatchEntry`. CYC = 2.

---

## Agent Tracking

| Field | Value |
|-------|-------|
| Agent Name | v12-phase4-tickets |
| Bobcoins Used | 0.6 |
| Execution Time | 2026-06-29T23:00:00Z |
| Wave | 7 |
| Epic | EPIC-W7-131 |
