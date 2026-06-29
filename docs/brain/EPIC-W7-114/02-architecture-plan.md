# Phase 2: Architecture Plan — EPIC-W7-114

**Agent:** v12-phase2-architecture
**Wave:** 7 | **Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:10:00Z
**Input:** docs/brain/EPIC-W7-114/01-scope-boundary.md

---

## Method Under Extraction

| Field | Value |
|---|---|
| **Method** | `ProcessShutdownSIMA` |
| **Source File** | `src/V12_002.SIMA.Lifecycle.cs` |
| **Class** | `V12_002 : Strategy` (partial) |
| **Visibility** | `private void` |
| **Line Range** | 98 – 138 |
| **Original CYC** | **8** (manual McCabe — tooling reported 0 due to partial-class indexing gap) |

### jcodemunch get_context_bundle result

`get_context_bundle` with symbol_id `src/V12_002.SIMA.Lifecycle.cs::V12_002.ProcessShutdownSIMA#method` returned the full 40-line source body (lines 98–138). The method contains three sequential teardown calls (`CancelAllV12GtcOrders` → `StopReaperAudit` → `UnsubscribeFromFleetAccounts`), followed by two inline drain-and-rollback loops (`_photonDispatchRing` and `_pendingFleetDispatches`), and a final Print statement. Initial `get_context_bundle` call with bare name failed due to partial-class tooling gap; resolved via disambiguated symbol ID.

### jcodemunch get_call_hierarchy result

`get_call_hierarchy` (depth=2, direction=both) confirmed: **1 caller** (`ProcessApplySimaState` — line 38, same file), **32 callees** across 6 partial-class files. Key depth-1 callees: `CancelAllV12GtcOrders`, `StopReaperAudit`, `UnsubscribeFromFleetAccounts`, `AddExpectedPositionDelta`, `ClearDispatchSyncPending`, `_photonPool.ReleaseByIndex`. All callees are inferred via AST — no direct callers other than `ProcessApplySimaState`.

### jcodemunch get_dependency_graph result

`get_dependency_graph` (direction=both, depth=1) returned 0 import edges and 0 importer edges for `src/V12_002.SIMA.Lifecycle.cs`. This is expected: all partial-class files compile as a single unit with no explicit `using` cross-references. The file has no standalone importers outside the partial-class compilation.

### jcodemunch get_extraction_candidates result

`get_extraction_candidates` (min_complexity=3, min_callers=1) returned an empty candidates list. This is consistent with the CYC=0 tooling gap — the partial-class symbol is invisible to the complexity ranker. The actual CYC=8 and extraction plan are grounded in Phase 0 manual analysis and confirmed by the full source retrieved via `get_context_bundle`.

---

## Sequential Thinking Summary

`sequentialthinking` chain (5 thoughts) validated the extraction design:

- **Thought 1:** Confirmed actual CYC=8 from hotspots. Identified three structural complexity drivers from the full source body retrieved via get_context_bundle: the ordered teardown triplet (implicit ordering dependency), the photon ring drain loop (CYC +5, 4 inner conditionals), and the fleet dispatch queue drain loop (CYC +2).
- **Thought 2:** Designed three extractions — `TeardownFleetConnections` (CYC=1), `DrainPhotonRingWithRollback` (CYC=5), `DrainPendingDispatchesWithRollback` (CYC=2). Parent reduces to CYC=1. Max across all = 5, all <= 8.
- **Thought 3:** Validated signatures: all `private void`, no parameters (close over class fields via partial-class), same file. V12.23 scope compliance confirmed. Lock-free preserved (ConcurrentQueue.TryDequeue — no lock() blocks).
- **Thought 4:** Risk assessed: extraction preserves ordering safety, does not worsen exception isolation gap (pre-existing risk; deferred to Phase 3 audit). No new illegal states introduced.
- **Thought 5:** Final hypothesis verified — 3 extractions, extraction_count=3, max_cyc_projected=5. Decision: proceed.

---

## Extraction Plan

| Helper Method Name | Signature | Responsibility | Projected CYC |
|---|---|---|---|
| `TeardownFleetConnections` | `private void TeardownFleetConnections()` | Ordered cancel-stop-unsubscribe teardown triplet: `CancelAllV12GtcOrders(false)` → `StopReaperAudit()` → `UnsubscribeFromFleetAccounts()`. Names the safety ordering constraint explicitly. | 1 |
| `DrainPhotonRingWithRollback` | `private void DrainPhotonRingWithRollback()` | Drains `_photonDispatchRing` (FleetDispatchSlot). For each slot: bounds-check sideband index, rollback `ReservedDelta` via `AddExpectedPositionDelta`, clear sync barrier via `ClearDispatchSyncPending`, release pool slot, zero sideband entry. | 5 |
| `DrainPendingDispatchesWithRollback` | `private void DrainPendingDispatchesWithRollback()` | Drains `_pendingFleetDispatches` (FleetDispatchRequest). For each request: rollback `ReservedDelta` if non-zero, clear sync barrier. Lock-free TryDequeue loop. | 2 |

---

## Parent Method After Extraction

```csharp
private void ProcessShutdownSIMA()
{
    TeardownFleetConnections();
    DrainPhotonRingWithRollback();
    DrainPendingDispatchesWithRollback();
    Print("[SIMA LIFECYCLE] SIMA DISABLED -- Reaper stopped, handlers unsubscribed");
}
```

- **Remaining logic:** 4 sequential calls — 3 helper delegates + 1 Print statement
- **Projected CYC:** 1 (straight-line, no branches)

---

## max_cyc_projected: 5
## extraction_count: 3

---

## Jane Street Alignment

| Principle | Status |
|---|---|
| CYC<=8 achieved | YES — parent CYC=1, max helper CYC=5 |
| Single-responsibility per helper | YES — each helper owns exactly one logical phase of teardown |
| Lock-free / Actor pattern preserved | YES — ConcurrentQueue.TryDequeue is lock-free; no lock() blocks added or present |
| Illegal states unrepresentable | YES — extraction does not introduce new partial-disable paths; all helpers called unconditionally in sequence |
| No scope creep (V12.23) | YES — 3 new private helpers in same file; caller unchanged; no cross-file changes |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 3 |
| **Execution Time** | 2026-06-29T01:10:00Z |
| **jcodemunch tools called** | get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **extraction_count** | 3 |
| **max_cyc_projected** | 5 |
