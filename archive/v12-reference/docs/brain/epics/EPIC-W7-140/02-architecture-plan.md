# Phase 2: Architecture Plan — EPIC-W7-140

**Agent:** v12-phase2-architecture
**Wave:** 7 | **Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:10:00Z
**Input:** docs/brain/EPIC-W7-140/01-scope-boundary.md

---

## Method Under Extraction

| Field | Value |
|---|---|
| **Method** | `InitiateStopReplacement` |
| **Source File** | `src/V12_002.Trailing.StopUpdate.cs` |
| **Lines** | 307–369 |
| **Original CYC** | 10 (manual static count; tool reports 0 — symbol not fully indexed; 00-scope.md notes CYC=53 as full-method complexity; authoritative per-body count from 00-hotspots.md = 10) |
| **Target CYC** | <= 8 |
| **Callers** | 1 — `UpdateStopOrder` (same file, line 128) |

### jcodemunch get_context_bundle / search_symbols result

`search_symbols` found the method at `src/V12_002.Trailing.StopUpdate.cs:307` (get_context_bundle
fell back to search_symbols — symbol present but not complexity-indexed).

```
private void InitiateStopReplacement(
    string entryName,
    PositionInfo pos,
    Order currentStop,
    double validatedStopPrice,
    int newTrailLevel
)
```

Duplicate backup copy also detected at `src-vm-backup/V12_002.Trailing.StopUpdate.cs:284` — backup only, not a second live call site.

### jcodemunch get_call_hierarchy result

- **Callers (depth 1):** `UpdateStopOrder` (same file, AST-resolved)
- **Callee chain (depth 1–2):**
  - `GetTargetOrdersDictionary` (src/V12_002.UI.Callbacks.cs:1039) — called ×5 in for-loop
  - `pendingStopReplacements` constant (src/V12_002.cs:210)
  - `CancelOrderForReplace` (src/V12_002.Orders.CancelGateway.cs:33)
  - `MarkStickyDirty` (src/V12_002.StickyState.cs:619)
  - `LogBuffer.Format` (src/V12_002.Perf.LogBuffer.cs:28)
  - Depth-2: `IsOrderTerminal`, `StampReaperMoveGrace`, `CancelOrderSafe`, `LogBuffer.ValidateThreadAffinity`, `LogBuffer.FormatInternal`

### jcodemunch get_dependency_graph result

No import edges detected for `src/V12_002.Trailing.StopUpdate.cs` — consistent with C# partial-class
architecture where all partial class files share a single compilation unit. No cross-file import
rewrites required. Blast radius is contained to the single partial class.

### jcodemunch get_extraction_candidates result

No candidates returned by tool (symbol complexity not indexed). Extraction plan derived from manual
static analysis in 00-hotspots.md (authoritative CYC=10) combined with code structure review.

---

## Sequential Thinking Summary

**Thought 1 — Problem framing:** `InitiateStopReplacement` has manual CYC=10 (tool-reported 0 due
to indexing gap). Three distinct complexity clusters identified by 00-hotspots.md: (a) for-loop
target-snapshot block ~4 CYC, (b) TryAdd + circuit-breaker nested if ~3 CYC, (c) nested ternary
level-name formatter ~2 CYC. Same source file as EPIC-W7-051; all helper names must be distinct.

**Thought 2 — Helper 1 design:** `TrySnapshotReplacementTargets` extracts the for-loop + 4-clause
compound-guard target-snapshot block (lines 317–336). Projected CYC = 5. "Replacement" noun in
name prevents clash with UpdateStopOrder helpers from EPIC-W7-051.

**Thought 3 — Helper 2 design:** `TryEnqueuePendingReplacement` extracts the TryAdd +
Interlocked.Increment + circuit-breaker activation block (lines 351–360). Projected CYC = 3.
"TryEnqueue" prefix satisfies Jane Street Actor/Enqueue mandate. Returns bool to surface duplicate-key
path that was previously silently swallowed.

**Thought 4 — Helper 3 design:** `FormatTrailLevelName` extracts the nested ternary string
formatter (line 367). Projected CYC = 2. Pure, stateless, side-effect-free. Deduplicates dual-site
pattern also present in `CreateDirectStopOrder` (line 454). Safe from EPIC-W7-051 name conflict.

**Thought 5 — Verification:** After 3 extractions, parent `InitiateStopReplacement` retains only
orchestration logic: call TrySnapshotReplacementTargets, iterate results dispatching CancelOrderForReplace
+ MarkStickyDirty, call TryEnqueuePendingReplacement, call FormatTrailLevelName, update pos state.
Projected parent CYC = 3–5 (conservative upper bound = 5). All helpers ≤ 5 CYC. All Jane Street
rules satisfied.

---

## Extraction Plan

| Helper Method Name | Responsibility | Projected CYC | Strategy Applied |
|---|---|---|---|
| `TrySnapshotReplacementTargets(string entryName, out List<(Order order, Dictionary<int,Order> targets)> snapshot)` | Iterate _tB=1..5, call GetTargetOrdersDictionary, apply 4-clause compound null+state guard, accumulate matched orders | 5 | Extract Loop Body + Extract Guard Clauses |
| `TryEnqueuePendingReplacement(string entryName, PositionInfo pos, Order currentStop, double validatedStopPrice, int newTrailLevel)` | Build PendingReplacement record, ConcurrentDictionary.TryAdd, Interlocked.Increment counter, activate circuit-breaker if threshold exceeded | 3 | Extract Named Helper Methods + FSM Decomposition (circuit-breaker as explicit state transition) |
| `FormatTrailLevelName(int level)` | Resolve int level to display string: <=0 → "Initial", 1 → "BE", N → "T"+(N-1) | 2 | Extract Named Helper Methods + Replace Nested Ternary with Named Function |

---

## Parent Method After Extraction

**Remaining logic:**
1. Entry guard: validate `currentStop.OrderState` (Working or Accepted) — delegated via TrySnapshotReplacementTargets
2. Call `TrySnapshotReplacementTargets(entryName, out var snapshot)` — returns bool + snapshot list
3. Early return if snapshot empty (no targets found)
4. Loop over snapshot entries: call `CancelOrderForReplace`, `MarkStickyDirty`
5. Update `pos.CurrentStopPrice = validatedStopPrice`, `pos.CurrentTrailLevel = newTrailLevel`
6. Call `TryEnqueuePendingReplacement(...)` — handles ConcurrentDictionary + circuit-breaker
7. Call `FormatTrailLevelName(newTrailLevel)` for logging
8. `LogBuffer.Format(...)` diagnostic print

**Projected CYC:** 5 (conservative; actual may be 3 if early-return path and loop are the only branches)

---

## max_cyc_projected: 5
## extraction_count: 3

---

## Jane Street Alignment

| Rule | Status | Detail |
|---|---|---|
| CYC<=8 achieved | YES | All helpers ≤5; parent ≤5 |
| Single-responsibility per helper | YES | Snapshot, Enqueue, Format — each owns exactly one concern |
| Lock-free / Actor pattern preserved | YES | TryEnqueuePendingReplacement uses ConcurrentDictionary.TryAdd + Interlocked.Increment; no lock() blocks introduced |
| Illegal states unrepresentable | YES | TryEnqueuePendingReplacement returns bool surfacing duplicate-key (previously silently swallowed); FormatTrailLevelName eliminates inline ternary ambiguity |
| Zero-allocation hot paths | PRESERVED | String concat in FormatTrailLevelName is unchanged behavior (not introduced by refactor) |

---

## Helper Naming Safety (EPIC-W7-051 Conflict Check)

EPIC-W7-051 targets `UpdateStopOrder` in the same file. All helpers extracted for W7-140 use
"Replacement"-scoped names that are semantically distinct from UpdateStopOrder helpers:
- `TrySnapshotReplacementTargets` — "Replacement" disambiguates from any UpdateStopOrder snapshot helper
- `TryEnqueuePendingReplacement` — "PendingReplacement" is the ConcurrentDictionary key type
- `FormatTrailLevelName` — pure utility; likely shared but not conflicting

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | ~15 |
| **Execution Time** | 2026-06-29T01:10:00Z |
| **Wave** | 7 |
| **Phase** | 2 |
| **jcodemunch tools called** | search_symbols (fallback for get_context_bundle), get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **Input: 01-scope-boundary.md** | boundary_verdict: PASS |
| **Input: 00-hotspots.md** | CYC=10 (manual static), 3 extractions recommended |
| **Output** | docs/brain/EPIC-W7-140/02-architecture-plan.md |
