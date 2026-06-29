# EPIC-W7-126 — Phase 0: Hotspot Analysis

## Target Method

| Property | Value |
|---|---|
| **Method** | `SymmetryGuardSubmitFollowerBracket` |
| **File** | `src/V12_002.Symmetry.Follower.cs` |
| **Lines** | 285–425 |
| **Class** | `V12_002 : Strategy` (partial) |
| **CYC Confirmed** | **16** |

---

## Cyclomatic Complexity Breakdown

CYC = 16 is derived from the structural decision graph of the method (McCabe: number of linearly-independent paths = edges − nodes + 2, or equivalently 1 + predicate count with compound-condition expansion):

| # | Decision Point | Location | +CYC |
|---|---|---|---|
| Base | Method entry | — | +1 |
| 1 | `if (pos.BracketSubmitted)` early-return guard | ln 287 | +1 |
| 2 | `if (acct == null)` early-return guard | ln 290 | +1 |
| 3 | Ternary: `pos.Direction == MarketPosition.Long ? Sell : BuyToCover` | ln 293 | +1 |
| 4 | Ternary: `!string.IsNullOrEmpty(pos.OcoGroupId) ? … : "SG_"+ticks` | ln 298 | +1 |
| 5 | `for (int targetNum = 1; targetNum <= 5; …)` target-build loop | ln 324 | +1 |
| 6 | `if (targetQty <= 0) continue` — skip zero-qty target | ln 327 | +1 |
| 7 | `if (IsRunnerTarget(targetNum)) … continue` — runner bypass | ln 330 | +1 |
| 8 | `if (targetPrice <= 0) … continue` — invalid-price skip | ln 337 | +1 |
| 9 | `for (int i = 0; i < 5; i++)` — FSM price-slot init loop | ln 386 | +1 |
| 10 | `foreach (var (tNum, tOrder) in stagedTargets)` — FSM population loop | ln 388 | +1 |
| 11 | `if (tNum >= 1 && tNum <= 5)` — compound `&&` (two predicates) | ln 390 | +2 |
| 12 | `foreach (var (targetNum, order) in stagedTargets)` — dict-write loop | ln 408 | +1 |
| **Total** | | | **16** |

---

## Blast Radius Summary

`SymmetryGuardSubmitFollowerBracket` is the **terminal commit point** for every follower bracket in the symmetry system. It writes to multiple shared state surfaces and has two direct callers within the same file, both on hot execution paths.

### Direct Callers (same file)

| Caller | Call Site | Path |
|---|---|---|
| `SymmetryGuardOnFollowerFill` | ln 62 | ANCHOR-01 fast-path: master already resolved at fill time |
| `SymmetryGuardTryResolveFollower` | ln 230 | Normal delayed-resolve path: anchor resolves after fill |

### Shared State Surfaces Written

| State Surface | Write Operation | Downstream Impact |
|---|---|---|
| `_followerBrackets[fleetEntryName]` | Indexer assignment (FSM init + State flip) | `V12_002.Symmetry.BracketFSM.cs`, `SIMA.Fleet.cs`, `SIMA.Dispatch.cs`, `SIMA.Shadow.cs`, `Orders.Callbacks.Propagation.cs`, `SIMA.Execution.cs` |
| `stopOrders[fleetEntryName]` | Enqueue write via actor pipeline (B966) | `Orders.Management.cs`, `Orders.Management.StopSync.cs`, `Orders.Callbacks.Execution.cs`, `REAPER.Repair.cs` |
| `targetNOrders[fleetEntryName]` (T1–T5 dicts) | `GetTargetOrdersDictionary(n)[key] = order` | `Orders.Management.Cleanup.cs`, `Orders.Callbacks.Propagation.cs`, `Symmetry.Replace.cs`, `Trailing.StopUpdate.cs` |
| `pos.BracketSubmitted` | Set `true` | Guards re-entry in `SymmetryGuardOnFollowerFill`, `SymmetryGuardTryResolveFollower`, and `REAPER.Audit.cs` |
| `acct.Submit(ordersToSubmit)` | Broker submission | Live OCO stop+limit orders placed; errors surface in `Orders.Callbacks.cs` / `Orders.Callbacks.AccountOrders.cs` |

### Downstream File Count

**28 files** reference at least one state surface or helper called by this method:
`V12_002.Symmetry.BracketFSM.cs`, `V12_002.SIMA.Fleet.cs`, `V12_002.SIMA.Dispatch.cs`,
`V12_002.SIMA.Shadow.cs`, `V12_002.SIMA.Execution.cs`, `V12_002.Orders.Management.cs`,
`V12_002.Orders.Management.StopSync.cs`, `V12_002.Orders.Management.Cleanup.cs`,
`V12_002.Orders.Management.Flatten.cs`, `V12_002.Orders.Callbacks.cs`,
`V12_002.Orders.Callbacks.Execution.cs`, `V12_002.Orders.Callbacks.AccountOrders.cs`,
`V12_002.Orders.Callbacks.Propagation.cs`, `V12_002.REAPER.cs`, `V12_002.REAPER.Audit.cs`,
`V12_002.REAPER.Repair.cs`, `V12_002.REAPER.NakedPosition.cs`, `V12_002.Symmetry.Replace.cs`,
`V12_002.Trailing.cs`, `V12_002.Trailing.StopUpdate.cs`, `V12_002.UI.SnapshotPool.cs`,
`V12_002.UI.Snapshot.cs`, `V12_002.UI.Compliance.cs`, `V12_002.UI.Callbacks.cs`,
`V12_002.MetadataGuard.cs`, `V12_002.Lifecycle.cs`, `V12_002.PositionInfo.cs`, `V12_002.cs`.

---

## Top 3 Complexity Drivers

### Driver 1 — Three-branch filter logic inside a `for` loop (+4 CYC in a single compound block)

The `for (targetNum = 1..5)` loop at **ln 324–372** contains three sequential `if / continue` gates before doing any work:

```csharp
for (int targetNum = 1; targetNum <= 5; targetNum++)       // +1
{
    int targetQty = GetTargetContracts(pos, targetNum);
    if (targetQty <= 0)  continue;                         // +1 (skip zero qty)
    if (IsRunnerTarget(targetNum)) { runnerQty += …; continue; }  // +1 (skip runner)
    double targetPrice = GetTargetPrice(pos, targetNum);
    if (targetPrice <= 0) { Print(…); continue; }          // +1 (skip bad price)
    // ... create limit order
}
```

This is the single heaviest block in the method — 4 decision points in 48 lines. The three filter conditions (qty, runner, price) represent distinct policy concerns stacked inside one loop body. Extracting the per-target work into `bool TryBuildTargetOrder(PositionInfo pos, int targetNum, string ocoId, OrderAction exitAction, out (int, Order) staged)` would collapse the loop body to a single call, reducing CYC by 3.

---

### Driver 2 — Parallel FSM-init and dict-write loops using staged list (+3 CYC across two constructs)

After the target-build loop, two additional loop constructs operate on `stagedTargets`:

```csharp
for (int i = 0; i < 5; i++)                               // +1
    fsm.ExpectedTargetPrices[i] = 0;

foreach (var (tNum, tOrder) in stagedTargets)             // +1
{
    if (tNum >= 1 && tNum <= 5)                           // +2 (compound &&)
    { fsm.Targets[tNum - 1] = …; fsm.ExpectedTargetPrices[tNum - 1] = …; }
}

foreach (var (targetNum, order) in stagedTargets)         // +1
    GetTargetOrdersDictionary(targetNum)[fleetEntryName] = order;
```

The bounds guard `tNum >= 1 && tNum <= 5` is a defensive compound predicate that is always true because only valid targets enter `stagedTargets` — it fires no real logic but adds 2 CYC. Extracting `CommitFsmTargets(fsm, stagedTargets)` and `CommitTargetOrderDictionaries(fleetEntryName, stagedTargets)` would encapsulate these 4 decision points, dropping CYC by 4.

---

### Driver 3 — OcoId ternary and Direction ternary as dual-path guards (+2 CYC, readability hazard)

Two consecutive ternary expressions at the top of the method body:

```csharp
OrderAction exitAction = pos.Direction == MarketPosition.Long   // +1
    ? OrderAction.Sell : OrderAction.BuyToCover;

string ocoId = !string.IsNullOrEmpty(pos.OcoGroupId)           // +1
    ? pos.OcoGroupId : ("SG_" + DateTime.UtcNow.Ticks.ToString());
```

These are small but add 2 CYC and create a subtle correctness coupling: the fallback `"SG_"+ticks` OcoGroupId is explicitly documented as a non-deterministic anti-pattern (Build 936 `[FIX-2]` comment). Extracting `ResolveOcoGroupId(PositionInfo pos)` would encapsulate the guard, document the fallback at the definition site, and remove 1 CYC from this method.

---

## Recommended Extraction Count

**3 extractions recommended:**

| # | Extracted Helper | Lines Absorbed | CYC Reduction |
|---|---|---|---|
| 1 | `TryBuildTargetOrder(PositionInfo pos, int targetNum, string ocoId, OrderAction action, out (int, Order) staged) → bool` | ln 326–371 (inner loop body) | −3 |
| 2 | `CommitFsmAndDictionaries(FollowerBracketFSM fsm, string key, List<(int,Order)> staged)` | ln 386–409 (two loops + compound guard) | −4 |
| 3 | `ResolveOcoGroupId(PositionInfo pos) → string` | ln 298–300 (ternary fallback) | −1 |

**Projected post-refactor CYC**: ≈ **8** (16 − 8 net reduction), at or below the Wave 7 project threshold of 10.

> Note: The two early-return guards (Driver entries #1, #2) and the direction ternary (#3) are cheap single-liners; they are **not** extraction targets — they are the correct guard-clause pattern and should remain in place.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase0-hotspot |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | ~75s |
| **Wave** | 7 |
| **Phase** | 0 — Hotspot Analysis |
| **Epic** | EPIC-W7-126 |
| **Source File** | `src/V12_002.Symmetry.Follower.cs` |
| **CYC Confirmed** | 16 |
