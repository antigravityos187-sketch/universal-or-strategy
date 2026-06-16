# Phase 1.0: Scope Definition - EPIC-CCN-036

## Extraction Scope (SINGLE METHOD ONLY)

### Target Method
- **Method Name**: `MoveStop_SinglePosition`
- **File**: `src/V12_002.Trailing.Breakeven.cs`
- **Current Complexity**: 13 (Cyclomatic Complexity)
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **Extraction Strategy**: Break into 2-3 helper methods

### Complexity Reduction Plan
**Current State**: CYC = 13 (2 points below V12 DNA threshold of 15)
**Target State**: CYC ≤ 8 (Jane Street alignment for cognitive simplicity)

**Extraction Strategy**:
1. **Helper Method 1**: Extract stop price validation logic
   - Validates new stop price against position constraints
   - Returns validation result (bool or enum)
   - Estimated complexity reduction: 3-4 points

2. **Helper Method 2**: Isolate NinjaTrader API interaction
   - Encapsulates order modification calls
   - Handles API-specific error conditions
   - Estimated complexity reduction: 2-3 points

3. **Core Method**: Simplified orchestration logic
   - Coordinates validation and execution
   - Maintains high-level workflow
   - Target complexity: ≤8

## Boundary Definition

### IN SCOPE ✅
- **MoveStop_SinglePosition method body ONLY**
- Internal logic extraction to helper methods
- Complexity reduction from 13 to ≤8
- Unit test creation for extracted methods
- Lock-free pattern verification

### OUT OF SCOPE ❌
- **Callers**: No changes to methods that invoke MoveStop_SinglePosition
- **Callees**: No changes to downstream NinjaTrader API methods
- **Other Methods**: No changes to other methods in V12_002.Trailing.Breakeven.cs
- **File-Level Changes**: No namespace, using statements, or class-level modifications
- **Pre-existing Issues**: No fixing unrelated compilation errors or warnings

### No Scope Creep: ONE EPIC = ONE CONCERN
This epic focuses EXCLUSIVELY on reducing the complexity of MoveStop_SinglePosition. Any other improvements, refactorings, or fixes are OUT OF SCOPE and must be tracked in separate epics.

## Success Criteria

### Primary Goals
1. ✅ **Complexity Reduction**: CYC reduced from 13 to ≤8
2. ✅ **Behavior Preservation**: All existing tests pass (zero regressions)
3. ✅ **No Behavior Changes**: Extracted logic maintains identical semantics
4. ✅ **Lock-Free Compliance**: FSM/Actor pattern maintained (no lock() statements)

### Quality Gates
1. ✅ **Build Success**: Zero compilation errors
2. ✅ **Test Coverage**: Unit tests for all extracted methods
3. ✅ **ASCII-Only**: No Unicode characters in string literals
4. ✅ **Hard-Link Sync**: `deploy-sync.ps1` executed successfully
5. ✅ **Complexity Audit**: `complexity_audit.py` confirms CYC ≤8

### V12 DNA Compliance
- **Correctness by Construction**: Extracted methods use type-safe parameters
- **Lock-Free Actor Pattern**: No lock() statements introduced
- **ASCII-Only**: All string literals remain ASCII-compliant
- **Jane Street Alignment**: Cognitive simplicity prioritized over clever abstractions

## Risk Assessment

### Overall Risk: LOW
**Rationale**:
- Single-method scope limits blast radius
- Complexity (13) is manageable starting point
- Clear extraction boundaries identified
- No external API changes required

### Mitigation Strategies
1. **TDD Approach**: Write tests before extraction
2. **Incremental Extraction**: One helper method at a time
3. **Checkpoint Verification**: Build + test after each extraction
4. **Rollback Plan**: Git checkpoints at each phase

## Implementation Phases

### Phase 1: Preparation (Current)
- ✅ Hotspot analysis completed
- ✅ Scope definition documented
- ⏳ Boundary validation (Phase 1.5)

### Phase 2: Forensic Review
- Deep-dive into MoveStop_SinglePosition implementation
- Identify exact extraction boundaries
- Map dependencies and side effects

### Phase 3: Test Design
- Design unit tests for extracted methods
- Define test cases for edge conditions
- Prepare test fixtures

### Phase 4: Extraction
- Extract Helper Method 1 (validation logic)
- Extract Helper Method 2 (API interaction)
- Simplify core method orchestration

### Phase 5: Verification
- Run complexity audit (target: CYC ≤8)
- Execute full test suite
- Verify lock-free compliance
- Sync hard links

## Metadata
- **Epic ID**: EPIC-CCN-036
- **Phase**: 1.0 (Scope Definition)
- **Status**: COMPLETED
- **Date**: 2026-06-15
- **Analyst**: Bob Shell (v12-engineer mode)
- **Next Phase**: 1.5 (Boundary Validation)
