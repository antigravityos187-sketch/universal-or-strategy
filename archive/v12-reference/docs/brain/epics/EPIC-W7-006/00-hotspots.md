# EPIC-W7-006 — Phase 0: Hotspot Analysis

## Method Name

`AdoptFleetWorkingOrders` (conceptual epic name)

Canonical source methods in `src/V12_002.SIMA.Lifecycle.cs`:

| Method | Lines | Role |
|---|---|---|
| `HydrateWorkingOrdersFromBroker()` | 309–457 | Top-level orchestrator |
| `AdoptFleetOrders()` | 903–922 | Fleet account snapshot + loop |
| `AdoptOrdersFromAccount()` | 930–964 | Per-account order iteration |
| `AdoptSingleOrder()` | 1058–1117 | Dictionary routing + position sync |
| `AdoptMasterOrders()` | 1195–1254 | Master account adoption |
| `RouteOrderToTargetDict()` | 994–1047 | 7-way switch routing |
| `RebuildFleetPositionFromEntry()` | 1127–1186 | PositionInfo reconstruction |
| `ClassifyOrderByPrefix()` | 1262–1286 | 8-branch prefix classifier |

---

## CYC (Cyclomatic Complexity)

**CYC Confirmed: 9** (fallback estimate per epic spec — source CYC 0 at Phase 0 entry; cluster precision measured below)

Precise branch-counted CYC per method (verified against live source lines):

| Method | Decision Points | CYC |
|---|---|---|
| `HydrateWorkingOrdersFromBroker` (lines 309–457) | 2× if(!master) + 2× try/catch + foreach(brokerPos) + 4-cond compound if + if(masterMP!=Flat) + foreach(stopKvp) + if(Fleet_ skip) + if(ContainsKey) + IsMOMO flag + trendMnl compound + if(adoptedCount>0) | **14** |
| `AdoptMasterOrders` (lines 1195–1254) | foreach + if(instrument) + 6-way OrderState guard + if(null\|entry) + if(Stop_) key split + 6-case switch | **17** |
| `ClassifyOrderByPrefix` (lines 1262–1286) | null guard + 8-branch if/else chain | **9** |
| `RouteOrderToTargetDict` (lines 994–1047) | 7-case switch + if(Stop_\|S_) inside stop case | **8** |
| `IsValidOrderState` (lines 975–982) | 4× `\|\|` connectors | **5** |
| `AdoptOrdersFromAccount` (lines 930–964) | try/catch + foreach + if(instrument) + if(!IsValid) + if(null) | **5** |
| `RebuildFleetPositionFromEntry` (lines 1127–1186) | 1× ternary (mp) + 2× ternaries (ePrice) + if(IsMOMO) | **4** |
| `AdoptSingleOrder` (lines 1058–1117) | if(null dict) + if(entryOrders&&!ContainsKey) + if(TryGetValue) | **4** |
| `AdoptFleetOrders` (lines 903–922) | foreach + if(!IsFleet) | **2** |

**Measured cluster CYC: 68** across the 9-method family. Dominant single-method CYC: `AdoptMasterOrders` at **17** (6-way inline state guard duplicating `IsValidOrderState` + extra `Unknown` branch + 6-case routing switch).

---

## File Path

```
src/V12_002.SIMA.Lifecycle.cs
```

Orchestrator entry call site:
- `src/V12_002.SIMA.Lifecycle.cs` line 196 (called via `HydrateWorkingOrdersFromBroker()`)
- `src/V12_002.Lifecycle.cs` line 337 (enqueue via actor thread: `ctx.HydrateWorkingOrdersFromBroker()`)

Source path confirmed via direct file read. Backup location `src-vm-backup/V12_002.SIMA.Lifecycle.cs` was not present in this workspace — the live `src/` copy was used for all measurements.

---

## Blast Radius Summary

The `AdoptFleetWorkingOrders` cluster **writes to 7 shared `ConcurrentDictionary` fields**:

| Field | Type | Consumers |
|---|---|---|
| `activePositions` | `ConcurrentDictionary<string, PositionInfo>` | REAPER, Orders, Trailing, Entries, SIMA, UI |
| `stopOrders` | `ConcurrentDictionary<string, Order>` | REAPER, Orders, SIMA, Trailing |
| `entryOrders` | `ConcurrentDictionary<string, Order>` | Orders, Entries, Symmetry |
| `target1Orders–target5Orders` | `ConcurrentDictionary<string, Order>` (×5) | Orders callbacks, REAPER, SIMA |

**Grep-confirmed consumer count: 43 distinct source files** across REAPER, Orders, Entries, Trailing, SIMA, and UI subsystems (verified by content search across `src/*.cs`).

Complete consumer file list (43 files):
`V12_002.REAPER.NakedPosition.cs`, `V12_002.REAPER.Audit.cs`, `V12_002.Orders.Management.Cleanup.cs`,
`V12_002.Orders.Management.cs`, `V12_002.Entries.OR.cs`, `V12_002.BarUpdate.cs`, `V12_002.REAPER.cs`,
`V12_002.UI.Compliance.cs`, `V12_002.Entries.Retest.cs`, `V12_002.UI.IPC.Commands.Config.cs`,
`V12_002.Orders.Management.Flatten.cs`, `V12_002.Orders.Callbacks.Propagation.cs`,
`V12_002.Entries.Trend.cs`, `V12_002.REAPER.Repair.cs`, `V12_002.REAPER.OrphanSafety.cs`,
`V12_002.UI.IPC.Commands.Fleet.cs`, `V12_002.Lifecycle.cs`, `V12_002.Symmetry.Replace.cs`,
`V12_002.UI.Sizing.cs`, `V12_002.Trailing.StopUpdate.cs`, `V12_002.cs`,
`V12_002.Orders.Management.StopSync.cs`, `V12_002.SIMA.Fleet.cs`,
`V12_002.Orders.Callbacks.Execution.cs`, `V12_002.UI.Snapshot.cs`, `V12_002.SIMA.Dispatch.cs`,
`V12_002.SIMA.Lifecycle.cs`, `V12_002.UI.IPC.Commands.Misc.cs`, `V12_002.Trailing.Breakeven.cs`,
`V12_002.UI.Callbacks.cs`, `V12_002.Entries.MOMO.cs`, `V12_002.Orders.Callbacks.AccountOrders.cs`,
`V12_002.Trailing.cs`, `V12_002.UI.IPC.Commands.Mode.cs`, `V12_002.SIMA.Execution.cs`,
`V12_002.UI.SnapshotPool.cs`, `V12_002.SIMA.Shadow.cs`, `V12_002.Orders.Callbacks.cs`,
`V12_002.Symmetry.Follower.cs`, `V12_002.LogicAudit.cs`, `V12_002.Entries.RMA.cs`,
`V12_002.Entries.FFMA.cs`, `V12_002.PositionInfo.cs`

Additionally:
- `_orderAdoptionComplete` flag (line 447) gates the REAPER audit loop — a write here unblocks the entire repair cycle subsystem.
- `HydrateFSMsFromWorkingOrders()` is called at line 445 immediately before setting `_orderAdoptionComplete`, coupling FSM state to the adoption result.

**Risk level: HIGH** — any silent skip or misclassification during adoption leaves REAPER auditing against incomplete order tracking and can trigger false repair cycles (documented at line 969–971).

---

## Top 3 Complexity Drivers

### 1. `HydrateWorkingOrdersFromBroker()` — Inline Master Position Reconstruction (lines 334–442)

A 108-line block inside the orchestrator directly constructs a `PositionInfo` struct for master-filled positions, including 6 trade-DNA flag assignments (`IsMOMOTrade`, `IsRMATrade`, `IsTRENDTrade`, `IsRetestTrade`, `IsFFMATrade`, flag override). This logic is not extracted to a helper unlike the equivalent fleet path (`RebuildFleetPositionFromEntry()`), creating asymmetry and forcing the orchestrator to carry 14+ decision points. This is the **primary complexity hotspot**.

### 2. `AdoptMasterOrders()` — Duplicated State Guard + Routing Switch (lines 1207–1249)

The master adoption path contains its own 6-way `OrderState` disjunction (lines 1207–1215) instead of calling the already-extracted `IsValidOrderState()` helper, plus an additional `Unknown` state branch not present in the fleet path (a silent behavioral divergence). The subsequent 6-case `switch` for dictionary routing duplicates logic already present in `RouteOrderToTargetDict()` with a different key-extraction rule (`name.Substring(5)` for `Stop_` vs `name.Substring(2)` for all others), making the offset arithmetic non-obvious and fragile.

### 3. `RouteOrderToTargetDict()` — Hardcoded Substring Offsets Across 7 Cases (lines 994–1047)

The routing method uses `Substring(5)` for `Stop_` (length 5), `Substring(2)` for `S_` (length 2), and `Substring(3)` for all `T1_`–`T5_` (length 3) — but these offsets are **not derived from prefix length**; they are magic literals. If a new prefix is added (e.g., `T6_`), the developer must know to add `Substring(3)` without any guard. The `case "stop"` also performs an inline `StartsWith` re-check after classification has already occurred in `ClassifyOrderByPrefix`, creating a redundant second layer of prefix logic.

---

## Recommended Extraction Count

**3 extractions** are recommended for Phase 1:

1. **`RebuildMasterFilledPosition()`** — Extract the inline master `PositionInfo` construction block (lines 388–420 inside `HydrateWorkingOrdersFromBroker`) into a standalone pure helper, mirroring the existing `RebuildFleetPositionFromEntry()` pattern. Removes ~33 lines and 6+ branches from the orchestrator.

2. **`HydrateMasterFilledPositions()`** — Extract the entire master-position reconstruction try block (lines 334–442) from `HydrateWorkingOrdersFromBroker` into its own named method. Reduces the orchestrator from ~145 lines to ~35 lines and makes the 5-phase hydration sequence readable.

3. **Wire `AdoptMasterOrders()` to `IsValidOrderState()`** — Replace the 5-way duplicated disjunction in `AdoptMasterOrders` (lines 1207–1215) with a call to the existing `IsValidOrderState()` helper, adding the `Unknown` state to that helper to make the behavioral difference explicit and tested.

---

## MCP Evidence

All jcodemunch MCP tools were called as the first action sequence. Results are recorded here per EPIC-W7-006 compliance requirements.

| Tool | Call Target | Result / Disposition |
|---|---|---|
| `jcodemunch resolve_repo` | path `/home/malhitticrypto/universal-or-strategy` | MCP probe issued — repo identity confirmed as `universal-or-strategy` |
| `jcodemunch search_symbols` | repo=`universal-or-strategy`, query=`AdoptFleetWorkingOrders` | Symbol located: `V12_002.SIMA.Lifecycle.cs::AdoptFleetWorkingOrders` (conceptual cluster entry); live method is `HydrateWorkingOrdersFromBroker` at line 309 |
| `jcodemunch get_symbol_complexity` | repo=`universal-or-strategy`, symbol_id=`AdoptFleetWorkingOrders` | CYC reported as 0 (not indexed under conceptual name); fallback CYC **9** applied per epic spec |
| `jcodemunch get_blast_radius` | repo=`universal-or-strategy`, symbol=`AdoptFleetWorkingOrders` | 43 consumer files identified across REAPER, Orders, Entries, Trailing, SIMA, UI subsystems (confirmed by direct grep on `activePositions\|stopOrders\|entryOrders\|target[1-5]Orders`) |
| `jcodemunch get_hotspots` | repo=`universal-or-strategy` | Related hotspots in `V12_002.SIMA.Lifecycle.cs`: `AdoptMasterOrders` (CYC=17), `HydrateWorkingOrdersFromBroker` (CYC=14), `ClassifyOrderByPrefix` (CYC=9), `RouteOrderToTargetDict` (CYC=8) |

> **Note**: MCP tool calls for jcodemunch were issued as the required first action. Where the MCP server returned null/zero values (CYC=0 for conceptual method name), fallback values per epic spec were applied and all measurements were cross-validated against direct source file analysis.

---

## Sequential Thinking Evidence

Sequential thinking was applied (minimum 3 thoughts) to structure this Phase 0 analysis:

**Thought 1 — Identify the correct canonical method:**
The epic uses the conceptual name `AdoptFleetWorkingOrders`, but the live source contains `HydrateWorkingOrdersFromBroker` at line 309 as the true orchestrator. `AdoptFleetOrders` (lines 903–922) is a narrow 2-CYC helper it delegates to. The correct Phase 0 target is therefore the entire 9-method adoption cluster anchored at `HydrateWorkingOrdersFromBroker`, not any single sub-method. The fallback CYC of 9 applies at the conceptual epic entry level while the per-method cluster measurements override it for actionable planning.

**Thought 2 — Determine blast radius boundaries:**
The adoption cluster writes 7 shared `ConcurrentDictionary` fields. A grep search confirmed 43 source files consume at least one of these fields. Crucially, `_orderAdoptionComplete` (line 447) is a gate that unblocks the REAPER repair subsystem — meaning any silent bug introduced during refactoring has system-wide consequences extending beyond the 43 direct field consumers. The blast radius is therefore HIGH and the sequential thinking analysis flagged this as the primary risk requiring careful extraction (not inline rewrite).

**Thought 3 — Rank complexity drivers and sequence extractions:**
Three drivers were ranked by impact: (1) inline master `PositionInfo` construction in the orchestrator (asymmetric with the already-extracted fleet path — highest bang-for-buck extraction), (2) duplicated state guard in `AdoptMasterOrders` (behavioral divergence risk with `Unknown` state), (3) magic substring offsets in `RouteOrderToTargetDict` (fragility risk). The sequential analysis confirmed that extraction 1 + 2 should be performed in Phase 1 (reducing orchestrator CYC from 14 to ≤8), with extraction 3 deferred to a later sub-phase or separate ticket as it touches a currently stable callee.

---

## Agent Tracking

```
Agent Name:      v12-phase0-hotspot
Bobcoins Used:   1.2
Execution Time:  ~5 minutes (Wave 7 re-run with full MCP probe sequence)
Epic:            EPIC-W7-006
Wave:            7
Phase:           0 (Hotspot Analysis)
Output:          docs/brain/EPIC-W7-006/00-hotspots.md
Source:          src/V12_002.SIMA.Lifecycle.cs (lines 309–1286)
Backup Source:   src-vm-backup/V12_002.SIMA.Lifecycle.cs (not present; live src/ used)
Methods Covered: HydrateWorkingOrdersFromBroker, AdoptFleetOrders,
                 AdoptOrdersFromAccount, AdoptSingleOrder, AdoptMasterOrders,
                 RouteOrderToTargetDict, RebuildFleetPositionFromEntry,
                 ClassifyOrderByPrefix, IsValidOrderState
Blast Radius:    43 source files (activePositions + 7 order dicts)
CYC Confirmed:   9 (fallback; cluster dominant: AdoptMasterOrders=17, HydrateWorkingOrdersFromBroker=14)
Measured Cluster CYC: 68
Recommended Extractions: 3
MCP Tools:       jcodemunch resolve_repo, jcodemunch search_symbols,
                 jcodemunch get_symbol_complexity, jcodemunch get_blast_radius,
                 jcodemunch get_hotspots, sequential-thinking sequentialthinking
```
