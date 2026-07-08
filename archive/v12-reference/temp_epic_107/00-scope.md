# Phase 1: Scope Definition - EPIC-CCN-107

## Target Method
- **Method Name**: `HydrateFromOpenPositions`
- **File**: `src/V12_002.SIMA.Lifecycle.cs`
- **Current Complexity**: 31 (CYC)
- **Target Complexity**: ≤15 (Jane Street aligned)
- **Violation Severity**: HIGH (2.07x over threshold)

## Extraction Strategy

### What to Extract

Based on Phase 0 hotspot analysis, extract the following sub-functions:

1. **Position Validation Logic** (Est. CYC reduction: 5)
   - Validate position data integrity
   - Check for null/invalid positions
   - Verify position state consistency

2. **State Transition Logic** (Est. CYC reduction: 8)
   - FSM state updates based on position data
   - State machine transitions
   - Actor Enqueue calls for state mutations

3. **Order Mapping Logic** (Est. CYC reduction: 6)
   - Order ID to position mapping
   - Order tracking and correlation
   - Order state synchronization

4. **Risk Calculation Logic** (Est. CYC reduction: 4)
   - Position size calculations
   - P&L tracking
   - Risk metric updates

### What to Keep

The orchestrator method will retain:
- High-level coordination logic
- Method signature and public interface
- Top-level error handling
- Calls to extracted helper methods

**Target Post-Extraction CYC**: 8-12 (orchestration only)

## Boundary Definition

### Single Method Scope (V12.23 No Scope Creep Protocol)

**IN SCOPE**:
- ✅ `HydrateFromOpenPositions` method only
- ✅ Extract helper methods within same class
- ✅ Maintain existing method signature
- ✅ Preserve public API contract

**OUT OF SCOPE**:
- ❌ Other methods in V12_002.SIMA.Lifecycle.cs
- ❌ Caller modifications
- ❌ Related but separate functionality
- ❌ Architectural changes beyond extraction

### Extraction Boundaries

- **File**: Single file only (`src/V12_002.SIMA.Lifecycle.cs`)
- **Class**: Single class only (SIMA lifecycle class)
- **Method**: Single method extraction (HydrateFromOpenPositions)
- **Helpers**: New private methods in same class

## Success Criteria

### Primary Goals

1. **Complexity Reduction**
   - ✅ HydrateFromOpenPositions CYC ≤15
   - ✅ Each extracted method CYC ≤15
   - ✅ Total complexity preserved (sum of parts)

2. **Functional Correctness**
   - ✅ All existing tests pass
   - ✅ No behavioral changes
   - ✅ Same input/output contract

3. **V12 DNA Compliance**
   - ✅ No lock(stateLock) blocks
   - ✅ ASCII-only string literals
   - ✅ Actor/FSM Enqueue pattern for state mutations

4. **Build & Deploy**
   - ✅ Zero build errors
   - ✅ Hard-link sync successful (deploy-sync.ps1)
   - ✅ NinjaTrader F5 test passes

### Quality Gates

- **Pre-Push Validation**: All 13 checks pass
- **Codacy**: No new complexity violations
- **CodeRabbit**: No critical/high findings
- **CSharpier**: Zero formatting issues

## Risk Assessment

### Overall Risk Level: **MEDIUM-HIGH**

**Risk Factors**:

1. **Complexity** (HIGH)
   - CYC 31 indicates deeply nested logic
   - Multiple decision points increase extraction risk
   - Potential for subtle behavioral changes

2. **Criticality** (HIGH)
   - Position hydration is core to strategy correctness
   - Errors could cause state desynchronization
   - Impacts live trading if deployed incorrectly

3. **State Mutation** (MEDIUM)
   - Multiple FSM state updates
   - Race condition risk if lock-free pattern violated
   - Requires careful Actor/Enqueue verification

4. **Testing Coverage** (MEDIUM)
   - Current test coverage unknown
   - May need new TDD tests for extracted methods
   - Integration tests required for orchestrator

### Mitigation Strategy

1. **Incremental Extraction**
   - Extract one sub-function at a time
   - Build and test after each extraction
   - Use checkpointing for rollback safety

2. **TDD Approach**
   - Write tests for extracted methods first
   - Verify behavior preservation
   - Add integration tests for orchestrator

3. **DNA Audit**
   - Phase 3 adversarial review (Arena AI)
   - Lock-free pattern verification
   - ASCII-only compliance check

4. **Staged Deployment**
   - Local build verification
   - Hard-link sync validation
   - NinjaTrader F5 smoke test
   - Stress test before production

## Dependencies

### Required Analysis (Phase 2)

- Call hierarchy analysis (who calls this method?)
- Data flow analysis (what state is mutated?)
- Lock-free pattern audit (any lock blocks?)
- ASCII compliance scan (any Unicode strings?)

### Tooling Requirements

- jCodemunch-MCP for symbol analysis
- complexity_audit.py for CYC verification
- pre_push_validation.ps1 for quality gates
- deploy-sync.ps1 for hard-link sync

## Next Phase

**Phase 2 (Planning)**: Create detailed implementation plan with:
- Exact extraction boundaries (line numbers)
- Helper method signatures
- Test strategy
- Step-by-step execution plan

---

**Scope Defined**: 2026-06-13
**Phase**: 1 (Scope Definition)
**Status**: COMPLETED
**Next Phase**: 2 (Planning)
