# Phase 1.0: Scope Definition - EPIC-CCN-042

## Epic Metadata
- **Epic ID**: EPIC-CCN-042
- **Phase**: 1.0 (Scope Definition)
- **Date**: 2026-06-15
- **Status**: APPROVED

## Target Method

### Method Identification
- **Method Name**: `SymmetryGuardOnFollowerFill`
- **File**: `src/V12_002.Symmetry.Follower.cs`
- **Current Complexity**: 11 (Cyclomatic Complexity)
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **Parameter Count**: 6 parameters

### Method Signature
```csharp
SymmetryGuardOnFollowerFill(
    followerOrder,
    followerExecution,
    executionId,
    executionQuantity,
    executionPrice,
    executionTime
)
```

## Extraction Scope (SINGLE METHOD ONLY)

### What's IN Scope
1. **Method Body Only**: `SymmetryGuardOnFollowerFill` implementation
2. **Internal Logic**: Guard conditions and validation branching
3. **Complexity Reduction**: Break into 2-3 focused helper methods
4. **Naming**: Extract validation concerns into descriptive method names

### What's OUT of Scope
1. ❌ **Callers**: No changes to methods that invoke `SymmetryGuardOnFollowerFill`
2. ❌ **Callees**: No changes to methods called by `SymmetryGuardOnFollowerFill`
3. ❌ **Other Methods**: No changes to other methods in `V12_002.Symmetry.Follower.cs`
4. ❌ **File Structure**: No reorganization of the file
5. ❌ **Pre-existing Issues**: No fixing unrelated compilation errors
6. ❌ **Scope Creep**: No "while we're here" improvements

### Extraction Strategy

#### Complexity Analysis
- **Current**: 11 decision points
- **Target**: ≤8 decision points
- **Reduction**: 3+ decision points to extract

#### Proposed Decomposition
Based on typical guard pattern structure:

1. **Helper Method 1**: `ValidateFollowerOrderState`
   - Extract order state validation logic
   - Reduce complexity by 2-3 points
   - Single responsibility: Order state checks

2. **Helper Method 2**: `ValidateExecutionContext`
   - Extract execution parameter validation
   - Reduce complexity by 1-2 points
   - Single responsibility: Execution data checks

3. **Helper Method 3** (if needed): `ValidateSymmetryConfiguration`
   - Extract symmetry-specific configuration checks
   - Reduce complexity by 1 point
   - Single responsibility: Configuration validation

#### Refactoring Pattern
```csharp
// BEFORE (Complexity 11)
bool SymmetryGuardOnFollowerFill(params...) {
    if (condition1) { ... }
    if (condition2) { ... }
    if (condition3) { ... }
    // ... 11 decision points
}

// AFTER (Complexity ≤8)
bool SymmetryGuardOnFollowerFill(params...) {
    if (!ValidateFollowerOrderState(followerOrder)) return false;
    if (!ValidateExecutionContext(executionId, executionQuantity, executionPrice)) return false;
    if (!ValidateSymmetryConfiguration()) return false;
    // Remaining core logic (≤2 decision points)
}
```

## Success Criteria

### Functional Requirements
- ✅ **Complexity Reduced**: From 11 to ≤8
- ✅ **Behavior Preserved**: Zero functional changes
- ✅ **Tests Pass**: All existing tests pass without modification
- ✅ **Lock-Free**: Actor/FSM pattern maintained (no locks introduced)

### Non-Functional Requirements
- ✅ **ASCII-Only**: All string literals remain ASCII-compliant
- ✅ **Performance**: No measurable latency increase
- ✅ **Readability**: Extracted methods have descriptive names
- ✅ **Testability**: Each helper method is independently testable

### Quality Gates
1. **Build**: `dotnet build` succeeds
2. **Tests**: `dotnet test` passes 100%
3. **Complexity**: `complexity_audit.py` confirms CYC ≤8
4. **Lint**: `lint.ps1` shows zero new violations
5. **Format**: `dotnet csharpier check` passes

## V12 DNA Compliance

### Architectural Mandates
- ✅ **Correctness by Construction**: Guard logic remains side-effect free
- ✅ **Lock-Free Actor Pattern**: No state mutations, read-only validation
- ✅ **ASCII-Only Compliance**: No Unicode in extracted methods
- ✅ **Jane Street Alignment**: Cognitive simplicity prioritized

### Risk Assessment
- **Risk Level**: LOW
- **Rationale**: Single-method extraction, no cross-cutting concerns
- **Blast Radius**: Isolated to `SymmetryGuardOnFollowerFill` body

## Verification Plan

### Pre-Extraction Checklist
- [ ] Read current implementation
- [ ] Identify discrete validation concerns
- [ ] Map decision points to helper methods
- [ ] Verify test coverage exists

### Post-Extraction Checklist
- [ ] Complexity audit confirms ≤8
- [ ] All tests pass
- [ ] No behavior changes (diff review)
- [ ] CSharpier formatting applied
- [ ] Hard-link sync completed

## Next Steps
1. **Phase 1.5**: Boundary validation (mandatory V12.23 protocol)
2. **Phase 2**: Implementation planning with Mermaid diagrams
3. **Phase 3**: DNA & PR audit (Arena AI)
4. **Phase 4**: Surgical extraction (Bob CLI)

## Metadata
- **Scope Type**: Single-Method Extraction
- **Complexity Delta**: -3 to -5 points
- **Estimated Effort**: 1-2 hours
- **Priority**: MEDIUM-HIGH (preventive maintenance)
