# EPIC-W7-065 — Phase 1: Scope Definition

| Field               | Value                                          |
|---------------------|------------------------------------------------|
| **Epic**            | EPIC-W7-065                                    |
| **Wave / Phase**    | 7 / 1                                          |
| **Method in Scope** | `HandleFsmFilled`                              |
| **Source File**     | `src/V12_002.Symmetry.BracketFSM.cs`           |
| **CYC (current)**   | **14**                                         |
| **CYC (target)**    | **≤ 8**                                        |
| **Callers Count**   | **1**                                          |
| **Date**            | 2025-07-11                                     |

---

## 1. Single Method in Scope

This phase operates on a **single method**: `HandleFsmFilled(AccountEvent evt, FollowerBracketFSM fsm)`
located in [`src/V12_002.Symmetry.BracketFSM.cs`](../../../src/V12_002.Symmetry.BracketFSM.cs) at line 349.

The scope boundary for EPIC-W7-065 Phase 1 is drawn precisely at the body of this one method.
No other methods, classes, or files fall inside the scope boundary for this phase.

```
V12_002 (partial class)
  └── BracketFSM Logic
        └── HandleFsmFilled(AccountEvent evt, FollowerBracketFSM fsm)   ◄── IN SCOPE
              File:  src/V12_002.Symmetry.BracketFSM.cs
              Lines: 349–375
              CYC:   14  →  target ≤ 8
```

---

## 2. Callers Analysis

A full `grep` of `src/` for the literal token `HandleFsmFilled` returns **exactly 2 matches**:

| Line  | Role        | Symbol                 |
|-------|-------------|------------------------|
| L349  | Definition  | `HandleFsmFilled` declaration |
| L397  | Call site   | `ProcessBracketEvent` (same file) |

**Callers count: 1** — the sole caller is `ProcessBracketEvent` at
[`src/V12_002.Symmetry.BracketFSM.cs:397`](../../../src/V12_002.Symmetry.BracketFSM.cs).
`HandleFsmFilled` is `private`; no external callers exist outside this file.

---

## 3. Scope Boundary

The **scope boundary** is defined as follows:

- **Inside scope boundary:** The body of `HandleFsmFilled` (lines 349–375) and any
  private helper predicates extracted from it during refactoring (e.g. `IsStopSignal`,
  `IsTargetSignal`) that live in the same partial-class file.
- **Outside scope boundary:** All other methods in `V12_002.Symmetry.BracketFSM.cs`,
  all downstream consumers (`REAPER.Audit.cs`, `SIMA.Shadow.cs`,
  `Orders.Management.Cleanup.cs`), and all test files.

The scope boundary is deliberately narrow to enable a safe, verifiable reduction in
cyclomatic complexity without disturbing the surrounding call chain.

---

## 4. Current Complexity — CYC 14

McCabe counting gives **CYC = 14**: 1 base path + 13 independent decision points.

| # | Decision Point                                                 | Lines   |
|---|----------------------------------------------------------------|---------|
| 1 | `!IsNullOrEmpty(evt.SignalName)` — isStop null guard          | 352–353 |
| 2 | `StartsWith("Stop_")` — stop prefix                           | 354     |
| 3 | `StartsWith("S_")` — short-circuit OR                         | 354     |
| 4 | `!IsNullOrEmpty(evt.SignalName)` — isTarget null guard        | 355–356 |
| 5 | `StartsWith("T1_")`                                           | 358     |
| 6 | `StartsWith("T2_")`                                           | 359     |
| 7 | `StartsWith("T3_")`                                           | 360     |
| 8 | `StartsWith("T4_")`                                           | 361     |
| 9 | `StartsWith("T5_")`                                           | 362     |
|10 | `if (isStop \|\| isTarget)` — outer branch                   | 365     |
|11 | `isStop \|\|` — short-circuit                                 | 365     |
|12 | `RemainingContracts <= 0 ? Filled : Active` — ternary        | 368     |
|13 | `else if (fsm.State == Accepted \|\| ...)` — if              | 370     |
|14 | `Accepted \|\| Submitted` — short-circuit                     | 370     |

**9 of 14 decisions** are inline prefix-dispatch branches (decisions 1–9) that can be
collapsed into two extracted boolean helper methods, reaching the ≤ 8 target.

---

## 5. Why Other Methods Are NOT in Scope (V12.23 Rule)

Per project rule **V12.23** (single-hotspot containment): a complexity-reduction phase
targets **one hotspot at a time**. Broadening scope to additional methods in the same
file or across the call chain introduces compounding risk:

- `ProcessBracketEvent` (CYC 6) is below the complexity threshold and is stable.
- `TransitionToAccepted`, `HandleFsmCancelled`, and other FSM handlers are not flagged
  hotspots in Wave 7 and must not be touched during this phase.
- Downstream consumers (`GetFsmExpectedPosition`, REAPER Audit, SIMA Shadow) depend on
  `fsm.State` and `fsm.RemainingContracts` written by `HandleFsmFilled`. Any change to
  their logic is a separate blast-radius concern requiring its own phase and test harness.
- Extracting helpers (`IsStopSignal`, `IsTargetSignal`) is permitted within the scope
  boundary because they are called exclusively from `HandleFsmFilled` and carry no
  independent callers or state side-effects.

V12.23 forbids co-scoping unrelated methods to prevent scope creep from masking the
measured complexity improvement of the targeted **single method**.

---

## 6. Target State

| Metric              | Before  | After (target) |
|---------------------|---------|----------------|
| CYC                 | 14      | ≤ 8            |
| Inline `StartsWith` branches | 9 | 0 (extracted) |
| Extracted helpers   | 0       | 2 (`IsStopSignal`, `IsTargetSignal`) |
| Responsibilities    | 3 (inline) | 3 (separated) |
| Callers             | 1       | 1 (unchanged)  |

---

## 7. Agent Tracking

```
Agent Name:   v12-phase1-scope
Epic:         EPIC-W7-065
Wave:         7
Phase:        1
Task:         Scope Definition
Status:       completed
Output:       docs/brain/EPIC-W7-065/00-scope.md
```
