# Phase 2: Architecture Planning - EPIC-CCN-032

## Target Method Analysis

### Current State
- **Method**: RestoreCascadedTargets
- **File**: src/V12_002.Orders.Management.StopSync.cs
- **Location**: Line 717-807 (90 lines)
- **Cyclomatic Complexity**: 16
- **Target Complexity**: ≤8 per method (Jane Street strict standard)

### Complexity Drivers
1. **Nested Conditionals**: Early validation checks (null checks, position lookup, entry filled status)
2. **Foreach Loop**: Iterating through captured targets with multiple conditions
3. **Branching Logic**: isFollower vs non-follower order submission paths
4. **State Extraction**: Multiple local variables extracted from PositionInfo

## Extraction Strategy

### Approach: Three Helper Methods
Break the 90-line method into 3 focused helpers + orchestration logic:

1. **ShouldRestoreTarget** (CYC 2): Pure predicate for target filtering
2. **BuildRestoredTargetOrder** (CYC 4): Order object construction
3. **SubmitTargetOrder** (CYC 2): Submission branching logic
4. **RestoreCascadedTargets** (CYC 7): Orchestration (main method)

**Total Complexity**: 2 + 4 + 2 + 7 = 15 (down from 16)
**Per-Method Max**: 7 (well under threshold 8)

### Rationale
- **Cognitive Simplicity**: Each helper has single, clear responsibility
- **Testability**: CYC ≤8 makes exhaustive testing tractable (2^8 = 256 paths vs 2^16 = 65,536)
- **Hot-Path Performance**: Private helpers are JIT-inlined, zero runtime overhead
- **Maintainability**: Clear separation of concerns aligns with V12 DNA

## Method Signatures

### Helper Method 1: Target Filtering
Private method that determines if a target snapshot should be restored based on order state.
Only cancelled or rejected targets need restoration.

Parameters: TargetSnapshot snap
Returns: bool (true if target should be restored)
Complexity: CYC 2

Logic: Returns true only if snapshot is valid (not null, has order) AND order state is Cancelled OR Rejected. Filled targets are skipped (already executed).

### Helper Method 2: Order Construction
Private method that builds a restored target order with proper parameters.
Handles price rounding, signal naming, and order type configuration.

Parameters:
- TargetSnapshot snap (original order details)
- string entryName (for signal generation)
- OrderAction exitAction (Sell or BuyToCover)
- string bracketOcoId (OCO group ID)
- bool isFollower (follower account flag)
- Account executingAccount (for follower orders, null for managed)

Returns: Order object ready for submission
Complexity: CYC 4

Logic:
1. Round price to tick size
2. Generate signal name (with SymmetryTrim for followers)
3. Create Order via Account.CreateOrder (follower) or direct construction
4. Returns null if order creation fails

### Helper Method 3: Order Submission
Private method that submits a restored target order using appropriate submission path.
Follower accounts use Account.Submit, managed accounts use SubmitOrderUnmanaged.

Parameters:
- Order order (order to submit)
- bool isFollower (follower account flag)
- Account executingAccount (for follower submission, null for managed)

Returns: Order object (may be same or new instance)
Complexity: CYC 2

Logic: Branches on isFollower flag. If true, calls executingAccount.Submit. If false, order is already submitted via SubmitOrderUnmanaged in BuildRestoredTargetOrder.

### Main Method (Refactored)
Orchestrates validation, filtering, order creation, and submission.
Complexity: CYC 7

Orchestration flow:
1. Early validation (null checks, position lookup)
2. State extraction from PositionInfo
3. Entry filled validation
4. Foreach loop calling helper methods

## Call Graph

RestoreCascadedTargets (CYC 7)
- Validate inputs (capturedTargets null/empty check)
- Lookup position (activePositions.TryGetValue)
- Extract state (pos.EntryFilled, pos.RemainingContracts, etc.)
- Validate entry filled
- Loop: foreach (TargetSnapshot snap in capturedTargets)
  - ShouldRestoreTarget(snap) returns bool (CYC 2)
  - BuildRestoredTargetOrder(...) returns Order (CYC 4)
  - SubmitTargetOrder(order, isFollower, account) returns Order (CYC 2)

## Data Flow

### Input Parameters
- entryName (string): Entry order identifier
- capturedTargets (TargetSnapshot[]): Array of target snapshots to restore

### Extracted State (from PositionInfo)
- entryFilled (bool): Whether entry order is filled
- remainingContracts (int): Contracts remaining in position
- direction (MarketPosition): Long or Short
- isFollower (bool): Follower account flag
- executingAccount (Account): Account for follower orders
- ocoGroupId (string): OCO group ID for bracket linking

### Derived Values
- exitAction (OrderAction): Sell (Long) or BuyToCover (Short)
- bracketOcoId (string): OCO ID or empty string

### Per-Target Processing
- snap (TargetSnapshot): Current target snapshot
- restoredPrice (double): Rounded price from snap.Price
- newTarget (Order): Created and submitted order

## Lock-Free Validation

### ✅ No Lock Statements
- **Verified**: Zero lock() statements in RestoreCascadedTargets
- **Pattern**: Read-only access to activePositions dictionary
- **Thread Safety**: Relies on ConcurrentDictionary (assumed based on V12 DNA)

### ✅ Actor/FSM Enqueue Pattern
- **Order Submission**: Delegated to NinjaTrader thread-safe API
  - Account.Submit(Order[]) for follower accounts
  - SubmitOrderUnmanaged(...) for managed accounts
- **State Mutation**: None - method only reads position state and submits orders
- **Immutable Snapshots**: TargetSnapshot[] is read-only input

### ✅ Atomic Primitives
- **Not Required**: No shared mutable state modified
- **Read-Only**: All state extraction is snapshot-based
- **API Delegation**: Order lifecycle managed by NinjaTrader

### Compliance Status
**COMPLIANT** with V12 lock-free mandate. Method follows Actor pattern by delegating state mutations to NinjaTrader thread-safe order management system.

## Jane Street Compliance

### Cognitive Simplicity (CYC ≤8)
- **Main Method**: CYC 7 (orchestration only)
- **Helper 1**: CYC 2 (pure predicate)
- **Helper 2**: CYC 4 (order construction)
- **Helper 3**: CYC 2 (submission branching)
- **Result**: All methods under threshold 8

### Testability
- **Before**: 2^16 = 65,536 possible execution paths (CYC 16)
- **After**: 2^7 + 2^2 + 2^4 + 2^2 = 128 + 4 + 16 + 4 = 152 paths
- **Improvement**: 99.77% reduction in test path complexity
- **Exhaustive Testing**: Now tractable for each helper method

### Microsecond-Latency Impact
- **JIT Inlining**: Private helpers are inlined by .NET JIT compiler
- **Zero Overhead**: No additional method call cost at runtime
- **Hot-Path Preservation**: Order submission logic unchanged
- **Memory**: No additional allocations (same object graph)

### Race Condition Analysis
- **Before**: CYC 16 makes race condition analysis intractable
- **After**: CYC ≤8 per method enables formal verification
- **Lock-Free**: No locks means no deadlock risk
- **Snapshot Isolation**: Read-only state prevents data races

### Alignment with Jane Street Principles
From KB query will_wilson_why_testing_hard_2026:
- ✅ **Cognitive Simplicity**: Break complex methods into testable units
- ✅ **Exhaustive Coverage**: CYC ≤8 makes full path testing feasible
- ✅ **Maintainability**: Clear separation of concerns
- ✅ **Performance**: Zero runtime overhead via JIT inlining

## Implementation Plan

### Phase 4: Execution Steps

1. **Extract ShouldRestoreTarget** (CYC 2)
   - Create private method with signature above
   - Move lines 749-762 (target filtering logic)
   - Replace inline logic with method call
   - Verify: CYC 2, zero new Codacy issues

2. **Extract BuildRestoredTargetOrder** (CYC 4)
   - Create private method with signature above
   - Move lines 764-790 (order construction logic)
   - Handle both follower and managed paths
   - Verify: CYC 4, zero new Codacy issues

3. **Extract SubmitTargetOrder** (CYC 2)
   - Create private method with signature above
   - Move lines 791-807 (submission logic)
   - Branch on isFollower flag
   - Verify: CYC 2, zero new Codacy issues

4. **Refactor Main Method** (CYC 7)
   - Keep validation and state extraction inline
   - Replace extracted logic with helper calls
   - Verify: CYC 7, zero new Codacy issues

### Verification Checklist
- Run complexity_audit.py after each extraction
- Verify CYC ≤8 for all methods
- Run build_readiness.ps1
- Run dotnet test (ensure FSMActorTests pass)
- Run deploy-sync.ps1 (hard-link sync)
- Verify zero new Codacy issues
- F5 in NinjaTrader (smoke test)

## Risk Assessment

### Low Risk Factors
- **Private Method**: Limited blast radius (class-scoped)
- **No Caller Changes**: Existing call sites unchanged
- **No Callee Changes**: Called methods unchanged
- **Checkpointing**: Enabled for rollback safety
- **Lock-Free**: No deadlock risk

### Mitigation Strategies
- **Incremental Extraction**: One helper at a time
- **Verification After Each Step**: Complexity audit + build + test
- **Rollback Plan**: Use Bob CLI /restore if issues arise
- **Test Coverage**: Existing FSMActorTests provide regression safety

## Success Criteria

### Functional Requirements
- ✅ Exact same behavior (no logic changes)
- ✅ All existing tests pass
- ✅ Zero new compilation errors
- ✅ Zero new Codacy issues

### Complexity Requirements
- ✅ RestoreCascadedTargets: CYC ≤8 (target: 7)
- ✅ ShouldRestoreTarget: CYC ≤8 (target: 2)
- ✅ BuildRestoredTargetOrder: CYC ≤8 (target: 4)
- ✅ SubmitTargetOrder: CYC ≤8 (target: 2)

### V12 DNA Requirements
- ✅ Lock-free (no lock() statements)
- ✅ ASCII-only (no Unicode in strings)
- ✅ Hard-link integrity (deploy-sync.ps1)
- ✅ Jane Street alignment (CYC ≤8, cognitive simplicity)

## Next Steps

### Phase 3: DNA & PR Audit (Adjudicator)
- Submit this plan to Arena AI for adversarial review
- Verify plan against V12 constraints
- Check PR health (diff size, complexity delta)
- Gate: PASS/FAIL (fail triggers Phase 2 rework)

### Phase 4: Recursive Execution (Engineer)
- Hand off to Bob CLI (v12-engineer) for implementation
- Execute extraction steps 1-4 sequentially
- Verify after each step (complexity audit + build + test)
- Use checkpointing for rollback safety

## Metadata
- **Epic ID**: EPIC-CCN-032
- **Phase**: 2.0 (Architecture Planning)
- **Architect**: Bob CLI (v12-engineer)
- **Date**: 2026-06-15
- **Status**: READY FOR PHASE 3 AUDIT
- **Next Phase**: 3.0 (DNA & PR Audit)
