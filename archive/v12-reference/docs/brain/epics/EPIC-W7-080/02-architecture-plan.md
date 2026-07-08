# Phase 2: Architecture Plan -- EPIC-W7-080

**Agent:** v12-phase2-architecture
**Wave:** 7 | **Phase:** 2 -- Architecture Planning
**Generated:** 2026-06-29T01:10:00Z
**Input:** docs/brain/EPIC-W7-080/01-scope-boundary.md

---

## Method Under Extraction

- **Method:** `PlacePanel`
- **Source File:** `src/V12_002.UI.Panel.Construction.cs`
- **Original CYC:** 13
- **Signature:** `private void PlacePanel()`
- **Lines:** 239--318

### jcodemunch get_context_bundle result

Symbol resolved as `src/V12_002.UI.Panel.Construction.cs::V12_002.PlacePanel#method`. Full source
retrieved (lines 239-318). The method is a three-path WPF visual-tree placement dispatcher:
- **PATH 1 (Hijack):** Finds ChartTrader slot via `FindChartTrader()`, attaches `rootContainer`
  using Grid column/row/span values, collapses native ChartTrader element,
  sets `_placementMode = PanelPlacement.Hijack`. Contains compound pattern-match guard
  (`_chartTraderElement != null && Parent is Grid traderGrid`) plus `rSpan > 1` and `cSpan > 1`
  conditionals.
- **PATH 2 (Inject):** Calls `FindChartTabGrid`, adds a new 210-wide `ColumnDefinition`, spans
  row if `RowDefinitions.Count > 1`, sets `_placementMode = PanelPlacement.Injected`.
- **PATH 3 (Retry/Fallback):** Guards `_placementRetryCount < 3`, lazily constructs
  `DispatcherTimer` with a Tick lambda containing a compound `_isTerminating || rootContainer ==
  null` guard, recursively calls `PlacePanel()`. After retries exhausted: falls back to
  `UserControlCollection.Add`.
- **Key state mutated:** `_placementMode`, `_placementGrid`, `_chartTraderElement`,
  `_placementRetryCount`, `_placementRetryTimer`, `rootContainer`.

### jcodemunch get_call_hierarchy result

- **Direct caller (depth 1):** `CreatePanel` (`src/V12_002.UI.Panel.Construction.cs:163`) --
  ast_resolved
- **Self-recursive caller:** `DispatcherTimer.Tick` lambda inside `PlacePanel` (internal)
- **Callees (depth 1):** `FindChartTrader` (`src/V12_002.UI.Panel.Helpers.cs:478`),
  `FindChartTabGrid` (`src/V12_002.UI.Panel.Helpers.cs:647`),
  `DumpVisualTree` (`src/V12_002.UI.Panel.Helpers.cs:353`),
  `_placementRetryTimer` field reference
- **Callees (depth 2):** `FindChartTraderViaOwnerChart`, `FindChartTraderViaChartTab`,
  `FindChartTraderBySiblingSearch`, `FindChartTraderByTypeName`, `FindChartTraderByButton`,
  `FindDescendantGrid` (all in `src/V12_002.UI.Panel.Helpers.cs` -- none modified by this epic)

### jcodemunch get_dependency_graph result

- **Import edges:** 0 (the file has no tracked inter-file imports in the index graph)
- **Importer edges:** 0 (no other files directly import this file in the index graph)
- **Assessment:** Construction file is a partial class. Dependencies are resolved at compile time
  by the C# partial-class mechanism rather than tracked import edges. Cross-file calls to
  `Helpers.cs` symbols are callees resolved via call hierarchy above. No dependency graph changes
  required by this epic.

### jcodemunch get_extraction_candidates result

- No extraction candidates surfaced by the tool (min_complexity=3, min_callers=1).
- **Analysis:** The index's extraction-candidate algorithm requires multiple distinct caller files.
  `PlacePanel` is called from one file only (`CreatePanel` in the same partial class). The
  candidates are nonetheless identified via direct source analysis and hotspot report -- the three
  logical sections are each cohesive, independently testable, and account for all 13 CYC points.
  Extraction proceeds as designed.

---

## Sequential Thinking Summary

**5-thought sequentialthinking chain completed. Final thought (thought 5):**

PlacePanel (CYC 13) decomposes into exactly 3 private helper methods, all same-file
(`src/V12_002.UI.Panel.Construction.cs`), no signature changes on parent, no cross-file impact.
Extraction map:
1. `private bool TryHijackChartTrader()` -- CYC 4, extracts Hijack path (lines 246-267),
   returns true on success.
2. `private bool TryInjectIntoChartTabGrid()` -- CYC 3, extracts Inject path (lines 271-288),
   returns true on success; introduces `private const double PanelColumnWidth = 210` to eliminate
   the magic number shared with `DestroyPanel`.
3. `private void SchedulePlacementRetry()` -- CYC 5, extracts retry counter + lazy
   `DispatcherTimer` + Tick lambda (lines 291-312).

Parent `PlacePanel` after extraction: CYC 4 -- entry guard (compound OR), `if TryHijack return`,
`if TryInject return`, fallback unconditional.

`max_cyc_projected = 5` (`SchedulePlacementRetry`). `extraction_count = 3`. All metrics satisfy
Jane Street CYC<=8 mandate. Plan is ready for Phase 3 (DNA & PR Audit) and Phase 4 (Ticket
Generation).

---

## Extraction Plan

| Helper Method Name | Responsibility | Projected CYC |
|---|---|---|
| `TryHijackChartTrader()` | Extract Hijack path: compound pattern-match guard, Grid col/row/span attachment, ChartTrader visibility collapse, `_placementMode`/`_placementGrid` write; returns `true` on success | 4 |
| `TryInjectIntoChartTabGrid()` | Extract Inject path: `FindChartTabGrid` null-check, `ColumnDefinitions.Add` with `PanelColumnWidth` const, row-span guard, grid attachment, `_placementMode` write; returns `true` on success | 3 |
| `SchedulePlacementRetry()` | Extract retry path: `_placementRetryCount < 3` guard, lazy `DispatcherTimer` construction, Tick lambda with `_isTerminating \|\| rootContainer == null` guard, `PlacePanel()` recursive call | 5 |

### New Constant

| Constant | Value | Purpose |
|---|---|---|
| `private const double PanelColumnWidth = 210` | 210.0 | Shared width between `TryInjectIntoChartTabGrid` and `DestroyPanel` heuristic -- eliminates magic number tight coupling |

---

## Parent Method After Extraction

**Remaining logic in `PlacePanel()` after extraction:**

```
private void PlacePanel()
{
    if (rootContainer == null || _placementMode != PanelPlacement.None)
        return;

    _chartTraderElement = FindChartTrader();

    if (TryHijackChartTrader())
        return;

    _chartTraderElement = null;
    if (TryInjectIntoChartTabGrid())
        return;

    SchedulePlacementRetry();
}
```

- **Remaining logic:** Entry guard (compound OR), discovery call, three path dispatches via helpers, fallback is inside `SchedulePlacementRetry` (moved to helper after retry exhausted -- alternatively the fallback stays in parent as a 4th line after `SchedulePlacementRetry` returns without placing, keeping the retry helper focused on the timer only).
- **Projected CYC:** 4

---

## max_cyc_projected: 5
## extraction_count: 3

---

## Jane Street Alignment

| Rule | Status |
|---|---|
| CYC<=8 achieved | YES -- parent=4, TryHijack=4, TryInject=3, ScheduleRetry=5; all <=8 |
| Single-responsibility per helper | YES -- each helper has exactly one concern |
| Lock-free/Actor pattern preserved | YES -- no `lock()` blocks in PlacePanel; DispatcherTimer tick is dispatcher-bound; no threading primitives added |
| ASCII-only string literals | YES -- all Print() strings use ASCII printable characters only |
| Illegal states unrepresentable | YES -- `_placementMode` guard prevents double-placement; each helper writes `_placementMode` exactly once on success; `PanelColumnWidth` const eliminates hidden coupling with DestroyPanel |
| xUnit [Fact] tests required | YES -- 1 test per extracted helper (3 tests) + 1 parent dispatch test = 4 xUnit [Fact] tests |
| No signature change on parent | YES -- `private void PlacePanel()` signature unchanged; 2 callers unaffected |
| Scope creep prevention | YES -- V12.23 boundary PASS from Phase 1.5; helpers are same-file private only |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 1.5 |
| **Execution Time** | 2026-06-29T01:10:00Z |
| **Wave** | 7 |
| **Phase** | 2 |
| **Method** | PlacePanel |
| **Original CYC** | 13 |
| **max_cyc_projected** | 5 |
| **extraction_count** | 3 |
| **jcodemunch tools called** | get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **Output** | docs/brain/EPIC-W7-080/02-architecture-plan.md |
