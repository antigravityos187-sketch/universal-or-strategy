# Phase 1.0: Scope Definition - EPIC-CCN-038

## Extraction Scope (SINGLE METHOD ONLY)

### Target Method
- **Method Name**: MoveSpecificTarget
- **File**: src/V12_002.Trailing.Breakeven.cs
- **Current Complexity**: 12
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **Extraction Strategy**: Break into 2-3 helper methods

### Complexity Reduction Plan
**Current State**: 12 cyclomatic complexity (80% of V12 threshold)
**Target State**: ≤8 cyclomatic complexity (Jane Street alignment)
**Reduction Required**: Minimum 4 complexity points

### Extraction Strategy
1. **Conditional Logic Extraction**: Extract nested if/else chains into validation methods
2. **Target Calculation Isolation**: Separate price target computation logic
3. **State Validation Separation**: Decouple state checking from action execution

## Boundary Definition

### IN SCOPE
- **MoveSpecificTarget method body ONLY**
- Refactoring internal logic into helper methods
- Reducing cyclomatic complexity from 12 to ≤8
- Maintaining lock-free Actor/FSM pattern
- Preserving exact behavior (zero functional changes)

### OUT OF SCOPE
- **Callers**: No changes to methods that call MoveSpecificTarget
- **Callees**: No changes to methods called by MoveSpecificTarget
- **Other Methods**: No changes to other methods in V12_002.Trailing.Breakeven.cs
- **File-Level Changes**: No namespace, using statements, or class-level modifications
- **Pre-existing Issues**: No fixing compilation errors outside this method
- **Scope Creep**: No "while we're here" improvements

### No Scope Creep Enforcement
- **ONE EPIC = ONE CONCERN**: This epic addresses ONLY MoveSpecificTarget complexity
- **No Bundling**: No combining with other refactoring tasks
- **No Opportunistic Fixes**: No fixing unrelated issues discovered during extraction

## Success Criteria

### Functional Requirements
1. **Complexity Reduced**: From 12 to ≤8 (Jane Street strict standard)
2. **All Tests Pass**: Zero test failures or regressions
3. **No Behavior Changes**: Exact functional equivalence maintained
4. **Lock-Free Pattern**: Actor/FSM Enqueue model preserved (no lock statements)

### Quality Requirements
1. **ASCII-Only Compliance**: No Unicode characters in string literals
2. **Build Success**: Zero compilation errors
3. **Lint Clean**: Zero new Roslyn violations
4. **CSharpier Formatted**: Automatic formatting applied

### V12 DNA Alignment
1. **Correctness by Construction**: Extracted methods have clear contracts
2. **Cognitive Simplicity**: Each extracted method has single responsibility
3. **Testability**: Extracted methods are independently testable
4. **Hard-Link Integrity**: deploy-sync.ps1 executed after changes

## Risk Assessment

### Blast Radius
- **Risk Level**: MEDIUM
- **Rationale**: Method is in critical path (trailing breakeven logic)
- **Mitigation**: Single-method extraction minimizes impact surface

### Dependencies
- **Expected Callers**: Main strategy execution loop
- **Expected Callees**: Order management subsystem, state machine
- **Coupling Risk**: Moderate (trailing breakeven affects all active positions)

### Testing Strategy
1. **Unit Tests**: Add tests for each extracted helper method
2. **Integration Tests**: Verify MoveSpecificTarget behavior unchanged
3. **Edge Cases**: Test boundary conditions in isolation
4. **Regression Suite**: Run full test suite before/after

## Metadata
- **Epic ID**: EPIC-CCN-038
- **Phase**: 1.0 (Scope Definition)
- **Protocol Version**: V12.23
- **Created**: 2026-06-15
- **Jane Street Alignment**: Verified (complexity threshold ≤8)
