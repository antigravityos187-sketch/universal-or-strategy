# Phase 1: Scope Definition - EPIC-CCN-115

## Target Method
- **Method**: `SweepTrackedOrders`
- **File**: `src/V12_002.SIMA.Lifecycle.cs`
- **Current Complexity**: 10
- **Target Complexity**: ≤ 15
- **Status**: ✅ ALREADY COMPLIANT (below threshold)

## Extraction Strategy

### What to Extract (IF optimization proceeds)
**Note**: This method is already below the complexity threshold (10 < 15). Extraction is OPTIONAL and LOW PRIORITY.

**Potential Extraction Candidates**:
1. **Order State Validation Logic**
   - Extract: Order state checking and validation rules
   - New Method: `ValidateTrackedOrderState(Order order)`
   - Complexity Reduction: ~3-4 points

2. **Collection Cleanup Logic**
   - Extract: Order removal and collection maintenance
   - New Method: `RemoveStaleOrders(List<Order> staleOrders)`
   - Complexity Reduction: ~2-3 points

3. **Logging and Diagnostics**
   - Extract: Diagnostic logging for sweep operations
   - New Method: `LogSweepDiagnostics(int removedCount, int remainingCount)`
   - Complexity Reduction: ~1-2 points

### What to Keep in Original Method
- Main sweep loop structure
- `_trackedOrders` collection access
- High-level orchestration logic
- Actor/FSM state coordination

## Boundary Definition

### Single Method Scope (V12.23 No Scope Creep Protocol)
- **Target**: `SweepTrackedOrders` ONLY
- **No Scope Creep**: Do NOT refactor adjacent methods
- **No Side Missions**: Do NOT "improve" unrelated code
- **Surgical Precision**: Touch only what is necessary for extraction

### Extraction Boundaries
```
┌─────────────────────────────────────────┐
│ SweepTrackedOrders (Complexity: 10)    │
│                                         │
│ ┌─────────────────────────────────┐   │
│ │ ValidateTrackedOrderState       │   │ ← Extract (Optional)
│ │ (Complexity: ~3)                │   │
│ └─────────────────────────────────┘   │
│                                         │
│ ┌─────────────────────────────────┐   │
│ │ RemoveStaleOrders               │   │ ← Extract (Optional)
│ │ (Complexity: ~2)                │   │
│ └─────────────────────────────────┘   │
│                                         │
│ ┌─────────────────────────────────┐   │
│ │ LogSweepDiagnostics             │   │ ← Extract (Optional)
│ │ (Complexity: ~1)                │   │
│ └─────────────────────────────────┘   │
│                                         │
│ Remaining: Loop + Orchestration (~4)  │
└─────────────────────────────────────────┘
```

## Success Criteria

### Primary Goal
- ✅ **ALREADY MET**: Current complexity (10) ≤ Target (15)

### Optional Optimization Goals (IF extraction proceeds)
1. **Complexity Reduction**
   - Post-extraction complexity: ≤ 5 (stretch goal)
   - Each extracted method: ≤ 5

2. **Code Quality**
   - Zero new `lock()` statements
   - ASCII-only compliance maintained
   - Actor/FSM pattern preserved

3. **Build Verification**
   - `dotnet build` succeeds with zero errors
   - `deploy-sync.ps1` completes successfully
   - NinjaTrader F5 test passes

4. **Test Coverage**
   - Existing tests pass (if any)
   - New unit tests for extracted methods (recommended)

## Risk Assessment

### Risk Level: **VERY LOW**

**Rationale**:
1. Method is already compliant (complexity 10 < 15)
2. Private method with limited blast radius
3. No external callers detected
4. No lock-based concurrency
5. Follows Actor/FSM pattern

### Identified Risks

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Breaking order tracking logic | Low | Medium | Comprehensive unit tests |
| Introducing race conditions | Very Low | High | Maintain Actor/FSM pattern |
| Performance regression | Very Low | Low | Benchmark before/after |
| Scope creep to adjacent methods | Medium | Medium | Strict boundary enforcement |

### Mitigation Strategy
1. **Pre-Extraction**:
   - Run `complexity_audit.py` to establish baseline
   - Review existing tests for coverage gaps
   - Document current behavior

2. **During Extraction**:
   - Use Bob CLI checkpointing (auto-enabled)
   - Extract one method at a time
   - Verify build after each extraction

3. **Post-Extraction**:
   - Run full test suite
   - Execute `deploy-sync.ps1`
   - Verify NinjaTrader integration (F5 test)
   - Run `pre_push_validation.ps1 -Fast`

## Recommendation

### Priority: **LOW (OPTIONAL)**

**Rationale**:
- Current complexity (10) is well below threshold (15)
- Method is production-ready as-is
- No immediate business value from extraction
- Resources better spent on higher-complexity methods

### Suggested Action
1. **Defer to EPIC-CCN-10 Backlog**: Add to future optimization review
2. **Monitor**: Track complexity during future changes
3. **Proceed Only If**: Complexity approaches 12+ in future edits

### If Extraction Proceeds Anyway
**Justification Required**:
- Educational/training exercise for extraction workflow
- Proactive optimization before planned feature additions
- Establishing extraction patterns for team reference

**Estimated Effort**: 2-4 hours (low complexity, minimal risk)

## V12 DNA Compliance Checklist

- ✅ No `lock()` statements in target method
- ✅ ASCII-only compliance verified
- ✅ Actor/FSM pattern followed
- ✅ Complexity below threshold (10 < 15)
- ✅ Private method (limited blast radius)
- ✅ No cross-module dependencies

## Phase 1 Conclusion

**Status**: SCOPE DEFINED (extraction optional)
**Next Phase**: Phase 2 (Planning) - ONLY if extraction proceeds
**Recommendation**: DEFER to backlog (method already compliant)

---

**Generated**: 2026-06-13
**Protocol**: V12.23 (No Scope Creep)
**Threshold**: Jane Street Aligned (CYC ≤ 15)
