# LaneA-TA-R5 Verification Report

**Ticket**: TA-R5
**Wave**: BWAVE-CYC Lane-A
**Verifier**: ptt-verifier (independent)
**File**: `src/PropTraderTools/CopyEngine.cs`
**Verdict**: VERIFY_PASS

---

## CCN Results (Independent Lizard Run)

| Method | Type | CCN Ceiling | CCN Actual | Pass |
|--------|------|-------------|------------|------|
| `IsReArmedAtmBracketCleanupRequired` | parent (helper target) | <= 4 | 4 | YES |
| `ReplaceFollowerCopyOnAtmCancel` | parent | <= 8 | 7 | YES |
| `TryFindRuleAndFollowerIndex` | parent (helper target) | <= 4 | 3 | YES |
| `TryReplacePttBeBrackets` | parent | <= 8 | 8 | YES |
| `SendAtmCancelReplace` | new helper | <= 4 | 3 | YES |
| `TryMatchFollowerInRule` | new helper | <= 4 | 3 | YES |
| `IsBeReplaceTargetValid` | new helper | <= 4 | 3 | YES |
| `TryIncrementBeReplaceAttempt` | new helper | <= 4 | 2 | YES |
| `IsQxTOrderStateValid` | new helper | <= 4 | 2 | YES |
| `IsQxTBracketNameValid` | new helper | <= 4 | 4 | YES |
| `TryGetCleanupEntryForFollower` | new helper | <= 4 | 3 | YES |
| `IsCleanupEntryCurrentAndMatching` | new helper | <= 4 | 4 | YES |

All 4 target methods ABSENT from lizard --CCN 8 warnings list.
Warnings list: 20 pre-existing entries (identical to engineer report).

---

## 7 Mandatory Scans (All Independent)

### SCAN-01: lock() check
Command: `Get-ChildItem src/PropTraderTools -Filter *.cs -Recurse | Select-String -Pattern "lock\("`
Result: All hits are in COMMENTS only ("no lock()", "lock-free", etc.)
**PASS -- 0 executable lock() calls**

### SCAN-02: async void check
Command: `Get-ChildItem src/PropTraderTools -Filter *.cs -Recurse | Select-String -Pattern "async void "`
Result: All hits are in COMMENTS only ("no async void", "not async void")
**PASS -- 0 executable async void declarations**

### SCAN-03: return null (new instances vs baseline)
Command: `Get-ChildItem src/PropTraderTools -Filter *.cs -Recurse | Select-String -Pattern "return null"`
Result: TA-R5 new code (lines 3736-4049) contains ZERO `return null` statements.
All new helpers return `bool` or `void`. No new `return null` introduced.
**PASS -- 0 new return null vs pre-wave baseline**

### SCAN-04: throw new (new instances vs baseline)
Command: `Get-ChildItem src/PropTraderTools -Filter *.cs -Recurse | Select-String -Pattern "throw new "`
Result: 2 pre-existing hits -- TradeCopierWindow.cs:871, Tests/B42Tests.cs:72
Neither is in TA-R5 scope (lines 3736-4049). No new `throw new` introduced.
**PASS -- 0 new throw new vs pre-wave baseline**

### SCAN-05a: lizard CCN check
Command: `lizard src/PropTraderTools/CopyEngine.cs --CCN 8`
Result: All 4 target methods absent from >8 warnings list.
20 pre-existing warnings (all confirmed pre-existing). All new helpers CCN <= 4.
Full CCN table: see section above.
**PASS**

### SCAN-05b: CodeScene delta
Command: `$env:CS_ACCESS_TOKEN=...; cs delta`
Result:
  CopyEngine.cs:       Code Health 1.61 -> 1.97  [IMPROVED]
  CopyEngineTests.cs:  Code Health 4.93 -> 4.93  [UNCHANGED]
  TradeCopierPanel.cs: Code Health 4.71 -> 6.08  [IMPROVED -- LaneC work]
  TradeCopierWindow.cs: Code Health 6.61 -> 7.43 [IMPROVED -- LaneC work]
  BwaveCycLaneCTests.cs: 9.38 -> 6.89 [LaneC test file, not TA-R5 scope]
Fixed issues reported for TA-R5 scope: IsReArmedAtmBracketCleanupRequired,
TryReplacePttBeBrackets, TryFindRuleAndFollowerIndex -- all no longer above threshold.
Exit code 1 = version-update notice from cs stderr (pre-existing, not a regression).
**PASS -- Code Health for CopyEngine.cs IMPROVED**

### SCAN-06: dotnet build
Command: `dotnet build archive/v12-reference/Linting.csproj`
Result: Build succeeded. 0 Warning(s). 0 Error(s).
**PASS**

### SCAN-07: dotnet test
Command: `dotnet test src/PropTraderTools/PropTraderTools.csproj --no-build`
Result: Failed: 22, Passed: 463, Skipped: 15, Total: 500
Baseline: 436 pass / 22 pre-existing IL-reflection / 15 skips
22 pre-existing IL-reflection failures -- accepted, baseline confirmed.
Pass count increase (436 -> 463) reflects 8 new TA-R5 tests plus prior wave additions.
**PASS -- 0 new failures. 22 pre-existing IL-reflection failures confirmed.**

---

## Behaviour Verification (Method-by-Method)

### ReplaceFollowerCopyOnAtmCancel (L3736-3761)
Read body: 6 guard-returns unchanged. Logic extracted: dispatch block moved to
SendAtmCancelReplace. No early returns added/removed to parent. Signal construction
identical. Private.
**PASS -- no logic change, no new early returns**

### SendAtmCancelReplace (L3767-3780) -- new helper
Absorbs mode-is-Named branch (1) + StatusUpdate?.Invoke null-conditional (1) from parent.
All 3 statements preserved: ResolveAtmMode, if/else dispatch, StatusUpdate?.Invoke.
Private. CCN=3.
**PASS**

### TryMatchFollowerInRule (L3785-3793) -- new helper
Uses Array.FindIndex instead of explicit inner for-loop. Result semantically identical:
finds first FollowerAccount.Name matching cancelledOrder.Account.Name. Returns index >= 0.
Null guard on followers preserved. Private. CCN=3.
**PASS -- behaviour identical via Array.FindIndex equivalence**

### TryFindRuleAndFollowerIndex (L3799-3817)
Foreach over _rules unchanged. Instrument continue unchanged. Inner for-loop replaced
with call to TryMatchFollowerInRule. Return matchedRule.HasValue unchanged. Out param
defaults (null, -1) preserved as specified in architect plan risk notes.
Private. CCN=3.
**PASS -- no logic change**

### IsBeReplaceTargetValid (L3856-3857) -- new helper
Expression body: cancelledStop != null && .Account != null && .Instrument != null.
Absorbs null compound guard from TryReplacePttBeBrackets guard-return (1).
Private. CCN=3.
**PASS**

### TryIncrementBeReplaceAttempt (L3862-3879) -- new helper
Absorbs attempt-count cap logic (DW-B111 cap=5), diagnostic log, and counter increment.
ConcurrentDictionary TryGetValue/index-set. Returns false when >= 5, true when under cap.
Private. CCN=2.
**PASS**

### TryReplacePttBeBrackets (L3889-3928)
Guards (1)-(5) all present. IsBeReplaceTargetValid replaces inline null check (1).
TryIncrementBeReplaceAttempt replaces inline attempt block (4). HasActiveQxOrdersForInstrument
(3c) was pre-existing extraction. _pendingFollowerBeSlots.TryAdd slot + QueueBeRetryFallback
unchanged. Diagnostic log at (5) preserved. Private void. CCN=8.
**PASS -- no logic change**

### IsReArmedAtmBracketCleanupRequired (L4039-4049)
4 if-branches: IsQxTOrderStateValid, IsQxTBracketNameValid, TryGetCleanupEntryForFollower,
IsCleanupEntryCurrentAndMatching. Out param threads entry to parent -- cleaner than
architect-plan alternative (re-fetch). Semantically equivalent per architect risk note
("safe because single-threaded NT8 dispatch; same entry will be found"). Private. CCN=4.
**PASS -- correct out-param design, no logic change**

### Sub-helpers for IsReArmedAtmBracketCleanupRequired
- IsQxTOrderStateValid (L4002): Working||Accepted check. CCN=2. Pure predicate.
- IsQxTBracketNameValid (L4007): null + StartsWith("PTT-QX-T") + Length>=9 + IsDigit[8]. CCN=4.
- TryGetCleanupEntryForFollower (L4016): Account null||!IsFollower + TryGetValue. CCN=3.
- IsCleanupEntryCurrentAndMatching (L4029): Expiry>UtcNow && FullName match. CCN=4. DateTime.UtcNow confirmed.
All private, all CCN <= 4, no side effects in predicates.
**PASS**

---

## Architect Plan Compliance (T4 Section)

| Architect Spec | Status |
|---|---|
| IsReArmedAtmBracketCleanupRequired private helper CCN<=4 | PASS (CCN=4) |
| TryFindRuleAndFollowerIndex private helper CCN<=4 | PASS (CCN=3) |
| Out-param defaults matchedRule=null, followerIndex=-1 | PASS |
| HasActiveQxOrdersForInstrument: diagnostic log preserved inside helper | PASS |
| HasActiveQxOrdersForInstrument: .ToList() snapshot preserved | PASS |
| TryReplacePttBeBrackets parent CCN (plan says <=7, ticket says <=8) | CCN=8 -- meets ticket ceiling <=8. Ticket is controlling spec. |
| All helpers private | PASS |
| No new public/internal surface | PASS |
| FindMatchingNativeAtmBracket preserved (T4 pre-existing) | PASS (L4053) |

Note on TryReplacePttBeBrackets CCN=8 vs plan <=7: The architect plan T4 originally
planned one extraction (HasActiveQxOrdersForInstrument). TA-R5 executed two additional
extractions (IsBeReplaceTargetValid, TryIncrementBeReplaceAttempt) that post-date the
original plan due to DW-B111 cap raise. The ticket specification <=8 is the controlling
ceiling and is met. The architect plan <=7 was drafted before the DW-B111 complexity
additions were committed.

---

## DNA Rule Check

| Rule | Check | Result |
|------|-------|--------|
| JS-021 no lock() | SCAN-01 independent run | PASS |
| JS-002 no new return null | SCAN-03 independent run | PASS |
| JS-033 no async void | SCAN-02 independent run | PASS |
| JS-001 no new throw new | SCAN-04 independent run | PASS |
| NT8-013 DateTime.UtcNow only | IsCleanupEntryCurrentAndMatching L4032 confirmed | PASS |
| NT8-014 CreateOrder "PTT-" prefix | No new CreateOrder calls introduced | N/A |
| All new helpers private | Confirmed via body reads | PASS |
| No new public/internal surface | Confirmed via body reads | PASS |
| mutable struct used across threads | No new structs introduced | N/A |
| SolidColorBrush not frozen | No WPF brushes in these helpers | N/A |
| FontFamily= on WPF element | No WPF in these helpers | N/A |
| hex color string #RRGGBB | No color strings in these helpers | N/A |

---

## [Fact] Tests Verification

Engineer reported 8 new tests added (426 -> 434 [Fact] count in CopyEngineTests.cs).
Test run confirms 22 IL-reflection failures (baseline) and 0 new failures.
All 8 new test names follow private helper existence/contract pattern.
Architect-specified test names from T4 all confirmed present (10 tests listed in engineer
report match T4 requirements at lines 162-166 and 175-177 and 185-187 of architect plan).

---

## Summary

All 7 scans PASS. All 4 target methods within CCN ceilings. All 8 new helpers CCN<=4.
No DNA violations. No new test failures. Code Health improved. Architect plan compliant
(ticket ceiling is controlling spec for TryReplacePttBeBrackets CCN=8).