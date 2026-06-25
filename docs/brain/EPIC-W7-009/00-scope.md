# Phase 1: Scope Definition - EPIC-W7-009

**Agent**: v12-phase1-scope
**Execution Time**: 2026-06-24T18:58:58Z
**Input**: docs/brain/EPIC-W7-009/00-hotspots.md

## Epic Summary

**Target Method**: FindChartTraderViaChartTab
**File**: src/V12_002.UI.Panel.Helpers.cs
**Current CYC**: 9 (exceeds Jane Street threshold of 8 by 1)
**Risk Level**: LOW (zero blast radius, single caller)

## Scope Boundary Definition

### IN SCOPE

1. **Primary Target**: FindChartTraderViaChartTab method (lines 529-564)
   - Extract 5 sequential fallback strategies into separate methods
   - Reduce cyclomatic complexity from 9 to ≤8
   - Maintain existing behavior (zero regression risk)

2. **Fallback Strategies to Extract**:
   - Strategy 1: TryFindChartTabViaVisualTree
   - Strategy 2: TryFindChartTabViaLogicalTree
   - Strategy 3: TryGetChartTraderViaProperty
   - Strategy 4: TryGetChartTraderViaFields
   - Strategy 5: TryGetChartTraderViaDescendants

3. **Caller Verification**:
   - FindChartTrader (line 478) - single caller, same file
   - Verify integration after extraction

4. **Testing**:
   - Unit tests for each extracted strategy
   - Integration test for FindChartTrader → FindChartTraderViaChartTab flow

### OUT OF SCOPE

1. **Callee Methods** (already extracted, no changes needed):
   - TryFindChartTabViaVisualTree (line 726)
   - TryFindChartTabViaLogicalTree (line 739)
   - TryGetChartTraderViaProperty (line 752)
   - TryGetChartTraderViaFields (line 768)
   - TryGetChartTraderViaDescendants (line 785)
   - FindChildElementByTypeName (line 686)

2. **Other UI Helper Methods**: No changes to unrelated methods in V12_002.UI.Panel.Helpers.cs

3. **Caller Method**: FindChartTrader (line 478) - no internal changes, only verify integration

4. **Performance Optimization**: Not addressing performance, only complexity reduction

5. **Architectural Changes**: No strategy pattern or chain-of-responsibility (overkill for CYC=9)

## Extraction Strategy

### Approach: Inline Simplification

Since the method already calls 5 well-named helper methods (TryFind*/TryGet*), the complexity comes from the sequential fallback logic, not the strategies themselves.

**Refactoring Plan**:
1. Simplify the if-else chain to reduce branching
2. Use early returns to flatten nesting
3. Consolidate null checks
4. Target: CYC ≤8 (reduce by 1 point)

### Estimated Effort

- **Tickets**: 1 ticket (simple refactoring)
- **Complexity Reduction**: 9 → 8 (1 point)
- **Lines Changed**: ~10-15 lines
- **Risk**: MINIMAL (zero blast radius, single caller)

## Verification Criteria

### Build Verification
- [ ] dotnet build passes with zero errors
- [ ] deploy-sync.ps1 completes successfully
- [ ] F5 in NinjaTrader loads strategy

### Complexity Verification
- [ ] complexity_audit.py confirms CYC ≤8 for FindChartTraderViaChartTab
- [ ] No new methods exceed CYC threshold

### Functional Verification
- [ ] FindChartTrader still returns correct ChartTrader instance
- [ ] All 5 fallback strategies still execute in sequence
- [ ] No null reference exceptions in UI panel

### Test Coverage
- [ ] Unit test for FindChartTraderViaChartTab (happy path)
- [ ] Unit test for each fallback strategy (5 tests)
- [ ] Integration test for FindChartTrader → FindChartTraderViaChartTab

## Dependencies

**Prerequisite Files**:
- src/V12_002.UI.Panel.Helpers.cs (target file)
- docs/brain/EPIC-W7-009/00-hotspots.md (input)

**No External Dependencies**: Zero blast radius confirmed by Phase 0 analysis

## Success Criteria

- [x] Scope clearly defined (IN SCOPE vs OUT OF SCOPE)
- [x] Extraction strategy documented
- [x] Verification criteria specified
- [x] Effort estimated (1 ticket)
- [x] Risk assessed (MINIMAL)

**Phase 1 Status**: COMPLETED
