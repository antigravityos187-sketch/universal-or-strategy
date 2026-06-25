# Phase 1: Scope Definition - EPIC-W7-093

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Mode**: plan
- **Execution Time**: 2026-06-24T19:36:21Z
- **Input**: 00-hotspots.md
- **Output**: 00-scope.md

## Epic Overview
- **Epic ID**: EPIC-W7-093
- **Target Method**: Dispatch_ProcessFleetLoop
- **File**: src/V12_002.SIMA.Dispatch.cs
- **Line**: 196
- **Current CYC**: 20
- **Target CYC**: ≤ 8
- **Lines of Code**: 153
- **Parameter Count**: 12

## Scope Boundary

### IN SCOPE: Extraction Targets

This epic will extract **5 helper methods** from Dispatch_ProcessFleetLoop to reduce complexity from CYC=20 to ≤8:

#### 1. Fleet Validation Logic (CYC ~3)
**Extract**: `ShouldProcessFleet()`
- **Calls**: ShouldSkipFleetAccount, ShouldSkipFleet_RunHealthCheck, ShouldSkipFleet_IsConsistencyLockHit
- **Purpose**: Consolidate all fleet skip checks into single validation method
- **Return**: bool (true = process fleet, false = skip)
- **Parameters**: Fleet account, health check state, consistency lock state

#### 2. Photon Pool Management (CYC ~4)
**Extract**: `ClaimAndPopulatePhotonSlot()`
- **Calls**: ClaimPhotonPoolSlot, PopulatePhotonSlot, EnqueueToPhotonRing, EnqueueLimitEntryToPhotonRing
- **Purpose**: Handle photon pool slot lifecycle (claim → populate → enqueue)
- **Return**: bool (true = success, false = pool exhausted)
- **Parameters**: Fleet data, dispatch shadow, order details

#### 3. Order Building Orchestration (CYC ~5)
**Extract**: `BuildAndPublishFleetOrders()`
- **Calls**: Dispatch_BuildFollowerOrders, Dispatch_PublishMarketBracketToPhoton, Dispatch_PublishLimitEntryToPhoton
- **Purpose**: Orchestrate order building and publishing flow
- **Return**: bool (true = orders published, false = build failed)
- **Parameters**: Fleet data, photon slot, dispatch shadow

#### 4. Calculation Pipeline (CYC ~4)
**Extract**: `CalculateFleetPricing()`
- **Calls**: CalculateATRStopDistance, CalculateTargetPrice, GetTargetDistribution, ValidateStopPrice
- **Purpose**: Consolidate all pricing calculations into single pipeline
- **Return**: PricingResult struct (stop distance, target prices, validation status)
- **Parameters**: Fleet data, ATR state, target distribution config

#### 5. State Management (CYC ~3)
**Extract**: `UpdateFleetDispatchState()`
- **Calls**: ClearDispatchSyncPending, AddExpectedPositionDeltaLocked, MarkDispatchSyncPending
- **Purpose**: Handle dispatch state transitions and position tracking
- **Return**: void
- **Parameters**: Fleet data, dispatch result, position delta

### OUT OF SCOPE: What Remains

The **main loop orchestration** will remain in Dispatch_ProcessFleetLoop with CYC ≤ 8:
- Loop iteration over fleet accounts
- High-level orchestration of extracted methods
- Error handling and logging
- Circuit breaker logic (TryIncrementDispatchCountWithCircuitBreaker)
- Symmetry guards (SymmetryGuardRegisterFollower)
- Telemetry (LogDispatchCompletion, TrackPhotonPoolExhausted)

### OUT OF SCOPE: Deferred Work

The following will **NOT** be addressed in this epic:
- Reducing parameter count (12 params) - deferred to future epic
- Refactoring downstream callees (88 methods) - out of scope
- Changing method signature or return type
- Modifying caller (ExecuteSmartDispatchEntry)
- Extracting data structure access patterns

## Dependencies

### Internal Dependencies
- **Caller**: ExecuteSmartDispatchEntry (single entry point)
- **Callees**: 88 downstream methods (will be called by extracted helpers)
- **Data Structures**: activePositions, entryOrders, stopOrders, _followerBrackets, _photonPool, _photonDispatchRing

### External Dependencies
- **None** - Method is private with zero external blast radius

### Risk Factors
1. **Deep Nesting (5 levels)**: Extraction must preserve control flow logic
2. **State Mutations**: Multiple state updates must maintain order
3. **Photon Pool Exhaustion**: Circuit breaker logic must remain intact
4. **Symmetry Guards**: ExpKey/SymmetryTrim calls must not be disrupted
5. **Logging Context**: LogBuffer.Format calls must preserve context

## Success Criteria

### Quantitative Metrics
- ✅ Reduce CYC from 20 to ≤ 8 (main method)
- ✅ Extract exactly 5 helper methods
- ✅ Each extracted method has CYC ≤ 5
- ✅ Zero change to method signature (12 params remain)
- ✅ Zero change to external behavior (100% equivalence)

### Qualitative Metrics
- ✅ Main loop reads as high-level orchestration
- ✅ Each extracted method has single responsibility
- ✅ Control flow logic preserved exactly
- ✅ State mutation order preserved exactly
- ✅ Error handling preserved exactly

### Verification Criteria
- ✅ `dotnet build` passes (zero errors)
- ✅ `powershell -File .\deploy-sync.ps1` succeeds
- ✅ F5 in NinjaTrader IDE loads strategy
- ✅ BUILD_TAG appears in output
- ✅ Unit tests pass for extracted methods
- ✅ `python scripts/complexity_audit.py` shows CYC ≤ 8

## Boundary Validation

### Scope Creep Prevention
This epic will **NOT**:
- ❌ Refactor other methods in V12_002.SIMA.Dispatch.cs
- ❌ Change method signatures or return types
- ❌ Modify data structures or field declarations
- ❌ Refactor downstream callees (88 methods)
- ❌ Add new features or change behavior
- ❌ Fix unrelated bugs or compilation errors

### Scope Adherence
This epic will **ONLY**:
- ✅ Extract 5 helper methods from Dispatch_ProcessFleetLoop
- ✅ Reduce main method CYC from 20 to ≤ 8
- ✅ Preserve 100% behavioral equivalence
- ✅ Add unit tests for extracted methods
- ✅ Update complexity audit baseline

## Risk Assessment

### Overall Risk: **LOW**
- Zero external blast radius (private method)
- Single caller (easy to test)
- No signature changes (no coordination needed)
- Clear extraction boundaries (5 distinct responsibilities)

### Mitigation Strategies
1. **Preserve Control Flow**: Use Sequential Thinking MCP to validate logic equivalence
2. **Preserve State Order**: Document state mutation sequence before extraction
3. **Preserve Error Handling**: Copy error handling blocks to extracted methods
4. **Verify Behavior**: F5 test after each extraction
5. **Unit Test Coverage**: Add tests for each extracted method

## Next Steps (Phase 2)
1. Architecture planning: Design extracted method signatures
2. Define PricingResult struct for CalculateFleetPricing
3. Map control flow dependencies between extractions
4. Plan extraction order (least coupled → most coupled)
5. Generate Phase 3 DNA audit checklist
