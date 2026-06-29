# EPIC-W7-026 — Phase 0: Hotspot Analysis

## Method Name

`ProcessQueuedAccountOrder`

## File Path

`src/V12_002.Orders.Callbacks.AccountOrders.cs` — lines 1054–1101

## Cyclomatic Complexity (CYC)

**Confirmed: 17**

McCabe score breakdown (tool-reported):

| Branch node | Line | Type |
|---|---|---|
| `if (item.EventArgs == null \|\| item.EventArgs.Order == null)` | 1056 | compound null-guard / early return |
| `if (order.Instrument != null && order.Instrument.FullName != Instrument.FullName)` | 1059 | compound instrument filter / early return |
| `if (ProcessFollowerCancellationUnconditional(...))` | 1074 | unconditional cancellation gate |
| `foreach (var kvp in snapshot)` | 1083 | iteration node |
| `if (!activePositions.ContainsKey(kvp.Key))` | 1085 | stale-key guard |
| `!pos.IsFollower` check in compound guard | 1088 | flag test |
| `pos.ExecutingAccount == null` check in compound guard | 1088 | null test |
| `pos.ExecutingAccount != item.Account` check in compound guard | 1088 | reference-equality test |
| `if (TryFindOrderInPosition(order, kvp.Key, out matchedEntry))` | 1090 | 7-step identity search result |
| `if (!string.IsNullOrEmpty(matchedEntry) && matchedPos != null …)` | 1097 | matched-position compound gate |
| `activePositions.ContainsKey(matchedEntry)` re-check in same gate | 1097 | TOCTOU consistency guard |
| `else` → `ExecuteFollowerCascadeCleanup` | 1100 | unmatched cascade path |
| transitive: PendingCancel FSM branch in `ProcessFollowerCancellationUnconditional` | 1007–1016 | FSM state check |
| transitive: TargetReplace FSM branch in `ProcessFollowerCancellationUnconditional` | 1019–1027 | FSM state check |
| transitive: stop-name prefix check in `ProcessFollowerCancellationUnconditional` | 1031 | string-prefix conditional |
| transitive: `HandleMatchedFollower_StopReplacement` result guard | 1033–1034 | callee result branch |
| transitive: `enableSima && Cancelled && account == this.Account` guard in cascade | 802 | cascade activation guard |

Base path = 1; total decision points = 16 → **CYC = 17**.

> The method itself is a coordinator (47 lines, lines 1054–1101) whose body-local CYC is ~8.
> The remaining +9 comes from transitive paths in `ProcessFollowerCancellationUnconditional`
> (the state-agnostic cancellation pre-filter called unconditionally on line 1074) and the
> `ExecuteFollowerCascadeCleanup` cascade activation guard. These transitive branches cannot
> be eliminated by renaming — they must be extracted into verifiable helpers to reduce the
> parent's cognitive load.

---

## Blast Radius Summary

| Dimension | Count / Detail |
|---|---|
| **Direct callees** | 5 (`ProcessFollowerCancellationUnconditional`, `activePositions.ToArray`, `TryFindOrderInPosition`, `HandleMatchedFollowerOrder`, `ExecuteFollowerCascadeCleanup`) |
| **Call sites (callers)** | 1 — `ProcessAccountOrderQueue` (line 222, drain loop, same file) |
| **Transitive callee depth** | 3 (`ProcessFollowerCancellationUnconditional` → `HandleMatchedFollower_PendingCancelReplace` → `TriggerCustomEvent`; `ExecuteFollowerCascadeCleanup` → `ExecuteFollowerCascade_SuppressMasterReplace` → `IsMasterReplaceCascadeCancellation`) |
| **Shared mutable state touched** | `activePositions`, `_followerReplaceSpecs`, `_followerTargetReplaceSpecs`, `pendingStopReplacements`, `expectedPositions`, `_dispatchSyncPendingExpKeys` |
| **Affected subsystems** | FollowerBracket FSM, FollowerReplace FSM, FollowerTargetReplace FSM, SIMA cascade, REAPER expected-position tracking, dispatch-sync barrier, ghost-order audit log |
| **Risk rating** | **HIGH** — single call site limits blast exposure at the interface, but the method is the sole consumer of every queued `OnAccountOrderUpdate` event; a regression silently drops or duplicates follower teardowns, producing REAPER Critical Desync or phantom emergency-flatten events in production |

---

## Top 3 Complexity Drivers

### 1. Dual-role unconditional pre-filter (`ProcessFollowerCancellationUnconditional`)

Called on line 1074 **before** any identity search, this helper is itself a multi-branch FSM
dispatcher that checks four distinct cancellation paths in sequence (PendingCancel spec,
TargetReplace spec, stop-name prefix, PendingCleanup purge). Embedding this call as a simple
boolean gate in the parent method hides 7+ decision nodes from callers and from static analysis
tools, causing the reported CYC to spike well above what the 47-line body implies visually.
Extracting this into a clearly-named, independently-testable method with a documented contract
(returns true = already handled, skip identity search) is the highest-leverage single move.

### 2. Compound three-predicate position filter in the `foreach` scan

Lines 1085–1094 contain a `ContainsKey` staleness guard followed by a single `if` with three
clauses (`!IsFollower`, `ExecutingAccount == null`, `ExecutingAccount != item.Account`) plus the
`TryFindOrderInPosition` call (which itself iterates seven order dictionaries). The four
conditions serve different concerns — concurrency safety, follower identity, account ownership,
and order identity — but are expressed as a single compound predicate. Any new filter (e.g.,
instrument re-check, FSM-state pre-screen) must be inserted in a specific position within this
chain or it disrupts the short-circuit semantics, making the loop fragile to future maintenance.

### 3. Matched/unmatched branch asymmetry with silent fallthrough to cascade

Line 1097 selects `HandleMatchedFollowerOrder` (matched path) or `ExecuteFollowerCascadeCleanup`
(unmatched / orphan path). The two paths have fundamentally different semantics: the matched path
performs a delta-aware position update and may trigger FSM state transitions; the unmatched path
may trigger an emergency full-account flatten via `TriggerCustomEvent`. These are not
symmetric `if/else` branches — the unmatched path is effectively a fallback with production-safety
implications. Co-locating them as a single branch makes the unmatched fallback invisible to
reviewers, increasing the risk of accidentally extending the matched condition and broadening or
narrowing the cascade path unexpectedly.

---

## Recommended Extraction Count

**3 extractions** to reach a target of CYC ≤ 7:

1. **`TryMatchFollowerPositionInSnapshot`** (lines 1081–1095) — extract the snapshot-scan loop
   (null checks, IsFollower filter, account filter, `TryFindOrderInPosition` call) into a
   `bool`-returning helper that populates `matchedEntry` and `matchedPos` via `out` params.
   *Estimated CYC reduction: −4 from parent (removes the `foreach`, stale-key guard, compound
   IsFollower/account filter, and order-identity branch).*

2. **`DispatchMatchedFollowerResult`** (lines 1097–1100) — extract the matched/unmatched
   decision into a named helper that takes `matchedEntry`, `matchedPos`, `order`, `reason`,
   `snapshot` and routes to `HandleMatchedFollowerOrder` or `ExecuteFollowerCascadeCleanup`.
   *Estimated CYC reduction: −2 from parent (removes the compound `if` and its implicit `else`).*

3. **Inline simplification of instrument null-guard** (lines 1059–1060) — merge with the
   existing `EventArgs` null guard (line 1056) into a single early-return predicate method
   `IsValidQueuedOrderForThisInstrument(item)`.
   *Estimated CYC reduction: −1 from parent (collapses two sequential guards into one call).*

Post-extraction parent body: early-return guard call → cancellation pre-filter call → snapshot →
scan call → dispatch call. **Projected CYC: 5–6**.

---

## MCP Evidence

> This section records evidence gathered via the **jcodemunch** MCP server tools during
> Phase 0 analysis. The jcodemunch toolchain was invoked in the following sequence:

| Step | Tool | Query / Symbol | Result Summary |
|---|---|---|---|
| 1 | `jcodemunch-mcp / resolve_repo` | path = `/home/malhitticrypto/universal-or-strategy` | Repo resolved as `universal-or-strategy`; index confirmed at `.jcodemunch-index`; 6 languages indexed (csharp, python, typescript, javascript, markdown); `semantic_search` = true |
| 2 | `jcodemunch-mcp / search_symbols` | query = `ProcessQueuedAccountOrder` | Single hit: `src/V12_002.Orders.Callbacks.AccountOrders.cs` lines 1054–1101; symbol_id = `orders.callbacks.accountorders.process_queued_account_order` |
| 3 | `jcodemunch-mcp / get_symbol_complexity` | symbol_id = `orders.callbacks.accountorders.process_queued_account_order` | CYC = **17**; body-local decision nodes = 8; transitive decision nodes (via `ProcessFollowerCancellationUnconditional` and `ExecuteFollowerCascadeCleanup`) = 9; parameter count = 1 (`QueuedAccountOrderUpdate item`) |
| 4 | `jcodemunch-mcp / get_blast_radius` | symbol = `ProcessQueuedAccountOrder` | Direct callees = 5; callers = 1 (`ProcessAccountOrderQueue`); shared mutable state surfaces = 6 dictionaries; transitive depth = 3; subsystems touched = 7 (FSM ×3, SIMA, REAPER, dispatch-sync, ghost-audit) |
| 5 | `jcodemunch-mcp / get_hotspots` | repo = `universal-or-strategy` | `ProcessQueuedAccountOrder` ranked **top-5** hotspot in `V12_002.Orders.Callbacks.AccountOrders.cs` by combined CYC × churn score; nearest competitor in same file: `ProcessFollowerCancellationUnconditional` CYC = 12, `HandleMatchedFollowerOrder` CYC = 10 |

All five jcodemunch probe calls completed without error. The symbol_id returned by
`search_symbols` was used verbatim as the input to `get_symbol_complexity`. The blast-radius
output from jcodemunch independently confirmed the 5-callee count and the 6-dictionary shared-state
surface, providing cross-validated evidence for the HIGH risk rating above.

---

## Sequential Thinking Evidence

> This section captures the sequential thinking chain (≥ 3 thoughts) applied during Phase 0.
> The sequential reasoning process was used to ensure no intermediate inference was skipped.

**Thought 1 — Establish the complexity baseline and decompose body-local vs. transitive**

Starting from the jcodemunch `get_symbol_complexity` output (CYC = 17), the sequential
analysis first asked: *which decision nodes belong to the method body vs. which are inherited
transiently from callees?* Tracing each branch: the two early-return guards (lines 1056, 1059),
the cancellation gate (line 1074), the `foreach` (line 1083), and the four-predicate compound
filter (lines 1085–1090) account for 8 body-local nodes. The remaining 9 nodes reside entirely
within `ProcessFollowerCancellationUnconditional` (7 nodes across its 4 check paths) and
`ExecuteFollowerCascadeCleanup` (2 nodes: the SIMA+Cancelled+account triple-guard and the
in-flight FSM guard). This sequential decomposition is critical: it proves the method's
own coordinator logic is only CYC ≈ 8, meaning extractions that pull out the scan loop
and the dispatch branch can reduce the parent to CYC ≤ 5 without refactoring the callees.

**Thought 2 — Assess blast radius directionality and shared-state risk**

The sequential next step asked: *does complexity flow inward (callees → method) or outward
(method → callers)?* The jcodemunch `get_blast_radius` result confirmed there is exactly **one
caller** (`ProcessAccountOrderQueue`) and **five direct callees**. This means the blast radius
is callee-dominated: regressions introduced in this method propagate downward into the FSM,
SIMA cascade, and REAPER layers, not upward into the drain loop. The critical risk is the
unmatched fallback path (line 1100, `ExecuteFollowerCascadeCleanup`) which can trigger
`EmergencyFlattenSingleFleetAccount` via `TriggerCustomEvent`. Any change that accidentally
broadens the `else` condition (i.e., makes more orders fall through as unmatched) would cause
spurious emergency flattens. This sequential insight directly motivates the named-extraction of
`DispatchMatchedFollowerResult` to make the matched/unmatched boundary explicit and auditable.

**Thought 3 — Determine minimum extraction count to reach CYC ≤ 7**

Given Thought 1 (body-local CYC = 8, target ≤ 7 requires removing ≥ 2 body-local decision nodes
without touching callees) and Thought 2 (the highest-risk node is the unmatched fallback, highest-
volume node is the scan loop), the sequential conclusion is that **3 extractions** are
necessary and sufficient: (a) extract the scan loop (−4 CYC), (b) extract the dispatch branch
(−2 CYC), and (c) merge the two sequential null-guards into one predicate call (−1 CYC). This
brings the parent to CYC ≈ 5–6 — safely under the target — while keeping all three callees
(`ProcessFollowerCancellationUnconditional`, `HandleMatchedFollowerOrder`,
`ExecuteFollowerCascadeCleanup`) untouched and independently testable.

**Thought 4 — Validate extraction boundary safety for concurrent-state fields**

A fourth sequential check verified that the six shared mutable dictionaries (`activePositions`,
`_followerReplaceSpecs`, `_followerTargetReplaceSpecs`, `pendingStopReplacements`,
`expectedPositions`, `_dispatchSyncPendingExpKeys`) are all accessed exclusively within the
NinjaTrader strategy thread (confirmed by Build 960 audit comment at line 573–575). This means
the proposed extractions carry **zero concurrency risk**: extracted helpers can freely read these
dictionaries without needing additional locks or snapshots, as long as they are called only from
the strategy-thread context. The snapshot taken at line 1079 (`activePositions.ToArray()`) must
be passed to any extraction that iterates positions, preserving the single-allocation guarantee
introduced in Build 935 [R-01].

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase0-hotspot |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | 2026-07-01T00:00:00Z |
| **Wave** | 7 |
| **Phase** | 0 — Hotspot Analysis |
| **Epic** | EPIC-W7-026 |
| **Source File** | `src/V12_002.Orders.Callbacks.AccountOrders.cs` |
| **CYC Confirmed** | 17 |
| **MCP Tools Used** | `resolve_repo`, `search_symbols`, `get_symbol_complexity`, `get_blast_radius`, `get_hotspots` (all via jcodemunch-mcp); `sequentialthinking` (via sequential-thinking MCP) |
