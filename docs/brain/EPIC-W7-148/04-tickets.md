# EPIC-W7-148 — Phase 4: Implementation Tickets

**Agent:** v12-phase4-tickets
**Wave:** 7 | **Phase:** 4 — Ticket Generation
**Method:** `UpdatePanelState` | **Source:** `src/V12_002.UI.Panel.StateSync.cs`
**Baseline CYC:** 16 | **Target CYC:** ≤ 8
**ticket_count:** 3

---

## Ticket Summary

| Ticket | Helper | CYC Removed | Projected Helper CYC |
|--------|--------|-------------|----------------------|
| T1 | `UpdatePanelState_PriceDisplay` | 4 | 7 |
| T2 | `UpdatePanelState_StateSync` | 6 | 7 |
| T3 | `UpdatePanelState_LivePosition` | 5 | 6 |

**projected_parent_cyc_after_all: 3**

---

## Ticket T1

- **ticket_id:** T1
- **helper_name:** `UpdatePanelState_PriceDisplay`
- **concern:** Price display rendering — renders last price text with market-position color ternary chain, applies RMA toggle opacity null guards. `private void UpdatePanelState_PriceDisplay(UIStateSnapshot snapshot)`. AggressiveInlining.
- **lines_to_move:** `lastPriceText != null` guard + price ternary + market position color ternary chain + `trendRmaToggle`/`retestRmaToggle` null-check opacity guards (Cluster 2)
- **cyc_reduction:** 4
- **projected_helper_cyc:** 7

## Ticket T2

- **ticket_id:** T2
- **helper_name:** `UpdatePanelState_StateSync`
- **concern:** State-sync conditional dispatch — mode change guard, config revision guard, count change guard, debounce compound guard + debounce check, delegates to SyncModeChipVisuals, SyncPanelConfigFromSnapshot, SyncCountChipVisuals. `private void UpdatePanelState_StateSync(UIStateSnapshot snapshot, int count)`. AggressiveInlining.
- **lines_to_move:** Mode change + config revision + count change + debounce compound (Cluster 3, 6 CYC)
- **cyc_reduction:** 6
- **projected_helper_cyc:** 7

## Ticket T3

- **ticket_id:** T3
- **helper_name:** `UpdatePanelState_LivePosition`
- **concern:** Live position and cleanup — live position null+HasLivePosition compound guard → SyncLiveTargetRows; cleanup guard → SetLiveTargetRowsVisible. `private void UpdatePanelState_LivePosition(UIStateSnapshot snapshot, int count)`.
- **lines_to_move:** Compound live-position guard + inner null check + cleanup guard + inner null check (Cluster 4, 5 CYC)
- **cyc_reduction:** 5
- **projected_helper_cyc:** 6

---

## projected_parent_cyc_after_all: 3

Parent `UpdatePanelState` retains: null/termination guard + snapshot acquisition + count computation + 3 helper delegation calls. CYC = 3.

---

## Agent Tracking

| Field | Value |
|-------|-------|
| Agent Name | v12-phase4-tickets |
| Bobcoins Used | 0.6 |
| Execution Time | 2026-06-29T23:00:00Z |
| Wave | 7 |
| Epic | EPIC-W7-148 |
