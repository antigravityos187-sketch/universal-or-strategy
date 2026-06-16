# Phase 2: Architecture Planning - EPIC-CCN-022

## Executive Summary

**Target Method**: `PropagateMaster_IdentifyMove`
**Current Complexity**: 18 (CYC)
**Target Complexity**: ≤8 (Jane Street strict standard)
**Extraction Strategy**: Break into 3 focused helper methods + simplified orchestrator
**Expected Reduction**: 55-66% complexity reduction

---

## 1. Extraction Strategy

### Current State
- **Method**: `PropagateMaster_IdentifyMove`
- **File**: `src/V12_002.Orders.Callbacks.Propagation.cs`
- **Complexity**: 18 (CYC)
- **LOC**: ~40 lines
- **Tier**: 1 (High Priority)
- **Overage**: +10 points (125% over Jane Street target of 8)

### Target State
Transform single complex method into 4 focused methods:

1. **Orchestrator Method** (Simplified `PropagateMaster_IdentifyMove`)
   - Target Complexity: 6-8 (CYC)
   - Responsibility: High-level flow coordination
   - Pattern: Linear orchestration with early returns

2. **Validation Helper** (`ValidateOrderStatesForPropagation`)
   - Target Complexity: 3-5 (CYC)
   - Responsibility: Order state validation logic
   - Reduction: -4 complexity points

3. **Decision Helper** (`DeterminePropagationAction`)
   - Target Complexity: 4-6 (CYC)
   - Responsibility: Propagation action determination
   - Reduction: -5 complexity points

4. **Error Handler** (`HandlePropagationError`)
   - Target Complexity: 2-3 (CYC)
   - Responsibility: Error handling and logging
   - Reduction: -3 complexity points

### Complexity Budget
Original:     18 (CYC)
Orchestrator:  6-8 (CYC)
Validation:    3-5 (CYC)
Decision:      4-6 (CYC)
Error:         2-3 (CYC)
----------------------------
Total:        15-22 (distributed across 4 methods)
Target Met:   Each method ≤8 (Jane Street strict)

---

## 2. Method Signatures

### 2.1 Original Method (Current)
private void PropagateMaster_IdentifyMove(Order masterOrder, Order slaveOrder, OrderAction action)

**Current Behavior**:
- Validates order states
- Determines propagation action
- Executes propagation via FSM
- Handles errors

**Complexity**: 18 (CYC) - Too high for cognitive simplicity

---

### 2.2 Proposed Helper Methods

#### Helper 1: Validation
private bool ValidateOrderStatesForPropagation(Order masterOrder, Order slaveOrder)

**Responsibilities**:
- Check master order state (e.g., Working, Filled, Cancelled)
- Check slave order state compatibility
- Validate order relationship (master-slave linkage)
- Return boolean result (no exceptions)

**Complexity Target**: 3-5 (CYC)
- Simple if/else chains
- No nested loops
- Early return pattern

**Thread-Safety**: Read-only, no locks required
**Jane Street Alignment**: Simple validation logic, easy to reason about

---

#### Helper 2: Decision Logic
private PropagationAction DeterminePropagationAction(Order masterOrder, Order slaveOrder, OrderAction action)

**Responsibilities**:
- Analyze master order action (Fill, Cancel, Modify)
- Determine slave order response (Propagate, Skip, Cancel)
- Apply business rules for propagation
- Return action enum (no side effects)

**Complexity Target**: 4-6 (CYC)
- Switch/case or if/else chains
- Business rule evaluation
- No state mutations

**Return Type** (Proposed Enum):
private enum PropagationAction
{
    None,           // No propagation needed
    PropagateMove,  // Propagate move to slave
    CancelSlave,    // Cancel slave order
    SkipPropagation // Skip due to invalid state
}

**Thread-Safety**: Read-only, no locks required
**Jane Street Alignment**: Decision logic isolated for testability

---

#### Helper 3: Error Handling
private void HandlePropagationError(Order masterOrder, Order slaveOrder, Exception exception)

**Responsibilities**:
- Log error details (order IDs, exception message)
- Record error metrics (if applicable)
- No state recovery (fail-fast pattern)
- No retries (keep it simple)

**Complexity Target**: 2-3 (CYC)
- Simple logging calls
- Minimal branching
- No complex error recovery

**Thread-Safety**: Logging is thread-safe, no locks required
**Jane Street Alignment**: Simple error handling, no complex recovery logic

---

### 2.3 Refactored Orchestrator Method

**Target Complexity**: 6-8 (CYC)
**Thread-Safety**: Uses FSM Enqueue pattern, no locks
**Jane Street Alignment**: Simple orchestration, easy to audit

**Complexity Breakdown**:
- Validation check: +1 (CYC)
- Switch statement: +4 (CYC) [4 cases]
- Exception handler: +1 (CYC)
- **Total**: 6 (CYC) - Well below target of 8

---

## 3. Call Graph

### 3.1 Method Invocation Flow

PropagateMaster_IdentifyMove (Orchestrator)
│
├─► ValidateOrderStatesForPropagation
│   └─► Returns: bool (valid/invalid)
│
├─► DeterminePropagationAction
│   └─► Returns: PropagationAction enum
│
├─► FSM Enqueue (Actor Pattern)
│   └─► Enqueues: PropagateCommand or CancelCommand
│
└─► HandlePropagationError (on exception)
    └─► Returns: void (logs error)

### 3.2 Data Flow

Input: (masterOrder, slaveOrder, action)
   │
   ├─► ValidateOrderStatesForPropagation(master, slave)
   │   └─► bool isValid
   │
   ├─► DeterminePropagationAction(master, slave, action)
   │   └─► PropagationAction actionType
   │
   ├─► FSM Enqueue(actionType, orders)
   │   └─► Command queued (async execution)
   │
   └─► HandlePropagationError(master, slave, ex)
       └─► Error logged (no return value)

### 3.3 Shared State Analysis

**No Shared Mutable State Between Helpers**:
- ValidateOrderStatesForPropagation: Read-only, no state mutations
- DeterminePropagationAction: Pure function, returns enum
- HandlePropagationError: Logging only, no state changes
- Orchestrator: Uses FSM Enqueue for state mutations

**Thread-Safety**:
- All helpers are stateless or read-only
- State mutations only via FSM Enqueue (Actor pattern)
- No lock() statements required

---

## 4. Lock-Free Validation

### 4.1 V12 DNA Compliance

**Mandatory Requirements**:
- No lock() statements in any method
- State mutations via FSM/Actor Enqueue pattern
- Atomic primitives for shared state (if any)
- ASCII-only strings (no Unicode, emoji, curly quotes)

### 4.2 Lock-Free Pattern Analysis

**BEFORE**: Potential lock usage (to be verified during implementation)
**AFTER**: Lock-free Actor pattern with FSM Enqueue

### 4.3 FSM/Actor Enqueue Pattern

**Command Pattern for State Mutations**:
All state changes go through FSM queue:
- _fsmQueue.Enqueue(new PropagateCommand(masterOrder, slaveOrder))
- _fsmQueue.Enqueue(new CancelCommand(slaveOrder))

FSM processes commands sequentially (single-threaded actor)
No race conditions, no locks needed

---

## 5. Jane Street Compliance

### 5.1 Cognitive Simplicity

**Jane Street Principle**: "Make illegal states unrepresentable"

**Current State** (CYC 18):
- High cognitive load (18 decision paths)
- Difficult to reason about under time pressure
- Exponential test paths (2^18 = 262,144 theoretical paths)

**Target State** (CYC ≤8 per method):
- Low cognitive load (max 8 decision paths per method)
- Easy to reason about (single responsibility per method)
- Linear test growth (isolated unit tests)

### 5.2 Microsecond Latency Requirements

**HFT Performance Considerations**:

1. **Hot Path Optimization**:
   - Orchestrator method is hot path (called frequently)
   - Target CYC 6-8 reduces instruction cache misses
   - Predictable branches improve CPU branch prediction

2. **Cold Path Extraction**:
   - Error handling is cold path (rarely executed)
   - Extract to separate method to keep hot path clean
   - Reduces code size in hot path

3. **Validation Efficiency**:
   - Simple boolean checks (fast)
   - Early return pattern (fail-fast)
   - No complex nested logic

### 5.3 Testability

**Jane Street Testing Principles** (from KB: "Why Testing Is Hard and How to Fix It"):

**Current State**:
- Exponential test paths (2^18 = 262,144)
- Difficult to isolate failure modes
- High test maintenance cost

**Target State**:
- Linear test growth (4 methods × ~5 tests each = 20 tests)
- Isolated unit tests per method
- Easy to verify correctness

### 5.4 Correctness by Construction

**Type-Safe Design**:
Use enums to make illegal states unrepresentable
Compiler enforces exhaustive switch handling

---

## 6. Implementation Checklist

### Phase 3: Pre-Implementation
- [ ] Review this architecture plan with Director
- [ ] Get approval for method signatures
- [ ] Verify PropagationAction enum doesn't already exist
- [ ] Check for existing validation/decision helper methods
- [ ] Run Arena AI adversarial audit on this plan

### Phase 4: Implementation
- [ ] Extract ValidateOrderStatesForPropagation (CYC 3-5)
- [ ] Extract DeterminePropagationAction (CYC 4-6)
- [ ] Extract HandlePropagationError (CYC 2-3)
- [ ] Refactor orchestrator to use helpers (CYC 6-8)
- [ ] Run python3 scripts/complexity_audit.py after each extraction
- [ ] Verify no lock() statements in modified file

### Phase 5: Verification
- [ ] Run dotnet build (zero errors)
- [ ] Run dotnet test (100% pass rate)
- [ ] Run dotnet csharpier check src/ (zero issues)
- [ ] Run powershell -File .\scripts\pre_push_validation.ps1 -Fast
- [ ] Manual F5 test in NinjaTrader
- [ ] Verify complexity: python3 scripts/complexity_audit.py

### Phase 6: Sign-off
- [ ] Run powershell -File .\deploy-sync.ps1
- [ ] Verify BUILD_TAG in NinjaTrader
- [ ] Update manifest.json with completion status
- [ ] Create PR with Arena AI audit

---

## 7. Risk Mitigation

### High-Risk Areas
1. **Order State Validation**: Complex state machine interactions
   - Mitigation: Isolate in pure function, add comprehensive tests
   
2. **Propagation Logic**: Business rules may be intricate
   - Mitigation: Use enum return type, exhaustive switch handling

3. **Error Handling**: Must not mask critical failures
   - Mitigation: Simple logging, fail-fast pattern

### Rollback Plan
- Bob CLI checkpointing enabled (auto-restore on failure)
- Git branch: epic-ccn-022-propagation-extraction
- Rollback command: git reset --hard HEAD~1

---

## 8. Success Metrics

### Complexity Reduction
- **Target**: CYC 18 → CYC ≤8 per method
- **Measurement**: python3 scripts/complexity_audit.py
- **Success**: All methods ≤8 (Jane Street strict)

### Test Coverage
- **Target**: 4 methods × 5 tests = 20 unit tests minimum
- **Measurement**: dotnet test --collect:"XPlat Code Coverage"
- **Success**: 100% branch coverage on extracted methods

### Build Health
- **Target**: Zero compilation errors, zero test failures
- **Measurement**: dotnet build && dotnet test
- **Success**: All green

### Lock-Free Compliance
- **Target**: Zero lock() statements in modified file
- **Measurement**: grep -r "lock(" src/V12_002.Orders.Callbacks.Propagation.cs
- **Success**: Zero matches

---

## Metadata
- **Epic**: EPIC-CCN-022
- **Phase**: 2.0 (Architecture Planning)
- **Date**: 2026-06-15
- **V12 Protocol**: V12.23
- **Analyzer**: Bob CLI (v12-engineer mode)
- **Jane Street Alignment**: Cognitive simplicity, testability, lock-free
- **Next Phase**: Phase 3 (DNA & PR Audit via Arena AI)
