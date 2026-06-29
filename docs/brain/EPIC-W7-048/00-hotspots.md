# EPIC-W7-048 Hotspot Analysis

**Method:** UpdateExistingPendingReplacement
**CYC:** 0 (reported; structural complexity assessed below)
**File:** src/V12_002.Trailing.StopUpdate.cs

---

## Overview

`UpdateExistingPendingReplacement` is a private helper inside the `V12_002` partial class, called
exclusively from `UpdateStopOrder` when `currentStop.OrderState` is `CancelPending` or `Submitted`.
Its role is to atomically upsert a `PendingStopReplacement` record into the `pendingStopReplacements`
`ConcurrentDictionary` and optionally activate the circuit breaker. The method spans lines 167–253
of [`src/V12_002.Trailing.StopUpdate.cs`](src/V12_002.Trailing.StopUpdate.cs:167).

The CYC is reported as 0 by the tool invocation (likely a measurement artefact for a method with
no explicit branching in the outer frame), but the structural coupling — including two lambdas with
distinct conditional paths, a TryAdd/AddOrUpdate split, and shared concurrent state mutation — makes
this a medium-risk hotspot regardless of the reported CYC.

---

## Blast Radius Summary

| Dimension | Detail |
|---|---|
| **Direct caller** | `UpdateStopOrder` (line 119, `src/V12_002.Trailing.StopUpdate.cs`) |
| **Caller chain** | `UpdateStopOrder` → `UpdateExistingPendingReplacement` (CancelPending/Submitted path) |
| **Peer methods in dispatcher** | `HandleStalePendingReplacement`, `InitiateStopReplacement`, `CreateDirectStopOrder` |
| **Shared concurrent state written** | `pendingStopReplacements` (ConcurrentDictionary), `pendingReplacementCount` (volatile int via Interlocked) |
| **Shared state read** | `circuitBreakerActive` (volatile bool), `circuitBreakerActivatedTime`, `CIRCUIT_BREAKER_THRESHOLD` (const 5) |
| **Side-effects** | Mutates `pos.CurrentStopPrice`, `pos.CurrentTrailLevel`; calls `MarkStickyDirty()`; emits `Print()` diagnostic |
| **Snapshot helpers called** | `CaptureTargetSnapshot(entryName)`, `RefreshTargetSnapshot(entryName)` |
| **Threading constraint** | Must be called on strategy thread; `ConcurrentDictionary` used lock-free via `TryAdd`/`AddOrUpdate` |
| **Circuit breaker reach** | 13 files touch `pendingReplacementCount` or `circuitBreakerActive`; see grep evidence |
| **Risk on change** | High — any change to the TryAdd/AddOrUpdate split risks duplicate-count bugs or double circuit-breaker activation |

**Affected symbol count (blast radius):** 8 symbols directly coupled; 2 shared concurrent-counter fields;
circuit-breaker flag propagated across 13 source files.

---

## Top 3 Complexity Drivers

1. **TryAdd / AddOrUpdate split with two distinct lambda paths and conditional count increment**
   The method first attempts `pendingStopReplacements.TryAdd(entryName, newPending)`. On success it
   increments `pendingReplacementCount` via `Interlocked.Increment` and checks the circuit breaker
   threshold. On failure (key already present) it falls into `AddOrUpdate` with two lambdas: an
   add-factory (race-condition recovery that also calls `Interlocked.Increment`) and an update-factory
   that conditionally calls `RefreshTargetSnapshot` and rebuilds the struct. This branching tree —
   TryAdd-success, TryAdd-fail → add-factory, TryAdd-fail → update-factory, breaker-check inside each
   success path — is the primary source of structural complexity. Even though the CYC tool scores it as
   0 (lambdas are not counted by some tools), the actual decision paths are ≥ 5.

2. **Conditional `BracketRestorationNeeded` logic with nullable array guards across two code paths**
   Both the outer scope (`_b955TargetsA != null && _b955TargetsA.Length > 0`) and the update-factory
   lambda (`!pending.BracketRestorationNeeded`, `_b950Refresh != null`, `_b950Refresh.Length > 0`)
   must agree on whether bracket restoration is required. The update-factory preserves the existing
   `CapturedTargets` if `BracketRestorationNeeded` is already true, otherwise it calls
   `RefreshTargetSnapshot`. This two-level conditional read-before-write on the captured snapshot is
   a latent correctness risk: if `RefreshTargetSnapshot` returns null concurrently with position
   teardown, the update-factory silently retains a stale `null` snapshot.

3. **Shared volatile/Interlocked state entangled with circuit breaker flag check**
   `pendingReplacementCount` is a `volatile int` incremented by `Interlocked.Increment`, and
   `circuitBreakerActive` is a `volatile bool` written without interlocked protection (`circuitBreakerActive = true`
   is not an atomic compare-and-swap). The check `currentCount >= CIRCUIT_BREAKER_THRESHOLD && !circuitBreakerActive`
   is a non-atomic read-check-write: two concurrent threads could both observe `!circuitBreakerActive`
   as true and both set it, emitting duplicate breaker log entries. This pattern is duplicated verbatim
   in `InitiateStopReplacement`, creating a systemic race that is one of the strongest arguments for
   extracting a dedicated `TryActivateCircuitBreaker` helper.

---

## Recommended Extraction Count

**2 extractions recommended.**

| # | Proposed Helper | Rationale |
|---|---|---|
| 1 | `TryActivateCircuitBreaker(int currentCount)` | De-duplicates the non-atomic breaker check that appears identically in `UpdateExistingPendingReplacement` and `InitiateStopReplacement`; allows the flag write to be replaced with a single `Interlocked.CompareExchange` |
| 2 | `BuildRefreshedPendingReplacement(string entryName, PendingStopReplacement pending, double validatedStopPrice)` | Isolates the update-factory lambda body (RefreshTargetSnapshot + conditional BracketRestorationNeeded merge + struct rebuild) into a named, testable method; reduces the cognitive load of reading the AddOrUpdate call |

The `TryAdd` / `AddOrUpdate` orchestration in the outer method body should remain in place after
extraction — it is the correct concurrency pattern for `ConcurrentDictionary` and should not be
further decomposed.

---

## Agent Tracking

Agent Name: bob-phase0-hotspot | Epic: EPIC-W7-048 | Wave: 7 | Phase: 0
Bobcoins Used: 1.0 | Execution Time: ~60s | CYC Reported: 0 | CYC Structural: ~5
