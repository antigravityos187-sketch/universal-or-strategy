# EPIC-W7-151 Hotspot Analysis

**Method:** `IsOrderAllowed`
**CYC:** 9
**File:** `src/V12_002.UI.Compliance.cs`

> **Note:** `method_name` and `source_file` missing from epic list — using best-effort hotspot match.

---

## Overview

`IsOrderAllowed` (lines 323–389, `src/V12_002.UI.Compliance.cs`) is the compliance enforcement
gate called at the start of every entry method. It evaluates two hard-block rules — trailing
drawdown breach and daily profit cap — returning `false` when an account has breached a
severity-2 compliance limit. Its CYC of 9 arises from three tightly nested conditional layers
(feature flag guards → account null-guards → dual hard-block rule checks), each adding 2–3
branches, with a try/catch and balance-retrieval error path contributing the final two.

Neighboring epics span the same `UI.Compliance.cs` module and the `UI.IPC.Commands.*` files,
confirming this method's position as the dominant complexity hotspot within that subsystem at
the targeted CYC range of ~9.

---

## Blast Radius Summary

| Dimension | Detail |
|---|---|
| **Direct callers** | All entry-submission paths that call `IsOrderAllowed()` before submitting orders |
| **Caller chain** | `ExecuteRMAEntryV2`, `ExecuteMultiAccountMarket`, `ExecuteMultiAccountBracket`, and all SIMA dispatch entry points |
| **Shared state read (write)** | `EnableComplianceHub` (read), `accountEquityPeak` (ConcurrentDictionary read), `TrailingDrawdownLimit` (read), `accountDailyProfit` (ConcurrentDictionary read), `MaxDailyProfitCap` (read), `EnableSIMA` (read), `EnableConsistencyLock` (read), `_uiCallbackFailures` (Interlocked write) |
| **External dependency** | `Account.Get(AccountItem.CashValue, ...)` — live broker balance call inside the compliance gate |
| **Side effects** | `Print(...)` on hard-block trigger; `Interlocked.Increment(ref _uiCallbackFailures)` on exception |
| **Threading constraint** | Called on strategy thread only; concurrent dict reads are lock-free safe |
| **Risk on change** | High — this is a hard safety gate; incorrect extraction could silently bypass compliance checks |

**Affected symbol count (blast radius):** ~8 entry-path callers + 6 shared state fields + 1 broker API call.

---

## Top 3 Complexity Drivers

1. **Nested feature-flag + account-null guard + trailing-drawdown compound condition**
   The outer `if (!EnableComplianceHub) return true` short-circuit is followed immediately by a
   two-part null guard (`accountName ?? Account?.Name` + `IsNullOrEmpty`) and then the
   `TryGetValue + peak > 0 && TrailingDrawdownLimit > 0` compound. This three-level guard fan-out
   contributes +4 CYC (feature flag path, null-empty path, dict-miss path, compound-condition
   false-arm) before any business logic executes. The live broker balance call inside a `try/catch`
   adds +2 more (try path + catch path with `Interlocked.Increment` side-effect), bringing the
   first rule block alone to ~6 CYC.

2. **Dual-guard second rule block: `EnableSIMA && EnableConsistencyLock` → inner daily-cap check**
   The second hard-block rule (`MaxDailyProfitCap`) is gated behind a conjunctive `if (EnableSIMA
   && EnableConsistencyLock)` followed by an inner `if (TryGetValue(...) && MaxDailyProfitCap > 0
   && dp >= MaxDailyProfitCap)`. The boolean short-circuit on the outer `&&` counts as one branch;
   the inner compound condition (three operands) adds two more. Together the second rule block
   contributes +3 CYC, pushing the running total to 9.

3. **Inline live-balance retrieval with exception path inside the compliance gate**
   Rather than using a cached balance, `IsOrderAllowed` calls `currentAccount.Get(AccountItem.
   CashValue, ...)` directly inside the guard. The wrapping `try/catch` creates an exception-
   path branch (+1 CYC) and the catch body logs via `Print` and increments `_uiCallbackFailures`
   before continuing with `balance=0`. This pattern is the dominant maintenance risk: any change
   to the exception-path recovery semantics (e.g., returning `false` instead of continuing)
   would silently alter compliance behaviour for every entry path in both solo and SIMA modes.

---

## Recommended Extraction Count

**2 extractions recommended for Phase 1.**

| Extraction | Proposed Name | Rationale |
|---|---|---|
| 1 | `IsTrailingDrawdownBreached(string acctName)` | Encapsulates the TryGetValue + balance retrieval + try/catch + buffer ≤ 0 check; reduces parent CYC by ~4 |
| 2 | `IsDailyProfitCapBreached(string acctName)` | Encapsulates the EnableSIMA + EnableConsistencyLock + TryGetValue + cap comparison; reduces parent CYC by ~3 |

After both extractions, `IsOrderAllowed` dispatcher CYC drops to ≤3 (feature-flag return,
null-guard, two delegating calls). Each extracted helper remains within CYC ≤5 independently.

---

## Agent Tracking

Agent Name: v12-phase0-hotspot | Bobcoins Used: 2.2 | Execution Time: ~180s
