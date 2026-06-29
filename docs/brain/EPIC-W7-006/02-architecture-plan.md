# Phase 2: Architecture Plan — EPIC-W7-006

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:10:00Z
**Input:** docs/brain/EPIC-W7-006/01-scope-boundary.md

---

## Method Under Extraction

- **Method:** `HydrateWorkingOrdersFromBroker`
  *(Epic concept: `AdoptFleetWorkingOrders` — canonical runtime name is `HydrateWorkingOrdersFromBroker`)*
- **Source File:** [`src/V12_002.SIMA.Lifecycle.cs`](src/V12_002.SIMA.Lifecycle.cs:309)
- **Original CYC:** 14 (measured; see [00-hotspots.md](docs/brain/EPIC-W7-006/00-hotspots.md))
- **Lines:** 309–457 (149 lines)
- **Signature:** `private void HydrateWorkingOrdersFromBroker()`

### jCodemunch `get_context_bundle` Result

Symbol not indexed under the conceptual name `AdoptFleetWorkingOrders`. Fallback via `search_symbols` confirmed:
- Symbol ID: `src/V12_002.SIMA.Lifecycle.cs::V12_002.HydrateWorkingOrdersFromBroker#method`
- Signature: `private void HydrateWorkingOrdersFromBroker()`
- File: `src/V12_002.SIMA.Lifecycle.cs`, line 309
- Two sibling symbols confirmed in same file: `HydrateFSMsFromWorkingOrders` (line 787), `HydrateExpectedPositionsFromBroker` (line 208)

### jCodemunch `get_call_hierarchy` Result

**Callers (depth=1, 2 total):**
| Caller | File | Line | Depth |
|---|---|---|---|
| `EnumerateApexAccounts` | `src/V12_002.SIMA.Lifecycle.cs` | 140 | 1 |
| `ProcessInitializeSIMA` | `src/V12_002.SIMA.Lifecycle.cs` | 90 | 2 |

**Direct Callees (depth=1, selected):**
| Callee | File | Line |
|---|---|---|
| `AdoptFleetOrders` | `src/V12_002.SIMA.Lifecycle.cs` | 903 |
| `AdoptMasterOrders` | `src/V12_002.SIMA.Lifecycle.cs` | 1195 |
| `HydrateFSMsFromWorkingOrders` | `src/V12_002.SIMA.Lifecycle.cs` | 787 |
| `IsFleetAccount` | `src/V12_002.cs` | 864 |
| `stopOrders`, `activePositions` | `src/V12_002.cs` | 201/199 |

Both upstream callers use void/zero-argument invocations — no signature risk.

### jCodemunch `get_dependency_graph` Result

- **File node count:** 1 — `src/V12_002.SIMA.Lifecycle.cs`
- **Import edges:** 0 (no resolvable static imports — all references are within the partial class)
- **Importers:** 0 at file level (partial class pattern; coupling is via shared state fields)
- **Conclusion:** File is a self-contained partial class; all dependencies are intra-repo via instance fields. Extraction of private helpers stays within the same file with zero cross-file dependency changes.

### jCodemunch `get_extraction_candidates` Result

- **Candidates returned:** 0
- **Reason:** Index was built without complexity metadata (requires re-index with jCodemunch >= 1.16). Complexity data sourced from manual branch-counting in hotspots.md (CYC=14 for `HydrateWorkingOrdersFromBroker`).
- **Action:** Proceeding with hotspots.md as authoritative CYC baseline.

---

## Sequential Thinking Summary

**5-thought chain executed. Final verdict (Thought 5):**

Architecture plan confirmed for EPIC-W7-006. Target: `HydrateWorkingOrdersFromBroker` (CYC=14 → projected CYC=5). Two private helper methods extracted:

1. **`RebuildMasterFilledPosition()`** — pure `PositionInfo` factory for master-filled positions, mirroring the existing `RebuildFleetPositionFromEntry()` pattern. Takes entry parameters, assigns all 6 trade-DNA flags (`IsMOMOTrade`, `IsRMATrade`, `IsTRENDTrade`, `IsRetestTrade`, `IsFFMATrade`, override), returns `PositionInfo`. Projected CYC=5.

2. **`HydrateMasterFilledPositions()`** — isolated master-path try-block orchestrator that calls `RebuildMasterFilledPosition()` and delegates to `AdoptMasterOrders()`. Absorbs the 108-line inline master-position reconstruction block from the parent. Projected CYC=6.

Parent after extraction retains: `if(!master)` guard + fleet path loop + stop-order loop + `adoptedCount` check = CYC=5.

All values ≤ 8. extraction_count=2, max_cyc_projected=6. Plan approved.

---

## Extraction Plan

| Helper Method Name | Responsibility | Projected CYC | Signature |
|---|---|---|---|
| `RebuildMasterFilledPosition` | Pure factory: constructs `PositionInfo` for a master-filled position with all 6 trade-DNA flags assigned. Mirrors existing `RebuildFleetPositionFromEntry()`. | **5** | `private PositionInfo RebuildMasterFilledPosition(string instrument, double entryPrice, int qty, bool isMomoCandidate, bool isTrend, bool isMnl)` |
| `HydrateMasterFilledPositions` | Isolated master-path orchestrator: iterates broker positions for master accounts, calls `RebuildMasterFilledPosition()` for filled positions, calls `AdoptMasterOrders()`. Contains the entire try-block previously inline in parent (lines 334–442). | **6** | `private void HydrateMasterFilledPositions()` |

### Extraction Detail — `RebuildMasterFilledPosition`

**Source block:** Lines ~388–420 (inline within `HydrateWorkingOrdersFromBroker`)

**Decision points moved to helper:**
- `if(masterMP != Flat)` — position direction guard [1]
- `IsMOMO` flag compound condition [1]
- `trendMnl` compound condition (2 connectors) [2]
- `IsRMA` / `IsFFMA` flag conditions [1]
**Projected CYC: 5**

**Return type:** `PositionInfo` (same struct used by `RebuildFleetPositionFromEntry`)

**Jane Street pattern:** Named helper, single responsibility (construct one object), pure function (no side effects on shared state), CYC ≤ 8.

---

### Extraction Detail — `HydrateMasterFilledPositions`

**Source block:** Lines ~334–442 (master-path try block in `HydrateWorkingOrdersFromBroker`)

**Decision points retained in helper:**
- `try/catch` [1]
- `foreach(brokerPos)` [1]
- `4-cond compound if` for master-position check [2]
- `if(masterMP != Flat)` delegate to `RebuildMasterFilledPosition()` [1]
- `if(adoptedCount > 0)` partial guard [1]
**Projected CYC: 6**

**Jane Street pattern:** Single responsibility (adopt master positions only), encapsulates try/catch away from parent orchestrator, calls extracted pure helper for PositionInfo construction.

---

## Parent Method After Extraction

**`HydrateWorkingOrdersFromBroker` remaining logic after extraction:**

```
1. if (!master) early-return guard              [1 branch]
2. call HydrateMasterFilledPositions()          [0 branches — delegated]
3. call AdoptFleetOrders()                      [0 branches — delegated]
4. foreach(stopKvp) stop-order loop             [1 branch]
5. if (Fleet_ skip) filter                      [1 branch]
6. if (ContainsKey) dict guard                  [1 branch]
7. if (adoptedCount > 0) completion gate        [1 branch]
8. call HydrateFSMsFromWorkingOrders()          [0 branches — delegated]
```

- **Projected CYC: 5** (5 remaining decision points ≤ 8)
- **Remaining lines:** ~35 lines (down from 149)
- **Callers unchanged:** `EnumerateApexAccounts` (line 140), `ProcessInitializeSIMA` (line 90)

---

## max_cyc_projected: 6
## extraction_count: 2

---

## xUnit Test Requirements

| Test Method | Helper Tested | Assertion |
|---|---|---|
| `TestRebuildMasterFilledPosition_SetsAllTradeDNAFlags` | `RebuildMasterFilledPosition` | All 6 trade-DNA flags correctly assigned for given input permutations |
| `TestRebuildMasterFilledPosition_FlatPositionHandled` | `RebuildMasterFilledPosition` | Flat masterMP condition returns default PositionInfo without crash |
| `TestHydrateMasterFilledPositions_SkipsNonMasterAccounts` | `HydrateMasterFilledPositions` | Non-master accounts produce zero adoptions |

---

## Jane Street Alignment

| Rule | Status | Notes |
|---|---|---|
| CYC <= 8 achieved | **YES** | Parent=5, RebuildMasterFilledPosition=5, HydrateMasterFilledPositions=6 |
| Single-responsibility per helper | **YES** | `RebuildMasterFilledPosition` = build PositionInfo only; `HydrateMasterFilledPositions` = adopt master positions only |
| Lock-free / Actor pattern preserved | **YES** | No `lock()` blocks introduced; `ConcurrentDictionary` retained as-is; caller uses `Enqueue` pattern |
| Illegal states unrepresentable | **YES** | Master path guard `if(!master)` is early return; `PositionInfo` construction encapsulated in typed factory; no raw struct mutation exposed |
| Extract Guard Clauses applied | **YES** | `if(!master)` becomes top-level early return in parent |
| Named helpers with exact one concern | **YES** | Two helpers, each with documented single responsibility |
| ASCII-only strings | **YES** | No Unicode, no curly quotes introduced |
| xUnit [Fact] tests required | **YES** | 3 test methods specified above |
| ONE method per epic | **YES** | Only `HydrateWorkingOrdersFromBroker` body is modified |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 1.5 |
| **Execution Time** | 2026-06-29T01:10:00Z |
| **Wave** | 7 |
| **Phase** | 2 (Architecture Planning) |
| **Epic** | EPIC-W7-006 |
| **jCodemunch tools called** | `resolve_repo`, `get_context_bundle` (fallback: `search_symbols`), `get_call_hierarchy`, `get_dependency_graph`, `get_extraction_candidates` |
| **sequential-thinking calls** | 5 |
| **extraction_count** | 2 |
| **max_cyc_projected** | 6 |
| **Source** | `src/V12_002.SIMA.Lifecycle.cs` (lines 309–457) |
| **Output** | `docs/brain/EPIC-W7-006/02-architecture-plan.md` |
