# Ticket 3: Extract Stop Loss Fill Handler

**Epic**: EPIC-CCN-15  
**Priority**: P2 (Depends on Ticket 1)  
**Estimated CYC**: 18 → 8 (56% reduction)  
**File**: `src/V12_002.Orders.Callbacks.Execution.cs`  
**Sub-Extraction**: `CancelTargetOrdersForEntry` (CYC 5)

---

## Objective

Extract stop loss fill handler (lines 302-363) to a new method `HandleStopLossFill`. This method handles stop execution, cancels remaining targets (manual OCO), and cleans up if position fully closed.

---

## Current Code (Lines 302-363)

```csharp
// ============================================================
// 1. STOP LOSS FILL - Manual OCO: Cancel all remaining targets
// ============================================================
if (orderName.StartsWith("Stop_"))
{
    string entryName = extractEntryName(orderName, "Stop_");
    if (
        !string.IsNullOrEmpty(entryName) && activePositions.TryGetValue(entryName, out PositionInfo pos)
    )
    {
        int remainingAfterStop;
        pos.RemainingContracts = Math.Max(0, pos.RemainingContracts - Math.Max(0, quantity));
        remainingAfterStop = pos.RemainingContracts;

        Print(string.Format("STOP FILLED: {0} @ {1:F2}. Cancelling targets.", quantity, price));

        // Manual OCO: Cancel all remaining profit targets immediately
        // V12.1101E [F-07]: Keep target dictionary refs until terminal broker confirmation.
        int cancelledTargets = 0;
        for (int tNum = 1; tNum <= 5; tNum++)
        {
            var tDict = GetTargetOrdersDictionary(tNum);
            if (tDict != null && tDict.TryGetValue(entryName, out var tOrder))
            {
                if (
                    tOrder != null
                    && (
                        tOrder.OrderState == OrderState.Working
                        || tOrder.OrderState == OrderState.Accepted
                    )
                )
                {
                    CancelOrderSafe(tOrder, pos);
                    cancelledTargets++;
                }
            }
        }

        if (cancelledTargets > 0)
        {
            Print(
                string.Format("OCO: Cancelled {0} target orders for {1}", cancelledTargets, entryName)
            );
        }

        // B957/D1: Only remove stopOrders and pendingStopReplacements when position is fully closed.
        // Do NOT remove on partial fills -- the stop may still be tracking residual contracts.
        if (remainingAfterStop <= 0)
        {
            stopOrders.TryRemove(entryName, out _);
            if (pendingStopReplacements.TryRemove(entryName, out _))
                Interlocked.Decrement(ref pendingReplacementCount);
            activePositions.TryRemove(entryName, out _);
            entryOrders.TryRemove(entryName, out _);
        }
        if (remainingAfterStop <= 0)
        {
            SymmetryGuardForgetEntry(entryName);
            Print(string.Format("Position {0} fully closed by stop.", entryName));
        }
    }
}
```

---

## Target Code (New Methods)

### Main Handler Method

**Location**: Insert after `CheckExecutionDeduplication` method

```csharp
/// <summary>
/// V12.CCN-15 [T3]: Handles stop loss fill execution.
/// Reduces position, cancels remaining targets (manual OCO), and cleans up if fully closed.
/// </summary>
/// <param name="orderName">Stop order name (e.g., "Stop_Entry1")</param>
/// <param name="quantity">Fill quantity</param>
/// <param name="price">Fill price</param>
private void HandleStopLossFill(string orderName, int quantity, double price)
{
    string entryName = ExtractEntryNameFromOrder(orderName, "Stop_");
    if (string.IsNullOrEmpty(entryName) || !activePositions.TryGetValue(entryName, out PositionInfo pos))
        return;
    
    // Reduce position
    pos.RemainingContracts = Math.Max(0, pos.RemainingContracts - Math.Max(0, quantity));
    int remainingAfterStop = pos.RemainingContracts;
    
    Print(string.Format("STOP FILLED: {0} @ {1:F2}. Cancelling targets.", quantity, price));
    
    // Manual OCO: Cancel all remaining profit targets
    int cancelledTargets = CancelTargetOrdersForEntry(entryName, pos);
    if (cancelledTargets > 0)
    {
        Print(string.Format("OCO: Cancelled {0} target orders for {1}", cancelledTargets, entryName));
    }
    
    // Cleanup if position fully closed
    // B957/D1: Only remove stopOrders and pendingStopReplacements when position is fully closed.
    if (remainingAfterStop <= 0)
    {
        stopOrders.TryRemove(entryName, out _);
        if (pendingStopReplacements.TryRemove(entryName, out _))
            Interlocked.Decrement(ref pendingReplacementCount);
        activePositions.TryRemove(entryName, out _);
        entryOrders.TryRemove(entryName, out _);
        SymmetryGuardForgetEntry(entryName);
        Print(string.Format("Position {0} fully closed by stop.", entryName));
    }
}
```

### Sub-Extraction: CancelTargetOrdersForEntry

**Location**: Insert after `HandleStopLossFill` method

```csharp
/// <summary>
/// V12.CCN-15 [T3-SUB]: Cancels all working target orders for an entry (manual OCO).
/// V12.1101E [F-07]: Keep target dictionary refs until terminal broker confirmation.
/// </summary>
/// <param name="entryName">Entry name</param>
/// <param name="pos">Position info</param>
/// <returns>Count of cancelled target orders</returns>
private int CancelTargetOrdersForEntry(string entryName, PositionInfo pos)
{
    int cancelledTargets = 0;
    for (int tNum = 1; tNum <= 5; tNum++)
    {
        var tDict = GetTargetOrdersDictionary(tNum);
        if (tDict != null && tDict.TryGetValue(entryName, out var tOrder))
        {
            if (tOrder != null && (tOrder.OrderState == OrderState.Working || tOrder.OrderState == OrderState.Accepted))
            {
                CancelOrderSafe(tOrder, pos);
                cancelledTargets++;
            }
        }
    }
    return cancelledTargets;
}
```

---

## Refactor Site (Replace Inline Logic)

**Location**: Lines 302-363 in `ProcessOnExecutionUpdate`

**Before**:
```csharp
// ============================================================
// 1. STOP LOSS FILL - Manual OCO: Cancel all remaining targets
// ============================================================
if (orderName.StartsWith("Stop_"))
{
    // ... (62 lines of stop fill logic)
}
```

**After**:
```csharp
// V12.CCN-15 [T3]: Stop loss fill handler
if (orderName.StartsWith("Stop_"))
{
    HandleStopLossFill(orderName, quantity, price);
}
```

---

## Implementation Steps

1. **Insert sub-extraction** `CancelTargetOrdersForEntry` first
2. **Insert main handler** `HandleStopLossFill` (calls sub-extraction)
3. **Replace inline logic** at lines 302-363 with method call
4. **Verify ASCII-only**: All string literals use straight quotes
5. **Run complexity audit**: `python scripts/complexity_audit.py`
   - Verify `ProcessOnExecutionUpdate` CYC reduced by ~15
   - Verify `HandleStopLossFill` CYC ≤8
   - Verify `CancelTargetOrdersForEntry` CYC ≤5
6. **Run deploy-sync**: `powershell -File .\deploy-sync.ps1`
7. **Bump BUILD_TAG** in `src/V12_002.cs`

---

## F5 Verification

**Test Scenario**: Hit stop loss, verify targets cancelled

**Setup**:
1. Place entry order (Buy 1 MES @ Market)
2. Entry fills, creates stop + 5 targets
3. Price moves against position
4. Stop fills

**Expected Behavior**:
1. "STOP FILLED: 1 @ [price]. Cancelling targets." log appears
2. "OCO: Cancelled 5 target orders for [entry]" log appears
3. All 5 target orders cancelled
4. "Position [entry] fully closed by stop." log appears
5. Position removed from `activePositions`
6. No errors in log

**Verification Command**:
```
F5 in NinjaTrader → Place entry → Hit stop → Check log for OCO cancellation
```

---

## Success Criteria

- [ ] Sub-extraction `CancelTargetOrdersForEntry` added
- [ ] Main handler `HandleStopLossFill` added
- [ ] Inline logic replaced with method call (lines 302-363)
- [ ] ASCII-only compliance verified (no Unicode)
- [ ] Complexity audit passes:
  - [ ] `ProcessOnExecutionUpdate` CYC reduced by ~15
  - [ ] `HandleStopLossFill` CYC ≤8
  - [ ] `CancelTargetOrdersForEntry` CYC ≤5
- [ ] Deploy-sync passes (ASCII gate)
- [ ] BUILD_TAG bumped
- [ ] F5 verification passes:
  - [ ] "STOP FILLED" log appears
  - [ ] "OCO: Cancelled X targets" log appears
  - [ ] All targets cancelled
  - [ ] Position fully closed
- [ ] Build + tests pass

---

## Risk Assessment

**Risk Level**: HIGH

**Rationale**:
- OCO cancellation loop has race condition potential
- Position cleanup logic is critical (must not leave orphaned orders)
- Stop quantity tracking affects downstream logic

**Mitigation**:
1. **Exact Copy**: Zero logic drift, preserve all comments
2. **Sub-Extraction**: Isolate OCO loop for testability
3. **F5 Verification**: Trigger stop fill scenario
4. **Unit Tests**: Test OCO cancellation with various target states

---

## Unit Tests (Add to Test Suite)

```csharp
[Test]
public void CancelTargetOrdersForEntry_AllWorkingTargets_CancelsAll()
{
    // Arrange
    string entryName = "TestEntry1";
    PositionInfo pos = CreateMockPosition(entryName, 5);
    SetupMockTargets(entryName, 5, OrderState.Working);
    
    // Act
    int cancelledCount = strategy.CancelTargetOrdersForEntry(entryName, pos);
    
    // Assert
    Assert.AreEqual(5, cancelledCount, "Should cancel all 5 working targets");
}

[Test]
public void CancelTargetOrdersForEntry_NoTargets_ReturnsZero()
{
    // Arrange
    string entryName = "TestEntry1";
    PositionInfo pos = CreateMockPosition(entryName, 5);
    // No targets setup
    
    // Act
    int cancelledCount = strategy.CancelTargetOrdersForEntry(entryName, pos);
    
    // Assert
    Assert.AreEqual(0, cancelledCount, "Should return 0 when no targets exist");
}

[Test]
public void CancelTargetOrdersForEntry_MixedStates_CancelsOnlyWorkingAndAccepted()
{
    // Arrange
    string entryName = "TestEntry1";
    PositionInfo pos = CreateMockPosition(entryName, 5);
    SetupMockTargets(entryName, 2, OrderState.Working);
    SetupMockTargets(entryName, 1, OrderState.Accepted);
    SetupMockTargets(entryName, 2, OrderState.Filled); // Should not cancel
    
    // Act
    int cancelledCount = strategy.CancelTargetOrdersForEntry(entryName, pos);
    
    // Assert
    Assert.AreEqual(3, cancelledCount, "Should cancel only Working and Accepted targets");
}
```

---

## Rollback Plan

If F5 verification fails:
1. Revert commit: `git reset --hard HEAD~1`
2. Restore inline stop fill logic
3. Report issue to Director
4. Investigate OCO cancellation or position cleanup bug

---

**Ticket Created**: 2026-06-09T03:39:00Z  
**Assigned To**: Bob Shell (v12-engineer)  
**Status**: Ready for Execution (Depends on Ticket 1)