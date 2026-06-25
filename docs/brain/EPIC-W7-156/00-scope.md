# Phase 1: Scope Definition - EPIC-W7-156

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.18
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T19:45:47Z
- **Input**: docs/brain/EPIC-W7-156/00-hotspots.md

## Epic Objective
Reduce cyclomatic complexity of `CancelAll_ProcessSingleFleetAccount` from CYC=18 to ≤8 (Jane Street strict standard) through surgical extraction of decision logic into helper methods.

## Target Method
- **Method**: CancelAll_ProcessSingleFleetAccount
- **File**: src/V12_002.UI.IPC.Commands.Fleet.cs
- **Line**: 300
- **Current CYC**: 18
- **Target CYC**: ≤8 (orchestrator ≤5)
- **Lines of Code**: 44
- **Max Nesting Depth**: 4

## Scope Boundaries

### IN SCOPE ✅

#### Primary Target
1. **CancelAll_ProcessSingleFleetAccount method** (lines 300-344)
   - Extract decision logic into helper methods
   - Reduce nesting depth from 4 to ≤2
   - Transform into orchestrator pattern (CYC ≤5)

#### Extraction Candidates (to be identified in Phase 2)
- Order state validation logic
- Terminal order filtering
- Account-specific cancellation logic
- Error handling patterns

#### Quality Gates
1. All extracted methods must have CYC ≤8
2. Original method becomes orchestrator (CYC ≤5)
3. Zero external behavior changes
4. Build passes with deploy-sync.ps1
5. F5 in NinjaTrader succeeds

### OUT OF SCOPE ❌

#### Caller Methods (No Changes)
1. **CancelAll_ProcessFleetOrders** (line 275)
   - Reason: Not part of this epics complexity target
   - Status: Caller remains unchanged

2. **CancelAll_ProcessFleetAccounts** (line 268)
   - Reason: Not part of this epics complexity target
   - Status: Caller remains unchanged

#### Callee Methods (No Changes)
1. **CancelOrderOnAccount** (src/V12_002.Orders.CancelGateway.cs:46)
   - Reason: External dependency, separate concern
   - Status: Interface contract preserved

2. **IsOrderTerminal** (src/V12_002.Orders.Management.Flatten.cs:698)
   - Reason: External dependency, separate concern
   - Status: Interface contract preserved

#### Other Files
- **src/V12_002.Orders.CancelGateway.cs**: No modifications
- **src/V12_002.Orders.Management.Flatten.cs**: No modifications
- **Any other files**: No modifications

### Scope Rationale

**Why This Scope is Surgical**:
1. ✅ **Isolated Blast Radius**: Zero external file dependencies
2. ✅ **Same-File Callers**: Both callers in same file (easy to verify)
3. ✅ **Clear Boundaries**: 44-line method with defined entry/exit points
4. ✅ **No Interface Changes**: Callee methods remain untouched
5. ✅ **Single Concern**: Fleet account order cancellation logic only

**Risk Mitigation**:
- Blast radius score: 0.0 (no external dependencies)
- Caller count: 2 (both in same file, easy to test)
- Callee count: 2 (interfaces preserved)
- Overall risk: MEDIUM-LOW

## Extraction Strategy

### Phase 2 Will Identify
1. Decision logic patterns (if/else chains)
2. Loop bodies with complex conditions
3. Nested validation checks
4. Error handling blocks

### Expected Outcome
- **Original Method**: Orchestrator with CYC ≤5
- **Helper Methods**: 2-4 methods, each with CYC ≤8
- **Total CYC Reduction**: From 18 to aggregate ≤8 per method
- **Nesting Reduction**: From 4 levels to ≤2 levels

## Success Criteria

### Phase 1 Completion ✅
- [x] Scope boundaries defined (IN SCOPE vs OUT OF SCOPE)
- [x] Target method identified with metrics
- [x] Caller/callee analysis complete
- [x] Risk assessment documented
- [x] Extraction strategy outlined

### Epic Completion (Future Phases)
- [ ] All extracted methods have CYC ≤8
- [ ] Original method is orchestrator (CYC ≤5)
- [ ] Zero external behavior changes
- [ ] Build passes with deploy-sync.ps1
- [ ] F5 in NinjaTrader succeeds
- [ ] Unit tests added for extracted methods

## Dependencies

### Prerequisites
- ✅ Phase 0 hotspot analysis complete
- ✅ jCodemunch index current
- ✅ Git status clean

### Blockers
- None identified

## Next Phase
**Phase 1.5**: Scope Boundary Validation
- Verify no scope creep
- Confirm extraction boundaries
- Validate risk assessment
