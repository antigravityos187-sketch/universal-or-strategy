# EPIC-CCN-18 Ticket 1: Extract Boolean Helpers

## Metadata
- **Epic**: EPIC-CCN-18
- **Ticket**: 1 of 4
- **Title**: Extract HasPendingEntryForAccount + HasActivePositionForAccount
- **CYC Reduction**: 37 → 23 (-14, 38%)
- **Effort**: 2-3 hours
- **BUILD_TAG**: `1111.043-epic-ccn-18-t1`
- **Mode**: `v12-engineer` (Bob CLI)
- **Status**: READY FOR EXECUTION

---

## Objective

Extract 2 pure boolean check helpers from [`HandleFlatPositionUpdate()`](src/V12_002.Orders.Callbacks.Execution.cs:69) to reduce cyclomatic complexity from 37 to 23. These helpers detect pending entry orders and active unfilled positions for a given account.

**Success Metric**: CYC 37 → 23 (-14 points, 38% reduction)

---

## Extraction Specifications

### Helper 1: `HasPendingEntryForAccount()`

**Signature**:
```csharp
private bool HasPendingEntryForAccount(string accountName)
```

**Purpose**: Detect if account has pending entry orders in flight

**Parameters**:
- `accountName` (string): Account name to search for (e.g., "Sim101")

**Returns**:
- `true`: Account has pending entry orders (Working/Accepted state)
- `false`: No pending entries found or account doesn't match

**Estimated CYC**: 6 (loop + 5-way AND condition)

**Logic** (Lines 78-92):
```csharp
private bool HasPendingEntryForAccount(string accountName)
{
    foreach (var kvp in entryOrders.ToArray())
    {
        var ord = kvp.Value;
        if (
            ord != null
            && !IsOrderTerminal(ord.OrderState)
            && activePositions.TryGetValue(kvp.Key, out var pos)
            && pos.ExecutingAccount != null
            && pos.ExecutingAccount.Name == accountName
        )
        {
            return true;
        }
    }
    return false;
}
```

**Type**: Pure function (read-only, no side effects)  
**Thread-Safety**: Uses `ToArray()` snapshot pattern  
**Dependencies**: `entryOrders`, `activePositions`, `IsOrderTerminal()`

---

### Helper 2: `HasActivePositionForAccount()`

**Signature**:
```csharp
private bool HasActivePositionForAccount(string accountName)
```

**Purpose**: Detect if account has active unfilled positions

**Parameters**:
- `accountName` (string): Account name to search for

**Returns**:
- `true`: Account has active unfilled positions
- `false`: No active positions found or all positions filled

**Estimated CYC**: 5 (loop + 3-way AND condition)

**Logic** (Lines 97-109):
```csharp
private bool HasActivePositionForAccount(string accountName)
{
    foreach (var kvp in activePositions.ToArray())
    {
        if (
            kvp.Value.ExecutingAccount != null
            && kvp.Value.ExecutingAccount.Name == accountName
            && !kvp.Value.EntryFilled
        )
        {
            return true;
        }
    }
    return false;
}
```

**Type**: Pure function (read-only, no side effects)  
**Thread-Safety**: Uses `ToArray()` snapshot pattern  
**Dependencies**: `activePositions`

---

## TDD Test Plan (BEFORE Extraction)

**Total Tests**: 11 (6 + 5)

### Helper 1 Tests: `HasPendingEntryForAccount` (6 tests)

1. **`HasPendingEntryForAccount_WithPendingOrder_ReturnsTrue`**
   - **Given**: Entry order exists with Working state, matching account
   - **When**: Call `HasPendingEntryForAccount("Sim101")`
   - **Then**: Returns `true`

2. **`HasPendingEntryForAccount_WithNoOrders_ReturnsFalse`**
   - **Given**: `entryOrders` dictionary is empty
   - **When**: Call `HasPendingEntryForAccount("Sim101")`
   - **Then**: Returns `false`

3. **`HasPendingEntryForAccount_WithCompletedOrder_ReturnsFalse`**
   - **Given**: Entry order exists with Filled state (terminal)
   - **When**: Call `HasPendingEntryForAccount("Sim101")`
   - **Then**: Returns `false`

4. **`HasPendingEntryForAccount_WithDifferentAccount_ReturnsFalse`**
   - **Given**: Entry order exists for "Sim102", searching for "Sim101"
   - **When**: Call `HasPendingEntryForAccount("Sim101")`
   - **Then**: Returns `false`

5. **`HasPendingEntryForAccount_WithMultipleAccounts_ReturnsCorrectly`**
   - **Given**: Multiple entry orders for different accounts
   - **When**: Call `HasPendingEntryForAccount("Sim101")`
   - **Then**: Returns `true` only if "Sim101" has pending entry

6. **`HasPendingEntryForAccount_WithNullAccount_ThrowsException`**
   - **Given**: Method called with `null` account name
   - **When**: Call `HasPendingEntryForAccount(null)`
   - **Then**: Throws `ArgumentNullException`

---

### Helper 2 Tests: `HasActivePositionForAccount` (5 tests)

7. **`HasActivePositionForAccount_WithActivePosition_ReturnsTrue`**
   - **Given**: Active position exists with `EntryFilled = false`, matching account
   - **When**: Call `HasActivePositionForAccount("Sim101")`
   - **Then**: Returns `true`

8. **`HasActivePositionForAccount_WithNoPositions_ReturnsFalse`**
   - **Given**: `activePositions` dictionary is empty
   - **When**: Call `HasActivePositionForAccount("Sim101")`
   - **Then**: Returns `false`

9. **`HasActivePositionForAccount_WithDifferentAccount_ReturnsFalse`**
   - **Given**: Active position exists for "Sim102", searching for "Sim101"
   - **When**: Call `HasActivePositionForAccount("Sim101")`
   - **Then**: Returns `false`

10. **`HasActivePositionForAccount_WithMultipleAccounts_ReturnsCorrectly`**
    - **Given**: Multiple active positions for different accounts
    - **When**: Call `HasActivePositionForAccount("Sim101")`
    - **Then**: Returns `true` only if "Sim101" has active unfilled position

11. **`HasActivePositionForAccount_WithNullAccount_ThrowsException`**
    - **Given**: Method called with `null` account name
    - **When**: Call `HasActivePositionForAccount(null)`
    - **Then**: Throws `ArgumentNullException`

---

## Implementation Steps

1. **Write TDD Tests** (11 tests)
   - Create test file: `tests/V12_Performance.Tests/Orders/HandleFlatPositionUpdateTests.cs`
   - Write all 11 tests BEFORE extraction
   - Verify tests compile with errors (methods don't exist yet)
   - Commit: `EPIC-CCN-18 T1: Add TDD tests for boolean helpers (11 tests)`

2. **Extract Helper 1: `HasPendingEntryForAccount()`**
   - Add method after line 176 in `src/V12_002.Orders.Callbacks.Execution.cs`
   - Copy lines 78-91 logic into helper
   - Verify method signature matches specification

3. **Replace Integration Point 1** (Lines 77-92)
   - **BEFORE**:
     ```csharp
     bool hasPendingEntry = false;
     foreach (var kvp in entryOrders.ToArray())
     {
         var ord = kvp.Value;
         if (
             ord != null
             && !IsOrderTerminal(ord.OrderState)
             && activePositions.TryGetValue(kvp.Key, out var pos)
             && pos.ExecutingAccount != null
             && pos.ExecutingAccount.Name == flatAcctName
         )
         {
             hasPendingEntry = true;
             break;
         }
     }
     ```
   - **AFTER**:
     ```csharp
     bool hasPendingEntry = HasPendingEntryForAccount(flatAcctName);
     ```

4. **Extract Helper 2: `HasActivePositionForAccount()`**
   - Add method after `HasPendingEntryForAccount()`
   - Copy lines 97-108 logic into helper
   - Verify method signature matches specification

5. **Replace Integration Point 2** (Lines 97-109)
   - **BEFORE**:
     ```csharp
     bool hasActivePositionForAcct = false;
     if (!hasPendingEntry)
     {
         foreach (var kvp in activePositions.ToArray())
         {
             if (
                 kvp.Value.ExecutingAccount != null
                 && kvp.Value.ExecutingAccount.Name == flatAcctName
                 && !kvp.Value.EntryFilled
             )
             {
                 hasActivePositionForAcct = true;
                 break;
             }
         }
     }
     ```
   - **AFTER**:
     ```csharp
     bool hasActivePositionForAcct = false;
     if (!hasPendingEntry)
     {
         hasActivePositionForAcct = HasActivePositionForAccount(flatAcctName);
     }
     ```

6. **Run Build**
   - Execute: `dotnet build`
   - Verify: Zero compilation errors

7. **Run Tests**
   - Execute: `dotnet test tests/V12_Performance.Tests/`
   - Verify: All 11 tests pass

8. **Run Complexity Audit**
   - Execute: `python scripts/complexity_audit.py`
   - Verify: `HandleFlatPositionUpdate` CYC = 23 (reduced from 37)
   - Verify: `HasPendingEntryForAccount` CYC ≤6
   - Verify: `HasActivePositionForAccount` CYC ≤5

9. **Run CSharpier**
   - Execute: `dotnet csharpier format src/`
   - Verify: Zero formatting issues

10. **Update BUILD_TAG**
    - Edit `src/V12_002.cs` line 1
    - Change: `// BUILD_TAG: 1111.042` → `// BUILD_TAG: 1111.043-epic-ccn-18-t1`

11. **Commit Extraction**
    - Message: `EPIC-CCN-18 Ticket 1: Extract pending/active position checks (CYC 37→23)`
    - Body:
      ```
      - Extract HasPendingEntryForAccount() helper (CYC 6)
      - Extract HasActivePositionForAccount() helper (CYC 5)
      - Add 11 TDD tests (6 + 5)
      - Reduce main method CYC from 37 to 23 (-14, 38% reduction)
      - Zero logic drift, pure structural refactoring
      ```

12. **Run Hard-Link Sync**
    - Execute: `powershell -File .\deploy-sync.ps1`
    - Verify: NinjaTrader hard links updated successfully

13. **STOP for F5 Verification**
    - **DO NOT PROCEED** to Ticket 2 until F5 verification passes
    - Load strategy in NinjaTrader
    - Verify: No runtime errors
    - Verify: Order management behavior unchanged

---

## Integration Points

### Integration Point 1: Pending Entry Detection (Lines 77-92)
- **Location**: Line 77
- **Variable**: `hasPendingEntry` (used in line 111)
- **Replacement**: Single helper call
- **Verification**: Variable name unchanged, boolean logic preserved

### Integration Point 2: Active Position Detection (Lines 97-109)
- **Location**: Line 97
- **Variable**: `hasActivePositionForAcct` (used in line 111)
- **Replacement**: Single helper call inside existing `if` block
- **Verification**: Conditional execution preserved (`if (!hasPendingEntry)`)

---

## Verification Checklist

- [ ] Build passes (zero errors)
- [ ] All 11 tests pass
- [ ] Complexity reduced (37 → 23)
- [ ] Helper 1 CYC ≤6
- [ ] Helper 2 CYC ≤5
- [ ] CSharpier formatted
- [ ] BUILD_TAG updated to `1111.043-epic-ccn-18-t1`
- [ ] deploy-sync.ps1 executed successfully
- [ ] F5 verification PASSED (NinjaTrader loads without errors)
- [ ] Order management behavior unchanged

---

## Success Criteria

**Quantitative**:
- ✅ Main method CYC = 23 (verified via `complexity_audit.py`)
- ✅ Helper 1 CYC ≤6
- ✅ Helper 2 CYC ≤5
- ✅ All 11 tests passing
- ✅ Zero compilation errors

**Qualitative**:
- ✅ Zero logic drift (pure structural refactoring)
- ✅ Thread-safety preserved (`ToArray()` snapshot pattern)
- ✅ Variable names unchanged (`hasPendingEntry`, `hasActivePositionForAcct`)
- ✅ Early exit optimization preserved (return on first match)

---

## Rollback Plan

**If F5 verification fails**:

1. **STOP immediately** (do not proceed to Ticket 2)
2. **Revert commit**: `git revert HEAD`
3. **Document failure**: Create `docs/brain/EPIC-CCN-18/ticket-01-failure.md`
4. **Analyze root cause**:
   - Logic drift detected?
   - Test gap identified?
   - Integration point error?
5. **Fix in separate session**:
   - Address root cause
   - Re-run TDD tests
   - Verify fix locally
6. **Re-execute Ticket 1** with corrected approach
7. **Report to Director** before proceeding to Ticket 2

---

## References

- **Master Tickets**: `docs/brain/EPIC-CCN-18/04-tickets.md`
- **Architecture Plan**: `docs/brain/EPIC-CCN-18/02-architecture-plan.md` (Section 1.1, 1.2, 3.1, 3.2)
- **Audit Report**: `docs/brain/EPIC-CCN-18/03-audit-report.md`
- **Source File**: `src/V12_002.Orders.Callbacks.Execution.cs` (Lines 69-176)
- **Test File**: `tests/V12_Performance.Tests/Orders/HandleFlatPositionUpdateTests.cs` (create)

---

**[TICKET-1-GATE]** Ready for execution via `epic-validate EPIC-CCN-18 --ticket 1`