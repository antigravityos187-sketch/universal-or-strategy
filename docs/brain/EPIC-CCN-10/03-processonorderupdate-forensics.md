# EPIC-CCN-10: ProcessOnOrderUpdate Forensic Analysis

**Date**: 2026-06-02  
**Analyst**: Advanced Mode (jCodemunch-MCP)  
**Target Method**: `ProcessOnOrderUpdate`  
**Priority**: P1 (Highest in CYC 16-20 tier)

---

## Executive Summary

**CRITICAL FINDING**: The complexity audit reported CYC 19, but actual measurement shows **CYC 21** - even higher priority than initially assessed.

**Method Location**: `src/V12_002.Orders.Callbacks.cs:159-203` (45 lines)

**Complexity Profile**:
- **Cyclomatic Complexity**: 21 (Target: ≤15, Jane Street threshold)
- **Max Nesting Depth**: 4
- **Parameter Count**: 9 (high coupling)
- **Lines of Code**: 45
- **Assessment**: HIGH complexity

**Volatility**: 
- **Churn Rate**: 2.49 commits/week (ACTIVE)
- **Total Commits**: 32 (last 90 days)
- **Authors**: 3 (mdasdispatch, mkalhitti, your-new-email)
- **Last Modified**: 2026-06-01 (very recent)

**Risk Level**: 🔴 **HIGH** - Critical order processing logic with active development

---

## Method Architecture

### Call Graph Analysis

**Caller**: 
- `OnOrderUpdate` (line 141) - NinjaTrader callback wrapper using Actor/FSM pattern
- Called via `Enqueue(ctx => ctx.ProcessOnOrderUpdate(...))` - lock-free dispatch

**Direct Callees** (6 handler methods):
1. `PropagateMasterPriceMove` (CYC unknown) - Price propagation to followers
2. `HandleEntryOrderFilled` (CYC 14) - Entry fill processing
3. `HandleSecondaryOrderFilled` (CYC 17) - Target/stop fill processing
4. `HandleOrderRejected` (CYC unknown) - Rejection handling
5. `HandleOrderCancelled` (CYC unknown) - Cancellation handling
6. `HandleOrderPriceOrQuantityChanged` (CYC 13) - Order modification handling
7. `RemoveGhostOrderRef` (CYC unknown) - Cleanup for orphaned orders

**Transitive Dependencies** (39 symbols at depth 2):
- Position management (SIMA, SymmetryGuard)
- Order submission (SubmitBracketOrders)
- Price calculations (CalculateTargetPriceFromPos)
- Stop management (CreateNewStopOrder, UpdateStopQuantity)
- Cleanup (CleanupPosition, RollbackExpectedPosition)

---

## Complexity Drivers

### Primary Complexity Sources

1. **State Machine Logic** (CYC +8):
   ```csharp
   if (orderState == OrderState.Filled)           // +1
   {
       if (entryOrders.Values.Contains(order))    // +1
           handled = HandleEntryOrderFilled(...);
       else                                        // +1
           handled = HandleSecondaryOrderFilled(...);
   }
   else if (orderState == OrderState.Rejected)    // +1
   {
       handled = HandleOrderRejected(...);
   }
   else if (orderState == OrderState.Cancelled)   // +1
   {
       handled = HandleOrderCancelled(...);
   }
   else if (orderState == OrderState.Accepted || orderState == OrderState.Working) // +2
   {
       handled = HandleOrderPriceOrQuantityChanged(...);
   }
   ```

2. **Nested Conditionals** (CYC +4):
   - Account check + state check (nesting depth 2)
   - Terminal catch-all with compound condition (CYC +3)

3. **Exception Handling** (CYC +1):
   - Try-catch wrapper adds 1 to CYC

4. **Compound Conditions** (CYC +7):
   - `order.Account == this.Account && (orderState == Working || Accepted || ChangeSubmitted)` (+3)
   - `!handled && (orderState == Cancelled || Rejected || Unknown)` (+3)
   - `orderState == Accepted || orderState == Working` (+1)

### Nesting Analysis

**Max Nesting Depth: 4**
```
try {                                    // Level 1
    if (account && state) {              // Level 2
        PropagateMasterPriceMove(...);
    }
    
    if (orderState == Filled) {          // Level 2
        if (entryOrders.Contains) {      // Level 3
            handled = ...;
        }
    }
    
    if (!handled && terminal) {          // Level 2
        RemoveGhostOrderRef(...);
    }
}
catch {                                  // Level 1
    Print(...);
}
```

---

## Extraction Strategy

### Recommended Approach: **State Machine Decomposition**

**Goal**: Reduce CYC 21 → ≤15 by extracting state-specific logic into dedicated methods.

### Phase 1: Extract State Handlers (Target: CYC 10)

**Extract 1: `ProcessOrderState_Filled`**
```csharp
private bool ProcessOrderState_Filled(Order order, int quantity, int filled, double averageFillPrice, DateTime time)
{
    if (entryOrders.Values.Contains(order))
        return HandleEntryOrderFilled(order, quantity, filled, averageFillPrice, time);
    else
        return HandleSecondaryOrderFilled(order, averageFillPrice);
}
```
**CYC Reduction**: -3 (removes if-else chain)

**Extract 2: `ProcessOrderState_Terminal`**
```csharp
private bool ProcessOrderState_Terminal(Order order, OrderState orderState, string nativeError)
{
    if (orderState == OrderState.Rejected)
        return HandleOrderRejected(order, nativeError);
    else if (orderState == OrderState.Cancelled)
        return HandleOrderCancelled(order);
    
    return false;
}
```
**CYC Reduction**: -2 (removes if-else chain)

**Extract 3: `ProcessOrderState_Working`**
```csharp
private bool ProcessOrderState_Working(Order order, double limitPrice, double stopPrice, int quantity)
{
    return HandleOrderPriceOrQuantityChanged(order, limitPrice, stopPrice, quantity);
}
```
**CYC Reduction**: -1 (removes conditional)

**Extract 4: `ShouldPropagatePriceMove`**
```csharp
private bool ShouldPropagatePriceMove(Order order, OrderState orderState)
{
    return order.Account == this.Account && 
           (orderState == OrderState.Working || 
            orderState == OrderState.Accepted || 
            orderState == OrderState.ChangeSubmitted);
}
```
**CYC Reduction**: -3 (removes compound condition)

### Phase 2: Simplified Main Method (Target: CYC 8)

```csharp
private void ProcessOnOrderUpdate(Order order, double limitPrice, double stopPrice,
    int quantity, int filled, double averageFillPrice, OrderState orderState,
    DateTime time, string nativeError)
{
    try
    {
        // Price propagation for working orders
        if (ShouldPropagatePriceMove(order, orderState))
        {
            PropagateMasterPriceMove(order, limitPrice, stopPrice, quantity);
        }

        bool handled = false;

        // State-specific processing
        if (orderState == OrderState.Filled)
            handled = ProcessOrderState_Filled(order, quantity, filled, averageFillPrice, time);
        else if (orderState == OrderState.Rejected || orderState == OrderState.Cancelled)
            handled = ProcessOrderState_Terminal(order, orderState, nativeError);
        else if (orderState == OrderState.Accepted || orderState == OrderState.Working)
            handled = ProcessOrderState_Working(order, limitPrice, stopPrice, quantity);

        // Terminal catch-all for unhandled states
        if (!handled && IsTerminalState(orderState))
        {
            RemoveGhostOrderRef(order, orderState.ToString().ToUpper());
        }
    }
    catch (Exception ex)
    {
        Print("ERROR OnOrderUpdate: " + ex.Message);
    }
}

private bool IsTerminalState(OrderState state)
{
    return state == OrderState.Cancelled || 
           state == OrderState.Rejected || 
           state == OrderState.Unknown;
}
```

**Final CYC**: ~8 (Target: ≤15) ✅

---

## Risk Assessment

### Risk Factors

1. **Critical Path**: ✅ HIGH
   - Core order processing logic
   - Handles all order state transitions
   - Affects position tracking, P&L, and risk management

2. **Test Coverage**: ⚠️ UNKNOWN
   - No test file found for `V12_002.Orders.Callbacks.cs`
   - Existing test: `tests/V12_Performance.Tests/Core/FSMActorTests.cs` (FSM/Actor only)
   - **Action Required**: Add TDD tests before extraction

3. **Volatility**: 🔴 HIGH
   - 2.49 commits/week (ACTIVE development)
   - 32 commits in 90 days
   - Last modified yesterday (2026-06-01)
   - **Risk**: Concurrent changes during refactoring

4. **Coupling**: 🔴 HIGH
   - 9 parameters (high coupling)
   - 39 transitive dependencies
   - Touches 6 major subsystems (SIMA, Symmetry, Orders, Position, Stop, Cleanup)

5. **Lock-Free Correctness**: ✅ VERIFIED
   - Called via Actor/FSM `Enqueue` pattern
   - No `lock()` statements in method body
   - V12 DNA compliant

### Mitigation Strategy

1. **Pre-Extraction Validation**:
   - ✅ Run `pre_push_validation.ps1 -Fast` to establish baseline
   - ✅ Verify all 13 quality gates pass
   - ✅ Document current behavior in TDD tests

2. **Extraction Protocol**:
   - ✅ Extract one method at a time
   - ✅ Run full test suite after each extraction
   - ✅ Verify NinjaTrader F5 build after each commit
   - ✅ Use `deploy-sync.ps1` to sync hard links

3. **Rollback Plan**:
   - ✅ Git checkpoint before each extraction
   - ✅ Bob CLI auto-checkpointing enabled
   - ✅ `/restore` command available for instant rollback

4. **Concurrent Change Detection**:
   - ✅ Rebase onto `origin/main` before starting
   - ✅ Run `verify_pr_hygiene.ps1` before push
   - ✅ Monitor for conflicts during extraction

---

## Complexity Comparison (File Context)

**Top 6 Complex Methods in `V12_002.Orders.Callbacks.cs`**:

| Rank | Method | CYC | LOC | Status |
|------|--------|-----|-----|--------|
| 1 | `ProcessOnOrderUpdate` | **21** | 45 | 🔴 **THIS EPIC** |
| 2 | `HandleSecondaryOrderFilled` | 17 | 55 | ⚠️ Future EPIC |
| 3 | `HandleEntryOrderFilled` | 14 | 47 | ⚠️ Future EPIC |
| 4 | `HandleOrderPriceOrQuantityChanged` | 13 | 36 | ⚠️ Future EPIC |
| 5 | `RequestStopCancelLifecycleSafe` | 12 | ? | ⚠️ Future EPIC |
| 6 | `HandleOrderCancelled_ProcessStopReplacement` | 11 | ? | ⚠️ Future EPIC |

**Observation**: This file is a **complexity hotspot**. After extracting `ProcessOnOrderUpdate`, we should continue with `HandleSecondaryOrderFilled` (CYC 17) and `HandleEntryOrderFilled` (CYC 14).

---

## Effort Estimate

### Time Breakdown

1. **TDD Test Creation**: 1 hour
   - Write tests for current behavior
   - Cover all 5 state transitions
   - Verify lock-free correctness

2. **Extraction (4 methods)**: 1.5 hours
   - Extract `ShouldPropagatePriceMove` (15 min)
   - Extract `ProcessOrderState_Filled` (20 min)
   - Extract `ProcessOrderState_Terminal` (20 min)
   - Extract `ProcessOrderState_Working` (15 min)
   - Extract `IsTerminalState` (10 min)
   - Refactor main method (20 min)

3. **Validation**: 0.5 hours
   - Run pre-push validation (full mode)
   - NinjaTrader F5 build test
   - Manual smoke test (place order, verify callbacks)

**Total Estimate**: 3 hours

---

## Success Criteria

### Quantitative Metrics

- ✅ CYC reduced from 21 → ≤15 (Target: 8)
- ✅ Max nesting depth reduced from 4 → ≤3
- ✅ All 13 pre-push validation checks pass
- ✅ Zero compilation errors
- ✅ Zero new lint violations
- ✅ TDD tests pass (100% coverage of state transitions)

### Qualitative Metrics

- ✅ Code is more readable (single responsibility per method)
- ✅ State machine logic is explicit (no hidden branches)
- ✅ Lock-free correctness preserved (Actor/FSM pattern intact)
- ✅ V12 DNA compliance maintained (ASCII-only, no locks)
- ✅ NinjaTrader F5 build succeeds
- ✅ Manual smoke test passes (order callbacks work correctly)

---

## Next Steps (Stage 1: Vision/Spec)

1. **Review this forensic report** with Director
2. **Create TDD tests** for current behavior
3. **Generate mini-spec.md** with Bob CLI (`v12-engineer` mode)
4. **Proceed to Stage 2** (Arch Planning) after approval

---

## Appendix: Source Code

### Current Implementation

```csharp
private void ProcessOnOrderUpdate(Order order, double limitPrice, double stopPrice,
            int quantity, int filled, double averageFillPrice, OrderState orderState,
            DateTime time, string nativeError)
        {
            try
            {
                if (order.Account == this.Account && 
                    (orderState == OrderState.Working || orderState == OrderState.Accepted || orderState == OrderState.ChangeSubmitted))
                {
                    PropagateMasterPriceMove(order, limitPrice, stopPrice, quantity);
                }

                bool handled = false;

                if (orderState == OrderState.Filled)
                {
                    if (entryOrders.Values.Contains(order))
                        handled = HandleEntryOrderFilled(order, quantity, filled, averageFillPrice, time);
                    else
                        handled = HandleSecondaryOrderFilled(order, averageFillPrice);
                }
                else if (orderState == OrderState.Rejected)
                {
                    handled = HandleOrderRejected(order, nativeError);
                }
                else if (orderState == OrderState.Cancelled)
                {
                    handled = HandleOrderCancelled(order);
                }
                else if (orderState == OrderState.Accepted || orderState == OrderState.Working)
                {
                    handled = HandleOrderPriceOrQuantityChanged(order, limitPrice, stopPrice, quantity);
                }

                // Terminal catch-all
                if (!handled && (orderState == OrderState.Cancelled || orderState == OrderState.Rejected || orderState == OrderState.Unknown))
                {
                    RemoveGhostOrderRef(order, orderState.ToString().ToUpper());
                }
            }
            catch (Exception ex)
            {
                Print("ERROR OnOrderUpdate: " + ex.Message);
            }
        }
```

**Location**: `src/V12_002.Orders.Callbacks.cs:159-203`

---

## References

- **Complexity Audit**: `complexity_audit_report.txt`
- **Jane Street Threshold**: CYC ≤15 (docs/intel/jane-street/)
- **V12 DNA**: `AGENTS.md` (Lock-Free Actor Pattern, ASCII-Only)
- **Phase 6 Protocol**: `AGENTS.md` Section 7 (Recursive Extraction)
- **Pre-Push Validation**: `scripts/pre_push_validation.ps1`

---

**Report Status**: ✅ COMPLETE  
**Ready for Stage 1**: YES  
**Recommended Next Agent**: Bob CLI (`v12-engineer` mode)