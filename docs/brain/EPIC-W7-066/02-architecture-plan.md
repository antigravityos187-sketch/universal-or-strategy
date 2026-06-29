# Phase 2: Architecture Plan — EPIC-W7-066

## Method Under Extraction

- **Method:** `RemoveFsmOrderIdMappings`
- **Source File:** `src/V12_002.Symmetry.BracketFSM.cs`
- **Lines:** 103–125
- **Original CYC:** 10
- **Class:** `V12_002` (partial)

### jcodemunch get_context_bundle result
Symbol `RemoveFsmOrderIdMappings` resolved via `get_symbol_source` (context_bundle returned not-found; search by exact ID succeeded). Confirmed source at lines 103–125:
```csharp
private void RemoveFsmOrderIdMappings(FollowerBracketFSM fsm)
{
    if (fsm == null) return;
    if (fsm.EntryOrder != null && !string.IsNullOrEmpty(fsm.EntryOrder.OrderId))
        _orderIdToFsmKey.TryRemove(fsm.EntryOrder.OrderId, out _);
    if (!string.IsNullOrEmpty(fsm.ReplacingCancelOrderId))
        _orderIdToFsmKey.TryRemove(fsm.ReplacingCancelOrderId, out _);
    if (fsm.StopOrder != null && !string.IsNullOrEmpty(fsm.StopOrder.OrderId))
        _orderIdToFsmKey.TryRemove(fsm.StopOrder.OrderId, out _);
    if (fsm.Targets == null) return;
    foreach (Order target in fsm.Targets)
    {
        if (target != null && !string.IsNullOrEmpty(target.OrderId))
            _orderIdToFsmKey.TryRemove(target.OrderId, out _);
    }
}
```
Callee is `_orderIdToFsmKey` (`ConcurrentDictionary<string,string>`, declared at `src/V12_002.cs:836`).

### jcodemunch get_call_hierarchy result
- **Callers (depth 1):** `TryTerminateFollowerBracket` (line 127, same file) — sole direct caller, AST-resolved.
- **Callers (depth 2):** Not reached (depth_reached=1); transitive callers are `V12_002.REAPER.Audit` and `V12_002.Orders.Management.Cleanup` as documented in Phase 0.
- **Callees:** `_orderIdToFsmKey` (constant, `src/V12_002.cs:836`) — all mutations via atomic `ConcurrentDictionary.TryRemove`.

### jcodemunch get_dependency_graph result
File `src/V12_002.Symmetry.BracketFSM.cs` has **0 import edges** (no explicit file-level imports/importers detected by the graph — the file is a C# partial class and links to sibling partials via the compiler, not import statements). Node count: 1. This confirms no cross-file blast radius from the extraction itself.

### jcodemunch get_extraction_candidates result
No candidates returned (method lacks external callers meeting the min_callers=1 threshold in the import graph). This is expected: `RemoveFsmOrderIdMappings` is private and called only from within the same file. The extraction design proceeds from direct source analysis.

---

## Sequential Thinking Summary

**Thought 5 (Final Verdict):** Extract exactly 2 private helper methods from `RemoveFsmOrderIdMappings`.

- **`RemoveOrderIdIfPresent(Order order)`** — private void; guards `order != null && !IsNullOrEmpty(order.OrderId)`; calls `TryRemove`. Projected CYC=3. Responsibility: remove a single `Order`'s OrderId mapping if the order and its id are valid.
- **`RemoveTargetOrderIds(IEnumerable<Order> targets)`** — private void; early-return null guard; foreach iterates and delegates per element to `RemoveOrderIdIfPresent`. Projected CYC=3. Responsibility: remove all target order ID mappings from an FSM's `Targets` collection.
- **Parent after extraction** — null guard on `fsm`, call `RemoveOrderIdIfPresent(fsm.EntryOrder)`, inline guard+remove for `fsm.ReplacingCancelOrderId` (bare string — no `Order` wrapper, cannot use Order helper), call `RemoveOrderIdIfPresent(fsm.StopOrder)`, call `RemoveTargetOrderIds(fsm.Targets)`. Projected CYC=3.
- **Max CYC projected = 3.** All criteria satisfied. Jane Street alignment CONFIRMED.

---

## Extraction Plan

| Helper Method Name | Responsibility | Projected CYC |
|---|---|---|
| `RemoveOrderIdIfPresent(Order order)` | Guard `order != null && !IsNullOrEmpty(order.OrderId)`, then call `_orderIdToFsmKey.TryRemove(order.OrderId, out _)`. Eliminates the repeated 2-branch null+empty guard that appears for EntryOrder, StopOrder, and each Targets element. | 3 |
| `RemoveTargetOrderIds(IEnumerable<Order> targets)` | Guard `targets == null` with early return, iterate with `foreach`, call `RemoveOrderIdIfPresent(target)` per element. Extracts the loop body per Jane Street Extract Loop Body rule. | 3 |

### Projected Method Signatures

```csharp
// Helper 1 — collapses null+IsNullOrEmpty guard for any Order field
private void RemoveOrderIdIfPresent(Order order)
{
    if (order != null && !string.IsNullOrEmpty(order.OrderId))
        _orderIdToFsmKey.TryRemove(order.OrderId, out _);
}

// Helper 2 — extracts foreach loop body (Jane Street: Extract Loop Body)
private void RemoveTargetOrderIds(IEnumerable<Order> targets)
{
    if (targets == null)
        return;
    foreach (Order target in targets)
        RemoveOrderIdIfPresent(target);
}

// Parent after extraction
private void RemoveFsmOrderIdMappings(FollowerBracketFSM fsm)
{
    if (fsm == null)
        return;
    RemoveOrderIdIfPresent(fsm.EntryOrder);
    if (!string.IsNullOrEmpty(fsm.ReplacingCancelOrderId))
        _orderIdToFsmKey.TryRemove(fsm.ReplacingCancelOrderId, out _);
    RemoveOrderIdIfPresent(fsm.StopOrder);
    RemoveTargetOrderIds(fsm.Targets);
}
```

---

## Parent Method After Extraction

- **Remaining logic:** Null guard on `fsm` (early return), delegate `EntryOrder` to `RemoveOrderIdIfPresent`, inline `ReplacingCancelOrderId` bare-string guard (cannot use Order helper — field is a string, not an `Order`), delegate `StopOrder` to `RemoveOrderIdIfPresent`, delegate `Targets` collection to `RemoveTargetOrderIds`.
- **Projected CYC:** 3 (1 base + 1 null-fsm branch + 1 ReplacingCancelOrderId empty-string branch)

---

## max_cyc_projected: 3
## extraction_count: 2

---

## Jane Street Alignment

| Rule | Status |
|---|---|
| CYC<=8 achieved | **YES** — max projected CYC = 3 across all methods |
| Single-responsibility per helper | **YES** — `RemoveOrderIdIfPresent` handles one order field; `RemoveTargetOrderIds` handles the collection iteration |
| Lock-free/Actor pattern preserved | **YES** — all mutations via `ConcurrentDictionary.TryRemove` (individually atomic); no `lock()` blocks introduced |
| Illegal states unrepresentable | **YES** — `RemoveOrderIdIfPresent` makes it structurally impossible for null or empty-string OrderIds to reach `TryRemove`; invalid key removal path cannot be reached |
| Zero-allocation hot path | **YES** — `foreach` used instead of LINQ to avoid closure allocations; helpers are stack-frame only with no heap allocations |
| Guard clauses (early returns) | **YES** — `if (fsm == null) return` and `if (targets == null) return` are explicit guard clauses |
| Extract Loop Body | **YES** — `foreach` body extracted into `RemoveOrderIdIfPresent(target)` |
| Single-method epic (V12.23) | **YES** — scope confined to `RemoveFsmOrderIdMappings` + 2 new private helpers in the same file |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Epic** | EPIC-W7-066 |
| **Wave** | 7 |
| **Phase** | 2 — Architecture Planning |
| **Bobcoins Used** | 1.5 |
| **Execution Time** | 2026-06-29T01:15:00Z |
| **jcodemunch tools called** | resolve_repo, get_symbol_source (fallback from get_context_bundle not-found), get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **Input** | docs/brain/EPIC-W7-066/01-scope-boundary.md |
| **Output** | docs/brain/EPIC-W7-066/02-architecture-plan.md |
| **CYC Before** | 10 |
| **CYC After (max projected)** | 3 |
| **Extraction Count** | 2 |
