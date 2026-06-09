# EPIC-CCN-13 Protocol Bypass Forensic Report

**Date**: 2026-06-09T02:27:00Z
**Severity**: P1 (Critical Protocol Violation)
**Status**: INVESTIGATION COMPLETE

## Executive Summary

EPIC-CCN-13 was executed on the **wrong branch** (`epic-ccn-13-pr` instead of `gitbutler/workspace`), bypassing the GitButler virtual branch workflow and the after_task auto-commit hook. This resulted in:
1. 5 commits isolated on a feature branch
2. Zero auto-commits to workspace
3. Manual cherry-pick attempt causing merge conflicts
4. Protocol confusion about branch strategy

## Root Cause Analysis

### 1. Branch Selection Error
**What Happened**: The epic was executed on `epic-ccn-13-pr` branch instead of `gitbutler/workspace`.

**Evidence**:
```bash
# Git log shows EPIC-CCN-13 commits are on a separate branch:
* ec713754 [EPIC-CCN-13] ticket-05 (on epic-ccn-13-pr)
* fe6f14d1 [EPIC-CCN-13] ticket-04 (on epic-ccn-13-pr)
...

# Meanwhile, gitbutler/workspace has EPIC-CCN-51 work:
* 60404020 GitButler Workspace Commit (EPIC-CCN-51)
* be8b3993 feat(epic-ccn-51-t4): extract AdoptMasterOrders
```

**Why It Happened**:
- The epic-run orchestrator did NOT verify the current branch before starting
- No pre-flight check in Phase -1 (Index Freshness) to confirm `gitbutler/workspace`
- The `before_new_task.py` hook was never triggered (hook only runs on Bob CLI task start, not epic-run)

### 2. Hook Bypass
**What Happened**: The `after_task.py` hook never executed during EPIC-CCN-13.

**Evidence**:
```python
# after_task.py line 135-139:
success, stdout, _ = run_command("git branch --show-current")
if not success or "gitbutler/workspace" not in stdout:
    print("[after_task] WARNING: Not on gitbutler/workspace branch")
    print("[after_task] Skipping auto-commit for safety")
    return 0
```

**Why It Happened**:
- Hook has a safety check: only auto-commits on `gitbutler/workspace`
- Since we were on `epic-ccn-13-pr`, the hook correctly skipped auto-commit
- BUT: This means the hook detected the wrong-branch condition and silently passed (exit 0)
- No alert was raised to the orchestrator or Director

### 3. GitButler Virtual Branch Not Created
**What Happened**: No GitButler virtual branch was created for EPIC-CCN-13.

**Evidence**:
```python
# before_new_task.py line 103-106:
exit_code, current_branch, _ = run_command(['git', 'branch', '--show-current'])
if exit_code != 0 or 'gitbutler/workspace' not in current_branch:
    print(f"[HOOK] Not in gitbutler/workspace, skipping branch creation", file=sys.stderr)
    sys.exit(0)
```

**Why It Happened**:
- The `before_new_task.py` hook also checks for `gitbutler/workspace`
- Since we started on `epic-ccn-13-pr`, it skipped virtual branch creation
- No virtual branch = no GitButler UI integration = no visual separation of work

## Timeline of Events

1. **Session Start**: Orchestrator mode activated for EPIC-CCN-13
2. **Branch Check SKIPPED**: No verification that we're on `gitbutler/workspace`
3. **Phase 0-4**: Planning completed successfully (branch-agnostic)
4. **Phase 5**: All 5 tickets executed on `epic-ccn-13-pr` branch
5. **F5 Gates**: All passed (F5 verification is branch-agnostic)
6. **Hook Execution**: `after_task.py` ran but skipped auto-commit (safety check)
7. **Phase 6 ABORTED**: Director stopped PR creation (realized wrong branch)
8. **Recovery Attempt**: Tried to cherry-pick to `gitbutler/workspace`
9. **Merge Conflict**: BUILD_TAG conflict between EPIC-CCN-13 and EPIC-CCN-51

## Impact Assessment

### Immediate Impact
- ✅ **Code Quality**: ZERO impact - all extractions are correct (CYC 91→5)
- ✅ **F5 Verification**: ZERO impact - all 5 tickets passed F5 gates
- ❌ **Branch Strategy**: CRITICAL - violated GitButler workflow
- ❌ **Commit History**: CRITICAL - commits isolated on wrong branch

### Downstream Impact
- **EPIC-CCN-14+**: Risk of repeating same error if not fixed
- **GitButler UI**: No visual tracking of EPIC-CCN-13 work
- **Merge Strategy**: Manual cherry-pick required (error-prone)

## Prevention Protocol

### 1. Add Pre-Flight Branch Check (MANDATORY)

**Location**: epic-run command, Phase -1 (Index Freshness)

**Implementation**:
```python
# Add to Phase -1 in epic-run command:
def verify_gitbutler_workspace():
    """Verify we're on gitbutler/workspace before starting epic."""
    result = subprocess.run(
        ['git', 'branch', '--show-current'],
        capture_output=True,
        text=True
    )
    
    if result.returncode != 0:
        raise RuntimeError("Failed to get current branch")
    
    current_branch = result.stdout.strip()
    
    if current_branch != 'gitbutler/workspace':
        raise RuntimeError(
            f"PROTOCOL VIOLATION: Epic must run on gitbutler/workspace, "
            f"currently on '{current_branch}'. "
            f"Run: git checkout gitbutler/workspace"
        )
    
    print(f"[PREFLIGHT] ✓ Branch check passed: {current_branch}")
```

### 2. Harden after_task.py Hook

**Current Behavior**: Silently skips auto-commit if not on workspace
**New Behavior**: FAIL LOUDLY if not on workspace during epic execution

**Implementation**:
```python
# Modify after_task.py line 135-139:
success, stdout, _ = run_command("git branch --show-current")
if not success or "gitbutler/workspace" not in stdout:
    # Check if we're in an epic context (env var set by epic-run)
    if os.environ.get('EPIC_RUN_ACTIVE') == '1':
        print("[after_task] CRITICAL: Epic running on wrong branch!")
        print(f"[after_task] Current: {stdout.strip()}")
        print("[after_task] Expected: gitbutler/workspace")
        return 1  # FAIL instead of silent skip
    else:
        # Non-epic task, safe to skip
        print("[after_task] WARNING: Not on gitbutler/workspace branch")
        print("[after_task] Skipping auto-commit for safety")
        return 0
```

### 3. Add Epic Context Flag

**Purpose**: Let hooks know when they're running inside an epic

**Implementation**:
```python
# In epic-run orchestrator, set environment variable:
os.environ['EPIC_RUN_ACTIVE'] = '1'
os.environ['EPIC_ID'] = epic_id

# Clear on epic completion:
del os.environ['EPIC_RUN_ACTIVE']
del os.environ['EPIC_ID']
```

### 4. Update AGENTS.md Protocol

**Add to Section 2 (Architectural Mandates)**:
```markdown
- **GitButler Workspace Mandate**: ALL epic execution MUST occur on `gitbutler/workspace` branch. 
  The epic-run orchestrator MUST verify branch before Phase 0. Violation is a P0 blocker.
```

## Recovery Plan

### Immediate Actions (This Session)
1. ✅ Abort cherry-pick (DONE)
2. ⏳ Create forensic report (IN PROGRESS)
3. ⏳ Implement pre-flight branch check
4. ⏳ Harden after_task.py hook
5. ⏳ Update AGENTS.md protocol
6. ⏳ Test fixes with mock epic

### Next Session Actions
1. Properly merge EPIC-CCN-13 commits to workspace
2. Verify GitButler UI shows EPIC-CCN-13 work
3. Continue with EPIC-CCN-14 (with fixes in place)

## Lessons Learned

### What Went Right
- ✅ Hooks had safety checks (prevented worse corruption)
- ✅ F5 verification caught no logic errors
- ✅ Director caught the protocol violation before PR submission

### What Went Wrong
- ❌ No pre-flight branch verification in epic-run
- ❌ Hooks failed silently instead of loudly
- ❌ No epic context awareness in hooks
- ❌ No visual confirmation of branch in orchestrator output

### Protocol Improvements
1. **Defense in Depth**: Add checks at multiple layers (orchestrator + hooks)
2. **Fail Loudly**: Critical violations should BLOCK, not WARN
3. **Context Awareness**: Hooks should know if they're in an epic
4. **Visual Feedback**: Orchestrator should display branch name in every gate

## Sign-Off

**Forensic Analyst**: Advanced Mode (Bob CLI)
**Reviewed By**: [Pending Director Review]
**Status**: FIXES READY FOR IMPLEMENTATION

---

**Next Steps**: Implement prevention protocol, then resume EPIC-CCN-13 recovery.