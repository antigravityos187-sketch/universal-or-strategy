# Phase 1: Scope Definition - EPIC-W7-120

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.18
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T19:40:22Z

## Epic Overview
- **Target Method**: HandleFsmFilled
- **File**: src/V12_002.Symmetry.BracketFSM.cs
- **Current CYC**: 14
- **Target CYC**: <=8 (Jane Street threshold)
- **Risk Level**: LOW-MEDIUM

## Scope Boundary Definition

### IN SCOPE

#### Primary Target
- **Method**: HandleFsmFilled (line 349, CYC=14)
  - Extract nested conditionals to reduce complexity
  - Break into 2-3 helper methods with CYC <=8 each
  - Preserve existing call sites

#### Extraction Strategy
1. **Conditional Logic Extraction**
   - Extract nested if/else blocks (nesting depth 3)
   - Create focused helper methods for each logical branch
   - Target: 2-3 helper methods, each CYC <=8

2. **Call Site Preservation**
   - ProcessBracketEvent (line 381) - PRESERVE
   - DrainAccountMailbox (line 88) - PRESERVE
   - No signature changes to HandleFsmFilled

3. **Testing Requirements**
   - Unit tests for extracted helper methods
   - Integration test for HandleFsmFilled
   - Verify call chain: DrainAccountMailbox -> ProcessBracketEvent -> HandleFsmFilled

### OUT OF SCOPE

#### Caller Methods (No Changes)
- **ProcessBracketEvent** (line 381, CYC unknown)
  - Rationale: Not the target of this epic
  - Action: PRESERVE as-is

- **DrainAccountMailbox** (line 88, CYC unknown)
  - Rationale: Indirect caller, not target
  - Action: PRESERVE as-is

#### Other File Methods
- All other methods in V12_002.Symmetry.BracketFSM.cs
  - Rationale: HandleFsmFilled is isolated (no external dependencies)
  - Action: NO CHANGES

#### Cross-File Changes
- No changes to any other files
  - Rationale: Blast radius = 0 (no external importers)
  - Action: SINGLE FILE REFACTOR ONLY

### Scope Validation

#### Complexity Reduction Target
- **Before**: CYC = 14
- **After**: CYC <= 8 (main method) + helpers <=8 each
- **Method**: Extract nested conditionals to helper methods

#### Blast Radius Confirmation
- **External Importers**: 0
- **Direct Callers**: 2 (both in same file)
- **Risk**: LOW (isolated refactor)

#### Jane Street Alignment
- Target CYC <=8 aligns with Jane Street strict standard
- Cognitive simplicity for microsecond-latency reasoning
- Exhaustive testing feasible with lower complexity

## Extraction Plan Summary

### Phase 2 Deliverables
1. Architecture plan for 2-3 helper method extractions
2. Mermaid diagram showing call flow
3. Complexity reduction strategy

### Phase 5 Deliverables
1. Extract helper methods (CYC <=8 each)
2. Refactor HandleFsmFilled to use helpers
3. Verify final CYC <=8
4. Unit tests for all extracted methods

## Success Criteria
- HandleFsmFilled CYC reduced from 14 to <=8
- All helper methods CYC <=8
- No changes to caller methods (ProcessBracketEvent, DrainAccountMailbox)
- No cross-file changes
- Build passes
- All tests pass

## Scope Boundary Enforcement
- **Single File**: src/V12_002.Symmetry.BracketFSM.cs ONLY
- **Single Method**: HandleFsmFilled (line 349) ONLY
- **Preservation**: All callers and other methods UNCHANGED
- **Testing**: Unit tests for extracted helpers REQUIRED

## Phase 1 Completion
- Scope boundary defined (IN SCOPE vs OUT OF SCOPE)
- Extraction strategy documented
- Success criteria established
- Ready for Phase 2 (Architecture Planning)

**Next Phase**: Phase 2 (Architecture Planning)
