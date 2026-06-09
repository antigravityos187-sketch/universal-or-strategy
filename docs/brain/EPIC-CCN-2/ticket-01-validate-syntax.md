---
# TICKET EPIC-CCN-2-01: Extract ValidateIpcCommandSyntax()
# Epic: EPIC-CCN-2
# Sequence: 1 of 5
# Depends on: NONE
---

## Objective
Extract the validation cascade (lines 280-310) from [`ProcessIpcCommands()`](../../../src/V12_002.UI.IPC.cs:260) into a dedicated `ValidateIpcCommandSyntax()` method to reduce complexity from CYC 76 to ~60.

## Scope
IN scope:
- **File**: [`src/V12_002.UI.IPC.cs`](../../../src/V12_002.UI.IPC.cs)
- **Target Method**: [`ProcessIpcCommands()`](../../../src/V12_002.UI.IPC.cs:260) (lines 260-430)
- **Extraction Range**: Lines 280-310 (validation cascade)
- **New Method**: `ValidateIpcCommandSyntax()` (to be created below `ProcessIpcCommands()`)

OUT of scope:
- Other methods in the file
- Call sites of `ProcessIpcCommands()` (unchanged)
- IPC hardening infrastructure (already extracted)

## Context References
- **Analysis**: [`docs/brain/EPIC-CCN-2/01-analysis.md`](01-analysis.md) -- Section "Root Causes of Complexity" (lines 60-81)
- **Approach**: [`docs/brain/EPIC-CCN-2/02-approach.md`](02-approach.md) -- Section "Extraction 1: ValidateIpcCommandSyntax()" (lines 214-261)
- **Incremental Plan**: [`docs/brain/EPIC-CCN-2/02-approach.md`](02-approach.md) -- Section "Step 1: Extract ValidateIpcCommandSyntax()" (lines 560-581)

## Implementation Instructions

### 1. Create New Method Signature
Add this method immediately after `ProcessIpcCommands()` (around line 431):

```csharp
private bool ValidateIpcCommandSyntax(
    string command,
    out string action,
    out string[] parts,
    out long senderTicks,
    out string rejectReason
)
```

### 2. Extract Validation Logic
Move the following validation blocks from `ProcessIpcCommands()` into the new method:

**Validation Sequence** (preserve exact order):
1. Malformed/oversize command check
2. Split command into parts
3. Empty action check
4. Timestamp extraction loop
5. Metadata guard validation (`MetadataGuardCommandTimestamp`)
6. IPC hardening validation (`ValidateIpcCommand` 5-way switch)
7. Allowlist validation (`IsAllowedIpcAction`)

**Return Logic**:
- Return `false` on any validation failure, setting `rejectReason` to descriptive message
- Return `true` on success, with all `out` parameters populated

### 3. Update Call Site in ProcessIpcCommands()
Replace the original validation block (lines 280-310) with:

```csharp
if (!ValidateIpcCommandSyntax(command, out string action, out string[] parts, 
    out long senderTicks, out string rejectReason))
{
    Print($"V12 IPC REJECT: {rejectReason}");
    continue;
}
```

### 4. Preserve Exact Behavior
**CRITICAL**: This is a pure structural refactoring. Do NOT change:
- Validation order or logic
- Error messages (except consolidating into `rejectReason`)
- Early-exit semantics (`continue` in loop)
- Variable names used downstream

## V12 DNA Guardrails
- [ ] Zero new lock() statements
- [ ] Zero non-ASCII characters in string literals
- [ ] Extracted method >= 15 LOC (validation cascade is ~35 lines)
- [ ] Residual `ProcessIpcCommands()` CYC target: ~60 (down from 76)
- [ ] New `ValidateIpcCommandSyntax()` CYC target: ≤ 8
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
- [ ] `ValidateIpcCommandSyntax()` method created in [`V12_002.UI.IPC.cs`](../../../src/V12_002.UI.IPC.cs)
- [ ] Method signature matches specification (5 parameters: 1 in, 4 out)
- [ ] All 7 validation checks moved into new method in correct order
- [ ] Call site in `ProcessIpcCommands()` updated to use new method
- [ ] `deploy-sync.ps1` ASCII gate: PASS
- [ ] `complexity_audit.py` shows:
  - `ProcessIpcCommands()`: CYC reduced (target ~60)
  - `ValidateIpcCommandSyntax()`: CYC ≤ 8
- [ ] `lock()` audit: ZERO matches
- [ ] Compile: `dotnet build` succeeds
- [ ] Director presses F5 in NinjaTrader -- BUILD_TAG banner visible
- [ ] Test: Send malformed IPC command, verify rejection message unchanged