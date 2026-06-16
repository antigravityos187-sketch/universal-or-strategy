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
- 15 Bob Shell API keys (160 bobcoins each = 2,400 total)
- jCodemunch-MCP with indexed repository
- Sequential Thinking MCP configured (`.bob/mcp.json`)
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

### 100% Completion Mandate (V12.28 - ABSOLUTE PRIORITY)

**CRITICAL**: ALL epics in scope MUST reach 100% completion before proceeding to next phase.

**Rules**:
- NEVER dismiss any epic as "not our concern" or "out of scope" without explicit Director approval
- If an epic exists in the roadmap or has a brain directory, it IS in scope and MUST be completed
- Naming mismatches (EPIC-CCN-27 vs EPIC-CCN-027) do NOT exempt an epic from completion
- Missing Phase 5 files do NOT exempt an epic from Phase 6 - execute Phase 5 first, then Phase 6
- The goal is ALWAYS N/N (100%), never N-1/N or "close enough"
- Every incomplete epic is a blocker to wave completion

**Example Violation** (Wave 4):
- EPIC-CCN-027 and 045 dismissed as "not our concern" because they had naming mismatches
- Result: Wave reported 79/79 complete when actually 77/80 (96.25%)
- Root cause: Assumed naming mismatch meant "out of scope"
- Correct action: Investigate ALL epics, execute missing phases, achieve true 80/80

**Reference**: `WAVE4_EPIC_027_045_STATUS.md`

### Phase-by-Phase Workflow (V12.25)

**Architecture**: Manifest-based independent subtasks (not monolithic)

Each phase runs as a separate session with clear inputs/outputs tracked in `manifest.json`.

### Phase 0: Hotspot Analysis (3-5 bobcoins/epic)

**Purpose**: Identify high-complexity methods using jCodemunch

**Sequential Thinking**: ❌ NOT REQUIRED (mechanical analysis)

**Test Framework**: N/A (no test generation in Phase 0)

**Launch**:
```bash
# 1. Generate Phase 0 scripts
python scripts/wave2/launch_phase0_fixed.py

# 2. Upload to VM
gcloud compute scp scripts/wave2/_p0_*.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/scripts/wave2/ --zone=us-central1-a
gcloud compute scp scripts/wave2/launch_phase0_all.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/scripts/wave2/ --zone=us-central1-a

# 3. MANDATORY: Verify Upload (CRITICAL - prevents silent failures)
LOCAL_COUNT=$(ls scripts/wave2/_p0_*.sh | wc -l)
VM_COUNT=$(gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="ls ~/universal-or-strategy/scripts/wave2/_p0_*.sh | wc -l")

if [ "$LOCAL_COUNT" != "$VM_COUNT" ]; then
    echo "ERROR: Upload incomplete. Local: $LOCAL_COUNT, VM: $VM_COUNT"
    exit 1
fi
echo "✅ Upload verified: $LOCAL_COUNT scripts"

# 4. Set permissions
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="chmod +x ~/universal-or-strategy/scripts/wave2/_p0_*.sh ~/universal-or-strategy/scripts/wave2/launch_phase0_all.sh"

# 5. Run pilot test (2 epics)
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd universal-or-strategy && ./scripts/wave2/launch_phase0_test.sh"
```

**Monitor**:
```bash
# Check progress every 4 minutes
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd universal-or-strategy && ls docs/brain/EPIC-CCN-*/00-hotspots.md | wc -l"
```

**Success Criteria**:
- ✅ File exists: `docs/brain/EPIC-CCN-{ID}/00-hotspots.md`
- ✅ File size >1K
- ✅ Contains jCodemunch hotspot data
- ✅ Bobcoin usage <5 per epic

### Phase 5: Ticket Execution (5-10 bobcoins/epic)

**Purpose**: Execute surgical refactoring tickets using Bob CLI

**Sequential Thinking**: ✅ REQUIRED (complex refactoring decisions)

**Test Framework**: xUnit 2.9.0+ ONLY (V12.32 Protocol)

**CRITICAL**: Bob CLI MUST generate xUnit tests, NEVER NUnit or MSTest

**Test Framework Validation**:
```bash
# Before Phase 5, verify project test framework
grep "xunit" tests/V12_Performance.Tests/V12_Performance.Tests.csproj
# Expected: <PackageReference Include="xunit" Version="2.9.0" />
```

**xUnit Patterns** (MANDATORY):
- ✅ Attributes: `[Fact]`, `[Theory]`, `[InlineData]`
- ✅ Assertions: `Assert.Equal()`, `Assert.NotNull()`, `Assert.True()`, `Assert.False()`
- ✅ Namespace: `using Xunit;`

**NUnit Patterns** (BANNED):
- ❌ Attributes: `[Test]`, `[TestFixture]`, `[TestCase]`
- ❌ Assertions: `Assert.AreEqual()`, `Assert.IsNotNull()`, `Assert.IsTrue()`
- ❌ Namespace: `using NUnit.Framework;`

**MSTest Patterns** (BANNED):
- ❌ Attributes: `[TestMethod]`, `[TestClass]`
- ❌ Assertions: `Assert.AreEqual()`, `Assert.IsNotNull()`
- ❌ Namespace: `using Microsoft.VisualStudio.TestTools.UnitTesting;`

**Rationale**: EPIC-027 TICKET-1 generated NUnit tests → 29 compilation errors → manual conversion required

**Launch**:
```bash
# 1. Generate Phase 5 scripts (copy from previous wave Phase 5)
python scripts/wave2/generate_wave2_phase5_scripts.py

# 2. Upload to VM
gcloud compute scp scripts/wave2/_p5_*.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/scripts/wave2/ --zone=us-central1-a

# 3. MANDATORY: Verify Upload
LOCAL_COUNT=$(ls scripts/wave2/_p5_*.sh | wc -l)
VM_COUNT=$(gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="ls ~/universal-or-strategy/scripts/wave2/_p5_*.sh | wc -l")

if [ "$LOCAL_COUNT" != "$VM_COUNT" ]; then
    echo "ERROR: Upload incomplete. Local: $LOCAL_COUNT, VM: $VM_COUNT"
    exit 1
fi
echo "✅ Upload verified: $LOCAL_COUNT scripts"

# 4. Set permissions
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="chmod +x ~/universal-or-strategy/scripts/wave2/_p5_*.sh"

# 5. Execute
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd ~/universal-or-strategy && ./scripts/wave2/launch_phase0_all.sh"
```

**Why Upload Verification Matters**:
- Wave 4 Phase 5: 7 scripts never uploaded → 7 epics failed silently
- No error message, scripts just missing on VM
- Cost: 1-2 hours recovery time + debugging effort
- **ALWAYS verify counts match before proceeding**

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
- ✅ **CPU usage metrics collected** (see Monitoring section below)

### Phase 1: Scope Definition (5-10 bobcoins/epic)

**Purpose**: Define extraction scope based on hotspot analysis

**Sequential Thinking**: ✅ MANDATORY (complex reasoning for scope boundaries)

**Launch**: Same pattern as Phase 0, use `launch_phase1_fixed.py`

### Phase 1.5: Scope Boundary (2-3 bobcoins/epic)

**Purpose**: MANDATORY validation gate to prevent scope creep (V12.23 Protocol)

**Sequential Thinking**: ✅ MANDATORY (validation requires step-by-step verification)

**Launch**: Same pattern, use `launch_phase1_5_fixed.py`

### Phase 2: Architecture Planning (10-15 bobcoins/epic)

**Purpose**: Create detailed extraction plan with method signatures

**Sequential Thinking**: ✅ MANDATORY (architectural decisions require explicit reasoning)

**Jane Street KB**: ✅ MANDATORY (query for extraction patterns)

**Launch**: Same pattern, use `launch_phase2_fixed.py`

### Phase 3: DNA & PR Audit (5-10 bobcoins/epic)

**Purpose**: V12 DNA compliance checks and PR hygiene validation

**Sequential Thinking**: ✅ MANDATORY (compliance verification requires systematic checks)

**Jane Street KB**: ⚠️ RECOMMENDED (query for V12 DNA rules)

**Launch**: Same pattern, use `launch_phase3_fixed.py`

### Phase 4: Ticket Generation (5-10 bobcoins/epic)

**Purpose**: Generate surgical extraction tickets

**Sequential Thinking**: ✅ MANDATORY (ticket breakdown requires logical decomposition)

**Launch**: Same pattern, use `launch_phase4_fixed.py`

### Phase 4.5: Ticket Review (5-10 bobcoins/epic)

**Purpose**: MANDATORY validation of generated tickets

**Sequential Thinking**: ✅ MANDATORY (ticket validation requires systematic review)

**Jane Street KB**: ✅ MANDATORY (query for ticket validation criteria)

**Launch**: Same pattern, use `launch_phase4_5_fixed.py`

### Phase 5: Ticket Execution (TBD - separate wave)

**Purpose**: Bob CLI executes surgical extractions

**Sequential Thinking**: ✅ MANDATORY (execution decisions require step-by-step reasoning)

**Jane Street KB**: ✅ MANDATORY (query for implementation patterns)

**Note**: This phase requires separate wave due to higher complexity

### Phase 5.V: Verification (5-10 bobcoins/epic)

**Purpose**: Verify ticket execution succeeded

**Sequential Thinking**: ✅ MANDATORY (verification requires systematic checks)

**Jane Street KB**: ⚠️ RECOMMENDED (query for testing patterns)

**Launch**: Same pattern, use `launch_phase5_v_fixed.py`

### Phase 6: Final Review (TBD - separate wave)

**Purpose**: Completion report and roadmap update

**Sequential Thinking**: ❌ NOT REQUIRED (mechanical reporting)

## Sequential Thinking MCP Integration (V12.25)

### Configuration Required

**Local Configuration** (already complete):
- ✅ `.bob/mcp.json` - Sequential thinking MCP configured
- ✅ `.bob/custom_modes.yaml` - autonomous-refactor mode updated
- ✅ `.mcp.json` - Claude configuration (if needed)

**VM Configuration** (must deploy before wave):
```bash
# 1. Upload Bob IDE MCP config (CRITICAL - used by Bob Shell + workers)
gcloud compute scp .bob/mcp.json v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/.bob/ --zone=us-central1-a

# 2. Upload custom modes
gcloud compute scp .bob/custom_modes.yaml v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/.bob/ --zone=us-central1-a

# 3. Verify uploads
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="ls -lh /home/malhitticrypto/universal-or-strategy/.bob/mcp.json"
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="ls -lh /home/malhitticrypto/universal-or-strategy/.bob/custom_modes.yaml"

# 4. Test sequential thinking MCP on VM
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="npx -y @modelcontextprotocol/server-sequential-thinking --version"
```

### Phase-Specific Usage

**Phases Requiring Sequential Thinking** (9 out of 10):
- ❌ Phase -1 (Pre-flight): Not required
- ❌ Phase 0 (Hotspot): Not required (mechanical analysis)
- ✅ Phase 1 (Scope + Boundary): MANDATORY
- ✅ Phase 2 (Architecture): MANDATORY + Jane Street KB
- ✅ Phase 3 (Audit): MANDATORY
- ✅ Phase 4 (Tickets): MANDATORY
- ✅ Phase 4.5 (Ticket Review): MANDATORY + Jane Street KB
- ✅ Phase 5 (Execution): MANDATORY + Jane Street KB
- ✅ Phase 5.V (Verification): MANDATORY
- ❌ Phase 6 (Final Review): Not required (mechanical reporting)

**How Bob Shell Uses Sequential Thinking**:
```bash
# Bob Shell automatically has access when .bob/mcp.json is present
bob --yolo --chat-mode plan "$(cat /tmp/phase2_msg_001.txt)"

# Bob Shell will:
# 1. Load .bob/mcp.json
# 2. See sequential-thinking MCP server
# 3. Use sequentialthinking tool for complex reasoning
# 4. Break down architectural decisions into explicit steps
```

### Verification Commands

**Check Sequential Thinking Available**:
```bash
# On VM, verify MCP server can run
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="npx -y @modelcontextprotocol/server-sequential-thinking --version"
```

**Monitor Sequential Thinking Usage**:
```bash
# Check logs for sequential thinking tool usage
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="grep -i 'sequentialthinking\|thought' /home/malhitticrypto/universal-or-strategy/logs/phase*/EPIC-CCN-*.log | head -20"
```

### Troubleshooting

**Issue**: "sequentialthinking tool not found"
- **Cause**: `.bob/mcp.json` not deployed to VM
- **Fix**: Upload `.bob/mcp.json` to VM (see commands above)

**Issue**: "npx command not found"
- **Cause**: Node.js not installed on VM
- **Fix**: Install Node.js on VM: `sudo apt-get install -y nodejs npm`

**Issue**: "MCP server failed to start"
- **Cause**: Network issue or package not available
- **Fix**: Test with `npx -y @modelcontextprotocol/server-sequential-thinking --version`

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
- **V2.4** (2026-06-15): **CRITICAL FIX**: Added MANDATORY upload verification step (prevents silent upload failures)
- **V2.5** (2026-06-15): **CRITICAL MANDATE**: Added 100% Completion Mandate (V12.28) - NEVER dismiss epics as "not our concern"
- **V2.6** (2026-06-16): **LOCAL EXECUTION**: Added local execution alternative with PowerShell adaptations for VM failure recovery

## Local Execution Alternative (V2.6)

**When VM Execution Fails**: Use local execution pattern to complete individual epics.

### Use Cases
- VM Phase 5 failures (e.g., method signature mismatch)
- Need to debug phase execution interactively
- Want to verify phase output before VM deployment
- Recovering from VM failures (EPIC-CCN-016 example)

### Pattern: Sequential Phase Execution

**Core Principle**: Execute ONE phase at a time using Bob CLI, mirroring VM script execution exactly.

**Steps**:
1. Read VM script to extract API key and instructions
2. Set `$env:BOBSHELL_API_KEY` with phase-specific key
3. Execute `bob --yolo --chat-mode [mode]` with instructions
4. Verify output files created
5. Move to next phase

**Example** (Phase 1 for EPIC-CCN-016):
```powershell
# 1. Extract API key from scripts/wave4/_p1_016.sh line 10
$env:BOBSHELL_API_KEY='bob_prod_bob-admin_t9tV9...'

# 2. Execute with Bob CLI
bob --yolo --chat-mode plan @"
Execute Phase 1 (Scope Definition) for EPIC-CCN-016.
[Copy full instructions from VM script heredoc]
"@

# 3. Verify output
Get-Item docs/brain/EPIC-CCN-016/01-scope.md | Select-Object Name, Length
```

### PowerShell Adaptations (CRITICAL)

**File I/O Protocol**:
- ❌ NEVER use Bob's `write_to_file`, `read_file`, `run_shell_command` (SSH mode bugs)
- ✅ ALWAYS use `execute_command` with PowerShell heredoc
- ✅ Set `cwd` parameter explicitly

**Command Equivalents**:
```powershell
# File creation
@' content '@ | Out-File -FilePath path/file.md -Encoding UTF8

# File reading
Get-Content path/file.md -Raw

# File verification
Get-Item path/file.md | Select-Object Name, Length

# Method extraction (instead of grep)
$content = Get-Content src/file.cs -Raw
if ($content -match '(?s)MethodName.*?^\s*\}') { $matches[0] }
```

### Success Criteria (Same as VM)
- ✅ Output files created in `docs/brain/EPIC-CCN-XXX/`
- ✅ File sizes >1K (not empty)
- ✅ Build passes (for code-changing phases)
- ✅ Bobcoin usage reported

### Complete Guide
See `building-blocks/autonomous-refactoring/LOCAL_EXECUTION_PATTERN.md` for:
- Phase-by-phase command templates
- PowerShell adaptation examples
- Common pitfalls and solutions
- EPIC-CCN-016 walkthrough (450+ lines)

## Post-Use Audit (MANDATORY)

After every use of this skill:
1. ✅ Check if any instruction was ambiguous
2. ✅ Update this file if gaps found
3. ✅ Document new failure modes in "Common Issues"
4. ✅ Add recovery procedures for new issues
5. ✅ State "skill(gcp-vm-wave-execution): no gaps identified" if no gaps found

**Last Audit**: 2026-06-16 06:18 UTC - **LOCAL EXECUTION PATTERN ADDED (V2.6)**: After EPIC-CCN-016 local completion (Wave 4 final epic), added comprehensive local execution alternative section. Documents sequential phase execution pattern using Bob CLI, PowerShell adaptations for Windows, file I/O protocol for SSH mode, and command equivalents. Enables recovery from VM failures by executing phases locally. Reference: building-blocks/autonomous-refactoring/LOCAL_EXECUTION_PATTERN.md (450+ lines), EPIC-CCN-016 completion

**Previous Audit**: 2026-06-16 04:47 UTC - **FILE ENCODING PROTOCOL ADDED (V12.33)**: After EPIC-CCN-027 TICKET-2 UTF-16 encoding failure (Bob CLI apply_diff achieved 0% similarity), added MANDATORY encoding pre-check to all wave execution workflows. Created comprehensive FILE_ENCODING_PROTOCOL.md (329 lines) and automated check_encoding.ps1 (118 lines). UTF-8 without BOM is now MANDATORY for all source files. Pre-check must run before EVERY phase execution. Encoding violations are P0 blockers. Reference: docs/protocol/FILE_ENCODING_PROTOCOL.md, EPIC-CCN-027 TICKET-2 incident

**Previous Audit**: 2026-06-15 23:31 UTC - **100% COMPLETION MANDATE ADDED (V12.28)**: After Wave 4 EPIC-027/045 incident (dismissed as "not our concern" due to naming mismatch), added ABSOLUTE mandate that ALL epics in scope MUST reach 100% completion. NEVER dismiss any epic without Director approval. If epic exists in roadmap or has brain directory, it IS in scope. Naming mismatches do NOT exempt from completion. Missing Phase 5 does NOT exempt from Phase 6 - execute Phase 5 first. Goal is ALWAYS N/N (100%), never N-1/N. Reference: WAVE4_EPIC_027_045_STATUS.md

**Previous Audit**: 2026-06-15 22:31 UTC - **UPLOAD VERIFICATION ADDED**: After Wave 4 Phase 5 root cause analysis (7 scripts never uploaded to VM), added MANDATORY upload verification step to all phase launch procedures. Verification compares local script count vs VM script count before proceeding. Prevents silent upload failures that cause epic failures. Updated Phase 0 launch example with verification commands. Reference: WAVE4_ROOT_CAUSE_ANALYSIS.md, docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md (V3.1)

**Previous Audit**: 2026-06-14 22:35 UTC - **POLLING PROTOCOL UPDATED**: Changed from 3-minute to 4-minute intervals per user request. Updated V2.0 protocol document (`docs/protocol/COST_OPTIMIZED_POLLING_PROTOCOL_V2.md`) with 91% cost reduction vs 30s polling. Formula: 1 min after first launch, then every 4 min. Updated custom mode (`.bob/custom_modes.yaml`) and monitoring commands to reflect new interval. Reference: docs/protocol/COST_OPTIMIZED_POLLING_PROTOCOL_V2.md

**Previous Audit**: 2026-06-13 05:59 UTC - **CRITICAL SOP ADDED**: After Phase 1 debugging (3 failed launches), created mandatory SOP `docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP.md`. Key rule: ALWAYS copy previous working phase scripts, NEVER generate from scratch. Phase 1 failures were caused by: (1) jq extraction vs hardcoded keys, (2) wrong JSON field `.key` vs `.apikey`, (3) wrong launcher pattern `bash -c` vs `bash -l`. All issues resolved by copying Phase 0 pattern exactly. Updated skill to reference SOP for all future phase script generation.

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