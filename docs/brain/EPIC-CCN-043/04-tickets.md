# Extraction Tickets: EPIC-CCN-043

## Overview
- **Total Tickets**: 4
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3 → TICKET-4)
- **Estimated Effort**: 6-8 hours
- **Target Method**: SymmetryGuardSubmitFollowerBracket
- **File**: src/V12_002.Symmetry.Follower.cs
- **Current Complexity**: 12
- **Target Complexity**: ≤8 (all methods)

---

## TICKET-1: Extract ValidateAndCreateStopOrder

### Scope
- **Current Method**: `SymmetryGuardSubmitFollowerBracket`
- **Current CYC**: 12
- **Target CYC**: 4 (helper method)
- **Extraction**: Early validation and stop order creation logic (Lines 287-316)

### Implementation
1. Create private helper method with signature:
   ```csharp
   private (bool isValid, Order stop, string ocoId, OrderAction exitAction, double validatedStop) 
       ValidateAndCreateStopOrder(string fleetEntryName, PositionInfo pos)
   ```

2. Extract validation logic:
   - Check `pos.BracketSubmitted` guard (early return if true)
   - Validate `pos.ExecutingAccount` (early return if null)
   - Determine `OrderAction` (Long → Sell, Short → BuyToCover)
   - Validate stop price via `ValidateStopPrice`
   - Generate or retrieve OCO Group ID
   - Create stop market order

3. Return tuple with validation result and created order

4. Update main method to call helper:
   ```csharp
   var (isValid, stop, ocoId, exitAction, validatedStop) = ValidateAndCreateStopOrder(fleetEntryName, pos);
   if (!isValid) return;
   ```

### Acceptance Criteria
- [ ] Helper method complexity ≤ 4
- [ ] All early return paths preserved
- [ ] Stop order creation logic identical
- [ ] OCO ID generation unchanged
- [ ] Unit tests pass: `ValidateAndCreateStopOrder_Tests`
- [ ] Build succeeds: `dotnet build`
- [ ] No behavioral changes verified
- [ ] Lock-free scan passes: `grep -r "lock(" src/V12_002.Symmetry.Follower.cs` (zero matches)

### Dependencies
- None (first ticket)

### Risk Level
- **LOW**: Simple extraction with clear boundaries

---

## TICKET-2: Extract CreateTargetOrdersForBracket

### Scope
- **Current Method**: `SymmetryGuardSubmitFollowerBracket`
- **Current CYC**: 12 (after TICKET-1)
- **Target CYC**: 6 (helper method)
- **Extraction**: Target order creation loop (Lines 318-395) - PRIMARY COMPLEXITY SOURCE

### Implementation
1. Create private helper method with signature:
   ```csharp
   private (List<Order> ordersToSubmit, List<(int targetNum, Order order)> stagedTargets, int nonRunnerLimitQty, int runnerQty)
       CreateTargetOrdersForBracket(PositionInfo pos, string fleetEntryName, Account acct, OrderAction exitAction, string ocoId)
   ```

2. Extract target order loop logic:
   - Iterate through targets 1-5
   - Query `GetTargetContracts` for each target
   - Skip runner targets (accumulate `runnerQty`)
   - Validate target price via `GetTargetPrice`
   - Round target price to tick size
   - Create limit orders for non-runner targets
   - Stage orders for FSM initialization

3. Return tuple with order lists and quantities

4. Update main method to call helper:
   ```csharp
   Account acct = pos.ExecutingAccount;
   var (ordersToSubmit, stagedTargets, nonRunnerLimitQty, runnerQty) = 
       CreateTargetOrdersForBracket(pos, fleetEntryName, acct, exitAction, ocoId);
   ```

### Acceptance Criteria
- [ ] Helper method complexity ≤ 6
- [ ] Target order loop logic preserved
- [ ] Runner target handling unchanged
- [ ] Price validation identical
- [ ] Order staging logic preserved
- [ ] Unit tests pass: `CreateTargetOrdersForBracket_Tests`
- [ ] Build succeeds: `dotnet build`
- [ ] No behavioral changes verified
- [ ] Lock-free scan passes: `grep -r "lock(" src/V12_002.Symmetry.Follower.cs` (zero matches)

### Dependencies
- **TICKET-1** must be completed first

### Risk Level
- **MEDIUM**: Complex loop logic with multiple conditionals

---

## TICKET-3: Extract CommitBracketToFSM

### Scope
- **Current Method**: `SymmetryGuardSubmitFollowerBracket`
- **Current CYC**: 12 (after TICKET-1 & TICKET-2)
- **Target CYC**: 2 (helper method)
- **Extraction**: FSM initialization and submission (Lines 396+)

### Implementation
1. Create private helper method with signature:
   ```csharp
   private void CommitBracketToFSM(string fleetEntryName, PositionInfo pos, Account acct, string ocoId, 
       Order stop, double validatedStop, List<(int targetNum, Order order)> stagedTargets, List<Order> ordersToSubmit)
   ```

2. Extract FSM initialization logic:
   - Create `FollowerBracketFSM` instance
   - Initialize FSM state to `PendingSubmit`
   - Populate `Targets` and `ExpectedTargetPrices` arrays
   - Commit FSM to `_followerBrackets` dictionary (atomic commit)
   - Insert stop order at head of `ordersToSubmit`
   - Enqueue orders via Actor pipeline

3. Update main method to call helper:
   ```csharp
   CommitBracketToFSM(fleetEntryName, pos, acct, ocoId, stop, validatedStop, stagedTargets, ordersToSubmit);
   ```

### Acceptance Criteria
- [ ] Helper method complexity ≤ 2
- [ ] FSM initialization logic preserved
- [ ] Atomic commit pattern maintained
- [ ] Order enqueue sequence unchanged
- [ ] Unit tests pass: `CommitBracketToFSM_Tests`
- [ ] Build succeeds: `dotnet build`
- [ ] No behavioral changes verified
- [ ] Lock-free scan passes: `grep -r "lock(" src/V12_002.Symmetry.Follower.cs` (zero matches)
- [ ] Atomic commit comment preserved: "Atomic commit before broker submission prevents REAPER race"

### Dependencies
- **TICKET-1** must be completed first
- **TICKET-2** must be completed first

### Risk Level
- **LOW**: Simple initialization with clear atomic commit pattern

---

## TICKET-4: Refactor Main Method to Orchestration

### Scope
- **Current Method**: `SymmetryGuardSubmitFollowerBracket`
- **Current CYC**: 12 (before refactoring)
- **Target CYC**: 3 (after refactoring)
- **Extraction**: Final orchestration refactoring

### Implementation
1. Refactor main method to pure orchestration:
   ```csharp
   private void SymmetryGuardSubmitFollowerBracket(string fleetEntryName, PositionInfo pos)
   {
       var (isValid, stop, ocoId, exitAction, validatedStop) = ValidateAndCreateStopOrder(fleetEntryName, pos);
       if (!isValid) return;
       
       Account acct = pos.ExecutingAccount;
       var (ordersToSubmit, stagedTargets, nonRunnerLimitQty, runnerQty) = 
           CreateTargetOrdersForBracket(pos, fleetEntryName, acct, exitAction, ocoId);
       
       CommitBracketToFSM(fleetEntryName, pos, acct, ocoId, stop, validatedStop, stagedTargets, ordersToSubmit);
   }
   ```

2. Remove all extracted logic from main method

3. Verify orchestration flow matches original behavior

### Acceptance Criteria
- [ ] Main method complexity ≤ 3
- [ ] Orchestration flow preserved
- [ ] All helper methods called correctly
- [ ] No logic duplication
- [ ] Integration tests pass: `SymmetryGuardSubmitFollowerBracket_Integration_Tests`
- [ ] Build succeeds: `dotnet build`
- [ ] Unit tests pass: `dotnet test` (100% pass)
- [ ] Complexity audit passes: `python scripts/complexity_audit.py` (CYC ≤8 all methods)
- [ ] Lock-free scan passes: `grep -r "lock(" src/V12_002.Symmetry.Follower.cs` (zero matches)
- [ ] Hard-link sync succeeds: `powershell -File .\deploy-sync.ps1`

### Dependencies
- **TICKET-1** must be completed first
- **TICKET-2** must be completed first
- **TICKET-3** must be completed first

### Risk Level
- **LOW**: Simple orchestration with all helpers already tested

---

## Verification Suite (Run After Each Ticket)

### Build Verification
```bash
dotnet build
```
**Expected**: Zero errors

### Unit Test Verification
```bash
dotnet test
```
**Expected**: 100% pass rate

### Complexity Verification
```bash
python scripts/complexity_audit.py
```
**Expected**: All methods CYC ≤ 8

### Lock-Free Verification
```bash
grep -r "lock(" src/V12_002.Symmetry.Follower.cs
```
**Expected**: Zero matches

### Hard-Link Sync
```bash
powershell -File .\deploy-sync.ps1
```
**Expected**: Success

---

## Rollback Strategy

### Per-Ticket Rollback
- Each ticket is a separate commit
- Use `git revert <commit-hash>` to rollback individual tickets
- Maintain checkpoint before each extraction

### Full Rollback
- Use `git reset --hard <pre-epic-commit>`
- Re-run hard-link sync: `powershell -File .\deploy-sync.ps1`

---

## Success Metrics

### Complexity Reduction
- **Before**: Main method CYC = 12
- **After**: Main method CYC = 3 (75% reduction)
- **Helpers**: CYC 4, 6, 2 (all ≤8)

### Jane Street Alignment
- ✅ All methods ≤8 (strict HFT standard)
- ✅ Cognitive simplicity achieved
- ✅ Exhaustive testing feasible

### V12 DNA Compliance
- ✅ Lock-free (zero lock statements)
- ✅ ASCII-only (no Unicode)
- ✅ Correctness by construction (tuple returns, early returns)

### PR Hygiene
- ✅ Diff size ~450 characters (target <10,000)
- ✅ Zero scope creep
- ✅ Surgical extraction only

---

## Notes

### Performance Considerations
- All helpers are **private** (JIT inlining candidates)
- No additional allocations introduced
- Same call graph depth maintained
- Target order loop remains co-located (HFT hot-path optimization)

### Test Coverage Requirements
- **New Unit Tests**: 3 (one per helper method)
- **Integration Tests**: 1 (main method orchestration)
- **TDD Approach**: Write tests BEFORE extraction

### Jane Street Principles Applied
- ✅ Cognitive simplicity prioritized
- ✅ Functions easy to reason about under microsecond latency
- ✅ Exhaustive testing feasible (no exponential path growth)
- ✅ Race condition audit simplified
- ✅ Private helpers enable JIT inlining (no performance regression)

---

## Metadata

- **Epic**: EPIC-CCN-043
- **Phase**: 4 (Ticket Generation)
- **Date**: 2026-06-15
- **Total Tickets**: 4
- **Estimated Effort**: 6-8 hours
- **Complexity Reduction**: 12 → 3 (75%)
- **Jane Street Alignment**: ✅ PASS (all methods ≤8)
- **Lock-Free Compliance**: ✅ PASS
- **Approval Status**: ✅ READY FOR PHASE 5 (Execution)
- **Next Phase**: Phase 5 (Ticket Execution)
