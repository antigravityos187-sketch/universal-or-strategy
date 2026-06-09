# EPIC-CCN-16 Ticket 05: Extract RegisterFSM

## Ticket Metadata
- **Epic**: EPIC-CCN-16
- **Ticket**: 05 of 7
- **Priority**: P2 (Medium-Risk, Independent)
- **Estimated Duration**: 45 minutes
- **Complexity**: CYC ~2
- **Risk Level**: MEDIUM

## Objective
Extract FSM registration logic into a focused method that centralizes dictionary updates and counter management.

## Current Code Location
**File**: [`src/V12_002.SIMA.Lifecycle.cs`](src/V12_002.SIMA.Lifecycle.cs:590)
**Lines**: 590-598

## Method Signature
```csharp
/// <summary>
/// Registers FSM in tracking dictionaries and updates counters.
/// Centralizes dictionary update logic for easier auditing.
/// </summary>
/// <param name="entryKey">FSM key</param>
/// <param name="fsm">FSM to register</param>
/// <param name="entryOrder">Entry order (may be null for position pass)</param>
/// <param name="ordersIndexed">Counter (incremented if entry order linked)</param>
/// <param name="fsmCreated">Counter (always incremented)</param>
private void RegisterFSM(
    string entryKey,
    FollowerBracketFSM fsm,
    Order entryOrder,
    ref int ordersIndexed,
    ref int fsmCreated
)
```

## Implementation Steps

### 1. Write Tests FIRST (TDD)
**File**: `tests/V12_Performance.Tests/Core/FSMHydrationTests.cs`

```csharp
public class RegisterFSMTests
{
    [Fact]
    public void RegisterFSM_WithEntryOrder_RegistersAndIndexes()
    {
        // Arrange
        var sut = new V12_002();
        var entryKey = "TEST_KEY";
        var fsm = new FollowerBracketFSM();
        var entryOrder = new Order { OrderId = "ORDER123" };
        int ordersIndexed = 0;
        int fsmCreated = 0;

        // Act
        sut.RegisterFSM(entryKey, fsm, entryOrder, ref ordersIndexed, ref fsmCreated);

        // Assert
        Assert.True(sut._followerBrackets.ContainsKey(entryKey));
        Assert.Equal(1, ordersIndexed);
        Assert.Equal(1, fsmCreated);
    }

    [Fact]
    public void RegisterFSM_WithoutEntryOrder_RegistersButDoesNotIndex()
    {
        // Arrange
        var sut = new V12_002();
        var entryKey = "TEST_KEY";
        var fsm = new FollowerBracketFSM();
        int ordersIndexed = 0;
        int fsmCreated = 0;

        // Act
        sut.RegisterFSM(entryKey, fsm, null, ref ordersIndexed, ref fsmCreated);

        // Assert
        Assert.True(sut._followerBrackets.ContainsKey(entryKey));
        Assert.Equal(0, ordersIndexed);
        Assert.Equal(1, fsmCreated);
    }

    [Fact]
    public void RegisterFSM_DuplicateKey_IsIdempotent()
    {
        // Arrange
        var sut = new V12_002();
        var entryKey = "TEST_KEY";
        var fsm1 = new FollowerBracketFSM();
        var fsm2 = new FollowerBracketFSM();
        int ordersIndexed = 0;
        int fsmCreated = 0;

        // Act
        sut.RegisterFSM(entryKey, fsm1, null, ref ordersIndexed, ref fsmCreated);
        sut.RegisterFSM(entryKey, fsm2, null, ref ordersIndexed, ref fsmCreated);

        // Assert
        Assert.Equal(fsm1, sut._followerBrackets[entryKey]); // First FSM preserved
        Assert.Equal(1, fsmCreated); // Only first registration counted
    }
}
```

### 2. Extract Method
**Location**: Add after `LinkTargetOrder`

```csharp
/// <summary>
/// Registers FSM in tracking dictionaries and updates counters.
/// Centralizes dictionary update logic for easier auditing.
/// </summary>
/// <param name="entryKey">FSM key</param>
/// <param name="fsm">FSM to register</param>
/// <param name="entryOrder">Entry order (may be null for position pass)</param>
/// <param name="ordersIndexed">Counter (incremented if entry order linked)</param>
/// <param name="fsmCreated">Counter (always incremented)</param>
private void RegisterFSM(
    string entryKey,
    FollowerBracketFSM fsm,
    Order entryOrder,
    ref int ordersIndexed,
    ref int fsmCreated
)
{
    _followerBrackets.TryAdd(entryKey, fsm);

    if (entryOrder != null && !string.IsNullOrEmpty(entryOrder.OrderId))
    {
        _orderIdToFsmKey[entryOrder.OrderId] = entryKey;
        ordersIndexed++;
    }

    fsmCreated++;
}
```

### 3. Update Callers
**Location 1**: Lines 590-598 in `HydrateFSMsFromWorkingOrders` (Entry Order Pass)
**Location 2**: Lines 731-734 in `HydrateFSMsFromWorkingOrders` (Position Pass)

**Before** (Entry Order Pass):
```csharp
_followerBrackets.TryAdd(entryKey, fsm);

if (!string.IsNullOrEmpty(entryOrder.OrderId))
{
    _orderIdToFsmKey[entryOrder.OrderId] = entryKey;
    ordersIndexed++;
}

fsmCreated++;
```

**After**:
```csharp
RegisterFSM(entryKey, fsm, entryOrder, ref ordersIndexed, ref fsmCreated);
```

**Before** (Position Pass):
```csharp
_followerBrackets.TryAdd(recoveredKey, fsm);

positionFsmCreated++;
fsmCreated++;
```

**After**:
```csharp
RegisterFSM(recoveredKey, fsm, null, ref ordersIndexed, ref fsmCreated);
positionFsmCreated++;
```

### 4. Verification Gates
```powershell
powershell -File .\scripts\build_readiness.ps1
dotnet test tests/V12_Performance.Tests/ --filter "FullyQualifiedName~RegisterFSM"
python scripts/complexity_audit.py
powershell -File .\scripts\lint.ps1
```

## Acceptance Criteria
- ✅ 3 test cases pass
- ✅ Build passes
- ✅ Method complexity: CYC ≤2
- ✅ Both callers updated
- ✅ Idempotency preserved (TryAdd semantics)
- ✅ Thread safety preserved (ConcurrentDictionary)
- ✅ Git commit created

## Dependencies
- **Upstream**: None
- **Downstream**: Ticket 06 (HydrateFromOpenPositions) will use this

## Thread Safety Notes
- Uses `TryAdd` (idempotent, thread-safe)
- No locks needed (actor-serialized model)
- ConcurrentDictionary handles concurrent reads during writes

## Notes
- Centralizes dictionary update logic
- Makes thread safety auditing easier
- Preserves exact behavior (TryAdd + counter updates)