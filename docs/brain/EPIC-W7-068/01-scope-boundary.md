
**Agent**: v12-phase0-hotspot
**Epic**: EPIC-W7-068
**Target Method**: TryParseTargetMode
**File**: V12_002.UI.IPC.cs
**Baseline Complexity**: 13
**Date**: 2026-06-22

## Executive Summary

This hotspot analysis targets the `TryParseTargetMode` method in V12_002.UI.IPC.cs with a cyclomatic complexity of 13, exceeding the Jane Street threshold of ≤8.

## jCodemunch Hotspot Analysis

### Method Complexity Profile
- **Cyclomatic Complexity**: 13
- **Threshold Violation**: +5 over Jane Street standard (≤8)
- **File**: V12_002.UI.IPC.cs
- **Category**: UI/IPC command parsing

### Blast Radius Assessment
Based on jCodemunch analysis:
- **Direct Callers**: IPC command processing pipeline
- **Impact Scope**: UI command parsing subsystem
- **Risk Level**: MEDIUM - Isolated to IPC layer

### Call Hierarchy
- **Upstream**: Called by IPC command dispatcher
- **Downstream**: Calls mode validation logic
- **Coupling**: Moderate - contained within IPC subsystem

## Hotspot Ranking Context

From jCodemunch top 50 hotspots analysis:
- This method ranks in the complexity reduction backlog
- Part of systematic CYC ≤8 enforcement wave
- Isolated scope reduces refactoring risk

## Refactoring Recommendation

**Priority**: MEDIUM
**Approach**: Extract mode parsing logic into helper methods
**Target Complexity**: ≤8 per method
**Estimated Tickets**: 1-2

### Extraction Candidates
1. Mode string validation logic
2. Mode enum conversion logic
3. Error handling paths

## Risk Assessment

**Technical Risk**: LOW
- Well-defined input/output contract
- Isolated to IPC layer
- No FSM state dependencies

**Business Risk**: LOW
- Non-critical path (UI commands)
- Existing test coverage available
- Rollback straightforward

## Success Criteria

- [ ] All extracted methods have CYC ≤8
- [ ] Original functionality preserved
- [ ] Unit tests pass
- [ ] Build verification successful
- [ ] deploy-sync.ps1 executed

## Agent Tracking

- **Agent Name**: v12-phase0-hotspot
- **Bobcoins Used**: ~150 (jCodemunch queries)
- **API Key**: jcodemunch-mcp
- **Execution Time**: <2 minutes

## Next Phase

Proceed to Phase 1 (Scope Definition) to define extraction boundaries.

# Phase 1: Scope Boundary - EPIC-W7-068

**Agent**: v12-phase1-scope
**Epic**: EPIC-W7-068
**Target Method**: TryParseTargetMode
**File**: V12_002.UI.IPC.cs
**Baseline Complexity**: 13
**Target Complexity**: ≤8
**Date**: 2026-06-24

## Executive Summary

This document defines the extraction scope for reducing TryParseTargetMode from CYC 13 to ≤8 through targeted helper method extraction.

## IN SCOPE

### Primary Target
- Method: TryParseTargetMode in V12_002.UI.IPC.cs
- Current CYC: 13
- Target CYC: ≤8

### Extraction Candidates

#### 1. Mode String Validation Logic
- Scope: Input validation and sanitization
- Complexity Contribution: ~3-4 branches
- Extract To: ValidateModeString(string input)
- Rationale: Isolates validation concerns

#### 2. Mode Enum Conversion Logic
- Scope: String-to-enum parsing with error handling
- Complexity Contribution: ~4-5 branches
- Extract To: ConvertToTargetModeEnum(string modeString)
- Rationale: Separates conversion logic from validation

#### 3. Error Handling Paths
- Scope: Error message construction and logging
- Complexity Contribution: ~2-3 branches
- Extract To: HandleParseError(string input, string reason)
- Rationale: Centralizes error handling

### Files to Modify
- src/V12_002.UI.IPC.cs - Primary refactoring target

### Files to Create
- None (extractions stay in same file)

### Testing Requirements
- Unit tests for each extracted method
- Integration test for original TryParseTargetMode behavior
- Verify IPC command pipeline still functions

## OUT OF SCOPE

### Explicitly Excluded

#### 1. IPC Command Dispatcher
- Rationale: Caller of target method, not part of this epic
- Risk: Expanding scope increases blast radius
- Future Work: Separate epic if needed

#### 2. Mode Validation Business Logic
- Rationale: Downstream dependency, separate concern
- Risk: Mixing validation rules with parsing logic
- Future Work: EPIC-W7-069 (if needed)

#### 3. Other IPC Parsing Methods
- Rationale: Each method is independent epic
- Risk: Scope creep, context window exhaustion
- Future Work: Separate epics per method

#### 4. UI Command Response Handling
- Rationale: Separate subsystem, different complexity profile
- Risk: Unrelated to parsing logic
- Future Work: Not planned

### Architectural Boundaries

#### Do NOT Touch
- FSM state machine logic
- Order management subsystem
- ATM (Automated Trade Management) logic
- Drawing/rendering subsystem
- Any lock-free Actor patterns

#### Preserve Contracts
- Method signature of TryParseTargetMode must remain unchanged
- Return type and parameter list are immutable
- Existing callers must work without modification

## Scope Validation

### Jane Street Alignment
- Single Responsibility: Each extracted method has one job
- Cognitive Simplicity: All methods ≤8 CYC
- Testability: Each method independently testable
- No Illegal States: Type system enforces valid modes

### V12 DNA Compliance
- ASCII-Only: No Unicode in string literals
- Lock-Free: No synchronization primitives (N/A for parsing)
- Correctness by Construction: Invalid modes rejected at parse time
- Hard-Link Integrity: deploy-sync.ps1 after changes

### Risk Mitigation
- Blast Radius: Contained to IPC layer
- Rollback Plan: Git revert + deploy-sync.ps1
- Testing: Unit + integration tests before merge
- Verification: F5 in NinjaTrader IDE

## Success Criteria

### Code Quality
- TryParseTargetMode reduced to CYC ≤8
- All extracted methods have CYC ≤8
- No new complexity violations introduced

### Functional Correctness
- All existing IPC commands still parse correctly
- Error handling behavior unchanged
- No regressions in UI command processing

### Build & Deployment
- dotnet build passes with zero errors
- deploy-sync.ps1 executes successfully
- F5 in NinjaTrader shows correct BUILD_TAG
- No compilation warnings introduced

### Testing
- Unit tests for each extracted method
- Integration test for TryParseTargetMode
- All tests pass (100% pass rate)

## Estimated Effort

- Tickets: 1-2
- Complexity: LOW
- Risk: LOW
- Bobcoins: ~300-400 (jCodemunch + implementation)
- Time: 1-2 agent sessions

## Next Phase

Proceed to Phase 2 (Architecture Planning) to design extraction strategy.

---

**Scope Boundary Validated**: 2026-06-24
**Agent**: v12-phase1-scope
**Status**: READY FOR PHASE 2
