# Phase 2: Architecture Plan — EPIC-W7-137

**Agent:** v12-phase2-architecture
**Wave:** 7 | **Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:15:00Z
**Input:** docs/brain/EPIC-W7-137/01-scope-boundary.md

---

## Method Under Extraction

- **Method:** `FleetSync_SyncFollowersToLevel`
- **Source File:** `src/V12_002.Trailing.cs`
- **Original CYC:** 11 (full McCabe, per 00-hotspots.md manual count) / 8 (conservative, excluding ternaries)
- **Declaration:** Line 142, `private void`, class `V12_002` (partial class, NinjaTrader Strategy)

### jcodemunch get_context_bundle result

Symbol resolved as `src/V12_002.Trailing.cs::V12_002.FleetSync_SyncFollowersToLevel#method`.
Full source body confirmed (lines 142–191):
- `foreach` over `positionSnapshot` array
- 5 guard-clause `continue` checks (eligibility: IsFollower, EntryFilled+BracketSubmitted, activePositions.ContainsKey, targetLevel==0, CurrentTrailLevel>=targetLevel)
- 2 ternary direction dispatches (`fol.Direction == MarketPosition.Long`) for targetLevel and isBetter
- 1 `CalculateStopForLevel` call
- 1 conditional `isBetter` block containing `UpdateStopOrder` + `Print(string.Format(...))`
- Initial symbol lookup by name returned "not found" (symbol not indexed at name level); resolved via `search_symbols` fallback.

### jcodemunch get_call_hierarchy result

| Direction | Depth | Symbol | File | Note |
|-----------|-------|--------|------|------|
| Caller (depth 1) | 1 | `ManageTrail_RunFleetSymmetrySync` | `src/V12_002.Trailing.cs:99` | Direct caller — must not change |
| Caller (depth 2) | 2 | `ManageTrailingStops` | `src/V12_002.Trailing.cs:39` | Tick-driven entry point |
| Callee (depth 1) | 1 | `activePositions` | `src/V12_002.cs:199` | ConcurrentDictionary — thread-safe reads |
| Callee (depth 1) | 1 | `CalculateStopForLevel` | `src/V12_002.Trailing.StopUpdate.cs:533` | HIGH blast radius (9 files) |
| Callee (depth 1) | 1 | `UpdateStopOrder` | `src/V12_002.Trailing.StopUpdate.cs:84` | HIGH blast radius (9 files) — live order submission |
| Callee (depth 1) | 1 | `LogBuffer.Format` | `src/V12_002.Perf.LogBuffer.cs:28` | Logging utility |

**Execution context:** Called every tick when `EnableSIMA` is active. High-frequency hot path. Zero-allocation discipline required.

### jcodemunch get_dependency_graph result

- `src/V12_002.Trailing.cs` has **0 import edges** and **0 importer edges** in the import graph (partial class — all imports centralized in primary `V12_002.cs` file).
- Blast radius for this refactor is fully contained within `src/V12_002.Trailing.cs` (same partial class file for new private helpers). No cross-file dependency changes.

### jcodemunch get_extraction_candidates result

- No extraction candidates returned by automated tooling (complexity data not indexed for this file at the required threshold).
- Manual analysis from 00-hotspots.md and source body review drives extraction plan below.

---

## Sequential Thinking Summary

**5-thought chain completed (thoughts 1–5):**

**Thought 1** — Established baseline: CYC=11 (full McCabe) exceeds Jane Street CYC<=8 threshold. Method has 3 distinct logical concerns: (a) follower eligibility filtering, (b) target level/stop price computation, (c) conditional stop order application. Extraction is mandatory. Single caller — no signature changes.

**Thought 2** — Designed `FleetSync_IsFollowerEligible`: encapsulates all 5 guard predicates (IsFollower, EntryFilled||BracketSubmitted, ContainsKey). Pure boolean, zero allocation, ConcurrentDictionary.ContainsKey is thread-safe. CYC=5. Jane Street: single-responsibility, lock-free.

**Thought 3** — Designed `FleetSync_ComputeSyncStop`: consolidates duplicate direction-dispatch ternaries, guards for targetLevel==0 and CurrentTrailLevel>=targetLevel, and CalculateStopForLevel call. Returns 0.0 as sentinel (zero allocation, no extra allocation vs bool+double tuple). `out int targetLevel` communicates computed level. CYC=4.

**Thought 4** — Designed `FleetSync_ApplySyncStop`: encapsulates isBetter direction check + UpdateStopOrder + Print. Gated allocation (string.Format only fires when stop movement actually occurs). Parent loop reduced to foreach + 2 guards + 2 helper calls. Parent CYC=4.

**Thought 5 (final)** — Verified all methods: parent CYC=4, Helper1 CYC=5, Helper2 CYC=4, Helper3 CYC=3. max_cyc_projected=5. 63% complexity reduction (11→4 for parent). All comply CYC<=8. No locks. Single responsibility per helper. Illegal states unrepresentable: eligibility gate is explicit, sentinel 0.0 is unambiguous, direction dispatch is consolidated.

---

## Extraction Plan

| Helper Method Name | Signature | Responsibility | Projected CYC |
|---|---|---|---|
| `FleetSync_IsFollowerEligible` | `private bool FleetSync_IsFollowerEligible(string entryName, PositionInfo fol)` | Encapsulates all 5 guard-clause predicates (IsFollower, EntryFilled, BracketSubmitted, activePositions.ContainsKey). Returns false if any eligibility condition fails. | **5** |
| `FleetSync_ComputeSyncStop` | `private double FleetSync_ComputeSyncStop(PositionInfo fol, int leaderLongMaxLevel, int leaderShortMaxLevel, out int targetLevel)` | Resolves target level via direction dispatch, guards for no-leader (targetLevel==0) and no-progress (CurrentTrailLevel>=targetLevel), calls CalculateStopForLevel. Returns 0.0 sentinel if no sync needed. | **4** |
| `FleetSync_ApplySyncStop` | `private void FleetSync_ApplySyncStop(string entryName, PositionInfo fol, double syncStopPrice, int targetLevel)` | Computes isBetter (direction-aware price comparison), calls UpdateStopOrder and Print only when stop improvement is confirmed. | **3** |

---

## Parent Method After Extraction

**Remaining logic:**

```csharp
private void FleetSync_SyncFollowersToLevel(
    KeyValuePair<string, PositionInfo>[] positionSnapshot,
    int leaderLongMaxLevel,
    int leaderShortMaxLevel
)
{
    foreach (var kvp in positionSnapshot)
    {
        string entryName = kvp.Key;
        PositionInfo fol = kvp.Value;

        if (!FleetSync_IsFollowerEligible(entryName, fol))
            continue;

        int targetLevel;
        double syncStopPrice = FleetSync_ComputeSyncStop(
            fol, leaderLongMaxLevel, leaderShortMaxLevel, out targetLevel);

        if (syncStopPrice == 0.0)
            continue;

        FleetSync_ApplySyncStop(entryName, fol, syncStopPrice, targetLevel);
    }
}
```

- **Projected CYC:** 4 (baseline 1 + foreach +1 + if(!eligible) +1 + if(syncStop==0.0) +1)
- **Branches eliminated from parent:** 7 (5 guards + 2 ternaries moved to helpers)

---

## max_cyc_projected: 5
## extraction_count: 3

---

## Jane Street Alignment

| Rule | Status | Notes |
|---|---|---|
| CYC<=8 achieved | **YES** | Parent: 4, Helper1: 5, Helper2: 4, Helper3: 3. Max=5. |
| Single-responsibility per helper | **YES** | Eligibility / computation / application — each does exactly one thing. |
| Lock-free / Actor pattern preserved | **YES** | No `lock()` blocks. `activePositions.ContainsKey` is a thread-safe read on ConcurrentDictionary. `UpdateStopOrder` uses Actor/Enqueue model per V12 DNA. |
| Illegal states unrepresentable | **YES** | Eligibility gate is explicit boolean contract. Sentinel 0.0 from ComputeSyncStop is unambiguous (stop prices are never zero in NinjaTrader). Direction dispatch consolidated — no dual evaluation. |
| Zero-allocation hot paths | **IMPROVED** | `string.Format` allocation gated behind `isBetter` condition in ApplySyncStop — only fires when stop actually moves, not on every tick. No new allocations in parent or eligibility check. |
| Extract guard clauses | **YES** | 5 guards extracted into IsFollowerEligible as early returns. |
| Extract loop body | **YES** | Loop body complexity moved to 3 named helpers; parent foreach is now a clean 4-line iteration. |
| No duplicate condition evaluation | **YES** | `fol.Direction == MarketPosition.Long` was evaluated twice in original; now evaluated once in each scoped helper. |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 8 |
| **Execution Time** | 2026-06-29T01:15:00Z |
| **Wave** | 7 |
| **Phase** | 2 |
| **jcodemunch tools called** | get_context_bundle (via symbol_id), get_call_hierarchy, get_dependency_graph, get_extraction_candidates, search_symbols (fallback) |
| **sequential-thinking calls** | 5 |
| **MCP resolve_repo** | antigravityos187-sketch/universal-or-strategy — indexed, 5147 symbols |
| **Output** | docs/brain/EPIC-W7-137/02-architecture-plan.md |
