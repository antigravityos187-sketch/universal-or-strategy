# Wave 4 Phase 6 Execution - Continuation Prompt

**Copy and paste this entire prompt into your new Claude session:**

---

# Context: Wave 4 Phase 6 (Verification) - Ready for Execution

You are the Wave 4 Execution Lead continuing autonomous complexity reduction for the Universal OR Strategy V12 Photon Kernel. Phases 0-5 are complete with 79/80 epics successful (98.75%). Phase 6 scripts need to be generated and executed for verification.

## Critical Status Summary

### Completed Phases ✅
- **Phase 0 (Hotspot)**: 79/80 complete (98.75%)
- **Phase 1 (Scope)**: 80/80 complete (100%)
- **Phase 2 (Architecture)**: 84/80 complete (105%)
- **Phase 3 (Audit)**: 80/80 complete (100%)
- **Phase 4 (Tickets)**: 80/80 complete (100%)
- **Phase 5 (Execution)**: 79/80 complete (98.75%)
- **Total**: 482/560 files (86% of all phases complete)

### Phase 5 Results
- ✅ **Successful**: 79 epics
- ❌ **Deferred**: 1 epic (EPIC-CCN-016 - scope mismatch, requires re-scoping)
- **Bobcoins Used**: 391.12 (51% under budget)
- **Average Cost**: 5.01 bobcoins/epic

### Budget Status
- **Phases 0-4 Used**: 391 bobcoins
- **Phase 5 Used**: 391.12 bobcoins
- **Total Used**: 782.12 bobcoins (32.6% of 2,400 total)
- **Remaining**: 1,617.88 bobcoins (67.4%)
- **Phase 6 Budget**: 400-800 bobcoins (estimated 5-10/epic)

## Your Mission

Execute Phase 6 (Verification) for 79 successful epics from Phase 5. Generate scripts using building-blocks method, run pilot test, then launch full wave.

## Phase 6 Overview

### What Phase 6 Does
- **Purpose**: Verify ticket execution succeeded and complexity targets met
- **MCP Tool**: `execute_phase_6` from `phase-6-review` server
- **Input**: `ticket-*-completion.md` files (from Phase 5)
- **Output**: `06-verification-report.md` (one per epic)
- **Duration**: ~10-15 minutes per epic
- **Budget**: 5-10 bobcoins/epic (~400-800 total)

### Verification Checks
1. ✅ All tickets executed successfully
2. ✅ Complexity targets met (CYC ≤ 15 or as specified)
3. ✅ Build passes
4. ✅ No behavioral changes
5. ✅ All acceptance criteria satisfied
6. ✅ Files persisted correctly

## Execution Plan (7 Steps)

### Step 1: Generate Phase 6 Scripts (Building-Blocks Method)

**MANDATORY**: Copy from Phase 5 scripts, modify only phase-specific parameters.

**Reference Phase 5 Script** (`scripts/wave4/_p5_001.sh`):
```bash
#!/bin/bash
# Phase 5 (Ticket Execution) for EPIC-CCN-001
set -e
cd /home/malhitticrypto/universal-or-strategy

EPIC_ID="EPIC-CCN-001"
API_KEY="bob_prod_bob-admin_..."

export BOBSHELL_API_KEY="$API_KEY"

# Prerequisite check
if [ ! -f "docs/brain/EPIC-CCN-001/04-tickets.md" ]; then
    echo "ERROR: Missing prerequisite file"
    exit 1
fi

# Create message file
cat > /tmp/phase5_msg_001.txt << 'EOFMSG'
Use the phase-5-execute MCP server to execute Phase 5 for EPIC-CCN-001.
Call the execute_phase_5 tool with epic_id="EPIC-CCN-001".
EOFMSG

# Execute with Bob Shell
bob --yolo "$(cat /tmp/phase5_msg_001.txt)"

# Verify files created
if ls docs/brain/EPIC-CCN-001/ticket-*-completion.md 1> /dev/null 2>&1; then
    echo "SUCCESS: Phase 5 complete"
else
    echo "ERROR: No completion files"
    exit 1
fi
```

**Phase 6 Script Template** (`scripts/wave4/_p6_001.sh`):
```bash
#!/bin/bash
# Phase 6 (Verification) for EPIC-CCN-001
set -e
cd /home/malhitticrypto/universal-or-strategy

EPIC_ID="EPIC-CCN-001"
API_KEY="bob_prod_bob-admin_..."  # Use same API rotation as Phase 5

export BOBSHELL_API_KEY="$API_KEY"

# Prerequisite check: Verify Phase 5 completion files exist
if ! ls docs/brain/EPIC-CCN-001/ticket-*-completion.md 1> /dev/null 2>&1; then
    echo "ERROR: Missing Phase 5 completion files"
    exit 1
fi

# Create message file
cat > /tmp/phase6_msg_001.txt << 'EOFMSG'
Use the phase-6-review MCP server to execute Phase 6 for EPIC-CCN-001.

Call the execute_phase_6 tool with epic_id="EPIC-CCN-001".

The tool will verify:
1. All tickets executed successfully
2. Complexity targets met
3. Build passes
4. No behavioral changes
5. All acceptance criteria satisfied

**Verification**: Confirm verification report exists on disk before reporting success.

**Bobcoin Tracking**: Report usage in format "Cost: X.XX | Balance: Y.YY"
EOFMSG

# Execute with Bob Shell
bob --yolo "$(cat /tmp/phase6_msg_001.txt)"

# Verify verification report created
if [ -f "docs/brain/EPIC-CCN-001/06-verification-report.md" ]; then
    echo "SUCCESS: Phase 6 complete for EPIC-CCN-001"
    echo "File: docs/brain/EPIC-CCN-001/06-verification-report.md"
    ls -lh docs/brain/EPIC-CCN-001/06-verification-report.md
else
    echo "ERROR: No verification report created for EPIC-CCN-001"
    exit 1
fi
```

**Key Changes from Phase 5**:
1. Phase number: `5` → `6`
2. MCP server: `phase-5-execute` → `phase-6-review`
3. MCP tool: `execute_phase_5` → `execute_phase_6`
4. Prerequisite: `04-tickets.md` → `ticket-*-completion.md`
5. Output file: `ticket-*-completion.md` → `06-verification-report.md`
6. Message file: `/tmp/phase5_msg_*.txt` → `/tmp/phase6_msg_*.txt`

**Generation Script**:
```python
#!/usr/bin/env python3
"""Generate Phase 6 scripts using building-blocks method."""

import json
from pathlib import Path

# Load API keys
api_keys = []
for api_file in Path('docs/API').glob('*.json'):
    with open(api_file) as f:
        data = json.load(f)
        api_keys.append(data['api_key'])

# Generate scripts for 79 successful epics (skip EPIC-CCN-016)
for i in range(1, 81):
    if i == 16:  # Skip EPIC-CCN-016 (deferred)
        continue
    
    epic_num = f"{i:03d}"
    epic_id = f"EPIC-CCN-{epic_num}"
    api_key = api_keys[i % len(api_keys)]  # Round-robin
    
    script = f"""#!/bin/bash
# Phase 6 (Verification) for {epic_id}
set -e
cd /home/malhitticrypto/universal-or-strategy

EPIC_ID="{epic_id}"
API_KEY="{api_key}"

export BOBSHELL_API_KEY="$API_KEY"

# Prerequisite check
if ! ls docs/brain/{epic_id}/ticket-*-completion.md 1> /dev/null 2>&1; then
    echo "ERROR: Missing Phase 5 completion files for {epic_id}"
    exit 1
fi

# Create message file
cat > /tmp/phase6_msg_{epic_num}.txt << 'EOFMSG'
Use the phase-6-review MCP server to execute Phase 6 for {epic_id}.

Call the execute_phase_6 tool with epic_id="{epic_id}".

The tool will verify:
1. All tickets executed successfully
2. Complexity targets met
3. Build passes
4. No behavioral changes
5. All acceptance criteria satisfied

**Verification**: Confirm verification report exists on disk before reporting success.

**Bobcoin Tracking**: Report usage in format "Cost: X.XX | Balance: Y.YY"
EOFMSG

# Execute with Bob Shell
bob --yolo "$(cat /tmp/phase6_msg_{epic_num}.txt)"

# Verify verification report created
if [ -f "docs/brain/{epic_id}/06-verification-report.md" ]; then
    echo "SUCCESS: Phase 6 complete for {epic_id}"
    echo "File: docs/brain/{epic_id}/06-verification-report.md"
    ls -lh docs/brain/{epic_id}/06-verification-report.md
else
    echo "ERROR: No verification report created for {epic_id}"
    exit 1
fi
"""
    
    output_path = Path(f'scripts/wave4/_p6_{epic_num}.sh')
    output_path.write_text(script)
    print(f"Generated: {output_path}")

print(f"\nTotal scripts generated: 79 (skipped EPIC-CCN-016)")
```

### Step 2: Upload Scripts to VM

```bash
# Upload all Phase 6 scripts
gcloud compute scp scripts/wave4/_p6_*.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/scripts/wave4/ --zone=us-central1-a

# Set permissions
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="chmod +x /home/malhitticrypto/universal-or-strategy/scripts/wave4/_p6_*.sh"

# Verify upload
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="ls -la /home/malhitticrypto/universal-or-strategy/scripts/wave4/_p6_*.sh | wc -l"
# Expected: 79
```

### Step 3: MANDATORY Pilot Test

**Launch Command**:
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd /home/malhitticrypto/universal-or-strategy && screen -dmS p6-001 bash -l -c './scripts/wave4/_p6_001.sh 2>&1 | tee logs/phase6/EPIC-CCN-001.log'"
```

**Wait 1 minute, then check**:
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd universal-or-strategy && screen -ls && ls docs/brain/EPIC-CCN-001/06-verification-report.md"
```

**Success Criteria**:
1. ✅ Screen session complete
2. ✅ File exists: `docs/brain/EPIC-CCN-001/06-verification-report.md`
3. ✅ File size >1K
4. ✅ No errors in log
5. ✅ Bobcoin usage reported

**If Pilot Fails**: Fix issue, re-test. DO NOT proceed to full wave.

### Step 4: Generate Launcher Scripts

**Test Launcher** (`scripts/wave4/launch_phase6_test.sh`):
```bash
#!/bin/bash
# Phase 6 Pilot Test - First 2 Epics
set -e
cd /home/malhitticrypto/universal-or-strategy

echo "[$(date)] Starting Phase 6 pilot test (2 epics)"
mkdir -p logs/phase6

# Launch EPIC-CCN-001
echo "[$(date)] Launching EPIC-CCN-001"
screen -dmS p6-001 bash -l -c \
    "./scripts/wave4/_p6_001.sh 2>&1 | tee logs/phase6/EPIC-CCN-001.log"
sleep 12

# Launch EPIC-CCN-002
echo "[$(date)] Launching EPIC-CCN-002"
screen -dmS p6-002 bash -l -c \
    "./scripts/wave4/_p6_002.sh 2>&1 | tee logs/phase6/EPIC-CCN-002.log"

echo "[$(date)] Pilot test launched (2 epics)"
echo "Monitor with: screen -ls"
echo "Check files: ls docs/brain/EPIC-CCN-{001,002}/06-verification-report.md"
```

**Full Launcher** (`scripts/wave4/launch_phase6_all.sh`):
```bash
#!/bin/bash
# Phase 6 Full Wave - 79 Epics (skip EPIC-CCN-016)
set -e
cd /home/malhitticrypto/universal-or-strategy

echo "[$(date)] Starting Phase 6 full wave (79 epics)"
mkdir -p logs/phase6

# Launch all epics except EPIC-CCN-016
for i in $(seq -f "%03g" 1 80); do
    if [ "$i" == "016" ]; then
        echo "[$(date)] Skipping EPIC-CCN-016 (deferred)"
        continue
    fi
    
    EPIC="EPIC-CCN-${i}"
    echo "[$(date)] Launching ${EPIC}"
    
    screen -dmS p6-${i} bash -l -c \
        "./scripts/wave4/_p6_${i}.sh 2>&1 | tee logs/phase6/${EPIC}.log"
    
    sleep 12
done

echo "[$(date)] All 79 epics launched for Phase 6"
echo "Monitor with: screen -ls | grep -c 'p6-'"
echo "Check files: ls docs/brain/EPIC-CCN-*/06-verification-report.md | wc -l"
```

### Step 5: Monitor Execution (V2.0 Protocol)

**Initial Check** (1 min after first script):
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd universal-or-strategy && echo '=== CHECK 1 ===' && screen -ls | grep -c 'p6-' && ls docs/brain/EPIC-CCN-*/06-verification-report.md 2>/dev/null | wc -l"
```

**Subsequent Checks** (every 4 minutes):
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd universal-or-strategy && echo '=== CHECK N ===' && date && screen -ls | grep -c 'p6-' && ls docs/brain/EPIC-CCN-*/06-verification-report.md 2>/dev/null | wc -l"
```

**Stop When**:
- All screen sessions complete (0 sessions)
- File count reaches 79
- Count stable for 2 consecutive checks

### Step 6: Recovery Loop (If Needed)

**Check Success Rate**:
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a \
  --command="cd universal-or-strategy && ls docs/brain/EPIC-CCN-*/06-verification-report.md 2>/dev/null | wc -l"
```

**Expected**: 79 (all except EPIC-CCN-016)

**IF <79**: Apply Recovery Loop Protocol V12.26 (same as Phase 5)

### Step 7: Completion Actions

1. **Sync Files**:
```bash
gcloud compute scp --recurse v12-test-golden-v2:~/universal-or-strategy/docs/brain/EPIC-CCN-*/06-verification-report.md ./docs/brain/ --zone=us-central1-a
```

2. **Extract Bobcoins**: Use `scripts/extract_phase6_bobcoins.py` (copy from Phase 5 script)

3. **Create Completion Report**: Document results, lessons learned, next steps

4. **Update Roadmap**: Mark Phase 6 complete for all successful epics

## Key Documents

**MUST READ**:
- [`WAVE4_PHASE5_COMPLETION_REPORT.md`](WAVE4_PHASE5_COMPLETION_REPORT.md) - Phase 5 results
- [`WAVE4_PHASE5_RECOVERY_REPORT.md`](WAVE4_PHASE5_RECOVERY_REPORT.md) - Recovery lessons
- [`docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md`](docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md) - Building-blocks method
- [`docs/protocol/RECOVERY_LOOP_PROTOCOL.md`](docs/protocol/RECOVERY_LOOP_PROTOCOL.md) - V12.26 protocol

## Success Criteria

### Per Epic
- ✅ File exists: `docs/brain/EPIC-CCN-{ID}/06-verification-report.md`
- ✅ File size >1K
- ✅ All verification checks passed
- ✅ Bobcoin usage <10 per epic

### Wave Completion
- ✅ 79/79 epics complete (100% of Phase 5 successful epics)
- ✅ Total bobcoin usage <800
- ✅ All APIs remain positive
- ✅ No wave-wide failures

## Timeline Estimate

- **Script Generation**: 10 minutes
- **Upload & Permissions**: 5 minutes
- **Pilot Test**: 15 minutes
- **Full Wave Launch**: 16 minutes (79 × 12s)
- **Execution**: ~10-15 min/epic (parallel)
- **Monitoring**: ~40 minutes (10 checks × 4 min)
- **Completion**: 30 minutes
- **TOTAL**: ~2-3 hours

## Your First Actions

1. Generate Phase 6 scripts using building-blocks method
2. Upload scripts to VM and set permissions
3. Run pilot test (EPIC-CCN-001)
4. Validate pilot success
5. Generate launcher scripts
6. Launch full wave
7. Monitor execution (V2.0 protocol)
8. Apply Recovery Loop if needed
9. Create completion report

---

**Ready to execute Phase 6!** Start with script generation using building-blocks method. Good luck! 🚀

---

**Session Context Version**: 3.0 (Phase 6)
**Last Updated**: 2026-06-15T20:01:00Z
**Maintainer**: Wave 4 Execution Lead
**Status**: 🟢 READY FOR PHASE 6 SCRIPT GENERATION