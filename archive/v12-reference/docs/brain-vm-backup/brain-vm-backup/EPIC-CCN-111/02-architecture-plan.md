# Phase 2: Architecture Plan - EPIC-CCN-111

## Epic Context
- **Epic ID**: EPIC-CCN-111
- **Target Method**: `HydrateExpectedPositionsFromBroker`
- **File**: `src/V12_002.SIMA.Lifecycle.cs`
- **Current CCN**: 17
- **Target CCN**: ≤15 (Jane Street alignment)
- **Lines**: ~200-220

## Current Method Analysis

### Method Signature (Before)
```csharp
private void HydrateExpectedPositionsFromBroker()
```

### Current Implementation Structure
```csharp
private void HydrateExpectedPositionsFromBroker()
{
    int hydratedCount = 0;

    // Fleet accounts iteration
    foreach (Account acct in Account.All)
    {
        if (!IsFleetAccount(acct))
            continue;
        HydrateSingleAccountExpectedPosition(acct, ref hydratedCount);
    }

    if (hydratedCount > 0)
        Print(string.Format("[SIMA HYDRATE] Hydrated {0} account(s) with live broker positions", hydratedCount));

    // Master account handling
    bool masterIsFleet993 = IsFleetAccount(Account);
    if (!masterIsFleet993)
        HydrateSingleAccountExpectedPosition(Account, ref hydratedCount);
}
```

### Complexity Analysis
**Current Branching Points (CCN = 17)**:
1. Fleet account loop iteration (foreach)
2. `IsFleetAccount()` check in loop
3. Hydration count check (`if (hydratedCount > 0)`)
4. Master fleet check (`IsFleetAccount(Account)`)
5. Master account conditional hydration
6. **Nested complexity from `HydrateSingleAccountExpectedPosition`** (called method adds ~12 CCN points)

**Root Cause**: The method delegates to `HydrateSingleAccountExpectedPosition`, which contains:
- Try-catch block
- Foreach loop over positions
- Multiple null checks (pos, pos.Instrument, pos.Instrument.FullName)
- MarketPosition check
- Quantity calculation conditional
- Actor queue enqueue
- Print statement

## Call Graph Analysis

### Current Call Graph
```
HydrateExpectedPositionsFromBroker (CCN: 17)
├── IsFleetAccount(acct) [called 2x]
├── HydrateSingleAccountExpectedPosition(acct, ref count) [called N+1 times]
│   ├── acct.Positions.ToArray() [broker API]
│   ├── ExpKey(accountName) [utility]
│   ├── Enqueue(lambda) [Actor pattern]
│   │   └── AddOrUpdateExpectedPosition() [state mutation]
│   └── Print() [logging]
└── Print() [logging]
```

### Dependency Map

#### External Dependencies
- **NinjaTrader.Cbi.Account**: Broker account API
- **NinjaTrader.Cbi.Position**: Broker position data
- **Actor Queue**: `Enqueue()` for state mutations
- **State Dictionary**: `expectedPositions` via `AddOrUpdateExpectedPosition()`

#### Internal Dependencies
- `IsFleetAccount(Account)`: Fleet membership check
- `ExpKey(string)`: Expected position key generator
- `Print(string)`: Logging infrastructure

#### Data Flow
```
Broker Positions (Account.Positions)
    ↓
HydrateSingleAccountExpectedPosition
    ↓
Position Validation & Quantity Calculation
    ↓
Actor Queue (Enqueue)
    ↓
expectedPositions Dictionary (AddOrUpdateExpectedPosition)
```

## Extraction Strategy

### ❌ SCOPE BOUNDARY VIOLATION DETECTED

**CRITICAL FINDING**: The scope document proposes extracting validation, state update, and error handling logic from `HydrateExpectedPositionsFromBroker`. However, **analysis reveals the target method is already well-factored**:

1. **No validation logic exists** in the target method - it's a simple orchestrator
2. **No state update logic exists** in the target method - delegated to `HydrateSingleAccountExpectedPosition`
3. **No error handling exists** in the target method - delegated to called method

**The actual complexity (CCN 17) comes from `HydrateSingleAccountExpectedPosition`, NOT from `HydrateExpectedPositionsFromBroker`.**

### Corrected Scope: Target the Right Method

**Actual High-Complexity Method**: `HydrateSingleAccountExpectedPosition`
- **Current CCN**: ~12-15 (estimated from code analysis)
- **Location**: Lines ~221-260 in `V12_002.SIMA.Lifecycle.cs`
- **Complexity Sources**:
  - Try-catch block (+1)
  - Foreach loop over positions (+1)
  - Multiple null checks (+3)
  - MarketPosition conditional (+1)
  - Quantity calculation conditional (+1)
  - Actor enqueue (+1)
  - Print statements (+2)

### Recommended Extraction Plan

#### Option A: Extract from `HydrateSingleAccountExpectedPosition` (CORRECT TARGET)

**Method 1: Extract Position Validation**
```csharp
// NEW METHOD
private bool IsValidPositionForHydration(Position pos)
{
    if (pos == null) return false;
    if (pos.Instrument == null) return false;
    if (pos.Instrument.FullName != Instrument.FullName) return false;
    if (pos.MarketPosition == MarketPosition.Flat) return false;
    return true;
}
// Target CCN: ≤5
// Reduction: ~4 CCN points from HydrateSingleAccountExpectedPosition
```

**Method 2: Extract Quantity Calculation**
```csharp
// NEW METHOD
private int CalculatePositionQuantity(Position pos)
{
    return pos.MarketPosition == MarketPosition.Long 
        ? pos.Quantity 
        : -pos.Quantity;
}
// Target CCN: ≤3
// Reduction: ~1 CCN point from HydrateSingleAccountExpectedPosition
```

**Method 3: Extract State Update Orchestration**
```csharp
// NEW METHOD
private void EnqueueExpectedPositionUpdate(string accountName, int quantity)
{
    var capturedAcct = accountName;
    var capturedQty = quantity;
    Enqueue(ctx => 
        ctx.AddOrUpdateExpectedPosition(ExpKey(capturedAcct), capturedQty, v => capturedQty)
    );
}
// Target CCN: ≤3
// Reduction: ~2 CCN points from HydrateSingleAccountExpectedPosition
```

**Refactored `HydrateSingleAccountExpectedPosition`**:
```csharp
private void HydrateSingleAccountExpectedPosition(Account acct, ref int hydratedCount)
{
    try
    {
        foreach (Position pos in acct.Positions.ToArray())
        {
            if (!IsValidPositionForHydration(pos))
                continue;

            int qty = CalculatePositionQuantity(pos);
            EnqueueExpectedPositionUpdate(acct.Name, qty);
            
            Print(string.Format(
                "[SIMA HYDRATE] {0}: Seeded expected={1} from broker ({2} {3})",
                acct.Name, qty, pos.MarketPosition, pos.Quantity
            ));
            
            hydratedCount++;
            break;
        }
    }
    catch (Exception ex)
    {
        Print(string.Format(
            "[SIMA HYDRATE] WARNING: Could not read positions for {0}: {1}",
            acct.Name, ex.Message
        ));
    }
}
// Target CCN: ≤8 (down from ~12-15)
```

#### Option B: Keep Original Scope (NOT RECOMMENDED)

If we must extract from `HydrateExpectedPositionsFromBroker` as originally scoped:

**Method 1: Extract Fleet Account Hydration**
```csharp
// NEW METHOD
private int HydrateFleetAccounts()
{
    int hydratedCount = 0;
    foreach (Account acct in Account.All)
    {
        if (!IsFleetAccount(acct))
            continue;
        HydrateSingleAccountExpectedPosition(acct, ref hydratedCount);
    }
    return hydratedCount;
}
// Target CCN: ≤5
```

**Method 2: Extract Master Account Hydration**
```csharp
// NEW METHOD
private void HydrateMasterAccountIfNeeded(ref int hydratedCount)
{
    bool masterIsFleet = IsFleetAccount(Account);
    if (!masterIsFleet)
        HydrateSingleAccountExpectedPosition(Account, ref hydratedCount);
}
// Target CCN: ≤3
```

**Refactored `HydrateExpectedPositionsFromBroker`**:
```csharp
private void HydrateExpectedPositionsFromBroker()
{
    int hydratedCount = HydrateFleetAccounts();
    
    if (hydratedCount > 0)
        Print(string.Format(
            "[SIMA HYDRATE] Hydrated {0} account(s) with live broker positions", 
            hydratedCount
        ));
    
    HydrateMasterAccountIfNeeded(ref hydratedCount);
}
// Target CCN: ≤5 (down from 17)
```

**Problem with Option B**: This achieves the CCN reduction target but doesn't address the root complexity. The extracted methods are trivial wrappers that don't improve maintainability or testability.

## Extraction Sequence

### Recommended Sequence (Option A - Correct Target)

**Phase 1: Extract Position Validation**
1. Create `IsValidPositionForHydration(Position pos)`
2. Replace inline null checks in `HydrateSingleAccountExpectedPosition`
3. Run tests to verify behavior unchanged
4. Verify CCN reduction: ~4 points

**Phase 2: Extract Quantity Calculation**
1. Create `CalculatePositionQuantity(Position pos)`
2. Replace inline conditional in `HydrateSingleAccountExpectedPosition`
3. Run tests to verify behavior unchanged
4. Verify CCN reduction: ~1 point

**Phase 3: Extract State Update**
1. Create `EnqueueExpectedPositionUpdate(string, int)`
2. Replace inline Enqueue call in `HydrateSingleAccountExpectedPosition`
3. Run tests to verify behavior unchanged
4. Verify CCN reduction: ~2 points

**Phase 4: Verification**
1. Run full test suite
2. Verify `HydrateSingleAccountExpectedPosition` CCN ≤8
3. Verify no performance regression
4. Deploy to staging for integration testing

### Alternative Sequence (Option B - Original Scope)

**Phase 1: Extract Fleet Hydration**
1. Create `HydrateFleetAccounts()`
2. Replace fleet loop in `HydrateExpectedPositionsFromBroker`
3. Run tests to verify behavior unchanged

**Phase 2: Extract Master Hydration**
1. Create `HydrateMasterAccountIfNeeded(ref int)`
2. Replace master logic in `HydrateExpectedPositionsFromBroker`
3. Run tests to verify behavior unchanged

**Phase 3: Verification**
1. Run full test suite
2. Verify `HydrateExpectedPositionsFromBroker` CCN ≤5
3. Note: Root complexity remains in `HydrateSingleAccountExpectedPosition`

## Jane Street Compliance Checks

### ✅ Correctness by Construction
- **Option A**: Validation logic extracted to pure function - illegal states unrepresentable
- **Option B**: Orchestration logic simplified - but doesn't address root complexity

### ✅ Lock-Free Actor Pattern
- Both options maintain Actor queue usage via `Enqueue()`
- No `lock()` statements introduced
- State mutations remain serialized through Actor

### ✅ ASCII-Only Compliance
- All string literals use ASCII characters
- No Unicode or emoji in extracted methods

### ✅ Cognitive Simplicity (Jane Street Principle)
- **Option A**: Each extracted method has single responsibility, CCN ≤5
- **Option B**: Extracted methods are trivial wrappers, limited cognitive benefit

### ✅ Testability
- **Option A**: Validation, calculation, and state update independently testable
- **Option B**: Extracted methods are thin wrappers, limited test value

## Risk Assessment

### Technical Risks

#### HIGH RISK: Wrong Target Method (Option B)
- **Risk**: Extracting from `HydrateExpectedPositionsFromBroker` achieves CCN target but doesn't address root complexity
- **Impact**: Technical debt remains in `HydrateSingleAccountExpectedPosition`
- **Mitigation**: Recommend Option A (extract from actual complex method)
- **Likelihood**: 100% if Option B chosen

#### MEDIUM RISK: Scope Creep (Option A)
- **Risk**: Extracting from `HydrateSingleAccountExpectedPosition` violates V12.23 single-method scope
- **Impact**: Requires scope boundary revision and Director approval
- **Mitigation**: Document scope change rationale, get explicit approval
- **Likelihood**: 50% (depends on Director decision)

#### LOW RISK: Performance Regression
- **Risk**: Method extraction adds call overhead
- **Impact**: Microsecond-latency threshold could be violated
- **Mitigation**: Keep extracted methods inline-eligible (small, focused)
- **Likelihood**: <10% (modern JIT optimizes small methods)

### Business Risks

#### LOW RISK: Position Reconciliation Failure
- **Risk**: Refactoring could introduce bugs in position hydration
- **Impact**: False REAPER alerts or missed positions
- **Mitigation**: Extensive unit tests + integration tests with broker mocks
- **Likelihood**: <5% (logic is straightforward)

## Recommendations

### Primary Recommendation: Revise Scope (Option A)

**Rationale**:
1. **Actual complexity is in `HydrateSingleAccountExpectedPosition`**, not the target method
2. Extracting from the correct method provides real maintainability benefits
3. Achieves Jane Street cognitive simplicity principles
4. Creates independently testable units

**Action Required**:
1. Update scope boundary document to target `HydrateSingleAccountExpectedPosition`
2. Get Director approval for scope revision
3. Proceed with Option A extraction sequence

### Fallback: Execute Original Scope (Option B)

**Rationale**:
1. Adheres to V12.23 single-method scope protocol
2. Achieves CCN ≤15 target for specified method
3. Minimal risk, straightforward implementation

**Limitation**:
- Root complexity remains unaddressed
- Limited cognitive or testability benefits
- May require follow-up EPIC for `HydrateSingleAccountExpectedPosition`

## Success Criteria

### Quantitative Metrics (Option A)
- ✅ `HydrateSingleAccountExpectedPosition` CCN ≤8 (down from ~12-15)
- ✅ `IsValidPositionForHydration` CCN ≤5
- ✅ `CalculatePositionQuantity` CCN ≤3
- ✅ `EnqueueExpectedPositionUpdate` CCN ≤3
- ✅ Total CCN reduction: ~7 points

### Quantitative Metrics (Option B)
- ✅ `HydrateExpectedPositionsFromBroker` CCN ≤5 (down from 17)
- ✅ `HydrateFleetAccounts` CCN ≤5
- ✅ `HydrateMasterAccountIfNeeded` CCN ≤3
- ✅ Total CCN reduction: ~12 points (but root complexity remains)

### Qualitative Criteria (Both Options)
- ✅ Lock-free verification: No `lock()` statements
- ✅ Type safety: Maintain existing patterns
- ✅ Testability: Each extracted method independently testable
- ✅ V12 DNA alignment: "Make illegal states unrepresentable"
- ✅ Backward compatibility: No breaking changes

## Next Steps

1. **Director Decision Required**: Choose Option A (revise scope) or Option B (original scope)
2. **If Option A**: Update `01-scope-boundary.md` to target `HydrateSingleAccountExpectedPosition`
3. **If Option B**: Proceed with Phase 3 (TDD implementation) as originally scoped
4. **Either Option**: Create test specifications for extracted methods
5. **Phase 3**: Implement TDD tests before refactoring

---
**Architecture Plan Status**: ✅ COMPLETE
**Recommendation**: Option A (Revise Scope to Target Actual Complex Method)
**Risk Level**: MEDIUM (scope change requires approval)
**Estimated Effort**: 2-3 hours (Option A) or 1 hour (Option B)