# EPIC-W7-063 — Phase 1: Scope Definition

**Epic:** EPIC-W7-063 | **Wave:** 7 | **Phase:** 1  
**Source File:** `src/V12_002.SIMA.Fleet.cs`  
**Agent Name:** v12-phase1-scope

---

## 1. Single Method In Scope

This phase operates on exactly one **single method**:

| Symbol | File | Line |
|---|---|---|
| `DrainAllDispatchQueuesOnAbort` | `src/V12_002.SIMA.Fleet.cs` | 287 |

The scope boundary is drawn at the entry and exit of `DrainAllDispatchQueuesOnAbort` exclusively. No other methods, helpers, callee implementations, or structural siblings fall inside the scope boundary for this phase.

---

## 2. Cyclomatic Complexity

| Metric | Value |
|---|---|
| Current CYC (per hotspots, CYC_CONFIRMED) | 0 (incremental/delta; absolute McCabe ≈ 8) |
| Target CYC | ≤ 8 |
| Delta to target | 0 — already within budget |

The CYC=0 value is the **incremental delta complexity** assigned by the refactoring ticket, reflecting that the method is pre-existing and not newly introduced in this wave. The raw McCabe count of independent paths inside the body is approximately 8 (two `while` loop conditions, four `if` guards, one nested `if`). The target ceiling of ≤ 8 is therefore met by the existing implementation without structural change in this phase.

---

## 3. Callers

A `grep` of `src/` for `DrainAllDispatchQueuesOnAbort` returns **2 hits** — 1 definition (line 287) and **1 call site**:

| Call Site | File | Line | Context |
|---|---|---|---|
| `PumpFleetDispatch()` | `src/V12_002.SIMA.Fleet.cs` | 238 | Invoked when `isFlattenRunning == true` OR `EnableSIMA == false` |

**Caller count: 1**

The method is called exclusively from `PumpFleetDispatch()`. There are no callers outside `src/V12_002.SIMA.Fleet.cs`. This tight coupling to a single call site reinforces the integrity of the scope boundary: changes to `DrainAllDispatchQueuesOnAbort` carry zero risk of unintended caller-side breakage beyond that single dispatch pump method.

---

## 4. Why Other Methods Are NOT In Scope

### V12.23 — Out-of-scope boundary rationale

The following structurally related methods were identified in Phase 0 hotspot analysis but are explicitly excluded from this phase's scope boundary:

| Method / Location | Reason Excluded |
|---|---|
| `SIMA.Lifecycle.cs:107–134` (shutdown drain) | Belongs to the lifecycle teardown subsystem; modifying it alongside the abort path in the same phase would conflate two distinct operational contexts (runtime abort vs. graceful shutdown) and exceed the single method constraint of V12.23 |
| `TryResetCircuitBreakerIfBelow(finalCount)` (`src/V12_002.SIMA.Fleet.cs:420`) | Callee, not in scope; its behaviour is a post-condition side-effect of the drain, not a structural part of the method under analysis |
| `AddExpectedPositionDeltaLocked(key, -delta)` (`src/V12_002.SIMA.cs:88`) | Cross-subsystem callee; H-2 threading inconsistency (locked vs. unlocked variant) is noted for a dedicated lifecycle phase, not bundled here |
| `VerifyPhotonSlotIntegrity` (rollback path) | Partial structural duplicate noted in H-1; extraction of shared helpers is a P1 recommendation but constitutes a separate refactoring unit |
| `PumpFleetDispatch()` (`src/V12_002.SIMA.Fleet.cs:233`) | Sole caller; modifying the caller's dispatch logic is outside the drain-path scope boundary |

**V12.23 rule:** Each phase targets a **single method**. Expanding scope to include structurally related methods — even when hotspot analysis reveals DRY violations or threading inconsistencies — requires a separate phase ticket. This phase is bounded to `DrainAllDispatchQueuesOnAbort` only.

---

## 5. Scope Summary

- **File:** `src/V12_002.SIMA.Fleet.cs`
- **Single method:** `DrainAllDispatchQueuesOnAbort` (line 287)
- **Scope boundary:** Entry point of `DrainAllDispatchQueuesOnAbort` to its closing brace (lines 287–323)
- **Caller count:** 1 (`PumpFleetDispatch`, line 238, same file)
- **Current CYC:** 0 (delta/incremental); absolute ≈ 8
- **Target CYC:** ≤ 8 ✓
- **Other methods excluded:** Per V12.23 single-method constraint; lifecycle drain, CB reset, cross-subsystem callees, and structural duplicates all deferred to dedicated phase tickets

---

## Agent Tracking

| Field | Value |
|---|---|
| Agent Name | v12-phase1-scope |
| Epic | EPIC-W7-063 |
| Wave | 7 |
| Phase | 1 — Scope Definition (REDO) |
| Source | `src/V12_002.SIMA.Fleet.cs` |
| Output | `docs/brain/EPIC-W7-063/00-scope.md` |
| Phase 0 Input | `docs/brain/EPIC-W7-063/00-hotspots.md` |

---

*Generated: Phase 1 Scope Definition — EPIC-W7-063 Wave 7*
