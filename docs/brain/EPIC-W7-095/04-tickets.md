# EPIC-W7-095 — Phase 4: Implementation Tickets
# ProcessSingleFleetRMAAccount

**Agent:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Input:** docs/brain/EPIC-W7-095/02-architecture-plan.md + docs/brain/EPIC-W7-095/03-audit-report.md
**Source File:** src/V12_002.SIMA.Execution.cs
**ticket_count:** 3

---

## Summary

`ProcessSingleFleetRMAAccount` has **CYC=12**. Three surgical extraction tickets reduce the parent to **residual CYC=6**, with no helper exceeding CYC=5. All projected CYCs satisfy the Jane Street threshold of ≤8.

| Ticket | Helper Method | CYC Reduction | Projected Helper CYC | Risk |
|--------|--------------|--------------|---------------------|------|
| T1 | `IsAccountEligibleForRMADispatch` | -3 | 4 | LOW |
| T2 | `RegisterFleetFollowerState` | -3 | 5 | HIGH-CRITICALITY |
| T3 | `RollbackFleetFollowerState` | -2 | 5 | MEDIUM |
| — | **Residual `ProcessSingleFleetRMAAccount`** | — | **6** | — |

**projected_parent_cyc_after_all: 6**

---

## Ticket T1 — Extract `IsAccountEligibleForRMADispatch`

**ticket_id:** EPIC-W7-095-T1
**helper_name:** `IsAccountEligibleForRMADispatch`
**concern:** Guard-branching eligibility filter (CYC Driver 1) — removes fleet-active dictionary check and P&L ceiling guard from parent method
**lines_to_move:** ~10 (Driver 1 guard block: `activeFleetAccounts.TryGetValue` compound check + `EnableConsistencyLock` outer + `dailyPL >= MaxDailyProfitCap` inner)
**cyc_reduction:** 3
**projected_helper_cyc:** 4

### Extraction Scope

Extract the following logic from `ProcessSingleFleetRMAAccount` into a new `[AggressiveInlining]` private bool method:

1. `activeFleetAccounts.TryGetValue(acct.Name, out bool isActive) || !isActive` — skips inactive or unregistered fleet accounts (compound branch: +1 to parent CYC removed)
2. `if (EnableConsistencyLock)` outer guard — skips consistency-locked accounts (branch: +1 removed)
3. `if (dailyPL >= MaxDailyProfitCap)` inner guard — skips over-ceiling accounts (branch: +1 removed)

### Method Signature

```csharp
[System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
private bool IsAccountEligibleForRMADispatch(
    Account acct,
    StringBuilder dispatchLog)
```

### Parent Method After Extraction

Replace the guard block with:

```csharp
if (!IsAccountEligibleForRMADispatch(acct, dispatchLog))
    return false;
```

### Invariants

- Pure query — no dict writes, no state mutation, no [923B-FIX-B] implications.
- `[AggressiveInlining]` required: this is called in every fleet-account loop iteration (hot path).
- Volatile reads on `EnableConsistencyLock` and `activeFleetAccounts` must be preserved as-is (no caching inside helper).

### CYC Arithmetic

| Before extraction | After extraction |
|---|---|
| Parent CYC: 12 | Parent CYC: 9 |
| Helper CYC: N/A | `IsAccountEligibleForRMADispatch` CYC: 4 |

### Risk Assessment

**LOW** — Pure filter with no side effects. No state writes. The [923B-FIX-B] ordering invariant is not implicated. Test by asserting return=false for inactive accounts and for accounts at/above P&L ceiling.

---

## Ticket T2 — Extract `RegisterFleetFollowerState`

**ticket_id:** EPIC-W7-095-T2
**helper_name:** `RegisterFleetFollowerState`
**concern:** [923B-FIX-B] state registration in strict invariant write-order — FSM init guard, dict writes BEFORE delta, MarkDispatchSyncPending, direction ternary, and AddExpectedPositionDeltaLocked
**lines_to_move:** ~25 (Driver 2b FSM guard + [923B-FIX-B] 5-write sequence + Driver 3a direction ternary + MarkDispatchSyncPending call)
**cyc_reduction:** 3
**projected_helper_cyc:** 5

### Extraction Scope

Extract the following logic from `ProcessSingleFleetRMAAccount` into a new private void method:

1. `if (!_followerBrackets.ContainsKey(fleetKey))` — FSM initialisation guard: creates `FollowerBracketFSM` only when not already registered (branch: +1 removed from parent)
2. `activePositions[fleetKey] = fleetFollowerPos` — first dict write ([923B-FIX-B] position 1)
3. `entryOrders[fleetKey] = fEntry` — second dict write ([923B-FIX-B] position 2)
4. `MarkDispatchSyncPending(expectedKey)` — syncPending flag set ([923B-FIX-B] position 3)
5. `reservedDelta = (direction == MarketPosition.Long) ? qty : -qty` — direction ternary (branch: +1 removed from parent)
6. `AddExpectedPositionDeltaLocked(reservedDelta)` — **LAST** dict-write surface ([923B-FIX-B] position 5; MUST remain after all dict writes above)

### Method Signature

```csharp
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
```

### Parent Method After Extraction

```csharp
RegisterFleetFollowerState(acct, fleetKey, expectedKey, fleetFollowerPos,
    fEntry, direction, qty, dispatchLog, out syncPending, out reservedDelta);
```

### ⚠️ HIGH-CRITICALITY: [923B-FIX-B] Write Ordering Invariant

> **THIS IS A CORRECTNESS CONTRACT. ANY RE-ORDERING CAUSES PHANTOM POSITION ERRORS DETECTED BY REAPER.**

The five write surfaces inside `RegisterFleetFollowerState` MUST execute in this exact order:

```
1. activePositions write
2. entryOrders write
3. MarkDispatchSyncPending
4. _followerBrackets FSM init (if not present)
5. AddExpectedPositionDeltaLocked  ← MUST be LAST
```

`AddExpectedPositionDeltaLocked` is read by the REAPER subsystem (`StampAccountFillGrace` at depth=2 in call hierarchy). If dict entries are absent when REAPER reads expectedPositions, phantom-position repair logic fires incorrectly. This ordering was established in fix [923B-FIX-B] and is non-negotiable.

Add the following inline comment at the top of `RegisterFleetFollowerState`:

```csharp
// [923B-FIX-B] WRITE ORDERING INVARIANT: activePositions + entryOrders + SyncPending
// MUST be written BEFORE AddExpectedPositionDeltaLocked. See REAPER.cs:StampAccountFillGrace.
// DO NOT reorder these writes.
```

### out Parameters

- `out bool syncPending` — set to `true` after `MarkDispatchSyncPending`; visible to parent for happy-path clear and to `RollbackFleetFollowerState` for conditional revert.
- `out int reservedDelta` — set from direction ternary; visible to parent and to `RollbackFleetFollowerState` for conditional delta reversal.

### CYC Arithmetic

| Before extraction | After extraction |
|---|---|
| Parent CYC: 9 (after T1) | Parent CYC: 6 |
| Helper CYC: N/A | `RegisterFleetFollowerState` CYC: 5 |

### Risk Assessment

**HIGH-CRITICALITY** — Contains [923B-FIX-B] correctness contract. Implement in a single atomic code block. Do not split the write sequence across conditionals. Write xUnit test asserting: (a) `activePositions` and `entryOrders` contain the new key after call; (b) `syncPending == true` after call; (c) `reservedDelta` sign matches `direction`.

---

## Ticket T3 — Extract `RollbackFleetFollowerState`

**ticket_id:** EPIC-W7-095-T3
**helper_name:** `RollbackFleetFollowerState`
**concern:** Exception catch rollback — full 5-surface atomic revert covering all write surfaces from RegisterFleetFollowerState, handling conditional sync-pending clear and conditional delta reversal
**lines_to_move:** ~14 (Driver 4 catch conditionals + Driver 5 catch-path syncPending clear + all 5 TryRemove/revert calls)
**cyc_reduction:** 2
**projected_helper_cyc:** 5

### Extraction Scope

Extract the following logic from the `catch (Exception ex)` block in `ProcessSingleFleetRMAAccount`:

1. `if (syncPending) { ClearDispatchSyncPending(expectedKey); syncPending = false; }` — conditional sync-pending revert (branch: +1 removed from catch body; catch skeleton stays in parent)
2. `if (reservedDelta != 0) { AddExpectedPositionDeltaLocked(-reservedDelta); }` — conditional delta reversal (branch: +1 removed from catch body)
3. `activePositions.TryRemove(fleetKey, out _)` — revert write surface 1
4. `entryOrders.TryRemove(fleetKey, out _)` — revert write surface 2
5. `_followerBrackets.TryRemove(fleetKey, out _)` — revert write surface 3
6. Log entry via `dispatchLog.AppendLine(...)` in catch context

### Method Signature

```csharp
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

### Parent Method After Extraction

```csharp
catch (Exception ex)
{
    RollbackFleetFollowerState(fleetKey, expectedKey, syncPending, reservedDelta,
        dispatchLog, acct);
    return false;
}
```

### Invariants

- `[NoInlining]` required: this is a cold catch path. `NoInlining` keeps catch-handler frames out of the JIT hot-path budget (Jane Street `carl_cook` rule).
- `syncPending` and `reservedDelta` passed by value (bool and int — no boxing).
- Must revert all 5 write surfaces registered by `RegisterFleetFollowerState` — no partial rollbacks permitted.
- The parent try/catch skeleton (and `return false`) remains in the outer method. Only the catch body is extracted.

### CYC Arithmetic

| Before extraction | After extraction |
|---|---|
| Parent CYC: 6 (after T1+T2) | Parent CYC: 6 (catch skeleton +1, but body branches removed; net = preserved at 6) |
| Helper CYC: N/A | `RollbackFleetFollowerState` CYC: 5 |

*Note: The try/catch control-flow boundary (+1) remains in the parent. The two conditional branches within the catch body (+2) move into the helper. Net parent CYC remains at 6 because the catch boundary itself was already counted in residual.*

### Risk Assessment

**MEDIUM** — Depends on T2's `out` parameter contract (`syncPending`, `reservedDelta`). Implement T2 before T3. Write xUnit test asserting: (a) `activePositions` does NOT contain `fleetKey` after rollback; (b) `entryOrders` does NOT contain `fleetKey`; (c) `_followerBrackets` does NOT contain `fleetKey`; (d) when `syncPending=true`, `ClearDispatchSyncPending` is called; (e) when `reservedDelta != 0`, the inverse delta is applied.

---

## Execution Order

| Step | Ticket | Dependency | Reason |
|------|--------|------------|--------|
| 1 | T1 — `IsAccountEligibleForRMADispatch` | None | Pure filter; isolated; reduces parent to CYC=9 |
| 2 | T2 — `RegisterFleetFollowerState` | T1 complete | Defines `out syncPending` + `out reservedDelta` needed by T3 |
| 3 | T3 — `RollbackFleetFollowerState` | T2 complete | Consumes T2's out-param contract; completes rollback symmetry |

---

## CYC Waterfall

```
ProcessSingleFleetRMAAccount  CYC=12  (baseline)
  - T1 extraction (-3)     ->  CYC=9
  - T2 extraction (-3)     ->  CYC=6
  - T3 extraction (body)   ->  CYC=6  (catch skeleton retained; body branches moved)
                                ^^
                           RESIDUAL = 6  ✅ (threshold: 8)

Helpers:
  IsAccountEligibleForRMADispatch   CYC=4  ✅
  RegisterFleetFollowerState        CYC=5  ✅
  RollbackFleetFollowerState        CYC=5  ✅
  max_cyc_projected                 CYC=5  ✅
```

---

## Critical Invariants Preserved by Ticket Architecture

| Invariant | How Preserved | Ticket |
|-----------|--------------|--------|
| [923B-FIX-B] dict BEFORE delta | Internal write order in `RegisterFleetFollowerState`; inline comment mandate | T2 |
| SyncPending brackets delta | `MarkDispatchSyncPending` in T2 (set); `ClearDispatchSyncPending` in T3 (clear on catch) + outer method (clear on happy path) | T2 + T3 |
| SymmetryGuard before dict | Outer method — NOT extracted; `SymmetryGuardRegisterFollower` stays before T2 call | None (outer) |
| Full rollback on catch | T3 reverts all 5 write surfaces symmetrically | T3 |
| Submit last | Outer method — NOT extracted; `acct.Submit` after T2 call | None (outer) |

---

## Jane Street Compliance

| Rule | Ticket | Compliance |
|------|--------|------------|
| `carl_cook` `[AggressiveInlining]` | T1 | Hot-path filter — inlining removes call-frame overhead |
| `carl_cook` `[NoInlining]` | T3 | Cold catch path — JIT budget preserved |
| `carl_cook` zero-alloc | T1, T2, T3 | No new heap allocs; `PositionInfo` struct; no LINQ |
| `gjengset` no lock() | All | Zero new lock() blocks; ConcurrentDictionary ops only |
| `trading_billions` SRP | All | T1=filter, T2=state-write, T3=state-revert |
| `trading_billions` CYC ≤ 8 | All | 4, 5, 5, residual 6 — all ≤ 8 ✅ |

---

## Sequential Thinking Evidence

### Thought 1 — Ticket Identification
Mapped all 5 CYC drivers to 3 tickets: Driver 1 (guard branching) → T1; Driver 2b+3a+[923B-FIX-B] writes → T2; Driver 4+5 catch body → T3. Confirmed lines_to_move estimates (~10, ~25, ~14) from Phase 2 architecture plan source analysis.

### Thought 2 — CYC Arithmetic Validation
Traced CYC waterfall: 12 → 9 (T1 removes 3) → 6 (T2 removes 3) → 6 (T3 removes catch body; catch skeleton retained at +1). Confirmed residual=6 matches Phase 2 target. Confirmed max_cyc_projected=5. All helpers ≤ 8 ✅.

### Thought 3 — Invariant and Risk Final Validation
Confirmed [923B-FIX-B] preserved by T2 internal ordering with inline comment mandate. Confirmed T3 depends on T2 out-param contract — execution order T1→T2→T3 mandatory. `[AggressiveInlining]` on T1 (hot path) and `[NoInlining]` on T3 (cold path) both validated. xUnit test assertions specified per ticket. All 5 invariants from architecture plan preserved by ticket architecture.

---

## Agent Tracking

| Field | Value |
|-------|-------|
| **Agent Name** | v12-phase4-tickets |
| **Wave** | 7 |
| **Phase** | 4 |
| **Epic** | EPIC-W7-095 |
| **Method** | ProcessSingleFleetRMAAccount |
| **Source File** | src/V12_002.SIMA.Execution.cs |
| **Lines** | 511–678 |
| **CYC (actual)** | 12 |
| **CYC target** | ≤ 8 per method |
| **ticket_count** | 3 |
| **max_cyc_projected** | 5 |
| **projected_parent_cyc_after_all** | 6 |
| **Risk Level** | HIGH-CRITICALITY ([923B-FIX-B] ordering invariant) |
| **DNA Verdict** | PASS (Phase 3) |
| **Violations** | [] |
| **MCP: resolve_repo** | antigravityos187-sketch/universal-or-strategy — indexed, loadable |
| **MCP: sequential_thinking** | 3 thoughts — ticket identification, CYC arithmetic, invariant validation |
