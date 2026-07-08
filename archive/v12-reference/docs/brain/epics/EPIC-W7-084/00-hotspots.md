# EPIC-W7-084 — Phase 0: Hotspot Analysis

**Wave:** 7 | **Phase:** 0 | **Status:** completed  
**Generated:** 2025-01-01 (automated — Bob/jCodeMunch pipeline)  
**Source file:** [`src/V12_002.REAPER.Audit.cs`](../../src/V12_002.REAPER.Audit.cs)

---

## Target Method

| Field | Value |
|---|---|
| Symbol | `AuditFleet_CalculateExpectedActual` |
| Class | `V12_002` (partial) |
| File | `src/V12_002.REAPER.Audit.cs` lines 382–451 |
| Visibility | `private void` |
| CYC (tool-reported) | **0** |
| Parameters (out) | 8 `out` parameters + 2 in |

---

## Method Summary

[`AuditFleet_CalculateExpectedActual`](../../src/V12_002.REAPER.Audit.cs:382) is the **position-state hydration kernel** of the REAPER fleet audit loop. It is called once per fleet account per audit cycle from [`AuditSingleFleetAccount`](../../src/V12_002.REAPER.Audit.cs:121) and populates all decision inputs needed by the downstream desync handlers via 10-parameter output tuple (C# `out` pattern — no return value).

The method performs four logical steps:

1. **Broker position read** — resolves `actualQty` (signed int) from `acct.Positions` filtered by `Instrument.FullName`. Flat = 0, Long = positive, Short = negative.
2. **FSM expected-position resolution** — queries [`GetFsmExpectedPosition(acct.Name)`](../../src/V12_002.Symmetry.BracketFSM.cs:422) which sums signed quantities from all `FollowerBracketFSM` entries in `_followerBrackets` for the account that are in an in-flight state. **FSM is the sole authority (Build 1105)** — `expectedPositions` dictionary is legacy/master only.
3. **Hydrated-Active FSM edge-case handling** — iterates `accountFsms` for `Active` FSMs that have a null `EntryOrder` (restart edge-case). If broker position exists, adds it to `fsmExpectedQty`; otherwise auto-terminates the stale FSM via [`TryTerminateFollowerBracket`](../../src/V12_002.Symmetry.BracketFSM.cs:127).
4. **Guard-flag population** — computes `syncPending` (`_dispatchSyncPendingExpKeys` lookup), `inFillGrace` ([`IsReaperFillGraceActive`](../../src/V12_002.REAPER.cs:61)), `hasState`, and `expectedKey` (`ExpKey(acct.Name)`).

---

## CYC: 0 — Interpretation

The jCodeMunch tool reports **CYC = 0** for this method. This is a scoring artifact: the tool's complexity metric is relative to a baseline and/or discounts control-flow inside LINQ lambdas (lines 395, 403) and `foreach` + nested `if` blocks that fall within single-exit paths. The method contains **~6 real decision points**:

| Line | Branch |
|---|---|
| 397 | `if (pos != null && pos.MarketPosition != MarketPosition.Flat)` |
| 399 | ternary — `Long ? pos.Quantity : -pos.Quantity` |
| 409 | `if (f.State == Active && f.EntryOrder == null)` |
| 411 | `if (actualQty != 0)` (inside foreach) |
| 433 | `if (fsmExpectedQty != 0)` |
| 447 | `if (shouldLog && hasState)` |

CYC = 0 from the tool means **no independent decision paths add structural risk** — the method is effectively a pure data-hydration function with no early returns or exception branches. This makes it a **low-risk refactor candidate** but a **high-impact blast-radius hotspot** because every downstream desync decision depends on its outputs.

---

## Blast Radius

`AuditFleet_CalculateExpectedActual` is the sole producer of 8 output values consumed by the entire fleet audit decision tree:

```
AuditSingleFleetAccount (L121)
 └── AuditFleet_CalculateExpectedActual  ← THIS METHOD
      ├── actualQty  → desync branch at L145–178
      ├── expectedQty → desync branch at L145–178
      ├── syncPending → AuditFleet_HandleDesyncRepair (L196)
      ├── inFillGrace → AuditFleet_HandleDesyncRepair (L196)
      ├── hasState    → return value of AuditSingleFleetAccount
      ├── accountFsms → DetectOrphanFSM (L181-184)
      ├── expectedKey → AuditFleet_HandleNakedPosition (L188)
      └── pos         → AuditFleet_HandleNakedPosition (L188)
```

**Callers:** 1 direct (`AuditSingleFleetAccount`)  
**Transitive callers:** `AuditApexPositions` → REAPER background timer → every 1s audit cycle  
**Data sources touched:** `_followerBrackets` (ConcurrentDictionary), `_dispatchSyncPendingExpKeys` (ConcurrentDictionary), `_accountFillGraceTicks`, `_positionPassFailedFirstSeen`, `acct.Positions` (broker live data)

---

## Hotspot Flags

| Flag | Severity | Detail |
|---|---|---|
| 10-parameter signature | Medium | Exceeds clean-code parameter limit; signals candidate for a value-object/struct return type |
| Dual-iteration of `accountFsms` | Low | `_followerBrackets.Values.Where(...)` allocates a `List<>` that is iterated once here and again in the caller; could be merged |
| Side-effect inside read method | Medium | [`TryTerminateFollowerBracket`](../../src/V12_002.Symmetry.BracketFSM.cs:127) mutates `_followerBrackets` on line 418 — a "calculate" method that also terminates FSMs violates CQS |
| Implicit `_positionPassFailedFirstSeen` mutation | Low | Line 435 clears position-pass grace as a side-effect of position calculation; semantically surprising in a `Calculate*` method |
| LINQ on hot path | Low | `acct.Positions.FirstOrDefault(...)` and `.Values.Where(...).ToList()` execute on every audit tick; negligible at normal scale but relevant under fleet stress |

---

## Recommendations for Phase 1+

1. **Introduce `FleetAuditState` struct** — replace the 8 `out` parameters with a single returned value object to eliminate the wide signature and make the data flow explicit.
2. **Extract stale-FSM termination** — move [`TryTerminateFollowerBracket`](../../src/V12_002.Symmetry.BracketFSM.cs:127) call out of the calculation path into the caller (`AuditSingleFleetAccount`) to restore CQS in `AuditFleet_CalculateExpectedActual`.
3. **Annotate authority invariant** — add XML doc `<remarks>` confirming FSM-as-sole-authority rule (Build 1105) so future refactors don't accidentally re-enable `expectedPositions` reads for fleet accounts.

---

## Files Investigated

- [`src/V12_002.REAPER.Audit.cs`](../../src/V12_002.REAPER.Audit.cs) — primary
- [`src/V12_002.REAPER.cs`](../../src/V12_002.REAPER.cs) — `IsReaperFillGraceActive`, fill-grace state
- [`src/V12_002.Symmetry.BracketFSM.cs`](../../src/V12_002.Symmetry.BracketFSM.cs) — `GetFsmExpectedPosition`, `TryTerminateFollowerBracket`
- [`src/V12_002.cs`](../../src/V12_002.cs) — `_followerBrackets`, `_dispatchSyncPendingExpKeys` field declarations
- [`src/V12_002.SIMA.cs`](../../src/V12_002.SIMA.cs) — `ExpKey`, `_dispatchSyncPendingExpKeys` lifecycle
