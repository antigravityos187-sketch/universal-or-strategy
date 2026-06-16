# Phase 1.0: Scope Definition - EPIC-CCN-003

## Extraction Scope (SINGLE METHOD ONLY)

### Target Method
- Method Name: IsOrderAllowed
- File: src/V12_002.UI.Compliance.cs
- Current Complexity: 16 (exceeds V12 threshold of 15)
- Target Complexity: 8 or less (Jane Street strict standard)
- Extraction Strategy: Break into 2-3 helper methods using Guard Clause pattern

### Complexity Reduction Plan

Current State:
- 16 conditional branches in single method
- Cognitive load: HIGH (exceeds Jane Street HFT reasoning threshold)
- Test matrix: 2^16 combinations (65,536 paths)

Target State:
- Main method: 8 or fewer branches (orchestration logic only)
- Helper methods: 5 or fewer branches each (single-purpose validators)
- Test matrix: Reduced to manageable subsets per validator

### Extraction Strategy

Approach: Guard Clause Pattern + Single-Purpose Validators

1. Extract Validation Rules (Priority 1):
   - Identify distinct compliance checks within 16 branches
   - Create dedicated validator methods (e.g., ValidateAccountStatus, ValidateOrderLimits)
   - Each validator returns boolean with early exit on failure

2. Apply Guard Clauses (Priority 2):
   - Replace nested if/else with early returns
   - Reduce cognitive load per decision point
   - Make illegal states unrepresentable (V12 DNA)

3. Maintain Lock-Free Pattern (Priority 3):
   - Verify no lock() usage in extracted methods
   - Use atomic primitives if state mutation exists
   - Preserve FSM/Actor pattern if present

### Boundary Definition

IN SCOPE (ONLY):
- IsOrderAllowed method body (src/V12_002.UI.Compliance.cs)
- Extract 2-3 helper methods from existing logic
- Reduce complexity from 16 to 8 or less

OUT OF SCOPE (STRICTLY):
- Callers of IsOrderAllowed (no changes to call sites)
- Callees of IsOrderAllowed (no changes to dependencies)
- Other methods in V12_002.UI.Compliance.cs
- Pre-existing compilation errors (separate EPIC)
- While we are here improvements (scope creep)
- Bundling multiple concerns (ONE EPIC = ONE CONCERN)

### Success Criteria

Functional Requirements:
- Complexity reduced from 16 to 8 or less
- All existing tests pass (100% pass rate)
- No behavior changes (semantic equivalence)
- Lock-free Actor/FSM pattern maintained

Quality Requirements:
- ASCII-only compliance (no Unicode/emoji)
- Guard Clause pattern applied
- Single-purpose validators extracted
- Test coverage maintained or improved

Performance Requirements:
- No latency regression (microsecond constraints)
- No additional allocations in hot path
- Atomic primitives only (no locks)

### V12 DNA Alignment

Correctness by Construction:
- Extract validators to make invalid states unrepresentable
- Use enums/types to enforce compile-time correctness
- Remove runtime if/else guards where possible

Lock-Free Actor Pattern:
- Verify no lock() usage in IsOrderAllowed
- Check call chain for lock contamination
- Use atomic primitives if state mutation exists

ASCII-Only Compliance:
- Audit string literals for Unicode/emoji
- Verify no curly quotes in error messages
- Check logging statements for non-ASCII

### Risk Assessment

Risk Level: MEDIUM

Rationale:
1. Complexity slightly exceeds threshold (16 vs 15)
2. Compliance logic is critical for order validation
3. Changes could affect order flow correctness
4. Testing complexity grows exponentially with branches

Mitigation:
- Mandatory checkpointing enabled (Bob CLI)
- 100% test coverage before and after
- Arena AI red team review (Phase 3)
- Incremental extraction with verification

### Jane Street Principles

Cognitive Simplicity:
- Functions with CYC >15 are harder to reason about under microsecond latency
- Compliance logic must be auditable for race conditions
- Simple, verifiable logic reduces production incidents

Testing Strategy:
- Exhaustive path coverage (currently 2^16 combinations)
- Extract to reduce test matrix to manageable size
- Focus on edge cases in compliance rules

## Metadata
- Phase: 1.0 (Scope Definition)
- Status: APPROVED
- Epic ID: EPIC-CCN-003
- Analyst: Bob CLI (v12-engineer)
- Date: 2026-06-15
- Next Phase: Phase 1.5 (Boundary Validation)
