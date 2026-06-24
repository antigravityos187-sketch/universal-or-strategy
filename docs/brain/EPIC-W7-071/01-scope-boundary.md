# Phase 1: Scope Definition - EPIC-W7-071

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Execution Time**: 2026-06-24T01:32:49Z
- **Input**: docs/brain/EPIC-W7-071/00-hotspots.md

## Target Method
- **Method**: ShadowProcessFollowerStopUpdate
- **File**: src/V12_002.SIMA.Shadow.cs
- **Line**: 246
- **Current CYC**: 13
- **Target CYC**: <=8

## Scope Boundary Definition

### IN SCOPE

#### Primary Extraction Target
**Method**: ShadowProcessFollowerStopUpdate (CYC 13 -> <=8)

#### Extraction Candidates (3-5 methods)

1. **Validation Logic Extraction**
   - Extract: Price validation and bracket validation logic
   - Target Method Name: ValidateFollowerStopUpdate
   - Estimated CYC Reduction: 2-3
   - Rationale: Isolate validation concerns

2. **State Management Extraction**
   - Extract: pendingStopReplacements handling logic
   - Target Method Name: ManagePendingStopReplacements
   - Estimated CYC Reduction: 3-4
   - Rationale: Separate state mutation from business logic

3. **Error Handling Extraction**
   - Extract: Exception handling and stale replacement handling
   - Target Method Name: HandleStopUpdateErrors
   - Estimated CYC Reduction: 2-3
   - Rationale: Centralize error recovery logic

4. **Order Operations Extraction**
   - Extract: Stop order creation and replacement logic
   - Target Method Name: ExecuteStopOrderUpdate
   - Estimated CYC Reduction: 2-3
   - Rationale: Isolate order execution concerns

#### Refactoring Constraints
- **Preserve**: All existing functionality and behavior
- **Maintain**: Lock-free Actor pattern (no lock() blocks)
- **Ensure**: ASCII-only compliance
- **Target**: Each extracted method CYC <=8
- **Preserve**: Call hierarchy (2 callers remain unchanged)

### OUT OF SCOPE

#### Caller Methods (No Changes)
1. **ShadowMoveFollowerStops** (line 297)
   - Rationale: Caller interface remains stable
   - Action: No modifications required

2. **PropagateAndCacheStopPrice** (line 138)
   - Rationale: Caller interface remains stable
   - Action: No modifications required

#### Callee Methods (No Changes)
- All 28 callee methods remain unchanged
- Rationale: Internal dependencies preserved
- Examples: UpdateStopOrder, ValidateStopPrice, InitiateStopReplacement, CreateDirectStopOrder, HandleUpdateException

#### External Files (No Changes)
- Zero external dependencies identified
- No other files require modification
- Blast radius: 0 files

#### Infrastructure (No Changes)
- No FSM state changes
- No Actor model changes
- No logging pattern changes
- No error handling pattern changes

### Scope Validation

#### Complexity Reduction Path
Current: ShadowProcessFollowerStopUpdate (CYC 13)
- Extract: ValidateFollowerStopUpdate (CYC 2-3)
- Extract: ManagePendingStopReplacements (CYC 3-4)
- Extract: HandleStopUpdateErrors (CYC 2-3)
- Extract: ExecuteStopOrderUpdate (CYC 2-3)
Result: Main method (CYC <=8) + 4 helpers (each CYC <=8)

#### Risk Mitigation
- **Low Blast Radius**: 0 external dependencies
- **Stable Callers**: 2 entry points unchanged
- **Preserved Callees**: 28 internal dependencies maintained
- **Isolated Changes**: Single file modification (V12_002.SIMA.Shadow.cs)

## Success Criteria

### Phase 1 Completion
- Hotspot analysis reviewed
- IN SCOPE defined (4 extraction candidates)
- OUT OF SCOPE defined (callers, callees, external files)
- Scope boundary validated
- Complexity reduction path documented

### Phase 2 Prerequisites
- Target CYC: <=8 for all methods
- Extraction count: 3-5 methods
- File scope: src/V12_002.SIMA.Shadow.cs only
- Caller stability: Preserved
- Callee stability: Preserved

## Conclusion

**EPIC-W7-071 Scope Definition: APPROVED**

The scope is well-defined with:
- Clear extraction targets (4 methods)
- Manageable complexity reduction (CYC 13 -> <=8)
- Zero external impact (blast radius 0)
- Stable interfaces (2 callers unchanged)
- Preserved dependencies (28 callees unchanged)

**Next Phase**: Proceed to Phase 2 (Architecture Planning) to design extraction strategy and ticket breakdown.
