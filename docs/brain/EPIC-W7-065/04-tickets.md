# Phase 4: Ticket Generation — EPIC-W7-065

## Summary

| Field | Value |
|---|---|
| **Epic** | EPIC-W7-065 |
| **Wave** | 7 |
| **Phase** | 4 — Ticket Generation |
| **Method** | `HandleFsmFilled` |
| **Source File** | `src/V12_002.Symmetry.BracketFSM.cs` |
| **Original CYC** | 14 |
| **ticket_count** | 2 |
| **projected_parent_cyc_after_all** | 6 |
| **max_cyc_projected** | 7 |
| **dna_verdict (Phase 3)** | PASS |

---

## Sequential Thinking Validation

### Thought 1 — Ticket Count Determination
`HandleFsmFilled` has two separable inline boolean classification clusters: (1) stop-signal prefix dispatch (`null` guard + `StartsWith("Stop_")` + `StartsWith("S_")`) and (2) target-signal prefix dispatch (`null` guard + 5× `StartsWith("T1_"`–`"T5_")` ). One ticket = one concern. **2 tickets** is the minimum-sufficient breakdown. The outer orchestration (`isStop || isTarget` branch + ternary state transition + `else if Accepted || Submitted`) remains in the parent — it is coordination logic, not a classifiable signal concern.

### Thought 2 — Lines to Move and Helper Names
- **Ticket 1**: Extract stop-classification inline logic (lines 349–353) → helper `IsStopSignal(string name)`, `private static bool`, CYC 4.
- **Ticket 2**: Extract target-classification inline logic (lines 354–360) → helper `IsTargetSignal(string name)`, `private static bool`, CYC 7.
- Parent retains: `isStop || isTarget` (2) + ternary (1) + `Accepted || Submitted` (2) + base (1) = **CYC 6**.

### Thought 3 — CYC ≤ 8 Verification
| Method | Projected CYC | ≤ 8? |
|---|---|---|
| `IsStopSignal` | 4 | ✅ |
| `IsTargetSignal` | 7 | ✅ |
| `HandleFsmFilled` (post-extraction) | 6 | ✅ |
| **max_cyc_projected** | **7** | **✅** |

All methods pass Jane Street strict standard (CYC ≤ 8). Ticket breakdown is complete and safe.

---

## Ticket Definitions

---

### TICKET-1: Extract `IsStopSignal` helper

| Field | Value |
|---|---|
| **ticket_id** | `EPIC-W7-065-T1` |
| **helper_name** | `IsStopSignal` |
| **concern** | Classify whether an `AccountEvent.SignalName` matches stop-order prefix patterns (`"Stop_"` or `"S_"`) |
| **source_method** | `HandleFsmFilled` |
| **source_file** | `src/V12_002.Symmetry.BracketFSM.cs` |
| **lines_to_move** | Lines 349–353 (inline stop-classification: null guard + `StartsWith("Stop_")` + `StartsWith("S_")`) |
| **cyc_reduction** | −3 (removes 3 decision points from parent) |
| **projected_helper_cyc** | **4** ✅ |
| **helper_visibility** | `private static` |

#### Signature
```csharp
private static bool IsStopSignal(string name)
```

#### Reference Implementation
```csharp
private static bool IsStopSignal(string name)
{
    return !string.IsNullOrEmpty(name)
        && (name.StartsWith("Stop_") || name.StartsWith("S_"));
}
```

#### Decision Points (CYC = 4)
| # | Decision | Type |
|---|---|---|
| 1 | `string.IsNullOrEmpty(name)` | null guard (short-circuit) |
| 2 | `name.StartsWith("Stop_")` | prefix match |
| 3 | `name.StartsWith("S_")` | prefix match |
| 4 | base path | function entry |

#### Replacement in Parent
Replace inline computation:
```csharp
// BEFORE (3 inline decisions)
bool isStop = evt.SignalName != null
    && (evt.SignalName.StartsWith("Stop_") || evt.SignalName.StartsWith("S_"));
```
With delegate call:
```csharp
// AFTER (0 inline decisions in parent)
bool isStop = IsStopSignal(evt.SignalName);
```

#### Acceptance Criteria
- [ ] `IsStopSignal` is `private static bool` in `V12_002.Symmetry.BracketFSM.cs`
- [ ] Returns `true` for `"Stop_"` prefix inputs
- [ ] Returns `true` for `"S_"` prefix inputs
- [ ] Returns `false` for `null`, empty string, and non-matching prefixes
- [ ] Parent `HandleFsmFilled` delegates via `IsStopSignal(evt.SignalName)`
- [ ] Build passes: `dotnet build src/`
- [ ] xUnit `[Fact]` tests cover all 4 cases (null, empty, matching, non-matching)

---

### TICKET-2: Extract `IsTargetSignal` helper

| Field | Value |
|---|---|
| **ticket_id** | `EPIC-W7-065-T2` |
| **helper_name** | `IsTargetSignal` |
| **concern** | Classify whether an `AccountEvent.SignalName` matches any of the 5 target-order prefix patterns (`"T1_"` through `"T5_"`) |
| **source_method** | `HandleFsmFilled` |
| **source_file** | `src/V12_002.Symmetry.BracketFSM.cs` |
| **lines_to_move** | Lines 354–360 (inline target-classification: null guard + 5× `StartsWith` for `T1_`–`T5_`) |
| **cyc_reduction** | −6 (removes 6 decision points from parent) |
| **projected_helper_cyc** | **7** ✅ |
| **helper_visibility** | `private static` |

#### Signature
```csharp
private static bool IsTargetSignal(string name)
```

#### Reference Implementation
```csharp
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

#### Decision Points (CYC = 7)
| # | Decision | Type |
|---|---|---|
| 1 | `string.IsNullOrEmpty(name)` | null guard (short-circuit) |
| 2 | `name.StartsWith("T1_")` | prefix match |
| 3 | `name.StartsWith("T2_")` | prefix match |
| 4 | `name.StartsWith("T3_")` | prefix match |
| 5 | `name.StartsWith("T4_")` | prefix match |
| 6 | `name.StartsWith("T5_")` | prefix match |
| 7 | base path | function entry |

#### Replacement in Parent
Replace inline computation:
```csharp
// BEFORE (6 inline decisions)
bool isTarget = evt.SignalName != null
    && (evt.SignalName.StartsWith("T1_") || evt.SignalName.StartsWith("T2_")
        || evt.SignalName.StartsWith("T3_") || evt.SignalName.StartsWith("T4_")
        || evt.SignalName.StartsWith("T5_"));
```
With delegate call:
```csharp
// AFTER (0 inline decisions in parent)
bool isTarget = IsTargetSignal(evt.SignalName);
```

#### Acceptance Criteria
- [ ] `IsTargetSignal` is `private static bool` in `V12_002.Symmetry.BracketFSM.cs`
- [ ] Returns `true` for each of `"T1_"`, `"T2_"`, `"T3_"`, `"T4_"`, `"T5_"` prefix inputs
- [ ] Returns `false` for `null`, empty string, and non-matching prefixes
- [ ] Parent `HandleFsmFilled` delegates via `IsTargetSignal(evt.SignalName)`
- [ ] Build passes: `dotnet build src/`
- [ ] xUnit `[Fact]` tests cover all 7 cases (null, empty, T1–T5 matching, non-matching)

---

## Parent Method After All Extractions

### Projected `HandleFsmFilled` (CYC = 6)
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

| Decision | CYC Contribution |
|---|---|
| `isStop \|\| isTarget` (short-circuit OR) | +2 |
| ternary `RemainingContracts <= 0 ?` | +1 |
| `Accepted \|\| Submitted` (short-circuit OR) | +2 |
| base path | +1 |
| **Total** | **6** |

**projected_parent_cyc_after_all = 6 ≤ 8** ✅

---

## CYC Reduction Summary

| Method | Before | After | Reduction |
|---|---|---|---|
| `HandleFsmFilled` (parent) | 14 | 6 | −8 |
| `IsStopSignal` (new) | — | 4 | new |
| `IsTargetSignal` (new) | — | 7 | new |
| **max_cyc_projected** | **14** | **7** | **−7** |

---

## Execution Order

Tickets are **sequential** (T2 can only run after T1 has updated the parent, to avoid merge conflicts on the same method body):

1. **TICKET-1** (`IsStopSignal`) — extract stop-signal classification
2. **TICKET-2** (`IsTargetSignal`) — extract target-signal classification

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Epic** | EPIC-W7-065 |
| **Wave** | 7 |
| **Phase** | 4 |
| **Bobcoins Used** | 0.9 |
| **Execution Time** | 2026-06-29T01:30:00Z |
| **jcodemunch tools called** | `resolve_repo`, `get_symbol_complexity`, `get_extraction_candidates` |
| **sequential-thinking calls** | 4 (1 probe + 3 validation thoughts) |
| **Input** | `docs/brain/EPIC-W7-065/02-architecture-plan.md`, `docs/brain/EPIC-W7-065/03-audit-report.md` |
| **Output** | `docs/brain/EPIC-W7-065/04-tickets.md` |
