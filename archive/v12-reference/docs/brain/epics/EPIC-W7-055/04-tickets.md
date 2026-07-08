# Phase 4: Ticket Generation — EPIC-W7-055

## Summary

| Field | Value |
|---|---|
| **Epic** | EPIC-W7-055 |
| **Wave** | 7 |
| **Method** | `DrainPhotonQueuesOnShutdown` |
| **Source File** | `src/V12_002.SIMA.Lifecycle.cs` |
| **Original CYC** | 8 |
| **Phase** | 4 — Ticket Generation |
| **ticket_count** | 2 |
| **projected_parent_cyc_after_all** | 1 |

---

## Extraction Tickets

---

### Ticket 1

| Field | Value |
|---|---|
| **ticket_id** | T1 |
| **helper_name** | `DrainPhotonRingOnShutdown` |
| **concern** | Drain the sideband-aware Photon dispatch ring (`_photonDispatchRing`) on SIMA shutdown: resolve pool slot index via ternary bounds check on `_photonSideband`, roll back `ReservedDelta` via `AddExpectedPositionDelta`, clear the dispatch sync barrier via `ClearDispatchSyncPending`, release the pool slot via `_photonPool.ReleaseByIndex`, zero the sideband entry via `_photonSideband[_sbIdx] = default(FleetDispatchSideband)`, and emit the completion log. |
| **lines_to_move** | `src/V12_002.SIMA.Lifecycle.cs` lines 106–124 — the entire `FleetDispatchSlot ringSlot` declaration and `while (_photonDispatchRing != null && _photonDispatchRing.TryDequeue(out ringSlot))` loop body including: `int _sbIdx = ringSlot.PoolSlotIndex;`, ternary `_expectedKey` resolution with compound `_sbIdx >= 0 && _sbIdx < _photonSideband.Length` guard, `if (ringSlot.ReservedDelta != 0 && _expectedKey != null) AddExpectedPositionDelta(...)`, `if (_expectedKey != null) ClearDispatchSyncPending(...)`, outer `if (_sbIdx >= 0)` block containing `_photonPool.ReleaseByIndex(_sbIdx)` and inner `if (_sbIdx < _photonSideband.Length) _photonSideband[_sbIdx] = default(FleetDispatchSideband)`, plus the trailing `Print("[SIMA] Photon ring cleared on shutdown with delta rollback.")` |
| **cyc_reduction** | 7 (removes from parent: while compound guard ×2, ternary bounds compound ×2, delta compound guard ×2, sync if ×1, pool-slot outer if ×1, sideband inner if ×1 — 7 binary decisions eliminated from `DrainPhotonQueuesOnShutdown`) |
| **projected_helper_cyc** | 7 |

**Method Signature:**
```csharp
// V12.23: same-file extraction, private void, zero-allocation, lock-free
private void DrainPhotonRingOnShutdown()
```

**CYC Breakdown for `DrainPhotonRingOnShutdown` (CYC = 1 + 6 = 7):**
| Branch | Count |
|---|---|
| `while` compound guard (`!= null` AND `TryDequeue`) | 2 |
| Ternary `_expectedKey`: `_sbIdx >= 0` AND `_sbIdx < _photonSideband.Length` | 2 |
| `if (ringSlot.ReservedDelta != 0 && _expectedKey != null)` | 1 |
| `if (_expectedKey != null)` — sync clear | 1 |
| `if (_sbIdx >= 0)` — outer pool/sideband block | 1 |
| `if (_sbIdx < _photonSideband.Length)` — inner sideband zero | 1 |
| **Base** | 1 |
| **Total CYC** | **7** |

**V12 Compliance:** CYC ≤ 8 ✅ | lock-free (ConcurrentQueue.TryDequeue, ObjectPool.ReleaseByIndex) ✅ | zero-allocation (FleetDispatchSlot is a struct) ✅ | single-responsibility (ring drain only) ✅

---

### Ticket 2

| Field | Value |
|---|---|
| **ticket_id** | T2 |
| **helper_name** | `DrainLegacyDispatchesOnShutdown` |
| **concern** | Drain the pre-Photon legacy fleet dispatch queue (`_pendingFleetDispatches`) on SIMA shutdown: dequeue each `FleetDispatchRequest`, roll back any non-zero `ReservedDelta` via `AddExpectedPositionDelta`, clear the dispatch sync barrier via `ClearDispatchSyncPending` unconditionally, and emit the completion log. This helper owns the B957/F2 audit-fix drain path exclusively and must not reference Photon ring or sideband fields. |
| **lines_to_move** | `src/V12_002.SIMA.Lifecycle.cs` lines 128–136 — the entire `FleetDispatchRequest ignored` declaration and `while (_pendingFleetDispatches.TryDequeue(out ignored))` loop body including: `if (ignored.ReservedDelta != 0) AddExpectedPositionDelta(ignored.ExpectedKey, -ignored.ReservedDelta)`, `ClearDispatchSyncPending(ignored.ExpectedKey)`, plus the trailing `Print("[SIMA] Dispatch queue cleared on shutdown with delta rollback.")` |
| **cyc_reduction** | 2 (removes from parent: while ×1, delta if ×1 — 2 binary decisions eliminated from `DrainPhotonQueuesOnShutdown`) |
| **projected_helper_cyc** | 3 |

**Method Signature:**
```csharp
// V12.23: same-file extraction, private void, zero-allocation, lock-free
private void DrainLegacyDispatchesOnShutdown()
```

**CYC Breakdown for `DrainLegacyDispatchesOnShutdown` (CYC = 1 + 2 = 3):**
| Branch | Count |
|---|---|
| `while (_pendingFleetDispatches.TryDequeue(out ignored))` | 1 |
| `if (ignored.ReservedDelta != 0)` — conditional delta rollback | 1 |
| **Base** | 1 |
| **Total CYC** | **3** |

**V12 Compliance:** CYC ≤ 8 ✅ | lock-free (ConcurrentQueue.TryDequeue) ✅ | zero-allocation (FleetDispatchRequest is a struct) ✅ | single-responsibility (legacy queue drain only) ✅

---

## Parent Method After All Extractions

```csharp
private void DrainPhotonQueuesOnShutdown()
{
    DrainPhotonRingOnShutdown();
    DrainLegacyDispatchesOnShutdown();
}
```

| Metric | Value |
|---|---|
| **projected_parent_cyc_after_all** | **1** (zero branches — pure sequential call coordinator) |
| Loops | 0 |
| Conditionals | 0 |
| External contract change | None — signature, callers, and side-effect contract unchanged |
| Caller (`ProcessShutdownSIMA`) | Unmodified — continues to call `DrainPhotonQueuesOnShutdown()` at line 107 |

---

## CYC Summary Table

| Method | CYC Before | CYC After | ≤ 8? |
|---|---|---|---|
| `DrainPhotonQueuesOnShutdown` (parent) | 8 | **1** | ✅ |
| `DrainPhotonRingOnShutdown` (Ticket 1 — new) | — | **7** | ✅ |
| `DrainLegacyDispatchesOnShutdown` (Ticket 2 — new) | — | **3** | ✅ |
| **max_cyc_projected** | | **7** | ✅ |

---

## V12.23 Constraint Verification

| Constraint | Status |
|---|---|
| All helpers in same file (`src/V12_002.SIMA.Lifecycle.cs`) | ✅ SATISFIED — partial class pattern requires same-file scope |
| No new cross-file dependencies | ✅ SATISFIED — same-file extraction introduces zero new import edges |
| No new `lock()` blocks | ✅ SATISFIED — both helpers use existing lock-free ConcurrentQueue primitives |
| No new heap allocations | ✅ SATISFIED — FleetDispatchSlot and FleetDispatchRequest are structs; all locals stack-allocated |
| Optional 3rd helper (`ProcessPhotonRingSlot`) | DEFERRED — separate epic per 01-scope-boundary.md |
| DNA audit verdict | PASS (Phase 3) |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Bobcoins Used** | 0.9 |
| **Execution Time** | 2026-06-29T04:15:00Z |
| **Epic** | EPIC-W7-055 |
| **Wave** | 7 |
| **Phase** | 4 — Ticket Generation |
| **ticket_count** | 2 |
| **projected_parent_cyc_after_all** | 1 |
| **max_cyc_projected** | 7 |
| **sequential-thinking calls** | 3 |
| **inputs** | `docs/brain/EPIC-W7-055/02-architecture-plan.md`, `docs/brain/EPIC-W7-055/03-audit-report.md` |
