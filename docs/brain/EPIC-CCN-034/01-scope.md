# Phase 1.0: Scope Definition - EPIC-CCN-034

## Extraction Scope (SINGLE METHOD ONLY)

### Target Method
- **Method Name**: ManageCIT
- **File**: src/V12_002.Orders.Management.Flatten.cs
- **Current Complexity**: 19
- **Target Complexity**: <=8 (Jane Street strict standard)
- **Threshold Violation**: +4 over Jane Street threshold (15)

### Extraction Strategy

**Approach**: Break into 2-3 helper methods to achieve CYC <=8

#### Proposed Extractions

1. **ValidateCITOrder** (NEW)
   - **Purpose**: Isolate order validation logic
   - **Estimated Complexity**: 3-4
   - **Extracts**: Order state checks, quantity validation, action validation
   - **Returns**: ValidationResult or throws exception

2. **HandleOCOCoordination** (NEW)
   - **Purpose**: Isolate One-Cancels-Other coordination logic
   - **Estimated Complexity**: 4-5
   - **Extracts**: OCO parameter handling, linked order management
   - **Returns**: void (side effects on order collections)

3. **TransitionCITState** (NEW)
   - **Purpose**: Isolate FSM state transition logic
   - **Estimated Complexity**: 5-6
   - **Extracts**: State machine transitions, event notifications
   - **Returns**: void (side effects on order state)

4. **ManageCIT** (REDUCED)
   - **Purpose**: Orchestrate CIT order management
   - **Target Complexity**: 5-6
   - **Remains**: High-level coordination, calls to extracted methods
   - **Maintains**: Public API contract

### Complexity Reduction Calculation

Current:  ManageCIT = 19

After Extraction:
  ValidateCITOrder       = 4
  HandleOCOCoordination  = 5
  TransitionCITState     = 6
  ManageCIT (reduced)    = 5
  Total Complexity       = 20 (distributed)
  Max Method Complexity  = 6 (<=8 target met)

## Boundary Definition

### IN SCOPE
- ManageCIT method body only
- Extract validation logic into ValidateCITOrder
- Extract OCO handling into HandleOCOCoordination
- Extract state transitions into TransitionCITState
- Reduce ManageCIT to orchestration logic
- Maintain lock-free Actor/FSM pattern
- Preserve all existing behavior

### OUT OF SCOPE
- Callers: No changes to methods that call ManageCIT
- Callees: No changes to methods called by ManageCIT
- Other Methods: No changes to other methods in V12_002.Orders.Management.Flatten.cs
- Order State Machine: No changes to FSM infrastructure
- Order Collections: No changes to collection structures
- Event System: No changes to event notification infrastructure
- OCO Infrastructure: No changes to OCO coordination framework

### No Scope Creep Mandate

ONE EPIC = ONE CONCERN

This EPIC addresses ONLY:
- Complexity reduction of ManageCIT from 19 to <=8
- Extraction of helper methods
- Preservation of existing behavior

This EPIC does NOT address:
- Pre-existing compilation errors
- Other high-complexity methods
- Performance optimizations
- Feature additions
- While we are here improvements

## Success Criteria

### Functional Requirements
1. Complexity Reduced: ManageCIT complexity <=8
2. All Tests Pass: Zero test failures
3. No Behavior Changes: Identical runtime behavior
4. Lock-Free Pattern: Actor/FSM pattern maintained
5. ASCII-Only: No Unicode in string literals

### Quality Gates
1. Build Success: Zero compilation errors
2. Lint Clean: Zero Roslyn violations
3. Format Clean: CSharpier passes
4. Complexity Audit: All methods <=15 (Jane Street threshold)
5. Pre-Push Validation: All 13 checks pass

### Testing Requirements
1. Unit Tests: Add tests for extracted methods
2. Integration Tests: Verify order management workflows
3. Stress Tests: Run test_stress.ps1
4. Regression Tests: Verify no behavior changes

### Documentation Requirements
1. Code Comments: Document extracted method purposes
2. Architecture Notes: Update if FSM patterns change
3. EPIC Manifest: Update completion status

## Risk Assessment

### Compilation Risk: LOW
- Isolated method extraction
- No changes to method signatures
- No changes to callers/callees

### Runtime Risk: MEDIUM
- Order state mutations
- OCO coordination logic
- FSM state transitions
- Requires comprehensive testing

### Testing Risk: HIGH
- 19 branches to test
- Complex conditional logic
- Multi-order dependencies
- Requires TDD approach

## Mitigation Strategy

1. Pre-Refactoring:
   - Add unit tests for all 19 branches
   - Document current behavior
   - Create test fixtures for order states

2. During Refactoring:
   - Use TDD for extracted methods
   - Verify behavior preservation after each extraction
   - Run tests after each commit

3. Post-Refactoring:
   - Run full test suite
   - Run stress tests
   - Verify OCO coordination
   - Check FSM state transitions

## Phase 1.0 Completion Status
- Extraction scope defined
- Boundary established
- Success criteria documented
- Risk assessment completed

Ready for Phase 1.5 (Boundary Validation)
