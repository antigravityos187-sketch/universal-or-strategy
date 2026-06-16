# Phase 1: Scope Definition - EPIC-CCN-016

## Target Method
- **Method**: TryHandleFleet_CancelAll
- **File**: src/V12_002.UI.IPC.Commands.Fleet.cs
- **Current Complexity**: 19
- **Target Complexity**: ≤8 (Jane Street strict standard)

## Extraction Scope (SINGLE METHOD ONLY)

### What's IN Scope
1. **Method Body**: TryHandleFleet_CancelAll implementation only
2. **Extraction Strategy**: Break into 2-3 helper methods:
   - Fleet validation logic (pre-cancellation checks)
   - Order iteration and cancellation loop
   - Error aggregation and reporting

### What's OUT of Scope
1. **Callers**: IPC command dispatcher, Fleet UI event handlers
2. **Callees**: Order cancellation primitives, state validation checks
3. **Other Methods**: No changes to other methods in V12_002.UI.IPC.Commands.Fleet.cs
4. **Pre-existing Issues**: No fixing compilation errors outside this method

## Boundary Definition

### ONE EPIC = ONE CONCERN
- ✅ Focus: TryHandleFleet_CancelAll complexity reduction only
- ❌ No scope creep: No "while we're here" improvements
- ❌ No bundling: No mixing multiple concerns in one PR

## Success Criteria

### Complexity Reduction
- **Before**: CYC 19
- **After**: CYC ≤8 per method (including extracted helpers)
- **Verification**: python scripts/complexity_audit.py --threshold 8

### Behavioral Preservation
- ✅ All existing tests pass
- ✅ No behavior changes
- ✅ Fleet cancellation logic remains identical

### V12 DNA Compliance
- ✅ Lock-free Actor/FSM pattern maintained
- ✅ ASCII-only compliance (no Unicode)
- ✅ Correctness by construction (invalid states unrepresentable)
- ✅ Atomic operations for fleet state changes

### Testing Requirements
- ✅ Unit tests for each extracted helper method
- ✅ Integration test: F5 in NinjaTrader IDE
- ✅ Verification: BUILD_TAG appears in output

## Extraction Strategy

### Phase 2 Planning (Next Step)
1. **Analyze Method Structure**: Identify decision paths and loops
2. **Design Helper Methods**: 
   - ValidateFleetForCancellation() - CYC ≤8
   - CancelAllFleetOrders() - CYC ≤8
   - AggregateAndReportErrors() - CYC ≤8
3. **Maintain Atomicity**: Ensure state changes remain atomic
4. **Preserve Error Handling**: Keep error reporting intact

### Jane Street Alignment
- **Cognitive Simplicity**: Each method does ONE thing
- **Testability**: Small methods = exhaustive test coverage
- **Microsecond Reasoning**: Simple logic = fast comprehension
- **Race Condition Auditing**: Fewer branches = easier verification

## Risk Mitigation

### High-Risk Areas
1. **Fleet State Management**: Ensure atomic updates
2. **Order Cancellation Pipeline**: Preserve transaction semantics
3. **Error Handling**: Maintain error aggregation logic
4. **IPC Command Routing**: Keep message handling intact

### Safeguards
1. **Characterization Tests**: Write tests for current behavior BEFORE extraction
2. **Incremental Extraction**: Extract one helper at a time
3. **Build Verification**: Run dotnet build after each extraction
4. **Hard Link Sync**: Run deploy-sync.ps1 after all changes

## Approval Gate

**Status**: PENDING Phase 1.5 Boundary Validation

**Next Step**: Create 01-scope-boundary.md for V12.23 Protocol compliance check

---
**Created**: 2026-06-15
**Epic**: EPIC-CCN-016
**Phase**: 1.0 (Scope Definition)
**Complexity Target**: CYC 19 → ≤8