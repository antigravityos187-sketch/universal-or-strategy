# EPIC-W7-108 — Phase 4 Tickets

**Method**: `DrainPhotonQueuesOnShutdown`
**Source**: `src/V12_002.SIMA.Lifecycle.cs`
**CYC**: 0 (parse artefact — method is inlined inside `ProcessShutdownSIMA`; inline body CYC ≈8 per static analysis)
**Lane**: P4-L7
**DNA Verdict**: PASS
**max_cyc_projected**: 6

> ⚠️ **DUPLICATE EPIC WARNING**: EPIC-W7-055 targets the identical inline body in `ProcessShutdownSIMA`.
> Phase 5 engineer MUST confirm with Wave 7 coordinator which ticket is active before execution.
> Execute ONLY ONE of W7-055 or W7-108. Executing both produces conflicting commits.

---

## Ticket Summary

| # | Ticket | Type | Methods Produced | CYC Target |
|---|--------|------|-----------------|-----------|
| 1 | Extract `DrainPhotonQueuesOnShutdown` from `ProcessShutdownSIMA` | extraction | `DrainPhotonQueuesOnShutdown()` | ≤1 |
| 2 | Extract `DrainPhotonRing` + `ReleasePhotonSlot` from `DrainPhotonQueuesOnShutdown` | extraction | `DrainPhotonRing()`, `ReleasePhotonSlot(FleetDispatchSlot)` | ≤6 |
| 3 | Extract `DrainLegacyDispatchQueue` from `DrainPhotonQueuesOnShutdown` | extraction | `DrainLegacyDispatchQueue()` | ≤3 |

---

## Ticket 1 — Extract `DrainPhotonQueuesOnShutdown` Orchestrator

**Type**: extraction
**Target CYC**: ≤1
**Methods Produced**: `DrainPhotonQueuesOnShutdown()`
**Source File**: `src/V12_002.SIMA.Lifecycle.cs`
**Prerequisite**: Duplicate-epic coordination completed (W7-055 vs W7-108).

### Objective

Move both inline drain blocks (photon ring drain + legacy dispatch queue drain) from `ProcessShutdownSIMA` into a new `private void DrainPhotonQueuesOnShutdown()`. Replace the two inline blocks in `ProcessShutdownSIMA` with a single call: `DrainPhotonQueuesOnShutdown();`.

### What to Extract

Inline body in `ProcessShutdownSIMA` (approximately lines 104–136 of `src/V12_002.SIMA.Lifecycle.cs`):
- The `while (_photonDispatchRing ...)` block
- The `while (_pendingFleetDispatches ...)` block
- Any `Print(...)` log calls scoped to the drain phase

### New Method Signature

```csharp
private void DrainPhotonQueuesOnShutdown()
{
    // Step 1 body: both drain blocks moved here verbatim
    // (ring loop + legacy queue loop — not yet decomposed into helpers)
}
```

### ProcessShutdownSIMA After Extraction

```csharp
// ... CancelAllV12GtcOrders, StopReaperAudit, UnsubscribeFromFleetAccounts ...
DrainPhotonQueuesOnShutdown();
// ... final Print ...
```

### Acceptance Criteria

- [ ] `DrainPhotonQueuesOnShutdown` exists as a standalone `private void` method in `src/V12_002.SIMA.Lifecycle.cs`
- [ ] `ProcessShutdownSIMA` calls `DrainPhotonQueuesOnShutdown()` — no inline drain logic remains in the parent
- [ ] CYC of `DrainPhotonQueuesOnShutdown` after Ticket 1 ≤8 (exact target: will be reduced to 1 after Tickets 2+3)
- [ ] `dotnet build` passes with zero errors
- [ ] Zero `lock(` blocks introduced
- [ ] All identifiers ASCII-only

### Jane Street Alignment

- Lock-free preserved: `TryDequeue` pattern unchanged
- No heap allocations introduced: value-type structs passed by value
- Single-responsibility: method owns drain orchestration only

---

## Ticket 2 — Extract `DrainPhotonRing` + `ReleasePhotonSlot`

**Type**: extraction
**Target CYC**: `DrainPhotonRing` ≤2, `ReleasePhotonSlot` ≤6
**Methods Produced**: `DrainPhotonRing()`, `ReleasePhotonSlot(FleetDispatchSlot slot)`
**Source File**: `src/V12_002.SIMA.Lifecycle.cs`
**Depends On**: Ticket 1 completed and build passing.

### Objective

Decompose the `while (_photonDispatchRing ...)` block inside `DrainPhotonQueuesOnShutdown` into two focused helpers:
1. `DrainPhotonRing()` — owns the ring iteration loop and the `Print` log call for ring draining
2. `ReleasePhotonSlot(FleetDispatchSlot slot)` — owns per-slot processing (extracted from the body of the `while` loop in `DrainPhotonRing`)

### Extraction Step 2a — `DrainPhotonRing`

Move the `while (_photonDispatchRing.TryDequeue(out var ringSlot)) { ... }` block from `DrainPhotonQueuesOnShutdown` into:

```csharp
private void DrainPhotonRing()
{
    while (_photonDispatchRing.TryDequeue(out var ringSlot))
    {
        ReleasePhotonSlot(ringSlot);
    }
    Print("PhotonRing drained on shutdown");
}
```

### Extraction Step 2b — `ReleasePhotonSlot`

Move the per-slot body (computing `_sbIdx` + `_expectedKey`, rolling back `ReservedDelta`, clearing `ClearDispatchSyncPending`, releasing pool index, zeroing `_photonSideband[_sbIdx]`) into:

```csharp
private void ReleasePhotonSlot(FleetDispatchSlot slot)
{
    var sbIdx = slot.PhotonSidebandIndex;
    if (sbIdx < 0) return;                          // guard clause — early return
    var expectedKey = slot.ExpectedKey;
    if (expectedKey == null) return;                // guard clause — early return
    AddExpectedPositionDelta(slot.ReservedDelta * -1);
    ClearDispatchSyncPending(expectedKey);
    _photonPool.ReleaseByIndex(sbIdx);
    _photonSideband[sbIdx] = default;
}
```

### DrainPhotonQueuesOnShutdown After Ticket 2

```csharp
private void DrainPhotonQueuesOnShutdown()
{
    DrainPhotonRing();
    // legacy queue drain still inline here (removed in Ticket 3)
}
```

### Acceptance Criteria

- [ ] `DrainPhotonRing()` exists as `private void` with CYC ≤2
- [ ] `ReleasePhotonSlot(FleetDispatchSlot slot)` exists as `private void` with CYC ≤6
- [ ] `DrainPhotonRing` body calls `ReleasePhotonSlot(ringSlot)` inside the while loop
- [ ] Guard clauses in `ReleasePhotonSlot` use early returns (not nested if-else)
- [ ] `dotnet build` passes with zero errors
- [ ] Zero `lock(` blocks introduced
- [ ] `FleetDispatchSlot` passed as value type — no boxing

### Jane Street Alignment

- Extract loop body pattern: `DrainPhotonRing` delegates per-slot work to `ReleasePhotonSlot`
- Guard clauses as early returns: `_sbIdx < 0` and null key checks expressed as `if (...) return`
- Illegal states unrepresentable: bounds checks encapsulated in `ReleasePhotonSlot` — callers cannot bypass guard

---

## Ticket 3 — Extract `DrainLegacyDispatchQueue` + Final Validation

**Type**: extraction
**Target CYC**: ≤3
**Methods Produced**: `DrainLegacyDispatchQueue()`
**Source File**: `src/V12_002.SIMA.Lifecycle.cs`
**Depends On**: Ticket 2 completed and build passing.

### Objective

Move the `while (_pendingFleetDispatches.TryDequeue(...)) { ... }` block from `DrainPhotonQueuesOnShutdown` into `private void DrainLegacyDispatchQueue()`. After this extraction, `DrainPhotonQueuesOnShutdown` becomes a pure sequential orchestrator with CYC=1.

### New Method Signature

```csharp
private void DrainLegacyDispatchQueue()
{
    while (_pendingFleetDispatches.TryDequeue(out var req))
    {
        AddExpectedPositionDelta(req.ReservedDelta * -1);
        ClearDispatchSyncPending(req.ExpectedKey);
    }
    Print("LegacyDispatchQueue drained on shutdown");
}
```

### DrainPhotonQueuesOnShutdown Final Form

```csharp
private void DrainPhotonQueuesOnShutdown()
{
    DrainPhotonRing();
    DrainLegacyDispatchQueue();
}
```

CYC of `DrainPhotonQueuesOnShutdown` after Ticket 3: **1** (no branches — pure sequential delegation).

### Final Validation Gate

After completing the extraction, run:

```bash
dotnet csharpier format src/
powershell -File .\scripts\pre_push_validation.ps1 -Fast
```

Verify complexity for all 4 extracted methods:

| Method | Target CYC | Acceptance |
|--------|-----------|-----------|
| `DrainPhotonQueuesOnShutdown` | ≤1 | PASS |
| `DrainPhotonRing` | ≤2 | PASS |
| `ReleasePhotonSlot` | ≤6 | PASS |
| `DrainLegacyDispatchQueue` | ≤3 | PASS |

### Acceptance Criteria

- [ ] `DrainLegacyDispatchQueue()` exists as `private void` with CYC ≤3
- [ ] `DrainPhotonQueuesOnShutdown` body contains exactly two calls: `DrainPhotonRing()` and `DrainLegacyDispatchQueue()` — no inline loop logic
- [ ] `DrainPhotonQueuesOnShutdown` CYC = 1
- [ ] `dotnet build` passes with zero errors
- [ ] `dotnet csharpier check src/` reports zero formatting issues
- [ ] `pre_push_validation.ps1 -Fast` passes all blocking checks
- [ ] Zero `lock(` blocks anywhere in `src/V12_002.SIMA.Lifecycle.cs`
- [ ] All string literals ASCII-only

### Jane Street Alignment

- Single-responsibility: `DrainLegacyDispatchQueue` owns legacy drain loop only
- Lock-free: `TryDequeue` on `_pendingFleetDispatches` — no lock() blocks
- Cognitive simplicity: `DrainPhotonQueuesOnShutdown` becomes a 2-line orchestrator — trivially auditable

---

## Implementation Sequence

```
Ticket 1  →  Ticket 2  →  Ticket 3
   ↓              ↓             ↓
 Build✅       Build✅     Build✅ + csharpier + pre-push
```

Each ticket MUST produce a passing build before the next begins.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Wave** | 7 |
| **Phase** | 4 — Ticket Generation |
| **Epic** | EPIC-W7-108 |
| **Method** | `DrainPhotonQueuesOnShutdown` (inline in `ProcessShutdownSIMA`) |
| **Source File** | `src/V12_002.SIMA.Lifecycle.cs` |
| **CYC (reported)** | 0 (parse artefact) |
| **CYC (inline body estimate)** | ≈8 |
| **max_cyc_projected** | 6 (`ReleasePhotonSlot`) |
| **extraction_count** | 3 helpers + 1 orchestrator |
| **ticket_count** | 3 |
| **jcodemunch tools called** | resolve_repo, get_symbol_complexity |
| **sequential-thinking calls** | 5 (1 probe + 4 planning thoughts) |
| **DNA Verdict** | PASS |
| **Duplicate Epic Flag** | EPIC-W7-055 — coordinate before Phase 5 execution |
| **Output** | docs/brain/EPIC-W7-108/04-tickets.md |
