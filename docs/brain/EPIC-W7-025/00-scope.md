# Phase 1: Scope Definition - EPIC-W7-025

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.18
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T19:26:22Z

## Epic Overview
- **Target Method**: CheckFFMAConditions
- **File**: src/V12_002.Entries.FFMA.cs
- **Current CYC**: 16
- **Target CYC**: <=8 (Jane Street strict standard)
- **Blast Radius**: 0 (safe to refactor)

## Scope Boundary Definition

### IN SCOPE

#### Primary Target
- **CheckFFMAConditions method** (lines 43-109, CYC=16)
  - FFMA condition validation logic
  - Position sizing calculations
  - Compliance checks
  - Entry execution orchestration

#### Extraction Candidates
Based on the 60 callees and deep nesting (6 levels), extract:

1. **Position Sizing Logic**
   - Calls to V12_PureLogic.CalculatePositionSize
   - Calls to V12_002.CalculatePositionSize
   - Target: Extract to ValidateFFMAPositionSize() (CYC <=5)

2. **Compliance Validation**
   - Calls to V12_002.IsOrderAllowed
   - Account equity checks
   - Target: Extract to ValidateFFMACompliance() (CYC <=5)

3. **Entry Execution Orchestration**
   - Calls to V12_002.ExecuteFFMAEntry
   - Calls to V12_002.ExecuteSmartDispatchEntry
   - Target: Extract to ExecuteFFMAEntryFlow() (CYC <=5)

4. **Logging and Diagnostics**
   - Calls to LogBuffer.Format
   - Diagnostic output
   - Target: Extract to LogFFMAConditionCheck() (CYC <=3)

#### Control Flow Simplification
- Replace deep nesting (6 levels) with early returns
- Extract nested conditionals to guard clauses
- Flatten if/else chains

### OUT OF SCOPE

#### Explicitly Excluded
1. **Other FFMA Methods**
   - ExecuteFFMAEntry (separate method, not in this epic)
   - DeactivateFFMAMode (separate method, not in this epic)
   - Any other methods in V12_002.Entries.FFMA.cs

2. **Callee Implementations**
   - V12_PureLogic.CalculatePositionSize (external dependency)
   - V12_002.IsOrderAllowed (separate concern)
   - LogBuffer.Format (logging infrastructure)
   - V12_002.Enqueue (FSM/Actor infrastructure)

3. **IPC Communication**
   - SendResponseToRemote (separate concern)
   - IPC client management (separate concern)

4. **Target Calculation Logic**
   - CalculateTargetPrice (separate concern)
   - GetTargetDistribution (separate concern)

5. **Thread Management**
   - Actor thread management (separate concern)
   - Thread affinity validation (separate concern)

### INVESTIGATION REQUIRED

#### Zero Callers Issue
**CRITICAL**: Phase 0 detected **0 callers** for CheckFFMAConditions. Before extraction:

1. **Search for Dynamic Invocation**
   - Search codebase for string "CheckFFMAConditions"
   - Check for reflection-based calls
   - Check for delegate/event subscriptions

2. **Verify Entry Point**
   - Confirm if method is called from NinjaTrader lifecycle hooks
   - Check if method is called via OnBarUpdate or OnMarketData
   - Verify if method is truly dead code

3. **Decision Point**
   - If dead code: Document and skip extraction (no value)
   - If entry point: Proceed with extraction (high value)
   - If reflection-based: Proceed with caution (test thoroughly)

**Action**: Phase 2 MUST resolve this before architecture planning.

## Extraction Strategy

### Target Architecture
CheckFFMAConditions (CYC <=8)
- ValidateFFMAPositionSize() (CYC <=5)
- ValidateFFMACompliance() (CYC <=5)
- ExecuteFFMAEntryFlow() (CYC <=5)
- LogFFMAConditionCheck() (CYC <=3)

### Complexity Reduction Plan
- **Current**: 1 method, CYC=16
- **Target**: 5 methods, each CYC <=8
- **Total CYC**: 16 to 26 (distributed across 5 methods)
- **Cognitive Load**: HIGH to LOW (each method single-responsibility)

### Success Criteria
1. CheckFFMAConditions reduced to CYC <=8
2. All extracted methods have CYC <=8
3. No lock() blocks introduced (V12 DNA mandate)
4. ASCII-only compliance maintained
5. Build passes after extraction
6. F5 in NinjaTrader successful
7. Zero callers issue resolved (dead code or entry point confirmed)

## Risk Assessment

### Refactoring Risk: **LOW**
- Zero blast radius (0 importers)
- No downstream dependencies
- Safe to refactor without breaking other code

### Investigation Risk: **MEDIUM**
- Zero callers requires investigation
- Potential dead code (wasted effort if unused)
- Potential reflection-based invocation (harder to test)

### Complexity Risk: **LOW**
- Clear extraction boundaries
- Well-defined helper methods
- Jane Street patterns applicable

## Jane Street Alignment

### Current State
- **CYC**: 16 (FAILS Jane Street strict standard)
- **Nesting**: 6 levels (FAILS cognitive simplicity)
- **Fan-Out**: 60 callees (HIGH coupling)

### Target State
- **CYC**: <=8 per method (PASSES Jane Street GODMODE)
- **Nesting**: <=3 levels (PASSES cognitive simplicity)
- **Fan-Out**: <=15 callees per method (MODERATE coupling)

### Rationale
Jane Street HFT systems prioritize:
- Cognitive simplicity for microsecond-latency reasoning
- Exhaustive testing (exponential path growth with CYC)
- Race condition auditing in lock-free code

## Next Phase

**Phase 1.5 (Scope Boundary Validation)**:
1. Investigate zero callers issue
2. Confirm method is not dead code
3. Validate extraction boundaries
4. Verify no scope creep beyond FFMA logic

**Blocker**: Zero callers issue MUST be resolved before Phase 2.

## Conclusion

Scope is **WELL-DEFINED** with clear IN/OUT boundaries. The extraction will decompose CheckFFMAConditions into 4 helper methods, each with CYC <=8, following Jane Street strict standard.

**CRITICAL**: Zero callers issue requires investigation in Phase 1.5 before proceeding to architecture planning.
