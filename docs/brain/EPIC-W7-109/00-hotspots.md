# Phase 0: Hotspot Analysis - EPIC-W7-109

## Agent Tracking
- **Agent Name**: v12-phase0-hotspot
- **Bobcoins Used**: 0.77
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-23T02:54:47Z to 2026-06-23T02:55:06Z

## Target Method
- **Method**: HydrateWorkingOrdersFromBroker
- **File**: src/V12_002.SIMA.Lifecycle.cs
- **Line**: 309
- **Cyclomatic Complexity**: 23
- **Lines of Code**: 149

## Complexity Metrics

### Symbol Complexity Analysis
- **Cyclomatic Complexity**: 23 (HIGH - exceeds threshold of 8)
- **Max Nesting Depth**: 7 (HIGH - deep nesting indicates complex control flow)
- **Parameter Count**: 0
- **Assessment**: HIGH RISK

### Hotspot Score Analysis
From get_hotspots (top 50 methods by complexity x churn):
- **Hotspot Score**: 81.773
- **Rank**: 5 out of 50 hotspots
- **Churn (90 days)**: 34 commits
- **Assessment**: HIGH - This method is both complex AND frequently modified

### Comparison to Top Hotspots
1. HydrateFromOpenPositions: 120.88 (CYC 34)
2. IsCommandForThisInstrument: 109.83 (CYC 38)
3. HandleTerminated: 102.04 (CYC 30)
4. SweepBrokerOrders: 99.55 (CYC 28)
5. HydrateWorkingOrdersFromBroker: 81.77 (CYC 23) - TARGET

## Blast Radius

### Direct Impact
- **Importer Count**: 0
- **Direct Dependents**: 0
- **Confirmed Files Affected**: 0
- **Potential Files Affected**: 0
- **Overall Risk Score**: 0.0 (LOW)

### Analysis
The blast radius analysis shows ZERO direct dependents, meaning:
- No other files directly import or reference this method
- Changes to this method have minimal ripple effects
- This is an internal implementation detail within V12_002.SIMA.Lifecycle.cs
- LOW RISK for breaking external code

## Call Hierarchy

### Callers (Who calls this method)
Depth 1 Callers (2 direct callers):
1. EnumerateApexAccounts (line 140, same file)
2. ProcessInitializeSIMA (line 90, same file) - depth 2 caller

### Callees (What this method calls)
Depth 1 Callees (41 methods called):

Primary Operations:
1. AdoptFleetOrders (line 903) - Fleet order adoption
2. AdoptMasterOrders (line 1195) - Master order adoption
3. HydrateFSMsFromWorkingOrders (line 787) - FSM hydration
4. IsFleetAccount - Account type checking
5. LogBuffer.Format - Logging operations

State Management:
6. stopOrders (constant) - Stop order collection
7. activePositions (constant) - Position tracking
8. entryOrders (constant) - Entry order collection
9. _followerBrackets (constant) - Follower bracket tracking

Helper Methods (depth 2):
10. AdoptOrdersFromAccount (line 930)
11. ClassifyOrderByPrefix (line 1262)
12. MapOrderStateToFSMState (line 469)
13. FindLivePosition (line 605)
14. ResolveRemainingContracts (line 532)
15. BuildFSM (line 505)
16. LinkTargetOrderToFSM (line 579)
17. RegisterFSM (line 551)
18. HydrateFromOpenPositions (line 625)

### Call Graph Depth
- **Maximum Depth Reached**: 2
- **Total Callers**: 2
- **Total Callees**: 41

### Analysis
- High Fan-Out: 41 callees indicates this method orchestrates many operations
- Low Fan-In: Only 2 callers suggests this is a specialized initialization routine
- Deep Call Chain: Depth 2 means changes could propagate through multiple layers
- Orchestration Pattern: This method appears to be a coordinator/orchestrator

## Risk Assessment

### Overall Risk: MEDIUM-HIGH

Risk Factors:
1. Complexity: CYC 23 (HIGH) - nearly 3x the Jane Street threshold of 8
2. Nesting: Max depth 7 (HIGH) - deep conditional logic
3. Churn: 34 commits in 90 days (HIGH) - frequently modified
4. Hotspot Score: 81.77 (HIGH) - rank 5 in top 50 hotspots
5. Fan-Out: 41 callees (HIGH) - orchestrates many operations
6. Lines of Code: 149 lines (HIGH) - large method

Mitigating Factors:
1. Blast Radius: 0 direct dependents (LOW) - isolated impact
2. Fan-In: Only 2 callers (LOW) - limited entry points
3. Scope: Internal to SIMA.Lifecycle.cs (LOW) - no cross-file dependencies

### Refactoring Priority: HIGH

Rationale:
- High complexity (CYC 23) makes this method hard to reason about
- High churn (34 commits) indicates active development area
- Deep nesting (7 levels) suggests complex control flow
- Large size (149 lines) makes testing difficult
- BUT: Low blast radius means refactoring is safer than other hotspots

### Recommended Approach

Strategy: Extract Method Pattern
1. Extract order classification logic (likely 20-30 lines)
2. Extract FSM hydration logic (likely 30-40 lines)
3. Extract fleet vs master branching (likely 15-20 lines)
4. Extract logging/validation (likely 10-15 lines)

Target: Reduce CYC from 23 to 8 or less per method

Estimated Tickets: 3-4 extraction tickets

## Jane Street Alignment

### Violations
1. CYC > 8: Method exceeds Jane Street strict threshold (23 vs 8)
2. Nesting > 4: Deep nesting (7) violates cognitive simplicity principle
3. Lines > 50: Method size (149) exceeds single-screen visibility

### Principles Applied
- Blast Radius Analysis: Confirms low external impact
- Hotspot Detection: Identifies high-risk area
- Churn Analysis: Validates active development concern

## Next Steps

### Phase 1: Scope Definition
1. Read method source code (lines 309-458)
2. Identify extraction boundaries
3. Map control flow branches
4. Define ticket breakdown

### Phase 2: Architecture Planning
1. Design extracted method signatures
2. Plan state management strategy
3. Define testing approach
4. Create extraction sequence

### Phase 3: DNA Audit
1. Verify ASCII-only compliance
2. Check for lock() usage (banned)
3. Validate FSM/Actor pattern usage
4. Review error handling

## Conclusion

HydrateWorkingOrdersFromBroker is a HIGH-PRIORITY refactoring target:
- High complexity (CYC 23)
- High churn (34 commits)
- High hotspot score (81.77)
- Low blast radius (0 dependents) - SAFE to refactor
- Clear extraction opportunities

Recommendation: Proceed to Phase 1 (Scope Definition)
