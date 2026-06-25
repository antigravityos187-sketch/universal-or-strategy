# Phase 0: Hotspot Analysis - EPIC-W7-039

## Agent Tracking
- **Agent Name**: v12-phase0-hotspot
- **Bobcoins Used**: 0.78
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-23T03:58:51Z

## Target Method
- **Method**: ManageTrailingStops
- **File**: src/V12_002.Trailing.cs
- **Line**: 39
- **Cyclomatic Complexity**: 15
- **Lines of Code**: 59

## Complexity Metrics

### Symbol Complexity Analysis
- **Cyclomatic Complexity**: 15 (HIGH - exceeds Jane Street threshold of 8)
- **Max Nesting Depth**: 3
- **Parameter Count**: 0
- **Assessment**: HIGH complexity

### Complexity Context
The method has CYC=15, which is 87.5% above the Jane Street strict standard (CYC ≤ 8). This indicates:
- Multiple decision paths requiring careful testing
- Higher cognitive load for maintenance
- Increased risk of race conditions in lock-free code
- Potential for hidden state interactions

## Blast Radius Analysis

### Direct Impact
- **Importer Count**: 0
- **Direct Dependents**: 0
- **Overall Risk Score**: 0.0
- **Confirmed Files**: 0
- **Potential Files**: 0

### Risk Assessment
**LOW BLAST RADIUS**: This method has zero external dependencies. No other files import or directly depend on it, making it a safe refactoring target with minimal ripple effects.

## Call Hierarchy Analysis

### Callers (Upstream)
- **Caller Count**: 0
- **Depth Analyzed**: 3 levels

**Finding**: No callers detected. This is an orchestrator method likely called from a timer or event handler not captured in static analysis.

### Callees (Downstream)
- **Callee Count**: 82
- **Depth Analyzed**: 3 levels

**Key Callees (Depth 1)**:
1. ManageTrail_AdaptiveThrottleTick - Throttling logic
2. activePositions - Position state access
3. SymmetryGuardIsAnchorPending - Fleet symmetry guard
4. ManageTrail_RunPerTradeBranches - Per-trade trailing logic
5. ManageTrail_RunPointBasedTrailing - Point-based trailing logic
6. ManageTrail_RunFleetSymmetrySync - Fleet synchronization
7. ShadowEngineCheck - Shadow engine validation

**Key Callees (Depth 2)**:
- CleanupStalePendingReplacements - Cleanup logic
- TrailHandler_TREND_E1 - Trend handler E1
- TrailHandler_TREND_E2 - Trend handler E2
- TrailHandler_RETEST - Retest handler
- ManageTrail_CalculateProfitPoints - Profit calculation
- ManageTrail_EvaluateManualBreakeven - Breakeven evaluation
- ManageTrail_ShouldCheckPointBasedTrailing - Point-based guard
- ManageTrail_ApplyPointBasedCascade - Cascade application
- UpdateStopOrder - Stop order update
- FleetSync_FindLeaderMaxLevels - Fleet leader sync
- ShadowPropagateStopMoves - Shadow propagation
- ShadowPropagateLeaderFlatten - Leader flatten propagation

**Key Callees (Depth 3)**:
- CreateNewStopOrder - Stop order creation
- RestoreCascadedTargets - Target restoration
- ValidateStopPrice - Price validation
- HandleStalePendingReplacement - Stale replacement handling
- InitiateStopReplacement - Replacement initiation
- ShadowMoveFollowerStops - Follower stop movement
- FlattenAllApexAccounts - Emergency flatten

### Call Hierarchy Insights
This is a **high-level orchestrator method** that coordinates 82 downstream operations across 3 levels:
- **Depth 1**: Core trailing stop logic branches (7 methods)
- **Depth 2**: Specialized handlers and calculations (12 methods)
- **Depth 3**: Low-level order management and validation (63 methods)

The method acts as a central dispatcher for trailing stop management, delegating to specialized handlers based on trade state and configuration.

## Hotspot Ranking Context

### Repository Hotspot Analysis (Top 50)
The target method ManageTrailingStops (CYC=15) does **NOT** appear in the top 50 hotspots. This indicates:
- **Lower churn rate** compared to top hotspots (which have CYC 13-43 with high churn)
- **Stable implementation** - not frequently modified
- **Moderate complexity** - below the highest complexity methods

**Top 5 Hotspots for Comparison**:
1. HydrateFromOpenPositions - CYC=34, hotspot_score=120.88
2. IsCommandForThisInstrument - CYC=38, hotspot_score=109.83
3. HandleTerminated - CYC=30, hotspot_score=102.04
4. SweepBrokerOrders - CYC=28, hotspot_score=99.55
5. HydrateWorkingOrdersFromBroker - CYC=23, hotspot_score=81.77

### Positioning
ManageTrailingStops is a **moderate complexity, low churn** method - ideal for refactoring:
- Complexity high enough to warrant extraction (CYC=15 vs threshold=8)
- Churn low enough to avoid merge conflicts
- Blast radius zero - safe to modify

## Risk Assessment

### Overall Risk: **LOW-MEDIUM**

**Risk Factors**:
- ✅ **Blast Radius**: LOW (0 importers, 0 dependents)
- ⚠️ **Complexity**: HIGH (CYC=15, exceeds threshold by 87.5%)
- ✅ **Churn**: LOW (not in top 50 hotspots)
- ⚠️ **Call Depth**: MEDIUM (82 callees across 3 levels)
- ✅ **Isolation**: HIGH (orchestrator with no upstream callers)

**Refactoring Safety**:
- **Safe to extract**: Zero blast radius means no external breakage risk
- **Moderate test burden**: 82 callees require comprehensive integration testing
- **Low merge conflict risk**: Stable file with low churn
- **High cognitive benefit**: Reducing CYC from 15 to ≤8 improves maintainability

## Recommended Extraction Strategy

### Phase 1: Extract Decision Logic
1. Extract throttling check
2. Extract symmetry guard
3. Extract branch selection

### Phase 2: Extract Orchestration
4. Extract per-trade logic
5. Extract point-based logic
6. Extract fleet sync logic

### Phase 3: Simplify Main Method
7. Reduce ManageTrailingStops to pure orchestration (CYC ≤ 3)

### Expected Outcome
- **Before**: 1 method, CYC=15, 59 lines
- **After**: 7 methods, each CYC ≤ 3, total ~70 lines (with extracted methods)
- **Benefit**: 80% reduction in per-method complexity, improved testability

## Success Criteria for Phase 1 (Scope Definition)
- [ ] Validate extraction targets exist in source
- [ ] Confirm no recent refactoring of this method
- [ ] Verify build passes before starting
- [ ] Document extraction boundaries

## Notes
- Method is an orchestrator with no upstream callers (likely timer-driven)
- 82 downstream callees indicate high coordination responsibility
- Zero blast radius makes this a low-risk refactoring target
- Stable churn pattern reduces merge conflict risk
- Jane Street threshold (CYC ≤ 8) requires 87.5% complexity reduction
