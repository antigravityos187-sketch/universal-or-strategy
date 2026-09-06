# BWAVE-REFACTOR Lane B -- Tickets

# Phase 3 Output

# Author: ptt-architect

# Plan source: docs/brain/BWAVE-REFACTOR/LaneB/02-architecture-plan.md (REVIEW_PASS)

# Review source: docs/brain/BWAVE-REFACTOR/LaneB/02-plan-review.md (REVIEW_PASS)

# Written: 2026-09-06

---

## Preamble

**File under modification**: `src/PropTraderTools/CopyEngine.cs`
**New test file**: `src/PropTraderTools/Tests/BwaveRefactorLaneBTests.cs`
**Execution order**: Sequential (T1 -> T2 -> T3 -> T4 -> T5). Do NOT start a ticket until the
previous ticket passes all 7 scans.
**Name collision rule**: Before adding any new private method, run:
`Select-String -Path src/PropTraderTools/CopyEngine.cs -Pattern "private.*HelperName"`
Must return zero matches. If name exists, choose a more specific name.
**InternalsVisibleTo**: Already declared at CopyEngine.cs L46 -- do NOT add another one.
**No behavior change**: All extractions are mechanical. No logic may change.
**No signature change**: Public and internal method signatures are frozen.

---

## Ticket 1 (T1) -- Tier A: CCN >= 20 (6 methods)

### Spec Requirement IDs

- BWAVE-REFACTOR-LaneB-T1
- Targets: ArmPendingBe CCN 27-><=8, ResubmitOneCollateralLeg CCN 25-><=8,
  SnapshotBeTargets CCN 24-><=8, TryCleanupReArmedAtmBracket CCN 23-><=8,
  SyncAtmFollowerTarget CCN 21-><=8, SyncFollowerBracket CCN 20-><=8

### Target Method Signatures (exact, from CopyEngine.cs)

```csharp
internal void ArmPendingBe(Instrument instr, Account masterAcc, int bufferTicks)           // L5729
private void ResubmitOneCollateralLeg(Account acc, Order fo, double newPrice,               // L3026
    double targetPrice, string suffix, Order leaderLeg = null)
private List<(double Price, int Qty, OrderAction Action)> SnapshotBeTargets(               // L5349
    Account acc, Instrument instrument)
internal void TryCleanupReArmedAtmBracket(OrderEventArgs e)                                 // L4138
private void SyncAtmFollowerTarget(Account acc, Order fo, double newPrice,                  // L3216
    Order? leaderOrder = null)
private void SyncFollowerBracket(Account acc, Order leaderOrder, bool isStop,              // L2539
    double newPrice, double tickSize)
```

### Extraction Instructions

#### 1. ArmPendingBe (L5729-L5785, CCN=27 -> target <=8)

The method has a large compound boolean block inside the `if (tickSize > 0.0)` branch (L5749-5770)
that computes `isLong`, `target`, `refBid`, `refAsk`, `refPx`, and `alreadyAtBe`, then fires
immediately if true.

**Extract A**: `IsImmediateBeEligible`

- **Signature**: `private static bool IsImmediateBeEligible(Position pos, Instrument instr, int bufferTicks)`
- **Visibility**: `private static` (uses only parameters, no instance fields)
- **Absorbs**: the entire tickSize guard body (L5749-5770): `bool isLong`, `double target`,
  `double refBid`, `double refAsk`, `double refPx`, `bool alreadyAtBe`, and the `return alreadyAtBe`
  expression. Returns `true` if price is already at/past BE level, `false` otherwise.
  Returns `false` immediately if `tickSize <= 0`.
- **Expected CCN**: <= 6

**Extract B**: `FireImmediateBe`

- **Signature**: `private void FireImmediateBe(Account masterAcc, Instrument instr, int bufferTicks)`
- **Visibility**: `private` (uses `BreakEven` instance method and `PendingBeFired` event field)
- **Absorbs**: The two statements inside `if (alreadyAtBe)` after the StatusUpdate: the
  `BreakEven(masterAcc, instr, bufferTicks)` call and the `PendingBeFired?.Invoke(...)` call.
- **Expected CCN**: <= 2

**Parent residual after extraction**:
`instr null(1) + masterAcc null(2) + IsFlat(3) + IsImmediateBeEligible(4) + if alreadyAtBe(5)

- StatusUpdate + FireImmediateBe + return(0) + slot upsert(6) + PendingBeArmed + subscribe = CCN<=7`

**Test seam**: Add immediately below `ArmPendingBe`:

```csharp
internal static bool IsImmediateBeEligibleTestable(Position pos, Instrument instr, int bufferTicks)
    => IsImmediateBeEligible(pos, instr, bufferTicks);
```

---

#### 2. ResubmitOneCollateralLeg (L3026-L3133, CCN=25 -> target <=8)

The method has two Block A-Prime foreach+if+try/catch cancel loops (lines 3038-3066) and two
Block B try/catch CreateOrder+Submit blocks (lines 3068-3132).

**Extract A**: `CancelLiveCollateralStop`

- **Signature**: `private void CancelLiveCollateralStop(Account acc, Order fo, string stpDragName)`
- **Visibility**: `private` (accesses `StatusUpdate` event field -- not static)
- **Absorbs**: Block A-Prime-Stop (L3037-3050): the foreach over `acc.Orders.ToList()`, the
  if-condition `IsPttStpDragCancellable(o) && o.Name == stpDragName && o.Instrument?.FullName == fo.Instrument?.FullName`,
  and the inner try/catch `acc.Cancel(new Order[] { o })`.
- **Expected CCN**: <= 4

**Extract B**: `CancelLiveCollateralTarget`

- **Signature**: `private void CancelLiveCollateralTarget(Account acc, Order fo, string tgtDragName)`
- **Visibility**: `private` (accesses `StatusUpdate`)
- **Absorbs**: Block A-Prime-Target (L3052-3066): foreach over `acc.Orders.ToList()`, the
  if-condition `IsTargetOrderLive(o) && o.Name == tgtDragName && o.Instrument?.FullName == fo.Instrument?.FullName`,
  and the inner try/catch `acc.Cancel(new Order[] { o })`.
- **Expected CCN**: <= 4

**Extract C**: `CreateAndSubmitCollateralStop`

- **Signature**: `private void CreateAndSubmitCollateralStop(Account acc, Order fo, double newPrice, string suffix, Order leaderLeg)`
- **Visibility**: `private` (accesses `StatusUpdate`)
- **Absorbs**: First Block B (L3068-3099): the try/catch that calls `acc.CreateOrder(StopMarket)`,
  checks `newStop == null`, calls `acc.Submit`, and calls `StatusUpdate`.
  Use `leaderLeg != null ? leaderLeg.Quantity : fo.Quantity` for qty (preserve existing logic).
  Order name: `"PTT-STP-Drag-" + suffix`.
- **Expected CCN**: <= 4

**Extract D**: `CreateAndSubmitCollateralTarget`

- **Signature**: `private void CreateAndSubmitCollateralTarget(Account acc, Order fo, double targetPrice, string suffix, Order leaderLeg)`
- **Visibility**: `private` (accesses `StatusUpdate`)
- **Absorbs**: Second Block B (L3101-3132): the try/catch that calls `acc.CreateOrder(Limit)`,
  checks `newTarget == null`, calls `acc.Submit`, and calls `StatusUpdate`.
  Order name: `"PTT-TGT-Drag-" + suffix`.
- **Expected CCN**: <= 4

**Parent residual**: `CancelLiveCollateralStop + CancelLiveCollateralTarget + leaderLeg null check(1)

- CreateAndSubmitCollateralStop + leaderLeg null check(1) + CreateAndSubmitCollateralTarget = CCN<=4`

---

#### 3. SnapshotBeTargets (L5349-L5392, CCN=24 -> target <=8)

The method's CCN is driven by the 7-state `stateOk` OR compound (L5362-5369) and the
compound `isNative`/`isPtt` predicates (L5375-5385).

**Extract A**: `IsBeTargetStateOk`

- **Signature**: `private static bool IsBeTargetStateOk(OrderState s)`
- **Visibility**: `private static` (pure predicate, no instance fields)
- **Absorbs**: The 7-arm `stateOk` boolean expression:
  `Working || Accepted || Submitted || Initialized || TriggerPending || ChangeSubmitted || CancelSubmitted`
- **Expected CCN**: <= 7

**Extract B**: `ClassifyBeTarget`

- **Signature**: `private static void ClassifyBeTarget(Order o, string instrFullName, out bool isNative, out bool isPtt)`
- **Visibility**: `private static` (pure computation from parameters)
- **Absorbs**: The compound `instrOk` check, the `OrderType.Limit` type check, the `isNative`
  4-part compound, and the `isPtt` 2-part compound (L5370-5385).
  Sets `isNative = false; isPtt = false` as defaults at entry.
  Returns early (both false) if `!instrOk || o.OrderType != OrderType.Limit`.
- **Expected CCN**: <= 6

**Parent residual**: `acc/instr null(1) + foreach(1) + o null continue(1) + IsBeTargetStateOk(1)

- ClassifyBeTarget(0) + isNative branch(1) + isPtt branch(1) = CCN<=7`

**Test seam**: Add immediately below `SnapshotBeTargets`:

```csharp
internal static bool IsBeTargetStateOkTestable(OrderState s) => IsBeTargetStateOk(s);
```

---

#### 4. TryCleanupReArmedAtmBracket (L4138-L4204, CCN=23 -> target <=8)

The method opens with a massive 10-condition compound OR guard (L4148-4162), then has a
foreach loop (L4172-4183), a cancel-if-found branch (L4186-4197), and a removal policy branch (L4201-4203).

**Extract A**: `IsCleanupAtmEligible`

- **Signature**: `private bool IsCleanupAtmEligible(OrderEventArgs e, out (Instrument Instr, DateTime Expiry) entry)`
- **Visibility**: `private` (calls `IsFollowerAccount` instance method and reads `_qxPendingFollowerCleanup`)
- **CRITICAL TYPE**: The out-param tuple type `(Instrument Instr, DateTime Expiry)` must EXACTLY
  match the value type stored in `_qxPendingFollowerCleanup`. Do NOT change it.
- **Absorbs**: The entire compound guard at L4148-4162 (all 10 conditions). Sets `entry = default`
  at the top. Returns `true` only if ALL conditions pass (i.e., the guard negation evaluates to false).
- **Expected CCN**: <= 8

**Extract B**: `TryCancelNativeAtmTarget`

- **Signature**: `private void TryCancelNativeAtmTarget(Account acc, Instrument instr, char tChar)`
- **Visibility**: `private` (calls `acc.Cancel`, reads no instance fields directly)
- **Absorbs**: The foreach loop (L4172-4183): iteration over `acc.Orders.ToList()`,
  the if-condition `o.Name == nativeName && o.Instrument?.FullName == instr.FullName && (Working||Accepted)`,
  assign `toCancel = o`, break; then the `if (toCancel != null)` cancel block (L4186-4197).
  Builds `nativeName = "Target" + tChar` locally.
- **Expected CCN**: <= 4

**Extract C**: `EvaluateCleanupRemoval`

- **Signature**: `private void EvaluateCleanupRemoval(Account acc, char tChar, DateTime expiry)`
- **Visibility**: `private` (accesses `_qxPendingFollowerCleanup`)
- **Absorbs**: L4201-4203: `bool shouldRemove = tChar == '3' || expiry <= DateTime.UtcNow;` and
  `if (shouldRemove) _qxPendingFollowerCleanup.TryRemove(acc.Name, out _);`
- **Expected CCN**: <= 2

**Parent residual after extraction**:

```csharp
if (!IsCleanupAtmEligible(e, out var entry)) return;   // (1)
char tChar = e.Order.Name[8];
TryCancelNativeAtmTarget(e.Order.Account, entry.Instr, tChar);
EvaluateCleanupRemoval(e.Order.Account, tChar, entry.Expiry);
// CCN <= 2
```

---

#### 5. SyncAtmFollowerTarget (L3216-L3300, CCN=21 -> target <=8)

The method has 3 top-level guards (L3223-3228), then a Block A-Prime foreach+try/catch (L3240-3257),
a Block A cancel try/catch (L3260-3267), a Block B create try/catch (L3270-3297), and a Phase C call.

**Extract A**: `IsAtmTargetSyncEligible`

- **Signature**: `private bool IsAtmTargetSyncEligible(Account acc, Order fo, double newPrice)`
- **Visibility**: `private` (calls `IsNoPriceChange` instance method)
- **Absorbs**: Guards at L3223-3228: `acc == null(1)`, `fo == null(2)`,
  `fo.LimitPrice <= 0 || IsNoPriceChange(fo.LimitPrice, newPrice)(3)`. Returns `false` if any
  guard fires, `true` if all pass.
- **Expected CCN**: <= 4

**Extract B**: `CancelBlockAAtmTarget`

- **Signature**: `private void CancelBlockAAtmTarget(Account acc, Order fo, string tgtDragName)`
- **Visibility**: `private` (accesses `StatusUpdate`)
- **Absorbs**: Block A-Prime (L3240-3257): foreach over `acc.Orders.ToList()`, if-condition
  `o.OrderState == Working && o.Name == tgtDragName && o.Instrument?.FullName == fo.Instrument?.FullName`,
  inner try/catch cancel. PLUS Block A cancel (L3260-3267): the standalone `try { acc.Cancel(new Order[]{fo}); } catch`.
- **Expected CCN**: <= 5

**Extract C**: `BlockBCreateAtmTarget`

- **Signature**: `private void BlockBCreateAtmTarget(Account acc, Order fo, double newPrice, string tgtDragName, Order leaderOrder)`
- **Visibility**: `private` (accesses `StatusUpdate`)
- **Absorbs**: Block B (L3270-3297): try/catch that calls `acc.CreateOrder(Limit)`, null-guards
  `newTarget`, calls `acc.Submit`, calls `StatusUpdate`. Use `leaderOrder != null ? leaderOrder.Quantity : fo.Quantity` for qty.
- **Expected CCN**: <= 4

**Parent residual**: `IsAtmTargetSyncEligible(1) + DeriveLeaderBracketIndex(0) + tgtDragName(0)

- CancelBlockAAtmTarget(0) + BlockBCreateAtmTarget(0) + ExecutePhaseCStopReplacement(0) = CCN<=2`

---

#### 6. SyncFollowerBracket (L2539-L2639, CCN=20 -> target <=8)

The method has: ATM STP path (L2571-2606), ATM TGT path (L2608-2612), trailing stop skip (L2614-2618),
and acc.Change() try/catch (L2620-2638).

**Extract A**: `HandleAtmStopSync`

- **Signature**: `private void HandleAtmStopSync(Account acc, Order fo, double newPrice, double tickSize, string legSuffix, Order leaderOrder)`
- **Visibility**: `private` (calls multiple instance methods)
- **Absorbs**: The entire `if (isStop && IsAtmSTPOrder(fo))` block body (L2572-2606):
  the `fo.StopPrice < tickSize` guard-return, `TryParseStopSuffix`, `CaptureLinkedTargetPrice`,
  `CaptureOtherLegTargetPrices`, `SyncAtmFollowerBracket`, the `capturedTargetPrice.HasValue` branch,
  `ResubmitTargetAfterCascade`, `ResubmitCollateralLegs`, and `return`.
  Pass `legSuffix` as the derived suffix string.
- **Expected CCN**: <= 6

**Extract B**: `HandleAtmTargetSync`

- **Signature**: `private void HandleAtmTargetSync(Account acc, Order fo, double newPrice, Order leaderOrder)`
- **Visibility**: `private` (calls `SyncAtmFollowerTarget`)
- **Absorbs**: The `if (!isStop && IsAtmSTPOrder(fo))` block body (L2609-2611):
  `SyncAtmFollowerTarget(acc, fo, newPrice, leaderOrder)` call and return.
- **Expected CCN**: <= 1

**Extract C**: `HandleNonAtmSync`

- **Signature**: `private void HandleNonAtmSync(Account acc, Order fo, bool isStop, double newPrice)`
- **Visibility**: `private` (accesses `StatusUpdate`)
- **Absorbs**: The trailing-stop skip (L2614-2617) and the acc.Change() try/catch (L2620-2638):
  `if (isStop && IsTrailingStop(fo)) { StatusUpdate; return; }`
  then the try/catch for `fo.StopPrice = newPrice` or `fo.LimitPrice = newPrice`, then `acc.Change`.
- **Expected CCN**: <= 4

**Parent residual**: `fo null(1) + tickSize priceDelta(2) + isStop&&IsAtmSTPOrder -> HandleAtmStopSync(3)

- !isStop&&IsAtmSTPOrder -> HandleAtmTargetSync(4) + else HandleNonAtmSync = CCN<=5`

---

### New Helper Signatures for T1

```csharp
// ArmPendingBe helpers
private static bool IsImmediateBeEligible(Position pos, Instrument instr, int bufferTicks)
private void FireImmediateBe(Account masterAcc, Instrument instr, int bufferTicks)
internal static bool IsImmediateBeEligibleTestable(Position pos, Instrument instr, int bufferTicks)

// ResubmitOneCollateralLeg helpers
private void CancelLiveCollateralStop(Account acc, Order fo, string stpDragName)
private void CancelLiveCollateralTarget(Account acc, Order fo, string tgtDragName)
private void CreateAndSubmitCollateralStop(Account acc, Order fo, double newPrice, string suffix, Order leaderLeg)
private void CreateAndSubmitCollateralTarget(Account acc, Order fo, double targetPrice, string suffix, Order leaderLeg)

// SnapshotBeTargets helpers
private static bool IsBeTargetStateOk(OrderState s)
private static void ClassifyBeTarget(Order o, string instrFullName, out bool isNative, out bool isPtt)
internal static bool IsBeTargetStateOkTestable(OrderState s)

// TryCleanupReArmedAtmBracket helpers
private bool IsCleanupAtmEligible(OrderEventArgs e, out (Instrument Instr, DateTime Expiry) entry)
private void TryCancelNativeAtmTarget(Account acc, Instrument instr, char tChar)
private void EvaluateCleanupRemoval(Account acc, char tChar, DateTime expiry)

// SyncAtmFollowerTarget helpers
private bool IsAtmTargetSyncEligible(Account acc, Order fo, double newPrice)
private void CancelBlockAAtmTarget(Account acc, Order fo, string tgtDragName)
private void BlockBCreateAtmTarget(Account acc, Order fo, double newPrice, string tgtDragName, Order leaderOrder)

// SyncFollowerBracket helpers
private void HandleAtmStopSync(Account acc, Order fo, double newPrice, double tickSize, string legSuffix, Order leaderOrder)
private void HandleAtmTargetSync(Account acc, Order fo, double newPrice, Order leaderOrder)
private void HandleNonAtmSync(Account acc, Order fo, bool isStop, double newPrice)
```

### JS Rule Constraints

| Rule                                | Applies To                                                                        |
| ----------------------------------- | --------------------------------------------------------------------------------- |
| JS-021 (no lock())                  | All helpers: zero new lock() calls                                                |
| JS-001 (no throw in hot path)       | CreateAndSubmitCollateralStop/Target: absorb existing try/catch, do NOT add throw |
| JS-002 (no return null in new code) | IsImmediateBeEligible, IsBeTargetStateOk, ClassifyBeTarget: return bool, not null |
| JS-033 (no async void)              | All helpers: no async modifier                                                    |
| ASCII-only                          | All new helper names and string literals: ASCII only                              |
| CYC<=8                              | Each new helper: verify with lizard after writing                                 |

### xUnit [Fact] Test Names (in BwaveRefactorLaneBTests.cs -- create file in T1)

Ticket 1 creates the test file. File header:

```csharp
// BwaveRefactorLaneBTests.cs -- xUnit structural tests for BWAVE-REFACTOR LaneB
// InternalsVisibleTo("PropTraderTools.Tests") declared at CopyEngine.cs L46.
using Xunit;
using PropTraderTools;

namespace PropTraderTools.Tests
{
    public class BwaveRefactorLaneBTests
    {
    }
}
```

One [Fact] per static helper (via test seam) and one [Fact] per instance helper (structural existence check):

```
IsBeTargetStateOk_Working_ReturnsTrue
IsBeTargetStateOk_CancelSubmitted_ReturnsTrue
IsBeTargetStateOk_Filled_ReturnsFalse
IsImmediateBeEligible_NullPosition_ReturnsFalse
IsImmediateBeEligible_ZeroTickSize_ReturnsFalse
```

**Test implementation guidance**:

- `IsBeTargetStateOk_Working_ReturnsTrue`: Call `CopyEngine.IsBeTargetStateOkTestable(OrderState.Working)`. Assert `true`.
- `IsBeTargetStateOk_CancelSubmitted_ReturnsTrue`: Call with `OrderState.CancelSubmitted`. Assert `true`.
- `IsBeTargetStateOk_Filled_ReturnsFalse`: Call with `OrderState.Filled`. Assert `false`.
- `IsImmediateBeEligible_NullPosition_ReturnsFalse`: Call `CopyEngine.IsImmediateBeEligibleTestable(null, null, 2)`. Assert `false`.
- `IsImmediateBeEligible_ZeroTickSize_ReturnsFalse`: Pass a Position stub where `TickSize=0`. Assert `false`. Note: if Position cannot be mocked without NT8 runtime, stub the seam to accept primitives (see NT8 constraints note below).

**NT8 note**: If Position/Instrument cannot be constructed in xUnit without NT8 runtime, restructure `IsImmediateBeEligibleTestable` to accept primitives: `bool isLong, double avgPrice, double refBid, double refAsk, int bufferTicks, double tickSize` and verify the arithmetic directly.

### 7-Scan Checklist

```
SCAN-01: lizard CCN -- run: lizard src/PropTraderTools/CopyEngine.cs --CCN 8
         PASS condition: zero rows output for the 6 methods in this ticket's scope:
         ArmPendingBe, ResubmitOneCollateralLeg, SnapshotBeTargets,
         TryCleanupReArmedAtmBracket, SyncAtmFollowerTarget, SyncFollowerBracket.
         Also verify all new helper methods score <= 8.

SCAN-02: grep lock( -- run: Select-String -Path src/PropTraderTools/CopyEngine.cs -Pattern "lock\s*\("
         PASS condition: zero matches.

SCAN-03: grep "async void" -- run: Select-String -Path src/PropTraderTools/CopyEngine.cs -Pattern "async\s+void"
         PASS condition: zero matches.

SCAN-04: grep "return null" in NEW helpers only:
         Select-String on the new helper methods added in this ticket.
         PASS condition: zero matches in newly added code.
         Pre-existing return null in CaptureLinkedTargetPrice (L2781) is grandfathered.

SCAN-05: dotnet build --no-incremental from C:\WSGTA\ptt-lane-b\
         PASS condition: zero errors, zero warnings.

SCAN-06: ASCII-only -- run:
         [System.IO.File]::ReadAllBytes("src/PropTraderTools/CopyEngine.cs") | Where-Object { $_ -gt 127 } | Measure-Object
         PASS condition: Count = 0.

SCAN-07: dotnet test --no-build from C:\WSGTA\ptt-lane-b\
         PASS condition: all tests in BwaveRefactorLaneBTests.cs pass, zero failures.
```

### NT8 Constraints

- `IsCleanupAtmEligible` out-param type must be exactly `(Instrument Instr, DateTime Expiry)` to match `_qxPendingFollowerCleanup` value type.
- `CreateAndSubmitCollateralStop/Target`: preserve `NinjaTrader.Core.Globals.MaxDate` and `(NinjaTrader.Cbi.CustomOrder)null` as arg11/arg12 in all `CreateOrder` calls.
- `BlockBCreateAtmTarget`: preserve `(NinjaTrader.Cbi.CustomOrder)null` as arg12.
- No `AtmStrategyCreate`, no `AtmStrategyChangeStopTarget` (StrategyBase-only, banned in AddOnBase).
- `HandleNonAtmSync`: `acc.Change()` is AddOnBase-available and correct for non-ATM brackets here.
- Thread safety: all helpers inherit parent thread context. No new `Dispatcher.InvokeAsync`.

### Acceptance Criteria

- [ ] Lizard reports CCN<=8 for all 6 parent methods and all new helpers.
- [ ] SCAN-02 through SCAN-06 return zero.
- [ ] SCAN-07: all 5 new [Fact] tests pass.
- [ ] Zero behavior change confirmed by code review of diff.
- [ ] No public or internal signature changed.

---

## Ticket 2 (T2) -- Tier B: CCN 16-19 (4 methods)

### Spec Requirement IDs

- BWAVE-REFACTOR-LaneB-T2
- Prerequisite: T1 must pass all 7 scans before starting T2.
- Targets: FlattenOneAccount CCN 19-><=8, MoveStopToBreakEven CCN 18-><=8,
  ReplaceFollowerCopyOnAtmCancel CCN 18-><=8, CancelQxBrackets 3-param CCN 16-><=8

### Target Method Signatures (exact, from CopyEngine.cs)

```csharp
private void FlattenOneAccount(Account acc, Instrument instrument)                           // L4714
private void MoveStopToBreakEven(Account acc, Instrument instrument,                         // L5404
    int bufferTicks, bool isRetry = false)
private void ReplaceFollowerCopyOnAtmCancel(Order cancelledOrder)                            // L3895
internal void CancelQxBrackets(Account acc, NinjaTrader.Cbi.Instrument instr,               // L991
    System.Collections.Generic.HashSet<NinjaTrader.Cbi.Order> snapshot)
```

### Extraction Instructions

#### 1. FlattenOneAccount (L4714-L4783, CCN=19 -> target <=8)

The method has a guard-scan foreach loop (L4724-4739) that checks for an active PTT-Flatten order
before proceeding, a position null/qty guard, a CancelAllAccountOrders call, a post-cancel re-read
guard, an action ternary, and a try/catch CreateOrder+Submit block.

**Extract A**: `IsAccountFlattenable`

- **Signature**: `private bool IsAccountFlattenable(Account acc, Instrument instr)`
- **Visibility**: `private` (calls `FindPosition`, `IsFlat` instance helpers; accesses `StatusUpdate`)
- **Absorbs**: The foreach guard-scan (L4724-4739): iterate `acc.Orders.ToList()`, check
  `o.Name == "PTT-Flatten"`, instrument match, then state check
  `Submitted || Accepted || Working`; if found: `StatusUpdate?.Invoke; return false`.
  After the loop, read position via `FindPosition`: if `pos == null || pos.Quantity == 0`:
  `StatusUpdate?.Invoke; return false`. Otherwise return `true`.
- **Expected CCN**: <= 4

**Extract B**: `SubmitMarketFlattenOrder`

- **Signature**: `private void SubmitMarketFlattenOrder(Account acc, Instrument instrument, Position pos)`
- **Visibility**: `private` (accesses `StatusUpdate`)
- **Absorbs**: The post-cancel re-read + action + try/catch block (L4748-4782):
  re-read position with `FindPosition`, null/qty guard return, ternary action computation,
  try `acc.CreateOrder(Market, "PTT-Flatten")`, null-guard on created order, `acc.Submit`, `StatusUpdate`, catch.
- **Expected CCN**: <= 4

**Parent residual**: `IsAccountFlattenable(1) + CancelAllAccountOrders(0) + SubmitMarketFlattenOrder(0) = CCN<=2`

---

#### 2. MoveStopToBreakEven (L5404-L5544, CCN=18 -> target <=8)

The method already delegates to `SnapshotBeTargets` and `PttBreakEvenSwap.Execute`. The remaining
branches are: `IsFlat` guard (1), calc (0), diag-log foreach (1), `SnapshotBeTargets` call (0),
while-cap loop (1), `PttBreakEvenSwap.Execute` (0), `targets == 0` block (2 inner branches),
`!isRetry && IsFollowerAccount` block (3 inner branches). Total: CCN=8 at boundary. Two sub-blocks
can be extracted to create headroom.

**Extract A**: `LogDiagOrderCount`

- **Signature**: `private void LogDiagOrderCount(Account acc, Instrument instrument)`
- **Visibility**: `private` (calls `NinjaTrader.Code.Output.Process`)
- **Absorbs**: The diag foreach and Output.Process call (L5433-5440): `int diagTotal = 0; foreach (Order o in acc.Orders) if (o?.Instrument?.FullName == instrument?.FullName) diagTotal++; NinjaTrader.Code.Output.Process(...)`.
- **Expected CCN**: <= 2

**Extract B**: `RegisterBeRetrySlotIfNeeded`

- **Signature**: `private void RegisterBeRetrySlotIfNeeded(Account acc, Instrument instrument, int bufferTicks, bool isRetry, int targetsCount, int leaderCount)`
- **Visibility**: `private` (accesses `_pendingFollowerBeSlots`, `IsFollowerAccount`, `IsFlat`,
  `FindPosition`, `QueueBeRetryFallback`, `NinjaTrader.Code.Output.Process`)
- **Absorbs**: The entire `targets.Count == 0` block (L5461-5479) AND the `!isRetry && IsFollowerAccount`
  block (L5482-5508). Both are combined because each references the retry slot registration pattern.
  The caller passes `targetsCount = targets.Count` and `leaderCount = CountLeaderTargets(instrument)`.
- **Expected CCN**: <= 6

**Parent residual**: `IsFlat(1) + calc(0) + LogDiagOrderCount(0) + SnapshotBeTargets(0)

- while-cap(1) + PttBreakEvenSwap.Execute(0) + targets==0 early return(1)
- RegisterBeRetrySlotIfNeeded(0) = CCN<=4`

Note: The `targets.Count == 0` early `return` after `RegisterBeRetrySlotIfNeeded` stays in the
parent. `RegisterBeRetrySlotIfNeeded` does NOT return early for the parent -- the parent returns
after the call when `targets.Count == 0`.

---

#### 3. ReplaceFollowerCopyOnAtmCancel (L3895-L3948, CCN=18 -> target <=8)

The method has: `!_isCopyEnabled` guard (1), a foreach+for-i match loop (2+3), multiple early returns
(4-6), and a Named-mode branch (7).

**Extract A**: `FindFollowerRuleForOrder`

- **Signature**: `private CopyRule? FindFollowerRuleForOrder(Order cancelledOrder, out int followerIndex)`
- **Visibility**: `private` (reads `_rules` instance field)
- **Absorbs**: The foreach+for-i block (L3901-3916) that iterates `_rules`, matches
  `rule.Instrument`, iterates `rule.FollowerAccounts`, matches account name, sets `matchedRule`
  and `followerIndex`, breaks. Returns the matched `CopyRule?` or null.
  Initialize `followerIndex = -1` at entry.
- **Expected CCN**: <= 5

**Extract B**: `IsReplaceDispatchEligible`

- **Signature**: `private bool IsReplaceDispatchEligible(CopyRule rule, int followerIndex, Order cancelledOrder)`
- **Visibility**: `private` (calls `HasOpenPosition`, `HasWorkingPttCopy` instance methods)
- **Absorbs**: The six eligibility checks (L3917-3927): `!matchedRule.HasValue || followerIndex < 0(1)`,
  `leader == null(2)`, `!HasOpenPosition(leader)(3)`, `HasOpenPosition(follower)(4)`,
  `HasWorkingPttCopy(5)`. Returns `false` for any early-exit condition, `true` if all pass.
  The `leader` is obtained from `rule.MasterAccount` inside this helper.
  Note: caller already confirms `matchedRule.HasValue` before calling -- helper receives unwrapped `CopyRule rule`.
- **Expected CCN**: <= 6

**Parent residual**: `!_isCopyEnabled(1) + FindFollowerRuleForOrder(0)

- IsReplaceDispatchEligible(1) + signal creation(0) + ResolveAtmMode(0) + Named branch(1) = CCN<=4`

---

#### 4. CancelQxBrackets 3-param overload (L991-L1040, CCN=16 -> target <=8)

This is `internal void CancelQxBrackets(Account acc, NinjaTrader.Cbi.Instrument instr, HashSet<Order> snapshot)`.
It has a null guard (1), foreach (2), 5-term stateOk OR (3), instrument filter (4), snapshot filter (5),
`IsQxCancelCandidate` (6), stale.Count==0 (7) = CCN=7 currently reported as 16 by Lizard because
the 5-term OR is counted per-branch. The key extraction reduces the OR chain.

**Extract A**: `IsQxCancelEligible3`

- **Signature**: `private static bool IsQxCancelEligible3(Order o, Instrument instr, System.Collections.Generic.HashSet<NinjaTrader.Cbi.Order> snapshot)`
- **Visibility**: `private static` (pure predicate, no instance fields)
- **Absorbs**: The 5-term stateOk OR compound, the instrument null+FullName check, the
  `snapshot != null && !snapshot.Contains(o)` race-skip check, and `IsQxCancelCandidate(o)`.
  Returns `true` only if: state is OK AND instrument matches AND (snapshot is null OR order is in snapshot) AND `IsQxCancelCandidate(o)`.
- **Expected CCN**: <= 7

**Extract B**: `CommitStaleCancelBatch`

- **Signature**: `private void CommitStaleCancelBatch(Account acc, System.Collections.Generic.List<Order> stale)`
- **Visibility**: `private` (accesses `acc.Cancel` -- instance context needed for exception pathway; no instance fields)
- **Absorbs**: The `stale.RemoveAll` race guard and the `try { acc.Cancel(stale.ToArray()); } catch { }` block (L1032-1039).
- **Expected CCN**: <= 2

**Consolidation note**: The engineer MAY consolidate `CommitStaleCancelBatch` (T2) and `CommitQxCancelBatch` (T3)
into a single `private void CommitCancelBatch(Account acc, System.Collections.Generic.List<Order> stale)`
if the method bodies are identical. Both callers must be updated if consolidated.

**Parent residual**: `null guard(1) + raceSkipped counter(0) + foreach(1) + IsQxCancelEligible3(1)

- stale.Count==0(1) + CommitStaleCancelBatch(0) = CCN<=5`

---

### New Helper Signatures for T2

```csharp
// FlattenOneAccount helpers
private bool IsAccountFlattenable(Account acc, Instrument instr)
private void SubmitMarketFlattenOrder(Account acc, Instrument instrument, Position pos)

// MoveStopToBreakEven helpers
private void LogDiagOrderCount(Account acc, Instrument instrument)
private void RegisterBeRetrySlotIfNeeded(Account acc, Instrument instrument, int bufferTicks,
    bool isRetry, int targetsCount, int leaderCount)

// ReplaceFollowerCopyOnAtmCancel helpers
private CopyRule? FindFollowerRuleForOrder(Order cancelledOrder, out int followerIndex)
private bool IsReplaceDispatchEligible(CopyRule rule, int followerIndex, Order cancelledOrder)

// CancelQxBrackets 3-param helpers
private static bool IsQxCancelEligible3(Order o, Instrument instr,
    System.Collections.Generic.HashSet<NinjaTrader.Cbi.Order> snapshot)
private void CommitStaleCancelBatch(Account acc,
    System.Collections.Generic.List<Order> stale)
```

### JS Rule Constraints

| Rule                                  | Applies To                                                                                                                                                  |
| ------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------- |
| JS-021 (no lock())                    | All helpers: zero new lock() calls                                                                                                                          |
| JS-001 (no throw)                     | SubmitMarketFlattenOrder, CommitStaleCancelBatch: absorb existing try/catch                                                                                 |
| JS-002 (no return null in new code)   | `FindFollowerRuleForOrder` returns `CopyRule?` nullable struct -- returning `null` for nullable struct IS compliant; do NOT return null for reference types |
| JS-033 (no async void)                | All helpers: no async modifier                                                                                                                              |
| JS-009 (no shared mutable Dictionary) | `RegisterBeRetrySlotIfNeeded` writes to `_pendingFollowerBeSlots` -- this is ConcurrentDictionary, compliant                                                |
| ASCII-only                            | All new helper names and strings                                                                                                                            |
| CYC<=8                                | Verify each helper with lizard                                                                                                                              |

### xUnit [Fact] Test Names (append to BwaveRefactorLaneBTests.cs)

```
IsQxCancelEligible3_NullSnapshot_PassesThrough
IsQxCancelEligible3_OrderNotInSnapshot_ReturnsFalse
IsAccountFlattenable_NullAccount_ReturnsFalse
```

**Test implementation guidance**:

- `IsQxCancelEligible3_NullSnapshot_PassesThrough`: Cannot be tested without NT8 Order objects.
  Use the following structural assertion: confirm `CopyEngine` class has a static method named
  `IsQxCancelEligible3` by attempting to call it via reflection or via an internal seam.
  Add test seam if needed: `internal static bool IsQxCancelEligible3Testable(...)`.
- `IsQxCancelEligible3_OrderNotInSnapshot_ReturnsFalse`: Pass a non-empty HashSet that does NOT
  contain the order. Assert `false`.
- `IsAccountFlattenable_NullAccount_ReturnsFalse`: structural existence test only if NT8 Account
  cannot be constructed; verify method signature matches plan.

### 7-Scan Checklist

```
SCAN-01: lizard CCN -- run: lizard src/PropTraderTools/CopyEngine.cs --CCN 8
         PASS condition: zero rows output for the 4 methods in this ticket's scope:
         FlattenOneAccount, MoveStopToBreakEven, ReplaceFollowerCopyOnAtmCancel,
         CancelQxBrackets (3-param overload).
         Also verify all new helper methods score <= 8.

SCAN-02: grep lock( -- Select-String -Path src/PropTraderTools/CopyEngine.cs -Pattern "lock\s*\("
         PASS condition: zero matches.

SCAN-03: grep "async void" -- Select-String -Path src/PropTraderTools/CopyEngine.cs -Pattern "async\s+void"
         PASS condition: zero matches.

SCAN-04: grep "return null" in NEW helpers only.
         PASS condition: zero matches in newly added code.

SCAN-05: dotnet build --no-incremental from C:\WSGTA\ptt-lane-b\
         PASS condition: zero errors, zero warnings.

SCAN-06: ASCII-only check on CopyEngine.cs.
         PASS condition: Count = 0 bytes > 127.

SCAN-07: dotnet test --no-build from C:\WSGTA\ptt-lane-b\
         PASS condition: all tests in BwaveRefactorLaneBTests.cs pass, zero failures.
```

### NT8 Constraints

- `SubmitMarketFlattenOrder`: preserve `DateTime.MaxValue` (NOT `DateTime.Now`) and `null` as the
  last two args to `CreateOrder`. Order name must be `"PTT-Flatten"` (PTT- prefix required).
- `CommitStaleCancelBatch`: `acc.Cancel(stale.ToArray())` is the correct AddOnBase pattern.
  Do NOT use `acc.Change()` here.
- `FindFollowerRuleForOrder`: reads `_rules` which is the CopyEngine's rule list. No thread safety
  concern -- this runs on the NT8 account bg thread, same as `ReplaceFollowerCopyOnAtmCancel`.

### Acceptance Criteria

- [ ] Lizard reports CCN<=8 for all 4 parent methods and all new helpers.
- [ ] SCAN-02 through SCAN-06 return zero.
- [ ] SCAN-07: all 3 new [Fact] tests pass plus all T1 tests continue to pass.
- [ ] Zero behavior change confirmed by code review of diff.
- [ ] No public or internal signature changed.

---

## Ticket 3 (T3) -- Tier C: CCN 13-15 (5 methods)

### Spec Requirement IDs

- BWAVE-REFACTOR-LaneB-T3
- Prerequisite: T2 must pass all 7 scans before starting T3.
- Targets: TryReplacePttBeBrackets CCN 14-><=8, CancelQxBrackets 2-param CCN 14-><=8,
  TryFirePositionState CCN 13-><=8, CountLeaderTargets CCN 13-><=8,
  ResubmitTargetAfterCascade CCN 13-><=8

### Target Method Signatures (exact, from CopyEngine.cs)

```csharp
private void TryReplacePttBeBrackets(Order cancelledStop)                                    // L4055
internal void CancelQxBrackets(Account acc, NinjaTrader.Cbi.Instrument instr)               // L911
private void TryFirePositionState(OrderEventArgs e)                                          // L3796
private int CountLeaderTargets(Instrument instrument)                                         // L5315
private void ResubmitTargetAfterCascade(Account acc, Order stpOrder,                        // L2907
    double targetPrice, Order leaderOrder, string suffix)
```

### Extraction Instructions

#### 1. TryReplacePttBeBrackets (L4055-L4126, CCN=14 -> target <=8)

The method has 5 top-level guards (L4057-4094) plus attempt-count guard (L4097-4108) and slot registration (L4115).

**Extract A**: `IsBeBracketRecoveryEligible`

- **Signature**: `private bool IsBeBracketRecoveryEligible(Order cancelledStop)`
- **Visibility**: `private` (calls `IsFollowerAccount`, `IsFlat`, `FindPosition`, reads `_qxCancelInProgress`)
- **Absorbs**: Guards 1-4 (L4057-4066): `cancelledStop?.Account == null || cancelledStop.Instrument == null(1)`,
  `!IsFollowerAccount(cancelledStop.Account)(2)`,
  `IsFlat(FindPosition(cancelledStop.Account, cancelledStop.Instrument))(3)`,
  `_qxCancelInProgress.ContainsKey(cancelledStop.Account.Name)(4)`.
  Returns `false` if any guard fires, `true` otherwise.
- **Expected CCN**: <= 5

**Extract B**: `HasActiveQxOrders`

- **Signature**: `private bool HasActiveQxOrders(Account acc, Instrument instr)`
- **Visibility**: `private` (reads `acc.Orders`)
- **Absorbs**: Guard 3c (L4074-4093): the `.Any(o => o.Name.StartsWith("PTT-QX-") && (Working||Submitted) && instr match)` LINQ predicate.
  Returns `true` if any such order exists (meaning QX is active, skip recovery).
  The `NinjaTrader.Code.Output.Process` log call at L4087-4093 stays inside this helper.
- **Expected CCN**: <= 4

**Parent residual**: `IsBeBracketRecoveryEligible(1) + HasActiveQxOrders(1) + prevAttempts>=5(1)

- counter increment(0) + TryAdd(1) + QueueBeRetryFallback(0) = CCN<=5`

---

#### 2. CancelQxBrackets 2-param overload (L911-L941, CCN=14 -> target <=8)

This is `internal void CancelQxBrackets(Account acc, NinjaTrader.Cbi.Instrument instr)` (no snapshot param).

**Extract A**: `IsQxCancelEligible2`

- **Signature**: `private static bool IsQxCancelEligible2(Order o, Instrument instr)`
- **Visibility**: `private static` (pure predicate, no instance fields)
- **Absorbs**: The 5-term stateOk OR compound, the instrument null+FullName check, and
  `IsQxCancelCandidate(o)` call. Returns `true` only if: state is OK AND instrument matches AND
  `IsQxCancelCandidate(o)`.
- **Expected CCN**: <= 7

**Extract B**: `CommitQxCancelBatch`

- **Signature**: `private void CommitQxCancelBatch(Account acc, System.Collections.Generic.List<Order> stale)`
- **Visibility**: `private`
- **Absorbs**: The `stale.RemoveAll` race guard and the `try { acc.Cancel(stale.ToArray()); } catch { }` block.
- **Expected CCN**: <= 2

**Consolidation note**: If `CommitQxCancelBatch` body is IDENTICAL to `CommitStaleCancelBatch` (T2),
the engineer may consolidate them into `CommitCancelBatch(Account acc, List<Order> stale)` and
update both callers. If consolidating, T2's `CancelQxBrackets 3-param` caller must also be updated.

**Parent residual**: `null guard(1) + foreach(1) + IsQxCancelEligible2(1) + stale.Count==0(1)

- CommitQxCancelBatch(0) = CCN<=4`

---

#### 3. TryFirePositionState (L3796-L3844, CCN=13 -> target <=8)

The method has a state filter (1), instrument null (1), Interlocked CAS (1), prior==newVal early return (1),
and a `!hasPos` block with foreach+break+isLeaderAcct check (3 branches).

**Extract A**: `IsPositionStateTriggerState`

- **Signature**: `private static bool IsPositionStateTriggerState(OrderState s)`
- **Visibility**: `private static` (pure predicate)
- **Absorbs**: `s != OrderState.Filled && s != OrderState.PartFilled`. Returns `true` if the state
  does NOT trigger position state (i.e., should early-return). Returns `false` if state should fire.
  Alternatively (cleaner for the parent): the method returns `true` when the order state IS a valid
  trigger (Filled or PartFilled). Engineer chooses the convention that makes the parent guard natural.
- **Expected CCN**: <= 2

**Extract B**: `TryClearLeaderDirectionOnFlat`

- **Signature**: `private void TryClearLeaderDirectionOnFlat(Account acc, string instrFullName)`
- **Visibility**: `private` (accesses `_rules`, `_lastLeaderDirection`, `ClearLiveEntryForInstrument`)
- **Absorbs**: The `!hasPos` block (L3824-3839): foreach `_rules`, check `acc.Name == r.MasterAccount?.Name`,
  set `isLeaderAcct = true; break`. Then `if (isLeaderAcct) { _lastLeaderDirection.TryRemove; ClearLiveEntryForInstrument; }`.
- **Expected CCN**: <= 4

**Parent residual**: `IsPositionStateTriggerState(1) + instrument null(1) + Interlocked CAS(1)

- prior==newVal early return(1) + TryClearLeaderDirectionOnFlat(0) + event invoke(0) = CCN<=5`

**Test seam**: Add immediately below `TryFirePositionState`:

```csharp
internal static bool IsPositionStateTriggerStateTestable(OrderState s)
    => IsPositionStateTriggerState(s);
```

---

#### 4. CountLeaderTargets (L5315-L5342, CCN=13 -> target <=8)

The method has `rule null(1)`, `leader null(2)`, `foreach(3)`, `o==null continue(4)`,
`!stateOk||!instrOk||type(5)`, and a 4-part `isTarget` compound (counts as 4 by Lizard).

**Extract A**: `IsNativeLeaderTarget`

- **Signature**: `private static bool IsNativeLeaderTarget(Order o, string instrFullName)`
- **Visibility**: `private static` (pure predicate)
- **Absorbs**: The combined `stateOk`, `instrOk`, `OrderType.Limit` type check, and the 4-part
  `isTarget` compound:
  `o.OrderState == Working(1)`, `o.Instrument != null && FullName match(2)`,
  `o.OrderType == Limit(3)`, `!string.IsNullOrEmpty(o.Name) && Length>=7 && StartsWith("Target") && IsDigit && !=0(4 parts -> counts as 4 by Lizard)`.
  Returns `true` only if all conditions pass.
- **Expected CCN**: <= 7

**Parent residual**: `rule null(1) + leader null(1) + foreach(1) + o null continue(1)

- IsNativeLeaderTarget(1) = CCN<=5`

**Test seam**: Add immediately below `CountLeaderTargets`:

```csharp
internal static bool IsNativeLeaderTargetTestable(Order o, string instrFullName)
    => IsNativeLeaderTarget(o, instrFullName);
```

---

#### 5. ResubmitTargetAfterCascade (L2907-L2973, CCN=13 -> target <=8)

The method has a Block A-Prime foreach+if+try/catch cancel loop and a Block B CreateOrder+Submit try/catch.

**Extract A**: `CancelStaleTargetDrag`

- **Signature**: `private void CancelStaleTargetDrag(Account acc, Order stpOrder, string tgtDragName)`
- **Visibility**: `private` (accesses `StatusUpdate`)
- **Absorbs**: Block A-Prime (L2919-2939): foreach `acc.Orders.ToList()`, if `o.OrderState == Working
&& o.Name == tgtDragName && o.Instrument?.FullName == stpOrder.Instrument?.FullName`,
  then `try { acc.Cancel(new Order[]{o}); } catch { StatusUpdate }`.
- **Expected CCN**: <= 4

**Extract B**: `CreateAndSubmitCascadeTarget`

- **Signature**: `private void CreateAndSubmitCascadeTarget(Account acc, Order stpOrder, double targetPrice, string tgtDragName, Order leaderOrder)`
- **Visibility**: `private` (accesses `StatusUpdate`)
- **Absorbs**: Block B (L2943-2972): try/catch `acc.CreateOrder(Limit)`, null-guard `newTarget`,
  `acc.Submit`, `StatusUpdate`, catch.
  Preserve: `leaderOrder.Quantity` for qty (DW-B142-QTY-DESYNC-01), `NinjaTrader.Core.Globals.MaxDate`,
  `(NinjaTrader.Cbi.CustomOrder)null`.
- **Expected CCN**: <= 3

**Parent residual**: `TryParseStopSuffix(1) + tgtDragName local var(0)

- CancelStaleTargetDrag(0) + CreateAndSubmitCascadeTarget(0) = CCN<=2`

---

### New Helper Signatures for T3

```csharp
// TryReplacePttBeBrackets helpers
private bool IsBeBracketRecoveryEligible(Order cancelledStop)
private bool HasActiveQxOrders(Account acc, Instrument instr)

// CancelQxBrackets 2-param helpers
private static bool IsQxCancelEligible2(Order o, Instrument instr)
private void CommitQxCancelBatch(Account acc, System.Collections.Generic.List<Order> stale)

// TryFirePositionState helpers
private static bool IsPositionStateTriggerState(OrderState s)
private void TryClearLeaderDirectionOnFlat(Account acc, string instrFullName)
internal static bool IsPositionStateTriggerStateTestable(OrderState s)

// CountLeaderTargets helpers
private static bool IsNativeLeaderTarget(Order o, string instrFullName)
internal static bool IsNativeLeaderTargetTestable(Order o, string instrFullName)

// ResubmitTargetAfterCascade helpers
private void CancelStaleTargetDrag(Account acc, Order stpOrder, string tgtDragName)
private void CreateAndSubmitCascadeTarget(Account acc, Order stpOrder, double targetPrice,
    string tgtDragName, Order leaderOrder)
```

### JS Rule Constraints

| Rule                                | Applies To                                                                          |
| ----------------------------------- | ----------------------------------------------------------------------------------- |
| JS-021 (no lock())                  | All helpers: zero new lock() calls                                                  |
| JS-001 (no throw)                   | CancelStaleTargetDrag, CreateAndSubmitCascadeTarget: absorb existing try/catch only |
| JS-002 (no return null in new code) | All helpers return bool or void; no new null returns                                |
| JS-033 (no async void)              | All helpers: no async modifier                                                      |
| ASCII-only                          | All new helper names and strings                                                    |
| CYC<=8                              | Verify each helper with lizard                                                      |

### xUnit [Fact] Test Names (append to BwaveRefactorLaneBTests.cs)

```
IsPositionStateTriggerState_Filled_ReturnsFalse
IsPositionStateTriggerState_Cancelled_ReturnsTrue
IsNativeLeaderTarget_NullOrder_ReturnsFalse
IsQxCancelEligible2_NullInstrument_ReturnsFalse
```

**Test implementation guidance**:

- `IsPositionStateTriggerState_Filled_ReturnsFalse`: If the helper returns `true` for trigger states
  (Filled/PartFilled), then call `IsPositionStateTriggerStateTestable(OrderState.Filled)` and
  assert `true`. If it returns `true` for NON-trigger states, assert `false`.
  Engineer: pick the convention that reads naturally in the parent guard and document it in a comment.
- `IsPositionStateTriggerState_Cancelled_ReturnsTrue`: Validates that Cancelled is NOT a trigger state.
- `IsNativeLeaderTarget_NullOrder_ReturnsFalse`: `IsNativeLeaderTargetTestable(null, "NQ 09-26")` -> assert `false`.
- `IsQxCancelEligible2_NullInstrument_ReturnsFalse`: structural test; add seam `IsQxCancelEligible2Testable` if needed.

### 7-Scan Checklist

```
SCAN-01: lizard CCN -- run: lizard src/PropTraderTools/CopyEngine.cs --CCN 8
         PASS condition: zero rows output for the 5 methods in this ticket's scope:
         TryReplacePttBeBrackets, CancelQxBrackets (2-param), TryFirePositionState,
         CountLeaderTargets, ResubmitTargetAfterCascade.
         Also verify all new helper methods score <= 8.

SCAN-02: grep lock( -- PASS condition: zero matches.

SCAN-03: grep "async void" -- PASS condition: zero matches.

SCAN-04: grep "return null" in NEW helpers -- PASS condition: zero matches in new code.

SCAN-05: dotnet build --no-incremental -- PASS condition: zero errors, zero warnings.

SCAN-06: ASCII-only check -- PASS condition: Count = 0 bytes > 127.

SCAN-07: dotnet test --no-build -- PASS condition: all T1+T2+T3 tests pass, zero failures.
```

### NT8 Constraints

- `CreateAndSubmitCascadeTarget`: preserve `NinjaTrader.Core.Globals.MaxDate` and
  `(NinjaTrader.Cbi.CustomOrder)null` as arg11/arg12.
- `HasActiveQxOrders`: `acc.Orders.ToList()` is the safe snapshot pattern (NT8 Orders not thread-safe for direct iteration).
- `IsNativeLeaderTarget`: static -- no NT8 API calls. Pure string/enum predicate.

### Acceptance Criteria

- [ ] Lizard reports CCN<=8 for all 5 parent methods and all new helpers.
- [ ] SCAN-02 through SCAN-06 return zero.
- [ ] SCAN-07: all 4 new [Fact] tests pass plus all T1+T2 tests continue to pass.
- [ ] Zero behavior change confirmed by code review of diff.
- [ ] No public or internal signature changed.

---

## Ticket 4 (T4) -- Tier D: CCN 10-12 (6 methods)

### Spec Requirement IDs

- BWAVE-REFACTOR-LaneB-T4
- Prerequisite: T3 must pass all 7 scans before starting T4.
- Targets: OnOrderUpdate CCN 12-><=8, CancelAllAccountOrders CCN 12-><=8,
  BuildQxSnapshot CCN 11-><=8, DrainThenDispatch CCN 11-><=8,
  FindFollowerBracketOrder IEnumerable overload CCN 11-><=8, MatchesLeaderName CCN 11-><=8

### Target Method Signatures (exact, from CopyEngine.cs)

```csharp
private void OnOrderUpdate(object sender, OrderEventArgs e)                                  // L1379
internal void CancelAllAccountOrders(Account acc, NinjaTrader.Cbi.Instrument instr)         // L1049
internal static System.Collections.Generic.HashSet<NinjaTrader.Cbi.Order> BuildQxSnapshot( // L952
    NinjaTrader.Cbi.Account acc, NinjaTrader.Cbi.Instrument instr)
private void DrainThenDispatch(Account follower, Instrument instrument,                      // L6516
    int qty, double price, OrderAction action, OrderType orderType)
private Order? FindFollowerBracketOrder(IEnumerable<Order> orders,                          // L3520
    string? fromEntrySignalName, bool isStop, string? leaderName = null)
private static bool MatchesLeaderName(Order order, string? leaderName, bool isStop)         // L3575
```

### Extraction Instructions

#### 1. OnOrderUpdate (L1379-L1486, CCN=12 -> target <=8)

The method already delegates heavily to extracted helpers. The remaining excess branches come from
the drain-state handling block (L1425-1435): `Cancelled||Rejected -> if ContainsKey -> OnDrainCancelAck`
and `else if Filled -> AbortDrainOnFill`.

**Extract A**: `HandleDrainTerminalState`

- **Signature**: `private void HandleDrainTerminalState(Order order)`
- **Visibility**: `private` (calls `OnDrainCancelAck`, `AbortDrainOnFill`, reads `_pendingDispatchDrains`)
- **Absorbs**: Lines L1425-1435:
  `if (order.OrderState == Cancelled || order.OrderState == Rejected)
{ if (_pendingDispatchDrains.ContainsKey(order.Account.Name)) OnDrainCancelAck(order.Account.Name); }
else if (order.OrderState == Filled) { AbortDrainOnFill(order.Account.Name); }`
- **Expected CCN**: <= 4

**Parent residual**: all existing pre-gate helper calls (0) + `HandleDrainTerminalState(1)`

- `TryDrainWatchdog(0)` + `!_isCopyEnabled(1)` + `FindMatchingRule(1)` + `null check(1)`
- `!Enabled(1)` + `TryFirePositionState(0)` + `TryMirrorOrderUpdate(0)`
- `TryCancelFollowerEntries(1)` + `TryDispatchLeaderFlat(1)` + `TryHandleDrag(1)`
- `DispatchCopy(0)` = CCN<=8. PASS.

---

#### 2. CancelAllAccountOrders (L1049-L1079, CCN=12 -> target <=8)

The method has null guard (1), foreach (2), a 4-term stateOk OR (3), and instrument name filter (4).
Lizard counts the 4-term OR as 4 separate branches, giving CCN=12.

**Extract A**: `IsCancelAllStateOk`

- **Signature**: `private static bool IsCancelAllStateOk(OrderState s)`
- **Visibility**: `private static` (pure predicate)
- **Absorbs**: `Working || Initialized || Submitted || Accepted` 4-term OR. Returns `true` if any match.
- **Expected CCN**: <= 4

**Parent residual**: `null guard(1) + foreach(1) + IsCancelAllStateOk(1) + instrument filter(1)

- RemoveAll terminal race guard(1) = CCN<=5`

**Test seam**: Add immediately below `CancelAllAccountOrders`:

```csharp
internal static bool IsCancelAllStateOkTestable(OrderState s) => IsCancelAllStateOk(s);
```

---

#### 3. BuildQxSnapshot (L952-L980, CCN=11 -> target <=8)

The method has null guard (1), foreach (2), 5-term stateOk OR (3..7), instrument filter, `IsQxCancelCandidate`.
Lizard counts each OR term as +1, giving CCN=11.

**Extract A**: `IsQxSnapshotStateOk`

- **Signature**: `private static bool IsQxSnapshotStateOk(OrderState s)`
- **Visibility**: `private static` (pure predicate)
- **Absorbs**: `Working || Initialized || Accepted || Submitted || TriggerPending` 5-term OR.
  Returns `true` if any match.
- **Expected CCN**: <= 5

**Parent residual**: `null guard(1) + foreach(1) + IsQxSnapshotStateOk(1) + instrument filter(1)

- IsQxCancelCandidate(1) = CCN<=5`

**Test seam**: Add immediately below `BuildQxSnapshot`:

```csharp
internal static bool IsQxSnapshotStateOkTestable(OrderState s) => IsQxSnapshotStateOk(s);
```

---

#### 4. DrainThenDispatch (L6516-L6571, CCN=11 -> target <=8)

The method has null guard (1), LINQ filter (2 branches for Where conditions), `!entryCandidates.Any()` (3),
`TryAdd` check (4), foreach cancel loop (5) = CCN=5 by strict McCabe, but Lizard counts LINQ
predicates as branches, pushing to 11. The key extraction removes the LINQ predicate chain.

**Extract A**: `IssueDrainCancels`

- **Signature**: `private int IssueDrainCancels(Account acc, Instrument instrument)`
- **Visibility**: `private` (accesses `ActiveOrders`, `_pendingDispatchDrains`, `_drainOwnedOrderIds`,
  `SubmitEntryDirect` -- instance methods/fields)
- **NOTE**: Do NOT move the `_pendingDispatchDrains.TryAdd` or payload construction into this helper.
  This helper only handles: building the `entryCandidates` LINQ filter, the `!entryCandidates.Any()`
  fast-path (calling `SubmitEntryDirect` and return 0), and the foreach cancel loop
  (`_drainOwnedOrderIds.TryAdd` + `follower.Cancel`). Returns the count of cancels issued.
- **Absorbs**: L6529-6570 minus the payload/TryAdd section:
  The `entryCandidates` LINQ `.Where(...)` filter and `.ToList()`, the `if (!entryCandidates.Any())`
  direct-submit path, the foreach cancel loop including `_drainOwnedOrderIds.TryAdd` and
  `follower.Cancel`, and the `Output.Process` log. Returns `entryCandidates.Count`.
- **Expected CCN**: <= 5

**Parent residual**: `_pendingDispatchDrains upsert (TryAdd check)(1)` + `null guard(1)` +
`IssueDrainCancels(0) + cancelCount==0 immediate-dispatch check(1) = CCN<=4`

**IMPORTANT**: If `IssueDrainCancels` absorbs the LINQ filter, the payload construction (L6544-6560)
that depends on `entryCandidates` must stay in the parent OR be restructured so the payload is built
before calling `IssueDrainCancels`. Recommended: keep payload construction in the parent; pass the
`entryCandidates` list as a parameter to `IssueDrainCancels`. Signature revision:
`private int IssueDrainCancels(Account acc, System.Collections.Generic.List<Order> entryCandidates)`.
The parent retains the LINQ filter and null guard, calls the helper with the pre-filtered list.

---

#### 5. FindFollowerBracketOrder IEnumerable overload (L3520-L3553, CCN=11 -> target <=8)

The method has: foreach (1), `OrderPassesBracketGate` (1), 4-state filter compound (4), isStop (1),
StopMarket||StopLimit (1) = 9 raw, but the 4-state filter `!=Working && !=Accepted && !=Submitted && !=ChangeSubmitted`
is counted as 4 by Lizard.

**Extract A**: `MatchesBracketType`

- **Signature**: `private static bool MatchesBracketType(Order order, bool isStop)`
- **Visibility**: `private static` (pure predicate, no instance fields)
- **Absorbs**: The type-matching block (L3538-3550): `if (isStop)` -> check `StopMarket || StopLimit`;
  else -> check `Limit && !IsStopLeg(order)`. Returns `true` on match, `false` otherwise.
- **Expected CCN**: <= 3

**Parent residual**: `foreach(1) + OrderPassesBracketGate(1) + 4-state filter(4) + MatchesBracketType(1) = CCN<=7`

**Test seam**: Add immediately below `FindFollowerBracketOrder` (IEnumerable overload):

```csharp
internal static bool MatchesBracketTypeTestable(OrderType t, bool isStop)
    // Create a minimal Order-like structure or use primitives
    // NOTE: if Order cannot be constructed without NT8, stub the seam differently:
    // internal static bool MatchesBracketTypeTestable(bool isStop, OrderType orderType, bool isStopLeg)
    => MatchesBracketType(order, isStop);
```

Engineer: since `Order` requires NT8 runtime, use the primitive-param form:

```csharp
internal static bool MatchesBracketTypeTestable(bool isStop, OrderType orderType, bool isOrderStopLeg)
{
    // Inline the same logic using primitives for test isolation
    if (isStop) return orderType == OrderType.StopMarket || orderType == OrderType.StopLimit;
    return orderType == OrderType.Limit && !isOrderStopLeg;
}
```

---

#### 6. MatchesLeaderName (L3575-L3592, CCN=11 -> target <=8)

The method has leaderName null (1), exact name (2), `legSuffix` extraction ternary (counted by Lizard
as multiple branches due to `&&` inside), `!isStop && legSuffix != null && TGT name(3)`,
`isStop && legSuffix != null && STP name(4)`.

**Extract A**: `ExtractLegSuffix`

- **Signature**: `private static string ExtractLegSuffix(string leaderName)`
- **Visibility**: `private static` (pure computation, no instance fields)
- **Absorbs**: Lines L3583-3586:
  `leaderName.Length > 0 && char.IsDigit(leaderName[leaderName.Length - 1])
? leaderName[leaderName.Length - 1].ToString() : null`
  Returns the trailing digit as a string, or `null` if no trailing digit.
- **Expected CCN**: <= 2

**Parent residual**: `leaderName null(1) + exact name(1) + ExtractLegSuffix(0)

- !isStop&&legSuffix!=null&&TGT name(1) + isStop&&legSuffix!=null&&STP name(1) = CCN<=4`

---

### New Helper Signatures for T4

```csharp
// OnOrderUpdate helpers
private void HandleDrainTerminalState(Order order)

// CancelAllAccountOrders helpers
private static bool IsCancelAllStateOk(OrderState s)
internal static bool IsCancelAllStateOkTestable(OrderState s)

// BuildQxSnapshot helpers
private static bool IsQxSnapshotStateOk(OrderState s)
internal static bool IsQxSnapshotStateOkTestable(OrderState s)

// DrainThenDispatch helpers
private int IssueDrainCancels(Account acc, System.Collections.Generic.List<Order> entryCandidates)

// FindFollowerBracketOrder IEnumerable overload helpers
private static bool MatchesBracketType(Order order, bool isStop)
internal static bool MatchesBracketTypeTestable(bool isStop, OrderType orderType, bool isOrderStopLeg)

// MatchesLeaderName helpers
private static string ExtractLegSuffix(string leaderName)
```

### JS Rule Constraints

| Rule                                | Applies To                                                                                                                                                                    |
| ----------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| JS-021 (no lock())                  | All helpers: zero new lock() calls                                                                                                                                            |
| JS-001 (no throw)                   | HandleDrainTerminalState: delegates to existing helpers, no new throw                                                                                                         |
| JS-002 (no return null in new code) | ExtractLegSuffix returns string (may return null for "no trailing digit" -- this is a nullable reference, acceptable in .NET 4.8 context; or return string.Empty as sentinel) |
| JS-033 (no async void)              | All helpers: no async modifier                                                                                                                                                |
| ASCII-only                          | All new helper names and strings                                                                                                                                              |
| CYC<=8                              | Verify each helper with lizard                                                                                                                                                |

### xUnit [Fact] Test Names (append to BwaveRefactorLaneBTests.cs)

```
IsCancelAllStateOk_Working_ReturnsTrue
IsCancelAllStateOk_Filled_ReturnsFalse
IsQxSnapshotStateOk_TriggerPending_ReturnsTrue
IsQxSnapshotStateOk_Rejected_ReturnsFalse
MatchesBracketType_StopMarket_IsStop_ReturnsTrue
MatchesBracketType_Limit_IsStop_ReturnsFalse
ExtractLegSuffix_Stop1_Returns1
ExtractLegSuffix_NoDigit_ReturnsNull
```

**Test implementation guidance**:

- `IsCancelAllStateOk_Working_ReturnsTrue`: `CopyEngine.IsCancelAllStateOkTestable(OrderState.Working)` -> assert `true`.
- `IsCancelAllStateOk_Filled_ReturnsFalse`: `CopyEngine.IsCancelAllStateOkTestable(OrderState.Filled)` -> assert `false`.
- `IsQxSnapshotStateOk_TriggerPending_ReturnsTrue`: `CopyEngine.IsQxSnapshotStateOkTestable(OrderState.TriggerPending)` -> assert `true`.
- `IsQxSnapshotStateOk_Rejected_ReturnsFalse`: assert `false`.
- `MatchesBracketType_StopMarket_IsStop_ReturnsTrue`: `CopyEngine.MatchesBracketTypeTestable(true, OrderType.StopMarket, false)` -> assert `true`.
- `MatchesBracketType_Limit_IsStop_ReturnsFalse`: `CopyEngine.MatchesBracketTypeTestable(true, OrderType.Limit, false)` -> assert `false`.
- `ExtractLegSuffix_Stop1_Returns1`: Add test seam `internal static string ExtractLegSuffixTestable(string n) => ExtractLegSuffix(n)`. Call with `"Stop1"` -> assert `"1"`.
- `ExtractLegSuffix_NoDigit_ReturnsNull`: Call with `"PTT-Copy"` -> assert `null` (or `string.Empty` if that is the sentinel chosen).

### 7-Scan Checklist

```
SCAN-01: lizard CCN -- run: lizard src/PropTraderTools/CopyEngine.cs --CCN 8
         PASS condition: zero rows output for the 6 methods in this ticket's scope:
         OnOrderUpdate, CancelAllAccountOrders, BuildQxSnapshot, DrainThenDispatch,
         FindFollowerBracketOrder (IEnumerable overload), MatchesLeaderName.
         Also verify all new helper methods score <= 8.

SCAN-02: grep lock( -- PASS condition: zero matches.

SCAN-03: grep "async void" -- PASS condition: zero matches.

SCAN-04: grep "return null" in NEW helpers -- PASS condition: zero matches in new code.
         ExtractLegSuffix returning null for nullable string is permitted (not a reference-type null return).

SCAN-05: dotnet build --no-incremental -- PASS condition: zero errors, zero warnings.

SCAN-06: ASCII-only check -- PASS condition: Count = 0 bytes > 127.

SCAN-07: dotnet test --no-build -- PASS condition: all T1+T2+T3+T4 tests pass, zero failures.
```

### NT8 Constraints

- `IssueDrainCancels`: `follower.Cancel(new Order[]{e})` is the AddOnBase cancel pattern. No `acc.Change()`.
- `BuildQxSnapshot` is `internal static` -- must remain `internal static` after extraction. `IsQxSnapshotStateOk` becomes a `private static` helper.
- `OnOrderUpdate` is a NT8 event handler (subscribed at L1369). The extracted `HandleDrainTerminalState` must NOT be async.

### Acceptance Criteria

- [ ] Lizard reports CCN<=8 for all 6 parent methods and all new helpers.
- [ ] SCAN-02 through SCAN-06 return zero.
- [ ] SCAN-07: all 8 new [Fact] tests pass plus all T1+T2+T3 tests continue to pass.
- [ ] Zero behavior change confirmed by code review of diff.
- [ ] No public or internal signature changed.

---

## Ticket 5 (T5) -- Tier E: CCN = 9 (11 methods)

### Spec Requirement IDs

- BWAVE-REFACTOR-LaneB-T5
- Prerequisite: T4 must pass all 7 scans before starting T5.
- Targets: HasNakedPosition CCN 9-><=8, RuleToDto CCN 9-><=8, IsFollowerAccount CCN 9-><=8,
  AllAccounts CCN 9-><=8, CaptureLinkedTargetPrice CCN 9-><=8, MirrorClose CCN 9-><=8,
  BuildUpdatedMultipliers CCN 9-><=8, CaptureOtherLegTargetPrices CCN 9-><=8,
  HandleEntryChange CCN 9-><=8, HandleBracketChange CCN 9-><=8,
  CreateFollowerReplacementStop CCN 9-><=8

### Target Method Signatures (exact, from CopyEngine.cs)

```csharp
private static bool HasNakedPosition(Account acct)                                           // L6473
private static CopyRuleDto RuleToDto(CopyRule rule)                                          // L6197
internal bool IsFollowerAccount(Account acc)                                                 // L778
internal IEnumerable<Account> AllAccounts(Instrument instrument)                             // L5116
private double? CaptureLinkedTargetPrice(Account acc, string stopName)                      // L2778
private void MirrorClose(Order masterOrder, CopyRule rule)                                   // L2119
private static int[] BuildUpdatedMultipliers(int[] existing, int index,                     // L1348
    int value, int count)
private double[] CaptureOtherLegTargetPrices(Account acc, Order fo, string excludeSuffix)   // L2812
private void HandleEntryChange(Order leaderOrder, CopyRule rule)                             // L3736
private void HandleBracketChange(Order leaderOrder, CopyRule rule)                           // L3414
private void CreateFollowerReplacementStop(Account followerAcc, Instrument instr,            // L3348
    int qty, OrderAction stopAction, double stopPrice)
```

### Extraction Instructions

All T5 methods are CCN=9 -- one branch over limit. Each needs exactly one small extraction.

#### 1. HasNakedPosition (L6473-L6502, CCN=9 -> target <=8)

The method has two foreach loops. The second loop (L6489-6500) over `acct.Orders` with 5 branches
(stateOk 2-term OR, StopMarket||StopLimit compound, Limit check) is the excess.

**Extract A**: `IsNakedConditionMet`

- **Signature**: `private static bool IsNakedConditionMet(Account acct)`
- **Visibility**: `private static` (reads `acct.Orders`, no instance fields)
- **Absorbs**: The second foreach (L6489-6500): iterate `acct.Orders`, skip non-Working/Submitted,
  set `hasStop = true` for stop types, set `hasTarget = true` for Limit, then
  `return !hasStop && !hasTarget`.
- **Expected CCN**: <= 4

**Parent residual**: `foreach Positions(1) + hasPosition check(1) + !hasPosition return(1)

- IsNakedConditionMet call(0) = CCN<=4`

---

#### 2. RuleToDto (L6197-L6232, CCN=9 -> target <=8)

The method has 3 for-i loops and a dictionary init block. The ATM template loop (the `foreach FollowerAtmTemplates` that the plan references) is represented by the `for (int i...)` loop at L6213-6219 that calls `GetAtmMode` per follower. This adds iteration+branch count.

**Extract A**: `ExtractAtmTemplateMap`

- **Signature**: `private static string[] BuildAtmModeNames(CopyRule rule)`
- **Visibility**: `private static` (calls `AtmModeToString`, `GetAtmMode` which must be static or accessible)
- **Absorbs**: The `atmNames` array construction block (L6213-6219): `var atmNames = new string[...]; for (int i...) { string accName = ...; atmNames[i] = AtmModeToString(GetAtmMode(rule, accName)); } return atmNames`.
  Returns the built `string[]`.
- **Expected CCN**: <= 4

**Parent residual**: `followerNames for-loop(1) + mults for-loop(1) + BuildAtmModeNames call(0)

- object initializer(0) = CCN<=3`

**NOTE**: The plan names this `ExtractAtmTemplateMap` returning `Dictionary<string,string>`.
However, the actual code at L6213-6219 builds a `string[]` array, not a Dictionary.
The correct extraction returns `string[]`. Name the helper `BuildAtmModeNames` and have it return
`string[]`. Update the plan name accordingly -- the reviewer confirmed JS-009 PASS based on no
shared mutable Dictionary, which remains true with `string[]`.

---

#### 3. IsFollowerAccount (L778-L797, CCN=9 -> target <=8)

The method has: `acc null(1)`, foreach rules (1), for-i (1), `f != null && name match(1)`,
`f == null && names != null && i in range && name match(1)`. The nested for-i with 4 conditions
is the excess.

**Extract A**: `MatchesFollowerSlot`

- **Signature**: `private static bool MatchesFollowerSlot(CopyRule rule, Account acc)`
- **Visibility**: `private static` (uses only parameters, no instance fields)
- **Absorbs**: The for-i body (L3783-3795): `for (int i = 0; i < rule.FollowerAccounts.Length; i++)`,
  check `f != null && f.Name == acc.Name` return true, check
  `f == null && names != null && i < names.Length && names[i] == acc.Name` return true.
  Returns `true` if any follower slot matches, `false` if loop exhausts.
- **Expected CCN**: <= 5

**Parent residual**: `acc null(1) + foreach rules(1) + MatchesFollowerSlot(1) = CCN<=4`

---

#### 4. AllAccounts (L5116-L5163, CCN=9 -> target <=8)

The method is an iterator (`yield return`). The excess branches come from the null-slot lazy
re-resolve block (L5134-5162) inside the for-i loop.

**Extract A**: `IsFollowerForInstrument`

- **Signature**: `private static bool IsFollowerForInstrument(Account acc, CopyRule rule)`
- **Visibility**: `private static` (uses only parameters)
- **NOTE**: The iterator pattern makes simple extraction harder. Instead, extract the inner null-slot
  resolution logic into a helper that the for-i body calls.
- **Absorbs**: The inner block for a null-slot `acc` (L5134-5162): check name, check
  `_resolvedFollowers.TryGetValue`, TryAdd, log, yield. However since this is an iterator method,
  the extraction must be to a non-iterator helper.
- **REVISED APPROACH**: Extract the null-slot resolution to:
  `private Account ResolveNullFollowerSlot(CopyRule rule, int i)` (private, non-static, accesses `_resolvedFollowers`).
  Absorbs: get name from `names[i]`, `string.IsNullOrEmpty` check, `_resolvedFollowers.TryGetValue`,
  `FindFollowerAccount`, `TryAdd`, `Output.Process` log. Returns resolved `Account` or `null`.
  Parent uses `var resolved = ResolveNullFollowerSlot(rule, i); if (resolved != null) yield return resolved;`
- **Signature**: `private Account ResolveNullFollowerSlot(CopyRule rule, int i)`
- **Expected CCN**: <= 3

**Parent residual**: `rule null(1) + for-i(1) + acc!=null(1) + null-slot call(1) + resolved!=null(1)
= CCN<=6`

---

#### 5. CaptureLinkedTargetPrice (L2778-L2796, CCN=9 -> target <=8)

The method has: `TryParseStopSuffix(1)`, `foreach acc.Orders(1)`, `IsTargetOrderLive && pttName(1)`,
`else if IsTargetOrderLive && atmName(1)`, `pttPrice.HasValue(1)` = 5 raw. Lizard counts `&&` in
compound predicates as extra branches, pushing to 9.

**Extract A**: `PickBestTargetPrice`

- **Signature**: `private static double? PickBestTargetPrice(double? pttPrice, double? atmPrice)`
- **Visibility**: `private static` (pure computation, no instance fields)
- **Absorbs**: `if (pttPrice.HasValue) return pttPrice.Value; return atmPrice;` -- the return logic
  at L2793-2795.
- **Expected CCN**: <= 2

**Parent residual**: `TryParseStopSuffix(1) + foreach(1) + pttPrice assign(1) + atmPrice assign(1)

- PickBestTargetPrice(0) = CCN<=5`

---

#### 6. MirrorClose (L2119-L2158, CCN=9 -> target <=8)

The method has: `instr null(1)`, `foreach FollowerAccounts(1)`, `acc null continue(1)`,
`pos null/qty(1)`. Then inside try: `CreateOrder` call. The excess is the try/catch around
CreateOrder plus the action ternary.

**Extract A**: `MirrorCloseOneAccount`

- **Signature**: `private void MirrorCloseOneAccount(Account acc, Instrument instr)`
- **Visibility**: `private` (accesses `StatusUpdate`)
- **Absorbs**: The inner body of the foreach loop (L2126-2157): `acc null continue`, `FindPosition`,
  `pos null/qty continue`, action ternary, try `acc.CreateOrder(Market, "PTT-Mirror-Close")`,
  null-guard (NOT needed since Market orders don't return null -- but preserve the Submit call),
  `StatusUpdate`, catch.
  Note: the existing code does NOT null-check the `CreateOrder` return for this path; it just calls
  `StatusUpdate?.Invoke`. The `acc.Submit` is NOT called here -- the existing code at L2150-2151
  does NOT call Submit (unlike other paths). Preserve this exactly.
- **Expected CCN**: <= 5

**Parent residual**: `instr null(1) + foreach(1) + MirrorCloseOneAccount(0) = CCN<=3`

**NOTE on Submit**: The `MirrorClose` method at L2119-2158 does NOT call `acc.Submit`. The `acc.CreateOrder` at L2137 is the NT8 "Automated" path that does not require Submit for this specific case, or this is a pre-B57 path. Do NOT add a Submit call. Preserve the exact behavior.

---

#### 7. BuildUpdatedMultipliers (L1348-L1364, CCN=9 -> target <=8)

The method has: `len ternary(1)`, `len==0 return(1)`, for-i (1), `existing && i<len ternary(1)`,
`index in range(1)` = 5 raw. Lizard counts compound `&&` as branches, pushing to 9.

**Extract A**: `ResolveMultiplierLength`

- **Signature**: `private static int ResolveMultiplierLength(int[] existing, int count)`
- **Visibility**: `private static` (pure computation)
- **Absorbs**: `int len = count > 0 ? count : (existing != null ? existing.Length : 0);` at L1355.
  Returns the computed `len`.
- **Expected CCN**: <= 3

**Parent residual**: `ResolveMultiplierLength(0) + len==0 return(1) + for-i(1) + existing ternary(1)

- index check(1) = CCN<=5`

---

#### 8. CaptureOtherLegTargetPrices (L2812-L2835, CCN=9 -> target <=8)

The method has: `fo.Name.StartsWith("Stop")(1)`, `foreach orders(1)`, `for i(1)`, `s==excludeSuffix(1)`,
PTT preferred `if(1)`, ATM fallback `else if(1)` = 6 raw. Lizard counts compound `&&` in `else if`
as branches pushing to 9.

**Extract A**: `UpdateLegTargetPrice`

- **Signature**: `private static void UpdateLegTargetPrice(double[] prices, int i, Order o, string excludeSuffix)`
- **Visibility**: `private static` (uses only parameters and local computation)
- **Absorbs**: The inner for-i body (L3821-3831): `string s = i.ToString(); if (s == excludeSuffix) continue;
if (IsTargetOrderLive(o) && o.Name == "PTT-TGT-Drag-" + s) prices[i-1] = o.LimitPrice;
else if (IsTargetOrderLive(o) && o.Name == "Target" + s && prices[i-1] == 0) prices[i-1] = o.LimitPrice;`
- **Expected CCN**: <= 4

**Parent residual**: `StartsWith guard(1) + foreach(1) + for-i(1) + UpdateLegTargetPrice call(0) = CCN<=4`

---

#### 9. HandleEntryChange (L3736-L3771, CCN=9 -> target <=8)

The method has: `instrument null(1)`, `tickSize ternary(1)`, `foreach acc(1)`, `acc null(1)`,
`fo null(1)`, `tickSize>0 && Math.Abs(1)` = 6 raw. Lizard counts the `&&` in the delta guard
as +1, and the ternary as +1, pushing to 9.

**Extract A**: `IsPriceDeltaSignificant`

- **Signature**: `private static bool IsPriceDeltaSignificant(double newPrice, double currentPrice, double tickSize)`
- **Visibility**: `private static` (pure computation)
- **Absorbs**: `tickSize > 0 && Math.Abs(newPrice - currentPrice) < tickSize` at L3763.
  Returns `true` if the delta is TOO SMALL to act on (i.e., should skip), `false` if significant.
- **Expected CCN**: <= 2

**Parent residual**: `instrument null(1) + tickSize ternary(1) + foreach(1) + acc null(1)

- fo null(1) + IsPriceDeltaSignificant(1) + DrainThenDispatch(0) = CCN<=7`

---

#### 10. HandleBracketChange (L3414-L3447, CCN=9 -> target <=8)

The method has: `isStop(1)`, `instrument null(2)`, `tickSize ternary(3)`, `rawPrice ternary(4)`,
`newPrice ternary(5)`, `_diagnosticMode(6)`, `foreach acc(7)`, `acc null(8)` = CCN=8 currently.
Wait -- plan says CCN=9. Reading actual source: `isStop = IsStopLeg(1)`, instr null(2),
tickSize ?? ternary(3), rawPrice ternary(4), `newPrice = tickSize>0 ?(5)`, `_diagnosticMode(6)`,
foreach(7), acc null(8) = 8. Lizard must count the `&&` in the diagnostic log compound as +1 = 9.

**Extract A**: `RoundToTick`

- **Signature**: `private static double RoundToTick(double rawPrice, double tickSize)`
- **Visibility**: `private static` (pure computation)
- **Absorbs**: `tickSize > 0 ? Math.Round(rawPrice / tickSize) * tickSize : rawPrice` at L3425.
  Returns the rounded price.
- **Expected CCN**: <= 2

**Parent residual**: `isStop(1) + instr null(2) + tickSize ??(3) + rawPrice ternary(4)

- RoundToTick call(0) + _diagnosticMode(5) + foreach(6) + acc null(7) = CCN<=7`

---

#### 11. CreateFollowerReplacementStop (L3348-L3393, CCN=9 -> target <=8)

The method has: `stopPrice<=0(1)`, `try(2)`, `newStop null(3)`, `catch(4)` = 4 raw. Lizard must
count compound conditions inside try or the null guard differently, or the `??` in StatusUpdate
`?.Invoke` counts as +1 for each of the 4 invocations (5 occurrences), pushing the total.

**Extract A**: `SubmitReplacementStopOrder`

- **Signature**: `private void SubmitReplacementStopOrder(Account followerAcc, Instrument instr, int qty, OrderAction stopAction, double stopPrice)`
- **Visibility**: `private` (accesses `StatusUpdate`)
- **Absorbs**: The try/catch block (L3361-3392): `try { CreateOrder(StopMarket, "PTT-STP-Drag", ...)`,
  null-guard on `newStop`, `followerAcc.Submit`, `StatusUpdate`, catch.
- **Expected CCN**: <= 4

**Parent residual**: `stopPrice<=0(1) + StatusUpdate + SubmitReplacementStopOrder(0) = CCN<=2`

---

### New Helper Signatures for T5

```csharp
// HasNakedPosition helpers
private static bool IsNakedConditionMet(Account acct)

// RuleToDto helpers
private static string[] BuildAtmModeNames(CopyRule rule)

// IsFollowerAccount helpers
private static bool MatchesFollowerSlot(CopyRule rule, Account acc)

// AllAccounts helpers
private Account ResolveNullFollowerSlot(CopyRule rule, int i)

// CaptureLinkedTargetPrice helpers
private static double? PickBestTargetPrice(double? pttPrice, double? atmPrice)

// MirrorClose helpers
private void MirrorCloseOneAccount(Account acc, Instrument instr)

// BuildUpdatedMultipliers helpers
private static int ResolveMultiplierLength(int[] existing, int count)

// CaptureOtherLegTargetPrices helpers
private static void UpdateLegTargetPrice(double[] prices, int i, Order o, string excludeSuffix)

// HandleEntryChange helpers
private static bool IsPriceDeltaSignificant(double newPrice, double currentPrice, double tickSize)

// HandleBracketChange helpers
private static double RoundToTick(double rawPrice, double tickSize)

// CreateFollowerReplacementStop helpers
private void SubmitReplacementStopOrder(Account followerAcc, Instrument instr, int qty,
    OrderAction stopAction, double stopPrice)
```

### JS Rule Constraints

| Rule                                | Applies To                                                                                                                                                                                                                |
| ----------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| JS-021 (no lock())                  | All helpers: zero new lock() calls                                                                                                                                                                                        |
| JS-001 (no throw)                   | SubmitReplacementStopOrder: absorb existing try/catch; no new throw                                                                                                                                                       |
| JS-002 (no return null in new code) | `ResolveNullFollowerSlot` returns Account (reference type); may return null as "not found" -- this is an existing NT8 pattern. Acceptable per plan's grandfathering of pre-existing null returns from FindBePosition etc. |
| JS-033 (no async void)              | All helpers: no async modifier                                                                                                                                                                                            |
| ASCII-only                          | All new helper names and strings                                                                                                                                                                                          |
| CYC<=8                              | Verify each helper with lizard                                                                                                                                                                                            |

### xUnit [Fact] Test Names (append to BwaveRefactorLaneBTests.cs)

```
ResolveMultiplierLength_CountZeroNullExisting_ReturnsZero
ResolveMultiplierLength_CountPositive_ReturnsCount
IsPriceDeltaSignificant_ZeroTickSize_ReturnsFalse
IsPriceDeltaSignificant_SmallDelta_ReturnsTrue
RoundToTick_ZeroTickSize_ReturnsRawPrice
RoundToTick_PositiveTickSize_ReturnsRoundedPrice
PickBestTargetPrice_PttHasValue_ReturnsPtt
PickBestTargetPrice_PttNull_ReturnsAtm
```

**Test implementation guidance** (all helpers are `private static`, add seams as needed):

- `ResolveMultiplierLength_CountZeroNullExisting_ReturnsZero`:
  Add seam: `internal static int ResolveMultiplierLengthTestable(int[] e, int c) => ResolveMultiplierLength(e, c)`.
  Call with `(null, 0)` -> assert `0`.
- `ResolveMultiplierLength_CountPositive_ReturnsCount`: Call with `(null, 3)` -> assert `3`.
- `IsPriceDeltaSignificant_ZeroTickSize_ReturnsFalse`:
  Add seam: `internal static bool IsPriceDeltaSignificantTestable(double n, double c, double t) => IsPriceDeltaSignificant(n, c, t)`.
  Call with `(100.0, 99.0, 0.0)` -> assert `false` (zero tick = not significant = don't skip).
- `IsPriceDeltaSignificant_SmallDelta_ReturnsTrue`: Call with `(100.0, 100.0, 0.25)` -> assert `true` (delta=0 < tick = skip).
- `RoundToTick_ZeroTickSize_ReturnsRawPrice`:
  Add seam: `internal static double RoundToTickTestable(double raw, double tick) => RoundToTick(raw, tick)`.
  Call with `(100.123, 0.0)` -> assert `100.123`.
- `RoundToTick_PositiveTickSize_ReturnsRoundedPrice`: Call with `(100.1, 0.25)` -> assert `100.0` (or `100.25` depending on rounding direction -- use the exact value the formula produces).
- `PickBestTargetPrice_PttHasValue_ReturnsPtt`:
  Add seam: `internal static double? PickBestTargetPriceTestable(double? p, double? a) => PickBestTargetPrice(p, a)`.
  Call with `(100.0, 99.0)` -> assert `100.0`.
- `PickBestTargetPrice_PttNull_ReturnsAtm`: Call with `(null, 99.0)` -> assert `99.0`.

### 7-Scan Checklist

```
SCAN-01: lizard CCN -- run: lizard src/PropTraderTools/CopyEngine.cs --CCN 8
         PASS condition: ZERO rows output for the ENTIRE file.
         This is the final ticket -- all 32 methods must be at CCN<=8.
         Also verify all new helper methods score <= 8.
         Full command: lizard src/PropTraderTools/CopyEngine.cs --CCN 8
         Expected output: empty (no methods exceed threshold).

SCAN-02: grep lock( -- Select-String -Path src/PropTraderTools/CopyEngine.cs -Pattern "lock\s*\("
         PASS condition: zero matches.

SCAN-03: grep "async void" -- Select-String -Path src/PropTraderTools/CopyEngine.cs -Pattern "async\s+void"
         PASS condition: zero matches.

SCAN-04: grep "return null" in NEW helpers -- PASS condition: zero matches in newly added code.

SCAN-05: dotnet build --no-incremental from C:\WSGTA\ptt-lane-b\
         PASS condition: zero errors, zero warnings.

SCAN-06: ASCII-only check on CopyEngine.cs.
         PASS condition: Count = 0 bytes > 127.

SCAN-07: dotnet test --no-build from C:\WSGTA\ptt-lane-b\
         PASS condition: all T1+T2+T3+T4+T5 tests pass, zero failures.
```

### NT8 Constraints

- `MirrorCloseOneAccount`: Order name must be `"PTT-Mirror-Close"` (PTT- prefix required). Do NOT add `acc.Submit` -- the existing `MirrorClose` does NOT call Submit; this preserves that behavior exactly.
- `SubmitReplacementStopOrder`: Order name must be `"PTT-STP-Drag"` (PTT- prefix, no suffix for this overload -- this is the original `CreateFollowerReplacementStop` which uses the unsuffixed name).
- `ResolveNullFollowerSlot`: accesses `_resolvedFollowers` (ConcurrentDictionary) -- `TryGetValue` and `TryAdd` are lock-free (JS-021 compliant).
- `UpdateLegTargetPrice`: calls `IsTargetOrderLive(o)` which is a private instance method. Mark the helper `private` (non-static) since it calls an instance method.
  Revised signature: `private void UpdateLegTargetPrice(double[] prices, int i, Order o, string excludeSuffix)`.

### Acceptance Criteria

- [ ] Lizard produces ZERO output for the entire CopyEngine.cs (all 32 original methods and all new helpers at CCN<=8).
- [ ] SCAN-02 through SCAN-06 return zero.
- [ ] SCAN-07: all 8 new [Fact] tests pass plus all T1+T2+T3+T4 tests continue to pass.
- [ ] Zero behavior change confirmed by code review of diff.
- [ ] No public or internal signature changed.
- [ ] `powershell -File scripts\ptt-sync-and-verify.ps1` completes with zero MISMATCH lines.
- [ ] F5 in NinjaTrader 8 compiles green after sync.

---

## Post-T5 Verification Gate

After T5 passes all 7 scans, the engineer runs the following final verification:

```powershell
# 1. Full CCN gate (must produce zero output)
lizard src/PropTraderTools/CopyEngine.cs --CCN 8

# 2. NT8 sync + MD5 verify
powershell -File scripts\ptt-sync-and-verify.ps1

# 3. Full test run
dotnet test --no-build

# 4. Final build
dotnet build --no-incremental
```

All four commands must succeed with zero errors before the engineer reports VERIFY_PASS to the
ptt-verifier.

---

## Name Collision Registry

All private helper names introduced in this epic. Engineer MUST verify each is absent before adding:

| Ticket | Helper Name                     |
| ------ | ------------------------------- |
| T1     | IsImmediateBeEligible           |
| T1     | FireImmediateBe                 |
| T1     | CancelLiveCollateralStop        |
| T1     | CancelLiveCollateralTarget      |
| T1     | CreateAndSubmitCollateralStop   |
| T1     | CreateAndSubmitCollateralTarget |
| T1     | IsBeTargetStateOk               |
| T1     | ClassifyBeTarget                |
| T1     | IsCleanupAtmEligible            |
| T1     | TryCancelNativeAtmTarget        |
| T1     | EvaluateCleanupRemoval          |
| T1     | IsAtmTargetSyncEligible         |
| T1     | CancelBlockAAtmTarget           |
| T1     | BlockBCreateAtmTarget           |
| T1     | HandleAtmStopSync               |
| T1     | HandleAtmTargetSync             |
| T1     | HandleNonAtmSync                |
| T2     | IsAccountFlattenable            |
| T2     | SubmitMarketFlattenOrder        |
| T2     | LogDiagOrderCount               |
| T2     | RegisterBeRetrySlotIfNeeded     |
| T2     | FindFollowerRuleForOrder        |
| T2     | IsReplaceDispatchEligible       |
| T2     | IsQxCancelEligible3             |
| T2     | CommitStaleCancelBatch          |
| T3     | IsBeBracketRecoveryEligible     |
| T3     | HasActiveQxOrders               |
| T3     | IsQxCancelEligible2             |
| T3     | CommitQxCancelBatch             |
| T3     | IsPositionStateTriggerState     |
| T3     | TryClearLeaderDirectionOnFlat   |
| T3     | IsNativeLeaderTarget            |
| T3     | CancelStaleTargetDrag           |
| T3     | CreateAndSubmitCascadeTarget    |
| T4     | HandleDrainTerminalState        |
| T4     | IsCancelAllStateOk              |
| T4     | IsQxSnapshotStateOk             |
| T4     | IssueDrainCancels               |
| T4     | MatchesBracketType              |
| T4     | ExtractLegSuffix                |
| T5     | IsNakedConditionMet             |
| T5     | BuildAtmModeNames               |
| T5     | MatchesFollowerSlot             |
| T5     | ResolveNullFollowerSlot         |
| T5     | PickBestTargetPrice             |
| T5     | MirrorCloseOneAccount           |
| T5     | ResolveMultiplierLength         |
| T5     | UpdateLegTargetPrice            |
| T5     | IsPriceDeltaSignificant         |
| T5     | RoundToTick                     |
| T5     | SubmitReplacementStopOrder      |

---

Tickets written by ptt-architect. Status: TICKETS_COMPLETE.
