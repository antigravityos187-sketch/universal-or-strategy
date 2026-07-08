# EPIC-W7-016 — Phase 4: Ticket Definitions

**Agent:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T01:20:00Z
**Inputs:** docs/brain/EPIC-W7-016/02-architecture-plan.md, docs/brain/EPIC-W7-016/03-audit-report.md

---

## Summary

| Field | Value |
|---|---|
| **Epic** | EPIC-W7-016 |
| **Method** | `TryHandleFleet_CancelAll` |
| **File** | `src/V12_002.UI.IPC.Commands.Fleet.cs` |
| **CYC Before** | 19 (MCP-confirmed Phase 2) |
| **ticket_count** | 3 |
| **projected_parent_cyc_after_all** | 4 |
| **max_cyc_projected** | 8 |
| **dna_verdict** | PASS (Phase 3) |

---

## Ticket Definitions

---

### Ticket 1 — Extract `CancelAll_IsActiveOrderState`

| Field | Value |
|---|---|
| **ticket_id** | 1 |
| **helper_name** | `CancelAll_IsActiveOrderState` |
| **concern** | Pure predicate: determines whether an order's state is eligible for cancellation (Working, Accepted, Submitted, ChangePending, or ChangeSubmitted) |
| **cyc_reduction** | 5 (removes the 5-way OrderState OR compound from the parent foreach-if condition) |
| **projected_helper_cyc** | 6 |
| **dependency** | None — implement first |

**Lines to Move:**

The 5-way `OrderState` compound OR predicate currently embedded inline inside the non-SIMA `foreach` guard:

```csharp
order.OrderState == OrderState.Working
    || order.OrderState == OrderState.Accepted
    || order.OrderState == OrderState.Submitted
    || order.OrderState == OrderState.ChangePending
    || order.OrderState == OrderState.ChangeSubmitted
```

**Target Implementation:**

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private bool CancelAll_IsActiveOrderState(Order order)
{
    return order.OrderState == OrderState.Working
        || order.OrderState == OrderState.Accepted
        || order.OrderState == OrderState.Submitted
        || order.OrderState == OrderState.ChangePending
        || order.OrderState == OrderState.ChangeSubmitted;
}
```

**Jane Street Notes:**
- `[AggressiveInlining]` — pure predicate, zero-allocation, called in hot foreach path
- No LINQ, no heap allocation
- CYC: base=1 + 5 OR conditions = **6** ✅

---

### Ticket 2 — Extract `CancelAll_IsBracketOrderName`

| Field | Value |
|---|---|
| **ticket_id** | 2 |
| **helper_name** | `CancelAll_IsBracketOrderName` |
| **concern** | Pure predicate: determines whether an order name belongs to a bracket/stop/target order that must be preserved (prefixed with Stop\_, S\_, T1\_–T5\_) |
| **cyc_reduction** | 7 (removes the 7-way StartsWith compound OR from the parent foreach inner-if block) |
| **projected_helper_cyc** | 8 |
| **dependency** | None — can implement in parallel with Ticket 1; must be complete before Ticket 3 |

**Lines to Move:**

The 7-way `StartsWith` compound OR predicate currently inline inside the non-SIMA `foreach` inner-if:

```csharp
oName.StartsWith("Stop_")
    || oName.StartsWith("S_")
    || oName.StartsWith("T1_")
    || oName.StartsWith("T2_")
    || oName.StartsWith("T3_")
    || oName.StartsWith("T4_")
    || oName.StartsWith("T5_")
```

**Target Implementation:**

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private bool CancelAll_IsBracketOrderName(string orderName)
{
    return orderName.StartsWith("Stop_")
        || orderName.StartsWith("S_")
        || orderName.StartsWith("T1_")
        || orderName.StartsWith("T2_")
        || orderName.StartsWith("T3_")
        || orderName.StartsWith("T4_")
        || orderName.StartsWith("T5_");
}
```

**Jane Street Notes:**
- `[AggressiveInlining]` — pure predicate, zero-allocation, called inside foreach (hot path)
- No LINQ, no heap allocation
- CYC: base=1 + 7 OR conditions = **8** ✅ (boundary-compliant)

---

### Ticket 3 — Extract `CancelAll_NonSimaPath` and Update Parent

| Field | Value |
|---|---|
| **ticket_id** | 3 |
| **helper_name** | `CancelAll_NonSimaPath` |
| **concern** | Encapsulates the entire non-SIMA cancel loop: iterates `Account.Orders`, applies both active-state and bracket-name predicates, calls `CancelOrderOnAccount` for eligible orders, and logs the result |
| **cyc_reduction** | 10 (removes the residual foreach + compound-check complexity from the parent's else-block; parent drops from 19 to 4) |
| **projected_helper_cyc** | 4 |
| **dependency** | Ticket 1 (CancelAll_IsActiveOrderState) and Ticket 2 (CancelAll_IsBracketOrderName) must be implemented first |

**Lines to Move:**

The entire non-SIMA `else`-block (~18 lines) from the parent's body:

```csharp
else
{
    int cancelled = 0;
    foreach (Order order in Account.Orders)
    {
        if (order != null
            && order.Instrument.FullName == Instrument.FullName
            && (
                order.OrderState == OrderState.Working
                || order.OrderState == OrderState.Accepted
                || order.OrderState == OrderState.Submitted
                || order.OrderState == OrderState.ChangePending
                || order.OrderState == OrderState.ChangeSubmitted
            ))
        {
            string oName = order.Name;
            if (
                oName.StartsWith("Stop_")
                || oName.StartsWith("S_")
                || oName.StartsWith("T1_")
                || oName.StartsWith("T2_")
                || oName.StartsWith("T3_")
                || oName.StartsWith("T4_")
                || oName.StartsWith("T5_")
            )
                continue;

            CancelOrderOnAccount(order, order.Account);
            cancelled++;
        }
    }
    Print($"[V12] CANCEL_ALL -> Cancelled {cancelled} pending entry orders");
}
```

**Target Implementation:**

```csharp
[MethodImpl(MethodImplOptions.NoInlining)]
private void CancelAll_NonSimaPath()
{
    int cancelled = 0;
    foreach (Order order in Account.Orders)
    {
        if (order != null
            && order.Instrument.FullName == Instrument.FullName
            && CancelAll_IsActiveOrderState(order))
        {
            if (CancelAll_IsBracketOrderName(order.Name))
                continue;

            CancelOrderOnAccount(order, order.Account);
            cancelled++;
        }
    }
    Print($"[V12] CANCEL_ALL -> Cancelled {cancelled} pending entry orders");
}
```

**Parent After Extraction:**

```csharp
private bool TryHandleFleet_CancelAll(string action, string cmdId)
{
    if (action != "CANCEL_ALL")
        return false;

    if (!MetadataGuardDuplicate(cmdId, action))
        return true;

    if (EnableSIMA)
    {
        int masterCancelled = CancelAll_ProcessMasterAccount();
        int fleetCancelled  = CancelAll_ProcessFleetAccounts();
        int totalCancelled  = masterCancelled + fleetCancelled;
        Print($"[SIMA] CANCEL_ALL -> Cancelled {totalCancelled} orders ...");
    }
    else
    {
        CancelAll_NonSimaPath();
    }

    return true;
}
```

**Jane Street Notes:**
- `[NoInlining]` — contains `Print()` logging (cold path); prevents JIT hot-path register pressure
- No new `lock()` blocks
- All reads (`Account.Orders`, `order.OrderState`) are pre-existing patterns — no new synchronization
- CYC: base=1 + foreach=+1 + compound-guard=+1 + bracket-skip=+1 = **4** ✅

---

## Complexity Summary (Post-Extraction)

| Symbol | CYC Before | CYC After | Status |
|---|---|---|---|
| `TryHandleFleet_CancelAll` (parent) | 19 | **4** | ✅ PASS (<= 8) |
| `CancelAll_IsActiveOrderState` (T1, new) | — | **6** | ✅ PASS (<= 8) |
| `CancelAll_IsBracketOrderName` (T2, new) | — | **8** | ✅ PASS (<= 8) |
| `CancelAll_NonSimaPath` (T3, new) | — | **4** | ✅ PASS (<= 8) |

**projected_parent_cyc_after_all: 4**
**max_cyc_projected: 8** ✅ (Jane Street <= 8 requirement met)

---

## Implementation Order

```
T1 (CancelAll_IsActiveOrderState)
T2 (CancelAll_IsBracketOrderName)
    ↓ (both T1 and T2 complete)
T3 (CancelAll_NonSimaPath + parent update)
```

T1 and T2 are independent and can be written in the same commit. T3 depends on both.

---

## Sequential Thinking Evidence

**Thought 1 — Ticket Count:** 3 tickets required (one per extracted helper). Ordering: T1 → T2 → T3, with T3 also updating the parent body. T1/T2 are independent; T3 consumes both.

**Thought 2 — Per-Ticket Detail:** T1 moves the 5-way OrderState predicate (CYC=6). T2 moves the 7-way StartsWith predicate (CYC=8). T3 moves the full non-SIMA else-block using T1/T2 as predicates (CYC=4). Parent else-block becomes a single `CancelAll_NonSimaPath()` call.

**Thought 3 — Verification:** All 4 symbols verified CYC <= 8. Parent=4, T1=6, T2=8, T3=4. Max=8 (boundary-compliant). Dependency order confirmed. Hypothesis VERIFIED — 3 tickets, complete and correct.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | batch |
| **Phase** | 4 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-016 |
| **MCP Tools Used** | resolve_repo, get_symbol_complexity (stale-index fallback to Phase 2 evidence), get_extraction_candidates, sequentialthinking (3 thoughts) |
| **Sequential Thinking Thoughts** | 3 |
| **ticket_count** | 3 |
| **projected_parent_cyc_after_all** | 4 |
| **max_cyc_projected** | 8 |
| **dna_verdict** | PASS (from Phase 3) |
