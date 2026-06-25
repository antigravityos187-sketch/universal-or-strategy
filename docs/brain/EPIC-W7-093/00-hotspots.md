# Phase 0: Hotspot Analysis - EPIC-W7-093

## Agent Tracking
- **Agent Name**: v12-phase0-hotspot
- **Bobcoins Used**: 0.75
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-23T02:52:16Z

## Target Method
- **Method**: Dispatch_ProcessFleetLoop
- **File**: src/V12_002.SIMA.Dispatch.cs
- **Line**: 196
- **Cyclomatic Complexity**: 20 (ACTUAL - not 14 as initially reported)

## Complexity Metrics

### Raw Metrics
- **Cyclomatic Complexity**: 20
- **Max Nesting Depth**: 5
- **Parameter Count**: 12
- **Lines of Code**: 153
- **Assessment**: HIGH

### Analysis
The method has HIGH complexity (CYC=20) which exceeds the Jane Street strict standard (CYC ≤ 8) by 2.5x. This indicates:
- Complex branching logic with multiple decision paths
- Deep nesting (5 levels) suggesting nested conditionals/loops
- High parameter count (12) indicating tight coupling
- Large method body (153 lines) suggesting multiple responsibilities

## Blast Radius

### Impact Analysis
- **Importer Count**: 0
- **Direct Dependents**: 0
- **Overall Risk Score**: 0.0
- **Confirmed Files**: 0
- **Potential Files**: 0

### Interpretation
The method is **PRIVATE** and has **ZERO external blast radius**. This is IDEAL for refactoring:
- No external callers to coordinate with
- Changes are fully contained within the file
- Low risk of breaking external dependencies
- Safe to extract and refactor aggressively

## Call Hierarchy

### Callers (1)
1. **ExecuteSmartDispatchEntry** (src/V12_002.SIMA.Dispatch.cs:45)
   - Single entry point
   - AST-resolved (high confidence)

### Callees (88 total)
The method calls 88 downstream symbols across multiple categories:

#### Fleet Management (4 calls)
- ShouldSkipFleetAccount (2 variants)
- ShouldSkipFleet_RunHealthCheck (2 variants)
- ShouldSkipFleet_IsConsistencyLockHit (2 variants)

#### Order Building (4 calls)
- Dispatch_BuildFollowerOrders (2 variants)
- Dispatch_PublishMarketBracketToPhoton (2 variants)
- Dispatch_PublishLimitEntryToPhoton (2 variants)

#### State Management (4 calls)
- ClearDispatchSyncPending (2 variants)
- AddExpectedPositionDeltaLocked (2 variants)
- MarkDispatchSyncPending (2 variants)

#### Photon Pool Management (10 calls)
- ClaimPhotonPoolSlot
- PopulatePhotonSlot
- EnqueueToPhotonRing
- EnqueueLimitEntryToPhotonRing
- ComputeFleetDispatchShadow (2 variants)
- TryIncrementDispatchCountWithCircuitBreaker (2 variants)

#### Order Publishing (6 calls)
- PublishPhoton_StopOrder
- PublishPhoton_TargetOrders
- RegisterTrackingDictionaries
- InitializeFollowerBracketFSM (3 variants)

#### Calculation Logic (12 calls)
- CalculateATRStopDistance (4 variants)
- CalculateTargetPrice (2 variants)
- GetTargetDistribution (4 variants)
- ValidateStopPrice (2 variants)

#### Position/Target Management (6 calls)
- GetTargetContracts (2 variants)
- IsRunnerTarget (2 variants)
- GetTargetPrice (2 variants)

#### Logging & Telemetry (4 calls)
- LogDispatchCompletion
- LogBuffer.Format (2 variants)
- TrackPhotonPoolExhausted

#### Symmetry & Utilities (10 calls)
- ExpKey (2 variants)
- SymmetryTrim (2 variants)
- SymmetryGuardRegisterFollower (2 variants)
- GetStableHash (2 variants)
- StampAccountFillGrace (2 variants)

#### Data Structures (18 calls)
- activePositions (2 variants)
- entryOrders (2 variants)
- stopOrders (2 variants)
- GetTargetOrdersDictionary (2 variants)
- _followerBrackets (2 variants)
- _photonPool (2 variants)
- _photonDispatchRing (2 variants)
- _photonMmioMirror (2 variants)
- _pendingFleetDispatches (2 variants)

## Risk Assessment

### Overall Risk: **MEDIUM-LOW**

**Rationale**:
1. Zero External Blast Radius: Private method with no external callers
2. Single Entry Point: Only called by ExecuteSmartDispatchEntry
3. High Internal Complexity: CYC=20 with 88 downstream calls
4. Deep Nesting: 5 levels suggests complex control flow
5. Large Method: 153 lines indicates multiple responsibilities

### Refactoring Opportunity: **EXCELLENT**

**Why This Is a Good Target**:
- Private scope = safe to refactor without coordination
- High complexity = significant cognitive load reduction potential
- 88 callees = clear extraction candidates (fleet validation, order building, photon management)
- Single caller = easy to test changes
- No external dependencies = low risk of breaking changes

### Recommended Approach
1. Extract Fleet Validation Logic (ShouldSkip* calls)
2. Extract Order Building Logic (Dispatch_BuildFollowerOrders flow)
3. Extract Photon Management (pool claiming, slot population, ring enqueue)
4. Extract Calculation Logic (ATR, targets, prices)
5. Simplify Main Loop to orchestration-only (CYC ≤ 8)

### Success Criteria
- Reduce CYC from 20 to ≤ 8
- Extract 4-6 helper methods with single responsibilities
- Maintain 100% behavioral equivalence
- Add unit tests for extracted methods
- Verify with F5 in NinjaTrader IDE

## Next Steps (Phase 1)
1. Define scope boundary (which extractions to include)
2. Validate scope does not creep beyond fleet loop refactoring
3. Proceed to Phase 2 (Architecture Planning)
