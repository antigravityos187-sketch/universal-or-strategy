# Phase 2: Architecture Plan -- EPIC-W7-057

## Method Under Extraction

- **Method:** `SymmetryGuardTryResolveFollower`
- **Source File:** `src/V12_002.Symmetry.Follower.cs`
- **Original CYC:** 10 (range 10-12 across phase docs; source analysis confirms ~11 decision points)

### jcodemunch get_context_bundle result

Symbol resolved at `src/V12_002.Symmetry.Follower.cs:129`. Method signature confirmed:
```
private bool SymmetryGuardTryResolveFollower(
    string fleetEntryName,
    PositionInfo pos,
    PendingFollowerFill pending,
    DateTime nowUtc
)
```

Source body contains 4 structurally distinct logic sections:
1. Dispatch context lookup (compound TryGetValue guard + AnchorWait timeout fallback calling `SymmetryGuardSkipFollower`)
2. Anchor snapshot resolution check (`ctx.Anchor.IsResolved` + AnchorWait timeout fallback)
3. Slippage calculation + breach check (`slippageTicks > max || slippageUsdPerContract > max`)
4. Anchor application + bracket dispatch decision (`BracketSubmitted` branch -> `alreadyAnchored` sub-branch -> retarget vs submit-fresh)

ADR-019 comment confirms lock-free atomic read of `AnchorSnapshot` via `Interlocked.CompareExchange`.

### jcodemunch get_call_hierarchy result

**Callers (depth 1):**
- `SymmetryGuardOnFollowerFill` -- `src/V12_002.Symmetry.Follower.cs:17` (intra-file)
- `SymmetryGuardProcessPendingFollowerFills` -- `src/V12_002.Symmetry.Follower.cs:97` (intra-file)

**Key callees (depth 1):**
- `SymmetryGuardSkipFollower` (`src/V12_002.Symmetry.Replace.cs:99`)
- `SymmetryGuardApplyMasterAnchor` (`src/V12_002.Symmetry.Follower.cs:248`)
- `SymmetryGuardRetargetExistingFollowerBracket` (`src/V12_002.Symmetry.Replace.cs:17`)
- `SymmetryGuardSubmitFollowerBracket` (`src/V12_002.Symmetry.Follower.cs:285`)
- `symmetryFleetEntryToDispatch` / `symmetryDispatchById` (ConcurrentDictionary reads)

**Caller signature impact:** None. Both callers pass the same 4 parameters. Refactoring does not change method signature.

### jcodemunch get_dependency_graph result

File `src/V12_002.Symmetry.Follower.cs` has 0 cross-file import edges in the index (C# partial class pattern -- dependencies are resolved at compile time via partial class merging, not import statements). All callees resolve within the same partial class boundary. No cross-file blast radius from the import graph.

### jcodemunch get_extraction_candidates result

No candidates returned (min_callers=1, min_complexity=3). The jcodemunch extraction-candidates tool requires callee cross-file usage for caller counting -- intra-class calls within a C# partial class are invisible to this signal. The manual analysis from the context bundle source is the authoritative extraction guide for this epic.

---

## Sequential Thinking Summary

**5-thought chain completed. Final verdict (Thought 5):**

Three guard-clause extractions reduce the parent from CYC ~11 to CYC 7, with each helper between CYC 3-4. The method's 4 logical sections were analyzed: the first 3 are pure guard predicates (fail-fast with SkipFollower or return-false); the 4th is the anchor application + bracket dispatch action block, which retains 6 residual decisions (BracketSubmitted, alreadyAnchored compound check, alreadyAnchored branch) for a parent CYC of 7.

Threading constraint verified: all 3 helpers perform ConcurrentDictionary TryGetValue (lock-free reads) or immutable snapshot reads only. No new enumeration sites on `_followerBrackets`. ADR-019 lock-free contract preserved throughout.

Illegal-states-unrepresentable: `out` parameters on `TryResolveDispatchContext` and `TryResolveAnchorSnapshot` force callers to receive `ctx`/`masterAnchor` only when the guard returns true -- no partial resolution state reachable at compile time.

---

## Extraction Plan

| Helper Method Name | Responsibility | Projected CYC |
|---|---|---|
| `TryResolveDispatchContext` | Looks up `symmetryFleetEntryToDispatch` + `symmetryDispatchById` (compound TryGetValue guard). If lookup fails and `AnchorWait` elapsed, calls `SymmetryGuardSkipFollower("Missing dispatch context")` and returns true. If lookup fails and within timeout, returns false. Returns found ctx via out param on success. | 4 |
| `TryResolveAnchorSnapshot` | Reads `ctx.Anchor` snapshot (immutable, ADR-019). Checks `IsResolved`. If not resolved and `AnchorWait` elapsed, calls `SymmetryGuardSkipFollower("Master anchor timeout")` and returns true. If not resolved within timeout, returns false. Returns `masterAnchor` price via out param. | 3 |
| `IsSlippageWithinTolerance` | Computes `slippagePoints`, `slippageTicks`, `slippageUsdPerContract` from `pending.FleetFillPrice` vs `masterAnchor`. Evaluates breach (`slippageTicks > max \|\| slippageUsdPerContract > max`). On breach calls `SymmetryGuardSkipFollower("Slippage Buffer breach...")` and returns false. Returns true when within tolerance. | 3 |

---

## Parent Method After Extraction

**Remaining logic:**
```
private bool SymmetryGuardTryResolveFollower(
    string fleetEntryName, PositionInfo pos, PendingFollowerFill pending, DateTime nowUtc)
{
    SymmetryDispatchContext ctx;
    if (!TryResolveDispatchContext(fleetEntryName, pos, pending, nowUtc, out ctx))
        return false;                                                        // decision 1

    double masterAnchor;
    if (!TryResolveAnchorSnapshot(fleetEntryName, pos, pending, nowUtc, ctx, out masterAnchor))
        return false;                                                        // decision 2

    if (!IsSlippageWithinTolerance(fleetEntryName, pos, pending, masterAnchor))
        return true;                                                         // decision 3

    double priorEntryPrice = pos.EntryPrice;
    SymmetryGuardApplyMasterAnchor(pos, masterAnchor);

    if (pos.BracketSubmitted)                                               // decision 4
    {
        bool alreadyAnchored = tickSize > 0                                 // decision 5
            && Math.Abs(priorEntryPrice - masterAnchor) < tickSize;
        if (alreadyAnchored)                                                // decision 6
        {
            Print(string.Format("[ANCHOR-02] Bracket already anchor-aligned ..."));
        }
        else
        {
            SymmetryGuardRetargetExistingFollowerBracket(fleetEntryName, pos);
        }
    }
    else
    {
        SymmetryGuardSubmitFollowerBracket(fleetEntryName, pos);
    }

    Print(string.Format("[SYMMETRY_GUARD] ANCHORED | ..."));
    return true;
}
```

- **Remaining logic:** Dispatch-context guard call, anchor-snapshot guard call, slippage guard call, apply master anchor, bracket-submitted branch with already-anchored sub-check, final Print + return.
- **Projected CYC:** 7 (base 1 + 6 decisions: 3 guard if-returns, 1 BracketSubmitted if-else, 1 compound `tickSize > 0 &&`, 1 `alreadyAnchored` if-else)

---

## max_cyc_projected: 7
## extraction_count: 3

---

## xUnit Test Plan

Each extracted helper requires [Fact]-decorated xUnit tests:

| Test Method | Helper Under Test | Scenario |
|---|---|---|
| `TryResolveDispatchContext_MissingEntry_TimeoutElapsed_SkipsAndReturnsTrue` | `TryResolveDispatchContext` | fleetEntry not found, nowUtc past AnchorWait |
| `TryResolveDispatchContext_MissingEntry_WithinTimeout_ReturnsFalse` | `TryResolveDispatchContext` | fleetEntry not found, nowUtc within AnchorWait |
| `TryResolveAnchorSnapshot_NotResolved_TimeoutElapsed_SkipsAndReturnsTrue` | `TryResolveAnchorSnapshot` | snapshot.IsResolved=false, nowUtc past AnchorWait |
| `TryResolveAnchorSnapshot_NotResolved_WithinTimeout_ReturnsFalse` | `TryResolveAnchorSnapshot` | snapshot.IsResolved=false, nowUtc within AnchorWait |
| `IsSlippageWithinTolerance_TicksBreach_SkipsAndReturnsFalse` | `IsSlippageWithinTolerance` | slippageTicks > SymmetryMaxSlippageTicks |
| `IsSlippageWithinTolerance_UsdBreach_SkipsAndReturnsFalse` | `IsSlippageWithinTolerance` | slippageUsdPerContract > SymmetryMaxSlippageUsdPerContract |
| `IsSlippageWithinTolerance_WithinBothLimits_ReturnsTrue` | `IsSlippageWithinTolerance` | both within tolerance |

---

## Jane Street Alignment

- **CYC<=8 achieved:** YES (parent=7, helpers=4/3/3, max=7)
- **Single-responsibility per helper:** YES (each helper encapsulates exactly one guard predicate with its timeout/skip fallback)
- **Lock-free/Actor pattern preserved:** YES (ADR-019 immutable snapshot reads unchanged; no new lock blocks; ConcurrentDictionary TryGetValue is lock-free)
- **Illegal states unrepresentable:** YES (`out` parameters ensure `ctx` and `masterAnchor` are only accessible after the guard returns true; no partial-resolution reachable at callsite)
- **ASCII-only string literals:** REQUIRED during implementation -- all new string literals in helper bodies must use ASCII-only characters (no Unicode, no curly quotes)
- **Extract Guard Clauses pattern:** YES (all 3 helpers are early-return guard patterns per Jane Street complexity-reduction.md)
- **Named helper methods, private scope:** YES (all helpers are `private`, each does exactly one thing)
- **xUnit [Fact] tests per helper:** YES (7 test cases planned above)
- **ONE method per epic:** YES (only `SymmetryGuardTryResolveFollower` is the extraction target; companion methods `SymmetryGuardSubmitFollowerBracket` and `SymmetryGuardOnFollowerFill` are excluded per V12.23)

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Epic** | EPIC-W7-057 |
| **Wave** | 7 |
| **Phase** | 2 -- Architecture Planning |
| **Bobcoins Used** | 2.5 |
| **Execution Time** | 2026-06-29T01:10:00Z |
| **jcodemunch tools called** | resolve_repo, get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **Boundary verdict from Phase 1.5** | PASS |
| **Output artifact** | docs/brain/EPIC-W7-057/02-architecture-plan.md |
