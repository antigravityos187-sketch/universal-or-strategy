# Phase 2: Architecture Plan — EPIC-W7-113

**Agent:** v12-phase2-architecture
**Wave:** 7 | **Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:20:00Z
**Input:** docs/brain/EPIC-W7-113/01-scope-boundary.md

---

## Method Under Extraction

- **Method:** `HydrateFSMsFromWorkingOrders`
- **Source File:** `src/V12_002.SIMA.Lifecycle.cs`
- **Lines:** 787–891 (104 lines)
- **Original CYC:** 12 (manual count — tool-reported value is 0 due to indexer gap on private partial-class method)
- **Target CYC:** ≤ 8 (Jane Street strict standard)

### jcodemunch get_context_bundle result

jcodemunch `get_context_bundle` returned `Symbol(s) not found` for the bare name `HydrateFSMsFromWorkingOrders` (known indexer gap for private partial-class methods). Fallback to jcodemunch `search_symbols` resolved the symbol at `src/V12_002.SIMA.Lifecycle.cs::V12_002.HydrateFSMsFromWorkingOrders#method` (line 787). Full source was retrieved via `get_symbol_source`. Key findings:

- **Signature:** `private void HydrateFSMsFromWorkingOrders()`
- **Docstring:** "Phase 5: Rebuilds `_followerBrackets` and `_orderIdToFsmKey` from already-adopted working orders. Called from `HydrateWorkingOrdersFromBroker()` before the adoption-complete gate is set. Idempotent — safe to call on every reconnect."
- **Structure confirmed:** Init counters → Entry Order Pass (foreach with 5 guard-continues + FSM construction pipeline) → Print telemetry → Position Pass (`HydrateFromOpenPositions`) → Print telemetry ×2.
- **Side effects:** Mutates `_followerBrackets` (ConcurrentDictionary) and `_orderIdToFsmKey` (ConcurrentDictionary). No lock() blocks present.

### jcodemunch get_call_hierarchy result

jcodemunch `get_call_hierarchy` (depth=2, direction=both):

- **Direct callers (depth 1):** `HydrateWorkingOrdersFromBroker` (line 309, same file) — 1 caller confirmed.
- **Transitive callers (depth 2):** `EnumerateApexAccounts` (line 140, same file).
- **Direct callees (depth 1):** `MapOrderStateToFSMState`, `FindLivePosition`, `ResolveRemainingContracts`, `BuildFSM`, `LinkTargetOrderToFSM`, `RegisterFSM`, `HydrateFromOpenPositions`, plus field accesses to `entryOrders`, `activePositions`, `stopOrders`, `_followerBrackets`, `target1Orders`–`target5Orders`.
- **Dependency chain:** `OnStateChange → ApplySimaState → HydrateWorkingOrdersFromBroker → HydrateFSMsFromWorkingOrders` (startup/reconnect cold path only — not hot path).

### jcodemunch get_dependency_graph result

jcodemunch `get_dependency_graph` for `src/V12_002.SIMA.Lifecycle.cs` (direction=both, depth=1): returned 1 node, 0 edges. The indexer found no explicit `using`/`import` edges for this C# partial-class file. Cross-file consumers of `_followerBrackets` (the primary mutated state) were established from Phase 0 hotspot analysis: `V12_002.Symmetry.BracketFSM.cs`, `V12_002.Symmetry.Follower.cs`, `V12_002.SIMA.Fleet.cs`, `V12_002.SIMA.Dispatch.cs`, `V12_002.SIMA.Shadow.cs`, `V12_002.UI.IPC.Commands.Fleet.cs`.

### jcodemunch get_extraction_candidates result

jcodemunch `get_extraction_candidates` (min_complexity=3, min_callers=1) returned no candidates — consistent with the CYC=0 indexer gap noted above. Extraction design is based on the manually-verified CYC=12 count documented in Phase 0.

---

## Sequential Thinking Summary

The sequentialthinking chain (5 thoughts) validated the following architecture:

**Thought 1** identified the 12 individual branch-points contributing to CYC=12 and confirmed the method's two-pass structure (entry-order pass + position pass via `HydrateFromOpenPositions`).

**Thought 2** evaluated the optimal extraction boundaries. Extracting `TryGetEntryPassCandidate` removes 5 guard-continue branches (B1–B5) from the loop body. Extracting `LinkStopOrderToFSM` removes 3 branches (B8–B10) for the stop-order linking block. Both reductions are achievable without invalidating the `state==null` (B6) and `state.Value==Active` (B7) guards which belong to the FSM-construction logic. Extracting `RunEntryOrderPass` further reduces the parent to a pure orchestrator (CYC=1) and creates a symmetric peer to `HydrateFromOpenPositions`.

**Thought 3** finalized all three helper method signatures and verified that each helper independently satisfies CYC ≤ 8. The `ref` parameter pattern for counters was confirmed as the correct zero-allocation approach consistent with existing callee patterns (`RegisterFSM`, `LinkTargetOrderToFSM`).

**Thought 4** confirmed all Jane Street alignment criteria: CYC ≤ 8 everywhere, single-responsibility per helper, no lock() introduction, guard consolidation makes illegal states (null order, master-account entry, duplicate FSM) structurally unrepresentable at call sites, zero-allocation compliance maintained.

**Thought 5 (final verdict):** Extract 3 private helpers. `max_cyc_projected = 6` (`TryGetEntryPassCandidate`). `extraction_count = 3`. Parent reduces to CYC=1. All Jane Street constraints satisfied.

---

## Extraction Plan

| Helper Method Name | Responsibility | Projected CYC |
|---|---|---|
| `TryGetEntryPassCandidate(string entryKey, Order entryOrder, out PositionInfo pi)` | Validates eligibility for FSM creation: checks null order, follower-only account (via `activePositions`), non-null `ExecutingAccount`, idempotent `_followerBrackets` key guard. Returns `false` on any failing guard, `true` with populated `pi` when all preconditions pass. Collapses 5 guard-continue branches into a single boolean precondition at the call site. | **6** |
| `LinkStopOrderToFSM(ref FollowerBracketFSM fsm, string entryKey, ref int ordersIndexed)` | Links the stop order for `entryKey` to the FSM: `stopOrders.TryGetValue` lookup, null check, `fsm.StopOrder` assignment, and `_orderIdToFsmKey` registration when `OrderId` is non-empty. Mirrors the existing `LinkTargetOrderToFSM` pattern for naming consistency. | **3** |
| `RunEntryOrderPass(ref int ordersIndexed, ref int fsmCreated)` | Orchestrates the complete entry-order foreach loop: iterates `entryOrders.ToArray()`, calls `TryGetEntryPassCandidate`, calls `MapOrderStateToFSMState`, skips terminal states, conditionally calls `FindLivePosition`, calls `ResolveRemainingContracts`, calls `BuildFSM`, calls `LinkStopOrderToFSM`, calls `LinkTargetOrderToFSM` ×5, calls `RegisterFSM`. Symmetric structural peer to `HydrateFromOpenPositions`. | **4** |

---

## Parent Method After Extraction

**Remaining logic:**

```
HydrateFSMsFromWorkingOrders():
  1. int fsmCreated = 0; int ordersIndexed = 0;
  2. Print("[SIMA] Phase 5 FSM Hydration: Starting entry order pass...");
  3. RunEntryOrderPass(ref ordersIndexed, ref fsmCreated);
  4. Print("[SIMA] Phase 5 FSM Hydration (Entry Pass): {fsmCreated} FSMs created, {ordersIndexed} order IDs indexed.");
  5. int positionFsmCreated = HydrateFromOpenPositions(..., ref ordersIndexed, ref fsmCreated);
  6. Print("[SIMA] Phase 5 FSM Hydration (Position Pass): {positionFsmCreated} Active FSMs created from open positions.");
  7. Print("[SIMA] Phase 5 FSM Hydration: {fsmCreated} FSMs created, {ordersIndexed} order IDs indexed.");
```

Pure orchestrator — no decision branches. All branching delegated to extracted helpers and pre-existing callees.

- **Projected CYC:** **1**

---

## max_cyc_projected: 6
## extraction_count: 3

---

## Jane Street Alignment

| Rule | Status |
|---|---|
| CYC ≤ 8 achieved | **YES** — TryGetEntryPassCandidate=6, LinkStopOrderToFSM=3, RunEntryOrderPass=4, parent=1; max=6 ≤ 8 |
| Single-responsibility per helper | **YES** — each helper does exactly one thing: (1) eligibility validation, (2) stop-order linking, (3) entry-pass orchestration |
| Lock-free / Actor pattern preserved | **YES** — no `lock()` blocks in source; `ConcurrentDictionary` ops unchanged; `ref` counters are single-threaded local scope in startup cold path |
| Illegal states unrepresentable | **YES** — `TryGetEntryPassCandidate` consolidates 5 precondition guards; the FSM construction pipeline in `RunEntryOrderPass` can only be reached when all invariants are confirmed, making null-order, master-account, and duplicate-key FSM creation structurally impossible |
| Zero-allocation hot paths | **YES** — startup cold path (not latency-critical); `out` / `ref` parameters avoid heap allocs in extracted helpers; no new object creation |
| Extract guard clauses | **YES** — `TryGetEntryPassCandidate` replaces 5 guard-continue branches with a single boolean gate |
| Extract loop body | **YES** — `RunEntryOrderPass` extracts the foreach body as a named method with clear responsibility |
| Extract to named helper methods | **YES** — `LinkStopOrderToFSM` mirrors the existing `LinkTargetOrderToFSM` naming convention |
| FSM decomposition | **YES** — two-pass structure (entry + position) made explicit and symmetric via `RunEntryOrderPass` / `HydrateFromOpenPositions` peers |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 12 |
| **Execution Time** | 2026-06-29T01:20:00Z |
| **Wave** | 7 |
| **Phase** | 2 |
| **Method** | `HydrateFSMsFromWorkingOrders` |
| **Output** | `docs/brain/EPIC-W7-113/02-architecture-plan.md` |
| **jcodemunch tools called** | get_context_bundle, search_symbols (fallback), get_symbol_source, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |

---

*Generated: Phase 2 — Architecture Planning | EPIC-W7-113 | Wave 7*
