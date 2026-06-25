# Phase 0: Hotspot Analysis - EPIC-W7-051

## Agent Tracking
- **Agent Name**: v12-phase0-hotspot
- **Bobcoins Used**: 0.74
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-23T02:44:13Z

## Target Method
- **Method**: UpdateStopOrder
- **File**: src/V12_002.Trailing.StopUpdate.cs
- **Line**: 84
- **Cyclomatic Complexity**: 13 (Target: ≤8)
- **Lines of Code**: 56

## Complexity Metrics

### Symbol Complexity Analysis
- **Cyclomatic Complexity**: 13
- **Max Nesting Depth**: 4
- **Parameter Count**: 4
- **Assessment**: HIGH (exceeds Jane Street threshold of 8)

**Method Signature**:
private void UpdateStopOrder(string entryName, PositionInfo pos, double newStopPrice, int newTrailLevel)

### Complexity Breakdown
The method has 13 decision points, indicating multiple conditional branches and control flow paths. With a max nesting depth of 4, there are deeply nested control structures that increase cognitive load.

**Jane Street Alignment**: This method violates the CYC ≤8 mandate. Functions with CYC >8 are harder to:
- Reason about under microsecond latency constraints
- Test exhaustively (exponential path growth)
- Audit for race conditions in lock-free code

## Blast Radius

### Import Analysis
- **Importer Count**: 0
- **Direct Dependents**: 0
- **Overall Risk Score**: 0.0
- **Confirmed Files**: 0
- **Potential Files**: 0

**Assessment**: LOW BLAST RADIUS
- This method is not directly imported by other files
- Changes are isolated to the V12_002.Trailing.StopUpdate.cs file
- No external dependencies detected

## Call Hierarchy

### Callers (Depth 2)
- **Caller Count**: 0
- **Assessment**: This method is not called by any other indexed symbols

### Callees (Depth 2)
- **Callee Count**: 40

**Direct Callees (Depth 1)**:
1. stopOrders (constant) - src/V12_002.cs:201
2. ValidateStopPrice (method) - src/V12_002.Orders.Management.StopSync.cs:1200
3. pendingStopReplacements (constant) - src/V12_002.cs:210
4. HandleStalePendingReplacement (method) - src/V12_002.Trailing.StopUpdate.cs:141
5. UpdateExistingPendingReplacement (method) - src/V12_002.Trailing.StopUpdate.cs:167
6. InitiateStopReplacement (method) - src/V12_002.Trailing.StopUpdate.cs:307
7. CreateDirectStopOrder (method) - src/V12_002.Trailing.StopUpdate.cs:371
8. HandleUpdateException (method) - src/V12_002.Trailing.StopUpdate.cs:496

**Indirect Callees (Depth 2)**:
- Validate_LongIsIllegalAdjust (method)
- Validate_ShortIsIllegalAdjust (method)
- LogBuffer.Format (method)
- MarkStickyDirty (method)
- CaptureTargetSnapshot (method)
- RefreshTargetSnapshot (method)
- GetTargetOrdersDictionary (method)
- CancelOrderForReplace (method)
- Enqueue (method)
- HandleStopSubmissionFailure (method)
- activePositions (constant)
- FlattenPositionByName (method)

### Call Graph Insights
The method orchestrates stop order updates through multiple helper methods:
- **Validation**: ValidateStopPrice, Validate_LongIsIllegalAdjust, Validate_ShortIsIllegalAdjust
- **State Management**: pendingStopReplacements, stopOrders, activePositions
- **Order Operations**: InitiateStopReplacement, CreateDirectStopOrder, CancelOrderForReplace
- **Error Handling**: HandleUpdateException, HandleStopSubmissionFailure
- **Snapshot Management**: CaptureTargetSnapshot, RefreshTargetSnapshot

## Risk Assessment

### Overall Risk: MEDIUM

**Risk Factors**:
1. ✅ **LOW Blast Radius**: No external importers, isolated changes
2. ❌ **HIGH Complexity**: CYC=13 exceeds threshold by 62.5%
3. ⚠️ **MEDIUM Call Depth**: 40 callees across 2 levels
4. ⚠️ **MEDIUM Nesting**: Max depth of 4 indicates nested conditionals

**Refactoring Safety**:
- **Isolation**: Changes will not ripple to other files
- **Testing**: 13 decision points require comprehensive test coverage
- **Extraction Candidates**: Multiple helper methods already exist, suggesting prior refactoring attempts

### Recommended Approach
1. **Extract Decision Logic**: Break down the 13 decision points into smaller, single-purpose methods
2. **Reduce Nesting**: Flatten nested conditionals using early returns
3. **Target CYC ≤8**: Aim for 2-3 extracted methods to achieve Jane Street compliance
4. **Preserve Behavior**: Existing helper methods suggest well-defined boundaries

## Hotspot Context

### File: src/V12_002.Trailing.StopUpdate.cs
This file contains trailing stop order update logic. The UpdateStopOrder method is the primary entry point for stop order modifications.

**Related Methods in File**:
- HandleStalePendingReplacement (line 141)
- UpdateExistingPendingReplacement (line 167)
- InitiateStopReplacement (line 307)
- CreateDirectStopOrder (line 371)
- HandleStopSubmissionFailure (line 458)
- HandleUpdateException (line 496)
- CalculateStopForLevel (line 533)

### Architectural Pattern
The method follows a **state machine pattern** for stop order lifecycle management:
1. Validate stop price
2. Check for pending replacements
3. Route to appropriate handler (stale/existing/new)
4. Handle exceptions

## Next Steps (Phase 1)
1. Define extraction boundaries for CYC reduction
2. Identify which decision points can be extracted
3. Plan test coverage for extracted methods
4. Validate against Jane Street patterns in knowledge base
