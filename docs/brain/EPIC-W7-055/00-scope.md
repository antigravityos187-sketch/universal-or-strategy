# EPIC-W7-055 — Phase 1: Scope Definition

## Scope Summary

| Field | Value |
|---|---|
| **Epic ID** | EPIC-W7-055 |
| **Wave** | 7 |
| **Phase** | 1 — Scope Definition |
| **Method Name** | `DrainPhotonQueuesOnShutdown` |
| **Source File** | `src/V12_002.SIMA.Lifecycle.cs` |
| **Current CYC** | 8 |
| **Target CYC** | ≤ 8 |
| **Callers in src/** | 0 (no callers by this name in source; logic is inlined within `ProcessShutdownSIMA` lines 98–138) |

## Scope Boundary

> **Only `DrainPhotonQueuesOnShutdown` and its new extracted helper methods are in scope.**

This epic covers exclusively the cyclomatic complexity reduction of `DrainPhotonQueuesOnShutdown` in `src/V12_002.SIMA.Lifecycle.cs`. The scope boundary is drawn at the single method: the drain blocks currently inlined inside `ProcessShutdownSIMA` (lines 98–138), which constitute the logical body of `DrainPhotonQueuesOnShutdown`. No other methods, files, or classes are within this scope boundary.

The public/internal signature of `DrainPhotonQueuesOnShutdown` must remain unchanged. New private helper methods extracted during refactoring exist within the same partial class file. The scope boundary is hard at this single method and its direct extractions — no cross-file changes are permitted.

## Single Method In Scope

This refactor targets a **single method**: `DrainPhotonQueuesOnShutdown` (implemented as the inline drain blocks of `ProcessShutdownSIMA`, `src/V12_002.SIMA.Lifecycle.cs`, lines 98–138).

The method performs two sequential operations:
1. **Photon ring drain** (`_photonDispatchRing` while-loop, lines 107–123): sideband-aware, delta-rollback, pool release.
2. **Legacy queue drain** (`_pendingFleetDispatches` while-loop, lines 129–134): pre-Photon compatibility path, delta rollback + sync-barrier clear.

**CYC breakdown (current = 8):**

| Branch | CYC contribution |
|---|---|
| Base (single entry point) | 1 |
| `while (_photonDispatchRing != null && TryDequeue(...))` compound loop guard | +1 |
| Ternary `(_sbIdx >= 0 && _sbIdx < _photonSideband.Length)` compound conditional | +1 |
| `if (ringSlot.ReservedDelta != 0 && _expectedKey != null)` compound AND guard | +1 |
| `if (_expectedKey != null)` sync-pending clear guard | +1 |
| `if (_sbIdx >= 0)` pool release outer guard | +1 |
| `if (_sbIdx < _photonSideband.Length)` sideband clear inner guard | +1 |
| `while (_pendingFleetDispatches.TryDequeue(...))` legacy queue drain loop | +1 |
| `if (ignored.ReservedDelta != 0)` legacy delta rollback guard | +1 |
| **Total** | **CYC 8** |

## Caller Count

A full grep of `src/` for `DrainPhotonQueuesOnShutdown` returns **0 matches**. The method name does not appear as a callable symbol in the source tree — it is the logical designation for the inline drain blocks within `ProcessShutdownSIMA`. All references exist in documentation, scripts, and analysis artifacts only. The blast radius of any change is therefore bounded entirely within `ProcessShutdownSIMA` and the SIMA enable/disable cycle.

## Why Other Methods Are NOT In Scope (V12.23)

Per **V12.23** (scope containment rule), the following methods are explicitly excluded from this epic:

1. **`DrainAllDispatchQueuesOnAbort`** (`src/V12_002.SIMA.Fleet.cs`, lines 287–323): This is the closest sibling — a near-duplicate of the Photon-ring + legacy-queue drain logic with three structural differences (`AddExpectedPositionDeltaLocked` vs unlocked, `TrackPhotonDequeue`, `Interlocked.Decrement(ref _pendingFleetDispatchCount)`, and `TryResetCircuitBreakerIfBelow`). Although the hotspot analysis identifies it as a divergence risk, it resides in a separate file (`V12_002.SIMA.Fleet.cs`) and carries different operational semantics. V12.23 prohibits extending the scope boundary to cross-file methods to contain blast radius. A separate epic must be opened for that method.

2. **`ProcessShutdownSIMA`** (`src/V12_002.SIMA.Lifecycle.cs`): This is the direct parent caller. The outer structure of `ProcessShutdownSIMA` (the `CancelAllV12GtcOrders`, `StopReaperAudit`, `UnsubscribeFromFleetAccounts` sequence) is out of scope. Only the drain blocks it contains are in scope as the body of the single method `DrainPhotonQueuesOnShutdown`.

3. **`ProcessApplySimaState`**, **`ProcessInitializeSIMA`**, **`EnumerateApexAccounts`**, and all hydration methods in `src/V12_002.SIMA.Lifecycle.cs`: These methods belong to the SIMA lifecycle but have no complexity overlap with the drain logic. Extending scope to them would constitute scope creep prohibited by V12.23.

4. **All methods in `src/V12_002.Lifecycle.cs`** (the strategy `Terminated` state path via `DrainQueuesForShutdown`): This is a call-chain ancestor, not the target method. V12.23 requires that scope containment stop at the single method boundary even when a call-chain ancestor exists.

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase1-scope |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | ~90s |
| **Wave** | 7 |
| **Phase** | 1 — Scope Definition |
| **Epic** | EPIC-W7-055 |
| **Source File** | `src/V12_002.SIMA.Lifecycle.cs` |
| **Method** | DrainPhotonQueuesOnShutdown |
| **CYC Confirmed** | 8 |
| **Target CYC** | ≤ 8 |
| **scope_confirmed_single_method** | true |
