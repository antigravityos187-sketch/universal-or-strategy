# EPIC-CCN-20 Ticket 1: Extract TryHandleFleet_CancelAll

**Status**: Ready for Execution  
**Epic**: EPIC-CCN-20 (TryHandleFleet_CancelAll CYC 19 → ≤8)  
**File**: `src/V12_002.UI.IPC.Commands.Fleet.cs`  
**Method**: `TryHandleFleet_CancelAll` (Line 177)  
**Current CYC**: 19  
**Target CYC**: ≤8  
**Estimated Duration**: 15-25 minutes

---

## Objective

Reduce cyclomatic complexity of `TryHandleFleet_CancelAll` from CYC 19 to ≤8 through surgical extraction, achieving Jane Street ultra-alignment (CYC ≤8 cognitive simplicity standard).

---

## Current State Analysis

### Method Signature
```csharp
private bool TryHandleFleet_CancelAll(string action, string cmdId)
```

### Complexity Breakdown (CYC 19)

**Primary Branches** (9 decision points):
1. `if (action != "CANCEL_ALL")` - Early return guard
2. `if (!MetadataGuardDuplicate(cmdId, action))` - Duplicate command check
3. `if (EnableSIMA)` - SIMA vs V12 mode routing
4. SIMA path: Master account processing
5. SIMA path: Fleet account processing
6. V12 path: Order iteration loop
7. V12 path: Order state validation (5 conditions OR'd)
8. V12 path: Order name filtering (7 conditions OR'd)
9. V12 path: Order cancellation

**Helper Methods Called**:
- `MetadataGuardDuplicate(cmdId, action)` - Duplicate detection
- `CancelAll_ProcessMasterAccount()` - Master account order cancellation
- `CancelAll_ProcessFleetAccounts()` - Fleet account order cancellation
- `CancelOrderOnAccount(order, account)` - Individual order cancellation

**Key Observations**:
1. **Dual-mode logic**: SIMA vs V12 paths (complexity driver)
2. **Nested conditionals**: Order state + name filtering (7 StartsWith checks)
3. **Loop complexity**: Iterating Account.Orders with multiple guards
4. **Already partially extracted**: SIMA path delegates to helpers (good!)
5. **V12 path needs extraction**: 30+ lines of inline logic

---

## Extraction Plan

### Target Structure (CYC ≤8)

```csharp
private bool TryHandleFleet_CancelAll(string action, string cmdId)
{
    if (action != "CANCEL_ALL") return false;                    // CYC +1
    if (!MetadataGuardDuplicate(cmdId, action)) return true;     // CYC +1
    
    if (EnableSIMA)                                              // CYC +1
    {
        CancelAll_SIMA();
    }
    else
    {
        CancelAll_V12();
    }
    
    return true;
}
// Target CYC: 3 ✅
```

### New Helper Methods

#### 1. `CancelAll_SIMA()` (CYC ≤5)
```csharp
private void CancelAll_SIMA()
{
    int masterCancelled = CancelAll_ProcessMasterAccount();
    int fleetCancelled = CancelAll_ProcessFleetAccounts();
    int totalCancelled = masterCancelled + fleetCancelled;
    Print($"[SIMA] CANCEL_ALL -> Cancelled {totalCancelled} orders (Entries + Orphaned Brackets) (local + fleet) [1001]");
}
// Target CYC: 1 ✅
```

#### 2. `CancelAll_V12()` (CYC ≤7)
```csharp
private void CancelAll_V12()
{
    int cancelled = 0;
    foreach (Order order in Account.Orders)                      // CYC +1
    {
        if (!ShouldCancelOrder_V12(order)) continue;             // CYC +1
        
        CancelOrderOnAccount(order, order.Account);
        cancelled++;
    }
    Print($"[V12] CANCEL_ALL -> Cancelled {cancelled} pending entry orders");
}
// Target CYC: 2 ✅
```

#### 3. `ShouldCancelOrder_V12(Order order)` (CYC ≤8)
```csharp
private bool ShouldCancelOrder_V12(Order order)
{
    if (order == null) return false;                             // CYC +1
    if (order.Instrument.FullName != Instrument.FullName) return false; // CYC +1
    if (!IsOrderWorking(order)) return false;                    // CYC +1
    if (IsProtectedBracketOrder(order.Name)) return false;       // CYC +1
    
    return true;
}
// Target CYC: 4 ✅
```

#### 4. `IsOrderWorking(Order order)` (CYC ≤6)
```csharp
private bool IsOrderWorking(Order order)
{
    return order.OrderState == OrderState.Working                // CYC +1
        || order.OrderState == OrderState.Accepted               // CYC +1
        || order.OrderState == OrderState.Submitted              // CYC +1
        || order.OrderState == OrderState.ChangePending          // CYC +1
        || order.OrderState == OrderState.ChangeSubmitted;       // CYC +1
}
// Target CYC: 5 ✅
```

#### 5. `IsProtectedBracketOrder(string orderName)` (CYC ≤8)
```csharp
private bool IsProtectedBracketOrder(string orderName)
{
    return orderName.StartsWith("Stop_")                         // CYC +1
        || orderName.StartsWith("S_")                            // CYC +1
        || orderName.StartsWith("T1_")                           // CYC +1
        || orderName.StartsWith("T2_")                           // CYC +1
        || orderName.StartsWith("T3_")                           // CYC +1
        || orderName.StartsWith("T4_")                           // CYC +1
        || orderName.StartsWith("T5_");                          // CYC +1
}
// Target CYC: 7 ✅
```

---

## Complexity Verification

| Method | Current CYC | Target CYC | Status |
|--------|-------------|------------|--------|
| `TryHandleFleet_CancelAll` | 19 | 3 | ✅ Reduced |
| `CancelAll_SIMA` | N/A | 1 | ✅ New |
| `CancelAll_V12` | N/A | 2 | ✅ New |
| `ShouldCancelOrder_V12` | N/A | 4 | ✅ New |
| `IsOrderWorking` | N/A | 5 | ✅ New |
| `IsProtectedBracketOrder` | N/A | 7 | ✅ New |

**Total CYC Reduction**: 19 → 3 (16 points reduced, 84% improvement)

---

## Jane Street Alignment

### Principles Applied

1. **Cognitive Simplicity** (CYC ≤8)
   - Main method: CYC 3 (ultra-simple)
   - All helpers: CYC ≤7 (within Jane Street threshold)

2. **Single Responsibility**
   - `CancelAll_SIMA`: SIMA-specific cancellation logic
   - `CancelAll_V12`: V12-specific cancellation logic
   - `ShouldCancelOrder_V12`: Order filtering predicate
   - `IsOrderWorking`: Order state validation
   - `IsProtectedBracketOrder`: Bracket order detection

3. **Explicit Error Handling**
   - Null checks in `ShouldCancelOrder_V12`
   - Early returns for invalid states

4. **Immutable Data**
   - No state mutation in predicates
   - Read-only order inspection

5. **Lock-Free Concurrency**
   - No locks introduced (maintains V12 DNA)
   - Delegates to existing `CancelOrderOnAccount` (already lock-free)

---

## V12 DNA Compliance

### Mandatory Checks

- ✅ **No Internal Locks**: Zero `lock()` blocks introduced
- ✅ **ASCII-Only**: No Unicode/emoji in strings
- ✅ **Surgical Extraction**: Pure structural movement, zero logic drift
- ✅ **FSM-Driven**: Delegates to existing FSM/Actor patterns
- ✅ **Complexity Standards**: All methods CYC ≤8

---

## Pre-Execution Checklist

- [ ] Verify current CYC via `python scripts/complexity_audit.py`
- [ ] Confirm line numbers match live source (Line 177)
- [ ] Check for pre-existing compilation errors
- [ ] Verify hard links synchronized (`powershell -File .\deploy-sync.ps1`)
- [ ] Confirm git status clean

---

## Execution Steps

### Phase 1: Extract SIMA Helper (2 min)
1. Create `CancelAll_SIMA()` method
2. Move SIMA-specific logic from main method
3. Verify CYC reduction

### Phase 2: Extract V12 Helper (3 min)
1. Create `CancelAll_V12()` method
2. Move V12-specific loop logic
3. Extract `ShouldCancelOrder_V12()` predicate

### Phase 3: Extract Order Predicates (3 min)
1. Create `IsOrderWorking()` method
2. Create `IsProtectedBracketOrder()` method
3. Wire predicates into `ShouldCancelOrder_V12()`

### Phase 4: Simplify Main Method (2 min)
1. Replace inline logic with helper calls
2. Verify early returns preserved
3. Confirm CYC ≤3

### Phase 5: Verification (5 min)
1. Run `python scripts/complexity_audit.py` (verify CYC ≤8)
2. Run `dotnet build` (zero errors)
3. Run `powershell -File .\deploy-sync.ps1` (ASCII gate pass)
4. Run `powershell -File .\scripts\pre_push_validation.ps1 -Fast`
5. Bump `BUILD_TAG` in `src/V12_002.cs`

---

## Success Criteria

- ✅ `TryHandleFleet_CancelAll` CYC reduced to ≤3
- ✅ All extracted methods CYC ≤8
- ✅ Zero compilation errors
- ✅ Zero logic drift (behavior unchanged)
- ✅ Pre-push validation passing (13/13 checks)
- ✅ Hard links synchronized (81/81 files)
- ✅ BUILD_TAG bumped

---

## Risk Mitigation

### Low Risk Factors
- ✅ SIMA path already uses helpers (minimal change)
- ✅ V12 path is self-contained (no cross-file dependencies)
- ✅ Order filtering logic is pure (no side effects)

### Rollback Plan
If extraction fails:
1. `git reset --hard HEAD~1`
2. Review failure in `docs/brain/EPIC-CCN-20/failure-analysis.md`
3. Fix root cause
4. Restart ticket

---

## References

- **Epic Plan**: `docs/brain/EPIC-LOOP-EXECUTION-PLAN.md`
- **Source File**: `src/V12_002.UI.IPC.Commands.Fleet.cs` (Line 177)
- **Jane Street KB**: Auto-loaded via `.bob/hooks/pre_session.py`
- **Complexity Audit**: `python scripts/complexity_audit.py`
- **Pre-Push Validation**: `scripts/pre_push_validation.ps1`

---

**Last Updated**: 2026-06-09  
**Status**: Ready for Execution  
**Estimated Completion**: 15 minutes
