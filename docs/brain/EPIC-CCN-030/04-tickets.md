# Extraction Tickets: EPIC-CCN-030

## Overview
- **Epic ID**: EPIC-CCN-030
- **Target Method**: ValidateOrphanedMasterOrders
- **File**: src/V12_002.Orders.Management.Cleanup.cs
- **Current Complexity**: 19 (CYC)
- **Target Complexity**: 4 (CYC)
- **Complexity Reduction**: 79%
- **Total Tickets**: 3
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3)
- **Estimated Effort**: 2-3 hours

## Extraction Strategy

**Approach**: Extract filtering and parsing logic into pure helper methods, leaving orchestration in main method.

**Rationale**:
- Filtering logic is a pure predicate (no side effects)
- Parsing logic is pure string manipulation (deterministic)
- Orchestration logic coordinates helpers and performs side effects
- Each extracted method has single responsibility (Jane Street alignment)

---

## TICKET-1: Extract Order Filtering Logic

### Scope
- **Current Method**: `ValidateOrphanedMasterOrders`
- **Current CYC**: 19
- **Target CYC**: N/A (helper method)
- **Helper CYC**: 4
- **Extraction**: Order validation predicate

### Purpose
Extract order filtering logic into a pure helper method that determines if an order is eligible for orphan validation. This is a pure predicate with no side effects.

### Implementation

**New Method Signature**:
```csharp
private bool IsValidOrderForValidation(Order order)
```

**Logic to Extract** (from lines 4-17 of original method):
1. Null check: return false if order is null
2. OrderState validation: return false if not Working or Accepted
3. Instrument matching: return false if not THIS instrument
4. Return true if all checks pass

**Code Structure**:
```csharp
private bool IsValidOrderForValidation(Order order)
{
    if (order == null)
    {
        return false;
    }
    
    if (order.OrderState != OrderState.Working && order.OrderState != OrderState.Accepted)
    {
        return false;
    }
    
    if (order.Instrument.FullName != Instrument.FullName)
    {
        return false;
    }
    
    return true;
}
```

**Placement**: Add as private method in V12_002.Orders.Management.Cleanup.cs, before ValidateOrphanedMasterOrders

**LOC**: ~10 lines
**Access Modifier**: private
**Return Type**: bool
**Parameters**: Order order
**Side Effects**: None (pure function)

### Unit Tests

**Test File**: tests/V12_Performance.Tests/Orders/OrphanValidationTests.cs

**Test Cases** (4 tests):
1. `IsValidOrderForValidation_NullOrder_ReturnsFalse`
   - Input: null
   - Expected: false

2. `IsValidOrderForValidation_WrongState_ReturnsFalse`
   - Input: Order with OrderState.Filled
   - Expected: false

3. `IsValidOrderForValidation_WrongInstrument_ReturnsFalse`
   - Input: Order with different Instrument.FullName
   - Expected: false

4. `IsValidOrderForValidation_ValidOrder_ReturnsTrue`
   - Input: Order with OrderState.Working and matching Instrument
   - Expected: true

### Acceptance Criteria
- [ ] Helper method created with CYC = 4
- [ ] Method is private and pure (no side effects)
- [ ] 4 unit tests added with 100% coverage
- [ ] All tests pass
- [ ] Build succeeds
- [ ] CSharpier formatting applied
- [ ] No behavioral changes to original method

### Dependencies
- None (first ticket)

### Verification Commands
```powershell
# Format code
dotnet csharpier format src/

# Build
dotnet build src/V12_002.csproj

# Run tests
dotnet test tests/V12_Performance.Tests/V12_Performance.Tests.csproj --filter "FullyQualifiedName~OrphanValidationTests"

# Complexity check
python scripts/complexity_audit.py
```

---

## TICKET-2: Extract Name Parsing Logic

### Scope
- **Current Method**: `ValidateOrphanedMasterOrders`
- **Current CYC**: 19
- **Target CYC**: N/A (helper method)
- **Helper CYC**: 5
- **Extraction**: Entry name extraction from order names

### Purpose
Extract name parsing logic into a pure helper method that extracts entry identifiers from order names by parsing prefixes and stripping timestamps. This is a pure function with deterministic output.

### Implementation

**New Method Signature**:
```csharp
private string ExtractEntryNameFromOrder(string orderName)
```

**Logic to Extract** (from lines 19-42 of original method):
1. Check for prefix signatures (Stop_, T1_, T2_, T3_, T4_, T5_, Flatten_, Trim_)
2. Return empty string if no prefix match
3. Extract entry name after first underscore
4. Strip timestamp if present (last underscore followed by >10 chars)
5. Return extracted entry name

**Code Structure**:
```csharp
private string ExtractEntryNameFromOrder(string orderName)
{
    if (string.IsNullOrEmpty(orderName))
    {
        return string.Empty;
    }
    
    // Check for known prefixes
    if (!orderName.StartsWith("Stop_") && 
        !orderName.StartsWith("T1_") && 
        !orderName.StartsWith("T2_") && 
        !orderName.StartsWith("T3_") && 
        !orderName.StartsWith("T4_") && 
        !orderName.StartsWith("T5_") && 
        !orderName.StartsWith("Flatten_") && 
        !orderName.StartsWith("Trim_"))
    {
        return string.Empty;
    }
    
    // Extract entry name after first underscore
    int firstUnderscore = orderName.IndexOf('_');
    if (firstUnderscore < 0 || firstUnderscore >= orderName.Length - 1)
    {
        return string.Empty;
    }
    
    string entryName = orderName.Substring(firstUnderscore + 1);
    
    // Strip timestamp if present (last underscore followed by >10 chars)
    int lastUnderscore = entryName.LastIndexOf('_');
    if (lastUnderscore > 0 && entryName.Length - lastUnderscore > 10)
    {
        entryName = entryName.Substring(0, lastUnderscore);
    }
    
    return entryName;
}
```

**Placement**: Add as private method in V12_002.Orders.Management.Cleanup.cs, after IsValidOrderForValidation

**LOC**: ~20 lines
**Access Modifier**: private
**Return Type**: string
**Parameters**: string orderName
**Side Effects**: None (pure function)

### Unit Tests

**Test File**: tests/V12_Performance.Tests/Orders/OrphanValidationTests.cs

**Test Cases** (4 tests):
1. `ExtractEntryNameFromOrder_NoPrefix_ReturnsEmpty`
   - Input: "InvalidOrderName"
   - Expected: string.Empty

2. `ExtractEntryNameFromOrder_StopPrefix_ReturnsEntryName`
   - Input: "Stop_MyEntry"
   - Expected: "MyEntry"

3. `ExtractEntryNameFromOrder_WithTimestamp_StripsTimestamp`
   - Input: "T1_MyEntry_20260615123456789"
   - Expected: "MyEntry"

4. `ExtractEntryNameFromOrder_NoUnderscore_ReturnsEmpty`
   - Input: "Stop"
   - Expected: string.Empty

### Acceptance Criteria
- [ ] Helper method created with CYC = 5
- [ ] Method is private and pure (no side effects)
- [ ] 4 unit tests added with 100% coverage
- [ ] All tests pass
- [ ] Build succeeds
- [ ] CSharpier formatting applied
- [ ] No behavioral changes to original method

### Dependencies
- TICKET-1 must be completed first (establishes test file structure)

### Verification Commands
```powershell
# Format code
dotnet csharpier format src/

# Build
dotnet build src/V12_002.csproj

# Run tests
dotnet test tests/V12_Performance.Tests/V12_Performance.Tests.csproj --filter "FullyQualifiedName~OrphanValidationTests"

# Complexity check
python scripts/complexity_audit.py
```

---

## TICKET-3: Refactor Main Method

### Scope
- **Current Method**: `ValidateOrphanedMasterOrders`
- **Current CYC**: 19
- **Target CYC**: 4
- **Complexity Reduction**: 79%
- **Extraction**: Refactor to use helper methods

### Purpose
Refactor the main ValidateOrphanedMasterOrders method to use the extracted helper methods, reducing complexity from 19 to 4 while preserving exact behavior.

### Implementation

**Refactored Method Structure**:
```csharp
private bool ValidateOrphanedMasterOrders(string reason)
{
    bool foundOrphans = false;
    
    foreach (Order order in Account.Orders)
    {
        // Use helper method for validation
        if (!IsValidOrderForValidation(order))
        {
            continue;
        }
        
        // Use helper method for name parsing
        string entryName = ExtractEntryNameFromOrder(order.Name);
        if (string.IsNullOrEmpty(entryName))
        {
            continue;
        }
        
        // Check for orphaned entry
        if (!activePositions.ContainsKey(entryName))
        {
            CancelOrderOnAccount(order, Account);
            foundOrphans = true;
            Log($"Cancelled orphaned order: {order.Name} (reason: {reason})", LogLevel.Information);
        }
    }
    
    return foundOrphans;
}
```

**Changes**:
1. Replace inline filtering logic with IsValidOrderForValidation() call
2. Replace inline parsing logic with ExtractEntryNameFromOrder() call
3. Preserve orchestration logic (iteration, cancellation, logging)
4. Maintain exact same behavior and return value

**LOC**: ~15 lines (net change: -17 lines from original 32)
**CYC**: 4 (3 if statements + 1 base path)

### Integration Testing

**Existing Tests**: Verify behavior preservation through existing integration test suite
- No new integration tests required (behavior unchanged)
- Run full test suite to ensure no regressions

### Acceptance Criteria
- [ ] Main method refactored to use helper methods
- [ ] Method complexity reduced to CYC = 4
- [ ] Exact behavior preserved (no logic changes)
- [ ] All existing integration tests pass
- [ ] All new unit tests pass (8 total from TICKET-1 and TICKET-2)
- [ ] Build succeeds with zero errors
- [ ] CSharpier formatting applied
- [ ] Complexity audit passes (CYC ≤ 15)
- [ ] Pre-push validation passes
- [ ] Hard-link sync completed (deploy-sync.ps1)
- [ ] F5 verification in NinjaTrader succeeds

### Dependencies
- TICKET-1 must be completed (IsValidOrderForValidation helper)
- TICKET-2 must be completed (ExtractEntryNameFromOrder helper)

### Verification Commands
```powershell
# Format code
dotnet csharpier format src/

# Build readiness (includes formatting check)
powershell -File .\scripts\build_readiness.ps1

# Run all tests
dotnet test

# Complexity audit
python scripts/complexity_audit.py

# Pre-push validation (full suite)
powershell -File .\scripts\pre_push_validation.ps1

# Hard-link sync
powershell -File .\deploy-sync.ps1

# F5 in NinjaTrader (manual verification)
# Load strategy and verify no errors
```

---

## Quality Gates

### Pre-Implementation Checklist
- [x] Architecture plan reviewed (Phase 2)
- [x] DNA & PR audit passed (Phase 3)
- [x] Tickets generated (Phase 4)
- [x] Extraction strategy validated
- [x] Test plan documented

### Implementation Checklist (Per Ticket)
- [ ] Code changes implemented
- [ ] Unit tests added (100% coverage on helpers)
- [ ] CSharpier formatting applied
- [ ] Build succeeds
- [ ] All tests pass
- [ ] Complexity audit passes

### Final Verification Checklist (After TICKET-3)
- [ ] Complexity target met (19 → 4, 79% reduction)
- [ ] Lock-free compliance verified (zero lock() blocks)
- [ ] ASCII-only compliance verified
- [ ] Jane Street alignment verified (CYC ≤ 8)
- [ ] Test coverage: 100% on helpers
- [ ] Integration tests pass (behavior preserved)
- [ ] Pre-push validation passes (all 13 checks)
- [ ] Hard-link sync completed
- [ ] F5 verification in NinjaTrader succeeds
- [ ] BUILD_TAG verified

## Success Metrics

### Complexity Reduction
- **Before**: CYC = 19
- **After**: CYC = 4
- **Reduction**: 79%
- **Target**: ≤8 (Jane Street strict standard)
- **Status**: ✅ EXCEEDS TARGET

### Code Quality
- **Helper Methods**: 2 pure functions
- **Test Coverage**: 100% on helpers (8 unit tests)
- **LOC Change**: -17 lines (net reduction)
- **Lock-Free**: Zero lock() blocks
- **ASCII-Only**: Compliant

### Jane Street Alignment
- ✅ Cognitive simplicity (CYC ≤8)
- ✅ Testability (pure functions)
- ✅ Correctness by construction (type safety)
- ✅ Zero performance penalty (JIT inlining)
- ✅ Microsecond-safe (no new allocations)

## Risk Mitigation

### Technical Risks
- **Order Filtering Logic**: Mitigated by pure predicate with exhaustive unit tests
- **Name Parsing Edge Cases**: Mitigated by pure function with 100% test coverage
- **Performance Impact**: None (JIT compiler will inline small helpers)
- **Test Coverage Gap**: Addressed by adding 8 unit tests

### Process Risks
- **Scope Creep**: None (single-method extraction with clear boundaries)
- **Regression**: Mitigated by integration tests and F5 verification

## Next Phase

**Phase 5: Ticket Execution**
- Execute TICKET-1 (Extract IsValidOrderForValidation)
- Execute TICKET-2 (Extract ExtractEntryNameFromOrder)
- Execute TICKET-3 (Refactor main method)
- Run quality gates after each ticket
- Final verification after TICKET-3

**Assigned To**: Bob CLI (v12-engineer) or Codex CLI (codex-rescue)

---

**Phase 4 Status**: ✅ COMPLETED
**Tickets Generated**: 3
**Total Effort**: 2-3 hours
**Ready for Phase 5**: YES
**Date**: 2026-06-15
