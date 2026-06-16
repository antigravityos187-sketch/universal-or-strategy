# Ticket 4: Extract CreateFollowerTargetReplaceSpec

**Epic**: EPIC-CCN-128  
**File**: `src/V12_002.Symmetry.Replace.cs`  
**Method**: `SymmetryGuardReplaceExistingFollowerTarget`  
**Priority**: P4 (Depends on Ticket 1, 2, 3)  
**Estimated Time**: 60 minutes

---

## Objective

Extract FSM replace spec creation logic into a dedicated helper method to preserve the two-phase replace pattern and improve testability.

---

## Current State

**Lines**: 69-89 (embedded in main method)  
**Logic**: Builds FSM replace spec and initiates two-phase cancel

```csharp
double newPrice = GetTargetPrice(pos, targetNumber);
if (newPrice <= 0)
    return;

OrderAction exitAction =
    pos.Direction == MarketPosition.Long ? OrderAction.Sell : OrderAction.BuyToCover;
string signalName = SymmetryTrim(targetTag + "_" + fleetEntryName, 40);

var tSpec = new FollowerTargetReplaceSpec
{
    EntryName = fleetEntryName,
    TargetNum = targetNumber,
    NewTargetPrice = Instrument.MasterInstrument.RoundToTickSize(newPrice),
    Quantity = qty,
    ExitAction = exitAction,
    TargetAccount = pos.ExecutingAccount,
    CancellingOrderId = oldTarget.OrderId,
};
_followerTargetReplaceSpecs[signalName] = tSpec;
StampReaperMoveGrace();
pos.ExecutingAccount.Cancel(new[] { oldTarget });
```

---

## Target State

**New Method Signature**:
```csharp
private void CreateFollowerTargetReplaceSpec(
    string fleetEntryName,
    PositionInfo pos,
    int targetNumber,
    int qty,
    Order oldTarget,
    string targetTag
)
```

**Implementation**:
```csharp
private void CreateFollowerTargetReplaceSpec(
    string fleetEntryName,
    PositionInfo pos,
    int targetNumber,
    int qty,
    Order oldTarget,
    string targetTag
)
{
    double newPrice = GetTargetPrice(pos, targetNumber);
    if (newPrice <= 0)
        return;

    OrderAction exitAction =
        pos.Direction == MarketPosition.Long ? OrderAction.Sell : OrderAction.BuyToCover;
    string signalName = SymmetryTrim(targetTag + "_" + fleetEntryName, 40);

    var tSpec = new FollowerTargetReplaceSpec
    {
        EntryName = fleetEntryName,
        TargetNum = targetNumber,
        NewTargetPrice = Instrument.MasterInstrument.RoundToTickSize(newPrice),
        Quantity = qty,
        ExitAction = exitAction,
        TargetAccount = pos.ExecutingAccount,
        CancellingOrderId = oldTarget.OrderId,
    };
    _followerTargetReplaceSpecs[signalName] = tSpec;
    StampReaperMoveGrace();
    pos.ExecutingAccount.Cancel(new[] { oldTarget });
}
```

**Complexity**: CYC 2 (1 if + 1 ternary)  
**Lines**: 25

---

## Caller Update

**Before**:
```csharp
if (!IsTargetOrderReplaceable(fleetEntryName, dict, out Order oldTarget))
    return;

double newPrice = GetTargetPrice(pos, targetNumber);
if (newPrice <= 0)
    return;

OrderAction exitAction =
    pos.Direction == MarketPosition.Long ? OrderAction.Sell : OrderAction.BuyToCover;
string signalName = SymmetryTrim(targetTag + "_" + fleetEntryName, 40);

var tSpec = new FollowerTargetReplaceSpec
{
    EntryName = fleetEntryName,
    TargetNum = targetNumber,
    NewTargetPrice = Instrument.MasterInstrument.RoundToTickSize(newPrice),
    Quantity = qty,
    ExitAction = exitAction,
    TargetAccount = pos.ExecutingAccount,
    CancellingOrderId = oldTarget.OrderId,
};
_followerTargetReplaceSpecs[signalName] = tSpec;
StampReaperMoveGrace();
pos.ExecutingAccount.Cancel(new[] { oldTarget });
```

**After**:
```csharp
if (!IsTargetOrderReplaceable(fleetEntryName, dict, out Order oldTarget))
    return;

CreateFollowerTargetReplaceSpec(fleetEntryName, pos, targetNumber, qty, oldTarget, targetTag);
```

---

## Unit Tests

**Test File**: `tests/V12_Performance.Tests/Symmetry/CreateFollowerTargetReplaceSpecTests.cs`

```csharp
[Fact]
public void CreateFollowerTargetReplaceSpec_ValidInputs_CreatesSpecAndCancels()
{
    // Arrange
    var pos = CreateMockPosition(MarketPosition.Long);
    var oldTarget = CreateMockOrder(OrderState.Working);
    string fleetEntryName = "TEST_ENTRY";
    int targetNumber = 1;
    int qty = 100;
    string targetTag = "T1";
    
    // Mock GetTargetPrice to return valid price
    // Mock StampReaperMoveGrace
    // Mock pos.ExecutingAccount.Cancel
    
    // Act
    _sut.CreateFollowerTargetReplaceSpec(
        fleetEntryName, pos, targetNumber, qty, oldTarget, targetTag
    );
    
    // Assert
    // Verify _followerTargetReplaceSpecs contains entry
    // Verify StampReaperMoveGrace was called
    // Verify Cancel was called with oldTarget
}

[Fact]
public void CreateFollowerTargetReplaceSpec_InvalidPrice_ReturnsEarly()
{
    // Arrange
    var pos = CreateMockPosition(MarketPosition.Long);
    var oldTarget = CreateMockOrder(OrderState.Working);
    
    // Mock GetTargetPrice to return 0
    
    // Act
    _sut.CreateFollowerTargetReplaceSpec(
        "TEST_ENTRY", pos, 1, 100, oldTarget, "T1"
    );
    
    // Assert
    // Verify _followerTargetReplaceSpecs is empty
    // Verify StampReaperMoveGrace was NOT called
    // Verify Cancel was NOT called
}

[Fact]
public void CreateFollowerTargetReplaceSpec_NegativePrice_ReturnsEarly()
{
    // Test negative price (should return early)
}

[Fact]
public void CreateFollowerTargetReplaceSpec_LongPosition_UsesSellAction()
{
    // Arrange
    var pos = CreateMockPosition(MarketPosition.Long);
    var oldTarget = CreateMockOrder(OrderState.Working);
    
    // Act
    _sut.CreateFollowerTargetReplaceSpec(
        "TEST_ENTRY", pos, 1, 100, oldTarget, "T1"
    );
    
    // Assert
    // Verify spec.ExitAction == OrderAction.Sell
}

[Fact]
public void CreateFollowerTargetReplaceSpec_ShortPosition_UsesBuyToCoverAction()
{
    // Arrange
    var pos = CreateMockPosition(MarketPosition.Short);
    var oldTarget = CreateMockOrder(OrderState.Working);
    
    // Act
    _sut.CreateFollowerTargetReplaceSpec(
        "TEST_ENTRY", pos, 1, 100, oldTarget, "T1"
    );
    
    // Assert
    // Verify spec.ExitAction == OrderAction.BuyToCover
}

[Fact]
public void CreateFollowerTargetReplaceSpec_ValidInputs_RoundsPrice()
{
    // Arrange
    var pos = CreateMockPosition(MarketPosition.Long);
    var oldTarget = CreateMockOrder(OrderState.Working);
    
    // Mock GetTargetPrice to return 50.123456
    
    // Act
    _sut.CreateFollowerTargetReplaceSpec(
        "TEST_ENTRY", pos, 1, 100, oldTarget, "T1"
    );
    
    // Assert
    // Verify spec.NewTargetPrice is rounded to tick size
}

[Fact]
public void CreateFollowerTargetReplaceSpec_ValidInputs_TrimsSignalName()
{
    // Arrange
    var pos = CreateMockPosition(MarketPosition.Long);
    var oldTarget = CreateMockOrder(OrderState.Working);
    string longFleetName = "VERY_LONG_FLEET_ENTRY_NAME_THAT_EXCEEDS_40_CHARS";
    
    // Act
    _sut.CreateFollowerTargetReplaceSpec(
        longFleetName, pos, 1, 100, oldTarget, "T1"
    );
    
    // Assert
    // Verify signal name is trimmed to 40 chars
}
```

---

## FSM Two-Phase Pattern Verification

**Phase 1** (This Method):
1. ✅ Build `FollowerTargetReplaceSpec` with new target price
2. ✅ Store spec in `_followerTargetReplaceSpecs` dictionary
3. ✅ Stamp REAPER grace window (`StampReaperMoveGrace`)
4. ✅ Cancel old target order

**Phase 2** (Automatic - `AccountOrders.cs` lines 352-382):
1. Detect cancel confirmation via `CancellingOrderId`
2. Fire `TriggerCustomEvent`
3. Call `SubmitFollowerTargetReplacement()` in `Propagation.cs`
4. Submit new target order with updated price

**Critical**: This extraction MUST preserve the exact FSM pattern. Any deviation breaks the two-phase replace logic.

---

## V12 DNA Compliance

- ✅ **Lock-Free**: No locks introduced (uses existing FSM dictionary)
- ✅ **ASCII-Only**: No Unicode characters
- ✅ **CYC ≤ 8**: CYC 2 (target: 8)
- ✅ **Correctness by Construction**: FSM pattern prevents illegal states
- ✅ **Single Responsibility**: FSM spec creation only

---

## Verification Steps

1. **Extract Method**:
   - Add `CreateFollowerTargetReplaceSpec` method to `V12_002.Symmetry.Replace.cs`
   - Update caller to use new method
   - Preserve exact logic (zero drift)

2. **Build**:
   ```powershell
   dotnet build
   ```

3. **Deploy**:
   ```powershell
   powershell -File .\deploy-sync.ps1
   ```

4. **Unit Tests**:
   ```powershell
   dotnet test --filter "FullyQualifiedName~CreateFollowerTargetReplaceSpecTests"
   ```

5. **Integration Test**:
   - F5 in NinjaTrader IDE
   - Verify BUILD_TAG appears in output
   - Verify no compilation errors
   - **CRITICAL**: Test symmetry replace logic in live market replay

6. **Complexity Audit**:
   ```powershell
   python scripts/complexity_audit.py --file src/V12_002.Symmetry.Replace.cs
   ```

7. **FSM Pattern Verification**:
   - Verify `_followerTargetReplaceSpecs` dictionary is populated
   - Verify `StampReaperMoveGrace` is called
   - Verify old target is cancelled
   - Verify Phase 2 triggers correctly (new target submitted)

---

## Success Criteria

- ✅ Method extracted with CYC 2
- ✅ All 7 unit tests pass
- ✅ `deploy-sync.ps1` executes successfully
- ✅ BUILD_TAG verified in NinjaTrader output
- ✅ No compilation errors
- ✅ Zero logic drift (behavior unchanged)
- ✅ FSM two-phase pattern preserved (verified in integration test)

---

## Rollback Plan

If verification fails:
```powershell
git checkout HEAD -- src/V12_002.Symmetry.Replace.cs
powershell -File .\deploy-sync.ps1
```

---

**Status**: PENDING  
**Dependencies**: Ticket 1, 2, 3  
**Blocks**: Ticket 5
