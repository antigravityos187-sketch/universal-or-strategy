# Phase 1: Scope Definition - EPIC-W7-095

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.18
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T19:36:46Z

## Epic Overview
- **Target Method**: ProcessSingleFleetRMAAccount
- **File**: src/V12_002.SIMA.Execution.cs
- **Line**: 511
- **Current CYC**: 25
- **Target CYC**: ≤ 8 per method
- **Lines**: 168
- **Parameters**: 9

## Scope Boundary Definition

### IN SCOPE

#### 1. State Access Extraction (Priority: HIGH)
**Rationale**: 6 dictionary lookups create noise and increase CYC
- Extract activeFleetAccounts lookup
- Extract activePositions lookup
- Extract entryOrders lookup
- Extract _followerBrackets lookup
- Extract expectedPositions lookup (depth 2)
- Extract _dispatchSyncPendingExpKeys lookup (depth 2)

**Target**: Create GetFleetAccountState() helper method (CYC ≤ 3)

#### 2. Symmetry Registration Logic (Priority: HIGH)
**Rationale**: SymmetryGuardRegisterFollower calls are complex and repeated
- Extract symmetry guard registration logic
- Extract symmetry dispatch lookup
- Extract follower bracket management

**Target**: Create RegisterFleetSymmetry() helper method (CYC ≤ 5)

#### 3. Position Delta Management (Priority: MEDIUM)
**Rationale**: AddExpectedPositionDeltaLocked is a critical operation
- Extract position delta calculation
- Extract expected position updates
- Extract fill grace stamping logic

**Target**: Create UpdateExpectedPositionDelta() helper method (CYC ≤ 4)

#### 4. Dispatch Synchronization (Priority: MEDIUM)
**Rationale**: MarkDispatchSyncPending/ClearDispatchSyncPending are paired operations
- Extract dispatch sync marking logic
- Extract dispatch sync clearing logic
- Extract dispatch key management

**Target**: Create ManageDispatchSync() helper method (CYC ≤ 3)

#### 5. Parameter Reduction (Priority: HIGH)
**Rationale**: 9 parameters indicate multiple responsibilities
- Group related parameters into FleetRMAContext struct

**Target**: Reduce to 1 parameter (context object)

### OUT OF SCOPE

#### 1. Core Algorithm Logic
**Rationale**: Preserve existing business logic, only extract helpers
- Do NOT modify core RMA entry logic
- Do NOT change order submission flow
- Do NOT alter symmetry guard behavior

#### 2. External Dependencies
**Rationale**: Zero blast radius means no external changes needed
- Do NOT modify ExecuteRMAEntryV2 (caller)
- Do NOT modify any of the 32 callees
- Do NOT change method signatures of dependencies

#### 3. State Management Infrastructure
**Rationale**: Dictionary structures are shared across codebase
- Do NOT modify activeFleetAccounts structure
- Do NOT modify activePositions structure
- Do NOT modify entryOrders structure
- Do NOT modify _followerBrackets structure

#### 4. Logging and Diagnostics
**Rationale**: Preserve existing logging for debugging
- Do NOT remove existing LogBuffer.Format calls
- Do NOT change log message formats
- Do NOT alter diagnostic output

#### 5. Error Handling
**Rationale**: Maintain existing error handling patterns
- Do NOT add new exception types
- Do NOT change existing error paths
- Do NOT modify null checks

## Extraction Strategy

### Phase 2 Targets (4 Helper Methods)

1. **GetFleetAccountState()** - CYC ≤ 3
   - Input: account, instrument
   - Output: FleetAccountState struct
   - Extracts: 6 dictionary lookups

2. **RegisterFleetSymmetry()** - CYC ≤ 5
   - Input: FleetRMAContext, FleetAccountState
   - Output: bool (success)
   - Extracts: SymmetryGuardRegisterFollower logic

3. **UpdateExpectedPositionDelta()** - CYC ≤ 4
   - Input: FleetRMAContext, FleetAccountState
   - Output: void
   - Extracts: AddExpectedPositionDeltaLocked logic

4. **ManageDispatchSync()** - CYC ≤ 3
   - Input: expKey, shouldMark
   - Output: void
   - Extracts: MarkDispatchSyncPending/ClearDispatchSyncPending

### Expected Complexity Reduction

| Metric | Before | After | Improvement |
|--------|--------|-------|-------------|
| Cyclomatic Complexity | 25 | ≤ 8 | 68% reduction |
| Lines of Code | 168 | ≤ 50 | 70% reduction |
| Parameter Count | 9 | 1 | 89% reduction |
| Max Nesting Depth | 5 | ≤ 3 | 40% reduction |

### Risk Mitigation

**Low Risk Factors**:
- Zero blast radius (no external dependents)
- Single caller (clear entry point)
- Well-isolated method

**Mitigation Strategy**:
1. Extract one helper at a time
2. Run build after each extraction
3. Verify F5 in NinjaTrader after each extraction
4. Add unit tests for each extracted helper

## Success Criteria

### Phase 2 (Architecture Planning)
- 4 helper methods designed with CYC ≤ 8 each
- FleetRMAContext struct defined
- FleetAccountState struct defined
- Extraction sequence documented

### Phase 5 (Ticket Execution)
- ProcessSingleFleetRMAAccount reduced to CYC ≤ 8
- All 4 helper methods implemented
- Parameter count reduced from 9 to 1
- Build passes
- F5 in NinjaTrader successful

### Phase 5.V (Verification)
- Complexity audit confirms CYC ≤ 8
- No new compilation errors
- No regression in existing functionality
- Unit tests pass for all extracted methods

## Scope Validation

### Boundary Checks
- **IN SCOPE**: Only touches ProcessSingleFleetRMAAccount internals
- **OUT OF SCOPE**: No changes to callers or callees
- **SAFE**: Zero blast radius confirmed
- **FOCUSED**: 4 helper methods, 2 context structs

### Jane Street Alignment
- **Complexity**: Target CYC ≤ 8 (Jane Street strict)
- **Simplicity**: Single responsibility per helper
- **Testability**: Each helper independently testable
- **Correctness**: Preserve existing business logic

---

**Phase 1 Status**: COMPLETE
**Generated**: 2026-06-24T19:36:46Z
**Agent**: v12-phase1-scope
**Next Phase**: Phase 1.5 (Scope Boundary Validation)
