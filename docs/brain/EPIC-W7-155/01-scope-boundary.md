# Phase 1: Scope Boundary - EPIC-W7-155

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Execution Time**: 2026-06-24T01:38:34Z
- **Input**: 00-hotspots.md
- **Output**: 01-scope-boundary.md

## Epic Summary
**Target**: TryHandleFleetCommand (CYC=20 → ≤8)
**File**: src/V12_002.UI.IPC.Commands.Fleet.cs
**Strategy**: Extract command router pattern to replace if/else chain

## IN SCOPE

### Primary Target
- **Method**: TryHandleFleetCommand (lines 37-82, CYC=20)
- **Refactoring Type**: Command dispatcher pattern extraction
- **Approach**: Replace if/else chain with dictionary-based command registry

### Specific Changes
1. **Extract Command Registry**
   - Create Dictionary<string, Func<string[], bool>> for command-to-handler mapping
   - Map 18 command types to existing handler methods
   - Initialize registry in constructor or static initializer

2. **Simplify Dispatcher Logic**
   - Replace 18+ if/else branches with single dictionary lookup
   - Preserve existing handler method signatures
   - Maintain backward compatibility with IPC command routing

3. **Target Complexity Reduction**
   - Before: CYC=20, 45 lines, 18+ branches
   - After: CYC≤8, ~15 lines, dictionary-based dispatch

### Existing Handler Methods (DO NOT MODIFY)
All 18 handler methods already exist and are OUT OF SCOPE:
1. TryHandleFleet_Trim
2. TryHandleFleet_Lock50
3. TryHandleFleet_FlattenOnly
4. TryHandleFleet_Flatten
5. TryHandleFleet_CancelAll
6. TryHandleFleet_ResetMemory
7. TryHandleFleet_LongShort
8. TryHandleFleet_OrLong
9. TryHandleFleet_OrShort
10. TryHandleFleet_TrendManualLimit
11. TryHandleFleet_RetestManualLimit
12. TryHandleFleet_FfmaManualLimit
13. TryHandleFleet_FfmaManualMarket
14. TryHandleFleet_CloseTarget
15. TryHandleFleet_MoveTarget
16. TryHandleFleet_FleetState
17. TryHandleFleet_ToggleAccount
18. TryHandleFleet_SetShadow

## OUT OF SCOPE

### Explicitly Excluded
1. **Handler Method Logic**: All 18 handler methods remain unchanged
2. **IPC Command Routing**: Upstream command routing logic unchanged
3. **Command Parsing**: Argument parsing logic unchanged
4. **Error Handling**: Existing error handling patterns preserved
5. **Logging**: Existing logging statements preserved

### Adjacent Code (DO NOT TOUCH)
- Other methods in V12_002.UI.IPC.Commands.Fleet.cs
- IPC command infrastructure
- Fleet state management logic
- Command validation logic

### Testing Scope
- Unit tests for new command registry (IN SCOPE)
- Integration tests for existing handlers (OUT OF SCOPE - already tested)

## Risk Assessment

### Blast Radius: ZERO
- **Direct Dependents**: 0
- **Importer Count**: 0
- **Risk Score**: 0.0
- **Conclusion**: Safe to refactor, no downstream impact

### Complexity Reduction
- **Current**: CYC=20 (2.5x Jane Street threshold)
- **Target**: CYC≤8 (Jane Street GODMODE compliance)
- **Method**: Structural refactoring only, no logic changes

### Churn Analysis
- **Commits (90 days)**: 8
- **Hotspot Score**: 43.9445 (rank #44)
- **Assessment**: Medium churn, high complexity = high priority

## Success Criteria

### Phase 2 (Architecture Planning)
- [ ] Command registry design documented
- [ ] Dictionary-based dispatcher pattern specified
- [ ] Backward compatibility verified

### Phase 3 (DNA Audit)
- [ ] No lock-free violations introduced
- [ ] ASCII-only compliance maintained
- [ ] Jane Street patterns followed

### Phase 4 (Ticket Generation)
- [ ] Ticket 1: Extract command registry
- [ ] Ticket 2: Replace if/else with dictionary lookup
- [ ] Ticket 3: Add unit tests

### Phase 5 (Execution)
- [ ] CYC reduced from 20 to ≤8
- [ ] All existing handlers unchanged
- [ ] Build passes
- [ ] Unit tests pass

## Scope Validation

### Jane Street Alignment
✅ **Correctness by Construction**: Command registry makes invalid commands unrepresentable
✅ **Cognitive Simplicity**: Dictionary lookup vs 18+ branches
✅ **CYC≤8 Threshold**: Target compliance achieved

### V12 DNA Compliance
✅ **Lock-Free**: No state mutations, pure dispatch logic
✅ **ASCII-Only**: No string literal changes
✅ **Zero Blast Radius**: Safe refactoring target

## Conclusion

**Scope Status**: APPROVED
**Risk Level**: LOW
**Complexity Reduction**: HIGH (20 → ≤8)
**Blast Radius**: ZERO

This epic targets a single method with a well-defined refactoring pattern. The scope is tightly bounded to structural changes only, with all handler logic preserved. Zero blast radius ensures safe execution.

**Next Phase**: Architecture Planning (Phase 2)
