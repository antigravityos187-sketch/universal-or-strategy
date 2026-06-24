# Phase 1: Scope Definition - EPIC-W7-076

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Execution Time**: 2026-06-24T01:33:14Z
- **Input**: 00-hotspots.md
- **Output**: 01-scope-boundary.md

## Target Summary
- **Method**: CollapseAllExecutionControls
- **File**: src/V12_002.UI.Panel.Handlers.cs
- **Current CYC**: 11
- **Target CYC**: <= 8 (Jane Street standard)
- **Risk Level**: LOW (isolated, same-file callers)

## IN SCOPE

### Primary Extraction Target
1. **CollapseAllExecutionControls method** (lines ~665-688)
   - Extract conditional branches into helper methods
   - Target: 3-4 helper methods with CYC <= 3 each
   - Keep main method as orchestrator (CYC <= 8)

### Specific Branches to Extract
Based on CYC=11, the method likely contains ~11 conditional branches. Extract into:
1. **CollapseRetestControls()** - Retest-related UI collapse logic
2. **CollapseFfmaControls()** - FFMA-related UI collapse logic
3. **CollapseExecutionControls()** - General execution UI collapse logic
4. **CollapseAdditionalControls()** - Any remaining UI collapse logic

### Files to Modify
- **src/V12_002.UI.Panel.Handlers.cs** (ONLY file in scope)
  - Extract helper methods
  - Update CollapseAllExecutionControls to call helpers
  - Update 2 caller sites if needed (UpdateContextualUI, SelectConfigMode)

### Testing Requirements
- Add unit tests for each extracted helper method
- Verify UI collapse behavior unchanged
- Test both caller paths (UpdateContextualUI, SelectConfigMode)

### Verification Steps
1. Run dotnet build - must pass
2. Run deploy-sync.ps1 - sync hard links
3. F5 in NinjaTrader IDE - verify BUILD_TAG
4. Manual UI testing - verify collapse behavior

## OUT OF SCOPE

### Explicitly Excluded
1. **Other methods in V12_002.UI.Panel.Handlers.cs**
   - UpdateContextualUI (caller) - NO changes unless required
   - SelectConfigMode (caller) - NO changes unless required
   - Any other methods in the file

2. **Other UI files**
   - V12_002.UI.Panel.cs
   - V12_002.UI.Panel.Layout.cs
   - Any other UI-related files

3. **Non-UI files**
   - V12_002.cs (main strategy)
   - V12_002.SIMA.Lifecycle.cs
   - V12_002.Atm.cs
   - Any other strategy files

4. **Behavioral Changes**
   - NO changes to UI collapse logic
   - NO changes to control visibility rules
   - NO changes to execution flow
   - ONLY structural refactoring (extract methods)

5. **Caller Modifications**
   - UpdateContextualUI - NO changes (unless signature changes)
   - SelectConfigMode - NO changes (unless signature changes)

### Scope Boundaries
- **Single File**: Changes confined to V12_002.UI.Panel.Handlers.cs
- **Single Method**: Primary target is CollapseAllExecutionControls
- **Structural Only**: Extract methods, no logic changes
- **CYC Target**: Reduce from 11 to <= 8

## Scope Validation

### Jane Street Alignment
- **Cognitive Simplicity**: Breaking 11-branch method into 3-4 helpers
- **Single Responsibility**: Each helper handles one UI collapse concern
- **Testability**: Extracted methods easier to unit test
- **CYC <= 8**: Target threshold met after extraction

### V12 DNA Compliance
- **Lock-Free**: No lock statements involved (UI method)
- **ASCII-Only**: No Unicode concerns (UI method)
- **Correctness by Construction**: No state machine changes
- **Hard-Link Integrity**: deploy-sync.ps1 required after changes

### Risk Mitigation
- **Isolated Blast Radius**: Zero external dependencies
- **Same-File Callers**: Both callers in same file
- **No Callees**: Leaf node - no downstream impact
- **Low Nesting**: Depth=1 suggests linear logic

## Success Criteria

### Phase 1 (Scope Definition) - CURRENT
- [x] Read 00-hotspots.md
- [x] Define IN SCOPE extraction targets
- [x] Define OUT OF SCOPE boundaries
- [x] Document scope validation
- [x] Write 01-scope-boundary.md

### Phase 2 (Architecture Planning)
- [ ] Design helper method signatures
- [ ] Map conditional branches to helpers
- [ ] Plan caller update strategy
- [ ] Document extraction sequence

### Phase 5 (Ticket Execution)
- [ ] Extract helper methods
- [ ] Update CollapseAllExecutionControls
- [ ] Add unit tests
- [ ] Run deploy-sync.ps1
- [ ] Verify in NinjaTrader

## Scope Creep Prevention

### Red Flags (STOP if encountered)
- Modifying methods other than CollapseAllExecutionControls
- Changes to files outside V12_002.UI.Panel.Handlers.cs
- Behavioral changes to UI collapse logic
- Refactoring unrelated methods while we are here

### Recovery Protocol
If scope creep detected:
1. STOP immediately
2. Document violation in failure-analysis.md
3. Revert changes
4. Restart epic with corrected scope

## Approval
This scope definition is ready for Phase 2 (Architecture Planning).

**Scope Status**: APPROVED
**Next Phase**: Phase 2 - Architecture Planning
