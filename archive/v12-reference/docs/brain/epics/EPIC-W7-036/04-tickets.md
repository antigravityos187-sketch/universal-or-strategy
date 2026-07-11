# Phase 4: Implementation Tickets — EPIC-W7-036

**Epic:** EPIC-W7-036 | **Method:** `MoveStop_SinglePosition` | **Source:** `src/V12_002.Trailing.Breakeven.cs` | **Original CYC:** 34 (indexed: 21) | **Wave:** 7

---

## ticket_count: 3

---

## Ticket 1

| Field | Value |
|---|---|
| **ticket_id** | 1 |
| **helper_name** | `ComputeBreakevenStopPrice` |
| **concern** | Pure arithmetic price computation — direction-aware `EntryPrice ± offsetPoints` rounded to tick size. No state mutation, no I/O. |
| **signature** | `private double ComputeBreakevenStopPrice(PositionInfo pos, double offsetPoints)` |
| **lines_to_move** | Price computation block at top of `MoveStop_SinglePosition`: direction ternary `pos.Direction == MarketPosition.Long ? pos.EntryPrice + offsetPoints : pos.EntryPrice - offsetPoints` wrapped in `Instrument.MasterInstrument.RoundToTickSize(...)`. |
| **call_site_in_parent** | Replace inline computation at method entry: `double newStopPrice = ComputeBreakevenStopPrice(pos, offsetPoints);` |
| **cyc_reduction** | −1 (removes 1 direction branch from parent) |
| **projected_helper_cyc** | 2 |
| **placement** | `src/V12_002.Trailing.Breakeven.cs` (same partial-class file) |

**Helper body (from Phase 2 plan):**
```csharp
private double ComputeBreakevenStopPrice(PositionInfo pos, double offsetPoints)
{
    double price = pos.Direction == MarketPosition.Long
        ? pos.EntryPrice + offsetPoints
        : pos.EntryPrice - offsetPoints;
    return Instrument.MasterInstrument.RoundToTickSize(price);
}
```

---

## Ticket 2

| Field | Value |
|---|---|
| **ticket_id** | 2 |
| **helper_name** | `IsBetterStop` |
| **concern** | Directional stop-improvement predicate. Shared pure predicate replacing both `isBetter` and `isBetterF` duplicated direction tests. No side effects. |
| **signature** | `private bool IsBetterStop(PositionInfo pos, double newStopPrice)` |
| **lines_to_move** | Duplicated direction guards: `isBetterF` ternary in the follower block AND `isBetter` ternary in the master commit path — both reduce to the same Long/Short comparison. Extract once, reference twice. |
| **call_site_in_parent** | Called from `ApplyFollowerBreakeven` (Ticket 3) + direct guard in master path: `if (!IsBetterStop(pos, newStopPrice)) { ... return; }` |
| **cyc_reduction** | −2 (removes both `isBetter` and `isBetterF` groups from parent) |
| **projected_helper_cyc** | 2 |
| **placement** | `src/V12_002.Trailing.Breakeven.cs` (same partial-class file) |

**Helper body (from Phase 2 plan):**
```csharp
private bool IsBetterStop(PositionInfo pos, double newStopPrice)
{
    return (pos.Direction == MarketPosition.Long && newStopPrice > pos.CurrentStopPrice)
        || (pos.Direction == MarketPosition.Short && newStopPrice < pos.CurrentStopPrice);
}
```

---

## Ticket 3

| Field | Value |
|---|---|
| **ticket_id** | 3 |
| **helper_name** | `ApplyFollowerBreakeven` |
| **concern** | Complete follower early-return execution path (Build 1108.002-HF1 follower bypass). Encapsulates: `IsBetterStop` guard → `UpdateStopOrder` → `ManualBreakevenTriggered = true` → `MarkStickyDirty()` → `Print`. Physically isolates follower path from master ARM GUARD logic. |
| **signature** | `private void ApplyFollowerBreakeven(string entryName, PositionInfo pos, double newStopPrice, double offsetPoints)` |
| **lines_to_move** | Entire `if (pos.IsFollower)` block from original source (lines 92–111): includes `isBetterF` check, `UpdateStopOrder`, flag assignment, `MarkStickyDirty()`, follower-variant `Print` format string. |
| **call_site_in_parent** | Replace follower block with: `if (pos.IsFollower) { ApplyFollowerBreakeven(entryName, pos, newStopPrice, offsetPoints); return; }` |
| **cyc_reduction** | −15+ (collapses entire nested follower block; dominant CYC driver) |
| **projected_helper_cyc** | 2 |
| **placement** | `src/V12_002.Trailing.Breakeven.cs` (same partial-class file) |

**Helper body (from Phase 2 plan):**
```csharp
private void ApplyFollowerBreakeven(string entryName, PositionInfo pos, double newStopPrice, double offsetPoints)
{
    if (!IsBetterStop(pos, newStopPrice))
        return;

    UpdateStopOrder(entryName, pos, newStopPrice, 1);
    pos.ManualBreakevenTriggered = true;
    MarkStickyDirty();
    Print(string.Format("BE+{0} MOVED (follower): {1} Stop -> {2:F2}", offsetPoints, entryName, newStopPrice));
}
```

---

## Parent Method After All Extractions

`MoveStop_SinglePosition` becomes a slim guard-clause orchestrator:

```csharp
private void MoveStop_SinglePosition(
    string entryName,
    PositionInfo pos,
    double offsetPoints,
    double lastKnownPrice)
{
    double newStopPrice = ComputeBreakevenStopPrice(pos, offsetPoints);

    if (pos.IsFollower)
    {
        ApplyFollowerBreakeven(entryName, pos, newStopPrice, offsetPoints);
        return;
    }

    if (lastKnownPrice <= 0)
    {
        Print(string.Format("[BE_ABORT] {0}: Price data stale (0). Waiting for next tick.", entryName));
        return;
    }

    bool priceCleared = pos.Direction == MarketPosition.Long
        ? lastKnownPrice >= newStopPrice
        : lastKnownPrice <= newStopPrice;

    if (!priceCleared)
    {
        pos.ManualBreakevenArmed = true;
        pos.ManualBreakevenTriggered = false;
        Print(string.Format("[V12] BE Armed: {0} Price has not reached threshold. Shielding entry once cleared.", entryName));
        return;
    }

    if (!IsBetterStop(pos, newStopPrice))
    {
        Print(string.Format("BE+{0}: Stop already better for {1}. Current={2:F2}, Request={3:F2}",
            offsetPoints, entryName, pos.CurrentStopPrice, newStopPrice));
        return;
    }

    UpdateStopOrder(entryName, pos, newStopPrice, 1);
    pos.ManualBreakevenTriggered = true;
    MarkStickyDirty();
    Print(string.Format("BE+{0} MOVED: {1} Stop -> {2:F2}", offsetPoints, entryName, newStopPrice));
}
```

**CYC breakdown:** baseline 1 + IsFollower (+1) + stale-price guard (+1) + priceCleared ternary (+1) + !priceCleared guard (+1) + !IsBetterStop guard (+1) = **6**

---

## projected_parent_cyc_after_all: 6

## CYC Summary

| Symbol | Projected CYC | ≤ 8? |
|---|---|---|
| `ComputeBreakevenStopPrice` (Ticket 1) | 2 | ✅ PASS |
| `IsBetterStop` (Ticket 2) | 2 | ✅ PASS |
| `ApplyFollowerBreakeven` (Ticket 3) | 2 | ✅ PASS |
| `MoveStop_SinglePosition` (parent, after) | 6 | ✅ PASS |
| **Max across all symbols** | **6** | ✅ **Jane Street CYC ≤ 8 satisfied** |

---

## Execution Order

Tickets MUST be executed in this order (dependency chain):

1. **Ticket 2 first** — `IsBetterStop` must exist before Ticket 3 can reference it.
2. **Ticket 1 second** — `ComputeBreakevenStopPrice` (no dependencies on other helpers).
3. **Ticket 3 last** — `ApplyFollowerBreakeven` depends on `IsBetterStop` from Ticket 2.

---

## Agent Tracking

| Field | Value |
|---|---|
| Agent Name | v12-phase4-tickets |
| Epic | EPIC-W7-036 |
| Wave | 7 |
| Phase | 4 — Ticket Generation |
| Bobcoins Used | 8 |
| Execution Time | 2026-06-29T01:25:00Z |
| jcodemunch tools called | `resolve_repo`, `search_symbols`, `get_symbol_complexity`, `get_extraction_candidates` |
| sequential-thinking calls | 4 (1 probe + 3 validation thoughts) |
| ticket_count | 3 |
| projected_parent_cyc_after_all | 6 |
| Original CYC | 34 (indexed: 21) |
| Input: 02-architecture-plan.md | docs/brain/EPIC-W7-036/02-architecture-plan.md |
| Input: 03-audit-report.md | docs/brain/EPIC-W7-036/03-audit-report.md |
| Output | docs/brain/EPIC-W7-036/04-tickets.md |
