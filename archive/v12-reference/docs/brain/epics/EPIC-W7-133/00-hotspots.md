# EPIC-W7-133 — Phase 0: Hotspot Analysis

## Method Identity

| Field        | Value                                         |
|--------------|-----------------------------------------------|
| Method Name  | `MoveStop_SinglePosition`                     |
| CYC Score    | **21** (tool-reported; structural count ≈ 12) |
| File Path    | `src/V12_002.Trailing.Breakeven.cs`           |
| Lines        | 73–163 (91 lines)                             |
| Class        | `V12_002` (partial)                           |
| Wave         | 7                                             |
| Epic         | EPIC-W7-133                                   |

---

## CYC Tool vs Structural Note

The static analysis tool (Codacy/McCabe) reports **CYC = 21**. A manual branch-count
of the method body yields a structural CYC of **12**. The delta of 9 is explained by the
tool counting each `&&` / `||` sub-expression in compound boolean conditions as individual
decision nodes (modified-condition/decision-coverage counting). This is consistent across
three compound boolean expressions in the method body, each contributing 3 sub-predicates.
The reported score of 21 is used as the authoritative value for this epic.

---

## Blast Radius Summary

### Direct Callers (1 caller)

| Caller Method                  | File                                    | Nature        |
|--------------------------------|-----------------------------------------|---------------|
| `MoveStopsToBreakevenWithOffset` | `src/V12_002.Trailing.Breakeven.cs`   | Orchestrator loop over `activePositions` |

### Indirect Callers (2 callers of `MoveStopsToBreakevenWithOffset`)

| Caller Method         | File                                   | Nature                             |
|-----------------------|----------------------------------------|------------------------------------|
| IPC command handler   | `src/V12_002.UI.IPC.Commands.Mode.cs` (line 340) | UI command path; user-triggered BE action |

### Downstream Side-Effects

| Touched Subsystem          | Mechanism                                        |
|----------------------------|--------------------------------------------------|
| `UpdateStopOrder`          | Called on both the follower fast-path and master slow-path — defined in `src/V12_002.Trailing.StopUpdate.cs`, touches 8 additional files |
| `MarkStickyDirty`          | Triggers sticky-state serialisation (`src/V12_002.StickyState.cs`); called from 2 branches inside this method |
| `pos.ManualBreakevenArmed` | Boolean flag read by `ManageTrailingStops()` in `src/V12_002.Trailing.cs` (line 447); write here arms deferred execution |
| `pos.ManualBreakevenTriggered` | Boolean flag read by `ManageTrailingStops()` and `src/V12_002.UI.Callbacks.cs` (line 1230); prevents double-fire |
| `activePositions` dictionary | Read-only iteration inside this method; 41 source files share this dictionary |

### Blast Radius Severity: **MEDIUM**
- Method is a pure leaf worker — it does not re-enter the position loop.
- Mutation surface is narrow: 3 `PositionInfo` fields + 1 stop order submission.
- Riskiest coupling: the ARM GUARD path writes `ManualBreakevenArmed = true` and returns,
  deferring actual stop execution to `ManageTrailingStops()`. Any extraction must preserve
  that deferred-execution contract exactly.

---

## Top 3 Complexity Drivers

### Driver 1 — Dual Routing Tree: Follower vs Master (lines 92–112)

The `if (pos.IsFollower)` block introduces an **early-return sub-tree** with its own
nested `isBetterF` compound conditional (2 direction checks joined by `||`). This block
fully duplicates the "is the new stop actually better?" guard logic that also appears
in the master path (lines 139–141). The duplication is the primary source of CYC inflation.

```
if (pos.IsFollower)                          // branch +1
    isBetterF = (Long && newStop > current)  // branch +2 (&&, ||)
             || (Short && newStop < current) // branch +2 (&&, ||)
    if (isBetterF)                           // branch +1
        → UpdateStopOrder + return
```

**CYC contribution: ~6 decision points**

---

### Driver 2 — ARM GUARD Multi-Exit Chain (lines 116–136)

Three sequential guard clauses each with their own early `return`:

1. `if (lastKnownPrice <= 0)` — stale price abort  
2. Ternary `referencePrice >= newStopPrice : referencePrice <= newStopPrice` — direction-aware threshold  
3. `if (!priceCleared)` — arm-and-defer path, sets `ManualBreakevenArmed = true`

Each guard is conceptually a separate concern (data validation, threshold evaluation,
arming) but they are sequenced inside a single method, raising both CYC and cognitive
load. The ternary on line 123 is evaluated mid-guard chain, making the logic hard to
follow at a glance.

**CYC contribution: ~5 decision points**

---

### Driver 3 — Directional `isBetter` Compound Conditional (lines 139–143)

The master path's improvement guard mirrors the follower's `isBetterF` check but is not
shared or abstracted:

```
bool isBetter =
    (pos.Direction == MarketPosition.Long  && newStopPrice > pos.CurrentStopPrice)  // +2
    || (pos.Direction == MarketPosition.Short && newStopPrice < pos.CurrentStopPrice); // +2
if (!isBetter) return;                                                                  // +1
```

This is the **third** site where `MarketPosition.Long` and `MarketPosition.Short` are
checked for the same conceptual purpose ("is the move profit-protecting?"). Extracting a
shared `IsStopImprovement(pos, newStopPrice)` predicate would collapse all three sites.

**CYC contribution: ~5 decision points**

---

## Recommended Extraction Count: **3 helpers**

| Proposed Helper                   | Responsibility                                                                | CYC Reduction |
|-----------------------------------|-------------------------------------------------------------------------------|---------------|
| `HandleFollowerBreakeven()`       | Encapsulates the entire `if (pos.IsFollower)` sub-tree (lines 92–112)        | −6            |
| `IsStopImprovement(pos, price)`   | Shared boolean predicate replacing `isBetterF` and `isBetter` duplicates     | −4            |
| `TryArmOrExecuteMasterBreakeven()`| Encapsulates the ARM GUARD chain + master `UpdateStopOrder` call (116–162)   | −5            |

**Projected post-refactor CYC: ≈ 6** (method becomes a thin 3-call dispatcher)

---

## Agent Tracking

| Field           | Value                          |
|-----------------|--------------------------------|
| Agent Name      | `v12-phase0-hotspot`           |
| Bobcoins Used   | 8                              |
| Execution Time  | ~95 seconds                    |
| Phase           | 0 — Hotspot Analysis           |
| Completed At    | 2025-07-14T00:00:00Z           |
| Output          | `docs/brain/EPIC-W7-133/00-hotspots.md` |
