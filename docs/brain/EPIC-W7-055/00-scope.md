# Phase 1: Scope Definition - EPIC-W7-055

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.18
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T20:07:03Z

## Target Method
- **Method**: DrainPhotonQueuesOnShutdown
- **File**: src/V12_002.SIMA.Lifecycle.cs
- **Line**: 165 (estimated)
- **Current CYC**: 11
- **Target CYC**: ≤8 (Jane Street strict standard)

## Scope Boundary Analysis

### IN SCOPE

#### Primary Extraction Target
**DrainPhotonQueuesOnShutdown** (CYC 11 → target ≤8)
- **Rationale**: Method exceeds Jane Street threshold by +3 points
- **Isolation**: Zero blast radius - no external dependencies
- **Stability**: Low churn (not in top 50 hotspots)
- **Safety**: Only 2 callers, both in same file

#### Extraction Candidates (4 methods)

1. **DrainPhotonDispatchRing()** - Target CYC ≤3
   - Extract photon dispatch ring cleanup logic
   - Handle ring iteration and item processing
   - Maintain lock-free pattern

2. **DrainPhotonPool()** - Target CYC ≤3
   - Extract photon pool cleanup logic
   - Handle pool iteration and item processing
   - Maintain lock-free pattern

3. **ClearPendingFleetDispatches()** - Target CYC ≤3
   - Extract fleet dispatch cleanup logic
   - Handle pending dispatch clearing
   - Maintain lock-free pattern

4. **SyncExpectedPositionDeltas()** - Target CYC ≤4
   - Extract position delta synchronization
   - Handle AddExpectedPositionDelta calls
   - Handle ClearDispatchSyncPending calls
   - Maintain lock-free pattern

#### Scope Constraints
- **File Boundary**: All work contained in src/V12_002.SIMA.Lifecycle.cs
- **No New Dependencies**: Zero blast radius must be maintained
- **No Interface Changes**: Method signature remains unchanged
- **No Behavioral Changes**: Shutdown sequence must behave identically
- **Lock-Free Mandate**: No new lock() blocks permitted
- **ASCII-Only Mandate**: All string literals must be ASCII-only

### OUT OF SCOPE

#### Caller Methods (Not Modified)
1. **ProcessShutdownSIMA** (line 144)
   - Rationale: Caller orchestration logic is separate concern
   - Risk: Modifying caller increases blast radius unnecessarily
   - Decision: Keep caller unchanged, only refactor target method

2. **ProcessApplySimaState** (line 70)
   - Rationale: State application logic is separate concern
   - Risk: Modifying caller increases blast radius unnecessarily
   - Decision: Keep caller unchanged, only refactor target method

#### Callee Methods (Not Modified)
- **AddExpectedPositionDelta** - Existing helper, no changes needed
- **ClearDispatchSyncPending** - Existing helper, no changes needed
- **AddExpectedPositionDeltaLocked** - Nested call, no changes needed

#### Data Structures (Not Modified)
- **_photonDispatchRing** - Existing data structure, no changes
- **_photonPool** - Existing data structure, no changes
- **_pendingFleetDispatches** - Existing data structure, no changes
- **_dispatchSyncPendingExpKeys** - Existing data structure, no changes

#### Related Hotspots (Separate Epics)
- **HydrateFromOpenPositions** (CYC 34) - EPIC-W7-001
- **IsCommandForThisInstrument** (CYC 38) - EPIC-W7-002
- **HandleTerminated** (CYC 30) - EPIC-W7-003
- **SweepBrokerOrders** (CYC 28) - EPIC-W7-004
- **HydrateWorkingOrdersFromBroker** (CYC 23) - EPIC-W7-005

#### Testing Infrastructure (Separate Work)
- Integration test framework setup - Not part of this epic
- Stress testing infrastructure - Not part of this epic
- Coverage tooling - Not part of this epic

## Extraction Strategy

### Step 1: Extract Queue Draining Methods (3 methods)
Extract three single-responsibility methods for queue cleanup:
- DrainPhotonDispatchRing() - CYC ≤3
- DrainPhotonPool() - CYC ≤3
- ClearPendingFleetDispatches() - CYC ≤3

### Step 2: Extract Position Delta Synchronization (1 method)
Extract position delta handling into helper:
- SyncExpectedPositionDeltas() - CYC ≤4

### Step 3: Simplify Main Method
Refactor DrainPhotonQueuesOnShutdown to orchestrate extracted methods:
- Call DrainPhotonDispatchRing()
- Call DrainPhotonPool()
- Call ClearPendingFleetDispatches()
- Call SyncExpectedPositionDeltas()
- Target CYC ≤8 for orchestration logic

### Step 4: Verification
- Unit test each extracted method independently
- Integration test shutdown sequence
- Verify no behavioral changes
- Verify lock-free pattern maintained
- Verify ASCII-only compliance

## Risk Mitigation

### Low Risk Factors
- **Zero Blast Radius**: No external dependencies to break
- **Stable Code**: Low churn rate (not in top 50 hotspots)
- **Clear Callers**: Only 2 callers, both in same file
- **No Parameters**: Zero parameter coupling reduces interface complexity

### Medium Risk Factors
- **Shutdown Critical**: Method is in shutdown path - must maintain correctness
- **Multi-Subsystem**: Coordinates across 3+ queue types
- **Moderate Nesting**: 4 levels of nesting requires careful extraction
- **Integration Testing**: Shutdown sequence requires end-to-end testing

### Mitigation Strategy
1. **Preserve Behavior**: Extract methods without changing logic
2. **Unit Test First**: Test extracted methods before integration
3. **Integration Test**: Test full shutdown sequence after extraction
4. **Incremental Approach**: Extract one method at a time, verify after each
5. **Lock-Free Audit**: Verify no new lock() blocks introduced
6. **ASCII Audit**: Verify no Unicode characters introduced

## Success Criteria

### Complexity Targets
- [ ] DrainPhotonQueuesOnShutdown: CYC ≤8 (currently 11)
- [ ] DrainPhotonDispatchRing: CYC ≤3 (new method)
- [ ] DrainPhotonPool: CYC ≤3 (new method)
- [ ] ClearPendingFleetDispatches: CYC ≤3 (new method)
- [ ] SyncExpectedPositionDeltas: CYC ≤4 (new method)

### Quality Gates
- [ ] Zero blast radius maintained (no new external dependencies)
- [ ] Shutdown behavior preserved (critical path)
- [ ] Unit tests added for all extracted methods
- [ ] Integration test for shutdown sequence
- [ ] ASCII-only compliance maintained
- [ ] Lock-free pattern preserved (no new locks)
- [ ] Build passes (dotnet build)
- [ ] Deploy sync successful (deploy-sync.ps1)
- [ ] F5 in NinjaTrader successful

### Documentation
- [x] 00-scope.md created (this file)
- [ ] manifest.json updated with phase1 status
- [x] Extraction strategy documented
- [x] Risk mitigation documented

## Scope Boundary Validation

### Boundary Check: IN SCOPE
- ✅ DrainPhotonQueuesOnShutdown (target method)
- ✅ 4 extracted methods (new code)
- ✅ Unit tests for extracted methods
- ✅ Integration test for shutdown sequence

### Boundary Check: OUT OF SCOPE
- ✅ Caller methods (ProcessShutdownSIMA, ProcessApplySimaState)
- ✅ Callee methods (AddExpectedPositionDelta, ClearDispatchSyncPending)
- ✅ Data structures (_photonDispatchRing, _photonPool, etc.)
- ✅ Related hotspots (separate epics)
- ✅ Testing infrastructure (separate work)

### Scope Creep Prevention
- **ONE EPIC = ONE CONCERN**: Only refactor DrainPhotonQueuesOnShutdown
- **No Pre-Existing Fixes**: Do not fix unrelated compilation errors
- **No "While We're Here"**: Do not add unrelated improvements
- **No Bundling**: Do not mix multiple concerns in one PR

## Notes
- Method is in src/V12_002.SIMA.Lifecycle.cs (active version)
- Shutdown path is critical - requires careful testing
- Must maintain lock-free Actor pattern (no new locks)
- Must maintain ASCII-only compliance
- Zero blast radius makes this a good refactoring candidate
- Low churn rate indicates stable code (good for refactoring)
