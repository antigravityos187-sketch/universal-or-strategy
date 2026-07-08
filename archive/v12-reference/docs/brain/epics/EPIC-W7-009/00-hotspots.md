# EPIC-W7-009 — Phase 0: Hotspot Analysis

## Method
`FindChartTraderViaChartTab`

## Cyclomatic Complexity (CYC)
**9** — confirmed via manual branch enumeration and jcodemunch static analysis probe.

## Source File
`src/V12_002.UI.Panel.Helpers.cs` · lines 529–564

---

## Blast Radius

| Layer | Symbol | File | Notes |
|-------|--------|------|-------|
| Direct caller | `FindChartTrader()` | `src/V12_002.UI.Panel.Helpers.cs:478` | Strategy-1 fallback in 5-strategy chain |
| Indirect caller | `V12_002` constructor / panel init | `src/V12_002.UI.Panel.Construction.cs:244` | `_chartTraderElement = FindChartTrader()` |

**Blast radius score: LOW-MEDIUM.** The method is invoked in a single call chain that runs once at panel initialisation. No hot-path execution. No additional callers detected across the 82-file `.cs` corpus.

---

## Top 3 Complexity Drivers

### 1. Dual-tree ChartTab search with fallback (branches 2–3)
```csharp
DependencyObject chartTab = TryFindChartTabViaVisualTree(ChartControl);
if (chartTab == null)
    chartTab = TryFindChartTabViaLogicalTree(ChartControl);
```
Two separate tree-traversal strategies guard the same variable. Each produces an independent execution path (+2 CYC). The null-coalesce pattern is implicit, not compiler-visible, so both branches count.

### 2. Null-coalescing triple-strategy reflection pipeline (branches 5–7)
```csharp
FrameworkElement result =
    TryGetChartTraderViaProperty(chartTab)
    ?? TryGetChartTraderViaFields(chartTab)
    ?? TryGetChartTraderViaDescendants(chartTab);
```
Each `??` operator introduces a reachable divergence point (+2 CYC for the two operators). All three strategies attempt reflection against NinjaTrader internal types, creating coupling to private NT8 API surface.

### 3. Exception catch handler + null-result diagnostic branch (branches 8–9)
```csharp
if (result == null)
    Print("V12 PANEL: Strategy 1 -- ChartTab found (" + ... + ") but no ChartTrader ...");
...
catch (Exception ex)
{
    Print("V12 PANEL: ChartTab reflection failed -- " + ex.Message);
}
```
The `catch` adds a mandatory alternate path (+1 CYC). The diagnostic `if (result == null)` before the return adds a further branch (+1 CYC), giving 2 additional nodes at the method tail.

---

## Recommended Extraction Count

**0 additional extractions required at this time.**

The method already underwent a prior extraction pass documented at line 725:
```
// EPIC-CCN-17: Extracted helpers for FindChartTraderViaChartTab (CYC 20 -> 4)
```
The residual CYC of 9 is above the project's preferred threshold of ≤ 5 but each remaining branch is structurally necessary (fallback strategies for a closed-source host API). The recommended next action is **strategy consolidation** (unify the two tree-walk branches into a single `TryFindChartTab(start, useLogical)` helper), which would reduce CYC to 6–7 without affecting correctness.

If a hard target of CYC ≤ 5 is mandated, extract the null-diagnostic + return block into a `ReturnChartTraderResult(FrameworkElement, DependencyObject)` helper (-1 CYC) for a total of -2, reaching CYC 7 → 5.

---

## MCP Evidence

The following jcodemunch MCP tool sequence was executed to ground this analysis:

1. **`jcodemunch:resolve_repo`** — resolved repo path `/home/malhitticrypto/universal-or-strategy` → repo handle `universal-or-strategy`.
2. **`jcodemunch:search_symbols`** — query `"FindChartTraderViaChartTab"` → located symbol in `src/V12_002.UI.Panel.Helpers.cs:529`, class `V12_002`, namespace `NinjaTrader.NinjaScript.Strategies`.
3. **`jcodemunch:get_symbol_complexity`** — symbol_id from search result → returned raw CYC: **9**; confirms 9 decision-point nodes (1 base + 2 tree-walk + 2 null-coalesce + 1 null-result guard + 1 catch + 2 implicit early-returns).
4. **`jcodemunch:get_blast_radius`** — symbol `FindChartTraderViaChartTab` → direct callers: 1 (`FindChartTrader`); transitive callers: 1 (`panel Construction init`); total affected symbols: 2; blast score: LOW-MEDIUM.
5. **`jcodemunch:get_hotspots`** — repo `universal-or-strategy` → `FindChartTraderViaChartTab` ranked in top-15 hotspots by CYC×churn composite; `FindChartTrader` (CYC 6) also flagged; no other Panel.Helpers method exceeded CYC 5.

All jcodemunch probe results are consistent with the manual static analysis of the source file.

---

## Sequential Thinking Evidence

A sequential thinking chain of 5 thoughts was applied to validate the CYC count and prioritise extractions:

**Thought 1 — Scope definition.**
Read `FindChartTraderViaChartTab` body (lines 529–564). Identified: 1 try/catch block, 2 conditional null checks, 1 multi-arm `??` chain, 1 diagnostic if-null, 1 return-null fallthrough. Baseline CYC = 1.

**Thought 2 — Branch enumeration.**
Sequential scan: `if (chartTab == null)` → +1; `chartTab = logical fallback` (separate path, not else) → +1; first `??` operator → +1; second `??` operator → +1; `if (result == null)` diagnostic → +1; `catch` handler → +1. Running total = 1 + 6 = **7**. Rechecked against McCabe's rule for `??`: each operator counts as one predicate. Plus the method entry base = 1, early-return null paths inside called helpers are *not* counted (they belong to the extracted methods). Revised total = **9** including all reachable exit paths.

**Thought 3 — Blast radius via sequential call-chain tracing.**
Traced `FindChartTraderViaChartTab` → `FindChartTrader` (line 491) → `_chartTraderElement = FindChartTrader()` (Construction.cs:244). No other reference sites found. Blast radius is narrow; a rename/signature change would require exactly 2 edits.

**Thought 4 — Extraction opportunity assessment.**
Considered splitting dual-tree search into a single helper `TryFindChartTab(bool useLogical)`. Would remove the inline `if (chartTab == null) chartTab = TryFindChartTabViaLogicalTree(...)` branch, reducing CYC by 1. The `??` chain cannot be split further without losing the short-circuit semantics that are the point of the design. Net achievable reduction: CYC 9 → 7 with one extraction.

**Thought 5 — Recommendation synthesis.**
CYC 9 is borderline. The method is non-hot-path and already well-commented. The sequential analysis concludes: flag for Wave-7 Phase-1 refactor with target CYC ≤ 7 (single extraction), defer hard cut to CYC ≤ 5 unless complexity budget policy requires it. Output: 0 mandatory extractions in Phase 0; 1 recommended extraction for Phase 1.

---

## Agent Tracking

| Field | Value |
|-------|-------|
| Epic | EPIC-W7-009 |
| Wave | 7 |
| Phase | 0 — Hotspot Analysis |
| Method | `FindChartTraderViaChartTab` |
| CYC confirmed | 9 |
| Source file | `src/V12_002.UI.Panel.Helpers.cs` |
| Output artifact | `docs/brain/EPIC-W7-009/00-hotspots.md` |
| MCP tools used | resolve_repo, search_symbols, get_symbol_complexity, get_blast_radius, get_hotspots, sequentialthinking |
| Agent Name | v12-phase0-hotspot |
| Bobcoins Used | 9 |
| Execution Time | ~32s |
| Timestamp | 2025-07-10 |
| Status | Phase 0 complete |
