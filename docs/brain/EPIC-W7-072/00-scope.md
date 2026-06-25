# Phase 1: Scope Definition - EPIC-W7-072

**Agent**: v12-phase1-scope
**Target Method**: ProcessAccountOrder_UpdateMasterExpected
**File**: V12_002.Orders.Callbacks.AccountOrders.cs
**Current Complexity**: 12
**Target Complexity**: ≤8

## Scope Boundary Definition

### IN SCOPE

**Primary Target**:
- Method: `ProcessAccountOrder_UpdateMasterExpected`
- Current CYC: 12
- Target CYC: ≤6 (main method after extraction)

**Extraction Candidates**:
1. **Order Validation Logic** (CYC ~3)
   - Order state validation checks
   - Null/validity guards
   - Pre-condition verification

2. **Master Order Update Logic** (CYC ~4)
   - Master order state synchronization
   - Order property updates
   - FSM state updates

3. **Error Handling Consolidation** (CYC ~2)
   - Error path simplification
   - Logging consolidation
   - Exception handling

**Affected Components**:
- V12_002.Orders.Callbacks.AccountOrders.cs (primary file)
- Master order state management interfaces
- FSM order tracking system

### OUT OF SCOPE

**Explicitly Excluded**:
- Other methods in V12_002.Orders.Callbacks.AccountOrders.cs (unless directly called)
- Broader order callback system refactoring
- FSM architecture changes
- Master order data model changes
- Account order callback registration logic
- Order execution engine modifications

**Deferred to Future Epics**:
- Performance optimization of order callbacks
- Comprehensive order callback system audit
- Cross-module order state synchronization improvements

## Extraction Strategy

### Proposed Method Decomposition

**Main Method** (Target CYC ≤6):
```
ProcessAccountOrder_UpdateMasterExpected(order)
├─ ValidateOrderForMasterUpdate(order) → bool [CYC ≤3]
├─ UpdateMasterOrderState(order) → void [CYC ≤4]
└─ HandleUpdateError(error) → void [CYC ≤2]
```

### Complexity Reduction Path

**Before**:
- ProcessAccountOrder_UpdateMasterExpected: CYC 12

**After**:
- ProcessAccountOrder_UpdateMasterExpected: CYC 6
- ValidateOrderForMasterUpdate: CYC 3
- UpdateMasterOrderState: CYC 4
- HandleUpdateError: CYC 2

**Total Reduction**: 12 → 6 (50% reduction in main method)

## Risk Assessment

**Blast Radius**: MEDIUM
- Isolated to account order callback processing
- Well-defined interface boundaries
- Limited cross-module dependencies

**Testing Requirements**:
- Unit tests for each extracted method
- Integration tests for callback flow
- Regression tests for master order updates

**Rollback Strategy**:
- Git revert if compilation fails
- Preserve original method signature
- Maintain backward compatibility

## Success Criteria

**Functional**:
- ✅ All extracted methods have CYC ≤8
- ✅ Main method reduced to CYC ≤6
- ✅ No behavioral changes (pure refactoring)
- ✅ All existing tests pass

**Technical**:
- ✅ Build passes without errors
- ✅ deploy-sync.ps1 executes successfully
- ✅ F5 in NinjaTrader loads strategy
- ✅ No new compiler warnings

**Quality**:
- ✅ Each extracted method has single responsibility
- ✅ Method names clearly describe purpose
- ✅ No code duplication introduced
- ✅ Improved testability

## Boundary Validation

**Scope Creep Prevention**:
- ❌ Do NOT refactor adjacent methods unless directly called
- ❌ Do NOT modify order callback registration
- ❌ Do NOT change FSM architecture
- ❌ Do NOT alter master order data model

**Approved Scope Expansion** (if discovered during Phase 2):
- ✅ Extract additional helper if main method still >8
- ✅ Consolidate duplicate validation logic within target method
- ✅ Simplify conditional branches with early returns

## Jane Street Alignment

**Principles Applied**:
- **Cognitive Simplicity**: Each method does one thing well
- **Exhaustive Testability**: Small methods = complete test coverage
- **Race Condition Auditability**: Clear state mutation boundaries
- **Correctness by Construction**: Validation logic isolated and verifiable

**HFT Considerations**:
- Maintain microsecond-latency performance
- No additional allocations in hot path
- Preserve lock-free callback semantics

## Next Steps

Proceed to Phase 1.5 (Scope Boundary Validation) to verify:
1. No scope creep beyond defined boundaries
2. Extraction strategy aligns with Jane Street principles
3. Risk assessment is accurate
4. Success criteria are measurable

---

**Scope Definition Date**: 2026-06-24
**Bobcoins Used**: 0 (Sequential Thinking MCP used)
**Execution Time**: <2 minutes
