# Phase 0: Hotspot Analysis - EPIC-W7-033

## Agent Tracking
- **Agent Name**: v12-phase0-hotspot
- **Bobcoins Used**: 0.77
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-23T02:40:54Z

## Target Method
- **Method**: FlattenSinglePosition
- **File**: src/V12_002.Orders.Management.Flatten.cs
- **Line**: 441
- **Cyclomatic Complexity**: 27 (Target: ≤8)
- **Max Nesting Depth**: 4
- **Parameter Count**: 2
- **Lines of Code**: 117
- **Assessment**: HIGH complexity

## Complexity Metrics

### Current State
- **Cyclomatic Complexity**: 27
- **Jane Street Target**: ≤8
- **Reduction Required**: 19 points (70% reduction)
- **Max Nesting Depth**: 4 (acceptable)
- **Parameter Count**: 2 (acceptable)

### Hotspot Ranking
- **Hotspot Score**: 74.86 (complexity × log(1 + churn))
- **Rank**: #8 out of top 50 hotspots
- **Churn (90 days)**: 15 commits
- **Assessment**: HIGH PRIORITY - Complex AND frequently changed

## Blast Radius Analysis

### Direct Impact
- **Importer Count**: 0
- **Direct Dependents**: 0
- **Overall Risk Score**: 0.0 (LOW)
- **Confirmed Files**: 0
- **Potential Files**: 0

### Risk Assessment
**LOW BLAST RADIUS** - Method is internally called, no external dependencies detected.

## Call Hierarchy

### Callers (Who calls this method)
1. **FlattenFilledMasterPositions** (depth 1)
   - File: src/V12_002.Orders.Management.Flatten.cs
   - Line: 424
   - Resolution: ast_resolved

2. **FlattenAll** (depth 2)
   - File: src/V12_002.Orders.Management.Flatten.cs
   - Line: 264
   - Resolution: ast_resolved

### Callees (What this method calls) - 20 dependencies
Key dependencies:
- LogBuffer.Format (logging)
- RequestStopCancelLifecycleSafe (order cancellation)
- GetTargetOrdersDictionary (target order retrieval)
- CancelOrderSafe (order cancellation gateway)
- IsOrderTerminal (order state validation)
- pendingStopReplacements (state management)
- stopOrders (order tracking)
- activePositions (position tracking)

## Risk Assessment

### Overall Risk: MEDIUM-LOW
- ✅ **Blast Radius**: LOW (0 external dependents)
- ⚠️ **Complexity**: HIGH (CYC 27, needs 70% reduction)
- ⚠️ **Churn**: MEDIUM (15 commits in 90 days)
- ✅ **Nesting**: ACCEPTABLE (depth 4)
- ✅ **Parameters**: ACCEPTABLE (2 params)

### Refactoring Safety
- **Safe to refactor**: YES
- **Rationale**: Low blast radius, internal method, well-contained
- **Recommended approach**: Extract decision logic into helper methods
- **Test coverage**: Required before extraction

## Recommended Extraction Strategy

### Phase 1: Extract Decision Logic (Target CYC ≤8 per method)
1. Extract stop order validation logic
2. Extract target order cancellation logic
3. Extract position state validation logic
4. Extract emergency flatten logic

### Phase 2: Simplify Control Flow
1. Replace nested if/else with early returns
2. Use guard clauses for preconditions
3. Consolidate duplicate cancellation calls

### Phase 3: Verify
1. Unit tests for each extracted method
2. Integration test for FlattenSinglePosition
3. Complexity audit verification (CYC ≤8)

## Success Criteria for EPIC-W7-033
- [ ] FlattenSinglePosition reduced to CYC ≤8
- [ ] All extracted methods have CYC ≤8
- [ ] Unit tests for all extracted methods
- [ ] Integration test passes
- [ ] No regression in flatten behavior
- [ ] Build passes
- [ ] deploy-sync.ps1 executed successfully
