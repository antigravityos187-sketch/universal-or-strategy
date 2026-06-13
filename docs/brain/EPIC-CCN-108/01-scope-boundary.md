# Phase 1.5: Scope Boundary Validation - EPIC-CCN-108

## Epic Context
- **Epic ID**: EPIC-CCN-108
- **Phase**: 1.5 (Scope Boundary Validation)
- **Date**: 2026-06-13
- **Status**: IN PROGRESS

## Target Method Details

### Primary Target
- **Method Name**: `SweepBrokerOrders`
- **File Path**: `src/V12_002.SIMA.Lifecycle.cs`
- **Current Complexity**: 24 CCN
- **Target Complexity**: ≤ 15 CCN
- **Complexity Reduction Required**: -9 CCN (37.5% reduction)
- **Lines of Code**: ~150-200 (estimated)

### Method Signature
```csharp
private void SweepBrokerOrders()
```

### Method Responsibility
Core order lifecycle management method responsible for:
1. Sweeping and validating broker order states
2. Synchronizing order state with FSM/Actor model
3. Processing order state transitions
4. Maintaining lock-free guarantees during order processing

## Extraction Strategy

### What to Extract (4 Methods)

#### 1. ValidateOrderState()
- **Purpose**: Extract order validation logic
- **Target CCN**: ~5
- **Responsibility**: Validate order state consistency and business rules
- **Extraction Rationale**: Isolates validation concerns from orchestration logic

#### 2. SyncBrokerOrderState()
- **Purpose**: Extract state synchronization logic
- **Target CCN**: ~4
- **Responsibility**: Synchronize broker order state with internal FSM state
- **Extraction Rationale**: Separates synchronization primitives from business logic

#### 3. ProcessOrderTransition()
- **Purpose**: Extract state machine transition logic
- **Target CCN**: ~6
- **Responsibility**: Handle FSM state transitions for order lifecycle events
- **Extraction Rationale**: Encapsulates state machine complexity

#### 4. LogOrderEvent()
- **Purpose**: Extract logging and telemetry
- **Target CCN**: ~2
- **Responsibility**: Centralize order event logging and metrics
- **Extraction Rationale**: Removes cross-cutting concerns from core logic

### What to Keep in SweepBrokerOrders()
- **Orchestration Logic**: High-level coordination of extracted methods
- **Loop Structure**: Main iteration over broker orders
- **Exception Handling**: Top-level error handling and recovery
- **Target CCN**: 8-10 (orchestration only)

### Extraction Sequence
1. **First**: Extract `LogOrderEvent()` (lowest risk, no business logic)
2. **Second**: Extract `ValidateOrderState()` (pure validation, no side effects)
3. **Third**: Extract `SyncBrokerOrderState()` (synchronization primitives)
4. **Fourth**: Extract `ProcessOrderTransition()` (state machine logic)

## Boundary Definition (V12.23 No Scope Creep Protocol)

### In-Scope (ONLY)
✅ **Single Method**: `SweepBrokerOrders` in `src/V12_002.SIMA.Lifecycle.cs`
✅ **4 Extracted Methods**: As defined above
✅ **Unit Tests**: For each extracted method
✅ **Integration Test**: For refactored `SweepBrokerOrders`
✅ **Documentation**: Method-level XML comments

### Out-of-Scope (FORBIDDEN)
❌ **Other Methods**: No changes to callers (OnBarUpdate, etc.)
❌ **Other Files**: No changes to V12_002.cs or other lifecycle files
❌ **Architecture Changes**: No FSM/Actor model modifications
❌ **API Changes**: Method signature must remain unchanged
❌ **Performance Optimization**: Focus on complexity only, not performance
❌ **Feature Additions**: No new functionality
❌ **Refactoring Other Methods**: Only SweepBrokerOrders

### Boundary Enforcement
- **Single Method Rule**: Only `SweepBrokerOrders` may be modified
- **No Cascading Changes**: Extracted methods must be private and local
- **Signature Preservation**: Public/protected signatures unchanged
- **Behavioral Equivalence**: Output must match original exactly

## Success Criteria

### Primary Success Criteria
1. ✅ **Complexity Target**: `SweepBrokerOrders` CCN ≤ 15
2. ✅ **Extracted Methods**: All 4 methods created with individual CCN ≤ 6
3. ✅ **Total CCN**: Sum of all methods ≤ 27 (original 24 + 3 overhead allowance)
4. ✅ **Behavioral Equivalence**: All existing tests pass unchanged
5. ✅ **Lock-Free Guarantee**: No lock blocks introduced

### Secondary Success Criteria
6. ✅ **Test Coverage**: 100% line coverage for extracted methods
7. ✅ **Documentation**: XML comments for all extracted methods
8. ✅ **Code Review**: Passes V12 DNA compliance check
9. ✅ **No Regressions**: Zero new bugs introduced
10. ✅ **ASCII-Only**: No Unicode violations

### Verification Checklist
- [ ] Run complexity analysis: `lizard src/V12_002.SIMA.Lifecycle.cs`
- [ ] Verify CCN ≤ 15 for `SweepBrokerOrders`
- [ ] Verify CCN ≤ 6 for each extracted method
- [ ] Run all existing tests: `dotnet test`
- [ ] Run new unit tests for extracted methods
- [ ] Verify lock-free guarantees (no `lock` keyword)
- [ ] Check ASCII-only compliance
- [ ] Review XML documentation completeness

## Risk Assessment

### Overall Risk Level: HIGH

### Risk Factors

#### 1. Complexity Risk (HIGH)
- **Current CCN**: 24 (60% over threshold)
- **Mitigation**: Incremental extraction with TDD approach
- **Validation**: Run tests after each extraction

#### 2. Behavioral Risk (HIGH)
- **Concern**: Order lifecycle is mission-critical
- **Mitigation**: Maintain exact behavioral equivalence
- **Validation**: Comprehensive integration tests

#### 3. Lock-Free Risk (MEDIUM)
- **Concern**: Must preserve lock-free guarantees
- **Mitigation**: No synchronization primitives in extracted methods
- **Validation**: Code review for `lock`, `Monitor`, `Mutex` keywords

#### 4. State Machine Risk (MEDIUM)
- **Concern**: FSM state transitions are complex
- **Mitigation**: Extract state logic carefully, preserve transitions
- **Validation**: State machine unit tests

#### 5. Testing Gap Risk (HIGH)
- **Concern**: No existing unit tests for this method
- **Mitigation**: Create comprehensive test suite before refactoring
- **Validation**: Achieve 100% coverage before extraction

### Risk Mitigation Strategy

#### Phase 1: Pre-Refactoring
1. Create comprehensive integration tests for current behavior
2. Document all edge cases and state transitions
3. Establish baseline metrics (CCN, LOC, test coverage)

#### Phase 2: Extraction
1. Extract one method at a time
2. Run full test suite after each extraction
3. Verify CCN reduction after each step
4. Commit after each successful extraction

#### Phase 3: Validation
1. Run full regression test suite
2. Verify lock-free guarantees
3. Check V12 DNA compliance
4. Perform code review

### Rollback Plan
- **Git Branch**: Create feature branch for refactoring
- **Commit Strategy**: Atomic commits per extraction
- **Rollback Trigger**: Any test failure or CCN increase
- **Recovery**: Revert to last known good commit

## Dependencies and Constraints

### Technical Dependencies
- **Language**: C# (.NET 8.0)
- **Framework**: NinjaTrader 8 API
- **Patterns**: FSM/Actor model (lock-free)
- **Testing**: xUnit or NUnit

### Constraints
1. **No Breaking Changes**: Method signature must remain unchanged
2. **Lock-Free Requirement**: No synchronization primitives
3. **Performance**: No performance degradation allowed
4. **Backward Compatibility**: Must work with existing callers

## V12 DNA Compliance

### Alignment with Jane Street Principles
- ✅ **Cognitive Simplicity**: Target CCN ≤ 15 (Jane Street aligned)
- ✅ **Correctness by Construction**: TDD approach with comprehensive tests
- ✅ **Testability**: 100% coverage for extracted methods
- ✅ **Composability**: Small, focused methods with single responsibility

### V12 Protocol Compliance
- ✅ **V12.23 No Scope Creep**: Single method boundary enforced
- ✅ **V12 Complexity Threshold**: Target CCN ≤ 15
- ✅ **Lock-Free Guarantee**: FSM/Actor pattern preserved
- ✅ **ASCII-Only**: No Unicode violations

## Next Steps (Phase 2: Forensic Deep Dive)

1. **Analyze Implementation**: Line-by-line analysis of `SweepBrokerOrders`
2. **Identify Extraction Points**: Mark exact lines for each extraction
3. **Design Test Cases**: Create test scenarios for each extracted method
4. **Create Extraction Plan**: Detailed step-by-step extraction sequence
5. **Prepare Test Suite**: Write tests before refactoring (TDD)

## Metadata
- **Document Version**: 1.0
- **Phase**: 1.5 (Scope Boundary Validation)
- **Status**: COMPLETED
- **Author**: V12 Phase 1.5 Scope Boundary Validator
- **Date**: 2026-06-13
- **Epic**: EPIC-CCN-108
- **Target Method**: SweepBrokerOrders
- **Target File**: src/V12_002.SIMA.Lifecycle.cs
