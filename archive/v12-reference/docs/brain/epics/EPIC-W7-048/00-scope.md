# EPIC-W7-048 — Phase 1: Scope Definition

## Single Method in Scope

**Method:** `UpdateExistingPendingReplacement`
**Source:** `src/V12_002.Trailing.StopUpdate.cs` (lines 167–253)
**Visibility:** `private` helper within the `V12_002` partial class
**Caller count:** 1 (called exclusively from `UpdateStopOrder` at line 119)

This document defines the scope boundary for EPIC-W7-048. Only a single method is subject to
analysis, extraction planning, and subsequent refactor work within this epic.

---

## Complexity Metrics

| Metric | Value |
|---|---|
| **CYC (reported by tool)** | 0 (measurement artefact — lambdas not counted by some CYC tools) |
| **CYC (structural estimate)** | ~5 (TryAdd-success, TryAdd-fail→add-factory, TryAdd-fail→update-factory, breaker-check, BracketRestorationNeeded guard) |
| **Target CYC after refactor** | ≤ 8 |
| **Blast-radius symbols** | 8 directly coupled |
| **Circuit-breaker reach** | 13 source files touch `pendingReplacementCount` or `circuitBreakerActive` |

The target CYC of **≤ 8** is achievable via the two recommended extractions identified in Phase 0
(`TryActivateCircuitBreaker` and `BuildRefreshedPendingReplacement`), which together resolve the
primary branching tree without altering the outer `TryAdd`/`AddOrUpdate` concurrency orchestration.

---

## Scope Boundary

The scope boundary for this epic is **exactly one method**: `UpdateExistingPendingReplacement`.

No other methods are included in this epic's refactor scope. Every Phase 2 change must trace
directly to reducing complexity inside this single method or to named helpers extracted exclusively
from it.

### Why Other Methods Are NOT in Scope

The project operates under **V12.23 conventions**, which mandate that each epic targets a single
hotspot method identified by its CYC metric. Peer methods in the same dispatcher —
`HandleStalePendingReplacement`, `InitiateStopReplacement`, and `CreateDirectStopOrder` — are
structurally coupled to `UpdateExistingPendingReplacement` through shared concurrent state
(`pendingStopReplacements`, `pendingReplacementCount`, `circuitBreakerActive`) but are explicitly
excluded from this epic under V12.23 rules for the following reasons:

1. **Separate CYC budget** — Each of those methods carries its own independent CYC score. V12.23
   requires a dedicated epic per hotspot method; bundling them here would widen the scope boundary
   beyond a single method and invalidate the phase-gated review process.

2. **Risk isolation** — `InitiateStopReplacement` shares the non-atomic circuit-breaker pattern
   identified as Complexity Driver 3, but changing it simultaneously with `UpdateExistingPendingReplacement`
   doubles the blast radius on `pendingReplacementCount` and risks introducing count-discrepancy bugs
   across two concurrent code paths in the same release.

3. **Caller independence** — `HandleStalePendingReplacement` and `CreateDirectStopOrder` are reached
   through separate conditional branches in `UpdateStopOrder` and are never called by
   `UpdateExistingPendingReplacement` itself. Refactoring them here provides no leverage on the
   complexity drivers enumerated in Phase 0.

Any future refactor of those peer methods must be opened as a separate epic, referencing this
document to confirm awareness of the shared-state coupling.

---

## Source File Context

- **File:** [`src/V12_002.Trailing.StopUpdate.cs`](src/V12_002.Trailing.StopUpdate.cs:167)
- **Method definition:** line 167
- **Only caller:** `UpdateStopOrder`, line 119, same file
- **Callers count:** 1

The method is `private` with a single caller, which means any signature change carries zero
call-site propagation risk outside the file.

---

## Phase 0 → Phase 1 Traceability

| Phase 0 Finding | Phase 1 Action |
|---|---|
| TryAdd/AddOrUpdate split with ≥5 decision paths | Extract `BuildRefreshedPendingReplacement` helper |
| Non-atomic circuit-breaker check duplicated in peer | Extract `TryActivateCircuitBreaker` helper |
| BracketRestorationNeeded two-level conditional | Contained inside `BuildRefreshedPendingReplacement` extraction |
| Structural CYC ~5, tool reports 0 | Target CYC ≤ 8 set conservatively to accommodate lambda measurement variance |

---

## Agent Tracking

Agent Name: v12-phase1-scope | Epic: EPIC-W7-048 | Wave: 7 | Phase: 1
Bobcoins Used: 1.0 | Execution Time: ~45s | Scope: single method | Method: UpdateExistingPendingReplacement
