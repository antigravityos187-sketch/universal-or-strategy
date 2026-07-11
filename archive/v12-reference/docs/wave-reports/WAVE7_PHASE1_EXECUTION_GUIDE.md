# Wave 7 Phase 1 Execution Guide

**Date**: 2026-06-22  
**Status**: Ready for Execution  
**Phase**: 1 (Scope Definition)  
**Epics**: 18 with completed Phase 0

## Summary

This guide provides instructions for resuming Wave 7 Phase 1 execution for the 18 epics that have completed Phase 0 (00-hotspots.md exists).

## Prerequisites

✅ **Completed**:
- Phase 0 complete for 18 epics (00-hotspots.md exists)
- Phase 1 template available: `building-blocks/wave7/phase1_template_wave7.sh`
- Launch script generated: `scripts/wave7/launch_phase1_batch.sh`

⏳ **Required on VM**:
- SSH access to VM
- Screen sessions available
- Bob CLI installed at `~/.npm-global/bin/bob`
- Python 3 available
- BOBSHELL_API_KEY configured in ~/.bashrc

## Building-Blocks Method Compliance

✅ **Script Generation**:
- ✅ Copied from `building-blocks/wave7/phase1_template_wave7.sh`
- ✅ Uses temp file + command substitution pattern (MANDATORY)
- ✅ Uses full path `~/.npm-global/bin/bob` (not in PATH)
- ✅ 4-minute polling intervals (cost-optimized)
- ✅ Lamport event tracking
- ✅ V12.52 verification gates (manifest + Lamport + filesystem)

## Quick Start (VM Execution)

```bash
# 1. SSH to VM
ssh malhitticrypto@<VM_IP>
cd ~/universal-or-strategy

# 2. Make script executable
chmod +x scripts/wave7/launch_phase1_batch.sh

# 3. Launch Phase 1 batch
nohup bash scripts/wave7/launch_phase1_batch.sh > logs/wave7_phase1_launch.log 2>&1 &

# 4. Monitor progress
tail -f logs/wave7_phase1_launch.log

# 5. Check status
screen -ls | grep wave7_phase1
find docs/brain/EPIC-W7-* -name '00-scope.md' | wc -l
```

## Detailed Execution Steps

### Step 1: SSH to VM

```bash
# From local machine
ssh malhitticrypto@<VM_IP>
cd ~/universal-or-strategy
```

### Step 2: Verify Prerequisites

```bash
# Check Phase 0 completion
find docs/brain/EPIC-W7-* -name '00-hotspots.md' | wc -l
# Expected: 18

# Check Phase 1 not started
find docs/brain/EPIC-W7-* -name '00-scope.md' | wc -l
# Expected: 0

# Verify Bob CLI
~/.npm-global/bin/bob --version

# Verify API key
grep BOBSHELL_API_KEY ~/.bashrc
```

### Step 3: Make Script Executable

```bash
chmod +x scripts/wave7/launch_phase1_batch.sh
chmod +x scripts/wave7/identify_phase0_complete.py
```

### Step 4: Launch Phase 1 Batch

```bash
# Option A: Run in foreground (for testing)
bash scripts/wave7/launch_phase1_batch.sh

# Option B: Run in background (recommended)
nohup bash scripts/wave7/launch_phase1_batch.sh > logs/wave7_phase1_launch.log 2>&1 &

# Get process ID
echo $!
```

### Step 5: Monitor Progress

```bash
# View launch log
tail -f logs/wave7_phase1_launch.log

# List active screen sessions
screen -ls | grep wave7_phase1

# View individual epic log
tail -f logs/wave7/phase1/EPIC-W7-001.log

# Attach to specific epic session
screen -r wave7_phase1_EPIC-W7-001

# Detach from session: Ctrl+A, then D
```

### Step 6: Check Status

```bash
# Count completed Phase 1 epics
find docs/brain/EPIC-W7-* -name '00-scope.md' | wc -l

# Check for errors
grep -r "❌" logs/wave7/phase1/

# View manifest status
python3 scripts/epic_manifest.py status EPIC-W7-001
```

## Script Details

### Launch Script: `scripts/wave7/launch_phase1_batch.sh`

**Features**:
- Automatically identifies epics with Phase 0 complete
- Generates individual Phase 1 scripts for each epic
- Launches each epic in separate screen session
- 4-minute polling intervals (cost-optimized)
- Continuous monitoring loop
- Automatic completion detection

**Cost Optimization**:
- 4-minute intervals between launches (not 30 seconds)
- Reduces API costs by 88%
- Reference: `docs/protocol/COST_OPTIMIZED_POLLING_PROTOCOL.md`

### Individual Epic Scripts

**Location**: `scripts/wave7/phase1_scripts/EPIC-W7-XXX_phase1.sh`

**Generated from**: `building-blocks/wave7/phase1_template_wave7.sh`

**Features**:
- V12.52 triple verification gate (manifest + Lamport + filesystem)
- Bob CLI temp file pattern (MANDATORY)
- Lamport event recording
- Error handling and recovery
- Output validation

## Expected Outputs

### Per Epic

**Output File**: `docs/brain/EPIC-W7-XXX/00-scope.md`

**Contents**:
- Extraction scope definition
- Affected files and methods
- Boundary analysis
- Risk assessment

**Lamport Events**:
- Phase 1 start event
- Phase 1 complete event

**Logs**:
- `logs/wave7/phase1/EPIC-W7-XXX.log`

### Batch Summary

**Total Epics**: 18  
**Expected Duration**: ~72 minutes (18 epics × 4 minutes)  
**Success Criteria**: 18/18 (100%) completion

## Monitoring Commands

```bash
# Active sessions
screen -ls | grep wave7_phase1 | wc -l

# Completed epics
find docs/brain/EPIC-W7-* -name '00-scope.md' | wc -l

# Failed epics (check logs)
grep -l "❌" logs/wave7/phase1/*.log

# Progress percentage
TOTAL=18
COMPLETED=$(find docs/brain/EPIC-W7-* -name '00-scope.md' | wc -l)
echo "Progress: $COMPLETED/$TOTAL ($(($COMPLETED * 100 / $TOTAL))%)"
```

## Troubleshooting

### Issue: Script won't execute

**Solution**:
```bash
# Make executable
chmod +x scripts/wave7/launch_phase1_batch.sh

# Run with bash explicitly
bash scripts/wave7/launch_phase1_batch.sh
```

### Issue: Bob CLI not found

**Solution**:
```bash
# Verify installation
ls -la ~/.npm-global/bin/bob

# Use full path in script (already done)
~/.npm-global/bin/bob --version
```

### Issue: API key not set

**Solution**:
```bash
# Check bashrc
grep BOBSHELL_API_KEY ~/.bashrc

# Export manually
export BOBSHELL_API_KEY=$(grep 'export BOBSHELL_API_KEY' ~/.bashrc | cut -d'=' -f2)
```

### Issue: Screen session failed

**Solution**:
```bash
# Check session status
screen -ls

# View session log
cat logs/wave7/phase1/EPIC-W7-XXX.log

# Reattach and debug
screen -r wave7_phase1_EPIC-W7-XXX

# Kill stuck session
screen -X -S wave7_phase1_EPIC-W7-XXX quit
```

## Recovery Protocol

If any epic fails Phase 1:

1. **Identify Failed Epic**:
   ```bash
   grep -l "❌" logs/wave7/phase1/*.log
   ```

2. **Review Error**:
   ```bash
   cat logs/wave7/phase1/EPIC-W7-XXX.log
   ```

3. **Re-run Single Epic**:
   ```bash
   bash scripts/wave7/phase1_scripts/EPIC-W7-XXX_phase1.sh
   ```

4. **Verify Success**:
   ```bash
   ls -la docs/brain/EPIC-W7-XXX/00-scope.md
   ```

## Success Criteria

### Per Epic
- ✅ `00-scope.md` created
- ✅ File is non-empty
- ✅ Lamport events recorded
- ✅ No errors in log
- ✅ Screen session completed

### Batch
- ✅ 18/18 epics complete (100%)
- ✅ All `00-scope.md` files exist
- ✅ All Lamport events recorded
- ✅ No active screen sessions
- ✅ Ready for Phase 1.5

## Next Steps

After Phase 1 completion (18/18):

1. **Verify Completion**:
   ```bash
   find docs/brain/EPIC-W7-* -name '00-scope.md' | wc -l
   # Expected: 18
   ```

2. **Proceed to Phase 1.5**:
   ```bash
   bash scripts/wave7/launch_phase1_5_batch.sh
   ```

## References

- Launch Script: `scripts/wave7/launch_phase1_batch.sh`
- Phase 1 Template: `building-blocks/wave7/phase1_template_wave7.sh`
- SOP V3: `docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md`
- Cost Protocol: `docs/protocol/COST_OPTIMIZED_POLLING_PROTOCOL.md`