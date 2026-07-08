# EPIC-W7-060 — Phase 2: Architecture Plan

**Agent Name:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:10:00Z
**Input:** docs/brain/EPIC-W7-060/01-scope-boundary.md

---

## Method Summary

| Field | Value |
|---|---|
| **Method** | `SweepTrackedOrders` |
| **File** | `src/V12_002.SIMA.Lifecycle.cs` |
| **Lines** | 1308–1353 (46 lines) |
| **Signature** | `private int SweepTrackedOrders(bool force)` |
| **CYC Baseline (live index)** | **11** (assessment: high) |
| **Max Nesting** | 4 |
| **Param Count** | 1 |
| **Target CYC** | <= 8 (Jane Street threshold) |
| **Caller** | `CancelAllV12GtcOrders` (DO NOT MODIFY) |

---

## MCP Evidence

### get_symbol_complexity Result

```json
{
  "symbol_id": "src/V12_002.SIMA.Lifecycle.cs::V12_002.SweepTrackedOrders#method",
  "cyclomatic": 11,
  "max_nesting": 4,
  "param_count": 1,
  "lines": 46,
  "assessment": "high"
}
```

**Conclusion:** CYC 11 > 8 threshold — extraction required.

### get_context_bundle — Source

```csharp
private int SweepTrackedOrders(bool force)
{
    // Build 990: Semantic separation -- force=false (SIMA disable) cancels only entry orders.
    // force=true (strategy terminate) cancels all tracked orders.
    var trackedDicts = force
        ? new ConcurrentDictionary<string, Order>[]
          { entryOrders, stopOrders, target1Orders, target2Orders,
            target3Orders, target4Orders, target5Orders }
        : new ConcurrentDictionary<string, Order>[] { entryOrders };

    int trackedCancels = 0;
    foreach (var dict in trackedDicts)
    {
        if (dict == null) continue;
        foreach (var kvp in dict.ToArray())
        {
            Order ord = kvp.Value;
            if (ord == null) continue;
            if (
                ord.OrderState != OrderState.Working
                && ord.OrderState != OrderState.Accepted
                && ord.OrderState != OrderState.Submitted
                && ord.OrderState != OrderState.ChangePending
                && ord.OrderState != OrderState.ChangeSubmitted
            )
                continue;
            try
            {
                CancelOrderOnAccount(ord, ord.Account);
                trackedCancels++;
            }
            catch { }
        }
    }
    return trackedCancels;
}
```

### get_call_hierarchy Result

**Callers (depth 2):**
- `CancelAllV12GtcOrders` (direct caller) — `src/V12_002.SIMA.Lifecycle.cs:1294` — DO NOT MODIFY
- `ProcessShutdownSIMA` (depth 2, calls CancelAllV12GtcOrders)

**Callees (depth 2):**
- `CancelOrderOnAccount` — `src/V12_002.Orders.CancelGateway.cs:46` — existing, no change
- `IsOrderTerminal` — `src/V12_002.Orders.Management.Flatten.cs:698` — existing, referenced in extraction plan

### get_dependency_graph Result

No import edges resolved at depth 1 (partial class — all in-file dependencies). Confirms blast radius is contained to `src/V12_002.SIMA.Lifecycle.cs`.

---

## Sequential Thinking Evidence

### Thought 1 — CYC Analysis

Live CYC = 11 (above Jane Street threshold of 8). Complexity drivers identified:
- `force ? [...] : [...]` ternary array construction: +1
- `foreach (trackedDicts)`: +1
- `if (dict == null)`: +1
- `foreach (kvp in dict.ToArray())`: +1
- `if (ord == null)`: +1
- `if (OrderState != Working && != Accepted && != Submitted && != ChangePending && != ChangeSubmitted)`: +5 (5 conditions)
- Total = CYC 11 (base 1 + 10 branches)

**Decision:** Extraction required.

### Thought 2 — Extraction Strategy

Two helper extractions identified:

1. **`BuildTrackedDictList(bool force)`** — extracts the ternary dict-array construction. CYC = 2.
2. **`SweepDictionary(ConcurrentDictionary<string,Order> dict)`** — extracts per-dictionary sweep loop. Delegates the 5-way OrderState check to existing `IsOrderTerminal`. CYC = 5.

Refactored parent `SweepTrackedOrders` becomes: build list, foreach, accumulate. CYC = 2.

### Thought 3 — CYC Validation

All methods projected <= 8:

| Method | CYC Projection | Branches |
|---|---|---|
| `SweepTrackedOrders` (refactored) | 2 | base=1, foreach=1 |
| `BuildTrackedDictList` | 2 | base=1, ternary=1 |
| `SweepDictionary` | 5 | base=1, null=1, foreach=1, null=1, IsOrderTerminal call=1 |

`max_cyc_projected = 5` — compliant with Jane Street threshold <= 8.

Using the existing `IsOrderTerminal` callee (already in call graph) replaces the 5-condition inline check without adding new external dependencies.

---

## Extraction Plan

| # | Helper Method | Signature | Extracted From | CYC |
|---|---|---|---|---|
| 1 | `BuildTrackedDictList` | `private ConcurrentDictionary<string, Order>[] BuildTrackedDictList(bool force)` | Lines 1312–1319 (force ternary) | 2 |
| 2 | `SweepDictionary` | `private int SweepDictionary(ConcurrentDictionary<string, Order> dict)` | Lines 1322–1347 (foreach + cancel logic) | 5 |

**Extraction count:** 2  
**max_cyc_projected:** 5

### Refactored SweepTrackedOrders Skeleton

```csharp
private int SweepTrackedOrders(bool force)
{
    var trackedDicts = BuildTrackedDictList(force);
    int trackedCancels = 0;
    foreach (var dict in trackedDicts)
        trackedCancels += SweepDictionary(dict);
    return trackedCancels;
}
```

CYC = 2 ✅

### BuildTrackedDictList Skeleton

```csharp
private ConcurrentDictionary<string, Order>[] BuildTrackedDictList(bool force)
{
    return force
        ? new ConcurrentDictionary<string, Order>[]
          { entryOrders, stopOrders, target1Orders, target2Orders,
            target3Orders, target4Orders, target5Orders }
        : new ConcurrentDictionary<string, Order>[] { entryOrders };
}
```

CYC = 2 ✅

### SweepDictionary Skeleton

```csharp
private int SweepDictionary(ConcurrentDictionary<string, Order> dict)
{
    if (dict == null) return 0;
    int count = 0;
    foreach (var kvp in dict.ToArray())
    {
        Order ord = kvp.Value;
        if (ord == null || IsOrderTerminal(ord)) continue;
        try
        {
            CancelOrderOnAccount(ord, ord.Account);
            count++;
        }
        catch { }
    }
    return count;
}
```

CYC = 5 ✅

---

## Jane Street KB Alignment

| Principle | Application |
|---|---|
| **carl_cook: zero-alloc hot path** | `BuildTrackedDictList` allocates only once; `SweepDictionary` avoids LINQ (uses `dict.ToArray()` which is already present) |
| **carl_cook: single responsibility** | Each helper does exactly one thing: build list vs sweep one dict |
| **carl_cook: avoid LINQ** | No LINQ introduced; existing `ToArray()` pattern preserved |
| **gjengset: no new lock() blocks** | No locks added; `ConcurrentDictionary` thread-safety preserved |
| **trading_billions: CYC <= 8 per helper** | All helpers CYC <= 8 (max 5) |
| **trading_billions: defense in depth** | Null guards preserved in `SweepDictionary` for both dict and ord |
| **trading_billions: single responsibility** | `BuildTrackedDictList` owns list construction; `SweepDictionary` owns per-dict cancel logic |

---

## V12.23 Scope Compliance

| Check | Status |
|---|---|
| Single method targeted | PASS |
| Helpers extracted from target only | PASS |
| Caller `CancelAllV12GtcOrders` not modified | PASS |
| No cross-file refactoring | PASS — new helpers are private same-file |
| `IsOrderTerminal` called not modified | PASS — used as-is, no signature change |
| Boundary matches 01-scope-boundary.md | PASS |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Wave** | 7 |
| **Epic** | EPIC-W7-060 |
| **Phase** | 2 |
| **CYC Baseline** | 11 |
| **max_cyc_projected** | 5 |
| **Extractions** | 2 |
| **Scope Verdict** | PASS |
