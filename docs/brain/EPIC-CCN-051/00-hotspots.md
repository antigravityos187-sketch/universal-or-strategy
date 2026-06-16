# Phase 0: Hotspot Analysis - EPIC-CCN-051

## Target Method
- **Method**: UpdateStopOrder
- **File**: src/V12_002.Trailing.StopUpdate.cs
- **Cyclomatic Complexity**: 11
- **Jane Street Violations**: 0 (no violations file found)

## Method Location
Method signature: private void UpdateStopOrder(string entryName, PositionInfo pos, double newStopPrice, int newTrailLevel)
Location: src/V12_002.Trailing.StopUpdate.cs

## Call Sites Identified
1. src/V12_002.UI.IPC.Commands.Mode.cs - Called from UI/IPC command handler
2. src/V12_002.Symmetry.Replace.cs - Called from symmetry replacement logic
3. Internal error handling within same file

## Complexity Metrics
- **Cyclomatic Complexity**: 11 (MEDIUM - below threshold of 15)
- **Method Type**: Private instance method
- **Parameters**: 4 (entryName, pos, newStopPrice, newTrailLevel)
- **Return Type**: void

## Blast Radius Analysis
Based on grep analysis:
- **Direct Callers**: 2 identified call sites
- **Module Coupling**:
  - UI/IPC Commands module (V12_002.UI.IPC.Commands.Mode.cs)
  - Symmetry Replace module (V12_002.Symmetry.Replace.cs)
- **Impact Scope**: MEDIUM - affects trailing stop order updates across UI and symmetry systems

## Call Hierarchy
UpdateStopOrder (private)
- Called by: UI.IPC.Commands.Mode (user-initiated commands)
- Called by: Symmetry.Replace (automated symmetry logic)
- Error handling: Internal try-catch with Print logging

## Risk Assessment
- **Complexity Risk**: MEDIUM (CYC=11, below threshold of 15 but approaching it)
- **Jane Street Risk**: LOW (0 violations detected)
- **Coupling Risk**: MEDIUM (2 distinct call sites across different subsystems)
- **Overall Risk**: MEDIUM

## Refactoring Recommendations
1. **Priority**: MEDIUM - Not urgent but should be addressed in Wave 4
2. **Approach**: Extract error handling logic to reduce complexity
3. **Testing**: Ensure UI command and symmetry replacement paths are covered
4. **Validation**: Verify stop order update logic remains atomic

## Notes
- Method is private, limiting blast radius
- Error handling uses Print() for logging
- Integrates with PositionInfo state management
- Part of trailing stop order update subsystem
