# Phase 4: Implementation Tickets - EPIC-CCN-119

## Epic Metadata
- **Epic ID**: EPIC-CCN-119
- **Method**: EmergencyFlattenSingleFleetAccount
- **File**: src/V12_002.SIMA.Flatten.cs
- **Lines**: 312-403 (92 lines)
- **Current Complexity**: 16
- **Target Complexity**: ≤ 8 (Jane Street HFT standard)
- **Ticket Count**: 2 (surgical extractions)
- **Execution Order**: Sequential (Ticket 1 → Ticket 2)

---

## Execution Order

```mermaid
graph LR
    A[Pre-Validation] --> B[Ticket 1: Extract Cancel Logic]
    B --> C[Ticket 2: Extract Close Logic]
    C --> D[Post-Validation]
    D --> E[Sign-off]
    
    style A fill:#87ceeb
    style B fill:#90ee90
    style C fill:#90ee90
    style D fill:#87ceeb
    style E fill:#ffd700
```

**Dependencies**:
- Ticket 1 has NO dependencies (can execute immediately after pre-validation)
- Ticket 2 depends on Ticket 1 completion (sequential execution required)

---

## Ticket 1: Extract CancelWorkingOrdersForEmergency

### Metadata
- **Ticket ID**: EPIC-CCN-119-T1
- **Type**: Surgical Extraction
- **Priority**: P5 (Surgical)
- **Estimated Effort**: 45 minutes
- **Complexity Reduction**: -5 points (16 → 11)
- **Dependencies**: None
- **Assigned Agent**: Bob CLI (v12-engineer)

### Method Signature

```csharp
/// <summary>
/// Cancels all working orders on the instrument for the specified account.
/// Returns the count of orders cancelled.
/// </summary>
/// <param name="acct">Fleet account to cancel orders for</param>
/// <returns>Number of orders cancelled</returns>
private int CancelWorkingOrdersForEmergency(Account acct)
```

### Extraction Steps

**Step 1: Create New Method**
1. Navigate to line 403 in src/V12_002.SIMA.Flatten.cs
2. Insert new private method after EmergencyFlattenSingleFleetAccount
3. Add XML documentation comment (3 lines)
4. Add method signature: `private int CancelWorkingOrdersForEmergency(Account acct)`
5. Open method body with `{`

**Step 2: Copy Logic from Original Method**
1. Copy lines 324-351 from EmergencyFlattenSingleFleetAccount
2. Paste into new method body
3. Verify exact copy (no modifications)
4. Close method body with `}`

**Step 3: Adjust Return Statement**
1. Locate line with `acct.Cancel(ordersToCancel);`
2. After the Print() statement, add: `return ordersToCancel.Count;`
3. After the if block (line ~351), add: `return 0;` (no orders cancelled)

**Step 4: Replace Original Logic**
1. Navigate to line 324 in EmergencyFlattenSingleFleetAccount
2. Delete lines 324-351 (28 lines)
3. Replace with single line: `int cancelledCount = CancelWorkingOrdersForEmergency(acct);`
4. Verify indentation matches surrounding code

**Step 5: Format and Verify**
1. Run CSharpier: `dotnet csharpier format src/V12_002.SIMA.Flatten.cs`
2. Verify no compilation errors: `dotnet build src/V12_002.SIMA.Flatten.cs`
3. Run complexity audit: `python scripts/complexity_audit.py | grep -A 5 "EmergencyFlattenSingleFleetAccount"`
4. Verify main method CYC reduced to ~11

### Test Requirements

**Unit Tests** (add to tests/V12_Performance.Tests/Core/FSMActorTests.cs or new file):

```csharp
[Test]
public void CancelWorkingOrdersForEmergency_NoOrders_ReturnsZero()
{
    // Arrange: Account with no orders
    // Act: Call CancelWorkingOrdersForEmergency
    // Assert: Returns 0
}

[Test]
public void CancelWorkingOrdersForEmergency_MultipleWorkingOrders_ReturnsCount()
{
    // Arrange: Account with 3 working orders on instrument
    // Act: Call CancelWorkingOrdersForEmergency
    // Assert: Returns 3, all orders cancelled
}

[Test]
public void CancelWorkingOrdersForEmergency_MixedOrderStates_OnlyCancelsWorking()
{
    // Arrange: Account with 2 working, 1 filled, 1 cancelled orders
    // Act: Call CancelWorkingOrdersForEmergency
    // Assert: Returns 2, only working orders cancelled
}

[Test]
public void CancelWorkingOrdersForEmergency_WrongInstrument_ReturnsZero()
{
    // Arrange: Account with orders on different instrument
    // Act: Call CancelWorkingOrdersForEmergency
    // Assert: Returns 0, no orders cancelled
}

[Test]
public void CancelWorkingOrdersForEmergency_AllOrderStates_CancelsCorrectly()
{
    // Arrange: Account with orders in all 5 cancellable states
    // Act: Call CancelWorkingOrdersForEmergency
    // Assert: Returns 5, all cancellable states handled
}
```

### Verification Criteria

**Pre-Extraction Checks**:
- [ ] Current EmergencyFlattenSingleFleetAccount CYC is 16
- [ ] Lines 324-351 contain order cancellation logic
- [ ] All tests pass: `dotnet test`
- [ ] Git checkpoint created: `git add -A && git commit -m "EPIC-CCN-119-T1: Pre-extraction checkpoint"`

**Post-Extraction Checks**:
- [ ] New method CancelWorkingOrdersForEmergency exists
- [ ] New method CYC ≤ 8 (target: 7)
- [ ] Main method CYC reduced to ~11
- [ ] Zero compilation errors: `dotnet build`
- [ ] All tests pass: `dotnet test`
- [ ] CSharpier compliant: `dotnet csharpier check src/V12_002.SIMA.Flatten.cs`
- [ ] Behavioral preservation: Same execution flow as original
- [ ] Git checkpoint created: `git add -A && git commit -m "EPIC-CCN-119-T1: Extracted CancelWorkingOrdersForEmergency"`

**Complexity Verification**:
```bash
# Expected output after extraction
python scripts/complexity_audit.py | grep -A 10 "EmergencyFlattenSingleFleetAccount"

# Should show:
# EmergencyFlattenSingleFleetAccount: CYC 11 (reduced from 16)
# CancelWorkingOrdersForEmergency: CYC 7 (new method)
```

### Rollback Steps

**If extraction fails**:
1. Run: `git reset --hard HEAD~1` (revert to pre-extraction checkpoint)
2. Verify rollback: `python scripts/complexity_audit.py | grep "EmergencyFlattenSingleFleetAccount"`
3. Should show CYC 16 (original state)
4. Report failure to Director with error details

### Estimated Complexity Reduction

- **Before**: EmergencyFlattenSingleFleetAccount CYC 16
- **After**: EmergencyFlattenSingleFleetAccount CYC 11, CancelWorkingOrdersForEmergency CYC 7
- **Reduction**: -5 points (31% reduction)
- **Progress**: 5/12 points toward target (42%)

---

## Ticket 2: Extract ClosePositionForEmergency

### Metadata
- **Ticket ID**: EPIC-CCN-119-T2
- **Type**: Surgical Extraction
- **Priority**: P5 (Surgical)
- **Estimated Effort**: 60 minutes
- **Complexity Reduction**: -7 points (11 → 4)
- **Dependencies**: Ticket 1 (EPIC-CCN-119-T1) must be completed
- **Assigned Agent**: Bob CLI (v12-engineer)

### Method Signature

```csharp
/// <summary>
/// Closes any open position on the instrument for the specified account.
/// Returns true if a position was closed, false if already flat.
/// </summary>
/// <param name="acct">Fleet account to close position for</param>
/// <returns>True if position closed, false if already flat</returns>
private bool ClosePositionForEmergency(Account acct)
```

### Extraction Steps

**Step 1: Create New Method**
1. Navigate to end of CancelWorkingOrdersForEmergency method
2. Insert new private method after CancelWorkingOrdersForEmergency
3. Add XML documentation comment (3 lines)
4. Add method signature: `private bool ClosePositionForEmergency(Account acct)`
5. Open method body with `{`

**Step 2: Copy Logic from Original Method**
1. Copy lines 353-394 from EmergencyFlattenSingleFleetAccount
2. Paste into new method body
3. Verify exact copy (no modifications)
4. Close method body with `}`

**Step 3: Adjust Return Statements**
1. Locate the if (pos != null) block
2. After the Print() statement inside the if block, add: `return true;`
3. In the else block, after the Print() statement, add: `return false;`
4. Verify both branches return bool

**Step 4: Replace Original Logic**
1. Navigate to line 353 in EmergencyFlattenSingleFleetAccount (after Ticket 1 changes)
2. Delete lines 353-394 (42 lines)
3. Replace with single line: `bool positionClosed = ClosePositionForEmergency(acct);`
4. Verify indentation matches surrounding code

**Step 5: Format and Verify**
1. Run CSharpier: `dotnet csharpier format src/V12_002.SIMA.Flatten.cs`
2. Verify no compilation errors: `dotnet build src/V12_002.SIMA.Flatten.cs`
3. Run complexity audit: `python scripts/complexity_audit.py | grep -A 10 "EmergencyFlattenSingleFleetAccount"`
4. Verify main method CYC reduced to 4

### Test Requirements

**Unit Tests** (add to tests/V12_Performance.Tests/Core/FSMActorTests.cs or new file):

```csharp
[Test]
public void ClosePositionForEmergency_NoPosition_ReturnsFalse()
{
    // Arrange: Account with no open position
    // Act: Call ClosePositionForEmergency
    // Assert: Returns false, no order submitted
}

[Test]
public void ClosePositionForEmergency_LongPosition_ClosesWithSell()
{
    // Arrange: Account with long position (10 contracts)
    // Act: Call ClosePositionForEmergency
    // Assert: Returns true, Sell order submitted for 10 contracts
}

[Test]
public void ClosePositionForEmergency_ShortPosition_ClosesWithBuyToCover()
{
    // Arrange: Account with short position (5 contracts)
    // Act: Call ClosePositionForEmergency
    // Assert: Returns true, BuyToCover order submitted for 5 contracts
}

[Test]
public void ClosePositionForEmergency_WrongInstrument_ReturnsFalse()
{
    // Arrange: Account with position on different instrument
    // Act: Call ClosePositionForEmergency
    // Assert: Returns false, no order submitted
}

[Test]
public void ClosePositionForEmergency_AlreadyFlat_ReturnsFalse()
{
    // Arrange: Account with flat position (MarketPosition.Flat)
    // Act: Call ClosePositionForEmergency
    // Assert: Returns false, no order submitted
}
```

### Verification Criteria

**Pre-Extraction Checks**:
- [ ] Ticket 1 (EPIC-CCN-119-T1) completed successfully
- [ ] Current EmergencyFlattenSingleFleetAccount CYC is 11
- [ ] Lines 353-394 contain position closing logic
- [ ] All tests pass: `dotnet test`
- [ ] Git checkpoint created: `git add -A && git commit -m "EPIC-CCN-119-T2: Pre-extraction checkpoint"`

**Post-Extraction Checks**:
- [ ] New method ClosePositionForEmergency exists
- [ ] New method CYC ≤ 8 (target: 5)
- [ ] Main method CYC reduced to 4 (TARGET MET)
- [ ] Zero compilation errors: `dotnet build`
- [ ] All tests pass: `dotnet test`
- [ ] CSharpier compliant: `dotnet csharpier check src/V12_002.SIMA.Flatten.cs`
- [ ] Behavioral preservation: Same execution flow as original
- [ ] Git checkpoint created: `git add -A && git commit -m "EPIC-CCN-119-T2: Extracted ClosePositionForEmergency"`

**Complexity Verification**:
```bash
# Expected output after extraction
python scripts/complexity_audit.py | grep -A 15 "EmergencyFlattenSingleFleetAccount"

# Should show:
# EmergencyFlattenSingleFleetAccount: CYC 4 (reduced from 16) ✅ TARGET MET
# CancelWorkingOrdersForEmergency: CYC 7 (extracted)
# ClosePositionForEmergency: CYC 5 (extracted)
```

### Rollback Steps

**If extraction fails**:
1. Run: `git reset --hard HEAD~1` (revert to pre-extraction checkpoint)
2. Verify rollback: `python scripts/complexity_audit.py | grep "EmergencyFlattenSingleFleetAccount"`
3. Should show CYC 11 (post-Ticket-1 state)
4. Report failure to Director with error details

**If both tickets need rollback**:
1. Run: `git reset --hard HEAD~3` (revert to pre-EPIC-CCN-119 state)
2. Verify rollback: `python scripts/complexity_audit.py | grep "EmergencyFlattenSingleFleetAccount"`
3. Should show CYC 16 (original state)

### Estimated Complexity Reduction

- **Before**: EmergencyFlattenSingleFleetAccount CYC 11
- **After**: EmergencyFlattenSingleFleetAccount CYC 4, ClosePositionForEmergency CYC 5
- **Reduction**: -7 points (64% reduction from Ticket 1 state)
- **Total Reduction**: -12 points (75% reduction from original)
- **Progress**: 12/12 points toward target (100%) ✅ TARGET MET

---

## Post-Execution Validation

### Final Verification Checklist

**Complexity Targets**:
- [ ] EmergencyFlattenSingleFleetAccount CYC ≤ 8 (target: 4) ✅
- [ ] CancelWorkingOrdersForEmergency CYC ≤ 8 (target: 7) ✅
- [ ] ClosePositionForEmergency CYC ≤ 8 (target: 5) ✅

**V12 DNA Compliance**:
- [ ] Zero lock() blocks introduced
- [ ] ASCII-only strings maintained
- [ ] Correctness by construction preserved
- [ ] Atomic operations preserved (SetExpectedPositionLocked)

**PR Hygiene**:
- [ ] Diff < 10,000 characters (estimated ~8,100)
- [ ] No whitespace mutation across files
- [ ] No scope creep (single-method boundary)
- [ ] CSharpier formatting compliant

**Testing**:
- [ ] All existing tests pass: `dotnet test`
- [ ] New unit tests added (10 tests minimum)
- [ ] Manual emergency scenario testing (if available)

**Pre-Push Validation** (13 checks):
```bash
powershell -File .\scripts\pre_push_validation.ps1
```
- [ ] Check 1: ASCII-Only (Zero non-ASCII)
- [ ] Check 2: Build (Zero errors)
- [ ] Check 3: Unit Tests (100% pass)
- [ ] Check 4: Lint (Zero violations)
- [ ] Check 5: Formatting (Zero issues)
- [ ] Check 6: Security (Zero secrets)
- [ ] Check 7: Markdown Links (Zero broken)
- [ ] Check 8: PR Hygiene (Diff <10k)
- [ ] Check 9: Complexity (CYC ≤ 15)
- [ ] Check 10: Dead Code (Zero dead methods)
- [ ] Check 11: Codacy Preview (Zero errors)
- [ ] Check 12: Semgrep (Zero findings)
- [ ] Check 13: CodeRabbit AI (Zero critical/high)

**Hard-Link Sync**:
```bash
powershell -File .\deploy-sync.ps1
```
- [ ] Hard-link sync successful
- [ ] DIFF GUARD passed (<10k chars)
- [ ] NinjaTrader bin/ updated

**Build Readiness**:
```bash
powershell -File .\scripts\build_readiness.ps1
```
- [ ] Build successful
- [ ] CSharpier check passed
- [ ] Zero compilation errors

---

## Success Criteria

### Primary Success Criteria (All Must Pass)

1. **Complexity Reduction**: EmergencyFlattenSingleFleetAccount CYC reduced from 16 to ≤ 8 (target: 4)
2. **Helper Method Compliance**: Both extracted methods CYC ≤ 8
3. **Behavioral Preservation**: Exact same execution flow as original
4. **Zero Regressions**: All tests pass, zero compilation errors
5. **V12 DNA Compliance**: No locks, ASCII-only, correctness by construction
6. **PR Hygiene**: Diff < 10k chars, no whitespace mutation, no scope creep

### Secondary Success Criteria (Recommended)

1. **Test Coverage**: 10+ unit tests added for extracted methods
2. **Documentation**: XML comments added to all new methods
3. **Jane Street Alignment**: Cognitive simplicity, single responsibility, testability
4. **Pre-Push Validation**: All 13 checks pass
5. **Hard-Link Sync**: deploy-sync.ps1 successful

### Failure Criteria (Any Triggers Rollback)

1. **Compilation Errors**: Any build errors after extraction
2. **Test Failures**: Any test failures after extraction
3. **Complexity Overshoot**: Any method CYC > 8 after extraction
4. **Behavioral Changes**: Any deviation from original execution flow
5. **V12 DNA Violations**: Lock() blocks, Unicode strings, illegal states
6. **PR Hygiene Violations**: Diff > 10k chars, whitespace mutation, scope creep

---

## Execution Timeline

### Estimated Timeline (Total: 2-3 hours)

**Pre-Validation** (15 minutes):
- Run complexity audit
- Run full test suite
- Create git checkpoint

**Ticket 1 Execution** (45 minutes):
- Extract CancelWorkingOrdersForEmergency
- Add unit tests
- Verify complexity reduction
- Create git checkpoint

**Ticket 2 Execution** (60 minutes):
- Extract ClosePositionForEmergency
- Add unit tests
- Verify complexity reduction
- Create git checkpoint

**Post-Validation** (30 minutes):
- Run pre-push validation (13 checks)
- Run build readiness
- Run deploy-sync
- Manual emergency scenario testing (if available)

**Documentation** (15 minutes):
- Update manifest.json
- Create verification report
- Request Director sign-off

---

## Risk Mitigation

### Risk 1: Emergency Handler Criticality

**Mitigation**:
- ✅ Extractions preserve exact behavior (no logic changes)
- ✅ Each extraction is independently testable
- ✅ Git checkpoints allow instant rollback
- ✅ Pre-push validation catches regressions

### Risk 2: Test Coverage Unknown

**Mitigation**:
- ✅ Run full test suite before and after each extraction
- ✅ Add 10+ unit tests for extracted methods
- ✅ Manual emergency scenario validation if tests insufficient
- ✅ Behavioral preservation is PRIMARY constraint

### Risk 3: Complexity Calculation Accuracy

**Mitigation**:
- ✅ Manual CYC calculation matches tool output
- ✅ Conservative estimates (7 and 5 vs target 8)
- ✅ Post-refactoring verification required

---

## Appendix A: Refactored Code Preview

### Main Method (After Both Tickets)

```csharp
/// <summary>
/// DEAD-01: Emergency single-account fleet kill. Called when a follower entry fills
/// AFTER the master order is cancelled (CASCADE-FILLED path). Cancels all working orders
/// on the instrument for this account, then submits a Market close if a position exists.
/// Must be called on strategy thread (via TriggerCustomEvent).
/// </summary>
private void EmergencyFlattenSingleFleetAccount(Account acct)
{
    if (acct == null)
        return;
    
    Print(string.Format("[DEAD-01] EmergencyFlatten: Initiating kill for {0}", acct.Name));

    try
    {
        // [938-EF-GUARD] Confirm bracket cancellation precedes market close.
        Print(string.Format("[938-EF-GUARD] EF cancelling bracket first: {0}", acct.Name));

        // Step 1: Cancel ALL working orders on this instrument for this account.
        int cancelledCount = CancelWorkingOrdersForEmergency(acct);

        // Step 2: Close any live position with a Market order.
        bool positionClosed = ClosePositionForEmergency(acct);

        // Phase 5.5: Direct call -- strategy thread (TriggerCustomEvent).
        SetExpectedPositionLocked(ExpKey(acct.Name), 0);
    }
    catch (Exception ex)
    {
        Print(string.Format("[DEAD-01] EmergencyFlatten ERROR on {0}: {1}", acct.Name, ex.Message));
    }
}
```

**CYC**: 4 (guard + try + catch + implicit control flow)

---

**Phase 4 Status**: ✅ COMPLETED
**Next Phase**: Phase 5 (Recursive Execution)
**Assigned Agent**: Bob CLI (v12-engineer)
**Ticket Count**: 2 (sequential execution)
**Estimated Effort**: 2-3 hours
**Risk Level**: LOW-MEDIUM

