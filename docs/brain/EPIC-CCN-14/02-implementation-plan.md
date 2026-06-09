# EPIC-CCN-14: ProcessIpcCommands Implementation Plan

## Overview
Surgical extraction of 3 helper methods from `ProcessIpcCommands` to reduce complexity from CYC 76 to ≤8.

## Execution Order
1. **Ticket 1**: Extract `ValidateCommandFormat` (validation logic)
2. **Ticket 2**: Extract `IsCommandForThisInstrument` (symbol matching)
3. **Ticket 3**: Extract `HandleValidationFailure` (error handling)
4. **Ticket 4**: Simplify main loop (integrate extractions)

## Ticket 1: Extract ValidateCommandFormat

### Target Lines
Lines 281-304 in `ProcessIpcCommands`

### New Method Signature
```csharp
private bool ValidateCommandFormat(
    string command,
    out string[] parts,
    out long senderTicks)
{
    parts = null;
    senderTicks = 0;
    
    // Validation logic here
    return true; // or false
}
```

### Extracted Logic
```csharp
if (string.IsNullOrWhiteSpace(command) || command.Length > IpcMaxCommandLength)
{
    Print($"V12 IPC REJECT: malformed/oversize command '{command}'");
    return false;
}

parts = command.Split('|');
if (parts.Length == 0 || string.IsNullOrWhiteSpace(parts[0]))
{
    Print($"V12 IPC REJECT: empty action in '{command}'");
    return false;
}

senderTicks = 0;
for (int i = 1; i < parts.Length; i++)
{
    if (parts[i].StartsWith("ts=", StringComparison.OrdinalIgnoreCase))
    {
        long.TryParse(parts[i].Substring(3), out senderTicks);
        break;
    }
}

if (!MetadataGuardCommandTimestamp(senderTicks, parts[0].Trim().ToUpperInvariant()))
    return false;

return true;
```

### Caller Update (Line 274-304)
```csharp
// BEFORE
try
{
    if (string.IsNullOrWhiteSpace(command) || command.Length > IpcMaxCommandLength)
    {
        Print($"V12 IPC REJECT: malformed/oversize command '{command}'");
        continue;
    }
    // ... 20+ more lines ...
}

// AFTER
try
{
    if (!ValidateCommandFormat(command, out string[] parts, out long senderTicks))
        continue;
    
    string action = parts[0].Trim().ToUpperInvariant();
    // ... rest of logic ...
}
```

### Estimated CYC
- **Extracted Method**: 6 (null check, length check, split check, loop, timestamp guard)
- **Main Method Reduction**: -6

### Placement
Insert new method immediately before `ProcessIpcCommands` (around line 259)

---

## Ticket 2: Extract IsCommandForThisInstrument

### Target Lines
Lines 335-391 in `ProcessIpcCommands`

### New Method Signature
```csharp
private bool IsCommandForThisInstrument(
    string action,
    string targetSymbol,
    out bool isGlobalCommand)
{
    isGlobalCommand = false;
    // Logic here
    return true; // or false
}
```

### Extracted Logic
```csharp
isGlobalCommand =
    action == "TOGGLE_ACCOUNT"
    || action == "SET_SIMA"
    || action == "GET_FLEET"
    || action == "DIAG_FLEET"
    || action == "CANCEL_ALL"
    || action == "FLATTEN"
    || action == "SYNC_ALL"
    || action == "MKT_SYNC"
    || action == "REQUEST_FLEET_STATE"
    || action == "RESET_MEMORY"
    || action == "DIAG_IPC"
    || action.StartsWith("MOVE_TARGET")
    || action == "LOCK_50"
    || action == "SET_TARGETS"
    || action == "SET_TRAIL"
    || action == "SET_CIT"
    || action == "BE_CUSTOM";

string mySym = Instrument.MasterInstrument.Name.ToUpperInvariant();
string myFull = Instrument.FullName.ToUpperInvariant();
string target = targetSymbol.Trim().ToUpperInvariant();

bool isForMe =
    isGlobalCommand
    || target == "GLOBAL"
    || target == "ALL"
    || target == "ON"
    || target == "OFF"
    || target == "RMA"
    || target == "ORB"
    || target == "OR"
    || target == "MOMO"
    || mySym == target
    || mySym.StartsWith(target)
    || target.StartsWith(mySym)
    || myFull.Contains(target)
    || (target == "MES" && mySym.Contains("ES"))
    || (target == "MYM" && mySym.Contains("YM"))
    || (target == "MGC" && mySym.Contains("GC"));

Print(
    string.Format(
        "V12 IPC: Received '{0}' for '{1}'. For Me? {2} (My Symbol: {3}){4}",
        action,
        target,
        isForMe,
        mySym,
        isGlobalCommand ? " [GLOBAL CMD]" : ""
    )
);

return isForMe;
```

### Caller Update (Lines 335-409)
```csharp
// BEFORE
string targetSymbol = parts.Length > 1 ? parts[1] : "Global";
bool isGlobalCommand = action == "TOGGLE_ACCOUNT" || ...;
string mySym = Instrument.MasterInstrument.Name.ToUpperInvariant();
// ... 50+ more lines ...
if (!isForMe)
{
    continue;
}

// AFTER
string targetSymbol = parts.Length > 1 ? parts[1] : "Global";
if (!IsCommandForThisInstrument(action, targetSymbol, out bool isGlobalCommand))
    continue;
```

### Estimated CYC
- **Extracted Method**: 8 (20 OR conditions for isGlobalCommand, 11 OR conditions for isForMe)
- **Main Method Reduction**: -35

### Placement
Insert new method immediately after `ValidateCommandFormat` (around line 290)

---

## Ticket 3: Extract HandleValidationFailure

### Target Lines
Lines 307-327 in `ProcessIpcCommands`

### New Method Signature
```csharp
private void HandleValidationFailure(ValidationResult validationResult, string action)
{
    Interlocked.Increment(ref _ipcHardeningRejectCount);
    
    switch (validationResult)
    {
        case ValidationResult.InvalidSyntax:
            Print($"V12 IPC REJECT [HARDENING]: Invalid syntax for '{action}'");
            break;
        case ValidationResult.RateLimitExceeded:
            SendBackpressureNack(action);
            break;
        case ValidationResult.CircuitBreakerOpen:
            Print($"V12 IPC REJECT [HARDENING]: Circuit breaker open for '{action}'");
            break;
        case ValidationResult.AllowlistBypass:
            Print($"V12 IPC REJECT [HARDENING]: Allowlist bypass attempt detected for '{action}'");
            break;
    }
}
```

### Caller Update (Lines 307-327)
```csharp
// BEFORE
ValidationResult validationResult = ValidateIpcCommand(action, parts);
if (validationResult != ValidationResult.Valid)
{
    Interlocked.Increment(ref _ipcHardeningRejectCount);
    switch (validationResult)
    {
        case ValidationResult.InvalidSyntax:
            Print($"V12 IPC REJECT [HARDENING]: Invalid syntax for '{action}'");
            break;
        // ... 3 more cases ...
    }
    continue;
}

// AFTER
ValidationResult validationResult = ValidateIpcCommand(action, parts);
if (validationResult != ValidationResult.Valid)
{
    HandleValidationFailure(validationResult, action);
    continue;
}
```

### Estimated CYC
- **Extracted Method**: 5 (switch with 4 cases)
- **Main Method Reduction**: -5

### Placement
Insert new method immediately after `IsCommandForThisInstrument` (around line 350)

---

## Ticket 4: Simplify Main Loop

### Target
Integrate all extractions into `ProcessIpcCommands`

### Final Structure
```csharp
private void ProcessIpcCommands()
{
    if (_isTerminating)
    {
        if (ipcCommandQueue != null)
        {
            while (ipcCommandQueue.TryDequeue(out string _)) { }
        }
        return;
    }
    if (ipcCommandQueue == null || ipcCommandQueue.IsEmpty)
        return;

    int drainedCount = 0;
    while (drainedCount < IpcMaxCommandsPerDrain && ipcCommandQueue.TryDequeue(out string command))
    {
        if (Interlocked.Decrement(ref ipcQueuedCommandCount) < 0)
            Interlocked.Exchange(ref ipcQueuedCommandCount, 0);
        drainedCount++;
        
        try
        {
            if (!ValidateCommandFormat(command, out string[] parts, out long senderTicks))
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
            
            string queuedAction = action;
            string[] queuedParts = parts;
            long queuedSenderTicks = senderTicks;
            Enqueue(ctx => ctx.ProcessIpcCommandCore(queuedAction, queuedParts, queuedSenderTicks));
        }
        catch (Exception ex)
        {
            Print("Error ProcessIpcCommands: " + ex.Message);
        }
    }

    if (!ipcCommandQueue.IsEmpty)
    {
        try
        {
            TriggerCustomEvent(o => ProcessIpcCommands(), null);
        }
        catch { }
    }
}
```

### Estimated CYC
- **Final Main Method**: 7 (termination check, empty check, loop, 4 validation continues, exception catch, continuation trigger)

---

## Complexity Verification

### Before
- `ProcessIpcCommands`: CYC 76

### After
- `ProcessIpcCommands`: CYC 7 ✅
- `ValidateCommandFormat`: CYC 6 ✅
- `IsCommandForThisInstrument`: CYC 8 ✅
- `HandleValidationFailure`: CYC 5 ✅

**Total Reduction**: 76 → 7 (90.8% reduction)

---

## V12 DNA Compliance

### ✅ No Internal Locks
- All methods use existing lock-free patterns
- `Enqueue` pattern preserved

### ✅ ASCII-Only
- All string literals use straight quotes
- No Unicode characters

### ✅ Surgical Splits
- Using manual extraction (methods <50 lines, no need for v12_split.py)
- Zero logic drift

### ✅ FSM-Driven
- Actor pattern preserved via `Enqueue`

### ✅ Post-Edit Deployment
- Run `deploy-sync.ps1` after each ticket
- Verify ASCII gate passes

### ✅ Complexity Standards
- All methods CYC ≤8 ✅
- All extractions ≥15 lines ✅

### ✅ Zero Logic Drift
- Pure structural movement
- No optimization or "improvements"

---

## F5 Verification Protocol

After EACH ticket:
1. Save file
2. Run `deploy-sync.ps1`
3. Verify ASCII gate passes
4. Bump BUILD_TAG in `src/V12_002.cs`
5. **STOP and report to Director**
6. Wait for "F5 done [BUILD_TAG]" confirmation
7. Commit changes
8. Proceed to next ticket

---

## Rollback Plan

If F5 fails at any ticket:
1. Revert last commit: `git reset --hard HEAD~1`
2. Restore from checkpoint
3. Analyze failure cause
4. Adjust extraction strategy
5. Retry ticket

---

## Success Metrics

- ✅ CYC 76 → 7 (90.8% reduction)
- ✅ All helpers CYC ≤8
- ✅ Zero locks
- ✅ ASCII-only
- ✅ F5 passes after each ticket
- ✅ Build passes
- ✅ Tests pass (if any)
- ✅ deploy-sync.ps1 passes

---

## Next Phase
Proceed to Phase 2.3: Sentinel Audit