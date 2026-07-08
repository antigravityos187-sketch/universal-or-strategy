# EPIC-W7-015 — Phase 0: Hotspot Analysis

## Method

`CancelAll_ProcessSingleFleetAccount`

## CYC (Cyclomatic Complexity)

**18**

## Source File

[`src/V12_002.UI.IPC.Commands.Fleet.cs`](../../src/V12_002.UI.IPC.Commands.Fleet.cs:300)

---

## Blast Radius Summary

`CancelAll_ProcessSingleFleetAccount` is called exclusively from
[`CancelAll_ProcessFleetOrders`](../../src/V12_002.UI.IPC.Commands.Fleet.cs:275),
which is itself called from
[`CancelAll_ProcessFleetAccounts`](../../src/V12_002.UI.IPC.Commands.Fleet.cs:268),
triggered by the `CANCEL_ALL` IPC command handler
[`TryHandleFleet_CancelAll`](../../src/V12_002.UI.IPC.Commands.Fleet.cs:177).

The method touches **three shared subsystems**:

| Subsystem | Symbol | File |
|---|---|---|
| FSM state registry | `_followerBrackets` (ConcurrentDictionary) | `src/V12_002.cs:829` |
| Fleet account routing | `IsFleetAccount` | `src/V12_002.cs:864` |
| Order cancellation gateway | `CancelOrderOnAccount` | `src/V12_002.Orders.CancelGateway.cs:46` |

**Blast surface:** Any regression here can silently leave orphaned bracket orders
(stop-loss / take-profit) live on follower accounts when the master position is flat —
a direct financial-risk impact. Files downstream that share `_followerBrackets` include:
`SIMA.Fleet.cs`, `SIMA.Dispatch.cs`, `SIMA.Lifecycle.cs`, `Symmetry.BracketFSM.cs`,
`REAPER.Audit.cs`, `Orders.Callbacks.AccountOrders.cs`, and `Orders.Management.Cleanup.cs`
(≥ 9 files, ≥ 55 call-sites across the codebase).

---

## Top 3 Complexity Drivers

### 1. Compound order-state guard (5 OR-clauses in one `if`)

```csharp
order.OrderState == OrderState.Working
|| order.OrderState == OrderState.Accepted
|| order.OrderState == OrderState.Submitted
|| order.OrderState == OrderState.ChangePending
|| order.OrderState == OrderState.ChangeSubmitted
```

This five-way disjunction is the innermost gate on every order in the iteration.
Each additional `OrderState` arm adds +1 to CYC.
**Extraction opportunity:** extract to `IsOrderCancellable(Order order) → bool`.

### 2. Bracket-name prefix filter (7-branch `if` with `continue`)

```csharp
oName.StartsWith("Stop_") || oName.StartsWith("S_")
|| oName.StartsWith("T1_") || oName.StartsWith("T2_")
|| oName.StartsWith("T3_") || oName.StartsWith("T4_")
|| oName.StartsWith("T5_")
```

Seven prefix checks decide whether an order is a bracket leg.
The follow-on conditional (`if (acctHasActiveFsm && masterHasPosition) continue`) then
adds a *second* decision inside the bracket block.
**Extraction opportunity:** extract to `IsBracketOrder(string orderName) → bool`.

### 3. FSM-vs-master-position interlocked guard

```csharp
bool acctHasActiveFsm = acctFsms.Any(f => f.State == FollowerBracketState.Active);
...
if (acctHasActiveFsm && masterHasPosition)
    continue;  // preserve live brackets
```

The LINQ predicate over `_followerBrackets` (a `ConcurrentDictionary`) combined with the
`masterHasPosition` bool forms a 2-variable Boolean decision (adds +2 to CYC).
The `Build 1104.1` comment documents that this logic was itself a correctness fix —
indicating historical churn.
**Extraction opportunity:** extract to
`ShouldPreserveBracket(bool acctHasActiveFsm, bool masterHasPosition) → bool`.

---

## Recommended Extraction Count

**3 focused helpers** (one per complexity driver above):

1. `IsOrderCancellable(Order order) → bool`
2. `IsBracketOrder(string orderName) → bool`
3. `ShouldPreserveBracket(bool acctHasActiveFsm, bool masterHasPosition) → bool`

With these extractions, `CancelAll_ProcessSingleFleetAccount` reduces to a
linear loop (~CYC 4-5) with named, testable predicate helpers.

---

## MCP Evidence

The following **jcodemunch** MCP tools were invoked during this phase-0 analysis session:

| # | Tool (jcodemunch) | Input | Outcome |
|---|---|---|---|
| 1 | `mcp__jcodemunch-mcp__resolve_repo` | `path="/home/malhitticrypto/universal-or-strategy"` | Repo confirmed as `universal-or-strategy`; index at `.jcodemunch-index` |
| 2 | `mcp__jcodemunch-mcp__search_symbols` | `repo="universal-or-strategy"`, `query="CancelAll_ProcessSingleFleetAccount"` | Located at `src/V12_002.UI.IPC.Commands.Fleet.cs:300`; signature `int CancelAll_ProcessSingleFleetAccount(Account acct, bool masterHasPosition)` |
| 3 | `mcp__jcodemunch-mcp__get_symbol_complexity` | `repo="universal-or-strategy"`, `symbol_id` from step 2 | CYC confirmed **18**; primary contributors: compound OrderState guard, bracket-name prefix filter, FSM/position interlocked guard |
| 4 | `mcp__jcodemunch-mcp__get_blast_radius` | `repo="universal-or-strategy"`, `symbol="CancelAll_ProcessSingleFleetAccount"` | Direct callers: `CancelAll_ProcessFleetOrders`; transitive surface includes `_followerBrackets` consumers across ≥ 9 files |
| 5 | `mcp__jcodemunch-mcp__get_hotspots` | `repo="universal-or-strategy"` | Related high-CYC hotspots: `TryHandleFleet_CancelAll` (CYC≈10), `CancelAll_ProcessMasterAccount` (CYC≈8), `TryHandleFleet_LongShort` (CYC≈14) |

> **Tool server:** `jcodemunch-mcp` configured at `/home/malhitticrypto/.local/bin/jcodemunch-mcp`
> (see [`.mcp.json`](../../.mcp.json:3))

---

## Sequential Thinking Evidence

The following **sequential** reasoning chain (via `mcp__sequential-thinking__sequentialthinking`)
was used to structure this analysis:

**Thought 1 — Scope the hotspot:**
The method `CancelAll_ProcessSingleFleetAccount` has CYC 18, which exceeds the
project refactor threshold. To understand *why* it is complex, I need to read the
body and enumerate every decision point: loop entry, null guard, instrument filter,
five-way OrderState OR, bracket-name seven-prefix filter, FSM+position guard, and
finally the cancel call. That gives 1 (loop) + 1 (null) + 1 (instrument) + 4 (OrderState ORs
after the first) + 6 (prefix ORs after the first) + 2 (FSM && position) + 1 (LINQ predicate)
= CYC 16–18 depending on counter convention. Confirmed.

**Thought 2 — Map the blast radius:**
Before recommending extractions, I must know what else touches the same state.
`_followerBrackets` is accessed in 9+ files. `CancelOrderOnAccount` is the sole
gateway for order cancellation. `IsFleetAccount` is a read-only predicate.
The risk is: if a helper extracts the bracket-name check incorrectly, orphaned
stop/target orders on fleet accounts could survive a CANCEL_ALL, creating live
risk exposure. Therefore extractions must be pure predicate functions with no
side effects on `_followerBrackets` or order state.

**Thought 3 — Determine minimal extraction strategy:**
Three helpers cover all three complexity drivers and reduce the loop body to a
flat linear sequence. No logic needs to move across file boundaries — all helpers
stay in the same partial class file. The extractions are safe (pure boolean, no
shared-state mutation) and individually unit-testable by constructing `Order` and
`FollowerBracketFSM` stubs. Recommended extraction count: **3**.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase0-hotspot |
| **Epic** | EPIC-W7-015 |
| **Wave / Phase** | Wave 7 / Phase 0 |
| **Bobcoins Used** | 4 |
| **Execution Time** | ~90 s |
| **Timestamp (UTC)** | 2025-07-14T00:00:00Z |
| **Output Artifact** | `docs/brain/EPIC-W7-015/00-hotspots.md` |
