# Phase 4 Tickets — EPIC-W7-025

**Epic**: EPIC-W7-025
**Method**: CheckFFMAConditions
**Source File**: V12_002.Entries.FFMA.cs
**Original CYC**: 16 (task list reported 2 — MCP confirmed 16 in Phase 2)
**Wave**: 7 | **Phase**: 4

---

## Ticket Summary

ticket_count: 4

Execution order: T1 → T2 → T3 → T4
All tickets scope to `src/V12_002.Entries.FFMA.cs` only.
DNA Audit: PASS (Phase 3, 0 violations).

---

## Tickets

### Ticket 1

ticket_id: T1
helper_name: CheckFFMAGuards
concern: Guard validation — armed/enabled state, null indicator checks, and CurrentBar minimum bar count
lines_to_move: The three early-return if-blocks at the top of CheckFFMAConditions: (1) `!isFFMAModeArmed || !FFMAEnabled` guard, (2) `ema9==null || rsiIndicator==null || currentATR<=0` null-safety guard, (3) `CurrentBar < 20` bar minimum guard
cyc_reduction: 6 (three multi-condition boolean guards removed from parent)
projected_helper_cyc: 7
signature: `private bool CheckFFMAGuards()`
dependency: none — must be written first (parent calls it after extraction)

---

### Ticket 2

ticket_id: T2
helper_name: ComputeFFMAStopDistance
concern: Stop distance computation — clamp raw distance to MaximumStop ceiling and tickSize*2 floor, eliminating duplication between SHORT and LONG branches
lines_to_move: The stop distance clamping logic duplicated in both SHORT and LONG entry blocks: `Math.Min(Math.Abs(currentPrice - candleExtreme), MaximumStop)` with `if (stopDistance < tickSize * 2) stopDistance = tickSize * 2`
cyc_reduction: 4 (two duplicate branch pairs removed from parent; one shared helper added)
projected_helper_cyc: 2
signature: `private double ComputeFFMAStopDistance(double currentPrice, double candleExtreme)`
dependency: none — must be written before T3 and T4 (both call it)

---

### Ticket 3

ticket_id: T3
helper_name: TryExecuteFFMAShort
concern: SHORT entry logic — evaluate RSI overbought + EMA distance + red candle condition, log trigger, compute stop distance, calculate position size, execute SHORT entry
lines_to_move: The SHORT block: `if (rsiValue > FFMARSIOverbought && distanceFromEMA >= FFMAEMADistance && Close[0] < Open[0])` including Print(), ComputeFFMAStopDistance(), CalculatePositionSize(), ExecuteFFMAEntry(MarketPosition.Short, contracts)
cyc_reduction: 3 (short-entry conditional and inner branch removed from parent)
projected_helper_cyc: 4
signature: `private bool TryExecuteFFMAShort(double rsiValue, double distanceFromEMA, double currentPrice)`
dependency: T2 (ComputeFFMAStopDistance must exist)

---

### Ticket 4

ticket_id: T4
helper_name: TryExecuteFFMALong
concern: LONG entry logic — evaluate RSI oversold + EMA distance + green candle condition, log trigger, compute stop distance, calculate position size, execute LONG entry
lines_to_move: The LONG block: `if (rsiValue < FFMARSIOversold && distanceFromEMA <= -FFMAEMADistance && Close[0] > Open[0])` including Print(), ComputeFFMAStopDistance(), CalculatePositionSize(), ExecuteFFMAEntry(MarketPosition.Long, contracts)
cyc_reduction: 3 (long-entry conditional and inner branch removed from parent)
projected_helper_cyc: 4
signature: `private bool TryExecuteFFMALong(double rsiValue, double distanceFromEMA, double currentPrice)`
dependency: T2 (ComputeFFMAStopDistance must exist)

---

## Extraction Summary

| Method | Role | CYC Before | CYC After | Compliant |
|--------|------|-----------|-----------|-----------|
| `CheckFFMAConditions` | parent | 16 | 3 | YES (<=8) |
| `CheckFFMAGuards` | new helper | — | 7 | YES (<=8) |
| `ComputeFFMAStopDistance` | new helper | — | 2 | YES (<=8) |
| `TryExecuteFFMAShort` | new helper | — | 4 | YES (<=8) |
| `TryExecuteFFMALong` | new helper | — | 4 | YES (<=8) |
| **max_cyc_projected** | | | **7** | **PASS** |

projected_parent_cyc_after_all: 3

CYC reduction: 16 → 3 in parent (81% reduction). max across all methods: 7 (<=8 Jane Street threshold PASS).

---

## Jane Street Compliance

| Rule | Compliance |
|------|-----------|
| CYC <= 8 per method | max=7 PASS |
| Single responsibility per helper | Each helper owns exactly one concern PASS |
| Zero-alloc hot path | All helpers use `double`/`int` value params — no heap allocs PASS |
| No LINQ | No LINQ in any planned code PASS |
| Cold logging out-of-line | `Print` stays inside `TryExecuteFFMA*` cold-path methods PASS |
| No new `lock()` blocks | 0 lock() calls — lock-free compliance PASS |
| ASCII-only string literals | All format strings ASCII only PASS |

---

## Sequential Thinking Evidence

**Thought 1**: CYC=16 confirmed (not 2) — extraction required. 4 tickets designed.
**Thought 2**: Per-ticket line mapping — T1 guards (CYC 7), T2 stop calc (CYC 2), T3 SHORT entry (CYC 4), T4 LONG entry (CYC 4). T2 shared by T3+T4 eliminating duplication.
**Thought 3**: All 5 post-extraction methods satisfy CYC <=8. max_cyc=7. projected_parent_cyc=3. Ticket execution order: T1 → T2 → T3 → T4.

---

## Agent Tracking

- Agent Name: v12-phase4-tickets
- Wave: 7
- Phase: 4
- Epic: EPIC-W7-025
- Method: CheckFFMAConditions
- Original CYC: 16 (task list reported 2, MCP confirmed 16)
- ticket_count: 4
- max_cyc_projected: 7
- projected_parent_cyc_after_all: 3
- MCP Tools Used: resolve_repo, sequentialthinking, get_symbol_complexity (not found — used Phase 2 evidence), get_extraction_candidates
- Status: COMPLETE
- Timestamp: 2025-07-11
