# Phase 1: Scope Definition - EPIC-W7-101

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Phase**: 1 (Scope Definition)
- **Input**: 00-hotspots.md
- **Output**: 00-scope.md
- **Execution Time**: 2026-06-24T20:13:51Z

## Epic Overview

### Target Method
- **Method**: `VerifyPhotonSlotIntegrity`
- **File**: `src/V12_002.SIMA.Fleet.cs`
- **Line**: 329
- **Current Complexity**: CYC 16 (HIGH)
- **Target Complexity**: CYC ≤ 4 (Jane Street compliant)

### Refactoring Goal
Extract 5 single-responsibility methods from `VerifyPhotonSlotIntegrity` to reduce cyclomatic complexity from 16 to ≤4, achieving Jane Street CYC ≤8 threshold compliance.

---

## Scope Boundaries

### What Will Be Extracted (5 Methods)

#### 1. CRC Validation Logic
**Extracted Method**: `ValidatePhotonCrc`
- **Responsibility**: CRC validation and failure tracking
- **Estimated CYC**: 4
- **Lines**: ~12
- **Dependencies**: 
  - `TrackPhotonCrcFailure` (telemetry)
  - `LogBuffer.Format` (logging)
- **Signature**: `private bool ValidatePhotonCrc(FleetDispatchSlot slot, FleetDispatchSideband sideband)`
- **Returns**: `true` if CRC valid, `false` if invalid

#### 2. Shadow Computation
**Extracted Method**: `ComputeAndValidateShadow`
- **Responsibility**: Photon pool shadow computation and validation
- **Estimated CYC**: 3
- **Lines**: ~10
- **Dependencies**:
  - `ComputeFleetDispatchShadow` (core computation)
  - `LogBuffer.Format` (logging)
- **Signature**: `private bool ComputeAndValidateShadow(FleetDispatchSlot slot, out int computedShadow)`
- **Returns**: `true` if shadow valid, `false` if mismatch

#### 3. Position Delta Management
**Extracted Method**: `ApplyPositionDelta`
- **Responsibility**: SIMA position delta application
- **Estimated CYC**: 3
- **Lines**: ~10
- **Dependencies**:
  - `AddExpectedPositionDeltaLocked` (SIMA state)
  - `activePositions`, `entryOrders`, `stopOrders` (state access)
- **Signature**: `private void ApplyPositionDelta(FleetDispatchSlot slot, int photonIndex)`
- **Returns**: `void` (side effects on SIMA state)

#### 4. Circuit Breaker Logic
**Extracted Method**: `CheckAndResetCircuitBreaker`
- **Responsibility**: Circuit breaker threshold check and reset
- **Estimated CYC**: 2
- **Lines**: ~8
- **Dependencies**:
  - `TryResetCircuitBreakerIfBelow` (circuit breaker management)
  - `LogBuffer.Format` (logging)
- **Signature**: `private void CheckAndResetCircuitBreaker(FleetDispatchSlot slot)`
- **Returns**: `void` (side effects on circuit breaker state)

#### 5. Telemetry Tracking
**Extracted Method**: `TrackSlotTelemetry`
- **Responsibility**: Consolidated telemetry tracking for slot verification
- **Estimated CYC**: 2
- **Lines**: ~8
- **Dependencies**:
  - `TrackPhotonCrcFailure` (telemetry)
  - `LogBuffer.Format` (logging)
- **Signature**: `private void TrackSlotTelemetry(FleetDispatchSlot slot, string eventType, string details)`
- **Returns**: `void` (side effects on telemetry state)

### What Will Remain (Orchestrator)

**Orchestrator Method**: `VerifyPhotonSlotIntegrity` (refactored)
- **Responsibility**: High-level orchestration of slot integrity verification
- **Target CYC**: ≤ 4
- **Lines**: ~20-25
- **Logic**:
  1. Call `ValidatePhotonCrc` → early return if invalid
  2. Call `ComputeAndValidateShadow` → early return if invalid
  3. Call `ApplyPositionDelta` → apply state changes
  4. Call `CheckAndResetCircuitBreaker` → manage circuit breaker
  5. Call `TrackSlotTelemetry` → log success
  6. Call `ClearDispatchSyncPending` → finalize dispatch
- **Pattern**: Sequential method calls with early returns (guard clauses)

---

## Dependencies and Coupling

### Internal Dependencies (Same File)
- `PumpFleetDispatch` (caller, line 233)
- `ProcessFleetSlot` (indirect caller, line 44)
- `ComputeFleetDispatchShadow` (callee)
- `ClearDispatchSyncPending` (callee)
- `GetTargetOrdersDictionary` (callee)

### Cross-File Dependencies
- `AddExpectedPositionDeltaLocked` (SIMA position management)
- `TrackPhotonCrcFailure` (telemetry subsystem)
- `TryResetCircuitBreakerIfBelow` (circuit breaker subsystem)
- `LogBuffer.Format` (logging subsystem)

### State Access
- `activePositions` (order state)
- `entryOrders` (order state)
- `stopOrders` (order state)
- `_followerBrackets` (fleet management)
- `_photonPool` (fleet management)

### Coupling Risk Assessment
- **High Coupling**: 49 callees across 5+ subsystems
- **Mitigation**: Extract methods will encapsulate subsystem interactions
- **Benefit**: Reduced cognitive load in orchestrator method

---

## Risk Analysis

### Blast Radius: LOW
- **Private Method**: No external visibility
- **Direct Callers**: 2 (PumpFleetDispatch, ProcessFleetSlot)
- **Import Impact**: 0 external files
- **Conclusion**: Contained refactoring with minimal breaking change risk

### Complexity Risk: HIGH → LOW (Post-Extraction)
- **Current**: CYC 16 (2x Jane Street threshold)
- **Target**: CYC ≤ 4 (Jane Street compliant)
- **Reduction**: 12 complexity points
- **Benefit**: Easier to reason about, test, and maintain

### Testing Risk: HIGH → MEDIUM
- **Current**: 16 decision points require exhaustive coverage
- **Post-Extraction**: 5 methods with 2-4 decision points each
- **Benefit**: Isolated unit tests per extracted method

### Regression Risk: LOW
- **Mitigation**: Preserve exact behavior via unit tests
- **Strategy**: Test-driven extraction (write tests first)
- **Verification**: Compare pre/post behavior with integration tests

---

## Success Criteria

### Functional Requirements
1. ✅ **Behavior Preservation**: Extracted methods produce identical results to original
2. ✅ **No Breaking Changes**: All callers (PumpFleetDispatch, ProcessFleetSlot) work unchanged
3. ✅ **State Integrity**: SIMA position state, circuit breaker state, telemetry state unchanged

### Complexity Requirements
1. ✅ **Orchestrator CYC**: ≤ 4 (Jane Street compliant)
2. ✅ **Extracted Method CYC**: Each ≤ 4 (Jane Street compliant)
3. ✅ **Total Reduction**: 16 → 4 (12-point reduction)

### Quality Requirements
1. ✅ **Unit Tests**: Each extracted method has dedicated unit tests
2. ✅ **Integration Tests**: End-to-end verification of VerifyPhotonSlotIntegrity
3. ✅ **Build Success**: `dotnet build` passes with zero errors
4. ✅ **Deploy Sync**: `deploy-sync.ps1` executes successfully
5. ✅ **NinjaTrader F5**: Strategy loads without compilation errors

### V12 DNA Compliance
1. ✅ **Correctness by Construction**: Single-responsibility methods
2. ✅ **Cognitive Simplicity**: CYC ≤ 8 per method
3. ✅ **Lock-Free Pattern**: No lock() statements (already compliant)
4. ✅ **ASCII-Only**: No Unicode issues (already compliant)

---

## Extraction Order

### Phase 5 Ticket Sequence
1. **Ticket 1**: Extract `ValidatePhotonCrc` (CYC 4)
2. **Ticket 2**: Extract `ComputeAndValidateShadow` (CYC 3)
3. **Ticket 3**: Extract `ApplyPositionDelta` (CYC 3)
4. **Ticket 4**: Extract `CheckAndResetCircuitBreaker` (CYC 2)
5. **Ticket 5**: Extract `TrackSlotTelemetry` (CYC 2)
6. **Ticket 6**: Refactor orchestrator to CYC ≤ 4

### Rationale
- **Sequential Extraction**: Each ticket reduces complexity incrementally
- **Early Wins**: High-CYC methods extracted first (ValidatePhotonCrc)
- **Low Risk**: Each extraction is independently testable
- **Final Refactor**: Orchestrator refactored last after all extractions complete

---

## Out of Scope

### Explicitly Excluded
1. ❌ **Caller Refactoring**: PumpFleetDispatch and ProcessFleetSlot remain unchanged
2. ❌ **Subsystem Changes**: ComputeFleetDispatchShadow, TrackPhotonCrcFailure, etc. unchanged
3. ❌ **State Structure Changes**: activePositions, entryOrders, stopOrders unchanged
4. ❌ **Performance Optimization**: Focus is complexity reduction, not performance
5. ❌ **Feature Additions**: No new functionality, only extraction

### Future Work (Separate Epics)
- **EPIC-W7-102**: Refactor PumpFleetDispatch (if CYC > 8)
- **EPIC-W7-103**: Refactor ProcessFleetSlot (if CYC > 8)
- **EPIC-W7-104**: Optimize ComputeFleetDispatchShadow (if performance bottleneck)

---

## Verification Strategy

### Pre-Extraction Baseline
1. Run `python scripts/complexity_audit.py --file src/V12_002.SIMA.Fleet.cs`
2. Capture current CYC: 16 for VerifyPhotonSlotIntegrity
3. Run existing unit tests (if any) to establish baseline behavior

### During Extraction (Per Ticket)
1. Write unit test for extracted method
2. Extract method
3. Run unit test → verify pass
4. Run `dotnet build` → verify zero errors
5. Run `python scripts/complexity_audit.py` → verify CYC reduction

### Post-Extraction Validation
1. Run `python scripts/complexity_audit.py` → verify CYC ≤ 4 for orchestrator
2. Run `dotnet build` → verify zero errors
3. Run `powershell -File .\deploy-sync.ps1` → verify hard link sync
4. F5 in NinjaTrader → verify strategy loads
5. Run integration tests → verify end-to-end behavior

---

## Conclusion

**EPIC-W7-101** targets `VerifyPhotonSlotIntegrity` (CYC 16) for extraction into 5 single-responsibility methods, reducing complexity to CYC ≤ 4. The refactoring has **LOW blast radius risk** (private method, 2 callers) and **HIGH complexity reduction benefit** (12-point reduction).

**Scope is WELL-DEFINED** with clear extraction targets, boundaries, dependencies, and success criteria. The sequential extraction order minimizes risk and enables incremental verification.

**Recommendation**: PROCEED to Phase 2 (Architecture Planning) to design the extraction implementation strategy.
