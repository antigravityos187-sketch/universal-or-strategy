# Extraction Tickets: EPIC-CCN-035

## Overview
- **Epic**: EPIC-CCN-035 - Extract SyncLimitTarget complexity reduction
- **Target Method**: SyncLimitTarget (src/V12_002.Orders.Management.StopSync.cs)
- **Current Complexity**: 17 → **Target**: ≤8 per method
- **Total Tickets**: 3
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3)
- **Estimated Effort**: 6-8 hours (2-3 hours per ticket including tests)

---

## TICKET-1: Extract UpdateTargetPrice Helper

### Scope
- **Current Method**: `SyncLimitTarget`
- **Current CYC**: 17
- **Target CYC After Extraction**: ≤15 (removes duplicated switch statement)
- **Helper Method CYC**: ≤2
- **Extraction**: Eliminate duplicated switch statement (lines 218-233 and 289-304)

### Implementation

#### Step 1: Create UpdateTargetPrice Method
Add new private method after SyncLimitTarget:

```csharp
/// <summary>
/// Updates the target price in PositionInfo based on target number.
/// Extracted from SyncLimitTarget to eliminate code duplication.
/// </summary>
/// <param name="pos">Position information to update</param>
/// <param name="targetNum">Target number (1-5)</param>
/// <param name="newPrice">New target price</param>
private void UpdateTargetPrice(PositionInfo pos, int targetNum, double newPrice)
{
    switch (targetNum)
    {
        case 1: pos.Target1Price = newPrice; break;
        case 2: pos.Target2Price = newPrice; break;
        case 3: pos.Target3Price = newPrice; break;
        case 4: pos.Target4Price = newPrice; break;
        case 5: pos.Target5Price = newPrice; break;
    }
}
```

#### Step 2: Replace First Occurrence (Lines 218-233)
Replace the switch statement in the repricing path:

**Before**:
```csharp
switch (targetNum)
{
    case 1: pos.Target1Price = newPrice; break;
    case 2: pos.Target2Price = newPrice; break;
    case 3: pos.Target3Price = newPrice; break;
    case 4: pos.Target4Price = newPrice; break;
    case 5: pos.Target5Price = newPrice; break;
}
```

**After**:
```csharp
UpdateTargetPrice(pos, targetNum, newPrice);
```

#### Step 3: Replace Second Occurrence (Lines 289-304)
Replace the switch statement in the new order submission path:

**Before**:
```csharp
switch (targetNum)
{
    case 1: pos.Target1Price = newPrice; break;
    case 2: pos.Target2Price = newPrice; break;
    case 3: pos.Target3Price = newPrice; break;
    case 4: pos.Target4Price = newPrice; break;
    case 5: pos.Target5Price = newPrice; break;
}
```

**After**:
```csharp
UpdateTargetPrice(pos, targetNum, newPrice);
```

#### Step 4: Write Unit Tests
Create test file: `tests/V12_Performance.Tests/Orders/UpdateTargetPriceTests.cs`

```csharp
[TestFixture]
public class UpdateTargetPriceTests
{
    [Test]
    [TestCase(1, 100.50)]
    [TestCase(2, 101.25)]
    [TestCase(3, 102.00)]
    [TestCase(4, 103.75)]
    [TestCase(5, 104.50)]
    public void UpdateTargetPrice_ValidTargetNumber_UpdatesCorrectPrice(int targetNum, double newPrice)
    {
        // Arrange
        var pos = new PositionInfo();
        
        // Act
        UpdateTargetPrice(pos, targetNum, newPrice);
        
        // Assert
        switch (targetNum)
        {
            case 1: Assert.AreEqual(newPrice, pos.Target1Price); break;
            case 2: Assert.AreEqual(newPrice, pos.Target2Price); break;
            case 3: Assert.AreEqual(newPrice, pos.Target3Price); break;
            case 4: Assert.AreEqual(newPrice, pos.Target4Price); break;
            case 5: Assert.AreEqual(newPrice, pos.Target5Price); break;
        }
    }
}
```

### Acceptance Criteria
- [ ] UpdateTargetPrice method created with complexity ≤2
- [ ] First switch statement replaced (lines 218-233)
- [ ] Second switch statement replaced (lines 289-304)
- [ ] Unit tests added for all 5 target numbers
- [ ] All tests pass: `dotnet test`
- [ ] Build succeeds: `dotnet build`
- [ ] Complexity audit passes: `python scripts/complexity_audit.py`
- [ ] No behavioral changes (functional equivalence verified)
- [ ] CSharpier formatting applied: `dotnet csharpier format src/`

### Dependencies
- None (first ticket)

### Verification Commands
```bash
# Build
dotnet build

# Run tests
dotnet test

# Complexity audit
python scripts/complexity_audit.py

# Format check
dotnet csharpier check src/
```

---

## TICKET-2: Extract RepriceExistingOrder Helper

### Scope
- **Current Method**: `SyncLimitTarget`
- **Current CYC**: ≤15 (after TICKET-1)
- **Target CYC After Extraction**: ≤9
- **Helper Method CYC**: ≤6
- **Extraction**: Repricing logic for existing working orders (lines 203-253)

### Implementation

#### Step 1: Create RepriceExistingOrder Method
Add new private method after UpdateTargetPrice:

```csharp
/// <summary>
/// Handles repricing logic for existing working orders.
/// Extracted from SyncLimitTarget to reduce complexity.
/// </summary>
/// <param name="existingOrder">Existing working order</param>
/// <param name="newPrice">New calculated target price</param>
/// <param name="pos">Position information</param>
/// <param name="targetNum">Target number (1-5)</param>
/// <param name="entryName">Entry name for logging</param>
/// <param name="refreshed">Counter for refreshed orders</param>
private void RepriceExistingOrder(
    Order existingOrder,
    double newPrice,
    PositionInfo pos,
    int targetNum,
    string entryName,
    ref int refreshed)
{
    double priceDelta = Math.Abs(existingOrder.LimitPrice - newPrice);
    
    if (priceDelta >= TickSize)
    {
        try
        {
            ChangeOrder(existingOrder, existingOrder.Quantity, existingOrder.LimitPrice, newPrice);
            UpdateTargetPrice(pos, targetNum, newPrice);
            refreshed++;
        }
        catch (Exception ex)
        {
            LogError($"[{entryName}] Failed to reprice target {targetNum}: {ex.Message}");
        }
    }
}
```

#### Step 2: Replace Repricing Logic in SyncLimitTarget
Replace lines 203-253 with method call:

**Before**:
```csharp
if (hasWorkingOrder && existingOrder != null)
{
    double priceDelta = Math.Abs(existingOrder.LimitPrice - newPrice);
    
    if (priceDelta >= TickSize)
    {
        try
        {
            ChangeOrder(existingOrder, existingOrder.Quantity, existingOrder.LimitPrice, newPrice);
            switch (targetNum)
            {
                case 1: pos.Target1Price = newPrice; break;
                case 2: pos.Target2Price = newPrice; break;
                case 3: pos.Target3Price = newPrice; break;
                case 4: pos.Target4Price = newPrice; break;
                case 5: pos.Target5Price = newPrice; break;
            }
            refreshed++;
        }
        catch (Exception ex)
        {
            LogError($"[{entryName}] Failed to reprice target {targetNum}: {ex.Message}");
        }
    }
}
```

**After**:
```csharp
if (hasWorkingOrder && existingOrder != null)
{
    RepriceExistingOrder(existingOrder, newPrice, pos, targetNum, entryName, ref refreshed);
}
```

#### Step 3: Write Unit Tests
Create test file: `tests/V12_Performance.Tests/Orders/RepriceExistingOrderTests.cs`

```csharp
[TestFixture]
public class RepriceExistingOrderTests
{
    [Test]
    public void RepriceExistingOrder_PriceDeltaAboveThreshold_CallsChangeOrder()
    {
        // Arrange
        var existingOrder = new Order { LimitPrice = 100.00, Quantity = 10 };
        var newPrice = 101.00; // Delta = 1.00 (> TickSize)
        var pos = new PositionInfo();
        int refreshed = 0;
        
        // Act
        RepriceExistingOrder(existingOrder, newPrice, pos, 1, "TEST", ref refreshed);
        
        // Assert
        Assert.AreEqual(1, refreshed);
        Assert.AreEqual(newPrice, pos.Target1Price);
    }
    
    [Test]
    public void RepriceExistingOrder_PriceDeltaBelowThreshold_NoChange()
    {
        // Arrange
        var existingOrder = new Order { LimitPrice = 100.00, Quantity = 10 };
        var newPrice = 100.01; // Delta = 0.01 (< TickSize)
        var pos = new PositionInfo();
        int refreshed = 0;
        
        // Act
        RepriceExistingOrder(existingOrder, newPrice, pos, 1, "TEST", ref refreshed);
        
        // Assert
        Assert.AreEqual(0, refreshed);
    }
    
    [Test]
    public void RepriceExistingOrder_ChangeOrderThrows_HandlesException()
    {
        // Arrange
        var existingOrder = new Order { LimitPrice = 100.00, Quantity = 10 };
        var newPrice = 101.00;
        var pos = new PositionInfo();
        int refreshed = 0;
        
        // Mock ChangeOrder to throw exception
        
        // Act & Assert
        Assert.DoesNotThrow(() => 
            RepriceExistingOrder(existingOrder, newPrice, pos, 1, "TEST", ref refreshed));
        Assert.AreEqual(0, refreshed); // Should not increment on failure
    }
}
```

### Acceptance Criteria
- [ ] RepriceExistingOrder method created with complexity ≤6
- [ ] Repricing logic replaced in SyncLimitTarget (lines 203-253)
- [ ] Unit tests added for price delta threshold, success, and exception cases
- [ ] All tests pass: `dotnet test`
- [ ] Build succeeds: `dotnet build`
- [ ] Complexity audit passes: `python scripts/complexity_audit.py`
- [ ] SyncLimitTarget complexity ≤9
- [ ] No behavioral changes (functional equivalence verified)
- [ ] CSharpier formatting applied: `dotnet csharpier format src/`

### Dependencies
- **TICKET-1** must be completed first (UpdateTargetPrice method required)

### Verification Commands
```bash
# Build
dotnet build

# Run tests
dotnet test

# Complexity audit (verify SyncLimitTarget ≤9)
python scripts/complexity_audit.py

# Format check
dotnet csharpier check src/
```

---

## TICKET-3: Extract SubmitNewTargetOrder Helper

### Scope
- **Current Method**: `SyncLimitTarget`
- **Current CYC**: ≤9 (after TICKET-2)
- **Target CYC After Extraction**: ≤5
- **Helper Method CYC**: ≤7
- **Extraction**: New order submission logic (lines 254-304)

### Implementation

#### Step 1: Create SubmitNewTargetOrder Method
Add new private method after RepriceExistingOrder:

```csharp
/// <summary>
/// Handles new order submission when no working order exists.
/// Extracted from SyncLimitTarget to reduce complexity.
/// </summary>
/// <param name="pos">Position information</param>
/// <param name="targetNum">Target number (1-5)</param>
/// <param name="targetQty">Target quantity</param>
/// <param name="newPrice">New calculated target price</param>
/// <param name="entryName">Entry name for logging</param>
/// <param name="targetDict">Dictionary to store submitted order</param>
private void SubmitNewTargetOrder(
    PositionInfo pos,
    int targetNum,
    int targetQty,
    double newPrice,
    string entryName,
    ConcurrentDictionary<string, Order> targetDict)
{
    OrderAction action = (pos.MarketPosition == MarketPosition.Long) 
        ? OrderAction.Sell 
        : OrderAction.BuyToCover;
    
    try
    {
        Order newOrder = SubmitOrderUnmanaged(
            0,
            action,
            OrderType.Limit,
            targetQty,
            newPrice,
            0,
            string.Empty,
            $"{entryName}_T{targetNum}"
        );
        
        if (newOrder != null)
        {
            targetDict[newOrder.Name] = newOrder;
            UpdateTargetPrice(pos, targetNum, newPrice);
        }
    }
    catch (Exception ex)
    {
        LogError($"[{entryName}] Failed to submit target {targetNum}: {ex.Message}");
    }
}
```

#### Step 2: Replace Submission Logic in SyncLimitTarget
Replace lines 254-304 with method call:

**Before**:
```csharp
else
{
    OrderAction action = (pos.MarketPosition == MarketPosition.Long) 
        ? OrderAction.Sell 
        : OrderAction.BuyToCover;
    
    try
    {
        Order newOrder = SubmitOrderUnmanaged(
            0,
            action,
            OrderType.Limit,
            targetQty,
            newPrice,
            0,
            string.Empty,
            $"{entryName}_T{targetNum}"
        );
        
        if (newOrder != null)
        {
            targetDict[newOrder.Name] = newOrder;
            switch (targetNum)
            {
                case 1: pos.Target1Price = newPrice; break;
                case 2: pos.Target2Price = newPrice; break;
                case 3: pos.Target3Price = newPrice; break;
                case 4: pos.Target4Price = newPrice; break;
                case 5: pos.Target5Price = newPrice; break;
            }
        }
    }
    catch (Exception ex)
    {
        LogError($"[{entryName}] Failed to submit target {targetNum}: {ex.Message}");
    }
}
```

**After**:
```csharp
else
{
    SubmitNewTargetOrder(pos, targetNum, targetQty, newPrice, entryName, targetDict);
}
```

#### Step 3: Write Unit Tests
Create test file: `tests/V12_Performance.Tests/Orders/SubmitNewTargetOrderTests.cs`

```csharp
[TestFixture]
public class SubmitNewTargetOrderTests
{
    [Test]
    public void SubmitNewTargetOrder_LongPosition_UsesSellAction()
    {
        // Arrange
        var pos = new PositionInfo { MarketPosition = MarketPosition.Long };
        var targetDict = new ConcurrentDictionary<string, Order>();
        
        // Act
        SubmitNewTargetOrder(pos, 1, 10, 100.50, "TEST", targetDict);
        
        // Assert
        // Verify SubmitOrderUnmanaged called with OrderAction.Sell
        Assert.AreEqual(100.50, pos.Target1Price);
    }
    
    [Test]
    public void SubmitNewTargetOrder_ShortPosition_UsesBuyToCoverAction()
    {
        // Arrange
        var pos = new PositionInfo { MarketPosition = MarketPosition.Short };
        var targetDict = new ConcurrentDictionary<string, Order>();
        
        // Act
        SubmitNewTargetOrder(pos, 2, 10, 99.50, "TEST", targetDict);
        
        // Assert
        // Verify SubmitOrderUnmanaged called with OrderAction.BuyToCover
        Assert.AreEqual(99.50, pos.Target2Price);
    }
    
    [Test]
    public void SubmitNewTargetOrder_Success_StoresOrderInDict()
    {
        // Arrange
        var pos = new PositionInfo { MarketPosition = MarketPosition.Long };
        var targetDict = new ConcurrentDictionary<string, Order>();
        var mockOrder = new Order { Name = "TEST_T1" };
        
        // Mock SubmitOrderUnmanaged to return mockOrder
        
        // Act
        SubmitNewTargetOrder(pos, 1, 10, 100.50, "TEST", targetDict);
        
        // Assert
        Assert.IsTrue(targetDict.ContainsKey("TEST_T1"));
        Assert.AreEqual(100.50, pos.Target1Price);
    }
    
    [Test]
    public void SubmitNewTargetOrder_SubmitThrows_HandlesException()
    {
        // Arrange
        var pos = new PositionInfo { MarketPosition = MarketPosition.Long };
        var targetDict = new ConcurrentDictionary<string, Order>();
        
        // Mock SubmitOrderUnmanaged to throw exception
        
        // Act & Assert
        Assert.DoesNotThrow(() => 
            SubmitNewTargetOrder(pos, 1, 10, 100.50, "TEST", targetDict));
        Assert.AreEqual(0, targetDict.Count); // Should not add order on failure
    }
}
```

#### Step 4: Verify Final SyncLimitTarget Complexity
After extraction, SyncLimitTarget should be reduced to orchestration only:

```csharp
private void SyncLimitTarget(
    string entryName,
    PositionInfo pos,
    int targetNum,
    int targetQty,
    ConcurrentDictionary<string, Order> targetDict,
    Order existingOrder,
    bool hasWorkingOrder,
    ref int refreshed)
{
    double newPrice = CalculateTargetPriceFromPos(pos, targetNum);
    
    if (newPrice <= 0)
    {
        return;
    }
    
    if (hasWorkingOrder && existingOrder != null)
    {
        RepriceExistingOrder(existingOrder, newPrice, pos, targetNum, entryName, ref refreshed);
    }
    else
    {
        SubmitNewTargetOrder(pos, targetNum, targetQty, newPrice, entryName, targetDict);
    }
}
```

**Expected Complexity**: ≤5 (orchestration only)

### Acceptance Criteria
- [ ] SubmitNewTargetOrder method created with complexity ≤7
- [ ] Submission logic replaced in SyncLimitTarget (lines 254-304)
- [ ] Unit tests added for Long/Short directions, success, and exception cases
- [ ] All tests pass: `dotnet test`
- [ ] Build succeeds: `dotnet build`
- [ ] Complexity audit passes: `python scripts/complexity_audit.py`
- [ ] **SyncLimitTarget complexity ≤5** (FINAL TARGET MET)
- [ ] No behavioral changes (functional equivalence verified)
- [ ] CSharpier formatting applied: `dotnet csharpier format src/`
- [ ] Integration test verifies orchestration works correctly

### Dependencies
- **TICKET-1** must be completed first (UpdateTargetPrice method required)
- **TICKET-2** must be completed first (RepriceExistingOrder method required)

### Verification Commands
```bash
# Build
dotnet build

# Run tests
dotnet test

# Complexity audit (verify SyncLimitTarget ≤5)
python scripts/complexity_audit.py

# Format check
dotnet csharpier check src/

# Pre-push validation (full suite)
powershell -File .\scripts\pre_push_validation.ps1 -Fast
```

---

## Final Verification Checklist

### Complexity Targets
- [ ] SyncLimitTarget: ≤5 (orchestrator)
- [ ] UpdateTargetPrice: ≤2
- [ ] RepriceExistingOrder: ≤6
- [ ] SubmitNewTargetOrder: ≤7
- [ ] **Total Budget**: 20 (acceptable vs 17 in monolith)

### DNA Compliance
- [ ] Zero lock() statements introduced
- [ ] ASCII-only compliance maintained
- [ ] Correctness by construction preserved
- [ ] Jane Street alignment verified (all methods ≤8)

### PR Hygiene
- [ ] Diff size <10k characters
- [ ] Single file modified (src/V12_002.Orders.Management.StopSync.cs)
- [ ] No whitespace mutations
- [ ] No changes to callers/callees
- [ ] No changes to method signature

### Quality Gates
- [ ] Build passes: `dotnet build`
- [ ] All tests pass: `dotnet test`
- [ ] Complexity audit passes: `python scripts/complexity_audit.py`
- [ ] CSharpier formatting: `dotnet csharpier check src/`
- [ ] Pre-push validation: `powershell -File .\scripts\pre_push_validation.ps1 -Fast`

### Documentation
- [ ] Method comments updated with extraction rationale
- [ ] Test coverage documented
- [ ] Manifest updated with Phase 4 completion

---

## Execution Timeline

### Day 1: TICKET-1 (2-3 hours)
- Write UpdateTargetPrice tests
- Extract UpdateTargetPrice method
- Replace duplicated switch statements
- Verify complexity reduction
- Commit: "Extract UpdateTargetPrice helper (CYC ≤2)"

### Day 2: TICKET-2 (2-3 hours)
- Write RepriceExistingOrder tests
- Extract RepriceExistingOrder method
- Replace repricing logic
- Verify complexity reduction
- Commit: "Extract RepriceExistingOrder helper (CYC ≤6)"

### Day 3: TICKET-3 (2-3 hours)
- Write SubmitNewTargetOrder tests
- Extract SubmitNewTargetOrder method
- Replace submission logic
- Verify final complexity ≤5
- Commit: "Extract SubmitNewTargetOrder helper (CYC ≤7)"

### Day 3: Final Verification (1 hour)
- Run full test suite
- Run pre-push validation
- Verify PR hygiene
- Update documentation
- Commit: "Update documentation for EPIC-CCN-035"

---

## Rollback Plan

If any ticket fails acceptance criteria:

1. **Git Revert**: `git revert HEAD` (revert last commit)
2. **Analyze Failure**: Review complexity audit output
3. **Adjust Strategy**: Modify extraction boundaries if needed
4. **Retry**: Re-attempt ticket with adjusted approach

---

**Document Version**: 1.0
**Created**: 2026-06-15
**Status**: READY FOR EXECUTION
**Next Phase**: Phase 5 (Ticket Execution)
