# Phase 0: Hotspot Analysis - EPIC-CCN-112

## Target Method
- **Method**: ClassifyOrderByPrefix
- **File**: src/V12_002.SIMA.Lifecycle.cs
- **Cyclomatic Complexity**: 17
- **Status**: Exceeds V12 threshold (CYC ≤ 15)

## Complexity Metrics

### Cyclomatic Complexity Analysis
- **Current Complexity**: 17
- **V12 Threshold**: 15 (Jane Street alignment)
- **Overage**: +2 (13% over threshold)
- **Risk Level**: MEDIUM

### Method Characteristics
- **Purpose**: Order classification by prefix matching
- **Pattern**: Conditional branching logic
- **Complexity Driver**: Multiple prefix comparison paths

## Blast Radius Assessment

### Direct Impact
- **File**: src/V12_002.SIMA.Lifecycle.cs
- **Module**: SIMA Lifecycle Management
- **Subsystem**: Order Processing

### Potential Callers
- Order entry handlers
- Bracket order processors
- SIMA state machine transitions

### Risk Factors
1. **Cognitive Load**: 17 branches require careful mental model
2. **Test Coverage**: Exponential path growth (2^17 = 131,072 theoretical paths)
3. **Race Conditions**: Prefix matching in lock-free context requires atomic guarantees
4. **Maintainability**: Future prefix additions increase complexity linearly

## Call Hierarchy

### Upstream Dependencies
- Called by order processing pipeline
- Invoked during SIMA lifecycle state transitions
- Used in bracket order classification

### Downstream Effects
- Determines order routing logic
- Affects state machine transitions
- Impacts order execution flow

## Refactoring Strategy

### Recommended Approach
1. **Extract Prefix Mapping**: Move prefix-to-classification logic to lookup table
2. **Strategy Pattern**: Replace conditional branches with polymorphic dispatch
3. **Atomic Operations**: Ensure lock-free correctness during refactoring

### Expected Outcome
- **Target Complexity**: ≤ 10 (Jane Street best practice)
- **Maintainability**: O(1) lookup vs O(n) branching
- **Testability**: Reduced from 131k paths to ~10 test cases

## Risk Assessment

**Overall Risk**: MEDIUM

### Justification
- Complexity overage is moderate (+2)
- Method is in critical path (SIMA lifecycle)
- Refactoring requires careful state machine validation
- Lock-free correctness must be preserved

### Mitigation
- Comprehensive unit tests before extraction
- Atomic state transition validation
- Blast radius containment via interface boundaries

## V12 DNA Alignment

### Current Violations
- ❌ Complexity exceeds threshold (17 > 15)
- ⚠️ Cognitive simplicity compromised

### Post-Refactoring Goals
- ✅ Complexity ≤ 10
- ✅ "Make illegal states unrepresentable" via type system
- ✅ Lock-free correctness preserved
- ✅ Testability improved (exponential → linear paths)

## Next Steps (Phase 1)

1. **Vision/Spec**: Define prefix classification domain model
2. **Architecture**: Design lookup table or strategy pattern
3. **Implementation**: Extract with TDD (test-first)
4. **Verification**: Validate state machine correctness

---

**Analysis Date**: 2026-06-13
**Analyst**: V12 Phase 0 Hotspot Analyzer
**Epic**: EPIC-CCN-112
**Phase**: 0 (Hotspot Analysis)
