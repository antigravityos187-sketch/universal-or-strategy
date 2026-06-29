# EPIC-W7-150 Hotspot Analysis

**Method:** ProcessQueuedExecution_HandleFleetBrackets
**CYC:** 10
**File:** src/V12_002.UI.Compliance.cs (lines 486–517)

---

## Overview

`ProcessQueuedExecution_HandleFleetBrackets` is a private method in the compliance/execution
processing pipeline. It fires on every dequeued account execution event and is responsible for
detecting when a fleet entry order has been filled, then delegating to `SymmetryGuardOnFollowerFill`
to anchor the follower bracket. Despite its 32-line body, the method accumulates CYC=10 through
deeply nested compound boolean guards wrapped inside a try/catch, a foreach scan over a live
dictionary snapshot, and an inline ternary for fill-price resolution.

---

## Blast Radius Summary

| Dimension | Detail |
|---|---|
| **Direct caller** | `ProcessQueuedExecution` (line 798, same file) |
| **Caller chain** | NinjaTrader execution callback → `_executionQueue.Enqueue` → `ProcessQueuedExecution` → `ProcessQueuedExecution_HandleFleetBrackets` |
| **Downstream callee** | `SymmetryGuardOnFollowerFill` (`src/V12_002.Symmetry.Follower.cs`, line 17) |
| **Shared state read** | `entryOrders` (ConcurrentDictionary — `.ToArray()` snapshot), `activePositions` (ConcurrentDictionary — `.TryGetValue`) |
| **Shared state written** | `activePositions` (via `SymmetryGuardOnFollowerFill` → sets `EntryFilled = true`) |
| **Position fields touched** | `PositionInfo.IsFollower`, `PositionInfo.EntryFilled` |
| **Side-effects** | Triggers follower bracket submission; sets `EntryFilled` flag on the matched position; prints error on exception |
| **Threading constraint** | Strategy thread only (called from queued execution pump); `entryOrders` snapshot via `.ToArray()` is the only thread-safety guard |
| **Risk on change** | **Medium-High** — the `break` after match inside `foreach` is load-bearing; removing it would cause multi-fire on same order. The `!pos.EntryFilled` guard prevents duplicate bracket submission and must be preserved in any extraction |
| **Sibling method** | `ProcessQueuedExecution_HandleFleetOCO` (line 698) follows in same call sequence |

**Affected symbol count (blast radius):** 5 symbols directly coupled; 2 shared concurrent state bags.

---

## Top 3 Complexity Drivers

1. **Three-level compound boolean guard on `activePositions` lookup (lines 498–502)**
   The inner `if` chains three independent conditions with `&&`:
   `activePositions.TryGetValue(fleetKey, out var pos) && pos.IsFollower && !pos.EntryFilled`.
   Each `&&` operand that can independently terminate the branch is scored +1 CYC by McCabe's
   rule, producing 3 branch points from a single logical check. Combined with the outer
   `if (kvp.Value == filledOrder)` guard, this creates a 4-level nesting depth inside the
   `foreach`. Sub-total: **~4 CYC points** from inner guard fan-out.

2. **`foreach` scan over `entryOrders` with load-bearing `break` (lines 493–510)**
   The loop over `entryOrders.ToArray()` must scan all active fleet orders to find the one
   matching `filledOrder` by reference equality. The loop body contains a nested 2-level `if`
   chain and a `break`, meaning three distinct exit paths exist from the loop body (no match,
   outer-if-false match, inner-if-true match). The `foreach` itself adds +1 CYC, and the inner
   `if (kvp.Value == filledOrder)` adds +1. The loop could grow further if `entryOrders`
   gains partitioned key schemas. Sub-total: **~3 CYC points** from loop + match branch.

3. **Inline ternary for fill-price resolution + outer `if` + `try/catch` structural overhead (lines 490–516)**
   `item.EventArgs.Execution != null ? item.EventArgs.Execution.Price : 0` (line 504–505) adds
   +1 CYC. The outer `if (filledOrder != null && filledOrder.OrderState == OrderState.Filled)`
   contributes +2 (the `if` branch itself plus the `&&` short-circuit on `OrderState`). The
   `catch (Exception ex)` block adds +1. Together these three structural elements contribute
   **~4 CYC points** of overhead that are independent of the fleet-matching business logic.

---

## Recommended Extraction Count

**2 extractions recommended.**

| # | Proposed Helper | Responsibility | Estimated CYC Reduction |
|---|---|---|---|
| 1 | `TryMatchEntryOrderToFleetKey(filledOrder, out string fleetKey)` | Encapsulates the `foreach` scan over `entryOrders`, key lookup, and `break` logic; returns `bool` + out key | −3 CYC from dispatcher |
| 2 | `TryActivateFollowerBracket(string fleetKey, QueuedAccountExecution item)` | Encapsulates the `activePositions.TryGetValue` triple-guard, fill-price ternary, and `SymmetryGuardOnFollowerFill` call | −3 CYC from dispatcher |

**Rationale:**
After extraction, `ProcessQueuedExecution_HandleFleetBrackets` would reduce to:
`try { if (filledOrder null-check) { if (TryMatch...) TryActivate...; } } catch { ... }`
targeting CYC ≤ 4. The two helpers would each carry CYC ≤ 5, well below the Wave 7 threshold.
The `!pos.EntryFilled` guard and the `break` must be preserved verbatim inside the extracted
helpers to maintain idempotence and single-fire semantics.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase0-hotspot |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | ~38s |
