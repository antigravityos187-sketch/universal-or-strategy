# Phase 1.0: Scope Definition - EPIC-CCN-056

## Target Method
- **Method Name**: SweepBrokerOrders
- **File**: src/V12_002.SIMA.Lifecycle.cs
- **Current Complexity**: 12 (Cyclomatic Complexity)
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **Risk Level**: MEDIUM

## Extraction Strategy

### Primary Goal
Reduce cyclomatic complexity from 12 to ≤8 through surgical extraction of 2-3 helper methods.

### Approach
1. **Identify Decision Points**: Analyze the 12 complexity points (branches, loops, conditionals)
2. **Extract Cohesive Logic**: Group related decision points into helper methods
3. **Preserve Semantics**: Maintain exact behavior, no logic changes
4. **Maintain Actor Pattern**: Ensure all extracted methods respect FSM/Actor Enqueue model

### Expected Extractions
- **Helper Method 1**: Order validation/filtering logic (estimated CYC: 3-4)
- **Helper Method 2**: Order processing/execution logic (estimated CYC: 3-4)
- **Main Method**: Orchestration only (target CYC: ≤4)

## Boundary Definition

### IN SCOPE (Single Method Only)
- ✅ SweepBrokerOrders method body
- ✅ Internal logic extraction
- ✅ Helper method creation within same class
- ✅ Complexity reduction to ≤8

### OUT OF SCOPE (Zero Tolerance)
- ❌ Callers of SweepBrokerOrders
- ❌ Methods called by SweepBrokerOrders (callees)
- ❌ Other methods in V12_002.SIMA.Lifecycle.cs
- ❌ Pre-existing compilation errors
- ❌ "While we're here" improvements
- ❌ Refactoring unrelated code

## Success Criteria

### Functional Requirements
1. ✅ Complexity reduced from 12 to ≤8
2. ✅ All existing tests pass (100% pass rate)
3. ✅ No behavior changes (bit-for-bit identical output)
4. ✅ Lock-free Actor/FSM pattern maintained

### Non-Functional Requirements
1. ✅ ASCII-only compliance (no Unicode)
2. ✅ No new lock() statements
3. ✅ Atomic state transitions preserved
4. ✅ Hard-link integrity via deploy-sync.ps1

### Quality Gates
1. ✅ Pre-push validation passes (13 checks)
2. ✅ CSharpier formatting compliant
3. ✅ Codacy shows "Up to quality standards"
4. ✅ PR diff <10,000 characters

## V12 DNA Alignment

### Correctness by Construction
- Extract methods must make illegal states unrepresentable
- Type signatures enforce valid state transitions
- No runtime guards for design-time constraints

### Jane Street Principles
- **Cognitive Simplicity**: Each method does ONE thing
- **Testability**: Extracted methods are independently testable
- **Auditability**: Clear control flow, no hidden complexity

## Risk Mitigation

### Checkpointing
- Mandatory checkpointing enabled via Bob CLI
- Restore points at each extraction step
- Rollback capability if tests fail

### Verification Protocol
1. Extract first helper method
2. Run tests (must pass)
3. Extract second helper method
4. Run tests (must pass)
5. Verify final complexity ≤8
6. Run full pre-push validation

## Exclusions (Scope Creep Prevention)

### Explicitly NOT Doing
- ❌ Fixing other methods in same file
- ❌ Refactoring callers
- ❌ Optimizing performance
- ❌ Adding new features
- ❌ Changing method signatures
- ❌ Modifying test files

### Rationale
ONE EPIC = ONE CONCERN. Scope creep is the #1 cause of failed refactorings. This epic focuses exclusively on reducing SweepBrokerOrders complexity.

## Approval Status
- **Status**: PENDING (awaiting Phase 1.5 boundary validation)
- **Next Phase**: Phase 1.5 - Boundary Validation
