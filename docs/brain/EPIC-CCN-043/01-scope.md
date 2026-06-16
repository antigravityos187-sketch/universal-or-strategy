# Phase 1.0: Scope Definition - EPIC-CCN-043

## Extraction Scope (SINGLE METHOD ONLY)

### Target Method
- **Method Name**: SymmetryGuardSubmitFollowerBracket
- **File**: src/V12_002.Symmetry.Follower.cs
- **Current Complexity**: 12
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **Extraction Strategy**: Break into 2-3 helper methods

### Complexity Reduction Plan
**Current State**: 12 cyclomatic complexity (80% of V12 threshold)
**Target State**: ≤8 cyclomatic complexity (safety margin below threshold)

**Extraction Strategy**:
1. Extract guard condition validation into separate method (reduces 3-4 branches)
2. Extract bracket submission validation into separate method (reduces 2-3 branches)
3. Apply early return pattern to reduce nesting depth
4. Maintain single responsibility: guard logic only

## Boundary Definition

### IN SCOPE
- **Method Body**: SymmetryGuardSubmitFollowerBracket implementation only
- **Extraction**: Create 2-3 focused helper methods for validation logic
- **Refactoring**: Reduce cyclomatic complexity from 12 to ≤8
- **Testing**: Add unit tests for extracted validation methods

### OUT OF SCOPE
- **Callers**: No changes to methods that call SymmetryGuardSubmitFollowerBracket
- **Callees**: No changes to downstream methods called by target method
- **Other Methods**: No changes to other methods in V12_002.Symmetry.Follower.cs
- **File Structure**: No changes to class structure or namespace
- **Compilation Errors**: No fixing pre-existing errors outside target method

### No Scope Creep: ONE EPIC = ONE CONCERN
- This EPIC focuses EXCLUSIVELY on SymmetryGuardSubmitFollowerBracket
- No "while we're here" improvements to adjacent code
- No bundling multiple refactoring concerns
- No architectural changes beyond single-method extraction

## Success Criteria

### Functional Requirements
1. **Complexity Reduced**: From 12 to ≤8 cyclomatic complexity
2. **All Tests Pass**: Existing test suite passes without modification
3. **No Behavior Changes**: Extracted methods preserve exact logic
4. **Lock-Free Pattern**: Actor/FSM pattern maintained (no lock() blocks)

### Quality Requirements
1. **ASCII-Only**: No Unicode characters in extracted code
2. **V12 DNA Compliance**: Correctness-by-construction pattern maintained
3. **Test Coverage**: Unit tests added for each extracted validation method
4. **Code Health**: CodeScene score improves or maintains current level

### Verification Requirements
1. **Build Success**: dotnet build passes with zero errors
2. **Test Success**: dotnet test passes with 100% pass rate
3. **Complexity Audit**: complexity_audit.py confirms ≤8
4. **Pre-Push Validation**: All 13 checks pass (fast mode minimum)

## Extraction Approach

### Method Decomposition Strategy
Original Method Structure (12 branches):
- Guard condition checks (4-5 branches)
- Bracket validation logic (3-4 branches)
- Submission state validation (2-3 branches)
- Final submission logic (1 branch)

Proposed Extraction (≤8 branches total):
- SymmetryGuardSubmitFollowerBracket (≤4 branches)
- ValidateGuardConditions() (≤3 branches)
- ValidateBracketSubmission() (≤3 branches)

### V12 DNA Alignment
- **Correctness by Construction**: Guard methods make invalid states unrepresentable
- **Lock-Free Actor Pattern**: No lock() blocks introduced
- **ASCII-Only Compliance**: All string literals remain ASCII
- **Jane Street Standard**: Target complexity ≤8 (strict HFT standard)

## Risk Assessment

### Overall Risk: LOW
**Rationale**:
1. **Single Method**: Isolated refactoring with clear boundaries
2. **Guard Logic**: Stateless validation (no shared state mutation)
3. **Test Coverage**: Existing tests validate behavior preservation
4. **Complexity**: Moderate (12) to low (≤8) reduction is straightforward

### Mitigation Strategy
1. **Checkpoint Before**: Create restore point before any changes
2. **Incremental Extraction**: Extract one helper method at a time
3. **Test After Each**: Run tests after each extraction step
4. **Rollback Ready**: Use Bob CLI /restore if tests fail

## Metadata
- **Epic**: EPIC-CCN-043
- **Phase**: 1.0 (Scope Definition)
- **Date**: 2026-06-15
- **Scope Type**: Single-Method Extraction
- **Complexity Target**: ≤8 (Jane Street strict)
- **Approval Status**: Pending Phase 1.5 boundary validation
