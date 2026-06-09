---
# TICKET EPIC-CCN-2-04: Extract LogIpcCommandReceived()
# Epic: EPIC-CCN-2
# Sequence: 4 of 5
# Depends on: ticket-03-symbol-matching.md
---

## Objective
Extract the diagnostic logging logic (lines 352-365) from [`ProcessIpcCommands()`](../../../src/V12_002.UI.IPC.cs:260) into a dedicated `LogIpcCommandReceived()` method to achieve CYC ≤ 3.

## Scope
IN scope:
- **File**: [`src/V12_002.UI.IPC.cs`](../../../src/V12_002.UI.IPC.cs)
- **Target Method**: [`ProcessIpcCommands()`](../../../src/V12_002.UI.IPC.cs:260)
- **Extraction Range**: Lines 352-365 (diagnostic logging Print call)
- **New Method**: `LogIpcCommandReceived()` (to be created below `IsCommandForThisChart()`)

OUT of scope:
- Previously extracted methods (unchanged)
- FSM enqueue logic (unchanged)
- Error handling try-catch wrapper (unchanged)

## Context References
- **Analysis**: [`docs/brain/EPIC-CCN-2/01-analysis.md`](01-analysis.md) -- Section "Root Causes of Complexity" (lines 73-76)
- **Approach**: [`docs/brain/EPIC-CCN-2/02-approach.md`](02-approach.md) -- Section "Extraction 4: LogIpcCommandReceived()" (lines 438-481)
- **Incremental Plan**: [`docs/brain/EPIC-CCN-2/02-approach.md`](02-approach.md) -- Section "Step 4: Extract LogIpcCommandReceived()" (lines 625-645)

## Implementation Instructions

### 1. Create New Method Signature
Add this method immediately after `IsCommandForThisChart()`:

```csharp
private void LogIpcCommandReceived(
    string action,
    string target,
    bool isForMe,
    string mySym,
    bool isGlobalCommand
)
{
    // CYC 2 (1 method entry + 1 conditional)
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
}
```

### 2. Update Call Site in ProcessIpcCommands()
Replace the original `Print()` call (lines 352-365) with:

```csharp
LogIpcCommandReceived(action, target, isForMe, mySym, isGlobalCommand);
```

### 3. Preserve Exact Behavior
**CRITICAL**: This is a pure structural refactoring. Do NOT change:
- Log message format (exact string template)
- Conditional `[GLOBAL CMD]` suffix logic
- Parameter order in format string
- Variable names used in format

### 4. Why This Extraction?
**Rationale**:
- Centralizes diagnostic logging for future enhancements (e.g., structured logging)
- Reduces visual clutter in orchestrator method
- Makes logging testable in isolation (future-proofing)
- Achieves CYC ≤ 3 (simple conditional formatting)

**Note**: This is the simplest extraction in the epic (~15 lines), but it completes the separation of concerns:
- Validation → `ValidateIpcCommandSyntax()`
- Classification → `IsGlobalCommand()`
- Routing → `IsCommandForThisChart()`
- Logging → `LogIpcCommandReceived()`
- Orchestration → `ProcessIpcCommands()` (residual)

## V12 DNA Guardrails
- [ ] Zero new lock() statements
- [ ] Zero non-ASCII characters in string literals
- [ ] Method >= 15 LOC (including formatting: ~15 lines)
- [ ] Residual `ProcessIpcCommands()` CYC target: ≤ 8 (down from ~25)
- [ ] New `LogIpcCommandReceived()` CYC target: ≤ 3
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
- [ ] `LogIpcCommandReceived()` method created in [`V12_002.UI.IPC.cs`](../../../src/V12_002.UI.IPC.cs)
- [ ] Method signature matches specification (5 parameters, void return)
- [ ] Log message format preserved exactly (including conditional `[GLOBAL CMD]` suffix)
- [ ] Call site in `ProcessIpcCommands()` updated to single-line method call
- [ ] `deploy-sync.ps1` ASCII gate: PASS
- [ ] `complexity_audit.py` shows:
  - `ProcessIpcCommands()`: CYC ≤ 8 (FINAL TARGET ACHIEVED)
  - `LogIpcCommandReceived()`: CYC ≤ 3
- [ ] `lock()` audit: ZERO matches
- [ ] Compile: `dotnet build` succeeds
- [ ] Director presses F5 in NinjaTrader -- BUILD_TAG banner visible
- [ ] Test: Send IPC command, verify log output format unchanged
- [ ] Test: Send global command, verify `[GLOBAL CMD]` suffix appears
- [ ] Test: Send symbol-specific command, verify no `[GLOBAL CMD]` suffix