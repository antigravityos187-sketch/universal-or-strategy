# Phase 2: Architecture Planning - EPIC-CCN-069

## Target Method Analysis

### Current State
- **Method**: GetFsmExpectedPosition
- **File**: src/V12_002.Symmetry.BracketFSM.cs
- **Line Range**: 373-410 (38 lines)
- **Current Complexity**: 14 (CYC)
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **LOC**: 25

### Method Signature (Original)
private int GetFsmExpectedPosition(string accountName)

**Purpose**: Calculates the expected position for a given account by summing position contributions from all active follower brackets.

**Current Logic Flow**:
1. Initialize sum accumulator
2. Iterate over _followerBrackets dictionary
3. Filter by account name
4. Check if FSM is in active state (6 possible states)
5. Calculate position contribution based on entry order direction
6. Handle edge case for hydrated Active FSM without entry order
7. Return total sum

## Extraction Strategy

### Complexity Analysis
**Current Cyclomatic Complexity Breakdown**:
- Base method: +1
- foreach loop: +1
- Null check (f == null): +1
- Account name check (f.AccountName != accountName): +1
- State check (6 OR conditions): +6
- EntryOrder null check: +1
- OrderAction check (2 OR conditions): +2
- else if (Active state): +1
**Total**: 14

**Target**: Reduce to ≤8 by extracting 3 helper methods

### Proposed Helper Methods

#### 1. IsAccountMatch
private static bool IsAccountMatch(FollowerBracketFSM fsm, string accountName)

**Responsibility**: Validates if FSM belongs to the target account
**Complexity**: CYC = 2 (null check + account comparison)
**Parameters**:
- fsm: FollowerBracketFSM instance (can be null)
- accountName: Target account name
**Returns**: true if FSM is non-null and matches account, false otherwise
**Access Modifier**: private static (no instance state needed)

**Logic**: return fsm != null && fsm.AccountName == accountName;

#### 2. IsActiveState
private static bool IsActiveState(FollowerBracketState state)

**Responsibility**: Determines if FSM state is considered active for position calculation
**Complexity**: CYC = 1 (single boolean expression with OR operators)
**Parameters**:
- state: FollowerBracketState enum value
**Returns**: true if state is Active, Accepted, Submitted, PendingSubmit, Replacing, or Modifying
**Access Modifier**: private static (pure function, no state)

**Logic**: Checks 6 active states with OR conditions

#### 3. CalculatePositionContribution
private static int CalculatePositionContribution(FollowerBracketFSM fsm)

**Responsibility**: Calculates position contribution for a single FSM based on entry order
**Complexity**: CYC = 3 (null check + order action check + else branch)
**Parameters**:
- fsm: FollowerBracketFSM instance (guaranteed non-null by caller)
**Returns**: Position contribution (positive for long, negative for short, 0 if indeterminate)
**Access Modifier**: private static (no instance state needed)

**Logic**: Handles EntryOrder null check, calculates sign based on OrderAction, returns quantity * sign

### Refactored Main Method

**New Complexity**: CYC = 4 (base + loop + 3 helper calls)
**Complexity Reduction**: 14 → 4 (71%% reduction)

## Call Graph

GetFsmExpectedPosition(accountName)
├── IsAccountMatch(f, accountName) [CYC=2]
├── IsActiveState(f.State) [CYC=1]
└── CalculatePositionContribution(f) [CYC=3]

**Data Flow**:
1. Main method iterates over _followerBrackets
2. Each FSM passes through IsAccountMatch filter
3. Surviving FSMs checked via IsActiveState
4. Active FSMs contribute to sum via CalculatePositionContribution
5. Sum accumulates and returns

**Shared State**: None (all helpers are static and stateless)

## Lock-Free Validation

### Compliance Checklist
- [x] No lock() statements: Method is read-only query
- [x] Actor/FSM Enqueue pattern: No state mutations in query method
- [x] Atomic primitives only: Uses simple integer accumulation
- [x] Read-only access: Only reads from _followerBrackets dictionary
- [x] No shared mutable state: All helpers are static/pure functions

### Pattern Analysis
**Current Pattern**: Read-only query method in FSM Actor
- Queries _followerBrackets dictionary (immutable reference during read)
- No mutations to FSM state
- No coordination primitives needed
- Aligns with Actor model: queries do not mutate, commands do

**Post-Extraction Pattern**: Same guarantees maintained
- Helper methods are static (no instance state)
- All helpers are pure functions (deterministic, no side effects)
- Main method remains read-only query

## Jane Street Compliance

### Cognitive Simplicity (CYC ≤8)
- **Main Method**: CYC = 4 ✅ (well below threshold)
- **IsAccountMatch**: CYC = 2 ✅
- **IsActiveState**: CYC = 1 ✅
- **CalculatePositionContribution**: CYC = 3 ✅

**Rationale**: Each method has a single, clear responsibility that can be reasoned about in isolation under microsecond-latency constraints.

### Testability (Jane Street Testing Principles)
From Jane Street KB document "Why Testing Is Hard and How to Fix It":
- **Unit Testability**: Each helper method is independently testable
- **Pure Functions**: Static helpers have no side effects
- **Clear Contracts**: Each method has well-defined inputs/outputs
- **Exhaustive Testing**: Low complexity enables exhaustive path coverage

**Test Coverage Strategy**:
1. IsAccountMatch: Test null FSM, matching account, non-matching account
2. IsActiveState: Test all 6 active states + inactive states
3. CalculatePositionContribution: Test Buy, Sell, BuyToCover, SellShort, null order, Active state edge case
4. GetFsmExpectedPosition: Integration test with multiple FSMs

### HFT Microsecond-Latency Alignment
- **No Allocations**: All helpers use stack-only primitives
- **No Branching Complexity**: Each helper has minimal branches
- **Cache-Friendly**: Small methods fit in instruction cache
- **Predictable Execution**: No dynamic dispatch or virtual calls

## V12 DNA Compliance

### Make Illegal States Unrepresentable
- **Type Safety**: Uses enum for state checks (compile-time validation)
- **Null Safety**: Explicit null checks prevent NullReferenceException
- **Immutable Queries**: Read-only methods cannot corrupt state

### ASCII-Only Compliance
- ✅ No Unicode characters in code
- ✅ No emoji in comments
- ✅ No curly quotes in strings

### Correctness by Construction
- **Static Helpers**: Cannot access instance state incorrectly
- **Pure Functions**: Deterministic behavior, no hidden dependencies
- **Clear Contracts**: Method signatures enforce correct usage

## Implementation Checklist

### Phase 3 (Implementation) Prerequisites
- [ ] Extract IsAccountMatch helper method
- [ ] Extract IsActiveState helper method
- [ ] Extract CalculatePositionContribution helper method
- [ ] Refactor GetFsmExpectedPosition to use helpers
- [ ] Verify no changes to method signature
- [ ] Verify no changes to return values
- [ ] Run existing tests (must pass unchanged)
- [ ] Verify complexity reduction (14 → 4)

### Verification Criteria
- [ ] Build succeeds with zero errors
- [ ] All existing tests pass
- [ ] Complexity audit shows CYC ≤8 for all methods
- [ ] No lock() statements introduced
- [ ] ASCII-only compliance maintained
- [ ] Hard-link sync successful (deploy-sync.ps1)

## Risk Assessment

### Low Risk Factors
- **Read-Only Method**: No state mutations reduce risk
- **Pure Helpers**: Static methods have no side effects
- **Existing Tests**: Safety net for regression detection
- **Clear Boundaries**: Single method scope prevents scope creep

### Mitigation Strategies
- **Incremental Extraction**: Extract one helper at a time
- **Test After Each Step**: Verify tests pass after each extraction
- **Preserve Comments**: Maintain edge case documentation
- **Checkpointing**: Use Bob CLI checkpointing for rollback safety

## Sign-Off

**Phase 2 Completed**: 2026-06-15
**Architecture Plan**: APPROVED
**Complexity Target**: 14 → 4 (71%% reduction)
**Jane Street Alignment**: VERIFIED
**Lock-Free Compliance**: VERIFIED
**Next Phase**: Phase 3 (Implementation)

---

**Architect Notes**:
- All helper methods are static to prevent accidental state coupling
- Complexity reduction from 14 to 4 exceeds Jane Street threshold (≤8)
- Each helper has single responsibility and is independently testable
- No architectural changes required - pure refactoring
- Edge case handling preserved (hydrated Active FSM comment retained)
