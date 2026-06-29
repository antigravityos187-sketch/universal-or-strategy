# Phase 2: Architecture Plan — EPIC-W7-064

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:15:00Z
**Input:** docs/brain/EPIC-W7-064/01-scope-boundary.md

---

## Method Under Extraction

- **Method:** `ResolveFsm_ByScan`
- **Source File:** `src/V12_002.Symmetry.BracketFSM.cs`
- **Lines:** 209–246
- **Original CYC:** 11
- **Signature:** `private FollowerBracketFSM ResolveFsm_ByScan(string accountAlias, string orderId)`

### jcodemunch get_context_bundle result
Symbol `ResolveFsm_ByScan` not found via bundle (ambiguous ID resolved via search_symbols fallback). Full source retrieved via `get_symbol_source` with canonical ID `src/V12_002.Symmetry.BracketFSM.cs::V12_002.ResolveFsm_ByScan#method`. Method is 38 lines, docstring: "Tier 3: Last-resort O(N) scan with backfill. Scan order: StopOrder -> Targets[0-4] -> EntryOrder." Confirmed two instances in index (src/ and src-vm-backup/); canonical target is `src/` variant.

### jcodemunch get_call_hierarchy result
- **Callers (depth 1):** `ResolveFsmFromEvent` — line 251, same file, AST-resolved
- **Callers (depth 2):** `ValidateFsmEventPreconditions` — line 272, same file, AST-resolved
- **Callees:** none — `ResolveFsm_ByScan` is a leaf method (calls no other indexed symbols)
- **Total caller count:** 2 (both in same file; no cross-file callers)

### jcodemunch get_dependency_graph result
- **File:** `src/V12_002.Symmetry.BracketFSM.cs`
- **Imports:** 0 (monolithic partial-class pattern; all dependencies within same compilation unit)
- **Importers:** 0 (no other files import this file directly in the index)
- **Edge count:** 0 — extraction is fully contained within single file, no import rewrites needed

### jcodemunch get_extraction_candidates result
No candidates returned by tool (min_complexity=3, min_callers=1). This is expected: the tool requires cross-file callers for extraction scoring; `ResolveFsm_ByScan` is a private method with a single same-file caller. Manual analysis from source confirms the extraction candidate `MatchOrderInFsm` as described below.

---

## Sequential Thinking Summary

**Thought 5 (final):** Architecture decision confirmed. Extract exactly 1 helper method: `MatchOrderInFsm(FollowerBracketFSM f, string orderId) : FollowerBracketFSM`. This achieves parent CYC 11 → 5 and helper CYC 5, both well within the Jane Street ≤8 mandate. The `foundT` dead-code flag (lines 225, 234-235) is removed — it is provably unreachable because `return f` at line 232 exits the method before the `if (foundT) break` guard can observe `foundT = true`. The extraction is minimal (1 helper, same file, same partial class), preserves all `_orderIdToFsmKey` backfill side-effects, introduces no locks, makes no heap allocations, and maintains the scan-order invariant (Stop → Targets[0-4] → Entry). Callers `ResolveFsmFromEvent` and `ValidateFsmEventPreconditions` are unaffected — method signature is unchanged.

---

## Extraction Plan

| Helper Method Name | Responsibility | Projected CYC |
|---|---|---|
| `MatchOrderInFsm` | Given a single `FollowerBracketFSM f` and `orderId`, perform the 3-slot scan (StopOrder → Targets[0-4] → EntryOrder), backfill `_orderIdToFsmKey` on match, and return `f` or `null`. Dead-code `foundT` flag removed. | 5 |

### Helper Method Signature

```csharp
private FollowerBracketFSM MatchOrderInFsm(FollowerBracketFSM f, string orderId)
```

### Helper Method Body (reference implementation)

```csharp
private FollowerBracketFSM MatchOrderInFsm(FollowerBracketFSM f, string orderId)
{
    if (f.StopOrder != null && f.StopOrder.OrderId == orderId)
    {
        _orderIdToFsmKey[orderId] = f.EntryName;
        return f;
    }

    for (int i = 0; i < 5; i++)
    {
        if (f.Targets[i] != null && f.Targets[i].OrderId == orderId)
        {
            _orderIdToFsmKey[orderId] = f.EntryName;
            return f;
        }
    }

    if (f.EntryOrder != null && f.EntryOrder.OrderId == orderId)
    {
        _orderIdToFsmKey[orderId] = f.EntryName;
        return f;
    }

    return null;
}
```

---

## Parent Method After Extraction

### Remaining logic

The parent `ResolveFsm_ByScan` retains only:
1. Guard clause: `if (string.IsNullOrEmpty(orderId)) return null`
2. Outer `foreach` over `_followerBrackets.Values`
3. Account-filter `continue`: `if (f.AccountName != accountAlias) continue`
4. Delegating call to `MatchOrderInFsm(f, orderId)` with null-check return

### Parent Reference Implementation

```csharp
private FollowerBracketFSM ResolveFsm_ByScan(string accountAlias, string orderId)
{
    if (string.IsNullOrEmpty(orderId))
        return null;

    foreach (var f in _followerBrackets.Values)
    {
        if (f.AccountName != accountAlias)
            continue;

        var match = MatchOrderInFsm(f, orderId);
        if (match != null)
            return match;
    }

    return null;
}
```

### Projected CYC: 5

CYC breakdown: 1 (base) + 1 (IsNullOrEmpty guard) + 1 (foreach) + 1 (AccountName filter) + 1 (match != null) = **5**

---

## max_cyc_projected: 5
## extraction_count: 1

---

## Jane Street Alignment

| Rule | Status |
|---|---|
| CYC<=8 achieved | YES — parent: 5, helper: 5, max: 5 |
| Single-responsibility per helper | YES — `MatchOrderInFsm` does exactly one thing: match orderId against a single FSM entry and backfill |
| Lock-free/Actor pattern preserved | YES — `_orderIdToFsmKey` is ConcurrentDictionary (lock-free atomic set); no `lock()` added or removed |
| Illegal states unrepresentable | YES — orderId null-guard remains in parent before any iteration; `MatchOrderInFsm` receives non-null `f` guaranteed by foreach; dead-code `foundT` flag removed eliminating false branch |
| Zero-allocation hot path | YES — no heap allocations; helper passes existing references, returns pre-existing FollowerBracketFSM reference |
| Extract Guard Clauses | YES — `IsNullOrEmpty` guard is early-return at top of parent |
| Extract to Named Helper Methods | YES — `MatchOrderInFsm` reflects single concern (FSM slot-scan with backfill) |
| FSM Decomposition | YES — 3-slot scan (Stop/Targets/Entry) encapsulated in dedicated helper; parent is pure orchestration |
| Dead-code removal | YES — `bool foundT` and `if (foundT) break` are provably unreachable and removed |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 1.5 |
| **Execution Time** | 2026-06-29T01:15:00Z |
| **jcodemunch tools called** | resolve_repo, get_context_bundle (fallback: search_symbols + get_symbol_source), get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-064 |
| **Phase** | 2 — Architecture Planning |
