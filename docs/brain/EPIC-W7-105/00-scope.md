# Phase 1: Scope Definition - EPIC-W7-105

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.18
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T19:37:58Z

## Epic Target
- **Method**: DrainAllDispatchQueuesOnAbort
- **File**: src/V12_002.SIMA.Fleet.cs
- **Line**: 287
- **Current CYC**: 12
- **Target CYC**: 8 or less (Jane Street strict standard)

## Scope Boundary

### IN SCOPE

#### Primary Target
- **DrainAllDispatchQueuesOnAbort** method (CYC 12 to 8 or less)
  - Extract queue draining logic
  - Extract circuit breaker reset logic
  - Extract position/sync state cleanup logic
  - Extract telemetry tracking logic

#### Extraction Units (Target CYC 3 or less each)
1. **Queue Draining Logic**
   - Drain photon dispatch ring
   - Clear pending fleet dispatches
   - Track dequeue telemetry

2. **Circuit Breaker Reset Logic**
   - Reset circuit breaker state
   - Manage circuit breaker thresholds

3. **Position/Sync State Cleanup**
   - Clear expected positions
   - Clear dispatch sync pending state
   - Update position deltas

4. **Telemetry Tracking**
   - Log drain operations
   - Track account fill grace

#### File Modifications
- **src/V12_002.SIMA.Fleet.cs** (line 287)
  - Refactor DrainAllDispatchQueuesOnAbort
  - Add 3-4 extracted helper methods
  - Maintain existing method signature (0 parameters)
  - Preserve all existing functionality

### OUT OF SCOPE

#### Caller Methods (No Changes)
- **PumpFleetDispatch** (line 233) - Direct caller, no modifications
- **ProcessFleetSlot** (line 44) - Indirect caller, no modifications
- **VerifyPhotonSlotIntegrity** (line 329) - Indirect caller, no modifications
- **ProcessValidPhotonSlot** (line 395) - Indirect caller, no modifications

#### Callee Methods (No Changes)
- All 25 callee methods remain unchanged
- No modifications to data structures or helper methods

#### Other Files
- No changes to any other files in src/
- No changes to test files
- No changes to infrastructure files

#### Behavioral Changes
- No changes to method behavior
- No changes to method signature
- No changes to return values
- No changes to side effects
- No changes to error handling

## Scope Rationale

### Why This Scope?
1. **Zero Blast Radius**: No external callers means isolated refactoring
2. **High Complexity**: CYC 12 exceeds Jane Street threshold (8 or less)
3. **Clear Boundaries**: Method is self-contained within fleet management
4. **Low Risk**: All callers are in same file, easy to verify

### Why Not Broader?
1. **Callers Are Clean**: 4 caller methods do not need refactoring
2. **Callees Are Stable**: 25 callee methods are already well-factored
3. **Single Responsibility**: Focus on one method reduces risk
4. **Incremental Progress**: Smaller scope equals faster completion

## Success Criteria

### Functional Requirements
- DrainAllDispatchQueuesOnAbort maintains exact same behavior
- All 4 callers continue to work without modification
- All 25 callees continue to work without modification
- No changes to method signature (0 parameters)
- No changes to return type (void)

### Quality Requirements
- DrainAllDispatchQueuesOnAbort achieves CYC 8 or less
- Each extracted helper method achieves CYC 3 or less
- No new compilation errors
- No new runtime errors
- Build passes after refactoring

### Testing Requirements
- Existing tests continue to pass
- F5 in NinjaTrader IDE succeeds
- BUILD_TAG appears in output
- No behavioral regressions

## Risk Assessment

### Risk Level: LOW
- **Blast Radius**: ZERO (no external callers)
- **Complexity**: HIGH (CYC 12) but isolated
- **Dependencies**: STABLE (25 callees unchanged)
- **Testing**: STRAIGHTFORWARD (existing tests sufficient)

### Mitigation Strategies
1. **Preserve Behavior**: Extract without changing logic
2. **Incremental Commits**: Commit after each extraction
3. **Continuous Verification**: Build and test after each change
4. **Rollback Ready**: Git history allows easy revert

## Next Steps (Phase 2)
1. Architecture planning - design extraction sequence
2. Identify exact extraction points in method body
3. Plan helper method signatures
4. Generate tickets for each extraction unit
