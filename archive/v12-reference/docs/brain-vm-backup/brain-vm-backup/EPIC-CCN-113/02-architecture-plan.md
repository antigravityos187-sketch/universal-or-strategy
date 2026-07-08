# Phase 2: Architecture Planning - EPIC-CCN-113

## Executive Summary

**Epic Status**: HOLD (Method Within Threshold)
**Method**: HydrateFSMsFromWorkingOrders
**File**: src/V12_002.SIMA.Lifecycle.cs
**Current Complexity**: 14
**Jane Street Threshold**: ≤15
**Compliance**: ✅ WITHIN THRESHOLD

## Architecture Decision

### V12.23 Protocol Compliance: NO EXTRACTION REQUIRED

Per Phase 1.5 scope boundary validation, this method does NOT require extraction at this time. Current complexity (14) is within the Jane Street threshold (≤15).

**This document provides the HYPOTHETICAL architecture plan** that would be executed IF complexity exceeds 15 in the future.

---

## Method Signature Analysis

### Current Signature (Before Extraction)

```csharp
// Location: src/V12_002.SIMA.Lifecycle.cs
private void HydrateFSMsFromWorkingOrders()
{
    // Current implementation
    // Complexity: 14
    // Lines: ~50-80 (estimated)
}
```

### Proposed Signatures (After Hypothetical Extraction)

```csharp
// Main orchestration method (reduced complexity: ~8)
private void HydrateFSMsFromWorkingOrders()
{
    foreach (var order in WorkingOrders)
    {
        if (!ValidateWorkingOrderState(order))
        {
            continue;
        }
        
        var fsm = InitializeFSMState(order);
        BindFSMToOrder(order, fsm);
    }
}

// Extracted validation method (complexity: ~4)
private bool ValidateWorkingOrderState(Order order)
{
    // Order state validation logic
    // Branching conditions extracted here
    return isValid;
}

// Extracted initialization method (complexity: ~3)
private FSMState InitializeFSMState(Order order)
{
    // FSM state setup logic
    // Initialization branching extracted here
    return fsmState;
}
```

---

## Call Graph Analysis

### Current Call Graph

```
HydrateFSMsFromWorkingOrders (CYC: 14)
├── WorkingOrders.GetEnumerator()
├── Order.State (property access)
├── Order.Type (property access)
├── FSM.Initialize()
├── FSM.SetState()
└── FSM.BindToOrder()
```

### Proposed Call Graph (After Extraction)

```
HydrateFSMsFromWorkingOrders (CYC: ~8)
├── ValidateWorkingOrderState() [NEW] (CYC: ~4)
│   ├── Order.State
│   ├── Order.Type
│   └── Order.IsValid()
├── InitializeFSMState() [NEW] (CYC: ~3)
│   ├── FSM.Initialize()
│   └── FSM.SetState()
└── BindFSMToOrder() [EXISTING]
    └── FSM.BindToOrder()
```

### Call Graph Impact Analysis

**Upstream Callers** (No changes required):
- Strategy initialization methods
- Lifecycle startup routines
- FSM hydration pipeline

**Downstream Callees** (No changes required):
- FSM state setters
- Order validators
- State transition handlers

**Blast Radius**: MINIMAL (internal refactor only)

---

## Dependency Mapping

### Data Dependencies

```
INPUT DEPENDENCIES:
├── WorkingOrders (IEnumerable<Order>)
│   ├── Order.State (OrderState enum)
│   ├── Order.Type (OrderType enum)
│   ├── Order.Instrument (string)
│   └── Order.Quantity (int)
└── FSM instances (FSMState objects)

OUTPUT DEPENDENCIES:
├── Hydrated FSM states
├── Order-to-FSM bindings
└── Initialized state machines

SHARED STATE:
├── SIMA lifecycle state (read-only)
├── Strategy configuration (read-only)
└── FSM registry (write access)
```

### Control Flow Dependencies

```
PRECONDITIONS:
├── WorkingOrders collection initialized
├── FSM registry available
└── Strategy configuration loaded

POSTCONDITIONS:
├── All valid orders have FSM bindings
├── Invalid orders logged/skipped
└── FSM states initialized correctly

INVARIANTS:
├── No lock() blocks (V12 DNA)
├── ASCII-only strings (V12 DNA)
└── Atomic state transitions (V12 DNA)
```

---

## Extraction Sequence

### Phase A: Preparation (Pre-Extraction)

**Step A1: Baseline Verification**
```bash
# Verify current complexity
python scripts/complexity_audit.py src/V12_002.SIMA.Lifecycle.cs

# Run existing tests
dotnet test tests/V12_Performance.Tests/ --filter "SIMA"

# Capture baseline metrics
powershell -File .\scripts\build_readiness.ps1
```

**Step A2: Code Analysis**
```bash
# Identify extraction boundaries
grep -A 50 "HydrateFSMsFromWorkingOrders" src/V12_002.SIMA.Lifecycle.cs

# Map dependencies
graphify update .
graphify query "dependencies of HydrateFSMsFromWorkingOrders"
```

### Phase B: Extraction (Surgical Changes)

**Step B1: Extract Validation Method**
```csharp
// Extract order validation logic
// Target complexity: ≤5
// Lines: ~10-15

private bool ValidateWorkingOrderState(Order order)
{
    if (order == null)
    {
        return false;
    }
    
    if (order.State != OrderState.Working)
    {
        return false;
    }
    
    if (order.Type == OrderType.Unknown)
    {
        return false;
    }
    
    return true;
}
```

**Step B2: Extract Initialization Method**
```csharp
// Extract FSM initialization logic
// Target complexity: ≤5
// Lines: ~15-20

private FSMState InitializeFSMState(Order order)
{
    var fsm = new FSMState();
    
    fsm.Initialize(order.Instrument);
    fsm.SetState(FSMStateType.Idle);
    fsm.SetOrderReference(order);
    
    return fsm;
}
```

**Step B3: Refactor Main Method**
```csharp
// Simplify main orchestration
// Target complexity: ≤8
// Lines: ~20-30

private void HydrateFSMsFromWorkingOrders()
{
    foreach (var order in WorkingOrders)
    {
        if (!ValidateWorkingOrderState(order))
        {
            LogSkippedOrder(order);
            continue;
        }
        
        var fsm = InitializeFSMState(order);
        BindFSMToOrder(order, fsm);
    }
}
```

### Phase C: Verification (Post-Extraction)

**Step C1: Complexity Verification**
```bash
# Verify complexity reduction
python scripts/complexity_audit.py src/V12_002.SIMA.Lifecycle.cs

# Expected results:
# - HydrateFSMsFromWorkingOrders: ≤8
# - ValidateWorkingOrderState: ≤5
# - InitializeFSMState: ≤5
```

**Step C2: Functional Verification**
```bash
# Run all tests
dotnet test tests/V12_Performance.Tests/

# Run stress tests
powershell -File .\scripts\test_stress.ps1

# Verify FSM hydration
# Manual test: F5 in NinjaTrader
```

**Step C3: DNA Compliance Verification**
```bash
# Check for lock() blocks (must be zero)
grep -r "lock(" src/V12_002.SIMA.Lifecycle.cs

# Check ASCII compliance
python check_ascii.py src/V12_002.SIMA.Lifecycle.cs

# Verify formatting
dotnet csharpier check src/
```

---

## Jane Street Compliance Checks

### Cognitive Simplicity (Primary Goal)

**Current State**:
- Complexity: 14 (within threshold)
- Cognitive load: MEDIUM
- Branching paths: ~8-10

**Target State** (if extraction triggered):
- Main method complexity: ≤8
- Extracted method complexity: ≤5 each
- Cognitive load: LOW
- Branching paths: ≤4 per method

### Correctness by Construction

**Validation Strategy**:
```csharp
// Make illegal states unrepresentable
// Use type system to enforce constraints

// BEFORE (runtime validation):
if (order.State != OrderState.Working)
{
    return; // Runtime check
}

// AFTER (type-level guarantee):
private bool ValidateWorkingOrderState(WorkingOrder order)
{
    // Type system guarantees order.State == Working
    // Compiler enforces correctness
    return order.IsValid();
}
```

**Type Safety Enhancements**:
- Use `WorkingOrder` type instead of generic `Order`
- Enforce state constraints at compile time
- Eliminate runtime validation where possible

### Lock-Free Pattern Compliance

**Current State**: ✅ COMPLIANT
- No `lock()` blocks detected
- Uses FSM/Actor Enqueue model
- Atomic state transitions

**Post-Extraction State**: ✅ MAINTAIN COMPLIANCE
- Preserve lock-free pattern
- No new synchronization primitives
- Maintain Actor model semantics

### ASCII-Only Compliance

**Current State**: ✅ COMPLIANT
- No Unicode characters detected
- No emoji in strings
- No curly quotes

**Post-Extraction State**: ✅ MAINTAIN COMPLIANCE
- Verify all new strings are ASCII-only
- Run `check_ascii.py` before commit
- Enforce in code review

---

## Risk Mitigation Strategies

### Risk 1: Initialization Timing

**Risk Level**: HIGH
**Impact**: FSM state corruption if initialization order changes

**Mitigation**:
1. **Preserve Execution Order**
   - Extract methods must maintain exact sequence
   - No reordering of initialization steps
   - Document execution dependencies

2. **Add Timing Tests**
   ```csharp
   [Test]
   public void HydrateFSMs_PreservesInitializationOrder()
   {
       // Verify FSM initialization happens before binding
       // Verify order validation happens before initialization
   }
   ```

3. **Checkpoint Before Changes**
   - Use Bob CLI checkpointing
   - Enable rollback if timing breaks
   - Test in isolated branch

### Risk 2: State Consistency

**Risk Level**: MEDIUM
**Impact**: Order state validation gaps could allow invalid FSM states

**Mitigation**:
1. **Comprehensive Validation Tests**
   ```csharp
   [Test]
   public void ValidateWorkingOrderState_RejectsInvalidStates()
   {
       // Test all invalid state combinations
       // Verify no false positives
   }
   ```

2. **Invariant Checks**
   - Add runtime assertions (debug builds)
   - Verify FSM state consistency
   - Log validation failures

3. **Type-Level Guarantees**
   - Use `WorkingOrder` type for valid orders
   - Compiler enforces state constraints
   - Eliminate runtime validation where possible

### Risk 3: Performance Overhead

**Risk Level**: LOW
**Impact**: Additional method call overhead (negligible for startup code)

**Mitigation**:
1. **Inline Candidates**
   - Mark extracted methods as `[MethodImpl(MethodImplOptions.AggressiveInlining)]`
   - Let JIT compiler optimize hot paths
   - Measure performance impact

2. **Benchmark Tests**
   ```csharp
   [Benchmark]
   public void BenchmarkFSMHydration()
   {
       // Measure hydration time before/after extraction
       // Verify no significant regression
   }
   ```

3. **Profile in Production**
   - Monitor FSM hydration time
   - Alert if latency increases
   - Rollback if performance degrades

### Risk 4: Scope Creep

**Risk Level**: MEDIUM
**Impact**: Refactoring beyond single method violates V12.23 Protocol

**Mitigation**:
1. **Strict Boundary Enforcement**
   - Extract ONLY from HydrateFSMsFromWorkingOrders
   - Do NOT refactor callers or callees
   - Do NOT modify data structures

2. **Scope Validation Checklist**
   - [ ] Only one method modified (main method)
   - [ ] Only two methods added (extracted helpers)
   - [ ] No changes to method signatures
   - [ ] No changes to data structures
   - [ ] No changes to callers/callees

3. **Code Review Gate**
   - Verify scope compliance before merge
   - Reject PRs that exceed single-method scope
   - Document scope violations

---

## Implementation Checklist

### Pre-Extraction
- [ ] Verify current complexity (14)
- [ ] Run baseline tests (all pass)
- [ ] Capture baseline metrics
- [ ] Create feature branch
- [ ] Enable Bob CLI checkpointing

### Extraction Phase
- [ ] Extract ValidateWorkingOrderState()
- [ ] Extract InitializeFSMState()
- [ ] Refactor main method
- [ ] Verify complexity ≤8 (main), ≤5 (extracted)
- [ ] Run CSharpier formatter

### Verification Phase
- [ ] All tests pass
- [ ] Complexity audit passes
- [ ] No lock() blocks detected
- [ ] ASCII-only compliance verified
- [ ] Stress tests pass
- [ ] Manual F5 test in NinjaTrader

### Deployment Phase
- [ ] Run pre-push validation (full mode)
- [ ] Update manifest.json
- [ ] Create PR with PHS loop
- [ ] Merge after approval
- [ ] Run deploy-sync.ps1
- [ ] Verify BUILD_TAG in NinjaTrader

---

## Success Metrics

### Complexity Metrics
- **Main Method**: 14 → ≤8 (43% reduction)
- **Extracted Methods**: ≤5 each
- **Total Complexity Budget**: ≤15 (maintained)

### Quality Metrics
- **Test Coverage**: 100% (all tests pass)
- **Code Health Score**: Improve by 1-2 points (CodeScene)
- **Codacy Grade**: Maintain B or improve to A

### Performance Metrics
- **FSM Hydration Time**: No regression (≤5% variance)
- **Startup Latency**: No regression (≤10ms variance)
- **Memory Allocation**: No increase

### V12 DNA Compliance
- **Lock-Free**: ✅ Zero lock() blocks
- **ASCII-Only**: ✅ Zero non-ASCII characters
- **Correctness**: ✅ Type-level guarantees
- **FSM/Actor**: ✅ Enqueue model preserved

---

## Conclusion

### Current Status: HOLD

This architecture plan is **HYPOTHETICAL** and should be executed ONLY if:
1. Future code changes push complexity >15
2. New requirements add branching logic
3. Method exceeds Jane Street threshold

### Trigger Condition

Monitor `HydrateFSMsFromWorkingOrders` complexity in future changes. If complexity exceeds 15, execute this architecture plan.

### Next Steps

1. **Monitor**: Track complexity in code reviews
2. **Alert**: Flag if complexity approaches 15
3. **Execute**: Trigger Phase 3 (Implementation) if threshold exceeded

### V12.23 Protocol Compliance

- ✅ Single method scope defined
- ✅ No scope creep (method-only boundary)
- ✅ Complexity target ≤15 maintained
- ✅ Extraction strategy documented
- ✅ Risk mitigation strategies defined
- ✅ Jane Street compliance verified

---

**Phase 2 Status**: COMPLETED (Hypothetical Plan)
**Recommendation**: HOLD - No extraction needed at this time
**Next Phase**: HOLD (trigger Phase 3 only if complexity >15)
