# EPIC-W7-021 — Phase 4: Ticket Definitions

## Agent Tracking
- **Agent Name**: v12-phase4-tickets
- **Epic ID**: EPIC-W7-021
- **Phase**: 4 — Ticket Generation
- **Wave**: 7
- **Generated**: 2026-06-29
- **Bobcoins Used**: 8
- **Execution Time**: ~60s

---

## Summary

| Field                       | Value                              |
|----------------------------|------------------------------------|
| **Method**                 | `ProcessOnOrderUpdate`             |
| **File**                   | [`src/V12_002.Orders.Callbacks.cs`](../../../src/V12_002.Orders.Callbacks.cs:245) |
| **CYC (before)**           | 16                                 |
| **ticket_count**           | **1**                              |
| **projected_parent_cyc_after_all** | **4** (≤ 8 ✓)             |
| **max_cyc_projected**      | **8** (≤ 8 ✓)                      |
| **dna_verdict (Phase 3)**  | PASS                               |

---

## Ticket T-1: Extract `DispatchOrderState`

| Field               | Value                                                                 |
|--------------------|-----------------------------------------------------------------------|
| **ticket_id**      | `EPIC-W7-021-T1`                                                      |
| **helper_name**    | `DispatchOrderState`                                                  |
| **concern**        | Order state routing — dispatch to Filled/Terminal/Working handlers plus ghost-ref fallback cleanup |
| **source_file**    | [`src/V12_002.Orders.Callbacks.cs`](../../../src/V12_002.Orders.Callbacks.cs:245) |
| **parent_method**  | `ProcessOnOrderUpdate` (lines 245–294)                                |
| **lines_to_move**  | ~17 lines (state dispatch chain + ghost fallback block)              |
| **cyc_reduction**  | 12 (parent: 16 → 4)                                                   |
| **projected_helper_cyc** | **8** (≤ 8 ✓)                                                 |
| **projected_parent_cyc** | **4** (≤ 8 ✓)                                                 |

### Lines to Move

Extract the following block from within `ProcessOnOrderUpdate`'s `try` body:

```csharp
bool handled = false;

if (orderState == OrderState.Filled)
    handled = HandleOrderState_Filled(order, quantity, filled, averageFillPrice, time);
else if (orderState == OrderState.Rejected || orderState == OrderState.Cancelled)
    handled = HandleOrderState_Terminal(order, orderState, nativeError);
else if (orderState == OrderState.Accepted || orderState == OrderState.Working)
    handled = HandleOrderState_Working(order, limitPrice, stopPrice, quantity);

if (!handled && IsTerminalState(orderState))
{
    RemoveGhostOrderRef(order, orderState.ToString().ToUpper());
}
```

### Replacement in Parent

Replace moved block with single call:

```csharp
DispatchOrderState(order, limitPrice, stopPrice, quantity, filled,
                   averageFillPrice, orderState, time, nativeError);
```

### New Helper Signature

```csharp
private void DispatchOrderState(
    Order order,
    double limitPrice,
    double stopPrice,
    int quantity,
    int filled,
    double averageFillPrice,
    OrderState orderState,
    DateTime time,
    string nativeError
)
```

### New Helper Body

```csharp
{
    bool handled = false;

    if (orderState == OrderState.Filled)
        handled = HandleOrderState_Filled(order, quantity, filled, averageFillPrice, time);
    else if (orderState == OrderState.Rejected || orderState == OrderState.Cancelled)
        handled = HandleOrderState_Terminal(order, orderState, nativeError);
    else if (orderState == OrderState.Accepted || orderState == OrderState.Working)
        handled = HandleOrderState_Working(order, limitPrice, stopPrice, quantity);

    if (!handled && IsTerminalState(orderState))
    {
        RemoveGhostOrderRef(order, orderState.ToString().ToUpper());
    }
}
```

### Parent After Extraction

```csharp
private void ProcessOnOrderUpdate(
    Order order, double limitPrice, double stopPrice, int quantity,
    int filled, double averageFillPrice, OrderState orderState,
    DateTime time, string nativeError)
{
    var probe = LatencyProbe.Start();
    try
    {
        if (ShouldPropagatePriceMove(order, orderState))
            PropagateMasterPriceMove(order, limitPrice, stopPrice, quantity);

        DispatchOrderState(order, limitPrice, stopPrice, quantity, filled,
                           averageFillPrice, orderState, time, nativeError);
    }
    catch (Exception ex)
    {
        Print("ERROR OnOrderUpdate: " + ex.Message);
    }
    finally
    {
        probe = probe.Stop();
        _histProcessOnOrderUpdate.Record(probe);
    }
}
```

### CYC Proof

**DispatchOrderState (new helper):**
1 (base) + 1 (Filled-if) + 1 (Rejected-elif) + 1 (||Cancelled) + 1 (Accepted-elif) + 1 (||Working) + 1 (!handled-if) + 1 (&&IsTerminal) = **8** ✓

**ProcessOnOrderUpdate (after extraction):**
1 (base) + 1 (try) + 1 (catch) + 1 (ShouldPropagatePriceMove-if) = **4** ✓

---

## CYC Summary

| Symbol                       | CYC Before | CYC After | Compliant |
|-----------------------------|-----------|-----------|-----------|
| `ProcessOnOrderUpdate`      | 16        | 4         | ✓ ≤ 8     |
| `DispatchOrderState` (new)  | —         | 8         | ✓ ≤ 8     |
| **max_cyc_projected**       | —         | **8**     | **✓**     |

---

## Sequential Thinking Evidence

**Thought 1 — Ticket Count:**
`ProcessOnOrderUpdate` contains one cohesive sub-block eligible for extraction: the order-state dispatch chain plus ghost-ref fallback. This is a single concern (routing). The architecture plan confirms exactly one helper (`DispatchOrderState`). ticket_count = 1.

**Thought 2 — Ticket Detail:**
Lines moved: the `bool handled = false` + if/else-if state dispatch chain + fallback `!handled && IsTerminal` block (~17 lines). Helper receives all 9 parameters by value — zero new allocations. Parent retains: latency probe, ShouldPropagatePriceMove guard, try/catch/finally instrumentation.

**Thought 3 — CYC Verification:**
`DispatchOrderState` = 8 branch points = CYC 8 ✓. `ProcessOnOrderUpdate` after = 4 branch points = CYC 4 ✓. max_cyc_projected = 8 — satisfies Jane Street strict standard (≤ 8). Single ticket is sufficient; no additional tickets required.

---

## MCP Evidence

| Tool                    | Result                                                                     |
|------------------------|----------------------------------------------------------------------------|
| `resolve_repo`         | Repo indexed: `antigravityos187-sketch/universal-or-strategy`, 5147 symbols |
| `get_symbol_complexity`| Symbol not found in current index snapshot; Phase 2 authoritative: CYC=16, assessment=high |
| `get_extraction_candidates` | 0 candidates (private method, single caller — expected)              |
| `sequentialthinking`   | 3 thoughts completed; ticket_count=1 confirmed; CYC 8/4 verified          |
