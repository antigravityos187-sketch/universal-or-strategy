# EPIC-W7-146 Hotspot Analysis

**Method:** CancelOrphanedTargets
**CYC:** 13
**File:** src/V12_002.UI.Compliance.cs

---

## Overview

`CancelOrphanedTargets` is a private helper in the `V12_002` partial class (Apex Compliance / Fleet OCO
module) that iterates over all orders on a fleet account and cancels any working profit-target orders
(T1–T5 prefix) left open after a stop order fills. It is the sole first action in
`HandleFleetStopFill` (line 522) and carries a Cyclomatic Complexity of 13 — driven almost entirely
by a compounded OR chain of five `StartsWith` prefix checks and three layered guard conditions inside
a single `foreach` loop.

---

## Blast Radius Summary

| Dimension | Detail |
|---|---|
| **Direct caller** | `HandleFleetStopFill` (line 522, `src/V12_002.UI.Compliance.cs`) |
| **Caller chain** | `ProcessQueuedExecution_HandleFleetOCO` → `HandleFleetStopFill` → `CancelOrphanedTargets` |
| **Downstream call** | `CancelOrderOnAccount` (`src/V12_002.Orders.CancelGateway.cs:46`) |
| **Shared state read** | `account.Orders` (NinjaTrader broker object — snapshot via `.ToArray()`) |
| **Shared state written** | None directly; side-effect is broker-side order cancellation via `CancelOrderOnAccount` |
| **Instrument guard** | Reads `Instrument?.FullName` (strategy-level field) — must remain consistent across partial classes |
| **Order-state guard** | Checks `OrderState.Working` and `OrderState.Accepted` — tightly coupled to NinjaTrader `Cbi.OrderState` enum |
| **Target prefix set** | Hard-coded string literals `"T1_"` through `"T5_"` — naming contract shared implicitly with order-submission code |
| **Threading constraint** | Called from `ProcessAccountExecutionQueue` which is marshalled via `TriggerCustomEvent` onto the strategy thread |
| **Risk on change** | Medium — any rename of T1–T5 prefix convention or addition of T6+ targets requires updating this method; extraction of the filter predicate is safe |

**Affected symbol count (blast radius):** 4 symbols directly coupled; 1 shared broker state object.

---

## Top 3 Complexity Drivers

### 1. Five-branch OR chain in the target-name predicate (+5 CYC)

The innermost `if` block (lines 562–571) tests `o.Name.StartsWith(...)` for each of five target
prefixes (`"T1_"` through `"T5_"`) connected by `||`. In C# short-circuit evaluation each `||`
operand after the first is a separate decision branch, contributing one CYC point each. The five
branches alone account for five of the thirteen complexity points. The entire block is also guarded
by `o.Name != null` adding a sixth point for this logical cluster. This cluster is the single largest
refactor target: extracting a private `IsOrphanedTargetOrder(Order o)` predicate eliminates all five
`||` branches from the main loop body and reduces the method by ~5 CYC.

### 2. Compound two-condition `OrderState` guard (+2 CYC) with an early-`continue`

Line 560 checks `o.OrderState != OrderState.Working && o.OrderState != OrderState.Accepted`. The
`&&` short-circuit constitutes a branch (+1), and the `if ... continue` itself is a branch (+1).
Together they add 2 CYC points to filter non-actionable orders before the more expensive name check.
Because this guard and the name predicate are fully independent, the `OrderState` test can be
extracted into a standalone `IsActionableOrder(Order o)` helper with zero logic change.

### 3. Two-level null / instrument guard on `o` and `o.Instrument` (+3 CYC)

Lines 558–559 perform: (a) a null check on `o`, (b) a `?.` null-conditional access on
`o.Instrument`, and (c) a `?.` null-conditional access on `Instrument` (strategy field). Each
null-conditional `?.` operator introduces an implicit branch (taken if the receiver is null).
Combined with the `||` joining the two halves of the guard, this yields 3 CYC points that exist
purely for defensive null safety. These cannot be eliminated without assuming non-null broker data,
but they can be consolidated inside a single `IsEligibleOrder(Order o)` guard that merges the null
check, instrument match, and OrderState test into one clearly named predicate.

---

## Recommended Extraction Count

**2 helper extractions recommended.**

| # | Proposed Helper | Lines Absorbed | CYC Reduction |
|---|---|---|---|
| 1 | `IsOrphanedTargetOrder(Order o)` | 562–571 (the 5-branch `StartsWith` OR + null guard) | −5 CYC |
| 2 | `IsActionableOrderState(OrderState state)` | 560 (`Working \|\| Accepted` guard) | −2 CYC |

**Projected post-extraction CYC of `CancelOrphanedTargets`:** ≈ 5 (base 1 + foreach +1 + null/instrument guard +3)

**Rationale:** The five-prefix OR chain is the dominant driver and a natural semantic unit
("is this order a profit-target order?"). Extracting it yields an immediately testable,
named predicate. The `OrderState` pair is a secondary extraction that pays for itself if the
set of actionable states ever expands. The null/instrument guards (driver 3) are too
intertwined with broker-object defensiveness to extract safely at Phase 0 without additional
knowledge of NinjaTrader's thread-safety guarantees.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase0-hotspot |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | ~60s |
