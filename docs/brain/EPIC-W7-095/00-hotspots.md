# EPIC-W7-095 — Hotspot Analysis
## Method: `ProcessSingleFleetRMAAccount`
**Source**: `src/V12_002.SIMA.Execution.cs` · Lines 511–678  
**Wave**: 7 | **Phase**: 0 — Hotspot Analysis  
**Generated**: 2026-06-14

---

## 1. Symbol Metadata

| Field | Value |
|-------|-------|
| Method | `ProcessSingleFleetRMAAccount` |
| Class | `V12_002` (partial: SIMA.Execution) |
| Return type | `bool` |
| Access | `private` |
| Lines (LOC) | 106 (lines 511–678) |
| Cyclomatic Complexity (wave7_methods.json) | **12** |
| Cyclomatic Complexity (PHASE0 crossref / historical) | 25 (pre-refactor estimate; stale jcodemunch index) |
| Cyclomatic Complexity (epic manifest cyc field) | **0** (wave7-epic-list.json: needs measurement) |
| Complexity Audit Action | `LOC > 80` violation flagged |
| CodeScene Visual | **RED** — HIGH PRIORITY |
| jCodemunch Rank | 20 (Rank 20 in crossref matrix) |

> **CYC Discrepancy Note**: The `wave7-epic-list.json` records `cyc: 0` (placeholder). The structural audit
> (`complexity_audit_full.txt` line 576) measures **CYC 12**, confirmed by `wave7_methods.json` line 355.
> The older PHASE0 crossref cited CYC 25, which was a pre-refactor or stale-index estimate.
> **Confirmed CYC for this epic: 12** (as measured on current source).

---

## 2. Method Signature & Parameters

```csharp
private bool ProcessSingleFleetRMAAccount(
    Account acct,
    string baseSignal,
    OrderAction entryAction,
    int qty,
    double price,
    MarketPosition direction,
    RMABracketPrices prices,
    string symmetryDispatchId,
    StringBuilder dispatchLog
)
```

**9 parameters** — already approaching max recommended arity; a parameter-object refactor is a candidate.

---

## 3. Responsibility Summary

`ProcessSingleFleetRMAAccount` is the **fleet follower dispatch unit** for RMA (Risk-Managed Approach) entries.
Called once per fleet account from `ExecuteRMAEntryV2` (line 782). It:

1. **Guards** — skips inactive accounts (`activeFleetAccounts`) and accounts that have hit the daily profit cap (`EnableConsistencyLock` / `MaxDailyProfitCap`).
2. **Creates** a limit entry order via `acct.CreateOrder(...)`, guarding the null-return case [M8.1 NRE-01].
3. **Atomically registers** tracking state into `activePositions`, `entryOrders` — BEFORE incrementing `expectedPositions` (Race-condition fix [923B-FIX-B]).
4. **Marks sync-pending** via `MarkDispatchSyncPending` to block premature REAPER scans.
5. **Initialises FSM** — proactively creates a `FollowerBracketFSM` in `_followerBrackets` (Phase 6 / FSM-P3).
6. **Increments** `expectedPositions` via `AddExpectedPositionDeltaLocked` (SECOND, after dict registration).
7. **Submits** the entry order with `acct.Submit(...)` (LAST — stateLock not held).
8. **Registers** the order ID → FSM key in `_orderIdToFsmKey` for O(1) FSM lookup.
9. **Clears sync-pending** and returns `true`.
10. **Exception handler** — full rollback: clears sync, reverses `expectedPositions`, removes from all tracking dicts and FSM.

---

## 4. Blast Radius

### Direct Caller
| Caller | File | Line |
|--------|------|------|
| `ExecuteRMAEntryV2` | `src/V12_002.SIMA.Execution.cs` | 782 |

### State Mutated (write surfaces — risk surface on failure)
| Data Structure | Operation | Ordering Constraint |
|----------------|-----------|---------------------|
| `activePositions` | Add / TryRemove | FIRST (before expectedPositions) |
| `entryOrders` | Add / TryRemove | FIRST (before expectedPositions) |
| `_followerBrackets` | TryAdd / TryRemove | FIRST (before expectedPositions) |
| `expectedPositions` (via `AddExpectedPositionDeltaLocked`) | +delta / -delta | SECOND |
| `_orderIdToFsmKey` | Add | After Submit |
| DispatchSync pending flag | Mark / Clear | Wraps expectedPositions increment |
| `SymmetryGuard` follower registry | RegisterFollower | Entry to try block |
| NinjaTrader broker (`acct.Submit`) | Submit limit order | LAST |

### Downstream systems touched
- **REAPER thread** — reads `entryOrders` + `expectedPositions` to detect phantom positions; ordering invariant is critical.
- **SymmetryGuard** — bracket submission on fill reads FSM registered here.
- **OnAccountExecutionUpdate** — on fill, submits brackets deferred from this method (V12.10 unified entry-only pattern).
- **ManageCIT** — chases all fleet entry orders via keys registered here.

---

## 5. Complexity Drivers

| Driver | Detail |
|--------|--------|
| **Guard branching** | 2 early-return guards (fleet-active check + consistency lock) add 3 branches |
| **Null-return guard** | `CreateOrder` null check [M8.1 NRE-01] |
| **FSM conditional** | `!_followerBrackets.ContainsKey(fleetKey)` initialisation branch |
| **Direction ternary** | `(direction == Long) ? qty : -qty` |
| **OrderId guard** | Null/empty check before `_orderIdToFsmKey` insert |
| **Exception handler** | `catch` with rollback logic (conditional delta, 3× dict removals, syncPending flag) |
| **`syncPending` flag** | Dual clear paths (happy path + catch) |
| **LOC > 80** | 106 lines — well above the 80-line extraction threshold |

---

## 6. Critical Invariants (must be preserved in any refactor)

1. **`activePositions` / `entryOrders` registered BEFORE `AddExpectedPositionDeltaLocked`** — [923B-FIX-B].
   Violating this ordering causes phantom-position repair by REAPER → double fill.
2. **`MarkDispatchSyncPending` / `ClearDispatchSyncPending` must bracket the `expectedPositions` increment**.
3. **Full rollback in `catch`**: all five write surfaces must be reverted on `Submit` failure.
4. **`SymmetryGuardRegisterFollower` called at try-entry** — before any dict writes; ensures follower is
   tracked even if dict registration partially fails.
5. **`acct.Submit` is the final step** — stateLock must NOT be held at call site.

---

## 7. Refactoring Candidates (Phase 1 Preview)

| Candidate | Rationale |
|-----------|-----------|
| Extract `BuildFleetFollowerPositionInfo(...)` | `PositionInfo` initialisation block (lines 588–617) is pure data construction |
| Extract `RegisterFleetFollowerState(...)` | Dict + FSM registration + `expectedPositions` increment is a reusable atomic unit |
| Extract `RollbackFleetFollowerState(...)` | The catch rollback mirrors `ExecuteSmartDispatchEntry` catch — shared helper opportunity |
| Parameter-object `FleetRMADispatchContext` | 9-parameter signature → single context struct |
| Extract guard into `IsAccountEligibleForDispatch(acct, dispatchLog)` | 2 early-return guards are duplicated across `ExecuteMultiAccountMarket` and `ExecuteMultiAccountBracket` |

---

## 8. Tool Cross-Reference

| Tool | Signal | Value |
|------|--------|-------|
| `wave7_methods.json` | Structural CYC | 12 |
| `complexity_audit_full.txt` | LOC / CYC | LOC=106 / CYC=12 |
| `WAVE7_COMPLETE_METHOD_LIST.md` | Rank in wave | #83 of 180 |
| `PHASE0_CODESCENE_JCODEMUNCH_CROSSREF.md` | CodeScene | RED — HIGH PRIORITY |
| `wave7-epic-list.json` | Epic manifest cyc | 0 (placeholder) |
| `autonomous_refactor_baseline_corrected.md` | Verified CYC | 12, LOC=106, status=HIGH |

---

## 9. Conclusion

`ProcessSingleFleetRMAAccount` is a **medium-complexity, high-criticality** dispatch unit.
- CYC 12 is above the Jane Street threshold of 8 by **+4 points**.
- LOC 106 exceeds the 80-line extraction threshold by **+26 lines**.
- The method carries **5 concurrent state mutations** with strict ordering invariants that make
  naïve refactoring high-risk; any decomposition must preserve the [923B-FIX-B] ordering contract.
- Priority: **HIGH** (CodeScene RED + jCodemunch Rank 20 consensus).
- Recommended action: **Extract helpers** (PositionInfo builder, state registrar, rollback helper,
  guard predicate) in Phase 1, keeping the ordering invariants explicit in extracted method names.
