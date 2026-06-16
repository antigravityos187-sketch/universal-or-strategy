# Phase 1.0: Scope Definition - EPIC-CCN-063

## Extraction Scope (SINGLE METHOD ONLY)

### Target Method
- **Method Name**: DrainAllDispatchQueuesOnAbort
- **File**: src/V12_002.SIMA.Fleet.cs
- **Current Complexity**: 11
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **Extraction Strategy**: Break into 2-3 focused helper methods

### Complexity Reduction Plan

**Current State**:
- Cyclomatic Complexity: 11
- Risk Level: LOW (below threshold 15)
- Jane Street Violations: 0

**Target State**:
- Cyclomatic Complexity: ≤8
- Maintain lock-free Actor/FSM pattern
- Zero behavior changes
- All tests pass

**Extraction Strategy**:
1. Identify logical sub-operations within DrainAllDispatchQueuesOnAbort
2. Extract 2-3 helper methods with single responsibilities
3. Each helper method should have CYC ≤5
4. Preserve exact semantics and error handling

## Boundary Definition

### IN SCOPE
- ONLY the method body of DrainAllDispatchQueuesOnAbort
- Internal logic refactoring
- Helper method extraction
- Complexity reduction from 11 to ≤8

### OUT OF SCOPE
- Callers of DrainAllDispatchQueuesOnAbort (no changes)
- Callees invoked by DrainAllDispatchQueuesOnAbort (no changes)
- Other methods in V12_002.SIMA.Fleet.cs (no changes)
- Class structure or field modifications (no changes)
- Public API surface (no changes)

### No Scope Creep
- ONE EPIC = ONE CONCERN: Single-method complexity reduction only
- No "while we're here" improvements
- No fixing pre-existing compilation errors
- No bundling multiple refactoring concerns

## Success Criteria

### Functional Requirements
1. Complexity reduced from 11 to ≤8
2. All existing tests pass (100% pass rate)
3. No behavior changes (exact semantic preservation)
4. Lock-free Actor/FSM pattern maintained

### Architectural Requirements
1. V12 DNA compliance (no locks, atomic operations only)
2. ASCII-only compliance (no Unicode/emoji)
3. Jane Street alignment (cognitive simplicity)
4. Helper methods follow single responsibility principle

### Quality Gates
1. CSharpier formatting passes
2. Build succeeds (zero errors)
3. Lint passes (zero violations)
4. Pre-push validation passes (all 13 checks)

### Verification Protocol
1. Run dotnet build - must succeed
2. Run dotnet test - must show 100% pass
3. Run python3 scripts/complexity_audit.py - verify CYC ≤8
4. Run powershell -File .\scripts\pre_push_validation.ps1 -Fast
5. Manual code review - confirm no behavior changes

## Risk Assessment

**Complexity Risk**: LOW
- Current CYC=11 is below Jane Street threshold (15)
- Refactoring is optional but improves maintainability
- Low blast radius (single method)

**Jane Street Risk**: LOW
- Zero P0 violations detected
- Method already follows lock-free patterns
- Extraction preserves existing architecture

**Overall Risk**: LOW
- Safe for refactoring
- No breaking changes expected
- Incremental improvement only

## Notes

- jCodemunch tools unavailable during Phase 0 analysis
- Manual code review required to identify helper method boundaries
- Blast radius assessment requires manual inspection of callers
- Test coverage verification needed before extraction
