# Phase 2: Architecture Plan — EPIC-W7-138

**Agent:** v12-phase2-architecture
**Wave:** 7 | **Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:10:00Z
**Input:** docs/brain/EPIC-W7-138/01-scope-boundary.md

---

## Method Under Extraction

- **Method:** `ManageTrail_RunPerTradeBranches`
- **Source File:** `src/V12_002.Trailing.cs`
- **Lines:** 240–255
- **Class:** `V12_002` (partial)
- **Signature:** `private bool ManageTrail_RunPerTradeBranches(string entryName, PositionInfo pos)`
- **Original CYC:** 11

### Distinction from EPIC-W7-049

Both EPIC-W7-049 and EPIC-W7-138 target the exact same method definition: `ManageTrail_RunPerTradeBranches` at line 240 of `src/V12_002.Trailing.cs`. There is only one definition in the canonical `src/` directory (confirmed via jcodemunch `search_symbols` — single result returned for `src/V12_002.Trailing.cs`). The two epics are duplicate wave-planning entries for the same method: EPIC-W7-049 was the first planning slot assigned; EPIC-W7-138 is a second independently-tracked entry. Under the V12 100% Completion Mandate, both must produce valid Phase 2 artifacts. This plan independently arrives at the same optimal extraction strategy as EPIC-W7-049, ensuring convergent correctness and providing a second-opinion validation of the approach.

---

### jcodemunch get_context_bundle Result

jcodemunch `get_context_bundle` (symbol_id=`src/V12_002.Trailing.cs::V12_002.ManageTrail_RunPerTradeBranches#method`) returned the full method source:

```csharp
private bool ManageTrail_RunPerTradeBranches(string entryName, PositionInfo pos)
{
    // V8.2: TREND Entry 1 - starts with fixed 2pt stop, switches to EMA9 trail when price crosses EMA
    if (pos.IsTRENDTrade && pos.IsTRENDEntry1 && !pos.IsRMATrade)
        return TrailHandler_TREND_E1(entryName, pos);

    // V8.2: TREND Entry 2 uses EMA15 trailing stop (1.1x ATR from live EMA15)
    if (pos.IsTRENDTrade && pos.IsTRENDEntry2 && !pos.IsRMATrade)
        return TrailHandler_TREND_E2(entryName, pos);

    // V8.4: RETEST trade - Phase 1: Wait for price to cross 9 EMA, Phase 2: Trail at 9 EMA
    if (pos.IsRetestTrade && !pos.IsRMATrade)
        return TrailHandler_RETEST(entryName, pos);

    return false;
}
```

Key findings: 16-line thin dispatcher with three sequential `if` blocks containing compound Boolean guards. The `!pos.IsRMATrade` exclusion is duplicated on every branch, driving CYC to 11 via repeated McCabe branch edges. No state mutations in the dispatcher body.

---

### jcodemunch get_call_hierarchy Result

jcodemunch `get_call_hierarchy` (depth=2, direction=both) confirmed:

- **Callers (depth 1):** `ManageTrailingStops` (src/V12_002.Trailing.cs, line 39) — sole caller; method used as a `continue`-gate inside the position-iteration loop
- **Callees (depth 1):** `TrailHandler_TREND_E1` (line 257), `TrailHandler_TREND_E2` (line 312), `TrailHandler_RETEST` (line 342) — all within same file
- **Callees (depth 2):** `LogBuffer.Format` (src/V12_002.Perf.LogBuffer.cs), `UpdateStopOrder` (src/V12_002.Trailing.StopUpdate.cs) — called by the TrailHandler methods; not in scope

jcodemunch `get_dependency_graph` (file=src/V12_002.Trailing.cs, direction=both, depth=1): returned 1 node, 0 edges — the file has no external import dependencies tracked at the file-graph level; all coupling is internal to the partial-class file set.

---

### jcodemunch get_extraction_candidates Result

jcodemunch `get_extraction_candidates` (file=src/V12_002.Trailing.cs, min_complexity=3, min_callers=1) returned: **no candidates**. This confirms the dispatcher body itself is the complexity source (predicate complexity, not embedded logic), validating the guard-hoist + predicate-extraction approach rather than a logic-extraction approach.

---

## Sequential Thinking Summary

sequentialthinking chain (5 thoughts) produced the following final verdict:

**Thought 5 — Final architecture decision (sequentialthinking complete):**

EPIC-W7-138 and EPIC-W7-049 share the same method definition. The distinction is administrative (duplicate wave-planning entry). Both must independently produce valid Phase 2 artifacts per the V12 100% Completion Mandate.

Extraction plan: 1 new private helper (`IsEMATradeCandidate`) + structural refactor of parent.
- `IsEMATradeCandidate(PositionInfo pos)` encapsulates the `!pos.IsRMATrade` exclusion as a named concept.
- Parent: guard-hoist early-return using the new helper + convert sequential `if` to `if / else if / else if` to encode mutual exclusivity.
- Parent CYC after: 1 + 1 (guard) + 2 (IsTRENDTrade && IsTRENDEntry1) + 2 (IsTRENDTrade && IsTRENDEntry2) + 1 (IsRetestTrade) = **7** ≤ 8 ✓
- Helper CYC: **1** ≤ 8 ✓
- All applicable Jane Street rules satisfied.

---

## Extraction Plan

| Helper Method Name | Responsibility | Projected CYC |
|---|---|---|
| `IsEMATradeCandidate` | Returns `!pos.IsRMATrade` — encapsulates the RMA-exclusion gate that determines whether a position is eligible for EMA-based trailing stop dispatch. Single boolean predicate, zero branches. | 1 |

### Helper Signature

```csharp
private bool IsEMATradeCandidate(PositionInfo pos)
{
    return !pos.IsRMATrade;
}
```

---

## Parent Method After Extraction

**Remaining logic:** Early-exit guard via `IsEMATradeCandidate`, then `if / else if / else if` dispatch chain routing to `TrailHandler_TREND_E1`, `TrailHandler_TREND_E2`, or `TrailHandler_RETEST`.

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

**Projected CYC:** 7 (1 base + 1 guard + 2 TREND-E1 + 2 TREND-E2 + 1 RETEST)

---

## max_cyc_projected: 7
## extraction_count: 1

---

## Jane Street Alignment

| Rule | Status |
|---|---|
| CYC <= 8 achieved | YES — parent: 7, helper: 1 |
| Single-responsibility per helper | YES — `IsEMATradeCandidate` encodes exactly one concept: RMA exclusion |
| Lock-free/Actor pattern preserved | YES — no state mutations in dispatcher body; pure read of `PositionInfo` fields |
| Illegal states unrepresentable | YES — RMA exclusion is now a named predicate; future branches must invoke `IsEMATradeCandidate` making omissions visible |
| Zero-allocation hot path | YES — helper returns `bool` (value type); no heap allocations introduced |
| Guard clause extraction | YES — `if (!IsEMATradeCandidate(pos)) return false;` at method top |
| Named helper methods | YES — 1 private named helper extracted |
| Replace sequential if with else-if | YES — mutual exclusivity encoded via `else if` chain |
| FSM decomposition | N/A — thin dispatcher, not a state machine |
| Extract loop body | N/A — no loops in this method |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 2.5 |
| **Execution Time** | 2026-06-29T01:10:00Z |
| **Wave** | 7 |
| **Phase** | 2 |
| **Epic** | EPIC-W7-138 |
| **Method** | ManageTrail_RunPerTradeBranches |
| **jcodemunch tools called** | get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **Output** | docs/brain/EPIC-W7-138/02-architecture-plan.md |
