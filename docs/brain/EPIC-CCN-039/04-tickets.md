# Extraction Tickets: EPIC-CCN-039

## Overview
- **Epic**: EPIC-CCN-039
- **Method**: ManageTrailingStops
- **File**: src/V12_002.Trailing.cs
- **Current Complexity**: 13
- **Target Complexity**: ~7
- **Total Tickets**: 3
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3)
- **Estimated Effort**: 4-6 hours (including tests)

---

## TICKET-1: Extract ShouldSkipPosition Helper

### Scope
- **Current Method**: `ManageTrailingStops`
- **Current CYC**: 13
- **Target CYC After Extraction**: ~10
- **Complexity Reduction**: -3 branches
- **Extraction**: Position validation logic into pure helper method

### Method Signature
```csharp
private bool ShouldSkipPosition(PositionInfo pos, string entryName)
```

### Implementation Steps
1. Create new private method `ShouldSkipPosition` below `ManageTrailingStops`
2. Extract validation logic:
   - Check `!pos.EntryFilled || !pos.BracketSubmitted`
   - Check `pos.IsFollower && SymmetryGuardIsAnchorPending(entryName)`
   - Return `true` if position should be skipped
3. Replace inline validation in `ManageTrailingStops` with helper call:
   ```csharp
   if (ShouldSkipPosition(pos, entryName)) continue;
   ```
4. Add XML documentation comment to helper method
5. Run `dotnet csharpier format src/V12_002.Trailing.cs`

### Acceptance Criteria
- [ ] Method complexity reduced from 13 to ~10
- [ ] Helper method is pure function (no side effects)
- [ ] Helper method has XML documentation
- [ ] All unit tests pass (FSMActorTests.cs)
- [ ] Build succeeds: `powershell -File .\scripts\build_readiness.ps1`
- [ ] No behavioral changes (integration test verification)
- [ ] CSharpier formatting applied
- [ ] Complexity verified: `python scripts/complexity_audit.py`

### Test Requirements
Create unit tests in `tests/V12_Performance.Tests/Core/TrailingStopsTests.cs`:
- Test case: EntryFilled=false → returns true
- Test case: BracketSubmitted=false → returns true
- Test case: IsFollower=true + SymmetryGuard pending → returns true
- Test case: All valid → returns false

### Dependencies
- None (first ticket)

### V12 DNA Compliance
- ✅ Pure function (Correctness by Construction)
- ✅ No locks required (Lock-Free Actor Pattern)
- ✅ ASCII-only (no Unicode)
- ✅ Cognitive simplicity (single concern)

---

## TICKET-2: Extract UpdateExtremePrice Helper

### Scope
- **Current Method**: `ManageTrailingStops`
- **Current CYC After TICKET-1**: ~10
- **Target CYC After Extraction**: ~8
- **Complexity Reduction**: -2 branches (ternary conditional)
- **Extraction**: Extreme price tracking logic into focused helper

### Method Signature
```csharp
private void UpdateExtremePrice(PositionInfo pos, double currentClose)
```

### Implementation Steps
1. Create new private method `UpdateExtremePrice` below `ShouldSkipPosition`
2. Extract extreme price logic:
   ```csharp
   pos.ExtremePriceSinceEntry = pos.MarketPosition == MarketPosition.Long
       ? Math.Max(pos.ExtremePriceSinceEntry, currentClose)
       : Math.Min(pos.ExtremePriceSinceEntry, currentClose);
   ```
3. Replace inline ternary in `ManageTrailingStops` with helper call:
   ```csharp
   UpdateExtremePrice(pos, Close[0]);
   ```
4. Add XML documentation comment to helper method
5. Run `dotnet csharpier format src/V12_002.Trailing.cs`

### Acceptance Criteria
- [ ] Method complexity reduced from ~10 to ~8
- [ ] Helper method uses atomic operations (Math.Max/Min)
- [ ] Helper method has XML documentation
- [ ] All unit tests pass
- [ ] Build succeeds: `powershell -File .\scripts\build_readiness.ps1`
- [ ] No behavioral changes (integration test verification)
- [ ] Thread safety verified (single-threaded per position)
- [ ] CSharpier formatting applied
- [ ] Complexity verified: `python scripts/complexity_audit.py`

### Test Requirements
Add unit tests to `tests/V12_Performance.Tests/Core/TrailingStopsTests.cs`:
- Test case: Long position, new high → updates ExtremePriceSinceEntry
- Test case: Long position, not new high → no change
- Test case: Short position, new low → updates ExtremePriceSinceEntry
- Test case: Short position, not new low → no change

### Dependencies
- TICKET-1 must be completed first

### V12 DNA Compliance
- ✅ Atomic operations only (Math.Max/Min)
- ✅ No locks required (Lock-Free Actor Pattern)
- ✅ ASCII-only (no Unicode)
- ✅ Single mutation (clear semantics)

---

## TICKET-3: Extract ShouldAllowPointBasedTrailing Helper

### Scope
- **Current Method**: `ManageTrailingStops`
- **Current CYC After TICKET-2**: ~8
- **Target CYC After Extraction**: ~7
- **Complexity Reduction**: -2 branches (boolean calculations)
- **Extraction**: Trade type filtering logic into pure helper

### Method Signature
```csharp
private bool ShouldAllowPointBasedTrailing(PositionInfo pos)
```

### Implementation Steps
1. Create new private method `ShouldAllowPointBasedTrailing` below `UpdateExtremePrice`
2. Extract trade type filtering logic:
   ```csharp
   bool isTrendOrRetestTrade = pos.TradeType == TradeType.TREND || pos.TradeType == TradeType.RETEST;
   bool allowPointBasedTrailing = isTrendOrRetestTrade && pos.TradeType != TradeType.RMA;
   return allowPointBasedTrailing;
   ```
3. Replace inline boolean calculations in `ManageTrailingStops` with helper call:
   ```csharp
   if (ShouldAllowPointBasedTrailing(pos))
   {
       ManageTrail_RunPointBasedTrailing(...);
   }
   ```
4. Add XML documentation comment to helper method
5. Run `dotnet csharpier format src/V12_002.Trailing.cs`

### Acceptance Criteria
- [ ] Method complexity reduced from ~8 to ~7 (target achieved)
- [ ] Helper method is pure function (no side effects)
- [ ] Helper method has XML documentation
- [ ] All unit tests pass
- [ ] Build succeeds: `powershell -File .\scripts\build_readiness.ps1`
- [ ] No behavioral changes (integration test verification)
- [ ] CSharpier formatting applied
- [ ] Complexity verified: `python scripts/complexity_audit.py`
- [ ] Final CYC ≤ 8 (Jane Street strict standard)

### Test Requirements
Add unit tests to `tests/V12_Performance.Tests/Core/TrailingStopsTests.cs`:
- Test case: TREND trade → returns true
- Test case: RETEST trade → returns true
- Test case: RMA trade → returns false
- Test case: Other trade types → returns false

### Dependencies
- TICKET-1 must be completed first
- TICKET-2 must be completed first

### V12 DNA Compliance
- ✅ Pure function (Correctness by Construction)
- ✅ No locks required (Lock-Free Actor Pattern)
- ✅ ASCII-only (no Unicode)
- ✅ Single concern (filtering logic only)

---

## Post-Extraction Validation

### Required Checks (After All Tickets Complete)
1. **Build Readiness**: `powershell -File .\scripts\build_readiness.ps1`
2. **Pre-Push Validation**: `powershell -File .\scripts\pre_push_validation.ps1 -Fast`
3. **Complexity Audit**: `python scripts/complexity_audit.py`
4. **Hard-Link Sync**: `powershell -File .\deploy-sync.ps1`
5. **NinjaTrader Test**: F5 in NinjaTrader, verify strategy loads

### Success Metrics
- ✅ ManageTrailingStops complexity: 13 → ~7 (46% reduction)
- ✅ All 3 helpers are pure/atomic (lock-free)
- ✅ Zero behavioral changes (integration tests pass)
- ✅ Build succeeds with zero errors
- ✅ CSharpier formatting applied
- ✅ All unit tests pass (100% coverage on helpers)

### Rollback Plan
If any ticket fails:
1. Revert changes: `git checkout src/V12_002.Trailing.cs`
2. Review failure reason
3. Adjust extraction strategy
4. Retry with modified approach

---

## Notes

### Jane Street Alignment
- Target CYC ~7 is well under Jane Street strict standard (≤8)
- Each helper has single concern (cognitive simplicity)
- Mechanical transformation (no semantic changes)
- Incremental refactoring (verifiable at each step)

### Performance Considerations
- Zero memory allocation (no new objects)
- JIT compiler likely to inline helpers (<32 bytes each)
- Improved cache locality (smaller methods fit in L1)
- Better branch prediction (simpler control flow)
- Expected latency impact: Neutral to slight improvement

### Thread Safety
- All helpers are thread-safe:
  - ShouldSkipPosition: Pure function
  - UpdateExtremePrice: Atomic operations (Math.Max/Min)
  - ShouldAllowPointBasedTrailing: Pure function
- Main method preserves existing ToArray() snapshot pattern

---

**Generated By**: Bob Shell (v12-engineer mode)  
**Date**: 2026-06-15  
**Protocol**: V12.23 Phase 4 Ticket Generation  
**Status**: Ready for Phase 5 (Execution)
