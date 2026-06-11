# Phase 1: Scope Definition - EPIC-CCN-128

## Epic Metadata
- **Epic ID**: EPIC-CCN-128
- **Target Method**: `SymmetryGuardReplaceExistingFollowerTarget`
- **File**: `src/V12_002.Symmetry.Replace.cs`
- **Lines**: 23-88 (66 lines)
- **Current CYC**: 18
- **Target CYC**: ≤8 (56% reduction)
- **Risk Level**: MEDIUM

## Method Purpose
Manages follower target order replacement in the Symmetry module. Handles two scenarios:
1. **Stale Target Cleanup**: Cancels and removes filled/runner/zero-quantity targets
2. **Active Target Replacement**: Uses two-phase FSM to replace working target orders

## Complexity Drivers

### Primary Drivers (CYC Contributors)
1. **Early Exit Guards** (CYC +4):
   - Null account check
   - Filled target check
   - Runner target check
   - Zero quantity check

2. **Stale Target Cleanup Path** (CYC +4):
   - Order state validation (4 states: Working, Accepted, Submitted, ChangePending)
   - Dictionary removal logic

3. **Active Target Replacement Path** (CYC +4):
   - Old target existence check
   - Order state validation (4 states again)
   - Price validation
   - FSM spec creation

4. **Nested Conditionals** (CYC +6):
   - Multiple if/else branches
   - Compound boolean conditions

## Extraction Strategy

### Proposed Extractions

#### 1. `ShouldSkipTargetReplacement` (CYC ≤3)
**Purpose**: Consolidate early exit logic
**Lines**: 30-32
**Logic**:
```csharp
private bool ShouldSkipTargetReplacement(
    PositionInfo pos,
    int targetNumber,
    int qty,
    bool isFilled,
    bool isRunner)
{
    if (pos.ExecutingAccount == null) return true;
    if (isFilled || isRunner || qty <= 0) return true;
    return false;
}
```
**CYC**: 3 (2 if statements)

#### 2. `IsOrderCancellable` (CYC ≤2)
**Purpose**: Extract order state validation logic
**Lines**: 38-42, 62-66
**Logic**:
```csharp
private bool IsOrderCancellable(Order order)
{
    if (order == null) return false;
    
    return order.OrderState == OrderState.Working
        || order.OrderState == OrderState.Accepted
        || order.OrderState == OrderState.Submitted
        || order.OrderState == OrderState.ChangePending;
}
```
**CYC**: 2 (1 if + 1 compound OR)

#### 3. `CleanupStaleFollowerTarget` (CYC ≤3)
**Purpose**: Handle stale target cleanup path
**Lines**: 34-47
**Logic**:
```csharp
private void CleanupStaleFollowerTarget(
    string fleetEntryName,
    PositionInfo pos,
    ConcurrentDictionary<string, Order> dict)
{
    if (!dict.TryGetValue(fleetEntryName, out var staleTarget) || staleTarget == null)
        return;
    
    if (IsOrderCancellable(staleTarget))
    {
        pos.ExecutingAccount.Cancel(new[] { staleTarget });
    }
    dict.TryRemove(fleetEntryName, out _);
}
```
**CYC**: 3 (2 if statements + helper call)

#### 4. `CreateFollowerTargetReplaceSpec` (CYC ≤4)
**Purpose**: Build FSM replacement spec
**Lines**: 68-82
**Logic**:
```csharp
private FollowerTargetReplaceSpec CreateFollowerTargetReplaceSpec(
    string fleetEntryName,
    PositionInfo pos,
    int targetNumber,
    Order oldTarget,
    int qty)
{
    double newPrice = GetTargetPrice(pos, targetNumber);
    if (newPrice <= 0) return null;
    
    string targetTag = "T" + targetNumber;
    OrderAction exitAction = pos.Direction == MarketPosition.Long 
        ? OrderAction.Sell 
        : OrderAction.BuyToCover;
    string signalName = SymmetryTrim(targetTag + "_" + fleetEntryName, 40);
    
    return new FollowerTargetReplaceSpec
    {
        EntryName = fleetEntryName,
        TargetNum = targetNumber,
        NewTargetPrice = Instrument.MasterInstrument.RoundToTickSize(newPrice),
        Quantity = qty,
        ExitAction = exitAction,
        TargetAccount = pos.ExecutingAccount,
        CancellingOrderId = oldTarget.OrderId,
    };
}
```
**CYC**: 4 (1 if + 1 ternary)

### Refactored Main Method (CYC ≤5)
```csharp
private void SymmetryGuardReplaceExistingFollowerTarget(
    string fleetEntryName,
    PositionInfo pos,
    int targetNumber,
    ConcurrentDictionary<string, Order> dict)
{
    string targetTag = "T" + targetNumber;
    bool isRunner = IsRunnerTarget(targetNumber);
    bool isFilled = IsTargetFilled(pos, targetNumber);
    int qty = GetTargetContracts(pos, targetNumber);
    
    // Early exit: skip if filled, runner, or invalid
    if (ShouldSkipTargetReplacement(pos, targetNumber, qty, isFilled, isRunner))
    {
        CleanupStaleFollowerTarget(fleetEntryName, pos, dict);
        return;
    }
    
    // Active replacement: two-phase FSM
    if (!dict.TryGetValue(fleetEntryName, out var oldTarget) || oldTarget == null)
        return;
    
    if (!IsOrderCancellable(oldTarget))
        return;
    
    var tSpec = CreateFollowerTargetReplaceSpec(fleetEntryName, pos, targetNumber, oldTarget, qty);
    if (tSpec == null)
        return;
    
    string signalName = SymmetryTrim(targetTag + "_" + fleetEntryName, 40);
    _followerTargetReplaceSpecs[signalName] = tSpec;
    StampReaperMoveGrace();
    pos.ExecutingAccount.Cancel(new[] { oldTarget });
}
```
**CYC**: 5 (4 if statements + 1 helper call with internal if)

## Complexity Reduction Summary

| Component | CYC |
|-----------|-----|
| **Original Method** | **18** |
| Refactored Main | 5 |
| ShouldSkipTargetReplacement | 3 |
| IsOrderCancellable | 2 |
| CleanupStaleFollowerTarget | 3 |
| CreateFollowerTargetReplaceSpec | 4 |
| **Total (Max Single Method)** | **5** |

**Reduction**: 18 → 5 = 72% reduction (exceeds 56% target)

## Blast Radius Analysis

### Direct Callers
1. `SymmetryGuardRetargetExistingFollowerBracket` (line 15)
   - Calls this method 5 times (once per target)
   - No changes required (signature unchanged)

### Dependencies
- `IsRunnerTarget(int)` - existing helper
- `IsTargetFilled(PositionInfo, int)` - existing helper
- `GetTargetContracts(PositionInfo, int)` - existing helper
- `GetTargetPrice(PositionInfo, int)` - existing helper
- `SymmetryTrim(string, int)` - existing helper
- `StampReaperMoveGrace()` - existing helper

### State Mutations
- `_followerTargetReplaceSpecs` dictionary (write)
- `dict` parameter (ConcurrentDictionary) - remove operations
- `pos.ExecutingAccount.Cancel()` - external side effect

### Risk Assessment
- **Low Risk**: Pure extraction, no logic changes
- **No Breaking Changes**: Signature unchanged, callers unaffected
- **Thread Safety**: Preserved (ConcurrentDictionary, no locks)
- **FSM Integrity**: Two-phase pattern maintained

## V12 DNA Compliance

### ✅ Lock-Free Actor Pattern
- No `lock()` statements in original or extracted methods
- Uses ConcurrentDictionary for thread-safe operations
- FSM two-phase pattern preserved

### ✅ ASCII-Only Compliance
- No Unicode characters detected
- All string literals are ASCII

### ✅ Correctness by Construction
- Early returns prevent invalid states
- Null checks before operations
- Order state validation before cancel

### ✅ Jane Street Alignment
- CYC ≤8 per method (target: ≤8, achieved: ≤5)
- Single responsibility per extracted method
- Cognitive simplicity maintained

## Scope Boundary

### In Scope
1. Extract `ShouldSkipTargetReplacement` helper
2. Extract `IsOrderCancellable` helper
3. Extract `CleanupStaleFollowerTarget` helper
4. Extract `CreateFollowerTargetReplaceSpec` helper
5. Refactor main method to use helpers
6. Verify CYC reduction (18 → 5)

### Out of Scope
- Modifying caller `SymmetryGuardRetargetExistingFollowerBracket`
- Changing method signature
- Altering FSM two-phase logic
- Modifying existing helper methods
- Adding new features

### Success Criteria
1. ✅ Main method CYC ≤8 (target: 5)
2. ✅ All extracted methods CYC ≤8
3. ✅ Zero logic drift (pure structural movement)
4. ✅ Build passes (`dotnet build`)
5. ✅ `deploy-sync.ps1` succeeds
6. ✅ F5 in NinjaTrader successful
7. ✅ No new compilation errors
8. ✅ ASCII-only compliance maintained

## Next Steps (Phase 1.5)
1. Validate scope boundary (no scope creep)
2. Confirm extraction targets are single-method only
3. Verify no cross-file dependencies
4. Proceed to Phase 2 (Architecture Planning)

## Status
✅ **Phase 1 Complete** - Ready for Phase 1.5 (Scope Boundary Validation)