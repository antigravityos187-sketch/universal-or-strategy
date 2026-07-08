# Phase 4: Implementation Tickets - EPIC-CCN-114

## Epic Metadata
- **Epic ID**: EPIC-CCN-114
- **Target Method**: ProcessShutdownSIMA
- **Source File**: src/V12_002.SIMA.Lifecycle.cs
- **Phase**: 4 (Ticket Generation)
- **Generated**: 2026-06-13
- **Protocol**: V12.23 No Scope Creep
- **Audit Status**: ✅ GO (Phase 3 approved)

## Execution Summary

**Total Tickets**: 1
**Estimated Complexity Reduction**: 3 points (11 → 8)
**Risk Level**: LOW
**Execution Order**: Sequential (single ticket)

---

## TICKET-114-001: Extract DrainPhotonQueuesOnShutdown

### Ticket Metadata
- **Ticket ID**: TICKET-114-001
- **Type**: Surgical Extraction
- **Priority**: P1 (Single extraction for this epic)
- **Estimated Time**: 30 minutes
- **Complexity Impact**: -3 points
- **Risk Level**: LOW

### Method Signature

```csharp
/// <summary>
/// Drains photon dispatch queues during SIMA shutdown.
/// Processes both the photon dispatch ring and pending fleet dispatches,
/// rolling back position deltas and clearing dispatch-sync barriers.
/// </summary>
/// <remarks>
/// Called exclusively by ProcessShutdownSIMA during shutdown sequence.
/// Ensures clean queue state before strategy termination.
/// Lock-free: Uses ConcurrentQueue and ObjectPool primitives.
/// </remarks>
private void DrainPhotonQueuesOnShutdown()
```

**Location**: Insert after ProcessShutdownSIMA (approximately line 182)

### Current Code Location

**File**: src/V12_002.SIMA.Lifecycle.cs
**Lines**: 155-179 (to be extracted)
**Parent Method**: ProcessShutdownSIMA (lines 150-181)

### Extraction Steps (Surgical)

#### Step 1: Create New Method
1. Navigate to line 182 (after ProcessShutdownSIMA closing brace)
2. Insert blank line
3. Add XML documentation header (see Method Signature above)
4. Create method signature: `private void DrainPhotonQueuesOnShutdown()`
5. Open method body with `{`

#### Step 2: Copy Queue Drain Logic
1. Copy lines 155-179 from ProcessShutdownSIMA
2. Paste into DrainPhotonQueuesOnShutdown method body
3. Preserve exact indentation and formatting
4. Preserve all comments (e.g., "// Drain photon dispatch ring")
5. Preserve all Print() statements

**Code Block to Extract**:
```csharp
// Drain photon dispatch ring
while (_photonDispatchRing.TryDequeue(out var slot))
{
    if (slot.DispatchKey != null)
    {
        AddExpectedPositionDelta(slot.DispatchKey, -slot.ReservedDelta);
        ClearDispatchSyncPending(slot.DispatchKey);
    }
    _photonPool.ReleaseByIndex(slot.PoolIndex);
}
Print("Photon dispatch ring drained");

// Drain pending fleet dispatches
while (_pendingFleetDispatches.TryDequeue(out var req))
{
    if (req.DispatchKey != null)
    {
        AddExpectedPositionDelta(req.DispatchKey, -req.ReservedDelta);
        ClearDispatchSyncPending(req.DispatchKey);
    }
}
Print("Pending fleet dispatches drained");
```

#### Step 3: Refactor ProcessShutdownSIMA
1. Delete lines 155-179 (extracted code)
2. Replace with single call: `DrainPhotonQueuesOnShutdown();`
3. Maintain indentation (8 spaces from method start)
4. Verify surrounding code unchanged (lines 152-154, 181)

**Resulting ProcessShutdownSIMA**:
```csharp
private void ProcessShutdownSIMA()
{
    CancelAllV12GtcOrders(false); // Skip accounts with open positions
    StopReaperAudit();
    UnsubscribeFromFleetAccounts();
    
    DrainPhotonQueuesOnShutdown();
    
    Print("SIMA shutdown complete");
}
```

#### Step 4: Format Code
1. Run CSharpier: `dotnet csharpier format src/V12_002.SIMA.Lifecycle.cs`
2. Verify no formatting issues
3. Verify braces added if missing

#### Step 5: Verify Extraction
1. Check method signature matches specification
2. Check XML documentation present
3. Check no code duplication
4. Check ProcessShutdownSIMA simplified
5. Visual diff review (git diff)

### Test Requirements

#### Automated Tests
1. **Existing Tests Must Pass**:
   - Run: `dotnet test tests/V12_Performance.Tests/Core/FSMActorTests.cs`
   - Expected: All tests pass (no regressions)
   - Rationale: FSM/Actor tests validate lock-free correctness

2. **Build Verification**:
   - Run: `powershell -File .\scripts\build_readiness.ps1`
   - Expected: Zero build errors
   - Expected: CSharpier check passes

3. **Complexity Audit**:
   - Run: `python scripts/complexity_audit.py`
   - Expected: ProcessShutdownSIMA complexity ≤ 8
   - Expected: DrainPhotonQueuesOnShutdown complexity ≤ 5

4. **Lock-Free Audit**:
   - Run: `grep -n "lock(" src/V12_002.SIMA.Lifecycle.cs`
   - Expected: Zero matches in ProcessShutdownSIMA
   - Expected: Zero matches in DrainPhotonQueuesOnShutdown

5. **ASCII Audit**:
   - Run: `python check_ascii.py src/V12_002.SIMA.Lifecycle.cs`
   - Expected: Zero non-ASCII characters

#### Manual Tests
1. **NinjaTrader Integration**:
   - Run: `powershell -File .\deploy-sync.ps1`
   - Launch NinjaTrader
   - Press F5 to compile strategy
   - Expected: Zero compilation errors
   - Expected: BUILD_TAG verification passes

2. **Shutdown Behavior**:
   - Enable SIMA in NinjaTrader
   - Disable SIMA (triggers ProcessShutdownSIMA)
   - Verify: "Photon dispatch ring drained" logged
   - Verify: "Pending fleet dispatches drained" logged
   - Verify: "SIMA shutdown complete" logged
   - Verify: No exceptions thrown

3. **Queue State Verification**:
   - After shutdown, verify _photonDispatchRing.Count == 0
   - After shutdown, verify _pendingFleetDispatches.Count == 0
   - Verify no orphaned pool slots

### Verification Criteria

#### Primary Criteria (Must Pass)
1. ✅ ProcessShutdownSIMA complexity ≤ 8
2. ✅ DrainPhotonQueuesOnShutdown complexity ≤ 5
3. ✅ No lock() blocks in either method
4. ✅ All existing tests pass
5. ✅ Build succeeds with zero errors
6. ✅ CSharpier formatting passes
7. ✅ ASCII-only compliance verified
8. ✅ No behavioral changes (exact preservation)

#### Secondary Criteria (Should Pass)
1. ✅ Code readability improved
2. ✅ High-level orchestration separated from low-level cleanup
3. ✅ XML documentation complete
4. ✅ Git diff < 150 lines
5. ✅ No whitespace mutation

#### Failure Criteria (Abort If)
1. ❌ Complexity exceeds 8 after refactoring
2. ❌ Tests fail after extraction
3. ❌ Build fails after extraction
4. ❌ Lock() blocks detected
5. ❌ Behavioral changes detected

### Estimated Complexity Reduction

**Before Extraction**:
- ProcessShutdownSIMA: 11 (nested loops + conditionals)

**After Extraction**:
- ProcessShutdownSIMA: 8 (high-level orchestration)
- DrainPhotonQueuesOnShutdown: 5 (sequential drain logic)

**Net Reduction**: -3 points
**Target Achievement**: ✅ 8 ≤ 15 (Jane Street threshold)

### Dependencies

#### Direct Dependencies (Used by DrainPhotonQueuesOnShutdown)
- `_photonDispatchRing` (ConcurrentQueue<FleetDispatchSlot>)
- `_pendingFleetDispatches` (ConcurrentQueue<FleetDispatchRequest>)
- `_photonSideband` (FleetDispatchSideband[])
- `_photonPool` (ObjectPool)
- `AddExpectedPositionDelta(string key, int delta)` (External method)
- `ClearDispatchSyncPending(string key)` (External method)
- `Print(string message)` (NinjaTrader API)

#### No New Dependencies
- ✅ All dependencies already exist in current implementation
- ✅ No changes to class-level state or fields
- ✅ No new external method calls

### Rollback Steps

#### If Extraction Fails
1. **Immediate Rollback**:
   ```bash
   git reset --hard HEAD~1
   ```

2. **Verify Rollback**:
   - Run: `git log -1` (verify commit reverted)
   - Run: `dotnet build` (verify build succeeds)
   - Run: `dotnet test` (verify tests pass)

3. **Analyze Failure**:
   - Review build errors
   - Review test failures
   - Review complexity audit output
   - Document failure reason

4. **Recovery Options**:
   - Option A: Fix issue and retry extraction
   - Option B: Defer to next sprint
   - Option C: Escalate to Director for review

#### If Tests Fail After Extraction
1. **Isolate Failure**:
   - Run: `dotnet test --logger "console;verbosity=detailed"`
   - Identify failing test(s)
   - Review test output

2. **Root Cause Analysis**:
   - Compare behavior before/after extraction
   - Check for logic changes (should be zero)
   - Check for state mutation issues

3. **Fix or Rollback**:
   - If fixable: Apply fix and re-test
   - If not fixable: Rollback via `git reset --hard HEAD~1`

### Success Criteria

#### Ticket Complete When:
1. ✅ DrainPhotonQueuesOnShutdown method created
2. ✅ ProcessShutdownSIMA refactored
3. ✅ All verification criteria passed
4. ✅ All tests passed
5. ✅ Build succeeded
6. ✅ Complexity audit passed
7. ✅ Lock-free audit passed
8. ✅ ASCII audit passed
9. ✅ Manual NinjaTrader test passed
10. ✅ Git commit created with descriptive message

#### Commit Message Template:
```
feat(EPIC-CCN-114): Extract DrainPhotonQueuesOnShutdown from ProcessShutdownSIMA

- Reduces ProcessShutdownSIMA complexity from 11 to 8
- Consolidates queue drain logic into dedicated method
- Preserves exact behavior (no functional changes)
- Maintains lock-free Actor pattern
- Complies with V12 DNA principles

Complexity Impact:
- ProcessShutdownSIMA: 11 → 8 (-3 points)
- DrainPhotonQueuesOnShutdown: 5 (new)

Verification:
- Build: PASS
- Tests: PASS
- Complexity: PASS (≤ 15)
- Lock-Free: PASS (zero lock() blocks)
- ASCII: PASS (zero non-ASCII)

Ticket: TICKET-114-001
Epic: EPIC-CCN-114
Protocol: V12.23 No Scope Creep
```

---

## Execution Order

### Sequential Execution (Single Ticket)

**Order**: TICKET-114-001 (only ticket)

**Rationale**: Single extraction, no dependencies, no parallel execution needed.

**Execution Flow**:
1. Create DrainPhotonQueuesOnShutdown (Step 1-2)
2. Refactor ProcessShutdownSIMA (Step 3)
3. Format code (Step 4)
4. Verify extraction (Step 5)
5. Run automated tests
6. Run manual tests
7. Commit changes
8. Deploy & sync

---

## Epic Success Criteria

### Epic Complete When:
1. ✅ TICKET-114-001 completed successfully
2. ✅ ProcessShutdownSIMA complexity ≤ 8
3. ✅ All V12 DNA compliance checks passed
4. ✅ All PR hygiene checks passed
5. ✅ Pre-push validation passed (13 checks)
6. ✅ NinjaTrader integration verified
7. ✅ Git commit pushed to feature branch
8. ✅ PR created and reviewed

### Final Verification Checklist
- [ ] Complexity audit: ProcessShutdownSIMA ≤ 8
- [ ] Lock-free audit: Zero lock() blocks
- [ ] ASCII audit: Zero non-ASCII characters
- [ ] Build: Zero errors
- [ ] Tests: 100% pass rate
- [ ] CSharpier: Zero formatting issues
- [ ] Pre-push validation: 13/13 checks passed
- [ ] NinjaTrader: F5 compile success
- [ ] Git: Commit pushed to feature/EPIC-CCN-114-shutdown-refactor
- [ ] PR: Created and ready for review

---

## Notes

### Implementation Notes
- Single extraction keeps changes minimal and focused
- Exact code preservation ensures zero behavioral changes
- Lock-free primitives maintained throughout
- Jane Street cognitive simplicity principle upheld

### Risk Mitigation
- Incremental extraction (one method at a time)
- Comprehensive verification at each step
- Clear rollback plan if issues arise
- Manual testing in NinjaTrader for integration confidence

### Future Work
- Add TDD tests for ProcessShutdownSIMA (EPIC-CCN-10 backlog)
- Consider extracting CancelAllV12GtcOrders if complexity grows
- Monitor shutdown performance under load

---

**Document Version**: 1.0
**Generated**: 2026-06-13
**Phase**: 4 (Ticket Generation)
**Protocol**: V12.23 No Scope Creep
**Next Phase**: Phase 5 (Recursive Execution)
**Recommended Engineer**: Bob CLI (`v12-engineer`)

