# Phase 0: Hotspot Analysis - EPIC-006

## Epic Overview
- **Epic ID**: EPIC-006
- **Target File**: src/V12_002.SIMA.Lifecycle.cs
- **Target Methods**: AdoptFleetWorkingOrders, ClassifyAndRouteFleetOrder
- **Analysis Date**: 2026-06-14

## Target Methods

### Method 1: AdoptFleetWorkingOrders
- **File**: src/V12_002.SIMA.Lifecycle.cs
- **Cyclomatic Complexity**: 17 (HIGH - Target: ≤8)
- **Status**: Requires extraction

### Method 2: ClassifyAndRouteFleetOrder
- **File**: src/V12_002.SIMA.Lifecycle.cs
- **Cyclomatic Complexity**: 16 (HIGH - Target: ≤8)
- **Status**: Requires extraction

## Complexity Distribution
The file contains multiple high-complexity methods:
- Complexity 17: 1 method
- Complexity 16: 1 method
- Complexity 12: 2 methods
- Complexity 11: 1 method
- Complexity 10: 1 method
- Complexity 9: 3 methods

**Total methods exceeding threshold (>8)**: 9 methods

## Complexity Metrics

### AdoptFleetWorkingOrders
- **Cyclomatic Complexity**: 17
- **Cognitive Load**: HIGH
- **Branching Factor**: Multiple conditional paths
- **Refactoring Priority**: P1 (Critical)

### ClassifyAndRouteFleetOrder
- **Cyclomatic Complexity**: 16
- **Cognitive Load**: HIGH
- **Branching Factor**: Multiple conditional paths
- **Refactoring Priority**: P1 (Critical)

## Blast Radius Analysis

### Impact Assessment
- **File**: src/V12_002.SIMA.Lifecycle.cs
- **Module**: SIMA Lifecycle Management
- **Subsystem**: Fleet Order Processing

### Dependencies
Both methods are part of the SIMA lifecycle:
- Fleet order adoption logic
- Order classification and routing
- State machine transitions
- Lock-free actor pattern integration

### Risk Factors
1. **High Complexity**: Both methods exceed CYC threshold by 2x
2. **Critical Path**: Fleet order processing is core functionality
3. **State Management**: Complex state transitions require careful extraction
4. **Lock-Free Constraints**: Must maintain atomic operations during refactoring

## Call Hierarchy

### AdoptFleetWorkingOrders
- **Called By**: SIMA lifecycle orchestrator
- **Calls**: Order validation, state transition helpers
- **Integration Points**: Fleet management subsystem

### ClassifyAndRouteFleetOrder
- **Called By**: Order processing pipeline
- **Calls**: Classification logic, routing decisions
- **Integration Points**: Order routing subsystem

## Hotspot Classification

### Hotspot Severity: HIGH

**Rationale**:
1. **Complexity**: Both methods significantly exceed CYC ≤8 threshold
2. **Criticality**: Core fleet order processing logic
3. **Maintainability**: High cognitive load impedes debugging
4. **Testing**: Exponential path growth (2^17 and 2^16 paths)
5. **V12 DNA Violation**: Exceeds Jane Street cognitive simplicity standard

### Jane Street Alignment Issues
- Functions with CYC >15 are harder to reason about under microsecond latency
- Exponential test path growth (131,072 and 65,536 paths respectively)
- Difficult to audit for race conditions in lock-free code
- Violates "Make illegal states unrepresentable" principle

## Risk Assessment

### Overall Risk: HIGH

**Risk Breakdown**:
- **Complexity Risk**: HIGH (CYC 17, 16 vs target 8)
- **Blast Radius**: MEDIUM (isolated to SIMA lifecycle)
- **Testing Risk**: HIGH (insufficient test coverage for complexity)
- **Refactoring Risk**: MEDIUM (well-defined boundaries)

### Mitigation Strategy
1. Extract decision logic into separate methods (CYC ≤8 each)
2. Use FSM/Actor pattern for state transitions
3. Add comprehensive unit tests for extracted methods
4. Maintain lock-free guarantees throughout extraction

## Recommended Extraction Strategy

### Phase 1: AdoptFleetWorkingOrders
1. Extract order validation logic
2. Extract state transition logic
3. Extract error handling paths
4. Target: 3-4 methods, each CYC ≤8

### Phase 2: ClassifyAndRouteFleetOrder
1. Extract classification logic
2. Extract routing decision logic
3. Extract edge case handling
4. Target: 3-4 methods, each CYC ≤8

## Success Criteria
- All extracted methods have CYC ≤8
- Lock-free guarantees maintained
- 100% test coverage for extracted logic
- No regression in fleet order processing
- ASCII-only compliance maintained

## Next Steps
1. Proceed to Phase 1 (Vision/Spec) for detailed extraction plan
2. Generate implementation plan with Mermaid diagrams
3. Execute Phase 4 (Recursive Execution) with Bob CLI
4. Verify with Phase 5 (Verification/Review)

---
**Analysis Status**: COMPLETE
**Recommendation**: PROCEED to Phase 1
**Priority**: P1 (Critical - High complexity hotspot)
