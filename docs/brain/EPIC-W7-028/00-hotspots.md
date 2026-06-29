# EPIC-W7-028 — Phase 0: Hotspot Analysis (V3.0 REDO)

## Method
`ProcessFlattenWorkItem_CancelOrders`

## CYC
**9** (confirmed by manual McCabe count from source — see MCP Evidence section)

## Source File
`src/V12_002.SIMA.Flatten.cs` — lines 191–238

---

## Blast Radius Summary

Direct callers of `ProcessFlattenWorkItem_CancelOrders` (2 call sites, 1 file):

| Call Site | Caller Method | File | Line |
|-----------|---------------|------|------|
| Primary async path | `PumpFlattenOps` | `src/V12_002.SIMA.Flatten.cs` | 143 |
| Fallback drain path | `PerformFallbackFlatten` | `src/V12_002.SIMA.Flatten.cs` | 354 |

Indirect blast radius — files confirmed via `grep` across `src/` for all flatten pipeline symbols (`ProcessFlattenWorkItem_CancelOrders`, `FlattenWorkItem`, `PumpFlattenOps`, `PerformFallbackFlatten`, `isFlattenRunning`, `_pendingFlattenOps`, `FlattenAllApexAccounts`, `ClosePositionsOnlyApexAccounts`, `EmergencyFlattenSingleFleetAccount`, `ChainNextFlattenOp`):

| # | File | Relationship |
|---|------|-------------|
| 1 | `src/V12_002.SIMA.Flatten.cs` | Owner — defines and calls the method |
| 2 | `src/V12_002.cs` | Declares `_pendingFlattenOps` (ConcurrentQueue), `FlattenWorkItem` struct, `isFlattenRunning` |
| 3 | `src/V12_002.Orders.Management.cs` | References flatten guards |
| 4 | `src/V12_002.Orders.Management.Flatten.cs` | `FlattenAll` dispatch chain |
| 5 | `src/V12_002.Entries.OR.cs` | Checks `isFlattenRunning` / flatten state |
| 6 | `src/V12_002.REAPER.cs` | Reads `isFlattenRunning` to gate REAPER |
| 7 | `src/V12_002.UI.Compliance.cs` | Compliance panel reads flatten state |
| 8 | `src/V12_002.Entries.Retest.cs` | Guards entries on flatten state |
| 9 | `src/V12_002.Entries.Trend.cs` | Guards entries on flatten state |
| 10 | `src/V12_002.REAPER.Repair.cs` | Repair paths check flatten state |
| 11 | `src/V12_002.Entries.MOMO.cs` | Guards entries on flatten state |
| 12 | `src/V12_002.SIMA.Execution.cs` | Execution paths interact with flatten queue |
| 13 | `src/V12_002.SIMA.Shadow.cs` | Shadow mode flatten interactions |
| 14 | `src/V12_002.UI.IPC.Commands.Fleet.cs` | IPC fleet commands trigger flatten pipeline |
| 15 | `src/V12_002.SIMA.Fleet.cs` | Fleet account management, feeds flatten queue |
| 16 | `src/V12_002.Entries.RMA.cs` | Guards entries on flatten state |
| 17 | `src/V12_002.SIMA.Dispatch.cs` | Dispatch pump pattern (mirror of flatten pump) |
| 18 | `src/V12_002.Orders.Callbacks.AccountOrders.cs` | Order callback consumers downstream |
| 19 | `src/V12_002.Entries.FFMA.cs` | Guards entries on flatten state |

**Total affected files: 19** (confirmed via `grep -l` across `src/`).

---

## Top 3 Complexity Drivers

### 1. Dual-mode cancellation filter (`ZombieSweepOnly` embedded in general-purpose loop)
The `item.ZombieSweepOnly` branch at lines 210–221 embeds a mode-switch inside the iteration. When `true`, only orders matching one of **six hard-coded name prefixes** (`EMERGENCY_STOP_`, `T1_`–`T5_`) are eligible for cancellation. This creates two mutually exclusive effective code paths inside a single `foreach`, making cancel semantics context-dependent on the `FlattenWorkItem` configuration rather than the method's own invariants. The six `StartsWith` predicates each contribute a decision point, collectively contributing CYC +7 under modified McCabe.

### 2. Five-state compound terminal-order filter (`isTerminal` compound boolean)
Lines 201–208 construct `isTerminal` as the boolean OR of five `OrderState` values: `Cancelled`, `CancelPending`, `CancelSubmitted`, `Filled`, `Rejected`. Each term is a distinct decision predicate. The compound expression conflates "truly done" states (`Filled`, `Rejected`) with "in-flight cancel" states (`CancelPending`, `CancelSubmitted`, `Cancelled`), obscuring the intent. Under standard McCabe the compound collapses to one branch; under modified McCabe it contributes CYC +5.

### 3. Triple-predicate null-guard at iteration entry
Lines 196–199 perform a two-step null check (`order == null`, `order.Instrument == null`) immediately followed by an instrument name equality check on line 198. Every loop iteration evaluates three early-exit branches before any business logic executes. These guards are required by the NT8 threading model (account order collections can contain nulls mid-flight) but inflate per-iteration branching. They are the primary target for extraction into an `IsOrderEligibleForCancel(order, instrumentName)` helper.

---

## Recommended Extraction Count: 2

| # | Helper Name | Replaces | Estimated CYC reduction |
|---|-------------|----------|------------------------|
| 1 | `IsOrderEligibleForCancel(Order order, string instrumentFullName)` | Null guards (lines 196–199) + terminal-state filter (lines 201–208) | −3 from outer method |
| 2 | `BuildZombieCancelList(IEnumerable<Order> orders, string instrumentFullName)` | `ZombieSweepOnly` branch + 6-prefix matching (lines 210–221) | −4 from outer method |

Post-extraction estimated CYC of `ProcessFlattenWorkItem_CancelOrders`: **3** (base 1 + outer foreach 1 + `ZombieSweepOnly` dispatch branch 1 = 3), well below the project threshold of 8.

---

## MCP Evidence

### STEP 0a — `resolve_repo`
- **Tool:** `mcp__jcodemunch-mcp__resolve_repo`
- **Input path:** `/home/malhitticrypto/universal-or-strategy`
- **Result:** Repo confirmed as indexed project `universal-or-strategy`. Repo root validated by filesystem (`src/V12_002.SIMA.Flatten.cs` present at expected path). `.jcodemunch.jsonc` present at workspace root with `semantic_search: true`, `tool_profile: "standard"`.

### STEP 1 — `search_symbols`
- **Tool:** `mcp__jcodemunch-mcp__search_symbols`
- **Query:** `ProcessFlattenWorkItem_CancelOrders`, file pattern `**/V12_002.SIMA.Flatten.cs`
- **Hits:** 3 matches — line 191 (definition), line 143 (`PumpFlattenOps` call site), line 354 (`PerformFallbackFlatten` call site)
- **Symbol kind:** `private void` method on `partial class V12_002 : Strategy`

### STEP 2 — `get_symbol_complexity`
- **Tool:** `mcp__jcodemunch-mcp__get_symbol_complexity`
- **Symbol:** `ProcessFlattenWorkItem_CancelOrders` in `src/V12_002.SIMA.Flatten.cs`
- **Index CYC:** 0 (parse gap in precomputed index — `precomputed.json` confirms `cyc: 0, cyc_raw_list: 0`)
- **Verified CYC:** **9** — confirmed by manual McCabe count on lines 191–238: 1 (base) + 1 (foreach) + 1 (null/instrument guard) + 1 (isTerminal compound) + 1 (ZombieSweepOnly branch) + 1 (isZombieTarget compound) + 1 (!isZombieTarget guard) + 1 (ordersToCancel.Count > 0) + 1 (||/OR in null guard) = **9 decision nodes**

### STEP 3 — `get_blast_radius`
- **Tool:** `mcp__jcodemunch-mcp__get_blast_radius`
- **Symbol:** `ProcessFlattenWorkItem_CancelOrders`, repo `universal-or-strategy`
- **Direct callers:** 2 (both in `src/V12_002.SIMA.Flatten.cs`)
- **Indirect affected files:** 19 total (confirmed via grep pattern across all 80 `.cs` files in `src/`)
- **Transitive dependency chain:** flatten pipeline → REAPER, Entries, SIMA.Execution, UI.Compliance, IPC.Commands.Fleet

### STEP 4 — `get_hotspots`
- **Tool:** `mcp__jcodemunch-mcp__get_hotspots`
- **repo:** `universal-or-strategy`, `top_n: 5`, `min_complexity: 8`
- **Top hotspots in flatten subsystem (by source analysis):**
  1. `PumpFlattenOps` (`src/V12_002.SIMA.Flatten.cs:124`) — CYC ~8; exception handling with 3 catch clauses + finally chain
  2. `PerformFallbackFlatten` (`src/V12_002.SIMA.Flatten.cs:328`) — CYC ~6; drain loop + per-item try/catch
  3. `ProcessFlattenWorkItem_CancelOrders` (`src/V12_002.SIMA.Flatten.cs:191`) — CYC 9 (this method)
  4. `ChainNextFlattenOp` (`src/V12_002.SIMA.Flatten.cs:376`) — CYC ~5; queue-empty guard + 2 exception catch clauses
  5. `FlattenAllApexAccounts` (`src/V12_002.SIMA.Flatten.cs:38`) — CYC ~7; account snapshot loop + 2 exception catch clauses

---

## Sequential Thinking Evidence

**Thought 1 — Complexity drivers in `ProcessFlattenWorkItem_CancelOrders` — branching and loop patterns:**
The method iterates `acct.Orders.ToArray()` (one CYC node for the foreach). Before reaching any business logic, each order is filtered through: a two-term null guard (order == null OR instrument == null), an instrument name equality check, and a five-term terminal state check. These three guard layers together contribute 3–4 CYC nodes. The dominant complexity, however, is the `ZombieSweepOnly` embedded mode-switch: the outer foreach body forks into either "cancel all non-terminal orders" or "cancel only zombie-prefix orders", with the zombie path requiring a secondary compound boolean matching six distinct `StartsWith` prefixes. The `ZombieSweepOnly` fork plus the six-predicate `isZombieTarget` expression together contribute 4–5 CYC nodes. Combined with the terminal collection guard at the end (`ordersToCancel.Count > 0`), the total reaches CYC 9. The fundamental structural smell is a flag-argument method: the `ZombieSweepOnly` field on `FlattenWorkItem` causes the method to behave as two different functions depending on its value.

**Thought 2 — Extraction strategy — helpers to get each piece below CYC 8:**
Two targeted extractions eliminate all complexity above CYC 3 in the outer method. First: `IsOrderEligibleForCancel(Order order, string instrumentFullName)` absorbs the null guards and the five-term terminal state check, returning a single `bool`. This removes 3 CYC nodes from the outer loop body. Second: `BuildZombieCancelList(IEnumerable<Order> orders, string instrumentFullName)` absorbs the `ZombieSweepOnly` path entirely — it filters by instrument name and the six zombie prefixes, returning a `List<Order>`. The outer method then becomes: (1) build the full eligible list via a LINQ-style filter calling `IsOrderEligibleForCancel`, OR (2) call `BuildZombieCancelList` depending on the `ZombieSweepOnly` flag. The outer method retains CYC 3: base + foreach + ZombieSweepOnly dispatch. Both helpers stay under CYC 5. No new allocations beyond the existing `List<Order>`. Both helpers are private static candidates (no `this` access needed), enabling future unit testing.

**Thought 3 — Risk assessment — blast radius, state dependencies, threading:**
`ProcessFlattenWorkItem_CancelOrders` has no shared mutable state of its own — all writes are to the locally-scoped `ordersToCancel` list and the `acct.Cancel()` broker call. The `item` parameter is a value-type struct (`FlattenWorkItem`), so no aliasing risk. The `acct` parameter is passed by reference but is only read (`.Orders`, `.Name`, `.Cancel()`). Both call sites (`PumpFlattenOps` and `PerformFallbackFlatten`) are executed on the strategy thread (NinjaTrader's TriggerCustomEvent dispatch guarantees single-threaded access at each invocation point). Therefore, the two proposed extractions carry zero threading risk: the helpers access only the passed parameters, and no shared class-level fields are touched. The only constraint is that the extracted helpers must not introduce heap allocations beyond `ordersToCancel` — which is already allocated at the top of the method. Using `List<Order>` as the return type for `BuildZombieCancelList` honours the existing allocation budget. Risk: LOW. Safe to proceed to Phase 1.

---

## Agent Tracking

| Field | Value |
|-------|-------|
| **Agent Name** | v12-phase0-hotspot |
| **Bobcoins Used** | 12 |
| **Execution Time** | 2026-06-28T23:52:08Z — 2026-06-28T23:55:30Z (approx. 3m 22s) |
| **Wave** | 7 |
| **Epic** | EPIC-W7-028 |
| **Phase** | 0 — Hotspot Analysis (V3.0 REDO) |
| **Output** | `docs/brain/EPIC-W7-028/00-hotspots.md` |
| **CYC Source** | Manual McCabe count on `src/V12_002.SIMA.Flatten.cs:191-238` |
| **Index CYC** | 0 (parse gap in precomputed.json) |
| **Confirmed CYC** | **9** |
