# EPIC-W7-050 — Phase 1: Scope Definition

---

## Method in Scope

| Field                    | Value                                              |
|--------------------------|----------------------------------------------------|
| **Method**               | `FleetSync_SyncFollowersToLevel`                   |
| **File**                 | `src/V12_002.Trailing.cs`                          |
| **Lines**                | 142–191                                            |
| **Class**                | `V12_002` (partial, `Strategy`)                    |
| **Module**               | Trailing Stops — Fleet Symmetry Sync               |
| **CYC (current)**        | 34                                                 |
| **CYC (target)**         | ≤ 8                                                |
| **Callers**              | 1 (see §Caller Inventory)                          |

This is a **single method** refactor. The scope boundary is drawn tightly around
`FleetSync_SyncFollowersToLevel` and nothing else. No other method body will be
modified during Phases 1–3 of this epic.

---

## Caller Inventory

A `grep` of `src/` for `FleetSync_SyncFollowersToLevel` returned **2 matches**:

| Match type  | Location                                          | Detail                                                        |
|-------------|---------------------------------------------------|---------------------------------------------------------------|
| Definition  | `src/V12_002.Trailing.cs:142`                     | Method declaration                                            |
| Call site   | `src/V12_002.Trailing.cs:115`                     | Called from `ManageTrail_RunFleetSymmetrySync` (1 caller)    |

**Total external callers: 1.**

`ManageTrail_RunFleetSymmetrySync` (line 115) is itself invoked every tick from
`ManageTrailingStops` when `EnableSIMA == true`. This single call site means that
every live follower position in SIMA multi-position mode flows through
`FleetSync_SyncFollowersToLevel` on every price update. There are no secondary
callers and no reflection-based invocation paths in the codebase.

---

## Scope Boundary

The **scope boundary** for this epic is defined as follows:

- **In scope:** the body of `FleetSync_SyncFollowersToLevel` and any private
  helper methods extracted *from* it during refactor (they do not exist yet and
  will be created net-new).
- **Out of scope:** all other methods in `src/V12_002.Trailing.cs`, the callee
  `UpdateStopOrder`, the callee `CalculateStopForLevel`, and the caller
  `ManageTrail_RunFleetSymmetrySync`. Their signatures and semantics must remain
  unchanged so that the refactor is purely internal.

The scope boundary is enforced at the method level; no class-level fields,
cross-file imports, or interface contracts will be altered.

---

## Why Other Methods Are NOT in Scope

Rule **V12.23** of the project's refactor governance policy states:

> *A wave-phase refactor targets one complexity hotspot at a time. Collateral
> changes to callees or callers require a separate epic, separate wave, and
> separate hotspot analysis.*

Applying V12.23 here:

- **`UpdateStopOrder`** — high complexity callee; warrants its own epic. Touching
  it here would widen the blast radius across all trailing-stop subsystems beyond
  what this epic's blast-radius analysis sanctioned.
- **`CalculateStopForLevel`** — low CYC (pure, 4-case switch), no complexity
  problem; no justification to modify it.
- **`ManageTrail_RunFleetSymmetrySync`** — sole caller; its call signature is the
  integration contract. Altering it would require re-validation of every upstream
  tick-handler, which is out of scope for a single-method refactor.
- **All other trailing handlers** (`TrailHandler_TREND_E1`, etc.) — share the
  directional-ternary pattern identified in the hotspot analysis, but each is a
  separate hotspot warranting its own wave entry per V12.23.

In summary: this epic refactors a **single method** and nothing else. V12.23
prohibits scope creep into callees or callers without a new epic charter.

---

## Complexity Reduction Plan (summary)

| Extraction                          | CYC removed | Rationale                                      |
|-------------------------------------|-------------|------------------------------------------------|
| `FleetSync_ValidateFollower`        | −5          | Consolidates 5-guard early-exit chain          |
| `FleetSync_ResolveTargetLevel`      | −3          | Isolates direction-dispatch ternary            |
| `FleetSync_IsStopImprovement`       | −3          | Encapsulates Long/Short stop-improvement check |
| Residual loop + orchestration       | ≈ 8         | Remaining orchestration logic                  |

Starting CYC: **34** → Target CYC: **≤ 8** (−26 decision points via 3 extractions).

---

## Agent Tracking

```
Agent Name:  v12-phase1-scope
EPIC:        EPIC-W7-050
WAVE:        7
PHASE:       1 — Scope Definition
STATUS:      completed
OUTPUT:      docs/brain/EPIC-W7-050/00-scope.md
METHOD:      FleetSync_SyncFollowersToLevel
CYC_CURRENT: 34
CYC_TARGET:  <=8
SOURCE_FILE: src/V12_002.Trailing.cs:142
CALLERS:     1 (ManageTrail_RunFleetSymmetrySync @ line 115)
SCOPE_RULE:  V12.23 — single method per wave-phase refactor
TIMESTAMP:   2025-07-15
```
