# Phase 1: Scope Boundary - EPIC-W7-063

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Execution Time**: 2026-06-24T01:49:25Z
- **Mode**: plan

## Epic Target
- **Method**: DrainAllDispatchQueuesOnAbort
- **File**: src/V12_002.SIMA.Fleet.cs
- **Line**: 287
- **Current CYC**: 12
- **Target CYC**: ≤8 (Jane Street strict standard)

## Scope Definition

### IN SCOPE ✅

#### Primary Extraction Target
**DrainAllDispatchQueuesOnAbort** (CYC 12 → target ≤5)
- Extract queue draining logic
- Extract telemetry tracking
- Extract position delta management
- Extract circuit breaker reset logic
- Retain orchestration in main method

#### Extraction Candidates (based on 25 callees)
1. **Queue Draining Logic**
   - _photonDispatchRing operations
   - TrackPhotonDequeue calls
   - Dequeue loop management
   - Target CYC: ≤3

2. **Telemetry & Tracking**
   - TrackPhotonDequeue
   - Metrics collection
   - Target CYC: ≤2

3. **Position Delta Management**
   - AddExpectedPositionDeltaLocked
   - Position tracking logic
   - Target CYC: ≤3

4. **Sync State Management**
   - ClearDispatchSyncPending
   - Sync flag operations
   - Target CYC: ≤2

5. **Circuit Breaker Reset**
   - TryResetCircuitBreakerIfBelow
   - Circuit breaker state management
   - Target CYC: ≤3

6. **Grace Period Management**
   - StampAccountFillGrace (REAPER)
   - Grace period logic
   - Target CYC: ≤2

#### Files to Modify
- `src/V12_002.SIMA.Fleet.cs` (primary target)

#### Callers to Verify (NO MODIFICATION)
1. PumpFleetDispatch (line 233)
2. ProcessFleetSlot (line 44)
3. VerifyPhotonSlotIntegrity (line 329)
4. ProcessValidPhotonSlot (line 395)

### OUT OF SCOPE ❌

#### Caller Methods (Zero Blast Radius Maintained)
- **PumpFleetDispatch** - No changes
- **ProcessFleetSlot** - No changes
- **VerifyPhotonSlotIntegrity** - No changes
- **ProcessValidPhotonSlot** - No changes

#### Related Subsystems (No Direct Changes)
- Telemetry subsystem (TrackPhotonDequeue interface unchanged)
- SIMA subsystem (AddExpectedPositionDeltaLocked interface unchanged)
- REAPER subsystem (StampAccountFillGrace interface unchanged)
- LogBuffer (logging interface unchanged)
- _photonPool (object pooling unchanged)
- _pendingFleetDispatches (fleet dispatch tracking unchanged)

#### Other High-Complexity Methods
- HydrateFromOpenPositions (CYC=34) - separate epic
- IsCommandForThisInstrument (CYC=38) - separate epic
- HandleTerminated (CYC=30) - separate epic

#### Infrastructure
- Build system
- Test framework
- Deployment scripts
- Hard link synchronization (deploy-sync.ps1 runs post-refactor)

## Scope Boundary Validation

### Inclusion Criteria ✅
- Method CYC >8 (DrainAllDispatchQueuesOnAbort = 12)
- Zero blast radius (safe to refactor)
- Single file modification (V12_002.SIMA.Fleet.cs)
- Clear extraction opportunities (25 callees)
- Aligns with Jane Street strict standard (CYC ≤8)

### Exclusion Criteria ❌
- Methods with CYC ≤8 (already compliant)
- Methods with high blast radius (risky)
- Multi-file refactors (scope creep)
- Caller modifications (zero blast radius principle)
- Subsystem interface changes (stability)

## Risk Mitigation

### Zero Blast Radius Guarantee
- **Importer Count**: 0
- **Direct Dependents**: 0
- **Overall Risk Score**: 0.0
- **Strategy**: Extract helpers, keep public signature unchanged

### Caller Verification Strategy
1. Verify all 4 callers compile after extraction
2. No signature changes to DrainAllDispatchQueuesOnAbort
3. No behavioral changes (pure refactor)
4. Maintain exact same execution paths

### Rollback Plan
- Git branch isolation (GitButler virtual branch)
- Pre-refactor snapshot
- Automated rollback if build fails
- Zero-downtime deployment (hard link sync)

## Success Criteria

### Complexity Reduction
- DrainAllDispatchQueuesOnAbort: CYC 12 → ≤5
- All extracted methods: CYC ≤8
- Total method count: +5-6 new helper methods

### Zero Blast Radius Maintained
- No changes to 4 caller methods
- No changes to subsystem interfaces
- Public signature unchanged

### Build & Test
- `dotnet build` passes
- All unit tests pass
- F5 in NinjaTrader successful
- deploy-sync.ps1 completes

### Code Quality
- CSharpier formatting passes
- ASCII-only compliance
- No lock() statements introduced
- Jane Street patterns followed

## Phase 1 Completion Checklist
- [x] Hotspot analysis reviewed
- [x] IN SCOPE defined (1 method, 5-6 extractions)
- [x] OUT OF SCOPE defined (4 callers, subsystems)
- [x] Scope boundary validated
- [x] Risk mitigation planned
- [x] Success criteria documented
- [ ] Phase 2 (Architecture Planning) ready to start
