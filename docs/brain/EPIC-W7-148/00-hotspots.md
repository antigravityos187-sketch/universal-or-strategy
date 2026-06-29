# EPIC-W7-148 — Phase 0: Hotspot Analysis

> Wave 7 | Phase 0 | Agent: v12-phase0-hotspot (top_orch direct write — sparse entry resolved via complexity audit)

---

## 1. Method Identity

| Field        | Value                                                      |
|--------------|------------------------------------------------------------|
| Method Name  | `UpdatePanelState`                                         |
| File         | `src/V12_002.UI.Panel.StateSync.cs`                        |
| Lines        | ~13–63 (51 LOC)                                            |
| Visibility   | `private`                                                  |
| Class        | `V12_002` (partial, `Strategy`)                            |
| CYC (audit)  | 16                                                         |

**Note**: Epic list entry was sparse (blank method_name/source_file). Method resolved from `complexity_audit.py` output: `UI.Panel.StateSync.cs::UpdatePanelState (CYC=16, LOC=51)`. Position in hotspot list and CYC=16 match epic #148 placement between W7-145 (HandleFleetTargetFill CYC=17) and W7-149 (LogApexPerformance CYC=20).

---

## 2. Blast Radius Summary

`UpdatePanelState` is the primary UI rendering dispatcher for the strategy panel. It is called from `OnBarUpdate` and UI timer callbacks to synchronize the WPF panel with current strategy state. It fans out to multiple sub-renderers: `UpdateHubStatusLed`, `UpdateTelemetryDisplay`, `UpdateComplianceDisplay`, `UpdateTrendIndicator`, `SyncModeChipVisuals`, `SyncCountChipVisuals`, and `SyncPanelConfigFromSnapshot`.

**Blast radius**: Medium. Changes affect panel rendering only — no order logic or state mutation. Callers are limited to the UI dispatch path.

---

## 3. Top 3 Complexity Drivers

1. **Null-guard + snapshot validation chain** — multiple `if (snapshot == null)` / `if (!IsEnabled)` guards at the top accumulate early-return branches
2. **Delegated sub-renderer dispatch** — each of 6–7 sub-method calls is wrapped in its own guard condition (feature flags, null checks), creating a sequence of independent if-blocks
3. **State-dependent rendering paths** — conditional rendering for compliance hub, trend indicator, and config target visibility adds branching at each visual section

---

## 4. Recommended Extraction Count

CYC=16 → target CYC ≤ 8 → **2 extractions** sufficient.

Suggested split:
- `UpdatePanelState_CoreDisplays()` — hub status + telemetry + compliance (shared always-on section)
- `UpdatePanelState_ConditionalDisplays()` — trend + mode chips + config rows (feature-flag-gated section)

Parent `UpdatePanelState` becomes a dispatcher with CYC ≤ 4.

---

## Agent Tracking

- **Agent Name**: v12-phase0-hotspot (top_orch direct write)
- **Bobcoins Used**: 0 (resolved from cached complexity_audit.py output)
- **Execution Time**: < 1s
