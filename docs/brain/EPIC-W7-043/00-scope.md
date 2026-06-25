# Phase 1: Scope Definition - EPIC-W7-043

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.00
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T20:06:28Z

## Epic Metadata
- **Epic ID**: EPIC-W7-043
- **Target Method**: SymmetryGuardSubmitFollowerBracket
- **File**: src/V12_002.Symmetry.Follower.cs
- **Line**: 285
- **Current CYC**: 16
- **Target CYC**: ≤8
- **Reduction Required**: 8 decision points

## IN SCOPE

### Primary Target
- **Method**: `SymmetryGuardSubmitFollowerBracket` (lines 285-426)
  - **Reason**: CYC=16, exceeds Jane Street threshold by 2x
  - **Lines**: 141 lines
  - **Nesting**: 5 levels (target: ≤3)
  - **Risk**: MEDIUM (high complexity, low blast radius)

### Extraction Candidates
Based on the 16 decision points and 5 nesting levels, the following logic blocks are IN SCOPE for extraction:

1. **Validation Logic** (estimated 3-4 decision points)
   - Stop price validation
   - Long/Short illegal adjust validation
   - Position validation checks

2. **Position Info Retrieval** (estimated 2-3 decision points)
   - Target contracts calculation
   - Runner target checks
   - Target price/mode retrieval

3. **Symmetry Trim Logic** (estimated 2-3 decision points)
   - Symmetry adjustment calculations
   - Trim condition evaluation

4. **Bracket Submission Logic** (estimated 3-4 decision points)
   - Order creation
   - Bracket configuration
   - Submission validation

5. **Error Handling Paths** (estimated 2-3 decision points)
   - Validation failures
   - Submission failures
   - State recovery

### Scope Boundaries
- **Start Line**: 285 (method signature)
- **End Line**: 426 (method closing brace)
- **Total Lines**: 141
- **File**: src/V12_002.Symmetry.Follower.cs

## OUT OF SCOPE

### Caller Methods (DO NOT MODIFY)
These methods call `SymmetryGuardSubmitFollowerBracket` but are NOT targets for this epic:
1. **SymmetryGuardOnFollowerFill** (line 17)
   - **Reason**: Separate responsibility, different complexity profile
2. **SymmetryGuardTryResolveFollower** (line 129)
   - **Reason**: Separate responsibility, different complexity profile
3. **SymmetryGuardProcessPendingFollowerFills** (line 97)
   - **Reason**: Indirect caller, separate epic scope

### Callee Methods (DO NOT MODIFY)
The 34 downstream methods called by the target are OUT OF SCOPE:
- **Validation methods**: ValidateStopPrice, Validate_LongIsIllegalAdjust, Validate_ShortIsIllegalAdjust
- **Position info methods**: GetTargetContracts, IsRunnerTarget, GetTargetPrice, GetTargetMode
- **Symmetry methods**: SymmetryTrim
- **Logging methods**: LogBuffer.Format, LogBuffer.ValidateThreadAffinity, LogBuffer.FormatInternal
- **Actor model methods**: Enqueue, IsActorThread, TryDrain, ScheduleActorDrain
- **UI methods**: GetTargetOrdersDictionary

**Reason**: These are utility/infrastructure methods with their own complexity profiles. Modifying them would expand blast radius unnecessarily.

### Other Files (DO NOT MODIFY)
- **src/V12_002.Symmetry.Leader.cs**: Separate symmetry logic
- **src/V12_002.SIMA.Lifecycle.cs**: FSM lifecycle management
- **src/V12_002.Atm.cs**: ATM logic
- **All other V12_002.*.cs files**: Out of epic scope

## Scope Validation

### Blast Radius Confirmation
- **Importer Count**: 0 (no external dependents)
- **Direct Dependents**: 0 (isolated to Symmetry.Follower module)
- **Overall Risk Score**: 0.0 (LOW)
- **Verdict**: ✅ SAFE - Changes are isolated, no cross-module impact

### Complexity Reduction Target
- **Current CYC**: 16
- **Target CYC**: ≤8
- **Reduction Required**: 8 decision points (50% reduction)
- **Strategy**: Extract 4-5 helper methods, each with CYC ≤3

### Jane Street Alignment
- **Current State**: CYC=16, Nesting=5 (FAILS standard)
- **Target State**: CYC≤8, Nesting≤3 (PASSES standard)
- **Extraction Count**: 4-5 methods
- **Expected Outcome**: Main method becomes orchestrator (CYC ≤5), extracted methods are single-purpose (CYC ≤3)

## Scope Boundary Enforcement

### MANDATORY CONSTRAINTS
1. **File Boundary**: ONLY modify `src/V12_002.Symmetry.Follower.cs`
2. **Line Boundary**: ONLY modify lines 285-426
3. **Method Boundary**: ONLY extract logic FROM `SymmetryGuardSubmitFollowerBracket`
4. **Caller Preservation**: DO NOT modify the 3 caller methods
5. **Callee Preservation**: DO NOT modify the 34 callee methods

### SCOPE CREEP PREVENTION
- ❌ DO NOT refactor caller methods
- ❌ DO NOT refactor callee methods
- ❌ DO NOT modify other Symmetry.*.cs files
- ❌ DO NOT add new dependencies
- ❌ DO NOT change method signatures of existing callees

### SUCCESS CRITERIA
- ✅ `SymmetryGuardSubmitFollowerBracket` CYC reduced from 16 to ≤8
- ✅ 4-5 new private helper methods created (each CYC ≤3)
- ✅ All 3 callers continue to work without modification
- ✅ All 34 callees continue to work without modification
- ✅ Build passes
- ✅ F5 in NinjaTrader successful

## Next Steps (Phase 2)
1. Architecture planning: Design extraction strategy
2. Identify exact decision points to extract
3. Plan helper method signatures
4. Generate tickets for surgical refactoring
