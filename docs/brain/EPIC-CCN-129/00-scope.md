# Phase 1: Scope Definition - EPIC-CCN-129

## Epic Metadata
- **Epic ID**: EPIC-CCN-129
- **Target Method**: `SymmetryGuardTryResolveFollowersForDispatch`
- **File**: `src/V12_002.Symmetry.Replace.cs`
- **Lines**: 138-189 (52 lines)
- **Current CYC**: 18
- **Target CYC**: ≤ 8
- **Reduction Required**: 56%

## Method Signature
```csharp
private void SymmetryGuardTryResolveFollowersForDispatch(string dispatchId, DateTime nowUtc)
```

## Current Structure Analysis

### Logical Blocks
1. **Guard Block** (lines 138-139): Early return validation
2. **Worklist Initialization** (line 141): `followersToResolve` list creation
3. **Dispatch Context Scan** (lines 143-161): First follower collection loop
4. **Pending Fills Scan** (lines 163-175): Second follower collection loop
5. **Resolution Loop** (lines 177-189): Process each collected follower

### Complexity Breakdown
- **Base**: 1
- **Guard**: +1 (if empty dispatchId)
- **Dispatch Context Block**: +6
  - TryGetValue: +1
  - null check: +1
  - foreach: +1
  - if empty: +1
  - TryGetValue: +1
  - if not equals: +1
  - if not contains: +1
- **Pending Fills Block**: +5
  - foreach: +1
  - TryGetValue: +1
  - if not equals: +1
  - if contains: +1
  - (skip add): +1
- **Resolution Loop**: +5
  - foreach: +1
  - TryGetValue: +1
  - TryGetValue: +1
  - if not null and IsFollower: +1
  - if TryResolve: +1

**Total CYC**: 18

## Scope Boundary (V12.23 Compliance)

### IN SCOPE ✅
1. Extract "Dispatch Context Scan" logic (lines 143-161)
2. Extract "Pending Fills Scan" logic (lines 163-175)
3. Extract "Resolution Loop" logic (lines 177-189)

### OUT OF SCOPE ❌
1. Modifying `SymmetryGuardTryResolveFollower` (called method)
2. Changing data structures (symmetryDispatchById, symmetryPendingFollowerFills)
3. Altering ADR-019 immutable snapshot semantics
4. Touching other methods in file

### Single-Method Boundary Validation
- ✅ All extractions are sub-blocks of target method
- ✅ No cross-method refactoring
- ✅ No data structure changes
- ✅ Pure structural decomposition

## Extraction Strategy

### Extraction 1: `CollectFollowersFromDispatchContext`
**Purpose**: Isolate dispatch context follower collection
**Lines**: 143-161
**Signature**:
```csharp
private void CollectFollowersFromDispatchContext(
    string dispatchId,
    List<string> followersToResolve
)
```
**CYC Reduction**: -6
**Rationale**: Self-contained loop with clear input/output

### Extraction 2: `CollectFollowersFromPendingFills`
**Purpose**: Isolate pending fills follower collection
**Lines**: 163-175
**Signature**:
```csharp
private void CollectFollowersFromPendingFills(
    string dispatchId,
    List<string> followersToResolve
)
```
**CYC Reduction**: -5
**Rationale**: Independent scan with distinct logic

### Extraction 3: `ResolveCollectedFollowers`
**Purpose**: Isolate follower resolution loop
**Lines**: 177-189
**Signature**:
```csharp
private void ResolveCollectedFollowers(
    List<string> followersToResolve,
    DateTime nowUtc
)
```
**CYC Reduction**: -5
**Rationale**: Clear separation of concerns

## Post-Extraction Structure

### Caller (CYC ≤ 3)
```csharp
private void SymmetryGuardTryResolveFollowersForDispatch(string dispatchId, DateTime nowUtc)
{
    if (string.IsNullOrEmpty(dispatchId))
        return;

    var followersToResolve = new List<string>();

    CollectFollowersFromDispatchContext(dispatchId, followersToResolve);
    CollectFollowersFromPendingFills(dispatchId, followersToResolve);
    ResolveCollectedFollowers(followersToResolve, nowUtc);
}
```

**Final CYC**: 2 (guard + base)

## Risk Assessment

### Complexity Risk: LOW
- Clear logical boundaries
- No shared mutable state between extractions
- List passed by reference (no copy overhead)

### Behavioral Risk: LOW
- Pure structural movement
- No logic changes
- Preserves ADR-019 immutable snapshot semantics
- Maintains lock-free concurrent access patterns

### Integration Risk: LOW
- Method is private (no external callers)
- Called from `OnBarUpdate` timer path
- No FSM state dependencies

## V12 DNA Compliance

### Lock-Free ✅
- No locks in target method
- Preserves ConcurrentDictionary semantics
- Maintains ADR-019 immutable snapshot pattern

### ASCII-Only ✅
- No Unicode in method
- All strings are ASCII

### Correctness by Construction ✅
- Early return prevents invalid state
- List initialization before population
- Null checks preserved in extractions

## Jane Street Alignment

### Cognitive Simplicity ✅
- 18 → 2 CYC reduction improves reasoning
- Clear separation of collection vs. resolution
- Each extracted method has single responsibility

### Microsecond Latency ✅
- No new allocations (list reused)
- No boxing/unboxing
- Preserves zero-lock concurrent access

### Exhaustive Testing ✅
- Reduced branching paths
- Easier to test each phase independently
- Clear test boundaries

## Dependencies

### Data Structures (Read-Only)
- `symmetryDispatchById` (ConcurrentDictionary)
- `symmetryFleetEntryToDispatch` (ConcurrentDictionary)
- `symmetryPendingFollowerFills` (ConcurrentDictionary)
- `activePositions` (ConcurrentDictionary)

### Called Methods
- `SymmetryGuardTryResolveFollower` (existing, no changes)

### Callers
- `OnBarUpdate` (timer path, no changes needed)

## Success Criteria

### Functional ✅
- [ ] All 3 extractions compile
- [ ] `deploy-sync.ps1` passes
- [ ] F5 in NinjaTrader loads strategy
- [ ] BUILD_TAG appears in output

### Quality ✅
- [ ] Caller CYC ≤ 3
- [ ] Each extracted method CYC ≤ 8
- [ ] No logic drift
- [ ] ASCII-only compliance
- [ ] Zero new locks

### Testing ✅
- [ ] Unit tests for each extracted method
- [ ] Integration test (F5 verification)
- [ ] Behavioral equivalence verified

## Blast Radius

### Direct Impact
- **Files Modified**: 1 (`V12_002.Symmetry.Replace.cs`)
- **Methods Modified**: 1 (target method)
- **Methods Added**: 3 (extractions)
- **Lines Changed**: ~52 lines refactored

### Indirect Impact
- **Callers**: 1 (`OnBarUpdate` timer path)
- **Callees**: 1 (`SymmetryGuardTryResolveFollower`)
- **Data Structures**: 0 (no changes)

### Risk Level: LOW
- Single-file change
- Private method (no external API impact)
- No data structure modifications
- Preserves all concurrent access patterns

## Phase 1 Completion Checklist

- [x] Method structure analyzed
- [x] Complexity breakdown documented
- [x] Extraction strategy defined
- [x] Scope boundary validated (V12.23)
- [x] Risk assessment completed
- [x] V12 DNA compliance verified
- [x] Jane Street alignment confirmed
- [x] Success criteria defined
- [x] Blast radius assessed

## Next Steps

**Phase 1.5**: Scope Boundary Validation
- Verify single-method boundary compliance
- Confirm no scope creep
- Director approval gate

**Phase 2**: Architecture Planning
- Detailed extraction plan
- Method signatures finalized
- Call graph diagrams
- Jane Street compliance checks

**Phase 3**: DNA & PR Audit
- Pre-flight safety checks
- Greptile/Arena AI review
- PR hygiene validation

**Phase 4**: Ticket Generation
- 3 tickets (one per extraction)
- Execution order defined
- Dependencies mapped

## Status
✅ **Phase 1 Complete** - Ready for Phase 1.5 (Scope Boundary Validation)
