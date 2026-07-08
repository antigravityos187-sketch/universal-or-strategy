# EPIC-W7-067 — Phase 1: Scope Definition

## Method in Scope

**Single method:** `SymmetryFindDispatchForMasterFill`
**File:** `src/V12_002.Symmetry.cs` (lines 326–352)
**Current CYC:** 8
**Target CYC:** ≤ 8 (hold-the-line; no reduction required, no increase permitted)

---

## Scope Boundary

The scope boundary for EPIC-W7-067 Phase 1 is strictly confined to a single method:
`SymmetryFindDispatchForMasterFill` in `src/V12_002.Symmetry.cs`. No other method, file, or
subsystem falls within this phase's scope boundary. Every analysis, planning artefact, and
refactor decision produced in this phase applies to that single method exclusively.

---

## Caller Count

A grep of `src/` for the symbol `SymmetryFindDispatchForMasterFill` returns **2 matches**:

| Match | File | Line | Role |
|---|---|---|---|
| Call site | `src/V12_002.Symmetry.cs` | 283 | Invocation inside `SymmetryGuardOnMasterFill` |
| Definition | `src/V12_002.Symmetry.cs` | 326 | Method declaration |

**Total callers: 1** (`SymmetryGuardOnMasterFill`, same file, line 283).

The full call chain is:
```
ValidateAndPrepareEntryFill          (src/V12_002.Orders.Callbacks.cs:368)
  └─► SymmetryGuardOnMasterFill      (src/V12_002.Symmetry.cs:283)
        └─► SymmetryFindDispatchForMasterFill   ← SCOPE TARGET
```

There is exactly one caller. No other code path invokes the method directly.

---

## Why Other Methods Are NOT in Scope

Per **project standard V12.23**, a Wave-7 EPIC scopes to a single method when:

1. The nominated method's CYC falls at or below the threshold (≤ 8), meaning no structural
   decomposition into child helpers is mandated by the complexity ceiling.
2. The blast-radius analysis (Phase 0, `00-hotspots.md`) confirms that all shared-state
   interactions are read-only snapshots (`ToArray()`) or guarded by `Volatile.Read` (ADR-019),
   so no callee needs to be co-refactored to preserve correctness.
3. The sole caller (`SymmetryGuardOnMasterFill`) drives a CAS-loop that consumes this method's
   return value without contributing to its internal branching; changing the caller is therefore
   out of scope for a CYC-reduction task.
4. Helper `SymmetryNormalizeTradeType` (`src/V12_002.Symmetry.Replace.cs:322`) is called as an
   input normalizer but lives in a separate file and is not modified here; modifying it would
   exceed the single-method scope boundary and risk de-stabilizing the Replace subsystem.

**V12.23 mandates single-method scope when CYC ≤ 8 and blast radius is fully contained.**
Both conditions are satisfied here; therefore all surrounding methods remain out of scope.

---

## Phase 1 Work Item (from Hotspot Analysis)

The one structural improvement flagged for this phase is:

> **Make the `symmetryMasterEntryToDispatch` pre-mapping mandatory at dispatch-time**, so that
> `SymmetryFindDispatchForMasterFill` is only reached as a defensive fallback and never executes
> on the latency-sensitive hot fill-callback path.

This eliminates the `ConcurrentDictionary.ToArray()` heap allocation for all normal fills without
touching the method's CYC or selection semantics (the oldest-wins `CreatedUtc` policy in H-11
remains intact).

---

## CYC Budget

| Metric | Value |
|---|---|
| Current CYC | 8 |
| Project ceiling (V12.23) | 8 |
| Target after Phase 1 | ≤ 8 |
| Permitted net increase | 0 |

The single method sits exactly at the project ceiling. Phase 2 (Refactor Implementation) must not
introduce any new decision point. Extraction of the four skip-predicates into named helpers is
**not** recommended (see `00-hotspots.md` § Recommended Extraction Count) because the predicates
are ordering-constrained and clearer in-line.

---

## Out-of-Scope Items (Explicit Exclusions)

- `SymmetryGuardOnMasterFill` — sole caller; not modified in this EPIC.
- `SymmetryNormalizeTradeType` — input normalizer in a separate file; not modified.
- `ValidateAndPrepareEntryFill` — upstream caller in `src/V12_002.Orders.Callbacks.cs`; not modified.
- `symmetryDispatchById` dictionary — shared state; its type and access pattern are not changed.
- `SymmetryDispatchTtl` constant — TTL value is not adjusted.
- All other methods in `src/V12_002.Symmetry.cs` — out of scope per V12.23 single-method rule.

---

## Agent Tracking

```
Agent Name:      v12-phase1-scope
Epic:            EPIC-W7-067
Wave:            7
Phase:           1 — Scope Definition (REDO)
Bobcoins Used:   1.0
Execution Time:  ~90s
Timestamp:       2026-06-26T02:35:31Z
```
