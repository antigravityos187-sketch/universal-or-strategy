# EPIC-W7-098 — Phase 2: Architecture Plan

**Method:** ProcessFlattenWorkItem_CancelOrders
**File:** src/V12_002.SIMA.Flatten.cs
**CYC Baseline:** 17 | **Target:** <=8 | **Wave:** 7
**Generated:** 2026-06-29T00:00:00Z
**Agent:** v12-phase2-architecture

---

## Summary

Two private static helpers extracted to remove compound-condition branches from the main loop body.
`IsTerminalOrderState` encapsulates the 5-way OrderState OR check; `IsZombieTargetOrder` encapsulates
the 6-prefix StartsWith check. Both are marked `[AggressiveInlining]` per carl_cook zero-alloc hot-path
guidelines. Main method post-extraction reaches CYC=8.

---

## Complexity Drivers

| Driver | CYC Impact |
|---|---|
| Base | +1 |
| foreach over acct.Orders | +1 |
| null / instrument guards | +2 |
| isTerminal 5-state OR block | +5 |
| if (item.ZombieSweepOnly) | +1 |
| isZombieTarget 6-prefix check | +6 |
| ordersToCancel.Count > 0 | +1 |
| **Total** | **17** |

---

## Extraction Plan

| Helper Name | Signature | CYC | Jane Street Attributes | Rationale |
|---|---|---|---|---|
| IsTerminalOrderState | `private static bool IsTerminalOrderState(OrderState state)` | 6 | `[MethodImpl(MethodImplOptions.AggressiveInlining)]` | Hot-path: called per order in tight foreach loop; encapsulates 5 terminal state OR checks (Cancelled, CancelPending, CancelSubmitted, Filled, Rejected) |
| IsZombieTargetOrder | `private static bool IsZombieTargetOrder(string orderName)` | 7 | `[MethodImpl(MethodImplOptions.AggressiveInlining)]` | Hot-path: called per order when ZombieSweepOnly=true; encapsulates 6 prefix checks (EMERGENCY_STOP_, T1_, T2_, T3_, T4_, T5_) |

---

## max_cyc_projected: 8

Post-extraction main method path count:
`base(1) + foreach(1) + null guard(1) + instrument check(1) + IsTerminalOrderState call(1) + ZombieSweepOnly guard(1) + IsZombieTargetOrder call(1) + Count>0(1) = 8`

---

## MCP Evidence

- **get_context_bundle:** Source confirmed — 48-line method (lines 191–238) with foreach loop over
  `acct.Orders.ToArray()`, compound isTerminal bool block (5 OR conditions on OrderState), and
  ZombieSweepOnly block with 6 `StartsWith` OR conditions. Signature:
  `private void ProcessFlattenWorkItem_CancelOrders(FlattenWorkItem item, Account acct)`
- **get_call_hierarchy:** 2 direct callers at depth=1 — `PumpFlattenOps` (line 124) and
  `PerformFallbackFlatten` (line 328) — both upstream only in `src/V12_002.SIMA.Flatten.cs`.
  Method signature unchanged; callers are unaffected.
- **get_dependency_graph:** `src/V12_002.SIMA.Flatten.cs` has 0 importers and 0 inter-file imports
  in the dependency graph. Blast radius is strictly single-file.

---

## Sequential Thinking Evidence

- **Thought 1:** Compound conditions in isTerminal (5x OR on OrderState) and isZombieTarget (6x
  `StartsWith` OR) are the primary CYC drivers, accounting for 11 of the 17 total CYC points.
- **Thought 2:** Two static `[AggressiveInlining]` helpers remove 11 CYC from the main body;
  main method reaches CYC=8 after extraction. No state capture required — pure predicate helpers.
- **Thought 3:** All helpers <= CYC 8 (PASS). No new lock() blocks (gjengset PASS). No new LINQ
  (carl_cook PASS). Single responsibility per helper (trading_billions PASS). Architecture APPROVED.

---

## Jane Street Compliance

| Standard | Check | Status |
|---|---|---|
| carl_cook | No new LINQ; [AggressiveInlining] on hot-path helpers; static zero-alloc | PASS |
| gjengset | No new lock() blocks; no volatile/MemoryBarrier changes required | PASS |
| trading_billions | Single responsibility per helper; each helper CYC <= 8; defense in depth | PASS |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Wave** | 7 |
| **Phase** | 2 |
| **Epic** | EPIC-W7-098 |
| **Bobcoins Used** | 0.5 |
