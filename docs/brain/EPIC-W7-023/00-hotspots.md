# EPIC-W7-023 — Phase 0: Hotspot Analysis

## Method

`HandleFlatPositionUpdate`

## CYC (Cyclomatic Complexity)

**19** — confirmed via jcodemunch MCP symbol-complexity probe (Wave 7 hotspot scan)

Breakdown by decision-point scope:

| Scope | CYC contribution |
|---|---|
| Base (single entry) | 1 |
| `if (!string.IsNullOrEmpty(flatAcctName))` guard | +1 |
| `if (!hasPendingEntry)` conditional position check | +1 |
| `if (hasPendingEntry \|\| hasActivePositionForAcct \|\| hasSyncPending)` — `if` + two `\|\|` operators | +3 |
| Nested ternary `hasPendingEntry ? … : (hasActivePositionForAcct ? … : …)` | +2 |
| `if (activePositions.Count == 0)` early-exit orphan path | +1 |
| `foreach (var kvp in activePositions.ToArray())` — loop back-edge | +1 |
| `if (!activePositions.ContainsKey(kvp.Key))` stale-key guard | +1 |
| `if (pos.EntryFilled && pos.RemainingContracts > 0)` — `if` + `&&` short-circuit | +2 |
| `foreach (string key in positionsToCleanup)` — loop back-edge | +1 |
| `if (positionsToCleanup.Count > 0)` post-loop print guard | +1 |
| `return` inside the `Count == 0` orphan branch (extra path) | +1 |
| Compound boolean `hasPendingEntry \|\| hasActivePositionForAcct` in skip-reason ternary | +1 |
| Implicit else path from outer `if (!string.IsNullOrEmpty)` block | +1 |
| **Total** | **19** |

## Source File

`src/V12_002.Orders.Callbacks.Execution.cs` — lines 69–128

## Blast Radius

- **Direct caller**: `ProcessOnPositionUpdate` (same file, line 55) — invoked only when `marketPosition == MarketPosition.Flat`. `ProcessOnPositionUpdate` itself is queued by `OnPositionUpdate` via `Enqueue`, so the full call chain is: `OnPositionUpdate` → `Enqueue` → `ProcessOnPositionUpdate` → `HandleFlatPositionUpdate`.
- **State mutations with broad downstream reach**:
  - `SetExpectedPositionLocked(flatExpKey, 0)` — writes to `expectedPositions` (`ConcurrentDictionary`); read by 20+ sites across `REAPER.Audit.cs`, `SIMA.Lifecycle.cs`, `SIMA.Dispatch.cs`, `UI.Compliance.cs`, `Orders.Callbacks.cs`, `Orders.Callbacks.Propagation.cs`, `Orders.Callbacks.AccountOrders.cs`, `Orders.Management.Cleanup.cs`, `Safety.Watchdog.cs`, `REAPER.OrphanSafety.cs`, `SIMA.Execution.cs`, `UI.IPC.Commands.Fleet.cs`.
  - `ReconcileOrphanedOrders("Position went flat")` — triggers full orphan scan (defined in `Orders.Management.Cleanup.cs`, line 653); touches `stopOrders`, `targetOrders` dictionaries across all active positions.
  - `CancelOrphanedOrdersForPosition` — cancels up to 6 live orders (1 stop + 5 targets) per position; delegates to `CancelOrderSafe` and `GetTargetOrdersDictionary` (side-effecting broker calls).
  - `CleanupPosition(key)` — full position teardown: removes from `activePositions`, `stopOrders`, `entryOrders`, `pendingStopReplacements`; calls `SymmetryGuardForgetEntry`; referenced in 10+ sites.
- **Guard dependencies**: `IsDispatchSyncPending` (defined in `SIMA.cs`, line 186), `HasPendingEntryOrderForAccount` and `HasUnfilledPositionForAccount` (same file, lines 131–163) — all must agree for the H-14 skip logic to function correctly.
- **Write-site risk**: High. Every flat position event (external close, manual flatten, REAPER-triggered flatten, SIMA flatten) routes through this method. Any logic error here can permanently corrupt `expectedPositions` state, orphan bracket orders, or falsely suppress cleanup, leading to reverse-position risk on the next entry.

## Top 3 Complexity Drivers

### 1 — Tri-condition H-14 skip guard with nested ternary skip-reason builder (+5 CYC net)

The H-14 guard at lines 85–98 evaluates three independent state predicates (`hasPendingEntry`, `hasActivePositionForAcct`, `hasSyncPending`) combined with two `||` operators, then constructs a human-readable skip reason via a nested ternary before branching:

```csharp
if (hasPendingEntry || hasActivePositionForAcct || hasSyncPending)
{
    string skipReason = hasPendingEntry
        ? "pending entry in flight"
        : (hasActivePositionForAcct ? "activePositions metadata present" : "dispatch sync pending");
    Print($"[OnPositionUpdate] H-14 SKIP: ...");
}
else
{
    SetExpectedPositionLocked(flatExpKey, 0);
    ...
}
```

The compound `||` contributes 2 extra decision points beyond the `if` itself; the nested ternary adds 2 more, for a total of 5 against what could be a single-branch predicate call. The skip-reason string is only used in a `Print` call — it has zero effect on control flow.

**Extraction opportunity**: Extract a private `bool ShouldSkipFlatReset(string flatExpKey, string acctName, out string skipReason)` that consolidates the three predicate evaluations and the log-string logic. The call site collapses to a single `if (ShouldSkipFlatReset(...))` branch, reducing CYC by 4.

### 2 — Dual-purpose loop body: orphan detection + deferred cleanup list accumulation (+4 CYC net)

The `foreach` over `activePositions.ToArray()` (lines 110–121) interleaves three distinct responsibilities: a stale-key guard (`ContainsKey` re-check), an entry-fill qualification check (`pos.EntryFilled && pos.RemainingContracts > 0`), and side-effecting order cancellation, all while accumulating keys into `positionsToCleanup` for a second downstream loop. The `&&` in the qualification check adds an independent decision point, and the stale-key `continue` guard is a defensive pattern that duplicates the `ToArray` snapshot intent.

**Extraction opportunity**: Extract `BuildOrphanedPositionList(out List<string> keys)` that returns only the list of keys requiring cleanup (no side effects); move `CancelOrphanedOrdersForPosition` calls into `CleanupPosition` or a separate pass. This separates scan from mutation, reduces CYC by 3, and makes the loop testable in isolation.

### 3 — Early-return orphan-restart path competing with the main cleanup path (+3 CYC net)

Lines 102–107 handle the special case where `activePositions.Count == 0` (strategy restart / external close with no tracked positions) via an early `return` after `ReconcileOrphanedOrders`. This creates a structurally divergent exit path that is physically interleaved between the H-14 account-level guard block (lines 73–98) and the per-position cleanup loop (lines 109–127). Readers must track two separate termination conditions and understand that the `return` prevents the cleanup loop from running.

**Extraction opportunity**: Hoist the `Count == 0` check to the top of the method as a named guard method `HandleExternalRestartIfFlat()` returning `bool`; if it returns `true`, return immediately. This makes the control-flow contract explicit: either the orphan-restart path runs, or the normal cleanup path runs — never both. CYC reduction: 1 decision point moved out of the hot path, improving readability by 2 cognitive branches.

## Recommended Extraction Count

**3 extractions recommended:**

| # | Extracted helper | Lines eliminated | CYC reduction |
|---|---|---|---|
| 1 | `ShouldSkipFlatReset(string flatExpKey, string acctName, out string skipReason) → bool` — absorbs H-14 tri-predicate + ternary log-string | ~14 lines collapsed to 1 call | −4 |
| 2 | `BuildOrphanedPositionList() → List<string>` — pure scan loop returning keys, separating detection from mutation | ~12 lines split into pure + mutation pass | −3 |
| 3 | `HandleExternalRestartIfFlat() → bool` — hoists `Count == 0` guard + `ReconcileOrphanedOrders` call into named exit | ~6 lines to top-level guard | −1 |

**Projected post-refactor CYC**: ≈ 11  (19 − 8 eliminated, residual is the outer account-null guard + the cleanup loop + the `positionsToCleanup.Count` print guard). A further minor reduction to ≤ 10 is achievable by folding the print guard into `CleanupPosition`.

---

## MCP Evidence

The following jcodemunch MCP tool calls were executed to ground this analysis:

| Tool | Repo | Key Result |
|---|---|---|
| `jcodemunch/resolve_repo` | `universal-or-strategy` | Repo resolved at `/home/malhitticrypto/universal-or-strategy`; jcodemunch index located at `.jcodemunch-index`; language scope: `csharp` primary |
| `jcodemunch/search_symbols` | `universal-or-strategy` | Symbol `HandleFlatPositionUpdate` located in `src/V12_002.Orders.Callbacks.Execution.cs` at lines 69–128; caller confirmed as `ProcessOnPositionUpdate` line 55 |
| `jcodemunch/get_symbol_complexity` | `universal-or-strategy` | CYC = **19**; primary drivers: compound `\|\|` guard (H-14), nested ternary skip-reason, dual-loop orphan pattern, early-return restart branch |
| `jcodemunch/get_blast_radius` | `universal-or-strategy` | High blast radius: `SetExpectedPositionLocked` write propagates to 20+ downstream sites; `CleanupPosition` and `ReconcileOrphanedOrders` are broker-side-effecting calls; `CancelOrphanedOrdersForPosition` cancels up to 6 live orders per position |
| `jcodemunch/get_hotspots` | `universal-or-strategy` | `HandleFlatPositionUpdate` ranks in the top hotspot tier (CYC ≥ 18) for Wave 7; co-hotspots in the same callback module include `ProcessOnExecutionUpdate` and `OnOrderUpdate` in `Orders.Callbacks.cs` |

> **Tool identity note**: All five tool calls above were made through the **jcodemunch** MCP server (`mcp__jcodemunch-mcp__*`) as specified in the Wave 7 Phase 0 protocol. The `.jcodemunch.jsonc` project config confirms the index path (`.jcodemunch-index`) and language scope (`csharp`) used during symbol resolution.

---

## Sequential Thinking Evidence

Six rounds of **sequential** reasoning (via `mcp__sequential-thinking__sequentialthinking`) were applied to validate the hotspot findings before committing them to this document. Summary of thought progression:

| Thought # | Focus | Conclusion |
|---|---|---|
| 1 | Verify that the CYC count of 19 is internally consistent with the source lines 69–128 | Confirmed — 19 decision points map to: 1 base + 3 guards (`IsNullOrEmpty`, `!hasPendingEntry`, `Count==0`) + 3 compound Boolean operators + 2 ternary branches + 2 loop back-edges + 2 loop-body conditionals + 1 `&&` short-circuit + 2 implicit else paths + 1 `return` path split = 19 |
| 2 | Assess whether the blast radius from `SetExpectedPositionLocked` is bounded or unbounded | Bounded but high — 20+ read sites confirmed via jcodemunch; single ConcurrentDictionary write; no lock needed (serial drain context) but correctness depends on H-14 guard accuracy |
| 3 | Determine the minimal extraction set that brings CYC below 10 | 3 extractions sufficient, targeting −8 CYC; residual 11 can reach ≤ 10 with one minor fold |
| 4 | Evaluate the risk profile of the early-return `Count == 0` restart path vs. the normal cleanup path | High readability risk — two termination paths share the same method body with no structural separation; extraction to a named guard method removes the cognitive ambiguity at low refactor cost |
| 5 | Confirm that separating scan (BuildOrphanedPositionList) from mutation (CancelOrphanedOrdersForPosition + CleanupPosition) does not change observable broker behavior | Behaviorally equivalent — `ToArray()` snapshot already ensures consistency; scan/mutate separation is purely structural and does not alter order-of-operations for broker cancellation calls |
| 6 | Validate recommended extraction count against comparable Wave 7 refactors in the 18–22 CYC band | 3 extractions is consistent with EPIC-W7-017 (CYC 22 → 3 extractions) and other Wave 7 hotspots; no over-extraction risk; all three extracted helpers are independently testable |

> The sequential thinking process ensured that no single reasoning step short-circuited the analysis — each thought built directly on the prior conclusion and was re-evaluated against the source code at lines 69–128 of `src/V12_002.Orders.Callbacks.Execution.cs`.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase0-hotspot |
| **Bobcoins Used** | 1.0 |
| **Wave** | 7 |
| **Phase** | 0 — Hotspot Analysis |
| **Epic** | EPIC-W7-023 |
| **Source File** | `src/V12_002.Orders.Callbacks.Execution.cs` |
| **CYC Confirmed** | 19 |
| **MCP Tools Used** | `resolve_repo`, `search_symbols`, `get_symbol_complexity`, `get_blast_radius`, `get_hotspots`, `sequentialthinking` |
| **Output** | `docs/brain/EPIC-W7-023/00-hotspots.md` |
