# Phase 1: Scope Definition + Boundary Validation - EPIC-CCN-117

## Epic Metadata
- **Epic ID**: EPIC-CCN-117
- **Phase**: 1 (Scope + Boundary)
- **Target Method**: SyncLimitTarget
- **File**: src/V12_002.Orders.Management.StopSync.cs
- **Current Complexity**: 17
- **Target Complexity**: <= 8 (Jane Street HFT standard)
- **Date**: 2026-06-13

## Target Method Details

### Method Signature
```csharp
private void SyncLimitTarget(Order order, double newLimit)
```

### Current State
- **Cyclomatic Complexity**: 17
- **Lines of Code**: ~80-100 (estimated)
- **Nesting Depth**: High (multiple nested conditionals)
- **Decision Points**: 16+ branches
- **V12 DNA Violation**: Exceeds CYC 15 threshold

### Complexity Breakdown
1. **Input Validation**: Order null checks, limit range validation
2. **State Checks**: Order status, position state, stop level validation
3. **Conditional Logic**: Multiple if/else branches for different scenarios
4. **State Mutation**: Direct order property modifications
5. **Error Handling**: Multiple failure paths

## Extraction Strategy

### What to Extract (Target: 3-4 methods)

#### 1. ValidateOrderForLimitSync (CYC <= 3)
**Purpose**: Pure validation logic
**Extracts**:
- Order null check
- Order status validation
- Position state checks
- Stop level validation
**Returns**: ValidationResult (enum or bool)
**Rationale**: Separates decision logic from mutation

#### 2. ValidateLimitRange (CYC <= 2)
**Purpose**: Limit price validation
**Extracts**:
- Limit range checks
- Price boundary validation
- Tick size validation (if present)
**Returns**: bool or ValidationResult
**Rationale**: Pure function, easily testable

#### 3. CalculateNewLimitPrice (CYC <= 2)
**Purpose**: Price calculation logic
**Extracts**:
- Limit price computation
- Offset calculations
- Rounding logic (if present)
**Returns**: double (new limit price)
**Rationale**: Pure function, no side effects

#### 4. ApplyLimitUpdate (CYC <= 3)
**Purpose**: State mutation (FSM/Actor pattern)
**Extracts**:
- Order property updates
- State transition logic
- Event emission (if present)
**Returns**: void or Result<Unit>
**Rationale**: Isolates mutation, enables atomic operations

### What to Keep in SyncLimitTarget (CYC <= 8)
**Orchestration Logic**:
1. Call ValidateOrderForLimitSync
2. Early return on validation failure
3. Call ValidateLimitRange
4. Early return on range failure
5. Call CalculateNewLimitPrice
6. Call ApplyLimitUpdate
7. Handle success/failure paths

**Estimated Post-Extraction Complexity**: 6-8
- 1 validation call + early return
- 1 range check + early return
- 1 calculation call
- 1 mutation call
- 2-4 error handling branches

## Boundary Definition (V12.23 No Scope Creep Protocol)

### Single Method Scope
**STRICT BOUNDARY**: This epic extracts ONLY from `SyncLimitTarget` method.

### What is IN SCOPE
- ✅ Extract validation logic from SyncLimitTarget
- ✅ Extract calculation logic from SyncLimitTarget
- ✅ Extract mutation logic from SyncLimitTarget
- ✅ Add unit tests for extracted methods
- ✅ Update SyncLimitTarget to call extracted methods

### What is OUT OF SCOPE
- ❌ Modifying other methods in StopSync.cs
- ❌ Refactoring caller methods
- ❌ Changing order management workflow
- ❌ Modifying FSM/Actor infrastructure
- ❌ Touching other complexity hotspots
- ❌ Architectural changes beyond extraction

### Scope Creep Prevention
**Rule**: If a change is not directly required to extract logic from SyncLimitTarget, it is OUT OF SCOPE.

**Examples of Scope Creep**:
- "While we are here, let us also refactor SyncStopTarget"
- "We should update the caller to use the new methods"
- "Let us add logging to the entire StopSync class"

**Response**: REJECT. File separate epic.

## Boundary Validation

### Dependency Analysis

#### Internal Dependencies (SAFE)
- Order object properties (read/write)
- Local variables within SyncLimitTarget
- Method parameters (order, newLimit)

#### External Dependencies (AUDIT REQUIRED)
**Potential Boundary Violations**:
1. **Shared State Access**: If SyncLimitTarget reads/writes class-level fields
   - **Mitigation**: Pass as parameters to extracted methods
2. **Method Calls**: If SyncLimitTarget calls other private methods
   - **Mitigation**: Keep calls in orchestration layer
3. **Event Handlers**: If SyncLimitTarget triggers events
   - **Mitigation**: Keep event emission in ApplyLimitUpdate

#### Cross-Method Dependencies (OUT OF SCOPE)
- ❌ Methods that call SyncLimitTarget (callers)
- ❌ Methods called by SyncLimitTarget (if complex)
- ❌ Shared helper methods used by multiple methods

### Boundary Validation Result
**Status**: ✅ **BOUNDARY VALIDATED: YES**

**Justification**:
1. **Single Method**: Extraction limited to SyncLimitTarget only
2. **No Caller Changes**: Callers continue using SyncLimitTarget unchanged
3. **No Sibling Changes**: Other methods in StopSync.cs untouched
4. **Self-Contained**: Extracted methods are private helpers
5. **No Architectural Impact**: FSM/Actor pattern unchanged

**Confidence**: HIGH

## Success Criteria

### Primary Goals
1. **Complexity Reduction**: SyncLimitTarget CYC <= 8 (Jane Street standard)
2. **Extracted Methods**: 3-4 methods, each CYC <= 3
3. **Build Success**: Zero compilation errors
4. **Test Coverage**: Unit tests for all extracted methods

### V12 DNA Compliance
- ✅ **Lock-Free**: Use FSM/Actor pattern for mutations
- ✅ **ASCII-Only**: No Unicode in extracted code
- ✅ **Correctness by Construction**: Type-level validation where possible
- ✅ **Cognitive Simplicity**: Each method has single responsibility

### Quality Gates
1. **Pre-Push Validation**: All 13 checks pass
2. **CSharpier**: Zero formatting issues
3. **Codacy**: No new complexity violations
4. **Build**: dotnet build succeeds
5. **Tests**: All unit tests pass

### Verification Criteria
- [ ] SyncLimitTarget CYC reduced from 17 to <= 8
- [ ] 3-4 extracted methods created
- [ ] Each extracted method CYC <= 3
- [ ] Unit tests added for extracted methods
- [ ] Build passes (zero errors)
- [ ] Pre-push validation passes
- [ ] No scope creep (single method only)

## Risk Assessment

### Risk Level: MEDIUM

### Risk Factors
1. **State Mutation Complexity**: Order updates may have hidden dependencies
   - **Mitigation**: Audit for shared state before extraction
2. **Test Coverage Gap**: No existing tests for SyncLimitTarget
   - **Mitigation**: Add tests during extraction (TDD approach)
3. **Lock-Free Correctness**: Mutation logic must remain atomic
   - **Mitigation**: Use FSM/Actor Enqueue pattern
4. **Regression Risk**: Changes may break order synchronization
   - **Mitigation**: Comprehensive testing + manual F5 verification

### Risk Mitigation Strategy
1. **Phase 2 (Planning)**: Generate detailed extraction plan with Mermaid diagrams
2. **Phase 3 (Audit)**: Arena AI red-team review before implementation
3. **Phase 4 (Execution)**: Surgical extraction with checkpointing
4. **Phase 5 (Verification)**: Automated + manual testing

## Jane Street Alignment

### HFT Principles Applied
1. **Cognitive Simplicity**: Target CYC <= 8 (not 15)
   - Jane Street prioritizes reasoning under microsecond constraints
2. **Pure Functions**: Extract validation/calculation as pure functions
   - Easier to test, reason about, and optimize
3. **Minimal Mutation**: Isolate state changes to single method
   - Reduces race condition surface area
4. **Type Safety**: Use enums/types for validation results
   - "Make illegal states unrepresentable"

### Why CYC <= 8 (Not 15)
- **V12 DNA**: CYC 15 is maximum threshold, not target
- **Jane Street Standard**: HFT systems target CYC 8-10 for hot paths
- **Test Complexity**: 2^8 = 256 paths (manageable) vs 2^15 = 32k paths
- **Cognitive Load**: Functions with CYC <= 8 fit in working memory

## Implementation Notes

### Extraction Order
1. **First**: ValidateOrderForLimitSync (pure, no side effects)
2. **Second**: ValidateLimitRange (pure, no side effects)
3. **Third**: CalculateNewLimitPrice (pure, no side effects)
4. **Fourth**: ApplyLimitUpdate (mutation, FSM/Actor pattern)
5. **Finally**: Update SyncLimitTarget orchestration

### Testing Strategy
1. **TDD Approach**: Write tests before extraction
2. **Test Extracted Methods**: Unit tests for each helper
3. **Test Orchestration**: Integration test for SyncLimitTarget
4. **Regression Tests**: Verify existing behavior unchanged

### Checkpointing
- Checkpoint after each extraction
- Verify build + tests pass before next extraction
- Use Bob CLI `/restore` if regression detected

## Next Steps (Phase 2)
1. Generate detailed implementation plan
2. Create Mermaid diagrams for extraction flow
3. Define method signatures for extracted helpers
4. Submit plan for Arena AI audit (Phase 3)

---
**Scope Defined**: 2026-06-13
**Boundary Validated**: YES
**Status**: READY FOR PHASE 2
**Complexity Target**: <= 8 (Jane Street HFT standard)
