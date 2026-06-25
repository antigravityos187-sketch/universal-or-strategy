# Phase 0: Hotspot Analysis - EPIC-W7-027

## Agent Tracking
- **Agent Name**: v12-phase0-hotspot
- **Bobcoins Used**: 0.77
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-23T02:39:42Z

## Target Method
- **Method**: Dispatch_PublishMarketBracketToPhoton
- **File**: src/V12_002.SIMA.Dispatch.cs
- **Line**: 612
- **Stated Complexity**: 21 (from epic roadmap)
- **Actual Complexity**: 9 (from jCodemunch index)

## CRITICAL DISCREPANCY DETECTED
The epic roadmap states CYC=21, but jCodemunch reports CYC=9. This suggests:
1. The index may be stale (needs refresh)
2. The method was already refactored
3. The roadmap data is outdated

**RECOMMENDATION**: Verify index freshness before proceeding to Phase 1.

## Complexity Metrics (from jCodemunch)
- **Cyclomatic Complexity**: 9
- **Max Nesting Depth**: 3
- **Parameter Count**: 16
- **Lines of Code**: 142
- **Assessment**: medium

### Jane Street Threshold Analysis
- **Target**: CYC <= 8 (Jane Street strict standard)
- **Current**: CYC = 9
- **Delta**: +1 over threshold
- **Priority**: LOW (only 1 point over threshold)

## Blast Radius Analysis
- **Importer Count**: 0
- **Direct Dependents**: 0
- **Overall Risk Score**: 0.0
- **Confirmed Impact Files**: 0
- **Potential Impact Files**: 0

### Risk Assessment
**RISK LEVEL: LOW**

Rationale:
- Zero external dependencies (no files import this method)
- Zero direct dependents (isolated method)
- Risk score of 0.0 indicates minimal blast radius
- Changes are unlikely to cascade to other parts of the codebase

## Call Hierarchy Analysis

### Callers (Who calls this method)
1. **Dispatch_ProcessFleetLoop** (src/V12_002.SIMA.Dispatch.cs:196)
   - Resolution: ast_resolved
   - Depth: 1

2. **ExecuteSmartDispatchEntry** (src/V12_002.SIMA.Dispatch.cs:45)
   - Resolution: ast_resolved
   - Depth: 2

### Callees (What this method calls)
Total: 58 callees across 2 depth levels

**Key Dependencies (Depth 1)**:
- PublishPhoton_StopOrder
- PublishPhoton_TargetOrders
- RegisterTrackingDictionaries
- InitializeFollowerBracketFSM
- SymmetryGuardRegisterFollower
- AddExpectedPositionDeltaLocked
- ClaimPhotonPoolSlot
- PopulatePhotonSlot
- TryIncrementDispatchCountWithCircuitBreaker
- EnqueueToPhotonRing
- LogDispatchCompletion

**Secondary Dependencies (Depth 2)**:
- ValidateStopPrice
- SymmetryTrim
- GetTargetContracts
- IsRunnerTarget
- GetTargetPrice
- CreateSingleTargetOrder
- GetTargetOrdersDictionary
- MarkDispatchSyncPending
- StampAccountFillGrace
- ComputeFleetDispatchShadow
- RollbackCircuitBreakerState
- TrackPhotonEnqueue
- TrackPhotonRingFull
- TrackPhotonPoolExhausted

## Hotspot Context (Top 50 Methods)
This method does NOT appear in the top 50 hotspots (complexity x log(1 + churn)).

**Top 5 Hotspots for Reference**:
1. HydrateFromOpenPositions (CYC=34, hotspot=120.88)
2. IsCommandForThisInstrument (CYC=38, hotspot=109.83)
3. HandleTerminated (CYC=30, hotspot=102.04)
4. SweepBrokerOrders (CYC=28, hotspot=99.55)
5. HydrateWorkingOrdersFromBroker (CYC=23, hotspot=81.77)

**Implication**: This method is NOT a high-priority refactoring target based on hotspot analysis.

## Refactoring Recommendation

### Priority: LOW
**Rationale**:
1. Only 1 point over Jane Street threshold (CYC=9 vs target <=8)
2. Zero blast radius (isolated method)
3. Not in top 50 hotspots
4. Medium assessment from jCodemunch

### Suggested Approach (if proceeding)
1. **Extract Parameter Object**: 16 parameters is excessive
   - Create a BracketDispatchContext struct
   - Group related parameters (entry, stop, targets, fleet config)
   - Reduces cognitive load and improves testability

2. **Extract Validation Logic**: Separate validation from dispatch
   - Extract pre-flight checks to ValidateBracketDispatch()
   - Reduces nesting depth from 3 to 2

3. **Extract Photon Ring Logic**: Separate ring buffer operations
   - Extract EnqueueBracketToPhotonRing()
   - Isolates lock-free ring buffer complexity

### Expected Outcome
- CYC: 9 -> 6 (below threshold)
- Nesting: 3 -> 2
- Parameters: 16 -> 1 (context object)
- Lines: 142 -> ~80 (main method)

## Index Freshness Warning
CRITICAL: The discrepancy between stated CYC=21 and actual CYC=9 suggests the jCodemunch index may be stale or the roadmap is outdated. Run verify_index_freshness.py before Phase 1.

## Next Steps
1. **MANDATORY**: Verify index freshness
2. **MANDATORY**: Reconcile CYC discrepancy (21 vs 9)
3. If index is fresh and CYC=9 is correct, consider SKIPPING this epic (already below threshold)
4. If CYC=21 is correct, proceed to Phase 1 (Scope Definition)

## Phase 0 Completion
- Hotspot analysis complete
- Blast radius analysis complete
- Call hierarchy analysis complete
- Risk assessment complete
- Index freshness verification REQUIRED before Phase 1
