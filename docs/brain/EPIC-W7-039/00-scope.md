# Phase 1: Scope Definition - EPIC-W7-039

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.00
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T20:06:16Z

## Target Method
- **Method**: ManageTrailingStops
- **File**: src/V12_002.Trailing.cs
- **Line**: 39
- **Current CYC**: 15
- **Target CYC**: ≤8 (Jane Street strict standard)

## Scope Boundary Analysis

### IN SCOPE

#### Primary Extraction Target
**ManageTrailingStops orchestration logic** (CYC=15 → ≤3)

**Extraction Candidates** (7 methods):
1. **Extract throttling check** (CYC ≤2)
   - Isolate ManageTrail_AdaptiveThrottleTick call
   - Return early if throttled

2. **Extract symmetry guard** (CYC ≤2)
   - Isolate SymmetryGuardIsAnchorPending check
   - Return early if guard active

3. **Extract branch selection logic** (CYC ≤3)
   - Isolate decision tree for per-trade vs point-based vs fleet sync
   - Return branch identifier

4. **Extract per-trade orchestration** (CYC ≤3)
   - Wrap ManageTrail_RunPerTradeBranches call
   - Add pre/post validation

5. **Extract point-based orchestration** (CYC ≤3)
   - Wrap ManageTrail_RunPointBasedTrailing call
   - Add pre/post validation

6. **Extract fleet sync orchestration** (CYC ≤3)
   - Wrap ManageTrail_RunFleetSymmetrySync call
   - Add pre/post validation

7. **Extract shadow engine validation** (CYC ≤2)
   - Wrap ShadowEngineCheck call
   - Return early if validation fails

#### Scope Justification
- **Zero blast radius**: No external dependencies, safe to refactor
- **Low churn**: Stable implementation, minimal merge conflict risk
- **High complexity**: CYC=15 exceeds Jane Street threshold by 87.5%
- **Orchestrator pattern**: Clear separation between coordination and execution
- **82 callees**: High coordination responsibility warrants extraction

### OUT OF SCOPE

#### Excluded from This Epic
1. **Downstream callees** (82 methods across 3 levels)
   - ManageTrail_AdaptiveThrottleTick
   - ManageTrail_RunPerTradeBranches
   - ManageTrail_RunPointBasedTrailing
   - ManageTrail_RunFleetSymmetrySync
   - ShadowEngineCheck
   - All depth-2 and depth-3 callees
   - **Rationale**: These are already extracted and have their own complexity profiles

2. **Position state management**
   - activePositions access
   - **Rationale**: Core state management, not part of orchestration logic

3. **Other methods in V12_002.Trailing.cs**
   - **Rationale**: Focus on single method to minimize scope creep

4. **Integration with other V12_002 partials**
   - **Rationale**: No cross-file dependencies detected

5. **Test file modifications**
   - **Rationale**: Tests will be added in Phase 5 (Ticket Execution)

## Extraction Strategy

### Phase 2 Architecture Plan
1. Create 7 private helper methods
2. Each helper has single responsibility
3. Main method becomes pure orchestrator (CYC ≤3)
4. Preserve existing call signatures
5. No changes to downstream callees

### Expected Outcome
- **Before**: 1 method, CYC=15, 59 lines
- **After**: 8 methods (1 orchestrator + 7 helpers), each CYC ≤3, total ~75 lines
- **Complexity Reduction**: 80% per-method reduction
- **Maintainability**: Improved testability and cognitive load

## Scope Validation

### Boundary Checks
- Single file: src/V12_002.Trailing.cs only
- Single method: ManageTrailingStops only
- No downstream changes: 82 callees remain untouched
- No upstream changes: Zero callers (timer-driven)
- No cross-file impact: Zero blast radius confirmed

### Risk Mitigation
- **Low blast radius**: Zero external dependencies
- **Stable churn**: Not in top 50 hotspots
- **Clear boundaries**: Orchestrator pattern with well-defined callees
- **Reversible**: Can revert extraction if issues arise

## Success Criteria
- 7 helper methods extracted with CYC ≤3 each
- Main method reduced to CYC ≤3
- All existing tests pass
- No changes to downstream callees
- Build passes with deploy-sync.ps1
- F5 in NinjaTrader successful

## Notes
- Method is timer-driven orchestrator (no static callers detected)
- 82 downstream callees indicate high coordination responsibility
- Zero blast radius makes this ideal for refactoring
- Jane Street threshold (CYC ≤8) requires 87.5% complexity reduction
- Extraction preserves existing behavior, only improves structure
