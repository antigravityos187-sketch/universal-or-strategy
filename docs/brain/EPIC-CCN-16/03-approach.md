# EPIC-CCN-16 Extraction Approach

## Phase 2: Architecture Planning - Extraction Strategy

### Extraction Philosophy

**Principle**: "Make illegal states unrepresentable" (Jane Street)

**Strategy**: Extract methods that:
1. Have single, clear responsibility
2. Are easy to test in isolation
3. Reduce cognitive load
4. Preserve exact behavior
5. Maintain thread safety guarantees

---

## Method Signatures

### 1. MapOrderStateToFSMState
**Purpose**: Map NinjaTrader OrderState to V12 FollowerBracketState

**Signature**:
```csharp
/// <summary>
/// Maps NinjaTrader OrderState to V12 FollowerBracketState.
/// Pure function - no side effects, deterministic mapping.
/// </summary>
/// <param name="entryState">NinjaTrader order state</param>
/// <returns>Corresponding FSM state, or null if terminal state (skip FSM creation)</returns>
private FollowerBracketState? MapOrderStateToFSMState(OrderState entryState)
```

**Complexity**: CYC ~8 (8 branches)
**Lines**: ~20
**Dependencies**: None (pure function)
**Test Cases**: 11 (one per OrderState value)

**Implementation Notes**:
- Return `null` for terminal states (Cancelled, Rejected, etc.)
- Caller checks for `null` and skips FSM creation
- No logging (pure function)

---

### 2. ResolveRemainingContracts
**Purpose**: Determine remaining contracts for FSM based on order state and live position

**Signature**:
```csharp
/// <summary>
/// Resolves remaining contracts for FSM based on order state and live position.
/// For Active state, queries live position. Otherwise uses order quantity.
/// </summary>
/// <param name="entryOrder">Entry order</param>
/// <param name="pi">Position info (contains executing account)</param>
/// <param name="state">FSM state (determines resolution strategy)</param>
/// <returns>Remaining contracts (always >= 0)</returns>
private int ResolveRemainingContracts(
    Order entryOrder,
    PositionInfo pi,
    FollowerBracketState state
)
```

**Complexity**: CYC ~3 (2 branches + 1 LINQ)
**Lines**: ~18
**Dependencies**: Instrument (instance field)
**Test Cases**: 3 (Active with position, Active without position, non-Active)

**Implementation Notes**:
- Extract position lookup logic (lines 508-515)
- Use `Math.Max(0, ...)` to ensure non-negative
- Handle null position gracefully

---

### 3. BuildFSM
**Purpose**: Factory method to construct FollowerBracketFSM instance

**Signature**:
```csharp
/// <summary>
/// Factory method to construct FollowerBracketFSM instance.
/// Centralizes FSM initialization logic.
/// </summary>
/// <param name="entryKey">FSM key (entry order name)</param>
/// <param name="pi">Position info (contains account name)</param>
/// <param name="entryOrder">Entry order (may be null for position pass)</param>
/// <param name="state">Initial FSM state</param>
/// <param name="remainingContracts">Remaining contracts</param>
/// <returns>Initialized FSM instance</returns>
private FollowerBracketFSM BuildFSM(
    string entryKey,
    PositionInfo pi,
    Order entryOrder,
    FollowerBracketState state,
    int remainingContracts
)
```

**Complexity**: CYC ~1 (no branches)
**Lines**: ~12
**Dependencies**: None
**Test Cases**: 2 (with entry order, without entry order)

**Implementation Notes**:
- Simple object initialization
- No business logic
- Easy to maintain if FSM structure changes

---

### 4. RegisterFSM
**Purpose**: Register FSM in tracking dictionaries and update counters

**Signature**:
```csharp
/// <summary>
/// Registers FSM in tracking dictionaries and updates counters.
/// Centralizes dictionary update logic for easier auditing.
/// </summary>
/// <param name="entryKey">FSM key</param>
/// <param name="fsm">FSM to register</param>
/// <param name="entryOrder">Entry order (may be null for position pass)</param>
/// <param name="ordersIndexed">Counter (incremented if entry order linked)</param>
/// <param name="fsmCreated">Counter (always incremented)</param>
private void RegisterFSM(
    string entryKey,
    FollowerBracketFSM fsm,
    Order entryOrder,
    ref int ordersIndexed,
    ref int fsmCreated
)
```

**Complexity**: CYC ~2 (1 null check + 1 empty check)
**Lines**: ~12
**Dependencies**: `_followerBrackets`, `_orderIdToFsmKey` (instance fields)
**Test Cases**: 3 (with entry order, without entry order, duplicate key)

**Implementation Notes**:
- Uses `TryAdd` (idempotent, thread-safe)
- Links entry order if present
- Updates both counters

---

### 5. HydrateFromOpenPositions
**Purpose**: Position Pass - create FSMs for accounts with open positions but terminal entry orders

**Signature**:
```csharp
/// <summary>
/// Position Pass: Creates FSMs for accounts with open positions but terminal entry orders.
/// Handles edge case where entry order is cancelled/rejected but position still open.
/// </summary>
/// <param name="stopOrders">Dictionary of stop orders (for key recovery)</param>
/// <param name="target1Orders">Dictionary of target1 orders</param>
/// <param name="target2Orders">Dictionary of target2 orders</param>
/// <param name="target3Orders">Dictionary of target3 orders</param>
/// <param name="target4Orders">Dictionary of target4 orders</param>
/// <param name="target5Orders">Dictionary of target5 orders</param>
/// <param name="ordersIndexed">Counter (incremented for each order linked)</param>
/// <param name="fsmCreated">Counter (incremented for each FSM created)</param>
/// <returns>Number of FSMs created in position pass</returns>
private int HydrateFromOpenPositions(
    Dictionary<string, Order> stopOrders,
    Dictionary<string, Order> target1Orders,
    Dictionary<string, Order> target2Orders,
    Dictionary<string, Order> target3Orders,
    Dictionary<string, Order> target4Orders,
    Dictionary<string, Order> target5Orders,
    ref int ordersIndexed,
    ref int fsmCreated
)
```

**Complexity**: CYC ~8 (multiple early returns + nested loops)
**Lines**: ~60 (orchestration) + 3 helper methods
**Dependencies**: `_followerBrackets`, `_positionPassFailedFirstSeen`, `IsFleetAccount`, `Print`
**Test Cases**: 5 (account with position, no position, existing FSM, no stop order, successful recovery)

**Implementation Notes**:
- Most complex extraction (lines 602-743)
- Consider sub-extraction: `RecoverStopOrderKey`, `CreatePositionPassFSM`
- Preserve REAPER grace window logic (lines 651-655)
- Preserve logging (lines 645-650, 736-742)

---

### 6. HydrateFSMsFromWorkingOrders (Refactored)
**Purpose**: Orchestration - coordinate FSM hydration using extracted methods

**Signature** (unchanged):

**Note**: `LinkTargetOrderToFSM` already exists at line 463 and is in use. No extraction needed.
```csharp
/// <summary>
/// Phase 5: Rebuilds _followerBrackets and _orderIdToFsmKey from already-adopted
/// working orders. Called from HydrateWorkingOrdersFromBroker() before the
/// adoption-complete gate is set. Idempotent -- safe to call on every reconnect.
/// </summary>
private void HydrateFSMsFromWorkingOrders()
```

**Complexity**: CYC ~8 (orchestration only)
**Lines**: ~60 (down from 295)
**Dependencies**: 5 extracted methods + existing LinkTargetOrderToFSM helper
**Test Cases**: 2 (integration tests - full hydration cycle, idempotency)

**Refactored Structure**:
```csharp
private void HydrateFSMsFromWorkingOrders()
{
    int fsmCreated = 0;
    int ordersIndexed = 0;

    // Entry Order Pass
    foreach (var kvp in entryOrders.ToArray())
    {
        string entryKey = kvp.Key;
        Order entryOrder = kvp.Value;
        if (entryOrder == null) continue;

        // Skip master account entries
        PositionInfo pi;
        if (!activePositions.TryGetValue(entryKey, out pi) || !pi.IsFollower) continue;
        if (pi.ExecutingAccount == null) continue;

        // Idempotent guard
        if (_followerBrackets.ContainsKey(entryKey)) continue;

        // Map state
        FollowerBracketState? state = MapOrderStateToFSMState(entryOrder.OrderState);
        if (state == null) continue; // Terminal state

        // Resolve contracts
        int remainingContracts = ResolveRemainingContracts(entryOrder, pi, state.Value);

        // Build FSM
        var fsm = BuildFSM(entryKey, pi, entryOrder, state.Value, remainingContracts);

        // Link stop order
        Order stopOrd;
        if (stopOrders.TryGetValue(entryKey, out stopOrd) && stopOrd != null)
        {
            fsm.StopOrder = stopOrd;
            if (!string.IsNullOrEmpty(stopOrd.OrderId))
            {
                _orderIdToFsmKey[stopOrd.OrderId] = entryKey;
                ordersIndexed++;
            }
        }

        // Link target orders (using existing helper)
        LinkTargetOrderToFSM(ref fsm, entryKey, 0, target1Orders, ref ordersIndexed);
        LinkTargetOrderToFSM(ref fsm, entryKey, 1, target2Orders, ref ordersIndexed);
        LinkTargetOrderToFSM(ref fsm, entryKey, 2, target3Orders, ref ordersIndexed);
        LinkTargetOrderToFSM(ref fsm, entryKey, 3, target4Orders, ref ordersIndexed);
        LinkTargetOrderToFSM(ref fsm, entryKey, 4, target5Orders, ref ordersIndexed);

        // Register FSM
        RegisterFSM(entryKey, fsm, entryOrder, ref ordersIndexed, ref fsmCreated);
    }

    // Position Pass
    int positionFsmCreated = HydrateFromOpenPositions(
        stopOrders, target1Orders, target2Orders, target3Orders, target4Orders, target5Orders,
        ref ordersIndexed, ref fsmCreated
    );

    Print(string.Format(
        "[SIMA] Phase 5 FSM Hydration (Position Pass): {0} Active FSMs created from open positions.",
        positionFsmCreated
    ));

    Print(string.Format(
        "[SIMA] Phase 5 FSM Hydration: {0} FSMs created, {1} order IDs indexed.",
        fsmCreated, ordersIndexed
    ));
}
```

---

## Extraction Order

### Phase 1: Low-Risk Extractions (Parallel)
1. **MapOrderStateToFSMState** - Pure function, no dependencies
2. **BuildFSM** - Simple factory, no dependencies

**Duration**: 1 ticket each (~30 min each)
**Risk**: LOW
**Parallelizable**: YES

**Note**: LinkTargetOrderToFSM already exists (line 463) - no extraction needed.

### Phase 2: Medium-Risk Extractions (Sequential)
3. **ResolveRemainingContracts** - Depends on MapOrderStateToFSMState
4. **RegisterFSM** - Independent extraction

**Duration**: 1 ticket each (~45 min each)
**Risk**: MEDIUM
**Parallelizable**: NO (sequential dependencies)

### Phase 3: High-Risk Extraction (Sequential)
5. **HydrateFromOpenPositions** - Depends on BuildFSM, RegisterFSM, and existing LinkTargetOrderToFSM

**Duration**: 1 ticket (~90 min)
**Risk**: HIGH
**Parallelizable**: NO (depends on all previous)

### Phase 4: Refactor Parent (Sequential)
6. **Refactor HydrateFSMsFromWorkingOrders** - Depends on all above

**Duration**: 1 ticket (~30 min)
**Risk**: LOW (simple orchestration)
**Parallelizable**: NO (final integration)

**Total Duration**: ~4.0 hours (6 tickets)

---

## Testing Strategy

### Test-Driven Development (TDD) Protocol
**Mandate**: Write tests BEFORE extraction

### Test Structure
**File**: `tests/V12_Performance.Tests/Core/FSMHydrationTests.cs` (new)

**Test Classes**:
1. `MapOrderStateToFSMStateTests` - 11 test cases
2. `ResolveRemainingContractsTests` - 3 test cases
3. `BuildFSMTests` - 2 test cases
4. `RegisterFSMTests` - 3 test cases
5. `HydrateFromOpenPositionsTests` - 6 test cases (includes grace window test)
6. `HydrateFSMsFromWorkingOrdersIntegrationTests` - 2 test cases

**Total Test Cases**: 27 (LinkTargetOrderToFSM already tested in production)

**Note**: LinkTargetOrderToFSM tests removed (method already exists and is production-validated).
**Addition**: Grace window preservation test added to HydrateFromOpenPositionsTests (sentinel recommendation).

### Test Patterns

#### 1. Pure Function Tests (MapOrderStateToFSMState)
```csharp
[Theory]
[InlineData(OrderState.Filled, FollowerBracketState.Active)]
[InlineData(OrderState.PartFilled, FollowerBracketState.Active)]
[InlineData(OrderState.Accepted, FollowerBracketState.Accepted)]
[InlineData(OrderState.Working, FollowerBracketState.Submitted)]
[InlineData(OrderState.Cancelled, null)] // Terminal state
public void MapOrderStateToFSMState_ReturnsCorrectMapping(
    OrderState input,
    FollowerBracketState? expected
)
{
    var result = _sut.MapOrderStateToFSMState(input);
    Assert.Equal(expected, result);
}
```

#### 2. Integration Tests (HydrateFSMsFromWorkingOrders)
```csharp
[Fact]
public void HydrateFSMsFromWorkingOrders_CreatesExpectedFSMs()
{
    // Arrange: Set up mock orders and positions
    // Act: Call HydrateFSMsFromWorkingOrders
    // Assert: Verify FSMs created, dictionaries populated
}

[Fact]
public void HydrateFSMsFromWorkingOrders_IsIdempotent()
{
    // Arrange: Set up mock orders
    // Act: Call twice
    // Assert: Same FSMs, no duplicates
}
```

#### 3. Invariant Tests
```csharp
[Fact]
public void RegisterFSM_PreservesOrderIdToFsmKeyMapping()
{
    // Assert: Each OrderId maps to exactly one FSM key
}

[Fact]
public void HydrateFromOpenPositions_PreservesIdempotency()
{
    // Assert: Existing FSMs not overwritten
}
```

---

## Migration Safety

### Verification Gates
After each extraction:
1. ✅ **Build**: `powershell -File .\scripts\build_readiness.ps1`
2. ✅ **Tests**: `dotnet test tests/V12_Performance.Tests/`
3. ✅ **Lint**: `powershell -File .\scripts\lint.ps1`
4. ✅ **Complexity**: `python scripts/complexity_audit.py`
5. ✅ **Git Commit**: Checkpoint for rollback

### Rollback Plan
**Strategy**: Git worktree isolation

**Setup**:
```powershell
git worktree add ../epic-ccn-16-worktree gitbutler/workspace
cd ../epic-ccn-16-worktree
```

**Rollback**:
```powershell
git reset --hard HEAD~1  # Undo last commit
```

**Cleanup**:
```powershell
cd ../universal-or-strategy
git worktree remove ../epic-ccn-16-worktree
```

---

## Interface Preservation

### Public API (None)
**Status**: ✅ NO PUBLIC API CHANGES

All extracted methods are `private`. No external callers affected.

### Internal API (Single Caller)
**Caller**: `HydrateWorkingOrdersFromBroker` (line 445)
**Call Site**: `HydrateFSMsFromWorkingOrders();`

**Status**: ✅ NO CHANGES REQUIRED

Caller signature unchanged. Extracted methods are internal implementation details.

---

## Documentation Updates

### Code Comments
**Strategy**: Preserve existing comments, add XML docs to extracted methods

**Example**:
```csharp
/// <summary>
/// Maps NinjaTrader OrderState to V12 FollowerBracketState.
/// Pure function - no side effects, deterministic mapping.
/// </summary>
/// <param name="entryState">NinjaTrader order state</param>
/// <returns>Corresponding FSM state, or null if terminal state (skip FSM creation)</returns>
/// <remarks>
/// Terminal states (Cancelled, Rejected, etc.) return null to signal caller to skip FSM creation.
/// This preserves the original behavior where terminal orders are ignored.
/// </remarks>
private FollowerBracketState? MapOrderStateToFSMState(OrderState entryState)
```

### Epic Documentation
**Files to Update**:
- ✅ This document (03-approach.md)
- ✅ Ticket files (04-tickets.md)
- ✅ Completion report (05-completion-report.md)

**No Updates Required**:
- ❌ README.md (internal refactoring)
- ❌ AGENTS.md (no protocol changes)
- ❌ Architecture docs (no architectural changes)

---

## Success Criteria

### Quantitative
- ✅ HydrateFSMsFromWorkingOrders: CYC ≤8 (currently 45)
- ✅ All extracted methods: CYC ≤8
- ✅ Test coverage: 100% of extracted methods (29 test cases)
- ✅ Build passes: Zero errors, zero warnings
- ✅ Complexity audit: All methods ≤15 (Jane Street threshold)

### Qualitative
- ✅ Code is more readable (single-responsibility methods)
- ✅ Easier to test (focused unit tests)
- ✅ Easier to maintain (clear separation of concerns)
- ✅ Preserves exact behavior (zero functional changes)
- ✅ Maintains thread safety (actor-serialized model)

### V12 DNA
- ✅ Zero `lock()` statements
- ✅ ASCII-only string literals
- ✅ Correctness by construction (FSM invariants preserved)
- ✅ Jane Street alignment (CYC ≤8)

---

## Approval Status
- **Phase 0**: ✅ Complete (Hotspot Analysis)
- **Phase 1**: ✅ Complete (Scope Definition)
- **Phase 1.5**: ✅ Complete (Pattern Analysis)
- **Phase 2**: ✅ Complete (Analysis + This Document)
- **Phase 2.3**: ✅ Complete (Sentinel Audit - PASSED)
- **Phase 3**: ⏳ In Progress (Architecture Validation)

## Phase 3 Updates (2026-06-09)

### Critical Finding Addressed
**Issue**: LinkTargetOrder extraction proposed, but `LinkTargetOrderToFSM` already exists at line 463.

**Resolution**:
- ✅ Removed LinkTargetOrder from extraction plan (Section 4 deleted)
- ✅ Updated extraction order (Phase 1: 2 tickets instead of 3)
- ✅ Updated total duration (4.0 hours instead of 4.5 hours)
- ✅ Updated test count (27 instead of 29 - removed 3 LinkTargetOrder tests)
- ✅ Added grace window test to HydrateFromOpenPositionsTests (sentinel recommendation)
- ✅ Updated refactored code to use existing `LinkTargetOrderToFSM` helper

### Validation Summary
- ✅ All V12 DNA constraints verified
- ✅ FSM invariants preserved
- ✅ Extraction order dependency-correct
- ✅ Test coverage adequate (27 test cases)
- ✅ Migration strategy safe (Git worktree isolation)

## Next Steps
1. Await Director approval for Phase 3 validation
2. Proceed to Phase 4 (Ticket Generation)
3. Begin TDD test writing (Phase 5)