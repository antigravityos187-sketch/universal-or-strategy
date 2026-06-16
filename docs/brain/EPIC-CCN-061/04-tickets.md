# Extraction Tickets: EPIC-CCN-061

## Overview
- **Total Tickets**: 2
- **Execution Order**: Sequential (TICKET-1 → TICKET-2)
- **Estimated Effort**: 2 hours
- **Target Method**: SubmitAndRegisterFleetOrders
- **File**: src/V12_002.SIMA.Fleet.cs
- **Current Complexity**: 11
- **Target Complexity**: 8

## TICKET-1: Extract PrepareOrdersForSubmission Helper

### Scope
- **Current Method**: `SubmitAndRegisterFleetOrders`
- **Current CYC**: 11
- **Target CYC**: 9 (after this ticket)
- **Extraction**: Array preparation and validation logic (lines 184-188)

### Implementation
1. Create new private method `PrepareOrdersForSubmission` immediately after main method (line 205)
2. Method signature: `private Order[] PrepareOrdersForSubmission(Order[] orders, int orderCount)`
3. Extract array trimming logic:
   - Check if `orderCount < orders.Length`
   - If true, create new array with `Array.Copy(orders, orderCount)`
   - Return trimmed or original array
4. Replace lines 184-188 in main method with single call: `orders = PrepareOrdersForSubmission(orders, orderCount);`
5. Add XML doc comment: `/// <summary>Validates and trims order array to actual count.</summary>`

### Acceptance Criteria
- [ ] Helper method created with CYC ≤ 2
- [ ] Main method complexity reduced to 9
- [ ] Method signature matches specification
- [ ] XML doc comment added
- [ ] All tests pass
- [ ] No behavioral changes
- [ ] Build succeeds
- [ ] CSharpier formatting check passes
- [ ] No lock() statements introduced

### Dependencies
- None (first ticket)

### Verification Commands
```powershell
# Format check
dotnet csharpier check src/V12_002.SIMA.Fleet.cs

# Complexity audit
python scripts/complexity_audit.py

# Build
powershell -File .\scripts\build_readiness.ps1
```

---

## TICKET-2: Extract UpdateFollowerBracketState Helper

### Scope
- **Current Method**: `SubmitAndRegisterFleetOrders`
- **Current CYC**: 9 (after TICKET-1)
- **Target CYC**: 2 (final)
- **Extraction**: FSM state update logic (lines 194-203)

### Implementation
1. Create new private method `UpdateFollowerBracketState` after PrepareOrdersForSubmission helper
2. Method signature: `private void UpdateFollowerBracketState(string fleetEntryName)`
3. Extract FSM update logic:
   - TryGetValue from _followerBrackets dictionary
   - Check if pFsm exists and state is AwaitingDispatch
   - Update pFsm.State to Dispatched
   - Update pFsm.LastUpdateUtc to DateTime.UtcNow
4. Replace lines 194-203 in main method with single call: `UpdateFollowerBracketState(fleetEntryName);`
5. Add XML doc comment: `/// <summary>Updates FollowerBracket FSM state after order submission.</summary>`

### Acceptance Criteria
- [ ] Helper method created with CYC ≤ 4
- [ ] Main method complexity reduced to 2
- [ ] Method signature matches specification
- [ ] XML doc comment added
- [ ] All tests pass
- [ ] No behavioral changes
- [ ] Build succeeds
- [ ] CSharpier formatting check passes
- [ ] No lock() statements introduced
- [ ] Final complexity audit shows CYC ≤ 8

### Dependencies
- TICKET-1 must be completed first

### Verification Commands
```powershell
# Format check
dotnet csharpier check src/V12_002.SIMA.Fleet.cs

# Complexity audit (final verification)
python scripts/complexity_audit.py

# Full pre-push validation
powershell -File .\scripts\pre_push_validation.ps1 -Fast

# Hard-link sync
powershell -File .\deploy-sync.ps1
```

---

## Final Verification Checklist

After completing both tickets:

- [ ] Main method CYC = 2
- [ ] Helper 1 CYC = 2
- [ ] Helper 2 CYC = 4
- [ ] Total complexity = 8 (meets Jane Street standard)
- [ ] Zero lock() statements
- [ ] ASCII-only compliance maintained
- [ ] Build succeeds
- [ ] All tests pass
- [ ] Hard-links synchronized
- [ ] PR diff < 10k characters
- [ ] No whitespace mutations outside target method

## Execution Notes

### Branch Strategy
- Create feature branch: `feature/epic-ccn-061-submit-fleet-orders`
- Base branch: `main`
- Merge strategy: Squash and merge

### Testing Strategy
- Existing integration tests should pass (black-box behavior unchanged)
- Manual F5 test in NinjaTrader (smoke test)
- Optional: Add unit tests for helpers (not blocking)

### Risk Mitigation
- Checkpointing enabled via Bob CLI
- Restore available via `/restore` command
- Conservative extraction (no algorithmic changes)
- Helpers have clear, testable contracts

---

**Generated**: 2026-06-15
**Protocol**: V12.23 Phase 4 (Ticket Generation)
**Epic**: EPIC-CCN-061
**Status**: Ready for Phase 5 (Execution)
