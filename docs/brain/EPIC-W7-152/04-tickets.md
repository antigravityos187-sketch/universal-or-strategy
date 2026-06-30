# EPIC-W7-152 — Phase 4: Implementation Tickets

**Agent:** v12-phase4-tickets
**Wave:** 7 | **Phase:** 4 — Ticket Generation
**Method:** `TryApplyConfigTarget_Value` | **Source:** `src/V12_002.UI.IPC.Commands.Config.cs`
**Baseline CYC:** 17 | **Target CYC:** ≤ 8
**ticket_count:** 1

---

## Ticket Summary

| Ticket | Helper | CYC Removed | Projected Helper CYC |
|--------|--------|-------------|----------------------|
| T1 | `ApplyValidatedTargetValue` + `_numericTargetMap` field | 14 | 3 |

**projected_parent_cyc_after_all: 3**

---

## Ticket T1

- **ticket_id:** T1
- **helper_name:** `ApplyValidatedTargetValue`
- **concern:** Validated target value application — parse `val` as double, call `ValidateIpcMultiplier`, call `assign(v)` property setter lambda, else Print rejection message. Guard-clause style with early returns. Also creates `_numericTargetMap` Dictionary<string, Action<double>> field mapping T1-T5 to property setters, eliminating the 5-arm if-chain entirely.
- **lines_to_move:** All 5 `if (key == "Tn")` arms from TryApplyConfigTarget_Value + associated TryParse + ValidateIpcMultiplier + assignment triples; replace with dictionary dispatch
- **cyc_reduction:** 14
- **projected_helper_cyc:** 3

---

## projected_parent_cyc_after_all: 3

Parent `TryApplyConfigTarget_Value` retains: CIT guard (early return) + `_numericTargetMap.TryGetValue` + `ApplyValidatedTargetValue` call + fall-through `return false`. CYC = 3.

---

## Agent Tracking

| Field | Value |
|-------|-------|
| Agent Name | v12-phase4-tickets |
| Bobcoins Used | 0.4 |
| Execution Time | 2026-06-29T23:00:00Z |
| Wave | 7 |
| Epic | EPIC-W7-152 |
