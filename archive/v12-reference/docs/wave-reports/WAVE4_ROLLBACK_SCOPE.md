# Wave 4 Rollback Scope - What to Keep vs Discard

**Date**: 2026-06-16T19:53:00Z
**Question**: Do we have to rollback all phases or just some?

## TL;DR: Only Rollback Phase 5 Output (Code Changes)

**Keep**: Phases 0-4 outputs (analysis, plans, tickets)
**Discard**: Phase 5-6 outputs (code changes, verification reports)
**Reason**: The analysis is good, the execution is bad

## Phase-by-Phase Breakdown

### ✅ KEEP: Phase 0 (Hotspot Analysis)

**Location**: `docs/brain/EPIC-CCN-*/00-hotspots.md`
**Status**: GOOD - No issues
**Why Keep**: 
- jCodemunch hotspot analysis is objective
- Identifies high-complexity methods correctly
- No Bob CLI involvement
- Can reuse for retry

**Example**: `docs/brain/EPIC-CCN-001/00-hotspots.md`
```markdown
Method: HydrateFSM_RecoverFromOpenPositions
File: V12_002.SIMA.Lifecycle.cs
Complexity: 42
Churn: High
```

**Action**: KEEP ALL 80 files

---

### ✅ KEEP: Phase 1 (Scope Definition)

**Location**: `docs/brain/EPIC-CCN-*/00-scope.md`
**Status**: GOOD - No issues
**Why Keep**:
- Scope definitions are correct
- Identifies extraction boundaries
- No Bob CLI involvement
- Can reuse for retry

**Example**: `docs/brain/EPIC-CCN-001/00-scope.md`
```markdown
## Extraction Target
Extract FSM recovery logic from HydrateFSM_RecoverFromOpenPositions
Target CYC: ≤8
```

**Action**: KEEP ALL 80 files

---

### ✅ KEEP: Phase 1.5 (Scope Boundary Validation)

**Location**: `docs/brain/EPIC-CCN-*/01-scope-boundary.md`
**Status**: GOOD - No issues
**Why Keep**:
- Boundary validation is correct
- Prevents scope creep
- No Bob CLI involvement
- Can reuse for retry

**Action**: KEEP ALL 80 files

---

### ✅ KEEP: Phase 2 (Architecture Planning)

**Location**: `docs/brain/EPIC-CCN-*/02-architecture-plan.md`
**Status**: GOOD - No issues
**Why Keep**:
- Architecture plans are sound
- Method signatures correct
- Call graphs accurate
- No Bob CLI involvement
- Can reuse for retry

**Example**: `docs/brain/EPIC-CCN-001/02-architecture-plan.md`
```markdown
## Extraction Plan
New method: RecoverPositionFromSnapshot
Signature: private void RecoverPositionFromSnapshot(...)
Location: V12_002.SIMA.Lifecycle.cs
```

**Action**: KEEP ALL 80 files

---

### ✅ KEEP: Phase 3 (DNA & PR Audit)

**Location**: `docs/brain/EPIC-CCN-*/03-audit-report.md`
**Status**: GOOD - No issues
**Why Keep**:
- Audit reports are accurate
- DNA compliance checks correct
- PR hygiene validation sound
- No Bob CLI involvement
- Can reuse for retry

**Action**: KEEP ALL 80 files

---

### ✅ KEEP: Phase 4 (Ticket Generation)

**Location**: `docs/brain/EPIC-CCN-*/04-tickets.md`
**Status**: GOOD - No issues
**Why Keep**:
- Tickets are well-defined
- Extraction instructions clear
- Acceptance criteria correct
- No Bob CLI involvement
- Can reuse for retry

**Example**: `docs/brain/EPIC-CCN-001/04-tickets.md`
```markdown
## Ticket 1: Extract FSM Recovery Logic
Extract RecoverPositionFromSnapshot from HydrateFSM_RecoverFromOpenPositions
Target CYC: ≤8
Preserve: All null guards, error handling
```

**Action**: KEEP ALL 80 files

---

### ❌ DISCARD: Phase 5 (Ticket Execution)

**Location**: `docs/brain/EPIC-CCN-*/ticket-*-completion.md`
**Status**: BAD - Contains buggy code
**Why Discard**:
- Bob CLI introduced behavioral changes
- Removed safety guards
- Changed semantics
- Violated Jane Street principles
- **This is the source of all 28 issues**

**Example**: `docs/brain/EPIC-CCN-001/ticket-1-completion.md`
```markdown
## Changes Made
- Extracted RecoverPositionFromSnapshot
- ❌ Removed null guard (BUG)
- ❌ Introduced LINQ (VIOLATION)
- ❌ Changed DateTime source (BUG)
```

**Action**: DELETE ALL 79 files

---

### ❌ DISCARD: Phase 5.V (Verification)

**Location**: `docs/brain/EPIC-CCN-*/ticket-*-verification.md`
**Status**: BAD - False positives
**Why Discard**:
- Only checked file existence
- Did not catch semantic changes
- Did not catch Jane Street violations
- Gave false confidence

**Example**: `docs/brain/EPIC-CCN-001/ticket-1-verification.md`
```markdown
✅ File exists
✅ Build passes
✅ CYC ≤8
❌ Did NOT check: semantics, guards, LINQ
```

**Action**: DELETE ALL 79 files

---

### ❌ DISCARD: Phase 6 (Final Review)

**Location**: `docs/brain/EPIC-CCN-*/06-completion-report.md`
**Status**: BAD - Based on buggy code
**Why Discard**:
- Reviews buggy Phase 5 output
- Gives false confidence
- No value for retry

**Action**: DELETE ALL 79 files (10 from remaining epics)

---

### ❌ DISCARD: Code Changes in src/

**Location**: `src/V12_002.*.cs` (29 files modified)
**Status**: BAD - Contains 28 critical issues
**Why Discard**:
- Contains all the bugs
- Compilation errors
- Behavioral changes
- Jane Street violations

**Action**: Reset `gitbutler/workspace` to pre-Wave 4 commit

---

## Rollback Procedure (Step-by-Step)

### Step 1: Close PRs (5 minutes)

```bash
# Close all 7 PRs with explanation
gh pr close 16 -c "Rollback: Bob CLI introduced behavioral changes. See WAVE4_FULL_PR_AUDIT.md"
gh pr close 15 -c "Rollback: Bob CLI introduced behavioral changes. See WAVE4_FULL_PR_AUDIT.md"
gh pr close 14 -c "Rollback: Bob CLI introduced behavioral changes. See WAVE4_FULL_PR_AUDIT.md"
gh pr close 13 -c "Rollback: Bob CLI introduced behavioral changes. See WAVE4_FULL_PR_AUDIT.md"
gh pr close 12 -c "Rollback: Bob CLI introduced behavioral changes. See WAVE4_FULL_PR_AUDIT.md"
gh pr close 11 -c "Rollback: Bob CLI introduced behavioral changes. See WAVE4_FULL_PR_AUDIT.md"
gh pr close 10 -c "Rollback: Bob CLI introduced behavioral changes. See WAVE4_FULL_PR_AUDIT.md"
```

### Step 2: Delete Feature Branches (5 minutes)

```bash
# Delete all feature branches
git branch -D wave4-pr1-s1-sima
git branch -D wave4-pr2-s2-execution
git branch -D wave4-pr3-s3-ui-ipc
git branch -D wave4-pr4-s4-reaper
git branch -D wave4-pr5-s5-kernel-state
git branch -D wave4-pr6-s6-signals
git branch -D wave4-pr7-s7-infrastructure

# Delete remote branches
git push origin --delete wave4-pr1-s1-sima
git push origin --delete wave4-pr2-s2-execution
git push origin --delete wave4-pr3-s3-ui-ipc
git push origin --delete wave4-pr4-s4-reaper
git push origin --delete wave4-pr5-s5-kernel-state
git push origin --delete wave4-pr6-s6-signals
git push origin --delete wave4-pr7-s7-infrastructure
```

### Step 3: Reset gitbutler/workspace (5 minutes)

```bash
# Find commit before Wave 4 Phase 5
git log --oneline gitbutler/workspace | grep -B 1 "Wave 4"

# Reset to that commit (example: abc123)
git checkout gitbutler/workspace
git reset --hard abc123

# Verify clean state
git status
# Should show: nothing to commit, working tree clean
```

### Step 4: Delete Phase 5-6 Outputs (10 minutes)

```bash
# Delete Phase 5 completion files
rm docs/brain/EPIC-CCN-*/ticket-*-completion.md

# Delete Phase 5.V verification files
rm docs/brain/EPIC-CCN-*/ticket-*-verification.md

# Delete Phase 6 completion files
rm docs/brain/EPIC-CCN-*/06-completion-report.md

# Verify Phases 0-4 still exist
ls docs/brain/EPIC-CCN-001/
# Should show: 00-hotspots.md, 00-scope.md, 01-scope-boundary.md, 02-architecture-plan.md, 03-audit-report.md, 04-tickets.md
```

### Step 5: Commit Rollback (5 minutes)

```bash
# Commit the rollback
git add docs/brain/
git commit -m "rollback: Wave 4 Phase 5-6 outputs (Bob CLI behavioral changes)

- Close all 7 PRs (28 critical issues found)
- Delete feature branches
- Reset gitbutler/workspace to pre-Phase 5
- Delete Phase 5-6 outputs (ticket completions, verifications, reviews)
- Keep Phases 0-4 outputs (analysis, scope, plans, tickets)

Root cause: Bob CLI over-optimized beyond surgical extraction
Next: Fix Phase 5/5.V protocols, retry with surgical mandate

See: WAVE4_FULL_PR_AUDIT.md, WAVE4_ROLLBACK_VS_FIX_ANALYSIS.md"

git push origin main
```

**Total Rollback Time**: 30 minutes

---

## What We Keep (Reusable)

### Brain Files (Phases 0-4)
```
docs/brain/EPIC-CCN-001/
  ✅ 00-hotspots.md          (Phase 0)
  ✅ 00-scope.md             (Phase 1)
  ✅ 01-scope-boundary.md    (Phase 1.5)
  ✅ 02-architecture-plan.md (Phase 2)
  ✅ 03-audit-report.md      (Phase 3)
  ✅ 04-tickets.md           (Phase 4)
  ❌ ticket-1-completion.md  (Phase 5 - DELETE)
  ❌ ticket-1-verification.md (Phase 5.V - DELETE)
  ❌ 06-completion-report.md (Phase 6 - DELETE)
```

**Total Files Kept**: 480 (6 files × 80 epics)
**Total Files Deleted**: 237 (3 files × 79 epics)

### Bobcoin Investment
- **Phases 0-4**: ~800 bobcoins (KEEP - reusable)
- **Phases 5-6**: ~400 bobcoins (LOST - buggy output)
- **Total Lost**: $4 (400 bobcoins × $0.01)

---

## Retry Strategy (Using Kept Files)

### Phase 0-4: SKIP (Already Done)
- ✅ Use existing hotspot analysis
- ✅ Use existing scope definitions
- ✅ Use existing architecture plans
- ✅ Use existing audit reports
- ✅ Use existing tickets

**Savings**: ~800 bobcoins, ~16 hours VM time

### Phase 5: RE-EXECUTE (With Fixed Protocol)
- ❌ Delete old completion files
- ✅ Execute tickets with new "SURGICAL ONLY" mandate
- ✅ Bob CLI queries Jane Street KB
- ✅ Bob CLI preserves ALL guards

**Cost**: ~400 bobcoins, ~8 hours VM time

### Phase 5.V: RE-EXECUTE (With Fixed Protocol)
- ❌ Delete old verification files
- ✅ Verify with semantic diff review
- ✅ Verify Jane Street compliance
- ✅ Verify no behavioral changes

**Cost**: ~200 bobcoins, ~4 hours VM time

### Phase 6: RE-EXECUTE
- ❌ Delete old completion reports
- ✅ Final review of clean output
- ✅ Generate completion reports

**Cost**: ~200 bobcoins, ~2 hours VM time

**Total Retry Cost**: ~800 bobcoins ($8), ~14 hours VM time

---

## Summary

### What to Rollback
- ❌ Phase 5 outputs (ticket completions)
- ❌ Phase 5.V outputs (verifications)
- ❌ Phase 6 outputs (completion reports)
- ❌ Code changes in src/ (gitbutler/workspace)
- ❌ All 7 PRs and feature branches

### What to Keep
- ✅ Phase 0 outputs (hotspot analysis)
- ✅ Phase 1 outputs (scope definitions)
- ✅ Phase 1.5 outputs (boundary validation)
- ✅ Phase 2 outputs (architecture plans)
- ✅ Phase 3 outputs (audit reports)
- ✅ Phase 4 outputs (tickets)

### Savings from Keeping Phases 0-4
- **Bobcoins**: 800 saved (~$8)
- **VM time**: 16 hours saved
- **Human time**: 4 hours saved (analysis review)

### Total Rollback Effort
- **Time**: 30 minutes
- **Cost**: $0 (just git operations)
- **Risk**: Zero (reversible)

---

**Conclusion**: We only rollback Phase 5-6 outputs (the buggy code). We keep Phases 0-4 (the good analysis). This makes retry much faster and cheaper.

---

**Generated**: 2026-06-16T19:53:00Z
**Author**: Wave 4 Rollback Scope Lead
**Status**: 🟢 READY TO EXECUTE