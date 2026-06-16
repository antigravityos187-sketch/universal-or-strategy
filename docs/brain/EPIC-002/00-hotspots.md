# Phase 0: Hotspot Analysis - EPIC-002

## Epic Overview
**Epic ID**: EPIC-002
**Target File**: src/V12_002.Orders.Management.Flatten.cs
**Phase**: 0 (Hotspot Analysis)
**Date**: 2026-06-14

## Target Methods

### Method 1: ManageCIT
- **Cyclomatic Complexity**: 19
- **Lines of Code**: 77
- **Target Complexity**: <=8
- **Reduction Required**: 11 points
- **Risk Level**: HIGH

### Method 2: FlattenSinglePosition
- **Cyclomatic Complexity**: 16
- **Lines of Code**: 76
- **Target Complexity**: <=8
- **Reduction Required**: 8 points
- **Risk Level**: HIGH

### Method 3: HasActiveOrPendingOrderForEntry
- **Cyclomatic Complexity**: 12
- **Lines of Code**: 15
- **Target Complexity**: <=8
- **Reduction Required**: 4 points
- **Risk Level**: MEDIUM

### Method 4: CancelAllBracketOrdersForPosition
- **Cyclomatic Complexity**: 11
- **Lines of Code**: 9
- **Target Complexity**: <=8
- **Reduction Required**: 3 points
- **Risk Level**: MEDIUM

## Complexity Metrics Summary

Total Complexity Points to Reduce: 26 points

## Blast Radius Analysis

### ManageCIT Method
- Primary Function: Manages CIT order logic
- Dependencies: Order management, position tracking
- Impact Scope: Core order execution flow
- Refactoring Risk: MEDIUM-HIGH

## Risk Assessment

### Overall Risk: HIGH

Rationale:
1. High Complexity Concentration: 4 methods, total CYC=58
2. Critical Path Code: Order management is core trading logic
3. Large Reduction Required: 26 complexity points
4. Tight Coupling: Methods share state and dependencies

## Hotspot Analysis Conclusion

**Phase 0 Status**: COMPLETED

**Key Findings**:
- 4 methods require complexity reduction (total 26 points)
- ManageCIT is highest priority (CYC=19, LOC=77)
- File contains concentrated complexity hotspot
- Refactoring is feasible with proper extraction strategy

**Recommendation**: PROCEED to Phase 1 (Vision/Spec)
