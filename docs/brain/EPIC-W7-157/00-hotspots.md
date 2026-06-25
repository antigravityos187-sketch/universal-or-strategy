# Phase 0: Hotspot Analysis - EPIC-W7-157

## Agent Tracking
- **Agent Name**: v12-phase0-hotspot
- **Bobcoins Used**: 0.77
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-23T03:33:57Z

## Target Method
- **Method**: TryHandleFleet_MoveTarget
- **File**: src/V12_002.UI.IPC.Commands.Fleet.cs
- **Line**: 645
- **Cyclomatic Complexity**: 17
- **Assessment**: HIGH

## Complexity Metrics

### Symbol Complexity Analysis
- **Cyclomatic Complexity**: 17 (HIGH - exceeds Jane Street threshold of 8)
- **Max Nesting Depth**: 5
- **Parameter Count**: 2
- **Lines of Code**: 49
- **Assessment**: HIGH complexity

### Complexity Context
The method has a cyclomatic complexity of 17, which is:
- **2.1x over Jane Street strict standard** (CYC ≤ 8)
- **1.1x over Codacy threshold** (CYC ≤ 15)
- Classified as HIGH complexity by jCodemunch

This level of complexity indicates:
- Multiple decision paths (17 distinct execution paths)
- Moderate nesting (5 levels deep)
- Potential for cognitive overload during maintenance
- Higher risk of bugs in edge cases

## Blast Radius Analysis

### Direct Impact
- **Importer Count**: 0
- **Direct Dependents**: 0
- **Overall Risk Score**: 0.0 (LOW)
- **Confirmed Files**: 0
- **Potential Files**: 0

### Risk Assessment
**LOW BLAST RADIUS**: This method has minimal external dependencies:
- No files directly import this method
- No confirmed downstream consumers
- Changes are isolated to the Fleet command processing subsystem
- Risk of breaking external code is minimal

## Call Hierarchy

### Callers (Who calls this method)
**1 Direct Caller**:
1. `TryHandleFleetCommand` (src/V12_002.UI.IPC.Commands.Fleet.cs:37)
   - Resolution: AST resolved
   - This is the main Fleet command dispatcher

### Callees (What this method calls)
**30 Methods Called** (depth=3):

#### Depth 1 - Direct Calls (8 methods):
1. `MoveSpecificTargetAbsolute` (src/V12_002.Trailing.Breakeven.cs:559)
2. `MoveSpecificTarget` (src/V12_002.Trailing.Breakeven.cs:335)
3. `ValidateTargetMoveAbsoluteRequest` (src/V12_002.Trailing.Breakeven.cs:415)
4. `activePositions` (src/V12_002.cs:199) - constant
5. `FindTargetOrderForAbsoluteMove` (src/V12_002.Trailing.Breakeven.cs:438)
6. `LogBuffer.Format` (src/V12_002.Perf.LogBuffer.cs:28)
7. `ExecuteTargetAbsoluteMove` (src/V12_002.Trailing.Breakeven.cs:467)
8. `ValidateMoveTargetRequest` (src/V12_002.Trailing.Breakeven.cs:166)

#### Depth 2 - Indirect Calls (6 methods):
9. `FindTargetOrderForPosition` (src/V12_002.Trailing.Breakeven.cs:186)
10. `CalculateAndValidateNewTargetPrice` (src/V12_002.Trailing.Breakeven.cs:225)
11. `ExecuteFollowerTargetMove` (src/V12_002.Trailing.Breakeven.cs:275)
12. `ExecuteMasterTargetMove` (src/V12_002.Trailing.Breakeven.cs:312)
13. `LogBuffer.ValidateThreadAffinity` (src/V12_002.Perf.LogBuffer.cs:119)
14. `LogBuffer.FormatInternal` (src/V12_002.Perf.LogBuffer.cs:56)

#### Depth 3 - Transitive Calls (2 methods):
15. `StampReaperMoveGrace` (src/V12_002.SIMA.cs:199)

### Call Pattern Analysis
The method orchestrates target order movement through:
1. **Validation layer**: ValidateMoveTargetRequest, ValidateTargetMoveAbsoluteRequest
2. **Lookup layer**: FindTargetOrderForPosition, FindTargetOrderForAbsoluteMove
3. **Calculation layer**: CalculateAndValidateNewTargetPrice
4. **Execution layer**: ExecuteMasterTargetMove, ExecuteFollowerTargetMove
5. **Logging layer**: LogBuffer.Format with thread affinity checks

This is a **coordinator pattern** with high fan-out (30 callees).

## Hotspot Context (Top 50 Repository Hotspots)

### Method Ranking
**TryHandleFleet_MoveTarget** does NOT appear in the top 50 hotspots by hotspot score.

### Related Fleet Methods in Top 50
- **TryHandleFleet_LongShort** (rank 38): CYC=21, hotspot_score=46.14, HIGH
- **TryHandleFleetCommand** (rank 45): CYC=20, hotspot_score=43.94, HIGH

### Top 5 Hotspots for Reference
1. **HydrateFromOpenPositions** (CYC=34, score=120.88) - SIMA Lifecycle
2. **IsCommandForThisInstrument** (CYC=38, score=109.83) - IPC
3. **HandleTerminated** (CYC=30, score=102.04) - Lifecycle
4. **SweepBrokerOrders** (CYC=28, score=99.55) - SIMA Lifecycle
5. **HydrateWorkingOrdersFromBroker** (CYC=23, score=81.77) - SIMA Lifecycle

## Risk Assessment

### Overall Risk: MEDIUM

**Complexity Risk**: HIGH
- CYC=17 exceeds Jane Street threshold (8) by 2.1x
- 5 levels of nesting indicate complex control flow
- 49 lines with 17 decision paths = high cognitive load

**Blast Radius Risk**: LOW
- Zero external dependents
- Isolated to Fleet command subsystem
- Only called by TryHandleFleetCommand dispatcher

**Churn Risk**: UNKNOWN
- Not in top 50 hotspots (complexity × churn)
- Suggests either low churn or recent extraction
- Requires git history analysis for confirmation

**Call Hierarchy Risk**: MEDIUM
- High fan-out (30 callees) creates coordination complexity
- Deep call chains (depth=3) increase debugging difficulty
- Coordinator pattern is appropriate but adds coupling

### Refactoring Priority
**MEDIUM-HIGH**: While blast radius is low (safe to refactor), the high complexity (CYC=17) and coordinator pattern with 30 callees suggest this method would benefit from extraction to improve:
1. Testability (isolate validation, lookup, execution concerns)
2. Readability (reduce cognitive load from 17 to ≤8 per extracted method)
3. Maintainability (single-responsibility principle)

## Recommended Extraction Strategy

### Candidate Extractions (to achieve CYC ≤ 8 per method)
1. **Extract validation logic** → ValidateFleetMoveTargetRequest()
2. **Extract absolute move path** → HandleAbsoluteTargetMove()
3. **Extract relative move path** → HandleRelativeTargetMove()
4. **Extract error handling** → LogFleetMoveError()

### Expected Outcome
- Original method: CYC 17 → CYC 4-6 (orchestration only)
- Extracted methods: CYC ≤ 8 each
- Improved testability: Each concern tested independently
- Maintained blast radius: LOW (no external callers affected)

## Conclusion

**TryHandleFleet_MoveTarget** is a **MEDIUM-HIGH priority refactoring target**:
- ✅ Safe to refactor (low blast radius)
- ⚠️ High complexity (CYC=17, exceeds threshold by 2.1x)
- ⚠️ Coordinator pattern with 30 callees
- ✅ Clear extraction opportunities (validation, absolute, relative paths)
- ✅ No external dependencies to coordinate

**Recommendation**: Proceed with extraction to achieve Jane Street strict standard (CYC ≤ 8).
