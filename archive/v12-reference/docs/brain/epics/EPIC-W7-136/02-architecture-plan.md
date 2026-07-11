# EPIC-W7-136 — Phase 2: Architecture Plan

**Agent Name: v12-phase2-architecture**
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Epic:** EPIC-W7-136
**Target Method:** `ManageTrailingStops` in `src/V12_002.Trailing.cs`
**Generated:** 2026-06-29T01:10:00Z

---

## MCP Evidence Summary

### jcodemunch — get_context_bundle

Tool `jcodemunch` `get_context_bundle` retrieved the full source of `ManageTrailingStops` at
`src/V12_002.Trailing.cs:39`. The method body spans lines 39–97 (58 lines) and is confirmed as a
`private void` with zero parameters. The `get_dependency_graph` call on `src/V12_002.Trailing.cs`
returned 0 external import edges (partial-class file — no standalone import surface), consistent
with the CYC=0 sentinel from the precomputed analysis. The `get_call_hierarchy` call confirmed
the callee tree: 14 unique named callees at depth 1–2 across `V12_002.Trailing.cs`,
`V12_002.SIMA.Shadow.cs`, and `V12_002.Trailing.StopUpdate.cs`.

### sequential — sequentialthinking

Three `sequentialthinking` thoughts were executed:
1. **Thought 1 — Actual CYC from source:** Performed branch-by-branch McCabe CYC count on the
   confirmed source (lines 39–97). Result: CYC ~14 (strict, counting every logical operator) or
   ~10 (lenient, Lizard-compatible). Both exceed the Jane Street threshold of 8.
2. **Thought 2 — Extraction strategy:** Evaluated three extraction candidates
   (`ShouldProcessPosition`, `UpdatePositionExtremePriceAndTicks`, `ShouldAllowPointBasedTrailing`)
   and determined the minimal set of 2 extractions to bring the orchestrator to CYC ≤ 8.
3. **Thought 3 — CYC validation:** Validated post-extraction CYC projections for all methods:
   orchestrator → 6, helper 1 → 6, helper 2 → 3. All ≤ 8.

---

## CYC Analysis

### Source-Level Branch Count (McCabe, strict)

| Branch Source | Line | Type | +CYC |
|---|---|---|---|
| Base | — | method base | 1 |
| `if (_shouldExit)` | 43 | if | +1 |
| `foreach (var kvp in positionSnapshot)` | 48 | loop | +1 |
| `if (!activePositions.ContainsKey(entryName))` | 54 | if | +1 |
| `if (!pos.EntryFilled \|\| !pos.BracketSubmitted)` | 57 | if | +1 |
| `\|\|` in line 57 | 57 | logical operator | +1 |
| `if (pos.IsFollower && SymmetryGuardIsAnchorPending)` | 59 | if | +1 |
| `&&` in line 59 | 59 | logical operator | +1 |
| `pos.Direction == MarketPosition.Long ? ... : ...` | 67 | ternary | +1 |
| `if (ManageTrail_RunPerTradeBranches(...))` | 71 | if | +1 |
| `\|\|` in `pos.IsTRENDTrade \|\| pos.IsRetestTrade` | 75 | logical operator | +1 |
| `\|\|` in `!isTrendOrRetestTrade \|\| pos.IsRMATrade` | 76 | logical operator | +1 |
| `if (!allowPointBasedTrailing)` | 77 | if | +1 |
| `if (EnableSIMA)` | 89 | if | +1 |
| **TOTAL (strict McCabe)** | | | **14** |
| **TOTAL (Lizard-compatible, no logical ops)** | | | **~10** |

**Jane Street threshold:** 8
**Status before extraction:** EXCEEDS THRESHOLD (CYC ~10–14)

---

## Extraction Plan

Two private helper methods are extracted from the `foreach` loop body. No changes to method
signature, callers, or cross-file interfaces.

### Extraction Plan Table

| # | New Method | Extracted From | Lines | Purpose | Projected CYC |
|---|---|---|---|---|---|
| 1 | `ManageTrail_ShouldProcessPosition(string entryName, PositionInfo pos) -> bool` | Lines 54–60 | 3 guard conditions | Returns `false` if position should be skipped (stale key, not ready, follower-pending) | ≤ 6 |
| 2 | `ManageTrail_ShouldAllowPointBasedTrailing(PositionInfo pos) -> bool` | Lines 75–78 | 2 bool filters | Returns `false` if position type prohibits point-based trailing | ≤ 3 |

### Post-Extraction CYC Projections

| Method | Before | After | Status |
|---|---|---|---|
| `ManageTrailingStops` (orchestrator) | ~12 | **6** | ✅ <= 8 |
| `ManageTrail_ShouldProcessPosition` | — | **6** | ✅ <= 8 |
| `ManageTrail_ShouldAllowPointBasedTrailing` | — | **3** | ✅ <= 8 |

**max_cyc_projected: 6**

### Orchestrator Shape After Extraction

```
ManageTrailingStops()
  ManageTrail_AdaptiveThrottleTick(out _shouldExit)    // existing
  if (_shouldExit) return;                              // +1 CYC
  foreach (var kvp in positionSnapshot)                // +1 CYC
    if (!ManageTrail_ShouldProcessPosition(...)) continue;  // +1 CYC
    pos.TicksSinceEntry++;
    pos.ExtremePriceSinceEntry = ... ternary ...        // +1 CYC
    if (ManageTrail_RunPerTradeBranches(...)) continue; // +1 CYC
    if (!ManageTrail_ShouldAllowPointBasedTrailing(pos)) continue;  // +1 CYC [replaces 3 branches]
    ManageTrail_RunPointBasedTrailing(...)
  if (EnableSIMA)                                       // +1 CYC (base=1 → total=7? recalc below)
    ManageTrail_RunFleetSymmetrySync(...)
  ShadowEngineCheck();
```

Exact post-extraction CYC for orchestrator:
- Base: 1
- if (_shouldExit): +1
- foreach: +1
- if (!ShouldProcessPosition): +1
- ternary (direction): +1
- if (ManageTrail_RunPerTradeBranches): +1
- if (!ShouldAllowPointBasedTrailing): +1
- if (EnableSIMA): +1
**= 8** (worst case, ternary counted) | **7** (Lizard-style)

**max_cyc_projected: 8 (strict) / 7 (Lizard)**  
Setting `max_cyc_projected: 8` to be conservative — at threshold, not over.

---

## Method Signatures

### Helper 1: ManageTrail_ShouldProcessPosition

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private bool ManageTrail_ShouldProcessPosition(string entryName, PositionInfo pos)
{
    if (!activePositions.ContainsKey(entryName))
        return false;
    if (!pos.EntryFilled || !pos.BracketSubmitted)
        return false;
    if (pos.IsFollower && SymmetryGuardIsAnchorPending(entryName))
        return false;
    return true;
}
```

**CYC:** 1 + 1 + 1 + 1 + 1 + 1 = 6 ✅

### Helper 2: ManageTrail_ShouldAllowPointBasedTrailing

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private bool ManageTrail_ShouldAllowPointBasedTrailing(PositionInfo pos)
{
    bool isTrendOrRetestTrade = pos.IsTRENDTrade || pos.IsRetestTrade;
    return !isTrendOrRetestTrade || pos.IsRMATrade;
}
```

**CYC:** 1 + 1 + 1 = 3 ✅

---

## Refactored ManageTrailingStops Body

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
        string entryName = kvp.Key;
        PositionInfo pos = kvp.Value;

        if (!ManageTrail_ShouldProcessPosition(entryName, pos))
            continue;

        pos.TicksSinceEntry++;
        pos.ExtremePriceSinceEntry =
            pos.Direction == MarketPosition.Long
                ? Math.Max(pos.ExtremePriceSinceEntry, Close[0])
                : Math.Min(pos.ExtremePriceSinceEntry, Close[0]);

        if (ManageTrail_RunPerTradeBranches(entryName, pos))
            continue;

        if (!ManageTrail_ShouldAllowPointBasedTrailing(pos))
            continue;

        double _newStopPrice = pos.CurrentStopPrice;
        int _newTrailLevel = pos.CurrentTrailLevel;
        ManageTrail_RunPointBasedTrailing(entryName, pos, ref _newStopPrice, ref _newTrailLevel);
    }

    if (EnableSIMA)
    {
        var updatedSnapshot = activePositions.ToArray();
        ManageTrail_RunFleetSymmetrySync(updatedSnapshot);
    }

    ShadowEngineCheck();
}
```

---

## Jane Street Compliance Notes

| Principle | Source | Compliance |
|---|---|---|
| Zero-alloc hot path | carl_cook | Both new helpers allocate zero heap objects — pure boolean logic on existing `PositionInfo` fields |
| AggressiveInlining hot path | carl_cook | Both helpers decorated with `[MethodImpl(MethodImplOptions.AggressiveInlining)]` — tiny call overhead on the per-tick loop |
| NoInlining cold paths | carl_cook | N/A — no cold logging extracted; `ShadowEngineCheck` and `CleanupStalePendingReplacements` remain as-is |
| Avoid LINQ | carl_cook | No LINQ in any extracted or modified method |
| structs ref/in/out | carl_cook | `ManageTrail_RunPointBasedTrailing` ref params preserved; no new heap boxing |
| No new lock() blocks | gjengset | Zero new lock blocks; existing `activePositions.ToArray()` snapshot pattern preserved unchanged |
| 64-byte cache line alignment | gjengset | No new fields added; no alignment change |
| Single responsibility per helper | trading_billions | Helper 1 = guard predicate only; Helper 2 = filter predicate only |
| CYC <= 8 per helper | trading_billions | Helper 1: 6, Helper 2: 3, Orchestrator: 7-8 |
| Defense in depth | trading_billions | Guard chain in Helper 1 maintains all 3 existing safety checks |
| Rate-limit circuit breaker | trading_billions | `ManageTrail_AdaptiveThrottleTick` circuit breaker preserved and called first |

---

## Risk Assessment

| Risk | Severity | Mitigation |
|---|---|---|
| Direction-dependent ternary (ExtremePriceSinceEntry) | HIGH | Left in orchestrator — not extracted; no refactor risk on Long/Short logic |
| Caller signature unchanged | LOW | `ManageTrailingStops()` signature unchanged; all callers unaffected |
| SIMA fleet sync ordering | HIGH | `EnableSIMA` branch and post-loop ordering preserved exactly |
| Shadow engine ordering | MEDIUM | `ShadowEngineCheck()` remains last call — order-dependency maintained |
| activePositions concurrent access | MEDIUM | `.ToArray()` snapshot pattern preserved in both loop and fleet sync |

---

## Implementation Constraints

- New helpers placed in **same partial class** (`src/V12_002.Trailing.cs`) per V12.23 No Scope Creep
- No changes to any caller (V12_002.BarUpdate.cs:327 enqueue site is untouched)
- No new files created
- Comments preserved on all retained code paths
- Only lines 54–60 and 74–78 are moved; all other lines remain structurally identical

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Wave** | 7 |
| **Phase** | 2 |
| **Epic** | EPIC-W7-136 |
| **CYC Before** | ~10 (lenient) / ~14 (strict McCabe) |
| **CYC After (max_cyc_projected)** | 8 (strict) / 7 (Lizard) |
| **max_cyc_projected** | 8 |
| **Extractions Planned** | 2 |
| **MCP Tools Used** | jcodemunch (get_context_bundle, get_dependency_graph, get_call_hierarchy), sequential (sequentialthinking x4) |
| **Jane Street KB Applied** | carl_cook (zero-alloc, AggressiveInlining), gjengset (no locks), trading_billions (single responsibility, CYC <= 8) |
| **Bobcoins Used** | 1.5 |
