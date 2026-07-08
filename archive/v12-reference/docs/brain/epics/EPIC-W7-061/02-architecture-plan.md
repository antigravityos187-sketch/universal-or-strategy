# EPIC-W7-061 — Phase 2: Architecture Plan

**Agent Name:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T00:00:00Z
**Input:** docs/brain/EPIC-W7-061/01-scope-boundary.md

---

## Target Method Summary

| Field            | Value                                          |
|------------------|------------------------------------------------|
| **Method**       | `SubmitAndRegisterFleetOrders`                 |
| **File**         | `src/V12_002.SIMA.Fleet.cs`                   |
| **Lines**        | 174–217                                        |
| **CYC Baseline** | 12                                             |
| **Target CYC**   | <= 8                                           |
| **Risk Level**   | MEDIUM                                         |
| **Threshold**    | 8 (Jane Street strict)                         |
| **CYC Over**     | 4                                              |

---

## MCP Evidence

### get_context_bundle Result

Symbol resolved at `src/V12_002.SIMA.Fleet.cs:174`. Full method source retrieved:

```csharp
private void SubmitAndRegisterFleetOrders(
    Account acct,
    Order[] orders,
    int orderCount,
    string fleetEntryName,
    string expectedKey,
    ref bool syncCleared
)
{
    Order[] submitOrders = orders;
    if (orders != null && orderCount > 0 && orderCount < orders.Length)
    {
        submitOrders = new Order[orderCount];
        Array.Copy(orders, submitOrders, orderCount);
    }

    acct.Submit(submitOrders);
    ClearDispatchSyncPending(expectedKey);
    syncCleared = true;

    FollowerBracketFSM pFsm;
    if (
        _followerBrackets.TryGetValue(fleetEntryName, out pFsm)
        && pFsm != null
        && pFsm.State == FollowerBracketState.PendingSubmit
    )
    {
        pFsm.State = FollowerBracketState.Submitted;
        pFsm.LastUpdateUtc = DateTime.UtcNow;
    }

    FollowerBracketFSM fsm;
    if (_followerBrackets.TryGetValue(fleetEntryName, out fsm))
    {
        for (int i = 0; i < orderCount; i++)
        {
            var ord = orders[i];
            if (ord != null && !string.IsNullOrEmpty(ord.OrderId))
                _orderIdToFsmKey[ord.OrderId] = fleetEntryName;
        }
    }

    Print(string.Format("[PUMP] Submitted {0} orders for {1} | {2}",
        orderCount, fleetEntryName, acct.Name));
}
```

**Dependency graph:** `src/V12_002.SIMA.Fleet.cs` has 0 external import edges (self-contained partial class).

### get_call_hierarchy Result

**Callers (depth 2):**
- `ProcessFleetSlot` (depth 1, same file) — direct caller, NOT modified
- `PumpFleetDispatch` (depth 2, same file) — indirect via ProcessFleetSlot, NOT modified
- `ProcessValidPhotonSlot` (depth 2, same file) — indirect, NOT modified

**Callees (depth 1):**
- `ClearDispatchSyncPending` — `src/V12_002.SIMA.cs:179`
- `_followerBrackets` — field (ConcurrentDictionary), `src/V12_002.cs:829`
- `_orderIdToFsmKey` — field (ConcurrentDictionary), inferred
- `Print` — NinjaTrader base method (cold logging path)

All callers confirmed NOT in scope for modification.

---

## Sequential Thinking Evidence

### Thought 1: Complexity Drivers (CYC = 12)

The method performs four distinct concerns, each contributing to CYC:

| Concern | Code Block | CYC Contribution |
|---------|-----------|-----------------|
| A: Array slice prep | `if (orders != null && orderCount > 0 && orderCount < orders.Length)` | +3 (compound &&) |
| B: Submit + sync | `acct.Submit / ClearDispatchSyncPending / syncCleared = true` | +0 (no branches) |
| C: FSM state transition | `if (TryGetValue && pFsm != null && pFsm.State == PendingSubmit)` | +3 (compound &&) |
| D: Order ID registration | `if (TryGetValue) { for(...) { if (ord != null && !IsNullOrEmpty) } }` | +4 (if+for+compound if) |
| Base | — | +1 |
| **Total** | — | **12** |

### Thought 2: Extraction Strategy

Two private helper extractions reduce parent to CYC 4:

**Extraction 1: `UpdateFleetFsmState(string fleetEntryName)`**
- Absorbs Concern C: FSM state transition block
- Takes: `string fleetEntryName`
- Effect: transitions `PendingSubmit -> Submitted`, sets `LastUpdateUtc`
- Parent loses 3 CYC from compound-guard block

**Extraction 2: `RegisterOrderIdsToFsmKey(string fleetEntryName, Order[] orders, int orderCount)`**
- Absorbs Concern D: order ID registration loop
- Takes: `string fleetEntryName`, `Order[] orders`, `int orderCount`
- Effect: maps `ord.OrderId -> fleetEntryName` in `_orderIdToFsmKey`
- Parent loses 4 CYC (if + for + compound if)

**Concern A (array slice)** stays in parent — removing it would exceed extraction necessity; parent CYC after removing C+D = 4, which satisfies <= 8.

### Thought 3: CYC Validation

| Method | CYC Calculation | Result |
|--------|----------------|--------|
| `SubmitAndRegisterFleetOrders` (parent, after extraction) | 1 base + 3 (Concern A &&) = **4** | PASS (<= 8) |
| `UpdateFleetFsmState` | 1 base + 1 (TryGetValue) + 1 (null check) + 1 (state check) = **4** | PASS (<= 8) |
| `RegisterOrderIdsToFsmKey` | 1 base + 1 (TryGetValue) + 1 (for) + 1 (null) + 1 (IsNullOrEmpty) = **5** | PASS (<= 8) |

**max_cyc_projected = 5** (RegisterOrderIdsToFsmKey is highest)

All three methods satisfy CYC <= 8. No further extractions required.

---

## Extraction Plan

| Helper | Absorbs | Parameters | Est. CYC | Visibility |
|--------|---------|------------|----------|------------|
| `UpdateFleetFsmState` | FSM state transition (Concern C): TryGetValue + state guard + state write | `string fleetEntryName` | 4 | `private` |
| `RegisterOrderIdsToFsmKey` | Order ID registration (Concern D): TryGetValue + for loop + null/orderId guard + dict write | `string fleetEntryName, Order[] orders, int orderCount` | 5 | `private` |

**Parent after extraction:** CYC = 4

---

## Method Signatures

### Updated Parent (Signature Unchanged)

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

### New Helper 1

```csharp
private void UpdateFleetFsmState(string fleetEntryName)
```

### New Helper 2

```csharp
private void RegisterOrderIdsToFsmKey(
    string fleetEntryName,
    Order[] orders,
    int orderCount
)
```

---

## Jane Street KB Alignment

| Principle | Source | Application |
|-----------|--------|-------------|
| Zero-alloc hot path | carl_cook | No new allocations in helpers; array slice stays in parent (existing allocation) |
| Extract cold logging out-of-line | carl_cook | `Print(...)` stays in parent; could move out-of-line but not required for CYC compliance |
| AggressiveInlining hot / NoInlining cold | carl_cook | Extracted helpers are private; `[AggressiveInlining]` candidate for UpdateFleetFsmState (small, hot) |
| No new lock() blocks | gjengset | Neither helper introduces any lock; uses existing ConcurrentDictionary (lock-free) |
| 64-byte cache line alignment | gjengset | No struct layout changes; no new fields introduced |
| Single responsibility per helper | trading_billions | UpdateFleetFsmState: one concern only; RegisterOrderIdsToFsmKey: one concern only |
| Each helper CYC <= 8 | trading_billions | Max CYC = 5 (RegisterOrderIdsToFsmKey). PASS |
| No LINQ | carl_cook | No LINQ anywhere in the method or extracted helpers |
| Defense in depth | trading_billions | Null guards for `ord` and `ord.OrderId` preserved in RegisterOrderIdsToFsmKey |

---

## V12.23 Scope Compliance

| Check | Status |
|-------|--------|
| Single method targeted | PASS |
| Helpers extracted from subject only | PASS |
| No caller modifications (ProcessFleetSlot, PumpFleetDispatch, ProcessValidPhotonSlot) | PASS |
| No sibling method modifications | PASS |
| No cross-file refactoring | PASS |
| Helpers added as private methods in same partial class | PASS |
| Boundary matches 01-scope-boundary.md | PASS |

---

## Agent Tracking

| Field | Value |
|-------|-------|
| **Agent Name** | v12-phase2-architecture |
| **Epic** | EPIC-W7-061 |
| **Phase** | 2 |
| **Wave** | 7 |
| **CYC Baseline** | 12 |
| **max_cyc_projected** | 5 |
| **Extractions** | 2 |
| **Status** | completed |
