# Phase 2: Architecture Planning - EPIC-CCN-007

## Epic Metadata
- **Epic ID**: EPIC-CCN-007
- **Target Method**: ShadowPropagateStopMoves
- **File**: src/V12_002.SIMA.Shadow.cs
- **Current Complexity**: 20
- **Target Complexity**: ≤8 per method
- **Phase**: 2 - Architecture Planning
- **Date**: 2026-06-15

## 1. Current Method Analysis

### Method Signature
```csharp
private void ShadowPropagateStopMoves()
```

### Current Structure (48 LOC, CYC=20)
The method contains two distinct responsibilities:

1. **Propagation Loop** (Lines 35-60, ~26 LOC)
   - Iterates through activePositions
   - Validates leader position state (6 conditional checks)
   - Checks for stop price changes
   - Propagates stop moves to followers
   - Updates leader stop price cache

2. **Cleanup Loop** (Lines 62-78, ~17 LOC)
   - Iterates through _leaderLastStopPrice cache
   - Validates position/stop still exists (7 conditional checks)
   - Removes stale cache entries

### Complexity Breakdown
- **Nested loops**: 2 (foreach loops)
- **Conditional branches**: 13 (if/continue statements)
- **Dictionary lookups**: 4 (TryGetValue calls)
- **Method calls**: 1 (ShadowMoveFollowerStops)

### Cognitive Load Factors
- Multiple validation checks scattered throughout
- Mixed concerns (propagation + cleanup)
- Deep nesting (loop → multiple if statements)
- State mutations across multiple dictionaries

## 2. Extraction Strategy

### Design Principle: Single Responsibility
Extract three helper methods, each with a single, well-defined purpose:

1. **ValidateLeaderPosition**: Consolidate all leader position validation logic
2. **PropagateStopToFollowers**: Handle stop price propagation logic
3. **IsStaleStopPriceCacheEntry**: Isolate cache cleanup logic

### Complexity Distribution Target
- **ShadowPropagateStopMoves** (orchestrator): CYC ≤ 4
- **ValidateLeaderPosition**: CYC ≤ 6
- **PropagateStopToFollowers**: CYC ≤ 3
- **IsStaleStopPriceCacheEntry**: CYC ≤ 7

**Total Complexity**: 4 + 6 + 3 + 7 = 20 (preserved, but distributed)
**Max Per-Method Complexity**: 7 (within Jane Street ≤8 threshold)

## 3. Proposed Method Signatures

### Helper Method 1: Validation
```csharp
private bool ValidateLeaderPosition(
    string positionKey,
    PositionInfo position,
    out Order leaderStop
)
```

**Responsibility**: Consolidate all 6 validation checks for leader positions
**Complexity**: CYC = 6

### Helper Method 2: Propagation
```csharp
private bool PropagateStopToFollowers(
    string positionKey,
    double newStopPrice
)
```

**Responsibility**: Handle stop price change detection and propagation
**Complexity**: CYC = 3

### Helper Method 3: Cleanup
```csharp
private bool IsStaleStopPriceCacheEntry(string cacheKey)
```

**Responsibility**: Determine if a cache entry is stale
**Complexity**: CYC = 7

## 4. Call Graph & Data Flow

### Orchestrator Flow
```
ShadowPropagateStopMoves()
├─> foreach (activePositions)
│   ├─> ValidateLeaderPosition(key, pos, out stop)
│   └─> PropagateStopToFollowers(key, stop.StopPrice)
└─> foreach (_leaderLastStopPrice)
    └─> IsStaleStopPriceCacheEntry(key)
```

### Thread Safety
- All dictionaries are ConcurrentDictionary (lock-free)
- ToArray() creates snapshot for safe iteration
- TryGetValue/TryRemove are atomic operations
- No lock() statements introduced

## 5. Implementation Plan

### Step 1: Extract ValidateLeaderPosition
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

### Step 2: Extract PropagateStopToFollowers
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

### Step 3: Extract IsStaleStopPriceCacheEntry
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

### Step 4: Refactor Orchestrator
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

## 6. Lock-Free Validation

### ✅ No Lock Statements
- Zero lock() blocks in original method
- Zero lock() blocks in extracted methods
- All synchronization via ConcurrentDictionary atomic operations

### ✅ FSM/Actor Pattern Compliance
- Method is called from FSM event handler
- No shared mutable state outside ConcurrentDictionary
- All state transitions are atomic

### ✅ Atomic Primitives Only
- ConcurrentDictionary.TryGetValue() - atomic read
- ConcurrentDictionary.TryRemove() - atomic remove
- ConcurrentDictionary[key] = value - atomic write

## 7. Jane Street Compliance

### Cognitive Simplicity
**Before**: CYC=20, 48 LOC, 2 nested loops with 13 conditionals
**After**: Max CYC=7 per method, clear single responsibilities

**Jane Street Principle**: Functions with CYC >15 are harder to reason about under microsecond latency
- ✅ All methods ≤8 complexity threshold
- ✅ Each method has single, verifiable purpose
- ✅ Reduced cognitive load for auditing race conditions

### Testing Requirements
**Unit Tests Required** (per extracted method):
1. ValidateLeaderPosition: 7 test cases
2. PropagateStopToFollowers: 4 test cases
3. IsStaleStopPriceCacheEntry: 8 test cases

**Integration Test Required**:
- Test full propagation cycle
- Verify bit-identical behavior vs original

## 8. Risk Assessment

### Low Risk Factors
- ✅ No signature changes
- ✅ No new dependencies
- ✅ No lock-free pattern violations
- ✅ Bit-identical logic

### Medium Risk Factors
- ⚠️ Mission-critical stop loss logic
- ⚠️ Multiple dictionary interactions
- ⚠️ Cache consistency

### Mitigation Strategies
1. Comprehensive unit tests (100% coverage)
2. Integration tests
3. Manual F5 testing in NinjaTrader
4. Arena AI adversarial audit
5. Git checkpoint before extraction

## 9. Success Criteria

### Functional Requirements
- [ ] All extracted methods compile
- [ ] All unit tests pass
- [ ] Integration tests verify bit-identical behavior
- [ ] Manual testing confirms stop propagation
- [ ] No regressions

### Non-Functional Requirements
- [ ] ShadowPropagateStopMoves complexity ≤ 4
- [ ] ValidateLeaderPosition complexity ≤ 6
- [ ] PropagateStopToFollowers complexity ≤ 3
- [ ] IsStaleStopPriceCacheEntry complexity ≤ 7
- [ ] No lock() statements
- [ ] ASCII-only compliance
- [ ] CSharpier formatting passes

### Quality Gates
- [ ] Codacy reports no new issues
- [ ] CodeRabbit AI review passes
- [ ] Pre-push validation passes
- [ ] Arena AI adversarial audit approves
- [ ] Director sign-off after F5 test

## 10. Next Steps (Phase 3)

### Phase 3: DNA & PR Audit (Adjudicator)
**Agent**: Arena AI (Red Team)
**Input**: This architecture plan
**Output**: PASS/FAIL decision

**Audit Checklist**:
1. Verify no lock() statements
2. Verify complexity targets ≤8
3. Verify ASCII-only compliance
4. Verify Jane Street alignment
5. Verify testing requirements
6. Verify risk mitigation
7. Check for scope creep

**Gate**: PASS required to proceed to Phase 4

### Phase 4: Implementation (Engineer)
**Agent**: Bob CLI (v12-engineer) or Codex CLI (codex-rescue)
**Actions**:
1. Extract three helper methods
2. Refactor orchestrator
3. Run CSharpier formatting
4. Run pre-push validation
5. Create unit tests
6. Create integration test
7. Manual F5 test

**Output**: Pull request ready for Phase 5 review

## Appendix: Complexity Calculation

### Original Method (CYC=20)
- Base: 1
- foreach loop: 1
- 6 validation checks: 8
- price change check: 1
- propagation check: 1
- foreach loop: 1
- 7 cleanup checks: 7
- Total: 20

### Extracted Methods
- ValidateLeaderPosition: 6
- PropagateStopToFollowers: 3
- IsStaleStopPriceCacheEntry: 7
- ShadowPropagateStopMoves: 4
- Total: 20 (preserved, distributed)
- Max: 7 (within Jane Street ≤8 threshold)
