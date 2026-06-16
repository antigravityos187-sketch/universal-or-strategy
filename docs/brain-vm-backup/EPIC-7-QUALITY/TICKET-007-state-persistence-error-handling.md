# [EPIC-7-QUALITY-007] Error Handling: State Persistence

## Priority: P1 HIGH

## Labels
`security`, `error-handling`, `P1`, `epic-7-quality-phase2`, `state-persistence`

## Summary
7 empty catch blocks in state persistence code silently swallow file I/O errors, risking data loss and state corruption.

## Affected Files
- [`src/V12_002.StickyState.cs`](../../src/V12_002.StickyState.cs)
  - Line 100: `catch { }` - Backup file cleanup
- [`src/V12_002.UI.Compliance.cs`](../../src/V12_002.UI.Compliance.cs)
  - Line 310: `catch { }` - CSV write
  - Line 371: `catch { }` - JSON export
  - Line 392: `catch { }` - File dialog
  - Line 408: `catch { }` - File dialog
  - Line 420: `catch { }` - File dialog
  - Line 719: `catch { }` - Compliance log write

## Security Impact
- **Severity:** HIGH
- **Risk:** Silent file I/O failures can cause:
  - **State corruption** - Partial writes leave invalid JSON
  - **Data loss** - Failed writes not detected, user assumes success
  - **Compliance violations** - Missing trade logs for audit trail
  - **Desync on restart** - Strategy loads stale/corrupt state

## V12 DNA Violation
Violates **Platinum Standard**: "Make illegal states unrepresentable" - silent failures create undetectable corrupt states.

## Root Cause Analysis
File I/O uses empty catch blocks for defensive programming, but this masks critical failures:
1. **Disk full** - Write fails, no notification
2. **Permission denied** - File locked by another process
3. **Network drive timeout** - MyDocuments on network share
4. **Corrupt filesystem** - Write succeeds but data unreadable

## Remediation Approach

### Tier 1: Critical State Persistence (MUST log + fallback)
**StickyState.cs:100, Compliance.cs:719**
```csharp
catch (Exception ex)
{
    // V12.EPIC-7-QUALITY-007: Critical state persistence failure
    Print($"[STATE] ERROR: Failed to persist state to {_stickyStatePath}: {ex.Message}");
    
    // Fallback: Try in-memory backup
    if (_lastKnownGoodState != null)
    {
        Print("[STATE] WARNING: Using last known good state from memory");
        // Keep strategy running with cached state
    }
    else
    {
        Print("[STATE] CRITICAL: No fallback state available - manual intervention required");
        // Consider: Alert user via UI, disable auto-trading
    }
}
```

### Tier 2: Compliance Logging (MUST log + retry)
**Compliance.cs:310, 371, 719**
```csharp
catch (Exception ex)
{
    // V12.EPIC-7-QUALITY-007: Compliance log write failure
    Print($"[COMPLIANCE] ERROR: Failed to write {logType} to {path}: {ex.Message}");
    
    // Retry with exponential backoff (max 3 attempts)
    if (retryCount < 3)
    {
        Thread.Sleep(100 * (int)Math.Pow(2, retryCount));
        return WriteComplianceLog(data, ++retryCount);
    }
    
    // Final fallback: Queue for later write
    _pendingComplianceLogs.Enqueue(new ComplianceLogEntry { Data = data, Timestamp = DateTime.UtcNow });
    Print($"[COMPLIANCE] WARNING: Queued log for retry ({_pendingComplianceLogs.Count} pending)");
}
```

### Tier 3: UI File Dialogs (SHOULD log warning)
**Compliance.cs:392, 408, 420**
```csharp
catch (Exception ex)
{
    // V12.EPIC-7-QUALITY-007: File dialog failure (user-initiated)
    Print($"[UI] WARNING: File dialog failed: {ex.Message}");
    // User will see dialog didn't open - no silent failure
}
```

### Additional Improvements
1. **Atomic writes** - Write to temp file, then move (prevents partial writes)
2. **Checksum validation** - Verify file integrity after write
3. **Retry queue** - Persist failed writes to memory, retry on next tick
4. **Health monitoring** - Track write success rate, alert on degradation

## Acceptance Criteria
- [ ] All 7 catch blocks have logging with context
- [ ] Critical paths (StickyState, compliance logs) have fallback strategies
- [ ] Atomic file writes implemented for state persistence
- [ ] Retry logic added for compliance logging (max 3 attempts)
- [ ] Write failure metrics tracked (success rate, retry count)
- [ ] Build passes with 0 warnings
- [ ] State persistence stress test passes (100 rapid save/load cycles)

## Testing Strategy
1. **Unit Test**: Mock file I/O exceptions, verify logging and fallback
2. **Integration Test**: Simulate disk full, verify retry logic
3. **Stress Test**: Rapid state changes (1000 writes in 10s)
4. **Corruption Test**: Kill process mid-write, verify atomic write protection
5. **Forensic Test**: Inject permission errors, verify log output

## Effort Estimate
**3-4 hours**
- 1h: Add logging to all catch blocks
- 1h: Implement atomic file writes
- 0.5h: Add retry logic for compliance logs
- 0.5-1h: Testing and verification

## Dependencies
- None (independent ticket)

## Blockers
- None

## References
- Codacy Finding: "Empty catch block" (7 instances in state persistence)
- V12 DNA: [AGENTS.md](../../AGENTS.md) Lines 46-59 (Platinum Standard)
- Jane Street: Fail-fast philosophy, explicit error propagation

## Implementation Notes
- Use `File.Move` for atomic writes (POSIX rename semantics on Windows)
- Add `[MethodImpl(MethodImplOptions.Synchronized)]` to prevent concurrent writes
- Consider using `FileStream` with `FileShare.None` for exclusive access
- Validate JSON after write using `JsonDocument.Parse` (cheap validation)

## Related Tickets
- **TICKET-010**: File I/O path validation (should complete before this)
- **TICKET-011**: Retry logic (this ticket implements basic retry, 011 adds exponential backoff)

## Status
🔴 **OPEN** - Not Started

## Assigned To
_Unassigned_ (Recommend: Advanced mode)

## Created
2026-05-26T17:06:00Z