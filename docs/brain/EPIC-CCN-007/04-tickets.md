# Extraction Tickets: EPIC-CCN-007

## Overview
- **Total Tickets**: 6
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3 → TICKET-4 → TICKET-5 → TICKET-6)
- **Estimated Effort**: 2-3 hours
- **Target File**: src/V12_002.SIMA.Shadow.cs
- **Target Method**: ShadowPropagateStopMoves
- **Current Complexity**: 20
- **Target Complexity**: ≤8 per method

---

## TICKET-1: Extract ValidateLeaderPosition

### Scope
- **Current Method**: `ShadowPropagateStopMoves`
- **Current CYC**: 20
- **Target CYC**: 6 (for extracted method)
- **Extraction**: Consolidate all 6 validation checks for leader positions into single helper method

### Implementation
1. Create new private method `ValidateLeaderPosition(string positionKey, PositionInfo position, out Order leaderStop)`
2. Move validation logic:
   - Check if position is null or IsFollower
   - Check if EntryFilled and RemainingContracts > 0
   - Check if stopOrders contains position key
   - Check if leaderStop is valid and StopPrice > 0
3. Return boolean indicating validation success
4. Set `out Order leaderStop` parameter for valid positions
5. Run CSharpier formatting
6. Verify compilation

### Code Template
```csharp
private bool ValidateLeaderPosition(string positionKey, PositionInfo position, out Order leaderStop)
{
    leaderStop = null;
    if (position == null || position.IsFollower)
        return false;
    if (!position.EntryFilled || position.RemainingContracts <= 0)
        return false;
    if (!stopOrders.TryGetValue(positionKey, out leaderStop))
        return false;
    if (leaderStop == null || leaderStop.StopPrice <= 0)
        return false;
    return true;
}
```

### Acceptance Criteria
- [ ] Method complexity = 6
- [ ] Method compiles without errors
- [ ] No lock() statements introduced
- [ ] ASCII-only compliance verified
- [ ] CSharpier formatting passes
- [ ] Method is private
- [ ] Returns bool + out parameter

### Dependencies
- None (first ticket)

---

## TICKET-2: Extract PropagateStopToFollowers

### Scope
- **Current Method**: `ShadowPropagateStopMoves`
- **Current CYC**: 20
- **Target CYC**: 3 (for extracted method)
- **Extraction**: Handle stop price change detection and propagation logic

### Implementation
1. Create new private method `PropagateStopToFollowers(string positionKey, double newStopPrice)`
2. Move propagation logic:
   - Get last known stop price from cache
   - Check if price delta exceeds threshold (tickSize * 0.5)
   - Call ShadowMoveFollowerStops if threshold exceeded
3. Return boolean indicating if propagation occurred
4. Run CSharpier formatting
5. Verify compilation

### Code Template
```csharp
private bool PropagateStopToFollowers(string positionKey, double newStopPrice)
{
    double lastKnown;
    _leaderLastStopPrice.TryGetValue(positionKey, out lastKnown);
    if (Math.Abs(newStopPrice - lastKnown) < tickSize * 0.5)
        return false;
    return ShadowMoveFollowerStops(positionKey, newStopPrice);
}
```

### Acceptance Criteria
- [ ] Method complexity = 3
- [ ] Method compiles without errors
- [ ] No lock() statements introduced
- [ ] ASCII-only compliance verified
- [ ] CSharpier formatting passes
- [ ] Method is private
- [ ] Returns bool indicating propagation success

### Dependencies
- TICKET-1 must be completed first

---

## TICKET-3: Extract IsStaleStopPriceCacheEntry

### Scope
- **Current Method**: `ShadowPropagateStopMoves`
- **Current CYC**: 20
- **Target CYC**: 7 (for extracted method)
- **Extraction**: Isolate cache cleanup validation logic

### Implementation
1. Create new private method `IsStaleStopPriceCacheEntry(string cacheKey)`
2. Move cleanup validation logic:
   - Check if position exists in activePositions
   - Check if position is null or IsFollower
   - Check if EntryFilled and RemainingContracts > 0
   - Check if stopOrders contains position key
   - Check if stop is valid and StopPrice > 0
3. Return boolean indicating if cache entry is stale
4. Run CSharpier formatting
5. Verify compilation

### Code Template
```csharp
private bool IsStaleStopPriceCacheEntry(string cacheKey)
{
    PositionInfo livePos;
    Order liveStop;
    return !activePositions.TryGetValue(cacheKey, out livePos)
        || livePos == null
        || livePos.IsFollower
        || !livePos.EntryFilled
        || livePos.RemainingContracts <= 0
        || !stopOrders.TryGetValue(cacheKey, out liveStop)
        || liveStop == null
        || liveStop.StopPrice <= 0;
}
```

### Acceptance Criteria
- [ ] Method complexity = 7
- [ ] Method compiles without errors
- [ ] No lock() statements introduced
- [ ] ASCII-only compliance verified
- [ ] CSharpier formatting passes
- [ ] Method is private
- [ ] Returns bool indicating staleness

### Dependencies
- TICKET-1 and TICKET-2 must be completed first

---

## TICKET-4: Refactor Orchestrator

### Scope
- **Current Method**: `ShadowPropagateStopMoves`
- **Current CYC**: 20
- **Target CYC**: 4 (for refactored orchestrator)
- **Extraction**: Replace inline logic with extracted helper method calls

### Implementation
1. Refactor propagation loop:
   - Replace validation checks with `ValidateLeaderPosition()` call
   - Replace propagation logic with `PropagateStopToFollowers()` call
   - Update cache only if propagation succeeded
2. Refactor cleanup loop:
   - Replace cleanup checks with `IsStaleStopPriceCacheEntry()` call
   - Remove stale entries using TryRemove
3. Run CSharpier formatting
4. Verify compilation
5. Run complexity audit to confirm CYC=4

### Code Template
```csharp
private void ShadowPropagateStopMoves()
{
    foreach (var kvp in activePositions.ToArray())
    {
        Order leaderStop;
        if (!ValidateLeaderPosition(kvp.Key, kvp.Value, out leaderStop))
            continue;
        if (PropagateStopToFollowers(kvp.Key, leaderStop.StopPrice))
            _leaderLastStopPrice[kvp.Key] = leaderStop.StopPrice;
    }
    foreach (var cacheKvp in _leaderLastStopPrice.ToArray())
    {
        if (IsStaleStopPriceCacheEntry(cacheKvp.Key))
            _leaderLastStopPrice.TryRemove(cacheKvp.Key, out _);
    }
}
```

### Acceptance Criteria
- [ ] Method complexity = 4
- [ ] Method compiles without errors
- [ ] No lock() statements introduced
- [ ] ASCII-only compliance verified
- [ ] CSharpier formatting passes
- [ ] Bit-identical behavior to original
- [ ] All helper methods called correctly

### Dependencies
- TICKET-1, TICKET-2, and TICKET-3 must be completed first

---

## TICKET-5: Unit Tests

### Scope
- **Test File**: tests/V12_Performance.Tests/Core/ShadowPropagationTests.cs (new file)
- **Target Coverage**: 100% of extracted methods
- **Test Count**: 19 unit tests

### Implementation

#### ValidateLeaderPosition Tests (7 tests)
1. Test: Returns false when position is null
2. Test: Returns false when position.IsFollower is true
3. Test: Returns false when position.EntryFilled is false
4. Test: Returns false when position.RemainingContracts <= 0
5. Test: Returns false when stopOrders does not contain key
6. Test: Returns false when leaderStop is null or StopPrice <= 0
7. Test: Returns true and sets out parameter for valid leader position

#### PropagateStopToFollowers Tests (4 tests)
1. Test: Returns false when price delta < threshold
2. Test: Returns false when ShadowMoveFollowerStops returns false
3. Test: Returns true when price delta >= threshold and propagation succeeds
4. Test: Handles missing cache entry (lastKnown = 0)

#### IsStaleStopPriceCacheEntry Tests (8 tests)
1. Test: Returns true when position not in activePositions
2. Test: Returns true when position is null
3. Test: Returns true when position.IsFollower is true
4. Test: Returns true when position.EntryFilled is false
5. Test: Returns true when position.RemainingContracts <= 0
6. Test: Returns true when stopOrders does not contain key
7. Test: Returns true when stop is null or StopPrice <= 0
8. Test: Returns false for valid leader position with valid stop

### Acceptance Criteria
- [ ] All 19 unit tests implemented
- [ ] All tests pass
- [ ] 100% code coverage of extracted methods
- [ ] Tests use xUnit framework
- [ ] Tests follow V12 naming conventions
- [ ] No test dependencies on external state

### Dependencies
- TICKET-4 must be completed first

---

## TICKET-6: Integration Test & Verification

### Scope
- **Test Type**: Integration test + manual verification
- **Goal**: Verify bit-identical behavior to original implementation

### Implementation

#### Integration Test
1. Create test: Full propagation cycle
   - Setup: Create leader position with followers
   - Action: Trigger stop price change
   - Assert: Verify followers receive stop updates
   - Assert: Verify cache updated correctly
   - Assert: Verify stale entries cleaned up
2. Run test and verify pass

#### Manual Verification
1. Run `powershell -File .\scripts\build_readiness.ps1`
2. Run `powershell -File .\scripts\pre_push_validation.ps1`
3. Run `powershell -File .\deploy-sync.ps1` (hard-link sync)
4. F5 test in NinjaTrader with live market data
5. Verify stop propagation behavior matches original
6. Monitor for any errors or unexpected behavior

#### Quality Gates
1. Run Codacy audit (verify no new issues)
2. Run CodeRabbit AI review
3. Run Arena AI adversarial audit (optional)
4. Director sign-off

### Acceptance Criteria
- [ ] Integration test passes
- [ ] Build succeeds (zero errors)
- [ ] All unit tests pass
- [ ] Pre-push validation passes (all 13 checks)
- [ ] Hard-link sync succeeds
- [ ] F5 manual test confirms bit-identical behavior
- [ ] No regressions detected
- [ ] Codacy reports no new issues
- [ ] CodeRabbit AI review passes
- [ ] Director approves for merge

### Dependencies
- TICKET-5 must be completed first

---

## Execution Checklist

### Pre-Execution
- [ ] Create git checkpoint: `git checkout -b epic-ccn-007-extraction`
- [ ] Verify current branch is clean
- [ ] Review architecture plan (02-architecture-plan.md)
- [ ] Review audit report (03-audit-report.md)

### During Execution
- [ ] Execute TICKET-1 (ValidateLeaderPosition)
- [ ] Execute TICKET-2 (PropagateStopToFollowers)
- [ ] Execute TICKET-3 (IsStaleStopPriceCacheEntry)
- [ ] Execute TICKET-4 (Refactor orchestrator)
- [ ] Execute TICKET-5 (Unit tests)
- [ ] Execute TICKET-6 (Integration test & verification)

### Post-Execution
- [ ] Run `powershell -File .\deploy-sync.ps1`
- [ ] Commit changes with message: "EPIC-CCN-007: Extract ShadowPropagateStopMoves helpers (CYC 20→7)"
- [ ] Push to remote
- [ ] Create pull request
- [ ] Request Director review

---

## Risk Mitigation

### Rollback Plan
- Git checkpoint created before extraction
- Instant rollback via: `git checkout main && git branch -D epic-ccn-007-extraction`

### Validation Strategy
- 19 unit tests (100% coverage)
- 1 integration test (full cycle)
- Manual F5 testing (live market data)
- Pre-push validation (13 checks)
- Arena AI adversarial audit (optional)

### Success Metrics
- Max complexity per method: ≤7 (target ≤8)
- Total complexity preserved: 20
- Zero lock() statements
- Zero behavioral changes
- Zero compilation errors
- Zero test failures

---

## Phase 5 Readiness

Upon completion of all 6 tickets:
- ✅ All extracted methods implemented
- ✅ All tests passing
- ✅ Build succeeds
- ✅ Pre-push validation passes
- ✅ Manual verification complete
- ✅ Ready for Phase 5 (Verification/Review)

**Next Phase**: Phase 5 - Verification/Review (Bob CLI verify cycle)
