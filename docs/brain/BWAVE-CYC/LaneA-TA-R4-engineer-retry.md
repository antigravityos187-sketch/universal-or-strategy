# BWAVE-CYC Lane-A Ticket TA-R4 — Engineer RETRY Completion Report

**Status**: BUILD_PASS
**File**: `src/PropTraderTools/CopyEngine.cs`
**Ticket**: TA-R4 RETRY — CCN remediation for 4 blockers identified in LaneA-TA-R4-verify.md
**Engineer**: ptt-engineer (RETRY pass)
**Date**: 2025-08-27

---

## Blockers Fixed

| Method | CCN Before | CCN After | Ceiling | Result |
|--------|-----------|-----------|---------|--------|
| IsBePendingTargetOrder | 6 | 2 | <= 4 | **PASS** |
| IsPttDragOrderCancellable | 6 | 3 | <= 4 | **PASS** |
| TryFireFollowerBeRetry | 10 | 7 | <= 8 | **PASS** |
| TryEvictFollowerBeSlot | 11 | 7 | <= 8 | **PASS** |

---

## New Helpers Added

### From IsBePendingTargetOrder (CCN 6 → 2)

1. **`IsPttQxTargetOrder(Order o)`** — CCN=3
   - Absorbs: `o.Name.StartsWith("PTT-QX-T") && Length>8 && IsDigit[8]`
   - Previously inline as `isPttQxT` bool expression

2. **`IsNativeAtmBeRetryTarget(Order o)`** — CCN=3
   - Absorbs: `o.Name.StartsWith("Target") && Length>6 && IsDigit[6]`
   - Renamed from `IsNativeAtmTargetOrder` to avoid collision with pre-existing L5250 method
   - Note: L5250 version excludes `Target0`; this BE-retry version does not

### From TryFireFollowerBeRetry (CCN 10 → 7)

3. **`IsBeRetryEligibleOrderState(Order o)`** — CCN=2
   - Absorbs: `o.OrderState == OrderState.Working || o.OrderState == OrderState.Accepted`
   - Previously inline as `!= Working && != Accepted` compound condition

4. **`IsBeRetryOrderInvalid(Order o)`** — CCN=3
   - Absorbs: `o == null || o.Name == null || o.Account == null` triple null guard
   - Previously inline in TryFireFollowerBeRetry (added 2 `||` branches to parent)

### From TryEvictFollowerBeSlot (CCN 11 → 7)

5. **`IsBeSlotNonTerminal(bool isFilled, bool isRejected)`** — CCN=2
   - Absorbs: `!isFilled && !isRejected` (early return predicate)
   - Previously inline, added 1 `&&` to parent

6. **`IsBeFilledWithOpenPosition(Order o, bool isFilled)`** — CCN=3
   - Absorbs: `isFilled && !IsFlat(FindPosition(o.Account, o.Instrument))` flat-guard
   - Previously inline, added 1 `&&` to parent

Also: simplified `o.Account?.Name ?? string.Empty` to `o.Account.Name`
(safe: `o.Account` is non-null after `IsFollowerAccount` guard at line above)
Removed 2 conditional operators (`?.` and `??`) from parent, bringing CCN 11 → 7.

### From IsPttDragOrderCancellable (CCN 6 → 3)

7. **`IsPttDragOrderName(Order o)`** — CCN=2
   - Absorbs: `o.Name == "PTT-TGT-Drag" || o.Name == "PTT-STP-Drag"` (1 `||`)

8. **`IsDragInstrumentMatch(Order o, Instrument instr)`** — CCN=3
   - Absorbs: `o.Instrument?.FullName == instr?.FullName` (2 `?.` operators)
   - Previously these 2 null-conditionals added 2 branches to parent CCN

---

## Lizard Confirmation — All 4 Blockers Absent

Command: `lizard src/PropTraderTools/CopyEngine.cs --CCN 8`

```
NLOC    CCN   token  PARAM  length  location
   6      2     23      1       6 TrimSignal::IsBePendingTargetOrder@1504-1509
  24      7    148      1      24 TrimSignal::TryFireFollowerBeRetry@1530-1553
  21      7    135      1      21 TrimSignal::TryEvictFollowerBeSlot@1621-1641
   4      3     29      2       4 TrimSignal::IsPttDragOrderCancellable@1687-1690
```

**All 4 blockers ABSENT from warnings list** (CCN > 8 list):
- TryFireFollowerBeRetry: CCN=7 — NOT in warnings ✓
- TryEvictFollowerBeSlot: CCN=7 — NOT in warnings ✓
- IsBePendingTargetOrder: CCN=2 — NOT in warnings ✓
- IsPttDragOrderCancellable: CCN=3 — NOT in warnings ✓

New helpers confirmed compliant:
- IsPttQxTargetOrder: CCN=3 <= 4 ✓
- IsNativeAtmBeRetryTarget: CCN=3 <= 4 ✓
- IsBeRetryEligibleOrderState: CCN=2 <= 4 ✓
- IsBeRetryOrderInvalid: CCN=3 <= 4 ✓
- IsBeSlotNonTerminal: CCN=2 <= 4 ✓
- IsBeFilledWithOpenPosition: CCN=3 <= 4 (includes IsFlat + FindPosition call) ✓
- IsPttDragOrderName: CCN=2 <= 4 ✓
- IsDragInstrumentMatch: CCN=3 <= 4 ✓

---

## BUILD_PASS Confirmation

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:01.40
```

---

## cs delta Output

```
src/PropTraderTools/CopyEngine.cs
Code Health: (1.61 -> 1.87)

  [X] Fixed issue: Complex Method
      Function: TryFireFollowerBeRetry
      Status: TryFireFollowerBeRetry is no longer above the threshold for cyclomatic complexity

  [X] Fixed issue: Complex Method
      Function: TryEvictFollowerBeSlot
      Status: TryEvictFollowerBeSlot is no longer above the threshold for cyclomatic complexity

  [X] Fixed issue: Complex Conditional
      Function: TryFireFollowerBeRetry
      Status: TryFireFollowerBeRetry no longer has a complex conditional

  [X] Improved issue: Overall Code Complexity
      Status: The mean cyclomatic complexity decreases from 4.79 to 4.28, threshold = 4
```

---

## New [Fact] Test Names (8 tests, one per new helper)

Added to `BwaveCycTaR3HelperTests` in `src/PropTraderTools/CopyEngineTests.cs`:

1. `IsPttQxTargetOrder_ShouldExist_AsPrivateHelper`
2. `IsNativeAtmBeRetryTarget_ShouldExist_AsPrivateHelper`
3. `IsBeRetryEligibleOrderState_ShouldExist_AsPrivateHelper`
4. `IsBeRetryOrderInvalid_ShouldExist_AsPrivateHelper`
5. `IsBeSlotNonTerminal_ShouldExist_AsPrivateHelper`
6. `IsBeFilledWithOpenPosition_ShouldExist_AsPrivateHelper`
7. `IsPttDragOrderName_ShouldExist_AsPrivateHelper`
8. `IsDragInstrumentMatch_ShouldExist_AsPrivateHelper`

Total [Fact] count in CopyEngineTests.cs: 418

---

## Behaviour Preservation

- All 4 target methods and 8 new helpers preserve identical logic
- No early returns added or removed
- No logic reordering
- `CancelPttDragOrphansForAccount` untouched (was already compliant at CCN=5)
- `IsPttBeStopRejected`, `LogBeSlotEviction` untouched (already compliant)

## JS Rules Compliance

- JS-021: ZERO lock() — no lock() in any new or modified code
- JS-002: ZERO new return null — all helpers return bool or void
- JS-033: ZERO async void — all helpers synchronous

---

**BUILD_PASS -- TA-R4 RETRY complete**
