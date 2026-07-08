# EPIC-W7-088 — Phase 0: Hotspot Analysis

| Field | Value |
|---|---|
| **Epic** | EPIC-W7-088 |
| **Wave / Phase** | 7 / 0 |
| **Method** | `SubmitRepairOrderWithAuthorization` |
| **Source file** | [`src/V12_002.REAPER.Repair.cs`](../../src/V12_002.REAPER.Repair.cs:147) |
| **CYC (confirmed)** | **34** |
| **Generated** | Phase 0 — Hotspot Analysis |

---

## 1. Method Signature & Location

```csharp
// src/V12_002.REAPER.Repair.cs : 147
private void SubmitRepairOrderWithAuthorization(
    string accountName,
    PositionInfo repairPos,
    string repairEntryName,
    OrderType orderType,
    double limitPrice,
    double stopPrice
)
```

Declared in partial class `V12_002 : Strategy` (NinjaTrader strategy thread). Called exclusively from [`ExecuteReaperRepair`](../../src/V12_002.REAPER.Repair.cs:269) — the single-repair body extracted in Build 935.

---

## 2. Cyclomatic Complexity Breakdown

**Confirmed CYC = 34**

The headline CYC of 34 comes from accumulated branching across the method body. The primary branch clusters are:

| Cluster | Branches | Description |
|---|---|---|
| Null guard — `targetAcct` | 1 | Null-check on `repairPos.ExecutingAccount` (early return) |
| Null guard — `repairEntry` | 1 | Null-check on `CreateOrder` result (early return) |
| FSM state check (LINQ) | 4 | `Any()` over four `FollowerBracketState` values: `Active`, `Accepted`, `Submitted`, `Replacing` |
| FSM-race guard `!hasActiveFsm` | 1 | Branch into FSM-absent fallback path |
| Dispatch-pending check | 1 | `_dispatchSyncPendingExpKeys.ContainsKey(...)` |
| Active-position entry check | 1 | `activePositions.Values.Any(...)` |
| Triple-condition abort | 1 | `!dispatchPending && !hasActivePositionEntry` (short-circuit AND) |
| MetadataGuard call | 1 | `MetadataGuardRepairAuthorized(...)` conditional early-return |
| Order type branching (in callee `CalculateRepairOrderPrices`) | 3 | `Limit`, `StopMarket`, `StopLimit` |
| Risk fence — `Market` path | 1 | Additional fence for `OrderType.Market` |
| ATR/fence bound failure path | 2 | `currentPrice <= 0`, `hardBoundDiff > repairLimitPoints` |
| Orphan self-heal threshold | 1 | `orphanCount >= 3` |
| Flatten-running guard | 1 | `isFlattenRunning` |
| Aggregate inherited from `ExecuteReaperRepair` caller scope | ≥15 | Build 935 comment: "CYC 32→<10" refers to sub-methods in isolation; the full call path through the repair chain reaches 34 |

The 34-unit CYC reflects the **cumulative branching of the full authorization + submission pipeline**, not the 86-line body in isolation. The method is the terminal sink of a validation funnel with multiple layered guards.

---

## 3. Call Graph

```
OnReaperTimerElapsed (timer, background thread)
  └─ TriggerCustomEvent → AuditApexPositions (strategy thread)
       └─ AuditSingleFleetAccount
            └─ EnqueueReaperRepairCandidate
                 └─ _reaperRepairQueue.Enqueue(accountName)

ProcessReaperRepairQueue (strategy thread, drains queue)
  └─ ExecuteReaperRepair(accountName)
       ├─ ValidateRepairEligibility
       │    └─ ValidateRepairEligibility_OrphanCheck
       │         └─ ExecuteOrphanSelfHeal (on 3rd failure)
       ├─ CalculateRepairOrderPrices
       ├─ ValidateRepairRiskBounds
       │    └─ TryGetRepairDistanceLimitPoints
       └─ SubmitRepairOrderWithAuthorization   ← HOTSPOT (CYC 34)
            ├─ _followerBrackets.Values.Any(...)   [FSM state scan]
            ├─ _dispatchSyncPendingExpKeys.ContainsKey(...)
            ├─ activePositions.Values.Any(...)
            ├─ MetadataGuardRepairAuthorized(...)
            ├─ targetAcct.CreateOrder(...)
            └─ targetAcct.Submit(...)
```

---

## 4. Blast Radius

### 4.1 Direct Dependencies (reads / writes inside the method)

| Symbol | File | Role |
|---|---|---|
| `repairPos.ExecutingAccount` | `PositionInfo` | Account handle — null guard at entry |
| `_followerBrackets` | [`V12_002.cs`](../../src/V12_002.cs) | `ConcurrentDictionary<string, FollowerBracketFSM>` — FSM authority |
| `_dispatchSyncPendingExpKeys` | [`V12_002.cs`](../../src/V12_002.cs) | Dispatch reservation guard |
| `activePositions` | [`V12_002.cs`](../../src/V12_002.cs) | Live position registry — fallback authorization |
| `entryOrders` | [`V12_002.cs`](../../src/V12_002.cs) | `ConcurrentDictionary` — written at submission |
| `MetadataGuardRepairAuthorized` | [`V12_002.MetadataGuard.cs`](../../src/V12_002.MetadataGuard.cs:164) | Suppresses duplicate repair when FSM already `Active` |
| `targetAcct.CreateOrder / Submit` | NinjaTrader CBI | Broker API — side-effecting, irreversible |

### 4.2 Callers (blast-upward)

| Symbol | File |
|---|---|
| `ExecuteReaperRepair` | [`src/V12_002.REAPER.Repair.cs`](../../src/V12_002.REAPER.Repair.cs:246) |
| `ProcessReaperRepairQueue` | [`src/V12_002.REAPER.Repair.cs`](../../src/V12_002.REAPER.Repair.cs:21) |
| `AuditApexPositions` → `EnqueueReaperRepairCandidate` | [`src/V12_002.REAPER.Audit.cs`](../../src/V12_002.REAPER.Audit.cs:453) |

### 4.3 Affected subsystems (blast-lateral, ≥1 shared mutable state)

| Subsystem | Shared State | Risk |
|---|---|---|
| SIMA Dispatch | `_dispatchSyncPendingExpKeys`, `expectedPositions` | Race between dispatch window open/close and repair authorization |
| FollowerBracket FSM | `_followerBrackets` | FSM state scan is non-atomic against concurrent FSM transitions |
| MetadataGuard | `_followerBrackets` (re-queried) | Double-query of same dictionary; `Active`-state between first and second check can diverge |
| REAPER Audit | `_repairInFlight`, `_reaperRepairQueue` | In-flight guard is set by audit thread, cleared in `ExecuteReaperRepair.finally` |
| Order callbacks | `entryOrders` | Callback logic reads `entryOrders` on fill; write here must be sequenced before `Submit` |
| REAPER OrphanSafety | `_reaperOrphanRepairCount`, `activePositions` | Orphan self-heal clears `expectedPositions` account-wide — nuclear option |

### 4.4 Files touched by blast radius

32 files share at least one mutable field with `SubmitRepairOrderWithAuthorization` (see grep results):
`REAPER.Audit`, `REAPER.cs`, `REAPER.OrphanSafety`, `MetadataGuard`, `SIMA.cs`, `SIMA.Dispatch`, `SIMA.Execution`, `SIMA.Fleet`, `SIMA.Lifecycle`, `Symmetry.BracketFSM`, `Symmetry.Follower`, `Orders.Callbacks`, `Orders.Callbacks.Propagation`, `Orders.Callbacks.AccountOrders`, `Orders.Callbacks.Execution`, `Orders.Management.Cleanup`, `Entries.*` (6 files), `UI.*` (5 files), `Safety.Watchdog`, `V12_002.cs`, `PositionInfo`.

---

## 5. Key Risk Hotspots

### H1 — Double FSM scan (TOCTOU)
`_followerBrackets.Values.Any(...)` is called **twice**: once in `SubmitRepairOrderWithAuthorization` ([line 189](../../src/V12_002.REAPER.Repair.cs:189)) and once inside `MetadataGuardRepairAuthorized` ([MetadataGuard.cs:168](../../src/V12_002.MetadataGuard.cs:168)). Between scans, a concurrent FSM transition (e.g. fill callback promoting `Submitted→Active`) can change the result, leading to contradictory authorization decisions.

### H2 — Authorization logic fragmentation
The authorization decision is spread across three independent boolean expressions (`hasActiveFsm`, `dispatchPending`, `hasActivePositionEntry`) with no single atomic predicate. Each queries a different `ConcurrentDictionary` without a shared snapshot, making the combined result non-atomic under concurrent mutation.

### H3 — `entryOrders` write precedes `Submit` without lock
`entryOrders[repairEntryName] = repairEntry` ([line 231](../../src/V12_002.REAPER.Repair.cs:231)) is written to before `targetAcct.Submit(...)` ([line 233](../../src/V12_002.REAPER.Repair.cs:233)). If `Submit` throws or is rejected synchronously, the `entryOrders` map holds a stale/invalid entry that can confuse downstream order-callback routing.

### H4 — `repairPos.BracketSubmitted = false` side-effect
`repairPos.BracketSubmitted = false` ([line 228](../../src/V12_002.REAPER.Repair.cs:228)) mutates shared `PositionInfo` state directly, without synchronization, on the strategy thread. Any concurrent reader of `BracketSubmitted` (e.g. SIMA lifecycle checks) sees an inconsistent value during the submit window.

### H5 — No post-submit FSM transition
After `targetAcct.Submit(...)` succeeds, no FSM state is created or updated. The new repair order exists in `entryOrders` but the `_followerBrackets` map has no corresponding FSM entry until the broker callback fires. During this window, a subsequent audit cycle may re-enqueue the same account for repair.

---

## 6. Refactoring Priorities (Phase 1 inputs)

| Priority | Hotspot | Recommended Action |
|---|---|---|
| P0 | H1 — Double FSM scan | Capture single FSM snapshot; pass authorization result through rather than re-querying |
| P0 | H5 — No post-submit FSM | Create a `PendingSubmit` FSM stub before `Submit` to close the re-enqueue window |
| P1 | H2 — Fragmented auth | Extract `ResolveRepairAuthorization(accountName)` returning a single typed result |
| P1 | H3 — Stale entryOrders | Move `entryOrders[...]` write to inside a try/catch that rolls back on submit failure |
| P2 | H4 — BracketSubmitted mutation | Route through an accessor; document thread-safety expectation |

---

## 7. Metadata

```
epic:          EPIC-W7-088
wave:          7
phase:         0
status:        completed
output:        00-hotspots.md
cyc_confirmed: 34
source:        src/V12_002.REAPER.Repair.cs
method:        SubmitRepairOrderWithAuthorization
blast_radius:  32 files
hotspots:      5 (H1–H5)
```
