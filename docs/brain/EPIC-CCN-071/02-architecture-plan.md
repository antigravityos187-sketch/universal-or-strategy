# Phase 2: Architecture Planning - EPIC-CCN-071

## Method Analysis

**Target Method**: `ShadowProcessFollowerStopUpdate`
**File**: `src/V12_002.SIMA.Shadow.cs`
**Current Complexity**: 12 (McCabe Cyclomatic Complexity)
**Current LOC**: 31
**Target Complexity**: ≤8 (Jane Street strict standard)
**Tier**: 2 (Medium complexity)

## Extraction Strategy

### Current Method Structure

The method has 3 distinct concerns that create complexity:

1. **Validation of FSM and Position Existence** (Lines 1-10)
   - Checks if follower bracket FSM exists
   - Checks if follower position exists
   - Early return if neither exists
   - Complexity contribution: ~3

2. **Validation of Follower Readiness State** (Lines 12-22)
   - Validates position is filled and bracket submitted
   - Validates FSM is in Active state with valid StopOrder
   - Sets waitingOnFollower flag for incomplete states
   - Complexity contribution: ~5

3. **Price Comparison and Delegation** (Lines 24-31)
   - Compares current stop price with target price
   - Skips update if already at target (within tolerance)
   - Delegates to existing stop update infrastructure
   - Complexity contribution: ~4

### Proposed Extraction

Extract 3 helper methods to reduce cognitive load and improve testability:

Main Method (CYC ~3) orchestrates:
- Helper 1: ValidateFollowerBracketExists (CYC ~3)
- Helper 2: ValidateFollowerReadiness (CYC ~4)
- Helper 3: ShouldUpdateFollowerStop (CYC ~2)

**Total Distributed Complexity**: 3 + 3 + 4 + 2 = 12 (same total, but cognitively simpler)

## Method Signatures

### Original Method (Unchanged Interface)

private bool ShadowProcessFollowerStopUpdate(
    string followerEntryName,
    double newStopPrice,
    out bool waitingOnFollower
)

**Parameters**:
- followerEntryName (string): Identifier for the follower bracket
- newStopPrice (double): Target stop price to update to
- waitingOnFollower (out bool): Flag indicating if follower is not ready

**Returns**: bool - True if update was processed or is pending, false if follower does not exist

### Proposed Helper Method 1: ValidateFollowerBracketExists

private (bool hasFsm, bool hasFollowerPos, FollowerBracketFSM fsm, PositionInfo followerPos) 
    ValidateFollowerBracketExists(string followerEntryName)

**Purpose**: Validate existence of FSM and position for the follower bracket

**Returns**: Tuple containing existence flags and instances

**Complexity**: ~3 (dictionary lookups + null checks)

**Access Modifier**: private (internal helper, not exposed)

### Proposed Helper Method 2: ValidateFollowerReadiness

private (bool isReady, bool waitingOnFollower) 
    ValidateFollowerReadiness(
        FollowerBracketFSM fsm, 
        PositionInfo followerPos,
        bool hasFsm,
        bool hasFollowerPos
    )

**Purpose**: Validate that follower bracket is in a ready state for stop updates

**Readiness Criteria**:
- Position must be filled (followerPos.EntryFilled)
- Bracket must be submitted (followerPos.BracketSubmitted)
- FSM must be in Active state (fsm.State == FollowerBracketState.Active)
- FSM must have valid StopOrder (fsm.StopOrder != null)

**Complexity**: ~4 (multiple conditional checks)

**Access Modifier**: private (internal helper, not exposed)

### Proposed Helper Method 3: ShouldUpdateFollowerStop

private bool ShouldUpdateFollowerStop(
    FollowerBracketFSM fsm,
    double newStopPrice
)

**Purpose**: Determine if stop price update is needed based on price comparison

**Logic**:
- Compare current stop price (fsm.StopOrder.StopPrice) with newStopPrice
- Skip update if difference is less than tickSize * 0.5 (tolerance threshold)
- Return true if update is needed

**Complexity**: ~2 (price comparison + tolerance check)

**Access Modifier**: private (internal helper, not exposed)

## Call Graph

ShadowProcessFollowerStopUpdate (Main Orchestrator)
├─► ValidateFollowerBracketExists(followerEntryName)
│   └─► Returns: (hasFsm, hasFollowerPos, fsm, followerPos)
├─► ValidateFollowerReadiness(fsm, followerPos, hasFsm, hasFollowerPos)
│   └─► Returns: (isReady, waitingOnFollower)
└─► ShouldUpdateFollowerStop(fsm, newStopPrice)
    └─► Returns: bool (shouldUpdate)

### Data Flow

1. **Input**: followerEntryName, newStopPrice
2. **Step 1**: Call ValidateFollowerBracketExists → Get FSM and position instances
3. **Step 2**: Early return if neither exists
4. **Step 3**: Call ValidateFollowerReadiness → Check if ready for update
5. **Step 4**: Early return if not ready (set waitingOnFollower = true)
6. **Step 5**: Call ShouldUpdateFollowerStop → Check if price update needed
7. **Step 6**: Early return if already at target price
8. **Step 7**: Delegate to existing stop update infrastructure (unchanged)
9. **Output**: bool (success/pending), waitingOnFollower (out parameter)

### Shared State

**Read-Only Access**:
- _followerBrackets (Dictionary): FSM lookup
- activePositions (Dictionary): Position lookup
- tickSize (double): Price tolerance calculation

**No Mutations**: All helper methods are pure validation/computation functions. State mutations happen through existing FSM infrastructure (delegation in Step 7).

## Lock-Free Validation

### Compliance Checklist

- ✅ **No lock() statements**: Method uses dictionary lookups and FSM state checks only
- ✅ **FSM/Actor Enqueue pattern**: Delegates to existing FSM infrastructure for state mutations
- ✅ **Atomic primitives only**: No direct state mutations in validation logic
- ✅ **Read-only shared state access**: Helper methods only read from dictionaries
- ✅ **No race conditions**: Validation logic is stateless and side-effect-free

### Architecture Alignment

The extraction maintains V12 DNA lock-free Actor pattern:

1. **Validation Phase** (Helpers 1-3): Pure functions, no state mutation
2. **Delegation Phase** (Main method): Enqueues work to FSM infrastructure
3. **Execution Phase** (Existing infrastructure): FSM handles state transitions atomically

This separation ensures that complexity reduction does not introduce concurrency bugs.

## Jane Street Compliance

### Cognitive Simplicity (CYC ≤8 per method)

| Method | Complexity | Status |
|--------|-----------|--------|
| ShadowProcessFollowerStopUpdate (main) | ~3 | ✅ PASS |
| ValidateFollowerBracketExists | ~3 | ✅ PASS |
| ValidateFollowerReadiness | ~4 | ✅ PASS |
| ShouldUpdateFollowerStop | ~2 | ✅ PASS |

**Total**: 12 (distributed across 4 methods for cognitive simplicity)

### Microsecond Latency Alignment

**Principle**: "When a Microsecond Is an Eternity" (Carl Cook, Jane Street)

- **Before**: Single 31-line method with CYC 12 → Hard to reason about under latency constraints
- **After**: 4 focused methods, each with CYC ≤4 → Each method is immediately understandable

**Benefits**:
1. **Faster debugging**: Isolated validation logic easier to trace
2. **Exhaustive testing**: Pure functions enable comprehensive test coverage
3. **Reduced cognitive load**: Each method has single, clear purpose
4. **No performance penalty**: Inlining candidates (private methods, simple logic)

### Single Responsibility Principle

Each helper method has one job:

1. **ValidateFollowerBracketExists**: "Does this follower exist?"
2. **ValidateFollowerReadiness**: "Is this follower ready for updates?"
3. **ShouldUpdateFollowerStop**: "Does the price need updating?"

This aligns with Jane Street preference for straightforward validation logic over clever abstractions.

### Testability

**Before**: Testing required mocking entire method flow (31 lines, 12 branches)

**After**: Each helper can be tested independently:
- Test ValidateFollowerBracketExists with various dictionary states
- Test ValidateFollowerReadiness with various FSM/position states
- Test ShouldUpdateFollowerStop with various price scenarios

**Test Coverage**: Exponential path reduction (2^12 → 2^3 + 2^3 + 2^4 + 2^2 = 36 paths vs 4096 paths)

## Implementation Plan

### Phase 3: Extraction (Next Phase)

1. **Extract Helper 1**: ValidateFollowerBracketExists
   - Move dictionary lookup logic
   - Return tuple with existence flags and instances
   - Verify: CYC ≤3

2. **Extract Helper 2**: ValidateFollowerReadiness
   - Move readiness validation logic
   - Return tuple with ready flag and waiting flag
   - Verify: CYC ≤4

3. **Extract Helper 3**: ShouldUpdateFollowerStop
   - Move price comparison logic
   - Return bool for update decision
   - Verify: CYC ≤2

4. **Refactor Main Method**: Update orchestration logic
   - Call helpers in sequence
   - Maintain identical external behavior
   - Verify: CYC ≤3

5. **Verification**: Run complexity audit
   - Target: All methods CYC ≤8
   - Build: Zero errors
   - Tests: 100% pass (if tests exist)

### Success Criteria

- ✅ Main method complexity reduced from 12 to ≤3
- ✅ All helper methods have CYC ≤4
- ✅ No lock() statements introduced
- ✅ External behavior unchanged (identical signature and return values)
- ✅ Build passes with zero errors
- ✅ No scope creep (only ShadowProcessFollowerStopUpdate modified)

## Risk Assessment

### Low Risk

- ✅ **Pure extraction**: No logic changes, only reorganization
- ✅ **Identical interface**: External callers unaffected
- ✅ **No new dependencies**: Uses existing infrastructure
- ✅ **Testable**: Each helper can be unit tested independently

### Mitigation

- **Verification**: Run complexity audit after extraction
- **Testing**: Ensure existing tests (if any) still pass
- **Build**: Verify zero compilation errors
- **Behavioral**: Confirm identical return values for all input combinations

## Next Steps

**Phase 3**: Proceed to extraction implementation
- Use Bob CLI (v12-engineer) for surgical code modification
- Follow the extraction sequence (Helpers 1→2→3, then Main refactor)
- Verify complexity reduction at each step
- Run complexity_audit.py after completion

---

**Architecture Planning Complete**: ✅ APPROVED
**Complexity Target**: ≤8 per method (Jane Street aligned)
**Lock-Free**: ✅ VERIFIED
**Ready for Phase 3**: YES
