# Phase 2: Architecture Plan — EPIC-W7-076

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:10:00Z
**Input:** docs/brain/EPIC-W7-076/01-scope-boundary.md

---

## Method Under Extraction

- **Method:** `CollapseAllExecutionControls`
- **Source File:** `src/V12_002.UI.Panel.Handlers.cs`
- **Lines:** 665–687
- **Class:** `V12_002` (partial)
- **Namespace:** `NinjaTrader.NinjaScript.Strategies`
- **Original CYC:** 1 (McCabe score 1 — confirmed by Phase 0 hotspot analysis; tool measured CYC=0/1, no logical branching decisions)

### jcodemunch get_context_bundle result

Symbol resolved via ID `src/V12_002.UI.Panel.Handlers.cs::V12_002.CollapseAllExecutionControls#method`. Source confirmed: `private void CollapseAllExecutionControls()` — 22 lines, 10 null-guard if-checks assigning `Visibility.Collapsed` or `Visibility.Visible`, no callees, no loops, no lock blocks. Imports are standard WPF/UI namespaces only.

### jcodemunch get_call_hierarchy result

- **Callers (depth 1):** `UpdateContextualUI` (line 654, same file) — AST-resolved
- **Callers (depth 2):** `SelectConfigMode` (line 591, same file) — AST-resolved
- **Callees:** None — method is a pure leaf with no outgoing calls
- **Caller count:** 2 across 2 depths; 1 direct caller

### jcodemunch get_dependency_graph result

- **Direction:** both (imports + importers)
- **Node count:** 1 — `src/V12_002.UI.Panel.Handlers.cs`
- **Edge count:** 0
- **Imports:** none tracked (C# partial class; uses project-internal references)
- **Importers:** none tracked
- File is a standalone partial class handler with no cross-file import edges in the index.

### jcodemunch get_extraction_candidates result

- **Candidates returned:** 0
- **Threshold:** min_complexity=3, min_callers=1
- No symbols in `src/V12_002.UI.Panel.Handlers.cs` meet the extraction threshold. Confirms `CollapseAllExecutionControls` (CYC=1) is below the complexity floor warranting extraction.

---

## Sequential Thinking Summary

**Final thought (thought 5 of 5):**

`CollapseAllExecutionControls` has CYC=1 as confirmed by Phase 0 hotspot analysis and validated by jCodemunch `get_context_bundle` (source inspection shows 10 null-guard assignments with no conditional branching logic). The method is already CYC-compliant (1 ≤ 8). No extraction is required. The architecture plan for EPIC-W7-076 is: `extraction_count=0`, `max_cyc_projected=1`, parent method unchanged. All Jane Street rules are satisfied. The Phase 2 output is a compliance confirmation with zero code changes required for Phase 5 execution.

---

## Extraction Plan

| Helper Method Name | Responsibility | Projected CYC |
|---|---|---|
| _No extraction required — method is already CYC=1 (≤8 compliant)_ | — | — |

No helper methods need to be created. The method body is already a single-responsibility sequential visibility-reset operation with no branching complexity.

---

## Parent Method After Extraction

- **Remaining logic:** All 10 null-guarded Visibility assignments remain in place — no modification required
- **Projected CYC:** 1

---

## max_cyc_projected: 1
## extraction_count: 0

---

## Jane Street Alignment

| Rule | Status |
|---|---|
| CYC ≤ 8 achieved | YES — CYC=1, already compliant |
| Single-responsibility per helper | YES — method itself has single responsibility (collapse all execution controls) |
| Lock-free / Actor pattern preserved | YES — no `lock()` blocks; pure Visibility property assignments on UI thread |
| Illegal states unrepresentable | YES — `Visibility` is a WPF enum; null-guards prevent NullReferenceException; no invalid states possible |
| Zero-allocation hot path | YES — no heap allocations; all assignments are direct property setters |
| Extract Guard Clauses | N/A — null-guards are already the idiomatic defensive guard pattern, correct as-is |
| Single-callee extraction | N/A — no extraction warranted |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 1.5 |
| **Execution Time** | 2026-06-29T01:10:00Z |
| **Wave** | 7 |
| **Phase** | 2 |
| **jcodemunch tools called** | get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **extraction_count** | 0 |
| **max_cyc_projected** | 1 |
