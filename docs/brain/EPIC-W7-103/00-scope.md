# Phase 1: Scope Definition — EPIC-W7-103

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Epic**: EPIC-W7-103
- **Execution Time**: 2026-06-23

---

## Method Under Refactoring

| Attribute          | Value                                      |
|--------------------|--------------------------------------------|
| **Method**         | `ProcessFleetSlot`                         |
| **File**           | `src/V12_002.SIMA.Fleet.cs`                |
| **Line**           | 44                                         |
| **Current CYC**    | 13 (threshold: ≤ 8)                        |
| **Max Nesting**    | 5                                          |
| **Parameters**     | 8                                          |
| **Lines of Code**  | 54                                         |
| **Visibility**     | `private`                                  |

### Current Structure

`ProcessFleetSlot` has three structural zones:

1. **`try` block** (lines 57–65): timestamp validation → FSM init → order submission.  
   Already delegated to three existing helpers: `ValidateDispatchTimestamp`,
   `InitializeFollowerBracketFSM`, `SubmitAndRegisterFleetOrders`.

2. **`catch` block** (lines 67–75): prints error, conditionally clears sync-pending,
   conditionally rolls back delta, rolls back dispatch state. Flat, no nested branches
   beyond two `if` guards — contributes ~2 CYC.

3. **`finally` block** (lines 77–96): this is the **sole remaining complexity source**.  
   It performs four sequential concerns:
   - Pool slot release (conditional `if (poolSlotIndex >= 0)`)
   - `Interlocked.Decrement` on `_pendingFleetDispatchCount`
   - Circuit-breaker reset via `TryResetCircuitBreakerIfBelow`
   - Conditional pump-priming (`if` on ring/queue non-empty) with an inner `try/catch`
     referencing `TriggerCustomEvent` → adds nesting depth 4–5 and ~4 CYC

The `catch` block contributes ~2 CYC. The `finally` block contributes ~5 CYC (one outer `if`,
one compound boolean `if`, one inner `try/catch`). Together with the surrounding
`try/catch/finally` structure itself and the top-level `try` guard (~2 CYC), the total is 13.

---

## IN SCOPE — Extractions Required

The following two private helper methods shall be extracted from the `finally` block of
`ProcessFleetSlot` to bring CYC to ≤ 8.

### Helper 1: `ReleaseFleetSlotResources`

**Purpose**: Encapsulate the resource-release and counter-decrement step that must always
run regardless of outcome.

**Extracted logic**:
```csharp
if (poolSlotIndex >= 0)
    _photonPool.ReleaseByIndex(poolSlotIndex);
Interlocked.Decrement(ref _pendingFleetDispatchCount);
int currentCount = Volatile.Read(ref _pendingFleetDispatchCount);
TryResetCircuitBreakerIfBelow(currentCount);
```

**Proposed signature**:
```csharp
private void ReleaseFleetSlotResources(int poolSlotIndex)
```

**CYC contribution after extraction**: 1 (linear, single conditional inside helper is counted
there, not in `ProcessFleetSlot`).

---

### Helper 2: `TryPrimeFleetPump`

**Purpose**: Encapsulate the conditional pump-prime logic (ring/queue non-empty check +
`TriggerCustomEvent` with guarded catch) that re-schedules `PumpFleetDispatch`.

**Extracted logic**:
```csharp
if ((_photonDispatchRing != null && !_photonDispatchRing.IsEmpty) || !_pendingFleetDispatches.IsEmpty)
    try
    {
        TriggerCustomEvent(o => PumpFleetDispatch(), null);
    }
    catch (Exception ex)
    {
        if (_diagFleet)
            Print("[FLEET_CATCH] ProcessFleetSlot pump prime failed: " + ex.Message);
    }
```

**Proposed signature**:
```csharp
private void TryPrimeFleetPump()
```

**CYC contribution after extraction**: 1 (single call site in `ProcessFleetSlot`; the
internal branching counts against the helper's own CYC, not the parent).

---

## Resulting `finally` Block After Extraction

```csharp
finally
{
    ReleaseFleetSlotResources(poolSlotIndex);
    TryPrimeFleetPump();
}
```

Two statements, zero branches — contributes CYC 1.

---

## Projected CYC Breakdown After Refactoring

| Zone                          | CYC Before | CYC After |
|-------------------------------|-----------|-----------|
| `try` block (3 callees)       | 2         | 2         |
| `catch` block (2 `if` guards) | 2         | 2         |
| `finally` block               | 5         | 1         |
| Structural overhead           | 4         | 3         |
| **Total**                     | **13**    | **≤ 8**   |

---

## OUT OF SCOPE

- **Signature of `ProcessFleetSlot` is unchanged** — same 8 parameters, same return type
  (`void`), same `private` visibility.
- **No behavior change** — all logic is moved verbatim; no reordering, no new conditions,
  no semantic alterations.
- **`try` and `catch` blocks are not touched** — `ValidateDispatchTimestamp`,
  `InitializeFollowerBracketFSM`, `SubmitAndRegisterFleetOrders`, and the catch rollback
  logic remain exactly as-is.
- **All other methods in `V12_002.SIMA.Fleet.cs` are untouched** — including
  `PumpFleetDispatch`, `VerifyPhotonSlotIntegrity`, `ProcessValidPhotonSlot`,
  `ShouldSkipFleetAccount`, `RollbackFleetDispatchState`, and all helpers.
- **No callers are modified** — `PumpFleetDispatch` (line 233), `ProcessValidPhotonSlot`
  (line 395), and `VerifyPhotonSlotIntegrity` (line 329) call `ProcessFleetSlot` with
  identical arguments; their call sites are untouched.
- **No new fields, properties, or types** introduced.
- **No changes to any file outside `src/V12_002.SIMA.Fleet.cs`**.

---

## Extraction Plan

### Step 1 — Extract `ReleaseFleetSlotResources`

1. Cut the four lines (pool release conditional, `Interlocked.Decrement`, `Volatile.Read`,
   `TryResetCircuitBreakerIfBelow` call) from the `finally` block.
2. Paste into a new `private void ReleaseFleetSlotResources(int poolSlotIndex)` method
   placed immediately after `ProcessFleetSlot` in the same `#region V12 SIMA Fleet`.
3. Replace the cut lines in `finally` with: `ReleaseFleetSlotResources(poolSlotIndex);`

### Step 2 — Extract `TryPrimeFleetPump`

1. Cut the compound `if` + inner `try/catch` block from the `finally` block.
2. Paste into a new `private void TryPrimeFleetPump()` method placed immediately after
   `ReleaseFleetSlotResources`.
3. Replace the cut block in `finally` with: `TryPrimeFleetPump();`

### Step 3 — Verify

- Confirm `ProcessFleetSlot` `finally` block now contains exactly two statements.
- Confirm no `ref` or `out` parameters are needed by either helper (neither mutates
  `syncCleared` or any other `ref` variable; `poolSlotIndex` is read-only).
- Confirm `_photonPool`, `_pendingFleetDispatchCount`, `_photonDispatchRing`,
  `_pendingFleetDispatches`, `_diagFleet` are all instance fields accessible from the new
  private helpers without parameter threading.

---

## Risk Assessment

| Risk                          | Severity | Mitigation                                              |
|-------------------------------|----------|---------------------------------------------------------|
| `poolSlotIndex` passed by value — correct, no aliasing concern | LOW | `int` is value type; helper receives copy |
| `finally` semantic preserved — helpers are called in-order, unconditionally | LOW | Extraction is purely structural; no control-flow change |
| `_pendingFleetDispatchCount` volatile reads must stay paired with the decrement | LOW | Both remain inside `ReleaseFleetSlotResources` in original order |
| `TriggerCustomEvent` lambda capture (`PumpFleetDispatch`) — no new closure variables | LOW | Lambda body unchanged; no additional captures introduced |
| Blast radius of callers | LOW | Phase 0 confirmed private scope, 3 callers, no external importers |
| Regression in `catch` rollback path — `syncCleared` not involved in `finally` | NONE | `finally` helpers do not touch `syncCleared` |

**Overall residual risk: LOW**

---

## Success Criteria

1. `ProcessFleetSlot` cyclomatic complexity ≤ 8 after extraction (target: ~8).
2. Two new private helpers added: `ReleaseFleetSlotResources(int)` and `TryPrimeFleetPump()`.
3. `ProcessFleetSlot` signature unchanged (8 parameters, `private void`).
4. Zero behavior change — all existing logic executed identically on all code paths.
5. All three callers (`PumpFleetDispatch`, `ProcessValidPhotonSlot`,
   `VerifyPhotonSlotIntegrity`) compile and call `ProcessFleetSlot` without modification.
6. No other methods in the file are modified.
7. File compiles without errors or new warnings.
