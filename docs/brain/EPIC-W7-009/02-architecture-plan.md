# Phase 2: Architecture Plan — EPIC-W7-009

## Method Under Extraction

- **Method:** `FindChartTraderViaChartTab`
- **Source File:** `src/V12_002.UI.Panel.Helpers.cs`
- **Lines:** 529–564
- **Original CYC:** 9
- **Class:** `V12_002`
- **Namespace:** `NinjaTrader.NinjaScript.Strategies`
- **Visibility:** `private`
- **Return type:** `FrameworkElement`

### jcodemunch get_context_bundle result

Full source confirmed via symbol ID `src/V12_002.UI.Panel.Helpers.cs::V12_002.FindChartTraderViaChartTab#method`:

```csharp
private FrameworkElement FindChartTraderViaChartTab()
{
    try
    {
        DependencyObject chartTab = TryFindChartTabViaVisualTree(ChartControl);
        if (chartTab == null)
            chartTab = TryFindChartTabViaLogicalTree(ChartControl);

        if (chartTab == null)
        {
            Print("V12 PANEL: Strategy 1 -- ChartTab not found in visual/logical tree");
            return null;
        }

        FrameworkElement result =
            TryGetChartTraderViaProperty(chartTab)
            ?? TryGetChartTraderViaFields(chartTab)
            ?? TryGetChartTraderViaDescendants(chartTab);

        if (result == null)
            Print("V12 PANEL: Strategy 1 -- ChartTab found ("
                + chartTab.GetType().Name
                + ") but no ChartTrader property/field/child");

        return result;
    }
    catch (Exception ex)
    {
        Print("V12 PANEL: ChartTab reflection failed -- " + ex.Message);
    }
    return null;
}
```

Key findings: 9 branch nodes — base(1) + if-null visual fallback(1) + if-null-assign(implicit path)(1) + if-null early return(1) + ??(1) + ??(1) + if-result-null diagnostic(1) + catch(1) + return-null post-catch(1).

### jcodemunch get_call_hierarchy result

- **Callers (depth 1):** `FindChartTrader` (line 478, same file) — 1 direct caller
- **Callees (depth 1):** `TryFindChartTabViaVisualTree` (line 726), `TryFindChartTabViaLogicalTree` (line 739), `TryGetChartTraderViaProperty` (line 752), `TryGetChartTraderViaFields` (line 768), `TryGetChartTraderViaDescendants` (line 785)
- **Callees (depth 2):** `FindChildElementByTypeName` (line 686) — called by the depth-1 helpers

### jcodemunch get_dependency_graph result

- `src/V12_002.UI.Panel.Helpers.cs` has **0 cross-file import edges and 0 importers** at depth 1
- The file is a self-contained partial class with no direct file-level dependency edges in the index
- Blast radius is isolated to the single source file

### jcodemunch get_extraction_candidates result

- No candidates returned by tool at `min_complexity=3, min_callers=1`
- Rationale: all callees are already-extracted helpers (from EPIC-CCN-17) invoked by only this one method — they do not qualify as multi-caller extraction candidates
- The extraction opportunity identified by this plan (ResolveChartTab) is a NEW extraction from the parent method body, not a re-extraction of existing helpers

---

## Sequential Thinking Summary

**5-thought chain completed:**

1. **Thought 1 — Source confirmed.** Full method body validated from context bundle. CYC=9 with 9 identified branch nodes. Callees are all existing well-tested helpers from EPIC-CCN-17.

2. **Thought 2 — Helper identification.** The dual-tree ChartTab search (visual fallback then logical — branches 2-3) is the single extractable concern. Named `ResolveChartTab(ChartControl chart)`. Uses `??` operator instead of if-assign to eliminate one explicit branch, reducing its own CYC to 2. Parent drops from CYC 9 to CYC 6 after extraction.

3. **Thought 3 — Contract design.** `ResolveChartTab` signature: `private DependencyObject ResolveChartTab(ChartControl chart)`. Body: `return TryFindChartTabViaVisualTree(chart) ?? TryFindChartTabViaLogicalTree(chart);`. Zero heap allocation (reference comparison only). No shared state. No lock blocks.

4. **Thought 4 — CYC verification.** `ResolveChartTab` CYC = 2 (<=8 ✓). Parent after extraction CYC = 6 (<=8 ✓). All Jane Street rules verified.

5. **Thought 5 — Final verdict: APPROVED.** 1 extraction, max projected CYC = 6. Implementation: add helper near line 726 (with existing TryFindChartTab* helpers), replace the dual-tree if-assign pattern in parent with a single `ResolveChartTab(ChartControl)` call. Build + CSharpier check = sufficient verification.

---

## Extraction Plan

| Helper Method Name | Responsibility | Projected CYC |
|---|---|---|
| `ResolveChartTab` | Resolves a `DependencyObject` ChartTab by attempting visual-tree traversal first, falling back to logical-tree traversal via `??` operator — returns null if both fail | 2 |

### Helper Signature

```csharp
private DependencyObject ResolveChartTab(ChartControl chart)
{
    return TryFindChartTabViaVisualTree(chart) ?? TryFindChartTabViaLogicalTree(chart);
}
```

**Placement:** Co-located with existing `TryFindChartTab*` helpers near line 726 in `src/V12_002.UI.Panel.Helpers.cs`.

### Replacement in Parent

Replace:
```csharp
DependencyObject chartTab = TryFindChartTabViaVisualTree(ChartControl);
if (chartTab == null)
    chartTab = TryFindChartTabViaLogicalTree(ChartControl);
```

With:
```csharp
DependencyObject chartTab = ResolveChartTab(ChartControl);
```

---

## Parent Method After Extraction

**Remaining logic:**
1. Call `ResolveChartTab(ChartControl)` to get chartTab
2. Guard clause: if `chartTab == null` → Print diagnostic + return null
3. `??` reflection chain: `TryGetChartTraderViaProperty ?? TryGetChartTraderViaFields ?? TryGetChartTraderViaDescendants`
4. Null-result diagnostic Print if result is null
5. Return result
6. `catch` handler for reflection exceptions → Print + return null (post-catch)

**Branch count:** base(1) + if-null guard(1) + ??(1) + ??(1) + if-result-null(1) + catch(1) = **CYC 6**

- **Projected CYC:** 6

---

## max_cyc_projected: 6
## extraction_count: 1

---

## Jane Street Alignment

| Rule | Status |
|---|---|
| CYC<=8 achieved | **YES** — parent CYC 6, helper CYC 2, both <=8 |
| Single-responsibility per helper | **YES** — `ResolveChartTab` does exactly one thing: resolve ChartTab from either tree |
| Lock-free/Actor pattern preserved | **YES** — no state mutations, pure functional reference resolution |
| Illegal states unrepresentable | **YES** — `ResolveChartTab` returns either a valid `DependencyObject` or null; no partially-found intermediate state; parent guard clause ensures downstream reflection only runs with a non-null reference |
| Zero-allocation hot paths | **YES** — `??` operator on reference types; no heap allocations |
| Extract Guard Clauses | **YES** — null-check with Print+return stays in parent as explicit guard clause |
| Replace Switch/If-Chains with Lookup Tables | **N/A** — no switch or if-chain present |
| FSM Decomposition | **N/A** — no FSM state machine in this method |
| Extract Loop Body | **N/A** — no loops present |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Epic** | EPIC-W7-009 |
| **Wave** | 7 |
| **Phase** | 2 — Architecture Planning |
| **Method** | `FindChartTraderViaChartTab` |
| **Original CYC** | 9 |
| **Max Projected CYC** | 6 |
| **Extraction Count** | 1 |
| **Bobcoins Used** | 1.5 |
| **Execution Time** | 2026-06-29T01:10:00Z |
| **jcodemunch tools called** | resolve_repo, get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **Output artifact** | `docs/brain/EPIC-W7-009/02-architecture-plan.md` |
| **Status** | Phase 2 complete |
