# VM + Local Git Workflow Guide

**Version**: 1.0  
**Date**: 2026-06-24  
**Context**: Managing 2k+ pending changes across VM and local environments

---

## The Problem

You have:
- **VM**: Running autonomous refactoring (Wave 7 Phase 1.5 complete)
- **Local**: Working with Bob IDE on other tasks
- **2k+ pending changes** in source control
- **Question**: When to commit/push? How to sync?

---

## Recommended Workflow: GitButler Virtual Branches

### Why GitButler?

**Problem with Regular Git**:
- ❌ Can't work on multiple concerns simultaneously
- ❌ Switching branches loses uncommitted work
- ❌ Merge conflicts when syncing VM ↔ Local
- ❌ Hard to separate VM work from local work

**GitButler Solution**:
- ✅ Multiple virtual branches on same physical branch
- ✅ Each concern isolated (VM work, local work, infrastructure)
- ✅ Commit to virtual branches independently
- ✅ Push virtual branches as separate PRs
- ✅ No branch switching - all work visible

### Setup (One-Time)

```bash
# Install GitButler CLI
curl -fsSL https://gitbutler.com/install.sh | sh

# Initialize in repository
cd /path/to/universal-or-strategy
but init

# Create virtual branches
but branch new "wave7-phase1-5-vm"        # VM work
but branch new "local-feature-work"       # Local work
but branch new "infrastructure-updates"   # Config/docs
```

---

## Workflow: VM Side

### Phase Completion Checkpoints

**Commit AFTER each phase completes** (not during):

```bash
# After Phase 1.5 complete (161/161 epics)
but add docs/brain/EPIC-W7-*
but add .lamport/wave7/
but add building-blocks/wave7/
but add WAVE7_*.md

but commit -m "Wave 7 Phase 1.5: Boundary validation complete (161/161 epics)

- All scope boundaries validated
- Lamport clock synchronized (323 events)
- Jessica API key removed
- Ready for Phase 2"

# Push to GitHub
but push wave7-phase1-5-vm
```

**When to Commit on VM**:
- ✅ After Phase 0 complete (all hotspots analyzed)
- ✅ After Phase 1 complete (all scopes defined)
- ✅ After Phase 1.5 complete (all boundaries validated) ← **YOU ARE HERE**
- ✅ After Phase 2 complete (all architectures planned)
- ✅ After Phase 3 complete (all audits done)
- ✅ After Phase 4 complete (all tickets generated)
- ✅ After Phase 5 complete (all tickets executed) ← **INCLUDES src/ CHANGES**
- ✅ After Phase 6 complete (final review done)

**What to Commit**:
```bash
# Phase 0-4: Planning artifacts only
docs/brain/EPIC-W7-*/
building-blocks/wave7/
.lamport/wave7/
logs/wave7_*/
WAVE7_*.md
epic_roadmap_wave7.json

# Phase 5: Planning + source code
docs/brain/EPIC-W7-*/
src/  # ← ONLY in Phase 5
building-blocks/wave7/
.lamport/wave7/
```

---

## Workflow: Local Side

### Separate Virtual Branch for Local Work

```bash
# On local machine
but branch new "local-feature-xyz"

# Make changes
# ... edit files ...

# Commit to local virtual branch
but add src/MyFeature.cs
but commit -m "Add feature XYZ"

# Push as separate PR
but push local-feature-xyz
```

**Key Point**: Local work goes to **different virtual branch** than VM work.

---

## Syncing VM ↔ Local

### Option 1: GitButler Virtual Branches (Recommended)

**No manual syncing needed!** Each environment works on its own virtual branch.

```bash
# VM: Works on wave7-phase1-5-vm branch
but branch switch wave7-phase1-5-vm
# ... do VM work ...
but commit -m "Phase 1.5 complete"
but push wave7-phase1-5-vm

# Local: Works on local-feature-work branch
but branch switch local-feature-work
# ... do local work ...
but commit -m "Feature XYZ complete"
but push local-feature-work

# Both PRs independent - no conflicts!
```

### Option 2: Manual Sync (If Not Using GitButler)

**After Phase Completion on VM**:

```bash
# 1. On VM: Commit phase work
git add docs/brain/EPIC-W7-*
git add .lamport/wave7/
git add building-blocks/wave7/
git commit -m "Wave 7 Phase 1.5 complete"
git push origin wave7-phase1-5

# 2. On Local: Pull VM changes
git fetch origin
git merge origin/wave7-phase1-5

# 3. On Local: Continue local work
# ... edit files ...
git add src/MyFeature.cs
git commit -m "Local feature XYZ"
git push origin local-feature-xyz
```

**Problem**: Merge conflicts if both touch same files.

---

## Handling 2k+ Pending Changes

### Current Situation Analysis

```bash
# Check what's pending
git status | head -50

# Categorize changes
git status | grep "docs/brain/EPIC-W7-" | wc -l  # Epic artifacts
git status | grep "src/" | wc -l                  # Source code
git status | grep "building-blocks/" | wc -l      # Templates
git status | grep "logs/" | wc -l                 # Logs
```

### Strategy: Batch Commits by Phase

**Don't commit all 2k+ at once!** Break into logical phases:

```bash
# Commit 1: Phase 0 artifacts (if not already committed)
git add docs/brain/EPIC-W7-*/00-hotspots.md
git add docs/brain/EPIC-W7-*/manifest.json
git commit -m "Wave 7 Phase 0: Hotspot analysis (161 epics)"

# Commit 2: Phase 1 artifacts
git add docs/brain/EPIC-W7-*/00-scope.md
git commit -m "Wave 7 Phase 1: Scope definition (161 epics)"

# Commit 3: Phase 1.5 artifacts (current)
git add docs/brain/EPIC-W7-*/01-scope-boundary.md
git add .lamport/wave7/
git add WAVE7_*.md
git commit -m "Wave 7 Phase 1.5: Boundary validation (161 epics)"

# Commit 4: Building-blocks and tools
git add building-blocks/wave7/
git add _p0_*.sh _p1_*.sh _p1_5_*.sh
git commit -m "Wave 7: Execution scripts and templates"

# Commit 5: Logs (optional - usually gitignored)
# git add logs/wave7_*
# git commit -m "Wave 7: Execution logs"
```

---

## Best Practices

### 1. Commit Frequency

**VM (Autonomous Refactoring)**:
- ✅ Commit after each phase completes
- ✅ One commit per phase (not per epic)
- ✅ Include all artifacts for that phase
- ❌ Don't commit during phase execution

**Local (Manual Development)**:
- ✅ Commit after each feature/fix
- ✅ Smaller, more frequent commits
- ✅ Follow conventional commits format

### 2. Branch Strategy

**Use GitButler Virtual Branches**:
```
gitbutler/workspace (physical branch)
├── wave7-phase0-vm (virtual)
├── wave7-phase1-vm (virtual)
├── wave7-phase1-5-vm (virtual) ← Current
├── local-feature-xyz (virtual)
└── infrastructure-updates (virtual)
```

**Alternative: Git Worktrees** (if not using GitButler):
```bash
# Create worktree for VM work
git worktree add ../universal-or-vm wave7-phase1-5

# Create worktree for local work
git worktree add ../universal-or-local local-feature-xyz
```

### 3. What to Commit

**Always Commit**:
- ✅ `docs/brain/EPIC-W7-*/` (epic artifacts)
- ✅ `building-blocks/wave7/` (templates)
- ✅ `.lamport/wave7/` (event log)
- ✅ `WAVE7_*.md` (status reports)
- ✅ `epic_roadmap_wave7.json` (roadmap)
- ✅ `src/` (ONLY in Phase 5+)

**Never Commit**:
- ❌ `logs/` (execution logs - too large)
- ❌ `bin/`, `obj/` (build artifacts)
- ❌ `*.tmp`, `*.bak` (temporary files)
- ❌ `.vscode/` (editor config)
- ❌ `node_modules/` (dependencies)

### 4. Push Frequency

**VM**:
- Push after each phase completion
- Creates checkpoint for disaster recovery
- Allows local to pull latest planning artifacts

**Local**:
- Push after each feature/fix
- More frequent than VM
- Independent of VM work (if using virtual branches)

---

## Recommended Workflow (Step-by-Step)

### Current State: Phase 1.5 Complete on VM

```bash
# === ON VM ===

# 1. Check current status
git status | wc -l
# Result: 2k+ pending changes

# 2. Stage Phase 1.5 artifacts ONLY
git add docs/brain/EPIC-W7-*/01-scope-boundary.md
git add .lamport/wave7/event_log.jsonl
git add WAVE7_PHASE1_5_*.md
git add WAVE7_API_KEY_STATUS.md

# 3. Commit Phase 1.5
git commit -m "Wave 7 Phase 1.5: Boundary validation complete (161/161 epics)

- All scope boundaries validated
- Lamport clock synchronized (323 events)
- Jessica API key removed and replaced
- Building-Blocks Method proven effective
- Ready for Phase 2 (Architecture Planning)

Refs: #WAVE7"

# 4. Push to GitHub
git push origin wave7-phase1-5

# 5. Create PR (optional - can wait until Phase 5)
# gh pr create --title "Wave 7 Phase 1.5: Boundary Validation" --body "..."
```

### After Phase 5 (Source Code Changes)

```bash
# === ON VM (After Phase 5 complete) ===

# 1. Stage source code changes
git add src/

# 2. Stage Phase 5 artifacts
git add docs/brain/EPIC-W7-*/ticket-*-completion.md
git add docs/brain/EPIC-W7-*/ticket-*-verification.md

# 3. Commit Phase 5
git commit -m "Wave 7 Phase 5: Ticket execution complete (161 epics)

- All complexity reductions implemented
- All methods now CYC ≤ 8 (Jane Street standard)
- xUnit tests generated for all extractions
- UTF-8 encoding verified
- Ready for Phase 6 (Final Review)"

# 4. Push and create PR
git push origin wave7-phase5
gh pr create --title "Wave 7 Phase 5: Complexity Reduction" --body "..."
```

---

## Disaster Recovery

### If VM Crashes Before Commit

**Use the backup archive**:
```bash
# On local machine
scp malhitticrypto@VM_IP:/home/malhitticrypto/wave7_phase0_complete_*.tar.gz .
tar -xzf wave7_phase0_complete_*.tar.gz
cd wave7_backup_*/
cp -r docs/brain/EPIC-W7-* /path/to/universal-or-strategy/docs/brain/
# ... integrate other files ...
git add docs/brain/EPIC-W7-*
git commit -m "Wave 7 Phase 1.5: Recovered from VM backup"
```

### If Local and VM Diverge

**Use GitButler to reconcile**:
```bash
# List all virtual branches
but branch list

# Apply VM branch
but branch apply wave7-phase1-5-vm

# Apply local branch
but branch apply local-feature-xyz

# Both changes visible - commit separately
but commit wave7-phase1-5-vm -m "VM work"
but commit local-feature-xyz -m "Local work"
```

---

## Summary: Your Workflow

### Current Recommendation

**Use GitButler Virtual Branches**:

1. **VM**: Work on `wave7-phase1-5-vm` virtual branch
   - Commit after each phase completes
   - Push to GitHub as separate PR
   - No conflicts with local work

2. **Local**: Work on `local-feature-xyz` virtual branch
   - Commit after each feature/fix
   - Push to GitHub as separate PR
   - No conflicts with VM work

3. **Sync**: No manual syncing needed
   - Each branch independent
   - Merge PRs separately on GitHub
   - Pull merged changes to both environments

### Without GitButler (Manual Sync)

1. **VM**: Commit phase work to `wave7-phase1-5` branch
2. **Local**: Pull VM changes, work on `local-feature-xyz` branch
3. **Sync**: Merge VM branch into local branch periodically
4. **Push**: Push both branches, create separate PRs

---

## Next Steps

### Immediate (Phase 1.5 Complete)

```bash
# 1. Commit Phase 1.5 artifacts on VM
git add docs/brain/EPIC-W7-*/01-scope-boundary.md
git add .lamport/wave7/
git add WAVE7_*.md
git commit -m "Wave 7 Phase 1.5 complete (161/161)"
git push origin wave7-phase1-5

# 2. Continue local work independently
# (no need to pull VM changes yet - no src/ changes)

# 3. After Phase 5 complete on VM
# Pull VM changes to local (includes src/ changes)
# Review and merge
```

### Long-Term

1. **Adopt GitButler** for cleaner workflow
2. **Commit after each phase** on VM
3. **Keep local work separate** from VM work
4. **Merge PRs independently** on GitHub
5. **Pull merged changes** to both environments

---

**Workflow Status**: ✅ READY TO COMMIT PHASE 1.5