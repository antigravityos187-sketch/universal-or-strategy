# Phase 0: Hotspot Analysis - EPIC-W7-026

## Agent Tracking
- **Agent Name**: v12-phase0-hotspot
- **Execution Time**: 2026-06-23T02:39:29Z
- **Bobcoins Used**: 0.74
- **API Key**: jCodemunch MCP

## Target Method
- **Method**: ProcessQueuedAccountOrder
- **File**: src/V12_002.Orders.Callbacks.AccountOrders.cs
- **Line**: 1054
- **Cyclomatic Complexity**: 17 (HIGH - exceeds threshold of 8)

## Complexity Metrics

### Raw Metrics
- **Cyclomatic Complexity**: 17
- **Max Nesting Depth**: 3
- **Parameter Count**: 1
- **Lines of Code**: 48
- **Assessment**: HIGH

### Analysis
The method has a cyclomatic complexity of 17, which is more than double the Jane Street strict standard of 8. This indicates:
- Multiple decision paths (17 distinct execution paths)
- Moderate nesting (3 levels deep)
- Substantial logic concentration (48 lines)
- Single parameter suggests focused responsibility but complex internal logic

## Blast Radius

### Impact Analysis
- **Direct Importers**: 0
- **Confirmed Dependencies**: 0
- **Potential Dependencies**: 0
- **Overall Risk Score**: 0.0 (LOW)

### Interpretation
The method has zero external blast radius, meaning:
- No other files import or directly depend on this method
- Changes are isolated to the containing file
- Refactoring risk is minimal from a dependency perspective
- This is an internal implementation detail of the AccountOrders callback system

## Call Hierarchy

### Callers (Who calls this method)
1. **ProcessAccountOrderQueue** (line 182)
   - Direct caller at depth 1
   - Queue processing orchestrator
   
2. **ProcessAccountOrder_EnqueueTerminalUpdate** (line 154)
   - Indirect caller at depth 2
   - Terminal state update handler

### Callees (What this method calls)
The method calls 48 distinct symbols across 2 depth levels:

#### Depth 1 (Direct calls - 10 symbols)
1. LogBuffer.Format (logging)
2. ProcessFollowerCancellationUnconditional (follower cleanup)
3. activePositions (state access)
4. TryFindOrderInPosition (order lookup)
5. HandleMatchedFollowerOrder (follower matching)
6. ExecuteFollowerCascadeCleanup (cascade cleanup)

#### Depth 2 (Indirect calls - 38 symbols)
- LogBuffer validation and formatting internals
- Follower replacement specs (_followerReplaceSpecs, _followerTargetReplaceSpecs)
- Multiple HandleMatchedFollower_* variants:
  - PendingCancelReplace
  - TargetReplaceCancel
  - StopReplacement
  - PendingCleanupPurge
  - DeltaRollback
- ExecuteFollowerCascade_* variants:
  - SuppressMasterReplace
  - ResolveFollowers
  - CleanupUnfilled
  - EmergencyFlattenFilled
- Order finding utilities (TryFindOrder_MatchesEntryStopOrT1, TryFindOrder_MatchesT2ThroughT5)
- Ghost order cleanup (RemoveGhostOrderRef)
- Safe cancellation (ProcessFollowerCancellationSafe)

### Call Pattern Analysis
The method exhibits a coordinator pattern:
- Orchestrates multiple follower order handling scenarios
- Delegates to specialized handlers based on order state
- Manages cascading cleanup operations
- Heavy branching logic to route to appropriate handlers

## Risk Assessment

### Overall Risk: MEDIUM

#### Risk Factors
1. LOW External Risk: Zero blast radius means refactoring will not break other files
2. HIGH Complexity Risk: CYC 17 is 2.1x the threshold (cognitive load)
3. MEDIUM Coordination Risk: 48 callees indicates complex orchestration logic
4. LOW Isolation Risk: Only 2 callers means limited entry points

#### Refactoring Recommendation
**PROCEED WITH CAUTION**

**Strengths**:
- Isolated method (no external dependencies)
- Clear entry points (only 2 callers)
- Well-defined responsibility (queue processing)

**Challenges**:
- High branching complexity (17 paths)
- Extensive delegation (48 callees)
- Likely contains multiple responsibilities that can be extracted

**Suggested Approach**:
1. Extract decision logic into separate methods (reduce branching)
2. Group related follower handling into cohesive units
3. Simplify cascade cleanup orchestration
4. Target CYC ≤ 8 per extracted method

## Hotspot Score Calculation

Using the formula: hotspot_score = cyclomatic_complexity × log(1 + churn)

**Note**: Churn data not available in Phase 0 (requires git history analysis in Phase 1)

**Estimated Hotspot Score**: 17 × log(1 + estimated_churn)
- If low churn (5 commits): 17 × 0.78 = 13.3
- If medium churn (20 commits): 17 × 1.32 = 22.4
- If high churn (50 commits): 17 × 1.71 = 29.1

## Next Steps (Phase 1)

1. Analyze git history for actual churn metrics
2. Identify specific branching patterns to extract
3. Map follower handling scenarios to extraction candidates
4. Define scope boundary (which branches to extract vs. keep)
5. Validate no hidden dependencies via deeper code analysis
