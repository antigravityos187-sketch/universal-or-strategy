# Phase 1: Scope Definition - EPIC-W7-008

**Agent**: v12-phase1-scope
**Epic**: EPIC-W7-008
**Target Method**: ManageCIT
**File**: V12_002.Orders.Management.Flatten.cs
**Date**: 2026-06-24

## Scope Boundary

### IN SCOPE

#### Primary Target
- **Method**: `ManageCIT` (CYC 19 → ≤8)
- **File**: `V12_002.Orders.Management.Flatten.cs`
- **Lines**: Entire method body (estimated 150-200 lines)

#### Extraction Targets (4 Methods)

1. **ValidateCITEligibility** (CYC ~4)
   - **Purpose**: Extract CIT eligibility validation logic
   - **Scope**: All conditional checks determining if CIT can be applied
   - **Includes**:
     - Order state validation
     - FSM state checks
     - Market condition checks
     - Risk limit validation
   - **Excludes**: Order execution logic, state transitions

2. **TransitionCITState** (CYC ~3)
   - **Purpose**: Extract FSM state transition logic
   - **Scope**: All FSM state updates related to CIT
   - **Includes**:
     - FSM state mutation calls
     - State transition logging
     - State consistency checks
   - **Excludes**: Order creation, validation logic

3. **ProcessCITOrder** (CYC ~4)
   - **Purpose**: Extract order creation/modification logic
   - **Scope**: All order object manipulation for CIT
   - **Includes**:
     - Order creation calls
     - Order property updates
     - Order submission logic
   - **Excludes**: Validation, error handling, state transitions

4. **HandleCITError** (CYC ~3)
   - **Purpose**: Extract error recovery logic
   - **Scope**: All error handling paths in CIT management
   - **Includes**:
     - Error detection logic
     - Recovery actions
     - Error logging
     - Fallback state transitions
   - **Excludes**: Happy path logic, validation

#### Orchestration Method
- **ManageCIT** (post-extraction CYC ~5)
  - **Role**: High-level orchestration only
  - **Calls**: ValidateCITEligibility → ProcessCITOrder → TransitionCITState
  - **Error Path**: HandleCITError on exceptions
  - **No Business Logic**: Pure delegation to extracted methods

### OUT OF SCOPE

#### Explicitly Excluded

1. **Caller Methods**
   - Do NOT modify methods that call `ManageCIT`
   - Caller signatures remain unchanged
   - No changes to call sites

2. **FSM Core Logic**
   - Do NOT modify SIMA_FSM class
   - Do NOT change FSM state machine definitions
   - Do NOT alter FSM transition rules

3. **Order Management Infrastructure**
   - Do NOT modify Order class
   - Do NOT change order validation framework
   - Do NOT alter order submission infrastructure

4. **Other CIT-Related Methods**
   - Do NOT modify other CIT methods outside ManageCIT
   - Do NOT refactor adjacent methods "while we are here"
   - Strict single-epic focus

5. **Test Files**
   - Do NOT modify existing test files (unless adding new tests)
   - Do NOT refactor test infrastructure
   - Only ADD new unit tests for extracted methods

6. **Configuration/Constants**
   - Do NOT modify CIT configuration values
   - Do NOT change risk limits or thresholds
   - Do NOT alter logging configuration

#### Boundary Violations (FORBIDDEN)

- Changing method signatures of callers
- Modifying FSM state machine logic
- Refactoring adjacent methods
- Changing order validation rules
- Altering error handling framework
- Modifying logging infrastructure
- Changing configuration values

## Scope Validation

### Complexity Budget
- **Before**: ManageCIT CYC 19
- **After**:
  - ManageCIT CYC ~5 (orchestration)
  - ValidateCITEligibility CYC ~4
  - TransitionCITState CYC ~3
  - ProcessCITOrder CYC ~4
  - HandleCITError CYC ~3
- **Total**: 19 (redistributed, not reduced)
- **Target Met**: All methods ≤8

### Blast Radius
- **Direct Impact**: 1 file (V12_002.Orders.Management.Flatten.cs)
- **Indirect Impact**: 2-3 caller methods (signatures unchanged)
- **Test Impact**: Add 4 new unit test methods
- **Risk Level**: LOW (isolated extraction)

### Jane Street Alignment
- **Cognitive Simplicity**: Each method has single responsibility
- **Testability**: Small methods easier to test exhaustively
- **Correctness by Construction**: FSM invariants preserved
- **No Performance Impact**: Not on hot path, JIT can inline

## Success Criteria

### Phase 1 Complete When:
- Scope boundaries clearly defined (IN SCOPE vs OUT OF SCOPE)
- 4 extraction targets identified with CYC estimates
- Orchestration method role defined
- Boundary violations explicitly listed
- Complexity budget validated (all methods ≤8)

### Ready for Phase 1.5 (Boundary Validation):
- No scope creep detected
- Single-epic focus maintained
- Blast radius contained
- Jane Street principles applied

## Risk Mitigation

### Extraction Risks
1. **FSM State Consistency**: Ensure atomic state transitions preserved
   - Mitigation: Extract entire transition logic as single method
2. **Order Validation Behavior**: Preserve exact validation logic
   - Mitigation: Copy-paste validation logic, no modifications
3. **Error Handling Paths**: Maintain all error recovery paths
   - Mitigation: Extract error handling as separate method

### Testing Strategy
- Unit test each extracted method independently
- Integration test ManageCIT orchestration
- Verify FSM state consistency after extraction
- Validate order flow end-to-end

## Next Phase

**Phase 1.5**: Scope Boundary Validation
- Verify no scope creep
- Confirm single-epic focus
- Validate complexity budget
- Approve for Phase 2 (Architecture Planning)

---

**Phase 1 Status**: COMPLETE
**Scope Creep Risk**: LOW
**Ready for Phase 1.5**: YES
**Blocking Issues**: NONE
