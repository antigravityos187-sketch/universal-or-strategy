# Extraction Tickets: EPIC-CCN-072

## Overview
- **Total Tickets**: 3
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3)
- **Estimated Effort**: 2.5 hours
- **Target File**: src/V12_002.Symmetry.BracketFSM.cs
- **Target Method**: ProcessBracketEvent (lines 304-350)
- **Current Complexity**: 14
- **Target Complexity**: ≤8

## Extraction Strategy
Each ticket extracts one helper method from the ProcessBracketEvent switch statement. Execution follows complexity order (simplest first) to minimize risk and enable incremental verification.

---

## TICKET-1: Extract HandleAcceptedState

### Scope
- **Current Method**: `ProcessBracketEvent`
- **Current CYC**: 14
- **Target CYC**: 12 (after this extraction)
- **Helper CYC**: 2
- **Extraction**: Accepted/Working case logic (lines ~315-320)

### Implementation
1. Create new private method `HandleAcceptedState(FollowerBracketFSM fsm)`
2. Extract logic from Accepted/Working case:
   ```csharp
   if (fsm.State == FollowerBracketState.Submitted || 
       fsm.State == FollowerBracketState.PendingSubmit)
   {
       fsm.State = FollowerBracketState.Accepted;
   }
   ```
3. Replace switch case with method call: `HandleAcceptedState(fsm);`
4. Place helper method immediately after ProcessBracketEvent
5. Run CSharpier formatting: `dotnet csharpier format src/`
6. Verify build: `dotnet build`
7. Run tests: `dotnet test`
8. Checkpoint: Commit with message "EPIC-CCN-072: Extract HandleAcceptedState (TICKET-1)"

### Acceptance Criteria
- [ ] HandleAcceptedState method created with CYC ≤2
- [ ] ProcessBracketEvent complexity reduced to 12
- [ ] Zero compilation errors
- [ ] All tests pass (100%)
- [ ] CSharpier formatting passes
- [ ] No Roslyn violations
- [ ] Checkpoint commit created

### Dependencies
- None (first ticket)

### Estimated Time
- 30 minutes

---

## TICKET-2: Extract HandleRejectedState

### Scope
- **Current Method**: `ProcessBracketEvent`
- **Current CYC**: 12 (after TICKET-1)
- **Target CYC**: 11 (after this extraction)
- **Helper CYC**: 1
- **Extraction**: Rejected case logic (lines ~340-343)

### Implementation
1. Create new private method `HandleRejectedState(AccountEvent evt, FollowerBracketFSM fsm)`
2. Extract logic from Rejected case:
   ```csharp
   fsm.State = FollowerBracketState.Rejected;
   fsm.LastBrokerError = evt.Message;
   ```
3. Replace switch case with method call: `HandleRejectedState(evt, fsm);`
4. Place helper method after HandleAcceptedState
5. Run CSharpier formatting: `dotnet csharpier format src/`
6. Verify build: `dotnet build`
7. Run tests: `dotnet test`
8. Checkpoint: Commit with message "EPIC-CCN-072: Extract HandleRejectedState (TICKET-2)"

### Acceptance Criteria
- [ ] HandleRejectedState method created with CYC ≤1
- [ ] ProcessBracketEvent complexity reduced to 11
- [ ] Zero compilation errors
- [ ] All tests pass (100%)
- [ ] CSharpier formatting passes
- [ ] No Roslyn violations
- [ ] Checkpoint commit created

### Dependencies
- TICKET-1 must be completed first

### Estimated Time
- 30 minutes

---

## TICKET-3: Extract HandleCancelledState

### Scope
- **Current Method**: `ProcessBracketEvent`
- **Current CYC**: 11 (after TICKET-2)
- **Target CYC**: ≤8 (after this extraction)
- **Helper CYC**: 3
- **Extraction**: Cancelled case logic (lines ~330-338)

### Implementation
1. Create new private method `HandleCancelledState(AccountEvent evt, FollowerBracketFSM fsm)`
2. Extract logic from Cancelled case:
   ```csharp
   if (fsm.State == FollowerBracketState.Submitted || 
       fsm.State == FollowerBracketState.PendingSubmit)
   {
       fsm.State = FollowerBracketState.Cancelled;
   }
   else
   {
       Print($"[BracketFSM] Cancelled event for FSM in unexpected state: {fsm.State}");
   }
   ```
3. Replace switch case with method call: `HandleCancelledState(evt, fsm);`
4. Place helper method after HandleRejectedState
5. Run CSharpier formatting: `dotnet csharpier format src/`
6. Verify build: `dotnet build`
7. Run tests: `dotnet test`
8. Run complexity audit: `python scripts/complexity_audit.py`
9. Verify ProcessBracketEvent complexity ≤8
10. Checkpoint: Commit with message "EPIC-CCN-072: Extract HandleCancelledState (TICKET-3)"

### Acceptance Criteria
- [ ] HandleCancelledState method created with CYC ≤3
- [ ] ProcessBracketEvent complexity reduced to ≤8
- [ ] Zero compilation errors
- [ ] All tests pass (100%)
- [ ] CSharpier formatting passes
- [ ] No Roslyn violations
- [ ] Complexity audit confirms CYC ≤8
- [ ] Checkpoint commit created

### Dependencies
- TICKET-2 must be completed first

### Estimated Time
- 45 minutes

---

## Final Verification (After All Tickets)

### Pre-Push Validation
Run full validation suite:
```powershell
powershell -File .\scripts\pre_push_validation.ps1
```

**Blocking Checks** (must pass):
- [ ] ASCII-Only (Check #1)
- [ ] Build (Check #2)
- [ ] Unit Tests (Check #3)
- [ ] Lint (Check #4)
- [ ] Formatting (Check #5)
- [ ] PR Hygiene (Check #8)
- [ ] Complexity (Check #9)

### Hard-Link Synchronization
```powershell
powershell -File .\deploy-sync.ps1
```

**Verification**:
- [ ] deploy-sync.ps1 completes successfully
- [ ] NinjaTrader hard links updated
- [ ] No DIFF GUARD failures

### Success Criteria
- [ ] ProcessBracketEvent complexity: ≤8
- [ ] HandleAcceptedState complexity: ≤2
- [ ] HandleRejectedState complexity: ≤1
- [ ] HandleCancelledState complexity: ≤3
- [ ] Zero compilation errors
- [ ] Zero Roslyn violations
- [ ] CSharpier formatting passes
- [ ] All tests pass (100%)
- [ ] Pre-push validation passes (all blocking checks)
- [ ] deploy-sync.ps1 completes
- [ ] Git history shows 3 checkpoint commits

---

## Risk Mitigation

### Rollback Strategy
Each ticket creates a checkpoint commit. If any ticket fails:
1. Identify failing ticket (TICKET-1, TICKET-2, or TICKET-3)
2. Revert to previous checkpoint: `git reset --hard HEAD~1`
3. Review failure cause
4. Fix issue and retry ticket

### Blast Radius
- **Scope**: Single file (V12_002.Symmetry.BracketFSM.cs)
- **Impact**: Single method (ProcessBracketEvent)
- **Callers**: 1 caller (line 98 in same file)
- **Risk Level**: LOW (surgical extraction, no API changes)

### V12 DNA Compliance
- ✅ Lock-Free Actor Pattern: No locks introduced
- ✅ ASCII-Only: All string literals use ASCII
- ✅ FSM Pattern: State transitions remain explicit
- ✅ Hard-Link Integrity: deploy-sync.ps1 in final verification

---

## Bobcoin Tracking

**Phase 4 Estimated Cost**: 1.50 Bobcoins
- TICKET-1: 0.40 Bobcoins
- TICKET-2: 0.40 Bobcoins
- TICKET-3: 0.50 Bobcoins
- Final Verification: 0.20 Bobcoins

**Cumulative Epic Cost**: 2.24 Bobcoins (Phases 0-4)

---

**Ticket Generation Status**: ✅ COMPLETE
**Ready for Phase 5**: ✅ YES
**Engineer**: Bob CLI (`v12-engineer` mode)
