# Phase 1: Scope Definition - EPIC-W7-011

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Execution Time**: 2026-06-24T01:29:50Z
- **Input**: 00-hotspots.md
- **Output**: 01-scope-boundary.md

## Epic Summary
**Target**: DestroyPanel method in src/V12_002.UI.Panel.Construction.cs
**Current Complexity**: CYC=19 (exceeds ≤8 threshold by 237%)
**Risk Level**: LOW-MEDIUM (zero blast radius, high complexity)
**Lines of Code**: 190 lines

## IN SCOPE

### Primary Extraction Target
- **Method**: DestroyPanel (line 320, CYC=19)
- **File**: src/V12_002.UI.Panel.Construction.cs
- **Objective**: Reduce CYC from 19 to ≤8 through extraction

### Extraction Candidates (Based on 190 lines, CYC=19)

#### 1. Timer Disposal Logic
- **Rationale**: References _placementRetryTimer (line 157)
- **Expected CYC**: ≤3
- **Responsibility**: Clean up timer resources
- **Target Method Name**: DisposePlacementRetryTimer()

#### 2. Panel Handler Detachment
- **Rationale**: Calls DetachPanelHandlers (line 229 in Panel.Handlers.cs)
- **Expected CYC**: ≤3
- **Responsibility**: Detach event handlers before disposal
- **Target Method Name**: DetachAndCleanupHandlers()

#### 3. Resource Cleanup Sequences
- **Rationale**: Nested conditionals (6 levels deep) suggest complex cleanup logic
- **Expected CYC**: ≤5
- **Responsibility**: Dispose panel resources in correct order
- **Target Method Name**: CleanupPanelResources()

#### 4. Error Handling Paths
- **Rationale**: High CYC (19) suggests multiple error paths
- **Expected CYC**: ≤4
- **Responsibility**: Handle disposal errors gracefully
- **Target Method Name**: HandleDisposalErrors()

### Files to Modify
1. **src/V12_002.UI.Panel.Construction.cs** (primary target)
   - Extract helper methods from DestroyPanel
   - Maintain DestroyPanel as orchestrator (CYC ≤8)

### Dependencies to Preserve
1. **DetachPanelHandlers** (src/V12_002.UI.Panel.Handlers.cs, line 229)
   - Keep existing call, no changes to this method
2. **_placementRetryTimer** (src/V12_002.UI.Panel.Construction.cs, line 157)
   - Keep existing field reference, no changes to declaration

## OUT OF SCOPE

### External Files (Zero Blast Radius)
- **NO changes** to src/V12_002.UI.Panel.Handlers.cs
- **NO changes** to any other files outside Panel.Construction.cs
- **Rationale**: Zero blast radius means no external dependencies

### Method Signatures
- **NO changes** to DestroyPanel signature (0 parameters, void return)
- **NO changes** to public API surface
- **Rationale**: Maintain backward compatibility (though no external callers exist)

### Behavioral Changes
- **NO changes** to disposal order or logic flow
- **NO changes** to error handling behavior
- **NO changes** to timer cleanup semantics
- **Rationale**: Refactoring for complexity only, not behavior

### Related Methods (Not in This Epic)
- **DetachPanelHandlers** (already exists, CYC unknown)
- **Other Panel.Construction.cs methods** (separate epics if needed)
- **Panel.Handlers.cs methods** (separate file, out of scope)

### Testing Scope
- **NO new test files** (if no tests exist currently)
- **NO test modifications** (unless tests break due to extraction)
- **Rationale**: Focus on complexity reduction, not test coverage expansion

## Scope Boundaries

### What We WILL Do
1. ✅ Extract 3-4 helper methods from DestroyPanel
2. ✅ Reduce DestroyPanel CYC from 19 to ≤8
3. ✅ Ensure each extracted method has CYC ≤8
4. ✅ Maintain single entry point (DestroyPanel as orchestrator)
5. ✅ Preserve all existing behavior and semantics
6. ✅ Keep all method signatures unchanged

### What We WILL NOT Do
1. ❌ Modify any files outside Panel.Construction.cs
2. ❌ Change DestroyPanel method signature
3. ❌ Alter disposal logic or error handling behavior
4. ❌ Refactor DetachPanelHandlers or other callees
5. ❌ Add new tests (unless required for verification)
6. ❌ Change timer field declarations or initialization

## Success Criteria

### Complexity Targets
- **DestroyPanel**: CYC ≤8 (currently 19)
- **Each extracted method**: CYC ≤8
- **Total reduction**: At least 11 CYC points removed from DestroyPanel

### Code Quality
- **Max nesting depth**: ≤4 levels (currently 6)
- **Method size**: DestroyPanel ≤50 lines (currently 190)
- **Single responsibility**: Each method does one thing

### Verification
- **Build passes**: dotnet build succeeds
- **No behavioral changes**: Disposal logic unchanged
- **No external impacts**: Zero files affected (confirmed by blast radius)

## Risk Mitigation

### Low Risk Factors
- ✅ Zero blast radius (no external dependencies)
- ✅ No external callers (no coordination needed)
- ✅ Low churn (stable, well-understood code)
- ✅ Clear dependencies (only 2 callees)

### Medium Risk Factors
- ⚠️ High complexity (CYC=19) requires careful extraction
- ⚠️ Deep nesting (6 levels) may hide subtle dependencies
- ⚠️ Large method (190 lines) increases extraction complexity

### Mitigation Strategy
1. **Extract incrementally**: One helper method at a time
2. **Verify after each extraction**: Build and test after each change
3. **Preserve semantics**: No behavioral changes, only structural
4. **Document dependencies**: Track all field/method references

## Phase 1 Completion

### Artifacts Generated
- ✅ 01-scope-boundary.md (this file)
- ⏳ manifest.json (to be updated)

### Next Phase
- **Phase 1.5**: Scope Boundary Validation (prevent scope creep)
- **Validator**: Confirm IN SCOPE items are achievable
- **Validator**: Confirm OUT OF SCOPE items are respected

### Key Decisions
1. **Extraction count**: 3-4 helper methods (based on 190 lines, CYC=19)
2. **Orchestrator pattern**: DestroyPanel remains as entry point
3. **File isolation**: Only modify Panel.Construction.cs
4. **Signature preservation**: No API changes
5. **Behavior preservation**: No logic changes, only structure

---

**Phase 1 Status**: ✅ COMPLETED
**Scope Clarity**: HIGH (clear boundaries, zero external dependencies)
**Extraction Strategy**: INCREMENTAL (one method at a time)
**Risk Level**: LOW-MEDIUM (isolated but complex)
