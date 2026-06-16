# Phase 2: Architecture Planning - EPIC-CCN-019

## V12 Photon Kernel Extraction Protocol

This document defines the architectural plan for extracting helper methods from TryHandleFleet_MoveTarget to reduce cyclomatic complexity from 15 to ≤8 per method.

## 1. Extraction Strategy

### Current State
- **Method**: TryHandleFleet_MoveTarget
- **File**: src/V12_002.UI.IPC.Commands.Fleet.cs
- **Current Complexity**: 15 (CYC)
- **Current LOC**: 33
- **Tier**: 1 (High Priority)

### Target State
- **Target Complexity**: ≤8 per method (Jane Street strict standard)
- **Extraction Count**: 2 helper methods
- **Total Methods**: 3 (1 orchestrator + 2 helpers)
- **Complexity Distribution**: ~5 + ~5 + ~5 = ~15 total (distributed)

### Extraction Rationale
The current method handles multiple concerns that can be cleanly separated:
1. **Validation Concern**: Parameter validation, fleet state checks, target position validation
2. **Processing Concern**: Core movement logic, FSM/Actor Enqueue, event emission
3. **Orchestration Concern**: Coordinating validation and processing with error handling

This separation achieves:
- Single Responsibility Principle (each method has one clear purpose)
- Cognitive Simplicity (CYC ≤8 enables microsecond-latency reasoning)
- Testability (each concern can be tested in isolation)
- Maintainability (changes to validation do not affect processing logic)

## 2. Method Signatures

### Original Method (Orchestrator)
Original signature remains UNCHANGED. After refactoring, it orchestrates validation and processing with target CYC ~5.

### Extracted Helper 1: Validation
Private method ValidateFleetMoveCommand with target CYC ~5. Validates context, fleetId, fleet existence, fleet state, and target position. Returns bool with out errorMessage parameter.

### Extracted Helper 2: Processing
Private method ProcessFleetMoveTarget with target CYC ~5. Constructs command, enqueues to FSM/Actor (lock-free), emits event. Returns bool with out errorMessage parameter.

## 3. Call Graph

Linear flow with no shared mutable state:
- TryHandleFleet_MoveTarget calls ValidateFleetMoveCommand
- If validation passes, calls ProcessFleetMoveTarget
- All communication via parameters and return values
- Lock-free: uses FSM/Actor Enqueue pattern
- Atomic operations handled by FSM/Actor internally

## 4. Lock-Free Validation

- ValidateFleetMoveCommand: Pure validation, no state mutation, no locks
- ProcessFleetMoveTarget: Uses FSM/Actor Enqueue (lock-free by design)
- TryHandleFleet_MoveTarget: Orchestration only, no locks
- No lock(stateLock) blocks
- FSM/Actor Enqueue pattern preserved
- Atomic operations maintained
- No new synchronization primitives
- ASCII-only string literals
- Correctness by construction (validation before processing)

## 5. Jane Street Compliance

### Cognitive Simplicity (CYC ≤8)
Jane Street HFT systems prioritize cognitive simplicity. Functions with CYC >8 are harder to reason about under microsecond-latency constraints, test exhaustively, and audit for race conditions.

Our extraction achieves:
- ValidateFleetMoveCommand: CYC ~5
- ProcessFleetMoveTarget: CYC ~5
- TryHandleFleet_MoveTarget: CYC ~5
- Total: 3 methods, each ≤8, achieving Jane Street standard

### Testability
From Jane Street KB "Why Testing Is Hard and How to Fix It": High complexity makes exhaustive testing exponentially harder. Our extraction enables ~15 focused test cases vs exponential growth for CYC 15.

### Microsecond-Latency Reasoning
- Small methods fit in L1 cache (cognitive and CPU)
- Single responsibility (no mental context switching)
- Predictable branches (≤5 per method)
- Lock-free (no unpredictable lock contention)

## 6. Test Plan

### Unit Tests for ValidateFleetMoveCommand (5 test cases)
1. Valid command returns true
2. Null context returns false with error
3. Empty fleetId returns false with error
4. Fleet not found returns false with error
5. Out of bounds target returns false with error

### Unit Tests for ProcessFleetMoveTarget (3 test cases)
6. Successful processing returns true
7. Enqueue failure returns false with error
8. Event emission verification

### Integration Test for TryHandleFleet_MoveTarget (2 test cases)
9. Black-box equivalence with valid input
10. Validation failure propagation

### Test Coverage Target
- ValidateFleetMoveCommand: 100% branch coverage (5 branches)
- ProcessFleetMoveTarget: 100% branch coverage (5 branches)
- TryHandleFleet_MoveTarget: 100% branch coverage (5 branches)
- Total: 10 test cases for ~15 branches

## 7. Implementation Checklist

### Pre-Implementation
- Scope boundary validated (Phase 1.5)
- Architecture plan created (Phase 2)
- Arena AI red team review (Phase 3) - PENDING
- Bob CLI checkpointing enabled - REQUIRED

### Implementation Steps (Incremental with Verification)
1. Extract ValidateFleetMoveCommand (first helper)
2. Verify build succeeds
3. Run unit tests for ValidateFleetMoveCommand
4. Extract ProcessFleetMoveTarget (second helper)
5. Verify build succeeds
6. Run unit tests for ProcessFleetMoveTarget
7. Refactor TryHandleFleet_MoveTarget (orchestrator)
8. Verify build succeeds
9. Run integration test for TryHandleFleet_MoveTarget
10. Run full test suite
11. Verify complexity reduction (CYC ≤8 per method)
12. Run deploy-sync.ps1
13. F5 in NinjaTrader (smoke test)

### Post-Implementation
- Arena AI adversarial audit
- PR hygiene check (verify_pr_hygiene.ps1)
- Pre-push validation (pre_push_validation.ps1)
- Merge to main

## 8. Risk Mitigation

### Risk 1: Breaking FSM/Actor Pattern
**Mitigation**: ProcessFleetMoveTarget preserves exact Enqueue call, no changes to FSM/Actor infrastructure

### Risk 2: Black-Box Equivalence Violation
**Mitigation**: Integration test verifies same behavior as original method, rollback on failure

### Risk 3: Complexity Redistribution Failure
**Mitigation**: Incremental extraction with verification after each step, complexity audit after each extraction

### Risk 4: Test Coverage Gap
**Mitigation**: 10 test cases defined upfront, 100% branch coverage target, test-first approach

## 9. Success Criteria

### Functional Requirements
- TryHandleFleet_MoveTarget maintains original signature
- Black-box equivalence preserved (same inputs → same outputs)
- FSM/Actor Enqueue pattern preserved
- Event emission preserved
- Error handling preserved

### Non-Functional Requirements
- ValidateFleetMoveCommand: CYC ≤8
- ProcessFleetMoveTarget: CYC ≤8
- TryHandleFleet_MoveTarget: CYC ≤8
- All methods: ASCII-only strings
- All methods: Lock-free
- Test coverage: 100% branch coverage

### Quality Gates
- Build succeeds (zero errors)
- All tests pass (100%)
- Complexity audit passes (CYC ≤8)
- Pre-push validation passes
- Arena AI audit passes

## 10. Next Steps (Phase 3)

**Arena AI Red Team Review**:
1. Validate extraction strategy against V12 DNA
2. Verify lock-free compliance
3. Audit test plan completeness
4. Check for scope creep
5. Approve/reject for Phase 4 (Implementation)

**If Approved**: Proceed to Phase 4 (Recursive Execution) with Bob CLI

**If Rejected**: Return to Phase 2 with feedback, revise architecture plan

---

**ARCHITECTURE PLAN COMPLETE**
**READY FOR PHASE 3 (ARENA AI RED TEAM REVIEW)**
