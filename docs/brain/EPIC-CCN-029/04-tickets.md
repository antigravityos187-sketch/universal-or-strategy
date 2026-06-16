# Extraction Tickets: EPIC-CCN-029

## Overview
- **Total Tickets**: 4
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3 → TICKET-4)
- **Estimated Effort**: 6-8 hours
- **Target Method**: `ShouldSkipFleet_RunHealthCheck`
- **Current Complexity**: 31 (Cyclomatic Complexity)
- **Target Complexity**: ≤8 per method (Jane Street strict standard)

## Complexity Breakdown Analysis

### Current Method Structure
The `ShouldSkipFleet_RunHealthCheck` method performs H-13 stale state reconciliation with the following logical units:

1. **Broker Position Detection**: Snapshot positions, find matching instrument, determine flat state (~CYC 5)
2. **FSM State Detection**: Iterate _followerBrackets to find active FSM entries for account (~CYC 8)
3. **Active Position Detection**: Iterate activePositions to find entries for account (~CYC 5)
4. **Dispatch Pending Check**: Check _dispatchSyncPendingExpKeys (~CYC 3)
5. **State Reconciliation Logic**: Conditional logging based on state combinations (~CYC 8)

### Extraction Strategy
Extract 4 helper methods, each with CYC ≤8, maintaining the diagnostic-only (void return) contract.

---

## TICKET-1: Extract Broker Position Detection

### Scope
- **Current Method**: `ShouldSkipFleet_RunHealthCheck`
- **Current CYC**: 31
- **Target CYC**: ≤8 (main method after extraction)
- **Extraction**: Broker position snapshot and flat state detection logic

### Implementation
1. Create new private method: `GetBrokerPositionState(Account acct, out bool brokerFlat)`
   - Returns: `Position` (or null if flat)
   - Out parameter: `brokerFlat` boolean
   - Encapsulates: Position snapshot, instrument matching, flat state determination
2. Replace inline logic in `ShouldSkipFleet_RunHealthCheck` with method call
3. Verify null safety checks preserved (acct, acct.Positions)
4. Maintain [939-P0] snapshot pattern to prevent broker-thread mutation

### Acceptance Criteria
- [ ] New method `GetBrokerPositionState` created with CYC ≤8
- [ ] Method signature: `private Position GetBrokerPositionState(Account acct, out bool brokerFlat)`
- [ ] Null safety checks preserved (acct, acct.Positions)
- [ ] Position snapshot pattern maintained (ToArray())
- [ ] For-loop iteration preserved (no LINQ allocation)
- [ ] All existing tests pass (zero regression)
- [ ] Build succeeds (`powershell -File .\scripts\build_readiness.ps1`)
- [ ] CSharpier formatting passes
- [ ] Unit test added for `GetBrokerPositionState`

### Dependencies
- None (first ticket)

---

## TICKET-2: Extract FSM State Detection

### Scope
- **Current Method**: `ShouldSkipFleet_RunHealthCheck`
- **Current CYC**: ~23 (after TICKET-1)
- **Target CYC**: ≤8 (main method after extraction)
- **Extraction**: FSM active state detection for account

### Implementation
1. Create new private method: `HasActiveFsmForAccount(string accountName)`
   - Returns: `bool`
   - Encapsulates: _followerBrackets iteration, state checks
2. Replace inline foreach loop with method call
3. Preserve state checks: Active, Accepted, Submitted, Replacing
4. Maintain thread-safe ConcurrentDictionary enumeration pattern

### Acceptance Criteria
- [ ] New method `HasActiveFsmForAccount` created with CYC ≤8
- [ ] Method signature: `private bool HasActiveFsmForAccount(string accountName)`
- [ ] All FSM states checked: Active, Accepted, Submitted, Replacing
- [ ] Thread-safe enumeration preserved (no snapshot needed for ConcurrentDictionary)
- [ ] Null safety checks for FSM entries (f != null, f.AccountName != null)
- [ ] All existing tests pass (zero regression)
- [ ] Build succeeds
- [ ] CSharpier formatting passes
- [ ] Unit test added for `HasActiveFsmForAccount`

### Dependencies
- TICKET-1 must be completed first

---

## TICKET-3: Extract Active Position Detection

### Scope
- **Current Method**: `ShouldSkipFleet_RunHealthCheck`
- **Current CYC**: ~18 (after TICKET-2)
- **Target CYC**: ≤8 (main method after extraction)
- **Extraction**: Active position detection for account

### Implementation
1. Create new private method: `HasActivePositionForAccount(string accountName)`
   - Returns: `bool`
   - Encapsulates: activePositions iteration, follower checks
2. Replace inline foreach loop with method call
3. Preserve checks: IsFollower, ExecutingAccount match
4. Maintain thread-safe ConcurrentDictionary enumeration pattern

### Acceptance Criteria
- [ ] New method `HasActivePositionForAccount` created with CYC ≤8
- [ ] Method signature: `private bool HasActivePositionForAccount(string accountName)`
- [ ] IsFollower check preserved
- [ ] ExecutingAccount null safety preserved (p.ExecutingAccount != null)
- [ ] Thread-safe enumeration preserved
- [ ] All existing tests pass (zero regression)
- [ ] Build succeeds
- [ ] CSharpier formatting passes
- [ ] Unit test added for `HasActivePositionForAccount`

### Dependencies
- TICKET-2 must be completed first

---

## TICKET-4: Simplify State Reconciliation Logic

### Scope
- **Current Method**: `ShouldSkipFleet_RunHealthCheck`
- **Current CYC**: ~13 (after TICKET-3)
- **Target CYC**: ≤8 (final target)
- **Extraction**: State reconciliation logging logic

### Implementation
1. Create new private method: `LogStateReconciliation(string accountName, bool brokerFlat, bool hasActiveFsm, bool hasActivePosition, bool hasDispatchPending, StringBuilder dispatchLog)`
   - Returns: `void`
   - Encapsulates: Conditional logging based on state combinations
2. Replace inline conditional logging with method call
3. Preserve all diagnostic messages
4. Maintain StringBuilder append pattern for batch logging

### Implementation Details
```csharp
private void LogStateReconciliation(
    string accountName,
    bool brokerFlat,
    bool hasActiveFsm,
    bool hasActivePosition,
    bool hasDispatchPending,
    StringBuilder dispatchLog
)
{
    if (brokerFlat && !hasActiveFsm && !hasActivePosition && !hasDispatchPending)
    {
        dispatchLog.AppendLine(
            string.Format(
                "[DISPATCH] H-13: {0} broker flat, no FSM/position/dispatch -- no action",
                accountName
            )
        );
    }
    else if (brokerFlat && (hasActiveFsm || hasActivePosition || hasDispatchPending))
    {
        dispatchLog.AppendLine(
            string.Format(
                "[DISPATCH] H-13 SKIP: {0} Flat but {1} -- not resetting",
                accountName,
                hasActiveFsm
                    ? "FSM active"
                    : (hasDispatchPending ? "dispatch pending" : "activePos present")
            )
        );
    }
}
```

### Acceptance Criteria
- [ ] New method `LogStateReconciliation` created with CYC ≤8
- [ ] Method signature matches specification above
- [ ] All diagnostic messages preserved exactly
- [ ] StringBuilder append pattern maintained
- [ ] Main method `ShouldSkipFleet_RunHealthCheck` achieves CYC ≤8
- [ ] All existing tests pass (zero regression)
- [ ] Build succeeds
- [ ] CSharpier formatting passes
- [ ] Unit test added for `LogStateReconciliation`
- [ ] Final complexity audit confirms all methods ≤8

### Dependencies
- TICKET-3 must be completed first

---

## Final Method Structure (After All Tickets)

### Main Method (CYC ≤8)
```csharp
private void ShouldSkipFleet_RunHealthCheck(Account acct, StringBuilder dispatchLog)
{
    try
    {
        if (acct == null || acct.Positions == null)
        {
            return;
        }

        bool brokerFlat;
        Position brokerPos = GetBrokerPositionState(acct, out brokerFlat);

        bool hasActiveFsmForAcct = HasActiveFsmForAccount(acct.Name);
        bool hasActivePositionForAcct = HasActivePositionForAccount(acct.Name);
        bool hasDispatchPending = _dispatchSyncPendingExpKeys.ContainsKey(ExpKey(acct.Name));

        LogStateReconciliation(
            acct.Name,
            brokerFlat,
            hasActiveFsmForAcct,
            hasActivePositionForAcct,
            hasDispatchPending,
            dispatchLog
        );
    }
    catch (Exception ex)
    {
        if (_diagFleet)
            Print("[FLEET_CATCH] ProcessFleetSlot account iteration failed: " + ex.Message);
    }
}
```

### Helper Methods (Each CYC ≤8)
1. `GetBrokerPositionState(Account acct, out bool brokerFlat)` - CYC ~5
2. `HasActiveFsmForAccount(string accountName)` - CYC ~6
3. `HasActivePositionForAccount(string accountName)` - CYC ~5
4. `LogStateReconciliation(...)` - CYC ~4

---

## V12 DNA Compliance Checklist

### Lock-Free Pattern
- [ ] Zero lock() blocks in all extracted methods
- [ ] Thread-safe ConcurrentDictionary enumeration preserved
- [ ] No new synchronization primitives introduced

### ASCII-Only Compliance
- [ ] All string literals use ASCII characters only
- [ ] No Unicode, emoji, or curly quotes

### Correctness by Construction
- [ ] Null safety checks preserved at all boundaries
- [ ] Out parameters used correctly (GetBrokerPositionState)
- [ ] Boolean return values unambiguous

### Testing Requirements
- [ ] Unit test for each extracted method (4 tests minimum)
- [ ] Integration test for main method after all extractions
- [ ] Regression test suite passes (zero failures)

---

## Execution Notes

### Checkpoint Strategy
- Create checkpoint after each ticket completion
- Use Bob CLI `/restore` if regression detected
- Verify build + tests after each extraction

### Verification Commands
```powershell
# After each ticket
powershell -File .\scripts\build_readiness.ps1
dotnet csharpier check src/
python scripts/complexity_audit.py

# Final verification
powershell -File .\scripts\pre_push_validation.ps1
```

### Risk Mitigation
- **Incremental Extraction**: One helper at a time, verify after each
- **Behavior Preservation**: Compare diagnostic logs before/after
- **Rollback Safety**: Bob CLI checkpointing enabled throughout

---

## Success Metrics

### Complexity Reduction
- **Before**: ShouldSkipFleet_RunHealthCheck CYC = 31
- **After**: Main method CYC ≤8, all helpers CYC ≤8
- **Total Methods**: 1 → 5 (main + 4 helpers)

### Code Quality
- **Cognitive Load**: Reduced via single-responsibility helpers
- **Testability**: Each helper independently testable
- **Maintainability**: Clear separation of concerns

### V12 Alignment
- **Jane Street Standard**: CYC ≤8 achieved (stricter than V12 threshold of 15)
- **Lock-Free Pattern**: Preserved throughout
- **Zero Regression**: All tests pass, build succeeds
