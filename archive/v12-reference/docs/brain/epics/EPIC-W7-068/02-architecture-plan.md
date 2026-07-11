# Phase 2: Architecture Plan -- EPIC-W7-068

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 -- Architecture Planning
**Generated:** 2026-06-29T01:10:00Z
**Input:** docs/brain/EPIC-W7-068/01-scope-boundary.md

---

## Method Under Extraction

- **Method:** `TryParseTargetMode`
- **Source File:** `src/V12_002.UI.IPC.cs`
- **Lines:** 97-128
- **Signature:** `private static bool TryParseTargetMode(string raw, out TargetMode mode)`
- **Original CYC:** 7 (actual McCabe; tool reported 0 due to C# partial-class analyser gap)

### jcodemunch get_context_bundle result

Symbol resolved via id `src/V12_002.UI.IPC.cs::V12_002.TryParseTargetMode#method`. Source retrieved successfully (lines 97-128). Key findings:
- Method is `private static` -- pure computation helper, no state mutation.
- Guard clause already present: `if (string.IsNullOrWhiteSpace(raw)) return false;`
- Switch on normalized string maps 9 string aliases to 4 `TargetMode` enum values.
- All string literals are ASCII-only: `"ATR"`, `"A"`, `"TICKS"`, `"TICK"`, `"T"`, `"POINTS"`, `"POINT"`, `"PTS"`, `"P"`, `"RUNNER"`, `"R"`.
- `out` parameter `mode` is always assigned before any `return` path -- no uninitialized-out-param risk.
- Default arm returns `false` silently (no logging) -- observability gap identified in Phase 0.

### jcodemunch get_call_hierarchy result

Returned 0 callers and 0 callees from index. This is consistent with the partial-class fragmentation: the index cannot resolve cross-partial-class call edges. From manual analysis confirmed in Phase 0 and Phase 1:
- **Callers (1 unique method, 5 invocations):** `TryApplyConfigTarget_Type` in `src/V12_002.UI.IPC.Commands.Config.cs` (lines 303, 311, 319, 327, 335)
- **Callees:** None (pure BCL calls: `string.IsNullOrWhiteSpace`, `.Trim()`, `.ToUpperInvariant()` -- all inline, no extractable sub-methods)

### jcodemunch get_dependency_graph result

Returned 0 edges (imports: [], importers: []). Consistent with partial-class fragmentation -- file's using directives resolve to NinjaTrader and System namespaces not indexed as local files. No cross-file local dependency edges beyond the partial class contract. File is self-contained for this method.

### jcodemunch get_extraction_candidates result

No candidates found (`min_complexity=3`, `min_callers=1`). Confirms: the index sees no extractable sub-functions with sufficient complexity or reuse. Consistent with CYC=7 being already within the acceptable threshold.

---

## Sequential Thinking Summary

**5-thought chain completed. Final verdict (Thought 5):**

CYC=7 is already compliant with the <= 8 Jane Street mandate. No structural extraction is required. The authorized changes per 00-scope.md are:

1. **Observability improvement (in-place):** Add `Print` diagnostic in the `default:` arm before `return false;` -- adds zero branches, CYC remains 7.
2. **Tooling fix (config/infra):** Document that the partial-class analyser must be configured to resolve C# `partial` class boundaries so future automated runs report CYC=7 accurately instead of 0.

Jane Street pattern compliance is already satisfied: guard clause in place, ASCII-only literals, no locks, pure computation, single responsibility, illegal states unrepresentable (mode always assigned).

---

## Extraction Plan

| Helper Method Name | Responsibility | Projected CYC |
|---|---|---|
| *(none -- CYC 7 already <= 8; in-place observability change only)* | N/A | N/A |

**No new helper methods are extracted.** The sole code change is adding a `Print` call in the `default:` arm:

```csharp
default:
    Print("TryParseTargetMode: unrecognized target mode value '" + raw + "'");
    return false;
```

This is a single-statement addition with zero cyclomatic complexity impact.

---

## Parent Method After Extraction

- **Remaining logic:** Full method body unchanged structurally. Guard clause, string normalization, and switch-based dispatch preserved as-is. One `Print` statement added to `default:` arm for observability.
- **Projected CYC:** 7 (unchanged -- `Print` is a statement, not a decision branch)

---

## max_cyc_projected: 7
## extraction_count: 0

---

## Jane Street Alignment

| Rule | Status |
|---|---|
| CYC <= 8 achieved | YES -- CYC=7, compliant before and after change |
| Single-responsibility per helper | N/A -- no helpers; method itself is single-responsibility (parse one string to one TargetMode) |
| Lock-free/Actor pattern preserved | YES -- no locks present; method is pure computation |
| Guard clause pattern applied | YES -- `IsNullOrWhiteSpace` early return already in place |
| ASCII-only string literals | YES -- all switch case labels are ASCII |
| Illegal states unrepresentable | YES -- `out mode` is always assigned before any return path; default value = `TargetMode.ATR`; no null/uninitialized escape |
| Extract Guard Clauses | ALREADY DONE -- no further guard extraction needed |
| Replace Switch with Lookup Table | NOT APPLIED -- CYC is already compliant at 7; lookup table would be over-engineering for a stable, simple 4-enum mapping |
| FSM Decomposition | NOT APPLICABLE -- no state machine transitions in this method |
| Extract Loop Body | NOT APPLICABLE -- no loops |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | 2026-06-29T01:10:00Z |
| **Phase** | 2 -- Architecture Planning |
| **Wave** | 7 |
| **Epic** | EPIC-W7-068 |
| **jcodemunch tools called** | get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |

---

*Wave 7 | Phase 2 | EPIC-W7-068*
