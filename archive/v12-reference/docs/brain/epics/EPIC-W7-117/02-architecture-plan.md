# EPIC-W7-117 — Phase 2: Architecture Plan

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Epic:** EPIC-W7-117
**Method:** `SymmetryGuardReplaceExistingFollowerTarget`
**Source File:** `src/V12_002.Symmetry.Replace.cs`
**CYC Baseline:** 9
**CYC Target:** ≤ 8

---

## Extraction Plan

| # | New Helper | Signature | Extracted Logic | CYC Projected | Jane Street Attribute |
|---|-----------|-----------|-----------------|---------------|----------------------|
| 1 | `IsOrderLiveState` | `private static bool IsOrderLiveState(Order o)` | 4-case OrderState OR predicate (Working \|\| Accepted \|\| Submitted \|\| ChangePending). Appears twice in parent — eliminates both duplicate blocks and drift risk vs Propagation.cs:424. | 1 | `[MethodImpl(AggressiveInlining)]` — hot-path predicate, zero-alloc |
| 2 | `ExecuteTargetReplacePhase1` | `private void ExecuteTargetReplacePhase1(PositionInfo pos, Order oldTarget, int targetNumber, string fleetEntryName, string signalName)` | Entire replace-eligible branch: FollowerTargetReplaceSpec construction, `_followerTargetReplaceSpecs` write, `StampReaperMoveGrace()`, `pos.ExecutingAccount.Cancel()`. Encapsulates DNA-FIX Phase 1 FSM explicitly. | 3 | `[MethodImpl(NoInlining)]` — cold broker-interaction path |

### Parent Method CYC After Extraction

| Path | Branches | Count |
|------|----------|-------|
| Null guard (`ExecutingAccount == null`) | +1 | 1 |
| `isFilled \|\| isRunner` (2 OR predicates) | +2 | 3 |
| `qty <= 0` | +1 | 4 |
| Stale dict `TryGetValue` + null check | +1 | 5 |
| `IsOrderLiveState(staleTarget)` if-branch | +1 | 6 |
| Dict miss guard (`!TryGetValue`) | +1 | 7 |
| `IsOrderLiveState(oldTarget)` if-branch | +1 | 8 |
| Delegated to `ExecuteTargetReplacePhase1` | — | — |

**max_cyc_projected = 8** ✓

---

## Refactored Method Sketch

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private static bool IsOrderLiveState(Order o) =>
    o.OrderState == OrderState.Working
    || o.OrderState == OrderState.Accepted
    || o.OrderState == OrderState.Submitted
    || o.OrderState == OrderState.ChangePending;

private void SymmetryGuardReplaceExistingFollowerTarget(
    string fleetEntryName,
    PositionInfo pos,
    int targetNumber,
    ConcurrentDictionary<string, Order> dict
)
{
    if (pos.ExecutingAccount == null)
        return;
    string targetTag = "T" + targetNumber;
    bool isRunner = IsRunnerTarget(targetNumber);
    bool isFilled = IsTargetFilled(pos, targetNumber);
    int qty = GetTargetContracts(pos, targetNumber);

    if (isFilled || isRunner || qty <= 0)
    {
        if (dict.TryGetValue(fleetEntryName, out var staleTarget) && staleTarget != null)
        {
            if (IsOrderLiveState(staleTarget))
                pos.ExecutingAccount.Cancel(new[] { staleTarget });
            dict.TryRemove(fleetEntryName, out _);
        }
        return;
    }

    if (!dict.TryGetValue(fleetEntryName, out var oldTarget) || oldTarget == null)
        return;

    if (IsOrderLiveState(oldTarget))
    {
        string signalName = SymmetryTrim(targetTag + "_" + fleetEntryName, 40);
        ExecuteTargetReplacePhase1(pos, oldTarget, targetNumber, fleetEntryName, signalName);
    }
}

[MethodImpl(MethodImplOptions.NoInlining)]
private void ExecuteTargetReplacePhase1(
    PositionInfo pos,
    Order oldTarget,
    int targetNumber,
    string fleetEntryName,
    string signalName
)
{
    double newPrice = GetTargetPrice(pos, targetNumber);
    if (newPrice <= 0)
        return;
    int qty = GetTargetContracts(pos, targetNumber);
    OrderAction exitAction =
        pos.Direction == MarketPosition.Long ? OrderAction.Sell : OrderAction.BuyToCover;
    var tSpec = new FollowerTargetReplaceSpec
    {
        EntryName = fleetEntryName,
        TargetNum = targetNumber,
        NewTargetPrice = Instrument.MasterInstrument.RoundToTickSize(newPrice),
        Quantity = qty,
        ExitAction = exitAction,
        TargetAccount = pos.ExecutingAccount,
        CancellingOrderId = oldTarget.OrderId,
    };
    _followerTargetReplaceSpecs[signalName] = tSpec;
    StampReaperMoveGrace();
    pos.ExecutingAccount.Cancel(new[] { oldTarget });
}
```

---

## MCP Evidence

| Tool | Key Finding |
|------|-------------|
| `search_symbols` | Confirmed `SymmetryGuardReplaceExistingFollowerTarget` at `src/V12_002.Symmetry.Replace.cs:27` |
| `get_context_bundle` | Full source retrieved — 71-line method, CYC=9, duplicate 4-state OrderState block confirmed |
| `get_call_hierarchy` | 1 caller: `SymmetryGuardRetargetExistingFollowerBracket` (same file, line 17). 14 callees including `IsRunnerTarget`, `IsTargetFilled`, `GetTargetContracts`, `GetTargetPrice`, `SymmetryTrim`, `StampReaperMoveGrace` |
| `get_dependency_graph` | File imports `System.Collections.Concurrent`, `NinjaTrader.Cbi` — ConcurrentDictionary state mutation confirmed lock-free |

---

## Sequential Thinking Evidence

| Thought | Finding |
|---------|---------|
| 1 — Complexity Drivers | Dual 4-case OrderState predicate (two sites in one method), three-way entry-path decision, cross-file DNA-FIX Phase 1/Phase 2 FSM dependency |
| 2 — Extraction Strategy | Extract `IsOrderLiveState` (eliminates duplicated predicate + drift risk), extract `ExecuteTargetReplacePhase1` (names and isolates DNA-FIX Phase 1 FSM). Parent CYC reduces from 9 to 8. |
| 3 — CYC Validation | Parent post-extraction = 8 ✓; `IsOrderLiveState` = 1 ✓; `ExecuteTargetReplacePhase1` = 3 ✓. All ≤ 8. Jane Street alignment confirmed: AggressiveInlining on hot predicate, NoInlining on cold broker path, no lock(), no LINQ. |

---

## Jane Street Compliance

| Rule | Applied |
|------|---------|
| Zero-alloc hot path | `IsOrderLiveState` is a pure predicate, no heap allocation |
| AggressiveInlining hot / NoInlining cold | `IsOrderLiveState`: AggressiveInlining; `ExecuteTargetReplacePhase1`: NoInlining |
| No new `lock()` blocks | State via `ConcurrentDictionary` — lock-free ✓ |
| Single responsibility per helper | `IsOrderLiveState`: state test only; `ExecuteTargetReplacePhase1`: Phase 1 FSM only |
| Each helper CYC ≤ 8 | `IsOrderLiveState`=1, `ExecuteTargetReplacePhase1`=3 ✓ |
| Avoid LINQ | No LINQ in any helper ✓ |

---

## Agent Tracking

| Field | Value |
|-------|-------|
| **Agent Name** | v12-phase2-architecture |
| **Phase** | 2 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-117 |
| **CYC Baseline** | 9 |
| **max_cyc_projected** | 8 |
| **Extractions** | 2 |
