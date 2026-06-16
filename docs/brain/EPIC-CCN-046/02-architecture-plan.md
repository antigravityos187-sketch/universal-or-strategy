# Phase 2: Architecture Planning - EPIC-CCN-046

## V12.23 Protocol Compliance

**Purpose**: Design extraction strategy for HandleChartClick_ConvertPrice complexity reduction.

**Status**: Architecture planning phase (Phase 2 of 7)

## Target Method Analysis

### Current State
- **Method**: HandleChartClick_ConvertPrice
- **File**: src/V12_002.UI.Callbacks.cs
- **Current Complexity**: 9 (CYC)
- **Current LOC**: 54
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **Tier**: 2 (Below threshold, surgical extraction)

### Complexity Breakdown
- **Current Decision Points**: 9
- **Target Distribution**: 
  - Main orchestrator: ≤3 decision points
  - Helper 1 (Validation): ≤2 decision points
  - Helper 2 (Conversion): ≤3 decision points
  - Helper 3 (State Update): ≤2 decision points
- **Total After Extraction**: 10 decision points across 4 methods (each under threshold)

## Extraction Strategy

### Principle: Orchestrator Pattern
Transform HandleChartClick_ConvertPrice from a monolithic method into a simple orchestrator that delegates to focused helper methods.

### Proposed Helper Methods (3 methods)

#### 1. ValidateChartClickInput
**Responsibility**: Validate chart click coordinates and chart state
**Complexity Target**: ≤2 decision points
**Logic**:
- Check if chart instance is valid (not null)
- Validate click coordinates are within chart bounds
- Return bool (true = valid, false = invalid)

#### 2. ConvertPriceCoordinates
**Responsibility**: Convert chart pixel coordinates to price values
**Complexity Target**: ≤3 decision points
**Logic**:
- Extract price scale from chart
- Apply coordinate transformation
- Handle edge cases (out of range, invalid scale)
- Return nullable price value (null = conversion failed)

#### 3. UpdateChartState
**Responsibility**: Update UI state with converted price
**Complexity Target**: ≤2 decision points
**Logic**:
- Update chart overlay with price marker
- Trigger UI refresh if needed
- Return void (fire-and-forget)

### Main Orchestrator (HandleChartClick_ConvertPrice)
**Complexity Target**: ≤3 decision points
**Logic**:
1. Call ValidateChartClickInput → if false, early return
2. Call ConvertPriceCoordinates → if null, early return
3. Call UpdateChartState → complete

## Method Signatures

### Original Method (Preserved)
Method signature preserved - no changes to callers.

### Proposed Helper Methods

#### Helper 1: Validation
Validates chart click input parameters and chart state.
Parameters: chart, clickX, clickY
Returns: bool (true if valid, false otherwise)
Pure function - no side effects, ≤2 decision points

#### Helper 2: Conversion
Converts chart pixel coordinates to price value.
Parameters: chart, clickY
Returns: double? (converted price or null if conversion fails)
Pure function - no side effects, ≤3 decision points

#### Helper 3: State Update
Updates chart UI state with converted price marker.
Parameters: chart, price
Returns: void
Side effects allowed (UI update), ≤2 decision points

## Call Graph

### Linear Data Flow (No Circular Dependencies)

HandleChartClick_ConvertPrice (Orchestrator) calls:
1. ValidateChartClickInput(chart, x, y) → returns bool
   - false → early return (exit)
   - true → continue
2. ConvertPriceCoordinates(chart, y) → returns double?
   - null → early return (exit)
   - value → continue
3. UpdateChartState(chart, price) → returns void (complete)

### Shared State Analysis
- **No Shared Mutable State**: All data flows through parameters and return values
- **Pure Functions**: ValidateChartClickInput and ConvertPriceCoordinates are pure (no side effects)
- **Isolated Side Effects**: UpdateChartState is the only method with side effects (UI update)
- **Thread Safety**: No lock() statements needed - UI callbacks run on main thread

## Lock-Free Validation

### No lock() Statements
- **ValidateChartClickInput**: Pure function, no locking needed
- **ConvertPriceCoordinates**: Pure function, no locking needed
- **UpdateChartState**: Uses atomic operations or FSM Enqueue pattern if state mutation required
- **Main Orchestrator**: No shared state, no locking needed

### FSM/Actor Enqueue Pattern
If UpdateChartState needs to mutate shared state, it will use FSM Enqueue pattern.
No direct state mutation outside FSM/Actor pattern.

### Atomic Primitives Only
If simple state updates needed, use Interlocked operations.
No manual lock() blocks or Monitor.Enter/Exit.

## Jane Street Compliance

### Principle 1: Make Illegal States Unrepresentable

**Application**:
- ValidateChartClickInput returns bool → invalid state cannot proceed
- ConvertPriceCoordinates returns double? → null represents conversion impossible
- No exception-based control flow → outcomes are explicit in type system
- Early returns prevent invalid state propagation

**Verification**: Type system enforces valid state transitions

### Principle 2: Cognitive Simplicity

**Application**:
- Each helper has single, clear responsibility
- Main orchestrator is simple sequential flow
- No clever abstractions or hidden control flow
- Each method ≤3 decision points (easy to reason about)
- Total complexity distributed across 4 focused methods

**Verification**: Each method is independently understandable

### Principle 3: Test Exhaustively

**Testing Strategy**:
1. Unit Tests for Helpers (if test infrastructure exists)
2. Integration Test for Orchestrator
3. Behavior Preservation verification

**Verification**: Each helper is independently testable

### Principle 4: Microsecond Latency Awareness

**Application**:
- Pure functions have zero allocation overhead
- No exception throwing in hot path
- No lock contention (lock-free design)
- Minimal method call overhead

**Verification**: No performance regression expected

## Incremental Extraction Sequence

### Step 1: Extract ValidateChartClickInput
1. Create private method ValidateChartClickInput
2. Move validation logic from main method
3. Update main method to call helper
4. Test: Verify validation behavior unchanged
5. Commit: EPIC-CCN-046: Extract ValidateChartClickInput

### Step 2: Extract ConvertPriceCoordinates
1. Create private method ConvertPriceCoordinates
2. Move conversion logic from main method
3. Update main method to call helper
4. Test: Verify conversion behavior unchanged
5. Commit: EPIC-CCN-046: Extract ConvertPriceCoordinates

### Step 3: Extract UpdateChartState
1. Create private method UpdateChartState
2. Move state update logic from main method
3. Update main method to call helper
4. Test: Verify UI update behavior unchanged
5. Commit: EPIC-CCN-046: Extract UpdateChartState

### Step 4: Verify Complexity Reduction
1. Run complexity audit
2. Verify HandleChartClick_ConvertPrice ≤8 CYC
3. Verify each helper ≤3 CYC
4. Run full test suite (if exists)
5. Commit: EPIC-CCN-046: Verify complexity reduction

## Risk Mitigation

### Checkpoint Strategy
- Git commit before extraction
- Incremental commits after each helper extraction
- Rollback plan if behavior changes

### Verification Checklist
- Build succeeds after each extraction
- No new compiler warnings
- Complexity audit shows CYC ≤8
- UI behavior identical to original
- No new race conditions (lock-free audit)
- Hard-link sync via deploy-sync.ps1

### Rollback Triggers
- Compilation errors after extraction
- Behavior changes detected in testing
- Complexity not reduced to ≤8
- New lock() statements introduced
- Performance regression detected

## V12 DNA Compliance Checklist

### Pre-Implementation Verification
- Extraction strategy defined (3 helpers + orchestrator)
- Method signatures designed (pure functions + isolated side effects)
- Call graph documented (linear flow, no circular dependencies)
- Lock-free validation complete (no lock() statements)
- Jane Street alignment verified (cognitive simplicity, testability)
- Incremental extraction sequence planned
- Risk mitigation strategy defined

### Implementation Constraints
- Lock-free Actor/FSM pattern maintained
- ASCII-only compliance required
- Curly braces on all control structures
- No whitespace mutation
- Hard-link sync via deploy-sync.ps1

## Success Criteria

### Complexity Reduction
- **Target**: HandleChartClick_ConvertPrice CYC ≤8
- **Verification**: complexity_audit.py
- **Acceptance**: Zero methods with CYC >8 in V12_002.UI.Callbacks.cs

### Behavior Preservation
- **Target**: Identical UI behavior before/after extraction
- **Verification**: Manual testing + integration tests (if exist)
- **Acceptance**: No regressions detected

### Lock-Free Compliance
- **Target**: Zero lock() statements in extracted code
- **Verification**: grep for lock() statements
- **Acceptance**: Zero matches

### Jane Street Alignment
- **Target**: Each helper ≤3 decision points
- **Verification**: Complexity audit per method
- **Acceptance**: All helpers under threshold

## Next Steps

1. **Phase 3**: DNA & PR Audit (Adjudicator review)
2. **Phase 4**: Recursive Execution (Bob CLI implementation)
3. **Phase 5**: Verification/Review (Forensics audit)
4. **Phase 6**: Sign-off (Director approval)

---

**Document Version**: 1.0
**Created**: 2026-06-15
**Protocol**: V12.23 Phase 2 (Architecture Planning)
**Status**: READY FOR PHASE 3 REVIEW
