# BWAVE-REFACTOR LaneB — Architecture Plan (Phase 1)

**Epic**: BWAVE-REFACTOR LaneB  
**File**: `src/PropTraderTools/CopyEngine.cs`  
**Date**: 2026-09-06  
**Status**: PLAN_COMPLETE  
**Author**: ptt-architect (Phase 1)

---

## Section 0: Lane-Split Gate Result (MANDATORY FIRST)

```
LANE-SPLIT GATE RESULT: SINGLE-PIPELINE
  Q1 (proximity):        NO  — violations span different methods across ~5000 lines
  Q2 (design dep):       NO  — each method extraction is independently specifiable
  Q3 (standalone value): YES — each ticket delivers independent CCN reduction
  Q4 (SIM independence): YES — each method has an independent SIM verification path

Decision: SINGLE-PIPELINE chosen.
Gate formula yields LANES-APPROVED (Q1=NO, Q2=NO, Q3=YES, Q4=YES), BUT:
All work modifies a SINGLE file (CopyEngine.cs). Parallel lane workers on the same
file would cause merge conflicts. Protocol default is single pipeline. The 5 tickets
execute SEQUENTIALLY on one file. No parallel execution.
```

---

## Section 1: Scope and Approach

### 1a. Target File
`src/PropTraderTools/CopyEngine.cs` — single-file extraction only.  
All extracted helpers are `private` or `private static` within `CopyEngine`.  
No new source files. No API surface changes.

### 1b. Measurement Tool
**Lizard** (lizard CCN) is the authoritative measurement tool.  
Lizard counts `||` and `&&` as decision points (+1 each).  
Project McCabe comments in the file use a different convention (do not count `||`/`&&`).  
The discrepancy between code comments ("CYC=7") and Lizard ("CCN=20") is explained entirely by compound boolean operators.

**Primary extraction technique**: Extract compound `||`/`&&` boolean expressions into named private static predicate methods. Example:

```csharp
// Before — 5 || operators = 4 extra Lizard branches in parent method:
bool stateOk = o.OrderState == OrderState.Working
    || o.OrderState == OrderState.Initialized
    || o.OrderState == OrderState.Accepted
    || o.OrderState == OrderState.Submitted
    || o.OrderState == OrderState.TriggerPending;

// After — parent gains 0 branches, predicate has CCN=5:
bool stateOk = IsQxCancellableState(o);
```

### 1c. Out-of-Scope (DISMISSED)
- `src/PropTraderTools/Features/*.cs` — Lane C
- `src/PropTraderTools/TradeCopierPanel.cs` — Lane A
- `src/PropTraderTools/TradeCopierWindow.cs` — Lane A
- `TickCount64` cast `(long)(int)` — .NET 4.8 correct, dismissed per spec
- `ActiveOrders .ToList()` — stays per DW-NEXT-A-07
- `_drainOwnedOrderIds` — ConcurrentDictionary<string,byte>, unchanged

---

## Section 2: Component List and Extraction Map

### Ticket T1: QX Order State Predicate Extraction
**Methods touched**: `CancelQxBrackets` (2-param, CCN=16), `CancelQxBrackets` (3-param, CCN=16), `CancelAllAccountOrders` (CCN=12), `BuildQxSnapshot` (CCN=11)

**Root cause**: The 5-state `stateOk` compound (`Working || Initialized || Accepted || Submitted || TriggerPending`) appears in 3 of 4 methods. Each `||` = +1 Lizard branch. 4 operators x 3 methods = 12 extra Lizard branches.

**Extracted helpers:**

| Helper Name | Visibility | Signature | Concern |
|---|---|---|---|
| `IsQxCancellableState` | `private static` | `bool IsQxCancellableState(Order o)` | 5-state compound for QX/BE cancel eligibility |
| `IsInstrumentMatch` | `private static` | `bool IsInstrumentMatch(Order o, Instrument instr)` | Null-safe instrument FullName equality |
| `IsQxCancellableStateTestable` | `internal static` | `bool IsQxCancellableStateTestable(OrderState s)` | Test seam |

**Post-extraction target CCN:**
- `CancelQxBrackets` (2-param): 16 → ≤8
- `CancelQxBrackets` (3-param): 16 → ≤8
- `CancelAllAccountOrders`: 12 → ≤8
- `BuildQxSnapshot`: 11 → ≤8

**`IsQxCancellableState` body** (CCN=5):
```csharp
private static bool IsQxCancellableState(Order o) =>
    o.OrderState == OrderState.Working
    || o.OrderState == OrderState.Initialized
    || o.OrderState == OrderState.Accepted
    || o.OrderState == OrderState.Submitted
    || o.OrderState == OrderState.TriggerPending;
```
Note: `CancelAllAccountOrders` uses a 4-state variant (no TriggerPending). It uses `IsQxCancellableState` minus TriggerPending OR a separate predicate `IsCancelAllEligibleState`. Use separate `IsCancelAllEligibleState` to avoid behavior change.

**`IsInstrumentMatch` body** (CCN=2):
```csharp
private static bool IsInstrumentMatch(Order o, Instrument instr) =>
    o.Instrument != null && o.Instrument.FullName == instr.FullName;
```

---

### Ticket T2: ATM Bracket Snapshot and Drag Sync Extraction
**Methods touched**: `ResubmitOneCollateralLeg` (CCN=25), `SnapshotBeTargets` (CCN=24), `SyncFollowerBracket` (CCN=20), `FindFollowerBracketOrder` (IEnumerable overload, CCN=11), `MatchesLeaderName` (CCN=11)

**Root cause**:
- `ResubmitOneCollateralLeg`: 2 foreach+if blocks with compound `&&` conditions + 2 CreateOrder/submit blocks with null checks
- `SnapshotBeTargets`: 7-state `stateOk` compound (`||` x6) + `instrOk` compound + name predicates
- `SyncFollowerBracket`: each `&&` in the `isStop && IsAtmSTPOrder(fo)` branches adds +1 Lizard
- `FindFollowerBracketOrder`: 4-state filter compound (`&&` between state options) + type-match `||`

**Extracted helpers:**

| Helper Name | Visibility | Signature | Concern |
|---|---|---|---|
| `IsBeTargetStateOk` | `private static` | `bool IsBeTargetStateOk(Order o)` | 7-state compound for SnapshotBeTargets |
| `IsNativeBeTarget` | `private static` | `bool IsNativeBeTarget(Order o, Instrument instr)` | Native Target1..9 check with instrument match |
| `IsPttBeTarget` | `private static` | `bool IsPttBeTarget(Order o, Instrument instr)` | PTT-QX-T* or PTT-BE-Target-* check |
| `IsFoAtmStopBranch` | `private static` | `bool IsFoAtmStopBranch(Order fo, bool isStop)` | Fuses `isStop && IsAtmSTPOrder(fo)` for SyncFollowerBracket |
| `IsFoAtmTargetBranch` | `private static` | `bool IsFoAtmTargetBranch(Order fo, bool isStop)` | Fuses `!isStop && IsAtmSTPOrder(fo)` for SyncFollowerBracket |
| `CancelMatchingStpDrag` | `private` | `void CancelMatchingStpDrag(Account acc, Order fo, string stpDragName)` | Block A-Prime-Stop sweep for ResubmitOneCollateralLeg |
| `CancelMatchingTgtDrag` | `private` | `void CancelMatchingTgtDrag(Account acc, Order fo, string tgtDragName)` | Block A-Prime-Target sweep for ResubmitOneCollateralLeg |
| `SubmitCollateralStop` | `private` | `void SubmitCollateralStop(Account acc, Order fo, double newPrice, string suffix, Order leaderLeg)` | Block B stop submit for ResubmitOneCollateralLeg |
| `SubmitCollateralTarget` | `private` | `void SubmitCollateralTarget(Account acc, Order fo, double targetPrice, string suffix, Order leaderLeg)` | Block C target submit for ResubmitOneCollateralLeg |
| `IsFoBracketState` | `private static` | `bool IsFoBracketState(Order fo)` | 4-state filter for FindFollowerBracketOrder (Working/Accepted/Submitted/ChangeSubmitted) |
| `IsBeTargetStateOkTestable` | `internal static` | `bool IsBeTargetStateOkTestable(OrderState s)` | Test seam |

**Post-extraction target CCN:**
- `ResubmitOneCollateralLeg`: 25 → ≤8
- `SnapshotBeTargets`: 24 → ≤8
- `SyncFollowerBracket`: 20 → ≤8
- `FindFollowerBracketOrder` (IEnumerable): 11 → ≤8
- `MatchesLeaderName`: 11 → ≤8 (extract `BuildLegSuffix(string leaderName) : string` to absorb the ternary `legSuffix =` assignment)

---

### Ticket T3: ATM Cleanup and Copy Replacement Extraction
**Methods touched**: `TryCleanupReArmedAtmBracket` (CCN=23), `SyncAtmFollowerTarget` (CCN=21), `ReplaceFollowerCopyOnAtmCancel` (CCN=18)

**Root cause**:
- `TryCleanupReArmedAtmBracket`: Compound guard at (1) has 10+ `||` operators in a single `if` condition — Lizard counts each as +1 branch
- `SyncAtmFollowerTarget`: `fo.LimitPrice <= 0 || IsNoPriceChange(...)` compound; foreach A-Prime has compound `&&` conditions
- `ReplaceFollowerCopyOnAtmCancel`: Nested `foreach rules` + `for followers` with `||` in loop guards; `mode is FollowerAtmMode.Named namedAtm` is a branch

**Extracted helpers:**

| Helper Name | Visibility | Signature | Concern |
|---|---|---|---|
| `IsQxCleanupTriggerOrder` | `private static` | `bool IsQxCleanupTriggerOrder(Order o)` | Absorbs name+length+digit compound from TryCleanupReArmedAtmBracket guard (1) |
| `IsQxCleanupAccountEligible` | `private` | `bool IsQxCleanupAccountEligible(Order o, out (Instrument Instr, DateTime Expiry) entry)` | Absorbs follower+TryGetValue+expiry+instrument compound from guard (1) |
| `IsAtmTargetEligible` | `private static` | `bool IsAtmTargetEligible(Account acc, Order fo, double newPrice)` | Absorbs acc null + fo null + LimitPrice<=0 + IsNoPriceChange compounds for SyncAtmFollowerTarget |
| `FindFollowerRuleAndIndex` | `private` | `bool FindFollowerRuleAndIndex(Order cancelledOrder, out CopyRule? rule, out int idx)` | Absorbs the nested foreach+for lookup loop from ReplaceFollowerCopyOnAtmCancel |
| `IsReplaceFollowerEligible` | `private` | `bool IsReplaceFollowerEligible(Account leader, Order cancelledOrder)` | Fuses HasOpenPosition leader(5) + follower NoPosition(5b) + HasWorkingPttCopy(6) guards |
| `IsQxCleanupTriggerOrderTestable` | `internal static` | `bool IsQxCleanupTriggerOrderTestable(OrderState s, string name)` | Test seam |

**`IsQxCleanupTriggerOrder` body** (CCN=4 — absorbs from guard):
```csharp
private static bool IsQxCleanupTriggerOrder(Order o) =>
    (o.OrderState == OrderState.Working || o.OrderState == OrderState.Accepted)
    && o.Name != null
    && o.Name.StartsWith("PTT-QX-T", StringComparison.Ordinal)
    && o.Name.Length >= 9
    && char.IsDigit(o.Name[8]);
```

**Post-extraction target CCN:**
- `TryCleanupReArmedAtmBracket`: 23 → ≤8
- `SyncAtmFollowerTarget`: 21 → ≤8
- `ReplaceFollowerCopyOnAtmCancel`: 18 → ≤8

---

### Ticket T4: BE and Flatten Extraction
**Methods touched**: `ArmPendingBe` (CCN=27), `MoveStopToBreakEven` (CCN=18), `FlattenOneAccount` (CCN=19), `TryFirePositionState` (CCN=13)

**Root cause**:
- `ArmPendingBe`: The `if (tickSize > 0.0)` block contains nested ternaries: `isLong ? refBid : refAsk`, `refPx > 0.0 && (isLong ? ...)` — each `?:` = +1 Lizard, each `&&` = +1 Lizard
- `FlattenOneAccount`: Inner foreach with 3-state `||` (`Submitted || Accepted || Working`) = 2 extra Lizard branches
- `MoveStopToBreakEven`: `!isRetry && !IsFlat(...)` chain, `!isRetry && IsFollowerAccount(acc)` chain, nested `&&` inside if body
- `TryFirePositionState`: `foreach _rules` with `e.Order.Account.Name == r.MasterAccount?.Name` compound check; Interlocked.Exchange pattern

**Extracted helpers:**

| Helper Name | Visibility | Signature | Concern |
|---|---|---|---|
| `TryFireBeImmediate` | `private` | `bool TryFireBeImmediate(Account masterAcc, Instrument instr, Position pos, int bufferTicks)` | Absorbs the `if (tickSize > 0.0)` block from ArmPendingBe — returns true if fired immediately |
| `IsFlattenOrderInFlight` | `private static` | `bool IsFlattenOrderInFlight(Order o, Instrument instr)` | Absorbs `name==PTT-Flatten && instr match && 3-state` compound from FlattenOneAccount foreach |
| `HasActiveFlattenInFlight` | `private` | `bool HasActiveFlattenInFlight(Account acc, Instrument instr)` | Wraps the foreach+guard loop from FlattenOneAccount into one call |
| `ShouldRegisterZeroTargetRetry` | `private static` | `bool ShouldRegisterZeroTargetRetry(bool isRetry, bool isFlat)` | Absorbs `!isRetry && !IsFlat(...)` from MoveStopToBreakEven targets==0 branch |
| `ShouldRegisterPartialRetry` | `private` | `bool ShouldRegisterPartialRetry(bool isRetry, Account acc, Instrument instr, int leaderCount, int targetCount)` | Absorbs DW-B79-07 partial-target check |
| `IsLeaderAccountName` | `private` | `bool IsLeaderAccountName(string accName)` | Absorbs the foreach _rules leader-name check in TryFirePositionState |
| `TryFireBeImmediateTestable` | `internal` | `bool TryFireBeImmediateTestable(Account a, Instrument i, Position p, int buf)` | Test seam |

**`TryFireBeImmediate` body** (CCN=6, extracted from ArmPendingBe):
```csharp
private bool TryFireBeImmediate(Account masterAcc, Instrument instr, Position pos, int bufferTicks)
{
    double tickSize = instr.MasterInstrument?.TickSize ?? 0.0;
    if (tickSize <= 0.0)
        return false;
    bool isLong = pos.MarketPosition == NinjaTrader.Cbi.MarketPosition.Long;
    double target = pos.AveragePrice + (isLong ? 1.0 : -1.0) * bufferTicks * tickSize;
    double refBid = instr.MarketData?.Bid?.Price ?? 0.0;
    double refAsk = instr.MarketData?.Ask?.Price ?? 0.0;
    double refPx = isLong ? refBid : refAsk;
    bool alreadyAtBe = refPx > 0.0 && (isLong ? (refPx >= target) : (refPx <= target));
    if (!alreadyAtBe)
        return false;
    StatusUpdate?.Invoke("PTT-BE: price already at BE for " + masterAcc.Name + " -- firing immediately");
    BreakEven(masterAcc, instr, bufferTicks);
    PendingBeFired?.Invoke(instr.FullName ?? string.Empty, masterAcc.Name ?? string.Empty);
    return true;
}
```
Note: The ternaries inside are counted separately in the extracted method (CCN=6). ArmPendingBe caller becomes: `if (TryFireBeImmediate(...)) return;` — one branch instead of the entire block.

**Post-extraction target CCN:**
- `ArmPendingBe`: 27 → ≤8
- `FlattenOneAccount`: 19 → ≤8
- `MoveStopToBreakEven`: 18 → ≤8
- `TryFirePositionState`: 13 → ≤8

---

### Ticket T5: Adjacent Band Methods
**Methods touched**: `OnOrderUpdate` (CCN=12), `CountLeaderTargets` (CCN=13), `DrainThenDispatch` (CCN=11)

**Root cause**:
- `OnOrderUpdate`: The drain routing block at lines 1425-1435 uses `|| e.Order.OrderState == OrderState.Rejected` compound and the `else if` chain
- `CountLeaderTargets`: The `bool isTarget =` compound expression uses multiple `&&` and method calls — the assignment itself counts as branches in Lizard
- `DrainThenDispatch`: The LINQ `Where` predicate has multiple `&&` and `||` operators counted by Lizard

**Extracted helpers:**

| Helper Name | Visibility | Signature | Concern |
|---|---|---|---|
| `IsDrainCancelOrReject` | `private static` | `bool IsDrainCancelOrReject(OrderState s)` | Absorbs `Cancelled || Rejected` compound from OnOrderUpdate drain block |
| `IsCountableLeaderTarget` | `private static` | `bool IsCountableLeaderTarget(Order o, Instrument instr)` | Absorbs the stateOk + instrOk + type + isTarget compound from CountLeaderTargets |
| `IsEntryDrainCandidate` | `private static` | `bool IsEntryDrainCandidate(Order o, Instrument instr)` | Absorbs the LINQ Where predicate compound from DrainThenDispatch |
| `IsCountableLeaderTargetTestable` | `internal static` | `bool IsCountableLeaderTargetTestable(OrderState s, string instrFN, OrderType t, string name, string instrFN2)` | Test seam |

**Post-extraction target CCN:**
- `OnOrderUpdate`: 12 → ≤8
- `CountLeaderTargets`: 13 → ≤8
- `DrainThenDispatch`: 11 → ≤8

---

## Section 3: Ticket Grouping Summary

| Ticket | Methods (god + adjacent) | New Helpers | Est. LOC change |
|---|---|---|---|
| T1 | CancelQxBrackets (x2), CancelAllAccountOrders, BuildQxSnapshot | IsQxCancellableState, IsCancelAllEligibleState, IsInstrumentMatch (+seam) | +35 lines |
| T2 | ResubmitOneCollateralLeg, SnapshotBeTargets, SyncFollowerBracket, FindFollowerBracketOrder, MatchesLeaderName | IsBeTargetStateOk, IsNativeBeTarget, IsPttBeTarget, IsFoAtmStopBranch, IsFoAtmTargetBranch, CancelMatchingStpDrag, CancelMatchingTgtDrag, SubmitCollateralStop, SubmitCollateralTarget, IsFoBracketState (+seam) | +120 lines |
| T3 | TryCleanupReArmedAtmBracket, SyncAtmFollowerTarget, ReplaceFollowerCopyOnAtmCancel | IsQxCleanupTriggerOrder, IsQxCleanupAccountEligible, IsAtmTargetEligible, FindFollowerRuleAndIndex, IsReplaceFollowerEligible (+seam) | +80 lines |
| T4 | ArmPendingBe, MoveStopToBreakEven, FlattenOneAccount, TryFirePositionState | TryFireBeImmediate, IsFlattenOrderInFlight, HasActiveFlattenInFlight, ShouldRegisterZeroTargetRetry, ShouldRegisterPartialRetry, IsLeaderAccountName (+seam) | +100 lines |
| T5 | OnOrderUpdate, CountLeaderTargets, DrainThenDispatch | IsDrainCancelOrReject, IsCountableLeaderTarget, IsEntryDrainCandidate (+seam) | +40 lines |

---

## Section 4: Method Signatures (All Extracted Helpers)

### 4a. T1 Helpers
```csharp
// CYC=5. Absorbs 5-state QX cancel eligibility compound.
private static bool IsQxCancellableState(Order o);

// CYC=4. Absorbs 4-state cancel eligibility for CancelAllAccountOrders.
private static bool IsCancelAllEligibleState(Order o);

// CYC=2. Absorbs o.Instrument != null && FullName == check.
private static bool IsInstrumentMatch(Order o, Instrument instr);

// Test seam.
internal static bool IsQxCancellableStateTestable(OrderState s);
internal static bool IsCancelAllEligibleStateTestable(OrderState s);
```

### 4b. T2 Helpers
```csharp
// CYC=7. Absorbs 7-state SnapshotBeTargets stateOk compound.
private static bool IsBeTargetStateOk(Order o);

// CYC=4. Absorbs native Target1-9 check + instrument + state + name pattern.
private static bool IsNativeBeTarget(Order o, Instrument instr);

// CYC=4. Absorbs PTT-QX-T* or PTT-BE-Target-* check + instrument.
private static bool IsPttBeTarget(Order o, Instrument instr);

// CYC=2. Fuses isStop && IsAtmSTPOrder for SyncFollowerBracket branch (3).
private static bool IsFoAtmStopBranch(Order fo, bool isStop);

// CYC=2. Fuses !isStop && IsAtmSTPOrder for SyncFollowerBracket branch (3b).
private static bool IsFoAtmTargetBranch(Order fo, bool isStop);

// CYC=3. Cancels matching PTT-STP-Drag-N orders for one collateral leg.
private void CancelMatchingStpDrag(Account acc, Order fo, string stpDragName);

// CYC=3. Cancels matching PTT-TGT-Drag-N orders for one collateral leg.
private void CancelMatchingTgtDrag(Account acc, Order fo, string tgtDragName);

// CYC=4. Submit Block B stop for ResubmitOneCollateralLeg. Uses acc.CreateOrder+Submit.
private void SubmitCollateralStop(Account acc, Order fo, double newPrice, string suffix, Order leaderLeg);

// CYC=4. Submit Block C target for ResubmitOneCollateralLeg. Uses acc.CreateOrder+Submit.
private void SubmitCollateralTarget(Account acc, Order fo, double targetPrice, string suffix, Order leaderLeg);

// CYC=4. Absorbs 4-state filter compound for FindFollowerBracketOrder (IEnumerable).
private static bool IsFoBracketState(Order fo);

// CYC=4. Absorbs legSuffix ternary assignment from MatchesLeaderName.
private static string BuildLegSuffix(string leaderName);

// Test seams.
internal static bool IsBeTargetStateOkTestable(OrderState s);
internal static bool IsFoBracketStateTestable(OrderState s);
```

### 4c. T3 Helpers
```csharp
// CYC=4. Absorbs name/length/digit compound from TryCleanupReArmedAtmBracket guard (1a).
private static bool IsQxCleanupTriggerOrder(Order o);

// CYC=5. Absorbs follower+TryGetValue+expiry+instrument compound from guard (1d-1f).
// out param: the matching cleanup entry (valid only when returns true).
private bool IsQxCleanupAccountEligible(Order o, out (Instrument Instr, DateTime Expiry) entry);

// CYC=3. Absorbs acc null + fo null + LimitPrice<=0 + IsNoPriceChange compounds.
private static bool IsAtmTargetEligible(Account acc, Order fo, double newPrice);

// CYC=5. Absorbs nested foreach+for lookup from ReplaceFollowerCopyOnAtmCancel.
// Returns true+sets out params when a matching follower rule+index is found.
private bool FindFollowerRuleAndIndex(Order cancelledOrder, out CopyRule? rule, out int idx);

// CYC=4. Fuses leader-has-position(5) + follower-no-position(5b) + no-in-flight-copy(6).
private bool IsReplaceFollowerEligible(Account leader, Order cancelledOrder);

// Test seams.
internal static bool IsQxCleanupTriggerOrderTestable(OrderState s, string name);
internal static bool IsAtmTargetEligibleTestable(bool accIsNull, bool foIsNull, double foLimitPrice, double newPrice);
```

### 4d. T4 Helpers
```csharp
// CYC=6. Absorbs the immediate-fire block from ArmPendingBe.
// Returns true if BE was fired immediately (caller returns after this call).
private bool TryFireBeImmediate(Account masterAcc, Instrument instr, Position pos, int bufferTicks);

// CYC=3. Absorbs name==PTT-Flatten && instr && 3-state compound from FlattenOneAccount.
private static bool IsFlattenOrderInFlight(Order o, Instrument instr);

// CYC=2. Wraps the foreach+guard loop in FlattenOneAccount.
private bool HasActiveFlattenInFlight(Account acc, Instrument instr);

// CYC=2. Absorbs !isRetry && !IsFlat from MoveStopToBreakEven targets==0 branch.
private static bool ShouldRegisterZeroTargetRetry(bool isRetry, bool isFlat);

// CYC=4. Absorbs DW-B79-07 partial-target retry condition.
private bool ShouldRegisterPartialRetry(bool isRetry, Account acc, Instrument instr, int leaderCount, int targetCount);

// CYC=3. Absorbs foreach _rules leader-check loop from TryFirePositionState.
private bool IsLeaderAccountName(string accName);

// Test seams.
internal bool TryFireBeImmediateTestable(Account a, Instrument i, Position p, int buf);
internal static bool IsFlattenOrderInFlightTestable(OrderState s, string orderName, string instrFN, string checkFN);
```

### 4e. T5 Helpers
```csharp
// CYC=2. Absorbs Cancelled || Rejected compound from OnOrderUpdate drain routing.
private static bool IsDrainCancelOrReject(OrderState s);

// CYC=6. Absorbs stateOk + instrOk + type + isTarget compound from CountLeaderTargets.
private static bool IsCountableLeaderTarget(Order o, Instrument instr);

// CYC=5. Absorbs the LINQ Where predicate compound from DrainThenDispatch.
private static bool IsEntryDrainCandidate(Order o, Instrument instr);

// Test seams.
internal static bool IsDrainCancelOrRejectTestable(OrderState s);
internal static bool IsCountableLeaderTargetTestable(OrderState s, string instrFN, OrderType t, string name, string checkFN);
```

---

## Section 5: NinjaTrader 8 API Usage

All extracted helpers use existing NT8 API patterns only:

| API Call | Usage | NT8 Context |
|---|---|---|
| `acc.Cancel(Order[])` | CancelMatchingStpDrag, CancelMatchingTgtDrag | AddOnBase available |
| `acc.CreateOrder(...)` | SubmitCollateralStop, SubmitCollateralTarget | AddOnBase available; 12-arg form; arg12=(CustomOrder)null |
| `acc.Submit(Order[])` | SubmitCollateralStop, SubmitCollateralTarget | AddOnBase available; after CreateOrder |
| `acc.Orders.ToList()` | CancelMatchingStpDrag, CancelMatchingTgtDrag | Thread-safe snapshot |
| `DateTime.UtcNow` | IsQxCleanupAccountEligible expiry check | NOT DateTime.Now |
| `DateTime.MaxValue` | All CreateOrder calls | NT8-013 pattern |

**Banned NT8 API (confirmed NOT used):**
- `AtmStrategyCreate()` — StrategyBase only, NOT AddOnBase. Not used.
- `AtmStrategyChangeStopTarget()` — StrategyBase only, NOT AddOnBase. Not used.
- `Account.Change()` on ATM brackets — not introduced in new helpers.

---

## Section 6: Threading Model

| Path | Thread | Helpers Called | Safety |
|---|---|---|---|
| `OnOrderUpdate` hot path | NT8 account bg thread | T1+T5 helpers (pure predicates) | All lock-free ConcurrentDictionary ops |
| `TryCleanupReArmedAtmBracket` | NT8 account bg thread | T3 helpers | ConcurrentDictionary.TryGetValue only |
| `FlattenOneAccount` | WPF UI thread (via Dispatcher.InvokeAsync) | T4 helpers | WPF-thread-only acc.Orders scan |
| `ArmPendingBe` | WPF UI thread (button click) | T4 TryFireBeImmediate | instr.MarketData reads OK on UI thread |
| `CancelQxBrackets` | NT8 dispatch thread | T1 helpers | lock-free predicates + acc.Cancel |

**Rule**: All extracted helpers are pure predicates or delegates to existing methods. Zero new thread-unsafe constructs.

---

## Section 7: 7-Scan Checklist (Per Ticket)

Each ticket MUST pass all 7 scans before merge:

| Scan | Command | Pass Condition |
|---|---|---|
| SCAN-01 | `dotnet build src/PropTraderTools/ --no-incremental` | 0 errors, 0 warnings |
| SCAN-02 | `grep -r "lock(" src/PropTraderTools/ --include="*.cs"` | 0 results |
| SCAN-03 | `grep -rn "async void " src/PropTraderTools/ --include="*.cs"` | 0 new results (existing event handlers exempt) |
| SCAN-04 | `lizard src/PropTraderTools/CopyEngine.cs -C 8 --languages csharp` | 0 methods > 8 in CopyEngine.cs |
| SCAN-05 | `grep -rn "return null;" src/PropTraderTools/CopyEngine.cs` | 0 new occurrences in modified methods |
| SCAN-06 | Codepage check on modified .cs files | 0 non-ASCII characters in identifiers or string literals |
| SCAN-07 | `dotnet test tests/PropTraderTools.Tests/ --no-build` | All pass |

---

## Section 8: Test Strategy

**Test file**: `tests/PropTraderTools.Tests/BwaveRefactorLaneBTests.cs` (NEW)

Each ticket writes structural `[Fact]` tests confirming:
1. The extracted private helper exists and is callable via `internal` test seam
2. The method returns the expected value for a simple canary input

**Test format** (one per extracted helper that has a seam):

```csharp
[Fact]
public void IsQxCancellableState_Working_ReturnsTrue()
{
    var result = CopyEngine.IsQxCancellableStateTestable(OrderState.Working);
    Assert.True(result);
}

[Fact]
public void IsQxCancellableState_Filled_ReturnsFalse()
{
    var result = CopyEngine.IsQxCancellableStateTestable(OrderState.Filled);
    Assert.False(result);
}
```

No behavioral tests required. Structural existence + canary value only.

**Test file header:**
```csharp
// BwaveRefactorLaneBTests.cs
// Structural [Fact] tests for BWAVE-REFACTOR LaneB extracted helpers.
// xUnit only. No NUnit. No MSTest. JS-051.
using Xunit;
using PropTraderTools;
namespace PropTraderTools.Tests;
```

---

## Section 9: NT8 Sync + F5 Gate

After every ticket:
1. Run `powershell -File scripts\ptt-sync-and-verify.ps1`
2. Verify 0 MISMATCH lines in output
3. Press **F5** in NinjaTrader 8 to recompile
4. Verify green (no compile errors in NT8 Output window)
5. DO NOT merge ticket PR until F5 is green

---

## Section 10: Risk Notes / DW Items

| Risk | Classification | Resolution |
|---|---|---|
| `SubmitCollateralStop`/`SubmitCollateralTarget` extract changes call order inside `ResubmitOneCollateralLeg` | LOW | Helpers are identical to inline code — no behavior change. SCAN-04 confirms CCN. |
| `FindFollowerRuleAndIndex` extracts the nested loop from `ReplaceFollowerCopyOnAtmCancel` | LOW | `out` params preserve return values. No new failure modes. |
| `TryFireBeImmediate` fires `BreakEven()` directly — same as inline | LOW | Same call, same parameters. No behavior change. |
| `IsQxCleanupAccountEligible` uses `out` tuple param — test seam complexity | LOW | Test seam uses testable overload with primitive args, avoids NT8 tuple. |
| Lizard CCN may still exceed 8 on edge cases with nested lambda `&&`/`||` | MEDIUM | After extraction, re-run `lizard` before declaring scan pass. |

**DW Deferred:**
- DW-NEXT-A-07: `ActiveOrders .ToList()` stays unchanged — not touched in this plan.
- All previously deferred items remain deferred.

---

## Section 11: Data Flow Summary

```
OrderEvent
    ↓
OnOrderUpdate [T5: CCN 12→≤8 via IsDrainCancelOrReject]
    ↓
... pre-gate helpers (unchanged) ...
    ↓
TryCancelFollowerEntries → CancelQxBrackets [T1: CCN 16→≤8 via IsQxCancellableState]
    ↓
TryDispatchLeaderFlat → FlattenOneAccount [T4: CCN 19→≤8 via HasActiveFlattenInFlight]
    ↓
TryHandleDrag → HandleBracketChange → SyncFollowerBracket [T2: CCN 20→≤8]
                                      ↓
                              SyncAtmFollowerBracket (unchanged - already ≤8)
                              SyncAtmFollowerTarget [T3: CCN 21→≤8]
                              ResubmitOneCollateralLeg [T2: CCN 25→≤8]
                              ↓
                              SnapshotBeTargets [T2: CCN 24→≤8]
    ↓
TryCleanupReArmedAtmBracket [T3: CCN 23→≤8 via IsQxCleanupTriggerOrder]
TryReplaceOnAtmCancel → ReplaceFollowerCopyOnAtmCancel [T3: CCN 18→≤8 via FindFollowerRuleAndIndex]

Button press (UI thread)
    ↓
ArmPendingBe [T4: CCN 27→≤8 via TryFireBeImmediate]
    ↓
MoveStopToBreakEven [T4: CCN 18→≤8 via ShouldRegisterZeroTargetRetry]
    ↓
SnapshotBeTargets [T2: already handled above]
CountLeaderTargets [T5: CCN 13→≤8 via IsCountableLeaderTarget]
```

---

**Return: PLAN_COMPLETE**
