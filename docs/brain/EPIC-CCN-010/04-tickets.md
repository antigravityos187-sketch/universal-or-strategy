# Extraction Tickets: EPIC-CCN-010

## Overview
- **Total Tickets**: 4
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3 → TICKET-4)
- **Estimated Effort**: 2-3 hours
- **Target File**: src/V12_002.UI.Panel.Handlers.cs
- **Target Method**: ShowModeSpecificControls
- **Current Complexity**: CYC=20
- **Target Complexity**: CYC<=5 (orchestrator), CYC<=8 (helpers)

---

## TICKET-1: Extract ValidateModeState Helper

### Scope
- **Current Method**: `ShowModeSpecificControls`
- **Current CYC**: 20
- **Target CYC**: <=5 (validation logic only)
- **Extraction**: Mode state validation logic into private helper method

### Implementation
1. Identify mode state validation conditionals in ShowModeSpecificControls
2. Extract validation logic into new private method: `private bool ValidateModeState()`
3. Method returns true if mode state is valid for UI updates; otherwise, false
4. Replace original validation code with call to ValidateModeState()
5. Preserve exact validation logic (no behavior changes)
6. Run CSharpier formatting
7. Verify CYC<=5 using complexity_audit.py

### Method Signature
```csharp
/// <summary>
/// Validates current trading mode state for UI updates.
/// </summary>
/// <returns>True if mode state is valid for UI updates; otherwise, false.</returns>
private bool ValidateModeState()
{
    // Extracted validation logic
}
```

### Acceptance Criteria
- [ ] ValidateModeState() method created with CYC<=5
- [ ] Original validation logic preserved exactly
- [ ] ShowModeSpecificControls calls ValidateModeState()
- [ ] No behavioral changes (same UI behavior)
- [ ] CSharpier formatting compliant
- [ ] Build succeeds (dotnet build)
- [ ] All tests pass (dotnet test)
- [ ] Complexity audit passes (CYC<=5)

### Dependencies
- None (first ticket)

### Verification Commands
```powershell
# Format check
dotnet csharpier check src/V12_002.UI.Panel.Handlers.cs

# Complexity check
python scripts/complexity_audit.py

# Build
dotnet build

# Tests
dotnet test
```

---

## TICKET-2: Extract UpdateControlGroupVisibility Helper

### Scope
- **Current Method**: `ShowModeSpecificControls`
- **Current CYC**: ~15 (after TICKET-1)
- **Target CYC**: <=8 (panel visibility logic only)
- **Extraction**: Control group visibility logic into private helper method

### Implementation
1. Identify panel/section visibility conditionals in ShowModeSpecificControls
2. Extract visibility logic into new private method: `private void UpdateControlGroupVisibility(TradingMode mode)`
3. Method accepts TradingMode parameter for mode-specific visibility
4. Replace original visibility code with call to UpdateControlGroupVisibility(mode)
5. Preserve exact visibility logic (no behavior changes)
6. Run CSharpier formatting
7. Verify CYC<=8 using complexity_audit.py

### Method Signature
```csharp
/// <summary>
/// Updates visibility of control groups based on trading mode.
/// </summary>
/// <param name="mode">The current trading mode.</param>
private void UpdateControlGroupVisibility(TradingMode mode)
{
    // Extracted visibility logic
}
```

### Acceptance Criteria
- [ ] UpdateControlGroupVisibility() method created with CYC<=8
- [ ] Original visibility logic preserved exactly
- [ ] ShowModeSpecificControls calls UpdateControlGroupVisibility(mode)
- [ ] No behavioral changes (same UI visibility)
- [ ] CSharpier formatting compliant
- [ ] Build succeeds (dotnet build)
- [ ] All tests pass (dotnet test)
- [ ] Complexity audit passes (CYC<=8)

### Dependencies
- TICKET-1 must be completed first

### Verification Commands
```powershell
# Format check
dotnet csharpier check src/V12_002.UI.Panel.Handlers.cs

# Complexity check
python scripts/complexity_audit.py

# Build
dotnet build

# Tests
dotnet test
```

---

## TICKET-3: Extract ApplyModeSpecificSettings Helper

### Scope
- **Current Method**: `ShowModeSpecificControls`
- **Current CYC**: ~10 (after TICKET-1 and TICKET-2)
- **Target CYC**: <=7 (control state logic only)
- **Extraction**: Mode-specific control state logic into private helper method

### Implementation
1. Identify control state update conditionals in ShowModeSpecificControls
2. Extract state logic into new private method: `private void ApplyModeSpecificSettings(TradingMode mode)`
3. Method accepts TradingMode parameter for mode-specific settings
4. Replace original state code with call to ApplyModeSpecificSettings(mode)
5. Preserve exact state logic (no behavior changes)
6. Run CSharpier formatting
7. Verify CYC<=7 using complexity_audit.py

### Method Signature
```csharp
/// <summary>
/// Applies mode-specific control states and properties.
/// </summary>
/// <param name="mode">The current trading mode.</param>
private void ApplyModeSpecificSettings(TradingMode mode)
{
    // Extracted settings logic
}
```

### Acceptance Criteria
- [ ] ApplyModeSpecificSettings() method created with CYC<=7
- [ ] Original settings logic preserved exactly
- [ ] ShowModeSpecificControls calls ApplyModeSpecificSettings(mode)
- [ ] No behavioral changes (same control states)
- [ ] CSharpier formatting compliant
- [ ] Build succeeds (dotnet build)
- [ ] All tests pass (dotnet test)
- [ ] Complexity audit passes (CYC<=7)

### Dependencies
- TICKET-1 must be completed first
- TICKET-2 must be completed first

### Verification Commands
```powershell
# Format check
dotnet csharpier check src/V12_002.UI.Panel.Handlers.cs

# Complexity check
python scripts/complexity_audit.py

# Build
dotnet build

# Tests
dotnet test
```

---

## TICKET-4: Refactor ShowModeSpecificControls to Orchestrator

### Scope
- **Current Method**: `ShowModeSpecificControls`
- **Current CYC**: ~5 (after TICKET-1, TICKET-2, TICKET-3)
- **Target CYC**: <=5 (orchestration only)
- **Refactor**: Simplify to orchestrator pattern calling extracted helpers

### Implementation
1. Verify all helper methods are extracted (TICKET-1, TICKET-2, TICKET-3)
2. Refactor ShowModeSpecificControls to orchestration pattern:
   - Call ValidateModeState() for precondition check
   - If valid, get current mode via GetCurrentMode()
   - Call UpdateControlGroupVisibility(mode)
   - Call ApplyModeSpecificSettings(mode)
3. Remove all extracted logic (should be in helpers)
4. Preserve exact execution order and logic flow
5. Run CSharpier formatting
6. Verify CYC<=5 using complexity_audit.py

### Expected Orchestrator Pattern
```csharp
/// <summary>
/// Shows mode-specific controls by orchestrating validation and updates.
/// </summary>
private void ShowModeSpecificControls()
{
    // Validate preconditions
    if (!ValidateModeState())
    {
        return;
    }

    // Get current mode
    TradingMode mode = GetCurrentMode();

    // Update UI visibility
    UpdateControlGroupVisibility(mode);

    // Apply mode-specific settings
    ApplyModeSpecificSettings(mode);
}
```

### Acceptance Criteria
- [ ] ShowModeSpecificControls reduced to CYC<=5
- [ ] Orchestration pattern implemented correctly
- [ ] All helper methods called in correct order
- [ ] No behavioral changes (same UI behavior)
- [ ] CSharpier formatting compliant
- [ ] Build succeeds (dotnet build)
- [ ] All tests pass (dotnet test)
- [ ] Complexity audit passes (CYC<=5)
- [ ] Manual testing in NinjaTrader UI confirms no regressions

### Dependencies
- TICKET-1 must be completed first
- TICKET-2 must be completed first
- TICKET-3 must be completed first

### Verification Commands
```powershell
# Format check
dotnet csharpier check src/V12_002.UI.Panel.Handlers.cs

# Complexity check (verify CYC<=5 for ShowModeSpecificControls)
python scripts/complexity_audit.py

# Build
dotnet build

# Tests
dotnet test

# Full pre-push validation
powershell -File .\scripts\pre_push_validation.ps1 -Fast
```

---

## Final Validation (After All Tickets)

### Pre-Push Validation (13 Checks)
Run full validation suite before pushing:
```powershell
powershell -File .\scripts\pre_push_validation.ps1
```

**Expected Results**:
- ✅ Check #1: ASCII-Only (zero non-ASCII)
- ✅ Check #2: Build (zero errors)
- ✅ Check #3: Unit Tests (100% pass)
- ✅ Check #4: Lint (zero violations)
- ✅ Check #5: Formatting (CSharpier compliant)
- ✅ Check #9: Complexity (CYC<=8 per method)

### Hard-Link Sync
After all tickets complete, sync NinjaTrader hard links:
```powershell
powershell -File .\deploy-sync.ps1
```

### Manual Testing
1. Open NinjaTrader
2. Load V12_002 strategy
3. Test mode transitions:
   - Switch to Scalp mode → verify UI updates
   - Switch to Swing mode → verify UI updates
   - Switch to Hybrid mode → verify UI updates
4. Verify no UI regressions or errors

---

## Success Metrics

### Complexity Reduction
- **Before**: ShowModeSpecificControls CYC=20
- **After**: 
  - ShowModeSpecificControls CYC<=5 (75% reduction)
  - ValidateModeState CYC<=5
  - UpdateControlGroupVisibility CYC<=8
  - ApplyModeSpecificSettings CYC<=7

### V12 DNA Compliance
- ✅ Zero lock() statements
- ✅ FSM/Actor pattern preserved
- ✅ ASCII-only compliance
- ✅ Correctness by construction

### Jane Street Alignment
- ✅ Cognitive simplicity (CYC<=8 per method)
- ✅ Single responsibility per method
- ✅ Independent testability
- ✅ No performance degradation

### PR Hygiene
- ✅ Diff size: ~2,500 chars (well under 10k limit)
- ✅ Scope: 1 file, 1 method (surgical)
- ✅ No breaking changes (pure refactoring)

---

## Execution Notes

### Agent Selection
- **Primary**: Bob CLI (`v12-engineer`) - Full extraction + orchestration
- **Secondary**: Codex CLI (`codex-rescue`) - If Bob delegates logic hardening

### Mode
- **Advanced mode** (`advanced`) - Code modification with MCP tools

### Checkpointing
- Enabled via `.bob/settings.json`
- Restore via `/restore` if needed

### Estimated Timeline
- TICKET-1: 30 minutes (validation extraction)
- TICKET-2: 45 minutes (visibility extraction)
- TICKET-3: 45 minutes (settings extraction)
- TICKET-4: 30 minutes (orchestrator refactor)
- **Total**: 2.5 hours

### Bobcoin Budget
- Implementation: ~0.50-0.75
- Testing: ~0.25
- **Total**: ~0.75-1.00

---

*Generated by Bob Shell (v12-engineer) - Phase 4 Ticket Generation*
*Date: 2026-06-15T16:48:00Z*
*Protocol Version: V12.23*
