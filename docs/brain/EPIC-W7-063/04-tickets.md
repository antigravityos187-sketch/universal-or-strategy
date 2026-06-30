# EPIC-W7-063 — Phase 4: Ticket Generation

**Agent Name:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29
**Inputs:** docs/brain/EPIC-W7-063/02-architecture-plan.md + docs/brain/EPIC-W7-063/03-audit-report.md

---

## Target Method

| Field | Value |
|---|---|
| **Method Name** | `DrainAllDispatchQueuesOnAbort` |
| **File** | `src/V12_002.SIMA.Fleet.cs` |
| **Lines** | 287–323 (37 lines) |
| **CYC Baseline** | 12 (live MCP index) |
| **CYC Target** | <= 8 |
| **DNA Verdict** | PASS |
| **Extraction Required** | YES |

> **Note on epic header CYC=0:** The CYC listed in the epic task header is a stub/placeholder. The live MCP index value confirmed by `get_symbol_complexity` in Phase 2 is **CYC = 12** (assessment: high). All ticket projections are based on the live-index value of 12.

---

## ticket_count: 2

---

## Ticket 1

| Field | Value |
|---|---|
| **ticket_id** | T1 |
| **helper_name** | `DrainPhotonRingOnAbort` |
| **file** | `src/V12_002.SIMA.Fleet.cs` |
| **concern** | Photon ring sideband-aware teardown — drain `_photonDispatchRing` with per-slot delta rollback, sideband state reset, pool release, and atomic counter decrement |
| **signature** | `[MethodImpl(MethodImplOptions.NoInlining)] private void DrainPhotonRingOnAbort()` |
| **lines_to_move** | Lines 289–308 of `DrainAllDispatchQueuesOnAbort`: the `FleetDispatchSlot abortSlot` local declaration, the `while (_photonDispatchRing != null && _photonDispatchRing.TryDequeue(out abortSlot))` loop and all its body — `TrackPhotonDequeue()`, pool-slot-index lookup, `_expectedKey` conditional computation, `AddExpectedPositionDeltaLocked` guard, `ClearDispatchSyncPending` guard, `_photonPool.ReleaseByIndex` block with sideband reset, and `Interlocked.Decrement(ref _pendingFleetDispatchCount)` |
| **cyc_reduction** | 11 (removes 5 decision branches from parent: `while`-condition compound `&&`, `if(ReservedDelta != 0 && _expectedKey != null)`, `if(_expectedKey != null)`, `if(_sbIdx >= 0)` outer, `if(_sbIdx < _photonSideband.Length)` inner; plus base-count adjustment leaves parent at CYC 1 after both extractions) |
| **projected_helper_cyc** | 6 |

### Decision-point breakdown for `DrainPhotonRingOnAbort` (CYC = 1 + 5 branches = 6)

| # | Branch | Count |
|---|---|---|
| 1 | `while (_photonDispatchRing != null && _photonDispatchRing.TryDequeue(...))` — loop condition with compound `&&` | 1 |
| 2 | `if (abortSlot.ReservedDelta != 0 && _expectedKey != null)` | 1 |
| 3 | `if (_expectedKey != null)` — guard for `ClearDispatchSyncPending` | 1 |
| 4 | `if (_sbIdx >= 0)` — outer pool-release guard | 1 |
| 5 | `if (_sbIdx < _photonSideband.Length)` — inner sideband reset guard | 1 |
| — | Base | 1 |
| **Total** | | **6** |

### Jane Street Compliance

| Rule | Status |
|---|---|
| `[MethodImpl(MethodImplOptions.NoInlining)]` — cold abort path | REQUIRED |
| Zero allocations — struct `FleetDispatchSlot` accessed by value | PASS |
| No `lock()` blocks — `Interlocked.Decrement` only | PASS |
| CYC <= 8 | PASS (6) |
| Single responsibility: photon ring teardown only | PASS |

---

## Ticket 2

| Field | Value |
|---|---|
| **ticket_id** | T2 |
| **helper_name** | `DrainLegacyDispatchQueueOnAbort` |
| **file** | `src/V12_002.SIMA.Fleet.cs` |
| **concern** | Legacy ConcurrentQueue teardown — drain `_pendingFleetDispatches` with per-request delta rollback, sync pending clear, and atomic counter decrement |
| **signature** | `[MethodImpl(MethodImplOptions.NoInlining)] private void DrainLegacyDispatchQueueOnAbort()` |
| **lines_to_move** | Lines 310–317 of `DrainAllDispatchQueuesOnAbort`: the `FleetDispatchRequest stale` local declaration, the `while (_pendingFleetDispatches.TryDequeue(out stale))` loop and all its body — `if (stale.ReservedDelta != 0) AddExpectedPositionDeltaLocked(...)`, unconditional `ClearDispatchSyncPending(stale.ExpectedKey)`, and `Interlocked.Decrement(ref _pendingFleetDispatchCount)` |
| **cyc_reduction** | 2 (removes `while`-condition branch and `if(stale.ReservedDelta != 0)` branch from parent) |
| **projected_helper_cyc** | 3 |

### Decision-point breakdown for `DrainLegacyDispatchQueueOnAbort` (CYC = 1 + 2 branches = 3)

| # | Branch | Count |
|---|---|---|
| 1 | `while (_pendingFleetDispatches.TryDequeue(out stale))` — loop condition | 1 |
| 2 | `if (stale.ReservedDelta != 0)` — delta rollback guard | 1 |
| — | Base | 1 |
| **Total** | | **3** |

### Jane Street Compliance

| Rule | Status |
|---|---|
| `[MethodImpl(MethodImplOptions.NoInlining)]` — cold abort path | REQUIRED |
| Zero allocations — struct `FleetDispatchRequest` accessed by value | PASS |
| No `lock()` blocks — `Interlocked.Decrement` only | PASS |
| CYC <= 8 | PASS (3) |
| Single responsibility: legacy queue teardown only | PASS |

---

## Parent Method After All Extractions

```csharp
/// <summary>
/// V12 Phase 7 [T13]: Drain both Photon ring and legacy queue when SIMA disabled or flatten running.
/// Performs sideband-aware delta rollback and pool release for all pending dispatches.
/// </summary>
private void DrainAllDispatchQueuesOnAbort()
{
    DrainPhotonRingOnAbort();
    DrainLegacyDispatchQueueOnAbort();

    // REAPER-EXPANSION P0 FIX: Reset circuit breaker after drain completes
    // After flatten drains both queues to zero, CB must reset to accept future dispatches
    int finalCount = Volatile.Read(ref _pendingFleetDispatchCount);
    TryResetCircuitBreakerIfBelow(finalCount);
}
```

**projected_parent_cyc_after_all: 1**
(0 decision branches + base = 1; pure orchestrator with Volatile.Read memory barrier and circuit breaker call retained)

---

## CYC Summary

| Method | Baseline | Projected | Delta | Compliant (<=8) |
|---|---|---|---|---|
| `DrainAllDispatchQueuesOnAbort` | 12 | **1** | -11 | ✅ YES |
| `DrainPhotonRingOnAbort` | N/A (new) | **6** | — | ✅ YES |
| `DrainLegacyDispatchQueueOnAbort` | N/A (new) | **3** | — | ✅ YES |
| **max_cyc_projected** | — | **6** | — | ✅ YES |

---

## Scope Constraints (from Architecture Plan + Audit)

| Constraint | Status |
|---|---|
| Helpers private, same file only | PASS |
| `PumpFleetDispatch` caller — DO NOT MODIFY | PASS |
| `ProcessFleetSlot` (depth-2 caller) — DO NOT MODIFY | PASS |
| `VerifyPhotonSlotIntegrity` (depth-2 caller) — DO NOT MODIFY | PASS |
| Signature of `DrainAllDispatchQueuesOnAbort` unchanged | PASS |
| No cross-file refactoring | PASS |
| No circular dependency introduction | PASS (cycle_count=0 confirmed) |
| `Volatile.Read` memory barrier retained in parent | REQUIRED — retained |
| `TryResetCircuitBreakerIfBelow` retained in parent | REQUIRED — retained |
| Test framework: xUnit `[Fact]` + `Assert.Equal()` only | REQUIRED |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Wave** | 7 |
| **Phase** | 4 |
| **Epic** | EPIC-W7-063 |
| **ticket_count** | 2 |
| **projected_parent_cyc_after_all** | 1 |
| **max_cyc_projected** | 6 |
| **DNA Verdict (Phase 3)** | PASS |
| **Bobcoins Used** | 7 |
| **Execution Time** | ~35s |
