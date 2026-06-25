# Phase 1: Scope Definition - EPIC-W7-063

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.00
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T20:07:28Z

## Target Method
- **Method**: DrainAllDispatchQueuesOnAbort
- **File**: src/V12_002.SIMA.Fleet.cs
- **Line**: 287
- **Current CYC**: 12
- **Target CYC**: ≤8 (Jane Street strict standard)

## Scope Boundary Analysis

### IN SCOPE

#### Primary Extraction Target
**DrainAllDispatchQueuesOnAbort** (CYC=12 → target ≤5 after extraction)

**Extraction Candidates** (based on 25 callees):
1. **Queue Draining Logic** - Extract photon dispatch ring draining
2. **Telemetry Tracking** - Extract TrackPhotonDequeue calls
3. **Position Delta Management** - Extract AddExpectedPositionDeltaLocked operations
4. **Circuit Breaker Reset** - Extract TryResetCircuitBreakerIfBelow logic
5. **Sync State Management** - Extract ClearDispatchSyncPending operations

**Rationale**:
- Zero blast radius (0 importers, 0 direct dependents)
- 4 internal callers will NOT require modification
- High callee count (25) indicates God-method pattern
- Clear single responsibility: drain queues on abort

#### Success Criteria
- Main method reduced to CYC ≤5 (orchestration only)
- All extracted methods have CYC ≤8
- Zero blast radius maintained
- All 4 callers (PumpFleetDispatch, ProcessFleetSlot, VerifyPhotonSlotIntegrity, ProcessValidPhotonSlot) continue working without modification

### OUT OF SCOPE

#### Caller Methods (DO NOT MODIFY)
1. **PumpFleetDispatch** (src/V12_002.SIMA.Fleet.cs:233)
2. **ProcessFleetSlot** (src/V12_002.SIMA.Fleet.cs:44)
3. **VerifyPhotonSlotIntegrity** (src/V12_002.SIMA.Fleet.cs:329)
4. **ProcessValidPhotonSlot** (src/V12_002.SIMA.Fleet.cs:395)

**Rationale**: These are consumers of DrainAllDispatchQueuesOnAbort. Since the method signature will remain unchanged, no caller modifications needed.

#### Subsystem Dependencies (DO NOT MODIFY)
- **_photonDispatchRing** - Dispatch queue infrastructure
- **_photonPool** - Object pooling infrastructure
- **_pendingFleetDispatches** - Fleet dispatch tracking
- **LogBuffer** - Logging infrastructure
- **Telemetry subsystem** - Metrics tracking
- **SIMA subsystem** - Position tracking
- **REAPER subsystem** - Grace period management

**Rationale**: These are stable dependencies. Extraction will use them as-is without modifying their implementations.

#### Other Fleet Methods (DO NOT MODIFY)
- Any method in V12_002.SIMA.Fleet.cs NOT listed in "IN SCOPE"
- Methods with CYC ≤8 already compliant

**Rationale**: This epic targets ONLY DrainAllDispatchQueuesOnAbort. Other methods are separate refactoring candidates.

## Extraction Strategy

### Phase 2 Architecture Plan Will Define:
1. Exact method signatures for extracted helpers
2. Parameter passing strategy (avoid excessive parameters)
3. Error handling delegation
4. Logging strategy for extracted methods
5. Test coverage requirements

### Constraints
- **ASCII-Only**: No Unicode in extracted code
- **Lock-Free**: No lock() statements (use FSM/Actor pattern)
- **Jane Street Alignment**: Query KB for "queue draining patterns" and "abort handling"
- **Single Responsibility**: Each extracted method does ONE thing

## Risk Mitigation

### Zero Blast Radius Advantage
- No external importers to break
- No cross-file dependencies to update
- Changes isolated to V12_002.SIMA.Fleet.cs

### Caller Stability
- Method signature unchanged: void DrainAllDispatchQueuesOnAbort()
- All 4 callers use simple invocation (no complex parameter passing)
- No return value dependencies

### Testing Strategy
- Unit tests for each extracted method
- Integration test: verify all 4 callers still work
- Stress test: verify queue draining under load

## Scope Validation

**Scope Creep Prevention**:
- ✅ ONLY DrainAllDispatchQueuesOnAbort in scope
- ✅ NO caller modifications
- ✅ NO subsystem infrastructure changes
- ✅ NO other Fleet method modifications

**Boundary Enforcement**:
- If Phase 2 discovers additional complexity in callers → STOP and escalate
- If subsystem dependencies require changes → STOP and escalate
- If extraction requires modifying other Fleet methods → STOP and escalate

## Approval Status
- **Scope Defined**: ✅ YES
- **Boundary Clear**: ✅ YES
- **Risk Assessed**: ✅ MEDIUM-HIGH (manageable with zero blast radius)
- **Ready for Phase 2**: ✅ YES
