# Phase 1: Scope Definition - EPIC-W7-122

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.00
- **API Key**: jCodemunch MCP
- **Execution Time**: ~10 seconds

## Epic Overview
**Target**: RemoveFsmOrderIdMappings method complexity reduction
**File**: src/V12_002.Symmetry.BracketFSM.cs
**Current CYC**: 10
**Target CYC**: ≤8
**Gap**: 2 complexity points

## Scope Boundary Definition

### IN SCOPE ✅

#### Primary Target
- **Method**: RemoveFsmOrderIdMappings (line 103)
  - Extract conditional branches to reduce CYC from 10 to ≤8
  - Maintain single-caller pattern (TryTerminateFollowerBracket)
  - Preserve dictionary access pattern (_orderIdToFsmKey)

#### Extraction Strategy
1. **Extract null/empty checks** to helper method
2. **Extract dictionary removal logic** to separate method
3. **Maintain method signature** (single parameter: SIMA_FSM fsm)
4. **Preserve error handling** patterns

#### Testing Requirements
- Unit tests for extracted helper methods
- Integration test for TryTerminateFollowerBracket caller
- Verify dictionary state after removal operations

### OUT OF SCOPE ❌

#### Explicitly Excluded
1. **TryTerminateFollowerBracket** (caller method)
   - Only update call site if signature changes
   - Do NOT refactor caller logic
   
2. **_orderIdToFsmKey dictionary**
   - Do NOT modify dictionary structure
   - Do NOT change access patterns
   
3. **Other BracketFSM methods**
   - Do NOT touch adjacent methods
   - Do NOT expand scope to other complexity hotspots
   
4. **Cross-file changes**
   - Zero external blast radius confirmed
   - No changes outside V12_002.Symmetry.BracketFSM.cs

#### Deferred to Future Epics
- Other methods in BracketFSM class with CYC >8
- Broader FSM architecture improvements
- Dictionary performance optimizations

## Scope Validation

### Boundary Checks
✅ **Single file**: src/V12_002.Symmetry.BracketFSM.cs only
✅ **Single method**: RemoveFsmOrderIdMappings only
✅ **Zero external impact**: No cross-file dependencies
✅ **Minimal caller impact**: Single caller (TryTerminateFollowerBracket)

### Risk Mitigation
- **Blast radius**: 0.0 (confirmed by Phase 0)
- **Breaking changes**: NONE (private method)
- **Test coverage**: Add new tests for extracted helpers
- **Rollback plan**: Git revert if CYC reduction fails

## Success Criteria

### Phase 1 Completion
✅ Scope clearly defined (IN vs OUT)
✅ Boundary validated against hotspot analysis
✅ Risk assessment confirms low-risk extraction
✅ Testing strategy documented

### Epic Success (Phase 5)
- RemoveFsmOrderIdMappings CYC reduced to ≤8
- All tests passing
- No regression in TryTerminateFollowerBracket
- Build successful with deploy-sync.ps1

## Jane Street Alignment
- **Cognitive Simplicity**: Reduce branching complexity
- **Single Responsibility**: Extract focused helper methods
- **Testability**: Improve unit test coverage
- **Maintainability**: Clearer code structure

## Phase 1 Status
✅ Scope definition complete
✅ Boundaries validated
✅ Ready for Phase 2 (Architecture Planning)
