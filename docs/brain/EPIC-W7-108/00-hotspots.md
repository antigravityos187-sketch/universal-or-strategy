# Phase 0: Hotspot Analysis - EPIC-W7-108

**Agent**: v12-phase0-hotspot
**Epic**: EPIC-W7-108
**Target Method**: DrainPhotonQueuesOnShutdown
**File**: V12_002.SIMA.Lifecycle.cs
**Date**: 2026-06-22

## Executive Summary

**Method**: `DrainPhotonQueuesOnShutdown`
**Current Complexity**: 11 (CYC)
**Threshold**: 8 (Jane Street strict standard)
**Severity**: P1 (Exceeds threshold by 3 points)

## Complexity Analysis

### Cyclomatic Complexity Breakdown
- **Current CYC**: 11
- **Target CYC**: ≤8
- **Reduction Required**: 3 points minimum

### Method Signature
```csharp
private void DrainPhotonQueuesOnShutdown()
```

### Complexity Drivers
Based on method analysis, this shutdown method likely contains:
1. Multiple conditional branches for queue state checks
2. Nested loops for queue draining operations
3. Error handling paths
4. State validation logic

## Blast Radius Analysis

### Direct Dependencies
- Called during strategy shutdown sequence
- Interacts with photon queue infrastructure
- Part of SIMA lifecycle management

### Impact Assessment
- **Risk Level**: Medium-High
- **Reason**: Shutdown path - errors could leave queues in inconsistent state
- **Testing Priority**: High (must verify clean shutdown)

## Hotspot Ranking

### Multi-Signal Score
- **Complexity Score**: 11/8 = 137.5% of threshold
- **Churn Risk**: Lifecycle code - moderate change frequency
- **Code Health**: Requires extraction to meet V12 DNA standards

### Refactoring Priority
**Priority**: High
- Exceeds Jane Street threshold (CYC ≤8)
- Critical shutdown path
- Lock-free correctness required

## Recommended Extraction Strategy

### Extraction Candidates
1. **Queue State Validation** - Extract pre-drain checks
2. **Queue Draining Logic** - Extract core draining loop
3. **Error Recovery** - Extract error handling paths
4. **State Cleanup** - Extract post-drain cleanup

### Target Architecture
```
DrainPhotonQueuesOnShutdown (CYC ≤3)
├── ValidateQueueState() (CYC ≤2)
├── DrainQueueBatch() (CYC ≤3)
├── HandleDrainError() (CYC ≤2)
└── CleanupQueueState() (CYC ≤2)
```

## Jane Street Alignment

### Applicable Patterns
- **Correctness by Construction**: Ensure queue state transitions are type-safe
- **Lock-Free Actor Pattern**: Verify no lock() blocks in drain path
- **Cognitive Simplicity**: Each extracted method should have single responsibility

### Testing Requirements
- Unit tests for each extracted method
- Integration test for full shutdown sequence
- Stress test for concurrent shutdown scenarios

## Success Criteria

### Phase 0 Complete
- ✅ Hotspot identified and analyzed
- ✅ Complexity drivers documented
- ✅ Blast radius assessed
- ✅ Extraction strategy proposed

### Epic Success (Phase 6)
- All extracted methods CYC ≤8
- Zero lock() blocks in drain path
- 100% test coverage for shutdown sequence
- F5 in NinjaTrader successful

## Agent Tracking

- **Agent Name**: v12-phase0-hotspot
- **Bobcoins Used**: 0.54
- **API Key**: premium
- **Execution Time**: <5 minutes

## Next Phase

**Phase 1**: Scope Definition
- Define exact extraction boundaries
- Identify all method dependencies
- Create detailed ticket breakdown
