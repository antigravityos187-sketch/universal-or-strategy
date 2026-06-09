# EPIC-CCN-16 Ticket 02: Extract BuildFSM

## Ticket Metadata
- **Epic**: EPIC-CCN-16
- **Ticket**: 02 of 7
- **Priority**: P1 (Low-Risk, No Dependencies)
- **Estimated Duration**: 30 minutes
- **Complexity**: CYC ~1
- **Risk Level**: LOW

## Objective
Extract FSM construction logic into a factory method that centralizes `FollowerBracketFSM` initialization.

## Current Code Location
**File**: [`src/V12_002.SIMA.Lifecycle.cs`](src/V12_002.SIMA.Lifecycle.cs:520)
**Lines**: 520-528

## Method Signature
```csharp
/// <summary>
/// Factory method to construct FollowerBracketFSM instance.
/// Centralizes FSM initialization logic.
/// </summary>
/// <param name="entryKey">FSM key (entry order name)</param>
/// <param name="pi">Position info (contains account name)</param>
/// <param name="entryOrder">Entry order (may be null for position pass)</param>
/// <param name="state">Initial FSM state</param>
/// <param name="remainingContracts">Remaining contracts</param>
/// <returns>Initialized FSM instance</returns>
private FollowerBracketFSM BuildFSM(
    string entryKey,
    PositionInfo pi,
    Order entryOrder,
    FollowerBracketState state,
    int remainingContracts
)
```

## Implementation Steps

### 1. Write Tests FIRST (TDD)
**File**: `tests/V12_Performance.Tests/Core/FSMHydrationTests.cs`

```csharp
public class BuildFSMTests
{
    [Fact]
    public void BuildFSM_WithEntryOrder_InitializesCorrectly()
    {
        // Arrange
        var sut = new V12_002();
        var entryKey = "TEST_ENTRY";
        var pi = new PositionInfo { ExecutingAccount = new Account { Name = "TestAccount" } };
        var entryOrder = new Order { /* mock order */ };
        var state = FollowerBracketState.Active;
        var remainingContracts = 10;

        // Act
        var fsm = sut.BuildFSM(entryKey, pi, entryOrder, state, remainingContracts);

        // Assert
        Assert.Equal("TestAccount", fsm.AccountName);
        Assert.Equal(entryKey, fsm.EntryName);
        Assert.Equal(state, fsm.State);
        Assert.Equal(remainingContracts, fsm.RemainingContracts);
        Assert.Equal(entryOrder, fsm.EntryOrder);
        Assert.NotNull(fsm.LastUpdateUtc);
    }

    [Fact]
    public void BuildFSM_WithoutEntryOrder_InitializesCorrectly()
    {
        // Arrange
        var sut = new V12_002();
        var entryKey = "TEST_ENTRY";
        var pi = new PositionInfo { ExecutingAccount = new Account { Name = "TestAccount" } };
        var state = FollowerBracketState.Active;
        var remainingContracts = 10;

        // Act
        var fsm = sut.BuildFSM(entryKey, pi, null, state, remainingContracts);

        // Assert
        Assert.Equal("TestAccount", fsm.AccountName);
        Assert.Null(fsm.EntryOrder);
    }
}
```

### 2. Extract Method
**Location**: Add after `MapOrderStateToFSMState`

```csharp
/// <summary>
/// Factory method to construct FollowerBracketFSM instance.
/// Centralizes FSM initialization logic.
/// </summary>
/// <param name="entryKey">FSM key (entry order name)</param>
/// <param name="pi">Position info (contains account name)</param>
/// <param name="entryOrder">Entry order (may be null for position pass)</param>
/// <param name="state">Initial FSM state</param>
/// <param name="remainingContracts">Remaining contracts</param>
/// <returns>Initialized FSM instance</returns>
private FollowerBracketFSM BuildFSM(
    string entryKey,
    PositionInfo pi,
    Order entryOrder,
    FollowerBracketState state,
    int remainingContracts
)
{
    return new FollowerBracketFSM
    {
        AccountName = pi.ExecutingAccount.Name,
        EntryName = entryKey,
        State = state,
        RemainingContracts = remainingContracts,
        LastUpdateUtc = DateTime.UtcNow,
        EntryOrder = entryOrder,
    };
}
```

### 3. Update Callers
**Location 1**: Lines 520-528 in `HydrateFSMsFromWorkingOrders` (Entry Order Pass)
**Location 2**: Lines 662-670 in `HydrateFSMsFromWorkingOrders` (Position Pass)

**Before** (Entry Order Pass):
```csharp
var fsm = new FollowerBracketFSM
{
    AccountName = pi.ExecutingAccount.Name,
    EntryName = entryKey,
    State = hydrationState,
    RemainingContracts = hydratedRemainingContracts,
    LastUpdateUtc = DateTime.UtcNow,
    EntryOrder = entryOrder,
};
```

**After**:
```csharp
var fsm = BuildFSM(entryKey, pi, entryOrder, hydrationState.Value, hydratedRemainingContracts);
```

**Before** (Position Pass):
```csharp
var fsm = new FollowerBracketFSM
{
    AccountName = acct.Name,
    EntryName = recoveredKey,
    State = FollowerBracketState.Active,
    RemainingContracts = Math.Abs(acctPos.Quantity),
    LastUpdateUtc = DateTime.UtcNow,
    EntryOrder = null, // Terminal entry order
};
```

**After**:
```csharp
// Create temporary PositionInfo for BuildFSM signature compatibility
var tempPi = new PositionInfo { ExecutingAccount = acct };
var fsm = BuildFSM(
    recoveredKey,
    tempPi,
    null, // Terminal entry order
    FollowerBracketState.Active,
    Math.Abs(acctPos.Quantity)
);
```

### 4. Verification Gates
```powershell
powershell -File .\scripts\build_readiness.ps1
dotnet test tests/V12_Performance.Tests/ --filter "FullyQualifiedName~BuildFSM"
python scripts/complexity_audit.py
powershell -File .\scripts\lint.ps1
```

## Acceptance Criteria
- ✅ 2 test cases pass
- ✅ Build passes
- ✅ Method complexity: CYC ≤1
- ✅ Both callers updated
- ✅ Behavior preserved
- ✅ Git commit created

## Dependencies
- **Upstream**: None
- **Downstream**: Ticket 06 (HydrateFromOpenPositions) will use this

## Notes
- Simple factory method
- Centralizes FSM initialization
- Makes future FSM structure changes easier