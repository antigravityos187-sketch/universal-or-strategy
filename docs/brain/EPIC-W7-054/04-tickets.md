# Phase 4: Ticket Definitions — EPIC-W7-054

**Agent:** v12-phase4-tickets
**Wave:** 7 | **Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T03:00:00Z
**Inputs:** docs/brain/EPIC-W7-054/02-architecture-plan.md + docs/brain/EPIC-W7-054/03-audit-report.md

---

## Summary

| Field | Value |
|---|---|
| **Method** | `DrainAllDispatchQueuesOnAbort` |
| **Source File** | `src/V12_002.SIMA.Fleet.cs` |
| **Lines** | 287–323 |
| **Original CYC** | 20 |
| **Ticket Count** | 4 |
| **Max Projected CYC** | 6 (`DrainPhotonDispatchSlot`) |
| **Parent CYC After All Extractions** | 1 |
| **DNA Verdict** | PASS |

---

## Sequential Thinking Validation

**Thought 1 — Ticket count decision:**
CYC 20 requires 4 helpers (per Phase 2 architecture). V12 protocol: one ticket = one extracted helper = one concern. Four tickets (T-1 through T-4). Each ticket extracts one helper, calls it from the parent stub, and adds one xUnit `[Fact]` suite. T-4 also finalizes the parent to its clean CYC 1 form.

**Thought 2 — Line movements and helper names:**
- T-1: `ResolveSidebandKey(int sbIdx)` — 3-condition ternary from photon slot block. CYC 3.
- T-2: `DrainPhotonDispatchSlot(FleetDispatchSlot abortSlot)` — per-slot processing body from photon loop. CYC 6. Depends on T-1.
- T-3: `DrainPhotonDispatchRing()` — photon while-loop + null-guard. CYC 3. Depends on T-2.
- T-4: `DrainLegacyFleetDispatches()` — legacy ConcurrentQueue while-loop. CYC 3. Finalizes parent to CYC 1.

**Thought 3 — CYC verification:**
- `ResolveSidebandKey`: 3 ≤ 8 ✅
- `DrainPhotonDispatchSlot`: 6 ≤ 8 ✅
- `DrainPhotonDispatchRing`: 3 ≤ 8 ✅
- `DrainLegacyFleetDispatches`: 3 ≤ 8 ✅
- `DrainAllDispatchQueuesOnAbort` (parent, post-T-4): 1 ≤ 8 ✅
- max_cyc_projected = 6. All methods satisfy Jane Street CYC ≤ 8 threshold. **APPROVED.**

---

## Ticket T-1: Extract `ResolveSidebandKey`

| Field | Value |
|---|---|
| **Ticket ID** | T-1 |
| **Epic** | EPIC-W7-054 |
| **Type** | Extraction |
| **Source File** | `src/V12_002.SIMA.Fleet.cs` |
| **Target Lines** | Within 287–323 (sideband key ternary inside photon slot block) |
| **Helper Name** | `ResolveSidebandKey(int sbIdx)` |
| **Return Type** | `string?` |
| **Visibility** | `private` |
| **Projected CYC** | 3 |
| **Dependency** | None — extracted first |

### Work Description

Extract the 3-condition sideband key lookup from the photon slot processing block inside `DrainAllDispatchQueuesOnAbort`. The extracted body:

```csharp
private string? ResolveSidebandKey(int sbIdx)
{
    if (_photonSideband == null) return null;
    if (sbIdx < 0 || sbIdx >= _photonSideband.Length) return null;
    return _photonSideband[sbIdx].ExpectedKey;
}
```

Replace the inline ternary at its call site (inside the photon slot loop body) with `ResolveSidebandKey(abortSlot.SidebandIndex)`.

### Acceptance Criteria

- [ ] `ResolveSidebandKey(int sbIdx)` added as `private` method in `src/V12_002.SIMA.Fleet.cs`
- [ ] Inline ternary replaced with call to `ResolveSidebandKey`
- [ ] Build passes: `dotnet build` zero errors
- [ ] xUnit `[Fact]` — in-bounds sbIdx returns correct `ExpectedKey`
- [ ] xUnit `[Fact]` — out-of-bounds sbIdx returns null
- [ ] xUnit `[Fact]` — null `_photonSideband` returns null
- [ ] Projected CYC: 3

### Constraints

- No `lock()` blocks
- ASCII-only string literals
- Do NOT modify any caller files outside `src/V12_002.SIMA.Fleet.cs`
- Do NOT change `DrainAllDispatchQueuesOnAbort` signature

---

## Ticket T-2: Extract `DrainPhotonDispatchSlot`

| Field | Value |
|---|---|
| **Ticket ID** | T-2 |
| **Epic** | EPIC-W7-054 |
| **Type** | Extraction |
| **Source File** | `src/V12_002.SIMA.Fleet.cs` |
| **Target Lines** | Within 287–323 (per-slot body of photon while-loop) |
| **Helper Name** | `DrainPhotonDispatchSlot(FleetDispatchSlot abortSlot)` |
| **Return Type** | `void` |
| **Visibility** | `private` |
| **Projected CYC** | 6 |
| **Dependency** | T-1 must be complete (`ResolveSidebandKey` must exist) |

### Work Description

Extract the per-slot processing body from inside the photon ring while-loop. The extracted method performs: sideband key resolution via `ResolveSidebandKey`, conditional delta rollback via `AddExpectedPositionDeltaLocked`, `ClearDispatchSyncPending`, pool `ReleaseByIndex`, sideband entry reset to `default`, and `Interlocked.Decrement` of the pending count.

```csharp
private void DrainPhotonDispatchSlot(FleetDispatchSlot abortSlot)
{
    var sbKey = ResolveSidebandKey(abortSlot.SidebandIndex);
    if (sbKey != null)
        AddExpectedPositionDeltaLocked(sbKey, -abortSlot.ReservedDelta);
    ClearDispatchSyncPending(abortSlot.InstrumentKey);
    _photonPool?.ReleaseByIndex(abortSlot.PoolIndex);
    if (abortSlot.SidebandIndex >= 0 && abortSlot.SidebandIndex < _photonSideband?.Length)
        _photonSideband[abortSlot.SidebandIndex] = default;
    Interlocked.Decrement(ref _pendingFleetDispatchCount);
}
```

Replace the per-slot loop body with a single call to `DrainPhotonDispatchSlot(abortSlot)`.

### Acceptance Criteria

- [ ] `DrainPhotonDispatchSlot(FleetDispatchSlot abortSlot)` added as `private` method
- [ ] Per-slot loop body replaced with `DrainPhotonDispatchSlot(abortSlot)`
- [ ] Build passes: `dotnet build` zero errors
- [ ] xUnit `[Fact]` — slot with non-null sideband key triggers delta rollback
- [ ] xUnit `[Fact]` — slot with null sideband key skips delta rollback
- [ ] xUnit `[Fact]` — sideband entry reset to `default` after processing
- [ ] Projected CYC: 6

### Constraints

- No `lock()` blocks — use `Interlocked.Decrement` only
- `FleetDispatchSlot` passed by value (struct) — zero allocation
- ASCII-only string literals
- Do NOT modify callee files: `TrackPhotonDequeue`, `AddExpectedPositionDeltaLocked`, `ClearDispatchSyncPending`

---

## Ticket T-3: Extract `DrainPhotonDispatchRing`

| Field | Value |
|---|---|
| **Ticket ID** | T-3 |
| **Epic** | EPIC-W7-054 |
| **Type** | Extraction |
| **Source File** | `src/V12_002.SIMA.Fleet.cs` |
| **Target Lines** | Within 287–323 (photon ring while-loop with null-guard) |
| **Helper Name** | `DrainPhotonDispatchRing()` |
| **Return Type** | `void` |
| **Visibility** | `private` |
| **Projected CYC** | 3 |
| **Dependency** | T-2 must be complete (`DrainPhotonDispatchSlot` must exist) |

### Work Description

Extract the photon ring while-loop (including its null-guard on `_photonDispatchRing`) from `DrainAllDispatchQueuesOnAbort`. The extracted method calls `TrackPhotonDequeue()` and `DrainPhotonDispatchSlot(abortSlot)` per iteration.

```csharp
private void DrainPhotonDispatchRing()
{
    if (_photonDispatchRing == null) return;
    while (_photonDispatchRing.TryDequeue(out var abortSlot))
    {
        TrackPhotonDequeue();
        DrainPhotonDispatchSlot(abortSlot);
    }
}
```

Replace the photon ring loop block in `DrainAllDispatchQueuesOnAbort` with a single call to `DrainPhotonDispatchRing()`.

### Acceptance Criteria

- [ ] `DrainPhotonDispatchRing()` added as `private` method
- [ ] Photon ring loop block replaced with `DrainPhotonDispatchRing()`
- [ ] Build passes: `dotnet build` zero errors
- [ ] xUnit `[Fact]` — null `_photonDispatchRing` exits immediately (no exception)
- [ ] xUnit `[Fact]` — empty ring completes without iterating
- [ ] xUnit `[Fact]` — single-element ring calls `TrackPhotonDequeue` and `DrainPhotonDispatchSlot` once
- [ ] Projected CYC: 3

### Constraints

- No `lock()` blocks
- ASCII-only string literals
- Do NOT modify `TrackPhotonDequeue` in `src/V12_002.Telemetry.cs`

---

## Ticket T-4: Extract `DrainLegacyFleetDispatches` + Finalize Parent

| Field | Value |
|---|---|
| **Ticket ID** | T-4 |
| **Epic** | EPIC-W7-054 |
| **Type** | Extraction + Parent Finalization |
| **Source File** | `src/V12_002.SIMA.Fleet.cs` |
| **Target Lines** | Within 287–323 (legacy ConcurrentQueue while-loop; parent body after extraction) |
| **Helper Name** | `DrainLegacyFleetDispatches()` |
| **Return Type** | `void` |
| **Visibility** | `private` |
| **Projected CYC (helper)** | 3 |
| **Projected CYC (parent)** | 1 |
| **Dependency** | T-3 must be complete (`DrainPhotonDispatchRing` must exist) |

### Work Description

Extract the legacy `ConcurrentQueue<FleetDispatchRequest>` while-loop from `DrainAllDispatchQueuesOnAbort`. The extracted method performs per-item: conditional `ReservedDelta` rollback, `ClearDispatchSyncPending`, and `Interlocked.Decrement`.

```csharp
private void DrainLegacyFleetDispatches()
{
    while (_pendingFleetDispatches.TryDequeue(out var req))
    {
        if (req.ReservedDelta != 0)
            AddExpectedPositionDeltaLocked(req.InstrumentKey, -req.ReservedDelta);
        ClearDispatchSyncPending(req.InstrumentKey);
        Interlocked.Decrement(ref _pendingFleetDispatchCount);
    }
}
```

After extraction, finalize `DrainAllDispatchQueuesOnAbort` to its clean CYC 1 form:

```csharp
private void DrainAllDispatchQueuesOnAbort()
{
    DrainPhotonDispatchRing();
    DrainLegacyFleetDispatches();
    int finalCount = Volatile.Read(ref _pendingFleetDispatchCount);
    TryResetCircuitBreakerIfBelow(finalCount);
}
```

### Acceptance Criteria

- [ ] `DrainLegacyFleetDispatches()` added as `private` method
- [ ] Legacy ConcurrentQueue loop block replaced with `DrainLegacyFleetDispatches()`
- [ ] `DrainAllDispatchQueuesOnAbort` body matches the finalized CYC 1 form above
- [ ] Build passes: `dotnet build` zero errors
- [ ] xUnit `[Fact]` — empty queue completes without iterating
- [ ] xUnit `[Fact]` — item with non-zero `ReservedDelta` triggers rollback
- [ ] xUnit `[Fact]` — item with zero `ReservedDelta` skips rollback
- [ ] xUnit `[Fact]` — parent method calls all 3 delegates in order (ring, legacy, circuit breaker)
- [ ] Projected CYC (helper): 3
- [ ] Projected CYC (parent): 1

### Constraints

- No `lock()` blocks — use `Interlocked.Decrement` only
- `Volatile.Read` preserved in parent for `_pendingFleetDispatchCount` final read
- ASCII-only string literals
- Do NOT modify `TryResetCircuitBreakerIfBelow` at `src/V12_002.SIMA.Fleet.cs:420`
- Do NOT modify `AddExpectedPositionDeltaLocked` in `src/V12_002.SIMA.cs`

---

## Execution Order

```
T-1 (ResolveSidebandKey)
  └─► T-2 (DrainPhotonDispatchSlot)       [depends on T-1]
        └─► T-3 (DrainPhotonDispatchRing) [depends on T-2]
              └─► T-4 (DrainLegacyFleetDispatches + parent finalization) [depends on T-3]
```

Each ticket is independently buildable upon completion of its predecessor.

---

## CYC Projection Summary

| Method | Before | After | Threshold | Status |
|---|---|---|---|---|
| `DrainAllDispatchQueuesOnAbort` (parent) | 20 | 1 | ≤ 8 | ✅ PASS |
| `ResolveSidebandKey(int sbIdx)` | — | 3 | ≤ 8 | ✅ PASS |
| `DrainPhotonDispatchSlot(FleetDispatchSlot)` | — | 6 | ≤ 8 | ✅ PASS |
| `DrainPhotonDispatchRing()` | — | 3 | ≤ 8 | ✅ PASS |
| `DrainLegacyFleetDispatches()` | — | 3 | ≤ 8 | ✅ PASS |
| **max_cyc_projected** | — | **6** | ≤ 8 | ✅ PASS |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Epic** | EPIC-W7-054 |
| **Wave** | 7 |
| **Phase** | 4 |
| **Method** | `DrainAllDispatchQueuesOnAbort` |
| **Source File** | `src/V12_002.SIMA.Fleet.cs` |
| **Original CYC** | 20 |
| **Ticket Count** | 4 |
| **Max Projected CYC** | 6 |
| **Parent CYC After All Extractions** | 1 |
| **DNA Verdict (Phase 3)** | PASS |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | 2026-06-29T03:00:00Z |
| **jcodemunch tools called** | resolve_repo, get_symbol_complexity, get_extraction_candidates |
| **sequential-thinking calls** | 4 (1 probe + 3 validation) |
| **Output** | `docs/brain/EPIC-W7-054/04-tickets.md` |
