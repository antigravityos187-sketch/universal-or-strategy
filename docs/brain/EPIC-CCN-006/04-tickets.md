# Extraction Tickets: EPIC-CCN-006

## Overview
- **Total Tickets**: 3
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3)
- **Estimated Effort**: 4-6 hours
- **Target Method**: `AdoptFleetWorkingOrders`
- **Current CYC**: 17
- **Target CYC**: 6
- **File**: `src/V12_002.SIMA.Lifecycle.cs`

## TICKET-1: Extract IsValidFleetOrder (Validation Logic)

### Scope
- **Current Method**: `AdoptFleetWorkingOrders`
- **Current CYC**: 17
- **Target CYC**: 6 (after all extractions)
- **This Extraction CYC**: 6
- **Lines**: 471-486 (validation block)

### Implementation
1. Create new private method `IsValidFleetOrder(Order ord)` returning `bool`
2. Extract instrument validation logic:
   - Check if `ord.Instrument == Instrument`
3. Extract order state validation logic (5 valid states):
   - `OrderState.Working`
   - `OrderState.Accepted`
   - `OrderState.PendingSubmit`
   - `OrderState.PendingChange`
   - `OrderState.PendingCancel`
4. Return `true` if both instrument and state are valid, `false` otherwise
5. Replace validation block in `AdoptFleetWorkingOrders` with call to `IsValidFleetOrder(ord)`
6. Verify no behavior changes (exact logic preservation)

### Method Signature
```csharp
private bool IsValidFleetOrder(Order ord)
```

### Acceptance Criteria
- [ ] Method `IsValidFleetOrder` created with CYC ≤ 6
- [ ] Instrument validation logic extracted
- [ ] Order state validation logic extracted (all 5 states)
- [ ] Main method calls `IsValidFleetOrder(ord)` instead of inline validation
- [ ] All tests pass (no behavioral changes)
- [ ] Build succeeds (dotnet build)
- [ ] CSharpier formatting applied
- [ ] No lock() statements introduced
- [ ] Complexity audit passes (CYC ≤ 15)

### Dependencies
- None (first ticket)

### Testing Strategy
- Unit test: Valid instrument + valid state → returns `true`
- Unit test: Invalid instrument → returns `false`
- Unit test: Valid instrument + invalid state → returns `false`
- Unit test: Each of 5 valid states individually → returns `true`
- Integration test: Existing adoption flow still works

---

## TICKET-2: Extract ProcessAdoptedOrder (Processing Logic)

### Scope
- **Current Method**: `AdoptFleetWorkingOrders`
- **Current CYC**: After TICKET-1 completion
- **Target CYC**: 6 (after all extractions)
- **This Extraction CYC**: 4
- **Lines**: 488-509 (processing block)

### Implementation
1. Create new private method `ProcessAdoptedOrder(Order ord, Account acct, ref int adoptedCount)`
2. Extract order classification logic:
   - Call `ClassifyAndRouteFleetOrder(ord, acct)`
   - Store result in `classification` variable
3. Extract null check validation:
   - If `classification == null`, return early
4. Extract atomic storage logic:
   - Store order in `_fleetWorkingOrders` ConcurrentDictionary
5. Extract position synchronization logic:
   - If `classification.Position != null`, call `RebuildActivePositionForFleetEntry(classification.Position)`
   - Else call `SyncExistingPositionMetadata(ord)`
6. Extract success logging:
   - Log adoption success message
7. Extract counter increment:
   - Increment `adoptedCount` by reference
8. Replace processing block in `AdoptFleetWorkingOrders` with call to `ProcessAdoptedOrder(ord, acct, ref adoptedCount)`

### Method Signature
```csharp
private void ProcessAdoptedOrder(Order ord, Account acct, ref int adoptedCount)
```

### Acceptance Criteria
- [ ] Method `ProcessAdoptedOrder` created with CYC ≤ 4
- [ ] Classification logic extracted (calls `ClassifyAndRouteFleetOrder`)
- [ ] Null check validation extracted
- [ ] Atomic storage logic extracted (ConcurrentDictionary)
- [ ] Position synchronization logic extracted (conditional branching)
- [ ] Success logging extracted
- [ ] Counter increment extracted (ref parameter)
- [ ] Main method calls `ProcessAdoptedOrder(ord, acct, ref adoptedCount)`
- [ ] All tests pass (no behavioral changes)
- [ ] Build succeeds (dotnet build)
- [ ] CSharpier formatting applied
- [ ] No lock() statements introduced
- [ ] Complexity audit passes (CYC ≤ 15)

### Dependencies
- TICKET-1 must be completed first

### Testing Strategy
- Unit test: Valid classification → order stored, position synced, counter incremented
- Unit test: Null classification → early return, no storage, counter unchanged
- Unit test: Position exists → `RebuildActivePositionForFleetEntry` called
- Unit test: Position null → `SyncExistingPositionMetadata` called
- Integration test: Adoption flow with multiple orders

---

## TICKET-3: Extract LogAdoptionError (Error Handling)

### Scope
- **Current Method**: `AdoptFleetWorkingOrders`
- **Current CYC**: After TICKET-2 completion
- **Target CYC**: 6 (final)
- **This Extraction CYC**: 1
- **Lines**: 512-520 (catch block)

### Implementation
1. Create new private method `LogAdoptionError(Account acct, Exception ex)`
2. Extract error logging logic:
   - Format: "SIMA HYDRATE WARNING: Failed to adopt fleet working orders for account {acct.Name}: {ex.Message}"
   - Output: Print to NinjaTrader log
3. Replace catch block in `AdoptFleetWorkingOrders` with call to `LogAdoptionError(acct, ex)`
4. Verify error handling behavior unchanged

### Method Signature
```csharp
private void LogAdoptionError(Account acct, Exception ex)
```

### Acceptance Criteria
- [ ] Method `LogAdoptionError` created with CYC = 1
- [ ] Error message formatting extracted
- [ ] Log output extracted (NinjaTrader Print)
- [ ] Main method calls `LogAdoptionError(acct, ex)` in catch block
- [ ] All tests pass (no behavioral changes)
- [ ] Build succeeds (dotnet build)
- [ ] CSharpier formatting applied
- [ ] No lock() statements introduced
- [ ] Complexity audit passes (CYC ≤ 15)
- [ ] **Final CYC verification**: `AdoptFleetWorkingOrders` CYC = 6 ✅

### Dependencies
- TICKET-2 must be completed first

### Testing Strategy
- Unit test: Exception thrown → error logged with correct format
- Unit test: Account name included in error message
- Unit test: Exception message included in error message
- Integration test: Error during adoption → logged correctly, no crash

---

## Final Verification Checklist

### Post-Implementation (After All Tickets)
- [ ] Run `dotnet csharpier format src/` (auto-format)
- [ ] Run `dotnet csharpier check src/` (verify formatting)
- [ ] Run `dotnet build` (zero errors)
- [ ] Run `dotnet test` (100% pass)
- [ ] Run `python scripts/complexity_audit.py` (CYC ≤ 15 for all methods)
- [ ] Run `grep -r "lock(" src/` (zero matches)
- [ ] Run `powershell -File .\scripts\pre_push_validation.ps1 -Fast` (all checks pass)
- [ ] Verify `AdoptFleetWorkingOrders` final CYC = 6
- [ ] Verify `IsValidFleetOrder` CYC = 6
- [ ] Verify `ProcessAdoptedOrder` CYC = 4
- [ ] Verify `LogAdoptionError` CYC = 1
- [ ] Run `powershell -File .\deploy-sync.ps1` (NinjaTrader hard-link sync)

### DNA Compliance Verification
- [ ] Correctness by Construction: Type safety maintained ✅
- [ ] Lock-Free Actor Pattern: Zero lock() statements ✅
- [ ] ASCII-Only Compliance: No Unicode characters ✅
- [ ] Jane Street Alignment: All methods CYC ≤ 8 ✅

### PR Hygiene Verification
- [ ] Diff size < 10,000 characters ✅
- [ ] Single method focus (no scope creep) ✅
- [ ] Build readiness (no breaking changes) ✅

---

## Execution Notes

### Sequential Execution Required
Tickets MUST be executed in order (1 → 2 → 3) because:
1. TICKET-1 simplifies validation logic first
2. TICKET-2 builds on simplified main method
3. TICKET-3 completes the extraction with error handling

### Rollback Strategy
If any ticket fails:
1. Use Bob CLI `/restore` to revert to last checkpoint
2. Review failure reason
3. Adjust implementation approach
4. Retry ticket

### Performance Validation
After all tickets complete:
1. Benchmark adoption flow (before/after should be identical)
2. Verify no new allocations introduced
3. Confirm microsecond-latency characteristics preserved

---

**Ticket Generation Date**: 2026-06-15  
**Protocol Version**: V12.23  
**Phase**: 4 (Ticket Generation)  
**Status**: READY FOR EXECUTION
