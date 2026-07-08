# EPIC-W7-020 — Phase 2: Architecture Plan

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29
**Input:** docs/brain/EPIC-W7-020/01-scope-boundary.md

---

## Original Method

| Field | Value |
|---|---|
| **Method** | `HandleSecondaryOrderFilled` |
| **File** | `src/V12_002.Orders.Callbacks.cs` |
| **Line** | 571 |
| **CYC (MCP-confirmed)** | 4 (router); sub-handlers: _Target=7, _Stop=10, _TerminalCleanup=2 |
| **Aggregate CYC** | 23 (baseline task list: 34 = pre-refactor figure) |
| **Max Nesting** | 8 (_Target), 6 (_Stop) |
| **Lines** | 27 (router), 48 (_Target), 56 (_Stop), 12 (_TerminalCleanup) |
| **Params** | 2 (Order order, double averageFillPrice) |
| **CYC Violation** | `HandleSecondaryOrderFilled_Stop` at CYC=10 exceeds threshold ≤8 |

**Note:** The parent router `HandleSecondaryOrderFilled` (CYC=4) and sub-handlers `_Target` (CYC=7) and `_TerminalCleanup` (CYC=2) are within threshold. Only `HandleSecondaryOrderFilled_Stop` (CYC=10) requires extraction.

---

## Extraction Plan

| Helper Name | Responsibility | Lines Moved | Projected CYC |
|---|---|---|---|
| `TryCleanupStopByDictionaryLookup` | Iterate position snapshot, find matching stop order by dictionary lookup (`stopOrders.TryGetValue`), apply mutation-safety guard (`ContainsKey`), execute `CleanupPosition` or remove stale reference | ~20 lines from the `foreach` block in `_Stop` | 5 |

### `HandleSecondaryOrderFilled_Stop` After Extraction

The remaining body of `_Stop` after moving the foreach block:
1. Prefix guard: `!StartsWith(StopOrderPrefix) && !StartsWith(StopOrderPrefixShort)` → `return false` (1 branch)
2. `if (TryCleanupStopByDictionaryLookup(...))` → `return true` (1 branch)
3. Name-based fallback: `ExtractEntryNameFromStop` + `TryGetValue(entryName)` (1 branch)
4. `CleanupPosition(entryName)` + `return true`
5. `return false`

**Projected CYC for `_Stop` after extraction: 4**

---

## Projected CYC Summary

| Method | CYC Before | Action | CYC After |
|---|---|---|---|
| `HandleSecondaryOrderFilled` (router) | 4 | No change | 4 |
| `HandleSecondaryOrderFilled_Target` | 7 | No change (within threshold) | 7 |
| `HandleSecondaryOrderFilled_Stop` | 10 | Extract foreach body | 4 |
| `HandleSecondaryOrderFilled_TerminalCleanup` | 2 | No change | 2 |
| `TryCleanupStopByDictionaryLookup` (new) | — | New helper | 5 |

**Parent after extraction (router):** CYC = 4

**max_cyc_projected: 7** (HandleSecondaryOrderFilled_Target — existing, no extraction needed)

---

## Method Signatures

### New Helper

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

### Modified Method Signature (unchanged — same contract)

```csharp
private bool HandleSecondaryOrderFilled_Stop(
    Order order,
    string orderName,
    double averageFillPrice,
    KeyValuePair<string, PositionInfo>[] snapshot
)
```

---

## Jane Street Alignment Notes

| Principle | Application |
|---|---|
| **carl_cook: zero-alloc hot path** | New helper receives pre-allocated snapshot (no new allocations); mutation-safety guard is a simple `ContainsKey` check — no alloc |
| **carl_cook: AggressiveInlining hot / NoInlining cold** | `TryCleanupStopByDictionaryLookup` marked `[AggressiveInlining]` — it is on the hot path (every stop fill); `Print`/logging remains cold (no inlining directive) |
| **carl_cook: avoid LINQ** | No LINQ used; dictionary lookup + foreach over array snapshot |
| **gjengset: no new lock() blocks** | `TryRemove` and `TryGetValue` on `ConcurrentDictionary` remain lock-free; mutation-safety guard (`ContainsKey` before `CleanupPosition`) preserved in extracted helper |
| **gjengset: mutation-safety preserved** | The critical re-check guard is moved verbatim into `TryCleanupStopByDictionaryLookup` — race condition protection is not broken by extraction |
| **trading_billions: single responsibility** | `TryCleanupStopByDictionaryLookup` = scan-and-cleanup by exact order reference; `_Stop` = prefix routing + result dispatch |
| **trading_billions: CYC <= 8** | All methods reach ≤7 after extraction; max_cyc_projected = 7 |
| **trading_billions: defense in depth** | Mutation-safety guard preserved; fallback name-based path preserved in parent |

---

## MCP Evidence

| Tool | Result |
|---|---|
| `resolve_repo` | Repo `antigravityos187-sketch/universal-or-strategy` confirmed indexed, 5147 symbols |
| `search_symbols("HandleSecondaryOrderFilled")` | Found at `src/V12_002.Orders.Callbacks.cs:571`; sub-handlers at lines 427, 489, 554 |
| `get_symbol_complexity(_Stop)` | CYC=10, max_nesting=6, lines=56, params=5 — **exceeds threshold** |
| `get_symbol_complexity(_Target)` | CYC=7, max_nesting=8, lines=48 — within threshold (no extraction) |
| `get_symbol_complexity(_TerminalCleanup)` | CYC=2, max_nesting=3, lines=12 — compliant |
| `get_symbol_complexity(router)` | CYC=4, max_nesting=2, lines=27 — compliant |
| `get_symbol_source(_Stop)` | Full source retrieved; foreach block + fallback path identified as extraction targets |
| `get_symbol_source(_Target)` | Full source retrieved; T1-T5 loop confirmed within threshold |
| `get_call_hierarchy` | Callers: `HandleOrderState_Filled` (depth=1), `ProcessOnOrderUpdate` (depth=2); Callees: `activePositions`, `_Target`, `_Stop`, `_TerminalCleanup`, and downstream helpers |
| `get_dependency_graph` | No import edges for `V12_002.Orders.Callbacks.cs` (partial class, self-contained) |

---

## Sequential Thinking Evidence

### Thought 1 — Branch Point Enumeration

Analyzed `HandleSecondaryOrderFilled_Stop` (CYC=10) branch points:
1. `!orderName.StartsWith(StopOrderPrefix) && !StartsWith(StopOrderPrefixShort)` → 1 branch (early return)
2. `foreach (var kvp in snapshot)` → loop = 1 branch
3. `stopOrders.TryGetValue(kvp.Key, out var sOrder) && sOrder == order` → 2 conditions = 2 branches
4. `activePositions.ContainsKey(kvp.Key)` → if/else = 2 branches
5. `return true` inside loop body = 1 branch
6. `activePositions.TryGetValue(entryName, out var pos)` → fallback = 1 branch
7. `return true` in fallback = 1 branch

Total = 10 branches → CYC=10 confirmed. The highest-density cluster is the **foreach + mutation-safety guard block** (branches 2-5, density = 5 branches in ~15 lines).

### Thought 2 — Extraction Strategy

Identified single extraction target: the foreach loop body constitutes a distinct, nameable unit — "try to find and clean up a stop order by dictionary lookup with mutation safety." This maps cleanly to `TryCleanupStopByDictionaryLookup`.

After extraction:
- `_Stop` retains: prefix guard (1 branch) + call helper (1 branch) + name fallback (1 branch) = **CYC=4**
- New helper has: foreach (1) + TryGetValue&&sOrder==order (2) + ContainsKey (1) + else = **CYC=5**
- No changes to `_Target` (CYC=7, passes threshold; nesting=8 is quality debt but not a CYC violation)
- No scope creep — only touching `_Stop`

### Thought 3 — Validation

Post-extraction CYC projections validated:
- Router: 4 ✓ | _Target: 7 ✓ | _Stop after: 4 ✓ | _TerminalCleanup: 2 ✓ | New helper: 5 ✓
- **max_cyc_projected = 7** (existing _Target — no extraction needed)
- All ≤ 8 threshold ✓
- Jane Street alignment: zero-alloc, no new locks, single responsibility per helper ✓
- Mutation-safety guard is preserved verbatim in extracted helper ✓
- Scope: exactly 1 new private method, 1 modified method, same file ✓

---

## V12.23 No Scope Creep Compliance

| Check | Status |
|---|---|
| Only `HandleSecondaryOrderFilled_Stop` modified | PASS |
| New helper is private, same partial class | PASS |
| Router signature unchanged | PASS |
| Callers (`HandleOrderState_Filled`) not touched | PASS |
| No cross-file changes | PASS |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Epic ID** | EPIC-W7-020 |
| **Wave** | 7 |
| **Phase** | 2 |
| **Bobcoins Used** | 1.5 |
| **MCP Tools Used** | resolve_repo, search_symbols, get_symbol_complexity (x4), get_symbol_source (x3), get_call_hierarchy, get_dependency_graph, sequentialthinking (x4) |
| **max_cyc_projected** | 7 |
| **Output** | docs/brain/EPIC-W7-020/02-architecture-plan.md |
