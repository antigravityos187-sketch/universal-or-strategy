# Phase 0: Hotspot Analysis - EPIC-CCN-111

## Target Method
- **Method**: HydrateExpectedPositionsFromBroker
- **File**: src/V12_002.SIMA.Lifecycle.cs
- **Cyclomatic Complexity**: 17
- **Threshold**: 15 (Jane Street alignment)
- **Violation**: +2 over threshold

## Complexity Metrics

### Cyclomatic Complexity Analysis
- **Current CCN**: 17
- **Target CCN**: ≤15 (V12 DNA mandate)
- **Reduction Required**: Minimum 2 points
- **Cognitive Load**: HIGH - exceeds Jane Street microsecond-latency threshold

### Method Characteristics
- **Purpose**: Hydrates expected positions from broker account data
- **State Management**: Likely contains conditional logic for position reconciliation
- **Risk Factors**:
  - Complex branching logic (17 decision points)
  - Position state synchronization
  - Potential race conditions in broker data access

## Blast Radius Assessment

### Direct Dependencies
- **Callers**: Methods that trigger position hydration (likely OnAccountItemUpdate, OnPositionUpdate)
- **Callees**: Broker API methods, position state accessors
- **Data Flow**: Broker account → Position state → SIMA state machine

### Impact Analysis
- **Scope**: MEDIUM - Position hydration is critical but isolated to lifecycle management
- **Failure Mode**: Incorrect position state could cascade to order placement logic
- **Testing Surface**: Requires broker connection mocking for comprehensive coverage

## Call Hierarchy

### Upstream Callers
- Position reconciliation triggers
- Account update callbacks
- Lifecycle initialization sequences

### Downstream Callees
- Broker position queries
- State machine position updates
- Logging/diagnostics

## Risk Assessment: MEDIUM-HIGH

### Risk Factors
1. **Complexity Violation**: CCN 17 exceeds threshold by 2 points
2. **State Synchronization**: Broker data → internal state requires atomic correctness
3. **Testing Gap**: No dedicated test coverage for this method (per AGENTS.md audit)
4. **Lock-Free Requirement**: Must verify no legacy lock() blocks exist

### Mitigation Strategy
1. **Extract Decision Logic**: Split conditional branches into separate validation methods
2. **State Machine Pattern**: Use FSM/Actor Enqueue for position updates
3. **TDD Coverage**: Add tests before refactoring (V12.22 protocol)
4. **Atomic Operations**: Ensure broker data reads are lock-free

## Refactoring Recommendations

### Primary Extraction Targets
1. **Position Validation Logic**: Extract broker position validation into separate method
2. **State Update Logic**: Isolate position state mutation into atomic operation
3. **Error Handling**: Separate error paths from happy path logic

### Expected Outcome
- **Target CCN**: 10-12 per extracted method
- **Maintainability**: Improved testability and cognitive simplicity
- **V12 DNA Alignment**: "Make illegal states unrepresentable" through type-safe validation

## Phase 0 Completion Criteria
- ✅ Hotspot identified: HydrateExpectedPositionsFromBroker (CCN 17)
- ✅ Blast radius assessed: MEDIUM scope, HIGH criticality
- ✅ Risk level determined: MEDIUM-HIGH
- ✅ Refactoring strategy outlined: Extract validation + state update logic
- ✅ TDD requirement noted: Add tests before surgery

## Next Phase
**Phase 1**: Vision/Spec - Generate mini-spec.md with Director dialogue
- Define extraction boundaries
- Specify atomic state update contract
- Design test cases for position hydration scenarios
