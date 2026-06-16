# Phase 1.0: Scope Definition - EPIC-CCN-029

## Extraction Scope (SINGLE METHOD ONLY)

### Target Method
- **Method Name**: ShouldSkipFleet_RunHealthCheck
- **File**: src/V12_002.SIMA.Fleet.cs
- **Current Complexity**: 31 (Cyclomatic Complexity)
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **Violation Severity**: HIGH (2.07x over V12 threshold of 15)

### Extraction Strategy
**Approach**: Break into 2-3 helper methods

**Rationale**:
- Current CYC 31 indicates ~30 decision points
- Target: Each extracted method achieves CYC ≤8
- Maintain single responsibility principle
- Preserve lock-free Actor/FSM pattern

**Proposed Decomposition**:
1. **Helper Method 1**: Extract fleet validation logic (estimated CYC ≤8)
2. **Helper Method 2**: Extract health check conditions (estimated CYC ≤8)
3. **Helper Method 3**: Extract skip decision logic (estimated CYC ≤8)
4. **Main Method**: Orchestrate calls to helpers (target CYC ≤8)

## Boundary Definition

### IN SCOPE
- **ONLY**: ShouldSkipFleet_RunHealthCheck method body
- Method signature (if needed for clarity)
- Internal decision logic extraction
- Helper method creation within same class

### OUT OF SCOPE
- **Callers**: No modifications to methods that call ShouldSkipFleet_RunHealthCheck
- **Callees**: No modifications to methods called by ShouldSkipFleet_RunHealthCheck
- **Other Methods**: No changes to other methods in V12_002.SIMA.Fleet.cs
- **Class Structure**: No changes to class fields, properties, or constructors
- **External Dependencies**: No changes to imported namespaces or external classes

### No Scope Creep Mandate
**ONE EPIC = ONE CONCERN**
- This epic addresses ONLY the complexity of ShouldSkipFleet_RunHealthCheck
- No "while we're here" improvements
- No fixing pre-existing compilation errors in other methods
- No bundling multiple refactoring concerns

## Success Criteria

### Functional Requirements
- Complexity reduced from 31 to ≤8 for main method
- Each extracted helper method achieves CYC ≤8
- All existing tests pass (zero regression)
- No behavior changes (pure refactoring)
- Lock-free Actor/FSM pattern maintained

### V12 DNA Compliance
- Zero lock() blocks in refactored code
- FSM/Actor Enqueue pattern preserved
- ASCII-only compliance (no Unicode/emoji)
- Correctness by construction (illegal states unrepresentable)

### Quality Gates
- Build verification passes (deploy-sync.ps1)
- CSharpier formatting check passes
- Complexity audit passes (CYC ≤15 for all methods)
- Unit tests added for extracted methods (TDD)

### Documentation
- Implementation plan created (Phase 2)
- Extraction rationale documented
- Helper method signatures documented
- Test coverage plan documented

## Risk Assessment

### Refactoring Risk: MEDIUM
**Rationale**:
- Single-method scope limits blast radius
- High complexity increases likelihood of hidden dependencies
- Fleet health check logic may have subtle state dependencies

### Mitigation Strategy
1. **Comprehensive Testing**: Add unit tests before extraction
2. **Incremental Extraction**: Extract one helper at a time
3. **Behavior Verification**: Compare outputs before/after each extraction
4. **Checkpoint Restoration**: Use Bob CLI checkpointing for rollback safety

## Metadata
- **Epic ID**: EPIC-CCN-029
- **Phase**: 1.0 (Scope Definition)
- **Target Method**: ShouldSkipFleet_RunHealthCheck
- **Complexity**: 31 → ≤8
- **Analyst**: Bob Shell (v12-engineer mode)
- **Date**: 2026-06-15
- **V12 Protocol Version**: 12.23
