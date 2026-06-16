# Extraction Tickets: EPIC-CCN-063

## Overview
- **Epic ID**: EPIC-CCN-063
- **Target Method**: DrainAllDispatchQueuesOnAbort
- **File**: src/V12_002.SIMA.Fleet.cs
- **Current Complexity**: 11 (CYC)
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **Total Tickets**: 2
- **Execution Order**: Sequential (TICKET-1 → TICKET-2)
- **Estimated Effort**: 2 hours (1 hour per ticket)

## TICKET-1: Extract DrainPhotonRingSlot Helper

### Scope
- **Current Method**: `DrainAllDispatchQueuesOnAbort`
- **Current CYC**: 11
- **Target CYC**: 6-7 (after first extraction)
- **Extraction**: Photon ring slot cleanup logic

### Implementation
1. Create new private method `DrainPhotonRingSlot(FleetDispatchSlot slot)`
2. Move Photon ring slot cleanup logic from while loop body:
   - Extract sideband index and expected key from slot
   - Perform delta rollback if ReservedDelta != 0
   - Clear dispatch sync pending state
   - Release pool slot by index
   - Clear sideband entry
   - Decrement pending dispatch counter
3. Replace loop body with single method call: `DrainPhotonRingSlot(abortSlot)`
4. Add XML documentation to helper method
5. Verify compilation with `dotnet build`
6. Run unit tests with `dotnet test`

### Method Signature
```csharp
/// <summary>
/// Drains a single Photon ring dispatch slot during abort cleanup.
/// Handles delta rollback, sync clearing, pool release, and sideband cleanup.
/// </summary>
/// <param name="slot">The dequeued Photon ring slot containing dispatch metadata</param>
private void DrainPhotonRingSlot(FleetDispatchSlot slot)
```

### Acceptance Criteria
- [ ] Helper method created with correct signature
- [ ] Photon ring cleanup logic moved to helper
- [ ] Main method calls helper in while loop
- [ ] Method complexity reduced (main method CYC 6-7)
- [ ] Helper method CYC ≤5
- [ ] All tests pass (100% pass rate)
- [ ] No behavioral changes (exact same operations)
- [ ] Build succeeds (zero compilation errors)
- [ ] No lock() statements introduced
- [ ] XML documentation added

### Dependencies
- None (first ticket)

### Verification Commands
```powershell
# Complexity check
python scripts/complexity_audit.py

# Build verification
dotnet build

# Test verification
dotnet test

# Pre-push validation (fast mode)
powershell -File .\scripts\pre_push_validation.ps1 -Fast
```

---

## TICKET-2: Extract DrainLegacyQueueRequest Helper

### Scope
- **Current Method**: `DrainAllDispatchQueuesOnAbort`
- **Current CYC**: 6-7 (after TICKET-1)
- **Target CYC**: 3-4 (final target)
- **Extraction**: Legacy queue request cleanup logic

### Implementation
1. Create new private method `DrainLegacyQueueRequest(FleetDispatchRequest request)`
2. Move legacy queue cleanup logic from while loop body:
   - Perform delta rollback if ReservedDelta != 0
   - Clear dispatch sync pending state
   - Decrement pending dispatch counter
3. Replace loop body with single method call: `DrainLegacyQueueRequest(stale)`
4. Add XML documentation to helper method
5. Update main method XML comment to reference both helpers
6. Verify compilation with `dotnet build`
7. Run unit tests with `dotnet test`

### Method Signature
```csharp
/// <summary>
/// Drains a single legacy queue dispatch request during abort cleanup.
/// Handles delta rollback and sync clearing.
/// </summary>
/// <param name="request">The dequeued legacy queue request</param>
private void DrainLegacyQueueRequest(FleetDispatchRequest request)
```

### Acceptance Criteria
- [ ] Helper method created with correct signature
- [ ] Legacy queue cleanup logic moved to helper
- [ ] Main method calls helper in while loop
- [ ] Method complexity reduced to ≤8 (target 3-4)
- [ ] Helper method CYC ≤3
- [ ] All tests pass (100% pass rate)
- [ ] No behavioral changes (exact same operations)
- [ ] Build succeeds (zero compilation errors)
- [ ] No lock() statements introduced
- [ ] XML documentation added
- [ ] Main method documentation updated

### Dependencies
- **TICKET-1 must be completed first**
- Requires DrainPhotonRingSlot extraction to be merged

### Verification Commands
```powershell
# Complexity check (verify all methods ≤8)
python scripts/complexity_audit.py

# Build verification
dotnet build

# Test verification
dotnet test

# Full pre-push validation
powershell -File .\scripts\pre_push_validation.ps1

# Hard-link sync
powershell -File .\deploy-sync.ps1
```

---

## Final Verification Checklist

After completing both tickets:

- [ ] DrainAllDispatchQueuesOnAbort CYC ≤8 (target 3-4)
- [ ] DrainPhotonRingSlot CYC ≤5
- [ ] DrainLegacyQueueRequest CYC ≤3
- [ ] Zero compilation errors
- [ ] 100% test pass rate
- [ ] Zero lock() statements in any method
- [ ] Hard-link sync successful (deploy-sync.ps1)
- [ ] Pre-push validation passes (all 13 checks)
- [ ] No whitespace mutation in diff
- [ ] Diff size <10k characters
- [ ] F5 test in NinjaTrader successful

## Risk Assessment

### Low Risk Factors ✅
- **No Behavioral Changes**: Pure code organization refactoring
- **Preserved Semantics**: Exact same operations in same order
- **No New Dependencies**: Uses existing methods and fields
- **Single File Change**: Isolated to V12_002.SIMA.Fleet.cs
- **Below Threshold**: CYC 11 is manageable (not God-function territory)

### Mitigation Strategies
1. **Sequential Execution**: Complete TICKET-1 before TICKET-2
2. **Compilation Verification**: Build after each extraction
3. **Test Coverage**: Run full test suite after each ticket
4. **Checkpointing**: Commit after each successful extraction
5. **Rollback Plan**: Git revert if tests fail

## Notes

- **V12.23 Protocol**: Single-Method Extraction (no scope creep)
- **Jane Street Alignment**: Cognitive simplicity, testability, lock-free
- **Lock-Free Validation**: No new synchronization introduced
- **Atomic Operations**: Interlocked.Decrement preserved in helpers
- **Thread Safety**: Maintained through existing patterns

---

**Ticket Generation Complete**
**Date**: 2026-06-15
**Protocol**: V12.23 (Phase 4: Ticket Generation)
