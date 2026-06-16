# Phase 1.0: Scope Definition - EPIC-CCN-031

## Epic Metadata
- **Epic ID**: EPIC-CCN-031
- **Phase**: 1.0 (Scope Definition)
- **Date**: 2026-06-15
- **Status**: APPROVED

## Target Method
- **Method Name**: AuditMaster_HandleNakedPosition
- **File**: src/V12_002.REAPER.Audit.cs
- **Current Complexity**: 15 (at V12 DNA threshold)
- **Target Complexity**: <=8 (Jane Street strict standard)
- **Subsystem**: REAPER Audit

## Extraction Scope (SINGLE METHOD ONLY)

### Whats IN Scope
1. **Method Body**: Complete refactoring of AuditMaster_HandleNakedPosition internal logic
2. **Helper Method Extraction**: Break into 2-3 focused helper methods
3. **Complexity Reduction**: Reduce main method from CYC 15 to <=8
4. **Pattern Compliance**: Ensure lock-free Actor/FSM pattern maintained

### Whats OUT of Scope
1. **Callers**: No changes to methods that call AuditMaster_HandleNakedPosition
2. **Callees**: No changes to methods called by AuditMaster_HandleNakedPosition
3. **Other Methods**: No changes to other methods in V12_002.REAPER.Audit.cs
4. **File Structure**: No changes to class structure, namespaces, or imports
5. **Behavior Changes**: Zero functional changes - pure refactoring only
6. **Scope Creep**: No "while were here" improvements

## Extraction Strategy

### Recommended Decomposition
Based on Phase 0 hotspot analysis, extract into 3 helper methods:

1. **ValidateNakedPositionState** (CYC <=3)
   - Validate position state preconditions
   - Return validation result
   - Pure function - no side effects

2. **CalculateNakedPositionRisk** (CYC <=4)
   - Calculate risk metrics for naked position
   - Return risk assessment
   - Pure function - no side effects

3. **UpdateAuditStateForNakedPosition** (CYC <=3)
   - Update audit state via Actor/FSM Enqueue
   - Maintain lock-free pattern
   - Side effects isolated to state mutation

4. **Main Method** (CYC <=8)
   - Orchestrate validation -> calculation -> state update
   - Minimal branching logic
   - Clear control flow

### Complexity Budget
- Original: 15
- Target breakdown:
  - ValidateNakedPositionState: <=3
  - CalculateNakedPositionRisk: <=4
  - UpdateAuditStateForNakedPosition: <=3
  - Main orchestration: <=8
- **Total**: <=18 (distributed across 4 methods)

## Success Criteria

### Functional Requirements
- All existing tests pass (zero regressions)
- Behavior identical to original implementation
- Audit trail integrity preserved
- No performance degradation

### Architectural Requirements
- Lock-free Actor/FSM pattern maintained
- No lock() statements introduced
- State mutations via Enqueue only
- ASCII-only compliance (no Unicode)

### Quality Requirements
- Main method complexity reduced to <=8
- Each helper method complexity <=5
- Unit tests added for extracted methods
- Code coverage maintained or improved

### V12 DNA Compliance
- "Make illegal states unrepresentable" principle applied
- Correctness by construction
- Jane Street cognitive simplicity standard met
- No runtime guards for design-time constraints

## Risk Assessment

### Risk Level: LOW-MEDIUM
- **Complexity**: At threshold (15) - manageable extraction
- **Subsystem**: REAPER audit is core but well-tested
- **Pattern**: Lock-free Actor pattern well-established
- **Testing**: Existing test coverage provides safety net

### Mitigation Strategy
1. **Pre-Refactoring**: Verify existing test coverage
2. **Incremental**: Extract one helper at a time
3. **Verification**: Run tests after each extraction
4. **Rollback**: Git checkpoint before each step

## Jane Street Alignment

### Cognitive Simplicity
- Functions with CYC >15 are hard to reason about under microsecond latency
- Target CYC <=8 aligns with Jane Street HFT standards
- Simple functions = easier to test, audit, and optimize

### Testing Philosophy
Per Jane Street "Why Testing Is Hard" (Will Wilson):
- Small, focused functions are easier to test exhaustively
- Pure functions (validation, calculation) enable property-based testing
- Side effects isolated to single method (state update)

## Next Steps (Phase 2)
1. Generate detailed implementation plan
2. Design extraction boundaries with method signatures
3. Create test harness for current behavior
4. Plan incremental refactoring steps
5. Validate against V12 DNA principles

---
**Scope Status**: APPROVED
**Boundary Validation**: See 01-scope-boundary.md
**Ready for Phase 2**: YES
