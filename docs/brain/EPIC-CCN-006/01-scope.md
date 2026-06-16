# Phase 1: Scope Definition - EPIC-CCN-006

## Epic Metadata
- **Epic ID**: EPIC-CCN-006
- **Phase**: 1.0 (Scope Definition)
- **Date**: 2026-06-15
- **Status**: APPROVED

## Target Method

### Method Identification
- **Method Name**: AdoptFleetWorkingOrders
- **File**: src/V12_002.SIMA.Lifecycle.cs
- **Current Complexity**: 17 (CYC)
- **V12 Threshold**: 15 (Jane Street alignment)
- **Overage**: +2 (13% over threshold)

### Complexity Target
- **Target Complexity**: 8 or less (Jane Street strict standard)
- **Reduction Required**: -9 CYC points minimum
- **Strategy**: Extract 2-3 helper methods

## Extraction Scope (SINGLE METHOD ONLY)

### What is IN Scope
1. **AdoptFleetWorkingOrders Method Body**
   - All conditional logic within the method
   - All state validation checks
   - All error handling paths
   - All fleet synchronization logic

2. **Extraction Strategy**
   - Extract validation logic to ValidateFleetOrder() (Est. CYC: 3-4)
   - Extract state transition logic to TransitionFleetState() (Est. CYC: 2-3)
   - Extract error recovery logic to HandleAdoptionError() (Est. CYC: 2-3)
   - Remaining main flow in AdoptFleetWorkingOrders() (Target CYC: 8 or less)

3. **Refactoring Constraints**
   - Maintain exact same behavior (no logic changes)
   - Preserve all error handling semantics
   - Keep all logging/audit trail intact
   - Maintain lock-free Actor/FSM pattern

### What is OUT of Scope
1. **Callers** (Zero changes)
   - No modifications to methods that invoke AdoptFleetWorkingOrders
   - No changes to lifecycle event handlers
   - No changes to fleet state synchronization routines

2. **Callees** (Zero changes)
   - No modifications to methods called by AdoptFleetWorkingOrders
   - No changes to state validation methods
   - No changes to fleet state accessors
   - No changes to logging/audit utilities

3. **Other Methods in Same File** (Zero changes)
   - No modifications to other methods in V12_002.SIMA.Lifecycle.cs
   - No "while we are here" improvements
   - No fixing pre-existing issues outside target method

4. **Cross-Cutting Concerns** (Explicitly Excluded)
   - No changes to error handling infrastructure
   - No changes to logging framework
   - No changes to FSM/Actor base classes
   - No changes to fleet state data structures

## Success Criteria

### Functional Requirements
- All existing tests pass (100% pass rate)
- No behavior changes (bit-for-bit identical output)
- All error paths preserved
- All logging statements intact

### Complexity Requirements
- AdoptFleetWorkingOrders CYC reduced from 17 to 8 or less
- Each extracted method has CYC 5 or less
- Total complexity budget maintained (no complexity hiding)

### V12 DNA Compliance
- Lock-free Actor/FSM pattern maintained
- ASCII-only compliance (no Unicode/emoji)
- Atomic operations preserved
- No new lock() statements introduced

### Quality Gates
- CSharpier formatting passes
- Roslyn analyzer passes (zero violations)
- Pre-push validation passes (all 13 checks)
- Codacy shows "Up to quality standards"

## Risk Assessment

### Overall Risk Level: LOW-MEDIUM

**Justification**:
- **Moderate Overage**: Only +2 over V12 threshold (manageable)
- **Clear Extraction Path**: Well-defined helper methods
- **Single Method Focus**: Minimal blast radius
- **No Behavior Changes**: Pure refactoring (no logic modifications)

### Risk Mitigation
1. **Test Coverage**: Verify existing tests or add new ones before extraction
2. **Incremental Extraction**: Extract one helper method at a time
3. **Verification Loop**: Run tests after each extraction
4. **Rollback Plan**: Git checkpoints before each extraction step

## Scope Creep Prevention (V12.23 Protocol)

### Prohibited Actions
- No "while we are here" improvements to other methods
- No fixing pre-existing compilation errors outside target method
- No bundling multiple concerns into this epic
- No refactoring callers or callees
- No infrastructure changes (logging, error handling, FSM base)

### Enforcement
- **Phase 1.5**: Mandatory boundary validation before proceeding to Phase 2
- **Code Review**: Diff must show changes ONLY to AdoptFleetWorkingOrders and new helper methods
- **PR Hygiene**: Diff size less than 10k characters (surgical changes only)

## Next Steps

1. **Phase 1.5**: Boundary validation (mandatory V12.23 gate)
2. **Phase 2**: Forensic analysis and extraction planning
3. **Phase 3**: DNA and PR audit (Arena AI red team)
4. **Phase 4**: Incremental extraction (Bob CLI)
5. **Phase 5**: Verification and sign-off

## Approval

- **Status**: APPROVED
- **Approver**: V12 Phase 1 Scope Analyzer
- **Date**: 2026-06-15
- **Rationale**: Single-method extraction with clear boundaries, no scope creep detected
