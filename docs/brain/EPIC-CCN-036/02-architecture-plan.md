# Phase 2: Architecture Planning - EPIC-CCN-036

## Method Analysis

### Target Method: MoveStop_SinglePosition
- **File**: src/V12_002.Trailing.Breakeven.cs
- **Lines**: 73-165 (93 LOC)
- **Current Complexity**: 13 (CYC)
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **Tier**: 2 (Medium complexity)

### Current Method Signature
private void MoveStop_SinglePosition(
    string entryName,
    PositionInfo pos,
    double offsetPoints,
    double lastKnownPrice
)

## Complexity Analysis

### Complexity Drivers (13 decision points)
1. Direction checks (3 occurrences): pos.Direction == MarketPosition.Long
2. Price comparison logic (3 occurrences): Different logic for Long vs Short
3. Follower path (2 branches): IsFollower check + isBetterF check
4. ARM guard (3 branches): lastKnownPrice validation + priceCleared + armed state
5. Master execution (2 branches): isBetter check + final execution

### Logical Sections
1. Price Calculation (lines 80-87): Calculate and round new stop price
2. Follower Path (lines 93-107): Handle follower-specific logic with early return
3. ARM Guard (lines 111-133): Validate price threshold with early return
4. Master Execution (lines 136-163): Execute stop move for master positions

## Extraction Strategy

### Proposed Helper Methods (3 methods)

#### 1. CalculateNewStopPrice
Purpose: Isolate price calculation logic
Complexity: ~2 (1 direction check)
Lines: Extract from 80-87

Rationale:
- Single responsibility: price calculation only
- Eliminates 1 decision point from main method
- Testable in isolation (unit test: verify Long/Short calculations)

#### 2. IsPriceImprovement
Purpose: Centralize price comparison logic (used 2x in original)
Complexity: ~2 (1 direction check)
Lines: Extract duplicated logic from lines 96-98 and 138-141

Rationale:
- DRY principle: eliminates duplicated direction logic
- Reduces 2 decision points from main method (used twice)
- Testable in isolation (unit test: verify Long/Short improvement logic)
- Jane Street alignment: Make illegal states unrepresentable

#### 3. ValidatePriceCleared
Purpose: Isolate ARM guard threshold validation
Complexity: ~3 (3 branches: stale price + direction check + cleared state)
Lines: Extract from 111-133

Rationale:
- Single responsibility: threshold validation only
- Eliminates 3 decision points from main method
- Testable in isolation (unit test: verify stale price, cleared/not cleared states)
- Maintains ARM guard semantics (V12.12 feature)

## Refactored Method Structure

### New MoveStop_SinglePosition (Target CYC: ~5-6)
The refactored method will have approximately 4-5 cyclomatic complexity points:
- IsFollower check: +1
- IsPriceImprovement (follower): +1
- ValidatePriceCleared: +1
- IsPriceImprovement (master): +1
- Total: ~4-5 CYC (well under target of 8)

## Call Graph

MoveStop_SinglePosition (CYC ~5)
├── CalculateNewStopPrice (CYC ~2)
│   └── [No dependencies]
├── IsPriceImprovement (CYC ~2) [called 2x]
│   └── [No dependencies]
├── ValidatePriceCleared (CYC ~3)
│   └── [No dependencies, mutates pos state]
├── UpdateStopOrder (existing method)
└── MarkStickyDirty (existing method)

Data Flow:
1. pos + offsetPoints → CalculateNewStopPrice → newStopPrice
2. pos.Direction + newStopPrice + pos.CurrentStopPrice → IsPriceImprovement → bool
3. entryName + pos + newStopPrice + lastKnownPrice → ValidatePriceCleared → bool
4. All helpers are stateless except ValidatePriceCleared (mutates pos.ManualBreakevenArmed)

## Lock-Free Validation

### ✅ No Lock Statements
- Original method: No lock() statements present
- Helper methods: No lock() statements introduced
- Verification: grep returns zero matches

### ✅ FSM/Actor Pattern Compliance
- Method is called from OnBarUpdate() (NinjaTrader event loop)
- All state mutations via pos object (PositionInfo)
- No shared mutable state between threads
- MarkStickyDirty() uses atomic flag pattern

### ✅ Atomic Primitives Only
- pos.ManualBreakevenArmed (bool assignment - atomic on x64)
- pos.ManualBreakevenTriggered (bool assignment - atomic on x64)
- No compound operations requiring locks

## Jane Street Compliance

### Cognitive Simplicity (CYC ≤8)
- Original: 13 CYC (exceeds threshold)
- Refactored Main: ~5 CYC (well under threshold)
- Helper 1: ~2 CYC (simple calculation)
- Helper 2: ~2 CYC (simple comparison)
- Helper 3: ~3 CYC (threshold validation)
- Total Complexity: 12 CYC (distributed across 4 methods)

Jane Street Principle: Keep functions simple enough to reason about under microsecond latency constraints
- Each helper has single, clear responsibility
- No helper exceeds 3 decision points
- Main method reads like a recipe (Step 1, Step 2, etc.)

### Testability
From Jane Street KB (will_wilson_why_testing_hard_2026):
- Unit Test Isolation: Each helper can be tested independently
- Exhaustive Path Coverage: Reduced from 2^13 to 2^5 paths in main method
- Mock-Free Testing: Helpers are pure functions (except ValidatePriceCleared)

### HFT Latency Considerations
- No Additional Allocations: All helpers use stack-allocated primitives
- Inline Candidates: Small helpers (2-3 CYC) are JIT inline candidates
- Cache Locality: Helper methods co-located in same class (hot path)

## Risk Assessment

### Low Risk Factors
1. No Signature Changes: Callers unaffected
2. No New Dependencies: Helpers use existing NinjaTrader APIs
3. Behavior Preservation: Logic flow identical to original
4. Early Returns Preserved: ARM guard semantics maintained

### Mitigation Strategies
1. Unit Tests: Add tests for each helper method (Phase 3)
2. Integration Test: Verify end-to-end breakeven behavior (Phase 3)
3. Checkpointing: Bob CLI auto-checkpoint before extraction (Phase 4)
4. Rollback Plan: Git revert if NinjaTrader F5 test fails (Phase 6)

## Success Criteria

### Quantitative
- ✅ Main method complexity: ≤8 CYC (target: ~5)
- ✅ Helper method complexity: ≤3 CYC each
- ✅ Total LOC: ~100 (original: 93, +3 helpers ~30 LOC)
- ✅ Zero new lock() statements

### Qualitative
- ✅ Code reads like a recipe (Step 1, Step 2, etc.)
- ✅ Each helper has single, testable responsibility
- ✅ Direction logic centralized (DRY principle)
- ✅ ARM guard semantics preserved

### Verification
- ✅ dotnet build succeeds (Phase 5)
- ✅ python scripts/complexity_audit.py shows CYC ≤8 (Phase 5)
- ✅ NinjaTrader F5 test passes (Phase 6)
- ✅ Git diff shows isolated changes (Phase 6)

## Next Steps

### Phase 3: Implementation
1. Create unit tests for helper methods (TDD approach)
2. Extract CalculateNewStopPrice helper
3. Extract IsPriceImprovement helper
4. Extract ValidatePriceCleared helper
5. Refactor main method to use helpers
6. Run unit tests + integration tests

### Phase 4: Verification
1. Run dotnet build (zero errors)
2. Run python scripts/complexity_audit.py (verify CYC ≤8)
3. Run powershell -File .\deploy-sync.ps1 (sync NinjaTrader)
4. F5 in NinjaTrader (manual smoke test)

## Metadata
- Epic ID: EPIC-CCN-036
- Phase: 2.0 (Architecture Planning)
- Status: APPROVED
- Date: 2026-06-15
- Architect: Bob Shell (v12-engineer mode)
- Target Complexity: ≤8 CYC (Jane Street strict standard)
- Extraction Count: 3 helper methods
- Next Phase: 3.0 (Implementation)
