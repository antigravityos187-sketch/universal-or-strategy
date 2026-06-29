# Phase 2: Architecture Plan — EPIC-W7-139

**Agent:** v12-phase2-architecture
**Wave:** 7 | **Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:20:00Z
**Input:** docs/brain/EPIC-W7-139/01-scope-boundary.md

---

## Method Under Extraction

- **Method:** `UpdateStopOrder`
- **Source File:** `src/V12_002.Trailing.StopUpdate.cs`
- **Lines:** 84–139
- **Original CYC:** 8 (manual static count — tool-reported CYC=0 is a partial-class AST resolution artefact)

### jcodemunch get_context_bundle result

`get_context_bundle` returned `error: Symbol(s) not found: UpdateStopOrder` — consistent with the partial-class resolution failure documented in 00-hotspots.md. The scanner cannot resolve `UpdateStopOrder` without a full multi-file compilation pass. Fallback to `search_symbols` confirmed the method at `src/V12_002.Trailing.StopUpdate.cs` line 84 with signature `private void UpdateStopOrder(string entryName, PositionInfo pos, double newStopPrice, int newTrailLevel)`.

### jcodemunch get_call_hierarchy result

`get_call_hierarchy` (resolved via explicit symbol ID `src/V12_002.Trailing.StopUpdate.cs::V12_002.UpdateStopOrder#method`) returned:
- **caller_count:** 0 (strategy-thread dispatch — callers exist in 7 files per blast radius analysis but not resolvable via import graph due to partial-class split)
- **callee_count:** 40 (depth=2 including both src/ and src-vm-backup/ duplicates)
- **Depth-1 callees (src/ only):** `ValidateStopPrice`, `HandleStalePendingReplacement`, `UpdateExistingPendingReplacement`, `InitiateStopReplacement`, `CreateDirectStopOrder`, `HandleUpdateException`, `stopOrders`, `pendingStopReplacements`
- **Depth-2 callees (src/ only):** `CaptureTargetSnapshot`, `RefreshTargetSnapshot`, `CancelOrderForReplace`, `HandleStopSubmissionFailure`, `MarkStickyDirty`, `Enqueue`, `FlattenPositionByName`, `GetTargetOrdersDictionary`, `ValidateStopPrice` sub-validators

Key finding: `Enqueue` appears at depth-2, confirming the Actor/Enqueue lock-free pattern is already in use downstream.

### jcodemunch get_dependency_graph result

`get_dependency_graph` returned 0 import edges and 0 importer edges (`node_count=1`, `edge_count=0`). The partial-class file has no standalone `using` imports visible to the graph walker — all dependencies are resolved at compile time across the partial class split. No cross-file extraction is indicated or safe.

### jcodemunch get_extraction_candidates result

`get_extraction_candidates` returned 0 candidates (`min_complexity=3, min_callers=1`). This is consistent with the partial-class indexing limitation — cyclomatic complexity data is absent from the index for this file. The extraction plan is therefore derived from manual static analysis (00-hotspots.md) and the sequentialthinking chain below.

---

## Sequential Thinking Summary

sequentialthinking chain (5 thoughts, tool: `mcp__sequential-thinking__sequentialthinking`):

1. **Thought 1 — Context assessment:** CYC=8 confirmed from hotspots. `get_context_bundle` failed due to partial-class resolution (consistent with CYC=0 artefact). `get_call_hierarchy` resolved via explicit ID — 40 callees at depth-2 confirming `UpdateStopOrder` is a thin dispatcher; all real complexity in delegates. `get_extraction_candidates` returned 0 (indexer limitation). Extraction plan must be driven by manual static analysis.

2. **Thought 2 — Decision point decomposition:** Mapped all 8 CYC decision points. Identified 2 extractable clusters: the staleness gate (decision points 2+3, CYC contribution +2) → extract to `IsStalePendingReplacement`; the compound OR routing cascade (decision points 4+5, CYC contribution +4) → refactor to `RouteStopOrderByState` using switch expression with explicit default arm.

3. **Thought 3 — Helper signatures:** Designed `IsStalePendingReplacement(string entryName, out Order stalePendingOrder) : bool` (CYC=3) and `RouteStopOrderByState(string entryName, PositionInfo pos, Order currentStop, double validatedStopPrice, int newTrailLevel) : void` (CYC=4).

4. **Thought 4 — Parent CYC projection:** Post-extraction parent CYC = 1(base) + 1(TryGetValue guard) + 1(ValidateStopPrice if) + 1(IsStalePendingReplacement if) + 1(try/catch) = **5**. `RouteStopOrderByState` call contributes 0 branches to parent. Max CYC across all components = 5.

5. **Thought 5 — Jane Street alignment verification:** All components CYC <= 8. Single-responsibility confirmed per helper. Lock-free: `Enqueue` pattern preserved downstream, no `lock()` introduced. Illegal states made representable: switch with explicit `default` arm eliminates implicit fall-through. Zero-allocation: `out` parameter pattern, no boxing. V12.23 scope confined to same partial class file.

---

## Extraction Plan

| Helper Method Name | Responsibility | Signature | Projected CYC |
|---|---|---|---|
| `IsStalePendingReplacement` | Detects whether the pending replacement for `entryName` has exceeded the stale-threshold age. Encapsulates `pendingStopReplacements.TryGetValue` + `DateTime.Now` age arithmetic + `STALE_PENDING_FAST_PATH_SEC` threshold comparison. Returns `true` + the stale order via `out` parameter. | `private bool IsStalePendingReplacement(string entryName, out Order stalePendingOrder)` | 3 |
| `RouteStopOrderByState` | Routes the stop update to the correct execution path based on `currentStop.OrderState`. Replaces the compound-OR `if/else if` cascade with a `switch` expression containing explicit cases for `CancelPending`, `Submitted`, `Working`, `Accepted`, and a named `default` arm that calls `CreateDirectStopOrder`. Makes previously-implicit fall-through explicit and unrepresentable-as-invalid. | `private void RouteStopOrderByState(string entryName, PositionInfo pos, Order currentStop, double validatedStopPrice, int newTrailLevel)` | 4 |

---

## Parent Method After Extraction

**Remaining logic in `UpdateStopOrder` after extraction:**

```
1. Guard: if (!stopOrders.TryGetValue(entryName, out var currentStop)) return;
2. Validate: if (!ValidateStopPrice(entryName, pos, newStopPrice, out var validatedStopPrice)) return;
3. Stale check: if (IsStalePendingReplacement(entryName, out var stalePending)) { HandleStalePendingReplacement(...); return; }
4. Route: RouteStopOrderByState(entryName, pos, currentStop, validatedStopPrice, newTrailLevel);
   [no branch in parent — pure dispatch call]
5. try/catch wrapping all of the above -> HandleUpdateException
```

- **Projected CYC:** 5 (base=1, TryGetValue guard=+1, ValidateStopPrice if=+1, IsStalePendingReplacement if=+1, try/catch=+1)

---

## max_cyc_projected: 5
## extraction_count: 2

---

## Jane Street Alignment

| Principle | Status |
|---|---|
| CYC<=8 achieved (all components) | YES — parent=5, IsStalePendingReplacement=3, RouteStopOrderByState=4 |
| Single-responsibility per helper | YES — staleness detection vs. state routing are orthogonal concerns |
| Lock-free/Actor pattern preserved | YES — `Enqueue` pattern in downstream delegates unchanged; no `lock()` introduced |
| Illegal states unrepresentable | YES — `switch` with explicit `default` arm replaces implicit `OrderState` fall-through |
| Zero-allocation hot paths | YES — `out` parameter reuses stack slot; no new heap allocations |
| No scope creep (V12.23) | YES — all changes confined to `src/V12_002.Trailing.StopUpdate.cs`, private helpers only |
| Caller signature unchanged | YES — `UpdateStopOrder(string entryName, PositionInfo pos, double newStopPrice, int newTrailLevel)` preserved |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 3 |
| **Execution Time** | 2026-06-29T01:20:00Z |
| **Wave** | 7 |
| **Phase** | 2 |
| **jcodemunch tools called** | get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **extraction_count** | 2 |
| **max_cyc_projected** | 5 |
