# Phase 4: Implementation Tickets - EPIC-CCN-117

## Epic Metadata
- **Epic ID**: EPIC-CCN-117
- **Phase**: 4 (Ticket Generation)
- **Target Method**: SyncLimitTarget
- **File**: src/V12_002.Orders.Management.StopSync.cs
- **Current Complexity**: 17
- **Target Complexity**: ≤ 8 (Jane Street HFT standard)
- **Audit Status**: ✅ APPROVED (Phase 3)
- **Date**: 2026-06-14

---

## Execution Overview

**Total Tickets**: 5 (4 extractions + 1 verification)
**Estimated Effort**: 4-6 hours (including testing)
**Risk Level**: LOW-MEDIUM
**Dependencies**: Sequential execution (Ticket 1 → 2 → 3 → 4 → 5)

**Complexity Reduction Path**:
- Start: CYC 17
- After Ticket 1: CYC 7 (removes 10 decision points)
- After Ticket 2: CYC 6 (removes 1 decision point)
- After Ticket 3: CYC 4 (removes 2 decision points)
- After Ticket 4: CYC 3 (removes 1 decision point)
- **Final**: CYC 3-6 (target ≤ 8 achieved)

---

## TICKET-117-1: Extract UpdatePositionTargetPrice (Highest Impact)

### Priority: P1 (CRITICAL - Execute First)

### Objective
Extract duplicate switch statement logic into a single helper method to eliminate 10 decision points.

### Method Signature
```csharp
private void UpdatePositionTargetPrice(
    PositionInfo pos,
    int targetNum,
    double newPrice
)
```

### Current State
- **Lines**: 209-229, 287-307 (duplicate switch statements)
- **Complexity**: +10 decision points (5 cases × 2 occurrences)
- **Issue**: Violates DRY principle, increases maintenance burden

### Extraction Steps

#### Step 1.1: Create Method Stub
**Action**: Add new private method below `SyncLimitTarget` (after line 336)

**Code**:
```csharp
private void UpdatePositionTargetPrice(
    PositionInfo pos,
    int targetNum,
    double newPrice
)
{
    switch (targetNum)
    {
        case 1:
            pos.Target1Price = newPrice;
            break;
        case 2:
            pos.Target2Price = newPrice;
            break;
        case 3:
            pos.Target3Price = newPrice;
            break;
        case 4:
            pos.Target4Price = newPrice;
            break;
        case 5:
            pos.Target5Price = newPrice;
            break;
        default:
            throw new ArgumentOutOfRangeException(
                nameof(targetNum), 
                targetNum, 
                "Target number must be 1-5"
            );
    }
}
```

**Verification**: Build succeeds, no compilation errors

---

#### Step 1.2: Replace First Occurrence (Repricing Branch)
**Action**: Replace lines 209-229 in `SyncLimitTarget`

**Before**:
```csharp
switch (targetNum)
{
    case 1: pos.Target1Price = newPrice; break;
    case 2: pos.Target2Price = newPrice; break;
    case 3: pos.Target3Price = newPrice; break;
    case 4: pos.Target4Price = newPrice; break;
    case 5: pos.Target5Price = newPrice; break;
    default: return;
}
```

**After**:
```csharp
UpdatePositionTargetPrice(pos, targetNum, newPrice);
```

**Verification**: Build succeeds, logic preserved

---

#### Step 1.3: Replace Second Occurrence (Submission Branch)
**Action**: Replace lines 287-307 in `SyncLimitTarget`

**Before**:
```csharp
switch (targetNum)
{
    case 1: pos.Target1Price = newPrice; break;
    case 2: pos.Target2Price = newPrice; break;
    case 3: pos.Target3Price = newPrice; break;
    case 4: pos.Target4Price = newPrice; break;
    case 5: pos.Target5Price = newPrice; break;
    default: return;
}
```

**After**:
```csharp
UpdatePositionTargetPrice(pos, targetNum, newPrice);
```

**Verification**: Build succeeds, duplicate eliminated

---

#### Step 1.4: Write Unit Tests
**Action**: Create test file `tests/V12_Performance.Tests/Orders/SyncLimitTargetTests.cs`

**Tests**:
```csharp
[Test]
public void UpdatePositionTargetPrice_Target1_UpdatesCorrectProperty()
{
    var pos = new PositionInfo { Target1Price = 0 };
    UpdatePositionTargetPrice(pos, 1, 100.50);
    Assert.AreEqual(100.50, pos.Target1Price);
}

[Test]
public void UpdatePositionTargetPrice_AllTargets_UpdatesCorrectProperties()
{
    var pos = new PositionInfo();
    UpdatePositionTargetPrice(pos, 1, 101.0);
    Assert.AreEqual(101.0, pos.Target1Price);
    UpdatePositionTargetPrice(pos, 2, 102.0);
    Assert.AreEqual(102.0, pos.Target2Price);
    UpdatePositionTargetPrice(pos, 3, 103.0);
    Assert.AreEqual(103.0, pos.Target3Price);
    UpdatePositionTargetPrice(pos, 4, 104.0);
    Assert.AreEqual(104.0, pos.Target4Price);
    UpdatePositionTargetPrice(pos, 5, 105.0);
    Assert.AreEqual(105.0, pos.Target5Price);
}

[Test]
public void UpdatePositionTargetPrice_InvalidTargetNumber_ThrowsException()
{
    var pos = new PositionInfo();
    Assert.Throws<ArgumentOutOfRangeException>(() => 
        UpdatePositionTargetPrice(pos, 0, 100.0));
    Assert.Throws<ArgumentOutOfRangeException>(() => 
        UpdatePositionTargetPrice(pos, 6, 100.0));
}
```

**Verification**: All 3 tests pass

---

#### Step 1.5: Checkpoint
**Action**: Commit changes with message: "EPIC-117-T1: Extract UpdatePositionTargetPrice (CYC 17→7)"

**Verification**:
- Build: `dotnet build` succeeds
- Tests: `dotnet test` passes
- Complexity: Run `python scripts/complexity_audit.py` (verify CYC ≤ 7)

---

### Success Criteria
- [ ] Method `UpdatePositionTargetPrice` created
- [ ] Lines 209-229 replaced with method call
- [ ] Lines 287-307 replaced with method call
- [ ] 3 unit tests written and passing
- [ ] Build succeeds (zero errors)
- [ ] Complexity reduced from 17 to 7
- [ ] Checkpoint created

### Rollback Steps
1. Run `/restore` in Bob CLI
2. OR: `git reset --hard HEAD~1`

### Estimated Complexity Reduction
**Before**: CYC 17
**After**: CYC 7
**Reduction**: -10 decision points (59% reduction)

---

## TICKET-117-2: Extract ValidateTargetPrice (Pure Function)

### Priority: P2 (Execute Second)

### Objective
Extract price validation logic into a pure function with no side effects (beyond logging).

### Method Signature
```csharp
private bool ValidateTargetPrice(
    double newPrice,
    int targetNum,
    string entryName
)
```

### Current State
- **Lines**: 188-200
- **Complexity**: +1 decision point (if statement)
- **Issue**: Validation logic embedded in main method

### Extraction Steps

#### Step 2.1: Create Method Stub
**Action**: Add new private method above `SyncLimitTarget` (before line 176)

**Code**:
```csharp
private bool ValidateTargetPrice(
    double newPrice,
    int targetNum,
    string entryName
)
{
    if (newPrice <= 0)
    {
        Print($"[SYNC_ALL] T{targetNum} {entryName}: Calculated price invalid ({newPrice:F2}) -- skipped");
        return false;
    }
    return true;
}
```

**Verification**: Build succeeds

---

#### Step 2.2: Replace Validation Logic
**Action**: Replace lines 188-200 in `SyncLimitTarget`

**Before**:
```csharp
if (newPrice <= 0)
{
    Print($"[SYNC_ALL] T{targetNum} {entryName}: Calculated price invalid ({newPrice:F2}) -- skipped");
    return;
}
```

**After**:
```csharp
if (!ValidateTargetPrice(newPrice, targetNum, entryName))
{
    return;
}
```

**Verification**: Build succeeds, logic preserved

---

#### Step 2.3: Write Unit Tests
**Action**: Add tests to `SyncLimitTargetTests.cs`

**Tests**:
```csharp
[Test]
public void ValidateTargetPrice_ValidPrice_ReturnsTrue()
{
    double validPrice = 100.50;
    bool result = ValidateTargetPrice(validPrice, 1, "TEST_ENTRY");
    Assert.IsTrue(result);
}

[Test]
public void ValidateTargetPrice_ZeroPrice_ReturnsFalse()
{
    double invalidPrice = 0;
    bool result = ValidateTargetPrice(invalidPrice, 1, "TEST_ENTRY");
    Assert.IsFalse(result);
}

[Test]
public void ValidateTargetPrice_NegativePrice_ReturnsFalse()
{
    double invalidPrice = -10.0;
    bool result = ValidateTargetPrice(invalidPrice, 1, "TEST_ENTRY");
    Assert.IsFalse(result);
}
```

**Verification**: All 3 tests pass

---

#### Step 2.4: Checkpoint
**Action**: Commit changes with message: "EPIC-117-T2: Extract ValidateTargetPrice (CYC 7→6)"

**Verification**:
- Build: `dotnet build` succeeds
- Tests: `dotnet test` passes
- Complexity: Run `python scripts/complexity_audit.py` (verify CYC ≤ 6)

---

### Success Criteria
- [ ] Method `ValidateTargetPrice` created
- [ ] Lines 188-200 replaced with method call
- [ ] 3 unit tests written and passing
- [ ] Build succeeds (zero errors)
- [ ] Complexity reduced from 7 to 6
- [ ] Checkpoint created

### Rollback Steps
1. Run `/restore` in Bob CLI
2. OR: `git reset --hard HEAD~1`

### Estimated Complexity Reduction
**Before**: CYC 7
**After**: CYC 6
**Reduction**: -1 decision point (14% reduction)

---

## TICKET-117-3: Extract RepriceExistingLimitOrder (Repricing Branch)

### Priority: P3 (Execute Third)

### Objective
Extract order repricing logic into a dedicated method to isolate order modification behavior.

### Method Signature
```csharp
private bool RepriceExistingLimitOrder(
    Order existingOrder,
    double newPrice,
    PositionInfo pos,
    int targetNum,
    string entryName,
    ref int refreshed
)
```

### Current State
- **Lines**: 204-244
- **Complexity**: +2 decision points (price diff check + try/catch)
- **Issue**: Repricing logic embedded in main method

### Extraction Steps

#### Step 3.1: Create Method Stub
**Action**: Add new private method below `UpdatePositionTargetPrice`

**Code**:
```csharp
private bool RepriceExistingLimitOrder(
    Order existingOrder,
    double newPrice,
    PositionInfo pos,
    int targetNum,
    string entryName,
    ref int refreshed
)
{
    if (Math.Abs(existingOrder.LimitPrice - newPrice) < tickSize)
    {
        Print($"[SYNC_ALL] T{targetNum} {entryName}: Price unchanged at {newPrice:F2} -- no action");
        return false;
    }

    try
    {
        ChangeOrder(existingOrder, existingOrder.Quantity, newPrice, 0);
        UpdatePositionTargetPrice(pos, targetNum, newPrice);
        Print($"[SYNC_ALL] T{targetNum} {entryName}: Repriced -> {newPrice:F2}");
        refreshed++;
        return true;
    }
    catch (Exception ex)
    {
        Print($"[SYNC_ALL] T{targetNum} {entryName}: ChangeOrder failed -- {ex.Message}");
        return false;
    }
}
```

**Verification**: Build succeeds

---

#### Step 3.2: Replace Repricing Logic
**Action**: Replace lines 204-244 in `SyncLimitTarget` with method call

**Verification**: Build succeeds, logic preserved

---

#### Step 3.3: Write Unit Tests
**Action**: Add 3 tests to `SyncLimitTargetTests.cs` (price unchanged, price changed, exception)

**Verification**: All 3 tests pass

---

#### Step 3.4: Checkpoint
**Action**: Commit with message: "EPIC-117-T3: Extract RepriceExistingLimitOrder (CYC 6→4)"

---

### Success Criteria
- [ ] Method `RepriceExistingLimitOrder` created
- [ ] Lines 204-244 replaced with method call
- [ ] 3 unit tests written and passing
- [ ] Build succeeds (zero errors)
- [ ] Complexity reduced from 6 to 4
- [ ] Checkpoint created

### Rollback Steps
1. Run `/restore` in Bob CLI
2. OR: `git reset --hard HEAD~1`

### Estimated Complexity Reduction
**Before**: CYC 6
**After**: CYC 4
**Reduction**: -2 decision points (33% reduction)

---

## TICKET-117-4: Extract SubmitNewLimitOrder (Submission Branch)

### Priority: P4 (Execute Fourth)

### Objective
Extract order submission logic into a dedicated method to isolate order creation behavior.

### Method Signature
```csharp
private bool SubmitNewLimitOrder(
    PositionInfo pos,
    int targetNum,
    int targetQty,
    double newPrice,
    string entryName,
    ConcurrentDictionary<string, Order> targetDict,
    ref int refreshed
)
```

### Current State
- **Lines**: 257-335
- **Complexity**: +1 decision point (ternary operator)
- **Issue**: Submission logic embedded in main method

### Extraction Steps

#### Step 4.1: Create Method Stub
**Action**: Add new private method below `RepriceExistingLimitOrder`

**Code**:
```csharp
private bool SubmitNewLimitOrder(
    PositionInfo pos,
    int targetNum,
    int targetQty,
    double newPrice,
    string entryName,
    ConcurrentDictionary<string, Order> targetDict,
    ref int refreshed
)
{
    try
    {
        Order newLimit = pos.Direction == MarketPosition.Long
            ? SubmitOrderUnmanaged(0, OrderAction.Sell, OrderType.Limit, targetQty, newPrice, 0, "", $"T{targetNum}_{entryName}")
            : SubmitOrderUnmanaged(0, OrderAction.BuyToCover, OrderType.Limit, targetQty, newPrice, 0, "", $"T{targetNum}_{entryName}");

        if (newLimit == null)
        {
            Print($"[SYNC_ALL] T{targetNum} {entryName}: SubmitOrderUnmanaged returned null @ {newPrice:F2}");
            return false;
        }

        targetDict[entryName] = newLimit;
        UpdatePositionTargetPrice(pos, targetNum, newPrice);
        Print($"[SYNC_ALL] T{targetNum} {entryName}: New limit submitted @ {newPrice:F2} qty={targetQty}");
        refreshed++;
        return true;
    }
    catch (Exception ex)
    {
        Print($"[SYNC_ALL] T{targetNum} {entryName}: Submit failed -- {ex.Message}");
        return false;
    }
}
```

**Verification**: Build succeeds

---

#### Step 4.2: Replace Submission Logic
**Action**: Replace lines 257-335 in `SyncLimitTarget` with method call

**Verification**: Build succeeds, logic preserved

---

#### Step 4.3: Write Unit Tests
**Action**: Add 4 tests to `SyncLimitTargetTests.cs` (long position, short position, null return, exception)

**Verification**: All 4 tests pass

---

#### Step 4.4: Checkpoint
**Action**: Commit with message: "EPIC-117-T4: Extract SubmitNewLimitOrder (CYC 4→3)"

---

### Success Criteria
- [ ] Method `SubmitNewLimitOrder` created
- [ ] Lines 257-335 replaced with method call
- [ ] 4 unit tests written and passing
- [ ] Build succeeds (zero errors)
- [ ] Complexity reduced from 4 to 3
- [ ] Checkpoint created

### Rollback Steps
1. Run `/restore` in Bob CLI
2. OR: `git reset --hard HEAD~1`

### Estimated Complexity Reduction
**Before**: CYC 4
**After**: CYC 3
**Reduction**: -1 decision point (25% reduction)

---

## TICKET-117-5: Final Verification & Integration Tests

### Priority: P5 (Execute Last)

### Objective
Verify all extractions are complete, run full quality gates, and create integration tests.

### Verification Steps

#### Step 5.1: Verify Refactored SyncLimitTarget
**Action**: Confirm final method structure (~20 lines, down from 160)

**Expected Code**:
```csharp
private void SyncLimitTarget(
    string entryName,
    PositionInfo pos,
    int targetNum,
    int targetQty,
    ConcurrentDictionary<string, Order> targetDict,
    Order existingOrder,
    bool hasWorkingOrder,
    ref int refreshed
)
{
    double newPrice = CalculateTargetPriceFromPos(pos.Direction, pos.EntryPrice, pos, targetNum);
    
    if (!ValidateTargetPrice(newPrice, targetNum, entryName))
    {
        return;
    }

    if (hasWorkingOrder)
    {
        RepriceExistingLimitOrder(existingOrder, newPrice, pos, targetNum, entryName, ref refreshed);
    }
    else
    {
        SubmitNewLimitOrder(pos, targetNum, targetQty, newPrice, entryName, targetDict, ref refreshed);
    }
}
```

---

#### Step 5.2: Write Integration Tests
**Action**: Create `SyncLimitTargetIntegrationTests.cs` with 3 tests

**Tests**:
1. Valid price + working order → reprices
2. Valid price + no working order → submits new
3. Invalid price → no action

---

#### Step 5.3: Run Build Readiness
**Command**: `powershell -File .\scripts\build_readiness.ps1`

**Expected**: CSharpier check PASS, Build PASS, Tests PASS

---

#### Step 5.4: Run Complexity Audit
**Command**: `python scripts/complexity_audit.py`

**Expected**:
- `SyncLimitTarget`: CYC 3-6
- `ValidateTargetPrice`: CYC 2
- `UpdatePositionTargetPrice`: CYC 6
- `RepriceExistingLimitOrder`: CYC 4
- `SubmitNewLimitOrder`: CYC 5

---

#### Step 5.5: Run Pre-Push Validation
**Command**: `powershell -File .\scripts\pre_push_validation.ps1`

**Expected**: All 13 checks pass (or warnings only)

---

#### Step 5.6: Manual F5 Test
**Action**: Test in NinjaTrader (compile, run on historical data, verify orders)

---

#### Step 5.7: Hard-Link Sync
**Command**: `powershell -File .\deploy-sync.ps1`

**Expected**: Hard links synchronized, DIFF GUARD < 10k, BUILD_TAG updated

---

#### Step 5.8: Final Checkpoint
**Action**: Commit with message: "EPIC-117-T5: Final verification complete (CYC 17→3)"

---

### Success Criteria
- [ ] Refactored `SyncLimitTarget` verified (~20 lines)
- [ ] 3 integration tests written and passing
- [ ] Build readiness: PASS
- [ ] Complexity audit: CYC ≤ 8 (achieved 3-6)
- [ ] Pre-push validation: PASS (13 checks)
- [ ] Manual F5 test: PASS
- [ ] Hard-link sync: PASS
- [ ] Final checkpoint created

### Rollback Steps
1. Run `/restore` in Bob CLI to previous checkpoint
2. OR: `git reset --hard` to last known good commit

### Estimated Complexity Reduction
**Start**: CYC 17
**Final**: CYC 3-6
**Total Reduction**: -11 to -14 decision points (65-82% reduction)

---

## Execution Order & Dependencies

### Sequential Execution Required

**Dependency Chain**:
1. TICKET-117-1 (UpdatePositionTargetPrice) → MUST execute first
2. TICKET-117-2 (ValidateTargetPrice) → Independent
3. TICKET-117-3 (RepriceExistingLimitOrder) → Depends on T1
4. TICKET-117-4 (SubmitNewLimitOrder) → Depends on T1
5. TICKET-117-5 (Final Verification) → Depends on all

**CRITICAL**: Do NOT execute tickets out of order. T3 and T4 depend on T1.

---

## Overall Success Criteria

### Primary Goals
- [ ] **Complexity Reduction**: `SyncLimitTarget` CYC reduced from 17 to ≤ 8
- [ ] **Extracted Methods**: 4 methods created
- [ ] **Each Method CYC ≤ 6**: All extracted methods meet Jane Street standard
- [ ] **Build Success**: Zero compilation errors
- [ ] **Test Coverage**: 13+ unit tests + 3 integration tests
- [ ] **All Tests Pass**: 100% pass rate

### V12 DNA Compliance
- [x] **Lock-Free**: No new locks introduced
- [x] **ASCII-Only**: No Unicode in extracted code
- [x] **Correctness by Construction**: Type-level validation (ArgumentOutOfRangeException)
- [x] **Cognitive Simplicity**: Each method has single responsibility
- [x] **DRY**: Duplicate switch statements eliminated

### Quality Gates
- [ ] **Pre-Push Validation**: All 13 checks pass
- [ ] **CSharpier**: Zero formatting issues
- [ ] **Codacy**: No new complexity violations
- [ ] **Build**: `dotnet build` succeeds
- [ ] **Tests**: All unit + integration tests pass
- [ ] **Hard-Link Sync**: `deploy-sync.ps1` succeeds
- [ ] **Manual Verification**: F5 test in NinjaTrader

---

## Risk Mitigation

### Checkpoint Strategy
- **After Each Ticket**: Commit with descriptive message
- **Rollback Available**: `/restore` in Bob CLI or `git reset --hard`
- **Incremental Verification**: Build + test after each step

### Test-Driven Development
- **Write Tests First**: Before each extraction
- **Verify Behavior**: Tests confirm logic preserved
- **Regression Prevention**: Catch issues immediately

### Quality Gates
- **Build Verification**: After each ticket
- **Complexity Audit**: After each ticket
- **Pre-Push Validation**: Before final commit
- **Manual F5 Test**: Before sign-off

---

## Estimated Effort

### Time Breakdown
- **TICKET-117-1**: 1.5 hours (highest complexity)
- **TICKET-117-2**: 0.5 hours (simplest)
- **TICKET-117-3**: 1.0 hours (moderate complexity)
- **TICKET-117-4**: 1.0 hours (moderate complexity)
- **TICKET-117-5**: 1.0 hours (verification + integration tests)
- **Total**: 5.0 hours

### Complexity Reduction Timeline
- **Start**: CYC 17
- **After T1**: CYC 7 (1.5 hours)
- **After T2**: CYC 6 (2.0 hours)
- **After T3**: CYC 4 (3.0 hours)
- **After T4**: CYC 3 (4.0 hours)
- **After T5**: CYC 3-6 verified (5.0 hours)

---

**Phase 4 Status**: ✅ COMPLETE
**Next Phase**: Phase 5 (Execution)
**Approval Required**: Director sign-off
**Estimated Execution Time**: 5.0 hours
**Risk Level**: LOW-MEDIUM (acceptable)
