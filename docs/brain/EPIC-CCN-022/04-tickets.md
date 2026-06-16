# Extraction Tickets: EPIC-CCN-022

## Overview
- **Total Tickets**: 4
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3 → TICKET-4)
- **Estimated Effort**: 6-8 hours
- **Target Method**: `PropagateMaster_IdentifyMove`
- **Current Complexity**: 18 (CYC)
- **Target Complexity**: ≤8 (CYC) per method
- **Expected Reduction**: 55-66% complexity reduction

---

## TICKET-1: Extract Error Handler

### Scope
- **Current Method**: `PropagateMaster_IdentifyMove`
- **Current CYC**: 18
- **Target CYC**: ≤3 (for extracted method)
- **Extraction**: `HandlePropagationError` - Error handling and logging

### Implementation
1. Create new private method `HandlePropagationError(Order masterOrder, Order slaveOrder, Exception exception)`
2. Extract error logging logic from try-catch block
3. Keep error handling simple (logging only, no complex recovery)
4. Verify no state mutations (read-only logging)
5. Update orchestrator to call new method in catch block

### Code Changes
**File**: `src/V12_002.Orders.Callbacks.Propagation.cs`

**New Method**:
```csharp
private void HandlePropagationError(Order masterOrder, Order slaveOrder, Exception exception)
{
    // Log error details
    // Record error metrics (if applicable)
    // No state recovery (fail-fast pattern)
}
```

**Orchestrator Update**:
```csharp
catch (Exception ex)
{
    HandlePropagationError(masterOrder, slaveOrder, ex);
}
```

### Acceptance Criteria
- [ ] Method complexity ≤3 (CYC)
- [ ] No lock() statements
- [ ] ASCII-only strings
- [ ] All existing tests pass
- [ ] No behavioral changes
- [ ] Build succeeds: `dotnet build`
- [ ] Complexity verified: `python3 scripts/complexity_audit.py`

### Verification Commands
```bash
# Check complexity
python3 scripts/complexity_audit.py

# Verify no locks
grep -r "lock(" src/V12_002.Orders.Callbacks.Propagation.cs

# Build check
dotnet build

# Test check
dotnet test
```

### Dependencies
- None (first ticket - simplest extraction)

---

## TICKET-2: Extract Validation Logic

### Scope
- **Current Method**: `PropagateMaster_IdentifyMove`
- **Current CYC**: 18 (after TICKET-1: ~15-16)
- **Target CYC**: ≤5 (for extracted method)
- **Extraction**: `ValidateOrderStatesForPropagation` - Order state validation

### Implementation
1. Create new private method `ValidateOrderStatesForPropagation(Order masterOrder, Order slaveOrder)`
2. Extract order state validation logic (master/slave state checks)
3. Return boolean result (valid/invalid)
4. Use early return pattern for fail-fast validation
5. Ensure read-only (no state mutations)
6. Update orchestrator to call validation method

### Code Changes
**File**: `src/V12_002.Orders.Callbacks.Propagation.cs`

**New Method**:
```csharp
private bool ValidateOrderStatesForPropagation(Order masterOrder, Order slaveOrder)
{
    // Check master order state (Working, Filled, Cancelled)
    // Check slave order state compatibility
    // Validate order relationship (master-slave linkage)
    // Return boolean (no exceptions)
}
```

**Orchestrator Update**:
```csharp
if (!ValidateOrderStatesForPropagation(masterOrder, slaveOrder))
{
    return; // Early exit on invalid state
}
```

### Acceptance Criteria
- [ ] Method complexity ≤5 (CYC)
- [ ] Returns boolean (no exceptions)
- [ ] No lock() statements
- [ ] ASCII-only strings
- [ ] Read-only (no state mutations)
- [ ] All existing tests pass
- [ ] New unit tests added (5 tests minimum):
  - [ ] Valid master/slave states
  - [ ] Invalid master state
  - [ ] Invalid slave state
  - [ ] Invalid relationship
  - [ ] Edge case (null orders)
- [ ] Build succeeds: `dotnet build`
- [ ] Complexity verified: `python3 scripts/complexity_audit.py`

### Verification Commands
```bash
# Check complexity
python3 scripts/complexity_audit.py

# Verify no locks
grep -r "lock(" src/V12_002.Orders.Callbacks.Propagation.cs

# Build check
dotnet build

# Test check (with new tests)
dotnet test
```

### Dependencies
- TICKET-1 must be completed first

---

## TICKET-3: Extract Decision Logic

### Scope
- **Current Method**: `PropagateMaster_IdentifyMove`
- **Current CYC**: 18 (after TICKET-1,2: ~10-12)
- **Target CYC**: ≤6 (for extracted method)
- **Extraction**: `DeterminePropagationAction` - Propagation action determination

### Implementation
1. Create `PropagationAction` enum (if not exists):
   ```csharp
   private enum PropagationAction
   {
       None,           // No propagation needed
       PropagateMove,  // Propagate move to slave
       CancelSlave,    // Cancel slave order
       SkipPropagation // Skip due to invalid state
   }
   ```
2. Create new private method `DeterminePropagationAction(Order masterOrder, Order slaveOrder, OrderAction action)`
3. Extract business rule logic for propagation decisions
4. Return enum value (no side effects)
5. Use switch/case or if/else chains (no nested loops)
6. Update orchestrator to use enum-based decision

### Code Changes
**File**: `src/V12_002.Orders.Callbacks.Propagation.cs`

**New Enum**:
```csharp
private enum PropagationAction
{
    None,
    PropagateMove,
    CancelSlave,
    SkipPropagation
}
```

**New Method**:
```csharp
private PropagationAction DeterminePropagationAction(Order masterOrder, Order slaveOrder, OrderAction action)
{
    // Analyze master order action (Fill, Cancel, Modify)
    // Determine slave order response
    // Apply business rules
    // Return action enum (no state mutations)
}
```

**Orchestrator Update**:
```csharp
PropagationAction actionType = DeterminePropagationAction(masterOrder, slaveOrder, action);

switch (actionType)
{
    case PropagationAction.PropagateMove:
        _fsmQueue.Enqueue(new PropagateCommand(masterOrder, slaveOrder));
        break;
    case PropagationAction.CancelSlave:
        _fsmQueue.Enqueue(new CancelCommand(slaveOrder));
        break;
    case PropagationAction.SkipPropagation:
    case PropagationAction.None:
    default:
        return;
}
```

### Acceptance Criteria
- [ ] Method complexity ≤6 (CYC)
- [ ] Returns enum (type-safe)
- [ ] No lock() statements
- [ ] ASCII-only strings
- [ ] Pure function (no state mutations)
- [ ] Compiler-enforced exhaustive switch handling
- [ ] All existing tests pass
- [ ] New unit tests added (5 tests minimum):
  - [ ] PropagateMove case
  - [ ] CancelSlave case
  - [ ] SkipPropagation case
  - [ ] None case
  - [ ] Edge case (invalid action)
- [ ] Build succeeds: `dotnet build`
- [ ] Complexity verified: `python3 scripts/complexity_audit.py`

### Verification Commands
```bash
# Check complexity
python3 scripts/complexity_audit.py

# Verify no locks
grep -r "lock(" src/V12_002.Orders.Callbacks.Propagation.cs

# Verify enum exists
grep -A 5 "enum PropagationAction" src/V12_002.Orders.Callbacks.Propagation.cs

# Build check
dotnet build

# Test check (with new tests)
dotnet test
```

### Dependencies
- TICKET-1 must be completed first
- TICKET-2 must be completed first

---

## TICKET-4: Refactor Orchestrator

### Scope
- **Current Method**: `PropagateMaster_IdentifyMove`
- **Current CYC**: 18 (after TICKET-1,2,3: ~8-10)
- **Target CYC**: ≤8 (for orchestrator)
- **Refactoring**: Simplify orchestrator to use extracted helpers

### Implementation
1. Refactor orchestrator to call helper methods in sequence
2. Use early return pattern for validation failures
3. Use switch statement for enum-based decision logic
4. Keep orchestrator lean (coordination only, no business logic)
5. Verify FSM Enqueue pattern for state mutations
6. Ensure no lock() statements

### Code Changes
**File**: `src/V12_002.Orders.Callbacks.Propagation.cs`

**Refactored Orchestrator**:
```csharp
private void PropagateMaster_IdentifyMove(Order masterOrder, Order slaveOrder, OrderAction action)
{
    try
    {
        // Step 1: Validate
        if (!ValidateOrderStatesForPropagation(masterOrder, slaveOrder))
        {
            return; // Early exit
        }

        // Step 2: Determine action
        PropagationAction actionType = DeterminePropagationAction(masterOrder, slaveOrder, action);

        // Step 3: Execute via FSM
        switch (actionType)
        {
            case PropagationAction.PropagateMove:
                _fsmQueue.Enqueue(new PropagateCommand(masterOrder, slaveOrder));
                break;
            case PropagationAction.CancelSlave:
                _fsmQueue.Enqueue(new CancelCommand(slaveOrder));
                break;
            case PropagationAction.SkipPropagation:
            case PropagationAction.None:
            default:
                return;
        }
    }
    catch (Exception ex)
    {
        HandlePropagationError(masterOrder, slaveOrder, ex);
    }
}
```

### Acceptance Criteria
- [ ] Orchestrator complexity ≤8 (CYC)
- [ ] No lock() statements
- [ ] ASCII-only strings
- [ ] Uses FSM Enqueue pattern for state mutations
- [ ] All helper methods called correctly
- [ ] Early return pattern implemented
- [ ] All existing tests pass
- [ ] New integration tests added (7 tests minimum):
  - [ ] Valid propagation flow
  - [ ] Invalid state (validation fails)
  - [ ] PropagateMove action
  - [ ] CancelSlave action
  - [ ] SkipPropagation action
  - [ ] Exception handling
  - [ ] Edge case (null orders)
- [ ] Build succeeds: `dotnet build`
- [ ] Complexity verified: `python3 scripts/complexity_audit.py`
- [ ] **Final verification**: All methods in file ≤8 (CYC)

### Verification Commands
```bash
# Check complexity (all methods)
python3 scripts/complexity_audit.py

# Verify no locks
grep -r "lock(" src/V12_002.Orders.Callbacks.Propagation.cs

# Build check
dotnet build

# Test check (all tests)
dotnet test

# Format check
dotnet csharpier check src/

# Full pre-push validation
powershell -File .\scripts\pre_push_validation.ps1 -Fast
```

### Dependencies
- TICKET-1 must be completed first
- TICKET-2 must be completed first
- TICKET-3 must be completed first

---

## Post-Implementation Checklist

### Phase 5 Verification
- [ ] All 4 tickets completed
- [ ] Complexity audit: `python3 scripts/complexity_audit.py`
  - [ ] `PropagateMaster_IdentifyMove` ≤8 (CYC)
  - [ ] `ValidateOrderStatesForPropagation` ≤5 (CYC)
  - [ ] `DeterminePropagationAction` ≤6 (CYC)
  - [ ] `HandlePropagationError` ≤3 (CYC)
- [ ] Lock-free audit: `grep -r "lock(" src/V12_002.Orders.Callbacks.Propagation.cs` (zero matches)
- [ ] Build health: `dotnet build` (zero errors)
- [ ] Test health: `dotnet test` (100% pass rate)
- [ ] Format check: `dotnet csharpier check src/` (zero issues)
- [ ] Pre-push validation: `powershell -File .\scripts\pre_push_validation.ps1 -Fast` (all checks pass)

### Phase 6 Sign-off
- [ ] Manual F5 test in NinjaTrader
  - [ ] Master order fill → slave order propagation
  - [ ] Master order cancel → slave order cancel
  - [ ] Invalid state → error handling
- [ ] Deploy sync: `powershell -File .\deploy-sync.ps1`
- [ ] Verify BUILD_TAG in NinjaTrader
- [ ] Update manifest.json with completion status
- [ ] Create PR with Arena AI audit

---

## Risk Mitigation

### Rollback Strategy
- **Bob CLI Checkpointing**: Enabled (auto-restore on failure)
- **Git Branch**: `epic-ccn-022-propagation-extraction`
- **Rollback Command**: `git reset --hard HEAD~1` (per ticket)
- **Full Rollback**: `/restore` in Bob CLI

### High-Risk Areas
1. **Order State Validation** (TICKET-2)
   - Risk: Complex state machine interactions
   - Mitigation: Comprehensive unit tests (5+ tests)
   
2. **Propagation Logic** (TICKET-3)
   - Risk: Business rules may be intricate
   - Mitigation: Enum-based design, exhaustive switch handling

3. **Orchestrator Refactoring** (TICKET-4)
   - Risk: Integration issues between helpers
   - Mitigation: Integration tests (7+ tests)

---

## Success Metrics

### Complexity Reduction
- **Original**: 18 (CYC)
- **Target**: ≤8 (CYC) per method
- **Measurement**: `python3 scripts/complexity_audit.py`
- **Success**: All methods ≤8 (Jane Street strict)

### Test Coverage
- **Target**: 20+ unit tests (5 per helper + 7 integration)
- **Measurement**: `dotnet test --collect:"XPlat Code Coverage"`
- **Success**: 100% branch coverage on extracted methods

### Build Health
- **Target**: Zero compilation errors, zero test failures
- **Measurement**: `dotnet build && dotnet test`
- **Success**: All green

### Lock-Free Compliance
- **Target**: Zero lock() statements
- **Measurement**: `grep -r "lock(" src/V12_002.Orders.Callbacks.Propagation.cs`
- **Success**: Zero matches

---

## Metadata
- **Epic**: EPIC-CCN-022
- **Phase**: 4.0 (Ticket Generation)
- **Date**: 2026-06-15
- **Total Tickets**: 4
- **Execution Order**: Sequential (simple → complex)
- **Estimated Effort**: 6-8 hours
- **V12 Protocol**: V12.23
- **Jane Street Alignment**: Cognitive simplicity, testability, lock-free
- **Next Phase**: Phase 5 (Ticket Execution)
