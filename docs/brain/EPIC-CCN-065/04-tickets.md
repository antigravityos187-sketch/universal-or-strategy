# Extraction Tickets: EPIC-CCN-065

## Overview
- **Total Tickets**: 1
- **Execution Order**: Single atomic extraction
- **Estimated Effort**: 2-3 hours
- **Target Method**: HandleFsmFilled
- **Current CYC**: 13
- **Target CYC**: ≤8 per method

---

## TICKET-1: Extract HandleFsmFilled Helper Methods

### Scope
- **Current Method**: `HandleFsmFilled`
- **Current CYC**: 13
- **Target CYC**: ≤8 (Jane Street strict standard)
- **Extraction**: Split into 3 focused helper methods

### Method Breakdown

#### 1. IsStopOrTargetOrder (Classifier)
- **Purpose**: Encapsulate complex string prefix logic
- **Signature**: `private bool IsStopOrTargetOrder(string signalName, out bool isStop, out bool isTarget)`
- **Target CYC**: 6-7
- **Responsibility**: Order type classification via string prefix checks

#### 2. UpdateBracketStateForFill (State Mutator)
- **Purpose**: Handle contract decrement and state transition logic
- **Signature**: `private void UpdateBracketStateForFill(FollowerBracketFSM fsm, int filledQty)`
- **Target CYC**: 2-3
- **Responsibility**: Update RemainingContracts and transition Filled/Active state

#### 3. TransitionToActiveIfEntry (State Mutator)
- **Purpose**: Handle entry-specific state transition
- **Signature**: `private void TransitionToActiveIfEntry(FollowerBracketFSM fsm)`
- **Target CYC**: 2
- **Responsibility**: Entry fill state transition logic

### Implementation Steps

1. **Create IsStopOrTargetOrder Method**
   - Add method below HandleFsmFilled
   - Extract string null checks and StartsWith logic
   - Use out parameters for isStop and isTarget flags
   - Return combined boolean result
   - Add XML documentation

2. **Create UpdateBracketStateForFill Method**
   - Add method below IsStopOrTargetOrder
   - Extract contract decrement logic (Math.Max guard)
   - Extract Filled vs Active state decision (ternary operator)
   - Add XML documentation

3. **Create TransitionToActiveIfEntry Method**
   - Add method below UpdateBracketStateForFill
   - Extract entry state comparison and transition
   - Add XML documentation

4. **Refactor HandleFsmFilled**
   - Replace inline string checks with IsStopOrTargetOrder call
   - Replace contract/state update with UpdateBracketStateForFill call
   - Replace entry transition with TransitionToActiveIfEntry call
   - Verify orchestrator complexity ≤8

5. **Add Unit Tests**
   - Test IsStopOrTargetOrder: stop/target/entry/null cases
   - Test UpdateBracketStateForFill: contract decrement edge cases
   - Test TransitionToActiveIfEntry: state transition logic
   - Verify existing integration tests pass

### Acceptance Criteria

- [ ] HandleFsmFilled complexity reduced to ≤8 CYC
- [ ] IsStopOrTargetOrder complexity ≤8 CYC
- [ ] UpdateBracketStateForFill complexity ≤8 CYC
- [ ] TransitionToActiveIfEntry complexity ≤8 CYC
- [ ] All helper methods have XML documentation
- [ ] Zero behavioral changes (pure refactoring)
- [ ] All existing tests pass (100% pass rate)
- [ ] Build succeeds (dotnet build)
- [ ] Pre-push validation passes (all 13 checks)
- [ ] Lock-free Actor pattern maintained
- [ ] ASCII-only compliance maintained
- [ ] PR diff <10,000 characters

### V12 DNA Compliance Checklist

- [ ] ✅ Lock-Free Actor Pattern: No lock() statements added
- [ ] ✅ ASCII-Only: No Unicode in string literals
- [ ] ✅ Correctness by Construction: Clear method contracts
- [ ] ✅ Jane Street Alignment: All methods CYC ≤8
- [ ] ✅ Hard-Link Integrity: Run deploy-sync.ps1 after changes

### Testing Strategy

**Unit Tests** (New):
```csharp
[Test]
public void IsStopOrTargetOrder_StopOrder_ReturnsTrue()
[Test]
public void IsStopOrTargetOrder_TargetOrder_ReturnsTrue()
[Test]
public void IsStopOrTargetOrder_EntryOrder_ReturnsFalse()
[Test]
public void IsStopOrTargetOrder_NullSignal_ReturnsFalse()
[Test]
public void UpdateBracketStateForFill_FullFill_TransitionsToFilled()
[Test]
public void UpdateBracketStateForFill_PartialFill_TransitionsToActive()
[Test]
public void TransitionToActiveIfEntry_EntryState_TransitionsToActive()
```

**Integration Tests** (Existing):
- Verify all existing FSM tests pass unchanged
- No new test failures introduced

### Verification Commands

```powershell
# Step 1: Build
dotnet build

# Step 2: Run tests
dotnet test

# Step 3: Complexity audit
python3 scripts/complexity_audit.py

# Step 4: Pre-push validation (fast mode)
powershell -File .\scripts\pre_push_validation.ps1 -Fast

# Step 5: Hard-link sync
powershell -File .\deploy-sync.ps1
```

### Dependencies
- None (first and only ticket)

### Rollback Plan
- Restore from checkpoint via Bob CLI `/restore` command
- Revert commit if merged: `git revert <commit-sha>`
- Emergency rollback: restore from git history

---

## Execution Notes

### Lock-Free Validation
- ✅ All methods execute within FSM/Actor Enqueue queue
- ✅ Single-threaded access guaranteed by Actor pattern
- ✅ No atomic primitives needed
- ✅ No race conditions possible
- ✅ No deadlocks (no locks to deadlock on)

### Performance Impact
- **NEGLIGIBLE**: Method calls inlined by JIT compiler
- **Zero allocations**: No new objects created
- **Same execution path**: Just reorganized
- **Actor overhead**: Already present, unchanged

### Jane Street Principles Applied
1. **Cognitive Simplicity**: Each method has single, clear purpose
2. **Testability**: Helpers can be unit tested independently
3. **Incremental Improvement**: Small, focused change (no wholesale rewrite)
4. **Microsecond-Latency Safe**: No performance degradation

---

## Success Metrics

### Complexity Reduction
- Before: HandleFsmFilled = 13 CYC
- After: 
  - HandleFsmFilled = 3-4 CYC ✅
  - IsStopOrTargetOrder = 6-7 CYC ✅
  - UpdateBracketStateForFill = 2-3 CYC ✅
  - TransitionToActiveIfEntry = 2 CYC ✅

### Quality Gates
- ✅ Build: Zero errors
- ✅ Tests: 100% pass rate
- ✅ Lint: Zero violations
- ✅ Formatting: CSharpier compliant
- ✅ Security: Zero secrets detected
- ✅ Complexity: All methods ≤8 CYC
- ✅ PR Hygiene: Diff <10k characters

---

## References

- Architecture Plan: `docs/brain/EPIC-CCN-065/02-architecture-plan.md`
- Scope Definition: `docs/brain/EPIC-CCN-065/01-scope.md`
- Boundary Validation: `docs/brain/EPIC-CCN-065/01-scope-boundary.md`
- Hotspot Analysis: `docs/brain/EPIC-CCN-065/00-hotspots.md`
- V12 DNA Mandates: `AGENTS.md` (Section 2)
- Jane Street Standards: `AGENTS.md` (Section 3.5)
- Phase 6 Protocol: `AGENTS.md` (Section 7)
