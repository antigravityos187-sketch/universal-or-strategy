# EPIC-W7-065 · Wave 7 Phase 0 — Hotspot Analysis

| Field              | Value                                          |
|--------------------|------------------------------------------------|
| **Epic**           | EPIC-W7-065                                    |
| **Wave / Phase**   | 7 / 0                                          |
| **Method**         | `HandleFsmFilled`                              |
| **Source**         | `src/V12_002.Symmetry.BracketFSM.cs` (L349)   |
| **CYC Confirmed**  | **14**                                         |
| **Severity**       | 🔴 High (threshold: ≥10 = High, ≥15 = Very High) |
| **Date**           | 2025-07-11                                     |

---

## 1. Symbol Location

```
V12_002 (partial class)
  └── BracketFSM Logic (Actor Consumer)
        └── HandleFsmFilled(AccountEvent evt, FollowerBracketFSM fsm)
              Source: src/V12_002.Symmetry.BracketFSM.cs : 349–375
```

---

## 2. Cyclomatic Complexity Breakdown

**CYC = 14 (confirmed)**

McCabe counting: one base path + 13 independent decision points.

| # | Decision Point                                                            | Lines   |
|---|---------------------------------------------------------------------------|---------|
| 1 | `!IsNullOrEmpty(evt.SignalName)` — isStop guard                           | 352–353 |
| 2 | `StartsWith("Stop_")` — stop prefix                                       | 354     |
| 3 | `StartsWith("S_")` — short-circuit OR                                     | 354     |
| 4 | `!IsNullOrEmpty(evt.SignalName)` — isTarget guard                         | 355–356 |
| 5 | `StartsWith("T1_")`                                                       | 357     |
| 6 | `StartsWith("T2_")`                                                       | 358     |
| 7 | `StartsWith("T3_")`                                                       | 359     |
| 8 | `StartsWith("T4_")`                                                       | 360     |
| 9 | `StartsWith("T5_")`                                                       | 361     |
| 10| `if (isStop \|\| isTarget)` — outer branch                               | 365     |
| 11| `isStop \|\|` — short-circuit                                             | 365     |
| 12| `RemainingContracts <= 0 ? Filled : Active` — ternary                    | 368     |
| 13| `else if (fsm.State == Accepted \|\| fsm.State == Submitted)` — if       | 370     |
| 14| `Accepted \|\| Submitted` — short-circuit                                | 370     |

**9 of 14 decisions** are prefix-dispatch branches that could be collapsed into two boolean helper methods.

---

## 3. Method Responsibility Summary

`HandleFsmFilled` is called from [`ProcessBracketEvent`](../../../src/V12_002.Symmetry.BracketFSM.cs#L397)
on `OrderState.Filled` and `OrderState.PartFilled` events. It performs three distinct tasks:

1. **Signal classification** — determines whether the fill is on a Stop or a Target order by inspecting
   `evt.SignalName` prefix patterns (`Stop_`, `S_`, `T1_`–`T5_`).
2. **Contract accounting** — decrements `fsm.RemainingContracts` by `evt.FilledQty`, then transitions
   FSM state to `Filled` (fully closed) or `Active` (partially closed) based on the remaining count.
3. **Entry promotion** — detects that the *entry* order was filled (none of the above prefixes match)
   while the FSM is still in `Accepted`/`Submitted` state, and promotes it to `Active`.

These three responsibilities are entangled in a single method body, which drives the complexity.

---

## 4. Call-Chain & Blast Radius

```
OnBarUpdate (src/V12_002.BarUpdate.cs:262)
  └─ DrainAccountMailbox()              [BracketFSM.cs:88]
       └─ ProcessBracketEvent(evt)      [BracketFSM.cs:381]
            └─ HandleFsmFilled(evt, fsm) ◄── HOTSPOT [BracketFSM.cs:349]
```

**Downstream state consumers that depend on FSM correctness:**

| Consumer | File | Dependency |
|----------|------|------------|
| `GetFsmExpectedPosition` | `BracketFSM.cs:422` | Reads `fsm.State` + `fsm.EntryOrder.Quantity` |
| REAPER Audit | `REAPER.Audit.cs:404` | Calls `GetFsmExpectedPosition` as sole authority for follower expected qty |
| REAPER Stale-FSM cleanup | `REAPER.Audit.cs:418` | Terminates Active FSMs when broker is flat |
| Order Cleanup | `Orders.Management.Cleanup.cs:83` | `TryTerminateFollowerBracket` gated on FSM state |
| SIMA Shadow reconciliation | `SIMA.Shadow.cs:269` | Gates stop-sync on `fsm.State == Active` |
| `_followerBrackets` consumers | 16 source files | Read FSM state / `RemainingContracts` |

**Risk:** A bug in `HandleFsmFilled` (wrong state, wrong remaining-count) directly corrupts
`GetFsmExpectedPosition`, which is the **sole authority** (Build 1105) for follower expected position.
A corrupted expected position will cause REAPER to either suppress a needed repair or trigger
a spurious flatten — both are financially material.

---

## 5. Identified Risk Vectors

### RV-1 · Silent Misclassification (HIGH)
If `evt.SignalName` is `null` or does not match any known prefix, the entry-fill branch fires
(`else if Accepted || Submitted`). If a *stop/target* fill arrives with a malformed or missing
`SignalName`, it will skip contract accounting entirely and incorrectly promote state to `Active`
instead of reducing `RemainingContracts`. No warning is emitted.

### RV-2 · Double-guard Opacity (`Math.Max(0, …)` nested) (MEDIUM)
```csharp
fsm.RemainingContracts = Math.Max(0, fsm.RemainingContracts - Math.Max(0, evt.FilledQty));
```
The inner `Math.Max(0, evt.FilledQty)` silently swallows negative `FilledQty` values (which
indicate a data corruption upstream). A pre-condition assertion or log line would surface this.

### RV-3 · Missing `Replacing`/`Modifying` entry-fill guard (LOW-MEDIUM)
The `else if` entry promotion only guards `Accepted || Submitted`. A fill arriving on an FSM
in `Replacing` or `Modifying` state will fall through to the default (no transition), but
`RemainingContracts` is not updated either. This may leave the FSM in `Replacing` with an
incorrect contract count if the entry partial-fills during a replace cycle.

### RV-4 · No test coverage for partial-fill + partial-close path (MEDIUM)
`FSMActorTests.cs` exercises the happy path. The `PartFilled` → `Active` (partial close, count > 0)
→ subsequent `Filled` → `Filled` (full close) two-step is not confirmed covered per test review.

---

## 6. Refactor Recommendations (Phase 1+)

| Priority | Action | Expected CYC reduction |
|----------|--------|-----------------------|
| P1 | Extract `IsStopSignal(name)` and `IsTargetSignal(name)` helper predicates | −7 (remove 7 StartsWith branches) |
| P2 | Extract `HandleEntryFilled(fsm)` for the `else if` promotion path | −2 (separate concern + its guard) |
| P3 | Replace `Math.Max(0, evt.FilledQty)` with guarded assertion + log | 0 complexity, +observability |
| P4 | Add `Replacing`/`Modifying` state guard to entry-fill promotion path | 0 complexity, +safety |

Post-refactor estimated CYC: **≈4** (two helper-predicate calls + one outer if/else + one ternary).

---

## 7. Files In Scope for Next Phases

| File | Role |
|------|------|
| `src/V12_002.Symmetry.BracketFSM.cs` | Primary — `HandleFsmFilled`, `ProcessBracketEvent` |
| `src/V12_002.MetadataGuard.cs` | Pre-condition guard (`MetadataGuardFsmEvent`) |
| `src/V12_002.REAPER.Audit.cs` | Primary downstream consumer |
| `src/V12_002.SIMA.Shadow.cs` | FSM-state-gated shadow reconciliation |
| `src/V12_002.Orders.Management.Cleanup.cs` | Termination consumer |
| `tests/V12_Performance.Tests/Core/FSMActorTests.cs` | Test coverage reference |

---

## 8. Summary

`HandleFsmFilled` is the **#1 complexity hotspot** in the BracketFSM layer (CYC 14). Its complexity
is driven almost entirely by inline prefix-dispatch logic for signal classification (9/14 decisions).
The method is on the hot path of every broker fill event and is a direct input to REAPER's
position-authority calculation — making correctness critical and the current lack of separation of
concerns a meaningful technical risk.

**Recommended next phase:** targeted unit-test gap analysis (Phase 1) followed by
extract-method refactor for signal predicates (Phase 2).
