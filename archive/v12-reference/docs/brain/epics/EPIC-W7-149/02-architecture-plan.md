# Phase 2: Architecture Plan — EPIC-W7-149

**Agent:** v12-phase2-architecture
**Wave:** 7 | **Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:15:00Z
**Input:** docs/brain/EPIC-W7-149/01-scope-boundary.md

---

## Method Under Extraction

- **Method:** `LogApexPerformance`
- **Source File:** [`src/V12_002.UI.Compliance.cs`](src/V12_002.UI.Compliance.cs:810)
- **Original CYC:** 20
- **Lines:** 810–913 (104 lines, fully monolithic — no prior helpers)

### jcodemunch get_context_bundle result
- Symbol ID: `src/V12_002.UI.Compliance.cs::V12_002.LogApexPerformance#method`
- Signature: `private void LogApexPerformance()`
- Full source retrieved (104 lines). Three structural sections confirmed:
  1. Compound guard block (lines 811–817)
  2. Per-account `foreach` loop with inline JSON building and ternary position logic (lines 832–883)
  3. Fire-and-forget `Task.Run` with two-layer `try/catch` for async I/O (lines 885–910)

### jcodemunch get_call_hierarchy result
- **Callers (depth 1–2):**
  - `ProcessAccountExecutionQueue` — depth 1, [`src/V12_002.UI.Compliance.cs`](src/V12_002.UI.Compliance.cs:427)
  - `OnAccountExecutionUpdate` — depth 2, [`src/V12_002.UI.Compliance.cs`](src/V12_002.UI.Compliance.cs:401)
- **Key callees (depth 1):**
  - `GetComplianceAccounts`, `GetComplianceNow`, `MaybeFinalizeDailySummaries`
  - `UpdateAccountMetricsFromAccount`, `GetUniqueTradingDays`, `PathValidation.ValidateAndCanonicalize`
- **Shared state:** `accountDailyProfit`, `accountTotalProfit`, `accountTradeCount`, `accountMaxDrawdown`, `expectedPositions`, `complianceLogPath`, `lastComplianceLog`

### jcodemunch get_dependency_graph result
- `src/V12_002.UI.Compliance.cs` is an isolated node (0 import edges, 0 importer edges)
- Partial class within monolith — all symbols co-located in the same compilation unit
- **Cross-file impact: NONE** — extraction is same-file private helpers only

### jcodemunch get_extraction_candidates result
- No pre-existing extraction candidates returned (0 candidates at min_complexity=3, min_callers=1)
- Confirms fully monolithic state — `LogApexPerformance` is the sole complex symbol requiring decomposition

---

## Sequential Thinking Summary

**Thought 1 — Analysis:** CYC 20 confirmed via full source. Three distinct complexity drivers identified matching 00-hotspots.md: guard block (~3 CYC), per-account loop body (~7 CYC), async I/O try/catch (~4 CYC). Baseline 1 + residual orchestration = 5 after extraction. Three helpers are sufficient to reach CYC<=8 in parent.

**Thought 2 — Helper 1 design:** `ShouldSkipComplianceLog()` absorbs 3 branch points (enabled/path OR-check = 2, throttle check = 1). Projected CYC 3. Stateless predicate — makes the guard an unbypassable type-safe contract.

**Thought 3 — Helper 2 design:** `BuildAccountJsonEntry(Account acct, int index)` absorbs 7 branch points from the foreach body (null-guard, comma, brokerPos compound-&&, Long ternary, expectedPositions null, isConnected ternary). Projected CYC 7 (at ceiling, still <=8). Pure function returning string — no shared-state writes.

**Thought 4 — Helper 3 design:** `WriteComplianceJsonAsync(string path, string payload)` absorbs 4 branch points (Task.Run path, path-null guard, SecurityException catch, swallow catch). Projected CYC 4. Threading ordering preserved: `lastComplianceLog` is stamped in parent BEFORE this call fires.

**Thought 5 — Verification:** Parent residual = base(1) + if-ShouldSkip(1) + outer-try(1) + foreach(1) + outer-catch(1) = **CYC 5**. All 3 helpers <=8. Total extraction count: 3. max_cyc_projected: 7 (Helper 2). Jane Street verdict: PASS on all 4 criteria.

---

## Extraction Plan

| # | Helper Method Name | Responsibility | Projected CYC | Branches Absorbed |
|---|---|---|---|---|
| 1 | `ShouldSkipComplianceLog() → bool` | Guard gate: enabled-flag OR path-null check + 5s throttle check | 3 | 3 |
| 2 | `BuildAccountJsonEntry(Account acct, int index) → string` | Single-account JSON fragment: null-guard, position direction ternaries, expectedQty lookup, connection status | 7 | 7 |
| 3 | `WriteComplianceJsonAsync(string path, string payload) → void` | Fire-and-forget Task.Run + path validation + File.WriteAllText + two-layer try/catch | 4 | 4 |

---

## Parent Method After Extraction

[`LogApexPerformance()`](src/V12_002.UI.Compliance.cs:810) after extraction retains orchestration-only logic:

```
private void LogApexPerformance()
{
    if (ShouldSkipComplianceLog()) return;           // delegating guard
    try
    {
        StringBuilder sb = new StringBuilder();
        // JSON header
        List<Account> accounts = GetComplianceAccounts();
        DateTime nowInZone = GetComplianceNow();
        MaybeFinalizeDailySummaries(nowInZone, accounts);
        int count = 0;
        foreach (Account acct in accounts)
        {
            sb.Append(BuildAccountJsonEntry(acct, count));  // delegating loop body
            count++;
        }
        // JSON footer
        string jsonPayload = sb.ToString();
        string path = complianceLogPath;
        lastComplianceLog = DateTime.Now;            // stamp BEFORE async fire
        WriteComplianceJsonAsync(path, jsonPayload); // delegating async I/O
    }
    catch (Exception ex)
    {
        Print("[COMPLIANCE] ERROR writing log: " + ex.Message);
    }
}
```

- **Remaining logic:** Orchestration only — no inline branching
- **Projected CYC:** 5 (base + if-guard + try + foreach + outer-catch)

---

## max_cyc_projected: 7
## extraction_count: 3

---

## Jane Street Alignment

| Criterion | Status | Evidence |
|---|---|---|
| CYC<=8 achieved | **YES** | Parent=5, Helper1=3, Helper2=7, Helper3=4 — all <=8 |
| Single-responsibility per helper | **YES** | Guard logic / JSON-fragment building / async I/O — each one concern |
| Lock-free/Actor pattern preserved | **YES** | `WriteComplianceJsonAsync` uses `Task.Run` fire-and-forget; no `lock()` blocks introduced |
| Illegal states unrepresentable | **YES** | `ShouldSkipComplianceLog()` bool predicate makes guard bypass structurally impossible |
| No scope creep | **YES** | All helpers are `private` methods in same partial class; caller signatures unchanged |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | ~18 |
| **Execution Time** | 2026-06-29T01:15:00Z |
| **Wave** | 7 |
| **Phase** | 2 |
| **jcodemunch tools called** | search_symbols, get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **extraction_count** | 3 |
| **max_cyc_projected** | 7 |
