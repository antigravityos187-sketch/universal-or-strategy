# EPIC-W7-129 Hotspot Analysis

**Method:** `SymmetryGuardTryResolveFollower` (best-effort match)
**CYC:** 16
**File:** `src/V12_002.Symmetry.Follower.cs` (line 129)

> **Note:** `method_name` and `source_file` missing from epic list — using best-effort hotspot match.
> Candidate identified by positional context in `wave7-epic-list.json`: EPIC-W7-126/127 = `SymmetryGuardSubmitFollowerBracket` / `SymmetryGuardOnFollowerFill` (both Symmetry.Follower.cs), EPIC-W7-128 = `SymmetryGuardReplaceExistingFollowerTarget` (Symmetry.Replace.cs), EPIC-W7-130 = `SymmetryGuardCascadeFollowerCleanup` (Symmetry.Replace.cs). The only remaining Symmetry.Follower.cs method of comparable complexity is `SymmetryGuardTryResolveFollower`. Static analysis scores CYC ~15–16 depending on tool calibration; CodeScene canonical scan records 16.

---

## Blast Radius Summary

| Dimension | Detail |
|---|---|
| **Direct callers** | `SymmetryGuardOnFollowerFill` (line 84), `SymmetryGuardProcessPendingFollowerFills` (line 122) — both in `src/V12_002.Symmetry.Follower.cs` |
| **Cross-file caller** | `SymmetryGuardTryResolveFollowersForDispatch` in `src/V12_002.Symmetry.Replace.cs` (line 187) |
| **Top-level driver** | `SymmetryGuardOnMasterFill` → `SymmetryGuardTryResolveFollowersForDispatch` → this method |
| **Delegates into** | `SymmetryGuardSkipFollower`, `SymmetryGuardApplyMasterAnchor`, `SymmetryGuardSubmitFollowerBracket`, `SymmetryGuardRetargetExistingFollowerBracket` |
| **Shared state read** | `symmetryFleetEntryToDispatch` (ConcurrentDictionary), `symmetryDispatchById` (ConcurrentDictionary), `symmetryPendingFollowerFills` (ConcurrentDictionary) |
| **Shared state written** | `pos.EntryPrice` / bracket prices (via `SymmetryGuardApplyMasterAnchor`), `pos.BracketSubmitted` flag |
| **Threading constraint** | Strategy thread (bar-update / fill callback context); dispatch dictionaries are lock-free ConcurrentDictionary |
| **Risk on change** | High — method sits on the critical path for all fleet follower bracket submissions; incorrect extraction of slippage guard or anchor-timeout branches will silently skip or double-submit brackets |

**Affected symbol count (blast radius):** 7 symbols directly coupled; 3 shared concurrent state bags; 2 callers in same file, 1 cross-file caller.

---

## Top 3 Complexity Drivers

1. **Tri-state dispatch-context lookup with compound OR null-guard (lines 137–156)**
   A three-clause `||` chain (`!TryGetValue dispatchId`, `!TryGetValue ctx`, `ctx == null`) forms a single `if` with three independent failure paths. Each short-circuit adds +1 CYC. Inside this branch a timeout guard (`nowUtc - pending.QueuedUtc >= SymmetryAnchorWait`) adds another +1, and an identical timeout check repeats in the `!isResolved` path (line 166). The two timeout checks are structurally mirrored but semantically distinct (no-context timeout vs unresolved-anchor timeout), contributing **~5 CYC** from this guard cluster alone.

2. **Slippage breach evaluation with dual-threshold ternary initializers (lines 182–198)**
   Two ternary expressions initialise `slippageTicks` and `slippageUsdPerContract` with zero-guard denominators (`tickSize > 0 ?`, `pointValue > 0 ?`), each contributing +1 CYC. The breach predicate (`slippageTicks > max || slippageUsdPerContract > maxUsd`) then adds +2 (one `||`, one `if`). Total contribution: **~5 CYC** from the slippage subsection.

3. **BracketSubmitted fork with nested already-anchored optimisation (lines 209–231)**
   An outer `if (pos.BracketSubmitted)` splits into two routing paths (retarget vs submit-fresh). Inside the retarget path, an `&&`-compound already-anchored check (`tickSize > 0 && Math.Abs(priorEntryPrice - masterAnchor) < tickSize`) adds +2 CYC, followed by `if (alreadyAnchored)` for +1. This three-level conditional tree — outer split, compound guard, inner branch — contributes **~4 CYC** and is the primary extraction candidate for Phase 1.

---

## Recommended Extraction Count

**2 helpers recommended.**

| # | Proposed Helper | Extracted Logic | Target CYC |
|---|---|---|---|
| 1 | `SymmetryGuardResolveDispatchContext` | Lines 136–156: dispatch-dictionary lookup + timeout-skip path. Encapsulates the tri-clause `||` guard and missing-context timeout into a bool-returning helper with an `out SymmetryDispatchContext ctx`. Reduces caller CYC by 5. | ≤4 |
| 2 | `SymmetryGuardEvaluateSlippage` | Lines 181–198: slippage calculation (tick + USD), dual-ternary initializers, breach predicate. Returns a `SlippageBreach` struct or bool. Reduces caller CYC by 5. | ≤4 |

The `BracketSubmitted` fork (driver 3) is thin enough to remain inline after extractions 1 and 2 bring the parent to CYC ≤7. No further split is warranted at Phase 0.

**Post-extraction estimated CYC of `SymmetryGuardTryResolveFollower`:** ≤7  
**Aggregate extracted CYC:** ≤8 (spread across 2 helpers)

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase0-hotspot |
| **Bobcoins Used** | 2.1 |
| **Execution Time** | ~110s |
