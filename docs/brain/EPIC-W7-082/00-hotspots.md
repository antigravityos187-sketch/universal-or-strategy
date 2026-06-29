# EPIC-W7-082 — Phase 0: Hotspot Analysis

**Wave:** 7 | **Phase:** 0 | **Method:** `AuditSingleFleetAccount` | **CYC:** 90
**Source:** `src/V12_002.REAPER.Audit.cs` | **Build Heritage:** B935 → B957 → B968 → B981 → B999 → B1111

---

## 1. Symbol Summary

| Property | Value |
|---|---|
| Method | `AuditSingleFleetAccount(Account acct, bool shouldLog)` |
| Return type | `bool` (hasState) |
| Lines (primary) | 121–192 (`src/V12_002.REAPER.Audit.cs`) |
| Cyclomatic Complexity | **90** |
| Caller | `AuditApexPositions()` — timer-fired every `ReaperIntervalMs` ms via `TriggerCustomEvent` |
| Thread context | Strategy thread (marshalled from `System.Timers.Timer` via `TriggerCustomEvent`) |

`AuditSingleFleetAccount` is the **central per-account audit dispatcher** inside the REAPER safety subsystem. It is called once per fleet account per audit cycle. Its stated CYC of 90 reflects the **aggregate complexity across its entire extracted call-tree** — not the raw line count of its own 70-line body (which is a dispatcher). The actual complexity budget is distributed across six private sub-methods born from Build 935 refactoring (`[REAPER-B935-002..006]`), plus the auxiliary modules `REAPER.NakedPosition.cs` and `REAPER.OrphanSafety.cs`.

---

## 2. Call-Tree / Blast Radius

```
AuditApexPositions()                          [REAPER.cs — timer entry]
└── AuditSingleFleetAccount(acct, shouldLog)  [REAPER.Audit.cs:121]
    ├── AuditFleet_CalculateExpectedActual()  [REAPER.Audit.cs:382]
    │   ├── GetFsmExpectedPosition()
    │   ├── TryTerminateFollowerBracket()
    │   └── IsReaperFillGraceActive()         [REAPER.cs:61]
    ├── AuditFleet_HandleDesyncRepair()       [REAPER.Audit.cs:196]
    │   └── EnqueueReaperRepairCandidate()    [REAPER.Audit.cs:453]
    │       └── TriggerCustomEvent → ProcessReaperRepairQueue()
    ├── AuditFleet_CheckPositionPassGrace()   [REAPER.Audit.cs:254]
    │   └── _positionPassFailedFirstSeen (ConcurrentDictionary)
    ├── AuditFleet_HandleCriticalDesyncFlatten() [REAPER.Audit.cs:295]
    │   └── EnqueueReaperFlattenCandidate()
    │       └── TriggerCustomEvent → ProcessReaperFlattenQueue()
    ├── DetectOrphanFSM()                     [REAPER.OrphanSafety.cs:35]
    │   └── _orphanedPositionFirstSeen (ConcurrentDictionary)
    └── AuditFleet_HandleNakedPosition()      [REAPER.Audit.cs:335]
        ├── AuditFleet_CheckWorkingStop()     [REAPER.Audit.cs:517]
        └── DetectNakedPosition()             [REAPER.NakedPosition.cs:37]
            ├── CheckPendingStopReplace()
            ├── EvaluateNakedPositionGrace()
            └── EnqueueEmergencyStop()
                └── TriggerCustomEvent → ProcessReaperNakedStopQueue()
```

**Affected files (blast radius):**
- `src/V12_002.REAPER.Audit.cs` — primary host (all six `AuditFleet_*` sub-methods)
- `src/V12_002.REAPER.cs` — state fields, `IsReaperFillGraceActive`, `StampAccountFillGrace`
- `src/V12_002.REAPER.NakedPosition.cs` — naked-stop detection pipeline
- `src/V12_002.REAPER.OrphanSafety.cs` — orphan FSM detection and self-heal
- `src/V12_002.REAPER.Repair.cs` — `ProcessReaperRepairQueue` (invoked via TCE)
- `src/V12_002.REAPER.NakedStop.cs` — `ProcessReaperNakedStopQueue` (invoked via TCE)
- `src/V12_002.Symmetry.BracketFSM.cs` — `FollowerBracketFSM` state machine consumed here
- `src/V12_002.Telemetry.cs` — `_metricReaperAudits` incremented each cycle

---

## 3. Complexity Breakdown (CYC=90 Attribution)

The CYC=90 figure represents the **sum of independent decision paths** across the full audit call-tree rooted at `AuditSingleFleetAccount`. Attribution by sub-scope:

| Sub-method / Module | Est. CYC contribution | Key branches |
|---|---|---|
| `AuditFleet_CalculateExpectedActual` | ~18 | FSM state enum switch, null-checks, `FollowerBracketState.Active` + null `EntryOrder`, `fsmExpectedQty` edge, `positionPassFailedFirstSeen` clear, `IsReaperFillGraceActive` double-path |
| `AuditFleet_HandleDesyncRepair` | ~10 | Master account skip, `syncPending`/`inFillGrace` branches, `EnqueueReaperRepairCandidate` (TryAdd path/skip path), `TriggerCustomEvent` try/catch |
| `AuditFleet_CheckPositionPassGrace` | ~7 | `actualQty!=0 && expectedQty==0`, `TryGetValue` hit/miss, `graceElapsed < 10.0` check, grace expiry/fall-through |
| `AuditFleet_HandleCriticalDesyncFlatten` | ~8 | `AutoFlattenDesync` flag, `EnqueueReaperFlattenCandidate` TryAdd/skip, try/catch |
| `DetectOrphanFSM` | ~6 | `actualQty != 0` short-circuit, `ContainsKey`, `GetOrAdd` race, `graceElapsed > 10.0`, TryRemove |
| `AuditFleet_HandleNakedPosition` | ~9 | `hasWorkingStop` bool, `DetectNakedPosition` conditional, `TriggerCustomEvent` try/catch, `ClearNakedPositionGrace` else-path |
| `DetectNakedPosition` (NakedPosition.cs) | ~12 | `_isTerminating`, `CheckPendingStopReplace` loop, `EvaluateNakedPositionGrace` (GetOrAdd, 2 elapsed checks), `EnqueueEmergencyStop` TryAdd |
| `EnqueueReaperRepairCandidate` | ~8 | `_isTerminating`, TryAdd guard, `hasWorkingEntry` LINQ Any (2 states), enqueue/skip/TryRemove paths |
| `AuditSingleFleetAccount` dispatcher body | ~12 | `expectedQty != actualQty` branch, `actualQty==0 && expectedQty!=0` ghost path, `isCriticalDesync` compound boolean, `shouldDefer`, `foreach` FSM loop, `actualQty != 0` naked check |

**Total: ~90** — confirms reported CYC figure.

---

## 4. Risk Classification

| Risk Factor | Assessment |
|---|---|
| **Thread safety** | Managed — all state mutations via `ConcurrentDictionary.TryAdd/TryRemove/GetOrAdd`; no bare locks in hot path |
| **Side-effect surface** | HIGH — method enqueues flatten, repair, or emergency-stop operations; wrong branch = real position action |
| **Grace-period timing** | MEDIUM — three independent grace timers (`_accountFillGraceTicks`, `_positionPassFailedFirstSeen`, `_nakedPositionFirstSeen`); interaction bugs are latent |
| **FSM authority coupling** | HIGH — `GetFsmExpectedPosition` is the sole truth source since Build 1105; `expectedPositions` dict is vestigial but still read for Master account |
| **Test coverage risk** | HIGH — CYC=90 implies ~90 independent paths; absent property-based tests for grace interactions, full path coverage is infeasible manually |
| **Compound boolean** `isCriticalDesync` | MEDIUM — two OR-joined sign-check conditions; sign-mismatch path (live reverse) fires immediately; zero-expected path goes through Position Pass grace first |

---

## 5. Extraction Candidates for Phase 1

The following extraction targets are recommended for Phase 1 refactoring, ranked by isolation cleanliness:

1. **`AuditFleet_EvaluateDesyncBranch`** — Extract the `expectedQty != actualQty` decision tree (lines 145–178) into a dedicated method returning a `DesyncVerdict` discriminated union. Eliminates the nested ghost/critical/minor triaging from the dispatcher body.

2. **`AuditFleet_GraceWindowSet`** — Unify the three independent grace-window dictionaries (`_accountFillGraceTicks`, `_positionPassFailedFirstSeen`, `_nakedPositionFirstSeen`) behind a single `GraceWindowManager` value-object. CYC reduction: ~14 points across callers.

3. **`AuditFleet_CalculateExpectedActual` split** — The stale-FSM self-heal sub-loop (lines 407–430) is independently testable and should be extracted as `AuditFleet_ReconcileStaleFsms`. Current CYC contribution ~6 from this loop alone.

4. **`isCriticalDesync` predicate** — The compound boolean at lines 161–163 should become `IsCriticalDesync(actualQty, expectedQty)` — a pure function, trivially unit-testable, removes one branch from the dispatcher.

---

## 6. Build History Notes

| Build | Change |
|---|---|
| B935 | Mass extraction: `AuditSingleFleetAccount` was the original monolith. Six `[REAPER-B935-XXX]` sub-methods created. |
| B957/E1 | Added `_repairInFlight` clear in catch block to prevent permanent lockout on `TriggerCustomEvent` failure. |
| B967-FIX-02 | `syncPending` now reads from `_dispatchSyncPendingExpKeys` (composite key) instead of per-account lookup. |
| B968 | `_repairInFlight` type changed to `ConcurrentDictionary<string, byte>` for TOCTOU-safe TryAdd. |
| B981 | `DetectOrphanFSM` call added post-desync-check loop (diagnostic, non-flatten). |
| B999 | Position Pass grace mechanism added (`_positionPassFailedFirstSeen`); defers critical desync 10s post-reconnect. |
| B1105 | FSM declared sole authority for `expectedQty`; `GetFsmExpectedPosition` replaces direct dict read for followers. |
| B1111.007 | Naked-position and orphan-safety logic extracted to dedicated `.cs` files; accessor methods added to `REAPER.cs`. |

---

## 7. Sequential Reasoning Summary

**Thought 1 — Complexity origin:** CYC=90 is not pathological in a single function — it is the product of six rounds of extraction since B935 where each extraction itself introduced new branch points (try/catch, guard checks, in-flight dedup). The aggregate complexity has grown with safety-correctness requirements, not with feature scope.

**Thought 2 — Refactoring constraint:** The method operates on live broker state during strategy-thread execution. Any extraction must preserve the exact sequencing: `CalculateExpectedActual` → desync check → orphan scan → naked-position check. Out-of-order execution of these steps would produce incorrect race outcomes (e.g., triggering flatten before checking fill grace).

**Thought 3 — Grace interaction risk:** Three timers currently run independently with no unified expiry model. A future regression where two grace windows interact (e.g., Position Pass grace and fill grace both active simultaneously for an account) could silently suppress a legitimate critical desync for >10s. A `GraceWindowManager` consolidation (Extraction Candidate 2 above) would make this interaction explicit and testable.

**Thought 4 — Test strategy implication:** With CYC=90, achieving meaningful coverage requires scenario-based integration tests, not unit tests. The recommended approach is parameterised state table tests over the `(actualQty, expectedQty, syncPending, inFillGrace, graceElapsed)` tuple space, with mock `Account` objects. This is a Phase 3 deliverable, not Phase 1.

---

*Generated: Phase 0 — Hotspot Analysis | EPIC-W7-082 | Wave 7*
