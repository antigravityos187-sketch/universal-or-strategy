# EPIC-W7-052 — Phase 1: Scope Definition

## Overview

This document establishes the precise scope boundary for the EPIC-W7-052
refactoring effort. The work targets a single method with elevated cyclomatic
complexity inside the trailing stop subsystem of `V12_002`.

---

## Single Method in Scope

| Field              | Value                                              |
|--------------------|----------------------------------------------------|
| **Method**         | `CleanupStalePendingReplacements`                  |
| **File**           | `src/V12_002.Trailing.StopUpdate.cs`               |
| **Class**          | `V12_002` (partial) — `NinjaTrader.NinjaScript.Strategies` |
| **Visibility**     | `private void`                                     |
| **Current CYC**    | 11                                                 |
| **Target CYC**     | ≤ 8 (stretch goal: ≤ 4 per extracted sub-method)  |
| **Callers Count**  | 1 direct call site (`src/V12_002.Trailing.cs:222` inside `ManageTrailingStops`) |

The scope boundary is drawn exclusively around this single method and the three
sub-method extractions recommended in Phase 0. No other production file is
modified as a primary deliverable of this epic.

---

## Complexity Reduction Target

- **Current CYC**: 11 (10 branch points + 1 base)
- **Target CYC**: ≤ 8 for the orchestrating method after extraction
- **Mechanism**: Extract 3 sub-methods (`RemoveStalePendingEntry`,
  `RecoverStopForStaleEntry`, `ScheduleBracketRestoration`) so each private
  helper carries ≤ 4 CYC independently, and the top-level loop body is
  reduced to a linear sequence of helper calls.

---

## Caller Analysis

Grepping `src/` for `CleanupStalePendingReplacements` returns **4 matches**
across **2 files**:

| File | Line | Nature |
|------|------|--------|
| `src/V12_002.Trailing.cs` | 5 | Comment listing resident methods (non-call) |
| `src/V12_002.Trailing.cs` | 222 | **Live call site** inside `ManageTrailingStops` |
| `src/V12_002.Trailing.StopUpdate.cs` | 1 | Comment / build header (non-call) |
| `src/V12_002.Trailing.StopUpdate.cs` | 37 | **Method definition** |

**Effective caller count: 1** (`ManageTrailingStops` at `Trailing.cs:222`).

There is a single call site, which means the extraction refactor carries a
contained integration risk: only one upstream invocation path needs
regression validation.

---

## Scope Boundary

The scope boundary for EPIC-W7-052 is strictly limited to:

1. **`CleanupStalePendingReplacements`** in `src/V12_002.Trailing.StopUpdate.cs`
   — the single method being decomposed.
2. Up to **3 new private helper methods** added to the same partial class file
   (`src/V12_002.Trailing.StopUpdate.cs`) as direct extractions from item 1.
3. **No changes** to call sites, no changes to the surrounding
   `ManageTrailingStops` orchestrator beyond the already-existing single call.

Everything outside this boundary — shared state (`pendingStopReplacements`,
`activePositions`, `pendingReplacementCount`), downstream consumers
(REAPER, SIMA, Orders, Symmetry, UI, Lifecycle), and sibling methods
(`UpdateStopOrder`, `CalculateStopForLevel`) — is **explicitly out of scope**.

---

## Why Other Methods Are NOT in Scope

Per standing project rule **V12.23** (*one hotspot per epic, one epic per
wave phase*), each EPIC targets a single method. The following sibling methods
in `src/V12_002.Trailing.StopUpdate.cs` were evaluated and excluded:

| Method | CYC | Reason Excluded |
|--------|-----|-----------------|
| `UpdateStopOrder` | — | Below wave-7 CYC threshold; not flagged in hotspot scan |
| `CalculateStopForLevel` | — | Below wave-7 CYC threshold; not flagged in hotspot scan |
| `ManageTrailingStops` | — | Resides in `Trailing.cs`; separate partial file, separate hotspot candidacy |

Rule V12.23 prohibits bundling multiple methods into a single scope document
or a single refactoring epic, regardless of adjacency or shared state.
Violating V12.23 would invalidate the blast-radius containment guarantees
established in Phase 0 and expand regression risk beyond what the single-method
callers-count of 1 permits.

---

## Blast Radius Reminder (Informational, Not In-Scope)

Although only a single method is in scope, engineers must be aware that the
shared data surfaces mutated by `CleanupStalePendingReplacements` span ~41
production files (see `00-hotspots.md`). The extractions introduced in this
epic must be **pure restructuring** — no observable behaviour change, no
mutation-order change, no new state introduced — so the blast radius remains
contained.

---

## Acceptance Criteria

- [ ] Post-refactor CYC of `CleanupStalePendingReplacements` (orchestrator) ≤ 8
- [ ] Each extracted sub-method has individual CYC ≤ 4
- [ ] Zero changes outside `src/V12_002.Trailing.StopUpdate.cs`
- [ ] Single call site at `Trailing.cs:222` continues to compile and behave identically
- [ ] No new public API surface introduced

---

## Agent Tracking

```
Agent Name    : v12-phase1-scope
epic_id       : EPIC-W7-052
wave          : 7
phase         : 1
phase_name    : Scope Definition
output_file   : docs/brain/EPIC-W7-052/00-scope.md
method        : CleanupStalePendingReplacements
source_file   : src/V12_002.Trailing.StopUpdate.cs
cyc_current   : 11
cyc_target    : <=8
callers_count : 1
rule_applied  : V12.23 (single method per epic)
scope_type    : single method
generated_by  : Bob (technical assistant)
```
