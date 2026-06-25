# Phase 1: Scope Definition - EPIC-W7-134

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Execution Time**: 2026-06-23T03:00:00Z

---

## Method Under Refactoring

| Attribute            | Value                                   |
|----------------------|-----------------------------------------|
| **Method**           | `MoveSpecificTarget`                    |
| **File**             | `src/V12_002.Trailing.Breakeven.cs`     |
| **Line**             | 335                                     |
| **Current CYC**      | 15                                      |
| **Target CYC**       | ≤ 8 (Jane Street threshold)             |
| **Lines of Code**    | 76                                      |
| **Max Nesting Depth**| 4                                       |
| **Signature**        | `private void MoveSpecificTarget(int targetNum, double profitPoints)` |

The method is a five-step orchestrator:
1. Validate the request via `ValidateMoveTargetRequest`.
2. Iterate `activePositions`.
3. Locate the target order via `FindTargetOrderForPosition`.
4. Calculate & validate the new price via `CalculateAndValidateNewTargetPrice`.
5. Dispatch execution (follower FSM **or** master `ChangeOrder`) and report a summary.

Each branch inside the loop contributes decision points that collectively push CYC to 15.

---

## IN SCOPE — Extractions Required

The following helper methods will be created to reduce CYC to ≤ 8:

### Helper 1 — `ProcessSinglePosition`
**Purpose**: Encapsulate the per-position work performed inside the `foreach` body (steps 3–5): find order, calculate price, dispatch execution.  
**Moves**: The entire `foreach` body from the `targetOrder == null` guard through the `try/catch` dispatch block.  
**Signature (proposed)**:
```csharp
private bool ProcessSinglePosition(
    string entryName,
    PositionInfo pos,
    int targetNum,
    double profitPoints,
    out string failReason)
```
**Returns**: `true` if the position was successfully moved (increments caller's `movedCount`), `false` otherwise.  
**CYC reduction**: Removes ~5 decision points from `MoveSpecificTarget` (null-check on `targetOrder`, null-check on `notFoundReason`, negated price-validation guard, null-check on `rejectionReason`, follower/master dispatch branch).

### Helper 2 — `DispatchTargetMove`
**Purpose**: Isolate the follower-vs-master branch and its `try/catch` (step 5).  
**Moves**: The `if (pos.IsFollower && pos.ExecutingAccount != null)` block plus the surrounding `try/catch`.  
**Signature (proposed)**:
```csharp
private bool DispatchTargetMove(
    PositionInfo pos,
    string entryName,
    int targetNum,
    Order targetOrder,
    double newTargetPrice)
```
**Returns**: `true` on success, `false` (and internal `Print`) on exception.  
**CYC reduction**: Removes 2 decision points from `ProcessSinglePosition` (IsFollower branch + implicit exception path), keeping that helper's own CYC ≤ 4.

### Summary of Expected Post-Refactoring CYC

| Method                              | Before | After |
|-------------------------------------|--------|-------|
| `MoveSpecificTarget`                | 15     | ≤ 4   |
| `ProcessSinglePosition` (new)       | —      | ≤ 5   |
| `DispatchTargetMove` (new)          | —      | ≤ 3   |
| All pre-existing helpers (unchanged)| n/a    | n/a   |

All three methods individually satisfy CYC ≤ 8.

---

## OUT OF SCOPE

| Item                                                         | Reason                                              |
|--------------------------------------------------------------|-----------------------------------------------------|
| Signature of `MoveSpecificTarget`                            | Must not change — callers (even if currently zero) rely on it |
| Observable behaviour of `MoveSpecificTarget`                 | Pure structural refactor; no logic changes          |
| `ValidateMoveTargetRequest` (line 166)                       | Already a separate helper; CYC within threshold     |
| `FindTargetOrderForPosition` (line 186)                      | Already a separate helper; CYC within threshold     |
| `CalculateAndValidateNewTargetPrice` (line 225)              | Already a separate helper; CYC within threshold     |
| `ExecuteFollowerTargetMove` (line 275)                       | Already a separate helper; CYC within threshold     |
| `ExecuteMasterTargetMove` (line 312)                         | Already a separate helper; CYC within threshold     |
| `StampReaperMoveGrace` (src/V12_002.SIMA.cs, line 199)      | Different file; outside blast radius                |
| Any other method in `src/V12_002.Trailing.Breakeven.cs`     | Untouched                                           |
| Build system, tests, config                                  | No build or test execution permitted in this phase  |

---

## Extraction Plan

```
MoveSpecificTarget (CYC 15)
│
├── [KEEP] ValidateMoveTargetRequest call  ──────────────── no change
│
├── [EXTRACT → ProcessSinglePosition]
│   ├── ContainsKey guard (continue)
│   ├── FindTargetOrderForPosition call + null guard
│   ├── CalculateAndValidateNewTargetPrice call + bool guard
│   └── [EXTRACT → DispatchTargetMove]
│       ├── IsFollower branch
│       │   └── ExecuteFollowerTargetMove
│       └── else branch
│           └── ExecuteMasterTargetMove
│       (wrapped in try/catch)
│
└── [KEEP] movedCount summary reporting   ──────────────── no change
```

**Execution order for Phase 2 (implementation)**:
1. Extract `DispatchTargetMove` first (innermost) — this simplifies `ProcessSinglePosition`'s extraction.
2. Extract `ProcessSinglePosition` second — calls into `DispatchTargetMove`.
3. Rewrite `MoveSpecificTarget` to delegate the `foreach` body to `ProcessSinglePosition`.

---

## Risk Assessment

| Risk                              | Severity | Likelihood | Mitigation                                                          |
|-----------------------------------|----------|------------|---------------------------------------------------------------------|
| Silent behaviour drift            | HIGH     | LOW        | No logic moves across the extraction boundary; only call structure changes |
| `out` parameter threading         | MEDIUM   | LOW        | `failReason` out-param on `ProcessSinglePosition` keeps diagnostics intact |
| `movedCount` increment correctness| MEDIUM   | LOW        | `ProcessSinglePosition` returns `bool`; caller increments iff `true`      |
| `try/catch` scope narrowing       | LOW      | LOW        | `DispatchTargetMove` carries the same `try/catch`; exception message preserved |
| Zero external callers             | —        | —          | Blast radius = 0; any regression is self-contained to this file     |

**Overall Risk**: LOW-MEDIUM (unchanged from Phase 0 assessment).

---

## Success Criteria

1. `MoveSpecificTarget` CYC ≤ 8 (target: ≤ 4).
2. Every newly created helper method individually has CYC ≤ 8.
3. Signature of `MoveSpecificTarget` is byte-for-byte identical to current: `private void MoveSpecificTarget(int targetNum, double profitPoints)`.
4. All pre-existing helper methods (`ValidateMoveTargetRequest`, `FindTargetOrderForPosition`, `CalculateAndValidateNewTargetPrice`, `ExecuteFollowerTargetMove`, `ExecuteMasterTargetMove`) are unchanged.
5. No `src/` file other than `src/V12_002.Trailing.Breakeven.cs` is modified.
6. All `Print(...)` diagnostic messages that were present in the original method continue to be emitted under identical conditions.
7. `movedCount` is incremented if and only if the same conditions that triggered it before the refactor are satisfied.
