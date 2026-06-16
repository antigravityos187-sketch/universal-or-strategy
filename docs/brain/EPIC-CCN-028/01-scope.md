# Phase 1.0: Scope Definition - EPIC-CCN-028

## Extraction Scope (SINGLE METHOD ONLY)

### Target Method
- **Method Name**: `ProcessFlattenWorkItem_CancelOrders`
- **File**: `src/V12_002.SIMA.Flatten.cs`
- **Current Complexity**: 18 (Cyclomatic Complexity)
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **Extraction Strategy**: Break into 2-3 helper methods

### Complexity Reduction Plan

**Current State**: CYC = 18 (exceeds V12 threshold of 15)

**Target State**: CYC ≤ 8 (Jane Street alignment for cognitive simplicity)

**Extraction Strategy**:
1. **Extract Order Validation Logic** (estimated CYC reduction: 4-6)
   - Separate pre-cancellation validation checks
   - Consolidate order state verification
   - Return early on validation failures

2. **Extract Error Handling Paths** (estimated CYC reduction: 3-4)
   - Consolidate error logging and state transitions
   - Create dedicated error handler method
   - Reduce branching in main method

3. **Extract State Transition Logic** (estimated CYC reduction: 2-3)
   - Move FSM state updates to dedicated method
   - Ensure atomic state transitions
   - Maintain lock-free Actor/FSM pattern

**Expected Result**: Main method CYC ≤ 8, with 2-3 extracted helper methods each with CYC ≤ 5

## Boundary Definition

### What's IN Scope
- ✅ **Method Body Only**: `ProcessFlattenWorkItem_CancelOrders` implementation
- ✅ **Internal Logic**: Decision trees, error handling, state transitions within this method
- ✅ **Helper Method Creation**: New private methods extracted from this method's body
- ✅ **Complexity Reduction**: Refactoring to achieve CYC ≤ 8

### What's OUT of Scope
- ❌ **Callers**: No changes to methods that call `ProcessFlattenWorkItem_CancelOrders`
- ❌ **Callees**: No changes to methods called by `ProcessFlattenWorkItem_CancelOrders`
- ❌ **Other Methods**: No changes to other methods in `V12_002.SIMA.Flatten.cs`
- ❌ **File Structure**: No changes to class structure, namespaces, or imports
- ❌ **Pre-existing Issues**: No fixing of compilation errors outside this method
- ❌ **Scope Creep**: No "while we're here" improvements to adjacent code

### No Scope Creep Rule
**ONE EPIC = ONE CONCERN**

This epic addresses ONLY the complexity of `ProcessFlattenWorkItem_CancelOrders`. Any other concerns discovered during analysis must be logged as separate epics.

## Success Criteria

### Functional Requirements
1. ✅ **Complexity Reduced**: Method CYC reduced from 18 to ≤8
2. ✅ **All Tests Pass**: Existing test suite passes without modification
3. ✅ **No Behavior Changes**: Refactoring is purely structural (no logic changes)
4. ✅ **Lock-Free Pattern Maintained**: No introduction of `lock()` statements

### V12 DNA Compliance
1. ✅ **ASCII-Only**: No Unicode characters in string literals
2. ✅ **Atomic Operations**: State mutations use FSM/Actor `Enqueue` pattern
3. ✅ **Correctness by Construction**: Type-safe state transitions
4. ✅ **Cognitive Simplicity**: Each extracted method has single, clear purpose

### Quality Gates
1. ✅ **Build Success**: `dotnet build` completes without errors
2. ✅ **Lint Clean**: `powershell -File .\scripts\lint.ps1` passes
3. ✅ **Format Check**: `dotnet csharpier check src/` passes
4. ✅ **Complexity Audit**: `python scripts/complexity_audit.py` shows CYC ≤ 8

### Documentation Requirements
1. ✅ **Inline Comments**: Extracted methods have clear purpose documentation
2. ✅ **Commit Message**: Follows Conventional Commits specification
3. ✅ **PR Description**: Links to this scope document

## Risk Mitigation

### Risk Level: MEDIUM-HIGH
**Rationale**: Order cancellation is a critical trading operation with complex state management.

### Mitigation Strategies
1. **Incremental Extraction**: Extract one helper method at a time, verify tests after each
2. **Checkpoint Restoration**: Use Bob CLI checkpointing to rollback if needed
3. **Manual Testing**: F5 in NinjaTrader after deployment to verify runtime behavior
4. **Arena AI Review**: Submit implementation plan for adversarial audit before execution

## Metadata
- **Epic ID**: EPIC-CCN-028
- **Phase**: 1.0 (Scope Definition)
- **Status**: Completed
- **Date**: 2026-06-15
- **Complexity Target**: CYC ≤ 8 (Jane Street strict standard)
- **Extraction Count**: 2-3 helper methods
