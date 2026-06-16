# Phase 2: Architecture Planning - EPIC-CCN-020

## Target Method Analysis

### Current State
- **Method**: HandleSecondaryOrderFilled
- **File**: src/V12_002.Orders.Callbacks.cs
- **Current Complexity**: 21 (CYC)
- **Lines of Code**: 69
- **Tier**: 1 (High Priority)
- **Signature**: HandleSecondaryOrderFilled(Order order, Execution execution, string executionId, int quantity, double price, DateTime time)

### Target State
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **Strategy**: Extract 3 focused helper methods
- **Expected Complexity Distribution**:
  - Main method: CYC ≤8 (orchestration only)
  - Helper 1: CYC ≤5 (validation)
  - Helper 2: CYC ≤6 (position updates)
  - Helper 3: CYC ≤5 (state transitions)

## Extraction Strategy

### Responsibility Separation

The current monolithic method handles multiple concerns that should be separated:

1. **Validation Logic** → Extract to ValidateSecondaryOrderExecution
   - Validates order state consistency
   - Validates execution parameters
   - Checks quantity and price bounds
   - Pure function with no side effects

2. **Position & PnL Updates** → Extract to UpdatePositionAndPnL
   - Updates position quantities
   - Calculates realized/unrealized PnL
   - Enqueues atomic position updates
   - Uses Actor pattern for state mutations

3. **State Transition Logic** → Extract to TransitionOrderState
   - Manages FSM state transitions
   - Updates order status
   - Enqueues state change events
   - Uses Actor/FSM Enqueue pattern

4. **Orchestration** → Remains in HandleSecondaryOrderFilled
   - Calls helpers in sequence
   - Handles early returns on validation failure
   - Maintains public API contract
   - Reduced to simple control flow (CYC ≤8)

## Proposed Method Signatures

### Original Method (Preserved)
- Signature: private void HandleSecondaryOrderFilled(Order order, Execution execution, string executionId, int quantity, double price, DateTime time)
- Status: Signature UNCHANGED (API preservation requirement)

### Helper Method 1: Validation
- Signature: private bool ValidateSecondaryOrderExecution(Order order, Execution execution, int quantity, double price)
- Purpose: Validates order and execution parameters
- Returns: true if validation passes, false otherwise
- Complexity: CYC ≤5
- Characteristics: Pure function (no side effects), Early return on first validation failure, No state mutations, Easily testable in isolation

### Helper Method 2: Position Updates
- Signature: private void UpdatePositionAndPnL(Order order, int quantity, double price, DateTime time)
- Purpose: Updates position state and calculates PnL
- Returns: void (enqueues atomic updates)
- Complexity: CYC ≤6
- Characteristics: Uses Actor Enqueue pattern for state mutations, Atomic position updates, No direct state mutation (lock-free), Calculates realized/unrealized PnL

### Helper Method 3: State Transitions
- Signature: private void TransitionOrderState(Order order, string executionId, DateTime time)
- Purpose: Manages FSM state transitions for order lifecycle
- Returns: void (enqueues state change events)
- Complexity: CYC ≤5
- Characteristics: Uses FSM/Actor Enqueue pattern, No lock() statements, Atomic state transitions, Event-driven state changes

## Call Graph & Data Flow

### Orchestration Flow
HandleSecondaryOrderFilled (CYC ≤8) calls:
1. ValidateSecondaryOrderExecution (CYC ≤5) - Returns bool (validation result)
   - If false: Early return (log error)
   - If true: Continue to next step
2. UpdatePositionAndPnL (CYC ≤6) - Enqueues atomic position updates (fire-and-forget)
3. TransitionOrderState (CYC ≤5) - Enqueues FSM state transition (fire-and-forget)

### Data Flow
Input Parameters: order, execution, executionId, quantity, price, time
- ValidateSecondaryOrderExecution(order, execution, quantity, price) → Validation Result (bool)
- UpdatePositionAndPnL(order, quantity, price, time) → Enqueue(PositionUpdate) → Actor Queue
- TransitionOrderState(order, executionId, time) → Enqueue(StateTransition) → FSM Queue

### Shared State Analysis
- **No shared mutable state between helpers**
- Each helper operates on: Passed parameters (immutable during call), Enqueued atomic updates (Actor pattern), No direct field mutations
- **Lock-free guarantee**: All state changes via Actor/FSM queues

## Lock-Free Validation

### Compliance Checklist
- ✅ No lock() statements: All helpers use Actor/FSM Enqueue pattern
- ✅ Atomic primitives only: Position updates via atomic enqueue operations
- ✅ FSM/Actor pattern: State transitions through FSM queue
- ✅ Pure validation: ValidateSecondaryOrderExecution has no side effects
- ✅ No direct state mutation: All mutations via Actor pattern

### Lock-Free Patterns Used
1. Actor Model: Position and state updates enqueued to Actor mailbox
2. FSM Pattern: State transitions via FSM Enqueue (no direct mutation)
3. Pure Functions: Validation logic has no side effects
4. Atomic Operations: All state changes are atomic via queue processing

### Forensic Scan Requirement
After implementation, verify zero lock() statements: grep -r "lock(" src/V12_002.Orders.Callbacks.cs
Expected Result: Zero matches (lock-free requirement)

## Jane Street Compliance

### Cognitive Simplicity Alignment
- Current: Single method with CYC=21 (high cognitive load)
- Target: 4 methods with CYC ≤8 each (low cognitive load per method)
- Benefit: Each method is simple enough to reason about under microsecond latency constraints

### Jane Street Principles Applied

#### 1. Cognitive Load Reduction
- Before: 69 LOC monolithic method with 21 decision points
- After: 4 focused methods, each with single responsibility
- Impact: Easier to understand, audit, and optimize for HFT latency

#### 2. Testability (from Jane Street Testing Intel)
- Before: Monolithic method, hard to test edge cases in isolation
- After: Pure validation function + atomic state updates
- Impact: TDD coverage for validation, position, and state logic independently

#### 3. Concurrency Coordination (from Jane Street Concurrency Intel)
- Before: Potential lock contention in monolithic method
- After: Lock-free Actor pattern, zero coordination overhead
- Impact: Predictable microsecond-latency performance

#### 4. Correctness by Construction
- Validation: Pure function returns bool (impossible to have invalid state)
- State Transitions: FSM pattern makes illegal states unrepresentable
- Position Updates: Atomic enqueue ensures consistency

### Jane Street KB Query Results
- Query: "testing" → Found: "Why Testing Is Hard and How to Fix It"
- Relevance: Emphasizes testability of extracted pure functions
- Application: ValidateSecondaryOrderExecution is a pure function, easily testable

## Implementation Constraints

### V12.23 Boundary Compliance
- ✅ Single method extraction (no scope creep)
- ✅ No changes to callers or callees
- ✅ No changes to other methods in same file
- ✅ Helper methods are private implementation details
- ✅ Public API signature preserved
- ✅ Minimal blast radius (1 method, 1 file, 1 class)

### Code Quality Requirements
- Complexity: Each method CYC ≤8 (verified by complexity_audit.py)
- Formatting: CSharpier auto-format (adds missing braces)
- ASCII-Only: No Unicode, emoji, or curly quotes
- Lock-Free: Zero lock() statements (verified by grep)
- TDD: Unit tests required for all extracted methods

## Risk Assessment

### Complexity Reduction Risk
- Risk Level: LOW
- Rationale: Clear separation of concerns, well-defined boundaries
- Mitigation: TDD tests for each extracted method

### Lock-Free Compliance Risk
- Risk Level: LOW
- Rationale: Existing Actor/FSM infrastructure in place
- Mitigation: Forensic scan for lock() statements post-implementation

### Regression Risk
- Risk Level: MINIMAL
- Rationale: Public API unchanged, surgical extraction only
- Mitigation: Existing unit tests + new TDD tests for helpers

## Next Steps (Phase 3)

With architecture plan APPROVED, proceed to:
1. Phase 3: DNA & PR Audit (Arena AI Red Team) - Verify plan against V12 DNA constraints, Validate lock-free compliance, Check PR hygiene (diff <10k characters), PASS/FAIL gate before implementation
2. Phase 4: Recursive Execution (Bob CLI v12-engineer) - Implement extraction using Bob CLI, Create TDD tests for all helpers, Verify complexity reduction (CYC ≤8), Run pre-push validation
3. Phase 5: Verification/Review (Forensics) - Compare implementation against this plan, Run complexity audit, Verify lock-free compliance, F5 test in NinjaTrader

## Approval Criteria

### Architecture Plan Approval Checklist
- ✅ Extraction strategy defined (3 helper methods)
- ✅ Method signatures specified with complexity targets
- ✅ Call graph and data flow documented
- ✅ Lock-free validation confirmed (Actor/FSM pattern)
- ✅ Jane Street cognitive simplicity alignment verified
- ✅ V12.23 boundary compliance maintained
- ✅ Risk assessment completed (all LOW/MINIMAL)

**STATUS**: READY FOR PHASE 3 (DNA & PR AUDIT)

---

**Document Version**: 1.0
**Created**: 2026-06-15
**Epic**: EPIC-CCN-020
**Phase**: 2 (Architecture Planning)
**Next Phase**: 3 (DNA & PR Audit)
