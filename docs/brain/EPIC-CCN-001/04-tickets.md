# Extraction Tickets: EPIC-CCN-001

## Overview

- **Epic ID**: EPIC-CCN-001
- **Target Method**: `SymmetryGuardReplaceExistingFollowerTarget`
- **Target File**: `src/V12_002.Symmetry.Replace.cs`
- **Current Complexity**: 18 (CYC)
- **Target Complexity**: ≤8 (Jane Street strict standard)
- **Total Tickets**: 3
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3)
- **Estimated Effort**: 4-6 hours (including testing and verification)

---

## TICKET-1: Extract ShouldCancelTarget Helper

### Scope

- **Current Method**: `SymmetryGuardReplaceExistingFollowerTarget`
- **Current CYC**: 18
- **Target CYC**: 16 (reduction of 2)
- **Extraction**: Pure decision function for target cancellation logic

### Implementation

1. **Create Helper Method**
   ```csharp
   private static bool ShouldCancelTarget(bool isFilled, bool isRunner, int qty)
   {
       return isFilled || isRunner || qty <= 0;
   }
   ```

2. **Replace Inline Logic** (Lines 15-31 in original method)
   - Find: `if (isFilled || isRunner || qty <= 0)`
   - Replace with: `if (ShouldCancelTarget(isFilled, isRunner, qty))`

3. **Verify Behavior**
   - Run existing tests: `dotnet test`
   - Verify complexity reduced: `python scripts/complexity_audit.py`
   - Expected CYC: 16 (down from 18)

### Acceptance Criteria

- [ ] Helper method created with correct signature
- [ ] Inline logic replaced with helper call
- [ ] Method complexity reduced to 16 (verified via complexity_audit.py)
- [ ] All existing tests pass (dotnet test)
- [ ] No behavioral changes (exact logic preserved)
- [ ] Build succeeds (dotnet build)
- [ ] CSharpier formatting applied (dotnet csharpier format src/)

### Test Coverage (New Tests)

- [ ] Unit test: `ShouldCancelTarget_WhenFilled_ReturnsTrue`
- [ ] Unit test: `ShouldCancelTarget_WhenRunner_ReturnsTrue`
- [ ] Unit test: `ShouldCancelTarget_WhenQtyZero_ReturnsTrue`
- [ ] Unit test: `ShouldCancelTarget_WhenQtyNegative_ReturnsTrue`
- [ ] Unit test: `ShouldCancelTarget_WhenAllFalse_ReturnsFalse`

### Dependencies

- None (first ticket)

### Verification Commands

```powershell
# Format code
dotnet csharpier format src/

# Build
dotnet build

# Run tests
dotnet test

# Check complexity
python scripts/complexity_audit.py

# Verify CYC reduced to 16
# Expected output: SymmetryGuardReplaceExistingFollowerTarget: 16
```

### Estimated Time

- Implementation: 30 minutes
- Testing: 30 minutes
- Verification: 15 minutes
- **Total**: 1.25 hours

---

## TICKET-2: Extract IsOrderCancellable Helper

### Scope

- **Current Method**: `SymmetryGuardReplaceExistingFollowerTarget`
- **Current CYC**: 16 (after TICKET-1)
- **Target CYC**: 12 (reduction of 4)
- **Extraction**: Pure state validation function for OrderState checks

### Implementation

1. **Create Helper Method**
   ```csharp
   private static bool IsOrderCancellable(Order order)
   {
       return order.OrderState == OrderState.Working
           || order.OrderState == OrderState.Accepted
           || order.OrderState == OrderState.Submitted
           || order.OrderState == OrderState.ChangePending;
   }
   ```

2. **Replace First Inline Check** (Lines 23-26 in original method)
   - Find: `if (staleTarget.OrderState == OrderState.Working || staleTarget.OrderState == OrderState.Accepted || staleTarget.OrderState == OrderState.Submitted || staleTarget.OrderState == OrderState.ChangePending)`
   - Replace with: `if (IsOrderCancellable(staleTarget))`

3. **Replace Second Inline Check** (Lines 46-49 in original method)
   - Find: `if (oldTarget.OrderState == OrderState.Working || oldTarget.OrderState == OrderState.Accepted || oldTarget.OrderState == OrderState.Submitted || oldTarget.OrderState == OrderState.ChangePending)`
   - Replace with: `if (IsOrderCancellable(oldTarget))`

4. **Verify Behavior**
   - Run existing tests: `dotnet test`
   - Verify complexity reduced: `python scripts/complexity_audit.py`
   - Expected CYC: 12 (down from 16)

### Acceptance Criteria

- [ ] Helper method created with correct signature
- [ ] Both inline checks replaced with helper calls
- [ ] Method complexity reduced to 12 (verified via complexity_audit.py)
- [ ] All existing tests pass (dotnet test)
- [ ] No behavioral changes (exact logic preserved)
- [ ] Build succeeds (dotnet build)
- [ ] CSharpier formatting applied (dotnet csharpier format src/)

### Test Coverage (New Tests)

- [ ] Unit test: `IsOrderCancellable_WhenWorking_ReturnsTrue`
- [ ] Unit test: `IsOrderCancellable_WhenAccepted_ReturnsTrue`
- [ ] Unit test: `IsOrderCancellable_WhenSubmitted_ReturnsTrue`
- [ ] Unit test: `IsOrderCancellable_WhenChangePending_ReturnsTrue`
- [ ] Unit test: `IsOrderCancellable_WhenFilled_ReturnsFalse`
- [ ] Unit test: `IsOrderCancellable_WhenCancelled_ReturnsFalse`
- [ ] Unit test: `IsOrderCancellable_WhenRejected_ReturnsFalse`

### Dependencies

- **TICKET-1** must be completed first
- Requires CYC 16 baseline from TICKET-1

### Verification Commands

```powershell
# Format code
dotnet csharpier format src/

# Build
dotnet build

# Run tests
dotnet test

# Check complexity
python scripts/complexity_audit.py

# Verify CYC reduced to 12
# Expected output: SymmetryGuardReplaceExistingFollowerTarget: 12
```

### Estimated Time

- Implementation: 45 minutes
- Testing: 45 minutes
- Verification: 15 minutes
- **Total**: 1.75 hours

---

## TICKET-3: Extract CreateFollowerTargetReplaceSpec Helper

### Scope

- **Current Method**: `SymmetryGuardReplaceExistingFollowerTarget`
- **Current CYC**: 12 (after TICKET-2)
- **Target CYC**: 7-8 (reduction of 4-5)
- **Extraction**: Spec builder function for FollowerTargetReplaceSpec creation

### Implementation

1. **Create Helper Method**
   ```csharp
   private FollowerTargetReplaceSpec CreateFollowerTargetReplaceSpec(
       string fleetEntryName,
       PositionInfo pos,
       int targetNumber,
       int qty,
       string targetTag,
       Order oldTarget
   )
   {
       double targetPrice = GetTargetPrice(pos, targetNumber);
       
       if (targetPrice <= 0)
       {
           return null;
       }
       
       var exitAction = pos.Direction == MarketPosition.Long
           ? OrderAction.Sell
           : OrderAction.Buy;
       
       return new FollowerTargetReplaceSpec
       {
           FleetEntryName = fleetEntryName,
           TargetNumber = targetNumber,
           Qty = qty,
           LimitPrice = SymmetryTrim(
               Instrument.MasterInstrument,
               targetPrice,
               exitAction
           ),
           TargetTag = targetTag,
           OldTargetOrder = oldTarget
       };
   }
   ```

2. **Replace Inline Spec Creation** (Lines 51-72 in original method)
   - Find: Entire spec creation block (21 lines)
   - Replace with:
     ```csharp
     var spec = CreateFollowerTargetReplaceSpec(
         fleetEntryName,
         pos,
         targetNumber,
         qty,
         targetTag,
         oldTarget
     );
     
     if (spec != null)
     {
         _followerTargetReplaceSpecs[fleetEntryName] = spec;
         StampReaperMoveGrace();
         pos.ExecutingAccount.Cancel(oldTarget);
     }
     ```

3. **Verify Behavior**
   - Run existing tests: `dotnet test`
   - Verify complexity reduced: `python scripts/complexity_audit.py`
   - Expected CYC: 7-8 (down from 12)

### Acceptance Criteria

- [ ] Helper method created with correct signature
- [ ] Inline spec creation replaced with helper call
- [ ] Null check added for invalid price case
- [ ] Method complexity reduced to ≤8 (verified via complexity_audit.py)
- [ ] All existing tests pass (dotnet test)
- [ ] No behavioral changes (exact logic preserved)
- [ ] Build succeeds (dotnet build)
- [ ] CSharpier formatting applied (dotnet csharpier format src/)

### Test Coverage (New Tests)

- [ ] Unit test: `CreateFollowerTargetReplaceSpec_WhenValidPrice_ReturnsSpec`
- [ ] Unit test: `CreateFollowerTargetReplaceSpec_WhenZeroPrice_ReturnsNull`
- [ ] Unit test: `CreateFollowerTargetReplaceSpec_WhenNegativePrice_ReturnsNull`
- [ ] Unit test: `CreateFollowerTargetReplaceSpec_WhenLongPosition_SetsExitActionSell`
- [ ] Unit test: `CreateFollowerTargetReplaceSpec_WhenShortPosition_SetsExitActionBuy`
- [ ] Unit test: `CreateFollowerTargetReplaceSpec_VerifyAllFieldsPopulated`

### Dependencies

- **TICKET-2** must be completed first
- Requires CYC 12 baseline from TICKET-2

### Verification Commands

```powershell
# Format code
dotnet csharpier format src/

# Build
dotnet build

# Run tests
dotnet test

# Check complexity
python scripts/complexity_audit.py

# Verify CYC reduced to ≤8
# Expected output: SymmetryGuardReplaceExistingFollowerTarget: 7 or 8
```

### Estimated Time

- Implementation: 1 hour
- Testing: 1 hour
- Verification: 15 minutes
- **Total**: 2.25 hours

---

## Final Verification Protocol

### After All Tickets Complete

1. **Complexity Audit**
   ```powershell
   python scripts/complexity_audit.py
   ```
   - Expected: SymmetryGuardReplaceExistingFollowerTarget ≤8
   - Expected: ShouldCancelTarget = 2
   - Expected: IsOrderCancellable = 2
   - Expected: CreateFollowerTargetReplaceSpec = 4

2. **Pre-Push Validation** (All 13 Checks)
   ```powershell
   powershell -File .\scripts\pre_push_validation.ps1
   ```
   - Must pass all blocking checks
   - ASCII-Only: PASS
   - Build: PASS
   - Unit Tests: PASS
   - Lint: PASS
   - Formatting: PASS
   - Complexity: PASS (CYC ≤15)
   - PR Hygiene: PASS (diff <10k)

3. **NinjaTrader Integration**
   ```powershell
   powershell -File .\deploy-sync.ps1
   ```
   - Sync hard links
   - F5 compile test in NinjaTrader
   - Runtime verification (no exceptions)

4. **CodeScene Monitoring**
   - Open `src/V12_002.Symmetry.Replace.cs` in VS Code
   - Check Code Health Score (should improve)
   - Verify hotspot status (should move from red→yellow or yellow→green)

### Success Criteria (All Must Pass)

- [ ] Main method complexity: ≤8 (Jane Street strict)
- [ ] Helper 1 complexity: 2
- [ ] Helper 2 complexity: 2
- [ ] Helper 3 complexity: 4
- [ ] All existing tests pass
- [ ] All new unit tests pass (15 tests total)
- [ ] Pre-push validation passes (all 13 checks)
- [ ] NinjaTrader hard links synced
- [ ] NinjaTrader F5 compile succeeds
- [ ] No runtime exceptions in NinjaTrader
- [ ] CodeScene Code Health Score improved
- [ ] Zero lock() statements (forensic scan)
- [ ] Zero Unicode characters (ASCII-only scan)
- [ ] PR diff <10,000 characters

---

## Risk Mitigation

### Regression Prevention

- **Step-by-step verification**: Run tests after each ticket
- **Existing test coverage**: FSMActorTests.cs provides safety net
- **Behavior preservation**: No semantic changes, only structural refactoring
- **CSharpier formatting**: Prevents whitespace mutations

### Performance Validation

- **No new allocations**: Static helpers where possible
- **JIT inlining**: All helpers <20 LOC (inline candidates)
- **No virtual calls**: Private methods (direct calls)
- **Lock-free**: Maintains Actor Enqueue pattern

### Integration Safety

- **Method signature unchanged**: No caller impact
- **Private helpers**: No external visibility
- **Single-file refactoring**: No cross-file dependencies
- **Hard-link sync**: deploy-sync.ps1 ensures NinjaTrader consistency

---

## Rollback Plan

### If Any Ticket Fails

1. **Immediate Actions**
   - Stop execution
   - Document failure reason
   - Restore from checkpoint (Bob CLI `/restore`)

2. **Root Cause Analysis**
   - Review test failures
   - Check complexity metrics
   - Verify behavior preservation

3. **Recovery Options**
   - Fix and retry current ticket
   - Rollback to previous ticket
   - Escalate to Director for architectural review

### Checkpoint Strategy

- **After TICKET-1**: Checkpoint (CYC 16)
- **After TICKET-2**: Checkpoint (CYC 12)
- **After TICKET-3**: Checkpoint (CYC 7-8)

---

## Metadata

**Document Version**: 1.0  
**Created**: 2026-06-15  
**Author**: Bob Shell (Phase 4 - Ticket Generation)  
**Epic**: EPIC-CCN-001  
**Protocol**: V12.23 (Boundary Validation)  
**Total Tickets**: 3  
**Estimated Effort**: 5.25 hours (implementation + testing + verification)  
**Target Complexity**: ≤8 (Jane Street strict standard)  
**Execution Model**: Sequential (TICKET-1 → TICKET-2 → TICKET-3)

---

**Document Status**: FINAL  
**Ready for Phase 5**: YES (Ticket Execution)  
**Next Action**: Execute TICKET-1 (Extract ShouldCancelTarget)
