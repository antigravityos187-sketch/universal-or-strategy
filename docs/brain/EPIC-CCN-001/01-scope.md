# Phase 1.0: Scope Definition - EPIC-CCN-001

## Extraction Scope (SINGLE METHOD ONLY)

**Target Method**:
- **Method Name**: SymmetryGuardReplaceExistingFollowerTarget
- **File**: src/V12_002.Symmetry.Replace.cs
- **Current Complexity**: 18 (Cyclomatic Complexity)
- **Target Complexity**: <=8 (Jane Street strict standard)
- **Extraction Strategy**: Break into 2-3 helper methods

## Boundary Definition

### IN SCOPE
- Method body of SymmetryGuardReplaceExistingFollowerTarget ONLY
- Extract conditional branches into helper methods
- Reduce cyclomatic complexity from 18 to <=8
- Maintain lock-free Actor/FSM pattern
- Add unit tests for extracted methods

### OUT OF SCOPE
- Callers of SymmetryGuardReplaceExistingFollowerTarget
- Callees invoked by SymmetryGuardReplaceExistingFollowerTarget
- Other methods in V12_002.Symmetry.Replace.cs
- Pre-existing compilation errors in other files
- "While we're here" improvements
- Refactoring unrelated symmetry logic

### No Scope Creep Mandate
**ONE EPIC = ONE CONCERN**
- This EPIC addresses ONLY the complexity of SymmetryGuardReplaceExistingFollowerTarget
- No bundling of multiple refactoring concerns
- No fixing unrelated issues discovered during analysis

## Success Criteria

1. **Complexity Reduction**:
   - Cyclomatic complexity reduced from 18 to <=8
   - Each extracted helper method has complexity <=8

2. **Correctness**:
   - All existing tests pass (100% pass rate)
   - No behavior changes (pure refactoring)
   - Lock-free Actor/FSM pattern maintained

3. **Code Quality**:
   - ASCII-only compliance (no Unicode/emoji)
   - CSharpier formatting applied
   - No new Codacy violations

4. **Testing**:
   - Unit tests added for extracted helper methods
   - Test coverage for all new code paths

5. **Build Health**:
   - Zero compilation errors
   - Zero lint violations
   - Hard-link integrity maintained (deploy-sync.ps1)

## Extraction Strategy

### Approach
1. **Identify Conditional Branches**: Analyze the 18-point complexity to identify nested conditionals
2. **Extract Helper Methods**: Create 2-3 private helper methods for distinct logical concerns
3. **Preserve Semantics**: Ensure extracted methods maintain exact behavior
4. **Test Coverage**: Add unit tests for each extracted method

### Complexity Budget
- Original method: 18 points
- Target: <=8 points per method
- Expected breakdown:
  - Main method: 6-8 points (orchestration logic)
  - Helper 1: 4-6 points (specific concern)
  - Helper 2: 4-6 points (specific concern)
  - Helper 3 (if needed): 4-6 points (specific concern)

## Risk Mitigation

1. **Blast Radius**: Limited to single method (minimal risk)
2. **Regression**: Existing tests provide safety net
3. **Performance**: No new allocations or locks introduced
4. **Maintainability**: Improved cognitive simplicity (Jane Street alignment)

## Approval Gate

- **Status**: PENDING (awaits Phase 1.5 boundary validation)
- **Next Step**: Create 01-scope-boundary.md for V12.23 protocol compliance