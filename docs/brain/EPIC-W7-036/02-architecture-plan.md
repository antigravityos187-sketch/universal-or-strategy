# Phase 2: Architecture Plan — EPIC-W7-036

## Method Under Extraction

- **Method:** `MoveStop_SinglePosition`
- **Source File:** [`src/V12_002.Trailing.Breakeven.cs`](src/V12_002.Trailing.Breakeven.cs:73)
- **Original CYC:** 34
- **Target CYC:** ≤ 8 (Jane Street strict standard)

### jcodemunch get_context_bundle result

Symbol resolved via `src/V12_002.Trailing.Breakeven.cs::V12_002.MoveStop_SinglePosition#method`.
Full 91-line body retrieved. Method signature:

```csharp
private void MoveStop_SinglePosition(
    string entryName,
    PositionInfo pos,
    double offsetPoints,
    double lastKnownPrice)
```

Key findings from body analysis:
- **Segment 1 (lines 74–80):** Price computation — direction-aware `EntryPrice ± offsetPoints` + `RoundToTickSize`. Extractable as pure helper.
- **Segment 2 (lines 92–111):** `if (pos.IsFollower)` early-return fast path — contains duplicated `isBetterF` direction test, `UpdateStopOrder`, flag-set, `MarkStickyDirty`, `Print`. Entire block extractable as `ApplyFollowerBreakeven`.
- **Segment 3 (lines 114–128):** ARM GUARD — stale-price guard (`lastKnownPrice <= 0`) + `priceCleared` direction ternary + conditional arm-and-return.
- **Segment 4 (lines 130–145):** `isBetter` guard — same Long/Short ternary as `isBetterF` (duplicated). Extractable as shared predicate `IsBetterStop`.
- **Segment 5 (lines 147–155):** Master commit path — `UpdateStopOrder`, flag-set, `MarkStickyDirty`, `Print`.
- **Comment evidence:** Build 1108.002-HF1 comment confirms follower bypass is intentional architectural requirement.

### jcodemunch get_call_hierarchy result

| Direction | Symbol | File | Depth | Resolution |
|-----------|--------|------|-------|------------|
| **Caller** | `MoveStopsToBreakevenWithOffset` | `src/V12_002.Trailing.Breakeven.cs:41` | 1 | ast_resolved |
| **Callee** | `UpdateStopOrder` | `src/V12_002.Trailing.StopUpdate.cs:84` | 1 | ast_inferred |
| **Callee** | `MarkStickyDirty` | `src/V12_002.StickyState.cs:619` | 1 | ast_inferred |
| **Callee** | `LogBuffer.Format` | `src/V12_002.Perf.LogBuffer.cs:28` | 1 | ast_inferred |
| **Depth-2 callees** | `ValidateStopPrice`, `HandleStalePendingReplacement`, `UpdateExistingPendingReplacement`, `InitiateStopReplacement`, `CreateDirectStopOrder`, `HandleUpdateException` | `src/V12_002.Orders.Management.StopSync.cs` (via `UpdateStopOrder`) | 2 | ast_resolved |

**Confirmed:** 1 direct caller only. Signature must not change.

### jcodemunch get_dependency_graph result

- **Node count:** 1 (file is self-contained)
- **Edge count:** 0 (no explicit file-level imports/importers detected)
- `src/V12_002.Trailing.Breakeven.cs` is a standalone partial-class file — members share the `V12_002` partial class but the dependency graph shows no cross-file import edges (NinjaTrader partial-class pattern). All extracted helpers must remain in this same file.

### jcodemunch get_extraction_candidates result

- **Candidates returned:** 0 (no multi-caller candidates at `min_callers=1`)
- This confirms no existing shared helpers to leverage — all 3 extractions are net-new private methods.

---

## Sequential Thinking Summary

**Thought 1:** Established that CYC 34 arises from 3 structural drivers: Master/Follower fork (~12 CYC), ARM GUARD threshold gate (~5 CYC), and duplicated `isBetter`/`isBetterF` guards (~6 CYC). Confirmed from full source body.

**Thought 2:** Identified 3 minimal helpers sufficient to reach CYC ≤ 8 in parent. Verified the IsBetterStop helper is shared by both follower and master paths, eliminating the duplicate guard pattern.

**Thought 3:** Projected CYC for each helper: `ComputeBreakevenStopPrice` = 2, `IsBetterStop` = 2, `ApplyFollowerBreakeven` = 2. Parent residual = 6. All ≤ 8.

**Thought 4:** Designed final method signatures and guard-clause ordering. Confirmed zero-allocation, no lock() introduced, partial-class same-file placement.

**Thought 5 (Final):** Jane Street verdict — all 10 applicable rules satisfied. Extraction plan approved. max_cyc_projected = 6. extraction_count = 3.

---

## Extraction Plan

| Helper Method Name | Signature | Responsibility | Projected CYC |
|---|---|---|---|
| `ComputeBreakevenStopPrice` | `private double ComputeBreakevenStopPrice(PositionInfo pos, double offsetPoints)` | Computes directional stop price (`EntryPrice ± offsetPoints`) and rounds to tick size. Pure arithmetic, no state mutation. | 2 |
| `IsBetterStop` | `private bool IsBetterStop(PositionInfo pos, double newStopPrice)` | Single Long/Short "profit-protecting direction" test: Long → `newStopPrice > CurrentStopPrice`; Short → `newStopPrice < CurrentStopPrice`. Shared predicate used by both follower and master paths. Eliminates `isBetter`/`isBetterF` duplication. | 2 |
| `ApplyFollowerBreakeven` | `private void ApplyFollowerBreakeven(string entryName, PositionInfo pos, double newStopPrice, double offsetPoints)` | Complete follower early-return execution path: calls `IsBetterStop`, if true → `UpdateStopOrder`, sets `ManualBreakevenTriggered = true`, `MarkStickyDirty()`, `Print`. Fully encapsulates the Build 1108.002-HF1 follower bypass. | 2 |

### Extraction Details

**Helper 1 — `ComputeBreakevenStopPrice`**

```csharp
private double ComputeBreakevenStopPrice(PositionInfo pos, double offsetPoints)
{
    double price = pos.Direction == MarketPosition.Long
        ? pos.EntryPrice + offsetPoints
        : pos.EntryPrice - offsetPoints;
    return Instrument.MasterInstrument.RoundToTickSize(price);
}
```
- CYC: 2 (1 direction branch)
- Zero allocation: stack-only double arithmetic
- No state mutation

**Helper 2 — `IsBetterStop`**

```csharp
private bool IsBetterStop(PositionInfo pos, double newStopPrice)
{
    return (pos.Direction == MarketPosition.Long && newStopPrice > pos.CurrentStopPrice)
        || (pos.Direction == MarketPosition.Short && newStopPrice < pos.CurrentStopPrice);
}
```
- CYC: 2 (1 compound direction guard)
- Replaces both `isBetter` and `isBetterF` duplicates
- Pure predicate, no state mutation

**Helper 3 — `ApplyFollowerBreakeven`**

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
- CYC: 2 (1 guard on `IsBetterStop`)
- Delegates direction check to `IsBetterStop` — no duplicated ternary
- Fully isolated follower execution path

---

## Parent Method After Extraction

**Slim orchestrator — `MoveStop_SinglePosition` (after extraction):**

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

- **Remaining logic:** Pure guard-clause orchestrator — 4 sequential early-return guards, then commit path
- **Guard clause order:** IsFollower fast-path → stale price abort → ARM GUARD threshold → IsBetterStop check → commit
- **Projected CYC:** 6 (baseline 1 + 5 decision points: IsFollower, stale-price, priceCleared ternary, !priceCleared, !IsBetterStop)
- **Max nesting depth:** 1
- **No lock() blocks**

---

## max_cyc_projected: 6
## extraction_count: 3

---

## Jane Street Alignment

| Rule | Status | Notes |
|---|---|---|
| CYC <= 8 achieved | **YES** | Parent: 6, helpers: 2, 2, 2. Max: 6. |
| Single-responsibility per helper | **YES** | `ComputeBreakevenStopPrice` = price arithmetic only; `IsBetterStop` = direction predicate only; `ApplyFollowerBreakeven` = follower execution path only |
| Lock-free / Actor pattern preserved | **YES** | `MarkStickyDirty()` retains `Interlocked.Exchange` internally. No `lock()` introduced. No new state mutations outside of existing pattern. |
| Illegal states unrepresentable | **YES** | Follower path is physically isolated — cannot fall through to ARM GUARD master logic. `priceCleared` and `IsBetterStop` are named predicates with unambiguous semantics, eliminating silent wrong-direction execution. |
| Zero-allocation hot paths | **YES** | All helpers operate on `double` and `bool` stack locals. `RoundToTickSize` is a NinjaTrader API that does not heap-allocate. `String.Format` calls pre-existed. |
| Extract Guard Clauses | **YES** | Parent reduced to 4 top-level sequential guards, max nesting depth 1. |
| Extract to Named Helper Methods | **YES** | 3 private helpers, each CYC ≤ 8, each named for intent. |
| Lookup table replacement | **N/A** | Binary Long/Short direction — lookup table would increase complexity. |
| FSM decomposition | **N/A** | Method is not a state machine. ARM/TRIGGERED flag assignments are correct as direct assignments. |
| Extract Loop Body | **N/A** | No loops in `MoveStop_SinglePosition`. |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Epic** | EPIC-W7-036 |
| **Wave** | 7 |
| **Phase** | 2 — Architecture Planning |
| **Method** | `MoveStop_SinglePosition` |
| **Source** | `src/V12_002.Trailing.Breakeven.cs` |
| **CYC (before)** | 34 |
| **CYC (after, max)** | 6 |
| **extraction_count** | 3 |
| **max_cyc_projected** | 6 |
| **Bobcoins Used** | 4 |
| **Execution Time** | 2026-06-29T01:15:00Z |
| **jcodemunch tools called** | `resolve_repo`, `get_context_bundle`, `get_call_hierarchy`, `get_dependency_graph`, `get_extraction_candidates` |
| **sequential-thinking calls** | 5 |
| **Output** | `docs/brain/EPIC-W7-036/02-architecture-plan.md` |
