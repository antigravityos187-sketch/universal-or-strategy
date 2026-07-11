# EPIC-W7-151 — Phase 4: Implementation Tickets

**Agent:** v12-phase4-tickets
**Wave:** 7 | **Phase:** 4 — Ticket Generation
**Method:** `IsOrderAllowed` | **Source:** `src/V12_002.UI.Compliance.cs`
**Baseline CYC:** 9 | **Target CYC:** ≤ 8
**ticket_count:** 2

---

## Ticket Summary

| Ticket | Helper | CYC Removed | Projected Helper CYC |
|--------|--------|-------------|----------------------|
| T1 | `IsTrailingDrawdownAllowed` | 5 | 7 |
| T2 | `IsDailyProfitCapAllowed` | 3 | 4 |

**projected_parent_cyc_after_all: 3**

---

## Ticket T1

- **ticket_id:** T1
- **helper_name:** `IsTrailingDrawdownAllowed`
- **concern:** Trailing drawdown rule enforcement — TryGetValue compound gate (`peak > 0 && TrailingDrawdownLimit > 0`) + `currentAccount != null` guard + broker `Account.Get()` live call + try/catch + buffer check. NoInlining cold enforcement path.
- **lines_to_move:** Cluster A from IsOrderAllowed lines ~332-359: TryGetValue compound condition + null guard + try/catch + buffer check (5 CYC)
- **cyc_reduction:** 5
- **projected_helper_cyc:** 7

## Ticket T2

- **ticket_id:** T2
- **helper_name:** `IsDailyProfitCapAllowed`
- **concern:** Daily profit cap rule — SIMA+ConsistencyLock outer gate + inner TryGetValue compound condition for daily P&L check. NoInlining.
- **lines_to_move:** Cluster B from IsOrderAllowed lines ~361-376: outer gate + TryGetValue compound (3 CYC)
- **cyc_reduction:** 3
- **projected_helper_cyc:** 4

---

## projected_parent_cyc_after_all: 3

Parent `IsOrderAllowed` retains: feature-flag short-circuit + null guard + `IsTrailingDrawdownAllowed` call + `IsDailyProfitCapAllowed` call. CYC = 3.

---

## Agent Tracking

| Field | Value |
|-------|-------|
| Agent Name | v12-phase4-tickets |
| Bobcoins Used | 0.5 |
| Execution Time | 2026-06-29T23:00:00Z |
| Wave | 7 |
| Epic | EPIC-W7-151 |
