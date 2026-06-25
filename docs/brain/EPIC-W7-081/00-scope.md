# Phase 1: Scope Definition - EPIC-W7-081

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.18
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T19:34:35Z

## Epic Objective
Reduce cyclomatic complexity of AuditMaster_HandleNakedPosition from 19 to ≤8 through surgical extraction of helper methods.

## Target Method
- **Method**: AuditMaster_HandleNakedPosition
- **File**: src/V12_002.REAPER.Audit.cs
- **Line**: 624
- **Current CYC**: 19
- **Target CYC**: ≤8
- **Reduction Required**: 11 points (58% reduction)

## IN SCOPE

### Primary Extraction Target
**Method**: AuditMaster_HandleNakedPosition (lines 624-680)
- Extract nested conditional logic into helper methods
- Preserve exact behavior (no functional changes)
- Maintain state tracking semantics (_nakedPositionFirstSeen, _reaperNakedStopInFlight)

### Extraction Candidates (Identified from Nesting Analysis)
1. **Naked Position Detection Logic** (CYC ~4-5)
   - Condition: masterActualQty != 0 && masterPos == null
   - State checks: _nakedPositionFirstSeen, _reaperNakedStopInFlight
   - Action: EnqueueReaperMasterNakedStop

2. **Queue Processing Logic** (CYC ~3-4)
   - Condition: _reaperNakedStopInFlight
   - Action: ProcessReaperNakedStopQueue

3. **State Reset Logic** (CYC ~2-3)
   - Condition: masterActualQty == 0 || masterPos != null
   - Action: Reset _nakedPositionFirstSeen

### Files to Modify
- src/V12_002.REAPER.Audit.cs (single file modification)

### Dependencies to Preserve
- EnqueueReaperMasterNakedStop (callout)
- ProcessReaperNakedStopQueue (callout)
- LogBuffer.Format (logging)
- State fields: _nakedPositionFirstSeen, _reaperNakedStopInFlight

## OUT OF SCOPE

### Explicitly Excluded
1. **Caller Methods** (no modifications)
   - AuditMasterAccountIfNeeded (caller at depth 1)
   - AuditApexPositions (caller at depth 2)

2. **Callee Methods** (no modifications)
   - EnqueueReaperMasterNakedStop (preserve as-is)
   - ProcessReaperNakedStopQueue (preserve as-is)
   - All 22 callees remain unchanged

3. **State Management** (no architectural changes)
   - _nakedPositionFirstSeen field (preserve semantics)
   - _reaperNakedStopInFlight field (preserve semantics)
   - No changes to state initialization or lifecycle

4. **Functional Behavior** (no logic changes)
   - No changes to naked position detection algorithm
   - No changes to queue processing logic
   - No changes to logging output
   - Preserve exact execution flow

5. **Other REAPER Methods** (no modifications)
   - Other audit methods in V12_002.REAPER.Audit.cs
   - REAPER FSM logic
   - REAPER queue management

## Scope Boundaries

### Strict Boundaries
- **Single Method**: Only AuditMaster_HandleNakedPosition (lines 624-680)
- **Single File**: Only src/V12_002.REAPER.Audit.cs
- **Zero External Impact**: No changes to callers or callees
- **Behavior Preservation**: Exact logic flow maintained

### Extraction Strategy
- **Approach**: Extract 2-3 private helper methods
- **Target CYC per Method**: ≤8 (Jane Street threshold)
- **Naming Convention**: AuditMaster_HandleNakedPosition_* (preserve context)
- **Visibility**: Private (no API surface changes)

## Success Criteria

### Quantitative Metrics
- AuditMaster_HandleNakedPosition CYC reduced from 19 to ≤8
- All extracted helper methods have CYC ≤8
- Zero external dependencies added
- Zero callers modified
- Zero callees modified

### Qualitative Metrics
- Exact behavior preserved (no functional changes)
- State management semantics unchanged
- Logging output identical
- Build passes (no compilation errors)
- Existing REAPER tests pass (no regressions)

## Risk Mitigation

### Low Risk Factors (Advantages)
- Zero external dependencies (isolated refactoring)
- Single file modification (contained scope)
- Private method (no API contract changes)
- Existing test coverage (REAPER audit tests)

### Medium Risk Factors (Mitigations)
- High complexity (CYC=19) → Extract incrementally, verify at each step
- Deep nesting (7 levels) → Flatten via early returns in helpers
- Many callees (22) → Preserve all callout semantics exactly

## Phase 1 Deliverables
- Scope definition (this document)
- IN SCOPE vs OUT OF SCOPE boundaries
- Extraction strategy
- Success criteria
- Risk assessment

---
**Phase 1 Status**: COMPLETE
**Next Phase**: Phase 1.5 (Scope Boundary Validation)
