# EPIC-W7-098 — Phase 4: Implementation Tickets

**Method:** ProcessFlattenWorkItem_CancelOrders
**File:** src/V12_002.SIMA.Flatten.cs
**CYC Baseline:** 17 | **Target:** <=8 | **Wave:** 7
**Generated:** 2026-06-29T00:00:00Z
**Agent:** v12-phase4-tickets
**ticket_count:** 2
**projected_parent_cyc_after_all:** 8

---

## Overview

Two surgical extraction tickets reduce `ProcessFlattenWorkItem_CancelOrders` from CYC=17 to CYC=8.
Each ticket produces one private static `[AggressiveInlining]` helper via compound-condition extraction.
Both extractions are single-file, zero-blast-radius, and DNA-PASS verified.

---

## Ticket 1 — Extract IsTerminalOrderState

| Field | Value |
|---|---|
| **ticket_id** | EPIC-W7-098-T1 |
| **helper_name** | IsTerminalOrderState |
| **concern** | Encapsulate 5-way OrderState OR check (Cancelled, CancelPending, CancelSubmitted, Filled, Rejected) into a named predicate |
| **extraction** | Move the compound `state == OrderState.Cancelled \|\| state == OrderState.CancelPending \|\| state == OrderState.CancelSubmitted \|\| state == OrderState.Filled \|\| state == OrderState.Rejected` block out of `ProcessFlattenWorkItem_CancelOrders` into a new private static helper |
| **lines_to_move** | ~5 lines (the isTerminal bool compound-condition block, lines ~198–202 of src/V12_002.SIMA.Flatten.cs) |
| **signature** | `private static bool IsTerminalOrderState(OrderState state)` |
| **attributes** | `[MethodImpl(MethodImplOptions.AggressiveInlining)]` |
| **cyc_reduction** | -5 (5 OR branch points replaced by single call site in parent) |
| **projected_helper_cyc** | 6 (base=1 + 5 OR conditions) |
| **parent_cyc_after_ticket** | 12 (17 - 5) |

### Implementation Steps

1. Add `[MethodImpl(MethodImplOptions.AggressiveInlining)]` helper below the parent method in `src/V12_002.SIMA.Flatten.cs`.
2. Helper body: return the 5-way OR expression on `OrderState` enum values.
3. In parent method: replace the compound bool block with `IsTerminalOrderState(order.OrderState)`.
4. Verify: `dotnet build` passes, CYC of `IsTerminalOrderState` = 6.

### Acceptance Criteria

- [ ] Helper `IsTerminalOrderState` exists as `private static bool` in `src/V12_002.SIMA.Flatten.cs`
- [ ] Helper is decorated with `[MethodImpl(MethodImplOptions.AggressiveInlining)]`
- [ ] Helper CYC = 6 (verified by complexity audit)
- [ ] Parent method no longer contains the 5-way OrderState OR block inline
- [ ] Build passes with zero errors
- [ ] No new lock() blocks introduced

---

## Ticket 2 — Extract IsZombieTargetOrder

| Field | Value |
|---|---|
| **ticket_id** | EPIC-W7-098-T2 |
| **helper_name** | IsZombieTargetOrder |
| **concern** | Encapsulate 6-prefix StartsWith check (EMERGENCY_STOP_, T1_, T2_, T3_, T4_, T5_) into a named predicate |
| **extraction** | Move the compound `orderName.StartsWith("EMERGENCY_STOP_") \|\| orderName.StartsWith("T1_") \|\| ... \|\| orderName.StartsWith("T5_")` block out of `ProcessFlattenWorkItem_CancelOrders` into a new private static helper |
| **lines_to_move** | ~6 lines (the isZombieTarget bool compound-condition block inside the ZombieSweepOnly guard, lines ~215–220 of src/V12_002.SIMA.Flatten.cs) |
| **signature** | `private static bool IsZombieTargetOrder(string orderName)` |
| **attributes** | `[MethodImpl(MethodImplOptions.AggressiveInlining)]` |
| **cyc_reduction** | -4 (6 StartsWith OR branch points replaced by single call site; net parent delta -4 after call-site path) |
| **projected_helper_cyc** | 7 (base=1 + 6 StartsWith OR conditions) |
| **parent_cyc_after_ticket** | 8 (12 - 4) |

### Implementation Steps

1. Add `[MethodImpl(MethodImplOptions.AggressiveInlining)]` helper below `IsTerminalOrderState` in `src/V12_002.SIMA.Flatten.cs`.
2. Helper body: return the 6-way `StartsWith` OR expression on the `orderName` string parameter.
3. In parent method (inside the `ZombieSweepOnly` guard): replace the compound bool block with `IsZombieTargetOrder(order.Name)`.
4. Verify: `dotnet build` passes, CYC of `IsZombieTargetOrder` = 7, parent CYC = 8.

### Acceptance Criteria

- [ ] Helper `IsZombieTargetOrder` exists as `private static bool` in `src/V12_002.SIMA.Flatten.cs`
- [ ] Helper is decorated with `[MethodImpl(MethodImplOptions.AggressiveInlining)]`
- [ ] Helper CYC = 7 (verified by complexity audit)
- [ ] Parent method no longer contains the 6-way StartsWith block inline
- [ ] Build passes with zero errors
- [ ] Parent method `ProcessFlattenWorkItem_CancelOrders` final CYC = 8
- [ ] No new lock() blocks introduced

---

## CYC Reduction Summary

| Component | Baseline CYC | Projected CYC | Delta |
|---|---|---|---|
| ProcessFlattenWorkItem_CancelOrders (main) | 17 | 8 | -9 |
| IsTerminalOrderState (new helper) | N/A | 6 | — |
| IsZombieTargetOrder (new helper) | N/A | 7 | — |
| **projected_parent_cyc_after_all** | **17** | **8** | **-9** |

---

## Sequential Thinking Validation

- **Thought 1:** IsTerminalOrderState and IsZombieTargetOrder are independent extractions targeting different code regions — 2 tickets is the correct breakdown.
- **Thought 2:** Ticket sequencing validated: T1 then T2; parent CYC transitions 17→12→8. Both helpers are private static within the same file; zero cross-file blast radius.
- **Thought 3:** All projected CYC values <= 8 (PASS). No new lock() blocks. No LINQ. ASCII-only identifiers. ticket_count=2, projected_parent_cyc_after_all=8 confirmed.

---

## Jane Street Compliance

| Standard | Requirement | Status |
|---|---|---|
| carl_cook | No new LINQ; [AggressiveInlining] on hot-path helpers; zero-alloc static predicates | PASS |
| gjengset | No new lock() blocks; no volatile/MemoryBarrier changes | PASS |
| trading_billions | Single responsibility per helper; each helper CYC <= 8 | PASS |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Wave** | 7 |
| **Phase** | 4 |
| **Epic** | EPIC-W7-098 |
| **ticket_count** | 2 |
| **projected_parent_cyc_after_all** | 8 |
| **Bobcoins Used** | 0.3 |
| **MCP Tools Called** | resolve_repo, sequentialthinking (x3), read_file (x2) |
| **dna_verdict** | PASS (inherited from Phase 3) |
