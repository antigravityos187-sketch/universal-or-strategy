# Phase 0: Hotspot Analysis - EPIC-W7-031

## Agent Tracking
- **Agent Name**: v12-phase0-hotspot
- **Bobcoins Used**: 0.75
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-23T02:40:27Z

## Target Method
- **Method**: AuditMaster_HandleNakedPosition
- **File**: src/V12_002.REAPER.Audit.cs
- **Line**: 624
- **Cyclomatic Complexity**: 19 (Target: ≤8)
- **Signature**: private void AuditMaster_HandleNakedPosition(Position masterPos, int masterActualQty, string masterExpectedKey)

## Complexity Metrics

### Raw Metrics
- **Cyclomatic Complexity**: 19
- **Max Nesting Depth**: 7
- **Parameter Count**: 3
- **Lines of Code**: 56
- **Assessment**: HIGH

### Analysis
The method exceeds the Jane Street strict standard (CYC ≤8) by 11 points. With 7 levels of nesting and 56 lines, this is a clear extraction candidate. The high complexity indicates multiple decision paths that should be decomposed into smaller, single-responsibility methods.

## Blast Radius

### Direct Impact
- **Importer Count**: 0
- **Direct Dependents**: 0
- **Overall Risk Score**: 0.0
- **Confirmed Files**: 0
- **Potential Files**: 0

### Analysis
**LOW RISK**: This method has zero external dependencies. No other files import or directly depend on this method, making it an ideal candidate for refactoring. Changes will be isolated to the containing file.

## Call Hierarchy

### Callers (Who calls this method)
1. **AuditMasterAccountIfNeeded** (depth 1)
   - File: src/V12_002.REAPER.Audit.cs:684
   - Resolution: ast_resolved
   
2. **AuditApexPositions** (depth 2)
   - File: src/V12_002.REAPER.Audit.cs:16
   - Resolution: ast_resolved

### Callees (What this method calls) - 22 total
Key dependencies:
1. **EnqueueReaperMasterNakedStop** (depth 1) - Primary action method
2. **LogBuffer.Format** (depth 1) - Logging
3. **_nakedPositionFirstSeen** (depth 1) - State tracking
4. **_reaperNakedStopInFlight** (depth 1) - State tracking
5. **ProcessReaperNakedStopQueue** (depth 1) - Queue processing
6. **LogBuffer.ValidateThreadAffinity** (depth 2) - Thread safety
7. **LogBuffer.FormatInternal** (depth 2) - Internal logging
8. **_reaperNakedStopQueue** (depth 2) - Queue state
9. **ClearNakedStopInFlight** (depth 2) - State cleanup
10. **ExpKey** (depth 2) - Key calculation
11. **CalculateEmergencyStopPrice** (depth 2) - Price calculation

### Call Graph Analysis
The method has a deep call tree (depth 2) with 22 callees, indicating it orchestrates multiple subsystems:
- Logging subsystem (LogBuffer)
- State tracking (_nakedPositionFirstSeen, _reaperNakedStopInFlight)
- Queue management (_reaperNakedStopQueue, ProcessReaperNakedStopQueue)
- Emergency stop logic (EnqueueReaperMasterNakedStop, CalculateEmergencyStopPrice)

This suggests the method is doing too much and should be decomposed.

## Risk Assessment

### Overall Risk: MEDIUM

**Factors**:
- ✅ **LOW** blast radius (0 external dependents)
- ❌ **HIGH** complexity (CYC 19, nesting 7)
- ⚠️ **MEDIUM** call depth (22 callees across 2 levels)
- ✅ **LOW** caller count (only 2 callers, both internal)

### Refactoring Safety
**SAFE TO REFACTOR**: 
- Isolated method with no external dependencies
- Only 2 internal callers (easy to verify)
- Clear extraction candidates visible in call hierarchy
- No cross-file impact

### Recommended Approach
1. Extract emergency stop logic (EnqueueReaperMasterNakedStop + CalculateEmergencyStopPrice)
2. Extract state tracking logic (_nakedPositionFirstSeen, _reaperNakedStopInFlight)
3. Extract logging logic (LogBuffer calls)
4. Reduce nesting depth from 7 to ≤3
5. Target: 3-4 extracted methods, each with CYC ≤8

## Summary

**AuditMaster_HandleNakedPosition** is a HIGH complexity method (CYC 19) with LOW blast radius (0 dependents). It orchestrates naked position detection and emergency stop logic across multiple subsystems. The method is SAFE to refactor due to its isolation, with clear extraction opportunities in emergency stop logic, state tracking, and logging. Target decomposition: 3-4 methods, each CYC ≤8.
