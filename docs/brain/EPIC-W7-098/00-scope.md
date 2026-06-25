# Phase 1: Scope Definition - EPIC-W7-098

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.00
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T19:37:22Z

## Epic Metadata
- **Epic ID**: EPIC-W7-098
- **Target Method**: ProcessFlattenWorkItem_CancelOrders
- **File**: src/V12_002.SIMA.Flatten.cs
- **Line**: 191
- **Current CYC**: 17
- **Target CYC**: ≤8 per method

## Scope Boundary Definition

### IN SCOPE ✅

#### Primary Target
- **Method**: `ProcessFlattenWorkItem_CancelOrders` (lines 191-239)
  - **Current Metrics**: CYC 17, Nesting 5, 48 LOC
  - **Extraction Goal**: Reduce to CYC ≤8, Nesting ≤3

#### Extraction Candidates (Within Method Body)
1. **Order Cancellation Logic** (nested conditionals for order state validation)
   - Lines: ~195-215
   - Complexity: Nested if/else blocks checking order states
   - Target: Extract to `ValidateAndCancelOrder()`

2. **Logging and Error Handling** (repeated logging patterns)
   - Lines: ~216-230
   - Complexity: Multiple LogBuffer.Format calls with conditionals
   - Target: Extract to `LogCancellationResult()`

3. **Work Item State Management** (FSM state transitions)
   - Lines: ~231-239
   - Complexity: Conditional state updates based on cancellation results
   - Target: Extract to `UpdateWorkItemState()`

#### Affected Callers (Must Verify After Extraction)
1. `PumpFlattenOps` (src/V12_002.SIMA.Flatten.cs:124)
2. `PerformFallbackFlatten` (src/V12_002.SIMA.Flatten.cs:328)
3. `FlattenAllApexAccounts` (src/V12_002.SIMA.Flatten.cs:38)
4. `ChainNextFlattenOp` (src/V12_002.SIMA.Flatten.cs:376)
5. `ClosePositionsOnlyApexAccounts` (src/V12_002.SIMA.Flatten.cs:516)

### OUT OF SCOPE ❌

#### Explicitly Excluded
1. **Caller Methods** - No modifications to the 5 calling methods
   - Rationale: Zero blast radius means callers do not need changes
   - Verification: Signature of `ProcessFlattenWorkItem_CancelOrders` remains unchanged

2. **LogBuffer Infrastructure** - No changes to logging utilities
   - Files: src/V12_002.Perf.LogBuffer.cs
   - Rationale: Callees are stable infrastructure, not part of complexity problem

3. **Other Flatten Methods** - No changes to sibling methods in SIMA.Flatten.cs
   - Rationale: Single-method extraction, no scope creep

4. **FSM Core Logic** - No changes to SIMA_FSM state machine
   - Rationale: Only extracting order cancellation logic, not FSM itself

5. **Test Files** - No modifications to existing tests (new tests will be added)
   - Rationale: Additive testing only

## Extraction Strategy

### Approach: Surgical Extraction with Helper Methods
1. **Extract Order Validation** → `ValidateAndCancelOrder(Order order, SIMA_FSM fsm)`
   - Target CYC: ≤5
   - Responsibility: Validate order state and execute cancellation

2. **Extract Logging** → `LogCancellationResult(Order order, bool success, string reason)`
   - Target CYC: ≤3
   - Responsibility: Centralize cancellation logging

3. **Extract State Management** → `UpdateWorkItemState(FlattenWorkItem workItem, bool allCancelled)`
   - Target CYC: ≤4
   - Responsibility: Update work item state based on cancellation results

### Complexity Distribution
- **Original**: ProcessFlattenWorkItem_CancelOrders (CYC 17)
- **After Extraction**:
  - ProcessFlattenWorkItem_CancelOrders (CYC ≤8) - orchestration only
  - ValidateAndCancelOrder (CYC ≤5)
  - LogCancellationResult (CYC ≤3)
  - UpdateWorkItemState (CYC ≤4)
- **Total CYC**: ~20 (distributed across 4 methods, all ≤8)

## Risk Mitigation

### Zero Blast Radius Advantages
- ✅ No external dependencies to break
- ✅ All callers in same file (easy verification)
- ✅ No cross-file refactoring needed
- ✅ Rollback is trivial (single file)

### Testing Strategy
1. **Unit Tests**: Add tests for 3 extracted helper methods
2. **Integration Tests**: Verify 5 caller methods still work
3. **Regression Tests**: Run existing flatten workflow tests

## Success Criteria

### Phase 2 (Architecture Planning) Gates
- [ ] All extraction candidates identified with line ranges
- [ ] Helper method signatures defined
- [ ] Complexity distribution validated (all ≤8)
- [ ] No scope creep beyond ProcessFlattenWorkItem_CancelOrders

### Phase 5 (Ticket Execution) Gates
- [ ] ProcessFlattenWorkItem_CancelOrders reduced to CYC ≤8
- [ ] All extracted methods have CYC ≤8
- [ ] Nesting depth reduced from 5 to ≤3
- [ ] All 5 callers verified functional
- [ ] Unit tests added for extracted methods
- [ ] Build passes, deploy-sync.ps1 executed

## Jane Street Alignment
- **Principle**: "Make illegal states unrepresentable"
- **Application**: Extract validation logic to prevent invalid order cancellations
- **Cognitive Load**: Reduce from CYC 17 (HIGH) to ≤8 (ACCEPTABLE)
- **Testability**: Enable exhaustive testing of extracted methods

## Next Phase
Proceed to Phase 1.5 (Scope Boundary Validation) to verify no scope creep and confirm extraction boundaries are correct.
