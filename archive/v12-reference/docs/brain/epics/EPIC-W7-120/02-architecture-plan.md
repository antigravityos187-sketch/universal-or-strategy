# EPIC-W7-120 — Phase 2: Architecture Plan

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Epic:** EPIC-W7-120
**Method:** `HandleFsmFilled`
**Source File:** `src/V12_002.Symmetry.BracketFSM.cs`
**CYC Baseline:** 14
**CYC Target:** ≤ 8

---

## Extraction Plan

| # | New Helper | Signature | Extracted Logic | CYC Projected | Jane Street Attribute |
|---|-----------|-----------|-----------------|---------------|----------------------|
| 1 | `IsStopSignal` | `private static bool IsStopSignal(string signalName)` | IsNullOrEmpty null-guard + `StartsWith("Stop_")` + `\|\|` `StartsWith("S_")`. Removes 3 CYC from parent. | 4 | `[MethodImpl(AggressiveInlining)]` — hot-path fill predicate, zero-alloc |
| 2 | `IsTargetSignal` | `private static bool IsTargetSignal(string signalName)` | IsNullOrEmpty null-guard + 5×`StartsWith("Tn_")` OR arms (T1–T5). Removes 6 CYC from parent. | 7 | `[MethodImpl(AggressiveInlining)]` — hot-path fill predicate, zero-alloc |
| 3 | `ApplyFillContracts` | `private static void ApplyFillContracts(FollowerBracketFSM fsm, int filledQty)` | `RemainingContracts` decrement + ternary `State` assignment (Filled vs Active). Removes 2 CYC from parent. | 2 | `[MethodImpl(AggressiveInlining)]` — hot-path FSM mutation, zero-alloc |

### Parent Method CYC After Extraction

| Path | Branches | Count |
|------|----------|-------|
| Base | +1 | 1 |
| `if (isStop \|\| isTarget)` — two predicates | +2 | 3 |
| `else if (Accepted \|\| Submitted)` — two predicates | +2 | 5 |
| All `IsStopSignal`/`IsTargetSignal` decisions removed | — | 5 |

**max_cyc_projected = 5** ✓ (parent); **7** (IsTargetSignal); **4** (IsStopSignal); **2** (ApplyFillContracts)

---

## Refactored Method Sketch

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private static bool IsStopSignal(string signalName) =>
    !string.IsNullOrEmpty(signalName)
    && (signalName.StartsWith("Stop_") || signalName.StartsWith("S_"));

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

[MethodImpl(MethodImplOptions.AggressiveInlining)]
private static void ApplyFillContracts(FollowerBracketFSM fsm, int filledQty)
{
    fsm.RemainingContracts = Math.Max(0, fsm.RemainingContracts - Math.Max(0, filledQty));
    fsm.State = fsm.RemainingContracts <= 0
        ? FollowerBracketState.Filled
        : FollowerBracketState.Active;
}

// Refactored parent — CYC = 5
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

---

## MCP Evidence

| Tool | Key Finding |
|------|-------------|
| `get_context_bundle` | Full 27-line source retrieved. CYC=14 confirmed: isStop block (3 decisions), isTarget block (6 decisions), outer if ||, ternary, else if || = 14. |
| `get_call_hierarchy` | 1 direct caller: `ProcessBracketEvent` (line 381). Upstream: `DrainAccountMailbox` (line 88). 0 callees (pure FSM mutation, no helper calls). Confirms strategy-thread-only execution. |

---

## Sequential Thinking Evidence

| Thought | Finding |
|---------|---------|
| 1 — Complexity Drivers | 3 driver groups confirmed: IsStop block (+3), IsTarget block (+6), outer if/else-if compounds (+3) and ternary (+1) = 14. Purely boolean-compound driven (no loops). |
| 2 — Extraction Strategy | Extract `IsStopSignal` (static, AggressiveInlining, CYC=4), `IsTargetSignal` (static, AggressiveInlining, CYC=7), `ApplyFillContracts` (static, AggressiveInlining, CYC=2). Parent reduces to CYC=5. |
| 3 — CYC Validation | Parent=5 ✓; IsStopSignal=4 ✓; IsTargetSignal=7 ✓; ApplyFillContracts=2 ✓. All ≤ 8. Static helpers allow compiler devirtualization. No lock() needed — strategy-thread mailbox serialization preserved. |

---

## Jane Street Compliance

| Rule | Applied |
|------|---------|
| Zero-alloc hot path | All 3 helpers are `private static`, pure predicates/mutations — no heap allocation ✓ |
| AggressiveInlining hot | All 3 helpers: `AggressiveInlining` (fill event handler is a hot FSM path) ✓ |
| No new `lock()` blocks | Strategy-thread only (drain via mailbox), no new synchronization needed ✓ |
| Single responsibility per helper | `IsStopSignal`: stop detection; `IsTargetSignal`: target detection; `ApplyFillContracts`: contract tracking ✓ |
| Each helper CYC ≤ 8 | 4, 7, 2 ✓ |
| Avoid LINQ | No LINQ ✓ |

---

## Agent Tracking

| Field | Value |
|-------|-------|
| **Agent Name** | v12-phase2-architecture |
| **Phase** | 2 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-120 |
| **CYC Baseline** | 14 |
| **max_cyc_projected** | 7 (IsTargetSignal) |
| **Extractions** | 3 |
