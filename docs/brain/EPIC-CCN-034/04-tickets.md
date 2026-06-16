# Extraction Tickets: EPIC-CCN-034

## Overview
- **Epic ID**: EPIC-CCN-034
- **Target Method**: ManageCIT
- **File**: src/V12_002.Orders.Management.Flatten.cs
- **Total Tickets**: 4
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3 → TICKET-4)
- **Estimated Effort**: 2-3 hours (extraction + tests + verification)
- **Current Complexity**: CYC 19
- **Target Complexity**: Max CYC 6 per method (≤8 Jane Street threshold)

---

## TICKET-1: Extract ValidateCITPrerequisites

### Scope
- **Current Method**: `ManageCIT`
- **Current CYC**: 19
- **Target CYC**: 4 (extracted method)
- **Extraction**: Early validation and CIT offset parsing logic

### Implementation
1. Create new private method `ValidateCITPrerequisites()` returning `double`
2. Extract early validation logic:
   - Check `activePositions.Count` and `entryOrders.Count`
   - Validate `ChaseIfTouchPoints` configuration
   - Check `_propagationActive` flag (BUILD 924 Fix C)
3. Extract CIT offset parsing from string configuration
4. Return parsed offset value (or 0.0 on validation failure)
5. Update `ManageCIT` to call `ValidateCITPrerequisites()` at start
6. Preserve all early return paths

### Method Signature
```csharp
private double ValidateCITPrerequisites()
```

### Acceptance Criteria
- [ ] Method complexity CYC = 4
- [ ] All early validation paths preserved
- [ ] BUILD 924 Fix C (_propagationActive) validated
- [ ] CIT offset parsing logic extracted
- [ ] Returns 0.0 on validation failure (early exit)
- [ ] Returns parsed offset on success
- [ ] No behavioral changes (bit-identical)
- [ ] All existing tests pass
- [ ] Build succeeds with zero errors
- [ ] CSharpier formatting applied

### Test Coverage Required
- [ ] Test: activePositions.Count == 0 → returns 0.0
- [ ] Test: entryOrders.Count == 0 → returns 0.0
- [ ] Test: ChaseIfTouchPoints null → returns 0.0
- [ ] Test: _propagationActive == true → returns 0.0
- [ ] Test: Valid config → returns parsed offset
- [ ] Test: Invalid offset string → returns 0.0

### Dependencies
- None (first ticket)

### Estimated Time
- Implementation: 20 minutes
- Testing: 15 minutes
- Verification: 10 minutes
- **Total**: 45 minutes

---

## TICKET-2: Extract ShouldNudgeOrder

### Scope
- **Current Method**: `ManageCIT` (after TICKET-1)
- **Current CYC**: 15 (reduced from 19)
- **Target CYC**: 6 (extracted method)
- **Extraction**: Order state validation and price trigger logic

### Implementation
1. Create new private method `ShouldNudgeOrder(Order order, string orderKey)` returning `bool`
2. Extract order validation logic:
   - Check order state (Working only)
   - Check order type (Limit only)
   - Check if already nudged (via _nudgedOrders dictionary)
3. Extract BUILD 984 directional price trigger logic:
   - Long position: order.LimitPrice < currentPrice - citOffset
   - Short position: order.LimitPrice > currentPrice + citOffset
4. Return `true` if order should be nudged, `false` otherwise
5. Update `ManageCIT` to call `ShouldNudgeOrder()` in order loop
6. Preserve all conditional logic paths

### Method Signature
```csharp
private bool ShouldNudgeOrder(Order order, string orderKey)
```

### Acceptance Criteria
- [ ] Method complexity CYC = 6
- [ ] Order state validation extracted (Working only)
- [ ] Order type validation extracted (Limit only)
- [ ] Already-nudged check extracted
- [ ] BUILD 984 directional logic preserved
- [ ] Returns false for invalid orders
- [ ] Returns true for valid nudge candidates
- [ ] No behavioral changes (bit-identical)
- [ ] All existing tests pass
- [ ] Build succeeds with zero errors
- [ ] CSharpier formatting applied

### Test Coverage Required
- [ ] Test: Order state != Working → returns false
- [ ] Test: Order type != Limit → returns false
- [ ] Test: Already nudged → returns false
- [ ] Test: Long position, price above trigger → returns false
- [ ] Test: Long position, price below trigger → returns true
- [ ] Test: Short position, price below trigger → returns false
- [ ] Test: Short position, price above trigger → returns true
- [ ] Test: Edge case - price exactly at trigger

### Dependencies
- TICKET-1 must be completed first

### Estimated Time
- Implementation: 25 minutes
- Testing: 20 minutes
- Verification: 10 minutes
- **Total**: 55 minutes

---

## TICKET-3: Extract ExecuteCITNudge

### Scope
- **Current Method**: `ManageCIT` (after TICKET-2)
- **Current CYC**: 9 (reduced from 15)
- **Target CYC**: 5 (extracted method)
- **Extraction**: Nudge calculation and execution logic

### Implementation
1. Create new private method `ExecuteCITNudge(Order order, string orderKey, double citOffset, ref int brokerBudget)` returning `bool`
2. Extract follower determination logic:
   - Check if order is follower (via _followerOrders dictionary)
3. Extract nudge calculation:
   - Calculate nudge distance (currentPrice ± citOffset)
   - Calculate new limit price
4. Extract nudge execution:
   - Follower: Call `NudgeFollowerOrder()`
   - Local: Call `NudgeLocalOrder()`
5. Extract broker budget management (BUILD 1109):
   - Decrement `brokerBudget` via ref parameter
6. Mark order as nudged in `_nudgedOrders` dictionary
7. Return `true` on successful nudge, `false` on failure
8. Update `ManageCIT` to call `ExecuteCITNudge()` after validation

### Method Signature
```csharp
private bool ExecuteCITNudge(Order order, string orderKey, double citOffset, ref int brokerBudget)
```

### Acceptance Criteria
- [ ] Method complexity CYC = 5
- [ ] Follower determination logic extracted
- [ ] Nudge distance calculation extracted
- [ ] Follower nudge path preserved
- [ ] Local nudge path preserved
- [ ] BUILD 1109 broker budget management preserved
- [ ] Order marked as nudged on success
- [ ] Returns true on successful nudge
- [ ] Returns false on failure
- [ ] No behavioral changes (bit-identical)
- [ ] All existing tests pass
- [ ] Build succeeds with zero errors
- [ ] CSharpier formatting applied

### Test Coverage Required
- [ ] Test: Follower order → calls NudgeFollowerOrder()
- [ ] Test: Local order → calls NudgeLocalOrder()
- [ ] Test: Broker budget decremented correctly
- [ ] Test: Order marked as nudged on success
- [ ] Test: Long position nudge calculation
- [ ] Test: Short position nudge calculation
- [ ] Test: Nudge failure handling

### Dependencies
- TICKET-1 must be completed first
- TICKET-2 must be completed first

### Estimated Time
- Implementation: 30 minutes
- Testing: 25 minutes
- Verification: 10 minutes
- **Total**: 65 minutes

---

## TICKET-4: Refactor ManageCIT Orchestrator

### Scope
- **Current Method**: `ManageCIT` (after TICKET-3)
- **Current CYC**: 4 (reduced from 9)
- **Target CYC**: 5 (orchestrator only)
- **Extraction**: Final orchestration cleanup

### Implementation
1. Simplify `ManageCIT` to orchestration-only logic:
   - Call `ValidateCITPrerequisites()` → early exit if 0.0
   - Loop through `entryOrders`
   - Call `ShouldNudgeOrder()` → skip if false
   - Call `ExecuteCITNudge()` → continue on success
2. Remove all extracted logic (now in helper methods)
3. Preserve broker budget loop exit condition
4. Preserve error handling and logging
5. Verify final complexity CYC = 5

### Final Method Structure
```csharp
private void ManageCIT()
{
    // 1. Validate prerequisites
    double citOffset = ValidateCITPrerequisites();
    if (citOffset == 0.0) return;

    // 2. Process orders
    int brokerBudget = 10;
    foreach (var kvp in entryOrders)
    {
        if (brokerBudget <= 0) break;
        
        Order order = kvp.Value;
        string orderKey = kvp.Key;

        // 3. Check if order should be nudged
        if (!ShouldNudgeOrder(order, orderKey)) continue;

        // 4. Execute nudge
        ExecuteCITNudge(order, orderKey, citOffset, ref brokerBudget);
    }
}
```

### Acceptance Criteria
- [ ] Method complexity CYC = 5
- [ ] Orchestration-only logic (no business logic)
- [ ] All helper methods called correctly
- [ ] Broker budget loop exit preserved
- [ ] Error handling preserved
- [ ] No behavioral changes (bit-identical)
- [ ] All existing tests pass
- [ ] Build succeeds with zero errors
- [ ] CSharpier formatting applied
- [ ] Hard-link sync completed (`deploy-sync.ps1`)

### Test Coverage Required
- [ ] Integration test: Full CIT nudge cycle
- [ ] Test: Early exit on validation failure
- [ ] Test: Broker budget exhaustion
- [ ] Test: Multiple orders processed correctly
- [ ] Test: Order skipping logic
- [ ] Test: Nudge success/failure handling

### Dependencies
- TICKET-1 must be completed first
- TICKET-2 must be completed first
- TICKET-3 must be completed first

### Estimated Time
- Implementation: 15 minutes
- Testing: 20 minutes
- Verification: 15 minutes
- **Total**: 50 minutes

---

## Execution Summary

### Sequential Order
1. **TICKET-1**: Extract ValidateCITPrerequisites (45 min)
2. **TICKET-2**: Extract ShouldNudgeOrder (55 min)
3. **TICKET-3**: Extract ExecuteCITNudge (65 min)
4. **TICKET-4**: Refactor ManageCIT Orchestrator (50 min)

### Total Estimated Time
- **Implementation**: 90 minutes
- **Testing**: 80 minutes
- **Verification**: 45 minutes
- **Total**: 215 minutes (~3.5 hours)

### Complexity Targets
| Method | Before | After | Status |
|--------|--------|-------|--------|
| ManageCIT | 19 | 5 | ✅ Target met |
| ValidateCITPrerequisites | - | 4 | ✅ Target met |
| ShouldNudgeOrder | - | 6 | ✅ Target met |
| ExecuteCITNudge | - | 5 | ✅ Target met |
| **Max CYC** | 19 | 6 | ✅ ≤8 threshold |

---

## Final Verification Checklist

### Pre-Push Validation
- [ ] Run `powershell -File .\scripts\pre_push_validation.ps1 -Fast`
- [ ] ASCII-only compliance verified
- [ ] Build succeeds with zero errors
- [ ] All unit tests pass (20+ tests)
- [ ] Lint audit passes
- [ ] CSharpier formatting applied
- [ ] Complexity audit passes (CYC ≤ 15)

### Manual Testing
- [ ] F5 test in NinjaTrader
- [ ] Verify CIT nudge behavior with live market data
- [ ] Verify BUILD 984 directional logic
- [ ] Verify BUILD 924 Fix C (_propagationActive)
- [ ] Verify BUILD 1109 broker budget management

### Post-Implementation
- [ ] Run `powershell -File .\deploy-sync.ps1` (hard-link sync)
- [ ] Verify BUILD_TAG in NinjaTrader
- [ ] Create git checkpoint
- [ ] Update manifest.json (Phase 4 completed)

---

## Risk Mitigation

### Rollback Plan
- Git checkpoint created before extraction
- Instant rollback via `git reset --hard <checkpoint>`
- Hard-link sync restores NinjaTrader state

### Testing Strategy
- Unit tests for each extracted method (20+ tests)
- Integration test for full CIT nudge cycle
- Manual F5 testing in NinjaTrader
- BUILD-specific regression tests

### Success Criteria
- ✅ All tickets completed sequentially
- ✅ Max complexity CYC = 6 (≤8 threshold)
- ✅ Zero lock() statements
- ✅ ASCII-only compliance
- ✅ Bit-identical behavior
- ✅ All tests pass
- ✅ Build succeeds
- ✅ Hard-link sync completed

---

**Phase 4 Status**: ✅ COMPLETED  
**Next Phase**: Phase 5 (Ticket Execution)  
**Generated**: 2026-06-15  
**Protocol Version**: V12.23
