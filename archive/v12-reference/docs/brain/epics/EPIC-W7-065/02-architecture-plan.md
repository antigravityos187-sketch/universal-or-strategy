# Phase 2: Architecture Plan — EPIC-W7-065

## Method Under Extraction

- **Method:** `HandleFsmFilled`
- **Source File:** `src/V12_002.Symmetry.BracketFSM.cs`
- **Lines:** 349–375
- **Original CYC:** 14
- **Target CYC:** ≤ 8 (Jane Street strict standard)

### jcodemunch get_context_bundle result

Symbol resolved: `src/V12_002.Symmetry.BracketFSM.cs::V12_002.HandleFsmFilled#method`

Key findings:
- `private void HandleFsmFilled(AccountEvent evt, FollowerBracketFSM fsm)` — private, no external callers
- Computes `isStop`: null guard + 2× `StartsWith` (`"Stop_"`, `"S_"`) — 3 decision points inline
- Computes `isTarget`: null guard + 5× `StartsWith` (`"T1_"`–`"T5_"`) — 6 decision points inline
- Outer `if (isStop || isTarget)` → contract decrement + ternary state transition — 3 decision points
- `else if (Accepted || Submitted)` → entry promotion to Active — 2 decision points
- Total inline decisions: 14 → CYC 14 confirmed

### jcodemunch get_call_hierarchy result

| Direction | Symbol | File | Depth | Resolution |
|---|---|---|---|---|
| Caller (depth 1) | `ProcessBracketEvent` | `src/V12_002.Symmetry.BracketFSM.cs:381` | 1 | ast_resolved |
| Caller (depth 2) | `DrainAccountMailbox` | `src/V12_002.Symmetry.BracketFSM.cs:88` | 2 | ast_resolved |
| Callees | _(none)_ | — | — | — |

Caller count: 1 direct (`ProcessBracketEvent`). No callees — method is a leaf. Full call chain: `DrainAccountMailbox → ProcessBracketEvent → HandleFsmFilled`.

### jcodemunch get_dependency_graph result

- `src/V12_002.Symmetry.BracketFSM.cs`: 0 cross-file import edges, 0 importers detected at the file level
- File is a partial class (`V12_002`) — C# partial class resolution is compile-time, not import-graph-visible
- Extraction within same partial class file is safe; no cross-file dependency graph changes needed

### jcodemunch get_extraction_candidates result

No candidates returned by heuristic (min_complexity=3, min_callers=1). The file's extraction candidates are not surfaced because the inline boolean expressions (not standalone methods) are the complexity drivers. The architecture plan below addresses this via manual extraction design based on the actual source body.

---

## Sequential Thinking Summary

**Thought 1 (Analysis):** Confirmed CYC=14 via decision-point counting from actual source: 3 (isStop) + 6 (isTarget) + 2 (outer if short-circuit) + 1 (ternary) + 2 (else-if short-circuit) = 14. Extraction targets the 9 inline prefix-dispatch branches.

**Thought 2 (CYC Projection):** `IsStopSignal` → CYC 4 (null guard + 2 prefix branches + 1 base). `IsTargetSignal` → CYC 7 (null guard + 5 prefix branches + 1 base). Parent after extraction → CYC 6 (2 short-circuit + 1 ternary + 2 else-if + 1 base). max_cyc_projected = 7. All ≤ 8. ✅

**Thought 3 (Jane Street Alignment):** CYC ≤ 8 achieved on all three methods. Helpers are `private static` — single concern, zero allocation, guard-first. No lock() blocks introduced. No heap allocs (pure boolean logic with `StartsWith` on string literals).

**Thought 4 (Safety Validation):** HandleFsmFilled signature unchanged — ProcessBracketEvent call site unaffected. Helpers are private static — no accidental cross-class access. Dependency graph shows 0 cross-file edges — extraction is fully self-contained in `BracketFSM.cs`.

**Thought 5 (Final Verdict):** APPROVED. 2 helpers (IsStopSignal CYC=4, IsTargetSignal CYC=7), parent CYC=6. max_cyc_projected=7. Jane Street: FULL alignment. V12.23: COMPLIANT. Minimal change principle satisfied — no third helper needed, parent already at CYC=6 post-extraction.

---

## Extraction Plan

| Helper Method Name | Responsibility | Signature | Projected CYC |
|---|---|---|---|
| `IsStopSignal` | Returns `true` when `SignalName` matches the stop-order prefix pattern (`"Stop_"` or `"S_"`) | `private static bool IsStopSignal(string name)` | **4** |
| `IsTargetSignal` | Returns `true` when `SignalName` matches any of the 5 target-order prefix patterns (`"T1_"`–`"T5_"`) | `private static bool IsTargetSignal(string name)` | **7** |

### Helper Method Bodies (Reference Implementation)

```csharp
private static bool IsStopSignal(string name)
{
    return !string.IsNullOrEmpty(name)
        && (name.StartsWith("Stop_") || name.StartsWith("S_"));
}

private static bool IsTargetSignal(string name)
{
    return !string.IsNullOrEmpty(name)
        && (
            name.StartsWith("T1_")
            || name.StartsWith("T2_")
            || name.StartsWith("T3_")
            || name.StartsWith("T4_")
            || name.StartsWith("T5_")
        );
}
```

---

## Parent Method After Extraction

### Remaining Logic

```csharp
private void HandleFsmFilled(AccountEvent evt, FollowerBracketFSM fsm)
{
    bool isStop = IsStopSignal(evt.SignalName);
    bool isTarget = IsTargetSignal(evt.SignalName);

    if (isStop || isTarget)
    {
        fsm.RemainingContracts = Math.Max(0, fsm.RemainingContracts - Math.Max(0, evt.FilledQty));
        fsm.State = fsm.RemainingContracts <= 0 ? FollowerBracketState.Filled : FollowerBracketState.Active;
    }
    else if (fsm.State == FollowerBracketState.Accepted || fsm.State == FollowerBracketState.Submitted)
    {
        fsm.State = FollowerBracketState.Active;
    }
}
```

- **Remaining decision points:** `isStop || isTarget` (2) + ternary (1) + `Accepted || Submitted` (2) = 5
- **Projected CYC:** 6

---

## max_cyc_projected: 7
## extraction_count: 2

---

## Jane Street Alignment

| Principle | Status | Evidence |
|---|---|---|
| CYC ≤ 8 achieved | **YES** | IsStopSignal=4, IsTargetSignal=7, parent=6; max=7 |
| Single-responsibility per helper | **YES** | `IsStopSignal` classifies stop signals only; `IsTargetSignal` classifies target signals only |
| Lock-free / Actor pattern preserved | **YES** | No lock() blocks introduced or present; FSM state mutations remain direct field assignments |
| Illegal states unrepresentable | **YES** | Helpers are `private static` — cannot mutate FSM state; null guard prevents misclassification from null `SignalName`; boolean return type leaves no ambiguous state |
| Zero-allocation hot paths | **YES** | Both helpers use only primitive boolean logic and `StartsWith` on string literals — no heap allocations |
| Guard clauses (early return) | **YES** | `IsNullOrEmpty(name)` short-circuits before any `StartsWith` comparison in both helpers |
| Extract loop body / single concern | **YES** | Each helper encapsulates exactly one classification concern; parent delegates to named helpers |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Epic** | EPIC-W7-065 |
| **Wave** | 7 |
| **Phase** | 2 |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | 2026-06-29T01:15:00Z |
| **jcodemunch tools called** | resolve_repo, get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **Output** | `docs/brain/EPIC-W7-065/02-architecture-plan.md` |
