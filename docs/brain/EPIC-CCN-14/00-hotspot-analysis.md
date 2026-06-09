# EPIC-CCN-14: ProcessIpcCommands Hotspot Analysis

## Target Method
- **Method**: `ProcessIpcCommands`
- **File**: `src/V12_002.UI.IPC.cs`
- **Line**: 260-430 (171 lines)
- **Cyclomatic Complexity**: 76
- **Max Nesting**: 6
- **Churn**: 12 commits
- **Hotspot Score**: 194.94 (2nd highest in codebase)

## Complexity Breakdown

### Current Structure (CYC 76)
The method has a massive switch/dispatch pattern with multiple nested conditions:

1. **Termination Guard** (lines 262-269): CYC +2
   - Early exit if terminating
   - Queue drain logic

2. **Queue Processing Loop** (lines 273-420): CYC +50+
   - While loop with drain limit
   - Command validation (null/whitespace/length)
   - Command parsing (split by '|')
   - Timestamp extraction loop
   - Timestamp validation
   - IPC hardening validation (4-way switch)
   - Allowlist validation
   - Global command detection (20+ OR conditions)
   - Symbol matching logic (11+ OR conditions)
   - Diagnostic logging
   - Symbol filter check
   - Actor enqueue

3. **Continuation Trigger** (lines 422-429): CYC +2
   - Queue non-empty check
   - TriggerCustomEvent

### Complexity Hotspots

**Primary Driver**: Lines 341-391 (Global Command + Symbol Matching)
- 20+ boolean OR conditions for `isGlobalCommand`
- 11+ boolean OR conditions for `isForMe`
- **Estimated CYC contribution**: ~35

**Secondary Driver**: Lines 307-327 (IPC Hardening Validation)
- 4-way switch statement
- **Estimated CYC contribution**: ~5

**Tertiary Driver**: Lines 295-302 (Timestamp Extraction)
- Loop with conditional break
- **Estimated CYC contribution**: ~3

## Jane Street Alignment Issues

### Cognitive Complexity
- **Current**: Single 171-line method doing 5+ distinct responsibilities
- **Jane Street Standard**: Functions should be simple enough to reason about under microsecond latency constraints
- **Violation**: CYC 76 >> 15 threshold

### Testability
- **Current**: Exponential path growth (2^76 possible paths)
- **Jane Street Standard**: Exhaustive testing requires manageable path count
- **Violation**: Impossible to test all paths

### Lock-Free Correctness
- **Current**: Uses `Enqueue(ctx => ...)` pattern (GOOD)
- **Status**: ✅ No lock violations detected

## Extraction Strategy

### Phase 1: Command Validation (Target CYC ≤8)
Extract lines 281-304 into:
- `ValidateCommandFormat(string command, out string[] parts, out long senderTicks)`
- Handles: null/whitespace, length, parsing, timestamp extraction, timestamp guard
- **Estimated CYC**: 6

### Phase 2: Symbol Matching (Target CYC ≤8)
Extract lines 335-391 into:
- `IsCommandForThisInstrument(string action, string targetSymbol, out bool isGlobalCommand)`
- Handles: global command detection, symbol matching logic
- **Estimated CYC**: 8 (after simplification)

### Phase 3: Hardening Validation Handler (Target CYC ≤8)
Extract lines 307-327 into:
- `HandleValidationFailure(ValidationResult result, string action)`
- Handles: switch statement, logging, backpressure
- **Estimated CYC**: 5

### Phase 4: Main Loop Simplification (Target CYC ≤8)
After extractions, main loop becomes:
```csharp
while (drainedCount < IpcMaxCommandsPerDrain && ipcCommandQueue.TryDequeue(out string command))
{
    if (!ValidateCommandFormat(command, out var parts, out var senderTicks))
        continue;
    
    string action = parts[0].Trim().ToUpperInvariant();
    
    ValidationResult validationResult = ValidateIpcCommand(action, parts);
    if (validationResult != ValidationResult.Valid)
    {
        HandleValidationFailure(validationResult, action);
        continue;
    }
    
    if (!IsAllowedIpcAction(action))
    {
        Interlocked.Increment(ref _ipcAllowlistRejectCount);
        Print($"V12 IPC REJECT: action '{action}' is not allowed");
        continue;
    }
    
    string targetSymbol = parts.Length > 1 ? parts[1] : "Global";
    if (!IsCommandForThisInstrument(action, targetSymbol, out bool isGlobalCommand))
        continue;
    
    Enqueue(ctx => ctx.ProcessIpcCommandCore(action, parts, senderTicks));
}
```
**Estimated CYC**: 7

## Success Criteria
- ✅ All extracted methods CYC ≤8
- ✅ Main method CYC ≤8
- ✅ Zero locks (already compliant)
- ✅ ASCII-only (already compliant)
- ✅ Zero logic drift (pure structural movement)
- ✅ F5 verification passes after each ticket

## Risk Assessment
- **Complexity**: HIGH (CYC 76 → 8 requires 3 extractions)
- **Churn**: MEDIUM (12 commits, but stable pattern)
- **Dependencies**: LOW (self-contained IPC processing)
- **Test Coverage**: UNKNOWN (no existing tests for this method)

## Next Steps
1. Phase 1: Create intake document
2. Phase 2: Create implementation plan
3. Phase 2.3: Run sentinel audit
4. Phase 3: Validate plan against V12 DNA
5. Phase 4: Generate tickets
6. Phase 5: Execute tickets (F5 after each)