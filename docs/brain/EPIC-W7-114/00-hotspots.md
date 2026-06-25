# Phase 0: Hotspot Analysis - EPIC-W7-114

## Agent Tracking
- **Agent Name**: v12-phase0-hotspot
- **Bobcoins Used**: 0.74
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-23T02:56:03Z

## Target Method
- **Method**: ProcessShutdownSIMA
- **File**: src/V12_002.SIMA.Lifecycle.cs
- **Line**: 98
- **Cyclomatic Complexity**: 15 (actual, not 11 as initially reported)
- **Lines of Code**: 41
- **Max Nesting Depth**: 4
- **Parameter Count**: 0

## Complexity Metrics

### Assessment: HIGH COMPLEXITY
- **Cyclomatic Complexity**: 15 (threshold: ≤8 per Jane Street standard)
- **Complexity Rating**: HIGH (exceeds threshold by 87.5%)
- **Max Nesting Depth**: 4 levels
- **Method Size**: 41 lines
- **Parameters**: 0 (good - no parameter complexity)

### Complexity Breakdown
The method has 15 decision points, indicating multiple conditional branches and control flow paths. This exceeds the V12 DNA mandate of CYC ≤8 for microsecond-latency reasoning and exhaustive testing.

## Blast Radius Analysis

### Direct Impact: MINIMAL
- **Importer Count**: 0
- **Direct Dependents**: 0
- **Overall Risk Score**: 0.0
- **Confirmed Files Affected**: 0
- **Potential Files Affected**: 0

### Interpretation
The method has **zero external blast radius**. No other files import or directly depend on this method, making it an excellent refactoring candidate with minimal ripple effects.

## Call Hierarchy

### Callers (Who Calls This Method)
**Total Callers**: 1

1. **ProcessApplySimaState** (src/V12_002.SIMA.Lifecycle.cs:38)
   - Resolution: ast_resolved
   - Depth: 1
   - This is the only entry point to ProcessShutdownSIMA

### Callees (What This Method Calls)
**Total Callees**: 32

#### Depth 1 Callees (Direct Calls)
1. **CancelAllV12GtcOrders** (src/V12_002.SIMA.Lifecycle.cs:1294) - ast_resolved
2. **StopReaperAudit** (src/V12_002.REAPER.cs:143) - ast_inferred
3. **UnsubscribeFromFleetAccounts** (src/V12_002.SIMA.Fleet.cs:641) - ast_inferred
4. **_photonDispatchRing** (constant access)
5. **AddExpectedPositionDelta** (src/V12_002.cs:871) - ast_inferred
6. **ClearDispatchSyncPending** (src/V12_002.SIMA.cs:179) - ast_inferred
7. **_photonPool** (constant access)
8. **_pendingFleetDispatches** (constant access)

#### Depth 2 Callees (Transitive Calls)
9. **SweepTrackedOrders** (src/V12_002.SIMA.Lifecycle.cs:1308) - ast_resolved
10. **SweepBrokerOrders** (src/V12_002.SIMA.Lifecycle.cs:1360) - ast_resolved
11. **LogBuffer.Format** (src/V12_002.Perf.LogBuffer.cs:28) - ast_inferred
12. **_reaperTimer** (constant access)
13. **IsFleetAccount** (src/V12_002.cs:864) - ast_inferred
14. **_subscribedAccountNames** (constant access)
15. **AddExpectedPositionDeltaLocked** (src/V12_002.SIMA.cs:88) - ast_inferred
16. **_dispatchSyncPendingExpKeys** (constant access)

### Call Hierarchy Insights
- **Single Entry Point**: Only called by ProcessApplySimaState, making it easy to trace usage
- **Wide Fanout**: Calls 32 different methods/constants, indicating high internal complexity
- **Cross-Module Dependencies**: Touches REAPER, Fleet, SIMA, and core V12_002 modules
- **State Management**: Heavy interaction with dispatch rings, pools, and pending queues

## Risk Assessment

### Overall Risk: MEDIUM-HIGH

#### Risk Factors
1. **HIGH Complexity** (CYC=15): Exceeds Jane Street threshold by 87.5%
2. **Wide Fanout** (32 callees): High internal coupling
3. **Cross-Module Impact**: Touches 5+ different subsystems
4. **State Management**: Manipulates critical dispatch and pool structures

#### Mitigating Factors
1. **Zero Blast Radius**: No external dependents
2. **Single Caller**: Easy to test and validate
3. **Clear Scope**: Shutdown logic is well-defined
4. **No Parameters**: Reduces input complexity

### Refactoring Recommendation: PROCEED WITH CAUTION

**Confidence Level**: HIGH (85%)

**Rationale**:
- The zero blast radius makes this a safe refactoring target
- The single caller simplifies testing
- However, the wide fanout (32 callees) requires careful extraction to avoid breaking internal state management
- Recommend extracting into 3-4 focused helper methods, each with CYC ≤8

### Suggested Extraction Strategy
1. **Extract Fleet Cleanup** (UnsubscribeFromFleetAccounts + related logic)
2. **Extract Order Cancellation** (CancelAllV12GtcOrders + sweeps)
3. **Extract Reaper Shutdown** (StopReaperAudit + timer cleanup)
4. **Extract Dispatch Cleanup** (dispatch ring/pool/queue cleanup)

Each extracted method should:
- Have CYC ≤8
- Have a single, clear responsibility
- Maintain the same call order as the original
- Be testable in isolation

## Next Steps (Phase 1)
1. Review the method source code in detail
2. Identify exact extraction boundaries
3. Validate that extracted methods maintain shutdown semantics
4. Create detailed scope document with extraction plan
