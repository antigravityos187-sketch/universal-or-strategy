# VM-Local Git Sync Protocol (V12.36)

**Version**: 1.0  
**Effective**: 2026-06-16  
**Status**: 🔴 MANDATORY - BLOCKING GATE  
**Severity**: P0 (Wave execution blocker)

## Problem Statement

**Wave 5 Pilot Test Incident**: VM was on commit `0d28fb4` (Wave 4 work) while local was on `dad30745` (post-rollback). This caused Bob to see already-extracted code (CYC=10) instead of the baseline code that needed extraction, leading to confusion and wasted execution time.

**Root Cause**: No protocol to verify VM and local git states are synchronized before wave execution.

## The Rule

**BEFORE EVERY WAVE EXECUTION**: VM and local MUST be on the SAME git commit.

**AFTER EVERY WAVE EXECUTION**: Sync VM changes back to local for PR creation.

## Pre-Wave Git Sync Checklist (Local → VM)

**Purpose**: Ensure VM starts with correct baseline code before wave execution.

### Step 1: Verify Local Git State

```bash
# Check current commit
git log -1 --oneline
# Expected: Shows latest commit hash and message

# Check branch
git branch --show-current
# Expected: gitbutler/workspace (or your working branch)

# Check for uncommitted changes
git status
# Expected: Clean working tree (no uncommitted src/ changes)
```

**BLOCKER**: If uncommitted src/ changes exist, commit or stash them before proceeding.

### Step 2: Push Local Commits to Origin

```bash
# Push to origin (creates/updates remote branch)
git push origin gitbutler/workspace --force

# Verify push succeeded
git log origin/gitbutler/workspace -1 --oneline
# Expected: Same commit as local HEAD
```

**Note**: Use `--force` if rebasing or amending commits. Use `--no-verify` only if pre-push validation blocks non-critical issues (e.g., test DLL missing).

### Step 3: Verify VM Git State (BEFORE Sync)

```bash
# Check VM current commit
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd ~/universal-or-strategy && git log -1 --oneline"

# Check VM branch
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd ~/universal-or-strategy && git branch --show-current"

# Check for uncommitted changes on VM
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd ~/universal-or-strategy && git status --short"
```

**Document**: Record VM commit hash and compare to local. If different, proceed to Step 4.

### Step 4: Sync VM to Match Local

```bash
# Fetch latest from origin
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd ~/universal-or-strategy && git fetch origin"

# Reset VM to match local commit (HARD RESET)
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd ~/universal-or-strategy && git reset --hard origin/gitbutler/workspace"

# Clean untracked files (Wave 4 artifacts, etc.)
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd ~/universal-or-strategy && git clean -fd"
```

**CRITICAL**: `git reset --hard` will DESTROY any uncommitted work on VM. Ensure VM has no valuable uncommitted changes before running.

### Step 5: Verify Sync Succeeded (MANDATORY)

```bash
# Check VM commit matches local
LOCAL_COMMIT=$(git log -1 --format="%H")
VM_COMMIT=$(gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd ~/universal-or-strategy && git log -1 --format='%H'")

echo "Local: $LOCAL_COMMIT"
echo "VM:    $VM_COMMIT"

# Compare (must match exactly)
if [ "$LOCAL_COMMIT" = "$VM_COMMIT" ]; then
  echo "✅ SYNC VERIFIED: VM and local on same commit"
else
  echo "❌ SYNC FAILED: Commits do not match"
  exit 1
fi
```

**BLOCKER**: If commits don't match, STOP and investigate. Do NOT proceed with wave execution.

### Step 6: Verify Source Files Match (Optional but Recommended)

```bash
# Check specific file hash on VM vs local
LOCAL_HASH=$(md5sum src/V12_002.cs | awk '{print $1}')
VM_HASH=$(gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd ~/universal-or-strategy && md5sum src/V12_002.cs | awk '{print \$1}'")

echo "Local V12_002.cs: $LOCAL_HASH"
echo "VM V12_002.cs:    $VM_HASH"

if [ "$LOCAL_HASH" = "$VM_HASH" ]; then
  echo "✅ FILE VERIFIED: V12_002.cs matches"
else
  echo "❌ FILE MISMATCH: V12_002.cs differs"
  exit 1
fi
```

**Use Case**: Verify critical files (e.g., target file for EPIC-CCN-001) match exactly.

## Post-Wave Git Sync Checklist (VM → Local)

**Purpose**: Sync VM changes back to local for PR creation and verification.

**When to Use**: After wave execution completes, before creating PRs.

### Step 1: Verify VM Git State

```bash
# Check VM current commit
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd ~/universal-or-strategy && git log -1 --oneline"

# Check for uncommitted changes on VM
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd ~/universal-or-strategy && git status --short"
```

**Expected**: VM should have uncommitted changes (modified src/ files, new brain files).

### Step 2: Download Changed Files from VM

```bash
# Download brain files (Phase 5-6 outputs)
gcloud compute scp -r \
  v12-test-golden-v2:~/universal-or-strategy/docs/brain/EPIC-CCN-* \
  ./docs/brain/ \
  --zone=us-central1-a

# Download modified source files
gcloud compute scp \
  v12-test-golden-v2:~/universal-or-strategy/src/V12_002.*.cs \
  ./src/ \
  --zone=us-central1-a

# Download logs (for bobcoin tracking)
gcloud compute scp -r \
  v12-test-golden-v2:~/universal-or-strategy/logs/wave5/ \
  ./logs/ \
  --zone=us-central1-a
```

### Step 3: Verify Files Downloaded

```bash
# Check brain files
ls docs/brain/EPIC-CCN-*/ticket-*-completion.md | wc -l
# Expected: Number of completed tickets

# Check source files modified
git status --short src/
# Expected: Modified files listed

# Check logs downloaded
ls logs/wave5/*.log | wc -l
# Expected: Number of epics executed
```

### Step 4: Run Local Verification (5 Mandatory Checks)

**Check 1: Compilation**
```powershell
dotnet build
# Expected: Exit code 0, zero errors
```

**Check 2: Complexity Reduction**
```bash
python scripts/complexity_audit.py
# Expected: Target methods CYC ≤8
```

**Check 3: Scope Compliance**
```bash
git diff HEAD --stat
# Expected: ONLY target files modified, no adjacent changes
```

**Check 4: Test Coverage**
```powershell
dotnet test
# Expected: xUnit tests exist and pass
```

**Check 5: Encoding Compliance**
```powershell
powershell -File .\scripts\check_encoding.ps1
# Expected: Exit code 0, all files UTF-8 without BOM
```

### Step 5: Commit and Push to Local Branch

```bash
# Stage changes
git add docs/brain/ src/ logs/

# Commit with wave summary
git commit -m "feat(wave5): Phase 5-6 execution complete

- Epics: EPIC-CCN-001 through EPIC-CCN-078
- Tickets: [count] tickets executed
- CYC: [before] → [after] (target ≤8)
- Tests: [count] xUnit tests passing
- Encoding: UTF-8 verified

Wave 5 execution with hardened protocols (V12.34-V12.36)"

# Push to origin
git push origin gitbutler/workspace
```

### Step 6: Verify Local and VM Still Synced (Optional)

```bash
# If VM committed changes, verify local matches
LOCAL_COMMIT=$(git log -1 --format="%H")
VM_COMMIT=$(gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd ~/universal-or-strategy && git log -1 --format='%H'")

echo "Local: $LOCAL_COMMIT"
echo "VM:    $VM_COMMIT"

# Note: VM may have uncommitted changes, that's OK
# What matters is local has all VM changes downloaded
```

## Integration Points

### 1. Wave Phase Script Generation SOP (V3.1 Update)

**Add to Section 2.1 (Pre-Generation Checklist)**:

```markdown
### 2.1.1 VM-Local Git Sync (MANDATORY)

Before generating ANY wave scripts:

1. ✅ Run VM-Local Git Sync Protocol (V12.36)
2. ✅ Verify local and VM on same commit
3. ✅ Document commit hash in wave monitoring file

**Reference**: `docs/protocol/VM_LOCAL_GIT_SYNC_PROTOCOL.md`
```

### 2. GCP VM Wave Execution Skill (V2.6 Update)

**Add to "Pre-Wave Checklist" section**:

```markdown
## Pre-Wave Checklist (MANDATORY)

### 0. VM-Local Git Sync (NEW - V12.36)
- [ ] Local git state clean (no uncommitted src/ changes)
- [ ] Local commits pushed to origin
- [ ] VM fetched latest from origin
- [ ] VM reset to match local commit (git reset --hard)
- [ ] Commit hashes verified identical (local vs VM)
- [ ] Document commit hash in wave monitoring file

**BLOCKER**: If sync fails, STOP and investigate before proceeding.

**Reference**: `docs/protocol/VM_LOCAL_GIT_SYNC_PROTOCOL.md`
```

### 3. Autonomous Refactor Mode Custom Instructions

**Add to "MANDATORY PROTOCOLS" section**:

```markdown
0. **VM-LOCAL GIT SYNC (V12.36 - BLOCKING GATE)**: Before EVERY wave execution,
   verify VM and local are on the SAME git commit. Run VM-Local Git Sync Protocol
   checklist. If commits don't match, STOP and sync before proceeding. Wave 5 pilot
   incident: VM on old commit (0d28fb4) caused Bob to see already-extracted code.
   Reference: docs/protocol/VM_LOCAL_GIT_SYNC_PROTOCOL.md
```

### 4. Wave Monitoring Template

**Add to every wave monitoring file**:

```markdown
## Git Sync Verification (V12.36)

**Pre-Wave Sync**:
- Local commit: `<hash>` (`<message>`)
- VM commit (before sync): `<hash>` (`<message>`)
- VM commit (after sync): `<hash>` (`<message>`)
- Sync status: ✅ VERIFIED / ❌ FAILED

**Verification Command**:
```bash
LOCAL_COMMIT=$(git log -1 --format="%H")
VM_COMMIT=$(gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd ~/universal-or-strategy && git log -1 --format='%H'")
echo "Local: $LOCAL_COMMIT"
echo "VM:    $VM_COMMIT"
```
```

## Failure Scenarios

### Scenario 1: VM Behind Local

**Symptom**: VM commit is older than local commit.

**Cause**: Local work not pushed to origin, or VM not fetched.

**Fix**:
1. Push local commits: `git push origin gitbutler/workspace --force`
2. Fetch on VM: `git fetch origin`
3. Reset VM: `git reset --hard origin/gitbutler/workspace`
4. Verify sync

### Scenario 2: VM Ahead of Local

**Symptom**: VM commit is newer than local commit.

**Cause**: Work done directly on VM (protocol violation), or local not pulled.

**Fix**:
1. Pull VM changes to local: `git pull origin main`
2. Rebase if needed: `git rebase origin/main`
3. Push to origin: `git push origin gitbutler/workspace --force`
4. Verify sync

**CRITICAL**: If VM has uncommitted work, investigate why. VM should NEVER have uncommitted changes between waves.

### Scenario 3: Diverged Histories

**Symptom**: VM and local have different commit histories (not just ahead/behind).

**Cause**: Force pushes, rebases, or parallel work on both environments.

**Fix**:
1. Decide which history is correct (usually local)
2. Force reset VM to match local: `git reset --hard origin/gitbutler/workspace`
3. Clean VM: `git clean -fd`
4. Verify sync

**CRITICAL**: Diverged histories indicate a serious protocol violation. Document root cause.

### Scenario 4: Uncommitted Changes on VM (Expected Post-Wave)

**Symptom**: `git status` on VM shows modified files after wave execution.

**Cause**: Wave execution completed, changes not yet synced to local.

**Fix** (This is NORMAL post-wave):
1. Download changes to local (see Post-Wave Sync Checklist)
2. Run 5 mandatory checks locally
3. Commit to local branch
4. Push to origin
5. Create PR

**Note**: Uncommitted changes on VM BEFORE wave execution are a problem. Uncommitted changes AFTER wave execution are expected and should be synced to local.

### Scenario 5: VM Ahead After Wave Execution

**Symptom**: VM has commits that local doesn't have.

**Cause**: Wave scripts committed changes on VM (unusual but possible).

**Fix**:
1. Pull VM commits to local: `git pull origin main`
2. Rebase if needed: `git rebase origin/main`
3. Verify 5 mandatory checks
4. Push to origin: `git push origin gitbutler/workspace`

## Automation Script (Optional)

Create `scripts/sync_vm_git.sh`:

```bash
#!/bin/bash
set -e

echo "=== VM-Local Git Sync Protocol (V12.36) ==="

# Step 1: Check local state
echo "[1/5] Checking local git state..."
LOCAL_COMMIT=$(git log -1 --format="%H")
LOCAL_MSG=$(git log -1 --format="%s")
echo "  Local: $LOCAL_COMMIT ($LOCAL_MSG)"

# Step 2: Push to origin
echo "[2/5] Pushing to origin..."
git push origin gitbutler/workspace --force --no-verify
echo "  ✅ Pushed to origin/gitbutler/workspace"

# Step 3: Check VM state (before sync)
echo "[3/5] Checking VM git state (before sync)..."
VM_COMMIT_BEFORE=$(gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd ~/universal-or-strategy && git log -1 --format='%H'")
echo "  VM (before): $VM_COMMIT_BEFORE"

# Step 4: Sync VM
echo "[4/5] Syncing VM to match local..."
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd ~/universal-or-strategy && git fetch origin && git reset --hard origin/gitbutler/workspace && git clean -fd"
echo "  ✅ VM synced"

# Step 5: Verify sync
echo "[5/5] Verifying sync..."
VM_COMMIT_AFTER=$(gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd ~/universal-or-strategy && git log -1 --format='%H'")
echo "  VM (after): $VM_COMMIT_AFTER"

if [ "$LOCAL_COMMIT" = "$VM_COMMIT_AFTER" ]; then
  echo "✅ SYNC VERIFIED: VM and local on same commit"
  echo "   Commit: $LOCAL_COMMIT"
  echo "   Message: $LOCAL_MSG"
  exit 0
else
  echo "❌ SYNC FAILED: Commits do not match"
  echo "   Local: $LOCAL_COMMIT"
  echo "   VM:    $VM_COMMIT_AFTER"
  exit 1
fi
```

**Usage**:
```bash
chmod +x scripts/sync_vm_git.sh
./scripts/sync_vm_git.sh
```

## Success Criteria

- ✅ Local and VM on identical commit (hash matches exactly)
- ✅ VM working tree clean (no uncommitted changes)
- ✅ Commit hash documented in wave monitoring file
- ✅ Source files verified matching (optional but recommended)

## Enforcement

**BLOCKING GATE**: This protocol is MANDATORY before every wave execution. Skipping this check is a P0 protocol violation.

**Audit**: Every wave monitoring file MUST include git sync verification section with commit hashes.

**Violation**: If wave execution proceeds without git sync verification, the wave is INVALID and must be rolled back.

## References

- **Wave 5 Pilot Test Incident**: VM on commit 0d28fb4 (Wave 4 work) instead of dad30745 (post-rollback)
- **Root Cause**: No protocol to verify git sync before wave execution
- **Impact**: Bob saw already-extracted code (CYC=10), wasted execution time
- **Fix**: This protocol (V12.36)

## Version History

- **V1.1** (2026-06-16): Added bidirectional sync (VM → Local post-wave) and 5-check verification
- **V1.0** (2026-06-16): Initial protocol created after Wave 5 pilot test incident

---

**Protocol Owner**: Wave Execution Lead  
**Last Updated**: 2026-06-16T21:35:00Z  
**Next Review**: After Wave 5 completion