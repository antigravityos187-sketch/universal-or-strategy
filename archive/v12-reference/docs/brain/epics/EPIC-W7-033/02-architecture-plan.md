# EPIC-W7-033 — Phase 2: Architecture Plan

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:10:00Z
**Input:** docs/brain/EPIC-W7-033/01-scope-boundary.md

---

## Summary

**Target method:** `FlattenSinglePosition(string entryName, PositionInfo pos)`
**Source file:** [`src/V12_002.Orders.Management.Flatten.cs`](../../src/V12_002.Orders.Management.Flatten.cs:441)
**Baseline CYC:** 27
**Target CYC:** <= 8 (Jane Street strict standard)
**max_cyc_projected:** 5
**Extraction count:** 4 primary helpers + 1 predicate helper = 5 new private methods

---

## Extraction Plan

| Helper Name | Extracted Logic | Projected CYC | Jane Street Rule |
|---|---|---|---|
| `ClearPendingStopOrders(string entryName)` | `RequestStopCancelLifecycleSafe()` call + `pendingStopReplacements.TryRemove()` if-branch + associated `Print()` statements | 2 | trading_billions: single responsibility — stop-state cleanup |
| `CancelAllTargetOrders(string entryName, PositionInfo pos)` | `for(tNum=1..5)` loop + `GetTargetOrdersDictionary()` + null checks + `IsOrderCancellable()` predicate call + `CancelOrderSafe()` | 5 | trading_billions: single responsibility — T1-T5 target teardown |
| `IsOrderCancellable(Order order)` | Compound `OrderState == Working \|\| Accepted \|\| Submitted` predicate (extracted from T1-T5 loop condition) | 4 | trading_billions: make validity semantics explicit; CYC <= 8 |
| `ResolveFlattenQuantity(PositionInfo pos)` | `try/catch` for `Position.Quantity` read + `Position != null` + `MarketPosition != Flat` guards + `livePositionQty > 0` fallback logic — returns `int` | 5 | carl_cook: zero-alloc pure value-type return; cold logging stays co-located |
| `SubmitFlattenMarketOrder(string entryName, PositionInfo pos, int flattenQty)` | `flattenQty > 0` guard + `Direction == Long` ternary for `Sell`/`BuyToCover` + `SubmitOrderUnmanaged()` + `flattenOrder == null` null guard + result `Print()` | 4 | carl_cook: single submission path; AggressiveInlining candidate |
| `FlattenSinglePosition` (parent — after extraction) | Thin orchestrator: `Print()` header + sequential calls to the 4 helpers above; zero decision branches remaining | **1** | trading_billions: orchestrator has no business logic |

---

## CYC Validation

All units confirmed <= 8. HARD REQUIREMENT satisfied.

| Unit | Branch Count | Projected CYC | Pass? |
|---|---|---|---|
| `FlattenSinglePosition` (parent) | 0 (pure orchestrator) | 1 | PASS |
| `ClearPendingStopOrders` | 1 (`TryRemove` if) | 2 | PASS |
| `CancelAllTargetOrders` | 1 loop + 1 null(tDict) + 1 TryGetValue + 1 null(tOrder) + 1 bool call | 5 | PASS |
| `IsOrderCancellable` | 3 `OrderState` OR-chain branches | 4 | PASS |
| `ResolveFlattenQuantity` | 1 try/catch + 1 Position null + 1 MarketPosition + 1 liveQty > 0 | 5 | PASS |
| `SubmitFlattenMarketOrder` | 1 flattenQty > 0 + 1 Direction + 1 flattenOrder null | 4 | PASS |

**max_cyc_projected: 5**

---

## Method Signatures

```csharp
// Thin orchestrator — replaces all logic in FlattenSinglePosition body
private void FlattenSinglePosition(string entryName, PositionInfo pos)

// Helper 1: Stop/replacement cleanup
private void ClearPendingStopOrders(string entryName)

// Helper 2: Cancel T1-T5 target orders
private void CancelAllTargetOrders(string entryName, PositionInfo pos)

// Helper 2a: Order state validity predicate
private bool IsOrderCancellable(Order order)

// Helper 3: Resolve safe flatten quantity
private int ResolveFlattenQuantity(PositionInfo pos)

// Helper 4: Submit market close order
private void SubmitFlattenMarketOrder(string entryName, PositionInfo pos, int flattenQty)
```

---

## Parent Body After Extraction (Design Target)

```csharp
private void FlattenSinglePosition(string entryName, PositionInfo pos)
{
    Print(string.Format(
        "FLATTEN: Closing filled {0} position",
        pos.Direction == MarketPosition.Long ? "LONG" : "SHORT"
    ));
    ClearPendingStopOrders(entryName);
    CancelAllTargetOrders(entryName, pos);
    int flattenQty = ResolveFlattenQuantity(pos);
    SubmitFlattenMarketOrder(entryName, pos, flattenQty);
}
```

Note: The `Direction` ternary in the `Print` call is read-only formatting with CYC impact of 1 (already counted in parent CYC=1 base). No extraction needed for that line.

---

## Scope Boundary Compliance

- **Callers NOT changed:** `FlattenFilledMasterPositions` (line 424), `FlattenAll` (line 264)
- **Method signature:** Unchanged — `private void FlattenSinglePosition(string entryName, PositionInfo pos)`
- **New methods:** All `private` in same partial class — no interface changes
- **Files touched:** `src/V12_002.Orders.Management.Flatten.cs` only
- **V12.23 protocol:** PASS — ONE EPIC = ONE CONCERN

---

## Jane Street KB Application

| Rule Source | Applied Pattern | Location |
|---|---|---|
| `carl_cook` | Zero-alloc: all helpers return `void` or `int`/`bool` value types; no LINQ | All helpers |
| `carl_cook` | Extract cold logging out-of-line: `Print()` calls remain in helpers alongside logic but helpers are `NoInlining`-safe (infrequently called) | `ClearPendingStopOrders`, `SubmitFlattenMarketOrder` |
| `carl_cook` | `[AggressiveInlining]` candidates: `IsOrderCancellable` (tiny 3-branch predicate), `ResolveFlattenQuantity` (pure computation) | Noted for Phase 5 implementation |
| `gjengset` | No new `lock()` blocks introduced; `ResolveFlattenQuantity` uses try/catch only; `ClearPendingStopOrders` calls existing atomic `TryRemove` | All helpers |
| `gjengset` | `volatile` + `Thread.MemoryBarrier` not applicable here (no new shared state introduced) | N/A |
| `trading_billions` | Single responsibility per helper — each helper has exactly one named concern | All helpers |
| `trading_billions` | Defense in depth — `IsOrderCancellable` makes order-state check explicit and reusable | `IsOrderCancellable` |
| `trading_billions` | CYC <= 8 for every unit — max projected = 5 | All units PASS |

---

## MCP Evidence

### resolve_repo
- **Repo:** `antigravityos187-sketch/universal-or-strategy`
- **Status:** indexed (5,147 symbols, 2,000 files)
- **Source root:** `/home/malhitticrypto/universal-or-strategy`

### search_symbols (symbol ID resolution)
- **Confirmed symbol:** `src/V12_002.Orders.Management.Flatten.cs::V12_002.FlattenSinglePosition#method`
- **Line:** 441, **End line:** 557
- **Signature:** `private void FlattenSinglePosition(string entryName, PositionInfo pos)`

### get_context_bundle
- **Source lines read:** 441–557 (117 lines)
- **Key discoveries:**
  - T1-T5 loop with compound OrderState check (3 branches)
  - try/catch block for Position.Quantity read
  - Direction ternary for Sell/BuyToCover submission
  - `pendingStopReplacements.TryRemove` with `Interlocked.Decrement`
  - `SubmitOrderUnmanaged` called with 7-parameter overload

### get_call_hierarchy
- **Callers (depth=2):** `FlattenFilledMasterPositions` (direct, line 424), `FlattenAll` (indirect, line 264)
- **Callees (depth=1):** `RequestStopCancelLifecycleSafe`, `GetTargetOrdersDictionary`, `CancelOrderSafe`, `SubmitOrderUnmanaged`, `Print` (via LogBuffer.Format)
- **Callees (depth=2):** `LogBuffer.FormatInternal`, `LogBuffer.ValidateThreadAffinity`, `IsOrderTerminal`

### get_dependency_graph
- **Result:** `src/V12_002.Orders.Management.Flatten.cs` has 0 cross-file import edges in the index
- **Interpretation:** File is a partial class in a partial class project — dependencies resolved at compile time via partial class merging, not import statements. No cross-file blast radius from this extraction.

---

## Sequential Thinking Evidence

### Thought 1 — Complexity Drivers from Actual Source
Identified 17+ decision points in `FlattenSinglePosition` body:
- Direction check (Print log)
- `TryRemove` branch
- `for(tNum=1..5)` loop
- `tDict` null check
- `TryGetValue` check
- `tOrder` null check
- `OrderState == Working || Accepted || Submitted` (3 branches)
- try/catch for Position read
- `Position != null`
- `MarketPosition != Flat`
- `livePositionQty > 0`
- `flattenQty > 0`
- Direction ternary for order submission
- `flattenOrder == null`
Combined with Boolean short-circuit operators, total reaches CYC=27.

### Thought 2 — Extraction Strategy
Named 4 primary helpers with clear single responsibility per Jane Street rule `trading_billions`. Added `IsOrderCancellable` predicate to eliminate compound condition from T1-T5 loop, preventing CancelAllTargetOrders from approaching CYC=8 limit.

### Thought 3 — CYC Validation
All 6 units (parent + 5 helpers) confirmed CYC <= 8. max_cyc_projected=5. Callers `FlattenFilledMasterPositions` and `FlattenAll` confirmed untouched (from call hierarchy). Jane Street alignment verified for all 3 KB rules.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | batch |
| **Phase** | 2 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-033 |
| **Method** | FlattenSinglePosition |
| **Source** | src/V12_002.Orders.Management.Flatten.cs |
| **Baseline CYC** | 27 |
| **max_cyc_projected** | 5 |
| **Extractions** | 5 (4 primary + 1 predicate) |
