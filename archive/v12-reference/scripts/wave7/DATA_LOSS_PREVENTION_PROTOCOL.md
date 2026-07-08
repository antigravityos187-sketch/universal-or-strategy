# Data Loss Prevention Protocol - Wave 7

## Current Protection Status

### Git Hooks (VM Only)
**Location**: `.git/hooks/` on VM (`v12-test-golden-v2`)

**Installed Hooks**:
1. `post-commit`: Attempts auto-push (FAILS - no GitHub credentials)
2. `pre-push`: Verifies epic count before push
3. `post-merge`: Creates backup after git pull

**What's Protected**:
- ✅ Epic directories (`docs/brain/EPIC-W7-*`)
- ❌ NOT .cs files (hooks don't touch src/)
- ✅ VM only (not local)

**Critical Limitation**: 
Hooks commit locally but **cannot push to GitHub** because VM lacks credentials. Commits stay on VM and are vulnerable to loss.

## Three-Layer Protection Strategy

### Layer 1: Live Monitoring (Active)
**Status**: ENABLED per user request

- Agent watches every epic launch in real-time
- No 4-minute polling delays
- Immediate issue detection
- Manual intervention on failures

**Implementation**: Agent stays connected to VM, monitors screen sessions

### Layer 2: Periodic Sync (Every 20 Epics)
**Status**: MANDATORY

**Sync Strategy**:
All 145 remaining epics will run continuously. Agent will trigger manual sync every 20 epic completions during live monitoring.

**Checkpoint Schedule**:
- After 20 epics complete (36 total with existing 16)
- After 40 epics complete (56 total)
- After 60 epics complete (76 total)
- After 80 epics complete (96 total)
- After 100 epics complete (116 total)
- After 120 epics complete (136 total)
- After 140 epics complete (156 total)
- After all 145 complete (161 total - final sync)

**Process**:
```bash
# Agent runs every 20 completions
bash scripts/wave7/sync_epics_from_vm.sh
```

**What It Does**:
1. Counts completed epics on VM
2. Uses `gcloud compute scp` to copy epic directories to local
3. Commits to local git
4. Pushes to GitHub immediately

**Safety Net**: Even if VM is lost, GitHub has backups every 20 epics (max loss: 19 epics)

**Live Monitoring**: Agent watches execution continuously and triggers sync every 20 completions

### Layer 3: Safe Script Updates
**Status**: PROTOCOL ESTABLISHED

**When Updating Scripts**:
```bash
# Step 1: Sync FIRST (before any changes)
bash scripts/wave7/sync_epics_from_vm.sh

# Step 2: Update script locally
# Edit script, test, commit, push to GitHub

# Step 3: Pull on VM (SAFE - no reset/clean)
gcloud compute ssh malhitticrypto@v12-test-golden-v2 --zone=us-central1-a \
  --command="cd ~/universal-or-strategy && git pull origin main"

# Step 4: Continue execution
# No data loss - epic directories preserved
```

**BANNED COMMANDS**:
- ❌ `git reset --hard HEAD` (destroys uncommitted work)
- ❌ `git clean -fd` (deletes all untracked files)

**SAFE COMMANDS**:
- ✅ `git pull origin main` (merges changes, preserves local work)
- ✅ `git stash` (temporarily saves changes)
- ✅ `git stash pop` (restores saved changes)

## Why This Works

### Previous Failures
1. **Incident 1** (143 epics lost): `git reset --hard && git clean -fd` deleted all untracked epic directories
2. **Root Cause**: No intermediate commits, all work untracked

### Current Solution
1. **Periodic Sync**: Every 20 epics go to GitHub (max loss: 19 epics)
2. **No Destructive Commands**: Never use reset/clean
3. **Live Monitoring**: Catch failures immediately
4. **Safe Updates**: Always sync before pulling

### Recovery Capability
- **VM Lost**: Restore from GitHub (last sync point)
- **Script Update Needed**: Sync first, then pull safely
- **Epic Failure**: Re-run single epic, no batch loss
- **API Key Exhaustion**: Replace key, continue from checkpoint

## Execution Plan for 145 Remaining Epics

### Continuous Execution Strategy
- **Mode**: All 145 epics run continuously (no stopping between syncs)
- **API Keys**: 20 keys rotate automatically across epics
- **Monitoring**: Live (agent watches screen sessions in real-time)
- **Key Rotation**: Replace exhausted keys as needed during execution
- **Sync Frequency**: Every 20 epic completions (no execution pause)

### Sync Checkpoints (Every 20 Epics)
- Checkpoint 1: After 20 new epics (36 total)
- Checkpoint 2: After 40 new epics (56 total)
- Checkpoint 3: After 60 new epics (76 total)
- Checkpoint 4: After 80 new epics (96 total)
- Checkpoint 5: After 100 new epics (116 total)
- Checkpoint 6: After 120 new epics (136 total)
- Checkpoint 7: After 140 new epics (156 total)
- Final: After all 145 new epics (161 total)

### Failure Handling
1. **Epic Fails**: Note failure, continue execution
2. **Checkpoint Reached**: Sync to GitHub
3. **Review Failures**: Analyze logs during monitoring
4. **Re-run Failed**: Launch individually with fixes
5. **Sync Again**: After recovery completes

## API Key Management

### Current Keys (20 Total)
All keys have ~160 bobcoins each = 3,200 total capacity

### Usage Tracking
- Each epic uses ~15 bobcoins
- 145 epics × 15 = 2,175 bobcoins needed
- Buffer: 1,025 bobcoins (32% safety margin)

### Replacement Protocol
When key exhausted:
1. User provides fresh key
2. Update `docs/API/<keyname>.json`
3. Regenerate affected epic scripts
4. Continue execution

## Success Criteria

- ✅ 161/161 epics completed (100%)
- ✅ All epic directories in GitHub
- ✅ Zero data loss incidents
- ✅ All failures documented and recovered
- ✅ Final sync confirms all work preserved

## Emergency Procedures

### If VM Becomes Unresponsive
1. DO NOT restart VM immediately
2. Try to SSH and check epic count
3. If accessible: sync epics first
4. Then restart if needed

### If Script Update Causes Issues
1. DO NOT use git reset/clean
2. Sync current epics first
3. Fix script locally
4. Pull on VM (safe merge)
5. Continue execution

### If GitHub Push Fails
1. Check network connectivity
2. Verify GitHub credentials
3. Retry push manually
4. If persistent: save epic directories locally as backup

## Monitoring Commands

**Check VM Epic Count**:
```bash
gcloud compute ssh malhitticrypto@v12-test-golden-v2 --zone=us-central1-a \
  --command="find ~/universal-or-strategy/docs/brain -maxdepth 1 -type d -name 'EPIC-W7-*' -exec test -f {}/00-hotspots.md \; -print | wc -l"
```

**Sync Epics to GitHub**:
```bash
bash scripts/wave7/sync_epics_from_vm.sh
```

**Check Active Screen Sessions**:
```bash
gcloud compute ssh malhitticrypto@v12-test-golden-v2 --zone=us-central1-a \
  --command="screen -ls"
```

## Conclusion

This three-layer protocol ensures:
1. **Real-time visibility** (live monitoring)
2. **Regular backups** (every 20 epics)
3. **Safe updates** (no destructive commands)
4. **Quick recovery** (GitHub checkpoints)

**Maximum Data Loss**: 19 epics (between sync points)
**Recovery Time**: <5 minutes (re-run from last checkpoint)
**Success Probability**: >95% (based on previous wave patterns)