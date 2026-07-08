# EPIC-W7-001 — Phase 1: Scope Definition

## Single Method in Scope

**Method:** `ShouldSkipFleet_RunHealthCheck`
**File:** `src/V12_002.SIMA.Fleet.cs` (line 478)
**Class:** `V12_002 : Strategy` (partial class, namespace `NinjaTrader.NinjaScript.Strategies`)
**Signature:** `private void ShouldSkipFleet_RunHealthCheck(Account acct, StringBuilder dispatchLog)`

---

## Complexity Targets

| Metric | Current | Target |
|---|---|---|
| **CYC (pre-T-W1 monolith)** | 31 | — |
| **CYC (post-T-W1 dispatcher)** | ≤ 5 | ≤ 8 |
| **Helper: `IsBrokerPositionFlat`** | ~4 | ≤ 8 |
| **Helper: `HasActiveFsmForAccount`** | ~7 | ≤ 8 |
| **Helper: `HasActivePositionForAccount`** | ~3 | ≤ 8 |
| **Helper: `LogHealthCheckResult`** | ~5 | ≤ 8 |

The original CYC=31 monolith was decomposed by the T-W1 refactor into a thin dispatcher (≤5 CYC)
plus four extracted helpers. Phase 1 scope validates each helper independently and monitors the
`LogHealthCheckResult` ternary-in-format-string pattern for FSM growth risk.

---

## Callers Analysis

**Direct callers: 1**

| Caller | File | Line | Call Type |
|---|---|---|---|
| `ShouldSkipFleetAccount` | `src/V12_002.SIMA.Fleet.cs` | 465 | void (diagnostic-only) |

**Caller chain (full depth):**

```
ExecuteSmartDispatchEntry  (src/V12_002.SIMA.Dispatch.cs, line 222)
  └── ShouldSkipFleetAccount  (src/V12_002.SIMA.Fleet.cs, line 465)
        └── ShouldSkipFleet_RunHealthCheck  ← SUBJECT METHOD
```

`ExecuteSmartDispatchEntry` itself is called from 8 distinct call sites across 6 source files
(`V12_002.Entries.RMA.cs`, `V12_002.Entries.FFMA.cs`, `V12_002.Entries.Trend.cs`,
`V12_002.Entries.OR.cs`, `V12_002.Entries.MOMO.cs`, `V12_002.Entries.Retest.cs`), but these
are all upstream of `ShouldSkipFleetAccount` and therefore outside the scope boundary for this
epic. The single direct caller of the subject method is `ShouldSkipFleetAccount`.

---

## Scope Boundary Statement

**Only `ShouldSkipFleet_RunHealthCheck` and its new extracted helper methods are in scope.**

This is the scope boundary for EPIC-W7-001 Phase 1 through Phase 3. The refactor work is
confined to the subject method and any helper methods it calls that were extracted as part of
the T-W1 decomposition (`IsBrokerPositionFlat`, `HasActiveFsmForAccount`,
`HasActivePositionForAccount`, `LogHealthCheckResult`). No other methods are to be modified,
restructured, or reorganised under this epic.

---

## Why Other Methods Are NOT in Scope (V12.23 No Scope Creep Protocol)

The following methods appear within `src/V12_002.SIMA.Fleet.cs` or in the caller chain but are
explicitly excluded under the **V12.23 No Scope Creep Protocol**:

| Method | Reason Excluded |
|---|---|
| `ShouldSkipFleetAccount` | Caller only — it delegates to the subject method but is not itself a complexity hotspot for this wave (it is a thin dispatcher, CYC ≤ 10 post-T-W1). Modifying it would risk breaking the inactive-check and consistency-lock paths that are not under review. |
| `ShouldSkipFleet_IsConsistencyLockHit` | Sibling in the dispatcher chain; its own CYC (~2) is well within tolerance. Touching it is unnecessary and introduces regression risk to the `EnableConsistencyLock` / `MaxDailyProfitCap` decision path. |
| `ExecuteSmartDispatchEntry` | Top-level upstream caller residing in a different source file (`src/V12_002.SIMA.Dispatch.cs`). Contains unrelated fleet orchestration logic (circuit breaker, delta reservation, queue management). Changes here are outside this wave's mandate. |
| `ProcessFleetSlot`, `PumpFleetDispatch`, `DrainAllDispatchQueuesOnAbort` | Fleet execution path — completely orthogonal to the health-check diagnostic logic. Any modification here carries high risk to order submission and rollback correctness. |
| `InitializeFollowerBracketFSM`, `VerifyPhotonSlotIntegrity` | Identified as related complexity hotspots (est. CYC ~9 and ~14 respectively) in the Phase 0 hotspot analysis, but they are candidates for *future* waves, not Wave 7. |
| `UnsubscribeFromFleetAccounts` | Lifecycle/cleanup method with no coupling to health-check logic. |

**Rule citation:** V12.23 No Scope Creep Protocol — a wave targets a single declared hotspot
method. Helpers extracted *by* the subject method's refactor are in scope; all other methods
sharing the same file or caller chain are not.

---

## Extracted Helpers — Phase 1 Validation Checklist

These four helpers were produced by the T-W1 refactor and are in scope for Phase 1 CYC
validation only (no functional changes unless a specific defect is identified):

1. **`IsBrokerPositionFlat`** (line 516) — instrument-scoped position scan; ToArray snapshot
   guard for broker-thread safety. Watch: indexed `for` loop with 3-level null guard.

2. **`HasActiveFsmForAccount`** (line 539) — 4-way FSM state OR fan-out over
   `_followerBrackets`. Lock-free enumeration (strategy-thread-only constraint). This is the
   highest individual CYC contributor (~7) and must be monitored if `FollowerBracketState`
   gains additional values.

3. **`HasActivePositionForAccount`** (line 565) — `activePositions` scan; lowest CYC (~3).
   No action required unless `PositionRecord.IsFollower` semantics change.

4. **`LogHealthCheckResult`** (line 581) — dual-branch diagnostic formatter. Risk flag: the
   `hasActiveFsm ? "FSM active" : (hasDispatchPending ? "dispatch pending" : "activePos present")`
   ternary chain inside `string.Format` will silently produce incorrect log messages if a fourth
   state is introduced. Phase 1 should add a guard or enumeration here.

---

## Threading and State Constraints

- **Threading:** Strategy thread only (comment at line 443). No cross-thread mutation risk.
- **Shared state read:** `_followerBrackets` (ConcurrentDictionary, lock-free enumeration),
  `activePositions` (ConcurrentDictionary, lock-free enumeration),
  `_dispatchSyncPendingExpKeys` (read via `ContainsKey`).
- **External dependency:** `Account.Positions` (broker thread) — snapshot guard via `ToArray()`
  applied in `IsBrokerPositionFlat`; PR6-P0 null safety hardening already in place.
- **Side effects:** Diagnostic-only (void). Appends to `StringBuilder dispatchLog`. No position
  mutations, no order submissions, no state writes.

---

## Agent Tracking

- **Agent Name:** v12-phase1-scope
- **Bobcoins Used:** 3.0
- **Execution Time:** ~90s
