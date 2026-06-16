# Wave Phase Script Generation SOP V3

**Version**: 3.4
**Date**: 2026-06-16
**Status**: MANDATORY
**Supersedes**: V3.3

---

## Critical Update (V3)

**New Rule**: ALWAYS copy the SAME phase from the PREVIOUS wave, NOT adjacent phases from the current wave.

**Violation Discovered**: Wave 3 Phase 3 copied Wave 3 Phase 2 (wrong) instead of Wave 2 Phase 3 (correct).

**Impact**: Wrong execution mode → Wrong output format → 2 failed attempts → 34 minutes debugging → ~5.2 bobcoins wasted.

---

## The Golden Rule

### ALWAYS Copy Same Phase from Previous Wave

```
✅ CORRECT:
Wave 3 Phase 3 → Copy Wave 2 Phase 3
Wave 3 Phase 4 → Copy Wave 2 Phase 4
Wave 3 Phase 5 → Copy Wave 2 Phase 5

❌ WRONG:
Wave 3 Phase 3 → Copy Wave 3 Phase 2
Wave 3 Phase 4 → Copy Wave 3 Phase 3
Wave 3 Phase 5 → Copy Wave 3 Phase 4
```

### Why This Matters

**Each phase has unique requirements**:
- Different execution modes (ask/plan/advanced/v12-engineer)
- Different command patterns
- Different output formats
- Different validation requirements

**Adjacent phases are NOT interchangeable**.

---

## CRITICAL: VM vs Local Building Blocks (V3.3)

### Two Different Script Versions

**IMPORTANT**: There are TWO versions of building-block scripts:

1. **Local Building Blocks** (`scripts/wave{N}/_p{X}_*.sh` in local repo)
   - May have simplified prerequisite checks
   - Example: `if [ ! -f "docs/brain/EPIC-CCN-001/05-completion.md" ]`
   - Used for reference and initial generation

2. **VM Building Blocks** (scripts already on VM from previous waves)
   - Have **robust prerequisite checks** with OR logic
   - Example: `if ! find docs/brain/EPIC-CCN-001 -maxdepth 1 \( -name "05-*.md" -o -name "ticket-*-completion.md" \) -print -quit | grep -q .`
   - **ALWAYS use VM version as source of truth**

### Why This Matters

**Wave 4 Phase 6 Discovery**:
- Local `scripts/wave4/_p6_001.sh` had simple check: `if [ ! -f "docs/brain/EPIC-CCN-001/05-completion.md" ]`
- VM `scripts/wave4/_p6_001.sh` had robust check with OR logic for multiple file patterns
- **Root Cause**: VM scripts evolved during execution, local copies were stale

### Correct Workflow

**Step 1: Check VM for existing scripts**:
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cat ~/universal-or-strategy/scripts/wave{N-1}/_p{X}_001.sh"
```

**Step 2: Copy VM version, NOT local version**:
```bash
# Download VM script as template
gcloud compute scp v12-test-golden-v2:~/universal-or-strategy/scripts/wave{N-1}/_p{X}_001.sh \
  ./scripts/wave{N}/template_p{X}.sh --zone=us-central1-a

# Use this as your building block
```

**Step 3: Generate new scripts from VM template**:
- Use the downloaded VM script as your source
- Modify only epic numbers and API keys
- Preserve ALL logic, especially prerequisite checks

### Common Differences Between VM and Local

| Aspect | Local Version | VM Version (Correct) |
|--------|---------------|---------------------|
| **Prerequisite Check** | Simple file existence | Robust OR logic with find |
| **Error Messages** | Basic | Detailed with expected patterns |
| **Line Endings** | May have CRLF (Windows) | Always LF (Unix) |
| **Permissions** | May not be executable | Always executable |

### Line Ending Fix (MANDATORY)

**After uploading to VM, ALWAYS fix line endings**:
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd ~/universal-or-strategy/scripts/wave{N} && sed -i 's/\r$//' _p{X}_*.sh"
```

**Why**: Windows generates CRLF line endings, VM needs LF. Symptom: `/bin/bash^M: bad interpreter`

---

## Standard Operating Procedure

### Step 0: MANDATORY Encoding Pre-Check (V12.33)

**CRITICAL**: Before generating ANY phase scripts, verify UTF-8 encoding compliance.

**Command**:
```powershell
.\scripts\check_encoding.ps1
```

**Expected Output**:
```
=== File Encoding Validation ===
Scanning: src

=== Results ===
✓ All files use UTF-8 encoding (no BOM)
```

**If Violations Found**:
```powershell
# Automatically convert to UTF-8 without BOM
.\scripts\check_encoding.ps1 -Fix

# Verify fix succeeded
.\scripts\check_encoding.ps1
```

**Why This Matters**:
- UTF-16 encoding causes Bob CLI apply_diff to fail with 0% similarity
- EPIC-CCN-027 TICKET-2: 7+ minutes wasted, epic blocked
- Bob's tools expect UTF-8, cannot handle UTF-16 LE/BE
- Silent failure: No error message, just 0% match

**DO NOT PROCEED** until encoding check passes (exit code 0).

**Reference**: `docs/protocol/FILE_ENCODING_PROTOCOL.md` (V12.33)

### Step 0.5: MANDATORY VM-Local Git Sync (V12.36)

**CRITICAL**: Before generating ANY wave scripts, verify VM and local are on the SAME git commit.

**Why This Matters**:
- Wave 5 Pilot Test Incident: VM on commit `0d28fb4` (Wave 4 work) instead of `dad30745` (post-rollback)
- Impact: Bob saw already-extracted code (CYC=10) instead of baseline code needing extraction
- Result: Wasted execution time, confusion about work status

**Sync Checklist**:

1. **Check Local Git State**:
   ```bash
   git log -1 --oneline
   git status
   # Expected: Clean working tree, no uncommitted src/ changes
   ```

2. **Push Local Commits to Origin**:
   ```bash
   git push origin gitbutler/workspace --force --no-verify
   # Use --no-verify if pre-push validation blocks non-critical issues
   ```

3. **Check VM Git State (Before Sync)**:
   ```bash
   gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
     --command="cd ~/universal-or-strategy && git log -1 --oneline"
   ```

4. **Sync VM to Match Local**:
   ```bash
   gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
     --command="cd ~/universal-or-strategy && git fetch origin && git reset --hard origin/gitbutler/workspace && git clean -fd"
   ```

5. **Verify Sync Succeeded (MANDATORY)**:
   ```bash
   LOCAL_COMMIT=$(git log -1 --format="%H")
   VM_COMMIT=$(gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
     --command="cd ~/universal-or-strategy && git log -1 --format='%H'")
   
   echo "Local: $LOCAL_COMMIT"
   echo "VM:    $VM_COMMIT"
   
   if [ "$LOCAL_COMMIT" = "$VM_COMMIT" ]; then
     echo "✅ SYNC VERIFIED: VM and local on same commit"
   else
     echo "❌ SYNC FAILED: Commits do not match"
     exit 1
   fi
   ```

**BLOCKER**: If commits don't match, STOP and investigate. Do NOT proceed with wave execution.

**Document in Wave Monitoring File**:
```markdown
## Git Sync Verification (V12.36)

**Pre-Wave Sync**:
- Local commit: `<hash>` (`<message>`)
- VM commit (before sync): `<hash>` (`<message>`)
- VM commit (after sync): `<hash>` (`<message>`)
- Sync status: ✅ VERIFIED / ❌ FAILED
```

**Complete Protocol**: `docs/protocol/VM_LOCAL_GIT_SYNC_PROTOCOL.md` (V12.36)

### Step 1: Copy Previous Wave's Same Phase

**Command**:
```bash
cp scripts/wave{N-1}/generate_phase{X}_scripts.py scripts/wave{N}/generate_wave{N}_phase{X}_scripts.py
```

**Example** (Wave 3 Phase 4):
```bash
cp scripts/wave2/generate_phase4_scripts.py scripts/wave3/generate_wave3_phase4_scripts.py
```

**DO NOT**:
- Copy adjacent phase from current wave
- Generate from scratch
- Assume patterns are similar

### Step 2: Update Epic Numbers ONLY

**Change ONLY these lines**:
```python
# Epic to API key mapping (Wave 3: CCN-116 through CCN-125)
API_ALLOCATION = {
    "116": "b (2).json",  # Was "107"
    "117": "b.json",      # Was "108"
    "118": "bob (1).json", # Was "109"
    # ... etc
}
```

**DO NOT change**:
- Mode (ask/plan/advanced/v12-engineer)
- Command pattern
- Output format
- Validation requirements

### Step 3: Verify Against SOP

**Check these 4 things**:

1. **Mode matches SOP**:
   ```bash
   grep "chat-mode" scripts/wave{N}/generate_wave{N}_phase{X}_scripts.py
   ```

2. **Command pattern matches SOP**:
   ```bash
   grep "bob --" scripts/wave{N}/generate_wave{N}_phase{X}_scripts.py
   ```

3. **Output format matches SOP**:
   ```bash
   grep "0{X}-" scripts/wave{N}/generate_wave{N}_phase{X}_scripts.py
   ```

4. **Validation requirements match SOP**:
   - Check prompt includes required checks
   - Check manifest update logic
   - Check bobcoin reporting

### Step 4: Upload Scripts to VM

**Upload all scripts**:
```bash
gcloud compute scp scripts/wave{N}/_p{X}_*.sh v12-test-golden-v2:~/universal-or-strategy/scripts/wave{N}/ --zone=us-central1-a
```

**Set permissions**:
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="chmod +x ~/universal-or-strategy/scripts/wave{N}/_p{X}_*.sh"
```

### Step 5: MANDATORY Upload Verification

**CRITICAL**: Always verify ALL scripts uploaded successfully before proceeding.

**Count expected scripts**:
```bash
# Count local scripts
ls scripts/wave{N}/_p{X}_*.sh | wc -l
```

**Verify on VM**:
```bash
# Count VM scripts
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="ls ~/universal-or-strategy/scripts/wave{N}/_p{X}_*.sh | wc -l"
```

**Compare counts**:
```bash
# If counts don't match, STOP and investigate
LOCAL_COUNT=$(ls scripts/wave{N}/_p{X}_*.sh | wc -l)
VM_COUNT=$(gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="ls ~/universal-or-strategy/scripts/wave{N}/_p{X}_*.sh | wc -l")

if [ "$LOCAL_COUNT" != "$VM_COUNT" ]; then
    echo "ERROR: Upload incomplete. Local: $LOCAL_COUNT, VM: $VM_COUNT"
    exit 1
fi
```

**Why This Matters**:
- Wave 4 Phase 5: 7 scripts never uploaded → 7 epics failed
- Silent failure: No error message, scripts just missing
- Cost: 1-2 hours recovery time + debugging effort

**DO NOT PROCEED** until counts match exactly.

### Step 6: Test with 2 Epics (Pilot Test)

**Run pilot test**:
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd universal-or-strategy && ./scripts/wave{N}/_p{X}_116.sh"
```

**Verify output format**:
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="ls -lh docs/brain/EPIC-CCN-116/0{X}-*.md"
```

**Deploy all only after pilot success**.

### Step 7: Document Any Deviations

**If pattern must change**:
1. Create `WAVE{N}_PHASE{X}_DEVIATION.md`
2. Document why pattern changed
3. Update this SOP with new pattern
4. Verify with Director before proceeding

---

## Phase-Specific Requirements

### Phase 0 (Hotspot Analysis)
- **Mode**: `ask`
- **Command**: `bob --yolo --chat-mode ask "$(cat /tmp/phase0_msg_X.txt)"`
- **Output**: `00-hotspots.md`, `manifest.json`
- **Validation**: jCodemunch hotspot data

### Phase 1 (Scope Definition)
- **Mode**: `plan`
- **Command**: `bob --yolo --chat-mode plan "$(cat /tmp/phase1_msg_X.txt)"`
- **Output**: `00-scope.md`
- **Validation**: Single-method boundary

### Phase 2 (Architecture Planning)
- **Mode**: `plan`
- **Command**: `bob --yolo --chat-mode plan "$(cat /tmp/phase2_msg_X.txt)"`
- **Output**: `02-architecture-plan.md`, `02-diagrams.mmd`
- **Validation**: Jane Street alignment

### Phase 3 (DNA & PR Audit)
- **Mode**: `advanced`
- **Command**: `bob --yolo --chat-mode advanced "$(cat /tmp/phase3_msg_X.txt)"`
- **Output**: `03-audit-report.md`
- **Validation**: DNA compliance, PR hygiene

### Phase 4 (Ticket Generation)
- **Mode**: `plan`
- **Command**: `bob --yolo --chat-mode plan "$(cat /tmp/phase4_msg_X.txt)"`
- **Output**: `04-tickets.md`
- **Validation**: Ticket breakdown, execution order

### Phase 5 (Ticket Execution)
- **Mode**: `v12-engineer`
- **Command**: `bob --yolo --chat-mode v12-engineer "$(cat /tmp/phase5_msg_X.txt)"`
- **Output**: `ticket-X-completion.md`
- **Validation**: Build passes, tests pass
- **Test Framework**: xUnit ONLY (project uses xUnit 2.9.0+)
  - ✅ Use: `[Fact]`, `Assert.Equal()`, `Assert.NotNull()`, `Assert.True()`
  - ❌ NEVER use: NUnit (`[Test]`, `[TestFixture]`, `Assert.AreEqual()`, `Assert.IsNotNull()`)
  - ❌ NEVER use: MSTest (`[TestMethod]`, `[TestClass]`)
  - **Rationale**: EPIC-027 TICKET-1 generated NUnit tests → 29 compilation errors → manual conversion required

### Phase 6 (Final Review)
- **Mode**: `advanced`
- **Command**: `bob --yolo --chat-mode advanced "$(cat /tmp/phase6_msg_X.txt)"`
- **Output**: `05-completion-report.md`
- **Validation**: All tickets verified, roadmap updated

---

## Common Mistakes

### Mistake 1: Copying Adjacent Phase

**Wrong**:
```bash
# Copying Wave 3 Phase 2 for Wave 3 Phase 3
cp scripts/wave3/generate_wave3_phase2_scripts.py scripts/wave3/generate_wave3_phase3_scripts.py
```

**Right**:
```bash
# Copying Wave 2 Phase 3 for Wave 3 Phase 3
cp scripts/wave2/generate_phase3_scripts.py scripts/wave3/generate_wave3_phase3_scripts.py
```

### Mistake 2: Changing Mode

**Wrong**:
```python
# Changing mode from 'advanced' to 'plan'
bob --yolo --chat-mode plan "$(cat /tmp/phase3_msg_X.txt)"
```

**Right**:
```python
# Keeping mode as 'advanced' (from Wave 2 Phase 3)
bob --yolo --chat-mode advanced "$(cat /tmp/phase3_msg_X.txt)"
```

### Mistake 3: Skipping Test

**Wrong**:
```bash
# Deploying all 10 scripts without testing
gcloud compute scp _p3_*.sh v12-test-golden-v2:~/universal-or-strategy/
```

**Right**:
```bash
# Testing 2 scripts first
gcloud compute scp _p3_116.sh _p3_117.sh v12-test-golden-v2:~/universal-or-strategy/
# Verify output format
# Deploy all only after success
```

---

## Verification Checklist

Before deploying any phase scripts, verify:

- [ ] **ENCODING PRE-CHECK PASSED** (Step 0 - V12.33 MANDATORY)
- [ ] **VM-LOCAL GIT SYNC VERIFIED** (Step 0.5 - V12.36 MANDATORY)
- [ ] Copied from previous wave's SAME phase (not adjacent phase)
- [ ] Updated epic numbers only (107-115 → 116-125)
- [ ] Mode matches SOP (ask/plan/advanced/v12-engineer)
- [ ] Command pattern matches SOP
- [ ] Output format matches SOP (0X-*.md)
- [ ] Validation requirements match SOP
- [ ] **UPLOADED ALL SCRIPTS TO VM** (Step 4)
- [ ] **VERIFIED SCRIPT COUNT MATCHES** (Step 5 - MANDATORY)
- [ ] Tested with 2 epics first (pilot test)
- [ ] Output format verified
- [ ] Ready to deploy all

---

## Recovery Procedure

If wrong output format detected:

1. **STOP immediately** - Do not deploy remaining scripts
2. **Identify root cause** - Check which phase was copied
3. **Create corrected generator** - Copy correct phase from previous wave
4. **Test with 2 epics** - Verify output format
5. **Deploy all** - Only after success
6. **Document failure** - Update lessons learned

---

## Success Metrics

### Per Phase
- ✅ All scripts generated without errors
- ✅ All scripts use correct mode
- ✅ All scripts produce correct output format
- ✅ All scripts complete within budget
- ✅ All APIs remain positive

### Per Wave
- ✅ All phases follow SOP
- ✅ No architecture bugs
- ✅ No wrong output formats
- ✅ Budget maintained (>80% remaining)

---

## Version History

### V3.5 (2026-06-16)
- **Added**: MANDATORY VM-Local Git Sync (Step 0.5) before script generation
- **Added**: Git sync verification checklist (5 steps)
- **Updated**: Verification checklist with git sync requirement
- **Reason**: Wave 5 pilot test incident (VM on old commit, Bob saw extracted code)
- **Reference**: docs/protocol/VM_LOCAL_GIT_SYNC_PROTOCOL.md (V12.36)

### V3.4 (2026-06-16)
- **Added**: Local execution alternative section with PowerShell adaptations
- **Added**: File I/O protocol for SSH/non-interactive mode
- **Added**: PowerShell command equivalents for grep, cat, ls
- **Updated**: References to include LOCAL_EXECUTION_PATTERN.md
- **Reason**: EPIC-CCN-016 local completion after VM Phase 5 failure
- **Reference**: building-blocks/autonomous-refactoring/LOCAL_EXECUTION_PATTERN.md

### V3.3 (2026-06-16)
- **Added**: MANDATORY encoding pre-check (Step 0) before script generation
- **Added**: UTF-8 compliance verification using check_encoding.ps1
- **Updated**: Verification checklist with encoding pre-check requirement
- **Reason**: EPIC-CCN-027 TICKET-2 UTF-16 encoding failure (Bob CLI 0% similarity)
- **Reference**: docs/protocol/FILE_ENCODING_PROTOCOL.md (V12.33)

### V3.2 (2026-06-16)
- **Added**: xUnit framework requirement for Phase 5 (Ticket Execution)
- **Added**: Test framework validation (xUnit ONLY, no NUnit/MSTest)
- **Updated**: Phase 5 requirements with explicit framework constraints
- **Reason**: EPIC-027 TICKET-1 NUnit mismatch (29 errors, manual conversion)

### V3.1 (2026-06-15)
- **Added**: MANDATORY upload verification step (Step 5)
- **Added**: Script count comparison protocol
- **Updated**: Verification checklist with upload verification
- **Reason**: Wave 4 Phase 5 failures (7 scripts never uploaded to VM)

### V3.0 (2026-06-13)
- **Added**: Golden Rule (always copy same phase from previous wave)
- **Added**: Common mistakes section
- **Added**: Recovery procedure
- **Reason**: Wave 3 Phase 3 architecture bug (copied adjacent phase)

### V2.0 (2026-06-12)
- **Added**: Test with 2 epics before full deployment
- **Added**: Verification checklist
- **Reason**: Wave 3 Phase 1 failures (3 attempts)

### V1.0 (2026-06-11)
- **Initial**: Basic script generation procedure
- **Reason**: Wave 2 Phase 0 success

---

## Local Execution Alternative (V3.4)

**When to Use Local Execution**:
- VM execution failed for specific epics
- Need to debug phase execution interactively
- Want to verify phase output before VM deployment
- Recovering from VM failures (e.g., EPIC-CCN-016)

**Pattern**: Execute phases locally using Bob CLI, one at a time, mirroring VM script execution.

### Local Execution Steps

**1. Extract Phase Script from VM**:
```powershell
# Read the VM script to get API key and instructions
Get-Content scripts/wave4/_p1_016.sh
```

**2. Set API Key**:
```powershell
# Extract API key from line 10 of VM script
$env:BOBSHELL_API_KEY='bob_prod_bob-admin_...'
```

**3. Execute Phase with Bob CLI**:
```powershell
# Use same mode and instructions as VM script
bob --yolo --chat-mode plan @"
[Copy instructions from VM script's heredoc]
"@
```

**4. Verify Output**:
```powershell
# Check files created
Get-Item docs/brain/EPIC-CCN-XXX/0X-*.md | Select-Object Name, Length
```

**5. Repeat for Next Phase**:
```powershell
# Move to next phase script (_p2_016.sh)
# Extract new API key
# Execute with Bob CLI
```

### PowerShell Adaptations

**File I/O Protocol** (CRITICAL):
- ❌ NEVER use Bob's `write_to_file`, `read_file`, `run_shell_command` in SSH mode
- ✅ ALWAYS use `execute_command` with PowerShell heredoc syntax
- ✅ Set `cwd` parameter explicitly for directory-specific commands

**File Creation**:
```powershell
# WRONG (Bob's write_to_file fails in SSH mode)
write_to_file("path/file.md", "content")

# CORRECT (PowerShell heredoc)
@'
content here
'@ | Out-File -FilePath path/file.md -Encoding UTF8
```

**File Reading**:
```powershell
# WRONG (Bob's read_file fails in SSH mode)
read_file("path/file.md")

# CORRECT (PowerShell)
Get-Content path/file.md -Raw
```

**Method Extraction**:
```powershell
# WRONG (grep doesn't exist on Windows)
grep -A 50 "TryHandleFleet_CancelAll" src/file.cs

# CORRECT (PowerShell regex)
$content = Get-Content src/file.cs -Raw
if ($content -match '(?s)TryHandleFleet_CancelAll.*?^\s*\}') {
    $matches[0]
}
```

### Success Criteria (Same as VM)

**Per Phase**:
- ✅ Output files created in `docs/brain/EPIC-CCN-XXX/`
- ✅ File sizes >1K (not empty)
- ✅ Build passes (for code-changing phases)
- ✅ Bobcoin usage reported

**Complete Walkthrough**: See `building-blocks/autonomous-refactoring/LOCAL_EXECUTION_PATTERN.md`

---

## References

- **Wave 3 Phase 3 Bug**: `WAVE3_PHASE3_ARCHITECTURE_BUG_ANALYSIS.md`
- **Lessons Learned**: `building-blocks/autonomous-refactoring/WAVE3_PHASE3_LESSONS_LEARNED.md`
- **Complete Handoff**: `WAVE3_PHASE3_COMPLETE_HANDOFF.md`
- **Local Execution Pattern**: `building-blocks/autonomous-refactoring/LOCAL_EXECUTION_PATTERN.md` (V1.0)

---

**MANDATORY COMPLIANCE**: All agents MUST follow this SOP for all phase script generation.

**Violation Consequences**: Wrong output format, wasted bobcoins, debugging time, architecture rewrites.

**Next Update**: After Wave 4 completion.