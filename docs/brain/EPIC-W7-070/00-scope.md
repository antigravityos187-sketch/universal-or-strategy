# Phase 1: Scope Definition - EPIC-W7-070

## Agent Tracking
- Agent Name: v12-phase1-scope
- Bobcoins Used: 0.00
- API Key: jCodemunch MCP
- Execution Time: 2026-06-24T19:33:00Z

## Target Method
- Method: HydrateFSMsFromWorkingOrders
- File: src/V12_002.SIMA.Lifecycle.cs
- Line: 787
- Current CYC: 13
- Target CYC: <=8
- Gap: 5 points

## Scope Boundary Decision

### IN SCOPE
1. **Primary Extraction Target**: HydrateFSMsFromWorkingOrders (CYC 13)
   - Extract FSM state mapping logic
   - Extract position resolution logic
   - Extract FSM building and registration logic

2. **Extraction Candidates** (2-3 helper methods):
   - ExtractFSMStateMapping() - Map order state to FSM state
   - ExtractPositionResolution() - Resolve remaining contracts and find live positions
   - ExtractFSMBuildAndRegister() - Build FSM, link target order, register

3. **Scope Justification**:
   - Zero blast radius (0 external dependents) - SAFE
   - High complexity (CYC 13 vs target 8) - NEEDED
   - High churn (34 commits/90 days) - VOLATILE
   - Contained scope (same-file callers) - ISOLATED
   - Clear extraction boundaries (33 callees can be grouped)

### OUT OF SCOPE
1. **Caller Methods** (leave unchanged):
   - HydrateWorkingOrdersFromBroker (line 309)
   - EnumerateApexAccounts (line 140)
   - ProcessInitializeSIMA (line 90)

2. **Callee Methods** (leave unchanged):
   - MapOrderStateToFSMState
   - FindLivePosition
   - ResolveRemainingContracts
   - BuildFSM
   - LinkTargetOrderToFSM
   - RegisterFSM
   - HydrateFromOpenPositions
   - All other 26 callees

3. **Out of Scope Rationale**:
   - Callers are already at acceptable complexity
   - Callees are single-responsibility helpers
   - No cascading refactoring needed
   - Surgical extraction only

## Extraction Strategy
- **Approach**: Extract 2-3 helper methods from HydrateFSMsFromWorkingOrders
- **Target CYC**: Each extracted method <=8
- **Remaining CYC**: Parent method <=8 after extraction
- **Risk Level**: LOW (zero blast radius)
- **Estimated Effort**: 2-3 tickets

## Success Criteria
- [ ] HydrateFSMsFromWorkingOrders reduced to CYC <=8
- [ ] All extracted methods have CYC <=8
- [ ] Zero compilation errors
- [ ] All existing tests pass
- [ ] deploy-sync.ps1 executed successfully
- [ ] F5 in NinjaTrader successful

## Jane Street Alignment
- Current: CYC 13 (GODMODE violation)
- Target: CYC <=8 (GODMODE compliant)
- Extraction: 2-3 methods (cognitive simplicity)
- Pattern: Single-responsibility decomposition

## Phase 1 Completion
- Status: COMPLETED
- Scope: DEFINED
- Next Phase: Phase 1.5 (Scope Boundary Validation)
