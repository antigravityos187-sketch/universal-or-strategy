# BWAVE-CYC Lane-A Ticket TA-R4 -- Verifier Report (Second Pass)

**Status**: VERIFY_PASS
**File**: `src/PropTraderTools/CopyEngine.cs`
**Ticket**: TA-R4 RETRY -- TryFireFollowerBeRetry + TryEvictFollowerBeSlot + CancelPttDragOrphansForAccount
**Verifier**: ptt-verifier (independent Layer 3, second pass)
**Date**: 2025-08-27

---

## Prior VERIFY_FAIL Blockers -- All Resolved

| Method | Prior CCN | Retry CCN | Ceiling | Result |
|--------|-----------|-----------|---------|--------|
| TryFireFollowerBeRetry | 10 | **7** | <= 8 | **PASS** |
| TryEvictFollowerBeSlot | 11 | **7** | <= 8 | **PASS** |
| IsBePendingTargetOrder | 6 | **2** | <= 4 | **PASS** |
| IsPttDragOrderCancellable | 6 | **3** | <= 4 | **PASS** |

All 4 VERIFY_FAIL blockers from LaneA-TA-R4-verify.md are confirmed resolved.

---

## 7 Mandatory Scans (All Independent -- Do NOT Trust Engineer Self-Report)

### SCAN-01 -- lock() [PASS]
Command: `Select-String -Path src/PropTraderTools/*.cs -Pattern "lock\("`
Result: 7 hits -- ALL are comment-only references (e.g. "// JS-021: no lock()...").
Zero executable lock() calls found.
Verdict: **PASS**

### SCAN-02 -- async void [PASS]
Command: `Select-String -Path src/PropTraderTools/*.cs -Pattern "async void "`
Result: 3 hits -- ALL are comment-only references.
Zero executable async void declarations found.
Verdict: **PASS**

### SCAN-03 -- return null (new instances) [PASS]
Command: `Select-String -Path src/PropTraderTools/*.cs -Pattern "return null"`
Result: Multiple pre-existing hits. TA-R4 modified region is L1485-L1720 in CopyEngine.cs.
Nearest executable `return null` in CopyEngine.cs is at L1805 -- outside TA-R4 scope.
All 10 new helpers return bool or void. Zero return null in any new method.
Verdict: **PASS** (0 new instances)

### SCAN-04 -- throw new (new instances) [PASS]
Command: `Select-String -Path src/PropTraderTools/*.cs -Pattern "throw new "`
Result: 1 hit -- TradeCopierWindow.cs L871 (NotImplementedException in AccountDisplayConverter).
Pre-existing. Not in TA-R4 scope.
Verdict: **PASS** (0 new instances)

### SCAN-05a -- lizard CCN [PASS]
Command: `lizard src/PropTraderTools/CopyEngine.cs --CCN 8`

Raw lizard output for TA-R4 methods:

```
  NLOC    CCN   token  PARAM  length  location
     4      3     39      1       4 TrimSignal::IsPttQxTargetOrder@1485-1488
     4      3     39      1       4 TrimSignal::IsNativeAtmBeRetryTarget@1496-1499
     6      2     23      1       6 TrimSignal::IsBePendingTargetOrder@1506-1511
     2      2     22      1       2 TrimSignal::IsBeRetryEligibleOrderState@1517-1518
     2      3     22      1       2 TrimSignal::IsBeRetryOrderInvalid@1524-1525
    24      7    148      1      24 TrimSignal::TryFireFollowerBeRetry@1532-1555
     2      2     20      1       2 TrimSignal::IsPttBeStopRejected@1581-1582
    12      2     46      2      12 TrimSignal::LogBeSlotEviction@1588-1599
     2      2     15      2       2 TrimSignal::IsBeSlotNonTerminal@1605-1606
     2      2     26      2       2 TrimSignal::IsBeFilledWithOpenPosition@1612-1613
    21      7    135      1      21 TrimSignal::TryEvictFollowerBeSlot@1623-1643
     2      2     18      1       2 TrimSignal::IsPttDragOrderName@1673-1674
     2      3     21      2       2 TrimSignal::IsDragInstrumentMatch@1680-1681
     4      3     29      2       4 TrimSignal::IsPttDragOrderCancellable@1689-1692
    17      5     92      2      17 TrimSignal::CancelPttDragOrphansForAccount@1700-1716
```

Target methods NOT in warnings list (CCN > 8):
- TryFireFollowerBeRetry: CCN=7 -- NOT in warnings list (required <= 8) PASS
- TryEvictFollowerBeSlot: CCN=7 -- NOT in warnings list (required <= 8) PASS
- CancelPttDragOrphansForAccount: CCN=5 -- NOT in warnings list (required <= 8) PASS

All new helpers CCN <= 4:
- IsPttQxTargetOrder: CCN=3 <= 4 PASS
- IsNativeAtmBeRetryTarget: CCN=3 <= 4 PASS
- IsBePendingTargetOrder: CCN=2 <= 4 PASS
- IsBeRetryEligibleOrderState: CCN=2 <= 4 PASS
- IsBeRetryOrderInvalid: CCN=3 <= 4 PASS
- IsBeSlotNonTerminal: CCN=2 <= 4 PASS
- IsBeFilledWithOpenPosition: CCN=2 <= 4 PASS
- IsPttDragOrderName: CCN=2 <= 4 PASS
- IsDragInstrumentMatch: CCN=3 <= 4 PASS
- IsPttDragOrderCancellable: CCN=3 <= 4 PASS

Verdict: **PASS** -- All 4 prior blockers absent from warnings. All helpers compliant.

### SCAN-05b -- cs delta (Code Health) [PASS]
Command: `cs delta` (with CS_ACCESS_TOKEN)
Result: CopyEngine.cs Code Health 1.61 -> 1.87 (improvement).
TryFireFollowerBeRetry: [X] Fixed issue Complex Method
TryEvictFollowerBeSlot: [X] Fixed issue Complex Method
TryFireFollowerBeRetry: [X] Fixed issue Complex Conditional
Mean cyclomatic complexity: 4.79 -> 4.28 (improvement).
Degraded items (Lines of Code, Function Count) are pre-existing BWAVE-CYC wave growth.
Exit code 1 is pre-existing known issue (docs/Real Estate/ non-ASCII path).
Verdict: **PASS** (Code Health improved, no regression from TA-R4)

### SCAN-06 -- dotnet build [PASS]
Command: `dotnet build src/PropTraderTools/PropTraderTools.csproj`
Result:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```
Verdict: **PASS**

### SCAN-07 -- dotnet test [PASS]
Command: `dotnet test src/PropTraderTools/PropTraderTools.csproj --no-build`
Result: Failed: 22, Passed: 456, Skipped: 15, Total: 493

Baseline confirmation: The 22 failing tests were verified to fail on HEAD (committed code
before TA-R4 changes) by running with git stash and restoring. B135/B136 failures and
T_MSTBE_CR_02 (TargetParameterCountException IL-reflection) are all pre-existing.
22 pre-existing failures -- accepted, baseline confirmed.
456 passed (up from 453 in first-pass verify; +3 from TA-R4 retry new tests).
Zero new failures introduced by TA-R4.
Verdict: **PASS** (0 new failures)

---

## Behaviour Verification

All 3 parent method bodies and all 10 new helpers read independently (L1485-L1720).

### TryFireFollowerBeRetry (L1532-L1555, CCN=7)
Guard chain: e?.Order -> IsBeRetryOrderInvalid(o) -> !IsBePendingTargetOrder(o)
            -> !IsBeRetryEligibleOrderState(o) -> !TryRemove(slot) -> IsFlat(FindPosition)
All decisions accounted for in CCN=7 comment at L1527-1531.
IsBeRetryOrderInvalid absorbs triple null-guard (2 || branches removed from parent).
IsBeRetryEligibleOrderState absorbs Working/Accepted state pair (1 && removed from parent).
No logic changes. No new early returns. No reordering.
Behaviour: **IDENTICAL**

### TryEvictFollowerBeSlot (L1623-L1643, CCN=7)
Guard chain: e?.Order -> o null -> IsBeSlotNonTerminal(isFilled, isRejected)
            -> !IsFollowerAccount(o.Account) -> IsBeFilledWithOpenPosition(o, isFilled)
            -> slotEvicted gate -> LogBeSlotEviction(accName, isRejected)
o.Account.Name simplification (removed ?. and ??) -- safe: IsFollowerAccount guard above.
DW-B81-01 Rejected-path bypass of flat-guard preserved (isRejected path through IsBeFilledWithOpenPosition
where isFilled=false makes entire expression false, allowing execution to continue past guard).
_filledBeTargetCount.TryRemove (DW-B92) present at L1640.
_entryDispatchedOrders.Clear (DW-B95) present at L1632.
Behaviour: **IDENTICAL**

### CancelPttDragOrphansForAccount (L1700-L1716, CCN=5)
Unchanged from first pass. foreach -> IsPttDragOrderCancellable guard -> try/catch Cancel.
Behaviour: **IDENTICAL**

### Helper Verification
- IsBePendingTargetOrder (L1506-L1511): delegates to IsPttQxTargetOrder OR IsNativeAtmBeRetryTarget.
  Semantically equivalent to original inline isPttQxT OR IsNativeAtmTargetOrder expression.
- IsPttDragOrderCancellable (L1689-1692): Working && IsDragInstrumentMatch && IsPttDragOrderName.
  Semantically equivalent to original Working && instrument-match && (TGT-Drag || STP-Drag).
All 10 helpers are `private`. No public surface added. No logic reordering.
Behaviour verification: **PASS**

---

## Engineer Self-Report vs Verifier Layer 3 -- Alignment Table (Retry Pass)

| Method | Engineer Claimed CCN | Lizard Measured CCN | Match? |
|--------|---------------------|---------------------|--------|
| TryFireFollowerBeRetry | 7 | **7** | YES |
| TryEvictFollowerBeSlot | 7 | **7** | YES |
| IsBePendingTargetOrder | 2 | **2** | YES |
| IsPttDragOrderCancellable | 3 | **3** | YES |
| IsPttQxTargetOrder | 3 | **3** | YES |
| IsNativeAtmBeRetryTarget | 3 | **3** | YES |
| IsBeRetryEligibleOrderState | 2 | **2** | YES |
| IsBeRetryOrderInvalid | 3 | **3** | YES |
| IsBeSlotNonTerminal | 2 | **2** | YES |
| IsBeFilledWithOpenPosition | 3 | **2** | Close (CCN=2, within ceiling of 4) |
| IsPttDragOrderName | 2 | **2** | YES |
| IsDragInstrumentMatch | 3 | **3** | YES |

Note: IsBeFilledWithOpenPosition -- engineer claimed CCN=3, lizard measures CCN=2. Both are within the <= 4 ceiling. Not a discrepancy requiring action.

---

**VERIFY_PASS -- TA-R4**

All 4 prior blockers resolved. All 7 scans pass. Behaviour identical. Zero new failures.