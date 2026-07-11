# EPIC-W7-002 — Phase 2: Architecture Plan

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:00:00Z
**Input:** docs/brain/EPIC-W7-002/01-scope-boundary.md

---

## Target Method

| Field | Value |
|---|---|
| **Method** | `SymmetryGuardTryResolveFollowersForDispatch` |
| **File** | `src/V12_002.Symmetry.Replace.cs` |
| **Lines** | 134–191 |
| **Original CYC** | **16** |
| **CYC Target** | **<= 8** |
| **Signature** | `private void SymmetryGuardTryResolveFollowersForDispatch(string dispatchId, DateTime nowUtc)` |

---

## Complexity Driver Analysis

The method body contains three structurally distinct logical blocks, each contributing to the total CYC 16:

| Block | Lines | Description | Branch Count | Contribution |
|---|---|---|---|---|
| **Guard** | 136–137 | Early-exit on null/empty dispatchId | 1 | +1 |
| **Block A** | 141–160 | Build worklist from ctx snapshot (ADR-019 lock-free) | 7 | +7 |
| **Block B** | 162–174 | Legacy fallback scan deduplication | 4 | +4 |
| **Block C** | 176–190 | Resolve each follower entry (inner loop body) | 4 | +4 |
| **Base** | — | Base complexity | — | +1 |
| **Total** | — | — | — | **17 paths = CYC 16** |

Branch detail:
- B1: `if (string.IsNullOrEmpty(dispatchId))` → +1
- B2: `symmetryDispatchById.TryGetValue(...) && ctx != null` → +2 (compound)
- B3: `foreach (followerSnapshot)` → +1
- B4: `if (IsNullOrEmpty(fleetEntryName))` → +1
- B5: `if (!TryGetValue(fleetEntryName, linkedDispatch))` → +1
- B6: `if (!string.Equals(linkedDispatch, dispatchId, Ordinal))` → +1
- B7: `if (!ContainsKey(fleetEntryName))` → +1
- B8: `foreach (symmetryPendingFollowerFills.ToArray())` → +1
- B9: `if (!TryGetValue(fleetEntryName, linkedDispatch))` → +1
- B10: `if (!string.Equals(linkedDispatch, dispatchId, Ordinal))` → +1
- B11: `if (followersToResolve.Contains(fleetEntryName))` → +1
- B12: `foreach (followersToResolve)` → +1
- B13: `if (!TryGetValue(fleetEntryName, out pending))` → +1
- B14: `if (pos != null && pos.IsFollower)` → +2 (compound)
- B15: `if (SymmetryGuardTryResolveFollower(...))` → +1

---

## Extraction Plan

### Helper 1: `SymmetryGuardBuildFollowerWorklist_FromSnapshot`

**Signature:**
```csharp
[MethodImpl(MethodImplOptions.NoInlining)]
private void SymmetryGuardBuildFollowerWorklist_FromSnapshot(
    string dispatchId,
    List<string> worklist)
```

**Responsibility:** Extract all valid follower names from the ADR-019 lock-free ctx snapshot into `worklist`. Guards against null/empty names, verifies linked dispatch matches, and checks pending fill presence.

**Extracted branches:** B2, B3, B4, B5, B6, B7

**Projected CYC:** **7** ✅ (base 1 + compound B2(2) + B3(1) + B4(1) + B5(1) + B6(1) + B7(1) = 8 paths → CYC 7)

**Jane Street alignment:** NoInlining (cold construction path, not on hot tick loop). Preserves zero-alloc ADR-019 Interlocked snapshot access pattern.

---

### Helper 2: `SymmetryGuardBuildFollowerWorklist_FromLegacyScan`

**Signature:**
```csharp
[MethodImpl(MethodImplOptions.NoInlining)]
private void SymmetryGuardBuildFollowerWorklist_FromLegacyScan(
    string dispatchId,
    List<string> worklist)
```

**Responsibility:** Scan `symmetryPendingFollowerFills` for any followers linked to `dispatchId` that were missed by the snapshot (ADR-019 legacy-scan fallback). Deduplicates against existing worklist entries.

**Extracted branches:** B8, B9, B10, B11

**Projected CYC:** **5** ✅ (base 1 + B8(1) + B9(1) + B10(1) + B11(1) = 5)

**Jane Street alignment:** NoInlining (cold deduplication scan, involves `.ToArray()` alloc that pre-existed in original). Single responsibility: legacy catchup scan only.

---

### Helper 3: `SymmetryGuardResolveFollowerEntry`

**Signature:**
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private void SymmetryGuardResolveFollowerEntry(
    string fleetEntryName,
    DateTime nowUtc)
```

**Responsibility:** For a single follower entry: look up the pending fill, look up the active position, verify it is a follower position, call `SymmetryGuardTryResolveFollower`, and remove from pending on success.

**Extracted branches:** B13, B14 (compound), B15

**Projected CYC:** **5** ✅ (base 1 + B13(1) + B14(2 compound) + B15(1) = 5)

**Jane Street alignment:** AggressiveInlining (this is the inner per-follower hot path — small body, called once per follower per dispatch resolution event). Preserves V12.Phase8 [F-04] guard: `activePositions.TryGetValue` followed by null+IsFollower check.

---

### Parent Method After Extraction

**Signature (unchanged):**
```csharp
private void SymmetryGuardTryResolveFollowersForDispatch(string dispatchId, DateTime nowUtc)
```

**Remaining logic:**
```
1. Guard: if (IsNullOrEmpty(dispatchId)) return;          → +1
2. Build worklist = new List<string>();
3. if (ctx != null): call Helper1 (snapshot path)         → +1 (ctx null check)
4. call Helper2 (legacy scan always)                       → 0 branches
5. foreach follower in worklist: call Helper3              → +1 (loop)
```

**Projected CYC:** **4** ✅ (base 1 + B1_guard(1) + ctx_check(1) + foreach_loop(1) = 4)

---

## Summary Table

| Artifact | Role | Projected CYC | Jane Street Annotation |
|---|---|---|---|
| `SymmetryGuardTryResolveFollowersForDispatch` (parent) | Orchestrator | **4** | — |
| `SymmetryGuardBuildFollowerWorklist_FromSnapshot` | Helper 1 | **7** | `[NoInlining]` |
| `SymmetryGuardBuildFollowerWorklist_FromLegacyScan` | Helper 2 | **5** | `[NoInlining]` |
| `SymmetryGuardResolveFollowerEntry` | Helper 3 | **5** | `[AggressiveInlining]` |

**Extraction count:** 3
**max_cyc_projected:** **7** ✅ (all <= 8)
**CYC reduction:** 16 → 4 (parent); helpers individually <= 7

---

## Design Rule Validation

| Rule | Status |
|---|---|
| Each extracted helper CYC <= 8 | PASS (7, 5, 5) |
| Parent method CYC <= 8 after extraction | PASS (4) |
| max_cyc_projected <= 8 | PASS (7) |
| No new allocations on hot path | PASS |
| Caller signature unchanged | PASS |
| All helpers private to same partial class | PASS |
| V12.23 No Scope Creep | PASS |

---

## Jane Street Alignment Notes

### Carl Cook — Zero-Alloc Hot Path / AggressiveInlining
- `SymmetryGuardResolveFollowerEntry` (Helper 3) is tagged `[AggressiveInlining]`: it is the innermost per-follower loop body, small and simple (CYC 5), ideal for inlining to eliminate call overhead on the resolution hot path.
- Helper 1 and Helper 2 are `[NoInlining]`: they run once per dispatch event (cold path), contain more branches, and inlining them would bloat the parent JIT code size without latency benefit.
- The `new List<string>()` worklist is a single alloc per dispatch call — pre-sizing to `new List<string>(4)` is a recommended micro-optimization (not blocking for Phase 5 but noted).
- No new allocations introduced in Helper 3 (the hot-path extraction).

### Gjengset — Lock-Free / False Sharing / MemoryBarrier
- All four concurrent state dictionaries (`symmetryDispatchById`, `symmetryFleetEntryToDispatch`, `symmetryPendingFollowerFills`, `activePositions`) remain accessed as captured `this` fields in the extracted helpers — no copies, no struct boxing, no additional intermediate variables that could cause false sharing.
- ADR-019 lock-free contract (Interlocked.CompareExchange snapshot) is preserved: Helper 1 reads `ctx.Followers` as an immutable snapshot exactly as the original does; this comment is preserved verbatim in Helper 1's body.
- No new `lock()` blocks introduced (V12 lock-ban preserved).

### Trading Billions — Single Responsibility / Defense in Depth
- **Single responsibility per helper:** Helper 1 = snapshot-based collection only; Helper 2 = legacy fallback deduplication only; Helper 3 = single-follower resolution only. Zero overlap.
- **Defense in depth preserved:** All guard layers remain intact and are relocated to the appropriate helper:
  - Helper 1 guards: null/empty name, dispatch linkage match, pending-fill presence
  - Helper 2 guards: dispatch linkage match, deduplication
  - Helper 3 guards: pending-fill TryGet, pos null + IsFollower check
- **Circuit breaker note:** If `SymmetryGuardTryResolveFollower` (the callee in Helper 3) ever requires a rate-limit circuit breaker for external latency, the clean single-responsibility boundary of Helper 3 makes that insertion trivial — only one method to modify.

---

## MCP Evidence

### jcodemunch `resolve_repo`
- Repo: `antigravityos187-sketch/universal-or-strategy`
- Symbol count: 5147 | File count: 2000 | Status: loadable

### jcodemunch `search_symbols` (full detail)
- Symbol ID confirmed: `src/V12_002.Symmetry.Replace.cs::V12_002.SymmetryGuardTryResolveFollowersForDispatch#method`
- File: `src/V12_002.Symmetry.Replace.cs:134`
- Kind: method | Signature: `private void SymmetryGuardTryResolveFollowersForDispatch(string dispatchId, DateTime nowUtc)`

### jcodemunch `get_context_bundle`
- Full source retrieved: lines 134–191 (58 lines)
- Imports confirmed: System, ConcurrentDictionary, List, Linq, NinjaTrader namespaces
- ADR-019 lock-free comment confirmed in source

### jcodemunch `get_call_hierarchy`
- Callers: 0 direct callers found in index (per scope boundary: 1 caller documented in Phase 1)
- Direct callees (depth 1): `symmetryDispatchById`, `symmetryFleetEntryToDispatch`, `symmetryPendingFollowerFills`, `activePositions`, `SymmetryGuardTryResolveFollower`
- Depth 2 callees: `SymmetryGuardSkipFollower`, `LogBuffer.Format`, `SymmetryGuardApplyMasterAnchor`, `SymmetryGuardRetargetExistingFollowerBracket`, `SymmetryGuardSubmitFollowerBracket`

### jcodemunch `get_dependency_graph`
- File `src/V12_002.Symmetry.Replace.cs`: node_count=1, edge_count=0 (partial class — all dependencies resolved at class level via C# partial class merging, not file-level import edges)

### jcodemunch `get_extraction_candidates`
- Result: 0 candidates returned (min_callers=1 threshold; the method has 1 caller but complexity data shows no multi-caller sub-functions pre-indexed — confirms extraction must be manually designed, not auto-detected)

---

## Sequential Thinking Evidence

### Thought 2 — Complexity Drivers
Counted all 15 branch points mapped to CYC 16. Identified three coherent extraction regions (Block A, Block B, Block C) with branch counts 7, 4, and 4 respectively. Base=1, sum=16. Confirmed three-helper decomposition as the natural extraction plan.

### Thought 3 — Extraction Strategy
Designed signatures and projected CYC for each helper:
- Helper1: CYC 7 (base 1 + compound B2(2) + B3+B4+B5+B6+B7 = 7)
- Helper2: CYC 5 (base 1 + B8+B9+B10+B11 = 5)
- Helper3: CYC 5 (base 1 + B13+B14_compound(2)+B15 = 5)
- Parent:  CYC 4 (base 1 + B1+ctx_check+foreach = 4)
All <= 8. max_cyc_projected = 7.

### Thought 4 — Jane Street Alignment
- Carl Cook: AggressiveInlining for Helper3 (hot per-follower body), NoInlining for Helpers 1+2 (cold build paths). No new allocations.
- Gjengset: Lock-free contract (ADR-019) preserved. No new locking. Captured this-fields only.
- Trading Billions: Single responsibility per helper confirmed. Defense-in-depth validation layers preserved within each helper's body.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 3.5 |
| **Execution Time** | batch |
| **Phase** | 2 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-002 |
| **MCP Tools Used** | resolve_repo, search_symbols, get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **Sequential Thinking Thoughts** | 4 (probe + 3 analysis thoughts) |
| **Extraction Count** | 3 |
| **max_cyc_projected** | 7 |
| **Design Rule Validation** | ALL PASS |
