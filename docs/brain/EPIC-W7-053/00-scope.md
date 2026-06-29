# EPIC-W7-053 — Phase 1: Scope Definition

## Overview

This document establishes the formal scope boundary for EPIC-W7-053, Wave 7. The
work targets a single method whose complexity has been confirmed by Phase 0 hotspot
analysis and whose callers have been verified by static grep across the full `src/`
tree.

---

## Method in Scope

| Field                | Value                                          |
|----------------------|------------------------------------------------|
| **Method**           | `InitiateStopReplacement`                      |
| **File**             | `src/V12_002.Trailing.StopUpdate.cs`           |
| **Lines**            | 307–369 (63 loc)                               |
| **Class**            | `V12_002` (partial — Trailing module)          |
| **Visibility**       | `private void`                                 |
| **CYC (current)**    | 0 (tool-reported at intake) / 6 (manual count) |
| **CYC (target)**     | ≤ 8 (post-extraction ceiling per V12.23 policy)|
| **Build tag**        | Build 955 / V8.30                              |

This is a **single method** scope. No other method is included in this epic.

---

## Scope Boundary

The **scope boundary** is defined precisely as follows:

- **In scope:** The body of `InitiateStopReplacement` (lines 307–369,
  `src/V12_002.Trailing.StopUpdate.cs`) and any helper methods extracted
  exclusively from it during Phase 2.
- **Out of scope:** All other methods in `V12_002`, all other files, all
  callers of `InitiateStopReplacement`, and all consumers of the shared state
  mutated by `InitiateStopReplacement` (e.g., `pendingStopReplacements`,
  `circuitBreakerActive`, `pos.CurrentStopPrice`).

No changes to call signatures, public/internal APIs, or any file outside
`src/V12_002.Trailing.StopUpdate.cs` are permitted within this epic.

---

## Caller Count

Static grep of `src/` for `InitiateStopReplacement` yields **2 matches**:

| Match | File                                     | Line | Role        |
|-------|------------------------------------------|------|-------------|
| 1     | `src/V12_002.Trailing.StopUpdate.cs`     | 128  | **Caller**  |
| 2     | `src/V12_002.Trailing.StopUpdate.cs`     | 307  | Definition  |

**Caller count: 1** — `UpdateStopOrder` (same file, line 128) is the sole
direct caller, invoked only when `currentStop.OrderState` is `Working` or
`Accepted`. There are no cross-file callers. This low fan-in reduces regression
risk and confirms the method is safe to refactor in isolation.

---

## Why Other Methods Are NOT in Scope

Per **V12.23 policy** (single-method epic constraint), each EPIC-W7-xxx
engagement targets exactly one method per phase cycle. The following related
methods were identified in Phase 0 but are explicitly excluded:

| Method                              | Reason Excluded                                                      |
|-------------------------------------|----------------------------------------------------------------------|
| `UpdateExistingPendingReplacement`  | Separate CYC budget; shares circuit-breaker pattern but is independent |
| `UpdateStopOrder`                   | Sole caller; modifying it is outside the scope boundary              |
| `CaptureTargetSnapshot`             | Extraction *target* (receives delegated logic), not refactored itself|
| `CancelOrderForReplace`             | Downstream callback; no complexity issue in current build            |
| `CreateDirectStopOrder`             | Contains duplicated ternary but carries its own CYC budget           |

V12.23 mandates that blast-radius containment is achieved by limiting each
epic to a **single method** rather than cascade-refactoring across the call
graph. This prevents unintended regressions across the 13+ consumer files that
read `pendingStopReplacements` on the callback path.

---

## Complexity Profile

### Current state (Phase 0 findings)

- **Tool-reported CYC:** 0 (instrumentation gap at intake)
- **Manual static CYC:** 6
- Three complexity drivers identified (see `00-hotspots.md`):
  1. Inlined target-snapshot loop — ~20 loc duplication of `CaptureTargetSnapshot()`
  2. Eager circuit-breaker write inside `TryAdd` branch — mixes queue
     bookkeeping with global safety-mode mutation
  3. Nested ternary level-name formatter — +2 CYC, duplicated in
     `CreateDirectStopOrder`

### Target state (Phase 2 exit criteria)

- **Target CYC:** ≤ 8 (ceiling defined by V12.23)
- **Expected post-extraction CYC:** 3 (per Phase 0 estimate)
- Three extractions planned (see `00-hotspots.md` §Recommended Extraction Count)

---

## Agent Tracking

```
EPIC:         EPIC-W7-053
Wave:         7
Phase:        1 — Scope Definition
Status:       completed
Agent Name:   v12-phase1-scope
Output:       docs/brain/EPIC-W7-053/00-scope.md
Source:       src/V12_002.Trailing.StopUpdate.cs
Method:       InitiateStopReplacement
CYC current:  0 (tool-reported) / 6 (manual static count)
CYC target:   ≤ 8
Callers:      1 (UpdateStopOrder, line 128, same file)
Scope:        single method — InitiateStopReplacement only
```
