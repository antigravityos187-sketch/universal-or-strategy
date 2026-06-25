# Phase 1: Scope Definition - EPIC-W7-113

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.00
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T19:39:22Z

## Target Method
- **Method**: HydrateFSMsFromWorkingOrders
- **File**: src/V12_002.SIMA.Lifecycle.cs
- **Line**: 787
- **Current CYC**: 13
- **Target CYC**: <=8
- **Lines of Code**: 105

## Scope Boundary Definition

### IN SCOPE

#### Primary Extraction Target
**HydrateFSMsFromWorkingOrders** (CYC=13, 105 lines)
- **Rationale**: Exceeds Jane Street threshold (<=8) by 5 points
- **Blast Radius**: ZERO direct dependents - isolated refactoring
- **Churn**: 34 commits in 90 days - active hotspot
- **Rank**: #36 in top 50 hotspots (score 46.22)

#### Extraction Candidates (Within Method Body)
Based on 33 callees and 105 lines, identify natural break points:

1. **Order Collection Iteration Logic**
   - Loops through entryOrders, stopOrders, target1-5Orders
   - Candidate for extraction: IterateOrderCollections()
   - Estimated CYC reduction: 3-4 points

2. **FSM State Mapping Logic**
   - Calls MapOrderStateToFSMState (CYC=13 itself)
   - Candidate for extraction: MapAndValidateOrderState()
   - Estimated CYC reduction: 2-3 points

3. **FSM Lifecycle Orchestration**
   - BuildFSM, RegisterFSM, LinkTargetOrderToFSM sequence
   - Candidate for extraction: InitializeAndRegisterFSM()
   - Estimated CYC reduction: 2 points

4. **Position Resolution Logic**
   - FindLivePosition, ResolveRemainingContracts
   - Candidate for extraction: ResolvePositionContext()
   - Estimated CYC reduction: 1-2 points

#### Success Criteria
- Extract 2-3 helper methods
- Each extracted method: CYC <=8
- Remaining orchestration logic: CYC <=8
- Total CYC reduction: 5+ points (13 to <=8)

### OUT OF SCOPE

#### Callers (Preserve As-Is)
1. **HydrateWorkingOrdersFromBroker** (line 309)
   - **Rationale**: Caller orchestration, not target method
   - **Action**: No changes

2. **EnumerateApexAccounts** (line 140)
   - **Rationale**: Indirect caller (depth 2), not target method
   - **Action**: No changes

#### Callees (Preserve As-Is)
The 33 downstream callees are OUT OF SCOPE unless they:
- Are called ONLY from HydrateFSMsFromWorkingOrders
- Have CYC >8 themselves
- Are natural extraction candidates

**Explicitly OUT OF SCOPE callees:**
- MapOrderStateToFSMState (CYC=13) - separate epic candidate
- HydrateFromOpenPositions (CYC=34) - separate epic candidate
- BuildFSM, RegisterFSM, LinkTargetOrderToFSM - preserve as-is
- Order collection accessors - preserve as-is
- LogBuffer.Format methods - preserve as-is

#### Related Methods (Future Epics)
- **MapOrderStateToFSMState** (CYC=13) - EPIC-W7-114 candidate
- **HydrateFromOpenPositions** (CYC=34) - EPIC-W7-115 candidate

### Scope Validation

#### Boundary Rules
1. **Single Method Focus**: Only HydrateFSMsFromWorkingOrders
2. **No Caller Changes**: Preserve calling contracts
3. **No Callee Refactoring**: Only extract internal logic
4. **Preserve Semantics**: No behavior changes
5. **Maintain FSM Lifecycle**: Preserve BuildFSM to RegisterFSM to Link sequence

#### Risk Mitigation
- **Zero Blast Radius**: No external dependents to break
- **Clear Callers**: Only 2 callers, both in same file
- **Isolated Changes**: Extraction stays within method boundary
- **Test Coverage**: Verify FSM hydration logic post-extraction

## Extraction Strategy

### Approach
**Vertical Slice Extraction** - Break 105-line method into focused helpers:

1. **Phase 1**: Extract order collection iteration (lines ~10-30)
2. **Phase 2**: Extract FSM state mapping (lines ~30-50)
3. **Phase 3**: Extract FSM lifecycle orchestration (lines ~50-70)
4. **Phase 4**: Extract position resolution (lines ~70-90)
5. **Phase 5**: Verify remaining orchestration <=8 CYC

### Complexity Distribution Target
- **Original**: 1 method x CYC 13 = 13 total
- **Target**: 4 methods x CYC <=8 = <=32 total (distributed)
- **Orchestrator**: 1 method x CYC <=8 = <=8 (simplified)

### Jane Street Alignment
- **Current**: CYC 13 (exceeds threshold by 5)
- **Target**: CYC <=8 per method (Jane Street strict standard)
- **Rationale**: Microsecond-latency reasoning, exhaustive testing, race condition auditing

## Next Steps (Phase 1.5)
1. Validate scope boundary with Sequential Thinking MCP
2. Confirm no hidden dependencies on mutable state
3. Verify extraction candidates do not introduce new coupling
4. Proceed to Phase 2 (Architecture Planning)

## Scope Boundary Validation Checklist
- Single method focus confirmed
- Caller contracts preserved
- Callee interfaces unchanged
- No behavior changes planned
- FSM lifecycle sequence maintained
- Zero blast radius verified
- Extraction candidates identified
- CYC reduction path defined
