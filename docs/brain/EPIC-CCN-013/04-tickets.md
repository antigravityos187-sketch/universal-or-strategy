# Extraction Tickets: EPIC-CCN-013

## Overview
- **Epic ID**: EPIC-CCN-013
- **Target Method**: UpdatePanelState
- **File**: src/V12_002.UI.Panel.StateSync.cs
- **Current Complexity**: CYC = 16
- **Target Complexity**: CYC ≤ 8
- **Total Tickets**: 3
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3)
- **Estimated Effort**: 2.5 hours (0.5h per ticket + 1h verification)

---

## TICKET-1: Extract UpdatePriceDisplay

### Scope
- **Current Method**: `UpdatePanelState`
- **Current CYC**: 16
- **Target Lines**: 18-28 (price display logic)
- **New Method**: `private void UpdatePriceDisplay()`
- **Expected CYC**: 4
- **Extraction**: Price text formatting and color updates based on market position

### Implementation Steps
1. Create new private method `UpdatePriceDisplay()` below `UpdatePanelState`
2. Move lines 18-28 (price display logic) into new method
3. Add method call `UpdatePriceDisplay();` at line 18 in main method
4. Verify compilation: `dotnet build`
5. Run complexity audit: `python scripts/complexity_audit.py`

### Code Changes
```csharp
// New method to add after UpdatePanelState
private void UpdatePriceDisplay()
{
    // Lines 18-28 from original method
    if (currentPrice != null)
    {
        txtPrice.Text = currentPrice.Value.ToString("F2");
        
        if (currentPosition > 0)
        {
            txtPrice.Foreground = Brushes.Green;
        }
        else if (currentPosition < 0)
        {
            txtPrice.Foreground = Brushes.Red;
        }
        else
        {
            txtPrice.Foreground = Brushes.Gray;
        }
    }
}
```

### Acceptance Criteria
- [ ] New method `UpdatePriceDisplay()` created with CYC = 4
- [ ] Lines 18-28 moved to new method
- [ ] Main method calls `UpdatePriceDisplay();` at correct location
- [ ] Compilation succeeds: `dotnet build` returns 0 errors
- [ ] Complexity reduced: Main method CYC decreases by 4
- [ ] No behavioral changes (UI updates work identically)
- [ ] No lock() statements introduced
- [ ] ASCII-only compliance maintained

### Dependencies
- None (first ticket)

### Verification Commands
```powershell
# Format code
dotnet csharpier format src/

# Build
dotnet build

# Complexity check
python scripts/complexity_audit.py

# Lock-free check
grep -r "lock(" src/V12_002.UI.Panel.StateSync.cs
```

---

## TICKET-2: Extract UpdateModeAndConfig

### Scope
- **Current Method**: `UpdatePanelState`
- **Current CYC**: 12 (after TICKET-1)
- **Target Lines**: 30-44 (mode and config sync)
- **New Method**: `private void UpdateModeAndConfig()`
- **Expected CYC**: 2
- **Extraction**: Mode change detection and configuration revision tracking

### Implementation Steps
1. Create new private method `UpdateModeAndConfig()` below `UpdatePriceDisplay`
2. Move lines 30-44 (mode + config sync logic) into new method
3. Add method call `UpdateModeAndConfig();` at line 30 in main method
4. Verify compilation: `dotnet build`
5. Run complexity audit: `python scripts/complexity_audit.py`

### Code Changes
```csharp
// New method to add after UpdatePriceDisplay
private void UpdateModeAndConfig()
{
    // Lines 30-44 from original method
    if (currentMode != lastMode)
    {
        txtMode.Text = currentMode.ToString();
        lastMode = currentMode;
    }
    
    if (configRevision != lastConfigRevision)
    {
        // Sync config state
        txtConfig.Text = $"Rev {configRevision}";
        lastConfigRevision = configRevision;
    }
}
```

### Acceptance Criteria
- [ ] New method `UpdateModeAndConfig()` created with CYC = 2
- [ ] Lines 30-44 moved to new method
- [ ] Main method calls `UpdateModeAndConfig();` at correct location
- [ ] Compilation succeeds: `dotnet build` returns 0 errors
- [ ] Complexity reduced: Main method CYC decreases by 2
- [ ] No behavioral changes (mode/config sync works identically)
- [ ] No lock() statements introduced
- [ ] ASCII-only compliance maintained

### Dependencies
- **TICKET-1** must be completed first

### Verification Commands
```powershell
# Format code
dotnet csharpier format src/

# Build
dotnet build

# Complexity check
python scripts/complexity_audit.py

# Lock-free check
grep -r "lock(" src/V12_002.UI.Panel.StateSync.cs
```

---

## TICKET-3: Extract UpdateTargetCountWithGuard

### Scope
- **Current Method**: `UpdatePanelState`
- **Current CYC**: 10 (after TICKET-2)
- **Target Lines**: 46-58 (target count with guard)
- **New Method**: `private void UpdateTargetCountWithGuard()`
- **Expected CYC**: 3
- **Extraction**: Target count validation, guard timing, and UI updates

### Implementation Steps
1. Create new private method `UpdateTargetCountWithGuard()` below `UpdateModeAndConfig`
2. Move lines 46-58 (target count logic) into new method
3. Add method call `UpdateTargetCountWithGuard();` at line 46 in main method
4. Verify compilation: `dotnet build`
5. Run complexity audit: `python scripts/complexity_audit.py`
6. Verify final main method CYC ≤ 8

### Code Changes
```csharp
// New method to add after UpdateModeAndConfig
private void UpdateTargetCountWithGuard()
{
    // Lines 46-58 from original method
    if (targetCount > 0)
    {
        if (DateTime.Now - lastClickTime > TimeSpan.FromSeconds(2))
        {
            txtTargetCount.Text = targetCount.ToString();
            txtTargetCount.Foreground = Brushes.Blue;
        }
        else
        {
            txtTargetCount.Text = "GUARD";
            txtTargetCount.Foreground = Brushes.Orange;
        }
    }
}
```

### Acceptance Criteria
- [ ] New method `UpdateTargetCountWithGuard()` created with CYC = 3
- [ ] Lines 46-58 moved to new method
- [ ] Main method calls `UpdateTargetCountWithGuard();` at correct location
- [ ] Compilation succeeds: `dotnet build` returns 0 errors
- [ ] **FINAL GOAL**: Main method `UpdatePanelState` CYC ≤ 8 (target: CYC = 7)
- [ ] Total complexity preserved: 16 = 7 (main) + 4 + 2 + 3
- [ ] No behavioral changes (target count guard works identically)
- [ ] No lock() statements introduced
- [ ] ASCII-only compliance maintained

### Dependencies
- **TICKET-1** must be completed first
- **TICKET-2** must be completed first

### Verification Commands
```powershell
# Format code
dotnet csharpier format src/

# Build
dotnet build

# Complexity check (verify CYC ≤ 8 for UpdatePanelState)
python scripts/complexity_audit.py

# Lock-free check
grep -r "lock(" src/V12_002.UI.Panel.StateSync.cs

# Pre-push validation (fast mode)
powershell -File .\scripts\pre_push_validation.ps1 -Fast
```

---

## Post-Extraction Verification

### Final Complexity Breakdown
- **UpdatePanelState**: CYC = 7 ✅ (target ≤8)
- **UpdatePriceDisplay**: CYC = 4 ✅
- **UpdateModeAndConfig**: CYC = 2 ✅
- **UpdateTargetCountWithGuard**: CYC = 3 ✅
- **Total**: CYC = 16 (preserved, redistributed)

### Integration Testing
1. **Manual Test**: F5 in NinjaTrader
   - Verify panel price updates correctly
   - Verify mode/config sync works
   - Verify target count guard displays correctly
2. **Regression Test**: Run existing integration tests
3. **Hard-Link Sync**: `powershell -File .\deploy-sync.ps1`

### Success Criteria (All Tickets)
- [ ] All 3 helper methods extracted
- [ ] Main method CYC reduced from 16 to ≤8
- [ ] All methods have CYC ≤8 (Jane Street aligned)
- [ ] Zero compilation errors
- [ ] Zero lock() statements (lock-free compliance)
- [ ] Zero Unicode characters (ASCII-only compliance)
- [ ] All existing tests pass
- [ ] NinjaTrader hard links synchronized
- [ ] Pre-push validation passes

---

## Execution Notes

### Order of Operations
1. **TICKET-1** (UpdatePriceDisplay) - Independent, can start immediately
2. **TICKET-2** (UpdateModeAndConfig) - Depends on TICKET-1 completion
3. **TICKET-3** (UpdateTargetCountWithGuard) - Depends on TICKET-1 and TICKET-2

### Risk Assessment
- **Risk Level**: LOW
- **Rationale**: 
  - Surgical extractions with clear boundaries
  - No API surface changes (all methods private)
  - No parameter signature changes
  - Behavior preservation guaranteed (line-by-line move)
  - Small diff size (~1,200 chars total)

### Rollback Plan
If any ticket fails:
1. Use Bob Shell `/restore` command to revert to checkpoint
2. Review failure reason
3. Adjust extraction boundaries if needed
4. Retry ticket

---

## Metadata
- **Epic**: EPIC-CCN-013
- **Phase**: 4 (Ticket Generation)
- **Created**: 2026-06-15
- **Status**: READY FOR EXECUTION
- **Total Tickets**: 3
- **Estimated Duration**: 2.5 hours
- **Jane Street Alignment**: ✅ All methods CYC ≤8
- **Lock-Free Compliance**: ✅ Zero locks
- **ASCII-Only Compliance**: ✅ Zero Unicode
- **PR Hygiene**: ✅ Diff <10,000 chars
