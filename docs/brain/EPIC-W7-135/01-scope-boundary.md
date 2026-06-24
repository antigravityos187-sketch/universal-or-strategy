# Phase 1: Scope Definition - EPIC-W7-135

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.00 (plan mode)
- **Execution Time**: ~5 seconds

## Epic Objective
Reduce cyclomatic complexity of FindTargetOrderForPosition from 10 to ≤8 by extracting conditional logic into helper methods.

## IN SCOPE

### Primary Target
- **Method**: FindTargetOrderForPosition
- **File**: src/V12_002.Trailing.Breakeven.cs
- **Line**: 186
- **Current CYC**: 10
- **Target CYC**: ≤8

### Refactoring Actions
1. **Extract conditional branches** to reduce complexity
2. **Preserve method signature** (4 parameters including out parameter)
3. **Maintain single caller relationship** with MoveSpecificTarget
4. **Add unit tests** for extracted logic (xUnit framework)

### Success Criteria
- CYC reduced from 10 to ≤8
- All extracted methods have CYC ≤8
- Method behavior unchanged (verified by tests)
- Build passes after refactoring
- deploy-sync.ps1 executed successfully

## OUT OF SCOPE

### Explicitly Excluded
1. **Caller method** (MoveSpecificTarget) - separate epic if needed
2. **Other methods** in V12_002.Trailing.Breakeven.cs - not part of this epic
3. **Method signature changes** - preserve existing interface
4. **Performance optimization** - focus on complexity reduction only
5. **Architectural changes** - no FSM/Actor pattern conversion needed

### Boundary Conditions
- **No changes** to method parameters or return type
- **No changes** to caller (MoveSpecificTarget) unless absolutely necessary
- **No changes** to other files in the codebase
- **No changes** to test framework (xUnit only, per V12.32 mandate)

## Risk Mitigation

### Low Risk Factors
- Zero external dependencies (blast radius = 0.0)
- Single caller (isolated impact)
- No downstream calls (no cascading changes)
- Private method (no public API impact)

### Safeguards
1. **Unit tests** before refactoring (TDD approach)
2. **Incremental extraction** (one helper method at a time)
3. **Build verification** after each extraction
4. **Deploy-sync** after all changes

## Complexity Reduction Strategy

### Current Structure (CYC 10)
The method likely contains:
- Multiple conditional branches (if/else)
- Nested conditions (max nesting depth: 3)
- Early returns or guard clauses

### Target Structure (CYC ≤8)
Extract to helper methods:
1. **Validation logic** (parameter checks, null guards)
2. **Search logic** (order matching conditions)
3. **Error handling** (notFoundReason assignment)

### Extraction Pattern
BEFORE (CYC 10): FindTargetOrderForPosition contains 37 lines with nested conditions
AFTER (CYC ≤8): Simplified logic calling helpers, each helper has CYC ≤8

Helper methods to extract:
- ValidateSearchParameters
- SearchForMatchingOrder
- SetNotFoundReason

## Phase 1 Completion
- Scope clearly defined (IN vs OUT)
- Boundary conditions established
- Risk mitigation planned
- Extraction strategy outlined
- Ready for Phase 2 (Architecture Planning)
