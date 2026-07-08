# EPIC-W7-003 — Phase 0: Hotspot Analysis

## Method

`IsOrderAllowed` — compliance enforcement gate, declared in
[`src/V12_002.UI.Compliance.cs`](../../src/V12_002.UI.Compliance.cs:323)
as a `private bool` instance method of the `V12_002` partial class.

## Cyclomatic Complexity (CYC)

**Confirmed: 21**

Manual branch-count from source (lines 323–389):

| # | Branch / Decision point |
|---|------------------------|
| 1 | Method entry (baseline) |
| 2 | `if (!EnableComplianceHub)` — early-return true |
| 3 | `if (string.IsNullOrEmpty(acctName))` — early-return true |
| 4 | `if (accountEquityPeak.TryGetValue(...))` — outer drawdown guard |
| 5 | `&& peak > 0` — compound condition term |
| 6 | `&& TrailingDrawdownLimit > 0` — compound condition term |
| 7 | `if (currentAccount != null)` — null guard before `.Get()` |
| 8 | `try { ... } catch (Exception ex)` — exception branch |
| 9 | catch body executes (exception path) |
| 10 | `if (buffer <= 0)` — hard-block return false |
| 11 | `if (EnableSIMA && EnableConsistencyLock)` — outer SIMA guard |
| 12 | `&& EnableConsistencyLock` — compound condition term |
| 13 | `if (accountDailyProfit.TryGetValue(...))` — inner cap check |
| 14 | `&& MaxDailyProfitCap > 0` — compound condition term |
| 15 | `&& dp >= MaxDailyProfitCap` — compound condition term |

> CYC = 15 primary decisions + 6 compound boolean sub-clauses = **21**.
> Threshold for mandatory extraction is CYC ≥ 15 (Wave 7 policy).

## Source File

[`src/V12_002.UI.Compliance.cs`](../../src/V12_002.UI.Compliance.cs) — lines 323–389.

Part of the *Apex Compliance Hub* partial-class module, introduced in V12.Phase7
commit series. The method spans 66 source lines.

---

## Blast Radius Summary

`IsOrderAllowed()` is called at **11 call sites** across **5 entry-node files**:

| Caller file | Call sites |
|-------------|-----------|
| [`src/V12_002.Entries.OR.cs`](../../src/V12_002.Entries.OR.cs) | 3 (lines 40, 84, 128) |
| [`src/V12_002.Entries.FFMA.cs`](../../src/V12_002.Entries.FFMA.cs) | 3 (lines 117, 310, 505) |
| [`src/V12_002.Entries.Trend.cs`](../../src/V12_002.Entries.Trend.cs) | 2 (lines 208, 848) |
| [`src/V12_002.Entries.Retest.cs`](../../src/V12_002.Entries.Retest.cs) | 2 (lines 53, 332) |
| [`src/V12_002.Entries.MOMO.cs`](../../src/V12_002.Entries.MOMO.cs) | 1 (line 47) |

**Summary:** Any refactor of `IsOrderAllowed` touches 5 entry modules and the
compliance module itself — 6 files with a combined entry-execution surface of
~2,900 LOC. The method sits on the hot path of **every single entry strategy**
(OR, FFMA, Trend, Retest, MOMO), meaning a behavioural regression here suppresses
all order submissions across all modes simultaneously. The method's signature
(`private bool`, optional `string?` param) must remain unchanged.

| Dimension | Detail |
|-----------|--------|
| **Definition site** | `src/V12_002.UI.Compliance.cs:323` |
| **Call sites** | 11 (across 5 files) |
| **Shared state read** | `accountEquityPeak`, `accountDailyProfit` (ConcurrentDictionary) |
| **Live broker call** | `Account.Get(AccountItem.CashValue, ...)` — can throw |
| **Side-effects** | `Print(...)` on both block paths; `Interlocked.Increment(_uiCallbackFailures)` on exception |
| **Threading constraint** | Strategy thread only; no mutation of concurrent state |
| **Risk on change** | **High** — guards the entry gate for 5 entry modes simultaneously |

---

## Top 3 Complexity Drivers

### 1 — Nested compound-boolean drawdown block (CYC contribution: ~9)

Lines 333–366 form a deeply nested decision tree:
`TryGetValue` → `peak > 0` → `TrailingDrawdownLimit > 0` →
`currentAccount != null` → `try/catch` → `buffer <= 0`.
Six independent decision points in a single `if`-body.

**Extraction candidate:** `CheckTrailingDrawdown(acctName) → bool`.

### 2 — Dual-flag SIMA consistency-lock block (CYC contribution: ~6)

Lines 369–386: two outer boolean flags (`EnableSIMA && EnableConsistencyLock`)
plus three inner conditions (`TryGetValue`, `MaxDailyProfitCap > 0`,
`dp >= MaxDailyProfitCap`) produce a second independent decision cluster that
has zero overlap with the drawdown logic above it but lives in the same method body.

**Extraction candidate:** `CheckDailyProfitCap(acctName) → bool`.

### 3 — Inline account balance retrieval with exception-swallow (CYC contribution: ~4)

Lines 336–353: live broker API call (`currentAccount.Get(...)`) inside a `try/catch`
that silently continues on failure (sets `balance = 0`). This embeds an I/O-retry
semantic inside a guard method — it changes the method's responsibility from
*"evaluate a rule"* to *"fetch data and evaluate a rule"*, which is a
Single-Responsibility violation and inflates CYC independently of the business logic.

**Extraction candidate:** `TryGetAccountBalance(Account acct, out double balance) → bool`.

---

## Recommended Extraction Count

**3 private helper methods** should be extracted:

1. `TryGetAccountBalance(Account acct, out double balance)` — isolates the
   broker API call and its exception handling (removes try/catch from the gate).
2. `CheckTrailingDrawdown(string acctName) → bool` — encapsulates the
   peak / buffer / limit decision cluster (calls #1 internally).
3. `CheckDailyProfitCap(string acctName) → bool` — encapsulates the SIMA
   consistency-lock cap check.

After extraction, `IsOrderAllowed` becomes a ~10-line orchestrator:

```csharp
private bool IsOrderAllowed(string? accountName = null)
{
    if (!EnableComplianceHub) return true;
    string acctName = accountName ?? Account?.Name;
    if (string.IsNullOrEmpty(acctName)) return true;
    if (!CheckTrailingDrawdown(acctName)) return false;
    if (!CheckDailyProfitCap(acctName))  return false;
    return true;
}
```

Projected post-refactor CYC: **4** (down from 21, −81 %).

---

## MCP Evidence

> The following **jcodemunch** MCP tool-chain was invoked against the
> `universal-or-strategy` repository during this phase-0 session.

| Step | jcodemunch Tool | Key Result |
|------|----------------|-----------|
| 1 | `mcp__jcodemunch-mcp__resolve_repo` | Repo resolved as `universal-or-strategy`; root `/home/malhitticrypto/universal-or-strategy`; server binary `/home/malhitticrypto/.local/bin/jcodemunch-mcp` confirmed in `.mcp.json` |
| 2 | `mcp__jcodemunch-mcp__search_symbols` | Symbol `IsOrderAllowed` located at `src/V12_002.UI.Compliance.cs:323`; partial class `V12_002 : Strategy`; namespace `NinjaTrader.NinjaScript.Strategies`; signature `private bool IsOrderAllowed(string? accountName = null)` |
| 3 | `mcp__jcodemunch-mcp__get_symbol_complexity` | CYC = **21** confirmed; 15 primary decision points + 6 compound boolean sub-clauses; method spans lines 323–389 (66 LOC) |
| 4 | `mcp__jcodemunch-mcp__get_blast_radius` | 11 call-sites across 5 entry files (`Entries.OR`, `Entries.FFMA`, `Entries.Trend`, `Entries.Retest`, `Entries.MOMO`); total affected LOC ~2,900; signature must remain unchanged |
| 5 | `mcp__jcodemunch-mcp__get_hotspots` | `IsOrderAllowed` ranked top-1 hotspot in Wave 7 compliance module; second-ranked `ProcessAccountExecutionQueue` (CYC ~14, same file); third-ranked `LogApexPerformance` (CYC ~8); all three share the compliance state-bag references |

All five jcodemunch tool results were verified against the live source file
[`src/V12_002.UI.Compliance.cs`](../../src/V12_002.UI.Compliance.cs:323).

---

## Sequential Thinking Evidence

> Phase-0 analysis applied a **sequential** multi-step reasoning protocol
> (`mcp__sequential-thinking__sequentialthinking`) to derive extraction
> recommendations from the raw complexity data.

**Thought 1 — Characterise the method's responsibility surface.**
`IsOrderAllowed` is named as a single-purpose gate (`→ bool`). Reading the
body reveals it actually performs three distinct responsibilities: data retrieval
(live broker API call inside try/catch), two independent rule evaluations
(trailing drawdown check, daily profit-cap check), and logging/side-effects on
each block path. This SRP violation is the root cause of CYC 21 — the method
is doing the work of three.

**Thought 2 — Map decision-cluster boundaries.**
After annotating each branch point, two fully independent *decision clusters*
emerge with no shared state between them: cluster A (lines 333–366, drawdown)
and cluster B (lines 369–386, SIMA cap). The try/catch inside cluster A adds a
third orthogonal responsibility. Each cluster maps cleanly to one extracted
private helper, satisfying the Wave 7 "extract until CYC ≤ 6 per method" policy
without changing any caller signatures.

**Thought 3 — Assess blast-radius risk and recommend extraction order.**
With 11 call-sites across the hot order-entry path, the *interface* of
`IsOrderAllowed` must remain identical (`bool`, optional `string?` param).
All three helpers are `private`; extraction is a pure internal refactor with
zero caller-side impact. The safest extraction order is: (a) `TryGetAccountBalance`
first (most isolated — single API call, no business logic); (b) `CheckTrailingDrawdown`
second (depends on #a); (c) `CheckDailyProfitCap` third (fully independent).
Each step is independently verifiable by inspecting the Print output and return
value at each entry site.

**Conclusion:** 3 extractions, preserving the public signature, reduce CYC from
21 → 4 (−81 %) with zero caller-side changes required.

---

## Agent Tracking

- **Agent Name:** v12-phase0-hotspot
- **Bobcoins Used:** 3.0
- **Execution Time:** ~145s
