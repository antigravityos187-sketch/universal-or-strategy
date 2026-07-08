# EPIC-W7-103 — Phase 0: Hotspot Analysis

## Method Summary

| Field         | Value                                |
|---------------|--------------------------------------|
| **Method**    | `ProcessFleetSlot`                   |
| **CYC Score** | 13                                   |
| **File**      | `src/V12_002.SIMA.Fleet.cs`          |
| **Lines**     | 44–97 (signature to closing brace)   |
| **Class**     | `V12_002` (partial)                  |

---

## Blast Radius Summary

`ProcessFleetSlot` is the **shared fleet dispatch core** — invoked on both the Photon ring path and the
legacy `ConcurrentQueue` path. Any change to it propagates to **15 downstream files** that touch the
shared state it reads and writes.

### Direct Callers (2)

| Call Site | Location |
|-----------|----------|
| `PumpFleetDispatch()` — legacy queue branch | `V12_002.SIMA.Fleet.cs:271` |
| `ProcessValidPhotonSlot()` — Photon ring branch | `V12_002.SIMA.Fleet.cs:399` |

### Shared State Mutated

| State Field | Consuming Files |
|-------------|-----------------|
| `_pendingFleetDispatchCount` | `V12_002.SIMA.Fleet.cs`, `V12_002.SIMA.Dispatch.cs`, `V12_002.cs`, `V12_002.SIMA.Lifecycle.cs` |
| `_photonPool` | `V12_002.SIMA.Fleet.cs`, `V12_002.SIMA.Dispatch.cs`, `V12_002.SIMA.Shadow.cs` |
| `_photonDispatchRing` | `V12_002.SIMA.Fleet.cs`, `V12_002.SIMA.Dispatch.cs`, `V12_002.SIMA.Shadow.cs` |
| `_followerBrackets` | `V12_002.SIMA.Execution.cs`, `V12_002.Symmetry.BracketFSM.cs`, `V12_002.Symmetry.Follower.cs`, `V12_002.REAPER.Audit.cs`, `V12_002.REAPER.Repair.cs` |
| `_orderIdToFsmKey` | `V12_002.Orders.Callbacks.AccountOrders.cs`, `V12_002.Orders.Callbacks.Propagation.cs` |
| `_pendingFleetDispatches` | `V12_002.SIMA.Dispatch.cs`, `V12_002.cs` |

**Total affected files: 15**  
Impact surface spans SIMA dispatch, REAPER, order callbacks, symmetry FSM, and lifecycle layers.

---

## Top 3 Complexity Drivers

### Driver 1 — `finally` block: compound conditional + nested try/catch (CYC +4)

```csharp
finally
{
    if (poolSlotIndex >= 0)                           // branch +1
        _photonPool.ReleaseByIndex(poolSlotIndex);
    Interlocked.Decrement(ref _pendingFleetDispatchCount);

    int currentCount = Volatile.Read(ref _pendingFleetDispatchCount);
    TryResetCircuitBreakerIfBelow(currentCount);      // delegate branch inside (+1)

    if ((_photonDispatchRing != null && !_photonDispatchRing.IsEmpty)   // compound +2
        || !_pendingFleetDispatches.IsEmpty)
        try
        {
            TriggerCustomEvent(o => PumpFleetDispatch(), null);
        }
        catch (Exception ex)                          // catch path +1
        {
            if (_diagFleet)                           // guard branch +1
                Print(...);
        }
}
```

**CYC contribution: ~4** — The `finally` block hosts a pool-release guard, a circuit-breaker delegate,
a compound re-pump condition, and a defensive try/catch, producing 4 independent decision paths inside
a single cleanup scope.

---

### Driver 2 — `catch` block: dual compensation branches (CYC +3)

```csharp
catch (Exception ex)
{
    Print(...);
    if (!syncCleared)                                 // branch +1
        ClearDispatchSyncPending(expectedKey);
    if (reservedDelta != 0)                           // branch +1
        AddExpectedPositionDeltaLocked(expectedKey, -reservedDelta);
    RollbackFleetDispatchState(fleetEntryName);        // downstream +1 (loop in rollback)
}
```

**CYC contribution: ~3** — Two independent compensation guards (`syncCleared` and `reservedDelta != 0`)
live inside the catch scope. Each represents a separately testable rollback decision, and both must be
exercised independently to achieve full path coverage.

---

### Driver 3 — `try` body: three-phase sequential delegation (CYC +6, cumulative with guard)

```csharp
try
{
    if (!ValidateDispatchTimestamp(..., ref syncCleared))    // early-exit +1
        return;

    InitializeFollowerBracketFSM(...);                       // delegates own CYC (loop+if nesting)
    SubmitAndRegisterFleetOrders(...);                       // delegates own CYC (if+loop nesting)
}
```

**CYC contribution: ~6 (across delegation chain)** — The early-exit guard is the only explicit branch,
but `InitializeFollowerBracketFSM` (inner `for` loop + `StartsWith` chain + nested `for` with `break`)
and `SubmitAndRegisterFleetOrders` (array-copy guard + FSM state check + registration loop) both add
cyclomatic weight that flows back to `ProcessFleetSlot` as its nominal coordinator.

> Note: Current refactoring has already extracted these as private helpers. The residual CYC=13
> reflects the `try/catch/finally` trifecta structure, the compound conditions in `finally`, and the
> interaction with the pool-slot guard (`poolSlotIndex >= 0`).

---

## Recommended Extraction Count

| Extraction Target | Rationale | Estimated CYC Reduction |
|-------------------|-----------|------------------------|
| Extract `finally` pump-prime into `TryRepumpIfQueued()` | Isolates re-trigger logic; removes compound `&&`/`\|\|` from finally | −3 |
| Extract `catch` compensation into `HandleDispatchFailure(syncCleared, reservedDelta, expectedKey, fleetEntryName)` | Encapsulates the two independent rollback guards; makes catch a single-line delegation | −2 |
| Extract `try` body into `ExecuteDispatchCore(...)` | Makes `ProcessFleetSlot` a pure coordinator with try/catch/finally shell and zero internal branches | −2 |

**Total recommended extractions: 3**  
**Projected post-refactor CYC: 6** (try/catch/finally shell + 1 null guard for pool release)

---

## Agent Tracking

```
Agent Name:      v12-phase0-hotspot
Bobcoins Used:   6
Execution Time:  ~45s
Wave:            7
Phase:           0
Epic:            EPIC-W7-103
```
