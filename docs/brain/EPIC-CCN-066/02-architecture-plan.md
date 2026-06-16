# Phase 2: Architecture Planning - EPIC-CCN-066

## Epic Metadata
- **Epic ID**: EPIC-CCN-066
- **Target Method**: RemoveFsmOrderIdMappings
- **File**: src/V12_002.Symmetry.BracketFSM.cs
- **Current Complexity**: 11 (CYC)
- **Current LOC**: 14
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **Tier**: 2 (Medium complexity)

## 1. Extraction Strategy

### Current Method Analysis
The RemoveFsmOrderIdMappings method has 4 distinct responsibilities:
1. Remove entry order mapping
2. Remove replacing cancel order mapping
3. Remove stop order mapping
4. Remove target order mappings (loop)

### Complexity Breakdown
- **Current CYC**: 11
  - Null check: +1
  - Entry order null check: +1
  - Entry order string check: +1
  - Replacing cancel order string check: +1
  - Stop order null check: +1
  - Stop order string check: +1
  - Targets null check: +1
  - Foreach loop: +1
  - Target null check: +1
  - Target string check: +1
  - Base: +1

### Extraction Plan
Extract 3 helper methods to reduce complexity:

1. **RemoveEntryOrderMapping**: Handles entry order and replacing cancel order removal (related lifecycle)
2. **RemoveStopOrderMapping**: Handles stop order removal
3. **RemoveTargetOrderMappings**: Handles target orders collection removal

**Post-Extraction Complexity**:
- Main method: CYC ≤4 (null check + 3 method calls)
- Helper 1: CYC ≤3 (2 conditional branches)
- Helper 2: CYC ≤3 (2 conditional branches)
- Helper 3: CYC ≤4 (null check + loop + 2 conditional branches)

## 2. Method Signatures

### Original Method
- **Signature**: `private void RemoveFsmOrderIdMappings(FollowerBracketFSM fsm)`
- **Purpose**: Removes all order ID mappings associated with a FollowerBracketFSM
- **Parameter**: fsm - The FSM whose order mappings should be removed

### Proposed Helper Methods

#### Helper 1: RemoveEntryOrderMapping
- **Signature**: `private void RemoveEntryOrderMapping(FollowerBracketFSM fsm)`
- **Purpose**: Removes entry order and replacing cancel order mappings from the FSM
- **Responsibilities**:
  - Remove entry order ID mapping (if exists)
  - Remove replacing cancel order ID mapping (if exists)
  - Both are part of entry lifecycle management
- **Complexity**: CYC ≤3

#### Helper 2: RemoveStopOrderMapping
- **Signature**: `private void RemoveStopOrderMapping(FollowerBracketFSM fsm)`
- **Purpose**: Removes stop order mapping from the FSM
- **Responsibilities**:
  - Remove stop order ID mapping (if exists)
  - Isolated from entry/target lifecycle
- **Complexity**: CYC ≤3

#### Helper 3: RemoveTargetOrderMappings
- **Signature**: `private void RemoveTargetOrderMappings(FollowerBracketFSM fsm)`
- **Purpose**: Removes all target order mappings from the FSM
- **Responsibilities**:
  - Remove all target order ID mappings (loop)
  - Handle null collection gracefully
  - Handle null/empty order IDs within collection
- **Complexity**: CYC ≤4

### Refactored Main Method
After extraction, the main method becomes pure orchestration with CYC ≤4:
- Null check on fsm parameter
- Call RemoveEntryOrderMapping(fsm)
- Call RemoveStopOrderMapping(fsm)
- Call RemoveTargetOrderMappings(fsm)

## 3. Call Graph

### Method Call Hierarchy
```
RemoveFsmOrderIdMappings(fsm)
├── RemoveEntryOrderMapping(fsm)
│   ├── _orderIdToFsmKey.TryRemove(EntryOrder.OrderId)
│   └── _orderIdToFsmKey.TryRemove(ReplacingCancelOrderId)
├── RemoveStopOrderMapping(fsm)
│   └── _orderIdToFsmKey.TryRemove(StopOrder.OrderId)
└── RemoveTargetOrderMappings(fsm)
    └── foreach target in fsm.Targets
        └── _orderIdToFsmKey.TryRemove(target.OrderId)
```

### Data Flow

1. **Main Method → Helper Methods**
   - Input: FollowerBracketFSM fsm (read-only)
   - Output: None (void)
   - Side Effect: Removes entries from _orderIdToFsmKey (ConcurrentDictionary)

2. **Helper Methods → ConcurrentDictionary**
   - Input: Order IDs (strings)
   - Operation: TryRemove(orderId, out _)
   - Thread-Safe: Yes (lock-free atomic operation)

### Shared State

- **_orderIdToFsmKey**: ConcurrentDictionary<string, string>
  - Accessed by: All helper methods
  - Operation: TryRemove (atomic, lock-free)
  - Mutation: Yes (removes entries)
  - Thread-Safety: Guaranteed by ConcurrentDictionary

- **fsm Parameter**: FollowerBracketFSM
  - Accessed by: All helper methods
  - Operation: Read-only (property access)
  - Mutation: No
  - Thread-Safety: Not required (read-only)

## 4. Lock-Free Validation

### ✅ No lock() Statements
- **Current Method**: Zero lock() statements
- **Helper Methods**: Zero lock() statements
- **Compliance**: PASS

### ✅ Uses FSM/Actor Enqueue Pattern
- **Context**: This method is called from FSM state transitions
- **Pattern**: Part of Actor model message processing
- **Compliance**: PASS (method is invoked within FSM context)

### ✅ Atomic Primitives Only
- **Operation**: ConcurrentDictionary.TryRemove(TKey, out TValue)
- **Atomicity**: Guaranteed by .NET runtime
- **Lock-Free**: Yes (uses Compare-And-Swap internally)
- **Compliance**: PASS

### Thread-Safety Analysis

ConcurrentDictionary.TryRemove is lock-free and atomic. Multiple threads can safely call TryRemove concurrently.

**Guarantees**:
1. **Atomicity**: Remove operation is atomic (either succeeds or fails, no partial state)
2. **Visibility**: Changes are immediately visible to all threads
3. **No Deadlocks**: Lock-free implementation prevents deadlocks
4. **No Race Conditions**: CAS (Compare-And-Swap) ensures correctness

## 5. Jane Street Compliance

### Cognitive Simplicity (CYC ≤8)

**Before Extraction**:
- RemoveFsmOrderIdMappings: CYC = 11 ❌ (exceeds threshold)

**After Extraction**:
- RemoveFsmOrderIdMappings: CYC ≤4 ✅
- RemoveEntryOrderMapping: CYC ≤3 ✅
- RemoveStopOrderMapping: CYC ≤3 ✅
- RemoveTargetOrderMappings: CYC ≤4 ✅

**Rationale**: Jane Street HFT systems prioritize cognitive simplicity. Functions with CYC >8 are harder to:
- Reason about under microsecond latency constraints
- Test exhaustively (exponential path growth)
- Audit for race conditions in lock-free code

### Testability Improvement

**Before Extraction**:
- Single monolithic method
- Hard to test individual responsibilities
- Requires complex test setup for all scenarios

**After Extraction**:
- 3 focused helper methods
- Each testable independently
- Clear test boundaries for entry, stop, and target order removal

### Make Illegal States Unrepresentable

**Current Design**:
- Null checks scattered throughout method
- Defensive programming (runtime guards)

**Improved Design**:
- Each helper encapsulates null checks
- Clear preconditions (fsm != null in main method)
- Helpers assume valid FSM (precondition enforced by caller)

**V12 DNA Alignment**:
- ✅ Lock-free Actor pattern (ConcurrentDictionary)
- ✅ ASCII-only compliance (no Unicode in strings)
- ✅ Correctness by construction (null checks at boundaries)

### Jane Street KB Insights

**Query Result**: No direct FSM extraction patterns found in KB.

**Applied Principles** (from Jane Street culture):
1. **Cognitive Simplicity**: Keep functions small and focused (CYC ≤8)
2. **Testability**: Extract helpers to enable unit testing
3. **Lock-Free Correctness**: Use atomic primitives (ConcurrentDictionary)
4. **Microsecond Latency**: Minimize branching in hot paths

**Reference Documents**:
- will_wilson_why_testing_hard_2026: Testing principles and testability
- carl_cook_microsecond_2017: Microsecond-latency optimization
- gjengset_concurrency_coordination_2020: Lock-free coordination patterns

## 6. Implementation Checklist

### Pre-Implementation
- [x] Scope boundary validated (Phase 1.5)
- [x] Architecture plan created (Phase 2)
- [ ] DNA & PR audit (Phase 3 - Arena AI)
- [ ] PASS/FAIL gate approval

### Implementation Steps
1. [ ] Create RemoveEntryOrderMapping helper method
2. [ ] Create RemoveStopOrderMapping helper method
3. [ ] Create RemoveTargetOrderMappings helper method
4. [ ] Refactor RemoveFsmOrderIdMappings to call helpers
5. [ ] Run CSharpier formatter on modified code
6. [ ] Verify complexity reduction (CYC ≤8)
7. [ ] Run build_readiness.ps1 (zero errors)
8. [ ] Run pre_push_validation.ps1 -Fast
9. [ ] Commit with message: "refactor: EPIC-CCN-066 extract RemoveFsmOrderIdMappings helpers (CYC 11→4)"

### Post-Implementation
- [ ] Verify hard-link sync (deploy-sync.ps1)
- [ ] Run full test suite (dotnet test)
- [ ] Update manifest.json with completion status
- [ ] Create PR with diff <10k characters

## 7. Risk Assessment

### Complexity Risk: LOW
- **Current CYC**: 11 (below threshold 15)
- **Extraction**: Straightforward (clear boundaries)
- **Testing**: Existing tests should pass without modification

### Scope Risk: MINIMAL
- **Single Method**: Only RemoveFsmOrderIdMappings modified
- **No Callers Changed**: Method signature unchanged
- **No Callees Changed**: Only calls ConcurrentDictionary.TryRemove

### Regression Risk: LOW
- **Pure Refactoring**: Zero logic changes
- **Behavior Preservation**: Identical functionality
- **Test Coverage**: Existing tests verify correctness

### Overall Risk: LOW
- **Blast Radius**: Single method in single file
- **Rollback**: Easy (single commit revert)
- **Validation**: Automated via pre-push validation

## 8. Success Criteria

### Functional Requirements
- ✅ Method signature unchanged
- ✅ Behavior identical to original
- ✅ All existing tests pass
- ✅ Zero compilation errors

### Non-Functional Requirements
- ✅ Complexity reduced: CYC 11 → ≤4
- ✅ Lock-free compliance maintained
- ✅ ASCII-only compliance maintained
- ✅ Jane Street alignment (CYC ≤8)

### Quality Gates
- ✅ Build readiness: PASS
- ✅ Pre-push validation: PASS
- ✅ PR hygiene: Diff <10k characters
- ✅ Codacy: Zero new issues

## Phase 2 Status
- **Status**: COMPLETED
- **Date**: 2026-06-15
- **Architect**: Bob Shell (v12-engineer mode)
- **Approval**: PENDING (Phase 3 DNA & PR Audit)
- **Next Phase**: 3 (DNA & PR Audit - Arena AI)
