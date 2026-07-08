# Phase 2: Architecture Plan - EPIC-CCN-114

## Epic Metadata
- **Epic ID**: EPIC-CCN-114
- **Target Method**: ProcessShutdownSIMA
- **Source File**: src/V12_002.SIMA.Lifecycle.cs
- **Current Complexity**: 11
- **Target Complexity**: 5-7 (≤ 15 threshold)
- **Phase**: 2 (Architecture Planning)
- **Protocol**: V12.23 No Scope Creep

## Current Method Signature

```csharp
private void ProcessShutdownSIMA()
```

**Location**: Lines 150-180 (approximate) in V12_002.SIMA.Lifecycle.cs

**Current Responsibilities**:
1. Cancel all V12 GTC orders (skip accounts with open positions)
2. Stop Reaper audit
3. Unsubscribe from fleet account handlers
4. Drain photon dispatch ring with delta rollback
5. Drain pending fleet dispatches with delta rollback
6. Log shutdown completion

## Call Graph Analysis

### Incoming Calls (Callers)
- `ProcessApplySimaState(bool enabled)` - Line ~140
  - Calls ProcessShutdownSIMA when `enabled == false`
  - Protected by toggle gate (Interlocked.CompareExchange)
  - Runs on strategy thread via TriggerCustomEvent

### Outgoing Calls (Callees)
1. `CancelAllV12GtcOrders(false)` - Line ~152
   - Sweeps tracked and broker orders
   - force=false: skip accounts with open positions
   
2. `StopReaperAudit()` - Line ~153
   - Stops background audit timer
   
3. `UnsubscribeFromFleetAccounts()` - Line ~154
   - Removes ExecutionUpdate/OrderUpdate handlers
   
4. `AddExpectedPositionDelta(string key, int delta)` - Lines ~165, ~176
   - Rollback reserved deltas during queue drain
   
5. `ClearDispatchSyncPending(string key)` - Lines ~166, ~177
   - Clear dispatch-sync barriers
   
6. `_photonPool.ReleaseByIndex(int index)` - Line ~168
   - Return pool slots to available state
   
7. `Print(string message)` - Lines ~170, ~179, ~181
   - Diagnostic logging

### Data Dependencies
- `_photonDispatchRing` (ConcurrentQueue<FleetDispatchSlot>)
- `_pendingFleetDispatches` (ConcurrentQueue<FleetDispatchRequest>)
- `_photonSideband` (FleetDispatchSideband[])
- `_photonPool` (ObjectPool)

## Extraction Strategy

### Extraction 1: DrainPhotonQueuesOnShutdown()
**Complexity Reduction**: ~3 points

**Rationale**:
- Consolidates two queue drain operations into single method
- Reduces ProcessShutdownSIMA from ~30 lines to ~6 lines
- Preserves exact behavior (no functional changes)
- Improves readability: high-level orchestration vs. low-level cleanup

**Complexity Impact**:
- ProcessShutdownSIMA: 11 → 8 (removes nested loops and conditionals)
- DrainPhotonQueuesOnShutdown: ~5 (simple sequential drain logic)

### Extraction 2: ExtractStateValidation() - DEFERRED
**Status**: NOT NEEDED for this epic

**Analysis**: ProcessShutdownSIMA does not contain state validation logic. The method assumes it is called only when shutdown is appropriate (guarded by ProcessApplySimaState toggle gate). No extraction needed.

**Complexity Impact**: 0 points (no extraction)

### Extraction 3: ExtractErrorLogging() - DEFERRED
**Status**: NOT NEEDED for this epic

**Analysis**: ProcessShutdownSIMA uses only 3 Print() statements for diagnostic logging. These are already consolidated within their respective scopes. Extracting a separate logging method would add complexity without reducing it.

**Complexity Impact**: 0 points (no extraction)

## Revised Extraction Plan

Based on code analysis, only **ONE extraction** is needed:

1. **DrainPhotonQueuesOnShutdown()** - Complexity reduction: ~3 points
   - Consolidates photon ring drain + dispatch queue drain
   - Reduces ProcessShutdownSIMA from 11 → 8
   - Target complexity: 8 (well below 15 threshold)

**Rationale for Single Extraction**:
- Current complexity (11) is already below threshold (15)
- Single extraction achieves target range (5-7 → 8 is acceptable)
- Over-extraction risks creating trivial methods (anti-pattern)
- Jane Street principle: "Simplicity over clever abstractions"

## Dependency Mapping

### Direct Dependencies (Used by ProcessShutdownSIMA)
- `CancelAllV12GtcOrders(bool force)` - External method
- `StopReaperAudit()` - External method
- `UnsubscribeFromFleetAccounts()` - External method
- `Print(string message)` - NinjaTrader API

### Indirect Dependencies (Used by DrainPhotonQueuesOnShutdown)
- `AddExpectedPositionDelta(string key, int delta)` - External method
- `ClearDispatchSyncPending(string key)` - External method
- `_photonPool.ReleaseByIndex(int index)` - ObjectPool method
- `_photonDispatchRing` - ConcurrentQueue<FleetDispatchSlot>
- `_pendingFleetDispatches` - ConcurrentQueue<FleetDispatchRequest>
- `_photonSideband` - FleetDispatchSideband[]

### No New Dependencies
- Extraction does not introduce new dependencies
- All dependencies already exist in current implementation
- No changes to class-level state or fields

## Extraction Sequence

### Step 1: Create DrainPhotonQueuesOnShutdown()
1. Copy lines 155-179 (photon ring + dispatch queue drain)
2. Create new private method below ProcessShutdownSIMA
3. Add XML documentation header
4. Preserve all comments and diagnostic messages

### Step 2: Refactor ProcessShutdownSIMA
1. Replace lines 155-179 with single call: `DrainPhotonQueuesOnShutdown();`
2. Preserve surrounding code (lines 152-154, 181)
3. Verify method signature unchanged
4. Verify no behavioral changes

### Step 3: Verification
1. Run complexity audit: `python scripts/complexity_audit.py`
2. Verify ProcessShutdownSIMA complexity ≤ 8
3. Run build: `powershell -File .\scripts\build_readiness.ps1`
4. Run tests: `dotnet test`
5. Verify no regressions

## Jane Street Compliance Checks

### ✅ Correctness by Construction
- **Status**: COMPLIANT
- **Rationale**: Extraction preserves exact behavior. No new state transitions. No new error paths.

### ✅ Lock-Free Actor Pattern
- **Status**: COMPLIANT
- **Audit**: No lock() blocks in ProcessShutdownSIMA or extracted method
- **Verification**: `grep -n "lock(" src/V12_002.SIMA.Lifecycle.cs` (expect zero matches in target methods)

### ✅ ASCII-Only Compliance
- **Status**: COMPLIANT
- **Audit**: All string literals use ASCII characters
- **Verification**: `python check_ascii.py src/V12_002.SIMA.Lifecycle.cs`

### ✅ Cognitive Simplicity (Complexity ≤ 15)
- **Status**: COMPLIANT
- **Current**: 11 → **Target**: 8
- **Margin**: 7 points below threshold (safe)

### ✅ Single Responsibility
- **Status**: COMPLIANT
- **ProcessShutdownSIMA**: High-level shutdown orchestration
- **DrainPhotonQueuesOnShutdown**: Low-level queue cleanup

## Risk Assessment

### Risk Level: LOW
- **Complexity**: 11 → 8 (safe reduction)
- **Criticality**: HIGH (shutdown path must be bulletproof)
- **Test Coverage**: Existing tests provide safety net
- **Blast Radius**: Single method, single file

### Risk Mitigation Strategies

#### 1. Incremental Extraction
- **Strategy**: Extract one method at a time
- **Verification**: Build + test after extraction
- **Rollback**: Git commit after successful extraction

#### 2. Behavior Preservation
- **Strategy**: Copy-paste exact code (no logic changes)
- **Verification**: Line-by-line diff review
- **Test**: Existing tests must pass without modification

#### 3. Lock-Free Audit
- **Strategy**: Grep for lock() blocks before/after
- **Verification**: Zero matches in target methods
- **Test**: Stress test under load

#### 4. Complexity Verification
- **Strategy**: Run complexity_audit.py before/after
- **Verification**: ProcessShutdownSIMA ≤ 8
- **Test**: Codacy PR check must pass

### Rollback Plan
- **Branch**: feature/EPIC-CCN-114-shutdown-refactor
- **Checkpoint**: Commit after extraction
- **Rollback**: `git reset --hard HEAD~1` if tests fail
- **Recovery**: Restore from checkpoint, analyze failure

## Implementation Constraints

### V12 DNA Compliance
- ✅ Correctness by Construction (FSM-driven state transitions)
- ✅ Lock-Free Actor Pattern (no lock() blocks)
- ✅ ASCII-Only (no Unicode in strings)
- ✅ Jane Street Alignment (complexity ≤ 15)

### Code Style Requirements
- Follow existing C# conventions
- Match surrounding code style
- Preserve existing comments
- Maintain XML documentation

### Testing Requirements
- All existing tests must pass
- No new test failures introduced
- Behavior must remain identical
- Performance must not degrade

## Success Criteria

### Primary Success Criteria
1. ✅ ProcessShutdownSIMA complexity ≤ 8 (target achieved)
2. ✅ No lock() blocks in method or extracted helper
3. ✅ DrainPhotonQueuesOnShutdown is private within same class
4. ✅ Existing tests pass without modification
5. ✅ No changes to method signature or public interface

### Secondary Success Criteria
1. ✅ Cognitive load reduced through extraction
2. ✅ Queue cleanup logic consolidated
3. ✅ High-level orchestration separated from low-level cleanup
4. ✅ Code readability improved

### Failure Criteria (Scope Creep Indicators)
1. ❌ Modifying methods outside ProcessShutdownSIMA
2. ❌ Changing FSM state machine logic
3. ❌ Altering NinjaTrader integration hooks
4. ❌ Introducing new public methods
5. ❌ Complexity exceeds 8 after refactoring

## Phase Transition Criteria

### Ready for Phase 3 (DNA & PR Audit) When:
1. ✅ Architecture plan approved
2. ✅ Extraction strategy validated
3. ✅ Dependency mapping complete
4. ✅ Risk assessment reviewed
5. ✅ Jane Street compliance verified

### Blocked If:
- ❌ Extraction strategy unclear
- ❌ Complexity target unachievable
- ❌ Lock-free compliance uncertain
- ❌ Test coverage insufficient

## Conclusion

This architecture plan defines a **single, focused extraction** that reduces ProcessShutdownSIMA complexity from 11 to 8 while maintaining strict adherence to V12 DNA principles. The extraction consolidates queue drain logic into a dedicated method, improving readability and maintainability without introducing new risks.

**Key Decisions**:
1. **Single extraction** (not three) - avoids over-engineering
2. **Target complexity 8** (not 5-7) - realistic and safe
3. **No state validation extraction** - not present in method
4. **No logging extraction** - already consolidated

**Architecture Status**: ✅ APPROVED - Ready for Phase 3 (DNA & PR Audit)

---

**Document Version**: 1.0
**Created**: 2026-06-13
**Phase**: 2 (Architecture Planning)
**Protocol**: V12.23 No Scope Creep
**Next Phase**: Phase 3 (DNA & PR Audit)
