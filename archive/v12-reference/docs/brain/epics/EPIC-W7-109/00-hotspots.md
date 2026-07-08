# EPIC-W7-109 Hotspot Analysis

**Method:** `HydrateWorkingOrdersFromBroker`
**CYC:** 34
**File:** `src/V12_002.SIMA.Lifecycle.cs`

---

## Overview

`HydrateWorkingOrdersFromBroker` (lines 309–457) is the top-level order-adoption orchestrator
called on strategy startup and broker reconnect. It delegates to `AdoptFleetOrders()`,
`AdoptMasterOrders()`, and `HydrateFSMsFromWorkingOrders()`, but retains a substantial inline
master-account position-reconstruction block (lines 336–442) that independently accumulates
~17 CYC points. The remaining CYC budget is contributed by the conditional adoption gate,
nested position scan, and the `stopOrders` iteration with three early-continue guards.
The method is **not** hot-path (cold path: startup/reconnect only), but its structural depth
makes it risky to modify and difficult to unit-test in isolation.

---

## Blast Radius Summary

| Dimension | Detail |
|---|---|
| **Direct callers** | `EnumerateApexAccounts` (line 196, `src/V12_002.SIMA.Lifecycle.cs`); `ProcessOnConnectionStatusUpdate` via `Enqueue` lambda (line 337, `src/V12_002.Lifecycle.cs`) |
| **Caller chain** | `ProcessInitializeSIMA` → `EnumerateApexAccounts` → `HydrateWorkingOrdersFromBroker`; Reconnect path: `OnConnectionStatusChange` → `Enqueue` → `HydrateWorkingOrdersFromBroker` |
| **Callees (direct)** | `AdoptFleetOrders`, `AdoptMasterOrders`, `HydrateFSMsFromWorkingOrders`, `GetTargetDistribution`, `GetStableHash` |
| **State written** | `activePositions` (ConcurrentDictionary), `stopOrders` (read), `_orderAdoptionComplete` (volatile bool), `_positionPassFailedFirstSeen` (via `HydrateFSMsFromWorkingOrders`) |
| **State read** | `Account.Positions` (broker thread — snapshot guard via `.ToArray()`), `stopOrders`, `IsFleetAccount`, `Instrument.FullName` |
| **REAPER gate** | Sets `_orderAdoptionComplete = true` at exit; `V12_002.REAPER.cs` line 164 gates all audit cycles on this flag — **method exit is safety-critical** |
| **Threading constraint** | Must execute on strategy thread (actor-serialized); reconnect path enqueues via `TriggerCustomEvent` |
| **Risk on change** | **High** — incorrect exit (exception swallowed before line 447) leaves `_orderAdoptionComplete = false`, silently disabling REAPER indefinitely |

**Affected symbol count (blast radius):** 8 symbols directly coupled; 3 shared concurrent/volatile state objects.

---

## Top 3 Complexity Drivers

### 1. Inline master-position reconstruction block (lines 359–433, ~14 CYC)

The largest single sub-block: a `foreach` over `Account.Positions.ToArray()` with a 4-clause
`&&` guard (`brokerPos != null`, `.Instrument != null`, `.FullName ==`, `.MarketPosition !=`),
then a second `foreach` over `stopOrders.ToArray()` with three sequential `continue` guards
(`StartsWith("Fleet_")`, `ContainsKey`, `ContainsKey`), followed by a `PositionInfo` struct
init, five `StartsWith`-based trade-DNA flags (`IsMOMOTrade`, `IsTRENDTrade`, `IsRetestTrade`,
`IsRMATrade`, `IsFFMATrade`), and a final `if (pos.IsMOMOTrade)` override. Each boolean guard,
`&&` clause, and `if` branch contributes +1 CYC. This entire block is unextracted inline logic
that already has a natural extraction boundary (`ReconstructMasterPosition(key, masterMP, ...)`)
and is the primary target for Phase 1.

### 2. Dual `!masterIsFleetForOrders993` branches with asymmetric `try/catch` wrapping (lines 316–442, ~6 CYC)

The same `masterIsFleetForOrders993` boolean gates two separate `if (!masterIsFleet...)` blocks:
the first wraps `AdoptMasterOrders()` in a `try/catch`; the second wraps the inline
position-reconstruction block in a separate `try/catch`. Each `if` adds +1, each `catch`
path adds +1. Because the two blocks are structurally identical in their outer guard but
diverge entirely in body, a reader must mentally unify them — the catch in the second block
could also suppress the `_orderAdoptionComplete = true` assignment if an exception propagates
outside the guarded scope. This pattern also makes it impossible to reach line 447 via a test
without satisfying both guards.

### 3. `if (adoptedCount > 0)` terminal branch + call-depth delegation deferring CYC to callees (lines 448–456, +1 CYC; callees: ~13 CYC deferred)

While the terminal `if (adoptedCount > 0)` branch only adds +1 CYC to the method directly,
the method achieves an **apparent** CYC reduction by delegating to `AdoptFleetOrders` (which
itself loops over `Account.All` + calls `AdoptOrdersFromAccount`), `AdoptMasterOrders` (switch
with 6 cases + state guard with 5 OR conditions), and `HydrateFSMsFromWorkingOrders` (entry
pass + position pass, calling `MapOrderStateToFSMState`, `FindLivePosition`,
`HydrateFromOpenPositions`). The reported CYC of 34 includes the inlined complexity of the
master position block; the delegated methods add an additional ~20+ aggregate CYC that unit
testing must cover separately. This delegation structure is correct architecturally but means
the true test-coverage obligation for this subsystem substantially exceeds what the single
CYC=34 number suggests.

---

## Recommended Extraction Count

**2 targeted extractions recommended for Phase 1.**

| # | Extraction | Target Name | Lines | Estimated CYC reduction |
|---|---|---|---|---|
| 1 | Inline master-position reconstruction (stop-key scan + `PositionInfo` init + DNA flags) | `ReconstructMasterPositionFromStop(string key, MarketPosition mp, int qty, double avgPrice, double stopPrice)` | 361–432 | −12 to −14 from `HydrateWorkingOrdersFromBroker` |
| 2 | Dual `!masterIsFleet` guard unification (merge the two `if (!masterIsFleet)` blocks into one scope, extracting inner logic) | `AdoptAndReconstructMasterAccount(ref int adoptedCount)` | 316–442 | −4 structural CYC (removes duplicated guard + two independent catch blocks) |

**Rationale:** After extraction, `HydrateWorkingOrdersFromBroker` becomes a thin 5-step
sequencer (AdoptFleet → AdoptAndReconstructMaster → HydrateFSMs → gate → log) with CYC ≤ 5,
matching the pattern already established by `AdoptFleetOrders` and `CancelAllV12GtcOrders`.
No further extraction into micro-helpers is warranted at this complexity level.

---

## Agent Tracking

Agent Name: v12-phase0-hotspot | Bobcoins Used: 1.0 | Execution Time: ~90s
