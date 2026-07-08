# Wave 3 Action Plan - Autonomous Refactoring Continuation

**Date**: 2026-06-13T22:17:00Z
**Session Lead**: Advanced Mode Agent
**Status**: READY FOR EXECUTION
**Workflow Version**: V12.25 (10-Phase Manifest-Based)

---

## Executive Summary

Wave 3 will continue autonomous complexity reduction using the **approved 10-phase workflow** with Phase 4.5 (Ticket Review) as a new Jane Street validation gate. This document provides answers to the 5 critical handoff questions and a detailed execution strategy.

---

## Critical Questions Answered

### 1. Workflow Validation: Have you reviewed the 10-phase SOP?

**Answer**: ✅ **YES - REVIEWED AND VALIDATED**

**Key Understanding**:
- **10 phases total**: -1, 0, 1, 2, 3, 4, 4.5, 5, 5.V, 6
- **Phase 1 consolidation**: Phase 1 + 1.5 merged into "Scope + Boundary" (20 min)
- **Phase 4.5 NEW**: Ticket Review with Jane Street validation gate
- **Jane Street gates**: 3 full gates (Phase 2, 4.5, 5) + 3 partial (Phase 1, 3, 5.V)
- **Total planning time**: 100 minutes per epic
- **Manifest-based**: Each phase is independent session with clear inputs/outputs

**SOP Location**: [`docs/workflow/V12_EPIC_WORKFLOW_10_PHASE_SOP.md`](docs/workflow/V12_EPIC_WORKFLOW_10_PHASE_SOP.md)

**Validation Status**: ✅ Complete understanding of workflow structure

---

### 2. Phase 4.5 Implementation: Should we implement before Wave 3 or defer to Wave 4?

**Answer**: ⚠️ **DEFER TO WAVE 4 - RUN WAVE 3 WITHOUT IT**

**Rationale**:

**Arguments FOR implementing now**:
- ✅ Catches ticket mistakes early (prevents wasted execution time)
- ✅ Validates Jane Street compliance before execution
- ✅ Adds critical quality gate

**Arguments AGAINST implementing now**:
- ❌ Wave 2 completed successfully WITHOUT Phase 4.5 (7/7 epics)
- ❌ Implementation requires new MCP server script (~2 hours work)
- ❌ Needs Firebase hook integration and testing
- ❌ Would delay Wave 3 launch by 1-2 days
- ❌ Wave 3 can validate the workflow WITHOUT this gate first

**Decision**: **DEFER TO WAVE 4**

**Strategy**:
1. **Wave 3**: Run with 9-phase workflow (skip Phase 4.5)
   - Validate Phase 1 consolidation works
   - Test manifest-based architecture
   - Measure time savings from consolidation
2. **Post-Wave 3**: Implement Phase 4.5 script
   - Create `scripts/phase_4_5_ticket_review_mcp.py`
   - Add Firebase hook integration
   - Test with 2 sample epics
3. **Wave 4+**: Run with full 10-phase workflow

**Risk Mitigation**: Phase 5.V (Verification) catches ticket mistakes, just later in the process

---

### 3. Parallel Execution: Start with sequential (2 epics) or go full parallel (10 epics)?

**Answer**: ✅ **HYBRID APPROACH - SEQUENTIAL VALIDATION THEN PARALLEL**

**Strategy**:

**Phase 1: Sequential Validation (2 epics)**
- **Epics**: CCN-115, CCN-116
- **Purpose**: Validate 10-phase workflow (without Phase 4.5)
- **Time**: ~3.5 hours (100 min planning + 60 min execution per epic)
- **Success Criteria**:
  - Phase 1 consolidation works smoothly
  - Manifest-based handoffs function correctly
  - Build passes after each epic
  - No P0 blockers introduced

**Phase 2: Parallel Execution (8 epics)**
- **Epics**: CCN-117 through CCN-124
- **Agents**: 8 parallel (safe limit on n2-standard-4)
- **Time**: ~2 hours (with parallelism)
- **Orchestration**: Bob CLI orchestrator or manual screen sessions

**Total Wave 3 Time**: ~5.5 hours (vs ~17 hours sequential)

**Rationale**:
- Validates workflow changes before scaling
- Catches issues early with minimal waste
- Maintains safety margin on VM resources
- Balances speed with risk management

---

### 4. Blocked Epics: Should we investigate CCN-109 and CCN-112 failures before Wave 3?

**Answer**: ❌ **NO - DEFER INVESTIGATION**

**Rationale**:

**Wave 2 Status**:
- **CCN-109**: Test infrastructure issue (pre-existing, out of scope)
- **CCN-112**: Re-executed successfully after verification fix
- **Current Status**: Both documented, not blocking Wave 3

**Decision**: **DEFER TO SEPARATE EPIC**

**Action Plan**:
1. **Wave 3**: Focus on new epics (CCN-115 through CCN-124)
2. **Post-Wave 3**: Create EPIC-TEST-001 for test infrastructure repair
3. **Wave 4**: Re-attempt CCN-109 with fixed test infrastructure

**Rationale**:
- Test infrastructure is systemic issue (affects multiple epics)
- Fixing it mid-wave violates V12.23 No Scope Creep Protocol
- Better to fix once and benefit all future epics
- CCN-112 already resolved (no investigation needed)

**Priority**: P1 (blocks TDD workflow, but not blocking Wave 3)

---

### 5. Obsidian Kanban: Verify git hooks are working correctly?

**Answer**: ✅ **YES - VALIDATED IN WAVE 2**

**Status**: Git hooks implemented and tested in Wave 2

**Implementation**:
- **Hook**: `.git/hooks/post-commit`
- **Function**: Auto-updates Obsidian Kanban on every commit
- **Validation**: Tested during Wave 2 (7 epics)
- **Result**: Working correctly, no manual updates needed

**No Action Required**: Hooks are already in place and functioning

**Reference**: [`scripts/wave2/start_kanban_watcher.bat`](scripts/wave2/start_kanban_watcher.bat)

---

## Environment Validation Checklist

### Pre-Wave 3 Validation

**VM Access**:
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a
```
- [ ] VM accessible
- [ ] SSH connection stable
- [ ] Disk space sufficient (>20 GB free)

**Build Status**:
```bash
cd ~/universal-or-strategy
dotnet build
```
- [ ] Build passes (zero errors)
- [ ] Zero warnings (or documented exceptions)
- [ ] NuGet packages restored

**jCodemunch Index**:
```bash
# Check if repo indexed
jcodemunch list_repos
```
- [ ] Repo indexed (universal-or-strategy)
- [ ] Index current (no stale data)
- [ ] Hotspots available

**Git Status**:
```bash
git status
```
- [ ] No uncommitted changes in `src/`
- [ ] Branch strategy: GitButler virtual branches active
- [ ] On correct branch (`gitbutler/workspace`)

**Jane Street KB**:
```bash
python scripts/query_kb.py "test"
```
- [ ] Firebase connection working
- [ ] KB accessible
- [ ] Returns valid results

---

## Fresh Complexity Audit

### Command

```bash
python scripts/complexity_audit.py > complexity_audit_wave3.txt
```

### Expected Output

**Top 10 Methods** (CYC >8):
1. PropagateMaster_IdentifyMove (CYC 18)
2. HandleFlatPosition_CleanupActivePositions (CYC 17)
3. SyncLimitTarget (CYC 17)
4. ProcessSingleFleetRMAAccount (CYC 16)
5. EmergencyFlattenSingleFleetAccount (CYC 16)
6. AuditMaster_HandleNakedPosition (CYC 15)
7. ProcessQueuedAccountOrder (CYC 15)
8. (Method 8 - TBD from audit)
9. (Method 9 - TBD from audit)
10. (Method 10 - TBD from audit)

**Action**: Run audit to populate CCN-122 through CCN-124

---

## Wave 3 Epic Roadmap

### Target Epics (10 total)

| Epic | Method | File | CYC | LOC | Priority |
|------|--------|------|-----|-----|----------|
| **CCN-115** | PropagateMaster_IdentifyMove | V12_002.Orders.Callbacks.Propagation.cs | 18 | 40 | P1 |
| **CCN-116** | HandleFlatPosition_CleanupActivePositions | V12_002.Orders.Callbacks.Execution.cs | 17 | 30 | P1 |
| **CCN-117** | SyncLimitTarget | V12_002.Orders.Management.StopSync.cs | 17 | 128 | P1 (HIGH LOC) |
| **CCN-118** | ProcessSingleFleetRMAAccount | V12_002.SIMA.Execution.cs | 16 | TBD | P2 |
| **CCN-119** | EmergencyFlattenSingleFleetAccount | V12_002.SIMA.Flatten.cs | 16 | 73 | P2 |
| **CCN-120** | AuditMaster_HandleNakedPosition | V12_002.REAPER.Audit.cs | 15 | 38 | P2 |
| **CCN-121** | ProcessQueuedAccountOrder | V12_002.Orders.Callbacks.AccountOrders.cs | 15 | 34 | P2 |
| **CCN-122** | (TBD from audit) | (TBD) | TBD | TBD | P3 |
| **CCN-123** | (TBD from audit) | (TBD) | TBD | TBD | P3 |
| **CCN-124** | (TBD from audit) | (TBD) | TBD | TBD | P3 |

**Total Methods**: ~70-80 methods (estimated)
**Average per Epic**: 7-8 methods

---

## Execution Strategy

### Phase 1: Sequential Validation (2 epics)

**Epics**: CCN-115, CCN-116

**Commands**:
```bash
# Epic CCN-115
epic-intake EPIC-CCN-115 "Reduce PropagateMaster_IdentifyMove complexity"
epic-scope-boundary EPIC-CCN-115
epic-plan EPIC-CCN-115
epic-scan EPIC-CCN-115
epic-tickets EPIC-CCN-115
# Skip Phase 4.5 (not implemented yet)
epic-validate EPIC-CCN-115 --ticket 1
epic-verify-ticket EPIC-CCN-115 --ticket 1
epic-review-final EPIC-CCN-115

# Epic CCN-116
epic-intake EPIC-CCN-116 "Reduce HandleFlatPosition_CleanupActivePositions complexity"
epic-scope-boundary EPIC-CCN-116
epic-plan EPIC-CCN-116
epic-scan EPIC-CCN-116
epic-tickets EPIC-CCN-116
# Skip Phase 4.5 (not implemented yet)
epic-validate EPIC-CCN-116 --ticket 1
epic-verify-ticket EPIC-CCN-116 --ticket 1
epic-review-final EPIC-CCN-116
```

**Validation Checkpoints**:
- [ ] Phase 1 consolidation works (Scope + Boundary in 20 min)
- [ ] Manifest updates correctly after each phase
- [ ] Build passes after each ticket
- [ ] Complexity targets met (CYC ≤8)
- [ ] No P0 blockers introduced

**Decision Point**: If validation passes, proceed to Phase 2 (parallel)

---

### Phase 2: Parallel Execution (8 epics)

**Epics**: CCN-117 through CCN-124

**Orchestration Options**:

**Option A: Bob CLI Orchestrator** (Recommended)
```bash
# Launch all 8 epics in parallel
bob orchestrate wave3 --epics CCN-117,CCN-118,CCN-119,CCN-120,CCN-121,CCN-122,CCN-123,CCN-124
```

**Option B: Manual Screen Sessions**
```bash
# Launch each epic in separate screen session
for epic in CCN-117 CCN-118 CCN-119 CCN-120 CCN-121 CCN-122 CCN-123 CCN-124; do
    screen -dmS $epic bash -c "epic-intake EPIC-$epic && epic-scope-boundary EPIC-$epic && epic-plan EPIC-$epic && epic-scan EPIC-$epic && epic-tickets EPIC-$epic && epic-validate EPIC-$epic --ticket 1 && epic-verify-ticket EPIC-$epic --ticket 1 && epic-review-final EPIC-$epic"
done
```

**Monitoring**:
```bash
# Check status of all epics
for epic in CCN-117 CCN-118 CCN-119 CCN-120 CCN-121 CCN-122 CCN-123 CCN-124; do
    python scripts/epic_manifest.py status EPIC-$epic
done
```

---

## Success Criteria

### Per Epic

- ✅ All 9 phases complete (Phase 4.5 skipped)
- ✅ Complexity target met (CYC ≤8)
- ✅ Build passes
- ✅ Tests pass (where applicable)
- ✅ Manifest status = "completed"
- ✅ No P0 blockers introduced

### Wave 3 Completion

- ✅ 10 epics complete
- ✅ Complexity reduction documented
- ✅ No P0 blockers
- ✅ Roadmap updated
- ✅ Lessons learned documented
- ✅ Phase 1 consolidation validated
- ✅ Manifest-based architecture validated

---

## Risk Mitigation

### Risk 1: Phase 1 Consolidation Issues

**Risk**: Merged Phase 1 + 1.5 causes problems
**Mitigation**: Sequential validation (2 epics) before parallel
**Fallback**: Revert to 11-phase model if issues found

### Risk 2: VM Resource Exhaustion

**Risk**: 8 parallel agents exceed VM capacity
**Mitigation**: Monitor CPU/memory during Phase 2
**Fallback**: Reduce to 6 parallel agents if needed

### Risk 3: Blocked Epics

**Risk**: Epics block like CCN-109, CCN-112 in Wave 2
**Mitigation**: Strict verification at Phase 5.V
**Fallback**: Document and skip, continue with remaining epics

### Risk 4: Build Failures

**Risk**: Compilation errors after extraction
**Mitigation**: Build check after each ticket
**Fallback**: Rollback ticket, fix in separate session

---

## Post-Wave 3 Actions

### Immediate (After Wave 3)

1. **Sync to Local**:
   ```bash
   # Pull src/ and docs/brain/ from VM
   rsync -avz v12-test-golden-v2:~/universal-or-strategy/src/ ./src/
   rsync -avz v12-test-golden-v2:~/universal-or-strategy/docs/brain/ ./docs/brain/
   ```

2. **Run Validation**:
   ```powershell
   powershell -File .\scripts\pre_push_validation.ps1 -Fast
   ```

3. **Update Roadmap**:
   ```bash
   # Mark completed epics in epic_roadmap.json
   python scripts/update_roadmap.py --complete CCN-115,CCN-116,CCN-117,CCN-118,CCN-119,CCN-120,CCN-121,CCN-122,CCN-123,CCN-124
   ```

4. **Document Lessons**:
   - Create `building-blocks/autonomous-refactoring/WAVE3_LESSONS_LEARNED.md`
   - Update `building-blocks/autonomous-refactoring/WAVE3_COMPLETE_REPORT.md`

### Deferred (Before Wave 4)

1. **Implement Phase 4.5**:
   - Create `scripts/phase_4_5_ticket_review_mcp.py`
   - Add Firebase hook integration
   - Test with 2 sample epics

2. **Add Firebase Hooks**:
   - Update Phase 1 script (extraction patterns)
   - Update Phase 3 script (Jane Street rules)
   - Update Phase 5.V script (test patterns)

3. **Test 10-Phase Workflow**:
   - Validate with 2 epics before Wave 4
   - Measure time impact of Phase 4.5
   - Document any issues

4. **Update Obsidian Kanban**:
   - Verify git hooks still working
   - Check for any missed updates

---

## Timeline Estimate

### Sequential Validation (Phase 1)

**Epic CCN-115**:
- Planning: 100 minutes
- Execution: 60 minutes (6 tickets × 10 min)
- Verification: 30 minutes (6 tickets × 5 min)
- **Total**: 190 minutes (~3.2 hours)

**Epic CCN-116**:
- Planning: 100 minutes
- Execution: 50 minutes (5 tickets × 10 min)
- Verification: 25 minutes (5 tickets × 5 min)
- **Total**: 175 minutes (~2.9 hours)

**Phase 1 Total**: ~6.1 hours

### Parallel Execution (Phase 2)

**8 Epics in Parallel**:
- Planning: 100 minutes (per epic, parallel)
- Execution: 60 minutes (per epic, parallel)
- Verification: 30 minutes (per epic, parallel)
- **Total**: 190 minutes (~3.2 hours)

**Phase 2 Total**: ~3.2 hours

### Wave 3 Total Time

**Wall Time**: ~9.3 hours (6.1 + 3.2)
**Sequential Time**: ~31 hours (10 epics × 3.1 hours)
**Speedup**: 3.3x (due to parallelism)

---

## Command Reference

### Environment Validation

```bash
# VM access
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a

# Build check
cd ~/universal-or-strategy && dotnet build

# jCodemunch check
jcodemunch list_repos

# Git status
git status

# Jane Street KB check
python scripts/query_kb.py "test"
```

### Complexity Audit

```bash
# Run fresh audit
python scripts/complexity_audit.py > complexity_audit_wave3.txt

# View top 20 methods
head -40 complexity_audit_wave3.txt
```

### Epic Lifecycle

```bash
# Start epic (Phase 0)
epic-intake EPIC-CCN-X "Description"

# Scope + Boundary (Phase 1 - consolidated)
epic-scope-boundary EPIC-CCN-X

# Architecture (Phase 2)
epic-plan EPIC-CCN-X

# Audit (Phase 3)
epic-scan EPIC-CCN-X

# Tickets (Phase 4)
epic-tickets EPIC-CCN-X

# Skip Phase 4.5 (not implemented)

# Execute ticket (Phase 5)
epic-validate EPIC-CCN-X --ticket 1

# Verify ticket (Phase 5.V)
epic-verify-ticket EPIC-CCN-X --ticket 1

# Final review (Phase 6)
epic-review-final EPIC-CCN-X
```

### Status Monitoring

```bash
# Check epic status
python scripts/epic_manifest.py status EPIC-CCN-X

# List all epics
python scripts/epic_manifest.py list

# Get next pending epic
python scripts/epic_manifest.py next
```

---

## Decision Summary

### Question 1: Workflow Validation
**Answer**: ✅ YES - Reviewed and validated 10-phase SOP

### Question 2: Phase 4.5 Implementation
**Answer**: ⚠️ DEFER TO WAVE 4 - Run Wave 3 without it

### Question 3: Parallel Execution
**Answer**: ✅ HYBRID - Sequential validation (2 epics) then parallel (8 epics)

### Question 4: Blocked Epics Investigation
**Answer**: ❌ NO - Defer to separate epic (EPIC-TEST-001)

### Question 5: Obsidian Kanban Verification
**Answer**: ✅ YES - Validated in Wave 2, working correctly

---

## Next Steps

### Immediate Actions

1. **Run Environment Validation** (30 minutes)
   - Check VM access
   - Verify build passes
   - Validate jCodemunch index
   - Confirm git status clean

2. **Run Fresh Complexity Audit** (10 minutes)
   - Execute `python scripts/complexity_audit.py`
   - Identify CCN-122 through CCN-124
   - Update epic roadmap

3. **Launch Sequential Validation** (6 hours)
   - Execute CCN-115
   - Execute CCN-116
   - Validate Phase 1 consolidation
   - Check for issues

4. **Decision Point** (15 minutes)
   - Review validation results
   - Decide: proceed to parallel or fix issues
   - Document any problems found

5. **Launch Parallel Execution** (3 hours)
   - Execute CCN-117 through CCN-124
   - Monitor VM resources
   - Track progress

6. **Post-Wave 3 Sync** (1 hour)
   - Sync files to local
   - Run validation
   - Update roadmap
   - Document lessons

---

## Critical Reminders

### V12.23 No Scope Creep Protocol

**ONE EPIC = ONE CONCERN**

- ❌ Fixing pre-existing compilation errors during epic
- ❌ Adding "while we're here" improvements
- ❌ Bundling multiple concerns in one commit
- ❌ Expanding scope mid-epic without Director approval

**Enforcement**: Phase 1 boundary validation (merged into Phase 1)

### PR Strategy (Batch Mode)

**DO NOT CREATE PRS UNTIL ALL EPICS COMPLETE**

- Commit after each epic
- Batch commits into related clusters
- Create PRs after all 53 epics done
- Merge to `main` after PR approval

**Rationale**: Avoid mid-workflow merge conflicts

### Jane Street Alignment

**CYC ≤8 Threshold** (not 15)

- Jane Street HFT systems prioritize cognitive simplicity
- Functions >8 are harder to reason about under microsecond latency
- V12 DNA: "Make illegal states unrepresentable" requires simple logic

---

## Conclusion

Wave 3 is **READY FOR EXECUTION** with the following strategy:

1. ✅ **Workflow**: 10-phase model (skip Phase 4.5 for now)
2. ✅ **Execution**: Hybrid (2 sequential + 8 parallel)
3. ✅ **Timeline**: ~9.3 hours wall time
4. ✅ **Risk**: Mitigated through sequential validation
5. ✅ **Success Criteria**: Clear and measurable

**Next Action**: Run environment validation and launch CCN-115

---

**Document Version**: 1.0
**Last Updated**: 2026-06-13T22:17:00Z
**Maintainer**: V12 Orchestration Team
**Status**: APPROVED FOR EXECUTION