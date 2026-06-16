# Phase 1.0: Scope Definition - EPIC-CCN-060

## Extraction Scope (SINGLE METHOD ONLY)

**Target Method**:
- **Method Name**: SweepTrackedOrders
- **File**: src/V12_002.SIMA.Lifecycle.cs
- **Current Complexity**: 12 (Cyclomatic Complexity)
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **Extraction Strategy**: Break into 2-3 helper methods

## Boundary Definition

### IN SCOPE
- **SweepTrackedOrders method body ONLY**
- Refactor internal logic to reduce branching
- Extract conditional blocks into focused helper methods
- Maintain lock-free Actor/FSM pattern

### OUT OF SCOPE
- ❌ Callers of SweepTrackedOrders
- ❌ Callees invoked by SweepTrackedOrders
- ❌ Other methods in V12_002.SIMA.Lifecycle.cs
- ❌ Pre-existing compilation errors
- ❌ "While we're here" improvements
- ❌ Bundling multiple concerns

### NO SCOPE CREEP
**ONE EPIC = ONE CONCERN**
- This EPIC addresses ONLY the complexity of SweepTrackedOrders
- No architectural changes beyond method extraction
- No behavior modifications

## Success Criteria

### Functional Requirements
1. ✅ Complexity reduced from 12 to ≤8
2. ✅ All existing tests pass (100% pass rate)
3. ✅ No behavior changes (bit-for-bit identical output)
4. ✅ Lock-free Actor/FSM pattern maintained

### Quality Gates
1. ✅ CSharpier formatting check passes
2. ✅ Build succeeds with zero errors
3. ✅ Lint audit passes (zero violations)
4. ✅ Pre-push validation passes (all 13 checks)

### V12 DNA Compliance
1. ✅ ASCII-only strings (no Unicode/emoji)
2. ✅ No `lock()` statements introduced
3. ✅ Atomic state transitions preserved
4. ✅ "Make illegal states unrepresentable" principle maintained

## Extraction Strategy

### Approach
1. **Identify branching logic** in SweepTrackedOrders
2. **Extract conditional blocks** into focused helper methods
3. **Preserve call semantics** (parameters, return values)
4. **Maintain Actor/FSM pattern** (no locks, atomic operations)

### Expected Outcome
- **Before**: 1 method with CYC=12
- **After**: 1 orchestrator method (CYC≤8) + 2-3 helper methods (CYC≤5 each)

## Risk Mitigation
- **Checkpointing**: Enabled via Bob CLI
- **Rollback**: Restore points at each extraction step
- **Verification**: Run tests after each helper method extraction
- **Blast Radius**: Limited to single method (no caller/callee changes)

## Jane Street Alignment
- **Cognitive Simplicity**: CYC≤8 ensures functions are easy to reason about
- **Microsecond Latency**: Simpler logic = faster execution
- **Testability**: Smaller methods = exhaustive test coverage
- **Auditability**: Reduced branching = easier race condition detection
