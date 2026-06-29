# EPIC-W7-147 Hotspot Analysis

**Method:** `ProcessQueuedExecution_HandleFleetOCO`
**CYC:** 15
**File:** `src/V12_002.UI.Compliance.cs` (lines 698–727)

---

## Overview

`ProcessQueuedExecution_HandleFleetOCO` is the fleet OCO (One-Cancels-Other) dispatch router
inside the compliance execution pipeline. It is called from `ProcessQueuedExecution` (line 799)
on the strategy thread after each dequeued broker execution event. Its role is to classify a
filled/part-filled fleet order by name prefix (`Stop_` vs `T{n}_`) and delegate to one of two
heavy sub-handlers: `HandleFleetStopFill` or `HandleFleetTargetFill`.

The dispatcher itself is 30 lines but accumulates CYC=15 due to a deeply compound outer guard
(4-condition `&&` chain with an embedded `||`), a `try/catch`, a `StartsWith` branch, and a
multi-condition `else if` with two `&&` operands. The two sub-handlers it delegates to carry
substantial additional complexity (HandleFleetStopFill ~5 CYC, HandleFleetTargetFill ~14 CYC),
making this cluster the dominant OCO-handling hotspot in the compliance subsystem.

---

## Blast Radius Summary

| Dimension | Detail |
|---|---|
| **Direct caller** | `ProcessQueuedExecution` (line 799, `src/V12_002.UI.Compliance.cs`) |
| **Caller chain** | `ProcessAccountExecutionQueue` → `ProcessQueuedExecution` → `ProcessQueuedExecution_HandleFleetOCO` |
| **Sub-handlers delegated to** | `HandleFleetStopFill` (line 519), `HandleFleetTargetFill` (line 624) |
| **Helpers called transitively** | `CancelOrphanedTargets`, `ExtractEntryKeyFromStopName`, `FinalizeStopFilledPosition`, `ApplyTargetFill`, `CancelOrderOnAccount`, `IsFleetAccount`, `SymmetryGuardForgetEntry` |
| **Shared state mutated** | `activePositions` (ConcurrentDictionary), `stopOrders`, `entryOrders`, `pendingStopReplacements`, `_nakedPositionFirstSeen`, `pendingReplacementCount` |
| **External dependency** | `Account.Orders` (broker-side collection — snapshotted via `ToArray()` inside sub-handlers) |
| **Threading constraint** | Strategy thread only; arrives via `TriggerCustomEvent` marshal from `OnAccountExecutionUpdate` |
| **Risk on change** | High — OCO pair integrity (stop vs. target cancellation symmetry) must be preserved; any mis-routing silently leaves orphaned working orders on broker accounts |

**Affected symbol count (blast radius):** 9 symbols directly coupled; 6 shared concurrent state bags.

---

## Top 3 Complexity Drivers

### 1. Compound multi-condition outer guard with embedded disjunction (lines 704–709)

```csharp
if (
    ocoOrder != null
    && ocoAcct != null
    && IsFleetAccount(ocoAcct)
    && (ocoOrder.OrderState == OrderState.Filled || ocoOrder.OrderState == OrderState.PartFilled)
)
```

Four `&&`-chained predicates with one embedded `||` create five independent decision points
evaluated left-to-right. Per CYC counting each short-circuit `&&` operand beyond the first is a
branch: `ocoAcct != null` (+1), `IsFleetAccount(ocoAcct)` (+1), and the embedded
`(Filled || PartFilled)` adds +1 for the `||`. This single guard contributes approximately
**+4 CYC** — the largest single source — and cannot be collapsed without either suppressing the
null guards (unsafe) or introducing an `IsOcoOrderActionable(item)` predicate helper.

### 2. Multi-condition `else if` name-prefix classifier with chained `&&` (line 717)

```csharp
else if (ocoName.StartsWith("T") && ocoName.Length > 2 && ocoName[2] == '_')
```

Three `&&`-chained predicates in the `else if` branch add **+3 CYC** (the branch itself +1,
each additional `&&` operand +1). The `ocoName.Length > 2` bounds guard and the direct character
index `ocoName[2] == '_'` are defensive checks that cannot be removed without risking an
`IndexOutOfRangeException`, but they inflate the dispatcher's complexity unnecessarily. An
`IsTargetOrderName(string name)` predicate would encapsulate all three checks behind a single
decision point, reducing dispatcher CYC to approximately 8–9.

### 3. `try/catch` structural overhead + `StartsWith` branch nesting depth (lines 700–726)

The outermost `try/catch(Exception ex)` block adds **+1 CYC** (the catch path). Combined with the
`if (ocoName.StartsWith("Stop_"))` branch (+1) nested three levels deep inside the outer guard and
try, the effective nesting depth of the innermost target-handler call is **4 levels** (try →
outer-if → else-if → delegate call). This nesting depth — while not directly a CYC multiplier —
makes the method harder to reason about statically and pushes the aggregate CYC above thresholds
that IDE refactor lenses will flag. Extracting the name-prefix dispatch into a named enum or
`GetOcoOrderType(string name)` factory eliminates both the `try/catch`-redundant branching and
the nesting depth in a single move.

---

## Recommended Extraction Count

**3 targeted extractions recommended.**

| # | Extraction | Rationale | CYC reduction (dispatcher) |
|---|---|---|---|
| 1 | `IsOcoOrderActionable(QueuedAccountExecution item)` | Encapsulates the 4-predicate null + fleet + state guard into a single bool helper | −4 |
| 2 | `GetOcoOrderType(string name)` returning an `OcoOrderType` enum (`Stop`, `Target`, `Unknown`) | Collapses the `StartsWith("Stop_")` + multi-condition `else if` into a single `switch` in the dispatcher | −3 |
| 3 | `DispatchOcoOrderToHandler(OcoOrderType type, ...)` | Replaces the nested if/else-if delegating calls with a clean `switch` expression — no logic change, pure structure | −1 (nesting depth) |

After these 3 extractions the dispatcher CYC drops from **15 → ≤7**, bringing it well within the
target threshold of ≤10. The two sub-handlers (`HandleFleetStopFill`, `HandleFleetTargetFill`)
should be addressed in a subsequent phase as independent hotspots.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase0-hotspot |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | ~60s |
