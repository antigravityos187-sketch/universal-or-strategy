# Phase 0: Hotspot Analysis - EPIC-005

## Epic Overview
- **Epic ID**: EPIC-005
- **Target File**: src/V12_002.SIMA.Flatten.cs
- **Target Methods**: 2 methods requiring complexity reduction
- **Goal**: Reduce all methods to CYC <= 8 (Jane Street alignment)

## Target Methods

### Method 1: EmergencyFlattenSingleFleetAccount
- **Current Complexity**: 16
- **Target Complexity**: <= 8
- **Reduction Required**: 8 points (50% reduction)
- **Priority**: HIGH (exceeds threshold by 100%)

### Method 2: FlattenFleetPosition
- **Current Complexity**: 9
- **Target Complexity**: <= 8
- **Reduction Required**: 1 point (11% reduction)
- **Priority**: MEDIUM (slightly exceeds threshold)

## Complexity Analysis

### EmergencyFlattenSingleFleetAccount (CYC: 16)
**Risk Level**: HIGH

**Complexity Drivers**:
- Multiple conditional branches for emergency flatten logic
- Fleet account state validation
- Position aggregation and flattening operations
- Error handling and state transitions

**Refactoring Strategy**:
- Extract position validation logic into separate method
- Extract fleet account state checks into guard methods
- Extract flattening calculation logic
- Simplify conditional branches using early returns

**Estimated Extraction Points**: 3-4 methods

### FlattenFleetPosition (CYC: 9)
**Risk Level**: MEDIUM

**Complexity Drivers**:
- Position state validation
- Flatten operation logic
- Single conditional branch exceeding threshold

**Refactoring Strategy**:
- Extract position state validation
- Simplify flatten operation logic

**Estimated Extraction Points**: 1-2 methods

## Blast Radius Assessment

### File-Level Impact
- **File**: src/V12_002.SIMA.Flatten.cs
- **Module**: SIMA (State-Indexed Market Adapter)
- **Subsystem**: Flatten operations

### Method Dependencies
**EmergencyFlattenSingleFleetAccount**:
- Called by: Emergency flatten handlers
- Calls: Position aggregation methods, state validators
- Impact: HIGH - Core emergency flatten logic

**FlattenFleetPosition**:
- Called by: Fleet position management
- Calls: Position state methods
- Impact: MEDIUM - Standard flatten operations

### Risk Factors
1. **Emergency Logic**: EmergencyFlattenSingleFleetAccount handles critical emergency scenarios
2. **State Consistency**: Both methods manage position state transitions
3. **Fleet Coordination**: Methods coordinate multi-position operations
4. **Lock-Free Requirements**: Must maintain FSM/Actor pattern (no locks)

## Success Criteria

- EmergencyFlattenSingleFleetAccount: CYC <= 8
- FlattenFleetPosition: CYC <= 8
- All tests passing
- No lock() statements introduced
- ASCII-only compliance maintained
- Build verification successful
- Hard-link sync completed

---
**Analysis Date**: 2026-06-14
**Analyzer**: V12 Phase 0 Hotspot Analyzer
**Status**: READY FOR PHASE 1
