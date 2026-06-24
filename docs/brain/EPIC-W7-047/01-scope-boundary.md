# Phase 1: Scope Boundary - EPIC-W7-047

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.00
- **API Key**: N/A
- **Execution Time**: 2026-06-24T01:49:14Z

## Epic Objective
Reduce cyclomatic complexity of CancelOrphanedTargets from 13 to ≤8 by extracting nested conditional logic into focused helper methods.

## Target Method
- **Method**: CancelOrphanedTargets
- **File**: src/V12_002.UI.Compliance.cs
- **Line**: 553
- **Current CYC**: 13
- **Target CYC**: ≤8
- **Lines of Code**: 26

## IN SCOPE

### Primary Extraction Target
1. **CancelOrphanedTargets method body** (lines 553-579)
   - Extract nested conditional branches
   - Reduce from CYC 13 to ≤8
   - Maintain existing method signature
   - Preserve return type (int)

### Allowed Modifications
1. **Extract helper methods** within src/V12_002.UI.Compliance.cs
   - Create private helper methods for conditional logic
   - Each helper must have CYC ≤8
   - Maintain single responsibility principle
   
2. **Refactor conditional nesting**
   - Reduce nesting depth from 4 to ≤3
   - Use early returns where appropriate
   - Extract complex boolean expressions

3. **Add inline documentation**
   - Document extracted helper methods
   - Clarify business logic for orphaned target cancellation

### Callers (Must Remain Compatible)
1. **HandleFleetStopFill** (line 519)
   - Direct caller - signature must not change
   
2. **ProcessQueuedExecution_HandleFleetOCO** (line 698)
   - Indirect caller - signature must not change

### Callees (Dependencies)
1. **CancelOrderOnAccount** (src/V12_002.Orders.CancelGateway.cs:46)
   - External dependency - do not modify
   
2. **IsOrderTerminal** (src/V12_002.Orders.Management.Flatten.cs:698)
   - External dependency - do not modify

## OUT OF SCOPE

### Explicitly Excluded
1. **Caller modifications**
   - Do NOT modify HandleFleetStopFill
   - Do NOT modify ProcessQueuedExecution_HandleFleetOCO
   - Callers must continue to work without changes

2. **Callee modifications**
   - Do NOT modify CancelOrderOnAccount
   - Do NOT modify IsOrderTerminal
   - External order management methods are off-limits

3. **Other files**
   - Do NOT modify src/V12_002.Orders.CancelGateway.cs
   - Do NOT modify src/V12_002.Orders.Management.Flatten.cs
   - Do NOT modify any backup files (src-vm-backup/)

4. **Signature changes**
   - Do NOT change method name
   - Do NOT change parameter list (Account account)
   - Do NOT change return type (int)
   - Do NOT change access modifier (private)

5. **Business logic changes**
   - Do NOT alter cancellation behavior
   - Do NOT change order filtering logic
   - Do NOT modify terminal state checks
   - Refactor structure only, not semantics

6. **Cross-cutting concerns**
   - Do NOT add logging (unless already present)
   - Do NOT add error handling (unless already present)
   - Do NOT add performance optimizations
   - Focus solely on complexity reduction

## Scope Validation

### Boundary Enforcement
- **File Boundary**: Only src/V12_002.UI.Compliance.cs may be modified
- **Method Boundary**: Only CancelOrphanedTargets and new helper methods
- **Signature Boundary**: Public interface must remain unchanged
- **Behavior Boundary**: Semantic equivalence required (same inputs → same outputs)

### Success Criteria
1. CancelOrphanedTargets CYC reduced from 13 to ≤8
2. All extracted helpers have CYC ≤8
3. Nesting depth reduced from 4 to ≤3
4. Existing callers work without modification
5. Build passes (dotnet build)
6. Unit tests pass (if present)
7. Semantic equivalence verified

### Risk Mitigation
- **Low Blast Radius**: Zero external files affected
- **Isolated Scope**: Only 2 callers, both in same file
- **No API Changes**: Method signature preserved
- **Behavioral Preservation**: Logic extracted, not altered

## Extraction Strategy

### Recommended Approach
1. **Identify extraction candidates** (Phase 2)
   - Analyze nested conditionals
   - Identify cohesive logic blocks
   - Plan helper method boundaries

2. **Extract helpers** (Phase 5)
   - Create private helper methods
   - Move conditional logic to helpers
   - Reduce main method to orchestration

3. **Verify complexity** (Phase 5.V)
   - Run complexity_audit.py
   - Confirm all methods ≤8
   - Verify nesting depth ≤3

## Jane Street Alignment
- **Cognitive Simplicity**: CYC ≤8 enables microsecond-latency reasoning
- **Exhaustive Testing**: Lower complexity = exponentially fewer test paths
- **Race Condition Auditing**: Simpler logic = easier lock-free verification
- **Correctness by Construction**: Extract to make illegal states unrepresentable

## Phase 1 Completion
- Scope boundaries defined
- IN SCOPE items enumerated
- OUT OF SCOPE items explicitly excluded
- Success criteria established
- Risk mitigation documented

**Status**: SCOPE LOCKED - Ready for Phase 2 (Architecture Planning)
