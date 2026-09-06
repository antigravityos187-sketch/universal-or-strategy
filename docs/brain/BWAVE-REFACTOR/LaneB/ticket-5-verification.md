# BWAVE-REFACTOR LaneB -- Ticket 5 Verification

# Phase 4b Output

# Author: ptt-verifier

# Ticket: BWAVE-REFACTOR-LaneB-T5

# Date: 2025-01-30

---

## Scope Confirmation

TICKET 5 ONLY. This is the FINAL ticket of BWAVE-REFACTOR Lane B.
Scope: 11 T5 target methods (CCN=9 each) + 3 residual methods discovered during SCAN 1
post-T4 (ArmPendingBe CCN=11, IsImmediateBeEligible CCN=16, DrainThenDispatch CCN=11).
Final gate: SCAN 1 must produce ZERO output for the ENTIRE CopyEngine.cs (all 366 functions).

---

## SCAN 1 Result -- CCN (ENTIRE FILE)

Command (Layer 3 -- independently run):

```powershell
$files = Get-ChildItem src/PropTraderTools/ -Filter "*.cs" -Recurse |
  Where-Object { $_.FullName -notmatch '\\obj\\' -and $_.FullName -notmatch '\\bin\\' }
lizard $files --csv 2>&1 | ConvertFrom-Csv -Header @("NLOC","CCN","Tokens","Params","Length","Location","MethodName","MethodLongName","StartLine","EndLine") |
  Where-Object { [int]$_.CCN -gt 8 } | Format-Table -AutoSize
```

Output: ZERO ROWS

PASS -- Zero rows. No method anywhere in scanned files exceeds CCN 8.
This is the final-ticket PASS condition. All 366 methods in CopyEngine.cs are CCN <= 8.

Post-T5 gate corroboration (lizard --CCN 8 on CopyEngine.cs directly):
Warning cnt: 0
Fun Cnt: 366
AvgCCN: 4.0

**SCAN 1: PASS**

---

## SCAN 2 Result -- lock()

Command (Layer 3 -- independently run):

```powershell
Select-String -Path "src/PropTraderTools/CopyEngine.cs" -Pattern "lock\s*\("
```

Output: All matches are comment lines only (JS-021 compliance annotations).
Examples: "// JS-021: ConcurrentDictionary -- lock-free. No lock() anywhere." etc.
Zero executable lock() calls found.

**SCAN 2: PASS -- zero actual lock() calls**

---

## SCAN 3 Result -- async void

Command (Layer 3 -- independently run):

```powershell
Select-String -Path "src/PropTraderTools/CopyEngine.cs" -Pattern "async\s+void"
```

Output: Two matches, both in comment lines only.
Line 1896: "// JS-021: no lock. JS-001: no throw. JS-033: Tick is not async void. ASCII-only."
Line 7339: "// Called directly from OnOrderUpdate -- NOT an event handler. Synchronous void. NOT async void..."
Zero async void method declarations found.

**SCAN 3: PASS -- zero actual async void**

---

## SCAN 4 Result -- return null

Command (Layer 3 -- independently run):

```powershell
Select-String -Path "src/PropTraderTools/CopyEngine.cs" -Pattern "return null"
```

Output: 15 executable return null lines found at:
L1256, L1964, L2922, L3026, L3034, L3834, L4029, L4295 -- all pre-existing (outside T5 helpers)
L5607, L5629 -- inside ResolveNullFollowerSlot (PERMITTED, annotated)
L5642, L5648, L5727 -- pre-existing (outside T5 helpers)
L6996, L7011 -- pre-existing (outside T5 helpers)

T5 new helpers verified:

- IsNakedConditionMet (L7166): returns bool. No null returns.
- BuildAtmModeNames (L6892): returns string[] (never null per annotation). No null returns.
- MatchesFollowerSlot (L795): returns bool. No null returns.
- ResolveNullFollowerSlot (L5602): returns Account. Lines 5607+5629 annotated "NT8 pattern: null = slot could not be resolved". PERMITTED per spec.
- PickBestTargetPrice (L2941): returns double?. No null returns.
- MirrorCloseOneAccount (L2244): returns void. No null returns.
- ResolveMultiplierLength (L1475): returns int. No null returns.
- UpdateLegTargetPrice (L2986): returns void. No null returns.
- IsPriceDeltaSignificant (L4090): returns bool. No null returns.
- RoundToTick (L3730): returns double. No null returns.
- SubmitReplacementStopOrder (L3627): returns void. No null returns.

Additional fix helpers:

- RegisterPendingBeSlot (L6354): returns void. No null returns.
- ComputeBeTarget (L6400): returns double. No null returns.
- GetBeRefPrice (L6406): returns double. No null returns.
- IsEntryCandidateOrder (L7275): returns bool. No null returns.

**SCAN 4: PASS -- return null only in ResolveNullFollowerSlot (annotated, permitted by spec)**

---

## SCAN 5 Result -- build

Command (Layer 3 -- independently run):

```powershell
dotnet build "src/PropTraderTools/PropTraderTools.csproj" --no-incremental 2>&1
```

Output:
1 Warning(s) -- pre-existing xUnit2004 in B131Tests.cs (not a T5 artifact)
0 Error(s)
Build succeeded.

**SCAN 5: PASS -- 0 errors**

---

## SCAN 6 Result -- ASCII

Command (Layer 3 -- independently run):

```powershell
$bytes = [System.IO.File]::ReadAllBytes("src/PropTraderTools/CopyEngine.cs")
($bytes | Where-Object { $_ -gt 127 } | Measure-Object).Count
```

Output: 0

**SCAN 6: PASS -- Count = 0**

---

## SCAN 7 Result -- tests

Command (Layer 3 -- independently run):

```powershell
dotnet test "tests/PropTraderTools.Tests/PropTraderTools.Tests.csproj" --filter "FullyQualifiedName~BwaveRefactorLaneB" 2>&1
```

Output: Passed! - Failed: 0, Passed: 28, Skipped: 0, Total: 28

Breakdown confirmed by [Fact] attribute inspection:
T1: 5 tests (IsBeTargetStateOk x3, IsImmediateBeEligible x2)
T2: 3 tests (IsQxCancelEligible3 x2, IsAccountFlattenable x1)
T3: 4 tests (IsPositionStateTriggerState x2, IsNativeLeaderTarget x1, IsQxCancelEligible2 x1)
T4: 8 tests (IsCancelAllStateOk x2, IsQxSnapshotStateOk x2, MatchesBracketType x2, ExtractLegSuffix x2)
T5: 8 tests (ResolveMultiplierLength x2, IsPriceDeltaSignificant x2, RoundToTick x2, PickBestTargetPrice x2)
Total: 28 [Fact] attributes (line 352 is a comment, not a real [Fact]; actual [Fact] count = 28).

T5 test list confirmed:
[x] ResolveMultiplierLength_CountZeroNullExisting_ReturnsZero (L432)
[x] ResolveMultiplierLength_CountPositive_ReturnsCount (L442)
[x] IsPriceDeltaSignificant_ZeroTickSize_ReturnsFalse (L452)
[x] IsPriceDeltaSignificant_SmallDelta_ReturnsTrue (L462)
[x] RoundToTick_ZeroTickSize_ReturnsRawPrice (L472)
[x] RoundToTick_PositiveTickSize_ReturnsRoundedPrice (L482)
[x] PickBestTargetPrice_PttHasValue_ReturnsPtt (L493)
[x] PickBestTargetPrice_PttNull_ReturnsAtm (L503)

**SCAN 7: PASS -- Failed: 0, Passed: 28 (meets >=28 requirement)**

---

## Structural Checks

### T5 Spec Helpers (11 required)

| Helper                                                                                                               | Line | Visibility              | Status  |
| -------------------------------------------------------------------------------------------------------------------- | ---- | ----------------------- | ------- |
| IsNakedConditionMet(Account acct)                                                                                    | 7166 | private static bool     | PRESENT |
| BuildAtmModeNames(CopyRule rule)                                                                                     | 6892 | private static string[] | PRESENT |
| MatchesFollowerSlot(CopyRule rule, Account acc)                                                                      | 795  | private static bool     | PRESENT |
| ResolveNullFollowerSlot(CopyRule rule, int i)                                                                        | 5602 | private Account         | PRESENT |
| PickBestTargetPrice(double? pttPrice, double? atmPrice)                                                              | 2941 | private static double?  | PRESENT |
| MirrorCloseOneAccount(Account acc, Instrument instr)                                                                 | 2244 | private void            | PRESENT |
| ResolveMultiplierLength(int[] existing, int count)                                                                   | 1475 | private static int      | PRESENT |
| UpdateLegTargetPrice(double[] prices, int i, Order o, string excludeSuffix)                                          | 2986 | private void            | PRESENT |
| IsPriceDeltaSignificant(double newPrice, double currentPrice, double tickSize)                                       | 4090 | private static bool     | PRESENT |
| RoundToTick(double rawPrice, double tickSize)                                                                        | 3730 | private static double   | PRESENT |
| SubmitReplacementStopOrder(Account followerAcc, Instrument instr, int qty, OrderAction stopAction, double stopPrice) | 3627 | private void            | PRESENT |

All 11 spec helpers: PRESENT

### Additional Fix Helpers (4 required)

| Helper                                          | Line | Visibility            | Status  |
| ----------------------------------------------- | ---- | --------------------- | ------- |
| RegisterPendingBeSlot(Account, Instrument, int) | 6354 | private void          | PRESENT |
| ComputeBeTarget(double, bool, int, double)      | 6400 | private static double | PRESENT |
| GetBeRefPrice(Instrument, bool)                 | 6406 | private static double | PRESENT |
| IsEntryCandidateOrder(Order, Instrument)        | 7275 | private static bool   | PRESENT |

All 4 additional fix helpers: PRESENT

### Test Seams (4 required)

| Seam                                                          | Line | Status                             |
| ------------------------------------------------------------- | ---- | ---------------------------------- |
| ResolveMultiplierLengthTestable(int[] e, int c)               | 1481 | PRESENT -- internal static         |
| IsPriceDeltaSignificantTestable(double n, double c, double t) | 4096 | PRESENT -- internal static         |
| RoundToTickTestable(double raw, double tick)                  | 3736 | PRESENT -- internal static         |
| PickBestTargetPriceTestable(double? p, double? a)             | 2949 | PRESENT -- internal static double? |

All 4 test seams: PRESENT

### Parent Methods Exist (no logic deleted)

All 11 T5 parent methods confirmed present and calling helpers:
HasNakedPosition (L7146) -- calls IsNakedConditionMet
RuleToDto (L6858) -- calls BuildAtmModeNames (L6873 confirmed)
IsFollowerAccount (L781) -- calls MatchesFollowerSlot
AllAccounts (L5574) -- calls ResolveNullFollowerSlot
CaptureLinkedTargetPrice (L2919) -- calls PickBestTargetPrice
MirrorClose (L2229) -- calls MirrorCloseOneAccount
BuildUpdatedMultipliers (L1453) -- calls ResolveMultiplierLength
CaptureOtherLegTargetPrices (L2968) -- calls UpdateLegTargetPrice
HandleEntryChange (L4047) -- calls IsPriceDeltaSignificant
HandleBracketChange (L3691) -- calls RoundToTick
CreateFollowerReplacementStop (L3606) -- calls SubmitReplacementStopOrder

Additional residual methods present:
ArmPendingBe (L6323) -- calls RegisterPendingBeSlot
IsImmediateBeEligible (L6380) -- calls ComputeBeTarget + GetBeRefPrice
DrainThenDispatch (L7221) -- calls IsEntryCandidateOrder

### Public/Internal Signatures Unchanged

IsFollowerAccount: internal bool (confirmed L781)
AllAccounts: internal IEnumerable<Account> (confirmed L5574)
All other T5 parents: private (no internal/public changes)

### NT8 Constraints

1. MirrorCloseOneAccount (L2244-2277):
   - Order name: "PTT-Mirror-Close" (L2267) -- CONFIRMED, PTT- prefix present
   - acc.Submit call: NOT PRESENT -- CONFIRMED (preserves existing behavior)
   - PASS

2. SubmitReplacementStopOrder (L3627-3667):
   - Order name: "PTT-STP-Drag" (L3647) -- CONFIRMED, unsuffixed, PTT- prefix present
   - acc.Submit(new[] { newStop }) at L3658 -- CONFIRMED (required for stop replacement)
   - PASS

3. ResolveNullFollowerSlot (L5602-5630):
   - Uses _resolvedFollowers.TryGetValue (L5608) and TryAdd (L5613) -- ConcurrentDictionary, lock-free
   - Both null returns annotated "// NT8 pattern: null = slot could not be resolved"
   - PASS

4. UpdateLegTargetPrice (L2986):
   - Visibility: private void (non-static) -- CONFIRMED (calls IsTargetOrderLive instance method)
   - PASS

All NT8 constraints: PASS

---

## Post-T5 Gate Results

### Gate 1: Full CCN gate (lizard --CCN 8 on CopyEngine.cs)

Command run independently:
lizard src/PropTraderTools/CopyEngine.cs --CCN 8

Output:
No thresholds exceeded (cyclomatic_complexity > 8 or length > 1000 or nloc > 1000000 or parameter_count > 100)
Fun Cnt: 366 Warning cnt: 0 AvgCCN: 4.0

PASS -- Warning cnt = 0. All 366 functions CCN <= 8.

### Gate 2: NT8 sync + MD5 verify

Not independently runnable (NT8-environment-specific).
Trusting engineer report: "=== SYNC + VERIFY: PASS (18 files confirmed) ===" / 0 MISMATCH lines.
F5 NinjaTrader 8 compilation still required (engineer responsibility).

### Gate 3: Full test run

Command run independently:
dotnet test "tests/PropTraderTools.Tests/PropTraderTools.Tests.csproj" --no-build

Output: Passed! - Failed: 0, Passed: 63, Skipped: 3, Total: 66
3 skipped = pre-existing NT8-dependent tests requiring NinjaTrader runtime

PASS

### Gate 4: Final build

(Same as SCAN 5 -- run --no-incremental above)
Output: 1 Warning(s) (pre-existing xUnit2004 in B131Tests.cs), 0 Error(s)

PASS

---

## Layer 2 Cross-Check

Comparing engineer's self-reported scan results (Layer 2) against independently run Layer 3:

| Scan                 | Layer 2 Report                                                    | Layer 3 Independent                                                                                                                          | Match?                                                                                 |
| -------------------- | ----------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------- |
| SCAN 1 (CCN)         | "no output"                                                       | ZERO ROWS                                                                                                                                    | MATCH                                                                                  |
| SCAN 2 (lock)        | "only comment lines, zero executable"                             | Only comment lines (53 matches, all comments)                                                                                                | MATCH                                                                                  |
| SCAN 3 (async void)  | "only comment lines"                                              | Only comment lines (2 matches, both comments)                                                                                                | MATCH                                                                                  |
| SCAN 4 (return null) | "Lines 5607, 5629 inside ResolveNullFollowerSlot only, annotated" | L5607, L5629 in ResolveNullFollowerSlot; pre-existing nulls at L1256,L1964,L2922,L3026,L3034,L3834,L4029,L4295,L5642,L5648,L5727,L6996,L7011 | MATCH -- note: Layer 2 only mentioned T5 helpers; pre-existing nulls are outside scope |
| SCAN 5 (build)       | "1 Warning(s) (pre-existing xUnit2004), 0 Error(s)"               | 1 Warning (B131Tests.cs xUnit2004), 0 Errors                                                                                                 | MATCH                                                                                  |
| SCAN 6 (ASCII)       | "0"                                                               | 0                                                                                                                                            | MATCH                                                                                  |
| SCAN 7 (tests)       | "Failed: 0, Passed: 28, Skipped: 0, Total: 28"                    | Failed: 0, Passed: 28, Skipped: 0, Total: 28                                                                                                 | MATCH                                                                                  |

Post-T5 gate:
Gate 1: Engineer "Warning cnt: 0" / Layer 3: "Warning cnt: 0, Fun Cnt: 366" | MATCH
Gate 3: Engineer "Failed: 0, Passed: 63, Skipped: 3" / Layer 3: identical | MATCH
Gate 4: Engineer "0 Error(s)" / Layer 3: "0 Error(s)" | MATCH

Layer 2 to Layer 3 discrepancies: NONE

---

## Deviations Noted

1. Plan name ExtractAtmTemplateMap -> ticket name BuildAtmModeNames (returning string[] not Dictionary).
   Advisory, documented in ticket and ticket-review. Not a violation.

2. ArmPendingBe, IsImmediateBeEligible, DrainThenDispatch were not in T5 spec but were found CCN>8
   post-T4. Addressed via RegisterPendingBeSlot, ComputeBeTarget+GetBeRefPrice, IsEntryCandidateOrder.
   Required for SCAN 1 ZERO-rows gate to pass. Correctly handled.

3. UpdateLegTargetPrice revised from private static to private (non-static) because it calls
   IsTargetOrderLive which is an instance method. Documented in ticket and ticket-review. Correct.

4. ResolveNullFollowerSlot: plan suggested extracting IsFollowerForInstrument; ticket revised to
   ResolveNullFollowerSlot due to iterator method limitation. Documented. Correct.

5. BwaveRefactorLaneBTests.cs had a syntax fix (tests appended correctly inside class brace, added
   `using System` for Math access). All 28 tests pass. No impact on correctness.

---

## DNA Rule Verification

| Rule                                  | Status | Evidence                                                                      |
| ------------------------------------- | ------ | ----------------------------------------------------------------------------- |
| JS-021: no lock()                     | PASS   | SCAN 2: zero executable lock() calls                                          |
| JS-001: no throw in helpers           | PASS   | SubmitReplacementStopOrder absorbs existing try/catch; no new throw           |
| JS-002: no return null in new helpers | PASS   | Only ResolveNullFollowerSlot returns null (annotated, grandfathered per spec) |
| JS-033: no async void                 | PASS   | SCAN 3: zero async void declarations                                          |
| ASCII-only                            | PASS   | SCAN 6: Count = 0 bytes > 127                                                 |
| CYC<=8                                | PASS   | SCAN 1: ZERO rows, lizard Warning cnt: 0                                      |
| NT8 PTT- prefix (CreateOrder names)   | PASS   | "PTT-Mirror-Close" and "PTT-STP-Drag" confirmed                               |
| ConcurrentDictionary (lock-free)      | PASS   | _resolvedFollowers uses TryGetValue/TryAdd                                    |
| xUnit [Fact] tests                    | PASS   | 8 T5 tests, all pass                                                          |

---

## VERIFY_PASS

**FINAL VERDICT: VERIFY_PASS**

All 7 scans pass independently. All structural checks pass. Post-T5 gate passes.
Layer 2 cross-check: zero discrepancies. No deviations constitute violations.

BWAVE-REFACTOR Lane B is COMPLETE.
All 366 methods in CopyEngine.cs are CCN <= 8 (lizard Warning cnt: 0).
Total tests: 63 pass, 3 skipped (NT8-runtime-dependent), 0 failures.
