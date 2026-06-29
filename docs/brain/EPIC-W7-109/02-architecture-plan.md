# Phase 2: Architecture Plan — EPIC-W7-109

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:10:00Z
**Input:** docs/brain/EPIC-W7-109/01-scope-boundary.md

---

## Method Under Extraction

- **Method:** `HydrateWorkingOrdersFromBroker`
- **Source File:** `src/V12_002.SIMA.Lifecycle.cs`
- **Original CYC:** 34
- **Lines:** 309–457
- **Signature:** `private void HydrateWorkingOrdersFromBroker()`

### jcodemunch get_context_bundle result

Full source retrieved via jcodemunch get_context_bundle for symbol ID `src/V12_002.SIMA.Lifecycle.cs::V12_002.HydrateWorkingOrdersFromBroker#method`. The method (lines 309–457) is a `private void` with no parameters, actor-serialized on the strategy thread. Docstring confirms: "MUST be called on the strategy thread (via TriggerCustomEvent when initiated from a callback)." Three structural sections identified: (1) AdoptFleetOrders invocation; (2) `masterIsFleetForOrders993`-gated try/catch for `AdoptMasterOrders`; (3) second identical `masterIsFleetForOrders993` gate with inline master position reconstruction block (god-block spanning lines 336–443, including foreach over `Account.Positions.ToArray()`, foreach over `stopOrders`, PositionInfo struct init, 5 trade DNA flags, activePositions write); (4) `HydrateFSMsFromWorkingOrders` call + `_orderAdoptionComplete = true` assignment + terminal log. Source freshness: `fresh`.

### jcodemunch get_call_hierarchy result

jcodemunch get_call_hierarchy (depth=2, direction=both) resolved 2 callers and 41 callee references. **Callers (depth 1):** `EnumerateApexAccounts` (line 140, same file). **Callers (depth 2):** `ProcessInitializeSIMA` (line 90, same file). **Direct callees (depth 1, resolved):** `AdoptFleetOrders` (line 903), `IsFleetAccount` (resolved in `V12_002.cs`), `AdoptMasterOrders` (line 1195), `GetTargetDistribution` (resolved in `V12_002.PureLogic.cs`), `GetStableHash` (resolved in `V12_002.DrawingHelpers.cs`), `HydrateFSMsFromWorkingOrders` (line 787). **Depth-2 callees (via AdoptFleetOrders):** `AdoptOrdersFromAccount`, `ClassifyOrderByPrefix`. **Depth-2 callees (via HydrateFSMsFromWorkingOrders):** `MapOrderStateToFSMState`, `FindLivePosition`, `ResolveRemainingContracts`, `BuildFSM`, `LinkTargetOrderToFSM`, `RegisterFSM`, `HydrateFromOpenPositions`. State touched: `stopOrders`, `activePositions`, `entryOrders`, `_followerBrackets` (all ConcurrentDictionary fields). Method signature is unchanged by this extraction — all 2 callers remain unaffected.

### jcodemunch get_dependency_graph result

jcodemunch get_dependency_graph (direction=both, depth=1) for `src/V12_002.SIMA.Lifecycle.cs` returned node_count=1, edge_count=0, imports=[], importers=[]. The C# partial class model means file-level import edges are not resolvable via the import graph (all partial class files compile into one assembly). No cross-file import edges to protect; extraction is confined to the same partial class. Blast radius: `src/V12_002.SIMA.Lifecycle.cs` only.

### jcodemunch get_extraction_candidates result

jcodemunch get_extraction_candidates for `src/V12_002.SIMA.Lifecycle.cs` (min_complexity=3, min_callers=1) returned candidates=[]. The empty result is consistent with the partial-class compilation model — the index cannot resolve callers across partial class boundaries via import graph edges. The get_context_bundle and get_call_hierarchy analysis provides sufficient structural evidence for manual extraction planning.

---

## Sequential Thinking Summary

sequentialthinking chain completed (5 thoughts): Thought 1 established the god-function structure from get_context_bundle and call hierarchy (2 callers, 6 direct callees, inline master position reconstruction block as primary CYC contributor). Thought 2 designed the 5-helper decomposition with clear extraction boundaries aligned to natural code sections. Thought 3 validated precise method signatures and projected CYC for each helper (max = 7 for ApplyTradeDnaFlags). Thought 4 verified all Jane Street rules: CYC <= 8 per helper, single-responsibility, actor model preservation, illegal-state elimination via out-param bool return, zero-allocation on struct return, guard clause extraction, Extract Loop Body pattern. Thought 5 delivered final verdict: extraction_count=5, max_cyc_projected=7, parent_cyc=5. ALL rules PASS.

---

## Extraction Plan

| # | Helper Method Name | Responsibility | Projected CYC |
|---|---|---|---|
| 1 | `TryGetMasterBrokerPosition(out MarketPosition masterMP, out int masterQty, out double masterAvgPrice)` | Read-only snapshot of `Account.Positions` to find the matching instrument position. Returns `bool` — makes "position found" state explicit, eliminating `MarketPosition.Flat` sentinel reliance. | 4 |
| 2 | `IsMasterStopKeyEligible(string key)` | Guard predicate: returns `false` if key starts with `"Fleet_"` or if `activePositions` already contains the key. Encapsulates both `continue` guards from the stop-key loop into a single named check. | 2 |
| 3 | `BuildMasterPositionInfo(string key, MarketPosition direction, int qty, double avgPrice, double stopPrice)` | Constructs and returns a `PositionInfo` struct from the provided parameters. Delegates to `GetTargetDistribution` for target-quantity split. Pure struct construction, no branching. | 3 |
| 4 | `ApplyTradeDnaFlags(ref PositionInfo pos, string key)` | Classifies a position by trade DNA: sets `IsMOMOTrade`, `IsTRENDTrade`, `IsRetestTrade`, `IsRMATrade`, `IsFFMATrade` using `StartsWith` prefix checks, then applies the MOMO override (`if IsMOMOTrade → IsRMATrade = false`). Single-responsibility: DNA classification only. | 7 |
| 5 | `ReconstructMasterActivePositions()` | Orchestrates the master position reconstruction loop: calls `TryGetMasterBrokerPosition`, guards on non-flat position, iterates `stopOrders`, calls `IsMasterStopKeyEligible`, delegates to `BuildMasterPositionInfo` + `ApplyTradeDnaFlags`, writes to `activePositions`, logs each reconstruction. Replaces the inline god-block (lines 336–443). | 4 |

---

## Parent Method After Extraction

**Remaining logic in `HydrateWorkingOrdersFromBroker` after extraction:**

```
1. adoptedCount = AdoptFleetOrders()
2. if (!masterIsFleetForOrders993)
     try { adoptedCount += AdoptMasterOrders() }
     catch { Print warning }
3. if (!masterIsFleetForOrders993)
     try { ReconstructMasterActivePositions() }
     catch { Print warning }
4. HydrateFSMsFromWorkingOrders()
5. _orderAdoptionComplete = true
6. if (adoptedCount > 0) Print adopted log
   else Print no-orders log
```

- **Projected CYC:** 5
  - 1 (base) + 1 (`if !masterIsFleet` adopt gate) + 1 (catch branch) + 1 (`if !masterIsFleet` reconstruct gate) + 1 (`if adoptedCount > 0`) = 5
- **Safety invariant preserved:** `_orderAdoptionComplete = true` is unconditionally reached after all try/catch blocks, matching the original safety guarantee documented in `V12_002.REAPER.cs` gate dependency.
- **Signature unchanged:** `private void HydrateWorkingOrdersFromBroker()` — all 2 callers unaffected.

---

## max_cyc_projected: 7
## extraction_count: 5

---

## Jane Street Alignment

| Rule | Status |
|---|---|
| CYC <= 8 achieved (all helpers AND parent) | YES — max = 7, parent = 5 |
| Single-responsibility per helper | YES — each helper does exactly one thing |
| Lock-free/Actor pattern preserved | YES — no lock() introduced; actor-serialized context maintained |
| Illegal states unrepresentable | YES — `TryGetMasterBrokerPosition` uses bool+out instead of sentinel `MarketPosition.Flat` |
| Zero-allocation hot paths | YES — struct return for `BuildMasterPositionInfo`, `ref` param for `ApplyTradeDnaFlags`, cold path only |
| Guard clause extraction | YES — `IsMasterStopKeyEligible` encapsulates dual continue guards |
| Extract Loop Body pattern | YES — `BuildMasterPositionInfo` + `ApplyTradeDnaFlags` form the loop body processor |
| No scope creep (V12.23) | YES — all helpers are `private`, same partial class, no signature changes |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 4 |
| **Execution Time** | 2026-06-29T01:10:00Z |
| **Wave** | 7 |
| **Phase** | 2 |
| **jcodemunch tools called** | get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **extraction_count** | 5 |
| **max_cyc_projected** | 7 |
| **parent_cyc_projected** | 5 |
