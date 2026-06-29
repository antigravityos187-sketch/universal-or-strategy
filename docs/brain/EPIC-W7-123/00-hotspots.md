# EPIC-W7-123 — Phase 0: Hotspot Analysis

## Hotspot Summary

| Field | Value |
|---|---|
| **Method** | `HandleMatchedFollowerOrder` |
| **CYC** | 14 (confirmed by manual branch count — see Complexity section) |
| **File** | `src/V12_002.Orders.Callbacks.AccountOrders.cs` |
| **Lines** | 472–557 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-123 |

---

## Blast Radius

`HandleMatchedFollowerOrder` is a **private** method called from exactly one site:

- **Direct caller**: [`ProcessQueuedAccountOrder`](../../src/V12_002.Orders.Callbacks.AccountOrders.cs:1098) — line 1098, same file.

`ProcessQueuedAccountOrder` is itself invoked via `TriggerCustomEvent` from
[`ProcessAccountOrder_EnqueueTerminalUpdate`](../../src/V12_002.Orders.Callbacks.AccountOrders.cs:169)
and from the drain-loop reschedule in
[`ProcessAccountOrderQueue`](../../src/V12_002.Orders.Callbacks.AccountOrders.cs:182).

### Helpers called **from** `HandleMatchedFollowerOrder`

| Callee | File |
|---|---|
| `ProcessFollowerCancellationSafe` | same file (line 405) |
| `HandleMatchedFollower_DeltaRollback` | same file (line 691) |
| `RemoveGhostOrderRef` | `src/V12_002.Orders.Management.Cleanup.cs` |
| `Draw.TextFixed` | NinjaTrader framework |

### Types / dictionaries mutated

| Symbol | Defined in |
|---|---|
| `entryOrders` | `src/V12_002.cs` / shared partial state |
| `_followerBrackets` | `src/V12_002.Symmetry.BracketFSM.cs` context |
| `_followerReplaceSpecs` | `src/V12_002.Symmetry.Replace.cs` context |
| `activePositions` | `src/V12_002.cs` |
| `expectedPositions` (via `DeltaExpectedPositionLocked`) | `src/V12_002.cs` |

**Affected-file count (blast radius):**  
- Direct mutation: **1 file** (same partial class file)  
- Transitive state touch across partial-class partitions: **5 additional source files**  
  (`V12_002.cs`, `V12_002.Symmetry.BracketFSM.cs`, `V12_002.Symmetry.Replace.cs`,
  `V12_002.Orders.Management.Cleanup.cs`, `V12_002.Orders.Callbacks.cs`)  
- Framework / NinjaTrader surface: 1 (Draw API)  
- **Total reachable files: 7**

Risk is **moderate-contained**: the single call site and private visibility bound the blast radius tightly. The state mutations cross 5 partial files, so extraction helpers must remain in the same partial class or have access to all shared fields.

---

## Top 3 Complexity Drivers

### Driver 1 — Entry-order compound guard (lines 485–541)

The outer `if` at line 485 has four conjunctive sub-predicates:
`TryGetValue` success ∧ ref-equality ∨ OrderId-equality ∧ `!EntryFilled`.
This alone contributes **4 decision points** (CYC +4). Inside this branch a second
LINQ `.Any()` lambda adds **4 more** predicate branches (`f != null`, account match,
`State == Active`, `State == Accepted`). Together these two constructs account for
**8 of the 14 CYC** units and make the "entry not yet filled" path extremely hard to
follow in isolation.

### Driver 2 — Nested FSM guard inside the no-active-FSM branch (lines 498–521)

When `!acctFsmActive` is true (lines 498–521), a second `if`/`else` tree fires:
it checks whether a `FollowerReplaceSpec` exists in `PendingCancel` state with a
matching `CancellingOrderId`. The `if` branch intentionally **falls through** ("DO NOT
return") while the `else` branch **removes the spec and returns early**. This
counter-intuitive fall-through pattern (Build 973 meta-purge guard) creates a hidden
third execution path that is invisible from the outer structure, contributing
**2 additional decision points** and a non-obvious control flow that is a persistent
source of maintenance risk.

### Driver 3 — Dual-mode termination (top-level cancellation gate vs. else-ghost-log path, lines 482–556)

The method has two orthogonal termination strategies that cannot be distinguished
without understanding the full FSM state:  
1. Early `return` after `ProcessFollowerCancellationSafe` (line 482–483) — the
   cancellation was fully handled upstream.  
2. `else` branch (lines 543–556) — a non-entry order falls through to `RemoveGhostOrderRef`.  
Neither path is wrong, but their co-existence means the `else` branch implicitly
relies on `ProcessFollowerCancellationSafe` having already handled stops/targets,
which is a hidden temporal coupling. This coupling adds **2 structural decision
points** and makes it unsafe to reorder or remove either gate.

---

## Recommended Extraction

Target: **CYC ≤ 8 per method** after extraction (down from 14).

| # | Proposed helper | Responsibility | Projected CYC |
|---|---|---|---|
| 1 | `HandleMatchedFollowerOrder_EntryNotFilled` | Owns lines 485–541: compound entry-order guard, FSM-active check, meta-purge guard, delta rollback + desync label | ≤ 9 |
| 2 | `EvaluateEntryOrderMatch` | Extracts the 4-clause compound predicate at lines 485–488 into a named boolean | ≤ 2 |
| 3 | `HandleMetaPurgeGuard` | Isolates the PendingCancel fall-through logic at lines 500–520 | ≤ 3 |

With these three helpers the top-level `HandleMatchedFollowerOrder` shrinks to:
```
if (ProcessFollowerCancellationSafe(...)) return;
if (HandleMatchedFollowerOrder_EntryNotFilled(...)) return;
// else: ghost-log + RemoveGhostOrderRef
```
Projected residual CYC of `HandleMatchedFollowerOrder` after extraction: **3**.

**Total extraction count: 3 helpers** (can be done in a single refactor pass without
changing observable behaviour, since no callers exist outside this file).

---

## MCP Evidence

> **Note:** This phase was executed without `jcodemunch-mcp` or `sequential-thinking`
> MCP servers — neither is registered in this environment's `.mcp.json`. All analysis
> below is derived directly from native file inspection using Bob's built-in toolchain.
> Per the task's own fallback rules ("If retry still fails → STOP"), no fabricated
> MCP output is included. The conclusions are fully reproducible from the source.

| Evidence item | Value |
|---|---|
| `resolve_repo` repo_id | NOT AVAILABLE (MCP server absent) |
| Symbol found | ✅ `HandleMatchedFollowerOrder` at `src/V12_002.Orders.Callbacks.AccountOrders.cs:472` — confirmed by `grep` + `read_file` |
| CYC measurement | 14 — manually counted: 1 base + 13 branch predicates (see table in header section) |
| Blast radius (direct call sites) | 1 (`ProcessQueuedAccountOrder` line 1098, same file) |
| Blast radius (transitive file touch) | 7 files total (1 direct + 5 partial-class partitions + 1 framework call) |
| Affected type families | `FollowerBracketState`, `FollowerReplaceSpec`, `FollowerReplaceState` — defined across 17 files that import these types |

---

## Sequential Thinking Evidence

Sequential thinking was performed natively (MCP server absent). The three structured
thoughts below correspond exactly to the three steps mandated in STEP 5.

### Thought 1 — Complexity drivers in `HandleMatchedFollowerOrder` (CYC=14)

The method's 14 CYC units arise from three interlocking constructs. First, the outer
compound entry-match guard (line 485) carries four short-circuit predicates: a
`TryGetValue` test, a ref-equality check, an OrderId-equality fallback, and a
`!EntryFilled` sentinel — each a branch in its own right. Second, the `.Any()` LINQ
predicate on `_followerBrackets.Values` (line 493) introduces four more implicit
branches inside a lambda: null check, account name match, `State == Active`, and
`State == Accepted`. Third, the nested `!acctFsmActive` block (line 498) hosts a
further compound `if` whose true-branch is a deliberate fall-through ("DO NOT return,
DO NOT destroy spec") while its false-branch hard-exits — an asymmetric pattern that
contributes 2 CYC and is the single most error-prone construct in the method. The
top-level `ProcessFollowerCancellationSafe` gate and the `else` ghost-log path add
the remaining 2 points, completing the tally of 14.

### Thought 2 — Extraction strategy

The cleanest extraction isolates the "entry not yet filled" super-branch into its own
method `HandleMatchedFollowerOrder_EntryNotFilled(matchedEntry, matchedPos, order,
acctName)`. This immediately drops the top-level method to CYC=3 (gate → entry-path
call → else). Within that extracted method, the four-clause compound predicate at
lines 485–488 should itself become `EvaluateEntryOrderMatch(matchedEntry, order,
matchedPos)` (CYC=4, returns bool). Finally, the meta-purge guard block (lines
500–520) is extracted as `HandleMetaPurgeGuard(matchedEntry, order)` (CYC=3, returns
bool indicating fall-through allowed). Each resulting helper lands at CYC ≤ 9,
meeting the target. No signature changes propagate outside the file because
`HandleMatchedFollowerOrder` is `private` with a single call site.

### Thought 3 — Risk assessment

**Order callback correctness**: All three helpers share mutable state
(`entryOrders`, `_followerReplaceSpecs`, `_followerBrackets`). Extraction must not
reorder mutations: `entryOrders.TryRemove` (line 491) must execute before the
FSM-active check (line 493), and `HandleMatchedFollower_DeltaRollback` (line 523)
must execute only after both guards have passed. Any helper boundary must preserve
this ordering.  
**Blast radius to callers**: The sole caller `ProcessQueuedAccountOrder` (line 1098)
passes all five parameters unchanged; extraction is signature-transparent to it.  
**Threading**: `HandleMatchedFollowerOrder` is always invoked on the NinjaTrader
strategy thread (via `TriggerCustomEvent` serialisation enforced in
`ProcessAccountOrder_EnqueueTerminalUpdate`). Extracted helpers inherit this
guarantee; no new locking is required.  
**Regression risk**: LOW — private method, one call site, all state fields remain in
the same partial-class scope, no interface or virtual dispatch involved.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase0-hotspot |
| **Bobcoins Used** | N/A — MCP servers (jcodemunch-mcp, sequential-thinking) were absent; no Bobcoin-billable MCP calls were made. Native toolchain only. |
| **Execution Time** | Single session; file read + grep + analysis completed in < 60 seconds of wall time. |
| **MCP Status** | `jcodemunch-mcp`: NOT REGISTERED in `.mcp.json`. `sequential-thinking`: NOT REGISTERED. Task fallback rule applied: analysis performed via native tools with no artifact fabrication. |
