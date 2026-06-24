# Phase 1: Scope Boundary Definition - EPIC-W7-084

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.00
- **API Key**: N/A
- **Execution Time**: 2026-06-24T01:33:37Z

## Epic Overview
- **Target Method**: AuditFleet_CalculateExpectedActual
- **File**: src/V12_002.REAPER.Audit.cs
- **Current CYC**: 16
- **Target CYC**: ≤8 per extracted method
- **Blast Radius**: LOW (private method, no external dependencies)

## IN SCOPE

### 1. State Calculation Extraction
**Extract**: Expected vs Actual quantity calculation logic
- Separate GetFsmExpectedPosition calls
- Isolate actual position calculation
- Return structured result (not out parameters)
- **Target CYC**: ≤5

### 2. Grace Period Logic Extraction
**Extract**: Fill grace and sync pending checks
- IsReaperFillGraceActive logic
- _accountFillGraceTicks checks
- _dispatchSyncPendingExpKeys validation
- **Target CYC**: ≤4

### 3. FSM Collection Filtering
**Extract**: Follower bracket filtering logic
- _followerBrackets.Values filtering
- ExpKey matching logic
- Collection iteration patterns
- **Target CYC**: ≤5

### 4. FSM Lifecycle Management
**Extract**: Termination and cleanup logic
- TryTerminateFollowerBracket calls
- RemoveFsmOrderIdMappings cleanup
- _positionPassFailedFirstSeen state tracking
- **Target CYC**: ≤6

### 5. Result Encapsulation
**Introduce**: Value object to replace 10 out parameters
- Create AuditFleetResult struct
- Properties: expectedQty, actualQty, isGraceActive, syncPending, etc.
- Immutable design (readonly properties)
- **Benefit**: Type safety, no out parameter explosion

## OUT OF SCOPE

### 1. Caller Modifications
**Excluded**: AuditSingleFleetAccount and AuditApexPositions
- No changes to calling methods
- Maintain identical method signature (initially)
- Preserve existing call sites
- **Rationale**: Minimize blast radius, focus on target method only

### 2. FSM State Machine Changes
**Excluded**: Core FSM/Actor pattern modifications
- No changes to SIMA_FSM class
- No changes to FSM state transitions
- No changes to Enqueue patterns
- **Rationale**: FSM architecture is stable, dont introduce risk

### 3. Logging Infrastructure
**Excluded**: LogBuffer.Format modifications
- Keep existing logging calls
- No changes to log message formats
- No changes to log levels
- **Rationale**: Logging is cross-cutting concern, separate epic

### 4. Configuration Changes
**Excluded**: Grace period configuration
- No changes to _accountFillGraceTicks
- No changes to grace period thresholds
- No changes to configuration loading
- **Rationale**: Configuration is stable, no business logic changes

### 5. Test Infrastructure Changes
**Excluded**: Existing test modifications
- No changes to existing xUnit tests
- Only ADD new tests for extracted methods
- No changes to test fixtures
- **Rationale**: Preserve existing test coverage

## Scope Validation

### Complexity Budget
- **Current**: 16 decision points
- **Target Distribution**:
  - State Calculation: 5 decision points
  - Grace Period Logic: 4 decision points
  - FSM Filtering: 5 decision points
  - FSM Lifecycle: 6 decision points
  - Orchestration (remaining): 4 decision points
- **Total**: 24 decision points (includes new orchestration overhead)
- **Validation**: Each extracted method ≤8, orchestration ≤5

### Risk Mitigation
- **LOW Blast Radius**: Private method, no external callers
- **HIGH Test Coverage**: xUnit tests for each extracted method
- **NO Behavioral Changes**: Audit logic remains identical
- **Incremental Approach**: One extraction per ticket

### Jane Street Alignment
- CYC ≤8 per method (strict standard)
- FSM/Actor pattern preserved
- No lock() usage (already compliant)
- ASCII-only compliance (already compliant)
- Make illegal states unrepresentable (value object pattern)

## Success Criteria
- All extracted methods have CYC ≤8
- Original method reduced to CYC ≤5 (orchestration only)
- No changes to caller methods
- No changes to FSM state machine
- No changes to logging infrastructure
- xUnit tests cover all extracted methods
- Build passes (dotnet build)
- NinjaTrader F5 successful
- deploy-sync.ps1 executed successfully

## Next Phase
**Phase 2**: Architecture Planning
- Design 4-5 helper method signatures
- Define AuditFleetResult value object
- Create extraction sequence diagram
- Identify test scenarios for each helper
