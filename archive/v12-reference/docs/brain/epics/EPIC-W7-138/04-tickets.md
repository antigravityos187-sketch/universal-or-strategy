# Phase 4: Tickets — EPIC-W7-138

**Agent:** v12-phase4-tickets
**Wave:** 7 | **Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T01:16:00Z
**Input:** docs/brain/EPIC-W7-138/02-architecture-plan.md + docs/brain/EPIC-W7-138/03-audit-report.md

---

## Method Under Extraction

| Field | Value |
|---|---|
| **Method** | `ManageTrail_RunPerTradeBranches` |
| **Source File** | `src/V12_002.Trailing.cs` |
| **Lines** | 240–255 |
| **Class** | `V12_002` (partial) |
| **Original CYC** | 11 (confirmed by jcodemunch `get_symbol_complexity`) |
| **Original Lines** | 16 |
| **Original Max Nesting** | 1 |
| **dna_verdict** | PASS (Phase 3 audit) |

---

## MCP Evidence Summary

### jcodemunch get_symbol_complexity
- **symbol_id:** `src/V12_002.Trailing.cs::V12_002.ManageTrail_RunPerTradeBranches#method`
- **cyclomatic:** 11 | **assessment:** high | **max_nesting:** 1 | **param_count:** 2 | **lines:** 16

### jcodemunch get_extraction_candidates
- **file:** `src/V12_002.Trailing.cs` | **min_complexity:** 3 | **min_callers:** 1
- **Result:** `candidates=[]` — No embedded logic blobs; complexity source is predicate duplication. Validates guard-hoist + predicate-extraction approach.

---

## Sequential Thinking Summary

| Thought | Conclusion |
|---|---|
| 1 — How many tickets? | 1 ticket. One concern = `!pos.IsRMATrade` guard duplication. One extracted helper. |
| 2 — Lines, name, CYC delta | `IsEMATradeCandidate` extracts the RMA-exclusion predicate. Parent refactored to guard-hoist + `else if` chain. CYC reduction: 4 (11→7). Helper CYC: 1. |
| 3 — Verify all CYC ≤ 8 | Parent post-extraction CYC=7 ≤ 8 ✓. Helper CYC=1 ≤ 8 ✓. All checks pass. |

---

## ticket_count: 1

---

## Ticket 1

| Field | Value |
|---|---|
| **ticket_id** | 1 |
| **helper_name** | `IsEMATradeCandidate` |
| **concern** | Encapsulate the `!pos.IsRMATrade` exclusion gate as a named boolean predicate. This single responsibility determines whether a position is eligible for EMA-based trailing-stop dispatch, making the RMA-exclusion visible by name and eliminating the duplication across all three dispatch branches. |
| **lines_to_move** | The `!pos.IsRMATrade` sub-condition currently duplicated inside each of the three `if`-block compound Boolean predicates (lines 241, 244, 247). Extract to a new private helper `IsEMATradeCandidate(PositionInfo pos)` that returns `!pos.IsRMATrade`. Simultaneously: (a) add a guard-hoist `if (!IsEMATradeCandidate(pos)) return false;` at the top of the parent, (b) remove the `!pos.IsRMATrade` sub-condition from each of the three remaining guards, (c) convert the three sequential `if` blocks to an `if / else if / else if` chain to encode mutual exclusivity. |
| **cyc_reduction** | 4 (CYC drops from 11 to 7 in parent) |
| **projected_helper_cyc** | 1 |

### Helper Implementation

```csharp
private bool IsEMATradeCandidate(PositionInfo pos)
{
    return !pos.IsRMATrade;
}
```

### Parent After Extraction

```csharp
private bool ManageTrail_RunPerTradeBranches(string entryName, PositionInfo pos)
{
    if (!IsEMATradeCandidate(pos))
        return false;

    if (pos.IsTRENDTrade && pos.IsTRENDEntry1)
        return TrailHandler_TREND_E1(entryName, pos);
    else if (pos.IsTRENDTrade && pos.IsTRENDEntry2)
        return TrailHandler_TREND_E2(entryName, pos);
    else if (pos.IsRetestTrade)
        return TrailHandler_RETEST(entryName, pos);

    return false;
}
```

### Jane Street Alignment

| Rule | Status |
|---|---|
| CYC ≤ 8 — parent | ✅ YES — CYC=7 |
| CYC ≤ 8 — helper | ✅ YES — CYC=1 |
| Single-responsibility per helper | ✅ YES — RMA exclusion gate only |
| Lock-free / no state mutations | ✅ YES — pure read-only dispatcher |
| Illegal states unrepresentable | ✅ YES — guard hoist makes RMA exclusion non-bypassable |
| Zero-allocation hot path | ✅ YES — `bool` return, no heap allocation |
| Guard clause extraction | ✅ YES — upfront `if (!IsEMATradeCandidate) return false` |
| Replace sequential `if` with `else if` | ✅ YES — mutual exclusivity encoded |
| ASCII-only identifiers and literals | ✅ YES — all identifiers are ASCII |

---

## projected_parent_cyc_after_all: 7

---

## CYC Summary

| Symbol | Pre-Extraction CYC | Post-Extraction CYC | CYC ≤ 8? |
|---|---|---|---|
| `ManageTrail_RunPerTradeBranches` | 11 | 7 | ✅ YES |
| `IsEMATradeCandidate` (new) | — | 1 | ✅ YES |
| **max_cyc_projected** | — | **7** | ✅ YES |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Bobcoins Used** | 1.5 |
| **Execution Time** | 2026-06-29T01:16:00Z |
| **Wave** | 7 |
| **Phase** | 4 |
| **Epic** | EPIC-W7-138 |
| **Method** | `ManageTrail_RunPerTradeBranches` |
| **jcodemunch tools called** | `resolve_repo`, `get_symbol_complexity`, `get_extraction_candidates` |
| **sequential-thinking calls** | 4 (1 probe + 3 analysis) |
| **Output** | `docs/brain/EPIC-W7-138/04-tickets.md` |
