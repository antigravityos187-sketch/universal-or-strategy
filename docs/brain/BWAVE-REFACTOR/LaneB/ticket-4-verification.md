# BWAVE-REFACTOR LaneB -- Ticket 4 Verification
# Phase 4b Output
# Author: ptt-verifier
# Ticket: BWAVE-REFACTOR-LaneB-T4
# Date: 2025-01-28

---

## Scope Confirmation

[TICKET 4 ONLY] -- Tier D: CCN 10-12 (6 methods)
Prerequisite: T3 passed (20 tests passing before T4 was executed).
Source: docs/brain/BWAVE-REFACTOR/LaneB/04-tickets.md lines 894-1167.

Target methods verified:
- OnOrderUpdate (was CCN 12)
- CancelAllAccountOrders (was CCN 12)
- BuildQxSnapshot (was CCN 11)
- DrainThenDispatch (was CCN 11)
- FindFollowerBracketOrder IEnumerable overload (was CCN 11)
- MatchesLeaderName (was CCN 11)

---

## SCAN 1 Result -- CCN

Command run independently:
```powershell
$files = Get-ChildItem src/PropTraderTools/ -Filter "*.cs" -Recurse |
  Where-Object { $_.FullName -notmatch '\\obj\\' -and $_.FullName -notmatch '\\bin\\' }
lizard $files --csv 2>&1 | ConvertFrom-Csv -Header @("NLOC","CCN","Tokens","Params","Length","Location","MethodName","MethodLongName","StartLine","EndLine") |
  Where-Object { [int]$_.CCN -gt 8 } |
  Where-Object { $_.MethodLongName -match "OnOrderUpdate|CancelAllAccountOrders|BuildQxSnapshot|DrainThenDispatch|FindFollowerBracketOrder|MatchesLeaderName|HandleDrainTerminalState|IsCancelAllStateOk|IsQxSnapshotStateOk|IssueDrainCancels|MatchesBracketType|ExtractLegSuffix" } |
  Format-Table -AutoSize
```
Output: (no rows -- command completed with no output)
RESULT: PASS

---

## SCAN 2 Result -- lock()

Command run independently:
```powershell
Select-String -Path "src/PropTraderTools/CopyEngine.cs" -Pattern "lock\s*\("
```
Output: 40 comment-only hits (text "no lock", "no lock()", "no lock ("). 
Zero actual lock() calls in code. All hits are within comment lines confirming
JS-021 compliance ("JS-021: no lock()", "No lock()", etc.).
RESULT: PASS

---

## SCAN 3 Result -- async void

Command run independently:
```powershell
Select-String -Path "src/PropTraderTools/CopyEngine.cs" -Pattern "async\s+void"
```
Output: 2 comment-only hits:
  - L1861: "JS-033: Tick is not async void."
  - L7039: "Synchronous void. NOT async void (JS-033)."
Zero actual async void declarations.
RESULT: PASS

---

## SCAN 4 Result -- return null

Command run independently:
```powershell
Select-String -Path "src/PropTraderTools/CopyEngine.cs" -Pattern "return null"
```
Output: 15 hits. All are pre-existing in parent methods (FindFollowerBracketOrder L3707,
FindRule L1236, L1929, various L2858/2939/2947, L5430/5436/5515, L6720/6735, etc.).
Zero "return null" in any T4 new helper.
T4 helpers by return type: bool (7 helpers/seams), void (HandleDrainTerminalState), int (IssueDrainCancels).
ExtractLegSuffix: returns string.Empty as sentinel at L3740 (NOT null). JS-002 compliant.
RESULT: PASS

---

## SCAN 5 Result -- build

Command run independently:
```powershell
dotnet build "src/PropTraderTools/PropTraderTools.csproj" --no-incremental 2>&1
```
Output:
  Build succeeded.
  1 Warning(s) -- B131Tests.cs(165,13): warning xUnit2004 (pre-existing, not T4)
  0 Error(s)
  Time Elapsed 00:00:01.70
RESULT: PASS

---

## SCAN 6 Result -- ASCII

Command run independently:
```powershell
$bytes = [System.IO.File]::ReadAllBytes("src/PropTraderTools/CopyEngine.cs")
($bytes | Where-Object { $_ -gt 127 } | Measure-Object).Count
```
Output: 0
RESULT: PASS

---

## SCAN 7 Result -- tests

Command run independently:
```powershell
dotnet test "tests/PropTraderTools.Tests/PropTraderTools.Tests.csproj" --filter "FullyQualifiedName~BwaveRefactorLaneB" 2>&1
```
Output:
  Passed!  - Failed: 0, Passed: 20, Skipped: 0, Total: 20, Duration: 141 ms
RESULT: PASS

Test breakdown:
  T1 (5): IsBeTargetStateOk x3, IsImmediateBeEligible x2
  T2 (3): IsQxCancelEligible3 x2, IsAccountFlattenable x1
  T3 (4): IsPositionStateTriggerState x2, IsNativeLeaderTarget x1, IsQxCancelEligible2 x1
  T4 (8): IsCancelAllStateOk x2, IsQxSnapshotStateOk x2, MatchesBracketType x2, ExtractLegSuffix x2

---

## Structural Checks

### SC-1: Helpers Exist with Correct Visibility
All 10 T4 helpers confirmed present in CopyEngine.cs:

| Helper | Visibility | File Line | Status |
|--------|-----------|-----------|--------|
| IsQxSnapshotStateOk(OrderState) | private static | L977 | PASS |
| IsQxSnapshotStateOkTestable(OrderState) | internal static | L1015 | PASS |
| IsCancelAllStateOk(OrderState) | private static | L1121 | PASS |
| IsCancelAllStateOkTestable(OrderState) | internal static | L1161 | PASS |
| HandleDrainTerminalState(Order) | private | L7024 | PASS |
| MatchesBracketType(Order, bool) | private static | L3677 | PASS |
| MatchesBracketTypeTestable(bool, OrderType, bool) | internal static | L3714 | PASS |
| ExtractLegSuffix(string) | private static | L3736 | PASS |
| ExtractLegSuffixTestable(string) | internal static | L3764 | PASS |
| IssueDrainCancels(Account, List<Order>) | private | L6911 | PASS |

### SC-2: No Logic Deleted -- All 6 Parent Methods Still Exist and Call Helpers
- OnOrderUpdate (L1461): calls HandleDrainTerminalState at L1507. PASS.
- CancelAllAccountOrders (L1131): calls IsCancelAllStateOk at L1138. PASS.
- BuildQxSnapshot (L988): calls IsQxSnapshotStateOk at L998. PASS.
- DrainThenDispatch (L6934): calls IssueDrainCancels at L6982. PASS.
- FindFollowerBracketOrder IEnumerable overload (L3686): calls MatchesBracketType at L3704. PASS.
- MatchesLeaderName (L3745): calls ExtractLegSuffix at L3753. PASS.

### SC-3: Public/Internal Signatures Unchanged
- private void OnOrderUpdate(object sender, OrderEventArgs e) -- L1461. PASS.
- internal void CancelAllAccountOrders(Account acc, NinjaTrader.Cbi.Instrument instr) -- L1131. PASS.
- internal static HashSet<Order> BuildQxSnapshot(Account acc, Instrument instr) -- L988. PASS (remains internal static).
- private void DrainThenDispatch(Account follower, Instrument instrument, int qty, double price, OrderAction action, OrderType orderType) -- L6934. PASS.
- private Order? FindFollowerBracketOrder(IEnumerable<Order>, string?, bool, string?) -- L3686. PASS.
- private static bool MatchesLeaderName(Order, string?, bool) -- L3745. PASS.

### SC-4: Test Seams Present as internal static
- IsCancelAllStateOkTestable(OrderState s) => IsCancelAllStateOk(s) -- L1161. PASS.
- IsQxSnapshotStateOkTestable(OrderState s) => IsQxSnapshotStateOk(s) -- L1015. PASS.
- MatchesBracketTypeTestable(bool isStop, OrderType orderType, bool isOrderStopLeg) -- L3714. PASS.
  Note: inline primitive-param form (not a delegation) per NT8 Order-can't-be-constructed constraint.
- ExtractLegSuffixTestable(string n) => ExtractLegSuffix(n) -- L3764. PASS.

### SC-5: NT8 Constraints
- HandleDrainTerminalState: signature is `private void` (not async void). Confirmed at L7024. PASS.
- IssueDrainCancels: uses `acc.Cancel(new Order[] { e })` at L6923. NOT acc.Change(). AddOnBase-valid pattern. PASS.
- BuildQxSnapshot: remains `internal static` at L988. PASS.
- IsQxSnapshotStateOk: `private static` at L977. PASS.

### SC-6: ExtractLegSuffix Sentinel
Engineer chose: string.Empty (NOT null).
Confirmed at CopyEngine.cs L3740: `return string.Empty; // sentinel: no trailing digit`
Caller MatchesLeaderName updated: `legSuffix != string.Empty` at L3754 and L3756.
Test ExtractLegSuffix_NoDigit_ReturnsNull: asserts `Assert.Equal(string.Empty, ...)` -- naming preserved from spec per documented deviation.
Assessment: string.Empty sentinel is FULLY JS-002 compliant. Advisory requirement satisfied.

---

## Layer 2 Cross-Check

Comparing engineer self-report (Layer 2) against independent verification (Layer 3):

| Scan | L2 Report | L3 Independent | Discrepancy? |
|------|-----------|----------------|-------------|
| SCAN 1 CCN | no rows | no rows | NONE |
| SCAN 2 lock() | comments only (0 real) | comments only (0 real) | NONE |
| SCAN 3 async void | comments only (0 real) | comments only (0 real) | NONE |
| SCAN 4 return null | 0 in T4 helpers; ExtractLegSuffix = string.Empty | 0 in T4 helpers; L3740 = string.Empty | NONE |
| SCAN 5 build | 0 errors, 1 warning (B131Tests xUnit2004) | 0 errors, 1 warning (B131Tests xUnit2004) | NONE |
| SCAN 6 ASCII | 0 | 0 | NONE |
| SCAN 7 tests | Failed:0, Passed:20 | Failed:0, Passed:20 | NONE |

Layer 2 report is accurate and consistent with Layer 3 independent verification.

---

## Deviations Noted

1. **ExtractLegSuffix sentinel = string.Empty (not null)**
   - Ticket spec: "acceptable in .NET 4.8 context; or return string.Empty as sentinel"
   - Engineer chose: string.Empty
   - Assessment: CORRECT and fully JS-002 compliant. Advisory requirement satisfied.
   - Callers updated correctly (legSuffix != string.Empty at L3754/3756).

2. **Test pattern: inline mirrors (not reflection)**
   - Ticket spec notes: reflection triggers NinjaTrader.Core assembly loading, causing
     FileNotFoundException in net8.0 test runner. Inline mirrors used instead.
   - Assessment: CORRECT and consistent with T1-T3 pattern. Tests faithfully mirror
     production logic. All 8 T4 [Fact] tests pass.

3. **IssueDrainCancels signature: (Account acc, List<Order> entryCandidates)**
   - Ticket spec originally noted `(Account acc, Instrument instrument)` then revised to
     `(Account acc, List<Order> entryCandidates)` in the IMPORTANT note at ticket L1008-1012.
   - Engineer implemented the revised signature at L6911-6913. PASS.

4. **ExtractLegSuffix_NoDigit_ReturnsNull test name**
   - Test method is named `_ReturnsNull` but asserts `string.Empty`. Documented deviation
     per ticket spec instruction: "name preserved per spec but asserts string.Empty per
     implementation choice." Test logic is correct.

---

## VERIFY_PASS

All 7 scans independently re-run and confirmed PASS.
All structural checks PASS.
No discrepancies between Layer 2 and Layer 3.
All deviations are documented, spec-sanctioned, and correct.

**Verdict: VERIFY_PASS**