# Wave 1 Phase 0 Recovery Guide

**Date**: 2026-06-14
**Status**: Recovery scripts ready for execution
**Method**: Option B (Bash sed on VM)

## Problem Summary

EPIC-006 through EPIC-015 were executed with EPIC-003 template data due to PowerShell customization script failure. All 10 scripts analyzed the wrong file (V12_002.Orders.Management.StopSync.cs) instead of their assigned files.

**Impact**:
- 10 epics executed with wrong data
- ~10-12 bobcoins wasted on duplicate EPIC-003 analysis
- No valid output files created for EPIC-006-015

## Recovery Solution

**Method**: Bash sed script on VM (Option B)
- **Why**: Most reliable, avoids API key length issues
- **How**: Uses sed to find-and-replace 4 key sections in template
- **Time**: ~5 minutes total (script generation + execution)

## Recovery Scripts Created

### 1. Script Generator: `scripts/wave1/fix_epic_006_015.sh`

**Purpose**: Generate 10 corrected scripts from template using sed

**What it does**:
- Reads `_p0_003.sh` as template
- For each epic (006-015):
  - Replaces epic ID (EPIC-003 → EPIC-XXX)
  - Replaces method names (SyncLimitTarget → correct method)
  - Replaces file path (StopSync.cs → correct file)
  - Replaces complexity values (17, 9 → correct values)
- Creates `_p0_XXX_corrected.sh` for each epic
- Makes all scripts executable

**Usage**:
```bash
cd /home/malhitticrypto/universal-or-strategy
chmod +x fix_epic_006_015.sh
./fix_epic_006_015.sh
```

**Output**: 10 corrected scripts ready for execution

### 2. Launcher: `scripts/wave1/launch_phase0_006_015.sh`

**Purpose**: Launch all 10 corrected epics in screen sessions

**What it does**:
- Launches each epic in a detached screen session
- Logs output to `logs/phase0/EPIC-XXX.log`
- Provides monitoring commands

**Usage**:
```bash
cd /home/malhitticrypto/universal-or-strategy
chmod +x launch_phase0_006_015.sh
./launch_phase0_006_015.sh
```

## Execution Steps

### Step 1: Upload Scripts to VM

```bash
# From local machine
gcloud compute scp scripts/wave1/fix_epic_006_015.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a
gcloud compute scp scripts/wave1/launch_phase0_006_015.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a
gcloud compute scp scripts/wave1/_p0_003.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a
```

### Step 2: Generate Corrected Scripts on VM

```bash
# SSH to VM
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a

# Navigate to project
cd /home/malhitticrypto/universal-or-strategy

# Make generator executable
chmod +x fix_epic_006_015.sh

# Run generator
./fix_epic_006_015.sh

# Verify scripts created
ls -lh _p0_00{6..9}_corrected.sh _p0_01{0..5}_corrected.sh
```

**Expected output**:
```
✅ Created 10 corrected scripts

Files created:
-rwxr-xr-x 1 user user 4.2K Jun 14 00:00 _p0_006_corrected.sh
-rwxr-xr-x 1 user user 4.2K Jun 14 00:00 _p0_007_corrected.sh
...
-rwxr-xr-x 1 user user 4.2K Jun 14 00:00 _p0_015_corrected.sh
```

### Step 3: Launch All 10 Epics

```bash
# Make launcher executable
chmod +x launch_phase0_006_015.sh

# Launch all epics
./launch_phase0_006_015.sh
```

**Expected output**:
```
Launching Phase 0 for EPIC-006 through EPIC-015...
Each epic will run in a detached screen session

Starting EPIC-006 in screen session p0-006...
Starting EPIC-007 in screen session p0-007...
...
Starting EPIC-015 in screen session p0-015...

✅ All 10 epics launched in screen sessions
```

### Step 4: Monitor Execution

**Check running sessions**:
```bash
screen -ls
```

**Expected**: 10 sessions named `p0-006` through `p0-015`

**Watch specific log**:
```bash
tail -f logs/phase0/EPIC-006.log
```

**Check completion** (wait ~2 minutes):
```bash
# Count running sessions (0 = all done)
screen -ls | grep -c 'p0-' || echo "All complete"

# Count output files (expect 20: 10 hotspots + 10 manifests)
ls docs/brain/EPIC-*/00-hotspots.md 2>/dev/null | wc -l
ls docs/brain/EPIC-*/manifest.json 2>/dev/null | wc -l
```

### Step 5: Verify Output Files

```bash
# List all hotspot files
ls -lh docs/brain/EPIC-{006..015}/00-hotspots.md

# List all manifest files
ls -lh docs/brain/EPIC-{006..015}/manifest.json

# Check file sizes (hotspots should be >1KB)
du -h docs/brain/EPIC-{006..015}/00-hotspots.md
```

**Expected**: 20 files total (10 hotspots + 10 manifests)

### Step 6: Extract Bobcoin Usage

```bash
# Extract bobcoin reports from logs
grep -A 2 'BOBCOIN REPORT' logs/phase0/EPIC-{006..015}.log

# Or extract Cost + Balance lines
grep -E 'Cost:.*Balance:|Cost: [0-9]' logs/phase0/EPIC-{006..015}.log
```

**Expected**: ~1-3 bobcoins per epic, ~10-30 bobcoins total

## Success Criteria

- ✅ All 10 corrected scripts created
- ✅ All 10 screen sessions launched
- ✅ All 10 screen sessions completed (DONE_EXIT=0)
- ✅ 20 files created (10 hotspots + 10 manifests)
- ✅ All hotspot files >1KB
- ✅ Bobcoin usage reported in logs
- ✅ Total bobcoins used: 10-30 (acceptable range)

## Troubleshooting

### Issue: Script generation fails

**Symptom**: `fix_epic_006_015.sh` exits with error

**Solution**:
```bash
# Check template exists
ls -lh _p0_003.sh

# If missing, upload from local
gcloud compute scp scripts/wave1/_p0_003.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a
```

### Issue: Screen sessions fail to start

**Symptom**: `screen -ls` shows no sessions

**Solution**:
```bash
# Check logs for errors
tail -20 logs/phase0/EPIC-006.log

# Try launching one epic manually
./_p0_006_corrected.sh
```

### Issue: Files not created

**Symptom**: `ls docs/brain/EPIC-006/` shows no files

**Solution**:
```bash
# Check if Bob Shell ran
grep "bob --yolo" logs/phase0/EPIC-006.log

# Check for errors
grep -i "error\|failed" logs/phase0/EPIC-006.log

# Relaunch specific epic
screen -dmS "p0-006" bash -l -c "cd /home/malhitticrypto/universal-or-strategy && ./_p0_006_corrected.sh 2>&1 | tee logs/phase0/EPIC-006.log"
```

### Issue: Bobcoins exhausted

**Symptom**: API returns 401 or "insufficient credits"

**Solution**:
- STOP immediately
- Check API balance in IBM Bob Shell dashboard
- Contact IBM for credit reset if needed
- Do NOT continue execution

## Post-Recovery Actions

### 1. Validate All 15 Epics Complete

```bash
# Check all epic directories exist
ls -d docs/brain/EPIC-{001..015}

# Count total files (expect 30: 15 hotspots + 15 manifests)
ls docs/brain/EPIC-*/00-hotspots.md 2>/dev/null | wc -l
ls docs/brain/EPIC-*/manifest.json 2>/dev/null | wc -l
```

### 2. Extract Total Bobcoin Usage

```bash
# Extract all bobcoin reports
grep -E 'Cost:.*Balance:|Cost: [0-9]' logs/phase0/EPIC-*.log > bobcoin_usage_wave1_phase0.txt

# Calculate total (manual sum)
cat bobcoin_usage_wave1_phase0.txt
```

### 3. Update Documentation

- Update `WAVE1_PHASE0_COMPLETION_REPORT.md` with final status
- Document lessons learned in failure analysis
- Update todo list to mark Phase 0 complete

### 4. Proceed to Phase 1

Once all 15 epics validated:
- Generate Phase 1 scripts (Scope Definition)
- Use same pattern: copy working template, customize with sed
- Launch Phase 1 execution

## Lessons Learned

### What Went Wrong

1. **PowerShell Customization Failed**: Script had syntax errors, never executed
2. **No Validation**: Templates uploaded without verification
3. **Silent Failure**: Scripts executed successfully but with wrong data
4. **No Post-Execution Check**: Didn't verify output files matched epic IDs

### What Worked

1. **Building Blocks Method**: Proven reliable for EPIC-001, 002, 004 corrections
2. **Screen Sessions**: Reliable execution environment
3. **File Verification Protocol**: Caught the issue before proceeding

### Improvements for Future Phases

1. **Always Validate Customization**: Check scripts before upload
2. **Post-Execution Verification**: Verify epic ID in output files
3. **Use Bash sed on VM**: Avoid local PowerShell issues
4. **Test One Epic First**: Validate pattern before launching all

## Recovery Timeline

- **00:00**: Discovery of failure (EPIC-006-015 wrong data)
- **00:15**: Analysis and decision (Option B: Bash sed)
- **00:30**: Script creation (fix_epic_006_015.sh + launcher)
- **00:35**: Upload to VM
- **00:40**: Generate corrected scripts
- **00:45**: Launch all 10 epics
- **00:47**: Monitor execution (~2 minutes)
- **00:50**: Verify output files
- **00:55**: Extract bobcoin usage
- **01:00**: Recovery complete

**Total Recovery Time**: ~1 hour

## Budget Impact

**Wasted**: ~10-12 bobcoins (duplicate EPIC-003 analysis)
**Recovery Cost**: ~10-30 bobcoins (EPIC-006-015 correct analysis)
**Total Impact**: ~20-42 bobcoins

**Remaining Budget**: 1,600 - 6 (EPIC-001-005) - 12 (wasted) - 30 (recovery) = ~1,552 bobcoins

**Safety Margin**: 97% (well above 10% threshold)

## Next Steps

1. Execute recovery (Steps 1-6 above)
2. Validate all 15 epics complete
3. Update documentation
4. Proceed to Wave 1 Phase 1 (Scope Definition)

---

**Document Version**: 1.0
**Last Updated**: 2026-06-14T07:17:00Z
**Status**: Ready for execution
**Estimated Time**: 1 hour
**Risk Level**: LOW (proven method, validated scripts)