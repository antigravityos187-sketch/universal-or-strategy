# Phase 1.0: Scope Definition - EPIC-CCN-072

## Extraction Scope (SINGLE METHOD ONLY)

**Target Method**:
- **Method Name**: ProcessBracketEvent
- **File**: src/V12_002.Symmetry.BracketFSM.cs
- **Current Complexity**: 14 (Cyclomatic Complexity)
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **Extraction Strategy**: Break into 2-3 helper methods

## Boundary Definition

### IN SCOPE
- **ProcessBracketEvent method body only**
- Extract conditional logic into helper methods
- Reduce cyclomatic complexity from 14 to ≤8
- Maintain FSM semantics and atomic state transitions
- Preserve lock-free Actor/FSM pattern

### OUT OF SCOPE
- **Callers**: No changes to methods that call ProcessBracketEvent
- **Callees**: No changes to methods called by ProcessBracketEvent
- **Other Methods**: No changes to other methods in V12_002.Symmetry.BracketFSM.cs
- **File Structure**: No changes to class structure or namespace
- **Pre-existing Issues**: No fixing unrelated compilation errors
- **Scope Creep**: No "while we're here" improvements

### No Scope Creep: ONE EPIC = ONE CONCERN
This EPIC focuses exclusively on reducing the cyclomatic complexity of ProcessBracketEvent. Any other improvements, refactorings, or fixes are OUT OF SCOPE and must be tracked in separate EPICs.

## Success Criteria

### Functional Requirements
1. **Complexity Reduction**: Cyclomatic complexity reduced from 14 to ≤8
2. **Test Coverage**: All existing tests pass (100% pass rate)
3. **Behavior Preservation**: No behavior changes (pure refactoring)
4. **Lock-Free Pattern**: Actor/FSM pattern maintained (no locks introduced)

### Technical Requirements
1. **ASCII-Only**: No Unicode, emoji, or curly quotes in string literals
2. **Build Success**: Zero compilation errors
3. **Lint Clean**: Zero Roslyn violations
4. **Format Clean**: CSharpier formatting passes

### Quality Gates
1. **Pre-Push Validation**: All 13 checks pass (or warnings only)
2. **Codacy**: No new issues introduced
3. **CodeRabbit**: No critical/high findings (WARNING mode)
4. **Hard-Link Sync**: deploy-sync.ps1 completes successfully

## Extraction Strategy

### Approach: Conditional Logic Decomposition
Break down complex conditional branches into focused helper methods:

1. **Helper Method 1**: Extract bracket state validation logic
2. **Helper Method 2**: Extract bracket event processing logic
3. **Helper Method 3**: Extract state transition logic (if needed)

### Constraints
- Each helper method must have CYC ≤5
- Helper methods must be private and co-located with ProcessBracketEvent
- No changes to method signature or public API
- Preserve exact FSM state transition semantics

## Risk Assessment

**Complexity Risk**: MEDIUM
- Current complexity (14) is 1 point below threshold (15)
- Proactive refactoring prevents future threshold breach

**Blast Radius Risk**: MEDIUM
- FSM state logic affects bracket tracking
- State transition correctness is critical for order management

**Mitigation**:
- Comprehensive test coverage before extraction
- Incremental extraction with verification after each step
- Mandatory checkpointing enabled via Bob CLI

## Jane Street Alignment

**Cognitive Simplicity**: Functions with CYC >15 are harder to:
- Reason about under microsecond latency constraints
- Test exhaustively (exponential path growth)
- Audit for race conditions in lock-free code

**V12 DNA Mandate**: "Make illegal states unrepresentable"
- Requires simple, verifiable logic
- Target CYC ≤8 ensures cognitive simplicity
- Aligns with Jane Street HFT system principles

## Approval Status

**Status**: PENDING (awaiting Phase 1.5 Boundary Validation)
**Next Phase**: Phase 1.5 - Boundary Validation (V12.23 Protocol)
