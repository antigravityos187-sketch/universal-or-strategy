# Phase 1: Scope Definition - EPIC-W7-087

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.00
- **API Key**: N/A
- **Execution Time**: 2026-06-24T01:33:55Z

## Target Method Analysis

### Method Signature
```csharp
private bool AuditFleet_CheckWorkingStop(Account acct)
```

### Current Implementation (11 lines, CYC=9)
```csharp
private bool AuditFleet_CheckWorkingStop(Account acct)
{
    // Build 1108.003 [D3]: Snapshot broker orders before iteration. orderSnapshot
    var orders = acct.Orders.ToArray();
    return orders.Any(o =>
        o.Instrument?.FullName == Instrument?.FullName
        && (o.OrderState == OrderState.Working || o.OrderState == OrderState.Accepted)
        && (o.OrderType == OrderType.StopMarket || o.OrderType == OrderType.StopLimit)
        && (o.OrderAction == OrderAction.Sell || o.OrderAction == OrderAction.BuyToCover)
    );
}
```

## Complexity Breakdown

The method has CYC=9 due to:
1. Base method: +1
2. `Any()` predicate: +1
3. Instrument name check with null-conditional: +1
4. OrderState OR condition: +2 (Working || Accepted)
5. OrderType OR condition: +2 (StopMarket || StopLimit)
6. OrderAction OR condition: +2 (Sell || BuyToCover)

**Total**: 1 + 1 + 1 + 2 + 2 + 2 = 9

## Scope Definition

### IN SCOPE ✅

**Primary Extraction Target**: The LINQ predicate logic (5 boolean conditions)

1. **Extract Predicate to Helper Method**
   - Create `IsWorkingStopOrder(Order o)` helper method
   - Move all 4 conditional checks into this method
   - Reduces main method to CYC=2 (base + Any)
   - Helper method will have CYC=7 (within threshold)

2. **Specific Conditions to Extract**:
   - Instrument name matching: `o.Instrument?.FullName == Instrument?.FullName`
   - Order state validation: `o.OrderState == OrderState.Working || o.OrderState == OrderState.Accepted`
   - Order type validation: `o.OrderType == OrderType.StopMarket || o.OrderType == OrderType.StopLimit`
   - Order action validation: `o.OrderAction == OrderAction.Sell || o.OrderAction == OrderAction.BuyToCover`

3. **Preserve**:
   - Build comment: `// Build 1108.003 [D3]: Snapshot broker orders before iteration. orderSnapshot`
   - Order snapshot pattern: `var orders = acct.Orders.ToArray()`
   - Method signature and return type

### OUT OF SCOPE ❌

1. **No Changes to Callers**:
   - `AuditFleet_HandleNakedPosition` (line 335)
   - `AuditSingleFleetAccount` (line 121)
   - `AuditApexPositions` (line 16)

2. **No Changes to Related Methods**:
   - Other audit methods in the file
   - Fleet management logic
   - Position tracking logic

3. **No Architectural Changes**:
   - No new classes or interfaces
   - No changes to method visibility
   - No changes to parameter types

4. **No Performance Optimizations**:
   - Keep existing `ToArray()` snapshot pattern
   - No caching or memoization
   - No async/await conversion

## Extraction Strategy

### Approach: Single Helper Method
Extract the entire LINQ predicate into one helper method to achieve CYC ≤ 8 for both methods.

**Before** (CYC=9):
```csharp
private bool AuditFleet_CheckWorkingStop(Account acct)
{
    var orders = acct.Orders.ToArray();
    return orders.Any(o =>
        o.Instrument?.FullName == Instrument?.FullName
        && (o.OrderState == OrderState.Working || o.OrderState == OrderState.Accepted)
        && (o.OrderType == OrderType.StopMarket || o.OrderType == OrderType.StopLimit)
        && (o.OrderAction == OrderAction.Sell || o.OrderAction == OrderAction.BuyToCover)
    );
}
```

**After** (Main: CYC=2, Helper: CYC=7):
```csharp
private bool AuditFleet_CheckWorkingStop(Account acct)
{
    // Build 1108.003 [D3]: Snapshot broker orders before iteration. orderSnapshot
    var orders = acct.Orders.ToArray();
    return orders.Any(IsWorkingStopOrder);
}

private bool IsWorkingStopOrder(Order o)
{
    return o.Instrument?.FullName == Instrument?.FullName
        && (o.OrderState == OrderState.Working || o.OrderState == OrderState.Accepted)
        && (o.OrderType == OrderType.StopMarket || o.OrderType == OrderType.StopLimit)
        && (o.OrderAction == OrderAction.Sell || o.OrderAction == OrderAction.BuyToCover);
}
```

### Complexity Verification
- **Main Method**: CYC = 1 (base) + 1 (Any) = 2 ✅
- **Helper Method**: CYC = 1 (base) + 1 (null-conditional) + 2 (OrderState OR) + 2 (OrderType OR) + 2 (OrderAction OR) = 8 ✅

**Note**: The helper method is at CYC=8 (threshold), but this is acceptable because:
1. It's a pure predicate function (no side effects)
2. All conditions are related to the same concern (identifying working stop orders)
3. Further extraction would create artificial fragmentation
4. The logic is cohesive and readable

## Risk Assessment

### Extraction Risk: MINIMAL

**Justification**:
1. **Zero Blast Radius**: No external dependencies
2. **Internal Callers Only**: All 3 callers in same file
3. **Pure Function**: No side effects, deterministic output
4. **Simple Refactor**: Single extraction, no complex logic changes
5. **Testable**: Easy to unit test the helper method

### Testing Strategy
1. **Unit Test**: Verify `IsWorkingStopOrder` with various order states
2. **Integration Test**: Verify `AuditFleet_CheckWorkingStop` behavior unchanged
3. **Regression Test**: Run existing fleet audit tests

## Success Criteria

### Phase 1 Complete ✅
- [x] Method implementation analyzed
- [x] Complexity breakdown documented
- [x] IN SCOPE items defined
- [x] OUT OF SCOPE items defined
- [x] Extraction strategy planned
- [x] Risk assessment completed

### Ready for Phase 2 (Architecture Planning)
- Target: Reduce CYC from 9 to 2 (main) + 8 (helper)
- Approach: Single helper method extraction
- Risk: Minimal (zero blast radius, internal callers only)
- Effort: Low (11 lines, clear scope)

## Next Steps

Proceed to Phase 2: Architecture Planning to:
1. Design the exact method signatures
2. Plan the extraction sequence
3. Define test cases
4. Create implementation tickets
