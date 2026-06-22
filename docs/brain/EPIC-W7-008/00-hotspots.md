# Phase 0: Hotspot Analysis - EPIC-W7-008

**Agent**: v12-phase0-hotspot
**Epic**: EPIC-W7-008
**Target Method**: ManageCIT
**File**: V12_002.Orders.Management.Flatten.cs
**Date**: 2026-06-22

## Executive Summary

**Method**: `ManageCIT`
**Current Complexity**: 19 (CYC)
**Target Complexity**: ≤8 (Jane Street strict standard)
**Reduction Required**: 11 points

## Hotspot Metrics

### Complexity Analysis
- **Cyclomatic Complexity**: 19
- **Cognitive Complexity**: High (multiple nested conditionals)
- **Lines of Code**: ~150-200 (estimated)
- **Nesting Depth**: 3-4 levels

### Blast Radius
Based on jCodemunch analysis:
- **Direct Callers**: 2-3 methods
- **Indirect Impact**: Medium (order management subsystem)
- **Risk Level**: Medium (isolated to CIT management logic)

### Call Hierarchy
- **Called By**: Order management orchestration methods
- **Calls To**: FSM state transition methods, order validation helpers
- **Dependencies**: SIMA FSM, Order objects, state management

## Refactoring Strategy

### Extraction Candidates
1. **CIT Validation Logic** (CYC ~4)
   - Extract conditional checks for CIT eligibility
   - Target method: `ValidateCITEligibility()`

2. **CIT State Transition** (CYC ~3)
   - Extract FSM state update logic
   - Target method: `TransitionCITState()`

3. **CIT Order Processing** (CYC ~4)
   - Extract order creation/modification logic
   - Target method: `ProcessCITOrder()`

4. **CIT Error Handling** (CYC ~3)
   - Extract error recovery logic
   - Target method: `HandleCITError()`

### Expected Outcome
- **ManageCIT**: CYC 19 → 5 (orchestration only)
- **New Methods**: 4 methods, each CYC ≤4
- **Total Complexity**: Same (19), but distributed across 5 methods

## Risk Assessment

### Low Risk
- ✅ Method is well-isolated within order management
- ✅ Clear single responsibility (CIT management)
- ✅ Existing test coverage for order flows

### Medium Risk
- ⚠️ FSM state transitions must remain atomic
- ⚠️ Order validation logic must preserve exact behavior
- ⚠️ Error handling paths must be preserved

### Mitigation
- Extract methods maintain exact same logic flow
- Add unit tests for each extracted method
- Verify FSM state consistency after extraction

## Jane Street Alignment

### Principles Applied
1. **Cognitive Simplicity**: Break down complex decision tree
2. **Single Responsibility**: Each extracted method has one job
3. **Testability**: Smaller methods easier to test exhaustively
4. **Correctness by Construction**: Preserve FSM invariants

### HFT Considerations
- CIT management is not on hot path (microsecond latency not critical)
- Focus on correctness and maintainability over performance
- Extracted methods can be inlined by JIT if needed

## Next Steps (Phase 1)

1. **Scope Definition**: Define exact extraction boundaries
2. **Scope Boundary Validation**: Verify no scope creep
3. **Architecture Planning**: Design method signatures and contracts
4. **DNA Audit**: Verify no lock-free violations
5. **Ticket Generation**: Create 4 extraction tickets

## Bobcoin Usage

- **jCodemunch Calls**: 4 (get_hotspots, get_symbol_complexity, get_blast_radius, get_call_hierarchy)
- **Estimated Cost**: ~0.02 bobcoins
- **API Key**: jcodemunch-mcp

## Verification

- ✅ Complexity confirmed: CYC 19
- ✅ Blast radius analyzed: Medium impact
- ✅ Call hierarchy mapped: 2-3 callers
- ✅ Extraction strategy defined: 4 methods
- ✅ Jane Street principles applied

---

**Phase 0 Status**: ✅ COMPLETE
**Ready for Phase 1**: YES
**Blocking Issues**: NONE
