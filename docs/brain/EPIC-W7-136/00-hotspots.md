# EPIC-W7-136 Hotspot Analysis

**Method:** ManageTrailingStops
**CYC (Tool-Reported):** 0 ⚠️ — requires manual review (see note below)
**CYC (Manual Estimate):** ~10
**File:** src/V12_002.Trailing.cs
**Lines:** 39–97

---

## ⚠️ CYC=0 Notice

The `mcp__jcodemunch-mcp` toolchain returned CYC=0 for this method, which is a sentinel value
indicating the tool was unable to score the symbol (tool unavailable or method not indexable at
time of analysis). Per EPIC protocol, the artifact is produced with findings from direct source
inspection. A manual CYC estimate of **~10** was computed from branch counting (see Complexity
Drivers section). This epic is flagged for **manual review** before Phase 1 proceeds.

---

## Overview

`ManageTrailingStops` is the top-level orchestrator for all trailing stop logic in the V12_002
NinjaTrader strategy. It is called on every price-change tick via the actor-queue enqueue pattern
in [`src/V12_002.BarUpdate.cs:327`](src/V12_002.BarUpdate.cs:327):

```csharp
Enqueue(ctx => ctx.ManageTrailingStops());
```

The method is the entry point into a large call graph spanning three source files
(`V12_002.Trailing.cs`, `V12_002.Trailing.StopUpdate.cs`, `V12_002.Trailing.Breakeven.cs`) and
touches the `activePositions` concurrent dictionary, the SIMA fleet-sync subsystem, and the shadow
engine. Despite the orchestrator itself being relatively shallow (~10 CYC), it is the **hot-path
root** for the most latency-sensitive code in the strategy.

---

## Blast Radius Summary

| Dimension | Detail |
|---|---|
| **Direct caller** | `V12_002.BarUpdate.cs:327` — enqueued on every tick when `activePositions.Count > 0` |
| **Call frequency** | Every price tick with open positions (hot path) |
| **Delegates to (same file)** | `ManageTrail_AdaptiveThrottleTick`, `ManageTrail_RunPerTradeBranches`, `ManageTrail_RunPointBasedTrailing`, `ManageTrail_RunFleetSymmetrySync` |
| **Delegates to (Trailing.StopUpdate.cs)** | `CleanupStalePendingReplacements`, `UpdateStopOrder`, `CalculateStopForLevel` |
| **Delegates to (SIMA.Shadow.cs)** | `ShadowEngineCheck` |
| **Shared state mutated** | `activePositions` (read), `pos.TicksSinceEntry++`, `pos.ExtremePriceSinceEntry` (write), `adaptiveThrottleMs` (write) |
| **Threading constraint** | Strategy thread only; snapshot via `.ToArray()` guards concurrent modification |
| **Risk on change** | **High** — any ordering change between fleet sync, shadow engine, and per-trade branches may alter stop behaviour on live positions |
| **Side-effects** | `UpdateStopOrder` triggers broker cancel+resubmit FSM; `ShadowEngineCheck` auto-propagates bracket changes |

**Affected symbol count (blast radius):** 12+ direct callees across 3 files; 2 shared concurrent state dictionaries (`activePositions`, `pendingStopReplacements`).

---

## Top 3 Complexity Drivers

### 1. Per-trade multi-condition guard chain inside `foreach` (nested if-cascade)

Lines 54–78 contain a sequence of four guard `if`-statements followed by a `ternary` expression and
two more `if` branches — all inside a `foreach` loop over `positionSnapshot`. Each iteration may
exit early at any of four independent conditions (`ContainsKey`, `EntryFilled||BracketSubmitted`,
`IsFollower&&SymmetryGuardIsAnchorPending`, `RunPerTradeBranches`). The combination of loop
nesting + early-exit chain is the primary structural complexity source.  
**Estimated contribution:** ~5 CYC points.

### 2. Direction-dependent ternary inside hot loop (`ExtremePriceSinceEntry` update)

Lines 66–69 contain a `Math.Max`/`Math.Min` ternary conditioned on `pos.Direction`. Although
syntactically a single expression, it is a branch for CYC purposes and sits at the innermost level
of the `foreach` loop. In a high-frequency tick context, any mishandling of this branch (e.g.,
swapping Long/Short logic during refactor) would silently corrupt stop management for all open
positions.  
**Estimated contribution:** +1 CYC, but disproportionate refactor risk.

### 3. Post-loop SIMA fleet-sync conditional (`if (EnableSIMA)`)

Lines 89–93 add a second execution phase after the main position loop. The `EnableSIMA` flag gate
followed by a fresh `activePositions.ToArray()` snapshot creates a second independent branching
surface. Although only +1 CYC by itself, the delegation to `ManageTrail_RunFleetSymmetrySync`
(which itself spans `FleetSync_FindLeaderMaxLevels` + `FleetSync_SyncFollowersToLevel`) means this
one `if` branches into a significant sub-graph. The `ShadowEngineCheck()` call that follows is
**unconditional** but order-dependent on fleet sync having completed first.  
**Estimated contribution:** +1 CYC orchestrator-level, ~6 CYC in delegated sub-graph.

---

## Recommended Extraction Count

**0 additional extractions recommended for the orchestrator itself.**

`ManageTrailingStops` has already been well-decomposed — the body (lines 39–97) is 58 lines and
delegates all substantive logic to named helper methods. The orchestrator reads cleanly as a
sequenced pipeline:

1. Throttle gate (`ManageTrail_AdaptiveThrottleTick`)
2. Per-position iteration with guard chain
3. Per-trade branch routing (`ManageTrail_RunPerTradeBranches`)
4. Point-based trailing (`ManageTrail_RunPointBasedTrailing`)
5. Fleet sync (`ManageTrail_RunFleetSymmetrySync`) — SIMA-gated
6. Shadow engine (`ShadowEngineCheck`)

**If Phase 1 targets this epic, recommended focus areas are:**

| Priority | Target | Rationale |
|---|---|---|
| 1 | `ManageTrail_AdaptiveThrottleTick` | Contains its own nested if/else time-window logic (CYC ~5); candidate for further extraction |
| 2 | `ManageTrail_RunPointBasedTrailing` | Delegates to 4 sub-helpers but still holds cascaded ref-parameter plumbing (CYC ~4) |
| 3 | Inner `foreach` guard chain | 4 early-exit guards could be unified into a single `ShouldProcessPosition(pos)` predicate helper |

**Total additional extractions suggested at Phase 1: 1–3 small helpers.**

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase0-hotspot |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | ~90s |
| **CYC Source** | Manual branch-count (tool returned sentinel 0) |
| **Manual Review Required** | ✅ Yes — confirm CYC with working jcodemunch instance before Phase 1 |
