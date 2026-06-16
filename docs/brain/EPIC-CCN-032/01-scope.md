# Phase 1.0: Scope Definition - EPIC-CCN-032

## Extraction Scope (SINGLE METHOD ONLY)

### Target Method
- Method Name: RestoreCascadedTargets
- File: src/V12_002.Orders.Management.StopSync.cs
- Signature: private void RestoreCascadedTargets(Order order, StopLossState state)
- Current Complexity: 16 (CYC)
- Target Complexity: 8 or less (Jane Street strict standard)
- Violation: +1 over V12 threshold (CYC 15 or less)

### Extraction Strategy

Approach: Break into 2-3 focused helper methods

Based on CYC 16 analysis, the method likely contains:
1. Validation Logic (CYC reduction: 3-4)
   - Guard clauses
   - Null checks
   - State validation

2. State Transition Logic (CYC reduction: 2-3)
   - FSM/Actor state changes
   - StopLossState interactions

3. Target Restoration Logic (CYC reduction: 2-3)
   - Cascaded target updates
   - Order property modifications

Expected Outcome:
- Main method: CYC 8 or less (orchestration only)
- Helper methods: CYC 5 or less each (single-purpose)
- Total reduction: 16 to 8 (50% complexity reduction)

## Boundary Definition

### IN SCOPE
- RestoreCascadedTargets method body ONLY
- Extract validation logic to helper method(s)
- Extract state transition logic to helper method(s)
- Extract target restoration logic to helper method(s)
- Maintain exact same behavior (no logic changes)
- Preserve lock-free Actor/FSM pattern

### OUT OF SCOPE
- Callers: No changes to methods calling RestoreCascadedTargets
- Callees: No changes to methods called by RestoreCascadedTargets
- Other Methods: No changes to other methods in V12_002.Orders.Management.StopSync.cs
- Class Structure: No changes to class fields, properties, or constructor
- Pre-existing Issues: No fixing compilation errors outside this method
- Scope Creep: No "while we are here" improvements

### Scope Constraint: ONE EPIC = ONE CONCERN
This epic addresses ONLY the complexity violation in RestoreCascadedTargets.
All other concerns are deferred to separate epics.

## Success Criteria

### Functional Requirements
1. Complexity Reduced: CYC reduced from 16 to 8 or less
2. Behavior Preserved: All existing tests pass (zero regressions)
3. No Logic Changes: Extracted code is functionally identical
4. Lock-Free Pattern: FSM/Actor Enqueue model maintained

### Non-Functional Requirements
1. ASCII-Only: No Unicode, emoji, or curly quotes
2. Build Success: Zero compilation errors
3. Test Coverage: Existing tests cover extracted methods
4. Performance: No measurable latency increase

### Quality Gates
1. Pre-Push Validation: All 13 checks pass
2. CSharpier Format: Zero formatting issues
3. Codacy Review: Zero new issues introduced
4. Hard-Link Sync: deploy-sync.ps1 succeeds

## Refactoring Approach

### Phase 1: Extract Validation Logic
Target: Guard clauses, null checks, state validation
Expected CYC Reduction: 3-4
New Method: ValidateRestoreInputs(Order order, StopLossState state)

### Phase 2: Extract State Transition Logic
Target: FSM/Actor state changes
Expected CYC Reduction: 2-3
New Method: TransitionStopLossState(StopLossState state, ...)

### Phase 3: Extract Target Restoration Logic
Target: Cascaded target updates
Expected CYC Reduction: 2-3
New Method: ApplyCascadedTargets(Order order, ...)

## Risk Assessment

### Overall Risk: LOW
Justification:
1. Private Method: Limited blast radius (class-scoped only)
2. Single Concern: Focused extraction (no scope creep)
3. Testable: Existing tests provide regression safety net
4. Reversible: Checkpointing enabled for rollback

### Mitigation Strategy
1. TDD First: Write tests for extracted methods before extraction
2. Incremental: Extract one helper at a time, verify after each
3. Checkpoint: Save restore points before each extraction
4. Verify: Run full test suite after each extraction

## Jane Street Alignment

### Cognitive Simplicity Mandate
- Current: CYC 16 violates "make illegal states unrepresentable"
- Target: CYC 8 or less enables microsecond-latency reasoning
- Benefit: Simpler functions lead to easier race condition audits

### Testing Standards
- Current: 80-120 LOC method leads to exponential test path growth
- Target: 3-4 focused methods lead to linear test coverage
- Benefit: Exhaustive testing becomes tractable

### Lock-Free Verification
- Requirement: Zero lock(stateLock) blocks
- Pattern: FSM/Actor Enqueue model via StopLossState
- Verification: Forensic scan after extraction

## Metadata
- Epic ID: EPIC-CCN-032
- Phase: 1.0 (Scope Definition)
- Analyst: Bob CLI (v12-engineer)
- Date: 2026-06-15
- Status: APPROVED (pending Phase 1.5 boundary validation)
