# Extraction Tickets: EPIC-CCN-045

## Overview
- **Total Tickets**: 1
- **Execution Order**: Single ticket (atomic extraction)
- **Estimated Effort**: 0.5 hours
- **Epic**: OnKeyDown Modifier Key Routing Extraction
- **Target File**: `src/V12_002.UI.Callbacks.cs`

---

## TICKET-1: Extract TryHandleModifierAction Method

### Scope
- **Current Method**: `OnKeyDown`
- **Current CYC**: 9
- **Target CYC**: 4 (OnKeyDown) + 7 (TryHandleModifierAction) = Both ≤8 ✅
- **Extraction**: Modifier key routing logic (T1/T2/Runner hotkey handling)
- **Lines Affected**: 391-427 (37 lines total)
- **Strategy**: Extract Method refactoring

### Current Code Structure
```csharp
OnKeyDown (CYC 9)
├─> Dictionary lookup for basic hotkeys
└─> Inline modifier key checks (T1/T2/Runner)
    ├─> if (Keyboard.IsKeyDown(Key.D1) || ...) HandleTargetAction("T1", key)
    ├─> if (Keyboard.IsKeyDown(Key.D2) || ...) HandleTargetAction("T2", key)
    └─> if (Keyboard.IsKeyDown(Key.D3) || ...) HandleRunnerAction(key)
```

### Target Code Structure
```csharp
OnKeyDown (CYC 4)
├─> Dictionary lookup for basic hotkeys
└─> TryHandleModifierAction(key) [NEW METHOD]

TryHandleModifierAction (CYC 7)
├─> if (Keyboard.IsKeyDown(Key.D1) || ...) HandleTargetAction("T1", key)
├─> if (Keyboard.IsKeyDown(Key.D2) || ...) HandleTargetAction("T2", key)
└─> if (Keyboard.IsKeyDown(Key.D3) || ...) HandleRunnerAction(key)
```

### Implementation Steps

#### Step 1: Create TryHandleModifierAction Method
Insert new method after `OnKeyDown` (before `HandleTargetAction` at line ~439):

```csharp
// [EPIC-CCN-045] Modifier key routing (CYC 7) - Extracts T1/T2/Runner hotkey logic
private bool TryHandleModifierAction(Key key)
{
    // T1 Actions (1 + letter)
    if (Keyboard.IsKeyDown(Key.D1) || Keyboard.IsKeyDown(Key.NumPad1))
    {
        HandleTargetAction("T1", key);
        return true;
    }

    // T2 Actions (2 + letter)
    if (Keyboard.IsKeyDown(Key.D2) || Keyboard.IsKeyDown(Key.NumPad2))
    {
        HandleTargetAction("T2", key);
        return true;
    }

    // Runner Actions (3 + letter)
    if (Keyboard.IsKeyDown(Key.D3) || Keyboard.IsKeyDown(Key.NumPad3))
    {
        HandleRunnerAction(key);
        return true;
    }

    return false;
}
```

**Complexity Calculation**:
- Base: 1
- if (D1 || NumPad1): +2 (if + OR)
- if (D2 || NumPad2): +2 (if + OR)
- if (D3 || NumPad3): +2 (if + OR)
- **Total**: 7 ✅

#### Step 2: Refactor OnKeyDown
Replace lines 402-433 (modifier key blocks) with single call to `TryHandleModifierAction`:

**Before** (lines 391-427):
```csharp
private void OnKeyDown(object sender, KeyEventArgs e)
{
    // Basic hotkeys (no modifiers) - O(1) dictionary lookup
    if (_keyCommands != null && _keyCommands.TryGetValue(e.Key, out var cmd))
    {
        cmd();
        e.Handled = true;
        return;
    }

    // T1 Actions (1 + letter)
    if (Keyboard.IsKeyDown(Key.D1) || Keyboard.IsKeyDown(Key.NumPad1))
    {
        HandleTargetAction("T1", e.Key);
        e.Handled = true;
        return;
    }

    // T2 Actions (2 + letter)
    if (Keyboard.IsKeyDown(Key.D2) || Keyboard.IsKeyDown(Key.NumPad2))
    {
        HandleTargetAction("T2", e.Key);
        e.Handled = true;
        return;
    }

    // Runner Actions (3 + letter)
    if (Keyboard.IsKeyDown(Key.D3) || Keyboard.IsKeyDown(Key.NumPad3))
    {
        HandleRunnerAction(e.Key);
        e.Handled = true;
        return;
    }

    // RMA uses Shift+Click (R conflicts with NT search, Ctrl conflicts with chart drag)
}
```

**After**:
```csharp
private void OnKeyDown(object sender, KeyEventArgs e)
{
    // Basic hotkeys (no modifiers) - O(1) dictionary lookup
    if (_keyCommands != null && _keyCommands.TryGetValue(e.Key, out var cmd))
    {
        cmd();
        e.Handled = true;
        return;
    }

    // Modifier key actions (T1/T2/Runner)
    if (TryHandleModifierAction(e.Key))
    {
        e.Handled = true;
        return;
    }

    // RMA uses Shift+Click (R conflicts with NT search, Ctrl conflicts with chart drag)
}
```

**Complexity Calculation**:
- Base: 1
- if (_keyCommands != null && ...): +2 (if + AND)
- if (TryHandleModifierAction(...)): +1
- **Total**: 4 ✅

#### Step 3: Verify Complexity
```bash
python3 scripts/complexity_audit.py
```

**Expected Output**:
- `OnKeyDown`: CYC 4 ✅
- `TryHandleModifierAction`: CYC 7 ✅

#### Step 4: Build & Sync
```bash
dotnet build
powershell -File .\deploy-sync.ps1
```

#### Step 5: Manual Verification
1. F5 in NinjaTrader
2. Test hotkeys:
   - Basic hotkeys (no modifiers): e.g., press 'P' for pause
   - T1 actions: Hold '1' + press letter (e.g., 1+A)
   - T2 actions: Hold '2' + press letter (e.g., 2+B)
   - Runner actions: Hold '3' + press letter (e.g., 3+C)

### Acceptance Criteria
- [ ] `TryHandleModifierAction` method created with CYC 7
- [ ] `OnKeyDown` refactored to CYC 4
- [ ] Both methods ≤8 (Jane Street strict standard)
- [ ] All hotkeys function identically (no behavioral changes)
- [ ] `python3 scripts/complexity_audit.py` confirms CYC ≤8
- [ ] `dotnet build` succeeds (zero errors)
- [ ] `deploy-sync.ps1` succeeds (hard-link sync)
- [ ] F5 in NinjaTrader loads without errors
- [ ] Manual hotkey testing passes (basic, T1, T2, Runner)
- [ ] No lock() statements introduced (verified via grep)
- [ ] ASCII-only compliance maintained (no Unicode)
- [ ] Diff size <10,000 characters (~450 chars expected)

### DNA Compliance Checklist
- [ ] **Correctness by Construction**: Type safety maintained (Key enum)
- [ ] **Lock-Free Actor Pattern**: Zero lock() blocks (UI thread serialization)
- [ ] **ASCII-Only**: No Unicode characters in code or comments
- [ ] **Jane Street Alignment**: Cognitive simplicity (CYC ≤8 both methods)

### Dependencies
- None (first and only ticket)

### Estimated Effort
- **Implementation**: 15 minutes
- **Testing**: 15 minutes
- **Total**: 0.5 hours

### Risk Level
- **LOW**: Pure Extract Method refactoring
- **No breaking changes**: Private method, no signature changes
- **No state mutations**: Pure dispatch logic
- **Rollback**: Simple (revert single commit)

---

## Execution Notes

### Tools to Use
- **Preferred**: `apply_diff` or `search_and_replace` (surgical precision)
- **Fallback**: `write_to_file` (if diff tools fail)

### Verification Commands
```bash
# Complexity audit
python3 scripts/complexity_audit.py

# Build
dotnet build

# Hard-link sync
powershell -File .\deploy-sync.ps1

# Lock-free verification
grep -r "lock(" src/V12_002.UI.Callbacks.cs
# Expected: No matches

# ASCII-only verification
grep -P '[^\x00-\x7F]' src/V12_002.UI.Callbacks.cs
# Expected: No matches
```

### Success Metrics
- **Complexity Reduction**: CYC 9 → CYC 4 (55% improvement in OnKeyDown)
- **Maintainability**: Single responsibility per method
- **Testability**: 11 isolated test paths (4 + 7)
- **Performance**: No regression (O(1) dictionary lookup preserved)

---

**Ticket Status**: READY FOR EXECUTION  
**Phase 4 Completed**: 2026-06-15T16:54:59Z  
**Next Phase**: Phase 5 (Ticket Execution)
