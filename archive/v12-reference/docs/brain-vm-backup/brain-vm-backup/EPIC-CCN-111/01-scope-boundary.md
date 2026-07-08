# Phase 1.5: Scope Boundary Validation - EPIC-CCN-111

## Epic Context
- **Epic ID**: EPIC-CCN-111
- **Target Method**: `HydrateExpectedPositionsFromBroker`
- **File**: `src/V12_002.SIMA.Lifecycle.cs`
- **Current CCN**: 17
- **Target CCN**: ≤15 (Jane Street alignment)
- **Reduction Required**: Minimum 2 points

## Scope Boundary Definition

### SINGLE METHOD EXTRACTION SCOPE
**V12.23 No Scope Creep Protocol: This refactoring targets ONLY `HydrateExpectedPositionsFromBroker`**

### What to Extract (IN SCOPE)

#### 1. Position Validation Logic
- **Extract to**: `ValidateBrokerPosition(BrokerPosition position)`
- **Purpose**: Isolate broker position validation rules
- **Complexity Reduction**: ~3-4 CCN points
- **Rationale**: Validation logic contains multiple conditional branches that can be extracted
- **Contract**: Returns `Result<ValidatedPosition, ValidationError>`

#### 2. State Update Logic
- **Extract to**: `UpdatePositionState(ValidatedPosition position)`
- **Purpose**: Atomic position state mutation
- **Complexity Reduction**: ~2-3 CCN points
- **Rationale**: State updates should be isolated for testability and lock-free verification
- **Contract**: Uses FSM/Actor Enqueue pattern for state machine updates

#### 3. Error Path Consolidation
- **Extract to**: `HandlePositionHydrationError(Exception ex, string context)`
- **Purpose**: Centralize error handling and logging
- **Complexity Reduction**: ~1-2 CCN points
- **Rationale**: Error paths add branching complexity; consolidation improves maintainability
- **Contract**: Returns standardized error response

### What to Keep (OUT OF SCOPE)

#### Orchestration Logic (KEEP in HydrateExpectedPositionsFromBroker)
- High-level flow coordination
- Broker API call invocation
- Result aggregation and return
- Method signature and public interface

#### Dependencies (NO CHANGES)
- Broker API integration points
- SIMA state machine interface
- Logging infrastructure
- Existing test infrastructure

### Boundary Enforcement

**STRICT RULES:**
1. **No cascading refactors**: Do not modify callers or callees of target method
2. **No infrastructure changes**: Do not alter logging, state machine, or broker APIs
3. **No test rewrites**: Add new tests only; do not modify existing test suite
4. **No signature changes**: Maintain exact method signature for backward compatibility
5. **Single file scope**: All changes confined to `src/V12_002.SIMA.Lifecycle.cs`

## Extraction Strategy

### Phase 1: Extract Validation Logic
```csharp
// NEW METHOD
private Result<ValidatedPosition, ValidationError> ValidateBrokerPosition(BrokerPosition position)
{
    // Extract all validation conditionals here
    // Target CCN: ≤10
}
```

### Phase 2: Extract State Update Logic
```csharp
// NEW METHOD
private void UpdatePositionState(ValidatedPosition position)
{
    // Extract state mutation logic here
    // Use Actor.Enqueue for state machine updates
    // Target CCN: ≤8
}
```

### Phase 3: Extract Error Handling
```csharp
// NEW METHOD
private void HandlePositionHydrationError(Exception ex, string context)
{
    // Consolidate error logging and response
    // Target CCN: ≤5
}
```

### Phase 4: Refactor Original Method
```csharp
// REFACTORED METHOD
private void HydrateExpectedPositionsFromBroker()
{
    // Orchestration only:
    // 1. Call broker API
    // 2. Call ValidateBrokerPosition()
    // 3. Call UpdatePositionState()
    // 4. Handle errors via HandlePositionHydrationError()
    // Target CCN: ≤12
}
```

## Success Criteria

### Quantitative Metrics
- ✅ **Primary Goal**: `HydrateExpectedPositionsFromBroker` CCN ≤15
- ✅ **Stretch Goal**: `HydrateExpectedPositionsFromBroker` CCN ≤12
- ✅ **Extracted Methods**: Each new method CCN ≤10
- ✅ **Total CCN Reduction**: Minimum 2 points from original 17

### Qualitative Criteria
- ✅ **Lock-Free Verification**: No `lock()` statements in any method
- ✅ **Type Safety**: Use `Result<T, E>` pattern for validation
- ✅ **Testability**: Each extracted method independently testable
- ✅ **V12 DNA Alignment**: "Make illegal states unrepresentable"
- ✅ **Backward Compatibility**: No breaking changes to method signature

### Test Coverage Requirements
- ✅ **New Tests**: Add tests for each extracted method (TDD protocol)
- ✅ **Integration Tests**: Verify original method behavior unchanged
- ✅ **Edge Cases**: Test validation failures, state update failures, error paths
- ✅ **Performance**: Verify no performance regression (microsecond-latency threshold)

## Risk Assessment

### Technical Risks

#### HIGH RISK: State Synchronization
- **Risk**: Broker data → internal state requires atomic correctness
- **Mitigation**: Use Actor.Enqueue pattern for all state updates
- **Verification**: Add integration tests for concurrent position updates

#### MEDIUM RISK: Testing Gap
- **Risk**: No existing test coverage for this method
- **Mitigation**: Write tests BEFORE refactoring (V12.22 TDD protocol)
- **Verification**: Achieve 100% branch coverage for extracted methods

#### LOW RISK: Performance Regression
- **Risk**: Method extraction could add overhead
- **Mitigation**: Keep extracted methods inline-eligible (small, focused)
- **Verification**: Benchmark before/after refactoring

### Business Risks

#### MEDIUM RISK: Position Reconciliation Failure
- **Risk**: Incorrect position state could cascade to order placement
- **Mitigation**: Extensive integration testing with broker mocks
- **Verification**: Manual QA in staging environment

#### LOW RISK: Deployment Complexity
- **Risk**: Single-file change minimizes deployment risk
- **Mitigation**: Standard deployment process, no special handling required
- **Verification**: Canary deployment to subset of accounts

## Scope Creep Prevention (V12.23 Protocol)

### RED FLAGS - DO NOT PROCEED IF:
- ❌ Refactoring extends beyond `HydrateExpectedPositionsFromBroker`
- ❌ Changes required to broker API or state machine interfaces
- ❌ Modifications needed to calling methods or test infrastructure
- ❌ New dependencies or external libraries introduced
- ❌ Method signature changes breaking backward compatibility

### GREEN LIGHTS - PROCEED IF:
- ✅ All changes confined to single method and its extractions
- ✅ No interface changes to external systems
- ✅ Backward compatible with existing callers
- ✅ Test additions only (no test modifications)
- ✅ CCN reduction achievable within defined scope

## Phase 1.5 Completion Checklist

- ✅ **Scope Defined**: Single method extraction with clear boundaries
- ✅ **Extraction Strategy**: Three-phase approach (validation, state, error)
- ✅ **Success Criteria**: Quantitative (CCN ≤15) and qualitative (lock-free, testable)
- ✅ **Risk Assessment**: Technical and business risks identified with mitigations
- ✅ **Scope Creep Prevention**: V12.23 protocol enforced with red flags

## Next Phase
**Phase 2**: Mini-Spec Generation
- Generate detailed implementation specification
- Define test cases for each extracted method
- Create Director dialogue for validation
- Prepare for Phase 3 (TDD implementation)

---
**Scope Boundary Status**: ✅ VALIDATED
**Ready for Phase 2**: YES
**Estimated Complexity Reduction**: 5-7 CCN points
**Risk Level**: MEDIUM (manageable with defined mitigations)
