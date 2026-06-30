# Phase 4: Ticket Generation — EPIC-W7-039

**Agent:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T01:50:00Z
**Input:** docs/brain/EPIC-W7-039/02-architecture-plan.md | docs/brain/EPIC-W7-039/03-audit-report.md

---

## Summary

| Field | Value |
|---|---|
| **Epic** | EPIC-W7-039 |
| **Method** | `ManageTrailingStops` |
| **Source File** | [`src/V12_002.Trailing.cs`](src/V12_002.Trailing.cs:39) |
| **Original CYC** | 15 (jcodemunch confirmed) / 13 (epic spec) |
| **ticket_count** | 3 |
| **projected_parent_cyc_after_all** | 5 |
| **max_cyc_projected** | 5 |
| **dna_verdict** | PASS |

---

## Ticket Definitions

---

### TICKET T039-01 — Extract `ShouldSkipPosition`

| Field | Value |
|---|---|
| **ticket_id** | T039-01 |
| **helper_name** | `ShouldSkipPosition` |
| **concern** | Guard clause aggregator: returns `true` if this position must be skipped this tick (stale key, not-filled/not-bracketed, or follower with anchor pending) |
| **lines_to_move** | ~8 lines — the 3-branch guard block at the top of the `foreach` loop body in `ManageTrailingStops` |
| **cyc_reduction** | 3 (removes 3 branch conditions from parent: `ContainsKey` check, `EntryFilled/BracketSubmitted` check, `IsFollower && SymmetryGuardIsAnchorPending` check) |
| **projected_helper_cyc** | 5 |

#### Signature

```csharp
private bool ShouldSkipPosition(string entryName, PositionInfo pos)
```

#### Body to Extract

```csharp
private bool ShouldSkipPosition(string entryName, PositionInfo pos)
{
    if (!activePositions.ContainsKey(entryName))
        return true;
    if (!pos.EntryFilled || !pos.BracketSubmitted)
        return true;
    if (pos.IsFollower && SymmetryGuardIsAnchorPending(entryName))
        return true;
    return false;
}
```

#### Call-Site Replacement in `ManageTrailingStops`

```csharp
// Before:
// [3 inline guard branches] → continue;

// After:
if (ShouldSkipPosition(kvp.Key, kvp.Value))
    continue;
```

#### Acceptance Criteria

- [ ] New private method `ShouldSkipPosition` exists in `src/V12_002.Trailing.cs`
- [ ] All 3 guard conditions removed from `ManageTrailingStops` foreach loop body
- [ ] Single `if (ShouldSkipPosition(kvp.Key, kvp.Value)) continue;` present in loop
- [ ] Build passes with zero errors
- [ ] CYC of `ShouldSkipPosition` <= 8 (target: 5)

---

### TICKET T039-02 — Extract `UpdatePositionMetrics`

| Field | Value |
|---|---|
| **ticket_id** | T039-02 |
| **helper_name** | `UpdatePositionMetrics` |
| **concern** | Pure metrics update: increments `TicksSinceEntry` and updates `ExtremePriceSinceEntry` based on position direction via ternary |
| **lines_to_move** | ~5 lines — the `pos.TicksSinceEntry++` increment and `pos.ExtremePriceSinceEntry` ternary assignment from the foreach loop body |
| **cyc_reduction** | 1 (removes ternary condition from parent) |
| **projected_helper_cyc** | 2 |

#### Signature

```csharp
private void UpdatePositionMetrics(PositionInfo pos)
```

#### Body to Extract

```csharp
private void UpdatePositionMetrics(PositionInfo pos)
{
    pos.TicksSinceEntry++;
    pos.ExtremePriceSinceEntry =
        pos.Direction == MarketPosition.Long
            ? Math.Max(pos.ExtremePriceSinceEntry, Close[0])
            : Math.Min(pos.ExtremePriceSinceEntry, Close[0]);
}
```

#### Call-Site Replacement in `ManageTrailingStops`

```csharp
// Before:
// pos.TicksSinceEntry++;
// pos.ExtremePriceSinceEntry = pos.Direction == ... ? ... : ...;

// After:
UpdatePositionMetrics(kvp.Value);
```

#### Acceptance Criteria

- [ ] New private method `UpdatePositionMetrics` exists in `src/V12_002.Trailing.cs`
- [ ] `TicksSinceEntry++` and `ExtremePriceSinceEntry` ternary removed from `ManageTrailingStops` loop body
- [ ] Single `UpdatePositionMetrics(kvp.Value);` call present in loop
- [ ] Build passes with zero errors
- [ ] CYC of `UpdatePositionMetrics` <= 8 (target: 2)

---

### TICKET T039-03 — Extract `ExecutePositionTrail`

| Field | Value |
|---|---|
| **ticket_id** | T039-03 |
| **helper_name** | `ExecutePositionTrail` |
| **concern** | Trail dispatch: invokes EMA-branch via `ManageTrail_RunPerTradeBranches`; computes `allowPointBasedTrailing` flag; conditionally invokes point-based trailing via `ManageTrail_RunPointBasedTrailing` |
| **lines_to_move** | ~8 lines — the `ManageTrail_RunPerTradeBranches` call block, the `isTrendOrRetestTrade`/`allowPointBasedTrailing` computation, and the `ManageTrail_RunPointBasedTrailing` call block |
| **cyc_reduction** | 4 (removes dispatch block: if-return on PerTrade result, OR condition for isTrendOrRetestTrade, if-return on allowPointBasedTrailing) |
| **projected_helper_cyc** | 5 |

#### Signature

```csharp
private void ExecutePositionTrail(string entryName, PositionInfo pos)
```

#### Body to Extract

```csharp
private void ExecutePositionTrail(string entryName, PositionInfo pos)
{
    if (ManageTrail_RunPerTradeBranches(entryName, pos))
        return;

    bool isTrendOrRetestTrade = pos.IsTRENDTrade || pos.IsRetestTrade;
    bool allowPointBasedTrailing = !isTrendOrRetestTrade || pos.IsRMATrade;
    if (!allowPointBasedTrailing)
        return;

    double newStopPrice = pos.CurrentStopPrice;
    int newTrailLevel = pos.CurrentTrailLevel;
    ManageTrail_RunPointBasedTrailing(entryName, pos, ref newStopPrice, ref newTrailLevel);
}
```

#### Call-Site Replacement in `ManageTrailingStops`

```csharp
// Before:
// [ManageTrail_RunPerTradeBranches block + allowPointBasedTrailing computation + ManageTrail_RunPointBasedTrailing block]

// After:
ExecutePositionTrail(kvp.Key, kvp.Value);
```

#### Acceptance Criteria

- [ ] New private method `ExecutePositionTrail` exists in `src/V12_002.Trailing.cs`
- [ ] Trail dispatch block removed from `ManageTrailingStops` foreach loop body
- [ ] Single `ExecutePositionTrail(kvp.Key, kvp.Value);` call present in loop
- [ ] Build passes with zero errors
- [ ] CYC of `ExecutePositionTrail` <= 8 (target: 5)

---

## Residual `ManageTrailingStops` After All Extractions

```csharp
private void ManageTrailingStops()
{
    bool _shouldExit;
    ManageTrail_AdaptiveThrottleTick(out _shouldExit);
    if (_shouldExit)
        return;

    var positionSnapshot = activePositions.ToArray();
    foreach (var kvp in positionSnapshot)
    {
        if (ShouldSkipPosition(kvp.Key, kvp.Value))
            continue;
        UpdatePositionMetrics(kvp.Value);
        ExecutePositionTrail(kvp.Key, kvp.Value);
    }

    if (EnableSIMA)
    {
        var updatedSnapshot = activePositions.ToArray();
        ManageTrail_RunFleetSymmetrySync(updatedSnapshot);
    }

    ShadowEngineCheck();
}
```

**Projected CYC:** 5 (base + shouldExit-check + foreach + ShouldSkipPosition-continue + EnableSIMA-check)

---

## CYC Summary Table

| Unit | Original CYC | Projected CYC | <= 8? |
|---|---|---|---|
| `ManageTrailingStops` | 15 | 5 | YES |
| `ShouldSkipPosition` | N/A (new) | 5 | YES |
| `UpdatePositionMetrics` | N/A (new) | 2 | YES |
| `ExecutePositionTrail` | N/A (new) | 5 | YES |
| **max_cyc_projected** | — | **5** | **YES** |

---

## Execution Order

Tickets are independent (no cross-ticket dependencies). Recommended sequential order for safety:

1. **T039-01** — `ShouldSkipPosition` (guards first, most visible behavior)
2. **T039-02** — `UpdatePositionMetrics` (simplest extraction, 2 lines)
3. **T039-03** — `ExecutePositionTrail` (most complex dispatch, last)

All 3 tickets target only `src/V12_002.Trailing.cs`. Zero external file changes required.

---

## Risk Notes

| Risk | Severity | Mitigation |
|---|---|---|
| Dual `activePositions.ToArray()` snapshot ordering | HIGH | `positionSnapshot` (main loop) and `updatedSnapshot` (SIMA sync) remain separate; no merging in extractions |
| `ManageTrail_RunPointBasedTrailing` ref params | MEDIUM | `ExecutePositionTrail` declares local `newStopPrice`/`newTrailLevel` to hold ref params — no behavior change |
| Actor/Enqueue caller in BarUpdate.cs:327 | LOW | Method signature `private void ManageTrailingStops()` unchanged; sole caller unaffected |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Epic** | EPIC-W7-039 |
| **Wave** | 7 |
| **Phase** | 4 |
| **Lane** | P4-L3 |
| **ticket_count** | 3 |
| **projected_parent_cyc_after_all** | 5 |
| **max_cyc_projected** | 5 |
| **Bobcoins Used** | 6 |
| **Execution Time** | 2026-06-29T01:50:00Z |
| **jcodemunch tools called** | `resolve_repo`, `search_symbols`, `get_symbol_complexity`, `get_extraction_candidates` |
| **sequential-thinking calls** | 4 (1 probe + 3 ticket-breakdown thoughts) |
| **Status** | Completed |
