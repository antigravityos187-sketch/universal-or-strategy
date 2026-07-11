# EPIC-W7-072 Hotspot Analysis

**Method:** `ProcessAccountOrder_UpdateMasterExpected`
**CYC:** 12
**File:** `src/V12_002.Orders.Callbacks.AccountOrders.cs` (lines 81–115)

---

## Overview

`ProcessAccountOrder_UpdateMasterExpected` is a broker-thread callback handler called exclusively
from `OnAccountOrderUpdate` (line 71) when the firing account matches `this.Account` (master).
Its sole responsibility is to update `expectedPositions` for the master account key whenever a
Stop or Target order transitions to `Filled` or `PartFilled`. It defers all mutations to the
strategy thread via the `Enqueue(ctx => ...)` actor pattern.

Despite its small size (33 lines), the method carries CYC=12 due to a compound outer guard, a
two-branch signal-name dispatch (`Stop_` vs `T_`), and a three-level nested conditional inside
the deferred lambda closure for the target fill path. The lambda-internal branching is the primary
complexity hotspot because it is invisible to surface-level CYC tooling yet contains the
direction-aware signed-arithmetic that directly feeds REAPER desync detection.

---

## CYC Breakdown

| # | Branch | CYC Δ | Location |
|---|---|---|---|
| 1 | Base path | +1 | — |
| 2 | `if (Filled \|\| PartFilled)` outer guard | +1 | line 83 |
| 3 | `\|\|` compound in outer guard (PartFilled arm) | +1 | line 83 |
| 4 | `if (order.Name.StartsWith("Stop_"))` | +1 | line 85 |
| 5 | `else if (order.Name.StartsWith("T") && ...)` | +1 | line 93 |
| 6 | `&&` compound in target name check (`.Contains("_")`) | +1 | line 93 |
| 7 | `Enqueue` lambda closure (deferred branch point) | +1 | line 97 |
| 8 | `if (ctx.expectedPositions != null && TryGetValue)` in lambda | +1 | line 100 |
| 9 | `&&` compound in lambda null-guard (TryGetValue arm) | +1 | line 101 |
| 10 | `if (currentExp > 0)` direction positive arm | +1 | line 105 |
| 11 | `else if (currentExp < 0)` direction negative arm | +1 | line 107 |
| **Total** | | **12** | ✅ confirmed |

---

## Blast Radius Summary

| Dimension | Detail |
|---|---|
| **Direct caller** | `OnAccountOrderUpdate` line 71, `src/V12_002.Orders.Callbacks.AccountOrders.cs` |
| **Caller condition** | Only when `acct == this.Account && order.Instrument.FullName == Instrument.FullName` |
| **Symmetric sibling** | `ProcessAccountOrder_UpdateFleetExpected` (lines 117–152) — same logic for fleet accounts |
| **Deferred executor** | `SetExpectedPositionLocked` in `src/V12_002.SIMA.cs` lines 124–139 |
| **Primary consumer** | `AuditMaster_CalculatePositionState` → `AuditMaster_HandleDesyncFlatten` in `src/V12_002.REAPER.Audit.cs` lines 568–619 |
| **Secondary consumer** | `AuditMaster_HandleNakedPosition` (`src/V12_002.REAPER.Audit.cs` lines 624–679) — reads `_nakedPositionFirstSeen` cleared by Stop_ path |
| **State mutated** | `expectedPositions[ExpKey(Account.Name)]` (ConcurrentDictionary, strategy-thread via Enqueue) |
| **State cleared** | `_nakedPositionFirstSeen[Account.Name]` (ConcurrentDictionary, broker-thread direct `TryRemove`) |
| **Threading model** | Called on broker thread; mutations marshalled to strategy thread via `Enqueue`; `_nakedPositionFirstSeen.TryRemove` is the only direct broker-thread write (thread-safe: ConcurrentDictionary) |
| **Risk on change** | **HIGH** — wrong `expectedPositions` value triggers `AuditMaster_HandleDesyncFlatten` → emergency master flatten; must preserve Stop_ check order before Target check and direction-aware signed delta arithmetic |

**Affected symbol count (blast radius):** 8 symbols directly coupled; 2 shared concurrent state dictionaries (`expectedPositions`, `_nakedPositionFirstSeen`).

---

## Top 3 Complexity Drivers

1. **Direction-aware signed delta inside a deferred lambda (CYC +3 hidden)**
   The target-fill path (lines 97–112) captures `filledQty` and `mExpKey` on the broker thread
   then enqueues a lambda that re-reads `currentExp` and applies `Math.Max(0, currentExp - filledQty)`
   or `Math.Min(0, currentExp + filledQty)` based on sign. The two `if/else if` arms inside the
   closure each contribute +1 CYC but are only visible to tools that analyse lambda bodies. This
   hidden complexity is the root cause of the CYC=12 rating for what appears to be a 33-line method.

2. **Compound guard on both the outer state check and inner null-guard (CYC +2)**
   Both `(order.OrderState == OrderState.Filled || order.OrderState == OrderState.PartFilled)` and
   `(ctx.expectedPositions != null && ctx.expectedPositions.TryGetValue(...))` use `&&`/`||`
   compounding, each adding +1 CYC. The inner lambda guard is particularly risky: if
   `expectedPositions` is null (startup/teardown edge), the entire target-fill update is silently
   dropped without logging.

3. **Signal-name string-prefix dispatch used as order-type discriminator (CYC +2, fragility risk)**
   The method uses `order.Name.StartsWith("Stop_")` and `order.Name.StartsWith("T") && Contains("_")`
   as the sole mechanism to distinguish stop fills from target fills. This naming convention is
   load-bearing: any future rename of signal prefixes would silently route fills to neither branch,
   leaving `expectedPositions` stale — a REAPER false-flatten hazard. This is not a complexity
   driver per se, but it is the primary *refactoring risk* any Phase 1 work must account for.

---

## Recommended Extraction Plan (Phase 1 Preview)

| Extract | From Lines | New Method | Est. CYC |
|---|---|---|---|
| Stop-fill handler | 85–92 | `HandleMasterStopFill(Order)` | 2 |
| Target-fill handler + lambda | 93–113 | `HandleMasterTargetFill(Order)` | 5 |
| Thin dispatcher | 83–114 | `ProcessAccountOrder_UpdateMasterExpected` | 3 |

This reduces the dispatcher to CYC≤3 and isolates the direction-aware arithmetic into a named,
testable unit. The `else if` in the lambda body becomes the sole concern of `HandleMasterTargetFill`.

---

## Agent Tracking

Agent Name: bob-hotspot-w7-072 | Bobcoins Used: 1.0 | Execution Time: ~60s
