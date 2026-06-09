# EPIC-CCN-16 Test Plan

**Epic**: EPIC-CCN-16  
**Purpose**: Pilot test for V12 manifest-based epic workflow  
**Date**: 2026-06-09  
**Status**: Ready for Execution  
**Author**: V12 Architecture Team

## Executive Summary

This test plan validates the new manifest-based independent subtask workflow using EPIC-CCN-16 as a pilot. The test covers all 9 phases of the workflow, verifying manifest state management, artifact handoff, and phase independence.

## Test Objectives

### Primary Objectives

1. **Validate Manifest State Management**
   - Verify manifest.json created and updated correctly
   - Verify phase status transitions (pending → in_progress → completed)
   - Verify dependency validation works correctly

2. **Validate Artifact Handoff**
   - Verify each phase reads correct input artifacts from manifest
   - Verify each phase writes output artifacts to standard locations
   - Verify artifact naming convention followed

3. **Validate Phase Independence**
   - Verify each phase can run in separate session
   - Verify phase can resume after failure
   - Verify no context window exhaustion

4. **Validate Parallel Execution**
   - Verify independent tickets can run concurrently
   - Verify manifest updates are serialized correctly
   - Verify no race conditions or conflicts

### Secondary Objectives

1. **Performance Validation**
   - Measure time per phase
   - Compare to old workflow baseline
   - Verify parallel execution speedup

2. **Error Handling Validation**
   - Test failure recovery
   - Test rollback procedures
   - Test manifest corruption recovery

3. **Documentation Validation**
   - Verify walkthrough guide is accurate
   - Verify command syntax is correct
   - Verify troubleshooting guide is helpful

## Test Scope

### In Scope

- ✅ All 9 workflow phases (0, 1, 1.5, 2, 3, 4, 5.X, 5.X.V, 6)
- ✅ Manifest state management
- ✅ Artifact handoff protocol
- ✅ Phase independence
- ✅ Parallel ticket execution
- ✅ Failure recovery
- ✅ Documentation accuracy

### Out of Scope

- ❌ Watsonx Orchestrate integration (Phase 4 of refactoring)
- ❌ Bob CLI orchestrator (separate testing)
- ❌ Performance benchmarking (separate testing)
- ❌ Load testing (separate testing)

## Test Environment

### Prerequisites

- ✅ Git repository: `universal-or-strategy`
- ✅ Branch: `gitbutler/workspace`
- ✅ Python 3.11+
- ✅ PowerShell 7+
- ✅ Bob CLI installed
- ✅ All epic phase commands installed
- ✅ Manifest helper script available

### Test Data

**Target Method**: ProcessOrderUpdate  
**Current CYC**: 28  
**Target CYC**: ≤15  
**File**: `src/V12_002.cs`  
**Expected Tickets**: 3  
**Expected Duration**: 140 minutes

## Test Cases

### Phase 0: Hotspot Analysis

**Test Case ID**: TC-P0-001  
**Objective**: Verify Phase 0 creates manifest and identifies hotspots

**Preconditions**:
- Repository clean (no uncommitted changes)
- `docs/brain/EPIC-CCN-16/` does not exist

**Test Steps**:
1. Execute: `epic-intake EPIC-CCN-16 "Extract ProcessOrderUpdate complexity (CYC 28)"`
2. Wait for command completion
3. Verify manifest created: `docs/brain/EPIC-CCN-16/manifest.json`
4. Verify hotspots report created: `docs/brain/EPIC-CCN-16/00-hotspots.md`
5. Check manifest Phase 0 status: `python scripts/epic_manifest.py phase EPIC-CCN-16 0`

**Expected Results**:
- ✅ Command completes without errors
- ✅ Manifest exists and is valid JSON
- ✅ Phase 0 status = `completed`
- ✅ Hotspots report contains ProcessOrderUpdate with CYC 28
- ✅ Output artifacts listed in manifest

**Acceptance Criteria**:
```json
{
  "phases": {
    "0": {
      "status": "completed",
      "output_artifacts": ["docs/brain/EPIC-CCN-16/00-hotspots.md"]
    }
  }
}
```

**Rollback**: Delete `docs/brain/EPIC-CCN-16/` directory

---

### Phase 1: Scope Definition

**Test Case ID**: TC-P1-001  
**Objective**: Verify Phase 1 reads hotspots and defines scope

**Preconditions**:
- Phase 0 completed successfully
- Manifest Phase 0 status = `completed`

**Test Steps**:
1. Execute: `epic-scope-boundary EPIC-CCN-16 --phase 1`
2. Wait for command completion
3. Verify scope report created: `docs/brain/EPIC-CCN-16/00-scope.md`
4. Check manifest Phase 1 status: `python scripts/epic_manifest.py phase EPIC-CCN-16 1`
5. Verify scope contains: target methods, affected files, extraction plan

**Expected Results**:
- ✅ Command completes without errors
- ✅ Phase 1 status = `completed`
- ✅ Scope report exists
- ✅ Scope limited to 1-3 methods
- ✅ Affected files ≤ 5
- ✅ Extraction plan defined

**Acceptance Criteria**:
```markdown
# 00-scope.md
## Target Methods
- ProcessOrderUpdate (CYC 28)

## Affected Files
- src/V12_002.cs

## Extraction Plan
- Extract 3 helper methods
- Target CYC ≤ 15
```

**Rollback**: `python scripts/epic_manifest.py rollback EPIC-CCN-16 1`

---

### Phase 1.5: Scope Boundary Validation

**Test Case ID**: TC-P1.5-001  
**Objective**: Verify Phase 1.5 validates scope boundaries (V12.23 Protocol)

**Preconditions**:
- Phase 1 completed successfully
- Manifest Phase 1 status = `completed`

**Test Steps**:
1. Execute: `epic-scope-boundary EPIC-CCN-16 --phase 1.5`
2. Wait for command completion
3. Verify boundary report created: `docs/brain/EPIC-CCN-16/01-scope-boundary.md`
4. Check manifest Phase 1.5 status: `python scripts/epic_manifest.py phase EPIC-CCN-16 1.5`
5. Verify validation results: PASS/FAIL

**Expected Results**:
- ✅ Command completes without errors
- ✅ Phase 1.5 status = `completed`
- ✅ Boundary report exists
- ✅ Scope validation = PASS
- ✅ No hidden dependencies
- ✅ No pre-existing compilation errors

**Acceptance Criteria**:
```markdown
# 01-scope-boundary.md
## Scope Validation: PASS
## Hidden Dependencies: None
## Pre-existing Errors: None
## Boundary Violations: None
```

**Rollback**: `python scripts/epic_manifest.py rollback EPIC-CCN-16 1.5`

---

### Phase 2: Architecture Planning

**Test Case ID**: TC-P2-001  
**Objective**: Verify Phase 2 designs extraction architecture

**Preconditions**:
- Phase 1.5 completed successfully
- Manifest Phase 1.5 status = `completed`

**Test Steps**:
1. Execute: `epic-plan EPIC-CCN-16`
2. Wait for command completion
3. Verify architecture plan created: `docs/brain/EPIC-CCN-16/02-architecture-plan.md`
4. Verify diagrams created: `docs/brain/EPIC-CCN-16/02-diagrams.mmd`
5. Check manifest Phase 2 status: `python scripts/epic_manifest.py phase EPIC-CCN-16 2`

**Expected Results**:
- ✅ Command completes without errors
- ✅ Phase 2 status = `completed`
- ✅ Architecture plan exists
- ✅ Helper methods have clear signatures
- ✅ Extraction sequence defined
- ✅ Diagrams show before/after architecture

**Acceptance Criteria**:
```markdown
# 02-architecture-plan.md
## Helper Methods
1. ValidateOrderState(Order order) → bool (CYC -5)
2. CalculateRiskMetrics(Order order) → RiskMetrics (CYC -7)
3. UpdateOrderStatus(Order order, Status status) → void (CYC -6)

## Extraction Sequence
1. Extract ValidateOrderState
2. Extract CalculateRiskMetrics
3. Extract UpdateOrderStatus
```

**Rollback**: `python scripts/epic_manifest.py rollback EPIC-CCN-16 2`

---

### Phase 3: DNA & PR Audit

**Test Case ID**: TC-P3-001  
**Objective**: Verify Phase 3 audits plan against V12 DNA

**Preconditions**:
- Phase 2 completed successfully
- Manifest Phase 2 status = `completed`

**Test Steps**:
1. Execute: `epic-scan EPIC-CCN-16`
2. Wait for command completion
3. Verify audit report created: `docs/brain/EPIC-CCN-16/03-audit-report.md`
4. Check manifest Phase 3 status: `python scripts/epic_manifest.py phase EPIC-CCN-16 3`
5. Verify DNA compliance and PR health score

**Expected Results**:
- ✅ Command completes without errors
- ✅ Phase 3 status = `completed`
- ✅ Audit report exists
- ✅ DNA compliance = PASS
- ✅ PR health score ≥ 80
- ✅ No P0/P1 violations

**Acceptance Criteria**:
```markdown
# 03-audit-report.md
## DNA Compliance: PASS
## PR Health Score: 85/100
## Violations: None
## Recommendations: [List]
```

**Rollback**: `python scripts/epic_manifest.py rollback EPIC-CCN-16 3`

---

### Phase 4: Ticket Generation

**Test Case ID**: TC-P4-001  
**Objective**: Verify Phase 4 generates executable tickets

**Preconditions**:
- Phase 3 completed successfully
- Manifest Phase 3 status = `completed`

**Test Steps**:
1. Execute: `epic-tickets EPIC-CCN-16`
2. Wait for command completion
3. Verify tickets file created: `docs/brain/EPIC-CCN-16/04-tickets.md`
4. Check manifest Phase 4 status: `python scripts/epic_manifest.py phase EPIC-CCN-16 4`
5. Verify ticket count and details

**Expected Results**:
- ✅ Command completes without errors
- ✅ Phase 4 status = `completed`
- ✅ Tickets file exists
- ✅ 3 tickets generated
- ✅ Each ticket has clear method name and signature
- ✅ Tickets are independent (can run in parallel)

**Acceptance Criteria**:
```markdown
# 04-tickets.md
## Ticket 1: Extract ValidateOrderState
- Method: ValidateOrderState
- Signature: private bool ValidateOrderState(Order order)
- CYC Reduction: 5 points
- Dependencies: None

## Ticket 2: Extract CalculateRiskMetrics
[Similar format]

## Ticket 3: Extract UpdateOrderStatus
[Similar format]
```

**Rollback**: `python scripts/epic_manifest.py rollback EPIC-CCN-16 4`

---

### Phase 5.1: Ticket 1 Execution

**Test Case ID**: TC-P5.1-001  
**Objective**: Verify Phase 5.1 executes Ticket 1 successfully

**Preconditions**:
- Phase 4 completed successfully
- Manifest Phase 4 status = `completed`

**Test Steps**:
1. Execute: `epic-validate EPIC-CCN-16 --ticket 1`
2. Wait for command completion
3. Verify completion report created: `docs/brain/EPIC-CCN-16/ticket-1-completion.md`
4. Check manifest Phase 5.1 status: `python scripts/epic_manifest.py phase EPIC-CCN-16 5.1`
5. Verify build passes: `powershell -File .\scripts\build_readiness.ps1`

**Expected Results**:
- ✅ Command completes without errors
- ✅ Phase 5.1 status = `completed`
- ✅ Completion report exists
- ✅ Helper method extracted
- ✅ Calling code updated
- ✅ Build passes (zero errors)
- ✅ CYC reduced by ~5 points

**Acceptance Criteria**:
```markdown
# ticket-1-completion.md
## Extraction Summary
- Helper: ValidateOrderState extracted
- File: src/V12_002.cs modified
- CYC: 28 → 23 (reduction: 5)

## Build Status: PASS
## Test Status: PASS
```

**Rollback**: `python scripts/epic_manifest.py rollback EPIC-CCN-16 5.1`

---

### Phase 5.1.V: Ticket 1 Verification

**Test Case ID**: TC-P5.1.V-001  
**Objective**: Verify Phase 5.1.V validates Ticket 1 execution

**Preconditions**:
- Phase 5.1 completed successfully
- Manifest Phase 5.1 status = `completed`

**Test Steps**:
1. Execute: `epic-verify-ticket EPIC-CCN-16 --ticket 1`
2. Wait for command completion
3. Verify verification report created: `docs/brain/EPIC-CCN-16/ticket-1-verification.md`
4. Check manifest Phase 5.1.V status: `python scripts/epic_manifest.py phase EPIC-CCN-16 5.1.V`

**Expected Results**:
- ✅ Command completes without errors
- ✅ Phase 5.1.V status = `completed`
- ✅ Verification report exists
- ✅ Plan alignment = PASS
- ✅ Build status = PASS
- ✅ Complexity audit = PASS

**Acceptance Criteria**:
```markdown
# ticket-1-verification.md
## Plan Alignment: PASS
## Build Status: PASS
## Test Coverage: 85%
## Complexity Audit: PASS (CYC ≤ 15)
## Issues Found: None
```

**Rollback**: `python scripts/epic_manifest.py rollback EPIC-CCN-16 5.1.V`

---

### Phase 5.2 & 5.3: Parallel Ticket Execution

**Test Case ID**: TC-P5.2-001  
**Objective**: Verify Tickets 2 and 3 can execute in parallel

**Preconditions**:
- Phase 5.1.V completed successfully
- Tickets 2 and 3 are independent (no dependencies)

**Test Steps**:
1. Open two terminals
2. Terminal 1: `epic-validate EPIC-CCN-16 --ticket 2`
3. Terminal 2: `epic-validate EPIC-CCN-16 --ticket 3` (start immediately)
4. Wait for both to complete
5. Verify no conflicts or race conditions
6. Check manifest updates for both phases

**Expected Results**:
- ✅ Both commands complete without errors
- ✅ No manifest conflicts
- ✅ Phase 5.2 status = `completed`
- ✅ Phase 5.3 status = `completed`
- ✅ Both completion reports exist
- ✅ Build passes after both tickets

**Acceptance Criteria**:
- Both tickets complete successfully
- No "manifest locked" errors
- Manifest contains both phase entries
- Total CYC reduction = 18 points (5 + 7 + 6)

**Rollback**: 
```bash
python scripts/epic_manifest.py rollback EPIC-CCN-16 5.2
python scripts/epic_manifest.py rollback EPIC-CCN-16 5.3
```

---

### Phase 6: Final Review

**Test Case ID**: TC-P6-001  
**Objective**: Verify Phase 6 aggregates all verification results

**Preconditions**:
- All ticket verifications completed (5.1.V, 5.2.V, 5.3.V)
- All verification reports exist

**Test Steps**:
1. Execute: `epic-review-final EPIC-CCN-16`
2. Wait for command completion
3. Verify completion report created: `docs/brain/EPIC-CCN-16/05-completion-report.md`
4. Check manifest Phase 6 status: `python scripts/epic_manifest.py phase EPIC-CCN-16 6`
5. Check manifest epic status: `python scripts/epic_manifest.py status EPIC-CCN-16`

**Expected Results**:
- ✅ Command completes without errors
- ✅ Phase 6 status = `completed`
- ✅ Epic status = `completed`
- ✅ Completion report exists
- ✅ All tickets verified successfully
- ✅ Total CYC reduction = 18 points
- ✅ PR health score ≥ 80
- ✅ Deploy readiness = READY

**Acceptance Criteria**:
```markdown
# 05-completion-report.md
## Epic Summary
- Tickets Completed: 3/3
- CYC Reduction: 28 → 10 (18 points)
- Build Status: PASS
- Test Status: PASS
- PR Health Score: 90/100
- Deploy Readiness: READY
```

**Rollback**: `python scripts/epic_manifest.py rollback EPIC-CCN-16 6`

---

## Failure Recovery Tests

### Test Case: Phase Failure Recovery

**Test Case ID**: TC-FR-001  
**Objective**: Verify epic can resume after phase failure

**Test Steps**:
1. Start EPIC-CCN-16 from Phase 0
2. Complete Phases 0-2 successfully
3. Simulate Phase 3 failure (manually set status to `failed`)
4. Verify manifest shows Phase 3 = `failed`
5. Fix issue (reset Phase 3 to `pending`)
6. Retry Phase 3: `epic-scan EPIC-CCN-16`
7. Verify Phase 3 completes successfully
8. Continue with remaining phases

**Expected Results**:
- ✅ Epic resumes from Phase 3 without restarting from Phase 0
- ✅ No data loss from Phases 0-2
- ✅ Phase 3 completes successfully on retry
- ✅ Epic completes successfully

---

### Test Case: Manifest Corruption Recovery

**Test Case ID**: TC-FR-002  
**Objective**: Verify recovery from manifest corruption

**Test Steps**:
1. Complete Phases 0-2
2. Corrupt manifest.json (invalid JSON)
3. Attempt Phase 3: `epic-scan EPIC-CCN-16`
4. Verify error: "Invalid JSON in manifest"
5. Restore manifest from git: `git restore docs/brain/EPIC-CCN-16/manifest.json`
6. Retry Phase 3
7. Verify Phase 3 completes successfully

**Expected Results**:
- ✅ Corruption detected immediately
- ✅ Clear error message provided
- ✅ Manifest restored from git
- ✅ Epic continues successfully

---

## Performance Tests

### Test Case: Phase Duration Measurement

**Test Case ID**: TC-PERF-001  
**Objective**: Measure time per phase

**Test Steps**:
1. Record start time before each phase
2. Record end time after each phase
3. Calculate duration per phase
4. Calculate total epic duration
5. Compare to old workflow baseline (240 minutes)

**Expected Results**:
- ✅ Total duration ≤ 140 minutes (42% faster than old workflow)
- ✅ No phase exceeds 30 minutes
- ✅ Parallel execution reduces total time

**Baseline Comparison**:
| Phase | Old Workflow | New Workflow | Improvement |
|-------|--------------|--------------|-------------|
| 0-4 | 60 min | 55 min | 8% faster |
| 5.X | 90 min | 45 min | 50% faster (parallel) |
| 5.X.V | 30 min | 15 min | 50% faster (parallel) |
| 6 | 20 min | 20 min | Same |
| **Total** | **200 min** | **135 min** | **33% faster** |

---

## Success Criteria

### Must Pass (Blocking)

- ✅ All 9 phases complete successfully
- ✅ Manifest state management works correctly
- ✅ Artifact handoff protocol works correctly
- ✅ Phase independence verified (no context exhaustion)
- ✅ Parallel execution works without conflicts
- ✅ Failure recovery works correctly
- ✅ Build passes after epic completion
- ✅ CYC reduced from 28 to ≤15

### Should Pass (Non-Blocking)

- ✅ Total duration ≤ 140 minutes
- ✅ Documentation accurate and helpful
- ✅ No manual intervention required (except F5 gate)
- ✅ PR health score ≥ 80

## Test Execution Log

### Execution Date: [TBD]

| Phase | Status | Duration | Notes |
|-------|--------|----------|-------|
| 0 | ⏳ Pending | - | - |
| 1 | ⏳ Pending | - | - |
| 1.5 | ⏳ Pending | - | - |
| 2 | ⏳ Pending | - | - |
| 3 | ⏳ Pending | - | - |
| 4 | ⏳ Pending | - | - |
| 5.1 | ⏳ Pending | - | - |
| 5.1.V | ⏳ Pending | - | - |
| 5.2 | ⏳ Pending | - | - |
| 5.2.V | ⏳ Pending | - | - |
| 5.3 | ⏳ Pending | - | - |
| 5.3.V | ⏳ Pending | - | - |
| 6 | ⏳ Pending | - | - |

**Legend**:
- ⏳ Pending
- ✅ Passed
- ❌ Failed
- ⚠️ Passed with warnings

## Issues Found

### Issue Log

| ID | Phase | Severity | Description | Status | Resolution |
|----|-------|----------|-------------|--------|------------|
| - | - | - | - | - | - |

**Severity Levels**:
- **P0**: Blocker (prevents epic completion)
- **P1**: Critical (major functionality broken)
- **P2**: High (workaround available)
- **P3**: Medium (minor issue)
- **P4**: Low (cosmetic)

## Test Sign-Off

### Test Execution

- **Executed By**: [Name]
- **Execution Date**: [Date]
- **Duration**: [Total time]
- **Result**: [PASS/FAIL]

### Approval

- **Reviewed By**: V12 Architecture Team
- **Approval Date**: [Date]
- **Status**: [APPROVED/REJECTED]
- **Comments**: [Comments]

## Next Steps

### If Test Passes

1. Update AGENTS.md with "Pilot Tested" badge
2. Announce new workflow to team
3. Begin gradual migration of in-progress epics
4. Schedule Phase 3 (Orchestration) implementation

### If Test Fails

1. Document all issues in Issue Log
2. Prioritize issues by severity
3. Fix P0/P1 issues immediately
4. Re-run test after fixes
5. Update documentation based on findings

## References

- **Workflow Walkthrough**: `docs/workflow/EPIC_WORKFLOW_WALKTHROUGH.md`
- **Migration Guide**: `docs/workflow/EPIC_WORKFLOW_MIGRATION_GUIDE.md`
- **Architecture Design**: `docs/workflow/V12_EPIC_WORKFLOW_REFACTORING_DESIGN.md`
- **Manifest Schema**: `docs/workflow/EPIC_MANIFEST_SCHEMA.md`

---

**Document Status**: Ready for Execution  
**Last Updated**: 2026-06-09  
**Version**: 1.0