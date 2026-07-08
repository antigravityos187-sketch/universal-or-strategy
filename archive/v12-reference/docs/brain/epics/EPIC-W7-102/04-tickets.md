# Phase 4: Implementation Tickets — EPIC-W7-102

**Agent:** v12-phase4-tickets
**Wave:** 7 | **Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T22:40:00Z
**Input:** docs/brain/EPIC-W7-102/02-architecture-plan.md, docs/brain/EPIC-W7-102/03-audit-report.md

---

## Epic Summary

- **Method:** `ProcessBracketEvent` — `src/V12_002.Symmetry.BracketFSM.cs`
- **Original cyc:** 14 (index measurement includes HandleFsmFilled inline complexity)
- **Max cyc projected:** 6 (ProcessBracketEvent residual after extraction)
- **Extraction count:** 3
- **ticket_count:** 3 (ticket_count=3)
- **DNA verdict:** PASS

The actual complexity carrier is `HandleFsmFilled` (7 `StartsWith` comparisons + contract math +
state branch). `ProcessBracketEvent` itself is already a clean dispatcher that requires no body
changes. All extraction targets are private to `src/V12_002.Symmetry.BracketFSM.cs`.

---

## Ticket T1 — Introduce FillSignalKind Enum

**ID:** EPIC-W7-102-T1
**Type:** extraction (type introduction)
**Priority:** P0 — dependency for T2 and T3

### Description

Declare a `private enum FillSignalKind { Entry, Stop, Target }` inside
`src/V12_002.Symmetry.BracketFSM.cs`. This value type replaces the stringly-typed fill signal
classification currently spread across 7 `string.StartsWith` comparisons in `HandleFsmFilled`.
It makes illegal signal classification states unrepresentable by design (Jane Street principle:
illegal states unrepresentable).

### Acceptance Criteria

- [ ] `private enum FillSignalKind { Entry, Stop, Target }` exists in `V12_002.Symmetry.BracketFSM.cs`
- [ ] Enum is private and nested within the partial class scope (no public API surface change)
- [ ] Enum is a value type — zero heap allocation at runtime
- [ ] `dotnet build` passes with zero errors after this change alone
- [ ] No `lock()` blocks introduced
- [ ] ASCII-only identifiers

### Implementation Steps

1. Open `src/V12_002.Symmetry.BracketFSM.cs`
2. Locate the partial class declaration body
3. Add the following immediately before or after `HandleFsmFilled`:
   ```csharp
   private enum FillSignalKind { Entry, Stop, Target }
   ```
4. Run `dotnet build` — verify zero errors
5. Run `dotnet csharpier check src/` — fix any formatting issues

---

## Ticket T2 — Extract ClassifyFillSignalType Static Helper

**ID:** EPIC-W7-102-T2
**Type:** extraction (static method)
**Priority:** P1 — depends on T1, dependency for T3
**Projected cyc:** 4

### Description

Extract `private static FillSignalKind ClassifyFillSignalType(string signalName)` from the
inline 7-arm `StartsWith` chain currently embedded in `HandleFsmFilled`. This helper isolates
all signal parsing logic into a single-responsibility pure function. It is `private static`
because it requires no instance state — zero-allocation, no lock, no FSM mutation.

### Acceptance Criteria

- [ ] `private static FillSignalKind ClassifyFillSignalType(string signalName)` declared in same file
- [ ] Method contains all 7 `StartsWith` comparisons (or equivalent prefix checks) covering Stop, Target, and Entry patterns
- [ ] Returns `FillSignalKind.Stop`, `FillSignalKind.Target`, or `FillSignalKind.Entry` — no string returns
- [ ] Method is `private static` with no instance field access
- [ ] cyc of this method is 4 or below (verified by `python scripts/complexity_audit.py`)
- [ ] `HandleFsmFilled` calls `ClassifyFillSignalType(evt.SignalName)` to get `FillSignalKind kind`
- [ ] `dotnet build` passes with zero errors
- [ ] No `lock()` blocks introduced

### Implementation Steps

1. Read the current `HandleFsmFilled` body (lines ~349–380 in `src/V12_002.Symmetry.BracketFSM.cs`)
2. Identify all `evt.SignalName.StartsWith(...)` comparisons — map each prefix to a `FillSignalKind` value
3. Add the extracted method:
   ```csharp
   private static FillSignalKind ClassifyFillSignalType(string signalName)
   {
       if (signalName.StartsWith("STOP", StringComparison.OrdinalIgnoreCase)
           || signalName.StartsWith("SL", StringComparison.OrdinalIgnoreCase))
           return FillSignalKind.Stop;
       if (signalName.StartsWith("TARGET", StringComparison.OrdinalIgnoreCase)
           || signalName.StartsWith("PT", StringComparison.OrdinalIgnoreCase)
           || signalName.StartsWith("TP", StringComparison.OrdinalIgnoreCase))
           return FillSignalKind.Target;
       return FillSignalKind.Entry;
   }
   ```
   *(Adjust prefix strings to match the exact StartsWith comparisons found in the existing HandleFsmFilled body.)*
4. Replace the inline prefix comparisons in `HandleFsmFilled` with a single call:
   `FillSignalKind kind = ClassifyFillSignalType(evt.SignalName);`
5. Run `dotnet build` — verify zero errors
6. Run `python scripts/complexity_audit.py` — verify `ClassifyFillSignalType` cyc ≤ 4

---

## Ticket T3 — Extract ApplyFillStateTransition Instance Helper

**ID:** EPIC-W7-102-T3
**Type:** extraction (instance method + HandleFsmFilled refactor)
**Priority:** P2 — depends on T1 and T2
**Projected cyc (helper):** 3
**Projected cyc (HandleFsmFilled post-extraction):** 3

### Description

Extract `private void ApplyFillStateTransition(FollowerBracketFSM fsm, FillSignalKind kind, int filledQty)`
from `HandleFsmFilled`. This helper encapsulates the contract delta calculation and FSM state
mutation for fill events (Stop and Target legs). After extraction, `HandleFsmFilled` becomes a
thin coordinator delegating to `ClassifyFillSignalType` and `ApplyFillStateTransition`, reducing
its cyc from ~7 to 3.

### Acceptance Criteria

- [ ] `private void ApplyFillStateTransition(FollowerBracketFSM fsm, FillSignalKind kind, int filledQty)` declared in same file
- [ ] Method handles contract delta + FSM state mutation for Stop and Target fill kinds
- [ ] Method is `private` (non-static — accesses `fsm` parameter for mutation)
- [ ] cyc of `ApplyFillStateTransition` is 3 or below
- [ ] `HandleFsmFilled` post-refactor matches the architecture plan shape:
  ```csharp
  private void HandleFsmFilled(AccountEvent evt, FollowerBracketFSM fsm)
  {
      FillSignalKind kind = ClassifyFillSignalType(evt.SignalName);
      if (kind == FillSignalKind.Stop || kind == FillSignalKind.Target)
      {
          ApplyFillStateTransition(fsm, kind, Math.Max(0, evt.FilledQty));
      }
      else if (fsm.State == FollowerBracketState.Accepted || fsm.State == FollowerBracketState.Submitted)
      {
          fsm.State = FollowerBracketState.Active;
      }
  }
  ```
- [ ] `HandleFsmFilled` cyc = 3 (verified by `python scripts/complexity_audit.py`)
- [ ] `ProcessBracketEvent` cyc = 6 (unchanged — dispatcher body not modified)
- [ ] `dotnet build` passes with zero errors
- [ ] `dotnet csharpier format src/` applied, zero formatting issues
- [ ] No `lock()` blocks introduced
- [ ] `python scripts/complexity_audit.py` reports all new symbols ≤ 5

### Implementation Steps

1. Read the current `HandleFsmFilled` body after T2 is applied
2. Identify the contract delta + state mutation block triggered when `kind == Stop || kind == Target`
3. Add the extracted helper:
   ```csharp
   private void ApplyFillStateTransition(FollowerBracketFSM fsm, FillSignalKind kind, int filledQty)
   {
       fsm.ContractDelta += filledQty;
       fsm.State = kind == FillSignalKind.Stop
           ? FollowerBracketState.StopFilled
           : FollowerBracketState.TargetFilled;
   }
   ```
   *(Adjust field names and state values to match the actual HandleFsmFilled body.)*
4. Replace the inline block in `HandleFsmFilled` with a call to `ApplyFillStateTransition`
5. Apply the full `HandleFsmFilled` refactor per architecture plan shape above
6. Run `dotnet build` — verify zero errors
7. Run `dotnet csharpier format src/` — apply formatting
8. Run `python scripts/complexity_audit.py` — verify:
   - `HandleFsmFilled` cyc ≤ 5
   - `ApplyFillStateTransition` cyc ≤ 5
   - `ClassifyFillSignalType` cyc ≤ 5
   - `ProcessBracketEvent` cyc = 6 (unchanged)

---

## Execution Order

| Order | Ticket | Type | Depends On |
|-------|--------|------|------------|
| 1 | T1 — FillSignalKind enum | Type introduction | none |
| 2 | T2 — ClassifyFillSignalType | Static method extraction | T1 |
| 3 | T3 — ApplyFillStateTransition + HandleFsmFilled refactor | Instance method extraction | T1, T2 |

---

## Final Verification Gate (Phase 5.V)

After all 3 tickets complete, the Phase 5.V verifier MUST confirm:

- `python scripts/complexity_audit.py` — max cyc across all touched symbols = 6
- `dotnet build` — zero errors
- `grep -n "lock(" src/V12_002.Symmetry.BracketFSM.cs` — zero new lock() blocks
- `ProcessBracketEvent` body unchanged from pre-epic state
- All 3 extractions present: `FillSignalKind`, `ClassifyFillSignalType`, `ApplyFillStateTransition`
- ASCII-only: `grep -P "[\x80-\xFF]" src/V12_002.Symmetry.BracketFSM.cs` — zero matches
