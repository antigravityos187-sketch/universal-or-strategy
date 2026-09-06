# BWAVE-REFACTOR LaneB -- Ticket 5 Completion

# Phase 4a Output

# Author: ptt-engineer

# Ticket: BWAVE-REFACTOR-LaneB-T5

## Scope Confirmation

[TICKET 5 ONLY]

This is the FINAL ticket of BWAVE-REFACTOR Lane B. SCAN 1 (lizard --CCN 8) produces zero output
for the ENTIRE CopyEngine.cs file -- all 366 methods are CCN <= 8.

---

## New Helpers Added (T5 spec -- 11 required extractions)

| Helper                                                                                                                 | Visibility     | Parent Method                   | CCN |
| ---------------------------------------------------------------------------------------------------------------------- | -------------- | ------------------------------- | --- |
| `IsNakedConditionMet(Account acct)`                                                                                    | private static | `HasNakedPosition`              | <=4 |
| `BuildAtmModeNames(CopyRule rule)`                                                                                     | private static | `RuleToDto`                     | <=3 |
| `MatchesFollowerSlot(CopyRule rule, Account acc)`                                                                      | private static | `IsFollowerAccount`             | <=4 |
| `ResolveNullFollowerSlot(CopyRule rule, int i)`                                                                        | private        | `AllAccounts`                   | <=3 |
| `PickBestTargetPrice(double? pttPrice, double? atmPrice)`                                                              | private static | `CaptureLinkedTargetPrice`      | <=2 |
| `MirrorCloseOneAccount(Account acc, Instrument instr)`                                                                 | private        | `MirrorClose`                   | <=5 |
| `ResolveMultiplierLength(int[] existing, int count)`                                                                   | private static | `BuildUpdatedMultipliers`       | <=3 |
| `UpdateLegTargetPrice(double[] prices, int i, Order o, string excludeSuffix)`                                          | private        | `CaptureOtherLegTargetPrices`   | <=4 |
| `IsPriceDeltaSignificant(double newPrice, double currentPrice, double tickSize)`                                       | private static | `HandleEntryChange`             | <=2 |
| `RoundToTick(double rawPrice, double tickSize)`                                                                        | private static | `HandleBracketChange`           | <=2 |
| `SubmitReplacementStopOrder(Account followerAcc, Instrument instr, int qty, OrderAction stopAction, double stopPrice)` | private        | `CreateFollowerReplacementStop` | <=4 |

## Additional Helpers Added (T1-T4 residual CCN>8 cleanup + T5 final gate)

| Helper                                            | Visibility     | Parent Method           | Reason                                                          |
| ------------------------------------------------- | -------------- | ----------------------- | --------------------------------------------------------------- |
| `RegisterPendingBeSlot(Account, Instrument, int)` | private        | `ArmPendingBe`          | CCN 11->8: removed PendingBeArmed?.Invoke + ?? ops              |
| `ComputeBeTarget(double, bool, int, double)`      | private static | `IsImmediateBeEligible` | CCN 16->8: removed ternary in target calc                       |
| `GetBeRefPrice(Instrument, bool)`                 | private static | `IsImmediateBeEligible` | CCN 16->8: absorbed MarketData?.Bid/.Ask chained nullcoalescers |
| `IsEntryCandidateOrder(Order, Instrument)`        | private static | `DrainThenDispatch`     | CCN 11->5: moved LINQ Where predicate (6 logical ops) to helper |

## Test Seams Added

| Seam                                                            | Target Helper             |
| --------------------------------------------------------------- | ------------------------- |
| `ResolveMultiplierLengthTestable(int[] e, int c)`               | `ResolveMultiplierLength` |
| `IsPriceDeltaSignificantTestable(double n, double c, double t)` | `IsPriceDeltaSignificant` |
| `RoundToTickTestable(double raw, double tick)`                  | `RoundToTick`             |
| `PickBestTargetPriceTestable(double? p, double? a)`             | `PickBestTargetPrice`     |

---

## CCN Reduction

| Method                          | CCN Before | CCN After |
| ------------------------------- | ---------- | --------- |
| `HasNakedPosition`              | 9          | <=4       |
| `RuleToDto`                     | 9          | <=3       |
| `IsFollowerAccount`             | 9          | <=4       |
| `AllAccounts`                   | 9          | <=6       |
| `CaptureLinkedTargetPrice`      | 9          | <=5       |
| `MirrorClose`                   | 9          | <=3       |
| `BuildUpdatedMultipliers`       | 9          | <=5       |
| `CaptureOtherLegTargetPrices`   | 9          | <=4       |
| `HandleEntryChange`             | 9          | <=7       |
| `HandleBracketChange`           | 9          | <=7       |
| `CreateFollowerReplacementStop` | 9          | <=2       |
| `ArmPendingBe`                  | 11         | <=8       |
| `IsImmediateBeEligible`         | 16         | <=8       |
| `DrainThenDispatch`             | 11         | <=5       |

---

## 7-Scan Results

### SCAN 1 -- lizard CCN (ENTIRE FILE -- final ticket gate)

Command:

```powershell
$files = Get-ChildItem src/PropTraderTools/ -Filter "*.cs" -Recurse |
  Where-Object { $_.FullName -notmatch '\\obj\\' -and $_.FullName -notmatch '\\bin\\' }
lizard $files --csv 2>&1 | ConvertFrom-Csv -Header @(...) |
  Where-Object { [int]$_.CCN -gt 8 } | Format-Table -AutoSize
```

Output: (no output)
**PASS -- ZERO rows. No method anywhere in scanned files exceeds CCN 8.**

### SCAN 2 -- lock()

Command: `Select-String -Path "src/PropTraderTools/CopyEngine.cs" -Pattern "lock\s*\("`
Output: (only comment lines match, zero executable lock() calls)
**PASS -- zero actual lock() calls.**

### SCAN 3 -- async void

Command: `Select-String -Path "src/PropTraderTools/CopyEngine.cs" -Pattern "async\s+void"`
Output: (only comment lines match, zero async void methods)
**PASS -- zero actual async void.**

### SCAN 4 -- return null in new helpers

Command: `Select-String -Path "src/PropTraderTools/CopyEngine.cs" -Pattern "return null"`
Output: Lines 5607, 5629 inside `ResolveNullFollowerSlot` only (annotated with `// NT8 pattern: null = slot could not be resolved`). All other T5 helpers return bool, int, double, double?, string[], or void.
**PASS -- return null only in ResolveNullFollowerSlot (allowed by spec).**

### SCAN 5 -- build

Command: `dotnet build "src/PropTraderTools/PropTraderTools.csproj" --no-incremental 2>&1`
Output: `1 Warning(s)` (pre-existing xUnit2004 in B131Tests.cs), `0 Error(s)`
**PASS -- 0 errors.**

### SCAN 6 -- ASCII

Command: `$bytes = [System.IO.File]::ReadAllBytes(...); ($bytes | Where-Object { $_ -gt 127 } | Measure-Object).Count`
Output: `0`
**PASS -- Count = 0.**

### SCAN 7 -- tests

Command: `dotnet test "tests/PropTraderTools.Tests/PropTraderTools.Tests.csproj" --filter "FullyQualifiedName~BwaveRefactorLaneB" 2>&1`
Output: `Passed!  - Failed: 0, Passed: 28, Skipped: 0, Total: 28`
**PASS -- 28 tests pass (5 T1 + 3 T2 + 4 T3 + 8 T4 + 8 T5).**

---

## Post-T5 Verification Gate Results

### Gate 1: Full CCN gate (lizard --CCN 8)

Command: `lizard src/PropTraderTools/CopyEngine.cs --CCN 8`
Output: `No thresholds exceeded (cyclomatic_complexity > 8 ...) Warning cnt: 0`
**PASS**

### Gate 2: NT8 sync + MD5 verify

Command: `powershell -File scripts\ptt-sync-and-verify.ps1`
Output: `=== SYNC + VERIFY: PASS (18 files confirmed) ===`
**PASS -- 0 MISMATCH lines.**
Note: F5 in NinjaTrader 8 still required (mandatory compile step).

### Gate 3: Full test run

Command: `dotnet test "tests/PropTraderTools.Tests/PropTraderTools.Tests.csproj" --no-build`
Output: `Passed!  - Failed: 0, Passed: 63, Skipped: 3, Total: 66`
(3 skipped = pre-existing NT8-dependent tests requiring NinjaTrader runtime)
**PASS**

### Gate 4: Final build

Command: `dotnet build "src/PropTraderTools/PropTraderTools.csproj" --no-incremental`
Output: `1 Warning(s)` (pre-existing xUnit2004), `0 Error(s)`
**PASS**

---

## Test List (8 new T5 [Fact] tests)

| [Fact] Name                                                 | Covers                                                        |
| ----------------------------------------------------------- | ------------------------------------------------------------- |
| `ResolveMultiplierLength_CountZeroNullExisting_ReturnsZero` | `ResolveMultiplierLengthTestable(null, 0)` -> 0               |
| `ResolveMultiplierLength_CountPositive_ReturnsCount`        | `ResolveMultiplierLengthTestable(null, 3)` -> 3               |
| `IsPriceDeltaSignificant_ZeroTickSize_ReturnsFalse`         | `IsPriceDeltaSignificantTestable(100.0, 99.0, 0.0)` -> false  |
| `IsPriceDeltaSignificant_SmallDelta_ReturnsTrue`            | `IsPriceDeltaSignificantTestable(100.0, 100.0, 0.25)` -> true |
| `RoundToTick_ZeroTickSize_ReturnsRawPrice`                  | `RoundToTickTestable(100.123, 0.0)` -> 100.123                |
| `RoundToTick_PositiveTickSize_ReturnsRoundedPrice`          | `RoundToTickTestable(100.1, 0.25)` -> Math.Round formula      |
| `PickBestTargetPrice_PttHasValue_ReturnsPtt`                | `PickBestTargetPriceTestable(100.0, 99.0)` -> 100.0           |
| `PickBestTargetPrice_PttNull_ReturnsAtm`                    | `PickBestTargetPriceTestable(null, 99.0)` -> 99.0             |

---

## Deviations from Ticket Spec

1. **ArmPendingBe, IsImmediateBeEligible, DrainThenDispatch not in T5 spec** -- These 3 methods were CCN>8 but not listed in the T5 spec. Lizard's post-T5 verification gate (`lizard src/PropTraderTools/CopyEngine.cs --CCN 8`) flagged them. Since SCAN 1 requires ZERO output for the ENTIRE CopyEngine.cs file (final ticket gate), they were fixed:
   - `ArmPendingBe` CCN 11->8 via `RegisterPendingBeSlot` extraction
   - `IsImmediateBeEligible` CCN 16->8 via `ComputeBeTarget` + `GetBeRefPrice` extractions
   - `DrainThenDispatch` CCN 11->5 via `IsEntryCandidateOrder` extraction
     All extractions follow JS DNA (no lock, no async void, ASCII-only, CYC<=8).

2. **BwaveRefactorLaneBTests.cs syntax fix** -- T5 tests were appended outside the class brace (closing `}` at line 410 was pre-existing). Removed the duplicate class close to make T5 tests members of the correct class. Added `using System;` for `Math` access.

---

## BUILD_PASS
