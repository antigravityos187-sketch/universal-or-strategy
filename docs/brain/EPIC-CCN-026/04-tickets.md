# Extraction Tickets: EPIC-CCN-026

## Overview
- **Total Tickets**: 4
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3 → TICKET-4)
- **Estimated Effort**: 3-4 hours
- **Target Method**: ProcessQueuedAccountOrder
- **File**: src/V12_002.Orders.Callbacks.AccountOrders.cs
- **Current Complexity**: 15
- **Target Complexity**: ≤8
- **Estimated Final Complexity**: 5-6

---

## TICKET-1: Extract ValidateOrderContext

### Scope
- **Current Method**: `ProcessQueuedAccountOrder`
- **Current CYC**: 15
- **Target CYC**: 13 (after this extraction)
- **Extraction**: Early validation logic (lines 1056-1060)

### Implementation
1. Create new private method:
   ```csharp
   private bool ValidateOrderContext(QueuedAccountOrderUpdate item, out Order order, out string instrumentName)
   ```
2. Move validation logic from lines 1056-1060 into method body
3. Return `false` if validation fails (caller should return early)
4. Return `true` with out parameters if validation succeeds
5. Replace original lines with method call:
   ```csharp
   if (!ValidateOrderContext(item, out Order order, out string instrumentName))
   {
       return;
   }
   ```
6. Run complexity_audit.py to verify reduction
7. Run full test suite

### Acceptance Criteria
- [ ] ValidateOrderContext method created with CYC ≤ 2
- [ ] ProcessQueuedAccountOrder complexity reduced to ~13
- [ ] All existing tests pass
- [ ] No behavioral changes
- [ ] Build succeeds
- [ ] No lock() statements introduced
- [ ] ASCII-only compliance maintained

### Dependencies
- None (first ticket)

### Verification Commands
```bash
python scripts/complexity_audit.py
grep -r "lock(" src/V12_002.Orders.Callbacks.AccountOrders.cs
dotnet build
dotnet test
```

---

## TICKET-2: Extract LogOrderUpdate

### Scope
- **Current Method**: `ProcessQueuedAccountOrder`
- **Current CYC**: 13 (after TICKET-1)
- **Target CYC**: 12 (after this extraction)
- **Extraction**: Audit trail logging (lines 1062-1071)

### Implementation
1. Create new private method:
   ```csharp
   private void LogOrderUpdate(Order order, Account account, string orderState)
   ```
2. Move logging logic from lines 1062-1071 into method body
3. Replace original lines with method call:
   ```csharp
   LogOrderUpdate(order, item.Account, order.OrderState.ToString());
   ```
4. Run complexity_audit.py to verify reduction
5. Run full test suite

### Acceptance Criteria
- [ ] LogOrderUpdate method created with CYC ≤ 1
- [ ] ProcessQueuedAccountOrder complexity reduced to ~12
- [ ] All existing tests pass
- [ ] No behavioral changes
- [ ] Build succeeds
- [ ] Audit trail format unchanged
- [ ] ASCII-only compliance maintained

### Dependencies
- TICKET-1 must be completed first

### Verification Commands
```bash
python scripts/complexity_audit.py
dotnet build
dotnet test
```

---

## TICKET-3: Extract FindMatchedPosition

### Scope
- **Current Method**: `ProcessQueuedAccountOrder`
- **Current CYC**: 12 (after TICKET-2)
- **Target CYC**: 5-6 (after this extraction)
- **Extraction**: Position search loop (lines 1078-1091)

### Implementation
1. Create new private method:
   ```csharp
   private (string matchedEntry, PositionInfo matchedPos) FindMatchedPosition(Order order, Account account, KeyValuePair<string, PositionInfo>[] snapshot)
   ```
2. Move foreach loop from lines 1078-1091 into method body
3. Return tuple `(matchedEntry, matchedPos)` when match found
4. Return `(null, null)` if no match found
5. Replace original lines with method call:
   ```csharp
   var (matchedEntry, matchedPos) = FindMatchedPosition(order, item.Account, snapshot);
   ```
6. Update conditional logic to use tuple result
7. Run complexity_audit.py to verify target achieved
8. Run full test suite

### Acceptance Criteria
- [ ] FindMatchedPosition method created with CYC ≤ 3
- [ ] ProcessQueuedAccountOrder complexity reduced to ≤8 (target: 5-6)
- [ ] All existing tests pass
- [ ] No behavioral changes
- [ ] Build succeeds
- [ ] Snapshot pattern preserved (thread-safe)
- [ ] ASCII-only compliance maintained

### Dependencies
- TICKET-1 must be completed first
- TICKET-2 must be completed first

### Verification Commands
```bash
python scripts/complexity_audit.py
grep -r "lock(" src/V12_002.Orders.Callbacks.AccountOrders.cs
dotnet build
dotnet test
```

---

## TICKET-4: Final Verification & Documentation

### Scope
- **Verification**: Confirm all complexity targets met
- **Documentation**: Update method XML comments
- **Integration**: Hard-link sync and deployment readiness

### Implementation
1. Run full complexity audit on modified file
2. Verify ProcessQueuedAccountOrder complexity ≤8
3. Verify all helper methods complexity ≤8
4. Update XML documentation comments:
   - Add complexity metrics to method comments
   - Document helper method contracts
   - Reference EPIC-CCN-026 in commit message
5. Run deploy-sync.ps1 for hard-link integrity
6. Execute full test suite
7. Generate final metrics report

### Acceptance Criteria
- [ ] ProcessQueuedAccountOrder complexity ≤8 (verified)
- [ ] ValidateOrderContext complexity ≤2 (verified)
- [ ] LogOrderUpdate complexity ≤1 (verified)
- [ ] FindMatchedPosition complexity ≤3 (verified)
- [ ] All tests pass (100% pass rate)
- [ ] Hard-link sync successful
- [ ] XML documentation updated
- [ ] No lock() statements in file
- [ ] ASCII-only compliance verified
- [ ] Commit message references EPIC-CCN-026

### Dependencies
- TICKET-1 must be completed first
- TICKET-2 must be completed first
- TICKET-3 must be completed first

### Verification Commands
```bash
python scripts/complexity_audit.py
grep -r "lock(" src/V12_002.Orders.Callbacks.AccountOrders.cs
dotnet build
dotnet test
powershell -File .\deploy-sync.ps1
```

### Final Metrics Report Template
```markdown
## EPIC-CCN-026 Completion Report

**Method**: ProcessQueuedAccountOrder
**File**: src/V12_002.Orders.Callbacks.AccountOrders.cs

### Before
- Complexity: 15
- LOC: 47

### After
- Complexity: [Actual]
- LOC: [Actual]
- Helper Methods: 3
  - ValidateOrderContext: CYC [Actual]
  - LogOrderUpdate: CYC [Actual]
  - FindMatchedPosition: CYC [Actual]

### Quality Gates
- ✅ Complexity ≤8: [PASS/FAIL]
- ✅ All tests pass: [PASS/FAIL]
- ✅ No lock() statements: [PASS/FAIL]
- ✅ ASCII-only: [PASS/FAIL]
- ✅ Hard-link sync: [PASS/FAIL]
```

---

## Execution Notes

### Safety Protocols
- **Checkpointing**: Bob CLI checkpointing enabled
- **Rollback**: Use `/restore` if complexity target missed
- **Testing**: Run tests after each ticket completion

### DNA Compliance Checklist
- [ ] No lock() statements introduced
- [ ] FSM/Actor pattern preserved
- [ ] ASCII-only compliance maintained
- [ ] Type safety preserved (no nullable ambiguity)
- [ ] Jane Street cognitive simplicity achieved (CYC ≤8)

### PR Hygiene Checklist
- [ ] Diff size <10,000 characters
- [ ] Single method focus (no scope creep)
- [ ] No whitespace mutations
- [ ] Build succeeds
- [ ] All tests pass

---

*Generated by V12 Photon Kernel Phase 4 Protocol (V12.23)*
*Total Tickets: 4 | Estimated Effort: 3-4 hours | Target CYC: ≤8*
