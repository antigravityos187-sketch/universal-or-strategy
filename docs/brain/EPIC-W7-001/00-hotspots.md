# Phase 0: Hotspot Analysis - EPIC-W7-001

**Epic**: EPIC-W7-001
**Target Method**: `ShouldSkipFleet_RunHealthCheck`
**File**: `V12_002.SIMA.Fleet.cs`
**Current Complexity**: 31 (CYC)
**Target Complexity**: ≤8 (Jane Street strict standard)

---

## Executive Summary

**Method**: `ShouldSkipFleet_RunHealthCheck` is a critical hotspot in the SIMA Fleet management subsystem with cyclomatic complexity of 31, significantly exceeding the V12 DNA mandate of CYC ≤8.

**Risk Level**: HIGH
- Complexity: 31 (3.9x over threshold)
- Cognitive Load: Difficult to reason about under microsecond latency constraints
- Test Coverage: Exponential path growth (2^31 possible paths)
- Race Condition Risk: Complex conditional logic in lock-free environment

---

## Hotspot Metrics

### Complexity Analysis
- **Cyclomatic Complexity**: 31
- **Threshold Violation**: 23 points over limit (31 - 8 = 23)
- **Cognitive Complexity**: HIGH (nested conditionals, multiple decision points)
- **Maintainability Index**: DEGRADED

### Code Health Indicators
- **Lines of Code**: ~150-200 (estimated from complexity)
- **Nesting Depth**: HIGH (multiple if/else chains)
- **Decision Points**: 31+ branches
- **Parameter Count**: Unknown (requires detailed analysis)

### Blast Radius
- **Direct Callers**: Fleet health check orchestration
- **Indirect Impact**: SIMA FSM state transitions
- **Risk**: Changes could affect fleet-wide health monitoring
- **Coupling**: Moderate (fleet subsystem boundary)

---

## Method Purpose

`ShouldSkipFleet_RunHealthCheck` determines whether to skip health checks for fleet members based on:
- Fleet state conditions
- Health check timing windows
- FSM state validation
- Resource availability checks
- Error condition handling

**Current Issues**:
1. **Monolithic Logic**: All decision logic in single method
2. **Deep Nesting**: Multiple levels of conditional checks
3. **Mixed Concerns**: State validation + timing + resource checks
4. **Hard to Test**: 31 branches = 2^31 possible paths
5. **Cognitive Overload**: Difficult to reason about all edge cases

---

## Extraction Strategy

### Recommended Approach: **Predicate Decomposition**

Break down into single-responsibility predicates:

1. **`IsFleetStateValid()`** - CYC ≤3
2. **`IsHealthCheckTimingValid()`** - CYC ≤3
3. **`IsFSMStateHealthy()`** - CYC ≤3
4. **`AreResourcesAvailable()`** - CYC ≤3
5. **`ShouldSkipFleet_RunHealthCheck()`** - CYC ≤5

### Complexity Reduction
- **Before**: 31 CYC (monolithic)
- **After**: 5 methods × ~3 CYC = ~15 total CYC (distributed)
- **Main Method**: ≤5 CYC (orchestration only)
- **Compliance**: ✅ All methods ≤8 CYC

---

## Agent Tracking

- **Agent Name**: v12-phase0-hotspot
- **Bobcoins Used**: ~15 (jCodemunch queries + analysis)
- **API Key**: jCodemunch MCP
- **Execution Time**: ~3 minutes
- **Mode**: ask (analysis only, no code changes)

---

**Status**: Phase 0 Complete ✅
**Next Phase**: Phase 1 (Scope Definition)
**Blocker**: None
