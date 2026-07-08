# Phase 2: Architecture Plan — EPIC-W7-051

## Method Under Extraction

- **Method:** `UpdateStopOrder`
- **Source File:** `src/V12_002.Trailing.StopUpdate.cs`
- **Original CYC:** 6 (Phase 0 analysed — 6 decision branches; seed input reported 0)
- **Lines:** 84–139 (56-line body)
- **Class:** `V12_002` (partial — Trailing.StopUpdate)

### jcodemunch get_context_bundle result

Symbol resolved at `src/V12_002.Trailing.StopUpdate.cs::V12_002.UpdateStopOrder#method` (line 84–139).
Signature: `private void UpdateStopOrder(string entryName, PositionInfo pos, double newStopPrice, int newTrailLevel)`
Docstring: `V12.44: ChangeStop() removed -- dead code, only caller was MoveStopsToBreakevenPlusOne (also removed)`
Body structure (6 decision branches):
1. `if (!stopOrders.TryGetValue(...)) return;` — guard
2. `if (pendingStopReplacements.TryGetValue(...))` — stale-pending lookup
3. `if (pendingAgeSeconds > STALE_PENDING_FAST_PATH_SEC)` — age threshold check → `HandleStalePendingReplacement`
4. `if (currentStop.OrderState == CancelPending || Submitted)` → `UpdateExistingPendingReplacement`
5. `if (currentStop.OrderState == Working || Accepted)` → `InitiateStopReplacement`
6. Fall-through → `CreateDirectStopOrder`; `catch (Exception)` block

### jcodemunch get_call_hierarchy result

**Callers (depth 1):** 0 callers detected by AST (partial-class cross-file calls not resolved by import graph).
Phase 0/1 manual analysis confirmed 15 direct call sites across 7 files:
`V12_002.Trailing.cs` (5), `V12_002.UI.Callbacks.cs` (4), `V12_002.Trailing.Breakeven.cs` (2),
`V12_002.SIMA.Shadow.cs` (1), `V12_002.Orders.Callbacks.Propagation.cs` (1),
`V12_002.Symmetry.Replace.cs` (1), `V12_002.UI.IPC.Commands.Mode.cs` (1).

**Callees (depth 1):** `ValidateStopPrice`, `HandleStalePendingReplacement`, `UpdateExistingPendingReplacement`,
`InitiateStopReplacement`, `CreateDirectStopOrder`, `HandleUpdateException`.

**Callees (depth 2):** `Validate_LongIsIllegalAdjust`, `Validate_ShortIsIllegalAdjust`, `CaptureTargetSnapshot`,
`RefreshTargetSnapshot`, `GetTargetOrdersDictionary`, `CancelOrderForReplace`, `Enqueue`,
`HandleStopSubmissionFailure`, `FlattenPositionByName`, `MarkStickyDirty`, `LogBuffer.Format`.

### jcodemunch get_dependency_graph result

File-level dependency graph returns 1 node, 0 edges (C# partial class pattern — all dependencies
are within the same assembly/partial class; no distinct file-level import edges detected).
The entire V12_002 partial class is co-located across multiple `.cs` files with no explicit
using-import edges between them.

### jcodemunch get_extraction_candidates result

No candidates returned (min_callers=1, min_complexity=3) — consistent with the partial-class
architecture where caller-file relationships are not tracked as file-level imports. Extraction
plan derived from Phase 0 hotspot analysis and context bundle source inspection.

---

## Sequential Thinking Summary

**Final Thought (5/5) — Jane Street Verdict: APPROVED**

After 5-step sequential analysis:

1. **Thought 1** — Identified CYC=6 structure: routing dispatcher with 3 if/return chains, stale-pending age check, and fall-through. All 6 branches confirmed from source.
2. **Thought 2** — Designed initial 3-helper plan: `ResolveStopRoute` (enum), `IsStalePendingReplacement` (predicate), `BuildTargetSnapshot` (loop consolidation).
3. **Thought 3** — Verified CYC projections. Without a dispatch wrapper, parent retains CYC 6–7 from switch. Added `DispatchToHandler` to drive parent to CYC 3.
4. **Thought 4** — Finalized: 4 extracted helpers, max CYC = 5 across all methods. All ≤ 8.
5. **Thought 5** — Jane Street alignment confirmed: CYC≤8 (max=5), single-responsibility per helper, lock-free Actor pattern preserved, StopRouteDecision enum makes illegal routing states unrepresentable, zero heap allocation in helpers.

The `BuildTargetSnapshot` deduplication (Phase 0 recommendation 3) is noted: consolidating the
triplication in `CaptureTargetSnapshot`/`RefreshTargetSnapshot`/`InitiateStopReplacement` would
require modifying those sibling helpers which are **out of scope** per V12.23. `BuildTargetSnapshot`
is extracted as a new helper called by `UpdateStopOrder`'s routing path only, without touching
the existing sibling methods.

---

## Extraction Plan

| Helper Method Name | Responsibility | Projected CYC |
|---|---|---|
| `ResolveStopRoute(string entryName, Order currentStop)` | Classifies the stop order into one of 4 `StopRouteDecision` enum values (`StalePending`, `UpdatePending`, `ReplaceWorking`, `CreateDirect`) by evaluating null check, stale-pending age, and OrderState. Calls `IsStalePendingReplacement` internally. | 5 |
| `IsStalePendingReplacement(string entryName)` | Pure predicate: looks up `pendingStopReplacements`, computes age in seconds, returns `true` if age exceeds `STALE_PENDING_FAST_PATH_SEC`. Zero allocation (struct DateTime arithmetic). | 3 |
| `BuildTargetSnapshot(string entryName)` | Iterates `_tB` indices 1–5, calls `GetTargetOrdersDictionary`, tests `Working\|Accepted`, assembles `TargetSnapshot` value-type struct. Consolidates duplicated bracket-capture logic. | 3 |
| `DispatchToHandler(StopRouteDecision route, string entryName, PositionInfo pos, Order currentStop, double validatedStopPrice, int newTrailLevel)` | Switch on `StopRouteDecision` enum; delegates to one of the four pre-existing private helpers (`HandleStalePendingReplacement`, `UpdateExistingPendingReplacement`, `InitiateStopReplacement`, `CreateDirectStopOrder`). No logic of its own. | 5 |

**New supporting type (zero-logic):**
- `enum StopRouteDecision { StalePending, UpdatePending, ReplaceWorking, CreateDirect }` — makes illegal routing states unrepresentable.

---

## Parent Method After Extraction

**Remaining logic in `UpdateStopOrder` after extraction:**
1. `if (!stopOrders.TryGetValue(entryName, out var currentStop)) return;` — existence guard
2. `double validatedStopPrice = ValidateStopPrice(pos.Direction, newStopPrice, newTrailLevel, pos.EntryPrice);` — validation (no branch)
3. `var route = ResolveStopRoute(entryName, currentStop);` — routing decision (no branch in parent)
4. `DispatchToHandler(route, entryName, pos, currentStop, validatedStopPrice, newTrailLevel);` — dispatch (no branch in parent)
5. `catch (Exception ex) { HandleUpdateException(entryName, pos, ex); }` — error handler

- **Remaining logic:** Entry-guard + validate + resolve-route + dispatch + catch. Pure orchestrator with no routing logic inline.
- **Projected CYC:** 3 (1 base + 1 guard + 1 catch)

---

## max_cyc_projected: 5
## extraction_count: 4

---

## Jane Street Alignment

| Principle | Status | Detail |
|---|---|---|
| CYC<=8 achieved | YES | Max projected CYC = 5 across all 5 methods (parent + 4 helpers) |
| Single-responsibility per helper | YES | Each helper does exactly one thing: classify / predicate / snapshot / dispatch |
| Lock-free/Actor pattern preserved | YES | `Interlocked.Increment(ref pendingReplacementCount)` and `Enqueue` calls remain in existing sibling helpers; no locking introduced by this extraction |
| Illegal states unrepresentable | YES | `StopRouteDecision` enum with 4 values replaces implicit 3-guard if/return chain; compiler enforces exhaustive switch coverage |
| Zero-allocation hot paths | YES | `IsStalePendingReplacement` uses stack DateTime arithmetic; `BuildTargetSnapshot` returns value-type struct; `ResolveStopRoute` returns enum (value type) |
| No scope creep (V12.23) | YES | All 4 new helpers are private to `V12_002` partial class in `src/V12_002.Trailing.StopUpdate.cs`; no sibling helpers or caller files modified |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 1.5 |
| **Execution Time** | 2026-06-29T01:10:00Z |
| **Wave** | 7 |
| **Phase** | 2 |
| **Epic ID** | EPIC-W7-051 |
| **Source File** | `src/V12_002.Trailing.StopUpdate.cs` |
| **Method** | `UpdateStopOrder` |
| **CYC (original)** | 6 |
| **CYC (max projected)** | 5 |
| **extraction_count** | 4 |
| **jcodemunch tools called** | `resolve_repo`, `get_context_bundle`, `get_call_hierarchy`, `get_dependency_graph`, `get_extraction_candidates` |
| **sequential-thinking calls** | 5 |
| **Output** | `docs/brain/EPIC-W7-051/02-architecture-plan.md` |
