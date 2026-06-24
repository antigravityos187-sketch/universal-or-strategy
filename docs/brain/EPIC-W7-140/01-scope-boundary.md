# Phase 1: Scope Boundary - EPIC-W7-140

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Execution Time**: 2026-06-24T01:37:13Z
- **Input**: 00-hotspots.md
- **Output**: 01-scope-boundary.md

## Target Method
- **Method**: InitiateStopReplacement
- **File**: src/V12_002.Trailing.StopUpdate.cs
- **Current CYC**: 13
- **Target CYC**: ≤8 per method

## IN SCOPE

### Primary Extraction Target
**InitiateStopReplacement** (CYC=13) - Extract into 4 smaller methods:

1. **Extract Pending Replacement Lookup** (Target CYC ~3)
   - Check pendingStopReplacements dictionary
   - Validate existing pending state
   - Return early if already pending

2. **Extract Cancellation Coordination** (Target CYC ~3)
   - Determine cancellation strategy (CancelOrderForReplace vs CancelOrderSafe)
   - Execute order cancellation
   - Handle terminal order states

3. **Extract State Persistence** (Target CYC ~2)
   - MarkStickyDirty calls
   - Persistent state updates
   - State synchronization

4. **Keep Orchestration Logic** (Target CYC ~5)
   - High-level coordination
   - Logging and diagnostics
   - REAPER grace period stamping
   - Call extracted helper methods

### Scope Boundaries
- **File**: src/V12_002.Trailing.StopUpdate.cs ONLY
- **Method**: InitiateStopReplacement ONLY
- **Caller**: UpdateStopOrder (no changes to caller)
- **Callees**: No changes to existing helper methods

### Success Criteria
- All extracted methods have CYC ≤8
- Original method reduced to CYC ≤8
- No behavioral changes (pure refactoring)
- All existing tests pass
- Build succeeds

## OUT OF SCOPE

### Explicitly Excluded
1. **UpdateStopOrder** (caller) - No modifications
2. **Helper Methods** - No changes to:
   - GetTargetOrdersDictionary
   - CancelOrderForReplace
   - CancelOrderSafe
   - MarkStickyDirty
   - IsOrderTerminal
   - StampReaperMoveGrace
3. **Other Methods** in V12_002.Trailing.StopUpdate.cs
4. **Other Files** in src/
5. **Test Files** - No new tests (existing tests must pass)
6. **Behavioral Changes** - No logic modifications
7. **Performance Optimization** - Focus on complexity only
8. **Documentation Updates** - Code comments only

### Deferred to Future Epics
- UpdateStopOrder complexity reduction (if needed)
- Broader trailing stop subsystem refactoring
- Test coverage improvements
- Performance profiling

## Risk Mitigation

### Low Blast Radius Confirmed
- Private method with single caller (UpdateStopOrder)
- No external dependencies
- Changes isolated to one file
- Zero cross-file impact

### Complexity Reduction Strategy
- Extract 3 helper methods from InitiateStopReplacement
- Each helper has single responsibility
- Orchestration method coordinates helpers
- Target: 4 methods total, all CYC ≤8

### Validation Gates
1. **Pre-extraction**: Verify current CYC=13 via complexity_audit.py
2. **Post-extraction**: Verify all methods CYC ≤8
3. **Build**: dotnet build must succeed
4. **Tests**: All existing tests must pass
5. **Deploy**: deploy-sync.ps1 must succeed

## Jane Street Alignment
- **Current**: CYC=13 (FAILS ≤8 threshold)
- **Target**: CYC ≤8 per method (PASSES threshold)
- **Cognitive Load**: Reduce from HIGH to LOW
- **Testing Paths**: Reduce from 2^13 (8,192) to manageable levels

## Scope Validation
- ✅ Single method target (InitiateStopReplacement)
- ✅ Single file scope (V12_002.Trailing.StopUpdate.cs)
- ✅ Clear extraction plan (4 methods)
- ✅ Measurable success criteria (CYC ≤8)
- ✅ No scope creep risk (explicit OUT OF SCOPE list)

## Next Steps
Proceed to Phase 2 (Architecture Planning) to design extraction implementation.
