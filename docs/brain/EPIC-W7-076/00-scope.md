# Phase 1: Scope Definition - EPIC-W7-076

## Agent Tracking
- Agent Name: v12-phase1-scope
- Bobcoins Used: 0.00
- API Key: jCodemunch MCP
- Execution Time: 2026-06-24T19:33:59Z

## Epic Overview
- Target Method: CollapseAllExecutionControls
- File: src/V12_002.UI.Panel.Handlers.cs
- Current CYC: 11
- Target CYC: 8 or less (Jane Street strict standard)
- Risk Level: LOW (isolated, same-file callers)

## Scope Boundary Definition

### IN SCOPE

#### Primary Target
- Method: CollapseAllExecutionControls() (line 665)
- Extract conditional branches into helper methods
- Reduce CYC from 11 to 8 or less
- Maintain existing functionality

#### Extraction Strategy
- Extract 3-4 helper methods from conditional branches
- Suggested helper method names:
  - CollapseRetestControls()
  - CollapseFfmaControls()
  - CollapseExecutionModeControls()
  - CollapseAdditionalControls() (if needed)

#### Caller Updates
- UpdateContextualUI (line 654) - verify call site still works
- SelectConfigMode (line 591) - verify call site still works

#### Testing Requirements
- Unit tests for each extracted helper method
- Integration test for CollapseAllExecutionControls
- Verify UI collapse behavior unchanged

#### Build and Deployment
- Run deploy-sync.ps1 after changes
- F5 in NinjaTrader IDE to verify BUILD_TAG
- Confirm no compilation errors

### OUT OF SCOPE

#### Excluded Methods
- UpdateContextualUI - caller method, not target for this epic
- SelectConfigMode - caller method, not target for this epic
- Any other methods in V12_002.UI.Panel.Handlers.cs

#### Excluded Files
- All files except src/V12_002.UI.Panel.Handlers.cs
- No changes to other UI handler files
- No changes to core strategy logic

#### Excluded Refactoring
- No changes to method signatures
- No changes to caller method logic
- No UI behavior changes
- No performance optimizations beyond complexity reduction

#### Excluded Testing
- No stress testing required (UI method)
- No performance benchmarking
- No integration tests beyond caller verification

## Extraction Boundaries

### File Boundary
- Single File: src/V12_002.UI.Panel.Handlers.cs
- No Cross-File Changes: All work contained in one compilation unit

### Method Boundary
- Primary Method: CollapseAllExecutionControls (line 665)
- Helper Methods: 3-4 new private methods (to be created)
- Caller Methods: 2 existing methods (verify only, no changes)

### Complexity Boundary
- Target: Each method CYC 8 or less
- Primary Method After: CYC 8 or less (orchestration only)
- Helper Methods: CYC 8 or less each (single responsibility)

## Success Criteria

### Functional Requirements
- CollapseAllExecutionControls CYC reduced from 11 to 8 or less
- All extracted helper methods have CYC 8 or less
- UI collapse behavior unchanged
- Both caller sites work correctly

### Quality Requirements
- Zero compilation errors
- Unit tests pass for all new methods
- deploy-sync.ps1 executes successfully
- F5 in NinjaTrader shows BUILD_TAG

### Documentation Requirements
- Method XML comments for all new helpers
- Inline comments for complex logic
- Update EPIC-W7-076 completion report

## Risk Mitigation

### Low Risk Factors
- Isolated blast radius (no external dependencies)
- Same-file callers (easy to verify)
- No callees (leaf node in call graph)
- Low nesting depth (linear logic)

### Mitigation Strategies
- Run complexity audit before and after
- Verify hard link sync with deploy-sync.ps1
- Test in NinjaTrader IDE before commit
- Keep all changes in single commit for easy rollback

## Phase 1 Completion
- Scope Defined: YES
- Boundaries Clear: YES
- Success Criteria Established: YES
- Ready for Phase 2: YES
