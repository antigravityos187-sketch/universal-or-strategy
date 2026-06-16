# Phase 1.0: Scope Definition - EPIC-CCN-044

## Target Method
- **Method Name**: SymmetryGuardCascadeFollowerCleanup
- **File**: src/V12_002.Symmetry.Replace.cs
- **Current Complexity**: 10
- **Target Complexity**: <=8 (Jane Street strict standard)

## Extraction Strategy

### Complexity Reduction Plan
Break SymmetryGuardCascadeFollowerCleanup into 2-3 focused helper methods:

1. **Guard State Validation** (Extract conditional logic)
   - Validate guard state before cleanup
   - Check for null/invalid guards
   - Return early on invalid states

2. **Follower Reference Cleanup** (Extract follower-specific logic)
   - Clean up follower references
   - Handle empty follower collections
   - Deallocate follower resources

3. **Cascade Coordination** (Extract orchestration logic)
   - Coordinate cleanup across cascade
   - Manage cleanup ordering
   - Handle cleanup failures

### Expected Outcome
- Main method becomes orchestration-only (complexity <=5)
- Each helper method has single responsibility (complexity <=3)
- Total complexity reduced from 10 to <=8

## Boundary Definition

### IN SCOPE (Single Method Only)
✅ **SymmetryGuardCascadeFollowerCleanup method body**
- Extract conditional branches into helper methods
- Refactor cleanup logic into focused functions
- Add explicit state validation
- Maintain lock-free Actor/FSM pattern

### OUT OF SCOPE (Zero Changes)
❌ **Callers** - No changes to methods that invoke SymmetryGuardCascadeFollowerCleanup
❌ **Callees** - No changes to downstream utility methods
❌ **Other Methods** - No changes to other methods in V12_002.Symmetry.Replace.cs
❌ **Pre-existing Issues** - No fixing compilation errors outside this method
❌ **Scope Creep** - No "while we are here" improvements

### No Scope Creep Mandate
**ONE EPIC = ONE CONCERN**
- This epic extracts ONE method only
- No bundling multiple refactorings
- No fixing unrelated issues
- No architectural changes beyond extraction

## Success Criteria

### Functional Requirements
1. ✅ **Complexity Reduced**: From 10 to <=8
2. ✅ **All Tests Pass**: No regression in test suite
3. ✅ **No Behavior Changes**: Identical runtime behavior
4. ✅ **Lock-Free Pattern**: Actor/FSM pattern maintained

### Quality Requirements
1. ✅ **ASCII-Only**: No Unicode characters in code
2. ✅ **V12 DNA Compliance**: "Make illegal states unrepresentable"
3. ✅ **Jane Street Alignment**: Cognitive simplicity prioritized
4. ✅ **Single Responsibility**: Each extracted method has one purpose

### Verification Requirements
1. ✅ **Build Success**: dotnet build passes
2. ✅ **Test Coverage**: Unit tests for each extracted method
3. ✅ **Complexity Audit**: python scripts/complexity_audit.py confirms <=8
4. ✅ **Pre-Push Validation**: All 13 checks pass

## Risk Assessment

### Risk Level: LOW
**Rationale**:
- Single method extraction (minimal blast radius)
- Complexity 10 is manageable (not a God-function)
- Cleanup logic is well-understood domain
- No cross-file dependencies

### Mitigation Strategy
1. **Checkpointing**: Enable Bob CLI checkpointing for rollback
2. **Incremental Extraction**: Extract one helper at a time
3. **Test-Driven**: Write tests before extraction
4. **Verification Loop**: Run tests after each extraction

## Jane Street Alignment

### Cognitive Simplicity
- Functions with CYC >8 are harder to reason about under microsecond latency
- Cleanup methods often have hidden edge cases
- Extraction makes each path explicit and testable

### Testing Standards
Per Jane Street KB (will_wilson_why_testing_hard_2026):
- Test each cleanup scenario independently
- Make illegal states unrepresentable through types
- Verify no resource leaks in edge cases

## Next Steps
1. Proceed to Phase 1.5 (Boundary Validation)
2. Create 01-scope-boundary.md
3. Validate no scope creep
4. Get Director approval before Phase 2

---
**Created**: 2026-06-15
**Epic**: EPIC-CCN-044
**Phase**: 1.0 (Scope Definition)
**Status**: PENDING_BOUNDARY_VALIDATION
