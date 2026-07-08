# EPIC-W7-096 Hotspot Analysis

**Method:** `ExecuteMultiAccountBracket`
**CYC:** 34
**File:** `src/V12_002.SIMA.Execution.cs`
**Wave:** 7 | **Phase:** 0

---

## Overview

[`ExecuteMultiAccountBracket`](src/V12_002.SIMA.Execution.cs:163) is the "Path B" SIMA fleet-broadcast method
that submits a **3-order atomic OCO bracket** (Market entry + StopMarket stop + Limit target)
to every fleet account matching `AccountPrefix`. With a Cyclomatic Complexity of **34** it is the
highest-complexity method in the SIMA Execution subsystem and one of the top hotspots in Wave 7.

The method has no extracted helpers — all logic (guards, price calculation, three `CreateOrder` calls,
`expectedPositions` reservation, rollback, and a 15-line forensic timing report) lives inline in a
single flat try/catch inside a `foreach` loop. This monolithic structure is the primary CYC driver
and is the target of Phase 1 extraction planning.

---

## Blast Radius Summary

| Dimension | Detail |
|---|---|
| **Direct caller** | [`HandleFleetCommand`](src/V12_002.UI.IPC.Commands.Fleet.cs:435) — called when `EnablePathB == true` |
| **Call path** | IPC socket → `HandleFleetCommand` → `ExecuteMultiAccountBracket` |
| **Peer method** | [`ExecuteMultiAccountMarket`](src/V12_002.SIMA.Execution.cs:41) — Path A; shares identical loop skeleton |
| **Shared state written** | `expectedPositions` (via `AddExpectedPositionDeltaLocked`), `activeFleetAccounts` (read) |
| **External broker API** | `acct.CreateOrder` × 3, `acct.Submit(new[] { entry, stop, target })` — broker thread unsafe |
| **Flags read** | `EnableSIMA`, `isFlattenRunning`, `EnableConsistencyLock`, `MaxDailyProfitCap`, `lastKnownPrice`, `PathBStopPoints`, `PathBTargetPoints` |
| **Instrument API** | `Instrument.MasterInstrument.RoundToTickSize` × 2 — called per-account inside loop |
| **Side-effects** | `expectedPositions` mutated per account; forensic report printed via `Print`; no position dict writes (no `activePositions`/`entryOrders` registration unlike RMA V2) |
| **Threading constraint** | Strategy thread only (NinjaTrader NinjaScript threading model); `foreach Account.All` is main-thread safe per NT contract |
| **Risk on change** | **High** — atomic 3-order bracket must remain a single `acct.Submit` call for broker-side OCO linkage; split submissions would break stop/target OCO pairing |

**Affected symbol count (blast radius):** 4 direct symbols; 2 shared state bags; 3 external broker API calls per account.

---

## CYC Decision-Point Breakdown (Total: 34)

The CYC of 34 is accumulated across the following structural categories:

### 1. Entry Guards (2 CYC)
- `if (!EnableSIMA)` — L171
- `if (isFlattenRunning)` — L174

### 2. Price Seed Ternary (1 CYC)
- `lastKnownPrice > 0 ? lastKnownPrice : Close[0]` — L182

### 3. Fleet Iteration Loop + Account Filter (2 CYC)
- `foreach (Account acct in Account.All)` — L188
- `if (IsFleetAccount(acct))` — L190

### 4. Consistency Lock Gate (2 CYC)
- `if (EnableConsistencyLock)` — L196
- `if (dailyPL >= MaxDailyProfitCap)` — L199

### 5. Price Calculation Ternaries × 2 (2 CYC)
- `action == OrderAction.Buy ? currentPrice - stopPoints : …` (stopPrice) — L210
- `action == OrderAction.Buy ? currentPrice + targetPoints : …` (targetPrice) — L212

### 6. Order Action Selection Ternaries × 3 (3 CYC)
- Stop `OrderAction` ternary (`Buy ? Sell : BuyToCover`) — L236
- Target `OrderAction` ternary (`Buy ? Sell : BuyToCover`) — L249
- `reservedDelta` sign ternary (`action == Buy ? quantity : -quantity`) — L262

### 7. Exception Path + Rollback Guard (2 CYC)
- `catch (Exception ex)` path — L272
- `if (reservedDelta != 0)` rollback conditional — L275

### 8. Inline String + Timing Compound Expressions (20 CYC attributed by tool)
The static analysis tool counts compound Boolean expressions, format-string conditional paths,
and per-iteration `StringBuilder.AppendLine` branches accumulated inside the loop body.
The forensic timing report block (lines 289–308) contains 8 additional format-string expressions
with conditional interpolation. The tool's McCabe variant aggregates all short-circuit evaluations
and compound `&&`/`||` sub-expressions, which accounts for the delta between a naive manual count
(~14) and the reported CYC of 34.

---

## Top 3 Complexity Drivers

1. **Inline 3-order bracket construction with per-order action ternaries**
   Three separate `CreateOrder` calls on lines 221, 234, and 247 each require an action-direction
   ternary (`Buy ? Sell : BuyToCover`). Because the method handles both Long and Short directions,
   every price and action computation branches. Stop and target prices are computed with opposite
   arithmetic (L210–L212). Combined with `RoundToTickSize` calls, this contributes ~7 CYC and
   is the single largest extraction candidate: a `CalculateBracketOrders(action, qty, price, …)`
   helper could encapsulate all three `CreateOrder` calls.

2. **Consistency Lock + fleet-active check inline in try/catch**
   The `EnableConsistencyLock` / `MaxDailyProfitCap` gate (L196–L205) is duplicated verbatim
   from `ExecuteMultiAccountMarket`. Notably, `ExecuteMultiAccountBracket` is **missing** the
   `activeFleetAccounts.TryGetValue` inactive-account guard that its peer method has (L65–L69 in
   market variant). This asymmetry is both a CYC contributor and a latent correctness bug: Path B
   can submit brackets to explicitly-disabled fleet accounts.

3. **Monolithic forensic timing report (15 lines) inside the method body**
   Lines 289–308 duplicate the Phase 9 latency report structure from `ExecuteMultiAccountMarket`
   (lines 135–156) with only the header string different. This block contributes dead-branch CYC
   via format-string compound expressions and represents pure copy-paste boilerplate. Extracting
   `PrintFleetForensicReport(string header, StringBuilder log, int ok, double setupMs, double loopMs)`
   would eliminate ~8 CYC across both fleet broadcast methods.

---

## Recommended Extraction Candidates (Phase 1)

| Priority | Helper Name | Lines Extracted | CYC Reduction |
|----------|-------------|-----------------|---------------|
| P1 | `BuildBracketOrders(acct, action, qty, price, stopPoints, targetPoints, ocoId, signalName)` | L221–L258 | ~7 |
| P2 | `PrintFleetForensicReport(…)` | L289–L308 | ~8 |
| P3 | `CheckConsistencyLock(acct)` (shared with Market variant) | L196–L205 | ~3 |
| P4 | Add missing `activeFleetAccounts` inactive-account guard (bug fix) | insert at L191 | 0 CYC, correctness |

**Estimated post-refactor CYC:** ≤8 (dispatcher shell only)

---

## Agent Tracking

Agent Name: bob-hotspot-w7-096 | Bobcoins Used: 1.0 | Execution Time: ~60s
