# Phase 1.0: Scope Definition - EPIC-CCN-061

## Extraction Scope (SINGLE METHOD ONLY)

**Target Method**: SubmitAndRegisterFleetOrders
- **File**: src/V12_002.SIMA.Fleet.cs
- **Current Complexity**: 11 (Cyclomatic Complexity)
- **Target Complexity**: <=8 (Jane Street strict standard)
- **Extraction Strategy**: Break into 2-3 helper methods

## Boundary Definition

### IN SCOPE
- **Method body only**: SubmitAndRegisterFleetOrders implementation
- **Complexity reduction**: Extract logical blocks into helper methods
- **Maintain behavior**: Zero functional changes
- **Preserve patterns**: Lock-free Actor/FSM pattern must remain intact

### OUT OF SCOPE
- Callers of SubmitAndRegisterFleetOrders
- Callees invoked by SubmitAndRegisterFleetOrders
- Other methods in V12_002.SIMA.Fleet.cs
- Pre-existing compilation errors in the file
- While we are here improvements
- Refactoring adjacent code

### No Scope Creep
**ONE EPIC = ONE CONCERN**
- This EPIC addresses ONLY the complexity of SubmitAndRegisterFleetOrders
- No bundling of multiple refactoring concerns
- No fixing unrelated issues discovered during extraction

## Success Criteria

### Functional Requirements
1. Complexity reduced from 11 to <=8
2. All existing tests pass (100% pass rate)
3. No behavior changes (bit-for-bit identical output)
4. Lock-free Actor/FSM pattern maintained

### Technical Requirements
1. Extract 2-3 helper methods with clear, single responsibilities
2. Helper methods follow V12 DNA principles:
   - ASCII-only compliance
   - No lock() statements
   - Atomic state transitions
   - Make illegal states unrepresentable
3. CSharpier formatting compliance
4. Zero new Codacy violations

### Verification Requirements
1. complexity_audit.py confirms CYC <=8 for target method
2. dotnet build succeeds with zero errors
3. dotnet test passes all tests
4. deploy-sync.ps1 completes successfully (hard-link integrity)

## Extraction Strategy

### Candidate Helper Methods (2-3 methods)
Based on typical fleet order submission patterns:

1. **ValidateFleetOrderParameters**
   - Extract parameter validation logic
   - Return validation result (bool or enum)
   - Reduce branching in main method

2. **PrepareFleetOrderContext**
   - Extract order context preparation
   - Build necessary state for submission
   - Isolate setup logic from submission logic

3. **ExecuteFleetOrderSubmission**
   - Extract core submission logic
   - Handle Actor/FSM enqueue operations
   - Maintain lock-free pattern

### Complexity Reduction Target
- **Current**: 11 branches
- **After extraction**: <=8 branches in main method
- **Helper methods**: Each <=5 branches (simple, testable)

## Risk Assessment

### Low Risk Factors
- Method is below threshold (11 < 15)
- No Jane Street violations detected
- Single-method scope (minimal blast radius)

### Mitigation Strategy
- Checkpoint before extraction
- Incremental extraction (one helper at a time)
- Test after each helper extraction
- Rollback capability via Bob CLI /restore

## Jane Street Alignment

### Cognitive Simplicity
- Target CYC <=8 aligns with Jane Street keep functions simple principle
- Helper methods enable easier reasoning under microsecond latency constraints
- Reduced branching = fewer edge cases to audit for race conditions

### Testing Philosophy
- Simpler methods = exhaustive test coverage becomes feasible
- Each helper method can be tested independently
- Exponential path growth avoided by keeping CYC low

## Approval Gate

**Status**: PENDING Phase 1.5 Boundary Validation
**Next Step**: Create 01-scope-boundary.md for V12.23 Protocol compliance
