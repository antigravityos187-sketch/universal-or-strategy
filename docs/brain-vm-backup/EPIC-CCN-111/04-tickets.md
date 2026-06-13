# Phase 4: Implementation Tickets - EPIC-CCN-111

## Ticket Generation Metadata
- **Epic ID**: EPIC-CCN-111
- **Generation Date**: 2026-06-13
- **Phase**: 4 (Ticket Generation)
- **Architecture Plan**: docs/brain/EPIC-CCN-111/02-architecture-plan.md
- **Audit Report**: docs/brain/EPIC-CCN-111/03-audit-report.md

## Executive Summary

**CRITICAL DECISION REQUIRED**: The audit identified a scope boundary violation. The original scope targets `HydrateExpectedPositionsFromBroker` (CCN 17), but forensic analysis reveals the actual complexity is in `HydrateSingleAccountExpectedPosition` (CCN ~12-15).

**Two Ticket Sets Provided**:
1. **Option A (RECOMMENDED)**: Extract from `HydrateSingleAccountExpectedPosition` - requires Director approval for scope revision
2. **Option B (FALLBACK)**: Extract from `HydrateExpectedPositionsFromBroker` - adheres to original scope

---

## OPTION A: RECOMMENDED TICKETS (Scope Revision Required)

### Prerequisite: Director Approval Required

**Action**: Update `01-scope-boundary.md` to target `HydrateSingleAccountExpectedPosition`
**Rationale**: CCN 17 from nested calls, not intrinsic complexity
**Approval**: Director must sign off before executing tickets below

---

### Ticket A1: Extract Position Validation Logic

**Priority**: P1 (Foundation)
**Estimated Effort**: 45 minutes
**Complexity Reduction**: ~4 CCN points

#### Method Signature
```csharp
private bool IsValidPositionForHydration(Position pos)
```

#### Current Code Location
- **File**: `src/V12_002.SIMA.Lifecycle.cs`
- **Method**: `HydrateSingleAccountExpectedPosition`
- **Lines**: ~221-260 (approximate)

#### Extraction Steps

1. **Create new method** (lines ~260-270):
   - Add XML documentation
   - Implement null checks for pos, pos.Instrument
   - Check instrument name match
   - Check MarketPosition != Flat
   - Return boolean result

2. **Replace inline checks** in `HydrateSingleAccountExpectedPosition`:
   - Remove 4 separate if statements
   - Replace with single `if (!IsValidPositionForHydration(pos)) continue;`

3. **Verify no logic changes**:
   - Same early-exit behavior
   - Same null-safety guarantees
   - Same instrument matching logic

#### Test Requirements

**Unit Test File**: `tests/V12_Performance.Tests/Core/PositionHydrationTests.cs` (new)

**Test Cases** (6 total):
1. `IsValidPositionForHydration_NullPosition_ReturnsFalse()`
2. `IsValidPositionForHydration_NullInstrument_ReturnsFalse()`
3. `IsValidPositionForHydration_WrongInstrument_ReturnsFalse()`
4. `IsValidPositionForHydration_FlatPosition_ReturnsFalse()`
5. `IsValidPositionForHydration_ValidLongPosition_ReturnsTrue()`
6. `IsValidPositionForHydration_ValidShortPosition_ReturnsTrue()`

#### Verification Criteria

**Pre-Extraction**:
- [ ] Run `python scripts/complexity_audit.py` (baseline CCN)
- [ ] Verify `HydrateSingleAccountExpectedPosition` exists in target file
- [ ] Confirm no existing `IsValidPositionForHydration` method

**Post-Extraction**:
- [ ] Run `dotnet build` (expect: 0 errors)
- [ ] Run `dotnet test` (expect: 6 new tests, 100% pass)
- [ ] Run `python scripts/complexity_audit.py` (verify ~4 CCN reduction)
- [ ] Run `dotnet csharpier check src/` (expect: 0 issues)
- [ ] Verify `IsValidPositionForHydration` CCN ≤5

**Rollback Steps**:
1. Delete `IsValidPositionForHydration` method
2. Restore inline null checks in `HydrateSingleAccountExpectedPosition`
3. Run `dotnet build` to verify compilation
4. Delete test file `PositionHydrationTests.cs`

---

### Ticket A2: Extract Quantity Calculation Logic

**Priority**: P2 (Depends on A1)
**Estimated Effort**: 30 minutes
**Complexity Reduction**: ~1 CCN point

#### Method Signature
```csharp
private int CalculatePositionQuantity(Position pos)
```

#### Current Code Location
- **File**: `src/V12_002.SIMA.Lifecycle.cs`
- **Method**: `HydrateSingleAccountExpectedPosition`
- **Lines**: ~240-245 (approximate)

#### Extraction Steps

1. **Create new method** (lines ~270-280):
   - Add XML documentation
   - Implement ternary: `pos.MarketPosition == MarketPosition.Long ? pos.Quantity : -pos.Quantity`
   - Return signed integer

2. **Replace inline calculation**:
   - Remove inline ternary
   - Replace with `int qty = CalculatePositionQuantity(pos);`

3. **Verify sign correctness**:
   - Long positions: positive quantity
   - Short positions: negative quantity

#### Test Requirements

**Unit Test File**: `tests/V12_Performance.Tests/Core/PositionHydrationTests.cs` (append)

**Test Cases** (3 total):
1. `CalculatePositionQuantity_LongPosition_ReturnsPositive()` - Input: Long/10, Expected: +10
2. `CalculatePositionQuantity_ShortPosition_ReturnsNegative()` - Input: Short/5, Expected: -5
3. `CalculatePositionQuantity_ZeroQuantity_ReturnsZero()` - Input: Long/0, Expected: 0

#### Verification Criteria

**Pre-Extraction**:
- [ ] Verify Ticket A1 completed successfully
- [ ] Confirm inline quantity calculation exists
- [ ] Run baseline tests (expect: 6 tests pass)

**Post-Extraction**:
- [ ] Run `dotnet build` (expect: 0 errors)
- [ ] Run `dotnet test` (expect: 9 tests, 100% pass)
- [ ] Run `python scripts/complexity_audit.py` (verify ~1 CCN reduction)
- [ ] Verify `CalculatePositionQuantity` CCN ≤3

**Rollback Steps**:
1. Delete `CalculatePositionQuantity` method
2. Restore inline conditional
3. Run `dotnet build` to verify compilation
4. Delete 3 test cases from `PositionHydrationTests.cs`

---

### Ticket A3: Extract State Update Orchestration

**Priority**: P3 (Depends on A2)
**Estimated Effort**: 45 minutes
**Complexity Reduction**: ~2 CCN points

#### Method Signature
```csharp
private void EnqueueExpectedPositionUpdate(string accountName, int quantity)
```

#### Current Code Location
- **File**: `src/V12_002.SIMA.Lifecycle.cs`
- **Method**: `HydrateSingleAccountExpectedPosition`
- **Lines**: ~242-245 (approximate)

#### Extraction Steps

1. **Create new method** (lines ~280-295):
   - Add XML documentation
   - Capture accountName and quantity parameters
   - Call `Enqueue(ctx => ctx.AddOrUpdateExpectedPosition(...))`
   - Maintain Actor pattern semantics

2. **Replace inline Enqueue call**:
   - Remove variable captures and lambda
   - Replace with `EnqueueExpectedPositionUpdate(acct.Name, qty);`

3. **Verify Actor pattern maintained**:
   - State mutation still serialized through Actor queue
   - No `lock()` statements introduced

#### Test Requirements

**Unit Test File**: `tests/V12_Performance.Tests/Core/PositionHydrationTests.cs` (append)

**Test Cases** (3 total):
1. `EnqueueExpectedPositionUpdate_ValidAccount_EnqueuesCorrectly()` - Mock Actor queue, verify Enqueue called
2. `EnqueueExpectedPositionUpdate_NegativeQuantity_EnqueuesCorrectly()` - Verify short positions handled
3. `EnqueueExpectedPositionUpdate_ZeroQuantity_EnqueuesCorrectly()` - Edge case: zero quantity

#### Verification Criteria

**Pre-Extraction**:
- [ ] Verify Ticket A2 completed successfully
- [ ] Confirm inline Enqueue call exists
- [ ] Run baseline tests (expect: 9 tests pass)
- [ ] Run `grep -n "lock(" src/V12_002.SIMA.Lifecycle.cs` (expect: 0 matches)

**Post-Extraction**:
- [ ] Run `dotnet build` (expect: 0 errors)
- [ ] Run `dotnet test` (expect: 12 tests, 100% pass)
- [ ] Run `python scripts/complexity_audit.py` (verify ~2 CCN reduction)
- [ ] Verify `EnqueueExpectedPositionUpdate` CCN ≤3
- [ ] Run `grep -n "lock(" src/V12_002.SIMA.Lifecycle.cs` (expect: 0 matches)

**Rollback Steps**:
1. Delete `EnqueueExpectedPositionUpdate` method
2. Restore inline Enqueue call with lambda
3. Run `dotnet build` to verify compilation
4. Delete 3 test cases from `PositionHydrationTests.cs`
5. Verify Actor pattern still intact (no locks)

---

### Ticket A4: Final Verification & Integration

**Priority**: P4 (Depends on A3)
**Estimated Effort**: 30 minutes
**Complexity Reduction**: Verify total ~7 CCN reduction

#### Verification Steps

1. **Run full test suite**: `dotnet test --verbosity normal`
   - Expected: 12 new tests, 100% pass
   - Expected: All existing tests still pass

2. **Verify complexity reduction**: `python scripts/complexity_audit.py`
   - Expected: `HydrateSingleAccountExpectedPosition` CCN ≤8 (down from ~12-15)
   - Expected: All extracted methods CCN ≤5

3. **Run pre-push validation**: `powershell -File .\scripts\pre_push_validation.ps1 -Fast`
   - Expected: All checks pass

4. **Manual integration test**:
   - Run `powershell -File .\deploy-sync.ps1`
   - Launch NinjaTrader
   - Verify position hydration logs
   - Verify expected positions match broker positions

#### Success Criteria

**Quantitative Metrics**:
- ✅ `HydrateSingleAccountExpectedPosition` CCN ≤8 (down from ~12-15)
- ✅ All extracted methods CCN ≤5
- ✅ 12 new unit tests, 100% pass
- ✅ Zero lock() statements introduced
- ✅ Zero ASCII violations
- ✅ PR diff < 10k characters (~3,500 estimated)

**Qualitative Criteria**:
- ✅ Lock-free Actor pattern maintained
- ✅ Type safety preserved
- ✅ Each extracted method independently testable
- ✅ V12 DNA alignment: "Make illegal states unrepresentable"
- ✅ Jane Street cognitive simplicity: CCN ≤5 per method

#### Rollback Plan (Full Epic)

If integration test fails:
1. `git checkout HEAD -- src/V12_002.SIMA.Lifecycle.cs`
2. `git checkout HEAD -- tests/V12_Performance.Tests/Core/PositionHydrationTests.cs`
3. Run `dotnet build && dotnet test`
4. Document failure in `docs/brain/EPIC-CCN-111/rollback-report.md`

---

## OPTION B: FALLBACK TICKETS (Original Scope)

### Limitation: Root Complexity Remains Unaddressed

**Note**: These tickets adhere to the original scope (`HydrateExpectedPositionsFromBroker`) but provide limited cognitive benefit. The actual complexity (CCN ~12-15) remains in `HydrateSingleAccountExpectedPosition`.

---

### Ticket B1: Extract Fleet Account Hydration

**Priority**: P1 (Foundation)
**Estimated Effort**: 30 minutes
**Complexity Reduction**: ~5 CCN points

#### Method Signature
```csharp
private int HydrateFleetAccounts()
```

#### Current Code Location
- **File**: `src/V12_002.SIMA.Lifecycle.cs`
- **Method**: `HydrateExpectedPositionsFromBroker`
- **Lines**: ~200-210 (approximate)

#### Extraction Steps

1. **Create new method** (lines ~220-235):
   - Add XML documentation
   - Implement foreach loop over Account.All
   - Filter with IsFleetAccount check
   - Call HydrateSingleAccountExpectedPosition
   - Return hydrated count

2. **Replace fleet loop**:
   - Remove foreach loop and fleet filtering
   - Replace with `int hydratedCount = HydrateFleetAccounts();`

#### Test Requirements

**Unit Test File**: `tests/V12_Performance.Tests/Core/FleetHydrationTests.cs` (new)

**Test Cases** (4 total):
1. `HydrateFleetAccounts_NoFleetAccounts_ReturnsZero()`
2. `HydrateFleetAccounts_OneFleetAccount_ReturnsOne()`
3. `HydrateFleetAccounts_MultipleFleetAccounts_ReturnsCount()`
4. `HydrateFleetAccounts_MixedAccounts_OnlyHydratesFleet()`

#### Verification Criteria

**Pre-Extraction**:
- [ ] Run `python scripts/complexity_audit.py` (baseline CCN)
- [ ] Verify `HydrateExpectedPositionsFromBroker` exists
- [ ] Confirm no existing `HydrateFleetAccounts` method

**Post-Extraction**:
- [ ] Run `dotnet build` (expect: 0 errors)
- [ ] Run `dotnet test` (expect: 4 new tests, 100% pass)
- [ ] Run `python scripts/complexity_audit.py` (verify ~5 CCN reduction)
- [ ] Verify `HydrateFleetAccounts` CCN ≤5

**Rollback Steps**:
1. Delete `HydrateFleetAccounts` method
2. Restore inline fleet loop
3. Run `dotnet build` to verify compilation
4. Delete test file `FleetHydrationTests.cs`

---

### Ticket B2: Extract Master Account Hydration

**Priority**: P2 (Depends on B1)
**Estimated Effort**: 20 minutes
**Complexity Reduction**: ~3 CCN points

#### Method Signature
```csharp
private void HydrateMasterAccountIfNeeded(ref int hydratedCount)
```

#### Current Code Location
- **File**: `src/V12_002.SIMA.Lifecycle.cs`
- **Method**: `HydrateExpectedPositionsFromBroker`
- **Lines**: ~215-220 (approximate)

#### Extraction Steps

1. **Create new method** (lines ~235-245):
   - Add XML documentation
   - Check IsFleetAccount(Account)
   - Conditionally call HydrateSingleAccountExpectedPosition
   - Use ref parameter for count

2. **Replace master logic**:
   - Remove fleet check and conditional hydration
   - Replace with `HydrateMasterAccountIfNeeded(ref hydratedCount);`

#### Test Requirements

**Unit Test File**: `tests/V12_Performance.Tests/Core/FleetHydrationTests.cs` (append)

**Test Cases** (3 total):
1. `HydrateMasterAccountIfNeeded_MasterIsFleet_DoesNotHydrate()`
2. `HydrateMasterAccountIfNeeded_MasterIsNotFleet_Hydrates()`
3. `HydrateMasterAccountIfNeeded_IncrementsCount_WhenHydrated()`

#### Verification Criteria

**Pre-Extraction**:
- [ ] Verify Ticket B1 completed successfully
- [ ] Confirm inline master logic exists
- [ ] Run baseline tests (expect: 4 tests pass)

**Post-Extraction**:
- [ ] Run `dotnet build` (expect: 0 errors)
- [ ] Run `dotnet test` (expect: 7 tests, 100% pass)
- [ ] Run `python scripts/complexity_audit.py` (verify ~3 CCN reduction)
- [ ] Verify `HydrateMasterAccountIfNeeded` CCN ≤3

**Rollback Steps**:
1. Delete `HydrateMasterAccountIfNeeded` method
2. Restore inline master logic
3. Run `dotnet build` to verify compilation
4. Delete 3 test cases from `FleetHydrationTests.cs`

---

### Ticket B3: Final Verification & Integration

**Priority**: P3 (Depends on B2)
**Estimated Effort**: 20 minutes
**Complexity Reduction**: Verify total ~8 CCN reduction

#### Verification Steps

1. **Run full test suite**: `dotnet test --verbosity normal`
2. **Verify complexity reduction**: `python scripts/complexity_audit.py`
   - Expected: `HydrateExpectedPositionsFromBroker` CCN ≤5 (down from 17)
   - ⚠️ Note: `HydrateSingleAccountExpectedPosition` CCN still ~12-15 (unaddressed)
3. **Run pre-push validation**: `powershell -File .\scripts\pre_push_validation.ps1 -Fast`
4. **Manual integration test**: Run deploy-sync and verify in NinjaTrader

#### Success Criteria

**Quantitative Metrics**:
- ✅ `HydrateExpectedPositionsFromBroker` CCN ≤5 (down from 17)
- ✅ All extracted methods CCN ≤5
- ✅ 7 new unit tests, 100% pass
- ✅ PR diff < 10k characters (~2,200 estimated)

**Qualitative Criteria**:
- ✅ Lock-free Actor pattern maintained
- ✅ Backward compatibility preserved
- ⚠️ Limited cognitive benefit (thin wrappers)
- ⚠️ Root complexity remains in `HydrateSingleAccountExpectedPosition`

---

## Execution Order & Dependencies

### Option A (Recommended)

```
Director Approval → A1 (Validation) → A2 (Calculation) → A3 (State Update) → A4 (Verification)
```

**Execution Time**: ~2.5 hours (excluding approval wait)

### Option B (Fallback)

```
B1 (Fleet Hydration) → B2 (Master Hydration) → B3 (Verification)
```

**Execution Time**: ~1.5 hours

---

## Success Criteria Summary

### Option A Success Criteria

**Quantitative**:
- ✅ `HydrateSingleAccountExpectedPosition` CCN ≤8 (down from ~12-15)
- ✅ All extracted methods CCN ≤5
- ✅ 12 new unit tests, 100% pass
- ✅ Zero lock() statements
- ✅ Zero ASCII violations
- ✅ PR diff < 10k characters

**Qualitative**:
- ✅ Meaningful complexity reduction
- ✅ Independently testable units
- ✅ Jane Street cognitive simplicity
- ✅ V12 DNA alignment

### Option B Success Criteria

**Quantitative**:
- ✅ `HydrateExpectedPositionsFromBroker` CCN ≤5 (down from 17)
- ✅ All extracted methods CCN ≤5
- ✅ 7 new unit tests, 100% pass
- ✅ PR diff < 10k characters

**Qualitative**:
- ⚠️ Limited cognitive benefit
- ⚠️ Root complexity remains unaddressed
- ✅ V12.23 scope compliance

---

## Recommendation

**PRIMARY**: Execute Option A tickets (requires Director approval for scope revision)
- Addresses actual complexity source
- Provides real maintainability benefits
- Achieves Jane Street cognitive simplicity

**FALLBACK**: Execute Option B tickets (immediate execution)
- Adheres to original scope
- Achieves CCN target for specified method
- Plan follow-up EPIC-CCN-112 for `HydrateSingleAccountExpectedPosition`

---

**Ticket Generation Complete**: 2026-06-13
**Next Phase**: Phase 5 (TDD Implementation) - pending Director decision
**Generated By**: Bob Shell (v12-engineer mode)
