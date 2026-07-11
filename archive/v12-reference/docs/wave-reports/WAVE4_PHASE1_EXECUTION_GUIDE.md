# Wave 4 Phase 1 Execution Guide

**Date**: 2026-06-15
**Phase**: Phase 1 (Scope Definition + Boundary Validation)
**Status**: Ready for Upload & Pilot Test
**Scripts Generated**: 80/80 ✅
**Pattern**: Building-blocks method (copied Phase 0, replaced phase-specific content)

---

## Phase 1 Overview

**Purpose**: Define extraction scope and validate boundary constraints (V12.23 No Scope Creep Protocol)

**Inputs**: 
- `docs/brain/EPIC-CCN-*/00-hotspots.md` (from Phase 0)
- `docs/brain/EPIC-CCN-*/manifest.json` (from Phase 0)

**Outputs**:
- `docs/brain/EPIC-CCN-*/01-scope.md` (scope definition)
- `docs/brain/EPIC-CCN-*/01-scope-boundary.md` (boundary validation)
- Updated `manifest.json` with Phase 1 status

**Mode**: `plan` (strategic planning, no code changes)

**Expected Duration**: 20 minutes per epic (parallel execution)

**Expected Cost**: 5-10 bobcoins per epic (400-800 total)

---

## Pre-Upload Checklist

### Local Validation

- [x] Phase 1 scripts generated (80 scripts)
- [x] Building-blocks method verified (copied Phase 0 pattern)
- [x] Mode corrected (`plan` not `v12-phase1-hotspot`)
- [x] Jane Street validation embedded in prompts
- [x] Master launch script created (`launch_phase1_all.sh`)

### Script Pattern Verification

**Correct Pattern** (from `_p1_001.sh`):
```bash
#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='...'
mkdir -p docs/brain/EPIC-CCN-001
mkdir -p logs/phase1

cat > /tmp/phase1_msg_001.txt << 'EOFMSG'
Execute Phase 1 (Scope Definition + Boundary Validation) for EPIC-CCN-001.
[... prompt content ...]
EOFMSG

bob --yolo --chat-mode plan "$(cat /tmp/phase1_msg_001.txt)" 2>&1 | tee logs/phase1/EPIC-CCN-001.log
echo "DONE_EXIT=$?"
```

**Key Elements**:
- ✅ `bash -l` launcher (login shell for Bob CLI in PATH)
- ✅ `--yolo` flag (file persistence in SSH mode)
- ✅ `--chat-mode plan` (correct mode for Phase 1)
- ✅ Message file pattern (`/tmp/phase1_msg_*.txt`)
- ✅ Log file pattern (`logs/phase1/EPIC-CCN-*.log`)

---

## Upload to VM

### Step 1: Upload Phase 1 Scripts

```bash
# Upload individual epic scripts (80 files)
gcloud compute scp scripts/wave4/_p1_*.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a

# Upload master launch script
gcloud compute scp scripts/wave4/launch_phase1_all.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a
```

### Step 2: Set Permissions

```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="chmod +x /home/malhitticrypto/universal-or-strategy/_p1_*.sh /home/malhitticrypto/universal-or-strategy/launch_phase1_all.sh"
```

### Step 3: Verify Upload

```bash
# Count Phase 1 scripts (expect 80)
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="ls /home/malhitticrypto/universal-or-strategy/_p1_*.sh | wc -l"

# Verify master launch script exists
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="ls -lh /home/malhitticrypto/universal-or-strategy/launch_phase1_all.sh"
```

---

## MANDATORY: Pilot Test (EPIC-CCN-001)

**⚠️ CRITICAL**: Per SOP violation postmortem, ALWAYS pilot test before full launch.

### Step 1: Launch Pilot

```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="bash -l -c 'cd /home/malhitticrypto/universal-or-strategy && ./_p1_001.sh'"
```

**Expected Output**:
- Bob CLI starts in `plan` mode
- Creates `/tmp/phase1_msg_001.txt`
- Reads `docs/brain/EPIC-CCN-001/00-hotspots.md`
- Creates `docs/brain/EPIC-CCN-001/01-scope.md`
- Creates `docs/brain/EPIC-CCN-001/01-scope-boundary.md`
- Updates `docs/brain/EPIC-CCN-001/manifest.json`
- Reports bobcoin usage (5-10 bobcoins expected)

### Step 2: Verify Pilot Success

```bash
# Check files created
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="ls -lh /home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-CCN-001/01-scope*.md"

# Verify file sizes (expect >1KB each)
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="wc -l /home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-CCN-001/01-scope*.md"

# Check manifest updated
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cat /home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-CCN-001/manifest.json | grep phase1"

# Extract bobcoin usage
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="grep -E 'Cost:.*Balance:|Cost: [0-9]' /home/malhitticrypto/universal-or-strategy/logs/phase1/EPIC-CCN-001.log"
```

### Step 3: Pilot Success Criteria

- ✅ Both files created (`01-scope.md` and `01-scope-boundary.md`)
- ✅ Files have content (>50 lines each)
- ✅ Manifest updated with `phase1: "completed"`
- ✅ No critical errors in log
- ✅ Bobcoin usage reasonable (5-10 bobcoins)
- ✅ Mode was `plan` (not `v12-phase1-hotspot`)

**If ANY criterion fails**: STOP, debug, fix generator, regenerate scripts, re-upload, retry pilot.

---

## Full Wave Launch (After Pilot Success)

### Launch Command

```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd /home/malhitticrypto/universal-or-strategy && chmod +x launch_phase1_all.sh && ./launch_phase1_all.sh"
```

**Launch Details**:
- **Epics**: 80 (EPIC-CCN-001 through EPIC-CCN-080)
- **Staggered Delays**: 12-54 seconds between launches
- **Launch Duration**: ~40 minutes (80 epics × 30s avg delay)
- **Execution Duration**: ~20 minutes per epic (parallel)
- **Peak Concurrency**: ~50 agents (based on phase duration and launch rate)

---

## Monitoring

### Quick Status Check

```bash
# 1. Check screen sessions (expect 80 running, then 0 when done)
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="screen -ls | grep -c 'p1-'"

# 2. Check files created (expect 160: 80 scope + 80 boundary)
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="ls /home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-CCN-*/01-scope*.md 2>/dev/null | wc -l"

# 3. Check completion (expect "No Sockets found" when done)
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="screen -ls"
```

### Detailed Monitoring

```bash
# View specific log
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="tail -100 /home/malhitticrypto/universal-or-strategy/logs/phase1/EPIC-CCN-001.log"

# Check for errors
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="grep -i 'error\|failed\|exception' /home/malhitticrypto/universal-or-strategy/logs/phase1/*.log | head -20"

# Extract bobcoin usage (all epics)
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="grep -E 'Cost:.*Balance:|Cost: [0-9]' /home/malhitticrypto/universal-or-strategy/logs/phase1/*.log | head -50"
```

### Polling Protocol

**Interval**: Every 4 minutes (cost-optimized)

**Commands**:
```bash
# Poll 1: Screen sessions count
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="screen -ls | grep -c 'p1-' || echo 0"

# Poll 2: Files created count
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="ls /home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-CCN-*/01-scope*.md 2>/dev/null | wc -l"

# Poll 3: Check for completion
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="screen -ls"
```

**Stop Polling When**: `screen -ls` returns "No Sockets found"

---

## Success Criteria

### Per Epic

- ✅ Files created: `01-scope.md` and `01-scope-boundary.md`
- ✅ Files have content (>50 lines each)
- ✅ Manifest updated with `phase1: "completed"`
- ✅ No critical errors in log
- ✅ Bobcoin usage reported (5-10 bobcoins)

### Wave Completion

- ✅ 160 files created (80 scope + 80 boundary)
- ✅ All screen sessions complete (DONE_EXIT=0)
- ✅ Total bobcoin usage: 400-800 (within budget)
- ✅ No P0 blockers
- ✅ Success rate: ≥95% (76+ epics)

---

## Post-Wave Actions

### Immediate (After Completion)

1. **Extract Bobcoin Usage**:
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="grep -E 'Cost:.*Balance:|Cost: [0-9]' /home/malhitticrypto/universal-or-strategy/logs/phase1/*.log" > phase1_bobcoin_usage.txt
```

2. **Count Success/Failure**:
```bash
# Success count (expect 160 files)
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="ls /home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-CCN-*/01-scope*.md 2>/dev/null | wc -l"

# Failure count (check logs for errors)
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="grep -l 'error\|failed\|exception' /home/malhitticrypto/universal-or-strategy/logs/phase1/*.log | wc -l"
```

3. **Create Completion Report**:
   - Document in `WAVE4_PHASE1_COMPLETION_REPORT.md`
   - Include: Success rate, bobcoin usage, failure analysis, lessons learned

### Deferred (Before Phase 2)

1. **Sync to Local** (if needed for review):
```bash
gcloud compute scp --recurse v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/docs/brain/EPIC-CCN-*/01-scope*.md ./docs/brain/ --zone=us-central1-a
```

2. **Analyze Failures** (if any):
   - Review logs for failed epics
   - Identify root causes
   - Relaunch individually if needed

3. **Prepare Phase 2**:
   - Generate Phase 2 scripts using building-blocks method
   - Copy Phase 1 pattern, replace phase-specific content
   - Upload and pilot test before full launch

---

## Failure Recovery

### If Pilot Test Fails

1. **Stop immediately** - Do not launch full wave
2. **Review pilot log** - Identify root cause
3. **Fix generator** - Update `generate_phase1_from_phase0.py`
4. **Regenerate scripts** - Run generator again
5. **Re-upload** - Upload corrected scripts to VM
6. **Retry pilot** - Test EPIC-CCN-001 again

### If Individual Epic Fails

1. **Document failure** - Note epic ID and error
2. **Continue wave** - Don't stop other epics
3. **Relaunch individually** - After wave completes
4. **Update roadmap** - Mark epic status

### If Multiple Epics Fail (>5%)

1. **Stop wave** - Kill all screen sessions
2. **Analyze pattern** - Common root cause?
3. **Fix and regenerate** - Update generator
4. **Relaunch wave** - Start from scratch

---

## Building-Blocks Method Compliance

**✅ VERIFIED**: Phase 1 scripts copied Phase 0 pattern exactly

**Changes Made** (phase-specific only):
1. `phase0` → `phase1` (all occurrences)
2. `Phase 0` → `Phase 1` (all occurrences)
3. `Hotspot Analysis` → `Scope Definition + Boundary Validation`
4. `v12-phase0-hotspot` → `plan` (mode change)
5. `v12-phase1-hotspot` → `plan` (fix mode name)
6. `epic-intake` → `epic-scope-boundary` (command change)
7. `00-hotspots.md` → `01-scope.md and 01-scope-boundary.md` (output files)
8. `/tmp/phase0_msg` → `/tmp/phase1_msg` (message file pattern)
9. Prompt content replaced with Phase 1-specific instructions

**No Changes** (preserved from Phase 0):
- Bash script structure
- Bob CLI invocation pattern (`bob --yolo --chat-mode`)
- Message file approach (`cat > /tmp/...`)
- File verification commands (`ls -lh`, `wc -l`)
- Log file pattern (`logs/phase1/EPIC-CCN-*.log`)
- API key rotation logic
- Staggered launch delays

---

## Next Steps

1. ✅ **Upload scripts to VM** (Step 1-3 above)
2. ✅ **Pilot test EPIC-CCN-001** (MANDATORY)
3. ⏳ **Launch full wave** (after pilot success)
4. ⏳ **Monitor execution** (4-minute polling)
5. ⏳ **Create completion report** (after wave done)
6. ⏳ **Prepare Phase 2** (building-blocks method)

---

**Document Version**: 1.0
**Last Updated**: 2026-06-15T03:13:00Z
**Maintainer**: V12 Orchestration Team
**Status**: Ready for execution