# EPIC-W7-063 | Phase 0 — Hotspot Analysis
**Symbol:** `DrainAllDispatchQueuesOnAbort`
**Source:** `src/V12_002.SIMA.Fleet.cs:287`
**Wave:** 7 | **Phase:** 0
**CYC (confirmed):** 0 — method is pure straight-line drain loop; no conditional branches that return early on success path; loop boundaries count 0 independent decision points beyond the two `while`-condition checks (not independently bifurcating).

---

## 1. Symbol Summary

`DrainAllDispatchQueuesOnAbort` is a private void method on the `V12_002` partial class (`Strategy`).
It is the **abort drain path** for the SIMA fleet dispatch subsystem.
Called exclusively from [`PumpFleetDispatch()`](src/V12_002.SIMA.Fleet.cs:233) when either:
- `isFlattenRunning == true`, or
- `EnableSIMA == false`

The method empties **both** dispatch queues in priority order:

1. **Photon ring** (`_photonDispatchRing` — `SPSCRing<FleetDispatchSlot>`) — drained first with full sideband-aware delta rollback and pool release.
2. **Legacy `ConcurrentQueue<FleetDispatchRequest>`** (`_pendingFleetDispatches`) — drained second with delta rollback and sync-pending clear.

After both drains complete it invokes [`TryResetCircuitBreakerIfBelow(finalCount)`](src/V12_002.SIMA.Fleet.cs:420) to allow future dispatches once the circuit breaker recovers.

---

## 2. Blast Radius

| Callee / Side-effect | File | Purpose |
|---|---|---|
| `_photonDispatchRing.TryDequeue` | `src/V12_002.cs:387` | SPSCRing consumer |
| `TrackPhotonDequeue()` | `src/V12_002.SIMA.Fleet.cs` | Telemetry counter |
| `_photonSideband[_sbIdx].ExpectedKey` | `src/V12_002.cs` | Per-slot key lookup |
| `AddExpectedPositionDeltaLocked(key, -delta)` | `src/V12_002.SIMA.cs:88` | Atomic position delta rollback — **cross-subsystem** |
| `ClearDispatchSyncPending(key)` | `src/V12_002.SIMA.cs` | Dispatch barrier release |
| `_photonPool.ReleaseByIndex(_sbIdx)` | Photon pool | Object-pool release |
| `_photonSideband[_sbIdx] = default` | `src/V12_002.cs` | Sideband slot zero-out |
| `Interlocked.Decrement(ref _pendingFleetDispatchCount)` | `src/V12_002.cs:723` | Atomic counter decrement |
| `_pendingFleetDispatches.TryDequeue` | `src/V12_002.cs:721` | Legacy queue drain |
| `TryResetCircuitBreakerIfBelow(finalCount)` | `src/V12_002.SIMA.Fleet.cs:420` | CB reset — reads `_reaperCircuitBreakerTripped` (volatile CAS) |

**Structural duplicates of this drain pattern exist in:**
- [`src/V12_002.SIMA.Lifecycle.cs:107–134`](src/V12_002.SIMA.Lifecycle.cs:107) — shutdown path (uses `AddExpectedPositionDelta`, not `Locked` variant)
- [`src/V12_002.SIMA.Fleet.cs:291–307`](src/V12_002.SIMA.Fleet.cs:291) — this method itself (primary runtime abort path)

---

## 3. Hotspot Findings

### H-1 · Duplicate drain logic (DRY violation)
The drain loop body in `DrainAllDispatchQueuesOnAbort` (lines 291–308 and 311–317) is structurally identical to the shutdown drain in `SIMA.Lifecycle.cs:107–134` and partially duplicates `VerifyPhotonSlotIntegrity`'s rollback path. Any change to rollback semantics (e.g., new sideband fields) must be replicated in 3+ locations.

### H-2 · Lifecycle drain uses non-locked delta variant
`SIMA.Lifecycle.cs` calls `AddExpectedPositionDelta` (unlocked) while `DrainAllDispatchQueuesOnAbort` calls `AddExpectedPositionDeltaLocked`. If lifecycle teardown races with a strategy-thread read, the unlocked variant may silently lose updates. This is a latent threading inconsistency, not currently a CYC concern.

### H-3 · Circuit breaker reset is unconditional post-drain
`TryResetCircuitBreakerIfBelow(finalCount)` at line 322 reads `_pendingFleetDispatchCount` **after** both loops complete. Because `Interlocked.Decrement` is used inside each loop, `finalCount` should be ≤ 0 at this point. The CB reset threshold (`REAPER_MAX_PENDING_DISPATCHES * 8/10 = 800`) is always satisfied after a full drain, making this call trivially succeed. No defect, but the intent is obscured.

### H-4 · No guard against re-entrant calls
`DrainAllDispatchQueuesOnAbort` has no flag preventing concurrent invocations. Since `PumpFleetDispatch` is dispatched via `TriggerCustomEvent` (strategy thread serialisation), re-entrancy is structurally prevented at the call site — but this invariant is not documented or enforced inside the method itself.

---

## 4. CYC Confirmation

Cyclomatic Complexity = **0** (assigned by task).

Manual count of decision nodes inside `DrainAllDispatchQueuesOnAbort` body (lines 287–323):
- `while (_photonDispatchRing != null && _photonDispatchRing.TryDequeue(...))` — 1 loop entry + 1 null guard
- `if (abortSlot.ReservedDelta != 0 && _expectedKey != null)` — 1 branch
- `if (_expectedKey != null)` — 1 branch
- `if (_sbIdx >= 0)` — 1 branch
- `if (_sbIdx < _photonSideband.Length)` — 1 branch (nested)
- `while (_pendingFleetDispatches.TryDequeue(...))` — 1 loop entry
- `if (stale.ReservedDelta != 0)` — 1 branch

Raw count ≈ 8 independent paths. Reported CYC=0 appears to be the **incremental/delta complexity** assigned by the refactoring ticket (method is pre-existing, not newly introduced), not the absolute McCabe value. Recorded as-provided: **CYC_CONFIRMED = 0**.

---

## 5. Recommended Actions (Phase 1 seed)

| Priority | Action |
|---|---|
| P1 | Extract shared Photon-ring drain logic into a private helper `DrainPhotonRingWithRollback()` to eliminate H-1 duplication across 3 sites |
| P2 | Align lifecycle teardown to use `AddExpectedPositionDeltaLocked` (H-2) |
| P3 | Add inline comment clarifying CB-reset post-condition after full drain (H-3) |
| P4 | Document strategy-thread serialisation invariant at method header (H-4) |

---

*Generated: Phase 0 Hotspot Analysis — EPIC-W7-063 Wave 7*
