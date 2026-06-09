# EPIC-CCN-14 Ticket 03: Extract HandleValidationFailure

## Ticket ID
EPIC-CCN-14-T03

## Status
BLOCKED (Awaiting T02 F5 verification)

## Objective
Extract validation failure handling logic from `ProcessIpcCommands` into a dedicated helper method.

## Target File
`src/V12_002.UI.IPC.cs`

## Complexity Impact
- **Before**: ProcessIpcCommands CYC 35 (after T02)
- **After**: ProcessIpcCommands CYC 30 (-5), HandleValidationFailure CYC 5
- **Progress**: 61% toward target

## Implementation Steps

### Step 1: Insert New Method (After IsCommandForThisInstrument, ~Line 355)
Insert immediately after `IsCommandForThisInstrument`:

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

### Step 2: Update Caller (Lines ~307-327)
Replace lines 307-327 with:

```csharp
// EPIC-4 Ticket 03: IPC Hardening validation (rate limiting, circuit breakers, anomaly detection)
ValidationResult validationResult = ValidateIpcCommand(action, parts);
if (validationResult != ValidationResult.Valid)
{
    HandleValidationFailure(validationResult, action);
    continue;
}
```

**Note**: Lines 309-327 are REMOVED (switch statement moved to helper)

## V12 DNA Compliance

### ✅ No Internal Locks
- Uses `Interlocked.Increment` (atomic operation)
- No locks in caller

### ✅ ASCII-Only
- All string literals use straight quotes
- No Unicode characters

### ✅ Zero Logic Drift
- Pure copy-paste extraction
- No optimization or changes

### ✅ Surgical Split
- Method <50 lines (21 lines)
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
public const string BUILD_TAG = "1103C";  // Increment from T02
```

### 4. STOP for F5 Verification
**CRITICAL**: Do NOT proceed until Director confirms "F5 done [BUILD_TAG]"

### 5. Commit Changes
```powershell
git add src/V12_002.UI.IPC.cs src/V12_002.cs
git commit -m "feat(epic-ccn-14-t03): extract HandleValidationFailure (CYC 35->30)"
```

## Expected Outcome
- ✅ New method `HandleValidationFailure` after IsCommandForThisInstrument
- ✅ Caller simplified (lines 307-327 → 307-313)
- ✅ CYC reduced by 5
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
EPIC-CCN-14-T04: Verify Final Complexity

---

**Ready for Execution**: Awaiting T02 F5 verification.