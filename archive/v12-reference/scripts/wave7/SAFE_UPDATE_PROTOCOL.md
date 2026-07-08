# Safe Update Protocol - NEVER Delete Work Again

## The Problem

**What Happened**: `git clean -fd` deleted 143 completed epics because they weren't committed.

**Your Concern**: "Will this happen every time we update a script?"

**Answer**: NO - if we follow this protocol.

## The Solution: Three-Layer Protection

### Layer 1: Mandatory Commits (EVERY 20 EPICS)

**New Rule**: Commit progress automatically, not manually.

```bash
# Add to monitoring script - runs every 4 minutes
COMPLETED=$(find docs/brain/EPIC-W7-*/00-hotspots.md 2>/dev/null | wc -l)
LAST_COMMIT=$(cat .last_commit_count 2>/dev/null || echo 0)

# Auto-commit every 20 epics
if [ $((COMPLETED - LAST_COMMIT)) -ge 20 ]; then
    git add docs/brain/EPIC-W7-*
    git commit -m "auto-checkpoint: $COMPLETED epics complete"
    git push origin main
    echo $COMPLETED > .last_commit_count
fi
```

**Result**: Work is saved to GitHub every 20 epics automatically.

### Layer 2: NEVER Use git clean

**BANNED FOREVER**: `git clean -fd`

**Safe Alternatives for Merge Conflicts**:

#### Option A: Stash (Preserves Everything)
```bash
# When pulling updates:
git stash                    # Save uncommitted work
git pull origin main         # Pull updates
git stash pop                # Restore work
# Resolve conflicts if any
```

#### Option B: Commit First (Safest)
```bash
# Before pulling updates:
git add docs/brain/EPIC-W7-*
git commit -m "WIP: checkpoint before pull"
git pull origin main
# Resolve conflicts if any
```

#### Option C: Backup First (Paranoid Mode)
```bash
# Before ANY git operation:
rsync -av docs/brain/EPIC-W7-* /tmp/epic_backup/
# Then do git operations
# Restore if needed: rsync -av /tmp/epic_backup/* docs/brain/
```

### Layer 3: Automated Backup

**New Script**: `scripts/wave7/auto_backup.sh`

```bash
#!/bin/bash
# Runs every 10 minutes via cron

BACKUP_DIR="/tmp/wave7_backup_$(date +%Y%m%d_%H%M%S)"
mkdir -p "$BACKUP_DIR"

# Backup all epic directories
if [ -d "docs/brain/EPIC-W7-001" ]; then
    rsync -av docs/brain/EPIC-W7-* "$BACKUP_DIR/"
    echo "Backup created: $BACKUP_DIR"
    
    # Keep only last 6 backups (1 hour of history)
    ls -dt /tmp/wave7_backup_* | tail -n +7 | xargs rm -rf
fi
```

**Setup**:
```bash
# Add to crontab on VM:
*/10 * * * * cd /home/malhitticrypto/universal-or-strategy && ./scripts/wave7/auto_backup.sh
```

## The New Workflow

### When Updating Scripts (Safe Process)

```bash
# Step 1: Check current status
COMPLETED=$(find docs/brain/EPIC-W7-*/00-hotspots.md 2>/dev/null | wc -l)
echo "Current: $COMPLETED/161 epics"

# Step 2: Commit current work (if any)
if [ $COMPLETED -gt 0 ]; then
    git add docs/brain/EPIC-W7-*
    git commit -m "checkpoint: $COMPLETED epics before script update"
    git push origin main
fi

# Step 3: Pull updates safely
git stash                    # Save any uncommitted changes
git pull origin main         # Pull updates
git stash pop                # Restore changes (if any)

# Step 4: Verify nothing was lost
AFTER=$(find docs/brain/EPIC-W7-*/00-hotspots.md 2>/dev/null | wc -l)
if [ $AFTER -ne $COMPLETED ]; then
    echo "ERROR: Lost epics! Before: $COMPLETED, After: $AFTER"
    exit 1
fi

echo "✓ Safe update complete. Still have $AFTER epics."
```

### When Launching New Wave

```bash
# Step 1: Commit infrastructure
git add scripts/wave7/
git commit -m "feat(wave7): Add launch infrastructure"
git push origin main

# Step 2: Launch with auto-commit enabled
./scripts/launch_wave7_vm.sh --auto-commit

# Step 3: Monitor with auto-backup
./scripts/wave7/auto_backup.sh &  # Background backup every 10 min
./scripts/wave7/monitor_to_completion.sh  # Includes auto-commit
```

## Guarantees

With this protocol:

1. ✅ **Work is committed every 20 epics** (automatic)
2. ✅ **Backups created every 10 minutes** (automatic)
3. ✅ **git clean is BANNED** (never used)
4. ✅ **Safe pull process** (stash, not reset)
5. ✅ **Verification after every operation** (count epics before/after)

## Recovery If Something Goes Wrong

Even if disaster strikes:

```bash
# Option 1: Restore from git (if committed)
git log --oneline | head -20  # Find last checkpoint
git checkout <commit-hash> -- docs/brain/EPIC-W7-*

# Option 2: Restore from backup (if not committed)
ls -lt /tmp/wave7_backup_*  # Find latest backup
rsync -av /tmp/wave7_backup_LATEST/* docs/brain/

# Option 3: Restore from GitHub (if pushed)
git fetch origin main
git checkout origin/main -- docs/brain/EPIC-W7-*
```

## The Answer to Your Question

**"Will this happen every time we update a script?"**

**NO** - because:

1. Work is auto-committed every 20 epics (not waiting until end)
2. Backups are auto-created every 10 minutes
3. `git clean` is permanently banned
4. Safe pull process uses `git stash`, not `git reset --hard`
5. Verification checks run after every operation

**This incident taught us to build a bulletproof system.**

## Implementation Checklist

- [ ] Add auto-commit to monitoring script
- [ ] Create auto_backup.sh script
- [ ] Add backup to VM crontab
- [ ] Update launch scripts to use safe pull
- [ ] Ban `git clean` from all scripts
- [ ] Add verification checks to all git operations
- [ ] Test recovery procedures

**Once implemented, this CANNOT happen again.**