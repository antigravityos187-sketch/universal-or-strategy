# Phase 1: Scope Definition - EPIC-W7-114

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.0
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-23T21:52:18Z

---

## Method Under Refactoring

| Field | Value |
|---|---|
| **Method** | `ProcessShutdownSIMA` |
| **File** | `src/V12_002.SIMA.Lifecycle.cs` |
| **Line** | 98 |
| **Visibility** | `private void` |
| **Parameters** | 0 |
| **Current CYC** | 15 |
| **Target CYC** | ≤ 8 (Jane Street standard) |
| **Lines of Code** | 41 |
| **Only Caller** | `ProcessApplySimaState` (line 78, same file) |

### Current Method Body (Lines 98–138)

```csharp
private void ProcessShutdownSIMA()
{
    CancelAllV12GtcOrders(false); // [BUILD 948] GTC sweep before teardown -- skip accounts with open positions
    StopReaperAudit();
    UnsubscribeFromFleetAccounts();
    // v28.0 shutdown drain: sideband-aware, XorShadow-free
    {
        FleetDispatchSlot ringSlot;
        while (_photonDispatchRing != null && _photonDispatchRing.TryDequeue(out ringSlot))
        {
            int _sbIdx = ringSlot.PoolSlotIndex;
            string _expectedKey =
                (_sbIdx >= 0 && _sbIdx < _photonSideband.Length) ? _photonSideband[_sbIdx].ExpectedKey : null;
            if (ringSlot.ReservedDelta != 0 && _expectedKey != null)
                AddExpectedPositionDelta(_expectedKey, -ringSlot.ReservedDelta);
            if (_expectedKey != null)
                ClearDispatchSyncPending(_expectedKey);
            if (_sbIdx >= 0)
            {
                _photonPool.ReleaseByIndex(_sbIdx);
                if (_sbIdx < _photonSideband.Length)
                    _photonSideband[_sbIdx] = default(FleetDispatchSideband);
            }
        }
        Print("[SIMA] Photon ring cleared on shutdown with delta rollback.");
    }
    // A3-1: Drain ghost dispatch queue on SIMA disable (Build 960 audit fix)
    {
        FleetDispatchRequest ignored;
        while (_pendingFleetDispatches.TryDequeue(out ignored))
        {
            if (ignored.ReservedDelta != 0)
                AddExpectedPositionDelta(ignored.ExpectedKey, -ignored.ReservedDelta);
            ClearDispatchSyncPending(ignored.ExpectedKey);
        }
        Print("[SIMA] Dispatch queue cleared on shutdown with delta rollback.");
    }
    Print("[SIMA LIFECYCLE] SIMA DISABLED -- Reaper stopped, handlers unsubscribed");
}
```

### CYC Decision Points (15 total)

1. `while (_photonDispatchRing != null && ...)` — null-check guard (2 points: null-check + while)
2. `_photonDispatchRing.TryDequeue(...)` — loop continuation
3. `(_sbIdx >= 0 && _sbIdx < _photonSideband.Length)` — ternary guard (2 points)
4. `if (ringSlot.ReservedDelta != 0 && _expectedKey != null)` — compound branch (2 points)
5. `if (_expectedKey != null)` — null check
6. `if (_sbIdx >= 0)` — index guard
7. `if (_sbIdx < _photonSideband.Length)` — bounds guard
8. `while (_pendingFleetDispatches.TryDequeue(...))` — loop
9. `if (ignored.ReservedDelta != 0)` — delta guard
10. Method entry point (1)

**Total: 15** — exceeds ≤8 threshold by 87.5%.

---

## IN SCOPE

### Extractions to Create (3 helper methods)

All three helpers are `private void`, parameterless, instance methods on the same partial class.  
They exist **solely** to decompose `ProcessShutdownSIMA`; they are not reused elsewhere.

#### Helper 1 — `ShutdownSIMA_CancelAndUnsubscribe()`
- **Responsibility**: Pre-drain teardown — cancel open GTC orders, stop the Reaper audit, unsubscribe fleet accounts.
- **Extracted lines** (current 100–102):
  ```csharp
  CancelAllV12GtcOrders(false);
  StopReaperAudit();
  UnsubscribeFromFleetAccounts();
  ```
- **Estimated CYC**: 1 (straight-line, no branches)
- **Rationale**: Three sequential, thematically grouped teardown calls with no shared state.

#### Helper 2 — `ShutdownSIMA_DrainPhotonRing()`
- **Responsibility**: Drain the `_photonDispatchRing`, roll back `ReservedDelta` on each slot, clear dispatch-sync barriers, and release pool indices.
- **Extracted lines** (current 106–124):
  ```csharp
  FleetDispatchSlot ringSlot;
  while (_photonDispatchRing != null && _photonDispatchRing.TryDequeue(out ringSlot))
  {
      int _sbIdx = ringSlot.PoolSlotIndex;
      string _expectedKey = ...;
      if (ringSlot.ReservedDelta != 0 && _expectedKey != null)
          AddExpectedPositionDelta(_expectedKey, -ringSlot.ReservedDelta);
      if (_expectedKey != null)
          ClearDispatchSyncPending(_expectedKey);
      if (_sbIdx >= 0) { ... }
  }
  Print("[SIMA] Photon ring cleared on shutdown with delta rollback.");
  ```
- **Estimated CYC**: 8 (while + null-guard + ternary-2pts + compound-if-2pts + null-if + idx-if + bounds-if)
- **Rationale**: Entire sideband-aware ring-drain block is a self-contained unit with its own local variables.

#### Helper 3 — `ShutdownSIMA_DrainPendingDispatches()`
- **Responsibility**: Drain the `_pendingFleetDispatches` queue, roll back delta and clear sync barrier for each discarded request.
- **Extracted lines** (current 128–136):
  ```csharp
  FleetDispatchRequest ignored;
  while (_pendingFleetDispatches.TryDequeue(out ignored))
  {
      if (ignored.ReservedDelta != 0)
          AddExpectedPositionDelta(ignored.ExpectedKey, -ignored.ReservedDelta);
      ClearDispatchSyncPending(ignored.ExpectedKey);
  }
  Print("[SIMA] Dispatch queue cleared on shutdown with delta rollback.");
  ```
- **Estimated CYC**: 3 (while + if + entry)
- **Rationale**: Mirrors the ring-drain pattern but for the pending-dispatch queue; isolated local variable scope.

### Resulting `ProcessShutdownSIMA` After Extraction

```csharp
private void ProcessShutdownSIMA()
{
    ShutdownSIMA_CancelAndUnsubscribe();
    ShutdownSIMA_DrainPhotonRing();
    ShutdownSIMA_DrainPendingDispatches();
    Print("[SIMA LIFECYCLE] SIMA DISABLED -- Reaper stopped, handlers unsubscribed");
}
```

- **Resulting CYC of `ProcessShutdownSIMA`**: 1 (straight-line orchestrator)
- **All helpers CYC**: ≤ 8 ✓
- **Total CYC budget consumed**: 1 + 1 + 8 + 3 = 13 (distributed; max single method = 8 ✓)

---

## OUT OF SCOPE

| Item | Reason |
|---|---|
| Signature of `ProcessShutdownSIMA` | Must remain `private void ProcessShutdownSIMA()` — unchanged |
| Behavior / call order | All three helper calls must preserve the exact original call sequence |
| `ProcessApplySimaState` | Caller is untouched; it continues to call `ProcessShutdownSIMA()` with no changes |
| `ProcessInitializeSIMA` | Symmetric init method; not part of this epic |
| `CancelAllV12GtcOrders`, `StopReaperAudit`, `UnsubscribeFromFleetAccounts` | Callee implementations are not modified |
| `AddExpectedPositionDelta`, `ClearDispatchSyncPending` | Callee implementations are not modified |
| `_photonPool`, `_photonDispatchRing`, `_photonSideband`, `_pendingFleetDispatches` | Field declarations and types are not modified |
| Any other method in `src/V12_002.SIMA.Lifecycle.cs` | Only the three new helpers + the refactored orchestrator are added/changed |
| Any file outside `src/V12_002.SIMA.Lifecycle.cs` | Zero cross-file modifications |
| Logging strings / Print messages | Preserved verbatim in their respective extracted methods |
| Comments (build tags, audit notes) | Preserved verbatim; moved with their code block |

---

## Extraction Plan

### Step-by-Step

1. **Read** `src/V12_002.SIMA.Lifecycle.cs` lines 98–138 for exact text.
2. **Insert** `ShutdownSIMA_CancelAndUnsubscribe()` immediately after line 138 (after `ProcessShutdownSIMA` closing brace).
3. **Insert** `ShutdownSIMA_DrainPhotonRing()` after `ShutdownSIMA_CancelAndUnsubscribe`.
4. **Insert** `ShutdownSIMA_DrainPendingDispatches()` after `ShutdownSIMA_DrainPhotonRing`.
5. **Replace** the body of `ProcessShutdownSIMA` (lines 99–137) with the three delegation calls + final Print.
6. **Verify** that all original Print messages, build-tag comments, and local variable names are present (unchanged) inside the correct helper.

### Proposed Insertion Region

New helpers are placed directly below `ProcessShutdownSIMA` within the `#region V12 SIMA Lifecycle` block, keeping related lifecycle methods contiguous.

### Call Order Invariant

```
ProcessShutdownSIMA()
  └─ ShutdownSIMA_CancelAndUnsubscribe()   // 1st: orders → reaper → fleet
  └─ ShutdownSIMA_DrainPhotonRing()        // 2nd: ring drain with delta rollback
  └─ ShutdownSIMA_DrainPendingDispatches() // 3rd: queue drain with delta rollback
  └─ Print(...)                            // final status log (stays in orchestrator)
```

---

## Risk Assessment

| Risk | Severity | Mitigation |
|---|---|---|
| Incorrect extraction boundary splits a local variable scope | HIGH | Each extracted block uses its own `FleetDispatchSlot`/`FleetDispatchRequest` local; no shared locals across blocks |
| Call order changed between helpers | HIGH | Phase 2 must apply helpers in exact documented order; scope document is authoritative |
| Instance field access broken by extraction | LOW | All three helpers are instance methods on the same partial class; all fields remain accessible |
| Delta rollback semantics silently dropped | HIGH | Every `AddExpectedPositionDelta` and `ClearDispatchSyncPending` call is accounted for in the helper it belongs to |
| Blast radius to other files | NONE | CYC=0 external dependents confirmed by Phase 0 |
| Compile break from missing `using` / type resolution | LOW | No new types introduced; helpers use existing types already in scope |

**Overall Residual Risk: LOW** — zero external blast radius, clear extraction boundaries, no shared mutable locals across the two drain blocks.

---

## Success Criteria

| Criterion | Measurable Target |
|---|---|
| `ProcessShutdownSIMA` CYC | = 1 (straight-line orchestrator with 3 calls + 1 Print) |
| `ShutdownSIMA_CancelAndUnsubscribe` CYC | = 1 |
| `ShutdownSIMA_DrainPhotonRing` CYC | ≤ 8 |
| `ShutdownSIMA_DrainPendingDispatches` CYC | ≤ 8 |
| No behavioral change | All original Print strings, build-tag comments, and call sequences preserved verbatim |
| No signature change | `ProcessShutdownSIMA` remains `private void ProcessShutdownSIMA()` |
| No cross-file changes | Diff touches only `src/V12_002.SIMA.Lifecycle.cs` |
| Compilation | Zero new errors or warnings introduced |
| Caller unchanged | `ProcessApplySimaState` body is byte-for-byte identical after refactoring |
