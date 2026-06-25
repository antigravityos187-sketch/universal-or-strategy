# Phase 0: Hotspot Analysis - EPIC-W7-084

## Agent Tracking
- **Agent Name**: v12-phase0-hotspot
- **Bobcoins Used**: 0.75
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-23T02:50:27Z

## Target Method
- **Method**: AuditFleet_CalculateExpectedActual
- **File**: src/V12_002.REAPER.Audit.cs
- **Line**: 382
- **Cyclomatic Complexity**: 16 (HIGH - exceeds threshold of 8)

## Complexity Metrics
- **Cyclomatic Complexity**: 16
- **Max Nesting Depth**: 7
- **Parameter Count**: 10 (out parameters)
- **Lines of Code**: 70
- **Assessment**: HIGH

### Complexity Breakdown
The method has 16 decision points, indicating significant branching logic. With 7 levels of nesting and 10 parameters (all out parameters), this method is doing too much work and violates the Single Responsibility Principle.

## Blast Radius Analysis
- **Direct Dependents**: 0
- **Importer Count**: 0
- **Overall Risk Score**: 0.0 (LOW)
- **Confirmed Importers**: None
- **Potential Importers**: None

### Risk Assessment
The blast radius is **LOW** - this is a private helper method with no external dependencies. Changes are isolated to the REAPER audit subsystem.

## Call Hierarchy

### Callers (Who calls this method)
1. **AuditSingleFleetAccount** (depth 1)
   - File: src/V12_002.REAPER.Audit.cs
   - Line: 121
   - Resolution: ast_resolved

2. **AuditApexPositions** (depth 2)
   - File: src/V12_002.REAPER.Audit.cs
   - Line: 16
   - Resolution: ast_resolved

### Callees (What this method calls) - 26 total
Key dependencies:
- **GetFsmExpectedPosition** - FSM state calculation
- **TryTerminateFollowerBracket** - FSM lifecycle management
- **LogBuffer.Format** - Logging infrastructure
- **ExpKey** - Expected position key generation
- **IsReaperFillGraceActive** - Grace period checks
- **RemoveFsmOrderIdMappings** - FSM cleanup
- **_positionPassFailedFirstSeen** - State tracking
- **_dispatchSyncPendingExpKeys** - Sync state management
- **_followerBrackets** - FSM collection access
- **_accountFillGraceTicks** - Grace period configuration

## Hotspot Analysis

### Why This Is a Hotspot
1. **High Complexity**: CYC=16 exceeds Jane Street threshold of 8
2. **Deep Nesting**: 7 levels makes reasoning difficult
3. **Parameter Explosion**: 10 out parameters indicates poor encapsulation
4. **Multiple Responsibilities**: Calculating expected/actual state, checking grace periods, managing FSM lifecycle

### Refactoring Opportunities
1. **Extract State Calculation**: Separate expected vs actual quantity logic
2. **Extract Grace Period Logic**: Isolate fill grace and sync pending checks
3. **Extract FSM Management**: Separate FSM collection filtering and termination
4. **Introduce Value Object**: Replace 10 out parameters with a result struct

### Jane Street Alignment
- **Current**: CYC=16 (FAILS Jane Street strict standard of <=8)
- **Target**: CYC <=8 per extracted method
- **Pattern**: Use FSM/Actor pattern for state management
- **Principle**: "Make illegal states unrepresentable" - use types instead of out parameters

## Risk Assessment: MEDIUM

### Risk Factors
- LOW Blast Radius: Private method, no external dependencies
- HIGH Complexity: CYC=16 with 7 nesting levels
- MEDIUM Coupling: 26 callees indicates tight coupling to REAPER subsystem
- LOW Churn Risk: Audit logic is relatively stable

### Overall Risk: MEDIUM
The method is complex but isolated. Refactoring is safe due to low blast radius, but requires careful testing of the 26 callee interactions.

## Recommended Approach
1. **Phase 1**: Define scope boundary (audit calculation only, no FSM lifecycle changes)
2. **Phase 2**: Design extraction strategy (4-5 helper methods, each CYC <=8)
3. **Phase 3**: DNA audit (verify no lock() usage, ASCII-only compliance)
4. **Phase 4**: Generate tickets (one per extracted method)
5. **Phase 5**: Execute extractions with TDD (xUnit tests for each helper)
6. **Phase 6**: Verify build + NinjaTrader F5 test

## Success Criteria
- All extracted methods have CYC <=8
- Original method reduced to orchestration logic (CYC <=5)
- No behavioral changes (audit logic identical)
- Build passes + NinjaTrader F5 successful
- xUnit tests cover all extracted methods
