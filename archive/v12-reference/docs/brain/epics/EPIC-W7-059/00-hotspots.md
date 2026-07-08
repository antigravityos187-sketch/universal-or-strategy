# EPIC-W7-059 Hotspot Analysis

**Method:** AdoptMasterWorkingOrders (logical cluster — see Name Resolution below)
**CYC Confirmed:** 34
**File:** src/V12_002.SIMA.Lifecycle.cs

---

## Name Resolution

The backlog ticket names the target `AdoptMasterWorkingOrders` (CYC=34). No symbol by that exact
name exists in the codebase. The live equivalent is the **`HydrateWorkingOrdersFromBroker`**
orchestrator (lines 309–457) together with `AdoptMasterOrders` (lines 1195–1254) — this two-body
cluster represents the logical "AdoptMasterWorkingOrders" scope that totals CYC=34 across the
full adoption pipeline. Phase 1 planning must scope to this entire cluster, not just the leaf method.

---

## Overview

`HydrateWorkingOrdersFromBroker` is the cold-path (startup + reconnect) orchestrator that re-seeds
all V12 tracking dictionaries from live broker state after a strategy restart or connection loss.
It calls `AdoptFleetOrders` (fleet accounts), `AdoptMasterOrders` (master account), a 148-line
inline master-position reconstruction block, and `HydrateFSMsFromWorkingOrders` (FSM rebuild).
The method terminates by setting `_orderAdoptionComplete = true`, the critical gate that re-enables
REAPER auditing. Its aggregate Cyclomatic Complexity of 34 makes it the highest-complexity method
cluster in the SIMA Lifecycle subsystem.

---

## Blast Radius Summary

| Dimension | Detail |
|---|---|
| **Trigger sites** | `EnumerateApexAccounts` (line 196, startup path); `ProcessOnConnectionStatusUpdate` → `Enqueue(HydrateWorkingOrdersFromBroker)` (line 337, reconnect path) |
| **REAPER gate** | `_orderAdoptionComplete` flag — set at line 447; read by `OnReaperTimerElapsed` (REAPER.cs:164); any refactor that delays or mis-routes this flag silently suppresses the audit watchdog |
| **Downstream FSM rebuild** | `HydrateFSMsFromWorkingOrders` → `HydrateFromOpenPositions` (Phase 5 position pass); FSM population depends on tracking dicts being fully populated first |
| **Tracking dicts mutated** | `stopOrders`, `target1Orders`–`target5Orders`, `entryOrders`, `activePositions` (all `ConcurrentDictionary<string, Order/PositionInfo>`) |
| **Pure helpers called** | `ClassifyOrderByPrefix` (8-way prefix switch), `RouteOrderToTargetDict` (6-way switch + key extraction), `IsValidOrderState` (5-way OR), `RebuildFleetPositionFromEntry` (pure factory) |
| **State mutated on master** | `activePositions` (master position reconstruction block, lines 340–442); `_orderAdoptionComplete` (bool flag) |
| **Threading constraint** | Actor-serialized (strategy thread only); `AdoptMasterOrders` XML doc explicitly states "ACTOR-SERIALIZED: Must be called on strategy thread" |
| **Build tags** | Build 948 (adoption), Build 993 (master extension), Build 994 (Unknown state guard), Build 1108.003 (master activePositions reconstruction) |
| **Risk on change** | HIGH — adoption order is load-bearing: fleet first, master second, FSM hydration third, gate last; any permutation or early-return bug leaves REAPER blind |

**Affected symbol count (blast radius):** 11 direct symbols; 7 shared mutable dictionaries; 1 critical boolean gate.

---

## Top 3 Complexity Drivers

1. **Inline master-position reconstruction block (lines 340–442, ~12 CYC)**
   A 100-line embedded block inside `HydrateWorkingOrdersFromBroker` that iterates
   `Account.Positions.ToArray()`, applies a 4-level null-guard chain to find the live master
   position, then iterates `stopOrders` a second time to match stop keys to the position.
   The body constructs a `PositionInfo` struct and applies 5 trade-DNA boolean flags
   (`IsMOMOTrade`, `IsTRENDTrade`, `IsRetestTrade`, `IsRMATrade`, `IsFFMATrade`) each with
   their own `StartsWith` guard. Three nested try/catch blocks wrap the three sub-phases.
   This block should be extracted as `ReconstructMasterActivePosition()`.

2. **`AdoptMasterOrders` state-guard and classification fan-out (~10 CYC)**
   The method applies a 6-condition OR guard (`Working || Accepted || Submitted || ChangePending ||
   ChangeSubmitted || Unknown`) followed by `ClassifyOrderByPrefix` (8-way chain) and a 6-case
   `switch` routing to the appropriate dictionary. Each OR branch and each switch case contributes
   +1 CYC. The `Unknown` state variant (Build 994 NT8 Sim workaround) diverges from the
   `IsValidOrderState` helper used by fleet adoption — a silent inconsistency that raises regression
   risk if the helper is reused naively.

3. **Dual-account asymmetry and conditional execution pattern (~8 CYC)**
   `HydrateWorkingOrdersFromBroker` calls `AdoptFleetOrders` unconditionally, then guards
   `AdoptMasterOrders` and the inline reconstruction block behind `!masterIsFleetForOrders993`
   (two separate boolean captures for the same predicate). The reconnect caller
   (`ProcessOnConnectionStatusUpdate`) calls `HydrateWorkingOrdersFromBroker` directly,
   bypassing `EnumerateApexAccounts`, so the method must be fully self-sufficient — adding
   an implicit "must not have precondition on enumeration" constraint that is not enforced
   by the type system.

---

## Recommended Extraction Plan (Phase 1 Preview)

| Extraction | Target Method | Est. CYC Reduction |
|---|---|---|
| Master position reconstruction block (lines 340–442) | `ReconstructMasterActivePosition()` | −10 |
| Master order state guard (6-condition OR) | fold into or align with `IsValidOrderState` + `Unknown` overload | −3 |
| Position-to-stop matching loop (lines 361–434) | `TryMatchStopKeyForMasterPosition()` | −4 |

**Estimated post-refactor CYC for `HydrateWorkingOrdersFromBroker`:** ≤8 (down from 22)
**Estimated post-refactor CYC for `AdoptMasterOrders`:** ≤7 (down from 12)
**Cluster total after extraction:** ≤15 (target: <15 per method, <10 per helper)

---

## Key Risks

- **REAPER blind-window**: If any extraction introduces an early-return path before
  `_orderAdoptionComplete = true`, the REAPER watchdog will never resume. Gate assignment
  must be unconditional (finally-block pattern is strongly recommended).
- **`Unknown` state divergence**: `AdoptMasterOrders` accepts `OrderState.Unknown` (Build 994)
  but the shared `IsValidOrderState` helper does not. Unifying them without the Unknown case
  will silently drop previous-session sim orders on master.
- **Reconnect vs startup paths**: Both paths call into this cluster but only the startup path
  runs full `EnumerateApexAccounts`. Extractions must remain callable standalone.

---

## Agent Tracking

Agent Name: bob-hotspot-w7-059 | Bobcoins Used: 1.0 | Execution Time: ~90s
