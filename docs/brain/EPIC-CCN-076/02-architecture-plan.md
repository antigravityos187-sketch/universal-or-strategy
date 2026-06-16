# Phase 2: Architecture Planning - EPIC-CCN-076

## Target Method Analysis

### Current State
- **Method**: CollapseAllExecutionControls
- **File**: src/V12_002.UI.Panel.Handlers.cs
- **Line Range**: 665-686 (21 lines)
- **Current Complexity**: 11 (CYC)
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **Tier**: 2 (Medium complexity)

### Method Signature (Original)
private void CollapseAllExecutionControls()

## Extraction Strategy

### Complexity Analysis
The method contains 11 sequential if-statements that check UI controls and set their Visibility property. The logic naturally divides into three categories:

1. **Execution Rows** (2 controls): execRetestRow, execTrendRow
2. **Execution Buttons** (7 controls): rmaButton, momoButton, ffmaButton, ffmaManualButton, mButton, orLongButton, orShortButton
3. **Manual Entry Row** (1 control): manualEntryRow (set to Visible, not Collapsed)

### Proposed Extraction
Extract two private helper methods to reduce main method complexity from 11 to 3:

**Helper 1: CollapseExecutionRows**
- Responsibility: Collapse execution row UI elements
- Complexity: 2 (CYC)
- Controls: execRetestRow, execTrendRow

**Helper 2: CollapseExecutionButtons**
- Responsibility: Collapse execution button UI elements
- Complexity: 7 (CYC)
- Controls: rmaButton, momoButton, ffmaButton, ffmaManualButton, mButton, orLongButton, orShortButton

**Main Method (Refactored)**
- Complexity: 3 (CYC)
- Operations: Call helper 1, call helper 2, handle manual entry row

### Complexity Validation
- ✅ Main method: CYC 3 ≤ 8
- ✅ Helper 1: CYC 2 ≤ 8
- ✅ Helper 2: CYC 7 ≤ 8
- ✅ Total reduction: 11 → 3 (main method)

## Method Signatures

### Helper Method 1: CollapseExecutionRows
Collapses execution row UI elements (retest and trend rows).
private void CollapseExecutionRows()

**Parameters**: None (accesses instance fields)
**Return Type**: void
**Access Modifier**: private
**Complexity**: 2 (CYC)

### Helper Method 2: CollapseExecutionButtons
Collapses execution button UI elements (RMA, MOMO, FFMA, M, OR Long/Short).
private void CollapseExecutionButtons()

**Parameters**: None (accesses instance fields)
**Return Type**: void
**Access Modifier**: private
**Complexity**: 7 (CYC)

### Refactored Main Method
Collapses all execution control UI elements and shows manual entry row.
private void CollapseAllExecutionControls()

**Parameters**: None
**Return Type**: void
**Access Modifier**: private (unchanged)
**Complexity**: 3 (CYC) - reduced from 11

## Call Graph

Execution Flow:
1. Main method calls CollapseExecutionRows()
2. Main method calls CollapseExecutionButtons()
3. Main method handles manualEntryRow directly (set to Visible)

Data Flow:
- No parameters passed between methods
- No return values from helper methods
- Instance fields accessed directly by each method
- No shared mutable state between helpers (each accesses distinct UI controls)

## Lock-Free Validation

### ✅ No lock() Statements
- Original method: No locks present
- Helper methods: No locks introduced
- Main method: No locks required

### ✅ WPF Dispatcher Model
- All UI control access occurs on the UI thread
- WPF dispatcher model ensures thread safety
- No explicit synchronization needed for UI property access

### ✅ No Shared Mutable State
- Each helper accesses its own subset of UI controls
- No data races possible (UI thread affinity)
- No atomic primitives required (single-threaded UI operations)

### V12 DNA Compliance
- **Lock-Free**: ✅ No lock() statements
- **FSM/Actor Pattern**: N/A (UI event handler, not state machine)
- **Atomic Primitives**: N/A (WPF dispatcher handles synchronization)
- **Thread Safety**: ✅ WPF UI thread affinity model

## Jane Street Compliance

### Cognitive Simplicity (CYC ≤8)
Jane Street HFT systems prioritize functions simple enough to reason about under microsecond latency constraints:

- ✅ **Main method**: CYC 3 (trivial to reason about)
- ✅ **CollapseExecutionRows**: CYC 2 (trivial to reason about)
- ✅ **CollapseExecutionButtons**: CYC 7 (within cognitive limit)

### Single Responsibility Principle
Each method has one clear responsibility:
- **Main method**: Orchestrate UI collapse sequence
- **Helper 1**: Collapse row controls
- **Helper 2**: Collapse button controls

### Make Illegal States Unrepresentable
The extraction enforces logical grouping:
- Impossible to accidentally skip collapsing a category of controls
- Clear separation between rows, buttons, and manual entry
- Type system (method boundaries) prevents mixing concerns

### Testability
Each helper can be tested independently:
- **CollapseExecutionRows**: Test with 2 row controls
- **CollapseExecutionButtons**: Test with 7 button controls
- **Main method**: Test orchestration logic

## Verification Criteria

### Pre-Extraction Metrics
- Complexity: 11 (CYC)
- LOC: 21
- Methods: 1

### Post-Extraction Metrics
- Main method complexity: 3 (CYC)
- Helper 1 complexity: 2 (CYC)
- Helper 2 complexity: 7 (CYC)
- Total LOC: ~27 (includes XML docs)
- Methods: 3

### Success Criteria
- ✅ All methods have CYC ≤8
- ✅ No lock() statements introduced
- ✅ Behavior preservation (100% test pass)
- ✅ Zero compilation errors
- ✅ CSharpier formatting compliance
- ✅ Hard-link integrity maintained (deploy-sync.ps1)

## Risk Assessment

### Technical Risk: MINIMAL
- Simple extraction with no logic changes
- No API signature changes
- No caller modifications required

### Regression Risk: LOW
- Behavior preservation enforced
- Existing tests will validate correctness
- WPF UI thread model unchanged

### Integration Risk: NONE
- No changes to method signature
- No changes to callers
- No changes to callees

## Next Steps
1. Proceed to Phase 3: DNA & PR Audit (Adjudicator)
2. Submit plan for Triple-Agent UltraThink audit
3. Upon approval, proceed to Phase 4: Recursive Execution

## Validation Timestamp
- **Date**: 2026-06-15
- **Protocol Version**: V12.23
- **Planner**: Bob CLI (v12-engineer mode)
- **Status**: READY FOR AUDIT
