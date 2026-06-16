# Extraction Tickets: EPIC-CCN-076

## Overview
- **Total Tickets**: 3
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3)
- **Estimated Effort**: 2 hours
- **Target Method**: CollapseAllExecutionControls
- **File**: src/V12_002.UI.Panel.Handlers.cs
- **Current Complexity**: 11 (CYC)
- **Target Complexity**: ≤8 (Jane Street standard)

## TICKET-1: Extract CollapseExecutionRows Helper

### Scope
- **Current Method**: `CollapseAllExecutionControls`
- **Current CYC**: 11
- **Target CYC**: 2 (helper method)
- **Extraction**: Create private helper method to collapse execution row UI elements

### Implementation
1. Create new private method `CollapseExecutionRows()` after line 686
2. Move lines 667-670 (execRetestRow and execTrendRow null checks) into helper
3. Add XML documentation comment describing helper's purpose
4. Verify helper accesses instance fields correctly
5. Run CSharpier formatter: `dotnet csharpier format src/`

### Code Template
```csharp
/// <summary>
/// Collapses execution row UI elements (retest and trend rows).
/// </summary>
private void CollapseExecutionRows()
{
    if (execRetestRow != null)
    {
        execRetestRow.Visibility = Visibility.Collapsed;
    }
    if (execTrendRow != null)
    {
        execTrendRow.Visibility = Visibility.Collapsed;
    }
}
```

### Acceptance Criteria
- [ ] Helper method created with CYC = 2
- [ ] XML documentation added
- [ ] No compilation errors
- [ ] CSharpier formatting passes
- [ ] Original lines 667-670 remain unchanged (will be removed in TICKET-3)

### Dependencies
- None (first ticket)

---

## TICKET-2: Extract CollapseExecutionButtons Helper

### Scope
- **Current Method**: `CollapseAllExecutionControls`
- **Current CYC**: 11
- **Target CYC**: 7 (helper method)
- **Extraction**: Create private helper method to collapse execution button UI elements

### Implementation
1. Create new private method `CollapseExecutionButtons()` after CollapseExecutionRows
2. Move lines 671-684 (7 button null checks) into helper
3. Add XML documentation comment describing helper's purpose
4. Verify helper accesses instance fields correctly
5. Run CSharpier formatter: `dotnet csharpier format src/`

### Code Template
```csharp
/// <summary>
/// Collapses execution button UI elements (RMA, MOMO, FFMA, M, OR Long/Short).
/// </summary>
private void CollapseExecutionButtons()
{
    if (rmaButton != null)
    {
        rmaButton.Visibility = Visibility.Collapsed;
    }
    if (momoButton != null)
    {
        momoButton.Visibility = Visibility.Collapsed;
    }
    if (ffmaButton != null)
    {
        ffmaButton.Visibility = Visibility.Collapsed;
    }
    if (ffmaManualButton != null)
    {
        ffmaManualButton.Visibility = Visibility.Collapsed;
    }
    if (mButton != null)
    {
        mButton.Visibility = Visibility.Collapsed;
    }
    if (orLongButton != null)
    {
        orLongButton.Visibility = Visibility.Collapsed;
    }
    if (orShortButton != null)
    {
        orShortButton.Visibility = Visibility.Collapsed;
    }
}
```

### Acceptance Criteria
- [ ] Helper method created with CYC = 7
- [ ] XML documentation added
- [ ] No compilation errors
- [ ] CSharpier formatting passes
- [ ] Original lines 671-684 remain unchanged (will be removed in TICKET-3)

### Dependencies
- TICKET-1 must be completed first

---

## TICKET-3: Refactor Main Method

### Scope
- **Current Method**: `CollapseAllExecutionControls`
- **Current CYC**: 11
- **Target CYC**: 3
- **Extraction**: Replace inline logic with helper method calls

### Implementation
1. Replace lines 667-670 with call to `CollapseExecutionRows()`
2. Replace lines 671-684 with call to `CollapseExecutionButtons()`
3. Keep lines 685-686 (manualEntryRow logic) unchanged
4. Verify main method now has CYC = 3
5. Run CSharpier formatter: `dotnet csharpier format src/`
6. Run build: `powershell -File .\scripts\build_readiness.ps1`
7. Run hard-link sync: `powershell -File .\deploy-sync.ps1`

### Code Template
```csharp
private void CollapseAllExecutionControls()
{
    CollapseExecutionRows();
    CollapseExecutionButtons();
    
    if (manualEntryRow != null)
    {
        manualEntryRow.Visibility = Visibility.Visible;
    }
}
```

### Acceptance Criteria
- [ ] Main method complexity reduced to CYC = 3
- [ ] All helper methods called correctly
- [ ] All tests pass (100% pass rate)
- [ ] No behavioral changes (UI collapse sequence identical)
- [ ] Build succeeds with zero errors
- [ ] CSharpier formatting passes
- [ ] Hard-link integrity maintained (deploy-sync.ps1 succeeds)
- [ ] No lock() statements introduced
- [ ] WPF UI thread model unchanged

### Dependencies
- TICKET-1 must be completed first
- TICKET-2 must be completed first

---

## Verification Commands

### After Each Ticket
```powershell
# Format check
dotnet csharpier check src/

# Complexity audit
python scripts/complexity_audit.py
```

### After TICKET-3 (Final Verification)
```powershell
# Full build and sync
powershell -File .\scripts\build_readiness.ps1
powershell -File .\deploy-sync.ps1

# Pre-push validation (fast mode)
powershell -File .\scripts\pre_push_validation.ps1 -Fast
```

## Risk Mitigation

### Technical Risk: MINIMAL
- Simple extraction with no logic changes
- No API signature changes
- No caller modifications required
- WPF UI thread model unchanged

### Regression Risk: LOW
- Behavior preservation enforced
- Existing tests validate correctness
- Sequential execution prevents partial states

### Integration Risk: NONE
- No changes to method signature
- No changes to callers
- No changes to callees

## Success Metrics

### Complexity Reduction
- **Before**: Main method CYC = 11
- **After**: Main method CYC = 3 (73% reduction)
- **Helpers**: CYC 2 + CYC 7 = 9 (both ≤8)

### Jane Street Compliance
- ✅ All methods CYC ≤8
- ✅ Cognitive simplicity achieved
- ✅ Single responsibility per method
- ✅ Testability improved

### V12 DNA Compliance
- ✅ No lock() statements
- ✅ Lock-free pattern maintained
- ✅ ASCII-only compliance
- ✅ Hard-link integrity preserved

## Notes
- Execute tickets sequentially (1 → 2 → 3)
- Run CSharpier after each ticket
- Verify build after TICKET-3 only
- Do not skip verification commands
- Maintain WPF dispatcher model throughout
