# EPIC-W7-004 — Phase 0: Hotspot Analysis

## Method

`HandleFleetTargetFill` — `private void HandleFleetTargetFill(QueuedAccountExecution item, Order ocoOrder, Account ocoAcct, string ocoName)`

## CYC (Cyclomatic Complexity)

**34** — confirmed via jcodemunch `get_symbol_complexity` probe (see MCP Evidence below).

## Source File

`src/V12_002.UI.Compliance.cs` — lines 624–696

## Blast Radius Summary

| Caller | File | Relationship |
|---|---|---|
| `ProcessQueuedExecution_HandleFleetOCO` | `src/V12_002.UI.Compliance.cs:719` | Direct caller — dispatches on order-name prefix `T[n]_` |
| `ProcessQueuedExecution` | `src/V12_002.UI.Compliance.cs:799` | Indirect caller — routes all queued executions to OCO handler |
| `OnAccountExecutionUpdate` | `src/V12_002.UI.Compliance.cs:401` | Root trigger — enqueues `QueuedAccountExecution` items |

**Cross-file dependencies called from `HandleFleetTargetFill`:**

| Symbol | File |
|---|---|
| `ApplyTargetFill` | `src/V12_002.Orders.Callbacks.cs:47` |
| `CancelOrderOnAccount` | `src/V12_002.UI.Compliance.cs:573` |
| `activePositions` (ConcurrentDictionary) | `src/V12_002.cs` |
| `ocoAcct.Orders` (NinjaTrader Cbi) | External — `NinjaTrader.Cbi.Account` |

Blast radius spans **3 source files** plus the NinjaTrader platform broker layer. Any extraction must preserve the `out`-parameter contract of `ApplyTargetFill` and the `ocoAcct.Orders.ToArray()` defensive enumeration pattern.

## Top 3 Complexity Drivers

### 1 — String-parsed key decomposition (lines 626–631)
The method manually parses the OCO order name (`ocoName`) character-by-character to recover `tgtNum` and `tgtEntryKey`. This two-pass string slicing (`Substring`, `LastIndexOf`) encodes implicit knowledge of the naming convention (`T{n}_{entryKey}_{suffix}`) directly as imperative control flow, inflating branch count and making the logic opaque.

### 2 — Nested guard chain with dual output paths (lines 633–694)
A three-level nesting depth (`TryGetValue` guard → `tgtAlreadyProcessed` branch → `tgtRemaining <= 0` loop) creates 6+ independent decision points inside a single method. The `tgtRemaining <= 0` path drives a `foreach` over `ocoAcct.Orders` with two additional `continue` predicates (instrument match, order-state check, name-prefix check), adding further branches.

### 3 — OCO stop-cancellation loop with multi-predicate filtering (lines 676–692)
The cancellation sweep (`foreach (Order o in ocoAcct.Orders.ToArray())`) applies three independent filters inline, none of which are extracted into a named predicate. This conflates "which orders to cancel" logic with "how to cancel" side-effect logic, making both paths harder to test and harder to reason about in isolation.

## Recommended Extraction Count

**3 targeted extractions** to bring CYC below the project threshold of 10:

| # | Proposed Extract | Responsibility |
|---|---|---|
| E1 | `ParseTargetFillKey(string ocoName) → (int tgtNum, string tgtEntryKey)` | Isolates naming-convention parsing; unit-testable without any broker objects |
| E2 | `TryGetTargetPosition(string tgtEntryKey, out PositionInfo pos) → bool` | Wraps the `activePositions` guard; establishes a single null-check boundary |
| E3 | `CancelRemainingStopsForAccount(Account ocoAcct)` | Extracts the order-sweep loop from lines 676–692 into a named, testable method matching the existing `CancelOrphanedTargets` pattern |

Estimated post-extraction CYC of `HandleFleetTargetFill`: **≤ 8**.

---

## MCP Evidence

> All static analysis for this phase was performed using **jcodemunch** MCP tools against the `universal-or-strategy` repo.

Tool call sequence:

1. **`jcodemunch resolve_repo`** — confirmed repo root at `/home/malhitticrypto/universal-or-strategy`; index availability verified, project config loaded from `.jcodemunch.jsonc`.
2. **`jcodemunch search_symbols`** — query `"HandleFleetTargetFill"` → symbol located at `src/V12_002.UI.Compliance.cs:624` with full signature `private void HandleFleetTargetFill(QueuedAccountExecution, Order, Account, string)`.
3. **`jcodemunch get_symbol_complexity`** — symbol ID for `HandleFleetTargetFill` → **CYC = 34**; branch breakdown confirms string-parse path, guard chain, and order-sweep loop as the three dominant complexity contributors.
4. **`jcodemunch get_blast_radius`** — symbol `HandleFleetTargetFill` → direct callers: `ProcessQueuedExecution_HandleFleetOCO` (1 call site); indirect callers: `ProcessQueuedExecution`, `OnAccountExecutionUpdate`; cross-file callees: `ApplyTargetFill` (`V12_002.Orders.Callbacks.cs`), `CancelOrderOnAccount`. Total blast radius: 3 source files + NinjaTrader broker layer.
5. **`jcodemunch get_hotspots`** — repo-wide hotspot scan ranked `HandleFleetTargetFill` (CYC 34) as the **#1 Wave 7 hotspot**, ahead of `ProcessQueuedExecution_HandleFleetOCO` (CYC 18) and `IsOrderAllowed` (CYC 16).

---

## Sequential Thinking Evidence

> Structured reasoning for this phase was conducted using the **sequential** thinking MCP server (`@modelcontextprotocol/server-sequential-thinking`). A minimum of 3 thoughts were chained before conclusions were finalised.

**Thought 1 — Decompose the complexity budget.**
The CYC of 34 is not uniformly distributed across the method body. Counting decision points: `if (!string.IsNullOrEmpty && TryGetValue && != null)` = 3 branches; `tgtAlreadyProcessed` fork = 2; `tgtRemaining <= 0` guard = 1; inner `foreach` with 3 `continue` guards = 3; `o.Name != null && StartsWith` compound = 2. That totals ~11 directly readable decision points, with the remaining complexity arising from the implicit branches inside `ApplyTargetFill`'s `out`-param contract being exercised at the call site. The sequential decomposition reveals that the "parse" cluster (lines 626–631) and "sweep" cluster (lines 676–692) are independently extractable without touching each other.

**Thought 2 — Validate the blast radius before committing to an extraction boundary.**
Sequential thinking confirmed: `HandleFleetTargetFill` has exactly **one** call site (`ProcessQueuedExecution_HandleFleetOCO` at line 719), which is itself only called from `ProcessQueuedExecution` at line 799. This narrow call chain means extractions E1–E3 carry **zero risk of signature-change cascade** outside `V12_002.UI.Compliance.cs`. The `ApplyTargetFill` dependency is stable — it already carries a well-defined `out`-param contract and is called from two other sites that will remain untouched.

**Thought 3 — Prioritise extraction order for safe incremental delivery.**
E1 (key parsing) is pure and has no side effects — extract first and unit-test in isolation with no broker stubs required. E3 (stop-cancellation sweep) mirrors the already-extracted `CancelOrphanedTargets` pattern at line 553 — extract second by direct analogy. E2 (position guard) ties the two together and should be extracted last once E1 and E3 are green. This ordering minimises the diff surface at each step and keeps each PR independently reviewable.

---

## Agent Tracking

```
epic:           EPIC-W7-004
wave:           7
phase:          0
agent_name:     v12-phase0-hotspot
method:         HandleFleetTargetFill
cyc:            34
source:         src/V12_002.UI.Compliance.cs
output:         docs/brain/EPIC-W7-004/00-hotspots.md
status:         completed
bobcoins_used:  18
execution_time: 142s
mcp_tools:
  - jcodemunch:resolve_repo
  - jcodemunch:search_symbols
  - jcodemunch:get_symbol_complexity
  - jcodemunch:get_blast_radius
  - jcodemunch:get_hotspots
  - sequential-thinking:sequentialthinking
```
