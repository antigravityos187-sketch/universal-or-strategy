# Phase 2: Architecture Planning - EPIC-CCN-063

## Epic Metadata
- **Epic ID**: EPIC-CCN-063
- **Target Method**: DrainAllDispatchQueuesOnAbort
- **File**: src/V12_002.SIMA.Fleet.cs
- **Current Complexity**: 11 (CYC)
- **Current LOC**: 23
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **Tier**: 2

## 1. Extraction Strategy

### Current Method Analysis
**Method**: DrainAllDispatchQueuesOnAbort()
- **Purpose**: Cleanup and drain dispatch queues during abort operations
- **Current Structure**: Two sequential while loops with nested conditional logic
- **Complexity Sources**:
  - Photon ring draining loop (CYC +5): sideband handling, conditional delta rollback, conditional sync clearing, conditional pool release
  - Legacy queue draining loop (CYC +3): conditional delta rollback, sync clearing
  - Nested conditionals within loops (CYC +3)

### Extraction Approach
**Pattern**: Single-Responsibility Helper Methods

Extract two private helper methods to isolate cleanup logic:
1. **DrainPhotonRingSlot** - Handles single Photon ring slot cleanup
2. **DrainLegacyQueueRequest** - Handles single legacy queue request cleanup

**Rationale**:
- Reduces cognitive load by separating Photon ring vs legacy queue concerns
- Each helper method has single, clear responsibility
- Enables independent unit testing of cleanup logic
- Maintains exact semantics (no behavioral changes)

### Complexity Reduction
- **Original Method**: CYC 11 → **Target**: CYC 3-4
  - Main method: 2 while loops = CYC 3
- **Helper 1 (DrainPhotonRingSlot)**: CYC 4-5
  - Sideband index validation (CYC +1)
  - Delta rollback conditional (CYC +1)
  - Sync clearing conditional (CYC +1)
  - Pool release conditional (CYC +1)
- **Helper 2 (DrainLegacyQueueRequest)**: CYC 2-3
  - Delta rollback conditional (CYC +1)
  - Sync clearing (CYC +1)

**Total Complexity**: All methods ≤8 ✅

## 2. Method Signatures

### Original Method (Preserved)
private void DrainAllDispatchQueuesOnAbort()

### Proposed Helper Method 1
private void DrainPhotonRingSlot(FleetDispatchSlot slot)

**Parameters**:
- slot (FleetDispatchSlot): The dequeued Photon ring slot containing dispatch metadata

**Responsibilities**:
1. Extract sideband index and expected key from slot
2. Perform delta rollback if ReservedDelta != 0
3. Clear dispatch sync pending state
4. Release pool slot by index
5. Clear sideband entry
6. Decrement pending dispatch counter

**Access Modifier**: private (internal implementation detail)

### Proposed Helper Method 2
private void DrainLegacyQueueRequest(FleetDispatchRequest request)

**Parameters**:
- request (FleetDispatchRequest): The dequeued legacy queue request

**Responsibilities**:
1. Perform delta rollback if ReservedDelta != 0
2. Clear dispatch sync pending state
3. Decrement pending dispatch counter

**Access Modifier**: private (internal implementation detail)

## 3. Call Graph

### Method Invocation Flow
DrainAllDispatchQueuesOnAbort()
├─> while (_photonDispatchRing.TryDequeue(out slot))
│   └─> DrainPhotonRingSlot(slot)
│       ├─> AddExpectedPositionDeltaLocked() [existing]
│       ├─> ClearDispatchSyncPending() [existing]
│       ├─> _photonPool.ReleaseByIndex() [existing]
│       └─> Interlocked.Decrement() [atomic]
│
└─> while (_pendingFleetDispatches.TryDequeue(out request))
    └─> DrainLegacyQueueRequest(request)
        ├─> AddExpectedPositionDeltaLocked() [existing]
        ├─> ClearDispatchSyncPending() [existing]
        └─> Interlocked.Decrement() [atomic]

### Data Flow
Input: None (operates on instance state)
│
├─> Photon Ring Path:
│   ├─> Read: _photonDispatchRing (ConcurrentQueue)
│   ├─> Read: _photonSideband (array)
│   ├─> Write: _photonSideband[index] = default
│   ├─> Write: _photonPool.ReleaseByIndex()
│   └─> Write: _pendingFleetDispatchCount (atomic decrement)
│
└─> Legacy Queue Path:
    ├─> Read: _pendingFleetDispatches (ConcurrentQueue)
    └─> Write: _pendingFleetDispatchCount (atomic decrement)

Shared State:
├─> AddExpectedPositionDeltaLocked() [modifies position delta tracking]
└─> ClearDispatchSyncPending() [modifies sync state]

### Shared State Analysis
**No New Shared State**: Helper methods operate on parameters and call existing instance methods. No new fields or shared mutable state introduced.

**Thread Safety**: Preserved through:
- Atomic operations (Interlocked.Decrement)
- Lock-free queue operations (TryDequeue)
- Existing synchronization in called methods (AddExpectedPositionDeltaLocked, ClearDispatchSyncPending)

## 4. Lock-Free Validation

### Current Method Analysis
✅ **No lock() statements** - Method uses lock-free patterns throughout
✅ **Atomic primitives** - Uses Interlocked.Decrement for counter updates
✅ **Lock-free queues** - Uses TryDequeue operations (ConcurrentQueue)
⚠️ **Locked method call** - Calls AddExpectedPositionDeltaLocked (name suggests internal locking)

### Post-Extraction Validation
✅ **Preserved lock-free pattern** - Helper methods maintain same call patterns
✅ **No new locks introduced** - Extraction is purely organizational
✅ **Atomic operations maintained** - Interlocked.Decrement preserved in helpers
✅ **FSM/Actor pattern** - Cleanup operations follow Actor model (process dequeued items)

### Thread Safety Guarantees
1. **Queue Operations**: TryDequeue is lock-free and thread-safe
2. **Counter Updates**: Interlocked.Decrement provides atomic decrement
3. **Pool Release**: _photonPool.ReleaseByIndex() assumed lock-free (pool pattern)
4. **Sideband Clearing**: Array write is atomic for reference types

**Note**: AddExpectedPositionDeltaLocked method name suggests internal locking, but this is existing behavior preserved by extraction. No new synchronization introduced.

## 5. Jane Street Compliance

### Cognitive Simplicity ✅
**Principle**: Make illegal states unrepresentable

**Application**:
- **Before**: 23-line method with nested conditionals (CYC 11)
- **After**: 3 focused methods, each <10 lines (CYC ≤5 each)
- **Benefit**: Each method has single, verifiable responsibility

**Cognitive Load Reduction**:
- Photon ring cleanup isolated from legacy queue cleanup
- Sideband handling encapsulated in dedicated method
- Delta rollback logic clearly separated by queue type

### Testability ✅
**Principle**: Independent verification of components

**Test Strategy**:
1. **Unit Test DrainPhotonRingSlot**:
   - Mock FleetDispatchSlot with various sideband indices
   - Verify delta rollback called with correct parameters
   - Verify pool release called for valid indices
   - Verify counter decrement

2. **Unit Test DrainLegacyQueueRequest**:
   - Mock FleetDispatchRequest with various delta values
   - Verify delta rollback called when ReservedDelta != 0
   - Verify sync clearing called
   - Verify counter decrement

3. **Integration Test DrainAllDispatchQueuesOnAbort**:
   - Populate both queues with test data
   - Verify all items drained
   - Verify correct cleanup order (Photon ring first, then legacy)

### Maintainability ✅
**Principle**: Code should be easy to reason about under microsecond latency constraints

**Improvements**:
- **Clear Separation**: Photon ring vs legacy queue concerns isolated
- **Single Responsibility**: Each helper does one thing well
- **Reduced Nesting**: Flattened conditional logic within helpers
- **Explicit Intent**: Method names clearly describe purpose

### Performance Considerations ✅
**Principle**: Zero-overhead abstractions

**Analysis**:
- **No Additional Allocations**: Helper methods operate on passed parameters
- **Inline Candidate**: Small helper methods likely inlined by JIT
- **Same Call Pattern**: Existing method calls preserved (no new indirection)
- **Lock-Free Maintained**: No new synchronization overhead

**Microsecond Latency Impact**: Negligible (organizational refactoring only)

### Jane Street Knowledge Base Insights

**Query Results**: No specific FSM extraction patterns found in KB, but general principles apply from available documents on concurrency coordination, microsecond latency, and testing.

**Application to EPIC-CCN-063**:
- ✅ Maintains lock-free queue operations
- ✅ Preserves atomic counter updates
- ✅ Reduces cognitive complexity for hot-path cleanup
- ✅ Enables independent unit testing of cleanup logic

## 6. Implementation Plan

### Step 1: Extract DrainPhotonRingSlot
1. Create new private method with signature above
2. Move Photon ring slot cleanup logic from while loop body
3. Replace loop body with single method call: DrainPhotonRingSlot(abortSlot)
4. Verify compilation
5. Run unit tests

### Step 2: Extract DrainLegacyQueueRequest
1. Create new private method with signature above
2. Move legacy queue cleanup logic from while loop body
3. Replace loop body with single method call: DrainLegacyQueueRequest(stale)
4. Verify compilation
5. Run unit tests

### Step 3: Update Documentation
1. Add XML comments to helper methods
2. Update DrainAllDispatchQueuesOnAbort XML comment to reference helpers
3. Document extraction rationale in code comments

### Step 4: Verification
1. Run complexity audit
2. Verify CYC ≤8 for all methods
3. Run pre-push validation
4. Verify zero compilation errors
5. Verify 100% test pass rate

### Step 5: Hard-Link Sync
1. Run deploy-sync.ps1
2. Verify NinjaTrader hard links updated
3. Test F5 in NinjaTrader

## 7. Risk Assessment

### Low Risk Factors ✅
- **No Behavioral Changes**: Pure code organization refactoring
- **Preserved Semantics**: Exact same operations in same order
- **No New Dependencies**: Uses existing methods and fields
- **Single File Change**: Isolated to V12_002.SIMA.Fleet.cs
- **Below Threshold**: CYC 11 is manageable (not God-function territory)

### Mitigation Strategies
1. **Compilation Verification**: Build after each extraction step
2. **Test Coverage**: Run full test suite after each step
3. **Checkpointing**: Commit after each successful extraction
4. **Rollback Plan**: Git revert if tests fail

### Success Criteria
- CYC ≤8 for DrainAllDispatchQueuesOnAbort
- CYC ≤5 for each helper method
- Zero compilation errors
- 100% test pass rate
- Zero lock() statements introduced
- Hard-link sync successful
- Pre-push validation passes

## 8. V12.23 Protocol Compliance

### Single-Method Extraction ✅
- **Scope**: DrainAllDispatchQueuesOnAbort method body only
- **No Caller Changes**: Method signature preserved
- **No Callee Changes**: All called methods unchanged
- **No File Pollution**: Only adds 2 private helper methods

### Scope Creep Prevention ✅
- **No While We Are Here**: No unrelated improvements
- **No Bug Fixes**: No pre-existing issues addressed
- **No Feature Additions**: Pure complexity reduction
- **No Bundling**: Single concern (method extraction)

### Surgical Precision ✅
- **Minimal Blast Radius**: 1 file, 1 method, 2 helpers
- **Preserved Semantics**: Exact same behavior
- **Zero Breaking Changes**: No API modifications
- **Isolated Testing**: Can verify independently

## 9. Next Steps

### Phase 3: DNA & PR Audit (Adjudicator)
- Submit plan to Arena AI for adversarial review
- Verify V12 DNA compliance (no locks, atomic, ASCII-only)
- Validate PR health (diff <10k, no whitespace mutation)
- **Gate**: PASS/FAIL decision

### Phase 4: Recursive Execution (Engineer)
- Hand off to Bob CLI (v12-engineer) for implementation
- Execute extraction in 2 steps (DrainPhotonRingSlot, then DrainLegacyQueueRequest)
- Checkpoint after each step
- Verify complexity reduction after each extraction

### Phase 5: Verification/Review (Forensics)
- Compare implementation against this plan
- Run complexity audit
- Verify test coverage
- Validate lock-free compliance

### Phase 6: Sign-off (Director)
- Run deploy-sync.ps1
- Test F5 in NinjaTrader
- Verify BUILD_TAG
- Merge to main

## Approval Signature

**Plan Status**: ✅ READY FOR PHASE 3 AUDIT

**Architect**: Bob CLI (v12-engineer)
**Date**: 2026-06-15
**V12 Protocol**: V12.23 (Single-Method Extraction)
**Jane Street Alignment**: Verified (Cognitive Simplicity, Testability, Lock-Free)

---

**End of Architecture Plan - EPIC-CCN-063**
