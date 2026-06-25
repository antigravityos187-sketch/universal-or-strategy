# Phase 0: Hotspot Analysis - EPIC-W7-115

## Agent Tracking
- **Agent Name**: v12-phase0-hotspot
- **Bobcoins Used**: 0.77
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-23T02:56:17Z

## Target Method
- **Method**: SweepTrackedOrders
- **File**: src/V12_002.SIMA.Lifecycle.cs
- **Line**: 1308
- **Cyclomatic Complexity**: 11
- **Lines of Code**: 46

## Complexity Metrics

### Symbol Complexity Analysis
- **Cyclomatic Complexity**: 11 (HIGH - exceeds Jane Street threshold of 8)
- **Max Nesting Depth**: 4
- **Parameter Count**: 1
- **Lines of Code**: 46
- **Assessment**: HIGH complexity

### Complexity Breakdown
The method has a cyclomatic complexity of 11, which exceeds the V12 DNA mandate of CYC ≤ 8 (Jane Street strict standard). This indicates:
- Multiple decision points (11 independent paths)
- Moderate nesting (depth 4)
- Potential for difficult reasoning under microsecond-latency constraints
- Higher risk for race conditions in lock-free code

## Blast Radius

### Direct Impact Analysis
- **Importer Count**: 0
- **Direct Dependents**: 0
- **Overall Risk Score**: 0.0 (LOW)
- **Confirmed Files Affected**: 0
- **Potential Files Affected**: 0

### Interpretation
The blast radius analysis shows ZERO external dependencies:
- No files import this method directly
- No confirmed or potential downstream consumers
- This is an INTERNAL method with isolated scope
- Changes will have MINIMAL ripple effects across the codebase

**Risk Assessment**: LOW blast radius despite HIGH complexity

## Call Hierarchy

### Callers (Who calls this method)
The method is called by 3 internal methods within the same file:

1. **CancelAllV12GtcOrders** (line 1294)
   - Direct caller (depth 1)
   - Resolution: AST resolved

2. **ProcessShutdownSIMA** (line 98)
   - Indirect caller (depth 2)
   - Resolution: AST resolved

3. **ProcessApplySimaState** (line 38)
   - Indirect caller (depth 3)
   - Resolution: AST resolved

### Callees (What this method calls)
The method calls 4 downstream methods:

1. **CancelOrderOnAccount** (src/V12_002.Orders.CancelGateway.cs, line 46)
   - Direct callee (depth 1)
   - Resolution: AST inferred

2. **CancelOrderOnAccount** (src-vm-backup/V12_002.Orders.CancelGateway.cs, line 46)
   - Direct callee (depth 1)
   - Resolution: AST inferred
   - Note: Backup copy detected

3. **IsOrderTerminal** (src/V12_002.Orders.Management.Flatten.cs, line 698)
   - Indirect callee (depth 2)
   - Resolution: AST inferred

4. **IsOrderTerminal** (src-vm-backup/V12_002.Orders.Management.Flatten.cs, line 574)
   - Indirect callee (depth 2)
   - Resolution: AST inferred
   - Note: Backup copy detected

### Call Chain Analysis
- **Caller Depth**: 3 levels (shallow call chain)
- **Callee Depth**: 2 levels (shallow dependency chain)
- **Total Unique Callers**: 3
- **Total Unique Callees**: 4 (2 unique methods, 2 backup copies)

## Hotspot Context

### Position in Top 50 Hotspots
SweepTrackedOrders does NOT appear in the top 50 hotspots by composite score (complexity × log(1 + churn)).

**Top 5 Hotspots for Reference**:
1. HydrateFromOpenPositions (CYC 34, hotspot 120.88)
2. IsCommandForThisInstrument (CYC 38, hotspot 109.83)
3. HandleTerminated (CYC 30, hotspot 102.04)
4. SweepBrokerOrders (CYC 28, hotspot 99.55)
5. HydrateWorkingOrdersFromBroker (CYC 23, hotspot 81.77)

### Interpretation
While SweepTrackedOrders has HIGH complexity (CYC 11), it has:
- Lower churn than top hotspots
- Lower composite risk score
- More isolated scope (zero blast radius)

This suggests the method is STABLE but COMPLEX - a good candidate for refactoring to improve maintainability without high regression risk.

## Risk Assessment

### Overall Risk Profile: MEDIUM

**Factors Contributing to MEDIUM Risk**:
1. ✅ **LOW Blast Radius**: Zero external dependencies, isolated scope
2. ✅ **LOW Churn**: Not in top 50 hotspots, stable over time
3. ❌ **HIGH Complexity**: CYC 11 exceeds Jane Street threshold of 8
4. ✅ **Shallow Call Chains**: Only 3 callers, 4 callees (2 unique)

### Refactoring Recommendation
**PROCEED with refactoring** - This is a LOW-RISK, HIGH-VALUE target:
- Complexity reduction will improve cognitive load
- Minimal blast radius reduces regression risk
- Stable churn history suggests well-understood logic
- Internal scope allows safe extraction without API concerns

### Suggested Approach
1. Extract decision logic into smaller helper methods (CYC ≤ 8 each)
2. Reduce nesting depth from 4 to ≤ 3
3. Maintain single responsibility principle
4. Add unit tests for extracted methods
5. Verify no behavioral changes via existing callers

## Conclusion

SweepTrackedOrders is a **MEDIUM-RISK, HIGH-VALUE** refactoring target:
- HIGH complexity (CYC 11) warrants reduction to meet V12 DNA standards
- LOW blast radius (0 dependents) minimizes regression risk
- STABLE churn history suggests mature, well-understood logic
- INTERNAL scope allows safe extraction without breaking external contracts

**Recommendation**: PROCEED to Phase 1 (Scope Definition)
