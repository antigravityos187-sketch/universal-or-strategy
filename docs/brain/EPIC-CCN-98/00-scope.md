# Phase 1: Scope Definition - EPIC-CCN-98

## Epic Metadata
- **Epic ID**: EPIC-CCN-98
- **Target Method**: `ProcessFlattenWorkItem_CancelOrders`
- **File**: `src/V12_002.SIMA.Flatten.cs`
- **Current CYC**: 18
- **Target CYC**: ≤8 per method
- **Reduction**: 56%

## Method Analysis

### Current Structure
```csharp
private void ProcessFlattenWorkItem_CancelOrders(FlattenWorkItem item, Account acct)
{
    List<Order> ordersToCancel = new List<Order>();
    foreach (Order order in acct.Orders.ToArray())
    {
        // 1. Null/instrument validation (CYC +2)
        if (order == null || order.Instrument == null)
            continue;
        if (order.Instrument.FullName != Instrument.FullName)
            continue;

        // 2. Terminal state check (CYC +5)
        bool isTerminal =
            order.OrderState == OrderState.Cancelled
            || order.OrderState == OrderState.CancelPending
            || order.OrderState == OrderState.CancelSubmitted
            || order.OrderState == OrderState.Filled
            || order.OrderState == OrderState.Rejected;
        if (isTerminal)
            continue;

        // 3. ZombieSweepOnly filtering (CYC +7)
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

    // 4. Batch cancel (CYC +1)
    if (ordersToCancel.Count > 0)
    {
        acct.Cancel(ordersToCancel);
        Print(...);
    }
}
```

### Complexity Breakdown
| Section | CYC Contribution | Description |
|---------|------------------|-------------|
| Null/instrument checks | +2 | Early validation |
| Terminal state check | +5 | 5 OR conditions |
| ZombieSweepOnly branch | +1 | Outer if |
| Zombie target detection | +6 | 6 OR conditions (StartsWith) |
| Batch cancel check | +1 | Count > 0 |
| **Total** | **18** | **Current complexity** |

## Extraction Strategy

### Target Extractions (3 methods)

#### 1. `IsTerminalOrderState(Order order)` → CYC 5
**Purpose**: Consolidate terminal state detection
**Signature**: `private bool IsTerminalOrderState(Order order)`
**Logic**: Return true if order is in any terminal state
**Benefit**: Reduces main method CYC by 5

#### 2. `IsZombieTargetOrder(Order order)` → CYC 6
**Purpose**: Consolidate zombie target detection
**Signature**: `private bool IsZombieTargetOrder(Order order)`
**Logic**: Return true if order name matches any zombie prefix
**Benefit**: Reduces main method CYC by 6

#### 3. `ShouldCancelOrder(Order order, FlattenWorkItem item)` → CYC 4
**Purpose**: Consolidate all filtering logic
**Signature**: `private bool ShouldCancelOrder(Order order, FlattenWorkItem item)`
**Logic**: Combine validation, terminal check, and zombie filtering
**Benefit**: Single decision point, reduces main method CYC by 11

### Post-Extraction Structure
```csharp
// Main method: CYC 3 (foreach + if count > 0 + method call)
private void ProcessFlattenWorkItem_CancelOrders(FlattenWorkItem item, Account acct)
{
    List<Order> ordersToCancel = new List<Order>();
    foreach (Order order in acct.Orders.ToArray())
    {
        if (ShouldCancelOrder(order, item))
            ordersToCancel.Add(order);
    }

    if (ordersToCancel.Count > 0)
    {
        acct.Cancel(ordersToCancel);
        Print(...);
    }
}

// Helper 1: CYC 5
private bool IsTerminalOrderState(Order order)
{
    return order.OrderState == OrderState.Cancelled
        || order.OrderState == OrderState.CancelPending
        || order.OrderState == OrderState.CancelSubmitted
        || order.OrderState == OrderState.Filled
        || order.OrderState == OrderState.Rejected;
}

// Helper 2: CYC 6
private bool IsZombieTargetOrder(Order order)
{
    return order.Name.StartsWith("EMERGENCY_STOP_", StringComparison.OrdinalIgnoreCase)
        || order.Name.StartsWith("T1_", StringComparison.OrdinalIgnoreCase)
        || order.Name.StartsWith("T2_", StringComparison.OrdinalIgnoreCase)
        || order.Name.StartsWith("T3_", StringComparison.OrdinalIgnoreCase)
        || order.Name.StartsWith("T4_", StringComparison.OrdinalIgnoreCase)
        || order.Name.StartsWith("T5_", StringComparison.OrdinalIgnoreCase);
}

// Helper 3: CYC 4
private bool ShouldCancelOrder(Order order, FlattenWorkItem item)
{
    if (order == null || order.Instrument == null)
        return false;
    if (order.Instrument.FullName != Instrument.FullName)
        return false;
    if (IsTerminalOrderState(order))
        return false;
    if (item.ZombieSweepOnly && !IsZombieTargetOrder(order))
        return false;
    return true;
}
```

### Complexity Verification
| Method | CYC | Status |
|--------|-----|--------|
| `ProcessFlattenWorkItem_CancelOrders` (main) | 3 | ✅ ≤8 |
| `IsTerminalOrderState` | 5 | ✅ ≤8 |
| `IsZombieTargetOrder` | 6 | ✅ ≤8 |
| `ShouldCancelOrder` | 4 | ✅ ≤8 |

## Scope Boundaries

### IN SCOPE
- ✅ Extract terminal state detection
- ✅ Extract zombie target detection
- ✅ Extract combined filtering logic
- ✅ Maintain exact filtering behavior
- ✅ Preserve logging output
- ✅ Zero logic drift

### OUT OF SCOPE
- ❌ Modifying `ProcessFlattenWorkItem_ClosePositions` (separate method)
- ❌ Changing `FlattenWorkItem` structure
- ❌ Altering batch cancel behavior
- ❌ Modifying caller methods (`PumpFlattenOps`, etc.)
- ❌ Changing zombie prefix list (business logic)

## Risk Assessment

### Blast Radius: LOW
- **Callers**: 1 (`PumpFlattenOps` - same file)
- **Dependencies**: None (pure filtering logic)
- **State Mutations**: None (read-only order inspection)
- **Cross-File Impact**: None

### Risk Factors
| Factor | Level | Mitigation |
|--------|-------|------------|
| Logic Drift | LOW | Pure extraction, no optimization |
| Test Coverage | MEDIUM | Add unit tests for each helper |
| Regression | LOW | Single caller, deterministic logic |
| Performance | NONE | Zero allocation change |

## V12 DNA Compliance

### ✅ Checklist
- [x] **Lock-Free**: No locks in method or extractions
- [x] **ASCII-Only**: No Unicode in strings
- [x] **CYC ≤8**: All methods meet threshold
- [x] **Correctness by Construction**: Boolean helpers prevent invalid states
- [x] **Zero Logic Drift**: Pure structural movement
- [x] **Single Responsibility**: Each helper has one job

### Jane Street Alignment
- **Cognitive Simplicity**: 4 methods with clear names vs 1 complex method
- **Testability**: Each helper independently testable
- **Microsecond Reasoning**: Flat decision tree, no nested branches
- **Exhaustive Testing**: 4 small methods easier to test than 1 large method

## Success Criteria

### Phase 1 (Scope Definition) - COMPLETE
- [x] Method analyzed and documented
- [x] Extraction strategy defined
- [x] Complexity breakdown verified
- [x] Scope boundaries established
- [x] Risk assessment completed
- [x] V12 DNA compliance verified

### Phase 2 (Architecture Planning) - PENDING
- [ ] Detailed extraction plan with line numbers
- [ ] Call graph analysis
- [ ] Test strategy defined

### Phase 3 (DNA & PR Audit) - PENDING
- [ ] Pre-flight safety checks
- [ ] PR hygiene validation

### Phase 4 (Ticket Generation) - PENDING
- [ ] Surgical tickets generated

### Phase 5 (Execution) - PENDING
- [ ] Extractions implemented
- [ ] Tests added
- [ ] Build verified
- [ ] deploy-sync.ps1 executed

## Notes

### Why This Extraction Works
1. **Clear Boundaries**: Each helper has single responsibility
2. **No Side Effects**: Pure boolean functions
3. **Deterministic**: Same inputs always produce same outputs
4. **Testable**: Each helper can be unit tested independently
5. **Readable**: Method names document intent

### Alternative Considered: Single Helper
Could extract all logic into one `ShouldCancelOrder` helper (CYC 14), but this violates CYC ≤8 threshold. Three-helper approach provides better granularity and testability.

### Zombie Prefix Rationale
The 6 zombie prefixes (`EMERGENCY_STOP_`, `T1_` through `T5_`) are business logic constants. These should NOT be extracted to a configuration array in this epic (scope creep). If centralization is desired, create separate EPIC-CCN-99 for "Centralize Zombie Order Prefixes".

## Status
✅ **Phase 1 Complete** - Ready for Phase 2 (Architecture Planning)