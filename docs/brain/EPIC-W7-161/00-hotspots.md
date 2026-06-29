# EPIC-W7-161 — Phase 0: Hotspot Analysis

> Wave 7 | Phase 0 | Agent: v12-phase0-hotspot (top_orch direct write — sparse entry resolved via complexity audit)

---

## 1. Method Identity

| Field        | Value                                                      |
|--------------|------------------------------------------------------------|
| Method Name  | `SyncLiveTargetRows`                                       |
| File         | `src/V12_002.UI.Panel.StateSync.cs`                        |
| Lines        | ~158–178 (21 LOC)                                          |
| Visibility   | `private`                                                  |
| Class        | `V12_002` (partial, `Strategy`)                            |
| CYC (audit)  | 10                                                         |

**Note**: Epic list entry was sparse (blank method_name/source_file). Method resolved from `complexity_audit.py` output: `UI.Panel.StateSync.cs::SyncLiveTargetRows (CYC=10, LOC=21)`. Placed at epic #161 (final epic in Wave 7) matching CYC=10 from the sparse entry. SyncLiveTargetRows is the last CYC>8 method in the StateSync file after SyncModeChipVisuals (W7-158, CYC=9).

---

## 2. Blast Radius Summary

`SyncLiveTargetRows` synchronises the live target price rows in the WPF panel with the current `UILivePositionSnapshot`. Called from `UpdatePanelState` (W7-148) as part of the panel refresh cycle. Affects the target row visibility and price display logic in the panel UI.

**Blast radius**: Low. Single caller (`UpdatePanelState`). No order logic or state mutation. UI-only concern.

---

## 3. Top 3 Complexity Drivers

1. **Per-row iteration with state guards** — loops over N target rows (typically 4) with per-row visibility + content conditions
2. **Null-propagation chain** — each row access guards against null snapshot, null position, and missing target slot
3. **Conditional text formatting** — each target row conditionally renders price text vs empty string based on active/inactive state

---

## 4. Recommended Extraction Count

CYC=10 → target CYC ≤ 8 → **1 extraction** sufficient.

Suggested split:
- `SyncSingleTargetRow(int t, UILivePositionSnapshot snapshot)` — handles all per-row logic for target index t

Parent `SyncLiveTargetRows` becomes a simple loop calling the extracted helper, CYC ≤ 3.

---

## Agent Tracking

- **Agent Name**: v12-phase0-hotspot (top_orch direct write)
- **Bobcoins Used**: 0 (resolved from cached complexity_audit.py output)
- **Execution Time**: < 1s
