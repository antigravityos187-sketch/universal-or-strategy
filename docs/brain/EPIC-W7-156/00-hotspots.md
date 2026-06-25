# Phase 0: Hotspot Analysis - EPIC-W7-156

## Agent Tracking
- **Agent Name**: v12-phase0-hotspot
- **Bobcoins Used**: 0.74
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-23T03:33:43Z

## Target Method
- **Method**: CancelAll_ProcessSingleFleetAccount
- **File**: src/V12_002.UI.IPC.Commands.Fleet.cs
- **Line**: 300
- **Cyclomatic Complexity**: 18
- **Assessment**: HIGH

## Complexity Metrics
- **Cyclomatic Complexity**: 18 (Jane Street threshold: ≤8)
- **Max Nesting Depth**: 4
- **Parameter Count**: 2
- **Lines of Code**: 44
- **Assessment**: HIGH complexity - requires refactoring

### Complexity Analysis
The method exceeds the Jane Street strict standard (CYC ≤8) by 10 points. With CYC=18, this indicates:
- Multiple decision paths (likely nested if/else or switch statements)
- Moderate nesting depth (4 levels)
- Reasonable size (44 lines) but high branching logic
- HIGH risk for bugs and maintenance issues

## Blast Radius
- **Importer Count**: 0
- **Direct Dependents**: 0
- **Overall Risk Score**: 0.0
- **Confirmed Files**: 0
- **Potential Files**: 0

### Blast Radius Analysis
**ISOLATED METHOD** - This is excellent news for refactoring:
- No external files import this method
- No cross-file dependencies
- Changes are contained within the same file
- Very low risk of breaking external code
- Ideal candidate for surgical refactoring

## Call Hierarchy

### Callers (Who calls this method)
1. **CancelAll_ProcessFleetOrders** (depth 1)
   - File: src/V12_002.UI.IPC.Commands.Fleet.cs
   - Line: 275
   - Resolution: ast_resolved

2. **CancelAll_ProcessFleetAccounts** (depth 2)
   - File: src/V12_002.UI.IPC.Commands.Fleet.cs
   - Line: 268
   - Resolution: ast_resolved

### Callees (What this method calls)
1. **CancelOrderOnAccount** (depth 1)
   - File: src/V12_002.Orders.CancelGateway.cs
   - Line: 46
   - Resolution: ast_inferred

2. **IsOrderTerminal** (depth 2)
   - File: src/V12_002.Orders.Management.Flatten.cs
   - Line: 698
   - Resolution: ast_inferred

### Call Hierarchy Analysis
- **Caller Count**: 2 (both in same file)
- **Callee Count**: 2 unique methods
- **Depth Reached**: 2 levels
- **Pattern**: Fleet order cancellation workflow
- **Scope**: Internal to Fleet IPC command processing

## Risk Assessment

### Overall Risk: **MEDIUM-LOW**

**Factors Contributing to MEDIUM-LOW Risk**:
1. ✅ **Isolated Blast Radius**: Zero external dependencies
2. ✅ **Same-File Callers**: Both callers are in the same file
3. ✅ **Clear Purpose**: Fleet account order cancellation logic
4. ⚠️ **High Complexity**: CYC=18 requires careful extraction
5. ⚠️ **Moderate Nesting**: 4 levels of nesting needs attention

### Refactoring Strategy Recommendation
- **Approach**: Extract decision logic into helper methods
- **Target**: Reduce CYC from 18 to ≤8 per method
- **Risk**: LOW - isolated method with clear boundaries
- **Effort**: MEDIUM - 44 lines with 18 decision points
- **Priority**: HIGH - exceeds Jane Street threshold by 10 points

### Success Criteria for Refactoring
1. All extracted methods have CYC ≤8
2. Original method becomes orchestrator (CYC ≤5)
3. No change to external behavior
4. All tests pass (if tests exist)
5. Build succeeds with deploy-sync.ps1

## Hotspot Score Calculation
Using CodeScene methodology: `hotspot_score = complexity × log(1 + churn)`

**Note**: Churn data not available in Phase 0. Will be calculated in Phase 1 if git history analysis is performed.

**Estimated Hotspot Score** (assuming moderate churn):
- Complexity: 18
- Estimated log(1 + churn): ~2.0 (moderate)
- **Estimated Hotspot Score**: ~36

This places the method in the **HIGH PRIORITY** category for refactoring.

## Next Steps (Phase 1)
1. Define exact scope boundaries
2. Analyze git churn history
3. Review method implementation details
4. Identify extraction candidates
5. Plan ticket breakdown strategy
