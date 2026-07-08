# EPIC-W7-047 — Phase 1: Scope Definition

---

## Single Method in Scope

This epic targets exactly one method for cyclomatic complexity reduction. The scope boundary
is drawn around a single method and does not extend to any other symbol in the codebase.

| Field | Value |
|---|---|
| **Method** | `CancelOrphanedTargets` |
| **File** | `src/V12_002.UI.Compliance.cs` |
| **Lines** | 553–578 |
| **Current CYC** | 13 |
| **Target CYC** | ≤ 8 |
| **Wave** | 7 |
| **Phase** | 1 — Scope Definition |

---

## Scope Boundary

The scope boundary is defined as follows:

- **In scope:** the single method `CancelOrphanedTargets` (lines 553–578,
  `src/V12_002.UI.Compliance.cs`) and any private helper methods extracted directly from it
  during Phase 2 refactoring.
- **Out of scope:** all other methods in `src/V12_002.UI.Compliance.cs`, all callers in the
  call chain (`HandleFleetStopFill`, `ProcessQueuedExecution_HandleFleetOCO`,
  `OnAccountExecutionUpdate`), and the downstream gateway `CancelOrderOnAccount`
  (`src/V12_002.Orders.CancelGateway.cs`).

---

## Callers

A grep of `src/` for `CancelOrphanedTargets` returned **1 caller**:

| Call site | File | Line |
|---|---|---|
| `HandleFleetStopFill` | `src/V12_002.UI.Compliance.cs` | 522 |

The grep matched 2 lines total: the call site (line 522) and the method definition (line 553).
There is exactly **1 direct caller** of `CancelOrphanedTargets` within the codebase.

Full call chain for context (no additional callers, documented in `00-hotspots.md`):

```
OnAccountExecutionUpdate
  └─ ProcessAccountExecutionQueue (strategy thread)
       └─ ProcessQueuedExecution_HandleFleetOCO
            └─ HandleFleetStopFill          ← sole direct caller (line 522)
                 └─ CancelOrphanedTargets   ← IN SCOPE
                      └─ CancelOrderOnAccount (out of scope)
```

---

## Why Other Methods Are NOT in Scope

Per convention **V12.23**, scope for a single-method complexity epic is restricted to the
identified hotspot only. Expanding scope to neighbouring methods — even those with structural
duplication such as `HandleFleetTargetFill` (lines 676–693, same file) — is deferred to a
separate epic or to Phase 2 of this epic at the discretion of the refactor engineer.

Specific exclusions under V12.23:

1. **`HandleFleetStopFill`** (line 522) — sole direct caller; excluded because it does not
   contribute to the CYC=13 figure and any interface change to `CancelOrphanedTargets` is
   limited to its return type (`int`) and single `Account` parameter, both of which remain
   unchanged after extraction.

2. **`HandleFleetTargetFill`** (lines 676–693) — contains a structurally similar
   cancel-and-count loop. Noted as a Phase 2 generalisation candidate in `00-hotspots.md`
   (`CancelMatchingOrders`), but including it here would widen the scope boundary beyond a
   single method and introduce unnecessary regression risk before test coverage is in place.

3. **`CancelOrderOnAccount`** (`src/V12_002.Orders.CancelGateway.cs`, line 46) — downstream
   gateway called from 9 other sites across 6 files. Its signature is frozen for this epic;
   no changes to it are permitted. V12.23 prohibits cross-file scope expansion driven solely
   by a single-method CYC reduction.

4. **All other methods in `src/V12_002.UI.Compliance.cs`** — not referenced in the hotspot
   analysis and contribute zero CYC to the target figure.

---

## Complexity Reduction Plan (Summary)

Detailed rationale is in `00-hotspots.md`. Two extractions are planned for Phase 2:

| Extraction | New Symbol | Estimated CYC | Notes |
|---|---|---|---|
| 1 (required) | `IsOrphanedTarget(Order o)` | ~7 | Isolates full boolean filter; loop drops to ~4 |
| 2 (optional) | `CancelMatchingOrders(Account, Func<Order,bool>)` | ~2 | Generalises cancel-loop; Phase 2 only |

Post-extraction target for `CancelOrphanedTargets`: **CYC ≤ 4** (well within the ≤ 8 ceiling).
Combined CYC of both extracted helpers stays within policy limits.

---

## Constraints and Risks

- **Threading:** `CancelOrphanedTargets` executes on the strategy thread. Any extracted helper
  must not introduce a lock or alter the `.ToArray()` snapshot pattern.
- **Blast radius:** `CancelOrderOnAccount` is called from 9 other sites; its signature must not
  change. The scope boundary explicitly excludes it.
- **Correctness surface:** The five-arm T1–T5 prefix filter is the primary correctness surface.
  Extraction must be semantically equivalent — no prefix arm may be silently dropped.
- **Return value:** `CancelOrphanedTargets` returns `int` (cancelled count); `HandleFleetStopFill`
  uses this value for a `Print` log. The return contract is preserved by both planned extractions.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase1-scope |
| **Epic** | EPIC-W7-047 |
| **Wave** | 7 |
| **Phase** | 1 — Scope Definition |
| **Input artifacts** | `00-hotspots.md`, `manifest.json`, grep of `src/` |
| **Output artifact** | `00-scope.md` |
| **Scope confirmed** | Single method — `CancelOrphanedTargets` |
| **CYC current / target** | 13 / ≤ 8 |
| **Callers found** | 1 (`HandleFleetStopFill`, line 522) |
