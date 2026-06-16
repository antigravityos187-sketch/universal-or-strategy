# Phase 2: Architecture Planning - EPIC-CCN-064

## Target Method Analysis

**Method**: ResolveFsm_ByScan  
**File**: src/V12_002.Symmetry.BracketFSM.cs  
**Current Complexity**: 12 (CYC)  
**Current LOC**: 21  
**Target Complexity**: ≤8 (Jane Street strict standard)

### Current Method Signature

private FollowerBracketFSM ResolveFsm_ByScan(string accountAlias, string orderId)

**Parameters**:
- accountAlias (string): Account identifier for filtering
- orderId (string): Order ID to locate within FSM structures

**Return Type**: FollowerBracketFSM (nullable)

**Current Behavior**:
1. Early return for null/empty orderId
2. Iterates through _followerBrackets.Values
3. Filters by account name
4. Checks StopOrder match
5. Checks Targets array (5 slots) for match
6. Checks EntryOrder match
7. Caches orderId → EntryName mapping in _orderIdToFsmKey

## Complexity Analysis

### Cyclomatic Complexity Breakdown

| Decision Point | Complexity | Line |
|----------------|------------|------|
| if (string.IsNullOrEmpty(orderId)) | +1 | 211 |
| if (f.AccountName != accountAlias) | +1 | 216 |
| if (f.StopOrder != null && ...) | +2 | 219 |
| for (int i = 0; i < 5; i++) | +1 | 225 |
| if (f.Targets[i] != null && ...) | +2 | 227 |
| if (foundT) | +1 | 234 (dead code) |
| if (f.EntryOrder != null && ...) | +2 | 237 |

**Total CYC**: 10-12 (depending on tool calculation)

### Identified Issues

1. **Dead Code**: foundT flag and check at line 234 is unreachable (return inside loop)
2. **Nested Loops**: Foreach + for loop increases cognitive load
3. **Multiple Responsibilities**: Matching logic for 3 different order types
4. **Repeated Pattern**: Cache-and-return pattern duplicated 3 times

## Extraction Strategy

### Proposed Decomposition

Extract three specialized matching methods, each handling one order type:

1. **TryMatchStopOrder**: Check if orderId matches FSM's StopOrder
2. **TryMatchTargetOrder**: Check if orderId matches any Target in array
3. **TryMatchEntryOrder**: Check if orderId matches FSM's EntryOrder

### Benefits

- **Complexity Reduction**: Main method drops to CYC ≤5
- **Single Responsibility**: Each helper has one clear purpose
- **Testability**: Helpers can be unit tested independently
- **Readability**: Intent is explicit in method names
- **Maintainability**: Changes to matching logic isolated to specific helpers

## Proposed Helper Method Signatures

### Helper 1: TryMatchStopOrder

Checks if the given orderId matches the FSM's StopOrder.
If matched, caches the mapping and returns true.

private bool TryMatchStopOrder(FollowerBracketFSM fsm, string orderId)

**Responsibility**: 
- Check if fsm.StopOrder != null && fsm.StopOrder.OrderId == orderId
- If true: cache _orderIdToFsmKey[orderId] = fsm.EntryName
- Return boolean result

**Complexity**: CYC = 2 (null check + equality check)

### Helper 2: TryMatchTargetOrder

Checks if the given orderId matches any Target order in the FSM's Targets array.
If matched, caches the mapping and returns true.

private bool TryMatchTargetOrder(FollowerBracketFSM fsm, string orderId)

**Responsibility**:
- Iterate through fsm.Targets[0..4]
- Check if fsm.Targets[i] != null && fsm.Targets[i].OrderId == orderId
- If true: cache _orderIdToFsmKey[orderId] = fsm.EntryName and return true
- Return false if no match found

**Complexity**: CYC = 3 (loop + null check + equality check)

### Helper 3: TryMatchEntryOrder

Checks if the given orderId matches the FSM's EntryOrder.
If matched, caches the mapping and returns true.

private bool TryMatchEntryOrder(FollowerBracketFSM fsm, string orderId)

**Responsibility**:
- Check if fsm.EntryOrder != null && fsm.EntryOrder.OrderId == orderId
- If true: cache _orderIdToFsmKey[orderId] = fsm.EntryName
- Return boolean result

**Complexity**: CYC = 2 (null check + equality check)

## Refactored Main Method Structure

After extraction, the main method will have CYC = 5:
- if (string.IsNullOrEmpty(orderId)) = +1
- if (fsm.AccountName != accountAlias) = +1
- if (TryMatchStopOrder(...)) = +1
- if (TryMatchTargetOrder(...)) = +1
- if (TryMatchEntryOrder(...)) = +1

**Total**: 5 ✅ (meets Jane Street CYC ≤8 target)

## Call Graph

ResolveFsm_ByScan (CYC=5)
├── TryMatchStopOrder (CYC=2)
│   └── _orderIdToFsmKey[orderId] = fsm.EntryName (cache write)
├── TryMatchTargetOrder (CYC=3)
│   └── _orderIdToFsmKey[orderId] = fsm.EntryName (cache write)
└── TryMatchEntryOrder (CYC=2)
    └── _orderIdToFsmKey[orderId] = fsm.EntryName (cache write)

### Data Flow

1. **Input**: accountAlias, orderId → Main method
2. **Filtering**: Main method filters by account name
3. **Delegation**: Main method passes fsm + orderId to helpers
4. **Side Effect**: Helpers write to _orderIdToFsmKey dictionary (shared state)
5. **Return**: Helpers return boolean; main method returns FSM or null

### Shared State

- **_orderIdToFsmKey**: Dictionary<string, string> (orderId → EntryName mapping)
  - Written by all three helpers
  - No read conflicts (write-only in this method)
  - Thread-safety: Assumes single-threaded access or external synchronization

## Lock-Free Validation

### ✅ Compliance Checklist

- ✅ **No lock() statements**: Method uses no locks
- ✅ **FSM/Actor Pattern**: Method is called within FSM event handler context
- ✅ **Atomic Operations**: Dictionary writes are atomic at reference level
- ✅ **No Shared Mutable State Conflicts**: _orderIdToFsmKey writes are idempotent
- ✅ **No Race Conditions**: Single-threaded FSM execution model

### Thread-Safety Analysis

**Current Design**:
- Method is private and called from FSM event handlers
- FSM uses Actor/Enqueue pattern (single-threaded execution per FSM instance)
- Dictionary writes are safe within single-threaded context

**Post-Extraction**:
- Helpers maintain same thread-safety guarantees
- No new shared state introduced
- No new synchronization primitives required

## Jane Street Compliance

### Cognitive Simplicity (✅ PASS)

**Before**:
- CYC = 12 (exceeds Jane Street strict threshold of 8)
- Nested loops increase cognitive load
- Multiple responsibilities in single method

**After**:
- Main method: CYC = 5 ✅
- Helper 1: CYC = 2 ✅
- Helper 2: CYC = 3 ✅
- Helper 3: CYC = 2 ✅
- All methods ≤8 complexity

### Correctness by Construction (✅ PASS)

**Behavior Preservation**:
- Identical logic flow (no functional changes)
- Same early returns and null checks
- Same cache write behavior
- Same iteration order

**Type Safety**:
- All parameters strongly typed
- No new nullable references introduced
- Return types explicit (bool for helpers, FollowerBracketFSM for main)

### Testing Principles (Jane Street Intel: Will Wilson)

**Testability Improvements**:
1. **Isolated Logic**: Each helper can be tested independently
2. **Clear Contracts**: Boolean return + side effect (cache write)
3. **Reduced Mocking**: Helpers only need FSM instance + orderId
4. **Exhaustive Coverage**: Easier to test all branches in smaller methods

**Test Strategy** (Post-Extraction):
- Unit test each helper with mock FSM instances
- Test cache write side effects
- Test null handling in each helper
- Integration test main method with real FSM collection

## Verification Criteria

### Functional Correctness

1. **Behavior Equivalence**: Refactored method produces identical results for all inputs
2. **Cache Consistency**: _orderIdToFsmKey writes occur at same points
3. **Null Handling**: Early returns and null checks preserved
4. **Iteration Order**: Foreach loop behavior unchanged

### Complexity Targets

1. **Main Method**: CYC ≤ 5 ✅
2. **Helper 1**: CYC ≤ 3 ✅
3. **Helper 2**: CYC ≤ 4 ✅
4. **Helper 3**: CYC ≤ 3 ✅
5. **Total Complexity**: Sum ≤ 15 (currently 12, target 12)

### Code Quality

1. **No Dead Code**: Remove unreachable foundT logic
2. **ASCII-Only**: No Unicode characters introduced
3. **Consistent Style**: Match existing code conventions
4. **Clear Naming**: Helper names describe intent

## Implementation Steps

### Step 1: Create TryMatchStopOrder Helper

- Extract lines 219-223 into new private method
- Add XML documentation
- Verify CYC ≤ 3

### Step 2: Create TryMatchTargetOrder Helper

- Extract lines 225-233 into new private method
- Remove dead code (foundT flag and check)
- Add XML documentation
- Verify CYC ≤ 4

### Step 3: Create TryMatchEntryOrder Helper

- Extract lines 237-240 into new private method
- Add XML documentation
- Verify CYC ≤ 3

### Step 4: Refactor Main Method

- Replace extracted logic with helper calls
- Maintain early return for null/empty orderId
- Maintain account name filtering
- Verify CYC ≤ 5

### Step 5: Verification

- Run dotnet build (zero errors)
- Run dotnet csharpier check src/ (zero issues)
- Run python3 scripts/complexity_audit.py (verify CYC targets)
- Run powershell -File .\scripts\pre_push_validation.ps1 -Fast

## Risk Assessment

### Low Risk Factors

- ✅ Single method scope (no caller/callee changes)
- ✅ Pure refactoring (no behavior changes)
- ✅ Private methods (no API surface changes)
- ✅ Well-defined boundaries (clear extraction points)

### Mitigation Strategies

1. **Regression Testing**: Verify existing tests still pass
2. **Manual Testing**: F5 in NinjaTrader after deployment
3. **Rollback Plan**: Git revert if issues detected
4. **Incremental Deployment**: Test in dev environment first

## Success Criteria

### Phase 2 Completion

- ✅ Architecture plan documented
- ✅ Helper method signatures defined
- ✅ Complexity targets validated (all ≤8)
- ✅ Call graph documented
- ✅ Lock-free compliance verified
- ✅ Jane Street alignment confirmed
- ✅ Implementation steps outlined

### Ready for Phase 3 (DNA & PR Audit)

- Plan ready for adversarial review
- Extraction strategy clear and unambiguous
- Risk assessment complete
- Verification criteria defined

## Appendix: Jane Street Knowledge Base Query Results

**Query**: "testing"  
**Document**: "Why Testing Is Hard and How to Fix It" (Will Wilson)

**Key Takeaways**:
- Smaller methods are easier to test exhaustively
- Clear contracts (input → output + side effects) improve testability
- Isolated logic reduces mocking complexity
- Boolean return types simplify assertion logic

**Application to EPIC-CCN-064**:
- Extracted helpers have clear contracts (FSM + orderId → bool + cache write)
- Each helper can be tested with 2-3 test cases (null, no match, match)
- Main method integration test covers orchestration logic
- Reduced complexity enables exhaustive branch coverage

---

**Phase 2 Status**: ✅ COMPLETE  
**Next Phase**: Phase 3 (DNA & PR Audit via Arena AI)  
**Approval Gate**: Awaiting adversarial review of extraction strategy
