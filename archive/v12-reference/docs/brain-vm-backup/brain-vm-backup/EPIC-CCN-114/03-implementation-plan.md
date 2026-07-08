# Phase 3: Implementation Plan - EPIC-CCN-114

## Epic Metadata
- **Epic ID**: EPIC-CCN-114
- **Target Method**: ProcessShutdownSIMA
- **Source File**: src/V12_002.SIMA.Lifecycle.cs
- **Current Complexity**: 11
- **Target Complexity**: 8
- **Phase**: 3 (Implementation Planning)
- **Protocol**: V12.23 No Scope Creep
- **Engineer Mode**: `v12-engineer` (Bob CLI)

## Implementation Overview

This plan executes a **single, focused extraction** to reduce ProcessShutdownSIMA complexity from 11 to 8 by consolidating queue drain logic into a dedicated helper method.

### Extraction Summary
- **Method**: DrainPhotonQueuesOnShutdown()
- **Lines Extracted**: 122-151 (photon ring + dispatch queue drain)
- **Complexity Reduction**: ~3 points
- **Risk Level**: LOW
- **Behavioral Changes**: NONE (pure refactoring)

## Pre-Implementation Checklist

### Environment Validation
- [ ] Verify working directory: `/home/malhitticrypto/universal-or-strategy`
- [ ] Confirm branch: `feature/EPIC-CCN-114-shutdown-refactor` (or create)
- [ ] Run baseline complexity audit: `python scripts/complexity_audit.py`
- [ ] Verify baseline build: `powershell -File .\scripts\build_readiness.ps1`
- [ ] Confirm no uncommitted changes: `git status`

### Baseline Metrics
```bash
# Capture baseline complexity
python scripts/complexity_audit.py > artifacts/EPIC-CCN-114-baseline-complexity.txt

# Capture baseline build status
powershell -File .\scripts\build_readiness.ps1 > artifacts/EPIC-CCN-114-baseline-build.txt

# Verify no lock() blocks
grep -n "lock(" src/V12_002.SIMA.Lifecycle.cs
# Expected: Zero matches in ProcessShutdownSIMA (lines 116-155)
```

### Knowledge Base Query
```bash
# Query Jane Street KB for shutdown patterns
python scripts/query_kb.py "graceful shutdown"
python scripts/query_kb.py "resource cleanup"
python scripts/query_kb.py "queue drain"
```

## Implementation Steps

### Step 1: Create DrainPhotonQueuesOnShutdown Method

**Location**: Insert after ProcessShutdownSIMA (after line 155)

**Method Signature**:
```csharp
/// <summary>
/// Drains photon dispatch ring and pending fleet dispatch queue during SIMA shutdown.
/// Rolls back reserved position deltas and clears dispatch-sync barriers for each discarded item.
/// </summary>
/// <remarks>
/// V28.0: Sideband-aware drain with XorShadow-free cleanup (no verification on shutdown).
/// Build 960: Ghost dispatch queue drain with delta rollback (A3-1 audit fix).
/// </remarks>
private void DrainPhotonQueuesOnShutdown()
{
    // Drain photon dispatch ring
    {
        FleetDispatchSlot ringSlot;
        while (_photonDispatchRing != null && _photonDispatchRing.TryDequeue(out ringSlot))
        {
            int _sbIdx = ringSlot.PoolSlotIndex;
            string _expectedKey =
                (_sbIdx >= 0 && _sbIdx < _photonSideband.Length) ? _photonSideband[_sbIdx].ExpectedKey : null;
            if (ringSlot.ReservedDelta != 0 && _expectedKey != null)
                AddExpectedPositionDelta(_expectedKey, -ringSlot.ReservedDelta);
            if (_expectedKey != null)
                ClearDispatchSyncPending(_expectedKey);
            if (_sbIdx >= 0)
            {
                _photonPool.ReleaseByIndex(_sbIdx);
                if (_sbIdx < _photonSideband.Length)
                    _photonSideband[_sbIdx] = default(FleetDispatchSideband);
            }
        }
        Print("[SIMA] Photon ring cleared on shutdown with delta rollback.");
    }
    
    // Drain pending fleet dispatch queue
    // A3-1: Drain ghost dispatch queue on SIMA disable (Build 960 audit fix)
    // B957/F2: Rollback ReservedDelta and clear dispatch-sync barrier for each discarded request.
    {
        FleetDispatchRequest ignored;
        while (_pendingFleetDispatches.TryDequeue(out ignored))
        {
            if (ignored.ReservedDelta != 0)
                AddExpectedPositionDelta(ignored.ExpectedKey, -ignored.ReservedDelta);
            ClearDispatchSyncPending(ignored.ExpectedKey);
        }
        Print("[SIMA] Dispatch queue cleared on shutdown with delta rollback.");
    }
}
```

**Verification Checkpoint 1**:
```bash
# Verify method added correctly
grep -A 50 "private void DrainPhotonQueuesOnShutdown" src/V12_002.SIMA.Lifecycle.cs

# Verify no syntax errors
dotnet build src/V12_002.csproj
```

### Step 2: Refactor ProcessShutdownSIMA

**Original Method** (lines 116-155):
```csharp
private void ProcessShutdownSIMA()
{
    CancelAllV12GtcOrders(false); // [BUILD 984] GTC sweep before teardown -- skip accounts with open positions
    StopReaperAudit();
    UnsubscribeFromFleetAccounts();
    // v28.0 shutdown drain: sideband-aware, XorShadow-free (we do not verify on shutdown;
    // we just need to release pool + roll back delta). Sideband entries are zeroed after.
    {
        FleetDispatchSlot ringSlot;
        while (_photonDispatchRing != null && _photonDispatchRing.TryDequeue(out ringSlot))
        {
            int _sbIdx = ringSlot.PoolSlotIndex;
            string _expectedKey =
                (_sbIdx >= 0 && _sbIdx < _photonSideband.Length) ? _photonSideband[_sbIdx].ExpectedKey : null;
            if (ringSlot.ReservedDelta != 0 && _expectedKey != null)
                AddExpectedPositionDelta(_expectedKey, -ringSlot.ReservedDelta);
            if (_expectedKey != null)
                ClearDispatchSyncPending(_expectedKey);
            if (_sbIdx >= 0)
            {
                _photonPool.ReleaseByIndex(_sbIdx);
                if (_sbIdx < _photonSideband.Length)
                    _photonSideband[_sbIdx] = default(FleetDispatchSideband);
            }
        }
        Print("[SIMA] Photon ring cleared on shutdown with delta rollback.");
    }
    // A3-1: Drain ghost dispatch queue on SIMA disable (Build 960 audit fix)
    // B957/F2: Rollback ReservedDelta and clear dispatch-sync barrier for each discarded request.
    {
        FleetDispatchRequest ignored;
        while (_pendingFleetDispatches.TryDequeue(out ignored))
        {
            if (ignored.ReservedDelta != 0)
                AddExpectedPositionDelta(ignored.ExpectedKey, -ignored.ReservedDelta);
            ClearDispatchSyncPending(ignored.ExpectedKey);
        }
        Print("[SIMA] Dispatch queue cleared on shutdown with delta rollback.");
    }
    Print("[SIMA LIFECYCLE] SIMA DISABLED -- Reaper stopped, handlers unsubscribed");
}
```

**Refactored Method**:
```csharp
private void ProcessShutdownSIMA()
{
    CancelAllV12GtcOrders(false); // [BUILD 984] GTC sweep before teardown -- skip accounts with open positions
    StopReaperAudit();
    UnsubscribeFromFleetAccounts();
    DrainPhotonQueuesOnShutdown();
    Print("[SIMA LIFECYCLE] SIMA DISABLED -- Reaper stopped, handlers unsubscribed");
}
```

**Implementation Instructions**:
1. Replace lines 122-151 with single call: `DrainPhotonQueuesOnShutdown();`
2. Preserve lines 117-121 (GTC cancel, Reaper stop, unsubscribe)
3. Preserve line 155 (final Print statement)
4. Verify indentation matches surrounding code
5. Verify no trailing whitespace

**Verification Checkpoint 2**:
```bash
# Verify refactored method
grep -A 10 "private void ProcessShutdownSIMA" src/V12_002.SIMA.Lifecycle.cs

# Verify method is now ~8 lines (excluding braces)
# Expected: 7 lines of code (3 calls + 1 drain + 1 print + 2 braces)

# Run complexity audit
python scripts/complexity_audit.py | grep "ProcessShutdownSIMA"
# Expected: Complexity ≤ 8
```

### Step 3: Format and Lint

**CSharpier Formatting**:
```bash
# Format the modified file
dotnet csharpier format src/V12_002.SIMA.Lifecycle.cs

# Verify formatting
dotnet csharpier check src/V12_002.SIMA.Lifecycle.cs
```

**Lint Verification**:
```bash
# Run Roslyn analyzers
powershell -File .\scripts\lint.ps1

# Verify no new warnings in V12_002.SIMA.Lifecycle.cs
```

**Verification Checkpoint 3**:
```bash
# Verify no formatting issues
dotnet csharpier check src/

# Verify no lint violations
powershell -File .\scripts\lint.ps1 | grep "V12_002.SIMA.Lifecycle.cs"
# Expected: Zero new violations
```

### Step 4: Build and Test

**Build Verification**:
```bash
# Full build with readiness checks
powershell -File .\scripts\build_readiness.ps1

# Verify zero errors
# Expected: "Build succeeded. 0 Error(s)"
```

**Unit Test Execution**:
```bash
# Run all tests
dotnet test

# Verify all tests pass
# Expected: "Test Run Successful. Total tests: X, Passed: X, Failed: 0"
```

**Verification Checkpoint 4**:
```bash
# Capture post-refactor metrics
python scripts/complexity_audit.py > artifacts/EPIC-CCN-114-post-complexity.txt

# Compare baseline vs. post-refactor
diff artifacts/EPIC-CCN-114-baseline-complexity.txt artifacts/EPIC-CCN-114-post-complexity.txt

# Verify ProcessShutdownSIMA complexity reduced from 11 to 8
```

### Step 5: Lock-Free Compliance Audit

**Grep Audit**:
```bash
# Verify no lock() blocks in target methods
grep -n "lock(" src/V12_002.SIMA.Lifecycle.cs | grep -E "(ProcessShutdownSIMA|DrainPhotonQueuesOnShutdown)"

# Expected: Zero matches
```

**Manual Code Review**:
- [ ] ProcessShutdownSIMA uses no lock() blocks
- [ ] DrainPhotonQueuesOnShutdown uses no lock() blocks
- [ ] All state mutations use ConcurrentQueue.TryDequeue (lock-free)
- [ ] No Monitor.Enter/Exit calls
- [ ] No Mutex/Semaphore usage

**Verification Checkpoint 5**:
```bash
# Generate lock-free audit report
grep -r "lock(" src/V12_002.SIMA.Lifecycle.cs > artifacts/EPIC-CCN-114-lock-audit.txt

# Verify zero matches in target methods
cat artifacts/EPIC-CCN-114-lock-audit.txt | grep -E "(ProcessShutdownSIMA|DrainPhotonQueuesOnShutdown)"
# Expected: Empty output
```

### Step 6: ASCII-Only Compliance

**ASCII Validation**:
```bash
# Run ASCII checker
python check_ascii.py src/V12_002.SIMA.Lifecycle.cs

# Expected: "All files are ASCII-compliant"
```

**Manual String Literal Review**:
- [ ] All Print() statements use ASCII characters
- [ ] No Unicode emoji or special characters
- [ ] No curly quotes (" " ' ')
- [ ] No em-dashes or en-dashes

**Verification Checkpoint 6**:
```bash
# Verify ASCII compliance
python check_ascii.py src/V12_002.SIMA.Lifecycle.cs
# Expected: Zero violations
```

## Post-Implementation Validation

### Complexity Verification
```bash
# Run complexity audit
python scripts/complexity_audit.py | grep "ProcessShutdownSIMA"

# Expected Output:
# ProcessShutdownSIMA: 8 (was 11)
# DrainPhotonQueuesOnShutdown: ~5
```

### Build Verification
```bash
# Full build with all checks
powershell -File .\scripts\build_readiness.ps1

# Expected: Zero errors, zero warnings in modified file
```

### Test Verification
```bash
# Run unit tests
dotnet test

# Expected: All tests pass (no new failures)
```

### Pre-Push Validation
```bash
# Run full pre-push validation (fast mode)
powershell -File .\scripts\pre_push_validation.ps1 -Fast

# Expected: All checks pass
```

### Hard-Link Sync
```bash
# Sync NinjaTrader hard links
powershell -File .\deploy-sync.ps1

# Expected: "Sync completed successfully"
```

## Success Criteria Validation

### Primary Success Criteria
- [ ] ProcessShutdownSIMA complexity ≤ 8 (verified via complexity_audit.py)
- [ ] No lock() blocks in ProcessShutdownSIMA or DrainPhotonQueuesOnShutdown
- [ ] DrainPhotonQueuesOnShutdown is private within V12_002.SIMA.Lifecycle.cs
- [ ] All existing tests pass without modification
- [ ] No changes to ProcessShutdownSIMA method signature
- [ ] Build succeeds with zero errors

### Secondary Success Criteria
- [ ] Code readability improved (high-level orchestration vs. low-level cleanup)
- [ ] Queue cleanup logic consolidated in single method
- [ ] XML documentation added to extracted method
- [ ] CSharpier formatting applied
- [ ] No lint violations introduced

### V12 DNA Compliance
- [ ] ✅ Correctness by Construction (no new state transitions)
- [ ] ✅ Lock-Free Actor Pattern (no lock() blocks)
- [ ] ✅ ASCII-Only Compliance (verified via check_ascii.py)
- [ ] ✅ Jane Street Alignment (complexity ≤ 15)
- [ ] ✅ Hard-Link Integrity (deploy-sync.ps1 executed)

## Rollback Procedure

### If Tests Fail
```bash
# Rollback to last commit
git reset --hard HEAD~1

# Verify rollback
git log -1

# Re-run tests to confirm stability
dotnet test
```

### If Build Fails
```bash
# Check build errors
dotnet build src/V12_002.csproj 2>&1 | tee artifacts/EPIC-CCN-114-build-error.txt

# Analyze errors
cat artifacts/EPIC-CCN-114-build-error.txt

# If syntax error: fix and retry
# If logic error: rollback and re-plan
git reset --hard HEAD~1
```

### If Complexity Target Missed
```bash
# Re-run complexity audit
python scripts/complexity_audit.py | grep "ProcessShutdownSIMA"

# If complexity > 8:
# - Review extraction boundaries
# - Consider additional extraction
# - Consult with Director before proceeding
```

## Commit Strategy

### Commit 1: Extract DrainPhotonQueuesOnShutdown
```bash
git add src/V12_002.SIMA.Lifecycle.cs
git commit -m "refactor(EPIC-CCN-114): Extract DrainPhotonQueuesOnShutdown

- Extract queue drain logic from ProcessShutdownSIMA
- Reduce complexity from 11 to 8
- Consolidate photon ring + dispatch queue cleanup
- No behavioral changes (pure refactoring)

Complexity: ProcessShutdownSIMA 11 → 8
Lock-Free: Verified (no lock() blocks)
Tests: All pass
Protocol: V12.23 No Scope Creep"
```

### Commit 2: Format and Lint
```bash
git add src/V12_002.SIMA.Lifecycle.cs
git commit -m "style(EPIC-CCN-114): Apply CSharpier formatting

- Format V12_002.SIMA.Lifecycle.cs
- Fix line endings (CRLF → LF)
- Add missing braces (V12 DNA compliance)

Lint: Zero violations
Format: CSharpier compliant"
```

### Commit 3: Sync Hard Links
```bash
git add .
git commit -m "build(EPIC-CCN-114): Sync NinjaTrader hard links

- Run deploy-sync.ps1
- Update hard links for V12_002.SIMA.Lifecycle.cs
- Verify BUILD_TAG consistency

Sync: Successful
Build: Verified"
```

## Phase Transition Criteria

### Ready for Phase 4 (Execution) When:
- [x] Implementation plan approved
- [x] Extraction strategy validated
- [x] Success criteria defined
- [x] Rollback procedure documented
- [x] Commit strategy defined

### Ready for Phase 5 (Verification) When:
- [ ] All implementation steps completed
- [ ] All verification checkpoints passed
- [ ] All success criteria met
- [ ] All commits pushed to branch
- [ ] Pre-push validation passed

### Ready for Phase 6 (Sign-off) When:
- [ ] Phase 5 verification completed
- [ ] PR created and reviewed
- [ ] Codacy quality gate passed
- [ ] CodeRabbit AI review passed
- [ ] Director approval received

## Risk Mitigation

### Risk 1: Complexity Target Missed
**Mitigation**: Run complexity audit after each extraction. If target missed, analyze and adjust extraction boundaries before proceeding.

### Risk 2: Test Failures
**Mitigation**: Run tests after each step. If failures occur, rollback immediately and analyze root cause before retrying.

### Risk 3: Build Errors
**Mitigation**: Verify syntax after each code change. Use incremental compilation to catch errors early.

### Risk 4: Lock-Free Violation
**Mitigation**: Grep for lock() blocks after each extraction. Manual code review required before commit.

### Risk 5: Hard-Link Desync
**Mitigation**: Run deploy-sync.ps1 after all code changes. Verify BUILD_TAG consistency before push.

## Engineer Handoff Notes

### Context for Bob CLI (`v12-engineer`)
- **Epic ID**: EPIC-CCN-114
- **Phase**: 3 (Implementation)
- **Target File**: src/V12_002.SIMA.Lifecycle.cs
- **Target Method**: ProcessShutdownSIMA (lines 116-155)
- **Extraction**: DrainPhotonQueuesOnShutdown (lines 122-151)
- **Complexity Goal**: 11 → 8

### Key Constraints
1. **No Behavioral Changes**: Pure refactoring only
2. **Lock-Free Compliance**: No lock() blocks allowed
3. **ASCII-Only**: No Unicode in string literals
4. **Single Extraction**: Do not over-engineer
5. **Test Preservation**: All existing tests must pass

### Recommended Approach
1. Read ProcessShutdownSIMA (lines 116-155)
2. Create DrainPhotonQueuesOnShutdown method
3. Refactor ProcessShutdownSIMA to call new method
4. Verify complexity reduction (11 → 8)
5. Run full validation suite
6. Commit and sync hard links

### Success Indicators
- ✅ Complexity audit shows 8 (not 11)
- ✅ Build succeeds with zero errors
- ✅ All tests pass
- ✅ No lock() blocks in target methods
- ✅ CSharpier formatting applied
- ✅ Hard links synced successfully

## Conclusion

This implementation plan provides a **step-by-step guide** for extracting DrainPhotonQueuesOnShutdown from ProcessShutdownSIMA, reducing complexity from 11 to 8 while maintaining strict adherence to V12 DNA principles.

**Key Decisions**:
1. **Single extraction** (not multiple) - avoids over-engineering
2. **Target complexity 8** (not 5-7) - realistic and safe
3. **Incremental verification** - catch errors early
4. **Comprehensive rollback** - safety net for failures

**Implementation Status**: ✅ READY - Proceed to Phase 4 (Execution)

---

**Document Version**: 1.0
**Created**: 2026-06-13
**Phase**: 3 (Implementation Planning)
**Protocol**: V12.23 No Scope Creep
**Next Phase**: Phase 4 (Execution via Bob CLI)
**Engineer**: Bob CLI (`v12-engineer`)
