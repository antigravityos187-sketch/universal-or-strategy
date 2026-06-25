# Phase 1: Scope Definition — EPIC-W7-108

**Agent**: v12-phase1-scope
**Epic**: EPIC-W7-108
**Date**: 2026-06-22
**Source**: `src/V12_002.SIMA.Lifecycle.cs`

---

## 1. Method Under Refactoring

### Target: `DrainPhotonQueuesOnShutdown`

The logical shutdown drain block currently lives **inline inside `ProcessShutdownSIMA()`**
(lines 98–138 of `src/V12_002.SIMA.Lifecycle.cs`). For the purposes of this epic, the
"method under refactoring" is the photon-queue drain segment of `ProcessShutdownSIMA`,
which carries the attributed CYC of **11** and must be reduced to **≤ 8**.

```csharp
private void ProcessShutdownSIMA()          // host method — see lines 98–138
{
    CancelAllV12GtcOrders(false);
    StopReaperAudit();
    UnsubscribeFromFleetAccounts();

    // ── SCOPE BOUNDARY START ─────────────────────────────────────────
    // Block A: Photon dispatch ring drain (sideband-aware, lock-free)
    {
        FleetDispatchSlot ringSlot;
        while (_photonDispatchRing != null && _photonDispatchRing.TryDequeue(out ringSlot))
        {
            int _sbIdx = ringSlot.PoolSlotIndex;
            string _expectedKey = (_sbIdx >= 0 && _sbIdx < _photonSideband.Length)
                ? _photonSideband[_sbIdx].ExpectedKey : null;
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

    // Block B: Pending fleet dispatch queue drain
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
    // ── SCOPE BOUNDARY END ───────────────────────────────────────────

    Print("[SIMA LIFECYCLE] SIMA DISABLED -- Reaper stopped, handlers unsubscribed");
}
```

### Complexity Drivers (CYC = 11)

| Driver | Branch count | Notes |
|--------|-------------|-------|
| `while` loop — ring drain | 1 | TryDequeue loop |
| `_photonDispatchRing != null` guard | 1 | null-check inside while condition |
| `if (ringSlot.ReservedDelta != 0 && _expectedKey != null)` | 2 | compound condition |
| `if (_expectedKey != null)` | 1 | second key null-check |
| `if (_sbIdx >= 0)` outer | 1 | index guard |
| `if (_sbIdx < _photonSideband.Length)` inner | 1 | bounds check |
| `while` loop — dispatch drain | 1 | TryDequeue loop |
| `if (ignored.ReservedDelta != 0)` | 1 | delta guard |
| Base path | 1 | base complexity = 1 |
| **Total** | **11** | |

---

## 2. IN SCOPE — Extractions

The goal is to bring the inline drain segment to **CYC ≤ 8** by extracting **two**
single-responsibility private helpers. This yields a net reduction of **≥ 3 CYC points**.

### Extraction 1 — `DrainPhotonDispatchRing()`

| Attribute | Value |
|-----------|-------|
| Target CYC | ≤ 5 |
| Lines extracted | Block A (approx. lines 106–124 in current file) |
| Responsibility | Drain `_photonDispatchRing`; roll back `ReservedDelta` and sideband for each slot; release pool slot |
| Callers after | `ProcessShutdownSIMA` (1 call site) |
| Data touched | `_photonDispatchRing`, `_photonPool`, `_photonSideband` |
| Calls made | `AddExpectedPositionDelta`, `ClearDispatchSyncPending`, `_photonPool.ReleaseByIndex` |
| Lock-free | Yes — no `lock()` blocks; TryDequeue is lock-free ring op |

Proposed signature:
```csharp
private void DrainPhotonDispatchRing()
```

### Extraction 2 — `DrainPendingFleetDispatches()`

| Attribute | Value |
|-----------|-------|
| Target CYC | ≤ 3 |
| Lines extracted | Block B (approx. lines 127–136 in current file) |
| Responsibility | Drain `_pendingFleetDispatches`; roll back `ReservedDelta`; clear dispatch-sync barrier |
| Callers after | `ProcessShutdownSIMA` (1 call site) |
| Data touched | `_pendingFleetDispatches` |
| Calls made | `AddExpectedPositionDelta`, `ClearDispatchSyncPending` |
| Lock-free | Yes — `ConcurrentQueue.TryDequeue` is lock-free |

Proposed signature:
```csharp
private void DrainPendingFleetDispatches()
```

### Post-extraction orchestrator shape

After extraction, the drain segment of `ProcessShutdownSIMA` reduces to:

```csharp
DrainPhotonDispatchRing();         // CYC contribution: 1
DrainPendingFleetDispatches();     // CYC contribution: 1
```

Leaving `ProcessShutdownSIMA` with a net **CYC ≤ 8** (sequential calls replace all
11 original branches).

---

## 3. OUT OF SCOPE

### Signature Unchanged
- The public-facing contract is `private void ProcessShutdownSIMA()` — **signature not modified**.
- No parameters added, no return type changed, no access modifier altered.

### No Behavior Change
- Drain order is **preserved exactly**: ring drain executes before dispatch-queue drain.
- `Print(...)` log messages are preserved verbatim in the extracted methods.
- Delta rollback arithmetic (`-ringSlot.ReservedDelta`, `-ignored.ReservedDelta`) is
  moved, not altered.
- Sideband zeroing (`_photonSideband[_sbIdx] = default(...)`) is preserved.

### Other Methods — Untouched
The following methods are explicitly **not modified** by this epic:

| Method | File | Reason |
|--------|------|--------|
| `ProcessApplySimaState` | Same file | Caller of toggle gate — separate concern |
| `ProcessInitializeSIMA` | Same file | Initialization path — out of scope |
| `EnumerateApexAccounts` | Same file | Account enumeration — separate concern |
| `HydrateExpectedPositionsFromBroker` | Same file | Hydration — separate concern |
| `HydrateWorkingOrdersFromBroker` | Same file | Order adoption — separate concern |
| `AddExpectedPositionDelta` | Other file | Existing callee — no changes |
| `ClearDispatchSyncPending` | Other file | Existing callee — no changes |
| `_photonPool.ReleaseByIndex` | Other file | Existing callee — no changes |
| `CancelAllV12GtcOrders` | Other file | Pre-drain step — not extracted |
| `StopReaperAudit` | Other file | Pre-drain step — not extracted |
| `UnsubscribeFromFleetAccounts` | Other file | Pre-drain step — not extracted |

### Data Structures — Untouched
- `_photonDispatchRing` — not restructured
- `_photonPool` — not restructured
- `_photonSideband` — not restructured
- `_pendingFleetDispatches` — not restructured

### Other Related Hotspots (Separate Epics)
- `HydrateFromOpenPositions` (CYC 34) — EPIC-W7-001
- `IsCommandForThisInstrument` (CYC 38) — EPIC-W7-002
- `HandleTerminated` (CYC 30) — EPIC-W7-003
- `SweepBrokerOrders` (CYC 28) — EPIC-W7-004

---

## 4. Extraction Plan

### Step-by-Step

```
Step 1 — Extract DrainPhotonDispatchRing()
  • Create new private void DrainPhotonDispatchRing() in same partial class file
  • Move Block A body (lines ~106–124) verbatim into new method
  • Replace Block A in ProcessShutdownSIMA with single call: DrainPhotonDispatchRing();
  • Verify: DrainPhotonDispatchRing CYC ≤ 5

Step 2 — Extract DrainPendingFleetDispatches()
  • Create new private void DrainPendingFleetDispatches() in same partial class file
  • Move Block B body (lines ~127–136) verbatim into new method
  • Replace Block B in ProcessShutdownSIMA with single call: DrainPendingFleetDispatches();
  • Verify: DrainPendingFleetDispatches CYC ≤ 3

Step 3 — Verify ProcessShutdownSIMA CYC ≤ 8
  • Count remaining branches in ProcessShutdownSIMA — must be ≤ 8

Step 4 — Compliance Checks
  • Confirm zero lock() blocks in both new methods
  • Confirm all string literals are ASCII-only
  • Confirm no new external dependencies introduced

Step 5 — Test
  • Unit test DrainPhotonDispatchRing() with empty ring (no-op path)
  • Unit test DrainPhotonDispatchRing() with ring containing entries (delta rollback path)
  • Unit test DrainPendingFleetDispatches() with empty queue (no-op path)
  • Unit test DrainPendingFleetDispatches() with queued requests (delta rollback path)
  • Integration test: full ProcessShutdownSIMA sequence end-to-end
```

### Proposed Helper Method Names (Summary)

| Method | Target CYC | Responsibility |
|--------|-----------|----------------|
| `DrainPhotonDispatchRing()` | ≤ 5 | Drain ring; sideband rollback; pool release |
| `DrainPendingFleetDispatches()` | ≤ 3 | Drain dispatch queue; delta rollback |

### CYC Budget After Extraction

| Method | Before | After |
|--------|--------|-------|
| `ProcessShutdownSIMA` (drain segment) | 11 | ≤ 8 |
| `DrainPhotonDispatchRing` | — (new) | ≤ 5 |
| `DrainPendingFleetDispatches` | — (new) | ≤ 3 |

---

## 5. Risk Assessment

### Low Risk
- **Zero external blast radius**: Both extracted methods call only pre-existing helpers
  (`AddExpectedPositionDelta`, `ClearDispatchSyncPending`, `_photonPool.ReleaseByIndex`).
  No new dependencies are introduced.
- **Pure extraction**: No logic changes — only movement of existing code into named methods.
- **Lock-free path already established**: TryDequeue on `ConcurrentQueue` / ring buffer
  does not require synchronization; extraction cannot introduce race conditions.
- **Stable code**: `DrainPhotonQueuesOnShutdown` is not in the top-50 churn hotspots;
  low rate of concurrent modification during this epic.

### Medium Risk
- **Shutdown criticality**: The drain path is executed on SIMA disable. A mistake
  (e.g., wrong extraction boundary leaving a variable reference stranded) would break
  the shutdown sequence and could leave queues in a dirty state on live trading.
  **Mitigation**: Line-by-line copy, no paraphrasing. Review extracted method body
  against source before commit.
- **Sideband array bounds**: `_photonSideband` is accessed with a bounds check inside
  the ring drain loop. Extraction must not reorder or lose the guard.
  **Mitigation**: Both compound conditions extracted verbatim.
- **Two callers of `ProcessShutdownSIMA`**: `ProcessApplySimaState` (line 78) calls
  `ProcessShutdownSIMA`. If drain is moved to a new wrapper method instead of kept
  inline, the caller count must remain unchanged.
  **Mitigation**: Keep extraction **inside** `ProcessShutdownSIMA`, not above it.

### Negligible Risk
- No new `lock()` blocks — cannot introduce deadlock.
- No new parameters — no call-site changes outside the target method.
- No interface or public API changes.

---

## 6. Success Criteria

### Complexity (Primary Gate)
- [ ] `ProcessShutdownSIMA` drain segment CYC ≤ 8 (down from 11)
- [ ] `DrainPhotonDispatchRing` CYC ≤ 5
- [ ] `DrainPendingFleetDispatches` CYC ≤ 3

### Correctness
- [ ] Shutdown sequence behavior identical to pre-refactor (delta rollback, sideband zero, pool release, log messages all preserved)
- [ ] `ProcessShutdownSIMA` signature unchanged: `private void ProcessShutdownSIMA()`
- [ ] Zero blast radius: no other method signatures modified

### Compliance
- [ ] Zero `lock()` blocks in `DrainPhotonDispatchRing` and `DrainPendingFleetDispatches`
- [ ] All string literals in new methods are ASCII-only
- [ ] No new `using` directives required

### Testing
- [ ] Unit tests pass for `DrainPhotonDispatchRing` (empty + non-empty ring)
- [ ] Unit tests pass for `DrainPendingFleetDispatches` (empty + non-empty queue)
- [ ] Integration test: full `ProcessShutdownSIMA` sequence completes cleanly

### Build & Deploy
- [ ] `dotnet build` succeeds with zero new warnings
- [ ] `deploy-sync.ps1` succeeds
- [ ] F5 in NinjaTrader: strategy loads and SIMA disable drains without error

---

## Agent Tracking

- **Agent Name**: v12-phase1-scope
- **Phase**: 1 (Scope Definition)
- **Input**: `docs/brain/EPIC-W7-108/00-hotspots.md`
- **Output**: `docs/brain/EPIC-W7-108/00-scope.md`
