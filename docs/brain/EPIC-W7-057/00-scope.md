# EPIC-W7-057 — Phase 1: Scope Definition

## Single Method in Scope

This epic targets a **single method**: `SymmetryGuardTryResolveFollower`.

| Attribute        | Value                                      |
|------------------|--------------------------------------------|
| **Method**       | `SymmetryGuardTryResolveFollower`          |
| **Source File**  | `src/V12_002.Symmetry.Follower.cs`         |
| **Current CYC**  | 12                                         |
| **Target CYC**   | ≤ 8                                        |
| **Callers**      | 3 call sites across 2 files                |

### Caller Inventory

| Call Site                                    | File                                  |
|----------------------------------------------|---------------------------------------|
| `SymmetryGuardTryResolveFollowersForDispatch` | `src/V12_002.Symmetry.Replace.cs:187` |
| Intra-file call (line 84)                    | `src/V12_002.Symmetry.Follower.cs:84` |
| Intra-file call (line 122)                   | `src/V12_002.Symmetry.Follower.cs:122`|

---

## Scope Boundary

The **scope boundary** is strictly limited to `SymmetryGuardTryResolveFollower` in
[`src/V12_002.Symmetry.Follower.cs`](../../src/V12_002.Symmetry.Follower.cs). No other method
body will be modified as a primary refactor target in this phase. Extraction helpers introduced
to reduce CYC (e.g. `CanResolveFollowerBracket`, `MatchFollowerToMasterFill`) are new symbols —
they do not constitute modifications to methods outside the scope boundary.

---

## Why Other Methods Are NOT in Scope (V12.23)

Per **V12.23** (single-method atomicity rule): each phase-1 scope document governs exactly one
God-method. The two companion methods — `SymmetryGuardSubmitFollowerBracket` (CYC=12) and
`SymmetryGuardOnFollowerFill` (CYC=11) — are documented in `00-hotspots.md` as part of the same
cluster but are explicitly excluded from this scope for the following reasons:

1. **Separate call graphs.** `SymmetryGuardSubmitFollowerBracket` is called exclusively by
   `SymmetryGuardTryResolveFollower` (intra-file). Merging both into one refactor wave
   violates the V12.23 requirement to keep blast radius to a single caller graph per scope.

2. **Independent complexity drivers.** `SymmetryGuardSubmitFollowerBracket`'s CYC is driven
   by direction × mode price-calculation branching — a distinct extraction task that must be
   sequenced *after* the guard-predicate extractions in the lead method are stable and tested.

3. **Risk sequencing.** `SymmetryGuardOnFollowerFill` drives FSM advancement and contains a
   `try/catch` structural branch. Extracting `AdvanceFollowerBracketFsm` before the upstream
   resolution method is simplified would make it harder to verify FSM correctness end-to-end.
   V12.23 mandates sequential, independently-validatable scopes.

4. **Separate EPIC assignments.** The cluster's phase phasing means each method will receive its
   own EPIC ticket in subsequent wave iterations once EPIC-W7-057 refactor is validated.

---

## Complexity Reduction Plan

`SymmetryGuardTryResolveFollower` carries CYC=12, broken down as:

- ~7 CYC from FSM state fan-out (`Active`, `Accepted`, `Submitted`, `Replacing`) and compound
  null-guard chain (`follower != null`, `follower.AccountName != null`).
- ~5 CYC from early-return guard predicates (no master position, no dispatch, mismatched
  instrument) and intra-method branching.

**Planned extractions (residual CYC target ≤ 8, preferably ≤ 5):**

| Helper                        | Removes (approx.) | Nature               |
|-------------------------------|-------------------|----------------------|
| `CanResolveFollowerBracket`   | 3–4 CYC           | guard predicate      |
| `MatchFollowerToMasterFill`   | 3–4 CYC           | FSM state fan-out    |

Both helpers are pure-ish (read-only on shared state) and independently unit-testable, satisfying
the V12.23 "observable behaviour unchanged" constraint.

---

## Risk Notes

- **Threading constraint:** `_followerBrackets` (ConcurrentDictionary) must not be mutated during
  enumeration on the strategy thread. Extracted helpers must not introduce new enumeration sites.
- **Irreversible side-effect boundary:** `SymmetryGuardSubmitFollowerBracket` (called from within
  this method) touches `SubmitOrderUnmanaged`. The scope boundary ensures this call site is not
  accidentally moved or duplicated during extraction.
- **Blast radius:** 3 call sites; all on the strategy thread. Signature changes require updates
  at all 3 sites simultaneously.

---

## Agent Tracking

| Field             | Value                    |
|-------------------|--------------------------|
| **Agent Name**    | v12-phase1-scope         |
| **Epic**          | EPIC-W7-057              |
| **Wave**          | 7                        |
| **Phase**         | 1 — Scope Definition     |
| **Bobcoins Used** | 1.0                      |
