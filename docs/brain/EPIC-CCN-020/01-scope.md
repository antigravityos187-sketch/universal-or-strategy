# Phase 1.0: Scope Definition - EPIC-CCN-020

## Target Method
- Method: HandleSecondaryOrderFilled
- File: src/V12_002.Orders.Callbacks.cs
- Current Complexity: 21
- V12 Threshold: 15
- Excess: +6 (40%% over threshold)

## Extraction Scope (SINGLE METHOD ONLY)

### Primary Target
Method: HandleSecondaryOrderFilled(Order order, Execution execution, string executionId, int quantity, double price, DateTime time)

### Complexity Reduction Strategy
- Current Complexity: 21
- Target Complexity: 8 or less per method (Jane Street strict standard)
- Extraction Approach: Break into 2-4 focused helper methods

### Proposed Extraction Pattern
Based on the complexity breakdown from Phase 0, extract:

1. Validation Logic (Pure Function)
   - Extract order state validation checks
   - Extract execution type validation
   - Target: CYC 5 or less
   - Pattern: Pure function, no side effects

2. State Transition Logic (FSM/Actor)
   - Extract position management logic
   - Extract state mutation operations
   - Target: CYC 5 or less
   - Pattern: Maintain lock-free Actor/FSM pattern

3. Error Handling Logic (Pure Function)
   - Extract error path handling
   - Extract recovery logic
   - Target: CYC 5 or less
   - Pattern: Separate from happy path

4. Main Orchestration (Coordinator)
   - Coordinate extracted methods
   - Maintain atomic state transitions
   - Target: CYC 8 or less
   - Pattern: Thin orchestration layer

## Boundary Definition

### IN SCOPE
- HandleSecondaryOrderFilled method body ONLY
- Extracting helper methods within same class
- Adding private helper methods for validation/state/error logic
- Maintaining existing method signature
- Preserving all existing behavior
- Adding TDD tests for extracted methods

### OUT OF SCOPE
- Callers: No changes to methods that call HandleSecondaryOrderFilled
- Callees: No changes to methods called by HandleSecondaryOrderFilled
- Other Methods: No changes to other methods in V12_002.Orders.Callbacks.cs
- File Structure: No changes to class structure or namespace
- Dependencies: No changes to external dependencies
- Pre-existing Issues: No fixing compilation errors outside target method

### Scope Creep Prevention
ONE EPIC = ONE CONCERN: This epic ONLY reduces complexity of HandleSecondaryOrderFilled.

## Success Criteria

### Functional Requirements
1. Complexity Reduction: HandleSecondaryOrderFilled reduced from CYC=21 to CYC 8 or less
2. Behavior Preservation: Zero behavior changes (verified by tests)
3. Test Coverage: 100%% TDD coverage for all extracted methods
4. Build Success: Zero compilation errors
5. Test Pass: All existing tests pass

### Architectural Requirements
1. Lock-Free Pattern: Maintain Actor/FSM pattern (no lock statements)
2. ASCII-Only: No Unicode characters in string literals
3. Atomic Transitions: All state changes remain atomic
4. Pure Functions: Validation logic extracted as pure functions
5. Single Responsibility: Each extracted method has one clear purpose

## Jane Street Alignment

### Cognitive Simplicity
- Principle: Make illegal states unrepresentable
- Application: Extract validation to make invalid states impossible to reach
- Target: Functions simple enough to reason about under microsecond latency

### Testing Philosophy
From Jane Street KB: Test extracted methods independently, focus on edge cases and state transitions.

## Risk Assessment
Risk Level: MEDIUM-HIGH
Rationale: Order callback methods are critical hot-path code with state mutation

---
Scope Defined: 2026-06-15T03:32:00Z
Status: READY FOR PHASE 1.5 (BOUNDARY VALIDATION)
