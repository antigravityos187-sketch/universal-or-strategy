# Phase 1: Scope Definition - EPIC-W7-099

**Epic ID**: EPIC-W7-099
**Target Method**: `PurgePositionIfEligible`
**File**: `V12_002.Orders.Management.Cleanup.cs`
**Baseline Complexity**: 11
**Target Complexity**: ≤8
**Phase**: 1 — Scope Definition
**Author**: v12-phase1-scope
**Date**: 2026-06-22

---

## Method Under Refactoring

| Attribute | Value |
|---|---|
| Method | `PurgePositionIfEligible` |
| File | `V12_002.Orders.Management.Cleanup.cs` |
| Current CYC | 11 |
| Target CYC (main) | ≤5 |
| Overage | +3 (38% above Jane Street threshold of 8) |

The method coordinates position cleanup within the position management subsystem. It currently contains four interlocked concern clusters: eligibility validation, FSM state checking, cleanup decision evaluation, and error handling paths. All four clusters contribute independent decision branches that collectively inflate CYC to 11.

---

## IN SCOPE — Extractions to Bring CYC ≤ 8

Three helper methods will be extracted from `PurgePositionIfEligible`. Each extraction targets a cohesive cluster of branches that currently lives inline in the main method body.

### 1. `IsPositionEligibleForPurge()`

- **What it captures**: The eligibility-guard branches — null/sentinel checks on the position object, age/TTL threshold comparisons, and any flag-based early-exit conditions that determine whether the position is a candidate for purging at all.
- **Expected CYC contribution absorbed**: ~3 branches
- **Resulting helper CYC**: ≤3
- **Return type (proposed)**: `bool`

### 2. `ValidatePositionState()`

- **What it captures**: FSM state validation logic — assertions or conditional checks that confirm the position is in a permissible state for cleanup (e.g. not mid-fill, not locked, correct lifecycle node in the state machine).
- **Expected CYC contribution absorbed**: ~2 branches
- **Resulting helper CYC**: ≤3
- **Return type (proposed)**: `bool`

### 3. `ShouldPurgePosition()`

- **What it captures**: The cleanup decision logic — the composite guard that synthesises eligibility and state validity into a final go/no-go signal, plus any additional policy conditions (risk limits, cooldown windows, etc.).
- **Expected CYC contribution absorbed**: ~2 branches
- **Resulting helper CYC**: ≤3
- **Return type (proposed)**: `bool`

### Post-Extraction Complexity Budget

| Symbol | CYC (post-extraction) |
|---|---|
| `PurgePositionIfEligible` (main) | ≤5 |
| `IsPositionEligibleForPurge` | ≤3 |
| `ValidatePositionState` | ≤3 |
| `ShouldPurgePosition` | ≤3 |
| **All symbols** | **≤8 ✅** |

---

## OUT OF SCOPE

The following must remain completely unchanged throughout this refactoring:

1. **Public signature of `PurgePositionIfEligible`** — parameter list, return type, access modifier, and any attributes/annotations are frozen.
2. **Observable behaviour** — for every possible input state, the externally observable outcome (position purged or not purged, any side effects on shared state, any events raised) must be byte-for-byte identical before and after extraction.
3. **All other methods in `V12_002.Orders.Management.Cleanup.cs`** — no surrounding methods are touched, refactored, renamed, or reformatted.
4. **All other files** — no file outside `V12_002.Orders.Management.Cleanup.cs` is modified.
5. **FSM state machine internals** — the extraction does not alter state transitions; helpers only read/query FSM state.
6. **Performance characteristics** — no new heap allocations, no additional lock acquisitions, no observable latency change.
7. **Error handling contract** — exception propagation paths and any catch/finally semantics are preserved exactly.

---

## Extraction Plan

### Step 1 — Identify branch clusters in `PurgePositionIfEligible`

Read the method body and annotate each conditional node (if/else, switch arm, ternary, null-coalescing early return) with its assigned cluster:
- Cluster A → `IsPositionEligibleForPurge`
- Cluster B → `ValidatePositionState`
- Cluster C → `ShouldPurgePosition`
- Remaining orchestration → stays in main method

### Step 2 — Extract `IsPositionEligibleForPurge`

- Copy the Cluster A branch block into a new private method with the same local-variable scope.
- Replace the inline block in `PurgePositionIfEligible` with a single call site: `if (!IsPositionEligibleForPurge(...)) return;`
- Verify CYC of both symbols after extraction.

### Step 3 — Extract `ValidatePositionState`

- Copy the Cluster B branch block into a new private method.
- Replace the inline block with: `if (!ValidatePositionState(...)) return;`
- Verify CYC of both symbols after extraction.

### Step 4 — Extract `ShouldPurgePosition`

- Copy the Cluster C branch block into a new private method.
- Replace the inline block with a call to `ShouldPurgePosition(...)`, threading its bool result into whatever downstream action currently follows.
- Verify CYC of both symbols after extraction.

### Step 5 — Final CYC audit

Confirm all four symbols are ≤8 (target: main ≤5, each helper ≤3). If any symbol remains above threshold, re-partition cluster assignments before proceeding to Phase 2.

---

## Risk Assessment

| Risk | Likelihood | Severity | Mitigation |
|---|---|---|---|
| Incorrect branch boundary — shared local variable captured by two clusters | LOW | HIGH | Read method body carefully; pass shared state as explicit parameters to helper |
| Helper introduces unintended short-circuit (changes observable flow) | LOW | HIGH | Each helper must be pure Boolean predicate with no side effects; use identical guard conditions |
| FSM state read across extraction boundary yields different result due to concurrency | LOW | MEDIUM | Helper reads FSM state via same access path as original inline code; no new reads introduced |
| CYC budget miscounted — main method still >8 after three extractions | LOW | LOW | CYC is additive; removing 7 branch nodes from main drops it to ≤4–5; re-audit per step |
| Scope creep — developer touches adjacent methods during extraction PR | MEDIUM | MEDIUM | Phase boundary enforced: PR diff must be limited to additions of three helpers + edits inside `PurgePositionIfEligible` only |

**Overall Risk Level**: LOW — position cleanup is a stable, well-isolated subsystem (per Phase 0 blast-radius assessment).

---

## Success Criteria

1. ✅ `PurgePositionIfEligible` cyclomatic complexity ≤ 5 after extraction.
2. ✅ `IsPositionEligibleForPurge` cyclomatic complexity ≤ 8 (target ≤3).
3. ✅ `ValidatePositionState` cyclomatic complexity ≤ 8 (target ≤3).
4. ✅ `ShouldPurgePosition` cyclomatic complexity ≤ 8 (target ≤3).
5. ✅ Public signature of `PurgePositionIfEligible` is identical to baseline.
6. ✅ All existing unit and integration tests pass with zero modifications.
7. ✅ No files other than `V12_002.Orders.Management.Cleanup.cs` are modified.
8. ✅ No new heap allocations or lock acquisitions introduced by helpers.
9. ✅ Phase 1.5 boundary validation confirms scope-creep flag = `false`.

---

## References

- Phase 0 output: [`00-hotspots.md`](00-hotspots.md)
- Jane Street KB: CYC ≤8 mandate, cognitive-simplicity principle
- V12 DNA: single-responsibility extraction pattern
- FSM/Actor pattern: lock-free state management guidelines
