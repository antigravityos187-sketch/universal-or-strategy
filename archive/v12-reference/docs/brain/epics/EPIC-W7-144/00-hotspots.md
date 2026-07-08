# EPIC-W7-144 Hotspot Analysis

**Method:** `IsOrderAllowed`
**CYC:** 20
**File:** `src/V12_002.UI.Compliance.cs`

---

## Overview

[`IsOrderAllowed`](src/V12_002.UI.Compliance.cs:323) is the compliance enforcement gate called at
the start of every entry method across the strategy. It returns `false` when any hard compliance
limit is breached, preventing order submission. Despite being only ~66 lines, it scores CYC 20
due to three compound boolean guards (each multi-term `&&` chain counts individually under
modified McCabe), a try/catch block, two nested if-blocks, and null-coalescing/null-conditional
operators on the account reference.

---

## Blast Radius Summary

| Dimension | Detail |
|---|---|
| **Direct callers** | 11 call sites across 5 entry files (see below) |
| **Caller files** | `V12_002.Entries.FFMA.cs` (3×), `V12_002.Entries.Trend.cs` (2×), `V12_002.Entries.OR.cs` (3×), `V12_002.Entries.Retest.cs` (2×), `V12_002.Entries.MOMO.cs` (1×) |
| **Shared state read** | `accountEquityPeak`, `accountDailyProfit` (ConcurrentDictionary), `EnableComplianceHub`, `EnableSIMA`, `EnableConsistencyLock`, `TrailingDrawdownLimit`, `MaxDailyProfitCap` |
| **External dependency** | `Account.Get(AccountItem.CashValue, ...)` — live broker API call inside try/catch |
| **Side-effects** | `Interlocked.Increment(ref _uiCallbackFailures)` on catch path; `Print(...)` on every block path |
| **Threading constraint** | Strategy thread (called from entry methods which are always on-strategy-thread) |
| **Risk on change** | HIGH — any refactor must preserve exact short-circuit semantics; a false `true` return on any branch allows order submission and bypasses compliance enforcement |

**Affected symbol count (blast radius):** 11 call sites across 5 files; 7 shared state fields; 1 external broker API dependency.

---

## Top 3 Complexity Drivers

### 1. Trailing-Drawdown Block — Triple-term compound guard + nested try/catch (≈8 CYC)

```
if (accountEquityPeak.TryGetValue(acctName, out double peak) && peak > 0 && TrailingDrawdownLimit > 0)
{
    if (currentAccount != null)
    {
        try { balance = currentAccount.Get(...); }
        catch (Exception ex) { ... Interlocked.Increment(...); }
    }
    if (buffer <= 0) { ...; return false; }
}
```

The outer `&&` chain contributes +3 (one per term under modified McCabe). The `currentAccount !=
null` guard adds +1, the `catch` block adds +1, and `buffer <= 0` adds +1. The null-conditional
`?.` in `Account?.Name` on line 328 adds +1. Total: **~7–8 CYC** from this block alone.

### 2. SIMA/Consistency-Lock double-nested compound guard (≈6 CYC)

```
if (EnableSIMA && EnableConsistencyLock)
{
    if (accountDailyProfit.TryGetValue(acctName, out double dp)
        && MaxDailyProfitCap > 0
        && dp >= MaxDailyProfitCap)
    { ...; return false; }
}
```

The outer `&&` adds +2, the inner triple-term `&&` adds +3. The `return false` path is an
additional decision point. Total: **~5–6 CYC** from this block.

### 3. Entry-guard null-coalescing chain + early-return guards (≈4 CYC)

```
if (!EnableComplianceHub) return true;           // +1
string acctName = accountName ?? Account?.Name;  // ?? = +1, ?. = +1
if (string.IsNullOrEmpty(acctName)) return true; // +1
```

Three decision points at the method preamble (feature flag guard, null-coalesce fallback,
empty-name guard) contribute ~4 CYC before any business logic executes. These are structurally
necessary but represent the "invisible" complexity overhead pattern common across this codebase.

---

## Recommended Extraction Count

**2 helper extractions recommended.**

| Extraction | Proposed Name | Responsibility | Estimated CYC Reduction |
|---|---|---|---|
| 1 | `IsTrailingDrawdownBreached(string acctName)` | Encapsulates the equity-peak lookup, live balance fetch (try/catch), buffer computation, and Print log. Returns `bool`. | Removes ~8 CYC from `IsOrderAllowed`; new helper carries ~7 CYC independently. |
| 2 | `IsDailyProfitCapHit(string acctName)` | Encapsulates the SIMA+ConsistencyLock guard, `accountDailyProfit` lookup, cap comparison, and Print log. Returns `bool`. | Removes ~5 CYC from `IsOrderAllowed`; new helper carries ~4 CYC independently. |

**Post-extraction projected CYC for `IsOrderAllowed`:** ~5 (feature-flag guard, null-coalesce,
empty-name guard, two delegating `if (!helper()) return false` calls).

**Rationale:** The entry-guard preamble (Driver 3) should remain inline — it is idiomatic
NinjaScript early-return pattern and extracting it would provide zero readability gain. The two
hard-block business rules (drawdown and profit cap) are semantically independent, have distinct
logging paths, and each wraps its own external dependency or feature-flag pair; they are natural
extraction targets.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase0-hotspot |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | ~60s |
