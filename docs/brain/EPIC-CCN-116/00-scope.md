# Phase 1: Scope Definition + Boundary Validation - EPIC-CCN-116

## Target Method Details

### Method Identification
- **Method Name**: `HandleFlatPosition_CleanupActivePositions`
- **File Path**: `src/V12_002.Orders.Callbacks.Execution.cs`
- **Current Complexity**: 17
- **Target Complexity**: ≤8 (Jane Street HFT standard)
- **Overage**: +9 from target (112% over threshold)
- **Epic ID**: EPIC-CCN-116

### Method Signature
```csharp
// Exact signature to be confirmed during Phase 2 implementation planning
private void HandleFlatPosition_CleanupActivePositions(...)
```

## Extraction Strategy

### What to Extract

#### Extract 1: Position Validation Logic
- **Target Method**: `ValidatePositionForCleanup`
- **Responsibility**: Validate position state before cleanup
- **Expected Complexity**: ≤5
- **Rationale**: Isolate validation logic from cleanup operations

#### Extract 2: Active Position Cleanup Operations
- **Target Method**: `ExecuteActivePositionCleanup`
- **Responsibility**: Perform atomic cleanup of active positions
- **Expected Complexity**: ≤6
- **Rationale**: Separate cleanup mechanics from orchestration

#### Extract 3: Error Path Consolidation
- **Target Method**: `HandleCleanupError`
- **Responsibility**: Centralize error handling for cleanup failures
- **Expected Complexity**: ≤4
- **Rationale**: Simplify error paths using guard clauses

### What to Keep in Original Method
- High-level orchestration logic
- Method call sequencing
- State transition coordination
- **Residual Complexity Target**: ≤8

### Extraction Principles
1. **Single Responsibility**: Each extracted method handles one concern
2. **Lock-Free**: Maintain FSM/Actor pattern, no `lock()` blocks
3. **Atomic Operations**: Preserve consistency guarantees
4. **Testability**: Enable isolated unit testing of extracted logic

## Boundary Definition

### Scope Constraints (V12.23 No Scope Creep Protocol)

#### IN SCOPE
- ✅ `HandleFlatPosition_CleanupActivePositions` method ONLY
- ✅ Extract validation logic to new method
- ✅ Extract cleanup operations to new method
- ✅ Extract error handling to new method
- ✅ Refactor original method to orchestrate extracted methods

#### OUT OF SCOPE
- ❌ Modifications to caller methods
- ❌ Changes to position state management infrastructure
- ❌ Refactoring of related callback methods
- ❌ Database or persistence layer changes
- ❌ UI or presentation layer modifications

### Boundary Validation

#### Single-Method Scope Confirmation
**Question**: Does this extraction stay within a single method?
**Answer**: ✅ YES

**Justification**:
- Target method: `HandleFlatPosition_CleanupActivePositions` (1 method)
- Extraction creates NEW helper methods (not modifying existing methods)
- Original method remains in same file, same class
- No changes to method callers or external dependencies
- No cross-file refactoring required

#### Dependency Boundary Check

**Dependencies that WOULD violate boundary** (none detected):
- ❌ None - extraction is self-contained within method body

**Dependencies that are SAFE** (within scope):
- ✅ Position state validators (called by original method)
- ✅ Active position cleanup utilities (called by original method)
- ✅ Logging/diagnostic methods (called by original method)

**Rationale**: All dependencies are already called by the target method. Extraction moves existing calls into helper methods without changing dependency graph.

#### Explicit Boundary Statement
**Boundary Validated**: ✅ **YES**

**Confirmation**:
- Scope limited to single method: `HandleFlatPosition_CleanupActivePositions`
- No caller modifications required
- No external dependency changes
- No cross-file refactoring
- Extraction creates new private methods in same class
- V12.23 No Scope Creep Protocol: **COMPLIANT**

## Success Criteria

### Complexity Targets (Jane Street Alignment)
1. **Original Method**: Reduce from 17 to ≤8
2. **Extracted Method 1** (Validation): ≤5
3. **Extracted Method 2** (Cleanup): ≤6
4. **Extracted Method 3** (Error Handling): ≤4
5. **Total Complexity Budget**: ≤23 (sum of all methods)

### Functional Requirements
- ✅ Preserve all existing behavior
- ✅ Maintain atomic operation guarantees
- ✅ No performance degradation
- ✅ Lock-free implementation (FSM/Actor pattern)

### Quality Gates
- ✅ Zero compilation errors
- ✅ Zero Roslyn analyzer warnings
- ✅ CSharpier formatting compliance
- ✅ ASCII-only compliance (no Unicode)
- ✅ Build passes: `dotnet build src/V12_002.csproj`

### Testing Requirements
- ✅ Unit tests for each extracted method
- ✅ Integration test for original method orchestration
- ✅ Verify atomic operation guarantees preserved

## Risk Assessment

### Risk Level: MEDIUM

#### Risk Factors
1. **State Management Complexity**
   - **Impact**: HIGH
   - **Probability**: MEDIUM
   - **Mitigation**: Careful extraction preserving state transition logic

2. **Atomic Operation Guarantees**
   - **Impact**: HIGH
   - **Probability**: LOW
   - **Mitigation**: Verify FSM/Actor pattern maintained, no locks introduced

3. **Test Coverage Gaps**
   - **Impact**: MEDIUM
   - **Probability**: HIGH
   - **Mitigation**: Add comprehensive unit tests during extraction

4. **Regression Risk**
   - **Impact**: MEDIUM
   - **Probability**: LOW
   - **Mitigation**: Preserve exact behavior, verify with integration tests

### Risk Mitigation Strategy
1. **Phase 2**: Detailed implementation plan with verification steps
2. **Phase 3**: Adversarial audit by Arena AI before implementation
3. **Phase 5**: Automated verification against implementation plan
4. **Phase 6**: Manual sign-off with NinjaTrader F5 test

## Implementation Constraints

### V12 DNA Mandates
- **Lock-Free**: Use FSM/Actor `Enqueue` pattern, NEVER `lock()`
- **ASCII-Only**: No Unicode, emoji, or curly quotes in strings
- **Correctness by Construction**: Make illegal states unrepresentable
- **Jane Street Alignment**: Target complexity ≤8 for cognitive simplicity

### Hard-Link Integrity
- **Post-Modification**: Run `powershell -File .\deploy-sync.ps1`
- **Verification**: Confirm NinjaTrader hard links synchronized

### Code Quality
- **Formatting**: Auto-format with `dotnet csharpier format src/`
- **Linting**: Zero violations from `powershell -File .\scripts\lint.ps1`
- **Complexity**: Verify with `python scripts/complexity_audit.py`

## Next Steps (Phase 2)

### Implementation Planning Requirements
1. **Read Method Source**: Extract exact method signature and body
2. **Identify Extraction Points**: Mark lines for each extracted method
3. **Design Method Signatures**: Define parameters and return types
4. **Create Mermaid Diagrams**: Visualize before/after call flow
5. **Define Verification Criteria**: Specific checks for each extraction

### Phase 2 Deliverables
- `01-implementation-plan.md`: Detailed extraction plan with line numbers
- Mermaid sequence diagrams (before/after)
- Verification checklist for Phase 5

---
**Phase**: 1 (Scope + Boundary)
**Status**: ✅ COMPLETED
**Date**: 2026-06-13
**Analyst**: Bob Shell (v12-engineer)
**Boundary Validated**: ✅ YES
**Next Phase**: 2 (Implementation Planning)
