# EPIC-W7-093 — Phase 2: Architecture Plan
# Dispatch_ProcessFleetLoop

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Input:** docs/brain/EPIC-W7-093/01-scope-boundary.md

---

## Extraction Plan

| Helper Method | Extracted Logic | Params | Return | Projected CYC | Attribute |
|---|---|---|---|---|---|
| `Dispatch_ExecuteFleetAccountEntry` | Happy-path: `Dispatch_BuildFollowerOrders` call, `!_builtOk` early return, `isMarketEntry` fork → `Dispatch_PublishMarketBracketToPhoton` or `Dispatch_PublishLimitEntryToPhoton` | `acct`, `fleet[i]` (AccountRankInfo), `tradeType`, `action`, `quantity`, `entryPrice`, `entryOrderType`, `symmetryDispatchId`, `dispatchTargetCount`, `dispatchLog`, `ref syncPending`, `ref reservedDelta`, `ref registeredForCleanup`, `out fleetEntryName`, `out expectedKey` | `bool` (true = success, caller increments rmaCount) | 5 | `[MethodImpl(MethodImplOptions.AggressiveInlining)]` |
| `Dispatch_RollbackFleetAccountEntry` | Catch rollback: `syncPending` clear via `ClearDispatchSyncPending`, `reservedDelta` reversal via `AddExpectedPositionDeltaLocked`, `registeredForCleanup` 5-dict TryRemove loop, `_followerBrackets` cleanup, log append | `ref syncPending`, `ref reservedDelta`, `bool registeredForCleanup`, `string fleetEntryName`, `string expectedKey`, `Account acct`, `Exception ex`, `StringBuilder dispatchLog` | `void` | 6 | `[MethodImpl(MethodImplOptions.NoInlining)]` |

**Residual `Dispatch_ProcessFleetLoop` CYC: 6** (for-loop +1, acct==this.Account skip +1, ShouldSkipFleetAccount skip +1, Volatile.Read circuit-breaker guard +1, try/catch +1, !Execute() continue +1)

**max_cyc_projected: 6** ✅ (threshold: 8)

---

## Complexity Driver Analysis

### Driver 1 — Fleet-loop per-account exception recovery state machine (CYC +6)

The outer `for`-loop (CYC +1) wraps a try/catch block. The catch arm contains three independent compensating branches that must all fire in correct order on failure:

1. **`if (syncPending)`** (+1): Calls `ClearDispatchSyncPending(expectedKey)` and resets the flag. Guards against dangling sync-pending state leaking into subsequent loop iterations.
2. **`if (reservedDelta != 0)`** (+1): Calls `AddExpectedPositionDeltaLocked(expectedKey, -reservedDelta)` to reverse any position-delta reservation made during the publish phase.
3. **`if (registeredForCleanup)`** (+1): Enters a cleanup block that removes the failed fleet entry from five tracking dictionaries (`activePositions`, `entryOrders`, `stopOrders`, `targetOrders[1..5]`). Contains its own inner for-loop (+1) iterating tNum=1..5 with a null guard on `GetTargetOrdersDictionary(tNum)` (+1).
4. **`if (!string.IsNullOrEmpty(fleetEntryName))`** (+1): Removes the proactive FSM entry from `_followerBrackets` — required even if `registeredForCleanup` is false (FSM may have been initialized before tracking dicts).

Total: CYC +6 from this driver.

### Driver 2 — Dual order-type dispatch fork with volatile circuit-breaker (CYC +4)

Inside the happy path (try block) and the loop entry guards:

1. **`if (acct == this.Account) continue`** (+1): Master account skip guard — must remain in outer loop.
2. **`if (ShouldSkipFleetAccount(...)) continue`** (+1): Delegated health check (inactive, H-13, consistency lock) — must remain in outer loop.
3. **`if (Volatile.Read(ref _reaperCircuitBreakerTripped) == 1) continue`** (+1): Circuit-breaker fast-exit BEFORE any allocation. Jane Street critical ordering constraint.
4. **`if (isMarketEntry)`** (+1): Forks between `Dispatch_PublishMarketBracketToPhoton` (market bracket with stop + targets) and `Dispatch_PublishLimitEntryToPhoton` (limit-only entry). Extracted into the Execute helper.

Total: CYC +4 from this driver (guards 1-3 stay in residual; guard 4 moves to Execute helper).

### Driver 3 — Inner cleanup loop over 5 target dictionaries in catch (CYC +4)

Nested inside the `registeredForCleanup` branch of the catch arm:

1. **`for (int tNum = 1; tNum <= 5; tNum++)`** (+1): Iterates over the 5 target order dictionaries.
2. **`if (targetDict != null)`** (+1): Null-guard on `GetTargetOrdersDictionary(tNum)` return value.
3. **`if (!_builtOk) continue`** (+1): Early return from BuildFollowerOrders failure inside the try block — semantically adjacent complexity.
4. **String null/empty check on fleetEntryName** (+1): `if (!string.IsNullOrEmpty(fleetEntryName))` before FSM cleanup.

Total: CYC +4 from this driver (entire catch block moves to Rollback helper).

---

## Jane Street Alignment

| Rule | Application |
|---|---|
| carl_cook zero-alloc | `Volatile.Read(_reaperCircuitBreakerTripped)` guard stays BEFORE `Dispatch_ExecuteFleetAccountEntry` call in outer loop — no `out` param locals allocated when breaker is tripped |
| carl_cook AggressiveInlining | Applied to `Dispatch_ExecuteFleetAccountEntry` — hot loop path called once per fleet account per dispatch cycle |
| carl_cook NoInlining | Applied to `Dispatch_RollbackFleetAccountEntry` — cold catch/error path; inlining would pollute hot-path instruction cache |
| carl_cook ref/in/out | Rollback helper uses `ref syncPending`, `ref reservedDelta`; Execute helper uses `ref syncPending`, `ref reservedDelta`, `ref registeredForCleanup`, `out fleetEntryName`, `out expectedKey` — zero heap allocation for state passing |
| gjengset no lock() | No new `lock()` blocks introduced; existing `Volatile.Read` pattern untouched |
| gjengset volatile | `_reaperCircuitBreakerTripped` volatile read ordering preserved — MUST remain as third guard in outer loop, before Execute call, before any allocation |
| trading_billions SRP | `Dispatch_ExecuteFleetAccountEntry` = build + publish happy path only; `Dispatch_RollbackFleetAccountEntry` = compensation/cleanup only |
| trading_billions CYC<=8 | Execute=5, Rollback=6, Residual=6 — all ≤ 8 ✅ |
| trading_billions circuit-breaker | `Volatile.Read` circuit-breaker check preserved in outer loop at correct position; circuit-breaker rate-limit pattern not altered |

---

## rmaCount Semantics Contract

`rmaCount` MUST be incremented in `Dispatch_ProcessFleetLoop` only on `true` return from `Dispatch_ExecuteFleetAccountEntry`. Do NOT move the increment inside the helper. The outer loop pattern after extraction:

```csharp
bool ok = Dispatch_ExecuteFleetAccountEntry(acct, fleet[i], ..., ref syncPending, ref reservedDelta, ref registeredForCleanup, out fleetEntryName, out expectedKey);
if (!ok)
    continue;
rmaCount++;
```

This preserves count accuracy across all loop iterations. `rmaCount` represents successfully dispatched follower accounts — it must not be incremented on BuildFollowerOrders failure, circuit-breaker skip, or catch-arm entry.

---

## Method Signature Contracts

### Dispatch_ExecuteFleetAccountEntry (new private helper)

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private bool Dispatch_ExecuteFleetAccountEntry(
    Account acct,
    AccountRankInfo rankInfo,
    string tradeType,
    OrderAction action,
    int quantity,
    double entryPrice,
    OrderType entryOrderType,
    int index,
    string symmetryDispatchId,
    int dispatchTargetCount,
    StringBuilder dispatchLog,
    ref bool syncPending,
    ref int reservedDelta,
    ref bool registeredForCleanup,
    out string fleetEntryName,
    out string expectedKey
)
```

Returns `false` on `!_builtOk` (caller uses `continue`). Returns `true` after successful publish call. Caller increments `rmaCount` on `true`.

### Dispatch_RollbackFleetAccountEntry (new private helper)

```csharp
[MethodImpl(MethodImplOptions.NoInlining)]
private void Dispatch_RollbackFleetAccountEntry(
    ref bool syncPending,
    ref int reservedDelta,
    bool registeredForCleanup,
    string fleetEntryName,
    string expectedKey,
    Account acct,
    Exception ex,
    StringBuilder dispatchLog
)
```

Called exclusively from the catch block of `Dispatch_ProcessFleetLoop`. Handles all compensation: sync-pending clear, delta reversal, 5-dict cleanup, FSM bracket removal, log append.

---

## MCP Evidence

### resolve_repo
- Repo: `antigravityos187-sketch/universal-or-strategy`
- Source root: `/home/malhitticrypto/universal-or-strategy`
- Symbol count: 5147, File count: 2000
- Index status: loadable (sqlite backend, indexed 2026-06-29)

### get_context_bundle
- Symbol confirmed at `src/V12_002.SIMA.Dispatch.cs` line 196–348
- Signature: `private int Dispatch_ProcessFleetLoop(List<AccountRankInfo> fleet, HashSet<string> activeAccountSnapshot, int dispatchTargetCount, string symmetryDispatchId, string tradeType, OrderAction action, int quantity, double entryPrice, OrderType entryOrderType, Stopwatch sw, long tLoopStartTicks, StringBuilder dispatchLog)`
- Source fully retrieved; all three complexity drivers confirmed in live code

### get_call_hierarchy
- **Sole caller**: `ExecuteSmartDispatchEntry` (src/V12_002.SIMA.Dispatch.cs, line 45, ast_resolved) — zero blast radius outside the file
- **Direct callees (depth 1)**: `ShouldSkipFleetAccount`, `Dispatch_BuildFollowerOrders`, `Dispatch_PublishMarketBracketToPhoton`, `Dispatch_PublishLimitEntryToPhoton`, `ClearDispatchSyncPending`, `AddExpectedPositionDeltaLocked`, `GetTargetOrdersDictionary`, `_followerBrackets` (TryRemove), `activePositions`, `entryOrders`, `stopOrders`
- **Depth 2 callees include**: `TryIncrementDispatchCountWithCircuitBreaker`, `RegisterTrackingDictionaries`, `InitializeFollowerBracketFSM`, `ClaimPhotonPoolSlot`, `PopulatePhotonSlot`, `EnqueueToPhotonRing`, `EnqueueLimitEntryToPhotonRing`, `StampAccountFillGrace`
- All extractions remain private methods in same partial class — no import/export changes required

### get_dependency_graph
- `src/V12_002.SIMA.Dispatch.cs`: zero file-level import/importer edges (all symbols in same partial class — intra-file partial class pattern in C# NinjaTrader strategy)
- Extractions are private methods in same file — confirmed zero blast radius on file dependency graph

---

## Sequential Thinking Evidence

### Thought 1 — Complexity Driver Analysis
Confirmed CYC decomposition from live source: Driver 1 (catch state machine) = +6, Driver 2 (dispatch fork + circuit-breaker) = +4, Driver 3 (inner cleanup loop) = +4. Total CYC = 14. Three drivers map cleanly to two extraction targets: try-block happy path → Execute helper; catch block → Rollback helper.

### Thought 2 — Extraction Strategy Validation
Full parameter contracts derived from source: Execute needs `ref syncPending`, `ref reservedDelta`, `ref registeredForCleanup`, `out fleetEntryName`, `out expectedKey` to allow Rollback helper to receive correct state. rmaCount semantics validated: increment must stay in outer loop on `true` return. Volatile.Read ordering constraint confirmed: must be third guard in outer loop, before Execute call site, to satisfy Jane Street zero-alloc-before-guard rule.

### Thought 3 — CYC Validation and Jane Street Confirmation
Residual loop CYC counted: for +1, acct skip +1, ShouldSkip +1, CB guard +1, try/catch +1, !Execute() +1 = CYC 6. All three resulting methods ≤ 8 (Execute=5, Rollback=6, Residual=6). [AggressiveInlining] on Execute confirmed (hot path). [NoInlining] on Rollback confirmed (cold catch path, instruction cache preservation). No lock() blocks. No new heap allocations. Sole caller `ExecuteSmartDispatchEntry` — public signature of `Dispatch_ProcessFleetLoop` unchanged.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Wave** | 7 |
| **Phase** | 2 |
| **Epic** | EPIC-W7-093 |
| **max_cyc_projected** | 6 |
| **Jane Street KB** | carl_cook + gjengset + trading_billions applied |
