# Wave 5 Pilot Test - Continuation Prompt

**Copy and paste this entire prompt into your new Claude session:**

---

# Context: Wave 4 Rollback Complete, Wave 5 Pilot Test Ready

You are continuing from a completed rollback session. All Wave 4 Phase 5-6 files have been deleted, protocols have been hardened, and the VM is running. Now execute the Wave 5 pilot test on EPIC-CCN-001.

## Current State Summary

### What's Done ✅
1. **Wave 4 Rollback**: 7 PRs closed, 162 files deleted (78 epics)
2. **Protocol Hardening**: 3 new protocol files created, 2 MCP servers updated
3. **Commits**: 
   - `7ef25a35`: Rollback (gitbutler/workspace)
   - `dad30745`: Protocol hardening (gitbutler/workspace)
4. **VM Status**: RUNNING (v12-test-golden-v2, 34.69.75.104)

### Protocol Files Created (V12.34-V12.35)
1. **PHASE5_EXECUTION_PROTOCOL.md** (157 lines)
   - SURGICAL ONLY mandate
   - Absolute prohibitions (no pre-existing fixes, no adjacent changes)
   - Execution checklist

2. **PHASE5V_VERIFICATION_PROTOCOL.md** (298 lines)
   - 5 mandatory checks (compilation, complexity ≤8, scope, tests, encoding)
   - Verification report template
   - Failure scenarios

3. **LOCAL_EXECUTION_PROTOCOL.md** (298 lines)
   - Encoding-sensitive file detection
   - Local execution process
   - Pattern-based special cases

### MCP Servers Updated
1. **phase_5_execute_mcp.py**: Added SURGICAL ONLY mandate to prompts
2. **phase_5_verify_mcp.py**: Added 5-check protocol to prompts

### Branch Strategy (CRITICAL)
- **Local**: gitbutler/workspace (protocol work committed here)
- **VM**: main (VM always works on main branch)
- **Sync**: Changes must be synced from VM main → local gitbutler/workspace

## Your Mission: Execute Wave 5 Pilot Test (EPIC-CCN-001)

### Step 1: Find Wave 4 Phase 5 Script Template on VM

**Command**:
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cat ~/universal-or-strategy/scripts/wave4/_p5_001.sh"
```

**What to Extract**:
- API key (line ~10)
- Bob CLI command pattern
- Prompt instructions (heredoc section)
- Output file expectations

**Save to**: `scripts/wave5/template_p5.sh` (for reference)

---

### Step 2: Generate Phase 5 Script for EPIC-CCN-001

**Building-Blocks Method** (MANDATORY):
1. Copy Wave 4 Phase 5 script (SAME phase from PREVIOUS wave)
2. Modify ONLY epic number (001 stays 001 for pilot)
3. Update API key (use same API as Wave 4 EPIC-CCN-001)
4. Preserve ALL logic (especially prerequisite checks)

**Script Name**: `scripts/wave5/_p5_001.sh`

**Key Changes**:
```bash
# Line 10: API key (extract from Wave 4 script)
export BOBSHELL_API_KEY='bob_prod_bob-admin_...'

# Line 15: Epic ID (already 001, no change needed)
EPIC_ID="EPIC-CCN-001"

# Line 20: Wave number (update to wave5)
LOG_FILE="logs/wave5/EPIC-CCN-001.log"
```

**DO NOT CHANGE**:
- Mode: `v12-engineer`
- Command pattern: `bob --yolo --chat-mode v12-engineer`
- Output format: `ticket-X-completion.md`
- Prerequisite checks (robust OR logic with find)

---

### Step 3: Upload Script to VM and Verify Upload

**Upload**:
```bash
# Create wave5 directory on VM
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="mkdir -p ~/universal-or-strategy/scripts/wave5"

# Upload script
gcloud compute scp scripts/wave5/_p5_001.sh \
  v12-test-golden-v2:~/universal-or-strategy/scripts/wave5/ \
  --zone=us-central1-a

# Set permissions
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="chmod +x ~/universal-or-strategy/scripts/wave5/_p5_001.sh"

# Fix line endings (MANDATORY)
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd ~/universal-or-strategy/scripts/wave5 && sed -i 's/\r$//' _p5_001.sh"
```

**MANDATORY Upload Verification**:
```bash
# Count local scripts
ls scripts/wave5/_p5_*.sh | wc -l
# Expected: 1

# Count VM scripts
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="ls ~/universal-or-strategy/scripts/wave5/_p5_*.sh | wc -l"
# Expected: 1

# If counts don't match, STOP and investigate
```

**Why This Matters**: Wave 4 Phase 5 had 7 scripts never upload → 7 epics failed silently

---

### Step 4: Execute Pilot Test on VM

**Launch**:
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd ~/universal-or-strategy && screen -dmS p5-001 bash -l -c './scripts/wave5/_p5_001.sh 2>&1 | tee logs/wave5/EPIC-CCN-001.log'"
```

**Verify Launch**:
```bash
# Check screen session exists
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="screen -ls"
# Expected: p5-001 session running
```

---

### Step 5: Monitor Execution (4-Minute Polling)

**Initial Check** (1 minute after launch):
```bash
# Check if screen session still running
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="screen -ls"
```

**Subsequent Checks** (every 4 minutes):
```bash
# Check screen sessions
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="screen -ls"

# Count ticket completion files
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="ls ~/universal-or-strategy/docs/brain/EPIC-CCN-001/ticket-*-completion.md 2>/dev/null | wc -l"

# Extract bobcoin usage
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="grep -E 'Cost:.*Balance:|Cost: [0-9]' ~/universal-or-strategy/logs/wave5/EPIC-CCN-001.log | tail -5"
```

**Success Indicators**:
- Screen session completes (no longer in `screen -ls`)
- Ticket completion files created
- Bobcoin usage reported in logs

**Estimated Duration**: 15-30 minutes (Phase 5 typically 5-10 bobcoins)

---

### Step 6: Sync to Local and Verify 5 Checks

**Sync from VM to Local**:
```bash
# Download ticket completion files
gcloud compute scp \
  v12-test-golden-v2:~/universal-or-strategy/docs/brain/EPIC-CCN-001/ticket-*-completion.md \
  ./docs/brain/EPIC-CCN-001/ \
  --zone=us-central1-a

# Download any modified source files
gcloud compute scp \
  v12-test-golden-v2:~/universal-or-strategy/src/V12_002.*.cs \
  ./src/ \
  --zone=us-central1-a
```

**Run 5 Mandatory Checks Locally**:

#### ✅ Check 1: Compilation
```powershell
dotnet build
# Expected: Exit code 0, zero errors
```

#### ✅ Check 2: Complexity Reduction
```bash
python scripts/complexity_audit.py
# Expected: Target method CYC ≤8
```

#### ✅ Check 3: Scope Compliance
```bash
git diff HEAD~1 --stat
# Expected: ONLY target file modified, no adjacent changes
```

#### ✅ Check 4: Test Coverage
```powershell
dotnet test
# Expected: xUnit tests exist and pass
```

#### ✅ Check 5: Encoding Compliance
```powershell
powershell -File .\scripts\check_encoding.ps1
# Expected: Exit code 0, all files UTF-8 without BOM
```

**Document Results**:
Create `docs/brain/EPIC-CCN-001/06-verification-report.md` with all 5 check results.

---

### Step 7: Create Pilot PR and Run Greptile Audit

**Create PR**:
```bash
# Commit changes locally
git add docs/brain/EPIC-CCN-001/ src/
git commit -m "feat(EPIC-CCN-001): Wave 5 pilot test - Phase 5 execution

- Extracted: [method names]
- CYC: [before] → [after]
- Tests: [count] xUnit tests passing
- Encoding: UTF-8 verified

Wave 5 pilot test with hardened protocols (V12.34-V12.35)"

# Push to remote
git push origin gitbutler/workspace

# Create PR
gh pr create --title "Wave 5 Pilot: EPIC-CCN-001" \
  --body "Pilot test for Wave 5 with hardened protocols. See verification report." \
  --base main
```

**Run Greptile Audit**:
```bash
# Get PR number
gh pr list --state open

# Use Greptile MCP to audit PR
# (Use greptile MCP tools to analyze PR for issues)
```

**Success Criteria**:
- ✅ 0 P0 issues
- ✅ 0 P1 issues
- ✅ All 5 checks pass
- ✅ Greptile verdict: CLEAN

---

### Step 8: Document Pilot Results

Create `WAVE5_PILOT_TEST_RESULTS.md`:

```markdown
# Wave 5 Pilot Test Results: EPIC-CCN-001

## Execution Summary
- **Duration**: [X] minutes
- **Bobcoins Used**: [X.XX]
- **Screen Session**: Completed successfully
- **Files Created**: [count] ticket completion files

## 5-Check Verification
1. ✅/❌ Compilation: [result]
2. ✅/❌ Complexity: [before] → [after] (target ≤8)
3. ✅/❌ Scope: [files modified]
4. ✅/❌ Tests: [count] xUnit tests passing
5. ✅/❌ Encoding: UTF-8 verified

## Greptile Audit
- **P0 Issues**: [count]
- **P1 Issues**: [count]
- **P2 Issues**: [count]
- **Verdict**: CLEAN / ISSUES FOUND

## Decision
- ✅ **PROCEED**: Launch full Wave 5 (77 epics on VM, 1 local)
- ❌ **ITERATE**: Fix issues, re-run pilot

## Next Steps
[If PROCEED]: Generate Wave 5 scripts for all 78 epics
[If ITERATE]: Document issues, update protocols, re-test
```

---

## Critical Protocols (MANDATORY)

### Building-Blocks Method
- ✅ ALWAYS copy SAME phase from PREVIOUS wave
- ❌ NEVER copy adjacent phase from current wave
- ❌ NEVER generate scripts from scratch
- ✅ Use find-and-replace for phase-specific parameters only

**Reference**: `docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md`

### SURGICAL ONLY Mandate (V12.34)
- ✅ Touch ONLY target method
- ❌ NO pre-existing error fixes
- ❌ NO adjacent method improvements
- ❌ NO "while we're here" changes

**Reference**: `docs/protocol/PHASE5_EXECUTION_PROTOCOL.md`

### 5-Check Protocol (V12.34)
1. Compilation (dotnet build)
2. Complexity ≤8 (complexity_audit.py)
3. Scope compliance (git diff --stat)
4. Tests (dotnet test, xUnit only)
5. Encoding (check_encoding.ps1)

**Reference**: `docs/protocol/PHASE5V_VERIFICATION_PROTOCOL.md`

### Upload Verification (V12.27)
- ✅ ALWAYS verify script count matches (local vs VM)
- ❌ NEVER proceed with mismatched counts
- **Why**: Wave 4 Phase 5 had 7 scripts never upload → 7 epics failed

### Cost-Optimized Polling (V12.26)
- Initial check: 1 minute after FIRST script launch
- Subsequent checks: Every 4 minutes
- **Why**: 91% cost reduction vs 30s polling

**Reference**: `docs/protocol/COST_OPTIMIZED_POLLING_PROTOCOL_V2.md`

---

## Key Documents (Reference)

**Analysis & Planning**:
- `WAVE4_PROTOCOL_HARDENING_PLAN.md` (1000 lines, 7 protocols)
- `WAVE4_ROLLBACK_SCOPE_FINAL.md` (400 lines, epic breakdown)
- `WAVE4_FULL_PR_AUDIT.md` (28 issues catalogued)

**Protocols**:
- `docs/protocol/PHASE5_EXECUTION_PROTOCOL.md` (V12.34)
- `docs/protocol/PHASE5V_VERIFICATION_PROTOCOL.md` (V12.34)
- `docs/protocol/LOCAL_EXECUTION_PROTOCOL.md` (V12.35)
- `docs/protocol/SPECIAL_CASE_DETECTION_PROTOCOL.md` (pattern detection)
- `docs/protocol/RECOVERY_LOOP_PROTOCOL.md` (V12.26)
- `docs/protocol/FILE_ENCODING_PROTOCOL.md` (V12.33)

**Building-Blocks**:
- `docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md` (MANDATORY)
- `.bob/skills/gcp-vm-wave-execution/skill.md` (complete workflow)
- `building-blocks/autonomous-refactoring/` (templates)

---

## VM Configuration

- **Instance**: v12-test-golden-v2
- **Zone**: us-central1-a
- **Type**: n2-standard-8 (8 vCPU, 32 GB RAM)
- **Status**: RUNNING
- **External IP**: 34.69.75.104
- **Branch**: main (VM always works on main)
- **Local Branch**: gitbutler/workspace (protocol work here)

---

## Success Criteria

### Pilot Test Complete
- ✅ Script generated using building-blocks method
- ✅ Script uploaded and verified
- ✅ Execution completed successfully
- ✅ All 5 checks pass
- ✅ Greptile audit: 0 P0/P1 issues
- ✅ Results documented

### Ready for Full Wave 5
- ✅ Pilot test passed
- ✅ 0 Greptile issues
- ✅ All protocols validated
- ✅ Building-blocks method confirmed working
- ✅ Director approval to proceed

---

## Your First Actions

1. **Verify VM accessible**:
   ```bash
   gcloud compute instances list --filter="name=v12-test-golden-v2"
   # Expected: STATUS = RUNNING
   ```

2. **Find Wave 4 Phase 5 script** (Step 1 above)

3. **Generate Wave 5 Phase 5 script** (Step 2 above)

4. **Upload and verify** (Step 3 above)

5. **Execute pilot test** (Step 4 above)

6. **Monitor execution** (Step 5 above)

7. **Verify 5 checks** (Step 6 above)

8. **Document results** (Step 8 above)

---

**Ready to execute Wave 5 pilot test!** Follow the 8 steps systematically. Good luck! 🚀

---

**Session Context Version**: 3.0 (Wave 5 Pilot Test)
**Last Updated**: 2026-06-16T21:22:00Z
**Maintainer**: Wave 4 Rollback Lead
**Status**: 🟢 READY FOR PILOT TEST EXECUTION