# EPIC-W7-047 Hotspot Analysis

**Method:** CancelOrphanedTargets
**CYC:** 13
**File:** src/V12_002.UI.Compliance.cs

---

## Overview

`CancelOrphanedTargets` (lines 553–578, `src/V12_002.UI.Compliance.cs`) is a fleet OCO cleanup
routine invoked when a stop order fills on any fleet account. Its job is to iterate all open orders
on the account, match T1–T5 profit-target prefixes, and call `CancelOrderOnAccount` for each live
one. Despite being only 26 lines, it accumulates CYC=13 through a dense combination of a
per-element null guard, a dual-field nullable-traversal guard, a two-branch state check, and a
five-arm compound `||` name-prefix filter — every arm of which adds a distinct decision path.

---

## Blast Radius Summary

| Dimension | Detail |
|---|---|
| **Direct caller** | `HandleFleetStopFill` (line 522, same file) |
| **Caller chain** | `ProcessQueuedExecution_HandleFleetOCO` → `HandleFleetStopFill` → `CancelOrphanedTargets` |
| **Entry point** | `OnAccountExecutionUpdate` → broker-thread enqueue → `ProcessAccountExecutionQueue` (strategy thread) |
| **Downstream call** | `CancelOrderOnAccount` (src/V12_002.Orders.CancelGateway.cs, line 46) |
| **Shared state read** | `account.Orders` (broker-owned collection, snapshot via `.ToArray()`), `Instrument.FullName` |
| **Side-effects** | Submits live cancel requests to broker gateway via `CancelOrderOnAccount`; increments no counters directly but return value drives a `Print` log in `HandleFleetStopFill` |
| **Threading constraint** | Executes on strategy thread (called from `ProcessQueuedExecution_HandleFleetOCO`); `.ToArray()` snapshot prevents concurrent-modification, but broker-side order state may have already changed at cancel time |
| **Blast width** | 1 direct caller, 3 call-chain ancestors, 1 downstream gateway; `CancelOrderOnAccount` itself is called from 9 other sites across 6 files — any signature change ripples widely |
| **Risk on change** | Medium-High — the 5-arm prefix filter is the primary correctness surface; adding a T6 target tier would require a code change here, and any extraction must preserve the `account`-scoped order snapshot semantics |

**Affected symbol count (blast radius):** 5 symbols in the direct call chain; `CancelOrderOnAccount` shared by 9 additional call sites across 6 source files.

---

## Top 3 Complexity Drivers

1. **Five-arm compound `||` prefix filter (lines 565–571)**
   The `o.Name.StartsWith("T1_") || ... || o.Name.StartsWith("T5_")` expression contributes 4
   independent decision points (+4 CYC) on top of the outer `if (o.Name != null ...)` null guard
   (+1 CYC). Each prefix arm is a wholly independent conditional branch from the coverage
   perspective. The current pattern also silently excludes any future `T6_`/`T7_` tiers without a
   compile-time signal, making the filter a latent extensibility hazard. Sub-total: ~5 CYC points.

2. **Dual-field nullable instrument-name guard (line 558)**
   `o.Instrument?.FullName != Instrument?.FullName` uses two independent null-conditional
   operators, meaning both `o.Instrument` and `Instrument` can independently be null, creating
   two implicit short-circuit branches per loop iteration in addition to the explicit `o == null`
   check on the same line. This guard exists to prevent cancelling orders on unrelated instruments
   sharing the same account (a real multi-instrument scenario) but the three-part compound
   expression (`o == null || lhs != rhs` where either side can be null) adds ~3 CYC. Sub-total:
   ~3 CYC points.

3. **Two-state `OrderState` gate with `foreach` loop structure (lines 556–561)**
   The `foreach (Order o in account.Orders.ToArray())` loop body begins with two `continue`-based
   early exits before any business logic executes. The `OrderState != Working && != Accepted`
   combined check adds 2 CYC (one per state), and the `foreach` itself adds 1 CYC for the
   iteration boundary. The `.ToArray()` snapshot call is safe but relies on the broker-thread-safe
   property of `account.Orders`; no lock is held during cancel submission. Sub-total: ~3 CYC
   points (plus 1 for the base path and 1 for the loop = 2 structural), making the loop/guard
   cluster the third-largest driver.

---

## Recommended Extraction Count

**2 extractions recommended.**

**Rationale:**

The method's CYC=13 stems from two logically separable concerns crammed into a single loop body:
(a) **order filter predicate** — null checks, instrument match, state gate, and name-prefix test —
and (b) **cancel dispatch** — calling `CancelOrderOnAccount` and incrementing the counter.

Recommended extractions:

1. **`IsOrphanedTarget(Order o)`** — extract the full boolean filter (`o != null`, instrument
   match, state gate, and T1–T5 prefix test) into a private predicate method. This reduces the
   loop body to a single `if (IsOrphanedTarget(o))` guard and isolates the prefix list so a future
   T6 extension is a one-line change in one place. Estimated post-extraction CYC of extracted
   predicate: ~7; dispatcher loop drops to ~4.

2. **`CancelMatchingOrders(Account account, Func<Order,bool> predicate)`** — optional
   generalization of the cancel-and-count loop pattern that is already duplicated verbatim in
   `HandleFleetTargetFill` (lines 676–693, same file). Centralising would eliminate the
   structural duplication and reduce both call sites to ~2 CYC each. Lower priority than (1);
   implement in Phase 2 after test coverage is in place.

---

## Agent Tracking

Agent Name: bob-phase0-hotspot | Bobcoins Used: 1.0 | Execution Time: ~60s
