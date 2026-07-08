# EPIC-W7-114 — Phase 4 Tickets

**Method**: `ProcessShutdownSIMA`
**Source**: `src/V12_002.SIMA.Lifecycle.cs`
**Class**: `V12_002 : Strategy` (partial)
**CYC**: 0 (parse artefact — true McCabe CYC=8, partial-class indexing gap)
**Lane**: P4-L7
**DNA Verdict**: PASS
**Wave**: 7

---

## Ticket Summary

| # | Ticket | Type | Helper Method | Projected CYC | Execution Order |
|---|--------|------|--------------|--------------|----------------|
| 1 | Extract TeardownFleetConnections | extraction | `TeardownFleetConnections` | 1 | First — must precede drain operations |
| 2 | Extract DrainPhotonRingWithRollback | extraction | `DrainPhotonRingWithRollback` | 5 | Second — highest complexity, pool management |
| 3 | Extract DrainPendingDispatchesWithRollback | extraction | `DrainPendingDispatchesWithRollback` | 2 | Third — lock-free queue drain |

**Parent after extraction**: CYC=1 (4 sequential calls)
**Max CYC across all helpers**: 5 (DrainPhotonRingWithRollback)
**All methods satisfy CYC≤8**: YES

---

## Ticket 1 — Extract TeardownFleetConnections

**Type**: extraction
**Target CYC**: ≤5 (projected: 1)
**Execution Order**: 1 of 3 — MUST execute before Tickets 2 and 3
**Risk**: Low
**Source File**: `src/V12_002.SIMA.Lifecycle.cs`

### Responsibility

Encapsulates the ordered teardown triplet from `ProcessShutdownSIMA`:

```csharp
CancelAllV12GtcOrders(false);   // Step 1: Cancel all GTC orders (stop new fills)
StopReaperAudit();               // Step 2: Stop the reaper audit monitor
UnsubscribeFromFleetAccounts(); // Step 3: Disconnect fleet account handlers
```

The ordering is a **safety constraint**: cancelling orders before stopping the audit before unsubscribing prevents partial-disable states where the audit monitor fires against already-cancelled orders, or fleet events fire after handlers are disconnected. Naming this helper makes the ordering contract explicit in the callsite.

### Target Signature

```csharp
private void TeardownFleetConnections()
{
    CancelAllV12GtcOrders(false);
    StopReaperAudit();
    UnsubscribeFromFleetAccounts();
}
```

### Parent Call Site After This Ticket

```csharp
private void ProcessShutdownSIMA()
{
    TeardownFleetConnections();
    // [photon ring drain inline — extracted in Ticket 2]
    // [pending dispatch drain inline — extracted in Ticket 3]
    Print("[SIMA LIFECYCLE] SIMA DISABLED -- Reaper stopped, handlers unsubscribed");
}
```

### Acceptance Criteria

- [ ] `TeardownFleetConnections` method exists in `src/V12_002.SIMA.Lifecycle.cs`
- [ ] Method is `private void`, no parameters
- [ ] Calls `CancelAllV12GtcOrders(false)`, `StopReaperAudit()`, `UnsubscribeFromFleetAccounts()` in that exact order
- [ ] `ProcessShutdownSIMA` calls `TeardownFleetConnections()` at the top of its body
- [ ] Build passes — zero errors, zero warnings
- [ ] No lock() blocks introduced (lock-free mandate)
- [ ] No Unicode introduced (ASCII-only mandate)
- [ ] CYC of `TeardownFleetConnections` = 1 (straight-line, no branches)

### V12 DNA Checks

| Check | Expected |
|-------|---------|
| Lock-free | No lock() blocks |
| ASCII-only | All string literals and identifiers ASCII |
| Scope creep | Single file, single method extraction |
| CYC target | ≤5 (projected 1) |

---

## Ticket 2 — Extract DrainPhotonRingWithRollback

**Type**: extraction
**Target CYC**: ≤8 (projected: 5)
**Execution Order**: 2 of 3 — after Ticket 1
**Risk**: Medium (pool management, sideband indexing, rollback semantics)
**Source File**: `src/V12_002.SIMA.Lifecycle.cs`

### Responsibility

Encapsulates the photon dispatch ring drain-and-rollback loop from `ProcessShutdownSIMA`. Iterates over `_photonDispatchRing` (array of `FleetDispatchSlot`). For each occupied slot:

1. Bounds-check the sideband index (conditional +1 CYC)
2. Rollback `ReservedDelta` via `AddExpectedPositionDelta` (conditional +1 CYC)
3. Clear the dispatch sync barrier via `ClearDispatchSyncPending` (conditional +1 CYC)
4. Release the pool slot via `_photonPool.ReleaseByIndex` (conditional +1 CYC)
5. Zero the sideband entry to prevent stale references

Loop itself adds +1 CYC → total projected CYC=5.

### Target Signature

```csharp
private void DrainPhotonRingWithRollback()
{
    // Iterates _photonDispatchRing, performs rollback + pool release per slot
    // 4 inner conditionals + 1 loop = CYC=5
}
```

### Parent Call Site After This Ticket

```csharp
private void ProcessShutdownSIMA()
{
    TeardownFleetConnections();
    DrainPhotonRingWithRollback();
    // [pending dispatch drain inline — extracted in Ticket 3]
    Print("[SIMA LIFECYCLE] SIMA DISABLED -- Reaper stopped, handlers unsubscribed");
}
```

### Acceptance Criteria

- [ ] `DrainPhotonRingWithRollback` method exists in `src/V12_002.SIMA.Lifecycle.cs`
- [ ] Method is `private void`, no parameters
- [ ] Drains `_photonDispatchRing` completely (all occupied slots processed)
- [ ] Rollback logic calls `AddExpectedPositionDelta` to reverse `ReservedDelta`
- [ ] Sync barrier cleared via `ClearDispatchSyncPending` per slot
- [ ] Pool slot released via `_photonPool.ReleaseByIndex` per slot
- [ ] Sideband entry zeroed after release
- [ ] `ProcessShutdownSIMA` calls `DrainPhotonRingWithRollback()` as second call
- [ ] Build passes — zero errors, zero warnings
- [ ] No lock() blocks introduced
- [ ] No Unicode introduced
- [ ] CYC of `DrainPhotonRingWithRollback` ≤ 8 (projected 5)

### V12 DNA Checks

| Check | Expected |
|-------|---------|
| Lock-free | ConcurrentQueue / pool patterns only, no lock() |
| ASCII-only | All literals ASCII |
| Scope creep | Single file, single method extraction |
| CYC target | ≤8 (projected 5) |
| Rollback correctness | ReservedDelta fully reversed for all drained slots |

---

## Ticket 3 — Extract DrainPendingDispatchesWithRollback

**Type**: extraction
**Target CYC**: ≤5 (projected: 2)
**Execution Order**: 3 of 3 — after Ticket 2
**Risk**: Low (lock-free TryDequeue, simple conditional)
**Source File**: `src/V12_002.SIMA.Lifecycle.cs`

### Responsibility

Encapsulates the pending fleet dispatch queue drain-and-rollback loop from `ProcessShutdownSIMA`. Uses `ConcurrentQueue.TryDequeue` (lock-free) to drain `_pendingFleetDispatches` (queue of `FleetDispatchRequest`). For each dequeued request:

1. If `ReservedDelta` is non-zero: rollback via `AddExpectedPositionDelta` (conditional +1 CYC)
2. Clear the dispatch sync barrier via `ClearDispatchSyncPending`

Loop adds +1 CYC → total projected CYC=2. Simpler than the photon ring drain — no pool release, no sideband management.

### Target Signature

```csharp
private void DrainPendingDispatchesWithRollback()
{
    // Lock-free TryDequeue loop over _pendingFleetDispatches
    // 1 loop + 1 conditional = CYC=2
}
```

### Parent After All Three Tickets

```csharp
private void ProcessShutdownSIMA()
{
    TeardownFleetConnections();
    DrainPhotonRingWithRollback();
    DrainPendingDispatchesWithRollback();
    Print("[SIMA LIFECYCLE] SIMA DISABLED -- Reaper stopped, handlers unsubscribed");
}
```

**Parent CYC = 1** (4 straight-line calls, no branches)

### Acceptance Criteria

- [ ] `DrainPendingDispatchesWithRollback` method exists in `src/V12_002.SIMA.Lifecycle.cs`
- [ ] Method is `private void`, no parameters
- [ ] Uses `ConcurrentQueue.TryDequeue` (lock-free pattern — no lock() block)
- [ ] Drains `_pendingFleetDispatches` completely until queue is empty
- [ ] Rollback logic calls `AddExpectedPositionDelta` when `ReservedDelta != 0`
- [ ] Sync barrier cleared via `ClearDispatchSyncPending` per request
- [ ] `ProcessShutdownSIMA` calls `DrainPendingDispatchesWithRollback()` as third call
- [ ] `ProcessShutdownSIMA` body is now exactly 4 calls: 3 helpers + 1 Print (CYC=1)
- [ ] Build passes — zero errors, zero warnings
- [ ] No lock() blocks introduced
- [ ] No Unicode introduced
- [ ] CYC of `DrainPendingDispatchesWithRollback` ≤ 5 (projected 2)
- [ ] CYC of `ProcessShutdownSIMA` = 1 after all extractions

### V12 DNA Checks

| Check | Expected |
|-------|---------|
| Lock-free | ConcurrentQueue.TryDequeue only, zero lock() blocks |
| ASCII-only | All literals ASCII |
| Scope creep | Single file, completes the 3-ticket extraction |
| CYC target | ≤5 (projected 2) |
| Parent CYC | 1 after all 3 helpers extracted |

---

## Execution Notes

**Critical ordering**: Ticket 1 (`TeardownFleetConnections`) MUST be executed before Tickets 2 and 3. The teardown triplet must be isolated first to clearly delimit the non-drain section of the method body, enabling clean isolation of the two drain loops in Tickets 2 and 3.

**Lock-free verification**: All three extractions must use zero `lock()` blocks. The `_pendingFleetDispatches` drain uses `ConcurrentQueue.TryDequeue` (lock-free). The `_photonDispatchRing` drain uses array iteration with pool release — no locking required.

**Scope**: All changes confined to `src/V12_002.SIMA.Lifecycle.cs`. Zero cross-file changes. Caller `ProcessApplySimaState` signature unchanged.

**Test mandate (V12.32)**: Any test scaffolding MUST use xUnit `[Fact]` / `Assert.Equal()`. Never NUnit or MSTest.

---

## Agent Tracking

| Field | Value |
|-------|-------|
| **Agent Name** | v12-phase4-tickets |
| **Wave** | 7 |
| **Epic** | EPIC-W7-114 |
| **Phase** | 4 — Ticket Generation |
| **Generated** | 2026-06-29 |
| **jcodemunch tools called** | resolve_repo, get_symbol_complexity |
| **sequential-thinking calls** | 4 (1 probe + 3 analysis thoughts) |
| **ticket_count** | 3 |
| **max_cyc_projected** | 5 |
| **dna_verdict** | PASS (from Phase 3) |
