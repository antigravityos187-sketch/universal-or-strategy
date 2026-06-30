# EPIC-W7-136 — Phase 4: Ticket Definitions

**Agent Name: v12-phase4-tickets**
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Epic:** EPIC-W7-136
**Target Method:** `ManageTrailingStops` in [`src/V12_002.Trailing.cs`](../../src/V12_002.Trailing.cs)
**Generated:** 2026-06-29T01:20:00Z
**Lane:** P4-L8

---

## Summary

| Field | Value |
|---|---|
| Method | `ManageTrailingStops` |
| File | `src/V12_002.Trailing.cs` |
| CYC Before Extraction (strict McCabe) | 14 |
| CYC Before Extraction (Lizard-compatible) | 10 |
| CYC After Extraction (orchestrator, conservative) | 8 |
| Extraction Count | 2 |
| Ticket Count | 3 |
| DNA Verdict | PASS |
| max_cyc_projected | 8 |

**MCP Note:** `get_symbol_complexity` returned "Symbol not found" (expected data artifact — partial-class
method not indexed as standalone symbol). `get_extraction_candidates` returned empty (ManageTrailingStops
has a single caller via `Enqueue` pattern; tool requires `min_callers=2`). CYC is authoritative from Phase 2
manual McCabe branch count on source lines 39–97. Both results are consistent with Phase 2 documentation.

---

## Ticket Index

| ID | Type | CYC Target | Priority |
|---|---|---|---|
| [T136-01](#t136-01) | extraction | Helper CYC <= 6 | P0 — must precede T136-02 |
| [T136-02](#t136-02) | extraction | Helper CYC <= 3 | P0 — must precede T136-03 |
| [T136-03](#t136-03) | verification | Orchestrator CYC <= 8 | P0 — final gate |

---

## T136-01

**ID:** T136-01
**Type:** extraction
**Priority:** P0
**File:** [`src/V12_002.Trailing.cs`](../../src/V12_002.Trailing.cs)
**Depends On:** none (first extraction ticket)
**CYC Target:** `ManageTrail_ShouldProcessPosition` <= 6

### Description

Extract the 3-condition guard chain at lines 54–60 of `ManageTrailingStops` into a new private
helper method `ManageTrail_ShouldProcessPosition(string entryName, PositionInfo pos) -> bool`.
This extraction removes 6 branches (strict McCabe) from the orchestrator.

### Method to Create

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private bool ManageTrail_ShouldProcessPosition(string entryName, PositionInfo pos)
{
    if (!activePositions.ContainsKey(entryName))
        return false;
    if (!pos.EntryFilled || !pos.BracketSubmitted)
        return false;
    if (pos.IsFollower && SymmetryGuardIsAnchorPending(entryName))
        return false;
    return true;
}
```

**Placement:** Same partial class in `src/V12_002.Trailing.cs`, below `ManageTrailingStops`.
No new files. No caller changes.

### Orchestrator Change

Replace lines 54–60 in `ManageTrailingStops` with:

```csharp
if (!ManageTrail_ShouldProcessPosition(entryName, pos))
    continue;
```

### Acceptance Criteria

- [ ] `ManageTrail_ShouldProcessPosition` exists in `src/V12_002.Trailing.cs` (same partial class)
- [ ] Method is decorated with `[MethodImpl(MethodImplOptions.AggressiveInlining)]`
- [ ] Method signature: `private bool ManageTrail_ShouldProcessPosition(string entryName, PositionInfo pos)`
- [ ] Guard 1: `!activePositions.ContainsKey(entryName)` → `return false`
- [ ] Guard 2: `!pos.EntryFilled || !pos.BracketSubmitted` → `return false`
- [ ] Guard 3: `pos.IsFollower && SymmetryGuardIsAnchorPending(entryName)` → `return false`
- [ ] Orchestrator calls `!ManageTrail_ShouldProcessPosition(entryName, pos)` with `continue`
- [ ] New helper CYC <= 6 (strict McCabe: 1 base + 1 ContainsKey if + 1 OR logical + 1 EntryFilled/Bracket if + 1 IsFollower && + 1 AnchorPending if = 6)
- [ ] No LINQ introduced
- [ ] No `lock()` blocks introduced
- [ ] Zero heap allocations in new helper (pure boolean logic on existing fields)
- [ ] `dotnet build` passes with zero errors

### CYC Projection After T136-01

| Method | CYC |
|---|---|
| `ManageTrailingStops` (orchestrator, after T136-01 only) | ~12 (3 orchestrator branches replaced, 1 call added) |
| `ManageTrail_ShouldProcessPosition` | 6 |

---

## T136-02

**ID:** T136-02
**Type:** extraction
**Priority:** P0
**File:** [`src/V12_002.Trailing.cs`](../../src/V12_002.Trailing.cs)
**Depends On:** T136-01 (must be applied to post-T136-01 state of the file)
**CYC Target:** `ManageTrail_ShouldAllowPointBasedTrailing` <= 3

### Description

Extract the 2-filter predicate at lines 75–78 of the original `ManageTrailingStops` (after T136-01,
the line numbers will have shifted by approximately -5) into a new private helper method
`ManageTrail_ShouldAllowPointBasedTrailing(PositionInfo pos) -> bool`. This extraction removes
3 branches (strict McCabe) from the orchestrator, bringing it to CYC=8 (strict).

### Method to Create

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private bool ManageTrail_ShouldAllowPointBasedTrailing(PositionInfo pos)
{
    bool isTrendOrRetestTrade = pos.IsTRENDTrade || pos.IsRetestTrade;
    return !isTrendOrRetestTrade || pos.IsRMATrade;
}
```

**Placement:** Same partial class in `src/V12_002.Trailing.cs`, below `ManageTrail_ShouldProcessPosition`.
No new files. No caller changes.

### Orchestrator Change

Replace the inline block:

```csharp
bool isTrendOrRetestTrade = pos.IsTRENDTrade || pos.IsRetestTrade;
bool allowPointBasedTrailing = !isTrendOrRetestTrade || pos.IsRMATrade;
if (!allowPointBasedTrailing)
    continue;
```

With:

```csharp
if (!ManageTrail_ShouldAllowPointBasedTrailing(pos))
    continue;
```

### Acceptance Criteria

- [ ] `ManageTrail_ShouldAllowPointBasedTrailing` exists in `src/V12_002.Trailing.cs` (same partial class)
- [ ] Method is decorated with `[MethodImpl(MethodImplOptions.AggressiveInlining)]`
- [ ] Method signature: `private bool ManageTrail_ShouldAllowPointBasedTrailing(PositionInfo pos)`
- [ ] Logic: `bool isTrendOrRetestTrade = pos.IsTRENDTrade || pos.IsRetestTrade; return !isTrendOrRetestTrade || pos.IsRMATrade;`
- [ ] Orchestrator calls `!ManageTrail_ShouldAllowPointBasedTrailing(pos)` with `continue`
- [ ] New helper CYC <= 3 (strict McCabe: 1 base + 1 OR logical in assignment + 1 OR logical in return = 3)
- [ ] No LINQ introduced
- [ ] No `lock()` blocks introduced
- [ ] Zero heap allocations in new helper (pure boolean logic)
- [ ] `dotnet build` passes with zero errors

### CYC Projection After T136-02

| Method | CYC (strict McCabe) | CYC (Lizard) | Threshold | Status |
|---|---|---|---|---|
| `ManageTrailingStops` (orchestrator) | 8 | 7 | 8 | AT LIMIT |
| `ManageTrail_ShouldProcessPosition` | 6 | 4 | 8 | PASS |
| `ManageTrail_ShouldAllowPointBasedTrailing` | 3 | 3 | 8 | PASS |

---

## T136-03

**ID:** T136-03
**Type:** verification
**Priority:** P0
**File:** [`src/V12_002.Trailing.cs`](../../src/V12_002.Trailing.cs)
**Depends On:** T136-01, T136-02 (both extraction tickets must be applied)
**CYC Target:** `ManageTrailingStops` orchestrator <= 8

### Description

Verify the complete extraction result. Confirm orchestrator CYC reduction from 14 (strict McCabe)
to <= 8, validate the final refactored body matches the Phase 2 architecture plan, and confirm
all critical ordering constraints are preserved (circuit breaker first, ShadowEngineCheck last,
SIMA branch intact, activePositions snapshot pattern intact).

### Final Expected Orchestrator Body

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
        string entryName = kvp.Key;
        PositionInfo pos = kvp.Value;

        if (!ManageTrail_ShouldProcessPosition(entryName, pos))
            continue;

        pos.TicksSinceEntry++;
        pos.ExtremePriceSinceEntry =
            pos.Direction == MarketPosition.Long
                ? Math.Max(pos.ExtremePriceSinceEntry, Close[0])
                : Math.Min(pos.ExtremePriceSinceEntry, Close[0]);

        if (ManageTrail_RunPerTradeBranches(entryName, pos))
            continue;

        if (!ManageTrail_ShouldAllowPointBasedTrailing(pos))
            continue;

        double _newStopPrice = pos.CurrentStopPrice;
        int _newTrailLevel = pos.CurrentTrailLevel;
        ManageTrail_RunPointBasedTrailing(entryName, pos, ref _newStopPrice, ref _newTrailLevel);
    }

    if (EnableSIMA)
    {
        var updatedSnapshot = activePositions.ToArray();
        ManageTrail_RunFleetSymmetrySync(updatedSnapshot);
    }

    ShadowEngineCheck();
}
```

### Acceptance Criteria

- [ ] `ManageTrailingStops()` signature unchanged: `private void ManageTrailingStops()`
- [ ] `ManageTrail_AdaptiveThrottleTick` called FIRST (circuit breaker ordering preserved)
- [ ] `ShadowEngineCheck()` called LAST (order-dependency preserved)
- [ ] `EnableSIMA` branch and `ManageTrail_RunFleetSymmetrySync` call preserved with post-loop ordering
- [ ] `activePositions.ToArray()` snapshot pattern preserved in both foreach loop and fleet sync
- [ ] Direction-dependent ternary (`ExtremePriceSinceEntry` Long/Short) remains in orchestrator (NOT extracted)
- [ ] `ManageTrail_RunPerTradeBranches` call and its `continue` preserved unchanged
- [ ] Caller `V12_002.BarUpdate.cs:327` (`Enqueue(ctx => ctx.ManageTrailingStops())`) is UNTOUCHED
- [ ] No new `lock()` blocks in any modified or added code
- [ ] No LINQ in any modified or added code
- [ ] Both helpers present: `ManageTrail_ShouldProcessPosition` and `ManageTrail_ShouldAllowPointBasedTrailing`
- [ ] Both helpers decorated with `[MethodImpl(MethodImplOptions.AggressiveInlining)]`
- [ ] CYC of orchestrator: <= 8 (strict McCabe), <= 7 (Lizard-compatible)
- [ ] CYC of `ManageTrail_ShouldProcessPosition`: <= 6
- [ ] CYC of `ManageTrail_ShouldAllowPointBasedTrailing`: <= 3
- [ ] `dotnet build` passes with zero errors
- [ ] `powershell -File .\deploy-sync.ps1` executed (hard-link sync)

---

## Implementation Order

```
T136-01  →  T136-02  →  T136-03
(extract    (extract      (verify
 helper1)    helper2)      CYC<=8)
```

All tickets operate on `src/V12_002.Trailing.cs` only. No parallel execution — each ticket depends
on the prior ticket's file state.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Wave** | 7 |
| **Phase** | 4 |
| **Epic** | EPIC-W7-136 |
| **Target Method** | `ManageTrailingStops` |
| **Source File** | `src/V12_002.Trailing.cs` |
| **CYC Before** | 14 (strict McCabe) / 10 (Lizard) |
| **CYC After (max_cyc_projected)** | 8 (strict) / 7 (Lizard) |
| **Extraction Count** | 2 |
| **Ticket Count** | 3 |
| **DNA Verdict** | PASS |
| **MCP: resolve_repo** | confirmed live (5147 symbols) |
| **MCP: get_symbol_complexity** | data artifact — partial class (expected) |
| **MCP: get_extraction_candidates** | empty — single Enqueue caller (expected) |
| **Sequential Thinking Thoughts** | 3 (CYC analysis, ticket structure, finalization) |
| **Jane Street KB Applied** | carl_cook (AggressiveInlining, zero-alloc), gjengset (no locks, snapshot), trading_billions (single responsibility, CYC<=8) |
| **Bobcoins Used** | 0.8 |
