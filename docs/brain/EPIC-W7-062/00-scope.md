# Phase 1: Scope Definition - EPIC-W7-062

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.18
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T19:31:59Z
- **Input**: docs/brain/EPIC-W7-062/00-hotspots.md

## Epic Summary
- **Target Method**: ProcessFleetSlot
- **File**: src/V12_002.SIMA.Fleet.cs
- **Current Complexity**: CYC 13 (actual measurement)
- **Target Complexity**: CYC <= 8 (Jane Street threshold)
- **Gap**: +5 (62% over threshold)
- **Blast Radius**: 0 external dependents (SAFE TO REFACTOR)

## Scope Boundary Definition

### IN SCOPE

#### Primary Target
- **ProcessFleetSlot** method (lines 44-98)
  - Current: CYC 13, 54 lines, 8 parameters, max nesting 5
  - Target: CYC <= 8 through extraction

#### Extraction Candidates (4 helper methods)
1. **ValidateFleetDispatchPreconditions**
   - Extract: Timestamp validation logic
   - Includes: ValidateDispatchTimestamp call
   - Reduces: 2-3 complexity points

2. **InitializeFleetFollowerFSM**
   - Extract: FSM initialization logic
   - Includes: InitializeFollowerBracketFSM call
   - Reduces: 2-3 complexity points

3. **ExecuteFleetOrderSubmission**
   - Extract: Order submission orchestration
   - Includes: SubmitAndRegisterFleetOrders call
   - Reduces: 2-3 complexity points

4. **HandleFleetDispatchRollback**
   - Extract: Rollback logic
   - Includes: RollbackFleetDispatchState call
   - Reduces: 1-2 complexity points

#### State Access (Read-Only Analysis)
- _photonPool
- _followerBrackets
- _dispatchSyncPendingExpKeys
- expectedPositions
- activePositions
- entryOrders
- stopOrders
- _photonDispatchRing
- _pendingFleetDispatches

**Note**: State variables are accessed but NOT modified in extraction scope. Mutations remain in parent method.

#### Test Coverage
- Verify existing tests for ProcessFleetSlot
- Add tests for 4 extracted helper methods
- Maintain 100% test pass rate

### OUT OF SCOPE

#### Callers (Unchanged)
- **PumpFleetDispatch** (line 233) - No modifications
- **ProcessValidPhotonSlot** (line 395) - No modifications
- **VerifyPhotonSlotIntegrity** (line 329) - No modifications

#### Callees (Unchanged - 57 methods)
- ValidateDispatchTimestamp
- InitializeFollowerBracketFSM
- SubmitAndRegisterFleetOrders
- RollbackFleetDispatchState
- TryResetCircuitBreakerIfBelow
- PumpFleetDispatch (recursive call - NOT refactored)
- All 51 other callees

**Rationale**: These methods are already extracted and tested. No changes needed.

#### State Mutation Logic
- State variable mutations remain in ProcessFleetSlot
- Extracted methods receive state as parameters (read-only)
- No changes to state management patterns

#### Recursive Call Chain
- PumpFleetDispatch <-> ProcessFleetSlot interaction
- **OUT OF SCOPE**: Breaking recursive chain requires broader refactor
- **RISK ACCEPTED**: Recursive pattern is intentional for fleet dispatch pump

#### Parameter Reduction
- **OUT OF SCOPE**: 8 parameters will NOT be grouped into structs
- **RATIONALE**: Parameter grouping requires broader API changes
- **FUTURE EPIC**: Consider struct-based parameter passing in separate epic

#### Other Files
- **OUT OF SCOPE**: No changes to any file except src/V12_002.SIMA.Fleet.cs
- **ISOLATION**: Zero blast radius confirmed

### Scope Validation

#### Complexity Reduction Math
- **Current**: CYC 13
- **Extract 4 methods**: -8 to -11 complexity points
- **Target**: CYC 5-8 (within Jane Street threshold)
- **Confidence**: HIGH (multiple extraction paths)

#### Risk Assessment
- **Blast Radius**: 0 external dependents
- **Test Coverage**: Existing tests available
- **Isolation**: Single file change
- **Reversibility**: Easy rollback if needed

#### Success Criteria
1. ProcessFleetSlot achieves CYC <= 8
2. 4 helper methods extracted with CYC <= 5 each
3. All existing tests pass (100%)
4. No changes to method signature or callers
5. Build passes with deploy-sync.ps1
6. F5 in NinjaTrader successful

## Extraction Strategy

### Phase 2 Architecture Plan
1. **Validation Extraction**: ValidateFleetDispatchPreconditions
   - Input: 8 parameters (pass-through)
   - Output: bool (validation result)
   - Complexity: CYC <= 3

2. **FSM Initialization Extraction**: InitializeFleetFollowerFSM
   - Input: Subset of parameters (acct, fleetEntryName, poolSlotIndex)
   - Output: SIMA_FSM (initialized FSM)
   - Complexity: CYC <= 3

3. **Order Submission Extraction**: ExecuteFleetOrderSubmission
   - Input: FSM, orders, orderCount, signalTicks
   - Output: bool (submission success)
   - Complexity: CYC <= 4

4. **Rollback Extraction**: HandleFleetDispatchRollback
   - Input: expectedKey, reservedDelta, poolSlotIndex
   - Output: void
   - Complexity: CYC <= 2

### Dependency Order
1. Extract validation (no dependencies)
2. Extract FSM initialization (depends on validation)
3. Extract order submission (depends on FSM init)
4. Extract rollback (independent)

### Testing Strategy
- Unit tests for each extracted method
- Integration test for ProcessFleetSlot (end-to-end)
- Regression test suite (existing tests)

## Boundary Enforcement

### What Changes
- ProcessFleetSlot method body (extraction only)
- Add 4 new private helper methods
- Update complexity metrics in epic roadmap

### What Stays the Same
- Method signature (8 parameters unchanged)
- Caller methods (no API changes)
- Callee methods (no modifications)
- State variables (no new state)
- Recursive call pattern (intentional design)

## Jane Street Alignment

### Cognitive Simplicity
- **Before**: CYC 13 (hard to reason about)
- **After**: CYC <= 8 (Jane Street threshold)
- **Benefit**: Easier to audit for race conditions in lock-free code

### Testability
- **Before**: 1 complex method (exponential path growth)
- **After**: 5 simple methods (linear test coverage)
- **Benefit**: Exhaustive testing becomes feasible

### Maintainability
- **Before**: 54 lines, 5 nesting levels
- **After**: 4 focused methods, max 3 nesting levels
- **Benefit**: Faster onboarding, easier debugging

## Next Steps (Phase 2)

1. **Architecture Planning**: Design 4 helper method signatures
2. **Dependency Mapping**: Verify parameter flow between methods
3. **Test Design**: Write test cases for each extracted method
4. **Extraction Order**: Validate dependency sequence

## Scope Approval

**Scope Status**: DEFINED
**Boundary Clarity**: HIGH (clear IN/OUT separation)
**Risk Level**: LOW (zero blast radius, isolated change)
**Complexity Target**: ACHIEVABLE (CYC 13 -> <=8 via 4 extractions)

**Ready for Phase 2**: YES
