# EPIC-W7-004 — Phase 2: Architecture Plan

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T00:45:00Z
**Input:** docs/brain/EPIC-W7-004/01-scope-boundary.md

---

## Subject Method

| Field | Value |
|---|---|
| **Method** | `HandleFleetTargetFill` |
| **Source File** | `src/V12_002.UI.Compliance.cs` |
| **Lines** | 624–696 |
| **CYC (Reported)** | 34 |
| **Target CYC** | <= 8 (all units) |
| **Callers** | `ProcessQueuedExecution_HandleFleetOCO` (direct), `ProcessQueuedExecution` (indirect) |

---

## Original Method Structure

```csharp
// lines 624-696 — 3 logical phases + orchestration shell
private void HandleFleetTargetFill(
    QueuedAccountExecution item,
    Order ocoOrder,
    Account ocoAcct,
    string ocoName)
{
    // PHASE A: Entry key resolution (name parsing) — lines 626-631
    int tgtNum = ocoName[1] - '0';
    string tgtPrefix = "T" + tgtNum + "_";
    string tgtEntryKey = ocoName.Substring(tgtPrefix.Length);
    int tgtLastUnderscore = tgtEntryKey.LastIndexOf('_');
    if (tgtLastUnderscore > 0)
        tgtEntryKey = tgtEntryKey.Substring(0, tgtLastUnderscore);

    // PHASE B: Position lookup guard — lines 633-638
    PositionInfo tgtPos;
    if (!string.IsNullOrEmpty(tgtEntryKey)
        && activePositions.TryGetValue(tgtEntryKey, out tgtPos)
        && tgtPos != null)
    {
        // PHASE C: ApplyTargetFill + idempotency dispatch — lines 640-672
        bool tgtTerminal = ocoOrder.OrderState == OrderState.Filled;
        bool tgtAlreadyProcessed; int tgtApplied; int tgtRemaining;
        ApplyTargetFill(tgtPos, tgtNum, item.EventArgs.Execution.Quantity,
            tgtTerminal, out tgtAlreadyProcessed, out tgtApplied, out tgtRemaining);
        if (tgtAlreadyProcessed) { Print("[1104.1 GUARD] ..."); }
        else
        {
            Print("[1104.1] Fleet TARGET ...");
            // PHASE D: OCO stop cancellation sweep — lines 674-693
            if (tgtRemaining <= 0)
            {
                foreach (Order o in ocoAcct.Orders.ToArray())
                {
                    if (o == null || o.Instrument?.FullName != Instrument?.FullName) continue;
                    if (o.OrderState != OrderState.Working && ...) continue;
                    if (o.Name != null && o.Name.StartsWith("Stop_"))
                    {
                        CancelOrderOnAccount(o, ocoAcct);
                        Print("[1104.1 OCO] Fleet ...");
                    }
                }
            }
        }
    }
}
```

---

## Extraction Plan

### Helper 1: `ResolveFleetTargetEntryKey`

| Field | Value |
|---|---|
| **Signature** | `private static string ResolveFleetTargetEntryKey(string ocoName)` |
| **Extracted Lines** | 626–631 |
| **Responsibility** | Parse OCO order name string to extract the position entry key |
| **Projected CYC** | **2** |
| **Annotation** | `[MethodImpl(MethodImplOptions.AggressiveInlining)]` |
| **Static** | YES — pure computation, no instance state |
| **Return** | The resolved entry key string |

**Body outline:**
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private static string ResolveFleetTargetEntryKey(string ocoName)
{
    int tgtNum = ocoName[1] - '0';
    string tgtPrefix = "T" + tgtNum + "_";
    string tgtEntryKey = ocoName.Substring(tgtPrefix.Length);
    int tgtLastUnderscore = tgtEntryKey.LastIndexOf('_');
    if (tgtLastUnderscore > 0)
        tgtEntryKey = tgtEntryKey.Substring(0, tgtLastUnderscore);
    return tgtEntryKey;
}
```

**Jane Street Notes:**
- `carl_cook`: Hot-path eligible — pure string parse, deterministic, no heap allocation beyond existing substrings; `AggressiveInlining` applied.
- `trading_billions`: Single responsibility — only parses the entry key; no side effects.

---

### Helper 2: `LogFleetTargetFillResult`

| Field | Value |
|---|---|
| **Signature** | `private void LogFleetTargetFillResult(int tgtNum, string tgtEntryKey, bool tgtAlreadyProcessed, int tgtApplied, double price, int tgtRemaining)` |
| **Extracted Lines** | 653–672 |
| **Responsibility** | Emit diagnostic Print messages for fill result (guard or normal) |
| **Projected CYC** | **2** |
| **Annotation** | `[MethodImpl(MethodImplOptions.NoInlining)]` |
| **Static** | NO — uses `Print()` instance method |
| **Return** | void |

**Body outline:**
```csharp
[MethodImpl(MethodImplOptions.NoInlining)]
private void LogFleetTargetFillResult(
    int tgtNum, string tgtEntryKey,
    bool tgtAlreadyProcessed, int tgtApplied,
    double price, int tgtRemaining)
{
    if (tgtAlreadyProcessed)
    {
        Print(string.Format(
            "[1104.1 GUARD] Fleet T{0} already processed for {1} -- skipping duplicate.",
            tgtNum, tgtEntryKey));
    }
    else
    {
        Print(string.Format(
            "[1104.1] Fleet TARGET {0} filled: {1} @ {2:F2}. Remaining: {3}",
            tgtNum, tgtApplied, price, tgtRemaining));
    }
}
```

**Jane Street Notes:**
- `carl_cook`: Cold-path — `Print()` is NinjaTrader UI dispatch; extracted out-of-line with `NoInlining`.
- `trading_billions`: Single responsibility — only emits log messages; no state mutation.

---

### Helper 3: `CancelFleetStopOnAllTargetsFilled`

| Field | Value |
|---|---|
| **Signature** | `private void CancelFleetStopOnAllTargetsFilled(Account ocoAcct)` |
| **Extracted Lines** | 676–692 |
| **Responsibility** | Sweep account orders and cancel any working Stop orders (OCO cleanup) |
| **Projected CYC** | **6** |
| **Annotation** | `[MethodImpl(MethodImplOptions.NoInlining)]` |
| **Static** | NO — uses `CancelOrderOnAccount()` and `Print()` instance methods |
| **Return** | void |

**Body outline:**
```csharp
[MethodImpl(MethodImplOptions.NoInlining)]
private void CancelFleetStopOnAllTargetsFilled(Account ocoAcct)
{
    foreach (Order o in ocoAcct.Orders.ToArray())
    {
        if (o == null || o.Instrument?.FullName != Instrument?.FullName)
            continue;
        if (o.OrderState != OrderState.Working && o.OrderState != OrderState.Accepted)
            continue;
        if (o.Name != null && o.Name.StartsWith("Stop_"))
        {
            CancelOrderOnAccount(o, ocoAcct);
            Print(string.Format(
                "[1104.1 OCO] Fleet {0}: all targets filled -- cancelled stop.",
                ocoAcct.Name));
        }
    }
}
```

**Jane Street Notes:**
- `carl_cook`: Cold-path — only invoked when `tgtRemaining <= 0` (final fill); `NoInlining` applied. `.ToArray()` allocation is pre-existing and acceptable on cold path.
- `trading_billions`: Single responsibility — only cancels stop orders; defense in depth: three independent filter guards before any mutation.
- `gjengset`: No lock() usage; reads `ocoAcct.Orders` via NinjaTrader-managed callback-safe collection.

---

## Parent Method After Extraction

```csharp
private void HandleFleetTargetFill(
    QueuedAccountExecution item,
    Order ocoOrder,
    Account ocoAcct,
    string ocoName)
{
    string tgtEntryKey = ResolveFleetTargetEntryKey(ocoName);  // Helper 1
    int tgtNum = ocoName[1] - '0';

    PositionInfo tgtPos;
    if (!string.IsNullOrEmpty(tgtEntryKey)
        && activePositions.TryGetValue(tgtEntryKey, out tgtPos)
        && tgtPos != null)
    {
        bool tgtTerminal = ocoOrder.OrderState == OrderState.Filled;
        bool tgtAlreadyProcessed;
        int tgtApplied;
        int tgtRemaining;
        ApplyTargetFill(tgtPos, tgtNum, item.EventArgs.Execution.Quantity,
            tgtTerminal, out tgtAlreadyProcessed, out tgtApplied, out tgtRemaining);

        LogFleetTargetFillResult(                              // Helper 2
            tgtNum, tgtEntryKey,
            tgtAlreadyProcessed, tgtApplied,
            item.EventArgs.Execution.Price, tgtRemaining);

        if (!tgtAlreadyProcessed && tgtRemaining <= 0)
            CancelFleetStopOnAllTargetsFilled(ocoAcct);       // Helper 3
    }
}
```

**Parent CYC Projection: 5**
- base: 1
- `!IsNullOrEmpty`: +1
- `TryGetValue`: +1
- `tgtPos != null`: +1
- `!tgtAlreadyProcessed && tgtRemaining <= 0`: +1
- Delegated to helpers: 0

---

## CYC Summary Table

| Unit | Type | Projected CYC | Under Limit? |
|---|---|---|---|
| `ResolveFleetTargetEntryKey` | Helper | 2 | ✅ |
| `LogFleetTargetFillResult` | Helper | 2 | ✅ |
| `CancelFleetStopOnAllTargetsFilled` | Helper | 6 | ✅ |
| `HandleFleetTargetFill` (parent) | Parent | 5 | ✅ |
| **max_cyc_projected** | — | **6** | ✅ |

**Original CYC: 34 → Max projected: 6 (82% reduction)**

---

## Jane Street KB Alignment

| Pattern | Source | Application |
|---|---|---|
| AggressiveInlining hot | `carl_cook` | `ResolveFleetTargetEntryKey` — pure parse, no side effects |
| NoInlining cold | `carl_cook` | `LogFleetTargetFillResult`, `CancelFleetStopOnAllTargetsFilled` — cold paths |
| Extract cold logging out-of-line | `carl_cook` | All Print() calls moved to `LogFleetTargetFillResult` |
| Single responsibility per helper | `trading_billions` | Each helper has exactly one named concern |
| Defense in depth | `trading_billions` | Each helper has its own guards; parent coordinates guards |
| Rate-limit circuit breaker | `trading_billions` | `CancelFleetStopOnAllTargetsFilled` only called on final fill (tgtRemaining<=0) |
| Lock-free (no lock() blocks) | `gjengset` | No locks in any extracted helper; activePositions is ConcurrentDictionary |
| No false sharing introduced | `gjengset` | Helpers operate on method-local variables only; no new struct fields |

---

## MCP Evidence

| Tool | Finding |
|---|---|
| `resolve_repo` | Repo indexed: `antigravityos187-sketch/universal-or-strategy`, 5147 symbols |
| `search_symbols` | `HandleFleetTargetFill` confirmed at `src/V12_002.UI.Compliance.cs:624` |
| `get_context_bundle` | Full source retrieved (lines 624-696, 72 lines), method signature: `private void HandleFleetTargetFill(QueuedAccountExecution, Order, Account, string)` |
| `get_call_hierarchy` | Callers: `ProcessQueuedExecution_HandleFleetOCO` (direct), `ProcessQueuedExecution` (indirect). Callees: `activePositions`, `ApplyTargetFill`, `CancelOrderOnAccount`, `Print` |
| `get_dependency_graph` | `src/V12_002.UI.Compliance.cs` — 0 external file dependencies (self-contained partial class) |
| `get_extraction_candidates` | No candidates returned (complexity data requires re-index); source analysis performed directly via `get_context_bundle` |

---

## Sequential Thinking Evidence

| Thought | Topic | Conclusion |
|---|---|---|
| T1 | Complexity drivers | 4 distinct logical phases identified; CYC:34 attributed to nesting + compound conditions + ApplyTargetFill sub-calls |
| T2 | Extraction strategy | 3 helpers designed: key parsing (CYC:2), fill logging (CYC:2), stop cancellation (CYC:6); parent CYC:5 |
| T3 | Jane Street alignment | All 3 KB patterns applied correctly: gjengset (lock-free), carl_cook (inline hot/noinline cold), trading_billions (single responsibility + circuit breaker) |
| T4 | Final signatures | Method signatures finalized with static annotation for Helper 1; MethodImpl annotations specified; CYC projections confirmed |

---

## Scope Boundary Compliance

| Check | Status |
|---|---|
| Target method unchanged in signature | ✅ |
| All helpers are private, same class | ✅ |
| No caller modifications required | ✅ |
| No cross-file changes | ✅ |
| V12.23 No Scope Creep | ✅ |
| Lock-free (no lock() blocks added) | ✅ |
| ASCII-only string literals | ✅ |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 1.5 |
| **Execution Time** | batch |
| **Phase** | 2 |
| **Wave** | 7 |
| **MCP Tools Used** | resolve_repo, search_symbols, get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **Sequential Thinking Thoughts** | 4 (probe + 3 architecture thoughts + 1 finalization) |
| **extraction_count** | 3 |
| **max_cyc_projected** | 6 |
| **parent_cyc_projected** | 5 |
