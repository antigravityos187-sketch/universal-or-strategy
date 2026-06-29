# EPIC-W7-114 — Phase 0: Hotspot Analysis

## Method Metadata

| Field        | Value                                    |
|------------- |------------------------------------------|
| Method Name  | `ProcessShutdownSIMA`                    |
| Reported CYC | 0 (tooling miss — see note below)        |
| Observed CYC | **8** (manually counted from source)     |
| File Path    | `src/V12_002.SIMA.Lifecycle.cs`          |
| Line Range   | 98 – 138                                 |
| Visibility   | `private void`                           |
| Class        | `V12_002 : Strategy` (partial)           |

> **⚠ CYC=0 Note:** The automated tool reported CYC=0, indicating the symbol was not
> located by the static analyzer (likely a partial-class spanning multiple files). A
> direct source read confirmed the method exists at line 98. Manual McCabe analysis
> yields **CYC=8**. This artifact documents the grounded value and flags the tooling
> gap for calibration.

---

## Blast Radius Summary

`ProcessShutdownSIMA` is called exclusively from
`ProcessApplySimaState(bool enabled)` on the `else` branch (line 78), which itself
is reached via `TriggerCustomEvent` or the strategy actor thread. Downstream surface
touched on every invocation:

| Callsite / Resource            | Risk Level | Notes                                              |
|--------------------------------|------------|----------------------------------------------------|
| `CancelAllV12GtcOrders(false)` | HIGH       | Issues live order cancellations; affects all GTC brackets |
| `StopReaperAudit()`            | MEDIUM     | Halts safety watchdog; exposes gap window          |
| `UnsubscribeFromFleetAccounts()` | HIGH     | Tears down all execution/order update handlers     |
| `_photonDispatchRing` drain    | MEDIUM     | Modifies concurrent ring buffer; delta rollback    |
| `AddExpectedPositionDelta()`   | HIGH       | Mutates expectedPositions ledger under teardown    |
| `ClearDispatchSyncPending()`   | MEDIUM     | Clears IPC barriers; affects fleet sync            |
| `_photonPool.ReleaseByIndex()` | MEDIUM     | Returns pool slots; pool corruption on double-free |
| `_pendingFleetDispatches` drain | MEDIUM    | Drains second concurrent queue; delta rollback     |

**Blast radius: WIDE** — touches live order management, account event subscriptions,
two concurrent queues, a memory pool, and the position-ledger in a single synchronous
call with no recovery path on partial failure.

---

## Top 3 Complexity Drivers

### 1 · Photon Ring Drain Loop with Cascaded Guards (lines 107–122, CYC +5)

```csharp
while (_photonDispatchRing != null && _photonDispatchRing.TryDequeue(out ringSlot))
{
    int _sbIdx = ringSlot.PoolSlotIndex;
    string _expectedKey = (_sbIdx >= 0 && _sbIdx < _photonSideband.Length) ? ... : null;
    if (ringSlot.ReservedDelta != 0 && _expectedKey != null)   // +1 (compound)
        AddExpectedPositionDelta(...);
    if (_expectedKey != null)                                   // +1
        ClearDispatchSyncPending(...);
    if (_sbIdx >= 0)                                            // +1
    {
        _photonPool.ReleaseByIndex(_sbIdx);
        if (_sbIdx < _photonSideband.Length)                    // +1
            _photonSideband[_sbIdx] = default(...);
    }
}   // while +1
```

Four distinct conditional guards live *inside* the hot loop. Any one failing silently
skips rollback steps (delta leak, pool leak, or sync barrier left open). The loop has
no try/catch, so a single exception aborts the entire drain.

### 2 · Dual-Queue Teardown Without Shared Abstraction (lines 107–136, structural)

The method contains **two logically identical drain-and-rollback loops** for
`_photonDispatchRing` (ring buffer / `FleetDispatchSlot`) and
`_pendingFleetDispatches` (concurrent queue / `FleetDispatchRequest`). They share the
pattern `TryDequeue → check ReservedDelta → rollback delta → clear sync pending`, but
are written as separate inline blocks with no shared helper. Any future change to
rollback logic must be applied twice, creating drift risk.

### 3 · Sequential Ordering Dependency Across Method Steps (lines 100–102, implicit coupling)

The three teardown calls at the top — `CancelAllV12GtcOrders` → `StopReaperAudit` →
`UnsubscribeFromFleetAccounts` — have a strict safety ordering that is **not enforced
by the type system** and is only preserved by source position. If any step throws, the
subsequent steps are skipped silently (no try/finally wrapping the teardown sequence),
leaving SIMA in a partially-disabled state (e.g., Reaper stopped but handlers still
subscribed).

---

## Recommended Extraction Count

| Extraction | Proposed Name                     | Responsibility                                              | CYC Reduction |
|------------|-----------------------------------|-------------------------------------------------------------|---------------|
| 1          | `DrainPhotonRingWithRollback()`   | Encapsulate ring-buffer drain loop (Driver 1)               | −5            |
| 2          | `DrainPendingDispatchesWithRollback()` | Encapsulate queue drain loop (Driver 2, second block)  | −2            |
| 3          | `TeardownFleetConnections()`      | Wrap the ordered Cancel→Stop→Unsub triplet in try/finally  | −0 (safety)   |

**Recommended extractions: 3**

Post-extraction `ProcessShutdownSIMA` becomes a 4-line sequencer with CYC=1:

```csharp
private void ProcessShutdownSIMA()
{
    TeardownFleetConnections();
    DrainPhotonRingWithRollback();
    DrainPendingDispatchesWithRollback();
    Print("[SIMA LIFECYCLE] SIMA DISABLED -- Reaper stopped, handlers unsubscribed");
}
```

---

## Flags for Manual Review

- [ ] **CYC=0 tooling gap** — analyzer did not locate symbol; calibration ticket recommended
- [ ] **No exception isolation** in the two drain loops — a pool exception will leave the second queue un-drained
- [ ] **No partial-disable guard** — if `CancelAllV12GtcOrders` throws, Reaper and handlers remain active

---

## Agent Tracking

| Field           | Value                    |
|-----------------|--------------------------|
| Agent Name      | v12-phase0-hotspot       |
| Bobcoins Used   | 14                       |
| Execution Time  | ~42s                     |
| Analysis Method | Direct source read + manual McCabe count (MCP tools returned CYC=0 / symbol not found) |
| Status          | ✅ Completed — requires manual review for CYC tooling gap |
