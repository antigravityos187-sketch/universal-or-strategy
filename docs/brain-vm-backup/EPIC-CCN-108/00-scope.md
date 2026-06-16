# Phase 1: Scope Definition - EPIC-CCN-108

## Target Method
- **Method**: `SweepBrokerOrders()`
- **File**: `src/V12_002.SIMA.Lifecycle.cs`
- **Current Complexity**: 24 CCN
- **Target Complexity**: ≤ 15 CCN
- **Overage**: +9 (60% over threshold)

## Extraction Strategy

### What to Extract

Based on Phase 0 hotspot analysis, extract **4 focused methods**:

1. **ValidateOrderState()** (Target CCN: ~5)
   - Extract order validation logic
   - Guard clauses for invalid states
   - Pre-condition checks

2. **SyncBrokerOrderState()** (Target CCN: ~4)
   - Extract state synchronization logic
   - Atomic state updates
   - FSM/Actor enqueue operations

3. **ProcessOrderTransition()** (Target CCN: ~6)
   - Extract state machine transition logic
   - Order lifecycle state changes
   - Event emission

4. **LogOrderEvent()** (Target CCN: ~2)
   - Extract logging/telemetry
   - Diagnostic output
   - Performance metrics

### What to Keep in Main Method

The main `SweepBrokerOrders()` method will retain:
- High-level orchestration logic
- Loop structure over broker orders
- Calls to extracted methods
- Exception handling boundary

**Target Post-Refactoring CCN**: 8-10 (main method)

## Boundary Definition

### Single Method Scope (V12.23 No Scope Creep Protocol)

**IN SCOPE**:
- ✅ Refactor `SweepBrokerOrders()` method only
- ✅ Extract 4 private helper methods within same class
- ✅ Add unit tests for extracted methods
- ✅ Preserve lock-free guarantees
- ✅ Maintain ASCII-only compliance

**OUT OF SCOPE**:
- ❌ Refactoring caller methods (OnBarUpdate, etc.)
- ❌ Modifying order callback handlers
- ❌ Changing FSM/Actor infrastructure
- ❌ Altering other SIMA lifecycle methods
- ❌ Touching files outside `V12_002.SIMA.Lifecycle.cs`

### Scope Enforcement

- **File Limit**: 1 file (`src/V12_002.SIMA.Lifecycle.cs`)
- **Method Limit**: 1 method refactored + 4 methods extracted
- **Test File**: 1 new test file (`tests/V12_Performance.Tests/SIMA/SweepBrokerOrdersTests.cs`)
- **PR Diff Target**: < 500 lines (surgical extraction)

## Success Criteria

### Primary Goals

1. **Complexity Reduction**: `SweepBrokerOrders()` CCN ≤ 15
2. **Extracted Methods**: All 4 extracted methods CCN ≤ 10
3. **Test Coverage**: 100% coverage for extracted methods
4. **Lock-Free Guarantee**: Zero `lock()` blocks introduced
5. **ASCII-Only**: Zero Unicode violations

### Verification Checklist

- [ ] `complexity_audit.py` shows CCN ≤ 15 for `SweepBrokerOrders()`
- [ ] All extracted methods have CCN ≤ 10
- [ ] Unit tests pass (100% green)
- [ ] `grep -r "lock(" src/V12_002.SIMA.Lifecycle.cs` returns zero matches
- [ ] `check_ascii.py` returns zero violations
- [ ] `dotnet build` succeeds with zero errors
- [ ] `deploy-sync.ps1` completes without errors
- [ ] F5 in NinjaTrader loads strategy successfully

### Quality Gates

- **Build**: Must pass `powershell -File .\scripts\build_readiness.ps1`
- **Lint**: Must pass `powershell -File .\scripts\lint.ps1`
- **Format**: Must pass `dotnet csharpier check src/`
- **Pre-Push**: Must pass `powershell -File .\scripts\pre_push_validation.ps1 -Fast`

## Risk Assessment

### Overall Risk: MEDIUM-HIGH

**Risk Factors**:
1. **Criticality**: Core order lifecycle method (HIGH)
2. **Blast Radius**: Affects order management subsystem (MEDIUM)
3. **Complexity**: 24 CCN requires careful extraction (HIGH)
4. **Testing Gap**: No existing unit tests (HIGH)
5. **Lock-Free**: Must preserve atomic guarantees (HIGH)

### Mitigation Strategies

1. **TDD Approach**: Write tests before extraction
2. **Incremental Extraction**: Extract one method at a time
3. **Checkpoint After Each**: Use Bob CLI checkpointing
4. **Verify After Each**: Run full test suite after each extraction
5. **Rollback Plan**: Use `restore` tool if issues arise

### Risk Reduction Tactics

- **Pre-Extraction**: Capture baseline behavior with integration test
- **During Extraction**: Verify each extracted method independently
- **Post-Extraction**: Run stress test (`test_stress.ps1`)
- **Final Verification**: F5 in NinjaTrader with live data

## Extraction Order

**Recommended Sequence** (lowest risk first):

1. **LogOrderEvent()** - Lowest risk, pure side-effect
2. **ValidateOrderState()** - Pure function, easy to test
3. **SyncBrokerOrderState()** - Moderate risk, atomic operations
4. **ProcessOrderTransition()** - Highest risk, state machine logic

## V12 DNA Compliance

### Architectural Mandates

- ✅ **Correctness by Construction**: Use guard clauses, eliminate invalid states
- ✅ **Lock-Free Actor Pattern**: Preserve FSM/Actor enqueue model
- ✅ **ASCII-Only**: No Unicode in extracted methods
- ✅ **Jane Street Alignment**: Target CCN ≤ 15 (cognitive simplicity)

### Post-Refactoring Validation

- **Complexity**: All methods CCN ≤ 15
- **Testability**: 100% unit test coverage
- **Lock-Free**: Zero `lock()` blocks
- **ASCII-Only**: Zero Unicode violations

## Next Phase

**Phase 2**: Implementation Plan
- Detailed extraction plan for each method
- Test strategy and test cases
- Mermaid diagrams for method interactions
- Step-by-step implementation guide

## Metadata
- **Epic**: EPIC-CCN-108
- **Phase**: 1 (Scope Definition)
- **Status**: COMPLETED
- **Created**: 2026-06-13
- **Author**: Bob Shell (v12-engineer)
- **Scope**: Single method (`SweepBrokerOrders`)
- **Target CCN**: ≤ 15
