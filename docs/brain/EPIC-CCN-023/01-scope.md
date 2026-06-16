# Phase 1.0: Scope Definition - EPIC-CCN-023

## Extraction Scope (SINGLE METHOD ONLY)

### Target Method
- **Method Name**: HandleFlatPosition_CleanupActivePositions
- **File**: src/V12_002.Orders.Callbacks.Execution.cs
- **Current Complexity**: 17 (CYC)
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **Extraction Strategy**: Break into 2-3 helper methods

### Complexity Reduction Plan

**Current State**:
- Cyclomatic Complexity: 17
- Overage: +2 above V12 threshold (15)
- Risk Level: MEDIUM-HIGH (execution path critical)

**Target State**:
- Primary Method CYC: ≤8
- Helper Methods CYC: ≤5 each
- Total Reduction: 9 points (53% reduction)

### Extraction Strategy

**Approach**: Surgical method decomposition using guard clauses and single-purpose helpers

**Expected Extractions** (2-3 helper methods):
1. **Position State Validation** (CYC ≤5)
   - Extract early-return guard clauses
   - Consolidate position state checks
   - Return boolean or Result<T> for validation outcome

2. **Active Position Cleanup Logic** (CYC ≤5)
   - Extract core cleanup operations
   - Isolate collection/dictionary mutations
   - Ensure lock-free Actor/FSM pattern compliance

3. **Error Handling & Logging** (CYC ≤3, if needed)
   - Extract error path logic
   - Consolidate logging statements
   - Maintain V12 telemetry standards

### Boundary Definition

**IN SCOPE** (ONLY):
- Method body of HandleFlatPosition_CleanupActivePositions
- Internal logic refactoring
- Helper method extraction within same file
- Unit test additions for extracted methods

**OUT OF SCOPE** (STRICTLY FORBIDDEN):
- Callers of HandleFlatPosition_CleanupActivePositions
- Callees invoked by this method (no signature changes)
- Other methods in V12_002.Orders.Callbacks.Execution.cs
- Related files in order execution subsystem
- Pre-existing compilation errors elsewhere
- "While we're here" improvements
- Scope creep of any kind

### Success Criteria

**Functional Requirements**:
- Complexity reduced from 17 to ≤8
- All existing tests pass (100% pass rate)
- No behavior changes (semantic equivalence)
- Lock-free Actor/FSM pattern maintained
- ASCII-only compliance verified

**Quality Gates**:
- CSharpier formatting passes
- Roslyn analyzer clean (zero violations)
- Pre-push validation passes (all 13 checks)
- Codacy shows "Up to quality standards"
- No new technical debt introduced

**Testing Requirements**:
- TDD tests added for extracted helper methods
- Regression tests pass for order execution flow
- Integration tests verify position cleanup correctness
- Stress tests confirm no performance degradation

### V12 DNA Compliance

**Mandatory Checks**:
- No lock() statements in method or helpers
- Atomic state mutations (Interlocked/FSM pattern)
- ASCII-only string literals (no Unicode/emoji)
- Error handling uses Result<T> pattern
- Logging follows V12 standards
- Hard-link integrity maintained (deploy-sync.ps1)

### Risk Mitigation

**High-Risk Factors**:
- Execution path critical for trade correctness
- Cleanup logic historically bug-prone
- Position management requires microsecond latency

**Mitigation Strategy**:
1. TDD coverage BEFORE refactoring
2. Incremental extraction (one helper at a time)
3. Checkpoint after each extraction
4. Full regression suite after each commit
5. Manual F5 test in NinjaTrader

### Estimated Effort

- **Complexity**: LOW-MEDIUM (single method, +2 overage)
- **Duration**: 2-4 hours (including TDD setup)
- **Risk**: MEDIUM-HIGH (critical execution path)
- **Priority**: P3 (complexity debt reduction)

---

**Epic**: EPIC-CCN-023
**Phase**: 1.0 (Scope Definition)
**Status**: APPROVED (pending Phase 1.5 boundary validation)
**Date**: 2026-06-15
**Analyst**: V12 Phase 1 Protocol
