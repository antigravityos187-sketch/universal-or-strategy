# Phase 0: Hotspot Analysis - EPIC-CCN-007

## Target Method
- **Method**: ShadowPropagateStopMoves
- **File**: src/V12_002.SIMA.Shadow.cs
- **Cyclomatic Complexity**: 20
- **Epic ID**: EPIC-CCN-007

## Complexity Metrics

### Method Signature
Method: private void ShadowPropagateStopMoves

### Complexity Analysis
- **Cyclomatic Complexity**: 20 (EXCEEDS threshold of 15)
- **Risk Level**: HIGH
- **Refactoring Priority**: P1 (Critical)

### Complexity Breakdown
The method exceeds the Jane Street-aligned threshold (CYC <= 15) by 33%. This indicates:
- Multiple nested conditionals and branching logic
- Complex state management requiring extraction
- High cognitive load for maintenance and testing

## Blast Radius

### Direct Dependencies
- Called by: Shadow state management methods
- Calls: Stop loss propagation logic
- Modifies: Shadow order state

### Impact Assessment
- **Scope**: Shadow order subsystem
- **Risk**: MEDIUM-HIGH
- **Isolation**: Method is part of SIMA.Shadow module
- **Test Coverage**: Requires verification

## Call Hierarchy

### Callers (Upstream)
- Shadow order lifecycle methods
- State transition handlers

### Callees (Downstream)
- Stop loss calculation methods
- Order state mutation methods

## Risk Assessment

### Overall Risk: HIGH

**Rationale**:
1. **Complexity**: CYC=20 exceeds threshold by 33%
2. **Criticality**: Stop loss propagation is mission-critical
3. **Cognitive Load**: High branching complexity
4. **Test Surface**: Exponential path growth (2^20 theoretical paths)

### Refactoring Strategy
- **Approach**: Extract conditional branches into focused methods
- **Target**: Reduce to CYC <= 15 per method
- **Pattern**: Use FSM/Actor pattern for state transitions
- **Testing**: Add unit tests for extracted methods

## V12 DNA Alignment

### Current Violations
- Complexity exceeds Jane Street threshold (15)
- Potential lock-free pattern violations (requires audit)
- ASCII-only compliance (requires verification)

### Post-Refactoring Goals
- CYC <= 15 for all extracted methods
- Lock-free Actor pattern for state mutations
- Make illegal states unrepresentable via type design
- 100% test coverage for extracted logic

## Next Steps (Phase 1)

1. **Vision/Spec**: Define extraction boundaries
2. **Arch Planning**: Design method decomposition
3. **DNA Audit**: Verify lock-free compliance
4. **Implementation**: Extract and test
5. **Verification**: Confirm CYC <= 15 for all methods

## Metadata
- **Analysis Date**: 2026-06-15
- **Analyzer**: V12 Phase 0 Hotspot Analyzer
- **Protocol Version**: V12.23
- **Status**: Phase 0 Complete
