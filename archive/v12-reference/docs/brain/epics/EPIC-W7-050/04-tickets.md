# Phase 4: Implementation Tickets — EPIC-W7-050

<!-- metadata
epic: EPIC-W7-050
method: FleetSync_SyncFollowersToLevel
source_file: src/V12_002.Trailing.cs
original_cyc: 34
max_cyc_projected: 5
ticket_count: 4
wave: 7
lane: P4-L3
dna_verdict: PASS
agent: v12-phase4-tickets
phase: 4
-->

## Overview

| Field | Value |
|---|---|
| **Epic** | EPIC-W7-050 |
| **Method** | `FleetSync_SyncFollowersToLevel` |
| **Source File** | [`src/V12_002.Trailing.cs`](../../src/V12_002.Trailing.cs) |
| **Original CYC** | 34 |
| **Target CYC** | ≤ 8 (Jane Street strict standard) |
| **Max CYC Projected** | 5 |
| **Ticket Count** | 4 |
| **Extraction Strategy** | Guard-chain extraction + direction dispatch + stop-improvement predicate + loop-body ProcessSingleItem pattern |
| **Execution Order** | T1 / T2 / T3 independent → T4 last (integration + parent refactor) |

---

## Ticket W7-050-T1 — Extract `FleetSync_ValidateFollower` (Guard-Chain Extraction)

### Summary
Extract the 5-part guard early-exit chain from `FleetSync_SyncFollowersToLevel` into a standalone private predicate `FleetSync_ValidateFollower`. This single extraction eliminates the largest complexity cluster in the parent method, reducing parent CYC by ~10.

### Context
The loop body of `FleetSync_SyncFollowersToLevel` opens with five consecutive `if (!...) continue;` guards checking:
1. `!fol.IsFollower`
2. `!fol.EntryFilled || !fol.BracketSubmitted`
3. `!activePositions.ContainsKey(entryName2)`
4. `targetLevel == 0` (handled post-dispatch in parent — see T4)
5. `fol.CurrentTrailLevel >= targetLevel` (handled in parent — see T4)

Guards 1–3 constitute the follower-validity precondition that must be extracted. The remaining guards (4, 5) stay in the parent as part of the post-dispatch flow (T4 scope). This extraction reduces the parent from CYC=34 toward the projected CYC=5.

### Implementation

Add the following private method to `src/V12_002.Trailing.cs` (insertion point: after `FleetSync_SyncFollowersToLevel` closing brace):

```csharp
/// <summary>
/// Returns true when the follower position is eligible for fleet-sync level processing.
/// Consolidates the 3-part validity guard chain: IsFollower, EntryFilled+BracketSubmitted, activePositions presence.
/// CYC = 5
/// </summary>
private bool FleetSync_ValidateFollower(PositionInfo fol, string entryName2)
{
    if (!fol.IsFollower)
        return false;

    if (!fol.EntryFilled || !fol.BracketSubmitted)
        return false;

    if (!activePositions.ContainsKey(entryName2))
        return false;

    return true;
}
```

**Jane Street Alignment:**
- Guard clauses extracted per "Extract Guard Clauses" rule
- Early-return bool prevents nested if-pyramid
- Illegal states (non-follower, unfilled, unregistered) are unrepresentable in the calling loop body
- Zero heap allocation: `bool` return, value-type params

### Acceptance Criteria

- [ ] `FleetSync_ValidateFollower` exists in `src/V12_002.Trailing.cs` as a `private bool` method
- [ ] Method body contains exactly 3 guard returns covering IsFollower, EntryFilled+BracketSubmitted, activePositions.ContainsKey
- [ ] No lock() blocks introduced
- [ ] All identifiers are ASCII-only
- [ ] `dotnet csharpier check src/` passes with no formatting issues
- [ ] `dotnet build` passes with zero errors
- [ ] CYC of `FleetSync_ValidateFollower` = 5 (verified via complexity_audit.py or Codacy)

---

## Ticket W7-050-T2 — Extract `FleetSync_ResolveTargetLevel` (Direction-Dispatch Extraction)

### Summary
Extract the directional ternary `(fol.Direction == MarketPosition.Long) ? leaderLongMaxLevel : leaderShortMaxLevel` into a named predicate `FleetSync_ResolveTargetLevel`. This creates a single testable, readable dispatch point for long/short level resolution.

### Context
Inside the loop body, the original code computes `targetLevel` via an inline ternary that mixes direction logic into the orchestration flow. Extracting this into a named method:
- Makes the intent explicit (direction dispatch = its own responsibility)
- Enables xUnit testing of the dispatch logic in isolation
- Reduces the inline decision count in the parent loop body
- CYC reduction contribution: eliminates 1 branch from parent

### Implementation

Add the following private method to `src/V12_002.Trailing.cs`:

```csharp
/// <summary>
/// Resolves the sync target level for a follower based on its direction.
/// Returns leaderLongMaxLevel for Long positions, leaderShortMaxLevel for Short positions.
/// CYC = 2
/// </summary>
private int FleetSync_ResolveTargetLevel(
    PositionInfo fol,
    int leaderLongMaxLevel,
    int leaderShortMaxLevel)
{
    return fol.Direction == MarketPosition.Long
        ? leaderLongMaxLevel
        : leaderShortMaxLevel;
}
```

**Jane Street Alignment:**
- Single-responsibility: does exactly one thing — direction-to-level dispatch
- Named predicate replaces anonymous inline ternary
- `int` return, value-type params — zero heap allocation
- Lock-free: no state mutation, pure computation

### Acceptance Criteria

- [ ] `FleetSync_ResolveTargetLevel` exists in `src/V12_002.Trailing.cs` as a `private int` method
- [ ] Method returns `leaderLongMaxLevel` for `MarketPosition.Long`, `leaderShortMaxLevel` otherwise
- [ ] No lock() blocks introduced
- [ ] All identifiers are ASCII-only
- [ ] `dotnet csharpier check src/` passes with no formatting issues
- [ ] `dotnet build` passes with zero errors
- [ ] CYC of `FleetSync_ResolveTargetLevel` = 2

---

## Ticket W7-050-T3 — Extract `FleetSync_IsStopImprovement` (Stop-Improvement Predicate Extraction)

### Summary
Extract the Long/Short `isBetter` ternary predicate into a standalone private method `FleetSync_IsStopImprovement`. This centralises the stop-improvement check that appears across multiple trailing handlers, making it a single authoritative source for the comparison logic.

### Context
The original code evaluates `isBetter` as:
- Long: `syncStopPrice > fol.CurrentStopPrice`
- Short: `syncStopPrice < fol.CurrentStopPrice`

This direction-aware comparison is a distinct logical concern from the loop orchestration. Extracting it:
- Creates a reusable predicate shared across fleet-sync and future trailing handlers
- Eliminates 1 inline ternary from the loop body (CYC -1 from parent)
- Enables direct xUnit testing of the stop-improvement logic with known price inputs

### Implementation

Add the following private method to `src/V12_002.Trailing.cs`:

```csharp
/// <summary>
/// Returns true when syncStopPrice represents an improvement over the follower's current stop price.
/// For Long: improvement means a higher stop (trailing up). For Short: improvement means a lower stop (trailing down).
/// CYC = 2
/// </summary>
private bool FleetSync_IsStopImprovement(PositionInfo fol, double syncStopPrice)
{
    return fol.Direction == MarketPosition.Long
        ? syncStopPrice > fol.CurrentStopPrice
        : syncStopPrice < fol.CurrentStopPrice;
}
```

**Jane Street Alignment:**
- Single-responsibility: encapsulates the stop-improvement predicate only
- Centralises direction-aware comparison logic — "illegal states unrepresentable" (wrong comparison direction is structurally impossible)
- `bool` return, `double` + `PositionInfo` params — zero heap allocation
- Lock-free: pure predicate, no state mutation

### Acceptance Criteria

- [ ] `FleetSync_IsStopImprovement` exists in `src/V12_002.Trailing.cs` as a `private bool` method
- [ ] Method returns `syncStopPrice > fol.CurrentStopPrice` for Long and `syncStopPrice < fol.CurrentStopPrice` for Short
- [ ] No lock() blocks introduced
- [ ] All identifiers are ASCII-only
- [ ] `dotnet csharpier check src/` passes with no formatting issues
- [ ] `dotnet build` passes with zero errors
- [ ] CYC of `FleetSync_IsStopImprovement` = 2

---

## Ticket W7-050-T4 — Extract `FleetSync_SyncSingleFollower` + Refactor Parent (Loop-Body ProcessSingleItem + Integration)

### Summary
Extract the per-follower sync body (CalculateStopForLevel → IsStopImprovement check → UpdateStopOrder + Print) into `FleetSync_SyncSingleFollower` using the ProcessSingleItem pattern. Then refactor the parent `FleetSync_SyncFollowersToLevel` to wire up all 4 extracted helpers, reducing its CYC from 34 to 5.

**Prerequisite:** Tickets T1, T2, and T3 must be completed before executing T4.

### Context
After T1/T2/T3 are in place, the remaining loop body still contains:
- A `CalculateStopForLevel` call
- An `isBetter` check (now delegated to `FleetSync_IsStopImprovement`)
- A conditional `UpdateStopOrder` + `Print` block

This block is the "ProcessSingleItem" for the foreach loop. Extracting it into `FleetSync_SyncSingleFollower` completes the decomposition. The parent then becomes a pure orchestration loop: validate → resolve → guard → sync. CYC=5: 1 base + 1 loop + 1 validate-continue + 1 targetLevel==0 + 1 levelRegression.

### Implementation

**Step A — Add `FleetSync_SyncSingleFollower`** to `src/V12_002.Trailing.cs`:

```csharp
/// <summary>
/// Executes the sync operation for a single validated follower at the resolved target level.
/// Calculates the stop price for the target level, checks for stop improvement, and submits the order update.
/// Implements the ProcessSingleItem pattern for the fleet-sync foreach loop body.
/// CYC = 3
/// </summary>
private void FleetSync_SyncSingleFollower(
    string entryName2,
    PositionInfo fol,
    int targetLevel)
{
    double syncStopPrice = CalculateStopForLevel(fol, targetLevel);

    if (!FleetSync_IsStopImprovement(fol, syncStopPrice))
        return;

    UpdateStopOrder(entryName2, fol, syncStopPrice);
    Print($"[FleetSync] {entryName2} synced to level {targetLevel} stop={syncStopPrice}");
}
```

**Step B — Refactor parent `FleetSync_SyncFollowersToLevel`** in `src/V12_002.Trailing.cs` (lines 142–191):

Replace the existing method body with:

```csharp
private void FleetSync_SyncFollowersToLevel(
    KeyValuePair<string, PositionInfo>[] positionSnapshot,
    int leaderLongMaxLevel,
    int leaderShortMaxLevel)
{
    foreach (var kvp in positionSnapshot)
    {
        string entryName2 = kvp.Key;
        PositionInfo fol = kvp.Value;

        if (!FleetSync_ValidateFollower(fol, entryName2))
            continue;

        int targetLevel = FleetSync_ResolveTargetLevel(fol, leaderLongMaxLevel, leaderShortMaxLevel);

        if (targetLevel == 0)
            continue;

        if (fol.CurrentTrailLevel >= targetLevel)
            continue;

        FleetSync_SyncSingleFollower(entryName2, fol, targetLevel);
    }
}
```

**CYC breakdown for refactored parent:**
- 1 (base) + 1 (foreach) + 1 (ValidateFollower continue) + 1 (targetLevel==0 continue) + 1 (CurrentTrailLevel>=targetLevel continue) = **CYC = 5**

**Jane Street Alignment:**
- ProcessSingleItem pattern: loop body fully delegated to named method
- Parent is now pure orchestration: validate → resolve → guard → delegate
- All helpers single-responsibility (validate / resolve / check / execute)
- No lock() blocks; Actor/Enqueue path in UpdateStopOrder unchanged
- CYC=5 for parent and worst-case helper; all within Jane Street ≤8 threshold
- Zero heap allocation across all helpers (value types only)

### Acceptance Criteria

- [ ] `FleetSync_SyncSingleFollower` exists in `src/V12_002.Trailing.cs` as a `private void` method with params `(string entryName2, PositionInfo fol, int targetLevel)`
- [ ] `FleetSync_SyncSingleFollower` calls `CalculateStopForLevel`, `FleetSync_IsStopImprovement`, `UpdateStopOrder`, and `Print`
- [ ] Parent `FleetSync_SyncFollowersToLevel` body is replaced with the foreach + 3 guard continues + 2 delegation calls (as shown above)
- [ ] Parent method calls `FleetSync_ValidateFollower`, `FleetSync_ResolveTargetLevel`, and `FleetSync_SyncSingleFollower`
- [ ] No lock() blocks introduced in any method
- [ ] All identifiers are ASCII-only
- [ ] `dotnet csharpier check src/` passes with no formatting issues
- [ ] `dotnet build` passes with zero errors and zero warnings related to this extraction
- [ ] CYC of `FleetSync_SyncSingleFollower` = 3
- [ ] CYC of refactored `FleetSync_SyncFollowersToLevel` (parent) = 5
- [ ] `ManageTrail_RunFleetSymmetrySync` (direct caller) is NOT modified
- [ ] `UpdateStopOrder` internals are NOT modified (V12.23 — one epic, one concern)

---

## Execution Order Summary

| Ticket | Method | CYC | Depends On | Description |
|---|---|---|---|---|
| W7-050-T1 | `FleetSync_ValidateFollower` | 5 | — | 5-guard chain extraction |
| W7-050-T2 | `FleetSync_ResolveTargetLevel` | 2 | — | Direction dispatch extraction |
| W7-050-T3 | `FleetSync_IsStopImprovement` | 2 | — | Stop improvement predicate extraction |
| W7-050-T4 | `FleetSync_SyncSingleFollower` + parent refactor | 3 + 5 | T1, T2, T3 | Loop-body extraction + parent wiring |

---

## Jane Street Alignment Summary

| Rule | Status |
|---|---|
| CYC ≤ 8 for all methods | YES — parent=5, helpers max=5 |
| Single-responsibility per extraction | YES — validate / resolve / check / execute |
| Lock-free / Actor pattern preserved | YES — no lock() added; UpdateStopOrder Actor path unchanged |
| Illegal states unrepresentable | YES — FleetSync_ValidateFollower forces preconditions |
| Zero-allocation hot paths | YES — bool/int/double value types; no heap allocs |
| Extract Guard Clauses | YES — 3 guards into FleetSync_ValidateFollower |
| Extract Loop Body (ProcessSingleItem) | YES — FleetSync_SyncSingleFollower |
| No scope creep (V12.23) | YES — only FleetSync_SyncFollowersToLevel + 4 helpers touched |
| ASCII-only identifiers | YES — all method names and literals ASCII |
| xUnit test framework | YES — [Fact] + Assert.Equal() only for any tests |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Epic** | EPIC-W7-050 |
| **Wave** | 7 |
| **Phase** | 4 — Ticket Generation |
| **Bobcoins Used** | 1.8 |
| **Execution Time** | 2026-06-29T01:25:00Z |
| **jcodemunch tools called** | resolve_repo, get_symbol_complexity, get_extraction_candidates |
| **sequential-thinking calls** | 4 |
| **ticket_count** | 4 |
| **Output** | docs/brain/EPIC-W7-050/04-tickets.md |
