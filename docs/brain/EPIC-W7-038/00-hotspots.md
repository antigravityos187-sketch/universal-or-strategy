# Phase 0: Hotspot Analysis - EPIC-W7-038

**Agent**: v12-phase0-hotspot
**Epic**: EPIC-W7-038
**Target Method**: VerifyPhotonSlotIntegrity
**File**: V12_002.SIMA.Fleet.cs
**Baseline Complexity**: 14
**Date**: 2026-06-22

---

## Executive Summary

**Target Method**: `VerifyPhotonSlotIntegrity`
- **Current Complexity**: 14 (CYC)
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **Reduction Required**: 6 points (43% reduction)
- **File**: V12_002.SIMA.Fleet.cs
- **Classification**: Tier 1 hotspot (CYC 9-15)

---

## Complexity Analysis

### Method Signature
```csharp
private void VerifyPhotonSlotIntegrity()
```

### Complexity Breakdown
- **Cyclomatic Complexity**: 14
- **Cognitive Complexity**: High (nested conditionals, state checks)
- **Lines of Code**: ~80-100 (estimated)
- **Nesting Depth**: 3-4 levels

### Hotspot Characteristics
1. **State Validation**: Multiple FSM state checks
2. **Slot Management**: Photon slot allocation verification
3. **Error Handling**: Multiple error paths and logging
4. **Conditional Logic**: Nested if/else for slot integrity checks

---

## Blast Radius Analysis

### Direct Dependencies
- **Callers**: Fleet management methods
- **Callees**: FSM state accessors, logging methods
- **Shared State**: Photon slot arrays, FSM state dictionaries

### Impact Assessment
- **Risk Level**: MEDIUM
- **Test Coverage**: Requires FSM state validation tests
- **Breaking Change Potential**: LOW (internal method)

### Affected Components
1. SIMA Fleet Management
2. Photon Slot Allocation
3. FSM State Validation
4. Error Logging System

---

## Refactoring Strategy

### Extraction Candidates

#### 1. Slot Validation Logic (Priority: HIGH)
**Target**: Extract slot existence and allocation checks
**Estimated CYC Reduction**: 3-4 points
**New Method**: `ValidateSlotAllocation(int slotIndex)`

#### 2. FSM State Verification (Priority: HIGH)
**Target**: Extract FSM state consistency checks
**Estimated CYC Reduction**: 2-3 points
**New Method**: `VerifyFSMStateConsistency(SIMA_FSM fsm)`

#### 3. Error Reporting (Priority: MEDIUM)
**Target**: Extract error logging and reporting
**Estimated CYC Reduction**: 1-2 points
**New Method**: `ReportSlotIntegrityError(string errorType, int slotIndex)`

### Expected Outcome
- **Post-Refactor CYC**: 6-8 (target: ≤8)
- **New Methods**: 3 helper methods
- **Maintainability**: Significantly improved
- **Testability**: Each extracted method independently testable

---

## Jane Street Alignment

### Applicable Patterns
1. **Correctness by Construction**: Validate slot state at compile-time where possible
2. **Single Responsibility**: Each validation method checks one aspect
3. **Fail-Fast**: Early returns for invalid states
4. **Explicit State**: Clear error messages for each failure mode

### Complexity Reduction Principles
- **Cognitive Simplicity**: Each method does one thing
- **Exhaustive Testing**: Smaller methods = easier to test all paths
- **Race Condition Auditing**: Simpler logic = easier to verify lock-free correctness

---

## Risk Assessment

### Technical Risks
- **LOW**: Method is internal, well-encapsulated
- **LOW**: No external API surface changes
- **MEDIUM**: Requires careful FSM state handling

### Mitigation Strategy
1. Extract methods one at a time
2. Add unit tests for each extracted method
3. Verify FSM state consistency after each extraction
4. Run full integration tests after completion

---

## Success Criteria

### Phase 5 Validation
- [ ] All extracted methods have CYC ≤8
- [ ] Original method has CYC ≤8
- [ ] Unit tests added for each extracted method
- [ ] Integration tests pass
- [ ] No FSM state corruption
- [ ] Build passes with zero errors
- [ ] deploy-sync.ps1 executed successfully

### Quality Gates
- [ ] CSharpier formatting check passes
- [ ] Roslyn analyzer reports zero violations
- [ ] ASCII-only compliance verified
- [ ] No lock() statements introduced

---

## Metadata

**Bobcoins Used**: 4 MCP calls (jCodemunch)
- get_symbol_complexity: 1 call
- get_blast_radius: 1 call
- get_hotspots: 1 call
- get_call_hierarchy: 1 call

**API Key**: jCodemunch MCP server
**Execution Time**: <2 minutes
**Agent Mode**: v12-phase0-hotspot (ask mode)

---

## Next Steps

**Phase 1**: Scope Definition
- Define exact extraction boundaries
- Identify method signatures for extracted helpers
- Document FSM state dependencies

**Phase 1.5**: Scope Boundary Validation
- Verify no scope creep
- Confirm CYC reduction estimates
- Validate Jane Street pattern alignment

**Phase 2**: Architecture Planning
- Design extraction sequence
- Plan test coverage strategy
- Document rollback procedures
