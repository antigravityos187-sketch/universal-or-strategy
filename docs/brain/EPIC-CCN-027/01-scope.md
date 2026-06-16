# Phase 1.0: Scope Definition - EPIC-CCN-027

## Extraction Scope (SINGLE METHOD ONLY)

### Target Method
- **Method Name**: `Dispatch_PublishMarketBracketToPhoton`
- **File**: `src/V12_002.SIMA.Dispatch.cs`
- **Current Complexity**: 21 (CYC)
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **Fallback Target**: ≤15 (V12 DNA threshold)

### Extraction Strategy

**Primary Goal**: Break into 2-3 helper methods to achieve CYC ≤8

**Proposed Decomposition**:
1. **Extract Bracket Validation Logic** → `ValidateMarketBracketState()`
   - Isolate all bracket state validation checks
   - Return validation result (bool or enum)
   - Estimated complexity reduction: 5-7 points

2. **Extract Photon Message Construction** → `BuildPhotonBracketMessage()`
   - Isolate message/payload construction logic
   - Pure function (no side effects)
   - Estimated complexity reduction: 3-5 points

3. **Simplify Main Method** → Guard clauses + single responsibility
   - Early returns for invalid states
   - Single Photon enqueue call
   - Target complexity: 6-8 points

**Expected Outcome**:
- Main method: CYC 6-8 (orchestration only)
- Helper 1 (validation): CYC 4-6
- Helper 2 (construction): CYC 3-5
- Total complexity preserved, cognitive load reduced

## Boundary Definition

### ✅ IN SCOPE
- **ONLY**: `Dispatch_PublishMarketBracketToPhoton` method body
- Extract helper methods within same class
- Refactor conditional logic within method
- Add guard clauses for early returns
- Maintain lock-free Actor/FSM pattern

### ❌ OUT OF SCOPE
- **NO** changes to callers (SIMA dispatch orchestration)
- **NO** changes to callees (Photon kernel, bracket state helpers)
- **NO** changes to other methods in `V12_002.SIMA.Dispatch.cs`
- **NO** changes to method signature
- **NO** changes to public API surface
- **NO** "while we're here" improvements
- **NO** fixing pre-existing compilation errors
- **NO** bundling multiple concerns

### Scope Creep Prevention
**ONE EPIC = ONE CONCERN**: This epic addresses ONLY the complexity of `Dispatch_PublishMarketBracketToPhoton`. Any other issues discovered during analysis must be logged as separate epics.

## Success Criteria

### Functional Requirements
1. ✅ **Complexity Reduced**: CYC reduced from 21 to ≤8 (strict) or ≤15 (acceptable)
2. ✅ **All Tests Pass**: Existing test suite passes without modification
3. ✅ **No Behavior Changes**: Identical runtime behavior (verified via tests)
4. ✅ **Lock-Free Pattern Maintained**: No introduction of `lock()` statements
5. ✅ **Actor/FSM Pattern Preserved**: Enqueue model unchanged

### Non-Functional Requirements
1. ✅ **ASCII-Only Compliance**: No Unicode, emoji, or curly quotes
2. ✅ **Jane Street Alignment**: Cognitive simplicity prioritized
3. ✅ **Testability Improved**: Extracted methods are unit-testable
4. ✅ **Auditability Enhanced**: Simpler logic easier to review for race conditions

### Quality Gates
1. ✅ **Build Passes**: `dotnet build` succeeds
2. ✅ **Tests Pass**: `dotnet test` 100% pass rate
3. ✅ **Lint Clean**: `powershell -File .\scripts\lint.ps1` zero violations
4. ✅ **Complexity Verified**: `python scripts/complexity_audit.py` confirms CYC ≤8
5. ✅ **Pre-Push Validation**: `powershell -File .\scripts\pre_push_validation.ps1` passes

## Risk Assessment

### Overall Risk: LOW-MEDIUM

**Rationale**:
- **Isolated Change**: Single method, no API changes
- **Well-Defined Scope**: Clear boundaries prevent scope creep
- **Test Coverage**: Existing tests validate behavior preservation
- **Reversible**: Git history allows rollback if issues arise

### Mitigation Strategies
1. **Incremental Extraction**: Extract one helper at a time, test after each
2. **Behavior Verification**: Run tests after each extraction step
3. **Complexity Monitoring**: Verify CYC reduction after each change
4. **Peer Review**: Arena AI adversarial audit before merge

## Jane Street Alignment

### Cognitive Simplicity Principle
- **Current State**: CYC 21 violates cognitive simplicity (hard to reason about)
- **Target State**: CYC ≤8 aligns with Jane Street HFT standards
- **Benefit**: Easier to audit for race conditions in lock-free code

### Testing Philosophy (from Jane Street KB)
- **Extracted Methods**: Pure functions easier to test exhaustively
- **Reduced Complexity**: Exponential path growth eliminated
- **Testability**: Each helper method independently verifiable

## Next Steps (Phase 2)

1. **Architectural Planning**: Generate detailed extraction plan with Mermaid diagrams
2. **Pure Function Identification**: Identify side-effect-free extraction candidates
3. **State Validation Strategy**: Design validation logic decomposition
4. **TDD Test Coverage**: Plan unit tests for extracted methods

---
**Document Version**: 1.0
**Created**: 2026-06-15
**Status**: APPROVED (pending Phase 1.5 boundary validation)
**Epic**: EPIC-CCN-027
**Phase**: 1.0 (Scope Definition)
