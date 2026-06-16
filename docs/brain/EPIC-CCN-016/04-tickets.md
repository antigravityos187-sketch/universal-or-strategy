# Extraction Tickets: EPIC-CCN-016

## Overview
- **Total Tickets**: 4
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3 → TICKET-4)
- **Estimated Effort**: 3-4 hours
- **Target Method**: `TryHandleFleet_CancelAll`
- **Current CYC**: 19
- **Target CYC**: ≤5 (main method after all extractions)

---

## TICKET-1: Extract IsOrderCancellable Helper

### Scope
- **Current Method**: `TryHandleFleet_CancelAll`
- **Current CYC**: 19
- **Target CYC**: 13 (after this extraction)
- **Extraction**: Order state validation logic into `IsOrderCancellable(Order order)`

### Implementation
1. Create new private method `IsOrderCancellable(Order order)` returning `bool`
2. Move order state validation logic:
   - Null check
   - Instrument match check (order.Instrument.FullName == Instrument.FullName)
   - Order state validation (Working, Accepted, Submitted, ChangePending, ChangeSubmitted)
3. Replace inline validation in main method with call to `IsOrderCancellable(order)`
4. Run complexity audit to verify CYC reduction (19 → 13, reduction of ~6)

### Method Signature
```csharp
private bool IsOrderCancellable(Order order)
{
    if (order == null) return false;
    if (order.Instrument.FullName != Instrument.FullName) return false;
    
    return order.OrderState == OrderState.Working
        || order.OrderState == OrderState.Accepted
        || order.OrderState == OrderState.Submitted
        || order.OrderState == OrderState.ChangePending
        || order.OrderState == OrderState.ChangeSubmitted;
}
```

### Acceptance Criteria
- [ ] New method `IsOrderCancellable` created with CYC ≤8
- [ ] Main method CYC reduced from 19 to ~13
- [ ] All tests pass (dotnet test)
- [ ] No behavioral changes
- [ ] Build succeeds (dotnet build)
- [ ] No lock() statements introduced

### Dependencies
- None (first ticket)

### Complexity Impact
- **IsOrderCancellable**: CYC 6 (1 base + 1 null + 1 instrument + 5 OR conditions)
- **Main method**: CYC 13 (reduction of 6)

---

## TICKET-2: Extract IsProtectedOrderName Helper

### Scope
- **Current Method**: `TryHandleFleet_CancelAll`
- **Current CYC**: 13 (after TICKET-1)
- **Target CYC**: 6 (after this extraction)
- **Extraction**: Protected order name prefix checks into `IsProtectedOrderName(string orderName)`

### Implementation
1. Create new private method `IsProtectedOrderName(string orderName)` returning `bool`
2. Move order name prefix validation logic:
   - Check for "Stop_", "S_", "T1_", "T2_", "T3_", "T4_", "T5_" prefixes
3. Replace inline prefix checks in main method with call to `IsProtectedOrderName(order.Name)`
4. Run complexity audit to verify CYC reduction (13 → 6, reduction of ~7)

### Method Signature
```csharp
private bool IsProtectedOrderName(string orderName)
{
    if (string.IsNullOrEmpty(orderName)) return false;
    
    return orderName.StartsWith("Stop_")
        || orderName.StartsWith("S_")
        || orderName.StartsWith("T1_")
        || orderName.StartsWith("T2_")
        || orderName.StartsWith("T3_")
        || orderName.StartsWith("T4_")
        || orderName.StartsWith("T5_");
}
```

### Acceptance Criteria
- [ ] New method `IsProtectedOrderName` created with CYC ≤8
- [ ] Main method CYC reduced from 13 to ~6
- [ ] All tests pass (dotnet test)
- [ ] No behavioral changes
- [ ] Build succeeds (dotnet build)
- [ ] No lock() statements introduced

### Dependencies
- TICKET-1 must be completed first

### Complexity Impact
- **IsProtectedOrderName**: CYC 7 (1 base + 7 OR conditions)
- **Main method**: CYC 6 (reduction of 7)

---

## TICKET-3: Extract CancelAll_ProcessNonSIMAAccount Helper

### Scope
- **Current Method**: `TryHandleFleet_CancelAll`
- **Current CYC**: 6 (after TICKET-2)
- **Target CYC**: ≤5 (after this extraction)
- **Extraction**: Non-SIMA order cancellation loop into `CancelAll_ProcessNonSIMAAccount()`

### Implementation
1. Create new private method `CancelAll_ProcessNonSIMAAccount()` returning `int` (cancelled count)
2. Move non-SIMA order cancellation logic:
   - Loop through Account.Orders
   - Call IsOrderCancellable() to filter eligible orders
   - Call IsProtectedOrderName() to skip protected orders
   - Call CancelOrderOnAccount() for each eligible order
   - Return total cancelled count
3. Replace inline loop in main method with call to `CancelAll_ProcessNonSIMAAccount()`
4. Run complexity audit to verify CYC reduction (6 → 5, reduction of ~1)

### Method Signature
```csharp
private int CancelAll_ProcessNonSIMAAccount()
{
    int cancelledCount = 0;
    
    foreach (Order order in Account.Orders)
    {
        if (!IsOrderCancellable(order)) continue;
        if (IsProtectedOrderName(order.Name)) continue;
        
        CancelOrderOnAccount(order);
        cancelledCount++;
    }
    
    return cancelledCount;
}
```

### Acceptance Criteria
- [ ] New method `CancelAll_ProcessNonSIMAAccount` created with CYC ≤8
- [ ] Main method CYC reduced to ≤5
- [ ] All tests pass (dotnet test)
- [ ] No behavioral changes
- [ ] Build succeeds (dotnet build)
- [ ] No lock() statements introduced

### Dependencies
- TICKET-1 must be completed first (uses IsOrderCancellable)
- TICKET-2 must be completed first (uses IsProtectedOrderName)

### Complexity Impact
- **CancelAll_ProcessNonSIMAAccount**: CYC 3 (1 base + 1 loop + 1 conditional)
- **Main method**: CYC 5 (final target achieved)

---

## TICKET-4: Final Verification & Integration

### Scope
- **Verification**: Confirm all extractions successful and integrated
- **Target**: Main method CYC ≤5, all helpers CYC ≤8
- **Integration**: Hard link sync and NinjaTrader F5 test

### Implementation
1. Run full complexity audit on file
2. Verify main method CYC ≤5
3. Verify all helper methods CYC ≤8
4. Run `powershell -File .\deploy-sync.ps1` to sync hard links
5. F5 in NinjaTrader IDE to verify BUILD_TAG
6. Run unit tests to verify behavioral preservation
7. Update epic_roadmap.json with completion status

### Acceptance Criteria
- [ ] Main method `TryHandleFleet_CancelAll` CYC ≤5
- [ ] Helper method `IsOrderCancellable` CYC ≤8
- [ ] Helper method `IsProtectedOrderName` CYC ≤8
- [ ] Helper method `CancelAll_ProcessNonSIMAAccount` CYC ≤8
- [ ] All tests pass (dotnet test)
- [ ] Build succeeds (dotnet build)
- [ ] Hard links synced (deploy-sync.ps1)
- [ ] Integration verified (F5 shows BUILD_TAG)
- [ ] No lock() statements in any method
- [ ] No behavioral changes detected

### Dependencies
- TICKET-1 must be completed
- TICKET-2 must be completed
- TICKET-3 must be completed

### Quality Gates
- ✅ Main method CYC ≤5
- ✅ All helpers CYC ≤8
- ✅ Zero lock() statements
- ✅ Build passes (dotnet build)
- ✅ Tests pass (dotnet test)
- ✅ Integration passes (F5 shows BUILD_TAG)

---

## Execution Notes

### Pre-Execution Checklist
- [ ] Run characterization tests to capture current behavior
- [ ] Verify jCodemunch index is fresh (avoid EPIC-CCN-1 failure mode)
- [ ] Ensure git status is clean before starting extraction
- [ ] Verify GitButler virtual branch active

### During Execution
- Extract helpers one at a time (sequential order)
- Run complexity audit after each extraction
- Run build after each extraction to catch compilation errors early
- Commit after each successful ticket with format: `[EPIC-CCN-016] TICKET-X: description -- CYC before->after`

### Post-Execution
- Run `powershell -File .\deploy-sync.ps1` to sync hard links
- F5 in NinjaTrader IDE to verify BUILD_TAG
- Run unit tests to verify behavioral preservation
- Update complexity audit baseline in epic_roadmap.json
- Update manifest.json with completion status

---

**Created**: 2026-06-16T06:10:09Z
**Epic**: EPIC-CCN-016
**Phase**: 4 (Ticket Generation)
**Total Tickets**: 4
**Estimated Effort**: 3-4 hours
**Complexity Reduction**: 19 → 5 (74% reduction)
