# EPIC-W7-006 — Phase 1: Scope Definition

## Single Method in Scope

**`HydrateWorkingOrdersFromBroker()`**

> This is the canonical source method for the epic concept `AdoptFleetWorkingOrders`.
> It is the top-level orchestrator of the entire order-adoption cluster (lines 309–457,
> `src/V12_002.SIMA.Lifecycle.cs`). All refactoring in this epic targets this method's body only.

---

## Complexity

| Metric | Value |
|---|---|
| Current CYC | **14** (measured; see 00-hotspots.md) |
| Target CYC | **≤ 8** |
| Current lines | 149 (lines 309–457) |
| Primary driver | Inline master `PositionInfo` construction block (lines 388–420) carrying 6 trade-DNA flag branches, asymmetric with the already-extracted `RebuildFleetPositionFromEntry()` fleet path |

---

## File

```
src/V12_002.SIMA.Lifecycle.cs
```

- Method declaration: line 309
- Method close: line 457

---

## Callers

| Call Site | File | Line | Context |
|---|---|---|---|
| Direct call (startup) | `src/V12_002.SIMA.Lifecycle.cs` | 196 | Called inline during `OnStateChange` startup sequence |
| Enqueued call (reconnect) | `src/V12_002.Lifecycle.cs` | 337 | `Enqueue(ctx => ctx.HydrateWorkingOrdersFromBroker())` on broker reconnect |

**Callers count: 2**

Both call sites pass zero arguments and consume no return value. Method signature is stable;
internal refactoring carries zero interface risk to callers.

---

## Scope Boundary Statement

> **Only `HydrateWorkingOrdersFromBroker` and its new extracted helper methods.**

- **In scope:** `HydrateWorkingOrdersFromBroker` (existing body, to be refactored) + up to 3 new
  private helper methods created by extraction (e.g., `RebuildMasterFilledPosition`,
  `HydrateMasterFilledPositions`, and the wiring of `AdoptMasterOrders` → `IsValidOrderState`
  per hotspots.md recommendations).
- **Out of scope:** All other existing methods in the cluster (`AdoptFleetOrders`,
  `AdoptOrdersFromAccount`, `AdoptSingleOrder`, `AdoptMasterOrders`, `RouteOrderToTargetDict`,
  `RebuildFleetPositionFromEntry`, `ClassifyOrderByPrefix`, `IsValidOrderState`) — these are
  passive callees that must not be modified unless a future phase explicitly targets them.
- **Out of scope:** All 43 consumer files of the shared `ConcurrentDictionary` fields
  (`activePositions`, `stopOrders`, `entryOrders`, `target1Orders`–`target5Orders`).
- **Out of scope:** `HydrateFSMsFromWorkingOrders()` (called at line 445; must remain an
  unmodified callee).

---

## Sequential Thinking Summary

**Thought 1 — Identify the single in-scope method:**  
`HydrateWorkingOrdersFromBroker` is the highest-CYC non-yet-refactored orchestrator in the cluster
(CYC 14). It carries 108 lines of inline logic (master position reconstruction) not yet extracted
to a helper, unlike the symmetric fleet path (`RebuildFleetPositionFromEntry`). It is the
Phase 1 target.

**Thought 2 — Confirm out-of-scope boundaries:**  
`AdoptMasterOrders` (CYC 17) is a *callee*, not the orchestrator. The three recommended
extractions (hotspots.md §Recommended Extractions) produce *new* helper symbols owned by this
epic — they are not independent modifications to existing out-of-scope methods. All other 7
cluster methods are read-only for this phase.

**Thought 3 — Validate caller count and interface stability:**  
Exactly 2 direct call sites exist; both are void-return, zero-argument invocations. No signature
change is needed. Scope boundary is clean and carries zero risk to the broader system interface.

---

## Agent Tracking

```
Agent Name:      v12-phase1-scope
Bobcoins Used:   0.8
Execution Time:  ~3 minutes
Epic:            EPIC-W7-006
Wave:            7
Phase:           1 (Scope Definition)
Output:          docs/brain/EPIC-W7-006/00-scope.md
Source:          src/V12_002.SIMA.Lifecycle.cs (lines 309–457)
Method in Scope: HydrateWorkingOrdersFromBroker (CYC 14 → target ≤ 8)
Callers Count:   2
```
