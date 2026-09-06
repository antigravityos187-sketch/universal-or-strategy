# BWAVE-REFACTOR Lane B -- Architecture Plan
# Phase 2 Output
# Status: PLAN_COMPLETE (awaiting reviewer)
# Written: 2026-09-05
# Origin: DW-NEXT-B-04

---

## 1. Executive Summary

**Goal**: Reduce all methods in src/PropTraderTools/CopyEngine.cs with CCN > 8 to CCN <= 8
via extraction of private/private static helpers. Zero behavior change. Zero signature change on
public/internal methods.

**Scope**: Single file (CopyEngine.cs). 32 methods exceed CCN=8 per lizard baseline (2026-09-06).
Tests in new file src/PropTraderTools/Tests/BwaveRefactorLaneBTests.cs.

**Method**: Extract guard clauses and loop bodies to named private helpers immediately below the
parent method. Parent CCN drops; helpers stay <= 8. No new public/internal APIs. No lock(). No
async void. No return null in new code. ASCII-only identifiers. No DateTime.Now.

**Deferred**: All items from BWAVE-NEXT LaneBRepair backlog that are NOT CCN violations in
CopyEngine.cs. Features/*.cs is Lane C scope only.

---

## 2. LANE-SPLIT GATE RESULT

**LANE-SPLIT GATE RESULT: SINGLE-PIPELINE**

Gate evaluation:
- Q1. Same method or within 50 lines? **YES** -- all 32 methods are in a single 6500-line file
  (CopyEngine.cs). Helpers are co-located with their parents.
- Q2. Fix B design depends on Fix A final design? **YES** -- extracted helpers share the same
  CopyEngine instance fields. Helper names must be unique across tickets to avoid collisions.
  Ticket 2 helpers must not reuse names introduced by Ticket 1.

Q1=YES, Q2=YES => SINGLE-PIPELINE. Sequential tickets required.

---

## 3. CCN Baseline Table (lizard, 2026-09-06)

`
CCN  Method                               Location
---  ------                               --------
 27  ArmPendingBe                         L5729-5785
 25  ResubmitOneCollateralLeg             L3026-3133
 24  SnapshotBeTargets                    L5349-5392
 23  TryCleanupReArmedAtmBracket          L4138-4204
 21  SyncAtmFollowerTarget                L3216-3300
 20  SyncFollowerBracket                  L2539-2639
 19  FlattenOneAccount                    L4714-4783
 18  MoveStopToBreakEven                  L5404-5544
 18  ReplaceFollowerCopyOnAtmCancel       L3895-3948
 16  CancelQxBrackets (3-param)           L991-1040
 14  TryReplacePttBeBrackets              L4055-4126
 14  CancelQxBrackets (2-param)           L911-941
 13  TryFirePositionState                 L3796-3844
 13  CountLeaderTargets                   L5315-5342
 13  ResubmitTargetAfterCascade           L2907-2973
 12  OnOrderUpdate                        L1379-1486
 12  CancelAllAccountOrders               L1049-1079
 11  BuildQxSnapshot                      L952-980
 11  DrainThenDispatch                    L6516-6571
 11  FindFollowerBracketOrder (IEnumerable)  L3520-3553
 11  MatchesLeaderName                    L3575-3592
  9  HasNakedPosition                     L6473-6502
  9  RuleToDto                            L6197-6232
  9  IsFollowerAccount                    L778-797
  9  AllAccounts                          L5116-5163
  9  CaptureLinkedTargetPrice             L2778-2796
  9  MirrorClose                          L2119-2158
  9  BuildUpdatedMultipliers              L1348-1364
  9  CaptureOtherLegTargetPrices          L2812-2835
  9  HandleEntryChange                    L3736-3771
  9  HandleBracketChange                  L3414-3447
  9  CreateFollowerReplacementStop        L3348-3393
`

**Total**: 32 methods with CCN > 8. All must reach CCN <= 8 for VERIFY_PASS.

---

## 4. Ticket Plan

### Ticket 1 -- Tier A (CCN >= 20): 6 methods

**Methods**: ArmPendingBe(27), ResubmitOneCollateralLeg(25), SnapshotBeTargets(24),
TryCleanupReArmedAtmBracket(23), SyncAtmFollowerTarget(21), SyncFollowerBracket(20)

**File**: src/PropTraderTools/CopyEngine.cs
**Tests**: src/PropTraderTools/Tests/BwaveRefactorLaneBTests.cs

### Ticket 2 -- Tier B (CCN 16-19): 4 methods

**Methods**: FlattenOneAccount(19), MoveStopToBreakEven(18), ReplaceFollowerCopyOnAtmCancel(18),
CancelQxBrackets 3-param(16)

**File**: src/PropTraderTools/CopyEngine.cs
**Tests**: src/PropTraderTools/Tests/BwaveRefactorLaneBTests.cs

### Ticket 3 -- Tier C (CCN 13-15): 5 methods

**Methods**: TryReplacePttBeBrackets(14), CancelQxBrackets 2-param(14),
TryFirePositionState(13), CountLeaderTargets(13), ResubmitTargetAfterCascade(13)

**File**: src/PropTraderTools/CopyEngine.cs
**Tests**: src/PropTraderTools/Tests/BwaveRefactorLaneBTests.cs

### Ticket 4 -- Tier D (CCN 10-12): 6 methods

**Methods**: OnOrderUpdate(12), CancelAllAccountOrders(12), BuildQxSnapshot(11),
DrainThenDispatch(11), FindFollowerBracketOrder IEnumerable overload(11), MatchesLeaderName(11)

**File**: src/PropTraderTools/CopyEngine.cs
**Tests**: src/PropTraderTools/Tests/BwaveRefactorLaneBTests.cs

### Ticket 5 -- Tier E (CCN = 9): 11 methods

**Methods**: HasNakedPosition(9), RuleToDto(9), IsFollowerAccount(9), AllAccounts(9),
CaptureLinkedTargetPrice(9), MirrorClose(9), BuildUpdatedMultipliers(9),
CaptureOtherLegTargetPrices(9), HandleEntryChange(9), HandleBracketChange(9),
CreateFollowerReplacementStop(9)

**File**: src/PropTraderTools/CopyEngine.cs
**Tests**: src/PropTraderTools/Tests/BwaveRefactorLaneBTests.cs

---

## 5. Extraction Strategy Per Method Group

All helpers placed immediately below their parent method in the file.
No file moves. No new namespaces. All helpers are private or private static.

### 5.1 Ticket 1 Extractions

#### ArmPendingBe (CCN=27 -> target <=8)

Parent at L5729-5785. Lizard counts ?., ??, ternary ?:, and compound booleans as branches.

Extract:
- IsImmediateBeEligible(Position pos, Instrument instr, int bufferTicks) -> bool (private static)
  Absorbs: tickSize guard, isLong calc, target calc, refBid/refAsk reads, refPx, alreadyAtBe compound. CCN<=6.
- FireImmediateBe(Account masterAcc, Instrument instr, int bufferTicks) -> void (private)
  Absorbs: BreakEven() call + PendingBeFired?.Invoke(). CCN<=2.

Parent residual: null guard(1) + null guard(2) + IsFlat(3) + IsImmediateBeEligible(4)
+ tickSize guard(5) + slot upsert + subscribe = CCN<=7. PASS.

Test seam: internal bool IsImmediateBeEligibleTestable(Position p, Instrument i, int buf)
  -- accepts Position/Instrument stubs; delegates to IsImmediateBeEligible.

#### ResubmitOneCollateralLeg (CCN=25 -> target <=8)

Parent at L3026-3133. Two Block A-Prime foreach loops + two CreateOrder/Submit try blocks.

Extract:
- CancelLiveCollateralStop(Account acc, Order fo, string stpDragName) -> void (private)
  Absorbs: Block A-Prime-Stop foreach + if + try/catch. CCN<=4.
- CancelLiveCollateralTarget(Account acc, Order fo, string tgtDragName) -> void (private)
  Absorbs: Block A-Prime-Target foreach + if + try/catch. CCN<=4.
- CreateAndSubmitCollateralStop(Account acc, Order fo, double newPrice, string suffix, Order leaderLeg) -> void (private)
  Absorbs: CreateOrder(StopMarket) + null guard + Submit + StatusUpdate + catch. CCN<=4.
- CreateAndSubmitCollateralTarget(Account acc, Order fo, double targetPrice, string suffix, Order leaderLeg) -> void (private)
  Absorbs: CreateOrder(Limit) + null guard + Submit + StatusUpdate + catch. CCN<=4.

Parent residual: CancelLiveCollateralStop(0) + CancelLiveCollateralTarget(0)
+ leaderLeg null check(1) + CreateAndSubmitCollateralStop(0)
+ leaderLeg null check(1) + CreateAndSubmitCollateralTarget(0) = CCN<=4. PASS.

#### SnapshotBeTargets (CCN=24 -> target <=8)

Parent at L5349-5392. Multi-state stateOk compound OR counted as multiple branches by Lizard.

Extract:
- IsBeTargetStateOk(OrderState s) -> bool (private static)
  Absorbs: 7-state OR (Working, Accepted, Submitted, Initialized, TriggerPending,
  ChangeSubmitted, CancelSubmitted). CCN<=7.
- ClassifyBeTarget(Order o, string instrFullName, out bool isNative, out bool isPtt) -> void (private static)
  Absorbs: instrOk compound, type check, isNative compound, isPtt compound. CCN<=6.

Parent residual: null guard(1) + foreach(1) + o null continue(1) + IsBeTargetStateOk(1)
+ ClassifyBeTarget(0) + isNative branch(1) + isPtt branch(1) = CCN<=7. PASS.

Test seam: internal static bool IsBeTargetStateOkTestable(OrderState s) => IsBeTargetStateOk(s);

#### TryCleanupReArmedAtmBracket (CCN=23 -> target <=8)

Parent at L4138-4204. Massive 10-condition compound guard at entry.

Extract:
- IsCleanupAtmEligible(OrderEventArgs e, out (Instrument Instr, DateTime Expiry) entry) -> bool (private)
  Absorbs: all 10 compound conditions (state checks, name checks, account null guard,
  IsFollowerAccount, TryGetValue, expiry check, instrument match). CCN<=8.
- TryCancelNativeAtmTarget(Account acc, Instrument instr, char tChar) -> void (private)
  Absorbs: nativeName construction + foreach + inner if + acc.Cancel(). CCN<=4.
- EvaluateCleanupRemoval(Account acc, char tChar, DateTime expiry) -> void (private)
  Absorbs: shouldRemove ternary + TryRemove call. CCN<=2.

Parent residual: IsCleanupAtmEligible(1) + tChar extraction(0)
+ TryCancelNativeAtmTarget(0) + EvaluateCleanupRemoval(0) = CCN<=2. PASS.

NOTE: Caller pattern: if (!IsCleanupAtmEligible(e, out var entry)) return;
  char tChar = e.Order.Name[8];
  TryCancelNativeAtmTarget(e.Order.Account, entry.Instr, tChar);
  EvaluateCleanupRemoval(e.Order.Account, tChar, entry.Expiry);

#### SyncAtmFollowerTarget (CCN=21 -> target <=8)

Parent at L3216-3300. Null guards + Block A-Prime foreach + Block A cancel + Block B create.

Extract:
- IsAtmTargetSyncEligible(Account acc, Order fo, double newPrice) -> bool (private)
  NOTE: IsSyncAtmBracketEligible (L2663) covers the stop path.
  This new method covers the target path with fo.LimitPrice<=0 guard (B142-DIRECT-5).
  Absorbs: acc null(1), fo null(2), fo.LimitPrice<=0(3), IsNoPriceChange(4). CCN<=4.
- CancelBlockAAtmTarget(Account acc, Order fo, string tgtDragName) -> void (private)
  Absorbs: Block A-Prime foreach + if(Working && Name && Instrument) + try/catch. CCN<=5.
- BlockBCreateAtmTarget(Account acc, Order fo, double newPrice, string tgtDragName, Order leaderOrder) -> void (private)
  Absorbs: Block B CreateOrder + null guard + Submit + StatusUpdate + catch. CCN<=4.

Parent residual: IsAtmTargetSyncEligible(1) + DeriveLeaderBracketIndex(0) + tgtDragName(0)
+ Block A cancel try(1) + Block A cancel catch(0) + CancelBlockAAtmTarget(0)
+ BlockBCreateAtmTarget(0) + ExecutePhaseCStopReplacement(0) = CCN<=3. PASS.

#### SyncFollowerBracket (CCN=20 -> target <=8)

Parent at L2539-2639. ATM-STP path, ATM-TGT path, trailing-stop skip, acc.Change path.

Extract:
- HandleAtmStopSync(Account acc, Order fo, double newPrice, double tickSize, string legSuffix, Order leaderOrder) -> void (private)
  Absorbs: B142-DIRECT-2 stopPrice guard + CancelExistingPttStpDrag + SyncAtmFollowerBracket
  + capturedTargetPrice + ResubmitTargetAfterCascade + CaptureOtherLegTargetPrices
  + ResubmitCollateralLegs. CCN<=6.
- HandleAtmTargetSync(Account acc, Order fo, double newPrice, Order leaderOrder) -> void (private)
  Absorbs: SyncAtmFollowerTarget call. CCN<=1.
- HandleNonAtmSync(Account acc, Order fo, bool isStop, double newPrice) -> void (private)
  Absorbs: IsTrailingStop skip + try { price = newPrice; acc.Change } + catch. CCN<=4.

Parent residual: fo null(1) + priceDelta(2) + isStop && IsAtmSTPOrder(3)
+ !isStop && IsAtmSTPOrder(4) + else HandleNonAtmSync = CCN<=5. PASS.

### 5.2 Ticket 2 Extractions

#### FlattenOneAccount (CCN=19 -> target <=8)

Parent at L4714-4783.

Extract:
- IsAccountFlattenable(Account acc, Instrument instr) -> bool (private)
  Absorbs: acc null guard, instr null guard, position null/quantity guard. CCN<=4.
- SubmitMarketFlattenOrder(Account acc, Instrument instr, Position pos) -> void (private)
  Absorbs: OrderAction from MarketPosition + CreateOrder(Market) try + null guard + Submit + catch. CCN<=4.

Parent residual: IsAccountFlattenable(1) + CancelAllAccountOrders(0)
+ SubmitMarketFlattenOrder(0) = CCN<=2. PASS.

#### MoveStopToBreakEven (CCN=18 -> target <=8)

Parent at L5404-5544. Already delegates to SnapshotBeTargets and PttBreakEvenSwap.Execute.

Extract:
- LogDiagOrderCount(Account acc, Instrument instrument) -> void (private)
  Absorbs: diagTotal foreach + NinjaTrader.Code.Output.Process call. CCN<=2.
- RegisterBeRetrySlotIfNeeded(Account acc, Instrument instrument, int bufferTicks, bool isRetry, int targetsCount, int leaderCount) -> void (private)
  Absorbs: targets==0 slot block (slot + QueueBeRetryFallback 500ms)
  + partial-targets block (!isRetry && IsFollowerAccount && leaderCount>0 && targets<leaderCount
  + slot + QueueBeRetryFallback 200ms). CCN<=6.

Parent residual: IsFlat(1) + calc(0) + LogDiagOrderCount(0) + SnapshotBeTargets(0)
+ while-cap(1) + PttBreakEvenSwap.Execute(0) + targets==0 early return(1)
+ RegisterBeRetrySlotIfNeeded(0) = CCN<=5. PASS.

#### ReplaceFollowerCopyOnAtmCancel (CCN=18 -> target <=8)

Parent at L3895-3948.

Extract:
- FindFollowerRuleForOrder(Order cancelledOrder, out int followerIndex) -> CopyRule? (private)
  Absorbs: foreach rules + instrument match + for-i + name match + break. CCN<=5.
- IsReplaceDispatchEligible(CopyRule rule, int followerIndex, Order cancelledOrder) -> bool (private)
  Absorbs: !matchedRule.HasValue(1) + followerIndex<0(2) + leader null(3)
  + HasOpenPosition leader(4) + HasOpenPosition follower(5) + HasWorkingPttCopy(6). CCN<=6.

Parent residual: !_isCopyEnabled(1) + FindFollowerRuleForOrder(0)
+ IsReplaceDispatchEligible(1) + signal(0) + ResolveAtmMode(0) + Named branch(1) = CCN<=4. PASS.

#### CancelQxBrackets 3-param (CCN=16 -> target <=8)

Parent at L991-1040.

Extract:
- IsQxCancelEligible3(Order o, Instrument instr, System.Collections.Generic.HashSet<NinjaTrader.Cbi.Order> snapshot) -> bool (private static)
  Absorbs: stateOk 5-term OR + instrument null+FullName + snapshot null+Contains. CCN<=7.
- CommitStaleCancelBatch(Account acc, System.Collections.Generic.List<Order> stale) -> void (private)
  Absorbs: RemoveAll terminal-state race guard + acc.Cancel try/catch. CCN<=2.

Parent residual: null guard(1) + foreach(1) + IsQxCancelEligible3(1)
+ stale.Count==0(1) + CommitStaleCancelBatch(0) = CCN<=5. PASS.

### 5.3 Ticket 3 Extractions

#### TryReplacePttBeBrackets (CCN=14 -> target <=8)

Parent at L4055-4126.

Extract:
- IsBeBracketRecoveryEligible(Order cancelledStop) -> bool (private)
  Absorbs: cancelledStop null(1) + instr null(1) + IsFollowerAccount(1)
  + IsFlat(1) + _qxCancelInProgress.ContainsKey(1). CCN<=5.
- HasActiveQxOrders(Account acc, Instrument instr) -> bool (private)
  Absorbs: acc.Orders.ToList().Any with PTT-QX prefix + Working/Submitted + instr match. CCN<=4.

Parent residual: IsBeBracketRecoveryEligible(1) + HasActiveQxOrders(1)
+ prevAttempts>=5(1) + counter increment(0) + TryAdd(1) + QueueBeRetryFallback(0) = CCN<=5. PASS.

#### CancelQxBrackets 2-param (CCN=14 -> target <=8)

Parent at L911-941.

Extract:
- IsQxCancelEligible2(Order o, Instrument instr) -> bool (private static)
  Absorbs: stateOk 5-term OR + instrument null+FullName + IsQxCancelCandidate. CCN<=7.
- CommitQxCancelBatch(Account acc, System.Collections.Generic.List<Order> stale) -> void (private)
  Absorbs: RemoveAll race guard + acc.Cancel try/catch. CCN<=2.
  NOTE: Engineer may consolidate CommitQxCancelBatch and CommitStaleCancelBatch (T2)
  into CommitCancelBatch(Account, List<Order>) if bodies are identical.

Parent residual: null guard(1) + foreach(1) + IsQxCancelEligible2(1)
+ stale.Count==0(1) + CommitQxCancelBatch(0) = CCN<=4. PASS.

#### TryFirePositionState (CCN=13 -> target <=8)

Parent at L3796-3844.

Extract:
- IsPositionStateTriggerState(OrderState s) -> bool (private static)
  Absorbs: state != Filled && state != PartFilled. CCN<=2.
- TryClearLeaderDirectionOnFlat(Account acc, string instrFullName) -> void (private)
  Absorbs: !hasPos block: foreach rules + isLeaderAcct loop + if(isLeaderAcct) TryRemove
  + ClearLiveEntryForInstrument. CCN<=4.

Parent residual: IsPositionStateTriggerState(1) + instrument null(1)
+ Interlocked CAS(1) + prior==newVal early return(1)
+ TryClearLeaderDirectionOnFlat(0) + event invoke(0) = CCN<=5. PASS.

Test seam: internal static bool IsPositionStateTriggerStateTestable(OrderState s)
  => IsPositionStateTriggerState(s);

#### CountLeaderTargets (CCN=13 -> target <=8)

Parent at L5315-5342.

Extract:
- IsNativeLeaderTarget(Order o, string instrFullName) -> bool (private static)
  Absorbs: stateOk(1) + instrOk compound(1) + type check(1) + isTarget 4-part compound(4). CCN<=7.

Parent residual: rule null(1) + leader null(1) + foreach(1) + o null continue(1)
+ IsNativeLeaderTarget(1) = CCN<=5. PASS.

Test seam: internal static bool IsNativeLeaderTargetTestable(OrderState s, string oInstrFN,
  OrderType t, string name, string checkInstrFN).

#### ResubmitTargetAfterCascade (CCN=13 -> target <=8)

Parent at L2907-2973. Block A-Prime + Block B.

Extract:
- CancelStaleTargetDrag(Account acc, Order stpOrder, string tgtDragName) -> void (private)
  Absorbs: Block A-Prime foreach + if(Working && Name && Instrument) + try/catch. CCN<=4.
- CreateAndSubmitCascadeTarget(Account acc, Order stpOrder, double targetPrice, string tgtDragName, Order leaderOrder) -> void (private)
  Absorbs: Block B CreateOrder + null guard(1) + Submit + StatusUpdate + catch. CCN<=3.

Parent residual: TryParseStopSuffix(1) + tgtDragName(0)
+ CancelStaleTargetDrag(0) + CreateAndSubmitCascadeTarget(0) = CCN<=2. PASS.

### 5.4 Ticket 4 Extractions

#### OnOrderUpdate (CCN=12 -> target <=8)

Parent at L1379-1486. Already highly extracted. Residual branches from drain handling block.

Extract:
- HandleDrainTerminalState(Order order) -> void (private)
  Absorbs: Cancelled/Rejected drain-ack branch (ContainsKey + OnDrainCancelAck call)
  + Filled abort-drain branch (AbortDrainOnFill call). CCN<=4.

Parent residual: all existing pre-gate helper calls(0) + HandleDrainTerminalState(1)
+ TryDrainWatchdog(0) + !_isCopyEnabled(1) + FindMatchingRule(1) + null check(1)
+ !Enabled(1) + TryFirePositionState(0) + TryMirrorOrderUpdate(0)
+ TryCancelFollowerEntries(1) + TryDispatchLeaderFlat(1) + TryHandleDrag(1)
+ DispatchCopy(0) = CCN<=8. PASS.

#### CancelAllAccountOrders (CCN=12 -> target <=8)

Parent at L1049-1079.

Extract:
- IsCancelAllStateOk(OrderState s) -> bool (private static)
  Absorbs: Working || Initialized || Submitted || Accepted 4-term OR. CCN<=4.

Parent residual: null guard(1) + foreach(1) + IsCancelAllStateOk(1) + instrument filter(1)
+ RemoveAll terminal race guard(1) = CCN<=5. PASS.

Test seam: internal static bool IsCancelAllStateOkTestable(OrderState s)
  => IsCancelAllStateOk(s);

#### BuildQxSnapshot (CCN=11 -> target <=8)

Parent at L952-980.

Extract:
- IsQxSnapshotStateOk(OrderState s) -> bool (private static)
  Absorbs: Working || Initialized || Accepted || Submitted || TriggerPending 5-term OR. CCN<=5.

Parent residual: null guard(1) + foreach(1) + IsQxSnapshotStateOk(1) + instrument filter(1)
+ IsQxCancelCandidate(1) = CCN<=5. PASS.

Test seam: internal static bool IsQxSnapshotStateOkTestable(OrderState s)
  => IsQxSnapshotStateOk(s);

#### DrainThenDispatch (CCN=11 -> target <=8)

Parent at L6516-6571.

Extract:
- IssueDrainCancels(Account acc, Instrument instrument) -> int (private)
  Absorbs: foreach acc.Orders + stateOk filter + instrument filter
  + cancel issue + _drainOwnedOrderIds TryAdd. Returns cancel count. CCN<=5.

Parent residual: _pendingDispatchDrains upsert(0) + IssueDrainCancels(1)
+ cancelCount==0 immediate-dispatch check(1) = CCN<=4. PASS.

#### FindFollowerBracketOrder IEnumerable overload (CCN=11 -> target <=8)

Parent at L3520-3553.

Extract:
- MatchesBracketType(Order order, bool isStop) -> bool (private static)
  Absorbs: isStop(1) + StopMarket||StopLimit(1) + else Limit+!IsStopLeg(1). CCN<=3.

Parent residual: foreach(1) + OrderPassesBracketGate(1) + 4-state filter(4)
+ MatchesBracketType(1) = CCN<=7. PASS.

Test seam: internal static bool MatchesBracketTypeTestable(OrderType t, OrderState s,
  bool isStop, string name) -- uses primitives.

#### MatchesLeaderName (CCN=11 -> target <=8)

Parent at L3575-3592.

Extract:
- ExtractLegSuffix(string leaderName) -> string (private static)
  Absorbs: leaderName.Length > 0 && char.IsDigit(...) ternary assignment. CCN<=2.

Parent residual: leaderName null(1) + exact name(1) + ExtractLegSuffix(0)
+ !isStop && legSuffix != null && TGT name(1) + isStop && legSuffix != null && STP name(1)
= CCN<=4. PASS.

### 5.5 Ticket 5 Extractions

All Ticket 5 methods are CCN=9 -- one branch over the limit. One small extraction each.

#### HasNakedPosition (CCN=9 -> target <=8)

Parent at L6473-6502.

Extract:
- IsNakedConditionMet(Account acc, Instrument instr) -> bool (private)
  Absorbs: 1 compound boolean cluster. CCN<=4.

#### RuleToDto (CCN=9 -> target <=8)

Parent at L6197-6232.

Extract:
- ExtractAtmTemplateMap(CopyRule rule) -> Dictionary<string, string> (private static)
  Absorbs: foreach FollowerAtmTemplates + conditional value extraction. CCN<=4.

#### IsFollowerAccount (CCN=9 -> target <=8)

Parent at L778-797.

Extract:
- MatchesFollowerSlot(CopyRule rule, Account acc) -> bool (private static)
  Absorbs: for-i + null-slot check + name comparison + FollowerAccountNames fallback. CCN<=5.

#### AllAccounts (CCN=9 -> target <=8)

Parent at L5116-5163.

Extract:
- IsFollowerForInstrument(Account acc, CopyRule rule) -> bool (private static)
  Absorbs: inner follower-account array iteration + null guard. CCN<=3.

#### CaptureLinkedTargetPrice (CCN=9 -> target <=8)

Parent at L2778-2796.

Extract:
- PickBestTargetPrice(double? pttPrice, double? atmPrice) -> double? (private static)
  Absorbs: pttPrice.HasValue ternary + return. CCN<=2.

#### MirrorClose (CCN=9 -> target <=8)

Parent at L2119-2158.

Extract:
- MirrorCloseOneAccount(Account acc, Instrument instr) -> void (private)
  Absorbs: acc null + FindPosition + null/qty guard + direction + CreateOrder try/catch. CCN<=5.

#### BuildUpdatedMultipliers (CCN=9 -> target <=8)

Parent at L1348-1364.

Extract:
- ResolveMultiplierLength(int[] existing, int count) -> int (private static)
  Absorbs: len = count > 0 ? count : (existing != null ? existing.Length : 0). CCN<=3.

#### CaptureOtherLegTargetPrices (CCN=9 -> target <=8)

Parent at L2812-2835.

Extract:
- UpdateLegTargetPrice(double[] prices, int i, Order o, string excludeSuffix) -> void (private static)
  Absorbs: inner for-i body with exclude check + PTT preferred + ATM fallback. CCN<=4.

#### HandleEntryChange (CCN=9 -> target <=8)

Parent at L3736-3771.

Extract:
- IsPriceDeltaSignificant(double newPrice, double currentPrice, double tickSize) -> bool (private static)
  Absorbs: tickSize > 0 && Math.Abs compound. CCN<=2.

#### HandleBracketChange (CCN=9 -> target <=8)

Parent at L3414-3447.

Extract:
- RoundToTick(double rawPrice, double tickSize) -> double (private static)
  Absorbs: tickSize > 0 ternary Math.Round. CCN<=2.

#### CreateFollowerReplacementStop (CCN=9 -> target <=8)

Parent at L3348-3393.

Extract:
- SubmitReplacementStopOrder(Account followerAcc, Instrument instr, int qty, OrderAction stopAction, double stopPrice) -> void (private)
  Absorbs: CreateOrder + null guard + Submit + StatusUpdate + catch. CCN<=4.


---

## 6. Test Strategy

**Test file**: src/PropTraderTools/Tests/BwaveRefactorLaneBTests.cs
**Framework**: xUnit [Fact] ONLY. No NUnit. No MSTest.
**Access**: InternalsVisibleTo granted at CopyEngine.cs L46.

One structural [Fact] per extracted helper method.
For static helpers: pass primitive values directly (existing pattern at L1665-1686).
For instance helpers: test via internal stub seam (pattern: IsPttStpDragCancellableTestable L3152).

Naming: [HelperName]_[Scenario]_[Expected]
Example: IsBeTargetStateOk_Working_ReturnsTrue

Test seam: internal static T {HelperName}Testable(primitive params) => {HelperName}(args);

---

## 7. Scan Checklist (7 Scans -- Engineer Contract)

Each ticket engineer MUST run ALL 7 scans before committing.

### SCAN-01: Lizard CCN -- Zero methods >8 in CopyEngine.cs

Run the exact lizard command from Section 3 of this plan. PASS = zero rows output.

### SCAN-02: No lock() in CopyEngine.cs

Select-String for lock pattern. PASS: Zero matches.

### SCAN-03: No async void

Select-String for async void pattern. PASS: Zero matches.

### SCAN-04: No return null in NEW extracted helpers

Verify no extracted helper introduces a new return null statement.
Pre-existing return null in FindBePosition and FindFollowerBracketOrder etc. are grandfathered.

### SCAN-05: dotnet build -- Zero errors

dotnet build --no-incremental. PASS: Zero errors, zero warnings.

### SCAN-06: ASCII-only -- Zero non-ASCII in CopyEngine.cs

Read all bytes; count those > 127. PASS: Count = 0.

### SCAN-07: dotnet test -- All tests pass

dotnet test --no-build. PASS: All tests pass, zero failures.

---

## 8. NT8 Constraints

### 8.1 Origin

DW-NEXT-B-04: CopyEngine.cs god method complexity accumulated across blocks B7..B142.
This epic closes DW-NEXT-B-04 by driving all CopyEngine methods to CCN <= 8.

### 8.2 Signature Preservation (ABSOLUTE)

Zero changes to ANY public or internal method signatures.

Methods that MUST NOT change signature:
- ArmPendingBe(Instrument instr, Account masterAcc, int bufferTicks) -- internal
- TryCleanupReArmedAtmBracket(OrderEventArgs e) -- internal (test seam caller)
- FindFollowerBracketOrder overloads -- private (test seam overloads exist)
- SnapshotBeTargets(Account acc, Instrument instrument) -- private
- CountLeaderTargets(Instrument instrument) -- private
- MoveStopToBreakEven(Account acc, Instrument instrument, int bufferTicks, bool isRetry) -- private

### 8.3 Thread Safety

All extracted helpers inherit the thread-safety model of their parent:
- Helpers from OnOrderUpdate path: NT8 account bg thread. No UI access.
- Helpers from ArmPendingBe: UI thread (Panel/Window context).
- Helpers from MoveStopToBreakEven: both bg and UI paths.
- Zero new lock() calls. Zero new Dispatcher.InvokeAsync in extracted helpers.

### 8.4 .NET 4.8 Compatibility

- No record types
- No System.Collections.Immutable
- No init-only properties
- NT8 CreateOrder 12-arg form preserved; arg12=(NinjaTrader.Cbi.CustomOrder)null
- NinjaTrader.Core.Globals.MaxDate for GTC expiry

### 8.5 Dismissed Items (Do Not Touch)

- (long)(int)Environment.TickCount -- .NET 4.8 correct. Leave as-is.
- ActiveOrders .ToList() -- DW-NEXT-A-07. Leave as-is.
- _drainOwnedOrderIds ConcurrentDictionary<string, byte> -- NT8 OrderId is string. Leave as-is.
- Features/*.cs -- Lane C scope only. Do not modify.

---

## 9. Risk Register

### R-01: Helper Name Collision Across Tickets

Risk: Two tickets introduce a helper with the same name.
Mitigation: All helper names in this plan are unique (verified in sequential thinking).
Engineer must verify before adding each helper:
  Select-String on CopyEngine.cs for the new method name.

### R-02: Lizard vs Code Comment Discrepancy

Risk: Code comments say CYC=6 but Lizard says CCN=27 (e.g. ArmPendingBe).
Cause: Lizard counts ?., ??, ternary ?:, and each || and && as +1 branch.
Mitigation: Lizard is the authoritative tool. SCAN-01 uses Lizard. All CCN targets
in Section 5 are calibrated against Lizard counting, not hand-counted McCabe.

### R-03: IsCleanupAtmEligible out-param type

Risk: The out-param type (Instrument Instr, DateTime Expiry) must exactly match
the _qxPendingFollowerCleanup value type.
Mitigation: Use exactly:
  private bool IsCleanupAtmEligible(OrderEventArgs e,
    out (Instrument Instr, DateTime Expiry) entry)
Caller: if (!IsCleanupAtmEligible(e, out var entry)) return;

### R-04: CommitStaleCancelBatch / CommitQxCancelBatch consolidation

Risk: Engineer consolidates T2 and T3 cancel-batch helpers incorrectly.
Mitigation: Plan permits consolidation as CommitCancelBatch(Account, List<Order>).
Both callers must be updated if consolidated.

### R-05: SyncAtmFollowerTarget Block A/B separation

Risk: After extracting CancelBlockAAtmTarget, the Block A cancel try/catch stays in parent.
The parent still has 2 branches from the Block A try/catch.
Mitigation: Parent residual CCN calculation accounts for these 2 branches (CYC<=4). PASS.

---

## 10. Deferred Items

The following items are explicitly NOT in scope for BWAVE-REFACTOR LaneB:

- DW-NEXT-A-07: ActiveOrders .ToList() -- deferred. Leave as-is.
- DW-NEXT-A-07: (long)(int)Environment.TickCount -- .NET 4.8 correct. Dismissed.
- _drainOwnedOrderIds type -- ConcurrentDictionary<string, byte> correct. Dismissed.
- Features/*.cs CCN violations -- Lane C scope only. Do not touch.
- BWAVE-NEXT LaneBRepair backlog items unrelated to CCN in CopyEngine.cs -- deferred.

---

## Component Summary

| Component | Type | File | Purpose |
|-----------|------|------|---------|
| CopyEngine | Modified | src/PropTraderTools/CopyEngine.cs | 32 methods extracted to CCN<=8 |
| BwaveRefactorLaneBTests | New | src/PropTraderTools/Tests/BwaveRefactorLaneBTests.cs | xUnit structural tests |

VERIFY_PASS requirement: Zero CopyEngine.cs methods with CCN > 8 in Lizard output.
NT8 SYNC: powershell -File scripts\ptt-sync-and-verify.ps1 must show 18/18 OK.
F5: NinjaTrader 8 F5 compilation required before FINAL_PASS.

---

Architecture plan written by ptt-architect. Status: PLAN_COMPLETE.
