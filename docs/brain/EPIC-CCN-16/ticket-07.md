# EPIC-CCN-16 Ticket 07: Refactor Parent Method

## Ticket Metadata
- **Epic**: EPIC-CCN-16
- **Ticket**: 07 of 7
- **Priority**: P4 (Final Integration, Depends on All Previous)
- **Estimated Duration**: 30 minutes
- **Complexity**: CYC ~8
- **Risk Level**: LOW

## Objective
Refactor `HydrateFSMsFromWorkingOrders` to use all extracted methods, reducing it to a simple orchestration method.

## Current Code Location
**File**: [`src/V12_002.SIMA.Lifecycle.cs`](src/V12_002.SIMA.Lifecycle.cs:464)
**Lines**: 464-759 (295 lines, CYC 45)

## Target State
**Lines**: ~60 lines
**Complexity**: CYC ≤8
**Role**: Orchestration only (delegates to extracted methods)

## Implementation Steps

### 1. Write Integration Tests FIRST (TDD)
**File**: `tests/V12_Performance.Tests/Core/FSMHydrationTests.cs`

```csharp
public class HydrateFSMsFromWorkingOrdersIntegrationTests
{
    [Fact]
    public void HydrateFSMsFromWorkingOrders_CreatesExpectedFSMs()
    {
        // Arrange
        var sut = new V12_002();
        // Set up mock orders and positions
        sut.entryOrders["TEST_ENTRY"] = new Order { OrderState = OrderState.Filled, Quantity = 10 };
        sut.stopOrders["TEST_ENTRY"] = new Order { OrderId = "STOP123" };
        sut.target1Orders["TEST_ENTRY"] = new Order { OrderId = "T1_123" };
        var pi = new PositionInfo { IsFollower = true, ExecutingAccount = new Account { Name = "TestAccount" } };
        sut.activePositions["TEST_ENTRY"] = pi;

        // Act
        sut.HydrateFSMsFromWorkingOrders();

        // Assert
        Assert.True(sut._followerBrackets.ContainsKey("TEST_ENTRY"));
        Assert.Equal(FollowerBracketState.Active, sut._followerBrackets["TEST_ENTRY"].State);
        Assert.Equal(3, sut._orderIdToFsmKey.Count); // Entry + Stop + Target1
    }

    [Fact]
    public void HydrateFSMsFromWorkingOrders_IsIdempotent()
    {
        // Arrange
        var sut = new V12_002();
        sut.entryOrders["TEST_ENTRY"] = new Order { OrderState = OrderState.Filled, Quantity = 10 };
        var pi = new PositionInfo { IsFollower = true, ExecutingAccount = new Account { Name = "TestAccount" } };
        sut.activePositions["TEST_ENTRY"] = pi;

        // Act
        sut.HydrateFSMsFromWorkingOrders();
        int firstCount = sut._followerBrackets.Count;
        sut.HydrateFSMsFromWorkingOrders(); // Call again

        // Assert
        Assert.Equal(firstCount, sut._followerBrackets.Count); // No duplicates
    }
}
```

### 2. Refactor Parent Method
**Location**: Lines 464-759

**After** (orchestration only):
```csharp
/// <summary>
/// Phase 5: Rebuilds _followerBrackets and _orderIdToFsmKey from already-adopted
/// working orders. Called from HydrateWorkingOrdersFromBroker() before the
/// adoption-complete gate is set. Idempotent -- safe to call on every reconnect.
/// </summary>
private void HydrateFSMsFromWorkingOrders()
{
    int fsmCreated = 0;
    int ordersIndexed = 0;

    // Entry Order Pass: Process all entry orders
    foreach (var kvp in entryOrders.ToArray())
    {
        string entryKey = kvp.Key;
        Order entryOrder = kvp.Value;
        if (entryOrder == null)
            continue;

        // Skip master account entries
        PositionInfo pi;
        if (!activePositions.TryGetValue(entryKey, out pi) || !pi.IsFollower)
            continue;
        if (pi.ExecutingAccount == null)
            continue;

        // Idempotent guard
        if (_followerBrackets.ContainsKey(entryKey))
            continue;

        // Map state
        FollowerBracketState? hydrationState = MapOrderStateToFSMState(entryOrder.OrderState);
        if (hydrationState == null)
            continue; // Terminal state -- FSM not needed

        // Resolve contracts
        int hydratedRemainingContracts = ResolveRemainingContracts(entryOrder, pi, hydrationState.Value);

        // Build FSM
        var fsm = BuildFSM(entryKey, pi, entryOrder, hydrationState.Value, hydratedRemainingContracts);

        // Link stop order
        Order stopOrd;
        if (stopOrders.TryGetValue(entryKey, out stopOrd) && stopOrd != null)
        {
            fsm.StopOrder = stopOrd;
            if (!string.IsNullOrEmpty(stopOrd.OrderId))
            {
                _orderIdToFsmKey[stopOrd.OrderId] = entryKey;
                ordersIndexed++;
            }
        }

        // Link target orders
        LinkTargetOrder(fsm, target1Orders, entryKey, 0, ref ordersIndexed);
        LinkTargetOrder(fsm, target2Orders, entryKey, 1, ref ordersIndexed);
        LinkTargetOrder(fsm, target3Orders, entryKey, 2, ref ordersIndexed);
        LinkTargetOrder(fsm, target4Orders, entryKey, 3, ref ordersIndexed);
        LinkTargetOrder(fsm, target5Orders, entryKey, 4, ref ordersIndexed);

        // Register FSM
        RegisterFSM(entryKey, fsm, entryOrder, ref ordersIndexed, ref fsmCreated);
    }

    // Position Pass: Handle accounts with open positions but terminal entry orders
    int positionFsmCreated = HydrateFromOpenPositions(
        stopOrders,
        target1Orders,
        target2Orders,
        target3Orders,
        target4Orders,
        target5Orders,
        ref ordersIndexed,
        ref fsmCreated
    );

    Print(
        string.Format(
            "[SIMA] Phase 5 FSM Hydration (Position Pass): {0} Active FSMs created from open positions.",
            positionFsmCreated
        )
    );

    Print(
        string.Format(
            "[SIMA] Phase 5 FSM Hydration: {0} FSMs created, {1} order IDs indexed.",
            fsmCreated,
            ordersIndexed
        )
    );
}
```

### 3. Verification Gates
```powershell
# Build
powershell -File .\scripts\build_readiness.ps1

# All tests
dotnet test tests/V12_Performance.Tests/

# Complexity audit
python scripts/complexity_audit.py

# Verify target method
python scripts/complexity_audit.py | Select-String "HydrateFSMsFromWorkingOrders"

# Lint
powershell -File .\scripts\lint.ps1

# Deploy sync
powershell -File .\deploy-sync.ps1
```

### 4. Final Verification
```powershell
# F5 in NinjaTrader
# Verify no runtime errors
# Verify FSM hydration logs appear
# Verify REAPER operates correctly
```

## Acceptance Criteria
- ✅ 2 integration test cases pass
- ✅ All 29 unit tests pass (from tickets 01-06)
- ✅ Build passes
- ✅ Method complexity: CYC ≤8 (down from 45)
- ✅ Method lines: ~60 (down from 295)
- ✅ Behavior preserved (zero functional changes)
- ✅ deploy-sync.ps1 succeeds
- ✅ F5 in NinjaTrader successful
- ✅ Git commit created

## Dependencies
- **Upstream**: ALL previous tickets (01-06)
- **Downstream**: None (final ticket)

## Complexity Reduction Summary
- **Before**: 295 lines, CYC 45
- **After**: ~60 lines, CYC ~8
- **Reduction**: 79.7% lines, 82.2% complexity

## Extracted Methods Summary
1. **MapOrderStateToFSMState**: CYC ~8, 20 lines
2. **BuildFSM**: CYC ~1, 12 lines
3. **LinkTargetOrder**: CYC ~2, 12 lines
4. **ResolveRemainingContracts**: CYC ~3, 18 lines
5. **RegisterFSM**: CYC ~2, 12 lines
6. **HydrateFromOpenPositions**: CYC ~8, 120 lines

**Total**: 7 methods (1 refactored + 6 new), average CYC ~4.1

## Final Verification Checklist
- [ ] All 29 tests pass
- [ ] Build passes (zero errors, zero warnings)
- [ ] Complexity audit: all methods ≤8
- [ ] Lint passes
- [ ] deploy-sync.ps1 succeeds
- [ ] F5 in NinjaTrader successful
- [ ] No runtime errors in logs
- [ ] FSM hydration logs appear
- [ ] REAPER operates correctly
- [ ] Git commit created with message: "EPIC-CCN-16: Extract HydrateFSMsFromWorkingOrders (CYC 45 → 8)"

## Notes
- Final integration ticket
- Depends on all previous tickets
- Simple orchestration (delegates to extracted methods)
- Preserves exact behavior
- Achieves 82.2% complexity reduction