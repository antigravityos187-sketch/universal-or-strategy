# Phase 1: Scope Definition - EPIC-W7-161

## Agent Tracking
- Agent Name: v12-phase1-scope
- Bobcoins Used: 0.00
- API Key: jCodemunch MCP
- Execution Time: 2026-06-24T19:46:35Z
- Input: docs/brain/EPIC-W7-161/00-hotspots.md

## Target Method
- Method: FlattenSpecificTarget
- File: src/V12_002.UI.IPC.Commands.Misc.cs
- Line: 268
- Current CYC: 10
- Target CYC: ≤8
- Max Nesting Depth: 6 → Target: ≤3

## Scope Boundary Definition

### IN SCOPE ✅

#### Primary Extraction Target
1. **FlattenSpecificTarget method body** (lines 268-314)
   - Nested conditional logic requiring extraction
   - Deep nesting (6 levels) to be reduced to ≤3
   - CYC reduction from 10 to ≤8

#### Extraction Candidates
Based on nesting depth analysis, extract:
1. **Position validation logic** - Early validation checks
2. **Order state verification** - Terminal state checks
3. **Conditional exit logic** - Nested if/else chains

#### Existing Helper Methods (DO NOT MODIFY)
These already exist and should remain unchanged:
1. FlattenSpecificTarget_ResolveTarget (line 315)
2. FlattenSpecificTarget_CancelLimit (line 360)
3. FlattenSpecificTarget_RequestStopCancel (line 385)
4. FlattenSpecificTarget_SubmitMarketExit (line 391)

### OUT OF SCOPE ❌

#### Explicitly Excluded
1. **Helper methods** - Already extracted, working correctly
2. **Caller methods** - Zero callers, no upstream changes needed
3. **Callee methods** - 26 downstream methods, no modifications
4. **Other IPC commands** - Only FlattenSpecificTarget in this epic
5. **Test files** - No test modifications (zero test coverage currently)
6. **Documentation** - No XML doc changes required
7. **Logging infrastructure** - LogBuffer calls remain unchanged
8. **Thread safety** - ValidateThreadAffinity calls remain unchanged

#### Related Files (No Changes)
1. src-vm-backup/ - Duplicate, ignore per hotspot analysis
2. Other methods in V12_002.UI.IPC.Commands.Misc.cs - Out of scope
3. Position tracking (activePositions) - No changes
4. Order tracking (stopOrders) - No changes

## Extraction Strategy

### Approach
1. **Extract nested conditionals** into focused helper methods
2. **Reduce nesting depth** from 6 to ≤3 levels
3. **Maintain existing pattern** - Follow helper method naming convention
4. **Preserve behavior** - Zero functional changes, pure refactoring

### Success Criteria
- ✅ CYC reduced from 10 to ≤8
- ✅ Nesting depth reduced from 6 to ≤3
- ✅ Zero blast radius maintained (no callers affected)
- ✅ All existing helper methods unchanged
- ✅ Build passes after extraction
- ✅ F5 in NinjaTrader successful

## Risk Assessment

### Risk Level: LOW
- Zero blast radius (no callers)
- Already partially refactored (4 helpers exist)
- Low churn (not in top 50 hotspots)
- Stable, well-understood code

### Mitigation
- Follow existing helper method pattern
- Surgical extraction only
- No functional changes
- Comprehensive build verification

## Scope Validation

### Boundary Checks
- ✅ Single method target (FlattenSpecificTarget)
- ✅ Single file modification (V12_002.UI.IPC.Commands.Misc.cs)
- ✅ No cross-file dependencies
- ✅ No caller modifications needed
- ✅ No callee modifications needed

### Jane Street Alignment
- ✅ CYC ≤8 threshold (Jane Street strict standard)
- ✅ Cognitive simplicity (reduce nesting)
- ✅ Single responsibility (extract focused helpers)
- ✅ Testability (simpler methods easier to test)

## Phase 1 Completion
- Status: ✅ COMPLETE
- Scope: DEFINED
- Boundary: VALIDATED
- Next Phase: Architecture Planning (Phase 2)
