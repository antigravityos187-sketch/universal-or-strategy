# Phase 1: Scope Definition - EPIC-W7-031

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.18
- **MCP Tools Used**: jCodemunch (get_file_outline, get_symbol_source)
- **Sequential Thinking**: Used for scope boundary validation
- **Execution Time**: 2026-06-24T20:05:51Z

## Epic Objective
Reduce cyclomatic complexity of AuditMaster_HandleNakedPosition from 19 to ≤8 through surgical extraction of emergency stop logic, state tracking, and logging subsystems.

## Target Method
- **Method**: AuditMaster_HandleNakedPosition
- **File**: src/V12_002.REAPER.Audit.cs
- **Line**: 624
- **Current CYC**: 19
- **Target CYC**: ≤8
- **Lines**: 56
- **Nesting Depth**: 7

## IN SCOPE

### Primary Extraction Targets (3-4 methods)

#### 1. Emergency Stop Logic Extraction
**Method Name**: EnqueueEmergencyNakedStop
**Responsibility**: Orchestrate emergency stop creation and queueing
**Extracted Lines**: ~15-20 lines
**Target CYC**: ≤5
**Includes**:
- EnqueueReaperMasterNakedStop call
- CalculateEmergencyStopPrice call
- Emergency stop price calculation logic
- Queue enqueue logic

#### 2. State Tracking Extraction
**Method Name**: UpdateNakedPositionState
**Responsibility**: Manage naked position detection state
**Extracted Lines**: ~10-15 lines
**Target CYC**: ≤4
**Includes**:
- _nakedPositionFirstSeen dictionary updates
- _reaperNakedStopInFlight flag management
- State initialization logic
- State cleanup via ClearNakedStopInFlight

#### 3. Logging Extraction
**Method Name**: LogNakedPositionEvent
**Responsibility**: Centralize naked position logging
**Extracted Lines**: ~8-12 lines
**Target CYC**: ≤3
**Includes**:
- LogBuffer.Format calls
- LogBuffer.ValidateThreadAffinity calls
- Structured logging with position details
- Thread safety validation

#### 4. Queue Processing Coordination (Optional)
**Method Name**: ProcessNakedStopQueueIfNeeded
**Responsibility**: Conditional queue processing
**Extracted Lines**: ~5-8 lines
**Target CYC**: ≤3
**Includes**:
- ProcessReaperNakedStopQueue call
- Queue state checks
- Conditional processing logic

### Scope Boundaries

**File Boundary**:
- ONLY src/V12_002.REAPER.Audit.cs
- NO changes to other files

**Method Boundary**:
- ONLY AuditMaster_HandleNakedPosition (line 624)
- Extract 3-4 new private methods in same file
- NO changes to callers (AuditMasterAccountIfNeeded, AuditApexPositions)

**Signature Boundary**:
- Keep original method signature unchanged
- Keep original method as orchestrator
- NO parameter changes to public/internal methods

**State Boundary**:
- Access existing fields (_nakedPositionFirstSeen, _reaperNakedStopInFlight, _reaperNakedStopQueue)
- NO new fields or state variables
- NO changes to field initialization

### Success Criteria
1. **Complexity**: AuditMaster_HandleNakedPosition CYC reduced from 19 to ≤8
2. **Extracted Methods**: 3-4 new private methods, each CYC ≤8
3. **Nesting**: Reduce max nesting from 7 to ≤3
4. **Behavior**: Zero functional changes (pure refactoring)
5. **Tests**: All existing tests pass
6. **Build**: Clean compilation with no warnings

## OUT OF SCOPE

### Explicitly Excluded

#### 1. Caller Modifications
- NO changes to AuditMasterAccountIfNeeded (line 684)
- NO changes to AuditApexPositions (line 16)
- **Rationale**: Callers are internal and working correctly

#### 2. Callee Modifications
- NO changes to EnqueueReaperMasterNakedStop
- NO changes to CalculateEmergencyStopPrice
- NO changes to ProcessReaperNakedStopQueue
- NO changes to LogBuffer methods
- **Rationale**: Callees are shared subsystems, changes would expand blast radius

#### 3. State Structure Changes
- NO modifications to _nakedPositionFirstSeen dictionary structure
- NO modifications to _reaperNakedStopInFlight flag type
- NO modifications to _reaperNakedStopQueue structure
- **Rationale**: State structures are used across multiple methods

#### 4. Cross-File Changes
- NO changes to other REAPER files
- NO changes to FSM files
- NO changes to shared utilities
- **Rationale**: Zero blast radius requirement

#### 5. Behavioral Changes
- NO logic changes
- NO algorithm changes
- NO timing changes
- NO error handling changes
- **Rationale**: Pure refactoring only

#### 6. Test File Changes
- NO new test files (unless required by TDD)
- NO test modifications (unless tests are brittle)
- **Rationale**: Existing tests should pass unchanged

### Deferred to Future Epics

#### 1. Related Complexity Hotspots
- AuditMasterAccountIfNeeded (CYC unknown) - separate epic
- AuditApexPositions (CYC unknown) - separate epic
- Other REAPER audit methods - separate epics

#### 2. Architecture Improvements
- REAPER subsystem redesign - separate epic
- Naked position detection strategy - separate epic
- Emergency stop coordination - separate epic

#### 3. Performance Optimization
- Queue processing optimization - separate epic
- State tracking optimization - separate epic
- Logging performance - separate epic

## Risk Mitigation

### Low Risk Factors (Advantages)
- Zero external dependencies (blast radius = 0)
- Only 2 internal callers
- Clear extraction boundaries
- No cross-file impact

### Medium Risk Factors (Mitigations)
- High complexity (CYC 19) → Extract incrementally, verify after each extraction
- Deep nesting (7 levels) → Use early returns to flatten
- 22 callees → Keep callee invocations unchanged

### Mitigation Strategy
1. **Incremental Extraction**: Extract one method at a time
2. **Build Verification**: Compile after each extraction
3. **Test Verification**: Run tests after each extraction
4. **Rollback Plan**: Git checkpoint before each extraction

## Verification Plan

### Pre-Extraction
1. Verify method exists at line 624
2. Verify CYC = 19 via complexity_audit.py
3. Verify 2 callers via jCodemunch
4. Verify 0 external dependents via blast radius

### Post-Extraction
1. Verify CYC ≤8 for original method
2. Verify CYC ≤8 for each extracted method
3. Verify max nesting ≤3
4. Verify build passes (dotnet build)
5. Verify tests pass (dotnet test)
6. Verify deploy-sync.ps1 succeeds

## Summary

EPIC-W7-031 targets AuditMaster_HandleNakedPosition (CYC 19 → ≤8) through extraction of 3-4 private methods: emergency stop logic, state tracking, logging, and optional queue processing. Scope is strictly limited to one file (src/V12_002.REAPER.Audit.cs) with zero blast radius. All callers, callees, and state structures remain unchanged. This is a pure refactoring epic with low risk due to isolation and clear extraction boundaries.
