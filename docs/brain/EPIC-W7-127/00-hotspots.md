# EPIC-W7-127 Hotspot Analysis

**Method:** SymmetryGuardOnFollowerFill
**CYC:** 16
**File:** src/V12_002.Symmetry.Follower.cs

---

## Overview

`SymmetryGuardOnFollowerFill` is the entry-gate handler for follower position fill events within
the V12 Symmetry subsystem. It sits at the intersection of the ANCHOR-01/ANCHOR-02 optimisation
path (V12.Phase7.1), the `PendingFollowerFill` queue, and the `FollowerBracketFSM` lifecycle.
Its CYC of 16 is driven by three layered guard chains: a null+flag pre-check on `followerPos`,
a lock-free double-lookup into two `ConcurrentDictionary` maps (`symmetryFleetEntryToDispatch` /
`symmetryDispatchById`) with compound anchor readiness conditions, and a deferred-vs-immediate
bracket submission bifurcation. Every CYC point touches live broker state.

---

## Blast Radius Summary

| Dimension | Detail |
|---|---|
| **Direct caller** | `HandleFleetEntryFill` (line 506, `src/V12_002.UI.Compliance.cs`) |
| **Caller chain** | `AccountEventQueue drain` → `HandleFleetEntryFill` → `SymmetryGuardOnFollowerFill` |
| **Downstream dispatches (same call-frame)** | `SymmetryGuardApplyMasterAnchor`, `SymmetryGuardSubmitFollowerBracket`, `SymmetryGuardTryResolveFollower` |
| **Async drain path** | `SymmetryGuardProcessPendingFollowerFills` (polled each bar update) re-enters `SymmetryGuardTryResolveFollower` for unresolved entries |
| **Shared mutable state written** | `symmetryPendingFollowerFills[fleetEntryName]` (ConcurrentDictionary write), `followerPos.EntryFilled`, `followerPos.RemainingContracts`, `followerPos.BracketSubmitted` |
| **Shared mutable state read** | `symmetryFleetEntryToDispatch`, `symmetryDispatchById`, `_followerBrackets` |
| **FSM side-effect** | Instantiates and registers `FollowerBracketFSM` via `SymmetryGuardSubmitFollowerBracket` → `_followerBrackets[fleetEntryName]` |
| **Broker submission side-effect** | `acct.Submit(ordersToSubmit[])` — live GTC stop + limit orders sent to broker |
| **Files directly coupled** | `src/V12_002.Symmetry.Follower.cs`, `src/V12_002.Symmetry.Replace.cs`, `src/V12_002.Symmetry.cs`, `src/V12_002.Symmetry.BracketFSM.cs`, `src/V12_002.UI.Compliance.cs`, `src/V12_002.cs` |
| **Threading constraint** | Called on strategy thread via actor queue drain; `ConcurrentDictionary` writes are visible to REAPER audit thread — any extraction must preserve lock-free write ordering per ADR-019 |
| **Risk on change** | **High** — wrong-prices-first elimination (ANCHOR-01), cancel+replace skipping (ANCHOR-02), and slippage-buffer enforcement are all inlined here; incorrect extraction order creates transient drift or ghost brackets |

**Affected symbol count (blast radius):** 9 symbols directly coupled; 3 shared concurrent state bags; 1 broker I/O path.

---

## Top 3 Complexity Drivers

### 1. Double-map anchor pre-check with compound boolean readiness guard (ANCHOR-01 path, lines 37–59)

The `!followerPos.BracketSubmitted` block contains a double `TryGetValue` chain across
`symmetryFleetEntryToDispatch` and `symmetryDispatchById` (2 CYC from `&&` short-circuit), followed
by a compound `if (anchorReady && preCheckAnchor > 0)` (2 CYC). This 4-branch tree is nested inside
the outer `!BracketSubmitted` guard (+1 CYC) and the `shouldSubmitImmediately` fork that follows
(+1 CYC for the `else`-branch print path). Total sub-tree: **~6 CYC points** — the single
largest contributor. The fact that both the "anchor already hot" fast-path and the "defer until
anchor resolves" slow-path are expressed inline (rather than as delegated helpers) is the root
cause of this concentration.

### 2. Null + flag compound pre-check with defensive `RemainingContracts` fallback (lines 23–28)

The method opens with `if (followerPos == null || !followerPos.IsFollower) return false` (2 CYC
from `||` short-circuit) immediately followed by `if (followerPos.RemainingContracts <= 0)` (+1 CYC)
with a `Math.Max` mutation. While each individual branch is trivial, this defensive-initialisation
pattern inlined at the top of the guard method means any extracted sub-function also needs to
re-validate `followerPos` state, creating copy-pressure. Sub-total: **~3 CYC points** of
structural overhead that could be collapsed into a single `ValidateAndInitFollower` predicate.

### 3. Dual-outcome `TryResolveFollower` + pending-queue mutation (lines 84–85)

The inline `if (SymmetryGuardTryResolveFollower(...))` test-and-remove idiom (+1 CYC) is deceptively
simple at call-site but delegates into a 7-branch method (`SymmetryGuardTryResolveFollower`,
estimated CYC ≈ 9) that itself drives `SkipFollower`, `ApplyMasterAnchor`, `SubmitFollowerBracket`,
and `RetargetExistingFollowerBracket`. The pending-queue write immediately before (`symmetryPendingFollowerFills[fleetEntryName] = pending`) creates a write-then-conditional-remove
pattern: if `TryResolve` returns true the entry is removed synchronously; if false it remains
for the async drain. This write–decide–maybe-remove pattern adds implicit temporal coupling that
is invisible to static CYC counting but raises change-risk significantly. Sub-total: **~3 CYC
points** visible + high hidden coupling cost.

---

## Recommended Extraction Count

**3 extractions recommended for Phase 1.**

| # | Proposed Helper | Lines in `SymmetryGuardOnFollowerFill` | Expected CYC reduction |
|---|---|---|---|
| 1 | `ValidateAndInitFollowerPos(followerPos)` | 23–28 | −3 CYC (null guard + flag check + RemainingContracts init collapsed to single predicate) |
| 2 | `TryApplyPreCheckAnchor(fleetEntryName, followerPos, out bool shouldSubmit)` | 37–72 (full `!BracketSubmitted` block) | −6 CYC (double-map lookup + compound anchor readiness + submit/defer branch extracted to named helper) |
| 3 | `EnqueueAndTryResolveFollower(fleetEntryName, followerPos, followerFillPrice)` | 75–85 (PendingFollowerFill construct + queue write + conditional remove) | −2 CYC (write–decide–remove pattern given single responsibility) |

**Rationale:** After extraction the residual `SymmetryGuardOnFollowerFill` should have CYC ≤ 5
(1 base + 1 for each of the 3 delegating calls + early-return). `SymmetryGuardTryResolveFollower`
(the deepest downstream, CYC ≈ 9) is a separate Wave 7 EPIC candidate and is **out of scope** for
this ticket — it must not be modified as a side-effect of these extractions.

---

## Agent Tracking

| Field | Value |
|---|---|
| Agent Name | v12-phase0-hotspot |
| Bobcoins Used | 1.0 |
| Execution Time | ~90s |
