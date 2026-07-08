# EPIC-W7-036 — Phase 1: Scope Definition

---

## Single Method In Scope

This phase targets exactly one **single method**: [`MoveStop_SinglePosition`](src/V12_002.Trailing.Breakeven.cs:73).
No additional methods are included within the scope boundary of Phase 1.

| Field               | Value                                            |
|---------------------|--------------------------------------------------|
| **Method**          | `MoveStop_SinglePosition`                        |
| **Class**           | `V12_002` (partial class, inherits `Strategy`)   |
| **Source File**     | `src/V12_002.Trailing.Breakeven.cs`              |
| **CYC (current)**   | 34                                               |
| **CYC (target)**    | ≤ 8                                              |
| **Wave / Phase**    | Wave 7 / Phase 1                                 |
| **Epic**            | EPIC-W7-036                                      |

---

## Scope Boundary

The **scope boundary** for this phase is defined as follows:

- **In scope:** The body of `MoveStop_SinglePosition` (lines 73–163 of
  `src/V12_002.Trailing.Breakeven.cs`) and the three private helper methods to be
  extracted from it (`ComputeBreakevenStopPrice`, `IsBetterStop`,
  `ApplyFollowerBreakeven`). All new helpers are added to the same partial-class file
  and remain package-private to the `V12_002` partial class.

- **At the scope boundary:** The single direct caller
  `MoveStopsToBreakevenWithOffset` (same file, line 59) — its call-site signature
  must remain unchanged. No modification to `MoveStopsToBreakevenWithOffset` itself
  is permitted.

- **Outside the scope boundary:** All other files in the project, including every
  method listed in the blast-radius table from Phase 0 (`UpdateStopOrder`,
  `ManageTrail_EvaluateManualBreakeven`, `ManageTrailingStops`, state flags in
  `V12_002.PositionInfo.cs`, persistence layer in `V12_002.StickyState.cs`, IPC
  command handler in `V12_002.UI.IPC.Commands.Mode.cs`, propagation and execution
  callback files). These files are read-only artefacts for this phase.

---

## Callers

| # | Caller Type     | Caller Symbol                    | File                                   | Line |
|---|-----------------|----------------------------------|----------------------------------------|------|
| 1 | Direct caller   | `MoveStopsToBreakevenWithOffset` | `src/V12_002.Trailing.Breakeven.cs`    | 59   |
| 2 | Indirect caller | `TryHandleBreakeven`             | `src/V12_002.UI.IPC.Commands.Mode.cs`  | 340  |

**Total callers confirmed:** 2 (1 direct, 1 indirect via `MoveStopsToBreakevenWithOffset`).

Grep over `src/` for `MoveStop_SinglePosition` returned exactly **2 hits**: the
call-site at line 59 and the definition at line 73, both within
`src/V12_002.Trailing.Breakeven.cs`. The indirect caller (`TryHandleBreakeven`)
reaches the method through `MoveStopsToBreakevenWithOffset` and does not reference
`MoveStop_SinglePosition` by name.

---

## Why Other Methods Are NOT In Scope

The strategy codebase is structured as a large partial class (`V12_002`) spread
across 45+ source files following the **V12.23** partial-class convention. Under
V12.23, each `.cs` file owns a distinct sub-domain of the strategy's behaviour
(trailing, breakeven, IPC, orders, UI, etc.). This convention imposes the following
constraints on scope for any single refactor phase:

1. **V12.23 partial-class boundary rule:** Helpers extracted from a method must live
   in the same partial-class file as the method being refactored. Cross-file helper
   injection violates the V12.23 file-ownership rule and is not permitted.

2. **Blast-radius isolation:** The 9 files identified in Phase 0 (hotspot analysis)
   are downstream consumers or side-effect targets of `MoveStop_SinglePosition`.
   Modifying those files in the same phase would expand the regression surface beyond
   what a single focused review can safely validate.

3. **Single-method discipline:** The EPIC-W7-036 work order specifies a
   **single method** per refactor phase. Expanding scope to additional methods (e.g.
   `MoveStopsToBreakevenWithOffset`, `UpdateStopOrder`, or any trailing-module
   orchestrator) would violate this discipline and risk CYC regression in methods
   not currently targeted.

4. **Caller signature freeze:** `MoveStopsToBreakevenWithOffset` (the direct caller)
   is itself called by the IPC command handler. Changing its signature would require
   coordinated changes across the scope boundary into `V12_002.UI.IPC.Commands.Mode.cs`,
   which is explicitly out of scope.

In summary, all methods other than `MoveStop_SinglePosition` are excluded from the
scope boundary because: (a) V12.23 file-ownership rules restrict cross-file helper
placement, (b) blast-radius safety requires downstream files to remain untouched, and
(c) the single-method work-order discipline is a hard constraint for Phase 1.

---

## CYC Reduction Plan (Summary)

| Extraction             | Proposed Helper                     | Estimated CYC Reduction |
|------------------------|-------------------------------------|-------------------------|
| Follower fast-path     | `ApplyFollowerBreakeven(...)`        | −12                     |
| Shared direction test  | `IsBetterStop(PositionInfo, double)` | −4                      |
| Price computation      | `ComputeBreakevenStopPrice(...)`     | −2                      |
| **Orchestrator residual** | `MoveStop_SinglePosition` (slim) | **CYC ≈ 6–8**           |

Starting CYC **34** → target CYC **≤ 8** after 3 extractions.

---

## Phase Inputs / Outputs

| Item       | Value                                             |
|------------|---------------------------------------------------|
| Input      | `docs/brain/EPIC-W7-036/00-hotspots.md` (Phase 0 output) |
| Output     | `docs/brain/EPIC-W7-036/00-scope.md` (this file) |
| Next phase | Phase 2 — Refactor Orchestrator                  |

---

## Agent Tracking Block

```
EPIC               : EPIC-W7-036
Wave               : 7
Phase              : 1 (Scope Definition)
Status             : completed
Output             : docs/brain/EPIC-W7-036/00-scope.md
Agent Name         : v12-phase1-scope
Method             : MoveStop_SinglePosition
Source             : src/V12_002.Trailing.Breakeven.cs
CYC_current        : 34
CYC_target         : <= 8
Callers_direct     : 1  (MoveStopsToBreakevenWithOffset, same file, line 59)
Callers_indirect   : 1  (TryHandleBreakeven → MoveStopsToBreakevenWithOffset, V12_002.UI.IPC.Commands.Mode.cs:340)
Callers_total      : 2
Scope_boundary     : single method — MoveStop_SinglePosition only
V12_convention     : V12.23 partial-class file-ownership rule enforced
Blast_files        : 9 (read-only for this phase)
Extractions_planned: 3
Timestamp          : 2025-07-14T12:00:00Z
```
