# Phase 1: Scope Boundary - EPIC-W7-055

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Mode**: plan
- **Execution Time**: 2026-06-24T01:31:49Z

## Epic Target
- **Method**: DrainPhotonQueuesOnShutdown
- **File**: src/V12_002.SIMA.Lifecycle.cs
- **Current CYC**: 11
- **Target CYC**: ≤8 (Jane Street strict standard)
- **Overage**: +3 points

## IN SCOPE

### Primary Extraction Targets
1. **DrainPhotonDispatchRing()** - Extract photon dispatch ring draining logic
   - Target CYC: ≤3
   - Responsibility: Drain _photonDispatchRing queue
   - Pattern: Single-purpose helper method

2. **DrainPhotonPool()** - Extract photon pool draining logic
   - Target CYC: ≤3
   - Responsibility: Drain _photonPool queue
   - Pattern: Single-purpose helper method

3. **ClearPendingFleetDispatches()** - Extract fleet dispatch cleanup
   - Target CYC: ≤3
   - Responsibility: Clear _pendingFleetDispatches queue
   - Pattern: Single-purpose helper method

4. **SyncExpectedPositionDeltas()** - Extract position delta synchronization
   - Target CYC: ≤4
   - Responsibility: Manage expected position deltas and dispatch sync state
   - Pattern: Coordination helper method

### Refactoring Scope
- **Control Flow Simplification**: Reduce nested conditionals via early returns
- **Guard Clause Extraction**: Extract null/empty checks to method entry
- **Loop Flattening**: Reduce nesting depth from 4 to ≤2
- **Method Signature**: Preserve zero-parameter signature (no interface changes)

### Testing Scope
- **Unit Tests**: Add tests for each extracted method (4 new test methods)
- **Integration Test**: Verify shutdown sequence behavior unchanged
- **Stress Test**: Verify queue draining under load conditions

### Quality Gates
- All extracted methods CYC ≤8
- ASCII-only compliance maintained
- Lock-free Actor pattern preserved (no new locks)
- Zero blast radius maintained (no new external dependencies)
- Build passes after extraction
- All tests pass

## OUT OF SCOPE

### Caller Modifications
- **ProcessShutdownSIMA** - No changes to caller logic
- **ProcessApplySimaState** - No changes to caller logic
- Callers will continue to invoke DrainPhotonQueuesOnShutdown with same signature

### Callee Modifications
- **AddExpectedPositionDelta** - No changes to existing helper methods
- **ClearDispatchSyncPending** - No changes to existing helper methods
- **AddExpectedPositionDeltaLocked** - No changes to nested calls
- All 14 existing callees remain unchanged

### Data Structure Changes
- **_photonDispatchRing** - No structural changes to queue
- **_photonPool** - No structural changes to pool
- **_pendingFleetDispatches** - No structural changes to collection
- **_dispatchSyncPendingExpKeys** - No structural changes to sync state

### Behavioral Changes
- **Shutdown Sequence**: No changes to shutdown order or timing
- **Queue Draining Logic**: Preserve exact draining behavior
- **Error Handling**: Preserve existing error handling patterns
- **Logging**: Preserve existing logging (may add debug logs for extracted methods)

### Cross-File Changes
- **Zero Blast Radius**: No changes to files outside src/V12_002.SIMA.Lifecycle.cs
- **No New Dependencies**: No new imports or external references
- **No Interface Changes**: No changes to public/internal method signatures

### Performance Optimization
- **No Algorithmic Changes**: Preserve existing algorithms
- **No Caching**: No new caching mechanisms
- **No Parallelization**: No new async/parallel patterns
- This epic focuses on complexity reduction, not performance tuning

### Related Hotspots
- **HydrateFromOpenPositions** (CYC 34) - Separate epic
- **IsCommandForThisInstrument** (CYC 38) - Separate epic
- **HandleTerminated** (CYC 30) - Separate epic
- **SweepBrokerOrders** (CYC 28) - Separate epic
- These higher-priority hotspots are out of scope for EPIC-W7-055

## Scope Validation

### Complexity Budget
- **Current**: DrainPhotonQueuesOnShutdown CYC 11
- **After Extraction**:
  - DrainPhotonQueuesOnShutdown: CYC ≤5 (orchestrator)
  - DrainPhotonDispatchRing: CYC ≤3
  - DrainPhotonPool: CYC ≤3
  - ClearPendingFleetDispatches: CYC ≤3
  - SyncExpectedPositionDeltas: CYC ≤4
- **Total CYC**: ≤18 (distributed across 5 methods, all ≤8)

### Risk Mitigation
- **Zero Blast Radius**: Changes isolated to single file
- **Stable Code**: Low churn rate (not in top 50 hotspots)
- **Clear Call Hierarchy**: Only 2 callers, both in same file
- **No Parameter Coupling**: Zero parameters reduces interface risk
- **Critical Path**: Shutdown sequence requires careful testing

### Success Criteria
✅ All extracted methods have CYC ≤8
✅ Zero blast radius maintained
✅ Shutdown behavior preserved
✅ Unit tests added for extracted methods
✅ Integration test for shutdown sequence
✅ ASCII-only compliance maintained
✅ Lock-free pattern preserved
✅ Build passes
✅ All tests pass

## Boundary Enforcement

### Phase 1.5 Gate (Scope Boundary Validation)
Before proceeding to Phase 2 (Architecture Planning), verify:
1. Scope is focused on single method (DrainPhotonQueuesOnShutdown)
2. No scope creep into caller/callee modifications
3. No data structure changes
4. No cross-file changes
5. Extraction targets are clearly defined (4 methods)
6. CYC targets are achievable (all ≤8)
7. Testing scope is comprehensive but bounded

### Scope Creep Prevention
- **ONE EPIC = ONE CONCERN**: Only DrainPhotonQueuesOnShutdown complexity reduction
- **No "While We're Here" Fixes**: Resist temptation to fix unrelated issues
- **Separate PRs**: Any pre-existing issues found must be separate PRs
- **Director Approval Required**: Any scope expansion requires explicit approval

## Notes
- Method is in active version (src/V12_002.SIMA.Lifecycle.cs)
- Shutdown path is critical - requires careful testing
- Must maintain lock-free Actor pattern (no new locks)
- Must maintain ASCII-only compliance
- Zero blast radius makes this an ideal refactoring candidate
- Low churn rate indicates stable code (good for refactoring)
