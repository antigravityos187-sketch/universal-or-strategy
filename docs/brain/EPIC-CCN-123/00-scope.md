# Phase 1: Scope Definition + Boundary Validation - EPIC-CCN-123

## Target Method Analysis

### Method Signature
```csharp
private void SyncPanelConfigFromSnapshot(UIStateSnapshot snapshot)
```

### Location
- **File**: `src/V12_002.UI.Panel.StateSync.cs`
- **Lines**: 37
- **Current Complexity**: 15 (at Jane Street threshold)
- **Target Complexity**: ≤ 8 (Jane Street HFT standard)

### Complexity Sources

**Primary Complexity Drivers:**
1. **Sequential UI Control Updates** (15+ null checks)
   - 5 target value TextBox updates (svT1Val through svT5Val)
   - 5 target type ComboBox updates (svT1Type through svT5Type)
   - 3 additional control updates (strVal, maxVal, citVal)
   - 1 conditional ComboBox update (svStrType)

2. **Repetitive Pattern**
   - Each control follows: null check → assignment
   - No complex branching, just linear null-safe assignments

3. **Mixed Responsibilities**
   - Target value synchronization
   - Target type synchronization
   - Stop/Risk configuration synchronization
   - Count chip visual synchronization

### Current Method Structure
```
SyncPanelConfigFromSnapshot(snapshot)
├── Extract config from snapshot
├── Update 5 target value TextBoxes (5 null checks)
├── Update 5 target type ComboBoxes (5 null checks)
├── Update stop value TextBox (1 null check)
├── Update max risk TextBox (1 null check)
├── Update chase-if-touch TextBox (1 null check)
├── Update stop type ComboBox (1 null check + conditional)
├── Sync count chip visuals (method call)
└── Update target visibility (method call)
```

## Extraction Strategy

### What to Extract

**Extract 1: SyncTargetValuesToPanel**
- **Purpose**: Update all 5 target value TextBoxes
- **Complexity Reduction**: 5 branches → 1 method call
- **Signature**: `private void SyncTargetValuesToPanel(UIConfigSnapshot config)`
- **Lines**: ~12

**Extract 2: SyncTargetTypesToPanel**
- **Purpose**: Update all 5 target type ComboBoxes
- **Complexity Reduction**: 5 branches → 1 method call
- **Signature**: `private void SyncTargetTypesToPanel(UIConfigSnapshot config)`
- **Lines**: ~12

**Extract 3: SyncStopAndRiskToPanel**
- **Purpose**: Update stop, max risk, and chase-if-touch controls
- **Complexity Reduction**: 3 branches → 1 method call
- **Signature**: `private void SyncStopAndRiskToPanel(UIConfigSnapshot config, string mode)`
- **Lines**: ~10

### What to Keep in Original Method

**Orchestration Logic** (remains in SyncPanelConfigFromSnapshot):
```csharp
private void SyncPanelConfigFromSnapshot(UIStateSnapshot snapshot)
{
    UIConfigSnapshot config = snapshot.Config ?? new UIConfigSnapshot();
    
    // Extracted method calls
    SyncTargetValuesToPanel(config);
    SyncTargetTypesToPanel(config);
    SyncStopAndRiskToPanel(config, snapshot.Mode);
    
    // Existing method calls (already extracted)
    int count = Math.Max(1, Math.Min(5, snapshot.TargetCount));
    _panelLastSyncedTargetCount = count;
    SyncCountChipVisuals(count);
    UpdateTargetVisibility(count);
}
```

**Post-Extraction Complexity**: 3 (orchestration only)

### Extraction Pattern

**Pattern**: Sequential Decomposition
- Group related UI updates into cohesive methods
- Each extracted method handles one category of controls
- Preserve null-safety in extracted methods
- No shared state mutations (all updates are independent)

## Boundary Definition

### Single Method Scope (V12.23 Protocol)

**IN SCOPE:**
- ✅ SyncPanelConfigFromSnapshot method body only
- ✅ Extract helper methods within same file
- ✅ Preserve existing helper method calls (FormatPanelDouble, GetPanelTargetModeText, SetComboSelection)

**OUT OF SCOPE:**
- ❌ Modifying UpdatePanelState (caller)
- ❌ Modifying helper methods (FormatPanelDouble, etc.)
- ❌ Changing UIStateSnapshot or UIConfigSnapshot structures
- ❌ Refactoring other methods in StateSync.cs

### Dependency Analysis

**Inbound Dependencies (Callers):**
- `UpdatePanelState()` - calls this method when config revision changes
- No other callers identified

**Outbound Dependencies (Callees):**
- `FormatPanelDouble(double)` - existing helper (no changes)
- `GetPanelTargetModeText(int)` - existing helper (no changes)
- `SetComboSelection(ComboBox, string)` - existing helper (no changes)
- `SyncCountChipVisuals(int)` - existing method (no changes)
- `UpdateTargetVisibility(int)` - existing method (no changes)

**State Dependencies:**
- Reads: `snapshot.Config`, `snapshot.Mode`, `snapshot.TargetCount`
- Writes: `_panelLastSyncedTargetCount` (field mutation)
- UI Controls: 14 TextBox/ComboBox references (all null-checked)

### Boundary Validation

**Constraint Check:**
- ✅ **Single Method**: Extraction limited to SyncPanelConfigFromSnapshot only
- ✅ **No Scope Creep**: No modifications to callers or callees
- ✅ **No Structural Changes**: Extracted methods stay in same file/class
- ✅ **Preserve Semantics**: All null checks and assignments preserved
- ✅ **No New Dependencies**: Uses only existing helper methods

**Boundary Validated: YES**

This extraction strictly adheres to the single-method boundary. No cross-method refactoring, no structural changes, no new dependencies.

## Success Criteria

### Complexity Target
- **Current**: CYC = 15
- **Target**: CYC ≤ 8 (Jane Street HFT standard)
- **Expected Post-Extraction**: CYC = 3 (orchestration only)

### Jane Street Alignment
- ✅ **Cognitive Simplicity**: Each extracted method has single responsibility
- ✅ **Testability**: Extracted methods can be unit tested independently
- ✅ **Maintainability**: Clear separation of concerns (values, types, stop/risk)

### V12 DNA Compliance
- ✅ **Lock-Free**: No lock() usage (verified in code review)
- ✅ **ASCII-Only**: No Unicode/emoji in string literals (verified)
- ✅ **Atomic Updates**: All UI updates are independent (no race conditions)

### Verification Criteria
1. **Build Success**: Code compiles without errors
2. **Complexity Audit**: `complexity_audit.py` shows CYC ≤ 8
3. **Functional Equivalence**: Panel config sync behavior unchanged
4. **No Regressions**: Existing tests pass (if any)

## Risk Assessment

### Overall Risk: LOW

**Factors Contributing to Low Risk:**
1. **Simple Extraction**: No complex logic, just grouping assignments
2. **No Branching Changes**: All null checks preserved as-is
3. **No State Mutations**: Extracted methods are pure UI updates
4. **No Caller Impact**: UpdatePanelState continues to call same method
5. **No Dependency Changes**: All helper methods remain unchanged

### Mitigation Strategy
- **Checkpoint Before**: Create restore point before extraction
- **Incremental Extraction**: Extract one method at a time, verify build
- **Preserve Formatting**: Maintain existing code style and indentation
- **Test After Each**: Run build + complexity audit after each extraction

### Rollback Plan
- **Trigger**: Build failure or complexity increase
- **Action**: Use Bob CLI `/restore` to revert to checkpoint
- **Verification**: Re-run build_readiness.ps1

## Implementation Plan

### Phase 2 Tasks (for Engineer)
1. **Extract SyncTargetValuesToPanel**
   - Move 5 target value updates to new method
   - Verify: Build success, CYC reduction

2. **Extract SyncTargetTypesToPanel**
   - Move 5 target type updates to new method
   - Verify: Build success, CYC reduction

3. **Extract SyncStopAndRiskToPanel**
   - Move stop/risk/chase updates to new method
   - Verify: Build success, CYC reduction

4. **Update Original Method**
   - Replace extracted code with method calls
   - Verify: Build success, CYC ≤ 8

5. **Final Verification**
   - Run `complexity_audit.py`
   - Run `build_readiness.ps1`
   - Verify CYC = 3 for SyncPanelConfigFromSnapshot

## Metadata
- **Epic**: EPIC-CCN-123
- **Phase**: 1 (Scope + Boundary)
- **Analyst**: Bob CLI (v12-engineer)
- **Date**: 2026-06-13
- **Status**: Scope Defined, Boundary Validated
- **Next Phase**: Phase 2 (Implementation Planning)

