# Phase 1: Scope Definition - EPIC-W7-061

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.37
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T19:31:58Z

## Epic Overview
- **Target Method**: SubmitAndRegisterFleetOrders
- **File**: src/V12_002.SIMA.Fleet.cs
- **Line**: 174-217 (44 lines)
- **Current CYC**: 12
- **Target CYC**: ≤8 (Jane Street threshold)
- **Reduction Required**: -4 CYC points (33% reduction)

## Scope Boundaries

### What Will Be Extracted

#### Extraction Target 1: PrepareOrderArrayForSubmission
**Lines**: 183-188
**Responsibility**: Order array preparation and validation
**Complexity Contribution**: ~2 CYC (conditional + array operations)

```csharp
// EXTRACT THIS BLOCK
Order[] submitOrders = orders;
if (orders != null && orderCount > 0 && orderCount < orders.Length)
{
    submitOrders = new Order[orderCount];
    Array.Copy(orders, submitOrders, orderCount);
}
```

**Rationale**: 
- Single responsibility: array preparation
- Clear input/output contract
- No side effects on FSM state
- Reduces nesting in main method

**Signature**:
```csharp
private Order[] PrepareOrderArrayForSubmission(Order[] orders, int orderCount)
```

#### Extraction Target 2: UpdateFSMStateAfterSubmission
**Lines**: 194-202
**Responsibility**: FSM state transition for PendingSubmit → Submitted
**Complexity Contribution**: ~3 CYC (nested conditionals)

```csharp
// EXTRACT THIS BLOCK
FollowerBracketFSM pFsm;
if (
    _followerBrackets.TryGetValue(fleetEntryName, out pFsm)
    && pFsm != null
    && pFsm.State == FollowerBracketState.PendingSubmit
)
{
    pFsm.State = FollowerBracketState.Submitted;
    pFsm.LastUpdateUtc = DateTime.UtcNow;
}
```

**Rationale**:
- Single responsibility: FSM state management
- Encapsulates state transition logic
- Reduces nesting depth in main method
- Clear precondition (PendingSubmit state)

**Signature**:
```csharp
private void UpdateFSMStateAfterSubmission(string fleetEntryName)
```

#### Extraction Target 3: RegisterOrderIdsToFSM
**Lines**: 204-211
**Responsibility**: Map submitted order IDs to FSM key
**Complexity Contribution**: ~3 CYC (nested conditionals + loop)

```csharp
// EXTRACT THIS BLOCK
FollowerBracketFSM fsm;
if (_followerBrackets.TryGetValue(fleetEntryName, out fsm))
{
    for (int i = 0; i < orderCount; i++)
    {
        var ord = orders[i];
        if (ord != null && !string.IsNullOrEmpty(ord.OrderId))
            _orderIdToFsmKey[ord.OrderId] = fleetEntryName;
    }
}
```

**Rationale**:
- Single responsibility: order ID registration
- Encapsulates mapping logic
- Reduces loop nesting in main method
- Clear side effect (updates _orderIdToFsmKey)

**Signature**:
```csharp
private void RegisterOrderIdsToFSM(Order[] orders, int orderCount, string fleetEntryName)
```

### What Will Remain in Original Method

**Core Orchestration Logic** (Lines 190-192, 213):
```csharp
private void SubmitAndRegisterFleetOrders(
    Account acct,
    Order[] orders,
    int orderCount,
    string fleetEntryName,
    string expectedKey,
    ref bool syncCleared
)
{
    // EXTRACTED: PrepareOrderArrayForSubmission
    Order[] submitOrders = PrepareOrderArrayForSubmission(orders, orderCount);
    
    // REMAINS: Core submission logic
    acct.Submit(submitOrders);
    ClearDispatchSyncPending(expectedKey);
    syncCleared = true;
    
    // EXTRACTED: UpdateFSMStateAfterSubmission
    UpdateFSMStateAfterSubmission(fleetEntryName);
    
    // EXTRACTED: RegisterOrderIdsToFSM
    RegisterOrderIdsToFSM(orders, orderCount, fleetEntryName);
    
    // REMAINS: Logging
    Print(string.Format("[PUMP] Submitted {0} orders for {1} | {2}", orderCount, fleetEntryName, acct.Name));
}
```

**Remaining Complexity**: ~4 CYC
- Method orchestration: 1 CYC (base)
- Conditional in PrepareOrderArrayForSubmission call: 0 CYC (delegated)
- Sequential calls: 3 CYC (3 method calls)

**Expected Final CYC**: 4 (well below threshold of 8)

## Dependencies and Risks

### Internal Dependencies
1. **_followerBrackets** (ConcurrentDictionary<string, FollowerBracketFSM>)
   - Used by: UpdateFSMStateAfterSubmission, RegisterOrderIdsToFSM
   - Risk: LOW (read-only access, no mutation conflicts)

2. **_orderIdToFsmKey** (ConcurrentDictionary<string, string>)
   - Used by: RegisterOrderIdsToFSM
   - Risk: LOW (write-only access, no read dependencies)

3. **ClearDispatchSyncPending** (method)
   - Called by: SubmitAndRegisterFleetOrders (remains)
   - Risk: NONE (no change to call site)

### External Dependencies (Callers)
1. **ProcessFleetSlot** (src/V12_002.SIMA.Fleet.cs:44)
2. **PumpFleetDispatch** (src/V12_002.SIMA.Fleet.cs:233)
3. **ProcessValidPhotonSlot** (src/V12_002.SIMA.Fleet.cs:395)
4. **VerifyPhotonSlotIntegrity** (src/V12_002.SIMA.Fleet.cs:329)

**Caller Impact**: NONE - Method signature remains unchanged

### Risk Assessment

#### LOW RISK FACTORS ✅
- **Blast Radius**: 0 external importers, 4 internal callers (unchanged)
- **Churn**: Low (not in top 50 hotspots)
- **Test Coverage**: Isolated to Fleet module
- **Side Effects**: All side effects preserved in extracted methods

#### MEDIUM RISK FACTORS ⚠️
- **FSM State Consistency**: Must preserve exact state transition order
- **Order ID Registration**: Must maintain registration timing (after submission)
- **Concurrent Access**: _followerBrackets accessed twice (potential race condition)

#### MITIGATION STRATEGIES
1. **Preserve Execution Order**: Extract methods called in same sequence
2. **Atomic Operations**: Use existing ConcurrentDictionary thread-safety
3. **No Logic Changes**: Pure extraction, no behavioral modifications
4. **Verification**: Unit tests for each extracted method

## Success Criteria

### Quantitative Metrics
- ✅ **CYC Reduction**: 12 → ≤8 (target: 4)
- ✅ **Method Count**: 1 → 4 (3 new extracted methods)
- ✅ **Max Nesting Depth**: 4 → ≤2
- ✅ **Parameter Count**: 6 → 6 (unchanged signature)
- ✅ **Lines per Method**: 44 → ~10-15 per method

### Qualitative Criteria
- ✅ **Single Responsibility**: Each method has one clear purpose
- ✅ **No Behavioral Changes**: Exact same execution flow
- ✅ **Caller Compatibility**: No changes to call sites
- ✅ **Thread Safety**: Preserved concurrent access patterns
- ✅ **Readability**: Improved through method naming

### Verification Checklist
- [ ] Build passes (dotnet build)
- [ ] All 4 callers unchanged
- [ ] FSM state transitions preserved
- [ ] Order ID registration timing preserved
- [ ] Logging output unchanged
- [ ] No new compiler warnings
- [ ] CYC ≤8 confirmed via complexity_audit.py

## Extraction Order

### Phase 5 Ticket Sequence
1. **Ticket 1**: Extract PrepareOrderArrayForSubmission
   - Simplest extraction (no FSM dependencies)
   - Reduces nesting immediately
   - CYC: 12 → 10

2. **Ticket 2**: Extract UpdateFSMStateAfterSubmission
   - FSM state transition logic
   - Reduces conditional nesting
   - CYC: 10 → 7

3. **Ticket 3**: Extract RegisterOrderIdsToFSM
   - Order ID mapping logic
   - Removes loop from main method
   - CYC: 7 → 4

**Rationale**: Bottom-up extraction (simplest first) minimizes risk and allows incremental verification.

## Boundary Validation

### Clear Boundaries ✅
- **Array Preparation**: Lines 183-188 (self-contained)
- **FSM State Update**: Lines 194-202 (single state transition)
- **Order ID Registration**: Lines 204-211 (single mapping operation)

### No Boundary Violations ✅
- **No Shared Mutable State**: Each extraction operates on distinct data
- **No Temporal Coupling**: Execution order preserved through sequential calls
- **No Hidden Dependencies**: All dependencies explicit in parameters

### Edge Cases Handled ✅
- **Null Orders**: Handled in PrepareOrderArrayForSubmission
- **Empty Order Array**: Handled via orderCount parameter
- **Missing FSM Entry**: Handled via TryGetValue pattern
- **Null Order IDs**: Handled in RegisterOrderIdsToFSM

## Phase 1 Conclusion

**SCOPE APPROVED** - Proceed to Phase 2 (Architecture Planning)

**Scope Summary**:
- 3 methods to extract
- 0 signature changes
- 4 CYC reduction (12 → 4)
- LOW risk, HIGH confidence
- Clear boundaries, no violations

**Next Phase**: Architecture Planning (Phase 2) - Define extraction implementation strategy and generate Mermaid diagrams.
