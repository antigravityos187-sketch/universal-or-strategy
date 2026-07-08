# Phase 4: Implementation Tickets - EPIC-CCN-108

## Epic Context
- **Epic ID**: EPIC-CCN-108
- **Phase**: 4 (Ticket Generation)
- **Date**: 2026-06-13
- **Target Method**: `SweepBrokerOrders`
- **File**: `src/V12_002.SIMA.Lifecycle.cs`
- **Current CCN**: ~18
- **Target CCN**: ≤12 (actual target: ~6)

---

## Execution Summary

**Total Tickets**: 5 (3 extractions + 1 test suite + 1 verification)

**Execution Order**:
1. TICKET-108-0: Create Test Suite (MANDATORY FIRST)
2. TICKET-108-1: Extract IsOrderCancellable
3. TICKET-108-2: Extract TryCancelBrokerOrder
4. TICKET-108-3: Extract ProcessAccountOrders
5. TICKET-108-4: Final Verification & Documentation

**Estimated Total Time**: 4.5 hours

**Success Criteria**:
- ✅ SweepBrokerOrders CCN ≤12 (target: ~6)
- ✅ All helpers CCN ≤8
- ✅ 100% test coverage for extracted methods
- ✅ Zero compilation errors
- ✅ All existing tests pass
- ✅ Lock-free compliance verified
- ✅ ASCII-only compliance verified

---

## TICKET-108-0: Create Test Suite (MANDATORY)

### Priority: P0 (BLOCKING)

### Description
Create comprehensive unit test suite for SweepBrokerOrders and all extracted methods. This ticket MUST be completed before any extractions begin (Phase 3 audit requirement).

### Method Signature
N/A (Test creation only)

### Extraction Steps

#### Step 1: Create Test File
```bash
mkdir -p tests/V12_Performance.Tests/Lifecycle
touch tests/V12_Performance.Tests/Lifecycle/SweepBrokerOrdersTests.cs
```

#### Step 2: Implement Test Class Structure
- Create test fixtures for IsOrderCancellable
- Create test fixtures for TryCancelBrokerOrder
- Create test fixtures for ProcessAccountOrders
- Create integration tests for SweepBrokerOrders

#### Step 3: Run Test Suite
```bash
dotnet test tests/V12_Performance.Tests/Lifecycle/SweepBrokerOrdersTests.cs
```

### Test Requirements
- ✅ 100% line coverage for all extracted methods
- ✅ Test all 5 valid OrderState values
- ✅ Test invalid OrderState values (Filled, Cancelled, Rejected)
- ✅ Test exception handling in TryCancelBrokerOrder
- ✅ Test force=true and force=false scenarios
- ✅ Test bracket protection logic
- ✅ Test multi-account aggregation

### Verification Criteria
- [ ] Test file created at correct path
- [ ] All test methods compile without errors
- [ ] All tests pass (green)
- [ ] Code coverage report shows 100% for target methods
- [ ] No test flakiness (run 3 times, all pass)

### Estimated Complexity Reduction
N/A (Test creation only)

### Rollback Steps
```bash
git checkout HEAD -- tests/V12_Performance.Tests/Lifecycle/SweepBrokerOrdersTests.cs
rm tests/V12_Performance.Tests/Lifecycle/SweepBrokerOrdersTests.cs
```

### Dependencies
- None (first ticket)

### Estimated Time
1.5 hours

### Risk Level
LOW (test creation, no production code changes)

---

## TICKET-108-1: Extract IsOrderCancellable

### Priority: P1 (After TICKET-108-0)

### Description
Extract the 5-condition OrderState validation guard into a dedicated helper method. This is the lowest-risk extraction and reduces main method CCN by ~5.

### Method Signature
```csharp
private bool IsOrderCancellable(OrderState state)
```

### Extraction Steps

#### Step 1: Create Helper Method (Line 1389)
Insert after ShouldProtectBracketOrder method with XML documentation and implementation.

#### Step 2: Replace Call Site (Lines 1308-1314)
Replace 5-condition guard with single call: `if (!IsOrderCancellable(ord.OrderState)) continue;`

#### Step 3: Run Tests
```bash
dotnet test tests/V12_Performance.Tests/Lifecycle/SweepBrokerOrdersTests.cs
```

#### Step 4: Verify CCN Reduction
```bash
lizard src/V12_002.SIMA.Lifecycle.cs -l csharp | grep SweepBrokerOrders
# Expected: CCN reduced from ~18 to ~13
```

#### Step 5: Run Build Readiness
```bash
powershell -File .\scripts\build_readiness.ps1
```

#### Step 6: Commit Changes
```bash
git add src/V12_002.SIMA.Lifecycle.cs
git commit -m "EPIC-CCN-108: Extract IsOrderCancellable"
```

### Test Requirements
- ✅ Run IsOrderCancellable unit tests
- ✅ Run SweepBrokerOrders integration tests
- ✅ Verify all 5 valid states return true
- ✅ Verify invalid states return false

### Verification Criteria
- [ ] Method created at line 1389
- [ ] Call site replaced at lines 1308-1314
- [ ] All tests pass (green)
- [ ] CCN reduced: ~18 → ~13
- [ ] IsOrderCancellable CCN ≤2
- [ ] Build succeeds with 0 errors
- [ ] No lock keywords introduced
- [ ] ASCII-only compliance maintained

### Estimated Complexity Reduction
- **Main Method**: -5 CCN (18 → 13)
- **Helper Method**: +1 CCN (new method)
- **Net System CCN**: -4 CCN

### Rollback Steps
```bash
git reset --hard HEAD~1
```

### Dependencies
- ✅ TICKET-108-0 (test suite) MUST be completed first

### Estimated Time
30 minutes

### Risk Level
LOW (pure validation logic, no side effects)

---

## TICKET-108-2: Extract TryCancelBrokerOrder

### Priority: P2 (After TICKET-108-1)

### Description
Extract order cancellation logic with error handling into a dedicated helper method. Reduces main method CCN by ~2 and improves error handling isolation.

### Method Signature
```csharp
private bool TryCancelBrokerOrder(Account account, Order order, ref int cancelCount)
```

### Extraction Steps

#### Step 1: Create Helper Method (Line 1403)
Insert after IsOrderCancellable method with XML documentation and implementation.

#### Step 2: Replace Call Site (Lines 1326-1336)
Replace try-catch block with single call: `TryCancelBrokerOrder(acct, ord, ref brokerCancels);`

#### Step 3: Run Tests
```bash
dotnet test tests/V12_Performance.Tests/Lifecycle/SweepBrokerOrdersTests.cs
```

#### Step 4: Verify CCN Reduction
```bash
lizard src/V12_002.SIMA.Lifecycle.cs -l csharp | grep SweepBrokerOrders
# Expected: CCN reduced from ~13 to ~11
```

#### Step 5: Run Build Readiness
```bash
powershell -File .\scripts\build_readiness.ps1
```

#### Step 6: Commit Changes
```bash
git add src/V12_002.SIMA.Lifecycle.cs
git commit -m "EPIC-CCN-108: Extract TryCancelBrokerOrder"
```

### Test Requirements
- ✅ Run TryCancelBrokerOrder unit tests
- ✅ Test success case (counter incremented, return true)
- ✅ Test exception case (counter NOT incremented, return false)
- ✅ Run SweepBrokerOrders integration tests

### Verification Criteria
- [ ] Method created at line 1403
- [ ] Call site replaced at lines 1326-1336
- [ ] All tests pass (green)
- [ ] CCN reduced: ~13 → ~11
- [ ] TryCancelBrokerOrder CCN ≤3
- [ ] Build succeeds with 0 errors
- [ ] Exception handling preserved

### Estimated Complexity Reduction
- **Main Method**: -2 CCN (13 → 11)
- **Helper Method**: +2 CCN (new method)
- **Net System CCN**: 0 CCN (complexity moved)

### Rollback Steps
```bash
git reset --hard HEAD~1
```

### Dependencies
- ✅ TICKET-108-0 (test suite) completed
- ✅ TICKET-108-1 (IsOrderCancellable) completed

### Estimated Time
30 minutes

### Risk Level
LOW (encapsulates existing error handling)

---

## TICKET-108-3: Extract ProcessAccountOrders

### Priority: P3 (After TICKET-108-2)

### Description
Extract inner order processing loop into a dedicated helper method. This is the largest extraction and reduces main method CCN by ~6, achieving the target CCN of ~6 for SweepBrokerOrders.

### Method Signature
```csharp
private int ProcessAccountOrders(Account account, string[] v12Prefixes, bool force)
```

### Extraction Steps

#### Step 1: Create Helper Method (Line 1423)
Insert after TryCancelBrokerOrder method with XML documentation and implementation.

#### Step 2: Replace Call Site (Lines 1303-1337)
Replace inner foreach loop with single call: `brokerCancels += ProcessAccountOrders(acct, v12Prefixes, force);`

#### Step 3: Run Tests
```bash
dotnet test tests/V12_Performance.Tests/Lifecycle/SweepBrokerOrdersTests.cs
```

#### Step 4: Verify CCN Reduction
```bash
lizard src/V12_002.SIMA.Lifecycle.cs -l csharp | grep SweepBrokerOrders
# Expected: CCN reduced from ~11 to ~6 (TARGET ACHIEVED)
```

#### Step 5: Run Build Readiness
```bash
powershell -File .\scripts\build_readiness.ps1
```

#### Step 6: Commit Changes
```bash
git add src/V12_002.SIMA.Lifecycle.cs
git commit -m "EPIC-CCN-108: Extract ProcessAccountOrders - TARGET ACHIEVED"
```

### Test Requirements
- ✅ Run ProcessAccountOrders unit tests
- ✅ Test empty orders (return 0)
- ✅ Test mixed V12/non-V12 orders
- ✅ Test force=true (cancel all V12)
- ✅ Test force=false (protect brackets)
- ✅ Run SweepBrokerOrders integration tests

### Verification Criteria
- [ ] Method created at line 1423
- [ ] Call site replaced at lines 1303-1337
- [ ] All tests pass (green)
- [ ] CCN reduced: ~11 → ~6 (TARGET ACHIEVED)
- [ ] ProcessAccountOrders CCN ≤8
- [ ] Build succeeds with 0 errors
- [ ] Account-level exception handling preserved

### Estimated Complexity Reduction
- **Main Method**: -6 CCN (11 → 6) ✅ TARGET ACHIEVED
- **Helper Method**: +6 CCN (new method)
- **Net System CCN**: 0 CCN (complexity moved)

### Rollback Steps
```bash
git reset --hard HEAD~1
```

### Dependencies
- ✅ TICKET-108-0 (test suite) completed
- ✅ TICKET-108-1 (IsOrderCancellable) completed
- ✅ TICKET-108-2 (TryCancelBrokerOrder) completed

### Estimated Time
45 minutes

### Risk Level
MEDIUM (larger extraction, but well-isolated logic)

---

## TICKET-108-4: Final Verification & Documentation

### Priority: P4 (After TICKET-108-3)

### Description
Perform comprehensive verification of all extractions, update documentation, and prepare for merge. This ticket ensures all V12 DNA compliance checks pass and the epic is ready for production.

### Extraction Steps

#### Step 1: Run Full Test Suite
```bash
dotnet test
```

#### Step 2: Verify Complexity Targets
```bash
lizard src/V12_002.SIMA.Lifecycle.cs -l csharp
```

#### Step 3: Run Build Readiness
```bash
powershell -File .\scripts\build_readiness.ps1
```

#### Step 4: Run Hard-Link Sync
```bash
powershell -File .\deploy-sync.ps1
```

#### Step 5: Verify Lock-Free Compliance
```bash
grep -n "lock(" src/V12_002.SIMA.Lifecycle.cs
# Expected: 0 matches
```

#### Step 6: Verify ASCII-Only Compliance
```bash
python check_ascii.py src/V12_002.SIMA.Lifecycle.cs
```

#### Step 7: Check Diff Size
```bash
git diff --stat src/V12_002.SIMA.Lifecycle.cs
git diff src/V12_002.SIMA.Lifecycle.cs | wc -c
# Expected: <10,000 characters
```

#### Step 8: Prepare Merge
```bash
git fetch origin main
git rebase origin/main
git push origin epic-ccn-108-sweep-broker-orders
```

### Verification Criteria
- [ ] Full test suite passes (green)
- [ ] SweepBrokerOrders CCN ≤12 (actual: ~6)
- [ ] All helpers CCN ≤8
- [ ] Build succeeds with 0 errors
- [ ] Hard-link sync successful
- [ ] Lock-free compliance verified
- [ ] ASCII-only compliance verified
- [ ] Diff size <10k characters
- [ ] XML documentation complete
- [ ] Ready for PR

### Dependencies
- ✅ TICKET-108-0 completed
- ✅ TICKET-108-1 completed
- ✅ TICKET-108-2 completed
- ✅ TICKET-108-3 completed

### Estimated Time
30 minutes

### Risk Level
LOW (verification only)

---

## Success Criteria Summary

### Primary Criteria
1. SweepBrokerOrders CCN ≤12 (target: ~6)
2. All helpers CCN ≤8
3. 100% test coverage for extracted methods
4. Zero compilation errors
5. All existing tests pass

### V12 DNA Compliance
6. Lock-free compliance verified
7. ASCII-only compliance verified
8. Jane Street alignment (CCN ≤15)
9. PR hygiene (diff <10k chars)

---

## Document Metadata
- **Document Version**: 1.0
- **Phase**: 4 (Ticket Generation)
- **Status**: COMPLETED
- **Date**: 2026-06-13
- **Epic**: EPIC-CCN-108
- **Total Tickets**: 5
- **Estimated Effort**: 4.5 hours
- **Risk Level**: LOW-MEDIUM (acceptable)
