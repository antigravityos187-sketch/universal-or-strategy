# EPIC-W7-042 — Phase 1: Scope Definition

## Overview

This document defines the precise scope boundary for the EPIC-W7-042 refactor effort within
Wave 7, Phase 1. It is produced from the Phase 0 hotspot analysis (`00-hotspots.md`) and the
grep-confirmed caller inventory performed during this phase.

---

## Single Method in Scope

This epic targets a **single method** only:

| Field            | Value                              |
|------------------|------------------------------------|
| **Method**       | `SymmetryGuardOnFollowerFill`      |
| **File**         | `src/V12_002.Symmetry.Follower.cs` |
| **Line Range**   | 17 – 88                            |
| **Current CYC**  | 16                                 |
| **Target CYC**   | ≤ 8 (Jane Street threshold)        |
| **Max Nesting**  | 6                                  |
| **Param Count**  | 3                                  |
| **Visibility**   | `private`                          |
| **Risk Score**   | 0.0 (zero blast radius)            |

---

## Scope Boundary

The **scope boundary** for EPIC-W7-042 is drawn tightly around `SymmetryGuardOnFollowerFill`
alone. No other methods, classes, or files fall within the refactor scope for this epic.
All work — extraction of helpers, complexity reduction, and validation — must remain inside
`src/V12_002.Symmetry.Follower.cs` and must not alter observable behaviour of any method
outside the scope boundary.

---

## Callers

Grep of `src/` for `SymmetryGuardOnFollowerFill` returned **1 confirmed external call site**
(plus the definition itself):

| # | File                              | Line | Role              |
|---|-----------------------------------|------|-------------------|
| 1 | `src/V12_002.UI.Compliance.cs`    | 506  | External caller   |
| 2 | `src/V12_002.Symmetry.Follower.cs`| 17   | Definition        |

`SymmetryGuardOnFollowerFill` is `private`, meaning only callers within the same partial-class
assembly are valid. The single external call site at
[`src/V12_002.UI.Compliance.cs:506`](../../src/V12_002.UI.Compliance.cs:506) passes
`(fleetKey, pos, fleetFillPrice)` — the method signature must be preserved unchanged after
refactoring. **Total external callers: 1.**

---

## Why Other Methods Are NOT in Scope

Per the V12.23 scope rule, epics in Wave 7 target one complexity hotspot at a time.
The following sibling methods appear in `src/V12_002.Symmetry.Follower.cs` and were
considered but explicitly excluded:

| Method                                  | Reason Excluded                                                     |
|-----------------------------------------|---------------------------------------------------------------------|
| `SymmetryGuardProcessPendingFollowerFills` | Separate concern; CYC within threshold; no hotspot classification |
| `SymmetryGuardTryResolveFollower`        | Called by the target method, not a driver of its own CYC spike     |
| `IsAnchorPending`                        | Utility predicate; CYC = 1; no refactor benefit                    |
| `ApplyMasterAnchor`                      | Anchor write path governed by ADR-019; separate epic if needed      |
| `SubmitFollowerBracket`                  | Dispatch leaf; CYC within threshold                                 |

V12.23 explicitly prohibits bundling adjacent-method refactors into a single epic to prevent
scope creep and keep validation surface area minimal. Only `SymmetryGuardOnFollowerFill`
breaches the CYC ≤ 8 threshold (CYC = 16), so only it falls within this epic's scope boundary.

---

## Complexity Drivers (from Phase 0)

Three drivers inflate CYC from the baseline to 16:

1. **ANCHOR-01 Pre-check Block** (~6 CYC) — nested double-TryGetValue chain reaching nesting
   depth 4, with conditional `SymmetryGuardApplyMasterAnchor` call.
2. **`shouldSubmitImmediately` Branch** (~4 CYC) — boolean gate forking into immediate bracket
   submission vs. ANCHOR-GATE delay; tightly coupled with Driver 1.
3. **PendingFollowerFill + TryResolve + TryRemove tail** (~3 CYC) — three sequential
   conditional operations interleaved in the method tail.

---

## Planned Extractions (Phase 2 Preview)

| # | Extracted Helper                                                        | Estimated CYC Reduction |
|---|-------------------------------------------------------------------------|--------------------------|
| 1 | `TryPreApplyMasterAnchor(fleetEntryName, followerPos, out bool submitted)` | ~6                     |
| 2 | `SubmitOrEnqueueFollowerBracket(fleetEntryName, followerPos, bool shouldSubmitImmediately)` | ~3 |

Post-extraction target: CYC ≤ 6 (comfortably within the ≤ 8 threshold).

---

## Constraints

- **ADR-019 Compliance**: `AnchorSnapshot` reads are intentionally lock-free via
  `Interlocked.CompareExchange`. Extracted helpers must NOT introduce any lock blocks around
  these reads.
- **ANCHOR-01 Annotation**: The `// [ANCHOR-01] V12.Phase7.1` comment must be preserved in
  whichever helper absorbs that block, to maintain the audit trail.
- **Signature Preservation**: The public-facing signature of `SymmetryGuardOnFollowerFill`
  must remain `(string fleetKey, Position pos, double fleetFillPrice)` — the single external
  caller at `src/V12_002.UI.Compliance.cs:506` must require zero changes.

---

## Agent Tracking

| Field             | Value                        |
|-------------------|------------------------------|
| **Agent Name**    | v12-phase1-scope             |
| **Epic**          | EPIC-W7-042                  |
| **Wave**          | 7                            |
| **Phase**         | 1 — Scope Definition (REDO)  |
| **Input Docs**    | `00-hotspots.md`, `manifest.json` |
| **Grep Confirmed**| `src/` — 1 external caller   |
| **Completed At**  | 2026-06-26T03:00:00Z         |
