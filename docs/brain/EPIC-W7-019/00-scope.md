# Phase 1: Scope Definition - EPIC-W7-019

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.00
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T20:05:03Z

## Target Method
- **Method**: TryHandleFleet_MoveTarget
- **File**: src/V12_002.UI.IPC.Commands.Fleet.cs
- **Line**: 645
- **Current CYC**: 17
- **Target CYC**: ≤ 8 per method

## Scope Boundary Analysis

### IN SCOPE ✅

#### Primary Extraction Target
**TryHandleFleet_MoveTarget** (src/V12_002.UI.IPC.Commands.Fleet.cs:645)
- **Rationale**: CYC 17 exceeds Jane Street threshold by 2.1x
- **Blast Radius**: ZERO (no external dependencies)
- **Single Caller**: TryHandleFleetCommand (excellent isolation)
- **High Fan-Out**: Calls 30 methods (orchestration complexity)

#### Extraction Strategy
Based on the 30 callees, extract these logical groups:

1. **Validation Logic Group**
   - ValidateTargetMoveAbsoluteRequest
   - ValidateMoveTargetRequest
   - Related validation checks

2. **Lookup Logic Group**
   - FindTargetOrderForAbsoluteMove
   - FindTargetOrderForPosition
   - Position/order lookup operations

3. **Calculation Logic Group**
   - CalculateAndValidateNewTargetPrice
   - Price calculation and validation

4. **Execution Logic Group**
   - ExecuteTargetAbsoluteMove
   - ExecuteFollowerTargetMove
   - ExecuteMasterTargetMove
   - MoveSpecificTargetAbsolute
   - MoveSpecificTarget

#### Expected Outcome
- **Main Method**: Orchestration only (CYC ≤ 8)
- **Helper Methods**: 3-4 extracted methods (each CYC ≤ 8)
- **Total Methods**: 4-5 methods replacing 1 complex method

### OUT OF SCOPE ❌

#### Caller Method
**TryHandleFleetCommand** (src/V12_002.UI.IPC.Commands.Fleet.cs:37)
- **Rationale**: Not the complexity hotspot
- **Role**: Entry point dispatcher
- **Action**: Leave unchanged

#### Downstream Methods (30 callees)
**All 30 methods called by TryHandleFleet_MoveTarget**
- **Rationale**: Already extracted and focused
- **Examples**:
  - MoveSpecificTargetAbsolute
  - MoveSpecificTarget
  - ValidateTargetMoveAbsoluteRequest
  - FindTargetOrderForAbsoluteMove
  - ExecuteTargetAbsoluteMove
  - ValidateMoveTargetRequest
  - FindTargetOrderForPosition
  - CalculateAndValidateNewTargetPrice
  - ExecuteFollowerTargetMove
  - ExecuteMasterTargetMove
- **Action**: Use as-is, do not modify

#### Other Fleet Commands
**Other methods in V12_002.UI.IPC.Commands.Fleet.cs**
- **Rationale**: Not identified as complexity hotspots
- **Action**: Leave unchanged

#### Logging Infrastructure
**LogBuffer methods**
- **Rationale**: Utility infrastructure
- **Action**: Use as-is

## Scope Justification

### Why This Scope?
1. **Surgical Precision**: Target only the CYC 17 method
2. **Zero Blast Radius**: No external dependencies to break
3. **Single Entry Point**: Easy to test and verify
4. **Clear Extraction Pattern**: Validation → Lookup → Calculation → Execution
5. **Jane Street Alignment**: Reduce CYC from 17 to ≤8 per method

### Risk Mitigation
- **Low Refactoring Risk**: Isolated method with single caller
- **No Cascading Changes**: Zero external dependencies
- **Testable**: Single entry point simplifies verification
- **Reversible**: Can rollback if issues arise

## Success Criteria
- [ ] TryHandleFleet_MoveTarget reduced to CYC ≤ 8
- [ ] 3-4 helper methods extracted (each CYC ≤ 8)
- [ ] All 30 callees remain unchanged
- [ ] TryHandleFleetCommand (caller) remains unchanged
- [ ] Build passes
- [ ] F5 in NinjaTrader successful

## Boundary Validation
- **Scope Creep Check**: ✅ PASS (only 1 method targeted)
- **Blast Radius Check**: ✅ PASS (zero external dependencies)
- **Complexity Check**: ✅ PASS (CYC 17 → target ≤8)
- **Jane Street Alignment**: ✅ PASS (cognitive simplicity mandate)
