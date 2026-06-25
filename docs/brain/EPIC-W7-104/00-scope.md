# Phase 1: Scope Definition - EPIC-W7-104

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.00
- **Execution Time**: 2026-06-23T02:54:17Z

---

## Method Under Refactoring

| Property            | Value                                        |
|---------------------|----------------------------------------------|
| **Method**          | `SubmitAndRegisterFleetOrders`               |
| **File**            | `src/V12_002.SIMA.Fleet.cs`                  |
| **Line**            | 174                                          |
| **Signature**       | `private void SubmitAndRegisterFleetOrders(Account acct, Order[] orders, int orderCount, string fleetEntryName, string expectedKey, ref bool syncCleared)` |
| **CYC (current)**   | 12                                           |
| **CYC (target)**    | ≤ 8                                          |
| **Lines of Code**   | 44                                           |
| **Max Nesting**     | 4                                            |
| **Parameters**      | 6                                            |

### Full Method Body (as-found)

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
    // Block 1: trim orders array to orderCount if necessary
    Order[] submitOrders = orders;
    if (orders != null && orderCount > 0 && orderCount < orders.Length)
    {
        submitOrders = new Order[orderCount];
        Array.Copy(orders, submitOrders, orderCount);
    }

    // Block 2: submit and mark sync cleared
    acct.Submit(submitOrders);
    ClearDispatchSyncPending(expectedKey);
    syncCleared = true;

    // Block 3: advance FSM state if in PendingSubmit
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

    // Block 4: register each order's ID → FSM key
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

---

## IN SCOPE — Extractions to Reduce CYC to ≤ 8

Three self-contained logical blocks within the method are candidates for extraction. Each extraction removes at least one branching node from the parent method's complexity count.

### Extraction 1 — `BuildSubmitOrderSlice`

| Property      | Detail |
|---------------|--------|
| **Extracts**  | Lines 183–188 (array-trim guard) |
| **Signature** | `private Order[] BuildSubmitOrderSlice(Order[] orders, int orderCount)` |
| **Branches removed from parent** | 3 (null check `orders != null`, `orderCount > 0`, `orderCount < orders.Length`) |
| **Rationale** | The trim-or-passthrough pattern is a pure, stateless transformation that has no side effects and is independently testable. |

### Extraction 2 — `AdvanceFsmToSubmitted`

| Property      | Detail |
|---------------|--------|
| **Extracts**  | Lines 194–203 (FSM state transition guard) |
| **Signature** | `private void AdvanceFsmToSubmitted(string fleetEntryName)` |
| **Branches removed from parent** | 3 (`TryGetValue` branch, `pFsm != null`, `pFsm.State == PendingSubmit`) |
| **Rationale** | FSM state advancement is a distinct responsibility. Extracting it makes the guard logic reusable and the intent readable at a glance. |

### Extraction 3 — `RegisterOrderIdsForFleetEntry`

| Property      | Detail |
|---------------|--------|
| **Extracts**  | Lines 205–214 (order-ID → FSM-key registration loop) |
| **Signature** | `private void RegisterOrderIdsForFleetEntry(Order[] orders, int orderCount, string fleetEntryName)` |
| **Branches removed from parent** | 3 (`TryGetValue` branch, `ord != null`, `!IsNullOrEmpty(ord.OrderId)`) |
| **Rationale** | Registration of order IDs is a bookkeeping concern separate from submission and FSM advancement. Isolation makes the loop independently testable. |

### Post-extraction CYC estimate for `SubmitAndRegisterFleetOrders`

Remaining branches in the orchestrating method after all three extractions:

| Remaining branch | Count |
|------------------|-------|
| Entry (method itself) | 1 |
| *(no remaining conditionals — all guards moved to helpers)* | 0 |

**Estimated residual CYC: ≤ 4** — well within the ≤ 8 target.

---

## OUT OF SCOPE

| Item | Reason |
|------|--------|
| **Public / internal signature of `SubmitAndRegisterFleetOrders`** | Must remain unchanged; callers (`ProcessFleetSlot`, `PumpFleetDispatch`, `ProcessValidPhotonSlot`, `VerifyPhotonSlotIntegrity`) must not be modified. |
| **Observable behavior** | No behavioral change permitted; pure structural refactoring only. |
| **Other methods in `V12_002.SIMA.Fleet.cs`** | `RollbackFleetDispatchState` and all other methods are untouched. |
| **Logging / Print call** | The `Print(...)` statement stays in the orchestrating method as-is. |
| **`ClearDispatchSyncPending` / `acct.Submit` call sites** | No reordering or wrapping of the submit + sync-clear sequence. |
| **Unit test files** | Phase 1 does not create or modify tests. |
| **Build artifacts** | No build, compile, or lint runs in Phase 1. |

---

## Extraction Plan — Proposed Helper Methods

```
SubmitAndRegisterFleetOrders   (orchestrator, CYC → ≤4)
│
├── BuildSubmitOrderSlice(orders, orderCount)
│       → returns Order[]  (trim array or return original)
│
├── AdvanceFsmToSubmitted(fleetEntryName)
│       → void  (guard + state transition + timestamp)
│
└── RegisterOrderIdsForFleetEntry(orders, orderCount, fleetEntryName)
        → void  (TryGetValue guard + registration loop)
```

All three helpers are `private`, live in the same class, and access the same instance fields (`_followerBrackets`, `_orderIdToFsmKey`) that the original method accesses. No new fields, properties, or injected dependencies are introduced.

---

## Risk Assessment

| Risk | Severity | Mitigation |
|------|----------|------------|
| Accidental behavior change in `BuildSubmitOrderSlice` | LOW | Pure function; return value directly replaces local `submitOrders` — semantically identical. |
| FSM double-lookup (two `TryGetValue` calls on same key) already exists in the original | LOW | Pre-existing; extraction preserves the pattern without introducing a new one. |
| `orders` reference vs `submitOrders` in `RegisterOrderIdsForFleetEntry` | LOW | Original code uses `orders[i]` (not `submitOrders[i]`) in the registration loop — the extraction must mirror this exactly. |
| Nesting depth in helpers | NONE | Each extracted helper has at most 2 levels of nesting (CYC ≤ 4 each). |
| Callers broken | NONE | Orchestrating method signature is frozen; callers are untouched. |

**Overall Phase 1 Risk: LOW**

---

## Success Criteria

| Criterion | Measurable Test |
|-----------|-----------------|
| `SubmitAndRegisterFleetOrders` CYC ≤ 8 | Static analysis on refactored method reports ≤ 8. |
| Signature unchanged | Grep for all 4 call sites confirms zero diff at call sites. |
| No new public API surface | All three helpers carry `private` visibility. |
| No behavioral change | All 12 original execution paths produce identical outcomes. |
| Three new helpers present | File contains `BuildSubmitOrderSlice`, `AdvanceFsmToSubmitted`, `RegisterOrderIdsForFleetEntry`. |
| No other methods modified | Git diff limited to `SubmitAndRegisterFleetOrders` block + 3 new helper method bodies. |
