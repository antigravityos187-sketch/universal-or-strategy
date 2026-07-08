# EPIC-W7-067 Hotspot Analysis

**Method:** `SymmetryFindDispatchForMasterFill`
**CYC:** 8
**File:** `src/V12_002.Symmetry.cs` (lines 326–352)

---

## Overview

`SymmetryFindDispatchForMasterFill` is the fallback dispatch-lookup path inside the Symmetry Guard
subsystem. When a master-entry fill arrives and no direct `symmetryMasterEntryToDispatch` mapping is
found, this method scans the entire `symmetryDispatchById` dictionary to locate the best matching
`SymmetryDispatchContext` by trade-type, direction, and TTL. It returns the oldest surviving
unresolved context that matches the incoming fill — functioning as a linear-scan reconciler for
out-of-band fills that bypass the happy-path entry-name lookup.

The method is called exclusively from [`SymmetryGuardOnMasterFill`](src/V12_002.Symmetry.cs:283),
which is itself invoked from `ValidateAndPrepareEntryFill` in
[`src/V12_002.Orders.Callbacks.cs`](src/V12_002.Orders.Callbacks.cs:368) on every master-position
fill event on the strategy thread.

---

## Blast Radius Summary

| Dimension | Detail |
|---|---|
| **Direct caller** | `SymmetryGuardOnMasterFill` (line 283, `src/V12_002.Symmetry.cs`) |
| **Caller chain** | `ValidateAndPrepareEntryFill` → `SymmetryGuardOnMasterFill` → `SymmetryFindDispatchForMasterFill` |
| **Shared state read** | `symmetryDispatchById` (`ConcurrentDictionary<string, SymmetryDispatchContext>`) — `.ToArray()` snapshot |
| **Helper called** | `SymmetryNormalizeTradeType` (normalization pre-filter, `src/V12_002.Symmetry.Replace.cs:322`) |
| **AnchorSnapshot dependency** | `ctx.Anchor.IsResolved` — read via `Volatile.Read` per ADR-019 |
| **TTL constant** | `SymmetryDispatchTtl` (5 min, `src/V12_002.Symmetry.cs:137`) |
| **Post-call side-effect** | Result feeds the CAS-loop in `SymmetryGuardOnMasterFill` that publishes `AnchorSnapshot`; failure returns null and aborts anchor publication |
| **Threading constraint** | Strategy thread (called from order-callback path); `ToArray()` snapshot makes iteration safe but allocates on every fallback invocation |
| **Risk on change** | High — the oldest-wins selection policy (`ctx.CreatedUtc < best.CreatedUtc`) is the sole guard against the duplicate-dispatch race condition documented in H-11 (Phase 7); any change to selection semantics can silently double-anchor a fleet |

**Affected symbol count (blast radius):** 7 symbols directly coupled; 1 shared concurrent state bag;
ADR-019 CAS-loop correctness depends on this method returning a unique, unresolved context.

---

## CYC=8 Breakdown

The eight independent paths through `SymmetryFindDispatchForMasterFill` are:

| # | Branch | CYC contribution |
|---|---|---|
| 1 | Method entry (base path) | +1 |
| 2 | `foreach` loop over `symmetryDispatchById.ToArray()` — loop body entered | +1 |
| 3 | `ctx == null \|\| ctx.Anchor.IsResolved` — null/resolved guard, skip | +1 |
| 4 | `ctx.Direction != direction` — direction mismatch, skip | +1 |
| 5 | `!string.Equals(ctx.TradeType, norm, ...)` — trade-type mismatch, skip | +1 |
| 6 | `fillTimeUtc - ctx.CreatedUtc > SymmetryDispatchTtl` — TTL expired, skip | +1 |
| 7 | `best == null` — first qualifying candidate, assign unconditionally | +1 |
| 8 | `ctx.CreatedUtc < best.CreatedUtc` — subsequent candidate older than current best, replace | +1 |

All eight guards sit in a flat `foreach` body with no nested loops; CYC is exactly 8 with no
hidden recursion.

---

## Top 3 Complexity Drivers

1. **Four-predicate sequential skip-filter inside a dictionary scan**
   Guards 3–6 form a cascade of four independent early-`continue` predicates applied to every
   context in `symmetryDispatchById`. Each predicate contributes +1 CYC and must be evaluated in
   the correct order: the null/resolved check must precede the direction and type checks (which
   dereference `ctx`), and the TTL check should follow type filtering to avoid timestamp arithmetic
   on discarded contexts. Any reordering risks NPE or stale-dispatch selection.

2. **Oldest-wins selection policy requiring two-branch comparison**
   Guards 7 and 8 encode a min-by-`CreatedUtc` fold: the first branch handles the `best == null`
   initialization case, the second handles subsequent replacement. While only 2 CYC points, this
   is the semantic core of the H-11 duplicate-dispatch guard. If collapsed into a single ternary
   or replaced with LINQ `MinBy`, the null-initialization semantic must be preserved exactly;
   LINQ `MinBy` on an empty sequence throws, making the current explicit null-start pattern safer.

3. **`ToArray()` snapshot allocation on the hot fill-callback path**
   The `ConcurrentDictionary.ToArray()` call is not a CYC contributor but is a runtime hotspot:
   it allocates a `KeyValuePair[]` heap object on every master-fill event that lacks a direct
   entry-name mapping. In high-frequency fill bursts (multi-contract partial fills), this path
   executes once per fill. Extracting the snapshot to a reusable buffer, or making the direct
   `symmetryMasterEntryToDispatch` mapping mandatory at dispatch-time, would eliminate the
   allocation entirely and make this method unreachable on the hot path.

---

## Recommended Extraction Count

**0 helper extractions recommended for Phase 0. 1 structural refactor flagged for Phase 1.**

**Rationale:**

At CYC=8 the method is at the upper bound of the acceptable range (≤8 per project standard) and
requires no immediate extraction. The four skip-predicates are tightly coupled by ordering
constraints and are clearer in-line than if split across helpers. The oldest-wins fold is two lines
and benefits from co-location with its guards.

The Phase 1 work item is: **make the `symmetryMasterEntryToDispatch` pre-mapping mandatory**, so
that `SymmetryFindDispatchForMasterFill` is only invoked as a defensive fallback and never on the
latency-sensitive hot path. This eliminates the `ToArray()` allocation for all normal fills without
changing the method's CYC or semantics.

---

## Agent Tracking

Agent Name: bob-phase0-hotspot | Bobcoins Used: 1.0 | Execution Time: ~60s
