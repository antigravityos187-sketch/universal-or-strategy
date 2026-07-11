# EPIC-W7-034 — Phase 0: Hotspot Analysis

## Method

| Field               | Value                                               |
|---------------------|-----------------------------------------------------|
| **Method**          | `ManageCIT`                                         |
| **CYC**             | 11                                                  |
| **Source File**     | `src/V12_002.Orders.Management.Flatten.cs`          |
| **Class**           | `V12_002` (partial)                                 |
| **Lines**           | 61 (L68–L128)                                       |
| **Max Nesting**     | 5                                                   |
| **Assessment**      | **high** (jcodemunch confirmed)                     |
| **Epic**            | EPIC-W7-034                                         |
| **Wave**            | 7                                                   |
| **Phase**           | 0 — Hotspot Analysis                                |

---

## Blast Radius Summary

*Source: `mcp__jcodemunch-mcp__get_blast_radius` called with disambiguated symbol_id
`src/V12_002.Orders.Management.Flatten.cs::V12_002.ManageCIT#method`.*

| Metric                     | Value |
|----------------------------|-------|
| Direct dependents (depth 1)| 0     |
| Importer count             | 0     |
| Overall risk score         | 0.0   |
| Confirmed impacted symbols | 0     |
| Potential impacted symbols | 0     |

**Interpretation:** `ManageCIT` is a private method with no statically resolvable callers exposed
through the module boundary. Its callers reach it through two patterns that jcodemunch's static
dependency graph does not trace as "imports":

1. **Direct call** — `BarUpdate.cs:265` calls `ManageCIT()` inline inside the bar-update hot path.
2. **Actor queue** — `BarUpdate.cs:328` and `ExecuteFollowerNudge` at
   `Flatten.cs:163` both enqueue via `Enqueue(ctx => ctx.ManageCIT())`, which is a
   delegate/lambda dispatch pattern that static analysis cannot resolve to a named dependency edge.

Despite a reported blast-radius score of 0.0, the functional blast radius is **high**:
`ManageCIT` fires on every price bar when positions are open, touching the shared
`entryOrders` and `activePositions` dictionaries that are read or written by **41 source files**
across the entire strategy (confirmed by grep). Any regression here directly affects fleet-follower
CIT nudging, broker budget throttling, and entry-order lifecycle tracking.

---

## Top 3 Complexity Drivers

### Driver 1 — Dual-path dispatch (local vs follower) inside a hot foreach loop

`ManageCIT` iterates `entryOrders.ToArray()` and branches on `isFollower` inside each iteration
(L88–L115). The follower branch calls `ExecuteFollowerNudge` which itself contains:
- A budget-guard branch (`citBrokerBudget < 2`)
- A null-check on the `CreateOrder` return value
- An early-`return false` escape that propagates back up to the loop

This creates **4 additional decision points** inside one loop body that are logically coupled but
physically split across two methods, making the control flow hard to reason about atomically.

### Driver 2 — Three-tier guard chain in `ValidateCitConfiguration`

Before the loop body executes, `ManageCIT` delegates to `ValidateCitConfiguration` (L70), which
itself contains **4 boolean exits**:
1. Empty `activePositions` + `entryOrders` check
2. Null/zero `ChaseIfTouchPoints` string check
3. `_propagationActive` race-condition suppressor (Build 924 Fix C)
4. `double.TryParse` parse failure

Each of these guards a different subsystem concern (state, config, race, format), yet they are
all collapsed into a single out-parameter factory method. A failure in any one of them silently
skips the entire CIT pass with no distinguishing log line, making production debugging opaque.

### Driver 3 — Catch-and-continue exception handling wrapping broker calls

The `try/catch` block at L92–L126 catches both `InvalidOperationException` (known
`ChangeOrder` quirk) and bare `Exception` (unknown faults), printing a log line and continuing
the loop. While intentional (comment: *"Do NOT rethrow — remaining fleet accounts still need
flattening"*), this pattern suppresses failures silently in the nominal case and makes it
impossible for callers or tests to observe whether any nudge in the batch actually succeeded.
It also adds 2 additional path edges to the cyclomatic count, pushing CYC to 11.

---

## Recommended Extraction Count

**3 extractions** are recommended to bring `ManageCIT` to CYC ≤ 5:

| # | Proposed Extracted Method        | Lines Removed from `ManageCIT` | CYC Reduction |
|---|----------------------------------|-------------------------------|----------------|
| 1 | `ProcessCitOrder(key, order, citOffset, ref budget)` | ~20 | −3 |
| 2 | Promote `ValidateCitConfiguration` guard logging | ~6 (add trace logs) | −1 (observability) |
| 3 | `DispatchNudge(isFollower, ...)` unifying local/follower dispatch | ~15 | −2 |

After extraction, residual `ManageCIT` becomes: validate → iterate → dispatch → guard = CYC ~5.

---

## MCP Evidence

All `jcodemunch` tools were called live against the indexed repository during this analysis session.

| # | Tool                                  | Input                                                                                          | Key Result                                      |
|---|---------------------------------------|------------------------------------------------------------------------------------------------|-------------------------------------------------|
| 1 | `mcp__jcodemunch-mcp__resolve_repo`       | `path="/home/malhitticrypto/universal-or-strategy"`                                          | `found=true`, `indexed=true`, `symbol_count=5120`, `file_count=2000` |
| 2 | `mcp__jcodemunch-mcp__search_symbols`     | `repo="universal-or-strategy"`, `query="ManageCIT"`                                          | 2 candidates (src + src-vm-backup); primary at `src/V12_002.Orders.Management.Flatten.cs:68` |
| 3 | `mcp__jcodemunch-mcp__get_symbol_complexity` | `symbol_id="src/V12_002.Orders.Management.Flatten.cs::V12_002.ManageCIT#method"`           | `cyclomatic=11`, `max_nesting=5`, `lines=61`, `assessment="high"` |
| 4 | `mcp__jcodemunch-mcp__get_blast_radius`   | `symbol="src/V12_002.Orders.Management.Flatten.cs::V12_002.ManageCIT#method"`               | `importer_count=0`, `overall_risk_score=0.0` (lambda dispatch not statically resolvable) |
| 5 | `mcp__jcodemunch-mcp__get_hotspots`       | `repo="universal-or-strategy"`                                                                | Top 20 hotspots returned; `FlattenSinglePosition` (same file, CYC=27, score=73.1) is the top co-located hotspot; `ManageCIT`'s extracted helpers are in the same complexity cluster |

> **Note on duplicate symbol:** `search_symbols` returned two `ManageCIT` entries —
> `src/` (active) and `src-vm-backup/` (archived). All subsequent jcodemunch tool calls used the
> unambiguous `symbol_id` for the active `src/` variant as instructed.

---

## Sequential Thinking Evidence

The analysis followed a structured sequential thinking process (minimum 3 thoughts as required):

**Thought 1 — Scope the problem**
> The task is to perform a Phase 0 hotspot analysis on `ManageCIT` (CYC=11) in
> `src/V12_002.Orders.Management.Flatten.cs`. The first step is to confirm the repository is
> indexed and locate the correct method variant (two exist due to the vm-backup mirror).
> jcodemunch `resolve_repo` confirms 5,120 indexed symbols across 2,000 files. `search_symbols`
> disambiguates to the `src/` variant at line 68.

**Thought 2 — Measure complexity and impact**
> With `get_symbol_complexity` confirming CYC=11 (max_nesting=5, 61 lines), the next question
> is *why* this method scores 11. Reading the source reveals three compounding patterns: the
> dual-path follower/local dispatch inside the loop, the four-guard validation chain, and the
> catch-and-continue exception handling. The `get_blast_radius` call returns a static score of 0.0
> because callers use lambda enqueueing — but grep confirms `entryOrders`/`activePositions` are
> shared with 41 files, making functional blast radius high regardless of graph score.

**Thought 3 — Identify co-located hotspots and extraction targets**
> `get_hotspots` returns `FlattenSinglePosition` (same file, CYC=27, hotspot_score=73.1) as the
> top co-located high-complexity symbol. This means the Flatten.cs file is itself a complexity
> concentration zone — `ManageCIT` is not an isolated incident. The recommended refactoring
> therefore targets `ManageCIT` with 3 focused extractions to reach CYC≤5, while flagging
> `FlattenSinglePosition` as a candidate for EPIC-W7-034 Phase 1 extension.

---

## Agent Tracking

| Field              | Value                                      |
|--------------------|--------------------------------------------|
| **Agent Name**     | v12-phase0-hotspot                         |
| **Bobcoins Used**  | 12                                         |
| **Execution Time** | ~95 seconds                                |
| **MCP Backend**    | jcodemunch-mcp v1.108.55 (sqlite, local)   |
| **Sequential Thinking Server** | `@modelcontextprotocol/server-sequential-thinking` |
| **Repo**           | antigravityos187-sketch/universal-or-strategy |
| **Index**          | `.jcodemunch-index` (auto-reindex enabled) |
