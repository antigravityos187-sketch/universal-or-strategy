# Extraction Tickets: EPIC-CCN-064

## Overview
- **Epic**: EPIC-CCN-064
- **Target Method**: ResolveFsm_ByScan
- **File**: src/V12_002.Symmetry.BracketFSM.cs
- **Current CYC**: 12
- **Target CYC**: 5 (main method) + 2+3+2 (helpers) = 12 total
- **Total Tickets**: 4
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3 → TICKET-4)
- **Estimated Effort**: 3-4 hours

## Strategy
Extract three specialized matching methods, each handling one order type:
1. TryMatchStopOrder - Check StopOrder match
2. TryMatchTargetOrder - Check Targets array match
3. TryMatchEntryOrder - Check EntryOrder match
4. Refactor main method to orchestrate helpers

---

## TICKET-1: Extract TryMatchStopOrder Helper

### Scope
- **Current Method**: `ResolveFsm_ByScan`
- **Lines to Extract**: 219-223
- **New Method**: `TryMatchStopOrder`
- **Target CYC**: ≤ 2

### Implementation

1. **Create new private method** above ResolveFsm_ByScan:
```csharp
/// <summary>
/// Checks if the given orderId matches the FSM's StopOrder.
/// If matched, caches the mapping and returns true.
/// </summary>
/// <param name="fsm">The FSM instance to check</param>
/// <param name="orderId">The order ID to match</param>
/// <returns>True if StopOrder matches; otherwise false</returns>
private bool TryMatchStopOrder(FollowerBracketFSM fsm, string orderId)
{
    if (fsm.StopOrder != null && fsm.StopOrder.OrderId == orderId)
    {
        _orderIdToFsmKey[orderId] = fsm.EntryName;
        return true;
    }
    return false;
}
```

2. **Verify extraction**:
   - Method signature matches architecture plan
   - XML documentation added
   - Cache write preserved (_orderIdToFsmKey)
   - Return type is bool

3. **Run complexity check**:
```bash
python3 scripts/complexity_audit.py
```
   - Verify TryMatchStopOrder shows CYC ≤ 2

### Acceptance Criteria
- [ ] New method TryMatchStopOrder created
- [ ] XML documentation added
- [ ] Method complexity CYC ≤ 2
- [ ] Cache write behavior preserved
- [ ] Build succeeds: `dotnet build`
- [ ] Formatting passes: `dotnet csharpier check src/`

### Dependencies
- None (first ticket)

---

## TICKET-2: Extract TryMatchTargetOrder Helper

### Scope
- **Current Method**: `ResolveFsm_ByScan`
- **Lines to Extract**: 225-233
- **New Method**: `TryMatchTargetOrder`
- **Target CYC**: ≤ 3
- **Dead Code Removal**: Remove foundT flag and unreachable check at line 234

### Implementation

1. **Create new private method** above ResolveFsm_ByScan:
```csharp
/// <summary>
/// Checks if the given orderId matches any Target order in the FSM's Targets array.
/// If matched, caches the mapping and returns true.
/// </summary>
/// <param name="fsm">The FSM instance to check</param>
/// <param name="orderId">The order ID to match</param>
/// <returns>True if any Target matches; otherwise false</returns>
private bool TryMatchTargetOrder(FollowerBracketFSM fsm, string orderId)
{
    for (int i = 0; i < 5; i++)
    {
        if (fsm.Targets[i] != null && fsm.Targets[i].OrderId == orderId)
        {
            _orderIdToFsmKey[orderId] = fsm.EntryName;
            return true;
        }
    }
    return false;
}
```

2. **Remove dead code**:
   - Delete `bool foundT = false;` declaration
   - Delete unreachable `if (foundT) return f;` check at line 234

3. **Verify extraction**:
   - Method signature matches architecture plan
   - XML documentation added
   - Loop logic preserved (0 to 4 inclusive)
   - Early return on match preserved
   - Cache write preserved

4. **Run complexity check**:
```bash
python3 scripts/complexity_audit.py
```
   - Verify TryMatchTargetOrder shows CYC ≤ 3

### Acceptance Criteria
- [ ] New method TryMatchTargetOrder created
- [ ] XML documentation added
- [ ] Method complexity CYC ≤ 3
- [ ] Dead code removed (foundT flag and check)
- [ ] Loop logic preserved (5 iterations)
- [ ] Cache write behavior preserved
- [ ] Build succeeds: `dotnet build`
- [ ] Formatting passes: `dotnet csharpier check src/`

### Dependencies
- TICKET-1 must be completed first

---

## TICKET-3: Extract TryMatchEntryOrder Helper

### Scope
- **Current Method**: `ResolveFsm_ByScan`
- **Lines to Extract**: 237-240
- **New Method**: `TryMatchEntryOrder`
- **Target CYC**: ≤ 2

### Implementation

1. **Create new private method** above ResolveFsm_ByScan:
```csharp
/// <summary>
/// Checks if the given orderId matches the FSM's EntryOrder.
/// If matched, caches the mapping and returns true.
/// </summary>
/// <param name="fsm">The FSM instance to check</param>
/// <param name="orderId">The order ID to match</param>
/// <returns>True if EntryOrder matches; otherwise false</returns>
private bool TryMatchEntryOrder(FollowerBracketFSM fsm, string orderId)
{
    if (fsm.EntryOrder != null && fsm.EntryOrder.OrderId == orderId)
    {
        _orderIdToFsmKey[orderId] = fsm.EntryName;
        return true;
    }
    return false;
}
```

2. **Verify extraction**:
   - Method signature matches architecture plan
   - XML documentation added
   - Cache write preserved
   - Return type is bool

3. **Run complexity check**:
```bash
python3 scripts/complexity_audit.py
```
   - Verify TryMatchEntryOrder shows CYC ≤ 2

### Acceptance Criteria
- [ ] New method TryMatchEntryOrder created
- [ ] XML documentation added
- [ ] Method complexity CYC ≤ 2
- [ ] Cache write behavior preserved
- [ ] Build succeeds: `dotnet build`
- [ ] Formatting passes: `dotnet csharpier check src/`

### Dependencies
- TICKET-2 must be completed first

---

## TICKET-4: Refactor Main Method

### Scope
- **Method**: `ResolveFsm_ByScan`
- **Current CYC**: 12
- **Target CYC**: ≤ 5
- **Action**: Replace extracted logic with helper calls

### Implementation

1. **Refactor ResolveFsm_ByScan** to use helpers:
```csharp
private FollowerBracketFSM ResolveFsm_ByScan(string accountAlias, string orderId)
{
    if (string.IsNullOrEmpty(orderId))
    {
        return null;
    }

    foreach (var f in _followerBrackets.Values)
    {
        if (f.AccountName != accountAlias)
        {
            continue;
        }

        if (TryMatchStopOrder(f, orderId))
        {
            return f;
        }

        if (TryMatchTargetOrder(f, orderId))
        {
            return f;
        }

        if (TryMatchEntryOrder(f, orderId))
        {
            return f;
        }
    }

    return null;
}
```

2. **Verify refactoring**:
   - Early return for null/empty orderId preserved
   - Account name filtering preserved
   - Helper calls replace inline logic
   - Return FSM on first match (short-circuit behavior)
   - Return null if no match found

3. **Run complexity check**:
```bash
python3 scripts/complexity_audit.py
```
   - Verify ResolveFsm_ByScan shows CYC ≤ 5

4. **Run full validation**:
```bash
powershell -File .\scripts\pre_push_validation.ps1 -Fast
```

### Acceptance Criteria
- [ ] Main method refactored to use helpers
- [ ] Method complexity CYC ≤ 5
- [ ] Behavior equivalence verified (same logic flow)
- [ ] Early returns preserved
- [ ] Account filtering preserved
- [ ] Cache writes occur at same points
- [ ] Build succeeds: `dotnet build`
- [ ] All tests pass: `dotnet test`
- [ ] Formatting passes: `dotnet csharpier check src/`
- [ ] Complexity audit passes: `python3 scripts/complexity_audit.py`
- [ ] Pre-push validation passes (fast mode)

### Dependencies
- TICKET-3 must be completed first

---

## Verification Strategy

### Per-Ticket Verification
After each ticket:
1. Run `dotnet build` (zero errors)
2. Run `dotnet csharpier check src/` (zero issues)
3. Run `python3 scripts/complexity_audit.py` (verify CYC targets)

### Final Verification (After TICKET-4)
1. Run `powershell -File .\scripts\pre_push_validation.ps1 -Fast`
2. Verify all complexity targets met:
   - ResolveFsm_ByScan: CYC ≤ 5 ✅
   - TryMatchStopOrder: CYC ≤ 2 ✅
   - TryMatchTargetOrder: CYC ≤ 3 ✅
   - TryMatchEntryOrder: CYC ≤ 2 ✅
3. Manual test: F5 in NinjaTrader (verify no runtime errors)
4. Run `powershell -File .\deploy-sync.ps1` (sync hard links)

## Rollback Plan

If any ticket fails verification:
1. `git status` - check modified files
2. `git diff src/V12_002.Symmetry.BracketFSM.cs` - review changes
3. `git checkout src/V12_002.Symmetry.BracketFSM.cs` - revert if needed
4. Re-attempt ticket with corrected approach

## Success Criteria

### Phase 4 Completion
- ✅ All 4 tickets documented
- ✅ Each ticket has clear scope and implementation steps
- ✅ Acceptance criteria defined for each ticket
- ✅ Dependencies documented (sequential execution)
- ✅ Verification strategy defined
- ✅ Rollback plan documented

### Ready for Phase 5 (Execution)
- Tickets ready for Bob CLI (`v12-engineer`) execution
- Clear acceptance criteria for each step
- Complexity targets validated
- Risk mitigation strategies in place

---

**Phase 4 Status**: ✅ COMPLETE  
**Next Phase**: Phase 5 (Ticket Execution via Bob CLI)  
**Execution Mode**: Sequential (TICKET-1 → TICKET-2 → TICKET-3 → TICKET-4)
