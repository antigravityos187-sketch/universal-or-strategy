# EPIC-CCN-16 Ticket 01: Extract MapOrderStateToFSMState

## Ticket Metadata
- **Epic**: EPIC-CCN-16
- **Ticket**: 01 of 7
- **Priority**: P1 (Low-Risk, No Dependencies)
- **Estimated Duration**: 30 minutes
- **Complexity**: CYC ~8
- **Risk Level**: LOW

## Objective
Extract state mapping logic into a pure function that maps NinjaTrader `OrderState` to V12 `FollowerBracketState`.

## Current Code Location
**File**: [`src/V12_002.SIMA.Lifecycle.cs`](src/V12_002.SIMA.Lifecycle.cs:488)
**Lines**: 488-503

## Method Signature
```csharp
/// <summary>
/// Maps NinjaTrader OrderState to V12 FollowerBracketState.
/// Pure function - no side effects, deterministic mapping.
/// </summary>
/// <param name="entryState">NinjaTrader order state</param>
/// <returns>Corresponding FSM state, or null if terminal state (skip FSM creation)</returns>
/// <remarks>
/// Terminal states (Cancelled, Rejected, etc.) return null to signal caller to skip FSM creation.
/// This preserves the original behavior where terminal orders are ignored.
/// </remarks>
private FollowerBracketState? MapOrderStateToFSMState(OrderState entryState)
```

## Implementation Steps

### 1. Write Tests FIRST (TDD)
**File**: `tests/V12_Performance.Tests/Core/FSMHydrationTests.cs` (create if not exists)

```csharp
public class MapOrderStateToFSMStateTests
{
    [Theory]
    [InlineData(OrderState.Filled, FollowerBracketState.Active)]
    [InlineData(OrderState.PartFilled, FollowerBracketState.Active)]
    [InlineData(OrderState.Accepted, FollowerBracketState.Accepted)]
    [InlineData(OrderState.Working, FollowerBracketState.Submitted)]
    [InlineData(OrderState.Submitted, FollowerBracketState.Submitted)]
    [InlineData(OrderState.Initialized, FollowerBracketState.Submitted)]
    [InlineData(OrderState.ChangePending, FollowerBracketState.Submitted)]
    [InlineData(OrderState.ChangeSubmitted, FollowerBracketState.Submitted)]
    [InlineData(OrderState.Cancelled, null)]
    [InlineData(OrderState.Rejected, null)]
    [InlineData(OrderState.Unknown, null)]
    public void MapOrderStateToFSMState_ReturnsCorrectMapping(
        OrderState input,
        FollowerBracketState? expected
    )
    {
        // Arrange
        var sut = new V12_002(); // Or use test fixture

        // Act
        var result = sut.MapOrderStateToFSMState(input);

        // Assert
        Assert.Equal(expected, result);
    }
}
```

### 2. Extract Method
**Location**: Add after line 463 (before `HydrateFSMsFromWorkingOrders`)

```csharp
/// <summary>
/// Maps NinjaTrader OrderState to V12 FollowerBracketState.
/// Pure function - no side effects, deterministic mapping.
/// </summary>
/// <param name="entryState">NinjaTrader order state</param>
/// <returns>Corresponding FSM state, or null if terminal state (skip FSM creation)</returns>
/// <remarks>
/// Terminal states (Cancelled, Rejected, etc.) return null to signal caller to skip FSM creation.
/// This preserves the original behavior where terminal orders are ignored.
/// </remarks>
private FollowerBracketState? MapOrderStateToFSMState(OrderState entryState)
{
    if (entryState == OrderState.Filled || entryState == OrderState.PartFilled)
        return FollowerBracketState.Active;
    
    if (entryState == OrderState.Accepted)
        return FollowerBracketState.Accepted;
    
    if (
        entryState == OrderState.Working
        || entryState == OrderState.Submitted
        || entryState == OrderState.Initialized
        || entryState == OrderState.ChangePending
        || entryState == OrderState.ChangeSubmitted
    )
        return FollowerBracketState.Submitted;
    
    // Terminal states (Cancelled, Rejected, etc.) - skip FSM creation
    return null;
}
```

### 3. Update Caller
**Location**: Lines 488-503 in `HydrateFSMsFromWorkingOrders`

**Before**:
```csharp
FollowerBracketState hydrationState;
OrderState entryState = entryOrder.OrderState;
if (entryState == OrderState.Filled || entryState == OrderState.PartFilled)
    hydrationState = FollowerBracketState.Active;
else if (entryState == OrderState.Accepted)
    hydrationState = FollowerBracketState.Accepted;
else if (
    entryState == OrderState.Working
    || entryState == OrderState.Submitted
    || entryState == OrderState.Initialized
    || entryState == OrderState.ChangePending
    || entryState == OrderState.ChangeSubmitted
)
    hydrationState = FollowerBracketState.Submitted;
else
    continue; // Terminal state -- FSM not needed
```

**After**:
```csharp
FollowerBracketState? hydrationState = MapOrderStateToFSMState(entryOrder.OrderState);
if (hydrationState == null)
    continue; // Terminal state -- FSM not needed
```

### 4. Verification Gates
Run after implementation:
```powershell
# Build
powershell -File .\scripts\build_readiness.ps1

# Tests
dotnet test tests/V12_Performance.Tests/ --filter "FullyQualifiedName~MapOrderStateToFSMState"

# Complexity
python scripts/complexity_audit.py

# Lint
powershell -File .\scripts\lint.ps1
```

## Acceptance Criteria
- ✅ All 11 test cases pass
- ✅ Build passes (zero errors, zero warnings)
- ✅ Method complexity: CYC ≤8
- ✅ Caller updated to use new method
- ✅ Behavior preserved (no functional changes)
- ✅ Git commit created

## Dependencies
- **Upstream**: None (pure function)
- **Downstream**: Ticket 02 (ResolveRemainingContracts) will use this

## Rollback Plan
```powershell
git reset --hard HEAD~1
```

## Notes
- Pure function - easiest extraction
- No side effects, no dependencies
- Good starting point for TDD workflow