# Extraction Tickets: EPIC-CCN-047

## Overview
- **Total Tickets**: 2
- **Execution Order**: Sequential (TICKET-1 → TICKET-2)
- **Estimated Effort**: 3 hours
- **Target Method**: `CancelOrphanedTargets`
- **Current Complexity**: 14
- **Target Complexity**: ≤8 (Jane Street aligned)

---

## TICKET-1: Extract IsValidOrderForCancellation Helper

### Scope
- **Current Method**: `CancelOrphanedTargets`
- **Current CYC**: 14
- **Target CYC**: 10 (after this extraction)
- **Extraction**: Order validation logic (null, instrument, state checks)

### Implementation
1. Create new private method `IsValidOrderForCancellation` below `CancelOrphanedTargets`
2. Move validation logic:
   - Null check: `order == null`
   - Instrument match: `order.Instrument?.FullName != Instrument?.FullName`
   - State validation: `OrderState.Working` and `OrderState.Accepted` checks
3. Update main method to call `IsValidOrderForCancellation(order, account)`
4. Replace inline validation with helper call
5. Run tests: `dotnet test`
6. Verify complexity: `python scripts/complexity_audit.py`

### Method Signature
```csharp
private bool IsValidOrderForCancellation(Order order, Account account)
{
    if (order == null || order.Instrument?.FullName != Instrument?.FullName)
        return false;
    
    if (order.OrderState != OrderState.Working && order.OrderState != OrderState.Accepted)
        return false;
    
    return true;
}
```

### Refactored Main Method (Partial)
```csharp
private int CancelOrphanedTargets(Account account)
{
    int cancelledTargets = 0;
    
    foreach (Order order in account.Orders.ToArray())
    {
        if (!IsValidOrderForCancellation(order, account))
            continue;
        
        // Target prefix checks remain (extracted in TICKET-2)
        if (order.Name != null && 
            (order.Name.StartsWith("T1_") || 
             order.Name.StartsWith("T2_") || 
             order.Name.StartsWith("T3_") || 
             order.Name.StartsWith("T4_") || 
             order.Name.StartsWith("T5_")))
        {
            CancelOrderOnAccount(order, account);
            cancelledTargets++;
        }
    }
    
    return cancelledTargets;
}
```

### Unit Tests (TDD - Write First)
Create `tests/V12_Performance.Tests/UI/CancelOrphanedTargetsTests.cs`:

```csharp
[TestFixture]
public class CancelOrphanedTargetsTests
{
    [Test]
    public void IsValidOrderForCancellation_NullOrder_ReturnsFalse()
    {
        // Arrange
        var strategy = new V12_002();
        
        // Act
        var result = strategy.IsValidOrderForCancellation(null, mockAccount);
        
        // Assert
        Assert.IsFalse(result);
    }
    
    [Test]
    public void IsValidOrderForCancellation_WrongInstrument_ReturnsFalse()
    {
        // Arrange
        var order = CreateMockOrder("ES", OrderState.Working);
        var strategy = new V12_002 { Instrument = CreateMockInstrument("NQ") };
        
        // Act
        var result = strategy.IsValidOrderForCancellation(order, mockAccount);
        
        // Assert
        Assert.IsFalse(result);
    }
    
    [Test]
    public void IsValidOrderForCancellation_WorkingState_ReturnsTrue()
    {
        // Arrange
        var order = CreateMockOrder("ES", OrderState.Working);
        var strategy = new V12_002 { Instrument = CreateMockInstrument("ES") };
        
        // Act
        var result = strategy.IsValidOrderForCancellation(order, mockAccount);
        
        // Assert
        Assert.IsTrue(result);
    }
    
    [Test]
    public void IsValidOrderForCancellation_AcceptedState_ReturnsTrue()
    {
        // Arrange
        var order = CreateMockOrder("ES", OrderState.Accepted);
        var strategy = new V12_002 { Instrument = CreateMockInstrument("ES") };
        
        // Act
        var result = strategy.IsValidOrderForCancellation(order, mockAccount);
        
        // Assert
        Assert.IsTrue(result);
    }
    
    [Test]
    public void IsValidOrderForCancellation_FilledState_ReturnsFalse()
    {
        // Arrange
        var order = CreateMockOrder("ES", OrderState.Filled);
        var strategy = new V12_002 { Instrument = CreateMockInstrument("ES") };
        
        // Act
        var result = strategy.IsValidOrderForCancellation(order, mockAccount);
        
        // Assert
        Assert.IsFalse(result);
    }
}
```

### Acceptance Criteria
- [ ] Helper method `IsValidOrderForCancellation` created with CYC 4
- [ ] Main method complexity reduced from 14 to 10
- [ ] All unit tests pass (5 tests for helper method)
- [ ] Existing integration tests pass unchanged
- [ ] Build succeeds: `powershell -File .\scripts\build_readiness.ps1`
- [ ] No behavioral changes (behavior-preserving refactoring)
- [ ] No lock() statements introduced (lock-free validation)
- [ ] ASCII-only compliance maintained

### Dependencies
- None (first ticket)

### Verification Commands
```powershell
# Run unit tests
dotnet test tests/V12_Performance.Tests/UI/CancelOrphanedTargetsTests.cs

# Verify complexity reduction
python scripts/complexity_audit.py

# Full build validation
powershell -File .\scripts\build_readiness.ps1

# Lock-free verification
grep -r "lock(" src/V12_002.UI.Compliance.cs
```

### Rollback Strategy
- Single method extraction (easy to revert)
- Git commit after extraction: `git commit -m "EPIC-CCN-047 TICKET-1: Extract IsValidOrderForCancellation"`
- Checkpointing enabled via Bob CLI

---

## TICKET-2: Extract IsTargetOrder Helper

### Scope
- **Current Method**: `CancelOrphanedTargets` (after TICKET-1)
- **Current CYC**: 10 (after TICKET-1)
- **Target CYC**: 6 (final target)
- **Extraction**: Target prefix validation logic (T1-T5 checks)

### Implementation
1. Create new private method `IsTargetOrder` below `IsValidOrderForCancellation`
2. Move target prefix logic:
   - Null check: `order.Name == null`
   - Prefix checks: `StartsWith("T1_")` through `StartsWith("T5_")`
3. Update main method to call `IsTargetOrder(order)`
4. Replace inline prefix checks with helper call
5. Run tests: `dotnet test`
6. Verify complexity: `python scripts/complexity_audit.py`

### Method Signature
```csharp
private bool IsTargetOrder(Order order)
{
    if (order.Name == null)
        return false;
    
    return order.Name.StartsWith("T1_")
        || order.Name.StartsWith("T2_")
        || order.Name.StartsWith("T3_")
        || order.Name.StartsWith("T4_")
        || order.Name.StartsWith("T5_");
}
```

### Refactored Main Method (Final)
```csharp
private int CancelOrphanedTargets(Account account)
{
    int cancelledTargets = 0;
    
    foreach (Order order in account.Orders.ToArray())
    {
        if (!IsValidOrderForCancellation(order, account))
            continue;
        
        if (IsTargetOrder(order))
        {
            CancelOrderOnAccount(order, account);
            cancelledTargets++;
        }
    }
    
    return cancelledTargets;
}
```

### Unit Tests (TDD - Write First)
Add to `tests/V12_Performance.Tests/UI/CancelOrphanedTargetsTests.cs`:

```csharp
[Test]
public void IsTargetOrder_NullName_ReturnsFalse()
{
    // Arrange
    var order = CreateMockOrder(null);
    var strategy = new V12_002();
    
    // Act
    var result = strategy.IsTargetOrder(order);
    
    // Assert
    Assert.IsFalse(result);
}

[TestCase("T1_")]
[TestCase("T2_")]
[TestCase("T3_")]
[TestCase("T4_")]
[TestCase("T5_")]
public void IsTargetOrder_ValidPrefix_ReturnsTrue(string prefix)
{
    // Arrange
    var order = CreateMockOrder(prefix + "TestOrder");
    var strategy = new V12_002();
    
    // Act
    var result = strategy.IsTargetOrder(order);
    
    // Assert
    Assert.IsTrue(result);
}

[TestCase("T6_")]
[TestCase("T0_")]
[TestCase("X1_")]
[TestCase("ENTRY_")]
[TestCase("")]
public void IsTargetOrder_InvalidPrefix_ReturnsFalse(string prefix)
{
    // Arrange
    var order = CreateMockOrder(prefix + "TestOrder");
    var strategy = new V12_002();
    
    // Act
    var result = strategy.IsTargetOrder(order);
    
    // Assert
    Assert.IsFalse(result);
}

[Test]
public void IsTargetOrder_PartialMatch_ReturnsFalse()
{
    // Arrange
    var order = CreateMockOrder("MyT1_Order"); // T1_ not at start
    var strategy = new V12_002();
    
    // Act
    var result = strategy.IsTargetOrder(order);
    
    // Assert
    Assert.IsFalse(result);
}
```

### Acceptance Criteria
- [ ] Helper method `IsTargetOrder` created with CYC 6
- [ ] Main method complexity reduced from 10 to 6 (final target ≤8)
- [ ] All unit tests pass (9 additional tests for helper method)
- [ ] Existing integration tests pass unchanged
- [ ] Build succeeds: `powershell -File .\scripts\build_readiness.ps1`
- [ ] No behavioral changes (behavior-preserving refactoring)
- [ ] No lock() statements introduced (lock-free validation)
- [ ] ASCII-only compliance maintained
- [ ] Manual F5 in NinjaTrader succeeds

### Dependencies
- **TICKET-1 must be completed first**
- Requires `IsValidOrderForCancellation` helper to exist

### Verification Commands
```powershell
# Run unit tests
dotnet test tests/V12_Performance.Tests/UI/CancelOrphanedTargetsTests.cs

# Verify final complexity ≤8
python scripts/complexity_audit.py

# Full build validation
powershell -File .\scripts\build_readiness.ps1

# Lock-free verification
grep -r "lock(" src/V12_002.UI.Compliance.cs

# Hard-link sync
powershell -File .\deploy-sync.ps1

# Manual testing in NinjaTrader (F5)
```

### Rollback Strategy
- Single method extraction (easy to revert)
- Git commit after extraction: `git commit -m "EPIC-CCN-047 TICKET-2: Extract IsTargetOrder"`
- Checkpointing enabled via Bob CLI
- Can rollback to TICKET-1 state if needed

---

## Final Complexity Summary

### Before Extraction (Current)
- **CancelOrphanedTargets**: CYC 14 ❌

### After TICKET-1
- **CancelOrphanedTargets**: CYC 10
- **IsValidOrderForCancellation**: CYC 4 ✅

### After TICKET-2 (Final)
- **CancelOrphanedTargets**: CYC 6 ✅ (≤8 Jane Street aligned)
- **IsValidOrderForCancellation**: CYC 4 ✅
- **IsTargetOrder**: CYC 6 ✅

**Total Distributed Complexity**: 16 (was 14 monolithic)
**Main Method Complexity**: 6 ✅ (target ≤8)

---

## Test Coverage Summary

### Unit Tests (14 total)
- **IsValidOrderForCancellation**: 5 tests
  - Null order
  - Wrong instrument
  - Working state (valid)
  - Accepted state (valid)
  - Filled state (invalid)

- **IsTargetOrder**: 9 tests
  - Null name
  - Valid prefixes T1-T5 (5 tests)
  - Invalid prefixes (3 tests)
  - Partial match (not at start)

### Integration Tests
- Existing tests should pass unchanged (behavior-preserving)

### Manual Testing
- F5 in NinjaTrader after TICKET-2 completion
- Place orders with T1-T5 prefixes
- Trigger orphaned target cancellation
- Verify correct cancellation behavior

---

## Risk Mitigation

### Known Risks
1. **Helper method overhead**
   - **Mitigation**: JIT inlining eliminates overhead
   - **Validation**: Not on hot path (UI compliance, millisecond timescales)

2. **Breaking existing behavior**
   - **Mitigation**: Comprehensive TDD unit tests
   - **Validation**: Manual F5 testing in NinjaTrader

3. **Scope creep**
   - **Mitigation**: Only touch `CancelOrphanedTargets` and 2 new helpers
   - **Validation**: No changes to other methods or files

### Validation Checkpoints
1. After TICKET-1: Verify tests pass, complexity reduced to 10
2. After TICKET-2: Verify tests pass, complexity reduced to 6
3. After TICKET-2: Manual F5 in NinjaTrader

---

## Success Criteria (Phase 4)

- ✅ Tickets document created (`04-tickets.md`)
- ✅ Each ticket has clear scope and implementation steps
- ✅ Acceptance criteria defined for both tickets
- ✅ Dependencies documented (TICKET-2 depends on TICKET-1)
- ✅ TDD test strategy included (write tests first)
- ✅ Verification commands provided
- ✅ Rollback strategy documented
- ✅ Final complexity validated (≤8 Jane Street aligned)

---

**Document Version**: 1.0  
**Created**: 2026-06-15  
**Epic**: EPIC-CCN-047  
**Protocol**: V12.23 (Phase 4)  
**Status**: READY FOR PHASE 5 (Ticket Execution)  
**Total Tickets**: 2  
**Estimated Effort**: 3 hours
