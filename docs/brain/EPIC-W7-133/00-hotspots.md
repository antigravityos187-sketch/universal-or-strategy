# Phase 0: Hotspot Analysis - EPIC-W7-133

## Agent Tracking
- **Agent Name**: v12-phase0-hotspot
- **Bobcoins Used**: 0.74
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-23T02:59:40Z

## Target Method
- **Method**: MoveStop_SinglePosition
- **File**: src/V12_002.Trailing.Breakeven.cs
- **Line**: 73
- **Cyclomatic Complexity**: 21 (HIGH - exceeds threshold of 8)
- **Lines of Code**: 91
- **Max Nesting Depth**: 5
- **Parameter Count**: 4

## Complexity Metrics

### Assessment: HIGH COMPLEXITY
The method has a cyclomatic complexity of 21, which significantly exceeds the Jane Street strict standard of <=8. This indicates:
- Multiple decision paths (21 distinct execution paths)
- Deep nesting (5 levels)
- Difficult to reason about under microsecond latency constraints
- Exponential test path growth
- Higher risk for race conditions in lock-free code

### Method Signature
private void MoveStop_SinglePosition(string entryName, PositionInfo pos, double offsetPoints, double lastKnownPrice)

## Blast Radius Analysis

### Direct Impact: LOW
- **Importer Count**: 0
- **Direct Dependents**: 0
- **Overall Risk Score**: 0.0
- **Confirmed Files**: 0
- **Potential Files**: 0

### Interpretation
The method has zero external dependencies - no other files import or directly depend on this method. This is IDEAL for refactoring because:
- Changes are isolated to the containing file
- No risk of breaking external consumers
- Safe to extract without coordination
- Low blast radius = low regression risk

## Call Hierarchy

### Callers (Who calls this method)
**1 Direct Caller**:
- MoveStopsToBreakevenWithOffset (src/V12_002.Trailing.Breakeven.cs:41)

### Callees (What this method calls)
**46 Total Callees** across 3 depth levels

#### Depth 1 (Direct calls):
1. UpdateStopOrder - Stop order update logic
2. MarkStickyDirty - State persistence
3. LogBuffer.Format - Performance logging

#### Depth 2 (Indirect calls):
- stopOrders constant access
- pendingStopReplacements constant access
- ValidateStopPrice
- HandleStalePendingReplacement
- UpdateExistingPendingReplacement
- InitiateStopReplacement
- CreateDirectStopOrder
- HandleUpdateException

#### Depth 3 (Transitive calls):
- Validate_LongIsIllegalAdjust
- Validate_ShortIsIllegalAdjust
- CaptureTargetSnapshot
- RefreshTargetSnapshot
- GetTargetOrdersDictionary
- CancelOrderForReplace
- Enqueue (FSM/Actor pattern)
- HandleStopSubmissionFailure
- FlattenPositionByName

## Risk Assessment: MEDIUM-HIGH

### Risk Factors
1. LOW Blast Radius: Zero external dependencies (SAFE)
2. HIGH Complexity: CYC 21 (2.6x over threshold)
3. Deep Nesting: 5 levels (cognitive load)
4. Wide Coupling: 46 callees (internal complexity)
5. Single Caller: Isolated entry point (SAFE)

### Overall Risk: MEDIUM-HIGH
- **Refactoring Safety**: HIGH (zero blast radius)
- **Cognitive Complexity**: HIGH (CYC 21, nesting 5)
- **Testing Burden**: HIGH (21 execution paths)
- **Regression Risk**: LOW (isolated, single caller)

### Recommended Approach
1. Extract validation logic (reduce CYC by ~5-7)
2. Extract stop order update logic (reduce CYC by ~4-6)
3. Extract error handling (reduce CYC by ~3-4)
4. Target: Reduce to CYC <=8 per extracted method
5. Strategy: Vertical slice extraction (preserve call semantics)

## Jane Street Alignment
- **Current**: CYC 21 (FAILS strict standard)
- **Target**: CYC <=8 per method
- **Rationale**: Microsecond-latency reasoning, exhaustive testing, race condition auditing
- **V12 DNA**: Make illegal states unrepresentable - requires simple, verifiable logic

## Next Phase
Proceed to Phase 1: Scope Definition
