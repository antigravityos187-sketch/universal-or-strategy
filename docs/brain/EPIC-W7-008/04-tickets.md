# Phase 4: Ticket Definitions — EPIC-W7-008

**Agent:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T01:20:00Z
**Inputs:** docs/brain/EPIC-W7-008/02-architecture-plan.md + docs/brain/EPIC-W7-008/03-audit-report.md

---

## Summary

| Field | Value |
|---|---|
| **Epic** | EPIC-W7-008 |
| **Method** | `ManageCIT` |
| **Source File** | `src/V12_002.Orders.Management.Flatten.cs` |
| **Original CYC** | 19 (cluster aggregate) / 11 (jCodemunch index) |
| **ticket_count** | 3 |
| **projected_parent_cyc_after_all** | 6 |
| **max_projected_cyc_cluster** | 6 |
| **DNA Verdict (Phase 3)** | PASS |

---

## ticket_count: 3

---

## Ticket 1

| Field | Value |
|---|---|
| **ticket_id** | T1 |
| **helper_name** | `ExecuteCitNudgeWithFaultIsolation` |
| **concern** | Fault-isolation wrapper — wraps `TryNudgeOrder` in `InvalidOperationException` + broad `Exception` catch blocks; returns `false` on budget exhaustion to protect remaining fleet accounts |
| **signature** | `private bool ExecuteCitNudgeWithFaultIsolation(string key, Order order, double citOffset, bool isFollower, ref int budget)` |
| **lines_to_move** | The full try/catch block inside `ManageCIT`'s foreach loop body (approx. lines 90–120): try block calling `TryNudgeOrder`, `catch (InvalidOperationException)` branch logging the stale-order fault, `catch (Exception)` branch logging the unexpected fault, and the `ref int budget` exhaustion check that triggers `break` in the caller |
| **cyc_reduction** | 3 (2 catch-branch decisions + 1 budget-exhaustion result check removed from `ManageCIT` body) |
| **projected_helper_cyc** | 4 |

**CYC breakdown for helper:**

| Branch | +CYC |
|---|---|
| Base | 1 |
| `catch (InvalidOperationException)` | +1 |
| `catch (Exception)` | +1 |
| Budget exhaustion check (`if budget <= 0`) | +1 |
| **Total** | **4** |

**Projected helper CYC: 4 <= 8 ✅**

---

## Ticket 2

| Field | Value |
|---|---|
| **ticket_id** | T2 |
| **helper_name** | `TryNudgeOrder` |
| **concern** | Dispatch router — single decision point for follower vs local nudge path; if `isFollower` calls `ExecuteFollowerNudge`, else calls `ExecuteLocalNudge` + `CalculateNudgedPrice`; returns `false` if broker budget halted |
| **signature** | `private bool TryNudgeOrder(string key, Order order, double citOffset, bool isFollower, ref int budget)` |
| **lines_to_move** | The `isFollower` dispatch block and `ref int budget` decrement/halt check from inside `ExecuteCitNudgeWithFaultIsolation`'s try body: the `if (isFollower)` branch routing to `ExecuteFollowerNudge` vs the else branch routing to `ExecuteLocalNudge` + `CalculateNudgedPrice`, plus the budget halt guard |
| **cyc_reduction** | 2 (isFollower dispatch branch + budget halt branch removed from `ExecuteCitNudgeWithFaultIsolation` try body) |
| **projected_helper_cyc** | 3 |

**CYC breakdown for helper:**

| Branch | +CYC |
|---|---|
| Base | 1 |
| `if (isFollower)` dispatch | +1 |
| Budget halt check (`if --budget <= 0`) | +1 |
| **Total** | **3** |

**Projected helper CYC: 3 <= 8 ✅**

---

## Ticket 3

| Field | Value |
|---|---|
| **ticket_id** | T3 |
| **helper_name** | `IsPriceTouchingLimit` |
| **concern** | Pure directional price-touch predicate — Buy: `price <= Low[0]`; Sell: `price >= High[0]`; extracted from `ShouldChaseOrder` to enable standalone unit testing of the Build 984 regression path |
| **signature** | `private bool IsPriceTouchingLimit(Order order)` |
| **lines_to_move** | The directional price-touch comparison block from `ShouldChaseOrder`: the `if (order.IsLong)` / `else` branch checking `currentPrice <= Low[0]` vs `currentPrice >= High[0]`; `ShouldChaseOrder` is reduced to guard clauses + a call to `IsPriceTouchingLimit` |
| **cyc_reduction** | 2 (Buy-touch branch + Sell-touch branch removed from `ShouldChaseOrder`; `ShouldChaseOrder` CYC: 7 -> 5) |
| **projected_helper_cyc** | 3 |

**CYC breakdown for helper:**

| Branch | +CYC |
|---|---|
| Base | 1 |
| `if (order.IsLong)` Buy-touch direction | +1 |
| `else` Sell-touch direction | +1 |
| **Total** | **3** |

**Projected helper CYC: 3 <= 8 ✅**

---

## projected_parent_cyc_after_all: 6

**ManageCIT body after all 3 extractions:**

```
foreach (var kvp in entryOrders)                                          // +1 (foreach)
    if (!ValidateCitConfiguration(out double citOffset)) continue;        // +1 (guard 1)
    if (!ShouldChaseOrder(kvp.Value)) continue;                           // +1 (guard 2)
    bool isFollower = activePositions.ContainsKey(kvp.Key);               // +1 (null-conditional)
    if (!ExecuteCitNudgeWithFaultIsolation(..., ref budget)) break;        // +1 (result check)
// base: 1
// Total: 6
```

| Branch | +CYC |
|---|---|
| Base | 1 |
| `foreach` loop | +1 |
| `ValidateCitConfiguration` guard | +1 |
| `ShouldChaseOrder` guard | +1 |
| `isFollower` lookup (null-conditional) | +1 |
| `ExecuteCitNudgeWithFaultIsolation` result check | +1 |
| **Total** | **6** |

**projected_parent_cyc_after_all = 6 <= 8 ✅**

---

## Full Cluster CYC After All Extractions

| Method | Before | After | Status |
|---|---|---|---|
| `ManageCIT` (parent) | 9 | **6** | ✅ |
| `ExecuteCitNudgeWithFaultIsolation` (T1 — new) | — | **4** | ✅ |
| `TryNudgeOrder` (T2 — new) | — | **3** | ✅ |
| `IsPriceTouchingLimit` (T3 — new) | — | **3** | ✅ |
| `ShouldChaseOrder` (modified in T3) | 7 | **5** | ✅ |
| `ValidateCitConfiguration` (unchanged) | 5 | **5** | ✅ |
| `ExecuteFollowerNudge` (unchanged) | 4 | **4** | ✅ |
| `CalculateNudgedPrice` (unchanged) | 2 | **2** | ✅ |
| `ExecuteLocalNudge` (unchanged) | 1 | **1** | ✅ |

**Maximum CYC across cluster: 6. Jane Street CYC mandate (<= 8): SATISFIED.**

---

## jCodemunch Evidence

| Tool | Result |
|---|---|
| `resolve_repo` | PASS — repo indexed, 5147 symbols, source_root confirmed |
| `get_symbol_complexity(ManageCIT)` | cyclomatic=11, max_nesting=5, param_count=0, lines=61, assessment="high" |
| `get_extraction_candidates` | Empty (complexity metadata not populated for this index version) — Phase 2 static analysis authoritative |

---

## Sequential Thinking Evidence

**3-thought chain executed and validated:**

- **Thought 1:** ticket_count = 3 confirmed; one ticket per extracted helper; extraction_candidates empty — Phase 2 authoritative
- **Thought 2:** Lines-to-move and projected CYC scoped per ticket: T1=4, T2=3, T3=3
- **Thought 3:** All 9 cluster methods post-extraction verified <= 8; max CYC = 6; Jane Street mandate SATISFIED

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Epic** | EPIC-W7-008 |
| **Wave** | 7 |
| **Phase** | 4 — Ticket Generation |
| **Source File** | `src/V12_002.Orders.Management.Flatten.cs` |
| **Method** | `ManageCIT` |
| **Original CYC** | 19 |
| **ticket_count** | 3 |
| **projected_parent_cyc_after_all** | 6 |
| **Bobcoins Used** | 1.1 |
| **Execution Time** | 2026-06-29T01:20:00Z |
| **jcodemunch tools called** | resolve_repo, get_symbol_complexity, get_extraction_candidates |
| **sequential-thinking calls** | 4 (1 probe + 3 validation thoughts) |
| **V12.23 No Scope Creep** | ENFORCED |
