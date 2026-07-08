# EPIC-W7-049 Hotspot Analysis

**Method:** `ManageTrail_RunPerTradeBranches`
**CYC:** 11
**File:** `src/V12_002.Trailing.cs` (lines 240–255)

---

## Overview

`ManageTrail_RunPerTradeBranches` is a per-trade dispatch gate called inside the inner
position loop of `ManageTrailingStops`. It routes each active position to one of three
EMA-based trailing-stop handlers — `TrailHandler_TREND_E1`, `TrailHandler_TREND_E2`,
or `TrailHandler_RETEST` — and returns `true` to short-circuit the outer point-based
trailing logic when a specialised handler applies. The method is intentionally a thin
dispatcher: it contains no trailing arithmetic itself, but its compound boolean guards
(three flag fields ANDed with an RMA-exclusion NOT) drive CYC to 11 and represent the
primary classification surface for trade-type routing in the trailing subsystem.

---

## Blast Radius Summary

| Dimension | Detail |
|---|---|
| **Direct caller** | `ManageTrailingStops` (line 71, `src/V12_002.Trailing.cs`) — called once per position per throttle tick |
| **Delegated callees** | `TrailHandler_TREND_E1` (line 257), `TrailHandler_TREND_E2` (line 312), `TrailHandler_RETEST` (line 342) — all in the same file |
| **Shared state read** | `pos.IsTRENDTrade`, `pos.IsTRENDEntry1`, `pos.IsTRENDEntry2`, `pos.IsRetestTrade`, `pos.IsRMATrade` (fields of `PositionInfo`) |
| **Short-circuit effect** | `true` return prevents `ManageTrail_RunPointBasedTrailing` from executing for TREND/RETEST non-RMA positions |
| **Side-effects** | None in the dispatcher body itself; all side-effects (stop order mutations, `Print` calls) occur inside delegated handlers |
| **Threading constraint** | Strategy thread only (called from `ManageTrailingStops` which is already behind adaptive throttle gate) |
| **PositionInfo consumers** | 15 source files reference the flag fields (`IsTRENDTrade`, `IsRMATrade`, etc.), making PositionInfo a high-blast shared state bag |
| **Risk on change** | Medium — any reordering of the three guard branches could silently misdirect RMA+TREND/RETEST positions to EMA handlers; the `!IsRMATrade` exclusion must be preserved on every branch |

**Affected symbol count (blast radius):** 4 symbols directly coupled (1 caller + 3 callees); 5 `PositionInfo` flag fields read; 15 files share those flags.

---

## Top 3 Complexity Drivers

1. **Compound flag guards with RMA exclusion repeated on every branch (+6 CYC)**
   Each of the three dispatch conditions ANDs two or three `PositionInfo` boolean fields
   together with a `!pos.IsRMATrade` negation guard. Under McCabe counting, each `&&`
   operand and each `!` inversion is an independent predicate and adds a branch edge.
   The TREND-E1 condition alone (`IsTRENDTrade && IsTRENDEntry1 && !IsRMATrade`) contributes
   3 branch edges; TREND-E2 contributes 3 more; RETEST contributes 2. This repeated exclusion
   pattern is the dominant CYC driver and the primary extraction candidate: a single
   `IsEMAOnlyTrade(pos)` predicate would collapse all three guards and eliminate 4–5 CYC points.

2. **Implicit fall-through semantics with `return false` as the default path (+2 CYC)**
   The method's contract relies on mutual exclusivity of the three guard conditions. Because
   no `else` chain is used, all three `if` blocks are evaluated in sequence even though at
   most one fires, creating three independent decision nodes rather than a single switch/
   dispatch table. If a `PositionInfo` could satisfy more than one condition (e.g., a future
   trade type that is both TREND and Retest), the first matching branch would silently win
   without warning — a latent correctness hazard baked into the control flow shape.

3. **`!IsRMATrade` negation duplicated across branches instead of pre-computed (+3 CYC)**
   The RMA-exclusion flag is evaluated separately inside each of the three guard conditions
   rather than being factored out as a single early-exit guard at the top of the method. This
   duplication means the same predicate participates in three independent branch edges,
   inflating CYC without adding any new semantic distinction. Pre-computing a local
   `bool isEmaCandidate = !pos.IsRMATrade;` and exiting early when false would remove two
   of the three redundant negation branch points.

---

## Recommended Extraction Count

**2 targeted refactors recommended; no additional method extractions required beyond those.**

**Rationale:**

The dispatcher body (16 lines) is already well-scoped — its CYC comes from predicate
complexity, not from embedded logic. The two recommended refactors are:

1. **Extract `IsEMATradeCandidate(PositionInfo)` predicate** — consolidates the shared
   `!pos.IsRMATrade` exclusion into a single named concept, reducing the dispatcher's CYC
   by ~4 points and making the routing intent self-documenting.

2. **Replace sequential `if` chain with early RMA exit guard** — add `if (pos.IsRMATrade) return false;`
   at the top of the method, then remove the `!IsRMATrade` condition from all three branches.
   This does not require a new method but eliminates 2–3 CYC points from negation duplication.

No extraction of the three `TrailHandler_*` callees is warranted at Phase 0 — they are
already independent methods with well-defined responsibilities.

---

## Agent Tracking

| Field | Value |
|---|---|
| Agent Name | bob-phase0-hotspot |
| Epic | EPIC-W7-049 |
| Wave / Phase | 7 / 0 |
| Bobcoins Used | 1.0 |
| Execution Time | ~60s |
| CYC Confirmed | 11 |
