# Phase 1: Scope Definition - EPIC-W7-021

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.18
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T19:25:46Z

## Epic Objective
Reduce cyclomatic complexity of ProcessOnOrderUpdate from CYC 16 to ≤ 8 through state-based extraction while preserving NinjaTrader callback contract.

## Target Method
- **Method**: ProcessOnOrderUpdate
- **File**: src/V12_002.Orders.Callbacks.cs
- **Line**: 245
- **Current CYC**: 16
- **Target CYC**: ≤ 8
- **Blast Radius**: ZERO (no direct dependents)

## IN SCOPE

### Primary Extraction Targets
1. **Price Propagation Logic** (Lines ~250-270)
   - Extract to: ShouldPropagateAndApply()
   - Responsibility: Determine if price move should propagate and execute
   - Complexity Reduction: ~3 CYC points
   - Rationale: Self-contained logic with clear input/output

2. **Ghost Cleanup Logic** (Lines ~280-295)
   - Extract to: CleanupGhostReferences()
   - Responsibility: Remove ghost order references
   - Complexity Reduction: ~2 CYC points
   - Rationale: Distinct concern, already has helper method

3. **Order State Routing Simplification** (Lines ~300-350)
   - Refactor: Consolidate switch/if-else to pure routing
   - Responsibility: Dispatch to existing handlers only
   - Complexity Reduction: ~3 CYC points
   - Rationale: Reduce nested conditionals in dispatcher

### Scope Boundaries
- **Preserve**: 9-parameter callback signature (NinjaTrader contract)
- **Preserve**: Performance histogram tracking (_histProcessOnOrderUpdate)
- **Preserve**: Existing extracted handlers (HandleOrderState_*)
- **Modify**: Main method body only (routing logic)

### Success Criteria
1. ProcessOnOrderUpdate CYC reduced from 16 to ≤ 8
2. All extracted methods have CYC ≤ 8
3. Zero compilation errors
4. Zero test failures
5. Performance histogram still captures latency
6. NinjaTrader callback contract preserved

## OUT OF SCOPE

### Explicitly Excluded
1. **Existing Extracted Handlers** (DO NOT MODIFY)
   - HandleOrderState_Filled
   - HandleOrderState_Terminal
   - HandleOrderState_Working
   - Rationale: Already extracted, working correctly

2. **Downstream Callees** (DO NOT REFACTOR)
   - PropagateMasterPriceMove
   - ShouldPropagatePriceMove
   - RemoveGhostOrderRef
   - Rationale: Out of epic scope, separate complexity concerns

3. **Callback Signature** (DO NOT CHANGE)
   - 9 parameters required by NinjaTrader
   - Rationale: External contract, cannot modify

4. **Performance Instrumentation** (DO NOT REMOVE)
   - _histProcessOnOrderUpdate histogram
   - Rationale: Critical for latency monitoring

5. **Cross-File Refactoring** (DO NOT ATTEMPT)
   - Other methods in V12_002.Orders.Callbacks.cs
   - Rationale: Single-method epic, avoid scope creep

### Deferred to Future Epics
1. Parameter object refactoring (9 params → OrderUpdateContext)
2. Downstream callee complexity reduction (35 callees)
3. Test coverage expansion beyond extracted methods

## Extraction Strategy

### Phase 2 Architecture Plan
1. **Extract ShouldPropagateAndApply()**
   - Input: Order, price move parameters
   - Output: bool (propagation executed)
   - Logic: Combine ShouldPropagatePriceMove check + PropagateMasterPriceMove call

2. **Extract CleanupGhostReferences()**
   - Input: Order
   - Output: void
   - Logic: Wrap RemoveGhostOrderRef with null checks

3. **Simplify Main Dispatcher**
   - Remove nested conditionals
   - Pure switch on OrderState
   - Delegate to extracted handlers only

### Complexity Budget
- Current: 16 CYC
- Target: ≤ 8 CYC
- Extraction 1 (ShouldPropagateAndApply): -3 CYC
- Extraction 2 (CleanupGhostReferences): -2 CYC
- Routing simplification: -3 CYC
- **Projected Final**: 8 CYC ✅

## Risk Mitigation

### Low Risk Factors
- Zero blast radius (no external dependents)
- Callback isolation (no direct callers)
- Clear delegation pattern already exists

### Medium Risk Factors
- Active churn area (16 commits in 90 days)
- High callee count (35 downstream calls)
- Performance-critical path (order updates)

### Mitigation Strategy
1. Add unit tests for each extracted method BEFORE extraction
2. Preserve performance histogram to detect regressions
3. Test with NinjaTrader simulator after extraction
4. Keep extracted methods in same file (avoid cross-file complexity)

## Jane Street Alignment

### Principles Applied
1. **Cognitive Simplicity**: Target CYC ≤ 8 (strict standard)
2. **Single Responsibility**: Each extracted method has one concern
3. **Testability**: Extracted methods easier to unit test
4. **Isolation**: Preserve callback isolation (no new external coupling)

### Principles Preserved
1. **Performance**: Histogram tracking maintained
2. **Delegation**: Existing handler pattern preserved
3. **Contract Stability**: NinjaTrader callback signature unchanged

## Scope Validation

### Boundary Checks
✅ Single method target (ProcessOnOrderUpdate only)
✅ Single file modification (V12_002.Orders.Callbacks.cs)
✅ No cross-file refactoring
✅ No downstream callee modifications
✅ No callback signature changes
✅ Complexity budget achievable (16 → 8)

### Scope Creep Prevention
- If additional methods need refactoring: Create separate epic
- If downstream callees need work: Defer to future epic
- If parameter object needed: Defer to future epic
- If test coverage gaps found: Address in Phase 5.V only

## Conclusion

**Scope Status**: VALIDATED ✅

This epic has a **clear, achievable scope** with:
- 3 targeted extractions
- Zero blast radius risk
- Measurable success criteria (CYC 16 → 8)
- No scope creep vectors

**Recommendation**: Proceed to Phase 2 (Architecture Planning)
