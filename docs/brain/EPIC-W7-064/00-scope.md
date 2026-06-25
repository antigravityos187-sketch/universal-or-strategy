# Phase 1: Scope Definition - EPIC-W7-064

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.00
- **API Key**: jCodemunch MCP
- **Execution Time**: ~10 seconds

## Target Method
- **Method**: ResolveFsm_ByScan
- **File**: src/V12_002.Symmetry.BracketFSM.cs
- **Line**: 209
- **Current CYC**: 11
- **Target CYC**: ≤8 (Jane Street threshold)

## Scope Boundary Definition

### IN SCOPE
1. **Primary Target**: ResolveFsm_ByScan method body (lines 209-247)
   - Extract nested conditional logic to helper methods
   - Reduce cyclomatic complexity from 11 to ≤8
   - Maintain existing method signature (2 parameters)

2. **Extraction Candidates**:
   - Nested conditionals (max nesting depth 4)
   - FSM state validation logic
   - Bracket matching logic
   - Event type filtering

3. **Callers to Preserve**:
   - ResolveFsmFromEvent (line 251) - direct caller
   - ValidateFsmEventPreconditions (line 272) - indirect caller

4. **Success Criteria**:
   - ResolveFsm_ByScan CYC reduced to ≤8
   - All extracted methods have CYC ≤8
   - Zero compilation errors
   - Existing callers function correctly
   - Unit tests pass

### OUT OF SCOPE
1. **Caller Methods**: Do NOT modify ResolveFsmFromEvent or ValidateFsmEventPreconditions
2. **Method Signature**: Do NOT change ResolveFsm_ByScan parameters or return type
3. **Other Files**: Do NOT modify any files outside src/V12_002.Symmetry.BracketFSM.cs
4. **Unrelated Methods**: Do NOT refactor other methods in the file
5. **Business Logic**: Do NOT alter FSM resolution behavior or semantics

## Risk Mitigation

### Zero Blast Radius Advantage
- **Importer Count**: 0 (no external dependencies)
- **Risk Score**: 0.0 (lowest possible)
- **Impact**: Changes isolated to single file
- **Callers**: Only 2, both in same file

### Constraints
- Maintain exact same behavior for callers
- Preserve method signature (2 parameters)
- Keep all extracted methods private
- No changes to public API surface

## Extraction Strategy

### Complexity Reduction Plan
1. **Current**: CYC 11, max nesting 4
2. **Target**: CYC ≤8, max nesting ≤3
3. **Method**: Extract nested conditionals to helper methods
4. **Validation**: Each helper must have CYC ≤8

### Helper Method Guidelines
- All helpers must be private
- Clear, descriptive names
- Single responsibility
- CYC ≤8 per method
- No side effects

## Verification Checklist
- [ ] ResolveFsm_ByScan CYC ≤8
- [ ] All extracted methods CYC ≤8
- [ ] Zero compilation errors
- [ ] ResolveFsmFromEvent still works
- [ ] ValidateFsmEventPreconditions still works
- [ ] Unit tests pass
- [ ] deploy-sync.ps1 executed
- [ ] F5 in NinjaTrader successful

## Next Phase
Proceed to Phase 1.5 (Scope Boundary Validation) to verify:
1. Scope boundaries are clear and enforceable
2. No scope creep risks identified
3. Extraction plan is feasible
4. Success criteria are measurable
