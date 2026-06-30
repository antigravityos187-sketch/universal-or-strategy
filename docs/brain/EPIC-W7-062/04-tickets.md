# EPIC-W7-062 — Phase 4: Ticket Definitions

**Agent Name:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29
**Inputs:**
- `docs/brain/EPIC-W7-062/02-architecture-plan.md`
- `docs/brain/EPIC-W7-062/03-audit-report.md`

---

## Summary

| Field | Value |
|---|---|
| **Epic** | EPIC-W7-062 |
| **Method** | `ProcessFleetSlot` |
| **File** | `src/V12_002.SIMA.Fleet.cs` |
| **CYC Baseline** | 13 |
| **CYC Target** | <= 8 |
| **Ticket Count** | 2 |
| **max_cyc_projected** | 8 |
| **DNA Verdict (Phase 3)** | PASS |
| **Violations** | [] |

---

## MCP Evidence

### get_symbol_complexity — ProcessFleetSlot

```json
{
  "repo": "antigravityos187-sketch/universal-or-strategy",
  "symbol_id": "src/V12_002.SIMA.Fleet.cs::V12_002.ProcessFleetSlot#method",
  "name": "ProcessFleetSlot",
  "kind": "method",
  "file": "src/V12_002.SIMA.Fleet.cs",
  "line": 44,
  "cyclomatic": 13,
  "max_nesting": 5,
  "param_count": 8,
  "lines": 54,
  "assessment": "high"
}
```

### get_extraction_candidates — src/V12_002.SIMA.Fleet.cs

```json
{
  "repo": "antigravityos187-sketch/universal-or-strategy",
  "file": "src/V12_002.SIMA.Fleet.cs",
  "candidates": [],
  "min_complexity": 5,
  "min_callers": 2
}
```

> Note: Zero automatic candidates returned because the two target helpers (`HandleFleetSlotCatch`,
> `HandleFleetSlotFinally`) do not yet exist in the index — they are new private methods to be
> introduced by this epic. The extraction plan is driven by the Phase 2 architecture analysis.

---

## Sequential Thinking Evidence

### Thought 1 — How many tickets? One ticket = one concern.

Two structurally distinct concerns exist inside `ProcessFleetSlot`:

1. **Catch block error recovery** — 2 conditional guards + rollback call. Self-contained;
   independent of the finally block. Maps to **Ticket T1**.
2. **Finally block cleanup + re-trigger** — pool release guard, atomic counters, circuit breaker,
   compound re-pump boolean gate (3 operators), inner try/catch with `_diagFleet` conditional.
   The primary complexity hotspot (contributes ~7 CYC). Maps to **Ticket T2**.

Conclusion: **2 tickets**, each covering one extraction.

### Thought 2 — For each ticket: lines moved, helper name, projected CYC.

**T1 — HandleFleetSlotCatch:**
- Lines moved: catch block internals — `if (!syncCleared)` guard +
  `if (reservedDelta != 0)` guard + `RollbackFleetDispatchState(...)` call
- Lines staying in parent catch: `Print(string.Format("[PUMP] Submit FAILED..."))` — no branches
- Call site: `HandleFleetSlotCatch(fleetEntryName, expectedKey, reservedDelta, syncCleared);`
- Projected CYC: base(1) + `if(!syncCleared)`(1) + `if(reservedDelta!=0)`(1) = **3**

**T2 — HandleFleetSlotFinally:**
- Lines moved: entire finally body — pool release guard, `Interlocked.Decrement`,
  `Volatile.Read`, `TryResetCircuitBreakerIfBelow`, compound re-pump gate, inner try/catch +
  `if (_diagFleet)` guard
- Lines staying in parent finally: `HandleFleetSlotFinally(poolSlotIndex);`
- Projected CYC: base(1) + pool guard(1) + null-check(1) + `&&`(1) + `||`(1) +
  inner try(1) + inner catch(1) + `if(_diagFleet)`(1) = **8**

### Thought 3 — CYC validation: all methods <= 8 post-extraction.

| Method | Branches | Projected CYC | Limit | Status |
|---|---|---|---|---|
| `ProcessFleetSlot` (residual) | base + `if(!Validate)` + catch | 3 | <= 8 | **PASS** |
| `HandleFleetSlotCatch` | 2 if guards | 3 | <= 8 | **PASS** |
| `HandleFleetSlotFinally` | 7 branches listed above | 8 | <= 8 | **PASS** (boundary) |
| **max_cyc_projected** | | **8** | <= 8 | **PASS** |

CYC delta: 13 → 8 max (reduction of 5). Constraint satisfied for all methods.

---

## Ticket Definitions

---

### T1 — Extract `HandleFleetSlotCatch`

| Field | Value |
|---|---|
| **Ticket ID** | EPIC-W7-062-T1 |
| **Title** | Extract catch block recovery logic into `HandleFleetSlotCatch` |
| **Priority** | P1 — Execute first (simpler, establishes extraction pattern) |
| **Phase** | 5.1 |
| **Agent** | Bob CLI (`v12-engineer`) |
| **File** | `src/V12_002.SIMA.Fleet.cs` |
| **Target Method** | `ProcessFleetSlot` (lines 44–97) |
| **CYC Before** | 13 (whole method) |
| **CYC After (helper)** | 3 |
| **CYC After (parent)** | Reduced by 2 branches (catch conditionals) |

#### What to Extract

Extract the following lines from the `catch (Exception ex)` block of `ProcessFleetSlot`:

```csharp
// EXTRACT these lines:
if (!syncCleared)
    ClearDispatchSyncPending(expectedKey);
if (reservedDelta != 0)
    AddExpectedPositionDeltaLocked(fleetEntryName, -reservedDelta);
RollbackFleetDispatchState(fleetEntryName, ...);
```

#### New Helper Signature

```csharp
private void HandleFleetSlotCatch(
    string fleetEntryName,
    string expectedKey,
    int reservedDelta,
    bool syncCleared)
{
    if (!syncCleared)
        ClearDispatchSyncPending(expectedKey);
    if (reservedDelta != 0)
        AddExpectedPositionDeltaLocked(fleetEntryName, -reservedDelta);
    RollbackFleetDispatchState(fleetEntryName, ...);
}
```

#### Residual Catch Block

```csharp
catch (Exception ex)
{
    Print(string.Format("[PUMP] Submit FAILED for {0} ({1}): {2}",
        fleetEntryName, acct.Name, ex.Message));
    HandleFleetSlotCatch(fleetEntryName, expectedKey, reservedDelta, syncCleared);
}
```

#### Acceptance Criteria

- [ ] `HandleFleetSlotCatch` private method added to same partial class in `src/V12_002.SIMA.Fleet.cs`
- [ ] Method signature matches exactly: `private void HandleFleetSlotCatch(string, string, int, bool)`
- [ ] Catch block in `ProcessFleetSlot` calls `HandleFleetSlotCatch(...)` as single delegating call
- [ ] `Print(string.Format(...))` logging line remains in parent catch (not moved)
- [ ] `syncCleared` variable still declared in `ProcessFleetSlot` outer scope
- [ ] No `lock()` blocks introduced
- [ ] ASCII-only string literals
- [ ] Build passes with zero errors

---

### T2 — Extract `HandleFleetSlotFinally`

| Field | Value |
|---|---|
| **Ticket ID** | EPIC-W7-062-T2 |
| **Title** | Extract finally block cleanup+re-trigger into `HandleFleetSlotFinally` |
| **Priority** | P1 — Execute after T1 (primary complexity target) |
| **Phase** | 5.2 |
| **Agent** | Bob CLI (`v12-engineer`) |
| **File** | `src/V12_002.SIMA.Fleet.cs` |
| **Target Method** | `ProcessFleetSlot` (lines 44–97) |
| **CYC Before** | Contributes ~7 CYC to parent (primary hotspot) |
| **CYC After (helper)** | 8 (exactly at boundary) |
| **CYC After (parent)** | 3 (residual after both extractions) |

#### What to Extract

Extract the entire body of the `finally` block from `ProcessFleetSlot`:

```csharp
// EXTRACT entire finally body:
if (poolSlotIndex >= 0)
    _fleetSlotPool.Release(poolSlotIndex);
Interlocked.Decrement(ref _activeFleetDispatches);
var current = Volatile.Read(ref _activeFleetDispatches);
TryResetCircuitBreakerIfBelow(current);
if (_fleetRepumpQueue != null
    && _fleetRepumpQueue.TryDequeue(out var repumpEntry)
    && repumpEntry != null)
{
    try { PumpFleetDispatch(repumpEntry); }
    catch (Exception diagEx)
    {
        if (_diagFleet)
            TriggerCustomEvent(string.Format("[FLEET] Repump error: {0}", diagEx.Message));
    }
}
```

#### New Helper Signature

```csharp
private void HandleFleetSlotFinally(int poolSlotIndex)
{
    if (poolSlotIndex >= 0)
        _fleetSlotPool.Release(poolSlotIndex);
    Interlocked.Decrement(ref _activeFleetDispatches);
    var current = Volatile.Read(ref _activeFleetDispatches);
    TryResetCircuitBreakerIfBelow(current);
    if (_fleetRepumpQueue != null
        && _fleetRepumpQueue.TryDequeue(out var repumpEntry)
        && repumpEntry != null)
    {
        try { PumpFleetDispatch(repumpEntry); }
        catch (Exception diagEx)
        {
            if (_diagFleet)
                TriggerCustomEvent(string.Format("[FLEET] Repump error: {0}", diagEx.Message));
        }
    }
}
```

#### Residual Finally Block

```csharp
finally
{
    HandleFleetSlotFinally(poolSlotIndex);
}
```

#### Acceptance Criteria

- [ ] `HandleFleetSlotFinally` private method added to same partial class in `src/V12_002.SIMA.Fleet.cs`
- [ ] Method signature matches exactly: `private void HandleFleetSlotFinally(int poolSlotIndex)`
- [ ] Finally block in `ProcessFleetSlot` contains only `HandleFleetSlotFinally(poolSlotIndex);`
- [ ] `Interlocked.Decrement` preserved (not wrapped in lock())
- [ ] `Volatile.Read` preserved (not replaced with direct field access)
- [ ] `TryResetCircuitBreakerIfBelow` call preserved in helper
- [ ] Inner try/catch for repump preserved with `_diagFleet` guard intact
- [ ] No `lock()` blocks introduced
- [ ] ASCII-only string literals (including `[FLEET]` prefix)
- [ ] Build passes with zero errors

---

## Residual `ProcessFleetSlot` (Post All Extractions)

```csharp
private void ProcessFleetSlot(
    Account acct, Order[] orders, int orderCount,
    string fleetEntryName, string expectedKey,
    int reservedDelta, long signalTicks, int poolSlotIndex)
{
    bool syncCleared = false;
    try
    {
        if (!ValidateDispatchTimestamp(signalTicks, fleetEntryName, expectedKey, reservedDelta, ref syncCleared))
            return;
        InitializeFollowerBracketFSM(orders, orderCount, fleetEntryName, acct.Name, reservedDelta);
        SubmitAndRegisterFleetOrders(acct, orders, orderCount, fleetEntryName, expectedKey, ref syncCleared);
    }
    catch (Exception ex)
    {
        Print(string.Format("[PUMP] Submit FAILED for {0} ({1}): {2}", fleetEntryName, acct.Name, ex.Message));
        HandleFleetSlotCatch(fleetEntryName, expectedKey, reservedDelta, syncCleared);
    }
    finally
    {
        HandleFleetSlotFinally(poolSlotIndex);
    }
}
```

**Residual CYC:** 3 (base + if-guard + catch) — well within limit.

---

## CYC Projection Summary

| Method | Projected CYC | Limit | Status |
|---|---|---|---|
| `ProcessFleetSlot` (residual) | 3 | <= 8 | **PASS** |
| `HandleFleetSlotCatch` | 3 | <= 8 | **PASS** |
| `HandleFleetSlotFinally` | 8 | <= 8 | **PASS** (at boundary) |
| **max_cyc_projected** | **8** | <= 8 | **PASS** |

**CYC Reduction:** 13 → 8 (delta = 5)

---

## V12.23 Scope Compliance

| Check | Status |
|---|---|
| Single method targeted (`ProcessFleetSlot`) | PASS |
| Callers NOT modified (`PumpFleetDispatch`, `ProcessValidPhotonSlot`) | PASS |
| No cross-file refactoring | PASS |
| Helpers are private, same partial class | PASS |
| No signature change to `ProcessFleetSlot` | PASS |
| No pre-existing bug fixes bundled | PASS |
| DNA audit Phase 3 verdict | PASS |

---

## Execution Order

```
T1 (HandleFleetSlotCatch) → T2 (HandleFleetSlotFinally)
```

Both tickets are implemented in a single Phase 5 session by Bob CLI (`v12-engineer`).
T1 is applied first to establish the pattern, then T2 completes the extraction.
Both can be committed atomically as they do not overlap in source lines.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Phase** | 4 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-062 |
| **Bobcoins Used** | 0.6 |
| **Ticket Count** | 2 |
| **max_cyc_projected** | 8 |
| **CYC Baseline** | 13 |
| **CYC Reduction** | 5 |
| **MCP Tools Used** | resolve_repo, sequentialthinking (probe), get_symbol_complexity, get_extraction_candidates |
| **Sequential Thinking Thoughts** | 3 |
