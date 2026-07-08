# Phase 1: Scope Definition - EPIC-CCN-109

## Target Method
- **Method**: `HydrateWorkingOrdersFromBroker`
- **File**: `src/V12_002.SIMA.Lifecycle.cs`
- **Current Complexity**: 19
- **Target Complexity**: ≤ 15
- **Epic ID**: EPIC-CCN-109

## Extraction Scope

### What to Extract
Based on hotspot analysis, extract THREE helper methods:

1. **ValidateBrokerOrder()** - Order validation logic
   - Input: Broker order object
   - Output: ValidationResult (bool + error message)
   - Complexity Reduction: ~4 branches

2. **MergeOrderState()** - State synchronization logic
   - Input: Existing order, broker order
   - Output: Merged order state
   - Complexity Reduction: ~5 branches

3. **HandleHydrationError()** - Error handling logic
   - Input: Exception, order context
   - Output: void (logs + state cleanup)
   - Complexity Reduction: ~3 branches

### What to Keep
- Main orchestration flow in HydrateWorkingOrdersFromBroker
- High-level broker API calls
- Order collection iteration logic
- Critical state transitions

## Boundary Definition

### Single Method Constraint (V12.23 No Scope Creep Protocol)
- **ONLY** refactor `HydrateWorkingOrdersFromBroker`
- **DO NOT** touch callers: OnConnectionStatusUpdate, OnOrderUpdate
- **DO NOT** modify broker API wrappers
- **DO NOT** change order management methods

### Extraction Strategy
- Extract pure logic blocks (no state mutation in helpers)
- Maintain existing method signature
- Preserve all existing behavior (zero functional changes)
- Keep extracted methods private to SIMA class

## Success Criteria

### Primary Goal
- [ ] Reduce HydrateWorkingOrdersFromBroker complexity from 19 to ≤ 15

### Quality Gates
- [ ] All existing tests pass (zero regressions)
- [ ] No new compiler warnings
- [ ] ASCII-only compliance maintained
- [ ] Lock-free pattern preserved (no new locks)
- [ ] CSharpier formatting passes

### Verification
- [ ] Complexity audit shows ≤ 15 for target method
- [ ] Build succeeds with zero errors
- [ ] deploy-sync.ps1 completes successfully
- [ ] NinjaTrader F5 test passes

## Risk Assessment

### Overall Risk: HIGH
**Rationale**: Central method in order lifecycle with multiple dependencies

### Specific Risks

1. **State Synchronization Risk** (HIGH)
   - Impact: Order state corruption
   - Mitigation: Preserve exact state mutation sequence
   - Test: Verify order state consistency after extraction

2. **Broker Communication Risk** (MEDIUM)
   - Impact: Failed order hydration
   - Mitigation: Keep broker API calls in main method
   - Test: Verify broker connection handling

3. **Performance Risk** (LOW)
   - Impact: Additional method call overhead
   - Mitigation: Inline-friendly extraction (JIT optimization)
   - Test: Benchmark before/after extraction

4. **Regression Risk** (MEDIUM)
   - Impact: Broken order tracking
   - Mitigation: Comprehensive test coverage before extraction
   - Test: Run full test suite + manual NinjaTrader test

### Risk Mitigation Strategy
- Create checkpoint before extraction (Bob CLI auto-checkpoint)
- Extract one method at a time with verification
- Run tests after each extraction
- Keep original method structure visible in git history

## Extraction Order

1. **First**: ValidateBrokerOrder (lowest risk, pure validation)
2. **Second**: HandleHydrationError (isolated error handling)
3. **Third**: MergeOrderState (highest risk, state mutation)

## Dependencies

### Required Before Extraction
- [ ] Test coverage verification (FSMActorTests.cs exists)
- [ ] Dependency mapping complete (from Phase 0)
- [ ] Backup checkpoint created

### Required After Extraction
- [ ] Complexity audit confirms ≤ 15
- [ ] All tests pass
- [ ] deploy-sync.ps1 succeeds
- [ ] Manual NinjaTrader verification

## Metadata
- **Phase**: 1 (Scope Definition)
- **Status**: Completed
- **Created**: 2026-06-13
- **Next Phase**: Phase 2 (Architecture Planning)
- **Estimated Complexity Reduction**: 19 → 10-12 (target ≤ 15)
