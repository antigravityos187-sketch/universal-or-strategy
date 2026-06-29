# EPIC-W7-019 — Phase 1: Scope Definition

## Single Method In Scope

| Field | Value |
|-------|-------|
| **Method** | `TryHandleFleet_MoveTarget` |
| **File** | `src/V12_002.UI.IPC.Commands.Fleet.cs` |
| **Lines** | 645–693 |
| **Current CYC** | 17 |
| **Target CYC** | ≤ 8 |
| **CYC Reduction Required** | −9 (minimum) |

This epic targets a **single method**: `TryHandleFleet_MoveTarget`. No other method is included in the refactor scope for this epic.

---

## Scope Boundary

The **scope boundary** is strictly limited to the body of `TryHandleFleet_MoveTarget` (lines 645–693 in `src/V12_002.UI.IPC.Commands.Fleet.cs`) and any new private helper methods extracted from it within the same partial class file. No changes are permitted to:

- The public/private signature of `TryHandleFleet_MoveTarget` itself (callers must not change)
- Any method in `src/V12_002.UI.IPC.Commands.Fleet.cs` other than `TryHandleFleet_MoveTarget` and its newly-extracted helpers
- Any file outside `src/V12_002.UI.IPC.Commands.Fleet.cs`

This boundary exists because the refactor is a pure complexity-reduction exercise. Crossing it would risk cascading side-effects in the IPC dispatch loop and live order-mutation paths.

---

## Callers Analysis

**Direct callers of `TryHandleFleet_MoveTarget`: 1**

| Caller | File | Line |
|--------|------|------|
| `TryHandleFleetCommand` | `src/V12_002.UI.IPC.Commands.Fleet.cs` | 72 |

`TryHandleFleet_MoveTarget` is a private method invoked from `TryHandleFleetCommand` — a chain-of-responsibility dispatcher that tries each handler in sequence. There is exactly **one call site** for `TryHandleFleet_MoveTarget`. The method signature `(string action, string[] parts) → bool` is preserved across all phases; no signature changes are required or permitted.

`TryHandleFleetCommand` itself is called from two higher-level sites:

| Caller of `TryHandleFleetCommand` | File | Line |
|-----------------------------------|------|------|
| IPC dispatch loop | `src/V12_002.UI.IPC.cs` | 466 |
| Panel handler dispatch | `src/V12_002.UI.Panel.Handlers.cs` | 952 |

These upstream callers are **outside the scope boundary** and will not be touched.

---

## Why Other Methods Are NOT In Scope

Version constraint **V12.23** mandates a single-method, zero-cascade refactor policy for Wave 7 hotspot epics. Specifically:

1. **`TryHandleFleetCommand`** (the caller) — excluded because it is a structural dispatcher and its CYC is within acceptable bounds; modifying it would expand the blast radius without addressing the hotspot.
2. **`MoveSpecificTargetAbsolute` / `MoveSpecificTarget`** (outbound callees, `src/V12_002.Trailing.Breakeven.cs`) — excluded because they reside in a different file and touch live mutable state (`activePositions`, broker order objects, SIMA FSM). Changes to these callees are outside the Wave 7 scope boundary and carry MEDIUM-HIGH blast radius risk.
3. **All other `TryHandleFleet_*` siblings** in `src/V12_002.UI.IPC.Commands.Fleet.cs` — excluded because their CYC scores are within target thresholds and V12.23 prohibits opportunistic refactoring of in-bounds methods during a focused hotspot epic.
4. **`src/V12_002.UI.IPC.cs` and `src/V12_002.UI.Panel.Handlers.cs`** — excluded because they are callers-of-callers and changing them would widen scope beyond the single method defined in this epic.

The V12.23 policy rationale: in a live-trading system with shared mutable state, widening scope mid-epic is the primary cause of regressions. The single method constraint is non-negotiable.

---

## CYC Reduction Plan (Summary)

Three targeted extractions will bring `TryHandleFleet_MoveTarget` from CYC **17** to residual CYC **≈ 4**, well below the ≤ 8 target:

| # | Extraction | New Helper | CYC Reduction |
|---|------------|-----------|---------------|
| 1 | Compound `targetId` validation guard | `TryParseTargetId(string, out int)` | −4 |
| 2 | `SET_TARGET_PRICE` absolute-move path | `TryHandleMoveTargetAbsolute(int, string)` | −3 |
| 3 | Relative-move path (`1pt` / `2pt`) | `TryHandleMoveTargetRelative(int, string)` | −3 |
| | **Residual CYC in parent** | | **≈ 4** |

All three extracted helpers are pure parsing/dispatch transformations with no direct access to shared mutable state.

---

## Source Context (Confirmed via grep)

```
Found 2 matches in *.cs files for "TryHandleFleet_MoveTarget":
  src/V12_002.UI.IPC.Commands.Fleet.cs  Line  72  — call site (inside TryHandleFleetCommand)
  src/V12_002.UI.IPC.Commands.Fleet.cs  Line 645  — definition
```

Caller count confirmed: **1** (single call site within the same file).

---

## Agent Tracking

| Field | Value |
|-------|-------|
| **Agent Name** | v12-phase1-scope |
| **Wave** | 7 |
| **Phase** | 1 — Scope Definition |
| **Epic** | EPIC-W7-019 |
| **Source File** | `src/V12_002.UI.IPC.Commands.Fleet.cs` |
| **Method In Scope** | `TryHandleFleet_MoveTarget` |
| **Current CYC** | 17 |
| **Target CYC** | ≤ 8 |
| **Callers Count** | 1 (`TryHandleFleetCommand`, same file, line 72) |
| **Scope Boundary** | Single method + newly extracted private helpers, same file only |
| **V12.23 Constraint** | Other methods excluded per single-method hotspot policy |
| **Output File** | `docs/brain/EPIC-W7-019/00-scope.md` |
