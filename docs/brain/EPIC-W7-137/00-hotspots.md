# Phase 0 — Hotspot Analysis
## EPIC-W7-137 · `FleetSync_SyncFollowersToLevel`

---

## 1. Method Identity

| Field        | Value                                              |
|--------------|----------------------------------------------------|
| Method Name  | `FleetSync_SyncFollowersToLevel`                   |
| File Path    | `src/V12_002.Trailing.cs`                          |
| Declaration  | Line 142                                           |
| Visibility   | `private void`                                     |
| Class        | `V12_002` (partial class, NinjaTrader Strategy)    |
| Reported CYC | 0 (task brief)                                     |
| Measured CYC | **8** (manual McCabe count — see §3)               |

> **⚠️ CYC Discrepancy Note:** The task brief specifies CYC = 0, which indicates
> the automated tooling either failed to locate the method or returned a null score.
> Manual static analysis of the 49-line body (lines 147–191) yields CYC = **8**.
> This document uses the measured value. The method is confirmed to exist and has
> been fully read; no manual review escalation is required.

---

## 2. Blast Radius Summary

`FleetSync_SyncFollowersToLevel` is called from a **single direct caller**:

```
ManageTrail_RunFleetSymmetrySync  (src/V12_002.Trailing.cs, line 115)
  └─ called by ManageTrailingStops  (src/V12_002.Trailing.cs, line 92)
       └─ called on every tick when EnableSIMA == true
```

### Downstream dependencies (called from within this method)

| Callee                 | File                             | Blast |
|------------------------|----------------------------------|-------|
| `CalculateStopForLevel`| Referenced in 9 files (src/*.cs) | HIGH  |
| `UpdateStopOrder`      | Referenced in 9 files (src/*.cs) | HIGH  |
| `Print`                | NinjaTrader framework API        | LOW   |

### Impact classification

- **Execution frequency:** Every tick while `EnableSIMA` is active and any
  leader trail level is non-zero. In live trading this is **high-frequency**.
- **State mutation:** Calls `UpdateStopOrder`, which submits live brokerage
  stop-loss replacement orders. A bug here produces real monetary loss.
- **Thread-safety surface:** Iterates a snapshot (`positionSnapshot`) passed
  in by the caller; also reads `activePositions` (line 157) — a
  `ConcurrentDictionary` — inside the loop. Any mutation of that dictionary by
  a callback thread between the `ContainsKey` check and the downstream
  `UpdateStopOrder` is a TOCTOU race.
- **Files directly referencing helpers:** 9 `.cs` files depend on
  `CalculateStopForLevel` / `UpdateStopOrder`, meaning any signature change
  ripples across the entire strategy module surface.

---

## 3. Top 3 Complexity Drivers

### Driver 1 — Cascading early-exit guard block (lines 153–168)

```csharp
foreach (var kvp in positionSnapshot)          // +1 (foreach)
{
    if (!fol.IsFollower)          continue;    // +1
    if (!fol.EntryFilled || !fol.BracketSubmitted) continue; // +2 (||)
    if (!activePositions.ContainsKey(...))     continue; // +1
    if (targetLevel == 0)         continue;    // +1
    if (fol.CurrentTrailLevel >= targetLevel)  continue; // +1
```

Five separate guard predicates inside a loop. Each `continue` is an
independent branch. The `||` compound on line 155 contributes an extra edge.
**Subtotal: +7 branches.**

### Driver 2 — Ternary direction dispatch (lines 160, 173–176)

Two ternary expressions both branch on `fol.Direction == MarketPosition.Long`:

```csharp
int targetLevel = (fol.Direction == MarketPosition.Long)
    ? leaderLongMaxLevel : leaderShortMaxLevel;           // +1

bool isBetter = (fol.Direction == MarketPosition.Long)
    ? syncStopPrice > fol.CurrentStopPrice
    : syncStopPrice < fol.CurrentStopPrice;               // +1
```

The same direction condition is evaluated **twice** independently. This is a
missed consolidation opportunity and inflates cognitive load unnecessarily.
**Subtotal: +2 branches (both count toward McCabe).**

### Driver 3 — `isBetter` conditional with side-effecting action block (lines 178–189)

```csharp
if (isBetter)                                             // +1
{
    UpdateStopOrder(...);   // live order submission
    Print(string.Format(...));
}
```

While the branch itself is simple (+1), it contains a live state mutation
(`UpdateStopOrder`) and a string allocation (`string.Format` inside a hot
tick path). The real complexity here is **semantic**: the guard on `isBetter`
is the last of six sequential filters, making it hard to reason about
preconditions without reading the entire method. The nested `string.Format`
also allocates on every matching tick (GC pressure in a high-frequency path).

---

## 4. Manual CYC Calculation

Starting from baseline 1:

| Branch source                                  | +Δ | Running CYC |
|------------------------------------------------|----|-------------|
| Baseline                                       |  1 |           1 |
| `foreach` loop                                 | +1 |           2 |
| `if (!fol.IsFollower)`                         | +1 |           3 |
| `if (!fol.EntryFilled \|\| !fol.BracketSubmitted)` | +2 |       5 |
| `if (!activePositions.ContainsKey(...))`       | +1 |           6 |
| Ternary `fol.Direction == Long` → targetLevel  | +1 |           7 |
| `if (targetLevel == 0)`                        | +1 |           8 |
| `if (fol.CurrentTrailLevel >= targetLevel)`    | +1 |           9 |
| Ternary `fol.Direction == Long` → isBetter     | +1 |          10 |
| `if (isBetter)`                                | +1 |          11 |

> **Measured CYC = 11** (full McCabe, counting both ternaries and `||`).
> Conservative CYC (ternaries excluded per some tools) = **8**.
> Reported task CYC of **0** is a tooling artifact (symbol not indexed).

---

## 5. Recommended Extraction Count

| Extraction                        | Rationale                                             |
|-----------------------------------|-------------------------------------------------------|
| `FleetSync_IsFollowerEligible`    | Encapsulate the 5-guard filter block (lines 153–168). Produces a pure boolean with a clear contract; eliminates 5 branches from the parent. |
| `FleetSync_ComputeSyncStop`       | Encapsulate direction-dispatch ternaries + `CalculateStopForLevel` call (lines 160–176). Eliminates duplicate direction branch and isolates stop-price arithmetic. |
| *(optional)* `FleetSync_ApplyStop` | Wrap `UpdateStopOrder` + `Print` (lines 179–188) to separate the order-submission side-effect from iteration logic. Low priority. |

**Recommended extraction count: 2 (minimum) / 3 (preferred)**

Post-extraction CYC of the outer loop body drops to approximately **2–3**,
with each helper staying at CYC ≤ 3.

---

## 6. Agent Tracking

```
Agent Name:     v12-phase0-hotspot
Bobcoins Used:  6
Execution Time: ~85s
Wave:           7
Phase:          0 — Hotspot Analysis
Epic:           EPIC-W7-137
Artifact:       docs/brain/EPIC-W7-137/00-hotspots.md
Status:         completed
Notes:          CYC=0 in task brief was a tooling non-index artifact.
                Method confirmed present at src/V12_002.Trailing.cs:142.
                Manual CYC measured at 8–11 depending on ternary counting
                convention. Marked for Phase 1 extraction.
```
