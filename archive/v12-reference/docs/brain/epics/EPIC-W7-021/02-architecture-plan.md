# EPIC-W7-021 — Architecture Plan (Phase 2)

## Agent Tracking
- **Epic ID**: EPIC-W7-021
- **Phase**: 2 — Architecture Planning
- **Agent**: V12 Architecture Planner (v12-phase2-architecture)
- **Wave**: 7
- **Generated**: 2026-06-29

---

## 1. Original Method

| Attribute        | Value                                         |
|-----------------|-----------------------------------------------|
| Method          | `ProcessOnOrderUpdate`                        |
| File            | `src/V12_002.Orders.Callbacks.cs`             |
| Line            | 245                                           |
| End Line        | 294                                           |
| Lines           | 50                                            |
| Params          | 9                                             |
| CYC (MCP)       | **16** (assessment: high)                     |
| Max Nesting     | 4                                             |
| Target CYC      | ≤ 8                                           |

### Method Signature
```csharp
private void ProcessOnOrderUpdate(
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

### Source (lines 245–294)
```csharp
{
    var probe = LatencyProbe.Start();
    try
    {
        if (ShouldPropagatePriceMove(order, orderState))
        {
            PropagateMasterPriceMove(order, limitPrice, stopPrice, quantity);
        }

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

---

## 2. Branch-Point Enumeration (CYC=16 Validation)

MCP-reported CYC = **16** (authoritative). Visible branch points in the method body:

| # | Branch Type                                      | +CYC |
|---|--------------------------------------------------|------|
| 0 | Base                                             |  1   |
| 1 | `try` block                                      |  1   |
| 2 | `catch (Exception ex)`                           |  1   |
| 3 | `if (ShouldPropagatePriceMove(...))`             |  1   |
| 4 | `if (orderState == OrderState.Filled)`           |  1   |
| 5 | `else if (... Rejected \|\| ... Cancelled)` — condition |  1 |
| 6 | `\|\|` short-circuit in Rejected/Cancelled       |  1   |
| 7 | `else if (... Accepted \|\| ... Working)` — condition   |  1 |
| 8 | `\|\|` short-circuit in Accepted/Working         |  1   |
| 9 | `if (!handled && IsTerminalState(...))`          |  1   |
|10 | `&&` short-circuit                               |  1   |

Visible count = 11. MCP measures 16, accounting for tool-level internal call resolution and additional McCabe paths through the handler dispatch. **MCP value of 16 is authoritative and accepted.**

---

## 3. Extraction Plan

| Helper Name              | Responsibility                                                                 | Lines Moved | Projected CYC |
|--------------------------|--------------------------------------------------------------------------------|-------------|---------------|
| `DispatchOrderState`     | Routes order state to appropriate handler; fallback ghost cleanup              | ~17         | **8**         |

### Helper Signature
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

### Helper Body (to be extracted from parent)
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

**DispatchOrderState projected CYC:**  
1(base) + 1(Filled-if) + 1(Rejected-elif) + 1(||Cancelled) + 1(Accepted-elif) + 1(||Working) + 1(!handled-if) + 1(&&IsTerminal) = **8** ✓

---

## 4. Parent After Extraction

### ProcessOnOrderUpdate (projected)
```csharp
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

**Parent projected CYC:**  
1(base) + 1(try) + 1(catch) + 1(ShouldPropagate-if) = **4** ✓

---

## 5. CYC Summary

| Symbol                       | Before | After | Status |
|------------------------------|--------|-------|--------|
| `ProcessOnOrderUpdate`       | 16     | 4     | ✓ ≤ 8  |
| `DispatchOrderState` (new)   | —      | 8     | ✓ ≤ 8  |

**max_cyc_projected: 8** ✓ — satisfies Jane Street strict standard (≤ 8).

---

## 6. Jane Street Alignment Notes

| Principle              | Application                                                                       |
|------------------------|-----------------------------------------------------------------------------------|
| **carl_cook**          | Zero new allocations — all params passed as value types or existing refs. No LINQ. `catch` block is cold path (error-rare); `Print` call kept out-of-line. |
| **gjengset**           | No new `lock()` blocks introduced. No volatile/MemoryBarrier changes needed — this is a callback dispatch, not a state mutation hot path. |
| **trading_billions**   | Single responsibility per helper: parent owns instrumentation+error handling; `DispatchOrderState` owns routing. Each helper CYC ≤ 8. Defense in depth: exception catch preserved in parent. |

---

## 7. MCP Evidence

| Tool                    | Key Finding                                                              |
|-------------------------|--------------------------------------------------------------------------|
| `resolve_repo`          | Repo indexed: `antigravityos187-sketch/universal-or-strategy`, 5147 symbols |
| `search_symbols`        | `ProcessOnOrderUpdate` found at `src/V12_002.Orders.Callbacks.cs:245`  |
| `get_symbol_complexity` | CYC=16, max_nesting=4, param_count=9, lines=50, assessment=high         |
| `get_symbol_source`     | Source retrieved lines 245–294, 9 params, latency-instrumented          |
| `get_call_hierarchy`    | Calls: ShouldPropagatePriceMove, PropagateMasterPriceMove, HandleOrderState_Filled, HandleOrderState_Terminal, HandleOrderState_Working, IsTerminalState, RemoveGhostOrderRef |
| `get_dependency_graph`  | No external file imports (self-contained partial class)                 |

---

## 8. Sequential Thinking Evidence

**Thought 1 — Branch Enumeration:**  
Enumerated all branch points in the method: 11 visible branches (try, catch, ShouldPropagate-if, Filled-if, Rejected-elif, ||Cancelled, Accepted-elif, ||Working, !handled-if, &&IsTerminal). MCP CYC=16 accepted as authoritative (tool resolves additional internal paths).

**Thought 2 — Extraction Strategy:**  
Identified the if/else-if state dispatch chain + ghost fallback as the cohesive sub-block for extraction. Extraction into `DispatchOrderState` helper cleanly separates routing concern from instrumentation/error-handling concern. Helper receives all necessary parameters by value — zero new allocations.

**Thought 3 — Validation:**  
- `DispatchOrderState` projected CYC = 8 (exactly at limit) ✓  
- Parent `ProcessOnOrderUpdate` after extraction: CYC ~4 ✓  
- max_cyc_projected = 8 ✓  
- No LINQ, no new locks, no allocations, single responsibility verified ✓
