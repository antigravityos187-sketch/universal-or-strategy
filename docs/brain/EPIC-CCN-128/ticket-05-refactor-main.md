# Ticket 5: Refactor Main Method

**Epic**: EPIC-CCN-128  
**File**: `src/V12_002.Symmetry.Replace.cs`  
**Method**: `SymmetryGuardReplaceExistingFollowerTarget`  
**Priority**: P5 (Depends on Ticket 1, 2, 3, 4)  
**Estimated Time**: 30 minutes

---

## Objective

Refactor the main method to use all extracted helpers, reducing cyclomatic complexity from 18 to 3.

---

## Current State

**Lines**: 28-91 (64 lines)  
**CYC**: 18  
**Nesting Depth**: 4

```csharp
private void SymmetryGuardReplaceExistingFollowerTarget(
    string fleetEntryName,
    PositionInfo pos,
    int targetNumber,
    ConcurrentDictionary<string, Order> dict
)
{
    if (pos.ExecutingAccount == null)
        return;
    string targetTag = "T" + targetNumber;
    bool isRunner = IsRunnerTarget(targetNumber);
    bool isFilled = IsTargetFilled(pos, targetNumber);
    int qty = GetTargetContracts(pos, targetNumber);

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

    if (!dict.TryGetValue(fleetEntryName, out var oldTarget) || oldTarget == null)
        return;

    if (
        oldTarget.OrderState == OrderState.Working
        || oldTarget.OrderState == OrderState.Accepted
        || oldTarget.OrderState == OrderState.Submitted
        || oldTarget.OrderState == OrderState.ChangePending
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
}
```

---

## Target State

**New Lines**: ~18 (72% reduction)  
**New CYC**: 3 (83% reduction)  
**New Nesting Depth**: 1

```csharp
private void SymmetryGuardReplaceExistingFollowerTarget(
    string fleetEntryName,
    PositionInfo pos,
    int targetNumber,
    ConcurrentDictionary<string, Order> dict
)
{
    if (pos.ExecutingAccount == null)
        return;

    string targetTag = "T" + targetNumber;
    int qty = GetTargetContracts(pos, targetNumber);

    if (ShouldSkipTargetReplacement(pos, targetNumber, qty, out bool isFilled, out bool isRunner))
    {
        CleanupStaleTargetOrder(fleetEntryName, pos, dict);
        return;
    }

    if (!IsTargetOrderReplaceable(fleetEntryName, dict, out Order oldTarget))
        return;

    CreateFollowerTargetReplaceSpec(fleetEntryName, pos, targetNumber, qty, oldTarget, targetTag);
}
```

---

## Changes Summary

### Removed Lines
- Lines 33-36: Variable setup (moved to helpers)
- Lines 38-52: Skip condition check + cleanup (now `ShouldSkipTargetReplacement` + `CleanupStaleTargetOrder`)
- Lines 54-55: Old target validation (now `IsTargetOrderReplaceable`)
- Lines 57-89: FSM replace logic (now `CreateFollowerTargetReplaceSpec`)

### Added Lines
- Line 14: Call to `ShouldSkipTargetReplacement`
- Line 16: Call to `CleanupStaleTargetOrder`
- Line 20: Call to `IsTargetOrderReplaceable`
- Line 23: Call to `CreateFollowerTargetReplaceSpec`

### Preserved Lines
- Line 30: Early exit guard (`pos.ExecutingAccount == null`)
- Line 33: Target tag setup
- Line 34: Quantity retrieval

---

## Integration Tests

**Test File**: `tests/V12_Performance.Tests/Symmetry/SymmetryGuardReplaceIntegrationTests.cs`

```csharp
[Fact]
public void SymmetryGuardReplaceExistingFollowerTarget_FilledTarget_CleansUpStale()
{
    // Arrange
    var pos = CreateMockPosition();
    var dict = new ConcurrentDictionary<string, Order>();
    var staleOrder = CreateMockOrder(OrderState.Working);
    dict.TryAdd("TEST_ENTRY", staleOrder);
    
    // Mock IsTargetFilled to return true
    
    // Act
    _sut.SymmetryGuardReplaceExistingFollowerTarget("TEST_ENTRY", pos, 1, dict);
    
    // Assert
    Assert.False(dict.ContainsKey("TEST_ENTRY")); // Stale order removed
    // Verify Cancel was called
}

[Fact]
public void SymmetryGuardReplaceExistingFollowerTarget_RunnerTarget_CleansUpStale()
{
    // Test runner target cleanup
}

[Fact]
public void SymmetryGuardReplaceExistingFollowerTarget_ZeroQuantity_CleansUpStale()
{
    // Test zero quantity cleanup
}

[Fact]
public void SymmetryGuardReplaceExistingFollowerTarget_NoOldTarget_ReturnsEarly()
{
    // Arrange
    var pos = CreateMockPosition();
    var dict = new ConcurrentDictionary<string, Order>();
    
    // Act
    _sut.SymmetryGuardReplaceExistingFollowerTarget("MISSING_ENTRY", pos, 1, dict);
    
    // Assert
    // Verify no FSM spec created
    // Verify no cancel called
}

[Fact]
public void SymmetryGuardReplaceExistingFollowerTarget_FilledOldTarget_ReturnsEarly()
{
    // Test filled old target (not replaceable)
}

[Fact]
public void SymmetryGuardReplaceExistingFollowerTarget_ValidInputs_CreatesSpecAndCancels()
{
    // Arrange
    var pos = CreateMockPosition(MarketPosition.Long);
    var dict = new ConcurrentDictionary<string, Order>();
    var oldTarget = CreateMockOrder(OrderState.Working);
    dict.TryAdd("TEST_ENTRY", oldTarget);
    
    // Mock GetTargetPrice to return valid price
    
    // Act
    _sut.SymmetryGuardReplaceExistingFollowerTarget("TEST_ENTRY", pos, 1, dict);
    
    // Assert
    // Verify FSM spec created
    // Verify StampReaperMoveGrace called
    // Verify Cancel called with oldTarget
}

[Fact]
public void SymmetryGuardReplaceExistingFollowerTarget_NullAccount_ReturnsEarly()
{
    // Arrange
    var pos = CreateMockPosition();
    pos.ExecutingAccount = null;
    var dict = new ConcurrentDictionary<string, Order>();
    
    // Act
    _sut.SymmetryGuardReplaceExistingFollowerTarget("TEST_ENTRY", pos, 1, dict);
    
    // Assert
    // Verify no action taken
}
```

---

## V12 DNA Compliance

- ✅ **Lock-Free**: No locks introduced
- ✅ **ASCII-Only**: No Unicode characters
- ✅ **CYC ≤ 8**: CYC 3 (target: 8)
- ✅ **Correctness by Construction**: Helper contracts prevent illegal states
- ✅ **Single Responsibility**: Orchestration only (delegates to helpers)

---

## Verification Steps

1. **Refactor Method**:
   - Replace main method body with helper calls
   - Preserve exact behavior (zero drift)
   - Verify all 4 helpers are called correctly

2. **Build**:
   ```powershell
   dotnet build
   ```

3. **Deploy**:
   ```powershell
   powershell -File .\deploy-sync.ps1
   ```

4. **Unit Tests** (All Tickets):
   ```powershell
   dotnet test --filter "FullyQualifiedName~Symmetry"
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
   - Verify main method CYC = 3
   - Verify all helpers CYC ≤ 8

7. **Pre-Push Validation**:
   ```powershell
   powershell -File .\scripts\pre_push_validation.ps1 -Fast
   ```

---

## Success Criteria

- ✅ Main method refactored with CYC 3
- ✅ All 7 integration tests pass
- ✅ All unit tests pass (Tickets 1-4)
- ✅ `deploy-sync.ps1` executes successfully
- ✅ BUILD_TAG verified in NinjaTrader output
- ✅ No compilation errors
- ✅ Zero logic drift (behavior unchanged)
- ✅ Pre-push validation passes (all 13 checks)

---

## Rollback Plan

If verification fails:
```powershell
git checkout HEAD -- src/V12_002.Symmetry.Replace.cs
powershell -File .\deploy-sync.ps1
```

---

## Epic Completion Checklist

After Ticket 5 success:

- [ ] Update `docs/brain/EPIC-CCN-128/manifest.json` (Phase 5 complete)
- [ ] Generate `05-completion-report.md`
- [ ] Update `src/AGENTS.md` "Recent Major Refactors" table
- [ ] Commit with message: `[EPIC-CCN-128] Complete: SymmetryGuardReplaceExistingFollowerTarget CYC 18→3 [BUILD_TAG]`
- [ ] Push to branch: `git push origin feature/src-epic-128`
- [ ] Create PR with title: `EPIC-CCN-128: Reduce SymmetryGuardReplaceExistingFollowerTarget complexity (CYC 18→3)`

---

**Status**: PENDING  
**Dependencies**: Ticket 1, 2, 3, 4  
**Blocks**: Epic completion
