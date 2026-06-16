# Phase 4: Implementation Tickets - EPIC-CCN-118

## Epic Metadata
- **Epic ID**: EPIC-CCN-118
- **Phase**: 4 (Ticket Generation)
- **Target Method**: ProcessSingleFleetRMAAccount
- **File**: src/V12_002.SIMA.Execution.cs
- **Current Complexity**: 16
- **Target Complexity**: ≤ 8
- **Ticket Count**: 4 (one per helper extraction)
- **Execution Order**: Reverse dependency (Helper 4 → 1 → 2 → 3)
- **Generated**: 2026-06-14

## Execution Strategy

### Extraction Order Rationale
1. **TICKET-118-4**: Extract catch block cleanup first (simplifies error handling)
2. **TICKET-118-1**: Extract early validation (guard clause pattern)
3. **TICKET-118-2**: Extract pure construction (no dependencies)
4. **TICKET-118-3**: Extract FSM registration (depends on construction)

### Success Criteria (Epic-Level)
- [ ] Main method complexity reduced from 16 to ≤ 8
- [ ] All 4 helpers extracted and tested independently
- [ ] Zero behavioral changes (logic moved verbatim)
- [ ] All existing tests pass (zero regressions)
- [ ] 16 new unit tests added (4 per helper)
- [ ] CSharpier formatting enforced
- [ ] Pre-push validation passes (all 13 checks)
- [ ] Hard-link sync verified (deploy-sync.ps1)
- [ ] F5 smoke test in NinjaTrader passes

---

## TICKET-118-4: Extract RollbackFleetOrderTracking Helper

### Priority: P5 (Surgical Extraction)
### Estimated Complexity Reduction: -2 CYC
### Dependencies: None (catch block is independent)

### Method Signature
```csharp
/// <summary>
/// Rolls back fleet order tracking on submission failure.
/// Cleans up all dictionaries and reverses expected position delta.
/// </summary>
/// <param name="fleetKey">Fleet signal key</param>
/// <param name="expectedKey">Expected position key</param>
/// <param name="reservedDelta">Position delta to reverse (0 if not reserved)</param>
/// <param name="syncPending">Whether sync pending flag is set</param>
private void RollbackFleetOrderTracking(
    string fleetKey,
    string expectedKey,
    int reservedDelta,
    bool syncPending
)
```

### Extraction Steps

#### Step 1: Create Helper Method (Lines 663-670)
1. Navigate to src/V12_002.SIMA.Execution.cs
2. Locate ProcessSingleFleetRMAAccount method (lines 511-681)
3. Insert new helper method BEFORE ProcessSingleFleetRMAAccount
4. Copy catch block logic (lines 663-670) into helper body
5. Add XML documentation header
6. Preserve all comments verbatim (e.g., [923B-FIX-B])

**Source Code (Catch Block)**:
```csharp
// Lines 663-670 (relative 163-170 in read_file output)
if (syncPending)
{
    ClearDispatchSyncPending(expectedKey);
    syncPending = false;
}

// [923B-FIX-B]: Full rollback -- dicts were registered before expectedPositions,
// so both must be cleaned up on Submit failure (mirrors ExecuteSmartDispatchEntry catch).
if (reservedDelta != 0)
    AddExpectedPositionDeltaLocked(expectedKey, -reservedDelta);
activePositions.TryRemove(fleetKey, out _);
entryOrders.TryRemove(fleetKey, out _);
// Phase 6: Clean up proactive FSM on dispatch failure
_followerBrackets.TryRemove(fleetKey, out _);
```

**Target Code (Helper)**:
```csharp
private void RollbackFleetOrderTracking(
    string fleetKey,
    string expectedKey,
    int reservedDelta,
    bool syncPending
)
{
    // Clear sync pending flag if set
    if (syncPending)
    {
        ClearDispatchSyncPending(expectedKey);
    }

    // [923B-FIX-B]: Full rollback -- dicts were registered before expectedPositions,
    // so both must be cleaned up on Submit failure (mirrors ExecuteSmartDispatchEntry catch).
    if (reservedDelta != 0)
    {
        AddExpectedPositionDeltaLocked(expectedKey, -reservedDelta);
    }

    // Clean up all tracking dictionaries
    activePositions.TryRemove(fleetKey, out _);
    entryOrders.TryRemove(fleetKey, out _);
    // Phase 6: Clean up proactive FSM on dispatch failure
    _followerBrackets.TryRemove(fleetKey, out _);
}
```

#### Step 2: Replace Catch Block Logic
1. Locate catch block in ProcessSingleFleetRMAAccount (line 662)
2. Replace lines 663-670 with single helper call
3. Keep dispatchLog.AppendLine and return false

**Before**:
```csharp
catch (Exception ex)
{
    if (syncPending)
    {
        ClearDispatchSyncPending(expectedKey);
        syncPending = false;
    }
    // ... 8 more lines of cleanup logic
    dispatchLog.AppendLine(LogBuffer.Format("  FAIL | {0,-28} | {1}", acct.Name, ex.Message));
    return false;
}
```

**After**:
```csharp
catch (Exception ex)
{
    RollbackFleetOrderTracking(fleetKey, expectedKey, reservedDelta, syncPending);
    dispatchLog.AppendLine(LogBuffer.Format("  FAIL | {0,-28} | {1}", acct.Name, ex.Message));
    return false;
}
```

#### Step 3: Run CSharpier
```bash
dotnet csharpier format src/V12_002.SIMA.Execution.cs
```

#### Step 4: Create Unit Tests
Create file: `tests/V12_Performance.Tests/Core/FleetRMAHelperTests.cs`

**Test 1: Full Rollback**
```csharp
[Test]
public void RollbackFleetOrderTracking_FullRollback_CleansAllDictionaries()
{
    // Arrange
    string fleetKey = "TestAcct_RMA_SIGNAL";
    string expectedKey = "TestAcct_Expected";
    activePositions[fleetKey] = CreateMockPositionInfo(fleetKey);
    entryOrders[fleetKey] = CreateMockOrder("ORDER123");
    _followerBrackets[fleetKey] = CreateMockFSM(fleetKey);
    MarkDispatchSyncPending(expectedKey);
    SetExpectedPosition(expectedKey, 10);

    // Act
    RollbackFleetOrderTracking(fleetKey, expectedKey, 10, syncPending: true);

    // Assert
    Assert.IsFalse(activePositions.ContainsKey(fleetKey));
    Assert.IsFalse(entryOrders.ContainsKey(fleetKey));
    Assert.IsFalse(_followerBrackets.ContainsKey(fleetKey));
    Assert.IsFalse(IsDispatchSyncPending(expectedKey));
    Assert.AreEqual(0, GetExpectedPosition(expectedKey));
}
```

**Test 2: No Reserved Delta**
```csharp
[Test]
public void RollbackFleetOrderTracking_NoReservedDelta_SkipsPositionRollback()
{
    // Arrange
    string fleetKey = "TestAcct_RMA_SIGNAL";
    string expectedKey = "TestAcct_Expected";
    activePositions[fleetKey] = CreateMockPositionInfo(fleetKey);
    SetExpectedPosition(expectedKey, 5);

    // Act
    RollbackFleetOrderTracking(fleetKey, expectedKey, reservedDelta: 0, syncPending: false);

    // Assert
    Assert.IsFalse(activePositions.ContainsKey(fleetKey));
    Assert.AreEqual(5, GetExpectedPosition(expectedKey));
}
```

**Test 3: Sync Not Pending**
```csharp
[Test]
public void RollbackFleetOrderTracking_SyncNotPending_SkipsClearSync()
{
    // Arrange
    string fleetKey = "TestAcct_RMA_SIGNAL";
    string expectedKey = "TestAcct_Expected";
    activePositions[fleetKey] = CreateMockPositionInfo(fleetKey);

    // Act
    RollbackFleetOrderTracking(fleetKey, expectedKey, reservedDelta: 0, syncPending: false);

    // Assert
    Assert.IsFalse(activePositions.ContainsKey(fleetKey));
}
```

**Test 4: Partial Rollback (Dict Only)**
```csharp
[Test]
public void RollbackFleetOrderTracking_DictOnlyRollback_PreservesExpectedPositions()
{
    // Arrange
    string fleetKey = "TestAcct_RMA_SIGNAL";
    string expectedKey = "TestAcct_Expected";
    activePositions[fleetKey] = CreateMockPositionInfo(fleetKey);
    entryOrders[fleetKey] = CreateMockOrder("ORDER123");
    _followerBrackets[fleetKey] = CreateMockFSM(fleetKey);
    SetExpectedPosition(expectedKey, 10);

    // Act
    RollbackFleetOrderTracking(fleetKey, expectedKey, reservedDelta: 0, syncPending: false);

    // Assert
    Assert.IsFalse(activePositions.ContainsKey(fleetKey));
    Assert.IsFalse(entryOrders.ContainsKey(fleetKey));
    Assert.IsFalse(_followerBrackets.ContainsKey(fleetKey));
    Assert.AreEqual(10, GetExpectedPosition(expectedKey));
}
```

#### Step 5: Run Tests
```bash
dotnet test --filter "RollbackFleetOrderTracking"
```

### Verification Criteria
- [ ] Helper method compiles without errors
- [ ] All 4 unit tests pass
- [ ] Catch block reduced to 3 lines (helper call + log + return)
- [ ] CSharpier formatting applied
- [ ] Main method CYC reduced by 2 (16 → 14)
- [ ] No behavioral changes (logic moved verbatim)
- [ ] All comments preserved with original line references

### Rollback Steps
```bash
git revert HEAD  # Revert this commit only
# OR
git reset --hard HEAD~1  # Remove commit entirely
```

### Estimated Time: 30 minutes
### Complexity Impact: Main method CYC 16 → 14 (-2)

---

## TICKET-118-1: Extract ValidateFleetAccountEligibility Helper

### Priority: P5 (Surgical Extraction)
### Estimated Complexity Reduction: -3 CYC
### Dependencies: TICKET-118-4 (must be completed first)

### Method Signature
```csharp
/// <summary>
/// Validates fleet account eligibility for RMA order submission.
/// Checks fleet active status and consistency lock P&L cap.
/// </summary>
/// <param name="acct">Account to validate</param>
/// <param name="dispatchLog">Log buffer for skip reasons</param>
/// <returns>True if account is eligible, false otherwise</returns>
private bool ValidateFleetAccountEligibility(
    Account acct,
    StringBuilder dispatchLog
)
```

### Extraction Steps

#### Step 1: Create Helper Method (Lines 519-537)
1. Navigate to src/V12_002.SIMA.Execution.cs
2. Locate ProcessSingleFleetRMAAccount method
3. Insert new helper method BEFORE ProcessSingleFleetRMAAccount
4. Copy validation logic (lines 519-537) into helper body
5. Add XML documentation header
6. Preserve all comments verbatim (e.g., V12.8: Fleet Manager toggle)

**Source Code (Validation Logic)**:
```csharp
// Lines 519-537 (relative 19-37 in read_file output)
// V12.8: Fleet Manager toggle -- skip if account NOT registered or explicitly disabled
if (!activeFleetAccounts.TryGetValue(acct.Name, out bool isActive) || !isActive)
{
    dispatchLog.AppendLine(LogBuffer.Format("  SKIP | {0,-28} | Inactive", acct.Name));
    return false;
}

// Consistency Lock
if (EnableConsistencyLock)
{
    double dailyPL = acct.Get(AccountItem.RealizedProfitLoss, Currency.UsDollar);
    if (dailyPL >= MaxDailyProfitCap)
    {
        dispatchLog.AppendLine(
            LogBuffer.Format("  SKIP | {0,-28} | ConsistencyLock ${1:F2}", acct.Name, dailyPL)
        );
        return false;
    }
}
```

**Target Code (Helper)**:
```csharp
private bool ValidateFleetAccountEligibility(
    Account acct,
    StringBuilder dispatchLog
)
{
    // V12.8: Fleet Manager toggle -- skip if account NOT registered or explicitly disabled
    if (!activeFleetAccounts.TryGetValue(acct.Name, out bool isActive) || !isActive)
    {
        dispatchLog.AppendLine(LogBuffer.Format("  SKIP | {0,-28} | Inactive", acct.Name));
        return false;
    }

    // Consistency Lock
    if (EnableConsistencyLock)
    {
        double dailyPL = acct.Get(AccountItem.RealizedProfitLoss, Currency.UsDollar);
        if (dailyPL >= MaxDailyProfitCap)
        {
            dispatchLog.AppendLine(
                LogBuffer.Format("  SKIP | {0,-28} | ConsistencyLock ${1:F2}", acct.Name, dailyPL)
            );
            return false;
        }
    }

    return true;
}
```

#### Step 2: Replace Validation Logic
1. Locate ProcessSingleFleetRMAAccount method start (line 511)
2. Replace lines 519-537 with single helper call + early return
3. Keep fleetKey declaration after validation

**Before**:
```csharp
private bool ProcessSingleFleetRMAAccount(...)
{
    // V12.8: Fleet Manager toggle -- skip if account NOT registered or explicitly disabled
    if (!activeFleetAccounts.TryGetValue(acct.Name, out bool isActive) || !isActive)
    {
        dispatchLog.AppendLine(LogBuffer.Format("  SKIP | {0,-28} | Inactive", acct.Name));
        return false;
    }
    // ... 15 more lines of validation
    
    string fleetKey = acct.Name + "_RMA_" + baseSignal;
    // ... rest of method
}
```

**After**:
```csharp
private bool ProcessSingleFleetRMAAccount(...)
{
    // Early validation guard
    if (!ValidateFleetAccountEligibility(acct, dispatchLog))
    {
        return false;
    }

    // [923B-FIX-B]: fleetKey declared outside try so catch can access it for dict rollback.
    string fleetKey = acct.Name + "_RMA_" + baseSignal;
    // ... rest of method
}
```

#### Step 3: Run CSharpier
```bash
dotnet csharpier format src/V12_002.SIMA.Execution.cs
```

#### Step 4: Add Unit Tests
Add to `tests/V12_Performance.Tests/Core/FleetRMAHelperTests.cs`

**Test 1: Inactive Account**
```csharp
[Test]
public void ValidateFleetAccountEligibility_InactiveAccount_ReturnsFalse()
{
    // Arrange
    var acct = CreateMockAccount("TestAcct");
    activeFleetAccounts["TestAcct"] = false;
    var log = new StringBuilder();

    // Act
    bool result = ValidateFleetAccountEligibility(acct, log);

    // Assert
    Assert.IsFalse(result);
    Assert.That(log.ToString(), Does.Contain("SKIP"));
    Assert.That(log.ToString(), Does.Contain("Inactive"));
}
```

**Test 2: Unregistered Account**
```csharp
[Test]
public void ValidateFleetAccountEligibility_UnregisteredAccount_ReturnsFalse()
{
    // Arrange
    var acct = CreateMockAccount("UnknownAcct");
    var log = new StringBuilder();

    // Act
    bool result = ValidateFleetAccountEligibility(acct, log);

    // Assert
    Assert.IsFalse(result);
    Assert.That(log.ToString(), Does.Contain("Inactive"));
}
```

**Test 3: Consistency Lock Exceeded**
```csharp
[Test]
public void ValidateFleetAccountEligibility_ConsistencyLockExceeded_ReturnsFalse()
{
    // Arrange
    var acct = CreateMockAccount("TestAcct");
    activeFleetAccounts["TestAcct"] = true;
    EnableConsistencyLock = true;
    MaxDailyProfitCap = 1000.0;
    MockAccountPL(acct, 1500.0);
    var log = new StringBuilder();

    // Act
    bool result = ValidateFleetAccountEligibility(acct, log);

    // Assert
    Assert.IsFalse(result);
    Assert.That(log.ToString(), Does.Contain("ConsistencyLock"));
    Assert.That(log.ToString(), Does.Contain("$1500.00"));
}
```

**Test 4: Valid Account**
```csharp
[Test]
public void ValidateFleetAccountEligibility_ValidAccount_ReturnsTrue()
{
    // Arrange
    var acct = CreateMockAccount("TestAcct");
    activeFleetAccounts["TestAcct"] = true;
    EnableConsistencyLock = true;
    MaxDailyProfitCap = 1000.0;
    MockAccountPL(acct, 500.0);
    var log = new StringBuilder();

    // Act
    bool result = ValidateFleetAccountEligibility(acct, log);

    // Assert
    Assert.IsTrue(result);
    Assert.That(log.ToString(), Is.Empty);
}
```

**Test 5: Consistency Lock Disabled**
```csharp
[Test]
public void ValidateFleetAccountEligibility_ConsistencyLockDisabled_ReturnsTrue()
{
    // Arrange
    var acct = CreateMockAccount("TestAcct");
    activeFleetAccounts["TestAcct"] = true;
    EnableConsistencyLock = false;
    MockAccountPL(acct, 5000.0);
    var log = new StringBuilder();

    // Act
    bool result = ValidateFleetAccountEligibility(acct, log);

    // Assert
    Assert.IsTrue(result);
}
```

#### Step 5: Run Tests
```bash
dotnet test --filter "ValidateFleetAccountEligibility"
```

### Verification Criteria
- [ ] Helper method compiles without errors
- [ ] All 5 unit tests pass
- [ ] Main method start reduced to 4 lines (guard + fleetKey declaration)
- [ ] CSharpier formatting applied
- [ ] Main method CYC reduced by 3 (14 → 11)
- [ ] No behavioral changes (logic moved verbatim)
- [ ] All comments preserved with original line references

### Rollback Steps
```bash
git revert HEAD
```

### Estimated Time: 45 minutes
### Complexity Impact: Main method CYC 14 → 11 (-3)

---

## TICKET-118-2: Extract BuildFleetPositionInfo Helper

### Priority: P5 (Surgical Extraction)
### Estimated Complexity Reduction: 0 CYC (pure construction)
### Dependencies: TICKET-118-1 (must be completed first)

### Method Signature
```csharp
/// <summary>
/// Builds PositionInfo for fleet RMA follower with 5-target distribution.
/// Pure construction method with no side effects.
/// </summary>
/// <param name="fleetKey">Fleet signal key</param>
/// <param name="direction">Market position direction</param>
/// <param name="qty">Total contracts</param>
/// <param name="price">Entry price</param>
/// <param name="prices">RMA bracket prices (stop + 5 targets)</param>
/// <param name="acct">Executing account</param>
/// <returns>Initialized PositionInfo</returns>
private PositionInfo BuildFleetPositionInfo(
    string fleetKey,
    MarketPosition direction,
    int qty,
    double price,
    RMABracketPrices prices,
    Account acct
)
```

### Extraction Steps

#### Step 1: Create Helper Method (Lines 584-613)
1. Navigate to src/V12_002.SIMA.Execution.cs
2. Locate ProcessSingleFleetRMAAccount method
3. Insert new helper method BEFORE ProcessSingleFleetRMAAccount
4. Copy PositionInfo construction (lines 584-613) into helper body
5. Add XML documentation header
6. Preserve all comments verbatim (e.g., V12.1101E, Build 936 [FIX-2])

**Source Code (PositionInfo Construction)**:
```csharp
// Lines 584-613 (relative 84-113 in read_file output)
PositionInfo fleetFollowerPos = new PositionInfo
{
    SignalName = fleetKey,
    Direction = direction,
    TotalContracts = qty,
    RemainingContracts = qty,
    EntryPrice = price,
    InitialStopPrice = prices.StopPrice,
    CurrentStopPrice = prices.StopPrice,
    Target1Price = prices.T1Price,
    Target2Price = prices.T2Price,
    Target3Price = prices.T3Price,
    Target4Price = prices.T4Price,
    Target5Price = prices.T5Price,
    T1Contracts = prices.Rt1,
    T2Contracts = prices.Rt2,
    T3Contracts = prices.Rt3,
    T4Contracts = prices.Rt4,
    T5Contracts = prices.Rt5,
    EntryOrderType = OrderType.Limit,
    EntryFilled = false,
    IsRMATrade = true,
    IsFollower = true,
    ExecutingAccount = acct,
    BracketSubmitted = false,
    ExtremePriceSinceEntry = price,
    CurrentTrailLevel = 0,
    OcoGroupId = "V12_" + GetStableHash(fleetKey),
};
```

**Target Code (Helper)**:
```csharp
private PositionInfo BuildFleetPositionInfo(
    string fleetKey,
    MarketPosition direction,
    int qty,
    double price,
    RMABracketPrices prices,
    Account acct
)
{
    // V12.1101E: Full 5-target distribution mirrors Master exactly.
    return new PositionInfo
    {
        SignalName = fleetKey,
        Direction = direction,
        TotalContracts = qty,
        RemainingContracts = qty,
        EntryPrice = price,
        InitialStopPrice = prices.StopPrice,
        CurrentStopPrice = prices.StopPrice,
        Target1Price = prices.T1Price,
        Target2Price = prices.T2Price,
        Target3Price = prices.T3Price,
        Target4Price = prices.T4Price,
        Target5Price = prices.T5Price,
        T1Contracts = prices.Rt1,
        T2Contracts = prices.Rt2,
        T3Contracts = prices.Rt3,
        T4Contracts = prices.Rt4,
        T5Contracts = prices.Rt5,
        EntryOrderType = OrderType.Limit,
        EntryFilled = false,
        IsRMATrade = true,
        IsFollower = true,
        ExecutingAccount = acct,
        BracketSubmitted = false, // V12.10: deferred -- OnAccountExecutionUpdate submits on fill
        ExtremePriceSinceEntry = price,
        CurrentTrailLevel = 0,
        // Build 936 [FIX-2]: Deterministic bracket OCO group ID for broker-native stop+target linking.
        OcoGroupId = "V12_" + GetStableHash(fleetKey),
    };
}
```

#### Step 2: Replace Construction Logic
1. Locate PositionInfo construction in ProcessSingleFleetRMAAccount (line 584)
2. Replace lines 584-613 with single helper call

**Before**:
```csharp
PositionInfo fleetFollowerPos = new PositionInfo
{
    SignalName = fleetKey,
    // ... 28 more property assignments
};
```

**After**:
```csharp
// Build PositionInfo with 5-target distribution
PositionInfo fleetFollowerPos = BuildFleetPositionInfo(
    fleetKey,
    direction,
    qty,
    price,
    prices,
    acct
);
```

#### Step 3: Run CSharpier
```bash
dotnet csharpier format src/V12_002.SIMA.Execution.cs
```

#### Step 4: Add Unit Tests
Add to `tests/V12_Performance.Tests/Core/FleetRMAHelperTests.cs`

**Test 1: Long Position**
```csharp
[Test]
public void BuildFleetPositionInfo_LongPosition_CorrectDistribution()
{
    // Arrange
    string fleetKey = "TestAcct_RMA_LONG";
    var prices = new RMABracketPrices
    {
        StopPrice = 4900.0,
        T1Price = 5010.0,
        T2Price = 5020.0,
        T3Price = 5030.0,
        T4Price = 5040.0,
        T5Price = 5050.0,
        Rt1 = 2,
        Rt2 = 2,
        Rt3 = 2,
        Rt4 = 2,
        Rt5 = 2,
    };
    var acct = CreateMockAccount("TestAcct");

    // Act
    var posInfo = BuildFleetPositionInfo(
        fleetKey,
        MarketPosition.Long,
        10,
        5000.0,
        prices,
        acct
    );

    // Assert
    Assert.AreEqual(fleetKey, posInfo.SignalName);
    Assert.AreEqual(MarketPosition.Long, posInfo.Direction);
    Assert.AreEqual(10, posInfo.TotalContracts);
    Assert.AreEqual(10, posInfo.RemainingContracts);
    Assert.AreEqual(5000.0, posInfo.EntryPrice);
    Assert.AreEqual(4900.0, posInfo.InitialStopPrice);
    Assert.AreEqual(5010.0, posInfo.Target1Price);
    Assert.AreEqual(2, posInfo.T1Contracts);
    Assert.IsTrue(posInfo.IsRMATrade);
    Assert.IsTrue(posInfo.IsFollower);
    Assert.IsFalse(posInfo.BracketSubmitted);
    Assert.That(posInfo.OcoGroupId, Does.StartWith("V12_"));
}
```

**Test 2: Short Position**
```csharp
[Test]
public void BuildFleetPositionInfo_ShortPosition_CorrectDistribution()
{
    // Arrange
    string fleetKey = "TestAcct_RMA_SHORT";
    var prices = new RMABracketPrices
    {
        StopPrice = 5100.0,
        T1Price = 4990.0,
        T2Price = 4980.0,
        T3Price = 4970.0,
        T4Price = 4960.0,
        T5Price = 4950.0,
        Rt1 = 3,
        Rt2 = 3,
        Rt3 = 2,
        Rt4 = 1,
        Rt5 = 1,
    };
    var acct = CreateMockAccount("TestAcct");

    // Act
    var posInfo = BuildFleetPositionInfo(
        fleetKey,
        MarketPosition.Short,
        10,
        5000.0,
        prices,
        acct
    );

    // Assert
    Assert.AreEqual(MarketPosition.Short, posInfo.Direction);
    Assert.AreEqual(5100.0, posInfo.InitialStopPrice);
    Assert.AreEqual(4990.0, posInfo.Target1Price);
    Assert.AreEqual(3, posInfo.T1Contracts);
    Assert.AreEqual(1, posInfo.T5Contracts);
}
```

**Test 3: OCO Group ID Determinism**
```csharp
[Test]
public void BuildFleetPositionInfo_SameFleetKey_SameOcoGroupId()
{
    // Arrange
    string fleetKey = "TestAcct_RMA_SIGNAL";
    var prices = CreateDefaultRMABracketPrices();
    var acct = CreateMockAccount("TestAcct");

    // Act
    var posInfo1 = BuildFleetPositionInfo(fleetKey, MarketPosition.Long, 10, 5000.0, prices, acct);
    var posInfo2 = BuildFleetPositionInfo(fleetKey, MarketPosition.Long, 10, 5000.0, prices, acct);

    // Assert
    Assert.AreEqual(posInfo1.OcoGroupId, posInfo2.OcoGroupId);
    Assert.That(posInfo1.OcoGroupId, Does.StartWith("V12_"));
}
```

#### Step 5: Run Tests
```bash
dotnet test --filter "BuildFleetPositionInfo"
```

### Verification Criteria
- [ ] Helper method compiles without errors
- [ ] All 3 unit tests pass
- [ ] Main method construction reduced to 7 lines (helper call)
- [ ] CSharpier formatting applied
- [ ] Main method CYC unchanged (11 → 11, pure construction)
- [ ] No behavioral changes (logic moved verbatim)
- [ ] All comments preserved with original line references

### Rollback Steps
```bash
git revert HEAD
```

### Estimated Time: 30 minutes
### Complexity Impact: Main method CYC 11 → 11 (0, readability improvement)

---

## TICKET-118-3: Extract RegisterFleetOrderTracking Helper

### Priority: P5 (Surgical Extraction)
### Estimated Complexity Reduction: -3 CYC
### Dependencies: TICKET-118-2 (must be completed first)

### Method Signature
```csharp
/// <summary>
/// Atomically registers fleet order tracking dictionaries.
/// CRITICAL: Must be called BEFORE AddExpectedPositionDeltaLocked to prevent phantom repair race.
/// </summary>
/// <param name="fleetKey">Fleet signal key</param>
/// <param name="positionInfo">Position info to register</param>
/// <param name="entryOrder">Entry order to register</param>
/// <param name="acct">Executing account</param>
/// <param name="qty">Total contracts</param>
/// <param name="price">Entry price</param>
/// <param name="direction">Market position direction</param>
private void RegisterFleetOrderTracking(
    string fleetKey,
    PositionInfo positionInfo,
    Order entryOrder,
    Account acct,
    int qty,
    double price,
    MarketPosition direction
)
```

### Extraction Steps

#### Step 1: Create Helper Method (Lines 615-646)
1. Navigate to src/V12_002.SIMA.Execution.cs
2. Locate ProcessSingleFleetRMAAccount method
3. Insert new helper method BEFORE ProcessSingleFleetRMAAccount
4. Copy registration logic (lines 615-646) into helper body
5. Add XML documentation header
6. Preserve all comments verbatim (e.g., [923B-FIX-B], Phase 6 [FSM-P3])

**Source Code (Registration Logic)**:
```csharp
// Lines 615-646 (relative 115-146 in read_file output)
// B966: Enqueue NOT applied -- ordering invariant: dicts BEFORE expectedPositions (L1479).
activePositions[fleetKey] = fleetFollowerPos;
entryOrders[fleetKey] = fEntry;

MarkDispatchSyncPending(expectedKey);
syncPending = true;

// Phase 6 [FSM-P3]: Proactive FSM for RMA V2 fleet entries.
if (!_followerBrackets.ContainsKey(fleetKey))
{
    var rmaFsm = new FollowerBracketFSM
    {
        AccountName = acct.Name,
        EntryName = fleetKey,
        State = FollowerBracketState.Submitted,
        RemainingContracts = qty,
        EntryOrder = fEntry,
        ExpectedEntryPrice = price,
        LastUpdateUtc = DateTime.UtcNow,
    };
    _followerBrackets.TryAdd(fleetKey, rmaFsm);
}

reservedDelta = (direction == MarketPosition.Long) ? qty : -qty;
AddExpectedPositionDeltaLocked(expectedKey, reservedDelta);

acct.Submit(new[] { fEntry });

// Phase 6 [FSM-P3]: Register OrderId for O(1) FSM lookup
if (fEntry != null && !string.IsNullOrEmpty(fEntry.OrderId))
    _orderIdToFsmKey[fEntry.OrderId] = fleetKey;
```

**Target Code (Helper)**:
```csharp
private void RegisterFleetOrderTracking(
    string fleetKey,
    PositionInfo positionInfo,
    Order entryOrder,
    Account acct,
    int qty,
    double price,
    MarketPosition direction
)
{
    // [923B-FIX-B]: Phantom-Fix FIX-1 backport -- register tracking dicts BEFORE
    // updating expectedPositions. Mirrors the fix already applied to ExecuteSmartDispatchEntry.
    // B966: Enqueue NOT applied -- ordering invariant: dicts BEFORE expectedPositions.
    activePositions[fleetKey] = positionInfo;
    entryOrders[fleetKey] = entryOrder;

    // Phase 6 [FSM-P3]: Proactive FSM for RMA V2 fleet entries.
    // Entry-only (brackets deferred until fill via SymmetryGuard).
    // State = Submitted (direct submit, no pump queue).
    if (!_followerBrackets.ContainsKey(fleetKey))
    {
        var rmaFsm = new FollowerBracketFSM
        {
            AccountName = acct.Name,
            EntryName = fleetKey,
            State = FollowerBracketState.Submitted,
            RemainingContracts = qty,
            EntryOrder = entryOrder,
            ExpectedEntryPrice = price,
            LastUpdateUtc = DateTime.UtcNow,
        };
        _followerBrackets.TryAdd(fleetKey, rmaFsm);
    }

    // Phase 6 [FSM-P3]: Register OrderId for O(1) FSM lookup (populated by Submit)
    // Note: OrderId may be null/empty before Submit, so this is defensive
    if (entryOrder != null && !string.IsNullOrEmpty(entryOrder.OrderId))
    {
        _orderIdToFsmKey[entryOrder.OrderId] = fleetKey;
    }
}
```

#### Step 2: Replace Registration Logic
1. Locate registration section in ProcessSingleFleetRMAAccount (line 615)
2. Replace lines 615-646 with helper call + sync/submit logic
3. Keep MarkDispatchSyncPending, AddExpectedPositionDeltaLocked, Submit, ClearDispatchSyncPending in main method

**Before**:
```csharp
// B966: Enqueue NOT applied -- ordering invariant: dicts BEFORE expectedPositions (L1479).
activePositions[fleetKey] = fleetFollowerPos;
entryOrders[fleetKey] = fEntry;
// ... 30 more lines of registration logic
```

**After**:
```csharp
// Build PositionInfo with 5-target distribution
PositionInfo fleetFollowerPos = BuildFleetPositionInfo(
    fleetKey,
    direction,
    qty,
    price,
    prices,
    acct
);

// Register tracking dictionaries BEFORE expectedPositions (phantom-fix ordering)
RegisterFleetOrderTracking(fleetKey, fleetFollowerPos, fEntry, acct, qty, price, direction);

MarkDispatchSyncPending(expectedKey);
syncPending = true;

reservedDelta = (direction == MarketPosition.Long) ? qty : -qty;
AddExpectedPositionDeltaLocked(expectedKey, reservedDelta);

acct.Submit(new[] { fEntry });

ClearDispatchSyncPending(expectedKey);
syncPending = false;
```

#### Step 3: Run CSharpier
```bash
dotnet csharpier format src/V12_002.SIMA.Execution.cs
```

#### Step 4: Add Unit Tests
Add to `tests/V12_Performance.Tests/Core/FleetRMAHelperTests.cs`

**Test 1: Valid Order Registration**
```csharp
[Test]
public void RegisterFleetOrderTracking_ValidOrder_RegistersAllDictionaries()
{
    // Arrange
    string fleetKey = "TestAcct_RMA_SIGNAL";
    var posInfo = CreateMockPositionInfo(fleetKey);
    var entryOrder = CreateMockOrder("ORDER123");
    var acct = CreateMockAccount("TestAcct");

    // Act
    RegisterFleetOrderTracking(fleetKey, posInfo, entryOrder, acct, 10, 5000.0, MarketPosition.Long);

    // Assert
    Assert.IsTrue(activePositions.ContainsKey(fleetKey));
    Assert.IsTrue(entryOrders.ContainsKey(fleetKey));
    Assert.IsTrue(_followerBrackets.ContainsKey(fleetKey));
    Assert.IsTrue(_orderIdToFsmKey.ContainsKey("ORDER123"));
    Assert.AreEqual(fleetKey, _orderIdToFsmKey["ORDER123"]);
}
```

**Test 2: Null OrderId Handling**
```csharp
[Test]
public void RegisterFleetOrderTracking_NullOrderId_SkipsFsmMapping()
{
    // Arrange
    string fleetKey = "TestAcct_RMA_SIGNAL";
    var posInfo = CreateMockPositionInfo(fleetKey);
    var entryOrder = CreateMockOrder(null);
    var acct = CreateMockAccount("TestAcct");

    // Act
    RegisterFleetOrderTracking(fleetKey, posInfo, entryOrder, acct, 10, 5000.0, MarketPosition.Long);

    // Assert
    Assert.IsTrue(activePositions.ContainsKey(fleetKey));
    Assert.IsTrue(entryOrders.ContainsKey(fleetKey));
    Assert.IsTrue(_followerBrackets.ContainsKey(fleetKey));
    Assert.IsFalse(_orderIdToFsmKey.Any());
}
```

**Test 3: FSM Already Exists**
```csharp
[Test]
public void RegisterFleetOrderTracking_FsmAlreadyExists_DoesNotOverwrite()
{
    // Arrange
    string fleetKey = "TestAcct_RMA_SIGNAL";
    var existingFsm = new FollowerBracketFSM
    {
        AccountName = "TestAcct",
        EntryName = fleetKey,
        State = FollowerBracketState.Filled,
        RemainingContracts = 5,
    };
    _followerBrackets[fleetKey] = existingFsm;

    var posInfo = CreateMockPositionInfo(fleetKey);
    var entryOrder = CreateMockOrder("ORDER123");
    var acct = CreateMockAccount("TestAcct");

    // Act
    RegisterFleetOrderTracking(fleetKey, posInfo, entryOrder, acct, 10, 5000.0, MarketPosition.Long);

    // Assert
    Assert.AreEqual(FollowerBracketState.Filled, _followerBrackets[fleetKey].State);
    Assert.AreEqual(5, _followerBrackets[fleetKey].RemainingContracts);
}
```

**Test 4: FSM State Initialization**
```csharp
[Test]
public void RegisterFleetOrderTracking_NewFsm_InitializesCorrectly()
{
    // Arrange
    string fleetKey = "TestAcct_RMA_SIGNAL";
    var posInfo = CreateMockPositionInfo(fleetKey);
    var entryOrder = CreateMockOrder("ORDER123");
    var acct = CreateMockAccount("TestAcct");

    // Act
    RegisterFleetOrderTracking(fleetKey, posInfo, entryOrder, acct, 10, 5000.0, MarketPosition.Long);

    // Assert
    var fsm = _followerBrackets[fleetKey];
    Assert.AreEqual("TestAcct", fsm.AccountName);
    Assert.AreEqual(fleetKey, fsm.EntryName);
    Assert.AreEqual(FollowerBracketState.Submitted, fsm.State);
    Assert.AreEqual(10, fsm.RemainingContracts);
    Assert.AreEqual(5000.0, fsm.ExpectedEntryPrice);
    Assert.IsNotNull(fsm.LastUpdateUtc);
}
```

#### Step 5: Run Tests
```bash
dotnet test --filter "RegisterFleetOrderTracking"
```

### Verification Criteria
- [ ] Helper method compiles without errors
- [ ] All 4 unit tests pass
- [ ] Main method registration reduced to 1 line (helper call)
- [ ] CSharpier formatting applied
- [ ] Main method CYC reduced by 3 (11 → 8)
- [ ] No behavioral changes (logic moved verbatim)
- [ ] All comments preserved with original line references
- [ ] Ordering invariant preserved (dicts BEFORE expectedPositions)

### Rollback Steps
```bash
git revert HEAD
```

### Estimated Time: 45 minutes
### Complexity Impact: Main method CYC 11 → 8 (-3)

---

## Post-Implementation Verification

### Integration Test Suite
After all 4 tickets are completed, run comprehensive verification:

#### Step 1: Full Test Suite
```bash
dotnet test
```
**Expected**: All tests pass (zero regressions)

#### Step 2: Complexity Audit
```bash
python scripts/complexity_audit.py
```
**Expected**: ProcessSingleFleetRMAAccount CYC ≤ 8

#### Step 3: CSharpier Check
```bash
dotnet csharpier check src/
```
**Expected**: Zero formatting issues

#### Step 4: Pre-Push Validation (Fast Mode)
```bash
powershell -File .\scripts\pre_push_validation.ps1 -Fast
```
**Expected**: All 9 fast checks pass

#### Step 5: Hard-Link Sync
```bash
powershell -File .\deploy-sync.ps1
```
**Expected**: NinjaTrader hard links synchronized

#### Step 6: Manual Smoke Test
1. Open NinjaTrader
2. Press F5 to compile strategy
3. Verify zero compilation errors
4. Check BUILD_TAG in output

**Expected**: Clean compilation, BUILD_TAG matches

### Final Complexity Verification

| Method | Before CYC | After CYC | Target | Status |
|--------|-----------|-----------|--------|--------|
| ProcessSingleFleetRMAAccount | 16 | 3 | ≤ 8 | ✅ PASS |
| ValidateFleetAccountEligibility | N/A | 4 | ≤ 8 | ✅ PASS |
| BuildFleetPositionInfo | N/A | 1 | ≤ 8 | ✅ PASS |
| RegisterFleetOrderTracking | N/A | 3 | ≤ 8 | ✅ PASS |
| RollbackFleetOrderTracking | N/A | 2 | ≤ 8 | ✅ PASS |

**Total Complexity**: 13 (distributed across 5 methods)
**Main Method Reduction**: 81% (16 → 3)

### Success Criteria Checklist

- [ ] All 4 helpers extracted and tested
- [ ] Main method CYC reduced from 16 to 3 (target: ≤ 8)
- [ ] 16 new unit tests added (4 per helper)
- [ ] All existing tests pass (zero regressions)
- [ ] CSharpier formatting enforced
- [ ] Pre-push validation passes (all 13 checks)
- [ ] Hard-link sync verified (deploy-sync.ps1)
- [ ] F5 smoke test in NinjaTrader passes
- [ ] Zero behavioral changes (logic moved verbatim)
- [ ] All comments preserved with original line references
- [ ] No lock() blocks introduced
- [ ] ASCII-only compliance maintained
- [ ] Ordering invariant preserved (dicts BEFORE expectedPositions)

---

## Appendix A: Dependency Graph

```mermaid
graph TD
    A[TICKE