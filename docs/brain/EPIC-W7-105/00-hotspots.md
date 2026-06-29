# Phase 0: Hotspot Analysis — EPIC-W7-105

## Method Metadata

| Field       | Value                             |
|-------------|-----------------------------------|
| Method Name | `DrainAllDispatchQueuesOnAbort`   |
| CYC Score   | **12**                            |
| File Path   | `src/V12_002.SIMA.Fleet.cs`       |
| Line Range  | 287–323                           |
| Caller      | `PumpFleetDispatch` (line 238)    |
| Phase Tag   | V12 Phase 7 [T13]                 |

---

## Blast Radius Summary

The method is a **single call-site drain** invoked exclusively by `PumpFleetDispatch` (line 238) when `isFlattenRunning == true` or `!EnableSIMA`. It touches three shared mutable subsystems:

| Subsystem | Fields Mutated | Thread Safety |
|-----------|---------------|---------------|
| Photon ring | `_photonDispatchRing` (dequeue), `_photonSideband[]` (reset), `_photonPool` (release) | SPSC ring; pool is internally atomic |
| Legacy queue | `_pendingFleetDispatches` (dequeue) | `ConcurrentQueue<T>` — safe |
| Counter / CB | `_pendingFleetDispatchCount` (Interlocked.Decrement), `_reaperCircuitBreakerTripped` (via `TryResetCircuitBreakerIfBelow`) | Interlocked / Volatile — safe |

**Callee surface** (methods called from within `DrainAllDispatchQueuesOnAbort`):
- `TrackPhotonDequeue()` — telemetry, no state side-effect
- `AddExpectedPositionDeltaLocked(key, delta)` — mutates expected-position accounting dict
- `ClearDispatchSyncPending(key)` — mutates `_dispatchSyncPendingExpKeys`
- `_photonPool.ReleaseByIndex(idx)` — frees pool slot
- `Interlocked.Decrement(ref _pendingFleetDispatchCount)` — atomic counter
- `TryResetCircuitBreakerIfBelow(count)` — may CAS `_reaperCircuitBreakerTripped`

**Parallel drain risk:** An identical drain loop exists in `V12_002.SIMA.Lifecycle.cs` (lines 107–133). Both loops iterate the same ring and queue. If both execute concurrently (e.g. `OnTermination` races with `PumpFleetDispatch`), double-decrement of `_pendingFleetDispatchCount` and double-release of pool slots are possible. This is the primary blast-radius concern.

---

## Top 3 Complexity Drivers

### 1 — Dual-path while-loop nesting with compound guard (CYC +5)

```csharp
// Lines 291–308
while (_photonDispatchRing != null && _photonDispatchRing.TryDequeue(out abortSlot))
{
    int _sbIdx = abortSlot.PoolSlotIndex;
    string _expectedKey =
        (_sbIdx >= 0 && _sbIdx < _photonSideband.Length)   // +2: ternary with &&
            ? _photonSideband[_sbIdx].ExpectedKey : null;
    if (abortSlot.ReservedDelta != 0 && _expectedKey != null)  // +2: compound if
        AddExpectedPositionDeltaLocked(_expectedKey, -abortSlot.ReservedDelta);
    if (_expectedKey != null)                                   // +1
        ClearDispatchSyncPending(_expectedKey);
    if (_sbIdx >= 0)                                            // +1
    {
        _photonPool.ReleaseByIndex(_sbIdx);
        if (_sbIdx < _photonSideband.Length)                   // +1
            _photonSideband[_sbIdx] = default(FleetDispatchSideband);
    }
    Interlocked.Decrement(ref _pendingFleetDispatchCount);
}
```

**Driver:** The Photon ring loop body contains 3 nested `if` checks + 1 ternary with `&&`, all required per-iteration. Extracting the per-slot rollback logic into a helper (`DrainPhotonSlotOnAbort`) would eliminate 5 decision points from this method.

---

### 2 — Legacy queue while-loop with conditional rollback (CYC +2)

```csharp
// Lines 311–317
while (_pendingFleetDispatches.TryDequeue(out stale))       // +1
{
    if (stale.ReservedDelta != 0)                           // +1
        AddExpectedPositionDeltaLocked(stale.ExpectedKey, -stale.ReservedDelta);
    ClearDispatchSyncPending(stale.ExpectedKey);
    Interlocked.Decrement(ref _pendingFleetDispatchCount);
}
```

**Driver:** Structurally simpler than Driver 1, but the `ReservedDelta != 0` guard is a distinct decision path. Extracting to `DrainLegacySlotOnAbort` would isolate this loop.

---

### 3 — Sideband index bounds-check repeated twice (CYC +2, readability debt)

The `_sbIdx` bounds check `(_sbIdx >= 0 && _sbIdx < _photonSideband.Length)` appears as a ternary on line 296, and then `if (_sbIdx >= 0)` / `if (_sbIdx < _photonSideband.Length)` as separate guards on lines 301–305. The same pattern recurs in `VerifyPhotonSlotIntegrity` and `ProcessValidPhotonSlot`. This redundancy inflates CYC and suggests a missing `TryGetSidebandKey(int sbIdx, out string key)` helper that centralises bounds logic.

---

## Recommended Extraction Plan

| Extraction | Target Method | CYC Reduction |
|------------|--------------|---------------|
| 1 | `DrainPhotonSlotOnAbort(FleetDispatchSlot slot)` — encapsulate Photon ring per-slot rollback | −5 |
| 2 | `DrainLegacySlotOnAbort(FleetDispatchRequest req)` — encapsulate legacy queue per-slot rollback | −2 |
| 3 | `TryGetSidebandKey(int sbIdx, out string key)` — centralise bounds-safe sideband key read | −2 (across callers) |

**Recommended extraction count: 3**

Post-extraction target CYC for `DrainAllDispatchQueuesOnAbort`: **≤ 4**
(two while-loops with single method-call bodies + one post-drain CB reset = 3 decisions + 1 = CYC 4)

---

## Agent Tracking

| Field            | Value                                |
|------------------|--------------------------------------|
| Agent Name       | v12-phase0-hotspot                   |
| Bobcoins Used    | 6                                    |
| Execution Time   | ~55s                                 |
| MCP Tools Used   | read_file, glob, grep, write_file    |
| Timestamp (UTC)  | 2025-07-16                           |
