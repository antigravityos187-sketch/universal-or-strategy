# Phase 2: Architecture Plan - EPIC-CCN-079

## Method Under Refactoring

**Target Method**: CreateSection0_Identity
**File**: src/V12_002.UI.Panel.Construction.cs
**Lines**: 511-665 (154 LOC)
**Current Complexity**: CYC = 13
**Target Complexity**: CYC ≤ 8 (Jane Street strict standard)

## 1. Extraction Strategy

### Current State Analysis

The CreateSection0_Identity method constructs the "SECTION 0: IDENTITY" UI panel with three main responsibilities:

1. **Leader Account Row** (lines 518-545): Creates a Grid with status LED and leader account combo box
2. **Fleet Selection Button** (lines 547-567): Creates the button that triggers fleet account selection
3. **Fleet Popup** (lines 569-665): Builds the popup containing checkboxes for fleet account selection with event handlers

**Complexity Breakdown**:
- Main orchestration: ~3 branches
- Leader account row: ~2 branches
- Fleet button creation: ~1 branch
- Fleet popup with event handlers: ~8-9 branches (nested conditionals in event handlers)

### Proposed Extraction

Extract **3 helper methods** to reduce complexity from CYC=13 to CYC≤8:

1. **CreateLeaderAccountRow()** → CYC ~2
2. **CreateFleetSelectionButton()** → CYC ~1
3. **CreateFleetPopup()** → CYC ~8

**Post-Extraction Main Method**: CYC ~3 (orchestration only)

### Rationale

- **Single Responsibility**: Each helper method handles one UI component
- **Cognitive Simplicity**: Each method is independently understandable
- **Testability**: Extracted methods can be unit tested in isolation
- **Maintainability**: Clear separation of concerns for future modifications

## 2. Method Signatures

### Original Method

private Border CreateSection0_Identity()

**Returns**: Border containing the complete Section 0 UI
**Parameters**: None (uses class fields)
**Access**: private

### Proposed Helper Methods

#### Helper 1: CreateLeaderAccountRow

private Grid CreateLeaderAccountRow()

**Purpose**: Creates the leader account display row with status LED
**Returns**: Grid containing status LED and leader account combo box
**Parameters**: None (accesses class fields: hubStatusLed, leaderAccountCombo, Account)
**Complexity**: CYC ~2
**LOC**: ~35 lines

**Responsibilities**:
- Create Grid with 2 columns (Auto, Star)
- Initialize hubStatusLed Border (status indicator)
- Create leaderAccountCombo ComboBox with account name
- Configure layout and styling

#### Helper 2: CreateFleetSelectionButton

private Button CreateFleetSelectionButton()

**Purpose**: Creates the fleet account selection button
**Returns**: Button configured for fleet selection
**Parameters**: None (accesses class fields: fleetSelectButton)
**Complexity**: CYC ~1
**LOC**: ~20 lines

**Responsibilities**:
- Initialize fleetSelectButton with styling
- Set button content, fonts, colors
- Configure layout properties

#### Helper 3: CreateFleetPopup

private Popup CreateFleetPopup()

**Purpose**: Builds the fleet account selection popup with checkboxes
**Returns**: Popup containing account selection UI
**Parameters**: None (accesses class fields: fleetPopup, fleetCheckboxPanel, selectedFleetAccounts, activeFleetAccounts)
**Complexity**: CYC ~8
**LOC**: ~95 lines

**Responsibilities**:
- Create popup border and container
- Add "Select ALL" checkbox with event handlers
- Iterate through fleet accounts and create individual checkboxes
- Wire up Checked/Unchecked event handlers
- Initialize selectedFleetAccounts collection

## 3. Call Graph

### Before Extraction

CreateSection0_Identity (CYC=13)
├── CreateSectionBorder()
├── CreateSectionHeader()
├── CreateCombo()
├── GetFleetAccountsSnapshot()
├── UpdateFleetButtonText()
└── PanelCommand()

### After Extraction

CreateSection0_Identity (CYC=3)
├── CreateSectionBorder()
├── CreateSectionHeader()
├── CreateLeaderAccountRow() (CYC=2)
│   └── CreateCombo()
├── CreateFleetSelectionButton() (CYC=1)
└── CreateFleetPopup() (CYC=8)
    ├── GetFleetAccountsSnapshot()
    ├── UpdateFleetButtonText()
    └── PanelCommand()

### Data Flow

**Shared State** (Class Fields):
- hubStatusLed (Border) - initialized in CreateLeaderAccountRow
- leaderAccountCombo (ComboBox) - initialized in CreateLeaderAccountRow
- fleetSelectButton (Button) - initialized in CreateFleetSelectionButton
- fleetPopup (Popup) - initialized in CreateFleetPopup
- fleetCheckboxPanel (StackPanel) - initialized in CreateFleetPopup
- selectedFleetAccounts (List<string>) - modified in CreateFleetPopup event handlers
- activeFleetAccounts (Dictionary) - read in CreateFleetPopup

**No Parameters Passed**: All helpers access class-level fields directly (standard WPF pattern for UI construction)

**Return Values**: Each helper returns its constructed UI element, which is added to the parent container in the main method

## 4. Lock-Free Validation

### Analysis

✅ **No lock() statements present** in the method or proposed extractions

✅ **UI Thread Safety**: This is WPF UI construction code that runs exclusively on the UI thread (single-threaded by design)

✅ **Event Handlers**: All event handlers (Checked/Unchecked) execute on the UI thread via WPF dispatcher

✅ **Collection Access**: selectedFleetAccounts is modified only in UI thread event handlers (no concurrent access)

✅ **PanelCommand() Routing**: The PanelCommand() method should route through FSM/Actor Enqueue pattern (verify in implementation phase)

### Potential Concerns

⚠️ **selectedFleetAccounts Collection**: Currently a List<string> modified in event handlers. If accessed from non-UI threads, should use:
- ConcurrentBag<string> or
- Interlocked operations or
- FSM/Actor message passing

**Recommendation**: Verify selectedFleetAccounts access patterns in Phase 3. If only UI thread access, current implementation is safe.

### FSM/Actor Pattern Compliance

The method uses PanelCommand("TOGGLE_ACCOUNT|...") to communicate state changes, which should route through the FSM/Actor pattern. This is correct - UI events trigger commands rather than directly mutating shared state.

## 5. Jane Street Compliance

### Cognitive Simplicity ✅

**Principle**: "Make illegal states unrepresentable"

- **Type Safety**: WPF type system enforces valid UI hierarchies at compile time
- **Single Responsibility**: Each helper method has one clear purpose
- **Low Complexity**: Target CYC ≤8 per method (strict Jane Street standard)
- **Readable**: Method names clearly describe their purpose

### HFT System Constraints ✅

**Not Applicable**: This is UI construction code (one-time initialization), not hot-path trading logic

- **No Performance Impact**: UI construction happens once at panel creation
- **No Latency Concerns**: Not in microsecond-critical execution path
- **No Lock Contention**: Single-threaded UI operations

### Testing Strategy (Jane Street Aligned)

**From Jane Street KB** (will_wilson_why_testing_hard_2026):
- Test behavior, not implementation details
- Focus on observable outcomes
- Avoid brittle tests tied to internal structure

**Proposed Tests**:
1. **Unit Test**: Verify each helper method returns correct UI element type
2. **Integration Test**: Verify CreateSection0_Identity assembles components correctly
3. **Behavioral Test**: Verify fleet selection updates selectedFleetAccounts
4. **Regression Test**: Verify no visual changes (screenshot comparison)

### Extraction Pattern Alignment

**Jane Street Principle**: Incremental refactoring with behavior preservation

✅ **Single Method Focus**: Only refactoring CreateSection0_Identity (no scope creep)
✅ **Behavior Preservation**: Extracted methods maintain identical functionality
✅ **Incremental Verification**: Each extraction can be tested independently
✅ **Rollback Safety**: Git restore single file if issues arise

## 6. Implementation Sequence

### Step 1: Extract CreateLeaderAccountRow
- Move lines 518-545 to new private method
- Replace with method call in main method
- Run complexity_audit.py (expect CYC reduction to ~11)
- Run build_readiness.ps1 (verify zero errors)
- Commit with message: "EPIC-CCN-079: Extract CreateLeaderAccountRow (CYC 13→11)"

### Step 2: Extract CreateFleetSelectionButton
- Move lines 547-567 to new private method
- Replace with method call in main method
- Run complexity_audit.py (expect CYC reduction to ~10)
- Run build_readiness.ps1 (verify zero errors)
- Commit with message: "EPIC-CCN-079: Extract CreateFleetSelectionButton (CYC 11→10)"

### Step 3: Extract CreateFleetPopup
- Move lines 569-665 to new private method
- Replace with method call in main method
- Run complexity_audit.py (expect CYC reduction to ~3)
- Run build_readiness.ps1 (verify zero errors)
- Commit with message: "EPIC-CCN-079: Extract CreateFleetPopup (CYC 10→3)"

### Step 4: Final Verification
- Run deploy-sync.ps1 (sync NinjaTrader hard links)
- Run pre_push_validation.ps1 -Fast
- Verify zero Codacy violations
- Test in NinjaTrader (F5 load, verify UI renders correctly)

## 7. Success Criteria

- [ ] Main method complexity reduced from CYC=13 to CYC≤8
- [ ] Each helper method has CYC≤8
- [ ] Zero compilation errors
- [ ] Zero test failures
- [ ] Zero new Codacy violations
- [ ] UI renders identically in NinjaTrader
- [ ] No lock() statements introduced
- [ ] ASCII-only compliance maintained
- [ ] Hard links synchronized via deploy-sync.ps1

## 8. Risk Mitigation

### Low Risk Factors
- UI construction code (not hot-path)
- Single-threaded execution (UI thread only)
- No shared state mutations outside UI thread
- Clear extraction boundaries
- Incremental verification at each step

### Rollback Plan
git restore src/V12_002.UI.Panel.Construction.cs
powershell -File .\deploy-sync.ps1

## 9. Phase 2 Approval

**Status**: READY FOR DIRECTOR REVIEW

**Deliverables**:
- ✅ Extraction strategy defined
- ✅ Helper method signatures specified
- ✅ Call graph documented (before/after)
- ✅ Lock-free validation completed
- ✅ Jane Street compliance verified
- ✅ Implementation sequence planned
- ✅ Success criteria defined

**Next Phase**: Phase 3 - Implementation (requires Director approval)

**Estimated Effort**: 30-45 minutes (3 extractions + verification)

**Confidence Level**: HIGH (clear boundaries, low risk, incremental approach)
