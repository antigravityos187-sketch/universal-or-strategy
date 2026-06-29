# EPIC-W7-119 — Phase 2: Architecture Plan

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Epic:** EPIC-W7-119
**Method:** `Dispatch_ProcessFleetLoop`
**Source File:** `src/V12_002.SIMA.Dispatch.cs`
**CYC Baseline:** 14
**CYC Target:** ≤ 8

---

## Extraction Plan

| # | New Helper | Signature | Extracted Logic | CYC Projected | Jane Street Attribute |
|---|-----------|-----------|-----------------|---------------|----------------------|
| 1 | `ShouldSkipFleetIteration` | `private bool ShouldSkipFleetIteration(Account acct, StringBuilder dispatchLog)` | 3 early-continue guards: CB tripped check (`_reaperCircuitBreakerTripped`). Note: `ShouldSkipFleetAccount` already exists; this adds CB guard only. Returns `true` = skip. | 2 | `[MethodImpl(AggressiveInlining)]` — hot-path per-iteration predicate, zero-alloc |
| 2 | `Dispatch_RollbackFleetSlot` | `private void Dispatch_RollbackFleetSlot(string fleetEntryName)` | For-loop rollback of 5 target order dicts + null-guard inside catch body. Removes `for` + `if targetDict != null` from catch scope. | 3 | `[MethodImpl(NoInlining)]` — cold error-recovery path |
| 3 | `Dispatch_HandleFleetSlotException` | `private void Dispatch_HandleFleetSlotException(Exception ex, bool syncPending, int reservedDelta, bool registeredForCleanup, string fleetEntryName, string expectedKey, Account acct, StringBuilder dispatchLog)` | Full catch body: syncPending rollback, reservedDelta rollback, registeredForCleanup cleanup (calls `Dispatch_RollbackFleetSlot`), FSM cleanup, log append. Removes 4 if-guards from parent catch. | 5 | `[MethodImpl(NoInlining)]` — cold error path |

### Parent Method CYC After Extraction

| Path | Branches | Count |
|------|----------|-------|
| Base | +1 | 1 |
| `for (i < fleet.Count)` loop | +1 | 2 |
| `acct == this.Account` skip | +1 | 3 |
| `ShouldSkipFleetAccount(...)` skip | +1 | 4 |
| `ShouldSkipFleetIteration(...)` CB skip | +1 | 5 |
| `if (!_builtOk) continue` | +1 | 6 |
| `if (isMarketEntry)` | +1 | 7 |
| `catch` → `Dispatch_HandleFleetSlotException(...)` | — | 7 |

**max_cyc_projected = 7** ✓ (parent); **3** (Dispatch_RollbackFleetSlot); **5** (Dispatch_HandleFleetSlotException); **2** (ShouldSkipFleetIteration)

---

## Refactored Method Sketch

```csharp
// New helper: CB guard only (ShouldSkipFleetAccount already handles inactive+H13+consistency)
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private bool ShouldSkipFleetIteration(Account acct, StringBuilder dispatchLog)
{
    if (Volatile.Read(ref _reaperCircuitBreakerTripped) == 1)
    {
        dispatchLog.AppendLine($"[DISPATCH] CB tripped - skipping {acct.Name} (no allocation)");
        return true;
    }
    return false;
}

// New helper: 5-target rollback loop
[MethodImpl(MethodImplOptions.NoInlining)]
private void Dispatch_RollbackFleetSlot(string fleetEntryName)
{
    activePositions.TryRemove(fleetEntryName, out _);
    entryOrders.TryRemove(fleetEntryName, out _);
    stopOrders.TryRemove(fleetEntryName, out _);
    for (int tNum = 1; tNum <= 5; tNum++)
    {
        var targetDict = GetTargetOrdersDictionary(tNum);
        if (targetDict != null)
            targetDict.TryRemove(fleetEntryName, out _);
    }
}

// New helper: full catch handler
[MethodImpl(MethodImplOptions.NoInlining)]
private void Dispatch_HandleFleetSlotException(
    Exception ex,
    bool syncPending,
    int reservedDelta,
    bool registeredForCleanup,
    string fleetEntryName,
    string expectedKey,
    Account acct,
    StringBuilder dispatchLog)
{
    if (syncPending)
    {
        ClearDispatchSyncPending(expectedKey);
    }
    if (reservedDelta != 0)
        AddExpectedPositionDeltaLocked(expectedKey, -reservedDelta);
    if (registeredForCleanup)
        Dispatch_RollbackFleetSlot(fleetEntryName);
    if (!string.IsNullOrEmpty(fleetEntryName))
        _followerBrackets.TryRemove(fleetEntryName, out _);
    dispatchLog.AppendLine($"[DISPATCH] [X] FAILED on {acct.Name}: {ex.Message}");
}

// Refactored parent — CYC = 7
private int Dispatch_ProcessFleetLoop(/* ... same signature ... */)
{
    int rmaCount = 0;
    for (int i = 0; i < fleet.Count; i++)
    {
        Account acct = fleet[i].Account;
        if (acct == this.Account)
            continue;
        if (ShouldSkipFleetAccount(acct, fleet[i], activeAccountSnapshot, dispatchLog))
            continue;
        if (ShouldSkipFleetIteration(acct, dispatchLog))
            continue;
        // ... local vars ...
        try
        {
            bool _builtOk = Dispatch_BuildFollowerOrders(/* ... */);
            if (!_builtOk)
                continue;
            bool isMarketEntry = (entryOrderType == OrderType.Market);
            if (isMarketEntry)
                Dispatch_PublishMarketBracketToPhoton(/* ... */);
            else
                Dispatch_PublishLimitEntryToPhoton(/* ... */);
            rmaCount++;
        }
        catch (Exception ex)
        {
            Dispatch_HandleFleetSlotException(ex, syncPending, reservedDelta,
                registeredForCleanup, fleetEntryName, expectedKey, acct, dispatchLog);
        }
    }
    return rmaCount;
}
```

---

## MCP Evidence

| Tool | Key Finding |
|------|-------------|
| `get_context_bundle` | Full 153-line source retrieved. CYC=14 confirmed: for×1, if×10, catch×1, CB volatile read. Catch body contains 4 independent `if` guards + 5-target rollback for-loop. |
| `get_call_hierarchy` | 1 caller: `ExecuteSmartDispatchEntry` (line 45, same file). 88 callees at depth 2. Key: `ShouldSkipFleetAccount`, `Dispatch_BuildFollowerOrders`, `Dispatch_PublishMarketBracketToPhoton`, `Dispatch_PublishLimitEntryToPhoton`, `ClearDispatchSyncPending`, `AddExpectedPositionDeltaLocked`, `GetTargetOrdersDictionary`. |

---

## Sequential Thinking Evidence

| Thought | Finding |
|---------|---------|
| 1 — Complexity Drivers | 3 guard clauses in loop body (flatten/dedup/CB), market-vs-limit split, catch body with 4 guards + rollback for-loop (total CYC 14) |
| 2 — Extraction Strategy | Extract `ShouldSkipFleetIteration` (CB guard, AggressiveInlining), `Dispatch_RollbackFleetSlot` (for+if rollback), `Dispatch_HandleFleetSlotException` (full catch body calling rollback helper). Parent CYC reduces from 14 to 7. |
| 3 — CYC Validation | Parent=7 ✓; ShouldSkipFleetIteration=2 ✓; Dispatch_RollbackFleetSlot=3 ✓; Dispatch_HandleFleetSlotException=5 ✓. All ≤ 8. Thread-affinity preserved (no new sync primitives). |

---

## Jane Street Compliance

| Rule | Applied |
|------|---------|
| Zero-alloc hot path | `ShouldSkipFleetIteration`: pure guard, zero-alloc; only heap alloc is the string format (dispatchLog.AppendLine on CB trip — cold branch) |
| AggressiveInlining hot / NoInlining cold | `ShouldSkipFleetIteration`: AggressiveInlining; `Dispatch_RollbackFleetSlot` + `Dispatch_HandleFleetSlotException`: NoInlining ✓ |
| No new `lock()` blocks | `Volatile.Read` for CB check (lock-free); `ConcurrentDictionary.TryRemove` (lock-free) ✓ |
| Single responsibility per helper | Each helper: one concern (guard, rollback, exception handling) ✓ |
| Each helper CYC ≤ 8 | 2, 3, 5 ✓ |
| Avoid LINQ | No LINQ ✓ |

---

## Agent Tracking

| Field | Value |
|-------|-------|
| **Agent Name** | v12-phase2-architecture |
| **Phase** | 2 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-119 |
| **CYC Baseline** | 14 |
| **max_cyc_projected** | 7 (parent) |
| **Extractions** | 3 |
