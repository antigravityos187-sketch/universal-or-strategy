# EPIC-W7-095 — Phase 2: Architecture Plan
# ProcessSingleFleetRMAAccount

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Input:** docs/brain/EPIC-W7-095/01-scope-boundary.md

---

## Extraction Plan

| Helper Method | Extracted Logic | Params | Return | Projected CYC | Attribute |
|---|---|---|---|---|---|
| `IsAccountEligibleForRMADispatch` | Fleet-active check (`activeFleetAccounts.TryGetValue` + `!isActive`) + consistency-lock ceiling (`EnableConsistencyLock` outer guard + `dailyPL >= MaxDailyProfitCap` inner guard) | `Account acct, StringBuilder dispatchLog` | `bool` (true = eligible) | 4 | `[AggressiveInlining]` |
| `RegisterFleetFollowerState` | Dict + FSM registration in [923B-FIX-B] order: `activePositions`/`entryOrders` writes FIRST, `MarkDispatchSyncPending`, `!_followerBrackets.ContainsKey` FSM init, direction ternary `reservedDelta`, `AddExpectedPositionDeltaLocked` SECOND | `Account acct, string fleetKey, string expectedKey, PositionInfo fleetFollowerPos, Order fEntry, MarketPosition direction, int qty, StringBuilder dispatchLog, out bool syncPending, out int reservedDelta` | `void` | 5 | — |
| `RollbackFleetFollowerState` | Catch rollback: `if (syncPending) ClearDispatchSyncPending`, `if (reservedDelta != 0) AddExpectedPositionDeltaLocked(-reservedDelta)`, `activePositions.TryRemove`, `entryOrders.TryRemove`, `_followerBrackets.TryRemove` | `string fleetKey, string expectedKey, bool syncPending, int reservedDelta, StringBuilder dispatchLog, Account acct` | `void` | 5 | `[NoInlining]` |

**Residual `ProcessSingleFleetRMAAccount` CYC: 6**
(IsEligible guard-return +1, CreateOrder null check +1, `fEntry` orderId compound guard +1, try/catch boundary +1, base = 1, total = ~6)

**max_cyc_projected: 5** ✅ (threshold: 8)

---

## Critical Invariants Table

| Invariant | Code Contract | Enforced By |
|---|---|---|
| [923B-FIX-B] dict BEFORE delta | `activePositions`/`entryOrders` written before `AddExpectedPositionDeltaLocked` | `RegisterFleetFollowerState` internal ordering |
| SyncPending brackets delta | `MarkDispatchSyncPending` before / `ClearDispatchSyncPending` after expectedPositions increment | `RegisterFleetFollowerState` (mark) + outer method happy path + `RollbackFleetFollowerState` (clear on catch) |
| SymmetryGuard before dict | `SymmetryGuardRegisterFollower` called before try-block dict writes | Outer method (NOT extracted) |
| Full rollback on catch | All 5 write surfaces reverted: `activePositions`, `entryOrders`, `_followerBrackets`, expectedPositions delta, syncPending flag | `RollbackFleetFollowerState` |
| Submit last | `acct.Submit` after all dict writes and after `RegisterFleetFollowerState` completes | Outer method (NOT extracted) |

---

## Complexity Driver Analysis

### Driver 1 — Guard Branching (CYC +3)
Two early-return guards protect entry:
1. `activeFleetAccounts.TryGetValue(acct.Name, out bool isActive) || !isActive` — skips inactive/unregistered fleet accounts. One compound branch.
2. `if (EnableConsistencyLock)` outer check + `if (dailyPL >= MaxDailyProfitCap)` inner check — skips accounts over daily P&L ceiling.
Total branches contributed: **+3**. Both guards extracted entirely into `IsAccountEligibleForRMADispatch`.

### Driver 2 — Null/FSM Guards (CYC +2)
1. `if (fEntry == null)` — [M8.1 NRE-01] null-return guard after `CreateOrder` for disconnected/invalid account pairs. Guard before any reservation; no rollback needed at this point.
2. `if (!_followerBrackets.ContainsKey(fleetKey))` — FSM initialisation guard: only creates FollowerBracketFSM if not already registered.
Total branches contributed: **+2**. FSM guard extracted into `RegisterFleetFollowerState`; `fEntry == null` stays in outer method (close to `CreateOrder` allocation).

### Driver 3 — Direction Ternary + OrderId Guard (CYC +2)
1. `reservedDelta = (direction == MarketPosition.Long) ? qty : -qty` — ternary computes signed delta for expected-position accounting.
2. `if (fEntry != null && !string.IsNullOrEmpty(fEntry.OrderId))` — compound null/empty check before `_orderIdToFsmKey[fEntry.OrderId] = fleetKey` O(1) FSM lookup insert.
Total branches contributed: **+2**. Direction ternary extracted into `RegisterFleetFollowerState`; orderId guard stays in outer method (post-Submit, requires live `fEntry.OrderId`).

### Driver 4 — Exception Handler Rollback (CYC +3)
The `catch (Exception ex)` block contains:
1. `if (syncPending)` conditional — clears sync only if it was set.
2. `if (reservedDelta != 0)` conditional — reverses `expectedPositions` only if delta was applied.
3. The try/catch control-flow split itself (CYC +1 for the catch branch).
Total branches contributed: **+3**. Full catch body extracted into `RollbackFleetFollowerState`; the try/catch skeleton and `return false` remain in outer method.

### Driver 5 — syncPending Dual Clear Path (CYC +2)
`syncPending` is cleared in two separate execution paths:
1. **Happy path**: `ClearDispatchSyncPending(expectedKey); syncPending = false;` after `acct.Submit` succeeds.
2. **Catch path**: `ClearDispatchSyncPending(expectedKey); syncPending = false;` inside `catch` (via `RollbackFleetFollowerState`).
These two clear paths represent two distinct code paths through the method, contributing **+2** to CYC. Post-extraction: happy-path clear stays in outer method; catch-path clear moves into `RollbackFleetFollowerState`.

---

## Method Signatures

```csharp
// Helper 1: Hot-skip eligibility filter
[System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
private bool IsAccountEligibleForRMADispatch(
    Account acct,
    StringBuilder dispatchLog)

// Helper 2: State registration in [923B-FIX-B] invariant order
private void RegisterFleetFollowerState(
    Account acct,
    string fleetKey,
    string expectedKey,
    PositionInfo fleetFollowerPos,
    Order fEntry,
    MarketPosition direction,
    int qty,
    StringBuilder dispatchLog,
    out bool syncPending,
    out int reservedDelta)

// Helper 3: Full atomic rollback on catch
[System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
private void RollbackFleetFollowerState(
    string fleetKey,
    string expectedKey,
    bool syncPending,
    int reservedDelta,
    StringBuilder dispatchLog,
    Account acct)
```

---

## Residual `ProcessSingleFleetRMAAccount` Skeleton

```csharp
private bool ProcessSingleFleetRMAAccount(
    Account acct, string baseSignal, OrderAction entryAction, int qty,
    double price, MarketPosition direction, RMABracketPrices prices,
    string symmetryDispatchId, StringBuilder dispatchLog)
{
    // Driver 1 extracted → IsAccountEligibleForRMADispatch
    if (!IsAccountEligibleForRMADispatch(acct, dispatchLog))
        return false;

    string fleetKey = acct.Name + "_RMA_" + baseSignal;
    string expectedKey = ExpKey(acct.Name);
    int reservedDelta = 0;
    bool syncPending = false;

    try
    {
        // Invariant #3: SymmetryGuard before all dict writes — NOT extracted
        SymmetryGuardRegisterFollower(symmetryDispatchId, fleetKey);

        // CreateOrder stays in outer method — null guard (Driver 2a) immediate after
        Order fEntry = acct.CreateOrder(...);
        if (fEntry == null) { dispatchLog...; return false; }  // Driver 2a — stays here

        // PositionInfo construction stays in outer method
        PositionInfo fleetFollowerPos = new PositionInfo { ... };

        // Driver 2b (FSM guard) + [923B-FIX-B] ordering + Driver 3a (ternary) extracted
        RegisterFleetFollowerState(acct, fleetKey, expectedKey, fleetFollowerPos,
            fEntry, direction, qty, dispatchLog, out syncPending, out reservedDelta);

        // Invariant #5: Submit is LAST — NOT extracted
        acct.Submit(new[] { fEntry });

        // Driver 3b: orderId guard stays here (post-Submit, needs live OrderId)
        if (fEntry != null && !string.IsNullOrEmpty(fEntry.OrderId))
            _orderIdToFsmKey[fEntry.OrderId] = fleetKey;

        ClearDispatchSyncPending(expectedKey);
        syncPending = false;

        dispatchLog.AppendLine(...);
        return true;
    }
    catch (Exception ex)
    {
        // Driver 4 + Driver 5 (catch path) extracted into RollbackFleetFollowerState
        RollbackFleetFollowerState(fleetKey, expectedKey, syncPending, reservedDelta,
            dispatchLog, acct);
        return false;
    }
}
```

---

## Jane Street Alignment

| Rule | Application |
|---|---|
| `carl_cook` zero-alloc | No new heap alloc in helpers; `PositionInfo` remains a struct (value type); rollback uses `ConcurrentDictionary.TryRemove` (no boxing); no LINQ |
| `carl_cook` `[AggressiveInlining]` | Applied to `IsAccountEligibleForRMADispatch` — hot-skip path called for every fleet account iteration; inlining removes call-frame overhead in the common-case filter |
| `carl_cook` `[NoInlining]` | Applied to `RollbackFleetFollowerState` — cold catch path; `NoInlining` keeps catch-handler frames out of JIT hot-path budget |
| `carl_cook` ref/in/out | `syncPending` and `reservedDelta` as `out` params from `RegisterFleetFollowerState`; passed by value to `RollbackFleetFollowerState` (int/bool — no boxing) |
| `gjengset` no lock() | Zero new `lock()` blocks introduced; existing `ConcurrentDictionary` ops remain; no lock contention added |
| `gjengset` volatile | `EnableConsistencyLock`, `EnableSIMA`, `activeFleetAccounts` volatile reads preserved in outer guards (passed into `IsAccountEligibleForRMADispatch` by reference/value, not cached in helpers) |
| `trading_billions` SRP | `IsEligible` = filter (query only, no writes), `Register` = state-write (5 surfaces in fixed order), `Rollback` = state-revert (inverse of Register) |
| `trading_billions` CYC ≤ 8 | `IsEligible`=4, `Register`=5, `Rollback`=5, Residual=6 — all ≤ 8 ✅ |
| `trading_billions` defense-in-depth | [923B-FIX-B] ordering contract encoded in `RegisterFleetFollowerState` implementation with inline comment; method name itself documents the ordering responsibility |

---

## MCP Evidence

### resolve_repo
- Repo: `antigravityos187-sketch/universal-or-strategy`
- Status: indexed, loadable
- Symbol count: 5,147 | File count: 2,000
- Source root: `/home/malhitticrypto/universal-or-strategy`

### get_context_bundle (ProcessSingleFleetRMAAccount)
- Symbol ID: `src/V12_002.SIMA.Execution.cs::V12_002.ProcessSingleFleetRMAAccount#method`
- Lines: 511–678 | Kind: method | Signature: `private bool ProcessSingleFleetRMAAccount(Account, string, OrderAction, int, double, MarketPosition, RMABracketPrices, string, StringBuilder)`
- Full source retrieved — [923B-FIX-B] inline comments confirmed; 5-surface dict pattern confirmed; PositionInfo struct construction confirmed; ConcurrentDictionary TryAdd/TryRemove confirmed.

### get_call_hierarchy (depth=2, direction=both)
- **Callers (depth=1):** `ExecuteRMAEntryV2` (`src/V12_002.SIMA.Execution.cs:686`) — sole caller; this is a loop over fleet accounts.
- **Key callees (depth=1):** `SymmetryGuardRegisterFollower`, `ExpKey`, `MarkDispatchSyncPending`, `AddExpectedPositionDeltaLocked`, `ClearDispatchSyncPending`, `activePositions`, `entryOrders`, `_followerBrackets`, `GetStableHash`, `LogBuffer.Format`
- **Key callees (depth=2):** `symmetryDispatchById` (Symmetry.cs), `_dispatchSyncPendingExpKeys`, `expectedPositions`, `StampAccountFillGrace` (REAPER.cs)
- **REAPER dependency confirmed:** `StampAccountFillGrace` in `V12_002.REAPER.cs` at depth=2 — validates the [923B-FIX-B] phantom-position repair risk.

### get_dependency_graph (src/V12_002.SIMA.Execution.cs, depth=1, both)
- Node count: 1 | Edge count: 0
- No external file-level import edges from `V12_002.SIMA.Execution.cs` — all dependencies resolved within the same partial class files (`V12_002.cs`, `V12_002.SIMA.cs`, etc.) via C# partial class mechanics. Extraction stays within same file/partial class — no import changes required.

---

## Sequential Thinking Evidence

### Thought 1 — Complexity Driver Analysis
Verified all 5 CYC drivers sum to 12: guard branching (+3 from 3 branches across 2 guards), null/FSM guards (+2), direction ternary + orderId compound guard (+2), try/catch + 2 catch conditionals (+3), total = base(1) + 11 branches = 12. Confirmed against actual source from get_context_bundle.

### Thought 2 — Extraction Strategy
Designed 3 helpers with CYC budgets: `IsEligible` CYC=4 (2 guards → 3 branches + base), `Register` CYC=5 (FSM-init guard + direction ternary + base + 2 internal), `Rollback` CYC=3–5 (2 conditionals + base). [923B-FIX-B] ordering preserved by internal sequence in `RegisterFleetFollowerState`. `SymmetryGuardRegisterFollower` and `acct.Submit` remain in outer method by invariant design.

### Thought 3 — CYC Validation + Jane Street Pass
All projected CYCs ≤ 8 ✅. `[AggressiveInlining]` on hot-filter correct. `[NoInlining]` on cold-rollback correct. No new lock() blocks. SyncPending out-param design preserves flag visibility across try/catch boundary. All 5 invariants preserved by extraction architecture.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Wave** | 7 |
| **Phase** | 2 |
| **Epic** | EPIC-W7-095 |
| **Method** | ProcessSingleFleetRMAAccount |
| **Source File** | src/V12_002.SIMA.Execution.cs |
| **Lines** | 511–678 |
| **CYC (actual)** | 12 |
| **CYC target** | ≤ 8 per method |
| **max_cyc_projected** | 5 |
| **Helpers extracted** | 3 |
| **Residual CYC** | 6 |
| **Jane Street KB** | carl_cook + gjengset + trading_billions applied |
| **Risk Level** | HIGH-CRITICALITY ([923B-FIX-B] ordering invariant) |
| **Invariants** | 5 — all preserved by extraction architecture |
