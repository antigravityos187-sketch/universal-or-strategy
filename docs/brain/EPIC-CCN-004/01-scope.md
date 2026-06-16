# Phase 1.0: Scope Definition - EPIC-CCN-004

## Extraction Scope (SINGLE METHOD ONLY)

### Target Method
- **Method Name**: HandleFleetTargetFill
- **File**: src/V12_002.UI.Compliance.cs
- **Current Complexity**: 16 (CYC)
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **Violation**: Exceeds threshold by 1 (15 is max)

### Extraction Strategy
Break HandleFleetTargetFill into 2-3 helper methods:

1. **ValidateFleetTarget()** - Pure validation logic
   - Input: Fleet target parameters
   - Output: Validation result (bool/enum)
   - Complexity target: ≤3

2. **ProcessFleetFill()** - Fill processing logic
   - Input: Validated fleet target, fill data
   - Output: Processing result
   - Complexity target: ≤3

3. **UpdateFleetState()** - State mutation coordination
   - Input: Processing result
   - Output: void (Actor/FSM enqueue)
   - Complexity target: ≤2

**Orchestrator**: HandleFleetTargetFill (reduced to ≤8)
- Calls extracted helpers in sequence
- Minimal branching logic
- Clear error handling paths

## Boundary Definition

### IN SCOPE
- HandleFleetTargetFill method body ONLY
- Extract pure logic to helper methods
- Maintain existing method signature
- Preserve all existing behavior
- Keep lock-free Actor/FSM pattern

### OUT OF SCOPE
- Callers: No changes to methods that call HandleFleetTargetFill
- Callees: No changes to methods called by HandleFleetTargetFill
- Other Methods: No changes to other methods in V12_002.UI.Compliance.cs
- File Structure: No changes to class structure or namespace
- Dependencies: No new external dependencies

### No Scope Creep: ONE EPIC = ONE CONCERN
- No "while we're here" improvements
- No fixing unrelated compilation errors
- No bundling multiple refactoring concerns
- No architectural changes beyond extraction
- No performance optimizations (unless required for correctness)

## Success Criteria

### Functional Requirements
1. Complexity Reduction: CYC reduced from 16 to ≤8
2. Behavior Preservation: All existing behavior unchanged
3. Test Coverage: All tests pass (existing + new)
4. Lock-Free Compliance: No lock(stateLock) blocks introduced
5. ASCII-Only: No Unicode in string literals

### Quality Gates
1. Build: Zero compilation errors
2. Tests: 100% pass rate (existing test suite)
3. Lint: Zero Roslyn violations
4. Formatting: CSharpier compliant
5. Complexity: Lizard audit shows CYC ≤8 for HandleFleetTargetFill

### V12 DNA Compliance
- Lock-Free Actor Pattern: Maintained
- Atomic State Mutations: Preserved
- Correctness by Construction: Enhanced (simpler logic)
- Cognitive Simplicity: Improved (CYC 16→8)
- ASCII-Only: Verified

### TDD Requirements (Jane Street Alignment)
Per "Why Testing Is Hard and How to Fix It" (Will Wilson):
1. Write Tests First: Unit tests for extracted helpers BEFORE implementation
2. Test Pure Functions: ValidateFleetTarget, ProcessFleetFill are pure
3. Test State Transitions: UpdateFleetState Actor message flow
4. Test Error Paths: All error handling branches covered

## Risk Assessment

### Low Risk Factors
- Single method extraction (minimal blast radius)
- No caller/callee changes (isolated refactoring)
- Existing test coverage (regression detection)
- Lock-free pattern already in place (no concurrency changes)

### Medium Risk Factors
- UI coupling (business logic in UI layer)
- Unknown test coverage (may need new tests)
- Potential hidden state dependencies

### Mitigation Strategy
1. Read Full Method: Use jCodemunch to understand all logic paths
2. Audit Locks: grep for lock(stateLock) in method body
3. Write Tests First: TDD for extracted helpers
4. Incremental Extraction: One helper at a time, verify after each
5. Checkpoint Frequently: Use Bob CLI restore points

## Effort Estimate

- Phase 1 (Scope): 30 minutes (CURRENT)
- Phase 2 (Planning): 1-2 hours (architectural design)
- Phase 3 (Audit): 30 minutes (Arena AI review)
- Phase 4 (Execution): 2-4 hours (TDD + extraction)
- Phase 5 (Verification): 1 hour (testing + review)
- Phase 6 (Sign-off): 30 minutes (deploy-sync + F5 test)

**Total**: 5-8 hours (1 day)

## Next Steps

1. Phase 1.0 Complete: Scope defined
2. Phase 1.5: Boundary validation (MANDATORY V12.23)
3. Phase 2: Read method with jCodemunch, create implementation plan
4. Phase 3: Arena AI audit of plan
5. Phase 4: TDD + surgical extraction
6. Phase 5: Verification loop
7. Phase 6: Deploy-sync + sign-off

---

**Scope Status**: DEFINED
**Boundary Status**: PENDING (Phase 1.5)
**Approval Status**: PENDING (awaiting Phase 1.5)
**Date**: 2026-06-15
**Analyst**: V12 Phase 1 Scope Protocol
