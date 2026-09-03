# BWAVE-CYC Lane-A Ticket TA-R4 — Verifier Report

**Status**: VERIFY_FAIL
**File**: `src/PropTraderTools/CopyEngine.cs`
**Ticket**: TA-R4 — TryFireFollowerBeRetry + TryEvictFollowerBeSlot + CancelPttDragOrphansForAccount
**Verifier**: ptt-verifier (independent Layer 3)
**Date**: 2025-08-27

---

## Blocker — SCAN-05a: CCN Still Exceeds Threshold

Architecture plan constraint (LaneA-02-architect-plan.md line 10):
> Each parent after extraction CCN <= 8 (Jane Street strict standard)
> Each helper CCN <= 4 (leave headroom for future feature growth)

Independent lizard scan results:

| Method | Lizard CCN | Required | Result |
|--------|-----------|----------|--------|
| TryFireFollowerBeRetry (L1501) | **10** | <= 8 | **FAIL** |
| TryEvictFollowerBeSlot (L1578) | **11** | <= 8 | **FAIL** |
| CancelPttDragOrphansForAccount (L1639) | 5 | <= 8 | PASS |
| IsBePendingTargetOrder (L1485) | **6** | <= 4 | **FAIL** |
| IsPttBeStopRejected (L1553) | 2 | <= 4 | PASS |
| LogBeSlotEviction (L1560) | 2 | <= 4 | PASS |
| IsPttDragOrderCancellable (L1628) | **6** | <= 4 | **FAIL** |

**Violations**: 4 methods exceed their CCN ceiling.

Raw lizard warnings output:
```
!!!! Warnings (cyclomatic_complexity > 8 or length > 1000 or ...) !!!!
  NLOC    CCN   token  PARAM  length  location
      27     10    169      1      27 TrimSignal::TryFireFollowerBeRetry@1501-1527
      21     11    149      1      21 TrimSignal::TryEvictFollowerBeSlot@1578-1598
```

**Discrepancy vs engineer self-report**:
The engineer reported "CCN After <= 8" for all three parent methods. Independent lizard measurement
shows TryFireFollowerBeRetry=10 and TryEvictFollowerBeSlot=11. Engineer claim is INCORRECT.

**Discrepancy on helpers**:
The engineer reported IsBePendingTargetOrder CCN=4, IsPttDragOrderCancellable CCN=3.
Lizard independently measures both at CCN=6. These exceed the CCN<=4 helper ceiling.

---

## 7 Mandatory Scans

### SCAN-01 — lock() [PASS]
Command: `Select-String -Path src/PropTraderTools/*.cs -Pattern "lock\("`
Result: 7 hits — ALL are comment-only references (e.g. "// JS-021: no lock()...").
Zero executable lock() calls found.
Verdict: **PASS**

### SCAN-02 — async void [PASS]
Command: `Select-String -Path src/PropTraderTools/*.cs -Pattern "async void "`
Result: 3 hits — ALL are comment-only references (e.g. "// JS-033: not async void...").
Zero executable async void declarations found.
Verdict: **PASS**

### SCAN-03 — return null (new instances) [PASS]
Command: `Select-String -Path src/PropTraderTools/*.cs -Pattern "return null"`
Result: Multiple hits found. TA-R4 modified region is L1485–L1660.
No `return null` exists in any of the 4 new helpers or 3 target methods.
All return null hits are pre-existing (lowest line in modified region is L1744 in CopyEngine.cs).
Verdict: **PASS** (0 new instances)

### SCAN-04 — throw new (new instances) [PASS]
Command: `Select-String -Path src/PropTraderTools/*.cs -Pattern "throw new "`
Result: 1 hit — TradeCopierWindow.cs line 871 (NotImplementedException in AccountDisplayConverter).
Not in TA-R4 scope. Pre-existing.
Verdict: **PASS** (0 new instances)

### SCAN-05a — lizard CCN [FAIL]
Command: `lizard src/PropTraderTools/CopyEngine.cs --CCN 8`
Result: See blocker table above.
- TryFireFollowerBeRetry: CCN=10 — IN warnings list (required: <= 8)
- TryEvictFollowerBeSlot: CCN=11 — IN warnings list (required: <= 8)
- IsBePendingTargetOrder: CCN=6 — exceeds helper ceiling of 4
- IsPttDragOrderCancellable: CCN=6 — exceeds helper ceiling of 4
- CancelPttDragOrphansForAccount: CCN=5 — PASS (but this one was already <=8)
- IsPttBeStopRejected: CCN=2 — PASS
- LogBeSlotEviction: CCN=2 — PASS
Verdict: **FAIL — 4 violations**

### SCAN-05b — cs delta (Code Health) [PASS]
Command: `cs delta` (with CS_ACCESS_TOKEN)
Result: CopyEngine.cs Code Health 1.61 → 1.81 (improvement).
Exit code 1 is pre-existing known issue (docs/Real Estate/ non-ASCII path) — not a regression.
Verdict: **PASS**

### SCAN-06 — dotnet build [PASS]
Command: `dotnet build src/PropTraderTools/PropTraderTools.csproj`
Result:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```
Verdict: **PASS**

### SCAN-07 — dotnet test [PASS]
Command: `dotnet test src/PropTraderTools/PropTraderTools.csproj --no-build`
Result: Failed: 22, Passed: 453, Skipped: 15, Total: 490
22 pre-existing IL-reflection failures — accepted, baseline confirmed.
453 passed (up from 436 baseline; increase due to 17 new tests across recent tickets — no new failures).
Verdict: **PASS** (0 new failures)

---

## Behaviour Verification

All 3 target method bodies and 4 helpers were read independently.

### TryFireFollowerBeRetry (L1501–L1527)
- Guard chain intact: null-guard → IsBePendingTargetOrder → Working/Accepted state → TryRemove → IsFlat
- `IsBePendingTargetOrder` helper extracted correctly: PTT-QX-T prefix + Length>8 + IsDigit[8], OR Target + Length>6 + IsDigit[6]
- No logic changes. No new early returns added vs original.
- Behaviour: **IDENTICAL**

### TryEvictFollowerBeSlot (L1578–L1598)
- Guard chain intact: null-guard → isFilled/isRejected → Clear → follower-guard → flat-guard(Filled-only) → TryRemove → TryRemove → TryRemove → LogBeSlotEviction(if slot)
- `IsPttBeStopRejected` extracted correctly: Rejected + "PTT-BE-Stop"
- `LogBeSlotEviction` extracted correctly: ternary log with reason string, no side effects
- DW-B81-01 flat-guard bypass for Rejected path preserved
- Behaviour: **IDENTICAL**

### CancelPttDragOrphansForAccount (L1639–L1655)
- foreach with IsPttDragOrderCancellable guard (continue on false), try/catch Cancel pattern
- `IsPttDragOrderCancellable` extracted correctly: Working + Instrument match + PTT-TGT-Drag or PTT-STP-Drag
- try/catch absorbs ErrorCode.UnableToCancelOrder as intended
- Behaviour: **IDENTICAL**

All helpers are `private`. No behaviour changes. No logic reordering.

---

## Engineer Self-Report vs Verifier Layer 3 — Discrepancy Table

| Method | Engineer Claimed CCN | Lizard Measured CCN | Discrepancy |
|--------|---------------------|---------------------|-------------|
| TryFireFollowerBeRetry | 7 (comment) | **10** | +3 |
| TryEvictFollowerBeSlot | 8 (comment) | **11** | +3 |
| IsBePendingTargetOrder | 4 | **6** | +2 |
| IsPttDragOrderCancellable | 3 | **6** | +3 |

Note: The engineer's cs delta showed 14→9 for TryFireFollowerBeRetry and 11→10 for TryEvictFollowerBeSlot.
CodeScene CCN and lizard CCN use different counting models. The binding standard per the architect plan
is lizard (the same tool used at baseline). Lizard is authoritative per BWAVE-CYC wave protocol.

---

## Required Remediation

Engineer must reduce cyclomatic complexity in 4 locations:

1. **TryFireFollowerBeRetry (CCN=10, needs CCN<=8)**
   Extract 2 more decision points. Possible target: the Working/Accepted state pair
   (lines 1508–1511) into `private bool IsBeRetryEligibleOrderState(Order o)` — CCN=2 helper.

2. **TryEvictFollowerBeSlot (CCN=11, needs CCN<=8)**
   Extract 3 more decision points. Possible target: the isFilled + isRejected compound check
   (lines 1583–1585) into `private bool IsBeFillOrRejectTerminal(bool isFilled, bool isRejected)` — CCN=2.
   Also consider extracting the flat-guard (line 1590) into a named helper.

3. **IsBePendingTargetOrder (CCN=6, needs CCN<=4)**
   The isPttQxT block has 2 decisions and the return statement has 2 more. Extract one branch:
   `private bool IsNativeAtmTargetOrder(Order o)` for the Target+Length+IsDigit check — CCN=2.

4. **IsPttDragOrderCancellable (CCN=6, needs CCN<=4)**
   The expression-body has 3 `&&` and 1 `||`. Extract: `private bool IsPttDragOrderName(Order o)`
   for the name check (`PTT-TGT-Drag || PTT-STP-Drag`) — CCN=2. Then parent drops to CCN=3.

---

**VERIFY_FAIL -- TA-R4**

Blockers:
1. TryFireFollowerBeRetry: lizard CCN=10 > ceiling 8 (CopyEngine.cs L1501)
2. TryEvictFollowerBeSlot: lizard CCN=11 > ceiling 8 (CopyEngine.cs L1578)
3. IsBePendingTargetOrder: lizard CCN=6 > helper ceiling 4 (CopyEngine.cs L1485)
4. IsPttDragOrderCancellable: lizard CCN=6 > helper ceiling 4 (CopyEngine.cs L1628)