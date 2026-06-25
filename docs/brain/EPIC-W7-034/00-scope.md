# Phase 1: Scope Definition - EPIC-W7-034

## Agent Tracking
- Agent Name: v12-phase1-scope
- Bobcoins Used: 0.18
- API Key: jCodemunch MCP
- Execution Time: 2026-06-24T19:27:34Z

## Epic Objective
Reduce cyclomatic complexity of ManageCIT method from CYC 11 to 8 or less through surgical extraction of nested control flow logic.

## Target Method Summary
- Method: ManageCIT
- File: src/V12_002.Orders.Management.Flatten.cs
- Line: 68
- Current CYC: 11
- Target CYC: 8 or less
- Blast Radius: 0.0 (ZERO external dependencies)
- Risk Level: LOW-MEDIUM

## Scope Boundary Definition

### IN SCOPE

#### Primary Target
ManageCIT method (lines 68-128 in src/V12_002.Orders.Management.Flatten.cs)
- Extract nested control flow (max nesting depth 5)
- Reduce CYC from 11 to 8 or less
- Preserve FSM/Actor pattern (Enqueue usage)
- Maintain ASCII-only compliance

#### Extraction Candidates
1. CIT Configuration Validation Logic
   - Already has helper: ValidateCitConfiguration (line 241)
   - Verify if validation is inline or delegated

2. Order Chase Decision Logic
   - Already has helper: ShouldChaseOrder (line 199)
   - Verify if all chase logic is extracted

3. Price Calculation Logic
   - Already has helper: CalculateNudgedPrice (line 228)
   - Verify if all price logic is extracted

4. Nudge Execution Logic
   - ExecuteFollowerNudge (line 146)
   - ExecuteLocalNudge (line 133)
   - Verify if all nudge logic is extracted

#### State Access (Read-Only)
- entryOrders (constant, line 200 in V12_002.cs)
- activePositions (constant, line 199 in V12_002.cs)
- _citNudgedKeys (constant, line 841 in V12_002.cs)

NO modifications to shared state structures.

### OUT OF SCOPE

#### External Dependencies
- NO external callers (zero blast radius)
- NO direct dependents
- Risk Score: 0.0

#### Sibling Methods (No Modification)
- ValidateCitConfiguration
- ShouldChaseOrder
- CalculateNudgedPrice
- ExecuteFollowerNudge
- ExecuteLocalNudge
- Enqueue (FSM/Actor pattern)

#### Shared State Structures
- entryOrders collection
- activePositions collection
- _citNudgedKeys collection

#### Other Methods
All other methods in V12_002.Orders.Management.Flatten.cs

#### Test Files
No test file modifications in this epic

## Success Criteria
- ManageCIT method CYC reduced from 11 to 8 or less
- Max nesting depth reduced from 5 to 3 or less
- All extracted logic delegated to helper methods
- FSM/Actor pattern preserved
- ASCII-only compliance maintained
- Zero regression risk confirmed
- Build passes after extraction
- F5 in NinjaTrader successful

## Scope Validation

### Boundary Clarity: EXCELLENT
IN SCOPE: ManageCIT method only (lines 68-128)
OUT OF SCOPE: All other methods, shared state, external files

### Risk Mitigation: EXCELLENT
- Zero blast radius
- Existing helpers proven
- Low churn, stable code

### Complexity Target: ACHIEVABLE
- CYC 11 to 8 or less (27% reduction)
- Nesting 5 to 3 or less (40% reduction)
- 61 lines, manageable extraction

## Next Phase: Phase 2 (Architecture Planning)
- Read ManageCIT source code
- Identify inline logic vs helper delegation
- Design extraction plan
- Generate architecture diagram
- Document extraction approach
