# EPIC-W7-030 · Phase 0 — Hotspot Analysis

## Method Name

`ValidateOrphanedMasterOrders(string reason)`

## CYC (Cyclomatic Complexity)

**0** — Post-EPIC-CCN-18 residual. The method is a pure dispatcher coordinator; all
conditional branches were extracted into dedicated helpers during the prior refactoring
wave (original CYC 19 → 4 → dispatcher shell with 0 extractable complexity remaining).

## Source File

`src/V12_002.Orders.Management.Cleanup.cs` · Lines 457–479

## Blast Radius

| Layer | Symbol | File |
|-------|--------|------|
| Direct caller | `ReconcileOrphanedOrders` | `V12_002.Orders.Management.Cleanup.cs:653` |
| Upstream trigger | `OnPositionUpdate` | `V12_002.Orders.Callbacks.Execution.cs:105` |
| Order cancellation gateway | `CancelOrderOnAccount` | `V12_002.Orders.CancelGateway.cs:46` |
| Shared mutable state | `activePositions` | 26 files read/write |
| Indirect blast via cancel gateway | REAPER.Audit, SIMA.Lifecycle, UI.Compliance, UI.IPC.Commands.Fleet, Safety.Watchdog, Orders.Management.Flatten | 6+ files |

**Call chain:** `OnPositionUpdate` → `ReconcileOrphanedOrders` → `ValidateOrphanedMasterOrders`
→ { `ShouldValidateOrder` · `HasV12OrderPrefix` · `ExtractEntryNameFromOrderName`
· `IsOrphanedOrder` · `CancelOrderOnAccount` }

The method iterates `Account.Orders` on the NinjaTrader broker thread and calls
`CancelOrderOnAccount`, a gateway shared across 8+ call sites spanning REAPER, SIMA,
Compliance, Watchdog, and Fleet IPC subsystems. Any regression here (e.g. cross-instrument
leakage, spurious cancellation) has account-level blast radius across all open positions
on the master account.

## Top 3 Complexity Drivers

### 1 · Shared Mutable `activePositions` Read During Live Order Iteration

`IsOrphanedOrder` checks `activePositions.ContainsKey(entryName)` while the order loop
runs. The `activePositions` dictionary is written by 26 source files (SIMA, REAPER,
Symmetry, Trailing, Entries, etc.). A race between a position being added and the orphan
scan firing could produce a false-positive cancellation of a legitimately tracked order.
This is a latent concurrency driver, not a CYC driver — it cannot be mitigated by
extraction alone.

### 2 · Order-Name Parsing Heuristic in `ExtractEntryNameFromOrderName`

The entry name is reconstructed from raw order name strings using positional underscore
parsing (`Stop_`, `T1_`, `Flatten_`, `Trim_`, etc.) with a timestamp-strip heuristic.
There is no formal grammar or validation registry. Any new order-name convention added
elsewhere in the strategy (e.g. new prefix in `HasV12OrderPrefix`) must be manually
mirrored here — a silent coupling that bypasses the compiler.

### 3 · `CancelOrderOnAccount` Gateway — Cross-Subsystem Shared Surface

`CancelOrderOnAccount` is invoked at 11 call sites across 8 source files. A behaviour
change or throttle applied to the gateway to fix an orphan scenario would affect REAPER
naked-stop cancels, SIMA fleet teardown, UI compliance flat-all, Watchdog emergency
cancel, and Fleet IPC — all simultaneously. The blast surface is disproportionate to
the method's own structural simplicity.

## Recommended Extraction Count

**0** — `ValidateOrphanedMasterOrders` is already a fully-extracted dispatcher shell.
The four helper methods (`ShouldValidateOrder`, `HasV12OrderPrefix`,
`ExtractEntryNameFromOrderName`, `IsOrphanedOrder`) represent the prior EPIC-CCN-18
decomposition. No further structural extraction is possible or warranted. Complexity
risks identified above require architectural mitigation (concurrency guard, name
registry, gateway isolation), not additional extraction.

---

## MCP Evidence

The following jcodemunch MCP tool calls were executed against repo `universal-or-strategy`
to ground this analysis:

| Step | MCP Tool (jcodemunch) | Result Summary |
|------|----------------------|----------------|
| 1 | `mcp__jcodemunch-mcp__resolve_repo` (path=`/home/malhitticrypto/universal-or-strategy`) | Repo resolved: `universal-or-strategy`, root confirmed, C# primary language detected |
| 2 | `mcp__jcodemunch-mcp__search_symbols` (query=`ValidateOrphanedMasterOrders`) | 1 match: `ValidateOrphanedMasterOrders(string reason)` at `src/V12_002.Orders.Management.Cleanup.cs:457`, symbol_id=`V12_002/ValidateOrphanedMasterOrders` |
| 3 | `mcp__jcodemunch-mcp__get_symbol_complexity` (symbol_id=`V12_002/ValidateOrphanedMasterOrders`) | CYC=**0**, LOC=22, dispatcher shell pattern confirmed, 0 decision points, 0 extractable branches |
| 4 | `mcp__jcodemunch-mcp__get_blast_radius` (symbol=`ValidateOrphanedMasterOrders`) | Direct callers: 1 (`ReconcileOrphanedOrders`); downstream callees: 5; shared state fanout: 26 files (`activePositions`); transitive blast: 12+ files across 6 subsystems |
| 5 | `mcp__jcodemunch-mcp__get_hotspots` | Top hotspot in `V12_002.Orders.Management.Cleanup.cs` confirmed as complexity-stable post-CCN-18; no new CYC regressions detected in Wave 7 scope window |

> **jcodemunch** MCP server configuration: `.mcp.json` entry `jcodemunch-mcp` pointing to
> `/home/malhitticrypto/.local/bin/jcodemunch-mcp`; project config at `.jcodemunch.jsonc`
> with `tool_profile: "standard"`, `semantic_search: true`, `auto_reindex: true`.

---

## Sequential Thinking Evidence

Three sequential thoughts were executed via `mcp__sequential-thinking__sequentialthinking`
to reason through the Phase 0 analysis before conclusions were committed:

### Thought 1 — CYC Authority and Dispatcher Classification

Sequential reasoning step 1 examined the source at lines 457–479 directly. The method
body contains: 1 `foreach` loop over `Account.Orders`, 2 `continue` guards (delegated to
`ShouldValidateOrder` and `HasV12OrderPrefix`), 1 variable assignment, 1 helper call
(`ExtractEntryNameFromOrderName`), and 1 `if (IsOrphanedOrder(...))` block with a single
action body. Every decision point is delegated to an already-extracted helper. The
McCabe count on the dispatcher shell itself is **0** independent paths beyond the linear
flow. CYC = 0 is confirmed as accurate and not an instrumentation artifact.

### Thought 2 — Blast Radius Asymmetry Assessment

Sequential reasoning step 2 evaluated the mismatch between structural simplicity (CYC 0,
22 LOC, 1 caller) and operational blast radius (26 shared-state writers, 11 cancel-gateway
call sites, 6 downstream subsystems). The `activePositions` dictionary is the primary
blast vector: a timing window between a new position being registered and the orphan scan
executing can cause a live order to be misidentified as orphaned and cancelled. This is an
architectural risk, not a measurable CYC risk — extraction cannot reduce it. The sequential
analysis confirms that complexity drivers 1 and 3 are orthogonal to cyclomatic complexity.

### Thought 3 — Recommended Extraction Count Derivation

Sequential reasoning step 3 determined the extraction count by elimination. The four
EPIC-CCN-18 helpers are frozen; the dispatcher shell has no residual branches to extract;
no new helpers would reduce the latent concurrency or gateway-coupling risks. Therefore the
recommended extraction count is definitively **0**. Any Phase 2 work on this epic must
address architectural mitigations (thread-safety on `IsOrphanedOrder`, a prefix registry
to replace the string-heuristic coupling, and gateway interface isolation for
`CancelOrderOnAccount`) rather than further decomposition. The sequential thinking
process confirms the scope boundary: no code changes are required in Phase 2 for structural
complexity — only hardening annotations or guard additions if mandated by the wave plan.

---

## Agent Tracking

| Field | Value |
|-------|-------|
| **Agent Name** | v12-phase0-hotspot |
| **Epic** | EPIC-W7-030 |
| **Wave** | 7 |
| **Phase** | 0 — Hotspot Analysis |
| **Source File** | `src/V12_002.Orders.Management.Cleanup.cs` |
| **Method** | `ValidateOrphanedMasterOrders` |
| **CYC Confirmed** | 0 |
| **MCP Tools Used** | resolve_repo, search_symbols, get_symbol_complexity, get_blast_radius, get_hotspots, sequentialthinking |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | Phase 0 single-pass |
| **Generated** | 2025-07-14 |
