# EPIC-W7-002 — Phase 4: Ticket Definitions

**Agent:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T01:20:00Z
**Input:** docs/brain/EPIC-W7-002/02-architecture-plan.md + docs/brain/EPIC-W7-002/03-audit-report.md

---

## Target Method

| Field | Value |
|---|---|
| **Method** | `SymmetryGuardTryResolveFollowersForDispatch` |
| **File** | `src/V12_002.Symmetry.Replace.cs` |
| **Lines** | 134–191 |
| **Original CYC** | **16** (jcodemunch confirmed: cyclomatic=16, assessment="high", lines=58, max_nesting=4, param_count=2) |
| **CYC Target** | **<= 8** |
| **DNA Verdict** | PASS (Phase 3) |

---

## ticket_count: 3

---

## Ticket 1

| Field | Value |
|---|---|
| **ticket_id** | T1 |
| **helper_name** | `SymmetryGuardBuildFollowerWorklist_FromSnapshot` |
| **annotation** | `[MethodImpl(MethodImplOptions.NoInlining)]` |
| **concern** | Extract all valid follower names from the ADR-019 lock-free `Interlocked.CompareExchange` ctx snapshot into the worklist. Guards against null/empty entry names, verifies linked dispatch matches `dispatchId` via `symmetryFleetEntryToDispatch`, and checks `symmetryPendingFollowerFills` contains the entry before adding. |
| **lines_to_move** | Block A, approximately lines 141–160: the `TryGetValue` call on `symmetryDispatchById` to obtain `ctx`, followed by `foreach (followerSnapshot in ctx.Followers)` iteration with guards B4 (IsNullOrEmpty check), B5 (TryGetValue linkage check), B6 (string equality Ordinal check), B7 (ContainsKey pending-fill check), and add-to-worklist action. |
| **branches_extracted** | B2 compound (symmetryDispatchById.TryGetValue && ctx != null) +2, B3 foreach +1, B4 IsNullOrEmpty +1, B5 TryGetValue linkage +1, B6 string.Equals Ordinal +1, B7 ContainsKey +1 |
| **cyc_reduction** | -7 (7 paths removed from parent) |
| **projected_helper_cyc** | **7** ✅ (base 1 + 6 independent branches = 7) |

### Signature

```csharp
[MethodImpl(MethodImplOptions.NoInlining)]
private void SymmetryGuardBuildFollowerWorklist_FromSnapshot(
    string dispatchId,
    List<string> worklist)
```

### Jane Street Notes
- `[NoInlining]`: Cold construction path — called once per dispatch event, not on the per-tick hot loop. Inlining would bloat parent JIT code size.
- ADR-019 lock-free contract preserved: reads `ctx.Followers` as an immutable Interlocked snapshot (no new `lock()` blocks).
- No new allocations introduced (worklist passed by reference; the List itself is already allocated in parent).

---

## Ticket 2

| Field | Value |
|---|---|
| **ticket_id** | T2 |
| **helper_name** | `SymmetryGuardBuildFollowerWorklist_FromLegacyScan` |
| **annotation** | `[MethodImpl(MethodImplOptions.NoInlining)]` |
| **concern** | Scan `symmetryPendingFollowerFills` for any followers linked to `dispatchId` that were missed by the ADR-019 snapshot path (legacy fallback). Deduplicates against existing worklist entries via `Contains` check before adding. |
| **lines_to_move** | Block B, approximately lines 162–174: `foreach (symmetryPendingFollowerFills.ToArray())` iteration with B9 (TryGetValue linkage check), B10 (string.Equals Ordinal check for dispatchId match), B11 (Contains deduplication check), and add-to-worklist action. |
| **branches_extracted** | B8 foreach ToArray +1, B9 TryGetValue linkage +1, B10 string.Equals Ordinal +1, B11 Contains dedup +1 |
| **cyc_reduction** | -4 (4 paths removed from parent) |
| **projected_helper_cyc** | **5** ✅ (base 1 + 4 independent branches = 5) |

### Signature

```csharp
[MethodImpl(MethodImplOptions.NoInlining)]
private void SymmetryGuardBuildFollowerWorklist_FromLegacyScan(
    string dispatchId,
    List<string> worklist)
```

### Jane Street Notes
- `[NoInlining]`: Cold deduplication scan — involves pre-existing `.ToArray()` alloc; must stay off the hot path.
- Single responsibility: legacy catchup scan only — does not touch snapshot dict or position state.
- `.ToArray()` alloc pre-existed in original code; no new allocations introduced by extraction.

---

## Ticket 3

| Field | Value |
|---|---|
| **ticket_id** | T3 |
| **helper_name** | `SymmetryGuardResolveFollowerEntry` |
| **annotation** | `[MethodImpl(MethodImplOptions.AggressiveInlining)]` |
| **concern** | Resolve a single follower entry: look up its pending fill in `symmetryPendingFollowerFills`, look up the active position in `activePositions` and verify it is a follower position (`pos != null && pos.IsFollower`), call `SymmetryGuardTryResolveFollower`, and remove from pending fills on success. |
| **lines_to_move** | Block C (inner loop body), approximately lines 176–190: `TryGetValue` on `symmetryPendingFollowerFills` for `fleetEntryName` (B13), `TryGetValue` on `activePositions` with compound null+IsFollower guard (B14), call to `SymmetryGuardTryResolveFollower` conditioned on its bool return (B15), and `Remove` on success. The `foreach (followersToResolve)` loop header (B12) stays in the parent as the iteration boundary. |
| **branches_extracted** | B13 TryGetValue pending +1, B14 compound (pos != null && pos.IsFollower) +2, B15 SymmetryGuardTryResolveFollower result check +1 |
| **cyc_reduction** | -4 (4 paths removed from parent; B12 foreach stays in parent as the dispatch loop) |
| **projected_helper_cyc** | **5** ✅ (base 1 + B13(1) + B14_compound(2) + B15(1) = 5) |

### Signature

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private void SymmetryGuardResolveFollowerEntry(
    string fleetEntryName,
    DateTime nowUtc)
```

### Jane Street Notes
- `[AggressiveInlining]`: Hot per-follower path — called once per follower per dispatch resolution event. Small body (CYC 5), ideal for inlining to eliminate call overhead.
- V12.Phase8 [F-04] guard preserved: `activePositions.TryGetValue` followed by `pos != null && pos.IsFollower` compound check.
- No new allocations on hot path. Only `this`-field access — no struct boxing, no additional intermediate variables.
- Circuit-breaker note: If `SymmetryGuardTryResolveFollower` ever requires rate-limiting, Helper 3's clean single-responsibility boundary makes that insertion trivial.

---

## Parent Method After All Extractions

| Field | Value |
|---|---|
| **Method** | `SymmetryGuardTryResolveFollowersForDispatch` |
| **Signature** | `private void SymmetryGuardTryResolveFollowersForDispatch(string dispatchId, DateTime nowUtc)` (unchanged) |
| **Remaining logic** | B1 guard `if (IsNullOrEmpty(dispatchId)) return` (+1), allocate `worklist = new List<string>()`, ctx null-check before calling T1 (+1), call T2 unconditionally (0 branches), `foreach (follower in worklist)` → call T3 (+1) |
| **projected_parent_cyc_after_all** | **4** ✅ (base 1 + B1_guard(1) + ctx_check(1) + B12_foreach(1) = 4) |

---

## CYC Verification Matrix

| Artifact | Role | Projected CYC | <= 8? |
|---|---|---|---|
| `SymmetryGuardTryResolveFollowersForDispatch` (parent) | Orchestrator | **4** | ✅ |
| `SymmetryGuardBuildFollowerWorklist_FromSnapshot` (T1) | Helper 1 | **7** | ✅ |
| `SymmetryGuardBuildFollowerWorklist_FromLegacyScan` (T2) | Helper 2 | **5** | ✅ |
| `SymmetryGuardResolveFollowerEntry` (T3) | Helper 3 | **5** | ✅ |
| **max_cyc_projected** | — | **7** | ✅ |

**Original CYC:** 16 → **max projected:** 7 (55% reduction in worst-case complexity)

---

## Branch Distribution Audit

| Branch | Description | Destination |
|---|---|---|
| B1 | `if (IsNullOrEmpty(dispatchId))` | Parent |
| B2 | `symmetryDispatchById.TryGetValue && ctx != null` (compound) | T1 |
| B3 | `foreach (followerSnapshot)` | T1 |
| B4 | `if (IsNullOrEmpty(fleetEntryName))` | T1 |
| B5 | `if (!TryGetValue(fleetEntryName, linkedDispatch))` | T1 |
| B6 | `if (!string.Equals(linkedDispatch, dispatchId, Ordinal))` | T1 |
| B7 | `if (!ContainsKey(fleetEntryName))` | T1 |
| B8 | `foreach (symmetryPendingFollowerFills.ToArray())` | T2 |
| B9 | `if (!TryGetValue(fleetEntryName, linkedDispatch))` | T2 |
| B10 | `if (!string.Equals(linkedDispatch, dispatchId, Ordinal))` | T2 |
| B11 | `if (followersToResolve.Contains(fleetEntryName))` | T2 |
| B12 | `foreach (followersToResolve)` | Parent (loop boundary) |
| B13 | `if (!TryGetValue(fleetEntryName, out pending))` | T3 |
| B14 | `if (pos != null && pos.IsFollower)` (compound) | T3 |
| B15 | `if (SymmetryGuardTryResolveFollower(...))` | T3 |

All 15 branches accounted for, zero lost or duplicated. ✅

---

## Design Rule Validation

| Rule | Status |
|---|---|
| ticket_count >= 1 | PASS (3) |
| One ticket = one extracted helper = one concern | PASS |
| Each helper CYC <= 8 | PASS (7, 5, 5) |
| projected_parent_cyc_after_all <= 8 | PASS (4) |
| max_cyc_projected <= 8 | PASS (7) |
| All branches accounted for (no duplicates, no losses) | PASS (15/15) |
| No new lock() blocks | PASS |
| ASCII-only identifiers | PASS |
| V12.23 No Scope Creep | PASS |
| DNA Phase 3 verdict | PASS |

---

## MCP Evidence

### jcodemunch `get_symbol_complexity`
```json
{
  "symbol_id": "src/V12_002.Symmetry.Replace.cs::V12_002.SymmetryGuardTryResolveFollowersForDispatch#method",
  "name": "SymmetryGuardTryResolveFollowersForDispatch",
  "kind": "method",
  "file": "src/V12_002.Symmetry.Replace.cs",
  "line": 134,
  "cyclomatic": 16,
  "max_nesting": 4,
  "param_count": 2,
  "lines": 58,
  "assessment": "high"
}
```

### jcodemunch `get_extraction_candidates`
```json
{
  "file": "src/V12_002.Symmetry.Replace.cs",
  "candidates": [],
  "min_complexity": 5,
  "min_callers": 1
}
```
Zero auto-detected candidates — extraction designed manually per Phase 2 architecture plan (expected: index cannot auto-detect sub-function extraction candidates within a single method body).

---

## Sequential Thinking Evidence

### Thought 1 — Ticket Count Decision
Identified 3 structurally distinct concern blocks (Block A: snapshot build, Block B: legacy fallback dedup, Block C: per-entry resolution). One-ticket-per-concern principle yields **ticket_count = 3**. Guard (B1) and orchestration loop (B12) remain in parent as inherent glue logic.

### Thought 2 — Per-Ticket Detail
Mapped each ticket: lines to move, branches extracted, CYC reduction, projected helper CYC, and annotation rationale. Verified `[AggressiveInlining]` for T3 (hot per-follower inner body) and `[NoInlining]` for T1+T2 (cold build paths). All projected helper CYC values <= 8.

### Thought 3 — Verification
Branch math verified: B2_compound(2)+B3(1)+B4(1)+B5(1)+B6(1)+B7(1) = 7 for T1; B8(1)+B9(1)+B10(1)+B11(1) = 4+base 1 = 5 for T2; B13(1)+B14_compound(2)+B15(1) = 4+base 1 = 5 for T3; B1(1)+ctx_check(1)+B12(1)+base 1 = 4 for parent. All 15 original branches distributed, none lost or duplicated. max_cyc_projected = 7. All <= 8. Ticket definitions finalized.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Bobcoins Used** | 3.0 |
| **Execution Time** | batch |
| **Phase** | 4 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-002 |
| **MCP Tools Used** | resolve_repo, sequentialthinking (probe), get_symbol_complexity, get_extraction_candidates, sequentialthinking (3 analysis thoughts) |
| **Sequential Thinking Thoughts** | 4 (1 probe + 3 analysis) |
| **ticket_count** | 3 |
| **max_cyc_projected** | 7 |
| **projected_parent_cyc_after_all** | 4 |
| **Design Rule Validation** | ALL PASS |
