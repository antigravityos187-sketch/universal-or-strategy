# Extraction Tickets: EPIC-CCN-069

## Overview
- **Epic ID**: EPIC-CCN-069
- **Target Method**: GetFsmExpectedPosition
- **Target File**: src/V12_002.Symmetry.BracketFSM.cs
- **Current Complexity**: 14 (CYC)
- **Target Complexity**: ≤8 (Jane Street aligned)
- **Total Tickets**: 4
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3 → TICKET-4)
- **Estimated Effort**: 2-3 hours
- **Complexity Reduction**: 71% (14 → 4)

---

## TICKET-1: Extract IsAccountMatch Helper

### Scope
- **Current Method**: `GetFsmExpectedPosition`
- **Current CYC**: 14
- **Extraction**: Account matching logic
- **New Helper**: `IsAccountMatch`
- **Helper CYC**: 2
- **Access Modifier**: private static

### Implementation

1. **Create Helper Method**
   ```csharp
   private static bool IsAccountMatch(FollowerBracketFSM fsm, string accountName)
   {
       return fsm != null && fsm.AccountName == accountName;
   }
   ```

2. **Add Method Location**
   - Insert after line 372 (before GetFsmExpectedPosition)
   - Add XML documentation comment explaining purpose

3. **Add Unit Tests**
   - Test null FSM → returns false
   - Test matching account → returns true
   - Test non-matching account → returns false

### Acceptance Criteria
- [ ] Helper method created with correct signature
- [ ] Method is private static (no instance state)
- [ ] XML documentation added
- [ ] Unit tests added and passing
- [ ] Build succeeds with zero errors
- [ ] CSharpier formatting applied
- [ ] No behavioral changes to GetFsmExpectedPosition yet

### Dependencies
- None (first ticket)

### Verification Commands
```powershell
# Build check
dotnet build src/V12_002.csproj

# Format check
dotnet csharpier check src/V12_002.Symmetry.BracketFSM.cs

# Test check (if tests added)
dotnet test tests/V12_Performance.Tests/
```

---

## TICKET-2: Extract IsActiveState Helper

### Scope
- **Current Method**: `GetFsmExpectedPosition`
- **Current CYC**: 14
- **Extraction**: Active state validation logic
- **New Helper**: `IsActiveState`
- **Helper CYC**: 1
- **Access Modifier**: private static

### Implementation

1. **Create Helper Method**
   ```csharp
   private static bool IsActiveState(FollowerBracketState state)
   {
       return state == FollowerBracketState.Active
           || state == FollowerBracketState.Accepted
           || state == FollowerBracketState.Submitted
           || state == FollowerBracketState.PendingSubmit
           || state == FollowerBracketState.Replacing
           || state == FollowerBracketState.Modifying;
   }
   ```

2. **Add Method Location**
   - Insert after IsAccountMatch helper
   - Add XML documentation comment listing all 6 active states

3. **Add Unit Tests**
   - Test all 6 active states → returns true
   - Test inactive states (Cancelled, Rejected, etc.) → returns false
   - Test edge case states

### Acceptance Criteria
- [ ] Helper method created with correct signature
- [ ] Method is private static (pure function)
- [ ] All 6 active states correctly identified
- [ ] XML documentation added
- [ ] Unit tests added and passing (100% state coverage)
- [ ] Build succeeds with zero errors
- [ ] CSharpier formatting applied
- [ ] No behavioral changes to GetFsmExpectedPosition yet

### Dependencies
- TICKET-1 must be completed first

### Verification Commands
```powershell
# Build check
dotnet build src/V12_002.csproj

# Format check
dotnet csharpier check src/V12_002.Symmetry.BracketFSM.cs

# Test check
dotnet test tests/V12_Performance.Tests/
```

---

## TICKET-3: Extract CalculatePositionContribution Helper

### Scope
- **Current Method**: `GetFsmExpectedPosition`
- **Current CYC**: 14
- **Extraction**: Position calculation logic
- **New Helper**: `CalculatePositionContribution`
- **Helper CYC**: 3
- **Access Modifier**: private static

### Implementation

1. **Create Helper Method**
   ```csharp
   private static int CalculatePositionContribution(FollowerBracketFSM fsm)
   {
       if (fsm.EntryOrder == null)
       {
           // Edge case: Hydrated Active FSM without entry order
           if (fsm.State == FollowerBracketState.Active)
           {
               return 0;
           }
           return 0;
       }

       if (fsm.EntryOrder.OrderAction == OrderAction.Buy 
           || fsm.EntryOrder.OrderAction == OrderAction.BuyToCover)
       {
           return fsm.EntryOrder.Quantity;
       }
       else if (fsm.EntryOrder.OrderAction == OrderAction.Sell 
                || fsm.EntryOrder.OrderAction == OrderAction.SellShort)
       {
           return -fsm.EntryOrder.Quantity;
       }

       return 0;
   }
   ```

2. **Add Method Location**
   - Insert after IsActiveState helper
   - Add XML documentation comment explaining position sign logic
   - Preserve edge case comment about hydrated Active FSM

3. **Add Unit Tests**
   - Test Buy order → positive contribution
   - Test Sell order → negative contribution
   - Test BuyToCover order → positive contribution
   - Test SellShort order → negative contribution
   - Test null EntryOrder → returns 0
   - Test Active state with null order → returns 0 (edge case)

### Acceptance Criteria
- [ ] Helper method created with correct signature
- [ ] Method is private static (no instance state)
- [ ] All order actions handled correctly
- [ ] Edge case comment preserved
- [ ] XML documentation added
- [ ] Unit tests added and passing (100% path coverage)
- [ ] Build succeeds with zero errors
- [ ] CSharpier formatting applied
- [ ] No behavioral changes to GetFsmExpectedPosition yet

### Dependencies
- TICKET-1 must be completed first
- TICKET-2 must be completed first

### Verification Commands
```powershell
# Build check
dotnet build src/V12_002.csproj

# Format check
dotnet csharpier check src/V12_002.Symmetry.BracketFSM.cs

# Test check
dotnet test tests/V12_Performance.Tests/

# Complexity check
python scripts/complexity_audit.py
```

---

## TICKET-4: Refactor Main Method to Use Helpers

### Scope
- **Current Method**: `GetFsmExpectedPosition`
- **Current CYC**: 14
- **Target CYC**: 4
- **Refactoring**: Replace inline logic with helper calls
- **Complexity Reduction**: 71% (14 → 4)

### Implementation

1. **Refactor GetFsmExpectedPosition**
   ```csharp
   private int GetFsmExpectedPosition(string accountName)
   {
       int sum = 0;
       foreach (var f in _followerBrackets.Values)
       {
           if (!IsAccountMatch(f, accountName))
           {
               continue;
           }

           if (!IsActiveState(f.State))
           {
               continue;
           }

           sum += CalculatePositionContribution(f);
       }
       return sum;
   }
   ```

2. **Preserve Method Signature**
   - No changes to parameters
   - No changes to return type
   - No changes to access modifier

3. **Preserve Comments**
   - Keep any existing XML documentation
   - Ensure edge case comments are in helper methods

4. **Run Full Test Suite**
   - All existing tests must pass unchanged
   - Integration tests verify behavior preservation

### Acceptance Criteria
- [ ] Main method refactored to use all 3 helpers
- [ ] Method signature unchanged (backward compatible)
- [ ] Method complexity reduced to CYC = 4
- [ ] All existing tests pass (regression check)
- [ ] Build succeeds with zero errors
- [ ] CSharpier formatting applied
- [ ] Hard-link sync successful (deploy-sync.ps1)
- [ ] Complexity audit shows CYC ≤8 for all methods
- [ ] No lock() statements introduced
- [ ] ASCII-only compliance maintained

### Dependencies
- TICKET-1 must be completed first
- TICKET-2 must be completed first
- TICKET-3 must be completed first

### Verification Commands
```powershell
# Build check
dotnet build src/V12_002.csproj

# Format check
dotnet csharpier check src/V12_002.Symmetry.BracketFSM.cs

# Full test suite
dotnet test tests/V12_Performance.Tests/

# Complexity audit
python scripts/complexity_audit.py

# Hard-link sync
powershell -File .\deploy-sync.ps1

# Pre-push validation (fast mode)
powershell -File .\scripts\pre_push_validation.ps1 -Fast
```

### Post-Implementation Verification

1. **Complexity Metrics**
   - GetFsmExpectedPosition: CYC = 4 ✅
   - IsAccountMatch: CYC = 2 ✅
   - IsActiveState: CYC = 1 ✅
   - CalculatePositionContribution: CYC = 3 ✅

2. **V12 DNA Compliance**
   - Lock-free pattern: ✅ (read-only query)
   - Correctness by construction: ✅ (type-safe helpers)
   - ASCII-only: ✅ (no Unicode)
   - Jane Street aligned: ✅ (CYC ≤8)
   - Hard-link integrity: ✅ (deploy-sync.ps1)

3. **PR Hygiene**
   - Diff size: ~150 lines (well below 10k limit) ✅
   - No whitespace mutation: ✅ (CSharpier enforced)
   - Rebase mandate: ✅ (pre-push validation)
   - Three-tier branch: ✅ (source code only)

---

## Execution Strategy

### Sequential Execution (Recommended)
Execute tickets in order 1 → 2 → 3 → 4 to minimize risk:
- Each ticket adds one helper method
- Test after each ticket
- Rollback capability via Bob CLI checkpointing
- Final ticket integrates all helpers

### Parallel Execution (Advanced)
TICKETS 1-3 can be executed in parallel (independent helpers), then TICKET-4 integrates:
- Requires careful merge coordination
- Higher risk of conflicts
- Only recommended for experienced engineers

### Rollback Strategy
- Bob CLI checkpointing enabled by default
- Use `/restore` command to rollback to previous state
- Each ticket is a checkpoint boundary

---

## Success Metrics

### Quantitative
- **Complexity Reduction**: 71% (14 → 4)
- **Helper Methods**: 3 (all CYC ≤3)
- **Test Coverage**: 100% path coverage for helpers
- **Build Status**: Zero errors
- **Diff Size**: ~150 lines (well below 10k limit)

### Qualitative
- **Readability**: Each method has single, clear purpose
- **Testability**: Pure functions enable isolated unit tests
- **Maintainability**: Low complexity enables easy reasoning
- **Performance**: No allocations, minimal branching

---

## Phase 4 Sign-Off

**Phase 4 Completed**: 2026-06-15
**Tickets Generated**: 4
**Execution Order**: Sequential (1 → 2 → 3 → 4)
**Estimated Effort**: 2-3 hours
**Next Phase**: Phase 5 (Recursive Execution)
**Primary Engineer**: Bob CLI (`v12-engineer`)

---

**END OF TICKET BREAKDOWN**
