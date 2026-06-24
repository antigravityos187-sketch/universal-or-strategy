# Phase 1: Scope Boundary - EPIC-W7-079

## Agent Tracking
- **Agent Name**: plan mode
- **Phase**: 1 (Scope Definition)
- **Execution Time**: 2026-06-24T01:49:37Z

## Epic Summary
**Target**: CreateSection0_Identity method in V12_002.UI.Panel.Construction.cs
**Current CYC**: 12 (exceeds Jane Street threshold of 8)
**Goal**: Reduce complexity to ≤8 through surgical extraction

## IN SCOPE

### Primary Extraction Targets
1. **Fleet Account Combo Box Creation**
   - Lines involving CreateCombo for fleet accounts
   - Fleet account snapshot retrieval logic
   - Fleet button text update logic
   - Rationale: Self-contained UI element with clear boundaries

2. **Identity TextBox Creation**
   - TextBox creation for identity fields
   - Keyboard handler attachment
   - Rationale: Repeatable pattern, can be extracted to helper

3. **Button Creation Logic**
   - CreateButton calls with PanelCommand handlers
   - Button layout and positioning
   - Rationale: Standardized UI element creation

4. **Section Border and Header**
   - CreateSectionBorder and CreateSectionHeader calls
   - Section layout initialization
   - Rationale: Reusable section setup pattern

### Complexity Reduction Strategy
- Extract 3-4 helper methods to reduce CYC from 12 to ≤8
- Each helper should have CYC ≤3
- Maintain single entry point (CreateSection0_Identity)
- Preserve existing UI behavior exactly

### Success Criteria
- Main method CYC reduced to ≤8
- All extracted methods have CYC ≤3
- Zero behavioral changes (UI looks/acts identical)
- Build passes after extraction
- F5 in NinjaTrader successful

## OUT OF SCOPE

### Explicitly Excluded
1. **Core Trading Logic**
   - FSM operations (Enqueue, IsActorThread, TryDrain)
   - Order management
   - Position tracking
   - Rationale: High-risk, outside UI construction domain

2. **External Dependencies**
   - Changes to V12_002.cs (main strategy file)
   - Changes to V12_002.UI.Panel.Helpers.cs
   - Changes to V12_002.UI.IPC.cs
   - Rationale: Zero blast radius must be maintained

3. **Behavioral Changes**
   - UI layout modifications
   - Event handler logic changes
   - Control flow alterations
   - Rationale: Surgical refactoring only, no feature work

4. **Other Panel Sections**
   - CreateSection1_* methods
   - CreateSection2_* methods
   - Other UI construction methods
   - Rationale: One epic = one method

5. **Test Coverage Expansion**
   - New unit tests (beyond verification tests)
   - Integration test modifications
   - Rationale: Focus on complexity reduction, not test expansion

## Risk Mitigation

### Low-Risk Factors (Confirmed)
- ✅ Zero blast radius (no external callers)
- ✅ Single entry point (CreatePanel only)
- ✅ UI code (non-critical path)
- ✅ Low churn (stable code)

### Medium-Risk Factors (Managed)
- ⚠️ High fan-out (38 callees) - Mitigated by preserving all calls
- ⚠️ CYC=12 - Addressed by extraction strategy

### Constraints
1. **No Whitespace Mutation**: Preserve formatting in unchanged code
2. **ASCII-Only**: All new code must be ASCII-compliant
3. **Lock-Free**: No lock() blocks (not applicable to UI code)
4. **Hard-Link Sync**: Run deploy-sync.ps1 after changes

## Extraction Candidates

### Candidate 1: CreateFleetAccountCombo
**Lines**: ~20-30 lines
**CYC Estimate**: 2-3
**Extracts**: Fleet account combo box creation, snapshot retrieval, button text update

### Candidate 2: CreateIdentityTextBox
**Lines**: ~15-20 lines
**CYC Estimate**: 1-2
**Extracts**: TextBox creation with keyboard handlers

### Candidate 3: CreateIdentityButton
**Lines**: ~10-15 lines
**CYC Estimate**: 1
**Extracts**: Button creation with PanelCommand handler

### Candidate 4: InitializeSection0Layout
**Lines**: ~10-15 lines
**CYC Estimate**: 1
**Extracts**: Section border and header setup

## Verification Plan

### Pre-Extraction
1. Run complexity audit: `python scripts/complexity_audit.py`
2. Verify build: `dotnet build`
3. Capture baseline CYC: 12

### Post-Extraction
1. Verify CYC ≤8 for main method
2. Verify CYC ≤3 for all extracted methods
3. Run build: `dotnet build`
4. Run deploy-sync: `powershell -File .\deploy-sync.ps1`
5. F5 in NinjaTrader IDE
6. Verify BUILD_TAG appears in output

## Next Steps (Phase 2)
1. Architecture planning: Define exact method signatures
2. Determine extraction order (dependencies first)
3. Plan ticket breakdown (1 ticket per extraction)
4. Generate Mermaid diagrams for call flow

---
**Phase 1 Status**: ✅ COMPLETED
**Generated**: 2026-06-24T01:49:37Z
**Scope**: LOCKED (no scope creep allowed)
