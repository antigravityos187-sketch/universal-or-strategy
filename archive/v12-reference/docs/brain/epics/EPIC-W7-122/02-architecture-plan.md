# EPIC-W7-122 — Phase 2: Architecture Plan

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:00:00Z
**Input:** docs/brain/EPIC-W7-122/01-scope-boundary.md

---

## Summary

Reduce `RemoveFsmOrderIdMappings` from CYC 10 to CYC 2 by extracting 3 private helper methods
into the same partial class in `src/V12_002.Symmetry.BracketFSM.cs`. The parent method becomes
a flat coordinator. All extracted helpers have projected CYC <= 8. max_cyc_projected = 3.

---

## Original Method Analysis

| Field | Value |
|---|---|
| **Method** | `RemoveFsmOrderIdMappings` |
| **File** | `src/V12_002.Symmetry.BracketFSM.cs` |
| **Lines** | 103-125 |
| **Original CYC** | 10 |
| **Caller Count** | 1 (`TryTerminateFollowerBracket`) |
| **Callees** | `_orderIdToFsmKey.TryRemove` (ConcurrentDictionary) |

### Original Source (lines 103-125)

```csharp
private void RemoveFsmOrderIdMappings(FollowerBracketFSM fsm)
{
    if (fsm == null)
        return;

    if (fsm.EntryOrder != null && !string.IsNullOrEmpty(fsm.EntryOrder.OrderId))
        _orderIdToFsmKey.TryRemove(fsm.EntryOrder.OrderId, out _);

    if (!string.IsNullOrEmpty(fsm.ReplacingCancelOrderId))
        _orderIdToFsmKey.TryRemove(fsm.ReplacingCancelOrderId, out _);

    if (fsm.StopOrder != null && !string.IsNullOrEmpty(fsm.StopOrder.OrderId))
        _orderIdToFsmKey.TryRemove(fsm.StopOrder.OrderId, out _);

    if (fsm.Targets == null)
        return;

    foreach (Order target in fsm.Targets)
    {
        if (target != null && !string.IsNullOrEmpty(target.OrderId))
            _orderIdToFsmKey.TryRemove(target.OrderId, out _);
    }
}
```

### CYC=10 Branch Decomposition

| Branch # | Code | CYC Contribution |
|---|---|---|
| 1 | `if (fsm == null)` | +1 |
| 2 | `fsm.EntryOrder != null` | +1 |
| 3 | `!string.IsNullOrEmpty(fsm.EntryOrder.OrderId)` | +1 |
| 4 | `!string.IsNullOrEmpty(fsm.ReplacingCancelOrderId)` | +1 |
| 5 | `fsm.StopOrder != null` | +1 |
| 6 | `!string.IsNullOrEmpty(fsm.StopOrder.OrderId)` | +1 |
| 7 | `if (fsm.Targets == null)` | +1 |
| 8 | `foreach (Order target in fsm.Targets)` | +1 |
| 9 | `target != null` | +1 |
| 10 | `!string.IsNullOrEmpty(target.OrderId)` | +1 |
| **Total** | | **10** |

### Logical Sub-Concerns

1. **FSM null guard** — pre-condition check; inlined in parent (1 branch)
2. **Entry-order removal** — compound null+empty guard + TryRemove for an `Order` object (2 branches)
3. **Replacing-cancel removal** — simple string-null guard + TryRemove for a raw `string` field (1 branch)
4. **Stop-order removal** — compound null+empty guard + TryRemove for an `Order` object (2 branches)
5. **Target-order removal** — array null guard + foreach iteration + per-element guard (4 branches)

Sub-concerns 2 and 4 are structurally identical and can share a single reusable helper.

---

## Extraction Plan

### Helper 1 — `RemoveSingleOrderMapping`

| Field | Value |
|---|---|
| **Signature** | `private void RemoveSingleOrderMapping(Order order)` |
| **Responsibility** | Guard-check one `Order` (null + empty OrderId), then TryRemove from `_orderIdToFsmKey` |
| **Projected CYC** | **3** |
| **Jane Street Pattern** | `carl_cook` — AggressiveInlining candidate; zero-alloc; hot-path leaf |

```csharp
[System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
private void RemoveSingleOrderMapping(Order order)
{
    if (order != null && !string.IsNullOrEmpty(order.OrderId))
        _orderIdToFsmKey.TryRemove(order.OrderId, out _);
}
```

Called by parent for: `fsm.EntryOrder` and `fsm.StopOrder` (deduplicates identical logic).

---

### Helper 2 — `RemoveReplacingCancelMapping`

| Field | Value |
|---|---|
| **Signature** | `private void RemoveReplacingCancelMapping(string cancelOrderId)` |
| **Responsibility** | Guard-check a raw string cancel-order ID, then TryRemove from `_orderIdToFsmKey` |
| **Projected CYC** | **2** |
| **Jane Street Pattern** | `trading_billions` — single responsibility; no coupling to Order object |

```csharp
private void RemoveReplacingCancelMapping(string cancelOrderId)
{
    if (!string.IsNullOrEmpty(cancelOrderId))
        _orderIdToFsmKey.TryRemove(cancelOrderId, out _);
}
```

---

### Helper 3 — `RemoveTargetOrderMappings`

| Field | Value |
|---|---|
| **Signature** | `private void RemoveTargetOrderMappings(Order[] targets)` |
| **Responsibility** | Guard-check the targets array for null, iterate, delegate each element to `RemoveSingleOrderMapping` |
| **Projected CYC** | **3** |
| **Jane Street Pattern** | `gjengset` — iteration kernel isolated; avoids false-sharing by keeping loop body minimal; `trading_billions` — single-responsibility array processor |

```csharp
private void RemoveTargetOrderMappings(Order[] targets)
{
    if (targets == null)
        return;

    foreach (Order target in targets)
        RemoveSingleOrderMapping(target);
}
```

---

### Parent Method After Extraction

| Field | Value |
|---|---|
| **Signature** | `private void RemoveFsmOrderIdMappings(FollowerBracketFSM fsm)` |
| **Projected CYC** | **2** |
| **Role** | Flat coordinator — single null guard, then delegates to 3 helpers |

```csharp
private void RemoveFsmOrderIdMappings(FollowerBracketFSM fsm)
{
    if (fsm == null)
        return;

    RemoveSingleOrderMapping(fsm.EntryOrder);
    RemoveReplacingCancelMapping(fsm.ReplacingCancelOrderId);
    RemoveSingleOrderMapping(fsm.StopOrder);
    RemoveTargetOrderMappings(fsm.Targets);
}
```

---

## CYC Projection Summary

| Symbol | Projected CYC | Passes <=8? |
|---|---|---|
| `RemoveFsmOrderIdMappings` (parent) | 2 | PASS |
| `RemoveSingleOrderMapping` | 3 | PASS |
| `RemoveReplacingCancelMapping` | 2 | PASS |
| `RemoveTargetOrderMappings` | 3 | PASS |
| **max_cyc_projected** | **3** | **PASS** |

**Reduction:** CYC 10 → 2 (parent), with helpers max CYC 3. Total extracted helpers: 3.

---

## Jane Street Alignment Notes

| Pattern Source | Rule Applied | Helper |
|---|---|---|
| `carl_cook` | AggressiveInlining on hot-path 2-branch leaf; zero-alloc | `RemoveSingleOrderMapping` |
| `carl_cook` | Extract cold logic out-of-line; keep hot path flat | Parent coordinator shape |
| `gjengset` | Isolate iteration kernel; avoid cache-line ping-pong from mixed concerns in one loop | `RemoveTargetOrderMappings` |
| `trading_billions` | Single responsibility per helper; defense-in-depth null guards at each level | All 3 helpers |

---

## Scope Compliance (V12.23)

| Check | Status |
|---|---|
| One epic = one concern | PASS |
| All helpers are private, same partial class | PASS |
| No caller signature changed (`TryTerminateFollowerBracket`) | PASS |
| No cross-file changes | PASS |
| No sibling method modifications | PASS |

---

## MCP Evidence

| Tool | Result |
|---|---|
| `resolve_repo` | Repo `antigravityos187-sketch/universal-or-strategy` indexed, 5147 symbols |
| `search_symbols` | `RemoveFsmOrderIdMappings` found at `src/V12_002.Symmetry.BracketFSM.cs:103` |
| `get_symbol_source` | Full body lines 103-125 retrieved, content_hash verified |
| `get_call_hierarchy` | 1 caller (`TryTerminateFollowerBracket`), 1 callee (`_orderIdToFsmKey`) |
| `get_dependency_graph` | BracketFSM.cs is a self-contained partial file; 0 external imports/importers |
| `get_file_outline` | Parent class `V12_002` (partial), method confirmed private, adjacent helpers identified |
| `get_extraction_candidates` | No automated candidates returned (method not multi-caller) — manual decomposition applied per Jane Street patterns |

---

## Sequential Thinking Evidence

| Thought | Conclusion |
|---|---|
| 1 — CYC Structure | 10 branches map to 5 logical sub-concerns: FSM null guard, entry-order, replacing-cancel, stop-order, targets array |
| 2 — Extraction Design | 3 helpers: `RemoveSingleOrderMapping` (CYC 3), `RemoveReplacingCancelMapping` (CYC 2), `RemoveTargetOrderMappings` (CYC 3); parent reduces to CYC 2 |
| 3 — Validation | All helpers CYC <= 8; max_cyc_projected = 3; Jane Street alignment confirmed; V12.23 scope compliance verified |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 2.5 |
| **Execution Time** | batch |
| **Phase** | 2 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-122 |
| **Original CYC** | 10 |
| **Extraction Count** | 3 |
| **max_cyc_projected** | 3 |
