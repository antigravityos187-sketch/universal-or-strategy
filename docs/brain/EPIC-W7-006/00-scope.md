# Phase 1: Scope Definition - EPIC-W7-006

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Mode**: v12-phase1-scope
- **Bobcoins Used**: 0.18
- **API Key**: jCodemunch MCP
- **Execution Time**: ~15 seconds

## Epic Metadata
- **Epic ID**: EPIC-W7-006
- **Target Method**: AdoptFleetWorkingOrders
- **File**: src/V12_002.SIMA.Lifecycle.cs
- **Current CYC**: 21
- **Target CYC**: <=8 (Jane Street strict standard)
- **Lines of Code**: 70
- **Max Nesting Depth**: 6

## Scope Boundary Definition

### IN SCOPE

#### Primary Extraction Target
1. **AdoptFleetWorkingOrders** (CYC=21, Line 460)
   - **Rationale**: Exceeds Jane Street threshold by 2.6x
   - **Blast Radius**: VERY LOW (0 external dependencies)
   - **Risk**: MEDIUM (safe to refactor)

#### Extraction Candidates (from 21 callees)
2. **Fleet Account Classification Logic**
   - IsFleetAccount validation (2 variants)
   - Fleet order routing logic
   - **Target CYC**: <=3 per extracted method

3. **Position Synchronization Logic**
   - RebuildActivePositionForFleetEntry
   - SyncExistingPositionMetadata
   - **Target CYC**: <=4 per extracted method

4. **Order Classification & Routing**
   - ClassifyAndRouteFleetOrder orchestration
   - GetTargetDistribution business logic
   - **Target CYC**: <=5 per extracted method

#### Affected Callers (3 methods)
5. **HydrateWorkingOrdersFromBroker** (depth 1)
   - Update call site after extraction
   - Verify signature compatibility

6. **EnumerateApexAccounts** (depth 2)
   - Update call site after extraction
   - Verify signature compatibility

7. **ProcessInitializeSIMA** (depth 3)
   - Update call site after extraction
   - Verify signature compatibility

### OUT OF SCOPE

#### Excluded from This Epic
1. **Caller Method Refactoring**
   - HydrateWorkingOrdersFromBroker (separate epic if needed)
   - EnumerateApexAccounts (separate epic if needed)
   - ProcessInitializeSIMA (separate epic if needed)
   - **Rationale**: One epic = one concern (No Scope Creep Protocol V12.23)

2. **Callee Method Refactoring**
   - IsFleetAccount (already simple)
   - ClassifyAndRouteFleetOrder (separate epic if CYC>8)
   - RebuildActivePositionForFleetEntry (separate epic if CYC>8)
   - SyncExistingPositionMetadata (separate epic if CYC>8)
   - **Rationale**: Only refactor if directly blocking this epic

3. **Logging Infrastructure**
   - LogBuffer.Format calls (utility, not business logic)
   - **Rationale**: Logging is cross-cutting concern

4. **Utility Methods**
   - GetStableHash (utility, not business logic)
   - **Rationale**: Utilities are stable, low-risk

5. **Test File Creation**
   - Unit tests for extracted methods
   - **Rationale**: Separate TDD epic (EPIC-CCN-10 backlog)

## Extraction Strategy

### Phase 2 Architecture Plan Will Define:
1. **Method Decomposition**
   - Extract fleet account validation (CYC <=3)
   - Extract position synchronization (CYC <=4)
   - Extract order classification (CYC <=5)
   - Orchestrator method (CYC <=8)

2. **Naming Convention**
   - Prefix: AdoptFleet_ for extracted methods
   - Example: AdoptFleet_ValidateAccount, AdoptFleet_SyncPosition

3. **Actor/FSM Pattern Compliance**
   - Ensure no lock(stateLock) blocks introduced
   - Use FSM/Actor Enqueue model if state mutations needed

## Success Criteria

### Phase 1 (This Phase)
- Scope boundary clearly defined (IN vs OUT)
- Primary target identified (AdoptFleetWorkingOrders)
- Extraction candidates listed (3 categories)
- Affected callers documented (3 methods)
- Exclusions justified (5 categories)

### Phase 2 (Architecture Planning)
- Detailed extraction plan with method signatures
- Mermaid diagrams showing before/after call hierarchy
- CYC reduction roadmap (21 to <=8)

### Phase 5 (Ticket Execution)
- All extracted methods CYC <=8
- No lock(stateLock) blocks introduced
- ASCII-only compliance maintained
- Build passes after extraction
- F5 in NinjaTrader successful

## Risk Mitigation

### Low Blast Radius Advantage
- **0 external dependencies** = isolated refactoring
- **3 callers only** = minimal call site updates
- **SIMA.Lifecycle module** = single file scope

### Jane Street KB Alignment
- Query KB for "complexity reduction" patterns
- Query KB for "FSM extraction" patterns
- Apply HFT microsecond-latency reasoning principles

## Phase 1 Completion
- Scope definition complete
- IN SCOPE: 1 primary target + 3 extraction categories + 3 callers
- OUT OF SCOPE: 5 exclusion categories justified
- Success criteria defined for Phases 2 and 5
- Risk mitigation strategy documented
