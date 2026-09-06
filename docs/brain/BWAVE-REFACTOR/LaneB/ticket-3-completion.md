# BWAVE-REFACTOR LaneB -- Ticket 3 Completion
# Phase 4a Output
# Author: ptt-engineer
# Ticket: BWAVE-REFACTOR-LaneB-T3
# Date: 2026-09-06

---

## Scope Confirmation

[TICKET 3 ONLY] -- T2 must pass all 7 scans before T3 starts (confirmed).
Prerequisite: TICKET_REVIEW_PASS confirmed in 04-ticket-review.md.

---

## New Helpers Added

| Helper | Visibility | Parent Method |
|--------|-----------|---------------|
| IsQxCancelEligible2 | private static | CancelQxBrackets 2-param |
| IsQxCancelEligible2Testable | internal static (seam) | -- |
| CommitQxCancelBatch | private | CancelQxBrackets 2-param |
| CancelStaleTargetDrag | private | ResubmitTargetAfterCascade |
| CreateAndSubmitCascadeTarget | private | ResubmitTargetAfterCascade |
| IsPositionStateTriggerState | private static | TryFirePositionState |
| IsPositionStateTriggerStateTestable | internal static (seam) | -- |
| TryClearLeaderDirectionOnFlat | private | TryFirePositionState |
| IsBeBracketRecoveryEligible | private | TryReplacePttBeBrackets |
| HasActiveQxOrders | private | TryReplacePttBeBrackets |
| IsNativeLeaderTarget | private static | CountLeaderTargets |
| IsNativeLeaderTargetTestable | internal static (seam) | -- |

---

## CCN Reduction

| Method | Before | After |
|--------|--------|-------|
| CancelQxBrackets 2-param | 14 | <=4 |
| ResubmitTargetAfterCascade | 13 | <=2 |
| TryFirePositionState | 13 | <=5 |
| TryReplacePttBeBrackets | 14 | <=5 |
| CountLeaderTargets | 13 | <=5 |

---

## T2 Consolidation Decision

T2's engineer created `CommitStaleCancelBatch` (NOT consolidated into `CommitCancelBatch`).
T3's `CommitQxCancelBatch` therefore delegates to `CommitStaleCancelBatch` to avoid duplicating
the DW-B79-09 race guard and `acc.Cancel` try/catch block. This is the cleanest approach per
the ticket's consolidation note.

## IsPositionStateTriggerState Convention

Chosen convention: returns `true` when state does NOT trigger position state (parent should early-return).
Returns `false` when state IS a trigger (Filled or PartFilled) -- parent should fire.
Parent guard: `if (IsPositionStateTriggerState(state)) return;`
Test assertions:
- `IsPositionStateTriggerState_Filled_ReturnsFalse` -- Filled IS a trigger -> returns false
- `IsPositionStateTriggerState_Cancelled_ReturnsTrue` -- Cancelled is NOT a trigger -> returns true

---

## 7-Scan Results

### SCAN 1 -- lizard CCN
Command: lizard $files --csv | ConvertFrom-Csv | Where-Object CCN > 8 | Where-Object MethodLongName matches T3 methods
Output: SCAN 1 PASS: zero rows
PASS: all 5 parent methods and all new helpers score CCN <= 8.

### SCAN 2 -- lock()
Command: Select-String -Path src/PropTraderTools/CopyEngine.cs -Pattern "lock\s*\("
Output: SCAN 2 PASS: zero lock() calls (all matches are comments)
PASS.

### SCAN 3 -- async void
Command: Select-String -Path src/PropTraderTools/CopyEngine.cs -Pattern "async\s+void"
Output: 2 matches (both in comments -- "no lock. ... NOT async void")
PASS: zero actual async void methods.

### SCAN 4 -- return null in new helpers
Command: Select-String -Path src/PropTraderTools/CopyEngine.cs -Pattern "return null"
Output: 16 matches, all in pre-existing grandfathered code (FindBePosition, FindFollowerBracketOrder
etc.) -- none in T3 helpers.
PASS: zero new return null in T3 helpers.

### SCAN 5 -- build
Command: dotnet build src/PropTraderTools/PropTraderTools.csproj --no-incremental
Output: 1 Warning(s), 0 Error(s)
Note: 1 pre-existing xUnit2004 warning in B131Tests.cs (not T3 code). Zero errors.
PASS.

### SCAN 6 -- ASCII
Command: $bytes=[System.IO.File]::ReadAllBytes(...); count = bytes > 127
Output: SCAN 6: Non-ASCII byte count = 0
PASS.

### SCAN 7 -- tests
Command: dotnet test tests/PropTraderTools.Tests/PropTraderTools.Tests.csproj --filter "FullyQualifiedName~BwaveRefactorLaneB"
Output: Failed: 0, Passed: 12, Skipped: 0, Total: 12
PASS: all 12 tests pass (5 T1 + 3 T2 + 4 T3).

---

## Test List

| [Fact] Name | Covers |
|-------------|--------|
| IsBeTargetStateOk_Working_ReturnsTrue | T1: IsBeTargetStateOk inline mirror |
| IsBeTargetStateOk_CancelSubmitted_ReturnsTrue | T1: IsBeTargetStateOk inline mirror |
| IsBeTargetStateOk_Filled_ReturnsFalse | T1: IsBeTargetStateOk inline mirror |
| IsImmediateBeEligible_NullPosition_ReturnsFalse | T1: IsImmediateBeEligible inline mirror |
| IsImmediateBeEligible_ZeroTickSize_ReturnsFalse | T1: IsImmediateBeEligible inline mirror |
| IsQxCancelEligible3_NullSnapshot_PassesThrough | T2: IsQxCancelEligible3Testable seam existence |
| IsQxCancelEligible3_OrderNotInSnapshot_ReturnsFalse | T2: IsQxCancelEligible3Testable seam existence |
| IsAccountFlattenable_NullAccount_ReturnsFalse | T2: IsAccountFlattenable structural existence |
| IsPositionStateTriggerState_Filled_ReturnsFalse | T3: IsPositionStateTriggerState inline mirror |
| IsPositionStateTriggerState_Cancelled_ReturnsTrue | T3: IsPositionStateTriggerState inline mirror |
| IsNativeLeaderTarget_NullOrder_ReturnsFalse | T3: IsNativeLeaderTargetTestable seam existence |
| IsQxCancelEligible2_NullInstrument_ReturnsFalse | T3: IsQxCancelEligible2Testable seam existence |

---

## Deviations from Ticket Spec

1. **CommitQxCancelBatch delegates to CommitStaleCancelBatch**: Per consolidation note in ticket
   and architecture plan R-04. T2's CommitStaleCancelBatch already encapsulates the DW-B79-09 race
   guard + acc.Cancel try/catch. Delegation avoids logic duplication. Body is one call (CCN<=1).

2. **IsQxCancelEligible2Testable seam added**: Ticket said "add seam if needed". Added as
   `internal static bool IsQxCancelEligible2Testable(Order o, Instrument instr)` to support the
   IsQxCancelEligible2_NullInstrument_ReturnsFalse structural test. Consistent with T1/T2 pattern.

No other deviations.

---

## BUILD_PASS
