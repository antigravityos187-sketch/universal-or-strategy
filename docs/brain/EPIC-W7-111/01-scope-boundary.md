# Phase 1: Scope Definition - EPIC-W7-111

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.00
- **API Key**: N/A
- **Execution Time**: 2026-06-24T01:50:01Z

## Epic Summary
**Target**: HydrateExpectedPositionsFromBroker (CYC=18, 93 lines, max nesting=8)
**File**: src/V12_002.SIMA.Lifecycle.cs:208
**Objective**: Reduce cyclomatic complexity from 18 to <=8 per method through surgical extraction

## Scope Boundary Definition

### IN SCOPE

#### Primary Target
- **Method**: HydrateExpectedPositionsFromBroker (lines 208-300)
- **Current CYC**: 18
- **Target CYC**: <=8 per extracted method
- **Extraction Strategy**: Decompose nested conditional logic into helper methods

#### Extraction Candidates (Based on Nesting Depth Analysis)
1. **Account Filtering Logic** (nesting depth 2-3)
   - Fleet account validation
   - Account enumeration and filtering
   - Expected: CYC <=3

2. **Position Hydration Logic** (nesting depth 4-6)
   - Position lookup and validation
   - Expected position creation
   - Position state synchronization
   - Expected: CYC <=5

3. **Error Handling & Logging** (nesting depth 2-4)
   - Error condition detection
   - Log buffer formatting
   - Actor thread validation
   - Expected: CYC <=3

#### Refactoring Constraints
- **Blast Radius**: MUST remain 0.0 (keep extracted methods private)
- **Callers**: MUST NOT modify the 2 existing callers (EnumerateApexAccounts, ProcessInitializeSIMA)
- **Actor Pattern**: MUST preserve Enqueue/TryDrain semantics
- **Thread Safety**: MUST maintain IsActorThread validation
- **Logging**: MUST preserve LogBuffer.Format calls

### OUT OF SCOPE

#### Caller Methods (No Modifications)
- **EnumerateApexAccounts** (src/V12_002.SIMA.Lifecycle.cs:140)
  - Rationale: Caller should remain unchanged to minimize blast radius
  
- **ProcessInitializeSIMA** (src/V12_002.SIMA.Lifecycle.cs:90)
  - Rationale: Caller should remain unchanged to minimize blast radius

#### Callee Methods (No Modifications)
- **IsFleetAccount** (src/V12_002.cs:864)
- **Enqueue** (src/V12_002.cs:428)
- **ExpKey** (src/V12_002.SIMA.cs:209)
- **LogBuffer.Format** (src/V12_002.Perf.LogBuffer.cs:28)
- **All 20 callee methods**: Rationale: These are stable dependencies, no changes needed

#### Related Methods (Deferred to Future Epics)
- **HydrateFromOpenPositions** (CYC=34, rank #1) - EPIC-W7-XXX
- **HydrateWorkingOrdersFromBroker** (CYC=23, rank #5) - EPIC-W7-XXX
- **SweepBrokerOrders** (CYC=28, rank #4) - EPIC-W7-XXX
- Rationale: Each hotspot requires dedicated epic, avoid scope creep

#### Infrastructure Changes (Not Needed)
- **Actor Queue Implementation**: No changes to _cmdQueue
- **FSM State Machine**: No changes to SIMA_FSM
- **Logging Infrastructure**: No changes to LogBuffer
- Rationale: Infrastructure is stable, focus on method extraction only

### Boundary Validation

#### Scope Creep Prevention
- Single Method Focus: Only HydrateExpectedPositionsFromBroker
- Zero Blast Radius: No changes to callers or callees
- Private Extraction: All extracted methods remain private
- No Infrastructure Changes: Actor pattern and logging unchanged

#### Success Criteria
1. **Complexity Reduction**: Main method CYC <=8
2. **Extracted Methods**: Each extracted method CYC <=8
3. **Blast Radius**: Remains 0.0 (no new external dependencies)
4. **Caller Compatibility**: 2 existing callers work unchanged
5. **Build Success**: dotnet build passes
6. **Test Coverage**: Unit tests for each extracted method

## Extraction Plan Overview

### Phase 2 Deliverables (Architecture Planning)
1. **Detailed extraction map**: Line-by-line breakdown of nested logic
2. **Helper method signatures**: Proposed method names and parameters
3. **Complexity projections**: Expected CYC for each extracted method
4. **Test strategy**: Unit test plan for each helper method

### Phase 3 Deliverables (DNA & PR Audit)
1. **V12 DNA compliance**: Verify lock-free Actor pattern preserved
2. **ASCII-only audit**: Ensure no Unicode in extracted code
3. **Jane Street alignment**: Confirm CYC <=8 per method
4. **PR hygiene check**: Verify diff <10k characters

### Phase 4 Deliverables (Ticket Generation)
1. **Ticket 1**: Extract account filtering logic (CYC <=3)
2. **Ticket 2**: Extract position hydration logic (CYC <=5)
3. **Ticket 3**: Extract error handling & logging (CYC <=3)
4. **Ticket 4**: Refactor main method to orchestrate helpers (CYC <=8)

## Risk Mitigation

### Low Risk Factors
- **Zero Blast Radius**: No external dependencies to break
- **Limited Callers**: Only 2 entry points to test
- **Isolated Method**: Changes contained within SIMA.Lifecycle.cs
- **High Confidence AST**: Good visibility into call graph

### Medium Risk Factors
- **High Churn**: 34 commits in 90 days (active maintenance area)
- **Deep Nesting**: Max depth 8 requires careful extraction
- **20 Callees**: High coupling to other methods

### Mitigation Strategies
1. **Incremental Extraction**: One helper method per ticket
2. **Comprehensive Testing**: Unit test each extracted method
3. **Build Verification**: Run dotnet build after each ticket
4. **Deploy Sync**: Run deploy-sync.ps1 after each ticket
5. **F5 Verification**: Test in NinjaTrader IDE after each ticket

## Conclusion

**Scope is LOCKED for EPIC-W7-111**

The scope is tightly bounded to the single method HydrateExpectedPositionsFromBroker with zero blast radius. All extracted methods will remain private, preserving the isolated nature of this refactoring. No changes to callers, callees, or infrastructure.

**Next Phase**: Proceed to Phase 2 (Architecture Planning) to design the extraction strategy and helper method signatures.
