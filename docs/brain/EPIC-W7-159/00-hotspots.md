# Phase 0: Hotspot Analysis - EPIC-W7-159

## Agent Tracking
- **Agent Name**: v12-phase0-hotspot
- **Bobcoins Used**: 0.78
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-23T03:34:11Z

## Target Method
- **Method**: TryHandleFleet_LongShort
- **File**: src/V12_002.UI.IPC.Commands.Fleet.cs
- **Line**: 383
- **Cyclomatic Complexity**: 21
- **Lines of Code**: 76

## Complexity Metrics
- **Cyclomatic Complexity**: 21 (HIGH - exceeds threshold of 8)
- **Max Nesting Depth**: 5
- **Parameter Count**: 2
- **Assessment**: HIGH complexity
- **Hotspot Score**: 46.14 (ranked #38 in top 50 hotspots)
- **Churn**: 8 commits in last 90 days

## Blast Radius Analysis
- **Direct Dependents**: 0
- **Importer Count**: 0
- **Overall Risk Score**: 0.0 (LOW)
- **Confirmed Impact Files**: 0
- **Potential Impact Files**: 0

**Interpretation**: This method has ZERO blast radius - no other code directly depends on it. This makes it an IDEAL refactoring candidate with minimal regression risk.

## Call Hierarchy

### Callers (1)
1. **TryHandleFleetCommand** (src/V12_002.UI.IPC.Commands.Fleet.cs:37)
   - Resolution: ast_resolved
   - This is the only entry point to TryHandleFleet_LongShort

### Callees (40)
The method calls 40 different symbols across multiple files:

**Core Logic**:
- MetadataGuardDuplicate (duplicate check)
- CalculateATRStopDistance (risk calculation)
- CalculatePositionSize (position sizing)
- ExecuteMultiAccountBracket (order execution)
- ExecuteMultiAccountMarket (market order execution)

**Actor Pattern**:
- Enqueue (FSM/Actor pattern - V12 DNA compliant)
- IsActorThread (thread safety check)
- TryDrain (queue processing)
- ScheduleActorDrain (async scheduling)

**State Management**:
- AddExpectedPositionDeltaLocked (position tracking)
- ExpKey (expected position key generation)
- IsFleetAccount (account validation)

**Logging**:
- LogBuffer.Format (performance logging)

## Risk Assessment

### Overall Risk: **MEDIUM-LOW**

**Risk Factors**:
1. LOW Blast Radius: Zero direct dependents
2. HIGH Complexity: CYC=21 (2.6x over threshold of 8)
3. Moderate Churn: 8 commits (not volatile)
4. Single Caller: Only one entry point (TryHandleFleetCommand)
5. Actor Pattern: Uses Enqueue (lock-free, V12 DNA compliant)

**Refactoring Safety**:
- SAFE: Zero blast radius means no downstream breakage risk
- TESTABLE: Single entry point makes testing straightforward
- ISOLATED: Method is self-contained within Fleet command handling

**Recommended Approach**:
1. Extract ATR calculation logic (CalculateATRStopDistance calls)
2. Extract position sizing logic (CalculatePositionSize calls)
3. Extract order execution logic (ExecuteMultiAccountBracket/Market calls)
4. Target: Reduce CYC from 21 to <=8 per extracted method

## Hotspot Context
This method ranks #38 in the top 50 hotspots with a score of 46.14, calculated as:
- Hotspot Score = Cyclomatic Complexity x log(1 + churn)
- 21 x log(1 + 8) = 46.14

**Comparison to Top Hotspots**:
- #1: HydrateFromOpenPositions (CYC=34, score=120.88)
- #38: TryHandleFleet_LongShort (CYC=21, score=46.14)

This is a mid-tier hotspot - not the most critical, but still exceeds the Jane Street threshold of CYC<=8.

## Phase 0 Conclusion
PROCEED TO PHASE 1: This method is a viable refactoring candidate with:
- Clear complexity reduction opportunity (CYC 21->8)
- Minimal regression risk (zero blast radius)
- Well-defined extraction targets (ATR, sizing, execution)
- V12 DNA compliance (already uses Actor pattern)
