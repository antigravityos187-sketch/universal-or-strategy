# Execution Guide: EPIC-CCN-128

**Epic ID**: EPIC-CCN-128  
**Method**: `SymmetryGuardReplaceExistingFollowerTarget`  
**File**: `src/V12_002.Symmetry.Replace.cs`  
**Total Tickets**: 5  
**Estimated Time**: 3.5 hours

---

## Execution Order

### Sequential Execution (MANDATORY)

Tickets MUST be executed in this order due to dependencies:

```
Ticket 1 (Foundation)
    ↓
Ticket 2 (Cleanup)
    ↓
Ticket 3 (Validation)
    ↓
Ticket 4 (FSM Spec)
    ↓
Ticket 5 (Main Refactor)
```

**Rationale**: Each ticket builds on the previous one. Ticket 5 requires all 4 helpers to be extracted first.

---

## Ticket Execution Workflow

### Per-Ticket Steps

1. **Read Ticket Brief**: `docs/brain/EPIC-CCN-128/ticket-XX-*.md`
2. **Extract Method**: Add helper to `src/V12_002.Symmetry.Replace.cs`
3. **Update Caller**: Modify main method to use helper
4. **Build**: `dotnet build`
5. **Deploy**: `powershell -File .\deploy-sync.ps1`
6. **Unit Test**: `dotnet test --filter "FullyQualifiedName~[TestClass]"`
7. **Integration Test**: F5 in NinjaTrader IDE
8. **Complexity Audit**: `python scripts/complexity_audit.py --file src/V12_002.Symmetry.Replace.cs`
9. **Commit**: `git commit -m "[EPIC-CCN-128] ticket-XX: description -- CYC before->after [BUILD_TAG]"`

### After All Tickets

1. **Full Test Suite**: `dotnet test --filter "FullyQualifiedName~Symmetry"`
2. **Pre-Push Validation**: `powershell -File .\scripts\pre_push_validation.ps1 -Fast`
3. **Update Manifest**: Mark Phase 5 complete
4. **Generate Completion Report**: `05-completion-report.md`
5. **Update AGENTS.md**: Add to "Recent Major Refactors" table
6. **Push**: `git push origin feature/src-epic-128`
7. **Create PR**: Title: `EPIC-CCN-128: Reduce SymmetryGuardReplaceExistingFollowerTarget complexity (CYC 18→3)`

---

## Ticket Details

### Ticket 1: Extract ShouldSkipTargetReplacement
- **File**: `ticket-01-extract-should-skip.md`
- **Priority**: P1 (Foundation)
- **Time**: 30 minutes
- **CYC**: 3
- **Tests**: 8 unit tests
- **Dependencies**: None
- **Blocks**: Ticket 2, 3, 4, 5

### Ticket 2: Extract CleanupStaleTargetOrder
- **File**: `ticket-02-extract-cleanup.md`
- **Priority**: P2
- **Time**: 45 minutes
- **CYC**: 5
- **Tests**: 8 unit tests
- **Dependencies**: Ticket 1
- **Blocks**: Ticket 5

### Ticket 3: Extract IsTargetOrderReplaceable
- **File**: `ticket-03-extract-validation.md`
- **Priority**: P3
- **Time**: 45 minutes
- **CYC**: 6
- **Tests**: 10 unit tests
- **Dependencies**: Ticket 1, 2
- **Blocks**: Ticket 5

### Ticket 4: Extract CreateFollowerTargetReplaceSpec
- **File**: `ticket-04-extract-fsm-spec.md`
- **Priority**: P4
- **Time**: 60 minutes
- **CYC**: 2
- **Tests**: 7 unit tests
- **Dependencies**: Ticket 1, 2, 3
- **Blocks**: Ticket 5
- **CRITICAL**: Preserves FSM two-phase replace pattern

### Ticket 5: Refactor Main Method
- **File**: `ticket-05-refactor-main.md`
- **Priority**: P5 (Final)
- **Time**: 30 minutes
- **CYC**: 3
- **Tests**: 7 integration tests
- **Dependencies**: Ticket 1, 2, 3, 4
- **Blocks**: Epic completion

---

## Complexity Reduction Tracking

| Ticket | Method | CYC Before | CYC After | Reduction |
|--------|--------|------------|-----------|-----------|
| 1 | ShouldSkipTargetReplacement | N/A | 3 | N/A |
| 2 | CleanupStaleTargetOrder | N/A | 5 | N/A |
| 3 | IsTargetOrderReplaceable | N/A | 6 | N/A |
| 4 | CreateFollowerTargetReplaceSpec | N/A | 2 | N/A |
| 5 | SymmetryGuardReplaceExistingFollowerTarget | 18 | 3 | 83% |

**Total Complexity**: CYC 18 → CYC 3 (main) + CYC 16 (helpers) = CYC 19 (net +1, but distributed across 5 testable units)

---

## Risk Mitigation

### Low-Risk Tickets (1, 2, 3, 5)
- Self-contained logic
- Clear boundaries
- No FSM pattern changes

### Medium-Risk Ticket (4)
- **Risk**: FSM two-phase pattern preservation
- **Mitigation**: 
  - Preserve exact FSM dictionary usage
  - Preserve exact REAPER grace stamping
  - Preserve exact cancel logic
  - Integration test with live market replay

---

## Rollback Strategy

### Per-Ticket Rollback
```powershell
git checkout HEAD -- src/V12_002.Symmetry.Replace.cs
powershell -File .\deploy-sync.ps1
```

### Full Epic Rollback
```powershell
git reset --hard HEAD~5
powershell -File .\deploy-sync.ps1
```

---

## Success Criteria

### Per-Ticket Success
- ✅ Helper method extracted with CYC ≤ 8
- ✅ Unit tests pass (100% coverage)
- ✅ `deploy-sync.ps1` executes successfully
- ✅ BUILD_TAG verified in NinjaTrader output
- ✅ No compilation errors

### Epic Success
- ✅ Main method CYC reduced from 18 to 3 (83% reduction)
- ✅ All 4 helpers have CYC ≤ 8
- ✅ Zero logic drift (pure structural movement)
- ✅ FSM two-phase pattern preserved
- ✅ Integration test passes (F5 in NinjaTrader IDE)
- ✅ All unit tests pass (40 tests total)
- ✅ Pre-push validation passes (all 13 checks)

---

## Common Pitfalls

### ❌ Skipping deploy-sync.ps1
**Problem**: Changes don't appear in NinjaTrader  
**Solution**: Run after EVERY src/ modification

### ❌ Breaking FSM Pattern (Ticket 4)
**Problem**: Two-phase replace logic fails  
**Solution**: Preserve exact FSM dictionary, grace stamping, and cancel logic

### ❌ Logic Drift
**Problem**: "Improving" code during extraction  
**Solution**: Pure structural movement only - zero logic changes

### ❌ Incomplete Testing
**Problem**: Edge cases not covered  
**Solution**: Run all 40 unit tests + 7 integration tests

---

## Timeline

- **Day 1**: Tickets 1, 2, 3 (2 hours)
- **Day 2**: Ticket 4 (1 hour)
- **Day 3**: Ticket 5 + Epic completion (1.5 hours)

**Total**: 3.5 hours over 3 days

---

**Execution Status**: READY  
**Next Action**: Execute Ticket 1
