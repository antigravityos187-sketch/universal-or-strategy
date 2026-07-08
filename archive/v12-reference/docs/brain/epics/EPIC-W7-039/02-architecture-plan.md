# Phase 2: Architecture Plan — EPIC-W7-039

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:20:00Z
**Input:** docs/brain/EPIC-W7-039/01-scope-boundary.md

---

## Method Under Extraction

- **Method:** `ManageTrailingStops`
- **Source File:** [`src/V12_002.Trailing.cs`](src/V12_002.Trailing.cs:39)
- **Original CYC:** 13
- **Target CYC:** <= 8

### jcodemunch get_context_bundle result

Source body confirmed. `ManageTrailingStops` is a private void method with:
1. `ManageTrail_AdaptiveThrottleTick(out _shouldExit)` — throttle gate
2. `activePositions.ToArray()` snapshot + `foreach` loop with 4 guard conditions, tick/metrics updates, and dual dispatch (`ManageTrail_RunPerTradeBranches` + `ManageTrail_RunPointBasedTrailing`)
3. `if (EnableSIMA)` block invoking `ManageTrail_RunFleetSymmetrySync` via a second fresh snapshot
4. `ShadowEngineCheck()` — unconditional terminal call

### jcodemunch get_call_hierarchy result

- **Callers (depth 1):** 0 direct AST-resolved callers (method is invoked via Actor `Enqueue` lambda in `src/V12_002.BarUpdate.cs:327` — not a direct call site, hence 0 AST callers)
- **Callees (depth 1):** `ManageTrail_AdaptiveThrottleTick` (line 193), `SymmetryGuardIsAnchorPending` (Symmetry.Follower.cs:90), `ManageTrail_RunPerTradeBranches` (line 240), `ManageTrail_RunPointBasedTrailing` (line 398), `ManageTrail_RunFleetSymmetrySync` (line 99), `ShadowEngineCheck` (SIMA.Shadow.cs:18)
- **Callees (depth 2):** `TrailHandler_TREND_E1` (line 257), `TrailHandler_TREND_E2` (line 312), `TrailHandler_RETEST` (line 342), `ManageTrail_CalculateProfitPoints` (line 433), `ManageTrail_EvaluateManualBreakeven` (line 440), `ManageTrail_ShouldCheckPointBasedTrailing` (line 491), `ManageTrail_ApplyPointBasedCascade` (line 511), `ManageTrail_ShouldUpdatePointBasedStop` (line 601), `UpdateStopOrder` (StopUpdate.cs:84), `FleetSync_FindLeaderMaxLevels` (line 119), `FleetSync_SyncFollowersToLevel` (line 142), `ShadowPropagateStopMoves` (SIMA.Shadow.cs:34), `ShadowPropagateLeaderFlatten` (SIMA.Shadow.cs:328)

### jcodemunch get_dependency_graph result

- `src/V12_002.Trailing.cs` shows 0 explicit import edges in the index — the file is part of a C# partial class and shares context via partial class resolution rather than file-level imports. All cross-file symbol references are intra-class (same partial class `V12_002`). No cross-assembly dependencies identified.

### jcodemunch get_extraction_candidates result

- `get_extraction_candidates` returned 0 candidates (min_callers=1, min_complexity=3). This is expected: the tool scores by *external* caller file count; all callers of sub-methods here are within the same partial class, so cross-file callee count is 0. The extraction plan is driven by in-method CYC analysis, not by external caller density.

---

## Sequential Thinking Summary

**5-thought chain conclusion (Thought 5):**

Extraction plan produces 3 new private helper methods:

1. **`ShouldSkipPosition(string entryName, PositionInfo pos) → bool`** — Encapsulates all per-position guard logic (staleness check, fill/bracket readiness, follower/anchor guard). CYC ~5.
2. **`UpdatePositionMetrics(PositionInfo pos) → void`** — Increments tick counter and updates `ExtremePriceSinceEntry` via a single ternary. CYC ~2.
3. **`ExecutePositionTrail(string entryName, PositionInfo pos) → void`** — Dispatches EMA-trail branch (via `ManageTrail_RunPerTradeBranches`) and, if applicable, point-based trail (via `ManageTrail_RunPointBasedTrailing`) after evaluating `allowPointBasedTrailing` guard. CYC ~5.

After extraction, `ManageTrailingStops` becomes a 5-step orchestrator: throttle gate → snapshot loop calling the 3 helpers → SIMA fleet sync → shadow check. Projected CYC ~5.

**Max projected CYC across all units: 5.** All <= 8. All Jane Street constraints satisfied.

---

## Extraction Plan

| Helper Method Name | Responsibility | Projected CYC |
|---|---|---|
| `ShouldSkipPosition(string entryName, PositionInfo pos)` | Guard clause aggregator: returns `true` if position must be skipped this tick (stale key, not-filled/bracketed, or follower with anchor pending) | 5 |
| `UpdatePositionMetrics(PositionInfo pos)` | Pure metrics update: increments `TicksSinceEntry` and updates `ExtremePriceSinceEntry` based on direction | 2 |
| `ExecutePositionTrail(string entryName, PositionInfo pos)` | Trail dispatch: runs EMA-branch via `ManageTrail_RunPerTradeBranches`; computes `allowPointBasedTrailing`; runs point-based trail if allowed | 5 |

### Extracted Helper Signatures

```csharp
// Guard: returns true if this position should be skipped this tick
private bool ShouldSkipPosition(string entryName, PositionInfo pos)
{
    if (!activePositions.ContainsKey(entryName))
        return true;
    if (!pos.EntryFilled || !pos.BracketSubmitted)
        return true;
    if (pos.IsFollower && SymmetryGuardIsAnchorPending(entryName))
        return true;
    return false;
}

// Pure metrics: tick counter + extreme price tracking (no branches except ternary)
private void UpdatePositionMetrics(PositionInfo pos)
{
    pos.TicksSinceEntry++;
    pos.ExtremePriceSinceEntry =
        pos.Direction == MarketPosition.Long
            ? Math.Max(pos.ExtremePriceSinceEntry, Close[0])
            : Math.Min(pos.ExtremePriceSinceEntry, Close[0]);
}

// Trail dispatch: EMA-only OR point-based depending on trade type
private void ExecutePositionTrail(string entryName, PositionInfo pos)
{
    if (ManageTrail_RunPerTradeBranches(entryName, pos))
        return;

    bool isTrendOrRetestTrade = pos.IsTRENDTrade || pos.IsRetestTrade;
    bool allowPointBasedTrailing = !isTrendOrRetestTrade || pos.IsRMATrade;
    if (!allowPointBasedTrailing)
        return;

    double newStopPrice = pos.CurrentStopPrice;
    int newTrailLevel = pos.CurrentTrailLevel;
    ManageTrail_RunPointBasedTrailing(entryName, pos, ref newStopPrice, ref newTrailLevel);
}
```

---

## Parent Method After Extraction

### Residual `ManageTrailingStops` body

```csharp
private void ManageTrailingStops()
{
    bool _shouldExit;
    ManageTrail_AdaptiveThrottleTick(out _shouldExit);
    if (_shouldExit)
        return;

    var positionSnapshot = activePositions.ToArray();
    foreach (var kvp in positionSnapshot)
    {
        if (ShouldSkipPosition(kvp.Key, kvp.Value))
            continue;
        UpdatePositionMetrics(kvp.Value);
        ExecutePositionTrail(kvp.Key, kvp.Value);
    }

    if (EnableSIMA)
    {
        var updatedSnapshot = activePositions.ToArray();
        ManageTrail_RunFleetSymmetrySync(updatedSnapshot);
    }

    ShadowEngineCheck();
}
```

- **Remaining logic:** Throttle gate → snapshot loop (3 helper calls per position) → SIMA fleet sync (conditional) → shadow check (unconditional)
- **Projected CYC:** 5
  - +1 base
  - +1 `if (_shouldExit)`
  - +1 `foreach` loop
  - +1 `if (ShouldSkipPosition)` continue
  - +1 `if (EnableSIMA)`

---

## max_cyc_projected: 5
## extraction_count: 3

---

## Risk Mitigations

| Risk | Severity | Mitigation in This Plan |
|---|---|---|
| Threading — dual `activePositions.ToArray()` snapshots | HIGH | Two separate snapshots preserved: `positionSnapshot` for main loop, `updatedSnapshot` for fleet sync. Extractions do NOT merge them. |
| Stop-order call ordering | HIGH | `ManageTrail_RunFleetSymmetrySync` call remains AFTER the main loop. `ExecutePositionTrail` → `ManageTrail_RunPointBasedTrailing` → `UpdateStopOrder` call chain is unchanged. |
| Blast radius — fill-callback path | MEDIUM | No change to `UpdateStopOrder` call conditions or signature. `ExecutePositionTrail` is a pure loop-body extract; external behavior is identical. |

---

## Jane Street Alignment

| Principle | Status | Notes |
|---|---|---|
| **CYC <= 8 achieved** | YES | Max CYC = 5 (across all units) |
| **Single-responsibility per helper** | YES | `ShouldSkipPosition` = guard only; `UpdatePositionMetrics` = metrics only; `ExecutePositionTrail` = dispatch only |
| **Lock-free / Actor pattern preserved** | YES | Called via `Enqueue` in BarUpdate.cs:327; no `lock()` blocks introduced |
| **Illegal states unrepresentable** | YES | `ShouldSkipPosition` acts as mandatory gate; `ExecutePositionTrail` can assume position is valid/filled/active |
| **Zero-allocation hot paths** | YES | All helpers use stack-only operations; no heap allocations added |
| **Extract Guard Clauses** | YES | `ShouldSkipPosition` aggregates all 3 guards into early-returns |
| **Extract Loop Body** | YES | Per-position logic fully extracted (ProcessSingleItem pattern) |
| **Thread-safety preserved** | YES | Two independent snapshots maintained; Race 1 (dual-snapshot) not widened |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Epic** | EPIC-W7-039 |
| **Wave** | 7 |
| **Phase** | 2 |
| **Bobcoins Used** | 8 |
| **Execution Time** | 2026-06-29T01:20:00Z |
| **jcodemunch tools called** | `resolve_repo`, `get_context_bundle`, `get_call_hierarchy`, `get_dependency_graph`, `get_extraction_candidates` |
| **sequential-thinking calls** | 5 |
| **Status** | Completed |
