# EPIC-W7-043 — Phase 1: Scope Definition

## Single Method in Scope

This phase operates on exactly one **single method**:

| Field | Value |
|---|---|
| Method | `SymmetryGuardSubmitFollowerBracket` |
| File | `src/V12_002.Symmetry.Follower.cs` |
| Lines | 285–425 |

The scope boundary for this epic is defined as the body of `SymmetryGuardSubmitFollowerBracket` and the three private extraction targets it will produce. Nothing outside this boundary is modified, renamed, or restructured.

---

## Cyclomatic Complexity

| Metric | Value |
|---|---|
| CYC (task header / reported) | 0 (placeholder default at epic creation) |
| CYC (actual — per Phase 0 hotspot analysis) | ~10 |
| CYC target (post-refactor) | ≤ 8 |

The actual CYC of ~10 was established by the Phase 0 decision-point inventory (see [`00-hotspots.md`](00-hotspots.md)). The 10 counted decision points are:

1. `if (pos.BracketSubmitted) return` — line 287
2. `if (acct == null) return` — line 290
3. Ternary — `exitAction` Long vs Short — line 293
4. Ternary — `ocoId` OcoGroupId vs Ticks fallback — line 298
5. `for (targetNum = 1..5)` loop — line 324
6. `if (targetQty <= 0) continue` — line 327
7. `if (IsRunnerTarget) continue` — line 330
8. `if (targetPrice <= 0) continue` — line 337
9. `if (tNum >= 1 && tNum <= 5)` FSM array guard — line 388
10. Exit path through `ordersToSubmit` construction (implied)

The three planned extractions reduce orchestration complexity by −8 across the loop cluster, the FSM commit block, and the inline ternaries, producing a residual CYC ≈ 2 in the orchestrator shell.

---

## Callers

`SymmetryGuardSubmitFollowerBracket` has **2 call sites**, both located within the same file (`src/V12_002.Symmetry.Follower.cs`):

| Caller Method | Line | Path |
|---|---|---|
| `SymmetryGuardOnFollowerFill` | 62 | ANCHOR-01 pre-check path — master anchor already resolved at fill time |
| `SymmetryGuardTryResolveFollower` | 230 | Normal deferred-resolve path — bracket not yet submitted |

**Callers count: 2** — both are internal to the same partial class file. No cross-file call sites exist. The method signature is `private void`, which confirms no external consumers exist and that signature changes carry zero cross-file risk.

---

## Why Other Methods Are NOT in Scope (V12.23)

Per the V12.23 rule, methods that share infrastructure with `SymmetryGuardSubmitFollowerBracket` but are not themselves the target method are explicitly excluded from this epic's scope boundary. This includes:

- `SymmetryGuardOnFollowerFill` (line 62 caller) — orchestrates fill events; refactoring it would expand the blast radius across the entire fill-dispatch pipeline.
- `SymmetryGuardTryResolveFollower` (line 230 caller) — owns the deferred-resolve decision tree; changes here risk breaking the `PendingSubmit → Submitted` FSM transition protocol across `Symmetry.BracketFSM.cs` and the 16 impacted files catalogued in Phase 0.
- All methods touching `_followerBrackets`, `FollowerBracketFSM`, `GetTargetOrdersDictionary`, `ValidateStopPrice`, `GetTargetContracts`, and `GetTargetPrice` — these share blast surface but have no complexity driver inside this epic's target method.

V12.23 mandates single-method scope containment: one CYC-violating method is isolated, decomposed via private extractions within the same file, and validated before any adjacent method is touched. Multi-method scope expansion violates the wave's risk budget.

---

## Planned Extractions (Phase 2 targets)

These three extractions will be implemented in Phase 2 and remain within the scope boundary (same file, `private` visibility):

| # | Extraction Name | Source Lines | CYC Reduction |
|---|---|---|---|
| 1 | `BuildFollowerStopOrder` | 293–316 | −1 |
| 2 | `BuildFollowerTargetOrders` | 318–372 | −4 |
| 3 | `CommitFollowerBracketFSM` | 376–413 | −3 |

---

## Agent Tracking

| Field | Value |
|---|---|
| Agent Name | v12-phase1-scope |
| Wave | 7 |
| Phase | 1 |
| Epic | EPIC-W7-043 |
| Source File | `src/V12_002.Symmetry.Follower.cs` |
| Output | `docs/brain/EPIC-W7-043/00-scope.md` |
| Bobcoins Used | 1.0 |
