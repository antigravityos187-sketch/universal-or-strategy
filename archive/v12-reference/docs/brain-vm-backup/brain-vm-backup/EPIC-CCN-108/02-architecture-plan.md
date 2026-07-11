# Phase 2: Architecture Plan - EPIC-CCN-108

## Epic Context
- **Epic ID**: EPIC-CCN-108
- **Phase**: 2 (Architecture Planning)
- **Date**: 2026-06-13
- **Target Method**: `SweepBrokerOrders`
- **File**: `src/V12_002.SIMA.Lifecycle.cs`
- **Lines**: 1267-1346 (main method body)

## Executive Summary

The `SweepBrokerOrders` method is responsible for scanning broker order lists and canceling V12-managed GTC orders during SIMA shutdown or strategy termination. The method has **already been partially refactored** with two helper methods extracted in previous builds:

1. **IsV12OrderPrefix** (lines 1352-1360) - CCN ~2
2. **ShouldProtectBracketOrder** (lines 1368-1388) - CCN ~3

**Current State**: The main method body (lines 1268-1346) has an estimated CCN of **15-18**, which is at or slightly above the V12 threshold of 15.

**Refactoring Strategy**: Extract 2-3 additional helper methods to reduce main method CCN to **≤12** (safety margin below threshold).

---

## Method Signature Analysis

### Current Signature (UNCHANGED)
```csharp
private int SweepBrokerOrders(bool force)
```

**Parameters**:
- `force` (bool): Controls cancellation scope
  - `true`: Cancel all V12 orders (strategy terminate)
  - `false`: Cancel only entry orders, protect brackets (SIMA disable)

**Return Type**: `int` - Count of broker orders cancelled

**Visibility**: `private` - Internal lifecycle method

**Caller**: `CancelAllV12GtcOrders(bool force)` (line 1218)

### Post-Refactoring Signature (UNCHANGED)
```csharp
private int SweepBrokerOrders(bool force)
```

**Guarantee**: Method signature remains **100% unchanged**. All extractions are internal helpers.

---

## Current Implementation Analysis

### Method Structure (Lines 1268-1346)

```
SweepBrokerOrders(bool force)
├── Initialize brokerCancels counter
├── Build v12Prefixes array (conditional on force)
├── Outer loop: foreach Account in Account.All
│   ├── Guard: Skip non-fleet accounts
│   ├── Try-catch wrapper (account-level)
│   │   ├── Inner loop: foreach Order in acct.Orders.ToArray()
│   │   │   ├── Guard: Skip wrong instrument
│   │   │   ├── Guard: Skip non-working order states
│   │   │   ├── Extract order name
│   │   │   ├── Guard: Skip non-V12 prefixes (calls IsV12OrderPrefix)
│   │   │   ├── Guard: Skip protected brackets (calls ShouldProtectBracketOrder)
│   │   │   └── Try-catch wrapper (order-level)
│   │   │       ├── Cancel order via acct.Cancel()
│   │   │       └── Increment brokerCancels
│   │   └── Catch: Log account iteration failure
│   └── Return brokerCancels
```

### Complexity Breakdown

| Code Section | CCN Contribution | Notes |
|--------------|------------------|-------|
| Method entry | +1 | Base complexity |
| `force` ternary (v12Prefixes) | +1 | Conditional array initialization |
| Outer foreach (accounts) | +1 | Loop |
| `!IsFleetAccount` guard | +1 | Conditional |
| Try-catch (account-level) | +1 | Exception path |
| Inner foreach (orders) | +1 | Nested loop |
| Instrument guard | +1 | Conditional |
| OrderState guard (5 conditions) | +5 | Complex multi-condition |
| `!IsV12OrderPrefix` guard | +1 | Conditional (delegates to helper) |
| `ShouldProtectBracketOrder` guard | +1 | Conditional (delegates to helper) |
| Try-catch (order-level) | +1 | Exception path |
| Catch block (account) | +1 | Exception handler |
| Catch block (order) | +1 | Exception handler |
| **TOTAL** | **~18** | **Above threshold** |

### Already Extracted Helpers

#### 1. IsV12OrderPrefix (Lines 1352-1360)
```csharp
private bool IsV12OrderPrefix(string orderName, string[] v12Prefixes)
{
    for (int pi = 0; pi < v12Prefixes.Length; pi++)
    {
        if (orderName.StartsWith(v12Prefixes[pi], StringComparison.OrdinalIgnoreCase))
            return true;
    }
    return false;
}
```
- **CCN**: ~2 (loop + conditional)
- **Purpose**: Check if order name matches any V12 prefix
- **Status**: ✅ Already extracted (Build 984+)

#### 2. ShouldProtectBracketOrder (Lines 1368-1388)
```csharp
private bool ShouldProtectBracketOrder(string orderName, bool force, string accountName)
{
    if (force)
        return false;

    bool isBracketOrder =
        orderName.StartsWith("Stop_", StringComparison.OrdinalIgnoreCase)
        || orderName.StartsWith("S_", StringComparison.OrdinalIgnoreCase)
        || orderName.StartsWith("T1_", StringComparison.OrdinalIgnoreCase)
        || orderName.StartsWith("T2_", StringComparison.OrdinalIgnoreCase)
        || orderName.StartsWith("T3_", StringComparison.OrdinalIgnoreCase)
        || orderName.StartsWith("T4_", StringComparison.OrdinalIgnoreCase)
        || orderName.StartsWith("T5_", StringComparison.OrdinalIgnoreCase)
        || orderName.StartsWith("Target_", StringComparison.OrdinalIgnoreCase);

    if (isBracketOrder)
    {
        Print(string.Format("[FIX-FF] Protected bracket order from sweep: {0} on {1}", orderName, accountName));
        return true;
    }
    return false;
}
```
- **CCN**: ~3 (force guard + isBracketOrder conditional + print conditional)
- **Purpose**: Determine if bracket order should be protected from cancellation
- **Status**: ✅ Already extracted (Build 990+)

---

## Proposed Extractions

### Extraction Strategy

**Goal**: Reduce main method CCN from ~18 to **≤12** by extracting 2-3 additional helpers.

**Target Reductions**:
1. **Extract order state validation** (-5 CCN): Complex multi-condition guard
2. **Extract order cancellation logic** (-2 CCN): Try-catch + cancel + increment
3. **Optional: Extract order processing loop** (-3 CCN): Inner foreach with guards

### Extraction 1: IsOrderCancellable

**Purpose**: Consolidate the 5-condition OrderState guard into a single helper.

**Signature**:
```csharp
private bool IsOrderCancellable(OrderState state)
```

**Implementation**:
```csharp
/// <summary>
/// Validates whether an order state qualifies for cancellation during sweep.
/// Working, Accepted, Submitted, ChangePending, and ChangeSubmitted states are cancellable.
/// </summary>
/// <param name="state">Order state to validate</param>
/// <returns>True if order should be cancelled</returns>
private bool IsOrderCancellable(OrderState state)
{
    return state == OrderState.Working
        || state == OrderState.Accepted
        || state == OrderState.Submitted
        || state == OrderState.ChangePending
        || state == OrderState.ChangeSubmitted;
}
```

**CCN**: ~1 (single return with OR chain)

**Extraction Location**: Lines 1389-1402 (after ShouldProtectBracketOrder)

**Call Site Change** (Line 1308-1314):
```csharp
// BEFORE:
if (
    ord.OrderState != OrderState.Working
    && ord.OrderState != OrderState.Accepted
    && ord.OrderState != OrderState.Submitted
    && ord.OrderState != OrderState.ChangePending
    && ord.OrderState != OrderState.ChangeSubmitted
)
    continue;

// AFTER:
if (!IsOrderCancellable(ord.OrderState))
    continue;
```

**CCN Reduction**: -5 (removes 5 conditions from main method)

**Risk**: LOW - Pure validation logic, no side effects

---

### Extraction 2: TryCancelBrokerOrder

**Purpose**: Encapsulate order cancellation with error handling.

**Signature**:
```csharp
private bool TryCancelBrokerOrder(Account account, Order order, ref int cancelCount)
```

**Implementation**:
```csharp
/// <summary>
/// Attempts to cancel a single broker order with error handling.
/// Increments cancelCount on success.
/// </summary>
/// <param name="account">Account owning the order</param>
/// <param name="order">Order to cancel</param>
/// <param name="cancelCount">Reference to cancellation counter (incremented on success)</param>
/// <returns>True if cancellation succeeded, false if exception occurred</returns>
private bool TryCancelBrokerOrder(Account account, Order order, ref int cancelCount)
{
    try
    {
        account.Cancel(new[] { order });
        cancelCount++;
        return true;
    }
    catch (Exception ex)
    {
        if (_diagFleet)
            Print("[FLEET_CATCH] SweepBrokerOrders per-order cancel failed: " + ex.Message);
        return false;
    }
}
```

**CCN**: ~2 (try-catch + conditional print)

**Extraction Location**: Lines 1403-1422 (after IsOrderCancellable)

**Call Site Change** (Lines 1326-1336):
```csharp
// BEFORE:
try
{
    acct.Cancel(new[] { ord });
    brokerCancels++;
}
catch (Exception ex)
{
    if (_diagFleet)
        Print("[FLEET_CATCH] SweepBrokerOrders per-order cancel failed: " + ex.Message);
}

// AFTER:
TryCancelBrokerOrder(acct, ord, ref brokerCancels);
```

**CCN Reduction**: -2 (removes try-catch + increment from main method)

**Risk**: LOW - Encapsulates existing error handling, no behavioral change

---

### Extraction 3 (Optional): ProcessAccountOrders

**Purpose**: Extract inner order processing loop to reduce nesting depth.

**Signature**:
```csharp
private int ProcessAccountOrders(Account account, string[] v12Prefixes, bool force)
```

**Implementation**:
```csharp
/// <summary>
/// Processes all orders for a single account, cancelling V12-managed orders.
/// </summary>
/// <param name="account">Account to process</param>
/// <param name="v12Prefixes">Array of V12 order name prefixes</param>
/// <param name="force">If true, cancel all V12 orders; if false, protect brackets</param>
/// <returns>Count of orders cancelled for this account</returns>
private int ProcessAccountOrders(Account account, string[] v12Prefixes, bool force)
{
    int accountCancels = 0;
    
    foreach (Order ord in account.Orders.ToArray())
    {
        if (ord.Instrument?.FullName != Instrument?.FullName)
            continue;
        
        if (!IsOrderCancellable(ord.OrderState))
            continue;

        string ordName = ord.Name ?? string.Empty;
        if (!IsV12OrderPrefix(ordName, v12Prefixes))
            continue;

        if (ShouldProtectBracketOrder(ordName, force, account.Name))
            continue;

        TryCancelBrokerOrder(account, ord, ref accountCancels);
    }
    
    return accountCancels;
}
```

**CCN**: ~6 (loop + 4 guards + method call)

**Extraction Location**: Lines 1423-1455 (after TryCancelBrokerOrder)

**Call Site Change** (Lines 1303-1337):
```csharp
// BEFORE:
try
{
    foreach (Order ord in acct.Orders.ToArray())
    {
        // ... 30+ lines of order processing logic ...
    }
}
catch (Exception ex)
{
    if (_diagFleet)
        Print("[FLEET_CATCH] SweepBrokerOrders account iteration failed: " + ex.Message);
}

// AFTER:
try
{
    brokerCancels += ProcessAccountOrders(acct, v12Prefixes, force);
}
catch (Exception ex)
{
    if (_diagFleet)
        Print("[FLEET_CATCH] SweepBrokerOrders account iteration failed: " + ex.Message);
}
```

**CCN Reduction**: -6 (removes inner loop + guards from main method)

**Risk**: MEDIUM - Larger extraction, but well-isolated logic

**Decision**: **RECOMMENDED** - Significantly reduces main method complexity and improves readability

---

## Post-Refactoring Complexity

### Main Method (SweepBrokerOrders) - Target CCN ≤12

```
SweepBrokerOrders(bool force)
├── Initialize brokerCancels counter
├── Build v12Prefixes array (conditional on force)          [+1 CCN]
├── Outer loop: foreach Account in Account.All              [+1 CCN]
│   ├── Guard: Skip non-fleet accounts                      [+1 CCN]
│   ├── Try-catch wrapper (account-level)                   [+1 CCN]
│   │   ├── Call ProcessAccountOrders()                     [+0 CCN]
│   │   └── Accumulate brokerCancels
│   └── Catch: Log account iteration failure                [+1 CCN]
└── Return brokerCancels

TOTAL CCN: ~6 (well below threshold of 15)
```

### Extracted Helper Methods

| Method | CCN | Lines | Purpose |
|--------|-----|-------|---------|
| **IsV12OrderPrefix** | ~2 | 8 | Check order name prefix (existing) |
| **ShouldProtectBracketOrder** | ~3 | 20 | Protect bracket orders (existing) |
| **IsOrderCancellable** | ~1 | 14 | Validate order state (new) |
| **TryCancelBrokerOrder** | ~2 | 20 | Cancel order with error handling (new) |
| **ProcessAccountOrders** | ~6 | 33 | Process all orders for account (new) |
| **TOTAL HELPERS** | **~14** | **~95** | **5 methods** |

### Complexity Distribution

- **Main Method**: CCN ~6 (orchestration only)
- **Helper Methods**: CCN 1-6 each (all ≤8, Jane Street aligned)
- **Total System CCN**: ~20 (main + helpers)
- **Original CCN**: ~18 (before additional extractions)

**Net Change**: +2 CCN total (acceptable overhead for improved maintainability)

---

## Call Graph Analysis

### Current Call Graph

```
CancelAllV12GtcOrders(bool force)
├── SweepTrackedOrders(bool force)
└── SweepBrokerOrders(bool force)
    ├── IsV12OrderPrefix(string, string[])
    └── ShouldProtectBracketOrder(string, bool, string)
```

### Post-Refactoring Call Graph

```
CancelAllV12GtcOrders(bool force)
├── SweepTrackedOrders(bool force)
└── SweepBrokerOrders(bool force)
    └── ProcessAccountOrders(Account, string[], bool)
        ├── IsOrderCancellable(OrderState)
        ├── IsV12OrderPrefix(string, string[])
        ├── ShouldProtectBracketOrder(string, bool, string)
        └── TryCancelBrokerOrder(Account, Order, ref int)
```

### Dependency Analysis

**External Dependencies**:
- `Account.All` (NinjaTrader API)
- `Account.Orders` (NinjaTrader API)
- `Account.Cancel()` (NinjaTrader API)
- `IsFleetAccount()` (V12 helper)
- `Print()` (NinjaTrader logging)
- `_diagFleet` (V12 diagnostic flag)

**Internal Dependencies**:
- All extracted methods are `private` and local to `V12_002.SIMA.Lifecycle.cs`
- No cross-file dependencies introduced
- No changes to public/protected API surface

**Coupling**: LOW - All extractions are internal helpers with no external visibility

---

## Extraction Sequence

### Phase 1: Extract IsOrderCancellable (Lowest Risk)
1. Create `IsOrderCancellable(OrderState)` method at line 1389
2. Replace 5-condition guard at lines 1308-1314 with single call
3. Run unit tests
4. Verify CCN reduction: ~18 → ~13

### Phase 2: Extract TryCancelBrokerOrder (Low Risk)
1. Create `TryCancelBrokerOrder(Account, Order, ref int)` at line 1403
2. Replace try-catch block at lines 1326-1336 with single call
3. Run unit tests
4. Verify CCN reduction: ~13 → ~11

### Phase 3: Extract ProcessAccountOrders (Medium Risk)
1. Create `ProcessAccountOrders(Account, string[], bool)` at line 1423
2. Replace inner foreach loop at lines 1303-1337 with single call
3. Run unit tests
4. Verify CCN reduction: ~11 → ~6

### Phase 4: Verification
1. Run full test suite
2. Verify complexity: `lizard src/V12_002.SIMA.Lifecycle.cs`
3. Confirm CCN ≤12 for main method
4. Confirm all helpers CCN ≤8

---

## Jane Street Compliance

### Cognitive Simplicity ✅
- **Main Method**: Reduced to orchestration-only logic (CCN ~6)
- **Helper Methods**: All ≤8 CCN (Jane Street threshold)
- **Readability**: Clear separation of concerns

### Correctness by Construction ✅
- **No Behavioral Changes**: All extractions preserve exact semantics
- **Type Safety**: Strong typing maintained throughout
- **Error Handling**: Exception paths preserved in extracted methods

### Testability ✅
- **Unit Testable**: Each helper can be tested independently
- **Isolation**: Pure validation methods (IsOrderCancellable, IsV12OrderPrefix)
- **Mocking**: Account/Order dependencies can be mocked

### Composability ✅
- **Single Responsibility**: Each helper has one clear purpose
- **Reusability**: Helpers can be reused in future refactorings
- **Layering**: Clear orchestration → processing → validation hierarchy

---

## Risk Assessment

### Overall Risk: LOW-MEDIUM

### Risk Factors

#### 1. Behavioral Equivalence Risk: LOW
- **Mitigation**: All extractions are pure refactorings (no logic changes)
- **Validation**: Comprehensive integration tests required
- **Rollback**: Git atomic commits per extraction

#### 2. Performance Risk: NEGLIGIBLE
- **Analysis**: Method call overhead is <1ns on modern CLR
- **Context**: Order sweeps are infrequent (shutdown/disable only)
- **Validation**: No performance tests required

#### 3. Lock-Free Risk: NONE
- **Analysis**: No synchronization primitives introduced
- **Validation**: Code review for `lock`, `Monitor`, `Mutex` keywords

#### 4. Testing Gap Risk: MEDIUM
- **Current State**: No existing unit tests for SweepBrokerOrders
- **Mitigation**: Create comprehensive test suite before refactoring
- **Validation**: Achieve 100% line coverage for extracted methods

#### 5. Integration Risk: LOW
- **Analysis**: Method is called only from CancelAllV12GtcOrders
- **Mitigation**: Integration test covers full call chain
- **Validation**: Verify order cancellation in test environment

### Risk Mitigation Strategy

#### Pre-Refactoring (Phase 0)
1. Create integration test for CancelAllV12GtcOrders
2. Document current behavior with test scenarios
3. Establish baseline metrics (CCN, LOC)

#### During Refactoring (Phases 1-3)
1. Extract one method at a time
2. Run full test suite after each extraction
3. Verify CCN reduction after each step
4. Commit after each successful extraction

#### Post-Refactoring (Phase 4)
1. Run full regression test suite
2. Verify complexity targets met
3. Code review for V12 DNA compliance
4. Performance smoke test (optional)

---

## Implementation Plan

### Step 1: Pre-Refactoring Setup
- **Duration**: 1 hour
- **Tasks**:
  1. Create feature branch: `epic-ccn-108-sweep-broker-orders`
  2. Run baseline complexity analysis
  3. Create integration test suite
  4. Document current behavior

### Step 2: Extract IsOrderCancellable
- **Duration**: 30 minutes
- **Tasks**:
  1. Create method at line 1389
  2. Replace call site at lines 1308-1314
  3. Run tests
  4. Verify CCN reduction
  5. Commit: "EPIC-CCN-108: Extract IsOrderCancellable"

### Step 3: Extract TryCancelBrokerOrder
- **Duration**: 30 minutes
- **Tasks**:
  1. Create method at line 1403
  2. Replace call site at lines 1326-1336
  3. Run tests
  4. Verify CCN reduction
  5. Commit: "EPIC-CCN-108: Extract TryCancelBrokerOrder"

### Step 4: Extract ProcessAccountOrders
- **Duration**: 45 minutes
- **Tasks**:
  1. Create method at line 1423
  2. Replace call site at lines 1303-1337
  3. Run tests
  4. Verify CCN reduction
  5. Commit: "EPIC-CCN-108: Extract ProcessAccountOrders"

### Step 5: Verification & Documentation
- **Duration**: 30 minutes
- **Tasks**:
  1. Run full test suite
  2. Run complexity analysis
  3. Update XML documentation
  4. Code review
  5. Merge to main

### Total Estimated Time: 3.5 hours

---

## Success Criteria

### Primary Criteria ✅
1. **Complexity Target**: SweepBrokerOrders CCN ≤12 (target: ~6)
2. **Helper Methods**: All extracted methods CCN ≤8
3. **Behavioral Equivalence**: All existing tests pass unchanged
4. **No Breaking Changes**: Method signature unchanged

### Secondary Criteria ✅
5. **Test Coverage**: 100% line coverage for extracted methods
6. **Documentation**: XML comments for all extracted methods
7. **Code Review**: Passes V12 DNA compliance check
8. **No Regressions**: Zero new bugs introduced

### Verification Checklist
- [ ] Run complexity analysis: `lizard src/V12_002.SIMA.Lifecycle.cs`
- [ ] Verify CCN ≤12 for SweepBrokerOrders
- [ ] Verify CCN ≤8 for each extracted method
- [ ] Run all existing tests: `dotnet test`
- [ ] Run new unit tests for extracted methods
- [ ] Verify lock-free guarantees (no `lock` keyword)
- [ ] Check ASCII-only compliance
- [ ] Review XML documentation completeness

---

## Rollback Plan

### Git Strategy
- **Branch**: `epic-ccn-108-sweep-broker-orders`
- **Commits**: Atomic per extraction (3 commits)
- **Rollback Trigger**: Any test failure or CCN increase

### Recovery Steps
1. Identify failing extraction (Step 2, 3, or 4)
2. Revert to last known good commit
3. Analyze failure root cause
4. Adjust extraction strategy
5. Retry with modified approach

---

## Metadata

- **Document Version**: 1.0
- **Phase**: 2 (Architecture Planning)
- **Status**: COMPLETED
- **Author**: V12 Phase 2 Architecture Planner
- **Date**: 2026-06-13
- **Epic**: EPIC-CCN-108
- **Target Method**: SweepBrokerOrders
- **Target File**: src/V12_002.SIMA.Lifecycle.cs
- **Estimated Effort**: 3.5 hours
- **Risk Level**: LOW-MEDIUM
- **Complexity Reduction**: ~18 → ~6 CCN (67% reduction)