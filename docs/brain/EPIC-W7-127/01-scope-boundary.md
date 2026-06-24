# Phase 1: Scope Boundary - EPIC-W7-127

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Execution Time**: 2026-06-24T01:50:14Z
- **Input**: 00-hotspots.md
- **Output**: 01-scope-boundary.md

## Target Method
- **Method**: SymmetryGuardOnFollowerFill
- **File**: src/V12_002.Symmetry.Follower.cs
- **Current CYC**: 16
- **Target CYC**: ≤8 (Jane Street strict standard)
- **Reduction Required**: -8 complexity points

## Scope Definition

### IN SCOPE

#### Primary Extraction Target
**SymmetryGuardOnFollowerFill** (CYC 16 → ≤8)
- **Location**: src/V12_002.Symmetry.Follower.cs:17
- **Lines**: 72
- **Nesting Depth**: 6 levels
- **Rationale**: Exceeds Jane Street threshold by 2x, requires decomposition

#### Extraction Strategy
Based on the 60 callees identified, extract logical blocks into helper methods:

1. **Fleet Entry Validation** (estimated CYC 2-3)
   - Validate symmetryFleetEntryToDispatch
   - Early return on null/invalid state

2. **Dispatch Resolution** (estimated CYC 2-3)
   - Resolve symmetryDispatchById
   - Handle missing dispatch scenarios

3. **Master Anchor Application** (estimated CYC 2-3)
   - Call SymmetryGuardApplyMasterAnchor
   - Handle anchor application results

4. **Follower Bracket Submission** (estimated CYC 2-3)
   - Call SymmetryGuardSubmitFollowerBracket
   - Handle submission results

5. **Follower Resolution** (estimated CYC 2-3)
   - Call SymmetryGuardTryResolveFollower
   - Handle resolution outcomes

6. **Pending Fill Management** (estimated CYC 2-3)
   - Manage symmetryPendingFollowerFills
   - Update pending state

#### Files to Modify
- `src/V12_002.Symmetry.Follower.cs` (primary target)

#### Expected Outcome
- Main method: CYC ≤8 (orchestration only)
- 4-6 extracted helper methods: CYC ≤8 each
- Total complexity distributed across focused, single-responsibility methods

### OUT OF SCOPE

#### No External Dependencies
- **Zero importers**: No other files depend on this method
- **Zero dependents**: No external symbols reference this method
- **Isolated change**: Refactoring contained within single file

#### Excluded from Refactoring
1. **Existing Helper Methods** (already called by target)
   - SymmetryGuardApplyMasterAnchor
   - SymmetryGuardSubmitFollowerBracket
   - SymmetryGuardTryResolveFollower
   - These are callees, not targets for extraction

2. **Data Structures**
   - symmetryFleetEntryToDispatch (constant)
   - symmetryDispatchById (constant)
   - symmetryPendingFollowerFills (constant)
   - No structural changes to these collections

3. **Logging Infrastructure**
   - LogBuffer.Format calls remain unchanged
   - No modifications to logging patterns

4. **Test Files**
   - No test modifications required (LOW blast radius)
   - Tests will be added for extracted methods only

5. **Other Symmetry Methods**
   - This epic targets ONLY SymmetryGuardOnFollowerFill
   - Other methods in V12_002.Symmetry.Follower.cs are out of scope

## Scope Validation

### Complexity Budget
- **Current**: 16 CYC
- **Target**: ≤8 CYC per method
- **Extraction Count**: 4-6 helper methods
- **Validation**: Each extracted method must have CYC ≤8

### Blast Radius Confirmation
- **Direct Importers**: 0 ✅
- **Direct Dependents**: 0 ✅
- **Risk Level**: LOW ✅
- **Isolation**: Changes contained to single file ✅

### Jane Street Alignment
- **Cognitive Simplicity**: Each method should be reasoned about in <10 seconds
- **Exhaustive Testing**: Each method should have <256 test paths (2^8)
- **Race Condition Auditing**: Simpler methods easier to audit for lock-free correctness
- **Microsecond Latency**: Reduced nesting improves branch prediction

## Success Criteria

### Phase 1 Completion
- ✅ Scope boundary clearly defined (IN SCOPE vs OUT OF SCOPE)
- ✅ Extraction strategy documented (4-6 helper methods)
- ✅ Complexity budget validated (16 → ≤8 per method)
- ✅ Blast radius confirmed (LOW risk, isolated change)
- ✅ Jane Street principles applied (cognitive simplicity)

### Ready for Phase 2
- Architecture planning can proceed with clear scope
- No ambiguity about what will/won't be refactored
- Complexity reduction path is well-defined

## Phase 1 Status: COMPLETED

**Next Phase**: Phase 2 (Architecture Planning)
