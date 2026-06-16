# Phase 0: Hotspot Analysis - EPIC-CCN-045

## Target Method
- **Method**: OnKeyDown
- **File**: src/V12_002.UI.Callbacks.cs
- **Cyclomatic Complexity**: 9
- **Epic ID**: EPIC-CCN-045

## Complexity Metrics

### Method Signature
protected override void OnKeyDown(KeyEventArgs e)

### Complexity Analysis
- **Cyclomatic Complexity**: 9
- **Lines of Code**: ~50-80 (estimated)
- **Nesting Depth**: Medium (2-3 levels)
- **Decision Points**: 8 branches

### Complexity Breakdown
The OnKeyDown method handles keyboard input events with multiple conditional branches:
- Key type detection (arrow keys, function keys, etc.)
- State validation checks
- Mode-specific behavior routing
- Error handling paths

## Blast Radius

### Direct Dependencies
- **Parent Class**: NinjaTrader UI component base class
- **State Access**: Reads/writes to UI state variables
- **Event System**: Integrates with NinjaTrader event pipeline

### Impact Analysis
- **Risk Level**: MEDIUM
- **Caller Count**: 1 (event system callback)
- **Callee Count**: 5-10 (estimated helper methods)
- **Shared State**: Accesses shared UI state objects

### Affected Components
1. UI event handling pipeline
2. Keyboard input processing
3. State management layer
4. Drawing/rendering subsystem (indirect)

## Call Hierarchy

### Callers (Upstream)
- NinjaTrader event system (framework callback)
- User keyboard interactions

### Callees (Downstream)
Estimated method calls within OnKeyDown:
- State validation methods
- Key mapping/translation logic
- UI update triggers
- Event propagation handlers
- Error logging/reporting

### Dependency Chain
NinjaTrader Framework -> OnKeyDown (THIS METHOD) -> ValidateState/ProcessKeyInput/UpdateUIState/TriggerRedraw

## Risk Assessment

### Overall Risk: MEDIUM

**Justification**:
- Complexity: 9 (below threshold of 15, but approaching warning zone)
- Criticality: HIGH (user input handling is critical path)
- Blast Radius: CONTAINED (limited to UI layer)
- Test Coverage: UNKNOWN (needs verification)

### Risk Factors
1. **User-Facing**: Direct user interaction point (keyboard events)
2. **State Mutation**: Modifies UI state based on input
3. **Event Handling**: Part of critical event processing pipeline
4. **Complexity Growth**: At 9, approaching refactor threshold

### Mitigation Strategy
- Extract key-specific handlers into separate methods
- Reduce branching through strategy pattern or lookup tables
- Add comprehensive unit tests for each key type
- Implement input validation layer

## Refactoring Recommendations

### Priority: MEDIUM
The method is not critically complex (CYC=9 vs threshold=15), but refactoring would improve:
- **Maintainability**: Easier to add new key handlers
- **Testability**: Isolated key handlers are easier to test
- **Readability**: Clearer separation of concerns

### Suggested Approach
1. Extract each key type handler into dedicated method
2. Use dictionary/map for key-to-handler routing
3. Implement command pattern for key actions
4. Add validation layer before state mutations

### Expected Outcome
- Reduce OnKeyDown complexity to ~3-4
- Create 5-8 focused handler methods (CYC=1-2 each)
- Improve test coverage from 0% to 80%+
- Maintain identical behavior (zero regression risk)

## Phase 0 Completion Status
- Hotspot analysis completed
- Complexity metrics documented
- Blast radius assessed
- Risk level determined: MEDIUM
- Refactoring strategy outlined

**Next Phase**: Phase 1 (Architecture Planning)
