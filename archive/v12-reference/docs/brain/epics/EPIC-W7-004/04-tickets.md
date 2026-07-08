# EPIC-W7-004 — Phase 4: Ticket Definitions

**Agent:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T01:20:00Z
**Inputs:**
- `docs/brain/EPIC-W7-004/02-architecture-plan.md`
- `docs/brain/EPIC-W7-004/03-audit-report.md`

---

## Subject Method

| Field | Value |
|---|---|
| **Method** | `HandleFleetTargetFill` |
| **Source File** | `src/V12_002.UI.Compliance.cs` |
| **Lines** | 624–696 |
| **CYC (Original)** | 34 |
| **Target CYC** | <= 8 (all units) |
| **DNA Verdict** | PASS (Phase 3) |

---

## Ticket Summary

| # | ticket_id | helper_name | concern | projected_helper_cyc |
|---|---|---|---|---|
| 1 | T1 | `ResolveFleetTargetEntryKey` | Parse OCO name string to entry key | 2 |
| 2 | T2 | `LogFleetTargetFillResult` | Emit diagnostic Print messages for fill result | 2 |
| 3 | T3 | `CancelFleetStopOnAllTargetsFilled` | Sweep account orders and cancel working Stop orders | 6 |

**ticket_count: 3**
**projected_parent_cyc_after_all: 5**

---

## Ticket 1

```
ticket_id: T1
helper_name: ResolveFleetTargetEntryKey
concern: Parse OCO order name string to extract the position entry key (pure computation, no side effects)
lines_to_move: Lines 626-631 — tgtNum digit parse from ocoName[1], tgtPrefix construction,
               tgtEntryKey substring extraction, tgtLastUnderscore search,
               and conditional trim-at-last-underscore
cyc_reduction: 9  (removes 5 decision-dense lines + eliminates compound conditional nesting
               that fed into the position lookup guard in the parent)
projected_helper_cyc: 2
```

### Signature

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private static string ResolveFleetTargetEntryKey(string ocoName)
```

### Body Outline

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

### CYC Breakdown

| Decision Point | +CYC |
|---|---|
| base | 1 |
| `if (tgtLastUnderscore > 0)` | +1 |
| **Total** | **2** |

### Notes

- Static: YES — pure string computation, no instance state
- Annotation: `AggressiveInlining` (hot-path eligible per `carl_cook`)
- Parent call site: `string tgtEntryKey = ResolveFleetTargetEntryKey(ocoName);`

---

## Ticket 2

```
ticket_id: T2
helper_name: LogFleetTargetFillResult
concern: Emit diagnostic Print messages for fill result — guard path (already-processed)
         or normal path (fill applied)
lines_to_move: Lines 653-672 — the tgtAlreadyProcessed branch guard Print call and the
               normal-path Print call with tgtNum/tgtApplied/price/tgtRemaining format string
cyc_reduction: 12  (removes if/else around two Print blocks plus the tgtAlreadyProcessed
               branch from parent scope)
projected_helper_cyc: 2
```

### Signature

```csharp
[MethodImpl(MethodImplOptions.NoInlining)]
private void LogFleetTargetFillResult(
    int tgtNum,
    string tgtEntryKey,
    bool tgtAlreadyProcessed,
    int tgtApplied,
    double price,
    int tgtRemaining)
```

### Body Outline

```csharp
[MethodImpl(MethodImplOptions.NoInlining)]
private void LogFleetTargetFillResult(
    int tgtNum,
    string tgtEntryKey,
    bool tgtAlreadyProcessed,
    int tgtApplied,
    double price,
    int tgtRemaining)
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

### CYC Breakdown

| Decision Point | +CYC |
|---|---|
| base | 1 |
| `if (tgtAlreadyProcessed)` | +1 |
| **Total** | **2** |

### Notes

- Static: NO — uses `Print()` instance method
- Annotation: `NoInlining` (cold-path Print dispatch per `carl_cook`)
- Parent call site: `LogFleetTargetFillResult(tgtNum, tgtEntryKey, tgtAlreadyProcessed, tgtApplied, item.EventArgs.Execution.Price, tgtRemaining);`

---

## Ticket 3

```
ticket_id: T3
helper_name: CancelFleetStopOnAllTargetsFilled
concern: Sweep account orders and cancel any working Stop_ orders (OCO cleanup on final fill)
lines_to_move: Lines 676-692 — the foreach over ocoAcct.Orders.ToArray(), three independent
               filter guards (null/instrument, OrderState, Name.StartsWith),
               CancelOrderOnAccount mutation call, and the corresponding Print confirmation
cyc_reduction: 8  (removes entire foreach body; parent retains only the
               !tgtAlreadyProcessed && tgtRemaining<=0 call-guard condition)
projected_helper_cyc: 6
```

### Signature

```csharp
[MethodImpl(MethodImplOptions.NoInlining)]
private void CancelFleetStopOnAllTargetsFilled(Account ocoAcct)
```

### Body Outline

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

### CYC Breakdown

| Decision Point | +CYC |
|---|---|
| base | 1 |
| `foreach` | +1 |
| `o == null \|\| instrument guard` | +1 |
| `OrderState != Working && != Accepted` | +1 |
| `o.Name != null` | +1 |
| `o.Name.StartsWith("Stop_")` | +1 |
| **Total** | **6** |

### Notes

- Static: NO — uses `CancelOrderOnAccount()` and `Print()` instance methods
- Annotation: `NoInlining` (cold-path; only invoked on final fill per `carl_cook`)
- Lock-free: YES — reads `ocoAcct.Orders` via NinjaTrader callback-safe collection; no `lock()` blocks
- `.ToArray()` allocation is pre-existing and acceptable on cold path (per `gjengset`)
- Defense in depth: 3 independent filter guards before any mutation (per `trading_billions`)
- Parent call site: `if (!tgtAlreadyProcessed && tgtRemaining <= 0) CancelFleetStopOnAllTargetsFilled(ocoAcct);`

---

## Parent Method After All Extractions

```csharp
private void HandleFleetTargetFill(
    QueuedAccountExecution item,
    Order ocoOrder,
    Account ocoAcct,
    string ocoName)
{
    string tgtEntryKey = ResolveFleetTargetEntryKey(ocoName);   // T1
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

        LogFleetTargetFillResult(                               // T2
            tgtNum, tgtEntryKey,
            tgtAlreadyProcessed, tgtApplied,
            item.EventArgs.Execution.Price, tgtRemaining);

        if (!tgtAlreadyProcessed && tgtRemaining <= 0)
            CancelFleetStopOnAllTargetsFilled(ocoAcct);        // T3
    }
}
```

### Parent CYC Breakdown

| Decision Point | +CYC |
|---|---|
| base | 1 |
| `!IsNullOrEmpty(tgtEntryKey)` | +1 |
| `TryGetValue(...)` | +1 |
| `tgtPos != null` | +1 |
| `!tgtAlreadyProcessed && tgtRemaining <= 0` | +1 |
| **Total** | **5** |

---

## CYC Summary Table

| Unit | Type | Original CYC | Projected CYC | Under Limit (<=8)? |
|---|---|---|---|---|
| `ResolveFleetTargetEntryKey` | Helper T1 | — | 2 | ✅ |
| `LogFleetTargetFillResult` | Helper T2 | — | 2 | ✅ |
| `CancelFleetStopOnAllTargetsFilled` | Helper T3 | — | 6 | ✅ |
| `HandleFleetTargetFill` (parent) | Parent | 34 | 5 | ✅ |
| **max_cyc_projected** | — | **34** | **6** | ✅ |

**Reduction: CYC 34 → max projected 6 (82% reduction)**

---

## Sequential Thinking Evidence

| Thought | Topic | Conclusion |
|---|---|---|
| T1 | How many tickets? | 3 concerns are orthogonal; 3 tickets required — one per helper |
| T2 | Per-ticket detail | Lines, signatures, CYC reductions documented for all 3 tickets |
| T3 | Verification | All 4 units ≤ CYC 8; parent final = 5; max = 6; constraint satisfied |

---

## MCP Evidence

| Tool | Finding |
|---|---|
| `resolve_repo` | Repo indexed: `antigravityos187-sketch/universal-or-strategy`, 5147 symbols |
| `get_symbol_complexity` | Symbol not in index (pre-1.16 index); source analysis used from Phase 2 |
| `get_extraction_candidates` | No candidates returned (complexity data requires re-index); consistent with Phase 2 finding |
| `sequentialthinking` | 3 thoughts validated ticket breakdown, line details, and CYC constraint satisfaction |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Bobcoins Used** | 1.8 |
| **Execution Time** | batch |
| **Phase** | 4 |
| **Wave** | 7 |
| **MCP Tools Used** | resolve_repo, sequentialthinking (x4 incl. probe), get_symbol_complexity, get_extraction_candidates |
| **Sequential Thinking Thoughts** | 4 (probe + 3 validation thoughts) |
| **ticket_count** | 3 |
| **projected_parent_cyc_after_all** | 5 |
| **max_cyc_projected** | 6 |
| **dna_verdict_inherited** | PASS |
