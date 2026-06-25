# Phase 0: Hotspot Analysis - EPIC-W7-049

## Agent Tracking
- **Agent Name**: v12-phase0-hotspot
- **Bobcoins Used**: 0.77
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-23T02:43:55Z

## Target Method
- **Method**: ManageTrail_RunPerTradeBranches
- **File**: src/V12_002.Trailing.cs
- **Line**: 240
- **Cyclomatic Complexity**: 11 (ACTUAL - updated from initial 9)
- **Assessment**: HIGH

## Complexity Metrics

### Symbol Complexity Analysis
- **Cyclomatic Complexity**: 11
- **Max Nesting Depth**: 1
- **Parameter Count**: 2
- **Lines of Code**: 16
- **Assessment**: HIGH (exceeds Jane Street threshold of 8)

**Rationale for HIGH Assessment**:
- CYC 11 exceeds V12 DNA mandate (≤8)
- Jane Street HFT systems require cognitive simplicity
- Functions >8 are harder to reason about under microsecond latency
- Exponential path growth for exhaustive testing

## Blast Radius

### Impact Analysis
- **Importer Count**: 0
- **Direct Dependents**: 0
- **Overall Risk Score**: 0.0
- **Confirmed Files**: 0
- **Potential Files**: 0

**Interpretation**:
- LOW blast radius - method is internally called only
- No external dependencies detected
- Safe for refactoring with minimal ripple effects

## Call Hierarchy

### Callers (Depth 1)
1. **ManageTrailingStops** (src/V12_002.Trailing.cs:39)
   - Resolution: ast_resolved
   - Single entry point for this method

### Callees (Depth 1-3)
**Direct Callees (Depth 1)**:
1. TrailHandler_TREND_E1 (src/V12_002.Trailing.cs:257)
2. TrailHandler_TREND_E2 (src/V12_002.Trailing.cs:312)
3. TrailHandler_RETEST (src/V12_002.Trailing.cs:342)

**Indirect Callees (Depth 2)**:
4. LogBuffer.Format (src/V12_002.Perf.LogBuffer.cs:28)
5. UpdateStopOrder (src/V12_002.Trailing.StopUpdate.cs:84)

**Deep Callees (Depth 3)**:
6. LogBuffer.ValidateThreadAffinity (src/V12_002.Perf.LogBuffer.cs:119)
7. LogBuffer.FormatInternal (src/V12_002.Perf.LogBuffer.cs:56)
8. stopOrders constant (src/V12_002.cs:201)
9. ValidateStopPrice (src/V12_002.Orders.Management.StopSync.cs:1200)
10. pendingStopReplacements constant (src/V12_002.cs:210)
11. HandleStalePendingReplacement (src/V12_002.Trailing.StopUpdate.cs:141)
12. UpdateExistingPendingReplacement (src/V12_002.Trailing.StopUpdate.cs:167)
13. InitiateStopReplacement (src/V12_002.Trailing.StopUpdate.cs:307)
14. CreateDirectStopOrder (src/V12_002.Trailing.StopUpdate.cs:371)
15. HandleUpdateException (src/V12_002.Trailing.StopUpdate.cs:496)

**Total Callees**: 30 (including duplicates from src-vm-backup)

## Repository Hotspot Context

### Top 5 Hotspots (CYC × log(1 + churn))
1. **HydrateFromOpenPositions** (CYC 34, churn 34, score 120.88) - SIMA.Lifecycle.cs
2. **IsCommandForThisInstrument** (CYC 38, churn 17, score 109.83) - UI.IPC.cs
3. **HandleTerminated** (CYC 30, churn 29, score 102.04) - Lifecycle.cs
4. **SweepBrokerOrders** (CYC 28, churn 34, score 99.55) - SIMA.Lifecycle.cs
5. **HydrateWorkingOrdersFromBroker** (CYC 23, churn 34, score 81.77) - SIMA.Lifecycle.cs

**ManageTrail_RunPerTradeBranches Position**: Not in top 50 hotspots (CYC 11 with unknown churn)

## Risk Assessment

### Overall Risk: MEDIUM

**Factors**:
- LOW Blast Radius: No external dependencies, single caller
- HIGH Complexity: CYC 11 exceeds threshold of 8
- LOW Churn: Not in top 50 hotspots (implies stable code)
- MEDIUM Call Depth: 30 callees across 3 levels

### Refactoring Recommendation
**PROCEED WITH CAUTION**

**Strengths**:
- Isolated method with minimal blast radius
- Single entry point simplifies testing
- Stable code (low churn)

**Risks**:
- Complexity exceeds Jane Street threshold
- Deep call hierarchy (3 levels, 30 callees)
- Multiple trail handler branches (TREND_E1, TREND_E2, RETEST)

**Suggested Approach**:
1. Extract each trail handler branch into separate methods
2. Target CYC ≤8 per extracted method
3. Maintain single entry point (ManageTrailingStops)
4. Add unit tests for each extracted branch

## Jane Street Alignment

**Query Recommendations**:
- python scripts/query_kb.py "complexity reduction"
- python scripts/query_kb.py "branch extraction"
- python scripts/query_kb.py "trailing stop patterns"

**Expected Patterns**:
- Strategy pattern for trail handlers
- Guard clauses for early returns
- Single-responsibility principle per branch

## Next Steps

1. **Phase 1**: Scope definition and boundary validation
2. **Phase 2**: Architecture planning with Jane Street KB queries
3. **Phase 3**: DNA audit and PR hygiene check
4. **Phase 4**: Ticket generation (3 tickets expected - one per branch)
5. **Phase 5**: Surgical extraction with Bob CLI (v12-engineer mode)

---

**Analysis Complete**: 2026-06-23T02:43:55Z
