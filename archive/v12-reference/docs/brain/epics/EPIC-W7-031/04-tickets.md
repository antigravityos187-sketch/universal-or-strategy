# Phase 4 Tickets — EPIC-W7-031

**Epic**: EPIC-W7-031
**Method**: AuditMaster_HandleNakedPosition
**Source File**: V12_002.REAPER.Audit.cs
**Original CYC**: 19
**Wave**: 7 | **Phase**: 4

## Ticket Summary

ticket_count: 3

## Tickets

### Ticket 1

ticket_id: T1
helper_name: AuditMaster_HasWorkingStopOrder
concern: Stop-order detection predicate — owns the LINQ .Any() evaluation of whether a working stop order exists for the current instrument (state/type/action OR-conditions)
lines_to_move: Extract the inline LINQ .Any() lambda from the stop-order detection block: the predicate evaluating `o.Instrument == masterPos.Instrument` AND `o.OrderState == Working` AND (`o.OrderType == StopMarket || StopLimit`) AND (`o.OrderAction == Sell || SellShort`). Signature: `private bool AuditMaster_HasWorkingStopOrder(Order[] masterOrders)`. Annotate `[MethodImpl(MethodImplOptions.NoInlining)]`.
cyc_reduction: 5 (removes 4 OR-condition branches + 1 .Any() lambda branch from parent)
projected_helper_cyc: 6

---

### Ticket 2

ticket_id: T2
concern: Grace-window initialization — owns the cold-path first-seen dictionary insert and the Print cold log announcing the grace window start
helper_name: AuditMaster_InitNakedPositionGrace
lines_to_move: Extract the body of the `!TryGetValue` branch: `_nakedPositionFirstSeen[Account.Name] = DateTime.UtcNow;` and `Print(string.Format("Naked position grace window started..."))` log call. Signature: `private void AuditMaster_InitNakedPositionGrace(int masterActualQty, int graceSeconds)`. Annotate `[MethodImpl(MethodImplOptions.NoInlining)]` (cold path).
cyc_reduction: 0 (code was already inside an existing if-branch; value is cold-path isolation and NoInlining annotation)
projected_helper_cyc: 1

---

### Ticket 3

ticket_id: T3
helper_name: AuditMaster_DispatchNakedStop
concern: Naked stop dispatch — owns the EnqueueReaperMasterNakedStop guard, TriggerCustomEvent dispatch, try/catch exception handling, and _reaperNakedStopInFlight in-flight cleanup
lines_to_move: Extract the else-if branch body: `EnqueueReaperMasterNakedStop(...)` guard + `TriggerCustomEvent(...)` call + wrapping try/catch + `_reaperNakedStopInFlight` cleanup on success. Signature: `private void AuditMaster_DispatchNakedStop(Position masterPos, int masterActualQty, string masterExpectedKey, DateTime masterFirstSeen)`. Annotate `[MethodImpl(MethodImplOptions.NoInlining)]`.
cyc_reduction: 4 (removes enqueue_guard branch + try_normal path + catch handler from parent; else-if call site remains in parent)
projected_helper_cyc: 4

---

## Execution Order

| Order | Ticket | Reason |
|-------|--------|--------|
| 1 | T1 — AuditMaster_HasWorkingStopOrder | Must land first: parent's orchestration structure calls this helper before the first-seen / dispatch logic executes |
| 2 | T2 — AuditMaster_InitNakedPositionGrace | Lands second: cold-path isolation, no dependency on T3 |
| 3 | T3 — AuditMaster_DispatchNakedStop | Lands third: parent structure with T1 already merged must be stable before dispatch helper is wired in |

All tickets modify `src/V12_002.REAPER.Audit.cs` only. Execute sequentially (T1 → T2 → T3) to avoid merge conflicts and ensure each intermediate state compiles cleanly.

## Extraction Summary

projected_parent_cyc_after_all: 7

| Unit | Projected CYC | CYC<=8? |
|------|--------------|---------|
| `AuditMaster_HandleNakedPosition` (parent, post-extract) | 7 | YES |
| `AuditMaster_HasWorkingStopOrder` (T1) | 6 | YES |
| `AuditMaster_InitNakedPositionGrace` (T2) | 1 | YES |
| `AuditMaster_DispatchNakedStop` (T3) | 4 | YES |

**CYC baseline:** 19 → **max_cyc_projected:** 7 (63% peak complexity reduction)

## Jane Street Compliance

| Rule | Applied In |
|------|-----------|
| `carl_cook` — extract LINQ predicate out-of-line; cold log NoInlining | T1, T2 |
| `gjengset` — zero new lock() blocks; ConcurrentDictionary lock-free primitives preserved | All tickets |
| `trading_billions` — each helper single responsibility; parent is orchestrator only; exception handler isolated | T1, T2, T3 |

## Agent Tracking

- Agent Name: v12-phase4-tickets
- Wave: 7
- Phase: 4
- Epic: EPIC-W7-031
- Method: AuditMaster_HandleNakedPosition
- Original CYC: 19
- ticket_count: 3
- Sequential Thinking: 3 thoughts executed (CYC risk assessment → per-ticket breakdown → CYC validation + execution order)
- MCP Tools: resolve_repo (PASS), sequentialthinking (PASS), get_symbol_complexity (symbol not in index — architecture plan used as authoritative source), get_extraction_candidates (empty — min_callers threshold; plan used as authoritative)
- DNA Audit: PASS (Phase 3)
- max_cyc_projected: 7
