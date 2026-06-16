# Phase 1: Scope Definition + Boundary Validation - EPIC-CCN-118

## Epic Metadata
- **Epic ID**: EPIC-CCN-118
- **Phase**: 1 (Scope + Boundary)
- **Target Method**: ProcessSingleFleetRMAAccount
- **File**: src/V12_002.SIMA.Execution.cs
- **Lines**: 511-680 (~170 lines)
- **Current Complexity**: 16
- **Target Complexity**: ≤ 8 (Jane Street HFT standard)
- **Analysis Date**: 2026-06-13

## Executive Summary
ProcessSingleFleetRMAAccount is a fleet RMA (Risk Management Account) order submission method with cyclomatic complexity of 16, exceeding the V12 DNA threshold of 15. This epic will extract decision logic into focused helper methods to achieve Jane Street's cognitive simplicity standard (CYC ≤ 8).

## Target Method Details

### Method Signature
```csharp
private bool ProcessSingleFleetRMAAccount(
    Account acct,
    string baseSignal,
    OrderAction entryAction,
    int qty,
    double price,
    MarketPosition direction,
    RMABracketPrices prices,
    string symmetryDispatchId,
    StringBuilder dispatchLog
)
```

### Current Responsibilities
1. **Fleet Validation**: Check if account is active in fleet registry
2. **Consistency Lock**: Validate daily P&L against cap
3. **Order Creation**: Create limit entry order via NinjaTrader API
4. **Position Tracking**: Build PositionInfo with 5-target distribution
5. **FSM Registration**: Register follower bracket state machine
6. **Symmetry Guard**: Register follower in symmetry dispatch system
7. **Expected Position Management**: Update atomic position counters
8. **Error Handling**: Full rollback on submission failure

### Complexity Breakdown
**Decision Points (16 total)**:
1. Fleet active check (`activeFleetAccounts.TryGetValue`)
2. Fleet active boolean validation (`!isActive`)
3. Consistency lock enabled check (`EnableConsistencyLock`)
4. Daily P&L cap check (`dailyPL >= MaxDailyProfitCap`)
5. CreateOrder null check (`fEntry == null`)
6. FSM containment check (`!_followerBrackets.ContainsKey`)
7. OrderId null check (`fEntry != null`)
8. OrderId empty check (`!string.IsNullOrEmpty(fEntry.OrderId)`)
9. Exception catch block (implicit branch)
10. Sync pending check in catch (`if (syncPending)`)
11. Reserved delta check in catch (`if (reservedDelta != 0)`)
12-16. Multiple TryRemove operations (implicit branches)

## Extraction Strategy

### Phase 1 Scope: Single Method Extraction
This epic targets **ONLY** ProcessSingleFleetRMAAccount. No scope creep to related methods.

### Extraction Plan

#### Helper 1: ValidateFleetAccountEligibility
**Purpose**: Consolidate fleet validation and consistency lock checks
**Extracts**:
- Fleet active registry check
- Consistency lock P&L validation
- Early return logic

**Signature**:
```csharp
private bool ValidateFleetAccountEligibility(
    Account acct,
    StringBuilder dispatchLog
)
```

**Complexity Reduction**: -4 decision points

#### Helper 2: BuildFleetPositionInfo
**Purpose**: Encapsulate PositionInfo construction
**Extracts**:
- PositionInfo object creation
- 5-target distribution setup
- Bracket metadata initialization

**Signature**:
```csharp
private PositionInfo BuildFleetPositionInfo(
    string fleetKey,
    MarketPosition direction,
    int qty,
    double price,
    RMABracketPrices prices,
    Account acct,
    Order entryOrder
)
```

**Complexity Reduction**: -0 (pure construction, no branches)

#### Helper 3: RegisterFleetOrderTracking
**Purpose**: Atomic registration of order tracking dictionaries
**Extracts**:
- activePositions registration
- entryOrders registration
- FSM registration
- OrderId-to-FSM mapping

**Signature**:
```csharp
private void RegisterFleetOrderTracking(
    string fleetKey,
    PositionInfo positionInfo,
    Order entryOrder,
    int qty
)
```

**Complexity Reduction**: -3 decision points (FSM check, OrderId checks)

#### Helper 4: RollbackFleetOrderTracking
**Purpose**: Centralize error rollback logic
**Extracts**:
- Dictionary cleanup (activePositions, entryOrders, FSM)
- Expected position delta reversal
- Sync pending flag cleanup

**Signature**:
```csharp
private void RollbackFleetOrderTracking(
    string fleetKey,
    string expectedKey,
    int reservedDelta,
    bool syncPending
)
```

**Complexity Reduction**: -3 decision points (rollback conditionals)

### What Stays in ProcessSingleFleetRMAAccount
**Core orchestration logic**:
1. Call ValidateFleetAccountEligibility (early return on failure)
2. Build fleetKey and expectedKey
3. Call SymmetryGuardRegisterFollower
4. Create order via acct.CreateOrder
5. Guard CreateOrder null result
6. Call BuildFleetPositionInfo
7. Mark sync pending
8. Call RegisterFleetOrderTracking
9. Update expected positions
10. Submit order
11. Clear sync pending
12. Return success
13. Catch block: Call RollbackFleetOrderTracking

**Expected Complexity After Extraction**: 6-8 decision points

## Boundary Validation

### Scope Boundary Definition
**IN SCOPE**:
- ProcessSingleFleetRMAAccount method ONLY (lines 511-680)
- Helper method extraction within same file
- No changes to method signature or public API
- No changes to caller (ExecuteRMAEntryV2)

**OUT OF SCOPE**:
- ExecuteRMAEntryV2 (caller method)
- ValidateRMAEntryGuards (existing helper)
- CalculateRMABracketPrices (existing helper)
- SubmitLocalRMAEntry (existing helper)
- Any other methods in V12_002.SIMA.Execution.cs
- Cross-file refactoring
- FSM state machine logic changes
- Symmetry guard protocol changes

### Dependency Analysis

#### Internal Dependencies (SAFE - within method)
- activeFleetAccounts (class field)
- EnableConsistencyLock (property)
- MaxDailyProfitCap (property)
- activePositions (class field)
- entryOrders (class field)
- _followerBrackets (class field)
- _orderIdToFsmKey (class field)

#### External Dependencies (BOUNDARY RISK)
**None identified**. All dependencies are:
1. Class-level fields (safe to access from helpers)
2. NinjaTrader API calls (Account.CreateOrder, Account.Submit)
3. Helper methods already extracted (SymmetryGuardRegisterFollower, etc.)

### Boundary Violation Checks

#### Check 1: Single Method Scope
✅ **PASS**: Extraction targets only ProcessSingleFleetRMAAccount
- No changes to ExecuteRMAEntryV2 (caller)
- No changes to existing helpers
- No cross-file modifications

#### Check 2: No Signature Changes
✅ **PASS**: Method signature remains unchanged
- Same 9 parameters
- Same return type (bool)
- Same access modifier (private)

#### Check 3: No Caller Impact
✅ **PASS**: Caller (ExecuteRMAEntryV2) unaffected
- Method still called with same arguments (line 782)
- Return value handling unchanged
- No behavioral changes visible to caller

#### Check 4: No Cross-Method Dependencies
✅ **PASS**: Extracted helpers are self-contained
- ValidateFleetAccountEligibility: Only reads class fields
- BuildFleetPositionInfo: Pure construction
- RegisterFleetOrderTracking: Only writes to class dictionaries
- RollbackFleetOrderTracking: Only cleans up class dictionaries

#### Check 5: No State Machine Changes
✅ **PASS**: FSM logic unchanged
- FollowerBracketFSM creation logic moved to helper
- State transitions unchanged
- FSM lookup logic unchanged

### Boundary Validation Result
**Status**: ✅ **BOUNDARY VALIDATED: YES**

**Justification**:
1. Extraction confined to single method (ProcessSingleFleetRMAAccount)
2. No signature changes or API modifications
3. No impact on callers or related methods
4. All dependencies are internal class fields (safe)
5. Helpers are pure extractions with no side effects beyond original logic

**V12.23 No Scope Creep Protocol**: ✅ **COMPLIANT**

## Success Criteria

### Primary Goals
1. **Complexity Reduction**: CYC 16 → ≤ 8
2. **Jane Street Alignment**: Achieve cognitive simplicity standard
3. **Zero Behavioral Changes**: Identical runtime behavior
4. **Test Coverage**: 100% path coverage for extracted helpers

### Verification Metrics
**Before Extraction**:
- Cyclomatic Complexity: 16
- Lines of Code: ~170
- Decision Points: 16
- Test Coverage: Unknown (needs audit)

**After Extraction**:
- Cyclomatic Complexity: ≤ 8
- Lines of Code: ~80-100 (main method)
- Decision Points: 6-8
- Test Coverage: 100% (new tests required)

### Acceptance Criteria
1. ✅ ProcessSingleFleetRMAAccount CYC ≤ 8
2. ✅ All 4 helper methods extracted and tested
3. ✅ Zero compilation errors
4. ✅ Zero behavioral regressions (existing tests pass)
5. ✅ New unit tests cover all extracted helpers
6. ✅ CSharpier formatting compliant
7. ✅ ASCII-only compliance maintained
8. ✅ No lock() blocks introduced

## Risk Assessment

### Overall Risk: LOW

#### Technical Risks
**1. Order Submission Timing (LOW)**
- Risk: Helper extraction could introduce latency
- Mitigation: Helpers are inline-eligible, no async calls
- Impact: Negligible (<1μs overhead)

**2. Dictionary Rollback Logic (MEDIUM)**
- Risk: Rollback helper could miss edge cases
- Mitigation: Comprehensive unit tests for all rollback paths
- Impact: Contained to error handling (rare path)

**3. FSM Registration Race (LOW)**
- Risk: Helper extraction could expose race condition
- Mitigation: Existing stateLock pattern preserved
- Impact: None (lock-free Actor pattern already in use)

#### Testing Risks
**1. Path Coverage (HIGH)**
- Risk: 16 paths require exhaustive testing
- Mitigation: TDD approach with path enumeration
- Impact: High effort, but necessary for correctness

**2. Integration Testing (MEDIUM)**
- Risk: Fleet execution requires multi-account setup
- Mitigation: Mock Account.All in unit tests
- Impact: Moderate (test infrastructure needed)

### Risk Mitigation Strategy
1. **TDD Approach**: Write tests before extraction
2. **Incremental Extraction**: One helper at a time
3. **Checkpoint Validation**: Build + test after each helper
4. **Rollback Plan**: Git checkpoints before each extraction
5. **Peer Review**: Arena AI adversarial audit (Phase 3)

## V12 DNA Alignment

### Current Violations
- ❌ Complexity > 15 (Jane Street threshold)
- ❌ Complexity > 8 (Jane Street HFT standard)
- ✅ No lock() blocks (Actor pattern compliant)
- ✅ ASCII-only compliant
- ✅ Atomic state updates (expectedPositions)

### Target State
- ✅ Complexity ≤ 8 (Jane Street HFT aligned)
- ✅ Single Responsibility Principle
- ✅ Make illegal states unrepresentable
- ✅ Lock-free Actor pattern
- ✅ Testable, verifiable logic

## Implementation Notes

### Extraction Order
1. **Helper 4 First**: RollbackFleetOrderTracking (simplifies catch block)
2. **Helper 1 Second**: ValidateFleetAccountEligibility (early returns)
3. **Helper 2 Third**: BuildFleetPositionInfo (pure construction)
4. **Helper 3 Last**: RegisterFleetOrderTracking (FSM logic)

### Testing Strategy
**Unit Tests Required**:
1. ValidateFleetAccountEligibility_InactiveAccount_ReturnsFalse
2. ValidateFleetAccountEligibility_ConsistencyLockExceeded_ReturnsFalse
3. ValidateFleetAccountEligibility_ValidAccount_ReturnsTrue
4. BuildFleetPositionInfo_Long_Correct5TargetDistribution
5. BuildFleetPositionInfo_Short_Correct5TargetDistribution
6. RegisterFleetOrderTracking_ValidOrder_RegistersAllDictionaries
7. RegisterFleetOrderTracking_NullOrderId_SkipsFsmMapping
8. RollbackFleetOrderTracking_FullRollback_CleansAllDictionaries
9. RollbackFleetOrderTracking_NoReservedDelta_SkipsPositionRollback
10. ProcessSingleFleetRMAAccount_Integration_FullPath

### Code Review Checklist
- [ ] All helpers are private
- [ ] No signature changes to ProcessSingleFleetRMAAccount
- [ ] No behavioral changes (identical output)
- [ ] CSharpier formatting applied
- [ ] ASCII-only compliance verified
- [ ] No lock() blocks introduced
- [ ] All dictionaries use TryAdd/TryRemove
- [ ] Expected positions use AddExpectedPositionDeltaLocked
- [ ] FSM state transitions unchanged
- [ ] Symmetry guard protocol unchanged

## Next Phase

### Phase 2: Implementation Plan
**Deliverables**:
1. Detailed extraction plan with line-by-line mapping
2. Mermaid sequence diagrams for each helper
3. Test case specifications
4. Rollback procedures

### Phase 3: DNA & PR Audit
**Deliverables**:
1. Arena AI adversarial review
2. PR health check (diff < 10k)
3. Complexity verification (CYC ≤ 8)
4. V12 DNA compliance audit

## Metadata
- **Phase**: 1 (Scope + Boundary)
- **Status**: COMPLETED
- **Analyst**: V12 Phase 1 Scope Planner
- **Date**: 2026-06-13
- **Next Phase**: Phase 2 (Implementation Plan)
- **Boundary Validated**: YES
- **Scope Creep Risk**: NONE
