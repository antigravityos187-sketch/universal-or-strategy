# Phase 1: Scope Definition - EPIC-W7-050

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.19
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T19:30:17Z

## Epic Objective
Reduce cyclomatic complexity of FleetSync_SyncFollowersToLevel from CYC 13 to ≤8 through surgical extraction of helper methods.

## Target Method
- **Method**: FleetSync_SyncFollowersToLevel
- **File**: src/V12_002.Trailing.cs
- **Line**: 142
- **Current CYC**: 13
- **Target CYC**: ≤8
- **Lines of Code**: 50
- **Max Nesting Depth**: 5

## IN SCOPE

### Primary Target
✅ **FleetSync_SyncFollowersToLevel method body** (lines 142-192)
- All logic within the method boundaries
- All conditional branches contributing to CYC
- All nested loops and error handling

### Extraction Candidates (Based on 48 Callees)
✅ **Validation Logic Extraction**
- ValidateStopPrice calls and related validation checks
- Input parameter validation
- Price range validation

✅ **Stop Replacement Flow Extraction**
- InitiateStopReplacement orchestration
- CreateDirectStopOrder calls
- Stop order lifecycle management

✅ **Error Handling Extraction**
- HandleUpdateException calls
- Try-catch block consolidation
- Error logging and recovery logic

✅ **Stop Calculation Extraction**
- CalculateStopForLevel calls
- Price adjustment logic
- Level-based stop computation

### Allowed Modifications
✅ Extract 3-5 private helper methods within V12_002.Trailing.cs
✅ Refactor conditional logic to reduce nesting depth
✅ Consolidate duplicate validation checks
✅ Add XML documentation to extracted methods
✅ Update existing unit tests (if any)

## OUT OF SCOPE

### Callers (DO NOT MODIFY)
❌ **ManageTrail_RunFleetSymmetrySync** (line 99)
- Caller context must remain unchanged
- Method signature of FleetSync_SyncFollowersToLevel must be preserved

❌ **ManageTrailingStops** (line 39)
- Indirect caller via ManageTrail_RunFleetSymmetrySync
- No changes to calling patterns

### Deep Callees (DO NOT REFACTOR)
❌ **UpdateStopOrder** - Existing order update infrastructure
❌ **CalculateStopForLevel** - Existing calculation logic
❌ **ValidateStopPrice** - Existing validation infrastructure
❌ **InitiateStopReplacement** - Existing replacement flow
❌ **CreateDirectStopOrder** - Existing order creation
❌ **HandleUpdateException** - Existing error handling
❌ **LogBuffer.Format** - Existing logging infrastructure

### Related Systems (DO NOT TOUCH)
❌ Fleet synchronization architecture
❌ Trailing stop state management
❌ Order management subsystem
❌ Broker integration layer
❌ FSM/Actor patterns in other files

### Testing Scope
❌ Integration tests (preserve existing behavior)
❌ End-to-end fleet synchronization tests
❌ Broker connectivity tests

## Scope Boundaries

### Method Signature Constraint
**IMMUTABLE**: The method signature must remain unchanged with 4 parameters (targetLevel, leaderStopPrice, leaderFsm, followerFsms)

**Rationale**: 2 callers depend on this exact signature. Any change would require cascading modifications (OUT OF SCOPE).

### File Boundary Constraint
**CONTAINED**: All extractions must remain within src/V12_002.Trailing.cs

**Rationale**:
- Zero external importers (blast radius = 0)
- Trailing stop logic is cohesive within this file
- Avoids cross-file dependencies

### Complexity Reduction Target
**GOAL**: CYC 13 → CYC ≤8 (38% reduction minimum)

**Strategy**:
1. Extract validation logic → -2 CYC
2. Extract stop replacement flow → -2 CYC
3. Extract error handling → -1 CYC
4. Simplify conditional nesting → -2 CYC
5. **Total Reduction**: -7 CYC (target: CYC 6)

### Behavioral Preservation
**MANDATORY**: Zero functional changes to:
- Fleet synchronization logic
- Stop price calculations
- Order update sequencing
- Error handling outcomes
- Logging output format

## Risk Mitigation

### Pre-Refactor Verification
1. ✅ Verify current CYC via get_symbol_complexity (resolve CYC 9 vs 13 discrepancy)
2. ✅ Capture current method source via get_symbol_source
3. ✅ Document all 48 callees for regression testing
4. ✅ Run existing unit tests (if any) to establish baseline

### Post-Refactor Validation
1. ✅ Verify CYC ≤8 via complexity_audit.py
2. ✅ Run dotnet build (zero errors)
3. ✅ Run deploy-sync.ps1 (hard link sync)
4. ✅ F5 in NinjaTrader IDE (integration test)
5. ✅ Verify BUILD_TAG in output

### Rollback Plan
If refactoring introduces regressions:
1. Revert to git commit before epic start
2. Document failure in docs/brain/EPIC-W7-050/FORENSIC_REPORT.md
3. Capture lesson to Firebase via capture_lesson.py

## Success Criteria

### Quantitative
- ✅ CYC reduced from 13 to ≤8
- ✅ Max nesting depth reduced from 5 to ≤3
- ✅ Zero new compilation errors
- ✅ Zero new test failures
- ✅ Method signature unchanged

### Qualitative
- ✅ Extracted methods have single responsibility
- ✅ Code readability improved (cognitive load reduced)
- ✅ XML documentation added to all extracted methods
- ✅ No scope creep (only target method modified)

## Data Discrepancy Resolution

**Issue**: Task brief stated CYC=9, but jCodemunch reports CYC=13 (44% discrepancy)

**Resolution Strategy**:
1. Phase 2 will capture current source via get_symbol_source
2. Manually verify CYC using Lizard or Visual Studio metrics
3. If CYC is actually 9, adjust extraction strategy accordingly
4. Document actual CYC in Phase 2 architecture plan

**Impact**: If CYC is 9 (not 13), extraction strategy may be over-engineered. Phase 2 will adjust scope if needed.

## Phase 1 Completion
- ✅ Scope boundaries defined (IN SCOPE vs OUT OF SCOPE)
- ✅ Method signature constraint documented
- ✅ File boundary constraint established
- ✅ Complexity reduction target set (CYC 13 → ≤8)
- ✅ Risk mitigation plan documented
- ✅ Success criteria defined

**Next Phase**: Phase 1.5 (Scope Boundary Validation)
