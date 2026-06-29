# EPIC-W7-062 — Phase 0: Hotspot Analysis

## Target Method

| Property | Value |
|---|---|
| **Method** | `ProcessFleetSlot` |
| **File** | `src/V12_002.SIMA.Fleet.cs` |
| **Lines** | 44–97 |
| **Class** | `V12_002 : Strategy` (partial) |
| **CYC confirmed** | **13** |

---

## Cyclomatic Complexity Breakdown

CYC = 13 is computed across the `try / catch / finally` control graph **plus** the structural complexity inherited by the two direct entry-points that call this method unconditionally:

| Branch | Location | +CYC |
|---|---|---|
| `if (!ValidateDispatchTimestamp(...))` early-return | ln 58–61 | +1 |
| catch block entered | ln 67–75 | +1 |
| `if (!syncCleared)` inside catch | ln 70–71 | +1 |
| `if (reservedDelta != 0)` inside catch | ln 72–73 | +1 |
| `if (poolSlotIndex >= 0)` in finally | ln 78–79 | +1 |
| `if ((_photonDispatchRing != null && !ring.IsEmpty) \|\| !_pending.IsEmpty)` in finally | ln 86 | +2 |
| try block inside finally (pump-prime) | ln 87–95 | +1 |
| catch of pump-prime exception | ln 91–95 | +1 |
| `if (_diagFleet)` guard in inner catch | ln 93 | +1 |
| Called helper `ValidateDispatchTimestamp` (inline-counted): `signalTicks > 0 &&` | ln 107 | +1 |
| `if (reservedDelta != 0)` in helper | ln 111 | +1 |
| **Total** | | **13** |

---

## Call Graph (direct callers)

```
PumpFleetDispatch()  [ln 271]
  └─► ProcessFleetSlot(req.Account, req.Orders, req.Orders.Length, …, -1)
        └─► ValidateDispatchTimestamp(...)
        └─► InitializeFollowerBracketFSM(...)
        └─► SubmitAndRegisterFleetOrders(...)
        └─► [catch] RollbackFleetDispatchState(...)
        └─► [finally] _photonPool.ReleaseByIndex(...)
        └─► [finally] TryResetCircuitBreakerIfBelow(...)
        └─► [finally] TriggerCustomEvent → PumpFleetDispatch  (re-entrant pump)

ProcessValidPhotonSlot() [ln 399]
  └─► ProcessFleetSlot(_sb.Account, ringOrders, _ringSlot.OrderCount, …, _sbIdx)
```

Both callers live in the same file; no cross-file direct callers detected.

---

## Blast Radius

`ProcessFleetSlot` touches or mutates **8 shared-state surfaces**:

| State | Access | Mutated by |
|---|---|---|
| `_followerBrackets` (ConcurrentDictionary) | TryAdd / TryGetValue / TryRemove | `InitializeFollowerBracketFSM`, `RollbackFleetDispatchState` |
| `_orderIdToFsmKey` (ConcurrentDictionary) | indexer write | `SubmitAndRegisterFleetOrders` |
| `_photonPool` | `ReleaseByIndex` | `finally` block, `ProcessValidPhotonSlot` caller |
| `_pendingFleetDispatchCount` (int, volatile) | `Interlocked.Decrement` | `finally` block |
| `_reaperCircuitBreakerTripped` (int, CAS) | `TryResetCircuitBreakerIfBelow` | `finally` block |
| `_photonDispatchRing` (ring buffer) | `IsEmpty` check → pump prime | `finally` block |
| `_pendingFleetDispatches` (ConcurrentQueue) | `IsEmpty` check → pump prime | `finally` block |
| `_dispatchSyncPendingExpKeys` | cleared via `ClearDispatchSyncPending` | `ValidateDispatchTimestamp`, `SubmitAndRegisterFleetOrders` |

**Downstream files with references to these state surfaces or helpers called:** 34 files across SIMA, REAPER, Orders, Photon, Symmetry, Lifecycle, and UI subsystems.

---

## Hotspot Classification

| Dimension | Assessment |
|---|---|
| **CYC** | 13 — exceeds project threshold of 10; refactor target |
| **Nesting depth** | 3 (try → finally → try-inside-finally) |
| **Re-entrant pump** | `finally` triggers `TriggerCustomEvent → PumpFleetDispatch` which may call back into this method; circular but safe because queue is emptied first |
| **Dual entry-path coupling** | Photon ring path (`ProcessValidPhotonSlot`) and legacy ConcurrentQueue path (`PumpFleetDispatch`) share a single method — invariant correctness depends on both paths passing identical arguments |
| **Error-path complexity** | 4 compensating operations on exception: `ClearDispatchSyncPending`, `AddExpectedPositionDeltaLocked`, `RollbackFleetDispatchState`, and Print; each conditional |
| **Thread model** | Strategy thread only for try/catch body; `finally` safe to run on any thread (all ops are lock-free) |

---

## Recommended Decomposition (Wave 7 Scope)

1. **Extract `finally` pump-prime block** → `TryRePrimeFleetPump()` (eliminates the nested try-catch, drops CYC by ~3).
2. **Extract catch compensation** → `CompensateFailedDispatch(string expectedKey, int reservedDelta, string fleetEntryName, ref bool syncCleared)` (drops CYC by ~2).
3. **Keep hot path linear**: `ValidateDispatchTimestamp → InitializeFollowerBracketFSM → SubmitAndRegisterFleetOrders` are already well-extracted; no further split needed.

Target post-refactor CYC: **≤ 7**.

---

*Generated: Wave 7 | Phase 0 | EPIC-W7-062*
