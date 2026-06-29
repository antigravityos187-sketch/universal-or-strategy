# EPIC-W7-012 — Phase 1: Scope Definition

## Single Method in Scope

This epic targets exactly one method. The **single method** under refactor is:

| Field | Value |
|---|---|
| **Method** | `SyncPanelConfigFromSnapshot` |
| **File** | `src/V12_002.UI.Panel.StateSync.cs` |
| **Lines** | 460–512 |
| **Class** | `V12_002` (partial) |
| **Namespace** | `NinjaTrader.NinjaScript.Strategies` |
| **Current CYC** | 34 |
| **Target CYC** | ≤ 8 |
| **CYC Reduction Required** | ≥ 26 points |

No other existing method enters the scope boundary at any phase of this epic.

---

## Caller Analysis

`SyncPanelConfigFromSnapshot` was located via `search_symbols` query against repo
`universal-or-strategy` and confirmed by static grep across the full `src/` tree.

| Caller | File | Line | Call Site Guard |
|---|---|---|---|
| `UpdatePanelState` | `src/V12_002.UI.Panel.StateSync.cs` | 42 | `snapshot.ConfigRevision != _panelAppliedConfigRevision` |

**Callers count: 1**

`SyncPanelConfigFromSnapshot` is declared `private` and has exactly **1 caller** in the entire
repository source tree. The call site is inside `UpdatePanelState` (same partial-class file,
line 42), wrapped in a config-revision divergence guard. No other `.cs` file in `src/` contains
a reference. This single-caller topology makes the method self-contained: any internal
restructuring (extraction of helper methods) requires no changes to any external call site.

---

## Scope Boundary Statement

> **Only `SyncPanelConfigFromSnapshot` and its new extracted helper methods are in scope.**

The **scope boundary** is strictly limited to the body of `SyncPanelConfigFromSnapshot`
(lines 460–512, `src/V12_002.UI.Panel.StateSync.cs`) and the new private helper methods that
will be created as outputs of this epic's refactor phase. No method that exists before the
refactor — including the sole caller `UpdatePanelState` and all downstream callees — falls
inside this scope boundary.

The scope boundary is enforced by protocol **V12.23 — No Scope Creep** (see section below).

---

## Why Other Methods Are NOT in Scope (V12.23 Protocol)

Protocol **V12.23** mandates that each epic targets a **single method**. The following methods
are explicitly excluded from scope, with individual rationale:

| Method | Reason Excluded |
|---|---|
| `UpdatePanelState` | Caller — its control flow and guard logic are NOT modified; it receives the same call signature post-refactor |
| `SetComboSelection` | Callee — treated as a stable, black-box API; not restructured |
| `SyncCountChipVisuals` | Callee — repaint logic is out of scope; called unchanged from the residual method body |
| `UpdateTargetVisibility` | Callee — already extracted (EPIC-CCN-16); touched only as a call target |
| `GetPanelTargetModeText` | Pure read-only helper — no mutations; not modified |
| `FormatPanelDouble` | Pure read-only formatter — not modified |
| `SyncModeChipVisuals` | Sibling method in `UpdatePanelState` — unrelated dispatch path |
| `UpdateContextualUI` | Sibling method in `UpdatePanelState` — unrelated dispatch path |
| Any method outside `src/V12_002.UI.Panel.StateSync.cs` | Cross-file changes are prohibited under V12.23 for a single-method epic |

V12.23 rationale: expanding scope to caller or callee methods introduces correlated blast
radius, complicates PR review, and risks observable behavioural change in paths unrelated to
the CYC reduction goal. The single-method constraint is the primary guard against this class
of scope creep.

---

## Planned Extractions (from Phase 0)

The following **3 new private helper methods** will be introduced into
[`src/V12_002.UI.Panel.StateSync.cs`](src/V12_002.UI.Panel.StateSync.cs) exclusively to reduce
the residual CYC of `SyncPanelConfigFromSnapshot`. These are the only new symbols created:

| New Method | CYC Reduction | Source Lines Extracted |
|---|---|---|
| `SyncTargetValueFields(UIConfigSnapshot)` | ~5 | 463–472 (5 null-guard + assign pairs) |
| `SyncTargetTypeFields(UIConfigSnapshot)` | ~5 | 474–483 (5 `SetComboSelection` call pairs) |
| `ApplyCountStateFromSnapshot(UIStateSnapshot)` | ~3 | 508–511 (count mutation + visual calls) |

Post-extraction residual CYC estimate for `SyncPanelConfigFromSnapshot`: **≤ 8**
(1 stop-type mode-branch + 3 single null-guards for `strVal`/`maxVal`/`citVal` + 3 delegation
calls to the new helpers).

These three extracted methods are **in scope** as outputs of this epic. No other methods are
created, renamed, deleted, or have their signatures modified.

---

## Out-of-Scope Items

The following are explicitly **out of scope** for EPIC-W7-012, per the scope boundary defined
above and protocol V12.23:

- `UpdatePanelState` — sole caller; not modified (scope boundary does not extend to callers)
- `SetComboSelection`, `SyncCountChipVisuals`, `UpdateTargetVisibility` — callees; stable APIs
- `GetPanelTargetModeText`, `FormatPanelDouble` — pure helpers; not modified
- All files outside `src/V12_002.UI.Panel.StateSync.cs`
- UI layout, WPF XAML bindings, or control construction (lives in `V12_002.UI.Panel.Construction.cs`)
- The `_panelChipClickTicks` guard logic in `UpdatePanelState` (lines 49–56) — separate concern
- Any field, property, or event handler not directly referenced inside `SyncPanelConfigFromSnapshot`

---

## Dependency Map

```
UpdatePanelState (line 13)  [NOT in scope]
  └── SyncPanelConfigFromSnapshot (line 460)  ← SINGLE METHOD IN SCOPE
        ├── [NEW] SyncTargetValueFields(UIConfigSnapshot)   ← extracted helper (in scope)
        ├── [NEW] SyncTargetTypeFields(UIConfigSnapshot)    ← extracted helper (in scope)
        ├── [NEW] ApplyCountStateFromSnapshot(UIStateSnapshot) ← extracted helper (in scope)
        ├── FormatPanelDouble(double)           [NOT in scope — read-only helper]
        ├── SetComboSelection(ComboBox, string) [NOT in scope — stable callee]
        ├── GetPanelTargetModeText(enum)        [NOT in scope — read-only helper]
        ├── SyncCountChipVisuals(int)           [NOT in scope — stable callee]
        └── UpdateTargetVisibility(int)         [NOT in scope — stable callee]
```

The extraction plan preserves the observable update ordering constraint identified in Phase 0:
TextBox assigns → ComboBox assigns → count chip visuals → target visibility. The residual
`SyncPanelConfigFromSnapshot` body will delegate to extracted helpers in this exact sequence.
`UpdatePanelState` continues to call the same single-method entry point with no signature change.

---

## Symbol Search Evidence

`search_symbols` was queried against repo `universal-or-strategy` with query
`SyncPanelConfigFromSnapshot`. Static grep across `src/` confirms:

- **Definition**: `src/V12_002.UI.Panel.StateSync.cs:460` — `private void SyncPanelConfigFromSnapshot(UIStateSnapshot snapshot)`
- **Call site**: `src/V12_002.UI.Panel.StateSync.cs:42` — inside `UpdatePanelState`, under `ConfigRevision` divergence guard
- **Other source files with call sites**: 0
- **Total callers in `src/`**: 1 (`UpdatePanelState`)

---

## Agent Tracking

```
EPIC:             EPIC-W7-012
Wave:             7
Phase:            1 — Scope Definition (REDO)
Agent Name:       v12-phase1-scope
Status:           completed
Output:           docs/brain/EPIC-W7-012/00-scope.md
Method in scope:  SyncPanelConfigFromSnapshot
File:             src/V12_002.UI.Panel.StateSync.cs
Callers count:    1 (UpdatePanelState, same file, line 42)
Current CYC:      34
Target CYC:       <= 8
Scope boundary:   Only SyncPanelConfigFromSnapshot and its new extracted helper methods
V12.23:           Enforced — single method scope, no caller/callee modifications
Extractions:      3 planned (SyncTargetValueFields, SyncTargetTypeFields, ApplyCountStateFromSnapshot)
Timestamp:        2025-07-15
```
