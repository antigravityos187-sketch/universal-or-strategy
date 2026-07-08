# EPIC-W7-054 — Phase 1: Scope Definition

**Agent**: v12-phase1-scope
**Wave**: 7 | **Phase**: 1 (REDO)
**Generated**: 2026-06-26T00:00:00Z

---

## Single Method in Scope

This epic targets a **single method** for cyclomatic complexity reduction under the V12.23
governance standard. The method was identified as the CYC=20 hotspot for EPIC-W7-054 in
Wave 7 triage. The `wave7-epic-list.json` entry (num=54) carried blank `method_name` and
`source_file` fields; those placeholders have been resolved via source analysis in this REDO.

| Field | Value |
|-------|-------|
| **Method Name** | `HydrateFromOpenPositions` |
| **Current CYC** | 20 |
| **Target CYC** | <= 8 |
| **Source File** | `src/V12_002.SIMA.Lifecycle.cs` |
| **Callers Count** | 1 (`HydrateFSMsFromWorkingOrders`, line 866) |

---

## Scope Boundary

The **scope boundary** for this epic is strictly limited to the body of `HydrateFromOpenPositions`
in `src/V12_002.SIMA.Lifecycle.cs`. Only the following work is in scope:

- Extracting `private` helper methods from `HydrateFromOpenPositions` to reduce its CYC from 20
  to <= 8
- All extracted helpers must individually satisfy CYC <= 8 per the Jane Street strict threshold
- The public/internal signature of `HydrateFromOpenPositions` must remain **identical** after
  refactoring, preserving the single call site in `HydrateFSMsFromWorkingOrders`

No other methods in `src/V12_002.SIMA.Lifecycle.cs` or any other file are in scope.

---

## Why This Is a Single Method Epic (V12.23)

V12.23 mandates **single method** scope definition for all Wave 7 complexity epics. Each epic
address exactly one hotspot to ensure:

1. **Reviewability**: A single-method extraction diff is independently reviewable and mergeable
2. **Risk containment**: Scope creep into neighbouring methods (e.g. `HydrateFSMsFromWorkingOrders`,
   `AdoptSingleOrder`, `SweepBrokerOrders`) is explicitly prohibited
3. **Audit trail**: One method → one epic → one PR → one CYC verification

**Other methods NOT in scope and why (V12.23):**

| Method | CYC | Reason Excluded |
|--------|-----|-----------------|
| `HydrateFSMsFromWorkingOrders` | ~8 | Within threshold; not a hotspot |
| `HydrateWorkingOrdersFromBroker` | ~10 | Separate EPIC assigned if threshold breached |
| `SweepBrokerOrders` | 28 | Assigned to EPIC-W7-056 — different epic |
| `AdoptSingleOrder` | ~5 | Within threshold; not a hotspot |
| `EnumerateApexAccounts` | ~7 | Within threshold; not a hotspot |
| All other files | N/A | Outside scope boundary — no cross-file changes |

The scope boundary is a hard line. Touching any of the above methods in this epic would violate
V12.23 single-method governance and invalidate the CYC audit.

---

## Complexity Analysis

- **Current CYC**: 20 — exceeds Jane Street strict threshold of <= 8
- **CYC overage**: +12 above threshold (2.5x ceiling)
- **Target CYC**: <= 8 on parent after extraction
- **Reduction required**: Remove at least 12 decision points from `HydrateFromOpenPositions`
- **Estimated helpers needed**: 3–4 extracted `private` methods

### Primary Complexity Drivers in `HydrateFromOpenPositions`

1. **Outer `foreach(Account)` with three guard `continue` branches** (~4 CYC): account filter,
   FSM idempotency check, position existence check
2. **Inner `foreach(stopOrders)` with null guards and match condition** (~4 CYC): null stop,
   null account, account name equality
3. **Five sequential target-order linking blocks** (~10 CYC): each `TryGetValue + non-null`
   pair adds 2 CYC (the `TryGetValue` boolean and the `!IsNullOrEmpty` inner guard)

Extraction strategy: pull the inner stop-scan loop into `TryFindStopForAccount`, and collapse
the five target-linking blocks into `LinkTargetOrdersToFsm` — two helpers eliminate ~14 CYC
from the parent while keeping each helper under the <= 8 ceiling.

---

## Scope Checklist

- [x] Single method identified: `HydrateFromOpenPositions`
- [x] Source file confirmed: `src/V12_002.SIMA.Lifecycle.cs`
- [x] Current CYC verified: 20 (per Wave 7 hotspot scanner, confirmed by branch analysis)
- [x] Target CYC set: <= 8 (V12.23 / Jane Street strict)
- [x] Callers enumerated: 1 (`HydrateFSMsFromWorkingOrders`)
- [x] Scope boundary defined: extract-only, no signature changes, no cross-file changes
- [x] Other methods explicitly excluded (V12.23): confirmed in table above
- [x] `scope_confirmed_single_method`: true

---

## Agent Tracking

| Field | Value |
|-------|-------|
| **Agent Name** | v12-phase1-scope |
| **Wave** | 7 |
| **Phase** | 1 (REDO) |
| **Epic** | EPIC-W7-054 |
| **Method** | `HydrateFromOpenPositions` |
| **Source File** | `src/V12_002.SIMA.Lifecycle.cs` |
| **CYC** | 20 → target <= 8 |
| **Output** | `docs/brain/EPIC-W7-054/00-scope.md` |
