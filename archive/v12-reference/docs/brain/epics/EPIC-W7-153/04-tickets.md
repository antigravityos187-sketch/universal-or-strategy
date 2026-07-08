# EPIC-W7-153 — Phase 4: Implementation Tickets

**Agent:** v12-phase4-tickets
**Wave:** 7 | **Phase:** 4 — Ticket Generation
**Method:** `HandleTrimCommand` | **Source:** `src/V12_002.UI.IPC.Commands.Config.cs`
**Baseline CYC:** 20 | **Target CYC:** ≤ 8
**ticket_count:** 5

---

## Ticket Summary

| Ticket | Helper | CYC Removed | Projected Helper CYC |
|--------|--------|-------------|----------------------|
| T1 | `ComputeSafeTrimQty` | 4 | 3 |
| T2 | `BuildTrimSignalName` | 2 | 2 |
| T3 | `SubmitSimaTrimOrder` | 3 | 1 |
| T4 | `SubmitUnmanagedTrimOrder` | 4 | 1 |
| T5 | `TrimSinglePosition` | 5 | 6 |

**projected_parent_cyc_after_all: 3**

---

## Ticket T1

- **ticket_id:** T1
- **helper_name:** `ComputeSafeTrimQty`
- **concern:** Safe trim quantity computation — pure function: compute safe trim quantity from snapshot `int remaining` value and `double percent`; returns -1 sentinel when trim is mathematically impossible, making invalid quantity state unrepresentable at call site.
- **lines_to_move:** Quantity computation logic from HandleTrimCommand body
- **cyc_reduction:** 4
- **projected_helper_cyc:** 3

## Ticket T2

- **ticket_id:** T2
- **helper_name:** `BuildTrimSignalName`
- **concern:** Signal name construction — constructs "Trim_" + signalName and truncates to 50 chars if needed. Single string-concern helper.
- **lines_to_move:** Signal name build + truncation from foreach body
- **cyc_reduction:** 2
- **projected_helper_cyc:** 2

## Ticket T3

- **ticket_id:** T3
- **helper_name:** `SubmitSimaTrimOrder`
- **concern:** SIMA fleet follower order path — calls BuildTrimSignalName, Account.CreateOrder, Account.Submit, Print with fleet log format
- **lines_to_move:** SIMA fleet order submission block from TrimSinglePosition body
- **cyc_reduction:** 3
- **projected_helper_cyc:** 1

## Ticket T4

- **ticket_id:** T4
- **helper_name:** `SubmitUnmanagedTrimOrder`
- **concern:** NinjaTrader unmanaged order path — Print with IPC log format, then SubmitOrderUnmanaged. Direction branch eliminated by pre-computed OrderAction param.
- **lines_to_move:** Unmanaged order submission block from TrimSinglePosition body
- **cyc_reduction:** 4
- **projected_helper_cyc:** 1

## Ticket T5

- **ticket_id:** T5
- **helper_name:** `TrimSinglePosition`
- **concern:** Per-position trim orchestration — guard clause early return, ComputeSafeTrimQty call, SIMA vs unmanaged routing via `pos.IsFollower`, calls SubmitSimaTrimOrder or SubmitUnmanagedTrimOrder
- **lines_to_move:** Full per-position trim body from HandleTrimCommand foreach
- **cyc_reduction:** 5
- **projected_helper_cyc:** 6

---

## projected_parent_cyc_after_all: 3

Parent `HandleTrimCommand` retains: percent ternary (not a McCabe branch) + foreach loop + `RemainingContracts > 1` guard. CYC = 3.

---

## Agent Tracking

| Field | Value |
|-------|-------|
| Agent Name | v12-phase4-tickets |
| Bobcoins Used | 0.7 |
| Execution Time | 2026-06-29T23:00:00Z |
| Wave | 7 |
| Epic | EPIC-W7-153 |
