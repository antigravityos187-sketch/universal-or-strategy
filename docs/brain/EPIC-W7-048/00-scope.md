# Phase 1: Scope Definition - EPIC-W7-048

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.18
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T19:29:46Z

## Epic Metadata
- **Epic ID**: EPIC-W7-048
- **Target Method**: UpdateExistingPendingReplacement
- **File**: src/V12_002.Trailing.StopUpdate.cs
- **Current CYC**: 15
- **Target CYC**: ≤8 per extracted method
- **Lines of Code**: 87

## Scope Boundary Definition

### IN SCOPE ✅

#### Primary Target
- **Method**: UpdateExistingPendingReplacement (lines 167-254)
  - Extract nested conditional blocks into helper methods
  - Reduce nesting depth from 6 to ≤3
  - Target CYC ≤8 per extracted method

#### Extraction Candidates
1. **Pending Replacement Validation Logic** (estimated CYC 3-4)
   - Lines checking pendingStopReplacements dictionary
   - Validation of existing pending replacement state
   
2. **Target Snapshot Management** (estimated CYC 2-3)
   - CaptureTargetSnapshot calls
   - RefreshTargetSnapshot calls
   - Target order dictionary access
   
3. **Stop Price Update Logic** (estimated CYC 4-5)
   - Stop price comparison and validation
   - Trail level update logic
   - Pending replacement state updates

4. **Logging and State Management** (estimated CYC 2-3)
   - LogBuffer.Format calls
   - MarkStickyDirty calls
   - State synchronization

#### Files to Modify
- src/V12_002.Trailing.StopUpdate.cs (primary target)

#### Dependencies to Preserve
- Single caller: UpdateStopOrder (line 84)
- 16 callees (maintain all existing calls)
- Zero blast radius (no external importers)

### OUT OF SCOPE ❌

#### Excluded from This Epic
- **UpdateStopOrder method** (caller) - separate epic if needed
- **Callee methods** (16 methods) - no modifications
- **Other files** - zero blast radius confirmed
- **Test file modifications** - tests added in Phase 5.V only
- **Signature changes** - preserve 5-parameter signature
- **Behavioral changes** - pure refactoring only

#### Deferred to Future Epics
- Churn analysis (git history not available in hotspot phase)
- Performance optimization
- Additional trailing stop methods

### Scope Validation

#### Complexity Budget
- **Current**: 15 CYC in 1 method
- **Target**: 4-5 methods with CYC ≤8 each
- **Estimated Distribution**:
  - Main method: CYC 4-5 (orchestration only)
  - Helper 1: CYC 3-4 (validation)
  - Helper 2: CYC 2-3 (snapshot management)
  - Helper 3: CYC 4-5 (price update logic)
  - Helper 4: CYC 2-3 (logging/state)

#### Risk Mitigation
- ✅ Zero blast radius (isolated method)
- ✅ Single caller (clear entry point)
- ✅ No signature changes (preserve interface)
- ✅ No behavioral changes (pure refactoring)
- ✅ All callees preserved (no dependency breaks)

#### Success Criteria
1. All extracted methods have CYC ≤8
2. Max nesting depth reduced to ≤3
3. Original method becomes orchestrator (CYC 4-5)
4. All 16 callee relationships preserved
5. Single caller relationship maintained
6. Zero compilation errors
7. Zero behavioral changes

## Boundary Enforcement

### Jane Street Alignment
- **Threshold**: CYC ≤8 (strict standard)
- **Rationale**: Microsecond-latency reasoning, exhaustive testing
- **Enforcement**: Pre-push validation (Check #9)

### V12 DNA Compliance
- ✅ Lock-free pattern (no locks in target method)
- ✅ ASCII-only (no Unicode in target method)
- ✅ Correctness by construction (preserve all validations)
- ✅ Surgical changes only (no scope creep)

## Phase 1 Completion
- **Scope Defined**: ✅
- **Boundaries Clear**: ✅
- **Risk Assessed**: ✅
- **Ready for Phase 1.5**: ✅
