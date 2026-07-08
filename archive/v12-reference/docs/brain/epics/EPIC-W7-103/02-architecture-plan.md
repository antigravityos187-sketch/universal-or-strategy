# Phase 2: Architecture Plan — EPIC-W7-103

**Agent:** v12-phase2-architecture
**Wave:** 7 | **Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:10:00Z
**Input:** docs/brain/EPIC-W7-103/01-scope-boundary.md

---

## Method Under Extraction

- **Method:** `ProcessFleetSlot`
- **Source File:** `src/V12_002.SIMA.Fleet.cs`
- **Original CYC:** 13
- **Class:** `V12_002` (partial), line 44

### jcodemunch get_context_bundle result

get_context_bundle returned symbol-not-found for `ProcessFleetSlot` (index has 5147 symbols; fallback to search_symbols succeeded). jcodemunch search_symbols confirmed symbol at `src/V12_002.SIMA.Fleet.cs` line 44, signature: `private void ProcessFleetSlot(Account acct, Order[] orders, int orderCount, string fleetEntryName, string expectedKey, int reservedDelta, long signalTicks, int poolSlotIndex)`. 8-parameter private coordinator method. A parallel symbol exists in `src-vm-backup/V12_002.SIMA.Fleet.cs` — scope applies to `src/` only.

### jcodemunch get_call_hierarchy result (callers/callees via get_dependency_graph)

jcodemunch get_call_hierarchy (depth=2, direction=both) on `src/V12_002.SIMA.Fleet.cs::V12_002.ProcessFleetSlot#method`:

**Direct callers (depth=1):**
- `PumpFleetDispatch` — `src/V12_002.SIMA.Fleet.cs:233` (ast_resolved)
- `ProcessValidPhotonSlot` — `src/V12_002.SIMA.Fleet.cs:395` (ast_resolved)

**Depth-2 caller:**
- `VerifyPhotonSlotIntegrity` — `src/V12_002.SIMA.Fleet.cs:329` (ast_resolved)

**Key direct callees (depth=1):** `ValidateDispatchTimestamp`, `InitializeFollowerBracketFSM`, `SubmitAndRegisterFleetOrders`, `ClearDispatchSyncPending`, `AddExpectedPositionDeltaLocked`, `RollbackFleetDispatchState`, `TryResetCircuitBreakerIfBelow`, `PumpFleetDispatch` (re-pump).

No caller signatures are modified by this epic — all 3 callers confirmed upstream-only.

### jcodemunch get_dependency_graph result

jcodemunch get_dependency_graph (direction=both, depth=1) for `src/V12_002.SIMA.Fleet.cs`: node_count=1, edge_count=0. No explicit import edges in the index (C# partial class — all symbols co-located in the NinjaTrader compilation unit). File is self-contained at the module graph level.

### jcodemunch get_extraction_candidates result

jcodemunch get_extraction_candidates (min_complexity=3, min_callers=1) returned 0 candidates. This is consistent with `ProcessFleetSlot` being a private method with no external callers meeting the threshold — extraction candidates tool is optimized for public/internal symbols with cross-file callers. The Phase 0 hotspot analysis provides the authoritative complexity breakdown for this private method.

---

## Sequential Thinking Summary

sequentialthinking chain (5 thoughts) produced the following final verdict:

**Thought 5 (Final):** ProcessFleetSlot (CYC 13) is structured as a try/catch/finally trifecta. Three private helpers are extracted to decompose the three structural zones of the method. All projected CYCs verified <= 8.

- `ExecuteDispatchCore` (from try body): validate timestamp early-exit + delegate to FSM init and order submission. Projected CYC: 2.
- `HandleDispatchFailure` (from catch body): log + two compensation guards + rollback delegation. Projected CYC: 3.
- `TryRepumpIfQueued` (from finally pump-prime): compound queue-check condition + defensive try/catch for TriggerCustomEvent. Projected CYC: 5.
- Parent `ProcessFleetSlot` reduced to: acquire slot → try/catch/finally shell delegating to helpers + pool release + Decrement + circuit-breaker reset. Projected CYC: 5.

max_cyc_projected = 5. All <= 8. Jane Street PASS. V12.23 No Scope Creep PASS (DrainAllDispatchQueuesOnAbort untouched).

---

## Extraction Plan

| Helper Method Name | Responsibility | Projected CYC |
|---|---|---|
| `ExecuteDispatchCore(Account acct, Order[] orders, int orderCount, string fleetEntryName, string expectedKey, int reservedDelta, long signalTicks, ref bool syncCleared)` | Happy-path dispatch sequence: validate timestamp with early-exit guard, initialize follower bracket FSM, submit and register fleet orders | 2 |
| `HandleDispatchFailure(Exception ex, bool syncCleared, int reservedDelta, string expectedKey, string fleetEntryName)` | Catch-path compensation: log exception, conditionally clear dispatch sync pending, conditionally reverse reserved position delta, rollback fleet dispatch state | 3 |
| `TryRepumpIfQueued()` | Check whether photon dispatch ring or pending fleet dispatch queue is non-empty; if so, re-trigger PumpFleetDispatch via TriggerCustomEvent with defensive try/catch and diagnostic logging | 5 |

---

## Parent Method After Extraction

**Remaining logic in `ProcessFleetSlot`:**

```
private void ProcessFleetSlot(Account acct, Order[] orders, int orderCount,
    string fleetEntryName, string expectedKey, int reservedDelta,
    long signalTicks, int poolSlotIndex)
{
    bool syncCleared = false;
    try
    {
        ExecuteDispatchCore(acct, orders, orderCount, fleetEntryName,
            expectedKey, reservedDelta, signalTicks, ref syncCleared);
    }
    catch (Exception ex)
    {
        HandleDispatchFailure(ex, syncCleared, reservedDelta,
            expectedKey, fleetEntryName);
    }
    finally
    {
        if (poolSlotIndex >= 0)
            _photonPool.ReleaseByIndex(poolSlotIndex);
        Interlocked.Decrement(ref _pendingFleetDispatchCount);
        int currentCount = Volatile.Read(ref _pendingFleetDispatchCount);
        TryResetCircuitBreakerIfBelow(currentCount);
        TryRepumpIfQueued();
    }
}
```

- **Projected CYC:** 5 (try/catch/finally structural paths + `if(poolSlotIndex >= 0)` finally guard + method entry baseline)

---

## max_cyc_projected: 5
## extraction_count: 3

---

## Jane Street Alignment

| Rule | Status |
|---|---|
| CYC<=8 achieved | YES — max projected CYC is 5 across all methods |
| Single-responsibility per helper | YES — ExecuteDispatchCore owns happy path only; HandleDispatchFailure owns catch compensation only; TryRepumpIfQueued owns re-pump logic only |
| Lock-free/Actor pattern preserved | YES — Interlocked.Decrement and Volatile.Read remain intact; no lock() blocks introduced; no lock() blocks removed |
| Illegal states unrepresentable | YES — `ref bool syncCleared` in ExecuteDispatchCore makes the state mutation visible at the call boundary; `bool syncCleared` passed by value to HandleDispatchFailure makes compensation guard state explicit in signature |
| Zero-allocation hot paths | YES — all helpers pass existing locals/primitives by value or ref; no closures captured; no heap allocations introduced |
| Extract guard clauses | YES — ValidateDispatchTimestamp early-exit moved inside ExecuteDispatchCore; ProcessFleetSlot body is now guard-free |
| V12.23 No Scope Creep | YES — DrainAllDispatchQueuesOnAbort (EPIC-W7-054) untouched; no sibling methods modified |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 1.5 |
| **Execution Time** | 2026-06-29T01:10:00Z |
| **jcodemunch tools called** | get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **Wave** | 7 |
| **Phase** | 2 |
| **Epic** | EPIC-W7-103 |
| **Output** | docs/brain/EPIC-W7-103/02-architecture-plan.md |
