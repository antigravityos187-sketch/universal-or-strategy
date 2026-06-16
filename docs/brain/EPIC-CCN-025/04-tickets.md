# Extraction Tickets: EPIC-CCN-025

## Overview
- **Total Tickets**: 4
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3 → TICKET-4)
- **Estimated Effort**: 4-6 hours
- **Target Method**: CheckFFMAConditions
- **File**: src/V12_002.Entries.FFMA.cs
- **Current Complexity**: 16
- **Target Complexity**: ≤8 per method (Jane Street strict standard)

---

## TICKET-1: Extract CalculateStopDistance

### Scope
- **Current Method**: `CheckFFMAConditions`
- **Current CYC**: 16
- **Target CYC**: Extract helper with CYC ~2
- **Extraction**: Stop loss distance calculation with minimum tick validation

### Method Signature
```csharp
private double CalculateStopDistance(double currentPrice, double stopPrice)
```

### Implementation
1. Create new private method `CalculateStopDistance` below `CheckFFMAConditions`
2. Extract stop distance calculation logic:
   - Calculate raw distance: `Math.Abs(currentPrice - stopPrice)`
   - Apply MaximumStop cap if configured
   - Enforce minimum tick size: `Math.Max(distance, TickSize)`
3. Replace inline calculations in SHORT and LONG blocks with method call
4. Run `dotnet csharpier format src/` to enforce formatting
5. Run `powershell -File .\scripts\build_readiness.ps1` to verify build
6. Run `powershell -File .\scripts\complexity_audit.py` to verify CYC reduction

### Acceptance Criteria
- [ ] Method `CalculateStopDistance` created with CYC ≤ 2
- [ ] All stop distance calculations use new helper method
- [ ] MaximumStop cap logic preserved
- [ ] Minimum tick size validation preserved
- [ ] All tests pass (if any exist)
- [ ] Build succeeds with zero errors
- [ ] CSharpier formatting applied
- [ ] No behavioral changes (pure extraction)

### Dependencies
- None (first ticket)

### Estimated Effort
1 hour

---

## TICKET-2: Extract CheckShortSetupConditions

### Scope
- **Current Method**: `CheckFFMAConditions`
- **Current CYC**: 16 → ~12 (after TICKET-1)
- **Target CYC**: Extract helper with CYC ~4
- **Extraction**: SHORT entry condition validation and execution

### Method Signature
```csharp
private bool CheckShortSetupConditions(double rsiValue, double distanceFromEMA, bool isRedCandle, double currentPrice)
```

### Implementation
1. Create new private method `CheckShortSetupConditions` below `CalculateStopDistance`
2. Extract SHORT setup block (lines ~60-80):
   - RSI overbought validation (>= OverboughtLevel)
   - Distance from EMA validation (>= MinDistanceFromEMA)
   - Red candle validation
   - Stop loss calculation using `CalculateStopDistance`
   - Position size calculation using existing `CalculatePositionSize`
   - Entry execution using existing `ExecuteFFMAEntry`
3. Replace SHORT block in `CheckFFMAConditions` with single method call
4. Return `true` if entry executed, `false` otherwise
5. Run `dotnet csharpier format src/`
6. Run `powershell -File .\scripts\build_readiness.ps1`
7. Run `powershell -File .\scripts\complexity_audit.py`

### Acceptance Criteria
- [ ] Method `CheckShortSetupConditions` created with CYC ≤ 4
- [ ] All SHORT entry logic encapsulated in helper
- [ ] RSI overbought validation preserved
- [ ] EMA distance validation preserved
- [ ] Red candle validation preserved
- [ ] Stop loss calculation uses `CalculateStopDistance`
- [ ] Position sizing uses existing `CalculatePositionSize`
- [ ] Entry execution uses existing `ExecuteFFMAEntry`
- [ ] All tests pass
- [ ] Build succeeds with zero errors
- [ ] CSharpier formatting applied
- [ ] No behavioral changes

### Dependencies
- TICKET-1 must be completed first (uses `CalculateStopDistance`)

### Estimated Effort
1.5 hours

---

## TICKET-3: Extract CheckLongSetupConditions

### Scope
- **Current Method**: `CheckFFMAConditions`
- **Current CYC**: ~12 → ~8 (after TICKET-2)
- **Target CYC**: Extract helper with CYC ~4
- **Extraction**: LONG entry condition validation and execution

### Method Signature
```csharp
private bool CheckLongSetupConditions(double rsiValue, double distanceFromEMA, bool isGreenCandle, double currentPrice)
```

### Implementation
1. Create new private method `CheckLongSetupConditions` below `CheckShortSetupConditions`
2. Extract LONG setup block (lines ~82-102):
   - RSI oversold validation (<= OversoldLevel)
   - Distance from EMA validation (>= MinDistanceFromEMA)
   - Green candle validation
   - Stop loss calculation using `CalculateStopDistance`
   - Position size calculation using existing `CalculatePositionSize`
   - Entry execution using existing `ExecuteFFMAEntry`
3. Replace LONG block in `CheckFFMAConditions` with single method call
4. Return `true` if entry executed, `false` otherwise
5. Run `dotnet csharpier format src/`
6. Run `powershell -File .\scripts\build_readiness.ps1`
7. Run `powershell -File .\scripts\complexity_audit.py`

### Acceptance Criteria
- [ ] Method `CheckLongSetupConditions` created with CYC ≤ 4
- [ ] All LONG entry logic encapsulated in helper
- [ ] RSI oversold validation preserved
- [ ] EMA distance validation preserved
- [ ] Green candle validation preserved
- [ ] Stop loss calculation uses `CalculateStopDistance`
- [ ] Position sizing uses existing `CalculatePositionSize`
- [ ] Entry execution uses existing `ExecuteFFMAEntry`
- [ ] All tests pass
- [ ] Build succeeds with zero errors
- [ ] CSharpier formatting applied
- [ ] No behavioral changes

### Dependencies
- TICKET-1 must be completed first (uses `CalculateStopDistance`)
- TICKET-2 should be completed first (parallel structure)

### Estimated Effort
1.5 hours

---

## TICKET-4: Refactor CheckFFMAConditions

### Scope
- **Current Method**: `CheckFFMAConditions`
- **Current CYC**: ~8 → ~5 (after TICKET-3)
- **Target CYC**: ≤5 (orchestration only)
- **Extraction**: Simplify main method to orchestration logic

### Implementation
1. Refactor `CheckFFMAConditions` to use extracted helpers:
   - Keep guard clauses (3 early returns)
   - Replace SHORT block with `CheckShortSetupConditions` call
   - Replace LONG block with `CheckLongSetupConditions` call
   - Preserve try-catch exception handling
2. Verify method now only contains:
   - Guard clauses for null/invalid state
   - Helper method calls
   - Exception handling
3. Run `dotnet csharpier format src/`
4. Run `powershell -File .\scripts\build_readiness.ps1`
5. Run `powershell -File .\scripts\complexity_audit.py`
6. Run `powershell -File .\deploy-sync.ps1` to sync NinjaTrader hard links

### Acceptance Criteria
- [ ] Method `CheckFFMAConditions` reduced to CYC ≤ 5
- [ ] Guard clauses preserved (3 early returns)
- [ ] SHORT logic delegated to `CheckShortSetupConditions`
- [ ] LONG logic delegated to `CheckLongSetupConditions`
- [ ] Exception handling preserved
- [ ] All tests pass
- [ ] Build succeeds with zero errors
- [ ] CSharpier formatting applied
- [ ] NinjaTrader hard links synchronized
- [ ] No behavioral changes
- [ ] F5 test in NinjaTrader passes (manual verification)

### Dependencies
- TICKET-1 must be completed first
- TICKET-2 must be completed first
- TICKET-3 must be completed first

### Estimated Effort
1 hour

---

## Verification Strategy

### After Each Ticket
1. **Build Verification**: `powershell -File .\scripts\build_readiness.ps1`
2. **Complexity Audit**: `powershell -File .\scripts\complexity_audit.py`
3. **Formatting Check**: `dotnet csharpier check src/`
4. **Lock-Free Audit**: `grep -r "lock(" src/V12_002.Entries.FFMA.cs` (expect zero matches)

### After All Tickets
1. **ASCII Audit**: `powershell -File .\scripts\pre_push_validation.ps1 -Fast`
2. **Hard Link Sync**: `powershell -File .\deploy-sync.ps1`
3. **F5 Test**: Manual verification in NinjaTrader (FFMA entry logic unchanged)
4. **Final Complexity Check**: Verify all methods ≤8 CYC (target ≤5 achieved)

## Rollback Plan

If any ticket breaks compilation or tests:
1. Use Bob CLI `/restore` command to revert changes
2. Review extraction logic for errors
3. Re-attempt extraction with corrected approach
4. Mandatory checkpointing enabled via `.bob/settings.json`

## Success Metrics

### Complexity Reduction
- **Before**: CheckFFMAConditions CYC 16
- **After**: 
  - CheckFFMAConditions: CYC ~5 ✅
  - CheckShortSetupConditions: CYC ~4 ✅
  - CheckLongSetupConditions: CYC ~4 ✅
  - CalculateStopDistance: CYC ~2 ✅
- **Total Distributed Complexity**: 15 (across 4 methods)

### V12 DNA Compliance
- ✅ Correctness by Construction (strongly-typed parameters)
- ✅ Lock-Free Actor Pattern (zero lock() statements)
- ✅ ASCII-Only Compliance (no Unicode characters)
- ✅ Jane Street Alignment (all methods ≤8 CYC, target ≤5)

### PR Hygiene
- ✅ Diff Size: ~2,800 characters (well under 10k limit)
- ✅ Scope Creep: Single method focus (no unrelated changes)
- ✅ Build Readiness: No breaking changes (private method refactoring)

## Sign-off

- **Phase 4 Status**: COMPLETED
- **Ticket Count**: 4
- **Execution Order**: Sequential
- **Estimated Total Effort**: 4-6 hours
- **Next Phase**: Phase 5 (Ticket Execution)

**Generated By**: V12 Phase 4 Ticket Generation System
**Timestamp**: 2026-06-15T16:50:59Z
