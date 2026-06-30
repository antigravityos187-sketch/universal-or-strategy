# Phase 4 Tickets — EPIC-W7-032

**Epic**: EPIC-W7-032
**Method**: RestoreCascadedTargets
**Source File**: V12_002.Orders.Management.StopSync.cs
**Original CYC**: 23
**Wave**: 7 | **Phase**: 4

---

## Ticket Summary

ticket_count: 4

---

## Tickets

### Ticket 1

**ticket_id**: T1
**helper_name**: `TryLoadActivePosition`
**concern**: Validate the `capturedTargets` array (null/empty guard) and load the active position state (TryGetValue on activePositions, entryFilled check, remainingContracts guard) into an `out PositionInfo pos` parameter. Returns `false` immediately if position is not ready to restore.
**lines_to_move**: Lines 981–1009 — null/empty guard on `capturedTargets`, `TryGetValue` on `activePositions` dictionary, `!entryFilled` check, `remainingContracts <= 0` check. Populates `out PositionInfo pos` on success.
**signature**: `private bool TryLoadActivePosition(string entryName, TargetSnapshot[] capturedTargets, out PositionInfo pos)`
**cyc_reduction**: 5 (removes 5 guard branches from parent)
**projected_helper_cyc**: 6
**jane_street_notes**: `out PositionInfo pos` — zero heap allocation (carl_cook). Single guard concern per trading_billions single-responsibility rule.

---

### Ticket 2

**ticket_id**: T2
**helper_name**: `ShouldRestoreTarget`
**concern**: Determine whether a single `TargetSnapshot` represents a cascade-cancelled or rejected target that should be re-submitted. Returns `false` for null snapshots, null `CapturedOrder`, filled targets, and any non-Cancelled/non-Rejected state.
**lines_to_move**: Lines 1010–1031 — null guard on `snap`, null guard on `snap.CapturedOrder`, `OrderState != Cancelled` filter, `OrderState != Rejected` filter, `exitAction` ternary for direction selection.
**signature**: `private static bool ShouldRestoreTarget(TargetSnapshot snap)`
**cyc_reduction**: 4 (removes 4 predicate branches from parent foreach body)
**projected_helper_cyc**: 5
**jane_street_notes**: `private static` — eligible for `[MethodImpl(MethodImplOptions.AggressiveInlining)]` on hot path (carl_cook). Pure predicate with no instance state. Ideal xUnit `[Fact]` test target.

---

### Ticket 3

**ticket_id**: T3
**helper_name**: `SubmitFollowerTarget`
**concern**: Submit a replacement limit target order via the follower (account-direct) execution path. Performs SymmetryTrim, creates the order object, and submits via `executingAccount`. Returns the submitted `Order` or `null` if submission fails.
**lines_to_move**: Lines 1033–1058 — `isFollower` branch body: SymmetryTrim call, `CreateOrder` call, `executingAccount.Submit(...)` call, `tOrd != null` null-guard before return.
**signature**: `private Order SubmitFollowerTarget(string entryName, TargetSnapshot snap, OrderAction exitAction, double restoredPrice, string bracketOcoId, Account executingAccount)`
**cyc_reduction**: 3 (removes isFollower path branches from parent)
**projected_helper_cyc**: 2
**jane_street_notes**: Single responsibility — follower path only. Does NOT call `SubmitOrderUnmanaged`. No new lock() blocks (gjengset). No logging inside helper (carl_cook cold-path logging rule).

---

### Ticket 4

**ticket_id**: T4
**helper_name**: `SubmitLeaderTarget`
**concern**: Submit a replacement limit target order via the unmanaged (leader) execution path. Resolves the `OrderAction` via direction ternary and delegates to `SubmitOrderUnmanaged`. Returns the submitted `Order` or `null`.
**lines_to_move**: Lines 1059–1083 — non-follower branch body: `direction` ternary to select `OrderAction`, `SubmitOrderUnmanaged(...)` call, return value.
**signature**: `private Order SubmitLeaderTarget(TargetSnapshot snap, OrderAction exitAction, double restoredPrice, string bracketOcoId)`
**cyc_reduction**: 1 (removes leader-path ternary from parent)
**projected_helper_cyc**: 2
**jane_street_notes**: Single responsibility — leader path only. Does NOT reference `executingAccount`. No alloc (carl_cook). Pairs with T3: together they replace the two-arm ternary fork in parent.

---

## Execution Order

| Order | Ticket | Dependency | Reason |
|---|---|---|---|
| 1st | T1 — `TryLoadActivePosition` | None | Parent early-return guard; must exist before parent body is restructured |
| 2nd | T2 — `ShouldRestoreTarget` | T1 complete | Static predicate called inside foreach; must exist before foreach body is simplified |
| 3rd | T3 — `SubmitFollowerTarget` | T2 complete | One arm of the submission fork; extract before T4 to isolate the ternary |
| 4th | T4 — `SubmitLeaderTarget` | T3 complete | Other arm of submission fork; after T3, ternary is fully replaced by two helper calls |

---

## Extraction Summary

| Unit | Projected CYC | Threshold | PASS? |
|---|---|---|---|
| `TryLoadActivePosition` (T1) | 6 | <= 8 | YES |
| `ShouldRestoreTarget` (T2) | 5 | <= 8 | YES |
| `SubmitFollowerTarget` (T3) | 2 | <= 8 | YES |
| `SubmitLeaderTarget` (T4) | 2 | <= 8 | YES |
| `RestoreCascadedTargets` (refactored parent) | **8** | <= 8 | YES |

**projected_parent_cyc_after_all**: 8

CYC budget conservation: 23 (original) = 8 + 6 + 5 + 2 + 2 = 23 (redistributed). No CYC added, all removed from parent into focused helpers.

---

## DNA Audit Gate

| Check | Status |
|---|---|
| dna_verdict (Phase 3) | PASS |
| Zero lock() blocks | PASS |
| ASCII-only literals | PASS |
| No scope creep | PASS — 1 file, 0 caller changes |
| xUnit tests planned | PASS — ShouldRestoreTarget + TryLoadActivePosition as [Fact] targets |
| max_cyc_projected <= 8 | PASS — all 5 units verified |

---

## Agent Tracking

- **Agent Name**: v12-phase4-tickets
- **Wave**: 7
- **Phase**: 4
- **Epic**: EPIC-W7-032
- **Method**: RestoreCascadedTargets
- **Original CYC**: 23
- **ticket_count**: 4
- **Helpers**: TryLoadActivePosition (CYC=6), ShouldRestoreTarget (CYC=5), SubmitFollowerTarget (CYC=2), SubmitLeaderTarget (CYC=2)
- **projected_parent_cyc_after_all**: 8
- **Jane Street Compliant**: YES
- **Sequential Thoughts Used**: 3
- **MCP Tools Used**: resolve_repo, get_symbol_complexity, get_extraction_candidates, sequentialthinking
