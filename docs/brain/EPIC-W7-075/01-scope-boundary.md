# Phase 1: Scope Boundary - EPIC-W7-075

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.00
- **API Key**: N/A
- **Execution Time**: 2026-06-24T01:33:01Z

## Epic Summary
- **Target Method**: OnSubmitClick
- **File**: src/V12_002.UI.Panel.Handlers.cs
- **Current CYC**: 20
- **Target CYC**: ≤ 8
- **Reduction Goal**: 60% (CYC 20 → 8)

## Scope Definition

### IN SCOPE

#### Primary Extraction Targets
1. **Config Mode Validation Logic**
   - Extract GetCurrentConfigMode() calls and validation
   - Create ValidateConfigMode() helper method
   - Expected CYC reduction: 3-4 points

2. **Input Validation**
   - Extract UI input validation checks
   - Create ValidateSubmitInputs() helper method
   - Expected CYC reduction: 2-3 points

3. **Command Building Logic**
   - Extract command parameter construction
   - Create BuildPanelCommand() helper method
   - Expected CYC reduction: 3-4 points

4. **Control Flow Simplification**
   - Apply early returns and guard clauses
   - Eliminate nested conditionals
   - Expected CYC reduction: 2-3 points

#### Files to Modify
- `src/V12_002.UI.Panel.Handlers.cs` (primary target)

#### Success Criteria
- OnSubmitClick reduced from CYC 20 to ≤ 8
- All extracted methods have CYC ≤ 8
- No lock() statements introduced
- FSM/Actor Enqueue pattern preserved
- All existing tests pass
- F5 in NinjaTrader successful

### OUT OF SCOPE

#### Excluded from This Epic
1. **PanelCommand() Method**
   - Called by OnSubmitClick but not modified
   - Separate refactoring target (CYC unknown)
   - Reason: Blast radius isolation

2. **TriggerGlow() Method**
   - Visual feedback mechanism
   - Already simple (likely CYC ≤ 8)
   - Reason: No complexity issue

3. **GetCurrentConfigMode() Method**
   - Config state retrieval
   - Already extracted in separate file
   - Reason: Already modular

4. **FSM/Actor Enqueue Pattern**
   - Core threading model
   - No modifications needed
   - Reason: Already compliant with V12 DNA

5. **UI Framework Integration**
   - Event handler wiring
   - WPF/WinForms callbacks
   - Reason: Framework-level concern

6. **Other UI Handlers**
   - OnCancelClick, OnResetClick, etc.
   - Separate refactoring targets
   - Reason: One epic = one concern

#### Architectural Boundaries
- **No changes to**: FSM/Actor pattern
- **No changes to**: IPC communication layer
- **No changes to**: UI panel lifecycle
- **No changes to**: Thread synchronization primitives

## Risk Mitigation

### Low Blast Radius Confirmed
- No direct importers (blast radius = 0)
- Isolated UI event handler
- Changes contained to single method

### Testing Strategy
1. Unit tests for extracted helper methods
2. Integration test: F5 in NinjaTrader
3. Verify BUILD_TAG appears in output
4. Manual UI testing: Submit button functionality

### Rollback Plan
- Git branch: `epic-w7-075-onsubmitclick`
- Checkpoint before extraction
- Revert via `git reset --hard` if F5 fails

## Complexity Reduction Roadmap

### Current State
```
OnSubmitClick (CYC 20)
├─ Config mode validation (CYC ~4)
├─ Input validation (CYC ~3)
├─ Command building (CYC ~4)
├─ Error handling (CYC ~3)
└─ Visual feedback (CYC ~2)
```

### Target State
```
OnSubmitClick (CYC ≤ 8)
├─ ValidateConfigMode() → CYC ≤ 8
├─ ValidateSubmitInputs() → CYC ≤ 8
├─ BuildPanelCommand() → CYC ≤ 8
└─ Simplified control flow
```

## Jane Street Alignment

### Cognitive Simplicity
- **Before**: CYC 20 (2^20 = 1M paths)
- **After**: CYC ≤ 8 (2^8 = 256 paths)
- **Improvement**: 99.98% reduction in path complexity

### Lock-Free Compliance
- Preserve Enqueue() pattern
- No lock() statements
- Thread-safe by design

### Correctness by Construction
- Extract validation into strongly-typed helpers
- Use early returns to eliminate invalid states
- Make illegal states unrepresentable

## Phase 1 Completion Checklist
- [x] Hotspot analysis reviewed
- [x] IN SCOPE targets identified (4 extraction candidates)
- [x] OUT OF SCOPE boundaries defined (6 exclusions)
- [x] Success criteria established (CYC 20 → ≤ 8)
- [x] Risk mitigation planned (low blast radius)
- [x] Testing strategy defined (unit + integration)
- [x] Jane Street alignment verified (cognitive simplicity)

## Next Phase
Proceed to Phase 2 (Architecture Planning) to design extraction signatures and call patterns.
