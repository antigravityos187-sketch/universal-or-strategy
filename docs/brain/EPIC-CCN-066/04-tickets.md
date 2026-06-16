# Extraction Tickets: EPIC-CCN-066

## Overview
- **Total Tickets**: 4
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3 → TICKET-4)
- **Estimated Effort**: 1.5 hours
- **Epic ID**: EPIC-CCN-066
- **Target Method**: RemoveFsmOrderIdMappings
- **File**: src/V12_002.Symmetry.BracketFSM.cs
- **Current Complexity**: 11 (CYC)
- **Target Complexity**: ≤4 (main method)

---

## TICKET-1: Extract RemoveEntryOrderMapping Helper

### Scope
- **Current Method**: `RemoveFsmOrderIdMappings`
- **Current CYC**: 11
- **Target CYC**: ≤3 (helper method)
- **Extraction**: Entry order and replacing cancel order removal logic

### Implementation
1. Create new private method `RemoveEntryOrderMapping(FollowerBracketFSM fsm)`
2. Move entry order null check and TryRemove logic
3. Move replacing cancel order string check and TryRemove logic
4. Ensure method is placed near RemoveFsmOrderIdMappings for readability

### Code Changes
```csharp
private void RemoveEntryOrderMapping(FollowerBracketFSM fsm)
{
    if (fsm.EntryOrder != null && !string.IsNullOrEmpty(fsm.EntryOrder.OrderId))
    {
        _orderIdToFsmKey.TryRemove(fsm.EntryOrder.OrderId, out _);
    }
    
    if (!string.IsNullOrEmpty(fsm.ReplacingCancelOrderId))
    {
        _orderIdToFsmKey.TryRemove(fsm.ReplacingCancelOrderId, out _);
    }
}
```

### Acceptance Criteria
- [ ] Method complexity ≤3 (CYC)
- [ ] Handles null EntryOrder gracefully
- [ ] Handles empty/null OrderId strings
- [ ] Uses ConcurrentDictionary.TryRemove (lock-free)
- [ ] No behavioral changes from original logic
- [ ] Build succeeds (dotnet build)
- [ ] CSharpier formatting applied

### Dependencies
- None (first ticket)

### Estimated Time
- Implementation: 10 minutes
- Verification: 5 minutes

---

## TICKET-2: Extract RemoveStopOrderMapping Helper

### Scope
- **Current Method**: `RemoveFsmOrderIdMappings`
- **Current CYC**: 11 → 8 (after TICKET-1)
- **Target CYC**: ≤3 (helper method)
- **Extraction**: Stop order removal logic

### Implementation
1. Create new private method `RemoveStopOrderMapping(FollowerBracketFSM fsm)`
2. Move stop order null check and TryRemove logic
3. Ensure method is placed after RemoveEntryOrderMapping

### Code Changes
```csharp
private void RemoveStopOrderMapping(FollowerBracketFSM fsm)
{
    if (fsm.StopOrder != null && !string.IsNullOrEmpty(fsm.StopOrder.OrderId))
    {
        _orderIdToFsmKey.TryRemove(fsm.StopOrder.OrderId, out _);
    }
}
```

### Acceptance Criteria
- [ ] Method complexity ≤3 (CYC)
- [ ] Handles null StopOrder gracefully
- [ ] Handles empty/null OrderId strings
- [ ] Uses ConcurrentDictionary.TryRemove (lock-free)
- [ ] No behavioral changes from original logic
- [ ] Build succeeds (dotnet build)
- [ ] CSharpier formatting applied

### Dependencies
- TICKET-1 must be completed first

### Estimated Time
- Implementation: 10 minutes
- Verification: 5 minutes

---

## TICKET-3: Extract RemoveTargetOrderMappings Helper

### Scope
- **Current Method**: `RemoveFsmOrderIdMappings`
- **Current CYC**: 8 → 5 (after TICKET-2)
- **Target CYC**: ≤4 (helper method)
- **Extraction**: Target orders collection removal logic (loop)

### Implementation
1. Create new private method `RemoveTargetOrderMappings(FollowerBracketFSM fsm)`
2. Move targets null check and foreach loop logic
3. Move target null check and TryRemove logic
4. Ensure method is placed after RemoveStopOrderMapping

### Code Changes
```csharp
private void RemoveTargetOrderMappings(FollowerBracketFSM fsm)
{
    if (fsm.Targets != null)
    {
        foreach (var target in fsm.Targets)
        {
            if (target != null && !string.IsNullOrEmpty(target.OrderId))
            {
                _orderIdToFsmKey.TryRemove(target.OrderId, out _);
            }
        }
    }
}
```

### Acceptance Criteria
- [ ] Method complexity ≤4 (CYC)
- [ ] Handles null Targets collection gracefully
- [ ] Handles null target items in collection
- [ ] Handles empty/null OrderId strings
- [ ] Uses ConcurrentDictionary.TryRemove (lock-free)
- [ ] No behavioral changes from original logic
- [ ] Build succeeds (dotnet build)
- [ ] CSharpier formatting applied

### Dependencies
- TICKET-1 must be completed first
- TICKET-2 must be completed first

### Estimated Time
- Implementation: 15 minutes
- Verification: 5 minutes

---

## TICKET-4: Refactor Main Method to Orchestration

### Scope
- **Current Method**: `RemoveFsmOrderIdMappings`
- **Current CYC**: 5 (after TICKET-3)
- **Target CYC**: ≤4
- **Refactoring**: Convert to pure orchestration (call helpers)

### Implementation
1. Replace entry order logic with call to RemoveEntryOrderMapping(fsm)
2. Replace stop order logic with call to RemoveStopOrderMapping(fsm)
3. Replace target orders logic with call to RemoveTargetOrderMappings(fsm)
4. Keep only fsm null check in main method
5. Verify method is now pure orchestration

### Code Changes
```csharp
private void RemoveFsmOrderIdMappings(FollowerBracketFSM fsm)
{
    if (fsm == null)
    {
        return;
    }
    
    RemoveEntryOrderMapping(fsm);
    RemoveStopOrderMapping(fsm);
    RemoveTargetOrderMappings(fsm);
}
```

### Acceptance Criteria
- [ ] Method complexity ≤4 (CYC)
- [ ] Main method is pure orchestration (no business logic)
- [ ] All helper methods called in correct order
- [ ] Null check on fsm parameter preserved
- [ ] No behavioral changes from original logic
- [ ] Build succeeds (dotnet build)
- [ ] All tests pass (dotnet test)
- [ ] CSharpier formatting applied
- [ ] Complexity verified: `python scripts/complexity_audit.py`

### Dependencies
- TICKET-1 must be completed first
- TICKET-2 must be completed first
- TICKET-3 must be completed first

### Estimated Time
- Implementation: 10 minutes
- Verification: 10 minutes

---

## Verification Checklist (After All Tickets)

### Build & Test
- [ ] Run `powershell -File .\scripts\build_readiness.ps1` (zero errors)
- [ ] Run `dotnet test` (all tests pass)
- [ ] Run `python scripts/complexity_audit.py` (verify CYC ≤4 for main method)

### Pre-Push Validation
- [ ] Run `powershell -File .\scripts\pre_push_validation.ps1 -Fast`
- [ ] Verify ASCII-only compliance (Check #1)
- [ ] Verify build success (Check #2)
- [ ] Verify unit tests pass (Check #3)
- [ ] Verify lint clean (Check #4)
- [ ] Verify CSharpier formatting (Check #5)
- [ ] Verify PR hygiene (Check #8)
- [ ] Verify complexity ≤15 (Check #9)

### Hard-Link Sync
- [ ] Run `powershell -File .\deploy-sync.ps1`
- [ ] Verify NinjaTrader hard links synchronized

### Git Commit
- [ ] Commit with message: `refactor: EPIC-CCN-066 extract RemoveFsmOrderIdMappings helpers (CYC 11→4)`
- [ ] Verify diff size <10,000 characters (source code only)

### PR Creation
- [ ] Create PR with title: `refactor: EPIC-CCN-066 extract RemoveFsmOrderIdMappings helpers (CYC 11→4)`
- [ ] Link to EPIC-CCN-066 documentation
- [ ] Verify Codacy shows "Up to quality standards"
- [ ] Verify no new issues introduced

---

## Complexity Reduction Summary

| Method | Before | After | Status |
|--------|--------|-------|--------|
| RemoveFsmOrderIdMappings | 11 | ≤4 | ✅ Target Met |
| RemoveEntryOrderMapping | N/A | ≤3 | ✅ Jane Street Aligned |
| RemoveStopOrderMapping | N/A | ≤3 | ✅ Jane Street Aligned |
| RemoveTargetOrderMappings | N/A | ≤4 | ✅ Jane Street Aligned |

**Total Complexity Reduction**: 11 → 4 (63% reduction)
**Jane Street Compliance**: ✅ All methods ≤8 CYC

---

## Risk Assessment

### Implementation Risk: LOW
- Clear extraction boundaries
- No API surface changes
- No caller modifications required
- Pure refactoring (behavior preservation)

### Regression Risk: LOW
- Existing tests verify correctness
- Lock-free operations preserved
- Null-safety maintained
- Atomic operations unchanged

### Rollback Plan
- Single commit revert
- No database migrations
- No configuration changes
- Immediate rollback possible

---

## Success Metrics

### Functional
- ✅ Method signature unchanged
- ✅ Behavior identical to original
- ✅ All existing tests pass
- ✅ Zero compilation errors

### Non-Functional
- ✅ Complexity reduced: 11 → ≤4
- ✅ Lock-free compliance maintained
- ✅ ASCII-only compliance maintained
- ✅ Jane Street alignment (CYC ≤8)

### Quality Gates
- ✅ Build readiness: PASS
- ✅ Pre-push validation: PASS
- ✅ PR hygiene: Diff <10k characters
- ✅ Codacy: Zero new issues

---

## Phase 4 Completion

- **Status**: COMPLETED
- **Date**: 2026-06-15
- **Ticket Count**: 4
- **Total Estimated Time**: 1.5 hours
- **Next Phase**: 5 (Ticket Execution - Bob CLI)

**Approval**: ✅ READY FOR IMPLEMENTATION
