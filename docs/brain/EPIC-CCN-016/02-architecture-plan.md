# Phase 2: Architecture Planning - EPIC-CCN-016

## Executive Summary

**Target Method**: `TryHandleFleet_CancelAll`
**Current Complexity**: CYC 19 (27% over Jane Street strict threshold)
**Target Complexity**: CYC ≤5 (main method after extraction)
**Extraction Strategy**: 3 helper methods with single responsibilities
**Lock-Free Compliance**: ✅ Verified (no locks, uses NinjaTrader thread-safe APIs)

---

## 1. Extraction Strategy

### Current Method Analysis

**Method**: `TryHandleFleet_CancelAll(string action, string cmdId)`
**File**: `src/V12_002.UI.IPC.Commands.Fleet.cs`
**Current CYC**: 19
**LOC**: 41

**Complexity Breakdown**:
- Base complexity: 1
- Action validation: +1
- Duplicate guard: +1
- EnableSIMA branch: +1
- Order state validation (5 OR conditions): +5
- Order name filtering (7 OR conditions): +7
- Loop and nested conditions: +3
- **Total**: 19

### Extraction Plan

**Goal**: Reduce main method to CYC ≤5 by extracting 3 helper methods

**Helper Methods**:
1. **IsOrderCancellable** - Validates order state and instrument match (CYC reduction: ~5)
2. **IsProtectedOrderName** - Checks protected order name prefixes (CYC reduction: ~6)
3. **CancelAll_ProcessNonSIMAAccount** - Encapsulates non-SIMA cancellation logic (CYC reduction: ~8)

**Expected Result**:
- Main method: CYC 5 (1 base + 1 action + 1 guard + 1 branch + 1 call)
- Helper 1: CYC 6 (order state validation)
- Helper 2: CYC 7 (name prefix checks)
- Helper 3: CYC 3 (loop with helper calls)

---

## 2. Method Signatures

### Original Method (Current)

```csharp
private bool TryHandleFleet_CancelAll(string action, string cmdId)
```

**Parameters**:
- `action` (string): IPC command action (expected: "CANCEL_ALL")
- `cmdId` (string): Command ID for duplicate detection

**Returns**: `bool` - true if command was handled, false otherwise

**Access Modifier**: `private`

### Proposed Helper Method 1: IsOrderCancellable

```csharp
private bool IsOrderCancellable(Order order)
```

**Purpose**: Validates if an order is eligible for cancellation based on state and instrument match

**Parameters**:
- `order` (Order): The order to validate

**Returns**: `bool` - true if order can be cancelled, false otherwise

**Logic**:
- Checks order is not null
- Validates instrument matches current instrument
- Validates order state is cancellable (Working, Accepted, Submitted, ChangePending, ChangeSubmitted)

**Complexity**: CYC 6 (1 base + 1 null check + 1 instrument check + 5 OR conditions for state)

**Access Modifier**: `private`

### Proposed Helper Method 2: IsProtectedOrderName

```csharp
private bool IsProtectedOrderName(string orderName)
```

**Purpose**: Determines if an order name indicates a protected order (stop/target) that should not be cancelled

**Parameters**:
- `orderName` (string): The order name to check

**Returns**: `bool` - true if order is protected (should skip cancellation), false otherwise

**Logic**:
- Checks if name starts with protected prefixes: "Stop_", "S_", "T1_", "T2_", "T3_", "T4_", "T5_"

**Complexity**: CYC 7 (1 base + 7 OR conditions for prefix checks)

**Access Modifier**: `private`

### Proposed Helper Method 3: CancelAll_ProcessNonSIMAAccount

```csharp
private int CancelAll_ProcessNonSIMAAccount()
```

**Purpose**: Processes order cancellation for non-SIMA mode (legacy single-account mode)

**Parameters**: None (accesses Account.Orders from instance state)

**Returns**: `int` - count of cancelled orders

**Logic**:
- Iterates through Account.Orders
- Uses IsOrderCancellable() to filter eligible orders
- Uses IsProtectedOrderName() to skip protected orders
- Calls CancelOrderOnAccount() for each eligible order
- Returns total cancelled count

**Complexity**: CYC 3 (1 base + 1 loop + 1 conditional call)

**Access Modifier**: `private`

---

## 3. Call Graph

### Method Call Hierarchy

```
TryHandleFleet_CancelAll (CYC 5)
├─ MetadataGuardDuplicate (existing, not modified)
├─ CancelAll_ProcessMasterAccount (existing, SIMA path)
├─ CancelAll_ProcessFleetAccounts (existing, SIMA path)
└─ CancelAll_ProcessNonSIMAAccount (NEW, CYC 3)
   ├─ IsOrderCancellable (NEW, CYC 6)
   └─ IsProtectedOrderName (NEW, CYC 7)
```

### Data Flow

**Main Method Flow**:
1. Validate action == "CANCEL_ALL"
2. Check duplicate via MetadataGuardDuplicate
3. Branch on EnableSIMA flag
4. SIMA path: Call existing helpers
5. Non-SIMA path: Call new CancelAll_ProcessNonSIMAAccount
6. Print summary and return true

**Helper Method Flow**:
- IsOrderCancellable: order → validation checks → bool
- IsProtectedOrderName: orderName → prefix checks → bool
- CancelAll_ProcessNonSIMAAccount: Account.Orders → filter → cancel → count

### Shared State

**Read-Only Access**:
- `Account.Orders` - Iterated in CancelAll_ProcessNonSIMAAccount
- `Instrument.FullName` - Used in IsOrderCancellable for instrument matching
- `EnableSIMA` - Branch condition in main method

**No Shared Mutable State**: All helper methods are stateless and operate on parameters only

**Thread Safety**: Relies on NinjaTrader built-in thread-safe order management APIs

---

## 4. Lock-Free Validation

### Audit Results

✅ **No lock() statements** - Verified in method body and proposed helpers
✅ **Uses FSM/Actor Enqueue pattern** - SIMA path uses existing Enqueue mechanism
✅ **Atomic primitives only** - No explicit synchronization needed
✅ **NinjaTrader API compliance** - CancelOrderOnAccount is thread-safe by design

### Concurrency Analysis

**SIMA Path**:
- Delegates to existing helpers (already verified lock-free)
- No additional locking required

**Non-SIMA Path**:
- Iterates Account.Orders collection (read-only)
- Calls CancelOrderOnAccount (thread-safe NinjaTrader API)
- No race conditions: each cancellation is atomic
- No shared mutable state between iterations

**Helper Methods**:
- IsOrderCancellable: Pure function, no side effects
- IsProtectedOrderName: Pure function, no side effects
- CancelAll_ProcessNonSIMAAccount: Only mutates local counter

### V12 DNA Compliance

✅ **Lock-Free Actor Pattern** - No locks introduced
✅ **Correctness by Construction** - Helper methods make invalid states unrepresentable
✅ **ASCII-Only** - No Unicode in string literals
✅ **Cyclomatic Complexity ≤8** - All methods meet threshold after extraction

---

## 5. Jane Street Compliance

### Cognitive Simplicity

**Before**: CYC 19 - High cognitive load, nested conditions
**After**: Main CYC 5, helpers CYC 3-7 - Simple, focused logic

**Jane Street Principle**: "Keep functions simple enough to reason about under microsecond-latency constraints"
**Alignment**: ✅ All methods ≤8, enabling rapid cognitive processing

### Testing Strategy

**Characterization Tests** (BEFORE extraction):
- Test current SIMA behavior
- Test current non-SIMA behavior

**Unit Tests** (AFTER extraction):
- IsOrderCancellable: Test all order states
- IsProtectedOrderName: Test all prefixes
- CancelAll_ProcessNonSIMAAccount: Test cancellation logic

**Jane Street Principle**: "Small methods enable exhaustive test coverage"
**Alignment**: ✅ Each helper testable independently

### Performance

**No Regression**: Helper calls inlined by JIT, same execution path
**Jane Street Principle**: "Zero-overhead abstractions"
**Alignment**: ✅ Extraction is zero-cost at runtime

---

## 6. Implementation Sequence

1. Write characterization tests
2. Extract IsOrderCancellable
3. Extract IsProtectedOrderName
4. Extract CancelAll_ProcessNonSIMAAccount
5. Verify with deploy-sync.ps1 and F5

---

## 7. Success Criteria

✅ **Complexity Reduction**: Main method CYC ≤5, all helpers CYC ≤8
✅ **Lock-Free Compliance**: No locks, thread-safe APIs
✅ **Jane Street Alignment**: Cognitive simplicity, testability
✅ **Behavioral Preservation**: Tests pass after extraction
✅ **Build Success**: dotnet build succeeds
✅ **Integration Success**: F5 shows BUILD_TAG

---

**Created**: 2026-06-16
**Epic**: EPIC-CCN-016
**Phase**: 2 (Architecture Planning)
**Decision**: APPROVED - Ready for Phase 3
