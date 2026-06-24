# Phase 1: Scope Boundary - EPIC-W7-012

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Execution Time**: 2026-06-24T01:30:01Z
- **Mode**: plan

## Epic Summary
**Target**: SyncPanelConfigFromSnapshot (CYC 19 -> Target <=8)
**File**: src/V12_002.UI.Panel.StateSync.cs:460
**Risk Level**: LOW-MEDIUM (zero blast radius, single caller)

## Scope Definition

### IN SCOPE

#### Primary Target
- **SyncPanelConfigFromSnapshot** (CYC 19, 53 lines)
  - Extract conditional validation logic
  - Extract UI update groupings
  - Reduce nesting via early returns
  - Target: CYC <=8 per extracted method

#### Extraction Candidates (from call hierarchy)
1. **Panel Value Formatting Logic**
   - FormatPanelDouble calls (2 occurrences)
   - Extract to: ValidateAndFormatPanelValues()

2. **Combo Selection Logic**
   - SetComboSelection calls
   - GetPanelTargetModeText calls
   - Extract to: SyncComboSelections()

3. **Visual State Updates**
   - SyncCountChipVisuals
   - UpdateTargetVisibility
   - Extract to: SyncVisualElements()

4. **Control State Management**
   - UpdateConfigControlsEnabled
   - UpdateConfigRowsVisibility
   - UpdateLiveButtonsVisibility
   - Extract to: SyncControlStates()

5. **Button Visibility Logic**
   - SetT1ButtonVisible
   - SetT2T5ButtonsVisible
   - Extract to: SyncButtonVisibility()

#### Testing Requirements
- Unit tests for each extracted method
- Integration test for UpdatePanelState -> SyncPanelConfigFromSnapshot flow
- Verify UI state consistency after refactoring

### OUT OF SCOPE

#### Caller Method
- **UpdatePanelState** (src/V12_002.UI.Panel.StateSync.cs:13)
  - Reason: Not the complexity hotspot
  - Action: Leave unchanged, only verify integration

#### Callee Methods (15 total)
All 15 methods called by SyncPanelConfigFromSnapshot are OUT OF SCOPE:
1. FormatPanelDouble (Construction.cs:1506)
2. SetComboSelection (Construction.cs:1471)
3. GetPanelTargetModeText (Construction.cs:1489)
4. SyncCountChipVisuals (StateSync.cs:410)
5. UpdateTargetVisibility (Handlers.cs:755/792)
6. UpdateConfigControlsEnabled (Handlers.cs:801)
7. UpdateConfigRowsVisibility (Handlers.cs:823)
8. UpdateLiveButtonsVisibility (Handlers.cs:838)
9. SetT1ButtonVisible (Handlers.cs:849)
10. SetT2T5ButtonsVisible (Handlers.cs:857)

**Rationale**: These are helper methods with their own complexity profiles. Refactoring them would expand scope beyond EPIC-W7-012 mandate.

#### Other UI Panel Files
- V12_002.UI.Panel.Construction.cs
- V12_002.UI.Panel.Handlers.cs
- Reason: Not part of this epic hotspot target

#### Top 10 Hotspots
All top 10 hotspots in the codebase are OUT OF SCOPE:
- HydrateFromOpenPositions (CYC 34)
- IsCommandForThisInstrument (CYC 38)
- HandleTerminated (CYC 30)
- SweepBrokerOrders (CYC 28)
- etc.

**Rationale**: Each hotspot requires its own dedicated epic.

### Scope Boundary Validation

#### Jane Street Alignment
- **Cognitive Simplicity**: Extract to CYC <=8 per method
- **Single Responsibility**: Each extracted method has one clear purpose
- **Testability**: Each extraction is independently testable

#### V12 DNA Compliance
- **Lock-Free**: No lock() blocks in target method
- **ASCII-Only**: No Unicode concerns in UI sync logic
- **Correctness by Construction**: UI state updates are idempotent

#### Blast Radius Confirmation
- **Zero External Dependencies**: No other files import this method
- **Single Caller**: Only UpdatePanelState calls this method
- **UI Layer**: Not in critical trading logic path

### Success Criteria

#### Phase 1 Complete When:
- Scope boundary clearly defined (IN vs OUT)
- Extraction candidates identified (5 groups)
- Testing requirements specified
- Jane Street alignment verified
- V12 DNA compliance confirmed

#### Phase 2 Prerequisites:
- Scope boundary approved by Director
- No scope creep beyond defined boundary
- All OUT OF SCOPE items documented with rationale

## Risk Mitigation

### Scope Creep Prevention
- **ONE EPIC = ONE CONCERN**: Only SyncPanelConfigFromSnapshot
- **No While We Are Here Fixes**: Resist temptation to fix callee methods
- **Separate PRs**: If issues found in callees, create new epics

### Rollback Plan
- Single caller makes rollback trivial
- Zero blast radius ensures no ripple effects
- UI layer allows safe F5 testing in NinjaTrader

## Next Phase
**Phase 2**: Architecture Planning
- Design extraction strategy for 5 identified groups
- Define method signatures for extracted methods
- Plan testing approach
- Generate Mermaid diagrams for before/after call hierarchy

## Approval Status
**AWAITING DIRECTOR APPROVAL** for Phase 2 progression.
