# EPIC-CCN-16 Ticket 03: Extract LinkTargetOrder

## Ticket Metadata
- **Epic**: EPIC-CCN-16
- **Ticket**: 03 of 7
- **Priority**: P1 (Low-Risk, Eliminates Repetition)
- **Estimated Duration**: 30 minutes
- **Complexity**: CYC ~2
- **Risk Level**: LOW

## Objective
Extract repetitive order linking logic into a single method, eliminating 59 lines of duplicated code.

## Current Code Location
**File**: [`src/V12_002.SIMA.Lifecycle.cs`](src/V12_002.SIMA.Lifecycle.cs:530)
**Lines**: 530-588 (repeated 6 times: stop + 5 targets)

## Method Signature
```csharp
/// <summary>
/// Links a single target order to FSM and updates tracking dictionaries.
/// Eliminates repetitive code (called 5 times for target1-target5).
/// </summary>
/// <param name="fsm">FSM to link order to</param>
/// <param name="targetOrders">Dictionary of target orders</param>
/// <param name="entryKey">FSM key (for dictionary lookup)</param>
/// <param name="targetIndex">Target slot index (0-4)</param>
/// <param name="ordersIndexed">Counter (incremented if order linked)</param>
private void LinkTargetOrder(
    FollowerBracketFSM fsm,
    Dictionary<string, Order> targetOrders,
    string entryKey,
    int targetIndex,
    ref int ordersIndexed
)
```

## Implementation Steps

### 1. Write Tests FIRST (TDD)
**File**: `tests/V12_Performance.Tests/Core/FSMHydrationTests.cs`

```csharp
public class LinkTargetOrderTests
{
    [Fact]
    public void LinkTargetOrder_WithValidOrder_LinksSuccessfully()
    {
        // Arrange
        var sut = new V12_002();
        var fsm = new FollowerBracketFSM { Targets = new Order[5] };
        var targetOrders = new Dictionary<string, Order>
        {
            ["TEST_KEY"] = new Order { OrderId = "ORDER123" }
        };
        int ordersIndexed = 0;

        // Act
        sut.LinkTargetOrder(fsm, targetOrders, "TEST_KEY", 0, ref ordersIndexed);

        // Assert
        Assert.NotNull(fsm.Targets[0]);
        Assert.Equal("ORDER123", fsm.Targets[0].OrderId);
        Assert.Equal(1, ordersIndexed);
    }

    [Fact]
    public void LinkTargetOrder_WithNullOrder_DoesNotLink()
    {
        // Arrange
        var sut = new V12_002();
        var fsm = new FollowerBracketFSM { Targets = new Order[5] };
        var targetOrders = new Dictionary<string, Order> { ["TEST_KEY"] = null };
        int ordersIndexed = 0;

        // Act
        sut.LinkTargetOrder(fsm, targetOrders, "TEST_KEY", 0, ref ordersIndexed);

        // Assert
        Assert.Null(fsm.Targets[0]);
        Assert.Equal(0, ordersIndexed);
    }

    [Fact]
    public void LinkTargetOrder_WithEmptyOrderId_LinksButDoesNotIndex()
    {
        // Arrange
        var sut = new V12_002();
        var fsm = new FollowerBracketFSM { Targets = new Order[5] };
        var targetOrders = new Dictionary<string, Order>
        {
            ["TEST_KEY"] = new Order { OrderId = "" }
        };
        int ordersIndexed = 0;

        // Act
        sut.LinkTargetOrder(fsm, targetOrders, "TEST_KEY", 0, ref ordersIndexed);

        // Assert
        Assert.NotNull(fsm.Targets[0]);
        Assert.Equal(0, ordersIndexed); // Not indexed due to empty OrderId
    }
}
```

### 2. Extract Method
**Location**: Add after `BuildFSM`

```csharp
/// <summary>
/// Links a single target order to FSM and updates tracking dictionaries.
/// Eliminates repetitive code (called 5 times for target1-target5).
/// </summary>
/// <param name="fsm">FSM to link order to</param>
/// <param name="targetOrders">Dictionary of target orders</param>
/// <param name="entryKey">FSM key (for dictionary lookup)</param>
/// <param name="targetIndex">Target slot index (0-4)</param>
/// <param name="ordersIndexed">Counter (incremented if order linked)</param>
private void LinkTargetOrder(
    FollowerBracketFSM fsm,
    Dictionary<string, Order> targetOrders,
    string entryKey,
    int targetIndex,
    ref int ordersIndexed
)
{
    Order targetOrd;
    if (targetOrders.TryGetValue(entryKey, out targetOrd) && targetOrd != null)
    {
        fsm.Targets[targetIndex] = targetOrd;
        if (!string.IsNullOrEmpty(targetOrd.OrderId))
        {
            _orderIdToFsmKey[targetOrd.OrderId] = entryKey;
            ordersIndexed++;
        }
    }
}
```

### 3. Update Callers
**Location 1**: Lines 542-588 in `HydrateFSMsFromWorkingOrders` (Entry Order Pass)
**Location 2**: Lines 684-729 in `HydrateFSMsFromWorkingOrders` (Position Pass)

**Before** (59 lines of repetition):
```csharp
Order targetOrd;
if (target1Orders.TryGetValue(entryKey, out targetOrd) && targetOrd != null)
{
    fsm.Targets[0] = targetOrd;
    if (!string.IsNullOrEmpty(targetOrd.OrderId))
    {
        _orderIdToFsmKey[targetOrd.OrderId] = entryKey;
        ordersIndexed++;
    }
}
// ... repeated 4 more times for target2-target5
```

**After** (5 lines):
```csharp
LinkTargetOrder(fsm, target1Orders, entryKey, 0, ref ordersIndexed);
LinkTargetOrder(fsm, target2Orders, entryKey, 1, ref ordersIndexed);
LinkTargetOrder(fsm, target3Orders, entryKey, 2, ref ordersIndexed);
LinkTargetOrder(fsm, target4Orders, entryKey, 3, ref ordersIndexed);
LinkTargetOrder(fsm, target5Orders, entryKey, 4, ref ordersIndexed);
```

**Note**: Stop order linking (lines 530-540) remains inline (different pattern - sets `fsm.StopOrder` not `fsm.Targets[]`)

### 4. Verification Gates
```powershell
powershell -File .\scripts\build_readiness.ps1
dotnet test tests/V12_Performance.Tests/ --filter "FullyQualifiedName~LinkTargetOrder"
python scripts/complexity_audit.py
powershell -File .\scripts\lint.ps1
```

## Acceptance Criteria
- ✅ 3 test cases pass
- ✅ Build passes
- ✅ Method complexity: CYC ≤2
- ✅ 59 lines of repetition eliminated
- ✅ Both callers updated (Entry Order Pass + Position Pass)
- ✅ Behavior preserved
- ✅ Git commit created

## Dependencies
- **Upstream**: None
- **Downstream**: Ticket 06 (HydrateFromOpenPositions) will use this

## Complexity Reduction
- **Before**: 59 lines, CYC ~10 (5 identical branches)
- **After**: 12 lines method + 5 calls = 17 lines, CYC ~2

## Notes
- Biggest line count reduction (59 → 17 lines)
- Eliminates maintenance burden (change once, not 5 times)
- Preserves exact behavior (same logic, just parameterized)