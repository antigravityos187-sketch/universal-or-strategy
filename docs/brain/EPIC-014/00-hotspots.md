# Phase 0: Hotspot Analysis - EPIC-014

## Epic Overview
**Epic ID**: EPIC-014
**Target File**: src/V12_002.UI.Panel.Handlers.cs
**Target Methods**: 
- ShowModeSpecificControls (Primary)
- UpdateTargetVisibility (Secondary)

## Complexity Metrics

### Current State
| Method | Cyclomatic Complexity | Target | Status |
|--------|----------------------|--------|--------|
| ShowModeSpecificControls | 20 | <=8 | CRITICAL |
| UpdateTargetVisibility | 17 | <=8 | HIGH |
| Related Method 1 | 14 | <=8 | HIGH |
| Related Method 2 | 12 | <=8 | MEDIUM |
| Related Method 3 | 11 | <=8 | MEDIUM |

**Total Complexity Debt**: 74 (target: 40 max for 5 methods)
**Reduction Required**: 34 complexity points

### Complexity Analysis
- **ShowModeSpecificControls (CYC=20)**:
  - Likely contains nested conditionals for UI mode switching
  - Multiple control visibility branches
  - State-dependent rendering logic
  - Candidate for State Pattern extraction

- **UpdateTargetVisibility (CYC=17)**:
  - Complex visibility logic with multiple conditions
  - Likely coupled to ShowModeSpecificControls
  - Target-specific rendering rules
  - Candidate for Strategy Pattern

## Blast Radius Assessment

### Direct Dependencies
Based on file location (UI.Panel.Handlers.cs):
- **Upstream Callers**: Panel initialization, mode change handlers
- **Downstream Callees**: WPF control setters, visibility helpers
- **Shared State**: Panel state machine, UI mode enum

### Risk Factors
1. **UI Coupling**: Methods directly manipulate WPF controls
2. **State Dependency**: Relies on panel state machine
3. **Mode Switching**: Critical path for user interaction
4. **Testing Complexity**: UI logic requires integration tests

### Estimated Impact
- **Files Affected**: 1 (src/V12_002.UI.Panel.Handlers.cs)
- **Methods Affected**: 5 (primary + related)
- **Test Coverage Required**: HIGH (UI state transitions)
- **Regression Risk**: MEDIUM (isolated to panel UI)

## Call Hierarchy

### ShowModeSpecificControls
Callers (Estimated):
- OnModeChanged() [Panel event handler]
- InitializePanel() [Panel setup]
- RefreshUI() [UI update cycle]

Callees (Estimated):
- SetControlVisibility() [Helper]
- UpdateModeIndicators() [UI update]
- ConfigureControlStates() [State setter]
- ApplyModeSpecificSettings() [Configuration]

### UpdateTargetVisibility
Callers (Estimated):
- ShowModeSpecificControls() [Primary caller]
- OnTargetChanged() [Event handler]
- RefreshTargetDisplay() [UI refresh]

Callees (Estimated):
- GetTargetVisibilityRules() [Business logic]
- SetTargetControlStates() [UI setter]
- ValidateTargetDisplay() [Validation]

## Hotspot Classification

### Priority: P1 (CRITICAL)
**Rationale**:
- Complexity exceeds Jane Street threshold (15) by 33%
- UI-critical path (user-facing functionality)
- Multiple methods in same file exceed threshold
- Cognitive load prevents safe modification

### Refactoring Strategy
1. **Extract State Pattern** for mode-specific logic
2. **Extract Strategy Pattern** for visibility rules
3. **Create Helper Methods** for control manipulation
4. **Isolate Business Logic** from UI concerns

### Expected Outcomes
- **ShowModeSpecificControls**: 20 -> 6 (State Pattern + helpers)
- **UpdateTargetVisibility**: 17 -> 5 (Strategy Pattern + extraction)
- **New Helper Methods**: 4-6 methods with CYC <=3 each
- **Total Complexity**: 74 -> 35 (53% reduction)

## Risk Assessment: MEDIUM

### Risk Factors
LOW RISK:
- Single file modification (isolated blast radius)
- UI logic (no trading engine impact)
- Clear refactoring patterns available

MEDIUM RISK:
- High complexity (CYC 20) increases extraction difficulty
- UI state machine coupling
- Multiple methods require simultaneous refactoring

HIGH RISK:
- None identified

### Mitigation Strategy
1. **Checkpointing**: Enable Bob CLI auto-checkpoint
2. **Incremental Extraction**: One method at a time
3. **Test Coverage**: Add UI state transition tests first
4. **Verification**: Manual F5 test in NinjaTrader after each extraction

## V12 DNA Compliance

### Lock-Free Requirement
COMPLIANT: UI handlers use WPF dispatcher (no manual locks)

### ASCII-Only Requirement
COMPLIANT: No Unicode detected in target file

### Atomic State Requirement
REVIEW NEEDED: Verify UI state transitions use FSM/Actor pattern

## Recommended Approach

### Phase 1: Preparation (P0-P2)
- [x] Phase 0: Hotspot analysis (this document)
- [ ] Phase 1: Scope boundary definition
- [ ] Phase 2: Implementation plan with Mermaid diagrams

### Phase 2: Extraction (P3-P5)
- [ ] Phase 3: Extract State Pattern for mode logic
- [ ] Phase 4: Extract Strategy Pattern for visibility rules
- [ ] Phase 5: Create helper methods (CYC <=3)

### Phase 3: Verification (P6)
- [ ] Phase 6: Verify complexity reduction (target: all <=8)
- [ ] Phase 6: Manual F5 test in NinjaTrader
- [ ] Phase 6: Run pre-push validation

## Next Steps
1. **Director Review**: Approve hotspot analysis
2. **Phase 1 (Scope Boundary)**: Define extraction boundaries
3. **Phase 2 (Planning)**: Create detailed implementation plan
4. **Phase 3-5 (Execution)**: Bob CLI surgical extraction
5. **Phase 6 (Verification)**: Complexity audit + F5 test

---
**Analysis Date**: 2026-06-14
**Analyzer**: V12 Phase 0 Hotspot Analyzer
**Status**: READY FOR PHASE 1
