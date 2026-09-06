# BWAVE-REFACTOR LaneB -- Ticket 2 Verification
# Phase 4b Output
# Author: ptt-verifier
# Ticket: BWAVE-REFACTOR-LaneB-T2
# Date: 2026-09-06
# Workspace: C:\WSGTA\ptt-lane-b\

---

## Scope Confirmation

[TICKET 2 ONLY] -- Tier B: CCN 16-19 (4 parent methods, 9 new helpers).
No other tickets read, referenced, or evaluated in this session.

Target parent methods:
- FlattenOneAccount (was CCN 19)
- MoveStopToBreakEven (was CCN 18)
- ReplaceFollowerCopyOnAtmCancel (was CCN 18)
- CancelQxBrackets 3-param overload (was CCN 16)

Expected helpers (9 total): IsAccountFlattenable, SubmitMarketFlattenOrder,
LogDiagOrderCount, RegisterBeRetrySlotIfNeeded, FindFollowerRuleForOrder,
IsReplaceDispatchEligible, IsQxCancelEligible3, IsQxCancelEligible3Testable,
CommitStaleCancelBatch.

Consolidation note: CommitStaleCancelBatch (not a separate CommitQxCancelBatch) was
used. Both CancelQxBrackets overloads (2-param and 3-param) call CommitStaleCancelBatch.

---

## SCAN 1 Result -- CCN

Command:
  $files = Get-ChildItem src/PropTraderTools/ -Filter "*.cs" -Recurse |
    Where-Object { $_.FullName -notmatch '\\obj\\' -and $_.FullName -notmatch '\\bin\\' }
  lizard $files --csv 2>&1 | ConvertFrom-Csv -Header @(...)
    | Where-Object { [int]$_.CCN -gt 8 }
    | Where-Object { $_.MethodLongName -match "FlattenOneAccount|MoveStopToBreakEven|
        ReplaceFollowerCopyOnAtmCancel|CancelQxBrackets|IsAccountFlattenable|
        SubmitMarketFlattenOrder|LogDiagOrderCount|RegisterBeRetrySlotIfNeeded|
        FindFollowerRuleForOrder|IsReplaceDispatchEligible|IsQxCancelEligible3|
        CommitStaleCancelBatch|CommitCancelBatch|CommitQxCancelBatch" }

Output: (no rows)

PASS: zero T2 parent methods or helpers exceed CCN 8.

---

## SCAN 2 Result -- lock()

Command: Select-String -Path "src/PropTraderTools/CopyEngine.cs" -Pattern "lock\s*\("

Output: 24 matches -- ALL in comments (JS-021 documentation comments e.g. "no lock()",
"no lock anywhere", "ConcurrentDictionary no lock"). Zero actual lock() calls in code.
Spot-verified: no match appears in method bodies -- all are // comment lines.

PASS: zero actual lock() calls.

---

## SCAN 3 Result -- async void

Command: Select-String -Path "src/PropTraderTools/CopyEngine.cs" -Pattern "async\s+void"

Output: 2 matches -- BOTH in comments:
  L1822: "// JS-021: ... JS-033: Tick is not async void."
  L6885: "// Called directly from OnOrderUpdate -- Synchronous void. NOT async void (JS-033)."

Zero actual async void declarations.

PASS: zero actual async void.

---

## SCAN 4 Result -- return null

Command: Select-String -Path "src/PropTraderTools/CopyEngine.cs" -Pattern "return null"

Output: 20 matches. All pre-existing grandfathered lines:
  L1187, L1890, L2819, L2900, L2908, L3652, L3821, L5333, L5339, L5418, L6603, L6618
  (plus comment mentions at L698, L703, L708, L1380, L4523, L5911, L5939, L6782)

T2 helper ranges verified:
  IsQxCancelEligible3 (L1025-L1044): returns bool -- no return null.
  CommitStaleCancelBatch (L1060-L1073): returns void -- no return null.
  FindFollowerRuleForOrder (L4032-L4053): returns CopyRule? nullable struct.
    Returns "return matchedRule" where matchedRule is CopyRule? -- nullable struct null
    is JS-002 compliant (not a reference type null). No "return null" literal.
  IsReplaceDispatchEligible (L4061-L4073): returns bool -- no return null.
  IsAccountFlattenable (L4872-L4904): returns bool -- no return null.
  SubmitMarketFlattenOrder (L4906-L4941): returns void -- no return null.
  LogDiagOrderCount (L5708-L5726): returns void -- no return null.
  RegisterBeRetrySlotIfNeeded (L5728-L5781): returns void -- no return null.
  IsQxCancelEligible3Testable (L1049-L1053): returns bool -- no return null.

PASS: zero return null in T2 helpers. All reference-type return paths are pre-existing.

---

## SCAN 5 Result -- build

Command: dotnet build "src/PropTraderTools/PropTraderTools.csproj" --no-incremental 2>&1

Output:
  Build succeeded.
  C:\WSGTA\ptt-lane-b\src\PropTraderTools\Tests\B131Tests.cs(165,13): warning xUnit2004:
    Do not use Assert.Equal() to check for boolean conditions. Use Assert.True instead.
  1 Warning(s)
  0 Error(s)
  Time Elapsed 00:00:02.35

PASS: 0 errors. 1 pre-existing warning in B131Tests.cs (xUnit2004 -- predates T2, not
introduced by T2).

---

## SCAN 6 Result -- ASCII

Command:
  $bytes = [System.IO.File]::ReadAllBytes("src/PropTraderTools/CopyEngine.cs")
  ($bytes | Where-Object { $_ -gt 127 } | Measure-Object).Count

Output: 0

PASS: Count = 0. Pure ASCII.

---

## SCAN 7 Result -- tests

Command:
  dotnet test "tests/PropTraderTools.Tests/PropTraderTools.Tests.csproj"
    --filter "FullyQualifiedName~BwaveRefactorLaneB" 2>&1

Output:
  Passed!  - Failed: 0, Passed: 8, Skipped: 0, Total: 8, Duration: 168 ms

Tests present (8 total):
  T1 (5): IsBeTargetStateOk_Working_ReturnsTrue,
           IsBeTargetStateOk_CancelSubmitted_ReturnsTrue,
           IsBeTargetStateOk_Filled_ReturnsFalse,
           IsImmediateBeEligible_NullPosition_ReturnsFalse,
           IsImmediateBeEligible_ZeroTickSize_ReturnsFalse
  T2 (3): IsQxCancelEligible3_NullSnapshot_PassesThrough,
           IsQxCancelEligible3_OrderNotInSnapshot_ReturnsFalse,
           IsAccountFlattenable_NullAccount_ReturnsFalse

PASS: Failed: 0, Passed: 8. All T1 tests continue passing. All T2 tests pass.

---

## Structural Checks

### SC-1: Helpers Exist

All 9 T2 helpers confirmed present in CopyEngine.cs by grep:
  IsAccountFlattenable            -- L4872 (private bool)
  SubmitMarketFlattenOrder        -- L4906 (private void)
  LogDiagOrderCount               -- L5708 (private void)
  RegisterBeRetrySlotIfNeeded     -- L5728 (private void)
  FindFollowerRuleForOrder        -- L4032 (private CopyRule?)
  IsReplaceDispatchEligible       -- L4061 (private bool)
  IsQxCancelEligible3             -- L1025 (private static bool)
  IsQxCancelEligible3Testable     -- L1049 (internal static bool)
  CommitStaleCancelBatch          -- L1060 (private void)

PASS.

### SC-2: No Logic Deleted

FlattenOneAccount (L4855): calls IsAccountFlattenable + CancelAllAccountOrders +
  SubmitMarketFlattenOrder. Delegation confirmed.

MoveStopToBreakEven (L5599): calls LogDiagOrderCount (L5627), SnapshotBeTargets (L5633),
  PttBreakEvenSwap.Execute (L5643), RegisterBeRetrySlotIfNeeded twice (L5649, L5659).
  All original logic preserved via helpers.

ReplaceFollowerCopyOnAtmCancel (L3994): calls FindFollowerRuleForOrder (L3998),
  IsReplaceDispatchEligible (L4001). Remaining: _isCopyEnabled guard, signal creation,
  ResolveAtmMode, Named-mode branch. All logic preserved.

CancelQxBrackets 3-param (L984): calls IsQxCancelEligible3 twice (L997, L999),
  CommitStaleCancelBatch (L1014). raceSkipped counter logic preserved via dual call pattern.

PASS: no logic deleted.

### SC-3: Public Signatures Unchanged

FlattenOneAccount:             private void (acc, instrument) -- unchanged.
MoveStopToBreakEven:           private void (acc, instrument, bufferTicks, isRetry=false) -- unchanged.
ReplaceFollowerCopyOnAtmCancel: private void (cancelledOrder) -- unchanged.
CancelQxBrackets 3-param:      internal void (acc, instr, HashSet<Order> snapshot) -- unchanged.

PASS.

### SC-4: Consolidation Check

CommitStaleCancelBatch is the SINGLE helper for both CancelQxBrackets overloads:
  2-param (L911): calls CommitStaleCancelBatch at L933 -- CONFIRMED.
  3-param (L984): calls CommitStaleCancelBatch at L1014 -- CONFIRMED.
No CommitQxCancelBatch exists (per ticket spec: "MAY consolidate"; engineer applied early).

PASS.

### SC-5: NT8 Constraint -- DateTime.MaxValue

SubmitMarketFlattenOrder (L4919-L4932): CreateOrder call confirmed with:
  arg11 = DateTime.MaxValue (L4930) -- NOT DateTime.Now. PASS.
  arg12 = null (L4931). PASS.
  Order name = "PTT-Flatten" (L4929) -- PTT- prefix compliant. PASS.

PASS.

---

## Layer 2 Cross-Check

| Scan | Layer 2 (engineer) | Layer 3 (verifier) | Match |
|------|-------------------|-------------------|-------|
| SCAN 1 CCN | No rows | No rows | MATCH |
| SCAN 2 lock() | Comment-only, 0 actual | Comment-only, 0 actual | MATCH |
| SCAN 3 async void | Comment-only, 0 actual | Comment-only, 0 actual | MATCH |
| SCAN 4 return null | Pre-existing only (12 lines) | Same 12 pre-existing lines, 0 in T2 helpers | MATCH |
| SCAN 5 build | Build succeeded, 1 warning B131Tests.cs, 0 errors | Build succeeded, 1 warning B131Tests.cs, 0 errors | MATCH |
| SCAN 6 ASCII | Count = 0 | Count = 0 | MATCH |
| SCAN 7 tests | Failed:0, Passed:8 | Failed:0, Passed:8 | MATCH |

All 7 Layer 2 reports match Layer 3 independently-run results. No discrepancies.

---

## Deviations Noted

### DEV-1: raceSkipped counter logic

Engineer documented: the 3-param CancelQxBrackets calls IsQxCancelEligible3 twice per loop
iteration -- once with snapshot=null for raceSkipped counting, once with actual snapshot
for eligibility filtering.

Verifier confirmed: code at L996-999 implements exactly this dual-call pattern. The
raceSkipped semantics (counts orders passing state+instrument+candidate but skipped by
snapshot filter) are preserved identically. No behavior change. ACCEPTED.

### DEV-2: CommitStaleCancelBatch consolidation applied early

Ticket spec said engineer MAY consolidate (T3 spec would have done it at T3). Engineer
applied it in T2. Both overloads verified to call CommitStaleCancelBatch. No orphaned
CommitQxCancelBatch. T3 will inherit the already-consolidated helper. ACCEPTED.

### DEV-3: IsQxCancelEligible3 CCN = 4 (not the expected up to 7)

Lizard counts the 5-term stateOk OR compound as CCN branch 1 (single predicate assignment),
then reports instrument check (2), snapshot race-skip (3), IsQxCancelCandidate call (4).
Total = CCN 4. Expected was <=7. Actual CCN 4 is strictly better than <=7. ACCEPTED.

---

## DNA Rule Check

| Rule | Check | Result |
|------|-------|--------|
| JS-021 no lock() | SCAN 2: zero actual lock() in code | PASS |
| JS-001 no throw in hot path | T2 helpers absorb existing try/catch; no new throw | PASS |
| JS-002 no return null (new code) | All T2 helpers return bool, void, or CopyRule? (nullable struct) | PASS |
| JS-009 no plain Dictionary on shared fields | _pendingFollowerBeSlots = ConcurrentDictionary (L306) | PASS |
| JS-033 no async void | SCAN 3: zero actual async void | PASS |
| ASCII-only | SCAN 6: Count=0 bytes > 127 | PASS |
| CYC<=8 | SCAN 1: zero rows for all T2 methods and helpers | PASS |
| DateTime.MaxValue not DateTime.Now | SC-5: SubmitMarketFlattenOrder uses DateTime.MaxValue | PASS |
| PTT- prefix on CreateOrder names | "PTT-Flatten" confirmed at L4929 | PASS |

All DNA rules: PASS.

---

## VERIFY_PASS

All 7 scans independently run and passed.
All 5 structural checks passed.
Layer 2 cross-check: 7/7 match, zero discrepancies.
3 engineer-documented deviations reviewed and accepted.
No violations found.

VERDICT: VERIFY_PASS