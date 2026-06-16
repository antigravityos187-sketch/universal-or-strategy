# Phase 1.0: Scope Definition - EPIC-CCN-070

## Extraction Scope (SINGLE METHOD ONLY)

### Target Method
- Method Name: HydrateFSMsFromWorkingOrders
- File: src/V12_002.SIMA.Lifecycle.cs
- Current Complexity: 9
- Target Complexity: 8 or less (Jane Street strict standard)
- Extraction Strategy: Break into 2-3 helper methods

### Complexity Reduction Plan
Current State: CYC = 9
Target State: CYC 8 or less
Approach: Extract logical sub-operations into focused helper methods

Potential Extractions:
1. Order validation logic
2. FSM initialization logic
3. State restoration logic

## Boundary Definition

### IN SCOPE
- ONLY the method body of HydrateFSMsFromWorkingOrders
- Internal logic refactoring
- Helper method extraction within same class
- Complexity reduction from 9 to 8 or less

### OUT OF SCOPE
- Callers of HydrateFSMsFromWorkingOrders (NO CHANGES)
- Callees invoked by HydrateFSMsFromWorkingOrders (NO CHANGES)
- Other methods in V12_002.SIMA.Lifecycle.cs (NO CHANGES)
- Method signature changes (NO CHANGES)
- Behavior modifications (NO CHANGES)
- Pre-existing compilation errors (NO FIXES)

### No Scope Creep Rule
ONE EPIC = ONE CONCERN
- This EPIC addresses ONLY the complexity of HydrateFSMsFromWorkingOrders
- No bundling of multiple refactoring concerns
- No fixing unrelated issues discovered during extraction

## Success Criteria

### Functional Requirements
1. Complexity reduced from 9 to 8 or less
2. All existing tests pass (100 percent pass rate)
3. No behavior changes (bit-for-bit identical output)
4. Lock-free Actor/FSM pattern maintained (V12 DNA)
5. ASCII-only compliance verified (no Unicode/emoji)

### Quality Gates
1. Build succeeds with zero errors
2. CSharpier formatting passes
3. Roslyn analyzer shows zero violations
4. Complexity audit confirms CYC 8 or less
5. Pre-push validation passes (all 13 checks)

### V12 DNA Compliance
1. No lock() statements introduced
2. Atomic operations or FSM/Actor Enqueue pattern only
3. Make illegal states unrepresentable principle maintained
4. Correctness by construction approach preserved

## Risk Assessment

### Complexity Risk
- Current: LOW (CYC=9, below threshold of 15)
- Refactoring Risk: MINIMAL (single method, no callers affected)
- Regression Risk: LOW (behavior-preserving transformation)

### Mitigation Strategy
1. Extract one helper method at a time
2. Run tests after each extraction
3. Use checkpointing for rollback safety
4. Verify complexity reduction incrementally

## Jane Street Alignment

### Cognitive Simplicity
- Functions with CYC greater than 8 are harder to reason about under microsecond latency
- Single-responsibility helpers improve testability
- Smaller functions reduce exponential path growth in testing

### HFT Principles
- Keep hot-path logic simple and verifiable
- Avoid clever abstractions that obscure control flow
- Prioritize readability for race condition auditing

## Implementation Constraints

### Hard Constraints (MUST)
1. Method signature MUST remain unchanged
2. Public API MUST remain unchanged
3. Behavior MUST be identical (no side effects)
4. Tests MUST pass without modification

### Soft Constraints (SHOULD)
1. Helper methods SHOULD be private
2. Extracted logic SHOULD have single responsibility
3. Method names SHOULD be descriptive and verb-based
4. Comments SHOULD explain why, not what

## Verification Plan

### Pre-Implementation
1. Read current method implementation
2. Identify logical sub-operations
3. Plan extraction boundaries
4. Review with Director (if needed)

### During Implementation
1. Extract one helper at a time
2. Run dotnet build after each extraction
3. Run dotnet test after each extraction
4. Verify complexity with python3 scripts/complexity_audit.py

### Post-Implementation
1. Run full pre-push validation suite
2. Verify CYC 8 or less in complexity audit
3. Confirm zero behavior changes
4. Update manifest.json with completion status

---
Scope Status: DEFINED
Approval Required: YES (Director sign-off before Phase 2)
Next Phase: Phase 1.5 (Boundary Validation)
