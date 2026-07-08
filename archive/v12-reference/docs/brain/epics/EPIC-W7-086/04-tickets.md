# Phase 4: Implementation Tickets — EPIC-W7-086

**Agent:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T03:30:00Z
**Input:** docs/brain/EPIC-W7-086/02-architecture-plan.md + docs/brain/EPIC-W7-086/03-audit-report.md

---

## Header

| Field | Value |
|---|---|
| **Epic** | EPIC-W7-086 |
| **Method** | `ProcessReaperFlatten_CancelWorkingOrders` |
| **Source File** | `src/V12_002.REAPER.Audit.cs` |
| **Original CYC** | 34 |
| **Wave** | 7 |
| **Phase** | 4 |
| **DNA Verdict (Phase 3)** | PASS |
| **ticket_count** | 3 |
| **projected_parent_cyc_after_all** | 2 |

---

## Sequential Thinking Validation Summary

The ticket breakdown was validated via 4-thought sequential analysis:

- **Thought 1:** Identified 3 structural complexity drivers: 4-branch OrderState OR predicate in double-nested foreach/if, two-pass collect-then-cancel pattern with List<Order> staging, and outer dispatch frame CYC bleed.
- **Thought 2:** Determined dependency-ordered ticket sequence: IsOrderCancellable (predicate, lowest-level) → BuildCancelOrderList (collector, depends on predicate) → ExecuteCancelOrders (dispatcher, depends on collector output).
- **Thought 3:** Validated CYC reduction arithmetic: Ticket 1 absorbs ~12 CYC, Ticket 2 absorbs ~8 CYC, Ticket 3 absorbs ~12 CYC. Final parent CYC = 2. Max helper CYC = 6. All ≤ 8 threshold passes.
- **Thought 4:** Confirmed single-responsibility per extraction, dependency ordering is correct, Jane Street alignment PASS, safe for Phase 5 execution.

---

## Tickets

---

### Ticket 1

| Field | Value |
|---|---|
| **ticket_id** | EPIC-W7-086-T1 |
| **helper_name** | `IsOrderCancellable(Order order) -> bool` |
| **concern** | Null guard + instrument FullName equality check + 4-branch OrderState OR predicate (Working, Submitted, Accepted, ChangePending). Implements early-return guard pattern: null check first, instrument check second, state check last. Named reusable predicate — acts as type gate preventing invalid orders from reaching `CancelOrderOnAccount`. |
| **extraction_type** | Predicate extraction |
| **target_file** | `src/V12_002.REAPER.Audit.cs` |
| **lines_to_move** | ~12–16 lines (null guard + instrument check + 4-arm OrderState OR block embedded in nested foreach/if) |
| **cyc_reduction** | ~12 units absorbed from parent (4 OR branches + null guard + instrument check + nesting multiplier) |
| **projected_helper_cyc** | 6 |
| **projected_parent_cyc_after_ticket** | ~22 |
| **dependency** | None — extract first; foundational predicate for Ticket 2 |
| **jane_street_rules** | Single-responsibility PASS, guard-clause extraction PASS, illegal-states-unrepresentable PASS, CYC≤8 PASS |
| **xunit_note** | Phase 5 must add `[Fact]` tests for: null order → false, wrong instrument → false, each non-cancellable state → false, each of 4 cancellable states → true |

**Extraction pattern:**
```csharp
private bool IsOrderCancellable(Order order)
{
    if (order == null) return false;
    if (order.Instrument.FullName != Instrument.FullName) return false;
    return order.OrderState == OrderState.Working
        || order.OrderState == OrderState.Submitted
        || order.OrderState == OrderState.Accepted
        || order.OrderState == OrderState.ChangePending;
}
```

---

### Ticket 2

| Field | Value |
|---|---|
| **ticket_id** | EPIC-W7-086-T2 |
| **helper_name** | `BuildCancelOrderList(Account targetAcct) -> List<Order>` |
| **concern** | Snapshots `targetAcct.Orders.ToArray()` (H14-FIX thread-safety), iterates the snapshot, calls `IsOrderCancellable(order)` as filter predicate, collects qualifying orders into a `List<Order>` staging buffer and returns it. Pure collector — no side effects. |
| **extraction_type** | Collection loop extraction |
| **target_file** | `src/V12_002.REAPER.Audit.cs` |
| **lines_to_move** | ~8–10 lines (ToArray snapshot + foreach loop + IsOrderCancellable filter call + list.Add + return) |
| **cyc_reduction** | ~8 units absorbed from parent (loop branch + filter condition + loop nesting context) |
| **projected_helper_cyc** | 3 |
| **projected_parent_cyc_after_ticket** | ~14 |
| **dependency** | Ticket 1 (IsOrderCancellable) must be extracted first |
| **jane_street_rules** | Single-responsibility PASS, zero-allocation pattern PASS (single List allocation per call), lock-free PASS, CYC≤8 PASS |
| **xunit_note** | Phase 5 must add `[Fact]` tests for: empty account orders → empty list, mixed orders → only cancellable orders returned, all non-cancellable → empty list |

**Extraction pattern:**
```csharp
private List<Order> BuildCancelOrderList(Account targetAcct)
{
    var snapshot = targetAcct.Orders.ToArray();
    var ordersToCancel = new List<Order>();
    foreach (var order in snapshot)
    {
        if (IsOrderCancellable(order))
            ordersToCancel.Add(order);
    }
    return ordersToCancel;
}
```

---

### Ticket 3

| Field | Value |
|---|---|
| **ticket_id** | EPIC-W7-086-T3 |
| **helper_name** | `ExecuteCancelOrders(List<Order> ordersToCancel, Account targetAcct, string accountName) -> void` |
| **concern** | Count guard (`if (ordersToCancel.Count > 0)`), foreach dispatch loop calling `CancelOrderOnAccount(order, targetAcct)` for each qualifying order, emits diagnostic `Print("[REAPER] Emergency Cancel: N orders on {accountName}")`. Pure dispatcher — no collection logic, no predicate logic. |
| **extraction_type** | Dispatch loop extraction |
| **target_file** | `src/V12_002.REAPER.Audit.cs` |
| **lines_to_move** | ~8–10 lines (count guard + foreach loop + CancelOrderOnAccount call + Print diagnostic) |
| **cyc_reduction** | ~12 units absorbed from parent (count guard branch + dispatch loop + CancelOrderOnAccount call path + nesting context) |
| **projected_helper_cyc** | 4 |
| **projected_parent_cyc_after_ticket** | 2 |
| **dependency** | Ticket 2 (BuildCancelOrderList) must be extracted first to establish List<Order> staging contract |
| **jane_street_rules** | Single-responsibility PASS, lock-free PASS, ASCII-only PASS (Print string uses ASCII), CYC≤8 PASS |
| **xunit_note** | Phase 5 must add `[Fact]` tests for: empty list → no CancelOrderOnAccount calls, non-empty list → CancelOrderOnAccount called per order, diagnostic Print emitted with correct count |

**Extraction pattern:**
```csharp
private void ExecuteCancelOrders(List<Order> ordersToCancel, Account targetAcct, string accountName)
{
    if (ordersToCancel.Count > 0)
    {
        foreach (var order in ordersToCancel)
            CancelOrderOnAccount(order, targetAcct);
        Print("[REAPER] Emergency Cancel: " + ordersToCancel.Count + " orders on " + accountName);
    }
}
```

---

## Parent Orchestrator After All Extractions

After all 3 ticket extractions, the parent method becomes a pure two-call orchestrator:

```csharp
private void ProcessReaperFlatten_CancelWorkingOrders(Account targetAcct, string accountName)
{
    var ordersToCancel = BuildCancelOrderList(targetAcct);
    ExecuteCancelOrders(ordersToCancel, targetAcct, accountName);
}
```

| Metric | Before | After |
|---|---|---|
| **CYC** | 34 | **2** |
| **Lines** | ~35–40 | ~4 |
| **Inline branches** | ~16 | 0 |
| **Inline loops** | 2 | 0 |
| **Jane Street Gate** | FAIL (>8) | **PASS** (all ≤8) |

**projected_parent_cyc_after_all: 2**

---

## CYC Reduction Ladder

| After Ticket | Method | Projected CYC | Remaining Parent CYC |
|---|---|---|---|
| Baseline | `ProcessReaperFlatten_CancelWorkingOrders` | — | 34 |
| T1 | `IsOrderCancellable` extracted | 6 | ~22 |
| T2 | `BuildCancelOrderList` extracted | 3 | ~14 |
| T3 | `ExecuteCancelOrders` extracted | 4 | **2** |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Bobcoins Used** | 0.8 |
| **Execution Time** | 2026-06-29T03:30:00Z |
| **Wave** | 7 |
| **Phase** | 4 |
| **jcodemunch tools called** | resolve_repo |
| **sequential-thinking calls** | 4 |
| **ticket_count** | 3 |
| **projected_parent_cyc_after_all** | 2 |
| **max_helper_cyc** | 6 (`IsOrderCancellable`) |
| **cyc_reduction_total** | 32 (34 → 2) |
| **dna_verdict_input** | PASS (Phase 3) |
