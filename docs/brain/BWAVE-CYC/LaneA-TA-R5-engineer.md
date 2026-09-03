# LaneA-TA-R5 Engineer Completion Report

**Ticket**: TA-R5
**Wave**: BWAVE-CYC Lane-A
**File**: `src/PropTraderTools/CopyEngine.cs`
**Status**: BUILD_PASS

---

## Methods Modified — CCN Before/After

| Method | CCN Before | CCN After | Ceiling | Pass |
|--------|-----------|-----------|---------|------|
| `IsReArmedAtmBracketCleanupRequired` | 14 | 4 | <= 4 (helper) | YES |
| `ReplaceFollowerCopyOnAtmCancel` | 9 | 7 | <= 8 (parent) | YES |
| `TryFindRuleAndFollowerIndex` | 9 | 3 | <= 4 (helper) | YES |
| `TryReplacePttBeBrackets` | 10 | 8 | <= 8 (parent) | YES |

---

## Helpers Extracted

### From `IsReArmedAtmBracketCleanupRequired` (CCN 14 → 4)

Three sub-helpers + one refactored parent:

1. **`IsQxTOrderStateValid(Order o) -> bool`** — CCN=2
   - Checks `o.OrderState == Working || o.OrderState == Accepted`

2. **`IsQxTBracketNameValid(string name) -> bool`** — CCN=4
   - Checks `name != null && StartsWith("PTT-QX-T") && Length >= 9 && IsDigit[8]`

3. **`TryGetCleanupEntryForFollower(OrderEventArgs e, out entry) -> bool`** — CCN=3
   - Checks `Account != null && IsFollowerAccount && TryGetValue entry`

4. **`IsCleanupEntryCurrentAndMatching(entry, Instrument) -> bool`** — CCN=4
   - Checks `Expiry > UtcNow && Instr?.FullName == orderInstr?.FullName`

### From `ReplaceFollowerCopyOnAtmCancel` (CCN 9 → 7)

1. **`SendAtmCancelReplace(Order cancelledOrder, CopyRule rule, in CopySignal signal) -> void`** — CCN=3
   - Resolves ATM mode, dispatches `SendCopyWithAtm`/`SendCopy`, fires `StatusUpdate?.Invoke`
   - Absorbs 2 branches from parent: `mode is Named` (1) + `?.Invoke` null-conditional (1)

### From `TryFindRuleAndFollowerIndex` (CCN 9 → 3)

1. **`TryMatchFollowerInRule(CopyRule rule, Order cancelledOrder, out int followerIndex) -> bool`** — CCN=3
   - Uses `Array.FindIndex` to avoid nested for-loop
   - Checks `followers == null` guard + lambda `?.Name` null-conditional

### From `TryReplacePttBeBrackets` (CCN 10 → 8)

1. **`IsBeReplaceTargetValid(Order cancelledStop) -> bool`** — CCN=3
   - Checks `cancelledStop != null && Account != null && Instrument != null`

2. **`TryIncrementBeReplaceAttempt(Account acc) -> bool`** — CCN=2
   - Checks attempt count against DW-B111 cap=5, logs diagnostic, increments counter

---

## Lizard Confirmation

Run: `lizard src/PropTraderTools/CopyEngine.cs --CCN 8`

**All 4 target methods ABSENT from warnings list.** Full warnings list for this file (20 pre-existing, none from TA-R5):

- IsFollowerAccount (CCN=9) — pre-existing, different ticket
- CancelQxBrackets (CCN=9, 11) — pre-existing T8
- SubmitBeStop (CCN=10) — pre-existing
- BuildUpdatedMultipliers (CCN=9) — pre-existing
- OnOrderUpdate (CCN=23) — pre-existing T1
- TryHandleEntryDrag (CCN=11) — pre-existing
- MirrorClose (CCN=9) — pre-existing
- IsExitSignalName (CCN=10) — pre-existing
- DispatchCopy (CCN=13) — pre-existing
- SyncAtmFollowerBracket (CCN=11) — pre-existing T5
- CreateFollowerReplacementStop (CCN=9) — pre-existing
- HandleBracketChange (CCN=9) — pre-existing
- FindFollowerBracketOrder (CCN=11) — pre-existing
- MatchesLeaderName (CCN=11) — pre-existing
- TryFirePositionState (CCN=11) — pre-existing T7
- FlattenOneAccount (CCN=11) — pre-existing T6
- GetRefPrice (CCN=10) — pre-existing
- RuleToDto (CCN=9) — pre-existing T8
- DtoToRule (CCN=11) — pre-existing T8

---

## Build Confirmation

```
dotnet build archive/v12-reference/Linting.csproj
Build succeeded.
  0 Warning(s)
  0 Error(s)
```

---

## CodeScene Delta (cs delta)

```
src/PropTraderTools/CopyEngine.cs
Code Health: (1.61 -> 1.97)  [IMPROVED]

[X] Fixed issue: Complex Method
    Function: IsReArmedAtmBracketCleanupRequired -- no longer above threshold

[X] Fixed issue: Complex Method
    Function: TryReplacePttBeBrackets -- no longer above threshold

[X] Fixed issue: Complex Method (x2)
    Function: TryFindRuleAndFollowerIndex -- no longer above threshold
```

---

## JS Rule Compliance

| Rule | Check | Result |
|------|-------|--------|
| JS-021 (no lock()) | `Select-String "lock(" src\PropTraderTools\CopyEngine.cs` | 0 hits in new code |
| JS-002 (no new return null) | No new `return null` introduced | PASS |
| JS-033 (no async void) | All new helpers are sync | PASS |
| NT8-013 | No `DateTime.Now` | PASS — `DateTime.UtcNow` only |
| NT8-014 | No new `CreateOrder` calls introduced | N/A |

---

## [Fact] Tests Added

File: `src/PropTraderTools/CopyEngineTests.cs`

**[Fact] count before**: 426
**[Fact] count after**: 434
**Added**: 8 new tests

New test names:
1. `IsQxTOrderStateValid_ShouldExist_AsPrivateHelper`
2. `IsQxTBracketNameValid_ShouldExist_AsPrivateHelper`
3. `TryGetCleanupEntryForFollower_ShouldExist_AsPrivateHelper`
4. `IsCleanupEntryCurrentAndMatching_ShouldExist_AsPrivateHelper`
5. `SendAtmCancelReplace_ShouldExist_AsPrivateHelper`
6. `TryMatchFollowerInRule_ShouldExist_AsPrivateHelper`
7. `IsBeReplaceTargetValid_ShouldReturnFalse_WhenOrderIsNull`
8. `TryIncrementBeReplaceAttempt_ShouldExist_AsPrivateHelper`

Previously existing architect-specified tests (T4 section) already present:
- `IsReArmedAtmBracketCleanupRequired_ShouldReturnFalse_WhenOrderStateIsNotWorkingOrAccepted`
- `IsReArmedAtmBracketCleanupRequired_ShouldReturnFalse_WhenNameDoesNotStartWithPttQxT`
- `IsReArmedAtmBracketCleanupRequired_ShouldReturnFalse_WhenTtlHasExpired`
- `IsReArmedAtmBracketCleanupRequired_ShouldReturnTrue_WhenAllConditionsMet`
- `TryFindRuleAndFollowerIndex_ShouldReturnFalse_WhenInstrumentDoesNotMatch`
- `TryFindRuleAndFollowerIndex_ShouldReturnTrue_WhenFollowerAccountMatches`
- `TryFindRuleAndFollowerIndex_ShouldSetFollowerIndex_WhenMatchFound`
- `HasActiveQxOrdersForInstrument_ShouldReturnTrue_WhenPttQxOrderIsWorking`
- `HasActiveQxOrdersForInstrument_ShouldReturnFalse_WhenNoQxOrdersExist`
- `HasActiveQxOrdersForInstrument_ShouldReturnFalse_WhenQxOrderIsFilledNotWorking`

---

## BUILD_PASS -- TA-R5 complete
