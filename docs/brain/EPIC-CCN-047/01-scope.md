# Phase 1.0: Scope Definition - EPIC-CCN-047

## Extraction Scope (SINGLE METHOD ONLY)

### Target Method
- Method Name: CancelOrphanedTargets
- File: src/V12_002.UI.Compliance.cs
- Current Complexity: 14
- Target Complexity: <=8 (Jane Street strict standard)
- Extraction Strategy: Break into 2-3 helper methods

### Complexity Reduction Plan

Current State:
- Cyclomatic Complexity: 14 (93% of threshold 15)
- Status: WARNING - Approaching complexity threshold
- Risk Level: MEDIUM (private method, critical target lifecycle logic)

Target State:
- Main method complexity: <=8 (orchestration only)
- Helper method complexity: <=5 each
- Total methods: 1 main + 2-3 helpers

### Extraction Strategy

Proposed Helper Methods (based on hotspot analysis):

1. IsTargetOrphaned (CYC <=5)
   - Purpose: Orphan detection criteria
   - Logic: Validate target state and determine if orphaned
   - Returns: bool

2. ExecuteTargetCancellation (CYC <=5)
   - Purpose: Cancellation execution
   - Logic: Perform actual cancellation operations
   - Returns: void or bool (success indicator)

3. CleanupCancelledTarget (CYC <=5)
   - Purpose: Post-cancellation state cleanup
   - Logic: Update state, log, notify
   - Returns: void

Main Method (CYC <=8):
- Orchestrates the 3 helper methods
- Iterates over targets
- Delegates complexity to helpers

## Boundary Definition

### Whats IN Scope
- ONLY the CancelOrphanedTargets method body
- Extracting helper methods from existing logic
- Maintaining exact same behavior
- Preserving lock-free Actor/FSM pattern
- Adding XML documentation to extracted methods

### Whats OUT of Scope
- Callers of CancelOrphanedTargets
- Callees (methods called by CancelOrphanedTargets)
- Other methods in V12_002.UI.Compliance.cs
- Fixing pre-existing compilation errors
- Refactoring adjacent code
- Performance optimizations beyond complexity reduction
- Changing method signatures or access modifiers

### No Scope Creep Rule
ONE EPIC = ONE CONCERN
- This epic ONLY reduces complexity of CancelOrphanedTargets
- No while we are here improvements
- No bundling multiple concerns
- No fixing unrelated issues

## Success Criteria

### Functional Requirements
- All existing tests pass (no behavior changes)
- Method behavior identical to original
- No new compilation errors introduced
- Lock-free Actor/FSM pattern maintained

### Complexity Requirements
- Main method complexity: <=8
- Each helper method complexity: <=5
- Total complexity reduction: 14 to <=8 (main method)
- Codacy complexity check passes

### Code Quality Requirements
- ASCII-only compliance (no Unicode)
- CSharpier formatting passes
- XML documentation for all extracted methods
- Consistent naming conventions
- No dead code introduced

### Testing Requirements
- Existing unit tests pass
- No new test failures
- Manual F5 test in NinjaTrader (if applicable)
- Build verification: dotnet build succeeds

### V12 DNA Compliance
- No lock() statements introduced
- Atomic operations preserved
- State mutations use FSM/Actor Enqueue model
- Make illegal states unrepresentable principle maintained

## Risk Assessment

### Overall Risk: LOW-MEDIUM
- Blast Radius: Small (private method, single file)
- Complexity: Medium (critical target lifecycle logic)
- Testing: Medium (requires state validation)
- Rollback: Easy (single method, checkpointing enabled)

### Mitigation Strategies
1. Checkpointing: Bob CLI auto-checkpoint before changes
2. Incremental Extraction: Extract one helper at a time
3. Test After Each Step: Run tests after each extraction
4. Manual Verification: F5 in NinjaTrader after completion

## Approval Gate

Status: PENDING PHASE 1.5 BOUNDARY VALIDATION

Next Step: Create 01-scope-boundary.md for V12.23 Protocol compliance

---
Document Version: 1.0
Created: 2026-06-15
Epic: EPIC-CCN-047
Protocol: V12.23 (Phase 1.0)
