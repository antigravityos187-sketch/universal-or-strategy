# Phase 1: Scope Definition - EPIC-W7-136

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.18
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T19:42:47Z

## Epic Overview
**Target**: ManageTrailingStops method complexity reduction
**File**: src/V12_002.Trailing.cs
**Current CYC**: 15
**Target CYC**: <=8 (Jane Street threshold)

## Scope Boundary Analysis

### IN SCOPE

#### Primary Target
- **ManageTrailingStops** (line 39, CYC=15)
  - Extract decision logic into helper methods
  - Reduce cyclomatic complexity from 15 to <=8
  - Maintain existing behavior (no logic changes)

#### Extraction Candidates (Based on Call Hierarchy)
1. **Throttling Logic**
   - ManageTrail_AdaptiveThrottleTick
   - Extract throttle decision logic

2. **Guard Conditions**
   - SymmetryGuardIsAnchorPending
   - Extract fleet symmetry guard checks

3. **Branch Routing**
   - ManageTrail_RunPerTradeBranches
   - ManageTrail_RunPointBasedTrailing
   - ManageTrail_RunFleetSymmetrySync
   - Extract branch selection logic

4. **Validation**
   - ShadowEngineCheck
   - Extract shadow engine validation

#### Scope Constraints
- **Max Methods Extracted**: 4-6 helper methods
- **Max CYC per Helper**: <=8
- **Preserve**: All existing behavior, no logic changes
- **Maintain**: Zero external callers (isolated refactor)

### OUT OF SCOPE

#### Downstream Methods (82 callees)
- **Depth 2 Methods**: TrailHandler_TREND_E1, TrailHandler_TREND_E2, etc.
- **Depth 3 Methods**: CreateNewStopOrder, RestoreCascadedTargets, etc.
- **Rationale**: These are already extracted and have their own complexity profiles

#### Related Subsystems
- **Fleet Synchronization**: FleetSync_FindLeaderMaxLevels, ShadowMoveFollowerStops
- **Order Management**: UpdateStopOrder, CreateNewStopOrder
- **Cleanup Logic**: CleanupStalePendingReplacements
- **Rationale**: Separate concerns, not part of ManageTrailingStops complexity

#### Other Trailing Methods
- **ManageTrail_AdaptiveThrottleTick**: Already extracted (called by target)
- **ManageTrail_RunPerTradeBranches**: Already extracted (called by target)
- **ManageTrail_RunPointBasedTrailing**: Already extracted (called by target)
- **ManageTrail_RunFleetSymmetrySync**: Already extracted (called by target)
- **Rationale**: These are callees, not part of the target method complexity

#### Test Files
- **Rationale**: No test changes required (behavior preservation)

#### Documentation
- **Rationale**: Code-only refactor, no API changes

## Extraction Strategy

### Phase 2 Architecture Plan Will Define:
1. **Helper Method Signatures**: Extract decision points into named helpers
2. **Complexity Distribution**: Ensure each helper has CYC <=8
3. **Call Flow**: Maintain existing execution order
4. **Error Handling**: Preserve all existing error paths

### Success Criteria
- ManageTrailingStops CYC reduced from 15 to <=8
- All extracted helpers have CYC <=8
- Zero behavior changes (logic preservation)
- Zero external caller impact (isolated refactor)
- Build passes after extraction
- F5 in NinjaTrader successful

## Risk Mitigation

### Low Blast Radius
- **0 external callers**: Changes are fully isolated
- **No API changes**: Internal refactor only
- **Stable code**: Not in top 50 hotspots (low churn)

### High Coordination Risk
- **82 downstream callees**: Must preserve all call sites
- **Mitigation**: Extract only decision logic, not call orchestration

### Complexity Reduction Target
- **Current**: 15 decision points
- **Target**: <=8 decision points in main method
- **Strategy**: Extract 4-6 helpers with CYC <=8 each

## Scope Validation

### Boundary Checks
- Target method identified: ManageTrailingStops
- Complexity threshold: CYC=15 exceeds 8
- Blast radius: 0 external callers (isolated)
- Extraction candidates: 4-6 decision points
- Out of scope: 82 downstream callees excluded

### Jane Street Alignment
- Cognitive simplicity: Reduce decision points
- Testability: Smaller methods easier to test
- Auditability: Clearer logic flow for race condition review

## Next Phase
**Phase 1.5**: Scope Boundary Validation (Jane Street gate)
- Verify no scope creep
- Confirm extraction candidates
- Validate complexity distribution plan
