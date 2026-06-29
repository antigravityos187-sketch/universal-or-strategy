# EPIC-W7-013 — Phase 1: Scope Definition

## Method in Scope

**Single method:** `UpdatePanelState`
**File:** `src/V12_002.UI.Panel.StateSync.cs` (lines 13–89)
**Class:** `V12_002` (partial class, `NinjaTrader.NinjaScript.Strategies` namespace)

This document defines the precise **scope boundary** for the refactor work in EPIC-W7-013. Only the
**single method** `UpdatePanelState` is targeted for complexity reduction in this epic. No other
methods are in scope.

---

## Complexity Targets

| Metric | Current | Target |
|--------|---------|--------|
| Cyclomatic Complexity (CYC) | **8** | **≤ 8** (reduction via extraction) |
| Lines of code | 76 (lines 13–89) | ~30–40 after extractions |
| Estimated post-refactor CYC | — | 4–5 (core method) |

The current CYC of **8** is confirmed by manual decision-node path-count across the method body and
cross-referenced with the jcodemunch tooling findings recorded in `00-hotspots.md`. The target is to
reduce the CYC of the *core* `UpdatePanelState` body to **≤ 8** (ideally 4–5) by extracting
responsibility into focused helper methods, each with CYC 2–3.

---

## Callers

Source-level grep across all `.cs` files in this workspace confirms **2 call sites**:

| # | File | Line | Context |
|---|------|------|---------|
| 1 | `src/V12_002.UI.Panel.Lifecycle.cs` | 81 | Timer-driven refresh path; protected by `_panelUpdateInProgress` interlocked flag (FREEZE-PROOF, Build 1109) |
| 2 | `src/V12_002.UI.Panel.Construction.cs` | 230 | One-shot call at panel construction completion |

**Callers count: 2**

Both callers are read-only with respect to this epic — they are documented here to establish the
**scope boundary** but are not modified. The method signature `private void UpdatePanelState()` is
preserved unchanged so both call sites remain valid without any modification.

Thread-safety contract: both callers execute inside `ChartControl.Dispatcher.InvokeAsync`, which is
the UI-thread dispatch mechanism. Any refactoring must preserve this contract in all extracted helpers.

---

## Scope Boundary

The **scope boundary** for EPIC-W7-013 is strictly:

```
IN SCOPE
  └─ UpdatePanelState()          src/V12_002.UI.Panel.StateSync.cs:13–89
       ├─ TryUpdateTargetCountChip()   [extracted helper — new, same file]
       ├─ ApplyLivePositionView()      [extracted helper — new, same file]
       └─ TeardownLivePositionView()   [extracted helper — new, same file]

OUT OF SCOPE (V12.23 rule — see below)
  ├─ GetUiSnapshot()
  ├─ SyncModeChipVisuals()
  ├─ UpdateContextualUI()
  ├─ SyncPanelConfigFromSnapshot()
  ├─ SyncCountChipVisuals()
  ├─ UpdateTargetVisibility()
  ├─ UpdateRmaButtonVisual()
  ├─ UpdateHubStatusLed()
  ├─ UpdateTelemetryDisplay()
  ├─ UpdateComplianceDisplay()
  ├─ UpdateTrendIndicator()
  ├─ SetConfigTargetButtonsVisible()
  ├─ SyncLiveTargetRows()
  └─ SetLiveTargetRowsVisible()
```

---

## Why Other Methods Are NOT in Scope (V12.23)

Per project rule **V12.23** (single-method-per-epic discipline), each EPIC targets exactly one
high-complexity method for the full refactor cycle. Violating this boundary by pulling downstream
callees into scope would:

1. **Expand blast radius unpredictably.** `SyncPanelConfigFromSnapshot` alone writes 10+ fields;
   touching it in the same epic would require a separate hotspot analysis, plan, and verification
   cycle that is not budgeted here.
2. **Break wave isolation.** Wave 7 tracks complexity metrics per method. Mixing multiple methods
   into a single epic corrupts per-method CYC telemetry and makes regression attribution ambiguous.
3. **Undermine incremental safety.** The 14 downstream callees of `UpdatePanelState` each have their
   own CYC profiles (see `00-hotspots.md` blast radius table). Refactoring any of them without a
   dedicated Phase 0 hotspot analysis risks introducing regressions that are invisible to the
   current epic's verification gate.
4. **V12.23 explicit text:** *"An epic MUST contain exactly one method under active refactor.
   Callers and callees are documented for blast-radius awareness only."* All 14 callees and both
   callers named in this document are present for awareness only; they are not modified in any
   phase of EPIC-W7-013.

The three *new* helper methods (`TryUpdateTargetCountChip`, `ApplyLivePositionView`,
`TeardownLivePositionView`) are considered part of the **single method** scope because they are
pure extractions — they contain code that currently lives inside `UpdatePanelState` and do not add
new behaviour. Their introduction does not change any existing call site.

---

## Summary

- **Epic:** EPIC-W7-013
- **Wave:** 7
- **Phase:** 1 — Scope Definition
- **Single method** in scope: `UpdatePanelState`
- **Source file:** `src/V12_002.UI.Panel.StateSync.cs`
- **Current CYC:** 8 | **Target CYC:** ≤ 8
- **Callers count:** 2 (`V12_002.UI.Panel.Lifecycle.cs:81`, `V12_002.UI.Panel.Construction.cs:230`)
- **Scope boundary** confirmed: no callee or caller is modified in this epic
- Out-of-scope exclusion justified by **V12.23** single-method-per-epic rule

---

## Agent Tracking

| Field | Value |
|-------|-------|
| **Agent Name** | v12-phase1-scope |
| **Wave / Phase** | Wave 7 / Phase 1 |
| **Epic** | EPIC-W7-013 |
| **Bobcoins Used** | 3 |
| **Execution Time** | ~30 s |
| **Source Verified** | `src/V12_002.UI.Panel.StateSync.cs` — read; grep confirmed 2 callers |
| **Output** | `docs/brain/EPIC-W7-013/00-scope.md` |
| **Previous Phase** | Phase 0 output: `00-hotspots.md` (status: completed) |
