# EPIC-W7-093 — Phase 4: Implementation Tickets
# Dispatch_ProcessFleetLoop

**Agent:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Input:** docs/brain/EPIC-W7-093/02-architecture-plan.md + docs/brain/EPIC-W7-093/03-audit-report.md
**Timestamp:** 2026-06-29

---

## Summary

| Field | Value |
|---|---|
| **Epic** | EPIC-W7-093 |
| **Method** | `Dispatch_ProcessFleetLoop` |
| **Source File** | `src/V12_002.SIMA.Dispatch.cs` |
| **CYC Before** | 14 |
| **ticket_count** | 2 |
| **projected_parent_cyc_after_all** | 6 |
| **Max CYC Projected** | 6 (all methods ≤ 8 threshold) |
| **DNA Verdict** | PASS |
| **Extraction Strategy** | 2-helper surgical extraction (happy-path + catch-arm rollback) |

---

## Tickets

---

### TICKET-1: Extract `Dispatch_ExecuteFleetAccountEntry` (Happy-Path)

| Field | Value |
|---|---|
| **ticket_id** | TICKET-W7-093-1 |
| **helper_name** | `Dispatch_ExecuteFleetAccountEntry` |
| **concern** | Happy-path extraction: `Dispatch_BuildFollowerOrders` call, `!_builtOk` early-return guard, `isMarketEntry` order-type fork → `Dispatch_PublishMarketBracketToPhoton` or `Dispatch_PublishLimitEntryToPhoton` |
| **lines_to_move** | Try-block body of `Dispatch_ProcessFleetLoop` (approximately lines 220–295): the `BuildFollowerOrders` call, the `_builtOk` guard, the `isMarketEntry` dispatch fork, and both Publish call sites |
| **cyc_reduction** | 8 (removes from parent: `_builtOk` guard +1, `isMarketEntry` fork +1, `PublishMarket` call-site branch +1, `PublishLimit` call-site branch +1, plus internal Build/Publish complexity) |
| **projected_helper_cyc** | 5 |
| **attribute** | `[MethodImpl(MethodImplOptions.AggressiveInlining)]` |
| **return_type** | `bool` (`true` = publish succeeded; caller increments `rmaCount` on `true`) |
| **dependency** | None — execute first |

#### Method Signature

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

#### CYC Decomposition (Projected = 5)

| Branch | CYC |
|---|---|
| `Dispatch_BuildFollowerOrders` call path | +1 |
| `if (!_builtOk) return false` early-return guard | +1 |
| `if (isMarketEntry)` order-type fork | +1 |
| `Dispatch_PublishMarketBracketToPhoton` branch | +1 |
| `Dispatch_PublishLimitEntryToPhoton` branch | +1 |
| **Total** | **5** |

#### Jane Street Constraints

- `[AggressiveInlining]` — hot path, called once per fleet account per dispatch cycle (carl_cook pattern)
- Returns `false` on `!_builtOk`; caller uses `continue` — no exception needed
- `ref` params for state passing — zero heap allocation
- `out fleetEntryName` and `out expectedKey` must be established here; consumed by TICKET-2 Rollback helper

#### Residual Parent After This Ticket

`Dispatch_ProcessFleetLoop` try-block becomes a single call site:

```csharp
bool ok = Dispatch_ExecuteFleetAccountEntry(acct, fleet[i], tradeType, action, quantity,
    entryPrice, entryOrderType, i, symmetryDispatchId, dispatchTargetCount, dispatchLog,
    ref syncPending, ref reservedDelta, ref registeredForCleanup,
    out fleetEntryName, out expectedKey);
if (!ok)
    continue;
rmaCount++;
```

---

### TICKET-2: Extract `Dispatch_RollbackFleetAccountEntry` (Catch-Arm Rollback)

| Field | Value |
|---|---|
| **ticket_id** | TICKET-W7-093-2 |
| **helper_name** | `Dispatch_RollbackFleetAccountEntry` |
| **concern** | Catch-arm rollback and compensation state machine extraction: `syncPending` clear, `reservedDelta` reversal, 5-dict `TryRemove` cleanup loop, `_followerBrackets` FSM cleanup, log append |
| **lines_to_move** | Entire catch block body of `Dispatch_ProcessFleetLoop` (approximately lines 296–340): `ClearDispatchSyncPending`, `AddExpectedPositionDeltaLocked`, 5-dict for-loop TryRemove, `_followerBrackets.TryRemove`, log append |
| **cyc_reduction** | 5 (removes from parent: `syncPending` +1, `reservedDelta` +1, `registeredForCleanup` +1, `tNum` for-loop +1, `fleetEntryName` IsNullOrEmpty +1; `try/catch` skeleton remains) |
| **projected_helper_cyc** | 6 |
| **attribute** | `[MethodImpl(MethodImplOptions.NoInlining)]` |
| **return_type** | `void` |
| **dependency** | TICKET-1 must be completed first (`out fleetEntryName` and `out expectedKey` signatures established) |

#### Method Signature

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

#### CYC Decomposition (Projected = 6)

| Branch | CYC |
|---|---|
| `if (syncPending)` → `ClearDispatchSyncPending` + reset | +1 |
| `if (reservedDelta != 0)` → `AddExpectedPositionDeltaLocked` reversal | +1 |
| `if (registeredForCleanup)` → enter 5-dict cleanup | +1 |
| `for (int tNum = 1; tNum <= 5; tNum++)` inner cleanup loop | +1 |
| `if (targetDict != null)` null-guard on `GetTargetOrdersDictionary` | +1 |
| `if (!string.IsNullOrEmpty(fleetEntryName))` FSM bracket cleanup | +1 |
| **Total** | **6** |

#### Jane Street Constraints

- `[NoInlining]` — cold catch/error path; inlining pollutes hot-path instruction cache (carl_cook pattern)
- `ref syncPending`, `ref reservedDelta` — caller's bool/int updated in-place after rollback, zero heap alloc
- Called exclusively from catch block of `Dispatch_ProcessFleetLoop`; no other call sites
- `registeredForCleanup`, `fleetEntryName`, `expectedKey` received as value params (by-value snapshot of state at catch entry)

#### Residual Parent After This Ticket

`Dispatch_ProcessFleetLoop` catch block becomes a single call site:

```csharp
catch (Exception ex)
{
    Dispatch_RollbackFleetAccountEntry(ref syncPending, ref reservedDelta,
        registeredForCleanup, fleetEntryName, expectedKey, acct, ex, dispatchLog);
}
```

---

## Residual `Dispatch_ProcessFleetLoop` After All Tickets

### Projected CYC = 6

| Branch | CYC |
|---|---|
| `for` loop over fleet accounts | +1 |
| `if (acct == this.Account) continue` master-account skip | +1 |
| `if (ShouldSkipFleetAccount(...)) continue` health check | +1 |
| `if (Volatile.Read(ref _reaperCircuitBreakerTripped) == 1) continue` circuit-breaker | +1 |
| `try/catch` block structure | +1 |
| `if (!ok) continue` after Execute call | +1 |
| **Total** | **6** |

### Critical Ordering Constraints (MUST NOT VIOLATE)

1. **`Volatile.Read(_reaperCircuitBreakerTripped)` MUST remain as the third guard in the outer loop BEFORE the `Dispatch_ExecuteFleetAccountEntry` call** — Jane Street `gjengset volatile` + `carl_cook zero-alloc` rule: no `out` param locals allocated when the circuit-breaker is tripped.

2. **`rmaCount++` MUST remain in the outer loop, only on `true` return from `Dispatch_ExecuteFleetAccountEntry`** — `rmaCount` counts successfully dispatched follower accounts; must not be incremented on `!_builtOk`, circuit-breaker skip, or catch-arm entry.

3. **Public signature of `Dispatch_ProcessFleetLoop` is unchanged** — sole caller `ExecuteSmartDispatchEntry` (line 45, same file) requires zero blast-radius extraction.

---

## CYC Reduction Summary

| Method | CYC Before | CYC After | Reduction |
|---|---|---|---|
| `Dispatch_ProcessFleetLoop` (parent) | 14 | 6 | -8 |
| `Dispatch_ExecuteFleetAccountEntry` (new) | — | 5 | new |
| `Dispatch_RollbackFleetAccountEntry` (new) | — | 6 | new |
| **projected_parent_cyc_after_all** | — | **6** | — |

All methods ≤ 8 (Jane Street strict threshold). Extraction complete with 2 tickets.

---

## Execution Order

```
TICKET-W7-093-1  →  TICKET-W7-093-2
(Execute helper)     (Rollback helper)
     ↓                     ↓
  CYC=5               CYC=6
                        ↓
              Residual parent CYC=6
```

TICKET-2 depends on TICKET-1 because the `out fleetEntryName` and `out expectedKey` parameters are established by the Execute helper's signature and consumed by the Rollback helper's catch-site call.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Wave** | 7 |
| **Phase** | 4 |
| **Epic** | EPIC-W7-093 |
| **ticket_count** | 2 |
| **projected_parent_cyc_after_all** | 6 |
| **Sequential Thinking Thoughts** | 3 (ticket breakdown, CYC accounting, execution constraints) |
| **MCP Tools Used** | resolve_repo, sequential-thinking (x3), read_file |
| **DNA Verdict Input** | PASS (Phase 3) |
| **Jane Street KB** | carl_cook + gjengset + trading_billions applied |
