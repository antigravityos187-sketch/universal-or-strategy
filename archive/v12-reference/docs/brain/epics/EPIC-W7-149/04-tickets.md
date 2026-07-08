# EPIC-W7-149 — Phase 4: Implementation Tickets

**Agent:** v12-phase4-tickets
**Wave:** 7 | **Phase:** 4 — Ticket Generation
**Method:** `LogApexPerformance` | **Source:** `src/V12_002.UI.Compliance.cs`
**Baseline CYC:** 20 | **Target CYC:** ≤ 8
**ticket_count:** 3

---

## Ticket Summary

| Ticket | Helper | CYC Removed | Projected Helper CYC |
|--------|--------|-------------|----------------------|
| T1 | `ShouldSkipComplianceLog` | 3 | 3 |
| T2 | `BuildAccountJsonEntry` | 7 | 7 |
| T3 | `WriteComplianceJsonAsync` | 4 | 4 |

**projected_parent_cyc_after_all: 5**

---

## Ticket T1

- **ticket_id:** T1
- **helper_name:** `ShouldSkipComplianceLog`
- **concern:** Guard gate — enabled-flag OR path-null check (2 branch points) + 5-second throttle check (1 branch). Returns bool. Stateless predicate making compliance logging prerequisites an unbypassable contract.
- **lines_to_move:** Enabled + path-null OR-check + throttle `lastComplianceLog` comparison from top of LogApexPerformance
- **cyc_reduction:** 3
- **projected_helper_cyc:** 3

## Ticket T2

- **ticket_id:** T2
- **helper_name:** `BuildAccountJsonEntry`
- **concern:** Single-account JSON fragment — null-guard on acct, comma separator, `brokerPos` compound-&&, Long ternary for direction string, `expectedPositions` null lookup, `isConnected` ternary. Pure function returning string, no shared-state writes.
- **lines_to_move:** Full per-account JSON entry construction from foreach body (7 branch points)
- **cyc_reduction:** 7
- **projected_helper_cyc:** 7

## Ticket T3

- **ticket_id:** T3
- **helper_name:** `WriteComplianceJsonAsync`
- **concern:** Fire-and-forget async write — `Task.Run` path, path-null guard, `File.WriteAllText`, `SecurityException` catch, swallow catch. Threading order: `lastComplianceLog` is stamped in parent BEFORE this call fires.
- **lines_to_move:** Task.Run JSON write block + 2-layer try/catch from LogApexPerformance
- **cyc_reduction:** 4
- **projected_helper_cyc:** 4

---

## projected_parent_cyc_after_all: 5

Parent `LogApexPerformance` retains: base + if-ShouldSkip + outer-try + foreach + outer-catch. CYC = 5.

---

## Agent Tracking

| Field | Value |
|-------|-------|
| Agent Name | v12-phase4-tickets |
| Bobcoins Used | 0.6 |
| Execution Time | 2026-06-29T23:00:00Z |
| Wave | 7 |
| Epic | EPIC-W7-149 |
