# Phase 0: Hotspot Analysis - EPIC-W7-112

## Agent Tracking
- **Agent Name**: v12-phase0-hotspot
- **Bobcoins Used**: 0.58
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-23T02:55:35Z

## Target Method
- **Method**: ClassifyOrderByPrefix
- **File**: src/V12_002.SIMA.Lifecycle.cs
- **Line**: 1262
- **Cyclomatic Complexity**: 20
- **Lines of Code**: 25

## Complexity Metrics
- **Cyclomatic Complexity**: 20 (HIGH - exceeds threshold of 8)
- **Max Nesting Depth**: 2 (LOW)
- **Parameter Count**: 1 (LOW)
- **Assessment**: HIGH complexity

**Analysis**: The method has high cyclomatic complexity (20) which significantly exceeds the Jane Street strict standard of ≤8. This indicates multiple decision paths that make the code harder to reason about, test exhaustively, and audit for race conditions. The low nesting depth (2) and single parameter are positive factors, but the high CYC is the primary concern.

## Hotspot Score
- **Hotspot Score**: 71.107 (HIGH)
- **Ranking**: #13 out of top 50 hotspots
- **Churn (90 days)**: 34 commits
- **Formula**: complexity × log(1 + churn) = 20 × log(1 + 34) = 71.107

**Analysis**: This method ranks in the top 13 hotspots due to the combination of high complexity (20) and very high churn (34 commits in 90 days). This indicates the method is both complex AND frequently modified, which is the highest-risk combination for bug introduction.

## Blast Radius
- **Direct Dependents**: 0
- **Importer Count**: 0
- **Overall Risk Score**: 0.0 (LOW)
- **Confirmed Impact Files**: 0
- **Potential Impact Files**: 0

**Analysis**: The blast radius analysis shows ZERO external dependencies. This method is not imported or called from outside its defining file, making it a LOW-RISK refactoring target from a dependency perspective. Changes to this method will not break external consumers.

## Call Hierarchy

### Callers (4 methods call ClassifyOrderByPrefix)
1. **AdoptOrdersFromAccount** (depth 1, ast_resolved)
   - File: src/V12_002.SIMA.Lifecycle.cs
   - Line: 930
   
2. **AdoptMasterOrders** (depth 1, ast_resolved)
   - File: src/V12_002.SIMA.Lifecycle.cs
   - Line: 1195
   
3. **AdoptFleetOrders** (depth 2, ast_resolved)
   - File: src/V12_002.SIMA.Lifecycle.cs
   - Line: 903
   
4. **HydrateWorkingOrdersFromBroker** (depth 2, ast_resolved)
   - File: src/V12_002.SIMA.Lifecycle.cs
   - Line: 309

### Callees (0 methods called by ClassifyOrderByPrefix)
- **No downstream calls detected**

**Analysis**: ClassifyOrderByPrefix is called by 4 methods within the same file (SIMA.Lifecycle.cs), all related to order adoption and hydration workflows. The method makes no downstream calls, suggesting it is a pure classification/decision function. All callers are AST-resolved (high confidence).

## Risk Assessment

### Overall Risk: MEDIUM-HIGH

**Risk Factors**:
- ✅ **LOW Blast Radius**: Zero external dependencies
- ✅ **LOW Nesting**: Max depth of 2
- ✅ **LOW Parameters**: Single parameter
- ⚠️ **HIGH Complexity**: CYC 20 (2.5x over threshold)
- ⚠️ **HIGH Churn**: 34 commits in 90 days
- ⚠️ **HIGH Hotspot Score**: 71.107 (top 13)

**Refactoring Recommendation**: PROCEED with caution
- The zero blast radius makes this a safe refactoring target
- The high complexity and churn indicate this is a valuable target
- All callers are in the same file, simplifying testing
- Focus on extracting decision logic into smaller, testable methods

## Jane Street Alignment
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **Current Complexity**: 20
- **Reduction Required**: 12 points (60% reduction)
- **Cognitive Load**: HIGH - multiple decision paths make reasoning difficult under microsecond latency constraints

## Next Steps (Phase 1)
1. Review method source code to understand classification logic
2. Identify decision branches contributing to CYC=20
3. Plan extraction of sub-classification methods (target CYC ≤8 each)
4. Validate that extracted methods maintain order classification semantics
5. Ensure all 4 callers continue to work correctly after refactoring
