# EPIC-W7-051 — Phase 0: Hotspot Analysis

## Method Under Analysis

| Field               | Value                                        |
|---------------------|----------------------------------------------|
| **Method**          | `UpdateStopOrder`                            |
| **CYC (reported)**  | 0 (seed input from task header)              |
| **CYC (analysed)**  | 6 (6 decision branches; see drivers below)   |
| **File**            | `src/V12_002.Trailing.StopUpdate.cs`         |
| **Lines**           | 84–139 (56 lines of body)                    |
| **Class**           | `V12_002` (partial — Trailing.StopUpdate)    |
| **Wave / Phase**    | Wave 7 / Phase 0                             |

---

## Blast Radius Summary

`UpdateStopOrder` is the **single choke-point for all live stop moves** in the strategy.
Every component that needs to reposition a protective stop calls this method directly.

| Caller file                                   | Call sites |
|-----------------------------------------------|-----------|
| `V12_002.Trailing.cs`                         | 5         |
| `V12_002.UI.Callbacks.cs`                     | 4         |
| `V12_002.Trailing.Breakeven.cs`               | 2         |
| `V12_002.SIMA.Shadow.cs`                      | 1         |
| `V12_002.Orders.Callbacks.Propagation.cs`     | 1         |
| `V12_002.Symmetry.Replace.cs`                 | 1         |
| `V12_002.UI.IPC.Commands.Mode.cs`             | 1         |

**Total direct call sites: 15 across 7 files.**

Downstream impact touches 13 additional files that depend on the shared state mutated here
(`pendingStopReplacements`, `stopOrders`, `activePositions`, `pendingReplacementCount`,
`circuitBreakerActive`) — namely the entire order-management, SIMA, REAPER, and UI layers.
Any regression in `UpdateStopOrder` has an **extreme blast radius** covering the full order
lifecycle subsystem.

---

## Top 3 Complexity Drivers

### 1 — Four-way routing dispatch (no unified strategy pattern)

`UpdateStopOrder` contains an implicit 4-path FSM driven by three consecutive `if`/return
guards, each delegating to a separate private helper:

```
stale-pending detected      → HandleStalePendingReplacement (return)
CancelPending | Submitted   → UpdateExistingPendingReplacement (return)
Working | Accepted          → InitiateStopReplacement (return)
<fall-through>              → CreateDirectStopOrder
```

The dispatch is implicit — there is no named state machine, no `enum OrderRoutingState`, and
no single place that documents all valid transitions. Adding a fifth order state requires
understanding all four paths simultaneously, making change fragile and error-prone.

### 2 — `pendingStopReplacements` / circuit-breaker state entangled with routing logic

`UpdateStopOrder` both **reads** order state (`stopOrders`, `pendingStopReplacements`) and
**drives** safety counters (`Interlocked.Increment(ref pendingReplacementCount)`,
`circuitBreakerActive = true`). This conflation of "where should I route this update?" with
"is the system overloaded?" violates single-responsibility. The circuit-breaker logic is
duplicated verbatim in `UpdateExistingPendingReplacement` (lines 193–205) and
`InitiateStopReplacement` (lines 353–358), creating a third complexity hotspot inside the
helpers extracted from this very method.

### 3 — `CaptureTargetSnapshot` / `RefreshTargetSnapshot` inline duplication

`InitiateStopReplacement` (lines 316–336) contains a full copy of the 5-target snapshot loop
that also exists in `CaptureTargetSnapshot` (lines 257–278) and `RefreshTargetSnapshot`
(lines 281–304). The loop body is identical (iterate `_tB` 1–5, call
`GetTargetOrdersDictionary`, test `OrderState.Working | Accepted`, build `TargetSnapshot`).
This triplication means any change to how brackets are captured must be applied in three
places, each named differently (`_tA`, `_t2`, `_tB`), with no shared helper enforcing
consistency.

---

## Recommended Extraction Count

| Extraction                                       | Rationale                                                  |
|--------------------------------------------------|------------------------------------------------------------|
| **1** — `ResolveStopRoute(entryName, currentStop)` → enum | Replace implicit 4-way if/return chain with explicit enum + switch |
| **2** — `IncrementAndCheckCircuitBreaker()`      | Remove duplicated Interlocked + `circuitBreakerActive` pattern |
| **3** — `BuildTargetSnapshot(entryName)` (shared) | Consolidate three identical target-capture loops into one helper |

**Recommended total: 3 extractions** (reducing analysed CYC from 6 → ≤3, eliminating
all three duplication hotspots).

---

## Agent Tracking

```
epic_id:        EPIC-W7-051
wave:           7
phase:          0
agent:          Bob (bob-assistant)
status:         completed
output:         docs/brain/EPIC-W7-051/00-hotspots.md
source_file:    src/V12_002.Trailing.StopUpdate.cs
method:         UpdateStopOrder
cyc_reported:   0
cyc_analysed:   6
blast_callers:  15
blast_files:    7 direct / 13 transitive
extractions:    3 recommended
completed_at:   2025-07-11
```
