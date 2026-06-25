# Phase 0: Hotspot Analysis - EPIC-W7-056

## Agent Tracking
- **Agent Name**: v12-phase0-hotspot
- **Bobcoins Used**: 0.77
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-23T02:45:17Z

## Target Method
- **Method**: SweepBrokerOrders
- **File**: src/V12_002.SIMA.Lifecycle.cs
- **Line**: 1360
- **Cyclomatic Complexity**: 28
- **Lines of Code**: 95

## Complexity Metrics

### Symbol Complexity Analysis
- **Cyclomatic Complexity**: 28 (HIGH - exceeds threshold of 8)
- **Max Nesting Depth**: 8 (HIGH - deeply nested logic)
- **Parameter Count**: 1 (LOW)
- **Lines of Code**: 95 (MEDIUM)
- **Assessment**: HIGH complexity

### Hotspot Score Analysis
From top 50 hotspots analysis:
- **Hotspot Score**: 99.5497 (4th highest in codebase)
- **Ranking**: #4 out of 50 analyzed methods
- **Churn**: 34 commits in last 90 days (HIGH volatility)
- **Assessment**: HIGH risk (complex + high churn)

### Jane Street Threshold Violation
- **Target**: CYC ≤ 8 (Jane Street strict standard)
- **Current**: CYC = 28
- **Violation**: 3.5x over threshold
- **Cognitive Load**: Exponential path growth makes exhaustive testing impractical

## Blast Radius

### Import Analysis
- **Direct Importers**: 0
- **Direct Dependents**: 0
- **Overall Risk Score**: 0.0 (LOW)
- **Confirmed Files**: 0
- **Potential Files**: 0

### Risk Assessment
- **Import Risk**: LOW (no external dependencies detected)
- **Refactoring Safety**: HIGH (isolated method, no blast radius)
- **Breaking Change Risk**: MINIMAL

## Call Hierarchy

### Callers (Who calls this method)
1. **CancelAllV12GtcOrders** (depth 1)
   - File: src/V12_002.SIMA.Lifecycle.cs
   - Line: 1294
   - Resolution: ast_resolved

2. **ProcessShutdownSIMA** (depth 2)
   - File: src/V12_002.SIMA.Lifecycle.cs
   - Line: 98
   - Resolution: ast_resolved

3. **ProcessApplySimaState** (depth 3)
   - File: src/V12_002.SIMA.Lifecycle.cs
   - Line: 38
   - Resolution: ast_resolved

### Callees (What this method calls)
1. **IsFleetAccount** (depth 1)
   - Files: src/V12_002.cs (line 864), src-vm-backup/V12_002.cs (line 830)
   - Resolution: ast_inferred

2. **LogBuffer.Format** (depth 1)
   - Files: src/V12_002.Perf.LogBuffer.cs (line 28), src-vm-backup/V12_002.Perf.LogBuffer.cs (line 28)
   - Resolution: ast_inferred

3. **LogBuffer.ValidateThreadAffinity** (depth 2)
   - Files: src/V12_002.Perf.LogBuffer.cs (line 119), src-vm-backup/V12_002.Perf.LogBuffer.cs (line 119)
   - Resolution: ast_resolved/ast_inferred

4. **LogBuffer.FormatInternal** (depth 2)
   - Files: src/V12_002.Perf.LogBuffer.cs (line 56), src-vm-backup/V12_002.Perf.LogBuffer.cs (line 56)
   - Resolution: ast_resolved/ast_inferred

### Call Graph Summary
- **Total Callers**: 3 (all within same file)
- **Total Callees**: 8 (4 unique methods, duplicated in src/ and src-vm-backup/)
- **Max Depth Reached**: 3
- **Internal Cohesion**: HIGH (all callers in same lifecycle file)

## Risk Assessment

### Overall Risk Profile
- **Complexity Risk**: HIGH (CYC 28, nesting 8)
- **Churn Risk**: HIGH (34 commits in 90 days)
- **Blast Radius Risk**: LOW (0 external dependencies)
- **Refactoring Safety**: HIGH (isolated, no importers)
- **Composite Risk**: MEDIUM-HIGH

### Refactoring Feasibility
- **Extraction Difficulty**: MEDIUM
  - High complexity suggests multiple responsibilities
  - Deep nesting (8 levels) indicates nested conditionals
  - 95 lines suggests substantial logic to decompose
  
- **Testing Requirements**: HIGH
  - 28 cyclomatic paths require extensive test coverage
  - Nested logic increases edge case complexity
  - Current test coverage unknown

- **Breaking Change Risk**: LOW
  - No external callers detected
  - All usage within same file (SIMA.Lifecycle.cs)
  - Refactoring can be done surgically

### Recommended Approach
1. **Extract nested conditionals** to helper methods (target CYC ≤ 8 per method)
2. **Decompose by responsibility** (likely order sweeping, validation, logging)
3. **Preserve call signatures** for internal callers
4. **Add unit tests** before extraction (TDD approach)
5. **Verify with F5 in NinjaTrader** after each extraction

## Hotspot Context (Top 10 for Reference)

1. HydrateFromOpenPositions (CYC 34, score 120.88) - HIGHER priority
2. IsCommandForThisInstrument (CYC 38, score 109.83) - HIGHER priority
3. HandleTerminated (CYC 30, score 102.04) - HIGHER priority
4. **SweepBrokerOrders (CYC 28, score 99.55) - THIS EPIC**
5. HydrateWorkingOrdersFromBroker (CYC 23, score 81.77)
6. AdoptMasterOrders (CYC 22, score 78.22)
7. ValidateStopOrderPreconditions (CYC 24, score 77.25)
8. FlattenSinglePosition (CYC 27, score 74.86)
9. UpdateStopQuantity (CYC 23, score 74.03)
10. RestoreCascadedTargets (CYC 23, score 74.03)

## Conclusion

**SweepBrokerOrders is a HIGH-priority refactoring target**:
- 4th highest hotspot score in codebase
- 3.5x over Jane Street complexity threshold
- High churn (34 commits) indicates active development area
- LOW blast radius makes refactoring safe
- Isolated within SIMA.Lifecycle.cs (no external dependencies)

**Recommended for Phase 1 scope definition and Phase 2 architecture planning.**
