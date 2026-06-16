# Phase 1.0: Scope Definition - EPIC-CCN-002

## Extraction Scope (SINGLE METHOD ONLY)

**Target Method**:
- **Method Name**: SymmetryGuardTryResolveFollowersForDispatch
- **File**: src/V12_002.Symmetry.Replace.cs
- **Current Complexity**: 18 (Cyclomatic Complexity)
- **Target Complexity**: <=8 (Jane Street strict standard)
- **Extraction Strategy**: Break into 2-3 helper methods

## Boundary Definition

### IN SCOPE
- ONLY the method body of SymmetryGuardTryResolveFollowersForDispatch
- Extract conditional logic into smaller, single-purpose helper methods
- Reduce cyclomatic complexity from 18 to <=8
- Maintain existing method signature and contract
- Preserve atomic semantics and lock-free Actor/FSM pattern

### OUT OF SCOPE
- Callers: No changes to methods that invoke SymmetryGuardTryResolveFollowersForDispatch
- Callees: No changes to methods called by SymmetryGuardTryResolveFollowersForDispatch
- Other Methods: No changes to other methods in V12_002.Symmetry.Replace.cs
- File Structure: No changes to class structure, namespaces, or imports
- Behavior Changes: No functional changes to order dispatch logic

### NO SCOPE CREEP
- ONE EPIC = ONE CONCERN: This epic addresses ONLY the complexity of SymmetryGuardTryResolveFollowersForDispatch
- No "while we are here" improvements to adjacent code
- No fixing pre-existing compilation errors in other methods
- No bundling multiple refactoring concerns

## Success Criteria

### Primary Goals
1. Complexity Reduction: Reduce cyclomatic complexity from 18 to <=8
2. Test Pass: All existing tests pass without modification
3. Behavior Preservation: No functional changes to order dispatch logic
4. Lock-Free Pattern: Maintain Actor/FSM pattern, no lock() blocks introduced

### V12 DNA Compliance
1. ASCII-Only: No Unicode, emoji, or curly quotes in string literals
2. Atomic Operations: State mutations use FSM/Actor Enqueue model or atomic primitives
3. Correctness by Construction: Extracted methods make illegal states unrepresentable
4. Jane Street Alignment: Each extracted method has CYC <=10

### Quality Gates
1. Build Success: dotnet build completes with zero errors
2. Lint Clean: powershell -File .\scripts\lint.ps1 passes
3. Format Check: dotnet csharpier check src/ passes
4. Complexity Audit: python scripts/complexity_audit.py shows CYC <=8 for target method

## Extraction Strategy

### Recommended Decomposition
Based on hotspot analysis, extract into 2-3 helper methods:

1. ValidateFollowerEligibility (CYC <=5)
   - Extract follower validation logic
   - Guard conditions for follower resolution
   - Return: bool indicating eligibility

2. ResolveFollowerActions (CYC <=5)
   - Extract follower list construction logic
   - State transition coordination
   - Return: List<OrderAction> of resolved followers

3. CoordinateDispatch (CYC <=5)
   - Extract dispatch coordination logic
   - Final validation before dispatch
   - Return: bool indicating dispatch readiness

### Main Method (CYC <=8)
After extraction, SymmetryGuardTryResolveFollowersForDispatch orchestrates:
- Call ValidateFollowerEligibility
- Call ResolveFollowerActions
- Call CoordinateDispatch
- Return final result

## Risk Assessment

**RISK LEVEL**: MEDIUM-HIGH

**Rationale**:
1. Complexity: CYC 18 exceeds Jane Street threshold (15)
2. Cognitive Load: Multiple conditional branches make reasoning difficult
3. Testing: Exponential path growth (2^18 = 262,144 theoretical paths)
4. Lock-Free Audit: Complex branching increases race condition surface area

**Mitigation**:
- Single-method extraction minimizes blast radius
- Preserve existing method signature (no caller changes)
- Comprehensive test coverage before and after extraction
- Arena AI adversarial audit (Phase 3)

## Next Steps (Phase 1.5)

1. Create 01-scope-boundary.md for boundary validation (V12.23 Protocol)
2. Proceed to Phase 2: Architecture Planning
3. Generate implementation_plan.md with Mermaid diagrams
4. Submit to Arena AI for DNA audit (Phase 3)

---

**Document Status**: DRAFT
**Phase**: 1.0 (Scope Definition)
**Approval**: Pending Phase 1.5 Boundary Validation
**Date**: 2026-06-15
