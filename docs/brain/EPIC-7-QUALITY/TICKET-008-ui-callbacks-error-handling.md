# [EPIC-7-QUALITY-008] Error Handling: UI Callbacks

## Priority: P2 MEDIUM

## Labels
`security`, `error-handling`, `P2`, `epic-7-quality-phase2`, `ui`

## Summary
2 empty catch blocks in UI callback cleanup code silently swallow errors, reducing observability of UI failures.

## Affected Files
- [`src/V12_002.UI.Callbacks.cs`](../../src/V12_002.UI.Callbacks.cs)
  - Line 122: `catch { }` - UI element cleanup
- [`src/V12_002.UI.IPC.Commands.Misc.cs`](../../src/V12_002.UI.IPC.Commands.Misc.cs)
  - Line 229: `catch { }` - Stale client cleanup

## Security Impact
- **Severity:** MEDIUM
- **Risk:** Silent UI failures reduce observability:
  - UI elements not properly disposed (minor memory leak)
  - Stale IPC clients not cleaned up (resource leak)
  - No forensic trail for debugging UI issues

## V12 DNA Violation
Violates **Karpathy Protocol**: "State verify criteria before each implementation stage" - can't verify UI cleanup without logging.

## Root Cause Analysis
UI cleanup uses empty catch blocks for defensive programming, but this reduces observability:
1. **UI element disposal** - May fail if element already disposed or thread mismatch
2. **IPC client cleanup** - May fail if socket already closed

## Remediation Approach

### Tier 1: UI Element Cleanup (SHOULD log at warning level)
**UI.Callbacks.cs:122**
```csharp
catch (Exception ex)
{
    // V12.EPIC-7-QUALITY-008: Log UI cleanup warnings for observability
    Print($"[UI] WARNING: UI element cleanup failed: {ex.Message}");
    // Continue - UI cleanup is best-effort, but we need visibility for debugging
}
```

### Tier 2: IPC Client Cleanup (SHOULD log at debug level)
**UI.IPC.Commands.Misc.cs:229**
```csharp
catch (Exception ex)
{
    // V12.EPIC-7-QUALITY-008: Log stale client cleanup for forensics
    Print($"[IPC] DEBUG: Stale client cleanup warning: {ex.Message}");
    // Expected during normal operation - client may have already disconnected
}
```

### Additional Improvements
1. **Add UI health metrics** - Track cleanup failure rate
2. **Implement UI element lifecycle tracking** - Detect leaked elements
3. **Add debug mode** - Verbose logging for UI troubleshooting

## Acceptance Criteria
- [ ] Both catch blocks have logging with context
- [ ] UI cleanup failures tracked in health metrics
- [ ] No new exceptions introduced by logging code
- [ ] Build passes with 0 warnings
- [ ] UI stress test passes (100 rapid panel open/close cycles)

## Testing Strategy
1. **Unit Test**: Mock UI exceptions, verify logging
2. **Integration Test**: Rapid panel open/close (leak detection)
3. **Stress Test**: 100 concurrent IPC clients, random disconnects
4. **Forensic Test**: Inject UI thread exceptions, verify log output

## Effort Estimate
**2-3 hours**
- 1h: Add logging to both catch blocks
- 0.5h: Add UI health metrics
- 0.5-1h: Testing and verification

## Dependencies
- None (independent ticket)

## Blockers
- None

## References
- Codacy Finding: "Empty catch block" (2 instances in UI code)
- V12 DNA: [AGENTS.md](../../AGENTS.md) Lines 105-110 (Goal-Driven Execution)

## Implementation Notes
- Use `Print()` for logging (NinjaTrader standard)
- Consider using `Dispatcher.InvokeAsync` for thread-safe UI cleanup
- Keep cleanup logic idempotent (safe to call multiple times)

## Status
🔴 **OPEN** - Not Started

## Assigned To
_Unassigned_ (Recommend: Advanced mode)

## Created
2026-05-26T17:08:00Z