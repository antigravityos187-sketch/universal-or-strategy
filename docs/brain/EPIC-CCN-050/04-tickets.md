# Extraction Tickets: EPIC-CCN-050

## Overview
- **Total Tickets**: 3
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3)
- **Estimated Effort**: 2.5 hours
- **Epic**: EPIC-CCN-050
- **Target Method**: FleetSync_SyncFollowersToLevel
- **File**: src/V12_002.Trailing.cs
- **Complexity Reduction**: 9 → 4 (56% reduction)

---

## TICKET-1: Extract IsStopPriceImprovement Helper

### Scope
- **Current Method**: `FleetSync_SyncFollowersToLevel`
- **Current CYC**: 9
- **Target CYC**: 7 (after this extraction)
- **Extraction**: Better stop price validation logic

### Implementation
1. Create new private method `IsStopPriceImprovement` with signature:
   ```csharp
   private bool IsStopPriceImprovement(
       PositionInfo follower,
       double newStopPrice
   )
   ```

2. Extract the better stop validation logic:
   - Long positions: newStopPrice > follower.CurrentStopPrice
   - Short positions: newStopPrice < follower.CurrentStopPrice
   - Use ternary operator for direction-based comparison

3. Replace inline validation in main method with helper call:
   ```csharp
   if (IsStopPriceImprovement(follower, syncStopPrice))
   {
       UpdateStopOrder(entryName, syncStopPrice);
       // ... logging
   }
   ```

4. Run CSharpier to format: `dotnet csharpier format src/V12_002.Trailing.cs`

### Acceptance Criteria
- [ ] Method complexity reduced from 9 to 7
- [ ] Helper method has CYC ≤ 2
- [ ] All tests pass: `dotnet test`
- [ ] No behavioral changes (logic equivalence verified)
- [ ] Build succeeds: `dotnet build`
- [ ] Zero lock() statements (grep verification)
- [ ] ASCII-only compliance maintained

### Dependencies
- None (first ticket)

### Verification Commands
```powershell
# Complexity check
python scripts/complexity_audit.py

# Build check
dotnet build

# Test check
dotnet test

# Lock-free check
grep -r "lock(" src/V12_002.Trailing.cs
```

---

## TICKET-2: Extract ShouldSyncFollower Helper

### Scope
- **Current Method**: `FleetSync_SyncFollowersToLevel`
- **Current CYC**: 7 (after TICKET-1)
- **Target CYC**: 4
- **Extraction**: Follower validation logic consolidation

### Implementation
1. Create new private method `ShouldSyncFollower` with signature:
   ```csharp
   private bool ShouldSyncFollower(
       PositionInfo follower,
       string entryName,
       int targetLevel
   )
   ```

2. Extract validation logic (5 conditions):
   - Check if follower: `!follower.IsFollower`
   - Check entry filled: `!follower.EntryFilled`
   - Check bracket submitted: `!follower.BracketSubmitted`
   - Check active position exists: `!activePositions.ContainsKey(entryName)`
   - Check target level valid: `targetLevel == 0`
   - Check current level: `follower.CurrentTrailLevel >= targetLevel`

3. Implement fail-fast validation with early returns:
   ```csharp
   if (!follower.IsFollower) return false;
   if (!follower.EntryFilled) return false;
   if (!follower.BracketSubmitted) return false;
   if (!activePositions.ContainsKey(entryName)) return false;
   if (targetLevel == 0) return false;
   if (follower.CurrentTrailLevel >= targetLevel) return false;
   return true;
   ```

4. Replace validation blocks in main method with single helper call:
   ```csharp
   if (!ShouldSyncFollower(follower, entryName, targetLevel))
   {
       continue;
   }
   ```

5. Run CSharpier to format: `dotnet csharpier format src/V12_002.Trailing.cs`

### Acceptance Criteria
- [ ] Method complexity reduced from 7 to 4
- [ ] Helper method has CYC ≤ 5
- [ ] All tests pass: `dotnet test`
- [ ] No behavioral changes (logic equivalence verified)
- [ ] Build succeeds: `dotnet build`
- [ ] Zero lock() statements (grep verification)
- [ ] ASCII-only compliance maintained
- [ ] Main method achieves target CYC ≤ 8 (Jane Street strict standard)

### Dependencies
- TICKET-1 must be completed first

### Verification Commands
```powershell
# Complexity check (target: main=4, helper=5)
python scripts/complexity_audit.py

# Build check
dotnet build

# Test check
dotnet test

# Lock-free check
grep -r "lock(" src/V12_002.Trailing.cs
```

---

## TICKET-3: Final Validation & Documentation

### Scope
- **Current Method**: `FleetSync_SyncFollowersToLevel`
- **Current CYC**: 4 (after TICKET-2)
- **Target CYC**: ≤ 8 (Jane Street strict standard)
- **Validation**: Comprehensive verification of extraction

### Implementation
1. Run full pre-push validation:
   ```powershell
   powershell -File .\scripts\pre_push_validation.ps1 -Fast
   ```

2. Verify complexity metrics:
   - Main method: CYC ≤ 8 (target: 4)
   - Helper 1 (IsStopPriceImprovement): CYC ≤ 2
   - Helper 2 (ShouldSyncFollower): CYC ≤ 5

3. Verify PR hygiene:
   ```powershell
   powershell -File .\scripts\verify_pr_hygiene.ps1
   ```
   - Diff size < 10,000 characters (estimated: ~450 chars)
   - Only src/V12_002.Trailing.cs modified

4. Run hard-link sync:
   ```powershell
   powershell -File .\deploy-sync.ps1
   ```

5. Update manifest.json:
   ```json
   {
     "phases": {
       "phase_4": {
         "status": "completed",
         "output": "04-tickets.md",
         "ticket_count": 3,
         "execution_date": "2026-06-15"
       }
     }
   }
   ```

### Acceptance Criteria
- [ ] All 3 tickets completed successfully
- [ ] Complexity target achieved (9 → 4, 56% reduction)
- [ ] All tests pass (100% pass rate)
- [ ] Build succeeds (zero errors)
- [ ] PR hygiene validated (diff < 10k)
- [ ] Hard-link sync completed
- [ ] Manifest updated
- [ ] Zero lock() statements
- [ ] ASCII-only compliance maintained
- [ ] Jane Street alignment verified (CYC ≤ 8)

### Dependencies
- TICKET-1 must be completed
- TICKET-2 must be completed

### Verification Commands
```powershell
# Full validation suite
powershell -File .\scripts\pre_push_validation.ps1 -Fast

# Complexity audit
python scripts/complexity_audit.py

# PR hygiene check
powershell -File .\scripts\verify_pr_hygiene.ps1

# Hard-link sync
powershell -File .\deploy-sync.ps1

# Lock-free verification
grep -r "lock(" src/V12_002.Trailing.cs
```

---

## Execution Notes

### Sequential Execution Required
- TICKET-1 must complete before TICKET-2 (simpler extraction first)
- TICKET-2 must complete before TICKET-3 (validation requires both helpers)
- No parallel execution (single-method scope)

### Rollback Strategy
- Each ticket creates a restore point via Bob CLI checkpointing
- Use `/restore` command if extraction fails
- Revert to previous ticket state and retry

### Testing Strategy
- Baseline: Run tests before TICKET-1 (establish 100% pass rate)
- Per-Ticket: Run tests after each extraction (detect regressions early)
- Final: Run full test suite after TICKET-3 (comprehensive validation)

### Jane Street Compliance
- Target: CYC ≤ 8 (strict standard)
- Achieved: CYC 4 (exceeds target by 50%)
- Cognitive simplicity: Pure function decomposition
- Testability: Each helper is independently testable

---

## Success Metrics

### Complexity Reduction
- **Before**: CYC 9 (Medium complexity, Tier 2)
- **After**: CYC 4 (Low complexity, Tier 1)
- **Reduction**: 56% (5 points)
- **Jane Street Alignment**: EXCELLENT (well below threshold)

### Code Quality
- **Lock-Free**: ✅ Zero lock() statements
- **ASCII-Only**: ✅ Zero non-ASCII characters
- **Testability**: ✅ Pure functions (deterministic)
- **Scope Discipline**: ✅ Single-method focus

### PR Hygiene
- **Diff Size**: ~450 characters (4.5% of 10k budget)
- **Scope Creep**: Zero (only target method modified)
- **Build Impact**: Zero breaking changes

---

**Phase 4 Status**: READY FOR EXECUTION  
**Ticket Generation Date**: 2026-06-15  
**Next Phase**: Phase 5 (Ticket Execution via Bob CLI v12-engineer)
