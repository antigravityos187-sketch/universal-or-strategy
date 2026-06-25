# Phase 1: Scope Definition - EPIC-W7-017

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.97
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T20:06:03Z

## Epic Overview
**Target Method**: TryApplyConfigTarget_Value
**File**: src/V12_002.UI.IPC.Commands.Config.cs
**Current Complexity**: 22 (2.75x over Jane Street threshold of 8)
**Lines of Code**: 89
**Max Nesting Depth**: 5

## Scope Boundary Definition

### IN SCOPE
**Primary Extraction Target**:
- Method: TryApplyConfigTarget_Value (lines 209-298)
- Complexity: 22 to Target <=8 per extracted method
- Approach: Extract target-specific validation into separate methods

**Extraction Strategy**:
1. Extract T1 Validation to ValidateT1Target(string val, out double multiplier) - Target CYC <=8
2. Extract T2 Validation to ValidateT2Target(string val, out double multiplier) - Target CYC <=8
3. Extract T3 Validation to ValidateT3Target(string val, out double multiplier) - Target CYC <=8
4. Extract T4 Validation to ValidateT4Target(string val, out double multiplier) - Target CYC <=8
5. Extract T5 Validation to ValidateT5Target(string val, out double multiplier) - Target CYC <=8

**Files to Modify**: src/V12_002.UI.IPC.Commands.Config.cs

### OUT OF SCOPE
- TryApplyConfigTargets (line 196) - Parent method
- HandleConfigCommand (line 153) - Top-level handler
- ValidateIpcMultiplier (line 134) - Already extracted
- Other IPC command handlers
- Infrastructure (IPC protocol, logging, error handling, FSM)

## Risk Assessment
- Blast Radius: LOW (0 importers, risk score 0.0)
- Complexity Reduction: HIGH PRIORITY (CYC 22 to <=8)
- Maintenance Impact: POSITIVE (89 lines to 6 focused methods)

## Success Criteria
Phase 2: Detailed extraction plan, method signatures, complexity budget
Phase 5: All 5 methods extracted, build passes, CYC <=8 verified
Phase 5.V: Complexity audit, no errors, deploy-sync success

## Phase 1 Completion
- **Status**: COMPLETE
- **Scope Defined**: IN SCOPE (5 extractions) vs OUT OF SCOPE
- **Next Phase**: Phase 1.5 (Scope Boundary Validation)
