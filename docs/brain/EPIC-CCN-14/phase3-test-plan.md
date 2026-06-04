# EPIC-CCN-14 Phase 3 Test Plan

**Epic**: EPIC-CCN-14 (ShouldSkipFleet_RunHealthCheck extraction)  
**Test Date**: TBD  
**Test Engineer**: TBD  
**Purpose**: Validate Phase 3 performance integration (benchmarks + complexity thresholds)

---

## Test Objectives

1. Verify benchmark infrastructure runs successfully in pr-loop
2. Verify complexity audit enforces CYC ≤ 10 threshold
3. Verify all gates pass (hygiene, validation, benchmarks)
4. Validate EPIC-CCN-14 extraction completes without regressions

---

## Prerequisites

- [ ] Phase 3 infrastructure complete (benchmarks script, pr-loop integration, complexity updates)
- [ ] Benchmarks compile successfully (fix Mock type issues first)
- [ ] Baseline established in `docs/benchmarks/baseline.json`
- [ ] EPIC-CCN-14 scope defined (ShouldSkipFleet_RunHealthCheck method)

---

## Test Procedure

### Step 1: Pre-Test Validation

```powershell
# Verify benchmark script exists
Test-Path .\scripts\run_benchmarks.ps1

# Verify baseline exists
Test-Path .\docs\benchmarks\baseline.json

# Verify complexity threshold updated
python scripts/complexity_audit.py --help | Select-String "default: 10"

# Verify pr-loop updated
Select-String "Step 2.5" .bob\commands\pr-loop.md
```

**Expected**: All checks pass

---

### Step 2: Run EPIC-CCN-14

```powershell
# Start epic run
/epic-run EPIC-CCN-14
```

**Monitor**:
- Phase 1: Forensic intake completes
- Phase 2: Architecture planning completes
- Phase 3: DNA audit passes
- Phase 4: Extraction executes
- Phase 5: Verification passes

---

### Step 3: Verify Benchmark Integration

**During pr-loop Step 2.5**:

```powershell
# Benchmark script should run automatically
# Expected output:
# [BENCHMARK] V12 Performance Benchmark Runner
# [BENCHMARK] Step 1: Building benchmarks project...
# [BENCHMARK] Step 2: Running benchmarks...
# [BENCHMARK] Step 3: Parsing results...
# [BENCHMARK] Step 4: Extracting metrics...
# [BENCHMARK] Step 5: Saving latest run...
# [BENCHMARK] Step 6: Comparing against baseline...
# [BENCHMARK-PASS] All benchmarks within 5% of baseline
```

**Success Criteria**:
- [ ] Benchmarks run without errors
- [ ] No regressions detected (>5%)
- [ ] Zero-allocation mandate maintained
- [ ] Latest run saved to `docs/benchmarks/latest_run.json`

**Failure Modes**:
- Build errors → Fix benchmark Mock dependencies
- Regressions detected → Investigate performance impact
- Allocation violations → Critical - halt and fix

---

### Step 4: Verify Complexity Enforcement

**During pr-loop Step 2 (Local Repair)**:

```powershell
# Complexity audit should run as part of pre_push_validation.ps1
# Expected output:
# [PASS] All X methods are within CYC 10 threshold
```

**Success Criteria**:
- [ ] Complexity audit runs with threshold 10
- [ ] No methods exceed CYC 10
- [ ] Extracted method has CYC ≤ 10

**Failure Modes**:
- Violations detected → Refactor extracted method
- Threshold not applied → Check script updates

---

### Step 5: Full Gate Validation

**All gates must pass**:

1. **Hygiene Gate** (Step 0):
   - [ ] Branch rebased on main
   - [ ] Diff < 10k characters
   - [ ] No dirty files

2. **Local Validation** (Step 2):
   - [ ] ASCII-only check: PASS
   - [ ] Build: PASS
   - [ ] Unit tests: PASS
   - [ ] Lint: PASS
   - [ ] Formatting: PASS
   - [ ] Security: PASS (warnings OK)
   - [ ] Markdown links: PASS (warnings OK)
   - [ ] PR hygiene: PASS
   - [ ] **Complexity: PASS (NEW)**
   - [ ] Dead code: PASS (warnings OK)
   - [ ] Codacy preview: PASS (warnings OK)
   - [ ] Semgrep: PASS (warnings OK)
   - [ ] CodeRabbit: PASS (warnings OK)

3. **Benchmark Gate** (Step 2.5 - NEW):
   - [ ] Benchmarks run: PASS
   - [ ] No regressions: PASS
   - [ ] Zero allocations: PASS

4. **Global Validation** (Step 3):
   - [ ] Hard-link sync: PASS
   - [ ] Push successful: PASS
   - [ ] Bot checks: PASS
   - [ ] PHS: 100/100

---

## Test Results Template

```markdown
# EPIC-CCN-14 Phase 3 Test Results

**Test Date**: YYYY-MM-DD  
**Test Engineer**: [Name]  
**Duration**: [X hours]

## Summary

- [ ] EPIC-CCN-14 extraction: SUCCESS/FAIL
- [ ] Benchmark integration: SUCCESS/FAIL
- [ ] Complexity enforcement: SUCCESS/FAIL
- [ ] All gates passed: SUCCESS/FAIL

## Detailed Results

### Benchmark Integration

**Status**: SUCCESS/FAIL

**Output**:
```
[Paste benchmark output here]
```

**Metrics**:
- Benchmarks run: X
- Regressions detected: X
- Allocations: X bytes
- Baseline comparison: PASS/FAIL

**Issues**:
- [List any issues encountered]

### Complexity Enforcement

**Status**: SUCCESS/FAIL

**Output**:
```
[Paste complexity audit output here]
```

**Metrics**:
- Methods audited: X
- Violations (CYC > 10): X
- Extracted method CYC: X

**Issues**:
- [List any issues encountered]

### Gate Results

| Gate | Status | Notes |
|------|--------|-------|
| Hygiene | PASS/FAIL | |
| Build | PASS/FAIL | |
| Tests | PASS/FAIL | |
| Lint | PASS/FAIL | |
| Formatting | PASS/FAIL | |
| Complexity | PASS/FAIL | NEW |
| Benchmarks | PASS/FAIL | NEW |
| PHS | X/100 | |

### EPIC-CCN-14 Extraction

**Method Extracted**: ShouldSkipFleet_RunHealthCheck  
**Original CYC**: X  
**Extracted CYC**: X  
**Target CYC**: ≤ 10

**Files Modified**:
- [List files]

**Commits**:
- [List commit hashes]

## Issues Encountered

1. **[Issue Title]**
   - **Severity**: P0/P1/P2
   - **Description**: [Details]
   - **Resolution**: [How it was fixed]

## Recommendations

1. [Recommendation 1]
2. [Recommendation 2]

## Conclusion

**Overall Status**: SUCCESS/FAIL

**Phase 3 Integration**: VALIDATED/NEEDS_WORK

**Next Steps**:
- [List next steps]

---

**Signed Off By**: [Name]  
**Date**: YYYY-MM-DD
```

---

## Rollback Plan

If test fails:

1. **Benchmark failures**:
   - Revert `scripts/run_benchmarks.ps1`
   - Revert `.bob/commands/pr-loop.md` (remove Step 2.5)
   - Document issue in `docs/brain/EPIC-CCN-14/benchmark-failure.md`

2. **Complexity failures**:
   - Revert `scripts/complexity_audit.py`
   - Revert `.codacy.yml`
   - Restore threshold to 15

3. **EPIC-CCN-14 failures**:
   - Follow standard epic rollback protocol
   - Document in `docs/brain/EPIC-CCN-14/failure-analysis.md`

---

## Success Criteria Summary

- [x] Benchmark infrastructure created
- [x] pr-loop integration complete
- [x] Complexity thresholds updated
- [ ] EPIC-CCN-14 test executed
- [ ] All gates passed
- [ ] No regressions detected
- [ ] Documentation complete

---

**Status**: READY FOR EXECUTION  
**Estimated Duration**: 2 hours  
**Risk Level**: LOW (infrastructure changes only)

---

_Created: 2026-06-03 (Phase 3)_  
_Made with Bob_