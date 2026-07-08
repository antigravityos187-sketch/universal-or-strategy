# EPIC-W7-057 Hotspot Analysis

**Method:** SymmetryGuardTryResolveFollower (lead), SymmetryGuardSubmitFollowerBracket, SymmetryGuardOnFollowerFill
**CYC:** 12 / 12 / 11 (aggregate cluster CYC = 35)
**File:** src/V12_002.Symmetry.Follower.cs

---

## Overview

`V12_002.Symmetry.Follower.cs` contains three God-methods responsible for the full lifecycle of
follower bracket resolution during symmetry events. Two of the three methods exceed LOC > 80,
classifying them as primary refactor targets in Wave 7 Phase 0. The cluster sits on the critical
path for every master fill that cascades to follower accounts: a defect here causes silent
position desynchronisation across the fleet.

- `SymmetryGuardTryResolveFollower` — CYC=12, LOC=83. Resolves whether a follower bracket can
  be matched to an incoming master fill event. Combines null-guard chains, FSM state checks, and
  multi-branch early-return logic.
- `SymmetryGuardSubmitFollowerBracket` — CYC=12, LOC=101. Constructs and submits the follower
  bracket order. Largest method in the file; interleaves order-object construction, limit/stop
  price calculation branches, and submission side-effects.
- `SymmetryGuardOnFollowerFill` — CYC=11, LOC=47. Handles the post-fill state transition for a
  follower bracket; drives FSM advancement and diagnostic logging.

---

## Blast Radius Summary

| Dimension | Detail |
|---|---|
| **Direct caller (TryResolve)** | `SymmetryGuardOnMasterFill` (`V12_002.Symmetry.cs`, EPIC-W7-056) |
| **Direct caller (Submit)** | `SymmetryGuardTryResolveFollower` (intra-file call chain) |
| **Direct caller (OnFollowerFill)** | `ProcessBracketEvent` (`V12_002.Symmetry.BracketFSM.cs`, EPIC-W7-055) |
| **Shared state read** | `_followerBrackets` (ConcurrentDictionary) — enumerated lock-free on strategy thread |
| **Shared state write** | `activePositions` (ConcurrentDictionary), bracket FSM `State` field |
| **External dependency** | NinjaTrader `SubmitOrderUnmanaged` — irreversible side-effect on submission path |
| **Threading constraint** | Strategy thread only; `_followerBrackets` must not be mutated during enumeration |
| **Risk on change** | HIGH — two LOC>80 God-methods with interleaved side-effects; extraction order matters |

**Affected symbol count (blast radius):** 8 symbols directly coupled; 2 shared concurrent state bags; 1 broker API call site.

---

## Top 3 Complexity Drivers

1. **Multi-branch follower resolution with FSM state fan-out (SymmetryGuardTryResolveFollower)**
   The resolution path evaluates four FSM states (`Active`, `Accepted`, `Submitted`, `Replacing`)
   via chained `||` conditions, preceded by two levels of null-guard (`follower != null`,
   `follower.AccountName != null`). Each additional FSM branch contributes +1 CYC; the compound
   early-return chain (no master position, no dispatch, mismatched instrument) adds 3 more
   independent decision points. Sub-total: ~7 CYC points from state fan-out and guard chain alone.

2. **Price calculation branching in bracket submission (SymmetryGuardSubmitFollowerBracket)**
   Stop and limit prices are computed via 3–4-way conditional trees (long/short direction × ATR
   vs fixed offset vs derived-from-master modes). Each direction × mode combination is a distinct
   branch, and the method also guards against zero-quantity, invalid instrument, and pre-existing
   bracket conditions before the `SubmitOrderUnmanaged` call. The LOC=101 body means extraction
   candidates include: `CalculateFollowerStopPrice`, `CalculateFollowerLimitPrice`, and a guard
   predicate `CanSubmitFollowerBracket`. Sub-total: ~9 CYC points from price/direction fan-out.

3. **Post-fill FSM advancement with dual diagnostic log branches (SymmetryGuardOnFollowerFill)**
   After verifying the matched bracket is in an expected state, the method must advance the FSM,
   optionally archive the bracket, and emit a diagnostic log entry for both success and failure
   paths. The `try/catch` wrapper contributes 2 structural CYC points; the dual-log path (filled
   cleanly vs filled with stale state) adds 2 more. The null-guard on `filled.Order` and the
   instrument-equality assertion account for the remaining ~4 CYC points.

---

## Recommended Extraction Count

**6 helper methods recommended across the 3-method cluster.**

| Target Method | Recommended Helpers | Est. Residual CYC |
|---|---|---|
| `SymmetryGuardTryResolveFollower` | `CanResolveFollowerBracket` (guard predicate), `MatchFollowerToMasterFill` (state fan-out) | ≤ 5 |
| `SymmetryGuardSubmitFollowerBracket` | `CanSubmitFollowerBracket`, `CalculateFollowerStopPrice`, `CalculateFollowerLimitPrice` | ≤ 6 |
| `SymmetryGuardOnFollowerFill` | `AdvanceFollowerBracketFsm` (state-transition + archive) | ≤ 5 |

**Rationale:** All three methods mix guard predicates, business-logic computation, and
side-effects in a single scope. Extracting guard predicates into bool helpers removes 2–3 CYC
points per method without altering observable behaviour. Price-calculation helpers eliminate the
direction × mode branching from the submission body and are independently unit-testable.
Sequencing: extract guards first (lowest risk), then computation helpers, then submission body.

---

## Agent Tracking

Agent Name: v12-phase0-hotspot | Bobcoins Used: 1.0 | Execution Time: ~50s
