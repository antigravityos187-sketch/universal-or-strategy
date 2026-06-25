# Phase 0: Hotspot Analysis - EPIC-W7-136

## Agent Tracking
- **Agent Name**: v12-phase0-hotspot
- **Bobcoins Used**: 0.78
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-23T03:00:19Z

## Target Method
- **Method**: ManageTrailingStops
- **File**: src/V12_002.Trailing.cs
- **Line**: 39
- **Cyclomatic Complexity**: 15 (HIGH)
- **Max Nesting Depth**: 3
- **Parameter Count**: 0
- **Lines of Code**: 59

## Complexity Metrics

### Symbol Complexity Analysis
Cyclomatic: 15, Max Nesting: 3, Param Count: 0, Lines: 59, Assessment: high

**Assessment**: HIGH complexity (CYC=15 exceeds Jane Street threshold of 8)

### Complexity Breakdown
- **Cyclomatic Complexity**: 15 decision points
- **Nesting Depth**: 3 levels (acceptable)
- **Method Length**: 59 lines (moderate)
- **Parameters**: 0 (good - no parameter coupling)

## Blast Radius

### Import Analysis
- **Direct Importers**: 0
- **Direct Dependents**: 0
- **Overall Risk Score**: 0.0

**Blast Radius**: MINIMAL - This is an internal method with no external callers. Changes are isolated to the trailing stop subsystem.

## Call Hierarchy

### Callers (Upstream)
- **Count**: 0
- **Analysis**: ManageTrailingStops is NOT called by any other method in the indexed codebase.

### Callees (Downstream)
- **Count**: 82 methods
- **Depth**: 3 levels

#### Direct Callees (Depth 1)
1. ManageTrail_AdaptiveThrottleTick - Throttling logic
2. activePositions - Position state access
3. SymmetryGuardIsAnchorPending - Fleet symmetry guard
4. ManageTrail_RunPerTradeBranches - Per-trade trailing logic
5. ManageTrail_RunPointBasedTrailing - Point-based trailing
6. ManageTrail_RunFleetSymmetrySync - Fleet synchronization
7. ShadowEngineCheck - Shadow engine validation

#### Key Callees (Depth 2)
- CleanupStalePendingReplacements - Cleanup logic
- TrailHandler_TREND_E1 - Trend handler E1
- TrailHandler_TREND_E2 - Trend handler E2
- TrailHandler_RETEST - Retest handler
- ManageTrail_CalculateProfitPoints - Profit calculation
- ManageTrail_EvaluateManualBreakeven - Breakeven logic
- ManageTrail_ApplyPointBasedCascade - Cascade application
- UpdateStopOrder - Stop order updates
- FleetSync_FindLeaderMaxLevels - Fleet sync
- ShadowPropagateStopMoves - Shadow propagation

#### Deep Callees (Depth 3)
- CreateNewStopOrder - Order creation
- RestoreCascadedTargets - Target restoration
- ValidateStopPrice - Price validation
- InitiateStopReplacement - Replacement initiation
- ShadowMoveFollowerStops - Follower stop moves
- FlattenAllApexAccounts - Emergency flatten

## Hotspot Context

ManageTrailingStops does NOT appear in the top 50 hotspots by hotspot score.

**Top 5 Hotspots for Reference**:
1. HydrateFromOpenPositions (CYC=34, score=120.88)
2. IsCommandForThisInstrument (CYC=38, score=109.83)
3. HandleTerminated (CYC=30, score=102.04)
4. SweepBrokerOrders (CYC=28, score=99.55)
5. HydrateWorkingOrdersFromBroker (CYC=23, score=81.77)

## Risk Assessment

### Overall Risk: MEDIUM

**Risk Factors**:
1. LOW Blast Radius: No external callers, changes are isolated
2. HIGH Complexity: CYC=15 exceeds Jane Street threshold (8)
3. MODERATE Nesting: Max depth of 3 is acceptable
4. HIGH Coordination: Calls 82 downstream methods
5. LOW Churn: Not in top 50 hotspots (stable code)

### Refactoring Recommendation: PROCEED WITH CAUTION

**Rationale**:
- Pros: Isolated method with no external dependencies, stable codebase
- Cons: High complexity (15 decision points), deep call tree (82 callees)
- Strategy: Extract decision logic into smaller helper methods (CYC ≤ 8 each)

## Conclusion

ManageTrailingStops is a MEDIUM-RISK refactoring target:
- Isolated (no external callers)
- Stable (low churn)
- Complex (CYC=15)
- Deep call tree (82 callees)

**Recommendation**: Proceed with extraction, focusing on reducing cyclomatic complexity from 15 to ≤8 through helper method extraction.
