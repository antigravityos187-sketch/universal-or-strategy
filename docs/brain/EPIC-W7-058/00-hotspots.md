# Phase 0: Hotspot Analysis - EPIC-W7-058

## Agent Tracking
- **Agent Name**: v12-phase0-hotspot
- **Bobcoins Used**: 0.93
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-23T02:45:46Z

## Target Method
- **Method**: MapOrderStateToFSMState
- **File**: src/V12_002.SIMA.Lifecycle.cs
- **Line**: 469
- **Cyclomatic Complexity**: 13
- **Assessment**: HIGH

## Complexity Metrics

### Symbol Details
- **Kind**: method
- **Cyclomatic Complexity**: 13
- **Max Nesting Depth**: 1
- **Parameter Count**: 1
- **Lines of Code**: 25
- **Assessment**: HIGH (threshold: CYC ≤ 8)

### Complexity Analysis
The method has a cyclomatic complexity of 13, which exceeds the Jane Street strict standard of CYC ≤ 8 by 5 points. This indicates:
- **Cognitive Load**: HIGH - Multiple decision paths make reasoning difficult
- **Test Coverage**: Requires 13+ test cases for exhaustive path coverage
- **Maintenance Risk**: HIGH - Changes have high probability of introducing bugs

## Blast Radius

### Direct Impact
- **Importer Count**: 0
- **Direct Dependents**: 0
- **Overall Risk Score**: 0.0
- **Confirmed Files**: 0
- **Potential Files**: 0

### Analysis
The blast radius analysis shows **ZERO external dependencies**, meaning:
- ✅ This method is NOT imported by other files
- ✅ No confirmed or potential downstream consumers
- ✅ Changes are isolated to the containing file
- ✅ **LOW RISK** for refactoring - no external breakage possible

## Call Hierarchy

### Callers (Who calls this method)
1. **HydrateFSMsFromWorkingOrders** (depth 1)
   - File: src/V12_002.SIMA.Lifecycle.cs
   - Line: 787
   - Resolution: ast_resolved

2. **HydrateWorkingOrdersFromBroker** (depth 2)
   - File: src/V12_002.SIMA.Lifecycle.cs
   - Line: 309
   - Resolution: ast_resolved

3. **EnumerateApexAccounts** (depth 3)
   - File: src/V12_002.SIMA.Lifecycle.cs
   - Line: 140
   - Resolution: ast_resolved

### Callees (What this method calls)
- **None** - This is a leaf method with no downstream calls

### Call Chain Analysis
The method sits in a 3-level call chain:
```
EnumerateApexAccounts (L140)
  └─> HydrateWorkingOrdersFromBroker (L309)
      └─> HydrateFSMsFromWorkingOrders (L787)
          └─> MapOrderStateToFSMState (L469) ← TARGET
```

All callers are in the **same file** (V12_002.SIMA.Lifecycle.cs), confirming the isolated nature of this method.

## Hotspot Ranking

### Repository-Wide Context
- **Rank**: #35 out of 50 top hotspots
- **Hotspot Score**: 46.2195
- **Churn (90 days)**: 34 commits
- **Formula**: CYC × log(1 + churn) = 13 × log(1 + 34) = 46.22

### Comparative Analysis
Top 5 hotspots for reference:
1. HydrateFromOpenPositions (CYC 34, score 120.88)
2. IsCommandForThisInstrument (CYC 38, score 109.83)
3. HandleTerminated (CYC 30, score 102.04)
4. SweepBrokerOrders (CYC 28, score 99.55)
5. HydrateWorkingOrdersFromBroker (CYC 23, score 81.77)

**MapOrderStateToFSMState** is in the **middle tier** of complexity hotspots, with moderate churn activity.

## Risk Assessment

### Overall Risk: **MEDIUM**

#### Risk Factors
✅ **LOW BLAST RADIUS**: Zero external dependencies
✅ **ISOLATED SCOPE**: All callers in same file
✅ **LEAF METHOD**: No downstream calls to break
⚠️ **HIGH COMPLEXITY**: CYC 13 exceeds threshold by 5
⚠️ **MODERATE CHURN**: 34 commits in 90 days
⚠️ **COGNITIVE LOAD**: 13 decision paths to reason about

#### Refactoring Recommendation
**PROCEED WITH CONFIDENCE**

This is an **ideal extraction candidate** because:
1. Zero external blast radius eliminates cross-file breakage risk
2. All callers are in the same file (easy to verify)
3. Leaf method means no cascading changes needed
4. Moderate hotspot ranking suggests active but not critical code

#### Suggested Approach
1. Extract switch/case branches into separate helper methods
2. Target CYC ≤ 8 per extracted method
3. Maintain single responsibility per helper
4. Add unit tests for each extracted path
5. Verify all 3 callers still function correctly

## Sequential Thinking Analysis

### Complexity Breakdown
The method likely contains a large switch/case or if/else chain mapping OrderState enum values to FollowerBracketState enum values. With CYC=13 and max_nesting=1, this suggests:
- 13 distinct decision branches (likely 13 OrderState cases)
- Shallow nesting (good - no nested conditionals)
- Single parameter (OrderState entryState)
- Returns nullable FollowerBracketState

### Extraction Strategy
**Pattern**: State Mapping Table
- Replace switch/case with Dictionary<OrderState, FollowerBracketState>
- Or extract each case into a named helper method
- Reduce main method to simple lookup/dispatch logic
- Target: Main method CYC ≤ 3, helpers CYC ≤ 2

## Success Criteria for Phase 1

✅ **Scope Definition Ready**: All metrics gathered
✅ **Risk Profile Clear**: MEDIUM risk, HIGH confidence
✅ **Blast Radius Known**: Zero external dependencies
✅ **Call Hierarchy Mapped**: 3 callers, 0 callees
✅ **Hotspot Context**: Rank #35, score 46.22

**READY FOR PHASE 1: SCOPE DEFINITION**
