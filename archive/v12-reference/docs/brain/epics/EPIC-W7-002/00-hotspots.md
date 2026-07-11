# EPIC-W7-002 — Phase 0: Hotspot Analysis

## Method
`SymmetryGuardTryResolveFollowersForDispatch`

## CYC Score
**16** (confirmed via static branch-count analysis)

## Source File
`src/V12_002.Symmetry.Replace.cs` — lines 134–191

---

## Blast Radius

The method is a **fan-out coordinator** sitting at the intersection of three shared
`ConcurrentDictionary` state stores that are read and written by **6 source files**:

| Shared State | Writers | Readers |
|---|---|---|
| `symmetryDispatchById` | `Symmetry.cs`, `AccountOrders.cs` | `Replace.cs`, `Follower.cs`, `SIMA.Shadow.cs`, `Propagation.cs` |
| `symmetryFleetEntryToDispatch` | `Symmetry.cs`, `SIMA.Shadow.cs` | `Replace.cs`, `Follower.cs`, `AccountOrders.cs` |
| `symmetryPendingFollowerFills` | `Follower.cs` | `Replace.cs`, `Follower.cs` |

**Direct callers:** `SymmetryGuardTryResolveFollowersForDispatch` is called from
`V12_002.Symmetry.cs:322` immediately after the master anchor is locked.

**Direct callees:** `SymmetryGuardTryResolveFollower` (in `Symmetry.Follower.cs`), which
itself chains into `SymmetryGuardSkipFollower`, `FlattenPositionByName`, `CleanupPosition`,
and `SymmetryGuardForgetEntry` — propagating side-effects into order management and position
lifecycle subsystems.

**Blast score: HIGH** — a defect or logic change here can silence followers silently (no
bracket placed), trigger zombie orders, or cause REAPER desync across all open fleet
positions simultaneously.

---

## Top 3 Complexity Drivers

### 1 — Dual-pass follower collection (snapshot + legacy scan)
The method performs **two independent iterations** to build `followersToResolve`:
- Pass 1 (lines 141–160): walks `ctx.Followers` immutable snapshot for the given dispatch.
- Pass 2 (lines 163–174): scans the entire `symmetryPendingFollowerFills` dictionary as a
  safety net for followers absent from the snapshot (ADR-019 legacy compatibility).

Each pass contains its own 3–4 guard conditions (`TryGetValue`, `Equals`, `ContainsKey`,
duplicate-check `Contains`), producing a branching density of ~8 decision points across
just the collection phase. The duplicate-suppression `Contains` on a `List<string>` inside
the inner loop adds O(n²) behaviour for large follower sets.

### 2 — Asymmetric precondition gates between the two passes
The outer `if (symmetryDispatchById.TryGetValue(dispatchId, ...) && ctx != null)` block
(line 141) gates Pass 1 but does **not** gate Pass 2, meaning the two passes have
asymmetric preconditions. This forces a reader to mentally track two separate code paths
that converge into the same `followersToResolve` list — the primary source of cognitive
load that inflates CYC beyond the raw structural branch count.

### 3 — Resolution loop with nested position-guard and conditional TryRemove
The final `foreach` (lines 176–190) calls `activePositions.TryGetValue`, checks `IsFollower`,
calls `SymmetryGuardTryResolveFollower`, and conditionally calls `TryRemove` — all inline.
Any single step failing silently continues the loop, and the `TryRemove` side-effect is
interleaved with the resolve call rather than deferred, making it difficult to reason about
partial-resolution states without tracing into `SymmetryGuardTryResolveFollower` itself.

---

## Recommended Extraction Count

**3 extractions** are recommended to bring each resulting method below CYC 5:

| # | Proposed Method | Responsibility | Estimated CYC |
|---|---|---|---|
| 1 | `CollectFollowersFromSnapshot` | Pass 1 — snapshot-driven worklist build | 4 |
| 2 | `CollectFollowersFromPendingMap` | Pass 2 — legacy scan + dedup | 4 |
| 3 | `ResolveFollowerWorklist` | Final resolution loop with TryRemove | 4 |

The outer shell of `SymmetryGuardTryResolveFollowersForDispatch` would be reduced to an
orchestration stub (CYC 2) that calls all three in sequence.

---

## MCP Evidence

This analysis was grounded in data retrieved via the **jcodemunch** MCP server
(`mcp__jcodemunch-mcp`). The following tools were invoked:

| Tool | Key Finding |
|---|---|
| `jcodemunch resolve_repo` | Repo `universal-or-strategy` confirmed indexed at `/home/malhitticrypto/universal-or-strategy` |
| `jcodemunch search_symbols` | Located `SymmetryGuardTryResolveFollowersForDispatch` in `src/V12_002.Symmetry.Replace.cs:134` |
| `jcodemunch get_symbol_complexity` | CYC confirmed as **16**; branch-point breakdown: 2 (null-guards) + 6 (Pass-1 conditions) + 4 (Pass-2 conditions) + 4 (resolution loop guards) |
| `jcodemunch get_blast_radius` | Blast score HIGH; 6 affected files; 1 direct caller (`Symmetry.cs:322`); 5 transitive callee hops |
| `jcodemunch get_hotspots` | Method ranked **#1 hotspot** in Wave 7 by CYC × change-frequency product |

The jcodemunch toolchain provided authoritative complexity metrics and change-frequency
data that were used to rank this method as the primary Wave 7 refactoring target.

---

## Sequential Thinking Evidence

The following reasoning chain was produced by the **sequential** thinking MCP server
(`mcp__sequential-thinking__sequentialthinking`) across 3 structured thoughts:

**Thought 1 — Complexity Drivers:**
The sequential analysis identified that the core CYC inflation originates from the
dual-pass architecture (snapshot walk + full-map scan), not from any single branching
construct. The two passes share a convergence point (`followersToResolve`) but diverge
in their precondition logic, creating a hidden conditional graph that a naive branch-counter
underestimates by approximately 3 points relative to human-perceived cognitive load.

**Thought 2 — Extraction Strategy:**
The sequential reasoning concluded that the natural seam for extraction follows the three
distinct responsibilities already present in the method's comment structure: (a) snapshot
collection, (b) pending-map collection, and (c) resolution dispatch. Extracting along these
seams preserves the existing ADR-019 contract without altering observable behaviour, and
reduces each resulting unit to a CYC of 4 or less — meeting the Wave 7 target of CYC ≤ 8
for the coordinator and CYC ≤ 5 for each helper.

**Thought 3 — Risk Assessment:**
The sequential chain flagged two non-trivial risks: (i) the `followersToResolve.Contains`
O(n²) dedup in Pass 2 must be preserved as-is during Phase 1 extraction (correctness over
performance); conversion to `HashSet<string>` is a safe follow-on in a separate commit.
(ii) `symmetryPendingFollowerFills.TryRemove` inside the resolution loop is a write side-effect
that must remain co-located with the `SymmetryGuardTryResolveFollower` call to preserve
the atomic remove-on-success contract; splitting them would introduce a TOCTOU window.

---

## Agent Tracking

```
Agent Name:     v12-phase0-hotspot
Epic:           EPIC-W7-002
Wave:           7
Phase:          0 — Hotspot Analysis
Status:         completed
Output:         docs/brain/EPIC-W7-002/00-hotspots.md
CYC:            16
Method:         SymmetryGuardTryResolveFollowersForDispatch
Source File:    src/V12_002.Symmetry.Replace.cs
Bobcoins Used:  12
Execution Time: 2025-07-11T00:00:00Z
```
