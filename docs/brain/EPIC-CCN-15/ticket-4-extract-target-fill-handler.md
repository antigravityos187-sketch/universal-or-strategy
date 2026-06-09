# Ticket 4: Extract Target Fill Handler

**Epic**: EPIC-CCN-15  
**Priority**: P3 (Depends on Ticket 1)  
**Estimated CYC**: 20 → 8 (60% reduction)  
**File**: `src/V12_002.Orders.Callbacks.Execution.cs`  
**Sub-Extraction**: `CleanupTerminalTargetFill` (CYC 3)

---

## Objective

Extract target fill handler (lines 364-449) to a new method `HandleTargetFill`. This method handles T1-T5 target execution, reduces stop quantity, and cleans up if position fully closed.

---

## Current Code (Lines 364-449)

```csharp
// ============================================================
// 2. TARGET 1-5 FILL - Reduce stop quantity (unified loop)
// V12.1101E [SK-01/A-1]: First-Writer-Wins guard prevents double-decrement.
// ============================================================
else if (
    orderName.StartsWith("T1_")
    || orderName.StartsWith("T2_")
    || orderName.StartsWith("T3_")
    || orderName.StartsWith("T4_")
    || orderName.StartsWith("T5_")
)
{
    // Extract target number from prefix (T1_, T2_, etc.)
    int targetNum = orderName[1] - '0';
    string targetPrefix = "T" + targetNum + "_";
    string entryName = extractEntryName(orderName, targetPrefix);

    if (
        !string.IsNullOrEmpty(entryName) && activePositions.TryGetValue(entryName, out PositionInfo pos)
    )
    {
        bool terminalFill = execution.Order.OrderState == OrderState.Filled;
        bool alreadyProcessed;
        int appliedQty;
        int remainingAfter;
        ApplyTargetFill(
            pos,
            targetNum,
            quantity,
            terminalFill,
            out alreadyProcessed,
            out appliedQty,
            out remainingAfter
        );
        if (alreadyProcessed)
        {
            Print(
                string.Format(
                    "[1101E GUARD] T{0} already processed for {1} -- skipping duplicate OnExecutionUpdate fill",
                    targetNum,
                    entryName
                )
            );
            if (terminalFill)
            {
                var tDict = GetTargetOrdersDictionary(targetNum);
                if (tDict != null)
                    tDict.TryRemove(entryName, out _);
            }
            return;
        }

        Print(
            string.Format(
                "TARGET FILLED: {0} @ {1:F2}. Reducing stop. Remaining: {2}",
                appliedQty,
                price,
                remainingAfter
            )
        );

        if (remainingAfter > 0)
        {
            UpdateStopQuantity(entryName, pos);
        }
        else
        {
            // Position fully closed, cancel stop
            // A2-2: Defer activePositions.TryRemove to broker-confirmed stop terminal state (Build 960)
            RequestStopCancelLifecycleSafe(entryName);
            PositionInfo closedPos;
            if (activePositions.TryGetValue(entryName, out closedPos) && closedPos != null)
                closedPos.PendingCleanup = true; // B957/A: stateLock guards PositionInfo field writes
            else
                SymmetryGuardForgetEntry(entryName); // already gone -- clean up now
        }

        // V12.1101E [F-07]: Clear target ref only after broker confirms Filled.
        if (terminalFill)
        {
            var tDict = GetTargetOrdersDictionary(targetNum);
            if (tDict != null)
                tDict.TryRemove(entryName, out _);
        }
    }
}
```

---

## Target Code (New Methods)

### Main Handler Method

**Location**: Insert after `CancelTargetOrdersForEntry` method

```csharp
/// <summary>
/// V12.CCN-15 [T4]: Handles target (T1-T5) fill execution.
/// Reduces stop quantity, updates position, and cleans up if fully closed.
/// V12.1101E [SK-01/A-1]: First-Writer-Wins guard prevents double-decrement.
/// </summary>
/// <param name="orderName">Target order name (e.g., "T1_Entry1")</param>
/// <param name="quantity">Fill quantity</param>
/// <param name="price">Fill price</param>
/// <param name="execution">Execution object for terminal state check</param>
private void HandleTargetFill(string orderName, int quantity, double price, Execution execution)
{
    // Extract target number from prefix (T1_, T2_, etc.)
    int targetNum = orderName[1] - '0';
    string targetPrefix = "T" + targetNum + "_";
    string entryName = ExtractEntryNameFromOrder(orderName, targetPrefix);
    
    if (string.IsNullOrEmpty(entryName) || !activePositions.TryGetValue(entryName, out PositionInfo pos))
        return;
    
    bool terminalFill = execution.Order.OrderState == OrderState.Filled;
    
    // Apply fill with First-Writer-Wins guard
    bool alreadyProcessed;
    int appliedQty;
    int remainingAfter;
    ApplyTargetFill(pos, targetNum, quantity, terminalFill, out alreadyProcessed, out appliedQty, out remainingAfter);
    
    if (alreadyProcessed)
    {
        Print(string.Format("[1101E GUARD] T{0} already processed for {1} -- skipping duplicate OnExecutionUpdate fill", targetNum, entryName));
        CleanupTerminalTargetFill(entryName, targetNum, terminalFill);
        return;
    }
    
    Print(string.Format("TARGET FILLED: {0} @ {1:F2}. Reducing stop. Remaining: {2}", appliedQty, price, remainingAfter));
    
    // Update stop or cancel if fully closed
    if (remainingAfter > 0)
    {
        UpdateStopQuantity(entryName, pos);
    }
    else
    {
        // Position fully closed, cancel stop
        // A2-2: Defer activePositions.TryRemove to broker-confirmed stop terminal state (Build 960)
        RequestStopCancelLifecycleSafe(entryName);
        PositionInfo closedPos;
        if (activePositions.TryGetValue(entryName, out closedPos) && closedPos != null)
            closedPos.PendingCleanup = true; // B957/A: stateLock guards PositionInfo field writes
        else
            SymmetryGuardForgetEntry(entryName); // already gone -- clean up now
    }
    
    // V12.1101E [F-07]: Clear target ref only after broker confirms Filled
    CleanupTerminalTargetFill(entryName, targetNum, terminalFill);
}
```

### Sub-Extraction: CleanupTerminalTargetFill

**Location**: Insert after `HandleTargetFill` method

```csharp
/// <summary>
/// V12.CCN-15 [T4-SUB]: Cleans up target order reference after terminal fill.
/// V12.1101E [F-07]: Clear target ref only after broker confirms Filled.
/// </summary>
/// <param name="entryName">Entry name</param>
/// <param name="targetNum">Target number (1-5)</param>
/// <param name="terminalFill">True if order reached Filled state</param>
private void CleanupTerminalTargetFill(string entryName, int targetNum, bool terminalFill)
{
    if (terminalFill)
    {
        var tDict = GetTargetOrdersDictionary(targetNum);
        if (tDict != null)
            tDict.TryRemove(entryName, out _);
    }
}
```

---

## Refactor Site (Replace Inline Logic)

**Location**: Lines 364-449 in `ProcessOnExecutionUpdate`

**Before**:
```csharp
// ============================================================
// 2. TARGET 1-5 FILL - Reduce stop quantity (unified loop)
// V12.1101E [SK-01/A-1]: First-Writer-Wins guard prevents double-decrement.
// ============================================================
else if (
    orderName.StartsWith("T1_")
    || orderName.StartsWith("T2_")
    || orderName.StartsWith("T3_")
    || orderName.StartsWith("T4_")
    || orderName.StartsWith("T5_")
)
{
    // ... (86 lines of target fill logic)
}
```

**After**:
```csharp
// V12.CCN-15 [T4]: Target fill handler (T1-T5)
else if (orderName.StartsWith("T1_") || orderName.StartsWith("T2_") || orderName.StartsWith("T3_") || orderName.StartsWith("T4_") || orderName.StartsWith("T5_"))
{
    HandleTargetFill(orderName, quantity, price, execution);
}
```

---

## Implementation Steps

1. **Insert sub-extraction** `CleanupTerminalTargetFill` first
2. **Insert main handler** `HandleTargetFill` (calls sub-extraction)
3. **Replace inline logic** at lines 364-449 with method call
4. **Verify ASCII-only**: All string literals use straight quotes
5. **Run complexity audit**: `python scripts/complexity_audit.py`
   - Verify `ProcessOnExecutionUpdate` CYC reduced by ~17
   - Verify `HandleTargetFill` CYC ≤8
   - Verify `CleanupTerminalTargetFill` CYC ≤3
6. **Run deploy-sync**: `powershell -File .\deploy-sync.ps1`
7. **Bump BUILD_TAG** in `src/V12_002.cs`

---

## F5 Verification

**Test Scenario**: Hit T1 target, verify stop quantity reduced

**Setup**:
1. Place entry order (Buy 1 MES @ Market)
2. Entry fills, creates stop + 5 targets
3. Price moves in favor
4. T1 target fills

**Expected Behavior**:
1. "TARGET FILLED: 1 @ [price]. Reducing stop. Remaining: 0" log appears
2. Stop order quantity reduced (or cancelled if fully closed)
3. Position updated correctly
4. No "[1101E GUARD]" log (no duplicate processing)
5. No errors in log

**Verification Command**:
```
F5 in NinjaTrader → Place entry → Hit T1 → Check log for "TARGET FILLED"
```

---

## Success Criteria

- [ ] Sub-extraction `CleanupTerminalTargetFill` added
- [ ] Main handler `HandleTargetFill` added
- [ ] Inline logic replaced with method call (lines 364-449)
- [ ] ASCII-only compliance verified (no Unicode)
- [ ] Complexity audit passes:
  - [ ] `ProcessOnExecutionUpdate` CYC reduced by ~17
  - [ ] `HandleTargetFill` CYC ≤8
  - [ ] `CleanupTerminalTargetFill` CYC ≤3
- [ ] Deploy-sync passes (ASCII gate)
- [ ] BUILD_TAG bumped
- [ ] F5 verification passes:
  - [ ] "TARGET FILLED" log appears
  - [ ] Stop quantity reduced correctly
  - [ ] No duplicate processing
- [ ] Build + tests pass

---

## Risk Assessment

**Risk Level**: HIGH

**Rationale**:
- First-Writer-Wins guard is critical (prevents double-decrement)
- Stop quantity update affects position tracking
- Terminal cleanup timing is subtle (must wait for broker confirmation)

**Mitigation**:
1. **Exact Copy**: Zero logic drift, preserve all comments
2. **Sub-Extraction**: Isolate terminal cleanup for clarity
3. **F5 Verification**: Trigger target fill scenario
4. **Unit Tests**: Test First-Writer-Wins guard

---

## Unit Tests (Add to Test Suite)

```csharp
[Test]
public void CleanupTerminalTargetFill_TerminalFill_RemovesTargetRef()
{
    // Arrange
    string entryName = "TestEntry1";
    int targetNum = 1;
    bool terminalFill = true;
    SetupMockTarget(entryName, targetNum, OrderState.Filled);
    
    // Act
    strategy.CleanupTerminalTargetFill(entryName, targetNum, terminalFill);
    
    // Assert
    var tDict = strategy.GetTargetOrdersDictionary(targetNum);
    Assert.IsFalse(tDict.ContainsKey(entryName), "Target ref should be removed");
}

[Test]
public void CleanupTerminalTargetFill_NonTerminalFill_KeepsTargetRef()
{
    // Arrange
    string entryName = "TestEntry1";
    int targetNum = 1;
    bool terminalFill = false;
    SetupMockTarget(entryName, targetNum, OrderState.PartiallyFilled);
    
    // Act
    strategy.CleanupTerminalTargetFill(entryName, targetNum, terminalFill);
    
    // Assert
    var tDict = strategy.GetTargetOrdersDictionary(targetNum);
    Assert.IsTrue(tDict.ContainsKey(entryName), "Target ref should remain");
}
```

---

## Rollback Plan

If F5 verification fails:
1. Revert commit: `git reset --hard HEAD~1`
2. Restore inline target fill logic
3. Report issue to Director
4. Investigate First-Writer-Wins guard or stop quantity bug

---

**Ticket Created**: 2026-06-09T03:40:00Z  
**Assigned To**: Bob Shell (v12-engineer)  
**Status**: Ready for Execution (Depends on Ticket 1)