# Phase 1: Scope Definition - EPIC-W7-140

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Execution Time**: 2026-06-24T00:00:00Z
- **Input**: 00-hotspots.md
- **Output**: 00-scope.md

---

## Method Under Refactoring

| Attribute           | Value                                    |
|---------------------|------------------------------------------|
| **Method**          | `InitiateStopReplacement`                |
| **File**            | `src/V12_002.Trailing.StopUpdate.cs`     |
| **Line**            | 307                                      |
| **Visibility**      | `private`                                |
| **Cyclomatic Complexity (current)** | 13 (threshold: ≤8)    |
| **Max Nesting Depth** | 5                                      |
| **Lines of Code**   | 63                                       |
| **Parameter Count** | 5                                        |

### Signature (unchanged)
```csharp
private void InitiateStopReplacement(
    string entryName,
    PositionInfo pos,
    Order currentStop,
    double validatedStopPrice,
    int newTrailLevel
)
```

---

## Complexity Responsibility Map

The method currently owns five distinct responsibilities, each contributing to its CYC=13 score:

| # | Responsibility               | Key Callees                                      | CYC Contribution |
|---|------------------------------|--------------------------------------------------|-----------------|
| 1 | Pending replacement lookup   | `GetTargetOrdersDictionary`, `pendingStopReplacements` | ~3          |
| 2 | Order cancellation coordination | `CancelOrderForReplace`, `CancelOrderSafe`, `IsOrderTerminal` | ~3   |
| 3 | Sticky state persistence     | `MarkStickyDirty`                                | ~2              |
| 4 | REAPER grace period stamping | `StampReaperMoveGrace`                           | ~2              |
| 5 | Orchestration + logging      | `LogBuffer.Format`                               | ~3              |

---

## IN SCOPE — Extractions

The following **three** private helper methods will be extracted from `InitiateStopReplacement`. All helpers remain in the same class and file.

### Helper 1 — `TryGetPendingReplacement`

| Attribute     | Value |
|---------------|-------|
| **Purpose**   | Encapsulate the lookup into `GetTargetOrdersDictionary` / `pendingStopReplacements` and return whether a pending replacement already exists for `entryName`. |
| **Signature** | `private bool TryGetPendingReplacement(string entryName, out /* relevant type */ existing)` |
| **Target CYC** | ≤3 |
| **Replaces logic at approx. line range** | Top of `InitiateStopReplacement` before cancellation block |

### Helper 2 — `CoordinateCancellation`

| Attribute     | Value |
|---------------|-------|
| **Purpose**   | Encapsulate the dual-path cancellation decision: call `CancelOrderForReplace` when the order is live, fall back to `CancelOrderSafe` after checking `IsOrderTerminal`. |
| **Signature** | `private void CoordinateCancellation(Order currentStop, string entryName)` |
| **Target CYC** | ≤3 |
| **Replaces logic at approx. line range** | Cancellation conditional block within `InitiateStopReplacement` |

### Helper 3 — `PersistReplacementState`

| Attribute     | Value |
|---------------|-------|
| **Purpose**   | Encapsulate sticky-state persistence (`MarkStickyDirty`) and REAPER grace-period stamping (`StampReaperMoveGrace`) into one atomic state-commit call. |
| **Signature** | `private void PersistReplacementState(string entryName, double validatedStopPrice, int newTrailLevel)` |
| **Target CYC** | ≤2 |
| **Replaces logic at approx. line range** | State-write block at the tail of `InitiateStopReplacement` |

### Post-extraction orchestrator (`InitiateStopReplacement` residual)

After extraction the orchestrator will contain:
- One call to `TryGetPendingReplacement` (early-exit guard)
- One call to `CoordinateCancellation`
- One call to `PersistReplacementState`
- Logging via `LogBuffer.Format`
- **Target CYC**: ≤5

**Combined CYC ceiling across all four methods**: ≤13 (no new paths introduced), worst-case per method ≤5.

---

## OUT OF SCOPE

| Item | Reason |
|------|--------|
| Signature of `InitiateStopReplacement` | Must remain identical; sole caller `UpdateStopOrder` (line 84) must not require changes. |
| Behavior / observable side-effects | Zero behavior change — pure structural extraction. No logic is added, removed, or reordered. |
| `UpdateStopOrder` and all other methods in the file | Untouched; blast radius is zero beyond the target method. |
| Other files in `src/` | No cross-file changes. |
| Public/internal API surface | No visibility changes to any existing member. |
| Error handling additions | No new exception handling or guard clauses beyond what already exists. |
| Unit test creation | Out of scope for Phase 1; test scaffolding is a separate concern. |
| Performance optimizations | Out of scope; structural refactoring only. |

---

## Extraction Plan (Ordered Steps)

```
Step 1 — Extract TryGetPendingReplacement
  a. Identify the exact lines responsible for the pending-replacement lookup.
  b. Create private helper with the out-parameter signature.
  c. Replace original lines with a single call site.
  d. Verify: compile; no behavior change; CYC of helper ≤3.

Step 2 — Extract CoordinateCancellation
  a. Identify the cancellation conditional block (IsOrderTerminal / CancelOrderForReplace / CancelOrderSafe).
  b. Create private helper accepting Order + entryName.
  c. Replace original block with single call site.
  d. Verify: compile; no behavior change; CYC of helper ≤3.

Step 3 — Extract PersistReplacementState
  a. Identify the MarkStickyDirty + StampReaperMoveGrace tail block.
  b. Create private helper forwarding validatedStopPrice + newTrailLevel.
  c. Replace original block with single call site.
  d. Verify: compile; no behavior change; CYC of helper ≤2.

Step 4 — Verify residual orchestrator
  a. Confirm InitiateStopReplacement body is now: guard → cancel → persist → log.
  b. Measure CYC of residual; must be ≤5.
  c. Confirm full-file compilation passes with no warnings.
```

---

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| Accidental parameter-capture change (closure semantics differ from method params) | LOW | HIGH | Each extracted method receives all required data as explicit parameters; no captured state. |
| Variable lifetime / scoping error when splitting across method boundaries | LOW | MEDIUM | Read exact line ranges before extracting; ensure all locals referenced across split points are passed as parameters. |
| `out` parameter availability in `TryGetPendingReplacement` changes null-safety guarantees | LOW | MEDIUM | Preserve existing null checks; do not add new guards. |
| Residual orchestrator CYC exceeds 8 after extractions | VERY LOW | LOW | Three extractions mathematically reduce main-method branching to ≤5. |
| Unintended method visibility escalation | VERY LOW | LOW | All helpers are declared `private`; no API surface change. |

**Overall Phase Risk**: LOW — single private method, zero external callers, contained in one file.

---

## Success Criteria

| Criterion | Measurement |
|-----------|-------------|
| `InitiateStopReplacement` CYC ≤ 8 | Static analysis post-refactor |
| Each helper method CYC ≤ 5 | Static analysis post-refactor |
| No method in the file exceeds CYC 8 (newly introduced) | File-wide static analysis |
| Signature of `InitiateStopReplacement` unchanged | Diff check: method signature identical |
| `UpdateStopOrder` call site unchanged | Diff check: no edits to line 84 |
| Zero new public/internal members added | Diff check: all new methods are `private` |
| File compiles without errors or new warnings | Build pass |
| No other `src/` files modified | Diff check: single-file delta |
