# Phase 1: Scope Definition - EPIC-W7-126

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.18
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T19:41:24Z

## Epic Overview
**Target**: SymmetryGuardSubmitFollowerBracket (CYC 16 to 8 or less)
**File**: src/V12_002.Symmetry.Follower.cs
**Line**: 285
**Current Complexity**: 16 (2x Jane Street threshold)

## Scope Boundary Definition

### IN SCOPE

#### 1. Core Extraction Target
- **Method**: SymmetryGuardSubmitFollowerBracket (lines 285-426)
- **Current State**: 141 lines, CYC 16, 5-level nesting
- **Target State**: 3 extracted methods, each CYC 8 or less

#### 2. Extracted Methods (3 total)

**Extraction 1: ValidateFollowerBracketPreconditions**
- **Purpose**: Consolidate all validation logic
- **Scope**: ValidateStopPrice calls, Validate_LongIsIllegalAdjust checks, Validate_ShortIsIllegalAdjust checks, Early return pattern for failures
- **Target CYC**: 8 or less
- **Lines**: approximately 30-40

**Extraction 2: CalculateFollowerBracketPrices**
- **Purpose**: Isolate price calculation logic
- **Scope**: GetTargetPrice calls, GetTargetContracts calls, IsRunnerTarget checks, GetTargetMode queries
- **Target CYC**: 8 or less
- **Lines**: approximately 25-35

**Extraction 3: SubmitFollowerBracketOrders**
- **Purpose**: Pure order submission logic
- **Scope**: Enqueue calls (Actor pattern), GetTargetOrdersDictionary updates, SymmetryTrim calls, LogBuffer.Format calls
- **Target CYC**: 8 or less
- **Lines**: approximately 30-40

#### 3. Caller Updates (3 methods)
- SymmetryGuardOnFollowerFill (line 17) - Update call signature
- SymmetryGuardTryResolveFollower (line 129) - Update call signature
- SymmetryGuardProcessPendingFollowerFills (line 97) - Update call signature

#### 4. Test Coverage
- **New Tests**: 3 unit tests (one per extracted method)
- **Framework**: xUnit (V12.32 mandate)
- **Location**: To be determined in Phase 2

### OUT OF SCOPE

#### 1. Callee Methods (34 methods)
- **Rationale**: Already at acceptable complexity
- **Examples**: ValidateStopPrice, GetTargetContracts, LogBuffer.Format
- **Action**: Use as-is, no modifications

#### 2. Actor Pattern Infrastructure
- **Rationale**: Core V12 DNA, already lock-free compliant
- **Examples**: Enqueue method, _cmdQueue, TryDrain, ScheduleActorDrain
- **Action**: Preserve existing semantics

#### 3. Logging Infrastructure
- **Rationale**: Shared utility, out of epic scope
- **Examples**: LogBuffer.Format, LogBuffer.ValidateThreadAffinity, LogBuffer.FormatInternal
- **Action**: Use as-is

#### 4. Position Info Queries
- **Rationale**: Stable utility methods
- **Examples**: GetTargetMode, IsRunnerTarget, GetTargetPrice
- **Action**: Use as-is

#### 5. Other Symmetry Methods
- **Rationale**: Separate epics if needed
- **Examples**: SymmetryGuardOnFollowerFill, SymmetryGuardTryResolveFollower, SymmetryTrim
- **Action**: Out of scope for EPIC-W7-126

#### 6. Cross-File Dependencies
- **Rationale**: Zero external importers (blast radius analysis)
- **Action**: No cross-file changes required

## Scope Validation

### Complexity Budget
- **Before**: 1 method x CYC 16 = 16 total complexity
- **After**: 4 methods x CYC 8 or less = 32 or less total complexity
- **Net Change**: +16 complexity (acceptable for maintainability gain)

### Line Count Budget
- **Before**: 141 lines in 1 method
- **After**: approximately 30-40 lines x 4 methods = 120-160 lines
- **Net Change**: -21 to +19 lines (acceptable)

### Blast Radius Confirmation
- **External Importers**: 0 (confirmed via Phase 0)
- **Direct Callers**: 3 (all in same file)
- **Risk Score**: 0.0 (LOW)
- **Conclusion**: Isolated refactoring, minimal risk

## Success Criteria

### Phase 1 (Scope Definition)
- Scope boundary defined (IN vs OUT)
- 3 extraction targets identified
- Complexity budget validated
- Blast radius confirmed

### Phase 2 (Architecture Planning)
- Extraction sequence defined
- Actor pattern preservation strategy
- Test strategy defined

### Phase 3 (DNA Audit)
- Lock-free compliance verified
- ASCII-only compliance verified
- Jane Street alignment verified

### Phase 4 (Ticket Generation)
- 3 tickets generated (one per extraction)
- Execution order defined
- Dependencies mapped

### Phase 5 (Execution)
- Ticket 1: ValidateFollowerBracketPreconditions
- Ticket 2: CalculateFollowerBracketPrices
- Ticket 3: SubmitFollowerBracketOrders
- All methods CYC 8 or less
- All tests passing

## Risk Mitigation

### Risk 1: Actor Pattern Thread Safety
- **Mitigation**: Preserve all Enqueue calls, no direct state mutation
- **Validation**: Phase 3 DNA audit

### Risk 2: Validation Order Dependencies
- **Mitigation**: Extract validations in original order
- **Validation**: Phase 5 unit tests

### Risk 3: Logging Context Loss
- **Mitigation**: Pass context parameters to extracted methods
- **Validation**: Phase 5 integration tests

## Scope Approval

**Status**: APPROVED
**Rationale**: Clear extraction targets (3 methods), Low blast radius (internal only), Manageable complexity budget, No cross-file dependencies

**Next Phase**: Phase 2 (Architecture Planning)
