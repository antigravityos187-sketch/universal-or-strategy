# EPIC-W7-089 — Phase 2: Architecture Plan

**Agent Name:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T02:30:00Z
**Input:** docs/brain/EPIC-W7-089/01-scope-boundary.md

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | batch |
| **Phase** | 2 |
| **Wave** | 7 |

---

## MCP Evidence

**Repo Resolution:** `mcp__jcodemunch-mcp__resolve_repo`

| Field | Value |
|---|---|
| repo | antigravityos187-sketch/universal-or-strategy |
| status | loadable |
| indexed | true |
| symbol_count | 5147 |
| source_root | /home/malhitticrypto/universal-or-strategy |
| indexed_at | 2026-06-29T01:05:21Z |

Repo confirmed indexed and loadable. Source file `src/V12_002.Safety.Watchdog.cs` confirmed present with `CancelWatchdogWorkingOrders` at lines 138-165.

---

## Sequential Thinking Evidence

### Thought 1: Complexity Drivers Analysis

The CYC=10 decomposition maps directly to the hotspot data:

- **Base path:** +1
- **foreach (masterAccount.Orders.ToArray()):** +1 — collect-then-cancel snapshot pattern (H14-FIX)
- **null guard (order == null || order.Instrument == null):** +1 (if) + 1 (||) = effectively 2 branches within the guard
- **instrument filter (FullName != instrumentName):** +1
- **5-way OrderState OR-chain:** +5 (each `||` short-circuit is a branch: Working, Submitted, Accepted, ChangePending, ChangeSubmitted)
- **second foreach (ordersToCancel):** +1

**Primary driver:** The 5-way OrderState OR-chain (5 of 10 branches) is a self-contained classification concern — "is this order in a cancelable state?"

**Secondary concern:** The method fuses two distinct responsibilities: (a) collecting eligible orders and (b) dispatching cancellations. Per `trading_billions`, single responsibility per helper requires decomposition.

**Cold path:** The `Print(...)` log at line 163-164 fires only when cancellations occur — a cold path per `carl_cook` that should be extracted with `[NoInlining]`.

### Thought 2: Extraction Strategy

Three helpers naturally emerge from the complexity drivers:

1. **`IsOrderCancelable`** — extracts the 5-way OrderState OR-chain into a dedicated state classifier. Zero allocations, pure predicate, `[AggressiveInlining]` hot path.

2. **`CollectCancelableOrders`** — encapsulates the ToArray snapshot, null guard, instrument filter, and state check, building the `ordersToCancel` list. Preserves H14-FIX ToArray pattern for thread-safe enumeration on strategy thread.

3. **`LogWatchdogCancelCount`** — extracts the cold-path `Print` call per `carl_cook` cold-logging-out-of-line mandate. Decorated `[System.Runtime.CompilerServices.MethodImpl(MethodImplOptions.NoInlining)]`.

Orchestrator reduces to: collect → dispatch-loop → conditional-log. CYC = 3.

### Thought 3: CYC Validation

Full McCabe cyclomatic count per helper:

| Symbol | Branches | CYC |
|---|---|---|
| `IsOrderCancelable` | base(1) + 4× OR short-circuit = 5 | **5** |
| `CollectCancelableOrders` | base(1) + foreach(1) + null-guard-if(1) + null-guard-OR(1) + instrument-filter(1) + IsOrderCancelable-if(1) = 6 | **6** |
| `LogWatchdogCancelCount` | base(1) | **1** |
| `CancelWatchdogWorkingOrders` (orchestrator) | base(1) + foreach(1) + if-count-check(1) = 3 | **3** |

**max_cyc_projected = 6** — all helpers ≤ 8. Mandate satisfied.

---

## Extraction Plan

| Helper Name | Signature | Extracted Concern | Projected CYC |
|---|---|---|---|
| `IsOrderCancelable` | `private static bool IsOrderCancelable(Order order)` | 5-way OrderState OR-chain classifier | **5** |
| `CollectCancelableOrders` | `private static List<Order> CollectCancelableOrders(Account masterAccount, string instrumentName)` | ToArray snapshot + null guard + instrument filter + state check + list build | **6** |
| `LogWatchdogCancelCount` | `private void LogWatchdogCancelCount(int count)` | Cold-path Print logging (carl_cook NoInlining) | **1** |
| `CancelWatchdogWorkingOrders` (orchestrator) | `private void CancelWatchdogWorkingOrders(Account masterAccount, string instrumentName)` | Orchestration: collect + dispatch + log | **3** |

---

## Refactored Orchestrator Skeleton

```csharp
// Orchestrator — CYC = 3
private void CancelWatchdogWorkingOrders(Account masterAccount, string instrumentName)
{
    List<Order> ordersToCancel = CollectCancelableOrders(masterAccount, instrumentName);
    foreach (Order orderToCancel in ordersToCancel)
        CancelOrderOnAccount(orderToCancel, masterAccount);
    if (ordersToCancel.Count > 0)
        LogWatchdogCancelCount(ordersToCancel.Count);
}

// Helper 1: State classifier — CYC = 5
// [AggressiveInlining] — hot path predicate, zero alloc
[System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
private static bool IsOrderCancelable(Order order)
{
    return order.OrderState == OrderState.Working
        || order.OrderState == OrderState.Submitted
        || order.OrderState == OrderState.Accepted
        || order.OrderState == OrderState.ChangePending
        || order.OrderState == OrderState.ChangeSubmitted;
}

// Helper 2: Collect-then-cancel snapshot — CYC = 6
// Preserves H14-FIX ToArray pattern (same as W7-086)
private static List<Order> CollectCancelableOrders(Account masterAccount, string instrumentName)
{
    List<Order> result = new List<Order>();
    foreach (Order order in masterAccount.Orders.ToArray())
    {
        if (order == null || order.Instrument == null)
            continue;
        if (order.Instrument.FullName != instrumentName)
            continue;
        if (IsOrderCancelable(order))
            result.Add(order);
    }
    return result;
}

// Helper 3: Cold-path logger — CYC = 1
// [NoInlining] — cold path, per carl_cook out-of-line logging rule
[System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
private void LogWatchdogCancelCount(int count)
{
    Print("[WATCHDOG] Cancelled " + count + " master order(s) on strategy thread.");
}
```

---

## Jane Street Compliance

| Rule | Source | Applied? | Evidence |
|---|---|---|---|
| Zero-alloc hot path | carl_cook | YES | `IsOrderCancelable` is pure bool predicate, no allocation |
| Extract cold logging out-of-line | carl_cook | YES | `LogWatchdogCancelCount` with `[NoInlining]` |
| AggressiveInlining hot path | carl_cook | YES | `IsOrderCancelable` decorated |
| NoInlining cold path | carl_cook | YES | `LogWatchdogCancelCount` decorated |
| No LINQ | carl_cook | YES | ToArray() + foreach only, no `.Where()/.Select()` |
| No new lock() blocks | gjengset | YES | No locking introduced |
| Single responsibility per helper | trading_billions | YES | Each helper has exactly one concern |
| CYC <= 8 per helper | trading_billions | YES | max=6 (CollectCancelableOrders) |
| H14-FIX ToArray snapshot preserved | W7-086 pattern | YES | `CollectCancelableOrders` uses `.ToArray()` |

---

## Boundary Constraints

| Constraint | Status |
|---|---|
| Scope limited to `CancelWatchdogWorkingOrders` + extracted helpers | PASS |
| New helpers are private, same-file (same partial class) | PASS |
| Caller `ExecuteWatchdogLeadAccountFlatten` signature unchanged | PASS |
| No cross-file changes | PASS |
| No interface changes | PASS |
| `CancelOrderOnAccount` call sites unchanged | PASS |
| V12.23 No Scope Creep: ONE EPIC = ONE CONCERN | PASS |

---

## Summary

| Field | Value |
|---|---|
| **epic** | EPIC-W7-089 |
| **method** | CancelWatchdogWorkingOrders |
| **source** | src/V12_002.Safety.Watchdog.cs |
| **cyc_before** | 10 |
| **helpers_extracted** | 3 |
| **max_cyc_projected** | 6 |
| **orchestrator_cyc** | 3 |
| **boundary_verdict** | PASS |
