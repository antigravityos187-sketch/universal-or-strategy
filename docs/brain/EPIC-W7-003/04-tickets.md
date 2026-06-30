# EPIC-W7-003 — Phase 4: Ticket Definitions

**Agent:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation (V12 Epic Workflow)
**Generated:** 2026-06-29
**Input:** `docs/brain/EPIC-W7-003/02-architecture-plan.md` + `docs/brain/EPIC-W7-003/03-audit-report.md`
**Output:** `docs/brain/EPIC-W7-003/04-tickets.md`

---

## Summary

| Field | Value |
|---|---|
| **Epic** | EPIC-W7-003 |
| **Method** | `IsOrderAllowed` |
| **File** | `src/V12_002.UI.Compliance.cs` |
| **Original CYC** | 21 |
| **ticket_count** | **3** |
| **projected_parent_cyc_after_all** | **5** |
| **max_cyc_projected** | **6** |
| **DNA Verdict** | PASS |

---

## Ticket 1

| Field | Value |
|---|---|
| **ticket_id** | T1 |
| **helper_name** | `TryGetAccountBalance` |
| **concern** | Safe broker API call with error isolation — extract the `acct.Get(CashValue)` try/catch block into a dedicated helper that returns a success boolean and outputs the balance via `out double balance`. This isolates all exception-path allocations (string formatting, `Interlocked.Increment`) from the compliance hot path. |
| **lines_to_move** | The inner try/catch block inside the original drawdown evaluation section of `IsOrderAllowed`: the `acct == null` null guard, the `acct.Get(NinjaTrader.Cbi.AccountItem.CashValue, NinjaTrader.Cbi.Currency.UsDollar)` broker API call, the `catch (Exception ex)` handler with `Interlocked.Increment(ref _uiCallbackFailures)` and `Print(...)`. Annotate with `[MethodImpl(MethodImplOptions.NoInlining)]` (cold path marker). |
| **cyc_reduction** | 2 (removes the `null` branch (+1) and `catch` path (+1) from the drawdown block, simplifying `CheckTrailingDrawdown` in T2) |
| **projected_helper_cyc** | **3** |
| **signature** | `private bool TryGetAccountBalance(Account acct, out double balance)` |
| **called_by** | `CheckTrailingDrawdown` (T2) only |
| **implementation_step** | Step 1 — no dependencies; add first |

**Extracted body:**
```csharp
[MethodImpl(MethodImplOptions.NoInlining)]
private bool TryGetAccountBalance(Account acct, out double balance)
{
    balance = 0;
    if (acct == null)
        return false;
    try
    {
        balance = acct.Get(
            NinjaTrader.Cbi.AccountItem.CashValue,
            NinjaTrader.Cbi.Currency.UsDollar
        );
        return true;
    }
    catch (Exception ex)
    {
        Interlocked.Increment(ref _uiCallbackFailures);
        Print($"[UI_CALLBACK] Account balance retrieval failed: {ex.Message}");
        return false;
    }
}
```

**CYC path count:** base(1) + `acct == null`(+1) + `catch`(+1) = **3** ✅

---

## Ticket 2

| Field | Value |
|---|---|
| **ticket_id** | T2 |
| **helper_name** | `CheckTrailingDrawdown` |
| **concern** | Trailing drawdown hard-block evaluation (Defense Layer 1) — extract the entire drawdown guard block from `IsOrderAllowed`: the `accountEquityPeak.TryGetValue` compound guard, the call to `TryGetAccountBalance`, the buffer arithmetic (`balance - (peak - TrailingDrawdownLimit)`), the `buffer <= 0` hard-block check, and the `Print(...)` compliance log message. |
| **lines_to_move** | The drawdown evaluation block in `IsOrderAllowed` (~15–18 lines): compound TryGetValue guard (`!TryGetValue \|\| peak <= 0 \|\| TrailingDrawdownLimit <= 0 → return true`), `TryGetAccountBalance(this.Account, out double balance)` call, buffer computation, `if (buffer <= 0)` block with `Print(...)` and `return false`, final `return true`. |
| **cyc_reduction** | 8 (removes the full drawdown block: TryGetValue(+1) + peak>0(+1) + Limit>0(+1) + currentAccount!=null(+1) + try/catch(+1) + buffer<=0(+1) + additional control flow(+2) from parent) |
| **projected_helper_cyc** | **6** |
| **signature** | `private bool CheckTrailingDrawdown(string acctName)` |
| **called_by** | `IsOrderAllowed` (parent) only |
| **depends_on** | T1 (`TryGetAccountBalance` must exist) |
| **implementation_step** | Step 2 — depends on T1 |

**Extracted body:**
```csharp
private bool CheckTrailingDrawdown(string acctName)
{
    if (!accountEquityPeak.TryGetValue(acctName, out double peak)
        || peak <= 0
        || TrailingDrawdownLimit <= 0)
        return true;

    TryGetAccountBalance(this.Account, out double balance);
    double buffer = balance - (peak - TrailingDrawdownLimit);
    if (buffer <= 0)
    {
        Print(string.Format(
            "[COMPLIANCE BLOCKED] Entry suppressed for {0}: Trailing drawdown breached. Buffer=${1:F2}",
            acctName,
            buffer
        ));
        return false;
    }
    return true;
}
```

**CYC path count:** base(1) + `!TryGetValue`(+1) + `|| peak <= 0`(+1) + `|| Limit <= 0`(+1) + `buffer <= 0`(+1) = **5–6** ✅

---

## Ticket 3

| Field | Value |
|---|---|
| **ticket_id** | T3 |
| **helper_name** | `CheckDailyProfitCap` |
| **concern** | SIMA fleet daily profit cap hard-block evaluation (Defense Layer 2) — extract the SIMA/ConsistencyLock compound guard block from `IsOrderAllowed`: the `EnableSIMA && EnableConsistencyLock` gate, the `accountDailyProfit.TryGetValue` lookup, the `MaxDailyProfitCap > 0 && dp >= MaxDailyProfitCap` cap check, the `Print(...)` compliance log, and `return false`. After adding this helper, replace the `IsOrderAllowed` body with the 5-line thin orchestrator. |
| **lines_to_move** | The SIMA profit cap block in `IsOrderAllowed` (~12–15 lines): `!EnableSIMA \|\| !EnableConsistencyLock → return true` guard, `accountDailyProfit.TryGetValue(acctName, out double dp)` lookup, `MaxDailyProfitCap > 0 && dp >= MaxDailyProfitCap` check, `Print(...)` and `return false`. PLUS: replace the remaining `IsOrderAllowed` body (after removing all extracted blocks) with the 5-line orchestrator that calls `CheckTrailingDrawdown` and `CheckDailyProfitCap`. |
| **cyc_reduction** | 7 (removes EnableSIMA(+1) + ConsistencyLock(+1) + TryGetValue(+1) + cap>0(+1) + dp>=cap(+1) + residual control flow(+2) from parent) |
| **projected_helper_cyc** | **6** |
| **signature** | `private bool CheckDailyProfitCap(string acctName)` |
| **called_by** | `IsOrderAllowed` (parent) only |
| **depends_on** | T1 + T2 must be complete before parent rewrite |
| **implementation_step** | Step 3 (add helper) + Step 4 (rewrite parent orchestrator) + Step 5 (verify build) |

**Extracted body:**
```csharp
private bool CheckDailyProfitCap(string acctName)
{
    if (!EnableSIMA || !EnableConsistencyLock)
        return true;

    if (accountDailyProfit.TryGetValue(acctName, out double dp)
        && MaxDailyProfitCap > 0
        && dp >= MaxDailyProfitCap)
    {
        Print(string.Format(
            "[COMPLIANCE BLOCKED] Entry suppressed for {0}: Daily profit cap hit. DayPL=${1:F2}",
            acctName,
            dp
        ));
        return false;
    }
    return true;
}
```

**CYC path count:** base(1) + `!EnableSIMA`(+1) + `|| !ConsistencyLock`(+1) + `TryGetValue`(+1) + `&& cap>0`(+1) + `&& dp>=cap`(+1) = **6** ✅

**Parent orchestrator (included in T3):**
```csharp
private bool IsOrderAllowed(string? accountName = null)
{
    if (!EnableComplianceHub)
        return true;

    string acctName = accountName ?? Account?.Name;
    if (string.IsNullOrEmpty(acctName))
        return true;

    if (!CheckTrailingDrawdown(acctName))
        return false;

    if (!CheckDailyProfitCap(acctName))
        return false;

    return true;
}
```

**Parent CYC after T3:** base(1) + `!EnableComplianceHub`(+1) + `IsNullOrEmpty`(+1) + `!CheckTrailingDrawdown`(+1) + `!CheckDailyProfitCap`(+1) = **5** ✅

---

## CYC Verification Table

| Method | Projected CYC | <= 8? |
|---|---|---|
| `TryGetAccountBalance` (T1) | 3 | ✅ |
| `CheckTrailingDrawdown` (T2) | 6 | ✅ |
| `CheckDailyProfitCap` (T3) | 6 | ✅ |
| `IsOrderAllowed` (parent, post-T3) | 5 | ✅ |
| **projected_parent_cyc_after_all** | **5** | ✅ |
| **max_cyc_projected** | **6** | ✅ |

**CYC reduction: 21 → 5 (parent), max helper 6. Total reduction: 76%.**

---

## Implementation Order

```
T1: Add TryGetAccountBalance         (no deps)    → build passes
T2: Add CheckTrailingDrawdown        (needs T1)   → build passes
T3: Add CheckDailyProfitCap          (no deps)    → build passes
    Replace IsOrderAllowed body      (needs T1+T2+T3 helpers present)
    Verify build: zero errors, zero warnings
```

---

## Behavioral Invariants (must be preserved across all tickets)

| Invariant | Preserved |
|---|---|
| Returns `true` when `EnableComplianceHub` is false | ✅ |
| Returns `true` when `acctName` is null or empty | ✅ |
| Returns `false` on drawdown breach (`buffer <= 0`) | ✅ |
| Returns `false` on daily profit cap hit | ✅ |
| `balance = 0` on broker API exception (fallback) | ✅ |
| `_uiCallbackFailures` incremented on exception | ✅ |
| `Print()` log messages verbatim (format strings unchanged) | ✅ |
| Method signature unchanged (all 11 call sites unaffected) | ✅ |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Bobcoins Used** | 2.0 |
| **Execution Time** | ~90s |
| **Wave** | 7 |
| **Phase** | 4 |
| **MCP Tools Used** | resolve_repo, sequentialthinking (3 thoughts), get_symbol_complexity, get_extraction_candidates |
| **ticket_count** | 3 |
| **projected_parent_cyc_after_all** | 5 |
| **max_cyc_projected** | 6 |
| **Input** | `docs/brain/EPIC-W7-003/02-architecture-plan.md`, `docs/brain/EPIC-W7-003/03-audit-report.md` |
| **Output** | `docs/brain/EPIC-W7-003/04-tickets.md` |
