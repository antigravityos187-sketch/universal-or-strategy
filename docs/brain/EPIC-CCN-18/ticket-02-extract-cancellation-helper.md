# EPIC-CCN-18 Ticket 2: Extract Cancellation Helper

## Metadata
- **Epic**: EPIC-CCN-18
- **Ticket**: 2 of 4
- **Title**: Extract CancelOrphanedOrdersForPosition
- **CYC Reduction**: 23 → 13 (-10, 43%)
- **Effort**: 3-4 hours
- **BUILD_TAG**: `1111.044-epic-ccn-18-t2`
- **Mode**: `v12-engineer` (Bob CLI)
- **Dependency**: Ticket 1 MUST be complete and F5-verified
- **Status**: READY FOR EXECUTION (after Ticket 1)

---

## Objective

Extract complex order cancellation logic from [`HandleFlatPositionUpdate()`](src/V12_002.Orders.Callbacks.Execution.cs:69) to reduce cyclomatic complexity from 23 to 13. This helper cancels stop and target orders for orphaned positions after external close.

**Success Metric**: CYC 23 → 13 (-10 points, 43% reduction)

---

## Extraction Specification

### Helper 3: `CancelOrphanedOrdersForPosition()`

**Signature**:
```csharp
private void CancelOrphanedOrdersForPosition(string posKey, PositionInfo pos)
```

**Purpose**: Cancel stop and target orders for orphaned position after external close

**Parameters**:
- `posKey` (string): Position key (e.g., "Sim101_ES 03-25")
- `pos` (PositionInfo): Position metadata for cancellation context

**Returns**: void (side effects via `CancelOrderSafe`)

**Estimated CYC**: 10 (stop check + 5-iteration loop + target checks)

**Logic** (Lines 144-166):
```csharp
private void CancelOrphanedOrdersForPosition(string posKey, PositionInfo pos)
{
    // Cancel stop order if active
    if (stopOrders.TryGetValue(posKey, out var stopOrder))
    {
        if (
            stopOrder != null
            && (
                stopOrder.OrderState == OrderState.Working
                || stopOrder.OrderState == OrderState.Accepted
            )
        )
        {
            CancelOrderSafe(stopOrder, pos);
        }
    }

    // Cancel all 5 target orders if active
    for (int tNum = 1; tNum <= 5; tNum++)
    {
        var tDict = GetTargetOrdersDictionary(tNum);
        if (tDict != null && tDict.TryGetValue(posKey, out var tOrder))
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
            }
        }
    }
}
```

**Type**: Actor-serialized (modifies state via `CancelOrderSafe`)  
**Thread-Safety**: `CancelOrderSafe` uses Actor `Enqueue` pattern  
**Dependencies**: `stopOrders`, `GetTargetOrdersDictionary()`, `CancelOrderSafe()`

---

## TDD Test Plan (BEFORE Extraction)

**Total Tests**: 8 (4 stop + 4 target)

### Stop Order Tests (4 tests)

1. **`CancelOrphanedOrdersForPosition_WithWorkingStopOrder_CancelsStop`**
   - **Given**: Stop order exists with `Working` state
   - **When**: Call `CancelOrphanedOrdersForPosition(posKey, pos)`
   - **Then**: `CancelOrderSafe` called once for stop order

2. **`CancelOrphanedOrdersForPosition_WithAcceptedStopOrder_CancelsStop`**
   - **Given**: Stop order exists with `Accepted` state
   - **When**: Call `CancelOrphanedOrdersForPosition(posKey, pos)`
   - **Then**: `CancelOrderSafe` called once for stop order

3. **`CancelOrphanedOrdersForPosition_WithFilledStopOrder_DoesNotCancel`**
   - **Given**: Stop order exists with `Filled` state (terminal)
   - **When**: Call `CancelOrphanedOrdersForPosition(posKey, pos)`
   - **Then**: `CancelOrderSafe` NOT called for stop order

4. **`CancelOrphanedOrdersForPosition_WithNoStopOrder_DoesNotThrow`**
   - **Given**: No stop order exists for position key
   - **When**: Call `CancelOrphanedOrdersForPosition(posKey, pos)`
   - **Then**: No exception thrown, method completes successfully

---

### Target Order Tests (4 tests)

5. **`CancelOrphanedOrdersForPosition_WithWorkingTargetOrders_CancelsAll`**
   - **Given**: All 5 target orders exist with `Working` state
   - **When**: Call `CancelOrphanedOrdersForPosition(posKey, pos)`
   - **Then**: `CancelOrderSafe` called 5 times (once per target)

6. **`CancelOrphanedOrdersForPosition_WithAcceptedTargetOrders_CancelsAll`**
   - **Given**: All 5 target orders exist with `Accepted` state
   - **When**: Call `CancelOrphanedOrdersForPosition(posKey, pos)`
   - **Then**: `CancelOrderSafe` called 5 times (once per target)

7. **`CancelOrphanedOrdersForPosition_WithFilledTargetOrders_DoesNotCancel`**
   - **Given**: All 5 target orders exist with `Filled` state (terminal)
   - **When**: Call `CancelOrphanedOrdersForPosition(posKey, pos)`
   - **Then**: `CancelOrderSafe` NOT called for any target order

8. **`CancelOrphanedOrdersForPosition_WithMissingTargetOrders_DoesNotThrow`**
   - **Given**: No target orders exist for position key
   - **When**: Call `CancelOrphanedOrdersForPosition(posKey, pos)`
   - **Then**: No exception thrown, method completes successfully

---

## Implementation Steps

1. **Write TDD Tests** (8 tests)
   - Add to test file: `tests/V12_Performance.Tests/Orders/HandleFlatPositionUpdateTests.cs`
   - Write all 8 tests BEFORE extraction
   - Verify tests compile with errors (method doesn't exist yet)
   - Commit: `EPIC-CCN-18 T2: Add TDD tests for cancellation helper (8 tests)`

2. **Extract Helper 3: `CancelOrphanedOrdersForPosition()`**
   - Add method after `HasActivePositionForAccount()` in `src/V12_002.Orders.Callbacks.Execution.cs`
   - Copy lines 144-166 logic into helper
   - Verify method signature matches specification

3. **Replace Integration Point 3** (Lines 144-166)
   - **BEFORE**:
     ```csharp
     if (pos.EntryFilled && pos.RemainingContracts > 0)
     {
         Print("EXTERNAL CLOSE DETECTED - Position went flat. Cancelling orphaned orders...");
         if (stopOrders.TryGetValue(kvp.Key, out var stopOrder))
         {
             if (
                 stopOrder != null
                 && (
                     stopOrder.OrderState == OrderState.Working
                     || stopOrder.OrderState == OrderState.Accepted
                 )
             )
                 CancelOrderSafe(stopOrder, pos);
         }
         for (int tNum = 1; tNum <= 5; tNum++)
         {
             var tDict = GetTargetOrdersDictionary(tNum);
             if (tDict != null && tDict.TryGetValue(kvp.Key, out var tOrder))
             {
                 if (
                     tOrder != null
                     && (tOrder.OrderState == OrderState.Working || tOrder.OrderState == OrderState.Accepted)
                 )
                     CancelOrderSafe(tOrder, pos);
             }
         }
         positionsToCleanup.Add(kvp.Key);
     }
     ```
   - **AFTER**:
     ```csharp
     if (pos.EntryFilled && pos.RemainingContracts > 0)
     {
         Print("EXTERNAL CLOSE DETECTED - Position went flat. Cancelling orphaned orders...");
         CancelOrphanedOrdersForPosition(kvp.Key, pos);
         positionsToCleanup.Add(kvp.Key);
     }
     ```

4. **Run Build**
   - Execute: `dotnet build`
   - Verify: Zero compilation errors

5. **Run Tests**
   - Execute: `dotnet test tests/V12_Performance.Tests/`
   - Verify: All 19 tests pass (11 from Ticket 1 + 8 from Ticket 2)

6. **Run Complexity Audit**
   - Execute: `python scripts/complexity_audit.py`
   - Verify: `HandleFlatPositionUpdate` CYC = 13 (reduced from 23)
   - Verify: `CancelOrphanedOrdersForPosition` CYC ≤10

7. **Run CSharpier**
   - Execute: `dotnet csharpier format src/`
   - Verify: Zero formatting issues

8. **Update BUILD_TAG**
   - Edit `src/V12_002.cs` line 1
   - Change: `// BUILD_TAG: 1111.043-epic-ccn-18-t1` → `// BUILD_TAG: 1111.044-epic-ccn-18-t2`

9. **Commit Extraction**
   - Message: `EPIC-CCN-18 Ticket 2: Extract orphaned order cancellation (CYC 23→13)`
   - Body:
     ```
     - Extract CancelOrphanedOrdersForPosition() helper (CYC 10)
     - Add 8 TDD tests (4 stop + 4 target)
     - Reduce main method CYC from 23 to 13 (-10, 43% reduction)
     - Preserve Actor model thread-safety
     - Zero logic drift, pure structural refactoring
     ```

10. **Run Hard-Link Sync**
    - Execute: `powershell -File .\deploy-sync.ps1`
    - Verify: NinjaTrader hard links updated successfully

11. **STOP for F5 Verification**
    - **DO NOT PROCEED** to Ticket 3 until F5 verification passes
    - Load strategy in NinjaTrader
    - Verify: No runtime errors
    - Verify: Order cancellation behavior unchanged

---

## Integration Points

### Integration Point 3: Orphaned Order Cancellation (Lines 144-166)
- **Location**: Line 144
- **Context**: Inside `if (pos.EntryFilled && pos.RemainingContracts > 0)` block
- **Preserved**: `Print()` statement (line 143)
- **Preserved**: `positionsToCleanup.Add()` (line 167)
- **Replacement**: Single helper call
- **Verification**: Print statement preserved, cleanup list management preserved

---

## Verification Checklist

- [ ] Build passes (zero errors)
- [ ] All 19 tests pass (11 + 8)
- [ ] Complexity reduced (23 → 13)
- [ ] Helper 3 CYC ≤10
- [ ] CSharpier formatted
- [ ] BUILD_TAG updated to `1111.044-epic-ccn-18-t2`
- [ ] deploy-sync.ps1 executed successfully
- [ ] F5 verification PASSED (NinjaTrader loads without errors)
- [ ] Order cancellation behavior unchanged
- [ ] Actor model preserved (no new `lock()` statements)

---

## Success Criteria

**Quantitative**:
- ✅ Main method CYC = 13 (verified via `complexity_audit.py`)
- ✅ Helper 3 CYC ≤10
- ✅ All 19 tests passing (11 + 8)
- ✅ Zero compilation errors

**Qualitative**:
- ✅ Zero logic drift (pure structural refactoring)
- ✅ Actor model preserved (`CancelOrderSafe` uses `Enqueue`)
- ✅ Print statement preserved (user-facing diagnostic)
- ✅ Cleanup orchestration preserved

---

## Rollback Plan

**If F5 verification fails**:

1. **STOP immediately** (do not proceed to Ticket 3)
2. **Revert commit**: `git revert HEAD`
3. **Document failure**: Create `docs/brain/EPIC-CCN-18/ticket-02-failure.md`
4. **Analyze root cause**:
   - Order cancellation logic drift?
   - Test gap for edge case?
   - Actor model violation?
5. **Fix in separate session**:
   - Address root cause
   - Re-run TDD tests
   - Verify fix locally
6. **Re-execute Ticket 2** with corrected approach
7. **Report to Director** before proceeding to Ticket 3

---

## References

- **Master Tickets**: `docs/brain/EPIC-CCN-18/04-tickets.md`
- **Architecture Plan**: `docs/brain/EPIC-CCN-18/02-architecture-plan.md` (Section 1.3, 3.3)
- **Audit Report**: `docs/brain/EPIC-CCN-18/03-audit-report.md` (Section: Lock-Free Patterns)
- **Source File**: `src/V12_002.Orders.Callbacks.Execution.cs` (Lines 144-166)
- **Test File**: `tests/V12_Performance.Tests/Orders/HandleFlatPositionUpdateTests.cs`

---

**[TICKET-2-GATE]** Ready for execution via `epic-validate EPIC-CCN-18 --ticket 2` (after Ticket 1 F5 verification)