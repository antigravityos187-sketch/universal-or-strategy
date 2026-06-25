# Phase 0: Hotspot Analysis - EPIC-W7-080

## Agent Tracking
- **Agent Name**: v12-phase0-hotspot
- **Bobcoins Used**: 0.74
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-23T02:49:40Z

## Target Method
- **Method**: PlacePanel
- **File**: src/V12_002.UI.Panel.Construction.cs
- **Line**: 239
- **Cyclomatic Complexity**: 13
- **Lines of Code**: 80
- **Max Nesting Depth**: 4
- **Parameter Count**: 0

## Complexity Metrics

### Assessment: HIGH COMPLEXITY
The method has a cyclomatic complexity of 13, which exceeds the V12 DNA threshold of 8 (Jane Street strict standard). This indicates:
- Multiple decision paths (13 branches)
- Deep nesting (4 levels)
- Significant cognitive load for maintenance
- Higher risk of bugs and race conditions

### Detailed Metrics
```json
{
  "cyclomatic": 13,
  "max_nesting": 4,
  "param_count": 0,
  "lines": 80,
  "assessment": "high"
}
```

## Blast Radius

### Impact Analysis: LOW RISK
The blast radius analysis reveals this is an **isolated internal method**:
- **Importer Count**: 0 (no external files import this)
- **Direct Dependents**: 0 (no external dependencies)
- **Overall Risk Score**: 0.0
- **Confirmed Consumers**: 0
- **Potential Consumers**: 0

**Interpretation**: This method is self-contained within the UI.Panel.Construction.cs file. Changes will not ripple through the codebase, making it a **safe refactoring target**.

## Call Hierarchy

### Callers (Who calls PlacePanel)
1. **CreatePanel** (method)
   - File: src/V12_002.UI.Panel.Construction.cs
   - Line: 163
   - Resolution: ast_resolved
   - **Single caller** - straightforward refactoring path

### Callees (What PlacePanel calls)
PlacePanel invokes **20 helper methods** across multiple files:

#### Primary Dependencies
1. **FindChartTrader** - Chart trader discovery (2 variants)
2. **FindChartTabGrid** - Grid location (2 variants)
3. **_placementRetryTimer** - Timer field access
4. **DumpVisualTree** - Debug visualization (2 variants)

#### Secondary Dependencies (Depth 2)
5. **FindChartTraderViaOwnerChart** - Owner-based search
6. **FindChartTraderViaChartTab** - Tab-based search
7. **FindChartTraderBySiblingSearch** - Sibling traversal
8. **FindChartTraderByTypeName** - Type-based lookup
9. **FindChartTraderByButton** - Button-based discovery
10. **FindDescendantGrid** - Grid descendant search

**Pattern**: The method orchestrates multiple UI discovery strategies, suggesting it contains complex conditional logic for panel placement.

## Risk Assessment

### Overall Risk: MEDIUM-HIGH

**Complexity Risk**: HIGH
- CYC 13 exceeds threshold by 62.5%
- 4-level nesting indicates complex branching
- 80 lines suggests multiple responsibilities

**Blast Radius Risk**: LOW
- Isolated method (no external consumers)
- Single caller (CreatePanel)
- Changes won't cascade

**Refactoring Difficulty**: MEDIUM
- 20 callees suggest orchestration logic
- Multiple discovery strategies to preserve
- UI threading concerns (DispatcherTimer usage)

### Recommended Approach
1. **Extract conditional branches** into focused helper methods
2. **Preserve discovery strategy sequence** (5 fallback methods)
3. **Maintain timer retry logic** (placement retry mechanism)
4. **Target CYC ≤ 8** per extracted method

## Hotspot Score Calculation

Using CodeScene methodology: `hotspot_score = complexity × log(1 + churn)`

**Note**: Churn data requires git history analysis. For this phase, we focus on complexity metrics.

- **Complexity Component**: 13 (high)
- **Churn Component**: [Requires Phase 1 git analysis]
- **Preliminary Hotspot Score**: 13 (complexity-only baseline)

## Extraction Candidates

Based on call hierarchy, these sub-responsibilities can be extracted:

1. **Chart Trader Discovery Logic** (5 fallback strategies)
   - Extract to: `FindChartTraderWithFallbacks()`
   - Estimated CYC reduction: 5-7 points

2. **Grid Placement Logic** (tab grid + descendant search)
   - Extract to: `LocatePlacementGrid()`
   - Estimated CYC reduction: 2-3 points

3. **Retry Timer Setup** (timer initialization + event wiring)
   - Extract to: `ConfigurePlacementRetryTimer()`
   - Estimated CYC reduction: 1-2 points

**Target**: Reduce PlacePanel to CYC ≤ 8 (orchestration only)

## Next Steps (Phase 1)

1. **Scope Definition**: Define exact extraction boundaries
2. **Git Churn Analysis**: Calculate hotspot score with historical data
3. **Dependency Mapping**: Verify all 20 callees are safe to refactor around
4. **Test Coverage Check**: Ensure UI panel tests exist before surgery

## Conclusion

PlacePanel is a **high-complexity, low-risk refactoring target**. The method's isolation (0 external consumers) makes it safe to refactor, while its complexity (CYC 13) makes it necessary. The 20 callees suggest orchestration logic that can be decomposed into 3-4 focused helper methods.

**Recommendation**: PROCEED to Phase 1 (Scope Definition)
