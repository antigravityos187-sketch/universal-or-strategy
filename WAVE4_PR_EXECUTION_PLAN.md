# Wave 4 PR Execution Plan

**Date**: 2026-06-16
**Current Branch**: `gitbutler/workspace`
**Wave 4 .cs Commit**: `253305dc` (on gitbutler/workspace)
**Status**: 12 commits ahead of origin/gitbutler/workspace

## Current Situation

### Branch Analysis
- **gitbutler/workspace** (HEAD): 12 commits ahead of origin
  - Contains Wave 4 .cs changes (253305dc)
  - Contains all Wave 4 documentation (dff1d78b, 3f879999, etc.)
  - Contains PR analysis work (3c2723da)
  
- **main**: Behind gitbutler/workspace
  - Last sync: 49a791fb (Merge main into gitbutler/workspace)
  - Missing all Wave 4 work

### Uncommitted Changes
- `.bob/notes/pending-notes.txt` (modified, not staged)

## Execution Strategy

### Phase 1: Merge Non-.cs Files to Main (Direct Push)

**Goal**: Push all documentation, scripts, and non-source files to main without PR

**Steps**:
1. Commit pending changes on gitbutler/workspace
2. Checkout main branch
3. Merge gitbutler/workspace into main (excluding .cs files)
4. Push main to origin

**Files to Merge** (non-.cs only):
- All `docs/brain/EPIC-CCN-*/` Phase 5+6 completion reports (102 files)
- `WAVE4_PR_CLUSTER_ANALYSIS.md`
- `WAVE4_FINAL_STATUS_SUMMARY.md`
- `WAVE4_PHASE6_REMAINING_10_EPICS_PROMPT.md`
- `docs/workflow/PR_REVIEW_CLUSTER_STRATEGY.md`
- `docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md`
- `scripts/analyze_wave4_pr_clusters.py`
- `scripts/generate_phase6_remaining.py`
- All other non-.cs changes

**Command Sequence**:
```bash
# 1. Commit pending changes
git add .bob/notes/pending-notes.txt
git commit -m "chore: update pending notes"

# 2. Push gitbutler/workspace to origin (backup)
git push origin gitbutler/workspace

# 3. Checkout main
git checkout main

# 4. Merge non-.cs files from gitbutler/workspace
git merge gitbutler/workspace --no-commit
git reset HEAD src/*.cs  # Unstage .cs files
git checkout -- src/*.cs  # Restore .cs files from main
git commit -m "docs: Wave 4 documentation and tooling (non-.cs merge)"

# 5. Push main
git push origin main

# 6. Return to gitbutler/workspace
git checkout gitbutler/workspace
```

### Phase 2: Test PR (Check Greptile Quota)

**Goal**: Create smallest PR (PR-7) to test Greptile and tool availability

**PR-7 Details**:
- **Subsystem**: S7 Kernel Infrastructure
- **Files**: 1 (V12_002.Telemetry.cs)
- **Changes**: 52 lines (0 added, 52 deleted)
- **Type**: Pure deletion (dead code removal)
- **Risk**: Trivial

**Steps**:
1. Create branch `pr-7-s7-infrastructure` from 253305dc
2. Cherry-pick only V12_002.Telemetry.cs changes
3. Push branch to origin
4. Create GitHub PR
5. Monitor Greptile, CodeRabbit, Codacy responses
6. Check quota/usage limits

**Command Sequence**:
```bash
# 1. Create branch from Wave 4 .cs commit
git checkout -b pr-7-s7-infrastructure 253305dc

# 2. Reset to only include Telemetry.cs changes
git reset --soft 253305dc~1
git reset HEAD  # Unstage all
git add src/V12_002.Telemetry.cs
git commit -m "refactor(S7): Remove dead Telemetry code (EPIC-CCN-X)

- Delete 52 lines of unused telemetry methods
- Part of Wave 4 complexity reduction (79/80 epics)
- Subsystem: S7 Kernel Infrastructure
- Risk: Trivial (pure deletion)
- CYC Reduced: ~45"

# 3. Push branch
git push origin pr-7-s7-infrastructure

# 4. Create PR via GitHub CLI or web UI
gh pr create --base main --head pr-7-s7-infrastructure \
  --title "refactor(S7): Remove dead Telemetry code" \
  --body "$(cat docs/workflow/PR_REVIEW_CLUSTER_STRATEGY.md | grep -A 50 'PR-7')"
```

### Phase 3: Evaluate & Decide

**Decision Point**: After PR-7 creation, evaluate:

#### Option A: Continue on Current GitHub
**Conditions**:
- ✅ Greptile responds successfully
- ✅ CodeRabbit quota available
- ✅ Codacy analysis runs
- ✅ No rate limit errors

**Action**: Proceed with remaining 6 PRs (PR-1 through PR-6)

#### Option B: Migrate to New GitHub
**Conditions**:
- ❌ Greptile quota exhausted
- ❌ CodeRabbit blocked
- ❌ Tool integration failures
- ❌ Rate limits hit

**Action**: 
1. Create new GitHub account
2. Create new repository
3. Push all work (main + gitbutler/workspace)
4. Reconfigure integrations (Greptile, CodeRabbit, Codacy)
5. Create all 7 PRs on new account

### Phase 4: Create Remaining PRs

**If Option A** (current GitHub):
- Create PR-4 (S4 REAPER, 27 lines) - trivial
- Create PR-6 (S6 Signals, 470 lines) - low
- Create PR-5 (S5 Kernel State, 1,089 lines) - medium
- Create PR-3 (S3 UI/IPC, 702 lines) - medium
- Create PR-2 (S2 Execution, 2,119 lines) - high
- Create PR-1 (S1 SIMA Core, 3,253 lines) - high

**If Option B** (new GitHub):
- Setup new account and repository
- Push all branches
- Create all 7 PRs in recommended order

## File Isolation Strategy

### For Each PR Branch

**Example: PR-1 (S1 SIMA Core)**

```bash
# 1. Create branch from 253305dc
git checkout -b pr-1-s1-sima-core 253305dc

# 2. Reset to isolate only S1 files
git reset --soft 253305dc~1
git reset HEAD  # Unstage all

# 3. Stage only S1 files
git add src/V12_002.SIMA.cs
git add src/V12_002.SIMA.Dispatch.cs
git add src/V12_002.SIMA.Fleet.cs
git add src/V12_002.SIMA.Flatten.cs
git add src/V12_002.SIMA.Lifecycle.cs
git add src/V12_002.SIMA.Shadow.cs

# 4. Commit with subsystem tag
git commit -m "refactor(S1): SIMA Core complexity reduction (Wave 4)

- 6 files, 3,253 lines changed
- Net reduction: -535 lines
- CYC reduced: ~143
- Epics: 12 (EPIC-CCN-X, Y, Z...)
- Subsystem: S1 SIMA Core
- Risk: High (core orchestration logic)"

# 5. Push and create PR
git push origin pr-1-s1-sima-core
gh pr create --base main --head pr-1-s1-sima-core ...
```

## Success Criteria

### Phase 1 (Non-.cs Merge)
- ✅ All documentation on main
- ✅ No .cs files merged
- ✅ Build passes on main
- ✅ No conflicts

### Phase 2 (Test PR)
- ✅ PR-7 created successfully
- ✅ Greptile analysis runs
- ✅ CodeRabbit review appears
- ✅ Codacy reports issues
- ✅ No quota errors

### Phase 3 (Decision)
- ✅ Clear decision: Option A or B
- ✅ If Option B: New GitHub account ready

### Phase 4 (Remaining PRs)
- ✅ All 7 PRs created
- ✅ All PRs have proper subsystem tags
- ✅ All PRs link to Wave 4 documentation
- ✅ All PRs assigned to reviewers

## Risk Mitigation

### Backup Strategy
- ✅ Push gitbutler/workspace to origin before any operations
- ✅ Tag 253305dc as `wave4-cs-changes` for easy reference
- ✅ Keep local copy of all branches

### Rollback Plan
If Phase 1 fails:
```bash
git checkout main
git reset --hard origin/main
git checkout gitbutler/workspace
```

If Phase 2 fails:
```bash
git push origin --delete pr-7-s7-infrastructure
git branch -D pr-7-s7-infrastructure
```

## Timeline Estimate

- **Phase 1** (Non-.cs merge): 15 minutes
- **Phase 2** (Test PR): 20 minutes
- **Phase 3** (Evaluate): 10 minutes
- **Phase 4** (Remaining PRs): 60 minutes (if Option A) or 120 minutes (if Option B)
- **Total**: 105-165 minutes

## Next Immediate Actions

1. ✅ Commit `.bob/notes/pending-notes.txt`
2. ✅ Push gitbutler/workspace to origin (backup)
3. ✅ Tag 253305dc as `wave4-cs-changes`
4. ⏳ Checkout main
5. ⏳ Merge non-.cs files
6. ⏳ Push main
7. ⏳ Create PR-7 (test)
8. ⏳ Evaluate Greptile/tools
9. ⏳ Decide: Option A or B
10. ⏳ Execute Phase 4

---

**Ready to execute Phase 1!**