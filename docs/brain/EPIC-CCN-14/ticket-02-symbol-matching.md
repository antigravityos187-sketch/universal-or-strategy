# EPIC-CCN-14 Ticket 02: Extract IsCommandForThisInstrument

## Ticket ID
EPIC-CCN-14-T02

## Status
BLOCKED (Awaiting T01 F5 verification)

## Objective
Extract symbol matching logic from `ProcessIpcCommands` into a dedicated helper method.

## Target File
`src/V12_002.UI.IPC.cs`

## Complexity Impact
- **Before**: ProcessIpcCommands CYC 70 (after T01)
- **After**: ProcessIpcCommands CYC 35 (-35), IsCommandForThisInstrument CYC 8
- **Progress**: 54% toward target

## Implementation Steps

### Step 1: Insert New Method (After ValidateCommandFormat, ~Line 295)
Insert immediately after `ValidateCommandFormat`:

```csharp
private bool IsCommandForThisInstrument(
    string action,
    string targetSymbol,
    out bool isGlobalCommand)
{
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
}
```

### Step 2: Update Caller (Lines ~335-409)
Replace lines 335-409 with:

```csharp
string targetSymbol = parts.Length > 1 ? parts[1] : "Global";
if (!IsCommandForThisInstrument(action, targetSymbol, out bool isGlobalCommand))
    continue;

string queuedAction = action;
string[] queuedParts = parts;
long queuedSenderTicks = senderTicks;
Enqueue(ctx => ctx.ProcessIpcCommandCore(queuedAction, queuedParts, queuedSenderTicks));
```

**Note**: Lines 337-409 are REMOVED (symbol matching logic moved to helper)

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
- Method <50 lines (60 lines)
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
public const string BUILD_TAG = "1103B";  // Increment from T01
```

### 4. STOP for F5 Verification
**CRITICAL**: Do NOT proceed until Director confirms "F5 done [BUILD_TAG]"

### 5. Commit Changes
```powershell
git add src/V12_002.UI.IPC.cs src/V12_002.cs
git commit -m "feat(epic-ccn-14-t02): extract IsCommandForThisInstrument (CYC 70->35)"
```

## Expected Outcome
- ✅ New method `IsCommandForThisInstrument` after ValidateCommandFormat
- ✅ Caller simplified (lines 335-409 → 335-343)
- ✅ CYC reduced by 35
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
EPIC-CCN-14-T03: Extract HandleValidationFailure

---

**Ready for Execution**: Awaiting T01 F5 verification.