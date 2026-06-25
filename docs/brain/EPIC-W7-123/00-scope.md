# Phase 1: Scope Definition — EPIC-W7-123

## Agent Tracking
- Agent Name: v12-phase1-scope
- Epic: EPIC-W7-123
- Execution Time: 2026-06-24T04:00:00Z
- Input: 00-hotspots.md

---

## Method Under Refactoring

| Attribute        | Value                              |
|------------------|------------------------------------|
| Method           | `SymmetryGuardOnMasterFill`        |
| File             | `src/V12_002.Symmetry.cs`          |
| Line             | 258                                |
| Current CYC      | 14                                 |
| Target CYC       | ≤ 8                                |
| CYC Overage      | +6                                 |
| LOC              | 67                                 |
| Max Nesting      | 4                                  |
| Parameter Count  | 5                                  |

---

## IN SCOPE — Extractions

Four private helper methods will be carved out of `SymmetryGuardOnMasterFill`. Each extraction removes a self-contained logical block and its associated branching paths, reducing CYC on the parent method.

### 1. `ResolveDispatchForMasterFill` — CYC reduction: −3

- **Extracted logic:** The block that calls `SymmetryFindDispatchForMasterFill` and applies conditional branching on the returned dispatch object (null-checks, state guards, and dispatch-type routing).
- **Rationale:** This block is the largest single source of CYC overage. It has a clear input/output boundary and no side-effects beyond returning a resolved dispatch value or a guard exit.
- **Proposed signature:** `private DispatchResult? ResolveDispatchForMasterFill(<relevant params>)`

### 2. `InferAndNormalizeTradeType` — CYC reduction: −2

- **Extracted logic:** The block that calls `SymmetryInferTradeType` and then normalises/validates the inferred trade type through conditional branches (e.g. unknown-type fallback, cross-checks against fill direction).
- **Rationale:** Trade-type inference is conceptually independent of dispatch resolution and follower resolution. Isolating it also makes the normalisation path independently testable.
- **Proposed signature:** `private TradeType InferAndNormalizeTradeType(<relevant params>)`

### 3. `ResolveFollowersForDispatch` — CYC reduction: −3

- **Extracted logic:** The block that calls `SymmetryGuardTryResolveFollowersForDispatch` and applies conditional branching on the follower list (empty-list guard, per-follower iteration conditions).
- **Rationale:** Follower resolution is the second-largest CYC contributor. Its loop-plus-conditions pattern is cleanly separable and maps to a single responsibility.
- **Proposed signature:** `private IReadOnlyList<Follower> ResolveFollowersForDispatch(<relevant params>)`

### 4. `LogMasterFillEvent` — CYC reduction: −2

- **Extracted logic:** All `LogBuffer.Format` calls and surrounding guard conditions (log-level checks, conditional message assembly).
- **Rationale:** Logging logic inflates CYC with guard branches that have no business-logic meaning. Extracting them removes noise from the main flow without altering observable behaviour.
- **Proposed signature:** `private void LogMasterFillEvent(<relevant params>)`

### Post-Extraction CYC Estimate

| Contributor                     | CYC removed |
|---------------------------------|-------------|
| `ResolveDispatchForMasterFill`  | −3          |
| `InferAndNormalizeTradeType`    | −2          |
| `ResolveFollowersForDispatch`   | −3          |
| `LogMasterFillEvent`            | −2          |
| **Remaining in parent**         | **4**       |

Estimated parent CYC after all extractions: **4** (well below the ≤ 8 threshold).

---

## OUT OF SCOPE

The following are explicitly excluded from this refactoring to prevent scope creep and ensure zero behaviour change.

1. **Public signature of `SymmetryGuardOnMasterFill`** — The method name, return type, parameter names, parameter types, and parameter order must remain byte-for-byte identical. No new overloads.
2. **Observable behaviour** — Every code path through `SymmetryGuardOnMasterFill` must produce the same side-effects, return values, and log output as before. This is a pure structural refactoring.
3. **Other methods in `src/V12_002.Symmetry.cs`** — `SymmetryFindDispatchForMasterFill`, `SymmetryInferTradeType`, `SymmetryGuardTryResolveFollowersForDispatch`, and all other methods in the file are untouched.
4. **Callee internals** — No changes to any method called by `SymmetryGuardOnMasterFill`.
5. **Test files** — Writing tests is recommended but is a separate work item; no test files are modified as part of this epic's extraction commits.
6. **Performance optimisations** — No algorithmic changes; only structural reshuffling.
7. **Access modifier changes** — Extracted helpers are `private`; no visibility widening.

---

## Extraction Plan

Execute extractions in the following order to minimise intermediate diff noise. Each step must leave the codebase in a buildable, behaviourally-identical state before the next step begins.

```
Step 1 — Extract LogMasterFillEvent
         (lowest CYC impact, easiest isolation; de-clutters the method before
          extracting the more complex blocks)

Step 2 — Extract InferAndNormalizeTradeType
         (self-contained, no dependency on dispatch or follower results)

Step 3 — Extract ResolveDispatchForMasterFill
         (depends on step 2 being clean so dispatch-type routing is readable)

Step 4 — Extract ResolveFollowersForDispatch
         (depends on step 3 so dispatch result is already an extracted variable)
```

Each extracted method is placed immediately below `SymmetryGuardOnMasterFill` in the source file, in the same order as the extraction steps, to preserve locality.

---

## Risk Assessment

| Risk                                      | Likelihood | Impact | Mitigation                                                                 |
|-------------------------------------------|-----------|--------|---------------------------------------------------------------------------|
| Silent behaviour change in extracted block | Low       | High   | Extract with zero logic change; diff review against original per step      |
| Variable-capture mistake (wrong scope)    | Medium    | High   | Compile-time check after each step; all extracted helpers must compile clean |
| Reflection / dynamic dispatch caller missed | Low     | Medium | Phase 0 confirmed 0 callers; no mitigation required beyond awareness       |
| Logging format drift                      | Low       | Low    | `LogMasterFillEvent` must pass all `LogBuffer.Format` calls through unmodified |
| Merge conflict with concurrent edits      | Low       | Low    | File has LOW churn; coordinate via branch isolation                        |

**Overall Residual Risk: LOW**

---

## Success Criteria

1. `SymmetryGuardOnMasterFill` CYC ≤ 8 (target: 4) as measured by the project's complexity tooling.
2. Public signature of `SymmetryGuardOnMasterFill` is unchanged (verified by diff on method declaration line).
3. All four helper methods are `private` and reside in `src/V12_002.Symmetry.cs`.
4. No other methods in `src/V12_002.Symmetry.cs` are modified.
5. Project builds clean with zero new warnings after each extraction step.
6. No changes to any file outside `src/V12_002.Symmetry.cs`.
