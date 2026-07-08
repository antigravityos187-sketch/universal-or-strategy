# [EPIC-7-QUALITY-006] Error Handling: IPC Server Cleanup

## Priority: P1 HIGH

## Labels
`security`, `error-handling`, `P1`, `epic-7-quality-phase2`, `ipc`

## Summary
6 empty catch blocks in IPC server cleanup code silently swallow critical errors, masking connection failures and resource leaks.

## Affected Files
- [`src/V12_002.UI.IPC.Server.cs`](../../src/V12_002.UI.IPC.Server.cs)
  - Line 96: `catch (Exception)` - Client connection handling
  - Line 153: `catch { }` - Listener stop
  - Line 175: `catch { }` - Client close on disconnect
  - Line 357: `catch { }` - Session cleanup
  - Line 379: `catch { }` - Client close in loop
  - Line 385: `catch { }` - Final cleanup

## Security Impact
- **Severity:** HIGH
- **Risk:** Silent IPC failures can cause:
  - Zombie connections consuming resources
  - UI desync (strategy state not reflected in panel)
  - Memory leaks from unclosed sockets
  - Cascading failures in multi-account scenarios

## V12 DNA Violation
Violates **Jane Street Defensive Programming** principle: "All errors must be explicit and logged with context."

## Root Cause Analysis
IPC server cleanup uses empty catch blocks for defensive programming, but this masks legitimate errors:
1. **Network errors** (socket already closed, connection reset)
2. **Resource exhaustion** (too many open handles)
3. **Threading issues** (cleanup called from wrong thread)

## Remediation Approach

### Tier 1: Critical Paths (MUST log)
**Lines 96, 175, 357** - Active connection handling
```csharp
catch (Exception ex)
{
    // V12.EPIC-7-QUALITY-006: Log IPC cleanup errors for forensics
    Print($"[IPC] WARNING: Client cleanup failed [id={session?.ClientId ?? "unknown"}]: {ex.Message}");
    // Continue - cleanup is best-effort, but we need visibility
}
```

### Tier 2: Shutdown Paths (SHOULD log at debug level)
**Lines 153, 379, 385** - Server shutdown
```csharp
catch (Exception ex)
{
    // V12.EPIC-7-QUALITY-006: Debug-level logging for shutdown cleanup
    if (State == State.Terminated)
        return; // Expected during strategy termination
    Print($"[IPC] DEBUG: Shutdown cleanup warning: {ex.Message}");
}
```

### Additional Improvements
1. **Add connection state tracking** - Detect zombie connections
2. **Implement timeout-based cleanup** - Force-close stale connections after 30s
3. **Add IPC health metrics** - Track connection count, cleanup failures
4. **Graceful degradation** - Continue strategy execution even if IPC fails

## Acceptance Criteria
- [ ] All 6 catch blocks have logging with context
- [ ] Connection ID included in all log messages
- [ ] Cleanup failures tracked in IPC health metrics
- [ ] No new exceptions introduced by logging code
- [ ] Build passes with 0 warnings
- [ ] IPC stress test passes (100 rapid connect/disconnect cycles)

## Testing Strategy
1. **Unit Test**: Mock socket exceptions, verify logging
2. **Integration Test**: Rapid connect/disconnect (zombie connection detection)
3. **Stress Test**: 10 concurrent clients, random disconnects
4. **Forensic Test**: Inject network errors, verify log output

## Effort Estimate
**4-6 hours**
- 2h: Add logging to all catch blocks
- 1h: Implement connection state tracking
- 1h: Add IPC health metrics
- 1-2h: Testing and verification

## Dependencies
- None (independent ticket)

## Blockers
- None

## References
- Codacy Finding: "Empty catch block" (6 instances in IPC.Server.cs)
- V12 DNA: [AGENTS.md](../../AGENTS.md) Lines 77-110 (Karpathy Protocols)
- Jane Street: Explicit error propagation mandate

## Implementation Notes
- Use `Print()` for logging (NinjaTrader standard)
- Avoid throwing new exceptions in catch blocks
- Keep cleanup logic idempotent (safe to call multiple times)
- Consider adding `[MethodImpl(MethodImplOptions.AggressiveInlining)]` to hot paths

## Status
🔴 **OPEN** - Not Started

## Assigned To
_Unassigned_ (Recommend: Advanced mode)

## Created
2026-05-26T17:05:00Z