# EPIC-W7-028 — Phase 2: Architecture Plan

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Epic:** EPIC-W7-028
**Method:** `ProcessFlattenWorkItem_CancelOrders`
**Source:** `src/V12_002.SIMA.Flatten.cs` (lines 191–238)
**CYC Baseline:** 9 (manifest cyc_confirmed) | **Target:** <= 8

---

## Extraction Plan

| # | New Helper | Extracted Logic | Projected CYC | Jane Street Rule |
|---|---|---|---|---|
| 1 | `IsTerminalOrderState(OrderState state) -> bool` | 5-way OR on Cancelled\|CancelPending\|CancelSubmitted\|Filled\|Rejected | ~6 | AggressiveInlining; zero-alloc predicate; single responsibility |
| 2 | `IsZombieTargetOrder(Order order) -> bool` | 6-way StartsWith OR for EMERGENCY_STOP_/T1_/T2_/T3_/T4_/T5_ names | ~7 | NoInlining (cold path); single responsibility; OrdinalIgnoreCase fast |

**Parent method after extraction (projected CYC ~6):**
- foreach loop (+1)
- null/instrument guard (+2)
- call IsTerminalOrderState (replaces inline branch, +0 branch, +0 CYC)
- if(item.ZombieSweepOnly) (+1)
- call IsZombieTargetOrder (replaces inline OR chain, +0 CYC)
- if(ordersToCancel.Count > 0) (+1)
- base (+1) = CYC ~6

**max_cyc_projected: 7** (IsZombieTargetOrder helper) — within <= 8 threshold.

---

## Extraction Boundary

```
KEEP in parent ProcessFlattenWorkItem_CancelOrders:
  List<Order> ordersToCancel = new List<Order>();
  foreach (Order order in acct.Orders.ToArray())
  {
      if (order == null || order.Instrument == null) continue;
      if (order.Instrument.FullName != Instrument.FullName) continue;
      if (IsTerminalOrderState(order.OrderState)) continue;   // <-- delegated
      if (item.ZombieSweepOnly)
      {
          if (!IsZombieTargetOrder(order)) continue;          // <-- delegated
      }
      ordersToCancel.Add(order);
  }
  if (ordersToCancel.Count > 0) { acct.Cancel(...); Print(...); }

EXTRACT to IsTerminalOrderState(OrderState state) -> bool:
  return state == OrderState.Cancelled
      || state == OrderState.CancelPending
      || state == OrderState.CancelSubmitted
      || state == OrderState.Filled
      || state == OrderState.Rejected;
  // [MethodImpl(MethodImplOptions.AggressiveInlining)]

EXTRACT to IsZombieTargetOrder(Order order) -> bool:
  return order.Name.StartsWith("EMERGENCY_STOP_", StringComparison.OrdinalIgnoreCase)
      || order.Name.StartsWith("T1_", StringComparison.OrdinalIgnoreCase)
      || order.Name.StartsWith("T2_", StringComparison.OrdinalIgnoreCase)
      || order.Name.StartsWith("T3_", StringComparison.OrdinalIgnoreCase)
      || order.Name.StartsWith("T4_", StringComparison.OrdinalIgnoreCase)
      || order.Name.StartsWith("T5_", StringComparison.OrdinalIgnoreCase);
  // [MethodImpl(MethodImplOptions.NoInlining)] -- cold path
```

---

## Jane Street KB Compliance

| Rule | Application |
|---|---|
| carl_cook: zero-alloc hot path | IsTerminalOrderState is pure value comparison — zero alloc |
| carl_cook: AggressiveInlining hot | IsTerminalOrderState marked [AggressiveInlining] — called in tight loop |
| carl_cook: NoInlining cold | IsZombieTargetOrder marked [NoInlining] — ZombieSweepOnly is cold path |
| carl_cook: avoid LINQ | No LINQ introduced; existing ToArray() is pre-existing pattern |
| gjengset: no new lock() blocks | Zero new locks |
| trading_billions: single responsibility | IsTerminalOrderState = state classification; IsZombieTargetOrder = name classification |
| trading_billions: CYC <= 8 | Parent ~6, helper1 ~6, helper2 ~7 — all <= 8 |
| trading_billions: defense in depth | Null/instrument guards remain first in parent loop |

---

## MCP Evidence

- **resolve_repo:** `antigravityos187-sketch/universal-or-strategy` — indexed, 5147 symbols
- **get_context_bundle:** Full source lines 191–238 retrieved; 48 lines; multi-branch isTerminal (5 OR) + ZombieSweepOnly block (6 StartsWith OR) are primary CYC drivers
- **get_call_hierarchy:** 2 direct callers (PumpFlattenOps, PerformFallbackFlatten); 3 depth-2 callers (FlattenAllApexAccounts, ChainNextFlattenOp, ClosePositionsOnlyApexAccounts). All callers confirmed unmodified.
- **get_dependency_graph:** Zero cross-file edges — blast radius fully self-contained to `src/V12_002.SIMA.Flatten.cs`

---

## Sequential Thinking Evidence

- **Thought 1 (complexity drivers):** CYC=9 from foreach + 5-OR isTerminal + ZombieSweepOnly block with 6-OR StartsWith chain + Count guard.
- **Thought 2 (extraction strategy):** Extract 2 boolean predicates: IsTerminalOrderState (5-OR) and IsZombieTargetOrder (6-OR). Each becomes a focused, testable helper.
- **Thought 3 (CYC validation):** Parent ~6, IsTerminalOrderState ~6, IsZombieTargetOrder ~7 — max 7, all <= 8. Jane Street inlining hints applied based on hot/cold classification.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Phase** | 2 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-028 |
| **Extractions Planned** | 2 |
| **max_cyc_projected** | 7 |
