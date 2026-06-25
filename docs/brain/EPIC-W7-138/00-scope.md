# Phase 1: Scope Definition - EPIC-W7-138

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.18
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T19:43:12Z

## Epic Objective
Reduce cyclomatic complexity of ManageTrail_RunPerTradeBranches from 11 to ≤8 by extracting branch selection logic.

## Target Method
- **Method**: ManageTrail_RunPerTradeBranches
- **File**: src/V12_002.Trailing.cs
- **Line**: 240
- **Current CYC**: 11
- **Target CYC**: ≤8

## IN SCOPE

### Primary Extraction Target
1. **Branch Selection Logic** (Lines 240-256)
   - Decision logic that routes to TREND_E1, TREND_E2, or RETEST handlers
   - Conditional branching causing CYC 11
   - Extract to: SelectTrailHandler() or use strategy pattern

### Scope Boundaries
- **Start**: Method entry at line 240
- **End**: Method exit after handler delegation
- **Depth**: Single method only (ManageTrail_RunPerTradeBranches)

### Success Criteria
1. ManageTrail_RunPerTradeBranches reduced to CYC ≤8
2. Extracted method(s) each have CYC ≤8
3. Zero compilation errors
4. All existing tests pass
5. Single caller (ManageTrailingStops) remains unchanged

## OUT OF SCOPE

### Trail Handler Methods (DO NOT MODIFY)
1. TrailHandler_TREND_E1 (line 257)
2. TrailHandler_TREND_E2 (line 312)
3. TrailHandler_RETEST (line 342)

**Rationale**: These are callees with their own complexity profiles. Modifying them would expand scope beyond the single-method mandate.

### Supporting Infrastructure (DO NOT MODIFY)
1. UpdateStopOrder (src/V12_002.Trailing.StopUpdate.cs:84)
2. LogBuffer methods (src/V12_002.Perf.LogBuffer.cs)
3. Stop order management methods (30 callees in call chain)

**Rationale**: These are shared utilities used across multiple methods. Changes would violate the isolated blast radius advantage.

### Caller Method (DO NOT MODIFY)
1. ManageTrailingStops (line 39)

**Rationale**: Single caller should remain unchanged. Only the internal implementation of ManageTrail_RunPerTradeBranches is in scope.

## Extraction Strategy

### Recommended Approach: Strategy Pattern
Extract branch selection logic into separate method that returns appropriate handler, reducing main method complexity.

### Alternative Approach: Lookup Table
If branching is based on enum/constant values, use dictionary lookup to eliminate conditionals entirely (CYC 1).

## Risk Mitigation

### Pre-Extraction Checklist
1. Verify test coverage exists for all branch paths
2. Confirm blast radius remains 0 (no external dependents)
3. Query Jane Street KB for trail handler patterns
4. Run complexity audit before/after extraction

### Validation Gates
1. **Build Gate**: Zero compilation errors
2. **Test Gate**: All existing tests pass
3. **Complexity Gate**: All methods CYC ≤8
4. **Sync Gate**: deploy-sync.ps1 executes successfully

## Jane Street Alignment

### Principles Applied
1. **Cognitive Simplicity**: CYC ≤8 for microsecond-latency reasoning
2. **Exhaustive Testing**: Reduced path growth (2^11 → 2^8 max)
3. **Race Condition Auditing**: Simpler logic = easier lock-free verification

### Pattern Reference
- Query: python scripts/query_kb.py "trail handler patterns"
- Query: python scripts/query_kb.py "strategy pattern complexity"

## Scope Boundary Validation

### What Changes
- ManageTrail_RunPerTradeBranches internal implementation
- New extracted method(s) for branch selection

### What Stays Unchanged
- All 30 callees in the call chain
- Single caller (ManageTrailingStops)
- Trail handler methods (TREND_E1, TREND_E2, RETEST)
- Public API surface

## Next Phase (Phase 1.5)
Validate this scope boundary using Sequential Thinking MCP to ensure no scope creep.
