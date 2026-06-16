# Phase 0: Hotspot Analysis - EPIC-011

## Target Methods
- **Method 1**: TryHandleFleetCommand
- **Method 2**: TryHandleFleet_CancelAll
- **File**: src/V12_002.UI.IPC.Commands.Fleet.cs
- **Cyclomatic Complexity**: 19, 17, 15, 14, 12, 11 (Target: all <=8)

## Complexity Metrics

### TryHandleFleetCommand
- **Cyclomatic Complexity**: 19
- **Status**: HIGH RISK - Exceeds V12 DNA threshold (<=15)
- **Target**: Reduce to <=8 (Jane Street alignment)

### TryHandleFleet_CancelAll
- **Cyclomatic Complexity**: 17
- **Status**: HIGH RISK - Exceeds V12 DNA threshold (<=15)
- **Target**: Reduce to <=8 (Jane Street alignment)

### Additional Methods in File
- Multiple methods with complexity 15, 14, 12, 11
- All require refactoring to meet <=8 threshold

## Blast Radius Analysis

### Impact Assessment
- **File**: src/V12_002.UI.IPC.Commands.Fleet.cs
- **Domain**: UI IPC Commands - Fleet Management
- **Risk Level**: HIGH
  - Fleet command handling is critical for multi-strategy coordination
  - IPC layer changes affect cross-process communication
  - Command parsing errors could cascade to all fleet operations

### Dependencies
- UI layer integration
- IPC command protocol
- Fleet state management
- Error handling and validation logic

## Call Hierarchy

### TryHandleFleetCommand
- **Role**: Primary fleet command dispatcher
- **Callers**: IPC command router (likely from UI process)
- **Callees**: Fleet state validators, command parsers, error handlers, state mutation methods

### TryHandleFleet_CancelAll
- **Role**: Emergency fleet cancellation handler
- **Callers**: Fleet command dispatcher
- **Callees**: Fleet state accessors, cancellation propagation logic, cleanup routines

## Risk Assessment: HIGH

### Risk Factors
1. **Complexity Overload**: CYC 19 and 17 far exceed Jane Street threshold (<=8)
2. **Cognitive Load**: Multiple decision paths make reasoning difficult
3. **Testing Burden**: Exponential path growth (2^19 and 2^17 potential paths)
4. **Race Condition Risk**: Complex state mutations in IPC context
5. **Maintainability**: High churn + high complexity = hotspot

### V12 DNA Violations
- Complexity exceeds threshold (19, 17 vs <=15 mandate)
- Not "Make illegal states unrepresentable" (too many branches)
- Potential lock-free violations (needs audit)
- ASCII-only compliance (needs verification)

## Refactoring Strategy

### Extraction Targets
1. **Command Validation Logic** (CYC reduction: ~5)
2. **State Transition Guards** (CYC reduction: ~4)
3. **Error Handling Paths** (CYC reduction: ~3)
4. **Fleet State Queries** (CYC reduction: ~3)

### Expected Outcome
- TryHandleFleetCommand: 19 to <=8 (extract 3-4 methods)
- TryHandleFleet_CancelAll: 17 to <=8 (extract 2-3 methods)
- Improved testability (unit tests per extracted method)
- Reduced cognitive load (single-purpose functions)

## Next Steps (Phase 1)
1. Generate mini-spec.md with Director dialogue
2. Map exact decision tree for both methods
3. Identify atomic extraction boundaries
4. Verify lock-free compliance in extracted logic
5. Plan TDD test coverage for extracted methods

## Hotspot Priority: P0 (Critical)
- High complexity + IPC layer = system-wide impact
- Fleet commands are mission-critical for multi-strategy ops
- Refactoring required before any fleet feature additions
