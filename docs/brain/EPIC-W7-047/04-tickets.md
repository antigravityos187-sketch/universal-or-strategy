# Phase 4: Implementation Tickets — EPIC-W7-047

**Epic:** EPIC-W7-047
**Method:** CancelOrphanedTargets
**Source:** src/V12_002.UI.Compliance.cs
**Original CYC:** 13
**Wave:** 7 | **Phase:** 4 — Ticket Generation

---

## ticket_count: 3

---

## Ticket 1

- **ticket_id:** 1
- **helper_name:** `IsTargetOrderPrefix`
- **concern:** Encapsulate the 5-arm T1_..T5_ prefix filter as a single boolean predicate
- **lines_to_move:** Extract the compound `o.Name != null && (o.Name.StartsWith("T1_") || ... || o.Name.StartsWith("T5_"))` expression (approx. lines 562–571 of `src/V12_002.UI.Compliance.cs`) into a new private method. Signature: `private bool IsTargetOrderPrefix(string name)`. Body: `return name != null && (name.StartsWith("T1_") || name.StartsWith("T2_") || name.StartsWith("T3_") || name.StartsWith("T4_") || name.StartsWith("T5_"));`
- **cyc_reduction:** -5 (removes 5 branch points from the inline predicate in the parent loop body)
- **projected_helper_cyc:** 7
- **depends_on:** none
- **file:** `src/V12_002.UI.Compliance.cs`
- **test_coverage:**
  - `IsTargetOrderPrefix_ReturnsTrue_ForT1ThroughT5Prefixes` — all 5 valid prefixes return true
  - `IsTargetOrderPrefix_ReturnsFalse_ForNullOrOtherPrefixes` — null, "T6_", "TP_", "" return false

---

## Ticket 2

- **ticket_id:** 2
- **helper_name:** `IsOrphanedTarget`
- **concern:** Compose null guard, instrument match, state gate, and prefix test into a single order qualification predicate
- **lines_to_move:** Extract the three inline guard conditions from `CancelOrphanedTargets` loop body into a new private method. Guards: (a) `if (o == null || o.Instrument?.FullName != Instrument?.FullName) continue;` (~line 555), (b) `if (o.OrderState != OrderState.Working && o.OrderState != OrderState.Accepted) continue;` (~line 557), (c) the prefix test via `IsTargetOrderPrefix(o.Name)` (from Ticket 1). Signature: `private bool IsOrphanedTarget(Order o)`. Body: null guard returns false, instrument mismatch returns false, state gate returns false, then `return IsTargetOrderPrefix(o.Name);`
- **cyc_reduction:** -3 (removes 3 remaining conditional guards from the parent loop body; parent drops from ~8 to ~3)
- **projected_helper_cyc:** 7
- **depends_on:** Ticket 1 (calls `IsTargetOrderPrefix`)
- **file:** `src/V12_002.UI.Compliance.cs`
- **test_coverage:**
  - `IsOrphanedTarget_ReturnsFalse_WhenOrderIsNull` — null order guard
  - `IsOrphanedTarget_ReturnsFalse_WhenInstrumentMismatch` — different instrument FullName
  - `IsOrphanedTarget_ReturnsFalse_WhenOrderStateIsNotActive` — Cancelled/Filled states
  - `IsOrphanedTarget_ReturnsTrue_WhenAllConditionsMet` — Working state + T1_ prefix + instrument match

---

## Ticket 3

- **ticket_id:** 3
- **helper_name:** `CancelOrphanedTargets` (parent refactor — no new helper)
- **concern:** Replace all inline predicate guards in `CancelOrphanedTargets` with single delegation to `IsOrphanedTarget`, reducing parent CYC from 13 to 3
- **lines_to_move:** Modify existing `CancelOrphanedTargets` body (lines 553–578). Replace the three inline guard conditions and the T1_..T5_ prefix block with a single `if (!IsOrphanedTarget(o)) continue;` guard. Resulting body: foreach loop over `.ToArray()` snapshot, single `IsOrphanedTarget` predicate dispatch, `CancelOrderOnAccount` cancel submission, counter increment, return.
- **cyc_reduction:** -10 net (13 -> 3; base=1 + foreach=1 + if=1)
- **projected_helper_cyc:** 3 (this IS the parent; no new helper)
- **depends_on:** Ticket 2 (delegates to `IsOrphanedTarget`)
- **file:** `src/V12_002.UI.Compliance.cs`
- **test_coverage:** Covered by callers via integration: `HandleFleetStopFill` invokes `CancelOrphanedTargets` directly; unit tests on `IsOrphanedTarget` and `IsTargetOrderPrefix` (Tickets 1 & 2) provide predicate coverage.

---

## projected_parent_cyc_after_all: 3

---

## CYC Summary

| Method | Before | After | Within Limit |
|---|---|---|---|
| `CancelOrphanedTargets` | 13 | 3 | YES (<=8) |
| `IsTargetOrderPrefix` (new) | — | 7 | YES (<=8) |
| `IsOrphanedTarget` (new) | — | 7 | YES (<=8) |
| **Max across all** | **13** | **7** | **PASS** |

---

## Execution Order

Tickets MUST be executed sequentially (each depends on the previous):

1. **Ticket 1** → Add `IsTargetOrderPrefix` private method
2. **Ticket 2** → Add `IsOrphanedTarget` private method (calls `IsTargetOrderPrefix`)
3. **Ticket 3** → Refactor `CancelOrphanedTargets` body to delegate to `IsOrphanedTarget`

Build must pass after each ticket. Run `powershell -File .\scripts\build_readiness.ps1` after Ticket 3.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Epic** | EPIC-W7-047 |
| **Wave** | 7 |
| **Phase** | 4 — Ticket Generation |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | 2026-06-29T01:25:00Z |
| **jcodemunch tools called** | resolve_repo, get_symbol_complexity, get_extraction_candidates |
| **sequential-thinking calls** | 4 (1 probe + 3 ticket-breakdown thoughts) |
| **ticket_count** | 3 |
| **projected_parent_cyc_after_all** | 3 |
| **Original CYC** | 13 |
