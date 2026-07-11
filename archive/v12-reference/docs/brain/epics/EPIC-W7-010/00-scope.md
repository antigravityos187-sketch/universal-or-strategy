# EPIC-W7-010 — Phase 1: Scope Definition

## Method in Scope

This epic targets a **single method**. The scope boundary is absolute and enforced by the
V12.23 No Scope Creep Protocol (see below).

| Field                 | Value                                                    |
|-----------------------|----------------------------------------------------------|
| **Method**            | `ShowModeSpecificControls`                               |
| **File**              | `src/V12_002.UI.Panel.Handlers.cs`                       |
| **Lines**             | 690–719                                                  |
| **Class**             | `V12_002` (partial), namespace `NinjaTrader.NinjaScript.Strategies` |
| **Current CYC**       | 8                                                        |
| **Target CYC**        | ≤ 8                                                      |
| **CYC Status**        | At threshold — boundary-compliant today; one new mode pushes to 9 |
| **Wave**              | 7                                                        |
| **Epic**              | EPIC-W7-010                                              |

## CYC Rationale

`ShowModeSpecificControls` is a pure dispatch switch (post-EPIC-CCN-15 refactor). It contains
1 entry point + 7 decision branches (`ORB`, `RMA`, `RETEST`, `MOMO`, `FFMA`, `TREND`, `MNL`,
plus `default`) = **CYC 8**. The current CYC exactly meets the Jane Street ultra-alignment
ceiling of ≤ 8. No structural violation exists today; the target is to hold at CYC ≤ 8
and provide future headroom via an optional dictionary-dispatch pattern.

## Callers

Symbol search on `universal-or-strategy` confirms the following call sites:

| Role                  | Caller                                | Location                                      |
|-----------------------|---------------------------------------|-----------------------------------------------|
| **Direct caller (1)** | `UpdateContextualUI(string mode)`     | `src/V12_002.UI.Panel.Handlers.cs:661`        |
| Transitive caller     | `SelectConfigMode(string, Button)`    | `src/V12_002.UI.Panel.Handlers.cs:626`        |
| Transitive caller     | `UpdatePanelState`                    | `src/V12_002.UI.Panel.StateSync.cs:37`        |
| Transitive caller     | Construction initialiser              | `src/V12_002.UI.Panel.Construction.cs:217`    |

**Direct callers count: 1** (`UpdateContextualUI`)

The method signature `ShowModeSpecificControls(string mode)` is unchanged by any planned
refactor; all upstream callers remain unaffected and are outside the scope boundary.

## Scope Boundary

This epic covers exactly one unit of work: the body of the **single method**
`ShowModeSpecificControls` within `src/V12_002.UI.Panel.Handlers.cs`.

The scope boundary is defined as:

1. The body of `ShowModeSpecificControls` (lines 690–719) — the switch dispatch logic.
2. Optionally: a new `_modeControlShower` dictionary field and its initialiser call, both
   within the same partial-class file, **if** the dictionary-dispatch pattern is adopted in a
   later phase. No other file is modified.

Nothing outside these two items falls inside the scope boundary.

## Why Other Methods Are NOT in Scope (V12.23)

The **V12.23 No Scope Creep Protocol** mandates: *one epic = one concern*. Every changed line
must trace directly to the stated CYC-reduction goal. Methods outside `ShowModeSpecificControls`
are excluded for the following reasons:

| Method / Item                                      | Why Excluded                                                                         |
|----------------------------------------------------|--------------------------------------------------------------------------------------|
| `ShowOrbControls` … `ShowMnlControls` (×7 helpers) | Already extracted by EPIC-CCN-15; bodies are not touched — separate prior concern    |
| `CollapseAllExecutionControls`                     | Sibling helper called by `UpdateContextualUI`, not by `ShowModeSpecificControls`     |
| `UpdateContextualUI`                               | Direct upstream caller; contract unchanged — V12.23 prohibits opportunistic cleanup  |
| `PopulateDirectionCombo`                           | Sibling helper in same caller chain; separate concern, separate epic if needed        |
| `SelectConfigMode`, `UpdatePanelState`, Construction init | Transitive callers; fully isolated — V12.23 prohibits cascading scope   |
| `ShowFfmaControls` asymmetric side-effect fix      | Latent correctness risk flagged in Phase 0; separate concern requiring a separate ticket |
| All other methods in `src/V12_002.UI.Panel.Handlers.cs` | Same file, different responsibilities — V12.23 prohibits opportunistic cleanup |

**Rule citation:** V12.23 No Scope Creep Protocol — a wave targets a single declared hotspot
method. Touching callers, callees, or sibling methods is explicitly forbidden unless a
separate epic is opened for each additional concern.

## Phase 0 Hotspot Summary (Reference)

- **Primary driver:** 7-arm string `switch` on mode identity — accounts for all 7 independent
  paths (CYC = 7 branches + 1 entry = 8).
- **Secondary driver:** `default:` arm silently aliases `ShowOrbControls()` — hidden coupling,
  no error signal for unrecognised modes.
- **Tertiary driver:** Open-closed violation — each new mode requires editing this switch,
  pushing CYC above 8 at the next mode addition.
- **Recommended extraction count (Phase 0 verdict):** 0 additional extractions required.
  Dictionary-dispatch refactor deferred until CYC would breach 8.

## Blast Radius Summary

| Dimension               | Detail                                                                    |
|-------------------------|---------------------------------------------------------------------------|
| **Direct callers**      | 1 — `UpdateContextualUI`                                                  |
| **Dispatched callees**  | 7 leaf helpers (`ShowOrbControls` … `ShowMnlControls`); bodies unchanged  |
| **UI elements touched** | `orLongButton`, `orShortButton`, `rmaButton`, `execRetestRow`, `momoButton`, `ffmaButton`, `ffmaManualButton`, `manualEntryRow`, `execTrendRow`, `mButton` |
| **Blast scope**         | Medium — isolated to UI visibility toggling; no state mutations, no order submission |
| **External assemblies** | None — method is `private`, package-internal only                         |

## Agent Tracking Block

```
Agent Name:     v12-phase1-scope
Bobcoins Used:  1.0
Execution Time: ~60s
EPIC:           EPIC-W7-010
Wave:           7
Phase:          1 — Scope Definition (REDO)
Method:         ShowModeSpecificControls
File:           src/V12_002.UI.Panel.Handlers.cs
CYC Current:    8
CYC Target:     <= 8
Callers Count:  1 direct (UpdateContextualUI)
Scope:          single method
Protocol:       V12.23 No Scope Creep
Input:          docs/brain/EPIC-W7-010/00-hotspots.md
Output:         docs/brain/EPIC-W7-010/00-scope.md
Status:         completed
Authored-by:    Bob (AI assistant)
Timestamp:      2025-07-14T00:00:00Z
```
