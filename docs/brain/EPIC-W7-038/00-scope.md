# Phase 1: Scope Definition - EPIC-W7-038

**Agent**: v12-phase1-scope
**Epic**: EPIC-W7-038
**Target Method**: VerifyPhotonSlotIntegrity
**File**: V12_002.SIMA.Fleet.cs
**Baseline Complexity**: 14
**Target Complexity**: ≤8
**Date**: 2026-06-24

---

## Scope Boundary Definition

### IN SCOPE

#### Primary Target
- **Method**: `VerifyPhotonSlotIntegrity()` in V12_002.SIMA.Fleet.cs
- **Current CYC**: 14
- **Target CYC**: ≤8
- **Reduction Required**: 6 points (43%)

#### Extraction Plan (3 Methods)

##### 1. ValidateSlotAllocation
**Purpose**: Extract slot existence and allocation validation logic
**Signature**: `private bool ValidateSlotAllocation(int slotIndex)`
**Estimated CYC**: 3-4
**Scope**:
- Slot index bounds checking
- Slot null/existence validation
- Allocation state verification
- Return true if valid, false otherwise

##### 2. VerifyFSMStateConsistency
**Purpose**: Extract FSM state consistency checks
**Signature**: `private bool VerifyFSMStateConsistency(SIMA_FSM fsm, int slotIndex)`
**Estimated CYC**: 2-3
**Scope**:
- FSM null checks
- FSM state validation against slot
- State transition consistency
- Return true if consistent, false otherwise

##### 3. ReportSlotIntegrityError
**Purpose**: Extract error logging and reporting
**Signature**: `private void ReportSlotIntegrityError(string errorType, int slotIndex, string details = "")`
**Estimated CYC**: 1-2
**Scope**:
- Standardized error message formatting
- Logging with appropriate severity
- Error context capture (slot index, error type)
- No return value (void)

#### Modified Method
- **VerifyPhotonSlotIntegrity**: Orchestrates the three extracted methods
- **Post-Refactor CYC**: 6-8 (target: ≤8)
- **Logic**: Call extracted methods in sequence, handle results

---

### OUT OF SCOPE

#### Explicitly Excluded
1. **Other Fleet Methods**: No changes to other methods in V12_002.SIMA.Fleet.cs
2. **FSM State Machine Logic**: No modifications to SIMA_FSM class or state transitions
3. **Photon Slot Data Structures**: No changes to slot arrays or allocation structures
4. **Caller Methods**: No modifications to methods that call VerifyPhotonSlotIntegrity
5. **Logging Infrastructure**: No changes to logging framework or configuration
6. **Test Files**: Test creation is Phase 5 work, not scope definition

#### Boundary Conditions
- **No API Changes**: All extracted methods are private helpers
- **No State Mutations**: Extracted methods only read state, no writes
- **No Performance Optimization**: Focus is complexity reduction, not speed
- **No Architectural Changes**: Stay within existing Fleet management patterns

---

## Extraction Sequence

### Step 1: Extract ReportSlotIntegrityError
**Rationale**: Simplest extraction, lowest risk
**CYC Reduction**: 1-2 points
**Dependencies**: None

### Step 2: Extract ValidateSlotAllocation
**Rationale**: Medium complexity, clear boundaries
**CYC Reduction**: 3-4 points
**Dependencies**: Uses ReportSlotIntegrityError for errors

### Step 3: Extract VerifyFSMStateConsistency
**Rationale**: Most complex, depends on slot validation
**CYC Reduction**: 2-3 points
**Dependencies**: Uses ReportSlotIntegrityError for errors

### Step 4: Refactor VerifyPhotonSlotIntegrity
**Rationale**: Orchestrate extracted methods
**Final CYC**: 6-8 (target: ≤8)
**Dependencies**: All three extracted methods

---

## Method Signatures (Final)

```csharp
// Extracted Methods (all private)
private bool ValidateSlotAllocation(int slotIndex)
private bool VerifyFSMStateConsistency(SIMA_FSM fsm, int slotIndex)
private void ReportSlotIntegrityError(string errorType, int slotIndex, string details = "")

// Modified Method
private void VerifyPhotonSlotIntegrity()
```

---

## Jane Street Alignment

### Correctness by Construction
- **ValidateSlotAllocation**: Returns bool, caller decides action
- **VerifyFSMStateConsistency**: Returns bool, explicit state validation
- **ReportSlotIntegrityError**: Void, single responsibility (logging only)

### Cognitive Simplicity
- Each extracted method has single responsibility
- Clear input/output contracts
- No hidden side effects (except logging)

### Fail-Fast Principle
- Early returns in validation methods
- Explicit error reporting before failure
- No silent failures

---

## Scope Validation Checklist

### Boundary Compliance
- [x] All extractions from single method (VerifyPhotonSlotIntegrity)
- [x] No changes to FSM state machine
- [x] No changes to caller methods
- [x] No changes to data structures
- [x] All new methods are private helpers

### Complexity Targets
- [x] ValidateSlotAllocation: CYC 3-4 (≤8)
- [x] VerifyFSMStateConsistency: CYC 2-3 (≤8)
- [x] ReportSlotIntegrityError: CYC 1-2 (≤8)
- [x] VerifyPhotonSlotIntegrity (post): CYC 6-8 (≤8)

### Jane Street Compliance
- [x] Single responsibility per method
- [x] Explicit error handling
- [x] No lock() statements
- [x] ASCII-only compliance
- [x] Fail-fast pattern

---

## Risk Assessment

### Technical Risks
- **LOW**: All extractions are internal refactoring
- **LOW**: No API surface changes
- **MEDIUM**: FSM state handling requires care

### Mitigation
1. Extract in sequence (simplest first)
2. Verify build after each extraction
3. Run deploy-sync.ps1 after each step
4. Test FSM state consistency after completion

---

## Success Criteria

### Phase 1 Completion
- [x] Scope boundaries clearly defined (IN SCOPE vs OUT OF SCOPE)
- [x] Three extraction candidates identified with signatures
- [x] Extraction sequence documented
- [x] CYC reduction estimates provided
- [x] Jane Street alignment verified
- [x] Risk assessment completed

### Phase 1.5 Validation (Next)
- [ ] Verify no scope creep
- [ ] Confirm CYC estimates are achievable
- [ ] Validate extraction sequence is optimal

---

## Metadata

**Agent**: v12-phase1-scope (plan mode)
**MCP Tools Used**: Sequential Thinking (scope boundary validation)
**Execution Time**: <3 minutes
**Bobcoins Used**: 1 MCP call (sequentialthinking)

---

## Next Phase

**Phase 1.5**: Scope Boundary Validation
- Verify scope boundaries are respected
- Confirm no hidden dependencies
- Validate CYC reduction estimates
- Check for scope creep risks
