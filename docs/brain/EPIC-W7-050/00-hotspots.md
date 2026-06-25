# Phase 0: Hotspot Analysis - EPIC-W7-050

## Agent Tracking
- **Agent Name**: v12-phase0-hotspot
- **Bobcoins Used**: 2.39
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-23T02:45:05Z

## Target Method
- **Method**: FleetSync_SyncFollowersToLevel
- **File**: src/V12_002.Trailing.cs
- **Line**: 142
- **Cyclomatic Complexity**: 13 (NOTE: Task stated 9, but jCodemunch reports 13)

## Complexity Metrics
**Source**: get_symbol_complexity tool

- **Cyclomatic Complexity**: 13
- **Max Nesting Depth**: 5
- **Parameter Count**: 4
- **Lines of Code**: 50
- **Assessment**: HIGH

**Analysis**: The method exceeds the Jane Street threshold of CYC ≤ 8, indicating it requires refactoring to improve cognitive simplicity and testability.

## Blast Radius
**Source**: get_blast_radius tool (depth=1)

- **Direct Dependents**: 0
- **Importer Count**: 0
- **Overall Risk Score**: 0.0
- **Confirmed Files**: 0
- **Potential Files**: 0

**Analysis**: The blast radius analysis shows zero importers, which initially suggests low impact. However, this contradicts the call hierarchy data (see below), indicating the method is called internally within the same file or via dynamic dispatch.

## Call Hierarchy
**Source**: get_call_hierarchy tool (depth=3, direction=both)

### Callers (2)
1. **ManageTrail_RunFleetSymmetrySync** (depth 1)
   - File: src/V12_002.Trailing.cs
   - Line: 99
   - Resolution: ast_resolved

2. **ManageTrailingStops** (depth 2)
   - File: src/V12_002.Trailing.cs
   - Line: 39
   - Resolution: ast_resolved

### Callees (48 total)
**Key Dependencies**:
- **CalculateStopForLevel** (depth 1) - Stop price calculation
- **UpdateStopOrder** (depth 1) - Order update logic
- **ValidateStopPrice** (depth 2) - Price validation
- **InitiateStopReplacement** (depth 2) - Stop replacement flow
- **CreateDirectStopOrder** (depth 2) - Order creation
- **HandleUpdateException** (depth 2) - Error handling
- **LogBuffer.Format** (depth 1) - Logging

**Analysis**: The method has 48 callees across 3 depth levels, indicating significant internal complexity. It orchestrates trailing stop synchronization across fleet accounts, touching order management, validation, and error handling subsystems.

## Hotspot Ranking
**Source**: get_hotspots tool (top 50, 90 days)

**Result**: FleetSync_SyncFollowersToLevel does NOT appear in the top 50 hotspots.

**Top 5 Hotspots for Context**:
1. HydrateFromOpenPositions (CYC 34, hotspot score 120.88)
2. IsCommandForThisInstrument (CYC 38, hotspot score 109.83)
3. HandleTerminated (CYC 30, hotspot score 102.04)
4. SweepBrokerOrders (CYC 28, hotspot score 99.55)
5. HydrateWorkingOrdersFromBroker (CYC 23, hotspot score 81.77)

**Analysis**: The target method has relatively low churn (not in top 50), suggesting it is stable code. However, its CYC of 13 still warrants refactoring for maintainability.

## Risk Assessment

### Overall Risk: MEDIUM

**Rationale**:
- ✅ **Low Churn**: Not in top 50 hotspots (stable code)
- ✅ **Low Blast Radius**: Zero external importers (contained impact)
- ⚠️ **High Complexity**: CYC 13 exceeds Jane Street threshold (CYC ≤ 8)
- ⚠️ **Deep Call Tree**: 48 callees across 3 levels (orchestration complexity)
- ⚠️ **High Nesting**: Max nesting depth of 5 (cognitive load)

### Refactoring Priority: MEDIUM-HIGH

**Justification**:
1. **Complexity Violation**: CYC 13 vs. threshold 8 (62% over limit)
2. **Stable Code**: Low churn reduces regression risk
3. **Contained Impact**: Internal callers only (safe to refactor)
4. **Orchestration Pattern**: 48 callees suggest extraction opportunities

### Recommended Approach
1. Extract validation logic (ValidateStopPrice calls)
2. Extract stop replacement flow (InitiateStopReplacement, CreateDirectStopOrder)
3. Extract error handling (HandleUpdateException)
4. Target: Reduce CYC from 13 to ≤8 via 3-5 helper methods

## Data Discrepancy Note
**CRITICAL**: The task brief stated CYC=9, but jCodemunch reports CYC=13. This 44% discrepancy suggests:
1. The complexity audit data is stale, OR
2. The method was modified since the audit, OR
3. Different complexity calculation methods

**Action Required**: Verify current source code before proceeding to Phase 1.

## Phase 0 Completion
- ✅ Hotspot analysis complete
- ✅ Blast radius assessed
- ✅ Call hierarchy mapped
- ✅ Complexity metrics gathered
- ✅ Risk assessment documented
- ✅ Manifest updated

**Next Phase**: Phase 1 (Scope Definition)
