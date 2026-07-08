# Phase 2: Architecture Plan — EPIC-W7-108

**Agent:** v12-phase2-architecture
**Wave:** 7 | **Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:15:00Z
**Input:** docs/brain/EPIC-W7-108/01-scope-boundary.md

---

## Method Under Extraction

- **Method:** `DrainPhotonQueuesOnShutdown`
- **Source File:** `src/V12_002.SIMA.Lifecycle.cs`
- **Original CYC:** ≈8 (inline body in `ProcessShutdownSIMA`, lines 104–136; confirmed via static analysis in `00-hotspots.md` — method does not yet exist as a standalone symbol; the inline drain section has CYC≈8 per branch-count analysis)
- **Status:** Method is INLINED inside `ProcessShutdownSIMA`. Extraction of `DrainPhotonQueuesOnShutdown` is Phase 5's first action before complexity reduction proceeds.

---

### jcodemunch `get_context_bundle` result

Symbol `DrainPhotonQueuesOnShutdown` not found as a standalone symbol in `src/` index — consistent with hotspot finding that the method is inlined and not yet extracted. The vm-backup copy at `src-vm-backup/V12_002.SIMA.Lifecycle.cs:165` is indexed and confirms the expected shape of the future extracted method.

---

### jcodemunch `get_call_hierarchy` result (depth=2, both directions)

- **Symbol resolved:** `src-vm-backup/V12_002.SIMA.Lifecycle.cs::V12_002.DrainPhotonQueuesOnShutdown` (line 165)
- **Callers (depth=1):** `ProcessShutdownSIMA` (line 144, same file) — already the direct parent of the inline drain blocks
- **Callers (depth=2):** `ProcessApplySimaState` (line 70, same file) — calls ProcessShutdownSIMA
- **Callees (depth=1):** `_photonDispatchRing`, `AddExpectedPositionDelta`, `ClearDispatchSyncPending`, `_photonPool`, `_pendingFleetDispatches`
- **Callees (depth=2):** `AddExpectedPositionDeltaLocked`, `_dispatchSyncPendingExpKeys`
- **jcodemunch `get_dependency_graph` result:** `src/V12_002.SIMA.Lifecycle.cs` has 0 explicit import edges in the index (partial-class file, intra-assembly only). No cross-file import coupling to manage during extraction.

---

### jcodemunch `get_extraction_candidates` result

No candidates returned for `src/V12_002.SIMA.Lifecycle.cs` at min_complexity=3, min_callers=1. Consistent with the fact that `DrainPhotonQueuesOnShutdown` is not yet an indexed symbol — its inline body inside `ProcessShutdownSIMA` is not visible as a standalone extraction candidate. The extraction plan below is derived from direct source analysis.

---

## Sequential Thinking Summary

**sequentialthinking chain (5 thoughts) — final verdict:**

The inline drain blocks in `ProcessShutdownSIMA` (lines 104–136 of `src/V12_002.SIMA.Lifecycle.cs`) shall be extracted into `DrainPhotonQueuesOnShutdown` as a new private method. `DrainPhotonQueuesOnShutdown` shall then delegate to 3 private helpers.

Extraction hierarchy:
```
ProcessShutdownSIMA
  └── calls DrainPhotonQueuesOnShutdown()      [NEW private method — orchestrates]
        ├── calls DrainPhotonRing()            [NEW private helper — ring loop]
        │     └── calls ReleasePhotonSlot(FleetDispatchSlot slot)  [NEW private helper — per-slot]
        └── calls DrainLegacyDispatchQueue()   [NEW private helper — legacy queue loop]
```

All projected CYC values are <= 8 (max=6). Jane Street alignment: full compliance. Lock-free paths preserved. Duplicate-epic coordination with EPIC-W7-055 required before Phase 5 execution.

---

## Extraction Plan

| Helper Method Name | Responsibility | Projected CYC |
|---|---|---|
| `DrainPhotonQueuesOnShutdown()` | **New top-level private method** — extracted from `ProcessShutdownSIMA`. Sequential orchestrator: calls `DrainPhotonRing()` then `DrainLegacyDispatchQueue()`. No branches. | **1** |
| `DrainPhotonRing()` | Iterates `_photonDispatchRing` via `TryDequeue` loop; for each dequeued `FleetDispatchSlot`, delegates to `ReleasePhotonSlot(slot)`. Owns the ring-loop guard and the `Print` log call. | **2** |
| `ReleasePhotonSlot(FleetDispatchSlot slot)` | Processes ONE dequeued slot: computes `_sbIdx` and `_expectedKey` from slot fields, rolls back `ReservedDelta` via `AddExpectedPositionDelta`, clears `ClearDispatchSyncPending`, releases pool index via `_photonPool.ReleaseByIndex`, zeroes `_photonSideband[_sbIdx]`. All guard clauses expressed as early returns. | **6** |
| `DrainLegacyDispatchQueue()` | Iterates `_pendingFleetDispatches` via `TryDequeue` loop; for each `FleetDispatchRequest`, rolls back `ReservedDelta` and calls `ClearDispatchSyncPending`. Owns the `Print` log call. | **3** |

---

## Parent Method After Extraction

- **`ProcessShutdownSIMA` remaining logic:** Calls `CancelAllV12GtcOrders`, `StopReaperAudit`, `UnsubscribeFromFleetAccounts`, then `DrainPhotonQueuesOnShutdown()`, then final `Print`. All inline drain blocks replaced by single delegating call.
- **`ProcessShutdownSIMA` CYC reduction:** The drain section contributed ≈8 McCabe predicates to `ProcessShutdownSIMA`. After extraction, those predicates are removed from the parent, reducing `ProcessShutdownSIMA` complexity by ≈8.
- **`DrainPhotonQueuesOnShutdown` projected CYC:** **1** (no branches — pure sequential delegation)

---

## Implementation Sequence (Phase 5 Engineer)

Execute in this order to maintain build integrity at each step:

1. **Step 1:** Extract `DrainPhotonQueuesOnShutdown()` — move both drain blocks from `ProcessShutdownSIMA` into a new `private void DrainPhotonQueuesOnShutdown()`. Replace inline blocks with `DrainPhotonQueuesOnShutdown();`. ✅ Build must pass.
2. **Step 2:** Extract `DrainPhotonRing()` — move the `while (_photonDispatchRing ...)` block into `private void DrainPhotonRing()`. Replace in `DrainPhotonQueuesOnShutdown` with `DrainPhotonRing();`. ✅ Build must pass.
3. **Step 3:** Extract `ReleasePhotonSlot(FleetDispatchSlot slot)` — move the per-slot logic from `DrainPhotonRing` into `private void ReleasePhotonSlot(FleetDispatchSlot slot)`. Call `ReleasePhotonSlot(ringSlot)` inside the loop. ✅ Build must pass.
4. **Step 4:** Extract `DrainLegacyDispatchQueue()` — move the `while (_pendingFleetDispatches ...)` block into `private void DrainLegacyDispatchQueue()`. Replace with `DrainLegacyDispatchQueue();`. ✅ Build must pass.
5. **Step 5:** Run `dotnet csharpier format src/` then `powershell -File .\scripts\pre_push_validation.ps1 -Fast`. Verify all projected CYC values.

> ⚠️ **Duplicate epic check REQUIRED before Phase 5 execution:** EPIC-W7-055 targets the identical inline body. Confirm with Wave 7 coordinator which ticket is active. Do NOT execute both W7-055 and W7-108 — this would produce conflicting commits.

---

## max_cyc_projected: 6
## extraction_count: 3

---

## Jane Street Alignment

| Rule | Status | Evidence |
|---|---|---|
| CYC<=8 achieved | **YES** | max CYC = 6 (`ReleasePhotonSlot`); all 4 methods <= 8 |
| Single-responsibility per helper | **YES** | Each helper has one named concern: ring-loop / per-slot / legacy-loop |
| Lock-free/Actor pattern preserved | **YES** | All operations use `TryDequeue` (lock-free); no `lock()` blocks introduced or retained |
| Illegal states unrepresentable | **YES** | `_sbIdx` bounds checks encapsulated in `ReleasePhotonSlot` — callers cannot bypass the guard |
| Zero-allocation hot paths | **YES** | `FleetDispatchSlot` is a struct (value type); no heap allocations from extraction |
| Guard clauses as early returns | **YES** | `ReleasePhotonSlot` uses early-return guard for `_sbIdx < 0` and null key checks |
| Extract loop body pattern | **YES** | `DrainPhotonRing` delegates per-slot work to `ReleasePhotonSlot(slot)` |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 2.1 |
| **Execution Time** | 2026-06-29T01:15:00Z |
| **Wave** | 7 |
| **Phase** | 2 — Architecture Planning |
| **Epic** | EPIC-W7-108 |
| **Method** | `DrainPhotonQueuesOnShutdown` (inline in `ProcessShutdownSIMA`) |
| **Source File** | `src/V12_002.SIMA.Lifecycle.cs` |
| **jcodemunch tools called** | get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **extraction_count** | 3 |
| **max_cyc_projected** | 6 |
| **Output** | docs/brain/EPIC-W7-108/02-architecture-plan.md |
