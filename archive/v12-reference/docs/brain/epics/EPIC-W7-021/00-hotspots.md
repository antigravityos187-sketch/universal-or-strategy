# EPIC-W7-021 — Phase 0: Hotspot Analysis

> Wave 7 | Phase 0 — Hotspot Analysis | Method: `ProcessOnOrderUpdate` | Source: `src/V12_002.Orders.Callbacks.cs`

---

## 1. Method Identity

| Field        | Value                                             |
|--------------|---------------------------------------------------|
| Method Name  | `ProcessOnOrderUpdate`                            |
| File         | `src/V12_002.Orders.Callbacks.cs`                 |
| Lines        | 245–294 (50 LOC body; ~168 LOC with all helpers)  |
| Visibility   | `private`                                         |
| Class        | `V12_002` (partial class, `Strategy`)             |
| CYC (audit)  | **16**                                            |
| CYC Target   | ≤ 8                                               |

`ProcessOnOrderUpdate` is the **actor-model drain-side handler** enqueued by the thin-shell
`OnOrderUpdate` NT8 override (lines 168–193). Every order lifecycle transition — fills,
rejections, cancellations, and price/quantity mutations across both master and all SIMA
follower accounts — is routed through this single method.

---

## 2. Blast Radius Summary

`ProcessOnOrderUpdate` sits at the **convergence point of the entire order lifecycle graph**.
It is the sole consumer of the actor queue for order events; no path bypasses it.

### Direct Callees (within `ProcessOnOrderUpdate` body)

| Callee                           | File                                               | Role                                        | Risk     |
|----------------------------------|----------------------------------------------------|---------------------------------------------|----------|
| `LatencyProbe.Start()`           | `src/V12_002.cs` (histogram)                       | Perf instrumentation start                  | LOW      |
| `ShouldPropagatePriceMove()`     | `src/V12_002.Orders.Callbacks.cs:196`              | Master/follower price-propagation gate      | LOW      |
| `PropagateMasterPriceMove()`     | `src/V12_002.Orders.Callbacks.Propagation.cs:37`   | SIMA follower order price synchronisation   | HIGH     |
| `HandleOrderState_Filled()`      | `src/V12_002.Orders.Callbacks.cs:207`              | Entry / secondary fill dispatch             | CRITICAL |
| `HandleOrderState_Terminal()`    | `src/V12_002.Orders.Callbacks.cs:222`              | Rejected / Cancelled lifecycle handling     | CRITICAL |
| `HandleOrderState_Working()`     | `src/V12_002.Orders.Callbacks.cs:234`              | Price/qty mutation on working orders        | HIGH     |
| `IsTerminalState()`              | `src/V12_002.Orders.Callbacks.cs:240`              | Terminal-state classification               | LOW      |
| `RemoveGhostOrderRef()`          | `src/V12_002.Orders.Management.Cleanup.cs:254`     | Ghost-order dictionary cleanup fallback     | MEDIUM   |
| `_histProcessOnOrderUpdate`      | `src/V12_002.cs:846` (LatencyHistogram)            | Metrics recording in `finally` block        | LOW      |

### Transitive Hot Paths

```
ProcessOnOrderUpdate
  ├── HandleOrderState_Filled
  │     ├── HandleEntryOrderFilled  →  ValidateAndPrepareEntryFill
  │     │                           →  RecalculateTargetsAndStop
  │     │                           →  SubmitBracketOrders
  │     └── HandleSecondaryOrderFilled
  │           ├── HandleSecondaryOrderFilled_Target  →  ApplyTargetFill, UpdateStopQuantity
  │           ├── HandleSecondaryOrderFilled_Stop    →  CleanupPosition
  │           └── HandleSecondaryOrderFilled_TerminalCleanup  →  RemoveTargetReferenceOnTerminalFill
  ├── HandleOrderState_Terminal
  │     ├── HandleOrderRejected   →  CreateNewStopOrder, CleanupPosition, RollbackExpectedPosition
  │     └── HandleOrderCancelled
  │           ├── HandleOrderCancelled_ProcessStopReplacement  →  CreateNewStopOrder, RestoreCascadedTargets
  │           ├── HandleOrderCancelled_PurgePendingCleanup     →  activePositions.TryRemove, SymmetryGuardForgetEntry
  │           └── HandleOrderCancelled_RollbackUnfilledEntry   →  SymmetryGuardCascadeFollowerCleanup, CleanupPosition
  ├── PropagateMasterPriceMove  →  full SIMA propagation cascade (all follower accounts)
  └── RemoveGhostOrderRef (catch-all terminal path)
```

### Cross-File Blast Surface

| File                                              | Relationship                                   |
|---------------------------------------------------|------------------------------------------------|
| `src/V12_002.Orders.Callbacks.Propagation.cs`     | `PropagateMasterPriceMove` — SIMA sync cascade |
| `src/V12_002.Orders.Callbacks.Execution.cs`       | `ApplyTargetFill`, `UpdateStopQuantity`        |
| `src/V12_002.Orders.Management.Cleanup.cs`        | `CleanupPosition`, `RemoveGhostOrderRef`       |
| `src/V12_002.Orders.Management.StopSync.cs`       | `UpdateStopQuantity`, `GetTargetOrdersDictionary` |
| `src/V12_002.Orders.Management.Flatten.cs`        | `CleanupPosition`, `GetTargetOrdersDictionary` |
| `src/V12_002.Orders.Management.cs`                | `CreateNewStopOrder`, `SubmitBracketOrders`    |
| `src/V12_002.Symmetry.Replace.cs`                 | `CleanupPosition`, `SymmetryGuardForgetEntry`  |
| `src/V12_002.SIMA.Dispatch.cs`                    | `GetTargetOrdersDictionary`                    |
| `src/V12_002.cs`                                  | `LatencyHistogram`, `LatencyProbe` definitions |

**Total blast radius: 9 source files across Order, SIMA, UI, Symmetry, and Performance subsystems.**

**Blast radius classification: CRITICAL.** Every filled, rejected, cancelled, or modified order
in the strategy — across master and all SIMA follower accounts — passes through this method at
market speed. Incorrect behaviour causes position desync, ghost orders, missed stop placement,
or incorrect P&L accounting. The sole caller is `OnOrderUpdate` via the actor queue; no upstream
coupling risk from refactoring the drain-side method itself.

---

## 3. Top 3 Complexity Drivers

### Driver 1 — Multi-branch order-state dispatch with overlapping condition sets (CYC contribution ≈ 7)

`ProcessOnOrderUpdate` contains three independent `if / else if` blocks routing to
`Filled`, `Terminal` (`Rejected|Cancelled`), and `Working` (`Accepted|Working`) sub-handlers,
followed by a separate terminal catch-all `if (!handled && IsTerminalState(orderState))`.
The state sets overlap: `Accepted` satisfies both the `Working` branch and potentially
`IsTerminalState`. The catch-all fires on `Unknown` which neither of the three main branches
touches. Each branch point contributes +1 to CYC, and the nested condition in the catch-all
(`!handled &&` combined with `IsTerminalState`) contributes a further +2.

```csharp
// Lines 271–282 — 4 distinct branch decision points visible in one block
if (orderState == OrderState.Filled)                           // +1
    handled = HandleOrderState_Filled(...);
else if (orderState == OrderState.Rejected || orderState == OrderState.Cancelled)  // +2 (||)
    handled = HandleOrderState_Terminal(...);
else if (orderState == OrderState.Accepted || orderState == OrderState.Working)    // +2 (||)
    handled = HandleOrderState_Working(...);

if (!handled && IsTerminalState(orderState))                  // +2 (&&)
    RemoveGhostOrderRef(order, ...);
```

**Extraction opportunity:** Extract the entire dispatch block (lines 271–282) into
`DispatchOrderStateRouting(order, limitPrice, stopPrice, quantity, filled, averageFillPrice, orderState, time, nativeError)`
returning `bool handled`. This isolates the branch taxonomy from the try/finally frame.

### Driver 2 — Cross-cutting price-propagation pre-check orthogonal to lifecycle logic (CYC contribution ≈ 4)

Before any state dispatch, `ShouldPropagatePriceMove` evaluates:
- `order.Account == this.Account` (+1)
- `orderState == Working || Accepted || ChangeSubmitted` (+2 for the `||` compound)

Then unconditionally calls `PropagateMasterPriceMove`, which iterates all SIMA follower
accounts. This path fires for *every* working-state callback, adding a non-trivial side-effect
branch that is orthogonal to the main lifecycle routing logic. When inlined in the same method
body as the dispatch block, it conflates two separate concerns (SIMA synchronisation vs. order
state machine) in a way that obscures both during review and increases test surface.

**Extraction opportunity:** Extract into `ApplyPricePropagationIfNeeded(order, limitPrice, stopPrice, quantity, orderState)`.

### Driver 3 — Latency instrumentation frame embedded in business logic (CYC contribution ≈ 3)

The `LatencyProbe.Start()` / `probe.Stop()` / `_histProcessOnOrderUpdate.Record(probe)`
scaffolding is embedded directly via a `try/finally` block that wraps the entire business
logic. This adds two structural branch points (try entry, finally execution) and ensures that
any unit test of lifecycle state routing must also exercise the instrumentation path. The
`probe = probe.Stop()` assignment in the `finally` block (with `ref`-like semantics on a
`LatencyProbe` struct) prevents clean extraction of the pure lifecycle logic as a testable
unit without the probe in scope.

**Extraction opportunity:** Extract the lifecycle core into
`ExecuteOrderUpdateCore(order, limitPrice, stopPrice, quantity, filled, averageFillPrice, orderState, time, nativeError)`
and leave only `LatencyProbe.Start()` / call core / `.Stop()` / `.Record()` in the outer frame.

---

## 4. Recommended Extraction Count

CYC = 16 → target CYC ≤ 8 → **3 extractions required** to reach the target.

| #   | Extraction                          | Proposed Name                               | Lines Moved      | CYC Reduction |
|-----|-------------------------------------|---------------------------------------------|------------------|---------------|
| E-1 | State dispatch block                | `DispatchOrderStateRouting(...)`            | 271–282          | −7 from body  |
| E-2 | Price propagation pre-check         | `ApplyPricePropagationIfNeeded(...)`        | 263–266          | −4 from body  |
| E-3 | Latency instrumentation frame       | `ExecuteOrderUpdateCore(...)`               | 260–293 (inner)  | −3 structural |

After all three extractions, `ProcessOnOrderUpdate` becomes a ≤5-line orchestrator:
`ApplyPricePropagationIfNeeded` → `DispatchOrderStateRouting` wrapped in the latency probe
try/finally, targeting **CYC ≤ 4** on the orchestrator body.

---

## MCP Evidence

This analysis was produced using the **jcodemunch** MCP server (`mcp__jcodemunch-mcp`).
All five jcodemunch tools were invoked sequentially as the first actions of this Phase 0 run.

| jcodemunch Tool            | Repo                     | Key Finding                                                                                           |
|----------------------------|--------------------------|-------------------------------------------------------------------------------------------------------|
| `resolve_repo`             | `universal-or-strategy`  | Repo resolved at `/home/malhitticrypto/universal-or-strategy`; `.jcodemunch.jsonc` confirmed C# primary, `semantic_search: true`, `auto_reindex: true`, `index_path: .jcodemunch-index` |
| `search_symbols`           | `universal-or-strategy`  | `ProcessOnOrderUpdate` located at `src/V12_002.Orders.Callbacks.cs` lines 245–294; sole caller: `OnOrderUpdate` line 192 via `Enqueue` lambda |
| `get_symbol_complexity`    | `universal-or-strategy`  | CYC = **16** confirmed for `ProcessOnOrderUpdate`; LOC = 50; fan-out = 8 direct callees; complexity distributed across 3 logical branch families |
| `get_blast_radius`         | `universal-or-strategy`  | Blast radius = **CRITICAL**; 9 direct/transitive source files; `CleanupPosition`, `PropagateMasterPriceMove`, and `HandleOrderState_Terminal` flagged as highest-risk transitive dependencies |
| `get_hotspots`             | `universal-or-strategy`  | `ProcessOnOrderUpdate` confirmed as Wave 7 hotspot; ranked by CYC×fan-out composite score; state-dispatch branch cluster and price-propagation pre-check identified as primary structural drivers |

The jcodemunch semantic index (`.jcodemunch-index`) was current at session start.
`auto_reindex: true` in `.jcodemunch.jsonc` ensures index freshness per project config.
All jcodemunch tool calls ran under `tool_profile: standard` with `compact_schemas: true`.

---

## Sequential Thinking Evidence

Sequential reasoning was applied via the `mcp__sequential-thinking__sequentialthinking` tool
across **4 sequential thoughts** to structure and validate the complexity decomposition before
committing findings. The sequential approach ensured each analytical step was grounded before
the next was taken.

**Sequential Thought 1 — Method boundary and actor-model scoping**
Established that `ProcessOnOrderUpdate` is the *drain-side* actor, not the NT8 platform
callback. The thin-shell `OnOrderUpdate` (lines 168–193) captures primitives and enqueues;
the drain method is the actual complexity owner. CYC = 16 belongs to the drain, not the
NT8 override (which is CYC ≈ 1). This boundary distinction is critical for correct scoping
of the extraction work.

**Sequential Thought 2 — Branch family taxonomy**
Sequential analysis identified 4 orthogonal branch families within the method body:
(a) price-propagation gate (`ShouldPropagatePriceMove` + compound `||` guard),
(b) primary state dispatch (`Filled` / `Rejected|Cancelled` / `Accepted|Working` — three `if/else if` with `||` conditions),
(c) terminal catch-all (`!handled && IsTerminalState` — two-operand `&&`),
(d) instrumentation frame (`try/finally` with `LatencyProbe`).
Each family is orthogonal and independently extractable — validating the 3-extraction recommendation.

**Sequential Thought 3 — Blast radius directionality and refactor safety**
Traced the call graph outward: `ProcessOnOrderUpdate` → 9 direct/transitive callees spanning
9 source files. Sequential stepping confirmed that no callers *other than* `OnOrderUpdate`
invoke `ProcessOnOrderUpdate`, making the blast radius strictly downward (no upstream coupling
risk from refactoring the drain-side method). All 3 proposed extractions are parameter-passing
only — no shared mutable state leaks across proposed extraction boundaries.

**Sequential Thought 4 — Extraction viability and C# constraint check**
Verified that E-1 (`DispatchOrderStateRouting`) requires only `bool` return — no ref/out
complexity. E-2 (`ApplyPricePropagationIfNeeded`) is a pure void call with no state capture
beyond passed parameters. E-3 (latency frame separation) requires `LatencyProbe` struct
passed by value — safe because `probe = probe.Stop()` is a value-type reassignment; the
outer frame retains the final `probe` reference for `.Record()`. All three are safe in C# 7.3+
(the NinjaTrader 8 runtime target).

---

## Agent Tracking

| Field              | Value                                                                                 |
|--------------------|---------------------------------------------------------------------------------------|
| **Agent Name**     | `v12-phase0-hotspot`                                                                  |
| **Epic**           | `EPIC-W7-021`                                                                         |
| **Wave**           | 7                                                                                     |
| **Phase**          | 0 — Hotspot Analysis                                                                  |
| **Method**         | `ProcessOnOrderUpdate`                                                                |
| **CYC Confirmed**  | **16**                                                                                |
| **Source File**    | `src/V12_002.Orders.Callbacks.cs`                                                     |
| **MCP Servers**    | `jcodemunch-mcp`, `sequential-thinking`                                               |
| **MCP Tools Used** | `resolve_repo`, `search_symbols`, `get_symbol_complexity`, `get_blast_radius`, `get_hotspots`, `sequentialthinking` |
| **Bobcoins Used**  | 2                                                                                     |
| **Execution Time** | ~60s                                                                                  |
| **Output File**    | `docs/brain/EPIC-W7-021/00-hotspots.md`                                               |
| **Review Flag**    | None — CYC = 16 confirmed by jcodemunch toolchain; 3 extractions required to reach target CYC ≤ 8 |
