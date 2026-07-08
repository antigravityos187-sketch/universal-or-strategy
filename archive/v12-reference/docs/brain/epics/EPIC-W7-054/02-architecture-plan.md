# Phase 2: Architecture Plan — EPIC-W7-054

**Agent:** v12-phase2-architecture
**Wave:** 7 | **Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T02:10:00Z
**Input:** docs/brain/EPIC-W7-054/01-scope-boundary.md

---

## Method Under Extraction

- **Method:** `DrainAllDispatchQueuesOnAbort`
- **Source File:** `src/V12_002.SIMA.Fleet.cs`
- **Lines:** 287–323
- **Original CYC:** 20
- **Signature:** `private void DrainAllDispatchQueuesOnAbort()`

### jcodemunch `get_context_bundle` result

Symbol resolved at `src/V12_002.SIMA.Fleet.cs::V12_002.DrainAllDispatchQueuesOnAbort#method` (lines 287–323).
The method drains two distinct dispatch queues on SIMA abort: (1) the Photon ring buffer
(`_photonDispatchRing`) with sideband-aware delta rollback and pool release per slot, and (2) the
legacy `ConcurrentQueue<FleetDispatchRequest>` (`_pendingFleetDispatches`) with simpler delta
rollback and sync-clear. A circuit-breaker reset follows both drains. All state mutations use
`Interlocked.Decrement` and `Volatile.Read` — no lock() blocks present.

### jcodemunch `get_call_hierarchy` result

**Callers (depth 1-2):**
- `PumpFleetDispatch` (depth 1, `src/V12_002.SIMA.Fleet.cs:233`) — direct caller
- `ProcessFleetSlot` (depth 2, `src/V12_002.SIMA.Fleet.cs:44`) — calls via PumpFleetDispatch
- `VerifyPhotonSlotIntegrity` (depth 2, `src/V12_002.SIMA.Fleet.cs:329`) — calls via PumpFleetDispatch

**Callees (depth 1):**
- `TrackPhotonDequeue` (`src/V12_002.Telemetry.cs:161`)
- `AddExpectedPositionDeltaLocked` (`src/V12_002.SIMA.cs:88`)
- `ClearDispatchSyncPending` (`src/V12_002.SIMA.cs:179`)
- `TryResetCircuitBreakerIfBelow` (`src/V12_002.SIMA.Fleet.cs:420`)
- Fields: `_photonDispatchRing`, `_photonPool`, `_pendingFleetDispatches`, `_pendingFleetDispatchCount`

All callers call the method without depending on its internal structure — signature is unchanged.

### jcodemunch `get_dependency_graph` result

File `src/V12_002.SIMA.Fleet.cs` has 0 recorded import edges (partial class file — dependencies
resolved at partial-class merge time). No cross-file impact from internal extraction.

### jcodemunch `get_extraction_candidates` result

`get_extraction_candidates` returned 0 candidates (min_callers=1, min_complexity=3). This is expected:
the index stores pre-extraction complexity. The architecture plan below identifies the extraction
targets via direct source analysis from `get_context_bundle`.

---

## Sequential Thinking Summary

**Final Thought (5/5 — APPROVED):**

Four helpers extracted from `DrainAllDispatchQueuesOnAbort` (CYC 20 → CYC 1):

1. `ResolveSidebandKey(int sbIdx)` — isolates the three-condition ternary sideband key lookup,
   eliminating 3 CYC from the slot processor. CYC 3.

2. `DrainPhotonDispatchSlot(FleetDispatchSlot abortSlot)` — processes one dequeued photon slot:
   resolves key, conditionally rolls back delta, clears sync pending, releases pool slot, resets
   sideband entry, decrements counter. CYC 6.

3. `DrainPhotonDispatchRing()` — drives the photon while-loop, calls `TrackPhotonDequeue()` and
   `DrainPhotonDispatchSlot(abortSlot)` per iteration. CYC 3.

4. `DrainLegacyFleetDispatches()` — drives the legacy ConcurrentQueue while-loop with delta
   rollback guard, sync clear, and counter decrement per item. CYC 3.

**Parent after extraction:** Sequential calls to (3), (4), and `TryResetCircuitBreakerIfBelow`.
No branches. CYC 1. All helpers and parent satisfy CYC ≤ 8. Jane Street verdict: APPROVED.

---

## Extraction Plan

| Helper Method Name | Responsibility | Projected CYC |
|---|---|---|
| `ResolveSidebandKey(int sbIdx)` | Bounds-check sbIdx against `_photonSideband.Length`; return `ExpectedKey` or null. Extracts the 3-condition ternary. | 3 |
| `DrainPhotonDispatchSlot(FleetDispatchSlot abortSlot)` | Process one dequeued photon slot: delta rollback via `AddExpectedPositionDeltaLocked`, `ClearDispatchSyncPending`, pool `ReleaseByIndex`, sideband reset to `default`, `Interlocked.Decrement`. Calls `ResolveSidebandKey`. | 6 |
| `DrainPhotonDispatchRing()` | While-loop over `_photonDispatchRing.TryDequeue`; calls `TrackPhotonDequeue()` + `DrainPhotonDispatchSlot(abortSlot)` per iteration. Null-guards ring reference. | 3 |
| `DrainLegacyFleetDispatches()` | While-loop over `_pendingFleetDispatches.TryDequeue`; conditionally rolls back `ReservedDelta`, calls `ClearDispatchSyncPending`, `Interlocked.Decrement`. | 3 |

---

## Parent Method After Extraction

**Remaining logic:**

```csharp
private void DrainAllDispatchQueuesOnAbort()
{
    DrainPhotonDispatchRing();
    DrainLegacyFleetDispatches();
    int finalCount = Volatile.Read(ref _pendingFleetDispatchCount);
    TryResetCircuitBreakerIfBelow(finalCount);
}
```

- No conditional branches at parent level — purely sequential delegation
- **Projected CYC: 1**

---

## max_cyc_projected: 6
## extraction_count: 4

---

## Jane Street Alignment

| Rule | Status |
|---|---|
| CYC<=8 achieved | YES — max projected CYC is 6 (`DrainPhotonDispatchSlot`); parent CYC is 1 |
| Single-responsibility per helper | YES — each helper does exactly one thing (key resolution, slot drain, photon loop, legacy loop) |
| Lock-free/Actor pattern preserved | YES — no lock() introduced; existing `Interlocked.Decrement` and `Volatile.Read` patterns retained in extracted helpers |
| Illegal states unrepresentable | YES — `ResolveSidebandKey` returns null (not throws) for out-of-bounds sbIdx; callers must check null before use, enforced by existing null-guard pattern already present in original code |
| Zero-allocation hot paths | YES — `FleetDispatchSlot` passed by value (struct); `ResolveSidebandKey` returns existing string reference (no new string alloc) |
| Extract guard clauses | YES — bounds checks in `ResolveSidebandKey` use early return pattern |
| Extract loop body | YES — photon and legacy loop bodies extracted to `DrainPhotonDispatchSlot` and `DrainLegacyFleetDispatches` respectively |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Epic** | EPIC-W7-054 |
| **Wave** | 7 |
| **Phase** | 2 |
| **Method** | `DrainAllDispatchQueuesOnAbort` |
| **Source File** | `src/V12_002.SIMA.Fleet.cs` |
| **Original CYC** | 20 |
| **Max Projected CYC** | 6 |
| **Extraction Count** | 4 |
| **Bobcoins Used** | 1.5 |
| **Execution Time** | 2026-06-29T02:10:00Z |
| **jcodemunch tools called** | get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **Output** | `docs/brain/EPIC-W7-054/02-architecture-plan.md` |
