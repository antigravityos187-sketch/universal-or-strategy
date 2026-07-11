# EPIC-W7-038 — Phase 1: Scope Definition

## Overview

This document defines the exact scope boundary for EPIC-W7-038. It establishes which
single method is targeted for cyclomatic complexity reduction, what the reduction goal
is, and why all other methods in the file remain outside scope.

---

## Method in Scope

| Field                          | Value                                      |
|--------------------------------|--------------------------------------------|
| **Method**                     | `VerifyPhotonSlotIntegrity`                |
| **Source File**                | `src/V12_002.SIMA.Fleet.cs`               |
| **Lines**                      | 329–389                                    |
| **Current CYC**                | **9**                                      |
| **Target CYC**                 | **≤ 8**                                    |
| **Wave**                       | 7                                          |
| **Epic**                       | EPIC-W7-038                                |

This epic targets a **single method**: `VerifyPhotonSlotIntegrity`. No other method is
included within the scope boundary of this epic's planning or implementation phases.

---

## Scope Boundary

The **scope boundary** is defined as follows:

- **In scope:** The body of `VerifyPhotonSlotIntegrity` (lines 329–389 of
  `src/V12_002.SIMA.Fleet.cs`) and any new private helper methods introduced
  exclusively to reduce its CYC — provided those helpers have no additional callers
  outside this method at the time of introduction.

- **Out of scope:** All other methods in `src/V12_002.SIMA.Fleet.cs`, all methods in
  all other source files, and any opportunistic clean-up of surrounding code not
  directly required to bring `VerifyPhotonSlotIntegrity` CYC from 9 to ≤ 8.

The scope boundary is intentionally narrow. Blast-radius analysis (Phase 0) identified
14 downstream symbols across 7 files that are affected by `VerifyPhotonSlotIntegrity`.
Those symbols are **reference points for correctness verification only** — they are not
modification targets.

---

## Callers

A grep of `src/` for `VerifyPhotonSlotIntegrity` returned **1 caller**:

| Location                       | Line | Context                                        |
|--------------------------------|------|------------------------------------------------|
| `src/V12_002.SIMA.Fleet.cs`   | 258  | `PumpFleetDispatch()` — ring consumer hot path |

The method is called from `PumpFleetDispatch()` at line 258 of the same file. No
external callers exist outside `src/V12_002.SIMA.Fleet.cs`.

---

## Why Other Methods Are NOT in Scope

The following related methods appear in the same file and share rollback or scheduling
patterns with `VerifyPhotonSlotIntegrity`. They are explicitly excluded from scope
under **rule V12.23** — one epic, one method:

| Method                          | CYC | Reason Excluded                                                      |
|---------------------------------|-----|----------------------------------------------------------------------|
| `PumpFleetDispatch`             | 7   | V12.23: not the designated hotspot for this epic                     |
| `ProcessFleetSlot`              | 6   | V12.23: shares rollback patterns but has its own complexity budget   |
| `DrainAllDispatchQueuesOnAbort` | 5   | V12.23: already within acceptable CYC threshold; out of scope        |
| `ProcessValidPhotonSlot`        | —   | V12.23: downstream caller only; not a complexity hotspot             |
| `TryResetCircuitBreakerIfBelow` | —   | V12.23: utility method; invoked by target but not the target itself  |

**V12.23** mandates that each wave epic addresses a **single method** per phase cycle.
Bundling multiple methods into a single epic's scope creates overlapping blast radii,
increases rollback risk, and makes validation ambiguous. Even where patterns are
shared (e.g., the pump-reprime try/catch appearing 3× in `SIMA.Fleet.cs`), the
cross-method unification of those patterns is deferred to a separate epic if and when
that epic is raised by the wave planner.

Any helper methods extracted as part of this epic must remain private and must serve
`VerifyPhotonSlotIntegrity` exclusively. If a helper naturally serves multiple callers,
that shared extraction is out of scope for EPIC-W7-038 and must be proposed as a
follow-on epic in Wave 7 planning.

---

## CYC Reduction Strategy (Summary)

Phase 0 identified two recommended extractions to bring CYC 9 → ≤ 8:

1. **`RollbackPhotonSlotState`** — Extract the 5-branch inline rollback sequence
   (delta rollback → sync-clear → dict removes → pool release → sideband clear →
   counter decrement → circuit-breaker reset) into a single private helper.
   Projected reduction: CYC 9 → ~4.

2. **`TryReprimePump`** — Extract the guarded `TriggerCustomEvent` pump-reprime
   pattern into a zero-parameter private helper.
   Projected reduction (combined with extraction 1): CYC ~4 → ~3.

The target of CYC ≤ 8 is therefore conservative; the extractions are expected to
overshoot it significantly. The target ceiling is set at ≤ 8 to match the wave
threshold, but the implementation aim is CYC ≤ 4.

---

## Agent Tracking

| Field                  | Value                                                    |
|------------------------|----------------------------------------------------------|
| **Agent Name**         | `v12-phase1-scope`                                       |
| **Epic**               | EPIC-W7-038                                              |
| **Wave**               | 7                                                        |
| **Phase**              | 1 — Scope Definition                                     |
| **Output File**        | `docs/brain/EPIC-W7-038/00-scope.md`                    |
| **Bobcoins Used**      | 0                                                        |
| **Source Files Read**  | `src/V12_002.SIMA.Fleet.cs`                             |
| **Grep Passes**        | 1 (`VerifyPhotonSlotIntegrity` across `src/`)           |
| **Inputs Consumed**    | `00-hotspots.md`, `manifest.json`                        |
