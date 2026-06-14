# Phase 1: Scope + Boundary Definition - EPIC-CCN-119

## Target Method Details

### Method Identification
- **Method**: EmergencyFlattenSingleFleetAccount
- **File**: src/V12_002.SIMA.Flatten.cs
- **Current Complexity**: 16
- **Target Complexity**: ≤ 8 (Jane Street HFT standard)
- **Overage**: +8 (100% over threshold)
- **Epic ID**: EPIC-CCN-119

### Method Responsibility
Emergency handler for flattening positions in a single fleet account. Critical path for risk management during emergency scenarios.

## Extraction Strategy

### What to Extract

**Primary Extraction Candidates** (to achieve CYC ≤ 8):

1. **Fleet Account Validation Logic** (estimated CYC reduction: -3)
   - Extract pre-condition checks
   - Account state validation
   - Emergency eligibility verification
   - New method: `ValidateFleetAccountForEmergencyFlatten`

2. **Position Flattening Coordination** (estimated CYC reduction: -4)
   - Extract position enumeration and flattening loop
   - Order submission coordination
   - Position state tracking
   - New method: `ExecutePositionFlatteningForFleet`

3. **Error Handling and Logging** (estimated CYC reduction: -2)
   - Extract error path handling
   - Telemetry and logging coordination
   - Recovery state management
   - New method: `HandleFlatteningError`

**Extraction Rationale**:
- Current CYC 16 → Target CYC ≤ 8 requires ~8 point reduction
- Three extractions provide cumulative reduction of ~9 points
- Maintains emergency semantics and atomic guarantees
- Preserves single responsibility per extracted method

### What to Keep in Original Method

**Core Orchestration Logic** (remaining CYC ≤ 8):
- High-level emergency flattening workflow
- Call coordination to extracted methods
- Top-level state transitions
- Return value composition

**Rationale for Retention**:
- Emergency handler entry point must remain clear
- Orchestration logic is inherently simple (≤ 8 branches)
- Maintains caller contract and signature
- Preserves atomic operation semantics

## Boundary Definition

### Single Method Scope (V12.23 No Scope Creep Protocol)

**IN SCOPE**:
- ✅ EmergencyFlattenSingleFleetAccount method ONLY
- ✅ Extract 3 helper methods within same class
- ✅ Preserve method signature and contract
- ✅ Maintain emergency semantics
- ✅ Keep atomic operation guarantees

**OUT OF SCOPE**:
- ❌ Other methods in SIMA.Flatten.cs
- ❌ Caller modifications
- ❌ Callee modifications (unless extraction target)
- ❌ State machine changes
- ❌ Data structure changes
- ❌ Interface changes
- ❌ Cross-module refactoring

### Dependency Constraints

**Allowed Dependencies** (within boundary):
- Existing SIMA.Flatten.cs private methods
- Existing fleet account state accessors
- Existing position management primitives
- Existing logging/telemetry infrastructure

**Prohibited Dependencies** (violate boundary):
- New external module dependencies
- Changes to caller contracts
- Modifications to state machine logic
- Changes to data models or DTOs

## Boundary Validation

### Single-Method Scope Confirmation

**Validation Checklist**:
- ✅ Extraction targets ONLY EmergencyFlattenSingleFleetAccount
- ✅ All extracted methods are private helpers in same class
- ✅ No modifications to other methods in file
- ✅ No changes to method signature or public contract
- ✅ No cross-file changes required
- ✅ No state machine modifications
- ✅ No data structure changes
- ✅ Extraction is self-contained within single method scope

### Dependency Boundary Validation

**Dependencies That Would Violate Boundary**:
- ❌ NONE IDENTIFIED

**Rationale**:
- All extraction candidates use existing infrastructure
- No new dependencies required
- Helper methods will be private to same class
- Maintains encapsulation and isolation

### Explicit Boundary Statement

**Boundary Validated: YES**

**Justification**:
1. Extraction scope limited to single method (EmergencyFlattenSingleFleetAccount)
2. All helper methods are private within same class (no API surface changes)
3. No external dependencies introduced
4. No caller or callee contract modifications
5. No cross-module or cross-file changes
6. Maintains atomic operation semantics
7. Preserves emergency handler isolation

## Success Criteria

### Complexity Targets
- **Primary**: Reduce EmergencyFlattenSingleFleetAccount to CYC ≤ 8
- **Secondary**: Each extracted method has CYC ≤ 8
- **Verification**: Run complexity_audit.py post-refactoring

### Jane Street Alignment
- ✅ **Cognitive Simplicity**: CYC ≤ 8 (HFT standard, not 15)
- ✅ **Single Responsibility**: Each method has one clear purpose
- ✅ **Testability**: Each extracted method is independently testable
- ✅ **Reasoning Under Pressure**: Emergency logic is simple to audit

### V12 DNA Compliance
- ✅ **No lock() blocks**: Verify lock-free patterns preserved
- ✅ **ASCII-only**: Verify no Unicode in string literals
- ✅ **Atomic operations**: Verify state transitions remain atomic
- ✅ **Correctness by construction**: Verify illegal states unrepresentable

### Build and Test Gates
- ✅ **Build**: Zero compilation errors
- ✅ **Tests**: All existing tests pass
- ✅ **Lint**: Zero Roslyn violations
- ✅ **Format**: CSharpier compliance
- ✅ **Pre-Push**: All 13 validation checks pass

## Risk Assessment

### Overall Risk Level: LOW-MEDIUM

**Risk Factors**:
1. ✅ **Small Scope**: Single method, 3 extractions (manageable)
2. ✅ **Clear Boundary**: No scope creep risk
3. ⚠️ **Emergency Handler**: Critical path (requires careful testing)
4. ✅ **Fleet-Scoped**: Limited blast radius (not global)
5. ⚠️ **Unknown Test Coverage**: Requires verification before refactoring

### Mitigation Strategy

**Pre-Refactoring**:
1. Audit existing test coverage for EmergencyFlattenSingleFleetAccount
2. Add missing emergency scenario tests if needed
3. Document current behavior and edge cases
4. Verify lock-free compliance in current implementation

**During Refactoring**:
1. Extract one method at a time
2. Run tests after each extraction
3. Verify complexity reduction incrementally
4. Maintain atomic operation semantics

**Post-Refactoring**:
1. Run full test suite
2. Verify complexity targets met (CYC ≤ 8)
3. Run pre-push validation (all 13 checks)
4. Manual emergency scenario validation

### Rollback Plan
- Git checkpoint before each extraction
- Restore via `git restore` if tests fail
- Maximum 3 rollback points (one per extraction)

## Implementation Constraints

### Code Style
- Follow existing SIMA.Flatten.cs patterns
- Use CSharpier formatting
- Maintain consistent naming conventions
- Preserve existing comment style

### Performance
- Zero performance regression (emergency hot path)
- Maintain microsecond-latency characteristics
- No additional allocations in hot path
- Preserve inline-ability where applicable

### Behavioral Preservation
- **CRITICAL**: Zero behavioral changes
- Maintain exact emergency semantics
- Preserve error handling paths
- Keep logging/telemetry identical
- Maintain atomic operation guarantees

## Phase 1 Deliverables

### Outputs
1. ✅ This document (00-scope.md)
2. ⏳ Updated manifest.json (phase 1 completed)

### Next Phase Prerequisites
- Phase 1 approved by Director
- Boundary validation confirmed
- Risk assessment accepted
- Success criteria agreed upon

## Appendix: Extraction Method Signatures (Proposed)

```csharp
// Extracted Method 1: Fleet Account Validation
private bool ValidateFleetAccountForEmergencyFlatten(
    string fleetAccountId,
    out string validationError)
{
    // Pre-condition checks
    // Account state validation
    // Emergency eligibility verification
}

// Extracted Method 2: Position Flattening Coordination
private bool ExecutePositionFlatteningForFleet(
    string fleetAccountId,
    out int positionsFlattenedCount)
{
    // Position enumeration
    // Flattening loop coordination
    // Order submission
}

// Extracted Method 3: Error Handling
private void HandleFlatteningError(
    string fleetAccountId,
    Exception error,
    string context)
{
    // Error path handling
    // Telemetry and logging
    // Recovery state management
}
```

---

**Phase 1 Status**: ✅ COMPLETED
**Boundary Validated**: ✅ YES
**Next Phase**: Phase 2 (Architecture Planning)
**Assigned Agent**: Bob CLI (v12-engineer)
**Target Complexity**: ≤ 8 (Jane Street HFT standard)
**Risk Level**: LOW-MEDIUM
