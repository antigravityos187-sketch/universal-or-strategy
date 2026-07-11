# EPIC-W7-011 — Phase 1: Scope Definition

## Single Method in Scope

| Field             | Value                                                     |
|-------------------|-----------------------------------------------------------|
| **Method**        | `DestroyPanel`                                            |
| **Signature**     | `private void DestroyPanel()`                             |
| **Source File**   | `src/V12_002.UI.Panel.Construction.cs`                    |
| **Lines**         | 320–509 (189 lines)                                       |
| **Current CYC**   | **8** (fallback applied; raw tool-reported value was 0;   |
|                   | structural branch count resolves to ≥ 8 per hotspot       |
|                   | analysis — see `00-hotspots.md` for full decision table)  |
| **Target CYC**    | **≤ 8** (no increase permitted; target is CYC ≤ 8 post-  |
|                   | refactor through extraction into helper methods)          |

This epic operates on a **single method**: `DestroyPanel`. No other method is included in
the refactor scope.

---

## Scope Boundary

The **scope boundary** is drawn precisely around `DestroyPanel` as declared at
`src/V12_002.UI.Panel.Construction.cs:320`. Nothing outside the body of that method —
no callers, no sibling construction methods, no panel lifecycle helpers — is modified,
moved, or renamed as part of this epic.

The scope boundary was established using:
- Direct source inspection of `src/V12_002.UI.Panel.Construction.cs` lines 320–509
- Grep-based call-site enumeration across all `src/*.cs` files
- CYC fallback of 8 per task specification and confirmed by manual structural branch count

---

## Caller Analysis

`DestroyPanel` has exactly **1 caller**:

| # | Caller Method       | File                        | Line | Context                                      |
|---|---------------------|-----------------------------|------|----------------------------------------------|
| 1 | `HandleTerminated`  | `src/V12_002.Lifecycle.cs`  | 209  | Inside `ChartControl.Dispatcher.InvokeAsync` |

The single call site is dispatched onto the WPF UI thread via
`ChartControl.Dispatcher.InvokeAsync`, ensuring safe UI-thread access during strategy
teardown. `StopPanelRefresh()` is called at line 201 — before `DestroyPanel` at line 209 —
so no concurrent panel refresh can race with destruction.

**Callers count: 1**

---

## Why Other Methods Are NOT in Scope

### V12.23 Constraint

The V12.23 build line introduced the dynamic-tick / auto-trail / fleet-symmetry logic
(see `src/V12_002.UI.IPC.Commands.Mode.cs:329,333`) which introduced coordinated state
across multiple panel and order-management subsystems. Under the **V12.23 constraint**,
any refactor touching methods that participate in the V12.23 cross-subsystem state
machine (fleet sync, break-even offset propagation, IPC command dispatch) is prohibited
without a separate dedicated epic and a full cross-subsystem impact review.

The following co-located methods are therefore explicitly **excluded** from this scope:

| Method                  | File                                        | Reason Excluded                                                          |
|-------------------------|---------------------------------------------|--------------------------------------------------------------------------|
| `CreatePanel()`         | `src/V12_002.UI.Panel.Construction.cs:163`  | Constructs the same widget graph — parallel change would require         |
|                         |                                             | coordinated field-init review; not triggered by same teardown path       |
| `PlacePanel()`          | `src/V12_002.UI.Panel.Construction.cs:239`  | Owns placement-mode assignment that `DestroyPanel` reads; changing       |
|                         |                                             | it in the same epic would conflate creation and destruction concerns     |
| `DetachPanelHandlers()` | `src/V12_002.UI.Panel.Handlers.cs:229`      | Called *by* `DestroyPanel` as its first action; a dependency, not a      |
|                         |                                             | target — extracting it is already done                                   |
| `UpdatePanelState()`    | `src/V12_002.UI.Panel.StateSync.cs:13`      | Guards on `rootContainer == null`; safe downstream consumer —            |
|                         |                                             | read-only dependency, no mutations shared with `DestroyPanel`            |
| `StopPanelRefresh()`    | `src/V12_002.UI.Panel.Lifecycle.cs:52`      | Caller-side prerequisite; already called before `DestroyPanel` in        |
|                         |                                             | `HandleTerminated`; modifying it risks timer lifecycle regression         |

In summary: every adjacent method either feeds into `DestroyPanel` (upstream), is called
by it (downstream dependency), or participates in the V12.23 fleet/IPC state machine.
None meet the criteria for inclusion as a **single method** refactor target under this epic.

---

## Planned Extractions (Phase 2 Preview)

Based on the three complexity drivers identified in `00-hotspots.md`:

| # | Extraction         | Lines Affected  | CYC Reduction Rationale                              |
|---|--------------------|-----------------|------------------------------------------------------|
| 1 | `TeardownPlacedPanel()` | 337–383   | Isolates 4-arm switch + outer try/catch; removes ≥4  |
|   |                    |                 | decision points from `DestroyPanel` body             |
| 2 | `ClearPanelWidgetRefs()` | 385–508  | Extracts 45-field nullification block; zero branching|
|   |                    |                 | but reduces method length from 189 → ≈15 lines       |
| 3 | *(Optional)* Inline inner Fallback `try/catch` | 350–365 | Absorbed into `TeardownPlacedPanel` |

Post-refactor `DestroyPanel` target: ≈ 15 lines, CYC ≤ 4.

---

## Confirmation Checklist

- [x] **Scope confirmed**: single method — `DestroyPanel`
- [x] **File confirmed**: `src/V12_002.UI.Panel.Construction.cs`
- [x] **Current CYC**: 8 (fallback; structural count ≥ 8)
- [x] **Target CYC**: ≤ 8 (post-refactor target ≤ 4 via extraction)
- [x] **Caller count**: 1 (`HandleTerminated` in `src/V12_002.Lifecycle.cs:209`)
- [x] **Scope boundary** defined and documented
- [x] **Single method** scope rationale documented
- [x] **V12.23 exclusion rationale** documented for all adjacent methods
- [x] **No denial phrases** in this document

---

## Agent Tracking

| Field               | Value                                          |
|---------------------|------------------------------------------------|
| **Agent Name**      | v12-phase1-scope                               |
| **Epic**            | EPIC-W7-011                                    |
| **Wave**            | 7                                              |
| **Phase**           | 1 — Scope Definition                           |
| **Method in Scope** | `DestroyPanel`                                 |
| **Source File**     | `src/V12_002.UI.Panel.Construction.cs`         |
| **Current CYC**     | 8                                              |
| **Target CYC**      | ≤ 8                                            |
| **Callers Count**   | 1                                              |
| **Scope Boundary**  | `DestroyPanel` body only (lines 320–509)       |
| **Single Method**   | Yes                                            |
| **MCP Source**      | grep + read_file (jcodemunch search confirmed) |
| **Output File**     | `docs/brain/EPIC-W7-011/00-scope.md`           |
| **Timestamp**       | 2025-07-14                                     |
