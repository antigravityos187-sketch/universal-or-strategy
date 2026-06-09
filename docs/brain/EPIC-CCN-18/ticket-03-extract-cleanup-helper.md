# EPIC-CCN-18 Ticket 3: Extract Cleanup Helper

## Metadata
- **Epic**: EPIC-CCN-18
- **Ticket**: 3 of 4
- **Title**: Extract CollectPositionsForCleanup
- **CYC Reduction**: 13 → 7 (-6, 46%)
- **Effort**: 2-3 hours
- **BUILD_TAG**: `1111.045-epic-ccn-18-t3`
- **Mode**: `v12-engineer` (Bob CLI)
- **Dependency**: Ticket 2 MUST be complete and F5-verified
- **Status**: READY FOR EXECUTION (after Ticket 2)

---

## Objective

Extract position cleanup orchestration from [`HandleFlatPositionUpdate()`](src/V12_002.Orders.Callbacks.Execution.cs:69) to reduce cyclomatic complexity from 13 to 7. This helper identifies positions requiring cleanup after external close and orchestrates cancellation via Helper 3.

**Success Metric**: CYC 13 → 7 (-6 points, 46% reduction)  
**Target Achievement**: CYC 7 ≤10 (Jane Street aligned) ✅

---

## Extraction Specification

### Helper 4: `CollectPositionsForCleanup()`

**Signature**:
```csharp
private List<string> CollectPositionsForCleanup()
```

**Purpose**: Identify positions requiring cleanup after external close

**Parameters**: None (operates on instance state)

**Returns**:
- `List<string>`: Position keys requiring cleanup (empty list if none)

**Estimated CYC**: 7 (loop + concurrent check + entry check + helper call)

**Logic** (Lines 136-169, refactored):
```csharp
private List<string> CollectPositionsForCleanup()
{
    List<string> positionsToCleanup = new List<string>();
    
    foreach (var kvp in activePositions.ToArray())
    {
        // Concurrent safety check
        if (!activePositions.ContainsKey(kvp.Key))
            continue;
            
        PositionInfo pos = kvp.Value;
        
        // Check if position was externally closed
        if (pos.EntryFilled && pos.RemainingContracts > 0)
        {
            Print("EXTERNAL CLOSE DETECTED - Position went flat. Cancelling orphaned orders...");
            
            // Cancel orphaned orders for this position
            CancelOrphanedOrdersForPosition(kvp.Key, pos);
            
            // Mark for cleanup
            positionsToCleanup.Add(kvp.Key);
        }
    }
    
    return positionsToCleanup;
}
```

**Type**: Orchestration function (calls Helper 3, returns list)  
**Thread-Safety**: Uses `ToArray()` snapshot + `ContainsKey()` guard  
**Dependencies**: `activePositions`, `CancelOrphanedOrdersForPosition()`, `Print()`

---

## TDD Test Plan (BEFORE Extraction)

**Total Tests**: 6

1. **`CollectPositionsForCleanup_WithFlatPositions_ReturnsAll`**
   - **Given**: 3 positions with `EntryFilled = true` and `RemainingContracts > 0`
   - **When**: Call `CollectPositionsForCleanup()`
   - **Then**: Returns list with 3 position keys

2. **`CollectPositionsForCleanup_WithNoFlatPositions_ReturnsEmpty`**
   - **Given**: All positions have `RemainingContracts = 0`
   - **When**: Call `CollectPositionsForCleanup()`
   - **Then**: Returns empty list

3. **`CollectPositionsForCleanup_WithMixedPositions_ReturnsOnlyFlat`**
   - **Given**: 2 flat positions + 2 active positions
   - **When**: Call `CollectPositionsForCleanup()`
   - **Then**: Returns list with 2 flat position keys only

4. **`CollectPositionsForCleanup_WithMultipleAccounts_ReturnsAll`**
   - **Given**: Flat positions across multiple accounts
   - **When**: Call `CollectPositionsForCleanup()`
   - **Then**: Returns all flat positions regardless of account

5. **`CollectPositionsForCleanup_PreservesToArrayPattern_HandlesModification`**
   - **Given**: Position removed from `activePositions` during iteration
   - **When**: Call `CollectPositionsForCleanup()`
   - **Then**: No exception thrown, `ContainsKey()` guard prevents access

6. **`CollectPositionsForCleanup_WithEmptyCollection_ReturnsEmpty`**
   - **Given**: `activePositions` dictionary is empty
   - **When**: Call `CollectPositionsForCleanup()`
   - **Then**: Returns empty list

---

## Implementation Steps

1. **Write TDD Tests** (6 tests)
   - Add to test file: `tests/V12_Performance.Tests/Orders/HandleFlatPositionUpdateTests.cs`
   - Write all 6 tests BEFORE extraction
   - Verify tests compile with errors (method doesn't exist yet)
   - Commit: `EPIC-CCN-18 T3: Add TDD tests for cleanup helper (6 tests)`

2. **Extract Helper 4: `CollectPositionsForCleanup()`**
   - Add method after `CancelOrphanedOrdersForPosition()` in `src/V12_002.Orders.Callbacks.Execution.cs`
   - Move lines 136-169 logic into helper
   - Helper calls `CancelOrphanedOrdersForPosition()` (Ticket 2 output)
   - Verify method signature matches specification

3. **Replace Integration Point 4** (Lines 136-169)
   - **BEFORE**:
     ```csharp
     List<string> positionsToCleanup = new List<string>();
     foreach (var kvp in activePositions.ToArray())
     {
         if (!activePositions.ContainsKey(kvp.Key))
             continue;
         PositionInfo pos = kvp.Value;
         if (pos.EntryFilled && pos.RemainingContracts > 0)
         {
             Print("EXTERNAL CLOSE DETECTED - Position went flat. Cancelling orphaned orders...");
             CancelOrphanedOrdersForPosition(kvp.Key, pos);
             positionsToCleanup.Add(kvp.Key);
         }
     }
     
     foreach (string key in positionsToCleanup)
         CleanupPosition(key);
     ```
   - **AFTER**:
     ```csharp
     List<string> positionsToCleanup = CollectPositionsForCleanup();
     
     foreach (string key in positionsToCleanup)
         CleanupPosition(key);
     ```

4. **Run Build**
   - Execute: `dotnet build`
   - Verify: Zero compilation errors

5. **Run Tests**
   - Execute: `dotnet test tests/V12_Performance.Tests/`
   - Verify: All 25 tests pass (11 + 8 + 6)

6. **Run Complexity Audit**
   - Execute: `python scripts/complexity_audit.py`
   - Verify: `HandleFlatPositionUpdate` CYC = 7 (reduced from 13)
   - Verify: `CollectPositionsForCleanup` CYC ≤7
   - **TARGET ACHIEVED**: CYC 7 ≤10 (Jane Street aligned) ✅

7. **Run CSharpier**
   - Execute: `dotnet csharpier format src/`
   - Verify: Zero formatting issues

8. **Update BUILD_TAG**
   - Edit `src/V12_002.cs` line 1
   - Change: `// BUILD_TAG: 1111.044-epic-ccn-18-t2` → `// BUILD_TAG: 1111.045-epic-ccn-18-t3`

9. **Commit Extraction**
   - Message: `EPIC-CCN-18 Ticket 3: Extract position cleanup collection (CYC 13→7)`
   - Body:
     ```
     - Extract CollectPositionsForCleanup() helper (CYC 7)
     - Add 6 TDD tests
     - Reduce main method CYC from 13 to 7 (-6, 46% reduction)
     - Helper orchestrates cleanup via CancelOrphanedOrdersForPosition()
     - Zero logic drift, pure structural refactoring
     - TARGET ACHIEVED: CYC ≤10 (Jane Street aligned)
     ```

10. **Run Hard-Link Sync**
    - Execute: `powershell -File .\deploy-sync.ps1`
    - Verify: NinjaTrader hard links updated successfully

11. **STOP for F5 Verification**
    - **DO NOT PROCEED** to Ticket 4 until F5 verification passes
    - Load strategy in NinjaTrader
    - Verify: No runtime errors
    - Verify: Position cleanup behavior unchanged

---

## Integration Points

### Integration Point 4: Position Cleanup Collection (Lines 136-169)
- **Location**: Line 136
- **Preserved**: Cleanup execution loop (lines 171-172)
- **Preserved**: Final print statement (lines 174-175)
- **Replacement**: Single helper call
- **Verification**: List initialization moved to helper, cleanup loop preserved

---

## Verification Checklist

- [ ] Build passes (zero errors)
- [ ] All 25 tests pass (11 + 8 + 6)
- [ ] Complexity reduced (13 → 7)
- [ ] Helper 4 CYC ≤7
- [ ] CSharpier formatted
- [ ] BUILD_TAG updated to `1111.045-epic-ccn-18-t3`
- [ ] deploy-sync.ps1 executed successfully
- [ ] F5 verification PASSED (NinjaTrader loads without errors)
- [ ] Position cleanup behavior unchanged
- [ ] **TARGET ACHIEVED**: Main method CYC = 7 ≤10 ✅

---

## Success Criteria

**Quantitative**:
- ✅ Main method CYC = 7 (verified via `complexity_audit.py`)
- ✅ Helper 4 CYC ≤7
- ✅ All 25 tests passing (11 + 8 + 6)
- ✅ Zero compilation errors
- ✅ **TARGET ACHIEVED**: CYC 7 ≤10 (Jane Street aligned)

**Qualitative**:
- ✅ Zero logic drift (pure structural refactoring)
- ✅ Orchestration pattern preserved (calls Helper 3)
- ✅ Thread-safety preserved (`ToArray()` + `ContainsKey()`)
- ✅ Print statement preserved (user-facing diagnostic)
- ✅ Cleanup execution preserved

---

## Rollback Plan

**If F5 verification fails**:

1. **STOP immediately** (do not proceed to Ticket 4)
2. **Revert commit**: `git revert HEAD`
3. **Document failure**: Create `docs/brain/EPIC-CCN-18/ticket-03-failure.md`
4. **Analyze root cause**:
   - Cleanup orchestration logic drift?
   - Test gap for concurrent scenario?
   - Helper 3 integration error?
5. **Fix in separate session**:
   - Address root cause
   - Re-run TDD tests
   - Verify fix locally
6. **Re-execute Ticket 3** with corrected approach
7. **Report to Director** before proceeding to Ticket 4

---

## Post-Completion Decision

**After F5 verification passes**:

1. **Run Complexity Audit**:
   ```powershell
   python scripts/complexity_audit.py
   ```

2. **Check CYC Value**:
   - **If CYC ≤10**: ✅ **SKIP TICKET 4** - Target achieved, proceed to Phase 6 (Final Review)
   - **If CYC >10**: ⚠️ Execute Ticket 4 (Final Refactoring)

**Expected Outcome**: CYC = 7, Ticket 4 NOT needed

---

## References

- **Master Tickets**: `docs/brain/EPIC-CCN-18/04-tickets.md`
- **Architecture Plan**: `docs/brain/EPIC-CCN-18/02-architecture-plan.md` (Section 1.4, 3.4)
- **Audit Report**: `docs/brain/EPIC-CCN-18/03-audit-report.md` (Section: Complexity Reduction)
- **Source File**: `src/V12_002.Orders.Callbacks.Execution.cs` (Lines 136-169)
- **Test File**: `tests/V12_Performance.Tests/Orders/HandleFlatPositionUpdateTests.cs`

---

**[TICKET-3-GATE]** Ready for execution via `epic-validate EPIC-CCN-18 --ticket 3` (after Ticket 2 F5 verification)