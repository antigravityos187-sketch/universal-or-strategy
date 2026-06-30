# EPIC-W7-020 — Phase 4: Ticket Definitions

**Agent:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29
**Inputs:** docs/brain/EPIC-W7-020/02-architecture-plan.md, docs/brain/EPIC-W7-020/03-audit-report.md

---

## Summary

| Field | Value |
|---|---|
| **Method** | `HandleSecondaryOrderFilled` |
| **File** | `src/V12_002.Orders.Callbacks.cs` |
| **CYC Violator** | `HandleSecondaryOrderFilled_Stop` (CYC=10) |
| **ticket_count** | **1** |
| **projected_parent_cyc_after_all** | **4** |
| **max_cyc_projected** | **7** (existing `_Target`, no extraction needed) |
| **DNA Verdict** | PASS (Phase 3) |

---

## Ticket Definitions

### TICKET-W7-020-01

| Field | Value |
|---|---|
| **ticket_id** | `TICKET-W7-020-01` |
| **helper_name** | `TryCleanupStopByDictionaryLookup` |
| **concern** | Extract the foreach position-scan loop with mutation-safety guard from `HandleSecondaryOrderFilled_Stop` into a dedicated private helper. The extracted block iterates the pre-allocated `snapshot`, performs an exact `stopOrders.TryGetValue` + identity check, applies a `activePositions.ContainsKey` mutation-safety guard before calling `CleanupPosition`, and returns `true` on match. |
| **source_method** | `HandleSecondaryOrderFilled_Stop` |
| **source_file** | `src/V12_002.Orders.Callbacks.cs` |
| **lines_to_move** | ~20 lines — the `foreach (var kvp in snapshot)` block including: loop head, `TryGetValue && sOrder == order` condition, `ContainsKey` guard, `CleanupPosition(kvp.Key, ...)` call, else-branch stale-reference removal, and `return true` short-circuit |
| **cyc_before** | 10 |
| **cyc_reduction** | 6 |
| **projected_parent_cyc** | 4 |
| **projected_helper_cyc** | 5 |
| **helper_attribute** | `[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]` |
| **accessibility** | `private` |
| **jane_street_alignment** | zero-alloc (snapshot pre-allocated by caller), lock-free (`ConcurrentDictionary.TryGetValue`/`TryRemove`), no LINQ, `AggressiveInlining` on hot path, mutation-safety guard preserved verbatim |

#### New Helper Signature

```csharp
/// <summary>
/// Scans the position snapshot for a stop order matching the given order reference.
/// Applies mutation-safety guard before cleanup to prevent double-cleanup race conditions.
/// Extracted from HandleSecondaryOrderFilled_Stop.
/// </summary>
/// <param name="order">The filled stop order to match.</param>
/// <param name="snapshot">Pre-allocated snapshot of active positions.</param>
/// <param name="averageFillPrice">Average fill price for logging.</param>
/// <returns>True if a matching stop was found and handled.</returns>
[System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
private bool TryCleanupStopByDictionaryLookup(
    Order order,
    KeyValuePair<string, PositionInfo>[] snapshot,
    double averageFillPrice
)
```

#### Modified Caller Signature (unchanged contract)

```csharp
private bool HandleSecondaryOrderFilled_Stop(
    Order order,
    string orderName,
    double averageFillPrice,
    KeyValuePair<string, PositionInfo>[] snapshot
)
```

#### Post-Extraction Body of `_Stop` (projected)

```csharp
// 1. Prefix guard (1 branch)
if (!orderName.StartsWith(StopOrderPrefix) && !orderName.StartsWith(StopOrderPrefixShort))
    return false;

// 2. Dictionary-lookup cleanup via extracted helper (1 branch)
if (TryCleanupStopByDictionaryLookup(order, snapshot, averageFillPrice))
    return true;

// 3. Name-based fallback (1 branch)
var entryName = ExtractEntryNameFromStop(orderName);
if (activePositions.TryGetValue(entryName, out var pos))
{
    CleanupPosition(entryName, pos, averageFillPrice);
    return true;
}

return false;
// CYC = 4
```

#### Acceptance Criteria

- [ ] New method `TryCleanupStopByDictionaryLookup` added as `private` in `src/V12_002.Orders.Callbacks.cs` (same partial class)
- [ ] `HandleSecondaryOrderFilled_Stop` foreach block replaced with `if (TryCleanupStopByDictionaryLookup(...)) return true;`
- [ ] `[AggressiveInlining]` attribute on new helper
- [ ] Mutation-safety `ContainsKey` guard preserved verbatim inside new helper
- [ ] `CYC(HandleSecondaryOrderFilled_Stop)` after extraction ≤ 8 (projected: 4)
- [ ] `CYC(TryCleanupStopByDictionaryLookup)` ≤ 8 (projected: 5)
- [ ] Zero new `lock()` blocks introduced
- [ ] All existing callers (`HandleOrderState_Filled`) untouched
- [ ] `dotnet build` passes with zero errors
- [ ] xUnit tests written for `TryCleanupStopByDictionaryLookup` covering: match-found path, ContainsKey=false path (stale removal), no-match returns-false path

---

## CYC Projection Table

| Method | CYC Before | Action | CYC After | <= 8? |
|---|---|---|---|---|
| `HandleSecondaryOrderFilled` (router) | 4 | No change | 4 | PASS ✓ |
| `HandleSecondaryOrderFilled_Target` | 7 | No change (within threshold) | 7 | PASS ✓ |
| `HandleSecondaryOrderFilled_Stop` | 10 | Extract foreach block → `TryCleanupStopByDictionaryLookup` | 4 | PASS ✓ |
| `HandleSecondaryOrderFilled_TerminalCleanup` | 2 | No change | 2 | PASS ✓ |
| `TryCleanupStopByDictionaryLookup` (new) | — | New helper | 5 | PASS ✓ |

**projected_parent_cyc_after_all: 4** (`HandleSecondaryOrderFilled_Stop`)
**max_cyc_projected: 7** (existing `HandleSecondaryOrderFilled_Target`, no change required)

---

## Sequential Thinking Validation

**Thought 1** — ticket_count = 1. Only `_Stop` (CYC=10) violates the ≤8 threshold. Router (4), _Target (7), _TerminalCleanup (2) are compliant. One distinct concern maps to one extraction: the foreach+mutation-safety block.

**Thought 2** — Helper `TryCleanupStopByDictionaryLookup`: ~20 lines from the foreach block. Projected helper CYC=5 (foreach+TryGetValue+&&+ContainsKey if/else). Parent _Stop projected CYC=4 (prefix guard + call helper + name fallback).

**Thought 3** — All methods ≤8 post-extraction: Router=4 ✓, _Target=7 ✓, _Stop=4 ✓, _TerminalCleanup=2 ✓, new helper=5 ✓. max_cyc_projected=7. DNA PASS confirmed from Phase 3. Ticket is self-contained, no scope creep.

---

## MCP Evidence

| Tool | Result |
|---|---|
| `resolve_repo` | Repo `antigravityos187-sketch/universal-or-strategy` confirmed indexed, 5147 symbols |
| `get_symbol_complexity(HandleSecondaryOrderFilled_Stop)` | Not found by exact name in index (partial class); CYC=10 confirmed via Phase 2 MCP evidence (lines 121-122 of `02-architecture-plan.md`) |
| `get_extraction_candidates(src/V12_002.Orders.Callbacks.cs)` | candidates=[] (min_callers=2 threshold; private helpers are internal-only — consistent with partial class internals) |
| `sequentialthinking` (3 thoughts) | Validated: ticket_count=1, lines_to_move=~20, CYC projections all ≤8 |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Epic ID** | EPIC-W7-020 |
| **Wave** | 7 |
| **Phase** | 4 |
| **Bobcoins Used** | 0.8 |
| **Execution Time** | ~30s |
| **MCP Tools Used** | resolve_repo, get_symbol_complexity, get_extraction_candidates, sequentialthinking (x4) |
| **ticket_count** | 1 |
| **max_cyc_projected** | 7 |
| **projected_parent_cyc_after_all** | 4 |
| **Output** | docs/brain/EPIC-W7-020/04-tickets.md |
