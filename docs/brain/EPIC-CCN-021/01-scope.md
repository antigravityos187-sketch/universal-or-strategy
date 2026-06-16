# Phase 1.0: Scope Definition - EPIC-CCN-021

## Extraction Scope (SINGLE METHOD ONLY)

### Target Method
- **Method Name**: ProcessOnOrderUpdate
- **File**: src/V12_002.Orders.Callbacks.cs
- **Current Complexity**: 19 (CYC)
- **Target Complexity**: <=8 (Jane Street strict standard)
- **Threshold Violation**: +4 over V12 threshold (15), +11 over Jane Street strict (8)

### Extraction Strategy

**Primary Goal**: Reduce ProcessOnOrderUpdate from CYC=19 to <=8 through surgical method extraction.

**Approach**: Break into 2-4 helper methods based on Single Responsibility Principle:

1. **ValidateOrderUpdate()** - CYC target: <=5
   - Order state validation logic
   - Pre-condition checks
   - Input sanitization

2. **ProcessOrderStateChange()** - CYC target: <=8
   - State transition handling
   - FSM/Actor pattern enforcement
   - Atomic state updates

3. **UpdatePositionFromOrder()** - CYC target: <=6
   - Position tracking updates
   - Risk management validation
   - Position consistency checks

4. **HandleOrderUpdateError()** - CYC target: <=5
   - Error handling paths
   - Logging/telemetry
   - Failure recovery

**Main Method Post-Extraction**: CYC target: <=8
- Orchestration only
- Delegate to extracted helpers
- Maintain callback contract

## Boundary Definition

### IN SCOPE
- **ProcessOnOrderUpdate method body ONLY**
- Method signature (if needed for clarity)
- Internal logic extraction
- Helper method creation within same class
- Complexity reduction to <=8

### OUT OF SCOPE
- **Callers**: NinjaTrader OnOrderUpdate() event handler
- **Callees**: Existing helper methods called by ProcessOnOrderUpdate
- **Other methods**: Any other method in V12_002.Orders.Callbacks.cs
- **Other files**: No changes to other files in Orders subsystem
- **Feature changes**: No behavior modifications
- **Bug fixes**: No fixing pre-existing issues
- **Performance optimization**: No optimization beyond extraction
- **Logging changes**: No logging infrastructure changes

### No Scope Creep Rule
**ONE EPIC = ONE CONCERN**: This epic ONLY extracts ProcessOnOrderUpdate. Any discovered issues in other methods must be tracked as separate epics.

## Success Criteria

### Functional Requirements
1. **Complexity Reduction**: ProcessOnOrderUpdate CYC reduced from 19 to <=8
2. **Extracted Methods**: All extracted methods have CYC <=8
3. **Behavior Preservation**: Zero behavior changes (pure refactoring)
4. **Test Coverage**: All existing tests pass without modification
5. **Lock-Free Pattern**: Actor/FSM pattern maintained (no lock() blocks)

### Non-Functional Requirements
1. **ASCII-Only**: No Unicode, emoji, or curly quotes
2. **Build Success**: Zero compilation errors
3. **Lint Clean**: Zero new Roslyn violations
4. **Format Compliance**: CSharpier formatting passes
5. **Hard-Link Sync**: deploy-sync.ps1 succeeds

### Verification Gates
1. **Pre-Push Validation**: All 13 checks pass (fast mode minimum)
2. **F5 Test**: NinjaTrader loads without errors
3. **Complexity Audit**: python3 scripts/complexity_audit.py shows CYC <=8
4. **Codacy Review**: No new complexity violations

## Risk Assessment

### Risk Level: LOW-MEDIUM
- **Complexity**: 19 is manageable (not extreme like 45+)
- **Criticality**: Order callbacks are core but well-tested
- **Blast Radius**: Contained within single method
- **Reversibility**: Easy to revert via git

### Mitigation Strategy
1. **Extract-Only**: No logic changes during extraction
2. **Test-First**: Verify existing tests before extraction
3. **Incremental**: Extract one helper at a time
4. **Checkpoint**: Commit after each successful extraction
5. **Verify**: Run pre-push validation after each commit

## Jane Street Alignment

### Cognitive Simplicity Principles
- **CYC <=8**: Functions should be simple enough to reason about under microsecond latency constraints
- **Single Responsibility**: Each extracted method does ONE thing
- **Testability**: Extracted methods are independently testable
- **Atomic Operations**: State transitions remain atomic (lock-free)

### HFT Performance Considerations
- **Zero Allocation**: Extraction must not introduce heap allocations
- **Inline Candidates**: Small helpers (<=5 lines) should be inline-able
- **Branch Prediction**: Reduce nested conditionals for better CPU pipelining
- **Cache Locality**: Keep hot-path code co-located

## Metadata
- **Epic**: EPIC-CCN-021
- **Phase**: 1.0 (Scope Definition)
- **Priority**: P4 (Complexity Reduction)
- **Estimated Effort**: 2-4 hours
- **Blocking Issues**: NONE
- **Dependencies**: Phase 0 (Hotspot Analysis) - COMPLETED

---
**Phase 1.0 Status**: COMPLETED
**Ready for Phase 1.5**: YES (Boundary Validation)
