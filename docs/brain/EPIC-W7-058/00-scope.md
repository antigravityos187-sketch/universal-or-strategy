# Phase 1: Scope Definition - EPIC-W7-058

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.00
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T19:31:10Z
- **Input Artifact**: docs/brain/EPIC-W7-058/00-hotspots.md

## Target Method
- **Method**: MapOrderStateToFSMState
- **File**: src/V12_002.SIMA.Lifecycle.cs
- **Line**: 469
- **Cyclomatic Complexity**: 13 (exceeds threshold by 5)
- **Risk Level**: MEDIUM

## Scope Boundary Definition

### IN SCOPE

#### Primary Target
1. **MapOrderStateToFSMState method** (Line 469)
   - Extract switch/case branches into helper methods
   - Target: Reduce main method to CYC <= 8
   - Pattern: State mapping table or named helper methods

#### Extraction Strategy
1. **State Mapping Extraction**
   - Replace switch/case with Dictionary<OrderState, FollowerBracketState>
   - OR extract each case into named helper method
   - Main method becomes simple lookup/dispatch (CYC <= 3)
   - Each helper method: CYC <= 2

2. **Affected Callers** (verification only, no modification)
   - HydrateFSMsFromWorkingOrders (Line 787)
   - HydrateWorkingOrdersFromBroker (Line 309)
   - EnumerateApexAccounts (Line 140)

#### Testing Requirements
1. Unit tests for each extracted state mapping path
2. Integration tests for all 3 caller methods
3. Verify state transitions remain correct

### OUT OF SCOPE

#### Explicitly Excluded
1. **Caller Methods** - No modifications to:
   - HydrateFSMsFromWorkingOrders
   - HydrateWorkingOrdersFromBroker
   - EnumerateApexAccounts

2. **Other SIMA.Lifecycle Methods** - No changes to:
   - Other methods in V12_002.SIMA.Lifecycle.cs
   - FSM state management logic
   - Order hydration logic

3. **External Files** - Zero blast radius confirmed:
   - No other files import this method
   - No cross-file changes needed

4. **Enum Definitions** - No changes to:
   - OrderState enum
   - FollowerBracketState enum

5. **Performance Optimization** - Not in scope:
   - Caching strategies
   - Performance tuning
   - Memory optimization

## Scope Justification

### Why This Scope?
1. **Isolated Impact**: Zero external dependencies (blast radius = 0)
2. **Single File**: All callers in same file (easy verification)
3. **Leaf Method**: No downstream calls to break
4. **Clear Boundary**: State mapping logic is self-contained
5. **Jane Street Alignment**: CYC 13 to CYC <= 8 (strict standard)

### Risk Mitigation
- **LOW BLAST RADIUS**: No external files affected
- **ISOLATED SCOPE**: Changes contained to single method
- **VERIFICATION**: Only 3 callers to test
- **ROLLBACK**: Easy to revert if issues arise

## Success Criteria

### Phase 2 (Architecture Planning)
- Detailed extraction plan with helper method signatures
- State mapping table design (if using Dictionary approach)
- Test coverage plan for all 13 decision paths

### Phase 5 (Ticket Execution)
- Main method reduced to CYC <= 8
- All extracted helpers have CYC <= 2
- Unit tests pass for all state mappings
- All 3 callers verified functional
- Build passes with zero errors
- deploy-sync.ps1 executed successfully

## Scope Boundary Validation

### Boundary Checks
- **Single Method Focus**: Only MapOrderStateToFSMState targeted
- **No Caller Modifications**: Callers remain unchanged
- **No External Files**: Zero cross-file changes
- **No Enum Changes**: State definitions unchanged
- **Clear Exit Criteria**: CYC <= 8 achieved

### Scope Creep Prevention
- Any changes beyond MapOrderStateToFSMState require Director approval
- Caller modifications trigger scope violation alert
- External file changes trigger immediate halt
- Enum modifications require separate epic

## Next Phase Input
**Phase 2 (Architecture Planning)** will receive:
- This scope definition (00-scope.md)
- Hotspot analysis (00-hotspots.md)
- Scope boundary validation (above)

**Expected Output**: 02-architecture-plan.md with detailed extraction strategy
