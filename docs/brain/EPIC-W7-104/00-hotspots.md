# EPIC-W7-104 — Phase 0: Hotspot Analysis

## Method Under Analysis

| Field           | Value                                      |
|-----------------|------------------------------------------- |
| **Method**      | `SubmitAndRegisterFleetOrders`             |
| **CYC Score**   | 12                                         |
| **File**        | `src/V12_002.SIMA.Fleet.cs`               |
| **Lines**       | 174 – 217                                  |
| **Visibility**  | `private`                                  |
| **Class**       | `V12_002` (partial)                        |

---

## Blast Radius Summary

`SubmitAndRegisterFleetOrders` is called from exactly **one site** — [`ProcessFleetSlot()`](src/V12_002.SIMA.Fleet.cs:65) — making its direct call-graph footprint minimal. However its *side-effect radius* is broad because the method mutates three shared, cross-file data structures:

| Shared State             | Owner File(s)                                                                 | Risk |
|--------------------------|-------------------------------------------------------------------------------|------|
| `_followerBrackets`      | `V12_002.cs`, `V12_002.Symmetry.BracketFSM.cs`, `V12_002.SIMA.Dispatch.cs`, `V12_002.SIMA.Lifecycle.cs`, `V12_002.SIMA.Execution.cs`, `V12_002.REAPER.Audit.cs`, `V12_002.Orders.Callbacks.Propagation.cs` | **HIGH** — FSM state written; 14+ files read/write |
| `_orderIdToFsmKey`       | `V12_002.cs`, `V12_002.Symmetry.BracketFSM.cs`, `V12_002.SIMA.Lifecycle.cs`, `V12_002.Orders.Callbacks.Propagation.cs` | **MEDIUM** — O(n) order-loop write; index used by all broker callbacks |
| `ClearDispatchSyncPending` (via `_dispatchSyncPendingExpKeys`) | `V12_002.SIMA.cs`, `V12_002.Orders.Callbacks.cs`, `V12_002.Orders.Callbacks.AccountOrders.cs`, `V12_002.SIMA.Execution.cs` | **MEDIUM** — sync guard; cleared here, checked in 6+ files |

**Indirect call impact**: `acct.Submit(submitOrders)` is a broker I/O call that triggers `OnAccountOrderUpdate` and `OnAccountExecutionUpdate` callbacks on the broker thread, which in turn traverse `_followerBrackets` and `_orderIdToFsmKey`. Any regression in index correctness propagates immediately to FSM resolution across the entire order-lifecycle pipeline.

**Caller chain**:
```
PumpFleetDispatch()          (V12_002.SIMA.Fleet.cs:233)
  └─ ProcessFleetSlot()      (V12_002.SIMA.Fleet.cs:44)
       └─ SubmitAndRegisterFleetOrders()   ← TARGET
```
Both `PumpFleetDispatch` call sites are: the legacy `ConcurrentQueue` drain (line 271) and the Photon ring path via `ProcessValidPhotonSlot` (line 399).

---

## Top 3 Complexity Drivers

### Driver 1 — Compound `&&` Guard on Array Slice (Line 184, CYC +3)

```csharp
if (orders != null && orderCount > 0 && orderCount < orders.Length)
{
    submitOrders = new Order[orderCount];
    Array.Copy(orders, submitOrders, orderCount);
}
```

Three short-circuit operands each contribute +1 to CYC (McCabe per-operand counting). The allocation of `new Order[orderCount]` inside a hot-path dispatch method also violates the zero-allocation constraint in force elsewhere in the fleet pipeline. This block is a natural extraction candidate: `BuildSubmitSlice(orders, orderCount)`.

---

### Driver 2 — Triple-compound `&&` Guard on FSM PendingSubmit Transition (Lines 195–203, CYC +3)

```csharp
if (
    _followerBrackets.TryGetValue(fleetEntryName, out pFsm)
    && pFsm != null
    && pFsm.State == FollowerBracketState.PendingSubmit
)
{
    pFsm.State = FollowerBracketState.Submitted;
    pFsm.LastUpdateUtc = DateTime.UtcNow;
}
```

Three `&&` operands = +3 CYC. The FSM state transition (`PendingSubmit → Submitted`) is a distinct concern from order registration and duplicates transition logic already performed in `InitializeFollowerBracketFSM` (line 120) and `ProcessFleetSlot`'s `catch` rollback. This entire block should be extracted to `TransitionFsmToSubmitted(string fleetEntryName)`.

---

### Driver 3 — Nested `if`-inside-`for` Loop for Order-ID Registration (Lines 206–214, CYC +4)

```csharp
if (_followerBrackets.TryGetValue(fleetEntryName, out fsm))          // +1
{
    for (int i = 0; i < orderCount; i++)                              // +1
    {
        var ord = orders[i];
        if (ord != null && !string.IsNullOrEmpty(ord.OrderId))        // +2 (&&)
            _orderIdToFsmKey[ord.OrderId] = fleetEntryName;
    }
}
```

The outer `TryGetValue` guard + loop + compound null/empty check = +4 CYC. The `_orderIdToFsmKey` registration loop is a separate responsibility (order-ID index maintenance) that has identical counterparts in `V12_002.SIMA.Lifecycle.cs` (lines 563, 593) and `V12_002.Orders.Callbacks.Propagation.cs` (line 871). Extraction to `RegisterOrderIdsInFsmIndex(Order[] orders, int count, string fsmKey)` eliminates CYC here and enables reuse across those call sites.

---

## Recommended Extraction Count

**3 extractions** to reach target CYC ≤ 5:

| # | Extracted Method                     | Replaces Driver   | CYC Removed | Notes                              |
|---|--------------------------------------|-------------------|-------------|-------------------------------------|
| 1 | `BuildSubmitSlice(orders, orderCount)` | Driver 1 (line 184) | −3        | Isolates array-slice + allocation   |
| 2 | `TransitionFsmToSubmitted(string key)` | Driver 2 (lines 195–203) | −3   | Reusable FSM state transition      |
| 3 | `RegisterOrderIdsInFsmIndex(Order[], int, string)` | Driver 3 (lines 206–214) | −4 | Cross-file reuse opportunity   |

**Projected residual CYC**: 12 − 3 − 3 − 4 + 3 (one branch per call) = **5** (within target ≤ 5).

---

## Agent Tracking

| Field            | Value                           |
|------------------|---------------------------------|
| **Agent Name**   | v12-phase0-hotspot              |
| **Bobcoins Used**| 6                               |
| **Execution Time**| ~55s                           |
| **Phase**        | 0 — Hotspot Analysis            |
| **Output**       | `docs/brain/EPIC-W7-104/00-hotspots.md` |
