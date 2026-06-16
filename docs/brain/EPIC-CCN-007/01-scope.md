# Phase 1.0: Scope Definition - EPIC-CCN-007

## Epic Metadata
- **Epic ID**: EPIC-CCN-007
- **Target Method**: ShadowPropagateStopMoves
- **File**: src/V12_002.SIMA.Shadow.cs
- **Current Complexity**: 20 (CYC)
- **Target Complexity**: <=8 (Jane Street strict standard)
- **Phase**: 1.0 - Scope Definition
- **Date**: 2026-06-15

## 1. Extraction Scope (SINGLE METHOD ONLY)

### Target Method
- **Method Name**: ShadowPropagateStopMoves
- **Signature**: private void ShadowPropagateStopMoves
- **Current Complexity**: 20 (exceeds threshold by 33%)
- **Target Complexity**: <=8 per extracted method

### Extraction Strategy
**Approach**: Break into 2-3 focused helper methods

**Rationale**:
- CYC=20 indicates multiple nested conditionals and branching logic
- Jane Street principle: Keep functions cognitively simple (CYC <=15, target <=8)
- Stop loss propagation is mission-critical - requires clear, testable logic
- Exponential path growth (2^20 theoretical paths) makes testing infeasible

**Expected Decomposition**:
1. **Validation/Guard Method** (CYC <=5): Pre-condition checks, early returns
2. **Core Logic Method** (CYC <=8): Main stop loss propagation algorithm
3. **State Update Method** (CYC <=5): Shadow order state mutations

### Success Criteria
- All extracted methods have CYC <=8
- Original method becomes orchestrator (CYC <=5)
- All unit tests pass (100% coverage)
- No behavior changes (bit-identical output)
- Lock-free Actor/FSM pattern maintained
- ASCII-only compliance verified

## 2. Boundary Definition

### IN SCOPE (What We WILL Change)
- ShadowPropagateStopMoves method body ONLY
- Extract conditional branches into helper methods
- Add unit tests for extracted methods
- Update complexity metrics

### OUT OF SCOPE (What We WILL NOT Touch)
- Callers of ShadowPropagateStopMoves
- Callees (methods called by ShadowPropagateStopMoves)
- Other methods in V12_002.SIMA.Shadow.cs
- Pre-existing compilation errors
- Unrelated code improvements
- Formatting/style changes outside target method

### No Scope Creep Rule
**ONE EPIC = ONE CONCERN**

This epic addresses ONLY the complexity of ShadowPropagateStopMoves. Any other issues discovered during analysis must be logged as separate epics.

## 3. Risk Assessment

### Complexity Risk: HIGH
- **Current**: CYC=20 (33% over threshold)
- **Cognitive Load**: High branching complexity
- **Test Surface**: Exponential path growth
- **Mitigation**: Extract into testable units

### Behavioral Risk: MEDIUM
- **Criticality**: Stop loss propagation is mission-critical
- **Mitigation**: Bit-identical behavior verification
- **Testing**: 100% coverage for extracted methods

### Integration Risk: LOW
- **Isolation**: Single method extraction
- **Blast Radius**: Contained to Shadow subsystem
- **Mitigation**: No changes to callers/callees

## 4. V12 DNA Alignment

### Architectural Mandates
- **Correctness by Construction**: Extract methods to make illegal states unrepresentable
- **Lock-Free Actor Pattern**: Verify no lock() blocks introduced
- **ASCII-Only Compliance**: Verify no Unicode in string literals
- **Jane Street Alignment**: Target CYC <=8 (strict standard)

### Testing Requirements
- Unit tests for each extracted method
- Integration test for orchestrator method
- Verify FSM/Actor Enqueue pattern maintained
- Confirm atomic state transitions

## 5. Implementation Constraints

### Method Extraction Rules
1. Each extracted method must have single responsibility
2. No shared mutable state between extracted methods
3. Use readonly structs for parameter passing where possible
4. Maintain lock-free semantics (no lock() blocks)

### Naming Convention
- Prefix extracted methods with ShadowPropagate_ for clarity
- Use descriptive names: ShadowPropagate_ValidateConditions, ShadowPropagate_UpdateState

### Code Style
- Follow existing V12 patterns
- Use CSharpier formatting
- Add XML documentation comments
- Include complexity annotations

## 6. Verification Plan

### Pre-Implementation
- [ ] Read current method implementation
- [ ] Identify conditional branches (target for extraction)
- [ ] Map dependencies (callers/callees)
- [ ] Verify lock-free compliance

### Post-Implementation
- [ ] Run dotnet build (zero errors)
- [ ] Run dotnet test (100% pass)
- [ ] Run python scripts/complexity_audit.py (CYC <=8 for all methods)
- [ ] Run dotnet csharpier check src/ (zero issues)
- [ ] Verify no lock() blocks: grep -r "lock(" src/V12_002.SIMA.Shadow.cs

## 7. Next Steps

**Phase 1.5**: Boundary Validation (MANDATORY per V12.23 Protocol)
- Create 01-scope-boundary.md
- Validate no scope creep
- Get approval before Phase 2

**Phase 2**: Architectural Planning
- Design method decomposition
- Create Mermaid diagrams
- Define extracted method signatures

## Metadata
- **Created**: 2026-06-15
- **Protocol Version**: V12.23
- **Status**: Phase 1.0 Complete
- **Next Phase**: 1.5 - Boundary Validation
