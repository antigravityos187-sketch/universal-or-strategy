# Phase 1: Scope Definition - EPIC-W7-069

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.00
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T19:32:46Z

## Epic Objective
Reduce cyclomatic complexity of GetFsmExpectedPosition from CYC=14 to CYC ≤ 8 (Jane Street strict standard).

## Target Method
- **Method**: GetFsmExpectedPosition
- **File**: src/V12_002.Symmetry.BracketFSM.cs
- **Line**: 422
- **Current CYC**: 14
- **Target CYC**: ≤ 8
- **Lines of Code**: 39

## IN SCOPE

### Primary Extraction Target
1. **GetFsmExpectedPosition method** (CYC=14)
   - Extract decision logic into 2-3 helper methods
   - Each helper method must have CYC ≤ 5
   - Maintain single responsibility per extracted method
   - Preserve original method signature and behavior

### Scope Boundaries
- **File Boundary**: src/V12_002.Symmetry.BracketFSM.cs ONLY
- **Method Boundary**: GetFsmExpectedPosition and its extracted helpers ONLY
- **Complexity Target**: Reduce from CYC=14 to CYC ≤ 8
- **Behavioral Constraint**: Zero functional changes (pure refactoring)

### Allowed Modifications
1. Extract helper methods within same file
2. Rename variables for clarity (if needed)
3. Add XML documentation to extracted methods
4. Reorder logic for readability (preserving behavior)

## OUT OF SCOPE

### Explicitly Excluded
1. **Other methods in V12_002.Symmetry.BracketFSM.cs**
   - Do NOT touch any other methods in the file
   - Do NOT refactor adjacent code
   
2. **Other files**
   - Do NOT modify any other .cs files
   - Do NOT change imports/usings
   
3. **Functional changes**
   - Do NOT alter method behavior
   - Do NOT add new features
   - Do NOT fix unrelated bugs
   
4. **Test modifications**
   - Do NOT modify existing tests (unless they break due to signature changes)
   - New tests are Phase 5 responsibility
   
5. **Adjacent complexity**
   - Do NOT refactor other high-CYC methods in same file
   - Stay focused on GetFsmExpectedPosition ONLY

## Scope Rationale

### Why This Scope?
1. **Isolated Impact**: Zero blast radius (no external dependencies)
2. **Clear Boundary**: Single method with well-defined complexity violation
3. **Low Risk**: No callers detected, changes are contained
4. **Measurable Goal**: CYC=14 → CYC ≤ 8 (quantifiable success)

### Risk Mitigation
- **Blast Radius**: 0 confirmed files, 0 potential files
- **Coupling**: No detected callers/callees
- **Churn**: Not in top 50 hotspots (stable code)
- **Testing**: 14 execution paths to verify (manageable)

## Extraction Strategy

### Approach
1. **Identify decision clusters**: Group related conditional logic
2. **Extract 2-3 helpers**: Each with CYC ≤ 5
3. **Preserve semantics**: Maintain exact original behavior
4. **Verify complexity**: Run complexity audit after extraction

### Expected Outcome
- **Main method**: GetFsmExpectedPosition (CYC ≤ 8)
- **Helper 1**: [TBD in Phase 2] (CYC ≤ 5)
- **Helper 2**: [TBD in Phase 2] (CYC ≤ 5)
- **Helper 3**: [TBD in Phase 2] (CYC ≤ 5) [if needed]

## Success Criteria

### Phase 1 Success
- [x] Scope clearly defined (IN SCOPE vs OUT OF SCOPE)
- [x] Boundaries explicitly stated
- [x] Rationale documented
- [x] Risk assessment included

### Epic Success (Final)
- [ ] GetFsmExpectedPosition CYC ≤ 8
- [ ] All extracted helpers CYC ≤ 5
- [ ] Zero functional changes (behavior preserved)
- [ ] Build passes
- [ ] deploy-sync.ps1 executed successfully

## Scope Validation

### Boundary Checks
- **File count**: 1 (src/V12_002.Symmetry.BracketFSM.cs)
- **Method count**: 1 primary + 2-3 extracted helpers
- **Complexity reduction**: 14 → ≤ 8 (minimum 43% reduction)
- **Blast radius**: 0 (no external impact)

### Scope Creep Prevention
- Do NOT refactor other methods "while we're here"
- Do NOT fix unrelated issues in same file
- Do NOT expand to other files
- Do NOT add new features

## Phase 1 Completion
- Scope definition complete
- Boundaries clearly defined
- IN SCOPE vs OUT OF SCOPE documented
- Risk mitigation strategy outlined

**Next Phase**: Phase 1.5 (Scope Boundary Validation)
