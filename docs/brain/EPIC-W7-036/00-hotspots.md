# Phase 0: Hotspot Analysis - EPIC-W7-036

## Agent Tracking
- **Agent Name**: v12-phase0-hotspot
- **Bobcoins Used**: 0.77
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-23T02:41:28Z

## Target Method
- **Method**: MoveStop_SinglePosition
- **File**: src/V12_002.Trailing.Breakeven.cs
- **Line**: 73
- **Cyclomatic Complexity**: 21 (ACTUAL - corrected from initial 13)
- **Max Nesting Depth**: 5
- **Parameter Count**: 4
- **Lines of Code**: 91
- **Assessment**: HIGH COMPLEXITY

## Complexity Metrics

### Current State
- **Cyclomatic Complexity**: 21 (exceeds Jane Street threshold of 8 by 13 points)
- **Max Nesting Depth**: 5 (deep nesting indicates complex control flow)
- **Parameter Count**: 4 (reasonable)
- **Lines of Code**: 91 (substantial method size)
- **Assessment**: HIGH - Requires refactoring to meet V12 DNA standards

### Jane Street Alignment
- **Target Threshold**: CYC ≤ 8
- **Current Gap**: +13 points over threshold
- **Cognitive Load**: HIGH - difficult to reason about under microsecond latency constraints
- **Test Coverage Risk**: Exponential path growth (2^21 = 2M+ theoretical paths)
- **Race Condition Audit**: Complex due to deep nesting and branching

## Blast Radius

### Import Analysis
- **Direct Importers**: 0 files
- **Importer Count**: 0
- **Direct Dependents**: 0
- **Overall Risk Score**: 0.0 (LOW)

### Impact Assessment
- **Confirmed Files**: 0 (no files directly import this method)
- **Potential Files**: 0 (no wildcard imports detected)
- **Blast Radius**: MINIMAL - method is internally called, not externally imported

### Risk Interpretation
The zero blast radius indicates this method is:
1. Called only within the same file or class hierarchy
2. Not exposed as a public API
3. Safe to refactor without external coordination
4. Low risk for breaking changes outside the immediate file

## Call Hierarchy

### Callers (Who Calls This Method)
**Total Callers**: 1

1. **MoveStopsToBreakevenWithOffset** (depth 1)
   - File: src/V12_002.Trailing.Breakeven.cs
   - Line: 41
   - Resolution: ast_resolved (high confidence)
   - Context: Parent orchestrator method that calls MoveStop_SinglePosition

### Callees (What This Method Calls)
**Total Callees**: 26 (HIGH - indicates complex internal logic)

#### Depth 1 Callees (Direct Calls)
1. **UpdateStopOrder** - src/V12_002.Trailing.StopUpdate.cs:84
2. **MarkStickyDirty** - src/V12_002.StickyState.cs:619
3. **LogBuffer.Format** - src/V12_002.Perf.LogBuffer.cs:28

#### Depth 2 Callees (Transitive Calls)
4. **stopOrders** (constant) - src/V12_002.cs:201
5. **ValidateStopPrice** - src/V12_002.Orders.Management.StopSync.cs:1200
6. **pendingStopReplacements** (constant) - src/V12_002.cs:210
7. **HandleStalePendingReplacement** - src/V12_002.Trailing.StopUpdate.cs:141
8. **UpdateExistingPendingReplacement** - src/V12_002.Trailing.StopUpdate.cs:167
9. **InitiateStopReplacement** - src/V12_002.Trailing.StopUpdate.cs:307
10. **CreateDirectStopOrder** - src/V12_002.Trailing.StopUpdate.cs:371
11. **HandleUpdateException** - src/V12_002.Trailing.StopUpdate.cs:496
12. **LogBuffer.ValidateThreadAffinity** - src/V12_002.Perf.LogBuffer.cs:119
13. **LogBuffer.FormatInternal** - src/V12_002.Perf.LogBuffer.cs:56

### Call Hierarchy Insights
- **Fan-out**: 26 callees indicates high coupling and complex internal logic
- **Depth**: 2-level call depth shows moderate transitive complexity
- **Resolution Confidence**: Mix of ast_resolved (high) and ast_inferred (medium)
- **Refactoring Strategy**: Extract sub-methods to reduce fan-out and improve testability

## Repository Hotspot Context

### Top 10 Hotspots (Complexity × Churn)
1. **HydrateFromOpenPositions** - CYC 34, Score 120.88 (HIGHEST)
2. **IsCommandForThisInstrument** - CYC 38, Score 109.83
3. **HandleTerminated** - CYC 30, Score 102.04
4. **SweepBrokerOrders** - CYC 28, Score 99.55
5. **HydrateWorkingOrdersFromBroker** - CYC 23, Score 81.77
6. **AdoptMasterOrders** - CYC 22, Score 78.22
7. **ValidateStopOrderPreconditions** - CYC 24, Score 77.25
8. **FlattenSinglePosition** - CYC 27, Score 74.86
9. **UpdateStopQuantity** - CYC 23, Score 74.03
10. **RestoreCascadedTargets** - CYC 23, Score 74.03

### EPIC-W7-036 Position
- **MoveStop_SinglePosition** is NOT in the top 50 hotspots by churn
- **Reason**: Low git churn (method is stable, not frequently modified)
- **Implication**: Complexity is the primary concern, not volatility
- **Priority**: MEDIUM - high complexity but low churn reduces urgency

## Risk Assessment

### Overall Risk: MEDIUM-HIGH

#### Risk Factors
1. **Complexity Risk**: HIGH
   - CYC 21 exceeds Jane Street threshold by 13 points
   - Deep nesting (5 levels) increases cognitive load
   - 26 callees indicate high coupling

2. **Blast Radius Risk**: LOW
   - Zero external importers
   - Single internal caller (MoveStopsToBreakevenWithOffset)
   - Refactoring impact is contained

3. **Churn Risk**: LOW
   - Not in top 50 hotspots (low git activity)
   - Stable method with infrequent changes

4. **Testing Risk**: HIGH
   - Exponential path growth (2^21 theoretical paths)
   - Complex control flow difficult to test exhaustively
   - No existing unit tests detected

#### Risk Mitigation Strategy
1. **Extract sub-methods** to reduce CYC from 21 to ≤8 per method
2. **Add unit tests** for each extracted method (TDD approach)
3. **Preserve behavior** - refactor is pure extraction, no logic changes
4. **Verify with F5** in NinjaTrader after each extraction

### Refactoring Recommendation
**PROCEED WITH CAUTION**
- Complexity justifies refactoring (CYC 21 >> 8)
- Low blast radius reduces coordination overhead
- Stable method (low churn) reduces regression risk
- High test coverage gap requires TDD discipline

## Next Steps (Phase 1: Scope Definition)
1. Read full source of MoveStop_SinglePosition (91 lines)
2. Identify logical boundaries for extraction
3. Map decision points to CYC contributors
4. Propose 3-5 sub-methods with clear responsibilities
5. Estimate CYC reduction per extraction
6. Define test cases for each sub-method
