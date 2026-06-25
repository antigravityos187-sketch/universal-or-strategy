# Phase 1: Scope Definition - EPIC-W7-080

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.18
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T20:08:27Z
- **Input Artifact**: 00-hotspots.md

## Epic Summary
**Target**: PlacePanel method in src/V12_002.UI.Panel.Construction.cs
**Current Complexity**: CYC 13 (exceeds threshold of 8 by 62.5%)
**Goal**: Reduce to CYC ≤ 8 through focused extraction

## IN SCOPE

### Primary Extraction Target
- **Method**: PlacePanel() (lines 239-319, CYC 13)
- **File**: src/V12_002.UI.Panel.Construction.cs
- **Justification**: High complexity (CYC 13), isolated method (0 external consumers), single caller

### Extraction Candidates (3 Helper Methods)

#### 1. Chart Trader Discovery Logic
- **New Method**: FindChartTraderWithFallbacks()
- **Responsibility**: Orchestrate 5 fallback strategies for chart trader discovery
- **Extracted Calls**: FindChartTrader variants, FindChartTraderViaOwnerChart, FindChartTraderViaChartTab, FindChartTraderBySiblingSearch, FindChartTraderByTypeName, FindChartTraderByButton
- **Estimated CYC**: 5-7
- **Target CYC**: ≤ 8

#### 2. Grid Placement Logic
- **New Method**: LocatePlacementGrid()
- **Responsibility**: Determine grid location for panel placement
- **Extracted Calls**: FindChartTabGrid variants, FindDescendantGrid
- **Estimated CYC**: 2-3
- **Target CYC**: ≤ 8

#### 3. Retry Timer Configuration
- **New Method**: ConfigurePlacementRetryTimer()
- **Responsibility**: Initialize and wire up placement retry timer
- **Extracted Calls**: _placementRetryTimer field access, Timer event wiring
- **Estimated CYC**: 1-2
- **Target CYC**: ≤ 8

### Modified Method
- **Method**: PlacePanel() (orchestration only)
- **Target CYC**: ≤ 8
- **Responsibility**: High-level orchestration of 3 extracted helpers

## OUT OF SCOPE

### Existing Helper Methods (Already Extracted)
- FindChartTrader, FindChartTabGrid, FindChartTraderViaOwnerChart, FindChartTraderViaChartTab, FindChartTraderBySiblingSearch, FindChartTraderByTypeName, FindChartTraderByButton, FindDescendantGrid, DumpVisualTree

### Other Methods in File
- CreatePanel (caller of PlacePanel) - Not modified
- All other UI.Panel.Construction.cs methods - Not modified

### Related Files
- src/V12_002.UI.Panel.*.cs - Not modified
- src/V12_002.cs - Not modified

## Scope Boundaries

### What Changes
1. PlacePanel method body (lines 239-319)
2. Addition of 3 new private helper methods in same file
3. Addition of unit tests in test project

### What Stays the Same
1. Method signature of PlacePanel
2. All existing helper methods (20 callees)
3. Caller relationship (CreatePanel → PlacePanel)
4. UI threading behavior
5. Retry timer mechanism

## Risk Mitigation

### Low Blast Radius Confirmed
- **External Consumers**: 0
- **Direct Dependents**: 0
- **Overall Risk Score**: 0.0

### Single Caller Simplicity
- **Caller**: CreatePanel (same file)

## Success Criteria

### Complexity Targets
- PlacePanel: CYC ≤ 8 (currently 13)
- FindChartTraderWithFallbacks: CYC ≤ 8
- LocatePlacementGrid: CYC ≤ 8
- ConfigurePlacementRetryTimer: CYC ≤ 8

### Build Requirements
- Zero compilation errors
- deploy-sync.ps1 executes successfully
- F5 in NinjaTrader loads strategy

### Test Requirements
- 3 new unit tests (one per extracted method)
- All tests pass
- xUnit framework only

### Quality Gates
- ASCII-only compliance maintained
- No lock() statements introduced
- CSharpier formatting passes
- Pre-push validation passes

## Conclusion

**Scope Status**: WELL-DEFINED
- 3 focused extractions from 1 method
- Clear IN SCOPE / OUT OF SCOPE boundaries
- Low risk (isolated method, single caller)
- High value (CYC 13 → ≤ 8)

**Recommendation**: PROCEED to Phase 2 (Architecture Planning)
