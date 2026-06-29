# Phase 2: Architecture Plan — EPIC-W7-104

**Agent:** v12-phase2-architecture
**Wave:** 7 | **Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:10:00Z
**Input:** docs/brain/EPIC-W7-104/01-scope-boundary.md

---

## Method Under Extraction

- **Method:** `SubmitAndRegisterFleetOrders`
- **Source File:** `src/V12_002.SIMA.Fleet.cs`
- **Class:** `V12_002` (partial)
- **Lines:** 174–217
- **Visibility:** `private`
- **Original CYC:** 12

### jcodemunch get_context_bundle result

jcodemunch get_context_bundle (symbol_id `src/V12_002.SIMA.Fleet.cs::V12_002.SubmitAndRegisterFleetOrders#method`) retrieved the full source. Signature:

```csharp
private void SubmitAndRegisterFleetOrders(
    Account acct,
    Order[] orders,
    int orderCount,
    string fleetEntryName,
    string expectedKey,
    ref bool syncCleared
)
```

Body spans 44 lines with three distinct complexity clusters:
1. **Lines 184–188** — compound `&&` guard building an array slice (`orders != null && orderCount > 0 && orderCount < orders.Length`) → CYC +3
2. **Lines 195–203** — triple-`&&` guard for FSM `PendingSubmit → Submitted` state transition → CYC +3
3. **Lines 206–214** — nested `if` + `for` + compound null/empty guard registering order-IDs into `_orderIdToFsmKey` → CYC +4

Remaining sequential statements (Submit call, ClearDispatchSyncPending, syncCleared=true, Print) contribute no branches. Base CYC = 1. Total = 1+3+3+4 = **12**.

### jcodemunch get_call_hierarchy result

jcodemunch get_call_hierarchy (depth=2, direction=both) resolved 3 callers and 12 callees:

**Callers:**
- `ProcessFleetSlot` — direct caller (depth 1, `src/V12_002.SIMA.Fleet.cs:44`)
- `PumpFleetDispatch` — transitive caller (depth 2, line 233) — ConcurrentQueue drain path
- `ProcessValidPhotonSlot` — transitive caller (depth 2, line 395) — Photon ring path

**Key callees (depth 1):**
- `ClearDispatchSyncPending` — unconditional single call (`src/V12_002.SIMA.cs:179`)
- `_followerBrackets` — accessed twice: FSM state transition lookup + order-ID registration lookup
- `_orderIdToFsmKey` — written in the order-ID registration loop
- `LogBuffer.Format` — used by `Print` at method tail

**Impact:** Method signature must remain identical post-refactor. All 3 callers call `ProcessFleetSlot` which is the only direct call site; signature unchanged per scope boundary.

### jcodemunch get_dependency_graph result

jcodemunch get_dependency_graph (file=`src/V12_002.SIMA.Fleet.cs`, direction=both, depth=1) returned 1 node, 0 edges. The file has no explicit import edges in the index (all dependencies are resolved via the shared partial-class namespace). No cross-file refactoring is required — all extracted helpers are `private` methods in the same file/partial class, consistent with V12.23 scope boundary.

### jcodemunch get_extraction_candidates result

jcodemunch get_extraction_candidates (file=`src/V12_002.SIMA.Fleet.cs`, min_complexity=3, min_callers=1) returned 0 candidates. This is expected: the extraction candidate heuristic requires an existing symbol to have multiple callers; the helpers do not yet exist. The 3 candidate extractions are identified manually from get_context_bundle source analysis and the Phase 0 hotspot report, which independently identified the same 3 complexity drivers.

---

## Sequential Thinking Summary

sequentialthinking chain completed (5 thoughts):

- **Thought 1:** Mapped all jcodemunch findings to the 3 CYC drivers (D1 +3, D2 +3, D3 +4). Established extraction strategy: 3 helpers, one per driver, targeting max CYC 5.
- **Thought 2:** Designed `BuildSubmitSlice` — guard-clause inversion of the 3-`&&` array check. Projected CYC 4.
- **Thought 3:** Designed `TransitionFsmToSubmitted` — guard-clause decomposition of the FSM state-transition block. Projected CYC 4.
- **Thought 4:** Designed `RegisterOrderIdsInFsmIndex` — loop-body extraction with continue-guard replacing nested if. Projected CYC 5. Calculated parent residual: all 3 drivers extracted, parent becomes fully sequential → CYC 1.
- **Thought 5:** Jane Street alignment verified across all 5 checkpoints (CYC<=8, single-responsibility, lock-free, illegal-states-unrepresentable, zero-allocation). Final verdict: extraction_count=3, max_cyc_projected=5, all <=8 PASS.

---

## Extraction Plan

| Helper Method Name | Responsibility | Signature | Projected CYC |
|---|---|---|---|
| `BuildSubmitSlice` | Guard-clause array-slice builder: returns full `orders` array unchanged when slice is not needed; allocates and copies a trimmed slice only when `orderCount` is a valid sub-range | `private Order[] BuildSubmitSlice(Order[] orders, int orderCount)` | 4 |
| `TransitionFsmToSubmitted` | Single FSM state transition: guards that the FSM entry exists, is non-null, and is in `PendingSubmit` state before transitioning to `Submitted` and stamping `LastUpdateUtc` | `private void TransitionFsmToSubmitted(string fleetEntryName)` | 4 |
| `RegisterOrderIdsInFsmIndex` | Order-ID index maintenance: validates FSM entry existence then loops all submitted orders, registering each valid `OrderId → fleetEntryName` mapping in `_orderIdToFsmKey` | `private void RegisterOrderIdsInFsmIndex(Order[] orders, int orderCount, string fleetEntryName)` | 5 |

### Helper Implementation Notes

**`BuildSubmitSlice`** (replaces Driver 1, lines 184–188):
```csharp
private Order[] BuildSubmitSlice(Order[] orders, int orderCount)
{
    if (orders == null) return orders;
    if (orderCount <= 0) return orders;
    if (orderCount >= orders.Length) return orders;
    var slice = new Order[orderCount];
    Array.Copy(orders, slice, orderCount);
    return slice;
}
// CYC: 1(base) + 3(guard returns) = 4
```

**`TransitionFsmToSubmitted`** (replaces Driver 2, lines 195–203):
```csharp
private void TransitionFsmToSubmitted(string fleetEntryName)
{
    FollowerBracketFSM fsm;
    if (!_followerBrackets.TryGetValue(fleetEntryName, out fsm)) return;
    if (fsm == null) return;
    if (fsm.State != FollowerBracketState.PendingSubmit) return;
    fsm.State = FollowerBracketState.Submitted;
    fsm.LastUpdateUtc = DateTime.UtcNow;
}
// CYC: 1(base) + 3(guard returns) = 4
```

**`RegisterOrderIdsInFsmIndex`** (replaces Driver 3, lines 206–214):
```csharp
private void RegisterOrderIdsInFsmIndex(Order[] orders, int orderCount, string fleetEntryName)
{
    FollowerBracketFSM fsm;
    if (!_followerBrackets.TryGetValue(fleetEntryName, out fsm)) return;
    for (int i = 0; i < orderCount; i++)
    {
        var ord = orders[i];
        if (ord == null || string.IsNullOrEmpty(ord.OrderId)) continue;
        _orderIdToFsmKey[ord.OrderId] = fleetEntryName;
    }
}
// CYC: 1(base) + 1(guard return) + 1(for loop) + 2(null/empty check) = 5
```

---

## Parent Method After Extraction

**Remaining logic:**
```csharp
private void SubmitAndRegisterFleetOrders(
    Account acct, Order[] orders, int orderCount,
    string fleetEntryName, string expectedKey, ref bool syncCleared)
{
    var submitOrders = BuildSubmitSlice(orders, orderCount);
    acct.Submit(submitOrders);
    ClearDispatchSyncPending(expectedKey);
    syncCleared = true;
    TransitionFsmToSubmitted(fleetEntryName);
    RegisterOrderIdsInFsmIndex(orders, orderCount, fleetEntryName);
    Print(string.Format("[PUMP] Submitted {0} orders for {1} | {2}", orderCount, fleetEntryName, acct.Name));
}
```

Sequential calls only — no conditional branches remain in the parent.

- **Projected CYC:** 1

---

## max_cyc_projected: 5
## extraction_count: 3

---

## Jane Street Alignment

| Principle | Status |
|---|---|
| CYC<=8 achieved (max=5) | YES |
| Single-responsibility per helper | YES |
| Lock-free / Actor pattern preserved (no lock() blocks) | YES |
| Illegal states unrepresentable (PendingSubmit guard in TransitionFsmToSubmitted) | YES |
| Zero-allocation hot paths (allocation isolated to BuildSubmitSlice) | YES |
| Extract Guard Clauses pattern applied | YES |
| Extract Loop Body pattern applied (RegisterOrderIdsInFsmIndex) | YES |
| No scope creep — same-file private helpers only | YES |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 3 |
| **Execution Time** | 2026-06-29T01:10:00Z |
| **Wave** | 7 |
| **Phase** | 2 |
| **jcodemunch tools called** | get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **extraction_count** | 3 |
| **max_cyc_projected** | 5 |
