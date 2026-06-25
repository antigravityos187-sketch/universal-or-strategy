# Phase 1: Scope Definition - EPIC-W7-016

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.18
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T19:25:21Z

## Target Method
- **Method**: TryHandleFleet_CancelAll
- **File**: src/V12_002.UI.IPC.Commands.Fleet.cs
- **Line**: 177
- **Current CYC**: 19
- **Target CYC**: ≤8 (Jane Street threshold)

## Scope Boundary Definition

### IN SCOPE

#### Primary Extraction Target
**TryHandleFleet_CancelAll** (CYC 19 → ≤8)
- Extract duplicate command detection logic (CYC ~3)
- Extract master account cancellation orchestration (CYC ~5)
- Extract fleet account cancellation orchestration (CYC ~5)
- Extract position cleanup orchestration (CYC ~3)
- Reduce main method to high-level orchestration (CYC ~3)

#### Extraction Strategy
1. **ExtractDuplicateCommandCheck()** - Metadata guard logic
2. **CancelMasterAccountOrders()** - Master account cancellation flow
3. **CancelFleetAccountOrders()** - Fleet account cancellation flow
4. **CleanupUnfilledPositions()** - Position cleanup flow
5. **TryHandleFleet_CancelAll()** - Orchestration only (calls 1-4)

#### Success Criteria
- TryHandleFleet_CancelAll reduced to CYC ≤8
- All extracted methods have CYC ≤8
- Zero compilation errors
- Zero behavioral changes (pure refactoring)
- All existing helper methods remain unchanged

### OUT OF SCOPE

#### Existing Helper Methods (DO NOT MODIFY)
- **MetadataGuardDuplicate** - Already extracted, working correctly
- **CancelAll_ProcessMasterAccount** - Already extracted, working correctly
- **CancelAll_ProcessFleetAccounts** - Already extracted, working correctly
- **CancelOrderOnAccount** - Already extracted, working correctly
- **CancelAll_ProcessFleetOrders** - Already extracted, working correctly
- **CancelAll_CleanupUnfilledPositions** - Already extracted, working correctly
- **CancelAll_ProcessSingleFleetAccount** - Already extracted, working correctly

#### Related High-Complexity Methods (SEPARATE EPICS)
- **TryHandleFleet_LongShort** (CYC 21) - EPIC-W7-017
- **TryHandleFleetCommand** (CYC 20) - EPIC-W7-018

#### Infrastructure Changes (OUT OF SCOPE)
- No changes to IPC command routing
- No changes to fleet account management
- No changes to order cancellation logic
- No changes to position cleanup logic
- No changes to logging infrastructure

## Blast Radius Validation

### Confirmed Low Risk
- **Importer Count**: 0 (internal method)
- **Direct Dependents**: 0
- **Overall Risk Score**: 0.0
- **Single Caller**: TryHandleFleetCommand (same file)

### Safety Guarantees
- Method is private/internal to V12_002 class
- No external consumers
- No cross-file dependencies
- Safe for surgical refactoring

## Extraction Boundaries

### What Gets Extracted
1. Duplicate command detection block (lines ~180-185)
2. Master account cancellation block (lines ~187-195)
3. Fleet account cancellation block (lines ~197-210)
4. Position cleanup block (lines ~212-220)

### What Stays in Main Method
1. Method signature (unchanged)
2. High-level orchestration flow
3. Early returns for validation failures
4. Final return statement

## Verification Strategy

### Pre-Extraction Verification
- Confirm current CYC = 19
- Confirm zero compilation errors
- Confirm zero test failures

### Post-Extraction Verification
- Confirm TryHandleFleet_CancelAll CYC ≤8
- Confirm all extracted methods CYC ≤8
- Confirm zero compilation errors
- Confirm zero test failures
- Confirm zero behavioral changes

## Risk Mitigation

### Low Blast Radius Confirmed
- Zero external importers
- Single internal caller
- No cross-file dependencies

### Existing Decomposition Preserved
- All existing helper methods remain unchanged
- Only orchestration logic extracted
- No changes to helper method signatures

### Jane Street Alignment
- Target CYC ≤8 (strict threshold)
- Cognitive simplicity prioritized
- Single-responsibility principle enforced

## Scope Approval

**Scope Status**: APPROVED
**Rationale**:
- Clear extraction boundaries
- Low blast radius (risk score 0.0)
- Existing helper methods preserved
- Jane Street CYC ≤8 achievable
- No scope creep risk

**Ready for Phase 2**: YES
