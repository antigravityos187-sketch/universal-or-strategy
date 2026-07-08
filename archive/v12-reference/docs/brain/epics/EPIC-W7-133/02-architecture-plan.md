# EPIC-W7-133 — Phase 2: Architecture Plan

## Agent Tracking

| Field            | Value                                                      |
|------------------|------------------------------------------------------------|
| **Agent Name**   | v12-phase2-architecture                                    |
| **Wave**         | 7                                                          |
| **Phase**        | 2 — Architecture Planning                                  |
| **Generated**    | 2026-06-29T00:35:02Z                                       |
| **Input**        | `docs/brain/EPIC-W7-133/01-scope-boundary.md`              |
| **Output**       | `docs/brain/EPIC-W7-133/02-architecture-plan.md`           |
| **Bobcoins Used**| 1.5                                                        |

---

## MCP Evidence

### jcodemunch — get_context_bundle

Retrieved full source of `MoveStop_SinglePosition` (lines 73–163, 91 lines) via
`get_context_bundle` with symbol ID
`src/V12_002.Trailing.Breakeven.cs::V12_002.MoveStop_SinglePosition#method`.
This confirmed the exact body including all compound boolean expressions, early returns,
ternary guards, and call sites (`UpdateStopOrder`, `MarkStickyDirty`, `Print`).

### jcodemunch — get_dependency_graph

`get_dependency_graph` on `src/V12_002.Trailing.Breakeven.cs` (direction=both, depth=1)
returned zero import edges — the file is a self-contained partial class. No upstream
imports are introduced by this extraction. All new helpers remain in the same partial class.

### Call Hierarchy Summary (jcodemunch)

`get_call_hierarchy` (depth=2) confirmed:
- **1 direct caller**: `MoveStopsToBreakevenWithOffset` (same file, line 41)
- **26 transitive callees** — all downstream of `UpdateStopOrder` (in
  `src/V12_002.Trailing.StopUpdate.cs`) and `MarkStickyDirty` (in
  `src/V12_002.StickyState.cs`)
- No caller signature is modified by this extraction

### sequential-thinking — sequentialthinking

Three `sequentialthinking` thoughts executed:
1. **Complexity driver enumeration**: Mapped all 21 tool-reported decision nodes to 4
   clusters: direction calc, follower routing, ARM GUARD chain, master improvement check.
2. **Extraction strategy**: Designed 4 helpers with explicit CYC budgets; confirmed
   `IsStopImprovement` eliminates duplication across both follower and master paths.
3. **CYC validation**: Verified all helpers ≤ 8; max projected = 5; parent reduced to 2.

---

## Target Method

| Field           | Value                                       |
|-----------------|---------------------------------------------|
| **Method**      | `MoveStop_SinglePosition`                   |
| **File**        | `src/V12_002.Trailing.Breakeven.cs`         |
| **Class**       | `V12_002` (partial)                         |
| **Lines**       | 73–163 (91 lines)                           |
| **CYC Before**  | 21                                          |
| **CYC After**   | 2 (parent dispatcher)                       |
| **max_cyc_projected** | **5** (TryArmOrExecuteMasterBreakeven) |

---

## Complexity Driver Analysis

| Driver | Lines    | Description                                                      | Tool-Reported Decision Nodes |
|--------|----------|------------------------------------------------------------------|------------------------------|
| A      | 74–77    | Direction-based `newStopPrice` calculation (Long/Short branch)   | 1                            |
| B      | 83–112   | `if (pos.IsFollower)` sub-tree + `isBetterF` compound boolean    | 6                            |
| C      | 116–136  | ARM GUARD: stale price, ternary threshold, arm-and-defer         | 5                            |
| D      | 139–162  | Master `isBetter` compound boolean + final `UpdateStopOrder`     | 5                            |

**Duplication note**: `isBetterF` (Driver B) and `isBetter` (Driver D) are structurally
identical compound predicates. Extracting `IsStopImprovement` collapses both sites.

---

## Extraction Plan

| Helper Name                         | Responsibility                                                                           | Estimated CYC | Inlining Policy        |
|-------------------------------------|------------------------------------------------------------------------------------------|---------------|------------------------|
| `CalcBreakevenStopPrice`            | Compute direction-aware new stop price and round to tick size                            | **2**         | `AggressiveInlining`   |
| `IsStopImprovement`                 | Pure boolean predicate: is `newStopPrice` profit-protecting for `pos.Direction`?         | **4**         | `AggressiveInlining`   |
| `HandleFollowerBreakeven`           | Encapsulate entire `IsFollower` sub-tree: improvement check + stop update + state + log  | **2**         | `NoInlining`           |
| `TryArmOrExecuteMasterBreakeven`    | Encapsulate ARM GUARD chain + master improvement check + final `UpdateStopOrder`         | **5**         | `NoInlining`           |

**max_cyc_projected: 5** (all helpers ≤ 8, parent ≤ 8)

---

## Method Signatures

### Parent (After Extraction)

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private void MoveStop_SinglePosition(
    string entryName,
    PositionInfo pos,
    double offsetPoints,
    double lastKnownPrice)
```

Post-extraction body (3 statements, CYC 2):
```
double newStopPrice = CalcBreakevenStopPrice(pos, offsetPoints);
if (pos.IsFollower) { HandleFollowerBreakeven(entryName, pos, newStopPrice, offsetPoints); return; }
TryArmOrExecuteMasterBreakeven(entryName, pos, newStopPrice, lastKnownPrice, offsetPoints);
```

---

### Helper 1 — CalcBreakevenStopPrice

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private double CalcBreakevenStopPrice(PositionInfo pos, double offsetPoints)
```

**CYC: 2**
- 1 direction branch (`pos.Direction == MarketPosition.Long`)
- Returns `RoundToTickSize(entryPrice +/- offsetPoints)`
- Zero allocations; no LINQ; no lock()

---

### Helper 2 — IsStopImprovement

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private bool IsStopImprovement(PositionInfo pos, double newStopPrice)
```

**CYC: 4**
- `(pos.Direction == MarketPosition.Long && newStopPrice > pos.CurrentStopPrice)`
- `|| (pos.Direction == MarketPosition.Short && newStopPrice < pos.CurrentStopPrice)`
- Shared predicate — replaces both `isBetterF` (follower, lines 84–86) and `isBetter`
  (master, lines 139–141), eliminating the duplication identified in Driver B and D.
- Zero allocations; no LINQ; no lock()

---

### Helper 3 — HandleFollowerBreakeven

```csharp
[MethodImpl(MethodImplOptions.NoInlining)]
private void HandleFollowerBreakeven(
    string entryName,
    PositionInfo pos,
    double newStopPrice,
    double offsetPoints)
```

**CYC: 2**
- 1 branch: `if (IsStopImprovement(pos, newStopPrice))`
- On true: `UpdateStopOrder(entryName, pos, newStopPrice, 1)` + flag writes + `MarkStickyDirty()` + `Print`
- NoInlining: follower breakeven is a cold, user-triggered action
- Zero allocations; no LINQ; no lock()

---

### Helper 4 — TryArmOrExecuteMasterBreakeven

```csharp
[MethodImpl(MethodImplOptions.NoInlining)]
private void TryArmOrExecuteMasterBreakeven(
    string entryName,
    PositionInfo pos,
    double newStopPrice,
    double lastKnownPrice,
    double offsetPoints)
```

**CYC: 5**
- Guard 1: `if (lastKnownPrice <= 0)` → Print abort + return (+1)
- Guard 2: ternary `pos.Direction == MarketPosition.Long ? referencePrice >= newStopPrice : referencePrice <= newStopPrice` (+1)
- Guard 3: `if (!priceCleared)` → arm `ManualBreakevenArmed = true`, reset `ManualBreakevenTriggered`, Print + return (+1)
- Guard 4: `if (!IsStopImprovement(pos, newStopPrice))` → Print + return (+1)
- Hot execute: `UpdateStopOrder` + flag writes + `MarkStickyDirty()` + `Print`
- NoInlining: ARM GUARD path fires rarely; cold path
- **Deferred-execution contract preserved**: `ManualBreakevenArmed = true` write remains
  inside this helper, exactly matching the original behavior that arms deferred execution
  for `ManageTrailingStops()` to complete on next tick.
- Zero allocations; no LINQ; no lock()

---

## CYC Validation Summary

| Method                              | Decision Nodes                                    | CYC | Status      |
|-------------------------------------|---------------------------------------------------|-----|-------------|
| `MoveStop_SinglePosition` (parent)  | base(1) + IsFollower(1)                           | 2   | ✅ PASS ≤ 8 |
| `CalcBreakevenStopPrice`            | base(1) + direction(1)                            | 2   | ✅ PASS ≤ 8 |
| `IsStopImprovement`                 | base(1) + &&(1) + ||(1) + &&(1)                   | 4   | ✅ PASS ≤ 8 |
| `HandleFollowerBreakeven`           | base(1) + isBetter(1)                             | 2   | ✅ PASS ≤ 8 |
| `TryArmOrExecuteMasterBreakeven`    | base(1) + stale(1) + ternary(1) + arm(1) + imp(1) | 5   | ✅ PASS ≤ 8 |

**max_cyc_projected: 5**

---

## Jane Street Compliance

| Principle                                  | Compliance                                                                 |
|--------------------------------------------|----------------------------------------------------------------------------|
| No LINQ                                    | ✅ No LINQ in any helper or parent                                          |
| No new `lock()` blocks                     | ✅ No lock() introduced; all state writes are single-field assignments      |
| `AggressiveInlining` hot path              | ✅ `CalcBreakevenStopPrice` and `IsStopImprovement` marked AggressiveInlining |
| `NoInlining` cold path                     | ✅ `HandleFollowerBreakeven` and `TryArmOrExecuteMasterBreakeven` marked NoInlining |
| Zero heap allocations on hot path          | ✅ `string.Format` calls remain in cold helpers; hot path is allocation-free |
| Single responsibility per helper           | ✅ Each helper has exactly one concern (calc, predicate, follower, master)  |
| Defense in depth                           | ✅ ARM GUARD has 3 sequential guards each isolated with early-return        |
| CYC ≤ 8 for all helpers and parent         | ✅ max_cyc_projected = 5                                                    |
| Rate-limit / circuit breaker               | ✅ Not applicable (no network/IO paths in this method)                      |
| Deferred-execution contract preserved      | ✅ `ManualBreakevenArmed` write stays in `TryArmOrExecuteMasterBreakeven`   |

---

## Scope Confirmation (V12.23 No Scope Creep)

- **Single file modified**: `src/V12_002.Trailing.Breakeven.cs`
- **New symbols**: 4 private helpers (same partial class)
- **Caller signatures unchanged**: `MoveStop_SinglePosition` public contract identical
- **No cross-file changes**: `UpdateStopOrder`, `MarkStickyDirty`, `Print` called unchanged
- **Boundary verdict from Phase 1.5**: PASS

---

## Phase 5 Ticket Hints

1. **Ticket 1**: Add `IsStopImprovement` and `CalcBreakevenStopPrice` helpers (hot path, AggressiveInlining)
2. **Ticket 2**: Add `HandleFollowerBreakeven` helper (cold path, NoInlining)
3. **Ticket 3**: Add `TryArmOrExecuteMasterBreakeven` helper (cold path, NoInlining) — include ARM GUARD and deferred-execution comment preservation
4. **Ticket 4**: Rewrite `MoveStop_SinglePosition` body to delegate to 4 helpers (3-statement dispatcher)
5. **Verify**: `dotnet build` clean, CYC audit confirms all methods ≤ 8
