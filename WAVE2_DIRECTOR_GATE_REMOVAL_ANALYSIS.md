# Wave 2 Director Gate Removal Analysis

**Date**: 2026-06-13
**Issue**: Director approval gates blocking autonomous Phase 1/1.5/2 execution
**Impact**: 8/9 epics blocked at validation gates instead of completing

---

## Problem Statement

All three planning phases (1, 1.5, 2) have "Director approval gates" that halt execution and wait for manual confirmation:

| Phase | Command | Gate Location | Output Message |
|-------|---------|---------------|----------------|
| 1 | `/epic-intake` | Line 202-221 | "[INTAKE-GATE] Awaiting Director confirmation" |
| 1.5 | `/epic-scope-boundary` | Line 205-216 | "[SCOPE-GATE] Awaiting Director approval" |
| 2 | `/epic-plan` | Line 306-314 | "[PLAN-GATE] Awaiting Director approval" |

### Current Behavior
Bob completes the phase work (analysis, scope validation, architecture planning), writes all artifacts, updates manifest, then **STOPS** and waits for "APPROVED" keyword from Director.

### Impact on Wave 2 Autonomous Execution
- **Phase 2 Results**: 1/9 completed (EPIC-108), 8/9 blocked at gates
- **Wasted Resources**: Each epic consumed bobcoins to reach the gate, then halted
- **Manual Intervention Required**: Director must type "APPROVED" for each epic to proceed

---

## Root Cause Analysis

### Design Intent (Original)
These gates were designed for **interactive human-in-the-loop workflows**:
- Director reviews analysis before committing to approach
- Prevents Bob from proceeding with flawed assumptions
- Allows course correction before expensive implementation

### Autonomous Workflow Reality
In **Wave 2 autonomous execution**:
- No human is monitoring screen sessions in real-time
- Phases are designed as **independent subtasks** with clear inputs/outputs
- Manifest-based architecture already provides checkpointing
- Each phase validates its own work before updating manifest

### The Contradiction
**User's Insight**: "The phases ARE the gates by separation"

The manifest-based architecture already provides:
- ✅ Clear phase boundaries (each phase is a separate session)
- ✅ Dependency validation (Phase N can't start until Phase N-1 completes)
- ✅ Artifact handoff (outputs from Phase N become inputs to Phase N+1)
- ✅ Failure isolation (Phase N failure doesn't corrupt Phase N-1 state)

**Director approval gates are redundant** - they add a manual step to an already-gated workflow.

---

## Proposed Solution

### Option A: Remove Director Gates Entirely (RECOMMENDED)
**Rationale**: Phases are self-validating. If Phase N completes successfully (writes artifacts, updates manifest), it has passed its own quality gate.

**Changes Required**:
1. Remove lines 202-221 from `epic-intake.md`
2. Remove lines 205-216 from `epic-scope-boundary.md`
3. Remove lines 306-314 from `epic-plan.md`

**New Behavior**:
- Phase completes → updates manifest → exits cleanly
- Next phase can start immediately (if dependencies satisfied)
- Director reviews completed artifacts asynchronously (via manifest status)

**Advantages**:
- ✅ Enables true autonomous execution
- ✅ Maintains all safety via manifest validation
- ✅ Director can still review/rollback via manifest
- ✅ Aligns with "phases are gates" philosophy

**Risks**:
- ⚠️ Bob might proceed with flawed analysis (mitigated by Phase 1.5 validation)
- ⚠️ No human checkpoint before expensive Phase 3+ work (mitigated by manifest rollback)

### Option B: Add `--autonomous` Flag
**Rationale**: Keep gates for interactive workflows, bypass for autonomous execution.

**Changes Required**:
1. Add `--autonomous` parameter to each slash command
2. Wrap gate sections in conditional: `if not autonomous_mode:`
3. Update Wave 2 scripts to pass `--autonomous` flag

**Advantages**:
- ✅ Preserves interactive workflow for manual use
- ✅ Enables autonomous execution when needed
- ✅ Explicit opt-in to gate bypass

**Disadvantages**:
- ❌ More complex implementation
- ❌ Two code paths to maintain
- ❌ Flag must be threaded through all scripts

### Option C: Convert Gates to Warnings
**Rationale**: Log the gate message but don't block execution.

**Changes Required**:
1. Change gate sections from "STOP and wait" to "Log and continue"
2. Add manifest field: `director_review_pending: true`
3. Director can review asynchronously via manifest

**Advantages**:
- ✅ Minimal code changes
- ✅ Preserves audit trail
- ✅ Enables autonomous execution

**Disadvantages**:
- ❌ Confusing semantics (gate that doesn't gate?)
- ❌ Manifest pollution with review flags

---

## Recommendation

**Adopt Option A: Remove Director Gates Entirely**

### Justification
1. **Manifest-based architecture already provides gating**:
   - Phase dependencies enforce execution order
   - Artifact validation ensures quality
   - Rollback capability allows course correction

2. **Phases are self-validating**:
   - Phase 1.5 validates Phase 1 scope
   - Phase 2 validates Phase 1.5 boundaries
   - Phase 3 validates Phase 2 architecture

3. **Director review is still possible**:
   - Check manifest status: `python scripts/epic_manifest.py status EPIC-CCN-X`
   - Review artifacts: `docs/brain/EPIC-CCN-X/*.md`
   - Rollback if needed: Update manifest phase status to `pending`, re-run phase

4. **Aligns with autonomous execution goals**:
   - Wave 2 is a **pilot** for autonomous refactoring
   - Manual gates defeat the purpose of automation
   - Trust but verify: automate execution, audit results

### Implementation Plan

**Step 1**: Create backup of current commands
```bash
cp .bob/commands/epic-intake.md .bob/commands/epic-intake.md.backup
cp .bob/commands/epic-scope-boundary.md .bob/commands/epic-scope-boundary.md.backup
cp .bob/commands/epic-plan.md .bob/commands/epic-plan.md.backup
```

**Step 2**: Remove gate sections
- `epic-intake.md`: Delete lines 202-221, replace with simple completion message
- `epic-scope-boundary.md`: Delete lines 205-216, replace with completion message
- `epic-plan.md`: Delete lines 306-314, replace with completion message

**Step 3**: Update completion messages
Replace gate blocks with:
```markdown
---

## PHASE COMPLETE

Phase [N] artifacts written and manifest updated.

**Next Phase**: [Phase N+1 command]
**Review Artifacts**: `docs/brain/$1/[artifact-list]`
**Check Status**: `python scripts/epic_manifest.py status $1`

If you need to revise this phase:
1. Update manifest: `python scripts/epic_manifest.py update $1 [phase] pending`
2. Re-run: `/[command] $1`
```

**Step 4**: Test on single epic
- Manually run `/epic-intake EPIC-TEST-001 "Test epic"`
- Verify it completes without blocking
- Verify manifest updates correctly
- Verify next phase can start

**Step 5**: Deploy to Wave 2
- Regenerate Phase 1/1.5/2 scripts with updated commands
- Deploy to VM
- Launch on remaining 8 blocked epics

---

## Alternative: Quick Fix for Wave 2

If full gate removal is too risky for immediate deployment, **quick workaround**:

### Add Auto-Approval to Scripts
Modify `_p1_*.sh`, `_p1_5_*.sh`, `_p2_*.sh` to automatically send "APPROVED" after gate message:

```bash
# After bob command, check for gate message and auto-approve
bob --yolo /epic-intake EPIC-CCN-107 | tee -a "$LOG" &
BOB_PID=$!

# Monitor for gate message
tail -f "$LOG" | while read line; do
    if echo "$line" | grep -q "GATE.*Awaiting"; then
        echo "APPROVED" | bob --yolo --continue-session
        break
    fi
done

wait $BOB_PID
```

**Pros**: No command file changes needed
**Cons**: Hacky, fragile, doesn't solve root problem

---

## Decision Required

**Director**: Choose implementation approach:
- [ ] **Option A**: Remove gates entirely (RECOMMENDED)
- [ ] **Option B**: Add `--autonomous` flag
- [ ] **Option C**: Convert gates to warnings
- [ ] **Quick Fix**: Auto-approve in scripts (temporary)

**Next Steps After Decision**:
1. Implement chosen solution
2. Test on single epic
3. Deploy to Wave 2
4. Resume blocked epics (8 remaining)

---

## Current Wave 2 Status

**Completed**: 1/9 (EPIC-108)
**Blocked at Gates**: 8/9 (EPIC-107, 109-115)

**Blocked Epics Need**:
- Manual "APPROVED" input (current)
- OR gate removal (recommended)
- OR auto-approval script (quick fix)

**Cost Impact**:
- Each blocked epic consumed ~0.5-1.0 bobcoins to reach gate
- Total wasted: ~4-8 bobcoins (recoverable if we resume from gates)

---

## References

- **Manifest Architecture**: `docs/workflow/V12_EPIC_WORKFLOW_REFACTORING_DESIGN.md`
- **Phase Commands**: `.bob/commands/epic-*.md`
- **Wave 2 Scripts**: `scripts/wave2/_p*.sh`
- **Session Summary**: `WAVE2_SESSION_SUMMARY.md`