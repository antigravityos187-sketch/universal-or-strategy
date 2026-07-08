# Phase 2: Architecture Plan — EPIC-W7-035

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:10:00Z
**Input:** docs/brain/EPIC-W7-035/01-scope-boundary.md

---

## Method Under Extraction

- **Method:** `SyncLimitTarget`
- **Source File:** `src/V12_002.Orders.Management.StopSync.cs`
- **Lines:** 176–336 (161 LOC)
- **Original CYC:** 34
- **Class:** `partial class V12_002 : Strategy`
- **Namespace:** `NinjaTrader.NinjaScript.Strategies`
- **Access Modifier:** `private`

### Signature

```csharp
private void SyncLimitTarget(
    string entryName,
    PositionInfo pos,
    int targetNum,
    int targetQty,
    ConcurrentDictionary<string, Order> targetDict,
    Order existingOrder,
    bool hasWorkingOrder,
    ref int refreshed
)
```

### jcodemunch get_context_bundle result

Symbol not found via bundle (repo index resolves to two candidates due to `src/` and `src-vm-backup/` mirrors). Search_symbols fallback confirmed the canonical definition at `src/V12_002.Orders.Management.StopSync.cs` line 176 with the full 8-parameter signature including `ref int refreshed`. Summary field was empty (no docstring), indicating the method is undocumented — consistent with an organic growth hotspot.

### jcodemunch get_call_hierarchy result

- **Callers (depth 1):** `RefreshActivePositionOrders` — `src/V12_002.Orders.Management.StopSync.cs` line 37 (AST-resolved, same file)
- **Callers (depth 2):** None — `RefreshActivePositionOrders` is the sole indirect ancestor in the call chain visible to jcodemunch
- **Callees (depth 1):**
  - `CalculateTargetPriceFromPos` — `src/V12_002.PositionInfo.cs` line 264 (AST-inferred)
  - `LogBuffer.Format` — `src/V12_002.Perf.LogBuffer.cs` line 28 (AST-inferred, via Print calls)
- **Callees (depth 2):**
  - `CalculateTargetPrice` — `src/V12_002.PositionInfo.cs` line 165 (via CalculateTargetPriceFromPos)
  - `LogBuffer.ValidateThreadAffinity` — `src/V12_002.Perf.LogBuffer.cs` line 119
  - `LogBuffer.FormatInternal` — `src/V12_002.Perf.LogBuffer.cs` line 56
- **NinjaTrader broker API calls** (not in jcodemunch index; confirmed from source): `ChangeOrder`, `SubmitOrderUnmanaged`

### jcodemunch get_dependency_graph result

File `src/V12_002.Orders.Management.StopSync.cs` has **0 explicit import edges** in either direction per jcodemunch (node_count=1, edge_count=0). This reflects the NinjaTrader partial class architecture where all files belong to the same `partial class V12_002` and are compiled as a single unit — no using/import declarations appear in the file. Cross-file impact is therefore confined to the partial class scope; no external module contracts are touched by this refactor.

### jcodemunch get_extraction_candidates result

No candidates returned (min_complexity=3, min_callers=1). The extraction candidates tool requires cross-file call data; since `SyncLimitTarget` is a private method called from within the same file, the intra-file callee relationship is not surfaced. This is consistent with the architecture: all helpers will be `private` within the same partial class. Manual source analysis (confirmed from source read, lines 176–336) and phase 0/1 artifacts provide the authoritative extraction plan.

---

## Sequential Thinking Summary

**Final thought (5 of 5):**

After extracting all 3 helpers, `SyncLimitTarget` becomes a thin coordinator with residual CYC 4:

```
double newPrice = CalculateTargetPriceFromPos(...)  // no branch
if (newPrice <= 0) { Print(...); return; }            // +1
if (hasWorkingOrder)                                   // +1
    SyncLimitTarget_Reprice(...)
else                                                   // +1
    SyncLimitTarget_Submit(...)
```

Complete projected CYC table:

| Symbol | CYC |
|---|---|
| `SetTargetPrice` | 7 |
| `SyncLimitTarget_Reprice` | 4 |
| `SyncLimitTarget_Submit` | 4 |
| `SyncLimitTarget` (parent, post-extraction) | 4 |
| **Max projected CYC** | **7** |

Jane Street alignment verdict: All ≤8 ✓. Single-responsibility ✓. Lock-free ✓. Zero-allocation hot paths ✓. `ref int refreshed` correctly threaded ✓. Direction ternary resolved to named `exitAction` variable (matches `RestoreCascadedTargets` pattern at line 1007) ✓.

---

## Extraction Plan

| Helper Method Name | Responsibility | Projected CYC |
|---|---|---|
| `SetTargetPrice(PositionInfo pos, int targetNum, double price)` | Stamp `pos.Target{n}Price = price` for slot 1-5; guard default (invalid targetNum). Eliminates BOTH duplicated `switch(targetNum)` blocks. | 7 |
| `SyncLimitTarget_Reprice(string entryName, PositionInfo pos, int targetNum, Order existingOrder, double newPrice, ref int refreshed)` | Execute reprice of existing working order: delta guard → `ChangeOrder` → `SetTargetPrice` → `Print` → `refreshed++`. One `try/catch`. | 4 |
| `SyncLimitTarget_Submit(string entryName, PositionInfo pos, int targetNum, int targetQty, ConcurrentDictionary<string,Order> targetDict, double newPrice, ref int refreshed)` | Submit new unmanaged limit order: resolve `exitAction` → `SubmitOrderUnmanaged` → null guard → `targetDict` write → `SetTargetPrice` → `Print` → `refreshed++`. One `try/catch`. | 4 |

### SetTargetPrice — Detailed Design

```csharp
private void SetTargetPrice(PositionInfo pos, int targetNum, double price)
{
    switch (targetNum)
    {
        case 1: pos.Target1Price = price; break;
        case 2: pos.Target2Price = price; break;
        case 3: pos.Target3Price = price; break;
        case 4: pos.Target4Price = price; break;
        case 5: pos.Target5Price = price; break;
        default: return;  // Invalid targetNum — guard, no-op
    }
}
```

**CYC breakdown:** baseline(1) + cases 1-5(+5) + default(+1) = 7

### SyncLimitTarget_Reprice — Detailed Design

```csharp
private void SyncLimitTarget_Reprice(
    string entryName,
    PositionInfo pos,
    int targetNum,
    Order existingOrder,
    double newPrice,
    ref int refreshed
)
{
    if (Math.Abs(existingOrder.LimitPrice - newPrice) < tickSize)
    {
        Print(string.Format("[SYNC_ALL] T{0} {1}: Price unchanged at {2:F2} -- no action",
            targetNum, entryName, newPrice));
        return;
    }

    try
    {
        ChangeOrder(existingOrder, existingOrder.Quantity, newPrice, 0);
        SetTargetPrice(pos, targetNum, newPrice);
        Print(string.Format("[SYNC_ALL] T{0} {1}: Repriced -> {2:F2}", targetNum, entryName, newPrice));
        refreshed++;
    }
    catch (Exception ex)
    {
        Print(string.Format("[SYNC_ALL] T{0} {1}: ChangeOrder failed -- {2}",
            targetNum, entryName, ex.Message));
    }
}
```

**CYC breakdown:** baseline(1) + delta guard(+1) + early-return else path(+1) + try/catch(+1) = 4

### SyncLimitTarget_Submit — Detailed Design

```csharp
private void SyncLimitTarget_Submit(
    string entryName,
    PositionInfo pos,
    int targetNum,
    int targetQty,
    ConcurrentDictionary<string, Order> targetDict,
    double newPrice,
    ref int refreshed
)
{
    OrderAction exitAction = pos.Direction == MarketPosition.Long
        ? OrderAction.Sell
        : OrderAction.BuyToCover;

    try
    {
        Order newLimit = SubmitOrderUnmanaged(
            0, exitAction, OrderType.Limit, targetQty, newPrice, 0,
            "", "T" + targetNum + "_" + entryName);

        if (newLimit != null)
        {
            targetDict[entryName] = newLimit;
            SetTargetPrice(pos, targetNum, newPrice);
            Print(string.Format("[SYNC_ALL] T{0} {1}: New limit submitted @ {2:F2} qty={3}",
                targetNum, entryName, newPrice, targetQty));
            refreshed++;
        }
        else
        {
            Print(string.Format("[SYNC_ALL] T{0} {1}: SubmitOrderUnmanaged returned null @ {2:F2}",
                targetNum, entryName, newPrice));
        }
    }
    catch (Exception ex)
    {
        Print(string.Format("[SYNC_ALL] T{0} {1}: Submit failed -- {2}",
            targetNum, entryName, ex.Message));
    }
}
```

**CYC breakdown:** baseline(1) + direction ternary(+1) + try/catch(+1) + null guard(+1) = 4

---

## Parent Method After Extraction

### Residual SyncLimitTarget body

```csharp
private void SyncLimitTarget(
    string entryName,
    PositionInfo pos,
    int targetNum,
    int targetQty,
    ConcurrentDictionary<string, Order> targetDict,
    Order existingOrder,
    bool hasWorkingOrder,
    ref int refreshed
)
{
    double newPrice = CalculateTargetPriceFromPos(pos.Direction, pos.EntryPrice, pos, targetNum);
    if (newPrice <= 0)
    {
        Print(string.Format("[SYNC_ALL] T{0} {1}: Calculated price invalid ({2:F2}) -- skipped",
            targetNum, entryName, newPrice));
        return;
    }

    if (hasWorkingOrder)
        SyncLimitTarget_Reprice(entryName, pos, targetNum, existingOrder, newPrice, ref refreshed);
    else
        SyncLimitTarget_Submit(entryName, pos, targetNum, targetQty, targetDict, newPrice, ref refreshed);
}
```

- **Remaining logic:** Price calculation → invalid-price guard → two-arm dispatch to Reprice or Submit helpers
- **Projected CYC:** 4 (baseline 1 + newPrice guard +1 + hasWorkingOrder +1 + else +1)

---

## max_cyc_projected: 7
## extraction_count: 3

---

## Jane Street Alignment

| Rule | Status | Notes |
|---|---|---|
| CYC<=8 achieved | **YES** | Max CYC across all symbols = 7 (SetTargetPrice); parent = 4 |
| Single-responsibility per helper | **YES** | SetTargetPrice: price stamping only. _Reprice: reprice path only. _Submit: submit path only. Parent: coordinator only. |
| Lock-free/Actor pattern preserved | **YES** | No `lock()` blocks introduced. NinjaTrader API calls remain synchronous on NT dispatch thread as before. No state mutation model changed. |
| Zero-allocation hot paths | **YES** | `exitAction` is a stack-local enum (value type). All `string.Format` allocations match existing code style. No new heap objects introduced. |
| Guard clause extraction | **YES** | `newPrice <= 0` early-return guard preserved at parent level. Delta guard in Reprice arm inverted to early-return pattern. |
| Illegal states unrepresentable | **YES** | `SetTargetPrice` default case guards against invalid `targetNum` (impossible path made explicit). Direction ternary eliminates dual-call maintenance trap. |
| `ref int refreshed` threading | **YES** | Correctly threaded through to both `SyncLimitTarget_Reprice` and `SyncLimitTarget_Submit`. Refresh counter semantics unchanged. |
| Extract loop body pattern | **N/A** | No loop with complex body; pattern does not apply here. |
| Named helper methods (private, single concern) | **YES** | All 3 helpers are `private` to the same `partial class V12_002`, no external file changes. |

---

## Risk Assessment

| Dimension | Assessment |
|---|---|
| Call-site impact | None — `RefreshActivePositionOrders` (sole caller) is not modified |
| Cross-file impact | None — all helpers are `private` to `partial class V12_002` |
| Broker API impact | None — `ChangeOrder` and `SubmitOrderUnmanaged` calls preserved verbatim in helpers |
| Shared state impact | None — `targetDict` and `pos.Target{n}Price` writes preserved verbatim |
| `ref int refreshed` | Low — must be threaded to both arm helpers (verified in design above) |
| **Overall risk** | **Low** |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Epic** | EPIC-W7-035 |
| **Wave** | 7 |
| **Phase** | 2 — Architecture Planning |
| **Bobcoins Used** | 1.5 |
| **Execution Time** | 2026-06-29T01:10:00Z |
| **jcodemunch tools called** | resolve_repo, get_context_bundle (→ search_symbols fallback), get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **Extraction count** | 3 |
| **max_cyc_projected** | 7 |
| **Original CYC** | 34 |
| **Post-extraction parent CYC** | 4 |
