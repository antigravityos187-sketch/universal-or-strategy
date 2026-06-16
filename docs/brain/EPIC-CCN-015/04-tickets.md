# Extraction Tickets: EPIC-CCN-015

## Overview
- **Epic**: EPIC-CCN-015 - CancelAll_ProcessSingleFleetAccount Extraction
- **Total Tickets**: 4
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3 → TICKET-4)
- **Estimated Effort**: 6 hours (1.5h per ticket)
- **Target File**: src/V12_002.UI.IPC.Commands.Fleet.cs
- **Complexity Reduction**: CYC 18 → CYC 5 (72% reduction)

## Execution Strategy
Each ticket extracts one helper method and adds corresponding unit tests. TICKET-4 refactors the main method to use all helpers. All tickets must be executed sequentially to maintain build integrity.

---

## TICKET-1: Extract IsOrderCancellable Helper

### Scope
- **Current Method**: `CancelAll_ProcessSingleFleetAccount`
- **Current CYC**: 18
- **Target CYC After Extraction**: 13 (reduce by 5)
- **Extraction**: Order state validation logic (5 OR conditions)

### Implementation
1. **Create Helper Method**
   ```csharp
   private static bool IsOrderCancellable(Order order, Instrument targetInstrument)
   {
       return order.OrderState == OrderState.Accepted ||
              order.OrderState == OrderState.Working ||
              order.OrderState == OrderState.PendingSubmit ||
              order.OrderState == OrderState.PendingChange ||
              order.OrderState == OrderState.PendingCancel;
   }
   ```

2. **Add Unit Tests** (create `tests/V12_Performance.Tests/Core/OrderValidationTests.cs`)
   - Test case 1: Order with Accepted state → returns true
   - Test case 2: Order with Working state → returns true
   - Test case 3: Order with PendingSubmit state → returns true
   - Test case 4: Order with PendingChange state → returns true
   - Test case 5: Order with PendingCancel state → returns true
   - Test case 6: Order with Filled state → returns false
   - Test case 7: Order with Cancelled state → returns false

3. **Verify Build**
   - Run `dotnet build` (must succeed)
   - Run `dotnet test` (all tests must pass)
   - Run `dotnet csharpier check src/` (formatting must pass)

### Acceptance Criteria
- [ ] Helper method created with correct signature
- [ ] Helper method is static and private
- [ ] Helper method has CYC 5 (5 OR conditions)
- [ ] 7 unit tests added and passing
- [ ] Build succeeds with zero errors
- [ ] CSharpier formatting passes
- [ ] No behavioral changes to main method (not yet refactored)

### Dependencies
- None (first ticket)

### Verification Commands
```powershell
dotnet build
dotnet test --filter "FullyQualifiedName~OrderValidationTests"
dotnet csharpier check src/V12_002.UI.IPC.Commands.Fleet.cs
```

---

## TICKET-2: Extract IsBracketOrder Helper

### Scope
- **Current Method**: `CancelAll_ProcessSingleFleetAccount`
- **Current CYC**: 18 (unchanged from TICKET-1, helper not yet used)
- **Target CYC After Extraction**: 11 (reduce by 7 when refactored)
- **Extraction**: Order name prefix validation logic (7 OR conditions)

### Implementation
1. **Create Helper Method**
   ```csharp
   private static bool IsBracketOrder(string orderName)
   {
       return orderName.StartsWith("SL ") ||
              orderName.StartsWith("PT ") ||
              orderName.StartsWith("TRAIL ") ||
              orderName.StartsWith("BE ") ||
              orderName.StartsWith("STOP ") ||
              orderName.StartsWith("LIMIT ") ||
              orderName.StartsWith("MIT ");
   }
   ```

2. **Add Unit Tests** (extend `tests/V12_Performance.Tests/Core/OrderValidationTests.cs`)
   - Test case 1: Order name "SL 123" → returns true
   - Test case 2: Order name "PT 456" → returns true
   - Test case 3: Order name "TRAIL 789" → returns true
   - Test case 4: Order name "BE 101" → returns true
   - Test case 5: Order name "STOP 202" → returns true
   - Test case 6: Order name "LIMIT 303" → returns true
   - Test case 7: Order name "MIT 404" → returns true
   - Test case 8: Order name "ENTRY 505" → returns false
   - Test case 9: Empty string → returns false

3. **Verify Build**
   - Run `dotnet build` (must succeed)
   - Run `dotnet test` (all tests must pass)
   - Run `dotnet csharpier check src/` (formatting must pass)

### Acceptance Criteria
- [ ] Helper method created with correct signature
- [ ] Helper method is static and private
- [ ] Helper method has CYC 7 (7 OR conditions)
- [ ] 9 unit tests added and passing
- [ ] Build succeeds with zero errors
- [ ] CSharpier formatting passes
- [ ] No behavioral changes to main method (not yet refactored)

### Dependencies
- TICKET-1 must be completed first (build integrity)

### Verification Commands
```powershell
dotnet build
dotnet test --filter "FullyQualifiedName~OrderValidationTests"
dotnet csharpier check src/V12_002.UI.IPC.Commands.Fleet.cs
```

---

## TICKET-3: Extract ShouldPreserveBracket Helper

### Scope
- **Current Method**: `CancelAll_ProcessSingleFleetAccount`
- **Current CYC**: 18 (unchanged, helpers not yet used)
- **Target CYC After Extraction**: 9 (reduce by 2 when refactored)
- **Extraction**: Bracket preservation decision logic (AND condition)

### Implementation
1. **Create Helper Method**
   ```csharp
   private static bool ShouldPreserveBracket(bool acctHasActiveFsm, bool masterHasPosition)
   {
       return acctHasActiveFsm && masterHasPosition;
   }
   ```

2. **Add Unit Tests** (extend `tests/V12_Performance.Tests/Core/OrderValidationTests.cs`)
   - Test case 1: acctHasActiveFsm=true, masterHasPosition=true → returns true
   - Test case 2: acctHasActiveFsm=true, masterHasPosition=false → returns false
   - Test case 3: acctHasActiveFsm=false, masterHasPosition=true → returns false
   - Test case 4: acctHasActiveFsm=false, masterHasPosition=false → returns false

3. **Verify Build**
   - Run `dotnet build` (must succeed)
   - Run `dotnet test` (all tests must pass)
   - Run `dotnet csharpier check src/` (formatting must pass)

### Acceptance Criteria
- [ ] Helper method created with correct signature
- [ ] Helper method is static and private
- [ ] Helper method has CYC 2 (AND condition)
- [ ] 4 unit tests added and passing (truth table coverage)
- [ ] Build succeeds with zero errors
- [ ] CSharpier formatting passes
- [ ] No behavioral changes to main method (not yet refactored)

### Dependencies
- TICKET-1 must be completed first
- TICKET-2 must be completed first

### Verification Commands
```powershell
dotnet build
dotnet test --filter "FullyQualifiedName~OrderValidationTests"
dotnet csharpier check src/V12_002.UI.IPC.Commands.Fleet.cs
```

---

## TICKET-4: Refactor Main Method to Use Helpers

### Scope
- **Current Method**: `CancelAll_ProcessSingleFleetAccount`
- **Current CYC**: 18
- **Target CYC**: 5 (72% reduction)
- **Refactoring**: Replace inline logic with helper method calls

### Implementation
1. **Refactor Main Method**
   - Replace order state validation with `IsOrderCancellable(order, acct.Instrument)`
   - Replace order name prefix validation with `IsBracketOrder(order.Name)`
   - Replace bracket preservation logic with `ShouldPreserveBracket(acctHasActiveFsm, masterHasPosition)`
   - Maintain existing loop structure and CancelOrderOnAccount calls

2. **Expected Code Structure**
   ```csharp
   private int CancelAll_ProcessSingleFleetAccount(Account acct, bool masterHasPosition)
   {
       int cancelledCount = 0;
       var acctHasActiveFsm = acct.Strategies.OfType<V12_002>().FirstOrDefault() != null;
       
       foreach (Order order in acct.Orders)
       {
           if (order == null || order.Instrument != acct.Instrument)
               continue;
           
           if (!IsOrderCancellable(order, acct.Instrument))
               continue;
           
           if (IsBracketOrder(order.Name) && ShouldPreserveBracket(acctHasActiveFsm, masterHasPosition))
               continue;
           
           CancelOrderOnAccount(acct, order);
           cancelledCount++;
       }
       
       return cancelledCount;
   }
   ```

3. **Add Integration Test** (extend `tests/V12_Performance.Tests/Core/FSMActorTests.cs`)
   - Test case 1: Account with active FSM + master position → preserves brackets
   - Test case 2: Account with active FSM + no master position → cancels brackets
   - Test case 3: Account with no FSM → cancels all cancellable orders
   - Test case 4: Account with non-cancellable orders → skips them

4. **Verify Complexity Reduction**
   - Run `python scripts/complexity_audit.py` (verify CYC ≤ 15)
   - Confirm main method CYC = 5
   - Confirm all helpers CYC ≤ 8

5. **Full Validation**
   - Run `powershell -File .\scripts\build_readiness.ps1`
   - Run `powershell -File .\scripts\pre_push_validation.ps1 -Fast`
   - Run `powershell -File .\deploy-sync.ps1` (sync NinjaTrader hard links)

### Acceptance Criteria
- [ ] Main method refactored to use all 3 helpers
- [ ] Main method CYC reduced to 5 (verified by complexity_audit.py)
- [ ] All helper methods CYC ≤ 8
- [ ] 4 integration tests added and passing
- [ ] All existing tests still pass (no behavioral changes)
- [ ] Build succeeds with zero errors
- [ ] CSharpier formatting passes
- [ ] Pre-push validation passes (fast mode)
- [ ] NinjaTrader hard links synchronized
- [ ] No lock() statements introduced (grep verification)

### Dependencies
- TICKET-1 must be completed first
- TICKET-2 must be completed first
- TICKET-3 must be completed first

### Verification Commands
```powershell
# Complexity check
python scripts/complexity_audit.py

# Build and test
dotnet build
dotnet test

# Lock-free verification
grep -r "lock(" src/V12_002.UI.IPC.Commands.Fleet.cs

# Full validation
powershell -File .\scripts\build_readiness.ps1
powershell -File .\scripts\pre_push_validation.ps1 -Fast
powershell -File .\deploy-sync.ps1
```

---

## Post-Completion Checklist

### Phase 4 Deliverables
- [x] Ticket breakdown created (04-tickets.md)
- [ ] All 4 tickets executed sequentially
- [ ] Complexity reduced from CYC 18 to CYC 5
- [ ] 20 unit tests added (7 + 9 + 4 integration tests)
- [ ] Build passes with zero errors
- [ ] All tests pass
- [ ] NinjaTrader hard links synchronized

### DNA Compliance Verification
- [ ] Zero lock() statements (grep verification)
- [ ] ASCII-only compliance (no Unicode)
- [ ] All methods CYC ≤ 8 (Jane Street aligned)
- [ ] Pure functions (static helpers, no side effects)

### PR Hygiene Verification
- [ ] Diff size < 10,000 characters (estimated 450 chars)
- [ ] Single method focus (no scope creep)
- [ ] No breaking changes (internal refactoring only)
- [ ] Whitespace mutation minimal (CSharpier auto-formats)

---

**Epic**: EPIC-CCN-015
**Phase**: 4.0 (Ticket Generation)
**Status**: COMPLETE
**Date**: 2026-06-15
**Total Tickets**: 4
**Estimated Effort**: 6 hours
**Next Phase**: 5.0 (Ticket Execution)
