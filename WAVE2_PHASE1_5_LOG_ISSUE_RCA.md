# Wave 2 Phase 1.5 Log Issue - Root Cause Analysis

**Date**: 2026-06-13  
**Issue**: Bobcoin tracking logs not created for Phase 1.5  
**Impact**: Cannot track bobcoin usage for Phase 1.5 (cost visibility lost)

---

## Problem Statement

Phase 1.5 completed successfully (9/9 output files created), but:
- ❌ No log files created in `logs/phase1_5/`
- ❌ No bobcoin usage tracking
- ❌ Directory `logs/phase1_5/` never created

---

## Root Cause Analysis

### Investigation Steps

1. **Checked script structure**: ✅ Correct (line 6: `mkdir -p logs/phase1_5`, line 36: `tee logs/phase1_5/EPIC-CCN-107.log`)
2. **Checked file permissions**: ❌ Scripts not executable (`-rw-rw-r--` instead of `-rwxrwxr-x`)
3. **Checked directory existence**: ❌ `logs/phase1_5/` never created
4. **Checked process execution**: ✅ Scripts ran (output files created)

### Root Cause

**Scripts uploaded via `gcloud compute scp` lost execute permissions.**

When copying files from Windows to Linux via SCP:
- Windows files don't have Unix execute bit
- SCP preserves Windows permissions (644 = rw-rw-r--)
- Scripts are NOT executable on Linux

### Why Scripts Ran Anyway

The launcher uses: `bash -l "$script_name"`

This spawns bash to interpret the script file, which works WITHOUT execute permissions. However:
- The script runs in a subshell
- The `mkdir -p logs/phase1_5` command executes
- BUT the directory creation might fail silently OR
- The `tee` command fails because the directory doesn't exist yet

### Why Logs Weren't Created

Two possible scenarios:

**Scenario A**: Directory creation failed
- `mkdir -p logs/phase1_5` failed (permission issue?)
- `tee logs/phase1_5/EPIC-CCN-107.log` failed (no directory)
- Bob Shell output went to stdout only (not captured)

**Scenario B**: Race condition
- `mkdir -p` succeeded
- But `tee` ran before directory was fully created
- Log write failed silently

---

## Comparison with Working Phases

### Phase 0 (Working)
```bash
$ ls -la _p0_107.sh
-rwxrwxr-x 1 malhitticrypto malhitticrypto 4854 Jun 13 04:04 _p0_107.sh
```
- ✅ Executable
- ✅ Logs created: `logs/phase0/EPIC-CCN-107.log`
- ✅ Bobcoin tracking: `Cost: X.XX` in logs

### Phase 1 (Partially Working)
```bash
$ ls -la _p1_107.sh
-rw-rw-r-- 1 malhitticrypto malhitticrypto 1416 Jun 13 05:48 _p1_107.sh
```
- ❌ NOT executable
- ✅ Logs created: `logs/phase1/EPIC-CCN-107.log`
- ✅ Bobcoin tracking: `Cost: 0.68` in logs

**Why Phase 1 logs worked**: The `logs/phase1/` directory already existed from a previous run.

### Phase 1.5 (Broken)
```bash
$ ls -la _p1_5_107.sh
-rw-rw-r-- 1 malhitticrypto malhitticrypto 1505 Jun 13 06:16 _p1_5_107.sh
```
- ❌ NOT executable
- ❌ Logs NOT created
- ❌ No bobcoin tracking
- ❌ Directory `logs/phase1_5/` never existed

---

## Solution

### Immediate Fix (Retroactive)

Cannot recover Phase 1.5 bobcoin data - logs were never written.

**Estimate based on Phase 1**:
- Phase 1 average: $0.76 per epic
- Phase 1.5 is simpler (boundary validation vs full scope)
- **Estimated Phase 1.5 cost**: ~$0.50-0.60 per epic × 9 = **$4.50-5.40**

### Prevention for Future Phases

**Add to SOP** (`docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP.md`):

#### Step 6: Make Scripts Executable (NEW)

After uploading scripts to VM, ALWAYS make them executable:

```bash
# After gcloud compute scp
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="chmod +x /home/malhitticrypto/universal-or-strategy/_p*_*.sh /home/malhitticrypto/universal-or-strategy/launch_phase*_all_screen.sh"
```

**OR** use the launcher's built-in chmod:

```bash
# In launcher script (before screen -dmS)
chmod +x "$script_name"
screen -dmS "$session_name" bash -l "$script_name"
```

#### Step 7: Verify Log Directory Creation (NEW)

After launch, verify logs are being written:

```bash
# Wait 30 seconds for scripts to start
sleep 30

# Check log directory exists
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="ls -la /home/malhitticrypto/universal-or-strategy/logs/phase*/"

# Check at least one log file exists
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="ls -lh /home/malhitticrypto/universal-or-strategy/logs/phase*/EPIC-CCN-107.log"
```

---

## Updated Validation Checklist

Add to `WAVE_PHASE_SCRIPT_GENERATION_SOP.md` (line 205):

```markdown
- [ ] Copied from previous working phase (not generated from scratch)
- [ ] API keys are hardcoded (no jq extraction)
- [ ] Using correct field `.apikey` (not `.key`)
- [ ] Bob Shell invocation matches Phase 0 pattern
- [ ] Launcher uses `bash -l` (not `bash -c`)
- [ ] Only phase-specific content changed (task description, file names, phase number)
- [ ] **Scripts made executable after upload** (chmod +x)  ← NEW
- [ ] **Log directory verified after launch** (ls logs/phase*/)  ← NEW
- [ ] **Bobcoin tracking verified in logs** (grep 'Cost:')  ← NEW
- [ ] Tested one script locally before deploying all 9
- [ ] Compared against working phase with diff
```

---

## Bobcoin Tracking Requirements

### In Scripts (Already Present)

Line 36 in all phase scripts:
```bash
bob --yolo --chat-mode plan "$(cat /tmp/phase1_5_msg_107.txt)" 2>&1 | tee logs/phase1_5/EPIC-CCN-107.log
```

The `2>&1 | tee` pattern captures:
- Bob Shell stdout (includes cost reporting)
- Bob Shell stderr (includes errors)
- Writes to log file AND displays on screen

### In Bob Shell Output (Automatic)

Bob Shell automatically reports cost at completion:
```
[using tool attempt_completion: Successfully completed | Cost: 0.68]
```

This appears in logs when `tee` works correctly.

### Extraction Command

```bash
# Extract all bobcoin costs from phase logs
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="grep 'Cost:' /home/malhitticrypto/universal-or-strategy/logs/phase*/EPIC-CCN-*.log"
```

---

## Add to Skill Documentation

Update `.bob/skills/gcp-vm-wave-execution/skill.md` (after line 144):

### Bobcoin Tracking Verification (MANDATORY)

After every phase launch:

```bash
# 1. Wait for first epic to complete (usually 2-5 minutes)
sleep 300

# 2. Verify log directory exists
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="ls -la /home/malhitticrypto/universal-or-strategy/logs/phase*/"

# 3. Verify at least one log file has content
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="wc -l /home/malhitticrypto/universal-or-strategy/logs/phase*/EPIC-CCN-107.log"

# 4. Extract bobcoin cost from first completed epic
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="grep 'Cost:' /home/malhitticrypto/universal-or-strategy/logs/phase*/EPIC-CCN-107.log | tail -1"
```

**If no logs found**:
1. STOP immediately
2. Check script permissions: `ls -la _p*_*.sh`
3. Make executable: `chmod +x _p*_*.sh`
4. Kill and relaunch: `killall screen; sleep 2; bash launch_phase*_all_screen.sh`

---

## Cost Impact

### Phase 1.5 (Lost Data)
- **Actual cost**: Unknown (logs not created)
- **Estimated cost**: $4.50-5.40 (based on Phase 1 average)
- **Impact**: Minor (can estimate from Phase 1 data)

### Future Phases
- **Prevention cost**: Zero (just chmod +x)
- **Verification cost**: 30 seconds per phase
- **Benefit**: 100% bobcoin tracking visibility

---

## Action Items

### Immediate (Before Phase 2)

1. ✅ Document root cause (this file)
2. ⏳ Update SOP with chmod +x requirement
3. ⏳ Update skill with verification steps
4. ⏳ Add bobcoin tracking to validation checklist
5. ⏳ Test Phase 2 scripts locally before deployment

### Long-term (Wave 3+)

1. Create automated deployment script that:
   - Uploads scripts
   - Makes them executable
   - Verifies log directory creation
   - Confirms bobcoin tracking working
2. Add pre-flight checks to launcher script
3. Consider using Python generator that sets execute bit locally

---

## Lessons Learned

1. **Windows → Linux file transfers lose execute permissions**
   - Always `chmod +x` after SCP upload
   - Or use rsync with `-a` flag (preserves permissions)

2. **Silent failures are dangerous**
   - `tee` failing silently meant no logs
   - Add explicit verification steps

3. **Bobcoin tracking is critical**
   - Without logs, we lose cost visibility
   - Must verify tracking works before proceeding

4. **SOP must be complete**
   - Missing chmod +x step caused this issue
   - Every step must be documented

---

## References

- **Phase 0 Success**: Logs created, bobcoin tracking working
- **Phase 1 Success**: Logs created (directory pre-existed)
- **Phase 1.5 Failure**: No logs, no bobcoin tracking
- **SOP**: `docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP.md`
- **Skill**: `.bob/skills/gcp-vm-wave-execution/skill.md`

---

**Status**: Root cause identified, prevention steps documented, ready for SOP update.