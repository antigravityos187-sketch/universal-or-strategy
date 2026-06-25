# Phase 0: Hotspot Analysis - EPIC-W7-032

## Agent Tracking
- **Agent Name**: v12-phase0-hotspot
- **Bobcoins Used**: 0.77
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-23T02:40:40Z

## Target Method
- **Method**: RestoreCascadedTargets
- **File**: src/V12_002.Orders.Management.StopSync.cs
- **Line**: 981
- **Cyclomatic Complexity**: 23
- **Max Nesting Depth**: 6
- **Parameter Count**: 2
- **Lines of Code**: 118

## Complexity Metrics

### Symbol Complexity Analysis
- **Cyclomatic Complexity**: 23 (HIGH - exceeds threshold of 8)
- **Max Nesting Depth**: 6 (HIGH - deep nesting indicates complex control flow)
- **Parameter Count**: 2 (ACCEPTABLE)
- **Lines of Code**: 118 (LARGE - indicates potential for extraction)
- **Assessment**: HIGH complexity

### Hotspot Score Analysis
From repository-wide hotspot analysis (top 50):
- **Hotspot Score**: 74.0341 (HIGH)
- **Rank**: 10th highest hotspot in codebase
- **Churn**: 24 commits in last 90 days (HIGH volatility)
- **Formula**: complexity x log(1 + churn) = 23 x log(1 + 24) = 74.0341

### Comparison to Repository Baseline
Top 10 hotspots in codebase:
1. HydrateFromOpenPositions (120.88) - complexity 34
2. IsCommandForThisInstrument (109.83) - complexity 38
3. HandleTerminated (102.04) - complexity 30
4. SweepBrokerOrders (99.55) - complexity 28
5. HydrateWorkingOrdersFromBroker (81.77) - complexity 23
6. AdoptMasterOrders (78.22) - complexity 22
7. ValidateStopOrderPreconditions (77.25) - complexity 24
8. FlattenSinglePosition (74.86) - complexity 27
9. UpdateStopQuantity (74.03) - complexity 23
10. RestoreCascadedTargets (74.03) - complexity 23 (TARGET)

## Blast Radius

### Direct Impact Analysis
- **Importer Count**: 0 files
- **Direct Dependents**: 0 symbols
- **Overall Risk Score**: 0.0 (LOW)
- **Confirmed Consumers**: 0
- **Potential Consumers**: 0

### Interpretation
- **ISOLATED METHOD**: No other files import or directly depend on this method
- **LOW BLAST RADIUS**: Changes will not propagate to other parts of the codebase
- **SAFE REFACTORING TARGET**: Minimal risk of breaking downstream consumers

## Call Hierarchy

### Callers (Incoming)
- **Caller Count**: 0
- **Interpretation**: This method is NOT called by any other indexed symbols
- **Risk**: LOW - no upstream dependencies to break

### Callees (Outgoing)
- **Callee Count**: 12
- **Dependencies**:
  1. activePositions (constant) - src/V12_002.cs:199
  2. SymmetryTrim (method) - src/V12_002.Symmetry.Replace.cs:343
  3. GetTargetOrdersDictionary (method) - src/V12_002.UI.Callbacks.cs:1039
  4. LogBuffer.Format (method) - src/V12_002.Perf.LogBuffer.cs:28
  5. LogBuffer.ValidateThreadAffinity (method) - depth 2
  6. LogBuffer.FormatInternal (method) - depth 2

### Call Graph Depth
- **Maximum Depth Reached**: 2 levels
- **Interpretation**: Shallow call graph indicates limited transitive dependencies

## Risk Assessment

### Overall Risk: MEDIUM-LOW

#### Risk Factors (Positive)
1. Isolated Method: Zero callers, zero blast radius
2. No External Dependencies: Changes will not break other code
3. Shallow Call Graph: Only 2 levels deep
4. Safe Refactoring Target: Low risk of regression

#### Risk Factors (Negative)
1. High Complexity: CYC 23 (2.9x over threshold of 8)
2. High Churn: 24 commits in 90 days (volatile code)
3. Deep Nesting: 6 levels (hard to reason about)
4. Large Method: 118 lines (potential for extraction)
5. Top 10 Hotspot: 10th highest risk in entire codebase

### Refactoring Recommendation
PROCEED WITH CAUTION - Complexity and churn indicate this is error-prone code. Isolation means refactoring is safe (will not break other code). High churn suggests active development area (coordinate with team). Deep nesting and large size make this a good extraction candidate.

### Jane Street Alignment
- **Target**: CYC <= 8 (Jane Street strict standard)
- **Current**: CYC 23
- **Gap**: 15 points over threshold
- **Extraction Strategy**: Split into 3-4 smaller methods to achieve CYC <= 8 per method

## Next Steps (Phase 1)
1. Define scope boundary (what stays, what gets extracted)
2. Identify extraction candidates within the 118-line method
3. Plan architecture for extracted helper methods
4. Ensure each extracted method achieves CYC <= 8
