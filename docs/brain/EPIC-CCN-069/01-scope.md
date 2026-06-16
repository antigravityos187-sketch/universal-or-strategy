# Phase 1.0: Scope Definition - EPIC-CCN-069

## Extraction Scope (SINGLE METHOD ONLY)

**Target Method**: GetFsmExpectedPosition
- **File**: src/V12_002.Symmetry.BracketFSM.cs
- **Current Complexity**: 14 (Cyclomatic Complexity)
- **Target Complexity**: <=8 (Jane Street strict standard)
- **Extraction Strategy**: Break into 2-3 helper methods

### Complexity Reduction Plan

**Current State**:
- CYC = 14 (1 point below threshold of 15)
- Status: PASS but approaching limit
- Risk: MEDIUM - proactive refactoring recommended

**Target State**:
- CYC <= 8 (Jane Street strict standard for cognitive simplicity)
- Improved testability through smaller, focused methods
- Maintained lock-free Actor/FSM pattern

**Extraction Approach**:
1. Identify conditional branches contributing to complexity
2. Extract 2-3 helper methods for distinct logical concerns
3. Each helper method should have CYC <= 3-4
4. Preserve original method signature and behavior

## Boundary Definition

### IN SCOPE
- **Method Body**: GetFsmExpectedPosition implementation only
- **Refactoring**: Extract conditional logic into helper methods
- **Testing**: Verify existing tests still pass
- **Complexity**: Reduce from 14 to <=8

### OUT OF SCOPE
- **Callers**: No changes to methods that call GetFsmExpectedPosition
- **Callees**: No changes to methods called by GetFsmExpectedPosition
- **Other Methods**: No changes to other methods in V12_002.Symmetry.BracketFSM.cs
- **File Structure**: No changes to class structure or namespace
- **Behavior**: No functional changes - pure refactoring only

### Scope Creep Prevention (V12.23 Protocol)
- No "while we are here" improvements to adjacent code
- No fixing pre-existing compilation errors in other methods
- No bundling multiple concerns into single EPIC
- No architectural changes beyond method extraction
- ONE EPIC = ONE CONCERN = ONE METHOD

## Success Criteria

### Functional Requirements
1. **Behavior Preservation**: All existing tests pass without modification
2. **Signature Stability**: Method signature remains unchanged
3. **Return Value**: Identical output for all input combinations
4. **Side Effects**: No new side effects introduced

### Quality Requirements
1. **Complexity Target**: CYC reduced from 14 to <=8
2. **Helper Methods**: Each extracted method has CYC <=4
3. **Readability**: Improved cognitive load through focused methods
4. **Testability**: Each helper method is independently testable

### Architectural Requirements
1. **Lock-Free Pattern**: Maintain Actor/FSM Enqueue model
2. **ASCII-Only**: No Unicode characters in code or comments
3. **V12 DNA**: Make illegal states unrepresentable principle upheld
4. **Jane Street Alignment**: Cognitive simplicity over clever abstractions

### Verification Requirements
1. **Build**: dotnet build succeeds with zero errors
2. **Tests**: dotnet test passes with 100% success rate
3. **Lint**: powershell -File .\scripts\lint.ps1 shows no new violations
4. **Complexity**: python3 scripts/complexity_audit.py confirms CYC <=8

## Risk Assessment

**Complexity Risk**: LOW
- Single method extraction with clear boundaries
- No changes to callers or callees
- Existing tests provide safety net

**Blast Radius Risk**: MEDIUM
- Method is part of core FSM state management
- Position calculations affect bracket FSM behavior
- Requires careful testing of edge cases

**Jane Street Risk**: LOW
- Proactive refactoring before threshold breach
- Aligns with cognitive simplicity principles
- No performance-critical hot path modifications

**Overall Risk**: LOW-MEDIUM
- Well-defined scope with clear boundaries
- Existing test coverage provides safety
- Incremental approach minimizes disruption

## Approval Status

**Phase 1.0 Status**: READY FOR BOUNDARY VALIDATION
**Next Step**: Proceed to Phase 1.5 (Boundary Validation)
