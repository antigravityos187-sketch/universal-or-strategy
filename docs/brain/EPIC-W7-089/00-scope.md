# Phase 1: Scope Definition - EPIC-W7-089

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.00
- **API Key**: jCodemunch MCP
- **Execution Time**: ~10 seconds

## Epic Objective
Reduce cyclomatic complexity of `CancelWatchdogWorkingOrders` from CYC=10 to ≤8 (Jane Street threshold).

## Target Method
- **Method**: CancelWatchdogWorkingOrders
- **File**: src/V12_002.Safety.Watchdog.cs
- **Line**: 138
- **Current CYC**: 10
- **Target CYC**: ≤8
- **Lines of Code**: 28
- **Max Nesting**: 3
- **Parameters**: 2

## Scope Boundary

### IN SCOPE ✅

1. **Primary Target**
   - `CancelWatchdogWorkingOrders` method (lines 138-165)
   - Extract conditional logic into helper methods
   - Reduce nesting depth from 3 to 2
   - Reduce CYC from 10 to ≤8

2. **Extraction Candidates**
   - Order terminal state validation logic
   - Order cancellation decision logic
   - Working order filtering logic

3. **Allowed Modifications**
   - Create new private helper methods in V12_002.Safety.Watchdog.cs
   - Refactor conditional branches
   - Simplify nested if statements
   - Preserve exact behavior and semantics

4. **Testing Scope**
   - Unit tests for extracted helper methods
   - Integration test for CancelWatchdogWorkingOrders
   - Verify watchdog safety semantics preserved

### OUT OF SCOPE ❌

1. **Caller Method**
   - `ExecuteWatchdogLeadAccountFlatten` (line 211)
   - Reason: Single caller, no complexity issues reported

2. **Callee Methods**
   - `CancelOrderOnAccount` (src/V12_002.Orders.CancelGateway.cs:46)
   - `IsOrderTerminal` (src/V12_002.Orders.Management.Flatten.cs:698)
   - Reason: External methods, separate concerns

3. **Other Watchdog Methods**
   - Any other methods in V12_002.Safety.Watchdog.cs
   - Reason: Not part of this epic scope

4. **Behavioral Changes**
   - No changes to order cancellation logic
   - No changes to terminal state detection
   - No changes to watchdog safety semantics
   - Reason: Refactoring only, preserve exact behavior

## Extraction Strategy

### Approach
**Surgical Extraction** - Extract conditional branches into focused helper methods

### Proposed Helper Methods
1. `ShouldCancelWatchdogOrder(Order order)` - Consolidate cancellation decision logic
2. `GetCancellableWatchdogOrders(Account account, string instrumentName)` - Filter working orders

### Complexity Reduction Plan
- Current: 10 decision points
- Target: ≤8 decision points
- Method: Extract 2-3 conditional branches into helpers
- Expected: CYC reduction of 2-3 points

## Risk Assessment

### Blast Radius: VERY LOW ✅
- Zero external importers
- Zero direct dependents
- Single caller (internal)
- Overall risk score: 0.0

### Refactoring Safety: HIGH ✅
- Contained scope (watchdog subsystem)
- No cross-module dependencies
- Clear single responsibility
- Low churn rate (not in top 50 hotspots)

### Testing Requirements: STANDARD ✅
- Unit tests for helper methods
- Integration test for main method
- F5 verification in NinjaTrader

## Success Criteria

### Phase 1 (Scope Definition) ✅
- [x] Scope boundary defined (IN/OUT)
- [x] Extraction candidates identified
- [x] Risk assessment completed
- [x] 00-scope.md created

### Phase 2 (Architecture Planning)
- [ ] Helper method signatures designed
- [ ] Extraction sequence planned
- [ ] Test strategy defined

### Phase 5 (Ticket Execution)
- [ ] CYC reduced from 10 to ≤8
- [ ] Helper methods extracted
- [ ] Tests passing
- [ ] Build successful
- [ ] F5 verification passed

## Dependencies

### Prerequisites
- None (standalone epic)

### Blockers
- None identified

### Related Epics
- None (isolated refactoring)

## Notes
- Method is part of V12 Safety Watchdog subsystem
- Handles cancellation of working orders during watchdog operations
- Called exclusively by ExecuteWatchdogLeadAccountFlatten
- Uses order cancellation gateway and terminal state checks
- Low priority due to low churn and contained scope
- Good candidate for surgical extraction practice
