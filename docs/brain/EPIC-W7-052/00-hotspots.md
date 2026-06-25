# Phase 0: Hotspot Analysis - EPIC-W7-052

## Agent Tracking
- **Agent Name**: v12-phase0-hotspot
- **Bobcoins Used**: 0.74
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-23T02:44:27Z

## Target Method
- **Method**: CleanupStalePendingReplacements
- **File**: src/V12_002.Trailing.StopUpdate.cs
- **Line**: 37
- **Cyclomatic Complexity**: 11 (Target: ≤8)
- **Max Nesting Depth**: 7
- **Parameter Count**: 0
- **Lines of Code**: 44

## Complexity Metrics

### Assessment: HIGH COMPLEXITY
- **Cyclomatic Complexity**: 11 (exceeds Jane Street threshold of 8)
- **Max Nesting Depth**: 7 (deeply nested control flow)
- **Lines of Code**: 44 (moderate size)
- **Parameter Count**: 0 (no parameters)

**Complexity Breakdown**:
- CYC 11 indicates 11 independent execution paths
- Nesting depth of 7 suggests complex conditional logic
- Method is self-contained (no parameters) but internally complex

## Blast Radius Analysis

### Impact Assessment: LOW RISK
- **Direct Dependents**: 0
- **Importer Count**: 0
- **Overall Risk Score**: 0.0
- **Confirmed Importers**: None
- **Potential Importers**: None

**Interpretation**:
- Method is NOT called by any other code in the codebase
- Zero external dependencies on this method
- Refactoring has minimal blast radius
- Safe to extract/refactor without breaking other code

## Call Hierarchy

### Callers (Upstream)
- **Count**: 0
- **Analysis**: No methods call CleanupStalePendingReplacements
- **Implication**: Method may be dead code OR called via reflection/dynamic dispatch

### Callees (Downstream)
- **Count**: 26 methods called
- **Depth**: 2 levels analyzed

**Key Dependencies**:
1. `pendingStopReplacements` (constant) - data structure being cleaned
2. `activePositions` (constant) - position tracking
3. `LogBuffer.Format` - logging
4. `CreateNewStopOrder` - order creation
5. `RestoreCascadedTargets` - target restoration
6. `ValidateStopOrderPreconditions` - validation
7. `SubmitStopOrderToBroker` - order submission
8. `FlattenPositionByName` - position flattening
9. `Enqueue` - FSM/Actor pattern
10. `SymmetryTrim` - symmetry operations
11. `GetTargetOrdersDictionary` - target order retrieval

**Call Pattern Analysis**:
- Method orchestrates multiple subsystems (logging, orders, positions, FSM)
- High fan-out (26 callees) suggests coordination/orchestration role
- Calls span multiple partial classes (Orders.Management, Symmetry, UI.Callbacks)

## Risk Assessment: MEDIUM-HIGH

### Complexity Risk: HIGH
- CYC 11 exceeds threshold by 37.5%
- Nesting depth of 7 indicates complex branching logic
- 44 lines with 11 paths = high cognitive load

### Blast Radius Risk: LOW
- Zero callers = isolated method
- No external dependencies on this method
- Refactoring won't break other code

### Maintenance Risk: MEDIUM
- High fan-out (26 callees) = many dependencies
- Orchestrates multiple subsystems
- Changes may require understanding of order lifecycle, FSM, and position management

### Refactoring Feasibility: HIGH
- Isolated method (no callers) = safe to refactor
- Clear orchestration pattern = can extract sub-workflows
- No external contracts to maintain

## Recommended Approach

### Strategy: Extract Sub-Workflows
1. **Identify logical phases** within the 11 execution paths
2. **Extract helper methods** for each phase (target CYC ≤8 each)
3. **Preserve orchestration** in main method (reduce to coordinator)
4. **Maintain call semantics** (no behavioral changes)

### Extraction Candidates
Based on 26 callees, likely phases:
- Validation phase (ValidateStopOrderPreconditions)
- Cleanup phase (pendingStopReplacements iteration)
- Order creation phase (CreateNewStopOrder)
- Target restoration phase (RestoreCascadedTargets)
- Submission phase (SubmitStopOrderToBroker)

### Success Criteria
- Main method CYC ≤8
- Each extracted method CYC ≤8
- All 26 callees preserved
- Zero behavioral changes
- Build passes + F5 in NinjaTrader

## Next Steps (Phase 1)
1. Read full method source via jCodemunch
2. Identify the 11 decision points (if/else/switch/loop)
3. Map decision points to logical phases
4. Define extraction boundaries
5. Generate scope boundary document
