# Phase 4: Implementation Tickets — EPIC-W7-049

**Epic:** EPIC-W7-049
**Method:** ManageTrail_RunPerTradeBranches
**Source:** src/V12_002.Trailing.cs
**Original CYC:** 11
**Wave:** 7 | **Phase:** 4 — Ticket Generation

---

## ticket_count: 3

---

## Ticket 1

- **ticket_id:** 1
- **helper_name:** `IsTRENDEntry1EMACandidate`
- **concern:** Extract compound TREND Entry-1 EMA eligibility predicate into a named static helper
- **lines_to_move:** Boolean expression `pos.IsTRENDTrade && pos.IsTRENDEntry1 && !pos.IsRMATrade` from the first if-guard inside `ManageTrail_RunPerTradeBranches` (parent line ~244); replace parent guard with `if (IsTRENDEntry1EMACandidate(pos)) return TrailHandler_TREND_E1(entryName, pos);`
- **helper_signature:** `private static bool IsTRENDEntry1EMACandidate(PositionInfo pos)`
- **helper_body:** `=> pos.IsTRENDTrade && pos.IsTRENDEntry1 && !pos.IsRMATrade;`
- **cyc_reduction:** −3 (removes 2 `&&` boolean edges + 1 `!` negation from parent)
- **projected_helper_cyc:** 4
- **projected_parent_cyc_after_ticket:** 8
- **test:** `[Fact] void IsTRENDEntry1EMACandidate_ReturnsFalse_WhenRMATrade()` — Assert.False when `pos.IsRMATrade = true`; Assert.True when TREND+Entry1+NonRMA

---

## Ticket 2

- **ticket_id:** 2
- **helper_name:** `IsTRENDEntry2EMACandidate`
- **concern:** Extract compound TREND Entry-2 EMA eligibility predicate into a named static helper
- **lines_to_move:** Boolean expression `pos.IsTRENDTrade && pos.IsTRENDEntry2 && !pos.IsRMATrade` from the second if-guard (parent line ~248); replace parent guard with `if (IsTRENDEntry2EMACandidate(pos)) return TrailHandler_TREND_E2(entryName, pos);`
- **helper_signature:** `private static bool IsTRENDEntry2EMACandidate(PositionInfo pos)`
- **helper_body:** `=> pos.IsTRENDTrade && pos.IsTRENDEntry2 && !pos.IsRMATrade;`
- **cyc_reduction:** −3 (removes 2 `&&` boolean edges + 1 `!` negation from parent)
- **projected_helper_cyc:** 4
- **projected_parent_cyc_after_ticket:** 5
- **test:** `[Fact] void IsTRENDEntry2EMACandidate_ReturnsFalse_WhenRMATrade()` — Assert.False when `pos.IsRMATrade = true`; Assert.True when TREND+Entry2+NonRMA

---

## Ticket 3

- **ticket_id:** 3
- **helper_name:** `IsRetestEMACandidate`
- **concern:** Extract compound RETEST EMA eligibility predicate into a named static helper
- **lines_to_move:** Boolean expression `pos.IsRetestTrade && !pos.IsRMATrade` from the third if-guard (parent line ~252); replace parent guard with `if (IsRetestEMACandidate(pos)) return TrailHandler_RETEST(entryName, pos);`
- **helper_signature:** `private static bool IsRetestEMACandidate(PositionInfo pos)`
- **helper_body:** `=> pos.IsRetestTrade && !pos.IsRMATrade;`
- **cyc_reduction:** −1 (removes 1 `&&` edge + 1 `!` negation from parent; net parent -1 since one if-node branch remains)
- **projected_helper_cyc:** 3
- **projected_parent_cyc_after_ticket:** 4
- **test:** `[Fact] void IsRetestEMACandidate_ReturnsFalse_WhenRMATrade()` — Assert.False when `pos.IsRMATrade = true`; Assert.True when Retest+NonRMA

---

## Resulting Parent Method (after all tickets)

```csharp
private bool ManageTrail_RunPerTradeBranches(string entryName, PositionInfo pos)
{
    if (IsTRENDEntry1EMACandidate(pos)) return TrailHandler_TREND_E1(entryName, pos);
    if (IsTRENDEntry2EMACandidate(pos)) return TrailHandler_TREND_E2(entryName, pos);
    if (IsRetestEMACandidate(pos))      return TrailHandler_RETEST(entryName, pos);
    return false;
}
```

---

## projected_parent_cyc_after_all: 4

---

## CYC Reduction Summary

| Ticket | Helper | Helper CYC | Parent CYC After |
|---|---|---|---|
| Baseline | — | — | 11 |
| 1 | `IsTRENDEntry1EMACandidate` | 4 | 8 |
| 2 | `IsTRENDEntry2EMACandidate` | 4 | 5 |
| 3 | `IsRetestEMACandidate` | 3 | 4 |

**Total reduction:** −7 points (target was ≥3). All values ≤ 8 (Jane Street strict). ✅

---

## DNA Compliance

| Check | Result |
|---|---|
| CYC <= 8 for all symbols | PASS — max = 4 |
| Single-responsibility per helper | PASS — each is exactly one boolean predicate |
| Zero-allocation (static expression-bodied) | PASS — `private static bool ... =>` no heap allocations |
| Lock-free preserved | PASS — read-only dispatcher, no state mutations |
| Illegal states unrepresentable | PASS — `!IsRMATrade` encapsulated; RMA position cannot reach EMA handler |
| ASCII-only | PASS — all identifiers and literals are pure ASCII |
| No scope creep (V12.23) | PASS — private static in same file; parent signature unchanged |
| xUnit tests planned | PASS — [Fact] tests for all 3 helpers |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Epic** | EPIC-W7-049 |
| **Wave** | 7 |
| **Phase** | 4 — Ticket Generation |
| **Bobcoins Used** | 1.5 |
| **Execution Time** | 2026-06-29T01:20:00Z |
| **jcodemunch tools called** | resolve_repo, search_symbols, get_symbol_complexity, get_extraction_candidates |
| **sequential-thinking calls** | 4 (1 probe + 3 ticket-validation thoughts) |
| **ticket_count** | 3 |
| **projected_parent_cyc_after_all** | 4 |
| **Original CYC** | 11 |
