# Phase 1: Scope Definition - EPIC-W7-108

**Agent**: v12-phase1-scope
**Epic**: EPIC-W7-108
**Target Method**: DrainPhotonQueuesOnShutdown
**File**: V12_002.SIMA.Lifecycle.cs
**Date**: 2026-06-24

## Scope Boundary Definition

### IN SCOPE

#### Primary Target
- **Method**: `DrainPhotonQueuesOnShutdown`
- **Current CYC**: 11
- **Target CYC**: ≤8 (Jane Street strict standard)
- **File**: `src/V12_002.SIMA.Lifecycle.cs`

#### Extraction Targets (4 Methods)

1. **ValidateQueueState()**
   - **Purpose**: Pre-drain queue state validation
   - **Target CYC**: ≤2
   - **Scope**: Queue null checks, state verification, readiness checks
   - **Returns**: bool (true if ready to drain)

2. **DrainQueueBatch()**
   - **Purpose**: Core queue draining logic
   - **Target CYC**: ≤3
   - **Scope**: Dequeue operations, batch processing, item handling
   - **Returns**: int (items drained)

3. **HandleDrainError()**
   - **Purpose**: Error recovery during drain
   - **Target CYC**: ≤2
   - **Scope**: Exception handling, logging, state rollback
   - **Returns**: void

4. **CleanupQueueState()**
   - **Purpose**: Post-drain cleanup
   - **Target CYC**: ≤2
   - **Scope**: Queue disposal, state reset, resource cleanup
   - **Returns**: void

#### Testing Requirements
- Unit tests for each extracted method (4 tests minimum)
- Integration test for full shutdown sequence
- Verify zero lock() blocks in drain path
- F5 in NinjaTrader successful

#### Documentation Updates
- Update method XML comments
- Document extraction rationale
- Add Jane Street pattern references

### OUT OF SCOPE

#### Excluded Changes
- ❌ Other methods in V12_002.SIMA.Lifecycle.cs (unless direct dependencies)
- ❌ Queue infrastructure modifications (photon queue implementation)
- ❌ Shutdown sequence orchestration (caller methods)
- ❌ SIMA FSM state machine logic (separate concern)
- ❌ Performance optimizations beyond complexity reduction
- ❌ Logging framework changes
- ❌ Error handling strategy changes (use existing patterns)

#### Deferred to Future Epics
- Queue performance tuning
- Shutdown timeout configuration
- Metrics collection during drain
- Alternative drain strategies

### Scope Validation

#### Complexity Budget
- **Starting CYC**: 11
- **Target CYC**: ≤8
- **Reduction Required**: 3 points minimum
- **Extracted Methods Total**: ≤9 (4 methods × avg 2.25 CYC)
- **Main Method Remaining**: ≤3 (orchestration only)

#### Blast Radius
- **Files Modified**: 1 (V12_002.SIMA.Lifecycle.cs)
- **Methods Added**: 4 (all private)
- **Methods Modified**: 1 (DrainPhotonQueuesOnShutdown)
- **Test Files Added**: 1 (DrainPhotonQueuesTests.cs)

#### Risk Assessment
- **Risk Level**: Medium
- **Rationale**: Shutdown path - errors could leave queues inconsistent
- **Mitigation**: Comprehensive testing, gradual extraction, rollback plan

### Jane Street Alignment

#### Applicable Patterns
1. **Correctness by Construction**
   - Type-safe queue state transitions
   - Impossible to drain invalid queue

2. **Lock-Free Actor Pattern**
   - Verify no lock() blocks
   - Use atomic operations or FSM Enqueue

3. **Cognitive Simplicity**
   - Each method single responsibility
   - CYC ≤8 for microsecond-latency reasoning

#### Testing Standards
- Exhaustive path coverage (CYC ≤8 enables this)
- Race condition audit (lock-free verification)
- Shutdown stress test (concurrent scenarios)

### Success Criteria

#### Phase 1 Complete
- ✅ Scope boundaries clearly defined
- ✅ IN SCOPE: 4 extraction targets identified
- ✅ OUT OF SCOPE: Exclusions documented
- ✅ Complexity budget validated
- ✅ Blast radius assessed

#### Epic Success (Phase 6)
- DrainPhotonQueuesOnShutdown CYC ≤3
- All 4 extracted methods CYC ≤8
- Zero lock() blocks in drain path
- 100% test coverage for shutdown sequence
- F5 in NinjaTrader successful
- deploy-sync.ps1 executed successfully

### Dependencies

#### Prerequisites
- ✅ Phase 0 complete (hotspot analysis)
- ✅ jCodemunch index current
- ✅ Git status clean

#### Blockers
- None identified

### Execution Constraints

#### Branch Strategy
- GitButler virtual branch: `epic-w7-108-drain-queues`
- Physical branch: `gitbutler/workspace`

#### Build Requirements
- Zero compilation errors
- CSharpier formatting pass
- ASCII-only compliance

#### Deployment
- Hard link sync via deploy-sync.ps1
- NinjaTrader F5 verification

## Agent Tracking

- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.12 (estimated)
- **API Key**: premium
- **Execution Time**: <3 minutes

## Next Phase

**Phase 1.5**: Scope Boundary Validation
- Jane Street review of extraction strategy
- Verify no scope creep
- Confirm complexity budget realistic
