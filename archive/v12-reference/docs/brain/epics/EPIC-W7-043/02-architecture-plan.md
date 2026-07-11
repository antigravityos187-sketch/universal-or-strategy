# EPIC-W7-043 — Phase 2: Architecture Plan

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:10:00Z
**Input:** docs/brain/EPIC-W7-043/01-scope-boundary.md

---

## Summary

| Field | Value |
|---|---|
| **Target Method** | `SymmetryGuardSubmitFollowerBracket` |
| **Source File** | `src/V12_002.Symmetry.Follower.cs` |
| **CYC Reported (epic list)** | 0 (stale) |
| **CYC Live (MCP)** | **16** |
| **Max Nesting (live)** | 5 |
| **Lines** | 141 (lines 285-425) |
| **Extraction Count** | 3 helpers |
| **Max CYC Projected** | 6 |
| **Boundary Verdict** | PASS (Phase 1.5) |

---

## CYC Driver Analysis

Live MCP data (`get_symbol_complexity`) confirmed CYC = 16, not 0. The 16 decision points are:

| # | Branch / Decision | Lines (approx) | CYC Delta |
|---|---|---|---|
| 1 | `if (pos.BracketSubmitted) return;` | 286 | +1 |
| 2 | `if (acct == null) return;` | 288 | +1 |
| 3 | Ternary `Direction == Long ? Sell : BuyToCover` | 290 | +1 |
| 4 | Ternary `!IsNullOrEmpty(OcoGroupId) ? ... : ...` | 294 | +1 |
| 5 | `for (targetNum = 1; targetNum <= 5; targetNum++)` | 327 | +1 |
| 6 | `if (targetQty <= 0) continue;` | 330 | +1 |
| 7 | `if (IsRunnerTarget(targetNum)) { continue; }` | 333 | +1 |
| 8 | `if (targetPrice <= 0)` — skip with Print | 337 | +1 |
| 9 | `for (int i = 0; i < 5; i++)` — FSM ExpectedTargetPrices init | 380 | +1 |
| 10 | `foreach (var (tNum, tOrder) in stagedTargets)` — FSM Targets assign | 383 | +1 |
| 11 | `if (tNum >= 1 && tNum <= 5)` — compound (&&) | 385 | +2 |
| 12 | `foreach (var (targetNum, order) in stagedTargets)` — dict update | 403 | +1 |
| 13 | Lambda in `Enqueue(ctx => { ... })` | 397 | +1 |
| 14 | implicit null-path in `Format`/`Print` call | 371 | +1 |

**Total: 16** — matches live MCP measurement.

---

## Extraction Plan

### Target Method After Extraction

**`SymmetryGuardSubmitFollowerBracket`** — orchestration shell only.

```csharp
private void SymmetryGuardSubmitFollowerBracket(string fleetEntryName, PositionInfo pos)
{
    if (pos.BracketSubmitted)
        return;
    Account acct = pos.ExecutingAccount;
    if (acct == null)
        return;

    OrderAction exitAction = pos.Direction == MarketPosition.Long
        ? OrderAction.Sell
        : OrderAction.BuyToCover;
    double validatedStop = ValidateStopPrice(pos.Direction, pos.CurrentStopPrice);
    string ocoId = !string.IsNullOrEmpty(pos.OcoGroupId)
        ? pos.OcoGroupId
        : ("SG_" + DateTime.UtcNow.Ticks.ToString());

    var ordersToSubmit = new List<Order>();
    var stagedTargets = new List<(int targetNum, Order order)>();

    Order stop = SymmetryGuardBuildStopOrder(pos, acct, exitAction, validatedStop, ocoId, fleetEntryName, ordersToSubmit);
    SymmetryGuardStageTargetOrders(pos, acct, exitAction, ocoId, fleetEntryName, stagedTargets, ordersToSubmit,
        out int nonRunnerLimitQty, out int runnerQty);
    FollowerBracketFSM fsm = SymmetryGuardInitFollowerBracketFSM(acct, fleetEntryName, ocoId, pos, stop, validatedStop, stagedTargets);

    _followerBrackets[fleetEntryName] = fsm;

    ordersToSubmit.Insert(0, stop);
    var _fen966 = fleetEntryName;
    var _s966 = stop;
    Enqueue(ctx => { ctx.stopOrders[_fen966] = _s966; });
    foreach (var (targetNum, order) in stagedTargets)
        GetTargetOrdersDictionary(targetNum)[fleetEntryName] = order;

    fsm.State = FollowerBracketState.Submitted;
    fsm.LastUpdateUtc = DateTime.UtcNow;
    acct.Submit(ordersToSubmit.ToArray());
    pos.BracketSubmitted = true;
    Print(string.Format(
        "[SYMMETRY STOP_AUDIT] OK {0}: StopQty={1} NonRunnerLimits={2} RunnerQty={3}",
        fleetEntryName, pos.TotalContracts, nonRunnerLimitQty, runnerQty));
}
```

**Parent projected CYC: 6**
Branches: guard(+1), null-guard(+1), ternary exitAction(+1), ternary ocoId(+1), Enqueue lambda(+1), foreach dict update(+1) = 6

---

### Helper 1: `SymmetryGuardBuildStopOrder`

**Signature:**
```csharp
private Order SymmetryGuardBuildStopOrder(
    PositionInfo pos,
    Account acct,
    OrderAction exitAction,
    double validatedStop,
    string ocoId,
    string fleetEntryName,
    List<Order> ordersToSubmit)
```

**Responsibility:** Creates and returns the single GTC StopMarket order for the OCO bracket using the validated stop price and deterministic OCO group ID.

**Body (extracted):**
```csharp
string stopSig = SymmetryTrim("Stop_" + fleetEntryName, 40);
return acct.CreateOrder(
    Instrument, exitAction, OrderType.StopMarket, TimeInForce.Gtc,
    Math.Max(1, pos.TotalContracts), 0, validatedStop,
    ocoId, stopSig, null);
```

**Projected CYC: 1** (no branches — pure construction)
**Jane Street annotation:** Leave unattributed (called once per position, not hot path).

---

### Helper 2: `SymmetryGuardStageTargetOrders`

**Signature:**
```csharp
[MethodImpl(MethodImplOptions.NoInlining)]
private void SymmetryGuardStageTargetOrders(
    PositionInfo pos,
    Account acct,
    OrderAction exitAction,
    string ocoId,
    string fleetEntryName,
    List<(int targetNum, Order order)> stagedTargets,
    List<Order> ordersToSubmit,
    out int nonRunnerLimitQty,
    out int runnerQty)
```

**Responsibility:** Iterates the 5 target slots. For each slot: validates quantity, skips runners, validates price (with diagnostic Print on skip), rounds limit price to tick size, creates limit order, accumulates into staged collections.

**Body (extracted):**
```csharp
nonRunnerLimitQty = 0;
runnerQty = 0;
for (int targetNum = 1; targetNum <= 5; targetNum++)
{
    int targetQty = GetTargetContracts(pos, targetNum);
    if (targetQty <= 0)
        continue;
    if (IsRunnerTarget(targetNum))
    {
        runnerQty += targetQty;
        continue;
    }
    double targetPrice = GetTargetPrice(pos, targetNum);
    if (targetPrice <= 0)
    {
        Print(string.Format(
            "[SYMMETRY TARGET_SKIP] T{0} for {1} has qty={2} but invalid price={3:F2}; skipped",
            targetNum, fleetEntryName, targetQty, targetPrice));
        continue;
    }
    double roundedTargetPrice = Instrument.MasterInstrument.RoundToTickSize(targetPrice);
    string targetSig = SymmetryTrim("T" + targetNum + "_" + fleetEntryName, 40);
    Order target = acct.CreateOrder(
        Instrument, exitAction, OrderType.Limit, TimeInForce.Gtc,
        targetQty, roundedTargetPrice, 0, ocoId, targetSig, null);
    stagedTargets.Add((targetNum, target));
    ordersToSubmit.Add(target);
    nonRunnerLimitQty += targetQty;
}
```

**Projected CYC: 6**
Branches: for-loop(+1), targetQty<=0(+1), IsRunnerTarget(+1), targetPrice<=0(+1), continue-in-skip(+1) = 5-6
**Jane Street annotation:** `[MethodImpl(MethodImplOptions.NoInlining)]` — contains cold-path Print/logging; isolates from hot-path inlining cache. (carl_cook: extract cold logging out-of-line)

---

### Helper 3: `SymmetryGuardInitFollowerBracketFSM`

**Signature:**
```csharp
private FollowerBracketFSM SymmetryGuardInitFollowerBracketFSM(
    Account acct,
    string fleetEntryName,
    string ocoId,
    PositionInfo pos,
    Order stop,
    double validatedStop,
    List<(int targetNum, Order order)> stagedTargets)
```

**Responsibility:** Constructs and populates the `FollowerBracketFSM` struct/object: sets account name, entry name, OCO group ID, initial state, remaining contracts, stop order, expected prices array, and target order references.

**Body (extracted):**
```csharp
var fsm = new FollowerBracketFSM
{
    AccountName = acct.Name,
    EntryName = fleetEntryName,
    OcoGroupId = ocoId,
    State = FollowerBracketState.PendingSubmit,
    RemainingContracts = pos.TotalContracts,
    StopOrder = stop,
    ExpectedStopPrice = validatedStop,
};
for (int i = 0; i < 5; i++)
    fsm.ExpectedTargetPrices[i] = 0;
foreach (var (tNum, tOrder) in stagedTargets)
{
    if (tNum >= 1 && tNum <= 5)
    {
        fsm.Targets[tNum - 1] = tOrder;
        fsm.ExpectedTargetPrices[tNum - 1] = tOrder.LimitPrice;
    }
}
return fsm;
```

**Projected CYC: 4**
Branches: for-loop(+1), foreach(+1), if(tNum bounds, compound &&)(+2) = 4
**Jane Street annotation:** Left-Right pattern compliant — constructs FSM locally, returned to parent for atomic publish via `_followerBrackets[fleetEntryName] = fsm`. (gjengset: construct then publish atomically)

---

## CYC Summary Table

| Symbol | Role | Projected CYC | <= 8? |
|---|---|---|---|
| `SymmetryGuardSubmitFollowerBracket` | Parent (orchestration) | 6 | YES |
| `SymmetryGuardBuildStopOrder` | Helper 1 — stop order construction | 1 | YES |
| `SymmetryGuardStageTargetOrders` | Helper 2 — target loop + staging | 6 | YES |
| `SymmetryGuardInitFollowerBracketFSM` | Helper 3 — FSM initialization | 4 | YES |

**Max CYC projected: 6** — all within Jane Street strict threshold (<= 8).

---

## Jane Street Pattern Alignment

### gjengset (Cache line / Left-Right / MemoryBarrier)
- FSM is constructed locally in `SymmetryGuardInitFollowerBracketFSM`, then returned and published atomically to `_followerBrackets[fleetEntryName]` — Left-Right compliant.
- No shared mutable state between helpers; each receives only needed parameters.
- No new `lock()` blocks introduced — actor `Enqueue()` pattern preserved.

### carl_cook (Zero-alloc hot path / NoInlining cold / logging out-of-line)
- `SymmetryGuardStageTargetOrders` marked `[MethodImpl(MethodImplOptions.NoInlining)]` — isolates the `Print()` cold diagnostic path from hot-path dispatch.
- List allocations (`new List<Order>()`) remain; acceptable — bracket submission is a once-per-position cold event.
- `SymmetryGuardBuildStopOrder` and `SymmetryGuardInitFollowerBracketFSM` are unmarked (compiler defaults); not hot-path targets.

### trading_billions (Defense in depth / single responsibility / circuit breaker)
- `if (pos.BracketSubmitted) return;` double-submission circuit breaker preserved in parent.
- Each helper has strict single responsibility:
  - H1 = stop order construction
  - H2 = target slot iteration + validation + staging
  - H3 = FSM object initialization
- Parent = orchestration + atomic commit only.

---

## MCP Evidence

| MCP Tool | Result |
|---|---|
| `resolve_repo` | Repo indexed: 5147 symbols, 177 C# files |
| `get_symbol_source` | Source confirmed: lines 285-425, 141 lines |
| `get_symbol_complexity` | CYC=16 (live), max_nesting=5, assessment=high |
| `get_call_hierarchy` | 3 callers: OnFollowerFill, TryResolveFollower, ProcessPendingFollowerFills; 34 callees |
| `get_dependency_graph` | No cross-file import edges from Follower.cs (partial class pattern) |
| `search_symbols` | Symbol confirmed in `src/V12_002.Symmetry.Follower.cs` line 285 |

---

## Sequential Thinking Evidence

| Thought | Outcome |
|---|---|
| Thought 1 | Identified 14 CYC drivers; confirmed live CYC=16 supersedes stale report of 0 |
| Thought 2 | Designed 3 helpers with projected CYC: H1=1, H2=6, H3=4; parent=6 |
| Thought 3 | Verified Jane Street alignment: Left-Right for FSM, NoInlining for logging helper, circuit breaker preserved |

---

## Scope Compliance (V12.23)

- Helpers are same-file private methods in `src/V12_002.Symmetry.Follower.cs`
- No caller signature changes (`SymmetryGuardOnFollowerFill`, `SymmetryGuardTryResolveFollower` unchanged)
- No cross-file refactoring
- One epic = one concern: complexity reduction of `SymmetryGuardSubmitFollowerBracket` only

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 1.5 |
| **Execution Time** | batch |
| **Phase** | 2 |
| **Wave** | 7 |
| **MCP Tools Used** | resolve_repo, get_file_outline, search_symbols, get_symbol_source, get_symbol_complexity, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **Sequential Thinking Thoughts** | 3 |
| **Extraction Count** | 3 |
| **Max CYC Projected** | 6 |
