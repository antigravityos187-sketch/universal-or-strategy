# Phase 1.0: Scope Definition - EPIC-CCN-078

## Extraction Scope (SINGLE METHOD ONLY)

### Target Method
- **Method Name**: StopIpcServer
- **File**: src/V12_002.UI.IPC.Server.cs
- **Current Complexity**: 12
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **Extraction Strategy**: Break into 2-3 helper methods

### Complexity Reduction Plan

**Current State**:
- Cyclomatic Complexity: 12
- Status: Below V12 threshold (15) but above Jane Street strict standard (8)
- Risk Level: LOW

**Target State**:
- Cyclomatic Complexity: ≤8
- Extraction Method: Decompose into focused helper methods
- Estimated Helpers: 2-3 methods

### Boundary Definition

**IN SCOPE**:
- StopIpcServer method body only
- Internal logic decomposition
- Helper method extraction
- Complexity reduction to ≤8

**OUT OF SCOPE**:
- Callers of StopIpcServer
- Callees invoked by StopIpcServer
- Other methods in V12_002.UI.IPC.Server.cs
- IPC server lifecycle management beyond this method
- Pre-existing compilation errors
- While we are here improvements

### No Scope Creep Mandate

**ONE EPIC = ONE CONCERN**:
- This EPIC addresses ONLY StopIpcServer complexity
- No bundling of multiple refactoring concerns
- No fixing unrelated issues
- No architectural changes beyond method extraction

### Success Criteria

**Functional Requirements**:
- All existing tests pass
- No behavior changes
- Lock-free Actor/FSM pattern maintained
- ASCII-only compliance preserved

**Quality Requirements**:
- Complexity reduced from 12 to ≤8
- Helper methods have clear, single responsibilities
- No new Jane Street violations introduced
- Code remains idiomatic C#

**Process Requirements**:
- Pre-push validation passes (all 13 checks)
- CSharpier formatting applied
- No whitespace mutation in unrelated files
- PR diff <10k characters

### Extraction Strategy

**Approach**:
1. Identify logical blocks within StopIpcServer
2. Extract 2-3 helper methods with clear names
3. Maintain original control flow
4. Preserve error handling patterns

**Helper Method Candidates** (to be determined in Phase 2):
- Server cleanup logic
- Resource disposal sequence
- State transition handling

### Risk Mitigation

**Low Risk Factors**:
- Method already below V12 threshold (15)
- No Jane Street P0 violations detected
- Isolated scope (single method)

**Mitigation Strategy**:
- Incremental extraction (one helper at a time)
- Test after each extraction
- Checkpoint before each change

### Jane Street Alignment

**Cognitive Simplicity**:
- Target CYC ≤8 aligns with Jane Street HFT standards
- Simple, verifiable logic for microsecond-latency constraints
- Easier to audit for race conditions in lock-free code

**Correctness by Construction**:
- Helper methods will have clear preconditions
- Single responsibility per extracted method
- Make illegal states unrepresentable

## Phase 1.0 Status

- **Status**: COMPLETE
- **Next Phase**: Phase 1.5 (Boundary Validation)
- **Approval Required**: Director sign-off on scope boundaries
