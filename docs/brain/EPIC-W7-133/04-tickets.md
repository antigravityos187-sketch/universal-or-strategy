# EPIC-W7-133 — Phase 4: Implementation Tickets

**Agent:** v12-phase4-tickets
**Wave:** 7 | **Phase:** 4 — Ticket Generation
**Method:** `MoveStop_SinglePosition` | **Source:** `src/V12_002.Trailing.Breakeven.cs`
**Baseline CYC:** 21 | **Target CYC:** ≤ 8
**ticket_count:** 4

---

## Ticket Summary

| Ticket | Helper | CYC Removed | Projected Helper CYC |
|--------|--------|-------------|----------------------|
| T1 | `CalcBreakevenStopPrice` | 1 | 2 |
| T2 | `IsStopImprovement` | 2 | 4 |
| T3 | `HandleFollowerBreakeven` | 5 | 2 |
| T4 | `TryArmOrExecuteMasterBreakeven` | 11 | 5 |

**projected_parent_cyc_after_all: 2**

---

## Ticket T1

- **ticket_id:** T1
- **helper_name:** `CalcBreakevenStopPrice`
- **concern:** Breakeven stop price computation — direction-aware new stop price calc and tick-size rounding. AggressiveInlining hot-path helper.
- **lines_to_move:** Direction-aware stop price calculation block from MoveStop_SinglePosition body
- **cyc_reduction:** 1
- **projected_helper_cyc:** 2

## Ticket T2

- **ticket_id:** T2
- **helper_name:** `IsStopImprovement`
- **concern:** Stop improvement predicate — pure boolean: is `newStopPrice` profit-protecting for `pos.Direction`? Long → newStopPrice > currentStop, Short → newStopPrice < currentStop. AggressiveInlining.
- **lines_to_move:** isBetter ternary check from both follower and master paths (eliminates duplication)
- **cyc_reduction:** 2
- **projected_helper_cyc:** 4

## Ticket T3

- **ticket_id:** T3
- **helper_name:** `HandleFollowerBreakeven`
- **concern:** Follower breakeven path — entire `IsFollower` sub-tree: improvement check + UpdateStopOrder + MarkStickyDirty state + log. NoInlining cold-ish path.
- **lines_to_move:** Full `if (pos.IsFollower)` branch body from MoveStop_SinglePosition
- **cyc_reduction:** 5
- **projected_helper_cyc:** 2

## Ticket T4

- **ticket_id:** T4
- **helper_name:** `TryArmOrExecuteMasterBreakeven`
- **concern:** Master breakeven ARM GUARD chain + improvement check + final UpdateStopOrder — `_beArmGuard` compound + master improvement check + stop update call. NoInlining.
- **lines_to_move:** ARM GUARD chain + master-path improvement check + UpdateStopOrder call
- **cyc_reduction:** 11
- **projected_helper_cyc:** 5

---

## projected_parent_cyc_after_all: 2

Parent `MoveStop_SinglePosition` retains: newStopPrice = CalcBreakevenStopPrice + if IsFollower → HandleFollowerBreakeven else → TryArmOrExecuteMasterBreakeven. CYC = 2.

---

## Agent Tracking

| Field | Value |
|-------|-------|
| Agent Name | v12-phase4-tickets |
| Bobcoins Used | 0.6 |
| Execution Time | 2026-06-29T23:00:00Z |
| Wave | 7 |
| Epic | EPIC-W7-133 |
