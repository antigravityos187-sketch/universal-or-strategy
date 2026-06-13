---
name: gcp-vm-wave-execution
description: Launch autonomous epic execution waves on GCP VMs using pre-configured golden images for parallel refactoring with automatic recovery and file persistence verification
---

# GCP VM Wave Execution

Launch autonomous epic execution waves on GCP VMs using pre-configured golden images for parallel refactoring with automatic recovery and file persistence verification.

## What it does

Orchestrates parallel execution of V12 epic workflows on a GCP VM, using Bob Shell agents running in screen sessions. Each agent executes phases independently (Phase 0 → Phase 6), with automatic file verification and bobcoin tracking.

## When to use

- Starting a new wave of epic refactoring (Wave 2, Wave 3, etc.)
- Need to execute 9+ epics in parallel
- Want to leverage GCP spot instances for cost efficiency
- Require isolated execution environments for each epic
- Need automatic recovery from file persistence failures

## What you need

- GCP project with 12+ vCPU quota
- Golden image `v12-bob-shell-golden-v2` (or later)
- 10 Bob Shell API keys (160 bobcoins each = 1,600 total)
- jCodemunch-MCP with indexed repository
- gcloud CLI installed and authenticated
- Obsidian Kanban board at: `C:\Users\Mohammed Khalid\Documents\V12-Agent-Vault`

## Critical Configuration (NEVER REPEAT)

**Reference**: `docs/workflow/WAVE_2_CONFIGURATION.md`

This file contains:
- Obsidian Kanban path (permanent)
- API allocation (immutable - 1 unique API per epic)
- Monitoring commands (copy-paste ready)
- Success criteria (per phase)
- Emergency procedures (kill switches)

**ALWAYS read this file first** to avoid repeating setup.

## How to use it

### Phase-by-Phase Workflow (V12.25)

**Architecture**: Manifest-based independent subtasks (not monolithic)

Each phase runs as a separate session with clear inputs/outputs tracked in `manifest.json`.

### Phase 0: Hotspot Analysis (3-5 bobcoins/epic)

**Purpose**: Identify high-complexity methods using jCodemunch

**Launch**:
```bash
# 1. Generate Phase 0 scripts
python scripts/wave2/launch_phase0_fixed.py

# 2. Upload to VM
gcloud compute scp scripts/wave2/_p0_*.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a
gcloud compute scp scripts/wave2/launch_phase0_all.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a

# 3. Execute
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="chmod +x /home/malhitticrypto/universal-or-strategy/launch_phase0_all.sh && /home/malhitticrypto/universal-or-strategy/launch_phase0_all.sh"
```

**Monitor**:
```bash
# Check completion (expect "No Sockets found" when done)
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="screen -ls"

# Verify files created (expect 9)
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="ls /home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-CCN-*/00-hotspots.md 2>/dev/null | wc -l"

# Extract bobcoin usage
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="grep -A 2 'BOBCOIN REPORT' /home/malhitticrypto/universal-or-strategy/logs/phase0/*.log"
```

**Success Criteria**:
- ✅ All 9 screen sessions complete (DONE_EXIT=0)
- ✅ Files exist: `docs/brain/EPIC-CCN-{ID}/00-hotspots.md`
- ✅ Files exist: `docs/brain/EPIC-CCN-{ID}/manifest.json`
- ✅ Bobcoin usage reported in logs
- ✅ All APIs remain positive (>10 bobcoins)

### Phase 1: Scope Definition (5-10 bobcoins/epic)

**Purpose**: Define extraction scope based on hotspot analysis

**Launch**: Same pattern as Phase 0, use `launch_phase1_fixed.py`

### Phase 1.5: Scope Boundary (2-3 bobcoins/epic)

**Purpose**: MANDATORY validation gate to prevent scope creep (V12.23 Protocol)

**Launch**: Same pattern, use `launch_phase1_5_fixed.py`

### Phase 2: Architecture Planning (10-15 bobcoins/epic)

**Purpose**: Create detailed extraction plan with method signatures

**Launch**: Same pattern, use `launch_phase2_fixed.py`

### Phase 3: DNA & PR Audit (5-10 bobcoins/epic)

**Purpose**: V12 DNA compliance checks and PR hygiene validation

**Launch**: Same pattern, use `launch_phase3_fixed.py`

### Phase 4: Ticket Generation (5-10 bobcoins/epic)

**Purpose**: Generate surgical extraction tickets

**Launch**: Same pattern, use `launch_phase4_fixed.py`

### Phase 5: Ticket Execution (TBD - separate wave)

**Purpose**: Bob CLI executes surgical extractions

**Note**: This phase requires separate wave due to higher complexity

### Phase 6: Final Review (TBD - separate wave)

**Purpose**: Completion report and roadmap update

## Self-Healing Features

### 1. File Persistence Verification

**Problem**: Wave 2 v4 claimed "complete" but files never existed on disk

**Solution**: Message file approach + explicit verification
- Creates `/tmp/phase0_msg_{ID}.txt` (avoids bash multi-line escaping)
- Agents must confirm files exist on disk before reporting success
- Verification steps built into every phase prompt

**Reference**: `docs/workflow/V12_EPIC_WORKFLOW_FILE_PERSISTENCE_FIX.md`

### 2. Bobcoin Tracking (MANDATORY)

**Problem**: Risk of APIs going negative without tracking

**Solution**: Agents MUST self-report usage AND remaining balance
- Every phase prompt includes: "Cost: X.XX | Balance: Y.YY"
- Bob Shell automatically reports cost in attempt_completion
- Agents must also report remaining balance
- Extract with: `grep 'Cost:.*Balance:' logs/phase*/*.log`

**Format Required**:
```
Cost: 0.68 | Balance: 159.32
```

**Extraction Command**:
```bash
# Extract all bobcoin usage + balance
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="grep -E 'Cost:.*Balance:|Cost: [0-9]' /home/malhitticrypto/universal-or-strategy/logs/phase*/EPIC-CCN-*.log"
```

**If Balance Not Reported**:
- Use Cost only: Track cumulative usage
- Calculate balance: Initial (160) - Cumulative usage
- **CRITICAL**: If balance calculation shows <10 bobcoins, STOP immediately

### 3. API Isolation

**Problem**: Shared API causes quota contention

**Solution**: 1 unique API per epic (no sharing)
- API allocation is IMMUTABLE (defined in `WAVE_2_CONFIGURATION.md`)
- Validation before launch prevents duplicates
- Each epic has dedicated quota

### 4. SSH Connection Recovery

**Problem**: SSH fails with "No Sockets found" or connection errors

**Solution**: Automatic retry with troubleshooting
```bash
# If SSH fails, check VM status first
gcloud compute instances list --filter="name=v12-test-golden-v2"

# If RUNNING but SSH fails, use troubleshoot flag
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --troubleshoot
```

### 5. Phase Checkpointing

**Problem**: Monolithic workflow can't resume from failure

**Solution**: Manifest-based phase tracking
- Each phase updates `manifest.json` with status
- Can resume from any phase after failure
- No need to restart entire wave

## Common Issues & Auto-Recovery

### Issue: Files Not Persisting to Disk (CRITICAL)
**Meaning**: Bob Shell reports "files created" but they don't exist on VM disk
**Root Cause**: Missing `--yolo` flag in Bob Shell invocation for non-interactive/SSH mode
**Diagnosis**: Bob Shell requires explicit permission to modify files when running autonomously
**Solution**:
```bash
# WRONG (files appear created in logs but don't persist)
bob --chat-mode v12-phase0-hotspot "$(cat /tmp/phase0_msg_107.txt)"

# CORRECT (files actually persist to disk)
bob --yolo --chat-mode v12-phase0-hotspot "$(cat /tmp/phase0_msg_107.txt)"
```
**Fix Script**: `scripts/wave2/add_yolo_flag.ps1` - Adds `--yolo` flag to all Phase 0 scripts
**Validation**: Run test epic and verify files exist with `ls -lh docs/brain/EPIC-CCN-107/`
**Reference**: Wave 2 Phase 0 completion (2026-06-13) - 8/9 epics succeeded after fix

**MANDATORY**: ALL non-interactive Bob Shell invocations MUST include `--yolo` flag.

### Issue: API Key Authentication Failed (HTTP 401)
**Meaning**: Wrong environment variable used for Bob Shell API key
**Root Cause**: Bob Shell requires `BOBSHELL_API_KEY` environment variable, NOT `BOB_API_KEY_FILE`
**Solution**:
```bash
# WRONG (will fail with 401 Unauthorized)
export BOB_API_KEY_FILE="$HOME/.bob/api-keys/filename.json"

# CORRECT (working)
export BOBSHELL_API_KEY='bob_prod_bob-admin_...'
```
**Fix Script**: `scripts/wave2/fix_api_key_env.sh` - Extracts API key from JSON and sets correct env var
**Reference**: Wave 2 v4 launch script (line 135) uses `BOBSHELL_API_KEY` successfully

### Issue: "No Sockets found"
**Meaning**: All screen sessions completed (GOOD)
**Action**: Verify files created, extract bobcoin usage, launch next phase

### Issue: SSH connection fails
**Meaning**: Temporary network issue or VM restarted
**Action**: Check VM status, wait 30 seconds, retry

### Issue: Files not created (RESOLVED - see above)
**Meaning**: File persistence failure due to missing `--yolo` flag
**Action**: Add `--yolo` flag to Bob Shell invocation, verify with test epic

### Issue: Bobcoin usage not reported
**Meaning**: Agent didn't reach reporting section
**Action**: Check logs for errors, verify API key valid, relaunch

### Issue: API goes negative
**Meaning**: Budget exceeded or duplicate API usage
**Action**: STOP immediately, check API allocation, verify no duplicates, contact IBM for reset

## Budget Management

### Pre-Launch Validation
```python
# Always validate before launch
total_available = 10 * 160  # 1,600 bobcoins
phase_budget = epics * bobcoins_per_epic
safety_margin = (total_available - phase_budget) / total_available

assert safety_margin >= 0.10, "Safety margin must be ≥10%"
```

### Post-Launch Tracking
```bash
# Extract actual usage + balance
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="grep -E 'Cost:.*Balance:|Cost: [0-9]' /home/malhitticrypto/universal-or-strategy/logs/phase*/EPIC-CCN-*.log"

# Calculate totals
# If Balance reported: Use directly
# If only Cost reported: Sum costs, subtract from initial (160)

# Update tracking file
# Create: docs/workflow/WAVE_2_PHASE_X_BOBCOIN_USAGE.md
# Include: Per-epic cost, total cost, remaining balance per API
```

## Monitoring Dashboard

### Quick Status Check
```bash
# 1. VM running?
gcloud compute instances list --filter="name=v12-test-golden-v2"

# 2. Agents running?
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="screen -ls"

# 3. Files created?
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="ls /home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-CCN-*/00-hotspots.md 2>/dev/null | wc -l"

# 4. Bobcoins used + balance?
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="grep -E 'Cost:.*Balance:|Cost: [0-9]' /home/malhitticrypto/universal-or-strategy/logs/phase*/*.log | head -20"
```

### Detailed Monitoring
```bash
# View specific log
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="tail -100 /home/malhitticrypto/universal-or-strategy/logs/phase0/EPIC-CCN-107.log"

# Check for errors
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="grep -i 'error\|failed\|exception' /home/malhitticrypto/universal-or-strategy/logs/phase*/*.log"

# Attach to running agent (Ctrl+A, D to detach)
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="screen -r p0-107"
```

## SSH Terminal Setup (VSCode Remote-SSH)

**Purpose**: Connect VSCode directly to VM for live monitoring and file editing

**When to Use**:
- Want to watch phase execution in real-time
- Need to edit files directly on VM
- Debugging phase failures
- Monitoring screen sessions interactively

### Prerequisites
- gcloud CLI authenticated
- VM running and accessible
- SSH keys configured (`~/.ssh/google_compute_engine`)

### Setup Steps

**1. Get VM External IP**:
```bash
gcloud compute instances describe v12-test-golden-v2 \
  --zone=us-central1-a \
  --format="get(networkInterfaces[0].accessConfigs[0].natIP)"
```

**2. Create SSH Config**:

Create/edit `C:\Users\Mohammed Khalid\.ssh\config`:
```
Host v12-vm
    HostName <EXTERNAL_IP_FROM_STEP_1>
    User malhitticrypto
    IdentityFile ~/.ssh/google_compute_engine
    StrictHostKeyChecking no
    UserKnownHostsFile /dev/null
```

**3. Test Connection**:
```bash
ssh v12-vm
```

**4. Connect VSCode**:
- Open VSCode
- Press `Ctrl+Shift+P`
- Type "Remote-SSH: Connect to Host"
- Select "v12-vm"
- Wait for connection (first time may take 30-60 seconds)

### Using Remote Terminal

**Once connected**:
```bash
# Navigate to project
cd ~/universal-or-strategy

# List screen sessions
screen -ls

# Attach to specific session
screen -r p3-107

# Detach from session (keep it running)
# Press: Ctrl+A, then D

# View logs in real-time
tail -f logs/phase3/EPIC-CCN-107.log

# Check file creation
ls -lh docs/brain/EPIC-CCN-107/
```

### Common Issues

**Issue**: "Could not establish connection"
- **Cause**: Wrong hostname format or IP changed
- **Fix**: Re-run Step 1 to get current IP, update SSH config

**Issue**: "Permission denied (publickey)"
- **Cause**: SSH keys not configured
- **Fix**: Run `gcloud compute config-ssh` to regenerate keys

**Issue**: "screen: command not found" (locally)
- **Cause**: Running screen command on local machine instead of VM
- **Fix**: SSH to VM first, then run screen commands

### Tips

- **Live Monitoring**: Keep VSCode connected during phase execution
- **Multiple Terminals**: Open multiple terminals in VSCode to monitor different logs
- **File Editing**: Edit scripts directly on VM, no need to scp
- **Screen Sessions**: Use `screen -ls` to see all running phases
- **Detach Safely**: Always use `Ctrl+A, D` to detach (don't close terminal)

## Emergency Procedures

### Stop All Agents
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="killall screen"
```

### Relaunch Single Epic
```bash
# Example: Relaunch EPIC-CCN-107 Phase 0
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd /home/malhitticrypto/universal-or-strategy && screen -dmS p0-107 bash -l -c './_p0_107.sh 2>&1 | tee logs/phase0/EPIC-CCN-107.log'"
```

### Check API Balances
Login to IBM Bob Shell dashboard and verify all APIs remain positive.

## Tips

- **Cost**: Phase 0-4 = ~270-477 bobcoins total (17-30% of budget)
- **Time**: 30-60 minutes per phase (parallel execution)
- **Quality**: Always verify files on disk before proceeding
- **Recovery**: Can resume from any phase using manifest
- **Budget**: Maintain 10%+ safety margin at all times

## Related Skills

- `gcp-golden-image-creation` (prerequisite)
- `jcodemunch-complexity-analysis` (prerequisite)
- `v12-epic-workflow` (phase definitions)

## Version History

- **V1.0** (2026-06-11): Initial monolithic workflow
- **V2.0** (2026-06-12): Phase-by-phase with file persistence fix
- **V2.1** (2026-06-12): Added self-healing and bobcoin tracking
- **V2.2** (2026-06-12): Added API key environment variable fix (BOBSHELL_API_KEY)
- **V2.3** (2026-06-13): **CRITICAL FIX**: Added `--yolo` flag requirement for file persistence

## Post-Use Audit (MANDATORY)

After every use of this skill:
1. ✅ Check if any instruction was ambiguous
2. ✅ Update this file if gaps found
3. ✅ Document new failure modes in "Common Issues"
4. ✅ Add recovery procedures for new issues
5. ✅ State "skill(gcp-vm-wave-execution): no gaps identified" if no gaps found

**Last Audit**: 2026-06-13 05:59 UTC - **CRITICAL SOP ADDED**: After Phase 1 debugging (3 failed launches), created mandatory SOP `docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP.md`. Key rule: ALWAYS copy previous working phase scripts, NEVER generate from scratch. Phase 1 failures were caused by: (1) jq extraction vs hardcoded keys, (2) wrong JSON field `.key` vs `.apikey`, (3) wrong launcher pattern `bash -c` vs `bash -l`. All issues resolved by copying Phase 0 pattern exactly. Updated skill to reference SOP for all future phase script generation.

## Wave 2 Phase 0 Progress (2026-06-13)

**Status**: ✅ 8/9 Epics Completed Successfully (89% success rate)

**Completed Epics**:
- ✅ EPIC-CCN-107: HydrateFromOpenPositions (CYC 31) - 2.7K + 236B
- ✅ EPIC-CCN-108: 3.5K + 229B
- ✅ EPIC-CCN-109: 1.8K + 242B
- ✅ EPIC-CCN-110: 1.5K + 229B
- ✅ EPIC-CCN-111: 3.4K + 246B
- ✅ EPIC-CCN-113: 2.9K + 624B
- ✅ EPIC-CCN-114: 4.6K + 231B
- ✅ EPIC-CCN-115: 2.2K + 230B

**Failed Epics**:
- ❌ EPIC-CCN-112: jCodemunch MCP timeout (likely API rate limit from 9 parallel requests)

**Files Created**: 16/18 (88.9%)

**Key Learnings**:
1. `--yolo` flag is MANDATORY for file persistence in SSH mode
2. Custom mode configuration (`.bob/custom_modes.yaml`) was correct - no changes needed
3. Parallel execution works well (8/9 success) with proper API isolation
4. jCodemunch rate limits may affect parallel execution - consider staggering launches

**Next Steps**:
1. Retry EPIC-CCN-112 individually
2. Proceed to Phase 1 (Scope Definition) with all 9 epics
3. Apply `--yolo` flag to all future phase scripts

**Reference**: `WAVE2_PHASE0_COMPLETION_REPORT.md` for detailed analysis

## Wave 2 Phase 1 Progress (2026-06-13)

**Status**: ✅ RUNNING (after 3 failed launches)

**Root Cause of Failures**:
1. Generator script created Phase 1 from scratch instead of copying Phase 0
2. Used `jq` extraction instead of hardcoded API keys
3. Used wrong JSON field `.key` instead of `.apikey`
4. Used wrong launcher pattern `bash -c` instead of `bash -l`

**Solution Applied**:
- Fixed generator to load API keys from JSON and hardcode into scripts
- Updated launcher to use `bash -l` (login shell) like Phase 0
- Created mandatory SOP: `docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP.md`

**Key Learning**: **ALWAYS copy working phase scripts, NEVER generate from scratch**

**SOP Mandate**: For Phase 2 and beyond:
1. Copy Phase 1 scripts: `cp _p1_*.sh _p2_*.sh`
2. Use find-and-replace for phase-specific changes only
3. Verify against working phase with `diff`
4. Test one script before deploying all 9

**Reference**: `docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP.md` for complete procedure