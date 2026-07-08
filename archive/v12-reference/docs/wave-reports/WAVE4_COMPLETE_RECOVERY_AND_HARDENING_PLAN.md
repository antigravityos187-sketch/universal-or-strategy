# Wave 4 Complete Recovery & Protocol Hardening Plan

**Date**: 2026-06-15  
**Objective**: Achieve 80/80 (100%) completion + Remove all PR references from autonomous workflow  
**Current Status**: 69/80 (86.25%)  
**Critical Issues**: 11 incomplete epics + 312 files with PR references

---

## Executive Summary

Wave 4 has two critical issues that must be resolved:

1. **Incomplete Epics**: 11/80 epics not complete (13.75% failure rate)
2. **PR References**: 312 files contain PR references (autonomous workflow should NOT create PRs)

Both issues stem from protocol gaps that allowed:
- Recovery loop to stop at 95.8% instead of 100%
- Phase 3 MCP tool to include "PR Audit" in autonomous workflow
- Phase 4 MCP tool to include "PR Hygiene" requirements

---

## Part 1: Complete 11 Incomplete Epics (80/80 Goal)

### Category A: EPIC-CCN-016 (Deferred - Scope Mismatch)

**Status**: Phase 5 marked as "deferred"  
**Root Cause**: Epic scope didn't match actual code structure  
**Recovery**: Re-run Phases 1-6 with corrected scope

**Action Plan**:
1. Manual Phase 1 (Scope) - verify method exists and matches hotspot
2. Phase 1.5 (Boundary) - ensure single-method extraction
3. Phases 2-6 automated (if Phase 1.5 passes)

**Timeline**: 2 hours

### Category B: Phase 5 Failures (7 epics)

**Epics**: EPIC-CCN-003, 015, 030, 031, 033, 042, 055  
**Status**: No Phase 5 completion files  
**Root Cause**: TBD (need to check logs on VM)

**Action Plan**:
1. SSH to VM and check Phase 5 logs
2. Identify root causes (API exhaustion? Bob command issues? File verification gaps?)
3. Fix root causes
4. Re-run Phase 5 for all 7 epics
5. Run Phase 6 after Phase 5 succeeds

**Timeline**: 1-2 hours (depending on root cause)

### Category C: Phase 6 Failures (3 epics)

**Epics**: EPIC-CCN-012, 027, 045  
**Status**: Phase 5 complete, Phase 6 failed  
**Root Cause**: `bob: command not found` in screen sessions (PATH issue)

**Action Plan**:
1. Fix PATH in Phase 6 scripts (use absolute path to bob)
2. Re-run Phase 6 for 3 epics
3. Monitor until 100% completion

**Timeline**: 30 minutes

---

## Part 2: Remove PR References (312 Files)

### Audit Results

**Total Files**: 312 files with PR references  
**Breakdown by Phase**:
- Phase 0: 2 files
- Phase 1: 35 files
- Phase 2: 54 files
- Phase 3: 64 files (CRITICAL - "DNA & PR Audit")
- Phase 4: 53 files (CRITICAL - "PR Hygiene")
- Phase 5: 27 files
- Phase 6: 67 files

### Root Causes

1. **Phase 3 MCP Tool**: `scripts/phase_3_audit_mcp.py` generates "DNA & PR Audit Report"
2. **Phase 4 MCP Tool**: `scripts/phase_4_tickets_mcp.py` includes "PR Hygiene" section
3. **Building-Blocks Templates**: Copied from manual workflow that includes PR creation

### Fix Strategy

**Option A: Fix MCP Tools + Re-run Phases 3-6 (RECOMMENDED)**
- Update Phase 3 MCP tool: Remove "PR" from all outputs
- Update Phase 4 MCP tool: Remove "PR Hygiene" section
- Re-run Phases 3-6 for all 80 epics
- Timeline: 4-6 hours
- Result: Clean files, proper protocol

**Option B: Accept Current Files + Fix in Wave 5**
- Document PR references as "legacy from manual workflow"
- Fix MCP tools for Wave 5
- Timeline: 30 minutes (documentation only)
- Result: Wave 4 files remain "dirty"

**RECOMMENDATION**: Option A - Fix now to establish clean protocol

---

## Part 3: Protocol Hardening

### Issue 1: Recovery Loop Stopped at 95.8%

**Protocol Violation**: Recovery Loop Protocol V12.26 states "NEVER proceed with <100%"

**Root Cause**: No enforcement mechanism in wave execution scripts

**Fix**:
1. Update `launch_phase6_recovery.py` to loop until 100%
2. Add max_rounds parameter (default 5)
3. Escalate to manual after 5 rounds
4. Update skill: `.bob/skills/gcp-vm-wave-execution/skill.md`

### Issue 2: File Verification Gap

**Protocol Violation**: Phase 5 success determined by exit code, not file existence

**Root Cause**: Scripts check `$?` instead of verifying files on disk

**Fix**:
1. Update all phase scripts to verify files exist
2. Check file size >1KB
3. Fail if files missing (even if exit code 0)
4. Update SOP: `docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md`

### Issue 3: No Cross-Phase Validation

**Protocol Violation**: Phase 6 didn't verify Phase 5 prerequisites before starting

**Root Cause**: Prerequisite checks only verify file patterns, not actual success

**Fix**:
1. Create `validate_prerequisites.py` script
2. Check ALL previous phases complete before starting next
3. Verify file count matches expected (80 files for 80 epics)
4. Update manifest-based workflow to enforce dependencies

### Issue 4: PR References in Autonomous Workflow

**Protocol Violation**: Autonomous workflow should commit only, not create PRs

**Root Cause**: MCP tools copied from manual workflow without adaptation

**Fix**:
1. Update Phase 3 MCP: Remove "PR Audit" → "DNA Audit"
2. Update Phase 4 MCP: Remove "PR Hygiene" section
3. Update building-blocks templates
4. Update SOP to clarify: autonomous = commits only
5. Update mode description: `autonomous-refactor` mode

---

## Execution Plan (Prioritized)

### Priority 1: Fix MCP Tools (30 minutes)

**Phase 3 MCP** (`scripts/phase_3_audit_mcp.py`):
```python
# BEFORE
report_title = "DNA & PR Audit Report"
sections = ["DNA Compliance", "PR Hygiene", "Pre-flight Checks"]

# AFTER
report_title = "DNA Audit Report"
sections = ["DNA Compliance", "Code Quality", "Pre-flight Checks"]
```

**Phase 4 MCP** (`scripts/phase_4_tickets_mcp.py`):
```python
# REMOVE entire PR Hygiene section
# REMOVE "PR diff <10,000" requirement
# REMOVE "PR Hygiene: PASS" from ticket validation
```

### Priority 2: Analyze Phase 5 Failures (30 minutes)

```bash
# SSH to VM
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a

# Check logs for 7 failed epics
cd universal-or-strategy
for epic in EPIC-CCN-003 EPIC-CCN-015 EPIC-CCN-030 EPIC-CCN-031 EPIC-CCN-033 EPIC-CCN-042 EPIC-CCN-055; do
    echo "=== $epic ==="
    tail -50 logs/phase5/$epic.log 2>/dev/null || echo "No log"
done
```

### Priority 3: Fix Phase 6 PATH Issue (15 minutes)

```bash
# Update Phase 6 scripts with absolute path
for num in 012 027 045; do
    sed -i 's|bob --yolo|/home/malhitticrypto/.local/bin/bob --yolo|g' scripts/wave4/_p6_${num}.sh
done

# Upload to VM
gcloud compute scp scripts/wave4/_p6_{012,027,045}.sh v12-test-golden-v2:~/universal-or-strategy/scripts/wave4/ --zone=us-central1-a
```

### Priority 4: Re-run Phase 6 for 3 Epics (30 minutes)

```bash
# Launch recovery
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd universal-or-strategy && screen -dmS p6-012 bash -l -c './scripts/wave4/_p6_012.sh 2>&1 | tee logs/phase6/EPIC-CCN-012-recovery.log'"

# Wait 12s between launches
# Monitor until 100%
```

### Priority 5: Fix Phase 5 Failures (1-2 hours)

Based on log analysis, create targeted recovery scripts.

### Priority 6: Re-scope EPIC-CCN-016 (2 hours)

Manual Phase 1 + automated Phases 1.5-6.

### Priority 7: Re-run Phases 3-6 for All 80 Epics (4-6 hours)

After MCP tools fixed, re-run to get clean files without PR references.

---

## Success Criteria

### Part 1: 80/80 Completion
- ✅ All 80 epics have files for Phases 0-6
- ✅ File count: 80 × 7 = 560 files
- ✅ No gaps in epic sequence (001-080)
- ✅ All verification reports show "PASS"

### Part 2: PR Reference Cleanup
- ✅ Zero PR references in Phase 3 files (was 64)
- ✅ Zero PR references in Phase 4 files (was 53)
- ✅ Phase 3 title: "DNA Audit Report" (not "DNA & PR Audit")
- ✅ Phase 4 tickets: No "PR Hygiene" section

### Part 3: Protocol Hardening
- ✅ Recovery loop enforces 100% completion
- ✅ File verification checks existence + size
- ✅ Cross-phase validation prevents gaps
- ✅ MCP tools updated (Phase 3, Phase 4)
- ✅ Building-blocks templates updated
- ✅ SOP updated with autonomous workflow clarification
- ✅ Skill updated with 100% completion mandate

---

## Timeline Estimate

**Fast Track** (fix current wave only):
- Fix MCP tools: 30 min
- Analyze Phase 5 failures: 30 min
- Fix Phase 6 PATH: 15 min
- Re-run Phase 6 (3 epics): 30 min
- Fix Phase 5 failures: 1-2 hours
- Re-scope EPIC-016: 2 hours
- **TOTAL**: 5-6 hours to 80/80

**Complete Fix** (clean files + protocol):
- Fast Track: 5-6 hours
- Re-run Phases 3-6 (80 epics): 4-6 hours
- Protocol hardening: 1 hour
- **TOTAL**: 10-13 hours to 80/80 + clean files

---

## Recommendation

**Phased Approach**:

1. **Phase A** (Today): Achieve 80/80 completion
   - Fix Phase 6 PATH (3 epics)
   - Analyze + fix Phase 5 failures (7 epics)
   - Re-scope EPIC-016 (1 epic)
   - Timeline: 5-6 hours
   - Result: 80/80 complete

2. **Phase B** (Next Session): Clean up PR references
   - Fix MCP tools (Phase 3, Phase 4)
   - Re-run Phases 3-6 for all 80 epics
   - Timeline: 4-6 hours
   - Result: Clean files

3. **Phase C** (Next Session): Protocol hardening
   - Update recovery loop enforcement
   - Update file verification
   - Update cross-phase validation
   - Update documentation
   - Timeline: 1 hour
   - Result: Hardened protocol for Wave 5

---

## Next Actions

**Immediate** (User Decision Required):
1. Approve phased approach OR request different strategy
2. Confirm priority: 80/80 first, then clean files
3. Authorize VM access for log analysis

**After Approval**:
1. SSH to VM and analyze Phase 5 logs
2. Fix Phase 6 PATH issue
3. Launch Phase 6 recovery (3 epics)
4. Create Phase 5 recovery plan based on log analysis
5. Execute Phase 5 recovery
6. Re-scope EPIC-016
7. Verify 80/80 completion

---

**Status**: 🟡 AWAITING USER APPROVAL  
**Maintainer**: Wave 4 Execution Lead  
**Last Updated**: 2026-06-15T22:16:00Z