# EPIC-CCN-16 Ticket 04: Extract ResolveRemainingContracts

## Ticket Metadata
- **Epic**: EPIC-CCN-16
- **Ticket**: 04 of 7
- **Priority**: P2 (Medium-Risk, Depends on Ticket 01)
- **Estimated Duration**: 45 minutes
- **Complexity**: CYC ~3
- **Risk Level**: MEDIUM

## Objective
Extract position quantity resolution logic into a focused method that determines remaining contracts based on FSM state and live position.

## Current Code Location
**File**: [`src/V12_002.SIMA.Lifecycle.cs`](src/V12_002.SIMA.Lifecycle.cs:505)
**Lines**: 505-518

## Method Signature
```csharp
/// <summary>
/// Resolves remaining contracts for FSM based on order state and live position.
/// For Active state, queries live position. Otherwise uses order quantity.
/// </summary>
/// <param name="entryOrder">Entry order</param>
/// <param name="pi">Position info (contains executing account)</param>
/// <param name="state">FSM state (determines resolution strategy)</param>
/// <returns>Remaining contracts (always >= 0)</returns>
private int ResolveRemainingContracts(
    Order entryOrder,
    PositionInfo pi,
    FollowerBracketState state
)
```

## Implementation Steps

### 1. Write Tests FIRST (TDD)
**File**: `tests/V12_Performance.Tests/Core/FSMHydrationTests.cs`

```csharp
public class ResolveRemainingContractsTests
{
    [Fact]
    public void ResolveRemainingContracts_ActiveStateWithPosition_ReturnsPositionQuantity()
    {
        // Arrange
        var sut = new V12_002();
        var entryOrder = new Order { Quantity = 5 };
        var position = new Position { Quantity = 10, MarketPosition = MarketPosition.Long };
        var account = new Account();
        account.Positions.Add(position);
        var pi = new PositionInfo { ExecutingAccount = account };
        
        // Act
        var result = sut.ResolveRemainingContracts(entryOrder, pi, FollowerBracketState.Active);
        
        // Assert
        Assert.Equal(10, result);
    }

    [Fact]
    public void ResolveRemainingContracts_ActiveStateWithoutPosition_ReturnsOrderQuantity()
    {
        // Arrange
        var sut = new V12_002();
        var entryOrder = new Order { Quantity = 5 };
        var account = new Account(); // No positions
        var pi = new PositionInfo { ExecutingAccount = account };
        
        // Act
        var result = sut.ResolveRemainingContracts(entryOrder, pi, FollowerBracketState.Active);
        
        // Assert
        Assert.Equal(5, result);
    }

    [Fact]
    public void ResolveRemainingContracts_NonActiveState_ReturnsOrderQuantity()
    {
        // Arrange
        var sut = new V12_002();
        var entryOrder = new Order { Quantity = 5 };
        var pi = new PositionInfo { ExecutingAccount = new Account() };
        
        // Act
        var result = sut.ResolveRemainingContracts(entryOrder, pi, FollowerBracketState.Submitted);
        
        // Assert
        Assert.Equal(5, result);
    }
}
```

### 2. Extract Method
**Location**: Add after `MapOrderStateToFSMState`

```csharp
/// <summary>
/// Resolves remaining contracts for FSM based on order state and live position.
/// For Active state, queries live position. Otherwise uses order quantity.
/// </summary>
/// <param name="entryOrder">Entry order</param>
/// <param name="pi">Position info (contains executing account)</param>
/// <param name="state">FSM state (determines resolution strategy)</param>
/// <returns>Remaining contracts (always >= 0)</returns>
private int ResolveRemainingContracts(
    Order entryOrder,
    PositionInfo pi,
    FollowerBracketState state
)
{
    int hydratedRemainingContracts = Math.Max(0, entryOrder.Quantity);
    
    if (state == FollowerBracketState.Active)
    {
        Position livePosition = pi
            .ExecutingAccount.Positions.ToArray()
            .FirstOrDefault(p =>
                p != null
                && p.Instrument != null
                && p.Instrument.FullName == Instrument.FullName
                && p.MarketPosition != MarketPosition.Flat
            );
        
        if (livePosition != null)
            hydratedRemainingContracts = Math.Abs(livePosition.Quantity);
    }
    
    return hydratedRemainingContracts;
}
```

### 3. Update Caller
**Location**: Lines 505-518 in `HydrateFSMsFromWorkingOrders`

**Before**:
```csharp
int hydratedRemainingContracts = Math.Max(0, entryOrder.Quantity);
if (hydrationState == FollowerBracketState.Active)
{
    Position livePosition = pi
        .ExecutingAccount.Positions.ToArray()
        .FirstOrDefault(p =>
            p != null
            && p.Instrument != null
            && p.Instrument.FullName == Instrument.FullName
            && p.MarketPosition != MarketPosition.Flat
        );
    if (livePosition != null)
        hydratedRemainingContracts = Math.Abs(livePosition.Quantity);
}
```

**After**:
```csharp
int hydratedRemainingContracts = ResolveRemainingContracts(entryOrder, pi, hydrationState.Value);
```

### 4. Verification Gates
```powershell
powershell -File .\scripts\build_readiness.ps1
dotnet test tests/V12_Performance.Tests/ --filter "FullyQualifiedName~ResolveRemainingContracts"
python scripts/complexity_audit.py
powershell -File .\scripts\lint.ps1
```

## Acceptance Criteria
- ✅ 3 test cases pass
- ✅ Build passes
- ✅ Method complexity: CYC ≤3
- ✅ Caller updated
- ✅ Behavior preserved (position lookup logic unchanged)
- ✅ Git commit created

## Dependencies
- **Upstream**: Ticket 01 (MapOrderStateToFSMState) - uses FollowerBracketState
- **Downstream**: Ticket 07 (Refactor Parent) will use this

## Notes
- Medium complexity due to LINQ and position lookup
- Preserves exact position matching logic
- Encapsulates "Active state = use position, else use order" rule