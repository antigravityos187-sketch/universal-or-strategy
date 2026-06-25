# Phase 1: Scope Definition - EPIC-W7-127

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.00
- **Execution Time**: 2026-06-23T03:16:36Z (phase 0 baseline)

---

## 1. Method Under Refactoring

| Property | Value |
|---|---|
| **Method** | `SymmetryGuardOnFollowerFill` |
| **File** | `src/V12_002.Symmetry.Follower.cs` |
| **Line** | 17 |
| **Signature** | `private bool SymmetryGuardOnFollowerFill(string fleetEntryName, PositionInfo followerPos, double followerFillPrice)` |
| **Current CYC** | 16 |
| **Target CYC** | ≤ 8 |
| **LOC** | 72 |
| **Max Nesting Depth** | 6 |

### Current Body Summary

The method performs four distinct logical responsibilities in sequence:

1. **Guard + initialisation** (lines 23–28): Null/type guard on `followerPos`; initialise `RemainingContracts` if not yet set.
2. **Pre-anchor bracket submission** (lines 30–73): If the bracket has not yet been submitted, perform a lock-free read of `AnchorSnapshot`. If the anchor is already resolved and valid, apply it immediately and submit the bracket; otherwise log a delay gate message.
3. **Pending-fill registration** (lines 75–85): Construct a `PendingFollowerFill` record, register it in `symmetryPendingFollowerFills`, then attempt an immediate resolution via `SymmetryGuardTryResolveFollower`.
4. **Return** (line 87): Return `true`.

The complexity budget is consumed by:
- The outer `if (!followerPos.BracketSubmitted)` block (1 branch)
- The nested `TryGetValue` double-check (2 branches)
- The `anchorReady && preCheckAnchor > 0` combined guard (2 branches — short-circuit)
- The `shouldSubmitImmediately` fork (1 branch for submit, 1 branch for log)
- The conditional `RemainingContracts` reset (1 branch)
- The null/IsFollower guard (2 branches — null + IsFollower)
- The `SymmetryGuardTryResolveFollower` conditional removal (1 branch)
- The ternary `followerFillPrice > 0` (1 branch)

Total: 16 decision points, nesting depth 6 (3 levels inside `!BracketSubmitted` → `TryGetValue` chain → `anchorReady` check).

---

## 2. IN SCOPE — Extractions

The following helper methods will be extracted to bring `SymmetryGuardOnFollowerFill` to CYC ≤ 8. Each extraction targets a coherent sub-responsibility; no logic is altered—only locality changes.

### Helper 1: `SymmetryGuardInitialiseFollowerContracts`

| Property | Value |
|---|---|
| **Extracted from lines** | 26–28 |
| **Responsibility** | Ensure `RemainingContracts` has a valid floor value after the entry fill is acknowledged. |
| **Signature** | `private void SymmetryGuardInitialiseFollowerContracts(PositionInfo followerPos)` |
| **CYC contribution removed** | 1 (the `if (followerPos.RemainingContracts <= 0)` branch) |
| **Rationale** | Encapsulates a single defensive initialisation step that is semantically independent of bracket submission or anchor resolution. |

### Helper 2: `SymmetryGuardTryPreApplyAnchorAndSubmit`

| Property | Value |
|---|---|
| **Extracted from lines** | 30–73 |
| **Responsibility** | If the bracket has not yet been submitted: attempt a pre-flight anchor check; if the anchor is already resolved, apply it and submit the bracket immediately; otherwise log the delay gate. Returns `bool shouldSubmitImmediately` for audit purposes (caller does not branch on it). |
| **Signature** | `private void SymmetryGuardTryPreApplyAnchorAndSubmit(string fleetEntryName, PositionInfo followerPos)` |
| **CYC contribution removed** | 7 (outer `!BracketSubmitted` + double `TryGetValue` pattern [2] + `anchorReady && preCheckAnchor > 0` [2] + `shouldSubmitImmediately` fork [2]) |
| **Rationale** | This is the largest coherent block in the method. It represents the ANCHOR-01 pre-check path introduced in V12.Phase7.1 and is described in the source comment as a single logical concern ("eliminate double round-trip in volatile bursts"). Extracting it also localises the ADR-019 lock-free snapshot read pattern in one place. |

### Helper 3: `SymmetryGuardRegisterAndResolveFollower`

| Property | Value |
|---|---|
| **Extracted from lines** | 75–85 |
| **Responsibility** | Construct the `PendingFollowerFill` record (including the ternary fill-price selection), register it in `symmetryPendingFollowerFills`, and attempt immediate resolution. Removes the resolved entry from the map on success. |
| **Signature** | `private void SymmetryGuardRegisterAndResolveFollower(string fleetEntryName, PositionInfo followerPos, double followerFillPrice)` |
| **CYC contribution removed** | 2 (`followerFillPrice > 0` ternary + `TryResolveFollower` conditional removal) |
| **Rationale** | Pending-fill registration and resolution-attempt are always performed together; the ternary fill-price normalisation is a detail that belongs with record construction, not at the top-level orchestration layer. |

### Resulting CYC for `SymmetryGuardOnFollowerFill` after extractions

```
CYC remaining = 16 − 1 (Helper 1) − 7 (Helper 2) − 2 (Helper 3) = 6  ✅ (≤ 8)
```

The refactored orchestrator body will contain only:
1. Null/`IsFollower` guard → `return false` (2 decisions)
2. Call `SymmetryGuardInitialiseFollowerContracts` (0 decisions)
3. Call `SymmetryGuardTryPreApplyAnchorAndSubmit` (0 decisions)
4. Call `SymmetryGuardRegisterAndResolveFollower` (0 decisions)
5. `return true` (0 decisions)

CYC = 1 (base) + 2 (guard branches) + 1 (null short-circuit) = **3**, well within target.

---

## 3. OUT OF SCOPE

The following are explicitly excluded from this epic:

| Item | Reason |
|---|---|
| **Signature of `SymmetryGuardOnFollowerFill`** | Unchanged. Three parameters (`fleetEntryName`, `followerPos`, `followerFillPrice`) and `bool` return type are preserved exactly. |
| **Observable behaviour** | No logic, guard semantics, print messages, or state mutations change. All branch conditions are extracted verbatim. |
| **`SymmetryGuardIsAnchorPending`** | Separate method; CYC = 1; not a target. |
| **`SymmetryGuardProcessPendingFollowerFills`** | Separate method; not part of this epic. |
| **`SymmetryGuardTryResolveFollower`** | Called by the target method but is a separate, already-extracted method; its CYC is not in scope. |
| **`SymmetryGuardApplyMasterAnchor`** | Already a standalone helper; not touched. |
| **`SymmetryGuardSubmitFollowerBracket`** | Already a standalone method; not touched. |
| **Any other `src/` file** | Blast radius is zero—no callers outside this file. No other file is modified. |
| **Build system / project files** | Not modified. |
| **Test additions** | Not in Phase 1 deliverables. |

---

## 4. Extraction Plan

### Step-by-step execution order

```
Step 1  Create SymmetryGuardInitialiseFollowerContracts
        • Extract lines 26-28 into new private void method
        • Replace inline block with single method call
        • Delta CYC on orchestrator: -1

Step 2  Create SymmetryGuardTryPreApplyAnchorAndSubmit
        • Extract lines 30-73 (entire !BracketSubmitted block body and outer if)
        • Replace with single void call
        • Delta CYC on orchestrator: -7

Step 3  Create SymmetryGuardRegisterAndResolveFollower
        • Extract lines 75-85 (PendingFollowerFill construction + register + TryResolve)
        • Replace with single void call
        • Delta CYC on orchestrator: -2

Step 4  Verify
        • Confirm orchestrator CYC ≤ 8 (expected: 3)
        • Confirm each helper compiles without new dependencies
        • Confirm no src/ file other than V12_002.Symmetry.Follower.cs touched
```

### Proposed method placement

All three helpers will be added immediately below `SymmetryGuardOnFollowerFill` within the `#region Symmetry Follower` block, maintaining the existing locality convention of the file.

### Dependency map for new helpers

```
SymmetryGuardOnFollowerFill (orchestrator)
├── SymmetryGuardInitialiseFollowerContracts   [reads/writes: followerPos]
├── SymmetryGuardTryPreApplyAnchorAndSubmit    [reads: symmetryFleetEntryToDispatch,
│                                               symmetryDispatchById, AnchorSnapshot;
│                                               calls: SymmetryGuardApplyMasterAnchor,
│                                                      SymmetryGuardSubmitFollowerBracket,
│                                                      Print]
└── SymmetryGuardRegisterAndResolveFollower    [reads/writes: symmetryPendingFollowerFills;
                                                calls: SymmetryGuardTryResolveFollower]
```

No helper introduces a circular dependency. All fields/methods referenced already exist in scope.

---

## 5. Risk Assessment

| Risk | Severity | Likelihood | Mitigation |
|---|---|---|---|
| Extracted helper silently changes evaluation order of side-effects | HIGH | LOW | Extractions are straight-line code sequences (no lazy evaluation, no closures capturing loop variables); order is preserved by sequential call order in orchestrator. |
| `AnchorSnapshot` read split across helper boundary loses atomicity guarantees | MEDIUM | LOW | The snapshot is read once inside `SymmetryGuardTryPreApplyAnchorAndSubmit`; the single `ctx.Anchor` assignment is not split. ADR-019 semantics are fully preserved within the helper. |
| Method visibility change (private → accessible) | LOW | NONE | All three helpers are `private`; no visibility change. |
| Regression in `symmetryPendingFollowerFills` map state | MEDIUM | LOW | `SymmetryGuardRegisterAndResolveFollower` encapsulates both the write and the conditional removal atomically in the same method, preserving the original two-step sequence without interleaving. |
| Blast radius from caller changes | NONE | NONE | Phase 0 confirmed zero external callers; partial class boundary is respected. |
| Build break from missing `using` / namespace references | LOW | LOW | All types used in helpers (`PendingFollowerFill`, `AnchorSnapshot`, `SymmetryDispatchContext`, `DateTime`) are already in scope in the partial class file. No new imports required. |

**Overall Refactoring Risk: LOW**

---

## 6. Success Criteria

| Criterion | Measurable Target |
|---|---|
| Cyclomatic Complexity of `SymmetryGuardOnFollowerFill` | ≤ 8 (expected: 3) |
| Cyclomatic Complexity of each new helper | ≤ 8 each |
| Method signature of `SymmetryGuardOnFollowerFill` | Identical to pre-refactor: `private bool SymmetryGuardOnFollowerFill(string, PositionInfo, double)` |
| Files modified | Exactly 1: `src/V12_002.Symmetry.Follower.cs` |
| New methods added | Exactly 3: `SymmetryGuardInitialiseFollowerContracts`, `SymmetryGuardTryPreApplyAnchorAndSubmit`, `SymmetryGuardRegisterAndResolveFollower` |
| All original print messages preserved | Word-for-word match on format strings `[ANCHOR-01]`, `[ANCHOR-GATE]` |
| All original state mutations preserved | `followerPos.EntryFilled`, `followerPos.RemainingContracts`, `symmetryPendingFollowerFills` write + conditional remove |
| No new external dependencies | Zero new `using` directives, zero new field declarations |
| Nesting depth of orchestrator | ≤ 2 (down from 6) |

---

## Phase 1 Completion

✅ Method under refactoring identified and read from source  
✅ All seven direct callees mapped from Phase 0 call hierarchy cross-referenced against actual source  
✅ IN SCOPE extractions defined (3 helpers, named, signed, CYC delta computed)  
✅ OUT OF SCOPE boundary drawn (signature, behaviour, other methods, other files)  
✅ Extraction plan sequenced (4 steps)  
✅ Risk assessment completed  
✅ Success criteria defined and measurable  

**Next Phase**: Phase 2 (Architecture Plan)
