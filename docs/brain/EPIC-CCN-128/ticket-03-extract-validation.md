# Ticket 3: Extract IsTargetOrderReplaceable

**Epic**: EPIC-CCN-128  
**File**: `src/V12_002.Symmetry.Replace.cs`  
**Method**: `SymmetryGuardReplaceExistingFollowerTarget`  
**Priority**: P3 (Depends on Ticket 1, 2)  
**Estimated Time**: 45 minutes

---

## Objective

Extract old target order validation logic into a dedicated helper method to improve readability and testability.

---

## Current State

**Lines**: 54-55, 63-68 (embedded in main method)  
**Logic**: Validates if old target order exists and is in a replaceable state

```csharp
if (!dict.TryGetValue(fleetEntryName, out var oldTarget) || oldTarget == null)
    return;

if (
    oldTarget.OrderState == OrderState.Working
    || oldTarget.OrderState == OrderState.Accepted
    || oldTarget.OrderState == OrderState.Submitted
    || oldTarget.OrderState == OrderState.ChangePending
)
{
    // FSM replace logic...
}
```

---

## Target State

**New Method Signature**:
```csharp
private bool IsTargetOrderReplaceable(
    string fleetEntryName,
    ConcurrentDictionary<string, Order> dict,
    out Order oldTarget
)
```

**Implementation**:
```csharp
private bool IsTargetOrderReplaceable(
    string fleetEntryName,
    ConcurrentDictionary<string, Order> dict,
    out Order oldTarget
)
{
    if (!dict.TryGetValue(fleetEntryName, out oldTarget) || oldTarget == null)
        return false;

    return oldTarget.OrderState == OrderState.Working
        || oldTarget.OrderState == OrderState.Accepted
        || oldTarget.OrderState == OrderState.Submitted
        || oldTarget.OrderState == OrderState.ChangePending;
}
```

**Complexity**: CYC 6 (1 AND + 1 OR + 4 OR conditions)  
**Lines**: 10

---

## Caller Update

**Before**:
```csharp
if (!dict.TryGetValue(fleetEntryName, out var oldTarget) || oldTarget == null)
    return;

if (
    oldTarget.OrderState == OrderState.Working
    || oldTarget.OrderState == OrderState.Accepted
    || oldTarget.OrderState == OrderState.Submitted
    || oldTarget.OrderState == OrderState.ChangePending
)
{
    // FSM replace logic...
}
```

**After**:
```csharp
if (!IsTargetOrderReplaceable(fleetEntryName, dict, out Order oldTarget))
    return;

CreateFollowerTargetReplaceSpec(fleetEntryName, pos, targetNumber, qty, oldTarget, targetTag);
```

---

## Unit Tests

**Test File**: `tests/V12_Performance.Tests/Symmetry/IsTargetOrderReplaceableTests.cs`

```csharp
[Theory]
[InlineData(OrderState.Working, true)]
[InlineData(OrderState.Accepted, true)]
[InlineData(OrderState.Submitted, true)]
[InlineData(OrderState.ChangePending, true)]
[InlineData(OrderState.Filled, false)]
[InlineData(OrderState.Cancelled, false)]
[InlineData(OrderState.Rejected, false)]
[InlineData(OrderState.PartFilled, false)]
public void IsTargetOrderReplaceable_VariousStates_ReturnsExpected(
    OrderState state,
    bool expectedReplaceable
)
{
    // Arrange
    var dict = new ConcurrentDictionary<string, Order>();
    var order = CreateMockOrder(state);
    dict.TryAdd("TEST_ENTRY", order);
    
    // Act
    bool result = _sut.IsTargetOrderReplaceable("TEST_ENTRY", dict, out Order oldTarget);
    
    // Assert
    Assert.Equal(expectedReplaceable, result);
    if (expectedReplaceable)
    {
        Assert.NotNull(oldTarget);
        Assert.Equal(state, oldTarget.OrderState);
    }
}

[Fact]
public void IsTargetOrderReplaceable_NullOrder_ReturnsFalse()
{
    // Arrange
    var dict = new ConcurrentDictionary<string, Order>();
    dict.TryAdd("TEST_ENTRY", null);
    
    // Act
    bool result = _sut.IsTargetOrderReplaceable("TEST_ENTRY", dict, out Order oldTarget);
    
    // Assert
    Assert.False(result);
    Assert.Null(oldTarget);
}

[Fact]
public void IsTargetOrderReplaceable_MissingEntry_ReturnsFalse()
{
    // Arrange
    var dict = new ConcurrentDictionary<string, Order>();
    
    // Act
    bool result = _sut.IsTargetOrderReplaceable("MISSING_ENTRY", dict, out Order oldTarget);
    
    // Assert
    Assert.False(result);
    Assert.Null(oldTarget);
}
```

---

## V12 DNA Compliance

- ✅ **Lock-Free**: No locks introduced (uses ConcurrentDictionary)
- ✅ **ASCII-Only**: No Unicode characters
- ✅ **CYC ≤ 8**: CYC 6 (target: 8)
- ✅ **Correctness by Construction**: Out parameter makes state explicit
- ✅ **Single Responsibility**: Validation logic only

---

## Verification Steps

1. **Extract Method**:
   - Add `IsTargetOrderReplaceable` method to `V12_002.Symmetry.Replace.cs`
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
   dotnet test --filter "FullyQualifiedName~IsTargetOrderReplaceableTests"
   ```

5. **Integration Test**:
   - F5 in NinjaTrader IDE
   - Verify BUILD_TAG appears in output
   - Verify no compilation errors

6. **Complexity Audit**:
   ```powershell
   python scripts/complexity_audit.py --file src/V12_002.Symmetry.Replace.cs
   ```

---

## Success Criteria

- ✅ Method extracted with CYC 6
- ✅ All 10 unit tests pass
- ✅ `deploy-sync.ps1` executes successfully
- ✅ BUILD_TAG verified in NinjaTrader output
- ✅ No compilation errors
- ✅ Zero logic drift (behavior unchanged)

---

## Rollback Plan

If verification fails:
```powershell
git checkout HEAD -- src/V12_002.Symmetry.Replace.cs
powershell -File .\deploy-sync.ps1
```

---

**Status**: PENDING  
**Dependencies**: Ticket 1, 2  
**Blocks**: Ticket 5
