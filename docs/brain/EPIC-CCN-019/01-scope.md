# Phase 1.0: Scope Definition - EPIC-CCN-019

## Epic Metadata
- **Epic ID**: EPIC-CCN-019
- **Target Method**: TryHandleFleet_MoveTarget
- **File**: src/V12_002.UI.IPC.Commands.Fleet.cs
- **Current Complexity**: 15 (AT V12 THRESHOLD)
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **Protocol**: V12.23 Photon Kernel
- **Date**: 2026-06-15

## 1. Extraction Scope (SINGLE METHOD ONLY)

### Target Method
- **Method Name**: TryHandleFleet_MoveTarget
- **Current CYC**: 15
- **Target CYC**: ≤8
- **Lines**: ~50-80 (estimated from Phase 0 analysis)

### Extraction Strategy
Break into 3 focused helper methods:

1. **ValidateFleetMoveCommand** (Target CYC: ~5)
   - Parameter validation (null checks, range validation)
   - Fleet state checks (fleet exists, fleet is active)
   - Target position validation (valid coordinates, reachable)
   - Returns: ValidationResult or throws typed exception

2. **ProcessFleetMoveTarget** (Target CYC: ~5)
   - Core movement logic (state transition)
   - FSM/Actor Enqueue for state update
   - Event emission (movement started event)
   - Returns: ProcessingResult

3. **TryHandleFleet_MoveTarget** (Target CYC: ~5)
   - Orchestration ONLY
   - Call ValidateFleetMoveCommand
   - Call ProcessFleetMoveTarget
   - Error handling and logging
   - Returns: bool (success/failure)

### Complexity Reduction
- **Before**: 1 method @ CYC 15
- **After**: 3 methods @ CYC ~5 each
- **Total Reduction**: 15 to 5 (orchestrator complexity)
- **Cognitive Load**: HIGH to LOW

## 2. Boundary Definition

### IN SCOPE (SINGLE METHOD EXTRACTION)
- **Method Body**: TryHandleFleet_MoveTarget implementation only
- **Extraction**: Create 2 new private helper methods
- **Refactoring**: Split validation and processing logic
- **Testing**: Add unit tests for new helper methods
- **Documentation**: Update method XML comments

### OUT OF SCOPE (STRICT BOUNDARY)
- **Callers**: NO changes to IPC command router or dispatcher
- **Callees**: NO changes to existing fleet state management
- **Other Methods**: NO changes to other methods in V12_002.UI.IPC.Commands.Fleet.cs
- **Cross-File**: NO changes to files outside V12_002.UI.IPC.Commands.Fleet.cs
- **Pre-existing Issues**: NO fixing compilation errors in other methods
- **Scope Creep**: NO "while we are here" improvements

### Boundary Enforcement
- **ONE EPIC = ONE CONCERN**: Single-method complexity reduction
- **No Bundling**: Do not combine with other refactoring tasks
- **No Drift**: Stay within method body boundaries
- **No Expansion**: Do not touch adjacent code

## 3. Success Criteria

### Functional Requirements
- Behavior Preservation: Zero behavior changes (black-box equivalence)
- All Tests Pass: Existing FSMActorTests.cs must pass
- New Tests Added: Unit tests for ValidateFleetMoveCommand and ProcessFleetMoveTarget
- Integration Test: Full flow test for TryHandleFleet_MoveTarget

### Complexity Requirements
- Target CYC: TryHandleFleet_MoveTarget reduced to ≤8
- Helper CYC: ValidateFleetMoveCommand ≤8
- Helper CYC: ProcessFleetMoveTarget ≤8
- Verification: Run complexity_audit.py to confirm

### V12 DNA Compliance
- Lock-Free: No lock(stateLock) blocks introduced
- ASCII-Only: No Unicode, emoji, or curly quotes in string literals
- FSM/Actor Pattern: Maintain Enqueue model for state mutations
- Atomic Operations: Use atomic primitives where needed
- Correctness by Construction: Type-safe state representation

## 4. Risk Mitigation

### Identified Risks
1. IPC Boundary: Method is at IPC layer (well-isolated, LOW risk)
2. State Management: Must preserve FSM/Actor pattern (MEDIUM risk)
3. Validation Logic: Multiple branches to extract (MEDIUM risk)
4. Test Coverage: Limited existing tests (MEDIUM risk)

### Mitigation Strategy
1. Checkpointing: Enable Bob CLI checkpointing for rollback
2. Incremental: Extract one helper at a time, verify after each
3. Test-First: Write tests for helpers before extraction
4. Review: Arena AI red team review before merge

## 5. Next Steps (Phase 2)

After Phase 1.5 (Boundary Validation) approval:
1. Arch Planning (Bob CLI): Create 02-implementation-plan.md
2. Mermaid Diagrams: Document extraction flow and state transitions
3. DNA Audit (Arena AI): Red team review of plan
4. Execution (Bob CLI): Implement extraction with checkpointing

## Approval Gate

**Status**: PENDING PHASE 1.5 BOUNDARY VALIDATION
