# Phase 1: Scope Definition - EPIC-W7-036

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.18
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T19:27:58Z
- **Input**: docs/brain/EPIC-W7-036/00-hotspots.md

## Epic Objective
Reduce cyclomatic complexity of `MoveStop_SinglePosition` from 21 to ≤8 through surgical extraction of sub-methods, aligned with Jane Street strict standard.

## Target Method
- **Method**: MoveStop_SinglePosition
- **File**: src/V12_002.Trailing.Breakeven.cs
- **Line**: 73
- **Current CYC**: 21
- **Target CYC**: ≤8 per method
- **Lines of Code**: 91
- **Max Nesting Depth**: 5

## Scope Boundaries

### IN SCOPE ✅

#### Primary Target
1. **MoveStop_SinglePosition method** (lines 73-164)
   - Extract validation logic
   - Extract stop order lookup logic
   - Extract pending replacement handling logic
   - Extract stop replacement initiation logic
   - Extract error handling logic

#### Extraction Strategy
2. **Sub-method extractions** (3-5 methods)
   - Each extracted method must have CYC ≤8
   - Each method must have single responsibility
   - Preserve exact behavior (no logic changes)
   - Maintain parameter passing patterns

#### Testing Requirements
3. **Unit tests** (TDD approach)
   - Test each extracted method independently
   - Cover all decision paths
   - Verify behavior preservation

#### Verification
4. **Build and deployment**
   - Run dotnet build after each extraction
   - Run deploy-sync.ps1 after changes
   - F5 in NinjaTrader IDE to verify runtime behavior
   - Verify BUILD_TAG appears in output

### OUT OF SCOPE ❌

#### Caller Method
1. **MoveStopsToBreakevenWithOffset** (line 41)
   - This is the single caller method
   - Will NOT be modified in this epic
   - Separate epic required if CYC exceeds threshold

#### Callee Methods
2. **UpdateStopOrder** (src/V12_002.Trailing.StopUpdate.cs:84)
   - Already extracted in separate file
   - Will NOT be modified in this epic

3. **MarkStickyDirty** (src/V12_002.StickyState.cs:619)
   - State management method
   - Will NOT be modified in this epic

4. **LogBuffer methods** (src/V12_002.Perf.LogBuffer.cs)
   - Performance logging infrastructure
   - Will NOT be modified in this epic

5. **All 26 callee methods**
   - These are dependencies, not targets
   - Will NOT be modified in this epic
   - Separate epics required if they exceed CYC threshold

#### Other Files
6. **V12_002.Trailing.StopUpdate.cs**
   - Contains callee methods (UpdateStopOrder, etc.)
   - Will NOT be modified in this epic

7. **V12_002.StickyState.cs**
   - State persistence layer
   - Will NOT be modified in this epic

8. **V12_002.Perf.LogBuffer.cs**
   - Performance logging layer
   - Will NOT be modified in this epic

#### Infrastructure
9. **Build scripts**
   - deploy-sync.ps1 (used but not modified)
   - build_readiness.ps1 (used but not modified)

10. **Documentation**
    - AGENTS.md files (updated after epic completion)
    - Jane Street KB (reference only)

### BOUNDARY VALIDATION

#### Scope Creep Prevention
- **ONE EPIC = ONE METHOD**: Only MoveStop_SinglePosition
- **NO CALLER CHANGES**: MoveStopsToBreakevenWithOffset unchanged
- **NO CALLEE CHANGES**: All 26 callees unchanged
- **NO INFRASTRUCTURE CHANGES**: Build scripts unchanged
- **NO CROSS-FILE CHANGES**: Only V12_002.Trailing.Breakeven.cs modified

#### Scope Justification
1. **Complexity**: CYC 21 exceeds Jane Street threshold by 13 points
2. **Blast Radius**: Zero external importers (safe to refactor)
3. **Churn**: Low git activity (stable method)
4. **Testing**: High path count (2^21 theoretical paths) requires TDD
5. **Cognitive Load**: 5-level nesting difficult to reason about

## Extraction Plan (High-Level)

### Proposed Sub-Methods (3-5 extractions)
1. **ValidateStopMoveParameters** (CYC ≤3)
   - Validate input parameters
   - Check position state
   - Early return on invalid conditions

2. **FindExistingStopOrder** (CYC ≤3)
   - Lookup stop order from stopOrders collection
   - Handle missing order case
   - Return order or null

3. **HandlePendingStopReplacement** (CYC ≤5)
   - Check pendingStopReplacements collection
   - Handle stale replacements
   - Update existing replacements
   - Return replacement status

4. **InitiateStopMove** (CYC ≤5)
   - Calculate new stop price
   - Validate stop price
   - Initiate stop replacement
   - Handle direct stop order creation

5. **HandleStopMoveError** (CYC ≤3)
   - Catch and log exceptions
   - Mark sticky state dirty
   - Return error status

### Expected CYC Reduction
- **Before**: MoveStop_SinglePosition CYC 21
- **After**: 
  - MoveStop_SinglePosition (orchestrator) CYC ≤8
  - ValidateStopMoveParameters CYC ≤3
  - FindExistingStopOrder CYC ≤3
  - HandlePendingStopReplacement CYC ≤5
  - InitiateStopMove CYC ≤5
  - HandleStopMoveError CYC ≤3
- **Total**: All methods ≤8 (Jane Street compliant)

## Risk Assessment

### Refactoring Risks
1. **Behavior Preservation**: MEDIUM
   - 91 lines of complex logic
   - Must preserve exact behavior
   - Mitigation: TDD with comprehensive tests

2. **Integration Risk**: LOW
   - Single caller (MoveStopsToBreakevenWithOffset)
   - Zero external importers
   - Mitigation: F5 verification in NinjaTrader

3. **Testing Risk**: HIGH
   - No existing unit tests
   - 2^21 theoretical paths
   - Mitigation: TDD approach with path coverage

4. **Deployment Risk**: LOW
   - Standard deploy-sync.ps1 process
   - BUILD_TAG verification
   - Mitigation: F5 verification

### Success Criteria
1. ✅ All extracted methods have CYC ≤8
2. ✅ MoveStop_SinglePosition orchestrator has CYC ≤8
3. ✅ All unit tests pass
4. ✅ Build succeeds (dotnet build)
5. ✅ deploy-sync.ps1 succeeds
6. ✅ F5 in NinjaTrader shows BUILD_TAG
7. ✅ No behavior changes (pure refactoring)

## Next Steps (Phase 2: Architecture Planning)
1. Read full source of MoveStop_SinglePosition (91 lines)
2. Map each decision point to CYC contributors
3. Design extraction boundaries with clear interfaces
4. Define parameter passing patterns
5. Create test specifications for each sub-method
6. Generate Mermaid diagrams for before/after call flow
7. Estimate effort and risk per extraction

## Scope Approval
- **Scope Defined**: 2026-06-24T19:27:58Z
- **Boundary Validated**: ✅ No scope creep detected
- **Ready for Phase 2**: ✅ Architecture Planning
