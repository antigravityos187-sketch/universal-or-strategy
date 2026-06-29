# EPIC-W7-125 — Phase 2: Architecture Plan

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:15:00Z
**Input:** docs/brain/EPIC-W7-125/01-scope-boundary.md

---

## Method Identity (Confirmed)

| Property | Value |
|---|---|
| **Method** | `ShadowPropagateStopMoves` |
| **File** | `src/V12_002.SIMA.Shadow.cs` |
| **Line** | 34 |
| **Class** | `V12_002` (partial) |
| **CYC Baseline (historical)** | 20 |
| **CYC Current (MCP-confirmed)** | 4 |
| **Status** | Partially refactored — EPIC-CCN-12 extracted 4 helpers; one helper (ValidateCachedEntry) still at CYC=9 |

> **Note:** `method_name` and `source_file` were blank in precomputed.json. Identity confirmed via:
> `01-scope-boundary.md` (explicit method name), `00-hotspots.md` (file path + git archaeology),
> and `mcp__jcodemunch-mcp__search_symbols` + `get_context_bundle` MCP verification.

---

## Current Method Body (Source-Verified)

```csharp
private void ShadowPropagateStopMoves()
{
    foreach (var kvp in activePositions.ToArray())
    {
        Order leaderStop;
        if (!ValidateLeaderPosition(kvp.Value, kvp.Key, stopOrders, out leaderStop))
        {
            continue;
        }

        double lastKnown;
        if (!DetectStopPriceChange(kvp.Key, leaderStop.StopPrice, _leaderLastStopPrice, tickSize, out lastKnown))
        {
            continue;
        }

        PropagateAndCacheStopPrice(kvp.Key, leaderStop.StopPrice, _leaderLastStopPrice);
    }

    foreach (var cacheKvp in _leaderLastStopPrice.ToArray())
    {
        if (!ValidateCachedEntry(cacheKvp.Key, activePositions, stopOrders))
        {
            _leaderLastStopPrice.TryRemove(cacheKvp.Key, out _);
        }
    }
}
```

---

## CYC Audit — All Methods in Scope

| Method | Line | CYC (Measured) | Assessment | Action |
|---|---|---|---|---|
| `ShadowPropagateStopMoves` | 34 | **4** | Low | None — already ≤8 |
| `ValidateLeaderPosition` | 73 | **8** | Medium | None — at boundary, acceptable |
| `DetectStopPriceChange` | 113 | **2** | Low | None |
| `PropagateAndCacheStopPrice` | 138 | **2** | Low | None |
| `ValidateCachedEntry` | 158 | **9** ⚠️ | Medium | **EXTRACT** — 1 point over threshold |

---

## Extraction Plan

### Gap Analysis
`ValidateCachedEntry` (CYC=9) is the only method in scope exceeding the V12 ≤8 threshold.
Its body contains a single `if` with **8 chained `||` conditions** in two logical groups:
- **Group A (5 conditions):** Position liveness — TryGetValue, null, IsFollower, EntryFilled, RemainingContracts
- **Group B (3 conditions):** Stop order validity — TryGetValue, null, StopPrice > 0

### Extraction: `ValidateCachedPosition`

**New Method Signature:**
```csharp
private static bool ValidateCachedPosition(
    string entryKey,
    ConcurrentDictionary<string, PositionInfo> activePositions,
    out PositionInfo livePos
)
```

**Responsibility:** Validates the active-position side of a cache key — confirms the key maps to a non-null, non-follower, entry-filled leader with remaining contracts.

**Body (projected):**
```csharp
private static bool ValidateCachedPosition(
    string entryKey,
    ConcurrentDictionary<string, PositionInfo> activePositions,
    out PositionInfo livePos
)
{
    return activePositions.TryGetValue(entryKey, out livePos)
        && livePos != null
        && !livePos.IsFollower
        && livePos.EntryFilled
        && livePos.RemainingContracts > 0;
}
```

**Projected CYC:** 1 (base) + 4 (&&/|| short-circuit clauses) = **5** ✓

---

### Refactored `ValidateCachedEntry` (post-extraction)

```csharp
private static bool ValidateCachedEntry(
    string entryKey,
    ConcurrentDictionary<string, PositionInfo> activePositions,
    ConcurrentDictionary<string, Order> stopOrders
)
{
    PositionInfo livePos;
    Order liveStop;

    if (!ValidateCachedPosition(entryKey, activePositions, out livePos))
    {
        return false;
    }
    if (!stopOrders.TryGetValue(entryKey, out liveStop)
        || liveStop == null
        || liveStop.StopPrice <= 0)
    {
        return false;
    }

    return true;
}
```

**Projected CYC:** 1 (base) + 1 (if #1) + 1 (if #2) + 1 (|| null) + 1 (|| StopPrice) = **5** ✓ ≤8

---

## Final CYC Summary (Post-Extraction)

| Method | Projected CYC | Status |
|---|---|---|
| `ShadowPropagateStopMoves` | 4 | ✓ ≤8 |
| `ValidateLeaderPosition` | 8 | ✓ ≤8 |
| `DetectStopPriceChange` | 2 | ✓ ≤8 |
| `PropagateAndCacheStopPrice` | 2 | ✓ ≤8 |
| `ValidateCachedEntry` (refactored) | 5 | ✓ ≤8 |
| `ValidateCachedPosition` (new) | 5 | ✓ ≤8 |

**max_cyc_projected: 8** (ValidateLeaderPosition — existing, unchanged)
**extraction_count: 1** (ValidateCachedPosition extracted from ValidateCachedEntry)

---

## Jane Street Alignment

### gjengset — Cache line ping-ponging / Left-Right pattern
- `_leaderLastStopPrice` uses `ConcurrentDictionary` — provides internal lock-free read semantics
- Both loops use `.ToArray()` snapshot before iteration — prevents mutation-while-iterating without external locks
- Cache cleanup via `TryRemove` is atomic — no false-sharing risk
- **Status:** COMPLIANT

### carl_cook — Hot path zero-alloc / AggressiveInlining
- `activePositions.ToArray()` on hot path — unavoidable for safe snapshot; existing pattern, no regression
- All helper methods are `private static` — no closure allocation, no virtual dispatch
- **Recommendation:** Apply `[MethodImpl(MethodImplOptions.AggressiveInlining)]` to `ValidateCachedPosition` (pure predicate, zero branches projected hot)
- No logging in any helper — cold logging paths are caller-owned
- **Status:** COMPLIANT

### trading_billions — Single responsibility / defense in depth
- `ValidateCachedPosition`: single concern — "is the position side of this cache key still alive?"
- `ValidateCachedEntry` (post-split): single concern — "are BOTH position AND stop still alive?"
- Each helper is independently unit-testable with a mock `ConcurrentDictionary`
- No cross-concern logic in any helper
- **Status:** COMPLIANT

---

## MCP Evidence

| Tool | Finding |
|---|---|
| `resolve_repo` | Repo `antigravityos187-sketch/universal-or-strategy` indexed — 5147 symbols, loadable |
| `search_symbols` | `ShadowPropagateStopMoves` confirmed at `src/V12_002.SIMA.Shadow.cs:34` |
| `get_context_bundle` | Current method body verified — 4 helpers already extracted (EPIC-CCN-12) |
| `get_call_hierarchy` | 1 direct caller (`ShadowEngineCheck`); 4 callees (`ValidateLeaderPosition`, `DetectStopPriceChange`, `PropagateAndCacheStopPrice`, `ValidateCachedEntry`) |
| `get_symbol_complexity` | `ShadowPropagateStopMoves` CYC=4; `ValidateLeaderPosition` CYC=8; `DetectStopPriceChange` CYC=2; `PropagateAndCacheStopPrice` CYC=2; `ValidateCachedEntry` CYC=9 |
| `get_extraction_candidates` | No candidates returned (extraction_candidates empty — because CYC already low after EPIC-CCN-12) |
| `get_symbol_source` | `ValidateCachedEntry` body verified (lines 158-182) — 8 `||` in one compound if |

---

## Sequential Thinking Evidence

| Thought | Conclusion |
|---|---|
| **Thought 1** | Method identity confirmed (`ShadowPropagateStopMoves`, `src/V12_002.SIMA.Shadow.cs:34`). Current CYC=4. Historical CYC=20 (pre-EPIC-CCN-12). 4 helpers already extracted. |
| **Thought 2** | Architecture gap: `ValidateCachedEntry` CYC=9 (1 over threshold). Designed `ValidateCachedPosition` extraction to bring both to CYC≤5. All other methods pass ≤8. |
| **Thought 3** | All projected CYCs validated ≤8. max_cyc_projected=8 (ValidateLeaderPosition, unchanged). Jane Street alignment confirmed on all 3 patterns. |

---

## Historical Context

This epic targets a hotspot that was **substantially remediated** under EPIC-CCN-12 (commit `92b1c91`, PR #22). The Wave 7 Phase 2 plan identifies and closes the remaining gap:

- **EPIC-CCN-12 achieved:** CYC 20 → 4 on parent method (3 extractions)
- **Wave 7 remaining gap:** `ValidateCachedEntry` at CYC=9 (1 over V12 ≤8 standard)
- **Wave 7 action:** Extract `ValidateCachedPosition` → bring `ValidateCachedEntry` from CYC=9 to CYC=5

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 12 |
| **Execution Time** | ~6 min (MCP probe + symbol reads + complexity analysis + sequential thinking) |
| **Phase** | 2 |
| **Wave** | 7 |
| **max_cyc_projected** | 8 |
| **extraction_count** | 1 |
| **MCP Tools Used** | resolve_repo, search_symbols, get_context_bundle, get_call_hierarchy, get_extraction_candidates, get_symbol_complexity (x5), get_symbol_source (x2) |
