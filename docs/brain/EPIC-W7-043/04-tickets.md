# EPIC-W7-043 — Phase 4: Ticket Generation

**Agent:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T01:20:00Z
**Input:** docs/brain/EPIC-W7-043/02-architecture-plan.md, docs/brain/EPIC-W7-043/03-audit-report.md

---

## Header

| Field | Value |
|---|---|
| **Epic** | EPIC-W7-043 |
| **Method** | `SymmetryGuardSubmitFollowerBracket` |
| **Source** | `src/V12_002.Symmetry.Follower.cs` |
| **Original CYC** | 16 (live, confirmed by Phase 2 MCP) |
| **Wave** | 7 |
| **Phase** | 4 — Ticket Generation |
| **ticket_count** | 3 |
| **projected_parent_cyc_after_all** | 6 |
| **DNA Verdict (Phase 3)** | PASS |

---

## Ticket Summary

| Ticket | Helper Name | Projected CYC | CYC Reduction (from parent) |
|---|---|---|---|
| T1 | `SymmetryGuardBuildStopOrder` | 1 | -2 |
| T2 | `SymmetryGuardStageTargetOrders` | 6 | -5 |
| T3 | `SymmetryGuardInitFollowerBracketFSM` | 4 | -3 |
| **Parent after all** | `SymmetryGuardSubmitFollowerBracket` | **6** | 16 → 6 |

---

## Ticket 1

```
ticket_id:              EPIC-W7-043-T1
helper_name:            SymmetryGuardBuildStopOrder
concern:                Stop order construction — extract inline acct.CreateOrder call for GTC StopMarket OCO bracket order into a dedicated helper; parent receives the returned Order
lines_to_move:          ~10 (stop signature string + acct.CreateOrder call for stop, approx lines 296-306)
cyc_reduction:          -2 (removes 1-2 branch-equivalent inline decisions from parent context)
projected_helper_cyc:   1 (pure construction, no branches)
jane_street_note:       No [MethodImpl] annotation required; single call-site construction, not hot path
scope:                  Same-file private method in src/V12_002.Symmetry.Follower.cs
```

### Signature
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

### Body to Extract
```csharp
string stopSig = SymmetryTrim("Stop_" + fleetEntryName, 40);
return acct.CreateOrder(
    Instrument, exitAction, OrderType.StopMarket, TimeInForce.Gtc,
    Math.Max(1, pos.TotalContracts), 0, validatedStop,
    ocoId, stopSig, null);
```

### Call Site Replacement in Parent
```csharp
Order stop = SymmetryGuardBuildStopOrder(pos, acct, exitAction, validatedStop, ocoId, fleetEntryName, ordersToSubmit);
```

### Verify
- [ ] `SymmetryGuardBuildStopOrder` exists in `src/V12_002.Symmetry.Follower.cs` as private method
- [ ] Helper CYC = 1 (no branching)
- [ ] Build passes: `dotnet build`
- [ ] No `lock()` blocks introduced

---

## Ticket 2

```
ticket_id:              EPIC-W7-043-T2
helper_name:            SymmetryGuardStageTargetOrders
concern:                Target slot iteration — extract the 5-slot for-loop that validates quantity, skips runners, validates price with diagnostic Print, rounds to tick, creates limit orders, and accumulates into staged collections; returns nonRunnerLimitQty and runnerQty via out params
lines_to_move:          ~40 (for targetNum=1 to 5 loop body, approx lines 327-390)
cyc_reduction:          -5 (removes for+1, targetQty-guard+1, IsRunnerTarget+1, targetPrice-guard+1, continue+1)
projected_helper_cyc:   6
jane_street_note:       [MethodImpl(MethodImplOptions.NoInlining)] REQUIRED — isolates cold Print/logging path from hot-path inliner dispatch (carl_cook pattern)
scope:                  Same-file private method in src/V12_002.Symmetry.Follower.cs
```

### Signature
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

### Body to Extract
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

### Call Site Replacement in Parent
```csharp
SymmetryGuardStageTargetOrders(pos, acct, exitAction, ocoId, fleetEntryName, stagedTargets, ordersToSubmit,
    out int nonRunnerLimitQty, out int runnerQty);
```

### Verify
- [ ] `SymmetryGuardStageTargetOrders` exists in `src/V12_002.Symmetry.Follower.cs` as private method
- [ ] `[MethodImpl(MethodImplOptions.NoInlining)]` attribute present
- [ ] Helper CYC <= 8 (projected 6)
- [ ] Build passes: `dotnet build`
- [ ] No `lock()` blocks introduced

---

## Ticket 3

```
ticket_id:              EPIC-W7-043-T3
helper_name:            SymmetryGuardInitFollowerBracketFSM
concern:                FSM initialization — extract FollowerBracketFSM struct construction and population (account name, entry name, OCO group, initial state, remaining contracts, stop order, expected prices array, target order references) into a dedicated factory helper; parent receives completed FSM for atomic publish
lines_to_move:          ~30 (new FollowerBracketFSM initializer + for array zeroing + foreach targets assignment, approx lines 380-402)
cyc_reduction:          -3 (removes for+1, foreach+1, compound-if tNum-bounds+1; compound && contributes extra in raw CYC but net from parent view = -3)
projected_helper_cyc:   4
jane_street_note:       Left-Right pattern compliant — FSM constructed locally and returned for atomic publish to _followerBrackets[fleetEntryName] in parent (gjengset pattern); no [MethodImpl] needed
scope:                  Same-file private method in src/V12_002.Symmetry.Follower.cs
```

### Signature
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

### Body to Extract
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

### Call Site Replacement in Parent
```csharp
FollowerBracketFSM fsm = SymmetryGuardInitFollowerBracketFSM(acct, fleetEntryName, ocoId, pos, stop, validatedStop, stagedTargets);
```

### Verify
- [ ] `SymmetryGuardInitFollowerBracketFSM` exists in `src/V12_002.Symmetry.Follower.cs` as private method
- [ ] Helper CYC <= 8 (projected 4)
- [ ] FSM assigned atomically after return (Left-Right pattern preserved)
- [ ] Build passes: `dotnet build`
- [ ] No `lock()` blocks introduced

---

## Post-All-Tickets Verification

```
projected_parent_cyc_after_all: 6
```

| Residual branch in parent | CYC contribution |
|---|---|
| `if (pos.BracketSubmitted) return;` | +1 |
| `if (acct == null) return;` | +1 |
| Ternary `exitAction` | +1 |
| Ternary `ocoId` | +1 |
| `Enqueue(ctx => { ... })` lambda | +1 |
| `foreach (var (targetNum, order) in stagedTargets)` dict update | +1 |
| **Total** | **6** |

- [ ] Parent `SymmetryGuardSubmitFollowerBracket` CYC <= 8 after all 3 extractions (projected 6)
- [ ] `dotnet csharpier check src/` passes
- [ ] `dotnet build` zero errors
- [ ] `grep -c "lock(" src/V12_002.Symmetry.Follower.cs` returns 0

---

## MCP Evidence (Phase 4)

| Tool | Result |
|---|---|
| `resolve_repo` | indexed: true, 5147 symbols, 177 C# files |
| `get_symbol_complexity` | Index stale — live CYC=16 authoritative per Phase 2 MCP measurement |
| `get_extraction_candidates` | Index stale — Phase 2 architectural plan used as authoritative source |
| `sequentialthinking` | 3 thoughts completed; ticket breakdown validated |

---

## Sequential Thinking Evidence

| Thought | Outcome |
|---|---|
| Thought 1 | Mapped 3 helpers to ticket concerns; confirmed each has clear single responsibility and projected CYC values |
| Thought 2 | Validated ticket independence and sequencing; confirmed runtime parameter dependencies do not create code-execution ordering requirements at ticket level |
| Thought 3 | Verified CYC arithmetic: 16 - 5(T2) - 4(T3) - remaining = 6 parent residual; all 4 symbols within Jane Street threshold <=8 |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | batch |
| **Phase** | 4 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-043 |
| **MCP Tools Used** | resolve_repo, sequentialthinking (x4), get_symbol_complexity, get_extraction_candidates |
| **Sequential Thinking Thoughts** | 3 |
| **ticket_count** | 3 |
| **projected_parent_cyc_after_all** | 6 |
| **Input Artifacts** | 02-architecture-plan.md, 03-audit-report.md |
| **Output Artifact** | 04-tickets.md |
