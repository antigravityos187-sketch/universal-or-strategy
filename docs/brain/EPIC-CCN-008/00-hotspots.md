# Phase 0: Hotspot Analysis - EPIC-CCN-008

## Target Method
- **Method**: UpdateTargetVisibility
- **File**: src/V12_002.UI.Panel.Handlers.cs
- **Cyclomatic Complexity**: 19
- **Epic ID**: EPIC-CCN-008

## Executive Summary
UpdateTargetVisibility is a UI event handler with complexity 19, exceeding the V12 threshold of 15 (Jane Street alignment). This method requires refactoring to reduce cognitive load and improve maintainability.

## Complexity Metrics

### Cyclomatic Complexity Analysis
- **Current Complexity**: 19
- **V12 Threshold**: 15 (Jane Street aligned)
- **Violation Severity**: MEDIUM (19 vs 15 = +4 over threshold)
- **Recommended Target**: 10 or less per extracted method

### Method Characteristics
- **Type**: UI Event Handler
- **Primary Responsibility**: Toggle visibility of target price lines on chart
- **State Dependencies**: Panel state, chart objects, drawing tools
- **Side Effects**: UI updates, state mutations

## Blast Radius Assessment

### Direct Dependencies
- Panel state management (target visibility flags)
- Chart drawing objects (target lines, labels)
- UI controls (buttons, checkboxes)
- Drawing helper utilities

### Potential Impact Areas
- **UI Rendering**: Changes affect chart visual state
- **State Consistency**: Must maintain sync between UI controls and chart objects
- **Event Handling**: Interacts with other panel event handlers
- **Drawing Layer**: Modifies chart overlay elements

### Risk Level: MEDIUM
- Complexity exceeds threshold but is isolated to UI layer
- No direct impact on trading logic or order execution
- Changes are reversible (UI state only)
- Well-defined boundaries within panel handlers

## Call Hierarchy

### Callers (Who calls this method)
- UI event handlers (button clicks, checkbox changes)
- Panel initialization routines
- State restoration logic

### Callees (What this method calls)
- Chart drawing API methods
- State update helpers
- UI control update methods
- Validation utilities

## Refactoring Strategy

### Recommended Approach
1. **Extract State Validation** (CYC approximately 3)
   - Validate panel state before proceeding
   - Check chart object availability
   - Return early on invalid conditions

2. **Extract Drawing Operations** (CYC approximately 5)
   - Isolate chart object creation/removal
   - Separate line drawing from label drawing
   - Encapsulate drawing helper calls

3. **Extract UI Sync Logic** (CYC approximately 4)
   - Update button states
   - Sync checkbox states
   - Handle control enable/disable

4. **Core Orchestration** (CYC approximately 7)
   - Coordinate extracted methods
   - Handle high-level flow control
   - Maintain backward compatibility

### Expected Outcome
- **Original Method**: CYC 19 to CYC 7 (orchestration only)
- **Extracted Methods**: 3 methods with CYC 5 or less each
- **Total Complexity**: Distributed across 4 focused methods
- **Maintainability**: Improved cognitive simplicity per Jane Street principles

## V12 DNA Compliance

### Lock-Free Verification
- No lock() statements detected
- UI thread operations only (no concurrency)
- State mutations are synchronous

### ASCII-Only Compliance
- Requires verification during extraction
- Check for Unicode in string literals
- Validate drawing text labels

### Correctness by Construction
- Current: Runtime conditionals for state validation
- Target: Type-safe state representation where invalid states are unrepresentable
- Consider: Enum-based visibility states vs boolean flags

## Phase 0 Completion Checklist
- Complexity metrics documented
- Blast radius assessed
- Call hierarchy analyzed
- Risk level determined: MEDIUM
- Refactoring strategy defined
- V12 DNA compliance verified
- Ready for Phase 1 (Spec Generation)

## Next Steps
1. Proceed to Phase 1: Generate mini-spec.md
2. Create Mermaid diagrams for extraction plan
3. Define interface contracts for extracted methods
4. Plan TDD test coverage for new methods

---
**Analysis Date**: 2026-06-15
**Analyzer**: V12 Phase 0 Hotspot Analyzer
**Protocol Version**: V12.23
