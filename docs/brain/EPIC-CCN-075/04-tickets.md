# Extraction Tickets: EPIC-CCN-075

## Overview
- **Total Tickets**: 2
- **Execution Order**: Sequential (TICKET-1 → TICKET-2)
- **Estimated Effort**: 2 hours
- **Target Method**: `OnSubmitClick` in `src/V12_002.UI.Panel.Handlers.cs`
- **Current CYC**: 12
- **Target CYC**: ≤8 (Jane Street strict standard)

## TICKET-1: Extract ValidateAndExtractInputs Helper

### Scope
- **Current Method**: `OnSubmitClick`
- **Current CYC**: 12
- **Target CYC**: Reduce by ~3 (input validation extraction)
- **Extraction**: Extract UI input validation and normalization logic into pure helper method

### Implementation
1. Create new private method `ValidateAndExtractInputs()` with return type `(string direction, string price, string mode, string symbol)`
2. Move direction extraction logic from `OnSubmitClick` (lines 263-266)
3. Move price extraction logic (line 268)
4. Move mode extraction and normalization logic (lines 270-274)
5. Move symbol extraction logic (lines 276-279)
6. Return ValueTuple with all extracted values
7. Update `OnSubmitClick` to call helper and destructure tuple: `var (direction, price, mode, symbol) = ValidateAndExtractInputs();`

### Code Changes
**New Method** (insert after line 260):
```csharp
private (string direction, string price, string mode, string symbol) ValidateAndExtractInputs()
{
    string direction =
        (directionCombo != null && directionCombo.SelectedItem is ComboBoxItem directionItem)
            ? (directionItem.Content as string ?? "OR LONG")
            : "OR LONG";
    
    string price = priceInput != null ? priceInput.Text.Trim() : string.Empty;
    
    string mode = _panelLastSyncedMode;
    if (string.IsNullOrEmpty(mode))
        mode = GetCurrentConfigMode();
    if (string.Equals(mode, "OR", StringComparison.OrdinalIgnoreCase))
        mode = "ORB";
    
    string symbol =
        Instrument != null && Instrument.MasterInstrument != null
            ? Instrument.MasterInstrument.Name
            : string.Empty;
    
    return (direction, price, mode, symbol);
}
```

**Modified OnSubmitClick** (replace lines 263-279):
```csharp
var (direction, price, mode, symbol) = ValidateAndExtractInputs();
```

### Acceptance Criteria
- [ ] `ValidateAndExtractInputs()` method created with CYC ≤3
- [ ] Method returns ValueTuple with 4 string values
- [ ] `OnSubmitClick` calls helper and destructures result
- [ ] All tests pass (behavioral preservation)
- [ ] Build succeeds: `powershell -File .\scripts\build_readiness.ps1`
- [ ] CSharpier formatting applied: `dotnet csharpier format src/`
- [ ] Complexity audit passes: `python scripts/complexity_audit.py`
- [ ] No compilation errors
- [ ] No behavioral changes (F5 test in NinjaTrader)

### Dependencies
- None (first ticket)

### Verification Commands
```powershell
# Format code
dotnet csharpier format src/

# Build and verify
powershell -File .\scripts\build_readiness.ps1

# Complexity audit
python scripts/complexity_audit.py

# Sync NinjaTrader hard links
powershell -File .\deploy-sync.ps1
```

---

## TICKET-2: Extract BuildCommandString Helper

### Scope
- **Current Method**: `OnSubmitClick` (after TICKET-1 completion)
- **Current CYC**: ~9 (after TICKET-1)
- **Target CYC**: ≤2 (orchestrator only)
- **Extraction**: Extract command string construction logic into pure helper method

### Implementation
1. Create new private method `BuildCommandString(string mode, string symbol, string direction, string price)` with return type `string`
2. Move direction normalization logic (lines 281-282 in original)
3. Move all mode-specific command building logic (lines 283-291 in original)
4. Return constructed command string
5. Update `OnSubmitClick` to call helper: `string cmd = BuildCommandString(mode, symbol, direction, price);`
6. Verify `OnSubmitClick` now only contains: helper calls, `PanelCommand(cmd)`, and `TriggerGlow(GreenFg)`

### Code Changes
**New Method** (insert after `ValidateAndExtractInputs`):
```csharp
private string BuildCommandString(string mode, string symbol, string direction, string price)
{
    string dir = direction.IndexOf("SHORT", StringComparison.OrdinalIgnoreCase) >= 0 
        ? "SHORT" 
        : "LONG";
    
    if (string.Equals(mode, "TREND", StringComparison.OrdinalIgnoreCase))
    {
        return "TREND_MANUAL_LIMIT|" + symbol + "|" + dir + "|" + price;
    }
    else if (string.Equals(mode, "RETEST", StringComparison.OrdinalIgnoreCase))
    {
        return "RETEST_MANUAL_LIMIT|" + symbol + "|" + dir + "|" + price;
    }
    else if (string.Equals(mode, "FFMA", StringComparison.OrdinalIgnoreCase))
    {
        return "FFMA_MANUAL_LIMIT|" + symbol + "|" + dir + "|" + price;
    }
    else
    {
        string cmd = dir == "LONG" ? "OR_LONG" : "OR_SHORT";
        cmd += "|" + symbol;
        if (!string.IsNullOrEmpty(price) && price != "0.00")
            cmd += "|" + price;
        return cmd;
    }
}
```

**Final OnSubmitClick** (replace lines 281-291):
```csharp
string cmd = BuildCommandString(mode, symbol, direction, price);
PanelCommand(cmd);
TriggerGlow(GreenFg);
```

### Acceptance Criteria
- [ ] `BuildCommandString()` method created with CYC ≤8
- [ ] Method accepts 4 string parameters and returns command string
- [ ] `OnSubmitClick` reduced to CYC ≤2 (orchestrator only)
- [ ] `OnSubmitClick` contains exactly 6 lines: tuple destructure, command build, command dispatch, glow trigger
- [ ] All tests pass (behavioral preservation)
- [ ] Build succeeds: `powershell -File .\scripts\build_readiness.ps1`
- [ ] CSharpier formatting applied: `dotnet csharpier format src/`
- [ ] Complexity audit passes: `python scripts/complexity_audit.py`
- [ ] No compilation errors
- [ ] No behavioral changes (F5 test in NinjaTrader)
- [ ] Hard links synced: `powershell -File .\deploy-sync.ps1`

### Dependencies
- **TICKET-1** must be completed first
- `ValidateAndExtractInputs()` must exist and be functional

### Verification Commands
```powershell
# Format code
dotnet csharpier format src/

# Build and verify
powershell -File .\scripts\build_readiness.ps1

# Complexity audit (verify OnSubmitClick CYC ≤2)
python scripts/complexity_audit.py

# Sync NinjaTrader hard links
powershell -File .\deploy-sync.ps1

# Final F5 test in NinjaTrader (behavioral preservation)
```

---

## Final Verification Checklist

### Code Quality
- [ ] OnSubmitClick CYC reduced from 12 to ≤2
- [ ] ValidateAndExtractInputs CYC ≤3
- [ ] BuildCommandString CYC ≤8
- [ ] Total file CYC unchanged (complexity redistributed)
- [ ] Zero compilation errors
- [ ] Zero lock() statements introduced

### DNA Compliance
- [ ] Correctness by Construction: ValueTuple provides type-safe data passing
- [ ] Lock-Free Actor Pattern: Zero lock() blocks, pure functions only
- [ ] ASCII-Only Compliance: Zero non-ASCII characters
- [ ] Jane Street Alignment: Cognitive simplicity achieved (CYC ≤8)

### PR Hygiene
- [ ] Diff size <10,000 characters (estimated ~1,200)
- [ ] Zero scope creep (single method focus)
- [ ] Build readiness verified
- [ ] CSharpier formatting enforced
- [ ] Hard links synced to NinjaTrader

### Testing Strategy (Phase 5)
- [ ] Unit test `ValidateAndExtractInputs` with null/valid UI controls
- [ ] Unit test `BuildCommandString` with all mode combinations (TREND/RETEST/FFMA/OR)
- [ ] Integration test `OnSubmitClick` with mocked `PanelCommand`
- [ ] End-to-end F5 test in NinjaTrader

---

## Execution Notes

### Sequential Execution Required
- TICKET-1 and TICKET-2 must be executed in order
- Each ticket must pass all acceptance criteria before proceeding
- Checkpoint after each ticket completion

### Rollback Plan
- Git revert available if F5 test fails
- Each ticket is atomic and independently revertable

### Estimated Timeline
- TICKET-1: 1 hour (extraction + verification)
- TICKET-2: 1 hour (extraction + verification)
- **Total**: 2 hours

### Agent Assignment
- **Primary**: Bob CLI (`v12-engineer`) for surgical extraction
- **Verification**: Bob CLI (verify cycle) + Orchestrator
- **Safety**: Mandatory checkpointing enabled

---

## Success Criteria Summary

**Phase 4 Complete When**:
- ✅ 2 tickets generated with clear scope
- ✅ Each ticket has acceptance criteria
- ✅ Dependencies documented
- ✅ Verification commands provided
- ✅ Sequential execution order defined

**Phase 5 Ready When**:
- All tickets have implementation steps
- All tickets have verification commands
- Rollback plan documented
- Agent assignment confirmed
