# Phase 0 — Hotspot Analysis
## EPIC-W7-101

---

## Method Under Analysis

| Field        | Value                                                        |
|--------------|--------------------------------------------------------------|
| Method Name  | `VerifyPhotonSlotIntegrity`                                  |
| CYC Score    | **16**                                                       |
| File Path    | `src/V12_002.SIMA.Fleet.cs`                                  |
| Line Range   | 329 – 389                                                    |
| Signature    | `private bool VerifyPhotonSlotIntegrity(ref FleetDispatchSlot, FleetDispatchSideband, int)` |

---

## Blast Radius Summary

`VerifyPhotonSlotIntegrity` sits on the **hot path** of the Photon ring consumer inside `PumpFleetDispatch` (line 258). It is called on every dequeue from the SPSC ring (`SPSCRing<FleetDispatchSlot>`), making its failure branch a **full rollback sequence** that touches five independent subsystems:

| Subsystem                        | Impact on failure branch                                      |
|----------------------------------|---------------------------------------------------------------|
| `AddExpectedPositionDeltaLocked` | Reverses reserved position delta (thread-safe, locked)        |
| `ClearDispatchSyncPending`       | Releases dispatch sync semaphore keyed on `ExpectedKey`       |
| `activePositions` / `entryOrders` / `stopOrders` / target dicts | Five concurrent dictionary removals (in-order rollback) |
| `_followerBrackets`              | FSM state torn down for corrupted entry                       |
| `_photonPool` / `_photonSideband`| Pool slot released; sideband zeroed to prevent stale refs     |
| `TryResetCircuitBreakerIfBelow`  | Circuit breaker CAS reset after decrement                     |
| `TriggerCustomEvent → PumpFleetDispatch` | Recursive pump-prime on non-empty ring/queue          |

**Callers:** 1 direct caller — `PumpFleetDispatch` (line 258, same file).  
**Callees:** `ComputeFleetDispatchShadow`, `TrackPhotonCrcFailure`, `AddExpectedPositionDeltaLocked`, `ClearDispatchSyncPending`, `GetTargetOrdersDictionary`, `_photonPool.ReleaseByIndex`, `Interlocked.Decrement`, `TryResetCircuitBreakerIfBelow`, `TriggerCustomEvent`.

---

## CYC Decision-Point Breakdown

McCabe CYC = E − N + 2P = 1 (base) + 15 decision edges = **16**

| # | Line | Decision Point                                              | Type             |
|---|------|-------------------------------------------------------------|------------------|
| 1 | 336  | `if (_recomputed != _stored)` — integrity gate              | `if`             |
| 2 | 347  | `if (_ringSlot.ReservedDelta != 0 && _sb.ExpectedKey != null)` | compound `&&` |
| 3 | 347  | inner `&& _sb.ExpectedKey != null`                          | `&&` operand     |
| 4 | 349  | `if (_sb.ExpectedKey != null)` — sync clear guard           | `if`             |
| 5 | 351  | `if (_sb.FleetEntryName != null)` — rollback name guard     | `if`             |
| 6 | 356  | `for (int tNum = 1; tNum <= 5; tNum++)` — target loop       | `for`            |
| 7 | 359  | `if (td != null)` — null dict guard inside loop             | `if`             |
| 8 | 364  | `if (_sbIdx >= 0)` — pool release guard                     | `if`             |
| 9 | 367  | `if (_sbIdx < _photonSideband.Length)` — bounds check       | `if`             |
|10 | 376  | `if (!_photonDispatchRing.IsEmpty \|\| !_pendingFleetDispatches.IsEmpty)` | compound `\|\|` |
|11 | 376  | inner `\|\| !_pendingFleetDispatches.IsEmpty`               | `\|\|` operand   |
|12 | 377–385 | `try { TriggerCustomEvent(...) } catch (Exception ex)` — exception path | `catch` |
|13 | 383  | `if (_diagFleet)` — diagnostic guard inside catch           | `if`             |
|14–15 | 381 | lambda `o => PumpFleetDispatch()` creates implicit closure branch + early `return false` vs `return true` | implicit branches |

---

## Top 3 Complexity Drivers

### Driver 1 — Deeply Nested Rollback on Integrity Failure (lines 336–387)
The entire body of the method is a single large `if (_recomputed != _stored)` block spanning ~51 lines. Inside it, **eight sequential guarded operations** (items 2–13 above) fire in-order, each gating on a different nullability or index bound. The nesting of `for`→`if (td != null)` inside `if (_sb.FleetEntryName != null)` is the deepest point (depth 4). Extracting this failure block into a dedicated `HandleIntegrityFailure(ref FleetDispatchSlot, FleetDispatchSideband, int)` method would reduce CYC here to **2** (one decision: pass/fail).

### Driver 2 — Inline Pool/Sideband Teardown (lines 364–369)
The conditional pool release + sideband clear (`if (_sbIdx >= 0)` → `if (_sbIdx < _photonSideband.Length)`) appears **verbatim in four other locations** across the codebase (`DrainAllDispatchQueuesOnAbort`, `ProcessFleetSlot`, `SIMA.Dispatch.cs` × 2). Extracting this into `ReleasePhotonSlot(int sbIdx)` eliminates 2 decision points from this method and enables de-duplication of ~20 lines project-wide.

### Driver 3 — Pump-Prime try/catch + Diagnostic Guard (lines 376–385)
The inline `try { TriggerCustomEvent(...) } catch (Exception ex) { if (_diagFleet) ... }` pattern (also duplicated in `ProcessFleetSlot` lines 87–95) contributes 3 decision points (the `||` compound, `catch`, and `if (_diagFleet)`). Extracting it into `TryPrimePump(string context)` centralises the pattern and removes 3 CYC from both call sites.

---

## Recommended Extraction Count

| Extraction                                 | CYC Reduction | New Method                                  |
|--------------------------------------------|---------------|---------------------------------------------|
| Integrity failure rollback body            | −10           | `HandleIntegrityFailure(ref slot, sb, idx)` |
| Pool/sideband slot release                 | −2            | `ReleasePhotonSlot(int sbIdx)`              |
| Pump-prime try/catch                       | −3            | `TryPrimePump(string context)`              |
| **Total**                                  | **−15**       | Residual CYC ≈ **1** (shadow compare only) |

**Recommended extraction count: 3 methods**  
Post-refactor target CYC for `VerifyPhotonSlotIntegrity`: ≤ 3

---

## Agent Tracking

| Field            | Value                        |
|------------------|------------------------------|
| Agent Name       | `v12-phase0-hotspot`         |
| Bobcoins Used    | 6                            |
| Execution Time   | ~45s                         |
| Epic             | EPIC-W7-101                  |
| Wave             | 7                            |
| Phase            | 0 — Hotspot Analysis         |
| Output           | `docs/brain/EPIC-W7-101/00-hotspots.md` |
