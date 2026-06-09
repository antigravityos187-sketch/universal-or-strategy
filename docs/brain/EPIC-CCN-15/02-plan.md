# EPIC-CCN-15: Implementation Plan

## Phase 2: Surgical Extraction Plan

**Date**: 2026-06-09  
**Epic Number**: EPIC-CCN-15  
**Status**: Phase 2 - Planning Complete

---

## Extraction Order (Dependency-First)

### Ticket 1: Extract Entry Name Helper (Foundation)
**Priority**: P0 (Required by all other extractions)  
**Target Method**: `ExtractEntryNameFromOrder`  
**Estimated CYC**: 4 → 4 (no reduction needed, already simple)

**Rationale**: This helper is used by all 3 execution handlers. Extract first to avoid duplication.

---

### Ticket 2: Extract Deduplication Guard
**Priority**: P1 (Independent, high complexity)  
**Target Method**: `CheckExecutionDeduplication`  
**Estimated CYC**: 12 → 8

**Rationale**: Independent logic, no dependencies on other extractions. High complexity reduction.

---

### Ticket 3: Extract Stop Loss Fill Handler
**Priority**: P2 (Depends on Ticket 1)  
**Target Method**: `HandleStopLossFill`  
**Estimated CYC**: 18 → 8  
**Sub-extraction**: `CancelTargetOrdersForEntry` (OCO loop)

**Rationale**: Uses `ExtractEntryNameFromOrder`. Requires sub-extraction for OCO loop.

---

### Ticket 4: Extract Target Fill Handler
**Priority**: P3 (Depends on Ticket 1)  
**Target Method**: `HandleTargetFill`  
**Estimated CYC**: 20 → 8  
**Sub-extraction**: `CleanupTerminalTargetFill` (terminal state cleanup)

**Rationale**: Uses `ExtractEntryNameFromOrder`. Requires sub-extraction for terminal cleanup.

---

### Ticket 5: Extract Trim Execution Handler
**Priority**: P4 (Depends on Ticket 1)  
**Target Method**: `HandleTrimExecution`  
**Estimated CYC**: 15 → 8

**Rationale**: Uses `ExtractEntryNameFromOrder`. Simpler than other handlers, no sub-extraction needed.

---

## Method Signatures

### 1. ExtractEntryNameFromOrder (Ticket 1)

```csharp
/// <summary>
/// V12.CCN-15 [T1]: Extracts entry name from order name by removing prefix and optional timestamp suffix.
/// </summary>
/// <param name="orderName">Full order name (e.g., "Stop_Entry1_123456789012345")</param>
/// <param name="prefix">Prefix to remove (e.g., "Stop_", "T1_", "Trim_")</param>
/// <returns>Entry name without prefix or timestamp (e.g., "Entry1"), or empty string if prefix doesn't match</returns>
private static string ExtractEntryNameFromOrder(string orderName, string prefix)
{
    if (!orderName.StartsWith(prefix))
        return "";
    
    string entryPart = orderName.Substring(prefix.Length);
    
    // Strip timestamp suffix if present (format: _123456789012345)
    int lastUnderscore = entryPart.LastIndexOf('_');
    if (lastUnderscore > 0 && entryPart.Length - lastUnderscore > 10)
        entryPart = entryPart.Substring(0, lastUnderscore);
    
    return entryPart;
}
```

**CYC**: 4 (3 conditionals + 1 base)  
**LOC**: 12 (below 15-line floor, but justified for DRY + testability)

---

### 2. CheckExecutionDeduplication (Ticket 2)

```csharp
/// <summary>
/// V12.CCN-15 [T2]: Deduplication guard using FNV-1a hash rings.
/// Prevents double-decrement if OnOrderUpdate + OnExecutionUpdate both fire.
/// </summary>
/// <param name="executionId">Execution ID from broker (may be null/empty)</param>
/// <param name="execution">Execution object for fallback dedup</param>
/// <param name="orderName">Order name for logging</param>
/// <returns>True if duplicate (skip processing), false if new (proceed)</returns>
private bool CheckExecutionDeduplication(string executionId, Execution execution, string orderName)
{
    if (!string.IsNullOrEmpty(executionId))
    {
        // Primary dedup: executionId hash ring
        long execHash = FnvHash64(executionId);
        if (_executionIdRing.ContainsOrAdd(execHash))
        {
            Print(string.Format("[DEDUP] Skipping duplicate execution {0} for {1}", executionId, orderName));
            return true;
        }
    }
    else
    {
        // Fallback dedup: orderId + filled quantity hash ring
        string uniqueOrderId = !string.IsNullOrEmpty(execution.Order.OrderId)
            ? execution.Order.OrderId
            : execution.Order.Name;
        int dedupFilledQty = execution.Order.Filled > 0 ? execution.Order.Filled : Math.Max(0, execution.Quantity);
        string fallbackKey = string.Format("{0}|{1}", uniqueOrderId, dedupFilledQty);
        long fallbackHash = FnvHash64(fallbackKey);
        
        if (_executionIdFallbackRing.ContainsOrAdd(fallbackHash))
        {
            Print(string.Format("[DEDUP] Skipping duplicate execution (fallback) orderId={0}", execution.Order.OrderId));
            return true;
        }
    }
    
    return false; // Not a duplicate, proceed with processing
}
```

**CYC**: 8 (7 conditionals + 1 base)  
**LOC**: ~35 lines  
**Reduction**: 12 → 8 (33% reduction)

---

### 3. HandleStopLossFill (Ticket 3)

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

**CYC**: 8 (7 conditionals + 1 base)  
**LOC**: ~35 lines  
**Reduction**: 18 → 8 (56% reduction)

**Sub-extraction**: `CancelTargetOrdersForEntry`

```csharp
/// <summary>
/// V12.CCN-15 [T3-SUB]: Cancels all working target orders for an entry (manual OCO).
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

**CYC**: 5 (4 conditionals + 1 base)  
**LOC**: ~18 lines

---

### 4. HandleTargetFill (Ticket 4)

```csharp
/// <summary>
/// V12.CCN-15 [T4]: Handles target (T1-T5) fill execution.
/// Reduces stop quantity, updates position, and cleans up if fully closed.
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
        RequestStopCancelLifecycleSafe(entryName);
        PositionInfo closedPos;
        if (activePositions.TryGetValue(entryName, out closedPos) && closedPos != null)
            closedPos.PendingCleanup = true;
        else
            SymmetryGuardForgetEntry(entryName);
    }
    
    // Clear target ref if terminal
    CleanupTerminalTargetFill(entryName, targetNum, terminalFill);
}
```

**CYC**: 8 (7 conditionals + 1 base)  
**LOC**: ~50 lines  
**Reduction**: 20 → 8 (60% reduction)

**Sub-extraction**: `CleanupTerminalTargetFill`

```csharp
/// <summary>
/// V12.CCN-15 [T4-SUB]: Cleans up target order reference after terminal fill.
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

**CYC**: 3 (2 conditionals + 1 base)  
**LOC**: ~10 lines

---

### 5. HandleTrimExecution (Ticket 5)

```csharp
/// <summary>
/// V12.CCN-15 [T5]: Handles trim execution (partial position close).
/// CRITICAL: Reduces stop quantity to prevent reverse position on stop-out.
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
    
    // MANDATORY stop quantity reduction to prevent reverse position
    if (remainingAfterTrim > 0)
    {
        Print(string.Format("STOP INTEGRITY: Reducing stop quantity from {0} to {1} for {2}", previousQty, remainingAfterTrim, entryName));
        UpdateStopQuantity(entryName, pos);
    }
    else
    {
        // Position fully closed by trim
        Print(string.Format("TRIM FLATTEN: Position {0} fully closed. Cancelling stop.", entryName));
        RequestStopCancelLifecycleSafe(entryName);
        
        if (pendingStopReplacements.TryRemove(entryName, out _))
            Interlocked.Decrement(ref pendingReplacementCount);
        
        PositionInfo trimPos;
        if (activePositions.TryGetValue(entryName, out trimPos) && trimPos != null)
            trimPos.PendingCleanup = true;
        else
            SymmetryGuardForgetEntry(entryName);
    }
}
```

**CYC**: 8 (7 conditionals + 1 base)  
**LOC**: ~35 lines  
**Reduction**: 15 → 8 (47% reduction)

---

## Refactored ProcessOnExecutionUpdate (Main Method)

```csharp
private void ProcessOnExecutionUpdate(
    string orderName,
    string executionId,
    string orderId,
    int orderFilled,
    OrderState orderState,
    double price,
    int quantity,
    Execution execution
)
{
    try
    {
        if (string.IsNullOrEmpty(orderName))
            return;
        
        // V12.CCN-15 [T2]: Deduplication guard
        if (CheckExecutionDeduplication(executionId, execution, orderName))
            return;
        
        // V12.12: Compliance tracking for single-account mode
        if (EnableComplianceHub && !EnableSIMA)
        {
            TrackTradeEntry(Account, execution);
            TriggerCustomEvent(o => UpdateAccountMetricsFromAccount(Account), null);
            LogApexPerformance();
        }
        
        // V12.CCN-15 [T3]: Stop loss fill handler
        if (orderName.StartsWith("Stop_"))
        {
            HandleStopLossFill(orderName, quantity, price);
        }
        // V12.CCN-15 [T4]: Target fill handler (T1-T5)
        else if (orderName.StartsWith("T1_") || orderName.StartsWith("T2_") || orderName.StartsWith("T3_") || orderName.StartsWith("T4_") || orderName.StartsWith("T5_"))
        {
            HandleTargetFill(orderName, quantity, price, execution);
        }
        // V12.CCN-15 [T5]: Trim execution handler
        else if (orderName.StartsWith("Trim_"))
        {
            HandleTrimExecution(orderName, quantity, price);
        }
        
        // Build 1105: Shadow callback injection
        ShadowEngineCheck();
    }
    catch (Exception ex)
    {
        Print("Error OnExecutionUpdate: " + ex.Message);
    }
}
```

**Final CYC**: 8 (7 conditionals + 1 base)  
**Final LOC**: ~45 lines  
**Reduction**: 67 → 8 (88% reduction) ✅

---

## Parameter Passing Strategy

### Shared State (No Changes)
All extracted methods remain **instance methods** with access to:
- `activePositions` (ConcurrentDictionary)
- `stopOrders` (ConcurrentDictionary)
- `pendingStopReplacements` (ConcurrentDictionary)
- `_executionIdRing`, `_executionIdFallbackRing` (hash rings)
- `entryOrders` (ConcurrentDictionary)

### Method Parameters (Minimal)
- Pass only **execution-specific data** (orderName, quantity, price, execution)
- Do NOT pass dictionaries (use instance fields)
- Do NOT pass `PositionInfo` (retrieve via `TryGetValue` inside handler)

---

## F5 Verification Checkpoints

### Ticket 1: ExtractEntryNameFromOrder
**Test**: Place entry order, verify name extraction in logs
**Expected**: No behavior change, same entry name parsing

### Ticket 2: CheckExecutionDeduplication
**Test**: Trigger duplicate execution (OnOrderUpdate + OnExecutionUpdate)
**Expected**: "[DEDUP]" log appears, no double-decrement

### Ticket 3: HandleStopLossFill
**Test**: Hit stop loss, verify targets cancelled
**Expected**: "STOP FILLED" + "OCO: Cancelled X targets" logs

### Ticket 4: HandleTargetFill
**Test**: Hit T1, verify stop quantity reduced
**Expected**: "TARGET FILLED" + "Reducing stop" logs

### Ticket 5: HandleTrimExecution
**Test**: Execute trim order, verify stop quantity reduced
**Expected**: "TRIM EXECUTION" + "STOP INTEGRITY" logs

---

## Unit Test Requirements

### Test Coverage (New Tests)

1. **ExtractEntryNameFromOrder**
   - Test with valid prefix (returns entry name)
   - Test with invalid prefix (returns empty string)
   - Test with timestamp suffix (strips suffix)
   - Test without timestamp suffix (returns full entry name)

2. **CheckExecutionDeduplication**
   - Test primary dedup (executionId present)
   - Test fallback dedup (executionId missing)
   - Test non-duplicate (returns false)
   - Test duplicate (returns true)

3. **CancelTargetOrdersForEntry**
   - Test with working targets (cancels all)
   - Test with no targets (returns 0)
   - Test with mixed states (cancels only Working/Accepted)

4. **CleanupTerminalTargetFill**
   - Test terminal fill (removes target ref)
   - Test non-terminal fill (no removal)

---

## Risk Mitigation

### High-Risk Extractions

1. **CheckExecutionDeduplication** (Ticket 2)
   - **Risk**: Hash collision edge case
   - **Mitigation**: Preserve exact FNV-1a logic, add unit tests for collision scenarios

2. **HandleStopLossFill** (Ticket 3)
   - **Risk**: Race condition during OCO cancellation
   - **Mitigation**: Maintain First-Writer-Wins guard in `CancelOrderSafe`

3. **HandleTargetFill** (Ticket 4)
   - **Risk**: Double-decrement if guard fails
   - **Mitigation**: Preserve `ApplyTargetFill` guard logic, add unit tests

### Low-Risk Extractions

1. **ExtractEntryNameFromOrder** (Ticket 1): Pure function, no state mutation
2. **HandleTrimExecution** (Ticket 5): Similar to stop fill, well-tested pattern

---

## Success Metrics

### Complexity Reduction

| Method | Before | After | Reduction |
|--------|--------|-------|-----------|
| `ProcessOnExecutionUpdate` | CYC 67 | CYC 8 | 88% ✅ |
| `CheckExecutionDeduplication` | - | CYC 8 | New |
| `HandleStopLossFill` | - | CYC 8 | New |
| `HandleTargetFill` | - | CYC 8 | New |
| `HandleTrimExecution` | - | CYC 8 | New |
| `CancelTargetOrdersForEntry` | - | CYC 5 | New |
| `CleanupTerminalTargetFill` | - | CYC 3 | New |
| `ExtractEntryNameFromOrder` | - | CYC 4 | New |

### Hotspot Reduction

| Metric | Before | After | Reduction |
|--------|--------|-------|-----------|
| **Hotspot Score** | 166.49 | <40 | 76% ✅ |
| **Max Nesting** | 7 | 4 | 43% ✅ |
| **Method Length** | 300 lines | 45 lines | 85% ✅ |

---

## Next Steps

**Phase 2.3**: Run sentinel audit (`/epic-scan`) to verify:
1. No lock statements in extracted methods
2. ASCII-only compliance
3. No logic drift during extraction

**Phase 3**: Validate plan against V12 DNA constraints

**Phase 4**: Generate implementation tickets (5 tickets + 2 sub-extractions)

---

**Plan Complete**: 2026-06-09T03:34:00Z  
**Approved By**: Bob Shell (v12-engineer)  
**Next Phase**: Phase 2.3 - Sentinel Audit