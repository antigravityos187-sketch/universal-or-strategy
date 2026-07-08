# EPIC-W7-044 · Phase 0 — Hotspot Analysis

## Target Method

| Field             | Value                                              |
|-------------------|----------------------------------------------------|
| **Method**        | `SymmetryGuardCascadeFollowerCleanup`              |
| **CYC**           | 11                                                 |
| **File**          | `src/V12_002.Symmetry.Replace.cs` (lines 198–243) |
| **Class**         | `V12_002` (partial)                                |
| **Wave / Phase**  | Wave 7 / Phase 0                                   |

---

## Blast Radius Summary

`SymmetryGuardCascadeFollowerCleanup` is called from exactly **one** direct call-site:

- [`HandleOrderCancelled_RollbackUnfilledEntry`](src/V12_002.Orders.Callbacks.cs:756) —
  fires when a master entry order is confirmed-cancelled (SIMA enabled, non-follower).
  Guard: `EnableSIMA && !kvp.Value.IsFollower`.

Downstream blast fan:

1. **`CancelOrderSafe`** (`src/V12_002.Orders.CancelGateway.cs:18`) — routes each follower
   cancel through `ExecutingAccount.Cancel()` (fleet) or `CancelOrder()` (master).
   23 call-sites across 9 files depend on this gateway.

2. **`HandleMatchedFollower_DeltaRollback`** (`src/V12_002.Orders.Callbacks.AccountOrders.cs:691`) —
   deferred delta rollback that fires on confirmed cancel (Build 960 A2-3 audit fix).
   The FSM split here (cancel in this method, rollback deferred) means any refactor must
   preserve the two-phase ordering guarantee.

3. **`symmetryMasterEntryToDispatch`** / **`symmetryDispatchById`** / **`ctx.Followers`** —
   three concurrent maps read lock-free via the ADR-019 immutable-snapshot pattern;
   mutation races possible if extraction breaks the read ordering.

4. **`RollbackExpectedPosition` → `CleanupPosition`** — called in the *caller*
   (`HandleOrderCancelled_RollbackUnfilledEntry`) immediately after this method returns;
   extraction must not duplicate or re-order these calls.

**Total affected files (direct + transitive):** 5 source files.
**Total external call-sites of direct caller:** 1.

---

## Top 3 Complexity Drivers

| # | Driver | Lines | Detail |
|---|--------|-------|--------|
| 1 | **Triple-dictionary guard chain** | 200–206 | Two sequential `TryGetValue` early-exits on `symmetryMasterEntryToDispatch` then `symmetryDispatchById` before touching any follower state. Both early-exit paths add branches that inflate CYC. |
| 2 | **Per-follower `OrderState` multi-branch predicate** | 225–229 | The inner loop evaluates three `OrderState` enum values (Working / Submitted / Accepted) in an OR chain per follower — equivalent to 3 decision points inside the loop body. |
| 3 | **Null-guard cascade inside foreach** | 218–223 | Three consecutive `continue` guards (`activePositions`, `entryOrders`, null check on `order`) before the OrderState block each count as independent cyclomatic branches. |

---

## Recommended Extraction Count

**3 extractions** are recommended:

1. `TryResolveCascadeContext(string masterEntryName, out string[] followers)` —
   encapsulates the two-dictionary lookup and snapshot read (lines 200–206).

2. `IsFollowerEntryLive(Order order)` —
   encapsulates the three-state OrderState predicate (lines 225–229);
   already used verbatim in `SymmetryGuardReplaceExistingFollowerTarget` (lines 45–51)
   and is a natural pure-function extraction.

3. `TryCancelFollowerEntry(string followerName)` —
   encapsulates the three null-guards + conditional cancel body (lines 218–241),
   reducing the foreach body to a single method call.

Estimated post-extraction CYC: **4** (loop + two early-exits + SIMA guard in caller).

---

## Agent Tracking

```
EPIC:        EPIC-W7-044
WAVE:        7
PHASE:       0
STATUS:      completed
OUTPUT:      docs/brain/EPIC-W7-044/00-hotspots.md
AGENT:       Bob (analysis)
TIMESTAMP:   2025-07-17T00:00:00Z
CYC_SOURCE:  Static analysis — manual branch count confirmed against source
CYC_CONFIRMED: 11
NOTES: |
  Blast radius is shallow (1 direct caller). The two-phase cancel/rollback FSM split
  across Orders.Callbacks.cs and Orders.Callbacks.AccountOrders.cs is the primary
  safety constraint for any subsequent refactor phases.
```
