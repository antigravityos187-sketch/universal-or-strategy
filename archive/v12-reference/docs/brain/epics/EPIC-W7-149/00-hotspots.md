# EPIC-W7-149 Hotspot Analysis

**Method:** LogApexPerformance
**CYC:** 20
**File:** src/V12_002.UI.Compliance.cs

---

## Overview

`LogApexPerformance` (lines 810–913) is the compliance hub's primary JSON serialisation and disk-flush
routine. It aggregates account health metrics across the active fleet, builds a multi-account JSON
payload using raw `StringBuilder` concatenation, and fires a `Task.Run` fire-and-forget write to a
compliance log file. Its CYC of 20 arises from the accumulation of guard conditions at the top,
a `foreach`-over-accounts loop with several per-account decision paths, nested ternary expressions
for position direction and connection status, and a two-layer `try/catch` structure (outer + inner
lambda). There are no previously extracted helpers — the method is fully monolithic as delivered.

---

## Blast Radius Summary

| Dimension | Detail |
|---|---|
| **Direct callers** | `ProcessAccountExecutionQueue` (line 478, same file); `ProcessComplianceTracking` (line 574, `src/V12_002.Orders.Callbacks.Execution.cs`) |
| **Caller chain** | `OnAccountExecutionUpdate` → `ProcessAccountExecutionQueue` → `LogApexPerformance`; `OnExecutionUpdate` → `ProcessComplianceTracking` → `LogApexPerformance` |
| **Callees inside method** | `GetComplianceAccounts`, `GetComplianceNow`, `MaybeFinalizeDailySummaries`, `UpdateAccountMetricsFromAccount`, `GetUniqueTradingDays`, `PathValidation.ValidateAndCanonicalize` |
| **Shared state read** | `accountDailyProfit`, `accountTotalProfit`, `accountTradeCount`, `accountMaxDrawdown`, `expectedPositions` (ConcurrentDictionary), `complianceLogPath`, `lastComplianceLog` |
| **Shared state written** | `lastComplianceLog` (stamped before the `Task.Run`; not guarded by the inner lambda) |
| **External I/O** | `System.IO.File.WriteAllText` via `Task.Run` — writes `ApexPerformance_<symbol>.json` to disk (path initialised in `src/V12_002.Lifecycle.cs` line 574) |
| **Threading constraint** | Strategy thread only for the outer body; fire-and-forget `Task.Run` lambda executes on the thread-pool |
| **Risk on change** | Medium — `lastComplianceLog` is mutated on the strategy thread *before* the async lambda runs; extraction of the payload-building loop must not introduce a second `lastComplianceLog` write. `UpdateAccountMetricsFromAccount` is called per-account inside the loop which triggers `PublishUiSnapshot` on WPF UI (side-effect coupling). |

**Affected symbol count (blast radius):** 8 symbols directly coupled; 7 shared state fields.

---

## Top 3 Complexity Drivers

### 1. Per-account loop with multi-branch position-direction ternary (≈ 7 CYC points)

The `foreach (Account acct in accounts)` loop (line 832) accumulates the largest share of CYC:

- `if (acct == null) continue` — null guard (+1)
- `if (count > 0)` — JSON comma separator (+1)
- Outer ternary: `(brokerPos != null && brokerPos.MarketPosition != MarketPosition.Flat)` — two
  operands of `&&` each count as a branch (+2 in strict mode)
- Inner ternary: `MarketPosition == MarketPosition.Long ? qty : -qty` (+1)
- `if (expectedPositions != null)` — defensive null on shared dict (+1)
- `(isConnected ? "Connected" : "Disconnected")` — connection status ternary (+1)

This single `foreach` block accounts for approximately **7 CYC** without any business logic —
it is structural overhead from inline JSON building and defensive null-guarding.

**Recommended extraction:** `BuildAccountJsonEntry(Account acct, int count) → string`
Reduces the loop body to a single delegating call, collapses all ternaries and guards into one
focused helper, and cuts the outer method CYC by ~6 points.

---

### 2. Two-layer try/catch structure with fire-and-forget lambda (≈ 6 CYC points)

The method contains a nested exception-handling structure:

- Outer `try` (line 819) wraps the entire serialisation block (+1)
- Outer `catch (Exception ex)` at line 909 (+1)
- `Task.Run` lambda introduces an implicit execution path (+1)
- `if (path != null)` inside lambda (+1)
- Inner `catch (SecurityException ex)` (line 900) (+1)
- Inner catch-all swallow `catch` (line 903) (+1)

The two-layer structure (outer catches serialisation failures; inner catches I/O failures) is
correct in intent but inflates CYC without contributing branching to observable business state.
The lambda captures `path` and `jsonPayload` by value, but `lastComplianceLog` is written on the
caller thread before the lambda executes — a subtle ordering dependency that makes extraction risky
without explicit sequencing documentation.

**Recommended extraction:** `WriteComplianceJsonAsync(string path, string payload)` — encapsulates
the `Task.Run + try/catch + ValidationAndWrite` chain, removing ~4 CYC from the outer method.

---

### 3. Compound guard conditions at method entry (≈ 4 CYC points)

The top two guards add disproportionate branching relative to their size:

- `if (!EnableComplianceHub || string.IsNullOrEmpty(complianceLogPath))` — the `||` short-circuit
  creates two independent exit paths (+2 in branch-count mode)
- `if ((DateTime.Now - lastComplianceLog).TotalSeconds < 5)` — throttle guard (+1)
- Implicit fall-through to the `try` body as a distinct path (+1)

These four paths represent pure gate logic that precedes any business work. Extracting them into
`private bool ShouldSkipComplianceLog()` collapses them to a single call-site condition and makes
the throttle interval (5 s hard-coded on line 816) independently testable without loading
the full account enumeration context.

---

## Recommended Extraction Count

**3 helper extractions recommended:**

| # | Proposed Helper | CYC Reduction | Notes |
|---|---|---|---|
| 1 | `BuildAccountJsonEntry(Account acct, int index) → string` | ~6 | Absorbs position ternaries, null guards, connection check |
| 2 | `WriteComplianceJsonAsync(string path, string payload)` | ~4 | Encapsulates `Task.Run` + two-layer try/catch |
| 3 | `ShouldSkipComplianceLog() → bool` | ~3 | Gate guard: enabled-flag + path-null + throttle |

Post-extraction target CYC for `LogApexPerformance`: **≤ 7**
(base 1 + outer try + foreach null-guard + 3 delegating calls + outer catch = 7)

---

## Agent Tracking

Agent Name: v12-phase0-hotspot | Bobcoins Used: 1.0 | Execution Time: ~60s
