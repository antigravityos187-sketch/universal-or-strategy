# EPIC-CCN-16 Ticket 06: Extract HydrateFromOpenPositions

## Ticket Metadata
- **Epic**: EPIC-CCN-16
- **Ticket**: 06 of 7
- **Priority**: P3 (High-Risk, Depends on Tickets 02, 03, 05)
- **Estimated Duration**: 90 minutes
- **Complexity**: CYC ~8
- **Risk Level**: HIGH

## Objective
Extract Position Pass logic into a separate method that handles accounts with open positions but terminal entry orders.

## Current Code Location
**File**: [`src/V12_002.SIMA.Lifecycle.cs`](src/V12_002.SIMA.Lifecycle.cs:602)
**Lines**: 602-743 (141 lines)

## Method Signature
```csharp
/// <summary>
/// Position Pass: Creates FSMs for accounts with open positions but terminal entry orders.
/// Handles edge case where entry order is cancelled/rejected but position still open.
/// </summary>
/// <param name="stopOrders">Dictionary of stop orders (for key recovery)</param>
/// <param name="target1Orders">Dictionary of target1 orders</param>
/// <param name="target2Orders">Dictionary of target2 orders</param>
/// <param name="target3Orders">Dictionary of target3 orders</param>
/// <param name="target4Orders">Dictionary of target4 orders</param>
/// <param name="target5Orders">Dictionary of target5 orders</param>
/// <param name="ordersIndexed">Counter (incremented for each order linked)</param>
/// <param name="fsmCreated">Counter (incremented for each FSM created)</param>
/// <returns>Number of FSMs created in position pass</returns>
private int HydrateFromOpenPositions(
    Dictionary<string, Order> stopOrders,
    Dictionary<string, Order> target1Orders,
    Dictionary<string, Order> target2Orders,
    Dictionary<string, Order> target3Orders,
    Dictionary<string, Order> target4Orders,
    Dictionary<string, Order> target5Orders,
    ref int ordersIndexed,
    ref int fsmCreated
)
```

## Implementation Steps

### 1. Write Tests FIRST (TDD)
**File**: `tests/V12_Performance.Tests/Core/FSMHydrationTests.cs`

```csharp
public class HydrateFromOpenPositionsTests
{
    [Fact]
    public void HydrateFromOpenPositions_AccountWithPosition_CreatesFSM()
    {
        // Arrange
        var sut = new V12_002();
        var account = new Account { Name = "TestAccount" };
        var position = new Position { Quantity = 10, MarketPosition = MarketPosition.Long };
        account.Positions.Add(position);
        var stopOrders = new Dictionary<string, Order>
        {
            ["TEST_KEY"] = new Order { Account = account }
        };
        int ordersIndexed = 0;
        int fsmCreated = 0;

        // Act
        var result = sut.HydrateFromOpenPositions(
            stopOrders, new(), new(), new(), new(), new(),
            ref ordersIndexed, ref fsmCreated
        );

        // Assert
        Assert.Equal(1, result);
        Assert.Equal(1, fsmCreated);
    }

    [Fact]
    public void HydrateFromOpenPositions_AccountWithoutPosition_DoesNotCreateFSM()
    {
        // Arrange
        var sut = new V12_002();
        var account = new Account { Name = "TestAccount" }; // No positions
        var stopOrders = new Dictionary<string, Order>
        {
            ["TEST_KEY"] = new Order { Account = account }
        };
        int ordersIndexed = 0;
        int fsmCreated = 0;

        // Act
        var result = sut.HydrateFromOpenPositions(
            stopOrders, new(), new(), new(), new(), new(),
            ref ordersIndexed, ref fsmCreated
        );

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void HydrateFromOpenPositions_ExistingFSM_DoesNotCreateDuplicate()
    {
        // Arrange
        var sut = new V12_002();
        var account = new Account { Name = "TestAccount" };
        var position = new Position { Quantity = 10, MarketPosition = MarketPosition.Long };
        account.Positions.Add(position);
        
        // Pre-create FSM
        sut._followerBrackets.TryAdd("TEST_KEY", new FollowerBracketFSM { AccountName = "TestAccount" });
        
        var stopOrders = new Dictionary<string, Order>
        {
            ["TEST_KEY"] = new Order { Account = account }
        };
        int ordersIndexed = 0;
        int fsmCreated = 0;

        // Act
        var result = sut.HydrateFromOpenPositions(
            stopOrders, new(), new(), new(), new(), new(),
            ref ordersIndexed, ref fsmCreated
        );

        // Assert
        Assert.Equal(0, result); // No new FSM created
    }

    [Fact]
    public void HydrateFromOpenPositions_NoStopOrder_LogsWarningAndStartsGrace()
    {
        // Arrange
        var sut = new V12_002();
        var account = new Account { Name = "TestAccount" };
        var position = new Position { Quantity = 10, MarketPosition = MarketPosition.Long };
        account.Positions.Add(position);
        var stopOrders = new Dictionary<string, Order>(); // No stop orders
        int ordersIndexed = 0;
        int fsmCreated = 0;

        // Act
        var result = sut.HydrateFromOpenPositions(
            stopOrders, new(), new(), new(), new(), new(),
            ref ordersIndexed, ref fsmCreated
        );

        // Assert
        Assert.Equal(0, result);
        Assert.True(sut._positionPassFailedFirstSeen.ContainsKey("TestAccount"));
    }

    [Fact]
    public void HydrateFromOpenPositions_LinksAllTargetOrders()
    {
        // Arrange
        var sut = new V12_002();
        var account = new Account { Name = "TestAccount" };
        var position = new Position { Quantity = 10, MarketPosition = MarketPosition.Long };
        account.Positions.Add(position);
        var stopOrders = new Dictionary<string, Order>
        {
            ["TEST_KEY"] = new Order { Account = account, OrderId = "STOP123" }
        };
        var target1Orders = new Dictionary<string, Order>
        {
            ["TEST_KEY"] = new Order { OrderId = "T1_123" }
        };
        int ordersIndexed = 0;
        int fsmCreated = 0;

        // Act
        var result = sut.HydrateFromOpenPositions(
            stopOrders, target1Orders, new(), new(), new(), new(),
            ref ordersIndexed, ref fsmCreated
        );

        // Assert
        Assert.Equal(1, result);
        Assert.Equal(2, ordersIndexed); // Stop + Target1
    }
}
```

### 2. Extract Method
**Location**: Add after `RegisterFSM`

```csharp
/// <summary>
/// Position Pass: Creates FSMs for accounts with open positions but terminal entry orders.
/// Handles edge case where entry order is cancelled/rejected but position still open.
/// </summary>
/// <param name="stopOrders">Dictionary of stop orders (for key recovery)</param>
/// <param name="target1Orders">Dictionary of target1 orders</param>
/// <param name="target2Orders">Dictionary of target2 orders</param>
/// <param name="target3Orders">Dictionary of target3 orders</param>
/// <param name="target4Orders">Dictionary of target4 orders</param>
/// <param name="target5Orders">Dictionary of target5 orders</param>
/// <param name="ordersIndexed">Counter (incremented for each order linked)</param>
/// <param name="fsmCreated">Counter (incremented for each FSM created)</param>
/// <returns>Number of FSMs created in position pass</returns>
private int HydrateFromOpenPositions(
    Dictionary<string, Order> stopOrders,
    Dictionary<string, Order> target1Orders,
    Dictionary<string, Order> target2Orders,
    Dictionary<string, Order> target3Orders,
    Dictionary<string, Order> target4Orders,
    Dictionary<string, Order> target5Orders,
    ref int ordersIndexed,
    ref int fsmCreated
)
{
    int positionFsmCreated = 0;
    
    foreach (Account acct in Account.All)
    {
        if (!IsFleetAccount(acct))
            continue;

        // Do we already have an FSM for this account?
        if (
            _followerBrackets.Values.Any(f =>
                string.Equals(f.AccountName, acct.Name, StringComparison.OrdinalIgnoreCase)
            )
        )
            continue;

        // Is there an open position for this instrument in this account?
        Position acctPos = acct.Positions.FirstOrDefault(p =>
            p.Instrument.FullName == Instrument.FullName && p.MarketPosition != MarketPosition.Flat
        );
        if (acctPos == null)
            continue;

        // Scan stopOrders for any entryKey belonging to this account
        string recoveredKey = null;
        Order recoveredStop = null;
        foreach (var stopKvp in stopOrders.ToArray())
        {
            Order stopCand = stopKvp.Value;
            if (stopCand == null)
                continue;
            if (stopCand.Account == null)
                continue;

            // If the stop order's original account matches our current iteration account
            if (string.Equals(stopCand.Account.Name, acct.Name, StringComparison.OrdinalIgnoreCase))
            {
                recoveredKey = stopKvp.Key;
                recoveredStop = stopCand;
                break;
            }
        }

        if (recoveredKey == null)
        {
            Print(
                string.Format(
                    "[SIMA] Phase 5 Position Pass: WARNING -- open position on {0} but no stopOrders key found. FSM not created. REAPER grace window started.",
                    acct.Name
                )
            );
            // Build 999: Mark account for REAPER grace window -- defer critical desync up to 10s.
            // CancelPending stop (stop-replace mid-flight at disable) causes this warning.
            // The replace cycle resolves within seconds; grace prevents premature flatten cascade.
            _positionPassFailedFirstSeen[acct.Name] = DateTime.UtcNow;
            continue;
        }

        // Idempotent guard
        if (_followerBrackets.ContainsKey(recoveredKey))
            continue;

        // Create temporary PositionInfo for BuildFSM signature compatibility
        var tempPi = new PositionInfo { ExecutingAccount = acct };
        var fsm = BuildFSM(
            recoveredKey,
            tempPi,
            null, // Terminal entry order
            FollowerBracketState.Active,
            Math.Abs(acctPos.Quantity)
        );

        // Link stop order
        if (recoveredStop != null)
        {
            fsm.StopOrder = recoveredStop;
            if (!string.IsNullOrEmpty(recoveredStop.OrderId))
            {
                _orderIdToFsmKey[recoveredStop.OrderId] = recoveredKey;
                ordersIndexed++;
            }
        }

        // Link target orders
        LinkTargetOrder(fsm, target1Orders, recoveredKey, 0, ref ordersIndexed);
        LinkTargetOrder(fsm, target2Orders, recoveredKey, 1, ref ordersIndexed);
        LinkTargetOrder(fsm, target3Orders, recoveredKey, 2, ref ordersIndexed);
        LinkTargetOrder(fsm, target4Orders, recoveredKey, 3, ref ordersIndexed);
        LinkTargetOrder(fsm, target5Orders, recoveredKey, 4, ref ordersIndexed);

        RegisterFSM(recoveredKey, fsm, null, ref ordersIndexed, ref fsmCreated);
        positionFsmCreated++;

        Print(
            string.Format(
                "[SIMA] Phase 5 Position Pass: Created FSM for {0} (key={1})",
                acct.Name,
                recoveredKey
            )
        );
    }
    
    return positionFsmCreated;
}
```

### 3. Update Caller
**Location**: Lines 602-750 in `HydrateFSMsFromWorkingOrders`

**Before** (141 lines):
```csharp
// Position Pass: handle accounts with open positions but terminal entry orders
int positionFsmCreated = 0;
foreach (Account acct in Account.All)
{
    // ... 141 lines of logic ...
}

Print(
    string.Format(
        "[SIMA] Phase 5 FSM Hydration (Position Pass): {0} Active FSMs created from open positions.",
        positionFsmCreated
    )
);
```

**After** (5 lines):
```csharp
// Position Pass: handle accounts with open positions but terminal entry orders
int positionFsmCreated = HydrateFromOpenPositions(
    stopOrders, target1Orders, target2Orders, target3Orders, target4Orders, target5Orders,
    ref ordersIndexed, ref fsmCreated
);

Print(
    string.Format(
        "[SIMA] Phase 5 FSM Hydration (Position Pass): {0} Active FSMs created from open positions.",
        positionFsmCreated
    )
);
```

### 4. Verification Gates
```powershell
powershell -File .\scripts\build_readiness.ps1
dotnet test tests/V12_Performance.Tests/ --filter "FullyQualifiedName~HydrateFromOpenPositions"
python scripts/complexity_audit.py
powershell -File .\scripts\lint.ps1
```

## Acceptance Criteria
- ✅ 5 test cases pass
- ✅ Build passes
- ✅ Method complexity: CYC ≤8
- ✅ Caller updated (141 lines → 5 lines)
- ✅ REAPER grace window logic preserved
- ✅ Logging preserved
- ✅ Behavior preserved
- ✅ Git commit created

## Dependencies
- **Upstream**: Tickets 02 (BuildFSM), 03 (LinkTargetOrder), 05 (RegisterFSM)
- **Downstream**: Ticket 07 (Refactor Parent) will use this

## Complexity Reduction
- **Before**: 141 lines inline, CYC ~15
- **After**: 120 lines method + 5 lines call, CYC ~8

## Risk Mitigation
- Most complex extraction (nested loops, multiple edge cases)
- Extensive test coverage (5 test cases)
- Preserves REAPER grace window logic (critical for production)
- Preserves all logging (debugging aid)

## Notes
- Largest extraction (141 lines)
- Highest complexity (CYC ~8)
- Most edge cases (no position, no stop order, existing FSM, etc.)
- Critical production logic (REAPER grace window)