# Phase 1: Scope Definition - EPIC-W7-045

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.18
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T19:29:10Z
- **Input Artifact**: docs/brain/EPIC-W7-045/00-hotspots.md

## Epic Objective
Reduce cyclomatic complexity of OnKeyDown method from CYC=9 to CYC<=8 through surgical extraction of conditional logic.

## Target Method
- **Method**: OnKeyDown
- **File**: src/V12_002.UI.Callbacks.cs
- **Line**: 391
- **Current CYC**: 9
- **Target CYC**: <=8
- **Lines of Code**: 36

## IN SCOPE

### Primary Extraction Target
OnKeyDown method body (lines 391-427 in V12_002.UI.Callbacks.cs)
- Extract key validation logic to IsValidKeyCommand()
- Extract command lookup logic to GetKeyCommand()
- Extract action dispatch logic to DispatchKeyAction()

### Allowed Modifications
1. Method Signature: Preserve exactly (NinjaTrader framework contract)
   - protected override void OnKeyDown(object sender, KeyEventArgs e)
   - No parameter changes allowed (UI callback contract)

2. New Private Helper Methods (same file):
   - IsValidKeyCommand(KeyEventArgs e) - Validate key input
   - GetKeyCommand(KeyEventArgs e) - Lookup command from _keyCommands
   - DispatchKeyAction(string command) - Route to HandleTargetAction/HandleRunnerAction

3. Existing Method Calls (preserve):
   - _keyCommands dictionary lookup
   - HandleTargetAction() calls
   - HandleRunnerAction() calls

### Success Criteria
- OnKeyDown CYC reduced from 9 to <=8
- All extracted methods have CYC <=8
- Zero functional changes (behavior preservation)
- All existing tests pass
- Build succeeds with zero errors

## OUT OF SCOPE

### Explicitly Excluded
1. Downstream Methods (do NOT modify):
   - HandleTargetAction() - Separate epic if needed
   - HandleRunnerAction() - Separate epic if needed
   - ExecuteTargetAction() - Separate epic if needed
   - Enqueue() - Actor pattern core, do NOT touch

2. Dictionary Structure:
   - _keyCommands dictionary - Do NOT modify structure
   - Key mappings - Do NOT change existing mappings

3. Other UI Callbacks:
   - OnMouseDown() - Separate epic
   - OnMouseMove() - Separate epic
   - Other keyboard handlers - Separate epic

4. Thread Safety Logic:
   - Actor pattern Enqueue calls - Do NOT modify
   - IsActorThread checks - Do NOT modify
   - Queue draining logic - Do NOT modify

5. Logging Infrastructure:
   - LogBuffer.Format calls - Preserve as-is
   - Log message content - Do NOT change

### Boundary Enforcement
STOP at method boundaries:
- If HandleTargetAction has high complexity, create separate epic
- If HandleRunnerAction has high complexity, create separate epic
- Do NOT cascade refactoring beyond OnKeyDown

STOP at framework contracts:
- Do NOT change method signature (NinjaTrader override)
- Do NOT change event handler behavior
- Do NOT modify KeyEventArgs handling

## Risk Mitigation

### Zero Blast Radius Confirmed
- No importers (framework-invoked only)
- No internal callers detected
- Changes isolated to OnKeyDown method body
- Safe to refactor without downstream impact

### Testing Requirements
1. Manual Testing (NinjaTrader IDE):
   - F5 compile and load
   - Test all keyboard shortcuts
   - Verify target actions (T, Shift+T, etc.)
   - Verify runner actions (R, Shift+R, etc.)

2. Regression Testing:
   - Existing unit tests must pass
   - No new compilation errors
   - BUILD_TAG verification

### Rollback Plan
- Git revert if CYC not reduced to <=8
- Git revert if any functional regression detected
- Git revert if build fails

## Extraction Strategy

### Phase 1: Extract Validation
Extract key validation logic
Expected CYC: 2-3

### Phase 2: Extract Lookup
Extract _keyCommands dictionary lookup
Expected CYC: 2-3

### Phase 3: Extract Dispatch
Extract HandleTargetAction/HandleRunnerAction routing
Expected CYC: 2-3

### Expected Outcome
- OnKeyDown: CYC 9 to 3 (orchestration only)
- IsValidKeyCommand: CYC <=3
- GetKeyCommand: CYC <=3
- DispatchKeyAction: CYC <=3
- Total complexity preserved, distribution improved

## Scope Boundary Validation

### Jane Street Alignment
- Single responsibility (keyboard event handling)
- Cognitive simplicity (CYC <=8 per method)
- Testability (smaller units)
- No illegal states (preserve framework contract)

### V12 DNA Compliance
- ASCII-only (no Unicode in extracted code)
- Lock-free (no new locks introduced)
- Correctness by construction (preserve event handler contract)
- Hard-link integrity (deploy-sync.ps1 after changes)

## Conclusion

Scope Status: VALIDATED
- Narrow surgical extraction (OnKeyDown only)
- Zero blast radius (no external dependencies)
- Clear boundary (stop at method calls)
- Low-medium risk (UI callback, stable code)
- Expected CYC reduction: 9 to <=8

Ready for Phase 2: Architecture Planning
