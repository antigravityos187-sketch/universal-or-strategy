# EPIC-CCN-14 Ticket 01: Extract ValidateCommandFormat

## Ticket ID
EPIC-CCN-14-T01

## Status
READY FOR EXECUTION

## Objective
Extract command validation logic from `ProcessIpcCommands` into a dedicated helper method.

## Target File
`src/V12_002.UI.IPC.cs`

## Complexity Impact
- **Before**: ProcessIpcCommands CYC 76
- **After**: ProcessIpcCommands CYC 70 (-6), ValidateCommandFormat CYC 6
- **Progress**: 8% toward target

## Implementation Steps

### Step 1: Insert New Method (Line 259)
Insert immediately before `ProcessIpcCommands`:

```csharp
private bool ValidateCommandFormat(
    string command,
    out string[] parts,
    out long senderTicks)
{
    parts = null;
    senderTicks = 0;
    
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
}
```

### Step 2: Update Caller (Lines 279-304)
Replace lines 281-304 with:

```csharp
try
{
    if (!ValidateCommandFormat(command, out string[] parts, out long senderTicks))
        continue;
    
    string action = parts[0].Trim().ToUpperInvariant();
    
    // EPIC-4 Ticket 03: IPC Hardening validation (rate limiting, circuit breakers, anomaly detection)
    ValidationResult validationResult = ValidateIpcCommand(action, parts);
```

**Note**: Lines 281-304 are REMOVED (validation logic moved to helper)

## V12 DNA Compliance

### ✅ No Internal Locks
- No locks in extracted method
- No locks in caller

### ✅ ASCII-Only
- All string literals use straight quotes
- No Unicode characters

### ✅ Zero Logic Drift
- Pure copy-paste extraction
- No optimization or changes

### ✅ Surgical Split
- Method <50 lines (37 lines)
- No need for v12_split.py

## Verification Steps

### 1. Save File
Save `src/V12_002.UI.IPC.cs`

### 2. Run Deploy-Sync
```powershell
powershell -File .\deploy-sync.ps1
```
**Expected**: ASCII gate PASS

### 3. Bump BUILD_TAG
Edit `src/V12_002.cs`:
```csharp
public const string BUILD_TAG = "1103A";  // Increment from current
```

### 4. STOP for F5 Verification
**CRITICAL**: Do NOT proceed until Director confirms "F5 done [BUILD_TAG]"

### 5. Commit Changes
```powershell
git add src/V12_002.UI.IPC.cs src/V12_002.cs
git commit -m "feat(epic-ccn-14-t01): extract ValidateCommandFormat (CYC 76->70)"
```

## Expected Outcome
- ✅ New method `ValidateCommandFormat` at line 259
- ✅ Caller simplified (lines 279-304 → 281-288)
- ✅ CYC reduced by 6
- ✅ Zero logic changes
- ✅ F5 verification passes
- ✅ Build passes
- ✅ deploy-sync.ps1 passes

## Rollback Plan
If F5 fails:
```powershell
git reset --hard HEAD~1
```

## Next Ticket
EPIC-CCN-14-T02: Extract IsCommandForThisInstrument

---

**Ready for Execution**: All checks passed. Awaiting F5 verification after implementation.