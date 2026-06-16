# Extraction Tickets: EPIC-CCN-078

## Overview
- **Total Tickets**: 4
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3 → TICKET-4)
- **Estimated Effort**: 6 hours
- **Target Method**: StopIpcServer
- **File**: src/V12_002.UI.IPC.Server.cs
- **Current CYC**: 12
- **Target CYC**: ≤8 (Jane Street strict standard)

## TICKET-1: Extract StopListener Helper

### Scope
- **Current Method**: `StopIpcServer`
- **Current CYC**: 12
- **Target CYC**: 10 (after this extraction)
- **Extraction**: Listener cleanup logic into dedicated helper

### Implementation
1. Create new private method `StopListener()` with CYC ~2
2. Move listener null check and Stop() call into helper
3. Preserve exact logic: `if (ipcListener != null) { ipcListener.Stop(); ipcListener = null; }`
4. Replace original code block with single call to `StopListener()`
5. Add XML documentation comment for helper method

### Acceptance Criteria
- [ ] Method `StopListener()` created with CYC ≤2
- [ ] Original listener cleanup logic preserved exactly
- [ ] StopIpcServer CYC reduced from 12 to ~10
- [ ] All tests pass (no behavioral changes)
- [ ] Build succeeds with zero errors
- [ ] No lock statements introduced (grep verification)
- [ ] CSharpier formatting applied

### Dependencies
- None (first ticket)

---

## TICKET-2: Extract StopThread Helper

### Scope
- **Current Method**: `StopIpcServer`
- **Current CYC**: 10 (after TICKET-1)
- **Target CYC**: 8 (after this extraction)
- **Extraction**: Thread cleanup logic into dedicated helper

### Implementation
1. Create new private method `StopThread()` with CYC ~2
2. Move thread null check, IsAlive check, and Join(500) call into helper
3. Preserve exact logic: `if (ipcThread != null && ipcThread.IsAlive) { ipcThread.Join(500); }`
4. Replace original code block with single call to `StopThread()`
5. Add XML documentation comment for helper method

### Acceptance Criteria
- [ ] Method `StopThread()` created with CYC ≤2
- [ ] Original thread cleanup logic preserved exactly
- [ ] StopIpcServer CYC reduced from ~10 to ~8
- [ ] All tests pass (no behavioral changes)
- [ ] Build succeeds with zero errors
- [ ] No lock statements introduced (grep verification)
- [ ] CSharpier formatting applied

### Dependencies
- TICKET-1 must be completed first

---

## TICKET-3: Extract CleanupConnectedClients Helper

### Scope
- **Current Method**: `StopIpcServer`
- **Current CYC**: 8 (after TICKET-2)
- **Target CYC**: 5 (after this extraction)
- **Extraction**: Client cleanup logic with zombie detection into dedicated helper

### Implementation
1. Create new private method `CleanupConnectedClients()` with CYC ~6
2. Move entire client cleanup block including:
   - Null check for connectedClients
   - Foreach loop over connectedClients.ToArray()
   - Socket shutdown logic with try-catch
   - Zombie connection detection (Interlocked.Increment)
   - Cleanup failure tracking (Interlocked.Increment)
   - connectedClients.Clear() call
3. Preserve all nested try-catch blocks exactly
4. Preserve all atomic counter operations (Interlocked.Increment)
5. Replace original code block with single call to `CleanupConnectedClients()`
6. Add XML documentation comment for helper method

### Acceptance Criteria
- [ ] Method `CleanupConnectedClients()` created with CYC ≤6
- [ ] Original client cleanup logic preserved exactly
- [ ] Zombie detection logic unchanged (Interlocked.Increment)
- [ ] Cleanup failure tracking unchanged (Interlocked.Increment)
- [ ] All try-catch blocks preserved
- [ ] StopIpcServer CYC reduced from ~8 to ~5
- [ ] All tests pass (no behavioral changes)
- [ ] Build succeeds with zero errors
- [ ] No lock statements introduced (grep verification)
- [ ] CSharpier formatting applied

### Dependencies
- TICKET-2 must be completed first

---

## TICKET-4: Extract ResetCounters Helper

### Scope
- **Current Method**: `StopIpcServer`
- **Current CYC**: 5 (after TICKET-3)
- **Target CYC**: 4 (after this extraction - FINAL TARGET MET)
- **Extraction**: Counter reset logic into dedicated helper

### Implementation
1. Create new private method `ResetCounters()` with CYC ~1
2. Move counter reset logic: `Interlocked.Exchange(ref ipcQueuedCommandCount, 0)`
3. Replace original code line with single call to `ResetCounters()`
4. Add XML documentation comment for helper method

### Acceptance Criteria
- [ ] Method `ResetCounters()` created with CYC ≤1
- [ ] Original counter reset logic preserved exactly (Interlocked.Exchange)
- [ ] StopIpcServer CYC reduced from ~5 to 4 ✅ **FINAL TARGET MET**
- [ ] All tests pass (no behavioral changes)
- [ ] Build succeeds with zero errors
- [ ] No lock statements introduced (grep verification)
- [ ] CSharpier formatting applied
- [ ] **EPIC-CCN-078 COMPLETE**: StopIpcServer CYC ≤8 achieved

### Dependencies
- TICKET-3 must be completed first

---

## Final Verification Checklist

After completing all 4 tickets:

- [ ] StopIpcServer final CYC = 4 (target ≤8) ✅
- [ ] All 4 helper methods created with documented CYC
- [ ] No behavioral changes (exact logic preservation)
- [ ] All tests pass
- [ ] Build succeeds with zero errors
- [ ] Zero lock statements in all methods (grep verification)
- [ ] CSharpier formatting applied to all modified code
- [ ] Jane Street compliance maintained (cognitive simplicity, testability)
- [ ] Hard-link sync completed: `powershell -File .\deploy-sync.ps1`
- [ ] Pre-push validation passed: `powershell -File .\scripts\pre_push_validation.ps1`

---

## Complexity Reduction Summary

| Stage | CYC | Change |
|-------|-----|--------|
| Original | 12 | Baseline |
| After TICKET-1 | 10 | -2 (StopListener extracted) |
| After TICKET-2 | 8 | -2 (StopThread extracted) |
| After TICKET-3 | 5 | -3 (CleanupConnectedClients extracted) |
| After TICKET-4 | 4 | -1 (ResetCounters extracted) |
| **Total Reduction** | **-8** | **67% reduction** ✅ |

---

**Document Status**: COMPLETE
**Phase 4 Status**: READY FOR EXECUTION
**Next Phase**: Phase 5 (Ticket Execution)
