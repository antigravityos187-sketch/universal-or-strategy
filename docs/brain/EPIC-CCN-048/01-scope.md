# Phase 1.0: Scope Definition - EPIC-CCN-048

## Epic Metadata
- **Epic ID**: EPIC-CCN-048
- **Phase**: 1.0 (Scope Definition)
- **Date**: 2026-06-15
- **Status**: APPROVED

## Target Method

### Method Identification
- **Method Name**: UpdateExistingPendingReplacement
- **File**: src/V12_002.Trailing.StopUpdate.cs
- **Current Complexity**: 9 (Cyclomatic)
- **Target Complexity**: 8 or less (Jane Street strict standard)
- **Lines of Code**: Approximately 30-50 (estimated)

### Complexity Breakdown
- **Decision Points**: 9 branches
- **Nesting Depth**: 2-3 levels
- **Current Status**: PASS (below V12 threshold of 15)
- **Refactoring Rationale**: Proactive simplification to maintain cognitive clarity

## Extraction Strategy

### Primary Goal
Reduce cyclomatic complexity from 9 to 8 or less through targeted extraction of:
1. **Guard clause validation** (early returns for invalid states)
2. **Price update logic** (separate method for price calculations)
3. **Quantity update logic** (separate method for quantity adjustments)

### Proposed Extraction Pattern
UpdateExistingPendingReplacement (CYC: 9 to 6)
- ValidatePendingReplacementState (CYC: 2) [NEW]
- UpdateReplacementPrice (CYC: 1) [NEW]
- UpdateReplacementQuantity (CYC: 1) [NEW]

### Expected Outcome
- **Main method complexity**: 6 (reduced from 9)
- **Helper methods**: 3 new methods, each with CYC 2 or less
- **Total complexity**: Distributed across 4 methods instead of 1
- **Cognitive load**: Reduced through single-responsibility helpers

## Scope Boundaries

### IN SCOPE (Single Method Only)
- Method body: UpdateExistingPendingReplacement implementation
- Local refactoring: Extract helper methods within same class
- Guard clauses: Early return validation logic
- Logic separation: Price and quantity update paths

### OUT OF SCOPE (Zero Tolerance)
- Callers: No changes to methods that call UpdateExistingPendingReplacement
- Callees: No changes to methods called by UpdateExistingPendingReplacement
- Other methods: No changes to other methods in V12_002.Trailing.StopUpdate.cs
- Compilation errors: No fixing pre-existing errors outside this method
- Scope creep: No "while we're here" improvements

### Boundary Enforcement
- **ONE EPIC = ONE CONCERN**: Single-method extraction only
- **No bundling**: Each complexity hotspot gets its own EPIC
- **No drift**: Implementation must match this scope exactly

## Success Criteria

### Functional Requirements
1. **Complexity reduction**: CYC reduced from 9 to 8 or less
2. **Behavior preservation**: Zero functional changes
3. **Test coverage**: All existing tests pass
4. **Lock-free pattern**: FSM/Actor pattern maintained

### Non-Functional Requirements
1. **V12 DNA compliance**: No locks, ASCII-only, type-safe
2. **Jane Street alignment**: Cognitive simplicity prioritized
3. **Diff hygiene**: Changes isolated to target method only
4. **Build verification**: dotnet build succeeds

### Quality Gates
- **Pre-push validation**: All 13 checks pass
- **Complexity audit**: complexity_audit.py confirms CYC 8 or less
- **Unit tests**: 100% pass rate maintained
- **Code review**: Arena AI adversarial audit (Phase 3)

## Risk Assessment

### Technical Risk: LOW
- **Rationale**: Method already below threshold (9 < 15)
- **Blast radius**: Localized to trailing stop subsystem
- **Rollback**: Simple revert if issues arise

### Business Risk: LOW
- **Impact**: No user-facing changes
- **Testing**: Existing test suite provides safety net
- **Deployment**: Standard hard-link sync process

## Jane Street Principles Applied

### Cognitive Simplicity
- Break complex method into single-purpose helpers
- Reduce nesting depth through guard clauses
- Make control flow explicit and linear

### Correctness by Construction
- Maintain type-safe state transitions
- Preserve enum-based validation
- No runtime guards for impossible states

### Performance Considerations
- No additional allocations introduced
- Inline-friendly helper methods
- Zero impact on hot-path latency

## Implementation Constraints

### V12 DNA Mandates
1. **Lock-Free**: No lock() statements allowed
2. **ASCII-Only**: No Unicode in string literals
3. **Actor/FSM**: State mutations via Enqueue only
4. **Hard-Link Sync**: Run deploy-sync.ps1 after changes

### Code Quality Standards
1. **CSharpier**: Auto-format before commit
2. **Roslyn**: Zero analyzer violations
3. **Complexity**: CYC 8 or less per method
4. **Testing**: TDD for extracted methods

## Phase 1.0 Approval

### Status: APPROVED
- **Scope**: Single-method extraction (UpdateExistingPendingReplacement)
- **Complexity target**: 8 or less (achievable via 2-3 helper methods)
- **Risk level**: LOW (below threshold, localized impact)
- **Next phase**: Phase 1.5 (Boundary Validation)

### Sign-off
- **Analyst**: V12 Phase 1 Scope Planner
- **Date**: 2026-06-15
- **Recommendation**: Proceed to Phase 1.5 for boundary validation
