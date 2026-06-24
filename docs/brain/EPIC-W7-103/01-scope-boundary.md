# Phase 1: Scope Boundary - EPIC-W7-103

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Mode**: plan
- **Execution Time**: 2026-06-24T01:34:49Z

## Epic Summary
**Target**: ProcessFleetSlot method in src/V12_002.SIMA.Fleet.cs
**Current CYC**: 13 (exceeds Jane Street threshold of 8 by 62.5%)
**Goal**: Reduce to CYC ≤ 8 through surgical extraction

## IN SCOPE

### Primary Extraction Target
- **Method**: ProcessFleetSlot (line 44, CYC 13)
- **File**: src/V12_002.SIMA.Fleet.cs
- **Lines**: 54 lines of code
- **Parameters**: 8 parameters

### Extraction Candidates
Based on complexity analysis, extract the following logical units:

1. **Dispatch Timestamp Validation** (CYC ~2)
   - ValidateDispatchTimestamp logic
   - Early return on validation failure
   - Extract to: ValidateFleetDispatchTimestamp()

2. **Follower Bracket FSM Initialization** (CYC ~3)
   - InitializeFollowerBracketFSM logic
   - Bracket configuration setup
   - Extract to: InitializeFleetFollowerBracket()

3. **Fleet Order Submission** (CYC ~4)
   - SubmitAndRegisterFleetOrders logic
   - Order registration and tracking
   - Extract to: SubmitFleetOrdersWithRegistration()

4. **Rollback and State Management** (CYC ~3)
   - RollbackFleetDispatchState logic
   - State cleanup on failure
   - Extract to: HandleFleetDispatchRollback()

### Affected Callers (Must Verify Post-Extraction)
1. PumpFleetDispatch (line 233)
2. ProcessValidPhotonSlot (line 395)
3. VerifyPhotonSlotIntegrity (line 329)

### Testing Requirements
- Unit tests for each extracted method
- Integration test for ProcessFleetSlot orchestration
- Verify all 3 callers still function correctly

## OUT OF SCOPE

### Excluded from This Epic
1. **Caller Refactoring**: PumpFleetDispatch, ProcessValidPhotonSlot, VerifyPhotonSlotIntegrity
   - Rationale: Separate concerns, avoid scope creep
   - Future Epic: If callers exceed CYC threshold

2. **Callee Refactoring**: 60 downstream methods
   - Rationale: Focus on ProcessFleetSlot only
   - Future Epic: If callees identified as hotspots

3. **Parameter Reduction**: 8 parameters
   - Rationale: Maintain signature compatibility
   - Future Epic: Consider parameter object pattern if needed

4. **Other Methods in V12_002.SIMA.Fleet.cs**
   - Rationale: One method per epic (No Scope Creep Protocol)
   - Future Epic: Address other hotspots separately

5. **Cross-File Changes**
   - Rationale: Private method, no external dependencies
   - Exception: Only if extraction reveals hidden coupling

## Scope Validation

### Jane Street Alignment
- ✅ Target CYC ≤ 8 (strict standard)
- ✅ Single responsibility per extracted method
- ✅ Maintain lock-free Actor pattern
- ✅ ASCII-only compliance

### V12 DNA Compliance
- ✅ Correctness by construction
- ✅ No scope creep (one method per epic)
- ✅ Surgical changes only
- ✅ Test coverage for all extractions

### Risk Mitigation
- ✅ LOW blast radius (private method)
- ✅ 3 callers identified for verification
- ✅ No external dependencies
- ✅ Contained within single file

## Success Criteria

### Complexity Reduction
- [ ] ProcessFleetSlot CYC reduced from 13 to ≤ 8
- [ ] All extracted methods CYC ≤ 8
- [ ] Max nesting depth reduced from 5 to ≤ 3

### Functional Correctness
- [ ] All 3 callers verified working
- [ ] Unit tests pass for extracted methods
- [ ] Integration tests pass
- [ ] Build passes (dotnet build)

### Quality Gates
- [ ] deploy-sync.ps1 executed successfully
- [ ] F5 in NinjaTrader successful
- [ ] No new compilation errors
- [ ] ASCII-only compliance maintained

## Phase 1 Completion
- Scope boundary defined
- IN SCOPE: 4 extraction candidates identified
- OUT OF SCOPE: 5 exclusions documented
- Success criteria established

**Status**: READY FOR PHASE 2 (Architecture Planning)
