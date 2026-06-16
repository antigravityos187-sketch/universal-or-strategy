# Phase 1.0: Scope Definition - EPIC-CCN-015

## Extraction Scope (SINGLE METHOD ONLY)

### Target Method
- Method Name: CancelAll_ProcessSingleFleetAccount
- File: src/V12_002.UI.IPC.Commands.Fleet.cs
- Current Complexity: 18 CYC
- Target Complexity: 8 CYC (Jane Street strict standard)
- Overage: +3 CYC (20% over V12 threshold of 15)

### Extraction Strategy
Break the method into 2-3 focused helper methods:

1. Validation Helper (~3-4 CYC)
   - Extract account validation logic
   - Extract pre-condition checks
   - Return early on invalid states

2. Cancellation Logic Helper (~3-4 CYC)
   - Extract core cancellation workflow
   - Isolate order iteration and cancellation calls
   - Handle cancellation state transitions

3. Error Handling Helper (~2-3 CYC) [if needed]
   - Extract error path logic
   - Consolidate exception handling
   - Ensure atomic error recovery

Target: Main method reduced to 8 CYC (orchestration only)

## Boundary Definition

### IN SCOPE
- ONLY the method body of CancelAll_ProcessSingleFleetAccount
- Internal logic refactoring (extract to private helpers)
- Complexity reduction from 18 to 8 CYC
- Preserving exact behavior and semantics
- Maintaining lock-free Actor/FSM pattern

### OUT OF SCOPE
- NO changes to callers of CancelAll_ProcessSingleFleetAccount
- NO changes to callees (methods this method calls)
- NO changes to other methods in V12_002.UI.IPC.Commands.Fleet.cs
- NO changes to method signature or visibility
- NO changes to class structure or dependencies
- NO while we are here improvements
- NO fixing pre-existing compilation errors outside this method

### Scope Creep Prevention
ONE EPIC = ONE CONCERN: This epic ONLY reduces complexity of CancelAll_ProcessSingleFleetAccount. Any other improvements require separate epics.

## Success Criteria

### Functional Requirements
1. Complexity Reduced: Method complexity drops from 18 to 8 CYC
2. Behavior Preserved: Zero functional changes (pure refactoring)
3. Tests Pass: All existing tests pass without modification
4. Lock-Free: No lock() statements introduced (Actor/FSM pattern maintained)

### Non-Functional Requirements
1. ASCII-Only: No Unicode, emoji, or curly quotes in code
2. Atomic Operations: State transitions remain atomic
3. Performance: No measurable latency regression
4. Readability: Extracted methods have clear, single-purpose names

### Verification Checklist
- Complexity audit shows CYC 8 or less for main method
- Complexity audit shows CYC 8 or less for all extracted helpers
- dotnet build succeeds with zero errors
- dotnet test passes 100%
- grep lock check returns zero matches in target file
- CSharpier formatting check passes
- Pre-push validation passes (all 13 checks)

## Risk Assessment

### Overall Risk: LOW
Justification:
- Private method (limited blast radius)
- Single-method scope (no cascading changes)
- Focused refactoring (complexity reduction only)
- Existing tests provide safety net

### Mitigation Strategy
1. Checkpointing: Bob CLI auto-checkpoint before each change
2. Incremental Extraction: Extract one helper at a time, verify after each
3. Test-Driven: Run tests after each extraction step
4. Rollback Ready: Use /restore if any step fails

## Jane Street Alignment

### Cognitive Simplicity Principle
- Current State: CYC 18 - hard to reason about under microsecond constraints
- Target State: CYC 8 - simple, verifiable logic paths
- Rationale: Make illegal states unrepresentable requires simple functions

### Testing Guidance (from Jane Street KB)
- Focus on property-based testing for extracted helpers
- Ensure each helper has single, testable responsibility
- Verify state transitions are atomic and deterministic

## Implementation Notes

### Method Context
- Domain: Fleet Management (IPC Commands)
- Operation: Cancel all orders for a single fleet account
- Criticality: HIGH (trading operations)
- Pattern: Likely iterates over orders, calls cancellation primitives

### Extraction Principles
1. Single Responsibility: Each helper does ONE thing
2. No Side Effects: Helpers should be pure where possible
3. Clear Naming: Method names describe exact behavior
4. Minimal Parameters: Pass only what is needed

### V12 DNA Compliance
- No locks (Actor/FSM pattern)
- ASCII-only strings
- Atomic state transitions
- Complexity 15 or less (targeting 8)

---

Epic: EPIC-CCN-015
Phase: 1.0 (Scope Definition)
Status: APPROVED (pending Phase 1.5 boundary validation)
Date: 2026-06-15
Next Phase: 1.5 (Boundary Validation)
