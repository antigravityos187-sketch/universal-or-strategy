# Phase 1: Scope Definition - EPIC-W7-075

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.18
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T20:08:03Z
- **Input**: docs/brain/EPIC-W7-075/00-hotspots.md

## Target Method
- **Method**: OnSubmitClick
- **File**: src/V12_002.UI.Panel.Handlers.cs
- **Line**: 261
- **Current CYC**: 20
- **Target CYC**: 8 or less (60% reduction)

## Scope Boundary Analysis

### IN SCOPE: Extraction Candidates

#### 1. Config Mode Validation Logic
**Rationale**: Multiple conditional branches for config mode checks contribute significantly to CYC 20.
- Extract ValidateConfigMode() method
- Consolidate mode-specific validation rules
- Return strongly-typed validation result
- **Estimated CYC Reduction**: 4-6 points

#### 2. Input Validation Logic
**Rationale**: Input validation checks add branching complexity.
- Extract ValidateSubmitInputs() method
- Consolidate null checks, range checks, format validation
- Return validation result with error messages
- **Estimated CYC Reduction**: 3-4 points

#### 3. Command Building Logic
**Rationale**: Command construction logic can be isolated.
- Extract BuildPanelCommand() method
- Encapsulate command parameter assembly
- Separate command construction from execution
- **Estimated CYC Reduction**: 2-3 points

#### 4. Error Handling Paths
**Rationale**: Multiple error handling branches increase complexity.
- Extract HandleSubmitError() method
- Centralize error logging and UI feedback
- Simplify error propagation
- **Estimated CYC Reduction**: 2-3 points

### OUT OF SCOPE: Preserve in OnSubmitClick

#### 1. Event Handler Signature
**Rationale**: WPF/WinForms framework contract - cannot change signature.
- Keep: private void OnSubmitClick(object sender, RoutedEventArgs e)
- Preserve: Event handler registration and lifecycle

#### 2. TriggerGlow() Call
**Rationale**: Visual feedback is core UI responsibility, minimal complexity.
- Keep: Direct call to TriggerGlow() for button animation
- No extraction needed (simple, single-purpose call)

#### 3. PanelCommand() Delegation
**Rationale**: Already extracted, well-established pattern.
- Keep: Call to PanelCommand() for business logic execution
- No further extraction needed (proper separation of concerns)

#### 4. FSM/Actor Enqueue Pattern
**Rationale**: Thread-safe state mutation pattern - do not modify.
- Keep: Enqueue() calls for state mutations
- Preserve: Lock-free Actor pattern compliance

#### 5. UI Framework Integration
**Rationale**: Framework-specific code should remain in event handler.
- Keep: Sender/EventArgs handling
- Keep: UI control access (button state, panel controls)

## Extraction Strategy

### Phase 2 Architecture Plan
1. **Validation Layer**: Extract all validation logic into separate methods
   - ValidateConfigMode() returns ConfigModeValidationResult
   - ValidateSubmitInputs() returns InputValidationResult

2. **Command Layer**: Extract command building logic
   - BuildPanelCommand() returns PanelCommandRequest

3. **Error Handling Layer**: Extract error handling logic
   - HandleSubmitError() void method with error context

4. **Orchestration**: OnSubmitClick becomes thin orchestrator
   - Call validation methods (early returns on failure)
   - Call command builder
   - Call PanelCommand with built command
   - Call TriggerGlow for visual feedback

### Expected Outcome
- **Current CYC**: 20
- **Target CYC**: 8 or less
- **Reduction**: 60% (12+ points)
- **New Methods**: 4 extracted methods, each with CYC 8 or less

## Risk Mitigation

### Low Blast Radius Advantage
- **0 direct importers**: No external dependencies to update
- **UI event handler**: Isolated scope, framework-managed lifecycle
- **Internal refactoring**: Changes are implementation details

### Testing Strategy
- **Unit Tests**: Add tests for each extracted method
- **Integration Test**: Verify OnSubmitClick still triggers correct behavior
- **UI Test**: Verify button click still executes expected command

## Jane Street Alignment

### Cognitive Simplicity
- **Before**: CYC 20 means 1,048,576 potential paths
- **After**: CYC 8 or less means 256 potential paths (99.98% reduction)
- **Impact**: Dramatically easier to reason about execution paths

### Correctness by Construction
- **Opportunity**: Extract validation into strongly-typed result objects
- **Pattern**: Replace runtime checks with type-level guarantees where possible
- **Example**: ConfigModeValidationResult enum instead of boolean flags

### Lock-Free Pattern Compliance
- **Preserved**: All Enqueue() calls remain unchanged
- **Verified**: No lock() statements in scope
- **Maintained**: Thread-safe FSM/Actor pattern

## Success Criteria

### Phase 2 (Architecture Planning)
- Design strongly-typed validation result types
- Define method signatures for 4 extracted methods
- Create sequence diagram for orchestration flow
- Verify each extracted method targets CYC 8 or less

### Phase 5 (Ticket Execution)
- Extract 4 methods with CYC 8 or less each
- Reduce OnSubmitClick to CYC 8 or less
- Add unit tests for each extracted method
- Verify build passes
- Verify F5 in NinjaTrader succeeds

## Conclusion

**Scope Status**: APPROVED

OnSubmitClick is a well-bounded refactoring target with clear extraction candidates:
- **4 methods to extract**: Validation (2), Command Building (1), Error Handling (1)
- **Low risk**: 0 importers, isolated UI handler
- **High value**: 60% complexity reduction (CYC 20 to 8 or less)
- **Jane Street aligned**: Cognitive simplicity, type-level guarantees

**Next Phase**: Phase 2 (Architecture Planning) - Design extracted method signatures and orchestration flow.
