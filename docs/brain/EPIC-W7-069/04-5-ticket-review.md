# EPIC-W7-069 — Phase 4.5 Ticket Review

| Field | Value |
|---|---|
| **Epic** | EPIC-W7-069 |
| **Method** | `GetFsmExpectedPosition` |
| **CYC (current)** | 14 (modified Lizard) / 7 (McCabe) |
| **Source File** | `src/V12_002.Symmetry.BracketFSM.cs` |
| **Wave** | 7 |
| **Phase** | 4.5 — Jane Street Ticket Validation Gate |
| **Reviewer Agent** | v12-phase4-5-review |

---

## Per-Ticket Verdict Table

| Ticket | Title | CYC Projected | CYC<=8 | Single-Resp | No Locks | Illegal States | Actionable | Verdict |
|---|---|---|---|---|---|---|---|---|
| T1 | Extract `IsActiveFollowerState` | 2 | PASS | PASS | PASS | PASS | PASS | **PASS** |
| T2 | Extract `ComputeEntrySignedQuantity` | 3 | PASS | PASS | PASS | PASS | PASS | **PASS** |
| T3 | Rewrite `GetFsmExpectedPosition` body | 7 | PASS | PASS | PASS | PASS | PASS | **PASS** |
| T4 | Build verification + CYC audit | N/A | N/A | PASS | PASS | N/A | PASS | **PASS** |

---

## Per-Ticket Detail

### T1 — Extract `IsActiveFollowerState`

- **CYC<=8**: Projected CYC=2. Switch expression with 6 `or`-pattern states merged into one `true` arm plus `_ => false` default. McCabe = base(1) + arm(1) = 2. PASS.
- **Single-responsibility**: Classifies whether a `FollowerBracketState` is non-terminal. One concern, no side effects, pure function. PASS.
- **No locks**: Explicitly stated in notes and validated by grep acceptance criterion. PASS.
- **Illegal states unrepresentable**: Exhaustive switch with `_ => false` default ensures all undeclared/future states are safely classified as inactive. PASS.
- **Actionable**: Provides exact signature, exact body, `[MethodImpl(MethodImplOptions.AggressiveInlining)]` decorator, placement instruction, and 10 binary acceptance criteria. PASS.

### T2 — Extract `ComputeEntrySignedQuantity`

- **CYC<=8**: Projected CYC=3. Base=1, ternary condition `Buy || BuyToCover` = +2 boolean sub-expressions. Total=3. PASS.
- **Single-responsibility**: Computes sign and multiplies by quantity. Pure arithmetic, no instance state, no I/O, no allocation. PASS.
- **No locks**: Explicitly stated. Acceptance criterion includes zero-new-lock grep. PASS.
- **Illegal states unrepresentable**: Sign defaults to -1 for all non-buy `OrderAction` values — covers unknown/future enum members safely. Null guard is caller-contract (documented). PASS.
- **Actionable**: Provides exact signature, exact body, caller-contract documentation, `AggressiveInlining` requirement, CYC rationale, and 10 binary acceptance criteria. PASS.

### T3 — Rewrite `GetFsmExpectedPosition` Body

- **CYC<=8**: Projected CYC=7. Breakdown: base=1, foreach=+1, null/account guard=+2, `IsActiveFollowerState` if=+1, `EntryOrder!=null`=+1, `else if Active`=+1. Total=7. max_cyc_projected=7<=8. PASS.
- **Single-responsibility**: Aggregation-only pass — iterates FSMs, filters via T1 helper, delegates arithmetic to T2 helper, accumulates sum. No inline state classification or sign logic. PASS.
- **No locks**: Notes "No lock() added." Acceptance criterion verifies zero new lock blocks. PASS.
- **Illegal states unrepresentable**: Null guards are explicit (`f == null`, `f.EntryOrder != null`). State filter delegates to exhaustive switch in T1. The `else if (f.State == Active)` no-op branch is a documented intentional path with an ASCII comment — not a hidden illegal state. PASS.
- **Actionable**: Provides exact replacement body, CYC breakdown table, lists removed logic, references source line ~422, and 10 binary acceptance criteria. PASS.

### T4 — Build Verification and CYC Audit

- **CYC<=8**: N/A — no code written.
- **Single-responsibility**: Verification only. Six ordered steps validating build, formatting, lock count, complexity, deploy sync, and pre-push gates. PASS.
- **No locks**: No code changes. Step 3 explicitly checks that lock count is unchanged. PASS.
- **Illegal states unrepresentable**: N/A — verification ticket.
- **Actionable**: Exact bash/PowerShell commands, expected outputs per step, dependency on T3 declared, and 10 binary acceptance criteria. PASS.

---

## Dependency Chain Validation

```
T1 (IsActiveFollowerState extraction)  --\
                                           +--> T3 (parent rewrite) --> T4 (build verify)
T2 (ComputeEntrySignedQuantity extraction) --/
```

- T1 and T2 are independent: PASS (no shared state, no file conflict, sequential application in same file is safe).
- T3 depends on T1 and T2: PASS (calls both helpers; will not compile without them).
- T4 depends on T3: PASS (verification-only; runs after all code changes are complete).

---

## Jane Street KB Rules Summary

| Rule | T1 | T2 | T3 | T4 |
|---|---|---|---|---|
| CYC <= 8 (Jane Street strict) | 2 PASS | 3 PASS | 7 PASS | N/A |
| Single responsibility | PASS | PASS | PASS | PASS |
| No lock() blocks | PASS | PASS | PASS | PASS |
| Illegal states unrepresentable | PASS | PASS | PASS | N/A |
| AggressiveInlining hot path | PASS | PASS | N/A | N/A |
| ASCII-only identifiers | PASS | PASS | PASS | PASS |
| Zero allocation / no LINQ | PASS | PASS | PASS | N/A |

---

## Overall Review

```
review_verdict: PASS
failed_tickets: []
max_cyc_projected: 7
extraction_count: 2
ticket_count: 4
```

All 4 tickets satisfy Jane Street KB rules. The extraction plan is safe, specific, and fully actionable for v12-engineer execution. Phase 5 (ticket execution) may proceed.
