# EPIC-W7-049 — Phase 1: Scope Definition

---

## Method in Scope

| Field              | Value                                        |
|--------------------|----------------------------------------------|
| **Method**         | `ManageTrail_RunPerTradeBranches`            |
| **File**           | `src/V12_002.Trailing.cs`                    |
| **Lines**          | 240–255                                      |
| **Current CYC**    | 11                                           |
| **Target CYC**     | ≤ 8                                          |
| **CYC Reduction**  | ≥ 3 points                                   |

This epic targets a **single method**: `ManageTrail_RunPerTradeBranches`. The scope
boundary is drawn precisely at this method's declaration — no surrounding logic, no
callee bodies, and no sibling dispatcher methods are included in the refactor surface.

---

## Caller Count

A `grep` of `src/` for the symbol `ManageTrail_RunPerTradeBranches` returns **2 matches**
in `src/V12_002.Trailing.cs`:

- **Line 240** — method definition (`private bool ManageTrail_RunPerTradeBranches(...)`)
- **Line 71** — the single call site inside `ManageTrailingStops`

**Caller count: 1** (`ManageTrailingStops`, same file, line 71).

The method is called once per active position per throttle tick. No other file in `src/`
calls or references this symbol, confirming a fully contained blast radius for the
refactor.

---

## Complexity Drivers (from Phase 0)

The CYC-11 reading is produced by three compounding factors:

1. **Compound flag guards with `!IsRMATrade` negation repeated on every branch** — each
   of the three dispatch arms ANDs two or three `PositionInfo` boolean fields with a
   `!pos.IsRMATrade` exclusion, contributing 6+ independent branch edges under McCabe
   counting.
2. **Sequential `if` chain with implicit fall-through** — the three guards are evaluated
   in order with no `else` linkage, creating three independent decision nodes instead of
   a single dispatch table.
3. **RMA exclusion evaluated three times instead of once** — the same `!pos.IsRMATrade`
   predicate participates in three separate branch points rather than being factored out
   as a single early-exit guard.

---

## Planned Refactors (≤ 2, per Phase 0 recommendation)

| # | Technique                               | CYC Reduction |
|---|-----------------------------------------|---------------|
| 1 | Extract `IsEMATradeCandidate(pos)` predicate consolidating `!IsRMATrade` | ~4 pts |
| 2 | Add early `if (pos.IsRMATrade) return false;` guard, remove per-branch negation | ~2–3 pts |

Combined expected CYC after both refactors: **≤ 8** (target met).

---

## Scope Boundary

The **scope boundary** for EPIC-W7-049 is the body of `ManageTrail_RunPerTradeBranches`
(lines 240–255, `src/V12_002.Trailing.cs`) plus any private predicate helper extracted
from it in the same file. Explicitly **excluded** from scope:

- `TrailHandler_TREND_E1` (line 257)
- `TrailHandler_TREND_E2` (line 312)
- `TrailHandler_RETEST` (line 342)
- `ManageTrailingStops` (line 71 caller)
- `ManageTrail_RunPointBasedTrailing`
- All `PositionInfo` flag field definitions

---

## Why Other Methods Are NOT in Scope

This project operates under **V12.23** conventions: refactors are constrained to the
minimum surface required to achieve the CYC target for the nominated method. Under V12.23,
a phase-scoped epic addresses exactly one nominated method per phase — expanding scope to
callee handlers or sibling methods would breach the single-method constraint, risk
unintended side-effects across the 15 files that share `PositionInfo` flag fields, and
invalidate the blast-radius analysis performed in Phase 0.

The three `TrailHandler_*` callees are already independent methods with well-defined
responsibilities and acceptable CYC; they require no refactoring to meet the EPIC-W7-049
objective. Touching them would constitute scope creep and is explicitly out of scope under
V12.23 rules.

---

## Summary

- **Single method** in scope: `ManageTrail_RunPerTradeBranches`
- Current CYC **11** → target CYC **≤ 8**
- **1 caller** (`ManageTrailingStops`, same file)
- **Scope boundary**: method body + any private predicate helper extracted within the same file
- **V12.23 constraint**: single method per phase; all other methods excluded

---

## Agent Tracking

| Field            | Value                   |
|------------------|-------------------------|
| **Agent Name**   | v12-phase1-scope        |
| **Epic**         | EPIC-W7-049             |
| **Wave / Phase** | 7 / 1                   |
| **Bobcoins Used**| 1.0                     |
| **CYC Confirmed**| 11 (target ≤ 8)         |
| **Output**       | `00-scope.md`           |
