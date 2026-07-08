# Phase 2: Architecture Plan — EPIC-W7-070

**Agent:** v12-phase2-architecture
**Wave:** 7 | **Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:10:00Z
**Input:** docs/brain/EPIC-W7-070/01-scope-boundary.md

---

## Method Under Extraction

- **Method:** `HydrateFSMsFromWorkingOrders`
- **Source File:** `src/V12_002.SIMA.Lifecycle.cs`
- **Line:** 787–891 (105 LOC)
- **Original CYC:** 13
- **Signature:** `private void HydrateFSMsFromWorkingOrders()`

### jcodemunch get_context_bundle result

Symbol resolved via `src/V12_002.SIMA.Lifecycle.cs::V12_002.HydrateFSMsFromWorkingOrders#method`.

Key findings:
- Method is `private void`, no parameters — internal orchestration only.
- Docstring confirms: "Phase 5: Rebuilds `_followerBrackets` and `_orderIdToFsmKey` from already-adopted working orders. Idempotent — safe to call on every reconnect."
- Two distinct passes: (1) **Entry Order Pass** — `foreach (var kvp in entryOrders.ToArray())` with 8 branch points; (2) **Position Pass** — single delegation to `HydrateFromOpenPositions(...)`.
- Source fully read from lines 787–891; no additional context files needed.

### jcodemunch get_call_hierarchy result

- **Callers (depth 2):**
  - `HydrateWorkingOrdersFromBroker` (line 309, depth 1, `ast_resolved`) — direct caller
  - `EnumerateApexAccounts` (line 140, depth 2, `ast_resolved`) — indirect caller
- **Callees (depth 1, key methods):**
  - `MapOrderStateToFSMState` (line 469)
  - `FindLivePosition` (line 605)
  - `ResolveRemainingContracts` (line 532)
  - `BuildFSM` (line 505)
  - `LinkTargetOrderToFSM` (line 579) — called 5× (target1–5Orders)
  - `RegisterFSM` (line 551)
  - `HydrateFromOpenPositions` (line 625)
  - `LogBuffer.Format` (via `Print` delegates)
- Total callee count: 33 (including constants and indirect log methods)

### jcodemunch get_dependency_graph result

- **File-level dependencies:** 0 imports, 0 importers (node_count=1, edge_count=0)
- `src/V12_002.SIMA.Lifecycle.cs` is a self-contained partial class file — no cross-file import edges in the graph
- Blast radius confirmed zero: refactoring is fully contained within this file

### jcodemunch get_extraction_candidates result

- No candidates returned by index heuristic (min_callers=1, min_complexity=3)
- Manual analysis of source confirms 2 clear extraction candidates within `HydrateFSMsFromWorkingOrders` — see Extraction Plan below

---

## Sequential Thinking Summary

**5-thought chain completed. Final conclusion (Thought 5):**

Architecture decision confirmed with 2 private helper extractions:

1. **`ProcessEntryOrderForFSMHydration`** absorbs the full foreach loop body (lines 797–854). It applies 4 guard-clause early returns (null check, follower+position check, ExecutingAccount null check, idempotent check), resolves FSM state via `MapOrderStateToFSMState` + `FindLivePosition` + `ResolveRemainingContracts`, builds the FSM via `BuildFSM`, delegates stop-order linking to helper 2 (`LinkStopOrderIfPresent`), calls 5x `LinkTargetOrderToFSM`, and registers via `RegisterFSM`. Projected CYC = 7.

2. **`LinkStopOrderIfPresent`** handles the stop-order association block exclusively: `stopOrders.TryGetValue` lookup + null check + `fsm.StopOrder` assignment + `_orderIdToFsmKey` insertion + `ordersIndexed` increment. Projected CYC = 3.

**Parent method** retains: counter initialization, start log, `foreach` calling `ProcessEntryOrderForFSMHydration`, entry-pass log, `HydrateFromOpenPositions` delegation, position-pass log, totals log. Projected CYC = 3.

**Verification:** max_cyc = 7 ≤ 8. All Jane Street rules met. Architecture is sound.

---

## Extraction Plan

| Helper Method Name | Responsibility | Projected CYC |
|---|---|---|
| `ProcessEntryOrderForFSMHydration(string entryKey, Order entryOrder, ref int ordersIndexed, ref int fsmCreated)` | Processes a single entry order through the full FSM hydration lifecycle: guard clauses, state mapping, contract resolution, FSM build, stop/target order linking, FSM registration | 7 |
| `LinkStopOrderIfPresent(FollowerBracketFSM fsm, string entryKey, ref int ordersIndexed)` | Looks up and links the stop order to the FSM; indexes the stop order ID into `_orderIdToFsmKey` if present | 3 |

---

## Parent Method After Extraction

**Remaining logic in `HydrateFSMsFromWorkingOrders` after extraction:**

```csharp
private void HydrateFSMsFromWorkingOrders()
{
    int fsmCreated = 0;
    int ordersIndexed = 0;

    Print("[SIMA] Phase 5 FSM Hydration: Starting entry order pass...");

    foreach (var kvp in entryOrders.ToArray())
        ProcessEntryOrderForFSMHydration(kvp.Key, kvp.Value, ref ordersIndexed, ref fsmCreated);

    Print(string.Format(
        "[SIMA] Phase 5 FSM Hydration (Entry Pass): {0} FSMs created, {1} order IDs indexed.",
        fsmCreated, ordersIndexed));

    int positionFsmCreated = HydrateFromOpenPositions(
        stopOrders, target1Orders, target2Orders, target3Orders, target4Orders, target5Orders,
        ref ordersIndexed, ref fsmCreated);

    Print(string.Format(
        "[SIMA] Phase 5 FSM Hydration (Position Pass): {0} Active FSMs created from open positions.",
        positionFsmCreated));

    Print(string.Format(
        "[SIMA] Phase 5 FSM Hydration: {0} FSMs created, {1} order IDs indexed.",
        fsmCreated, ordersIndexed));
}
```

- **Remaining logic:** counter init + start log + foreach delegating to helper + entry-pass log + position-pass delegation + position-pass log + totals log
- **Projected CYC:** 3 (1 for foreach, +1 for method entry, +1 for base = CYC 3 by standard counting)

---

## max_cyc_projected: 7
## extraction_count: 2

---

## Helper Method Signatures

### `ProcessEntryOrderForFSMHydration`

```csharp
/// <summary>
/// Processes a single entry order through the FSM hydration lifecycle.
/// Applies guard clauses, resolves FSM state, builds FSM, links orders, and registers.
/// Called exclusively from HydrateFSMsFromWorkingOrders entry order pass.
/// </summary>
private void ProcessEntryOrderForFSMHydration(
    string entryKey,
    Order entryOrder,
    ref int ordersIndexed,
    ref int fsmCreated)
```

**Guard clauses (4 early returns):**
1. `if (entryOrder == null) return;`
2. `if (!activePositions.TryGetValue(entryKey, out pi) || !pi.IsFollower) return;`
3. `if (pi.ExecutingAccount == null) return;`
4. `if (_followerBrackets.ContainsKey(entryKey)) return;` — idempotent guard

**Then:** MapOrderStateToFSMState → FindLivePosition (conditional) → ResolveRemainingContracts → BuildFSM → LinkStopOrderIfPresent → 5× LinkTargetOrderToFSM → RegisterFSM

### `LinkStopOrderIfPresent`

```csharp
/// <summary>
/// Links the stop order (if present and valid) to the FSM and indexes its order ID.
/// Single responsibility: stop order association only.
/// </summary>
private void LinkStopOrderIfPresent(
    FollowerBracketFSM fsm,
    string entryKey,
    ref int ordersIndexed)
```

**Logic:** `stopOrders.TryGetValue(entryKey, out stopOrd)` → null check → `fsm.StopOrder = stopOrd` → `IsNullOrEmpty` check → `_orderIdToFsmKey[stopOrd.OrderId] = entryKey` → `ordersIndexed++`

---

## Jane Street Alignment

| Rule | Status | Detail |
|---|---|---|
| CYC<=8 achieved | YES | max_cyc=7; parent=3, helper1=7, helper2=3 — all ≤ 8 |
| Single-responsibility per helper | YES | ProcessEntryOrderForFSMHydration = one-order lifecycle; LinkStopOrderIfPresent = stop-order association only |
| Lock-free/Actor pattern preserved | YES | No `lock()` blocks exist or introduced; `ref int` params maintain actor-serial execution model without boxing |
| Illegal states unrepresentable | YES | Guard clauses enforce valid state before FSM construction; null state from MapOrderStateToFSMState triggers early return (terminal states never reach FSM build path) |
| Zero-allocation hot paths | YES | `ref int` avoids boxing; no new heap collections created in helpers; `ToArray()` already present in original |
| Extract Guard Clauses applied | YES | 4 early returns at top of ProcessEntryOrderForFSMHydration |
| Extract Loop Body applied | YES | Full foreach body extracted to ProcessEntryOrderForFSMHydration |
| FSM Decomposition preserved | YES | FSM transition logic (state mapping, build, register) kept intact — no behavioral change |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 3.5 |
| **Execution Time** | 2026-06-29T01:10:00Z |
| **Wave** | 7 |
| **Phase** | 2 |
| **Method** | HydrateFSMsFromWorkingOrders |
| **jcodemunch tools called** | resolve_repo, get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates, search_symbols |
| **sequential-thinking calls** | 5 |
| **Output** | docs/brain/EPIC-W7-070/02-architecture-plan.md |
