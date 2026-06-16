# Extraction Tickets: EPIC-CCN-067

## Overview
- **Epic ID**: EPIC-CCN-067
- **Target Method**: SymmetryFindDispatchForMasterFill
- **File**: src/V12_002.Symmetry.cs
- **Total Tickets**: 3
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3)
- **Estimated Effort**: 2 hours
- **Current Complexity**: CYC=9
- **Target Complexity**: CYC=2 (main) + CYC=4 (helper1) + CYC=1 (helper2) = CYC=7 total

## Strategy
Separate filtering logic and selection logic into two focused helper methods to achieve cognitive simplicity and Jane Street alignment.

---

## TICKET-1: Extract IsValidDispatchCandidate Helper

### Scope
- **Current Method**: `SymmetryFindDispatchForMasterFill`
- **Current CYC**: 9
- **Target CYC**: 4 (for extracted helper)
- **Extraction**: Consolidate all 4 filter conditions into single predicate method

### Implementation
1. Create new private method `IsValidDispatchCandidate` with signature:
   ```csharp
   private bool IsValidDispatchCandidate(
       SymmetryDispatchContext ctx,
       string normalizedTradeType,
       MarketPosition direction,
       DateTime fillTimeUtc
   )
   ```

2. Move 4 filter conditions into helper:
   - Null check + IsResolved check
   - Direction match
   - TradeType match (normalized)
   - TTL validation

3. Return `true` if all conditions pass, `false` otherwise

4. Update main method to call helper in foreach loop

### Acceptance Criteria
- [ ] Helper method created with CYC=4
- [ ] All 4 filter conditions moved to helper
- [ ] Helper is pure function (no side effects)
- [ ] Main method calls helper correctly
- [ ] All tests pass (`dotnet test`)
- [ ] Build succeeds (`dotnet build`)
- [ ] No behavioral changes (logic preserved)
- [ ] CSharpier formatting applied

### Dependencies
- None (first ticket)

### Verification Commands
```bash
# Build check
dotnet build

# Test check
dotnet test

# Complexity check
python3 scripts/complexity_audit.py

# Format check
dotnet csharpier format src/
```

---

## TICKET-2: Extract SelectOldestCandidate Helper

### Scope
- **Current Method**: `SymmetryFindDispatchForMasterFill`
- **Current CYC**: Reduced after TICKET-1
- **Target CYC**: 1 (for extracted helper)
- **Extraction**: Isolate selection logic for finding oldest candidate

### Implementation
1. Create new private method `SelectOldestCandidate` with signature:
   ```csharp
   private SymmetryDispatchContext SelectOldestCandidate(
       SymmetryDispatchContext current,
       SymmetryDispatchContext candidate
   )
   ```

2. Move comparison logic into helper:
   - Compare current vs candidate
   - Return older of the two (or candidate if current is null)

3. Update main method to call helper when valid candidate found

### Acceptance Criteria
- [ ] Helper method created with CYC=1
- [ ] Selection logic moved to helper
- [ ] Helper is pure function (no side effects)
- [ ] Main method calls helper correctly
- [ ] All tests pass (`dotnet test`)
- [ ] Build succeeds (`dotnet build`)
- [ ] No behavioral changes (logic preserved)
- [ ] CSharpier formatting applied

### Dependencies
- **TICKET-1** must be completed first

### Verification Commands
```bash
# Build check
dotnet build

# Test check
dotnet test

# Complexity check
python3 scripts/complexity_audit.py

# Format check
dotnet csharpier format src/
```

---

## TICKET-3: Final Verification & Cleanup

### Scope
- **Current Method**: `SymmetryFindDispatchForMasterFill`
- **Target CYC**: 2 (main method only)
- **Verification**: Ensure all complexity targets met and quality gates pass

### Implementation
1. Verify main method complexity reduced to CYC=2:
   - 1 foreach loop (CYC+1)
   - 1 if statement for helper call (CYC+1)

2. Run full pre-push validation suite

3. Update manifest.json with Phase 4 completion

4. Run deploy-sync.ps1 for hard-link integrity

### Acceptance Criteria
- [ ] Main method CYC=2 (verified via complexity_audit.py)
- [ ] IsValidDispatchCandidate CYC=4
- [ ] SelectOldestCandidate CYC=1
- [ ] Total complexity CYC=7 (meets ≤8 requirement)
- [ ] All tests pass (100%)
- [ ] Build succeeds (zero errors)
- [ ] Pre-push validation passes (all 13 checks)
- [ ] Hard-link sync completed
- [ ] Manifest updated with Phase 4 status

### Dependencies
- **TICKET-1** must be completed
- **TICKET-2** must be completed

### Verification Commands
```bash
# Full complexity audit
python3 scripts/complexity_audit.py

# Pre-push validation (fast mode)
powershell -File .\scripts\pre_push_validation.ps1 -Fast

# Hard-link sync
powershell -File .\deploy-sync.ps1

# Manual test in NinjaTrader
# Press F5 and verify strategy loads without errors
```

### Manifest Update
Update `docs/brain/EPIC-CCN-067/manifest.json`:
```json
{
  "phases": {
    "4": {
      "status": "completed",
      "outputs": ["04-tickets.md"],
      "completed_at": "2026-06-15T16:59:32Z",
      "ticket_count": 3,
      "total_effort_hours": 2
    }
  }
}
```

---

## Execution Notes

### V12 DNA Compliance
- ✅ Lock-free pattern preserved (no locks introduced)
- ✅ Defensive copy pattern maintained (ToArray())
- ✅ Pure functions (no side effects)
- ✅ ASCII-only compliance (no Unicode)

### Jane Street Alignment
- ✅ Cognitive simplicity (CYC 9→2 for main method)
- ✅ Single responsibility principle (each helper has one job)
- ✅ Microsecond latency constraints met (JIT inlining candidates)
- ✅ Testing standards maintained (existing tests validate behavior)

### Risk Assessment
- **Implementation Risk**: MINIMAL (simple extraction pattern)
- **Regression Risk**: LOW (pure refactoring, no behavior changes)
- **Performance Risk**: ZERO (no allocations, JIT inlining)

### Rollback Plan
Single commit per ticket. If issues arise, revert the specific commit:
```bash
git revert <commit-hash>
```

---

## Success Criteria Summary

**Phase 4 Complete When**:
- ✅ All 3 tickets have clear scope and acceptance criteria
- ✅ Dependencies documented
- ✅ Execution order defined
- ✅ Verification commands provided
- ✅ Manifest update instructions included
- ✅ V12 DNA compliance verified
- ✅ Jane Street alignment confirmed

**Ready for Phase 5**: Ticket Execution (Bob CLI v12-engineer)
