# EPIC-W7-031 — Phase 1: Scope Definition

## Single Method in Scope

| Property        | Value                                      |
|-----------------|--------------------------------------------|
| **Method**      | `AuditMaster_HandleNakedPosition`          |
| **File**        | `src/V12_002.REAPER.Audit.cs`              |
| **Lines**       | 624–679                                    |
| **Current CYC** | 19                                         |
| **Target CYC**  | ≤ 8                                        |

This epic targets a **single method**: `AuditMaster_HandleNakedPosition`.  
The scope boundary is drawn tightly around this one method definition and its three
planned helper extractions. No adjacent methods, callers, or sibling files are modified
as part of this epic.

---

## Caller Analysis

Grep of `src/` for `AuditMaster_HandleNakedPosition` returned **2 matches** in
**1 file** (`src/V12_002.REAPER.Audit.cs`):

| Match type  | Location                                 | Details                                          |
|-------------|------------------------------------------|--------------------------------------------------|
| Definition  | `src/V12_002.REAPER.Audit.cs` line 624  | `private void AuditMaster_HandleNakedPosition(…)` |
| Call site   | `src/V12_002.REAPER.Audit.cs` line 701  | Called by `AuditMasterAccountIfNeeded`           |

**Callers count: 1** — `AuditMasterAccountIfNeeded` (same file, line 701).

The method is a leaf node in the call graph. Its single caller,
`AuditMasterAccountIfNeeded`, passes through `masterPos`, `masterActualQty`, and
`masterExpectedKey` and does not need to change as a result of this refactor.

---

## Scope Boundary

The **scope boundary** for this epic is defined as follows:

- **In scope:** the body of `AuditMaster_HandleNakedPosition` (lines 624–679) and up to
  three private helper methods that will be extracted from it within the same file
  (`src/V12_002.REAPER.Audit.cs`).
- **Out of scope:** all callers, all sibling audit methods, all shared-state files listed
  in the blast-radius table, and the fleet-account parallel path
  `AuditFleet_HandleNakedPosition`.

Nothing outside this scope boundary is touched, renamed, moved, or re-signed during
Phases 1–3 of this epic.

---

## Why Other Methods Are NOT in Scope (V12.23 Policy)

Per the **V12.23 single-method refactor policy**, only one high-CYC method is targeted
per epic wave task to:

1. **Minimise blast radius** — the method's shared-state footprint already spans 9 files
   (see `00-hotspots.md`). Widening the scope to include sibling methods such as
   `AuditFleet_HandleNakedPosition`, `AuditMaster_HandleDesyncFlatten`, or
   `AuditFleet_HandleCriticalDesyncFlatten` would create overlapping diffs that exceed
   the wave's risk tolerance.

2. **Preserve reviewer bandwidth** — a single-method scope produces a diff that a
   reviewer can fully reason about in one pass. Multi-method scope breaks this guarantee.

3. **Enable incremental CYC tracking** — targeting a single method lets the post-refactor
   CYC measurement be unambiguous. If two methods were refactored together, regression
   attribution would be unclear.

4. **Avoid unintended coupling** — `AuditFleet_CheckWorkingStop` (line 517) already
   exists as the fleet-side analogue of the inline LINQ being extracted here. Touching
   that method in the same wave would risk de-synchronising the fleet/master parallel
   logic.

The V12.23 policy therefore mandates the strict single method scope confirmed in this
document.

---

## Planned Extractions (Preview — detail in Phase 2)

| # | New Method                          | CYC Removed | Driver Addressed                            |
|---|-------------------------------------|-------------|---------------------------------------------|
| 1 | `AuditMaster_CheckWorkingStop()`    | ~6          | Inline stop-detection LINQ (5 predicates)   |
| 2 | `AuditMaster_RecordNakedFirstSeen()`| ~2          | Grace-window TryGetValue + dict-write branch|
| 3 | `AuditMaster_TriggerNakedStopEvent()`| ~3         | TriggerCustomEvent + catch + in-flight clear|

Expected residual CYC after all three extractions: **≤ 8** (target ≤ 8 ✓).

---

## Agent Tracking

| Key               | Value                         |
|-------------------|-------------------------------|
| **Agent Name**    | v12-phase1-scope              |
| **Epic**          | EPIC-W7-031                   |
| **Wave**          | 7                             |
| **Phase**         | 1 — Scope Definition          |
| **Method**        | `AuditMaster_HandleNakedPosition` |
| **CYC Current**   | 19                            |
| **CYC Target**    | ≤ 8                           |
| **Callers Count** | 1                             |
| **Source File**   | `src/V12_002.REAPER.Audit.cs` |
| **Source Tool**   | Bob (native file + grep tools)|
