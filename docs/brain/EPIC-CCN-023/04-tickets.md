# Extraction Tickets: EPIC-CCN-023

## Overview
- **Total Tickets**: 3
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3)
- **Estimated Effort**: 4 hours
- **Target Method**: `HandleFlatPosition_CleanupActivePositions`
- **Target File**: `src/V12_002.Orders.Callbacks.Execution.cs`
- **Current Complexity**: 17 CYC
- **Target Complexity**: ≤8 CYC (Jane Street strict standard)

---

## TICKET-1: Extract CancelStopOrderIfActive

### Scope
- **Current Method**: `HandleFlatPosition_CleanupActivePositions`
- **Current CYC**: 17
- **Target CYC**: 4 (helper) + reduced main method
- **Extraction**: Stop order cancellation logic

### Implementation
1. Create new private method `CancelStopOrderIfActive(string positionKey, PositionInfo pos)`
2. Extract stop order validation and cancellation logic (lines 162-169)
3. Add XML documentation with clear responsibility statement
4. Return `bool` indicating if stop order was cancelled
5. Update main method to call helper instead of inline logic

### Code Template
```csharp
/// <summary>
/// Cancels stop order if it exists and is in a cancellable state.
/// </summary>
/// <param name="positionKey">Position identifier</param>
/// <param name="pos">Position information</param>
/// <returns>True if stop order was cancelled, false otherwise</returns>
private bool CancelStopOrderIfActive(string positionKey, PositionInfo pos)
{
    if (!stopOrders.TryGetValue(positionKey, out var stopOrder))
        return false;
    
    if (stopOrder == null)
        return false;
    
    if (stopOrder.OrderState != OrderState.Working && 
        stopOrder.OrderState != OrderState.Accepted)
        return false;
    
    CancelOrderSafe(stopOrder, pos);
    return true;
}
```

### Acceptance Criteria
- [ ] Helper method created with CYC = 4
- [ ] XML documentation added
- [ ] Main method updated to call helper
- [ ] No lock() statements introduced
- [ ] All mutations via existing `CancelOrderSafe`
- [ ] Unit test written (TDD)
- [ ] Unit test passes
- [ ] Build succeeds (`dotnet build`)
- [ ] Complexity audit passes (`python scripts/complexity_audit.py`)
- [ ] ASCII-only compliance verified

### Dependencies
- None (first ticket)

### Verification Commands
```bash
# Build check
dotnet build

# Complexity check
python scripts/complexity_audit.py

# Lock-free check
grep -r "lock(" src/V12_002.Orders.Callbacks.Execution.cs

# ASCII check
powershell -File .\scripts\pre_push_validation.ps1 -Fast
```

---

## TICKET-2: Extract CancelTargetOrdersIfActive

### Scope
- **Current Method**: `HandleFlatPosition_CleanupActivePositions`
- **Current CYC**: 17 → further reduced
- **Target CYC**: 5 (helper) + reduced main method
- **Extraction**: Target order cancellation logic (T1-T5)

### Implementation
1. Create new private method `CancelTargetOrdersIfActive(string positionKey, PositionInfo pos)`
2. Extract target order iteration and cancellation logic (lines 173-183)
3. Add XML documentation with clear responsibility statement
4. Return `int` count of cancelled target orders
5. Update main method to call helper instead of inline logic

### Code Template
```csharp
/// <summary>
/// Cancels all active target orders (T1-T5) for a position.
/// </summary>
/// <param name="positionKey">Position identifier</param>
/// <param name="pos">Position information</param>
/// <returns>Count of target orders cancelled</returns>
private int CancelTargetOrdersIfActive(string positionKey, PositionInfo pos)
{
    int cancelledCount = 0;
    
    for (int tNum = 1; tNum <= 5; tNum++)
    {
        var tDict = GetTargetOrdersDictionary(tNum);
        if (tDict == null)
            continue;
        
        if (!tDict.TryGetValue(positionKey, out var tOrder))
            continue;
        
        if (tOrder == null)
            continue;
        
        if (tOrder.OrderState != OrderState.Working && 
            tOrder.OrderState != OrderState.Accepted)
            continue;
        
        CancelOrderSafe(tOrder, pos);
        cancelledCount++;
    }
    
    return cancelledCount;
}
```

### Acceptance Criteria
- [ ] Helper method created with CYC = 5
- [ ] XML documentation added
- [ ] Main method updated to call helper
- [ ] No lock() statements introduced
- [ ] All mutations via existing `CancelOrderSafe`
- [ ] Unit test written (TDD)
- [ ] Unit test passes
- [ ] Build succeeds (`dotnet build`)
- [ ] Complexity audit passes (`python scripts/complexity_audit.py`)
- [ ] ASCII-only compliance verified

### Dependencies
- TICKET-1 must be completed first

### Verification Commands
```bash
# Build check
dotnet build

# Complexity check
python scripts/complexity_audit.py

# Lock-free check
grep -r "lock(" src/V12_002.Orders.Callbacks.Execution.cs

# ASCII check
powershell -File .\scripts\pre_push_validation.ps1 -Fast
```

---

## TICKET-3: Extract FinalizePositionCleanup

### Scope
- **Current Method**: `HandleFlatPosition_CleanupActivePositions`
- **Current CYC**: 17 → 4 (final target)
- **Target CYC**: 2 (helper) + 4 (main method)
- **Extraction**: Position cleanup finalization logic

### Implementation
1. Create new private method `FinalizePositionCleanup(List<string> positionsToCleanup)`
2. Extract cleanup iteration and logging logic (lines 189-193)
3. Add XML documentation with clear responsibility statement
4. Return `void` (side effects only)
5. Update main method to call helper instead of inline logic

### Code Template
```csharp
/// <summary>
/// Finalizes cleanup by removing positions and logging completion.
/// </summary>
/// <param name="positionsToCleanup">List of position keys to clean up</param>
private void FinalizePositionCleanup(List<string> positionsToCleanup)
{
    if (positionsToCleanup.Count == 0)
        return;
    
    foreach (string key in positionsToCleanup)
        CleanupPosition(key);
    
    Print("Cleanup complete - Strategy still running, ready for new entries.");
}
```

### Acceptance Criteria
- [ ] Helper method created with CYC = 2
- [ ] XML documentation added
- [ ] Main method updated to call helper
- [ ] Main method final CYC = 4 (≤8 target achieved)
- [ ] No lock() statements introduced
- [ ] All mutations via existing `CleanupPosition`
- [ ] Unit test written (TDD)
- [ ] Unit test passes
- [ ] Integration test written (main method orchestration)
- [ ] Integration test passes
- [ ] Build succeeds (`dotnet build`)
- [ ] Complexity audit passes (`python scripts/complexity_audit.py`)
- [ ] ASCII-only compliance verified
- [ ] Full regression suite passes

### Dependencies
- TICKET-1 must be completed first
- TICKET-2 must be completed first

### Verification Commands
```bash
# Build check
dotnet build

# Full test suite
dotnet test

# Complexity check (verify main method CYC = 4)
python scripts/complexity_audit.py

# Lock-free check
grep -r "lock(" src/V12_002.Orders.Callbacks.Execution.cs

# Full pre-push validation
powershell -File .\scripts\pre_push_validation.ps1

# Hard-link sync
powershell -File .\deploy-sync.ps1
```

---

## Final Refactored Main Method

After all three tickets are completed, the main method should look like this:

```csharp
private void HandleFlatPosition_CleanupActivePositions()
{
    List<string> positionsToCleanup = new List<string>();
    
    foreach (var kvp in activePositions.ToArray())
    {
        if (!activePositions.ContainsKey(kvp.Key))
            continue;
        
        PositionInfo pos = kvp.Value;
        if (!pos.EntryFilled || pos.RemainingContracts <= 0)
            continue;
        
        Print("EXTERNAL CLOSE DETECTED - Position went flat. Cancelling orphaned orders...");
        
        CancelStopOrderIfActive(kvp.Key, pos);
        CancelTargetOrdersIfActive(kvp.Key, pos);
        
        positionsToCleanup.Add(kvp.Key);
    }
    
    FinalizePositionCleanup(positionsToCleanup);
}
```

**Final Complexity**: 4 CYC (1 base + 1 loop + 2 conditionals)

---

## Testing Strategy (Jane Street Standard)

### Unit Tests (Per Helper)
1. **CancelStopOrderIfActive** (4 test cases):
   - Stop order exists and is Working → should cancel
   - Stop order exists and is Accepted → should cancel
   - Stop order exists but is Filled → should not cancel
   - Stop order does not exist → should not cancel

2. **CancelTargetOrdersIfActive** (5 test cases):
   - All 5 targets exist and are Working → should cancel all
   - Mix of Working/Filled targets → should cancel only Working
   - No targets exist → should cancel none
   - Targets exist but are Filled → should cancel none
   - Dictionary is null → should handle gracefully

3. **FinalizePositionCleanup** (2 test cases):
   - Empty list → should not call CleanupPosition
   - Non-empty list → should call CleanupPosition for each key

### Integration Test (Main Method)
- **Scenario**: External close detected with active stop and target orders
- **Expected**: All orders cancelled, position cleaned up, log message printed
- **Verification**: Mock `CancelOrderSafe` and `CleanupPosition` calls

### Property-Based Test
- **Property**: "All active orders cancelled when position goes flat"
- **Generator**: Random position states with varying order configurations
- **Invariant**: After method execution, no Working/Accepted orders remain

---

## Phase 4 Completion Checklist

- [ ] TICKET-1 completed and verified
- [ ] TICKET-2 completed and verified
- [ ] TICKET-3 completed and verified
- [ ] All unit tests pass
- [ ] Integration test passes
- [ ] Build succeeds
- [ ] Complexity audit passes (all methods ≤8 CYC)
- [ ] Lock-free validation passes (zero locks)
- [ ] ASCII-only validation passes
- [ ] Hard-link sync completed
- [ ] Manifest updated with Phase 4 completion

---

**Epic**: EPIC-CCN-023  
**Phase**: 4 (Ticket Generation)  
**Status**: ✅ COMPLETE  
**Date**: 2026-06-15  
**Next Phase**: Phase 5 (Recursive Execution)
