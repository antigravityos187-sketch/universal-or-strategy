# Ticket 5: Extract Trim Execution Handler

**Epic**: EPIC-CCN-15  
**Priority**: P4 (Depends on Ticket 1)  
**Estimated CYC**: 15 → 8 (47% reduction)  
**File**: `src/V12_002.Orders.Callbacks.Execution.cs`

---

## Objective

Extract trim execution handler (lines 450-516) to a new method `HandleTrimExecution`. This method handles trim order execution (partial position close), reduces stop quantity to prevent reverse position, and cleans up if fully closed.

---

## Current Code (Lines 450-516)

```csharp
// ============================================================
// 5. TRIM EXECUTION - V10.3.1: Enhanced Stop Integrity
// ============================================================
// (!) CRITICAL: When a TRIM executes, we MUST reduce the stop order quantity
// to match the new position size. If we don't, hitting the stop after a trim
// would close more contracts than we hold, creating an unintended REVERSE position.
// Example: Long 4 contracts, stop at 4. Trim 2 (now Long 2). If stop stays at 4,
// getting stopped out would SELL 4 (close 2 + go SHORT 2) = DISASTER.
else if (orderName.StartsWith("Trim_"))
{
    string entryName = extractEntryName(orderName, "Trim_");
    if (
        !string.IsNullOrEmpty(entryName) && activePositions.TryGetValue(entryName, out PositionInfo pos)
    )
    {
        int previousQty;
        int remainingAfterTrim;
        previousQty = pos.RemainingContracts;
        pos.RemainingContracts = Math.Max(0, pos.RemainingContracts - Math.Max(0, quantity));
        remainingAfterTrim = pos.RemainingContracts;

        Print(
            string.Format(
                "TRIM EXECUTION: {0} contracts closed for {1}. Position: {2} -> {3}",
                quantity,
                entryName,
                previousQty,
                remainingAfterTrim
            )
        );

        // V10.3.1 FIX: MANDATORY stop quantity reduction to prevent reverse position
        if (remainingAfterTrim > 0)
        {
            Print(
                string.Format(
                    "STOP INTEGRITY: Reducing stop quantity from {0} to {1} for {2}",
                    previousQty,
                    remainingAfterTrim,
                    entryName
                )
            );
            UpdateStopQuantity(entryName, pos);
        }
        else
        {
            // Position fully closed by trim, cancel stop
            Print(
                string.Format("TRIM FLATTEN: Position {0} fully closed. Cancelling stop.", entryName)
            );
            // A2-2: Defer activePositions.TryRemove to broker-confirmed stop terminal state (Build 960)
            RequestStopCancelLifecycleSafe(entryName);

            // Also clean up any pending replacements
            if (pendingStopReplacements.TryRemove(entryName, out _))
            {
                Interlocked.Decrement(ref pendingReplacementCount);
            }

            PositionInfo trimPos;
            if (activePositions.TryGetValue(entryName, out trimPos) && trimPos != null)
                trimPos.PendingCleanup = true; // B957/A: stateLock guards PositionInfo field writes
            else
                SymmetryGuardForgetEntry(entryName); // already gone -- clean up now
        }
    }
}
```

---

## Target Code (New Method)

**Location**: Insert after `CleanupTerminalTargetFill` method

```csharp
/// <summary>
/// V12.CCN-15 [T5]: Handles trim execution (partial position close).
/// CRITICAL: Reduces stop quantity to prevent reverse position on stop-out.
/// V10.3.1: Enhanced Stop Integrity - prevents unintended reverse position.
/// Example: Long 4 contracts, stop at 4. Trim 2 (now Long 2). If stop stays at 4,
/// getting stopped out would SELL 4 (close 2 + go SHORT 2) = DISASTER.
/// </summary>
/// <param name="orderName">Trim order name (e.g., "Trim_Entry1")</param>
/// <param name="quantity">Trim quantity</param>
/// <param name="price">Trim price</param>
private void HandleTrimExecution(string orderName, int quantity, double price)
{
    string entryName = ExtractEntryNameFromOrder(orderName, "Trim_");
    if (string.IsNullOrEmpty(entryName) || !activePositions.TryGetValue(entryName, out PositionInfo pos))
        return;
    
    // Reduce position
    int previousQty = pos.RemainingContracts;
    pos.RemainingContracts = Math.Max(0, pos.RemainingContracts - Math.Max(0, quantity));
    int remainingAfterTrim = pos.RemainingContracts;
    
    Print(string.Format("TRIM EXECUTION: {0} contracts closed for {1}. Position: {2} -> {3}", quantity, entryName, previousQty, remainingAfterTrim));
    
    // V10.3.1 FIX: MANDATORY stop quantity reduction to prevent reverse position
    if (remainingAfterTrim > 0)
    {
        Print(string.Format("STOP INTEGRITY: Reducing stop quantity from {0} to {1} for {2}", previousQty, remainingAfterTrim, entryName));
        UpdateStopQuantity(entryName, pos);
    }
    else
    {
        // Position fully closed by trim
        Print(string.Format("TRIM FLATTEN: Position {0} fully closed. Cancelling stop.", entryName));
        // A2-2: Defer activePositions.TryRemove to broker-confirmed stop terminal state (Build 960)
        RequestStopCancelLifecycleSafe(entryName);
        
        // Also clean up any pending replacements
        if (pendingStopReplacements.TryRemove(entryName, out _))
            Interlocked.Decrement(ref pendingReplacementCount);
        
        PositionInfo trimPos;
        if (activePositions.TryGetValue(entryName, out trimPos) && trimPos != null)
            trimPos.PendingCleanup = true; // B957/A: stateLock guards PositionInfo field writes
        else
            SymmetryGuardForgetEntry(entryName); // already gone -- clean up now
    }
}
```

---

## Refactor Site (Replace Inline Logic)

**Location**: Lines 450-516 in `ProcessOnExecutionUpdate`

**Before**:
```csharp
// ============================================================
// 5. TRIM EXECUTION - V10.3.1: Enhanced Stop Integrity
// ============================================================
// (!) CRITICAL: When a TRIM executes, we MUST reduce the stop order quantity
// to match the new position size. If we don't, hitting the stop after a trim
// would close more contracts than we hold, creating an unintended REVERSE position.
// Example: Long 4 contracts, stop at 4. Trim 2 (now Long 2). If stop stays at 4,
// getting stopped out would SELL 4 (close 2 + go SHORT 2) = DISASTER.
else if (orderName.StartsWith("Trim_"))
{
    // ... (67 lines of trim logic)
}
```

**After**:
```csharp
// V12.CCN-15 [T5]: Trim execution handler
else if (orderName.StartsWith("Trim_"))
{
    HandleTrimExecution(orderName, quantity, price);
}
```

---

## Implementation Steps

1. **Insert new method** `HandleTrimExecution` after `CleanupTerminalTargetFill`
2. **Replace inline logic** at lines 450-516 with method call
3. **Verify ASCII-only**: All string literals use straight quotes
4. **Run complexity audit**: `python scripts/complexity_audit.py`
   - Verify `ProcessOnExecutionUpdate` CYC reduced by ~12
   - Verify `HandleTrimExecution` CYC ≤8
5. **Run deploy-sync**: `powershell -File .\deploy-sync.ps1`
6. **Bump BUILD_TAG** in `src/V12_002.cs`

---

## F5 Verification

**Test Scenario**: Execute trim order, verify stop quantity reduced

**Setup**:
1. Place entry order (Buy 2 MES @ Market)
2. Entry fills, creates stop for 2 contracts
3. Execute trim order (close 1 contract)

**Expected Behavior**:
1. "TRIM EXECUTION: 1 contracts closed for [entry]. Position: 2 -> 1" log appears
2. "STOP INTEGRITY: Reducing stop quantity from 2 to 1 for [entry]" log appears
3. Stop order quantity reduced from 2 to 1
4. Position updated correctly (1 contract remaining)
5. No errors in log

**Verification Command**:
```
F5 in NinjaTrader → Place entry (2 contracts) → Execute trim (1 contract) → Check log
```

---

## Success Criteria

- [ ] New method `HandleTrimExecution` added
- [ ] Inline logic replaced with method call (lines 450-516)
- [ ] ASCII-only compliance verified (no Unicode)
- [ ] Complexity audit passes:
  - [ ] `ProcessOnExecutionUpdate` CYC reduced by ~12
  - [ ] `HandleTrimExecution` CYC ≤8
- [ ] Deploy-sync passes (ASCII gate)
- [ ] BUILD_TAG bumped
- [ ] F5 verification passes:
  - [ ] "TRIM EXECUTION" log appears
  - [ ] "STOP INTEGRITY" log appears
  - [ ] Stop quantity reduced correctly
  - [ ] Position updated correctly
- [ ] Build + tests pass

---

## Risk Assessment

**Risk Level**: MEDIUM

**Rationale**:
- Stop quantity reduction is critical (prevents reverse position)
- Position tracking affects downstream logic
- Cleanup logic must handle partial vs full close

**Mitigation**:
1. **Exact Copy**: Zero logic drift, preserve all comments (especially CRITICAL warning)
2. **F5 Verification**: Trigger trim scenario with stop quantity verification
3. **Unit Tests**: Test stop quantity reduction with various trim quantities

---

## Unit Tests (Add to Test Suite)

```csharp
[Test]
public void HandleTrimExecution_PartialTrim_ReducesStopQuantity()
{
    // Arrange
    string orderName = "Trim_TestEntry1";
    int quantity = 1;
    double price = 5000.0;
    SetupMockPosition("TestEntry1", 2); // 2 contracts
    SetupMockStop("TestEntry1", 2); // Stop for 2 contracts
    
    // Act
    strategy.HandleTrimExecution(orderName, quantity, price);
    
    // Assert
    var pos = strategy.GetPosition("TestEntry1");
    Assert.AreEqual(1, pos.RemainingContracts, "Position should be reduced to 1");
    var stop = strategy.GetStopOrder("TestEntry1");
    Assert.AreEqual(1, stop.Quantity, "Stop quantity should be reduced to 1");
}

[Test]
public void HandleTrimExecution_FullTrim_CancelsStop()
{
    // Arrange
    string orderName = "Trim_TestEntry1";
    int quantity = 2;
    double price = 5000.0;
    SetupMockPosition("TestEntry1", 2); // 2 contracts
    SetupMockStop("TestEntry1", 2); // Stop for 2 contracts
    
    // Act
    strategy.HandleTrimExecution(orderName, quantity, price);
    
    // Assert
    var pos = strategy.GetPosition("TestEntry1");
    Assert.AreEqual(0, pos.RemainingContracts, "Position should be fully closed");
    var stop = strategy.GetStopOrder("TestEntry1");
    Assert.IsNull(stop, "Stop should be cancelled");
}

[Test]
public void HandleTrimExecution_PreventsReversePosition()
{
    // Arrange: Long 4 contracts, trim 2 (now Long 2)
    string orderName = "Trim_TestEntry1";
    int quantity = 2;
    double price = 5000.0;
    SetupMockPosition("TestEntry1", 4); // 4 contracts
    SetupMockStop("TestEntry1", 4); // Stop for 4 contracts
    
    // Act
    strategy.HandleTrimExecution(orderName, quantity, price);
    
    // Assert
    var pos = strategy.GetPosition("TestEntry1");
    Assert.AreEqual(2, pos.RemainingContracts, "Position should be 2 contracts");
    var stop = strategy.GetStopOrder("TestEntry1");
    Assert.AreEqual(2, stop.Quantity, "Stop quantity MUST be 2 (not 4) to prevent reverse position");
}
```

---

## Rollback Plan

If F5 verification fails:
1. Revert commit: `git reset --hard HEAD~1`
2. Restore inline trim logic
3. Report issue to Director
4. Investigate stop quantity calculation or position tracking bug

---

## Final Verification (After Ticket 5)

After completing this ticket, verify the entire EPIC-CCN-15 refactoring:

1. **Run complexity audit**: `python scripts/complexity_audit.py`
   - Verify `ProcessOnExecutionUpdate` CYC ≤8 (target achieved)
   - Verify all extracted methods CYC ≤8

2. **Run grep for locks**: `grep -r "lock(" src/V12_002.Orders.Callbacks.Execution.cs`
   - Verify zero matches (no locks introduced)

3. **Run deploy-sync**: `powershell -File .\deploy-sync.ps1`
   - Verify ASCII gate passes

4. **F5 Full Workflow Test**:
   - Place entry → Hit stop → Verify OCO cancellation
   - Place entry → Hit T1 → Verify stop quantity reduced
   - Place entry (2 contracts) → Trim 1 → Verify stop integrity

5. **Update epic_roadmap.json**:
   - Mark EPIC-CCN-15 as complete
   - Record final CYC and completion date

---

**Ticket Created**: 2026-06-09T03:40:00Z  
**Assigned To**: Bob Shell (v12-engineer)  
**Status**: Ready for Execution (Depends on Ticket 1)