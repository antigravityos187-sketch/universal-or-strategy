# Phase 0: Hotspot Analysis - EPIC-W7-019

## Agent Tracking
- **Agent Name**: v12-phase0-hotspot
- **Bobcoins Used**: 0.77
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-23T02:37:47Z to 2026-06-23T02:38:06Z

## Target Method
- **Method**: TryHandleFleet_MoveTarget
- **File**: src/V12_002.UI.IPC.Commands.Fleet.cs
- **Line**: 645
- **Cyclomatic Complexity**: 17 (actual measurement)
- **Max Nesting Depth**: 5
- **Parameter Count**: 2
- **Lines of Code**: 49

## Complexity Metrics

### Assessment: HIGH COMPLEXITY
- **Cyclomatic Complexity**: 17 (exceeds Jane Street threshold of 8)
- **Max Nesting Depth**: 5 (deep nesting indicates complex control flow)
- **Parameter Count**: 2 (reasonable)
- **Lines of Code**: 49 (moderate size)

### Complexity Analysis
The method has a cyclomatic complexity of 17, which is more than double the Jane Street strict standard of 8. This indicates:
- Multiple decision points (if/else, switch, loops)
- Complex branching logic
- Difficult to test exhaustively (2^17 = 131,072 potential paths)
- Higher risk of race conditions in lock-free code
- Cognitive load exceeds microsecond-latency reasoning threshold

## Blast Radius

### Direct Impact: MINIMAL
- **Importer Count**: 0
- **Direct Dependents**: 0
- **Overall Risk Score**: 0.0
- **Confirmed Files**: 0
- **Potential Files**: 0

### Analysis
The blast radius analysis shows this method has **zero external dependencies**. This is excellent for refactoring:
- No files import this method directly
- No downstream consumers to break
- Changes are isolated to the method itself
- Low risk of cascading failures

## Call Hierarchy

### Callers (1)
1. **TryHandleFleetCommand** (src/V12_002.UI.IPC.Commands.Fleet.cs:37)
   - Resolution: ast_resolved
   - Depth: 1
   - This is the only entry point to TryHandleFleet_MoveTarget

### Callees (30 methods called)
The method calls 30 downstream methods, indicating high coupling:

**Primary Callees (Depth 1)**:
1. MoveSpecificTargetAbsolute (src/V12_002.Trailing.Breakeven.cs:559)
2. MoveSpecificTarget (src/V12_002.Trailing.Breakeven.cs:335)
3. ValidateTargetMoveAbsoluteRequest (src/V12_002.Trailing.Breakeven.cs:415)
4. FindTargetOrderForAbsoluteMove (src/V12_002.Trailing.Breakeven.cs:438)
5. ExecuteTargetAbsoluteMove (src/V12_002.Trailing.Breakeven.cs:467)
6. ValidateMoveTargetRequest (src/V12_002.Trailing.Breakeven.cs:166)
7. FindTargetOrderForPosition (src/V12_002.Trailing.Breakeven.cs:186)
8. CalculateAndValidateNewTargetPrice (src/V12_002.Trailing.Breakeven.cs:225)
9. ExecuteFollowerTargetMove (src/V12_002.Trailing.Breakeven.cs:275)
10. ExecuteMasterTargetMove (src/V12_002.Trailing.Breakeven.cs:312)
11. LogBuffer.Format (src/V12_002.Perf.LogBuffer.cs:28)
12. activePositions constant (src/V12_002.cs:199)

**Secondary Callees (Depth 2-3)**:
- LogBuffer.ValidateThreadAffinity
- LogBuffer.FormatInternal
- StampReaperMoveGrace

### Call Hierarchy Analysis
- **Single Entry Point**: Only called by TryHandleFleetCommand (good for isolation)
- **High Fan-Out**: Calls 30 methods (indicates orchestration role)
- **Deep Call Chains**: Reaches depth 3 (moderate complexity)
- **Validation Pattern**: Multiple validation methods called
- **Execution Pattern**: Multiple execution methods called

## Risk Assessment: MEDIUM

### Risk Factors
- LOW BLAST RADIUS: Zero external dependencies (excellent isolation)
- SINGLE CALLER: Only one entry point (easy to test)
- HIGH COMPLEXITY: CYC 17 exceeds threshold by 2.1x
- HIGH FAN-OUT: Calls 30 methods (orchestration complexity)
- DEEP NESTING: Max depth 5 (cognitive load)
- LOW CHURN: Not in top 50 hotspots (stable)

### Overall Risk: MEDIUM
- **Refactoring Risk**: LOW (isolated, single caller)
- **Maintenance Risk**: MEDIUM (high complexity, deep nesting)
- **Testing Risk**: MEDIUM (17 decision points = 131k paths)
- **Production Risk**: LOW (stable, low churn)

## Refactoring Recommendations

### Strategy: EXTRACT METHOD PATTERN
Given the high fan-out (30 callees) and orchestration role, the method likely contains:
1. **Validation logic**
2. **Lookup logic**
3. **Calculation logic**
4. **Execution logic**

### Target Complexity
- **Current**: CYC 17
- **Target**: CYC ≤ 8 per method (Jane Street standard)
- **Approach**: Extract 3-4 helper methods to reduce main method to orchestration only
