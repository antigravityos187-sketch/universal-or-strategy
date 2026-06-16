# Extraction Tickets: EPIC-CCN-060

## Overview
- **Epic**: EPIC-CCN-060
- **Target Method**: `SweepTrackedOrders`
- **File**: `src/V12_002.SIMA.Lifecycle.cs`
- **Current Complexity**: 12 (CYC)
- **Target Complexity**: ≤8 (Jane Street standard)
- **Total Tickets**: 2
- **Execution Order**: Sequential (TICKET-1 → TICKET-2)
- **Estimated Effort**: 2-3 hours

---

## TICKET-1: Extract Dictionary Selection Logic

### Scope
- **Current Method**: `SweepTrackedOrders`
- **Current CYC**: 12
- **Target CYC After This Ticket**: ~8
- **Extraction**: Dictionary selection logic (ternary operator with 7 vs 1 dictionary)

### Implementation

#### 1. Create Helper Method
Add new private method after `SweepTrackedOrders`:

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private ConcurrentDictionary<string, Order>[] GetOrderDictionariesToSweep(bool force)
{
    return force
        ? new[]
        {
            entryOrders,
            stopOrders,
            targetOrders,
            trailingStopOrders,
            profitTargetOrders,
            stopLossOrders,
            breakEvenOrders
        }
        : new[] { entryOrders };
}
```

#### 2. Refactor Main Method
Replace lines ~1320-1327 (dictionary selection logic):

**Before**:
```csharp
var dictionaries = force
    ? new[]
    {
        entryOrders,
        stopOrders,
        targetOrders,
        trailingStopOrders,
        profitTargetOrders,
        stopLossOrders,
        breakEvenOrders
    }
    : new[] { entryOrders };
```

**After**:
```csharp
var dictionaries = GetOrderDictionariesToSweep(force);
```

#### 3. Add Using Directive (if needed)
Verify `System.Runtime.CompilerServices` is imported for `MethodImpl` attribute.

#### 4. Verification Steps
1. Run `dotnet build` - must succeed with zero errors
2. Run `python scripts/complexity_audit.py` - verify CYC reduction
3. Run `grep -n "lock(" src/V12_002.SIMA.Lifecycle.cs` - verify zero locks
4. Run `grep -P '[^\x00-\x7F]' src/V12_002.SIMA.Lifecycle.cs` - verify ASCII-only
5. Visual inspection: Confirm helper is marked `AggressiveInlining`

### Acceptance Criteria
- [ ] Helper method `GetOrderDictionariesToSweep` created with correct signature
- [ ] Helper marked with `[MethodImpl(MethodImplOptions.AggressiveInlining)]`
- [ ] Main method refactored to call helper (dictionary selection replaced)
- [ ] Method complexity reduced (CYC 12 → ~8)
- [ ] Build succeeds (`dotnet build` returns 0 errors)
- [ ] No lock() statements introduced (grep returns 0 matches)
- [ ] ASCII-only compliance maintained (grep returns 0 matches)
- [ ] No behavioral changes (logic preserved exactly)
- [ ] Diff size < 500 characters (surgical change)

### Dependencies
- None (first ticket in sequence)

### Test Requirements
Add unit test in `tests/V12_Performance.Tests/Core/SweepTrackedOrdersTests.cs`:

```csharp
[TestMethod]
public void GetOrderDictionariesToSweep_ForceTrue_ReturnsSevenDictionaries()
{
    // Arrange
    var sima = CreateTestSIMAInstance();
    
    // Act
    var result = sima.GetOrderDictionariesToSweep(force: true);
    
    // Assert
    Assert.AreEqual(7, result.Length);
}

[TestMethod]
public void GetOrderDictionariesToSweep_ForceFalse_ReturnsOneDictionary()
{
    // Arrange
    var sima = CreateTestSIMAInstance();
    
    // Act
    var result = sima.GetOrderDictionariesToSweep(force: false);
    
    // Assert
    Assert.AreEqual(1, result.Length);
    Assert.AreSame(sima.entryOrders, result[0]);
}
```

---

## TICKET-2: Extract OrderState Validation Logic

### Scope
- **Current Method**: `SweepTrackedOrders`
- **Current CYC After TICKET-1**: ~8
- **Target CYC After This Ticket**: 4
- **Extraction**: OrderState validation (5 OR-ed conditions)

### Implementation

#### 1. Create Helper Method
Add new private method after `GetOrderDictionariesToSweep`:

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private bool IsOrderCancellable(Order ord)
{
    return ord.OrderState == OrderState.Working
        || ord.OrderState == OrderState.Accepted
        || ord.OrderState == OrderState.Submitted
        || ord.OrderState == OrderState.ChangePending
        || ord.OrderState == OrderState.ChangeSubmitted;
}
```

#### 2. Refactor Main Method
Replace lines ~1335-1339 (OrderState validation):

**Before**:
```csharp
if (ord.OrderState != OrderState.Working
    && ord.OrderState != OrderState.Accepted
    && ord.OrderState != OrderState.Submitted
    && ord.OrderState != OrderState.ChangePending
    && ord.OrderState != OrderState.ChangeSubmitted)
{
    continue;
}
```

**After**:
```csharp
if (!IsOrderCancellable(ord))
{
    continue;
}
```

#### 3. Verification Steps
1. Run `dotnet build` - must succeed with zero errors
2. Run `python scripts/complexity_audit.py` - verify CYC ≤4
3. Run `grep -n "lock(" src/V12_002.SIMA.Lifecycle.cs` - verify zero locks
4. Run `grep -P '[^\x00-\x7F]' src/V12_002.SIMA.Lifecycle.cs` - verify ASCII-only
5. Visual inspection: Confirm helper is marked `AggressiveInlining`
6. Compare logic: Verify inverted condition (NOT cancellable → continue)

### Acceptance Criteria
- [ ] Helper method `IsOrderCancellable` created with correct signature
- [ ] Helper marked with `[MethodImpl(MethodImplOptions.AggressiveInlining)]`
- [ ] Main method refactored to call helper (OrderState validation replaced)
- [ ] Logic inverted correctly (NOT cancellable → continue)
- [ ] Method complexity reduced to CYC ≤4 (Jane Street target achieved)
- [ ] Build succeeds (`dotnet build` returns 0 errors)
- [ ] No lock() statements introduced (grep returns 0 matches)
- [ ] ASCII-only compliance maintained (grep returns 0 matches)
- [ ] No behavioral changes (logic preserved exactly)
- [ ] Diff size < 400 characters (surgical change)

### Dependencies
- **TICKET-1 must be completed first**
- Reason: Sequential extraction reduces risk of merge conflicts

### Test Requirements
Add unit tests in `tests/V12_Performance.Tests/Core/SweepTrackedOrdersTests.cs`:

```csharp
[TestMethod]
[DataRow(OrderState.Working, true)]
[DataRow(OrderState.Accepted, true)]
[DataRow(OrderState.Submitted, true)]
[DataRow(OrderState.ChangePending, true)]
[DataRow(OrderState.ChangeSubmitted, true)]
[DataRow(OrderState.Filled, false)]
[DataRow(OrderState.Cancelled, false)]
[DataRow(OrderState.Rejected, false)]
[DataRow(OrderState.PartFilled, false)]
public void IsOrderCancellable_ValidatesOrderState(OrderState state, bool expected)
{
    // Arrange
    var sima = CreateTestSIMAInstance();
    var order = CreateMockOrder(state);
    
    // Act
    var result = sima.IsOrderCancellable(order);
    
    // Assert
    Assert.AreEqual(expected, result);
}
```

---

## Post-Extraction Verification

### Complexity Metrics
| Metric | Before | After TICKET-1 | After TICKET-2 | Target | Status |
|--------|--------|----------------|----------------|--------|--------|
| **Main Method CYC** | 12 | ~8 | 4 | ≤8 | ✅ |
| **Helper 1 CYC** | N/A | 2 | 2 | ≤8 | ✅ |
| **Helper 2 CYC** | N/A | N/A | 3 | ≤8 | ✅ |
| **Max Method CYC** | 12 | ~8 | 4 | ≤8 | ✅ |

### Build & Test Commands
```bash
# 1. Build verification
dotnet build

# 2. Run all tests
dotnet test

# 3. Complexity audit
python scripts/complexity_audit.py

# 4. Lock-free verification
grep -n "lock(" src/V12_002.SIMA.Lifecycle.cs

# 5. ASCII-only verification
grep -P '[^\x00-\x7F]' src/V12_002.SIMA.Lifecycle.cs

# 6. Hard-link sync
powershell -File .\deploy-sync.ps1

# 7. NinjaTrader verification
# Press F5 in NinjaTrader, verify BUILD_TAG
```

### Performance Benchmark (Optional)
Add benchmark in `benchmarks/` to verify zero regression:

```csharp
[Benchmark(Baseline = true)]
public void SweepTrackedOrders_Baseline()
{
    // Measure before extraction (from git history)
}

[Benchmark]
public void SweepTrackedOrders_AfterExtraction()
{
    // Measure after extraction
    // Assert: Difference < 1% (within noise)
}
```

---

## Risk Mitigation

| Risk | Mitigation | Verification |
|------|------------|--------------|
| **Performance regression** | AggressiveInlining hints | Benchmark tests |
| **Logic drift** | Line-by-line comparison | Visual inspection + tests |
| **Null reference exceptions** | Preserve existing null checks | Unit tests with null inputs |
| **Thread safety violation** | No new locks, preserve ToArray() | grep verification |
| **Merge conflicts** | Sequential execution (TICKET-1 → TICKET-2) | Git status check |

---

## Success Criteria (Epic Level)

### Phase 4 Completion Checklist
- [ ] TICKET-1 completed and verified
- [ ] TICKET-2 completed and verified
- [ ] All unit tests added and passing
- [ ] Complexity reduced to CYC ≤4
- [ ] Build succeeds with zero errors
- [ ] Lock-free compliance maintained (0 locks)
- [ ] ASCII-only compliance maintained (0 Unicode)
- [ ] Hard-link sync completed (`deploy-sync.ps1`)
- [ ] NinjaTrader F5 test passed
- [ ] Diff size < 850 characters total
- [ ] No behavioral changes (logic preserved)

### Phase 5 Handoff
After completing both tickets:
1. Run full verification suite (commands above)
2. Update `manifest.json` with Phase 4 completion status
3. Commit changes with message: `feat(EPIC-CCN-060): Extract SweepTrackedOrders helpers (CYC 12→4)`
4. Proceed to Phase 5 (Verification/Review)

---

**Ticket Generation Complete**: Ready for Phase 4 execution via Bob CLI (`v12-engineer`)
