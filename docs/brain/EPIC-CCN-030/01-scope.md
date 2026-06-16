# Phase 1.0: Scope Definition - EPIC-CCN-030

## Extraction Scope (SINGLE METHOD ONLY)

**Target Method**: ValidateOrphanedMasterOrders
**File**: src/V12_002.Orders.Management.Cleanup.cs
**Current Complexity**: 19 (Cyclomatic Complexity)
**Target Complexity**: ≤8 (Jane Street strict standard)

### Extraction Strategy

Break ValidateOrphanedMasterOrders into 2-3 focused helper methods:

1. **Primary Method** (CYC ≤5): Orchestration logic only
   - Iterate over master orders
   - Delegate validation to helper methods
   - Aggregate results

2. **Helper Method 1** (CYC ≤5): Order State Validation
   - Check order state consistency
   - Validate state machine transitions
   - Return validation result

3. **Helper Method 2** (CYC ≤5): Orphaned Order Detection
   - Detect orphaned master orders
   - Check parent-child relationships
   - Return detection result

4. **Helper Method 3** (CYC ≤3): Error Handling & Logging
   - Centralize error reporting
   - Log validation failures
   - Maintain audit trail

### Complexity Reduction Plan

**Current**: 19 branches across multiple concerns
**Target**: 4 methods, each ≤8 branches

**Breakdown**:
- Main orchestration: CYC 5 (iteration + delegation)
- State validation: CYC 5 (state checks)
- Orphan detection: CYC 5 (relationship checks)
- Error handling: CYC 3 (logging branches)

**Total**: 18 branches distributed across 4 methods (vs 19 in single method)

## Boundary Definition

### IN SCOPE ✅

**ONLY** the method body of ValidateOrphanedMasterOrders:
- Method signature remains unchanged
- Internal logic extraction only
- Helper methods added to same class
- No changes to method contract

### OUT OF SCOPE ❌

**Explicitly EXCLUDED**:
- Callers of ValidateOrphanedMasterOrders (no changes)
- Callees invoked by ValidateOrphanedMasterOrders (no changes)
- Other methods in V12_002.Orders.Management.Cleanup.cs (no changes)
- Order state management infrastructure (no changes)
- Master order collections (no changes)
- Logging infrastructure (no changes)

### No Scope Creep Rule

**ONE EPIC = ONE CONCERN**

This EPIC addresses ONLY:
- Complexity reduction of ValidateOrphanedMasterOrders
- Extraction into helper methods
- Maintaining identical behavior

This EPIC does NOT address:
- Pre-existing compilation errors
- Performance optimization
- Feature additions
- Refactoring other methods
- Infrastructure changes

## Success Criteria

### Functional Requirements

1. **Complexity Reduction**: All methods CYC ≤8
   - Main method: CYC ≤5
   - Helper methods: CYC ≤5 each
   - Total complexity budget: ≤18

2. **Behavior Preservation**: Zero behavior changes
   - All existing tests pass
   - No new test failures
   - Identical output for all inputs

3. **Lock-Free Correctness**: Actor/FSM pattern maintained
   - No new lock() statements
   - Atomic state transitions preserved
   - Race condition safety verified

4. **Test Coverage**: Comprehensive unit tests
   - Test each extracted method independently
   - Cover all branches (100% path coverage)
   - Verify edge cases

### Non-Functional Requirements

1. **ASCII-Only Compliance**: No Unicode characters
2. **Code Style**: CSharpier formatting enforced
3. **Documentation**: XML comments for all new methods
4. **Performance**: No latency regression

### Verification Gates

**Pre-Implementation**:
- Phase 1.5 boundary validation approved
- Jane Street KB consulted for patterns
- Implementation plan reviewed

**Post-Implementation**:
- dotnet build succeeds (zero errors)
- dotnet test passes (100% pass rate)
- complexity_audit.py confirms CYC ≤8
- deploy-sync.ps1 completes successfully

## Jane Street Alignment

**Cognitive Simplicity Principle**:
- Functions with CYC >15 are harder to reason about under microsecond latency constraints
- Single-purpose functions with clear contracts
- Make illegal states unrepresentable through type design

**Testing Standard** (from will_wilson_why_testing_hard_2026):
- Exhaustive test coverage for all paths
- Test each extracted method independently
- Verify lock-free correctness through tests

**Refactoring Approach**:
- Extract methods with TDD approach
- Verify behavior preservation at each step
- Maintain lock-free Actor/FSM pattern

---

**Phase**: 1.0 (Scope Definition)
**Status**: DEFINED
**Next Phase**: 1.5 (Boundary Validation)
**Date**: 2026-06-15
