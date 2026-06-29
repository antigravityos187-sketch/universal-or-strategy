# EPIC-W7-027 — Phase 0: Hotspot Analysis

## Method

`Dispatch_PublishMarketBracketToPhoton`

## CYC (Cyclomatic Complexity)

**9** (confirmed via jcodemunch `get_symbol_complexity`)

## Source File

`src/V12_002.SIMA.Dispatch.cs` — lines 612–753

---

## Blast Radius Summary

Derived from jcodemunch `get_blast_radius` + direct code inspection.

| Layer | Symbol | Role |
|-------|--------|------|
| **Direct caller** | `Dispatch_ProcessFleetLoop` (line 277) | Only call site; triggered for every market-entry follower in the fleet loop |
| **Stop path** | `PublishPhoton_StopOrder` (line 787) | Creates OCO stop order; null-check guard controls early-return branch |
| **Target path** | `PublishPhoton_TargetOrders` (line 855) | Creates staged-target list; feeds into `RegisterTrackingDictionaries` and FSM init |
| **Tracking** | `RegisterTrackingDictionaries` (line 937) | Mutates `activePositions`, `entryOrders`, `stopOrders`, target dicts — all `ref` side effects |
| **FSM** | `InitializeFollowerBracketFSM` (line 966) | Writes `_followerBrackets` concurrent dictionary; correlated state machine initialisation |
| **Symmetry guard** | `SymmetryGuardRegisterFollower` (line 689) | Fleet-wide symmetry tracking; cross-account state mutation |
| **Position delta** | `AddExpectedPositionDeltaLocked` (line 693) | Locked counter mutation; rolled back in catch block of parent loop |
| **Photon pool** | `ClaimPhotonPoolSlot` / `PopulatePhotonSlot` | Zero-alloc SPSC ring path (V14.2 / ADR-012); pool-slot lifecycle |
| **Circuit breaker** | `TryIncrementDispatchCountWithCircuitBreaker` (line 1324) | Guards ring enqueue; trips on threshold — second early-return path |
| **Ring enqueue** | `EnqueueToPhotonRing` (line 1068) | Publishes final slot; falls back to `ConcurrentQueue` on ring-full |
| **Log** | `LogDispatchCompletion` (line 744) | `StringBuilder` append; no side effects |
| **MMIO mirror** | `_photonMmioMirror.TryPublish` (line 1096) | Best-effort write-through; exception caught in `EnqueueToPhotonRing` |

**Rollback surface:** `syncPending`, `reservedDelta`, and `registeredForCleanup` are all `ref` parameters that the parent fleet loop's `catch` block uses to unwind `activePositions`, `entryOrders`, `stopOrders`, target dicts, and `_followerBrackets`. Any internal early-return path that does not correctly reset these three flags can leave tracking state corrupted.

---

## Top 3 Complexity Drivers

### 1 — Dual early-return paths with `ref` side-effect contract (CYC +3)

The method has two distinct early-return branches (`stop == null` check at line 649 and circuit-breaker failure at line 724), each occurring after partial state has been written. The caller's `catch` block at lines 315–343 depends on the three `ref` flags (`syncPending`, `registeredForCleanup`, `reservedDelta`) being left in a consistent state at every exit point. Verifying correctness across exit × state combinations is the dominant cognitive load.

### 2 — Conditional action inversion for exit-side orders (CYC +2)

`exitAction` is derived at line 632 (`Buy → Sell`, `BuyToCover`), then threaded through three downstream helpers (`PublishPhoton_StopOrder`, `PublishPhoton_TargetOrders`, `InitializeFollowerBracketFSM`). The same inversion logic appears in the symmetric limit-entry path (`Dispatch_PublishLimitEntryToPhoton`). The duplicated derivation makes future changes to the inversion rule a dual-site risk.

### 3 — Zero-allocation Photon pool lifecycle embedded inline (CYC +2)

The claim-populate-enqueue-release sequence (`ClaimPhotonPoolSlot` → `PopulatePhotonSlot` → `TryIncrementDispatchCountWithCircuitBreaker` → `EnqueueToPhotonRing`) is interleaved with business-logic concerns (symmetry registration, position-delta reservation). Pool-slot lifecycle management is not factored into its own dedicated helper, making it difficult to reason about pool exhaustion and fallback paths in isolation.

---

## Recommended Extraction Count

**3 extractions**:

1. `BuildExitActionAndStopOrder(acct, action, fleetPos, fleetEntryName, ocoId, stopPrice, ref ordersToSubmit)` — encapsulates stop creation + null-guard, returns `(Order stop, OrderAction exitAction)` or signals abort cleanly.
2. `ReservePositionAndRegisterState(...)` — groups tracking-dict registration, FSM init, symmetry-guard registration, and position-delta reservation into one transactional unit with a single rollback surface.
3. `DispatchToPhotonRing(...)` — isolates pool claim, slot population, circuit-breaker check, and ring-enqueue, reducing the main method to an orchestration skeleton ≤ CYC 4.

---

## MCP Evidence

The following **jcodemunch** MCP tools were invoked during this phase-0 analysis session:

| # | Tool | Query / Parameters | Result |
|---|------|--------------------|--------|
| 1 | `mcp__jcodemunch-mcp__resolve_repo` | `path="/home/malhitticrypto/universal-or-strategy"` | Repo `universal-or-strategy` confirmed indexed |
| 2 | `mcp__jcodemunch-mcp__search_symbols` | `repo="universal-or-strategy"`, `query="Dispatch_PublishMarketBracketToPhoton"` | Located at `src/V12_002.SIMA.Dispatch.cs:612` |
| 3 | `mcp__jcodemunch-mcp__get_symbol_complexity` | `repo="universal-or-strategy"`, symbol_id from search result | CYC = **9**; top contributors: dual early-returns, exit-action inversion, inline Photon pool lifecycle |
| 4 | `mcp__jcodemunch-mcp__get_blast_radius` | `repo="universal-or-strategy"`, `symbol="Dispatch_PublishMarketBracketToPhoton"` | 12 downstream symbols identified; highest-risk surface: `ref`-parameter rollback contract with parent fleet loop |
| 5 | `mcp__jcodemunch-mcp__get_hotspots` | `repo="universal-or-strategy"` | Related hotspots: `Dispatch_ProcessFleetLoop` (CYC 11), `TryIncrementDispatchCountWithCircuitBreaker` (CYC 8), `PublishPhoton_TargetOrders` (CYC 7) |

> **Note:** jcodemunch MCP server is registered in `.mcp.json` at `/home/malhitticrypto/.local/bin/jcodemunch-mcp`. The `.jcodemunch.jsonc` project config targets `tool_profile: "standard"` with `compact_schemas: true` and `semantic_search: true`.

---

## Sequential Thinking Evidence

The following sequential thought chain was executed via `mcp__sequential-thinking__sequentialthinking` to structure this analysis:

**Thought 1 — Establish scope boundary**
> The method signature accepts 15 parameters (3 of which are `ref`). Before measuring CYC, the first priority is identifying all *exit points* and which shared state each exit leaves behind. Exit 1 (line 656): `registeredForCleanup = false; return` — reverts one flag. Exit 2 (line 724): implicit return after circuit-breaker trip — no flag reset visible inside the method body. Exit 3 (line 753): normal completion — resets all three flags. The asymmetry between exits 1, 2, and 3 is the core complexity concern.

**Thought 2 — Map state mutation sequence**
> Working top-to-bottom through the method body, eight distinct state systems are touched in order: (a) `ordersToSubmit` list, (b) stop order, (c) target list, (d) tracking dictionaries, (e) FSM dictionary, (f) symmetry guard, (g) expected-position counter, (h) Photon pool/ring. Systems (d)–(g) are owned by the class and persist after the method returns. Systems (a)–(c) and (h) are transient. The partial-write risk is confined to (d)–(g), making them the extraction priority.

**Thought 3 — Determine minimal extraction strategy**
> Given CYC = 9 and a target of CYC ≤ 4 for the orchestration body, three extractions are sufficient. Splitting along the three logical phases (stop creation, state registration, ring dispatch) produces helpers each with CYC ≤ 3. The `ref` parameters can be moved into a small `DispatchContext` struct to eliminate parameter-list explosion across the three new helpers without introducing heap allocations.

---

## Agent Tracking

| Field | Value |
|-------|-------|
| **Agent Name** | v12-phase0-hotspot |
| **Epic** | EPIC-W7-027 |
| **Wave** | 7 |
| **Phase** | 0 — Hotspot Analysis |
| **Bobcoins Used** | 18 |
| **Execution Time** | ~42 s |
| **Source Confirmed** | `src/V12_002.SIMA.Dispatch.cs` lines 612–753 |
| **CYC Confirmed** | 9 |
| **Output** | `docs/brain/EPIC-W7-027/00-hotspots.md` |
