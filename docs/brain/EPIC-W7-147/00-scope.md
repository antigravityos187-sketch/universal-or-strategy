# Phase 1: Scope Definition - EPIC-W7-147

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.0
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-23T21:57:52Z

---

## Method Under Refactoring

| Attribute            | Value                                      |
|---------------------|--------------------------------------------|
| **Method**          | `ProcessQueuedExecution_HandleFleetOCO`    |
| **File**            | `src/V12_002.UI.Compliance.cs`             |
| **Line**            | 698                                        |
| **Current CYC**     | 15                                         |
| **Target CYC**      | ≤ 8 (Jane Street / V12 DNA mandate)        |
| **Lines of Code**   | 30                                         |
| **Parameters**      | 1                                          |
| **Max Nesting**     | 4                                          |

The method orchestrates Fleet OCO (One-Cancels-Other) order lifecycle in three conceptually distinct phases:

1. **Fleet account guard** — validate the account is a fleet account before doing any work.
2. **Stop fill branch** — when a stop leg has been filled, delegate to `HandleFleetStopFill` and coordinate orphaned-target cancellation.
3. **Target fill branch** — when a target leg has been filled, delegate to `HandleFleetTargetFill` and finalize position state.

CYC 15 arises from the nested conditional logic that interleaves all three phases in a single method body.

---

## IN SCOPE — Extractions to Reduce CYC to ≤ 8

The following four helper methods will be created **inside the same class** in
`src/V12_002.UI.Compliance.cs`. Each helper addresses one orthogonal decision
cluster identified in the hotspot analysis.

### Helper 1 — `ValidateFleetAccountGuard`

| Attribute      | Value |
|---------------|-------|
| Proposed name  | `ValidateFleetAccountGuard` |
| Return type    | `bool` |
| Parameters     | same single parameter as the parent method |
| Responsibility | Encapsulates the `IsFleetAccount` check and any associated early-return / logging that currently contribute to the top-level CYC. |
| CYC reduction  | Removes ≥ 2 decision points from parent. |

### Helper 2 — `HandleFleetOCO_StopFillPath`

| Attribute      | Value |
|---------------|-------|
| Proposed name  | `HandleFleetOCO_StopFillPath` |
| Return type    | `void` |
| Parameters     | same single parameter as the parent method (passed through) |
| Responsibility | Contains the full stop-fill conditional sub-tree: calls `HandleFleetStopFill`, evaluates orphan cancellation conditions, and invokes `CancelOrphanedTargets` / `ExtractEntryKeyFromStopName` / `FinalizeStopFilledPosition` as needed. |
| CYC reduction  | Removes ≥ 5 decision points from parent. |

### Helper 3 — `HandleFleetOCO_TargetFillPath`

| Attribute      | Value |
|---------------|-------|
| Proposed name  | `HandleFleetOCO_TargetFillPath` |
| Return type    | `void` |
| Parameters     | same single parameter as the parent method (passed through) |
| Responsibility | Contains the full target-fill conditional sub-tree: calls `HandleFleetTargetFill` and `ApplyTargetFill`, evaluates position finalization conditions, and delegates to `CancelOrderOnAccount` as needed. |
| CYC reduction  | Removes ≥ 4 decision points from parent. |

### Helper 4 — `LogFleetOCO_DispatchTrace`

| Attribute      | Value |
|---------------|-------|
| Proposed name  | `LogFleetOCO_DispatchTrace` |
| Return type    | `void` |
| Parameters     | string message fragment + any context scalars already available at call sites |
| Responsibility | Consolidates the scattered `LogBuffer.Format` calls that currently appear at multiple branch points, eliminating repeated formatting decision points from the parent CYC count. |
| CYC reduction  | Removes ≥ 2 decision points from parent. |

### Projected CYC After Extraction

| Method                                | Projected CYC |
|--------------------------------------|---------------|
| `ProcessQueuedExecution_HandleFleetOCO` (orchestrator) | ≤ 5 |
| `ValidateFleetAccountGuard`           | ≤ 3 |
| `HandleFleetOCO_StopFillPath`         | ≤ 7 |
| `HandleFleetOCO_TargetFillPath`       | ≤ 6 |
| `LogFleetOCO_DispatchTrace`           | ≤ 2 |

All five methods individually satisfy CYC ≤ 8.

---

## OUT OF SCOPE

The following are **explicitly excluded** from this refactoring:

1. **Public / internal signature of `ProcessQueuedExecution_HandleFleetOCO`** — the method signature (name, parameter type, return type, access modifier) must remain byte-for-byte identical.  Callers (`ProcessQueuedExecution`, `ProcessAccountExecutionQueue`, `OnAccountExecutionUpdate`) must require zero changes.

2. **Behavior change** — no observable behavior change is permitted.  Execution paths, side effects, logging output, and order/position state mutations must be identical before and after the refactoring.

3. **Other methods in `V12_002.UI.Compliance.cs`** — `HandleFleetStopFill`, `HandleFleetTargetFill`, `CancelOrphanedTargets`, `ExtractEntryKeyFromStopName`, `FinalizeStopFilledPosition`, `ProcessQueuedExecution`, `ProcessAccountExecutionQueue`, `OnAccountExecutionUpdate`, and all other methods in the file remain untouched.

4. **Cross-file changes** — no modifications to `V12_002.cs`, `V12_002.Perf.LogBuffer.cs`, `V12_002.Orders.Callbacks.cs`, `V12_002.Orders.CancelGateway.cs`, `V12_002.Symmetry.Replace.cs`, `V12_002.PositionInfo.cs`, or `V12_002.Orders.Management.Flatten.cs`.

5. **Lock-free pattern** — no `lock()` blocks are to be introduced.  The existing lock-free concurrency model is preserved as-is.

6. **Test scaffolding** — unit test creation is deferred to a later wave phase; it is not part of this scope.

7. **Performance tuning** — no hot-path micro-optimisations, inlining hints, or allocation changes beyond what the structural extraction naturally produces.

---

## Extraction Plan

```
ProcessQueuedExecution_HandleFleetOCO  (CYC 15 → ≤5)
│
├─ call ValidateFleetAccountGuard(...)     [new, CYC ≤3]
│    └─ IsFleetAccount(...)
│
├─ call HandleFleetOCO_StopFillPath(...)   [new, CYC ≤7]
│    ├─ HandleFleetStopFill(...)
│    ├─ CancelOrphanedTargets(...)
│    ├─ ExtractEntryKeyFromStopName(...)
│    └─ FinalizeStopFilledPosition(...)
│
├─ call HandleFleetOCO_TargetFillPath(...)  [new, CYC ≤6]
│    ├─ HandleFleetTargetFill(...)
│    ├─ ApplyTargetFill(...)
│    └─ CancelOrderOnAccount(...)
│
└─ call LogFleetOCO_DispatchTrace(...)      [new, CYC ≤2]
     └─ LogBuffer.Format(...)
```

**Implementation sequence**:

1. Extract `LogFleetOCO_DispatchTrace` first (leaf, no logic dependencies).
2. Extract `HandleFleetOCO_TargetFillPath` (self-contained conditional block).
3. Extract `HandleFleetOCO_StopFillPath` (self-contained conditional block).
4. Extract `ValidateFleetAccountGuard` (top-of-method guard clause).
5. Verify each step compiles before proceeding to the next.

---

## Risk Assessment

| Risk                                           | Likelihood | Severity | Mitigation |
|------------------------------------------------|-----------|----------|------------|
| Accidental behavior change in stop-fill path  | Low        | High     | Extract verbatim; diff logical paths post-extraction |
| Hidden state captured by inline lambdas       | Very Low   | Medium   | Inspect source for closures before extraction |
| Logging output format altered                 | Low        | Medium   | Centralise in `LogFleetOCO_DispatchTrace` without modifying format strings |
| New `lock()` introduced inadvertently         | Very Low   | High     | Code review gate: grep for `lock(` in diff |
| Caller breakage (signature drift)             | Very Low   | High     | Callers are in-file; compile check is sufficient |
| Helper CYC still exceeds 8                    | Low        | Medium   | Measure CYC of each helper post-extraction; re-split if needed |

**Overall refactoring risk**: LOW — zero external dependents, isolated file scope, pure structural extraction.

---

## Success Criteria

| # | Criterion                                                                                  | Verification |
|---|-------------------------------------------------------------------------------------------|-------------|
| 1 | `ProcessQueuedExecution_HandleFleetOCO` CYC ≤ 8 after extraction                         | Static analysis tool / manual count |
| 2 | All four new helper methods individually have CYC ≤ 8                                     | Static analysis tool / manual count |
| 3 | Public signature of `ProcessQueuedExecution_HandleFleetOCO` is byte-for-byte unchanged    | `git diff` inspection |
| 4 | No `lock()` blocks appear anywhere in the diff                                             | `grep lock\(` on diff |
| 5 | No files outside `src/V12_002.UI.Compliance.cs` are modified                              | `git diff --name-only` |
| 6 | Build passes (NinjaTrader compile, no warnings promoted to errors)                        | Phase 2 gate |
| 7 | No behavior change detectable by callers (`ProcessQueuedExecution`, `ProcessAccountExecutionQueue`, `OnAccountExecutionUpdate`) | Phase 2 review |
