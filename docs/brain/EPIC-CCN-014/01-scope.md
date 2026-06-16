# Phase 1.0: Scope Definition - EPIC-CCN-014

## Extraction Scope (SINGLE METHOD ONLY)

### Target Method
- Method Name: TryHandleFleetCommand
- File: src/V12_002.UI.IPC.Commands.Fleet.cs
- Current Complexity: 19
- Target Complexity: <=8 (Jane Street strict standard)
- Violation: +11 over Jane Street threshold (19 - 8 = 11)

### Extraction Strategy
Break TryHandleFleetCommand into 2-3 helper methods following these principles:

1. Command Validation Helper (estimated CYC: 3-4)
   - Extract command parsing and validation logic
   - Return early on invalid commands
   - Single responsibility: validate input

2. Command Routing Helper (estimated CYC: 3-4)
   - Extract command type discrimination logic
   - Route to appropriate handlers
   - Single responsibility: dispatch commands

3. Core Handler (remaining CYC: <=8)
   - Retain main orchestration logic
   - Call validation and routing helpers
   - Single responsibility: coordinate flow

Target Distribution: 4 + 4 + 8 = 16 total (vs current 19), with main method at <=8

## Boundary Definition

### IN SCOPE
- ONLY TryHandleFleetCommand method body
- Internal logic extraction to private helper methods
- Preserving exact behavior and return values
- Maintaining lock-free Actor/FSM pattern

### OUT OF SCOPE
- Callers of TryHandleFleetCommand (no changes)
- Callees invoked by TryHandleFleetCommand (no changes)
- Other methods in V12_002.UI.IPC.Commands.Fleet.cs (no changes)
- Class structure or field modifications (no changes)
- Test modifications (unless extraction breaks existing tests)

### No Scope Creep
- No "while we're here" improvements
- No fixing unrelated compilation errors
- No bundling multiple concerns
- No refactoring adjacent methods
- ONE EPIC = ONE CONCERN = ONE METHOD

## Success Criteria

### Functional Requirements
1. Complexity Reduction: Main method complexity reduced from 19 to <=8
2. Behavior Preservation: Zero behavior changes (bit-for-bit identical output)
3. Test Pass Rate: 100% of existing tests pass
4. Lock-Free Compliance: Actor/FSM pattern maintained (no lock() statements)

### Non-Functional Requirements
1. ASCII-Only: No Unicode, emoji, or curly quotes in extracted code
2. Build Success: Zero compilation errors after extraction
3. Performance: No measurable latency regression (IPC subsystem critical path)
4. Readability: Extracted helpers have clear, single-purpose names

### V12 DNA Compliance
- Correctness by Construction: Type-safe extraction
- Lock-Free Actor Pattern: Preserved in all extracted methods
- ASCII-Only: Verified in all new code
- Jane Street Alignment: CYC <=8 for cognitive simplicity

## Risk Assessment

### Overall Risk: LOW
- Single method extraction (well-understood pattern)
- No cross-file dependencies
- IPC subsystem isolated from core trading logic
- Complexity violation manageable (+11 over threshold)

### Mitigation Strategies
1. Checkpointing: Enabled via Bob CLI for rollback safety
2. Test-First: Run existing tests before and after extraction
3. Incremental: Extract one helper at a time, verify after each
4. Review: Arena AI adversarial audit before merge

---

Phase 1.0 Status: COMPLETED
Next Phase: Phase 1.5 (Boundary Validation)
Analyst: Bob CLI v12-engineer
Date: 2026-06-15
