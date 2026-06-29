# EPIC-W7-062 — Phase 2: Architecture Plan

**Agent Name:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29
**Input:** docs/brain/EPIC-W7-062/01-scope-boundary.md

---

## Target Method

| Field | Value |
|---|---|
| **Method** | `ProcessFleetSlot` |
| **File** | `src/V12_002.SIMA.Fleet.cs` |
| **Lines** | 44–97 |
| **CYC Baseline** | 13 |
| **CYC Target** | <= 8 |
| **Parameters** | 8 |
| **Callers** | `PumpFleetDispatch`, `ProcessValidPhotonSlot` (DO NOT MODIFY) |
| **Symbol ID** | `src/V12_002.SIMA.Fleet.cs::V12_002.ProcessFleetSlot#method` |

---

## MCP Evidence

### Context Bundle (get_context_bundle)

The full source of `ProcessFleetSlot` was retrieved from the jCodemunch index
(freshness=fresh). Key structural observations:

- **try block**: Calls `ValidateDispatchTimestamp` (guard), `InitializeFollowerBracketFSM`,
  `SubmitAndRegisterFleetOrders` — 3 sequential helper calls with 1 early-return branch.
- **catch block**: 3 conditional guards + `RollbackFleetDispatchState` — recovery path.
- **finally block**: Pool release guard, `Interlocked.Decrement`, `Volatile.Read`,
  `TryResetCircuitBreakerIfBelow`, compound boolean re-pump gate with **nested try/catch**
  and inner `_diagFleet` guard — the primary complexity hotspot.

### Call Hierarchy (get_call_hierarchy, direction=both, depth=2)

Direct callers (depth=1):
- `PumpFleetDispatch` (line 233) — ast_resolved
- `ProcessValidPhotonSlot` (line 395) — ast_resolved

Direct callees (depth=1):
- `ValidateDispatchTimestamp` (line 99)
- `InitializeFollowerBracketFSM` (line 120)
- `SubmitAndRegisterFleetOrders` (line 174)
- `ClearDispatchSyncPending`, `AddExpectedPositionDeltaLocked` (catch recovery)
- `RollbackFleetDispatchState` (line 219)
- `TryResetCircuitBreakerIfBelow` (line 420)
- `PumpFleetDispatch` (self-dispatch in finally)
- `TriggerCustomEvent` (in finally inner try)

### Dependency Graph (get_dependency_graph, direction=imports, depth=1)

`src/V12_002.SIMA.Fleet.cs` has 0 external import edges at the file level — it is a
partial class within the same assembly. All dependencies are intra-assembly partial class
references. No cross-file import rewrites required by this refactor.

---

## Sequential Thinking Evidence

### Thought 1 — Complexity Drivers (CYC=13)

The 13 CYC is distributed across three structural zones:

| Zone | Branches | CYC Contribution |
|---|---|---|
| try block guard | `if (!ValidateDispatchTimestamp)` | +1 |
| catch recovery | `if (!syncCleared)`, `if (reservedDelta != 0)` | +2 |
| finally pool guard | `if (poolSlotIndex >= 0)` | +1 |
| finally compound bool | `null &&`, `&&`, `\|\|` | +3 |
| finally inner try | try/catch exception path | +2 |
| finally inner catch | `if (_diagFleet)` | +1 |
| base | — | +1 |
| **Total** | | **~13** |

The finally block alone contributes ~7 CYC (compound boolean + nested try/catch +
inner conditional). This is the primary extraction target.

### Thought 2 — Extraction Strategy

Two private helpers extracted into the same partial class:

**Extraction 1: `HandleFleetSlotCatch`**
- Absorbs: `if (!syncCleared)`, `if (reservedDelta != 0)`, `RollbackFleetDispatchState`
- Signature: `private void HandleFleetSlotCatch(string fleetEntryName, string expectedKey, int reservedDelta, bool syncCleared)`
- Note: `Print(string.Format(...))` stays in parent catch (single logging statement, not a branch)
- Projected CYC: 1 (base) + 1 (syncCleared guard) + 1 (reservedDelta guard) = **3**

**Extraction 2: `HandleFleetSlotFinally`**
- Absorbs: pool release, `Interlocked.Decrement`, `Volatile.Read`, circuit breaker reset,
  compound re-pump gate, inner try/catch with `_diagFleet` guard
- Signature: `private void HandleFleetSlotFinally(int poolSlotIndex)`
- Projected CYC: 1 + 1 (pool guard) + 3 (compound bool) + 1 (inner try) + 1 (inner catch) + 1 (diagFleet) = **8**

**Residual `ProcessFleetSlot` after extraction:**
- Base + try/catch structure + 1 if guard = **3**

### Thought 3 — CYC Validation

| Method | Branches | Projected CYC | Status |
|---|---|---|---|
| `ProcessFleetSlot` (residual) | if guard + try/catch | 3 | PASS (<= 8) |
| `HandleFleetSlotCatch` | 2 if guards | 3 | PASS (<= 8) |
| `HandleFleetSlotFinally` | pool guard + 3 bool + try/catch + diagFleet | 8 | PASS (<= 8) |
| **Max across all** | | **8** | **PASS** |

CYC reduction: 13 → 8 max (delta = 5). Constraint satisfied.

---

## Extraction Plan

| # | Helper Method | Signature | Extracted From | Projected CYC |
|---|---|---|---|---|
| 1 | `HandleFleetSlotCatch` | `private void HandleFleetSlotCatch(string fleetEntryName, string expectedKey, int reservedDelta, bool syncCleared)` | catch block conditionals + rollback | 3 |
| 2 | `HandleFleetSlotFinally` | `private void HandleFleetSlotFinally(int poolSlotIndex)` | finally block: pool release, circuit breaker, re-pump gate | 8 |

**Total extractions:** 2
**max_cyc_projected:** 8

### Residual ProcessFleetSlot (Post-Extraction Sketch)

```csharp
private void ProcessFleetSlot(
    Account acct, Order[] orders, int orderCount,
    string fleetEntryName, string expectedKey,
    int reservedDelta, long signalTicks, int poolSlotIndex)
{
    bool syncCleared = false;
    try
    {
        if (!ValidateDispatchTimestamp(signalTicks, fleetEntryName, expectedKey, reservedDelta, ref syncCleared))
            return;
        InitializeFollowerBracketFSM(orders, orderCount, fleetEntryName, acct.Name, reservedDelta);
        SubmitAndRegisterFleetOrders(acct, orders, orderCount, fleetEntryName, expectedKey, ref syncCleared);
    }
    catch (Exception ex)
    {
        Print(string.Format("[PUMP] Submit FAILED for {0} ({1}): {2}", fleetEntryName, acct.Name, ex.Message));
        HandleFleetSlotCatch(fleetEntryName, expectedKey, reservedDelta, syncCleared);
    }
    finally
    {
        HandleFleetSlotFinally(poolSlotIndex);
    }
}
```

---

## Jane Street KB Alignment

| KB Rule | Source | Compliance |
|---|---|---|
| Zero-alloc hot path | carl_cook | No new allocations; `string.Format` already present in catch (cold path only) |
| Extract cold logging out-of-line | carl_cook | `Print(...)` remains in cold catch path; not moved to hot path |
| Structs ref/in/out | carl_cook | Existing `ref syncCleared` preserved; no new boxing |
| Avoid LINQ | carl_cook | No LINQ introduced |
| No new lock() blocks | gjengset | No locks added; `Interlocked.Decrement` and `Volatile.Read` preserved in extracted helper |
| 64-byte cache line alignment | gjengset | N/A — extraction is behavioral, not struct layout |
| Single responsibility per helper | trading_billions | `HandleFleetSlotCatch` = error recovery only; `HandleFleetSlotFinally` = cleanup + re-trigger only |
| Each helper CYC <= 8 | trading_billions | Max CYC = 8 across all extracted methods |
| Rate-limit circuit breaker | trading_billions | `TryResetCircuitBreakerIfBelow` call preserved in `HandleFleetSlotFinally` |
| Defense in depth | trading_billions | Error recovery in catch preserved; rollback logic intact |

---

## V12.23 Scope Compliance

| Check | Status |
|---|---|
| Single method targeted (`ProcessFleetSlot`) | PASS |
| Callers NOT modified (`PumpFleetDispatch`, `ProcessValidPhotonSlot`) | PASS |
| No cross-file refactoring | PASS |
| Helpers are private, same partial class | PASS |
| No signature change to `ProcessFleetSlot` | PASS |
| No pre-existing bug fixes bundled | PASS |
| Scope boundary PASS inherited from Phase 1.5 | PASS |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Phase** | 2 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-062 |
| **Bobcoins Used** | 1.0 |
| **Extractions** | 2 |
| **max_cyc_projected** | 8 |
| **CYC Baseline** | 13 |
| **CYC Reduction** | 5 |
