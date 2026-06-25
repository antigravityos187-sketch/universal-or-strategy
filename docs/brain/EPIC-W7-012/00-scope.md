# Phase 1: Scope Definition - EPIC-W7-012

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.18
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T19:24:46Z

## Target Method
- **Method**: SyncPanelConfigFromSnapshot
- **File**: src/V12_002.UI.Panel.StateSync.cs
- **Line**: 460
- **Current CYC**: 19
- **Target CYC**: ≤8
- **Reduction Required**: -11 points

## Scope Boundary Definition

### IN SCOPE ✅

#### Primary Extraction Target
**SyncPanelConfigFromSnapshot** (CYC 19 → ≤8)
- **Location**: src/V12_002.UI.Panel.StateSync.cs:460
- **Justification**: Exceeds Jane Street threshold by +11 points
- **Safety**: Zero blast radius, single caller (UpdatePanelState)

#### Extraction Candidates (Based on Call Hierarchy)
The method calls 15 helper methods. Scope includes refactoring the CONTROL FLOW within SyncPanelConfigFromSnapshot, specifically:

1. **Conditional Logic Extraction**
   - Multiple if/else branches for panel state validation
   - Snapshot null checks and validation
   - State-dependent UI updates

2. **UI Update Grouping**
   - Related control updates (combo boxes, text fields)
   - Visibility toggles (buttons, rows, config controls)
   - State synchronization logic

3. **Nesting Reduction**
   - Current max nesting: 3 levels
   - Target: ≤2 levels through early returns

#### Files in Scope
- ✅ **src/V12_002.UI.Panel.StateSync.cs** (primary target)

### OUT OF SCOPE ❌

#### Existing Helper Methods (Already Extracted)
These 15 methods are ALREADY properly extracted and should NOT be modified:

1. **FormatPanelDouble** - src/V12_002.UI.Panel.Construction.cs:1506
2. **SetComboSelection** - src/V12_002.UI.Panel.Construction.cs:1471
3. **GetPanelTargetModeText** - src/V12_002.UI.Panel.Construction.cs:1489
4. **SyncCountChipVisuals** - src/V12_002.UI.Panel.StateSync.cs:410
5. **UpdateTargetVisibility** - src/V12_002.UI.Panel.Handlers.cs:792
6. **UpdateConfigControlsEnabled** - src/V12_002.UI.Panel.Handlers.cs:801
7. **UpdateConfigRowsVisibility** - src/V12_002.UI.Panel.Handlers.cs:823
8. **UpdateLiveButtonsVisibility** - src/V12_002.UI.Panel.Handlers.cs:838
9. **SetT1ButtonVisible** - src/V12_002.UI.Panel.Handlers.cs:849
10. **SetT2T5ButtonsVisible** - src/V12_002.UI.Panel.Handlers.cs:857

**Rationale**: These are already single-responsibility methods with low complexity. Modifying them would violate the "No Scope Creep Protocol" (V12.23).

#### Caller Method
- ❌ **UpdatePanelState** - src/V12_002.UI.Panel.StateSync.cs:13
  - **Rationale**: Not the target of this epic. If it has high complexity, it should be addressed in a separate epic.

#### Other UI Panel Files
- ❌ **src/V12_002.UI.Panel.Construction.cs**
- ❌ **src/V12_002.UI.Panel.Handlers.cs**
- ❌ **src-vm-backup/** (backup files)
  - **Rationale**: Only touch files directly related to the target method extraction.

#### Non-UI Code
- ❌ All SIMA.Lifecycle.cs methods
- ❌ All Orders.Management.* methods
- ❌ All IPC/command handling code
  - **Rationale**: Different concern domains. Each requires separate epic.

## Extraction Strategy

### Phase 2 Architecture Plan Will Define:
1. **Validation Method Extraction**
   - Extract snapshot validation logic
   - Extract state precondition checks
   - Target: 1-2 methods, CYC ≤3 each

2. **UI Update Grouping**
   - Extract related control updates into cohesive methods
   - Group by UI concern (config controls, visibility, state sync)
   - Target: 2-3 methods, CYC ≤4 each

3. **Control Flow Simplification**
   - Replace nested if/else with early returns
   - Use guard clauses for preconditions
   - Reduce nesting from 3 → 2 levels

### Success Criteria
- ✅ SyncPanelConfigFromSnapshot: CYC 19 → ≤8
- ✅ All extracted methods: CYC ≤8
- ✅ Max nesting depth: ≤2 levels
- ✅ Zero compilation errors
- ✅ Unit tests pass
- ✅ F5 in NinjaTrader successful

## Risk Mitigation

### Low Risk Factors
- ✅ Zero blast radius (no external dependencies)
- ✅ Single caller (UpdatePanelState)
- ✅ UI layer (not critical trading logic)
- ✅ Clear extraction boundaries

### Safeguards
1. **No Scope Creep**: Only touch SyncPanelConfigFromSnapshot
2. **Preserve Behavior**: Extract without changing logic
3. **Test Coverage**: Add unit tests before refactoring
4. **Incremental Commits**: One extraction per commit

## Scope Validation

### Boundary Checks
- ✅ Target method identified: SyncPanelConfigFromSnapshot
- ✅ IN SCOPE: Only the target method's internal logic
- ✅ OUT OF SCOPE: All 15 existing helper methods
- ✅ OUT OF SCOPE: Caller method (UpdatePanelState)
- ✅ OUT OF SCOPE: Other UI panel files

### Jane Street Alignment
- ✅ Complexity threshold: CYC ≤8 (strict standard)
- ✅ Cognitive simplicity: Reduce decision paths
- ✅ Testability: Enable exhaustive testing
- ✅ Correctness by construction: Make illegal states unrepresentable

## Conclusion

**Scope is LOCKED and VALIDATED**

- ✅ Single method target: SyncPanelConfigFromSnapshot
- ✅ Clear IN SCOPE / OUT OF SCOPE boundaries
- ✅ No scope creep risk (existing helpers excluded)
- ✅ Low risk refactoring (zero blast radius)
- ✅ Jane Street aligned (CYC ≤8 target)

**Next Phase**: Proceed to Phase 2 (Architecture Planning) to design specific extraction patterns.
