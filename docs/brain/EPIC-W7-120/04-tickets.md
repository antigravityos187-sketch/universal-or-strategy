# EPIC-W7-120 — Phase 4: Ticket Definitions

**Agent:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Epic:** EPIC-W7-120
**Method:** `HandleFsmFilled`
**Source File:** `src/V12_002.Symmetry.BracketFSM.cs`
**CYC Baseline:** 14
**CYC Target:** ≤ 8
**ticket_count:** 3

---

## Summary

Three extraction tickets reduce `HandleFsmFilled` from CYC 14 → 5 by isolating three distinct
boolean-compound concerns into private static helpers. All helpers and the refactored parent
satisfy the Jane Street CYC ≤ 8 mandate.

---

## Ticket 1 — Extract `IsStopSignal`

| Field | Value |
|-------|-------|
| **ticket_id** | T1 |
| **helper_name** | `IsStopSignal` |
| **concern** | Stop signal detection — answers "is this signal a stop order fill event?" |
| **signature** | `private static bool IsStopSignal(string signalName)` |
| **decorator** | `[MethodImpl(MethodImplOptions.AggressiveInlining)]` |
| **lines_to_move** | The `bool isStop` predicate block inside `HandleFsmFilled` (lines ~350–354): the `string.IsNullOrEmpty(signalName)` null-guard plus two `StartsWith` OR-arms — `signalName.StartsWith("Stop_") \|\| signalName.StartsWith("S_")` |
| **cyc_reduction** | 3 removed from parent (1 IsNullOrEmpty null-guard + 2 OR-branches) |
| **projected_helper_cyc** | 4 |

### Refactored Body

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private static bool IsStopSignal(string signalName) =>
    !string.IsNullOrEmpty(signalName)
    && (signalName.StartsWith("Stop_") || signalName.StartsWith("S_"));
```

### CYC Breakdown

| Branch | +CYC |
|--------|------|
| Base | 1 |
| `!IsNullOrEmpty` guard | +1 |
| `StartsWith("Stop_")` | +1 |
| `StartsWith("S_")` | +1 |
| **Total** | **4** ✓ |

---

## Ticket 2 — Extract `IsTargetSignal`

| Field | Value |
|-------|-------|
| **ticket_id** | T2 |
| **helper_name** | `IsTargetSignal` |
| **concern** | Target signal detection — answers "is this signal a target bracket fill event?" |
| **signature** | `private static bool IsTargetSignal(string signalName)` |
| **decorator** | `[MethodImpl(MethodImplOptions.AggressiveInlining)]` |
| **lines_to_move** | The `bool isTarget` predicate block inside `HandleFsmFilled` (lines ~355–362): the null-guard `string.IsNullOrEmpty(signalName)` plus 5 OR-arms — `signalName.StartsWith("T1_") \|\| signalName.StartsWith("T2_") \|\| signalName.StartsWith("T3_") \|\| signalName.StartsWith("T4_") \|\| signalName.StartsWith("T5_")` |
| **cyc_reduction** | 6 removed from parent (1 null-guard + 5 OR-branches) |
| **projected_helper_cyc** | 7 |

### Refactored Body

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private static bool IsTargetSignal(string signalName) =>
    !string.IsNullOrEmpty(signalName)
    && (
        signalName.StartsWith("T1_")
        || signalName.StartsWith("T2_")
        || signalName.StartsWith("T3_")
        || signalName.StartsWith("T4_")
        || signalName.StartsWith("T5_")
    );
```

### CYC Breakdown

| Branch | +CYC |
|--------|------|
| Base | 1 |
| `!IsNullOrEmpty` guard | +1 |
| `StartsWith("T1_")` | +1 |
| `StartsWith("T2_")` | +1 |
| `StartsWith("T3_")` | +1 |
| `StartsWith("T4_")` | +1 |
| `StartsWith("T5_")` | +1 |
| **Total** | **7** ✓ |

---

## Ticket 3 — Extract `ApplyFillContracts`

| Field | Value |
|-------|-------|
| **ticket_id** | T3 |
| **helper_name** | `ApplyFillContracts` |
| **concern** | FSM contract accounting — decrements remaining contracts and transitions FSM state to Filled or Active |
| **signature** | `private static void ApplyFillContracts(FollowerBracketFSM fsm, int filledQty)` |
| **decorator** | `[MethodImpl(MethodImplOptions.AggressiveInlining)]` |
| **lines_to_move** | The contract-mutation block inside the `if (isStop \|\| isTarget)` branch: `fsm.RemainingContracts = Math.Max(0, fsm.RemainingContracts - Math.Max(0, filledQty))` plus ternary state assignment `fsm.State = fsm.RemainingContracts <= 0 ? FollowerBracketState.Filled : FollowerBracketState.Active` |
| **cyc_reduction** | 2 removed from parent (nested Math.Max guard + ternary state branch) |
| **projected_helper_cyc** | 2 |

### Refactored Body

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private static void ApplyFillContracts(FollowerBracketFSM fsm, int filledQty)
{
    fsm.RemainingContracts = Math.Max(0, fsm.RemainingContracts - Math.Max(0, filledQty));
    fsm.State = fsm.RemainingContracts <= 0
        ? FollowerBracketState.Filled
        : FollowerBracketState.Active;
}
```

### CYC Breakdown

| Branch | +CYC |
|--------|------|
| Base | 1 |
| Ternary `<= 0` | +1 |
| **Total** | **2** ✓ |

---

## Projected Parent After All Extractions

```csharp
// HandleFsmFilled (parent) — CYC = 5
private void HandleFsmFilled(AccountEvent evt, FollowerBracketFSM fsm)
{
    bool isStop = IsStopSignal(evt.SignalName);
    bool isTarget = IsTargetSignal(evt.SignalName);

    if (isStop || isTarget)
    {
        ApplyFillContracts(fsm, evt.FilledQty);
    }
    else if (fsm.State == FollowerBracketState.Accepted
             || fsm.State == FollowerBracketState.Submitted)
    {
        fsm.State = FollowerBracketState.Active;
    }
}
```

### Parent CYC Breakdown

| Branch | +CYC |
|--------|------|
| Base | 1 |
| `if (isStop \|\| isTarget)` — OR compound | +2 |
| `else if (Accepted \|\| Submitted)` — OR compound | +2 |
| **Total** | **5** ✓ |

---

## CYC Summary

| Symbol | Baseline | Projected | ≤ 8? |
|--------|----------|-----------|------|
| `HandleFsmFilled` (parent) | 14 | 5 | ✓ |
| `IsStopSignal` | N/A (new) | 4 | ✓ |
| `IsTargetSignal` | N/A (new) | 7 | ✓ |
| `ApplyFillContracts` | N/A (new) | 2 | ✓ |
| **max_cyc_projected** | | **7** | ✓ |

**projected_parent_cyc_after_all: 5**

---

## Sequential Thinking Evidence

| Thought | Conclusion |
|---------|-----------|
| 1 — Ticket count | 3 tickets required — one per complexity driver (IsStop block, IsTarget block, contract mutation block). Each maps to exactly one single-responsibility helper. |
| 2 — Per-ticket detail | T1: IsStopSignal (CYC=4, removes 3 from parent); T2: IsTargetSignal (CYC=7, removes 6 from parent); T3: ApplyFillContracts (CYC=2, removes 2 from parent). |
| 3 — CYC verification | Parent=5 ✓, IsStopSignal=4 ✓, IsTargetSignal=7 ✓, ApplyFillContracts=2 ✓. Max=7 ≤ 8. All pass Jane Street mandate. |

---

## jCodemunch MCP Evidence

| Tool | Result |
|------|--------|
| `resolve_repo` | `repo="antigravityos187-sketch/universal-or-strategy"`, indexed=true, 5147 symbols |
| `search_symbols` (HandleFsmFilled) | Found at `src/V12_002.Symmetry.BracketFSM.cs` line 349, ID: `V12_002.HandleFsmFilled#method` |
| `get_symbol_complexity` | `cyclomatic=14`, `max_nesting=3`, `param_count=2`, `lines=27`, `assessment="high"` — CYC 14 confirmed |
| `get_extraction_candidates` | 0 candidates (thresholds: min_complexity=5, min_callers=2) — callers=1 excludes from auto-candidate; manual extraction plan from Phase 2 applies |

---

## Agent Tracking

| Field | Value |
|-------|-------|
| **Agent Name** | v12-phase4-tickets |
| **Phase** | 4 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-120 |
| **ticket_count** | 3 |
| **max_cyc_projected** | 7 (IsTargetSignal) |
| **projected_parent_cyc_after_all** | 5 |
| **Bobcoins Used** | 7 |
| **Execution Time** | ~60s |
| **MCP Tools Used** | resolve_repo, sequentialthinking (x4), search_symbols, get_symbol_complexity, get_extraction_candidates |
