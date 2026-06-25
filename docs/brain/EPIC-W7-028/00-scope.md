# Phase 1: Scope Definition - EPIC-W7-028

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.18
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T19:26:46Z

## Epic Objective
Reduce cyclomatic complexity of ProcessFlattenWorkItem_CancelOrders from 17 to <=8 through surgical extraction of nested conditionals.

## Target Method
- **Method**: ProcessFlattenWorkItem_CancelOrders
- **File**: src/V12_002.SIMA.Flatten.cs
- **Line**: 191
- **Current CYC**: 17
- **Target CYC**: <=8
- **Lines of Code**: 48

## Scope Boundaries

### IN SCOPE

#### Primary Extraction Target
1. **ProcessFlattenWorkItem_CancelOrders** (lines 191-239)
   - Extract nested conditional logic
   - Preserve public method signature
   - Maintain all 5 caller contracts

#### Extraction Candidates (Based on Nested Logic)
2. **Order Validation Logic**
   - Extract order null checks and state validation
   - Target CYC: 2-3
   - Estimated LOC: 8-10

3. **Cancel Decision Logic**
   - Extract cancel eligibility checks
   - Target CYC: 3-4
   - Estimated LOC: 10-12

4. **Cancel Execution Logic**
   - Extract actual cancel operation and logging
   - Target CYC: 2-3
   - Estimated LOC: 8-10

#### Supporting Infrastructure
5. **LogBuffer calls** (preserve as-is)
   - LogBuffer.Format
   - LogBuffer.ValidateThreadAffinity
   - LogBuffer.FormatInternal

### OUT OF SCOPE

#### Caller Methods (No Changes)
1. **PumpFlattenOps** (line 124) - caller only
2. **PerformFallbackFlatten** (line 328) - caller only
3. **FlattenAllApexAccounts** (line 38) - caller only
4. **ChainNextFlattenOp** (line 376) - caller only
5. **ClosePositionsOnlyApexAccounts** (line 516) - caller only

#### External Dependencies (No Changes)
6. **LogBuffer infrastructure** - preserve all calls
7. **Order class** - no modifications
8. **SIMA_FSM class** - no modifications

#### Other Flatten Methods
9. **ProcessFlattenWorkItem_ClosePositions** - separate epic
10. **ProcessFlattenWorkItem_ModifyOrders** - separate epic
11. **Any other methods in V12_002.SIMA.Flatten.cs** - out of scope

### BOUNDARY VALIDATION

#### Scope Creep Prevention
- **File Boundary**: Changes limited to src/V12_002.SIMA.Flatten.cs only
- **Method Boundary**: Only ProcessFlattenWorkItem_CancelOrders and new extracted helpers
- **Caller Boundary**: Zero changes to 5 caller methods
- **Dependency Boundary**: Zero changes to LogBuffer or Order classes

#### Risk Mitigation
- **Zero Blast Radius**: No external dependents to break
- **File-Local Callers**: All 5 callers in same file
- **Signature Preservation**: Public method signature unchanged
- **Test Coverage**: Add unit tests before extraction

## Extraction Strategy

### Phase 2 Architecture Plan
1. Extract 3 helper methods from ProcessFlattenWorkItem_CancelOrders
2. Each helper method: CYC <=4, LOC <=12
3. Main method reduced to: CYC <=8, orchestration only

### Expected Outcome
- **Before**: 1 method, CYC=17, LOC=48
- **After**: 4 methods, CYC <=8 each, total LOC=48-52

### Success Criteria
- ProcessFlattenWorkItem_CancelOrders: CYC <=8
- All extracted methods: CYC <=8
- All 5 callers: unchanged
- LogBuffer calls: unchanged
- Build passes
- Unit tests pass

## Jane Street Alignment

### Complexity Reduction
- **Current**: CYC=17 (112% over threshold)
- **Target**: CYC <=8 (Jane Street strict standard)
- **Approach**: Extract nested conditionals to helper methods

### Cognitive Simplicity
- Break complex decision tree into named, single-purpose methods
- Each method represents one clear responsibility
- Easier to reason about under microsecond latency constraints

### V12 DNA Compliance
- "Make illegal states unrepresentable" - simpler logic, fewer edge cases
- Lock-free Actor pattern - preserved (no state mutations)
- ASCII-only - preserved (no string changes)

## Scope Validation

### Mandatory Gate (Phase 1.5)
Before proceeding to Phase 2, verify:
1. Scope boundaries clearly defined
2. IN SCOPE items are minimal and focused
3. OUT OF SCOPE items prevent scope creep
4. Extraction strategy is surgical (not rewrite)
5. Risk assessment confirms LOW-MEDIUM risk

### Approval Criteria
- **Scope Size**: SMALL (1 method + 3 extractions)
- **Blast Radius**: ZERO (no external dependents)
- **Caller Impact**: MINIMAL (signature preserved)
- **Risk Level**: LOW-MEDIUM (file-local changes only)

## Next Steps

### Phase 1.5: Scope Boundary Validation
- Verify no scope creep
- Confirm extraction boundaries
- Validate risk assessment

### Phase 2: Architecture Planning
- Design 3 extracted method signatures
- Map control flow transformations
- Plan test coverage strategy

## Conclusion

Scope is **TIGHTLY BOUNDED** and **SURGICAL**:
- Single method refactoring (ProcessFlattenWorkItem_CancelOrders)
- Zero changes to callers or dependencies
- Clear extraction boundaries (3 helper methods)
- Low risk (zero blast radius, file-local changes)

**Recommendation**: Proceed to Phase 1.5 (Scope Boundary Validation) with HIGH confidence.
