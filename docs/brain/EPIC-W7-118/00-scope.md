# Phase 1: Scope Definition - EPIC-W7-118

## Agent Tracking
- Agent Name: v12-phase1-scope
- Bobcoins Used: TBD
- API Key: jCodemunch MCP
- Execution Time: 2026-06-24T19:39:59Z
- Input: docs/brain/EPIC-W7-118/00-hotspots.md

## Target Method
- Method: DeserializeSnapshot
- File: src/V12_002.StickyState.cs
- Line: 441
- Current CYC: 8 (at Jane Street threshold)
- Max Nesting Depth: 7 (high - primary concern)

## Scope Boundary Definition

### IN SCOPE

#### Primary Target
1. DeserializeSnapshot method (src/V12_002.StickyState.cs:441)
   - Extract nested JSON parsing logic to reduce nesting depth from 7 to 4 or less
   - Maintain CYC 8 or less (currently at threshold)
   - Preserve all existing functionality

#### Extraction Candidates
1. Nested try-catch blocks (lines approximately 460-500)
   - Extract JSON field parsing logic to helper methods
   - Group related field parsing (SIMA fields, order fields, state fields)

2. Conditional parsing logic (lines approximately 470-490)
   - Extract conditional field parsing to dedicated methods
   - Reduce nesting depth while maintaining error handling

#### Dependencies (Read-Only)
1. ParseJsonLong (src/V12_002.StickyState.cs:514) - existing helper
2. ParseJsonString (src/V12_002.StickyState.cs:564) - existing helper
3. ParseJsonInt (src/V12_002.StickyState.cs:539) - existing helper
4. ParseJsonBool (src/V12_002.StickyState.cs:544) - existing helper
5. LogBuffer.Format (src/V12_002.Perf.LogBuffer.cs:28) - logging

#### Callers (Verification Only)
1. LoadStateSnapshot (src/V12_002.StickyState.cs:153)
2. RollbackToLastGoodState (src/V12_002.StickyState.cs:258)
3. LoadStickyState (src/V12_002.StickyState.cs:369)

### OUT OF SCOPE

#### Explicitly Excluded
1. Caller methods - No modifications to LoadStateSnapshot, RollbackToLastGoodState, or LoadStickyState
2. Helper methods - No changes to ParseJson methods (already optimal)
3. LogBuffer - No changes to logging infrastructure
4. Other StickyState methods - Only DeserializeSnapshot is targeted
5. Cross-file changes - All work contained in V12_002.StickyState.cs

#### Deferred to Future Epics
1. Serialization logic - SerializeSnapshot method (if exists) is separate epic
2. State management refactoring - Broader StickyState architecture changes
3. Error handling strategy - Global error handling patterns

## Scope Rationale

### Why This Scope?
1. Isolated Impact: Zero external importers = minimal blast radius
2. Preventive Maintenance: CYC=8 (at threshold), nesting=7 (high)
3. Clear Boundary: Single method extraction, no cascading changes
4. Low Risk: Stable method (low churn, not in top 50 hotspots)

### Scope Constraints
1. Preserve Behavior: All existing functionality must remain identical
2. Maintain CYC 8 or less: Do not increase complexity during extraction
3. Reduce Nesting: Target nesting depth 4 or less (from current 7)
4. No API Changes: Method signature remains unchanged

## Success Criteria

### Functional Requirements
- DeserializeSnapshot maintains identical behavior
- All 3 callers continue to work without modification
- All JSON fields parsed correctly (no data loss)
- Error handling preserved (try-catch logic intact)

### Quality Requirements
- Cyclomatic Complexity 8 or less (maintain or reduce)
- Max Nesting Depth 4 or less (reduce from 7)
- All extracted methods have CYC 8 or less
- Code passes CSharpier formatting
- Build passes (dotnet build)

### Testing Requirements
- Unit tests for extracted methods
- Integration test: LoadStateSnapshot works
- Integration test: RollbackToLastGoodState works
- Integration test: LoadStickyState works

## Extraction Strategy

### Approach
1. Group Related Parsing: Extract JSON field parsing by logical groups
2. Preserve Error Handling: Maintain try-catch structure in extracted methods
3. Single Responsibility: Each extracted method handles one logical group
4. Minimal Disruption: No changes to method signature or callers

### Estimated Extractions
- 2-3 helper methods for grouped JSON field parsing
- Target: Reduce nesting from 7 to 4 or less
- Maintain: CYC 8 or less for all methods

## Risk Mitigation

### Low Risk Factors
- Zero external importers (isolated change)
- Stable method (low churn)
- Well-defined dependencies (utility methods only)
- Clear extraction boundaries

### Mitigation Strategies
- Comprehensive unit tests for extracted methods
- Integration tests for all 3 callers
- Pre-push validation (13 checks)
- F5 verification in NinjaTrader

## Scope Approval

Status: APPROVED
Rationale: Clear boundaries, low risk, preventive maintenance
Next Phase: Phase 2 (Architecture Planning)
