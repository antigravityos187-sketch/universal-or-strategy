# Phase 1.0: Scope Definition - EPIC-CCN-013

## Extraction Scope (SINGLE METHOD ONLY)

### Target Method
- **Method Name**: UpdatePanelState
- **File**: src/V12_002.UI.Panel.StateSync.cs
- **Current Complexity**: 16 (Cyclomatic Complexity)
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **Violation**: +1 over V12 threshold (CYC ≤ 15)

### Extraction Strategy
**Approach**: Break into 2-3 helper methods

**Rationale**:
- Complexity 16 indicates ~16 decision points in a single method
- Jane Street principle: Cognitive simplicity over clever abstractions
- Target CYC ≤8 requires splitting into smaller, single-purpose functions
- UI state synchronization logic likely has distinct phases that can be isolated

**Expected Decomposition**:
1. **Helper Method 1**: State validation/precondition checks (CYC ~3-4)
2. **Helper Method 2**: Core state update logic (CYC ~4-5)
3. **Helper Method 3**: Post-update synchronization/event dispatch (CYC ~3-4)
4. **Main Method**: Orchestration only (CYC ~2-3)

**Total Target**: Main method CYC ≤8, all helpers CYC ≤8

## Boundary Definition

### IN SCOPE
- **UpdatePanelState method body ONLY**
- Extracting conditional branches into helper methods
- Refactoring internal logic flow
- Maintaining exact same external behavior
- Preserving all state mutations (atomic operations)
- Keeping lock-free Actor/FSM pattern

### OUT OF SCOPE
- **Callers**: No changes to methods that call UpdatePanelState
- **Callees**: No changes to methods called by UpdatePanelState
- **Other Methods**: No changes to other methods in V12_002.UI.Panel.StateSync.cs
- **File Structure**: No changes to class structure, fields, or properties
- **Dependencies**: No changes to imported namespaces or external dependencies
- **Tests**: No changes to existing test files (only verify they still pass)

### No Scope Creep
**ONE EPIC = ONE CONCERN**
- This epic addresses ONLY the complexity of UpdatePanelState
- No "while we're here" improvements to adjacent code
- No fixing pre-existing compilation errors in other methods
- No bundling multiple refactoring concerns
- No architectural changes beyond method extraction

## Success Criteria

### Functional Requirements
1. **Complexity Reduced**: UpdatePanelState complexity reduced from 16 to ≤8
2. **All Tests Pass**: Existing test suite passes without modification
3. **No Behavior Changes**: Exact same external behavior (input/output equivalence)
4. **Lock-Free Compliance**: Actor/FSM pattern maintained (no lock() blocks)

### Non-Functional Requirements
5. **ASCII-Only**: All string literals remain ASCII-compliant
6. **Atomic Operations**: State mutations use atomic primitives or FSM Enqueue
7. **Thread Safety**: UI thread safety preserved
8. **Performance**: No measurable performance degradation

### Quality Gates
9. **Build Success**: dotnet build completes without errors
10. **Lint Clean**: lint.ps1 passes
11. **Format Check**: dotnet csharpier check passes
12. **Complexity Audit**: complexity_audit.py shows CYC ≤8

### Verification Protocol
- **Pre-Extraction Baseline**: Capture current test results and performance metrics
- **Post-Extraction Validation**: Compare against baseline (must be identical)
- **Manual Testing**: F5 in NinjaTrader, verify UI panel state updates work correctly
- **Code Review**: Verify extracted helpers follow V12 DNA principles

## Risk Assessment

### Overall Risk: LOW
**Rationale**:
- Single method extraction (minimal blast radius)
- Complexity 16 to ≤8 is achievable with 2-3 helper methods
- UI state sync is well-understood domain
- No changes to callers/callees reduces integration risk

### Mitigation Strategies
1. **Checkpointing**: Use Bob CLI checkpointing for easy rollback
2. **Incremental Extraction**: Extract one helper at a time, verify after each
3. **Test-Driven**: Run tests after each extraction step
4. **Manual Verification**: Test UI panel updates in NinjaTrader after completion

## Metadata
- **Epic**: EPIC-CCN-013
- **Phase**: 1.0 (Scope Definition)
- **Created**: 2026-06-15
- **Status**: APPROVED (pending Phase 1.5 boundary validation)
- **Estimated Effort**: 2-4 hours
- **Priority**: Medium (exceeds threshold by +1)
