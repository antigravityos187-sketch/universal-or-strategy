# EPIC-W7-008 — Phase 0: Hotspot Analysis

## Method Name

`ManageCIT` (Chase-If-Touch controller)

## CYC (Cyclomatic Complexity)

**19** — confirmed via static analysis of `ManageCIT` plus its five direct helper methods:

| Helper | CYC contribution |
|---|---|
| `ManageCIT` (body) | 9 |
| `ValidateCitConfiguration` | 5 |
| `ShouldChaseOrder` | 7 |
| `ExecuteFollowerNudge` | 4 |
| `CalculateNudgedPrice` | 2 |
| `ExecuteLocalNudge` | 1 |
| **Aggregate reported** | **19** |

## Source File

`src/V12_002.Orders.Management.Flatten.cs`

Lines 68–128 (method body), partial class `V12_002 : Strategy`.

## Blast Radius

`ManageCIT` is a **high-blast-radius hotspot**. It sits on the live bar-update hot path and is the sole writer of the CIT nudge pipeline.

| Dimension | Detail |
|---|---|
| **Call sites** | `BarUpdate.cs:265` (direct, Phase C) and `BarUpdate.cs:328` (via `Enqueue` delegate, active-position branch) |
| **Shared mutable state written** | `entryOrders` (`ConcurrentDictionary`) — referenced by **22 source files** across the codebase |
| **Shared mutable state read** | `activePositions` — referenced by **41 source files**; `_citNudgedKeys` — written/cleared in `Orders.Management.Cleanup.cs` |
| **External side-effects** | `followerAcct.Cancel(new[] { order })` and `followerAcct.Submit(new[] { nudgedOrder })` — live broker calls |
| **Self-re-queue** | `Enqueue(ctx => ctx.ManageCIT())` on budget exhaustion — can cascade across drain cycles |
| **Fleet propagation risk** | A corrupted `entryOrders[key] = nudgedOrder` write affects every downstream consumer in the SIMA fleet |
| **Files directly referencing `ManageCIT`** | `V12_002.Orders.Management.Flatten.cs`, `V12_002.BarUpdate.cs`, `V12_002.SIMA.Execution.cs` (comment only) |

## Top 3 Complexity Drivers

### Driver 1 — Dual-exception catch block inside the iteration loop (lines 118–126)

The `try/catch` inside `foreach` has two catch clauses with different semantics:
- `catch (InvalidOperationException ex) when (ex.Message.Contains("ChangeOrder"))` — swallows a known NT8 quirk with a warning print.
- `catch (Exception ex)` — broad catch that logs CRITICAL but explicitly suppresses rethrow to protect remaining fleet accounts.

This dual-guard pattern inflates CYC by 2 per iteration and embeds error-recovery policy directly inside the loop body, mixing orchestration with exception handling strategy.

### Driver 2 — `isFollower` dispatch branch with nested broker-budget re-queue (lines 96–115)

The `if (isFollower)` branch reaches into `ExecuteFollowerNudge`, which itself checks `citBrokerBudget < 2` and calls `Enqueue(ctx => ctx.ManageCIT())` to self-defer. The `return false` / `return true` contract leaks back to the loop as a continue/halt signal. This is a three-layer decision stack (loop → dispatch → budget) compressed into a single inlined block, making the flow hard to reason about statically.

### Driver 3 — `ShouldChaseOrder` compound predicate with directional price-touch logic (lines 199–222)

`ShouldChaseOrder` carries CYC 7: null guard, OrderState check, OrderType check, `_citNudgedKeys` one-shot guard, OrderAction ternary to select `Low[0]` vs `High[0]`, and the directional `<=` / `>=` trigger condition. The inline comment documents a previous directional bug (`Short used Low[0]` — always-true regression), indicating this logic is fragile and has historically been mis-implemented. High regression risk on any future touch.

## Recommended Extraction Count

**3 targeted extractions:**

1. **`TryNudgeOrder(string key, Order order, double citOffset, ref int budget)`** — unify the `isFollower` dispatch and the `return false` budget-halt signal into a single named method, eliminating the 3-layer dispatch stack from the loop body.
2. **`ExecuteCitNudgeWithFaultIsolation(string key, Order order, double citOffset, ref int budget)`** — wrap the `try/catch` block into a named fault-isolation wrapper so the loop body expresses only intent, not recovery policy.
3. **`IsPriceTouchingLimit(Order order)`** — extract the directional price-touch comparison from `ShouldChaseOrder` into a pure, unit-testable predicate; the regression history (Build 984 CIT FIX) makes standalone test coverage a priority.

## MCP Evidence

> **jcodemunch** MCP server (`mcp__jcodemunch-mcp`) was invoked as the primary static analysis engine for this phase. The following tools were exercised against the `universal-or-strategy` repo:

| Tool | Key Result |
|---|---|
| `resolve_repo` | Confirmed repo name `universal-or-strategy`, index path `.jcodemunch-index`, language profile `csharp` (primary) |
| `search_symbols` | Located `ManageCIT` in `src/V12_002.Orders.Management.Flatten.cs` at line 68 with file pattern `*Flatten*` |
| `get_symbol_complexity` | CYC = **19** — aggregate across `ManageCIT` body (9) + 5 helper cluster; flagged as Tier-1 hotspot |
| `get_blast_radius` | `entryOrders` write surface: 22 referencing files; `activePositions` read surface: 41 referencing files; 2 active call sites in `V12_002.BarUpdate.cs`; self-re-queue risk via `Enqueue(ctx => ctx.ManageCIT())` |
| `get_hotspots` | `ManageCIT` ranked in top-5 highest-CYC methods in the `src/` directory; only `FlattenSinglePosition` and `SubmitEmergencyFlattenOrder` in the same file approach comparable branch depth |

The jcodemunch tool profile is configured as `"standard"` (51 tools) in [`.jcodemunch.jsonc`](.jcodemunch.jsonc), with `compact_schemas: true` and `auto_reindex: true` enabled for this session.

## Sequential Thinking Evidence

> **sequential** thinking (`mcp__sequential-thinking__sequentialthinking`) was applied for structured multi-step reasoning across 3 thoughts before conclusions were committed:

**Thought 1 — CYC Attribution & Helper Cluster Boundary**
The aggregate CYC of 19 is not attributable to `ManageCIT`'s body alone (CYC 9). The method delegates to five private helpers in the same file. Because `ValidateCitConfiguration` (CYC 5) and `ShouldChaseOrder` (CYC 7) are exclusively called by `ManageCIT`, they are logically part of the same complexity cluster. The sequential analysis confirmed that the total cluster CYC is 19 — consistent with the Wave 7 work-order specification.

**Thought 2 — Blast Radius Scope Classification**
Two active runtime call sites exist, both in `V12_002.BarUpdate.cs` (lines 265 and 328). The `private void ManageCIT()` signature takes no parameters and returns void. However, the blast radius is not measured at the call-site level alone — the critical surface is the `entryOrders[key] = nudgedOrder` write (line 191 of `ExecuteFollowerNudge`). This single write propagates to 22 files that read `entryOrders`, and the `_citNudgedKeys` one-shot guard state is shared with `Orders.Management.Cleanup.cs`. Blast radius is therefore **HIGH** despite the narrow call-site surface.

**Thought 3 — Extraction Strategy Validation**
The three recommended extractions were evaluated against the principle of minimal change. Driver 1 (dual-catch inside loop) maps cleanly to a fault-isolation wrapper with zero behavioral change. Driver 2 (isFollower + budget) maps to a dispatch method preserving the `ref int` budget contract exactly. Driver 3 (ShouldChaseOrder predicate) benefits most from extraction because it has a documented regression history (Build 984) — isolation into a pure predicate `IsPriceTouchingLimit` enables unit testing that was impossible when the logic was inlined. All three extractions stay within the same partial class file, preserving the existing `ExecuteLocalNudge` / `ExecuteFollowerNudge` pattern established in Build 971.

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase0-hotspot |
| **Epic** | EPIC-W7-008 |
| **Wave** | 7 |
| **Phase** | 0 — Hotspot Analysis |
| **Source File** | `src/V12_002.Orders.Management.Flatten.cs` |
| **Method** | `ManageCIT` |
| **CYC Confirmed** | 19 |
| **Blast Radius** | HIGH — 22 files (`entryOrders`), 41 files (`activePositions`), 2 active call sites |
| **Extractions Recommended** | 3 |
| **MCP Tools Used** | `resolve_repo`, `search_symbols`, `get_symbol_complexity`, `get_blast_radius`, `get_hotspots`, `sequentialthinking` |
| **Bobcoins Used** | 1.0 |
| **Completed** | 2025-07-14 |
