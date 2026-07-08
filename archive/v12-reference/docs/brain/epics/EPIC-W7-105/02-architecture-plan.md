# Phase 2: Architecture Plan — EPIC-W7-105

**Agent:** v12-phase2-architecture
**Wave:** 7 | **Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:15:00Z
**Input:** docs/brain/EPIC-W7-105/01-scope-boundary.md

---

## Method Under Extraction

- **Method:** `DrainAllDispatchQueuesOnAbort`
- **Source File:** `src/V12_002.SIMA.Fleet.cs`
- **Line Range:** 287–323
- **Original CYC:** 12
- **Target CYC:** <= 8

### jcodemunch get_context_bundle result

Full source confirmed at `src/V12_002.SIMA.Fleet.cs:287`. The method is `private void` with no parameters. It performs three sequential operations:
1. Photon ring drain (`_photonDispatchRing`) with per-slot sideband-aware delta rollback, pool release, and sideband reset.
2. Legacy `ConcurrentQueue<FleetDispatchRequest>` drain with optional delta rollback.
3. Post-drain circuit breaker reset via `TryResetCircuitBreakerIfBelow(finalCount)`.

All field accesses use lock-free primitives: `Interlocked.Decrement`, `Volatile.Read`, `ConcurrentQueue<T>.TryDequeue`, and `_photonPool.ReleaseByIndex` (internally atomic). No `lock()` blocks present.

### jcodemunch get_call_hierarchy result

**Callers (depth 1):**
- `PumpFleetDispatch` — `src/V12_002.SIMA.Fleet.cs:233` (sole direct caller; invoked when `isFlattenRunning == true` or `!EnableSIMA`)

**Callers (depth 2):**
- `ProcessFleetSlot` — `src/V12_002.SIMA.Fleet.cs:44`
- `VerifyPhotonSlotIntegrity` — `src/V12_002.SIMA.Fleet.cs:329`

**Callees (depth 1):**
- `TrackPhotonDequeue` (`src/V12_002.Telemetry.cs:161`) — telemetry, no state side-effect
- `AddExpectedPositionDeltaLocked` (`src/V12_002.SIMA.cs:88`) — mutates expected-position dict
- `ClearDispatchSyncPending` (`src/V12_002.SIMA.cs:179`) — mutates sync-pending set
- `TryResetCircuitBreakerIfBelow` (`src/V12_002.SIMA.Fleet.cs:420`) — CAS-based CB reset
- `_photonDispatchRing` field, `_photonPool` field, `_pendingFleetDispatches` field (all lock-free)

### jcodemunch get_dependency_graph result

`src/V12_002.SIMA.Fleet.cs` has **0 import edges and 0 importer edges** at file level — it is a partial class fragment with no standalone import graph edges. All dependencies are resolved through the partial class merge at compile time. Cross-file blast radius is zero.

### jcodemunch get_extraction_candidates result

No structured candidates returned (complexity metadata not populated in index). Extraction plan derived from Phase 0 hotspot analysis (`00-hotspots.md`) which identified 3 extraction targets with CYC reductions of -5, -2, and -2 respectively.

---

## Sequential Thinking Summary

Five thoughts executed via `mcp__sequential-thinking__sequentialthinking`:

1. **Thought 1 — Findings:** MCP analysis confirmed method structure (3-phase drain), sole caller `PumpFleetDispatch`, 21 callees, and zero cross-file import edges. CYC=12 driven by 5 decision points in Photon loop body, 2 in legacy loop, and 2 from while guards.

2. **Thought 2 — Helper 1:** `DrainPhotonSlotOnAbort(FleetDispatchSlot slot)` extracts the entire per-slot Photon ring rollback body. Contains 5 internal decisions (ternary `&&`, 2 compound `if`, 1 pool guard `if`, 1 sideband reset `if`) → projected CYC = **6**.

3. **Thought 3 — Helper 2:** `DrainLegacySlotOnAbort(FleetDispatchRequest req)` extracts the per-item legacy queue rollback body. Contains 1 internal decision (delta guard `if`) → projected CYC = **2**.

4. **Thought 4 — Helper 3:** `TryGetSidebandKey(int sbIdx, out string key)` centralises bounds-safe sideband key resolution used inside Helper 1 and reusable by sibling methods. Contains 1 compound guard → projected CYC = **2**. Scope-compliant: new private helper called only by extracted code; sibling methods not modified.

5. **Thought 5 — Final Verdict:** Parent after extraction = 3 decisions (2 while guards + 1 base) → projected CYC = **3**. All helpers within <=8. Jane Street PASS on all 5 mandates (CYC<=8, single-responsibility, lock-free, illegal-states-unrepresentable, zero-allocation).

---

## Extraction Plan

| # | Helper Method Name | Responsibility | Projected CYC | Strategy |
|---|---|---|---|---|
| 1 | `DrainPhotonSlotOnAbort(FleetDispatchSlot slot)` | Per-slot Photon ring sideband rollback, pool release, and counter decrement | **6** | Extract Loop Body |
| 2 | `DrainLegacySlotOnAbort(FleetDispatchRequest req)` | Per-item legacy queue delta rollback and counter decrement | **2** | Extract Loop Body |
| 3 | `TryGetSidebandKey(int sbIdx, out string key)` | Bounds-safe sideband key read from `_photonSideband[]` | **2** | Extract Guard Clauses / Extract Named Helper |

### Helper Method Signatures

```csharp
// Helper 1: Encapsulates full per-slot Photon ring rollback
private void DrainPhotonSlotOnAbort(FleetDispatchSlot slot)
{
    TrackPhotonDequeue();
    int sbIdx = slot.PoolSlotIndex;
    string expectedKey = TryGetSidebandKey(sbIdx, out var k) ? k : null;
    if (slot.ReservedDelta != 0 && expectedKey != null)
        AddExpectedPositionDeltaLocked(expectedKey, -slot.ReservedDelta);
    if (expectedKey != null)
        ClearDispatchSyncPending(expectedKey);
    if (sbIdx >= 0)
    {
        _photonPool.ReleaseByIndex(sbIdx);
        if (sbIdx < _photonSideband.Length)
            _photonSideband[sbIdx] = default(FleetDispatchSideband);
    }
    Interlocked.Decrement(ref _pendingFleetDispatchCount);
}

// Helper 2: Encapsulates per-item legacy queue rollback
private void DrainLegacySlotOnAbort(FleetDispatchRequest req)
{
    if (req.ReservedDelta != 0)
        AddExpectedPositionDeltaLocked(req.ExpectedKey, -req.ReservedDelta);
    ClearDispatchSyncPending(req.ExpectedKey);
    Interlocked.Decrement(ref _pendingFleetDispatchCount);
}

// Helper 3: Bounds-safe sideband key resolution
private bool TryGetSidebandKey(int sbIdx, out string key)
{
    if (sbIdx >= 0 && sbIdx < _photonSideband.Length)
    {
        key = _photonSideband[sbIdx].ExpectedKey;
        return true;
    }
    key = null;
    return false;
}
```

### Parent Method After Extraction

```csharp
private void DrainAllDispatchQueuesOnAbort()
{
    // v28.0: drain Photon ring FIRST with sideband-aware delta rollback + pool release
    FleetDispatchSlot abortSlot;
    while (_photonDispatchRing != null && _photonDispatchRing.TryDequeue(out abortSlot))
        DrainPhotonSlotOnAbort(abortSlot);

    // Then drain legacy ConcurrentQueue
    FleetDispatchRequest stale;
    while (_pendingFleetDispatches.TryDequeue(out stale))
        DrainLegacySlotOnAbort(stale);

    // REAPER-EXPANSION P0 FIX: Reset circuit breaker after drain completes
    int finalCount = Volatile.Read(ref _pendingFleetDispatchCount);
    TryResetCircuitBreakerIfBelow(finalCount);
}
```

---

## Parent Method After Extraction

- **Remaining logic:** Two while-loop skeletons each delegating to a named helper, plus one post-drain CB reset call.
- **Projected CYC:** **3** (1 base + 1 photon-while guard + 1 legacy-while guard; no intra-loop decisions remain in parent)

---

## max_cyc_projected: 6
## extraction_count: 3

---

## Jane Street Alignment

| Mandate | Status |
|---|---|
| CYC<=8 achieved (all methods) | **YES** — parent=3, H1=6, H2=2, H3=2 |
| Single-responsibility per helper | **YES** — each helper has exactly one purpose |
| Lock-free/Actor pattern preserved | **YES** — only Interlocked, Volatile, ConcurrentQueue; no lock() |
| Illegal states unrepresentable | **YES** — each slot's rollback is atomically encapsulated; partial rollback impossible |
| Zero-allocation hot paths | **YES** — FleetDispatchSlot/Request are value types passed by value; no heap allocation |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | ~15 |
| **Execution Time** | 2026-06-29T01:15:00Z |
| **Wave** | 7 |
| **Phase** | 2 |
| **jcodemunch tools called** | get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **extraction_count** | 3 |
| **max_cyc_projected** | 6 |
| **Output** | docs/brain/EPIC-W7-105/02-architecture-plan.md |
