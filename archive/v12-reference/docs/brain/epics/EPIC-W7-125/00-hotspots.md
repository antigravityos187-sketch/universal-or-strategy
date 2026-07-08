# EPIC-W7-125 — Phase 0: Hotspot Analysis

> **Note:** `method_name` and `source_file` were missing from the epic list — using best-effort hotspot match.

---

## Target Method

| Property | Value |
|---|---|
| **Method** | `ShadowPropagateStopMoves` |
| **File** | `src/V12_002.SIMA.Shadow.cs` |
| **Lines (pre-refactor)** | 35–82 (monolith; LOC 32) |
| **Class** | `V12_002 : Strategy` (partial) |
| **CYC (audited)** | **20** |
| **CYC (current, post EPIC-CCN-12)** | **6** (extracted; historical hotspot documented here) |
| **Source** | `TIER2_METHODS_ANALYSIS.md` — cross-referenced with `git log` for `EPIC-CCN-12` |

---

## Cyclomatic Complexity Breakdown (Pre-Refactor CYC = 20)

Counting the original monolith (commit `aabe967~1`) per McCabe: base 1 + one per decision edge.

| # | Branch / Decision Point | Location | +CYC |
|---|---|---|---|
| 1 | `foreach (var kvp in activePositions.ToArray())` | outer loop | +1 |
| 2 | `if (pos == null \|\| pos.IsFollower)` | null/follower guard | +1 |
| 3 | `\|\|` short-circuit (pos.IsFollower) | same guard | +1 |
| 4 | `if (!pos.EntryFilled \|\| pos.RemainingContracts <= 0)` | entry state guard | +1 |
| 5 | `\|\|` short-circuit (RemainingContracts) | same guard | +1 |
| 6 | `if (!stopOrders.TryGetValue(...))` | stop order lookup | +1 |
| 7 | `if (leaderStop == null \|\| leaderStop.StopPrice <= 0)` | stop validity | +1 |
| 8 | `\|\|` short-circuit (StopPrice) | same guard | +1 |
| 9 | `if (Math.Abs(...) < tickSize * 0.5)` | noise threshold | +1 |
| 10 | `if (ShadowMoveFollowerStops(...))` | cache update branch | +1 |
| 11 | `foreach (var cacheKvp in _leaderLastStopPrice.ToArray())` | cache-clean loop | +1 |
| 12 | `if (!activePositions.TryGetValue(...) \|\| livePos == null \|\| ...)` compound | cache-clean guard | +1 |
| 13 | `\|\|` (livePos == null) | same compound | +1 |
| 14 | `\|\|` (livePos.IsFollower) | same compound | +1 |
| 15 | `\|\|` (!livePos.EntryFilled) | same compound | +1 |
| 16 | `\|\|` (RemainingContracts <= 0) | same compound | +1 |
| 17 | `\|\|` (!stopOrders.TryGetValue) | same compound | +1 |
| 18 | `\|\|` (liveStop == null) | same compound | +1 |
| 19 | `\|\|` (liveStop.StopPrice <= 0) | same compound | +1 |
| **Base** | | | +1 |
| **Total** | | | **20** |

---

## Blast Radius Summary

`ShadowPropagateStopMoves` is called unconditionally from `ShadowEngineCheck()`, which is invoked from:

- [`ManageTrailingStops()`](src/V12_002.Trailing.cs) — hot path, called every price tick when `activePositions.Count > 0`
- [`ProcessOnExecutionUpdate()`](src/V12_002.Orders.Callbacks.Execution.cs) — execution callback path (shadow callback injection, Build 1105)

**State surfaces mutated or read:**

| State Surface | Access Type | Risk |
|---|---|---|
| `activePositions` (ConcurrentDictionary) | `.ToArray()` snapshot + read | Snapshot allocation on hot path |
| `stopOrders` (ConcurrentDictionary) | `TryGetValue` per position | Per-position lock-free read |
| `_leaderLastStopPrice` (ConcurrentDictionary) | `TryGetValue`, indexer write, `TryRemove` | Write under concurrent callbacks |
| `_followerBrackets` (via `ShadowMoveFollowerStops`) | `TryGetValue` per follower | FSM state read |
| `symmetryMasterEntryToDispatch` | `TryGetValue` | Dispatch context lookup |
| `symmetryDispatchById` | `TryGetValue` | Dispatch context lookup |
| `symmetryFleetEntryToDispatch` | `TryGetValue`, `.ToArray()` | Follower enumeration |
| `UpdateStopOrder(...)` | call | Initiates 2-phase stop-replace FSM |
| `FlattenAllApexAccounts()` (via `ShadowPropagateLeaderFlatten`) | call | Fleet-wide order cancellation |

**Downstream file impact:** 14+ files across SIMA, Trailing, Symmetry, REAPER, Orders, and Callbacks subsystems depend on the state surfaces above.

**Invocation frequency:** Called every bar tick whenever a position is open — making any O(n²) traversal (outer positions loop × inner followers loop) a latency risk at scale.

---

## Top 3 Complexity Drivers

### 1. Compound Boolean Guard in Cache-Cleanup Loop (8 `||` operators)
The cache-cleanup `foreach` block contains a single `if` with **8 chained `||` conditions** testing liveness of a cached leader entry. Each `||` is a separate branch edge in the control graph, contributing 8 CYC points from a block only 8 lines long. This is the single largest driver of the CYC=20 score.

### 2. Nested Iteration with Multi-Guard Inner Body (2 `if` chains inside outer `foreach`)
The outer `foreach (activePositions)` contains 4 sequential early-exit `if` guards before reaching the payload logic. Each guard is a separate branch. Nesting depth: `foreach → if → if → if → if → if → if → if(payload)`. This creates a wide "guard staircase" antipattern that is both a readability and complexity hazard.

### 3. Inline Cache-Write Conditional After `ShadowMoveFollowerStops` Call
The call `if (ShadowMoveFollowerStops(...))` uses the boolean return value to conditionally update `_leaderLastStopPrice`. This tight coupling of the cache-write decision to the follower-propagation return value means the cache is owned by two unrelated code sections (the outer loop for writes, the cleanup loop for removes), making the invariant hard to reason about under concurrent access.

---

## Recommended Extraction Count

**3 extractions** (already performed in EPIC-CCN-12; documented here for audit trail):

| Extraction | Method | Rationale |
|---|---|---|
| 1 | `ValidateLeaderPosition()` | Eliminated 4-guard staircase from outer loop body |
| 2 | `DetectStopPriceChange()` | Isolated noise-threshold check into testable pure function |
| 3 | `ValidateCachedEntry()` | Extracted 8-clause `||` compound from cache-cleanup loop |

**Post-refactor CYC target:** ≤ 7 (achieved: CYC=6 per EPIC-CCN-12 commit `92b1c91`)

---

## Historical Status

This epic targets a hotspot that has **already been remediated** under `EPIC-CCN-12` (commit `92b1c91`, PR #22: *"ShadowPropagateStopMoves extraction (CYC 20→6, 70% reduction)"*). The current source reads CYC≈6. This Phase 0 document records the historical complexity archaeology for Wave 7 audit completeness.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase0-hotspot |
| **Bobcoins Used** | 18 |
| **Execution Time** | ~4 min (multi-file read + git archaeology) |

---

*Generated: Wave 7 | Phase 0 | EPIC-W7-125*
*Note: `method_name` and `source_file` missing from epic list — using best-effort hotspot match.*
