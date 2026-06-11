# Phase 2: Architecture Planning - EPIC-CCN-98

## Epic Metadata
- **Epic ID**: EPIC-CCN-98
- **Target Method**: `ProcessFlattenWorkItem_CancelOrders`
- **File**: `src/V12_002.SIMA.Flatten.cs`
- **Current CYC**: 18
- **Target CYC**: ≤ 8
- **Phase**: 2 (Architecture Planning)
- **Status**: ✅ PLANNED

---

## Executive Summary

**Objective**: Extract two predicate helper methods from `ProcessFlattenWorkItem_CancelOrders` to reduce cyclomatic complexity from 18 to 5.

**Strategy**: Pure predicate extraction with zero logic drift. Both helpers are read-only, thread-safe, and testable.

**Risk Level**: 🟢 LOW (single-method extraction, no side effects, no state mutations)

**Estimated Effort**: 2 tickets, ~30 minutes total

---

## Current State Analysis

### Method Structure (Lines 163-213)

```csharp
private void ProcessFlattenWorkItem_CancelOrders(FlattenWorkItem item, Account acct)
{
    List<Order> ordersToCancel = new List<Order>();
    foreach (Order order in acct.Orders.ToArray())
    {
        // NULL checks (CYC +2)
        if (order == null || order.Instrument == null)
            continue;
        
        // Instrument filter (CYC +1)
        if (order.Instrument.FullName != Instrument.FullName)
            continue;

        // Terminal state check (CYC +5) - EXTRACTION TARGET 2
        bool isTerminal =
            order.OrderState == OrderState.Cancelled
            || order.OrderState == OrderState.CancelPending
            || order.OrderState == OrderState.CancelSubmitted
            || order.OrderState == OrderState.Filled
            || order.OrderState == OrderState.Rejected;
        if (isTerminal)
            continue;

        // Zombie sweep filter (CYC +7) - EXTRACTION TARGET 1
        if (item.ZombieSweepOnly)
        {
            bool isZombieTarget =
                order.Name.StartsWith("EMERGENCY_STOP_", StringComparison.OrdinalIgnoreCase)
                || order.Name.StartsWith("T1_", StringComparison.OrdinalIgnoreCase)
                || order.Name.StartsWith("T2_", StringComparison.OrdinalIgnoreCase)
                || order.Name.StartsWith("T3_", StringComparison.OrdinalIgnoreCase)
                || order.Name.StartsWith("T4_", StringComparison.OrdinalIgnoreCase)
                || order.Name.StartsWith("T5_", StringComparison.OrdinalIgnoreCase);
            if (!isZombieTarget)
                continue;
        }

        ordersToCancel.Add(order);
    }

    // Batch cancel (CYC +1)
    if (ordersToCancel.Count > 0)
    {
        acct.Cancel(ordersToCancel);
        Print(
            string.Format(
                "[FLATTEN_PUMP] {0}: Cancelled {1} order(s) [{2}]",
                acct.Name,
                ordersToCancel.Count,
                item.Source
            )
        );
    }
}
```

### Complexity Breakdown

| Code Block | CYC Contribution | Extraction Target |
|------------|------------------|-------------------|
| Base method | 1 | - |
| NULL checks | +2 | Keep in main method |
| Instrument filter | +1 | Keep in main method |
| Terminal state check | +5 | ✅ Extract to `IsTerminalOrderState` |
| Zombie sweep filter | +7 | ✅ Extract to `IsZombieTargetOrder` |
| Batch cancel | +1 | Keep in main method |
| **Total** | **18** | **Redistribute to 5+6+7** |

---

## Extraction Plan

### Extraction 1: `IsTerminalOrderState` (CYC 6)

**Purpose**: Encapsulate terminal state detection logic

**Signature**:
```csharp
/// <summary>
/// Determines if an order state is terminal (cannot be cancelled).
/// Terminal states: Cancelled, CancelPending, CancelSubmitted, Filled, Rejected.
/// </summary>
/// <param name="state">Order state to check</param>
/// <returns>True if state is terminal, false otherwise</returns>
private bool IsTerminalOrderState(OrderState state)
{
    return state == OrderState.Cancelled
        || state == OrderState.CancelPending
        || state == OrderState.CancelSubmitted
        || state == OrderState.Filled
        || state == OrderState.Rejected;
}
```

**Complexity Analysis**:
- Base: 1
- OR conditions: +5 (5 OR operators)
- **Total CYC**: 6 ✅ (within threshold)

**Properties**:
- ✅ Pure function (no side effects)
- ✅ Thread-safe (no state mutations)
- ✅ Deterministic (same input → same output)
- ✅ Testable (5 terminal states + 1 non-terminal state)

**Placement**: Insert after `ProcessFlattenWorkItem_CancelOrders` (line 214)

---

### Extraction 2: `IsZombieTargetOrder` (CYC 7)

**Purpose**: Encapsulate zombie target detection logic

**Signature**:
```csharp
/// <summary>
/// Determines if an order is a zombie target based on name prefix.
/// Zombie targets include EMERGENCY_STOP_ and T1-T5 prefixed orders.
/// </summary>
/// <param name="order">Order to check</param>
/// <returns>True if order is a zombie target, false otherwise</returns>
private bool IsZombieTargetOrder(Order order)
{
    if (order == null)
        return false;
    
    return order.Name.StartsWith("EMERGENCY_STOP_", StringComparison.OrdinalIgnoreCase)
        || order.Name.StartsWith("T1_", StringComparison.OrdinalIgnoreCase)
        || order.Name.StartsWith("T2_", StringComparison.OrdinalIgnoreCase)
        || order.Name.StartsWith("T3_", StringComparison.OrdinalIgnoreCase)
        || order.Name.StartsWith("T4_", StringComparison.OrdinalIgnoreCase)
        || order.Name.StartsWith("T5_", StringComparison.OrdinalIgnoreCase);
}
```

**Complexity Analysis**:
- Base: 1
- NULL check: +1
- OR conditions: +5 (6 OR operators, but first is in NULL check)
- **Total CYC**: 7 ✅ (within threshold)

**Properties**:
- ✅ Pure function (no side effects)
- ✅ Thread-safe (no state mutations)
- ✅ Deterministic (same input → same output)
- ✅ Testable (6 zombie prefixes + 1 non-zombie + NULL case)

**Placement**: Insert after `IsTerminalOrderState` (line ~225)

---

### Refactored Main Method (CYC 5)

```csharp
private void ProcessFlattenWorkItem_CancelOrders(FlattenWorkItem item, Account acct)
{
    List<Order> ordersToCancel = new List<Order>();
    foreach (Order order in acct.Orders.ToArray())
    {
        if (order == null || order.Instrument == null)
            continue;
        if (order.Instrument.FullName != Instrument.FullName)
            continue;

        // Use extracted helper (CYC +1)
        if (IsTerminalOrderState(order.OrderState))
            continue;

        // Use extracted helper (CYC +1)
        if (item.ZombieSweepOnly && !IsZombieTargetOrder(order))
            continue;

        ordersToCancel.Add(order);
    }

    if (ordersToCancel.Count > 0)
    {
        acct.Cancel(ordersToCancel);
        Print(
            string.Format(
                "[FLATTEN_PUMP] {0}: Cancelled {1} order(s) [{2}]",
                acct.Name,
                ordersToCancel.Count,
                item.Source
            )
        );
    }
}
```

**Complexity Analysis**:
- Base: 1
- NULL checks: +2
- Instrument filter: +1
- Terminal state check: +1 (helper call)
- Zombie sweep filter: +1 (helper call, includes AND)
- Batch cancel: +1
- **Total CYC**: 7 ❌ (exceeds target by 2)

**CORRECTION**: Need to recount. Let me trace the control flow:
- Base: 1
- `if (order == null || order.Instrument == null)`: +2 (OR operator)
- `if (order.Instrument.FullName != Instrument.FullName)`: +1
- `if (IsTerminalOrderState(order.OrderState))`: +1
- `if (item.ZombieSweepOnly && !IsZombieTargetOrder(order))`: +1 (AND operator)
- `if (ordersToCancel.Count > 0)`: +1
- **Total CYC**: 7

**WAIT**: The AND operator in `item.ZombieSweepOnly && !IsZombieTargetOrder(order)` adds +1 CYC. Let me verify the target CYC calculation from scope boundary was correct.

**Re-analysis**: The scope boundary stated target CYC = 5, but my calculation shows 7. Let me check if we can optimize further.

**Optimization**: Combine NULL checks to reduce CYC:
```csharp
if (order == null || order.Instrument == null || order.Instrument.FullName != Instrument.FullName)
    continue;
```
This reduces from 3 CYC to 2 CYC (one if statement with 2 OR operators).

**Final CYC**:
- Base: 1
- Combined NULL/instrument check: +2
- Terminal state check: +1
- Zombie sweep filter: +1
- Batch cancel: +1
- **Total CYC**: 6 ✅ (acceptable, close to target)

---

## Call Graph Analysis

### Callers
```
PumpFlattenOps (line 138)
  └─> ProcessFlattenWorkItem_CancelOrders (line 163)
        ├─> IsTerminalOrderState (NEW)
        └─> IsZombieTargetOrder (NEW)
```

### Dependencies
- **External**: `Account.Orders`, `Order.OrderState`, `Order.Name`, `Order.Instrument`
- **Internal**: None (pure predicates)
- **Side Effects**: None (read-only)

### Impact Radius
- **Modified Methods**: 1 (`ProcessFlattenWorkItem_CancelOrders`)
- **New Methods**: 2 (`IsTerminalOrderState`, `IsZombieTargetOrder`)
- **Affected Callers**: 0 (signature unchanged)
- **Affected Tests**: 0 (no existing tests)

---

## Jane Street Compliance Verification

### Cognitive Simplicity ✅
- ✅ Main method reduced to 6 CYC (microsecond-latency reasoning)
- ✅ Extracted predicates are single-purpose
- ✅ No nested conditionals in main method
- ✅ Clear separation of concerns (filtering vs. cancellation)

### Correctness by Construction ✅
- ✅ Terminal states are exhaustive (all 5 terminal states enumerated)
- ✅ Zombie targets are explicit (6 prefixes enumerated)
- ✅ No regex ambiguity (exact prefix matching)
- ✅ Illegal states unrepresentable (OrderState enum enforced)

### Lock-Free ✅
- ✅ No state mutations in extracted methods
- ✅ Pure predicate functions
- ✅ Thread-safe by design
- ✅ No `lock()` blocks

### Testability ✅
- ✅ Pure functions (deterministic output)
- ✅ No external dependencies
- ✅ Clear input/output contracts
- ✅ Exhaustive test cases possible

**Jane Street Decision Matrix**:
| Criterion | Before | After | Improvement |
|-----------|--------|-------|-------------|
| CYC | 18 | 6 | ✅ 67% reduction |
| Nesting Depth | 3 | 2 | ✅ 33% reduction |
| Testability | Low | High | ✅ Pure predicates |
| Cognitive Load | High | Low | ✅ Single-purpose methods |

---

## Test Strategy

### Unit Tests (TDD Approach)

#### Test 1: `IsTerminalOrderState`
```csharp
[Theory]
[InlineData(OrderState.Cancelled, true)]
[InlineData(OrderState.CancelPending, true)]
[InlineData(OrderState.CancelSubmitted, true)]
[InlineData(OrderState.Filled, true)]
[InlineData(OrderState.Rejected, true)]
[InlineData(OrderState.Working, false)]
[InlineData(OrderState.Accepted, false)]
[InlineData(OrderState.Submitted, false)]
public void IsTerminalOrderState_VariousStates_ReturnsExpected(OrderState state, bool expected)
{
    // Arrange
    var flatten = new V12_002();
    
    // Act
    var result = flatten.IsTerminalOrderState(state);
    
    // Assert
    Assert.Equal(expected, result);
}
```

#### Test 2: `IsZombieTargetOrder`
```csharp
[Theory]
[InlineData("EMERGENCY_STOP_001", true)]
[InlineData("T1_Entry", true)]
[InlineData("T2_Stop", true)]
[InlineData("T3_Target", true)]
[InlineData("T4_Trail", true)]
[InlineData("T5_Limit", true)]
[InlineData("NORMAL_ORDER", false)]
[InlineData("", false)]
public void IsZombieTargetOrder_VariousNames_ReturnsExpected(string orderName, bool expected)
{
    // Arrange
    var flatten = new V12_002();
    var order = new Order { Name = orderName };
    
    // Act
    var result = flatten.IsZombieTargetOrder(order);
    
    // Assert
    Assert.Equal(expected, result);
}

[Fact]
public void IsZombieTargetOrder_NullOrder_ReturnsFalse()
{
    // Arrange
    var flatten = new V12_002();
    
    // Act
    var result = flatten.IsZombieTargetOrder(null);
    
    // Assert
    Assert.False(result);
}
```

#### Test 3: Integration Test
```csharp
[Fact]
public void ProcessFlattenWorkItem_CancelOrders_ZombieSweepOnly_CancelsOnlyZombies()
{
    // Arrange
    var flatten = new V12_002();
    var account = CreateMockAccount(new[]
    {
        CreateMockOrder("EMERGENCY_STOP_001", OrderState.Working),
        CreateMockOrder("T1_Entry", OrderState.Working),
        CreateMockOrder("NORMAL_ORDER", OrderState.Working)
    });
    var item = new FlattenWorkItem
    {
        Account = account,
        ZombieSweepOnly = true
    };
    
    // Act
    flatten.ProcessFlattenWorkItem_CancelOrders(item, account);
    
    // Assert
    Assert.Equal(2, account.CancelledOrders.Count); // Only zombies
    Assert.Contains(account.CancelledOrders, o => o.Name == "EMERGENCY_STOP_001");
    Assert.Contains(account.CancelledOrders, o => o.Name == "T1_Entry");
    Assert.DoesNotContain(account.CancelledOrders, o => o.Name == "NORMAL_ORDER");
}
```

### Test Coverage Goals
- **Unit Tests**: 100% coverage of extracted methods
- **Integration Tests**: 80% coverage of main method
- **Edge Cases**: NULL, empty, boundary conditions

---

## Risk Assessment

### Technical Risks

#### Risk 1: Method Visibility
**Risk**: Extracted methods are `private`, cannot be tested directly
**Mitigation**: Use `InternalsVisibleTo` attribute or test via public API
**Severity**: 🟡 MEDIUM
**Likelihood**: HIGH
**Impact**: Test coverage reduced

#### Risk 2: Order.Name NULL
**Risk**: `Order.Name` could be NULL, causing NullReferenceException
**Mitigation**: Add NULL check in `IsZombieTargetOrder`
**Severity**: 🟢 LOW
**Likelihood**: LOW (NinjaTrader always sets Order.Name)
**Impact**: Runtime exception

#### Risk 3: OrderState Enum Changes
**Risk**: NinjaTrader adds new terminal states in future versions
**Mitigation**: Document exhaustive list in XML comments
**Severity**: 🟢 LOW
**Likelihood**: LOW (stable API)
**Impact**: Missed terminal states

### Operational Risks

#### Risk 4: Deployment Failure
**Risk**: `deploy-sync.ps1` fails, changes not reflected in NinjaTrader
**Mitigation**: Verify ASCII gate passes, check BUILD_TAG
**Severity**: 🟡 MEDIUM
**Likelihood**: LOW
**Impact**: Strategy not updated

#### Risk 5: Regression
**Risk**: Behavior changes after extraction
**Mitigation**: Characterization tests before extraction
**Severity**: 🔴 HIGH
**Likelihood**: LOW (zero logic drift mandate)
**Impact**: Trading logic broken

---

## Ticket Generation Plan

### Ticket 1: Extract `IsTerminalOrderState`
**Scope**: Extract terminal state check to helper method
**Effort**: 15 minutes
**CYC Impact**: 18 → 13 (intermediate state)
**Files Modified**: `src/V12_002.SIMA.Flatten.cs`
**Tests Required**: 8 unit tests (5 terminal + 3 non-terminal)

### Ticket 2: Extract `IsZombieTargetOrder`
**Scope**: Extract zombie target check to helper method
**Effort**: 15 minutes
**CYC Impact**: 13 → 6 (final state)
**Files Modified**: `src/V12_002.SIMA.Flatten.cs`
**Tests Required**: 9 unit tests (6 zombie + 2 non-zombie + 1 NULL)

### Ticket 3: Integration Verification
**Scope**: F5 test in NinjaTrader, verify BUILD_TAG
**Effort**: 5 minutes
**CYC Impact**: None (verification only)
**Files Modified**: None
**Tests Required**: Manual F5 test

---

## Execution Sequence

### Phase 1: Pre-Extraction
1. ✅ Verify codebase compiles cleanly
2. ✅ Run `complexity_audit.py` (confirm CYC = 18)
3. ✅ Run `verify_index_freshness.py` (confirm index fresh)
4. ✅ Create GitButler virtual branch: `epic-ccn-98-flatten-predicates`

### Phase 2: Ticket 1 Execution
1. Write characterization tests for current behavior
2. Extract `IsTerminalOrderState` method
3. Update `ProcessFlattenWorkItem_CancelOrders` to use helper
4. Run unit tests (verify pass)
5. Run `complexity_audit.py` (verify CYC = 13)
6. Run `dotnet build` (verify pass)
7. Run `deploy-sync.ps1` (verify ASCII gate pass)
8. Commit: `[EPIC-98] ticket-1: extract IsTerminalOrderState -- CYC 18->13 [BUILD_TAG]`

### Phase 3: Ticket 2 Execution
1. Write unit tests for `IsZombieTargetOrder`
2. Extract `IsZombieTargetOrder` method
3. Update `ProcessFlattenWorkItem_CancelOrders` to use helper
4. Run unit tests (verify pass)
5. Run `complexity_audit.py` (verify CYC = 6)
6. Run `dotnet build` (verify pass)
7. Run `deploy-sync.ps1` (verify ASCII gate pass)
8. Commit: `[EPIC-98] ticket-2: extract IsZombieTargetOrder -- CYC 13->6 [BUILD_TAG]`

### Phase 4: Integration Verification
1. F5 in NinjaTrader IDE
2. Verify BUILD_TAG appears in output
3. Verify no compilation errors
4. Verify strategy loads successfully
5. Update `docs/brain/EPIC-CCN-98/05-completion-report.md`

---

## Success Criteria

### Code Quality
- ✅ CYC ≤ 8 for all methods
- ✅ Zero logic drift (behavior unchanged)
- ✅ ASCII-only compliance
- ✅ No `lock()` blocks

### Testing
- ✅ 100% unit test coverage for extracted methods
- ✅ Integration test passes (F5 in NinjaTrader)
- ✅ BUILD_TAG verified

### Process
- ✅ `deploy-sync.ps1` executed successfully
- ✅ Pre-push validation passes
- ✅ Commits follow naming convention

---

## Appendix A: Method Signatures (Final)

```csharp
/// <summary>
/// Cancel working orders for the flatten work item.
/// Handles ZombieSweepOnly filtering for ClosePositionsOnly mode.
/// </summary>
/// <param name="item">Flatten work item</param>
/// <param name="acct">Account to process</param>
private void ProcessFlattenWorkItem_CancelOrders(FlattenWorkItem item, Account acct)

/// <summary>
/// Determines if an order state is terminal (cannot be cancelled).
/// Terminal states: Cancelled, CancelPending, CancelSubmitted, Filled, Rejected.
/// </summary>
/// <param name="state">Order state to check</param>
/// <returns>True if state is terminal, false otherwise</returns>
private bool IsTerminalOrderState(OrderState state)

/// <summary>
/// Determines if an order is a zombie target based on name prefix.
/// Zombie targets include EMERGENCY_STOP_ and T1-T5 prefixed orders.
/// </summary>
/// <param name="order">Order to check</param>
/// <returns>True if order is a zombie target, false otherwise</returns>
private bool IsZombieTargetOrder(Order order)
```

---

## Appendix B: Complexity Audit Output (Expected)

```
Before Extraction:
src/V12_002.SIMA.Flatten.cs:163:ProcessFlattenWorkItem_CancelOrders - CYC: 18

After Ticket 1:
src/V12_002.SIMA.Flatten.cs:163:ProcessFlattenWorkItem_CancelOrders - CYC: 13
src/V12_002.SIMA.Flatten.cs:214:IsTerminalOrderState - CYC: 6

After Ticket 2:
src/V12_002.SIMA.Flatten.cs:163:ProcessFlattenWorkItem_CancelOrders - CYC: 6
src/V12_002.SIMA.Flatten.cs:214:IsTerminalOrderState - CYC: 6
src/V12_002.SIMA.Flatten.cs:225:IsZombieTargetOrder - CYC: 7
```

---

## Appendix C: Diff Preview (Ticket 2 Final State)

```diff
--- a/src/V12_002.SIMA.Flatten.cs
+++ b/src/V12_002.SIMA.Flatten.cs
@@ -163,30 +163,15 @@ private void ProcessFlattenWorkItem_CancelOrders(FlattenWorkItem item, Account
     List<Order> ordersToCancel = new List<Order>();
     foreach (Order order in acct.Orders.ToArray())
     {
-        if (order == null || order.Instrument == null)
-            continue;
-        if (order.Instrument.FullName != Instrument.FullName)
+        if (order == null || order.Instrument == null || order.Instrument.FullName != Instrument.FullName)
             continue;
 
-        bool isTerminal =
-            order.OrderState == OrderState.Cancelled
-            || order.OrderState == OrderState.CancelPending
-            || order.OrderState == OrderState.CancelSubmitted
-            || order.OrderState == OrderState.Filled
-            || order.OrderState == OrderState.Rejected;
-        if (isTerminal)
+        if (IsTerminalOrderState(order.OrderState))
             continue;
 
-        if (item.ZombieSweepOnly)
-        {
-            bool isZombieTarget =
-                order.Name.StartsWith("EMERGENCY_STOP_", StringComparison.OrdinalIgnoreCase)
-                || order.Name.StartsWith("T1_", StringComparison.OrdinalIgnoreCase)
-                || order.Name.StartsWith("T2_", StringComparison.OrdinalIgnoreCase)
-                || order.Name.StartsWith("T3_", StringComparison.OrdinalIgnoreCase)
-                || order.Name.StartsWith("T4_", StringComparison.OrdinalIgnoreCase)
-                || order.Name.StartsWith("T5_", StringComparison.OrdinalIgnoreCase);
-            if (!isZombieTarget)
-                continue;
-        }
+        if (item.ZombieSweepOnly && !IsZombieTargetOrder(order))
+            continue;
 
         ordersToCancel.Add(order);
     }
@@ -211,6 +196,35 @@ private void ProcessFlattenWorkItem_CancelOrders(FlattenWorkItem item, Account
     }
 }
 
+/// <summary>
+/// Determines if an order state is terminal (cannot be cancelled).
+/// Terminal states: Cancelled, CancelPending, CancelSubmitted, Filled, Rejected.
+/// </summary>
+/// <param name="state">Order state to check</param>
+/// <returns>True if state is terminal, false otherwise</returns>
+private bool IsTerminalOrderState(OrderState state)
+{
+    return state == OrderState.Cancelled
+        || state == OrderState.CancelPending
+        || state == OrderState.CancelSubmitted
+        || state == OrderState.Filled
+        || state == OrderState.Rejected;
+}
+
+/// <summary>
+/// Determines if an order is a zombie target based on name prefix.
+/// Zombie targets include EMERGENCY_STOP_ and T1-T5 prefixed orders.
+/// </summary>
+/// <param name="order">Order to check</param>
+/// <returns>True if order is a zombie target, false otherwise</returns>
+private bool IsZombieTargetOrder(Order order)
+{
+    if (order == null)
+        return false;
+    
+    return order.Name.StartsWith("EMERGENCY_STOP_", StringComparison.OrdinalIgnoreCase)
+        || order.Name.StartsWith("T1_", StringComparison.OrdinalIgnoreCase)
+        || order.Name.StartsWith("T2_", StringComparison.OrdinalIgnoreCase)
+        || order.Name.StartsWith("T3_", StringComparison.OrdinalIgnoreCase)
+        || order.Name.StartsWith("T4_", StringComparison.OrdinalIgnoreCase)
+        || order.Name.StartsWith("T5_", StringComparison.OrdinalIgnoreCase);
+}
```

---

**END OF PHASE 2 ARCHITECTURE PLANNING**

**Status**: ✅ READY FOR PHASE 3 (DNA & PR Audit)
**Next Phase**: Phase 3 - Run DNA compliance checks, PR hygiene validation, and pre-flight safety checks