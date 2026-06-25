# Phase 1: Scope Definition - EPIC-W7-097

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.00
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T19:37:11Z

## Target Method
- **Method**: ExecuteRMAEntryV2
- **File**: src/V12_002.SIMA.Execution.cs
- **Line**: 686
- **Current CYC**: 14
- **Target CYC**: <=8
- **Lines of Code**: 159

## Scope Boundary Analysis

### CRITICAL PRE-FLIGHT CHECK
**Status**: VERIFICATION REQUIRED

The hotspot analysis reveals **0 callers** for ExecuteRMAEntryV2. This is unusual for a 159-line method and requires verification before proceeding:

**Possible Scenarios**:
1. **Dead Code**: Method is unused and should be deleted (not refactored)
2. **Reflection/Dynamic Dispatch**: Called via reflection (not detected by static analysis)
3. **Recently Added**: Not yet integrated into codebase
4. **Unindexed Callers**: Called from tests or external assemblies

**Required Action**: Phase 1.5 (Scope Boundary Validation) MUST verify actual usage before proceeding to Phase 2.

### IN SCOPE

#### Primary Extraction Target
- **ExecuteRMAEntryV2** (src/V12_002.SIMA.Execution.cs:686)
  - Current CYC: 14
  - Target CYC: <=8
  - Reduction needed: 6 complexity points

#### Extraction Strategy
Based on call hierarchy analysis, the following helper methods **already exist** and should be leveraged:

1. **Validation Logic** (ALREADY EXTRACTED)
   - ValidateRMAEntryGuards (line 319) - Entry validation
   - Verify this method is being called properly

2. **Calculation Logic** (ALREADY EXTRACTED)
   - CalculateRMABracketPrices (line 381) - Bracket price calculation
   - Verify this method is being called properly

3. **Submission Logic** (ALREADY EXTRACTED)
   - SubmitLocalRMAEntry (line 422) - Local RMA submission
   - Verify this method is being called properly

4. **Fleet Processing** (ALREADY EXTRACTED)
   - ProcessSingleFleetRMAAccount (line 511) - Fleet account processing
   - Verify this method is being called properly

#### Refactoring Approach
**Observation**: The hotspot analysis shows 78 callees, but many helper methods already exist. This suggests:
- ExecuteRMAEntryV2 may not be fully delegating to these helpers
- Additional inline logic needs extraction
- Method orchestration needs simplification

**Target Extractions** (if needed):
1. Extract remaining validation logic to existing ValidateRMAEntryGuards
2. Extract remaining calculation logic to existing CalculateRMABracketPrices
3. Extract symmetry guard logic (SymmetryGuardBeginDispatch, SymmetryGuardRollbackDispatch)
4. Simplify control flow to reduce nesting depth from 5 to 3 or less

### OUT OF SCOPE

#### Existing Helper Methods (DO NOT MODIFY)
These methods are already extracted and should NOT be refactored in this epic:
- ValidateRMAEntryGuards (line 319)
- CalculateRMABracketPrices (line 381)
- SubmitLocalRMAEntry (line 422)
- ProcessSingleFleetRMAAccount (line 511)

#### Downstream Dependencies (DO NOT MODIFY)
The 78 callees identified in the call hierarchy are OUT OF SCOPE:
- Depth 1 callees (14 methods) - use as-is
- Depth 2 callees (42 methods) - use as-is
- Depth 3 callees (22 methods) - use as-is

**Rationale**: Zero blast radius means we can refactor ExecuteRMAEntryV2 in isolation without touching dependencies.

#### Other Files (DO NOT MODIFY)
- src/V12_002.Symmetry.cs - Symmetry guard methods
- src/V12_002.Perf.LogBuffer.cs - Logging infrastructure
- src/V12_002.cs - Core strategy file

### Blast Radius Confirmation
- **Direct Callers**: 0 (IDEAL - no cascading changes)
- **Importers**: 0 (IDEAL - no external dependencies)
- **Overall Risk Score**: 0.0 (LOW RISK)

**Implication**: This epic can be executed surgically without impacting other code.

## Complexity Reduction Plan

### Current State
- **CYC**: 14
- **Max Nesting**: 5
- **Lines**: 159
- **Assessment**: HIGH complexity

### Target State
- **CYC**: <=8 (Jane Street strict standard)
- **Max Nesting**: <=3
- **Lines**: <50 per method
- **Assessment**: LOW complexity

### Reduction Strategy
1. **Delegate to Existing Helpers** (Priority 1)
   - Ensure all validation goes through ValidateRMAEntryGuards
   - Ensure all calculations go through CalculateRMABracketPrices
   - Ensure all submissions go through SubmitLocalRMAEntry
   - Ensure all fleet processing goes through ProcessSingleFleetRMAAccount

2. **Extract Remaining Logic** (Priority 2)
   - Extract symmetry guard orchestration (if inline)
   - Extract error handling logic (if inline)
   - Extract logging logic (if inline)

3. **Simplify Control Flow** (Priority 3)
   - Replace nested if/else with early returns
   - Replace nested loops with helper methods
   - Reduce nesting depth from 5 to 3 or less

### Expected Outcome
- ExecuteRMAEntryV2 becomes a thin orchestrator (CYC <=8)
- All complex logic delegated to single-responsibility helpers
- Method length reduced to <50 lines
- Nesting depth reduced to 3 or less

## Risk Assessment

### Overall Risk: LOW
**Factors**:
- Zero blast radius (no callers to break)
- No external dependencies
- Helper methods already exist
- Can refactor in isolation
- Possible dead code (requires verification)

### Mitigation Strategy
1. **Phase 1.5 Verification**: Confirm method is actually used
2. **TDD Safety Net**: Add unit tests before refactoring
3. **Incremental Extraction**: Extract one concern at a time
4. **Build Verification**: Run dotnet build after each extraction
5. **Deploy Sync**: Run deploy-sync.ps1 after all changes
6. **Integration Test**: F5 in NinjaTrader to verify BUILD_TAG

## Success Criteria

### Phase 1 (Scope Definition) - COMPLETE
- Scope boundaries defined (IN SCOPE vs OUT OF SCOPE)
- Extraction strategy documented
- Risk assessment completed
- 00-scope.md created

### Phase 1.5 (Scope Boundary Validation) - REQUIRED NEXT
- Verify ExecuteRMAEntryV2 is actually used (0 callers detected)
- Search for reflection/dynamic calls
- Check test files for usage
- Confirm method is not dead code

### Phase 2 (Architecture Planning) - PENDING
- Detailed extraction plan
- Method signatures for new helpers (if needed)
- Control flow diagrams

### Phase 5 (Ticket Execution) - PENDING
- CYC reduced from 14 to <=8
- Nesting depth reduced from 5 to <=3
- Lines reduced from 159 to <50
- All tests passing
- BUILD_TAG verified

## Jane Street Alignment
- **Current CYC**: 14 (75% over threshold)
- **Target CYC**: <=8 (Jane Street strict standard)
- **Approach**: Delegate to existing helpers, extract remaining inline logic
- **Rationale**: Microsecond-latency reasoning requires cognitive simplicity

## Conclusion

**Scope Status**: DEFINED with VERIFICATION GATE

ExecuteRMAEntryV2 is a well-bounded refactoring target:
- Clear complexity reduction goal (14 to <=8)
- Zero blast radius (no callers)
- Helper methods already exist
- Can be refactored surgically

**CRITICAL GATE**: Phase 1.5 MUST verify method usage before proceeding to Phase 2. If method is dead code, recommend deletion instead of refactoring.

**Recommended Next Step**: Execute Phase 1.5 (Scope Boundary Validation) to confirm method is actually used.
