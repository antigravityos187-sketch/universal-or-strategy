# Phase 1: Scope Definition - EPIC-W7-065

## Epic Metadata
- **Epic ID**: EPIC-W7-065
- **Target Method**: HandleFsmFilled
- **File**: src/V12_002.Symmetry.BracketFSM.cs
- **Current CYC**: 14
- **Target CYC**: ≤8
- **Risk Level**: LOW-MEDIUM

## Scope Boundary

### IN SCOPE ✅

#### Primary Target
- **Method**: `HandleFsmFilled(AccountEvent evt, FollowerBracketFSM followerFsm)`
  - **Location**: src/V12_002.Symmetry.BracketFSM.cs:349
  - **Current CYC**: 14
  - **Lines**: 27
  - **Justification**: Exceeds Jane Street threshold (CYC ≤ 8) by 75%

#### Extraction Strategy
1. **Decision Logic Extraction**: Extract nested conditionals into helper methods
2. **State Validation**: Extract FSM state validation logic
3. **Event Processing**: Extract event-specific processing logic

#### Verification Points
- **Caller 1**: ProcessBracketEvent (line 381) - Direct caller
- **Caller 2**: DrainAccountMailbox (line 88) - Indirect caller via ProcessBracketEvent

#### Success Criteria
- All extracted methods have CYC ≤ 8
- Original method signature unchanged (no caller modifications needed)
- Zero blast radius maintained (no external file changes)
- Unit tests added for extracted methods
- Behavior verified at 2 call sites

### OUT OF SCOPE ❌

#### Excluded Methods
- **ProcessBracketEvent**: Caller method - not modified unless necessary
- **DrainAccountMailbox**: Indirect caller - not modified
- **Other methods in file**: Only touch HandleFsmFilled and its extractions

#### Excluded Files
- **All other files**: Zero blast radius means no other files affected
- **Test files**: New tests added, existing tests not modified

#### Excluded Concerns
- **Performance optimization**: Focus is complexity reduction only
- **API changes**: Method signature remains unchanged
- **Architectural changes**: No FSM pattern changes

## Extraction Plan

### Phase 2 Deliverables
1. **Architecture Plan**: Detailed extraction strategy with helper method signatures
2. **Dependency Analysis**: Confirm zero external dependencies
3. **Test Strategy**: Unit test plan for extracted methods

### Phase 3 Deliverables
1. **DNA Audit**: Verify lock-free patterns maintained
2. **PR Hygiene**: Confirm diff size <10k characters

### Phase 4 Deliverables
1. **Ticket 1**: Extract state validation logic (target CYC ≤ 5)
2. **Ticket 2**: Extract event processing logic (target CYC ≤ 5)
3. **Ticket 3**: Refactor main method to orchestrate helpers (target CYC ≤ 8)

## Risk Mitigation

### Low Risk Factors ✅
- **Zero blast radius**: No external dependencies to break
- **Contained callers**: Only 2 call sites to verify
- **File isolation**: All changes in single file
- **Clear inputs**: Method has well-defined parameters

### Mitigation Strategies
1. **Signature preservation**: Keep original method signature to avoid caller changes
2. **Incremental extraction**: Extract one helper at a time, verify after each
3. **Test coverage**: Add unit tests before refactoring
4. **Call site verification**: Test both ProcessBracketEvent and DrainAccountMailbox paths

## Scope Validation

### Boundary Checks
- ✅ **Single method focus**: Only HandleFsmFilled targeted
- ✅ **Single file scope**: Only V12_002.Symmetry.BracketFSM.cs modified
- ✅ **Zero external impact**: No other files affected
- ✅ **Caller preservation**: No caller modifications needed

### Complexity Reduction Target
- **Current**: CYC 14
- **Target**: CYC ≤ 8 (main method) + all helpers ≤ 8
- **Reduction**: 43% complexity reduction minimum

## Agent Tracking
- **Phase**: 1 (Scope Definition)
- **Agent**: v12-phase1-scope
- **Timestamp**: 2026-06-24T19:32:24Z
- **Input**: docs/brain/EPIC-W7-065/00-hotspots.md
- **Output**: docs/brain/EPIC-W7-065/00-scope.md
