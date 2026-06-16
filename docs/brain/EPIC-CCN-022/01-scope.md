# Phase 1.0: Scope Definition - EPIC-CCN-022

## Extraction Scope (SINGLE METHOD ONLY)

### Target Method
- **Method Name**: PropagateMaster_IdentifyMove
- **File**: src/V12_002.Orders.Callbacks.Propagation.cs
- **Current Complexity**: 18 (CYC)
- **Target Complexity**: <=8 (Jane Street strict standard)
- **Overage**: +10 points (125% over target)

### Extraction Strategy

**Approach**: Break into 2-3 focused helper methods

**Recommended Extractions** (from Phase 0 analysis):
1. State Validation Logic -> ValidateOrderStatesForPropagation()
   - Target complexity: 3-5
   - Estimated reduction: -4 complexity points
   
2. Propagation Decision Logic -> DeterminePropagationAction()
   - Target complexity: 4-6
   - Estimated reduction: -5 complexity points
   
3. Error Handling Logic -> HandlePropagationError()
   - Target complexity: 2-3
   - Estimated reduction: -3 complexity points

**Remaining Core Logic**: Simplified orchestration method
- Target complexity: 6-8
- Final complexity: <=8 (well below V12 threshold of 15)

### Expected Outcome
- **Original Complexity**: 18
- **Target Complexity**: 6-8
- **Reduction**: 55-66%
- **New Methods**: 3 focused, testable methods
- **Testability**: Exponential paths (2^18) to Linear test growth

## Boundary Definition

### IN SCOPE
- ONLY the method body of PropagateMaster_IdentifyMove
- Internal logic extraction to helper methods
- Complexity reduction from 18 to <=8
- Maintaining existing behavior (zero functional changes)
- Lock-free Actor/FSM pattern preservation

### OUT OF SCOPE
- Callers: Order callback handlers, propagation event processors
- Callees: Order state validators, propagation queue operations
- Other methods in V12_002.Orders.Callbacks.Propagation.cs
- Signature changes: Method parameters remain unchanged
- State machine modifications: No changes to order state machine
- Pre-existing bugs: No fixing unrelated compilation errors
- Performance optimizations: Focus is complexity reduction only

### No Scope Creep
- ONE EPIC = ONE CONCERN: Single-method extraction only
- No "while we are here" improvements: Resist temptation to fix adjacent code
- No bundling: Each complexity hotspot gets its own EPIC

## Success Criteria

### Primary Goals
1. Complexity Reduction: CYC reduced from 18 to <=8
2. All Tests Pass: Zero test failures or regressions
3. No Behavior Changes: Identical output for all inputs
4. Lock-Free Pattern: Actor/FSM Enqueue model maintained

### Secondary Goals
5. Testability: Each extracted method independently testable
6. Cognitive Simplicity: Jane Street alignment (simple, verifiable logic)
7. ASCII-Only: No Unicode, emoji, or curly quotes
8. Build Success: Zero compilation errors

### Verification Steps
1. Run complexity_audit.py to verify CYC <=8
2. Run dotnet test for 100% pass rate
3. Run build_readiness.ps1 for zero errors
4. Run grep to verify zero lock() matches in modified file
5. Manual F5 test in NinjaTrader to verify order propagation works

## Risk Assessment

### Overall Risk: MEDIUM-HIGH

**Risk Factors**:
1. Complexity Risk: HIGH - 18 cyclomatic paths to preserve
2. Coupling Risk: MEDIUM - Tightly coupled to order state machine
3. Testing Risk: MEDIUM - Exponential path growth (2^18 = 262,144 theoretical paths)
4. Maintenance Risk: HIGH (current state) - High cognitive load

### Mitigation Strategy
1. Incremental Extraction: One helper method at a time
2. Test-First: Write tests for extracted methods before extraction
3. Behavior Preservation: Use characterization tests
4. Checkpoint Frequently: Bob CLI auto-checkpointing enabled
5. Peer Review: Arena AI adversarial audit before merge

## Jane Street Alignment

### Cognitive Simplicity
- Current: POOR (complexity 18)
- Target: GOOD (complexity <=8)
- Strategy: Extract to achieve Jane Street standards

### Microsecond Latency Impact
- Current: Moderate (complex branching increases instruction cache misses)
- Target: Low (simplified hot path, predictable branches)
- Strategy: Extract cold paths, optimize hot path

### Testability
- Current: POOR (exponential paths, difficult to reason about)
- Target: GOOD (linear test growth, isolated unit tests)
- Strategy: Each extracted method gets focused unit tests

## Metadata
- Epic: EPIC-CCN-022
- Phase: 1.0 (Scope Definition)
- Date: 2026-06-15
- V12 Protocol: V12.23
- Threshold: CYC <=15 (V12 standard), CYC <=8 (Jane Street strict)
- Analyzer: Bob CLI (v12-engineer mode)
