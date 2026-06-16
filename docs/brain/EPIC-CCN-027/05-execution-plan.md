# Phase 5 Execution Plan: EPIC-CCN-027

## Environment Requirements
- **Platform**: Windows with .NET SDK 6.0+
- **IDE**: Visual Studio 2022 or VS Code with C# extension
- **NinjaTrader**: Version 8.0+ installed
- **Tools**: CSharpier, Python 3.12+ (for complexity audit)

## Execution Status
- **Phase**: 5 (Ticket Execution)
- **Status**: READY FOR EXECUTION
- **Environment**: Requires Windows/.NET (Linux environment detected - execution deferred)
- **Test File Created**: `tests/V12_Performance.Tests/Core/SIMADispatchTests.cs`

---

## TICKET-1: Extract CreateBracketOrders (Pure Function)

### Current State Analysis
**File**: `src/V12_002.SIMA.Dispatch.cs`
**Method**: `Dispatch_PublishMarketBracketToPhoton`
**Lines to Extract**: 606-710 (105 lines)
**Current CYC**: 21
**Target CYC**: ≤8

### Extraction Scope
```csharp
// Lines 606-710: Order creation and validation logic
// EXTRACT THIS BLOCK into private method CreateBracketOrders()

var ordersToSubmit = new List<Order> { entry };
OrderAction exitAction = action == OrderAction.Buy ? OrderAction.Sell : OrderAction.BuyToCover;
double validatedStop = ValidateStopPrice(fleetPos.Direction, fleetPos.CurrentStopPrice);

string stopSig = SymmetryTrim("Stop_" + fleetEntryName, 40);
Order stop = acct.CreateOrder(
    Instrument,
    exitAction,
    OrderType.StopMarket,
    TimeInForce.Gtc,
    Math.Max(1, fleetPos.TotalContracts),
    0,
    validatedStop,
    ocoId,
    stopSig,
    null
);

ordersToSubmit.Add(stop);

int nonRunnerLimitQty = 0;
int runnerQty = 0;
var stagedTargets = new List<StagedTarget>(5);

for (int targetNum = 1; targetNum <= dispatchTargetCount; targetNum++)
{
    int targetQty = GetTargetContracts(fleetPos, targetNum);
    if (targetQty <= 0)
        continue;

    if (IsRunnerTarget(targetNum))
    {
        runnerQty += targetQty;
        continue;
    }

    double targetPrice = GetTargetPrice(fleetPos, targetNum);
    if (targetPrice <= 0)
    {
        dispatchLog.AppendLine(
            LogBuffer.Format(
                "[SIMA TARGET_SKIP] T{0} for {1} has qty={2} but invalid price={3:F2}; skipped",
                targetNum,
                fleetEntryName,
                targetQty,
                targetPrice
            )
        );
        continue;
    }

    string targetSig = SymmetryTrim("T" + targetNum + "_" + fleetEntryName, 40);
    Order target = acct.CreateOrder(
        Instrument,
        exitAction,
        OrderType.Limit,
        TimeInForce.Gtc,
        targetQty,
        targetPrice,
        0,
        ocoId,
        targetSig,
        null
    );

    stagedTargets.Add(
        new StagedTarget
        {
            Num = targetNum,
            Price = targetPrice,
            Order = target,
        }
    );

    ordersToSubmit.Add(target);
    nonRunnerLimitQty += targetQty;
}
```

### Step 1: Define Return Type (BracketOrderSet struct)

**Location**: Add to `src/V12_002.SIMA.Dispatch.cs` (near other structs)

```csharp
private struct BracketOrderSet
{
    public Order Entry;
    public Order Stop;
    public List<Order> OrdersToSubmit;
    public List<StagedTarget> StagedTargets;
    public int NonRunnerLimitQty;
    public int RunnerQty;
}
```

### Step 2: Extract Method Signature

```csharp
private BracketOrderSet CreateBracketOrders(
    Account acct,
    OrderAction action,
    Order entry,
    PositionInfo fleetPos,
    string fleetEntryName,
    string ocoId,
    int dispatchTargetCount,
    StringBuilder dispatchLog
)
{
    // Extracted logic from lines 606-710
    var ordersToSubmit = new List<Order> { entry };
    OrderAction exitAction = action == OrderAction.Buy ? OrderAction.Sell : OrderAction.BuyToCover;
    double validatedStop = ValidateStopPrice(fleetPos.Direction, fleetPos.CurrentStopPrice);

    string stopSig = SymmetryTrim("Stop_" + fleetEntryName, 40);
    Order stop = acct.CreateOrder(
        Instrument,
        exitAction,
        OrderType.StopMarket,
        TimeInForce.Gtc,
        Math.Max(1, fleetPos.TotalContracts),
        0,
        validatedStop,
        ocoId,
        stopSig,
        null
    );

    ordersToSubmit.Add(stop);

    int nonRunnerLimitQty = 0;
    int runnerQty = 0;
    var stagedTargets = new List<StagedTarget>(5);

    for (int targetNum = 1; targetNum <= dispatchTargetCount; targetNum++)
    {
        int targetQty = GetTargetContracts(fleetPos, targetNum);
        if (targetQty <= 0)
            continue;

        if (IsRunnerTarget(targetNum))
        {
            runnerQty += targetQty;
            continue;
        }

        double targetPrice = GetTargetPrice(fleetPos, targetNum);
        if (targetPrice <= 0)
        {
            dispatchLog.AppendLine(
                LogBuffer.Format(
                    "[SIMA TARGET_SKIP] T{0} for {1} has qty={2} but invalid price={3:F2}; skipped",
                    targetNum,
                    fleetEntryName,
                    targetQty,
                    targetPrice
                )
            );
            continue;
        }

        string targetSig = SymmetryTrim("T" + targetNum + "_" + fleetEntryName, 40);
        Order target = acct.CreateOrder(
            Instrument,
            exitAction,
            OrderType.Limit,
            TimeInForce.Gtc,
            targetQty,
            targetPrice,
            0,
            ocoId,
            targetSig,
            null
        );

        stagedTargets.Add(
            new StagedTarget
            {
                Num = targetNum,
                Price = targetPrice,
                Order = target,
            }
        );

        ordersToSubmit.Add(target);
        nonRunnerLimitQty += targetQty;
    }

    return new BracketOrderSet
    {
        Entry = entry,
        Stop = stop,
        OrdersToSubmit = ordersToSubmit,
        StagedTargets = stagedTargets,
        NonRunnerLimitQty = nonRunnerLimitQty,
        RunnerQty = runnerQty
    };
}
```

### Step 3: Update Orchestrator Call Site

**Replace lines 606-710 with:**

```csharp
var bracketOrders = CreateBracketOrders(
    acct,
    action,
    entry,
    fleetPos,
    fleetEntryName,
    ocoId,
    dispatchTargetCount,
    dispatchLog
);

var ordersToSubmit = bracketOrders.OrdersToSubmit;
var stop = bracketOrders.Stop;
var stagedTargets = bracketOrders.StagedTargets;
int nonRunnerLimitQty = bracketOrders.NonRunnerLimitQty;
int runnerQty = bracketOrders.RunnerQty;
```

### Step 4: Complete Test Implementation

**Update**: `tests/V12_Performance.Tests/Core/SIMADispatchTests.cs`

```csharp
[Test]
public void CreateBracketOrders_ValidInputs_ReturnsCompleteOrderSet()
{
    // Arrange
    var mockAccount = CreateMockAccount();
    var mockEntry = CreateMockOrder(OrderAction.Buy);
    var mockFleetPos = CreateMockPositionInfo(10, 100.0, 99.0);
    var dispatchLog = new StringBuilder();
    
    // Act
    var result = CreateBracketOrders(
        mockAccount,
        OrderAction.Buy,
        mockEntry,
        mockFleetPos,
        "TEST_ENTRY",
        "OCO_123",
        3,
        dispatchLog
    );
    
    // Assert
    Assert.IsNotNull(result.Entry);
    Assert.IsNotNull(result.Stop);
    Assert.AreEqual(4, result.OrdersToSubmit.Count); // Entry + Stop + 3 Targets
    Assert.AreEqual(3, result.StagedTargets.Count);
    Assert.Greater(result.NonRunnerLimitQty, 0);
}

[Test]
public void CreateBracketOrders_InvalidTargetPrice_SkipsTarget()
{
    // Arrange
    var mockAccount = CreateMockAccount();
    var mockEntry = CreateMockOrder(OrderAction.Buy);
    var mockFleetPos = CreateMockPositionInfoWithInvalidTarget();
    var dispatchLog = new StringBuilder();
    
    // Act
    var result = CreateBracketOrders(
        mockAccount,
        OrderAction.Buy,
        mockEntry,
        mockFleetPos,
        "TEST_ENTRY",
        "OCO_123",
        3,
        dispatchLog
    );
    
    // Assert
    Assert.Less(result.StagedTargets.Count, 3); // Some targets skipped
    Assert.IsTrue(dispatchLog.ToString().Contains("TARGET_SKIP"));
}

// Add remaining 4 test cases following same pattern
```

### Step 5: Verification Commands

```powershell
# Run tests (should be GREEN after extraction)
dotnet test --filter "FullyQualifiedName~SIMADispatchTests.CreateBracketOrders"

# Complexity audit (should show CYC ≤8)
python scripts/complexity_audit.py

# Format code
dotnet csharpier format src/V12_002.SIMA.Dispatch.cs

# Build verification
dotnet build
```

### Success Criteria
- [ ] BracketOrderSet struct defined
- [ ] CreateBracketOrders method extracted (CYC ≤8)
- [ ] All 6 tests GREEN
- [ ] Build passes (zero errors)
- [ ] Formatting applied
- [ ] Complexity audit PASS

---

## TICKET-2: Extract RegisterBracketState (State Registration)

### Current State Analysis
**File**: `src/V12_002.SIMA.Dispatch.cs`
**Method**: `Dispatch_PublishMarketBracketToPhoton`
**Lines to Extract**: 712-760 (49 lines)
**Current CYC**: ~13 (after TICKET-1)
**Target CYC**: ≤8

### Extraction Scope
```csharp
// Lines 712-760: Dictionary registration and FSM creation
// EXTRACT THIS BLOCK into private method RegisterBracketState()

activePositions[fleetEntryName] = fleetPos;
entryOrders[fleetEntryName] = entry;
stopOrders[fleetEntryName] = stop;
foreach (var st in stagedTargets)
{
    var targetDict = GetTargetOrdersDictionary(st.Num);
    if (targetDict != null)
        targetDict[fleetEntryName] = st.Order;
}
registeredForCleanup = true;
MarkDispatchSyncPending(expectedKey);
syncPending = true;

if (!_followerBrackets.ContainsKey(fleetEntryName))
{
    var proFsm = new FollowerBracketFSM
    {
        AccountName = acct.Name,
        EntryName = fleetEntryName,
        State = FollowerBracketState.PendingSubmit,
        RemainingContracts = followerQty,
        EntryOrder = entry,
        ExpectedEntryPrice = entry.LimitPrice > 0 ? entry.LimitPrice : 0,
        StopOrder = stop,
        ExpectedStopPrice = stop != null ? stop.StopPrice : 0,
        OcoGroupId = ocoId,
        LastUpdateUtc = DateTime.UtcNow,
    };
    foreach (var st in stagedTargets)
    {
        if (st.Num >= 1 && st.Num <= 5)
        {
            proFsm.Targets[st.Num - 1] = st.Order;
            proFsm.ExpectedTargetPrices[st.Num - 1] = st.Price;
        }
    }
    _followerBrackets.TryAdd(fleetEntryName, proFsm);
}

reservedDelta = (action == OrderAction.Buy) ? followerQty : -followerQty;
AddExpectedPositionDeltaLocked(expectedKey, reservedDelta);
```

### Step 1: Extract Method Signature

```csharp
private void RegisterBracketState(
    BracketOrderSet bracketOrders,
    Account acct,
    OrderAction action,
    string fleetEntryName,
    string expectedKey,
    string ocoId,
    int followerQty,
    ref bool syncPending,
    ref int reservedDelta,
    ref bool registeredForCleanup
)
{
    // Register in dictionaries
    activePositions[fleetEntryName] = fleetPos;
    entryOrders[fleetEntryName] = bracketOrders.Entry;
    stopOrders[fleetEntryName] = bracketOrders.Stop;
    
    foreach (var st in bracketOrders.StagedTargets)
    {
        var targetDict = GetTargetOrdersDictionary(st.Num);
        if (targetDict != null)
            targetDict[fleetEntryName] = st.Order;
    }
    
    registeredForCleanup = true;
    MarkDispatchSyncPending(expectedKey);
    syncPending = true;

    // Create FSM (atomic via TryAdd)
    if (!_followerBrackets.ContainsKey(fleetEntryName))
    {
        var proFsm = new FollowerBracketFSM
        {
            AccountName = acct.Name,
            EntryName = fleetEntryName,
            State = FollowerBracketState.PendingSubmit,
            RemainingContracts = followerQty,
            EntryOrder = bracketOrders.Entry,
            ExpectedEntryPrice = bracketOrders.Entry.LimitPrice > 0 ? bracketOrders.Entry.LimitPrice : 0,
            StopOrder = bracketOrders.Stop,
            ExpectedStopPrice = bracketOrders.Stop != null ? bracketOrders.Stop.StopPrice : 0,
            OcoGroupId = ocoId,
            LastUpdateUtc = DateTime.UtcNow,
        };
        
        foreach (var st in bracketOrders.StagedTargets)
        {
            if (st.Num >= 1 && st.Num <= 5)
            {
                proFsm.Targets[st.Num - 1] = st.Order;
                proFsm.ExpectedTargetPrices[st.Num - 1] = st.Price;
            }
        }
        
        _followerBrackets.TryAdd(fleetEntryName, proFsm);
    }

    // Reserve position delta
    reservedDelta = (action == OrderAction.Buy) ? followerQty : -followerQty;
    AddExpectedPositionDeltaLocked(expectedKey, reservedDelta);
}
```

### Step 2: Update Orchestrator Call Site

**Replace lines 712-760 with:**

```csharp
RegisterBracketState(
    bracketOrders,
    acct,
    action,
    fleetEntryName,
    expectedKey,
    ocoId,
    followerQty,
    ref syncPending,
    ref reservedDelta,
    ref registeredForCleanup
);
```

### Step 3: Add Tests

```csharp
[Test]
public void RegisterBracketState_ValidOrders_RegistersInAllDictionaries()
{
    // Arrange
    var bracketOrders = CreateMockBracketOrderSet();
    
    // Act
    RegisterBracketState(bracketOrders, ...);
    
    // Assert
    Assert.IsTrue(activePositions.ContainsKey("TEST_ENTRY"));
    Assert.IsTrue(entryOrders.ContainsKey("TEST_ENTRY"));
    Assert.IsTrue(stopOrders.ContainsKey("TEST_ENTRY"));
}

[Test]
public void RegisterBracketState_NewBracket_CreatesFSMWithPendingSubmitState()
{
    // Arrange
    var bracketOrders = CreateMockBracketOrderSet();
    
    // Act
    RegisterBracketState(bracketOrders, ...);
    
    // Assert
    Assert.IsTrue(_followerBrackets.ContainsKey("TEST_ENTRY"));
    Assert.AreEqual(FollowerBracketState.PendingSubmit, _followerBrackets["TEST_ENTRY"].State);
}

// Add remaining 2 test cases
```

### Step 4: Verification Commands

```powershell
# Lock-free verification
grep -n "lock(" src/V12_002.SIMA.Dispatch.cs  # Should return zero matches

# Run tests
dotnet test --filter "FullyQualifiedName~SIMADispatchTests.RegisterBracketState"

# Complexity audit
python scripts/complexity_audit.py

# Build
dotnet build
```

### Success Criteria
- [ ] RegisterBracketState method extracted (CYC ≤8)
- [ ] All 4 tests GREEN
- [ ] Lock-free verified (zero lock() statements)
- [ ] FSM ordering invariant preserved
- [ ] Build passes

---

## TICKET-3: Extract DispatchToPhotonKernel (Zero-Allocation Dispatch)

### Current State Analysis
**File**: `src/V12_002.SIMA.Dispatch.cs`
**Method**: `Dispatch_PublishMarketBracketToPhoton`
**Lines to Extract**: 762-795 (34 lines)
**Current CYC**: ~8 (after TICKET-2)
**Target CYC**: ≤8 (orchestrator + helper)

### Extraction Scope
```csharp
// Lines 762-795: PhotonPool claim and kernel enqueue
// EXTRACT THIS BLOCK into private method DispatchToPhotonKernel()

int _poolSlotIndex = -1;
Order[] _proxyOrders = null;
{
    var _claimed = _photonPool.Claim();
    if (_claimed.Orders != null)
    {
        _proxyOrders = _claimed.Orders;
        _poolSlotIndex = _claimed.SlotIndex;
    }
    else
    {
        Print("[PHOTON] Pool exhausted -- fallback to heap alloc");
        _proxyOrders = new Order[MaxOrdersPerSlot];
        _poolSlotIndex = -1;
    }
}

int _orderIdx = 0;
_proxyOrders[_orderIdx++] = entry;
_proxyOrders[_orderIdx++] = stop;
foreach (var _st in stagedTargets)
    _proxyOrders[_orderIdx++] = _st.Order;

FleetDispatchSlot _slot = new FleetDispatchSlot
{
    EntryPrice = entryPrice,
    StopPrice = stopPrice,
    SignalTicks = DateTime.UtcNow.Ticks,
    PoolSlotIndex = _poolSlotIndex,
    OrderCount = _orderIdx,
    Quantity = followerQty,
    TargetCount = dispatchTargetCount,
    Action = (int)action,
    ReservedDelta = reservedDelta,
};
_slot.Shadow = ComputeFleetDispatchShadow(ref _slot, _photonShadowSalt);

// Enqueue to kernel (continues on next line...)
```

### Step 1: Extract Method Signature

```csharp
private void DispatchToPhotonKernel(
    BracketOrderSet bracketOrders,
    double entryPrice,
    double stopPrice,
    int followerQty,
    int dispatchTargetCount,
    OrderAction action,
    int reservedDelta
)
{
    int _poolSlotIndex = -1;
    Order[] _proxyOrders = null;
    
    // Claim from pool or fallback to heap
    var _claimed = _photonPool.Claim();
    if (_claimed.Orders != null)
    {
        _proxyOrders = _claimed.Orders;
        _poolSlotIndex = _claimed.SlotIndex;
    }
    else
    {
        Print("[PHOTON] Pool exhausted -- fallback to heap alloc");
        _proxyOrders = new Order[MaxOrdersPerSlot];
        _poolSlotIndex = -1;
    }

    // Populate proxy array
    int _orderIdx = 0;
    _proxyOrders[_orderIdx++] = bracketOrders.Entry;
    _proxyOrders[_orderIdx++] = bracketOrders.Stop;
    foreach (var _st in bracketOrders.StagedTargets)
        _proxyOrders[_orderIdx++] = _st.Order;

    // Build dispatch slot
    FleetDispatchSlot _slot = new FleetDispatchSlot
    {
        EntryPrice = entryPrice,
        StopPrice = stopPrice,
        SignalTicks = DateTime.UtcNow.Ticks,
        PoolSlotIndex = _poolSlotIndex,
        OrderCount = _orderIdx,
        Quantity = followerQty,
        TargetCount = dispatchTargetCount,
        Action = (int)action,
        ReservedDelta = reservedDelta,
    };
    _slot.Shadow = ComputeFleetDispatchShadow(ref _slot, _photonShadowSalt);

    // Enqueue to kernel
    _photonKernel.Enqueue(new PhotonDispatchCommand
    {
        Slot = _slot,
        Orders = _proxyOrders
    });
}
```

### Step 2: Update Orchestrator Call Site

**Replace lines 762-795 with:**

```csharp
DispatchToPhotonKernel(
    bracketOrders,
    entryPrice,
    stopPrice,
    followerQty,
    dispatchTargetCount,
    action,
    reservedDelta
);
```

### Step 3: Add Tests

```csharp
[Test]
public void DispatchToPhotonKernel_PoolAvailable_ClaimsSlotSuccessfully()
{
    // Arrange
    var bracketOrders = CreateMockBracketOrderSet();
    
    // Act
    DispatchToPhotonKernel(bracketOrders, ...);
    
    // Assert
    Assert.Greater(_photonPool.ClaimedCount, 0);
}

[Test]
public void DispatchToPhotonKernel_ValidOrders_PopulatesProxyArray()
{
    // Arrange
    var bracketOrders = CreateMockBracketOrderSet();
    
    // Act
    DispatchToPhotonKernel(bracketOrders, ...);
    
    // Assert
    // Verify proxy array populated correctly
}

[Test]
public void DispatchToPhotonKernel_Success_EnqueuesToKernel()
{
    // Arrange
    var bracketOrders = CreateMockBracketOrderSet();
    
    // Act
    DispatchToPhotonKernel(bracketOrders, ...);
    
    // Assert
    Assert.AreEqual(1, _photonKernel.QueueDepth);
}

// Add remaining 3 test cases + 1 stress test
```

### Step 4: Final Orchestrator Verification

**After all 3 extractions, orchestrator should look like:**

```csharp
private void Dispatch_PublishMarketBracketToPhoton(
    Account acct,
    OrderAction action,
    Order entry,
    PositionInfo fleetPos,
    string fleetEntryName,
    string expectedKey,
    string ocoId,
    int followerQty,
    double entryPrice,
    double stopPrice,
    int dispatchTargetCount,
    StringBuilder dispatchLog,
    ref bool syncPending,
    ref int reservedDelta,
    ref bool registeredForCleanup
)
{
    // Step 1: Create bracket orders (pure function)
    var bracketOrders = CreateBracketOrders(
        acct,
        action,
        entry,
        fleetPos,
        fleetEntryName,
        ocoId,
        dispatchTargetCount,
        dispatchLog
    );

    // Step 2: Register state (controlled side effects)
    RegisterBracketState(
        bracketOrders,
        acct,
        action,
        fleetEntryName,
        expectedKey,
        ocoId,
        followerQty,
        ref syncPending,
        ref reservedDelta,
        ref registeredForCleanup
    );

    // Step 3: Dispatch to kernel (zero-allocation)
    DispatchToPhotonKernel(
        bracketOrders,
        entryPrice,
        stopPrice,
        followerQty,
        dispatchTargetCount,
        action,
        reservedDelta
    );
}
```

**Expected CYC**: 3-5 (three sequential calls + minimal branching)

### Step 5: Integration Tests

```csharp
[Test]
public void EndToEnd_ValidBracket_CompletesFullDispatchCycle()
{
    // Arrange
    var mockAccount = CreateMockAccount();
    var mockEntry = CreateMockOrder(OrderAction.Buy);
    var mockFleetPos = CreateMockPositionInfo(10, 100.0, 99.0);
    
    // Act
    Dispatch_PublishMarketBracketToPhoton(
        mockAccount,
        OrderAction.Buy,
        mockEntry,
        mockFleetPos,
        "TEST_ENTRY",
        "EXPECTED_KEY",
        "OCO_123",
        10,
        100.0,
        99.0,
        3,
        new StringBuilder(),
        ref syncPending,
        ref reservedDelta,
        ref registeredForCleanup
    );
    
    // Assert
    Assert.IsTrue(_followerBrackets.ContainsKey("TEST_ENTRY"));
    Assert.AreEqual(FollowerBracketState.PendingSubmit, _followerBrackets["TEST_ENTRY"].State);
    Assert.AreEqual(1, _photonKernel.QueueDepth);
}

// Add 2 more integration tests
```

### Step 6: Final Verification Commands

```powershell
# Full test suite
dotnet test --filter "FullyQualifiedName~SIMADispatchTests"

# Complexity audit (all methods should be ≤8)
python scripts/complexity_audit.py

# Format code
dotnet csharpier format src/

# Build
dotnet build

# Sync hard links
powershell -File .\deploy-sync.ps1
```

### Success Criteria
- [ ] DispatchToPhotonKernel method extracted (CYC ≤8)
- [ ] All 6 unit tests GREEN
- [ ] All 3 integration tests GREEN
- [ ] Orchestrator CYC ≤8
- [ ] Zero-allocation pattern preserved
- [ ] Build passes
- [ ] Hard-link sync succeeds

---

## Final Validation Checklist

### Code Quality
- [ ] All 19 tests passing (16 unit + 3 integration)
- [ ] Orchestrator CYC ≤8
- [ ] All helper methods CYC ≤8
- [ ] Zero lock() statements (lock-free validation)
- [ ] Formatting applied (CSharpier)

### Build & Deploy
- [ ] `dotnet build` succeeds (zero errors)
- [ ] `powershell -File .\deploy-sync.ps1` succeeds
- [ ] ASCII-only compliance verified

### Documentation
- [ ] Ticket completion files created (ticket-1-completion.md, ticket-2-completion.md, ticket-3-completion.md)
- [ ] Manifest.json updated with Phase 5 status
- [ ] Complexity metrics documented (before/after)

---

## Execution Timeline

**Estimated Duration**: 6-8 hours
- TICKET-1: 2-3 hours (TDD cycle + struct definition)
- TICKET-2: 2-3 hours (TDD cycle + FSM verification)
- TICKET-3: 2-3 hours (TDD cycle + integration tests)

**Prerequisites**:
- Windows environment with .NET SDK 6.0+
- NinjaTrader 8.0+ installed
- All Phase 0-4 artifacts reviewed

**Next Steps**:
1. Execute in Windows/.NET environment
2. Follow TDD cycle (Red → Green → Refactor) per ticket
3. Document completion in ticket-X-completion.md files
4. Update manifest.json with final status
5. Proceed to Phase 5.V (Verification)

---

**Document Version**: 1.0
**Created**: 2026-06-15
**Epic**: EPIC-CCN-027
**Phase**: 5 (Execution Plan)
**Status**: READY FOR EXECUTION (Windows/.NET required)
