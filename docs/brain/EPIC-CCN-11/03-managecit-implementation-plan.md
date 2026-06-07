# EPIC-CCN-11: ManageCIT Implementation Plan

**Date**: 2026-06-02  
**Author**: V12 Photon Engineer (Bob CLI)  
**Stage**: Stage 2 (Arch Planning)  
**Protocol**: Phase 6 Recursive  
**Status**: READY FOR STAGE 3 (DNA & PR Audit)

---

## Section 1: Extraction Sequence

### Overview

**Bottom-Up Extraction Strategy** - Extract smallest, most independent helpers first to minimize risk and enable incremental verification.

**Total Extraction Time**: ~90 minutes (5 phases)

---

### Phase 1: Extract ValidateCitConfiguration

**Rationale**: Early validation logic with no dependencies on other helpers. Fail-fast pattern reduces main method complexity immediately.

**Target Lines**: 70-85 (16 LOC)

**Expected CYC Reduction**: 5 (from 26 → 21)

**Estimated Time**: 15 minutes

**Method Signature**:
```csharp
private bool ValidateCitConfiguration(out double citOffset)
```

**Extraction Steps**:
1. Create new private method after [`ManageCIT()`](src/V12_002.Orders.Management.Flatten.cs:68)
2. Move lines 70-85 into method body
3. Replace original lines with single call: `if (!ValidateCitConfiguration(out double citOffset)) return;`
4. Run CSharpier: `dotnet csharpier format src/`
5. Verify compilation: `dotnet build`
6. Run complexity audit: `python scripts/complexity_audit.py`
7. Commit: `refactor: Extract ValidateCitConfiguration from ManageCIT (Phase 1/5)`

**Verification**:
- [ ] CYC reduced to 21
- [ ] Build succeeds
- [ ] ASCII gate passes (deploy-sync.ps1)
- [ ] No logic drift (behavior identical)

---

### Phase 2: Extract CalculateNudgedPrice

**Rationale**: Pure function with no side effects. Simplest extraction, highest confidence.

**Target Lines**: 123-128 (6 LOC)

**Expected CYC Reduction**: 2 (from 21 → 19)

**Estimated Time**: 10 minutes

**Method Signature**:
```csharp
private double CalculateNudgedPrice(Order order, double citOffset)
```

**Extraction Steps**:
1. Create new private method after ValidateCitConfiguration
2. Move lines 123-128 into method body
3. Replace original lines with: `double newLimitPrice = CalculateNudgedPrice(order, citOffset);`
4. Run CSharpier: `dotnet csharpier format src/`
5. Verify compilation: `dotnet build`
6. Run complexity audit: `python scripts/complexity_audit.py`
7. Commit: `refactor: Extract CalculateNudgedPrice from ManageCIT (Phase 2/5)`

**Verification**:
- [ ] CYC reduced to 19
- [ ] Build succeeds
- [ ] ASCII gate passes (deploy-sync.ps1)
- [ ] No logic drift (behavior identical)

---

### Phase 3: Extract ShouldChaseOrder

**Rationale**: Decision logic that depends on Phase 1 validation. Contains Build 984 directional fix (critical business logic).

**Target Lines**: 93-114 (22 LOC)

**Expected CYC Reduction**: 7 (from 19 → 12)

**Estimated Time**: 20 minutes

**Method Signature**:
```csharp
private bool ShouldChaseOrder(Order order, string key, out double currentPrice, out double limitPrice)
```

**Extraction Steps**:
1. Create new private method after CalculateNudgedPrice
2. Move lines 93-114 into method body
3. Replace original lines with: `if (!ShouldChaseOrder(order, key, out double currentPrice, out double limitPrice)) continue;`
4. Run CSharpier: `dotnet csharpier format src/`
5. Verify compilation: `dotnet build`
6. Run complexity audit: `python scripts/complexity_audit.py`
7. Commit: `refactor: Extract ShouldChaseOrder from ManageCIT (Phase 3/5)`

**Verification**:
- [ ] CYC reduced to 12
- [ ] Build succeeds
- [ ] ASCII gate passes (deploy-sync.ps1)
- [ ] No logic drift (Build 984 directional logic preserved)

**Critical**: This helper contains the Build 984 fix for directional bar-price logic. Verify:
- Long entry: `Low[0] <= limitPrice` (price drops to limit)
- Short entry: `High[0] >= limitPrice` (price rises to limit)

---

### Phase 4: Extract ExecuteLocalNudge

**Rationale**: Simple action with no complex branching. Depends on Phase 2 (CalculateNudgedPrice).

**Target Lines**: 174-181 (8 LOC)

**Expected CYC Reduction**: 1 (from 12 → 11)

**Estimated Time**: 15 minutes

**Method Signature**:
```csharp
private void ExecuteLocalNudge(string key, Order order, double newLimitPrice)
```

**Extraction Steps**:
1. Create new private method after ShouldChaseOrder
2. Move lines 174-181 into method body (excluding `_citNudgedKeys.TryAdd()` - see note below)
3. Replace original lines with: `ExecuteLocalNudge(key, order, newLimitPrice);`
4. Run CSharpier: `dotnet csharpier format src/`
5. Verify compilation: `dotnet build`
6. Run complexity audit: `python scripts/complexity_audit.py`
7. Commit: `refactor: Extract ExecuteLocalNudge from ManageCIT (Phase 4/5)`

**Note**: The `_citNudgedKeys.TryAdd(key, true)` call (line 181) will be moved OUTSIDE both helper calls in Phase 5, since it applies to both local and follower nudges.

**Verification**:
- [ ] CYC reduced to 11
- [ ] Build succeeds
- [ ] ASCII gate passes (deploy-sync.ps1)
- [ ] No logic drift (behavior identical)

---

### Phase 5: Extract ExecuteFollowerNudge

**Rationale**: Most complex helper with budget management and self-enqueue logic. Depends on Phases 2-4. Extract last to minimize risk.

**Target Lines**: 130-173 (44 LOC)

**Expected CYC Reduction**: 3 (from 11 → 8, then refactor main to ≤15)

**Estimated Time**: 30 minutes

**Method Signature**:
```csharp
private void ExecuteFollowerNudge(string key, Order order, double newLimitPrice, PositionInfo pos, ref int brokerBudget)
```

**Extraction Steps**:
1. Create new private method after ExecuteLocalNudge
2. Move lines 130-173 into method body (excluding `_citNudgedKeys.TryAdd()`)
3. Replace original lines with: `ExecuteFollowerNudge(key, order, newLimitPrice, pos, ref _citBrokerBudget);`
4. Move `_citNudgedKeys.TryAdd(key, true)` AFTER both helper calls in main method
5. Run CSharpier: `dotnet csharpier format src/`
6. Verify compilation: `dotnet build`
7. Run complexity audit: `python scripts/complexity_audit.py`
8. Commit: `refactor: Extract ExecuteFollowerNudge from ManageCIT (Phase 5/5)`

**Critical**: This helper contains Build 1109 freeze-proof logic. Verify:
- Budget check: `if (brokerBudget <= 0)` → self-enqueue and return
- Budget decrement: `brokerBudget -= 2` (Cancel + Submit)
- Self-enqueue: `Enqueue(ctx => ctx.ManageCIT())`

**Verification**:
- [ ] CYC reduced to ≤15 (target achieved)
- [ ] Build succeeds
- [ ] ASCII gate passes (deploy-sync.ps1)
- [ ] No logic drift (Build 1109 budget logic preserved)

---

### Final Refactor: Main Method Cleanup

**After Phase 5**, refactor main method to call helpers:

**Before** (98 LOC, CYC=26):
```csharp
private void ManageCIT()
{
    // 98 lines of complex logic
}
```

**After** (~60 LOC, CYC≤15):
```csharp
private void ManageCIT()
{
    if (!ValidateCitConfiguration(out double citOffset))
        return;

    int _citBrokerBudget = MaxBrokerCallsPerCycle;
    
    foreach (var kvp in entryOrders.ToArray())
    {
        string key = kvp.Key;
        Order order = kvp.Value;
        
        if (!ShouldChaseOrder(order, key, out double currentPrice, out double limitPrice))
            continue;
        
        PositionInfo pos = null;
        activePositions.TryGetValue(key, out pos);
        bool isFollower = pos != null && pos.IsFollower && pos.ExecutingAccount != null;
        
        try
        {
            double newLimitPrice = CalculateNudgedPrice(order, citOffset);
            
            if (isFollower)
            {
                ExecuteFollowerNudge(key, order, newLimitPrice, pos, ref _citBrokerBudget);
            }
            else
            {
                ExecuteLocalNudge(key, order, newLimitPrice);
            }
            
            _citNudgedKeys.TryAdd(key, true); // One-shot guard (Build 949)
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("ChangeOrder"))
        {
            Print($"[CIT] WARNING chasing {key} (known quirk): {ex.Message}");
        }
        catch (Exception ex)
        {
            Print($"[CIT] CRITICAL chasing {key}: {ex.ToString()}");
        }
    }
}
```

**Verification**:
- [ ] Final CYC ≤15 (Jane Street compliant)
- [ ] Max nesting depth ≤4
- [ ] All 5 helpers extracted
- [ ] Zero logic drift
- [ ] V12 DNA compliance maintained

---

## Section 2: TDD Test Structure

### Test File Location

**Path**: `tests/V12_Performance.Tests/Orders/ManageCITTests.cs`

**Namespace**: `V12_Performance.Tests.Orders`

**Test Class**: `ManageCITTests`

---

### Test Infrastructure Reuse (from EPIC-CCN-10)

**Mock Types** (already implemented):
- `MockOrder` - Order state simulation
- `MockAccount` - Broker API simulation
- `MockInstrument` - Instrument/tick size simulation
- `MockPositionInfo` - Position tracking simulation

**Test Patterns**:
- Arrange-Act-Assert
- State machine verification
- Actor pattern validation

---

### Test Cases (15+ total)

#### ValidateCitConfiguration Tests (3 tests)

**Test 1**: `ValidateCitConfiguration_WhenDisabled_ReturnsFalse`
```csharp
[Fact]
public void ValidateCitConfiguration_WhenDisabled_ReturnsFalse()
{
    // Arrange
    activePositions.Clear();
    entryOrders.Clear();
    
    // Act
    bool result = ValidateCitConfiguration(out double citOffset);
    
    // Assert
    Assert.False(result);
    Assert.Equal(0, citOffset);
}
```

**Test 2**: `ValidateCitConfiguration_WhenEnabledWithValidConfig_ReturnsTrue`
```csharp
[Fact]
public void ValidateCitConfiguration_WhenEnabledWithValidConfig_ReturnsTrue()
{
    // Arrange
    activePositions.Add("test", new MockPositionInfo());
    ChaseIfTouchPoints = "2.5";
    _propagationActive = false;
    
    // Act
    bool result = ValidateCitConfiguration(out double citOffset);
    
    // Assert
    Assert.True(result);
    Assert.Equal(2.5, citOffset);
}
```

**Test 3**: `ValidateCitConfiguration_WhenPropagationActive_ReturnsFalse`
```csharp
[Fact]
public void ValidateCitConfiguration_WhenPropagationActive_ReturnsFalse()
{
    // Arrange
    activePositions.Add("test", new MockPositionInfo());
    ChaseIfTouchPoints = "2.5";
    _propagationActive = true; // Build 924 Fix C
    
    // Act
    bool result = ValidateCitConfiguration(out double citOffset);
    
    // Assert
    Assert.False(result);
}
```

---

#### CalculateNudgedPrice Tests (4 tests)

**Test 1**: `CalculateNudgedPrice_Long_ReturnsLowMinusTickSize`
```csharp
[Fact]
public void CalculateNudgedPrice_Long_ReturnsLowMinusTickSize()
{
    // Arrange
    var order = new MockOrder
    {
        OrderAction = OrderAction.Buy,
        LimitPrice = 100.00
    };
    double citOffset = 2.0;
    Instrument.MasterInstrument.TickSize = 0.25;
    
    // Act
    double result = CalculateNudgedPrice(order, citOffset);
    
    // Assert
    Assert.Equal(100.50, result); // 100.00 + (2.0 * 0.25)
}
```

**Test 2**: `CalculateNudgedPrice_Short_ReturnsHighPlusTickSize`
```csharp
[Fact]
public void CalculateNudgedPrice_Short_ReturnsHighPlusTickSize()
{
    // Arrange
    var order = new MockOrder
    {
        OrderAction = OrderAction.Sell,
        LimitPrice = 100.00
    };
    double citOffset = 2.0;
    Instrument.MasterInstrument.TickSize = 0.25;
    
    // Act
    double result = CalculateNudgedPrice(order, citOffset);
    
    // Assert
    Assert.Equal(99.50, result); // 100.00 - (2.0 * 0.25)
}
```

**Test 3**: `CalculateNudgedPrice_WithZeroTickSize_ThrowsException`
```csharp
[Fact]
public void CalculateNudgedPrice_WithZeroTickSize_ThrowsException()
{
    // Arrange
    var order = new MockOrder { OrderAction = OrderAction.Buy, LimitPrice = 100.00 };
    double citOffset = 2.0;
    Instrument.MasterInstrument.TickSize = 0; // Invalid
    
    // Act & Assert
    Assert.Throws<DivideByZeroException>(() => CalculateNudgedPrice(order, citOffset));
}
```

**Test 4**: `CalculateNudgedPrice_WithRounding_ReturnsTickAlignedPrice`
```csharp
[Fact]
public void CalculateNudgedPrice_WithRounding_ReturnsTickAlignedPrice()
{
    // Arrange
    var order = new MockOrder { OrderAction = OrderAction.Buy, LimitPrice = 100.00 };
    double citOffset = 1.5; // Non-integer offset
    Instrument.MasterInstrument.TickSize = 0.25;
    
    // Act
    double result = CalculateNudgedPrice(order, citOffset);
    
    // Assert
    Assert.Equal(100.25, result); // Rounded to nearest tick
}
```

---

#### ShouldChaseOrder Tests (5 tests)

**Test 1**: `ShouldChaseOrder_WhenBudgetExhausted_ReturnsFalse`
```csharp
[Fact]
public void ShouldChaseOrder_WhenOrderNull_ReturnsFalse()
{
    // Arrange
    Order order = null;
    string key = "test";
    
    // Act
    bool result = ShouldChaseOrder(order, key, out double currentPrice, out double limitPrice);
    
    // Assert
    Assert.False(result);
}
```

**Test 2**: `ShouldChaseOrder_WhenOneShotGuardActive_ReturnsFalse`
```csharp
[Fact]
public void ShouldChaseOrder_WhenOneShotGuardActive_ReturnsFalse()
{
    // Arrange
    var order = new MockOrder { OrderState = OrderState.Working, OrderType = OrderType.Limit };
    string key = "test";
    _citNudgedKeys.TryAdd(key, true); // Already nudged (Build 949)
    
    // Act
    bool result = ShouldChaseOrder(order, key, out double currentPrice, out double limitPrice);
    
    // Assert
    Assert.False(result);
}
```

**Test 3**: `ShouldChaseOrder_LongEntry_LowTouchedLimit_ReturnsTrue`
```csharp
[Fact]
public void ShouldChaseOrder_LongEntry_LowTouchedLimit_ReturnsTrue()
{
    // Arrange (Build 984 directional fix)
    var order = new MockOrder
    {
        OrderState = OrderState.Working,
        OrderType = OrderType.Limit,
        OrderAction = OrderAction.Buy,
        LimitPrice = 100.00
    };
    string key = "test";
    Low[0] = 99.75; // Price dropped to limit
    
    // Act
    bool result = ShouldChaseOrder(order, key, out double currentPrice, out double limitPrice);
    
    // Assert
    Assert.True(result);
    Assert.Equal(99.75, currentPrice); // Low[0] for Buy
    Assert.Equal(100.00, limitPrice);
}
```

**Test 4**: `ShouldChaseOrder_ShortEntry_HighTouchedLimit_ReturnsTrue`
```csharp
[Fact]
public void ShouldChaseOrder_ShortEntry_HighTouchedLimit_ReturnsTrue()
{
    // Arrange (Build 984 directional fix)
    var order = new MockOrder
    {
        OrderState = OrderState.Working,
        OrderType = OrderType.Limit,
        OrderAction = OrderAction.Sell,
        LimitPrice = 100.00
    };
    string key = "test";
    High[0] = 100.25; // Price rose to limit
    
    // Act
    bool result = ShouldChaseOrder(order, key, out double currentPrice, out double limitPrice);
    
    // Assert
    Assert.True(result);
    Assert.Equal(100.25, currentPrice); // High[0] for Sell
    Assert.Equal(100.00, limitPrice);
}
```

**Test 5**: `ShouldChaseOrder_WhenOrderNotWorking_ReturnsFalse`
```csharp
[Fact]
public void ShouldChaseOrder_WhenOrderNotWorking_ReturnsFalse()
{
    // Arrange
    var order = new MockOrder
    {
        OrderState = OrderState.Filled, // Not Working
        OrderType = OrderType.Limit
    };
    string key = "test";
    
    // Act
    bool result = ShouldChaseOrder(order, key, out double currentPrice, out double limitPrice);
    
    // Assert
    Assert.False(result);
}
```

---

#### ExecuteLocalNudge Tests (2 tests)

**Test 1**: `ExecuteLocalNudge_WhenOrderExists_CallsChangeOrder`
```csharp
[Fact]
public void ExecuteLocalNudge_WhenOrderExists_CallsChangeOrder()
{
    // Arrange
    var order = new MockOrder { Quantity = 1, LimitPrice = 100.00 };
    string key = "test";
    double newLimitPrice = 100.50;
    bool changeOrderCalled = false;
    
    // Mock ChangeOrder
    ChangeOrderCallback = (o, qty, limit, stop) =>
    {
        changeOrderCalled = true;
        Assert.Equal(order, o);
        Assert.Equal(1, qty);
        Assert.Equal(100.50, limit);
    };
    
    // Act
    ExecuteLocalNudge(key, order, newLimitPrice);
    
    // Assert
    Assert.True(changeOrderCalled);
    Assert.True(_citNudgedKeys.ContainsKey(key)); // One-shot guard set
}
```

**Test 2**: `ExecuteLocalNudge_LogsCorrectMessage`
```csharp
[Fact]
public void ExecuteLocalNudge_LogsCorrectMessage()
{
    // Arrange
    var order = new MockOrder { Quantity = 1, LimitPrice = 100.00 };
    string key = "test";
    double newLimitPrice = 100.50;
    string loggedMessage = null;
    
    // Mock Print
    PrintCallback = (msg) => { loggedMessage = msg; };
    
    // Act
    ExecuteLocalNudge(key, order, newLimitPrice);
    
    // Assert
    Assert.Contains("[CIT] LOCAL nudge: test", loggedMessage);
    Assert.Contains("100.00 -> 100.50", loggedMessage);
}
```

---

#### ExecuteFollowerNudge Tests (3 tests)

**Test 1**: `ExecuteFollowerNudge_WhenFollowerExists_CallsChangeOrder`
```csharp
[Fact]
public void ExecuteFollowerNudge_WhenFollowerExists_CallsChangeOrder()
{
    // Arrange
    var order = new MockOrder { OrderAction = OrderAction.Buy, Quantity = 1, LimitPrice = 100.00 };
    var pos = new MockPositionInfo { IsFollower = true, ExecutingAccount = new MockAccount() };
    string key = "test";
    double newLimitPrice = 100.50;
    int brokerBudget = 5;
    
    bool cancelCalled = false;
    bool submitCalled = false;
    
    pos.ExecutingAccount.CancelCallback = (orders) => { cancelCalled = true; };
    pos.ExecutingAccount.SubmitCallback = (orders) => { submitCalled = true; };
    
    // Act
    ExecuteFollowerNudge(key, order, newLimitPrice, pos, ref brokerBudget);
    
    // Assert
    Assert.True(cancelCalled);
    Assert.True(submitCalled);
    Assert.Equal(3, brokerBudget); // Decremented by 2
    Assert.True(entryOrders.ContainsKey(key)); // Dictionary updated
}
```

**Test 2**: `ExecuteFollowerNudge_WhenBudgetExhausted_EnqueuesRetry`
```csharp
[Fact]
public void ExecuteFollowerNudge_WhenBudgetExhausted_EnqueuesRetry()
{
    // Arrange (Build 1109 freeze-proof)
    var order = new MockOrder();
    var pos = new MockPositionInfo { IsFollower = true, ExecutingAccount = new MockAccount() };
    string key = "test";
    double newLimitPrice = 100.50;
    int brokerBudget = 0; // Exhausted
    
    bool enqueueCalled = false;
    EnqueueCallback = (action) => { enqueueCalled = true; };
    
    // Act
    ExecuteFollowerNudge(key, order, newLimitPrice, pos, ref brokerBudget);
    
    // Assert
    Assert.True(enqueueCalled); // Self-enqueue called
    Assert.Equal(0, brokerBudget); // Budget unchanged
}
```

**Test 3**: `ExecuteFollowerNudge_WhenCreateOrderReturnsNull_LogsError`
```csharp
[Fact]
public void ExecuteFollowerNudge_WhenCreateOrderReturnsNull_LogsError()
{
    // Arrange
    var order = new MockOrder { OrderAction = OrderAction.Buy, Quantity = 1 };
    var pos = new MockPositionInfo { IsFollower = true, ExecutingAccount = new MockAccount() };
    string key = "test";
    double newLimitPrice = 100.50;
    int brokerBudget = 5;
    
    pos.ExecutingAccount.CreateOrderCallback = (...) => null; // Simulate failure
    string loggedMessage = null;
    PrintCallback = (msg) => { loggedMessage = msg; };
    
    // Act
    ExecuteFollowerNudge(key, order, newLimitPrice, pos, ref brokerBudget);
    
    // Assert
    Assert.Contains("[CIT] ERROR: CreateOrder returned null", loggedMessage);
    Assert.Equal(3, brokerBudget); // Budget still decremented (Cancel called)
}
```

---

### Test Execution Order

**Phase 1**: Write all tests (red phase)
**Phase 2**: Extract helpers one by one (green phase)
**Phase 3**: Verify all tests pass (refactor phase)

**Command**: `dotnet test tests/V12_Performance.Tests/Orders/ManageCITTests.cs`

---

## Section 3: Code Changes

### File: `src/V12_002.Orders.Management.Flatten.cs`

**Changes Summary**:
1. Add 5 new private methods (after ManageCIT)
2. Refactor ManageCIT to call helpers
3. Preserve all existing behavior (zero logic drift)
4. Maintain V12 DNA compliance (lock-free, ASCII-only)

**Line Count Estimate**:
- **Before**: 98 lines (ManageCIT only)
- **After**: ~150 lines total (main + 5 helpers)
- **Net Change**: +52 lines

**Breakdown**:
- ManageCIT (main): ~60 LOC
- ValidateCitConfiguration: ~16 LOC
- ShouldChaseOrder: ~22 LOC
- CalculateNudgedPrice: ~6 LOC
- ExecuteLocalNudge: ~8 LOC
- ExecuteFollowerNudge: ~44 LOC

---

### Helper Method Placement

**Location**: After ManageCIT method (line 194), before next method

**Order** (top to bottom):
1. ManageCIT (main dispatcher)
2. ValidateCitConfiguration
3. ShouldChaseOrder
4. CalculateNudgedPrice
5. ExecuteLocalNudge
6. ExecuteFollowerNudge

**Rationale**: Logical flow from validation → decision → calculation → execution

---

### V12 DNA Compliance Checklist

- [ ] **Lock-Free**: No `lock()` statements added
- [ ] **ASCII-Only**: No Unicode characters in strings
- [ ] **Actor Pattern**: All `Enqueue()` calls preserved
- [ ] **Zero Logic Drift**: Behavior identical to original
- [ ] **Correctness by Construction**: Type-safe signatures

---

## Section 4: Validation Checklist

### Pre-Extraction

- [ ] Read mini-spec and understand all 5 helpers
- [ ] Confirm CYC=26 baseline via complexity audit
- [ ] Create feature branch: `git checkout -b src/epic-ccn-11-managecit`
- [ ] Backup current state: `git stash push -m "pre-epic-ccn-11-backup"`
- [ ] Verify clean working directory: `git status`

---

### During Extraction (per phase)

**Phase 1** (ValidateCitConfiguration):
- [ ] Extract helper method
- [ ] Update ManageCIT to call helper
- [ ] Run CSharpier: `dotnet csharpier format src/`
- [ ] Verify compilation: `dotnet build`
- [ ] Run complexity audit: `python scripts/complexity_audit.py`
- [ ] Verify CYC reduction (26 → 21)
- [ ] Commit: `refactor: Extract ValidateCitConfiguration from ManageCIT (Phase 1/5)`

**Phase 2** (CalculateNudgedPrice):
- [ ] Extract helper method
- [ ] Update ManageCIT to call helper
- [ ] Run CSharpier: `dotnet csharpier format src/`
- [ ] Verify compilation: `dotnet build`
- [ ] Run complexity audit: `python scripts/complexity_audit.py`
- [ ] Verify CYC reduction (21 → 19)
- [ ] Commit: `refactor: Extract CalculateNudgedPrice from ManageCIT (Phase 2/5)`

**Phase 3** (ShouldChaseOrder):
- [ ] Extract helper method
- [ ] Update ManageCIT to call helper
- [ ] Run CSharpier: `dotnet csharpier format src/`
- [ ] Verify compilation: `dotnet build`
- [ ] Run complexity audit: `python scripts/complexity_audit.py`
- [ ] Verify CYC reduction (19 → 12)
- [ ] Verify Build 984 directional logic preserved
- [ ] Commit: `refactor: Extract ShouldChaseOrder from ManageCIT (Phase 3/5)`

**Phase 4** (ExecuteLocalNudge):
- [ ] Extract helper method
- [ ] Update ManageCIT to call helper
- [ ] Run CSharpier: `dotnet csharpier format src/`
- [ ] Verify compilation: `dotnet build`
- [ ] Run complexity audit: `python scripts/complexity_audit.py`
- [ ] Verify CYC reduction (12 → 11)
- [ ] Commit: `refactor: Extract ExecuteLocalNudge from ManageCIT (Phase 4/5)`

**Phase 5** (ExecuteFollowerNudge):
- [ ] Extract helper method
- [ ] Update ManageCIT to call helper
- [ ] Move `_citNudgedKeys.TryAdd()` outside helper calls
- [ ] Run CSharpier: `dotnet csharpier format src/`
- [ ] Verify compilation: `dotnet build`
- [ ] Run complexity audit: `python scripts/complexity_audit.py`
- [ ] Verify CYC reduction (11 → ≤15)
- [ ] Verify Build 1109 budget logic preserved
- [ ] Commit: `refactor: Extract ExecuteFollowerNudge from ManageCIT (Phase 5/5)`

---

### Post-Extraction

- [ ] Run pre-push validation: `powershell -File .\scripts\pre_push_validation.ps1`
- [ ] Verify all 13 checks pass
- [ ] Verify final CYC ≤15
- [ ] Verify max nesting depth ≤4
- [ ] Run deploy-sync.ps1: `powershell -File .\deploy-sync.ps1`
- [ ] Verify ASCII gate passes
- [ ] Verify hard link integrity (81 files)
- [ ] Press F5 in NinjaTrader
- [ ] Verify BUILD_TAG loaded successfully
- [ ] Run risk audit (9 cases)
- [ ] Run TDD tests: `dotnet test tests/V12_Performance.Tests/Orders/ManageCITTests.cs`
- [ ] Verify 15+ tests passing
- [ ] Create PR: `gh pr create --title "EPIC-CCN-11: ManageCIT Extraction (CYC 26 → ≤15)"`
- [ ] Run `/pr-loop <PR_NUMBER>` to drive PHS to 100/100

---

## Section 5: Rollback Plan

### If Extraction Fails

**Immediate Rollback** (< 5 minutes):

1. **Revert Last Commit**:
   ```powershell
   git reset --hard HEAD~1
   ```

2. **Restore Backup**:
   ```powershell
   git stash pop
   ```

3. **Verify Rollback**:
   ```powershell
   dotnet build
   powershell -File .\deploy-sync.ps1
   ```

4. **Document Failure**:
   - Create: `docs/brain/EPIC-CCN-11/failure-analysis.md`
   - Include: Error logs, failed tests, root cause analysis
   - Action: Return to Stage 1 (Vision/Spec) with lessons learned

**Recovery Time**: < 5 minutes

---

### If Partial Extraction Succeeds

**Scenario**: Phases 1-3 succeed, Phase 4 fails

**Action**:
1. Keep successful phases (commit them)
2. Revert failed phase only: `git reset --soft HEAD~1`
3. Analyze failure in failed phase
4. Fix and retry failed phase
5. Continue with remaining phases

**Recovery Time**: 15-30 minutes

---

## Section 6: Success Criteria

### Quantitative Metrics

| Metric | Before | Target | Verification Method |
|--------|--------|--------|---------------------|
| **Cyclomatic Complexity** | 26 | ≤15 | `python scripts/complexity_audit.py` |
| **Max Nesting Depth** | 6 | ≤4 | Manual code review |
| **Lines of Code (main)** | 98 | ≤60 | Line count |
| **Helper Count** | 0 | 5 | File structure |
| **Test Count** | 0 | ≥15 | `dotnet test --list-tests` |
| **Test Coverage** | 0% | ≥80% | `dotnet test --collect:"XPlat Code Coverage"` |
| **Build Time** | Baseline | ≤Baseline+5% | `Measure-Command { dotnet build }` |

**Acceptance Criteria**:
- ✅ CYC reduced by ≥42% (26 → ≤15)
- ✅ Nesting reduced by ≥33% (6 → ≤4)
- ✅ All 15+ TDD tests passing
- ✅ Zero compilation errors
- ✅ Zero linting violations
- ✅ Pre-push validation: 13/13 checks passed

---

### Qualitative Criteria

- [ ] **Readability**: Each helper has clear single responsibility
- [ ] **Testability**: All helpers independently testable
- [ ] **Maintainability**: Future CIT enhancements easier to implement
- [ ] **Correctness**: Zero logic drift, behavior identical to original
- [ ] **V12 DNA Compliance**: Lock-free, ASCII-only, actor pattern preserved
- [ ] **Jane Street Alignment**: Cognitive simplicity, correctness by construction

---

### Business Verification

**Manual Testing in Sim101**:

1. **Long Entry Test**:
   - [ ] Place Buy Limit order below market (e.g., 100.00 when market is 101.00)
   - [ ] Wait for price to drop to limit (Low[0] <= 100.00)
   - [ ] Verify order nudged toward market (e.g., 100.50)
   - [ ] Verify one-shot guard (no re-nudge on next bar)
   - [ ] Verify log: `[CIT] LOCAL nudge: ... | 100.00 -> 100.50`

2. **Short Entry Test**:
   - [ ] Place Sell Limit order above market (e.g., 102.00 when market is 101.00)
   - [ ] Wait for price to rise to limit (High[0] >= 102.00)
   - [ ] Verify order nudged toward market (e.g., 101.50)
   - [ ] Verify one-shot guard (no re-nudge on next bar)
   - [ ] Verify log: `[CIT] LOCAL nudge: ... | 102.00 -> 101.50`

3. **Budget Exhaustion Test**:
   - [ ] Place 10 limit orders (exceed budget of 5)
   - [ ] Verify first 5 nudged immediately
   - [ ] Verify remaining 5 deferred (self-enqueue)
   - [ ] Verify log: `[CIT] Broker budget exhausted -- deferring remaining nudges`

4. **Follower Order Test** (if fleet accounts configured):
   - [ ] Place follower limit order
   - [ ] Wait for price touch
   - [ ] Verify follower order cancelled and resubmitted
   - [ ] Verify log: `[CIT] FLEET nudge: ... on [account] | ...`

---

## Section 7: Risk Mitigation

### Risk 1: Budget Exhaustion Logic Breaks

**Severity**: 🔴 HIGH  
**Probability**: 🟢 LOW

**Mitigation**:
- TDD test: `ExecuteFollowerNudge_WhenBudgetExhausted_EnqueuesRetry`
- Manual test: Place 10 orders, verify deferred continuation
- F5 verification: Monitor logs for budget exhaustion message

**Verification**:
- [ ] Run risk audit Case 2 (Contract Sizing)
- [ ] Verify no strategy thread stall
- [ ] Verify deferred orders processed on next drain

---

### Risk 2: Directional Logic Inverted

**Severity**: 🔴 HIGH  
**Probability**: 🟢 LOW

**Mitigation**:
- TDD tests: `ShouldChaseOrder_LongEntry_LowTouchedLimit_ReturnsTrue`, `ShouldChaseOrder_ShortEntry_HighTouchedLimit_ReturnsTrue`
- Manual test: Place Long + Short orders, verify correct directional nudge
- F5 verification: Monitor logs for correct price comparisons

**Verification**:
- [ ] Manual trade execution (Long + Short)
- [ ] Verify Long uses Low[0] <= limitPrice
- [ ] Verify Short uses High[0] >= limitPrice
- [ ] Verify no instant market conversion (Build 984 bug)

---

### Risk 3: One-Shot Guard Bypassed

**Severity**: 🔴 HIGH  
**Probability**: 🟢 LOW

**Mitigation**:
- TDD test: `ShouldChaseOrder_WhenOneShotGuardActive_ReturnsFalse`
- Manual test: Verify order not re-nudged on subsequent bars
- F5 verification: Monitor logs for single nudge per order

**Verification**:
- [ ] Run risk audit Case 4 (Symmetry Guard)
- [ ] Verify `_citNudgedKeys.TryAdd()` called after nudge
- [ ] Verify no double-nudge in logs

---

### Risk 4: Follower Propagation Fails

**Severity**: 🟡 MEDIUM  
**Probability**: 🟢 LOW

**Mitigation**:
- TDD test: `ExecuteFollowerNudge_WhenFollowerExists_CallsChangeOrder`
- Manual test: Place follower order, verify cancel + resubmit
- F5 verification: Monitor logs for FLEET nudge messages

**Verification**:
- [ ] Run risk audit Case 7 (SIMA Broadcast)
- [ ] Verify follower order cancelled
- [ ] Verify follower order resubmitted at new price
- [ ] Verify `entryOrders[key]` updated

---

### Risk 5: Performance Degradation

**Severity**: 🟡 MEDIUM  
**Probability**: 🟢 LOW

**Mitigation**:
- BenchmarkDotNet baseline comparison
- Measure latency histograms before/after
- Verify no additional allocations

**Verification**:
- [ ] Run benchmark: `dotnet run --project benchmarks/V12_Performance.Benchmarks.csproj`
- [ ] Compare latency: Before vs After
- [ ] Verify ≤5% performance impact

---

### Risk 6: Hard Link Desync

**Severity**: 🟡 MEDIUM  
**Probability**: 🟢 LOW

**Mitigation**:
- Run `deploy-sync.ps1` after every commit
- Verify ASCII gate passes
- Verify 81 hard links synchronized

**Verification**:
- [ ] Run: `powershell -File .\deploy-sync.ps1`
- [ ] Verify output: `ASCII gate: PASS`
- [ ] Verify output: `Hard links: 81/81 synchronized`

---

### Risk 7: Compilation Failure

**Severity**: 🟢 LOW  
**Probability**: 🟢 LOW

**Mitigation**:
- Incremental extraction with per-phase compilation
- Run `dotnet build` after each helper extraction
- Use CSharpier to fix formatting issues

**Verification**:
- [ ] Run: `dotnet build`
- [ ] Verify output: `Build succeeded. 0 Error(s)`
- [ ] Verify no warnings

---

## Appendix A: Commit Message Template

```
refactor: Extract [HelperName] from ManageCIT (Phase X/5)

- Reduces CYC by [N] (current: [X] → target: ≤15)
- Extracted lines [start]-[end]
- Zero logic drift confirmed
- V12 DNA compliance maintained

Part of EPIC-CCN-11 (ManageCIT extraction)
```

**Examples**:

**Phase 1**:
```
refactor: Extract ValidateCitConfiguration from ManageCIT (Phase 1/5)

- Reduces CYC by 5 (current: 26 → 21)
- Extracted lines 70-85
- Zero logic drift confirmed
- V12 DNA compliance maintained

Part of EPIC-CCN-11 (ManageCIT extraction)
```

**Phase 5**:
```
refactor: Extract ExecuteFollowerNudge from ManageCIT (Phase 5/5)

- Reduces CYC by 3 (current: 11 → 8, final refactor → ≤15)
- Extracted lines 130-173
- Zero logic drift confirmed
- V12 DNA compliance maintained
- Build 1109 freeze-proof logic preserved

Part of EPIC-CCN-11 (ManageCIT extraction)
```

---

## Appendix B: PR Description Template

```markdown
## EPIC-CCN-11: ManageCIT Extraction (CYC 26 → ≤15)

**Objective**: Reduce ManageCIT complexity to Jane Street threshold (≤15).

**Changes**:
- Extracted 5 helper methods:
  1. ValidateCitConfiguration (lines 70-85, CYC=5)
  2. CalculateNudgedPrice (lines 123-128, CYC=2)
  3. ShouldChaseOrder (lines 93-114, CYC=7)
  4. ExecuteLocalNudge (lines 174-181, CYC=1)
  5. ExecuteFollowerNudge (lines 130-173, CYC=3)
- Refactored main dispatcher to call helpers
- Added 15+ TDD tests

**Metrics**:
- CYC: 26 → [actual] (target ≤15) ✅
- Nesting: 6 → [actual] (target ≤4) ✅
- LOC: 98 → [actual] (main method)
- Test Coverage: 0% → [actual]% (target ≥80%) ✅

**Verification**:
- ✅ Pre-push validation: 13/13 checks passed
- ✅ F5 verification: BUILD [tag] loaded successfully
- ✅ Risk audit: 9/9 cases passed
- ✅ TDD tests: [N]/[N] passing
- ✅ Manual Sim101 testing: Long + Short entry scenarios verified

**V12 DNA Compliance**:
- ✅ Lock-free (actor pattern preserved)
- ✅ ASCII-only (no Unicode)
- ✅ Zero logic drift (behavior preserved)
- ✅ Correctness by construction (type-safe signatures)

**Critical Business Logic Preserved**:
- ✅ Build 984 directional fix (Long: Low[0], Short: High[0])
- ✅ Build 949 one-shot guard (_citNudgedKeys)
- ✅ Build 1109 freeze-proof budget management
- ✅ Build 966 actor-safe dictionary writes
- ✅ Build 924 propagation suppression

**Related**:
- Forensics: `docs/brain/EPIC-CCN-11/01-managecit-forensics.md`
- Mini-Spec: `docs/brain/EPIC-CCN-11/02-managecit-minispec.md`
- Implementation Plan: `docs/brain/EPIC-CCN-11/03-managecit-implementation-plan.md`

**Closes**: #[issue-number]
```

---

## Appendix C: Risk Audit Cases (9 cases)

**Run after extraction to verify no regressions**:

1. **Case 1: Order Placement**
   - [ ] Place limit order
   - [ ] Verify order appears in entryOrders
   - [ ] Verify order state is Working

2. **Case 2: Contract Sizing**
   - [ ] Place 10 orders (exceed budget)
   - [ ] Verify first 5 nudged
   - [ ] Verify remaining 5 deferred

3. **Case 3: Price Touch Detection**
   - [ ] Place Long limit below market
   - [ ] Wait for Low[0] <= limitPrice
   - [ ] Verify order nudged

4. **Case 4: Symmetry Guard**
   - [ ] Place order, wait for nudge
   - [ ] Wait for next bar
   - [ ] Verify order NOT re-nudged

5. **Case 5: Directional Logic**
   - [ ] Place Long limit, verify Low[0] comparison
   - [ ] Place Short limit, verify High[0] comparison
   - [ ] Verify no instant market conversion

6. **Case 6: Budget Management**
   - [ ] Monitor broker call count
   - [ ] Verify ≤5 calls per cycle
   - [ ] Verify self-enqueue on exhaustion

7. **Case 7: SIMA Broadcast**
   - [ ] Place follower order
   - [ ] Wait for nudge
   - [ ] Verify cancel + resubmit
   - [ ] Verify entryOrders updated

8. **Case 8: Propagation Suppression**
   - [ ] Trigger price-move propagation
   - [ ] Verify CIT suppressed during propagation
   - [ ] Verify log: `[CIT] Suppressed during price-move propagation`

9. **Case 9: Error Handling**
   - [ ] Simulate CreateOrder returning null
   - [ ] Verify error logged
   - [ ] Verify remaining orders processed

---

**End of Implementation Plan**

**Status**: ✅ READY FOR STAGE 3 (DNA & PR Audit)  
**Next Action**: Arena AI adversarial review → Stage 4 (Execution)  
**Estimated Stage 3 Duration**: 15 minutes (DNA compliance verification)