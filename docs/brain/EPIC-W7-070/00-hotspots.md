# Phase 0: Hotspot Analysis - EPIC-W7-070

## Agent Tracking
- Agent Name: v12-phase0-hotspot
- Bobcoins Used: 0.77
- API Key: jCodemunch MCP
- Execution Time: 2026-06-23T02:47:53Z

## Target Method
- Method: HydrateFSMsFromWorkingOrders
- File: src/V12_002.SIMA.Lifecycle.cs
- Line: 787
- Cyclomatic Complexity: 13
- Lines of Code: 105

## Complexity Metrics
- Cyclomatic Complexity: 13
- Max Nesting Depth: 4
- Parameter Count: 0
- Lines: 105
- Assessment: HIGH

## Hotspot Analysis
- Hotspot Score: 46.22
- Rank: 36 out of top 50 hotspots
- Churn 90 days: 34 commits
- Risk Level: HIGH

## Blast Radius
- Direct Dependents: 0
- Importer Count: 0
- Overall Risk Score: 0.0
- Confirmed Files: 0
- Potential Files: 0

Analysis: Zero direct dependents - LOW RISK from blast radius perspective.

## Call Hierarchy

### Callers
1. HydrateWorkingOrdersFromBroker depth 1 line 309
2. EnumerateApexAccounts depth 2 line 140
3. ProcessInitializeSIMA depth 3 line 90

### Callees 33 total
- MapOrderStateToFSMState
- FindLivePosition
- ResolveRemainingContracts
- BuildFSM
- LinkTargetOrderToFSM
- RegisterFSM
- HydrateFromOpenPositions

## Risk Assessment: MEDIUM

Factors:
1. LOW Blast Radius 0 external dependents
2. HIGH Complexity CYC 13 vs target 8
3. HIGH Churn 34 commits in 90 days
4. Deep Nesting 4 levels
5. Contained Scope same file callers
6. Many Dependencies 33 callees

## Refactoring Recommendation
PROCEED WITH CAUTION - Good candidate due to zero blast radius and high complexity.

## Jane Street Alignment
- Current CYC: 13
- Target CYC: 8
- Gap: 5 points
- Extraction Estimate: 2-3 helper methods
