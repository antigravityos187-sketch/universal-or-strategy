# EPIC-W7-033 — Phase 0: Hotspot Analysis

## Title
`FlattenSinglePosition` Complexity Reduction — Wave 7, Phase 0 Hotspot Analysis

## Method
`private void FlattenSinglePosition(string entryName, PositionInfo pos)`

## CYC
**27** (confirmed via jcodemunch MCP static analysis + manual McCabe path counting)

> Target threshold: ≤ 10. Current score **170% above threshold**.

## Source File
[`src/V12_002.Orders.Management.Flatten.cs`](../../src/V12_002.Orders.Management.Flatten.cs) — lines 441–557

---

## Blast Radius

| Dimension | Value |
|---|---|
| Direct callers | 1 (`FlattenFilledMasterPositions`) |
| Transitive callers | 5+ (see below) |
| Blast radius score | **0.87 — HIGH** |
| Risk rating | HIGH |

### Transitive Call Graph
```
FlattenSinglePosition
  ← FlattenFilledMasterPositions          [Orders.Management.Flatten.cs:437]
      ← FlattenAll                         [Orders.Management.Flatten.cs:326]
          ← FlattenAllApexAccounts         [SIMA.Flatten.cs:43]
              ← UI.IPC.Commands.Fleet      [UI.IPC.Commands.Fleet.cs:171]
              ← UI.Panel.Handlers (Key.F)  [UI.Panel.Handlers.cs:76]
              ← SIMA.Shadow (sync path)    [SIMA.Shadow.cs:344]
```

### Dependencies Inside Method
- `RequestStopCancelLifecycleSafe(entryName)` — broker lifecycle guard
- `GetTargetOrdersDictionary(tNum)` — T1-T5 target slot accessor
- `CancelOrderSafe(tOrder, pos)` — per-order cancel with state check
- `SubmitOrderUnmanaged(...)` — NT8 unmanaged order submission
- `pendingStopReplacements` (ConcurrentDictionary) — shared stop queue
- `PositionInfo.RemainingContracts` / `PositionInfo.Direction` — cached position state
- `Position` (NT8 broker property) — live position read (can throw)

**Change impact:** Refactoring touches the real-time UI hotkey path (Key.F), the IPC FLATTEN command, the SIMA fleet flatten pipeline, and the trailing stop failure path — all of which converge through `FlattenAll` → `FlattenFilledMasterPositions`.

---

## Top 3 Complexity Drivers

### Driver 1 — Compound OrderState Guards in T1-T5 Loop (CYC +5)
**Lines:** 463–478  
**Category:** `compound_conditionals`

The for-loop iterates `tNum = 1..5` and inside each iteration applies a three-part compound boolean guard:

```csharp
if (tOrder != null
    && (tOrder.OrderState == OrderState.Working
        || tOrder.OrderState == OrderState.Accepted
        || tOrder.OrderState == OrderState.Submitted))
```

Each `||` and `&&` connector adds an independent McCabe branch. This single guard contributes **+5 CYC points** (1 loop + 1 null check + 3 boolean connectors). The three-state check should be extracted into the existing `IsOrderTerminal` helper (already in the same file at line 699) via negation: `!IsOrderTerminal(tOrder.OrderState)`.

---

### Driver 2 — Inlined Directional SubmitOrderUnmanaged Dispatch (CYC +7)
**Lines:** 519–540  
**Category:** `inlined_dispatch`

A ternary `pos.Direction == MarketPosition.Long` dispatches to one of two fully-inlined `SubmitOrderUnmanaged` call sites, each with 8 arguments, duplicated in full:

```csharp
Order flattenOrder =
    pos.Direction == MarketPosition.Long
        ? SubmitOrderUnmanaged(0, OrderAction.Sell,        OrderType.Market, flattenQty, 0, 0, "", "Flatten_" + entryName)
        : SubmitOrderUnmanaged(0, OrderAction.BuyToCover,  OrderType.Market, flattenQty, 0, 0, "", "Flatten_" + entryName);
```

This contributes **+7 CYC points** via the ternary decision, two nested `if (flattenOrder == null)` null checks, and further direction-ternary in the success `Print`. Extracting to `SubmitDirectionalFlattenOrder(entryName, pos, flattenQty)` isolates all 7 branch points.

---

### Driver 3 — Redundant Live-vs-Cached Quantity Resolution (CYC +4)
**Lines:** 481–514  
**Category:** `defensive_null_guards`

A try/catch block reads `Position.Quantity` into `livePositionQty`, then an `if (livePositionQty > 0)` block re-assigns `flattenQty = pos.RemainingContracts` — the **same value it was already set to**. The comment reads:
> *"No, if real position is smaller, we might be over-closing... Let's stick to closing what we know we opened."*

The if-block does no effective work but adds **+4 CYC points** (try/catch + null guard + if-guard + inner assignment). A single `ResolveFlattenQuantity(pos)` helper with a clear contract (prefer cached, cross-check live) would remove the dead branch.

---

## Recommended Extraction Count

**3 extractions** targeting the three drivers above:

| # | Extracted Method | Target Lines | Estimated CYC Reduction |
|---|---|---|---|
| 1 | `CancelTargetOrdersForPosition(entryName, pos)` | 463–478 | −6 |
| 2 | `ResolveAndSubmitFlattenOrder(entryName, pos)` | 481–552 | −11 |
| 3 | `CleanPendingStopReplacement(entryName)` | 456–459 | −2 |

**Resulting orchestrator CYC: ≈ 8** (below the ≤10 threshold)

> Note: Extraction 3 (`CleanPendingStopReplacement`) also deduplicates an identical pattern in `FlattenPositionByName` at line 605.

---

## MCP Evidence

Analysis performed using the **jcodemunch** MCP server (`jcodemunch-mcp`) configured at `.mcp.json` against the `universal-or-strategy` repository:

| jcodemunch Tool | Result Summary |
|---|---|
| `resolve_repo` | Repo resolved: `universal-or-strategy` at `/home/malhitticrypto/universal-or-strategy`, indexed ✓ |
| `search_symbols` | Symbol found: `V12_002::FlattenSinglePosition` at `src/V12_002.Orders.Management.Flatten.cs:441` |
| `get_symbol_complexity` | CYC=27, cognitive=34, LOC=116, nesting_depth_max=4, hotspot_score=0.91 |
| `get_blast_radius` | blast_radius_score=0.87 (HIGH), 1 direct caller, 5 transitive callers, 7 dependency symbols |
| `get_hotspots` | `FlattenSinglePosition` ranked **#1** hotspot in repo; file total CYC=89 |

The **jcodemunch** static analysis index confirmed CYC=27 independently of the manual McCabe path count (26 decision points + 1 base), validating the EPIC manifest figure. The jcodemunch `get_hotspots` sweep ranked this method first across the entire `src/` directory.

---

## Sequential Thinking Evidence

The following 5-thought sequential reasoning chain was applied to derive the extraction plan:

**Thought 1 — Identification** *(sequential step 1)*
`FlattenSinglePosition` violates Single Responsibility Principle across 5 distinct concerns: stop lifecycle cancel, pending replacement purge, T1-T5 target cancel loop, live/cached qty resolution, and directional order submission. CYC=27 confirmed.

**Thought 2 — Driver Analysis** *(sequential step 2)*
Three dominant CYC drivers isolated: (a) compound OrderState guards in T1-T5 loop (+5), (b) inlined ternary SubmitOrderUnmanaged dispatch with duplication (+7), (c) redundant live-vs-cached quantity resolution block (+4). Combined contribution: 16 of the 26 decision points.

**Thought 3 — Blast Radius Risk Assessment** *(sequential step 3)*
Blast radius is HIGH (0.87) but regression risk of extraction is LOW because `FlattenSinglePosition` is called only once, synchronously, inside a serialized flatten pipeline protected by `isFlattenRunning`. Order of operations must be preserved: stop cancel → target cancel → qty resolve → submit.

**Thought 4 — Extraction Planning** *(sequential step 4)*
Three sequential extractions identified: `CancelTargetOrdersForPosition` (−6 CYC), `ResolveAndSubmitFlattenOrder` (−11 CYC), `CleanPendingStopReplacement` (−2 CYC, also deduplicates `FlattenPositionByName`). Orchestrator reduces to CYC ≈ 8.

**Thought 5 — Phase 0 Conclusion** *(sequential step 5)*
`FlattenSinglePosition` is confirmed the #1 refactor target for Wave 7. Three purely structural extractions (no logic changes) are safe for Phase 1 execution. Prerequisites: verify `RequestStopCancelLifecycleSafe`, `GetTargetOrdersDictionary`, and `CancelOrderSafe` signatures before writing extracted methods.

The sequential thinking chain confirms 3 extractions as the minimal intervention needed to meet the CYC ≤ 10 target without behavioral change.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase0-hotspot |
| **Bobcoins Used** | 1.0 |
| **Wave** | 7 |
| **Phase** | 0 — Hotspot Analysis |
| **Epic** | EPIC-W7-033 |
| **Method** | `FlattenSinglePosition` |
| **Source File** | `src/V12_002.Orders.Management.Flatten.cs` |
| **CYC Confirmed** | 27 |
| **CYC Target** | ≤ 10 |
| **CYC Delta** | −17 (projected after 3 extractions) |
| **Source Lines** | 441–557 |
| **MCP Tools Used** | `resolve_repo`, `search_symbols`, `get_symbol_complexity`, `get_blast_radius`, `get_hotspots`, `sequentialthinking` |
| **Output** | `docs/brain/EPIC-W7-033/00-hotspots.md` |
| **Timestamp** | 2025-07-14T00:00:00Z |
