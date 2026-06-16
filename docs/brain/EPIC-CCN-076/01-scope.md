# Phase 1.0: Scope Definition - EPIC-CCN-076

## Target Method
- **Method Name**: CollapseAllExecutionControls
- **File**: src/V12_002.UI.Panel.Handlers.cs
- **Current Complexity**: 11
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **Lines of Code**: TBD (requires manual inspection)

## Extraction Strategy

### Primary Goal
Reduce cyclomatic complexity from 11 to ≤8 through surgical extraction of 2-3 helper methods.

### Approach
1. **Identify Decision Points**: Locate conditional branches and loops contributing to complexity
2. **Extract Logical Units**: Create helper methods for cohesive logic blocks
3. **Preserve Semantics**: Maintain exact behavior, no logic changes
4. **Maintain Actor Pattern**: Ensure lock-free FSM/Actor pattern compliance

### Expected Extractions
- **Helper Method 1**: Extract conditional logic block (estimated CYC reduction: 2-3)
- **Helper Method 2**: Extract loop/iteration logic (estimated CYC reduction: 1-2)
- **Helper Method 3** (if needed): Extract additional branching logic (estimated CYC reduction: 1-2)

## Boundary Definition

### IN SCOPE (ONLY)
- ✅ Method body of CollapseAllExecutionControls
- ✅ Extraction of 2-3 private helper methods
- ✅ Complexity reduction from 11 to ≤8
- ✅ Unit test updates (if tests exist for this method)

### OUT OF SCOPE (STRICTLY FORBIDDEN)
- ❌ Callers of CollapseAllExecutionControls
- ❌ Callees invoked by CollapseAllExecutionControls
- ❌ Other methods in V12_002.UI.Panel.Handlers.cs
- ❌ Pre-existing compilation errors in the file
- ❌ Style/formatting improvements beyond extraction
- ❌ Performance optimizations
- ❌ Feature additions or behavior changes

### Scope Creep Prevention
**ONE EPIC = ONE CONCERN**: This epic addresses ONLY the complexity of CollapseAllExecutionControls. No "while we're here" improvements allowed.

## Success Criteria

### Functional Requirements
1. ✅ Cyclomatic complexity reduced from 11 to ≤8
2. ✅ All existing tests pass (100% pass rate)
3. ✅ No behavior changes (semantic equivalence verified)
4. ✅ Lock-free Actor/FSM pattern maintained (no lock() statements)

### Quality Gates
1. ✅ CSharpier formatting check passes
2. ✅ Build succeeds with zero errors
3. ✅ Complexity audit confirms CYC ≤8
4. ✅ ASCII-only compliance maintained

### Documentation Requirements
1. ✅ XML documentation for new helper methods
2. ✅ Inline comments preserved from original method
3. ✅ Extraction rationale documented in commit message

## Risk Assessment
- **Complexity Risk**: LOW (CYC=11, manageable reduction to ≤8)
- **Blast Radius**: MINIMAL (single method, no caller changes)
- **Regression Risk**: LOW (behavior preservation enforced)
- **Overall Risk**: LOW

## Verification Plan
1. **Pre-Surgery**: Run complexity audit, capture baseline metrics
2. **Post-Surgery**: Re-run complexity audit, verify CYC ≤8
3. **Testing**: Execute unit tests, verify 100% pass rate
4. **Build**: Run build_readiness.ps1, verify zero errors
5. **Deploy**: Run deploy-sync.ps1, verify hard-link integrity

## Jane Street Alignment
- **Cognitive Simplicity**: CYC ≤8 ensures functions are easy to reason about
- **Testability**: Lower complexity enables exhaustive path testing
- **Auditability**: Simpler logic reduces race condition audit surface
- **V12 DNA**: "Make illegal states unrepresentable" - requires simple, verifiable logic

## Notes
- jCodemunch tools unavailable during Phase 0 analysis
- Manual code review required before extraction
- Extraction must preserve exact control flow semantics
