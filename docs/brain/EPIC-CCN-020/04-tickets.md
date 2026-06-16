# Extraction Tickets: EPIC-CCN-020

## Overview
- **Total Tickets**: 4
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3 → TICKET-4)
- **Estimated Effort**: 6-8 hours
- **Target Method**: HandleSecondaryOrderFilled
- **Current Complexity**: CYC=21
- **Target Complexity**: CYC ≤8 (main method), CYC ≤6 (helpers)

## TICKET-1: Extract Validation Logic

### Scope
- **Current Method**: `HandleSecondaryOrderFilled`
- **Current CYC**: 21
- **Target CYC**: ≤5 (extracted method)
- **Extraction**: Create `ValidateSecondaryOrderExecution` helper method

### Implementation
1. **Create Pure Validation Function**
   - Signature: `private bool ValidateSecondaryOrderExecution(Order order, Execution execution, int quantity, double price)`
   - Extract all validation logic from HandleSecondaryOrderFilled
   - Validate order state consistency
   - Validate execution parameters
   - Check quantity and price bounds
   - Return true if all validations pass, false otherwise

2. **Characteristics**
   - Pure function (no side effects)
   - Early return on first validation failure
   - No state mutations
   - No Actor/FSM calls (validation only)

3. **Update Main Method**
   - Replace inline validation with call to ValidateSecondaryOrderExecution
   - Add early return if validation fails
   - Preserve error logging behavior

### Acceptance Criteria
- [ ] ValidateSecondaryOrderExecution method created with CYC ≤5
- [ ] Method is pure (no side effects, no state mutations)
- [ ] All validation logic extracted from main method
- [ ] Main method calls helper and handles false return
- [ ] Unit tests written for all validation edge cases
- [ ] All existing tests pass
- [ ] Build succeeds
- [ ] CSharpier formatting applied

### Dependencies
- None (first ticket)

### Testing Strategy
- **Unit Tests Required**:
  - Test valid order/execution parameters (returns true)
  - Test invalid order state (returns false)
  - Test invalid execution parameters (returns false)
  - Test quantity bounds violations (returns false)
  - Test price bounds violations (returns false)
  - Test null/edge cases (returns false)

---

## TICKET-2: Extract Position & PnL Updates

### Scope
- **Current Method**: `HandleSecondaryOrderFilled`
- **Current CYC**: 21 (after TICKET-1: ~16)
- **Target CYC**: ≤6 (extracted method)
- **Extraction**: Create `UpdatePositionAndPnL` helper method

### Implementation
1. **Create Position Update Method**
   - Signature: `private void UpdatePositionAndPnL(Order order, int quantity, double price, DateTime time)`
   - Extract position quantity updates
   - Extract realized/unrealized PnL calculations
   - Use Actor Enqueue pattern for all state mutations
   - No direct field mutations (lock-free requirement)

2. **Characteristics**
   - Uses Actor pattern for atomic updates
   - Enqueues position state changes
   - Calculates PnL based on order type and quantity
   - No lock() statements
   - Fire-and-forget (void return)

3. **Update Main Method**
   - Replace inline position/PnL logic with call to UpdatePositionAndPnL
   - Pass required parameters (order, quantity, price, time)
   - Maintain execution order (after validation, before state transition)

### Acceptance Criteria
- [ ] UpdatePositionAndPnL method created with CYC ≤6
- [ ] All position updates use Actor Enqueue pattern
- [ ] No lock() statements (verified by grep)
- [ ] PnL calculations extracted and correct
- [ ] Main method calls helper with correct parameters
- [ ] Unit tests written for position update scenarios
- [ ] All existing tests pass
- [ ] Build succeeds
- [ ] Forensic scan passes: `grep -r "lock(" src/V12_002.Orders.Callbacks.cs` returns zero matches

### Dependencies
- TICKET-1 must be completed first

### Testing Strategy
- **Unit Tests Required**:
  - Test position quantity updates (long/short)
  - Test realized PnL calculation (profit/loss)
  - Test unrealized PnL calculation
  - Test Actor Enqueue calls (verify queue operations)
  - Test edge cases (zero quantity, negative price)
  - Mock Actor queue to verify atomic operations

---

## TICKET-3: Extract State Transition Logic

### Scope
- **Current Method**: `HandleSecondaryOrderFilled`
- **Current CYC**: 21 (after TICKET-1,2: ~11)
- **Target CYC**: ≤5 (extracted method)
- **Extraction**: Create `TransitionOrderState` helper method

### Implementation
1. **Create State Transition Method**
   - Signature: `private void TransitionOrderState(Order order, string executionId, DateTime time)`
   - Extract FSM state transition logic
   - Update order status via FSM Enqueue
   - Enqueue state change events
   - No direct state mutation (lock-free requirement)

2. **Characteristics**
   - Uses FSM/Actor Enqueue pattern
   - Atomic state transitions via queue
   - Event-driven state changes
   - No lock() statements
   - Fire-and-forget (void return)

3. **Update Main Method**
   - Replace inline state transition logic with call to TransitionOrderState
   - Pass required parameters (order, executionId, time)
   - Maintain execution order (after position updates)

### Acceptance Criteria
- [ ] TransitionOrderState method created with CYC ≤5
- [ ] All state transitions use FSM Enqueue pattern
- [ ] No lock() statements (verified by grep)
- [ ] State change events enqueued correctly
- [ ] Main method calls helper with correct parameters
- [ ] Unit tests written for state transition scenarios
- [ ] All existing tests pass
- [ ] Build succeeds
- [ ] Forensic scan passes: `grep -r "lock(" src/V12_002.Orders.Callbacks.cs` returns zero matches

### Dependencies
- TICKET-1 must be completed first
- TICKET-2 must be completed first

### Testing Strategy
- **Unit Tests Required**:
  - Test valid state transitions (Filled → Completed)
  - Test FSM Enqueue calls (verify queue operations)
  - Test state change event generation
  - Test edge cases (invalid transitions)
  - Mock FSM queue to verify atomic operations
  - Verify no direct state mutation

---

## TICKET-4: Refactor Main Orchestration Method

### Scope
- **Current Method**: `HandleSecondaryOrderFilled`
- **Current CYC**: 21 (after TICKET-1,2,3: ~8)
- **Target CYC**: ≤8 (orchestration only)
- **Extraction**: Simplify main method to orchestration logic only

### Implementation
1. **Simplify Orchestration Flow**
   - Main method now only calls 3 helpers in sequence
   - Validation → Position Updates → State Transition
   - Early return if validation fails
   - No inline logic (all extracted to helpers)
   - Preserve public API signature (no breaking changes)

2. **Orchestration Pattern**
   ```csharp
   private void HandleSecondaryOrderFilled(Order order, Execution execution, string executionId, int quantity, double price, DateTime time)
   {
       // Step 1: Validate
       if (!ValidateSecondaryOrderExecution(order, execution, quantity, price))
       {
           // Log error and return
           return;
       }
       
       // Step 2: Update Position & PnL
       UpdatePositionAndPnL(order, quantity, price, time);
       
       // Step 3: Transition State
       TransitionOrderState(order, executionId, time);
   }
   ```

3. **Verification**
   - Complexity reduced to CYC ≤8
   - All logic delegated to helpers
   - Public API unchanged
   - Behavior preserved

### Acceptance Criteria
- [ ] Main method complexity reduced to CYC ≤8
- [ ] All inline logic removed (delegated to helpers)
- [ ] Public API signature unchanged
- [ ] Orchestration flow correct (validation → position → state)
- [ ] Early return on validation failure preserved
- [ ] Integration tests pass (end-to-end behavior)
- [ ] All unit tests pass
- [ ] Build succeeds
- [ ] Complexity audit passes: `python scripts/complexity_audit.py`
- [ ] Pre-push validation passes: `powershell -File .\scripts\pre_push_validation.ps1`

### Dependencies
- TICKET-1 must be completed first
- TICKET-2 must be completed first
- TICKET-3 must be completed first

### Testing Strategy
- **Integration Tests Required**:
  - Test full orchestration flow (validation → position → state)
  - Test early return on validation failure
  - Test successful execution path
  - Verify behavior matches original implementation
  - Regression test against existing test suite

---

## Execution Strategy

### Sequential Execution
1. **TICKET-1**: Extract validation (foundation)
2. **TICKET-2**: Extract position updates (depends on validation)
3. **TICKET-3**: Extract state transitions (depends on validation)
4. **TICKET-4**: Simplify orchestration (depends on all helpers)

### Verification Checkpoints
- After each ticket: Run unit tests
- After each ticket: Run complexity audit
- After each ticket: Run forensic scan (lock-free verification)
- After TICKET-4: Run full pre-push validation
- After TICKET-4: Run integration tests
- After TICKET-4: F5 test in NinjaTrader

### Rollback Plan
- Git checkpoint before each ticket (Bob CLI auto-checkpoint enabled)
- If any ticket fails: Revert to previous checkpoint
- If complexity target not met: Rework extraction strategy

---

## Success Metrics

### Complexity Reduction
- **Before**: HandleSecondaryOrderFilled CYC=21
- **After**: 
  - HandleSecondaryOrderFilled CYC ≤8
  - ValidateSecondaryOrderExecution CYC ≤5
  - UpdatePositionAndPnL CYC ≤6
  - TransitionOrderState CYC ≤5
- **Total Reduction**: 69% complexity reduction in main method

### Code Quality
- Zero lock() statements (lock-free compliance)
- Zero Unicode characters (ASCII-only compliance)
- All tests pass (regression-free)
- Build succeeds (no breaking changes)
- CSharpier formatted (braces added)

### Jane Street Alignment
- Cognitive simplicity: 4 focused methods vs 1 monolithic method
- Testability: Pure validation function + atomic state updates
- Concurrency: Lock-free Actor pattern (microsecond-latency optimized)
- Correctness by construction: FSM pattern makes illegal states unrepresentable

---

**Document Version**: 1.0
**Created**: 2026-06-15
**Epic**: EPIC-CCN-020
**Phase**: 4 (Ticket Generation)
**Next Phase**: 5 (Recursive Execution)
