# EPIC-W7-051 — Phase 1: Scope Definition

## Single Method in Scope

This epic targets a **single method**: `UpdateStopOrder`, defined in
[`src/V12_002.Trailing.StopUpdate.cs`](../../src/V12_002.Trailing.StopUpdate.cs) at line 84,
inside the partial class `V12_002` (file partition: `Trailing.StopUpdate`).

| Field                  | Value                                    |
|------------------------|------------------------------------------|
| **Method**             | `UpdateStopOrder`                        |
| **File**               | `src/V12_002.Trailing.StopUpdate.cs`     |
| **Class**              | `V12_002` (partial — Trailing.StopUpdate)|
| **Lines (body)**       | 84–139 (56 lines)                        |
| **CYC (hotspot phase)**| 6 (6 analysed decision branches)         |
| **CYC target**         | ≤ 8 (task-header target; current CYC 6 already meets threshold; refactor goal is ≤ 3 via 3 recommended extractions) |
| **Wave / Phase**       | Wave 7 / Phase 1                         |

---

## Scope Boundary

The **scope boundary** for this epic is drawn precisely at the `UpdateStopOrder` method
signature. Work performed under EPIC-W7-051 is authorised to:

1. Refactor the body of `UpdateStopOrder` (lines 84–139).
2. Extract private helpers called exclusively from within `UpdateStopOrder`'s routing logic,
   specifically the three recommended extractions identified in Phase 0:
   - `ResolveStopRoute(entryName, currentStop)` → routing enum
   - `IncrementAndCheckCircuitBreaker()` → deduplicated safety counter
   - `BuildTargetSnapshot(entryName)` → consolidated bracket-capture loop
3. Update XML-doc comments on `UpdateStopOrder` to reflect the new shape.

Work that is **outside** the scope boundary:
- Changes to any of the 7 caller files (see Callers section below).
- Changes to the 4 sibling helpers (`HandleStalePendingReplacement`,
  `UpdateExistingPendingReplacement`, `InitiateStopReplacement`, `CreateDirectStopOrder`)
  beyond what is strictly required to wire the extracted helpers.
- Changes to shared state fields (`pendingStopReplacements`, `stopOrders`,
  `pendingReplacementCount`, `circuitBreakerActive`) beyond renaming/inline calls.
- Any work touching the 13 transitively-dependent files.

---

## Callers

`grep src/ -r UpdateStopOrder` (excluding comments and the definition itself) returned
**15 direct call sites across 7 files**, confirming the blast-radius reported in Phase 0.

| Caller file                                       | Call sites |
|---------------------------------------------------|-----------|
| `src/V12_002.Trailing.cs`                         | 5         |
| `src/V12_002.UI.Callbacks.cs`                     | 4         |
| `src/V12_002.Trailing.Breakeven.cs`               | 2         |
| `src/V12_002.SIMA.Shadow.cs`                      | 1         |
| `src/V12_002.Orders.Callbacks.Propagation.cs`     | 1         |
| `src/V12_002.Symmetry.Replace.cs`                 | 1         |
| `src/V12_002.UI.IPC.Commands.Mode.cs`             | 1         |

**Total: 15 call sites / 7 caller files.**

None of these caller files are in scope for modification. The public signature of
`UpdateStopOrder` (`string entryName, PositionInfo pos, double newStopPrice, int newTrailLevel`)
must remain unchanged so that all 15 call sites continue to compile without modification.

---

## Why Other Methods Are NOT in Scope

Per V12.23 policy, scope for a complexity-reduction epic is restricted to the **single method**
named in the epic header. Broadening scope to include callers or sibling helpers would:

1. **Violate the V12.23 single-method rule** — V12.23 requires that each refactor epic
   declare exactly one method as its refactor target and hold that boundary through all phases.
   Expanding to callers or helpers would turn a focused CYC-reduction task into an
   undifferentiated redesign with unbounded blast radius.

2. **Create cross-cutting merge risk** — All 7 caller files are actively modified by other
   waves and epics. Touching them here would introduce merge conflicts and require
   re-validation of the entire order-management subsystem.

3. **Obscure the regression signal** — The value of a narrow scope is that any test failure
   after refactoring is provably caused by the single changed method. Widening scope destroys
   that isolation guarantee.

4. **Exceed the CYC budget** — The sibling helpers (`UpdateExistingPendingReplacement`,
   `InitiateStopReplacement`) each carry their own CYC and are candidates for separate epics.
   Including them here would inflate this epic's complexity beyond the ≤ 8 target.

The only exception is the three *new* private helpers that will be **created** as part of this
epic's extractions — those are within scope because they do not exist yet and are solely owned
by `UpdateStopOrder`.

---

## Current vs. Target CYC

| Metric           | Value |
|------------------|-------|
| CYC (reported)   | 0 (seed input from task header) |
| CYC (analysed)   | 6 (Phase 0 decision-branch count) |
| CYC target       | ≤ 8 (task constraint); ≤ 3 (recommended post-extraction) |
| Extractions plan | 3 (see Phase 0 hotspots doc for detail) |

The analysed CYC of 6 already satisfies the ≤ 8 threshold. The refactor proceeds to drive CYC
down to ≤ 3 by extracting the three identified complexity drivers, making future maintenance
and testing of `UpdateStopOrder` substantially safer given its 15-caller blast radius.

---

## Agent Tracking

```
epic_id:          EPIC-W7-051
wave:             7
phase:            1
Agent Name:       v12-phase1-scope
status:           completed
output:           docs/brain/EPIC-W7-051/00-scope.md
source_file:      src/V12_002.Trailing.StopUpdate.cs
method:           UpdateStopOrder
cyc_current:      6
cyc_target:       <=8 (goal <=3)
callers_count:    15
caller_files:     7
scope_type:       single method
scope_boundary:   UpdateStopOrder signature only
v12_policy:       V12.23
completed_at:     2025-07-11
```
