# Wave 4 Revised Execution Plan - Pilot Testing Approach

**Date**: 2026-06-15T02:08:00Z
**Status**: APPROVED - Pilot testing mandatory for Wave 4
**Authority**: User directive 2026-06-15

---

## Executive Summary

Wave 4 will use a **pilot testing approach**: Test EVERY phase with 1 epic before launching full wave. This ensures Jane Street integration works correctly and prevents 80 false starts.

## Pilot Testing Strategy

### Phase-by-Phase Pilot

**For EACH phase**:
1. Implement Jane Street integration
2. Test with EPIC-CCN-001 (single epic)
3. Verify integration works correctly
4. Document any issues found
5. Fix issues before full wave launch
6. Only proceed to next phase after pilot success

### Why Pilot Testing?

**User Requirement**: "test every phase with 1 epic in wave 4 since its a pilot. maybe its safer to do that at least for wave 4 and we can decide when to remove testing"

**Benefits**:
- ✅ Catch integration issues early (1 epic vs 80 epics)
- ✅ Validate Jane Street KB queries work correctly
- ✅ Test violation detection and fixing
- ✅ Verify file persistence and build success
- ✅ Measure actual bobcoin usage per phase
- ✅ Identify workflow gaps before full wave
- ✅ Build confidence in automation

**Cost**: ~2-3 hours additional testing time vs potential 40+ hours of rework

---

## Revised Timeline

### Phase 0: Hotspot Analysis (WITH Jane Street)

**Implementation** (2 hours):
1. Update Phase 0 script to load violations
2. Add violation count to hotspot report
3. Prioritize methods with high violations

**Pilot Test** (30 min):
1. Test with EPIC-CCN-001
2. Verify violation count appears in report
3. Check file persistence
4. Extract bobcoin usage

**Full Wave** (2 hours):
1. Launch all 80 epics with updated script
2. Monitor execution
3. Verify all files created
4. Extract bobcoin usage

**Total Phase 0**: 4.5 hours

---

### Phase 1: Scope + Boundary (WITH Jane Street)

**Implementation** (2 hours):
1. Update Phase 1 script to query Jane Street KB
2. Add violation validation to scope
3. Include violation fixes in scope definition

**Pilot Test** (30 min):
1. Test with EPIC-CCN-001
2. Verify KB query works
3. Check violations included in scope
4. Validate file persistence

**Full Wave** (3 hours):
1. Launch all 80 epics
2. Monitor execution
3. Verify all files created
4. Extract bobcoin usage

**Total Phase 1**: 5.5 hours

---

### Phase 2: Architecture (WITH Jane Street)

**Implementation** (2 hours):
1. Update Phase 2 script with Firebase hook
2. Add violation fixes to architecture plan
3. Validate plan addresses all violations

**Pilot Test** (30 min):
1. Test with EPIC-CCN-001
2. Verify Firebase hook works
3. Check violations in plan
4. Validate file persistence

**Full Wave** (3 hours):
1. Launch all 80 epics
2. Monitor execution
3. Verify all files created
4. Extract bobcoin usage

**Total Phase 2**: 5.5 hours

---

### Phase 3: Audit (WITH Jane Street)

**Implementation** (2 hours):
1. Update Phase 3 script to validate violations
2. Add Jane Street rule checks
3. Fail audit if violations not addressed

**Pilot Test** (30 min):
1. Test with EPIC-CCN-001
2. Verify violation checks work
3. Check audit report
4. Validate file persistence

**Full Wave** (2 hours):
1. Launch all 80 epics
2. Monitor execution
3. Verify all files created
4. Extract bobcoin usage

**Total Phase 3**: 4.5 hours

---

### Phase 4: Ticket Generation (WITH Jane Street)

**Implementation** (2 hours):
1. Update Phase 4 script to assign violations
2. Include violation fixes in tickets
3. Validate all violations assigned

**Pilot Test** (30 min):
1. Test with EPIC-CCN-001
2. Verify violations in tickets
3. Check ticket structure
4. Validate file persistence

**Full Wave** (2 hours):
1. Launch all 80 epics
2. Monitor execution
3. Verify all files created
4. Extract bobcoin usage

**Total Phase 4**: 4.5 hours

---

### Phase 4.5: Ticket Review (WITH Jane Street)

**Implementation** (1 hour):
1. Integrate existing script into wave execution
2. Test Firebase KB queries
3. Validate approval/rejection logic

**Pilot Test** (30 min):
1. Test with EPIC-CCN-001
2. Verify KB queries work
3. Check review report
4. Validate file persistence

**Full Wave** (2 hours):
1. Launch all 80 epics
2. Monitor execution
3. Verify all files created
4. Extract bobcoin usage

**Total Phase 4.5**: 3.5 hours

---

### Phase 5: Execution (WITH Jane Street)

**Implementation** (2 hours):
1. Verify Bob CLI has Firebase hooks
2. Test violation fixing during refactoring
3. Validate zero violations after changes

**Pilot Test** (1 hour):
1. Test with EPIC-CCN-001 (all tickets)
2. Verify Bob CLI queries KB
3. Check violations fixed
4. Validate build passes

**Full Wave** (Variable - depends on ticket count):
1. Launch tickets for all 80 epics
2. Monitor execution
3. Verify all files created
4. Extract bobcoin usage

**Total Phase 5**: 3+ hours (depends on tickets)

---

### Phase 5.V: Verification (WITH Jane Street)

**Implementation** (2 hours):
1. Update Phase 5.V script to re-check violations
2. Fail verification if violations remain
3. Document violation fixes

**Pilot Test** (30 min):
1. Test with EPIC-CCN-001
2. Verify zero violations after refactoring
3. Check verification report
4. Validate file persistence

**Full Wave** (2 hours):
1. Launch all 80 epics
2. Monitor execution
3. Verify all files created
4. Extract bobcoin usage

**Total Phase 5.V**: 4.5 hours

---

### Phase 6: Final Review (NO Jane Street)

**Implementation** (1 hour):
1. Update Phase 6 script for completion report
2. Aggregate violation fixes
3. Update roadmap

**Pilot Test** (30 min):
1. Test with EPIC-CCN-001
2. Verify completion report
3. Check roadmap update
4. Validate file persistence

**Full Wave** (2 hours):
1. Launch all 80 epics
2. Monitor execution
3. Verify all files created
4. Extract bobcoin usage

**Total Phase 6**: 3.5 hours

---

## Total Timeline

### Implementation + Pilot Testing
- Phase 0: 4.5 hours
- Phase 1: 5.5 hours
- Phase 2: 5.5 hours
- Phase 3: 4.5 hours
- Phase 4: 4.5 hours
- Phase 4.5: 3.5 hours
- Phase 5: 3+ hours
- Phase 5.V: 4.5 hours
- Phase 6: 3.5 hours

**Total**: ~39-40 hours (5 days at 8 hours/day)

### Full Wave Execution
- All phases: ~30-40 hours (parallel execution)

**Grand Total**: ~70-80 hours (9-10 days at 8 hours/day)

---

## Pilot Epic Selection

**Pilot Epic**: EPIC-CCN-001
- **Method**: `SymmetryGuardReplaceExistingFollowerTarget`
- **File**: `V12_002.Symmetry.Replace.cs`
- **CYC**: 18 (Tier 1 - High complexity)
- **LOC**: 49
- **Tier**: T1 (High complexity, larger budget)

**Why EPIC-CCN-001?**
- First in roadmap (natural starting point)
- High complexity (good test case)
- Tier 1 (tests higher budget allocation)
- Representative of typical epic

---

## Success Criteria

### Per Phase Pilot Test

- ✅ Jane Street integration works correctly
- ✅ KB queries return relevant results
- ✅ Violations detected and included in outputs
- ✅ Files persist to disk
- ✅ Build passes (for code-changing phases)
- ✅ Bobcoin usage within budget
- ✅ No errors in logs

### Full Wave Launch

- ✅ All 80 epics complete successfully
- ✅ All violations addressed in touched files
- ✅ Zero violations remain after refactoring
- ✅ Build passes
- ✅ Tests pass
- ✅ Bobcoin budget maintained

---

## Risk Mitigation

### Pilot Testing Catches

1. **Integration Failures**: KB queries fail, violations not loaded
2. **File Persistence Issues**: Files don't persist to disk
3. **Build Failures**: Refactoring breaks compilation
4. **Budget Overruns**: Bobcoin usage exceeds estimates
5. **Workflow Gaps**: Missing steps or dependencies
6. **Bob CLI Issues**: Firebase hooks not working

### Recovery Protocol

**If Pilot Test Fails**:
1. STOP immediately (don't launch full wave)
2. Document failure in `WAVE4_PILOT_FAILURE_ANALYSIS.md`
3. Fix issue in implementation
4. Re-run pilot test
5. Only proceed to full wave after pilot success

---

## Execution Sequence

### Week 1 (Days 1-5): Implementation + Pilot Testing

**Day 1** (8 hours):
- Phase 0 implementation (2h)
- Phase 0 pilot test (0.5h)
- Phase 0 full wave (2h)
- Phase 1 implementation (2h)
- Phase 1 pilot test (0.5h)
- Phase 1 full wave start (1h)

**Day 2** (8 hours):
- Phase 1 full wave complete (2h)
- Phase 2 implementation (2h)
- Phase 2 pilot test (0.5h)
- Phase 2 full wave (3h)
- Phase 3 implementation start (0.5h)

**Day 3** (8 hours):
- Phase 3 implementation complete (1.5h)
- Phase 3 pilot test (0.5h)
- Phase 3 full wave (2h)
- Phase 4 implementation (2h)
- Phase 4 pilot test (0.5h)
- Phase 4 full wave (1.5h)

**Day 4** (8 hours):
- Phase 4 full wave complete (0.5h)
- Phase 4.5 implementation (1h)
- Phase 4.5 pilot test (0.5h)
- Phase 4.5 full wave (2h)
- Phase 5 implementation (2h)
- Phase 5 pilot test (1h)
- Phase 5 full wave start (1h)

**Day 5** (8 hours):
- Phase 5 full wave continue (3h)
- Phase 5.V implementation (2h)
- Phase 5.V pilot test (0.5h)
- Phase 5.V full wave (2h)
- Phase 6 implementation start (0.5h)

### Week 2 (Days 6-10): Completion

**Day 6** (8 hours):
- Phase 6 implementation complete (0.5h)
- Phase 6 pilot test (0.5h)
- Phase 6 full wave (2h)
- Final validation (2h)
- Completion report (2h)
- Lessons learned (1h)

**Days 7-10**: Buffer for issues, re-runs, or early completion

---

## Decision Point: Remove Testing After Wave 4?

**After Wave 4 completion, evaluate**:

1. **Pilot Test Value**: Did pilot tests catch critical issues?
2. **Integration Stability**: Did full waves run smoothly after pilots?
3. **Time Cost**: Was 2-3 hours per phase worth it?
4. **Confidence Level**: Do we trust automation without pilots?

**Options for Wave 5+**:
- **Option A**: Keep pilot testing (safer, slower)
- **Option B**: Remove pilot testing (faster, riskier)
- **Option C**: Pilot test only new phases (hybrid)

**Recommendation**: Decide after Wave 4 data analysis

---

## Next Steps (IMMEDIATE)

1. ✅ **STOP Phase 1 execution** (already stopped - permission errors)
2. ⏳ **Implement Phase 0 Jane Street integration** (2 hours)
3. ⏳ **Pilot test Phase 0 with EPIC-CCN-001** (30 min)
4. ⏳ **Launch Phase 0 full wave** (80 epics, 2 hours)
5. ⏳ **Implement Phase 1 Jane Street integration** (2 hours)
6. ⏳ **Pilot test Phase 1 with EPIC-CCN-001** (30 min)
7. ⏳ **Launch Phase 1 full wave** (80 epics, 3 hours)
8. ⏳ **Continue through Phase 6** (following same pattern)

---

## Conclusion

Wave 4 will use **mandatory pilot testing** for every phase. This adds ~2-3 hours per phase but prevents catastrophic failures with 80 epics. After Wave 4, we'll evaluate whether to continue pilot testing or trust the automation.

**Status**: Ready to begin Phase 0 implementation + pilot testing

---

**Document Version**: 1.0
**Last Updated**: 2026-06-15T02:08:00Z
**Authority**: User directive - pilot testing mandatory for Wave 4
**Next Review**: After Wave 4 completion