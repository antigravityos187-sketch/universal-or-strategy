# BWAVE-REFACTOR Lane C -- Architecture Plan
# Phase 1 Output (ptt-architect)
# Status: PLAN_COMPLETE
# Date: 2026-08-21

---

## Section 1: Lane-Split Gate Result

**Result: SINGLE-PIPELINE**

This file IS the designated Lane C. The lane-split was performed by the orchestrator upstream.
Lane C scope: all `Features/*.cs` methods with CCN > 8.

Gate answers (confirmed):
- Q1: Are all targets in the same method or within 50 lines of each other? NO -- different files/classes.
- Q2: Does Fix B design depend on Fix A final design? NO -- independent class extractions.
- Q3: Does each fix have standalone value if the other is blocked? YES.
- Q4: Does each fix have an independent SIM verification path (lizard CCN scan per file)? YES.

---

## Section 2: Rules Catalog Gate Result

**Result: PASS**

File read: `docs/standards/jane-street/RULES_CATALOG.md` -- UTF-8 clean, fully readable.

P0 pre-scan against all 7 source files:

| P0 Rule | Pattern | Result |
|---------|---------|--------|
| JS-021: lock() | `lock\s*\(` | 0 hits -- all 7 files PASS |
| JS-033: async void | `async void \w` | 0 hits -- all 7 files PASS |
| JS-002: return null (new methods) | All new helpers return void, string, bool, int, or tuple | PASS |
| JS-001: throw in hot path | No new throw statements introduced | PASS |
| DateTime.Now | All date refs use DateTime.UtcNow or DateTime.MaxValue | PASS |
| ASCII-only | All new method names and string literals are ASCII | PASS |

**GATE RESULT: PASS -- all work proceeds.**

---

## Section 3: Source Analysis -- Per-File Target Summary

### 3.1 PttQuickExit.cs

**Target method:** `Execute(Account, Instrument, int, List<(double,int)>, bool, double, int)` -- lizard CCN=32.

**Root cause of high CCN:** The for-loop body (targetCount iterations) embeds two full `try/catch` blocks
for stop submit and target submit respectively, each containing null checks and log paths. All loop-body
branches count against the single method CYC.

**What the method does:**
1. Finds leader position; guards null/flat.
2. Rejects follower accounts (skipIfFollower guard).
3. Snapshots the existing stop price before any cancel.
4. Cancels existing ATM + PTT-QX brackets via CopyEngine.
5. Determines direction (isLong) and tick size.
6. Resolves target count from snapshot / leaderCount / default.
7. For each target pair (0..targetCount-1): computes OCO ID, stop name, target name, per-pair qty, then submits stop + limit OCO pair.
8. Raises PttBus.QuickExitFired with T1/T2 prices for Card B back-calc.

**Helpers to extract:**

| Helper Name | Visibility | Return | CYC | What it does |
|-------------|-----------|--------|-----|--------------|
| `SubmitQxOcoPair` | private | string (ocoId) | 8 | Per-iteration loop body: computes tNQty, ocoId, stop/target names; calls SubmitQxStop + SubmitQxTarget; returns ocoId for firstOcoId capture |
| `SubmitQxStop` | private static | void | 3 | CreateOrder(StopMarket) + Submit + null-guard log, wrapped in try/catch |
| `SubmitQxTarget` | private static | void | 3 | CreateOrder(Limit) + Submit + null-guard log, wrapped in try/catch |

**Execute after extraction CYC:** pos-null(1) + qty==0(2) + skipIfFollower(3) + IsFollower(4) + for-loop(5) + i==0 firstOcoId capture(6) + ocoId null guard(7) = **7**. PASS.

**Signatures:**

```csharp
// Execute is UNCHANGED in signature -- only internals refactored
internal void Execute(Account leader, Instrument instr, int t1Ticks,
    System.Collections.Generic.List<(double Price, int Qty)> targets,
    bool skipIfFollower = true, double leaderStop = 0, int leaderTargetCount = 0)

// NEW HELPERS:
private string SubmitQxOcoPair(
    Account leader, Instrument instr, bool isLong, double entryPx,
    double snapshotStop, double tick, int t1Ticks, int i, int targetCount,
    System.Collections.Generic.List<(double Price, int Qty)> targets)

private static void SubmitQxStop(
    Account acc, Instrument instr, bool isLong, double snapshotStop,
    int qty, string ocoId, string stopName)

private static void SubmitQxTarget(
    Account acc, Instrument instr, bool isLong, double tNPrice,
    int qty, string ocoId, string targetName)
```

**NT8 API notes:** CreateOrder 12-arg call preserved exactly. NT8-049 arg6/arg7 never swapped.
NT8-013: DateTime.MaxValue GTC preserved. NT8-014: "PTT-QX-Stop"/"PTT-QX-T*" prefixes preserved.

---

### 3.2 PttGlobalQuickExit.cs

**Target methods:**

| Method | lizard CCN | Comment |
|--------|-----------|---------|
| `SnapshotTargetOrders` | 20 | foreach + compound isNative/isPtt checks + dedup dictionary pass |
| `WaitForPttBeCancelled` | 10 | while-poll + inner foreach with 4 continue guards |
| `Execute()` (no-arg) | 9 | DW-B115-DIAG block with its own for-loop inflates parent count |
| `CancelPttBeOrders` | 9 | foreach + 4 compound guard conditions |
| `Execute(forcedTargets)` | 8 | AT limit -- excluded per instructions (only include if lizard >8) |

**Helpers to extract:**

| Helper Name | Visibility | Return | CYC | What it does |
|-------------|-----------|--------|-----|--------------|
| `IsActiveTargetOrder` | private static | bool | 5 | stateOk(Working\|Accepted) + instrOk + Limit-type + name-not-empty |
| `IsNativeTargetOrder` | private static | bool | 3 | StartsWith("Target") + Length>6 + IsDigit(name[6]) |
| `IsPttTargetOrder` | private static | bool | 2 | StartsWith("PTT-QX-T") + starts with "PTT-BE-Target-" |
| `DeduplicateByPrice` | private static | List<(double,int)> | 3 | Dictionary dedup keeping max qty per price level |
| `CountNonTerminalBeOrders` | private static | int | 5 | foreach acc.Orders: null + instrOk + IsPttBeOrder + IsNonTerminalPttBeState |
| `IsEligibleForBeCancel` | private static | bool | 4 | o null + instrOk + IsPttBeOrder + IsNonTerminalPttBeState |
| `LogLeaderTargetDiag` | private static | void | 3 | Extract DW-B115-DIAG StringBuilder block from Execute() |

**SnapshotTargetOrders after extraction CYC:** null(1) + foreach(2) + IsActiveTargetOrder(3) + IsNativeTargetOrder(4) + IsPttTargetOrder(5) + nativeTargets.Count==0(6) + DeduplicateByPrice(7) = **7**. PASS.

**WaitForPttBeCancelled after extraction CYC:** acc/count guard(1) + while(2) + CountNonTerminalBeOrders(3) + count==0(4) + Sleep(5) + timeout-log(6) = **6**. PASS.

**Execute() after extracting LogLeaderTargetDiag CYC:** flag-guard(1) + acc-loop(2) + follower-skip(3) + pos-loop(4) + pos-null-continue(5) + flatten-guard(6) + ExecuteFollowers(7) = **7**. PASS. (The DIAG block's for-loop is moved to LogLeaderTargetDiag; only the log call remains.)

**CancelPttBeOrders after extracting IsEligibleForBeCancel CYC:** acc/instr null(1) + foreach(2) + filter(3) + count==0(4) + acc.Cancel(5) + log(6) = **5**. PASS.

**Signatures:**

```csharp
// All existing public/internal signatures UNCHANGED

// NEW HELPERS (all private static within PttGlobalQuickExit):
private static bool IsActiveTargetOrder(
    NinjaTrader.Cbi.Order o, NinjaTrader.Cbi.Instrument instr)

private static bool IsNativeTargetOrder(string name)

private static bool IsPttTargetOrder(string name)

private static System.Collections.Generic.List<(double Price, int Qty)>
    DeduplicateByPrice(
        System.Collections.Generic.List<(double Price, int Qty)> targets)

private static int CountNonTerminalBeOrders(
    NinjaTrader.Cbi.Account acc, NinjaTrader.Cbi.Instrument instr)

private static bool IsEligibleForBeCancel(
    NinjaTrader.Cbi.Order o, NinjaTrader.Cbi.Instrument instr)

private static void LogLeaderTargetDiag(
    NinjaTrader.Cbi.Account acc,
    System.Collections.Generic.List<(double Price, int Qty)> targets,
    int posQty)
```

---

### 3.3 PttBreakEven.cs

**Target methods:**

| Method | lizard CCN | Comment |
|--------|-----------|---------|
| `SubmitBeTargetsLocal` | 15 | for-loop with two try/catch blocks; 0-targets bare-stop branch |
| `CancelStaleBracketsLocal` | 16 | foreach with 5-OR stateOk + instrOk + notBe + RemoveAll + try/catch |
| `SnapshotTargetsLocal` | 13 | foreach with 5-OR stateOk + instrOk + IsAtmTarget\|IsPttQx |
| `IsPttQxTarget` | 12 | 10 individual char comparisons (name[0]..name[8]) |
| `SubmitBeStopLocal` | 9 | null + pos + direction + CreateOrder + catch |

**Helpers to extract:**

| Helper Name | Visibility | Return | CYC | What it does |
|-------------|-----------|--------|-----|--------------|
| `SubmitBeBareStop` | private static | void | 5 | 0-targets path: FindPositionLocal + qty guard + CreateOrder(StopMarket) + Submit, try/catch |
| `SubmitBePairOrders` | private static | void | 6 | Per-pair: CreateOrder(StopMarket) try/catch + CreateOrder(Limit) try/catch for one OCO pair |
| `IsStaleOrderForBe` | private static | bool | 8 | stateOk(5 OR terms) + instrOk + notBe -- composite filter for CancelStaleBracketsLocal |
| `IsLiveTargetOrder` | private static | bool | 7 | stateOk(5 OR terms) + instrOk + (IsAtmTargetName \| IsPttQxTarget) |
| `SubmitSingleBeStop` | private static | void | 3 | CreateOrder(StopMarket) + Submit + null guard + catch -- bare stop for SubmitBeStopLocal |

**IsPttQxTarget reimplementation (not a new extraction -- in-place rewrite):**
Replace 10-char char-by-char with string equality:
```csharp
// BEFORE (CYC=12 per lizard -- 10 char comparisons):
private static bool IsPttQxTarget(string name)
{
    if (name == null || name.Length != 9) return false;
    return name[0]=='P' && name[1]=='T' && name[2]=='T' && name[3]=='-'
        && name[4]=='Q' && name[5]=='X' && name[6]=='-'
        && name[7]=='T' && name[8]>='1' && name[8]<='3';
}
// AFTER (CYC=3 -- logically equivalent, exact string match):
private static bool IsPttQxTarget(string name)
{
    if (name == null || name.Length != 9) return false; // (1)
    return string.Equals(name, "PTT-QX-T1", StringComparison.Ordinal)  // (2)
        || string.Equals(name, "PTT-QX-T2", StringComparison.Ordinal)  // (3)
        || string.Equals(name, "PTT-QX-T3", StringComparison.Ordinal);
}
```
Behavior: identical. Only names exactly "PTT-QX-T1", "PTT-QX-T2", "PTT-QX-T3" (length 9) return true.

**SubmitBeTargetsLocal after extraction CYC:** null(1) + targets-null(2) + 0-targets branch(3) + for-loop(4) + SubmitBePairOrders call(5) + end-log(6) = **5**. PASS.

**CancelStaleBracketsLocal after extraction CYC:** null(1) + foreach(2) + IsStaleOrderForBe(3) + count==0(4) + RemoveAll(5) + try/Cancel(6) + catch(7) = **7**. PASS.

**SnapshotTargetsLocal after extraction CYC:** null(1) + foreach(2) + IsLiveTargetOrder(3) + add+log(4) = **4**. PASS.

**SubmitBeStopLocal after extraction CYC:** null(1) + pos null(2) + direction(3) + SubmitSingleBeStop(4) = **4**. PASS.

**Signatures:**

```csharp
// All existing public/internal signatures UNCHANGED

// NEW HELPERS (all private static within PttBreakEven):
private static void SubmitBeBareStop(
    Account acc, Instrument instr, double bePrice, OrderAction stopDir)

private static void SubmitBePairOrders(
    Account acc, Instrument instr, double bePrice, OrderAction stopDir,
    (double Price, int Qty, OrderAction Action) t, string ocoId, int pairIndex)

private static bool IsStaleOrderForBe(Order o, Instrument instr)

private static bool IsLiveTargetOrder(Order o, Instrument instr)

private static void SubmitSingleBeStop(
    Account acc, Instrument instr, double bePrice, OrderAction direction,
    int qty, string ocoId, string stopName)
```

---

### 3.4 PttBreakEvenSwap.cs

**Target method:** `Execute(Account, Instrument, double, List<(double,int,OrderAction)>)` -- lizard CCN=15.

**What the method does:**
1. Null + flat guards.
2. CancelQxBrackets (cancels all ATM+PTT brackets).
3. Determines isLong from position.
4. 0-targets path: submits single bare PTT-BE-Stop if price is submittable.
5. With-targets path: for each target, submits PTT-BE-Stop-{i+1} + PTT-BE-Target-{i+1} as OCO pair.

**Helpers to extract:**

| Helper Name | Visibility | Return | CYC | What it does |
|-------------|-----------|--------|-----|--------------|
| `SubmitSwapBareStop` | private static | void | 5 | 0-targets path: IsStopPriceSubmittable guard + CreateOrder(StopMarket) try/catch + else-log |
| `SubmitSwapPairs` | private static | void | 8 | with-targets path: for-loop + IsStopPriceSubmittable + stop try/catch + target try/catch |

**Execute after extraction CYC:** null(1) + flat(2) + isLong(3) + 0-targets branch(4) + seq+for-loop(5) = **5**. PASS. (Cancel call is unconditional, no branch added.)

**SubmitSwapPairs AT-LIMIT NOTE:** CYC=8 is exactly at the lizard limit. If lizard reports 9 after implementation, engineer must further extract `SubmitSwapStopOrder` and `SubmitSwapTargetOrder` (each CYC=3), reducing SubmitSwapPairs to CYC=5. See RISK-04.

**Signatures:**

```csharp
// Execute UNCHANGED in signature

// NEW HELPERS (all private static within PttBreakEvenSwap):
private static void SubmitSwapBareStop(
    Account acc, Instrument instr, double newStop,
    OrderAction stopDir, Position pos)

private static void SubmitSwapPairs(
    Account acc, Instrument instr, double newStop, OrderAction stopDir,
    System.Collections.Generic.List<(double Price, int Qty, OrderAction Action)> targets,
    int seq)
```

---

### 3.5 PttTrim.cs

**Target method:** `TrimPositionLocal(Account, Instrument, int, Position, int, double, double, double)` -- lizard CCN=13.

**What the method does:**
1. Null/qty guard.
2. Determines direction from MarketPosition.
3. Computes useLimitOrder bool (tickSize > 0 AND market price available).
4. Branches: Limit path (computes limitPrice) vs Market path.
5. CreateOrder + Submit via try/catch.

**Helpers to extract:**

| Helper Name | Visibility | Return | CYC | What it does |
|-------------|-----------|--------|-----|--------------|
| `BuildTrimOrderParams` | private static | (OrderType, double, double) | 2 | If useLimitOrder: Limit + isLong ternary for limitPrice; else: Market + zeros |
| `SubmitTrimOrder` | private static | void | 4 | CreateOrder + null check + Submit + log (ternary for mkt/limit log) + catch |

**TrimPositionLocal after extraction CYC:** null(1) + direction(2) + useLimitOrder bool(3+4) + BuildParams(5) + SubmitOrder(6) = **6**. PASS.

**Signatures:**

```csharp
// TrimPositionLocal UNCHANGED in signature

// NEW HELPERS (all private static within PttTrim):
private static (OrderType Type, double LimitPrice, double StopPrice) BuildTrimOrderParams(
    bool isLong, bool useLimitOrder, double ask, double bid,
    int buffer, double tickSize)

private static void SubmitTrimOrder(
    Account acc, Instrument instr, OrderAction direction,
    int qty, OrderType orderType, double limitPrice, double stopPrice,
    bool useLimitOrder)
```

---

### 3.6 PttFlatten.cs

**Target method:** `FlattenPositionLocal(Account, Instrument, Position, int, double, double, double)` -- lizard CCN=13.

**Identical structure to TrimPositionLocal.** Same extraction pattern.

**Helpers to extract:**

| Helper Name | Visibility | Return | CYC | What it does |
|-------------|-----------|--------|-----|--------------|
| `BuildFlatOrderParams` | private static | (OrderType, double, double) | 2 | If useLimitOrder: Limit + isLong ternary for limitPrice; else: Market + zeros |
| `SubmitFlatOrder` | private static | void | 4 | CreateOrder + null check + Submit + log + catch |

**FlattenPositionLocal after extraction CYC:** null(1) + direction(2) + useLimitOrder(3+4) + BuildParams(5) + SubmitOrder(6) = **6**. PASS.

**Signatures:**

```csharp
// FlattenPositionLocal UNCHANGED in signature

// NEW HELPERS (all private static within PttFlatten):
private static (OrderType Type, double LimitPrice, double StopPrice) BuildFlatOrderParams(
    bool isLong, bool useLimitOrder, double ask, double bid,
    int buffer, double tickSize)

private static void SubmitFlatOrder(
    Account acc, Instrument instr, OrderAction direction,
    int qty, OrderType orderType, double limitPrice, double stopPrice,
    bool useLimitOrder)
```

---

### 3.7 PttCancel.cs

**Target method:** `CancelWorkingEntriesLocal(Account, Instrument)` -- lizard CCN=10.

**What the method does:**
1. Null guard.
2. foreach acc.Orders: builds list of Working|Initialized orders for instr.
3. Count==0 early return.
4. acc.Cancel(array) in try/catch.

**Helper to extract:**

| Helper Name | Visibility | Return | CYC | What it does |
|-------------|-----------|--------|-----|--------------|
| `IsWorkingOrderForInstrument` | private static | bool | 5 | o null + Working + Initialized + instrOk(null) + instrOk(FullName) |

**CancelWorkingEntriesLocal after extraction CYC:** null(1) + foreach(2) + filter(3) + count==0(4) + try Cancel(5) + catch(6) = **6**. PASS.

**Signature:**

```csharp
// CancelWorkingEntriesLocal UNCHANGED in signature

// NEW HELPER (private static within PttCancel):
private static bool IsWorkingOrderForInstrument(Order o, Instrument instr)
```

---

## Section 4: Ticket Plan

### Ticket T1 -- PttQuickExit.cs (CCN 32 -> <=8)

**File:** `src/PropTraderTools/Features/PttQuickExit.cs`
**Spec req IDs satisfied:** CCN-QX-01
**Work:** Refactor `Execute` main overload internals only. Extract 3 helpers. No public API change.

**Methods to implement:**

```csharp
// REFACTORED (existing, signature unchanged):
internal void Execute(Account leader, Instrument instr, int t1Ticks,
    System.Collections.Generic.List<(double Price, int Qty)> targets,
    bool skipIfFollower = true, double leaderStop = 0, int leaderTargetCount = 0)
// Target CYC: 7

// NEW:
private string SubmitQxOcoPair(
    Account leader, Instrument instr, bool isLong, double entryPx,
    double snapshotStop, double tick, int t1Ticks, int i, int targetCount,
    System.Collections.Generic.List<(double Price, int Qty)> targets)
// Target CYC: 8

private static void SubmitQxStop(
    Account acc, Instrument instr, bool isLong, double snapshotStop,
    int qty, string ocoId, string stopName)
// Target CYC: 3

private static void SubmitQxTarget(
    Account acc, Instrument instr, bool isLong, double tNPrice,
    int qty, string ocoId, string targetName)
// Target CYC: 3
```

**JS rule constraints per method:**
- All: JS-021 no lock, JS-033 no async void, JS-001 no throw (try/catch only), JS-002 no return null
- SubmitQxOcoPair: returns string (ocoId) -- never null (guaranteed by NextQxOcoId + Guid fallback)
- NT8-049: SubmitQxStop arg6=0, arg7=snapshotStop; SubmitQxTarget arg6=tNPrice, arg7=0
- NT8-007: arg12=(CustomOrder)null; NT8-013: DateTime.MaxValue; NT8-014: "PTT-QX-*" prefix

**7-Scan checklist:**
- SCAN-01: `dotnet build` -- 0 errors
- SCAN-02: `grep -P "[^\x00-\x7F]" src/PropTraderTools/Features/PttQuickExit.cs` -- 0 matches
- SCAN-03: `grep -n "lock(" src/PropTraderTools/Features/PttQuickExit.cs` -- 0 matches
- SCAN-04: `lizard src/PropTraderTools/Features/PttQuickExit.cs -C 8` -- 0 methods >8
- SCAN-05: `grep -n "async void" src/PropTraderTools/Features/PttQuickExit.cs` -- 0 matches
- SCAN-06: `grep -n "return null;" src/PropTraderTools/Features/PttQuickExit.cs` -- 0 matches in new methods (FindPositionLocal pre-existing null return exempt)
- SCAN-07: `powershell -File scripts\ptt-sync-and-verify.ps1` -- 18/18 OK

---

### Ticket T2 -- PttGlobalQuickExit.cs (all CCN>8 methods)

**File:** `src/PropTraderTools/Features/PttGlobalQuickExit.cs`
**Spec req IDs satisfied:** CCN-GQX-01, CCN-GQX-02, CCN-GQX-03, CCN-GQX-04

**Methods to implement:**

```csharp
// REFACTORED (internals only, signatures unchanged):
// - SnapshotTargetOrders  -- target CYC: 7
// - WaitForPttBeCancelled -- target CYC: 6
// - Execute()             -- target CYC: 7
// - CancelPttBeOrders     -- target CYC: 5

// NEW (all private static within PttGlobalQuickExit):
private static bool IsActiveTargetOrder(
    NinjaTrader.Cbi.Order o, NinjaTrader.Cbi.Instrument instr)
// Target CYC: 5

private static bool IsNativeTargetOrder(string name)
// Target CYC: 3

private static bool IsPttTargetOrder(string name)
// Target CYC: 2

private static System.Collections.Generic.List<(double Price, int Qty)>
    DeduplicateByPrice(
        System.Collections.Generic.List<(double Price, int Qty)> targets)
// Target CYC: 3

private static int CountNonTerminalBeOrders(
    NinjaTrader.Cbi.Account acc, NinjaTrader.Cbi.Instrument instr)
// Target CYC: 5

private static bool IsEligibleForBeCancel(
    NinjaTrader.Cbi.Order o, NinjaTrader.Cbi.Instrument instr)
// Target CYC: 4

private static void LogLeaderTargetDiag(
    NinjaTrader.Cbi.Account acc,
    System.Collections.Generic.List<(double Price, int Qty)> targets,
    int posQty)
// Target CYC: 3
```

**JS rule constraints per method:**
- All: JS-021 no lock, JS-033 no async void
- DeduplicateByPrice: returns non-null List (never null) -- JS-002 PASS
- CountNonTerminalBeOrders: returns int (never null)
- Thread.Sleep(20) remains in WaitForPttBeCancelled body -- preserved, not moved to helper
- DateTime.UtcNow: preserved in WaitForPttBeCancelled deadline computation (stays in method)

**7-Scan checklist:**
- SCAN-01: `dotnet build` -- 0 errors
- SCAN-02: ASCII grep on PttGlobalQuickExit.cs -- 0 non-ASCII
- SCAN-03: lock grep -- 0
- SCAN-04: lizard CCN<=8 -- 0 methods >8
- SCAN-05: async void grep -- 0
- SCAN-06: return null grep in new methods -- 0
- SCAN-07: ptt-sync-and-verify.ps1 -- 18/18 OK

---

### Ticket T3 -- PttBreakEven.cs + PttBreakEvenSwap.cs

**Files:**
- `src/PropTraderTools/Features/PttBreakEven.cs`
- `src/PropTraderTools/Features/PttBreakEvenSwap.cs`

**Spec req IDs satisfied:** CCN-BE-01, CCN-BE-02, CCN-BE-03, CCN-BE-04, CCN-BE-05, CCN-BES-01

**Methods to implement:**

**PttBreakEven.cs -- NEW helpers:**

```csharp
private static void SubmitBeBareStop(
    Account acc, Instrument instr, double bePrice, OrderAction stopDir)
// Target CYC: 5

private static void SubmitBePairOrders(
    Account acc, Instrument instr, double bePrice, OrderAction stopDir,
    (double Price, int Qty, OrderAction Action) t, string ocoId, int pairIndex)
// Target CYC: 6

private static bool IsStaleOrderForBe(Order o, Instrument instr)
// Target CYC: 8 (AT LIMIT -- see RISK-05)

private static bool IsLiveTargetOrder(Order o, Instrument instr)
// Target CYC: 7

private static void SubmitSingleBeStop(
    Account acc, Instrument instr, double bePrice, OrderAction direction,
    int qty, string ocoId, string stopName)
// Target CYC: 3

// IsPttQxTarget: in-place rewrite (no rename, just replace body)
// Target CYC: 3 (was 12)
```

**PttBreakEvenSwap.cs -- NEW helpers:**

```csharp
private static void SubmitSwapBareStop(
    Account acc, Instrument instr, double newStop,
    OrderAction stopDir, Position pos)
// Target CYC: 5

private static void SubmitSwapPairs(
    Account acc, Instrument instr, double newStop, OrderAction stopDir,
    System.Collections.Generic.List<(double Price, int Qty, OrderAction Action)> targets,
    int seq)
// Target CYC: 8 (AT LIMIT -- see RISK-04)
```

**JS rule constraints:**
- NT8-049: Stop orders: arg6=0, arg7=price. Limit orders: arg6=price, arg7=0. NEVER SWAP.
- NT8-007: arg11=(NinjaTrader.Cbi.CustomOrder)null
- NT8-013: DateTime.MaxValue for GTC
- NT8-014: "PTT-BE-Stop*" and "PTT-BE-Target-*" signal names preserved unchanged
- SubmitBePairOrders receives ocoId and pairIndex as params -- OCO ID is always computed by caller (SubmitBeTargetsLocal) and passed in. Never computed inside helper.
- AT-LIMIT methods: if lizard reports >8, engineer must sub-extract per RISK-04/05 mitigations (documented in Section 5).

**7-Scan checklist (both files):**
- SCAN-01: `dotnet build` -- 0 errors
- SCAN-02: ASCII grep on PttBreakEven.cs AND PttBreakEvenSwap.cs -- 0 non-ASCII each
- SCAN-03: lock grep -- 0 each
- SCAN-04: lizard CCN<=8 -- 0 methods >8 in either file
- SCAN-05: async void grep -- 0
- SCAN-06: return null grep in new/modified methods -- 0
- SCAN-07: ptt-sync-and-verify.ps1 -- 18/18 OK

---

### Ticket T4 -- PttTrim.cs + PttFlatten.cs + PttCancel.cs

**Files:**
- `src/PropTraderTools/Features/PttTrim.cs`
- `src/PropTraderTools/Features/PttFlatten.cs`
- `src/PropTraderTools/Features/PttCancel.cs`

**Spec req IDs satisfied:** CCN-TR-01, CCN-FL-01, CCN-CN-01

**Methods to implement:**

**PttTrim.cs -- NEW helpers:**

```csharp
private static (OrderType Type, double LimitPrice, double StopPrice) BuildTrimOrderParams(
    bool isLong, bool useLimitOrder, double ask, double bid,
    int buffer, double tickSize)
// Target CYC: 2

private static void SubmitTrimOrder(
    Account acc, Instrument instr, OrderAction direction,
    int qty, OrderType orderType, double limitPrice, double stopPrice,
    bool useLimitOrder)
// Target CYC: 4
```

**PttFlatten.cs -- NEW helpers:**

```csharp
private static (OrderType Type, double LimitPrice, double StopPrice) BuildFlatOrderParams(
    bool isLong, bool useLimitOrder, double ask, double bid,
    int buffer, double tickSize)
// Target CYC: 2

private static void SubmitFlatOrder(
    Account acc, Instrument instr, OrderAction direction,
    int qty, OrderType orderType, double limitPrice, double stopPrice,
    bool useLimitOrder)
// Target CYC: 4
```

**PttCancel.cs -- NEW helper:**

```csharp
private static bool IsWorkingOrderForInstrument(Order o, Instrument instr)
// Target CYC: 5
```

**JS rule constraints:**
- BuildTrimOrderParams and BuildFlatOrderParams return value tuples -- never null (struct types)
- JS-002: tuple return satisfies "no return null" (value types cannot be null)
- NT8-049: Limit arg6=limitPrice arg7=0; Market arg6=0 arg7=0 -- preserved via BuildParams helpers
- NT8-014: "PTT-Trim" and "PTT-Flatten" signal names preserved in caller SubmitTrimOrder/SubmitFlatOrder
- SubmitTrimOrder/SubmitFlatOrder receive pre-built orderType/limitPrice/stopPrice -- no NT8 logic in params layer

**7-Scan checklist (all three files):**
- SCAN-01: `dotnet build` -- 0 errors
- SCAN-02: ASCII grep on PttTrim.cs, PttFlatten.cs, PttCancel.cs -- 0 non-ASCII each
- SCAN-03: lock grep -- 0 each
- SCAN-04: lizard CCN<=8 -- 0 methods >8 in all three files
- SCAN-05: async void grep -- 0
- SCAN-06: return null grep in new/modified methods -- 0 (FindPositionLocal pre-existing exempt)
- SCAN-07: ptt-sync-and-verify.ps1 -- 18/18 OK

---

### Ticket T5 -- BwaveRefactorLaneCTests.cs (structural xUnit tests)

**File:** `src/PropTraderTools/Tests/BwaveRefactorLaneCTests.cs` (NEW FILE)
**Spec req IDs satisfied:** All CCN-* IDs (structural verification)

**Approach:** Reflection-based structural tests. Each `[Fact]` verifies that an extracted helper:
1. Exists in the declaring type (MethodInfo != null)
2. Has the correct arity (parameter count)
3. Has the correct return type

This pattern requires NO NT8 runtime. Tests compile and run against the PropTraderTools assembly in the standard xUnit test host.

**Complete test inventory (21 tests):**

```csharp
// T1 group -- PttQuickExit helpers
[Fact] void SubmitQxOcoPair_Exists_With10Params()        // PttQuickExit, 10 params, returns string
[Fact] void SubmitQxStop_Exists_With7Params()             // PttQuickExit, 7 params, returns void
[Fact] void SubmitQxTarget_Exists_With7Params()           // PttQuickExit, 7 params, returns void

// T2 group -- PttGlobalQuickExit helpers
[Fact] void IsActiveTargetOrder_Exists_With2Params()      // PttGlobalQuickExit, 2 params, returns bool
[Fact] void IsNativeTargetOrder_Exists_With1Param()       // PttGlobalQuickExit, 1 param, returns bool
[Fact] void IsPttTargetOrder_Exists_With1Param()          // PttGlobalQuickExit, 1 param, returns bool
[Fact] void DeduplicateByPrice_Exists_With1Param()        // PttGlobalQuickExit, 1 param, returns List
[Fact] void CountNonTerminalBeOrders_Exists_With2Params() // PttGlobalQuickExit, 2 params, returns int
[Fact] void IsEligibleForBeCancel_Exists_With2Params()    // PttGlobalQuickExit, 2 params, returns bool
[Fact] void LogLeaderTargetDiag_Exists_With3Params()      // PttGlobalQuickExit, 3 params, returns void

// T3a group -- PttBreakEven helpers
[Fact] void SubmitBeBareStop_Exists_With4Params()         // PttBreakEven, 4 params, returns void
[Fact] void SubmitBePairOrders_Exists_With7Params()       // PttBreakEven, 7 params, returns void
[Fact] void IsStaleOrderForBe_Exists_With2Params()        // PttBreakEven, 2 params, returns bool
[Fact] void IsLiveTargetOrder_Exists_With2Params()        // PttBreakEven, 2 params, returns bool
[Fact] void SubmitSingleBeStop_Exists_With7Params()       // PttBreakEven, 7 params, returns void

// T3b group -- PttBreakEvenSwap helpers
[Fact] void SubmitSwapBareStop_Exists_With5Params()       // PttBreakEvenSwap, 5 params, returns void
[Fact] void SubmitSwapPairs_Exists_With6Params()          // PttBreakEvenSwap, 6 params, returns void

// T4a group -- PttTrim helpers
[Fact] void BuildTrimOrderParams_Exists_With6Params()     // PttTrim, 6 params, returns ValueTuple
[Fact] void SubmitTrimOrder_Exists_With8Params()          // PttTrim, 8 params, returns void

// T4b group -- PttFlatten helpers
[Fact] void BuildFlatOrderParams_Exists_With6Params()     // PttFlatten, 6 params, returns ValueTuple
[Fact] void SubmitFlatOrder_Exists_With8Params()          // PttFlatten, 8 params, returns void

// T4c group -- PttCancel helper
[Fact] void IsWorkingOrderForInstrument_Exists_With2Params() // PttCancel, 2 params, returns bool
```

Wait -- that's 22 tests (3+7+5+2+2+2+1=22). Recount: T1=3, T2=7, T3a=5, T3b=2, T4a=2, T4b=2, T4c=1 = **22 tests total**.

**Test method template:**

```csharp
[Fact]
public void SubmitQxStop_Exists_With7Params()
{
    var mi = typeof(PttQuickExit).GetMethod(
        "SubmitQxStop",
        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    Assert.NotNull(mi);
    Assert.Equal(7, mi.GetParameters().Length);
    Assert.Equal(typeof(void), mi.ReturnType);
}
```

**7-Scan checklist (test file):**
- SCAN-01: `dotnet build` -- 0 errors
- SCAN-02: ASCII grep on BwaveRefactorLaneCTests.cs -- 0 non-ASCII
- SCAN-03: lock grep -- 0
- SCAN-04: lizard on test file -- N/A (xUnit test methods are CYC=1 by design)
- SCAN-05: async void grep -- 0
- SCAN-06: return null grep -- 0
- SCAN-07: ptt-sync-and-verify.ps1 -- 18/18 OK

---

## Section 5: Risk Register

| ID | Description | Severity | Mitigation |
|----|-------------|----------|-----------|
| RISK-01 | IsPttQxTarget in-place rewrite: char-by-char -> string equality | LOW | Behavior identical. Only names of length 9 exactly matching "PTT-QX-T1/T2/T3" return true. No test regression possible. |
| RISK-02 | DeduplicateByPrice uses double as dictionary key | LOW (pre-existing) | Existing code, not introduced by this refactor. Documented as DW-B123. No change to logic. |
| RISK-03 | SubmitQxOcoPair returns string (non-void helper) | NONE | Return value is always non-null (NextQxOcoId or Guid fallback). Satisfies JS-002. |
| RISK-04 | SubmitSwapPairs CYC=8 AT LIMIT | MEDIUM | If lizard reports >8: split into SubmitSwapStopOrder(acc,instr,t,ocoId,newStop,stopDir) CYC=3 and SubmitSwapTargetOrder(acc,instr,t,ocoId) CYC=3. SubmitSwapPairs becomes CYC=5. Add 2 test facts. |
| RISK-05 | IsStaleOrderForBe CYC=8 AT LIMIT | MEDIUM | If lizard reports >8: split compound stateOk check into IsActiveCancellableState(OrderState s) CYC=5 (5 OR terms). IsStaleOrderForBe calls it: CYC drops to 3. Add 1 test fact. |
| RISK-06 | NT8 sim regression from structural extraction | LOW | All cancel-before-resubmit patterns, OCO ID construction, and state machine transitions preserved exactly. No NT8 order flow logic changed. |
| RISK-07 | Reflection tests require PropTraderTools assembly accessible | LOW | Test project already references PropTraderTools. GetMethod(BindingFlags.NonPublic\|Static) works on private static methods in same assembly. |

**AT-LIMIT contingency plan:** Engineer must run `lizard` against EACH ticket's output file immediately after implementation, before declaring scan 4 PASS. If any AT-LIMIT helper reports >8, apply the named mitigation before proceeding to next ticket.

---

## Section 6: Definition of Done

All of the following must be simultaneously true before Lane C is declared complete:

| # | Check | Command | Pass Criterion |
|---|-------|---------|----------------|
| 1 | lizard CCN scan -- Features/ | `lizard src/PropTraderTools/Features/ -C 8 --csv` | Zero rows in output (0 methods with CCN > 8) |
| 2 | dotnet build | `dotnet build src/PropTraderTools/PropTraderTools.csproj` | Exit code 0, 0 errors, 0 warnings of type CS |
| 3 | NT8 sync + verify | `powershell -File scripts\ptt-sync-and-verify.ps1` | 18/18 OK (0 MISMATCH lines) |
| 4 | F5 NinjaTrader 8 compilation | Manual: F5 in NT8 on exact wave branch | Green compile in NinjaScript Output |
| 5 | xUnit test pass | `dotnet test src/PropTraderTools/Tests/` | 22/22 PASS (all structural [Fact] tests) |
| 6 | P0 scans | See 7-scan per ticket | All 7 scans pass for all 5 tickets |

**CCN target by file (post-extraction):**

| File | Methods Targeted | Target CCN (all) |
|------|----------------|-----------------|
| `PttQuickExit.cs` | Execute (main) | <=8 |
| `PttGlobalQuickExit.cs` | SnapshotTargetOrders, WaitForPttBeCancelled, Execute(), CancelPttBeOrders | <=8 |
| `PttBreakEven.cs` | SubmitBeTargetsLocal, CancelStaleBracketsLocal, SnapshotTargetsLocal, IsPttQxTarget, SubmitBeStopLocal | <=8 |
| `PttBreakEvenSwap.cs` | Execute | <=8 |
| `PttTrim.cs` | TrimPositionLocal | <=8 |
| `PttFlatten.cs` | FlattenPositionLocal | <=8 |
| `PttCancel.cs` | CancelWorkingEntriesLocal | <=8 |

**CCN=8 AT-LIMIT methods excluded from scope** (per instructions: only extract if lizard >8):
- `PttGlobalQuickExit::Execute(forcedTargets)` -- CCN=8
- `PttGlobalQuickExit` 80-NLOC method -- CCN=8

These are already compliant. No extraction required.

---

*ptt-architect Phase 1 output. Status: PLAN_COMPLETE.*
*Next phase: ptt-plan-reviewer reviews this document and returns REVIEW_PASS or REVIEW_FAIL.*
*If REVIEW_PASS: ptt-architect Phase 3 generates 04-tickets.md from this plan.*
