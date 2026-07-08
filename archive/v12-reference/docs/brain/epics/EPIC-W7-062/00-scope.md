# EPIC-W7-062 — Phase 1: Scope Definition

## Summary

This document establishes the precise scope boundary for the Wave 7 refactor captured under EPIC-W7-062. The unit of work is a **single method** — no more, no less.

---

## Method in Scope

| Property       | Value                              |
|----------------|------------------------------------|
| **Method**     | `ProcessFleetSlot`                 |
| **File**       | `src/V12_002.SIMA.Fleet.cs`        |
| **Lines**      | 44–97                              |
| **Class**      | `V12_002 : Strategy` (partial)     |
| **Current CYC**| **13**                             |
| **Target CYC** | **≤ 8**                            |

---

## Callers

A grep of `src/` for `ProcessFleetSlot` returned **5 matches, all within `src/V12_002.SIMA.Fleet.cs`**:

| Call Site | Caller Method           | Line |
|-----------|-------------------------|------|
| 1         | `PumpFleetDispatch`     | 271  |
| 2         | `ProcessValidPhotonSlot`| 399  |

Lines 94 and 393 are a diagnostic log string and an XML doc-comment respectively — not invocation sites. **Total direct callers: 2**, both residing in the same file. No cross-file callers exist.

---

## Scope Boundary

The **scope boundary** is drawn around `ProcessFleetSlot` and its inline complexity contribution from the helper `ValidateDispatchTimestamp` (which is counted in the CYC total but is already well-extracted and will not be restructured). Specifically:

- **In scope**: the body of `ProcessFleetSlot` (lines 44–97), including its `try / catch / finally` control graph and the nested pump-prime `try / catch` inside the `finally` block.
- **In scope (extract targets)**: the `finally` pump-prime block → candidate `TryRePrimeFleetPump()`, and the catch compensation block → candidate `CompensateFailedDispatch(...)`.
- **Out of scope**: every other method in the file and every downstream file that references the 8 shared-state surfaces identified in the hotspot analysis.

This is a **single method** refactor. The scope boundary does not expand to neighbours, callers, or callees beyond what is necessary to lower CYC to ≤ 8.

---

## Why Other Methods Are NOT in Scope

Per project rule **V12.23**, the Wave 7 scope is intentionally constrained:

> *"A refactor wave targets one hotspot method per EPIC. Adjacent methods may be renamed or have signatures adjusted as a side-effect, but they are never the primary target. Expanding scope mid-wave requires a new EPIC ticket."*

Applying V12.23 to this EPIC:

- `PumpFleetDispatch` (ln 271) — caller of `ProcessFleetSlot`; its CYC is within threshold and its logic does not need decomposition to achieve the target CYC reduction.
- `ProcessValidPhotonSlot` (ln 399) — second caller; similarly within threshold; adjusting its call site is limited to updating the argument list if `ProcessFleetSlot`'s signature changes, which is a trivial mechanical update, not a refactor.
- `ValidateDispatchTimestamp` (ln 107) — already extracted; well-scoped; its inline CYC contribution is resolved by extraction of `ProcessFleetSlot`'s body, not by modifying the helper itself.
- `InitializeFollowerBracketFSM`, `SubmitAndRegisterFleetOrders`, `RollbackFleetDispatchState` — downstream callees; already extracted helpers; no structural change needed per hotspot analysis Phase 0 recommendation.

Expanding into any of the above methods would violate V12.23 and requires a separate EPIC.

---

## Complexity Target Rationale

| Step | Action                                        | CYC Saved |
|------|-----------------------------------------------|-----------|
| 1    | Extract `finally` pump-prime → `TryRePrimeFleetPump()` | ~3 |
| 2    | Extract catch compensation → `CompensateFailedDispatch(...)` | ~2 |
| —    | Remaining baseline after extractions          | ≤ 8 |

The target of **≤ 8** is deliberately set one unit above the hotspot recommendation of ≤ 7 to provide a one-unit buffer against tooling variance in CYC measurement. The hard ceiling accepted by the EPIC is **≤ 8**.

---

## Agent Tracking

| Field        | Value                        |
|--------------|------------------------------|
| Agent Name   | v12-phase1-scope             |
| Epic ID      | EPIC-W7-062                  |
| Wave         | 7                            |
| Phase        | 1 — Scope Definition         |
| Generated    | Phase 1 (REDO)               |
| Source file  | `src/V12_002.SIMA.Fleet.cs`  |

---

*Wave 7 | Phase 1 | EPIC-W7-062*
