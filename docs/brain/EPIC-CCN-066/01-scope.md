# Phase 1.0: Scope Definition - EPIC-CCN-066

## Extraction Scope (SINGLE METHOD ONLY)

**Target Method**:
- **Method Name**: RemoveFsmOrderIdMappings
- **File**: src/V12_002.Symmetry.BracketFSM.cs
- **Current Complexity**: 11 (CYC)
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **Extraction Strategy**: Break into 2-3 helper methods

## Boundary Definition

### IN SCOPE
- RemoveFsmOrderIdMappings method body ONLY
- Extract conditional logic into helper methods
- Extract collection manipulation into helper methods
- Maintain lock-free Actor/FSM pattern

### OUT OF SCOPE
- Callers of RemoveFsmOrderIdMappings
- Callees invoked by RemoveFsmOrderIdMappings
- Other methods in V12_002.Symmetry.BracketFSM.cs
- Pre-existing compilation errors
- While we are here improvements
- Refactoring adjacent code

### No Scope Creep
- ONE EPIC = ONE CONCERN: Single-method complexity reduction
- No bundling: This EPIC does NOT fix other issues
- No opportunistic refactoring: Touch only RemoveFsmOrderIdMappings

## Success Criteria

### Functional Requirements
1. Complexity reduced from 11 to ≤8
2. All existing tests pass (zero regressions)
3. No behavior changes (pure refactoring)
4. Lock-free Actor/FSM pattern maintained

### Quality Gates
1. CSharpier formatting passes
2. Build succeeds (zero compilation errors)
3. Lint passes (zero new violations)
4. Pre-push validation passes

### V12 DNA Compliance
1. ASCII-only strings (no Unicode)
2. No lock() statements introduced
3. Atomic state transitions preserved
4. Make illegal states unrepresentable principle maintained

## Extraction Strategy

### Proposed Decomposition
Based on CYC=11, likely candidates for extraction:
1. Helper Method 1: Conditional validation logic (reduce branching)
2. Helper Method 2: Collection filtering/removal logic
3. Helper Method 3: State cleanup operations (if applicable)

### Verification Plan
1. Run complexity audit before extraction
2. Extract helper methods one at a time
3. Run tests after each extraction
4. Verify final CYC ≤8 with complexity audit

## Risk Assessment

### Low Risk Factors
- Method already below threshold (11 < 15)
- Single-method scope (minimal blast radius)
- Pure refactoring (no logic changes)

### Mitigation Strategy
- Checkpoint before each extraction
- Run tests after each helper method creation
- Use Bob CLI restore if regression detected

## Phase 1.0 Status
- **Status**: COMPLETED
- **Date**: 2026-06-15
- **Analyst**: Bob Shell (v12-engineer mode)
- **Next Phase**: 1.5 (Boundary Validation)
