---
# TICKET EPIC-CCN-2-02: Extract IsGlobalCommand()
# Epic: EPIC-CCN-2
# Sequence: 2 of 5
# Depends on: ticket-01-validate-syntax.md
---

## Objective
Extract the global command classification logic (lines 312-330) from [`ProcessIpcCommands()`](../../../src/V12_002.UI.IPC.cs:260) into a dedicated `IsGlobalCommand()` method using local functions to achieve CYC 4.

## Scope
IN scope:
- **File**: [`src/V12_002.UI.IPC.cs`](../../../src/V12_002.UI.IPC.cs)
- **Target Method**: [`ProcessIpcCommands()`](../../../src/V12_002.UI.IPC.cs:260)
- **Extraction Range**: Lines 312-330 (global command boolean expression)
- **New Method**: `IsGlobalCommand()` (to be created below `ValidateIpcCommandSyntax()`)

OUT of scope:
- Previously extracted `ValidateIpcCommandSyntax()` (unchanged)
- Symbol matching logic (handled in ticket-03)
- Logging logic (handled in ticket-04)

## Context References
- **Analysis**: [`docs/brain/EPIC-CCN-2/01-analysis.md`](01-analysis.md) -- Section "Risk Hotspots" (lines 90-99)
- **Approach**: [`docs/brain/EPIC-CCN-2/02-approach.md`](02-approach.md) -- Section "Extraction 2: IsGlobalCommand()" (lines 262-335)
- **Incremental Plan**: [`docs/brain/EPIC-CCN-2/02-approach.md`](02-approach.md) -- Section "Step 2: Extract IsGlobalCommand()" (lines 582-602)

## Implementation Instructions

### 1. Create New Method with Local Functions
Add this method immediately after `ValidateIpcCommandSyntax()`:

```csharp
private bool IsGlobalCommand(string action)
{
    // CYC 4 (3 OR conditions + 1 method entry)
    return IsFleetCommand(action)
        || IsModeCommand(action)
        || IsTargetCommand(action);
    
    // Local functions (not extracted as separate methods - avoid 15-LOC floor)
    bool IsFleetCommand(string a) => // CYC 8
        a == "TOGGLE_ACCOUNT" || a == "GET_FLEET" || a == "DIAG_FLEET"
        || a == "REQUEST_FLEET_STATE" || a == "CANCEL_ALL" || a == "FLATTEN"
        || a == "SYNC_ALL" || a == "MKT_SYNC";
    
    bool IsModeCommand(string a) => // CYC 5
        a == "SET_SIMA" || a == "SET_TARGETS" || a == "SET_TRAIL"
        || a == "SET_CIT" || a == "BE_CUSTOM";
    
    bool IsTargetCommand(string a) => // CYC 4
        a.StartsWith("MOVE_TARGET") || a == "LOCK_50"
        || a == "RESET_MEMORY" || a == "DIAG_IPC";
}
```

### 2. Logical Grouping Rationale
**Fleet Commands**: Account/fleet-level operations (8 commands)
- TOGGLE_ACCOUNT, GET_FLEET, DIAG_FLEET, REQUEST_FLEET_STATE
- CANCEL_ALL, FLATTEN, SYNC_ALL, MKT_SYNC

**Mode Commands**: Strategy behavior toggles (5 commands)
- SET_SIMA, SET_TARGETS, SET_TRAIL, SET_CIT, BE_CUSTOM

**Target Commands**: Position/diagnostic operations (4 commands)
- MOVE_TARGET* (prefix match), LOCK_50, RESET_MEMORY, DIAG_IPC

### 3. Update Call Site in ProcessIpcCommands()
Replace the original boolean expression (lines 312-330) with:

```csharp
bool isGlobalCommand = IsGlobalCommand(action);
```

### 4. Preserve Exact Behavior
**CRITICAL**: This is a pure structural refactoring. Do NOT change:
- Command string comparisons (case-sensitive)
- `StartsWith()` logic for MOVE_TARGET variants
- Boolean OR semantics
- Variable name `isGlobalCommand` used downstream

### 5. Local Functions Strategy
**Why Local Functions?**
- Avoids 15-LOC extraction floor violation (local functions don't count as separate methods)
- Each logical group has CYC ≤8 (Fleet: 8, Mode: 5, Target: 4)
- Parent method has CYC 4 (3 OR conditions + method entry)
- `complexity_audit.py` will report CYC 4 for `IsGlobalCommand()` (local functions are inlined by analyzer)

## V12 DNA Guardrails
- [ ] Zero new lock() statements
- [ ] Zero non-ASCII characters in string literals
- [ ] Method >= 15 LOC (including local functions: ~25 lines)
- [ ] Residual `ProcessIpcCommands()` CYC target: ~45 (down from ~60)
- [ ] New `IsGlobalCommand()` CYC target: 4 (verified by complexity_audit.py)
- [ ] No logic drift -- pure structural movement only

## Post-Edit Verification (Mandatory)
```powershell
# 1. Re-establish hard links (MANDATORY after every src/ edit)
powershell -File .\deploy-sync.ps1

# 2. Complexity verification
python scripts/complexity_audit.py

# 3. Lock regression (must return ZERO)
grep -r "lock(" src/

# 4. ASCII gate (must return ZERO)
grep -Prn "[^\x00-\x7F]" src/
```

## Acceptance Criteria
- [ ] `IsGlobalCommand()` method created in [`V12_002.UI.IPC.cs`](../../../src/V12_002.UI.IPC.cs)
- [ ] Method uses local functions (IsFleetCommand, IsModeCommand, IsTargetCommand)
- [ ] All 17 global command checks preserved (8 fleet + 5 mode + 4 target)
- [ ] `StartsWith("MOVE_TARGET")` logic preserved for prefix matching
- [ ] Call site in `ProcessIpcCommands()` updated to single-line method call
- [ ] `deploy-sync.ps1` ASCII gate: PASS
- [ ] `complexity_audit.py` shows:
  - `ProcessIpcCommands()`: CYC reduced (target ~45)
  - `IsGlobalCommand()`: CYC = 4
- [ ] `lock()` audit: ZERO matches
- [ ] Compile: `dotnet build` succeeds
- [ ] Director presses F5 in NinjaTrader -- BUILD_TAG banner visible
- [ ] Test: Send TOGGLE_ACCOUNT command, verify global classification works
- [ ] Test: Send FLATTEN command, verify it reaches all chart instances