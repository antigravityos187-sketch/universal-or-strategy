# Phase 1: Scope Definition - EPIC-W7-079

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.00
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T20:08:16Z

## Epic Metadata
- **Epic ID**: EPIC-W7-079
- **Target Method**: CreateSection0_Identity
- **File**: src/V12_002.UI.Panel.Construction.cs
- **Line**: 511
- **Current CYC**: 12
- **Target CYC**: ≤8
- **Lines of Code**: 195

## Scope Boundary Analysis

### IN SCOPE

#### Primary Extraction Target
**Method**: CreateSection0_Identity (CYC=12, 195 LOC)
- **Rationale**: Exceeds Jane Street threshold (CYC ≤ 8)
- **Risk Level**: LOW-MEDIUM (zero blast radius, single caller)
- **Priority**: MEDIUM (complexity reduction)

#### Extraction Candidates
Based on the call hierarchy analysis, the following UI element groups are candidates for extraction:

1. **Fleet Account Combo Creation** (Lines ~520-560)
   - CreateCombo for fleet accounts
   - GetFleetAccountsSnapshot
   - selectedFleetAccounts initialization
   - UpdateFleetButtonText
   - **Estimated CYC Reduction**: 2-3

2. **Fleet Button Creation** (Lines ~560-590)
   - CreateButton for fleet management
   - PanelCommand handler attachment
   - Button styling and positioning
   - **Estimated CYC Reduction**: 1-2

3. **Account TextBox Creation** (Lines ~590-620)
   - CreateTextBox for account input
   - Keyboard handler attachment
   - TextBox validation logic
   - **Estimated CYC Reduction**: 2-3

4. **Identity Section Layout** (Lines ~620-650)
   - Section border creation (CreateSectionBorder)
   - Section header creation (CreateSectionHeader)
   - Element positioning and spacing
   - **Estimated CYC Reduction**: 1-2

#### Scope Constraints
- **Preserve**: Single entry point (CreatePanel → CreateSection0_Identity)
- **Preserve**: Existing UI behavior and appearance
- **Preserve**: Event handler signatures and bindings
- **Maintain**: Actor/FSM pattern for command handling
- **Target**: Reduce CYC from 12 to ≤8 (minimum 4-point reduction)

### OUT OF SCOPE

#### Methods NOT to be Modified
1. **CreatePanel** (caller)
   - **Rationale**: Single caller, no complexity issues
   - **Action**: None (preserve existing call)

2. **Helper Methods** (callees)
   - CreateSectionBorder (line 1444)
   - CreateSectionHeader (line 1457)
   - CreateCombo (src/V12_002.UI.Panel.Helpers.cs, line 204)
   - CreateTextBox (src/V12_002.UI.Panel.Helpers.cs, line 67)
   - CreateButton (src/V12_002.UI.Panel.Helpers.cs, line 20)
   - **Rationale**: Already extracted, reusable helpers
   - **Action**: None (use as-is)

3. **IPC Methods** (indirect callees)
   - GetFleetAccountsSnapshot (src/V12_002.UI.IPC.cs, line 202)
   - IsFleetAccount (src/V12_002.cs, line 864)
   - **Rationale**: Cross-cutting concerns, separate responsibility
   - **Action**: None (preserve existing calls)

4. **Actor/FSM Infrastructure** (indirect callees)
   - Enqueue (src/V12_002.cs, line 428)
   - IsActorThread (src/V12_002.cs, line 439)
   - TryDrain (src/V12_002.cs, line 503)
   - ScheduleActorDrain (src/V12_002.cs, line 481)
   - **Rationale**: Core V12 DNA (lock-free pattern)
   - **Action**: None (preserve existing pattern)

#### Files NOT to be Modified
- src/V12_002.UI.Panel.Helpers.cs (helper methods already extracted)
- src/V12_002.UI.IPC.cs (IPC layer, separate concern)
- src/V12_002.cs (core strategy, separate concern)
- src/V12_002.UI.Panel.Handlers.cs (event handlers, separate concern)

#### Features NOT to be Changed
- Fleet account selection logic
- Account validation rules
- UI element styling and positioning
- Event handler behavior
- Command queue integration

## Extraction Strategy

### Approach: Vertical Slice Extraction
Extract UI element creation into focused helper methods, each with CYC ≤ 8.

### Proposed Helper Methods
1. **CreateFleetAccountCombo()** → CYC ≤ 3
2. **CreateFleetManagementButton()** → CYC ≤ 2
3. **CreateAccountInputTextBox()** → CYC ≤ 3
4. **LayoutIdentitySection()** → CYC ≤ 2

### Expected Outcome
- **Original CYC**: 12
- **Target CYC**: ≤8 (after extraction)
- **New Helper Methods**: 4
- **Total CYC Reduction**: 4-10 points

## Risk Mitigation

### Low Risk Factors
- ✅ Zero blast radius (no external dependencies)
- ✅ Single caller (CreatePanel)
- ✅ UI construction code (less critical than trading logic)
- ✅ Low churn (stable code)

### Medium Risk Factors
- ⚠️ High fan-out (38 callees) - must preserve all calls
- ⚠️ UI behavior preservation - must maintain exact appearance

### Mitigation Strategy
1. **Preserve All Callees**: No changes to helper method signatures
2. **Maintain UI State**: Preserve all variable assignments and event handlers
3. **Test After Extraction**: F5 in NinjaTrader IDE to verify UI loads correctly
4. **Incremental Extraction**: Extract one helper at a time, verify after each

## Success Criteria

### Phase 1 Completion
- ✅ Scope boundary defined (IN SCOPE vs OUT OF SCOPE)
- ✅ Extraction candidates identified (4 helper methods)
- ✅ Risk assessment completed (LOW-MEDIUM risk)
- ✅ Mitigation strategy documented

### Epic Completion (Future Phases)
- [ ] CYC reduced from 12 to ≤8
- [ ] All helper methods have CYC ≤8
- [ ] UI behavior preserved (F5 test passes)
- [ ] No regression in existing functionality
- [ ] deploy-sync.ps1 executed successfully

## Next Steps (Phase 2)
1. Architecture planning: Define helper method signatures
2. DNA audit: Verify compliance with V12 DNA mandates
3. Ticket generation: Create surgical refactoring tickets
4. Execution: Extract helpers one at a time

---
**Phase 1 Status**: ✅ COMPLETED
**Generated**: 2026-06-24T20:08:16Z
**Agent**: v12-phase1-scope
