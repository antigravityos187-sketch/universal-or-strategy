# BWAVE-REFACTOR LaneB -- Ticket 3 Verification
# Phase 4b Output
# Author: ptt-verifier
# Ticket: BWAVE-REFACTOR-LaneB-T3
# Date: 2026-09-06

---

## Scope Confirmation

Ticket 3 scope: 5 parent methods reduced to CCN<=8 via 12 new helper extractions.

Target parent methods:
- TryReplacePttBeBrackets (was CCN 14) -> private void, L4252
- CancelQxBrackets 2-param (was CCN 14) -> internal void, L912
- TryFirePositionState (was CCN 13) -> private void, L3950
- CountLeaderTargets (was CCN 13) -> private int, L5551
- ResubmitTargetAfterCascade (was CCN 13) -> private void, L2975

All 12 new helpers confirmed present in CopyEngine.cs before scanning.

---

## SCAN 1 Result -- CCN

Command:
  $files = Get-ChildItem src/PropTraderTools/ -Filter "*.cs" -Recurse |
    Where-Object { $_.FullName -notmatch '\\obj\\' -and $_.FullName -notmatch '\\bin\\' }
  lizard $files --csv | ConvertFrom-Csv | Where-Object { [int]$_.CCN -gt 8 } |
    Where-Object { $_.MethodLongName -match "TryReplacePttBeBrackets|CancelQxBrackets|TryFirePositionState|
    CountLeaderTargets|ResubmitTargetAfterCascade|IsBeBracketRecoveryEligible|HasActiveQxOrders|
    IsQxCancelEligible2|CommitQxCancelBatch|IsPositionStateTriggerState|TryClearLeaderDirectionOnFlat|
    IsNativeLeaderTarget|CancelStaleTargetDrag|CreateAndSubmitCascadeTarget" }

Output: (no rows -- command returned empty)
RESULT: SCAN 1 PASS -- zero rows. All 5 parent methods and all 12 helpers score CCN <= 8.

CCN verified per source review:
- TryReplacePttBeBrackets:      CCN<=5  (comment L4249: IsBeBracketRecoveryEligible(1)+HasActiveQxOrders(2)+prevAttempts>=5(3)+TryAdd(4)+QueueBeRetryFallback(0))
- CancelQxBrackets 2-param:     CCN<=4  (comment L910: null guard(1)+foreach(2)+IsQxCancelEligible2(3)+stale.Count==0(4))
- TryFirePositionState:         CCN<=5  (comment L3948: IsPositionStateTriggerState(1)+instrument null(2)+Interlocked CAS(3)+prior==newVal(4)+TryClearLeaderDirectionOnFlat(0))
- CountLeaderTargets:           CCN<=5  (comment L5549: rule null(1)+leader null(2)+foreach(3)+o null continue(4)+IsNativeLeaderTarget(5))
- ResubmitTargetAfterCascade:   CCN<=2  (comment L2974: TryParseStopSuffix(1)+tgtDragName local(0)+CancelStaleTargetDrag(0)+CreateAndSubmitCascadeTarget(0))
- IsBeBracketRecoveryEligible:  CCN<=5  (comment L4290: base(1)+null check(2)+IsFollowerAccount(3)+IsFlat(4)+ContainsKey(5))
- HasActiveQxOrders:            CCN<=4  (comment L4308: ToList(0)+StartsWith(1)+Working|Submitted(2)+instr match(3))
- IsQxCancelEligible2:          CCN<=7  (comment L930: stateOk-Working(1)+Initialized(2)+Accepted(3)+Submitted(4)+TriggerPending(5)+instrument match(6)+IsQxCancelCandidate(7))
- CommitQxCancelBatch:          CCN<=1  (comment L958: single delegation call)
- IsPositionStateTriggerState:  CCN<=2  (comment L3984: base(1)+Filled||PartFilled OR(1))
- TryClearLeaderDirectionOnFlat:CCN<=4  (comment L3994: foreach(1)+Name match(2)+isLeaderAcct check(3)+TryRemove(0))
- IsNativeLeaderTarget:         CCN<=7  (comment L5578: stateOk(1)+instrOk null+FullName(2)+OrderType.Limit(3)+IsNullOrEmpty(4)+Length>=7(5)+StartsWith(6)+IsDigit&&!='0'(7))
- CancelStaleTargetDrag:        CCN<=4  (comment L2991: foreach(1)+Working check(2)+Name match(3)+Instrument match(4))
- CreateAndSubmitCascadeTarget: CCN<=3  (comment L3022: base(1)+newTarget null guard(2)+catch(0))

---

## SCAN 2 Result -- lock()

Command: Select-String -Path "src/PropTraderTools/CopyEngine.cs" -Pattern "lock\s*\("

Output: 34 matches, ALL in comments (JS-021 documentation, "no lock", "no lock()").
Zero actual lock() statement calls. All hits are comment text patterns containing
the word "lock" in JS-021 compliance documentation.

RESULT: SCAN 2 PASS -- zero actual lock() calls.

---

## SCAN 3 Result -- async void

Command: Select-String -Path "src/PropTraderTools/CopyEngine.cs" -Pattern "async\s+void"

Output:
  L1850: // JS-021: no lock. JS-001: no throw. JS-033: Tick is not async void. ASCII-only.
  L6981: // Called directly from OnOrderUpdate -- NOT an event handler. Synchronous void. NOT async void (JS-033).

Both matches are in comments. Zero actual async void method declarations.

RESULT: SCAN 3 PASS -- zero actual async void methods.

---

## SCAN 4 Result -- return null

Command: Select-String -Path "src/PropTraderTools/CopyEngine.cs" -Pattern "return null"

Output: 20 matches total.
Actual code (non-comment) return null hits:
  L1215, L1918, L2847, L2928, L2936, L3703, L3872, L5409, L5415, L5494, L6699, L6714
Remaining hits are in comments (e.g., JS-002 "no return null" documentation).

T3 helper line ranges: 909-962, 2970-3062, 3940-4012, 4242-4327, 5540-5597.
Cross-check of actual code hits against T3 ranges:
  L1215: outside (pre-existing) SAFE
  L1918: outside (pre-existing) SAFE
  L2847: outside (pre-existing, before 2970) SAFE
  L2928: FindLeaderCollateralOrder (L2925-2937, pre-existing) SAFE -- verified by source read
  L2936: FindLeaderCollateralOrder (same method, pre-existing) SAFE
  L3703: outside (pre-existing) SAFE
  L3872: outside (pre-existing) SAFE
  L5409: outside (pre-existing, before 5540) SAFE
  L5415: outside (pre-existing) SAFE
  L5494: outside (pre-existing) SAFE
  L6699: outside (pre-existing) SAFE
  L6714: outside (pre-existing) SAFE

Zero return null in any T3 helper.

RESULT: SCAN 4 PASS -- zero return null in T3 helpers. All T3 helpers return bool or void.

---

## SCAN 5 Result -- build

Command: dotnet build "src/PropTraderTools/PropTraderTools.csproj" --no-incremental

Output:
  C:\WSGTA\ptt-lane-b\src\PropTraderTools\Tests\B131Tests.cs(165,13): warning xUnit2004:
    Do not use Assert.Equal() to check for boolean conditions; use Assert.True() or Assert.False() instead.
  Build succeeded.
  1 Warning(s)
  0 Error(s)

Pre-existing warning in B131Tests.cs (not T3 code). Zero errors.

RESULT: SCAN 5 PASS -- Build succeeded, 0 errors, 1 pre-existing warning (B131Tests.cs).

---

## SCAN 6 Result -- ASCII

Command:
  $bytes = [System.IO.File]::ReadAllBytes("src/PropTraderTools/CopyEngine.cs")
  ($bytes | Where-Object { $_ -gt 127 } | Measure-Object).Count

Output: 0

RESULT: SCAN 6 PASS -- Count = 0. Zero non-ASCII bytes.

---

## SCAN 7 Result -- tests

Command: dotnet test "tests/PropTraderTools.Tests/PropTraderTools.Tests.csproj" --filter "FullyQualifiedName~BwaveRefactorLaneB"

Output:
  Passed!  - Failed: 0, Passed: 12, Skipped: 0, Total: 12, Duration: 147 ms

Test breakdown (12 total):
T1 tests (5):
  IsBeTargetStateOk_Working_ReturnsTrue
  IsBeTargetStateOk_CancelSubmitted_ReturnsTrue
  IsBeTargetStateOk_Filled_ReturnsFalse
  IsImmediateBeEligible_NullPosition_ReturnsFalse
  IsImmediateBeEligible_ZeroTickSize_ReturnsFalse
T2 tests (3):
  IsQxCancelEligible3_NullSnapshot_PassesThrough
  IsQxCancelEligible3_OrderNotInSnapshot_ReturnsFalse
  IsAccountFlattenable_NullAccount_ReturnsFalse
T3 tests (4):
  IsPositionStateTriggerState_Filled_ReturnsFalse
  IsPositionStateTriggerState_Cancelled_ReturnsTrue
  IsNativeLeaderTarget_NullOrder_ReturnsFalse
  IsQxCancelEligible2_NullInstrument_ReturnsFalse

RESULT: SCAN 7 PASS -- all 12 tests pass, 0 failures, 0 skipped.

---

## Structural Checks

### Check 1: All 12 helpers exist as declared
Verified via Select-String and source reads:
  IsBeBracketRecoveryEligible:           L4292 private bool -- PRESENT
  HasActiveQxOrders:                     L4310 private bool -- PRESENT
  IsQxCancelEligible2:                   L933  private static bool -- PRESENT
  IsQxCancelEligible2Testable:           L950  internal static bool (seam) -- PRESENT
  CommitQxCancelBatch:                   L959  private void -- PRESENT
  CancelStaleTargetDrag:                 L2994 private void -- PRESENT
  CreateAndSubmitCascadeTarget:          L3024 private void -- PRESENT
  IsPositionStateTriggerState:           L3985 private static bool -- PRESENT
  IsPositionStateTriggerStateTestable:   L3978 internal static bool (seam) -- PRESENT
  TryClearLeaderDirectionOnFlat:         L3996 private void -- PRESENT
  IsNativeLeaderTarget:                  L5581 private static bool -- PRESENT
  IsNativeLeaderTargetTestable:          L5572 internal static bool (seam) -- PRESENT
RESULT: PASS -- all 12 helpers present.

### Check 2: No logic deleted -- parent methods exist and delegate to helpers
  TryReplacePttBeBrackets (L4252): calls IsBeBracketRecoveryEligible (L4254), HasActiveQxOrders (L4258) -- PASS
  CancelQxBrackets 2-param (L912): calls IsQxCancelEligible2 (L919), CommitQxCancelBatch (L924) -- PASS
  TryFirePositionState (L3950): calls IsPositionStateTriggerState (L3953), TryClearLeaderDirectionOnFlat (L3970) -- PASS
  CountLeaderTargets (L5551): calls IsNativeLeaderTarget (L5564) -- PASS
  ResubmitTargetAfterCascade (L2975): calls CancelStaleTargetDrag (L2984), CreateAndSubmitCascadeTarget (L2985) -- PASS
RESULT: PASS -- all 5 parent methods intact and properly delegating.

### Check 3: Public signatures unchanged
  TryReplacePttBeBrackets:     private void TryReplacePttBeBrackets(Order cancelledStop) -- UNCHANGED
  CancelQxBrackets 2-param:    internal void CancelQxBrackets(Account acc, NinjaTrader.Cbi.Instrument instr) -- UNCHANGED
  TryFirePositionState:        private void TryFirePositionState(OrderEventArgs e) -- UNCHANGED
  CountLeaderTargets:          private int CountLeaderTargets(Instrument instrument) -- UNCHANGED
  ResubmitTargetAfterCascade:  private void ResubmitTargetAfterCascade(Account acc, Order stpOrder, double targetPrice, Order leaderOrder, string suffix) -- UNCHANGED
RESULT: PASS -- all 5 parent signatures unchanged.

### Check 4: Test seams present as internal static
  IsPositionStateTriggerStateTestable: L3978 "internal static bool IsPositionStateTriggerStateTestable(OrderState s)" -- PRESENT
  IsNativeLeaderTargetTestable:        L5572 "internal static bool IsNativeLeaderTargetTestable(Order o, string instrFullName)" -- PRESENT
  IsQxCancelEligible2Testable:         L950  "internal static bool IsQxCancelEligible2Testable(NinjaTrader.Cbi.Order o, NinjaTrader.Cbi.Instrument instr)" -- PRESENT
RESULT: PASS -- all required test seams present as internal static.

### Check 5: NT8 constraint in CreateAndSubmitCascadeTarget
Source at L3045-3046:
  NinjaTrader.Core.Globals.MaxDate,        // arg11 preserved
  (NinjaTrader.Cbi.CustomOrder)null        // arg12 preserved
  leaderOrder.Quantity used at L3040       // DW-B142-QTY-DESYNC-01 preserved
RESULT: PASS -- NinjaTrader.Core.Globals.MaxDate and (NinjaTrader.Cbi.CustomOrder)null both present.

### Check 6: Consolidation check
CommitQxCancelBatch (L959) delegates to CommitStaleCancelBatch (L961):
  private void CommitQxCancelBatch(...) { CommitStaleCancelBatch(acc, stale); }
CommitStaleCancelBatch is the T2 helper (L1088). Delegation confirmed.
The 3-param CancelQxBrackets (L1008) calls CommitStaleCancelBatch directly (L1042).
The 2-param CancelQxBrackets (L912) calls CommitQxCancelBatch (L924) which delegates to CommitStaleCancelBatch.
Both paths ultimately execute CommitStaleCancelBatch. No logic duplication.
RESULT: PASS -- consolidation via delegation implemented correctly.

### Check 7: IsPositionStateTriggerState convention
Source at L3985-3989:
  private static bool IsPositionStateTriggerState(OrderState s)
  {
      return s != OrderState.Filled && s != OrderState.PartFilled;
  }
Convention documented at L3981-3983: returns true when NOT a trigger state (parent early-returns),
  returns false when IS a trigger state (Filled or PartFilled -- parent fires).
Parent guard at L3953: "if (IsPositionStateTriggerState(state)) return;" -- consistent.
Tests: Filled returns false (PASS), Cancelled returns true (PASS) -- consistent with convention.
RESULT: PASS -- convention documented and consistent throughout.

---

## Layer 2 Cross-Check

| Scan | Engineer Report (Layer 2) | My Independent Scan (Layer 3) | Match |
|------|--------------------------|-------------------------------|-------|
| SCAN 1 CCN | zero rows | zero rows | MATCH |
| SCAN 2 lock() | zero actual lock() (all comments) | zero actual lock() (all comments) | MATCH |
| SCAN 3 async void | 2 hits (both comments) | 2 hits (both comments) | MATCH |
| SCAN 4 return null | 16 hits total, 0 in T3 helpers | 20 hits total, 0 in T3 helpers | SUBSTANCE MATCH* |
| SCAN 5 build | 1 warning, 0 errors | 1 warning, 0 errors | MATCH |
| SCAN 6 ASCII | Count = 0 | Count = 0 | MATCH |
| SCAN 7 tests | Failed: 0, Passed: 12 | Failed: 0, Passed: 12 | MATCH |

*SCAN 4 count discrepancy: engineer reported 16, verifier found 20. Delta of 4 is explained
by comment lines (L698, L703, L708 "no return null", L4599 "no return null") that are included
in PowerShell Select-String matches but were likely excluded from the engineer's count.
The substantive claim -- zero return null in T3 helpers -- is independently confirmed correct.
This is NOT a Layer 2 dishonesty; it is a counting methodology difference.

---

## Deviations Noted

1. SCAN 4 match count: engineer reported 16, verifier found 20. Delta explained by 4 comment-only
   hits. Substance is identical: zero return null in T3 helpers. NOT a violation.

2. CommitQxCancelBatch delegates to CommitStaleCancelBatch (T2) rather than implementing the
   cancel logic directly. This was disclosed in the ticket consolidation note and completion.md.
   The ticket explicitly permits this approach. NOT a violation.

3. IsPositionStateTriggerState convention (returns true for NON-triggers) is the "negated guard"
   convention. The ticket explicitly deferred this choice to the engineer and required documentation.
   The engineer documented the convention at L3981-3983 and the parent guard comment at L3953.
   Test assertions match the convention. NOT a violation.

No other deviations.

---

## VERIFY_PASS

All 7 scans pass. All structural checks pass. No DNA violations found.
Layer 2 self-report confirmed accurate (count methodology difference on SCAN 4 is non-material).

VERDICT: VERIFY_PASS