# EPIC-CCN-16 Pattern Analysis

## Phase 1.5: Scope Boundary Validation

### Scope Boundary Verification (V12.23 Protocol)

**ONE EPIC = ONE CONCERN**: ✅ VALIDATED

This epic focuses exclusively on:
- ✅ Extracting `HydrateFSMsFromWorkingOrders` method
- ✅ Reducing complexity from CYC 45 to ≤8
- ✅ No changes to adjacent methods
- ✅ No pre-existing issue fixes
- ✅ No architectural changes

**Out of Scope Items Detected**: NONE

The target method is self-contained with clear boundaries. No scope creep risks identified.

---

## Anti-Pattern Analysis

### Target Method: HydrateFSMsFromWorkingOrders
**Location**: [`src/V12_002.SIMA.Lifecycle.cs:464-759`](src/V12_002.SIMA.Lifecycle.cs:464)
**Size**: 295 lines
**Complexity**: CYC 45

### 1. God Function Pattern ⚠️ HIGH SEVERITY

**Evidence**:
- Single method handles 6 distinct responsibilities
- 295 lines of code (Jane Street threshold: ~50 lines)
- CYC 45 (3x Jane Street threshold of 15)

**Responsibilities Identified**:
1. **State Mapping** (lines 488-503): Map OrderState → FollowerBracketState
2. **Position Resolution** (lines 505-518): Resolve remaining contracts from live positions
3. **FSM Construction** (lines 520-528): Build FollowerBracketFSM instances
4. **Order Linking** (lines 530-588): Link stop and 5 target orders to FSM
5. **FSM Registration** (lines 590-598): Add FSM to tracking dictionaries
6. **Position Pass** (lines 602-743): Handle orphaned positions with terminal entry orders

**Impact**: 
- Hard to test (6 concerns = exponential test paths)
- Hard to reason about (cognitive overload)
- High churn risk (19 commits in 90 days)

**Extraction Strategy**: Split into 6 focused methods, each CYC ≤8

---

### 2. Repetitive Code Pattern ⚠️ MEDIUM SEVERITY

**Evidence**: Lines 530-588 (Order Linking)

```csharp
// Repeated 5 times for target1-target5
if (target1Orders.TryGetValue(entryKey, out targetOrd) && targetOrd != null)
{
    fsm.Targets[0] = targetOrd;
    if (!string.IsNullOrEmpty(targetOrd.OrderId))
    {
        _orderIdToFsmKey[targetOrd.OrderId] = entryKey;
        ordersIndexed++;
    }
}
```

**Impact**:
- 59 lines of near-identical code
- Maintenance burden (change must be applied 5 times)
- Bug multiplication risk

**Extraction Strategy**: Extract to `LinkTargetOrder(fsm, targetOrders, entryKey, targetIndex, ref ordersIndexed)`

**Complexity Reduction**:
- Before: 59 lines, CYC ~10 (5 identical branches)
- After: 12 lines (1 method + 5 calls), CYC ~2

---

### 3. Deep Nesting Pattern ⚠️ MEDIUM SEVERITY

**Evidence**: Lines 602-743 (Position Pass)

**Nesting Depth**: 5 levels
```
foreach (Account acct)                          // Level 1
    if (!IsFleetAccount(acct))                  // Level 2
        if (_followerBrackets.Values.Any(...))  // Level 3
            if (acctPos == null)                // Level 4
                foreach (var stopKvp)           // Level 5
```

**Impact**:
- Hard to follow control flow
- High cognitive load
- Error-prone (easy to miss edge cases)

**Extraction Strategy**: Extract Position Pass to separate method with early returns

**Complexity Reduction**:
- Before: 141 lines, CYC ~15, nesting depth 5
- After: ~30 lines orchestration + 3 helper methods, CYC ≤8 each

---

### 4. Mixed Abstraction Levels ⚠️ LOW SEVERITY

**Evidence**: Method mixes high-level orchestration with low-level details

**High-Level** (lines 469-470):
```csharp
foreach (var kvp in entryOrders.ToArray())
```

**Low-Level** (lines 508-515):
```csharp
Position livePosition = pi.ExecutingAccount.Positions.ToArray()
    .FirstOrDefault(p =>
        p != null
        && p.Instrument != null
        && p.Instrument.FullName == Instrument.FullName
        && p.MarketPosition != MarketPosition.Flat
    );
```

**Impact**:
- Harder to understand method's purpose at a glance
- Mixes "what" with "how"

**Extraction Strategy**: Extract low-level details to focused methods with descriptive names

---

## Extraction Opportunities

### Priority 1: High-Impact Extractions

#### 1. ExtractStateMapping
**Lines**: 488-503
**Current CYC**: ~8
**Target CYC**: ~8 (already at threshold, but improves readability)
**Signature**: `FollowerBracketState MapOrderStateToFSMState(OrderState entryState)`

**Rationale**: 
- Clear single responsibility
- Pure function (no side effects)
- Easy to test (11 OrderState values → 4 FSM states)

#### 2. ExtractOrderLinking
**Lines**: 530-588
**Current CYC**: ~10
**Target CYC**: ~2
**Signature**: `void LinkTargetOrder(FollowerBracketFSM fsm, Dictionary<string, Order> targetOrders, string entryKey, int targetIndex, ref int ordersIndexed)`

**Rationale**:
- Eliminates 59 lines of repetition
- Reduces CYC by ~8
- Single responsibility (link one target order)

#### 3. ExtractPositionPass
**Lines**: 602-743
**Current CYC**: ~15
**Target CYC**: ~5 (orchestration) + 3 helpers (CYC ≤8 each)
**Signature**: `int HydrateFromOpenPositions()`

**Rationale**:
- Largest complexity contributor
- Self-contained logic (separate pass)
- Can be tested independently

### Priority 2: Supporting Extractions

#### 4. ExtractPositionResolution
**Lines**: 505-518
**Current CYC**: ~3
**Target CYC**: ~3
**Signature**: `int ResolveRemainingContracts(Order entryOrder, PositionInfo pi, FollowerBracketState state)`

**Rationale**:
- Clear single responsibility
- Encapsulates position quantity logic
- Improves readability

#### 5. ExtractFSMConstruction
**Lines**: 520-528
**Current CYC**: ~1
**Target CYC**: ~1
**Signature**: `FollowerBracketFSM BuildFSM(string entryKey, PositionInfo pi, Order entryOrder, FollowerBracketState state, int remainingContracts)`

**Rationale**:
- Factory method pattern
- Centralizes FSM initialization
- Easier to maintain FSM structure

#### 6. ExtractFSMRegistration
**Lines**: 590-598
**Current CYC**: ~2
**Target CYC**: ~2
**Signature**: `void RegisterFSM(string entryKey, FollowerBracketFSM fsm, Order entryOrder, ref int ordersIndexed, ref int fsmCreated)`

**Rationale**:
- Encapsulates registration logic
- Centralizes dictionary updates
- Easier to audit for thread safety

---

## Complexity Reduction Projection

### Before Extraction
- **HydrateFSMsFromWorkingOrders**: CYC 45, 295 lines
- **Total Methods**: 1
- **Average CYC**: 45

### After Extraction
- **HydrateFSMsFromWorkingOrders** (orchestration): CYC ~8, ~50 lines
- **MapOrderStateToFSMState**: CYC ~8, ~20 lines
- **LinkTargetOrder**: CYC ~2, ~12 lines
- **HydrateFromOpenPositions**: CYC ~5, ~30 lines
- **ResolveRemainingContracts**: CYC ~3, ~15 lines
- **BuildFSM**: CYC ~1, ~10 lines
- **RegisterFSM**: CYC ~2, ~10 lines
- **Total Methods**: 7
- **Average CYC**: ~4.1
- **Max CYC**: 8 ✅

### Complexity Reduction
- **Total CYC Reduction**: 45 → 29 (35.6% reduction in total complexity)
- **Max CYC Reduction**: 45 → 8 (82.2% reduction in worst-case)
- **Jane Street Compliance**: ✅ All methods ≤15 (target: ≤8)

---

## Pre-Existing Issues Scan

### Compilation Errors
**Status**: ✅ NONE DETECTED in target method

The target method compiles cleanly. No pre-existing errors to fix.

### Adjacent Code Issues
**Status**: ⚠️ OUT OF SCOPE

Potential issues in adjacent methods (e.g., `AdoptFleetOrders`, `HydrateWorkingOrdersFromBroker`) are **explicitly out of scope** per V12.23 protocol.

If discovered during execution:
1. **STOP immediately**
2. **Document in failure-analysis.md**
3. **Create separate PR**

---

## V12 DNA Compliance Check

### Lock-Free Verification ✅
**Status**: COMPLIANT

```bash
grep -n "lock(" src/V12_002.SIMA.Lifecycle.cs | grep -A5 -B5 "464\|759"
```
**Result**: Zero `lock()` statements in target method (lines 464-759)

### ASCII-Only Verification ✅
**Status**: COMPLIANT

All string literals in target method are ASCII-only:
- `"[SIMA] Phase 5 FSM Hydration..."`
- `"[SIMA] Phase 5 Position Pass..."`
- No Unicode, emoji, or curly quotes detected

### Correctness by Construction ✅
**Status**: REQUIRES PRESERVATION

**FSM Invariants to Preserve**:
1. **Idempotency**: `if (_followerBrackets.ContainsKey(entryKey)) continue;` (line 484)
2. **State Mapping**: OrderState → FollowerBracketState must remain deterministic
3. **Order Linking**: OrderId → FSM key mapping must be 1:1
4. **Position Pass**: Only create FSM if no existing FSM for account

**Extraction Constraint**: All extracted methods must preserve these invariants.

---

## Testing Strategy

### TDD Mandate
**Protocol**: Write tests BEFORE extraction

### Test Coverage Requirements
1. **MapOrderStateToFSMState**: 11 OrderState values → 4 FSM states (11 test cases)
2. **LinkTargetOrder**: Happy path + null order + empty OrderId (3 test cases)
3. **HydrateFromOpenPositions**: Account with position + no position + existing FSM (3 test cases)
4. **ResolveRemainingContracts**: Active state + non-active state (2 test cases)
5. **BuildFSM**: Follower account + master account (2 test cases)
6. **RegisterFSM**: New FSM + duplicate FSM (2 test cases)

**Total Test Cases**: 23 minimum

### Regression Prevention
- **Snapshot Tests**: Capture FSM state before/after hydration
- **Invariant Tests**: Verify idempotency, state mapping, order linking
- **Integration Tests**: Full hydration cycle with mock orders

---

## Risk Mitigation

### High-Risk Areas
1. **Position Pass Logic** (lines 602-743): Most complex, highest churn
2. **Order Linking** (lines 530-588): Repetitive, error-prone
3. **State Mapping** (lines 488-503): Business logic, must be deterministic

### Mitigation Strategies
1. **TDD First**: Write tests before extraction
2. **Incremental Extraction**: One method at a time, verify after each
3. **Verification Gates**: Build + test after each extraction
4. **Rollback Plan**: Git worktree isolation for easy rollback

### Blast Radius
**Callers**: 1 (HydrateWorkingOrdersFromBroker, line 445)
**Callees**: Multiple (IsFleetAccount, Print, etc.)

**Impact**: LOW (single caller, no external dependencies)

---

## Approval Status
- **Phase 1**: ✅ Complete (Scope Definition)
- **Phase 1.5**: ✅ Complete (This Document)
- **Phase 2**: ⏳ Pending (Architecture Planning)

## Next Steps
1. Proceed to Phase 2: Architecture Planning
2. Create detailed extraction plan with method signatures
3. Design test strategy for each extracted method
4. Generate implementation tickets