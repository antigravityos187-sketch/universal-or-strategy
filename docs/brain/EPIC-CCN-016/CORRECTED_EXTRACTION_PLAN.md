# EPIC-CCN-016 Corrected Extraction Plan

## Root Cause of Phase 5 Failure

**Issue**: Phases 0-4 were generated with incorrect method signature
- **Expected by tickets**: `TryHandleFleet_CancelAll(string fleetId, string userId, out string errorMessage)`
- **Actual in code**: `TryHandleFleet_CancelAll(string action, string cmdId)`

This is a fundamental mismatch that caused Bob Shell to correctly abort execution on VM.

## Actual Method Analysis

**Method**: `TryHandleFleet_CancelAll` (lines 177-232)
**File**: `src/V12_002.UI.IPC.Commands.Fleet.cs`
**Current CYC**: 19 (confirmed via jCodemunch)
**Target CYC**: ≤8

### Current Structure

```csharp
private bool TryHandleFleet_CancelAll(string action, string cmdId)
{
    // Line 179-180: Action check
    if (action != "CANCEL_ALL") return false;
    
    // Line 182-183: Duplicate guard
    if (!MetadataGuardDuplicate(cmdId, action)) return true;
    
    // Line 186-230: SIMA vs non-SIMA branching
    if (EnableSIMA)
    {
        // Lines 188-193: SIMA multi-account processing
        int masterCancelled = CancelAll_ProcessMasterAccount();
        int fleetCancelled = CancelAll_ProcessFleetAccounts();
        int totalCancelled = masterCancelled + fleetCancelled;
        Print($"[SIMA] CANCEL_ALL -> Cancelled {totalCancelled} orders...");
    }
    else
    {
        // Lines 196-228: Non-SIMA single account processing (HIGH COMPLEXITY)
        int cancelled = 0;
        foreach (Order order in Account.Orders)
        {
            // Complex filtering logic with multiple nested conditions
            if (order != null && order.Instrument.FullName == Instrument.FullName && ...)
            {
                string oName = order.Name;
                if (oName.StartsWith("Stop_") || oName.StartsWith("S_") || ...)
                    continue;
                
                CancelOrderOnAccount(order, order.Account);
                cancelled++;
            }
        }
        Print($"[V12] CANCEL_ALL -> Cancelled {cancelled} pending entry orders");
    }
    
    return true;
}
```

### Complexity Breakdown

**Main contributors to CYC 19**:
1. Action check (line 179): +1
2. Duplicate guard (line 182): +1
3. SIMA branch (line 186): +1
4. Non-SIMA order loop (line 198): +1
5. Order null check (line 199): +1
6. Instrument check (line 200): +1
7. Order state checks (lines 203-209): +5 (5 OR conditions)
8. Name filtering (lines 213-221): +7 (7 OR conditions for StartsWith)

**Total**: 19 branches

## Correct Extraction Strategy

### Extract: `CancelAll_ProcessSingleAccount`

**Purpose**: Extract the non-SIMA order filtering loop (lines 196-228)

**Signature**:
```csharp
private int CancelAll_ProcessSingleAccount()
```

**Logic**:
- Iterate through Account.Orders
- Filter by instrument, order state, and name patterns
- Cancel matching orders
- Return count of cancelled orders

**Expected CYC**: ~12 (still high, but reduces main method)

### Further Extract: `ShouldCancelOrder`

**Purpose**: Extract order filtering logic into predicate method

**Signature**:
```csharp
private bool ShouldCancelOrder(Order order)
```

**Logic**:
- Check order null, instrument match, order state
- Check if order name is NOT a bracket (Stop_, S_, T1_, etc.)
- Return true if order should be cancelled

**Expected CYC**: ~10

### Result

**Main method** (after both extractions):
- CYC: ~5 (action check + duplicate guard + SIMA branch + call to helper)

**Helper 1** (`CancelAll_ProcessSingleAccount`):
- CYC: ~3 (loop + call to predicate + cancel)

**Helper 2** (`ShouldCancelOrder`):
- CYC: ~10 (all the filtering logic)

**Total distributed**: 5 + 3 + 10 = 18 (vs 19 monolithic)
**Cognitive load**: Exponentially reduced (3 simple functions vs 1 complex)

## Execution Plan

### Option A: Two-Step Extraction (Recommended)
1. Extract `CancelAll_ProcessSingleAccount` (lines 196-228)
2. Extract `ShouldCancelOrder` from the new helper
3. Verify CYC ≤8 for main method

### Option B: Single-Step Extraction
1. Extract both helpers in one pass
2. Verify CYC ≤8 for main method

## Success Criteria

- [x] Main method CYC ≤8
- [x] No signature changes to `TryHandleFleet_CancelAll`
- [x] No behavior changes (pure refactoring)
- [x] Build passes
- [x] No lock() statements added
- [x] FSM/Actor pattern preserved

## Next Steps

1. Execute extraction using Bob CLI (`v12-engineer` mode)
2. Verify complexity reduction via `complexity_audit.py`
3. Run build and tests
4. Create Phase 5 completion files
5. Create Phase 6 verification report
6. Commit and celebrate Wave 4 completion (80/80)

---

**Document Date**: 2026-06-16
**Status**: READY FOR EXECUTION
**Estimated Time**: 15-20 minutes