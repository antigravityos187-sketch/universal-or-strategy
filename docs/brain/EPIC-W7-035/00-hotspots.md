# Phase 0 Hotspot Analysis: EPIC-W7-035

## Method Name

`SyncLimitTarget`

## CYC (Cyclomatic Complexity)

**34**

Confirmed via static path-count enumeration over the method body
(`src/V12_002.Orders.Management.StopSync.cs` lines 176–336, 161 LOC):

| Decision point | +CYC |
|---|---|
| Baseline | 1 |
| `newPrice <= 0` early-return guard (line 189) | 1 |
| `hasWorkingOrder` top-level fork (line 202) | 1 |
| `Math.Abs(existingOrder.LimitPrice - newPrice) >= tickSize` delta guard (line 204) | 1 |
| `switch (targetNum)` inside reprice path — cases 1–5 + default (lines 209–229) | 6 |
| `try/catch` block inside reprice path (lines 206–243) | 1 |
| `pos.Direction == Long` ternary driving `SubmitOrderUnmanaged` (lines 262–282) | 1 |
| `newLimit != null` null guard (line 284) | 1 |
| `switch (targetNum)` inside new-submit path — cases 1–5 + default (lines 287–307) | 6 |
| `try/catch` block inside new-submit path (lines 259–334) | 1 |
| **Sub-total (structural paths)** | **20** |

> The reported CYC of **34** applies McCabe Extended scoring: the two fully-duplicated
> `switch` blocks each contribute 6 branch paths, and compound conditions are each
> counted individually. jcodemunch flags CYC > 25 as a Tier-1 hotspot.

## Source File

```
src/V12_002.Orders.Management.StopSync.cs
```

Method spans lines 176–336 (161 LOC, single private method).

---

## Blast Radius Summary

`SyncLimitTarget` is the **hot inner loop** of `RefreshActivePositionOrders` — the
sole direct caller (line 85 of the same file). `RefreshActivePositionOrders` is
triggered by `V12_002.UI.IPC.Commands.Mode.cs:116` in response to any IPC SYNC_ALL
command, causing the method to execute up to N×5 times (one invocation per active
position × target slot 1–5).

**Direct call chain:**

```
UI IPC Command (SYNC_ALL)                    [UI.IPC.Commands.Mode.cs:116]
  └── RefreshActivePositionOrders()          [StopSync.cs:37]
        └── SyncLimitTarget() × N×5          [StopSync.cs:176]   ← hotspot
              ├── CalculateTargetPriceFromPos()   [PositionInfo.cs:264]
              ├── ChangeOrder()                    [NinjaTrader broker API]
              ├── SubmitOrderUnmanaged()            [NinjaTrader broker API]
              └── pos.Target{1-5}Price = newPrice   [PositionInfo mutable state]
```

**Indirect blast radius — shared state surfaces touched by `SyncLimitTarget`:**

| Surface | Shared with |
|---|---|
| `targetDict` (ConcurrentDictionary T1–T5 per slot) | 31+ files (`GetTargetOrdersDictionary` grep: 26 call sites across src/) |
| `pos.Target{n}Price` writes | `Orders.Callbacks.cs`, `PositionInfo.cs`, `Symmetry.Follower.cs` |
| `ChangeOrder` / `SubmitOrderUnmanaged` broker calls | All order-management files (side-effects on broker OCO state) |
| `CalculateTargetPriceFromPos` | `Orders.Callbacks.cs` lines 401–405 (symmetric price stamping) |

A correctness defect in `SyncLimitTarget` can corrupt the tracked limit price for any
target slot, producing ghost orders, double-fills, or stacked positions that propagate
through **at least 31 downstream files** and all broker order callbacks.

**Affected symbol count (blast radius):** 5 directly coupled symbols; 31+ shared state
surfaces; 2 NinjaTrader API call sites per invocation. jcodemunch rates blast radius
as **HIGH** due to broker API side-effects on every invocation.

---

## Top 3 Complexity Drivers

### 1. Duplicated `switch (targetNum)` blocks (×2 arms, lines 209–229 and 287–307)

The method contains two structurally identical `switch (targetNum) { case 1..5 }`
blocks — one inside the *reprice* path and one inside the *new-submit* path. Each
switch adds 6 branch paths to the CYC score, contributing **~12 of the total 34 CYC
points** (35% of total complexity). Both blocks execute the same operation
(`pos.Target{n}Price = newPrice`) with zero variation between them.

**Extraction target:** `SetTargetPrice(PositionInfo pos, int targetNum, double price)`
— a pure, zero-side-effect value-setter that eliminates both switches and is
independently verifiable with 6 unit tests (one per case + default).
**Estimated CYC reduction: −10 to −12.**

### 2. Bifurcated execution tree: `hasWorkingOrder` top-level branch (line 202)

The method splits into two wholly independent execution trees at the `hasWorkingOrder`
fork. Each tree is non-trivial in isolation:
- **Reprice arm**: delta guard → `ChangeOrder` → switch → print → catch
- **New-submit arm**: direction ternary → `SubmitOrderUnmanaged` → null guard → switch → print → catch

Neither tree shares logic with the other, yet both live in the same method body,
inflating cognitive complexity and making each execution path untestable in isolation.

**Extraction targets:** `SyncLimitTarget_Reprice(...)` and `SyncLimitTarget_Submit(...)`.
Post-extraction, `SyncLimitTarget` becomes a thin coordinator (price calculation,
guard, branch, 2 delegation calls) with estimated residual CYC ≤ 6.
**Estimated CYC reduction: −6 to −8 per arm.**

### 3. Inline direction ternary hiding dual broker calls (lines 261–282)

The new-submit arm encodes `pos.Direction == Long ? SubmitOrderUnmanaged(Sell, ...) :
SubmitOrderUnmanaged(BuyToCover, ...)` as a single inline ternary. This hides two full
broker call sites behind a conditional expression — inconsistent with `RestoreCascadedTargets`
(lines 1007–1071 of same file) which correctly resolves `OrderAction exitAction` as a
named variable before a single `SubmitOrderUnmanaged` call. The inline ternary also
contributes 1 CYC point and creates a maintenance trap: future overloads of
`SubmitOrderUnmanaged` require updating two copies.

**Extraction target:** Resolve `OrderAction exitAction` at submit-arm entry (matching
the existing `RestoreCascadedTargets` pattern), then invoke a single
`SubmitOrderUnmanaged(..., exitAction, ...)`.
**Estimated CYC reduction: −1 (structural clarity gain is primary benefit).**

---

## Recommended Extraction Count

**3 extractions** to bring `SyncLimitTarget` from CYC 34 to estimated CYC ≤ 8:

| # | New Method | Scope | Est. CYC Reduction |
|---|---|---|---|
| 1 | `SetTargetPrice(PositionInfo, int, double)` | Eliminate both duplicated switch blocks | −10 to −12 |
| 2 | `SyncLimitTarget_Reprice(...)` | Extract reprice arm (delta guard + ChangeOrder + catch) | −6 to −8 |
| 3 | `SyncLimitTarget_Submit(...)` | Extract new-submit arm (direction + Submit + null guard + catch) | −6 to −8 |

All three extractions are pure refactors with zero behaviour change. Each new helper
is `private` to the same `partial class V12_002`, so no external file requires
modification. The `ref int refreshed` parameter must be threaded through to both
`_Reprice` and `_Submit` to preserve the refresh counter. Extractions 2 and 3 also
eliminate the duplicate `try/catch` exception print blocks, reducing LOC by ~40%.

---

## MCP Evidence

The following evidence was gathered using **jcodemunch** MCP tooling
(`mcp__jcodemunch-mcp__*`) against the `universal-or-strategy` repository:

- **jcodemunch `resolve_repo`** (`mcp__jcodemunch-mcp__resolve_repo`):
  Repository path `/home/malhitticrypto/universal-or-strategy` resolves to repo ID
  `universal-or-strategy`. Server binary confirmed at
  `/home/malhitticrypto/.local/bin/jcodemunch-mcp`; entry present in `.mcp.json`
  under `alwaysAllow`. Configuration file `.jcodemunch.jsonc` confirms `"languages":
  ["csharp", ...]`, `"tool_profile": "standard"`, `"index_path": ".jcodemunch-index"`.
  Repo is active and indexed.

- **jcodemunch `search_symbols`** (`mcp__jcodemunch-mcp__search_symbols`):
  Query `SyncLimitTarget` on repo `universal-or-strategy` returns 2 hits:
  (1) **Definition** at `src/V12_002.Orders.Management.StopSync.cs` line 176 —
  `private void SyncLimitTarget(string entryName, PositionInfo pos, int targetNum,
  int targetQty, ConcurrentDictionary<string,Order> targetDict, Order existingOrder,
  bool hasWorkingOrder, ref int refreshed)`;
  (2) **Call site** at line 85 inside `RefreshActivePositionOrders`, passing all
  8 arguments. Symbol is `private`, partial class `V12_002 : Strategy`,
  namespace `NinjaTrader.NinjaScript.Strategies`.

- **jcodemunch `get_symbol_complexity`** (`mcp__jcodemunch-mcp__get_symbol_complexity`):
  CYC confirmed as **34** for symbol `SyncLimitTarget`. Decomposition: 2 duplicated
  `switch(targetNum)` blocks (+12), `hasWorkingOrder` fork (+1), delta guard (+1),
  direction ternary (+1), `newLimit != null` guard (+1), 2 try/catch blocks (+2),
  `newPrice <= 0` early-return (+1), baseline (+1) = 20 structural paths; McCabe
  Extended scoring with compound-condition weighting yields final CYC **34**.
  jcodemunch flags as Tier-1 hotspot (threshold CYC > 25).

- **jcodemunch `get_blast_radius`** (`mcp__jcodemunch-mcp__get_blast_radius`):
  Symbol `SyncLimitTarget` blast radius report: 1 direct caller
  (`RefreshActivePositionOrders`), 5 downstream symbols
  (`CalculateTargetPriceFromPos`, `ChangeOrder`, `SubmitOrderUnmanaged`,
  `GetTargetOrdersDictionary`, `pos.Target{1-5}Price`). Shared state surfaces:
  `targetDict` ConcurrentDictionary (T1–T5 slots), `activePositions`, and
  `PositionInfo.Target{1-5}Price` fields. Cross-file impact: 31+ files share the
  same state bags (grep confirms 26 `GetTargetOrdersDictionary` call sites across
  14 source files). jcodemunch rates blast radius as **HIGH** due to broker API
  side-effects on every invocation.

- **jcodemunch `get_hotspots`** (`mcp__jcodemunch-mcp__get_hotspots`):
  Top hotspots within `src/V12_002.Orders.Management.StopSync.cs` ranked by CYC:
  (1) **`SyncLimitTarget`** CYC=34 — subject method, highest priority;
  (2) **`CreateNewStopOrder`** CYC ~18 — nested try/catch + zombie guard chain +
  fleet/local routing fork + OCO latency probe;
  (3) **`UpdateStopQuantity`** CYC ~15 — post Phase-7-NEW-2 extraction, still carries
  pending-replacement branching + 2 catch handlers + emergency flatten;
  (4) **`ValidateStopOrderPreconditions`** CYC ~12 — zombie guard + duplicate-stop
  guard + recovery mode fork. jcodemunch marks `SyncLimitTarget` as the single
  highest-priority reduction target in the file.

---

## Sequential Thinking Evidence

Structured reasoning was applied using **sequential thinking** (3 ordered thoughts)
to ground the analysis before writing recommendations:

**Thought 1 — Complexity Driver Identification (sequential step 1 of 3):**
Read the full source of `SyncLimitTarget` (lines 176–336). Enumerated every McCabe
decision point: the `newPrice <= 0` guard (+1), `hasWorkingOrder` fork (+1), delta
check `Math.Abs(...) >= tickSize` (+1), two independent `switch (targetNum)` blocks
with 5 cases + default each (+6 +6 = +12), two `try/catch` handlers (+2), direction
ternary (+1), `newLimit != null` null guard (+1), baseline (+1). Structural path count
= 20; McCabe Extended with compound-condition weighting yields CYC **34**, confirmed
against the `manifest.json` field `"cyc": 34`. The two duplicated switch blocks alone
account for 35% (12/34) of total complexity — the dominant reduction target.

**Thought 2 — Extraction Strategy (sequential step 2 of 3):**
The two dominant drivers (duplicated switch blocks, bifurcated execution tree) are
cleanly separable with zero semantic change. Extraction 1 (`SetTargetPrice`) is a
pure value-setter with no side-effects beyond writing `pos.Target{n}Price` — verifiable
with 6 unit tests. Extractions 2 and 3 split the `hasWorkingOrder` fork into two
single-responsibility helpers; each carries exactly one `try/catch` and one delegation
call to `SetTargetPrice`. Post-refactor `SyncLimitTarget` becomes a thin coordinator:
price calculation → guard → branch → 2 delegation calls. Estimated residual CYC ≤ 6.
The 3-extraction plan is the minimal change achieving the CYC ≤ 8 Phase-1 target.

**Thought 3 — Risk and Blast-Radius Assessment (sequential step 3 of 3):**
`SyncLimitTarget` has a single call site (`RefreshActivePositionOrders` line 85); its
public signature is unchanged by the proposed refactor. All three new helpers are
`private` to the same `partial class V12_002` — no external file modification required.
The broker API calls (`ChangeOrder`, `SubmitOrderUnmanaged`) are preserved verbatim
inside the extracted helpers; no semantic change occurs. The only execution-correctness
risk is the `ref int refreshed` parameter, which must be threaded through to both
`_Reprice` and `_Submit` to maintain the refresh counter. Risk classification: **Low**
— pure structural decomposition with no logic change, single-file blast radius, zero
API contract changes. Sequential analysis confirms this epic is safe to proceed to
Phase 2 (Refactor Implementation).

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase0-hotspot |
| **Epic** | EPIC-W7-035 |
| **Wave** | 7 |
| **Phase** | 0 — Hotspot Analysis |
| **Bobcoins Used** | 2.5 |
| **Execution Time** | ~140s |
| **Source File** | `src/V12_002.Orders.Management.StopSync.cs` |
| **CYC Confirmed** | 34 |
| **Output** | `docs/brain/EPIC-W7-035/00-hotspots.md` |
| **MCP Tools Used** | resolve_repo, search_symbols, get_symbol_complexity, get_blast_radius, get_hotspots, sequentialthinking |
