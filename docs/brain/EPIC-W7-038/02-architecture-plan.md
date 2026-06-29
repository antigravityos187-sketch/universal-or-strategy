# Phase 2: Architecture Plan -- EPIC-W7-038

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 -- Architecture Planning
**Generated:** 2026-06-29T01:10:00Z
**Input:** docs/brain/EPIC-W7-038/01-scope-boundary.md

---

## Method Under Extraction

- **Method:** `VerifyPhotonSlotIntegrity`
- **Source File:** `src/V12_002.SIMA.Fleet.cs`
- **Lines:** 329-389
- **Original CYC:** 9
- **Signature:** `private bool VerifyPhotonSlotIntegrity(ref FleetDispatchSlot _ringSlot, FleetDispatchSideband _sb, int _sbIdx)`

### jcodemunch get_context_bundle result

Symbol resolved: `src/V12_002.SIMA.Fleet.cs::V12_002.VerifyPhotonSlotIntegrity#method` (line 329, end_line 389).
Full source obtained. Key findings:
- XorShadow mutation-recompute-restore on `_ringSlot.Shadow` (transient mutation, no branch)
- One outer integrity gate: `if (_recomputed != _stored)` -- the root complexity driver
- Failure path: TrackPhotonCrcFailure + Print + 7-branch rollback block + pump-reprime try/catch
- Rollback block contains: conditional delta rollback (&&), conditional sync-clear, conditional dict removes (3 direct + 5-target for-loop + follower), conditional pool release + sideband clear, Interlocked.Decrement, circuit-breaker reset
- Pump-reprime: compound-condition if + try/catch TriggerCustomEvent

### jcodemunch get_call_hierarchy result

- **Direct callers (depth 1):**
  - `PumpFleetDispatch` at `src/V12_002.SIMA.Fleet.cs:233` (ast_resolved) -- ring consumer hot path
- **Depth-2 callers:**
  - `ProcessFleetSlot` at `src/V12_002.SIMA.Fleet.cs:44` (ast_resolved)
- **Key callees (depth 1):**
  - `ComputeFleetDispatchShadow` (Photon.Pool.cs:352) -- shadow recompute
  - `TrackPhotonCrcFailure` (Telemetry.cs:179) -- failure telemetry
  - `AddExpectedPositionDeltaLocked` (SIMA.cs:88) -- delta rollback
  - `ClearDispatchSyncPending` (SIMA.cs:179) -- sync state clear
  - `GetTargetOrdersDictionary` (UI.Callbacks.cs:1039) -- 5-target dict access
  - `TryResetCircuitBreakerIfBelow` (SIMA.Fleet.cs:420) -- circuit breaker
  - `PumpFleetDispatch` (SIMA.Fleet.cs:233) -- re-scheduled via TriggerCustomEvent

### jcodemunch get_dependency_graph result

- **Node count:** 1 | **Edge count:** 0
- `src/V12_002.SIMA.Fleet.cs` has no import-graph edges at depth 1 (all dependencies within same partial class file boundary)
- No cross-file import mutations introduced by this refactor

### jcodemunch get_extraction_candidates result

- No candidates returned (index requires min_callers=1 with tracked complexity data)
- Manual analysis from source confirms 4 extraction candidates (see Extraction Plan below)

---

## Sequential Thinking Summary

**Thought 1:** Confirmed CYC=9 by counting 9 McCabe decision points in source: outer integrity if, compound-&& delta if (counts as 2), ExpectedKey guard, FleetEntryName guard, for-loop, td-null guard, sbIdx>=0 guard, sbIdx<len guard, compound-|| pump-reprime if.

**Thought 2:** Initial 2-helper plan (RollbackPhotonSlotState + TryReprimePump) analyzed -- rollback helper alone would carry CYC 9, violating the <=8 mandate. Decomposition into sub-helpers required.

**Thought 3:** Revised to 4-helper plan splitting the rollback into RollbackStateEntries (dict/follower removals) and RollbackSlotResources (delta, sync, pool, counter, circuit breaker). Added LogIntegrityFailure to isolate telemetry+logging from control flow. CYC verified for all helpers.

**Thought 4:** Validated all CYC projections: LogIntegrityFailure=1, RollbackStateEntries=4, RollbackSlotResources=6, TryReprimePump=3, parent=2. Max=6. All <=8. Jane Street compliance CONFIRMED.

**Thought 5 (final):** All helpers satisfy single-responsibility, private scope, ASCII-only strings, zero lock blocks, no illegal states. Extraction_count=4. Caller signature unchanged. Scope confined to VerifyPhotonSlotIntegrity per V12.23. Architecture plan complete.

---

## Extraction Plan

| Helper Method Name | Signature | Responsibility | Projected CYC |
|---|---|---|---|
| `LogIntegrityFailure` | `private void LogIntegrityFailure(ulong stored, ulong recomputed, string entryName)` | Calls TrackPhotonCrcFailure and Print with format string. No branches. | **1** |
| `RollbackStateEntries` | `private void RollbackStateEntries(FleetDispatchSideband _sb)` | Removes activePositions, entryOrders, stopOrders, 5 target-order dicts (for+if guard), and _followerBrackets -- all guarded by if(_sb.FleetEntryName != null) | **4** |
| `RollbackSlotResources` | `private void RollbackSlotResources(FleetDispatchSideband _sb, int _sbIdx, int _reservedDelta)` | Conditional delta rollback (if A&&B), conditional sync-clear (if ExpectedKey != null), conditional pool release + sideband clear (if _sbIdx >= 0 + bounds guard), Interlocked.Decrement, Volatile.Read, TryResetCircuitBreakerIfBelow | **6** |
| `TryReprimePump` | `private void TryReprimePump()` | Compound-condition guard (if ring or queue non-empty), try/catch TriggerCustomEvent(o => PumpFleetDispatch(), null), logs pump-prime failure on catch | **3** |

### CYC Decision Point Breakdown

**`LogIntegrityFailure` (CYC 1):** base=1. No branches.

**`RollbackStateEntries` (CYC 4):** base=1, +1 if(_sb.FleetEntryName != null), +1 for(tNum 1..5), +1 if(td != null) = 4.

**`RollbackSlotResources` (CYC 6):** base=1, +2 if(reservedDelta != 0 && ExpectedKey != null) [McCabe: && adds 1 extra], +1 if(ExpectedKey != null), +1 if(_sbIdx >= 0), +1 if(_sbIdx < _photonSideband.Length) = 6.

**`TryReprimePump` (CYC 3):** base=1, +1 if(!ring.IsEmpty || !queue.IsEmpty) [|| compound adds 1], +1 implicit catch-branch = 3.

---

## Parent Method After Extraction

**`VerifyPhotonSlotIntegrity` reduced body:**
```csharp
private bool VerifyPhotonSlotIntegrity(ref FleetDispatchSlot _ringSlot, FleetDispatchSideband _sb, int _sbIdx)
{
    ulong _stored = _ringSlot.Shadow;
    _ringSlot.Shadow = 0UL;
    ulong _recomputed = ComputeFleetDispatchShadow(ref _ringSlot, _photonShadowSalt);
    _ringSlot.Shadow = _stored;
    if (_recomputed != _stored)
    {
        LogIntegrityFailure(_stored, _recomputed, _sb.FleetEntryName);
        RollbackSlotResources(_sb, _sbIdx, _ringSlot.ReservedDelta);
        RollbackStateEntries(_sb);
        TryReprimePump();
        return false;
    }
    return true;
}
```

- **Remaining logic:** Shadow zero/recompute/restore (no branch), single integrity gate, 4 helper calls, two return paths
- **Projected CYC:** **2** (base=1, +1 for if mismatch gate)

---

## max_cyc_projected: 6

## extraction_count: 4

---

## Jane Street Alignment

| Principle | Status | Notes |
|---|---|---|
| CYC<=8 achieved | **YES** | Parent=2, helpers: 1, 4, 6, 3 -- max=6 |
| Single-responsibility per helper | **YES** | Each helper does exactly one thing: log, remove-entries, release-resources, reprime-pump |
| Lock-free / Actor pattern preserved | **YES** | Interlocked.Decrement and Volatile.Read retained in RollbackSlotResources; no lock blocks anywhere |
| Illegal states unrepresentable | **YES** | Rollback sequence encapsulated -- partial-rollback states (copy-paste drift) are structurally eliminated |
| ASCII-only string literals | **YES** | All format strings in source are ASCII; no Unicode or curly quotes |
| xUnit [Fact] tests per helper | **REQUIRED** | Phase 5 must produce xUnit [Fact] for: LogIntegrityFailure, RollbackStateEntries, RollbackSlotResources, TryReprimePump |
| ONE method per epic | **YES** | Only VerifyPhotonSlotIntegrity extracted; no sibling methods touched |
| Caller signature unchanged | **YES** | PumpFleetDispatch call at line 258/233 unaffected |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Epic** | EPIC-W7-038 |
| **Wave** | 7 |
| **Phase** | 2 -- Architecture Planning |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | 2026-06-29T01:10:00Z |
| **jcodemunch tools called** | resolve_repo, get_context_bundle, search_symbols (fallback), get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **Output File** | docs/brain/EPIC-W7-038/02-architecture-plan.md |
