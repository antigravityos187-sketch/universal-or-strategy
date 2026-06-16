# Phase 1.0: Scope Definition - EPIC-CCN-080

## Extraction Scope (SINGLE METHOD ONLY)

### Target Method
- Method Name: PlacePanel
- File: src/V12_002.UI.Panel.Construction.cs
- Current Complexity: 13 (cyclomatic)
- Target Complexity: 8 or less (Jane Street strict standard)
- Extraction Strategy: Break into 2-3 helper methods

### Complexity Reduction Plan
Current State: 13 cyclomatic complexity
Target State: 8 or less cyclomatic complexity
Reduction Required: Minimum 5 complexity points

Extraction Strategy:
1. Helper Method 1: Extract conditional logic chains
2. Helper Method 2: Extract panel positioning calculations
3. Helper Method 3 (if needed): Extract validation logic

### Boundary Definition

IN SCOPE:
- PlacePanel method body only
- Internal logic extraction into private helper methods
- Complexity reduction from 13 to 8 or less
- Maintaining existing method signature
- Preserving all existing behavior

OUT OF SCOPE:
- Callers of PlacePanel (no changes)
- Callees of PlacePanel (no changes)
- Other methods in V12_002.UI.Panel.Construction.cs
- Related files or classes
- Performance optimizations beyond complexity reduction
- Feature additions or behavior changes

### No Scope Creep Mandate
ONE EPIC = ONE CONCERN
- This epic addresses ONLY PlacePanel complexity
- No "while we are here" improvements
- No fixing pre-existing compilation errors in other methods
- No bundling multiple concerns

## Success Criteria

### Primary Goals
1. Complexity Reduced: PlacePanel cyclomatic complexity 8 or less
2. All Tests Pass: Zero test failures
3. No Behavior Changes: Identical runtime behavior
4. Lock-Free Pattern Maintained: Actor/FSM pattern compliance

### Quality Gates
1. Build: Zero compilation errors
2. Tests: 100% pass rate
3. Lint: Zero new violations
4. Complexity Audit: complexity_audit.py confirms 8 or less
5. ASCII-Only: No Unicode violations

### V12 DNA Compliance
- Correctness by Construction maintained
- Lock-free Actor pattern preserved
- ASCII-only compliance
- Jane Street alignment (cognitive simplicity)

## Risk Assessment

### Low Risk Factors
- Single method scope (isolated change)
- No API surface changes
- No caller/callee modifications
- Complexity 13 is below critical threshold (15)

### Mitigation Strategy
- Extract one helper method at a time
- Run tests after each extraction
- Use checkpointing for rollback safety
- Verify complexity reduction incrementally

## Implementation Approach

### Phase Sequence
1. Phase 1.5: Boundary validation (MANDATORY per V12.23)
2. Phase 2: Architectural planning (helper method design)
3. Phase 3: DNA audit (V12 compliance verification)
4. Phase 4: Surgical extraction (one helper at a time)
5. Phase 5: Verification (tests + complexity audit)
6. Phase 6: Sign-off (deploy-sync.ps1)

### Extraction Order
1. Identify highest complexity sub-logic
2. Extract to private helper method
3. Verify tests pass
4. Check complexity reduction
5. Repeat until target 8 or less achieved

## Jane Street Alignment

### Cognitive Simplicity Principles
- Functions with CYC >15 are hard to reason about under microsecond latency
- Target 8 or less provides safety margin below threshold
- Smaller functions = easier to test exhaustively
- Simpler logic = easier to audit for race conditions

### V12 DNA Mandate
"Make illegal states unrepresentable" - requires simple, verifiable logic
- Complex functions hide edge cases
- Simple functions expose invariants
- Extraction improves testability and auditability

## Approval Status
- Status: PENDING Phase 1.5 boundary validation
- Next Step: Create 01-scope-boundary.md
