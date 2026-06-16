# Extraction Tickets: EPIC-CCN-033

## Overview
- **Total Tickets**: 3
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3)
- **Estimated Effort**: 4-6 hours
- **Target Method**: `FlattenSinglePosition`
- **Current CCN**: 16
- **Target CCN**: ≤8 (Jane Street strict standard)
- **File**: `src/V12_002.Orders.Management.Flatten.cs`

## TICKET-1: Extract CancelStopAndTargetOrders

### Scope
- **Current Method**: `FlattenSinglePosition`
- **Current CCN**: 16
- **Target CCN**: 13 (after this extraction)
- **Extraction**: Stop and target order cancellation logic

### Method Signature
```csharp
private void CancelStopAndTargetOrders(string entryName, PositionInfo pos)
```

### Implementation Steps
1. Create new private method `CancelStopAndTargetOrders` with signature above
2. Move stop cancellation logic:
   - `RequestStopCancelLifecycleSafe(entryName)` call
   - `pendingStopReplacements.TryRemove(entryName, out _)` with atomic decrement
3. Move target order cancellation loop:
   - Iterate through T1-T5 target orders
   - Call `CancelOrderSafe(tOrder, pos)` for working/accepted/submitted orders
   - Use `TryGetValue` on concurrent dictionaries
4. Update `FlattenSinglePosition` to call new helper method
5. Run `python scripts/complexity_audit.py` to verify CCN reduction
6. Run unit tests to verify behavioral equivalence
7. Commit checkpoint: "TICKET-1: Extract CancelStopAndTargetOrders (CCN 16→13)"

### Acceptance Criteria
- [ ] New method `CancelStopAndTargetOrders` created with CCN ≤3
- [ ] Stop cancellation logic moved (RequestStopCancelLifecycleSafe + TryRemove)
- [ ] Target cancellation loop moved (T1-T5 iteration)
- [ ] Main method calls new helper method
- [ ] Complexity audit shows CCN reduction (16→13)
- [ ] All unit tests pass
- [ ] No behavioral changes (integration test passes)
- [ ] Build succeeds (`dotnet build`)
- [ ] Lock-free compliance verified (no lock() statements)
- [ ] Checkpoint committed

### Lock-Free Validation
- Uses `TryRemove` on `ConcurrentDictionary` (lock-free)
- Uses `Interlocked.Decrement` for atomic counter update
- Uses `TryGetValue` on concurrent dictionaries
- No `lock()` statements

### Dependencies
- None (first ticket)

---

## TICKET-2: Extract ValidateAndCalculateFlattenQuantity

### Scope
- **Current Method**: `FlattenSinglePosition`
- **Current CCN**: 13 (after TICKET-1)
- **Target CCN**: 9 (after this extraction)
- **Extraction**: Position quantity validation and calculation logic

### Method Signature
```csharp
private int ValidateAndCalculateFlattenQuantity(PositionInfo pos)
```

### Implementation Steps
1. Create new private method `ValidateAndCalculateFlattenQuantity` with signature above
2. Move position validation logic:
   - Read live position quantity from `Position.Quantity` (with exception handling)
   - Compare cached `pos.RemainingContracts` with live quantity
   - Apply V10 FLATTEN FIX logic (trust cached contracts if live is 0)
   - Log diagnostic information for troubleshooting
3. Return safe flatten quantity (int)
4. Update `FlattenSinglePosition` to call new helper method and capture return value
5. Run `python scripts/complexity_audit.py` to verify CCN reduction
6. Run unit tests to verify behavioral equivalence
7. Commit checkpoint: "TICKET-2: Extract ValidateAndCalculateFlattenQuantity (CCN 13→9)"

### Acceptance Criteria
- [ ] New method `ValidateAndCalculateFlattenQuantity` created with CCN ≤4
- [ ] Position validation logic moved (try-catch + null checks)
- [ ] V10 FLATTEN FIX logic preserved (trust cached if live is 0)
- [ ] Diagnostic logging included
- [ ] Method returns int (flatten quantity)
- [ ] Main method calls new helper and uses return value
- [ ] Complexity audit shows CCN reduction (13→9)
- [ ] All unit tests pass
- [ ] No behavioral changes (integration test passes)
- [ ] Build succeeds (`dotnet build`)
- [ ] Lock-free compliance verified (read-only Position access)
- [ ] Checkpoint committed

### Lock-Free Validation
- Read-only access to `Position` property (NinjaTrader API, thread-safe)
- No shared mutable state
- Exception handling for safe property access
- No `lock()` statements

### Dependencies
- **TICKET-1** must be completed first

---

## TICKET-3: Extract SubmitFlattenMarketOrder

### Scope
- **Current Method**: `FlattenSinglePosition`
- **Current CCN**: 9 (after TICKET-2)
- **Target CCN**: ≤7 (after this extraction, meets Jane Street ≤8 threshold)
- **Extraction**: Market order creation and submission logic

### Method Signature
```csharp
private void SubmitFlattenMarketOrder(string entryName, PositionInfo pos, int flattenQty)
```

### Implementation Steps
1. Create new private method `SubmitFlattenMarketOrder` with signature above
2. Move order submission logic:
   - Validate `flattenQty > 0` before submission
   - Determine order direction based on `pos.Direction` (Long→ExitLong, Short→ExitShort)
   - Call `SubmitOrderUnmanaged` with appropriate parameters
   - Store order reference in `flattenOrders` dictionary
   - Log order submission
3. Update `FlattenSinglePosition` to call new helper method with `flattenQty` parameter
4. Run `python scripts/complexity_audit.py` to verify final CCN ≤8
5. Run unit tests to verify behavioral equivalence
6. Commit checkpoint: "TICKET-3: Extract SubmitFlattenMarketOrder (CCN 9→7)"

### Acceptance Criteria
- [ ] New method `SubmitFlattenMarketOrder` created with CCN ≤2
- [ ] Quantity validation moved (flattenQty > 0 check)
- [ ] Direction logic moved (Long/Short to ExitLong/ExitShort)
- [ ] Order submission moved (SubmitOrderUnmanaged call)
- [ ] Dictionary storage moved (flattenOrders[entryName] = order)
- [ ] Main method calls new helper with flattenQty parameter
- [ ] Complexity audit shows final CCN ≤8 (Jane Street threshold met)
- [ ] All unit tests pass
- [ ] No behavioral changes (integration test passes)
- [ ] Build succeeds (`dotnet build`)
- [ ] Lock-free compliance verified (ConcurrentDictionary usage)
- [ ] Checkpoint committed

### Lock-Free Validation
- Uses `ConcurrentDictionary` for `flattenOrders` storage
- Calls NinjaTrader API (assumed thread-safe)
- No shared mutable state beyond concurrent collections
- No `lock()` statements

### Dependencies
- **TICKET-1** must be completed first
- **TICKET-2** must be completed first

---

## Final Verification Checklist

After completing all 3 tickets:

### Complexity Verification
- [ ] Run `python scripts/complexity_audit.py`
- [ ] Confirm `FlattenSinglePosition` CCN ≤8
- [ ] Confirm all extracted methods CCN ≤8
- [ ] Total CCN distributed: ~16 across 4 methods

### Build & Test Verification
- [ ] Run `powershell -File .\scripts\build_readiness.ps1`
- [ ] All unit tests pass (FSMActorTests + new extraction tests)
- [ ] Integration test passes (full FlattenSinglePosition flow)
- [ ] Stress test passes (concurrent flatten requests)

### Pre-Push Validation
- [ ] Run `powershell -File .\scripts\pre_push_validation.ps1 -Fast`
- [ ] ASCII-only compliance verified
- [ ] Build succeeds (zero errors)
- [ ] Unit tests pass (100%)
- [ ] Lint passes (zero violations)
- [ ] Formatting passes (CSharpier)

### Hard-Link Sync
- [ ] Run `powershell -File .\deploy-sync.ps1`
- [ ] Verify NinjaTrader hard links synchronized
- [ ] Test in NinjaTrader (F5 reload)

### PR Hygiene
- [ ] Diff size <10k characters (estimated ~2,400)
- [ ] Single file modified (src/V12_002.Orders.Management.Flatten.cs)
- [ ] No whitespace mutations
- [ ] No scope creep (only FlattenSinglePosition touched)

## Execution Strategy

### Sequential Extraction
1. **TICKET-1 First**: Extract cancellation logic (highest CCN contribution)
2. **TICKET-2 Second**: Extract validation logic (moderate CCN contribution)
3. **TICKET-3 Last**: Extract submission logic (lowest CCN contribution)

### Checkpoint Strategy
- Commit after each successful ticket completion
- Use Bob CLI `/restore` if extraction fails
- Verify tests pass before moving to next ticket

### Testing Strategy
- **Unit Tests**: Test each extracted method in isolation
- **Integration Test**: Verify full FlattenSinglePosition flow after each extraction
- **Stress Test**: Validate lock-free compliance under concurrent load (final verification)

## Success Metrics

### Quantitative
- **CCN Reduction**: 16 → ≤7 (56% reduction)
- **Method Count**: 1 → 4 (3 new helpers)
- **Max Method CCN**: 16 → ≤7 (meets Jane Street ≤8 threshold)
- **Diff Size**: ~2,400 characters (well under 10k limit)

### Qualitative
- **Cognitive Simplicity**: Each method has single responsibility
- **Testability**: Isolated methods enable exhaustive unit testing
- **Maintainability**: Clear separation of concerns
- **Lock-Free**: Zero contention on critical path

---

**Phase**: 4.0 (Ticket Generation)
**Status**: COMPLETE
**Next Phase**: 5.0 (Ticket Execution via Bob CLI)
**Date**: 2026-06-15
**Ticket Generator**: Bob CLI (v12-engineer mode)
