# Ticket 2: Extract CleanupStaleTargetOrder

**Epic**: EPIC-CCN-128  
**File**: `src/V12_002.Symmetry.Replace.cs`  
**Method**: `SymmetryGuardReplaceExistingFollowerTarget`  
**Priority**: P2 (Depends on Ticket 1)  
**Estimated Time**: 45 minutes

---

## Objective

Extract stale target order cleanup logic into a dedicated helper method to improve separation of concerns and testability.

---

## Current State

**Lines**: 40-51 (embedded in main method)  
**Logic**: Cancels and removes stale target orders when skip conditions are met

```csharp
if (isFilled || isRunner || qty <= 0)
{
    if (dict.TryGetValue(fleetEntryName, out var staleTarget) && staleTarget != null)
    {
        if (
            staleTarget.OrderState == OrderState.Working
            || staleTarget.OrderState == OrderState.Accepted
            || staleTarget.OrderState == OrderState.Submitted
            || staleTarget.OrderState == OrderState.ChangePending
        )
        {
            pos.ExecutingAccount.Cancel(new[] { staleTarget });
        }
        dict.TryRemove(fleetEntryName, out _);
    }
    return;
}
```

---

## Target State

**New Method Signature**:
```csharp
private void CleanupStaleTargetOrder(
    string fleetEntryName,
    PositionInfo pos,
    ConcurrentDictionary<string, Order> dict
)
```

**Implementation**:
```csharp
private void CleanupStaleTargetOrder(
    string fleetEntryName,
    PositionInfo pos,
    ConcurrentDictionary<string, Order> dict
)
{
    if (dict.TryGetValue(fleetEntryName, out var staleTarget) && staleTarget != null)
    {
        if (
            staleTarget.OrderState == OrderState.Working
            || staleTarget.OrderState == OrderState.Accepted
            || staleTarget.OrderState == OrderState.Submitted
            || staleTarget.OrderState == OrderState.ChangePending
        )
        {
            pos.ExecutingAccount.Cancel(new[] { staleTarget });
        }
        dict.TryRemove(fleetEntryName, out _);
    }
}
```

**Complexity**: CYC 5 (1 AND + 4 OR conditions)  
**Lines**: 15

---

## Caller Update

**Before**:
```csharp
if (ShouldSkipTargetReplacement(pos, targetNumber, qty, out bool isFilled, out bool isRunner))
{
    if (dict.TryGetValue(fleetEntryName, out var staleTarget) && staleTarget != null)
    {
        if (
            staleTarget.OrderState == OrderState.Working
            || staleTarget.OrderState == OrderState.Accepted
            || staleTarget.OrderState == OrderState.Submitted
            || staleTarget.OrderState == OrderState.ChangePending
        )
        {
            pos.ExecutingAccount.Cancel(new[] { staleTarget });
        }
        dict.TryRemove(fleetEntryName, out _);
    }
    return;
}
```

**After**:
```csharp
if (ShouldSkipTargetReplacement(pos, targetNumber, qty, out bool isFilled, out bool isRunner))
{
    CleanupStaleTargetOrder(fleetEntryName, pos, dict);
    return;
}
```

---

## Unit Tests

**Test File**: `tests/V12_Performance.Tests/Symmetry/CleanupStaleTargetOrderTests.cs`

```csharp
[Fact]
public void CleanupStaleTargetOrder_WorkingOrder_CancelsAndRemoves()
{
    // Arrange
    var dict = new ConcurrentDictionary<string, Order>();
    var order = CreateMockOrder(OrderState.Working);
    dict.TryAdd("TEST_ENTRY", order);
    var pos = CreateMockPosition();
    
    // Act
    _sut.CleanupStaleTargetOrder("TEST_ENTRY", pos, dict);
    
    // Assert
    Assert.False(dict.ContainsKey("TEST_ENTRY"));
    // Verify Cancel was called (mock verification)
}

[Fact]
public void CleanupStaleTargetOrder_AcceptedOrder_CancelsAndRemoves()
{
    // Test Accepted state
}

[Fact]
public void CleanupStaleTargetOrder_SubmittedOrder_CancelsAndRemoves()
{
    // Test Submitted state
}

[Fact]
public void CleanupStaleTargetOrder_ChangePendingOrder_CancelsAndRemoves()
{
    // Test ChangePending state
}

[Fact]
public void CleanupStaleTargetOrder_FilledOrder_RemovesOnly()
{
    // Arrange
    var dict = new ConcurrentDictionary<string, Order>();
    var order = CreateMockOrder(OrderState.Filled);
    dict.TryAdd("TEST_ENTRY", order);
    var pos = CreateMockPosition();
    
    // Act
    _sut.CleanupStaleTargetOrder("TEST_ENTRY", pos, dict);
    
    // Assert
    Assert.False(dict.ContainsKey("TEST_ENTRY"));
    // Verify Cancel was NOT called (mock verification)
}

[Fact]
public void CleanupStaleTargetOrder_CancelledOrder_RemovesOnly()
{
    // Test Cancelled state (no cancel call)
}

[Fact]
public void CleanupStaleTargetOrder_NullOrder_NoAction()
{
    // Arrange
    var dict = new ConcurrentDictionary<string, Order>();
    dict.TryAdd("TEST_ENTRY", null);
    var pos = CreateMockPosition();
    
    // Act
    _sut.CleanupStaleTargetOrder("TEST_ENTRY", pos, dict);
    
    // Assert
    Assert.True(dict.ContainsKey("TEST_ENTRY")); // Not removed
}

[Fact]
public void CleanupStaleTargetOrder_MissingEntry_NoAction()
{
    // Arrange
    var dict = new ConcurrentDictionary<string, Order>();
    var pos = CreateMockPosition();
    
    // Act (should not throw)
    _sut.CleanupStaleTargetOrder("MISSING_ENTRY", pos, dict);
    
    // Assert
    Assert.Empty(dict);
}
```

---

## V12 DNA Compliance

- ✅ **Lock-Free**: No locks introduced (uses ConcurrentDictionary)
- ✅ **ASCII-Only**: No Unicode characters
- ✅ **CYC ≤ 8**: CYC 5 (target: 8)
- ✅ **Correctness by Construction**: Null checks prevent invalid states
- ✅ **Single Responsibility**: Cleanup logic only

---

## Verification Steps

1. **Extract Method**:
   - Add `CleanupStaleTargetOrder` method to `V12_002.Symmetry.Replace.cs`
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
   dotnet test --filter "FullyQualifiedName~CleanupStaleTargetOrderTests"
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

- ✅ Method extracted with CYC 5
- ✅ All 8 unit tests pass
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
**Dependencies**: Ticket 1  
**Blocks**: Ticket 5
