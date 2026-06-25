# Phase 1: Scope Definition - EPIC-W7-035

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 1.51
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T19:28:24Z

## Epic Summary
**Target Method**: SyncLimitTarget
**File**: src/V12_002.Orders.Management.StopSync.cs
**Current Complexity**: CYC 21 (exceeds Jane Street threshold of 8 by 2.6x)
**Target Complexity**: CYC <= 8 per method after extraction

## Scope Boundary Definition

### IN SCOPE

#### 1. Extract UpdateTargetPrice Helper
**Purpose**: Eliminate duplicate switch statements (appears twice in method)
**Logic**: Encapsulate target price update logic for targets 1-5
**Signature**: private void UpdateTargetPrice(PositionInfo pos, int targetNum, double newPrice)
**Complexity Reduction**: Removes 10 lines x 2 occurrences = 20 lines
**Expected CYC**: 2 (single switch statement)

#### 2. Extract RepriceExistingOrder Helper
**Purpose**: Reduce nesting in hasWorkingOrder == true branch
**Logic**: Encapsulate ChangeOrder call, error handling, and price update
**Signature**: private bool RepriceExistingOrder(Order existingOrder, double newPrice, PositionInfo pos, int targetNum, string entryName, ref int refreshed)
**Complexity Reduction**: Removes 1 level of nesting, isolates try-catch
**Expected CYC**: 4 (price comparison + switch + try-catch)

#### 3. Extract SubmitNewLimitOrder Helper
**Purpose**: Reduce nesting in hasWorkingOrder == false branch
**Logic**: Encapsulate SubmitOrderUnmanaged call, direction logic, error handling
**Signature**: private Order SubmitNewLimitOrder(PositionInfo pos, int targetNum, int targetQty, double newPrice, string entryName, ConcurrentDictionary<string, Order> targetDict, ref int refreshed)
**Complexity Reduction**: Removes 1 level of nesting, isolates try-catch
**Expected CYC**: 5 (direction check + null check + switch + try-catch)

#### 4. Refactor Core Method
**Purpose**: Orchestrate extracted helpers, reduce to CYC <= 8
**Logic**:
- Calculate price (existing logic)
- Early return on invalid price (existing logic)
- Branch on hasWorkingOrder (existing logic)
- Delegate to RepriceExistingOrder or SubmitNewLimitOrder
**Expected CYC**: 3 (price validation + hasWorkingOrder branch)

### OUT OF SCOPE

#### 1. Signature Changes
**Rationale**: Single caller (RefreshActivePositionOrders) expects current signature
**Risk**: Breaking caller contract increases blast radius unnecessarily
**Decision**: Preserve all 9 parameters as-is

#### 2. Business Logic Changes
**Rationale**: Method behavior must remain identical
**Risk**: Changing logic introduces regression risk in active trading code
**Decision**: Pure structural refactoring only - no logic changes

#### 3. Caller Modifications
**Rationale**: RefreshActivePositionOrders is out of scope for this epic
**Risk**: Expanding scope violates No Scope Creep Protocol (V12.23)
**Decision**: Caller remains unchanged

#### 4. Price Calculation Logic
**Rationale**: CalculateTargetPriceFromPos is a separate method, already extracted
**Risk**: Modifying price calculation affects all target orders
**Decision**: Price calculation stays as-is

#### 5. Error Handling Strategy
**Rationale**: Current try-catch pattern is standard for NinjaTrader order submission
**Risk**: Changing error handling affects production stability
**Decision**: Preserve existing error handling, just relocate to helpers

#### 6. Logging Format
**Rationale**: Log messages are used for production monitoring
**Risk**: Changing log format breaks existing monitoring/alerting
**Decision**: Preserve all log messages verbatim

## Complexity Analysis

### Before Refactoring
- **Core Method CYC**: 21
- **Nesting Depth**: 6
- **Lines of Code**: ~180
- **Duplicate Code**: 2 identical switch statements (10 lines each)

### After Refactoring (Projected)
- **Core Method CYC**: 3 (price validation + branch)
- **UpdateTargetPrice CYC**: 2 (switch only)
- **RepriceExistingOrder CYC**: 4 (comparison + switch + try-catch)
- **SubmitNewLimitOrder CYC**: 5 (direction + null + switch + try-catch)
- **Total CYC**: 14 (distributed across 4 methods)
- **Max Method CYC**: 5 (all methods <= 8)

### Jane Street Compliance
- All methods <= 8 CYC (cognitive simplicity)
- Reduced nesting (easier race condition auditing)
- Single responsibility per method (exhaustive testing feasible)
- No signature changes (preserves caller contract)

## Risk Mitigation

### Testing Strategy
1. **Unit Tests**: Add tests for each extracted helper before refactoring
2. **Integration Test**: Verify RefreshActivePositionOrders still works
3. **Regression Test**: Run full strategy in NinjaTrader simulator

### Rollback Plan
- Git branch: epic-w7-035-scope-validation
- Checkpoint: Before any code changes
- Rollback: git reset --hard <checkpoint-sha>

### Verification Checklist
- [ ] All extracted methods have CYC <= 8
- [ ] Core method has CYC <= 8
- [ ] No signature changes to SyncLimitTarget
- [ ] RefreshActivePositionOrders unchanged
- [ ] All log messages preserved
- [ ] Build passes (dotnet build)
- [ ] deploy-sync.ps1 executed successfully
- [ ] F5 in NinjaTrader successful

## Success Criteria
1. Scope boundary clearly defined (IN SCOPE vs OUT OF SCOPE)
2. All extracted methods target CYC <= 8
3. No scope creep (caller, business logic, signatures unchanged)
4. Risk mitigation strategy documented
5. Jane Street alignment verified

## Next Phase
**Phase 1.5**: Scope Boundary Validation (mandatory gate)
- Verify no scope creep
- Confirm extraction candidates are feasible
- Validate complexity projections
- Approve or reject scope definition
