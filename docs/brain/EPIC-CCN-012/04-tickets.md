# Extraction Tickets: EPIC-CCN-012

## Overview
- **Epic ID**: EPIC-CCN-012
- **Method**: SyncPanelConfigFromSnapshot
- **File**: src/V12_002.UI.Panel.StateSync.cs
- **Current Complexity**: 15
- **Target Complexity**: ≤8 per method
- **Total Tickets**: 3
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3)
- **Estimated Effort**: 4 hours (1.5h per ticket + 0.5h integration)

---

## TICKET-1: Extract SyncTargetValues Helper

### Scope
- **Current Method**: `SyncPanelConfigFromSnapshot`
- **Current CYC**: 15
- **Target CYC**: 6 (for extracted method)
- **Extraction**: Synchronize all 5 target value text boxes (svT1Val through svT5Val)

### Implementation
1. **Create Test File** (if not exists): `tests/V12_Performance.Tests/UI/Panel/StateSyncTests.cs`
2. **Write Failing Tests**:
   - Test valid config with all 5 target values
   - Test null UI elements (defensive)
   - Test empty string values
3. **Extract Method**:
   ```csharp
   private void SyncTargetValues(UIConfigSnapshot config)
   {
       if (config.svT1Val != null) txtT1Val.Text = config.svT1Val;
       if (config.svT2Val != null) txtT2Val.Text = config.svT2Val;
       if (config.svT3Val != null) txtT3Val.Text = config.svT3Val;
       if (config.svT4Val != null) txtT4Val.Text = config.svT4Val;
       if (config.svT5Val != null) txtT5Val.Text = config.svT5Val;
   }
   ```
4. **Update Main Method**: Replace inline code with `SyncTargetValues(config);`
5. **Run Tests**: Verify all tests pass
6. **Complexity Audit**: Run `python scripts/complexity_audit.py` and verify CYC ≤ 6

### Acceptance Criteria
- [ ] Method complexity reduced to 6
- [ ] All existing tests pass
- [ ] New unit tests added (3 test cases minimum)
- [ ] No behavioral changes (pure refactoring)
- [ ] Build succeeds (`dotnet build`)
- [ ] Complexity audit passes
- [ ] Git checkpoint created before extraction

### Dependencies
- None (first ticket)

### Verification Commands
```bash
# Run tests
dotnet test tests/V12_Performance.Tests/UI/Panel/StateSyncTests.cs

# Complexity audit
python scripts/complexity_audit.py

# Build check
dotnet build
```

---

## TICKET-2: Extract SyncTargetTypes Helper

### Scope
- **Current Method**: `SyncPanelConfigFromSnapshot`
- **Current CYC**: 9 (after TICKET-1)
- **Target CYC**: 6 (for extracted method)
- **Extraction**: Synchronize all 5 target type combo boxes (svT1Type through svT5Type)

### Implementation
1. **Write Failing Tests**:
   - Test valid config with all 5 target types
   - Test null UI elements (defensive)
   - Test empty string values
2. **Extract Method**:
   ```csharp
   private void SyncTargetTypes(UIConfigSnapshot config)
   {
       if (config.svT1Type != null) cmbT1Type.SelectedItem = config.svT1Type;
       if (config.svT2Type != null) cmbT2Type.SelectedItem = config.svT2Type;
       if (config.svT3Type != null) cmbT3Type.SelectedItem = config.svT3Type;
       if (config.svT4Type != null) cmbT4Type.SelectedItem = config.svT4Type;
       if (config.svT5Type != null) cmbT5Type.SelectedItem = config.svT5Type;
   }
   ```
3. **Update Main Method**: Replace inline code with `SyncTargetTypes(config);`
4. **Run Tests**: Verify all tests pass
5. **Complexity Audit**: Run `python scripts/complexity_audit.py` and verify CYC ≤ 6

### Acceptance Criteria
- [ ] Method complexity reduced to 6
- [ ] All existing tests pass
- [ ] New unit tests added (3 test cases minimum)
- [ ] No behavioral changes (pure refactoring)
- [ ] Build succeeds (`dotnet build`)
- [ ] Complexity audit passes
- [ ] Git checkpoint created before extraction

### Dependencies
- **TICKET-1** must be completed first

### Verification Commands
```bash
# Run tests
dotnet test tests/V12_Performance.Tests/UI/Panel/StateSyncTests.cs

# Complexity audit
python scripts/complexity_audit.py

# Build check
dotnet build
```

---

## TICKET-3: Extract SyncRiskAndStopConfig Helper

### Scope
- **Current Method**: `SyncPanelConfigFromSnapshot`
- **Current CYC**: 4 (after TICKET-2)
- **Target CYC**: 7 (for extracted method)
- **Extraction**: Synchronize stop value, max risk, chase-if-touch points, and stop type (mode-dependent)

### Implementation
1. **Write Failing Tests**:
   - Test ORB mode (stop type = "ORB")
   - Test non-ORB mode (stop type from config)
   - Test empty chase-if-touch string
   - Test null UI elements (defensive)
2. **Extract Method**:
   ```csharp
   private void SyncRiskAndStopConfig(UIConfigSnapshot config, string mode)
   {
       if (config.strVal != null) txtStopVal.Text = config.strVal;
       if (config.maxVal != null) txtMaxRisk.Text = config.maxVal;
       if (config.citVal != null) txtChaseIfTouch.Text = config.citVal;
       
       if (config.svStrType != null)
       {
           cmbStopType.SelectedItem = string.Equals(mode, "ORB", StringComparison.OrdinalIgnoreCase)
               ? "ORB"
               : config.svStrType;
       }
   }
   ```
3. **Update Main Method**: Replace inline code with `SyncRiskAndStopConfig(config, snapshot.Mode);`
4. **Run Tests**: Verify all tests pass
5. **Complexity Audit**: Run `python scripts/complexity_audit.py` and verify CYC ≤ 7

### Acceptance Criteria
- [ ] Method complexity reduced to 7
- [ ] All existing tests pass
- [ ] New unit tests added (4 test cases minimum)
- [ ] No behavioral changes (pure refactoring)
- [ ] Build succeeds (`dotnet build`)
- [ ] Complexity audit passes
- [ ] Main method complexity reduced to 1
- [ ] Git checkpoint created before extraction

### Dependencies
- **TICKET-1** must be completed first
- **TICKET-2** must be completed first

### Verification Commands
```bash
# Run tests
dotnet test tests/V12_Performance.Tests/UI/Panel/StateSyncTests.cs

# Complexity audit (verify main method CYC = 1)
python scripts/complexity_audit.py

# Build check
dotnet build
```

---

## Final Integration Verification

### After All Tickets Complete

1. **Run Full Test Suite**:
   ```bash
   dotnet test
   ```

2. **Complexity Audit** (verify all methods ≤8):
   ```bash
   python scripts/complexity_audit.py
   ```

3. **Pre-Push Validation**:
   ```bash
   powershell -File .\scripts\pre_push_validation.ps1
   ```

4. **Hard-Link Sync**:
   ```bash
   powershell -File .\deploy-sync.ps1
   ```

5. **Manual Test**: F5 in NinjaTrader, verify UI panel loads and syncs correctly

### Expected Final State
- **SyncPanelConfigFromSnapshot**: CYC = 1 ✅
- **SyncTargetValues**: CYC = 6 ✅
- **SyncTargetTypes**: CYC = 6 ✅
- **SyncRiskAndStopConfig**: CYC = 7 ✅
- **Maximum Complexity**: 7 (well below target of 8) ✅

---

## Rollback Plan

### If Any Ticket Fails
1. **Git Restore**: `git restore src/V12_002.UI.Panel.StateSync.cs`
2. **Review Failure**: Analyze test output or complexity audit
3. **Re-Plan**: Update architecture plan if needed
4. **Retry**: Re-attempt extraction with corrected approach

### Checkpoints
- Checkpoint before TICKET-1: `git add -A && git commit -m "EPIC-CCN-012: Pre-TICKET-1 checkpoint"`
- Checkpoint before TICKET-2: `git add -A && git commit -m "EPIC-CCN-012: Pre-TICKET-2 checkpoint"`
- Checkpoint before TICKET-3: `git add -A && git commit -m "EPIC-CCN-012: Pre-TICKET-3 checkpoint"`

---

## Success Metrics

### Complexity Reduction
- **Before**: Main method CYC = 15
- **After**: Main method CYC = 1
- **Reduction**: 93% (14 points)

### Code Quality
- **Testability**: 3 new helper methods, each independently testable
- **Readability**: Clear separation of concerns (values, types, risk/stop config)
- **Maintainability**: Easy to modify one category without affecting others

### Jane Street Alignment
- ✅ **Keep functions small**: Each helper is 5-10 lines
- ✅ **Single responsibility**: Each method syncs one category
- ✅ **Explicit over implicit**: Clear method names
- ✅ **No clever abstractions**: Straightforward null-check patterns

---

**Document Version**: 1.0  
**Created**: 2026-06-15  
**Protocol**: V12.23 Phase 4 Ticket Generation  
**Next Phase**: Phase 5 (Ticket Execution)
