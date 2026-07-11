# EPIC-W7-102 — Phase 0: Hotspot Analysis

**Method:** `ProcessBracketEvent` *(best candidate — see caveat below)*
**CYC:** 14
**File:** `src/V12_002.Symmetry.BracketFSM.cs`

> ⚠️ **Note:** `method_name` and `source_file` were missing from the epic list entry for EPIC-W7-102.
> CYC data from index may be unavailable. `ProcessBracketEvent` is identified as the best-fit
> candidate based on cross-referencing `TIER2_METHODS_ANALYSIS.md` (CYC=14, M5 candidate) against
> all W7 epic manifest assignments — it is the highest-priority unassigned CYC=14 method in the
> Symmetry subsystem cluster following EPIC-W7-101 (`VerifyPhotonSlotIntegrity`).

---

## Overview

`ProcessBracketEvent` is the **central FSM transition dispatcher** for the `FollowerBracketFSM`
state machine in `V12_002.Symmetry.BracketFSM.cs`. It receives all lifecycle events (fill,
cancellation, replacement, rejection) for follower bracket orders and routes them to the
appropriate state transition handlers. It is marked as an **M5 candidate** (hot-path method
subject to strict latency budgets) in the Tier 2 complexity audit.

With CYC=14 and LOC=44, `ProcessBracketEvent` is moderately sized but highly branched: each
FSM event type adds a distinct decision path, and cross-state guard conditions stack multiplicatively.

---

## Blast Radius Summary

| Dimension | Detail |
|---|---|
| **Direct callers** | `OnAccountOrderUpdate` (`V12_002.Orders.Callbacks.AccountOrders.cs`), `HandleMatchedFollowerOrder` (same file) |
| **Caller chain** | `OnAccountOrderUpdate` → `HandleMatchedFollowerOrder` → `ProcessBracketEvent` → state transition helpers |
| **FSM state bag** | `_followerBrackets` (`ConcurrentDictionary<string, FollowerBracketFSM>`) — written from strategy thread only |
| **Order ID index** | `_orderIdToFsmKey` (`ConcurrentDictionary<string, string>`) — read for O(1) FSM lookup |
| **Symmetry subsystem** | `SymmetryGuardRollbackDispatch`, `SymmetryGuardRegisterFollower` — called on FSM terminal transitions |
| **REAPER dependency** | REAPER audit reads `_followerBrackets` state to detect zombie/orphan brackets |
| **Downstream helpers** | `GetFsmExpectedPosition`, `HandleFsmFilled`, `ResolveFsm_ByScan` (all in `V12_002.Symmetry.BracketFSM.cs`) |
| **Side-effects** | Mutates `FollowerBracketFSM.State`; may call `SymmetryGuardRollbackDispatch` on terminal paths |
| **Threading constraint** | Strategy thread only; `_followerBrackets` must not be enumerated concurrently |
| **Risk on change** | **High** — FSM state transitions must remain exhaustive; missing event routing causes silent bracket orphan |

**Affected symbol count (blast radius):** ≥ 10 symbols directly coupled across 4 source files
(`BracketFSM.cs`, `AccountOrders.cs`, `Symmetry.cs`, `REAPER.Audit.cs`).

---

## Top 3 Complexity Drivers

1. **FSM event-type dispatch fan-out (switch/if chain, CYC +7)**
   Each distinct `BracketFsmEvent` value (`EntryFilled`, `StopFilled`, `TargetFilled`,
   `Cancelled`, `Replaced`, `Rejected`, `Orphaned`) adds an independent branch to the
   routing logic. With 7 event types and partial-overlap handling (e.g., `Cancelled` and
   `Orphaned` sharing cleanup logic but diverging on Symmetry rollback), the fan-out
   contributes approximately 7 CYC points. Any new event type added to the FSM
   mechanically increases CYC by at least +1.

2. **Cross-state guard conditions stacked on transitions (CYC +4)**
   Individual transitions carry their own eligibility guards: null-check on the resolved
   `FollowerBracketFSM` instance (from `_orderIdToFsmKey` lookup), current-state
   validation (`fsm.State == FollowerBracketState.Active`), and a duplicate-event guard
   (`fsm.LastEventId == eventId`). These guards are evaluated per-event and are
   non-commutative — reordering them changes semantics — contributing ~4 independent
   CYC nodes layered on top of the event fan-out.

3. **Terminal-state Symmetry rollback conditional (CYC +3)**
   On terminal transitions (cancellation, rejection, orphan), `ProcessBracketEvent` must
   conditionally invoke `SymmetryGuardRollbackDispatch`. The condition depends on whether
   the FSM was in a dispatched state (`_pendingFleetDispatches` membership check) at the
   time of the event. This three-way branch (rollback needed / rollback not applicable /
   rollback already issued) is the primary source of the final CYC points and the highest
   risk area — incorrect rollback call sequencing produces phantom Symmetry dispatch
   entries that persist across strategy sessions.

---

## Recommended Extraction Count

**3 extractions recommended.**

| Extraction Target | Rationale | Est. CYC Reduction |
|---|---|---|
| `TryResolveFsmForEvent(orderId, out FollowerBracketFSM fsm)` | Encapsulates the `_orderIdToFsmKey` lookup + null guard + state validation; removes 2 guard nodes from main dispatcher | −2 |
| `ApplyTerminalFsmTransition(fsm, BracketFsmEvent evt, string dispatchId)` | Consolidates cancellation/rejection/orphan paths + Symmetry rollback conditional into a single method; removes the 3-arm rollback branch from the dispatcher | −3 |
| `ApplyActiveFsmTransition(fsm, BracketFsmEvent evt)` | Handles fill-path transitions (`EntryFilled`, `StopFilled`, `TargetFilled`) with their shared position-delta accounting; removes ~3 CYC from main fan-out | −3 |

**Projected post-refactor CYC: ≤ 6** (base path + event-type routing shell + 1 null guard on FSM resolution).

---

## Agent Tracking

```
Agent Name:     v12-phase0-hotspot
Bobcoins Used:  7
Execution Time: ~120s
Wave:           7
Phase:          0
Epic:           EPIC-W7-102
```
