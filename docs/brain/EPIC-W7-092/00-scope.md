# Phase 1: Scope Definition - EPIC-W7-092

Agent: v12-phase1-scope
Date: 2026-06-24T20:09:05Z
Input: docs/brain/EPIC-W7-092/00-hotspots.md

## Target Method
- Method: SetRmaAnchorFromIpc
- File: src/V12_002.SIMA.cs
- Line: 241
- Current CYC: 13
- Target CYC: ≤8 (Jane Street threshold)

## Scope Boundary Analysis

### IN SCOPE
1. Primary Extraction Target
   - Method: SetRmaAnchorFromIpc (CYC 13 to ≤8)
   - Rationale: Exceeds Jane Street threshold by 5 points
   - Approach: Extract conditional logic into helper methods

2. Complexity Reduction Strategy
   - Extract IPC command validation logic
   - Extract RMA anchor calculation logic
   - Extract state update logic
   - Maintain single responsibility per extracted method

3. Testing Requirements
   - Unit tests for extracted helper methods
   - Integration test for main method flow
   - Edge case coverage for IPC command parsing

### OUT OF SCOPE
1. Adjacent Methods (unless CYC >8)
   - Other IPC command handlers
   - RMA calculation methods in other contexts
   - State management methods

2. Architectural Changes
   - No FSM/Actor pattern changes
   - No IPC protocol modifications
   - No state machine refactoring

3. Cross-File Dependencies
   - No changes to V12_002.cs main file
   - No changes to other SIMA partial classes
   - No changes to IPC infrastructure

## Extraction Boundaries

### Method Signature (PRESERVE)
private void SetRmaAnchorFromIpc(string command)

### Internal Logic (REFACTOR)
- Conditional branches causing high CYC
- Nested validation checks
- State update sequences

### External Contracts (PRESERVE)
- IPC command format expectations
- State mutation side effects
- Logging behavior

## Success Criteria
1. SetRmaAnchorFromIpc CYC reduced to ≤8
2. All extracted methods have CYC ≤8
3. Zero compilation errors
4. All unit tests pass
5. Integration test confirms behavior unchanged
6. deploy-sync.ps1 executes successfully

## Risk Mitigation
- Blast Radius: LOW (0 detected importers/dependents)
- Call Complexity: LOW (isolated method, no detected callers)
- Testing Strategy: Unit tests for each extracted method
- Rollback Plan: Git revert if integration test fails

## Agent Tracking
- Agent: v12-phase1-scope
- Mode: plan
- Bobcoins Used: 0.75 (cumulative with Phase 0: 2.14)
- Tools Used: Sequential Thinking MCP (scope boundary validation)
