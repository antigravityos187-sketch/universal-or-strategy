# EPIC-W7-068 — Phase 1: Scope Definition

## Single Method in Scope

This document establishes the scope boundary for EPIC-W7-068, Wave 7.

The **single method** under analysis is:

| Field            | Value                                      |
|------------------|--------------------------------------------|
| Method           | `TryParseTargetMode`                       |
| Source file      | `src/V12_002.UI.IPC.cs`                    |
| Lines            | 97–128                                     |
| CYC (tool)       | **0** (phantom — partial-class artefact)   |
| CYC (manual)     | **7** (real McCabe count, per Phase 0)     |
| Target CYC       | **≤ 8**                                    |

The real CYC of 7 is already within the target threshold of ≤ 8, confirming the
primary remediation need is measurement accuracy, not structural refactoring.

---

## Caller Analysis

`grep src/ TryParseTargetMode` returned **6 matches**:

- **1 definition** — `src/V12_002.UI.IPC.cs` line 97  
- **5 call sites** — all inside the single caller
  `TryApplyConfigTarget_Type` in `src/V12_002.UI.IPC.Commands.Config.cs`
  (lines 303, 311, 319, 327, 335 — one per T1TYPE–T5TYPE assignment)

**Callers count: 1 unique caller method, 5 invocation sites.**  
No other files reference this helper, confirming the blast radius is narrow.

---

## Scope Boundary

The **scope boundary** for this epic is strictly limited to the single method
`TryParseTargetMode` defined in `src/V12_002.UI.IPC.cs`.

Work authorised within scope:
1. Fix analysis tooling to resolve C# `partial class` boundaries so future runs
   report CYC = 7 accurately instead of 0.
2. Add a `Print` / log diagnostic in the `default:` arm of `TryParseTargetMode`
   for observability (zero CYC impact, one-line change).

Any changes to `TryApplyConfigTarget_Type` (silent-failure propagation, NACK
signalling) are **Phase 1c candidates** and do not cross the scope boundary of
Phase 1 unless explicitly promoted.

---

## Why Other Methods Are NOT in Scope

Per V12.23 project policy, epic scope is locked to the nominated hotspot method
at the time of Phase 0 analysis. Methods outside the single method boundary —
including `TryApplyConfigTarget_Type`, the `TargetMode` enum declaration in
`src/V12_002.Properties.cs`, and any other IPC command handlers — are **out of
scope** for this epic because:

- They were not the subject of the CYC anomaly reported by the Wave 7 scanner.
- Including them would violate the V12.23 single-responsibility-per-epic rule,
  which requires each epic to address exactly one hotspot symbol.
- Their own observability / return-value concerns are tracked separately under
  the hotspot log (H3, H4) and may be promoted to independent epics by the
  triage board.

No scope creep into surrounding methods is permitted without a new epic charter.

---

## Agent Tracking

| Field       | Value                  |
|-------------|------------------------|
| Agent Name  | v12-phase1-scope       |
| Wave        | 7                      |
| Phase       | 1 — Scope Definition   |
| Epic        | EPIC-W7-068            |
| Generated   | Phase 1 (REDO)         |

---

*Wave 7 | Phase 1 | EPIC-W7-068*
