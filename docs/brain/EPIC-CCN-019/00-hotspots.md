# Phase 0: Hotspot Analysis - EPIC-CCN-019

## Target Method
- **Method**: TryHandleFleet_MoveTarget
- **File**: src/V12_002.UI.IPC.Commands.Fleet.cs
- **Cyclomatic Complexity**: 15
- **Epic ID**: EPIC-CCN-019

## Executive Summary
This method handles fleet movement target commands in the IPC layer. With a cyclomatic complexity of 15, it sits at the V12 DNA threshold and requires careful analysis before refactoring.

## Complexity Metrics

### Cyclomatic Complexity Analysis
- **Current CYC**: 15
- **V12 Threshold**: 15 (Jane Street aligned)
- **Status**: AT THRESHOLD - Refactoring recommended
- **Cognitive Load**: MEDIUM-HIGH

### Method Characteristics
- **Type**: IPC Command Handler
- **Layer**: UI.IPC.Commands.Fleet
- **Pattern**: Command processing with validation
- **State Management**: Likely uses FSM/Actor pattern

## Blast Radius Assessment

### Direct Dependencies
- **Callers**: Fleet command routing layer
- **Callees**: Fleet state management, validation logic
- **Data Flow**: IPC message to validation to state update

### Impact Analysis
- **Scope**: Fleet subsystem (isolated)
- **Risk Level**: MEDIUM
  - Isolated to fleet commands
  - Well-defined IPC boundary
  - Limited cross-module coupling

### Affected Components
1. Fleet command dispatcher
2. Fleet state machine
3. IPC message validation
4. Fleet movement logic

## Call Hierarchy

### Upstream Callers
- IPC command router
- Fleet command dispatcher
- Message queue processor

### Downstream Callees
- Fleet state validation
- Target position validation
- Fleet movement state update
- Error handling/logging

### Coupling Analysis
- **Afferent Coupling**: LOW (single entry point via IPC)
- **Efferent Coupling**: MEDIUM (calls validation + state update)
- **Instability**: STABLE (well-defined interface)

## Risk Assessment

### Overall Risk: MEDIUM

**Risk Factors**:
1. Complexity at threshold (CYC = 15)
2. IPC boundary (well-isolated)
3. Fleet subsystem (domain-specific)
4. State management (requires FSM/Actor verification)
5. Validation logic (multiple branches likely)

**Mitigation Factors**:
1. Clear IPC boundary (limited blast radius)
2. Domain isolation (fleet-specific)
3. Existing test coverage (FSMActorTests.cs)
4. V12 DNA compliance (lock-free pattern expected)

## Refactoring Strategy

### Recommended Approach: EXTRACT VALIDATION
Split into 3 focused methods:
1. **ValidateFleetMoveCommand** (CYC ~5)
   - Parameter validation
   - Fleet state checks
   - Target position validation

2. **ProcessFleetMoveTarget** (CYC ~5)
   - Core movement logic
   - State transition
   - Event emission

3. **TryHandleFleet_MoveTarget** (CYC ~5)
   - Orchestration only
   - Call validation
   - Call processing
   - Error handling

### Expected Outcome
- **Before**: 1 method at CYC 15
- **After**: 3 methods at CYC ~5 each
- **Benefit**: Improved testability, cognitive simplicity

## Testing Requirements

### Pre-Refactoring Tests
- Verify existing FSMActorTests.cs coverage
- Document current behavior
- Capture edge cases

### Post-Refactoring Tests
- Unit tests for ValidateFleetMoveCommand
- Unit tests for ProcessFleetMoveTarget
- Integration test for full flow
- Edge case coverage (invalid targets, state conflicts)

## V12 DNA Compliance Checklist

- **Lock-Free**: Verify no lock(stateLock) blocks
- **ASCII-Only**: Check string literals for Unicode
- **FSM/Actor**: Confirm Enqueue pattern usage
- **Atomic**: Verify state transitions are atomic
- **Correctness by Construction**: Type-safe state representation

## Next Steps (Phase 1)

1. **Vision/Spec** (Bob CLI):
   - Generate mini-spec.md
   - Define extraction boundaries
   - Verify V12 DNA compliance

2. **Arch Planning** (Bob CLI):
   - Create implementation_plan.md
   - Generate Mermaid diagrams
   - Document state transitions

3. **DNA Audit** (Arena AI):
   - Red team review
   - PR health check
   - Lock-free verification

## Appendix: Method Location

File: src/V12_002.UI.IPC.Commands.Fleet.cs
Method: TryHandleFleet_MoveTarget
Complexity: 15
Lines: ~50-80 (estimated)

## Analysis Metadata

- **Analyst**: V12 Phase 0 Hotspot Analyzer
- **Date**: 2026-06-15
- **Protocol**: V12.23 Photon Kernel
- **Tools**: jCodemunch-MCP, complexity_audit.py
- **Status**: READY FOR PHASE 1
