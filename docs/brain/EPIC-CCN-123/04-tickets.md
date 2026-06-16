# Phase 4: Implementation Tickets - EPIC-CCN-123

## Epic Overview

**Target**: SyncPanelConfigFromSnapshot method complexity reduction
**Current Complexity**: 15 (at Jane Street threshold)
**Target Complexity**: ≤ 8 (Jane Street HFT standard)
**Expected Post-Extraction**: 3 (orchestration only)
**File**: src/V12_002.UI.Panel.StateSync.cs
**Method Location**: Lines 537-569

---

## Execution Order

**Sequential Execution Required** (dependencies exist):

1. **TICKET-123-T1**: Extract SyncTargetValuesToPanel (no dependencies)
2. **TICKET-123-T2**: Extract SyncTargetTypesToPanel (depends on T1 completion)
3. **TICKET-123-T3**: Extract SyncStopAndRiskToPanel (depends on T2 completion)
4. **TICKET-123-T4**: Update Parent Method (depends on T1, T2, T3 completion)

**Rationale**: Each extraction must be verified independently before proceeding. Parent method update requires all helper methods to exist.

---

## TICKET-123-T1: Extract SyncTargetValuesToPanel

### Priority: P5 (Surgical Extraction)
### Estimated Time: 10 minutes
### Complexity Reduction: -5 branches from parent

### Objective
Extract all target value TextBox updates into a dedicated helper method to reduce parent method complexity.

### Method Signature
```csharp
private void SyncTargetValuesToPanel(UIConfigSnapshot config)
```

### Current Code Location
**File**: src/V12_002.UI.Panel.StateSync.cs
**Lines**: 542-546 (5 lines in parent method)

### Extraction Steps

1. **Create Checkpoint**
   - Action: Bob CLI auto-checkpoint (enabled by default)
   - Verification: Checkpoint ID logged in session
   - Rollback: `/restore` if needed

2. **Add New Method** (after line 569)
   ```csharp
   private void SyncTargetValuesToPanel(UIConfigSnapshot config)
   {
       if (svT1Val != null)
           svT1Val.Text = FormatPanelDouble(config.Target1Value);
       if (svT2Val != null)
           svT2Val.Text = FormatPanelDouble(config.Target2Value);
       if (svT3Val != null)
           svT3Val.Text = FormatPanelDouble(config.Target3Value);
       if (svT4Val != null)
           svT4Val.Text = FormatPanelDouble(config.Target4Value);
       if (svT5Val != null)
           svT5Val.Text = FormatPanelDouble(config.Target5Value);
   }
   ```

3. **Verify Build**
   - Command: `dotnet build src/V12_002.csproj`
   - Expected: Zero errors, zero new warnings
   - On Failure: Rollback to checkpoint

4. **Verify Complexity**
   - Command: `python scripts/complexity_audit.py`
   - Expected: SyncTargetValuesToPanel CYC = 5
   - Threshold: CYC ≤ 8 (Jane Street standard)

### Implementation Details

**Lines of Code**: 12
**Cyclomatic Complexity**: 5 (5 null checks)
**Branches**: 5 (one per target value)
**Dependencies**: None (pure extraction)

**Preserved Logic**:
- ✅ All 5 null checks maintained
- ✅ FormatPanelDouble() calls unchanged
- ✅ Same execution order
- ✅ No semantic changes

### Test Requirements

**Manual Test** (NinjaTrader):
1. Launch NinjaTrader with V12_002 strategy
2. Open strategy panel
3. Modify Target1-5 values in strategy parameters
4. Verify panel TextBoxes update correctly
5. Test with null controls (should not throw)

**Expected Result**: All 5 target value TextBoxes display formatted values

**Unit Test** (Future - EPIC-CCN-10):
- Test_SyncTargetValuesToPanel_AllControlsNull
- Test_SyncTargetValuesToPanel_AllControlsValid
- Test_SyncTargetValuesToPanel_PartialControlsNull

### Verification Criteria

**Build Health**:
- ✅ `dotnet build` succeeds with zero errors
- ✅ Zero new compiler warnings
- ✅ No Roslyn analyzer violations

**Complexity Metrics**:
- ✅ SyncTargetValuesToPanel CYC = 5 (within threshold)
- ✅ Parent method CYC reduced by 5 (15 → 10)

**Functional Equivalence**:
- ✅ Panel target values update correctly
- ✅ Null controls handled gracefully
- ✅ FormatPanelDouble() behavior unchanged

**V12 DNA Compliance**:
- ✅ No lock() statements
- ✅ ASCII-only strings
- ✅ Atomic UI updates

### Rollback Steps

**Trigger Conditions**:
- Build failure
- Complexity increase
- Functional regression

**Rollback Procedure**:
1. Execute: `/restore` in Bob CLI
2. Verify: `dotnet build src/V12_002.csproj`
3. Confirm: Parent method CYC = 15 (original)
4. Report: Document failure reason

**Rollback Time**: 2-3 minutes (automated)

### Success Criteria

- ✅ New method added at line ~570
- ✅ Build succeeds with zero errors
- ✅ Method complexity CYC = 5
- ✅ Parent complexity reduced to 10
- ✅ Manual test passes
- ✅ No V12 DNA violations

### Dependencies
- **Upstream**: None (first extraction)
- **Downstream**: TICKET-123-T4 (parent method update)

---

## TICKET-123-T2: Extract SyncTargetTypesToPanel

### Priority: P5 (Surgical Extraction)
### Estimated Time: 10 minutes
### Complexity Reduction: -5 branches from parent

### Objective
Extract all target type ComboBox updates into a dedicated helper method to further reduce parent method complexity.

### Method Signature
```csharp
private void SyncTargetTypesToPanel(UIConfigSnapshot config)
```

### Current Code Location
**File**: src/V12_002.UI.Panel.StateSync.cs
**Lines**: 548-552 (5 lines in parent method)

### Extraction Steps

1. **Verify T1 Completion**
   - Confirm: SyncTargetValuesToPanel exists
   - Confirm: Build is clean
   - Confirm: Parent CYC = 10

2. **Add New Method** (after SyncTargetValuesToPanel, line ~583)
   ```csharp
   private void SyncTargetTypesToPanel(UIConfigSnapshot config)
   {
       if (svT1Type != null)
           SetComboSelection(svT1Type, GetPanelTargetModeText(config.Target1Type));
       if (svT2Type != null)
           SetComboSelection(svT2Type, GetPanelTargetModeText(config.Target2Type));
       if (svT3Type != null)
           SetComboSelection(svT3Type, GetPanelTargetModeText(config.Target3Type));
       if (svT4Type != null)
           SetComboSelection(svT4Type, GetPanelTargetModeText(config.Target4Type));
       if (svT5Type != null)
           SetComboSelection(svT5Type, GetPanelTargetModeText(config.Target5Type));
   }
   ```

3. **Verify Build**
   - Command: `dotnet build src/V12_002.csproj`
   - Expected: Zero errors, zero new warnings
   - On Failure: Rollback to T1 checkpoint

4. **Verify Complexity**
   - Command: `python scripts/complexity_audit.py`
   - Expected: SyncTargetTypesToPanel CYC = 5
   - Expected: Parent CYC reduced to 5

### Implementation Details

**Lines of Code**: 12
**Cyclomatic Complexity**: 5 (5 null checks)
**Branches**: 5 (one per target type)
**Dependencies**: T1 must be complete

**Preserved Logic**:
- ✅ All 5 null checks maintained
- ✅ SetComboSelection() calls unchanged
- ✅ GetPanelTargetModeText() calls unchanged
- ✅ Same execution order
- ✅ No semantic changes

### Test Requirements

**Manual Test** (NinjaTrader):
1. Launch NinjaTrader with V12_002 strategy
2. Open strategy panel
3. Modify Target1-5 types in strategy parameters
4. Verify panel ComboBoxes update correctly
5. Test with null controls (should not throw)

**Expected Result**: All 5 target type ComboBoxes display correct mode text

**Unit Test** (Future - EPIC-CCN-10):
- Test_SyncTargetTypesToPanel_AllControlsNull
- Test_SyncTargetTypesToPanel_AllControlsValid
- Test_SyncTargetTypesToPanel_PartialControlsNull

### Verification Criteria

**Build Health**:
- ✅ `dotnet build` succeeds with zero errors
- ✅ Zero new compiler warnings
- ✅ No Roslyn analyzer violations

**Complexity Metrics**:
- ✅ SyncTargetTypesToPanel CYC = 5 (within threshold)
- ✅ Parent method CYC reduced by 5 (10 → 5)

**Functional Equivalence**:
- ✅ Panel target types update correctly
- ✅ Null controls handled gracefully
- ✅ SetComboSelection() behavior unchanged
- ✅ GetPanelTargetModeText() behavior unchanged

**V12 DNA Compliance**:
- ✅ No lock() statements
- ✅ ASCII-only strings
- ✅ Atomic UI updates

### Rollback Steps

**Trigger Conditions**:
- Build failure
- Complexity increase
- Functional regression

**Rollback Procedure**:
1. Execute: `/restore` in Bob CLI (to T1 checkpoint)
2. Verify: `dotnet build src/V12_002.csproj`
3. Confirm: Parent method CYC = 10 (T1 state)
4. Report: Document failure reason

**Rollback Time**: 2-3 minutes (automated)

### Success Criteria

- ✅ New method added at line ~583
- ✅ Build succeeds with zero errors
- ✅ Method complexity CYC = 5
- ✅ Parent complexity reduced to 5
- ✅ Manual test passes
- ✅ No V12 DNA violations

### Dependencies
- **Upstream**: TICKET-123-T1 (must be complete)
- **Downstream**: TICKET-123-T4 (parent method update)

---

## TICKET-123-T3: Extract SyncStopAndRiskToPanel

### Priority: P5 (Surgical Extraction)
### Estimated Time: 10 minutes
### Complexity Reduction: -5 branches from parent

### Objective
Extract stop, max risk, and chase-if-touch updates into a dedicated helper method to achieve final complexity target.

### Method Signature
```csharp
private void SyncStopAndRiskToPanel(UIConfigSnapshot config, string mode)
```

### Current Code Location
**File**: src/V12_002.UI.Panel.StateSync.cs
**Lines**: 554-560 (7 lines in parent method)

### Extraction Steps

1. **Verify T2 Completion**
   - Confirm: SyncTargetValuesToPanel exists
   - Confirm: SyncTargetTypesToPanel exists
   - Confirm: Build is clean
   - Confirm: Parent CYC = 5

2. **Add New Method** (after SyncTargetTypesToPanel, line ~596)
   ```csharp
   private void SyncStopAndRiskToPanel(UIConfigSnapshot config, string mode)
   {
       if (strVal != null)
           strVal.Text = FormatPanelDouble(config.StopValue);
       
       if (maxVal != null)
           maxVal.Text = FormatPanelDouble(config.MaxRiskValue);
       
       if (citVal != null)
           citVal.Text = string.IsNullOrEmpty(config.ChaseIfTouchPoints) ? "0" : config.ChaseIfTouchPoints;
       
       if (svStrType != null)
           SetComboSelection(svStrType, string.Equals(mode, "ORB", StringComparison.OrdinalIgnoreCase) ? "OR" : "ATR");
   }
   ```

3. **Verify Build**
   - Command: `dotnet build src/V12_002.csproj`
   - Expected: Zero errors, zero new warnings
   - On Failure: Rollback to T2 checkpoint

4. **Verify Complexity**
   - Command: `python scripts/complexity_audit.py`
   - Expected: SyncStopAndRiskToPanel CYC = 5
   - Expected: Parent CYC still 5 (not yet updated)

### Implementation Details

**Lines of Code**: 14
**Cyclomatic Complexity**: 5 (3 null checks + 1 null check + 1 ternary)
**Branches**: 5 (4 null checks + 1 ternary operator)
**Dependencies**: T1 and T2 must be complete

**Preserved Logic**:
- ✅ All 4 null checks maintained
- ✅ FormatPanelDouble() calls unchanged
- ✅ string.IsNullOrEmpty() check unchanged
- ✅ string.Equals() with OrdinalIgnoreCase unchanged
- ✅ Ternary operator logic unchanged
- ✅ Same execution order
- ✅ No semantic changes

### Test Requirements

**Manual Test** (NinjaTrader):
1. Launch NinjaTrader with V12_002 strategy
2. Open strategy panel
3. Test ORB mode: Verify svStrType shows "OR"
4. Test non-ORB mode: Verify svStrType shows "ATR"
5. Modify stop, max risk, chase values
6. Verify panel controls update correctly
7. Test with null controls (should not throw)

**Expected Result**: Stop/risk/chase controls display correct values, mode-dependent logic works

**Unit Test** (Future - EPIC-CCN-10):
- Test_SyncStopAndRiskToPanel_OrbMode
- Test_SyncStopAndRiskToPanel_NonOrbMode
- Test_SyncStopAndRiskToPanel_AllControlsNull
- Test_SyncStopAndRiskToPanel_EmptyChasePoints

### Verification Criteria

**Build Health**:
- ✅ `dotnet build` succeeds with zero errors
- ✅ Zero new compiler warnings
- ✅ No Roslyn analyzer violations

**Complexity Metrics**:
- ✅ SyncStopAndRiskToPanel CYC = 5 (within threshold)
- ✅ Parent method CYC still 5 (awaiting T4 update)

**Functional Equivalence**:
- ✅ Stop value updates correctly
- ✅ Max risk value updates correctly
- ✅ Chase-if-touch value updates correctly
- ✅ ORB mode shows "OR"
- ✅ Non-ORB mode shows "ATR"
- ✅ Null controls handled gracefully

**V12 DNA Compliance**:
- ✅ No lock() statements
- ✅ ASCII-only strings
- ✅ Atomic UI updates

### Rollback Steps

**Trigger Conditions**:
- Build failure
- Complexity increase
- Functional regression

**Rollback Procedure**:
1. Execute: `/restore` in Bob CLI (to T2 checkpoint)
2. Verify: `dotnet build src/V12_002.csproj`
3. Confirm: Parent method CYC = 5 (T2 state)
4. Report: Document failure reason

**Rollback Time**: 2-3 minutes (automated)

### Success Criteria

- ✅ New method added at line ~596
- ✅ Build succeeds with zero errors
- ✅ Method complexity CYC = 5
- ✅ Parent complexity still 5 (awaiting T4)
- ✅ Manual test passes (ORB/non-ORB modes)
- ✅ No V12 DNA violations

### Dependencies
- **Upstream**: TICKET-123-T1, TICKET-123-T2 (must be complete)
- **Downstream**: TICKET-123-T4 (parent method update)

---

## TICKET-123-T4: Update Parent Method

### Priority: P5 (Surgical Refactoring)
### Estimated Time: 15 minutes
### Complexity Reduction: Final reduction to CYC = 3

### Objective
Replace extracted code in parent method with helper method calls to achieve final complexity target of CYC = 3.

### Current Code Location
**File**: src/V12_002.UI.Panel.StateSync.cs
**Lines**: 537-569 (33 lines)

### Refactoring Steps

1. **Verify T1, T2, T3 Completion**
   - Confirm: SyncTargetValuesToPanel exists (CYC = 5)
   - Confirm: SyncTargetTypesToPanel exists (CYC = 5)
   - Confirm: SyncStopAndRiskToPanel exists (CYC = 5)
   - Confirm: Build is clean
   - Confirm: Parent CYC = 5

2. **Replace Lines 542-560 with Method Calls**
   **Before** (lines 542-560):
   ```csharp
   if (svT1Val != null) svT1Val.Text = FormatPanelDouble(config.Target1Value);
   if (svT2Val != null) svT2Val.Text = FormatPanelDouble(config.Target2Value);
   if (svT3Val != null) svT3Val.Text = FormatPanelDouble(config.Target3Value);
   if (svT4Val != null) svT4Val.Text = FormatPanelDouble(config.Target4Value);
   if (svT5Val != null) svT5Val.Text = FormatPanelDouble(config.Target5Value);
   
   if (svT1Type != null) SetComboSelection(svT1Type, GetPanelTargetModeText(config.Target1Type));
   if (svT2Type != null) SetComboSelection(svT2Type, GetPanelTargetModeText(config.Target2Type));
   if (svT3Type != null) SetComboSelection(svT3Type, GetPanelTargetModeText(config.Target3Type));
   if (svT4Type != null) SetComboSelection(svT4Type, GetPanelTargetModeText(config.Target4Type));
   if (svT5Type != null) SetComboSelection(svT5Type, GetPanelTargetModeText(config.Target5Type));
   
   if (strVal != null) strVal.Text = FormatPanelDouble(config.StopValue);
   if (maxVal != null) maxVal.Text = FormatPanelDouble(config.MaxRiskValue);
   if (citVal != null) citVal.Text = string.IsNullOrEmpty(config.ChaseIfTouchPoints) ? "0" : config.ChaseIfTouchPoints;
   if (svStrType != null) SetComboSelection(svStrType, string.Equals(snapshot.Mode, "ORB", StringComparison.OrdinalIgnoreCase) ? "OR" : "ATR");
   ```
   
   **After** (lines 542-544):
   ```csharp
   SyncTargetValuesToPanel(config);
   SyncTargetTypesToPanel(config);
   SyncStopAndRiskToPanel(config, snapshot.Mode);
   ```

3. **Verify Build**
   - Command: `dotnet build src/V12_002.csproj`
   - Expected: Zero errors, zero new warnings
   - On Failure: Rollback to T3 checkpoint

4. **Verify Complexity**
   - Command: `python scripts/complexity_audit.py`
   - Expected: SyncPanelConfigFromSnapshot CYC = 3
   - Threshold: CYC ≤ 8 (Jane Street standard)
   - Achievement: 62.5% below threshold

5. **Format Code**
   - Command: `dotnet csharpier format src/V12_002.UI.Panel.StateSync.cs`
   - Expected: No formatting issues
   - Verification: `dotnet csharpier check src/V12_002.UI.Panel.StateSync.cs`

6. **Final Verification Suite**
   - Command 1: `python scripts/complexity_audit.py` (verify CYC = 3)
   - Command 2: `powershell -File .\scripts\build_readiness.ps1` (full build check)
   - Command 3: `powershell -File .\deploy-sync.ps1` (sync hard links)
   - Expected: All checks pass

### Implementation Details

**Lines Removed**: 19 lines (extracted code)
**Lines Added**: 3 lines (method calls)
**Net Change**: -16 lines
**Final Parent Complexity**: 3 (1 null coalesce + 1 Math.Max + 1 Math.Min)
**Complexity Reduction**: 15 → 3 (80% reduction)

**Preserved Logic**:
- ✅ Same execution order (values → types → stop/risk)
- ✅ All null checks maintained (in helper methods)
- ✅ All helper method calls unchanged
- ✅ Same side effects
- ✅ No semantic changes

### Test Requirements

**Manual Test** (NinjaTrader - Full Flow):
1. Launch NinjaTrader with V12_002 strategy
2. Open strategy panel
3. Modify all config values (targets, types, stop, risk, chase)
4. Verify all panel controls update correctly
5. Test ORB mode switching
6. Test target count changes (1-5)
7. Verify SyncCountChipVisuals() works
8. Verify UpdateTargetVisibility() works

**Expected Result**: Panel displays match strategy config exactly, all orchestration works

**Unit Test** (Future - EPIC-CCN-10):
- Test_SyncPanelConfigFromSnapshot_NullConfig
- Test_SyncPanelConfigFromSnapshot_FullFlow
- Test_SyncPanelConfigFromSnapshot_TargetCount1
- Test_SyncPanelConfigFromSnapshot_TargetCount5

### Verification Criteria

**Build Health**:
- ✅ `dotnet build` succeeds with zero errors
- ✅ Zero new compiler warnings
- ✅ No Roslyn analyzer violations
- ✅ CSharpier formatting passes

**Complexity Metrics**:
- ✅ SyncPanelConfigFromSnapshot CYC = 3 (target achieved)
- ✅ 80% complexity reduction (15 → 3)
- ✅ 62.5% below Jane Street threshold (3 vs 8)
- ✅ All helper methods CYC ≤ 5

**Functional Equivalence**:
- ✅ All target values update correctly
- ✅ All target types update correctly
- ✅ Stop/risk/chase values update correctly
- ✅ ORB/non-ORB mode switching works
- ✅ Target count orchestration works
- ✅ Chip visuals update correctly
- ✅ Target visibility updates correctly

**V12 DNA Compliance**:
- ✅ No lock() statements
- ✅ ASCII-only strings
- ✅ Atomic UI updates
- ✅ Hard links synchronized

**PR Hygiene**:
- ✅ Diff size < 10k characters (~1,800 chars)
- ✅ No whitespace bloat (CSharpier enforced)
- ✅ Single file modification
- ✅ No scope creep

### Rollback Steps

**Trigger Conditions**:
- Build failure
- Complexity not reduced to 3
- Functional regression
- Hard link sync failure

**Rollback Procedure**:
1. Execute: `/restore` in Bob CLI (to T3 checkpoint)
2. Verify: `dotnet build src/V12_002.csproj`
3. Confirm: Parent method CYC = 5 (T3 state)
4. Sync: `powershell -File .\deploy-sync.ps1`
5. Report: Document failure reason

**Rollback Time**: 2-3 minutes (automated)

### Success Criteria

- ✅ Parent method updated (lines 542-544)
- ✅ Build succeeds with zero errors
- ✅ Parent complexity CYC = 3 (target achieved)
- ✅ All helper methods CYC ≤ 5
- ✅ CSharpier formatting passes
- ✅ Manual test passes (full flow)
- ✅ Hard links synchronized
- ✅ No V12 DNA violations
- ✅ No PR hygiene violations

### Dependencies
- **Upstream**: TICKET-123-T1, TICKET-123-T2, TICKET-123-T3 (all must be complete)
- **Downstream**: None (final ticket)

---

## Summary Metrics

### Complexity Reduction

| Metric | Before | After | Change |
|--------|--------|-------|--------|
| Parent Method CYC | 15 | 3 | -12 (-80%) |
| Total CYC | 15 | 18 | +3 (+20%) |
| Cognitive Load | High | Low | Improved |
| Test Paths (Parent) | 32,768 | 8 | -99.98% |
| Jane Street Compliance | At Threshold | 62.5% Below | Exceeds |

**Note**: Total complexity increases slightly due to method call overhead, but cognitive complexity decreases significantly due to separation of concerns.

### Effort Estimate

| Ticket | Time | Complexity |
|--------|------|------------|
| T1 | 10 min | Low |
| T2 | 10 min | Low |
| T3 | 10 min | Low |
| T4 | 15 min | Medium |
| **Total** | **45 min** | **Low-Medium** |

### Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| Build Failure | LOW | LOW | Incremental verification |
| Functional Regression | VERY LOW | MEDIUM | Logic preservation + manual test |
| Complexity Increase | VERY LOW | LOW | Complexity audit after each step |
| Hard Link Desync | LOW | MEDIUM | deploy-sync.ps1 + verification |
| **Overall** | **LOW** | **LOW-MEDIUM** | **Comprehensive** |

---

## Execution Checklist

### Pre-Execution
- ✅ Implementation plan reviewed (02-implementation-plan.md)
- ✅ Audit report reviewed (03-audit-report.md)
- ✅ Tickets generated (04-tickets.md)
- ✅ Bob CLI ready (v12-engineer mode)
- ✅ Checkpoint enabled (auto)
- ✅ Branch created: `feature/epic-ccn-123-sync-panel-config`

### During Execution
- ⏳ T1: Extract SyncTargetValuesToPanel
- ⏳ T2: Extract SyncTargetTypesToPanel
- ⏳ T3: Extract SyncStopAndRiskToPanel
- ⏳ T4: Update Parent Method

### Post-Execution
- ⏳ Complexity audit: CYC = 3
- ⏳ Build readiness: Zero errors
- ⏳ Hard links synced
- ⏳ Manual test: Panel sync works
- ⏳ PR created
- ⏳ Codacy review: No new violations

---

## Metadata

- **Epic**: EPIC-CCN-123
- **Phase**: 4 (Ticket Generation)
- **Generated By**: Bob CLI (v12-engineer)
- **Date**: 2026-06-14
- **Status**: Tickets Ready for Execution
- **Next Phase**: Phase 5 (Recursive Execution)
- **Total Tickets**: 4 (T1, T2, T3, T4)
- **Estimated Effort**: 45 minutes
- **Risk Level**: LOW
- **Complexity Target**: CYC ≤ 8 (Expected: CYC = 3)
- **Jane Street Alignment**: Exceeds Standard (62.5% below threshold)
