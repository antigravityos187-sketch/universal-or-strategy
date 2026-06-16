# Extraction Tickets: EPIC-CCN-036

## Overview
- **Total Tickets**: 4
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3 → TICKET-4)
- **Estimated Effort**: 6 hours
- **Method**: MoveStop_SinglePosition
- **File**: src/V12_002.Trailing.Breakeven.cs
- **Current CYC**: 13
- **Target CYC**: ≤8 (Jane Street strict standard)

---

## TICKET-1: Extract CalculateNewStopPrice Helper

### Scope
- **Current Method**: `MoveStop_SinglePosition`
- **Current CYC**: 13
- **Target CYC**: 11 (after this extraction)
- **Extraction**: Price calculation logic (lines 80-87)
- **Helper CYC**: ~2 (1 direction check)

### Implementation
1. **Write Unit Tests First (TDD)**
   - Test Long position: verify calculation with positive offset
   - Test Short position: verify calculation with negative offset
   - Test rounding: verify TickSize rounding behavior
   - Test edge cases: zero offset, negative prices

2. **Extract Helper Method**
   ```csharp
   private double CalculateNewStopPrice(PositionInfo pos, double offsetPoints)
   {
       double newStopPrice = pos.Direction == MarketPosition.Long
           ? pos.AveragePrice + offsetPoints
           : pos.AveragePrice - offsetPoints;
       
       return Instrument.MasterInstrument.RoundToTickSize(newStopPrice);
   }
   ```

3. **Refactor Main Method**
   - Replace lines 80-87 with: `double newStopPrice = CalculateNewStopPrice(pos, offsetPoints);`
   - Verify no behavioral changes

4. **Verification**
   - Run unit tests (100% pass)
   - Run `dotnet build` (zero errors)
   - Run `python scripts/complexity_audit.py` (verify CYC reduction)

### Acceptance Criteria
- [ ] Unit tests written and passing (4 test cases minimum)
- [ ] Helper method extracted with signature matching plan
- [ ] Main method refactored to use helper
- [ ] Method complexity reduced by 1 (13 → 12)
- [ ] All existing tests pass
- [ ] No behavioral changes (logic identical)
- [ ] Build succeeds (zero errors)
- [ ] CSharpier formatting applied

### Dependencies
- None (first ticket)

### Estimated Time
- 1.5 hours (0.5h tests + 0.5h extraction + 0.5h verification)

---

## TICKET-2: Extract IsPriceImprovement Helper

### Scope
- **Current Method**: `MoveStop_SinglePosition`
- **Current CYC**: 12 (after TICKET-1)
- **Target CYC**: 10 (after this extraction)
- **Extraction**: Price comparison logic (lines 96-98, 138-141 - duplicated)
- **Helper CYC**: ~2 (1 direction check)

### Implementation
1. **Write Unit Tests First (TDD)**
   - Test Long position improvement: newStop > currentStop
   - Test Long position no improvement: newStop ≤ currentStop
   - Test Short position improvement: newStop < currentStop
   - Test Short position no improvement: newStop ≥ currentStop
   - Test edge case: newStop == currentStop (both directions)

2. **Extract Helper Method**
   ```csharp
   private bool IsPriceImprovement(MarketPosition direction, double newStopPrice, double currentStopPrice)
   {
       return direction == MarketPosition.Long
           ? newStopPrice > currentStopPrice
           : newStopPrice < currentStopPrice;
   }
   ```

3. **Refactor Main Method (2 call sites)**
   - Replace lines 96-98 (follower path): `bool isBetterF = IsPriceImprovement(pos.Direction, newStopPrice, pos.CurrentStopPrice);`
   - Replace lines 138-141 (master path): `bool isBetter = IsPriceImprovement(pos.Direction, newStopPrice, pos.CurrentStopPrice);`
   - Verify DRY principle applied (eliminates duplication)

4. **Verification**
   - Run unit tests (100% pass)
   - Run `dotnet build` (zero errors)
   - Run `python scripts/complexity_audit.py` (verify CYC reduction)
   - Verify both call sites use helper

### Acceptance Criteria
- [ ] Unit tests written and passing (5 test cases minimum)
- [ ] Helper method extracted with signature matching plan
- [ ] Main method refactored at 2 call sites
- [ ] Method complexity reduced by 2 (12 → 10)
- [ ] DRY principle applied (duplication eliminated)
- [ ] All existing tests pass
- [ ] No behavioral changes (logic identical)
- [ ] Build succeeds (zero errors)
- [ ] CSharpier formatting applied

### Dependencies
- TICKET-1 must be completed first

### Estimated Time
- 2 hours (0.75h tests + 0.75h extraction + 0.5h verification)

---

## TICKET-3: Extract ValidatePriceCleared Helper

### Scope
- **Current Method**: `MoveStop_SinglePosition`
- **Current CYC**: 10 (after TICKET-2)
- **Target CYC**: 7 (after this extraction)
- **Extraction**: ARM guard threshold validation (lines 111-133)
- **Helper CYC**: ~3 (3 branches: stale price + direction check + cleared state)

### Implementation
1. **Write Unit Tests First (TDD)**
   - Test stale price (lastKnownPrice == 0): return false, no state mutation
   - Test Long position not cleared: return false, no state mutation
   - Test Long position cleared: return true, set ManualBreakevenArmed = true
   - Test Short position not cleared: return false, no state mutation
   - Test Short position cleared: return true, set ManualBreakevenArmed = true
   - Test edge case: price exactly at threshold (both directions)

2. **Extract Helper Method**
   ```csharp
   private bool ValidatePriceCleared(string entryName, PositionInfo pos, double newStopPrice, double lastKnownPrice)
   {
       // Stale price guard
       if (lastKnownPrice == 0)
       {
           return false;
       }
       
       // Check if price cleared threshold
       bool priceCleared = pos.Direction == MarketPosition.Long
           ? lastKnownPrice >= newStopPrice
           : lastKnownPrice <= newStopPrice;
       
       if (!priceCleared)
       {
           return false;
       }
       
       // ARM guard: set armed state
       pos.ManualBreakevenArmed = true;
       MarkStickyDirty();
       
       LogMessage($"[{entryName}] Manual breakeven ARMED at {newStopPrice:F2}");
       return true;
   }
   ```

3. **Refactor Main Method**
   - Replace lines 111-133 with: `if (!ValidatePriceCleared(entryName, pos, newStopPrice, lastKnownPrice)) { return; }`
   - Verify ARM guard semantics preserved
   - Verify early return behavior maintained

4. **Verification**
   - Run unit tests (100% pass)
   - Run `dotnet build` (zero errors)
   - Run `python scripts/complexity_audit.py` (verify CYC reduction)
   - Verify ARM guard behavior identical

### Acceptance Criteria
- [ ] Unit tests written and passing (6 test cases minimum)
- [ ] Helper method extracted with signature matching plan
- [ ] Main method refactored to use helper
- [ ] Method complexity reduced by 3 (10 → 7)
- [ ] ARM guard semantics preserved (V12.12 feature)
- [ ] Early return behavior maintained
- [ ] State mutation (ManualBreakevenArmed) verified
- [ ] All existing tests pass
- [ ] No behavioral changes (logic identical)
- [ ] Build succeeds (zero errors)
- [ ] CSharpier formatting applied

### Dependencies
- TICKET-2 must be completed first

### Estimated Time
- 2 hours (0.75h tests + 0.75h extraction + 0.5h verification)

---

## TICKET-4: Final Integration & Verification

### Scope
- **Current Method**: `MoveStop_SinglePosition`
- **Current CYC**: 7 (after TICKET-3)
- **Target CYC**: ≤8 ✅ (ACHIEVED)
- **Task**: End-to-end integration testing and verification

### Implementation
1. **Integration Test Suite**
   - Test follower path: verify early return with IsPriceImprovement
   - Test ARM guard path: verify early return with ValidatePriceCleared
   - Test master execution path: verify UpdateStopOrder called
   - Test end-to-end: Long position breakeven scenario
   - Test end-to-end: Short position breakeven scenario

2. **Complexity Verification**
   ```powershell
   python scripts/complexity_audit.py
   ```
   - Verify MoveStop_SinglePosition: CYC ≤8
   - Verify CalculateNewStopPrice: CYC ≤3
   - Verify IsPriceImprovement: CYC ≤3
   - Verify ValidatePriceCleared: CYC ≤3

3. **Pre-Push Validation**
   ```powershell
   powershell -File .\scripts\pre_push_validation.ps1
   ```
   - ASCII-only check (PASS)
   - Build check (PASS)
   - Unit tests (PASS)
   - Lint check (PASS)
   - Formatting check (PASS)

4. **Hard-Link Sync**
   ```powershell
   powershell -File .\deploy-sync.ps1
   ```
   - Sync src/ changes to NinjaTrader
   - Verify DIFF GUARD passes (<10k characters)

5. **NinjaTrader F5 Test**
   - Load strategy in NinjaTrader
   - Verify breakeven behavior (Long position)
   - Verify breakeven behavior (Short position)
   - Check for runtime errors
   - Verify ARM guard triggers correctly

### Acceptance Criteria
- [ ] Integration tests written and passing (5 test cases minimum)
- [ ] Complexity audit shows CYC ≤8 for main method
- [ ] Complexity audit shows CYC ≤3 for all helpers
- [ ] Pre-push validation passes (all checks)
- [ ] Hard-link sync succeeds (DIFF GUARD passes)
- [ ] NinjaTrader F5 test passes (no runtime errors)
- [ ] Breakeven behavior verified (Long + Short)
- [ ] ARM guard behavior verified
- [ ] Git diff shows isolated changes only
- [ ] No whitespace mutations
- [ ] Documentation updated (manifest.json)

### Dependencies
- TICKET-3 must be completed first

### Estimated Time
- 0.5 hours (integration tests + verification)

---

## Success Metrics

### Quantitative
- ✅ Main method complexity: 7 CYC (target: ≤8)
- ✅ Helper method complexity: ≤3 CYC each
- ✅ Total LOC: ~123 (original: 93, +3 helpers ~30 LOC)
- ✅ Zero new lock() statements
- ✅ Diff size: ~1,200 characters (target: <10,000)

### Qualitative
- ✅ Code reads like a recipe (Step 1, Step 2, etc.)
- ✅ Each helper has single, testable responsibility
- ✅ Direction logic centralized (DRY principle)
- ✅ ARM guard semantics preserved
- ✅ Jane Street alignment (cognitive simplicity)

### Test Coverage
- ✅ Unit tests: 15+ test cases (4 + 5 + 6)
- ✅ Integration tests: 5 test cases
- ✅ Total: 20+ test cases
- ✅ Coverage: 100% of extracted helpers

---

## Risk Mitigation

### Rollback Plan
- Bob CLI auto-checkpoint enabled (restore via `/restore`)
- Git revert available if F5 test fails
- Hard-link sync can be re-run to restore NinjaTrader state

### Verification Checkpoints
- After each ticket: Run unit tests + build
- After TICKET-3: Run complexity audit
- After TICKET-4: Run pre-push validation + F5 test

### Blockers
- None identified (audit report shows PASS)

---

## Metadata
- **Epic ID**: EPIC-CCN-036
- **Phase**: 4.0 (Ticket Generation)
- **Status**: READY FOR EXECUTION
- **Date**: 2026-06-15
- **Ticket Count**: 4
- **Total Estimated Time**: 6 hours
- **Next Phase**: 5.0 (Ticket Execution)
