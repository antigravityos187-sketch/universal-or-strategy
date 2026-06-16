# Phase 1.0: Scope Definition - EPIC-CCN-071

## Extraction Scope (SINGLE METHOD ONLY)

**Target Method**:
- **Method Name**: ShadowProcessFollowerStopUpdate
- **File**: src/V12_002.SIMA.Shadow.cs
- **Current Complexity**: 12 (Cyclomatic Complexity)
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **Extraction Strategy**: Break into 2-3 helper methods

## Boundary Definition

### IN SCOPE
- ONLY the method body of ShadowProcessFollowerStopUpdate
- Internal logic extraction into private helper methods
- Complexity reduction from CYC=12 to CYC≤8
- Maintaining lock-free Actor/FSM pattern

### OUT OF SCOPE
- Callers of ShadowProcessFollowerStopUpdate (no changes)
- Callees invoked by ShadowProcessFollowerStopUpdate (no changes)
- Other methods in V12_002.SIMA.Shadow.cs (no changes)
- Pre-existing compilation errors (not our concern)
- While we are here improvements (scope creep)
- Refactoring other methods in the same file

## No Scope Creep Mandate

**ONE EPIC = ONE CONCERN**
- This EPIC addresses ONLY ShadowProcessFollowerStopUpdate complexity
- No bundling of multiple refactoring concerns
- No fixing unrelated issues discovered during extraction

## Success Criteria

1. **Complexity Reduction**: CYC reduced from 12 to ≤8
2. **Test Pass**: All existing tests pass (no behavior changes)
3. **Lock-Free Compliance**: Actor/FSM pattern maintained (no lock() blocks)
4. **ASCII-Only**: No Unicode/emoji in string literals
5. **Build Success**: Zero compilation errors
6. **Surgical Changes**: Only ShadowProcessFollowerStopUpdate modified

## Extraction Strategy

**Approach**: Decompose into 2-3 helper methods
- Identify logical sub-concerns within the method
- Extract each sub-concern into a private helper method
- Maintain single responsibility principle
- Preserve existing behavior exactly

## Risk Assessment

- **Complexity Risk**: LOW (CYC=12, below threshold of 15)
- **Jane Street Risk**: LOW (0 violations detected in Phase 0)
- **Overall Risk**: LOW
- **Rationale**: Method is below threshold but approaching it; proactive refactoring prevents future technical debt

## Jane Street Alignment

- **Cognitive Simplicity**: Functions with CYC>8 are harder to reason about under microsecond latency constraints
- **Test Exhaustiveness**: Lower complexity = fewer code paths = easier to test exhaustively
- **Race Condition Auditing**: Simpler logic = easier to verify lock-free correctness
- **V12 DNA**: Make illegal states unrepresentable requires simple, verifiable logic

## Approval Gate

**Status**: PENDING (awaits Phase 1.5 Boundary Validation)
