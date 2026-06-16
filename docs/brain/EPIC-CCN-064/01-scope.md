# Phase 1.0: Scope Definition - EPIC-CCN-064

## Extraction Scope (SINGLE METHOD ONLY)

**Target Method**:
- **Method Name**: ResolveFsm_ByScan
- **File**: src/V12_002.Symmetry.BracketFSM.cs
- **Current Complexity**: 12 (Cyclomatic Complexity)
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **Extraction Strategy**: Break into 2-3 helper methods

## Boundary Definition

### IN SCOPE
- ResolveFsm_ByScan method body ONLY
- Internal logic extraction into helper methods
- Complexity reduction from 12 to ≤8
- Maintain lock-free Actor/FSM pattern
- Preserve existing behavior (zero functional changes)

### OUT OF SCOPE
- Callers of ResolveFsm_ByScan (no changes)
- Callees invoked by ResolveFsm_ByScan (no changes)
- Other methods in V12_002.Symmetry.BracketFSM.cs
- Related files or modules
- Pre-existing compilation errors
- While we are here improvements

### No Scope Creep Rule
**ONE EPIC = ONE CONCERN**: This epic addresses ONLY the complexity of ResolveFsm_ByScan. No bundling of unrelated refactoring tasks.

## Success Criteria

1. **Complexity Reduction**:
   - ResolveFsm_ByScan complexity reduced from 12 to ≤8
   - Helper methods each have CYC ≤8

2. **Correctness**:
   - All existing tests pass (100% pass rate)
   - No behavior changes (bit-for-bit identical output)
   - No new compilation errors introduced

3. **Architecture Compliance**:
   - Lock-free Actor/FSM pattern maintained
   - No lock() statements introduced
   - ASCII-only compliance (no Unicode)
   - V12 DNA principles preserved

4. **Testing**:
   - Existing test coverage maintained
   - No test modifications required (behavior unchanged)

## Extraction Strategy

### Proposed Decomposition
Based on CYC=12, likely candidates for extraction:
1. **Conditional Logic Clusters**: Extract nested if/else chains into decision methods
2. **State Validation**: Extract FSM state validation into helper method
3. **Scan Logic**: Extract core scanning algorithm into focused method

### Helper Method Naming Convention
- Use descriptive names: ValidateFsmState(), PerformBracketScan(), etc.
- Follow existing codebase patterns
- Maintain V12 naming conventions

## Risk Assessment

- **Complexity Risk**: LOW (CYC=12, below threshold of 15)
- **Jane Street Risk**: LOW (0 violations detected in Phase 0)
- **Blast Radius**: MINIMAL (single method, no caller/callee changes)
- **Overall Risk**: LOW

## Next Steps

1. Phase 1.0 Complete: Scope defined
2. Phase 1.5: Boundary validation (mandatory V12.23 protocol)
3. Phase 2: Architecture planning
4. Phase 3: DNA & PR audit
