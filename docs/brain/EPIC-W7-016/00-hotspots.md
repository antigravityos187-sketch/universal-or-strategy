# Phase 0: Hotspot Analysis - EPIC-W7-016

## Agent Tracking
- **Agent Name**: v12-phase0-hotspot
- **Bobcoins Used**: 0.77
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-23T02:37:31Z

## Target Method
- **Method**: TryHandleFleet_CancelAll
- **File**: src/V12_002.UI.IPC.Commands.Fleet.cs
- **Line**: 177
- **Cyclomatic Complexity**: 19
- **Lines of Code**: 56

## Complexity Metrics

### Symbol Complexity Analysis
Cyclomatic: 19, Max Nesting: 5, Param Count: 2, Lines: 56, Assessment: high

**Assessment**: HIGH complexity
- Cyclomatic complexity of 19 exceeds Jane Street threshold of 8 by 137%
- Maximum nesting depth of 5 indicates deeply nested control flow
- 56 lines of code in a single method suggests multiple responsibilities

### Hotspot Ranking Context
From top 50 hotspots analysis, TryHandleFleet_CancelAll ranks #38:
- **Hotspot Score**: 43.9445
- **Churn (90 days)**: 8 commits
- **Classification**: High-risk refactoring candidate

Related methods in same file with similar complexity:
- TryHandleFleet_LongShort (CYC 21, hotspot score 46.1417)
- TryHandleFleetCommand (CYC 20, hotspot score 43.9445)

## Blast Radius

### Direct Impact Analysis
Importer Count: 0, Direct Dependents: 0, Overall Risk Score: 0.0

**Assessment**: LOW blast radius
- Zero external importers detected
- Zero direct dependents
- Overall risk score: 0.0
- This method is internally scoped within the V12_002 class

**Interpretation**: The method has minimal external coupling, making it a safer refactoring target.

## Call Hierarchy

### Callers (Depth 1)
1. **TryHandleFleetCommand** (src/V12_002.UI.IPC.Commands.Fleet.cs:37)

### Callees (30 symbols across 3 depth levels)

**Depth 1 (Direct Calls)**:
1. MetadataGuardDuplicate
2. CancelAll_ProcessMasterAccount
3. CancelAll_ProcessFleetAccounts
4. CancelOrderOnAccount

**Depth 2 (Indirect Calls)**:
5. _processedCommandIds
6. LogBuffer.Format
7. CancelAll_ProcessFleetOrders
8. CancelAll_CleanupUnfilledPositions
9. IsOrderTerminal

**Depth 3 (Transitive Calls)**:
10. LogBuffer.ValidateThreadAffinity
11. LogBuffer.FormatInternal
12. IsFleetAccount
13. CancelAll_ProcessSingleFleetAccount
14. activePositions
15. CleanupPosition

## Risk Assessment

### Overall Risk: MEDIUM

**Risk Factors**:
1. LOW Blast Radius: Zero external dependents
2. HIGH Complexity: CYC 19 (137% above threshold)
3. MODERATE Churn: 8 commits in 90 days
4. GOOD Decomposition: Already delegates to helper methods
5. DEEP Nesting: Max nesting depth of 5

**Refactoring Safety**: Safe to refactor due to low blast radius

**Recommended Approach**:
1. Extract duplicate detection (CYC ~3)
2. Extract master account cancellation (CYC ~5)
3. Extract fleet account cancellation (CYC ~5)
4. Extract position cleanup (CYC ~3)
5. Reduce main method to orchestration (CYC ~3)

## Conclusion

TryHandleFleet_CancelAll is a **MEDIUM-risk, HIGH-priority** refactoring target.
