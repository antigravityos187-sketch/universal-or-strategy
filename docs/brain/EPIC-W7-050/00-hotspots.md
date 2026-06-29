# EPIC-W7-050 — Phase 0: Hotspot Analysis

## Method Under Analysis

| Field | Value |
|---|---|
| **Method** | `FleetSync_SyncFollowersToLevel` |
| **CYC (Cyclomatic Complexity)** | 34 |
| **File** | `src/V12_002.Trailing.cs` |
| **Lines** | 142–191 |
| **Class** | `V12_002` (partial, `Strategy`) |
| **Module** | Trailing Stops — Fleet Symmetry Sync |

---

## Blast Radius Summary

`FleetSync_SyncFollowersToLevel` sits at the heart of the **SIMA fleet-sync pipeline**. Its single call site is
`ManageTrail_RunFleetSymmetrySync` (line 115), which itself is called every tick from `ManageTrailingStops`
when `EnableSIMA == true`. Any change to this method therefore affects **every live follower position on every
tick** in multi-position SIMA mode.

Direct callees that carry their own complexity budget:

| Callee | File | Notes |
|---|---|---|
| `CalculateStopForLevel` | `src/V12_002.Trailing.StopUpdate.cs:533` | 4-case switch × 2 directions; safe, pure |
| `UpdateStopOrder` | `src/V12_002.Trailing.StopUpdate.cs:84` | High-complexity; branches on `OrderState`, holds circuit-breaker, spawns `PendingStopReplacement`, writes `activePositions` |
| `activePositions.ContainsKey` | internal field | `ConcurrentDictionary` — TOCTOU risk vs the later `UpdateStopOrder` write path |

The method mutates shared mutable state (`PositionInfo.CurrentTrailLevel`, `PositionInfo.CurrentStopPrice`)
indirectly through `UpdateStopOrder`, which also fires `MarkStickyDirty()` and may activate the circuit
breaker. A regression here can silently skip stop promotion for all followers, leaving them unprotected when
the leader advances.

---

## Top 3 Complexity Drivers

### 1. Compound guard chain (lines 153–168) — `if/continue` cascade
Five sequential early-exits (`IsFollower`, `EntryFilled`, `BracketSubmitted`, `ContainsKey`, `targetLevel==0`,
`CurrentTrailLevel >= targetLevel`) add **6 decision points** to the path count. Each guard uses a separate
condition expression on mutable state fields, making it hard to reason about which combination a given follower
hits and creating test-case explosion.

### 2. Directional ternary fan-out (lines 160, 173–176) — duplicated Long/Short logic
`targetLevel` selection and the `isBetter` price comparison each encode Long vs Short as an inline ternary.
This pattern is repeated across **every calling context in the trailing module** (see `CalculateStopForLevel`,
`TrailHandler_TREND_E1`, etc.). It prevents extraction of a single directional predicate and forces every
reader to mentally simulate both market sides simultaneously.

### 3. TOCTOU `activePositions.ContainsKey` → `UpdateStopOrder` (lines 157, 180)
The `ContainsKey` check is not atomic with the subsequent `UpdateStopOrder` → `activePositions.TryGetValue`
sequence inside `UpdateStopOrder` itself. In theory, the position can be removed between the two calls.
The current code silently proceeds rather than re-checking, which means the `stopOrders.TryGetValue` guard
inside `UpdateStopOrder` becomes the real last line of defence — adding implicit hidden coupling that inflates
the effective complexity a reviewer must trace.

---

## Recommended Extraction Count

**3 extractions** are advised to bring the method to CYC ≤ 10:

1. **`FleetSync_ValidateFollower(PositionInfo, string) → bool`** — consolidates the five-guard chain into a
   single named predicate, eliminating 5 decision points from the main loop body.
2. **`FleetSync_ResolveTargetLevel(PositionInfo, int, int) → int`** — wraps the direction-dispatch ternary
   for `targetLevel`, making direction logic testable in isolation.
3. **`FleetSync_IsStopImprovement(PositionInfo, double) → bool`** — encapsulates the `isBetter` ternary,
   centralising the Long/Short stop-improvement predicate used in multiple trailing handlers.

---

## Agent Tracking

```
EPIC:        EPIC-W7-050
WAVE:        7
PHASE:       0 — Hotspot Analysis
STATUS:      completed
OUTPUT:      docs/brain/EPIC-W7-050/00-hotspots.md
METHOD:      FleetSync_SyncFollowersToLevel
CYC:         34 (confirmed)
SOURCE_FILE: src/V12_002.Trailing.cs:142
ANALYST:     Bob (automated hotspot agent)
TIMESTAMP:   2025-07-15
```
