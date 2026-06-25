# Phase 1: Scope Definition - EPIC-W7-157

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Mode**: plan
- **Phase**: 1 (Scope Definition)
- **Input**: 00-hotspots.md
- **Execution Time**: 2026-06-24T19:46:00Z

## Epic Overview

### Target Method
- **Method**: `TryHandleFleet_MoveTarget`
- **File**: `src/V12_002.UI.IPC.Commands.Fleet.cs`
- **Line**: 645
- **Current CYC**: 17 (HIGH - 2.1x over Jane Street threshold of 8)
- **Target CYC**: ≤8 per method after extraction

### Refactoring Goal
Decompose the coordinator method into single-responsibility methods, each with CYC ≤8, while maintaining the low blast radius and preserving all functionality.

## Scope Boundaries

### What Will Be Extracted

#### 1. Validation Logic → `ValidateFleetMoveTargetRequest()`
**Responsibility**: Validate incoming Fleet move target requests

**Extracted Code**:
- Parameter validation (null checks, range checks)
- Request type validation (absolute vs relative)
- Position state validation
- Target order existence checks

**Expected CYC**: 4-6

**Rationale**: Validation is a distinct concern that should be isolated for:
- Independent testing of validation rules
- Reusability across other Fleet commands
- Clear error messaging

#### 2. Absolute Move Path → `HandleAbsoluteTargetMove()`
**Responsibility**: Process absolute target price moves

**Extracted Code**:
- Absolute move request parsing
- `ValidateTargetMoveAbsoluteRequest()` call
- `FindTargetOrderForAbsoluteMove()` call
- `ExecuteTargetAbsoluteMove()` call
- Absolute move error handling

**Expected CYC**: 5-7

**Rationale**: Absolute moves have distinct logic from relative moves:
- Different validation rules
- Different execution paths
- Different error scenarios

#### 3. Relative Move Path → `HandleRelativeTargetMove()`
**Responsibility**: Process relative target price moves

**Extracted Code**:
- Relative move request parsing
- `ValidateMoveTargetRequest()` call
- `FindTargetOrderForPosition()` call
- `CalculateAndValidateNewTargetPrice()` call
- `MoveSpecificTarget()` or `MoveSpecificTargetAbsolute()` call
- Relative move error handling

**Expected CYC**: 5-7

**Rationale**: Relative moves have distinct logic from absolute moves:
- Different validation rules
- Different calculation requirements
- Different execution paths

#### 4. Error Logging → `LogFleetMoveError()`
**Responsibility**: Centralized error logging for Fleet move operations

**Extracted Code**:
- Error message formatting
- `LogBuffer.Format()` calls
- Context enrichment (position ID, target price, error details)

**Expected CYC**: 2-3

**Rationale**: Consistent error logging across all move paths:
- Reduces duplication
- Ensures consistent log format
- Simplifies debugging

### What Will Remain in Original Method

**Orchestration Logic Only** (Expected CYC: 4-6):
- Request type discrimination (absolute vs relative)
- Delegation to extracted methods
- High-level error handling
- Return value aggregation

**Preserved Structure**:
```csharp
private bool TryHandleFleet_MoveTarget(string[] args, StringBuilder response)
{
    // 1. Validate request
    if (!ValidateFleetMoveTargetRequest(args, response))
        return false;
    
    // 2. Route to appropriate handler
    if (IsAbsoluteMoveRequest(args))
        return HandleAbsoluteTargetMove(args, response);
    else
        return HandleRelativeTargetMove(args, response);
}
```

### What Will NOT Be Touched

**Out of Scope**:
- Callee methods (30 methods in call hierarchy) - these are already extracted
- Caller method (`TryHandleFleetCommand`) - dispatcher logic is separate concern
- Related Fleet methods (`TryHandleFleet_LongShort`, etc.) - separate epics
- Validation/execution helper methods - already at appropriate granularity

**Rationale**: 
- Low blast radius (0 external dependents) means we can safely refactor in isolation
- Callee methods are already single-responsibility
- Other Fleet commands require separate analysis

## Dependencies and Constraints

### Internal Dependencies (Will Be Preserved)
1. **Validation Layer**:
   - `ValidateMoveTargetRequest()` - relative move validation
   - `ValidateTargetMoveAbsoluteRequest()` - absolute move validation

2. **Lookup Layer**:
   - `FindTargetOrderForPosition()` - find target by position
   - `FindTargetOrderForAbsoluteMove()` - find target by order ID

3. **Calculation Layer**:
   - `CalculateAndValidateNewTargetPrice()` - price calculation

4. **Execution Layer**:
   - `ExecuteMasterTargetMove()` - master position moves
   - `ExecuteFollowerTargetMove()` - follower position moves
   - `ExecuteTargetAbsoluteMove()` - absolute price moves
   - `MoveSpecificTarget()` - relative moves
   - `MoveSpecificTargetAbsolute()` - absolute moves

5. **Logging Layer**:
   - `LogBuffer.Format()` - thread-safe logging

### External Dependencies (None)
- **Blast Radius**: 0 external dependents
- **Risk**: LOW - changes are isolated to Fleet command subsystem

### Constraints
1. **Thread Safety**: Must preserve LogBuffer thread affinity
2. **Performance**: No additional allocations in hot path
3. **Behavior**: Must maintain exact same functionality
4. **Error Handling**: Must preserve all error messages and codes
5. **Testing**: Must maintain F5 compatibility in NinjaTrader

## Risk Assessment

### Complexity Risk: HIGH → LOW (After Extraction)
- **Before**: CYC=17 (2.1x over threshold)
- **After**: CYC≤8 per method (Jane Street compliant)
- **Mitigation**: Extract to 4 methods, each with single responsibility

### Blast Radius Risk: LOW (Unchanged)
- **External Dependents**: 0
- **Internal Caller**: 1 (TryHandleFleetCommand)
- **Mitigation**: No external coordination required

### Regression Risk: LOW
- **Test Coverage**: F5 in NinjaTrader validates all paths
- **Validation**: Pre-existing validation methods ensure correctness
- **Mitigation**: Preserve exact behavior, add unit tests for extracted methods

### Coordination Risk: MEDIUM → LOW (After Extraction)
- **Before**: 30 callees in single method (high fan-out)
- **After**: Callees distributed across 4 methods (reduced fan-out per method)
- **Mitigation**: Clear separation of concerns reduces cognitive load

## Success Criteria

### Functional Requirements
- ✅ All Fleet move target commands work identically to before
- ✅ Absolute move path preserves exact behavior
- ✅ Relative move path preserves exact behavior
- ✅ Error messages unchanged
- ✅ Logging format unchanged

### Quality Requirements
- ✅ Original method: CYC ≤8 (orchestration only)
- ✅ Extracted methods: CYC ≤8 each
- ✅ No new compilation errors
- ✅ No new Roslyn warnings
- ✅ CSharpier formatting compliant

### Testing Requirements
- ✅ F5 in NinjaTrader succeeds
- ✅ BUILD_TAG appears in output
- ✅ Manual testing of absolute moves
- ✅ Manual testing of relative moves
- ✅ Error path testing (invalid requests)

### Documentation Requirements
- ✅ XML comments on all extracted methods
- ✅ Update EPIC-W7-157 brain directory
- ✅ Update "Recent Major Refactors" table in src/AGENTS.md

## Extraction Strategy

### Phase 2 (Architecture Planning) Will Define:
1. **Method Signatures**: Exact parameters and return types
2. **Error Handling**: Exception vs return code strategy
3. **Logging Strategy**: Where to log in each extracted method
4. **Test Strategy**: Unit test coverage for each extraction

### Phase 3 (DNA Audit) Will Verify:
1. **V12 DNA Compliance**: Lock-free, ASCII-only, CYC≤8
2. **Jane Street Alignment**: Correctness by construction
3. **PR Hygiene**: Diff size, whitespace, formatting

### Phase 4 (Ticket Generation) Will Create:
1. **Ticket 1**: Extract ValidateFleetMoveTargetRequest()
2. **Ticket 2**: Extract HandleAbsoluteTargetMove()
3. **Ticket 3**: Extract HandleRelativeTargetMove()
4. **Ticket 4**: Extract LogFleetMoveError()
5. **Ticket 5**: Refactor orchestration logic in TryHandleFleet_MoveTarget()

## Boundary Validation

### Clear Boundaries Defined
- ✅ **Validation**: All parameter/state checks → ValidateFleetMoveTargetRequest()
- ✅ **Absolute Path**: All absolute move logic → HandleAbsoluteTargetMove()
- ✅ **Relative Path**: All relative move logic → HandleRelativeTargetMove()
- ✅ **Error Logging**: All error formatting → LogFleetMoveError()
- ✅ **Orchestration**: Request routing only → TryHandleFleet_MoveTarget()

### No Ambiguity
- Each extracted method has single, well-defined responsibility
- No overlap between extracted methods
- Clear delegation from orchestrator to handlers
- Preserved call hierarchy (30 callees distributed appropriately)

### Testability
- Each extracted method can be unit tested independently
- Orchestrator can be tested with mocked handlers
- Integration testing via F5 in NinjaTrader

## Conclusion

**Scope is well-defined and ready for Phase 2 (Architecture Planning)**:
- ✅ Clear extraction targets (4 methods)
- ✅ Well-defined boundaries (validation, absolute, relative, logging, orchestration)
- ✅ Low risk (0 external dependents, isolated subsystem)
- ✅ High value (CYC 17→≤8, improved testability, reduced cognitive load)
- ✅ Jane Street aligned (single responsibility, CYC≤8 per method)

**Next Phase**: Architecture Planning will define exact method signatures, error handling strategy, and test approach.
