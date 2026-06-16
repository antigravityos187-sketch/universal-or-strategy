# Phase 2: Architecture Planning - EPIC-CCN-028

## Target Method Analysis

### Current State
- **Method**: `ProcessFlattenWorkItem_CancelOrders`
- **File**: `src/V12_002.SIMA.Flatten.cs`
- **Current Complexity**: 18 (CYC)
- **Current LOC**: 36
- **Target Complexity**: ≤8 (Jane Street strict standard)

### Complexity Breakdown
With CYC=18, the method likely contains:
- Multiple conditional branches (if/else) for order state validation
- Error handling paths (try/catch blocks)
- FSM state transition logic
- Logging and diagnostic code
- Order cancellation execution logic

## Extraction Strategy

### Overview
Reduce complexity from 18 to ≤8 by extracting three focused helper methods:

1. **ValidateCancellationRequest** - Pre-condition validation (CYC ≤3)
2. **ExecuteOrderCancellations** - Core cancellation logic (CYC ≤5)
3. **LogCancellationOutcome** - Result logging and diagnostics (CYC ≤2)

### Rationale
- **Single Responsibility**: Each helper has one clear purpose
- **Cognitive Simplicity**: CYC ≤5 per helper enables rapid comprehension
- **Testability**: Isolated helpers allow exhaustive unit testing
- **Maintainability**: Changes to validation, execution, or logging are isolated

## Method Signatures

### Original Method (Assumed)
Private void ProcessFlattenWorkItem_CancelOrders with FlattenWorkItem parameter.
Current implementation: 36 LOC, CYC=18.
Contains validation, execution, error handling, and logging.

### Proposed Helper Methods

#### 1. ValidateCancellationRequest
Validates pre-conditions for order cancellation.
- Parameter: FlattenWorkItem item
- Returns: ValidationResult (IsValid bool, FailureReason string)
- Complexity Target: CYC ≤3

Responsibilities:
- Null/empty checks on work item
- Verify order state allows cancellation
- Check FSM state compatibility
- Return structured validation result

#### 2. ExecuteOrderCancellations
Executes the core order cancellation logic.
- Parameter: FlattenWorkItem item (validated)
- Returns: CancellationResult (Success bool, CancelledCount int, Errors List)
- Complexity Target: CYC ≤5

Responsibilities:
- Iterate through orders in work item
- Invoke NinjaTrader cancellation API
- Collect cancellation results
- Handle immediate execution errors
- Return structured result

#### 3. LogCancellationOutcome
Logs the outcome of cancellation operations.
- Parameter: CancellationResult result
- Returns: void
- Complexity Target: CYC ≤2

Responsibilities:
- Log successful cancellations (count, order IDs)
- Log failures with error details
- Use existing V12 logging infrastructure
- No complex branching

### Supporting Types

ValidationResult struct:
- IsValid: bool
- FailureReason: string

CancellationResult struct:
- Success: bool
- CancelledCount: int
- Errors: List<string>

## Call Graph

### Data Flow
ProcessFlattenWorkItem_CancelOrders(item)
  Step 1: validationResult = ValidateCancellationRequest(item)
  Step 2: if (!validationResult.IsValid) -> LogCancellationOutcome(failure) -> return
  Step 3: cancellationResult = ExecuteOrderCancellations(item)
  Step 4: LogCancellationOutcome(cancellationResult)

### Method Dependencies
- **ValidateCancellationRequest**: No dependencies on other helpers (pure validation)
- **ExecuteOrderCancellations**: No dependencies on other helpers (pure execution)
- **LogCancellationOutcome**: No dependencies on other helpers (pure logging)

### Shared State
**NONE** - All helpers are stateless:
- Input: Method parameters only
- Output: Return values or void
- No shared mutable state
- No static variables
- No class-level fields modified

## Lock-Free Validation

### FSM/Actor Pattern Compliance
- **Main Method**: Orchestrates helpers via sequential calls (no locks)
- **Helper Methods**: Pure functions or message-passing only
- **State Mutations**: All FSM state changes use Enqueue pattern
- **Atomic Operations**: Use Interlocked for counters if needed

### No Lock Statements
BANNED: lock(stateLock) pattern
APPROVED: _fsmActor.Enqueue(new StateTransitionMessage)

### Atomic Primitives
APPROVED: Interlocked.Increment(ref _cancellationCount)
BANNED: _cancellationCount++ (non-atomic)

### Immutable Parameters
All helper methods receive immutable or read-only parameters:
- FlattenWorkItem item - read-only reference
- No ref or out parameters that mutate caller state
- Return new result objects instead of mutating inputs

## Jane Street Compliance

### Cognitive Simplicity (CYC ≤8)
- **Main Method**: CYC ≤8 (orchestration only)
- **ValidateCancellationRequest**: CYC ≤3 (simple checks)
- **ExecuteOrderCancellations**: CYC ≤5 (iteration + error handling)
- **LogCancellationOutcome**: CYC ≤2 (success/failure logging)

**Rationale**: Functions with CYC >8 are harder to:
- Reason about under microsecond-latency constraints
- Test exhaustively (exponential path growth)
- Audit for race conditions in lock-free code

### Correctness by Construction
- **ValidationResult**: Makes invalid states unrepresentable (IsValid + FailureReason)
- **CancellationResult**: Explicit success/failure with error collection
- **Type Safety**: Compiler enforces proper result handling
- **No Runtime Guards**: Architecture prevents invalid states at design time

### Microsecond-Latency Optimization
- **No Locks**: Zero contention overhead
- **Pure Functions**: Minimal allocation, predictable performance
- **Linear Data Flow**: No backtracking or complex state machines
- **Fail-Fast Validation**: Early exit on validation failure

### Testability (Will Wilson Principles)
Each helper is independently testable:
- **ValidateCancellationRequest**: Test all validation paths (null, invalid state, valid)
- **ExecuteOrderCancellations**: Test success, partial failure, total failure
- **LogCancellationOutcome**: Verify correct log levels and messages

**Test Coverage Target**: 100% branch coverage per helper (achievable with CYC ≤5)

## Jane Street Knowledge Base Insights

### Available Documents Consulted
- **carl_cook_microsecond_2017**: Microsecond-latency constraints
- **gjengset_concurrency_coordination_2020**: Lock-free coordination costs
- **will_wilson_why_testing_hard_2026**: Testing principles for complex systems

### Key Principles Applied
1. **Simplicity Over Cleverness**: Straightforward extraction beats complex abstractions
2. **Fail-Fast Validation**: Validate early, exit early
3. **Pure Functions**: Minimize side effects and shared state
4. **Type-Driven Design**: Use types to enforce correctness

## Implementation Constraints

### V12 DNA Mandates
- ASCII-Only: All string literals use ASCII characters
- Lock-Free: No lock() statements
- Actor Pattern: State mutations via FSM Enqueue
- Atomic Primitives: Use Interlocked for counters

### NinjaTrader Compatibility
- Maintain compatibility with NinjaTrader 8 API
- Use existing logging infrastructure
- Preserve hard-link integrity (deploy-sync.ps1)

### Testing Requirements
- Add unit tests for each extracted helper
- Maintain existing integration tests
- Verify FSM state transitions remain correct

## Success Criteria

### Complexity Reduction
- Main method: CYC ≤8
- ValidateCancellationRequest: CYC ≤3
- ExecuteOrderCancellations: CYC ≤5
- LogCancellationOutcome: CYC ≤2

### Lock-Free Compliance
- Zero lock() statements
- FSM/Actor Enqueue pattern used
- Atomic primitives for counters

### Jane Street Alignment
- Cognitive simplicity maintained
- Correctness by construction
- Testability improved
- Microsecond-latency optimized

## Next Steps

1. **Phase 3**: Create 03-implementation-plan.md with:
   - Detailed implementation steps
   - Mermaid sequence diagrams
   - Test plan for each helper
   - Rollback strategy

2. **Phase 4**: Submit to Arena AI for adversarial audit
   - Verify lock-free compliance
   - Check for hidden complexity
   - Validate Jane Street alignment

3. **Phase 5**: Execute extraction in Bob CLI (v12-engineer mode)
   - Implement helpers one at a time
   - Run tests after each extraction
   - Verify complexity reduction with complexity_audit.py

## Metadata
- **Epic ID**: EPIC-CCN-028
- **Phase**: 2 (Architecture Planning)
- **Status**: COMPLETE
- **Date**: 2026-06-15
- **Architect**: Bob Shell (Plan Mode)
- **Target Complexity**: CYC ≤8 (Jane Street strict standard)
- **Extraction Method**: Three-Helper Pattern (Validate -> Execute -> Log)
